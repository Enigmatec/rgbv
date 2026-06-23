using Core.Data;
using Core.Entities;
using Core.Enums;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Service.Enums;
using Service.Extensions;
using Service.Helpers;
using Service.Interfaces;
using Service.Models;
using Service.StringKeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Coravel.Queuing.Interfaces;
using Service.AppServices;
using Service.Invocables;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;

namespace Service.Implementations
{
    /// <summary>
    /// Handles the actions for Cases
    /// </summary>
    public partial class CaseService : ICaseService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpContext _httpContext;
        private readonly INotification _notification;
        private readonly ISetting _setting;
        private readonly IConfiguration _configuration;

        private readonly IQueue _queue;

        private string UserId => _httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private DateTime CurrentDate => DateTime.Now.ToUniversalTime().AddHours(1);

        public CaseService(ApplicationDbContext context, IHttpContextAccessor accessor, INotification notification, ISetting setting, IQueue queue, IConfiguration configuration)
        {
            _context = context;
            _httpContext = accessor.HttpContext;
            _notification = notification;
            _setting = setting;
            _queue = queue;
            _configuration = configuration;
        }

        /// <summary>
        /// Creates or adds Incident type
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> CreateCategory(CaseCategoryCreationModel model)
        {
            var category = await _context.CaseCategories.FirstOrDefaultAsync(c => c.Name.ToLower() == model.Name.ToLower());

            if (category != null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = { $"Case Category {model.Name} already exists" },
                    Message = $"Case Category creation failed"
                };
            }

            category = new CaseCategory
            {
                Description = model.Description,
                Name = model.Name,
            };

            _context.Add(category);

            await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                Data = category.Id.ToString(),
                Message = "Category created successfully",
                StatusCode = StatusCodes.Status201Created
            };
        }

        /// <summary>
        /// get an Incident type
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<AppResult<CaseCategoryViewModel>> GetCategory(int Id)
        {
            var Category = await _context.CaseCategories.Where(c => c.Id == Id)
                            .Select(c => new CaseCategoryViewModel
                            {
                                Id = c.Id,
                                Cases = c.Cases.Count(),
                                Description = c.Description,
                                Name = c.Name
                            }).FirstOrDefaultAsync();

            if (Category == null)
            {
                return new AppResult<CaseCategoryViewModel>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Case Category not found"
                };
            }

            return new AppResult<CaseCategoryViewModel>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = Category,
                Message = "Category Found"
            };
        }

        /// <summary>
        /// get all incident types
        /// </summary>
        /// <returns></returns>
        public async Task<AppResult<List<CaseCategoryListModel>>> GetCategories()
        {
            var categories = await _context.CaseCategories
                             .Select(c => new CaseCategoryListModel
                             {
                                 Id = c.Id,
                                 Name = c.Name
                             }).ToListAsync();

            return new AppResult<List<CaseCategoryListModel>>
            {
                Data = categories,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successful"
            };
        }

        /// <summary>
        /// updates an incident type
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> UpdateCategory(int Id, CaseCategoryCreationModel model)
        {
            var category = await _context.CaseCategories.FindAsync(Id);

            if (category is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Case Category not found"
                };
            }

            category.Name = model.Name;
            category.Description = model.Description;
            category.ModifiedAt = CurrentDate;

            _context.Update(category);

            await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Case Category Updated Successfully",
                Data = category.Name
            };
        }

        /// <summary>
        /// Adds case by CSO Redundant
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //public async Task<AppResult<string>> AddCaseByCSO(CaseCreationCSOModel model)
        //{
        //    var User = await _context.Users
        //               .Include(c => c.State)
        //               .Include(c => c.Organisation).AsNoTracking().FirstOrDefaultAsync(c => c.Id == UserId);

        // if (User is null) { return new AppResult<string> { StatusCode =
        // StatusCodes.Status404NotFound, Message = "user not found", }; }

        // if (User.Type != "CSO") { return new AppResult<string> { StatusCode =
        // StatusCodes.Status400BadRequest, Message = "Not Added", Errors = { "User must be a CSO" }
        // }; }

        // //if (model.OrganisationId != User.OrganisationId) //{ // return new Result<string> // {
        // // StatusCode = StatusCodes.Status400BadRequest, // Errors = { "Organisation not found or
        // does not match Users organsation" }, // Message = "Creation Failed"

        // // };

        // //}

        // //validate category or incident type if (model.CaseCategoryId != 0) { var category =
        // await _context.CaseCategories.AsNoTracking() .FirstOrDefaultAsync(c => c.Id == model.CaseCategoryId);

        // if (category is null) { return new AppResult<string> { StatusCode =
        // StatusCodes.Status400BadRequest, Errors = { "Incidence Type not found" }, Message =
        // "Creation Failed" }; } } else if (model.CaseCategoryId == 0 &&
        // !string.IsNullOrWhiteSpace(model.OtherCategory)) { //create new Category var newCateogory
        // = new CaseCategory { Name = model.OtherCategory, };

        // _context.Add(newCateogory);

        // await _context.SaveChangesAsync();

        // model.CaseCategoryId = newCateogory.Id; }

        // var entries = await _context.Entries.OrderBy(c => c.Field).ToListAsync();

        // //Adds a new Entry if it doesn't exist in the database

        // //Todo: Also Validate the entries ValidateEntry(entries, model.WhoReportedIncident, Field.WhoReported);

        // ValidateEntry(entries, model.SexOfSurvior, Field.Sex);

        // ValidateEntry(entries, model.TypeOfServiceReceivedBySurvior, Field.TypeOfService);

        // ValidateEntry(entries, model.FollowUpActionByCSO, Field.FollowUpAction);

        // ValidateEntry(entries, model.TypeOfReferralServiceRequired, Field.TypeOfService);

        // ValidateEntry(entries, model.ActualReferralServiceReceived, Field.TypeOfService);

        // ValidateEntry(entries, model.ReferralOutcome, Field.ReferralOutcome);

        // var (IncidentCode, SerialNo) = await GenerateIncidentCode(User.State.Code, User.Organisation.Code);

        // var newCase = new Case { IncidentCode = IncidentCode, SerialNumber = SerialNo,
        // GBV_COVID19_Question1 = model.GBV_COVID19_Question1, GBV_COVID19_Question2 =
        // model.GBV_COVID19_Question2, GBV_COVID19_Question3 = null, GBV_COVID19_Question4 =
        // model.GBV_COVID19_Question4, CanBeEdited = true, CaseCategoryId = model.CaseCategoryId,
        // CreatedById = model.UserId, OrganisationId = model.OrganisationId, IncidentLGAId =
        // model.IncidentLGAId, IncidentStateId = model.IncidentStateId, AgeOfSurvior =
        // model.AgeOfSurvior, TimeOfDay = model.TimeOfDay, IncidentWardId = model.IncidentWardId,
        // WhoReportedIncident = model.WhoReportedIncident, SexOfSurvior = model.SexOfSurvior,
        // TypeOfServiceReceivedBySurvior =
        // JsonConvert.SerializeObject(model.TypeOfServiceReceivedBySurvior), FollowUpActionByCSO =
        // JsonConvert.SerializeObject(model.FollowUpActionByCSO), TypeOfReferralServiceRequired =
        // JsonConvert.SerializeObject(model.TypeOfReferralServiceRequired),
        // ActualReferralServiceReceived =
        // JsonConvert.SerializeObject(model.ActualReferralServiceReceived),
        // HasSurviorReceivedService = model.HasSurviorReceivedService, DateOfIncident =
        // model.DateOfIncident, DateReported = model.DateReported, NameOfServiceProviderReferredTo
        // = model.NameOfServiceProviderReferredTo, OutcomeOfServiceorReferral =
        // model.ReferralOutcome, ReceivingOrganisationCode = model.ReceivingOrganisationCode,
        // WasViolenceFatal = model.WasViolenceFatal, };

        // _context.Add(newCase);

        // await _context.SaveChangesAsync();

        //    return new AppResult<string>
        //    {
        //        StatusCode = StatusCodes.Status201Created,
        //        Data = IncidentCode,
        //        Message = $"Creation Successful, Copy and save the Incident Code Shown  Code: {IncidentCode}"
        //    };
        //}

        public async Task<AppResult<List<string>>> UploadCases(List<CaseCreationSPModel> models)
        {
            var validateUser = await ValidateUser();
            if (validateUser.StatusCode != StatusCodes.Status200OK)
                return new AppResult<List<string>>()
                {
                    StatusCode = validateUser.StatusCode,
                    Message = validateUser.Message
                };

            var returnData = new List<string>();
            var organistations = new List<Organisation>();

            foreach (var model in models.Select((c, index) => new { Case = c, Index = index + 1 }))
            {
                var validateCaseResult = await ValidateCaseBySp(model.Case);
                if (validateCaseResult.IsFailure)
                {
                    returnData.Add("case " + model.Index + ": " + validateCaseResult.Error);
                }
                else
                {
                    organistations.Add(validateCaseResult.Value);
                }
            }

            if (returnData.Any())
            {
                return new AppResult<List<string>>()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Error occured in some cases",
                    Data = returnData
                };
            }

            var serial = (await _context.Cases.OrderBy(c => c.SerialNumber).LastOrDefaultAsync()).SerialNumber + 1;
            foreach (var model in models)
            {
                var organization = organistations.FirstOrDefault(o => o.Id == model.OrganisationId);

                var number = CaseAndSerialNumber(validateUser.Data.State.Code, organization?.Code, serial);

                returnData.Add(number.incidentCode);

                var newCase = await MapRequestModelToCase(model, number.incidentCode, number.serial);

                await _context.AddAsync(newCase);
                serial++;
            }

            await _context.SaveChangesAsync();

            return new AppResult<List<string>>
            {
                StatusCode = StatusCodes.Status201Created,
                Data = returnData,
                Message = "Success"
            };
        }
        #region BulkUpload Implementation
        //public async Task<AppResult<string>> BulkUpload(IFormFile file)
        //{
        //    var result = new AppResult<string>();
        //    var fileName = _configuration.GetSection("BulkUploadSettings:Cases").Value.ToLower();
        //    if (file.FileName.ToLower()!=fileName)
        //    {
        //        result.StatusCode = StatusCodes.Status415UnsupportedMediaType;
        //        result.Message = " Invalid file";
        //        result.AddError("Invalid file");
        //        return result; 
        //    }
        //    //Check user
        //    var validateUser = await ValidateUser();
        //    if (validateUser.StatusCode != StatusCodes.Status200OK)
        //    {
        //        result.StatusCode = validateUser.StatusCode;
        //        result.Message = validateUser.Message;
        //        return result;
        //    }


        //    if (file?.Length > 0)
        //    {
        //        var stream = file.OpenReadStream();
        //        //List<CaseCreationSPModel> cases = new();

        //             //_context.Database.BeginTransaction();

        //        var executionStrategy =  _context.Database.CreateExecutionStrategy();
        //        executionStrategy.Execute(
        //        () =>
        //        {
        //            using var transaction = _context.Database.BeginTransaction();
        //            try
        //            {
        //                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        //                using var package = new ExcelPackage(stream);
        //                var worksheet = package.Workbook.Worksheets.First();
        //                var rowCount = worksheet.Dimension.Rows;
        //                var validRow = GetLastRow(worksheet);
        //                if (validRow == 3)
        //                {
        //                    result.StatusCode = StatusCodes.Status406NotAcceptable;
        //                    result.Message = " Template file is empty";
        //                    result.AddError("Template file is empty");
        //                    return;
        //                }
        //                //var cv = new CaseCreationSPModel();
        //                var isCaseValid = _configuration.GetSection("BulkUploadSettings:CaseValid").Value.ToLower();
        //                var textInFile = worksheet.Cells[3, 2].Value?.ToString().ToLower();
        //                if ( textInFile != isCaseValid ||textInFile==null)
        //                {
        //                    result.StatusCode = StatusCodes.Status415UnsupportedMediaType;
        //                    result.Message = " Invalid file";
        //                    result.AddError("Invalid file");
        //                    return;
        //                }
        //                var user = GetUserAsync(UserId).Result;

        //                var organisation = GetOrganisation(user.OrganisationId.Value).Result;
        //                var lGA = GetLocalGovernmentAreas();
        //                var states = GetStates();
        //                var wards = GetWards();
        //                List<CaseCategoryOrTypeOfViolence> CaseCategories = new();
        //                List<PerpetratorsInformationModel> PerpetratorsInformation = new();
        //                List<Case> listOfCases = new();
        //                for (var row = 4; row <= validRow; row++)
        //                {

        //                    var organisationId = user.OrganisationId.Value;
        //                    var organisationLgaName = worksheet.Cells[row, 2].Value.ToString().ToLower().Trim();
        //                    var organisationLga = GetALocalGovernmentArea(lGA, organisationLgaName).Result;
        //                    var organisationState = GetAState(organisation.Data.States, organisationLga.StateId);
        //                    if (organisationState == null)
        //                    {
        //                        result.StatusCode = StatusCodes.Status406NotAcceptable;
        //                        result.Message = $" case in row {row} : Incorrect local government";
        //                        result.AddError($" case in row {row} : Incorrect local government");
        //                        return;
        //                    }
        //                    //continue here
        //                    var contactChannel = worksheet.Cells[row, 3].Value?.ToString();
        //                    var wasViolenceFatal = worksheet.Cells[row, 4].Value?.ToString().ToLower().Trim() switch
        //                    {
        //                        "yes" => YesOrNo.Yes,
        //                        "no" => YesOrNo.No,
        //                        _ => YesOrNo.NotApplicable,
        //                    };
        //                    var whoReportedIncident = worksheet.Cells[row, 5].Value?.ToString();
        //                    var sexOfSurvivor = worksheet.Cells[row, 6].Value?.ToString();
        //                    var ageOfSurvior = int.Parse(worksheet.Cells[row, 7].Value?.ToString());
        //                    string maritalStatus = null, employmentStatusofSurvivor = null, survivorEstimatedAverageMonthlyIncome = null;
        //                    if (ageOfSurvior > 11)
        //                    {
        //                        maritalStatus = worksheet.Cells[row, 9].Value?.ToString();
        //                        employmentStatusofSurvivor = worksheet.Cells[row, 11].Value?.ToString();
        //                        if (employmentStatusofSurvivor.ToLower() == "currently employed")
        //                        {
        //                            survivorEstimatedAverageMonthlyIncome = worksheet.Cells[row, 16].Value?.ToString();
        //                        }

        //                    }

        //                    var survivorPhoneNumber = worksheet.Cells[row, 8].Value?.ToString();
        //                    var employmentStatusOfParentOrGuardian = worksheet.Cells[row, 10].Value?.ToString();
        //                    var vulnerablePopulation = worksheet.Cells[row, 12].Value?.ToString();
        //                    var education = worksheet.Cells[row, 13].Value?.ToString();
        //                    YesOrNo doesSurviveAlone;
        //                    switch (worksheet.Cells[row, 14].Value.ToString()?.ToLower().Trim())
        //                    {
        //                        case "yes":
        //                            doesSurviveAlone = YesOrNo.Yes;
        //                            break;
        //                        case "no":
        //                            doesSurviveAlone = YesOrNo.No;
        //                            var whoDoesSurviorLiveWith = worksheet.Cells[row, 15].Value?.ToString();
        //                            break;
        //                        default:
        //                            doesSurviveAlone = YesOrNo.NotApplicable;
        //                            break;
        //                    }

        //                    var dateOfIncident = Convert.ToDateTime(worksheet.Cells[row, 17].Value?.ToString());
        //                    var dateReported = Convert.ToDateTime(worksheet.Cells[row, 18].Value?.ToString());
        //                    var timeOfDay = worksheet.Cells[row, 19].Value?.ToString().ToLower().Trim() switch
        //                    {
        //                        "unknown" => TimeOfDay.Unknown,
        //                        "morning" => TimeOfDay.Morning,
        //                        "afternoon" => TimeOfDay.Afternoon,
        //                        "evening" => TimeOfDay.Evening,
        //                        _ => TimeOfDay.CannotRemember,
        //                    };
        //                    var incidentState = worksheet.Cells[row, 20].Value?.ToString().ToLower().Trim();

        //                    var doesStateExist = states.FirstOrDefaultAsync(x => x.Name.ToLower().Trim() == incidentState.ToLower().Trim()).Result;
        //                    if (doesStateExist == null)
        //                    {
        //                        result.StatusCode = StatusCodes.Status406NotAcceptable;
        //                        result.Message = $" case in row {row} : Incorrect Location of Violence (state)";
        //                        result.AddError($" case in row {row} : Incorrect Location of Violence (state)");
        //                        return;
        //                    }
        //                    var incidentStateId = doesStateExist.Id;
        //                    int incidentLGAId;
        //                    var incidentLGA = worksheet.Cells[row, 21].Value?.ToString().ToLower().Trim();
        //                    var doesLGAexistInTheState = GetALocalGovernmentArea(lGA, incidentLGA).Result;
        //                    if (doesLGAexistInTheState.StateId != incidentStateId)
        //                    {
        //                        result.StatusCode = StatusCodes.Status406NotAcceptable;
        //                        result.Message = $" case in row {row} : Incorrect Location of Violence (l.g.a)";
        //                        result.AddError($" case in row {row} : Incorrect Location of Violence (l.g.a)");
        //                        return;
        //                    }
        //                    incidentLGAId = doesLGAexistInTheState.Id;
        //                    int incidentWardId;
        //                    var incidentWard = worksheet.Cells[row, 22].Value?.ToString().ToLower().Trim();
        //                    var doesWardExistInLga = wards.AsNoTracking().FirstOrDefaultAsync(x => x.Name.ToLower().Trim() == incidentWard).Result;
        //                    if (doesWardExistInLga.LocalGovernmentAreaId != incidentLGAId)
        //                    {
        //                        result.StatusCode = StatusCodes.Status406NotAcceptable;
        //                        result.Message = $" case in row {row} : Incorrect Location of Violence (ward)";
        //                        result.AddError($" case in row {row} : Incorrect Location of Violence (ward)");
        //                        return;
        //                    }
        //                    incidentWardId = doesWardExistInLga.Id;
        //                    var actualLocationOfIncident = worksheet.Cells[row, 23].Value?.ToString();

        //                    if (worksheet.Cells[row, 24].Value.ToString().ToLower().Trim() == "yes")
        //                        CaseCategories.Add(CaseCategoryOrTypeOfViolence.PhysicalAssault);
        //                    if (worksheet.Cells[row, 25].Value.ToString().ToLower().Trim() == "yes")
        //                        CaseCategories.Add(CaseCategoryOrTypeOfViolence.Defilement);
        //                    if (worksheet.Cells[row, 26].Value.ToString().ToLower().Trim() == "yes")
        //                        CaseCategories.Add(CaseCategoryOrTypeOfViolence.Rape);
        //                    if (worksheet.Cells[row, 27].Value.ToString().ToLower().Trim() == "yes")
        //                        CaseCategories.Add(CaseCategoryOrTypeOfViolence.ForcedMarriage);
        //                    if (worksheet.Cells[row, 28].Value.ToString().ToLower().Trim() == "yes")
        //                        CaseCategories.Add(CaseCategoryOrTypeOfViolence.DenialOfResourcesOrServices);
        //                    if (worksheet.Cells[row, 29].Value.ToString().ToLower().Trim() == "yes")
        //                        CaseCategories.Add(CaseCategoryOrTypeOfViolence.EmotionalOrPsychological);
        //                    if (worksheet.Cells[row, 30].Value.ToString().ToLower().Trim() == "yes")
        //                        CaseCategories.Add(CaseCategoryOrTypeOfViolence.SexualAssault);
        //                    if (worksheet.Cells[row, 31].Value.ToString().ToLower().Trim() == "yes")
        //                        CaseCategories.Add(CaseCategoryOrTypeOfViolence.FemaleGenitalMutilation);
        //                    if (worksheet.Cells[row, 32].Value.ToString().ToLower().Trim() == "yes")
        //                        CaseCategories.Add(CaseCategoryOrTypeOfViolence.ViolationOfPropertyAndInheritanceRight);
        //                    if (worksheet.Cells[row, 33].Value.ToString().ToLower().Trim() == "yes")
        //                        CaseCategories.Add(CaseCategoryOrTypeOfViolence.ChildAbuseAndNeglect);
        //                    if (worksheet.Cells[row, 34].Value.ToString().ToLower().Trim() == "yes")
        //                        CaseCategories.Add(CaseCategoryOrTypeOfViolence.EarlyMarriage);
        //                    if (worksheet.Cells[row, 35].Value.ToString().ToLower().Trim() == "yes")
        //                        CaseCategories.Add(CaseCategoryOrTypeOfViolence.OnlineOrCyberBullying);
        //                    var caseCategoriesOther = worksheet.Cells[row, 36].Value?.ToString();
        //                    var isSurviorContinuousThreat = worksheet.Cells[row, 37].Value?.ToString().ToLower().Trim() switch
        //                    {
        //                        "yes" => YesOrNo.Yes,
        //                        "no" => YesOrNo.No,
        //                        _ => YesOrNo.NotApplicable,
        //                    };

        //                    var numberOfPerpetrators = worksheet.Cells[row, 38].Value?.ToString().ToLower();
        //                    //int numberOfPerps;

        //                    if (numberOfPerpetrators != "don't know")
        //                    {
        //                        //numberOfPerps= int.Parse(numberOfPerpetrators);
        //                        var sexOfPerpOne = worksheet.Cells[row, 39].Value?.ToString();
        //                        if (sexOfPerpOne != null)
        //                        {
        //                            PerpetratorsInformationModel perp1 = new()
        //                            {
        //                                SexOfPerpetrator = sexOfPerpOne,
        //                                AgeOfPerpetrator = int.Parse(worksheet.Cells[row, 40].Value?.ToString()),
        //                                SurviorRelationWithPerpetrator = worksheet.Cells[row, 41].Value?.ToString()
        //                            };
        //                            PerpetratorsInformation.Add(perp1);
        //                        }
        //                        var sexOfPerpTwo = worksheet.Cells[row, 42].Value?.ToString();
        //                        if (sexOfPerpTwo != null)
        //                        {
        //                            PerpetratorsInformationModel perp2 = new()
        //                            {
        //                                SexOfPerpetrator = sexOfPerpTwo,
        //                                AgeOfPerpetrator = int.Parse(worksheet.Cells[row, 43].Value?.ToString()),
        //                                SurviorRelationWithPerpetrator = worksheet.Cells[row, 44].Value?.ToString()
        //                            };
        //                            PerpetratorsInformation.Add(perp2);
        //                        }
        //                        var sexOfPerpThree = worksheet.Cells[row, 45].Value?.ToString();
        //                        if (sexOfPerpThree != null)
        //                        {
        //                            PerpetratorsInformationModel perp3 = new()
        //                            {
        //                                SexOfPerpetrator = sexOfPerpThree,
        //                                AgeOfPerpetrator = int.Parse(worksheet.Cells[row, 46].Value?.ToString()),
        //                                SurviorRelationWithPerpetrator = worksheet.Cells[row, 47].Value?.ToString()
        //                            };
        //                            PerpetratorsInformation.Add(perp3);
        //                        }
        //                    }

        //                    YesOrNo doestheSurviorWantJustice, hasCaseBeenClosed = YesOrNo.NotApplicable;
        //                    string outcomeOfProsecution = null, caseClosed = null, whoClosedTheCase = null;
        //                    DateTime? dateJusticeReceived = null, caseClosedDate = null;
        //                    switch (worksheet.Cells[row, 48].Value.ToString().ToLower().Trim())
        //                    {
        //                        case "yes":
        //                            doestheSurviorWantJustice = YesOrNo.Yes;
        //                            outcomeOfProsecution = worksheet.Cells[row, 49].Value?.ToString();
        //                            dateJusticeReceived = Convert.ToDateTime(worksheet.Cells[row, 50].Value?.ToString());
        //                            caseClosed = worksheet.Cells[row, 51].Value?.ToString().ToLower();


        //                            if (caseClosed == "yes")
        //                            {
        //                                whoClosedTheCase = worksheet.Cells[row, 52].Value?.ToString();
        //                                caseClosedDate = Convert.ToDateTime(worksheet.Cells[row, 53].Value?.ToString());
        //                                hasCaseBeenClosed = YesOrNo.Yes;
        //                            }
        //                            if (caseClosed == "no")
        //                                hasCaseBeenClosed = YesOrNo.No;
        //                            break;
        //                        case "no":
        //                            doestheSurviorWantJustice = YesOrNo.No;
        //                            break;
        //                        default:
        //                            doestheSurviorWantJustice = YesOrNo.NotApplicable;
        //                            hasCaseBeenClosed = YesOrNo.NotApplicable;
        //                            break;
        //                    }
        //                    var covidQuestion = worksheet.Cells[row, 54].Value?.ToString().ToLower();
        //                    string gbV_COVID19_Question1 = null, gbV_COVID19_Question2 = null, gbV_COVID19_Question3 = null, gbV_COVID19_Question4 = null;
        //                    if (covidQuestion == "yes")
        //                    {
        //                        gbV_COVID19_Question1 = worksheet.Cells[row, 55].Value?.ToString().ToLower();
        //                        gbV_COVID19_Question2 = worksheet.Cells[row, 56].Value?.ToString().ToLower();
        //                        gbV_COVID19_Question3 = worksheet.Cells[row, 57].Value?.ToString().ToLower();
        //                        gbV_COVID19_Question4 = worksheet.Cells[row, 58].Value?.ToString().ToLower();
        //                    }


        //                    CaseCreationSPModel model = new()
        //                    {
        //                        OrganisationId = organisationId,
        //                        OrganisationLgaId = organisationLga.Id,
        //                        ContactChannel = contactChannel,
        //                        WasViolenceFatal = wasViolenceFatal,
        //                        WhoReportedIncident = whoReportedIncident,
        //                        SexOfSurvior = sexOfSurvivor,
        //                        AgeOfSurvior = ageOfSurvior,
        //                        MaritalStatus = maritalStatus,
        //                        EmploymentStatus = employmentStatusofSurvivor,
        //                        SurvivorEstimatedAverageMonthlyIncome = survivorEstimatedAverageMonthlyIncome,
        //                        SurvivorMobileNo = survivorPhoneNumber,
        //                        EmploymentStatusOfParentOrGuardian = employmentStatusOfParentOrGuardian,
        //                        VulnerablePopulation = vulnerablePopulation,
        //                        Education = education,
        //                        DoesSurviorLiveAlone = doesSurviveAlone,
        //                        DateOfIncident = dateOfIncident,
        //                        DateReported = dateReported,
        //                        TimeOfDay = timeOfDay,
        //                        IncidentStateId = incidentStateId,
        //                        IncidentLGAId = incidentLGAId,
        //                        IncidentWardId = incidentWardId,
        //                        ActualLocationOfIncident = actualLocationOfIncident,
        //                        CaseCategories = CaseCategories,
        //                        CaseCategoriesOther = caseCategoriesOther,
        //                        IsSurviorContinuousThreat = isSurviorContinuousThreat,
        //                        NumberOfPerpetrators = numberOfPerpetrators,
        //                        PerpetratorsInformation = PerpetratorsInformation,
        //                        DoestheSurviorWantJustice = doestheSurviorWantJustice,
        //                        OutcomeOfProsecution = outcomeOfProsecution,
        //                        DateJusticeReceived = dateJusticeReceived,
        //                        HasCaseBeenClosed = hasCaseBeenClosed,
        //                        WhoClosedTheCase = whoClosedTheCase,
        //                        CaseClosedDate = caseClosedDate,
        //                        GBV_COVID19_Question1 = gbV_COVID19_Question1,
        //                        GBV_COVID19_Question2 = gbV_COVID19_Question2,
        //                        GBV_COVID19_Question3 = gbV_COVID19_Question3,
        //                        GBV_COVID19_Question4 = gbV_COVID19_Question4,
        //                        UserId = UserId

        //                    };

        //                    var validateCaseResult = ValidateCaseBySp(model).Result;
        //                    if (validateCaseResult.IsFailure)
        //                    {
        //                        result.StatusCode = StatusCodes.Status400BadRequest;
        //                        result.Message = $" case in row {row} : {validateCaseResult.Error}";
        //                        result.AddError($" case in row {row} : {validateCaseResult.Error}");
        //                        return;
        //                    };

        //                    var (incidentCode, serialNo) = GenerateIncidentCode(validateUser.Data.State.Code, validateCaseResult.Value.Code).Result;

        //                    var newCase = MapRequestModelToCase(model, incidentCode, serialNo).Result;

        //                    listOfCases.Add(newCase);
        //                    PerpetratorsInformation.Clear();
        //                    CaseCategories.Clear();

        //                    //await _context.AddAsync(newCase);

        //                    //var status = await _context.SaveChangesAsync() > 0;

        //                    //if (status)
        //                    //{
        //                    //    _queue.QueueInvocable<ClearDashboardCacheInvocable>();
        //                    //}

        //                    //return new AppResult<string>
        //                    //{
        //                    //    StatusCode = StatusCodes.Status201Created,
        //                    //    Data = newCase.IncidentCode,
        //                    //    Message = $"Creation Successful, Copy and save the Incident Code Shown  Code: {newCase.IncidentCode}"
        //                    //};
        //                    //cases.Add(newCase);


        //                }


        //                if (listOfCases.Count == 0)
        //                {
        //                    result.StatusCode = StatusCodes.Status406NotAcceptable;
        //                    result.Message = "An error occured while uploading data";
        //                    result.AddError("An error occured while uploading data");
        //                }
        //                //var result=await UploadCases(cases);
        //                _context.Cases.BulkInsertAsync(listOfCases);
        //                _context.SaveChanges();
        //                transaction.Commit();
        //                result.StatusCode = StatusCodes.Status201Created;
        //                result.Message = $"Success";
        //            }
        //            catch (Exception ex)
        //            {
        //                transaction.Rollback();
        //                result.StatusCode = StatusCodes.Status406NotAcceptable;
        //                result.Message = "An error occured while uploading data";
        //                result.AddError(ex.InnerException.Message);
        //            }

        //            if(result.HasError)
        //            {
        //               transaction.Rollback();
        //            }
        //        });
        //    }
        //    else
        //    {
        //        result.StatusCode = StatusCodes.Status415UnsupportedMediaType;
        //        result.Message = " Invalid file";
        //        result.AddError("Invalid file");
        //    }

        //    return result;

        //}
        #endregion
        private async Task<Result<Organisation>> ValidateCaseBySp(CaseCreationSPModel model)
        {
            if (model.DateReported < new DateTime(2020, 4, 1))
            {
                return Result.Failure<Organisation>("Incident date cannot be earlier than April 2020");
                //Cases that happened before apr 2020, should not be allowed to be submitted
            }

            if (!(await _setting.GetSettings()).Data.AllowPrevMonthCases)
            {
                if (model.DateOfIncident.Month < DateTime.Now.Month)
                {
                    return Result.Failure<Organisation>("You can only add case for this month");
                }
            }

            var organisation = await _context.Organisations.AsNoTracking().FirstOrDefaultAsync(c => c.Id == model.OrganisationId);

            if (organisation is null)
            {
                return Result.Failure<Organisation>("Organisation not found");
            }

            ////validate category or incident type
            //if (model.CaseCategoryId != 0)
            //{
            //    var category = await _context.CaseCategories.AsNoTracking()
            //                              .FirstOrDefaultAsync(c => c.Id == model.CaseCategoryId);

            //    if (category is null)
            //    {
            //        return Result.Failure<Organisation>("Incidence Type not found");
            //    }
            //}

            //if (model.CaseCategoryId == 0 && string.IsNullOrWhiteSpace(model.OtherCategory))
            //{
            //    return Result.Failure<Organisation>("Incident Type not provided");
            //}

            return Result.Success(organisation);
        }

        private async Task<AppResult<ApplicationUser>> ValidateUser()
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return new AppResult<ApplicationUser>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized"
                };
            }

            var user = await _context.Users
                .Include(c => c.State)
                .AsNoTracking().FirstOrDefaultAsync(c => c.Id == UserId);

            if (user is null)
            {
                return new AppResult<ApplicationUser>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User not found",
                };
            }

            return new AppResult<ApplicationUser>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "User not found",
                Data = user
            };
        }

        private async Task<Case> MapRequestModelToCase(CaseCreationSPModel model, string incidentCode, int serialNo)
        {
            //if (model.CaseCategoryId == 0 && !string.IsNullOrWhiteSpace(model.OtherCategory))
            //{
            //    //create new Category

            // var newCateogory = new CaseCategory { Name = model.OtherCategory, };

            // _context.Add(newCateogory);

            // await _context.SaveChangesAsync();

            //    model.CaseCategoryId = newCateogory.Id;
            //}

            if (model.IncidentWardId.HasValue && model.IncidentWardId.Value == 0)
            {
                model.IncidentWardId = !_configuration.GetValue<bool>(StartupKeys.IsLive) ? MetricsKeys.DevUnknown : MetricsKeys.LiveUnknown;
            }

            return new Case(
              incidentCode,
              serialNo,
              model.ContactChannel,
              model.WhoReportedIncident,
              model.AgeOfSurvior,
              model.SexOfSurvior,
              model.MaritalStatus,
              model.EmploymentStatus,
              model.EmploymentStatusOfParentOrGuardian,
              model.VulnerablePopulation is null ? "" : JsonConvert.SerializeObject(model.VulnerablePopulation),
              model.Education,
              model.DoesSurviorLiveAlone.GetValueOrDefault(),
              model.WhoDoesSurviorLiveWith,
              model.ActualLocationOfIncident,
              model.DateOfIncident,
              model.DateReported,
              model.TimeOfDay,
              model.HasSurviorReceivedService.GetValueOrDefault(),
              model.WasViolenceFatal.GetValueOrDefault(),
              model.TypeOfServiceReceivedBySurvior is null ? "" : JsonConvert.SerializeObject(model.TypeOfServiceReceivedBySurvior),
              model.TypeOfServiceProvidedToSurvior is null ? "" : JsonConvert.SerializeObject(model.TypeOfServiceProvidedToSurvior),
              model.ActualReferralServiceReceived is null ? "" : JsonConvert.SerializeObject(model.ActualReferralServiceReceived),
              model.NameOfServiceProviderReferredTo,
              model.OutcomeOfSerivce,
              model.ReceivingOrganisationCode,
           model.PerpetratorsInformation,
              model.IsSurviorContinuousThreat.GetValueOrDefault(),
              model.NumberOfPerpetrators,
              model.DoestheSurviorWantJustice.GetValueOrDefault(),
              model.GBV_COVID19_Question1,
              model.GBV_COVID19_Question2,
              model.GBV_COVID19_Question3,
              model.GBV_COVID19_Question4,
              model.HasCaseBeenClosed.GetValueOrDefault(),
              model.CaseCategories,
              model.CaseCategoriesOther,
              //model.CaseCategoryId,
              model.IncidentStateId,
              model.IncidentLGAId,
              model.IncidentWardId,
              UserId,
              model.OrganisationId,
              model.WhoClosedTheCase,
              model.Longitude,
              model.Latitude,
              model.ContactChannelOrganisation,
              model.ContactChannelOrganisationService is null ? "" : JsonConvert.SerializeObject(model.ContactChannelOrganisationService),
              model.OtherServiceProviderName,
              model.OtherServiceProviderAddress,
              model.OtherServiceProviderIncidentCode,
              model.ContactChannelOrganisationIncidentCode,
              model.CaseClosedDate,
              //model.SurviorDoesNotWantJusticeReasons is null ? "" : JsonConvert.SerializeObject(model.SurviorDoesNotWantJusticeReasons),
              model.ReferralOutcome,
              model.SurvivorEstimatedAverageMonthlyIncome,
              model.OutcomeOfProsecution,
              model.OrganisationLgaId,
              model.SurvivorMobileNo,
              //model.SurvivorName,
              model.DateJusticeReceived);
        }

        /// <summary>
        /// Adds case to the database
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> AddCaseBySP(CaseCreationSPModel model)
        {
            var validateCaseResult = await ValidateCaseBySp(model);
            if (validateCaseResult.IsFailure)
                return new AppResult<string>()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = validateCaseResult.Error
                };

            var validateUser = await ValidateUser();
            if (validateUser.StatusCode != StatusCodes.Status200OK)
                return new AppResult<string>()
                {
                    StatusCode = validateUser.StatusCode,
                    Message = validateUser.Message
                };

            //var entries = await _context.Entries.OrderBy(c => c.Field).ToListAsync();

            //// Adds a new Entry if it doesn't exist in the database

            //ValidateEntry(entries, model.WhoReportedIncident, Field.WhoReported);

            //ValidateEntry(entries, model.SexOfSurvior, Field.Sex);

            ////ValidateEntry(entries, model.SexOfPerpetrator, Field.Sex);

            //ValidateEntry(entries, model.TypeOfServiceReceivedBySurvior, Field.TypeOfService);
            //ValidateEntry(entries, model.TypeOfServiceProvidedToSurvior, Field.TypeOfService);
            //ValidateEntry(entries, model.ActualLocationOfIncident, Field.IncidentLocation);
            //ValidateEntry(entries, model.ActualReferralServiceReceived, Field.TypeOfService);

            ////ValidateEntry(entries, model.SurviorRelationWithPerpetrator, Field.Relationship);

            //ValidateEntry(entries, model.OutcomeOfSerivce, Field.ServiceOutcome);

            //ValidateEntry(entries, model.Education, Field.Education);

            //ValidateEntry(entries, model.VulnerablePopulation, Field.VulnerablePopulation);

            //ValidateEntry(entries, model.EmploymentStatusOfParentOrGuardian, Field.EmploymentStatus);
            //ValidateEntry(entries, model.EmploymentStatus, Field.EmploymentStatus);

            //ValidateEntry(entries, model.MaritalStatus, Field.MaritalStatus);

            //checks if the ward is unknown and asignment the values

            var (incidentCode, serialNo) = await GenerateIncidentCode(validateUser.Data.State.Code, validateCaseResult.Value.Code);


            var newCase = await MapRequestModelToCase(model, incidentCode, serialNo);

            await _context.AddAsync(newCase);

            var status = await _context.SaveChangesAsync() > 0;

            if (status)
            {
                _queue.QueueInvocable<ClearDashboardCacheInvocable>();
            }

            return new AppResult<string>
            {
                StatusCode = StatusCodes.Status201Created,
                Data = newCase.IncidentCode,
                Message = $"Creation Successful, Copy and save the Incident Code Shown  Code: {newCase.IncidentCode}"
            };
        }

        /// <summary>
        /// Adds and entry type Redundant
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> AddEntry(List<EntryModel> model)
        {
            var key = 1;

            foreach (var n in model)
            {
                var newEntry = new Entry
                {
                    Key = key.ToString(),
                    Field = n.Field,
                    Type = EntryType.Listed,
                    Value = n.Value,
                };

                _context.Add(newEntry);

                key++;
            }

            await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                Data = key.ToString(),
                StatusCode = StatusCodes.Status201Created
            };
        }

        /// <summary>
        /// gets all entry types
        /// </summary>
        /// <returns></returns>
        public async Task<AppResult<List<EntryModel>>> GetEntries()
        {
            var entires = await _context.Entries
                           .OrderBy(c => c.Field)
                           .Select(c => new EntryModel
                           {
                               Value = c.Value,
                               Field = c.Field,
                               Type = c.Type,
                           }).ToListAsync();

            return new AppResult<List<EntryModel>>
            {
                Data = entires,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successful"
            };
        }

        /// <summary>
        /// gets all incidents entered
        /// </summary>
        /// <param name="model">filter cases</param>
        /// <returns></returns>
        public async Task<AppResult<PaginatedList<CaseViewModel>>> GetAllCases(CaseSearchModel model)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return new AppResult<PaginatedList<CaseViewModel>>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized"
                };
            }

            var user = await _context.Users
                .AsNoTracking()
                       .Select(c => new { c.Id, c.Type, c.OrganisationId, c.State, c.LocalGovernments })
                       .FirstOrDefaultAsync(c => c.Id == UserId);

            if (user is null)
            {
                return new AppResult<PaginatedList<CaseViewModel>>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "User not found"
                };
            }

            var query = _context.Cases.AsQueryable();

            query = SortCasesBy(query, model.SortByDate, model.StartDate, model.EndDate);

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
                                       || user.Type == RoleKeys.ServiceProvider))
            {
                query = query.Where(c => c.IncidentCode.ToLower().Contains(user.State.Code.ToLower()) || c.IncidentState.Id == user.State.Id
                                         || c.CreatedById == user.Id);

                //check is user is a local government access
                if (user.LocalGovernments.Any() && user.Type == RoleKeys.LocalGovernment)
                {
                    var userLgas = string.Join(",", user.LocalGovernments.Select(l => $",{l.Id},"));
                    query = query.Where(c => userLgas.Contains("," + c.IncidentLGAId + ",") || c.CreatedById == user.Id);
                }
            }

            if (user.State != null && (user.Type == RoleKeys.CSOSupervior
                                    || user.Type == RoleKeys.ServiceProviderSupervior
                                    || user.Type == RoleKeys.StateSupervisor
                                    || user.Type == RoleKeys.StateAdministrator))//No matter the state where the incident occurred, it should be validated in the state where it was reported
            {
                query = query.Where(c => c.IncidentCode.ToLower().Contains(user.State.Code.ToLower()));
            }
            else
            {
                if (model.StateId.HasValue && model.StateId.Value != 0)
                {
                    query = query.Where(c => c.IncidentStateId == model.StateId || c.CreatedById == user.Id);
                }
            }

            if (!string.IsNullOrWhiteSpace(model.ValidateByRole))
            {
                query = query.Where(c => c.IsValidated && c.ValidatedBy.Type == model.ValidateByRole);
            }

            if (model.OrganisationType.HasValue)
            {
                query = query.Where(c => c.Organisation.OrganisationType == model.OrganisationType);
            }

            if (model.OrganisationId.HasValue && model.OrganisationId.Value != 0)
            {
                query = query.Where(c => c.OrganisationId == model.OrganisationId);
            }


            if (model.IncidentLGAId.HasValue && model.IncidentLGAId.Value != 0)
            {
                query = query.Where(c => c.IncidentLGAId == model.IncidentLGAId);
            }

            if (model.TimeOfDay.HasValue)
            {
                query = query.Where(c => c.TimeOfDay == model.TimeOfDay);
            }

            //if (model.CaseCategoryId.HasValue && model.CaseCategoryId.Value != 0)
            //{
            //    query = query.Where(c => c.CaseCategoryId == model.CaseCategoryId);
            //}

            if (!string.IsNullOrWhiteSpace(model.IncidentCode))
            {
                query = query.Where(c => c.IncidentCode.Trim().ToLower() == model.IncidentCode.Trim().ToLower());
            }

            if (model.IsCaseClosed.HasValue && model.IsCaseClosed.Value != YesOrNo.NotApplicable)
            {
                query = query.Where(c => c.HasCaseBeenClosed == model.IsCaseClosed);
            }
            if (model.IsApproved.HasValue)
            {
                query = query.Where(c => c.IsApproved == model.IsApproved.Value);

                if (model.IsRejected.HasValue && model.IsRejected.Value && model.IsApproved.Value)
                {
                    query = query.Where(c => c.ApprovedById != null);
                }
            }

            if (model.IsValidated.HasValue)
            {
                query = query.Where(c => c.IsValidated == model.IsValidated.Value);

                if (model.IsRejected.HasValue && model.IsRejected.Value && model.IsValidated.Value)
                {
                    query = query.Where(c => c.ValidatedById != null);
                }
            }

            if (model.IsLgaValidated.HasValue)
            {
                query = query.Where(c => c.LgaValidated == model.IsLgaValidated);
            }

            if (!string.IsNullOrWhiteSpace(model.Gender))
            {
                query = query.Where(c => c.SexOfSurvior.Trim().ToLower() == model.Gender.Trim().ToLower());
            }

            if (model.MinimumAge.HasValue)
            {
                query = query.Where(c => c.AgeOfSurvior >= model.MinimumAge);
            }
            if (model.MaximumAge.HasValue)
            {
                query = query.Where(c => c.AgeOfSurvior <= model.MaximumAge);
            }

            if (!string.IsNullOrWhiteSpace(model.TypeOfServiceProvided))
            {
                query = query.Where(c => c.TypeOfServiceProvidedToSurvior.ToLower().Contains(model.TypeOfServiceProvided.Trim().ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(model.VulnerablePopulation))
            {
                query = query.Where(c => c.VulnerablePopulation.ToLower().Contains(model.VulnerablePopulation.Trim().ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(model.Organisation))
            {
                query = query.Where(c => c.Organisation.Name.ToLower().Contains(model.Organisation.ToLower()));
            }


            {

                if (model.ValidationStatus.HasValue)
                {
                    if (model.ValidationStatus.Value == ValidationStatus.Submitted)
                    {
                        query = query.Where(c => !c.IsValidated && c.ValidatedById == null);
                    }
                    else if (model.ValidationStatus.Value == ValidationStatus.SpOrCso)
                    {
                        query = query.Where(c => c.IsValidated && !c.IsApproved && c.ApprovedById == null);
                    }
                    else if (model.ValidationStatus.Value == ValidationStatus.SpOrCsoRejected)
                    {
                        query = query.Where(c => !c.IsValidated && c.ValidatedById != null);
                    }
                    else if (model.ValidationStatus.Value == ValidationStatus.State)
                    {
                        query = query.Where(c => c.IsValidated && c.IsApproved);
                    }
                    else if (model.ValidationStatus.Value == ValidationStatus.StateRejected)
                    {
                        query = query.Where(c => c.IsValidated && !c.IsApproved && c.ApprovedById != null);
                    }
                }

            }

            var selected = query.Select(c => new CaseViewModel
            {
                Id = c.Id,
                IncidentCode = c.IncidentCode,
                //CaseCategoryId = c.CaseCategoryId,
                CaseCategories = c.CaseCategoriesList,
                CaseCategoriesOthers = c.CaseCategoriesOthers,
                ActualLocationOfIncident = c.ActualLocationOfIncident,
                ActualReferralServiceReceived = string.IsNullOrWhiteSpace(c.ActualReferralServiceReceived)
                   ? null
                   : JsonConvert.DeserializeObject<List<string>>(c.ActualReferralServiceReceived),
                PerpetratorsInformationList = c.PerpetratorsInformationList,
                AgeOfSurvior = c.AgeOfSurvior,
                CanBeEdited = c.CanBeEdited,
                Category = c.Category.Name,
                ContactChannel = c.ContactChannel,
                CreatedById = c.CreatedById,
                CreatedByName = $"{c.CreatedBy.FirstName} {c.CreatedBy.LastName}",
                DateOfIncident = c.DateOfIncident,
                DateReported = c.DateReported,
                DoesSurviorLiveAlone = c.DoesSurviorLiveAlone,
                DoestheSurviorWantJustice = c.DoestheSurviorWantJustice,
                Education = c.Education,
                EmploymentStatus = c.EmploymentStatus,
                EmploymentStatusOfParentOrGuardian = c.EmploymentStatusOfParentOrGuardian,
                FollowUpActionByCSO = string.IsNullOrWhiteSpace(c.FollowUpActionByCSO)
                   ? null
                   : JsonConvert.DeserializeObject<List<string>>(c.FollowUpActionByCSO),
                GBV_COVID19_Question1 = c.GBV_COVID19_Question1,
                GBV_COVID19_Question2 = c.GBV_COVID19_Question2,
                GBV_COVID19_Question3 = c.GBV_COVID19_Question3,
                GBV_COVID19_Question4 = c.GBV_COVID19_Question4,
                HasCaseBeenClosed = c.HasCaseBeenClosed,
                HasSurviorReceivedService = c.HasSurviorReceivedService,
                IncidentLGA = c.IncidentLGA.Name,
                IncidentLGAId = c.IncidentLGAId,
                IncidentState = c.IncidentState.Name,
                IncidentStateId = c.IncidentStateId,

                //complex check if incident ward is unknown
                IncidentWardId = !c.IncidentWardId.HasValue ? c.IncidentWardId :
                    !_configuration.GetValue<bool>(StartupKeys.IsLive) && c.IncidentWardId == MetricsKeys.DevUnknown ? 0 :
                    _configuration.GetValue<bool>(StartupKeys.IsLive) && c.IncidentWardId == MetricsKeys.LiveUnknown ? 0 : c.IncidentWardId,
                IncidentWard = c.IncidentWardId.HasValue ? c.IncidentWard.Name : null,
                IsSurviorContinuousThreat = c.IsSurviorContinuousThreat,
                MaritalStatus = c.MaritalStatus,
                NameOfServiceProviderReferredTo = c.NameOfServiceProviderReferredTo,
                NumberOfPerpetrators = c.NumberOfPerpetrators,
                Organisation = c.Organisation.Name,
                OrganisationId = c.OrganisationId,
                OrganisationType = c.Organisation.OrganisationType,
                OutcomeOfServiceorReferral = c.OutcomeOfServiceorReferral,
                SerialNumber = c.SerialNumber,
                SexOfSurvior = c.SexOfSurvior,
                TimeOfDay = c.TimeOfDay,
                TypeOfReferralServiceRequired = string.IsNullOrWhiteSpace(c.TypeOfReferralServiceRequired)
                   ? null
                   : JsonConvert.DeserializeObject<List<string>>(c.TypeOfReferralServiceRequired),
                TypeOfServiceProvidedToSurvior = string.IsNullOrWhiteSpace(c.TypeOfServiceProvidedToSurvior)
                   ? null
                   : JsonConvert.DeserializeObject<List<string>>(c.TypeOfServiceProvidedToSurvior),
                TypeOfServiceReceivedBySurvior = string.IsNullOrWhiteSpace(c.TypeOfServiceReceivedBySurvior)
                   ? null
                   : JsonConvert.DeserializeObject<List<string>>(c.TypeOfServiceReceivedBySurvior),
                VulnerablePopulation = string.IsNullOrWhiteSpace(c.VulnerablePopulation)
                   ? null
                   : JsonConvert.DeserializeObject<List<string>>(c.VulnerablePopulation),
                WasViolenceFatal = c.WasViolenceFatal,
                WhoDoesSurviorLiveWith = c.WhoDoesSurviorLiveWith,
                WhoReportedIncident = c.WhoReportedIncident,
                IsApproved = c.IsApproved,
                IsValidated = c.IsValidated,
                ValidatedAt = c.ValidatedAt,
                ApprovedAt = c.ApprovedAt,
                ValidatedById = c.ValidatedById,
                ValidatedByName = string.IsNullOrWhiteSpace(c.ValidatedById)
                   ? null
                   : $"{c.ValidatedBy.FirstName} {c.ValidatedBy.LastName}",
                ApprovedById = c.ApprovedById,
                ApprovedByName = string.IsNullOrWhiteSpace(c.ApprovedById)
                   ? null
                   : $"{c.ApprovedBy.FirstName} {c.ApprovedBy.LastName}",
                Latitude = c.Latitude,
                Longitude = c.Longitude,
                WhoClosedTheCase = c.WhoClosedTheCase,
                UserState = c.CreatedBy.State.Name,
                CreatedAt = c.CreatedAt,
                ModifiedAt = c.ModifiedAt,
                ContactChannelOrganisation = c.ContactChannelOrganisation,
                ContactChannelOrganisationService = string.IsNullOrWhiteSpace(c.ContactChannelOrganisationService)
                   ? null
                   : JsonConvert.DeserializeObject<List<string>>(c.ContactChannelOrganisationService),
                OtherServiceProviderName = c.OtherServiceProviderName,
                OtherServiceProviderAddress = c.OtherServiceProviderAddress,
                OtherServiceProviderIncidentCode = c.OtherServiceProviderIncidentCode,
                ContactChannelOrganisationIncidentCode = c.ContactChannelOrganisationIncidentCode,
                CaseClosedDate = c.CaseClosedDate,
                SurviorDoesNotWantJusticeReasons = string.IsNullOrWhiteSpace(c.SurviorDoesNotWantJusticeReasons)
                   ? null
                   : JsonConvert.DeserializeObject<List<string>>(c.SurviorDoesNotWantJusticeReasons),
                ReceivingOrganisationCode = c.ReceivingOrganisationCode,
                SurvivorEstimatedAverageMonthlyIncome = c.SurvivorEstimatedAverageMonthlyIncome,
                OutcomeOfProsecution = c.OutcomeOfProsecution,
                OrganisationLgaId = c.OrganisationLgaId,
                NotesList = c.NotesList,
                Stage = c.Stage,
                //SurvivorName = c.SurvivorName,
                SurvivorMobileNo = c.SurvivorMobileNo,
                DateJusticeReceived = c.DateJusticeReceived
            });

            var cases = await selected.PageAsync(model.PageIndex, model.PageSize);

            return new AppResult<PaginatedList<CaseViewModel>>
            {
                Data = cases,
                Message = "Successful",
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<AppResult<CaseViewModel>> GetCaseWithIncidentCode(CaseIncidentCode model)
        {
            var resultModel = new AppResult<CaseViewModel>();
                       
            try
            {
                var incidentCase = await _context.Cases.FirstOrDefaultAsync(x => x.IncidentCode.ToLower().Equals(model.IncidentCode.ToLower()));
                if (incidentCase == null)
                {
                    resultModel.AddError("Incorrect incident code");
                    resultModel.Message = "Incorrect incident code";
                    return resultModel;
                }

                var result = new CaseViewModel
                {
                    IncidentCode = incidentCase.IncidentCode,
                    Id = incidentCase.Id,
                    AgeOfSurvior = incidentCase.AgeOfSurvior,
                    SexOfSurvior = incidentCase.SexOfSurvior,
                    VulnerablePopulation = string.IsNullOrWhiteSpace(incidentCase.VulnerablePopulation)
                   ? null
                   : JsonConvert.DeserializeObject<List<string>>(incidentCase.VulnerablePopulation),
                };

                resultModel.Data = result;
                resultModel.Message = "Successful";
            }
            catch (Exception ex)
            {
                resultModel.AddError(ex.Message);
                return resultModel;
            }

            return resultModel;
        }

        /// <summary>
        /// updates cases by SP
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> UpdateCaseBySP(int Id, CaseCreationSPModel model)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "user authorized"
                };
            }
            var Case = await _context.Cases.FindAsync(Id);

            if (Case is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Errors = { "Incident Entry not found" },
                    Message = "Incident Entry not found",
                };
            }

            //if (Case.IsApproved)
            //{
            //    return new Result<string>
            //    {
            //        StatusCode = StatusCodes.Status400BadRequest,
            //        Errors = { $"Entrywith Incident code {Case.IncidentCode} cannot be edited" },
            //        Message = "Entry cannot be edited it has been approved."
            //    };
            //}

            //validates cases
            //if ((DateTime.Now.Date - Case.CreatedAt.Date).Days >= 90)
            //{
            //    return new Result<string>
            //    {
            //        StatusCode = StatusCodes.Status406NotAcceptable,
            //        Message = $"Entry cannot be edited after 3 months of submission",
            //        Errors = { $"Entry cannot be edited after 3 months of submission" }
            //    };
            //}

            var User = await _context.Users.AsNoTracking()
                       .Include(c => c.State)
                       .AsNoTracking().FirstOrDefaultAsync(c => c.Id == UserId);

            if (User is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "user not found",
                };
            }

            var organisation = await _context.Organisations.AsNoTracking()
                .Where(c => c.Id == model.OrganisationId)
                .Select(o => new { o.Name }).FirstOrDefaultAsync();

            if (organisation is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = { "Organisation not found" },
                    Message = "Creation Failed"
                };
            }

            //validate category or incident type
            //if (model.CaseCategoryId != 0)
            //{
            //    var category = await _context.CaseCategories.AsNoTracking()
            //                              .FirstOrDefaultAsync(c => c.Id == model.CaseCategoryId);

            //    if (category is null)
            //    {
            //        return new AppResult<string>
            //        {
            //            StatusCode = StatusCodes.Status400BadRequest,
            //            Errors = { "Incidence Type not found" },
            //            Message = "Creation Failed"
            //        };
            //    }
            //}
            //else if (model.CaseCategoryId == 0 && !string.IsNullOrWhiteSpace(model.OtherCategory))
            //{
            //    //create new Category

            // var newCateogory = new CaseCategory { Name = model.OtherCategory, };

            // _context.Add(newCateogory);

            // await _context.SaveChangesAsync();

            //    model.CaseCategoryId = newCateogory.Id;
            //}
            //else
            //{
            //    return new AppResult<string>
            //    {
            //        StatusCode = StatusCodes.Status400BadRequest,
            //        Errors = { "Incident Type not provided" },
            //        Message = "Creation Failed"
            //    };
            //}

            //var entries = await _context.Entries.OrderBy(c => c.Field).ToListAsync();

            ////Adds a new Entry if it doesn't exist in the database

            //ValidateEntry(entries, model.WhoReportedIncident, Field.WhoReported);

            //ValidateEntry(entries, model.SexOfSurvior, Field.Sex);

            ////ValidateEntry(entries, model.SexOfPerpetrator, Field.Sex);

            //ValidateEntry(entries, model.TypeOfServiceReceivedBySurvior, Field.TypeOfService);
            //ValidateEntry(entries, model.TypeOfServiceProvidedToSurvior, Field.TypeOfService);
            //ValidateEntry(entries, model.ActualLocationOfIncident, Field.IncidentLocation);

            ////ValidateEntry(entries, model.SurviorRelationWithPerpetrator, Field.Relationship);

            //ValidateEntry(entries, model.OutcomeOfSerivce, Field.ServiceOutcome);

            //ValidateEntry(entries, model.Education, Field.Education);

            //ValidateEntry(entries, model.VulnerablePopulation, Field.VulnerablePopulation);

            //ValidateEntry(entries, model.EmploymentStatusOfParentOrGuardian, Field.EmploymentStatus);
            //ValidateEntry(entries, model.EmploymentStatus, Field.EmploymentStatus);

            //ValidateEntry(entries, model.MaritalStatus, Field.MaritalStatus);

            //checks if the ward is unknown and asignment the values
            if (model.IncidentWardId.HasValue && model.IncidentWardId.Value == 0)
            {
                model.IncidentWardId = !_configuration.GetValue<bool>(StartupKeys.IsLive) ? MetricsKeys.DevUnknown : MetricsKeys.LiveUnknown;
            }

            //update case fields
            Case.GBV_COVID19_Question1 = model.GBV_COVID19_Question1;
            Case.GBV_COVID19_Question2 = model.GBV_COVID19_Question2;
            Case.GBV_COVID19_Question3 = model.GBV_COVID19_Question3;
            Case.GBV_COVID19_Question4 = model.GBV_COVID19_Question4;
            Case.CanBeEdited = true;
            //Case.CaseCategoryId = model.CaseCategoryId;
            Case.CaseCategories = JsonConvert.SerializeObject(model.CaseCategories.ConvertAll(c => c.ToString()));
            Case.CaseCategoriesOthers = model.CaseCategoriesOther;
            Case.IncidentLGAId = model.IncidentLGAId;
            Case.IncidentStateId = model.IncidentStateId;
            Case.AgeOfSurvior = model.AgeOfSurvior;
            Case.TimeOfDay = model.TimeOfDay;
            Case.WhoReportedIncident = model.WhoReportedIncident;
            Case.SexOfSurvior = model.SexOfSurvior;
            Case.TypeOfServiceReceivedBySurvior = model.TypeOfServiceReceivedBySurvior is null ? "" : JsonConvert.SerializeObject(model.TypeOfServiceReceivedBySurvior);
            Case.TypeOfServiceProvidedToSurvior = model.TypeOfServiceProvidedToSurvior is null ? "" : JsonConvert.SerializeObject(model.TypeOfServiceProvidedToSurvior);

            Case.ActualReferralServiceReceived = model.ActualReferralServiceReceived is null ? "" : JsonConvert.SerializeObject(model.ActualReferralServiceReceived);
            Case.HasSurviorReceivedService = model.HasSurviorReceivedService.GetValueOrDefault();
            Case.DateOfIncident = model.DateOfIncident;
            Case.DateReported = model.DateReported;
            Case.OutcomeOfServiceorReferral = model.OutcomeOfSerivce;
            Case.ReceivingOrganisationCode = model.ReceivingOrganisationCode;
            Case.WasViolenceFatal = model.WasViolenceFatal.GetValueOrDefault();
            Case.WhoDoesSurviorLiveWith = model.WhoDoesSurviorLiveWith;
            Case.DoestheSurviorWantJustice = model.DoestheSurviorWantJustice.GetValueOrDefault();
            Case.ActualLocationOfIncident = model.ActualLocationOfIncident;
            Case.PerpetratorsInformation = model.PerpetratorsInformation == null ? "[]" : JsonConvert.SerializeObject(model.PerpetratorsInformation);
            Case.DoesSurviorLiveAlone = model.DoesSurviorLiveAlone.GetValueOrDefault();

            Case.ContactChannel = model.ContactChannel;
            Case.Education = model.Education;
            Case.VulnerablePopulation = model.VulnerablePopulation is null ? "" : JsonConvert.SerializeObject(model.VulnerablePopulation);
            Case.EmploymentStatus = model.EmploymentStatus;
            Case.EmploymentStatusOfParentOrGuardian = model.EmploymentStatusOfParentOrGuardian;
            Case.HasCaseBeenClosed = model.HasCaseBeenClosed.GetValueOrDefault();
            Case.IsSurviorContinuousThreat = model.IsSurviorContinuousThreat.GetValueOrDefault();
            Case.MaritalStatus = model.MaritalStatus;
            Case.NumberOfPerpetrators = model.NumberOfPerpetrators;
            Case.ModifiedAt = CurrentDate;
            Case.IncidentWardId = model.IncidentWardId;
            Case.OrganisationId = model.OrganisationId;
            Case.WhoClosedTheCase = model.WhoClosedTheCase;
            Case.Latitude = model.Latitude;
            Case.Longitude = model.Longitude;
            Case.NameOfServiceProviderReferredTo = model.NameOfServiceProviderReferredTo;
            Case.ContactChannelOrganisation = model.ContactChannelOrganisation;
            Case.ContactChannelOrganisationService = model.ContactChannelOrganisationService is null ? "" : JsonConvert.SerializeObject(model.ContactChannelOrganisationService);
            Case.OtherServiceProviderName = model.OtherServiceProviderName;
            Case.OtherServiceProviderAddress = model.OtherServiceProviderAddress;
            Case.OtherServiceProviderIncidentCode = model.OtherServiceProviderIncidentCode;
            Case.ContactChannelOrganisationIncidentCode = model.ContactChannelOrganisationIncidentCode;
            Case.SurviorDoesNotWantJusticeReasons = model.SurviorDoesNotWantJusticeReasons is null ? "" : JsonConvert.SerializeObject(model.SurviorDoesNotWantJusticeReasons);
            Case.CaseClosedDate = model.CaseClosedDate;
            Case.ReferralOutcome = model.ReferralOutcome;
            Case.SurvivorEstimatedAverageMonthlyIncome = model.SurvivorEstimatedAverageMonthlyIncome;
            Case.OutcomeOfProsecution = model.OutcomeOfProsecution;
            Case.OrganisationLgaId = model.OrganisationLgaId;
            Case.SurvivorMobileNo = model.SurvivorMobileNo;
            Case.DateJusticeReceived = model.DateJusticeReceived;

            {
                Case.IsApproved = false;
                Case.ApprovedById = null;
                Case.ApprovedAt = null;

                Case.ValidatedAt = null;
                Case.IsValidated = false;
                Case.ValidatedById = null;
            }

            _context.Update(Case);

            var status = await _context.SaveChangesAsync() > 0;

            if (status)
            {
                _queue.QueueInvocable<ClearDashboardCacheInvocable>();
            }

            // get all super Admins
            var users = await _context.Users.AsNoTracking().Where(c => c.Type == RoleKeys.Administrator ||
                            ((c.Type == RoleKeys.StateAdministrator || c.Type == RoleKeys.StateSupervisor) && c.StateId == User.StateId))
                            .Select(c => new ApplicationUser
                            {
                                FirstName = c.FirstName,
                                LastName = c.LastName,
                            }).ToListAsync();

            //send mail
            await _notification.SendCaseUpdateNotification(users, $"{User.FirstName} {User.LastName}", User.State.Name, Case.IncidentCode, organisation.Name);

            return new AppResult<string>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = Case.IncidentCode,
                Message = "Update Successful"
            };
        }

        /// <summary>
        /// Approve or disapprove case
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isApproved">If false case is being rejected</param>
        /// <param name="note"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> ApproveOrDisapprove(int id, bool isApproved, string note = null)
        {
            //get the approvers Id  from token.
            // ensure that individual is state supervisor.

            if (string.IsNullOrWhiteSpace(UserId))
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "user not authorized"
                };
            }

            var existingCase = await _context.Cases.Include(c => c.CreatedBy).FirstOrDefaultAsync(c => c.Id == id);

            if (existingCase is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Errors = { "Incident Entry not found" },
                    Message = "Incident Entry not found",
                };
            }

            existingCase.IsApproved = isApproved;
            existingCase.ApprovedById = isApproved ? UserId : string.IsNullOrWhiteSpace(existingCase.ApprovedById) ? UserId : existingCase.ApprovedById; //* change to allow invalidation of cases by super admin //Super admin should be able to undo validation for State Supervisor, CSO Supervisor and/or Service Provider Supervisor
            existingCase.ApprovedAt = isApproved ? CurrentDate : existingCase.ApprovedAt; //* change to allow invalidation of cases by super admin //Super admin should be able to undo validation for State Supervisor, CSO Supervisor and/or Service Provider Supervisor

            //* change to allow invalidation of cases by super admin //Super admin should be able to undo validation for State Supervisor, CSO Supervisor and/or Service Provider Supervisor
            //if (!Isapproved) //if super admin is undoing approval validation should also be undone.
            //{
            //    Case.IsValidated = false;
            //    Case.ValidatedBy = null;
            //    Case.ValidatedAt = default;

            //    //Case.LgaValidated = false;
            //    //Case.LgaValidatedAt = null;
            //    //Case.LgaValidatedBy = null;
            //}

            if (!string.IsNullOrWhiteSpace(note))
            {
                var userName = await _context.Users
                    .AsNoTracking().Where(c => c.Id == UserId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefaultAsync();

                if (userName is null) return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "User not found"
                };
                existingCase.NotesList.Add(new CaseNotes(UserId, userName, note, DateTime.UtcNow));
                existingCase.Notes = JsonConvert.SerializeObject(existingCase.NotesList);
            }

            if (!isApproved)
            {
                await _notification.SendCaseRejectedEmail(existingCase.CreatedBy.Email, existingCase.CreatedBy.FullName, existingCase.IncidentCode, note);
            }

            _context.Update(existingCase);

            await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = existingCase.IncidentCode,
                Message = "Update Successful"
            };
        }

        /// <summary>
        /// approve or disapprove all
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> ApproveOrDisapproveAll(ApproveCase model)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "user authorized"
                };
            }

            var user = await _context.Users.AsNoTracking()
                .Where(c => c.Id == UserId)
                .Select(u => new { u.StateId })
                .FirstOrDefaultAsync();

            if (user is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "user not found",
                };
            }

            var cases = await _context.Cases.Where(c => c.IncidentStateId == user.StateId)
                         .ToListAsync();

            foreach (var item in model.Ids)
            {
                var caseItem = cases.FirstOrDefault(c => c.Id == item);

                if (caseItem != null)
                {
                    caseItem.IsApproved = model.IsApproved;
                    caseItem.ApprovedById = model.IsApproved ? UserId : string.IsNullOrWhiteSpace(caseItem.ApprovedById) ? UserId : caseItem.ApprovedById;//* change to allow invalidation of cases by super admin //Super admin should be able to undo validation for State Supervisor, CSO Supervisor and/or Service Provider Supervisor
                    caseItem.ApprovedAt = model.IsApproved ? CurrentDate : default; //* change to allow invalidation of cases by super admin //Super admin should be able to undo validation for State Supervisor, CSO Supervisor and/or Service Provider Supervisor

                    //* change to allow invalidation of cases by super admin //Super admin should be able to undo validation for State Supervisor, CSO Supervisor and/or Service Provider Supervisor
                    if (!model.IsApproved) //if super admin is undoing approval validation should also be undone.
                    {
                        caseItem.IsValidated = model.IsApproved;
                        caseItem.ValidatedBy = null;
                        caseItem.ValidatedAt = default;

                        caseItem.LgaValidated = false;
                        caseItem.LgaValidatedAt = null;
                        caseItem.LgaValidatedBy = null;
                    }
                    _context.Update(caseItem);
                }
            }

            var num = await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                Data = num.ToString(),
                Message = $"{num} item(s) updated",
                StatusCode = StatusCodes.Status200OK
            };
        }

        /// <summary>
        /// Validate a case
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="isValidated">If false case is been rejected</param>
        /// <param name="isLga"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> Validate(int Id, bool isValidated, bool? isLga, string note = null)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "user authorized"
                };
            }

            var existingCase = await _context.Cases.Include(a => a.CreatedBy).FirstOrDefaultAsync(c => c.Id == Id);

            if (existingCase is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Errors = { "Incident Entry not found" },
                    Message = "Incident Entry not found",
                };
            }

            if (isLga.HasValue && isLga.Value)
            {
                existingCase.LgaValidated = isValidated;
                existingCase.LgaValidatedById = isValidated ? UserId : null;
                existingCase.LgaValidatedAt = isValidated ? CurrentDate : default;
            }
            else
            {
                existingCase.IsValidated = isValidated;
                if (!string.IsNullOrWhiteSpace(note))
                {
                    var userName = await _context.Users
                        .AsNoTracking().Where(c => c.Id == UserId).Select(u => u.FullName).FirstOrDefaultAsync();

                    if (userName is null) return new AppResult<string>
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = "User not found"
                    };

                    var currentNotes = existingCase.NotesList;

                    currentNotes.Add(new CaseNotes(UserId, userName, note, DateTime.UtcNow));
                    existingCase.Notes = JsonConvert.SerializeObject(currentNotes);
                }

                existingCase.ValidatedById = isValidated ? UserId : string.IsNullOrWhiteSpace(existingCase.ValidatedById) ? UserId : existingCase.ValidatedById; //* change to allow invalidation of cases by super admin //Super admin should be able to undo validation for State Supervisor, CSO Supervisor and/or Service Provider Supervisor
                existingCase.ValidatedAt = isValidated ? CurrentDate : existingCase.ValidatedAt; //* change to allow invalidation of cases by super admin //Super admin should be able to undo validation for State Supervisor, CSO Supervisor and/or Service Provider Supervisor

                if (!isValidated)
                {
                    await _notification.SendCaseRejectedEmail(existingCase.CreatedBy.Email, existingCase.CreatedBy.FullName, existingCase.IncidentCode, note);
                }
            }

            _context.Update(existingCase);

            await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = existingCase.IncidentCode,
                Message = "Update Successful"
            };
        }

        /// <summary>
        /// validate all cases
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> ValidateAll(ApproveCase model)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "user authorized"
                };
            }

            var user = await _context.Users.AsNoTracking()
                .Where(c => c.Id == UserId)
                .Select(u => new { u.StateId })
                .FirstOrDefaultAsync();

            if (user is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "user not found",
                };
            }

            var cases = await _context.Cases.Where(c => c.IncidentStateId == user.StateId)
                         .ToListAsync();

            foreach (var item in model.Ids)
            {
                var caseItem = cases.FirstOrDefault(c => c.Id == item);

                if (caseItem != null)
                {
                    caseItem.LgaValidated = model.IsApproved;
                    if (model.IsLga.HasValue && model.IsLga.Value)
                    {
                        caseItem.LgaValidatedById = model.IsApproved ? UserId : null;
                        caseItem.LgaValidatedAt = model.IsApproved ? CurrentDate : default;
                    }
                    else
                    {
                        caseItem.ValidatedById = model.IsApproved ? UserId : string.IsNullOrWhiteSpace(caseItem.ValidatedById) ? UserId : caseItem.ValidatedById; //* change to allow invalidation of cases by super admin //Super admin should be able to undo validation for State Supervisor, CSO Supervisor and/or Service Provider Supervisor
                        caseItem.ValidatedAt = model.IsApproved ? CurrentDate : caseItem.ValidatedAt; //* change to allow invalidation of cases by super admin //Super admin should be able to undo validation for State Supervisor, CSO Supervisor and/or Service Provider Supervisor
                    }
                    _context.Update(caseItem);
                }
            }

            var num = await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                Data = num.ToString(),
                Message = $"{num} item(s) updated",
                StatusCode = StatusCodes.Status200OK
            };
        }

        /// <summary>
        /// delete a case
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> Delete(int Id)
        {
            var Case = await _context.Cases.FindAsync(Id);

            if (Case is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Errors = { "Incident Entry not found" },
                    Message = "Incident Entry not found",
                };
            }

            var code = Case.IncidentCode;
            //if (Case.IsApproved)
            //{
            //    return new Result<string>
            //    {
            //        StatusCode = StatusCodes.Status400BadRequest,
            //        Errors = { "Incident Entry has been approved" },
            //        Message = "Incident entry cannot be deleted"
            //    };
            //}

            _context.Remove(Case);

            await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                StatusCode = StatusCodes.Status204NoContent,
                Message = "Deleted Successfully",
                Data = code
            };
        }

        /// <summary>
        /// Adds a case follow up action
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> AddFollowUpAction(int Id, FollowUpCreationModel model)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "user authorized"
                };
            }

            var Case = await _context.Cases.FindAsync(Id);

            if (Case is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Errors = { "Incident Entry not found" },
                    Message = "Incident Entry not found",
                };
            }

            if (model.HasCaseBeenClosed != YesOrNo.NotApplicable)
            {
                Case.HasCaseBeenClosed = model.HasCaseBeenClosed;
                _context.Update(Case);
            }

            var followup = new FollowUp
            {
                JusticeReceivedDate = model.JusticeReceivedDate,
                CaseId = Case.Id,
                FinalOutcome = model.FinalOutcome,
                HasClientReceivedjustice = model.HasClientReceivedjustice,
                HasCaseBeenClosed = model.HasCaseBeenClosed,
                CreatedById = UserId,
                WhoClosedTheCase = model.WhoClosedTheCase,
                CaseClosedDate = model.CaseClosedDate,
                Longitude = model.Longitude,
                Latitude = model.Latitude
            };

            _context.Add(followup);

            await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                Data = Case.IncidentCode,
                Message = "Follow up action added Successful",
                StatusCode = StatusCodes.Status201Created
            };
        }

        /// <summary>
        /// delete a case follow up action
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> DeleteFollowUpAction(int CaseId, int Id)
        {
            var Case = await _context.Cases.FindAsync(CaseId);

            if (Case is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Errors = { "Incident Entry not found" },
                    Message = "Incident Entry not found",
                };
            }

            var Follow = await _context.FollowUps.FindAsync(Id);

            if (Follow is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Errors = { "Incident Follow up action not found" },
                    Message = "Incident Follow up action not found",
                };
            }

            _context.Remove(Follow);

            await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                Data = Case.IncidentCode,
                StatusCode = StatusCodes.Status204NoContent,
                Message = "Follow up action deleted successfully"
            };
        }

        /// <summary>
        /// update follow ups
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="Id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> UpdateFollowUpAction(int CaseId, int Id, FollowUpCreationModel model)
        {
            var Case = await _context.Cases.FindAsync(CaseId);

            if (Case is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Errors = { "Incident Entry not found" },
                    Message = "Incident Entry not found",
                };
            }

            var follow = await _context.FollowUps.FindAsync(Id);

            if (follow is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Errors = { "Incident Follow up action not found" },
                    Message = "Incident Follow up action not found",
                };
            }

            follow.JusticeReceivedDate = model.JusticeReceivedDate;
            follow.CaseId = Case.Id;
            follow.FinalOutcome = model.FinalOutcome;
            follow.HasClientReceivedjustice = model.HasClientReceivedjustice;
            follow.HasCaseBeenClosed = model.HasCaseBeenClosed;
            follow.CreatedById = UserId;
            follow.ModifiedAt = CurrentDate;

            _context.Update(follow);

            await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                Data = Case.IncidentCode,
                StatusCode = StatusCodes.Status200OK,
                Message = "Follow up action updated successfully"
            };
        }

        /// <summary>
        /// Get all follow up actions for a case
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public async Task<AppResult<List<FollowUpViewModel>>> GetFollowUpActions(int CaseId)
        {
            var Case = await _context.Cases.FindAsync(CaseId);

            if (Case is null)
            {
                return new AppResult<List<FollowUpViewModel>>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Errors = { "Incident Entry not found" },
                    Message = "Incident Entry not found",
                };
            }

            var followups = await _context.FollowUps.Where(c => c.CaseId == CaseId)
                              .Select(c => new FollowUpViewModel
                              {
                                  Id = c.Id,
                                  CaseId = c.CaseId,
                                  CreatedAt = c.CreatedAt,
                                  CreatedById = c.CreatedById,
                                  FinalOutcome = c.FinalOutcome,
                                  HasCaseBeenClosed = c.HasCaseBeenClosed,
                                  HasClientReceivedjustice = c.HasClientReceivedjustice,
                                  JusticeReceivedDate = c.JusticeReceivedDate,
                                  IncidentCode = Case.IncidentCode,
                                  CreatedBy = $"{c.CreatedBy.FirstName} {c.CreatedBy.LastName}",
                              }).ToListAsync();

            return new AppResult<List<FollowUpViewModel>>
            {
                Data = followups,
                Message = "Successful",
                StatusCode = StatusCodes.Status200OK
            };
        }


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

        private async Task<ApplicationUser> GetUserAsync(string userId) => await _context.Users.FirstOrDefaultAsync(x => x.Id == UserId);

        private IQueryable<LocalGovernmentArea> GetLocalGovernmentAreas() => _context.LocalGovernmentAreas.AsQueryable().AsNoTracking();

        private IQueryable<State> GetStates() => _context.States.AsQueryable().AsNoTracking();
        private IQueryable<Ward> GetWards() => _context.Wards.AsQueryable().AsNoTracking();

        private async Task<LocalGovernmentArea> GetALocalGovernmentArea(IQueryable<LocalGovernmentArea> localGovernmentAreas, string name) => await localGovernmentAreas.FirstOrDefaultAsync(l => l.Name.ToLower().Trim().Contains(name));

        private StateList GetAState(List<StateList> states, int id) => states.FirstOrDefault(l => l.Id == id);




    }

    public partial class CaseService
    {
        private Entry ValidateEntry(List<Entry> Entries, string Value, Field Field)
        {
            var fieldvalues = Entries.Where(c => c.Field == Field)
                          .OrderBy(c => int.Parse(c.Key)).ToList();

            var value = fieldvalues.Where(c => c.Value.ToLower() == Value.Trim().ToLower());

            if (value is null)
            {
                //create entry
                var Last = fieldvalues.LastOrDefault();

                var key = int.Parse(Last.Key) + 1;

                var newEntry = new Entry
                {
                    Key = key.ToString(),
                    Value = Value,
                    Field = Field,
                    Type = EntryType.Other,
                };

                _context.Add(newEntry);
            }
            return null;
        }

        private Entry ValidateEntry(List<Entry> Entries, List<string> Values, Field Field)
        {
            var fieldvalues = Entries.Where(c => c.Field == Field)
                         .OrderBy(c => int.Parse(c.Key)).ToList();

            var Last = fieldvalues.LastOrDefault().Key;

            var Key = int.Parse(Last);
            foreach (var v in Values)
            {
                var value = fieldvalues.Where(c => c.Value.ToLower() == v.Trim().ToLower());

                if (value is null)
                {
                    Key += 1;

                    var newEntry = new Entry
                    {
                        Key = Key.ToString(),
                        Value = v,
                        Field = Field,
                        Type = EntryType.Other,
                    };

                    _context.Add(newEntry);
                }
            }

            return null;
        }

        /// <summary>
        /// generate IncidentCode based on state and organisation code
        /// </summary>
        /// <param name="stateCode"></param>
        /// <param name="OrganisationCode"></param>
        /// <returns></returns>
        private async Task<(string, int)> GenerateIncidentCode(string stateCode, string OrganisationCode)
        {
            var lastCase = await _context.Cases.OrderBy(c => c.SerialNumber).LastOrDefaultAsync();

            var serialno = 0;
            if (lastCase == null)
            {
                serialno = 1;
            }
            else
            {
                serialno = lastCase.SerialNumber + 1;
            }

            return CaseAndSerialNumber(stateCode, OrganisationCode, serialno);
        }

        private (string incidentCode, int serial) CaseAndSerialNumber(string stateCode, string organisationCode, int serialno) =>
            ($"{stateCode.ToUpper()}/{organisationCode}/{serialno:0000}", serialno);

        private IQueryable<Case> SortCasesBy(IQueryable<Case> cases, SortByDate? byDate, DateTime? startDate, DateTime? endDate)
        {
            if (byDate == SortByDate.DateReported)
            {
                cases = cases.OrderByDescending(c => c.DateReported);

                if (startDate.HasValue)
                {
                    cases = cases.Where(c => c.DateReported >= startDate);
                }
                if (endDate.HasValue)
                {
                    cases = cases.Where(c => c.DateReported <= endDate);
                }
            }
            else if (byDate == SortByDate.DateSubmitted)
            {
                cases = cases.OrderByDescending(c => c.CreatedAt);

                if (startDate.HasValue)
                {
                    cases = cases.Where(c => c.CreatedAt >= startDate);
                }
                if (endDate.HasValue)
                {
                    cases = cases.Where(c => c.CreatedAt <= endDate);
                }
            }
            else if (byDate == SortByDate.DateOfIncident)
            {
                cases = cases.OrderByDescending(c => c.DateOfIncident);

                if (startDate.HasValue)
                {
                    cases = cases.Where(c => c.DateOfIncident >= startDate);
                }
                if (endDate.HasValue)
                {
                    cases = cases.Where(c => c.DateOfIncident <= endDate);
                }
            }
            else if (byDate == SortByDate.DateModified)
            {
                cases = cases.OrderByDescending(c => c.ModifiedAt);

                if (startDate.HasValue)
                {
                    cases = cases.Where(c => c.ModifiedAt >= startDate);
                }
                if (endDate.HasValue)
                {
                    cases = cases.Where(c => c.ModifiedAt <= endDate);
                }
            }

            return cases;
        }
    }
}