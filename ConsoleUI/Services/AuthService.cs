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
        private ILogger<Startup> Logger { get; }
        private AssignaClient Client { get; }

        public AuthService(ILogger<Startup> logger, AssignaClient apiClient)
        {
            Logger = logger;
            Client = apiClient;
        }

        // user registration
        public async Task<AuthResult> UserRegisterAsync(Register data)
        {
            try
            {
                var register = new Register
                {
                    UserName = data.UserName,
                    FirstName = data.FirstName,
                    Email = data.Email,
                    Password = data.Password,
                    Role = data.Role
                };

                // serializing
                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(register),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );

                // calling api
                using var response = await Client.Request.PostAsync("user/register", content);
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
                        Success = false,
                        Message = "Request not succeeded"
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new AuthResult
                {
                    Success = false,
                    Message = "Internal error"
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
                    UserName = data.UserName,
                    Password = data.Password,
                };

                // serializing
                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(register),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );

                // calling api
                using var response = await Client.Request.PostAsync("user/login", content);
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
                        Success = false,
                        Message = "Request not succeeded"
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new AuthResult
                {
                    Success = false,
                    Message = "Internal error"
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
                    Email = data.Email
                };

                // serializing
                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(register),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );

                // calling api
                using var response = await Client.Request.PostAsync("user/forgot-password", content);
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
                        Success = false,
                        Message = "Request not succeeded"
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new AuthResult
                {
                    Success = false,
                    Message = "Internal error"
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
                    Password = data.Password,
                    ConPassword = data.ConPassword,
                    ResetToken = data.ResetToken
                };

                // serializing
                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(register),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );

                // calling api
                using var response = await Client.Request.PostAsync("user/reset-password", content);
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
                        Success = false,
                        Message = "Request not succeeded"
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new AuthResult
                {
                    Success = false,
                    Message = "Internal error"
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
                    TokenRefresh=data.TokenRefresh
                };

                // serializing
                var content = new StringContent
                    (
                        JsonConvert.SerializeObject(register),
                        encoding: Encoding.UTF8,
                        mediaType: "application/json"
                    );

                // calling api
                using var response = await Client.Request.PostAsync("user/refresh-token", content);
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
                        Success = false,
                        Message = "Request not succeeded"
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return new AuthResult
                {
                    Success = false,
                    Message = "Internal error"
                };
            }
        }
    }
}