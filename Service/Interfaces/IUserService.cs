using Core.Entities;
using Service.Helpers;
using Service.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IUserService
    {
        Task<AppResult<string>> ActivateOrDeactivateUser(string Id);

        Task<AppResult<string>> CreateUser(UserCreationModel model);

        Task<AppResult<List<RolesListModel>>> GetRoles();

        Task<AppResult<UserViewModel>> GetUser(string Id);

        Task<AppResult<PaginatedList<UserViewModel>>> GetUsers(UserSearchModel model);

        Task<AppResult<string>> UpdateUser(string Id, UserCreationModel model);

        Task<AppResult<string>> UpdateProfile(UserProfileUpdateModel model);

        Task<AppResult<string>> ChangePassword(ChangePasswordModel model);

        Task<AppResult<string>> SoftDelete(string Id);

        Task<AppResult<UserProfileViewModel>> GetProfile();

        Task<AppResult<string>> ResetUser();
        Task<ApplicationUser> GetUserByEmail(string email);
        Task<ApplicationUser> GetUserById(string userId);
    }
}