using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models;
using Service.StringKeys;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Api.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        private readonly IEmailService _email;

        public UserController(IUserService userService, IEmailService email)
        {
            _userService = userService;
            _email = email;
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<IActionResult> Roles()
        {
            return ApiResult(await _userService.GetRoles());
        }

        [Authorize(Roles = "Administrator,Federal Supervisor")]
        [HttpPost("[action]")]
        public async Task<IActionResult> Create(UserCreationModel model)
        {
            return ApiResult(await _userService.CreateUser(model));
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> Get([FromRoute] string Id)
        {
            return ApiResult(await _userService.GetUser(Id));
        }

        [HttpPost("[action]")]
        [Authorize(Roles = RoleKeys.Administrator)]
        public async Task<IActionResult> GetAll([FromBody] UserSearchModel model)
        {
            return ApiResult(await _userService.GetUsers(model));
        }

        [HttpPut("[action]/{Id}")]
        [Authorize(Roles = RoleKeys.Administrator)]
        public async Task<IActionResult> Update([FromRoute] string Id, [FromBody] UserCreationModel model)
        {
            return ApiResult(await _userService.UpdateUser(Id, model));
        }

        [Authorize(Roles = RoleKeys.Administrator)]
        [HttpGet("[action]/{Id}")]
        public async Task<IActionResult> ActivateOrDeactivateUser([FromRoute] string Id)
        {
            return ApiResult(await _userService.ActivateOrDeactivateUser(Id));
        }

        [Authorize(Roles = RoleKeys.Administrator)]
        [HttpDelete("[action]/{Id}")]
        public async Task<IActionResult> Delete([FromRoute] string Id)
        {
            return ApiResult(await _userService.SoftDelete(Id));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Profile()
        {
            return ApiResult(await _userService.GetProfile());
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateModel model)
        {
            return ApiResult(await _userService.UpdateProfile(model));
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            return ApiResult(await _userService.ChangePassword(model));
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> Okra()
        {
            try
            {
                //gets the webhook response from flutterwave when a transaction has occured.

                var stringa = await new StreamReader(Request.Body).ReadToEndAsync();

                var template2 = new TemplateModel<string>(stringa).Add("Title", "CallBack");

                await _email.TemplateSendAsync("emekafrancis006@gmail.com", "Okra CallBack", "Mail", template2);
            }
            catch (Exception e)
            {
                string message = "error occured" + e.Message;
                var template = new TemplateModel<string>(message).Add("Title", "CallBack error");
                await _email.TemplateSendAsync("emekafrancis006@gmail.com", "Okra CallBack error", "Mail", template);
            }
            return Ok();
        }
    }
}