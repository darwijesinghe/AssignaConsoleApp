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
    /// <summary>
    /// Implementation for the IAuthService
    /// </summary>
    public class AuthService : IAuthService
    {
        // Services
        private ILogger<Startup> _logger;
        private AssignaClient    _client;

        public AuthService(ILogger<Startup> logger, AssignaClient apiClient)
        {
            _logger = logger;
            _client = apiClient;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="data">The user registration data.</param>
        /// <returns>
        /// An <see cref="AuthResult"/> result.
        /// </returns>
        public async Task<AuthResult> UserRegisterAsync(Register data)
        {
            try
            {
                // registration data
                var register = new Register
                {
                    UserName  = data.UserName,
                    FirstName = data.FirstName,
                    Email     = data.Email,
                    Password  = data.Password,
                    Role      = data.Role
                };

                // returns the result
                return await MakePostRequest(data: register, url: "user/register");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new AuthResult
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }

        /// <summary>
        /// Logins a new user.
        /// </summary>
        /// <param name="data">The user login data.</param>
        /// <returns>
        /// An <see cref="AuthResult"/> result.
        /// </returns>
        public async Task<AuthResult> UserLoginAsync(Login data)
        {
            try
            {
                // user login data
                var register = new Register
                {
                    UserName = data.UserName,
                    Password = data.Password,
                };

                // returns the result
                return await MakePostRequest(data: register, url: "user/login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new AuthResult
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }

        /// <summary>
        /// Stores the forgot password token for a user.
        /// </summary>
        /// <param name="data">The data containing the forgot password information.</param>
        /// <returns>
        /// An <see cref="AuthResult"/> result.
        /// </returns>
        public async Task<AuthResult> ForgotPasswordAsync(ForgotPassword data)
        {
            try
            {
                // forgot password data
                var forgot = new ForgotPassword
                {
                    Email = data.Email
                };

                // returns the result
                return await MakePostRequest(data: forgot, url: "user/forgot-password");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new AuthResult
                {
                    Success = false,
                    Message = "Internal error"
                };
            }
        }

        /// <summary>
        /// Resets the user's password.
        /// </summary>
        /// <param name="data">The data containing the reset password information.</param>
        /// <returns>
        /// A <see cref="AuthResult"/> indicating the outcome of the password reset.
        /// </returns>
        public async Task<AuthResult> ResetPasswordAsync(ResetPassword data)
        {
            try
            {
                // password reset data
                var reset = new ResetPassword
                {
                    Password    = data.Password,
                    ConPassword = data.ConPassword,
                    ResetToken  = data.ResetToken
                };

                // returns the result
                return await MakePostRequest(reset, "user/reset-password");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new AuthResult
                {
                    Success = false,
                    Message = "Internal error"
                };
            }
        }

        /// <summary>
        /// Gets the new refresh token.
        /// </summary>
        /// <param name="data">The data containing the token information.</param>
        /// <returns>
        /// A <see cref="AuthResult"/> indicating the outcome of the token refresh.
        /// </returns>
        public async Task<AuthResult> RefreshTokenAsync(RefreshToken data)
        {
            try
            {
                // token refresh data
                var refresh = new RefreshToken
                {
                    TokenRefresh=data.TokenRefresh
                };

                // returns the result
                return await MakePostRequest(refresh, "user/refresh-token");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new AuthResult
                {
                    Success = false,
                    Message = "Internal error"
                };
            }
        }

        // Helpers ----------------------------------------------------------

        /// <summary>
        /// Makes post request to the endpoint.
        /// </summary>
        /// <param name="data">The data to be sent.</param>
        /// <param name="url">The endpoint url.</param>
        /// <returns>
        ///  A <see cref="AuthResult"/> indicating the outcome of the operation.
        /// </returns>
        private async Task<AuthResult> MakePostRequest<T>(T data, string url)
        {
            try
            {
                // serializing
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                // calls the api
                using var response = await _client.Request.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    // reads the response
                    string result = await response.Content.ReadAsStringAsync();

                    // deserializing
                    AuthResult? authResult = JsonConvert.DeserializeObject<AuthResult>(result);

                    // returns the result
                    return authResult ?? new AuthResult();
                }
                else
                {
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Request not succeeded."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new AuthResult
                {
                    Success = false,
                    Message = "Internal error."
                };
            }
        }
    }
}