using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers;
using Service.Implementations;
using Service.Interfaces;
using Service.Models;
using Service.StringKeys;
using System.Threading.Tasks;

namespace Api.Controllers
{
    public class DonorController : BaseController
    {
        private readonly IDonorService _donorService;

        public DonorController(IDonorService donorService)
        {
            _donorService = donorService;
        }


        [HttpPost("[action]")]
        [ProducesResponseType(typeof(AppResult<string>), 200)]
        [Authorize(Roles = RoleKeys.Administrator)]
        public async Task<IActionResult> CreateDonor([FromBody] CreateDonorVM model)
        {
            return ApiResult(await _donorService.CreateDonor(model));
        }

        [HttpPut("[action]")]
        [ProducesResponseType(typeof(AppResult<string>), 200)]
        [Authorize(Roles = RoleKeys.Administrator)]
        public async Task<IActionResult> EditDonor([FromBody] EditDonorVM model)
        {
            return ApiResult(await _donorService.EditDonor(model));
        }


        [HttpPost("[action]")]
        [ProducesResponseType(typeof(AppResult<string>), 200)]
        [Authorize(Roles = RoleKeys.Administrator)]
        //[AllowAnonymous]
        public async Task<IActionResult> AddUserToDonor([FromBody] AddUserToDonor model)
        {
            return ApiResult(await _donorService.AddUser(model));
        }

        [HttpPut("[action]")]
        [ProducesResponseType(typeof(AppResult<string>), 200)]
        [Authorize(Roles = RoleKeys.Administrator)]
        public async Task<IActionResult> UpdateUser([FromBody] EditUserDonor model)
        {
            return ApiResult(await _donorService.UpdateUser(model));
        }


        [HttpPost("[action]")]
        [Authorize(Roles = RoleKeys.Administrator)]
        [ProducesResponseType(typeof(AppResult<PaginatedList<DonorVM>>), 200)]
        public async Task<IActionResult> GetAllDonor([FromBody] DonoSearchModel model)
        {
            return ApiResult(await _donorService.GetAllDonors(model));
        }

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(AppResult<PaginatedList<DonorInformationVM>>), 200)]
        public async Task<IActionResult> GetScreenPaginated([FromBody] DonoSearchModel model)
        {
            return ApiResult(await _donorService.GetScreenPaginated(model, null));
        }

        [Authorize(Roles = RoleKeys.Administrator)]
        [HttpGet("[action]")]
        [ProducesResponseType(typeof(AppResult<PaginatedList<DonorInformationVM>>), 200)]
        public async Task<IActionResult> AdminGetScreenPaginated([FromQuery] DonoSearchModel model, int? DonorId)
        {
            return ApiResult(await _donorService.GetScreenPaginated(model, DonorId));
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(AppResult<DonorUsersScreenVM>), 200)]
        public async Task<IActionResult> GetScreenMetric()
        {
            return ApiResult(await _donorService.GetScreenMetric(null));
        }

        [Authorize(Roles = RoleKeys.Administrator)]
        [HttpGet("[action]")]
        [ProducesResponseType(typeof(AppResult<DonorUsersScreenVM>), 200)]
        public async Task<IActionResult> AdminGetScreenMetric([FromQuery] int? DonorId)
        {
            return ApiResult(await _donorService.GetScreenMetric(DonorId));
        }
    }
}
