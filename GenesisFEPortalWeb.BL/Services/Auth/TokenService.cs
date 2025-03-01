using GenesisFEPortalWeb.BL.Repositories.Auth;
using GenesisFEPortalWeb.Models.Entities.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GenesisFEPortalWeb.BL.Services.Auth
{
    /// <summary>
    /// Define las operaciones para la gestión de tokens JWT en un contexto multi-tenant.
    /// Esta interfaz maneja la generación, validación y renovación de tokens de acceso.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Genera un nuevo par de tokens (access token y refresh token) para un usuario.
        /// El token generado incluirá información específica del tenant al que pertenece el usuario.
        /// </summary>
        /// <param name="user">El usuario para el cual se generarán los tokens</param>
        /// <returns>Una tupla conteniendo el token de acceso y el token de refresco</returns>
        Task<(string Token, string RefreshToken)> GenerateTokensAsync(UserModel user);

        /// <summary>
        /// Refresca un token existente, generando un nuevo par de tokens si el token actual
        /// y el refresh token son válidos. El nuevo token mantiene la información del tenant.
        /// </summary>
        /// <param name="token">El token de acceso actual</param>
        /// <param name="refreshToken">El token de refresco actual</param>
        /// <returns>Un nuevo par de tokens si la validación es exitosa, null en caso contrario</returns>
        Task<(string? Token, string? RefreshToken)?> RefreshTokenAsync(string token, string refreshToken);

        /// <summary>
        /// Revoca un token existente, invalidándolo para futuro uso.
        /// Esta operación debe considerar el contexto del tenant.
        /// </summary>
        /// <param name="token">El token a revocar</param>
        /// <returns>true si el token fue revocado exitosamente, false en caso contrario</returns>
        Task<bool> RevokeTokenAsync(string token);

        /// <summary>
        /// Valida un token JWT y retorna el ClaimsPrincipal si es válido.
        /// La validación considera el secreto específico del tenant.
        /// </summary>
        /// <param name="token">El token a validar</param>
        /// <returns>El ClaimsPrincipal si el token es válido, null en caso contrario</returns>
        Task<ClaimsPrincipal?> ValidateTokenAsync(string token);

        /// <summary>
        /// Extrae el ID de usuario y el ID de tenant de un token JWT.
        /// </summary>
        /// <param name="token">El token a analizar</param>
        /// <returns>Tupla con userId y tenantId, o null si no se pueden extraer</returns>
        (long? UserId, long? TenantId)? ExtractIdsFromToken(string token);
    }

    public class TokenService : ITokenService
    {
        private readonly ISecretRepository _secretRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenService> _logger;

        public TokenService(
            ISecretRepository secretRepository,
            IAuthRepository authRepository,
            IConfiguration configuration,
            ILogger<TokenService> logger)
        {
            _secretRepository = secretRepository;
            _authRepository = authRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(string Token, string RefreshToken)> GenerateTokensAsync(UserModel user)
        {
            try
            {
                // Intentar obtener el secreto específico del tenant
                var jwtSecret = await _secretRepository.GetSecretValueAsync("JWT_SECRET", user.TenantId);

                // Si no hay secreto específico, usar el secreto global
                if (string.IsNullOrEmpty(jwtSecret))
                {
                    _logger.LogWarning(
                        "No se encontró secreto específico para tenant {TenantId}, usando secreto global",
                        user.TenantId);
                    jwtSecret = _configuration["JWT:Secret"];
                }

                if (string.IsNullOrEmpty(jwtSecret))
                {
                    throw new InvalidOperationException("No se encontró un secreto JWT válido");
                }

                // Obtener la duración del token en minutos desde la configuración
                if (!int.TryParse(_configuration["JWT:TokenExpirationMinutes"], out int expirationMinutes))
                {
                    expirationMinutes = 60; // Valor predeterminado de 60 minutos
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.ID.ToString()),
                    new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                    new(ClaimTypes.Email, user.Email),
                    new("TenantId", user.TenantId.ToString()),
                    new("TenantName", user.Tenant.Name),
                    new(ClaimTypes.Role, user.Role.Name),
                    new("SecurityStamp", user.SecurityStamp ?? Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                    signingCredentials: credentials
                );

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenString = tokenHandler.WriteToken(token);
                var refreshToken = GenerateRefreshTokenString();

                // Guardar el refresh token en la base de datos
                var refreshTokenModel = new RefreshTokenModel
                {
                    UserId = user.ID,
                    Token = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(7), // 7 días de duración para el refresh token
                    CreatedAt = DateTime.UtcNow
                };

                await _authRepository.CreateRefreshTokenAsync(refreshTokenModel);
                await _authRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "Token generado exitosamente para usuario {UserId} del tenant {TenantId}",
                    user.ID, user.TenantId);

                return (tokenString, refreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error generando tokens para usuario {UserId} del tenant {TenantId}",
                    user.ID, user.TenantId);
                throw;
            }
        }

        public async Task<(string? Token, string? RefreshToken)?> RefreshTokenAsync(string token, string refreshToken)
        {
            try
            {
                // Extraer el ID de usuario y tenant del token
                var ids = ExtractIdsFromToken(token);
                if (!ids.HasValue || !ids.Value.UserId.HasValue || !ids.Value.TenantId.HasValue)
                {
                    _logger.LogError("No se pueden extraer IDs del token");
                    return null;
                }

                var userId = ids.Value.UserId.Value;
                var tenantId = ids.Value.TenantId.Value;

                // Verificar si el refresh token existe y es válido
                var storedRefreshToken = await _authRepository.GetRefreshTokenAsync(userId, refreshToken);
                if (storedRefreshToken == null)
                {
                    _logger.LogWarning("Refresh token no encontrado: {RefreshToken}", refreshToken);
                    return null;
                }

                // Verificar si el refresh token ha expirado o ha sido revocado
                if (storedRefreshToken.ExpiryDate < DateTime.UtcNow || storedRefreshToken.RevokedAt != null)
                {
                    _logger.LogWarning("Refresh token expirado o revocado: {RefreshToken}", refreshToken);
                    return null;
                }

                // Obtener el usuario
                var user = await _authRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {UserId}", userId);
                    return null;
                }

                // Marcar el refresh token anterior como usado
                storedRefreshToken.RevokedAt = DateTime.UtcNow;
                await _authRepository.UpdateRefreshTokenAsync(storedRefreshToken);

                // Generar nuevos tokens
                var (newToken, newRefreshToken) = await GenerateTokensAsync(user);

                _logger.LogInformation(
                    "Token refrescado exitosamente para usuario {UserId} del tenant {TenantId}",
                    userId, tenantId);

                return (newToken, newRefreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refrescando token");
                return null;
            }
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            try
            {
                var ids = ExtractIdsFromToken(token);
                if (!ids.HasValue || !ids.Value.UserId.HasValue)
                {
                    _logger.LogError("No se puede extraer el ID de usuario del token");
                    return false;
                }

                var userId = ids.Value.UserId.Value;

                // Revocar todos los refresh tokens activos del usuario
                await _authRepository.RevokeAllActiveRefreshTokensAsync(userId);

                _logger.LogInformation("Tokens revocados para usuario {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revocando token");
                return false;
            }
        }

        public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Obtener el TenantId del token
                var tenantIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "TenantId");
                if (tenantIdClaim == null || !long.TryParse(tenantIdClaim.Value, out var tenantId))
                {
                    _logger.LogError("TenantId no encontrado en el token");
                    return null;
                }

                // Obtener el secreto específico del tenant
                var jwtSecret = await _secretRepository.GetSecretValueAsync("JWT_SECRET", tenantId)
                    ?? _configuration["JWT:Secret"];

                if (string.IsNullOrEmpty(jwtSecret))
                {
                    _logger.LogError("No se encontró secreto JWT válido para tenant {TenantId}", tenantId);
                    return null;
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["JWT:ValidIssuer"],
                    ValidAudience = _configuration["JWT:ValidAudience"],
                    ClockSkew = TimeSpan.Zero
                };

                try
                {
                    var principal = handler.ValidateToken(token, validationParameters, out _);
                    return principal;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validando token para tenant {TenantId}", tenantId);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando token");
                return null;
            }
        }

        public (long? UserId, long? TenantId)? ExtractIdsFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                var tenantIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "TenantId");

                if (userIdClaim == null || tenantIdClaim == null)
                {
                    return null;
                }

                if (!long.TryParse(userIdClaim.Value, out var userId) ||
                    !long.TryParse(tenantIdClaim.Value, out var tenantId))
                {
                    return null;
                }

                return (userId, tenantId);
            }
            catch
            {
                return null;
            }
        }

        private static string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
