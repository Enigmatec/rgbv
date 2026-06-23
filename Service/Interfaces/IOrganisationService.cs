using Service.Helpers;
using Service.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IOrganisationService
    {
        // Task<Result<string>> AddWard(IFormFile file);

        //Task<Result<string>> AddStates();
        //Task<Result<List<StateListModel>>> AddStates(List<StateListModel> model);
        Task<AppResult<string>> Create(OrganisationCreationModel model);

        Task<AppResult<OrganisationViewModel>> Get(int Id);

        Task<AppResult<PaginatedList<OrganisationViewModel>>> GetAll(OrganisationSearchModel model);

        Task<AppResult<List<OrganisationListModel>>> GetList();

        Task<AppResult<List<StateListModel>>> GetStateAndLGAList();

        Task<AppResult<string>> SoftDelete(int Id);

        Task<AppResult<string>> Update(int Id, OrganisationCreationModel model);

        Task<AppResult<OrganisationViewModel>> GetWithUsers(int id);
    }
}