using GenesisFEPortalWeb.ApiService.Authentication;
using GenesisFEPortalWeb.BL.Repositories.Auth;
using GenesisFEPortalWeb.BL.Repositories.Core;
using GenesisFEPortalWeb.BL.Services.Audit;
using GenesisFEPortalWeb.BL.Services.Auth;
using GenesisFEPortalWeb.BL.Services.Core;
using GenesisFEPortalWeb.BL.Services.Notifications;
using GenesisFEPortalWeb.Database.Data;
using GenesisFEPortalWeb.Models.Entities.Tenant;
using GenesisFEPortalWeb.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Resend;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mi API", Version = "v1" });

    // Configurar autenticación JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ignorar referencias circulares
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        // Mantener las mayúsculas/minúsculas de las propiedades
        options.JsonSerializerOptions.PropertyNamingPolicy = null;

        // Ignorar valores nulos
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            JsonIgnoreCondition.WhenWritingNull;
    });


builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();

//Resend
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = Environment.GetEnvironmentVariable("re_A8z1ChH2_Q36aprhNpG1a6WN5PPBt36m8")!;
});
builder.Services.AddTransient<IResend, ResendClient>();
// Registrar el servicio de correo
builder.Services.AddScoped<IEmailService, ResendEmailService>();

#region servicios para autenticación
// Authentication services
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IAuthAuditLogger, AuthAuditLogger>();

// Configuración de seguridad
builder.Services.Configure<SecurityOptions>(
    builder.Configuration.GetSection("Security"));

// Registrar servicios relacionados con tenant y autenticación
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<ISecretService, SecretService>();
// Actualizar el registro del servicio de tokens
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ISecretRepository, SecretRepository>();

// Registrar el manejador de eventos personalizado
builder.Services.AddScoped<TenantJwtBearerEvents>();
builder.Services.AddScoped<IAuthenticationHandler, MultiTenantAuthenticationHandler>();
// JWT Configuration
// Agregar autenticación JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddScheme<JwtBearerOptions, MultiTenantAuthenticationHandler>(
    JwtBearerDefaults.AuthenticationScheme,
    options => {
        var jwtConfig = builder.Configuration.GetSection("JWT");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig["ValidIssuer"],
            ValidAudience = jwtConfig["ValidAudience"],
        };
    });
#endregion



var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapControllers(); // Agregado

// Add authentication middleware to pipeline
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapDefaultEndpoints();

app.Run();
