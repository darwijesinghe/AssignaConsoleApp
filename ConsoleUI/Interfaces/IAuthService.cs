using ConsoleUI.Auth;
using ConsoleUI.Response;
using System.Threading.Tasks;

namespace ConsoleUI.Interfaces
{
    /// <summary>
    /// Interface for user related operations
    /// </summary>
    public interface IAuthService
    {

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="data">The user registration data.</param>
        /// <returns>
        /// An <see cref="AuthResult"/> result.
        /// </returns>
        Task<AuthResult> UserRegisterAsync(Register data);

        /// <summary>
        /// Logins a new user.
        /// </summary>
        /// <param name="data">The user login data.</param>
        /// <returns>
        /// An <see cref="AuthResult"/> result.
        /// </returns>
        Task<AuthResult> UserLoginAsync(Login data);

        /// <summary>
        /// Stores the forgot password token for a user.
        /// </summary>
        /// <param name="data">The data containing the forgot password information.</param>
        /// <returns>
        /// An <see cref="AuthResult"/> result.
        /// </returns>
        Task<AuthResult> ForgotPasswordAsync(ForgotPassword data);

        /// <summary>
        /// Resets the user's password.
        /// </summary>
        /// <param name="data">The data containing the reset password information.</param>
        /// <returns>
        /// A <see cref="AuthResult"/> indicating the outcome of the password reset.
        /// </returns>
        Task<AuthResult> ResetPasswordAsync(ResetPassword data);

        /// <summary>
        /// Gets the new refresh token.
        /// </summary>
        /// <param name="data">The data containing the token information.</param>
        /// <returns>
        /// A <see cref="AuthResult"/> indicating the outcome of the token refresh.
        /// </returns>
        Task<AuthResult> RefreshTokenAsync(RefreshToken data);
    }
}
