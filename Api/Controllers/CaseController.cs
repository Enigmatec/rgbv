using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers;
using Service.Interfaces;
using Service.Models;
using Service.StringKeys;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Api.Controllers
{
    public class CaseController : BaseController
    {
        private readonly ICaseService _caseService;

        public CaseController(ICaseService caseService)
        {
            _caseService = caseService;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateCategory([FromBody] CaseCategoryCreationModel model)
        {
            return ApiResult(await _caseService.CreateCategory(model));
        }

        [HttpGet("[action]/{Id}")]
        public async Task<IActionResult> Category([FromRoute] int Id)
        {
            return ApiResult(await _caseService.GetCategory(Id));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Categories()
        {
            return ApiResult(await _caseService.GetCategories());
        }

        [HttpPut("[action]/{Id}")]
        [Authorize(Roles = RoleKeys.Administrator)]
        public async Task<IActionResult> UpdateCategory([FromRoute, ValueCannotBeZero] int Id, [FromBody] CaseCategoryCreationModel model)
        {
            return ApiResult(await _caseService.UpdateCategory(Id, model));
        }


        [HttpPost("[action]")]
        [Authorize(Policy = PolicyKeys.CSOandSP)]
        public async Task<IActionResult> AddCaseServiceProvider([FromBody] CaseCreationSPModel model)
        {
            return ApiResult(await _caseService.AddCaseBySP(model));
        }

        //[HttpPost("[action]")]
        //[Authorize(Policy = PolicyKeys.CSOandSP)]
        //public async Task<IActionResult> BulkUpload(IFormFile model)
        //{
        //    return ApiResult(await _caseService.BulkUpload(model));
        //}

        [HttpPost("[action]")]
        [Authorize(Policy = PolicyKeys.CSOandSP)]
        public async Task<IActionResult> UploadCases([FromBody] List<CaseCreationSPModel> model)
        {
            return ApiResult(await _caseService.UploadCases(model));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> GetAll([FromBody] CaseSearchModel model)
        {

            var result = await _caseService.GetAllCases(model);

            return ApiResult(result);
        }

        //[AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> GetCaseWithIncidentCode([FromBody] CaseIncidentCode model)
        {

            var result = await _caseService.GetCaseWithIncidentCode(model);

            return ApiResult(result);
        }



        [HttpPut("[action]/{Id}")]
        [Authorize(Policy = PolicyKeys.CSOandSP)]
        public async Task<IActionResult> UpdateCaseBySP([FromRoute, ValueCannotBeZero] int Id, CaseCreationSPModel model)
        {
            return ApiResult(await _caseService.UpdateCaseBySP(Id, model));
        }

        [HttpDelete("[action]/{Id}")]
        [Authorize(Policy = PolicyKeys.CSOandSP)]
        public async Task<IActionResult> Delete([FromRoute, ValueCannotBeZero] int Id)
        {
            return ApiResult(await _caseService.Delete(Id));
        }

        [HttpPut("[action]/{Id}")]
        //[Authorize(Roles = RoleKeys.StateSupervisor)]
        [Authorize(Roles = RoleKeys.StateSupervisor + "," + RoleKeys.StateAdministrator + "," + RoleKeys.Administrator  + "," + RoleKeys.FederalSupervisor)]
        public async Task<IActionResult> ApproveOrDisapprovedCase([FromRoute, ValueCannotBeZero] int Id, bool IsApproved, string note = null)
        {
            return ApiResult(await _caseService.ApproveOrDisapprove(Id, IsApproved, note));
        }

        [HttpPut("[action]")]
        //[Authorize(Roles = RoleKeys.StateSupervisor)]
        [Authorize(Roles = RoleKeys.StateSupervisor + "," + RoleKeys.StateAdministrator + "," + RoleKeys.Administrator + RoleKeys.FederalSupervisor)]
        public async Task<IActionResult> ApproveOrDisapproveAll([FromBody] ApproveCase Cases)
        {
            return ApiResult(await _caseService.ApproveOrDisapproveAll(Cases));
        }

        [HttpPut("[action]/{Id}")]
        [Authorize(Policy = PolicyKeys.OrganisationSupervisor)]
        public async Task<IActionResult> ValidateCase([FromRoute, ValueCannotBeZero] int Id, bool IsValidated, bool? IsLga, string note = null)
        {
            return ApiResult(await _caseService.Validate(Id, IsValidated, IsLga, note));
        }

        [HttpPut("[action]")]
        [Authorize(Policy = PolicyKeys.OrganisationSupervisor)]
        public async Task<IActionResult> ValidateAll([FromBody] ApproveCase Cases)
        {
            return ApiResult(await _caseService.ValidateAll(Cases));
        }

        [HttpPost("{CaseId}/[action]")]
        [Authorize(Policy = PolicyKeys.CSOandSP)]
        public async Task<IActionResult> AddFollowUpAction([FromRoute, ValueCannotBeZero] int CaseId, [FromBody] FollowUpCreationModel model)
        {
            return ApiResult(await _caseService.AddFollowUpAction(CaseId, model));
        }

        [HttpDelete("{CaseId}/[action]/{Id}")]
        [Authorize(Policy = PolicyKeys.CSOandSP)]
        public async Task<IActionResult> DeleteFollowUpAction([FromRoute, ValueCannotBeZero] int CaseId, [FromRoute, ValueCannotBeZero] int Id)
        {
            return ApiResult(await _caseService.DeleteFollowUpAction(CaseId, Id));
        }

        [HttpPut("{CaseId}/[action]/{Id}")]
        [Authorize(Policy = PolicyKeys.CSOandSP)]
        public async Task<IActionResult> UpdateFollowUpAction([FromRoute, ValueCannotBeZero] int CaseId, [FromRoute, ValueCannotBeZero] int Id, FollowUpCreationModel model)
        {
            return ApiResult(await _caseService.UpdateFollowUpAction(CaseId, Id, model));
        }

        [HttpGet("{CaseId}/[action]")]
        public async Task<IActionResult> GetFollowUpActions([FromRoute, ValueCannotBeZero] int CaseId)
        {
            return ApiResult(await _caseService.GetFollowUpActions(CaseId));
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> GetAllEntries()
        {
            return ApiResult(await _caseService.GetEntries());
        }
    }
}