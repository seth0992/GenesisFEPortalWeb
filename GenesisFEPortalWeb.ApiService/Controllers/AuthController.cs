using GenesisFEPortalWeb.BL.Services.Auth;
using GenesisFEPortalWeb.Models.Models;
using GenesisFEPortalWeb.Models.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenesisFEPortalWeb.ApiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<BaseResponseModel>> Login([FromBody] LoginDto model)
        {
            var (user, token, refreshToken) = await _authService.LoginAsync(model);

            if (user == null || token == null)
            {
                return Ok(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Credenciales inválidas"
                });
            }

            // Crear el objeto de respuesta de login
            var loginResponse = new LoginResponseModel
            {
                Token = token,
                RefreshToken = refreshToken!,
                TokenExpired = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds(),
                User = new UserDto
                {
                    Id = user.ID,
                    Email = user.Email,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    RoleName = user.Role.Name
                }
            };

            // Envolver en BaseResponseModel
            return Ok(new BaseResponseModel
            {
                Success = true,
                Data = loginResponse    // Este es el objeto que necesitamos deserializar
            });
        }

        //[HttpPost("refresh-token")]
        //public async Task<ActionResult<BaseResponseModel>> RefreshToken([FromBody] RefreshTokenRequest request)
        //{
        //    try
        //    {
        //        var (token, refreshToken) = await _authService.RefreshTokenAsync(
        //            request.Token,
        //            request.RefreshToken);

        //        if (token == null)
        //        {
        //            return Ok(new BaseResponseModel
        //            {
        //                Success = false,
        //                ErrorMessage = "Token inválido o expirado"
        //            });
        //        }

        //        return Ok(new BaseResponseModel
        //        {
        //            Success = true,
        //            Data = new { token, refreshToken }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error en refresh token");
        //        return Ok(new BaseResponseModel
        //        {
        //            Success = false,
        //            ErrorMessage = "Error al procesar la solicitud"
        //        });
        //    }
        //}

        [HttpPost("revoke")]
        [Authorize]
        public async Task<ActionResult<BaseResponseModel>> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            try
            {
                var success = await _authService.RevokeTokenAsync(request.Token);

                if (!success)
                {
                    return Ok(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Token inválido o expirado"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = "Token revocado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en revoke token");
                return Ok(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error al procesar la solicitud"
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<BaseResponseModel>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var (token, refreshToken) = await _authService.RefreshTokenAsync(
                    request.Token,
                    request.RefreshToken);

                if (token == null)
                {
                    return Ok(new BaseResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Token inválido o expirado"
                    });
                }

                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = new { token, refreshToken, tokenExpired = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds() }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en refresh token");
                return Ok(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error al procesar la solicitud"
                });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<BaseResponseModel>> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            try
            {
                var result = await _authService.ForgotPasswordAsync(model.Email);

                // Siempre retornamos éxito por seguridad, incluso si el email no existe
                return Ok(new BaseResponseModel
                {
                    Success = true,
                    Data = "Si el correo existe en nuestra base de datos, recibirás un email con instrucciones para restablecer tu contraseña."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en solicitud de restablecimiento de contraseña");
                return Ok(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error al procesar la solicitud"
                });
            }
        }

        [HttpPost("validate-reset-token")]
        public async Task<ActionResult<BaseResponseModel>> ValidateResetToken([FromBody] ResetPasswordDto model)
        {
            try
            {
                var isValid = await _authService.ValidatePasswordResetTokenAsync(model.Email, model.Token);

                return Ok(new BaseResponseModel
                {
                    Success = isValid,
                    ErrorMessage = isValid ? null : "El token es inválido o ha expirado"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando token de restablecimiento");
                return Ok(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error al procesar la solicitud"
                });
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<BaseResponseModel>> ResetPassword([FromBody] ResetPasswordDto model)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(model);

                return Ok(new BaseResponseModel
                {
                    Success = result,
                    ErrorMessage = result ? null : "No se pudo restablecer la contraseña",
                    Data = result ? "Contraseña restablecida correctamente" : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restableciendo contraseña");
                return Ok(new BaseResponseModel
                {
                    Success = false,
                    ErrorMessage = "Error al procesar la solicitud"
                });
            }
        }
    }
}
