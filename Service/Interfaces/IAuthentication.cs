using Service.Models;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IAuthentication
    {
        Task<AppResult<string>> ConfirmEmail(string UserId, string Code);

        Task<AppResult<string>> ForgotPassword(ForgotPasswordModel model);

        Task<AppResult<LoginResponse>> Login(LoginModel model);

        Task<AppResult<string>> Logout();

        Task<AppResult<string>> ResendConfirmationMail(string Email);

        Task<AppResult<string>> ResetPassword(string Id, string Code, string Password);
        Task UpdateCases();
    }
}