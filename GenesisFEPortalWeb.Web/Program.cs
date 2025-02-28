using Blazored.Toast;
using GenesisFEPortalWeb.Web;
using GenesisFEPortalWeb.Web.Authentication;
using GenesisFEPortalWeb.Web.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.Services.AddScoped<DialogService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Login & Authentication services
builder.Services.AddAuthenticationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddControllers();
builder.Services.AddLocalization(); //Agregar la localizacion 

// Configuraci�n de autenticaci�n para Blazor Server
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "ServerAuth";
    options.DefaultChallengeScheme = "ServerAuth";
})
.AddCookie("ServerAuth", options =>
{
    options.Cookie.Name = "BlazorServerAuth";
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRadzenComponents(); // Library for radzen
builder.Services.AddBlazoredToast(); // Library for toast notifications -> Change to your preferred library.

builder.Services.AddOutputCache();

builder.Services.AddHttpClient<ApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
