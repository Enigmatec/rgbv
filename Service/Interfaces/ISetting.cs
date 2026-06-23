using Core.Entities;
using CSharpFunctionalExtensions;
using Service.Helpers;
using Service.Models;
using Service.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface ISetting
    {
        Task<AppResult<SettingsViewModel>> GetSettings();

        Task<AppResult<string>> Save(SettingsViewModel model);

        Task<Result<int>> UploadFile(FileUploadRequest request, FileUploadCategory category);

        Task<Result> ChangeFileInfo(UpdateFileInfoRequest request);

        Task<Result<PaginatedList<FileUploadDto>>> GetAllFiles(FileUploadCategory? type, int pageIndex, int pageSize);

        Task<Result<FileUploadDto>> GetFileById(int id);

        Task<Result> DeleteFile(int id);
        Task<AppResult<List<StatesVM>>> GetAllStates();
        Task<AppResult<StatesVM>> GetAState(int id);
    }
}