using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
   
   namespace BlazorAuthentication.Client.Services
   {
       public class AuthenticationService : IAuthenticationService
       {
           private readonly HttpClient _httpClient;
           private readonly ILocalStorageService _localStorage;
   
           public AuthenticationService(HttpClient httpClient, ILocalStorageService localStorage)
           {
               _httpClient = httpClient;
               _localStorage = localStorage;
           }
   
           public async Task<bool> LoginAsync(string username, string password)
           {
               var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { username, password });
   
               if (response.IsSuccessStatusCode)
               {
                   var content = await response.Content.ReadAsStringAsync();
                   var result = JsonSerializer.Deserialize<LoginResult>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
   
                   await _localStorage.SetItemAsync("authToken", result.Token);
   
                   _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);
   
                   return true;
               }
   
               return false;
           }
   
           public async Task LogoutAsync()
           {
               await _localStorage.RemoveItemAsync("authToken");
               _httpClient.DefaultRequestHeaders.Authorization = null;
           }
   
           public async Task<bool> IsAuthenticatedAsync()
           {
               var token = await _localStorage.GetItemAsync<string>("authToken");
               return !string.IsNullOrEmpty(token);
           }
       }
   
       public class LoginResult
       {
           public string Token { get; set; }
       }
   
       public interface IAuthenticationService
       {
           Task<bool> LoginAsync(string username, string password);
           Task LogoutAsync();
           Task<bool> IsAuthenticatedAsync();
       }
   }
   