using ConsoleUI.Auth;
using ConsoleUI.Response;
using System.Threading.Tasks;

namespace ConsoleUI.Interfaces
{
    public interface IAuthService
    {

        // user registration
        Task<AuthResult> UserRegisterAsync(Register data);

        // user login
        Task<AuthResult> UserLoginAsync(Login data);

        // forgot password
        Task<AuthResult> ForgotPasswordAsync(ForgotPassword data);

        // reset password
        Task<AuthResult> ResetPasswordAsync(ResetPassword data);

        // refresh token
        Task<AuthResult> RefreshTokenAsync(RefreshToken data);
    }
}
