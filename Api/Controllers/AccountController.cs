using Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models;
using Service.StringKeys;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;

namespace Api.Controllers
{
    [AllowAnonymous]
    public class AccountController : BaseController
    {
        private readonly IAuthentication _authenticate;
        private readonly IConfiguration _configuration;
        

        public AccountController(IAuthentication authentication, IConfiguration configuration)
        {
            _authenticate = authentication;
            _configuration = configuration;
            
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            return ApiResult(await _authenticate.Login(model));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> ResendConfirmationMail(string Email)
        {
            return ApiResult(await _authenticate.ResendConfirmationMail(Email));
        }

        [HttpGet("[action]/{Id}")]
        public async Task<IActionResult> ConfirmEmail([FromRoute] string Id, string Code)
        {
            return ApiResult(await _authenticate.ConfirmEmail(Id, Code));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            return ApiResult(await _authenticate.ForgotPassword(model));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            return ApiResult(await _authenticate.ResetPassword(model.Id, model.Code, model.Password));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> SignOut()
        {
            return ApiResult(await _authenticate.Logout());
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> UdateCase()
        {
            await _authenticate.UpdateCases();

            return Ok();
        }

        [HttpGet("[action]")]
        public IActionResult getEnvironment()
        {
            if (!_configuration.GetValue<bool>(StartupKeys.IsLive))
            {
                return Ok("Development");
            }
            else
            {
                return Ok("Production");
            }
        }
    }
}