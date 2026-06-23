using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Service.Interfaces;
using Service.Models;
using Service.StringKeys;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Api.Controllers
{
    public class ServicesProvidedController : BaseController
    {
        private readonly IServicesProvidedService _servicesProvidedService;

        public ServicesProvidedController(IServicesProvidedService servicesProvidedService)
        {
            _servicesProvidedService = servicesProvidedService;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateServiceProvided(CreateServiceProvidedRequest request)
            => Response(await _servicesProvidedService.CreateServiceProvided(request));

        [HttpPost("[action]")]
        public async Task<IActionResult> GetAllServiceProvided(GetAllServiceProvidedRequest request)
            => Response(await _servicesProvidedService.GetAllServiceProvided(request));

        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateServiceProvided(UpdateServiceProvidedRequest request)
            => Response(await _servicesProvidedService.UpdateServiceProvided(request));

        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> DeleteServiceProvided(int id)
            => Response(await _servicesProvidedService.DeleteServiceProvided(id));

        [HttpPut("[action]/{id}/{undoApproval:bool}")]
        public async Task<IActionResult> ApproveServiceProvided(int id, bool undoApproval)
            => Response(await _servicesProvidedService.ApproveServiceProvided(id, undoApproval,false));

        [HttpPut("[action]/{id}/{undoApproval:bool}")]
        public async Task<IActionResult> RejectServiceProvided(int id, bool undoApproval)
          => Response(await _servicesProvidedService.ApproveServiceProvided(id, undoApproval,true));

        //[HttpPost("[action]")]
        //[Authorize(Policy = PolicyKeys.CSOandSP)]
        //public async Task<IActionResult> BulkUpload(IFormFile model)
        //{
        //    return ApiResult(await _servicesProvidedService.BulkUpload(model));
        //}


        [HttpGet("[action]")]
        public async Task<IActionResult> ExportServices([FromQuery] ServiceProvidedRequest model)
        {
            return ApiResult(await _servicesProvidedService.ExportServiceDataInExcelBackground(model));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DirectExportServices(ServiceProvidedRequest model)
        {
            return ApiResult(await _servicesProvidedService.DirectExportServicesInExcel(model));
        }
    }
}