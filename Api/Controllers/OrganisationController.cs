using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers;
using Service.Interfaces;
using Service.Models;
using Service.StringKeys;
using System.Threading.Tasks;

namespace Api.Controllers
{
    public class OrganisationController : BaseController
    {
        private readonly IOrganisationService _organisationService;

        public OrganisationController(IOrganisationService organisation)
        {
            _organisationService = organisation;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody] OrganisationCreationModel model)
        {
            return ApiResult(await _organisationService.Create(model));
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> Get([FromRoute, ValueCannotBeZero(ErrorMessage = "Id not Valid")] int Id)
        {
            return ApiResult(await _organisationService.Get(Id));
        }

        [HttpGet("[action]/{Id}")]
        public async Task<IActionResult> GetWithUsers([FromRoute, ValueCannotBeZero(ErrorMessage = "Id not Valid")] int Id)
        {
            return ApiResult(await _organisationService.GetWithUsers(Id));
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> GetAll([FromBody] OrganisationSearchModel model)
        {
            return ApiResult(await _organisationService.GetAll(model));
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> GetList()
        {
            return ApiResult(await _organisationService.GetList());
        }

        [HttpPut("[action]/{Id}")]
        public async Task<IActionResult> Update([FromRoute, ValueCannotBeZero(ErrorMessage = "Id not Valid")] int Id, [FromBody] OrganisationCreationModel model)
        {
            return ApiResult(await _organisationService.Update(Id, model));
        }

        [AllowAnonymous]
        [HttpGet("States")]
        public async Task<IActionResult> GetStates()
        {
            return ApiResult(await _organisationService.GetStateAndLGAList());
        }

        [Authorize(Roles = RoleKeys.Administrator)]
        [HttpDelete("[action]/{Id}")]
        public async Task<IActionResult> Delete([FromRoute, ValueCannotBeZero(ErrorMessage = "Id not Valid")] int Id)
        {
            return ApiResult(await _organisationService.SoftDelete(Id));
        }

        //[AllowAnonymous]
        //[HttpPost("[action]")]
        //public async Task<IActionResult> AddWard([FromForm] fa file)
        //{
        //    return ApiResult(await _organisationService.AddWard(file.file));
        //}
    }

    //public class fa
    //{
    //    public IFormFile file { get; set; }
    //}
}