using Microsoft.AspNetCore.Http;
using Service.Helpers;
using Service.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface ICaseService
    {
        //Task<AppResult<string>> AddCaseByCSO(CaseCreationCSOModel model);
        Task<AppResult<string>> AddCaseBySP(CaseCreationSPModel model);

        Task<AppResult<List<string>>> UploadCases(List<CaseCreationSPModel> model);

        Task<AppResult<string>> AddEntry(List<EntryModel> model);

        Task<AppResult<string>> AddFollowUpAction(int Id, FollowUpCreationModel model);

        Task<AppResult<string>> ApproveOrDisapprove(int Id, bool isApproved, string note = null);

        Task<AppResult<string>> ApproveOrDisapproveAll(ApproveCase model);

        Task<AppResult<string>> CreateCategory(CaseCategoryCreationModel model);
        //Task<AppResult<string>> BulkUpload(IFormFile file);

        Task<AppResult<string>> Delete(int Id);

        Task<AppResult<string>> DeleteFollowUpAction(int CaseId, int Id);

        Task<AppResult<PaginatedList<CaseViewModel>>> GetAllCases(CaseSearchModel model);
        Task<AppResult<CaseViewModel>> GetCaseWithIncidentCode(CaseIncidentCode model);

        Task<AppResult<List<CaseCategoryListModel>>> GetCategories();

        Task<AppResult<CaseCategoryViewModel>> GetCategory(int Id);

        Task<AppResult<List<EntryModel>>> GetEntries();

        Task<AppResult<List<FollowUpViewModel>>> GetFollowUpActions(int CaseId);

        //Task<AppResult<string>> UpdateCaseByCSO(int Id, CaseCreationCSOModel model);

        Task<AppResult<string>> UpdateCaseBySP(int Id, CaseCreationSPModel model);

        Task<AppResult<string>> UpdateCategory(int Id, CaseCategoryCreationModel model);

        Task<AppResult<string>> UpdateFollowUpAction(int CaseId, int Id, FollowUpCreationModel model);

        Task<AppResult<string>> Validate(int Id, bool IsValidated, bool? isLga, string note = null);

        Task<AppResult<string>> ValidateAll(ApproveCase model);
    }
}