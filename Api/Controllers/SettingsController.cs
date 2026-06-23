using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models;
using Service.StringKeys;
using System.Threading.Tasks;

namespace Api.Controllers
{
    public class SettingsController : BaseController
    {
        private readonly ISetting _setting;

        public SettingsController(ISetting setting)
        {
            _setting = setting;
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> Save([FromBody] SettingsViewModel model)
        {
            return ApiResult(await _setting.Save(model));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> FormIntialisation()
        {
            return ApiResult(await _setting.GetSettings());
        }

        [Authorize(Roles = RoleKeys.Administrator)]
        [HttpPost("[action]")]
        public async Task<IActionResult> UploadResource([FromForm] FileUploadRequest request)
            => Result(await _setting.UploadFile(request, FileUploadCategory.Resource));

        [Authorize(Roles = RoleKeys.Administrator)]
        [HttpPost("[action]")]
        public async Task<IActionResult> UploadTemplate([FromForm] FileUploadRequest request)
            => Result(await _setting.UploadFile(request, FileUploadCategory.Template));

        [AllowAnonymous]
        [HttpPut("[action]")]
        public async Task<IActionResult> ChangeFileInfo([FromBody] UpdateFileInfoRequest request)
            => Result(await _setting.ChangeFileInfo(request));

        [AllowAnonymous]
        [HttpGet("[action]/{pageIndex}/{pageSize}/{category}")]
        public async Task<IActionResult> GetAllFiles(int pageIndex, int pageSize, FileUploadCategory? category)
            => Result(await _setting.GetAllFiles(category, pageIndex, pageSize));

        [AllowAnonymous]
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetFileById(int id)
            => Result(await _setting.GetFileById(id));

        [AllowAnonymous]
        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> DeleteFile(int id)
            => Result(await _setting.DeleteFile(id));

        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllStates()
        {
            return ApiResult(await _setting.GetAllStates());
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAState([FromQuery] int id)
        {
            return ApiResult(await _setting.GetAState(id));
        }
    }
}