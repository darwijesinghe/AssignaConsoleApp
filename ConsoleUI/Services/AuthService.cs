using ConsoleUI.ApiClient;
using ConsoleUI.Auth;
using ConsoleUI.Interfaces;
using ConsoleUI.Response;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleUI.Services
{
    public class AuthService : IAuthService
    {
        // services
        private ILogger<Startup> _logger { get; }
        private AssignaClient _client { get; }

        public AuthService(ILogger<Startup> logger, AssignaClient apiClient)
        {
            _logger = logger;
            _client = apiClient;
        }

        // user registration
        public async Task<AuthResult> UserRegisterAsync(Register data)
        {
            try
            {
                var register = new Register
                {
                    user_name = data.user_name,
                    first_name = data.first_name,
                    email = data.email,
                    password = data.password,
                    role = data.role
                };

                // serializing
                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(register),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );

                // calling api
                using var response = await _client.request.PostAsync("user/register", content);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    // deserializing
                    AuthResult? authResult = JsonConvert.DeserializeObject<AuthResult>(result);

                    return authResult ?? new AuthResult();
                }
                else
                {
                    return new AuthResult
                    {
                        success = false,
                        message = "Request not succeeded"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new AuthResult
                {
                    success = false,
                    message = "Internal error"
                };
            }
        }

        // user login
        public async Task<AuthResult> UserLoginAsync(Login data)
        {
            try
            {
                var register = new Register
                {
                    user_name = data.user_name,
                    password = data.password,
                };

                // serializing
                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(register),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );

                // calling api
                using var response = await _client.request.PostAsync("user/login", content);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    // deserializing
                    AuthResult? authResult = JsonConvert.DeserializeObject<AuthResult>(result);

                    return authResult ?? new AuthResult();
                }
                else
                {
                    return new AuthResult
                    {
                        success = false,
                        message = "Request not succeeded"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new AuthResult
                {
                    success = false,
                    message = "Internal error"
                };
            }
        }

        // forgot password
        public async Task<AuthResult> ForgotPasswordAsync(ForgotPassword data)
        {
            try
            {
                var register = new ForgotPassword
                {
                    email = data.email
                };

                // serializing
                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(register),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );

                // calling api
                using var response = await _client.request.PostAsync("user/forgot-password", content);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    // deserializing
                    AuthResult? authResult = JsonConvert.DeserializeObject<AuthResult>(result);

                    return authResult ?? new AuthResult();
                }
                else
                {
                    return new AuthResult
                    {
                        success = false,
                        message = "Request not succeeded"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new AuthResult
                {
                    success = false,
                    message = "Internal error"
                };
            }
        }

        // reset password
        public async Task<AuthResult> ResetPasswordAsync(ResetPassword data)
        {
            try
            {
                var register = new ResetPassword
                {
                    password = data.password,
                    con_password = data.con_password,
                    reset_token = data.reset_token
                };

                // serializing
                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(register),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );

                // calling api
                using var response = await _client.request.PostAsync("user/reset-password", content);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    // deserializing
                    AuthResult? authResult = JsonConvert.DeserializeObject<AuthResult>(result);

                    return authResult ?? new AuthResult();
                }
                else
                {
                    return new AuthResult
                    {
                        success = false,
                        message = "Request not succeeded"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new AuthResult
                {
                    success = false,
                    message = "Internal error"
                };
            }
        }

        // refresh token
        public async Task<AuthResult> RefreshTokenAsync(RefreshToken data)
        {
            try
            {
                var register = new RefreshToken
                {
                    refresh_token=data.refresh_token
                };

                // serializing
                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(register),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );

                // calling api
                using var response = await _client.request.PostAsync("user/refresh-token", content);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    // deserializing
                    AuthResult? authResult = JsonConvert.DeserializeObject<AuthResult>(result);

                    return authResult ?? new AuthResult();
                }
                else
                {
                    return new AuthResult
                    {
                        success = false,
                        message = "Request not succeeded"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new AuthResult
                {
                    success = false,
                    message = "Internal error"
                };
            }
        }
    }
}