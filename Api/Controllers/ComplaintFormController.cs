using Microsoft.AspNetCore.Mvc;
using Service.Implementations;
using Service.Models;
using System.Threading.Tasks;

namespace Api.Controllers
{
    public class ComplaintFormController : BaseController
    {
        private readonly ComplaintFormService _complaintFormService;

        public ComplaintFormController(ComplaintFormService complaintFormService)
        {
            _complaintFormService = complaintFormService;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateComplaintForm([FromForm] CreateComplaintFormDto model)
        {
            return Result(await _complaintFormService.CreateComplaintForm(model));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> GetAll([FromBody] GetAllCompaintFormsRequest request)
        {
            return Result(await _complaintFormService.GetAll(request));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetById(int id)
        {
            return Result(await _complaintFormService.GetById(id));
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> ChangeFormStatus([FromBody] ChangeComplaintFormStatusRequest model)
        {
            return Result(await _complaintFormService.ChangeFormStatus(model));
        }
    }
}