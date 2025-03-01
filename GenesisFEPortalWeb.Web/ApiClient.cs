using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using GenesisFEPortalWeb.Models.Models.Auth;
using GenesisFEPortalWeb.Models.Models;
using GenesisFEPortalWeb.Web.Authentication;

namespace GenesisFEPortalWeb.Web;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ProtectedLocalStorage _localStorage;
    private readonly NavigationManager _navigationManager;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILogger<ApiClient> _logger;
    private bool _isRefreshing = false;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public ApiClient(
        HttpClient httpClient,
        ProtectedLocalStorage localStorage,
        NavigationManager navigationManager,
        AuthenticationStateProvider authStateProvider,
        ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _navigationManager = navigationManager;
        _authStateProvider = authStateProvider;
        _logger = logger;
    }

    //public async Task SetAuthorizationHeader()
    //{
    //    try
    //    {
    //        var sessionResult = await _localStorage.GetAsync<LoginResponseModel>("sessionState");
    //        var session = sessionResult.Success ? sessionResult.Value : null;

    //        if (session == null || string.IsNullOrEmpty(session.Token))
    //        {
    //            throw new Exception("No session found");
    //        }

    //        // Agregar el token a los headers
    //        _httpClient.DefaultRequestHeaders.Authorization =
    //            new AuthenticationHeaderValue("Bearer", session.Token);
    //    }
    //    catch (Exception)
    //    {
    //        await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
    //        _navigationManager.NavigateTo("/login");
    //    }
    //}
    public async Task SetAuthorizationHeader()
    {
        try
        {
            await _semaphore.WaitAsync();

            var sessionResult = await _localStorage.GetAsync<LoginResponseModel>("sessionState");
            var session = sessionResult.Success ? sessionResult.Value : null;

            if (session == null || string.IsNullOrEmpty(session.Token))
            {
                throw new Exception("No session found");
            }

            // Verificar si el token está por expirar (menos de 5 minutos de vida)
            var isTokenExpiring = session.TokenExpired - DateTimeOffset.UtcNow.ToUnixTimeSeconds() < 300;

            // Si el token está por expirar, intentar refrescarlo
            if (isTokenExpiring && !_isRefreshing)
            {
                _isRefreshing = true;
                var refreshed = await RefreshTokenAsync(session.Token, session.RefreshToken);
                _isRefreshing = false;

                if (!refreshed)
                {
                    await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
                    _navigationManager.NavigateTo("/login");
                    return;
                }

                // Obtenemos el nuevo token de la sesión actualizada
                sessionResult = await _localStorage.GetAsync<LoginResponseModel>("sessionState");
                session = sessionResult.Success ? sessionResult.Value : null;

                if (session == null || string.IsNullOrEmpty(session.Token))
                {
                    throw new Exception("Session refresh failed");
                }
            }

            // Agregar el token a los headers
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", session.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting authorization header");
            await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
            _navigationManager.NavigateTo("/login");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<bool> RefreshTokenAsync(string token, string refreshToken)
    {
        try
        {
            // Eliminar el token actual del header para evitar bucles
            _httpClient.DefaultRequestHeaders.Authorization = null;

            var refreshRequest = new RefreshTokenRequest
            {
                Token = token,
                RefreshToken = refreshToken
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/refresh-token", refreshRequest);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var baseResponse = JsonConvert.DeserializeObject<BaseResponseModel>(jsonString);

            if (baseResponse == null || !baseResponse.Success)
            {
                return false;
            }

            // Extraer los datos de la respuesta
            var dataJson = JsonConvert.SerializeObject(baseResponse.Data);
            var refreshResponse = JsonConvert.DeserializeObject<dynamic>(dataJson);

            if (refreshResponse == null)
            {
                return false;
            }

            // Actualizar la sesión con los nuevos tokens
            var sessionResult = await _localStorage.GetAsync<LoginResponseModel>("sessionState");
            if (!sessionResult.Success)
            {
                return false;
            }

            var session = sessionResult.Value;
            session.Token = refreshResponse.token;
            session.RefreshToken = refreshResponse.refreshToken;
            session.TokenExpired = refreshResponse.tokenExpired;

            await _localStorage.SetAsync("sessionState", session);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return false;
        }
    }


    public async Task<T> GetFromJsonAsync<T>(string path)
    {
        await SetAuthorizationHeader();
        var result = await _httpClient.GetFromJsonAsync<T>(path);
        return result ?? throw new InvalidOperationException("Received null response from the server.");
    }

    public async Task<T?> PatchAsync<T>(string requestUri, object? content = null)
    {
        var jsonContent = content != null ? JsonContent.Create(content) : null;
        var response = await _httpClient.PatchAsync(requestUri, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new ApplicationException($"Error en la solicitud: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<T>();
        return result;
    }

    /// <summary>
    /// Realiza una petición POST asíncrona
    /// </summary>
    /// <typeparam name="T1">Tipo de retorno esperado. Puede ser:
    /// - BaseResponseModel: Retorna la respuesta completa
    /// - bool: Retorna el valor de Success
    /// - otro tipo: Intenta deserializar desde Data
    /// </typeparam>
    /// <typeparam name="T2">Tipo del modelo a enviar</typeparam>
    public async Task<T1> PostAsync<T1, T2>(string path, T2 postModel)
    {
        // Caso especial para el login
        // No establecemos cabecera de autorización para login y operaciones relacionadas con la contraseña
        if (!path.Contains("/api/auth/login") &&
            !path.Contains("/api/auth/forgot-password") &&
            !path.Contains("/api/auth/reset-password") &&
            !path.Contains("/api/auth/validate-reset-token"))
        {
            await SetAuthorizationHeader();
        }

        try
        {                

            // Realizamos la petición POST
            var response = await _httpClient.PostAsJsonAsync(path, postModel);

            if (response != null && response.IsSuccessStatusCode)
            {
                // Leemos el contenido de la respuesta
                var jsonString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Respuesta del servidor: {jsonString}"); // Para diagnóstico

                // Deserializamos a BaseResponseModel
                var baseResponse = JsonConvert.DeserializeObject<BaseResponseModel>(jsonString);

                if (baseResponse != null)
                {
                    // Si T1 es BaseResponseModel, devolvemos directamente
                    if (typeof(T1) == typeof(BaseResponseModel))
                    {
                        return (T1)(object)baseResponse;
                    }

                    // Para otros tipos, intentamos deserializar el Data
                    if (baseResponse.Success)
                    {
                        try
                        {
                            // Si Data es null, y T1 es un tipo por valor (como bool),
                            // devolvemos el valor Success
                            if (baseResponse.Data == null && typeof(T1).IsValueType)
                            {
                                return (T1)(object)baseResponse.Success;
                            }

                            // En otro caso, intentamos deserializar Data
                            string dataJson = JsonConvert.SerializeObject(baseResponse.Data);
                            var result = JsonConvert.DeserializeObject<T1>(dataJson);
                            return result!;
                        }
                        catch (JsonSerializationException ex)
                        {
                            Console.WriteLine($"Error deserializando Data: {ex.Message}");
                            // Si falla la deserialización y T1 es bool, devolvemos Success
                            if (typeof(T1) == typeof(bool))
                            {
                                return (T1)(object)baseResponse.Success;
                            }
                            throw;
                        }
                    }
                    throw new ApplicationException(baseResponse.ErrorMessage ?? "Error desconocido en la respuesta");
                }
                throw new ApplicationException("Respuesta nula del servidor");
            }
            throw new HttpRequestException($"Error en la respuesta HTTP: {response?.StatusCode}");
        }
        catch (JsonSerializationException ex)
        {
            Console.WriteLine($"Error de serialización: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error general: {ex.Message}");
            throw new HttpRequestException($"Error en la petición a {path}: {ex.Message}", ex);
        }
    }
    private async Task<LoginResponseModel> ProcessLoginResponse(HttpResponseMessage response)
    {
        try
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException($"Error de servidor: {response.StatusCode}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            _logger.LogDebug($"Raw response from login: {jsonString}");

            var baseResponse = JsonConvert.DeserializeObject<BaseResponseModel>(jsonString);

            if (baseResponse == null)
            {
                throw new ApplicationException("La respuesta del servidor está vacía");
            }

            if (!baseResponse.Success)
            {
                throw new ApplicationException(baseResponse.ErrorMessage);
            }

            // Convertir el objeto Data a LoginResponseModel
            var dataJson = JsonConvert.SerializeObject(baseResponse.Data);
            var loginResponse = JsonConvert.DeserializeObject<LoginResponseModel>(dataJson);

            if (loginResponse == null)
            {
                throw new ApplicationException("Error al procesar la respuesta de login");
            }

            // Validar que tenemos toda la información necesaria
            if (string.IsNullOrEmpty(loginResponse.Token) ||
                string.IsNullOrEmpty(loginResponse.RefreshToken) ||
                loginResponse.User == null)
            {
                throw new ApplicationException("Respuesta de login incompleta");
            }

            return loginResponse;
        }
        catch (JsonSerializationException ex)
        {
            _logger.LogError(ex, "Error de deserialización");
            throw new ApplicationException("Error al procesar la respuesta del servidor", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error general en ProcessLoginResponse");
            throw new ApplicationException("Error en el proceso de login", ex);
        }
    }

    public async Task<T1> PutAsync<T1, T2>(string path, T2 postModel)
    {
        await SetAuthorizationHeader();
        var response = await _httpClient.PutAsJsonAsync(path, postModel);
        if (response != null && response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<T1>(await response.Content.ReadAsStringAsync()!)!;
        }
        return default!;
    }

    public async Task<T> DeleteAsync<T>(string path)
    {
        await SetAuthorizationHeader();
        var result = await _httpClient.DeleteFromJsonAsync<T>(path);
        return result ?? throw new InvalidOperationException("Received null response from the server.");
    }
}

