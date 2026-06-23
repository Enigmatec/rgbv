using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models;
using Service.Models.ViewModels;
using System.Threading.Tasks;

namespace Api.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _dashboardService;
        private readonly IMetrics _metrics;

        public DashboardController(IDashboardService dashboardService, IMetrics metrics)
        {
            _dashboardService = dashboardService;
            _metrics = metrics;
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        [ProducesResponseType(typeof(ApiResult<StateHomeVM>), 200)]
        public async Task<IActionResult> GetAllIncidentByState([FromQuery] SearchModel model)
        {
            return ApiResult(await _dashboardService.ByState(DashboardDetails.Incident, model));
        }
        [AllowAnonymous]
        [HttpPost("[action]")]
        public IActionResult CleareDashboardCache()
        {
            return ApiResult(_dashboardService.ClearDashboardCache());
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        [ProducesResponseType(typeof(ApiResult<CaseBySubject>), 200)]
        public async Task<IActionResult> CasesByStateByLga([FromQuery] StateAndLgaFilter model)
        {
            return ApiResult(await _dashboardService.IncidentCasesByStateByLga(model));
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        [ProducesResponseType(typeof(ApiResult<StateHomeVM>), 200)]
        public async Task<IActionResult> CasesByStateByLgaByWard([FromQuery] StateAndLgaFilter model)
        {
            return ApiResult(await _dashboardService.IncidentCasesByStateByLgaByWard(model));
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        [ProducesResponseType(typeof(ApiResult<StateHomeVM>), 200)]
        public async Task<IActionResult> ServicesByState([FromQuery] SearchModel model)
        {
            return ApiResult(await _dashboardService.ByState(DashboardDetails.Services, model));
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(ApiResult<ServicesDashboard>), 200)]
        public async Task<IActionResult> ServicesDashboard([FromQuery] SearchModel model)
        {
            return ApiResult(await _dashboardService.GetServicesDashboard(model));
        }
        //[AllowAnonymous]
        //[HttpGet("[action]")]
        //[ProducesResponseType(typeof(ApiResult<StateHomeVM>), 200)]
        //public async Task<IActionResult> ServicesByStateByLga([FromQuery] StateAndLgaFilter model)
        //{
        //    return ApiResult(await _dashboardService.ServicesByStateByLga(model));
        //}
        //[AllowAnonymous]
        //[HttpGet("[action]")]
        //[ProducesResponseType(typeof(ApiResult<StateHomeVM>), 200)]
        //public async Task<IActionResult> ServicesByStateByLgaByWard([FromQuery] StateAndLgaFilter model)
        //{
        //    return ApiResult(await _dashboardService.ServicesByStateByLgaByWard(model));
        //}

        //[AllowAnonymous]
        //[HttpGet("[action]")]
        //[ProducesResponseType(typeof(ApiResult<CSOProviders>), 200)]
        //public async Task<IActionResult> CsoProvidersByState()
        //{
        //    return ApiResult(await _dashboardService.CsoProvidersByState());
        //}

        //[AllowAnonymous]
        //[HttpGet("[action]")]
        //[ProducesResponseType(typeof(ApiResult<TotalNumberofCSOSP>), 200)]
        //public async Task<IActionResult> CsoProvidersByStateByLga([FromQuery] StateAndLgaFilter model)
        //{
        //    return ApiResult(await _dashboardService.CsoProvidersByStateByLga(model));
        //}
        //[AllowAnonymous]
        //[HttpGet("[action]")]
        //[ProducesResponseType(typeof(ApiResult<CSOSPVM>), 200)]
        //public async Task<IActionResult> CsoProvidersByStateByLgaByWard([FromQuery] StateAndLgaFilter model)
        //{
        //    return ApiResult(await _dashboardService.CsoProvidersByStateByLgaByWard(model));
        //}

    }
}
