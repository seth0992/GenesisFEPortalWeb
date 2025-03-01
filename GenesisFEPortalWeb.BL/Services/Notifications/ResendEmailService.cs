using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;
using System;
using System.Threading.Tasks;

namespace GenesisFEPortalWeb.BL.Services.Notifications
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task<bool> SendPasswordResetEmailAsync(string to, string resetLink);
    }

    public class ResendEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ResendEmailService> _logger;
        private readonly IResend _resend;

        public ResendEmailService(
            IConfiguration configuration,
            ILogger<ResendEmailService> logger,
            IResend resend)
        {
            _configuration = configuration;
            _logger = logger;
            _resend = resend;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var fromEmail = _configuration["Resend:FromEmail"];
                var fromName = _configuration["Resend:FromName"];

                var message = new EmailMessage
                {
                    From = $"{fromName} <{fromEmail}>"
                };
                message.To.Add(to);
                message.Subject = subject;

                if (isHtml)
                {
                    message.HtmlBody = body;
                }
                else
                {
                    message.TextBody = body;
                }

                var response = await _resend.EmailSendAsync(message);

                // Verificar si la respuesta es exitosa
                if (response != null)
                {
                    // Usar ToString() para mostrar el identificador (Guid)
                    _logger.LogInformation("Correo enviado exitosamente a {Email}, Id: {Id}", to, response.Content.ToString().ToLowerInvariant());
                    return true;
                }
                else
                {
                    _logger.LogWarning("Error enviando correo a {Email}", to);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando correo a {Email}", to);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string to, string resetLink)
        {
            var subject = "Restablecimiento de contraseña - GenesisFE Portal";

            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: #fff; padding: 20px; border-radius: 5px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; padding-bottom: 20px; border-bottom: 1px solid #eee; margin-bottom: 20px; }}
                        .footer {{ text-align: center; margin-top: 20px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #999; }}
                        .btn {{ display: inline-block; background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Restablecimiento de Contraseña</h2>
                        </div>
                        <p>Hola,</p>
                        <p>Hemos recibido una solicitud para restablecer la contraseña de tu cuenta en GenesisFE Portal.</p>
                        <p>Haz clic en el siguiente enlace para restablecer tu contraseña:</p>
                        <p style='text-align: center;'>
                            <a href='{resetLink}' class='btn'>Restablecer Contraseña</a>
                        </p>
                        <p>Si no solicitaste este cambio, puedes ignorar este mensaje y tu contraseña permanecerá sin cambios.</p>
                        <p>Este enlace expirará en 24 horas.</p>
                        <p>Saludos,<br>El equipo de GenesisFE</p>
                        <div class='footer'>
                            <p>Este es un correo automático, por favor no respondas a este mensaje.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            return await SendEmailAsync(to, subject, body);
        }
    }
}
