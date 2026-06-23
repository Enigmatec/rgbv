using AutoMapper;
using Core.Data;
using Core.Entities;
using Core.Enums;
using CSharpFunctionalExtensions;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;
using Service.Extensions;
using Service.Helpers;
using Service.Interfaces;
using Service.Models;
using Service.StringKeys;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Service.Implementations
{
    public class ServicesProvidedService : IServicesProvidedService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;

        private readonly IEmailService _emailService;
        private string UserId => _httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private DateTime CurrentDate => DateTime.Now.ToUniversalTime().AddHours(1);

        public ServicesProvidedService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _httpContext = httpContextAccessor.HttpContext;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<Result<string>> CreateServiceProvided(CreateServiceProvidedRequest request)
        {
            var user = await _context.Users
                .AsNoTracking().Where(c => c.Id == UserId)
                .Select(u => new { StateCode = u.State.Code, OrganisationCode = u.Organisation.Code, u.StateId, u.Id })
                .FirstOrDefaultAsync();

            if (user is null) return Result.Failure<string>("User not found");

            var codeResult = await GenerateServiceProvisionCode(user.StateCode, user.OrganisationCode);

            var serviceProvided = new ServiceProvided(
                codeResult.serial,
                codeResult.serviceProvisionCode,
                request.AgeOfSurvivorInYears,
                request.SexOfSurvivorOrVictim,
                request.TypeOfClient,
                request.HasSurvivorReceivedServiceFromAnotherOrganisation,
                request.IncidentCodeFromReferringOrganisation,
                request.TypeOfServiceReceivedAnotherOrganisation,
                request.ReferralCode,
                request.TypeOfServiceNeeded,
                request.TypeOfServiceProvided,
                request.TypeOfServiceReferredFor,
                request.NameOfServiceProviderOrCsoReferredTo,
                request.ReferralOutcome,
                request.OrganisationId,
                user.StateId.Value,
                request.Longitude,
                request.Latitude,
                request.DateOfServiceProvision,
                user.Id,
                request.GbvCovid19Question1,
                request.GbvCovid19Question2,
                request.GbvCovid19Question3,
                request.GbvCovid19Question4,
                request.OrganisationLgaId,
                request.IncidentCode,
                request.ReferralToAnotherCsoOrSPcode);

            await _context.ServicesProvided.AddAsync(serviceProvided);

            var status = await _context.SaveChangesAsync() > 0;

            return status ? Result.Success(serviceProvided.ServiceProvisionCode) : Result.Failure<string>("Failed to save");
        }

        //public async Task<List<ServiceProvidedDto>> GetAllServiceProvidedCSV()
        //{
        //    var user = await _context.Users
        //        .AsNoTracking()
        //             .Select(c => new { c.Id, c.Type, c.OrganisationId, c.State, c.LocalGovernments })
        //             .FirstOrDefaultAsync(c => c.Id == UserId);

        //    if(user is null)
        //    {
        //        return null;
        //    }

        //    var query = _context.ServicesProvided.OrderByDescending(d => d.CreatedAt)
        //        .AsQueryable().AsNoTracking();

        //    //filter by the state of the user
        //    if (user.OrganisationId.HasValue && (user.Type != RoleKeys.LocalGovernment
        //                                         || user.Type != RoleKeys.StateSupervisor
        //                                         || user.Type != RoleKeys.StateAdministrator
        //                                         || user.Type != RoleKeys.FederalSupervisor
        //                                         || user.Type != RoleKeys.Administrator))
        //    {
        //        query = query.Where(c => c.OrganisationId == user.OrganisationId.Value || c.CreatedById == user.Id);
        //    }

        //    if (user.State != null && (user.Type == RoleKeys.LocalGovernment
        //                               || user.Type == RoleKeys.CSO
        //                               || user.Type == RoleKeys.ServiceProvider
        //                               || user.Type == RoleKeys.StateSupervisor
        //                               || user.Type == RoleKeys.StateAdministrator))
        //    {
        //        query = query.Where(c => c.ServiceProvisionCode.ToLower().Contains(user.State.Code.ToLower()) || c.StateId == user.State.Id
        //                                 || c.CreatedById == user.Id);
        //    }

        //    query = FilterServicesProvided(query, null, user.Type);

        //    var result=query.Select(x => new ServiceProvidedDto
        //    {
        //        HasSurvivorReceivedServiceFromAnotherOrganisation = x.HasSurvivorReceivedServiceFromAnotherOrganisation,
        //        TypeOfServiceProvidedList = x.TypeOfServiceProvidedList
        //    }).ToList();

        //    return result;
        //}


        public async Task<Result<PaginatedList<ServiceProvidedDto>>> GetAllServiceProvided(GetAllServiceProvidedRequest request)
        {
            var user = await _context.Users
                .AsNoTracking()
                     .Select(c => new { c.Id, c.Type, c.OrganisationId, c.State, c.LocalGovernments })
                     .FirstOrDefaultAsync(c => c.Id == UserId);

            if (user is null)
            {
                return Result.Failure<PaginatedList<ServiceProvidedDto>>("User not found");
            }

            var query = _context.ServicesProvided.OrderByDescending(d => d.CreatedAt)
                .AsQueryable().AsNoTracking();

            //filter by the state of the user
            if (user.OrganisationId.HasValue && (user.Type != RoleKeys.LocalGovernment
                                                 || user.Type != RoleKeys.StateSupervisor
                                                 || user.Type != RoleKeys.StateAdministrator
                                                 || user.Type != RoleKeys.FederalSupervisor
                                                 || user.Type != RoleKeys.Administrator))
            {
                query = query.Where(c => c.OrganisationId == user.OrganisationId.Value || c.CreatedById == user.Id);
            }

            if (user.State != null && (user.Type == RoleKeys.LocalGovernment
                                       || user.Type == RoleKeys.CSO
                                       || user.Type == RoleKeys.ServiceProvider
                                       || user.Type == RoleKeys.StateSupervisor
                                       || user.Type == RoleKeys.StateAdministrator))
            {
                query = query.Where(c => c.ServiceProvisionCode.ToLower().Contains(user.State.Code.ToLower()) || c.StateId == user.State.Id
                                         || c.CreatedById == user.Id);
            }

            query = FilterServicesProvided(query, request, user.Type);

            var paginatedList = await query.PageProjectAsync<ServiceProvided, ServiceProvidedDto>(_mapper, request.PageIndex, request.PageSize);

            return Result.Success(paginatedList);
        }

        public async Task<AppResult<string>> ExportServiceDataInExcelBackground(ServiceProvidedRequest model)
        {
            BackgroundJob.Enqueue(() => ExportServicesInExcel(model, UserId));

            return new AppResult<string>
            {
                Data = "You will be notified in your email",
                Message = "success"
            };
        }
        public async Task<AppResult<string>> ExportServicesInExcel(ServiceProvidedRequest model, string id)
        {
            var resultModel = new AppResult<string>();
            UserViewModel user = null;
            try
            {
                var AppResult = await GetAllServiceProvidedforCSV(model, id);
                var package = await GetServiceExcelSheetNew(AppResult.Services);
                var stream = package.GetAsByteArray();
                string newFile;
                if (model.StartDate != null || model.EndDate != null)
                    newFile = $"Services Report from {model.StartDate:yyyy-MM-dd-hh-mm}-{model.EndDate:yyyy-MM-dd-hh-mm}.xlsx";
                else
                    newFile = $"Services Report.xlsx";
                user = AppResult.User;
                var result = await _emailService.SendCases(AppResult.User, stream, newFile);
                if (result.Item1 == false)
                {
                    resultModel.AddError("Email was not sent");
                    return resultModel;
                }
                resultModel.Data = "Email sent";
            }
            catch (Exception ex)
            {
                resultModel.AddError($"{ex.Message ?? ex.InnerException.Message}");
                await _emailService.SendErrorToAdmin(user, $"{ex.Message}");
                return resultModel;
            }

            return resultModel;



        }

        public async Task<AppResult<MemoryStream>> DirectExportServicesInExcel(ServiceProvidedRequest model)
        {
            var resultModel = new AppResult<MemoryStream>();
            try
            {
                var AppResult = await GetAllServiceProvidedforCSV(model, UserId);
                var package = await GetServiceExcelSheetNew(AppResult.Services);
                var stream = new MemoryStream(package.GetAsByteArray());
                string newFile;
                if (model.StartDate != null || model.EndDate != null)
                    newFile = $"Services Report from {model.StartDate:yyyy-MM-dd-hh-mm}-{model.EndDate:yyyy-MM-dd-hh-mm}.xlsx";
                else
                    newFile = $"Services Report.xlsx";
                resultModel.Data = stream;
                resultModel.Message = newFile;
            }
            catch (Exception ex)
            {
                resultModel.AddError($"{ex.Message ?? ex.InnerException.Message}");
                return resultModel;
            }

            return resultModel;

        }
        private async Task<ServiceViewBackground> GetAllServiceProvidedforCSV(ServiceProvidedRequest request, string id)
        {
            if (id is null)
            {
                return new ServiceViewBackground();
            }
            var resultModel = new ServiceViewBackground();


            var user = await _context.Users.AsNoTracking()
                       .Where(c => c.Id == id)
                       .Select(c => new UserViewModel
                       {
                           Designation = c.Designation,
                           Role = c.Type,
                           Email = c.Email,
                           FirstName = c.FirstName,
                           LastLogin = c.LastLogin,
                           Id = c.Id,
                           LastName = c.LastName,
                           Organisation = c.OrganisationId.HasValue ? c.Organisation.Name : null,
                           OrganisationId = c.OrganisationId,
                           PhoneNumber = c.PhoneNumber,
                           StateId = c.StateId,
                           State = c.StateId.HasValue ? c.State.Name : null,
                           StateCode = c.StateId.HasValue ? c.State.Code : null,
                           IsActivated = c.IsActivated,
                           LocalGovernments = c.LocalGovernments
                       })
                       .FirstOrDefaultAsync();

            if (user is null)
            {

                return resultModel;
            }

            var query = _context.ServicesProvided.Include(x => x.SpOrCsoApprovalBy).Include(x => x.StateApprovalBy).Where(x => x.Status == ValidationStatus.State).OrderByDescending(d => d.CreatedAt)
                .AsQueryable().AsNoTracking();

            //filter by the state of the user
            if (user.OrganisationId.HasValue && (user.Role != RoleKeys.LocalGovernment
                                                 || user.Role != RoleKeys.StateSupervisor
                                                 || user.Role != RoleKeys.StateAdministrator
                                                 || user.Role != RoleKeys.FederalSupervisor
                                                 || user.Role != RoleKeys.Administrator))
            {
                query = query.Where(c => c.OrganisationId == user.OrganisationId.Value || c.CreatedById == user.Id);
            }

            if (user.State != null && (user.Role == RoleKeys.LocalGovernment
                                       || user.Role == RoleKeys.CSO
                                       || user.Role == RoleKeys.ServiceProvider
                                       || user.Role == RoleKeys.StateSupervisor
                                       || user.Role == RoleKeys.StateAdministrator))
            {
                query = query.Where(c => c.ServiceProvisionCode.ToLower().Contains(user.StateCode.ToLower()) || c.StateId == user.StateId
                                         || c.CreatedById == user.Id);
            }

            query = FilterServicesProvided(query, request, user.Role);
            var LGAS = _context.LocalGovernmentAreas.AsNoTracking();
            //var paginatedList = await query.PageProjectAsync<ServiceProvided, ServiceProvidedDto>(_mapper, request.PageIndex, request.PageSize);
            var result = query.Select(x => new ServiceProvidedDto
            {
                AgeOfSurvivorInYears = x.AgeOfSurvivorInYears,
                CreatedAt = x.CreatedAt,
                CreatedByName = x.CreatedBy.FirstName,
                DateOfServiceProvision = x.DateOfServiceProvision,
                IncidentCode = x.IncidentCode,
                GbvCovid19Question1 = x.GbvCovid19Question1,
                GbvCovid19Question2 = x.GbvCovid19Question2,
                GbvCovid19Question3 = x.GbvCovid19Question3,
                GbvCovid19Question4 = x.GbvCovid19Question4,
                HasSurvivorReceivedServiceFromAnotherOrganisation = x.HasSurvivorReceivedServiceFromAnotherOrganisation,
                IncidentCodeFromReferringOrganisation = x.IncidentCodeFromReferringOrganisation,
                LGA = LGAS.FirstOrDefault(c => c.Id == x.OrganisationLgaId).Name,
                ModifiedAt = x.ModifiedAt,
                NameOfServiceProviderOrCsoReferredTo = x.NameOfServiceProviderOrCsoReferredTo,
                OrganisationName = x.Organisation.Name,
                ReferralCode = x.ReferralCode,
                ReferralOutcome = x.ReferralOutcome,
                ReferralToAnotherCsoOrSPcode = x.ReferralToAnotherCsoOrSPcode,
                SerialNumber = x.SerialNumber,
                ServiceProvisionCode = x.ServiceProvisionCode,
                SexOfSurvivorOrVictim = x.SexOfSurvivorOrVictim,
                SpOrCsoApprovalDate = x.SpOrCsoApprovalDate,
                StateName = x.State.Name,
                StateApprovalDate = x.StateApprovalDate,
                TypeOfServiceReferredForList = x.TypeOfServiceReferredForList,
                TypeOfServiceReceivedAnotherOrganisationList = x.TypeOfServiceReceivedAnotherOrganisationList,
                TypeOfServiceProvidedList = x.TypeOfServiceProvidedList,
                TypeOfServiceNeededList = x.TypeOfServiceNeededList,
                TypeOfClient = x.TypeOfClient,
                Status = x.Status,
                SpOrCsoApprovalById = x.SpOrCsoApprovalById
            }).ToList();
            var listOfServices = new ServiceViewBackground()
            {
                Services = result,
                User = user
            };
            return listOfServices;
        }
        private async Task<ExcelPackage> GetServiceExcelSheet(IEnumerable<ServiceProvidedDto> model)
        {

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            ExcelPackage Excelpackage = new();
            ExcelWorksheet WorkSheet = Excelpackage.Workbook.Worksheets.Add($"Service Report_{DateTime.Now:dd-MMM-yy}");

            var row = 2;
            var headers = new List<string>()
            {
                "SERIAL NUMBER",
                "INCIDENT CODE",
                "SERVICE PROVISION CODE",
                "AGE OF SURVIVOR IN YEARS",
                "SEX OF SURVIVOR OR VICTIM",
                "TYPE OF CLIENT",
                "HAS SURVIVOR RECEIVED SERVICE FROM ANOTHER ORGANISATION",
                "INCIDENT CODE FROM REFERRING ORGANISATION",
                "REFERRAL CODE",
                "TYPE OF SERVICE RECEIVED FROM ANOTHER ORGANISATION",
                "TYPE OF SERVICE NEEDED",
                "TYPE OF SERVICE PROVIDED",
                "TYPE OF SERVICE REFERRED",
                "NAME OF SERVICE PROVIDER OR CSO REFERRED TO",
                "REFERRAL OUTCOME",
                "ORGANIZATION NAME",
                "DATE CREATED",
                "CREATOR",
                "DATE OF SERVICE PROVISION",
                "REFERRAL TO ANOTHER CSO OR SP CODE",
                "SP OR CSO APPROVAL DATE",
                "STATE",
                "STATE APPROVAL DATE",
                "STATUS"
            };

            foreach (var header in headers.Select((item, index) => new { Index = index, Item = item }))
            {
                WorkSheet.Cells[row, header.Index + 1].Value = header.Item;
            }

            WorkSheet.Cells[row, 1, row, headers.Count].Style.Font.Bold = true;

            foreach (var item in model)
            {
                row++;
                //item.SpOrCsoApprovalDate.
                //var spOrCsoApprovalDate = item.SpOrCsoApprovalDate.HasValue ? "" : item.SpOrCsoApprovalDate.Value.ToLongDateString();
                try
                {
                    var dataRow = new List<string>()
                    {
                        item.SerialNumber.ToString(),
                        item.IncidentCode,
                        item.ServiceProvisionCode,
                        item.AgeOfSurvivorInYears.ToString(),
                        item.SexOfSurvivorOrVictim,
                        item.TypeOfClient,
                        item.HasSurvivorReceivedServiceFromAnotherOrganisation.GetDescription(),
                        item.IncidentCodeFromReferringOrganisation,
                        item.ReferralCode,
                        item.TypeOfServiceReceivedAnotherOrganisationList is null ? "" : string.Join(",", item.TypeOfServiceReceivedAnotherOrganisationList.Where(t => KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))),
                        item.TypeOfServiceNeededList is null ? "" : string.Join(",", item.TypeOfServiceNeededList.Where(t => KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))),
                        item.TypeOfServiceProvidedList is null ? "" : string.Join(",", item.TypeOfServiceProvidedList.Where(t => KeyLists.ServiceProvidedList.Any(s => s.ToLower().Contains(t.ToLower())))),
                        item.TypeOfServiceReferredForList is null ? "" : string.Join(",", item.TypeOfServiceReferredForList.Where(t => KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))),
                        item.NameOfServiceProviderOrCsoReferredTo,
                        item.ReferralOutcome is null ? "" : string.Join("", item.ReferralOutcome.Where(t => KeyLists.Outcome.Any(s => s.ToLower().Contains(t.ToString().ToLower())))),
                        item.OrganisationName,
                        item.CreatedAt.ToLongDateString(),
                        item.CreatedByName,
                        item.DateOfServiceProvision.HasValue?item.DateOfServiceProvision.Value.ToLongDateString():"",
                        item.ReferralToAnotherCsoOrSPcode,
                        item.SpOrCsoApprovalDate.HasValue?item.SpOrCsoApprovalDate.Value.ToLongDateString():"",
                        item.StateName,
                        item.StateApprovalDate.HasValue? item.StateApprovalDate.Value.ToLongDateString():"",
                        item.Status.GetDescription()

                    };

                    foreach (var data in dataRow.Select((item, index) => new { Index = index, Item = item }))
                    {
                        WorkSheet.Cells[row, data.Index + 1].Value = data.Item;
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }

            await Excelpackage.SaveAsync();
            return Excelpackage;
        }

        private async Task<ExcelPackage> GetServiceExcelSheetNew(IEnumerable<ServiceProvidedDto> model)
        {

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            ExcelPackage Excelpackage = new();
            ExcelWorksheet WorkSheet = Excelpackage.Workbook.Worksheets.Add($"Service Report_{DateTime.Now:dd-MMM-yy}");

            var row = 2;
            var headers = new List<string>()
            {
                "SERIAL NUMBER",
                "DATE CREATED",
                "DATE OF SERVICE PROVISION",
                "SERVICE PROVISION CODE",
                "ORGANIZATION NAME",
                "STATE",
                "LGA",
                "DO YOU HAVE AN INCIDENT CODE FOR THIS SERVICE?",
                "INCIDENT CODE",
                "AGE OF SURVIVOR IN YEARS",
                "SEX OF SURVIVOR OR VICTIM",
                "TYPE OF CLIENT",
                "HAS SURVIVOR RECEIVED SERVICE FROM ANOTHER ORGANISATION",
                "TYPE OF SERVICE RECEIVED FROM ANOTHER ORGANISATION_SERVICE OF POLICE/OTHER SECURITY ACTORS",
                "TYPE OF SERVICE RECEIVED FROM ANOTHER ORGANISATION_LEGAL ASSISTANCE",
                "TYPE OF SERVICE RECEIVED FROM ANOTHER ORGANISATION_LIVELIHOOD/SOCIAL WELFARE SERVICES",
                "TYPE OF SERVICE RECEIVED FROM ANOTHER ORGANISATION_SAFE HOUSE/SHELTER",
                "TYPE OF SERVICE RECEIVED FROM ANOTHER ORGANISATION_MEDICAL/HEALTH SERVICE",
                "TYPE OF SERVICE RECEIVED FROM ANOTHER ORGANISATION_PSYCHOSOCIAL/COUNSELLING",
                "TYPE OF SERVICE RECEIVED FROM ANOTHER ORGANISATION_EDUCATION",
                "TYPE OF SERVICE RECEIVED FROM ANOTHER ORGANISATION_REFERRAL",
                "TYPE OF SERVICE RECEIVED FROM ANOTHER ORGANISATION_OTHERS",
                "INCIDENT CODE FROM REFERRING ORGANISATION",
                "REFERRAL CODE (FROM REFERRING ORGANIZATION)",
                "TYPE OF SERVICE NEEDED_SERVICE OF POLICE/OTHER SECURITY ACTORS",
                "TYPE OF SERVICE NEEDED_LEGAL ASSISTANCE",
                "TYPE OF SERVICE NEEDED_LIVELIHOOD/SOCIAL WELFARE SERVICES",
                "TYPE OF SERVICE NEEDED_SAFE HOUSE/SHELTER",
                "TYPE OF SERVICE NEEDED_MEDICAL/HEALTH SERVICE",
                "TYPE OF SERVICE NEEDED_PSYCHOSOCIAL/COUNSELLING",
                "TYPE OF SERVICE NEEDED_EDUCATION",
                "TYPE OF SERVICE NEEDED_REFERRAL",
                "TYPE OF SERVICE NEEDED_OTHERS",
                "TYPE OF SERVICE PROVIDED_NONE",
                "TYPE OF SERVICE PROVIDED_SERVICE OF POLICE/OTHER SECURITY ACTORS",
                "TYPE OF SERVICE PROVIDED_LEGAL ASSISTANCE",
                "TYPE OF SERVICE PROVIDED_LIVELIHOOD/SOCIAL WELFARE SERVICES",
                "TYPE OF SERVICE PROVIDED_SAFE HOUSE/SHELTER",
                "TYPE OF SERVICE PROVIDED_MEDICAL/HEALTH SERVICE",
                "TYPE OF SERVICE PROVIDED_PSYCHOSOCIAL/COUNSELLING",
                "TYPE OF SERVICE PROVIDED_EDUCATION",
                "TYPE OF SERVICE PROVIDED_REFERRAL",
                "TYPE OF SERVICE PROVIDED_OTHERS",
                "TYPE OF SERVICE REFERRED_SERVICE OF POLICE/OTHER SECURITY ACTORS",
                "TYPE OF SERVICE REFERRED_LEGAL ASSISTANCE",
                "TYPE OF SERVICE REFERRED_LIVELIHOOD/SOCIAL WELFARE SERVICES",
                "TYPE OF SERVICE REFERRED_SAFE HOUSE/SHELTER",
                "TYPE OF SERVICE REFERRED_MEDICAL/HEALTH SERVICE",
                "TYPE OF SERVICE REFERRED_PSYCHOSOCIAL/COUNSELLING",
                "TYPE OF SERVICE REFERRED_EDUCATION",
                "TYPE OF SERVICE REFERRED_REFERRAL",
                "TYPE OF SERVICE REFERRED_OTHERS",
                "NAME OF SERVICE PROVIDER OR CSO REFERRED TO",
                "REFERRAL TO ANOTHER CSO OR SP CODE",
                "REFERRAL OUTCOME",
                "CREATOR",
                "SP OR CSO APPROVAL DATE",
                "STATE APPROVAL DATE",
                "STATUS"
            };

            foreach (var header in headers.Select((item, index) => new { Index = index, Item = item }))
            {
                WorkSheet.Cells[row, header.Index + 1].Value = header.Item;
            }

            WorkSheet.Cells[row, 1, row, headers.Count].Style.Font.Bold = true;

            foreach (var item in model)
            {
                row++;
                //item.SpOrCsoApprovalDate.
                //var spOrCsoApprovalDate = item.SpOrCsoApprovalDate.HasValue ? "" : item.SpOrCsoApprovalDate.Value.ToLongDateString();
                try
                {
                    var dataRow = new List<string>()
                    {
                        item.SerialNumber.ToString(),
                        item.CreatedAt.ToLongDateString(),
                        item.DateOfServiceProvision.HasValue?item.DateOfServiceProvision.Value.ToLongDateString():"",
                        item.ServiceProvisionCode,
                        item.OrganisationName,
                        item.StateName,
                        item.LGA,//Lga
                        item.IncidentCode is not null?YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),//DO YOU HAVE AN INCIDENT CODE FOR THIS SERVICE?
                        item.IncidentCode,
                        item.AgeOfSurvivorInYears.ToString(),
                        item.SexOfSurvivorOrVictim,
                        item.TypeOfClient,
                        item.HasSurvivorReceivedServiceFromAnotherOrganisation.GetDescription(),


                        item.TypeOfServiceReceivedAnotherOrganisationList?.Any(s=> s == MetricsKeys.ServiceProvided_PoliceSecurity) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceReceivedAnotherOrganisationList?.Any(s=> s == MetricsKeys.ServiceProvided_Legal) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceReceivedAnotherOrganisationList?.Any(s=> s == MetricsKeys.ServiceProvided_Livelihood) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceReceivedAnotherOrganisationList?.Any(s=> s == MetricsKeys.ServiceProvided_SafeHouse) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceReceivedAnotherOrganisationList?.Any(s=> s == MetricsKeys.ServiceProvided_Medical) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceReceivedAnotherOrganisationList?.Any(s=> s == MetricsKeys.ServiceProvided_Psychosocial) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceReceivedAnotherOrganisationList?.Any(s=> s == MetricsKeys.ServiceProvided_Education) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceReceivedAnotherOrganisationList?.Any(s=> s == MetricsKeys.ServiceProvided_Referral) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceReceivedAnotherOrganisationList is null ? "": string.Join(",", item.TypeOfServiceReceivedAnotherOrganisationList.Where(t => !KeyLists.ServiceProvidedList.Any(s => s.ToLower().Contains(t.ToLower())))),

                        item.IncidentCodeFromReferringOrganisation,
                        item.ReferralCode,



                        item.TypeOfServiceNeededList?.Any(s=> s == MetricsKeys.ServiceProvided_PoliceSecurity) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceNeededList?.Any(s=> s == MetricsKeys.ServiceProvided_Legal) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceNeededList?.Any(s=> s == MetricsKeys.ServiceProvided_Livelihood) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceNeededList?.Any(s=> s == MetricsKeys.ServiceProvided_SafeHouse) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceNeededList?.Any(s=> s == MetricsKeys.ServiceProvided_Medical) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceNeededList?.Any(s=> s == MetricsKeys.ServiceProvided_Psychosocial) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceNeededList?.Any(s=> s == MetricsKeys.ServiceProvided_Education) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceNeededList?.Any(s=> s == MetricsKeys.ServiceProvided_Referral) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceNeededList is null ? "": string.Join(",", item.TypeOfServiceReceivedAnotherOrganisationList.Where(t => !KeyLists.ServiceProvidedList.Any(s => s.ToLower().Contains(t.ToLower())))),


                        item.TypeOfServiceProvidedList?.Any(s=> s == MetricsKeys.None) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceProvidedList?.Any(s=> s == MetricsKeys.ServiceProvided_PoliceSecurity) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceProvidedList?.Any(s=> s == MetricsKeys.ServiceProvided_Legal) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceProvidedList?.Any(s=> s == MetricsKeys.ServiceProvided_Livelihood) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceProvidedList?.Any(s=> s == MetricsKeys.ServiceProvided_SafeHouse) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceProvidedList?.Any(s=> s == MetricsKeys.ServiceProvided_Medical) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceProvidedList?.Any(s=> s == MetricsKeys.ServiceProvided_Psychosocial) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceProvidedList?.Any(s=> s == MetricsKeys.ServiceProvided_Education) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceProvidedList?.Any(s=> s == MetricsKeys.ServiceProvided_Referral) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceProvidedList is null ? "": string.Join(",", item.TypeOfServiceReceivedAnotherOrganisationList.Where(t => !KeyLists.ServiceProvidedList.Any(s => s.ToLower().Contains(t.ToLower())))),


                        item.TypeOfServiceReferredForList?.Any(s=> s == MetricsKeys.ServiceProvided_PoliceSecurity) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceReferredForList?.Any(s=> s == MetricsKeys.ServiceProvided_Legal) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceReferredForList?.Any(s=> s == MetricsKeys.ServiceProvided_Livelihood) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceReferredForList?.Any(s=> s == MetricsKeys.ServiceProvided_SafeHouse) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceReferredForList?.Any(s=> s == MetricsKeys.ServiceProvided_Medical) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceReferredForList?.Any(s=> s == MetricsKeys.ServiceProvided_Psychosocial) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceReferredForList?.Any(s=> s == MetricsKeys.ServiceProvided_Education) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceReferredForList?.Any(s=> s == MetricsKeys.ServiceProvided_Referral) ==true ? YesOrNo.Yes.ToString(): YesOrNo.No.ToString(),
                        item.TypeOfServiceReferredForList is null ? "": string.Join(",", item.TypeOfServiceReceivedAnotherOrganisationList.Where(t => !KeyLists.ServiceProvidedList.Any(s => s.ToLower().Contains(t.ToLower())))),



                        //item.TypeOfServiceReceivedAnotherOrganisationList is null ? "" : string.Join(",", item.TypeOfServiceReceivedAnotherOrganisationList.Where(t => KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))),
                        //item.TypeOfServiceNeededList is null ? "" : string.Join(",", item.TypeOfServiceNeededList.Where(t => KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))),
                        //item.TypeOfServiceProvidedList is null ? "" : string.Join(",", item.TypeOfServiceProvidedList.Where(t => KeyLists.ServiceProvidedList.Any(s => s.ToLower().Contains(t.ToLower())))),
                        //item.TypeOfServiceReferredForList is null ? "" : string.Join(",", item.TypeOfServiceReferredForList.Where(t => KeyLists.TypeOfService.Any(s => s.ToLower().Contains(t.ToLower())))),
                        item.NameOfServiceProviderOrCsoReferredTo,
                        item.ReferralToAnotherCsoOrSPcode,
                        item.ReferralOutcome is null ? "" : string.Join("", item.ReferralOutcome.Where(t => KeyLists.Outcome.Any(s => s.ToLower().Contains(t.ToString().ToLower())))),


                        item.CreatedByName,


                        item.SpOrCsoApprovalDate.HasValue?item.SpOrCsoApprovalDate.Value.ToLongDateString():"",

                        item.StateApprovalDate.HasValue? item.StateApprovalDate.Value.ToLongDateString():"",
                        item.Status.GetDescription()

                    };

                    foreach (var data in dataRow.Select((item, index) => new { Index = index, Item = item }))
                    {
                        WorkSheet.Cells[row, data.Index + 1].Value = data.Item;
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }

            await Excelpackage.SaveAsync();
            return Excelpackage;
        }
        public IQueryable<ServiceProvided> FilterServicesProvided(IQueryable<ServiceProvided> query, GetAllServiceProvidedRequest request, string userType)
        {
            if (request.IsCaseValidated != null)
            {
                switch (request.IsCaseValidated.ToLower())
                {
                    case "yes":
                        query = query.Where(x => x.Status == ValidationStatus.State && x.Status == ValidationStatus.SpOrCso);
                        break;
                    case "no":
                        query = query.Where(x => x.Status == ValidationStatus.StateRejected && x.Status == ValidationStatus.SpOrCsoRejected);
                        break;
                    default:
                        break;
                }
            }

            //if (request.IsCaseValidated.ToLower() == "yes")
            //{
            //    query = query.Where(x => x.Status == ValidationStatus.State && x.Status == ValidationStatus.SpOrCso);
            //}
            //if (request.IsCaseValidated.ToLower() == "no")
            //{
            //    query = query.Where(x => x.Status == ValidationStatus.StateRejected && x.Status == ValidationStatus.SpOrCsoRejected);
            //}

            if (request.ValidationStatus.HasValue)
            {
                if (userType == RoleKeys.Administrator && request.ValidationStatus != ValidationStatus.Submitted)// helps admins get only cases that have been validated at cso/sp and state level
                {
                    query = query.Where(s => s.Status == ValidationStatus.SpOrCso || s.Status == ValidationStatus.State);
                }
                else
                {
                    query = query.Where(s => s.Status == request.ValidationStatus);
                }
            }

            if (request.StartDateOfServiceProvision.HasValue)
            {
                query = query.Where(s => s.DateOfServiceProvision >= request.StartDateOfServiceProvision);
            }

            if (request.EndDateOfServiceProvision.HasValue)
            {
                query = query.Where(s => s.DateOfServiceProvision <= request.EndDateOfServiceProvision);
            }

            if (!string.IsNullOrWhiteSpace(request.Organisation))
            {
                query = query.Where(s => s.Organisation.Name.ToLower().Contains(request.Organisation.ToLower()));
            }

            if (request.OrganisationId.HasValue)
            {
                query = query.Where(s => s.OrganisationId == request.OrganisationId);
            }

            if (request.OrganisationLgaId.HasValue)
            {
                query = query.Where(s => s.OrganisationLgaId == request.OrganisationLgaId);
            }

            if (request.StateId.HasValue)
            {
                query = query.Where(s => s.StateId == request.StateId);
            }

            if (!string.IsNullOrWhiteSpace(request.ServiceProvisionCode))
            {
                query = query.Where(s => s.ServiceProvisionCode.ToLower().Contains(request.ServiceProvisionCode.ToLower()));
            }

            if (request.TypeOfServiceProvided != null && request.TypeOfServiceProvided.Any())
            {
                var queryString = string.Join(" || ", request.TypeOfServiceProvided.Select(t => $"s.TypeOfServiceProvided.ToLower().Contains(\"{t.ToLower()}\")"));

                query = query.Where($"s => {queryString}");
            }

            return query;
        }
        public IQueryable<ServiceProvided> FilterServicesProvided(IQueryable<ServiceProvided> query, ServiceProvidedRequest request, string userType)
        {
            if (request.IsCaseValidated != null)
            {
                switch (request.IsCaseValidated.ToLower())
                {
                    case "yes":
                        query = query.Where(x => x.Status == ValidationStatus.State && x.Status == ValidationStatus.SpOrCso);
                        break;
                    case "no":
                        query = query.Where(x => x.Status == ValidationStatus.StateRejected && x.Status == ValidationStatus.SpOrCsoRejected);
                        break;
                    default:
                        break;
                }
            }
            if (request.ValidationStatus.HasValue)
            {
                if (userType == RoleKeys.Administrator && request.ValidationStatus != ValidationStatus.Submitted)// helps admins get only cases that have been validated at cso/sp and state level
                {
                    query = query.Where(s => s.Status == ValidationStatus.SpOrCso || s.Status == ValidationStatus.State);
                }
                else
                {
                    query = query.Where(s => s.Status == request.ValidationStatus);
                }
            }

            if (request.StartDateOfServiceProvision.HasValue)
            {
                query = query.Where(s => s.DateOfServiceProvision >= request.StartDateOfServiceProvision);
            }

            if (request.EndDateOfServiceProvision.HasValue)
            {
                query = query.Where(s => s.DateOfServiceProvision <= request.EndDateOfServiceProvision);
            }

            if (!string.IsNullOrWhiteSpace(request.Organisation))
            {
                query = query.Where(s => s.Organisation.Name.ToLower().Contains(request.Organisation.ToLower()));
            }

            if (request.OrganisationId.HasValue)
            {
                query = query.Where(s => s.OrganisationId == request.OrganisationId);
            }

            if (request.OrganisationLgaId.HasValue)
            {
                query = query.Where(s => s.OrganisationLgaId == request.OrganisationLgaId);
            }

            if (request.StateId.HasValue)
            {
                query = query.Where(s => s.StateId == request.StateId);
            }

            if (!string.IsNullOrWhiteSpace(request.ServiceProvisionCode))
            {
                query = query.Where(s => s.ServiceProvisionCode.ToLower().Contains(request.ServiceProvisionCode.ToLower()));
            }

            if (request.TypeOfServiceProvided != null && request.TypeOfServiceProvided.Any())
            {
                var queryString = string.Join(" || ", request.TypeOfServiceProvided.Select(t => $"s.TypeOfServiceProvided.ToLower().Contains(\"{t.ToLower()}\")"));

                query = query.Where($"s => {queryString}");
            }

            return query;
        }

        public async Task<Result> UpdateServiceProvided(UpdateServiceProvidedRequest request)
        {
            var user = await _context.Users
                .AsNoTracking().FirstOrDefaultAsync(c => c.Id == UserId);

            if (user is null) return Result.Failure("User not found");

            var serviceProvided = await _context.ServicesProvided.FirstOrDefaultAsync(c => c.Id == request.Id);

            if (serviceProvided is null) return Result.Failure("Service provided not found");

            if (user.Id != serviceProvided.CreatedById) return Result.Failure("You cannot edit data submitted by another user");

            serviceProvided.UpdateInfo(
                request.AgeOfSurvivorInYears,
                request.SexOfSurvivorOrVictim,
                request.TypeOfClient,
                request.HasSurvivorReceivedServiceFromAnotherOrganisation,
                request.IncidentCodeFromReferringOrganisation,
                request.ReferralCode,
                request.TypeOfServiceReceivedAnotherOrganisation,
                request.TypeOfServiceNeeded,
                request.TypeOfServiceProvided,
                request.TypeOfServiceReferredFor,
                request.NameOfServiceProviderOrCsoReferredTo,
                request.ReferralOutcome,
                request.Longitude,
                request.Latitude,
                request.DateOfServiceProvision,
                request.GbvCovid19Question1,
                request.GbvCovid19Question2,
                request.GbvCovid19Question3,
                request.GbvCovid19Question4,
                request.OrganisationLgaId,
                request.IncidentCode,
                request.ReferralToAnotherCsoOrSPcode);

            _context.Entry(serviceProvided).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> DeleteServiceProvided(int id)
        {
            var service = await _context.ServicesProvided.FirstOrDefaultAsync(s => s.Id == id);

            if (service is null) return Result.Failure("Service provided not found");

            _context.ServicesProvided.Remove(service);

            var status = await _context.SaveChangesAsync() > 0;

            return status ? Result.Success() : Result.Failure("Failed to delete");
        }

        public async Task<Result> ApproveServiceProvided(int serviceProvidedId, bool approvalUndo, bool isReject)
        {
            var user = await _context.Users
                .AsNoTracking().Where(c => c.Id == UserId)
                .Select(c => new { c.Type, c.Id })
                .FirstOrDefaultAsync();

            if (user is null) return Result.Failure("User not found");

            var service = await _context.ServicesProvided.FirstOrDefaultAsync(s => s.Id == serviceProvidedId);

            if (service is null) return Result.Failure("Service not found");

            if (service.Status == ValidationStatus.SpOrCsoRejected || service.Status == ValidationStatus.StateRejected)
            {
                return Result.Failure("Service has already been rejected and no further action can be done");
            }
            if (approvalUndo)
            {
                service.UndoApproval();
            }
            else
            {
                if (isReject)
                {
                    if (user.Type is RoleKeys.CSOSupervior or RoleKeys.ServiceProviderSupervior)
                    {
                        service.Approve(ValidationStatus.SpOrCsoRejected, user.Id);
                    }
                    else if (user.Type is RoleKeys.StateAdministrator or RoleKeys.StateSupervisor)
                    {
                        service.Approve(ValidationStatus.StateRejected, user.Id);
                    }
                    else if (user.Type == RoleKeys.Administrator)
                    {
                        service.UndoApproval();
                    }
                }

                else
                {
                    if (user.Type is RoleKeys.CSOSupervior or RoleKeys.ServiceProviderSupervior)
                    {
                        service.Approve(ValidationStatus.SpOrCso, user.Id);
                    }
                    else if (user.Type is RoleKeys.StateAdministrator or RoleKeys.StateSupervisor)
                    {
                        service.Approve(ValidationStatus.State, user.Id);
                    }
                    else if (user.Type == RoleKeys.Administrator)
                    {
                        service.UndoApproval();
                    }
                }
            }

            //_context.Entry(service).State = EntityState.Modified;
            var status = await _context.SaveChangesAsync() > 0;

            return status ? Result.Success() : Result.Failure("Failed to save");
        }


        public async Task<AppResult<string>> BulkUpload(IFormFile file)
        {
            var result = new AppResult<string>();
            var fileName = _configuration.GetSection("BulkUploadSettings:Services").Value.ToLower();
            if (file.FileName.ToLower() != fileName)
            {
                result.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                result.Message = " Invalid file";
                result.AddError("Invalid file");
                return result;
            }

            var user = await _context.Users
                .AsNoTracking().Where(c => c.Id == UserId)
                .Select(u => new { StateCode = u.State.Code, OrganisationCode = u.Organisation.Code, u.StateId, u.Id, OrganisationId = u.OrganisationId })
                .FirstOrDefaultAsync();

            if (user is null)
            {
                result.AddError("User not found");
                return result;
            }



            if (file?.Length > 0)
            {

                var stream = file.OpenReadStream();

                var executionStrategy = _context.Database.CreateExecutionStrategy();
                executionStrategy.Execute(
                () =>
                {
                    using var transaction = _context.Database.BeginTransaction();
                    try
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                        using var package = new ExcelPackage(stream);
                        var worksheet = package.Workbook.Worksheets.First();
                        var rowCount = worksheet.Dimension.Rows;
                        var validRow = GetLastRow(worksheet);
                        if (validRow == 3)
                        {
                            result.StatusCode = StatusCodes.Status406NotAcceptable;
                            result.Message = " Template file is empty";
                            result.AddError("Template file is empty");
                            return;
                        }

                        var isCaseValid = _configuration.GetSection("BulkUploadSettings:ServicesValid").Value.ToLower();
                        var textInFile = worksheet.Cells[3, 2].Value?.ToString().ToLower();
                        if (textInFile != isCaseValid || textInFile == null)
                        {
                            result.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                            result.Message = " Invalid file";
                            result.AddError("Invalid file");
                            return;
                        }

                        var codeResult = GenerateServiceProvisionCode(user.StateCode, user.OrganisationCode).Result;

                        var organisation = GetOrganisation(user.OrganisationId.Value).Result;
                        var lGA = GetLocalGovernmentAreas();
                        var states = GetStates();
                        var wards = GetWards();
                        List<string> typeOfServiceReceivedAnotherOrganisation = new();
                        List<string> typeOfServiceNeeded = new();
                        List<string> typeOfServiceProvided = new();
                        List<ServiceProvided> listOfServices = new();
                        List<string> typeOfServiceReferredFor = new();
                        for (var row = 4; row <= validRow; row++)
                        {
                            var organisationId = user.OrganisationId.Value;
                            var organisationLgaName = worksheet.Cells[row, 2].Value.ToString().ToLower().Trim();
                            var organisationLga = GetALocalGovernmentArea(lGA, organisationLgaName).Result;
                            var organisationState = GetAState(organisation.Data.States, organisationLga.StateId);
                            if (organisationState == null)
                            {
                                result.StatusCode = StatusCodes.Status406NotAcceptable;
                                result.Message = $" case in row {row} : Incorrect local government";
                                result.AddError($" case in row {row} : Incorrect local government");
                                return;
                            }


                            var incidentCodeIsAvailable = worksheet.Cells[row, 3].Value?.ToString().ToLower();
                            string incidentCode = null, hasSurvivorReceivedServiceFromAnotherOrganisationOther = null, incidentCodeFromReferringOrganisation = null, referralCode = null;
                            switch (incidentCodeIsAvailable)
                            {
                                case "yes":
                                    incidentCode = worksheet.Cells[row, 4].Value?.ToString();
                                    break;
                                default:
                                    break;
                            }
                            var dateOfServiceProvision = Convert.ToDateTime(worksheet.Cells[row, 5].Value?.ToString());
                            var ageOfSurvivorInYears = int.Parse(worksheet.Cells[row, 6].Value?.ToString());
                            var sexOfSurvivorOrVictim = worksheet.Cells[row, 7].Value?.ToString();
                            var typeOfClient = worksheet.Cells[row, 8].Value?.ToString();
                            var hasSurvivorReceivedServiceFromAnotherOrganisationResponse = worksheet.Cells[row, 9].Value?.ToString().ToLower();
                            YesOrNo hasSurvivorReceivedServiceFromAnotherOrganisation = YesOrNo.NotApplicable;

                            switch (hasSurvivorReceivedServiceFromAnotherOrganisationResponse)
                            {
                                case "yes":
                                    hasSurvivorReceivedServiceFromAnotherOrganisation = YesOrNo.Yes;
                                    if (worksheet.Cells[row, 10].Value?.ToString().ToLower() == "yes")
                                        typeOfServiceReceivedAnotherOrganisation.Add(MetricsKeys.ServiceProvided_PoliceSecurity);
                                    if (worksheet.Cells[row, 11].Value?.ToString().ToLower() == "yes")
                                        typeOfServiceReceivedAnotherOrganisation.Add(MetricsKeys.ServiceProvided_Legal);
                                    if (worksheet.Cells[row, 12].Value?.ToString().ToLower() == "yes")
                                        typeOfServiceReceivedAnotherOrganisation.Add(MetricsKeys.ServiceProvided_Livelihood);
                                    if (worksheet.Cells[row, 13].Value?.ToString().ToLower() == "yes")
                                        typeOfServiceReceivedAnotherOrganisation.Add(MetricsKeys.ServiceProvided_SafeHouse);
                                    if (worksheet.Cells[row, 14].Value?.ToString().ToLower() == "yes")
                                        typeOfServiceReceivedAnotherOrganisation.Add(MetricsKeys.ServiceProvided_Medical);
                                    if (worksheet.Cells[row, 15].Value?.ToString().ToLower() == "yes")
                                        typeOfServiceReceivedAnotherOrganisation.Add(MetricsKeys.ServiceProvided_Psychosocial);
                                    if (worksheet.Cells[row, 16].Value?.ToString().ToLower() == "yes")
                                        typeOfServiceReceivedAnotherOrganisation.Add(MetricsKeys.ServiceProvided_Education);
                                    if (worksheet.Cells[row, 17].Value?.ToString().ToLower() == "yes")
                                        typeOfServiceReceivedAnotherOrganisation.Add(MetricsKeys.ServiceProvided_Referral);
                                    hasSurvivorReceivedServiceFromAnotherOrganisationOther = worksheet.Cells[row, 18].Value?.ToString().ToLower();
                                    if (hasSurvivorReceivedServiceFromAnotherOrganisationOther != null)
                                        typeOfServiceReceivedAnotherOrganisation.Add(hasSurvivorReceivedServiceFromAnotherOrganisationOther);
                                    incidentCodeFromReferringOrganisation = worksheet.Cells[row, 19].Value?.ToString();
                                    referralCode = worksheet.Cells[row, 20].Value?.ToString();
                                    break;
                                case "no":
                                    hasSurvivorReceivedServiceFromAnotherOrganisation = YesOrNo.No;
                                    break;
                                default:
                                    break;
                            }



                            if (worksheet.Cells[row, 21].Value?.ToString().ToLower() == "yes")
                                typeOfServiceNeeded.Add(MetricsKeys.ServiceProvided_PoliceSecurity);
                            if (worksheet.Cells[row, 22].Value?.ToString().ToLower() == "yes")
                                typeOfServiceNeeded.Add(MetricsKeys.ServiceProvided_Legal);
                            if (worksheet.Cells[row, 23].Value?.ToString().ToLower() == "yes")
                                typeOfServiceNeeded.Add(MetricsKeys.ServiceProvided_Livelihood);
                            if (worksheet.Cells[row, 24].Value?.ToString().ToLower() == "yes")
                                typeOfServiceNeeded.Add(MetricsKeys.ServiceProvided_SafeHouse);
                            if (worksheet.Cells[row, 25].Value?.ToString().ToLower() == "yes")
                                typeOfServiceNeeded.Add(MetricsKeys.ServiceProvided_Medical);
                            if (worksheet.Cells[row, 26].Value?.ToString().ToLower() == "yes")
                                typeOfServiceNeeded.Add(MetricsKeys.ServiceProvided_Psychosocial);
                            if (worksheet.Cells[row, 27].Value?.ToString().ToLower() == "yes")
                                typeOfServiceNeeded.Add(MetricsKeys.ServiceProvided_Education);
                            if (worksheet.Cells[row, 28].Value?.ToString().ToLower() == "yes")
                                typeOfServiceNeeded.Add(MetricsKeys.ServiceProvided_Referral);
                            var typeOfServiceNeededOther = worksheet.Cells[row, 29].Value?.ToString().ToLower();
                            if (typeOfServiceNeededOther != null)
                                typeOfServiceNeeded.Add(typeOfServiceNeededOther);


                            string nameOfServiceProviderOrCsoReferredTo = null, referralOutcome = null, referralToAnotherCsoOrSPcode = null;

                            if (worksheet.Cells[row, 30].Value?.ToString().ToLower() == "yes")
                                typeOfServiceProvided.Add(MetricsKeys.ServiceProvided_PoliceSecurity);
                            if (worksheet.Cells[row, 31].Value?.ToString().ToLower() == "yes")
                                typeOfServiceProvided.Add(MetricsKeys.ServiceProvided_Legal);
                            if (worksheet.Cells[row, 32].Value?.ToString().ToLower() == "yes")
                                typeOfServiceProvided.Add(MetricsKeys.ServiceProvided_Livelihood);
                            if (worksheet.Cells[row, 33].Value?.ToString().ToLower() == "yes")
                                typeOfServiceProvided.Add(MetricsKeys.ServiceProvided_SafeHouse);
                            if (worksheet.Cells[row, 34].Value?.ToString().ToLower() == "yes")
                                typeOfServiceProvided.Add(MetricsKeys.ServiceProvided_Medical);
                            if (worksheet.Cells[row, 35].Value?.ToString().ToLower() == "yes")
                                typeOfServiceProvided.Add(MetricsKeys.ServiceProvided_Psychosocial);
                            if (worksheet.Cells[row, 36].Value?.ToString().ToLower() == "yes")
                                typeOfServiceProvided.Add(MetricsKeys.ServiceProvided_Education);
                            if (worksheet.Cells[row, 37].Value?.ToString().ToLower() == "yes")
                            {
                                typeOfServiceProvided.Add(MetricsKeys.ServiceProvided_Referral);
                                if (worksheet.Cells[row, 39].Value?.ToString().ToLower() == "yes")
                                    typeOfServiceReferredFor.Add(MetricsKeys.ServiceProvided_PoliceSecurity);
                                if (worksheet.Cells[row, 40].Value?.ToString().ToLower() == "yes")
                                    typeOfServiceReferredFor.Add(MetricsKeys.ServiceProvided_Legal);
                                if (worksheet.Cells[row, 41].Value?.ToString().ToLower() == "yes")
                                    typeOfServiceReferredFor.Add(MetricsKeys.ServiceProvided_Livelihood);
                                if (worksheet.Cells[row, 42].Value?.ToString().ToLower() == "yes")
                                    typeOfServiceReferredFor.Add(MetricsKeys.ServiceProvided_SafeHouse);
                                if (worksheet.Cells[row, 43].Value?.ToString().ToLower() == "yes")
                                    typeOfServiceReferredFor.Add(MetricsKeys.ServiceProvided_Medical);
                                if (worksheet.Cells[row, 44].Value?.ToString().ToLower() == "yes")
                                    typeOfServiceReferredFor.Add(MetricsKeys.ServiceProvided_Psychosocial);
                                if (worksheet.Cells[row, 45].Value?.ToString().ToLower() == "yes")
                                    typeOfServiceReferredFor.Add(MetricsKeys.ServiceProvided_Education);
                                var typeOfServiceReferredForOther = worksheet.Cells[row, 46].Value?.ToString().ToLower();
                                if (typeOfServiceReferredForOther != null)
                                    typeOfServiceReferredFor.Add(typeOfServiceReferredForOther);
                                nameOfServiceProviderOrCsoReferredTo = worksheet.Cells[row, 47].Value?.ToString();
                                referralToAnotherCsoOrSPcode = worksheet.Cells[row, 48].Value?.ToString();
                                referralOutcome = worksheet.Cells[row, 49].Value?.ToString();
                            }

                            var typeOfServiceProvidedOther = worksheet.Cells[row, 38].Value?.ToString().ToLower();
                            if (typeOfServiceNeededOther != null)
                                typeOfServiceProvided.Add(typeOfServiceProvidedOther);

                            var covidQuestion = worksheet.Cells[row, 50].Value?.ToString().ToLower();
                            string gbV_COVID19_Question1 = null, gbV_COVID19_Question2 = null, gbV_COVID19_Question3 = null, gbV_COVID19_Question4 = null;
                            if (covidQuestion == "yes")
                            {
                                gbV_COVID19_Question1 = worksheet.Cells[row, 51].Value?.ToString().ToLower();
                                gbV_COVID19_Question2 = worksheet.Cells[row, 52].Value?.ToString().ToLower();
                                gbV_COVID19_Question3 = worksheet.Cells[row, 53].Value?.ToString().ToLower();
                                gbV_COVID19_Question4 = worksheet.Cells[row, 53].Value?.ToString().ToLower();
                            }


                            var serviceProvided = new ServiceProvided(
                                codeResult.serial,
                                codeResult.serviceProvisionCode,
                                ageOfSurvivorInYears,
                                sexOfSurvivorOrVictim,
                                typeOfClient,
                                hasSurvivorReceivedServiceFromAnotherOrganisation,
                                incidentCodeFromReferringOrganisation,
                                typeOfServiceReceivedAnotherOrganisation,
                                referralCode,
                                typeOfServiceNeeded,
                                typeOfServiceProvided,
                                typeOfServiceReferredFor,
                                nameOfServiceProviderOrCsoReferredTo,
                                referralOutcome,
                                organisationId,
                                user.StateId.Value,
                                null,
                                null,
                                dateOfServiceProvision,
                                user.Id,
                                gbV_COVID19_Question1,
                                gbV_COVID19_Question2,
                                gbV_COVID19_Question3,
                                gbV_COVID19_Question4,
                                organisationLga.Id,
                                incidentCode,
                                referralToAnotherCsoOrSPcode
                                );


                            listOfServices.Add(serviceProvided);
                            typeOfServiceNeeded.Clear();
                            typeOfServiceProvided.Clear();
                            typeOfServiceReceivedAnotherOrganisation.Clear();
                            typeOfServiceReferredFor.Clear();

                        }


                        if (listOfServices.Count == 0)
                        {
                            result.StatusCode = StatusCodes.Status406NotAcceptable;
                            result.Message = "An error occured while uploading data";
                            result.AddError("An error occured while uploading data");
                        }
                        //var result=await UploadCases(cases);
                        _context.ServicesProvided.BulkInsertAsync(listOfServices);
                        _context.SaveChanges();
                        transaction.Commit();
                        result.StatusCode = StatusCodes.Status201Created;
                        result.Message = $"Success";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        result.StatusCode = StatusCodes.Status406NotAcceptable;
                        result.Message = "An error occured while uploading data";
                        result.AddError(ex.InnerException.Message);
                    }

                    if (result.HasError)
                    {
                        transaction.Rollback();
                    }
                });
            }

            else
            {
                result.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                result.Message = " Invalid file";
                result.AddError("Invalid file");
            }

            return result;


        }

        #region WIP
        //public async Task<AppResult<MemoryStream>>ExportMonthlyReportService(GetAllServiceProvidedRequest model)
        //{


        //var (status, message, data) = await GetServicesForMonthlyReport(model);

        //if (!status) return new AppResult<MemoryStream>
        //{
        //    Message = message,
        //    StatusCode = StatusCodes.Status400BadRequest,
        //};

        //var query = data.services;

        //var typeOfServicesProvided = new List<string>
        //{
        //    MetricsKeys.ServiceProvided_PoliceSecurity,
        //    MetricsKeys.ServiceProvided_Legal,
        //    MetricsKeys.ServiceProvided_Livelihood,
        //    MetricsKeys.ServiceProvided_SafeHouse,
        //    MetricsKeys.ServiceProvided_Medical,
        //    MetricsKeys.ServiceProvided_Psychosocial,
        //    MetricsKeys.ServiceProvided_Education,
        //    MetricsKeys.ServiceProvided_Referral,
        //};

        //var servicesByType = new List<CaseBySubject>();
        //List<string> test = new();
        //int referral = 0, police = 0, education = 0, safehouse = 0, legal = 0, medical = 0, psychosocial=0, livelihood = 0;
        //foreach (var item in query)
        //{
        //    test= !string.IsNullOrWhiteSpace(item.TypeOfServiceProvided)
        //    ? JsonConvert.DeserializeObject<List<string>>(item.TypeOfServiceProvided)
        //    : new List<string>();

        //    foreach (var it in typeOfServicesProvided)
        //    {
        //        if(test.Contains(it))
        //    }

        //}
        //foreach (var item in typeOfServicesProvided)
        //{
        //    var queryString = string.Join(" || ", request.TypeOfServiceProvided.Select(t => $"s.TypeOfServiceProvided.ToLower().Contains(\"{t.ToLower()}\")"));

        //    query = query.Where($"s => {queryString}");
        //    var serviceType=query.Where(x=>x.TypeOfServiceProvided)
        //}




        // }
        #endregion


        private int GetLastRow(ExcelWorksheet sheet)
        {
            if (sheet.Dimension == null) return 0;

            var row = sheet.Dimension.End.Row;
            while (row >= 1)
            {
                var range = sheet.Cells[row, 2, row, sheet.Dimension.End.Column];
                if (range.Any(c => !string.IsNullOrWhiteSpace(c.Text)))
                {
                    break;
                }
                row--;
            }
            return row;
        }


        private IQueryable<LocalGovernmentArea> GetLocalGovernmentAreas() => _context.LocalGovernmentAreas.AsQueryable().AsNoTracking();

        private IQueryable<State> GetStates() => _context.States.AsQueryable().AsNoTracking();
        private IQueryable<Ward> GetWards() => _context.Wards.AsQueryable().AsNoTracking();

        private async Task<LocalGovernmentArea> GetALocalGovernmentArea(IQueryable<LocalGovernmentArea> localGovernmentAreas, string name) => await localGovernmentAreas.FirstOrDefaultAsync(l => l.Name.ToLower().Trim().Contains(name));

        private StateList GetAState(List<StateList> states, int id) => states.FirstOrDefault(l => l.Id == id);
        private async Task<AppResult<OrganisationViewModel>> GetOrganisation(int Id)
        {
            var organisation = await _context.Organisations.AsNoTracking()
                                .Select(c => new OrganisationViewModel
                                {
                                    Id = c.Id,
                                    States = string.IsNullOrWhiteSpace(c.States) ? new List<StateList>() : JsonConvert.DeserializeObject<List<StateList>>(c.States),
                                    Address = c.Address,
                                    CreatedAt = c.CreatedAt,
                                    Code = c.Code,
                                    Email = c.Email,
                                    PhoneNumber = c.PhoneNumber,
                                    ModifiedAt = c.ModifiedAt.GetValueOrDefault(),
                                    Name = c.Name,
                                    OrganisationType = c.OrganisationType,
                                    NumberOfUsers = c.Users.Count(),
                                    Website = c.Website,
                                    Acronym = c.Acronym,
                                    HotLine = c.HotLine,
                                    TypeOfService = JsonConvert.DeserializeObject<List<string>>(c.TypeOfService ?? "[]"),
                                    SocialMediaData = JsonConvert.DeserializeObject<List<SocialMediaData>>(c.SocialMediaData ?? "[]")
                                }).FirstOrDefaultAsync(c => c.Id == Id);
            if (organisation is null)
            {
                return new AppResult<OrganisationViewModel>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Organisation not found"
                };
            }

            return new AppResult<OrganisationViewModel>
            {
                Data = organisation,
                StatusCode = StatusCodes.Status200OK,
                Message = "Organisation found"
            };
        }

        /// <summary>
        /// generate IncidentCode based on state and organisation code
        /// </summary>
        /// <param name="stateCode"></param>
        /// <param name="organisationCode"></param>
        /// <returns></returns>
        private async Task<(string serviceProvisionCode, int serial)> GenerateServiceProvisionCode(string stateCode, string organisationCode)
        {
            var todaysDate = DateTime.Now;
            var lastCaseSerialNumber = await _context.ServicesProvided
                .Where(c => c.CreatedAt.Month == todaysDate.Month && todaysDate.Year == c.CreatedAt.Year && c.Organisation.Code == organisationCode)
                .OrderBy(c => c.SerialNumber).Select(c => c.SerialNumber).LastOrDefaultAsync();

            var serialno = 0;
            if (lastCaseSerialNumber == 0)
            {
                serialno = 1;
            }
            else
            {
                serialno = lastCaseSerialNumber + 1;
            }

            var monthName = todaysDate.ToString("MMM", CultureInfo.InvariantCulture);
            return ServiceCodeAndSerialNumber(stateCode, organisationCode, serialno, todaysDate.Year.ToString(), monthName);
        }

        /// <summary>
        /// SER/CHU/21/NOV/001
        /// </summary>
        /// <param name="stateCode"></param>
        /// <param name="organisationCode"></param>
        /// <param name="serialNo"></param>
        /// <returns></returns>
        private static (string serviceProvisionCode, int serial) ServiceCodeAndSerialNumber(string stateCode, string organisationCode, int serialNo, string year, string month) =>
            ($"SER/{stateCode.ToUpper()}/{organisationCode}/{year.Substring(year.Length - 2)}/{month.ToUpper()}/{serialNo:0000}", serialNo);
    }
}

