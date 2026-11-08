using System.Net;
using System.Text;
using System.Text.Json;

namespace Raceday_Event_Management_System.MVC.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string ApiSessionKey = "ApiSessionCookie";

        public ApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AttachSessionCookie()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return;

            var storedCookie = session.GetString(ApiSessionKey);
            if (string.IsNullOrEmpty(storedCookie)) return;

            _http.DefaultRequestHeaders.Remove("Cookie");
            _http.DefaultRequestHeaders.Add("Cookie", storedCookie);
        }

        private void SaveSessionCookie(HttpResponseMessage response)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return;

            if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                var cookie = cookies.FirstOrDefault(c => c.StartsWith(".AspNetCore.Session"));
                if (!string.IsNullOrEmpty(cookie))
                {
                    var nameValue = cookie.Split(';')[0];
                    session.SetString(ApiSessionKey, nameValue);
                }
            }
        }

        public void ClearSessionCookie()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(ApiSessionKey);
        }

        // ============================================================
        // GET
        // ============================================================
        public async Task<ApiResponse<T>?> GetAsync<T>(string endpoint)
        {
            AttachSessionCookie();

            try
            {
                var response = await _http.GetAsync(endpoint);
                SaveSessionCookie(response);

                return await BuildResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    StatusCode = 0,
                    ErrorMessage = $"Network error: {ex.Message}"
                };
            }
        }

        // ============================================================
        // POST
        // ============================================================
        public async Task<ApiResponse<T>?> PostAsync<T>(string endpoint, object? body = null)
        {
            AttachSessionCookie();

            try
            {
                HttpContent? content = null;
                if (body != null)
                {
                    var json = JsonSerializer.Serialize(body);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await _http.PostAsync(endpoint, content);
                SaveSessionCookie(response);

                return await BuildResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    StatusCode = 0,
                    ErrorMessage = $"Network error: {ex.Message}"
                };
            }
        }

        // ============================================================
        // POST MULTIPART (for future file uploads)
        // ============================================================
        public async Task<ApiResponse<T>?> PostMultipartAsync<T>(string endpoint, MultipartFormDataContent content)
        {
            AttachSessionCookie();

            try
            {
                var response = await _http.PostAsync(endpoint, content);
                SaveSessionCookie(response);

                return await BuildResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    StatusCode = 0,
                    ErrorMessage = $"Network error: {ex.Message}"
                };
            }
        }

        // ============================================================
        // PUT
        // ============================================================
        public async Task<ApiResponse<T>?> PutAsync<T>(string endpoint, object? body = null)
        {
            AttachSessionCookie();

            try
            {
                HttpContent? content = null;
                if (body != null)
                {
                    var json = JsonSerializer.Serialize(body);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await _http.PutAsync(endpoint, content);
                SaveSessionCookie(response);

                return await BuildResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    StatusCode = 0,
                    ErrorMessage = $"Network error: {ex.Message}"
                };
            }
        }

        // ============================================================
        // PATCH
        // ============================================================
        public async Task<ApiResponse<T>?> PatchAsync<T>(string endpoint, object? body = null)
        {
            AttachSessionCookie();

            try
            {
                HttpContent? content = null;
                if (body != null)
                {
                    var json = JsonSerializer.Serialize(body);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint) { Content = content };
                var response = await _http.SendAsync(request);
                SaveSessionCookie(response);

                return await BuildResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    StatusCode = 0,
                    ErrorMessage = $"Network error: {ex.Message}"
                };
            }
        }

        // ============================================================
        // DELETE
        // ============================================================
        public async Task<ApiResponse<T>?> DeleteAsync<T>(string endpoint)
        {
            AttachSessionCookie();

            try
            {
                var response = await _http.DeleteAsync(endpoint);
                SaveSessionCookie(response);

                return await BuildResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    StatusCode = 0,
                    ErrorMessage = $"Network error: {ex.Message}"
                };
            }
        }

        // ============================================================
        // HELPER: Parse Response
        // ============================================================
        private async Task<ApiResponse<T>> BuildResponseAsync<T>(HttpResponseMessage response)
        {
            var result = new ApiResponse<T>
            {
                StatusCode = (int)response.StatusCode,
                IsSuccess = response.IsSuccessStatusCode
            };

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(content))
            {
                return result;
            }

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    result.Data = JsonSerializer.Deserialize<T>(content, options);
                }
                catch
                {
                    result.ErrorMessage = "Failed to deserialize successful response.";
                }
            }
            else
            {
                try
                {
                    var errorObj = JsonSerializer.Deserialize<ApiErrorResponse>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    result.ErrorMessage = errorObj?.Message ?? content;
                }
                catch
                {
                    result.ErrorMessage = content;
                }
            }

            return result;
        }
    }

    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class ApiErrorResponse
    {
        public string? Message { get; set; }
    }
}