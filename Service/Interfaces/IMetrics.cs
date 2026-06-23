using Service.Enums;
using Service.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IMetrics
    {
        Task<AppResult<MemoryStream>> ExportCaseDataInExcel(CaseSearchModel model, bool Isfulltext);

        Task<AppResult<MemoryStream>> ExportCodeDictionaryInExcel();

        Task<AppResult<MemoryStream>> ExportDataInSPSS(CaseSearchModel model);

        Task<AppResult<MemoryStream>> ExportMonthlyReport(DateModel model);

        Task<AppResult<HomeMetricsModel>> HomePageMetrics(SearchModel model);

        Task<AppResult<AdminDashBoardModel>> AdminDashBoard(SearchModel model, DashboardKeys dashboard);

        Task<AppResult<MemoryStream>> GetOrganisationsExcelSheet();

        Task<AppResult<MemoryStream>> ExportMonthlyReportNew(DateModel model, bool isCase);

        Task<AppResult<List<CaseBySubject>>> CasesByStateByLga(CaseByStateAndLgaFilter request);

        Task<AppResult<List<CaseBySubject>>> MonthlyCasesByWards(MonthlyCasesByWardsFilter request);

        Task<AppResult<List<CaseBySubject>>> ServicesByTypeOfServiceProvided(CaseByStateAndLgaFilter request);

        Task<AppResult<List<CaseBySubject>>> ServicesByStates();

        Task<AppResult<List<CaseBySubject>>> CsoCountByStates();

        Task<AppResult<List<CaseBySubject>>> ServicesByOrg(DataByIds request);
        Task<AppResult<List<CaseBySubject>>> MonthlyServiceByStates(MonthlyCasesByWardsFilter request);
        Task<AppResult<string>> ExportCaseDataInExcelBackground(CaseSearchModel model, bool IsfullText);
        Task<AppResult<string>> Cron();
        Task<AppResult<string>> UpdateDb();
    }
}