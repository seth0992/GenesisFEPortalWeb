using GenesisFEPortalWeb.BL.Repositories.Auth;
using GenesisFEPortalWeb.BL.Repositories.Core;
using GenesisFEPortalWeb.BL.Services.Audit;
using GenesisFEPortalWeb.BL.Services.Notifications;
using GenesisFEPortalWeb.Models.Entities.Security;
using GenesisFEPortalWeb.Models.Entities.Tenant;
using GenesisFEPortalWeb.Models.Models.Auth;
using GenesisFEPortalWeb.Models.Models.Exceptions;
using GenesisFEPortalWeb.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace GenesisFEPortalWeb.BL.Services.Auth
{
    public interface IAuthService
    {
        Task<(UserModel? User, string? Token, string? RefreshToken)> LoginAsync(LoginDto model);
        Task<(string? Token, string? RefreshToken)> RefreshTokenAsync(string token, string refreshToken);
        Task<bool> RevokeTokenAsync(string token);

        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ValidatePasswordResetTokenAsync(string email, string token);
        Task<bool> ResetPasswordAsync(ResetPasswordDto model);
    }


    public class AuthService : IAuthService
    {
        private const int MaxFailedAttempts = 5;
        private readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        private readonly IAuthRepository _authRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly ITenantService _tenantService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IAuthAuditLogger _authAuditLogger;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthService(
            IAuthRepository authRepository,
            ITenantRepository tenantRepository,
            ITenantService tenantService,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IAuthAuditLogger authAuditLogger, 
            IEmailService emailService,
             IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _authRepository = authRepository;
            _tenantRepository = tenantRepository;
            _tenantService = tenantService;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _authAuditLogger = authAuditLogger;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(UserModel? User, string? Token, string? RefreshToken)> LoginAsync(LoginDto model)
        {
            try
            {
                var user = await _authRepository.GetUserByEmailAsync(model.Email);

                if (user == null || !user.IsActive || !user.Tenant.IsActive)
                {
                    await _authAuditLogger.LogLoginAttempt(model.Email, false, "Usuario inactivo o no encontrado");
                    return (null, null, null);
                }

                if (await _authRepository.IsUserLockedOutAsync(user.ID))
                {
                    await _authAuditLogger.LogLoginAttempt(model.Email, false, "Cuenta bloqueada");
                    throw new AccountLockedException("La cuenta está temporalmente bloqueada por múltiples intentos fallidos");
                }

                if (!_passwordHasher.VerifyPassword(model.Password, user.PasswordHash))
                {
                    await HandleFailedLogin(user.ID);
                    await _authAuditLogger.LogLoginAttempt(model.Email, false, "Contraseña incorrecta");
                    return (null, null, null);
                }


                await HandleSuccessfulLogin(user.ID);
                var (token, refreshToken) = await _tokenService.GenerateTokensAsync(user);

                return (user, token, refreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el proceso de login para {Email}", model.Email);
                throw;
            }
        }

        public async Task<(string? Token, string? RefreshToken)> RefreshTokenAsync(string token, string refreshToken)
        {
            var result = await _tokenService.RefreshTokenAsync(token, refreshToken);
            if (!result.HasValue)
            {
                return (null, null);
            }

            var (newToken, newRefreshToken) = result.Value;
            return (newToken, newRefreshToken);
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            return await _tokenService.RevokeTokenAsync(token);
        }


        private async Task HandleFailedLogin(long userId)
        {
            await _authRepository.IncrementAccessFailedCountAsync(userId);
            var failedCount = await _authRepository.GetAccessFailedCountAsync(userId);

            if (failedCount >= MaxFailedAttempts)
            {
                await _authRepository.UpdateUserLockoutAsync(userId, DateTime.UtcNow.Add(LockoutDuration));
                _logger.LogWarning("Usuario bloqueado por múltiples intentos fallidos: {UserId}", userId);
            }
        }

        private async Task HandleSuccessfulLogin(long userId)
        {
            await _authRepository.ResetAccessFailedCountAsync(userId);
            await _authRepository.UpdateUserLockoutAsync(userId, null);
            await _authRepository.UpdateUserLastLoginAsync(userId, DateTime.UtcNow);
            await _authRepository.UpdateUserSecurityStampAsync(userId, Guid.NewGuid().ToString());
        }

        //public async Task<bool> ForgotPasswordAsync(string email)
        //{
        //    try
        //    {
        //        var user = await _authRepository.GetUserByEmailAsync(email);
        //        if (user == null || !user.IsActive || !user.Tenant.IsActive)
        //        {
        //            // No indicamos si el usuario existe o no por seguridad
        //            _logger.LogWarning("Intento de recuperación de contraseña para correo inexistente: {Email}", email);
        //            return true;
        //        }

        //        // Generar token único para restablecimiento
        //        var token = GenerateSecureToken();
        //        var expiryDate = DateTime.UtcNow.AddHours(24); // El token expira en 24 horas

        //        // Guardar el token en la base de datos
        //        await _authRepository.GeneratePasswordResetTokenAsync(user.ID, token, expiryDate);

        //        // Aquí deberías enviar un correo con el enlace de restablecimiento
        //        // Por ahora, solo registramos que se ha generado el token
        //        _logger.LogInformation("Token de restablecimiento generado para {Email}: {Token}", email, token);
        //        await _authAuditLogger.LogLoginAttempt(
        //            user.Email,
        //            true,
        //            $"Token de restablecimiento de contraseña generado");

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error en proceso de olvido de contraseña para {Email}", email);
        //        return false;
        //    }
        //}

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _authRepository.GetUserByEmailAsync(email);
                if (user == null || !user.IsActive || !user.Tenant.IsActive)
                {
                    // No indicamos si el usuario existe o no por seguridad
                    _logger.LogWarning("Intento de recuperación de contraseña para correo inexistente: {Email}", email);
                    return true;
                }

                // Generar token único para restablecimiento
                var token = GenerateSecureToken();
                var expiryDate = DateTime.UtcNow.AddHours(24); // El token expira en 24 horas

                // Guardar el token en la base de datos
                await _authRepository.GeneratePasswordResetTokenAsync(user.ID, token, expiryDate);

                // Generar URL de restablecimiento
                var baseUrl = _configuration["ApplicationUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    baseUrl = "https://localhost:7214"; // URL por defecto para desarrollo
                }

                var resetLink = $"{baseUrl}/reset-password?token={token}&email={Uri.EscapeDataString(email)}";

                // Enviar correo con enlace de restablecimiento
                await _emailService.SendPasswordResetEmailAsync(email, resetLink);

                await _authAuditLogger.LogLoginAttempt(
                    user.Email,
                    true,
                    $"Token de restablecimiento de contraseña generado");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en proceso de olvido de contraseña para {Email}", email);
                return false;
            }
        }

        public async Task<bool> ValidatePasswordResetTokenAsync(string email, string token)
        {
            try
            {
                return await _authRepository.ValidatePasswordResetTokenAsync(email, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando token de restablecimiento para {Email}", email);
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto model)
        {
            try
            {
                // Validar el token
                if (!await ValidatePasswordResetTokenAsync(model.Email, model.Token))
                {
                    return false;
                }

                // Obtener el usuario
                var user = await _authRepository.GetUserByPasswordResetTokenAsync(model.Token);
                if (user == null)
                {
                    return false;
                }

                // Cambiar la contraseña
                var newPasswordHash = _passwordHasher.HashPassword(model.Password);
                await _authRepository.UpdateUserPasswordAsync(user.ID, newPasswordHash);

                // Invalidar el token
                await _authRepository.InvalidatePasswordResetTokenAsync(user.ID);

                // Registrar la acción
                await _authAuditLogger.LogLoginAttempt(
                    user.Email,
                    true,
                    "Contraseña restablecida correctamente");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restableciendo contraseña");
                return false;
            }
        }

        private string GenerateSecureToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber)
                         .Replace("+", "-")
                         .Replace("/", "_")
                         .Replace("=", "")
                         .Substring(0, 20);
        }


    }
}
