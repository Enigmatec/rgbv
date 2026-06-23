using Service.Helpers;
using Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IDonorService
    {
        Task<AppResult<string>> CreateDonor(CreateDonorVM model);
        Task<AppResult<string>> AddUser(AddUserToDonor model);
        Task<AppResult<PaginatedList<DonorVM>>> GetAllDonors(DonoSearchModel model);
        Task<AppResult<string>> EditDonor(EditDonorVM model);
        Task<AppResult<string>> UpdateUser(EditUserDonor model);
        Task<AppResult<DonorUsersScreenVM>> GetScreenMetric(int? donorId);
        Task<AppResult<PaginatedList<DonorInformationVM>>> GetScreenPaginated(DonoSearchModel model, int? donorId);
    }
}
