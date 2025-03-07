﻿using GenesisFEPortalWeb.BL.Services.Auth;
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
                    Data = new { token, refreshToken }
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
    }
}
