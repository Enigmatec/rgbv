using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Enums;
using Service.Interfaces;
using Service.Models;
using Service.StringKeys;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Api.Controllers
{
    public class MetricsController : BaseController
    {
        private readonly IMetrics _metrics;

        public MetricsController(IMetrics metrics)
        {
            _metrics = metrics;
        }

        [Authorize(Policy = PolicyKeys.CSOandSP)]
        [HttpPost("[action]")]
        [ProducesResponseType(typeof(ApiResult<AdminDashBoardModel>), 200)]
        public async Task<IActionResult> CSOandSPDashBoard([FromBody] SearchModel model)
        {
            return ApiResult(await _metrics.AdminDashBoard(model, DashboardKeys.SPandCSODashboard));
        }


        [Authorize(Policy = PolicyKeys.AllSuperVisors)]
        [HttpPost("[action]")]
        [ProducesResponseType(typeof(ApiResult<AdminDashBoardModel>), 200)]
        public async Task<IActionResult> SuperVisorDashBoard([FromBody] SearchModel model)
        {
            return ApiResult(await _metrics.AdminDashBoard(model, DashboardKeys.SuperVisorDashBoard));
        }

        [Authorize(Policy = RoleKeys.Donor)]
        [HttpPost("[action]")]
        [ProducesResponseType(typeof(ApiResult<AdminDashBoardModel>), 200)]
        public async Task<IActionResult> DonorDashBoard([FromBody] SearchModel model)
        {
            return ApiResult(await _metrics.AdminDashBoard(model, DashboardKeys.DonorDashBoard));
        }

        //[AllowAnonymous]
        [Authorize(Roles = RoleKeys.Administrator)]
        [HttpPost("[action]")]
        [ProducesResponseType(typeof(ApiResult<AdminDashBoardModel>), 200)]
        public async Task<IActionResult> AdminDashBoard([FromBody] SearchModel model)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var result = await _metrics.AdminDashBoard(model, DashboardKeys.AdminDashBoard);

            stopWatch.Stop();
            var time = stopWatch.ElapsedMilliseconds / 1000;

            return ApiResult(result);
        }

        [Authorize(Roles = RoleKeys.Administrator)]
        [HttpGet("[action]")]
        public async Task<IActionResult> TestAdminDashBoard([FromQuery] SearchModel model)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var result = await _metrics.AdminDashBoard(model, DashboardKeys.AdminDashBoard);

            stopWatch.Stop();
            var time = stopWatch.ElapsedMilliseconds / 1000;

            return ApiResult(result);
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> HompageMetrics([FromBody] SearchModel model)
        {
            return ApiResult(await _metrics.HomePageMetrics(model));
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> MonthlyCasesByWards([FromBody] MonthlyCasesByWardsFilter model)
        {
            return ApiResult(await _metrics.MonthlyCasesByWards(model));
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        [ProducesResponseType(typeof(ApiResult<List<CaseBySubject>>), 200)]
        public async Task<IActionResult> ServicesByTypeOfServiceProvided([FromBody] CaseByStateAndLgaFilter model)
        {
            return ApiResult(await _metrics.ServicesByTypeOfServiceProvided(model));
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> CasesByStateByLga([FromBody] CaseByStateAndLgaFilter model)
        {
            return ApiResult(await _metrics.CasesByStateByLga(model));
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<IActionResult> ServicesByStates()
        {
            return ApiResult(await _metrics.ServicesByStates());
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<IActionResult> CsoCountByStates()
        {
            return ApiResult(await _metrics.CsoCountByStates());
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> ServicesByOrg(DataByIds request)
        {
            return ApiResult(await _metrics.ServicesByOrg(request));
        }


        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> MonthlyServiceByStates(MonthlyCasesByWardsFilter model)
        {
            return ApiResult(await _metrics.MonthlyServiceByStates(model));
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<IActionResult> CodeDictionary()
        {
            return ApiResult(await _metrics.ExportCodeDictionaryInExcel());
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ExportDataInExcel(CaseSearchModel model)
        {
            return ApiResult(await _metrics.ExportCaseDataInExcel(model, false));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ExportDataInExcelFullText(CaseSearchModel model)
        {
            return ApiResult(await _metrics.ExportCaseDataInExcel(model, true));
        }

        //[HttpPost("[action]")]
        //public async Task<IActionResult> ExportDataInExcelFullTextBackground(CaseSearchModel model)
        //{
        //    return ApiResult(await _metrics.ExportCaseDataInExcelBackground(model, true));
        //}

        [HttpPost("[action]")]
        public async Task<IActionResult> ExportDataInExcelBackground(CaseSearchModel model)
        {
            return ApiResult(await _metrics.ExportCaseDataInExcelBackground(model, true));
        }
        //[AllowAnonymous]
        //[HttpPost("[action]")]
        //public async Task<IActionResult> Cr()
        //{
        //    return ApiResult(await _metrics.Cron());
        //}
        //[AllowAnonymous]
        //[HttpPost("[action]")]
        //public async Task<IActionResult> db()
        //{
        //    return ApiResult(await _metrics.UpdateDb());
        //}

        [HttpGet("[action]")]
        public async Task<IActionResult> GetOrganisationsExcel()
        {
            return ApiResult(await _metrics.GetOrganisationsExcelSheet());
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ExportDataInSPSS(CaseSearchModel model)
        {
            return ApiResult(await _metrics.ExportDataInSPSS(model));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ExportMonthlyReport(DateModel model)
        {
            return ApiResult(await _metrics.ExportMonthlyReport(model));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ExportMonthlyReportNew(DateModel model)
        {
            return ApiResult(await _metrics.ExportMonthlyReportNew(model, true));
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> ExportMonthlyReportServicesNew(DateModel model)
        {
            return ApiResult(await _metrics.ExportMonthlyReportNew(model, false));
        }



    }
}