using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Service.Helpers;
using Service.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IServicesProvidedService
    {
        Task<Result<string>> CreateServiceProvided(CreateServiceProvidedRequest request);

        Task<Result<PaginatedList<ServiceProvidedDto>>> GetAllServiceProvided(GetAllServiceProvidedRequest request);

        Task<Result> UpdateServiceProvided(UpdateServiceProvidedRequest request);

        Task<Result> DeleteServiceProvided(int id);

        Task<Result> ApproveServiceProvided(int serviceProvidedId, bool undoApproval, bool isReject);
        Task<AppResult<string>> BulkUpload(IFormFile file);
        Task<AppResult<string>> ExportServiceDataInExcelBackground(ServiceProvidedRequest model);
        Task<AppResult<MemoryStream>> DirectExportServicesInExcel(ServiceProvidedRequest model);
    }
}