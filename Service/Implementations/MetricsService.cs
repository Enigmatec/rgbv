using Core.Data;
using Core.Entities;
using Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Service.Enums;
using Service.Interfaces;
using Service.Models;
using Service.StringKeys;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Coravel.Queuing.Interfaces;
using Service.AppServices;
using Z.EntityFramework.Plus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Globalization;
using Service.Models.ViewModels;
using Hangfire;
using CSharpFunctionalExtensions;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Service.Implementations
{
    /// <summary>
    /// Queries data and formats for presentation Excel and SPSS exports
    /// </summary>
    public partial class MetricsService : IMetrics
    {
        private readonly ApplicationDbContext _context;

        private readonly HttpContext _httpContext;

        private readonly IAppCachingService _appCachingService;
        private readonly IMapper _mapper;
        private readonly IQueue _queue;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        //check if the user is authenicated
        private string UserId => _httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public MetricsService(ApplicationDbContext context, IHttpContextAccessor accessor, IAppCachingService appCachingService, IMapper mapper, IQueue queue, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _httpContext = accessor.HttpContext;
            _appCachingService = appCachingService;
            _mapper = mapper;
            _queue = queue;
            _configuration = configuration;
            _emailService = emailService;
        }

        private static IQueryable<Case> DashboardFilters(IQueryable<Case> allCases, SearchModel model, UserViewModel user, DashboardKeys dashboard)
        {
            switch (model.IsCaseValidated.ToLower())
            {
                case "yes":
                    allCases = allCases
                        .Where(c => c.IsValidated)
                       .OrderByDescending(c => c.DateReported).AsNoTracking();
                    break;
                case "no":
                    allCases = allCases
                        .Where(c => !c.IsValidated)
                       .OrderByDescending(c => c.DateReported).AsNoTracking();
                    break;
                default:
                    break;
            }

            if (dashboard == DashboardKeys.AdminDashBoard)
            {
                //allCases = allCases.Where(c => c.IsApproved)
                //    .OrderByDescending(c => c.DateReported);

                if (model.OrganisationType.HasValue)
                {
                    allCases = allCases.Where(c => c.Organisation.OrganisationType == model.OrganisationType);
                }
                if (model.CaseCategories.Any())
                {
                    //allCases = allCases.Where(c => model.CaseCategories.Contains(c.CaseCategoryId));
                    allCases = allCases.Where(c => model.CaseCategories.Any(b => c.CaseCategories.Contains(((CaseCategoryOrTypeOfViolence)b).ToString())));

                    var caseCategoryList = Enum.GetValues(typeof(CaseCategoryOrTypeOfViolence)).ToListDynamic();
                    //var caseByType = new List<CaseBySubjectBySex>();

                    //foreach (var cat in caseCategoryList)
                    //{
                    //    var catInfo = cases.Where(c => c.CaseCategoriesList.Contains((int)cat));

                    //    //caseByType.Add(new CaseBySubjectBySex
                    //    //{
                    //    //    Subject = cat.ToString(), //catInfo.FirstOrDefault().Category.Name,
                    //    //    FemaleCount = catInfo.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female),
                    //    //    MaleCount = catInfo.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                    //    //    OtherCount = catInfo.Count(c => c.SexOfSurvior.ToLower() != MetricsKeys.Male && c.SexOfSurvior.ToLower() != MetricsKeys.Female),
                    //    //    TotalNewCases = catInfo.Count(c => c.HasSurviorReceivedService == YesOrNo.No),
                    //    //    TotalFollowUpCases = catInfo.Count(c => c.HasSurviorReceivedService == YesOrNo.Yes)
                    //    //});
                    //}
                    //allCases = allCases.Where(x => x.CaseCategories.);
                    //foreach (var item in model.CaseCategories)
                    //{
                    //    if(item==(int)CaseCategoryOrTypeOfViolence)
                    //}
                }

                if (model.Year.Any())
                {
                    allCases = allCases.Where(c => model.Year.Contains(c.DateReported.Year));

                }

                if (model.StartDate.HasValue)
                {
                    allCases = allCases.Where(c => c.DateReported >= model.StartDate.Value.Date);
                }

                if (model.EndDate.HasValue)
                {
                    allCases = allCases.Where(c => c.DateReported <= model.EndDate.Value);
                }

                if (model.States.Any())
                {
                    allCases = allCases.Where(c => model.States.Contains(c.IncidentStateId));
                }

                if (model.Lgas.Any())
                {
                    allCases = allCases.Where(c => model.Lgas.Contains(c.IncidentLGAId));
                }

                //if (!string.IsNullOrWhiteSpace(model.Gender))
                //{
                //    allCases = allCases.Where(c => c.SexOfSurvior.Trim().ToLower() == model.Gender.Trim().ToLower());
                //}

                //if (model.MinimumAge.HasValue)
                //{
                //    allCases = allCases.Where(c => c.AgeOfSurvior >= model.MinimumAge);
                //}
                //if (model.MaximumAge.HasValue)
                //{
                //    allCases = allCases.Where(c => c.AgeOfSurvior <= model.MaximumAge);
                //}


                if (model.HasSurvivorReceivedService.HasValue && model.HasSurvivorReceivedService.Value)
                    allCases = allCases.Where(c => c.OutcomeOfProsecution.ToLower() == "conviction");
                //if (model.HasSurvivorReceivedService.HasValue)
                //{
                //    var received = model.HasSurvivorReceivedService == true ? YesOrNo.Yes : YesOrNo.No;
                //    if (received == YesOrNo.Yes)
                //        allCases = allCases.Where(c => c.OutcomeOfProsecution.ToLower() == "conviction");
                //    //allCases = allCases.Where(c => c.HasSurviorReceivedService == received);
                //}
            }
            else if (dashboard == DashboardKeys.SuperVisorDashBoard)
            {
                //allCases = allCases.Where(c => c.IsApproved).OrderByDescending(c => c.DateReported);

                //apply model filters
                if (model.OrganisationType.HasValue)
                {
                    allCases = allCases.Where(c => c.Organisation.OrganisationType == model.OrganisationType);
                }

                if (model.CaseCategories.Count > 0)
                {
                    //allCases = allCases.Where(c => model.CaseCategories.Contains(c.CaseCategoryId));
                    allCases = allCases.Where(c => model.CaseCategories.Any(b => c.CaseCategories.Contains(((CaseCategoryOrTypeOfViolence)b).ToString())));
                }

                //if (model.Year.Count > 0)
                //{
                //    CaseQuery = CaseQuery.Where(c => model.Year.Contains(c.DateReported.Year));

                //}

                if (model.StartDate.HasValue)
                {
                    allCases = allCases.Where(c => c.DateReported >= model.StartDate.Value.Date);
                }

                if (model.EndDate.HasValue)
                {
                    allCases = allCases.Where(c => c.DateReported <= model.EndDate.Value);
                }

                if (model.States.Count > 0 && user.Role == RoleKeys.StateSupervisor)
                {
                    allCases = allCases.Where(c => model.States.Contains(c.IncidentStateId));
                }

                if (model.Lgas.Count > 0)
                {
                    allCases = allCases.Where(c => model.Lgas.Contains(c.IncidentLGAId));
                }

                if (!string.IsNullOrWhiteSpace(model.Gender))
                {
                    allCases = allCases.Where(c => c.SexOfSurvior.Trim().ToLower() == model.Gender.Trim().ToLower());
                }

                if (model.MinimumAge.HasValue)
                {
                    allCases = allCases.Where(c => c.AgeOfSurvior >= model.MinimumAge);
                }

                if (model.MaximumAge.HasValue)
                {
                    allCases = allCases.Where(c => c.AgeOfSurvior <= model.MaximumAge);
                }

                if (model.HasSurvivorReceivedService.HasValue && model.HasSurvivorReceivedService.Value)
                    allCases = allCases.Where(c => c.OutcomeOfProsecution.ToLower() == "conviction");
            }
            else if (dashboard == DashboardKeys.SPandCSODashboard)
            {
                allCases = allCases
                    .Where(c => c.OrganisationId == user.OrganisationId);
                //.Where(c => c.IsApproved).OrderByDescending(c => c.DateReported);

                //filter cases entered in  User State
                if (!string.IsNullOrWhiteSpace(user.StateCode))
                {
                    allCases = allCases.Where(c => c.IncidentCode.ToLower().Contains(user.StateCode.Trim().ToLower()));
                }

                if (model.CaseCategories.Count > 0)
                {
                    //allCases = allCases.Where(c => model.CaseCategories.Contains(c.CaseCategoryId));
                    allCases = allCases.Where(c => model.CaseCategories.Any(b => c.CaseCategories.Contains(((CaseCategoryOrTypeOfViolence)b).ToString())));
                }

                //if (model.Year.Count > 0)
                //{
                //    AllCases = AllCases.Where(c => model.Year.Contains(c.DateReported.Year));

                //}

                if (model.Lgas.Count > 0)
                {
                    allCases = allCases.Where(c => model.Lgas.Contains(c.IncidentLGAId));
                }

                if (model.StartDate.HasValue)
                {
                    allCases = allCases.Where(c => c.DateReported >= model.StartDate.Value.Date);
                }

                if (model.EndDate.HasValue)
                {
                    allCases = allCases.Where(c => c.DateReported <= model.EndDate.Value);
                }

                if (!string.IsNullOrWhiteSpace(model.Gender))
                {
                    allCases = allCases.Where(c => c.SexOfSurvior.Trim().ToLower() == model.Gender.Trim().ToLower());
                }

                if (model.MinimumAge.HasValue)
                {
                    allCases = allCases.Where(c => c.AgeOfSurvior >= model.MinimumAge);
                }

                if (model.MaximumAge.HasValue)
                {
                    allCases = allCases.Where(c => c.AgeOfSurvior <= model.MaximumAge);
                }

                if (model.HasSurvivorReceivedService.HasValue && model.HasSurvivorReceivedService.Value)
                    allCases = allCases.Where(c => c.OutcomeOfProsecution.ToLower() == "conviction");
            }
            else if (dashboard == DashboardKeys.DonorDashBoard)
            {
                if (model.CaseCategories.Count > 0)
                {
                    //allCases = allCases.Where(c => model.CaseCategories.Contains(c.CaseCategoryId));
                    allCases = allCases.Where(c => model.CaseCategories.Any(b => c.CaseCategories.Contains(((CaseCategoryOrTypeOfViolence)b).ToString())));
                }

                if (model.StartDate.HasValue)
                {
                    allCases = allCases.Where(c => c.DateReported >= model.StartDate.Value.Date);
                }

                if (model.EndDate.HasValue)
                {
                    allCases = allCases.Where(c => c.DateReported <= model.EndDate.Value);
                }
            }

            return allCases;
        }

        /// <summary>
        /// Data for Admin Dashboard.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<AdminDashBoardModel>> AdminDashBoard(SearchModel model, DashboardKeys dashboard)
        {
            //check for authorization
            if (UserId is null)
            {
                return new AppResult<AdminDashBoardModel>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized",
                    Errors = { "Cannot view this resource" }
                };
            }
            var user = await _context.Users.AsNoTracking()
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
                              }).FirstOrDefaultAsync(c => c.Id == UserId);

            if (user is null)
            {
                return new AppResult<AdminDashBoardModel>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User not found",
                    Errors = { "User not found" }
                };
            }

            var key = _appCachingService.ComposeMetricsCacheKey(dashboard, model, user.Role, user.OrganisationId.GetValueOrDefault(), user.StateId.GetValueOrDefault());

            var getFromCache = await _appCachingService.GetMetrics<AdminDashBoardModel>(key);

            if (getFromCache.status) return new AppResult<AdminDashBoardModel>
            {
                Data = getFromCache.data,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successful"
            };

            if (dashboard == DashboardKeys.AdminDashBoard)
            {
                model.States = model.States.Where(c => c > 0).ToList();
            }
            else
            {
                model.CaseCategories = model.CaseCategories.Where(c => c > 0).ToList();
                model.Lgas = model.Lgas.Where(c => c > 0).ToList();
            }

            //model.Year = model.Year.Where(c => c != 0).ToList();

            //query cases with filter
            //IQueryable<Case> allCases;
            //if(model.IsCaseValidated)
            //{
            //     allCases = _context.Cases
            //    .Where(c => c.IsApproved)
            //              .OrderByDescending(c => c.DateReported).AsNoTracking();
            //}
            var allCases = _context.Cases
                .OrderByDescending(c => c.DateReported).AsNoTracking();

            var gh = allCases.Count();
            //Apply filters in the query
            //NOTE: due to modifications of this dashboard some of these filters are redundant.

            allCases = DashboardFilters(allCases, model, user, dashboard);

            var fg = allCases.Count();
            if (!string.IsNullOrWhiteSpace(model.OrganisationName))
            {
                allCases = allCases.Where(c => c.Organisation.Name.ToLower().Contains(model.OrganisationName.ToLower()));
            }

            var organizations = _context.Organisations.Select(o => new
            {
                o.States,
                o.OrganisationType
            }).AsNoTracking();

            if (dashboard == DashboardKeys.SuperVisorDashBoard)
            {
                //check user role to check data that can be accessed
                //if user is an CSO or SP filter with orgranisation
                if (user.Role is RoleKeys.CSOSupervior or RoleKeys.ServiceProviderSupervior)
                {
                    allCases = allCases.Where(c => c.OrganisationId == user.OrganisationId);
                }

                //if the user the of state access
                if (user.Role is RoleKeys.StateSupervisor or RoleKeys.StateAdministrator or RoleKeys.LocalGovernment)
                {
                    //pull data for only that state
                    if (!string.IsNullOrWhiteSpace(user.StateCode))
                    {
                        allCases = allCases.Where(c => c.IncidentCode.ToLower().Contains(user.StateCode.ToLower()));
                    }

                    //pull data for only that local government
                    if (user.Role == RoleKeys.LocalGovernment && user.LocalGovernments.Any())
                    {
                        var userLgas = string.Join(",", user.LocalGovernments.Select(l => $",{l.Id},"));

                        allCases = allCases.Where(c => userLgas.Contains("," + c.IncidentLGAId + ","));
                    }

                    //query organisations belonging to a state
                    //the states is save as a json string in the data base
                    organizations = organizations.Where(c => c.States.Contains($":{user.StateId}"));

                    var jn = allCases.Count();
                }
            }

            //some of these queries are now redundant
            var usersCount = _context.Users.AsNoTracking().DeferredCount().FutureValue();

            var organizationsCount = organizations.DeferredCount().FutureValue();
            var noOfCso = organizations.Where(c => c.OrganisationType == OrganisationType.CSO).DeferredCount().FutureValue();
            var noOfSp = organizations.Where(c => c.OrganisationType == OrganisationType.ServiceProvider).DeferredCount().FutureValue();
            var noOfPart = organizations.Where(c => c.OrganisationType == OrganisationType.Partner).DeferredCount().FutureValue();


            var totalCases = allCases.DeferredCount().FutureValue();
            var newCases = allCases.DeferredCount(c => c.DateReported.Year == DateTime.Now.Year && c.DateReported.Month == DateTime.Now.Month).FutureValue();

            var followupCases = allCases.DeferredCount(c => c.HasSurviorReceivedService == YesOrNo.Yes).FutureValue();

            var totalCasesToday = allCases.DeferredCount(c => c.CreatedAt.Date == DateTime.Now.Date).FutureValue();

            var totalCasesMonth = allCases.DeferredCount(c => c.CreatedAt.Year == DateTime.Now.Year && c.CreatedAt.Month == DateTime.Now.Month).FutureValue();

            var totalCasesFatal = allCases.DeferredCount(c => c.WasViolenceFatal == YesOrNo.Yes).FutureValue();

            var totalApprovedCases = allCases.DeferredCount(c => c.IsApproved).FutureValue();

            var totalUnApprovedCases = allCases.DeferredCount(c => !c.IsApproved).FutureValue();

            var totalValidatedCases = allCases.DeferredCount(c => c.IsValidated).FutureValue();
            var totalUnValidatedCases = allCases.DeferredCount(c => !c.IsValidated).FutureValue();

            var totalCasesFemale = allCases.DeferredCount(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female).FutureValue();

            var totalCasesMale = allCases.DeferredCount(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male).FutureValue();

            var closedCasesYes = allCases.DeferredCount(c => c.HasCaseBeenClosed == YesOrNo.Yes).FutureValue();
            var closedCasesNotApplicable = allCases.DeferredCount(c => c.HasCaseBeenClosed == YesOrNo.NotApplicable).FutureValue();
            var closedCases = closedCasesYes + closedCasesNotApplicable;
            var openCases = allCases.DeferredCount(c => c.HasCaseBeenClosed == YesOrNo.No).FutureValue();

            var caseByAgeLessThan18 = allCases.DeferredCount(c => c.AgeOfSurvior < 18).FutureValue();
            var caseByAgeGreatNorthEqual18 = allCases.DeferredCount(c => c.AgeOfSurvior >= 18).FutureValue();

            var totalCasesEnteredByUser = dashboard == DashboardKeys.SPandCSODashboard ? allCases.DeferredCount(c => c.CreatedById == UserId).FutureValue() : 0;

            var caseFemaleGreaterThanorEqual18 = allCases.DeferredCount(c => c.AgeOfSurvior >= 18 && c.SexOfSurvior.ToLower() == MetricsKeys.Female).FutureValue();

            var caseMaleLessThan18 = allCases.DeferredCount(c => c.AgeOfSurvior < 18 && c.SexOfSurvior.ToLower() == MetricsKeys.Male).FutureValue();
            var maleGreatThanOrEqual18 = allCases.DeferredCount(c => c.AgeOfSurvior >= 18 && c.SexOfSurvior.ToLower() == MetricsKeys.Male).FutureValue();
            var doesSurvivorWantJustice = allCases.DeferredCount(c => c.DoestheSurviorWantJustice == YesOrNo.Yes).FutureValue();

            var convictedPerpetuation = allCases
                .DeferredCount(c => c.OutcomeOfProsecution.ToLower() == "conviction").FutureValue();

            //var convictedPerpetuation = allCases
            //  .Where(c => c.HasCaseBeenClosed == YesOrNo.Yes)
            //  .DeferredCount(c => c.OutcomeOfServiceorReferral.ToLower() == MetricsKeys.PerpetratorConvicted).FutureValue();

            var listCases = await allCases.ProjectTo<AdminDashboardProjectModel>(_mapper.ConfigurationProvider).Future().ToListAsync();

            var perpetratorsByRelationshipBySex = GetCaseByRelationShipBySex(listCases);

            var caseByVulnerable = GetCaseByVulnerablePopulation(listCases).Item1;

            var caseByTypeOfServiceProvided = GetCaseByTypeOfService(listCases).Item1;

            var accessToJusticeByTypeOfViolenceBySex = CasesByTypeOfViolenceBySex(listCases);

            var policeSvs = MetricsKeys.TypeOfService_Servicesofthepolice.Trim().Replace("TypeOfService_", "").ToLower();

            var receivedServiceOfPolice = CasesByTypeOfViolenceBySex(listCases.Where(c => c.TypeOfServiceProvidedToSurvior.Trim().ToLower().Contains(policeSvs)));

            var legalSvs = MetricsKeys.TypeOfService_Legalassistance.Trim().Replace("TypeOfService_", "").ToLower();

            var receivedLegalService = CasesByTypeOfViolenceBySex(listCases.Where(c => c.TypeOfServiceProvidedToSurvior.Trim().ToLower().Contains(legalSvs)));

            var accessToJusticeByAgeGroupBySex = GetCaseByAgeGroupBySex(listCases, true);

            var convictedPerpetratorsByTypeOfViolence = await GroupCaseByType(listCases, true);

            var casesByWhoClosedCaseBySex = CasesByWhoClosedCaseBySex(listCases);

            var casesBylga = new List<CaseBySubject>();
            if (dashboard is DashboardKeys.SuperVisorDashBoard or DashboardKeys.SPandCSODashboard)
            {
                var lgaIds = listCases.Select(s => s.IncidentLGAId).Distinct();

                foreach (var lgaId in lgaIds)
                {
                    var lgaNames = allCases.Where(c => c.IncidentLGAId == lgaId).Select(c => c.IncidentLGA.Name);
                    casesBylga.Add(new CaseBySubject
                    {
                        Count = lgaNames.Count(),
                        Name = lgaNames.FirstOrDefault(),
                        Id = lgaId
                    });
                }
            }

            //var casesByState = listCases.Where(x => x.OutcomeOfProsecution?.ToLower() == "conviction").GroupBy(c => c.IncidentStateId)
            //                      .Select(p => new CaseBySubject
            //                      {
            //                          Id = p.Key,
            //                          Name = p.FirstOrDefault()?.IncidentState?.Name,
            //                          Count = p.Count(),
            //                      }).ToList();
            var casesByState = listCases
                                    .GroupBy(c => c.IncidentStateId)
                                    .Select(p => new CaseBySubject
                                    {
                                        Id = p.Key,
                                        Name = p.FirstOrDefault()?.IncidentState?.Name,
                                        Count = p.Count()
                                    })
                                    .ToList();

            var caseByIncidentType = await GroupCaseByType(listCases);

            var casesByReportedYear = listCases.GroupBy(c => c.DateReported.Year)
                            .Select(p => new CaseBySubject
                            {
                                Id = p.Key,
                                Name = p.Key.ToString(),
                                Count = p.Count()
                            }).OrderBy(c => c.Id).ToList();

            var casesByIncidentYear = listCases.GroupBy(c => c.DateOfIncident.Year)
                             .Select(p => new CaseBySubject
                             {
                                 Id = p.Key,
                                 Name = p.Key.ToString(),
                                 Count = p.Count()
                             }).OrderBy(c => c.Id).ToList();

            //case by month
            var casesByIncidentMonthsPresentYear = listCases.Where(c => c.DateReported.Year == DateTime.Now.Year)
                                          .GroupBy(c => c.DateReported.Month)
                                          .Select(p => new CaseBySubject
                                          {
                                              Id = p.Key,
                                              Name = p.FirstOrDefault()?.DateReported.ToString("MMM"),
                                              Count = p.Count()
                                          }).OrderBy(c => c.Id).ToList();

            var casesByTimeOfViolence = listCases.GroupBy(c => c.TimeOfDay)
                                       .Select(p => new CaseBySubject
                                       {
                                           Id = (int)p.Key,
                                           Name = p.FirstOrDefault()?.TimeOfDay.ToString(),
                                           Count = p.Count()
                                       }).OrderByDescending(c => c.Count).ToList();

            var propOfCasessWithConvictedPerp = new PropOfCasessWithConvictedPerp
            {
                VictimsThatWantJustice = doesSurvivorWantJustice,
                PerpetratorsConvicted = convictedPerpetuation
            };

            var result = new AdminDashBoardModel
            {
                TotalValidatedCases = totalValidatedCases,
                TotalUnValidatedCases = totalUnValidatedCases,
                User = user,
                NoOfUsers = usersCount,
                CaseByState = casesByState,
                TotalCases = totalCases,
                TotalApprovedCases = totalApprovedCases,
                TotalCasesByFemale = totalCasesFemale,
                TotalCasesByMale = totalCasesMale,
                TotalCasesThisMonth = totalCasesMonth,
                TotalCasesToday = totalCasesToday,
                TotalFatalCases = totalCasesFatal,
                TotalUnApprovedCases = totalUnApprovedCases,
                NumberOfCSO = noOfCso,
                NumberOfSP = noOfSp,
                NumberOfPart = noOfPart,
                CaseByIncidentType = caseByIncidentType,
                NumberOfOrganisations = organizationsCount,
                ClosedCases = closedCases,
                OpenCases = openCases,
                CaseByReportedYear = casesByReportedYear,
                CaseByIncidentYear = casesByIncidentYear,
                CaseByMonthOfPresentYear = casesByIncidentMonthsPresentYear,
                CaseAgeGreatThanOrEqual18 = caseByAgeGreatNorthEqual18,
                CaseAgeLessThan18 = caseByAgeLessThan18,
                CaseFemaleGreatThanOrEqual18 = caseFemaleGreaterThanorEqual18,
                CaseFemaleLestThan18 = caseByAgeLessThan18,
                CaseMaleGreatThanOrEqual18 = maleGreatThanOrEqual18,
                CaseMaleLestThan18 = caseMaleLessThan18,
                PerpetratorsByRelationshipBySex = perpetratorsByRelationshipBySex,
                CaseByVulnerable = caseByVulnerable,
                ConvictedPerpetuators = convictedPerpetuation,
                CasesByTimeOfViolience = casesByTimeOfViolence,
                CasesByYearByMonths = GetCaseByYearByMonth(listCases),
                NoOfPeopleWhoWantJustice = doesSurvivorWantJustice,
                CasesByAgeGroupBySex = GetCaseByAgeGroupBySex(listCases),
                CaseByTypeOfServiceProvided = caseByTypeOfServiceProvided,
                NewCases = newCases,
                FollowUpCases = followupCases,
                AccessToJusticeByTypeOfViolenceBySex = accessToJusticeByTypeOfViolenceBySex,
                RecievedServiceOfPolice = receivedServiceOfPolice,
                RecievedLegalService = receivedLegalService,
                AccessToJusticeByAgeGroupBySex = accessToJusticeByAgeGroupBySex,
                ConvictedPerpetratorsByTypeOfViolence = convictedPerpetratorsByTypeOfViolence,
                CasesByWhoClosedCaseBySex = casesByWhoClosedCaseBySex,
                CaseByLGA = casesBylga,
                TotalCasesEnteredByUser = totalCasesEnteredByUser,
                PropOfCasessWithConvictedPerp = propOfCasessWithConvictedPerp
            };

            _queue.QueueAsyncTask(async () =>
            {
                await _appCachingService.AddMetrics(key, result);
            });

            return new AppResult<AdminDashBoardModel>
            {
                Data = result,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successful"
            };
        }

        /// <summary>
        /// Display Home page metrics
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<HomeMetricsModel>> HomePageMetrics(SearchModel model)
        {
            //var key = _appCachingService.ComposeMetricsCacheKey(DashboardKeys.HomePageMetrics, model);

            //var getFromCache = await _appCachingService.GetMetrics<HomeMetricsModel>(key);

            //if (getFromCache.status) return new AppResult<HomeMetricsModel>
            //{
            //    Data = getFromCache.data,
            //    StatusCode = StatusCodes.Status200OK,
            //    Message = "Successful"
            //};

            //var organisations = await _context.Organisations.AsNoTracking().Select(o => o.OrganisationType).ToListAsync();

            //var noofCSO = organisations.Count(c => c == OrganisationType.CSO);

            //var noofSP = organisations.Count(c => c == OrganisationType.ServiceProvider);

            //var totalOrganisations = organisations.Count();

            //pull cases
            var allCases = _context.Cases.AsNoTracking();

            //var counted = AllCases.Count();

            //ensure that the list contains integers greater than zero
            //model.CaseCategories = model.CaseCategories.Where(c => c > 0).ToList();
            //model.Lgas = model.Lgas.Where(c => c > 0).ToList();
            //model.States = model.States.Where(c => c > 0).ToList();
            //model.Year = model.Year.Where(c => c > 0).ToList();

            //Apply filters in the query
            //NOTE: due to modifications of this dashboard some of these filters are redundant.
            //Filter begins here
            // Can make it General Method??
            //Caching?
            // Filter parameters to add
            //Filiter by state, LGA and ward
            //Filter by state and LGA
            //Filter by state
            //if (model.OrganisationType.HasValue)
            //{
            //    allCases = allCases.Where(c => c.Organisation.OrganisationType == model.OrganisationType);
            //}
            //if (model.CaseCategories.Count > 0)
            //{
            //    //allCases = allCases.Where(c => model.CaseCategories.Contains(c.CaseCategoryId));
            //    allCases = allCases.Where(c => model.CaseCategories.Any(b => c.CaseCategories.Contains(((CaseCategoryOrTypeOfViolence)b).ToString())));
            //}

            //if (model.Year.Count > 0)
            //{
            //    allCases = allCases.Where(c => model.Year.Contains(c.DateReported.Year));
            //}

            //if (model.States.Count > 0)
            //{
            //    allCases = allCases.Where(c => model.States.Contains(c.IncidentStateId));
            //}

            //if (model.Lgas.Count > 0)
            //{
            //    allCases = allCases.Where(c => model.Lgas.Contains(c.IncidentLGAId));
            //}

            //if (model.StartDate.HasValue)
            //{
            //    allCases = allCases.Where(c => c.DateReported >= model.StartDate.Value.Date);
            //}

            //if (model.EndDate.HasValue)
            //{
            //    allCases = allCases.Where(c => c.DateReported <= model.EndDate.Value);
            //}

            //if (!string.IsNullOrWhiteSpace(model.Gender))
            //{
            //    allCases = allCases.Where(c => c.SexOfSurvior.Trim().ToLower() == model.Gender.Trim().ToLower());
            //}

            //if (model.MinimumAge.HasValue)
            //{
            //    allCases = allCases.Where(c => c.AgeOfSurvior >= model.MinimumAge);
            //}
            //if (model.MaximumAge.HasValue)
            //{
            //    allCases = allCases.Where(c => c.AgeOfSurvior <= model.MaximumAge);
            //}

            //order the cases by DateReported
            //var cases = await allCases.ProjectTo<HomePageMetricsProjectionModel>(_mapper.ConfigurationProvider)
            //    .OrderByDescending(c => c.DateReported).ToListAsync();

            var totalCases = allCases.DeferredCount().FutureValue();

            //Here begins the grouping of queried data to suite the displays
            //Note: Due to changes in this functionality some of this code is redundant

            //case by state
            //var caseByState = cases.GroupBy(c => c.IncidentStateId)
            //                          .Select(p => new CaseBySubject
            //                          {
            //                              Id = p.Key,
            //                              Name = p.FirstOrDefault()?.IncidentState.Name,
            //                              Count = p.Count(),
            //                          }).ToList();

            //case by incident type i.e caseCategory

            //var caseCategoryList = Enum.GetValues(typeof(CaseCategoryOrTypeOfViolence));

            //var caseByType = new List<CaseBySubject>();

            //foreach (var cat in caseCategoryList)
            //{
            //    var catInfo = cases.Where(c => c.CaseCategoriesList.Contains((int)cat));

            //    caseByType.Add(new CaseBySubject
            //    {
            //        Id = (int)cat,
            //        Name = cat.ToString(),
            //        Count = catInfo.Count()
            //    });
            //}

            //cases for the present year and grouped by month
            //var caseByMonthOfPresentYear = cases.Where(c => c.DateReported.Year == DateTime.Now.Year)
            //                            .GroupBy(c => c.DateReported.Month)
            //                            .Select(p => new CaseBySubject
            //                            {
            //                                Id = p.Key,
            //                                Name = p.FirstOrDefault()?.DateReported.ToString("MMM"),
            //                                Count = p.Count()
            //                            }).OrderBy(c => c.Id).ToList();

            //case by time of violence ie timeof the day
            //var casesbyTimeofViolence = cases.GroupBy(c => c.TimeOfDay)
            //                           .Select(p => new CaseBySubject
            //                           {
            //                               Id = (int)p.Key,
            //                               Name = p.FirstOrDefault()?.TimeOfDay.ToString(),
            //                               Count = p.Count()
            //                           }).ToList();

            var totalCasesFatal = allCases.DeferredCount(c => c.WasViolenceFatal == YesOrNo.Yes).FutureValue();

            //var totalApprovedCases = cases.Count(c => c.IsApproved);

            //var totalUnApprovedCases = cases.Count(c => c.IsApproved == false);

            var closedCasesYes = allCases.DeferredCount(c => c.HasCaseBeenClosed == YesOrNo.Yes).FutureValue();
            var closedCasesNotApplicable = allCases.DeferredCount(c => c.HasCaseBeenClosed == YesOrNo.NotApplicable).FutureValue();
            var ClosedCases = closedCasesYes + closedCasesNotApplicable;
            var OpenCases = allCases.DeferredCount(c => c.HasCaseBeenClosed == YesOrNo.No).FutureValue();

            //var doesSurviorWantJustice = cases.Count(c => c.DoestheSurviorWantJustice == YesOrNo.Yes);

            var convictedPerpetuator = allCases.DeferredCount(c => c.OutcomeOfProsecution.ToLower() == "conviction").FutureValue();

            //returns the model: some of these fields are now redundant
            var result = new HomeMetricsModel
            {
                //CaseByState = caseByState,
                TotalCases = totalCases,
                //TotalApprovedCases = totalApprovedCases,
                TotalFatalCases = totalCasesFatal,
                //TotalUnApprovedCases = totalUnApprovedCases,
                //NumberOfCSO = noofCSO,
                //NumberOfSP = noofSP,
                //CaseByIncidentType = caseByType,
                //NumberOfOrganisations = totalOrganisations,
                ClosedCases = ClosedCases,
                OpenCases = OpenCases,
                //CaseByMonthOfPresentYear = caseByMonthOfPresentYear,
                ConvictedPerpetuators = convictedPerpetuator,
                //CasesByTimeOfViolience = casesbyTimeofViolence,
                //NoOfPeopleWhoWantJustice = doesSurviorWantJustice,
            };

            //_queue.QueueAsyncTask(async () =>
            //{
            //    await _appCachingService.AddMetrics(key, result);
            //});

            return new AppResult<HomeMetricsModel>
            {
                Data = result,
                Message = "Successful",
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<AppResult<List<CaseBySubject>>> CasesByStateByLga(CaseByStateAndLgaFilter request)
        {
            var allCases = _context.Cases.AsNoTracking();

            if (request.States != null && request.States.Any())
            {
                allCases = allCases.Where(c => request.States.Contains(c.IncidentStateId));
            }
            if (request.Lgas != null && request.Lgas.Any())
            {
                allCases = allCases.Where(c => request.Lgas.Contains(c.IncidentLGAId));
            }

            var cases = await allCases.Select(c => c.HasCaseBeenClosed).ToListAsync();

            var casesByStateByLga = cases.Where(c => c != YesOrNo.NotApplicable).GroupBy(c => c)
                           .Select(p => new CaseBySubject
                           {
                               Id = (int)p.Key,
                               Name = p.FirstOrDefault() == YesOrNo.Yes ? "Closed Cases" : "Open Cases",
                               Count = p.Count()
                           }).ToList();

            return new AppResult<List<CaseBySubject>>
            {
                Data = casesByStateByLga,
                Message = "Successful",
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<AppResult<List<CaseBySubject>>> MonthlyCasesByWards(MonthlyCasesByWardsFilter request)
        {
            var allCases = _context.Cases.Where(c => c.DateReported.Year == DateTime.Now.Year).AsNoTracking();

            if (request.States != null && request.States.Any())
            {
                allCases = allCases.Where(c => request.States.Contains(c.IncidentStateId));
            }
            if (request.Lgas != null && request.Lgas.Any())
            {
                allCases = allCases.Where(c => request.Lgas.Contains(c.IncidentLGAId));
            }
            if (request.Wards != null && request.Wards.Any())
            {
                allCases = allCases.Where(c => c.IncidentWardId.HasValue && request.Lgas.Contains(c.IncidentWardId.Value));
            }

            var cases = await allCases.Select(c => c.DateReported).ToListAsync();

            var monthlyCasesByWards = new List<CaseBySubject>();

            var monthsOfYear = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

            foreach (int month in monthsOfYear)
            {
                var casesInMonth = cases.Where(c => c.Month == month);
                monthlyCasesByWards.Add(new CaseBySubject
                {
                    Id = month,
                    Name = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(month),
                    Count = casesInMonth.Count()
                });
            }

            return new AppResult<List<CaseBySubject>>
            {
                Data = monthlyCasesByWards,
                Message = "Successful",
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<AppResult<List<CaseBySubject>>> MonthlyServiceByStates(MonthlyCasesByWardsFilter request)
        {
            var allCases = _context.ServicesProvided
                .Where(c => c.DateOfServiceProvision.HasValue && c.DateOfServiceProvision.Value.Year == DateTime.Now.Year).AsNoTracking();

            if (request.States != null && request.States.Any())
            {
                allCases = allCases.Where(c => request.States.Contains(c.StateId));
            }
            if (request.Lgas != null && request.Lgas.Any())
            {
                allCases = allCases.Where(c => c.OrganisationLgaId.HasValue && request.Lgas.Contains(c.OrganisationLgaId.Value));
            }

            var cases = await allCases.Select(c => c.DateOfServiceProvision).ToListAsync();

            var monthlyCasesByWards = new List<CaseBySubject>();

            var monthsOfYear = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

            foreach (int month in monthsOfYear)
            {
                var casesInMonth = cases.Where(c => c.Value.Month == month);
                monthlyCasesByWards.Add(new CaseBySubject
                {
                    Id = month,
                    Name = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(month),
                    Count = casesInMonth.Count()
                });
            }

            return new AppResult<List<CaseBySubject>>
            {
                Data = monthlyCasesByWards,
                Message = "Successful",
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<AppResult<List<CaseBySubject>>> ServicesByTypeOfServiceProvided(CaseByStateAndLgaFilter request)
        {
            var allCases = _context.ServicesProvided.AsNoTracking();

            if (request.States != null && request.States.Any())
            {
                allCases = allCases.Where(c => request.States.Contains(c.StateId));
            }
            if (request.Lgas != null && request.Lgas.Any())
            {
                allCases = allCases.Where(c => c.OrganisationLgaId.HasValue && request.Lgas.Contains(c.OrganisationLgaId.Value));
            }

            var cases = await allCases.Select(c => new { TypeOfServiceProvided = c.TypeOfServiceProvided }).ToListAsync();

            var desrSerive = cases.ConvertAll(c => JsonConvert.DeserializeObject<List<string>>(c.TypeOfServiceProvided));

            var result = new List<CaseBySubject>();
            int othersCount = 0;

            foreach (var key in KeyLists.ServiceProvidedList)
            {
                var caseForKey = desrSerive.Where(c => c.Any(c => c.ToLower() == key.ToLower()));
                if (caseForKey.Count() == 0)
                {
                    othersCount++;
                }
                result.Add(new CaseBySubject
                {
                    Name = key,
                    Count = caseForKey.Count()
                });
            }

            result.Add(new CaseBySubject
            {
                Name = "Others",
                Count = othersCount
            });

            return new AppResult<List<CaseBySubject>>
            {
                Data = result,
                Message = "Successful",
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<AppResult<List<CaseBySubject>>> ServicesByStates()
        {
            var allCases = await _context.ServicesProvided.Select(c => new { State = c.State }).AsNoTracking().ToListAsync();

            var casesByState = allCases.GroupBy(c => c.State.Id)
                               .Select(p => new CaseBySubject
                               {
                                   Id = p.Key,
                                   Name = p.FirstOrDefault()?.State?.Name,
                                   Count = p.Count(),
                               }).ToList();

            return new AppResult<List<CaseBySubject>>
            {
                Data = casesByState,
                Message = "Successful",
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<AppResult<List<CaseBySubject>>> ServicesByOrg(DataByIds request)
        {
            var allCases = _context.ServicesProvided.AsNoTracking();

            if (request.Ids != null && request.Ids.Any())
            {
                allCases = allCases.Where(c => request.Ids.Contains(c.OrganisationId));
            }

            var cases = await allCases.Select(c => new { TypeOfServiceProvided = c.TypeOfServiceProvided }).ToListAsync();

            var desrSerive = cases.ConvertAll(c => JsonConvert.DeserializeObject<List<string>>(c.TypeOfServiceProvided));

            var result = new List<CaseBySubject>();
            int othersCount = 0;

            foreach (var key in KeyLists.ServiceProvidedList)
            {
                var caseForKey = desrSerive.Where(c => c.Any(c => c.ToLower() == key.ToLower()));
                if (desrSerive.Count() != 0)
                {
                    if (caseForKey.Count() == 0)
                    {
                        othersCount++;
                    }
                }
                result.Add(new CaseBySubject
                {
                    Name = key,
                    Count = caseForKey.Count()
                });
            }
            //MetricsKeys.ServiceProvided_Referral,
            //MetricsKeys.ServiceProvided_Education,
            //MetricsKeys.ServiceProvided_Psychosocial,
            //MetricsKeys.ServiceProvided_Medical,
            //MetricsKeys.ServiceProvided_SafeHouse,
            //MetricsKeys.ServiceProvided_Livelihood,
            //MetricsKeys.ServiceProvided_Legal,
            //MetricsKeys.ServiceProvided_PoliceSecurity,
            //MetricsKeys.None

            // var resu = desrSerive.Where(c => c.Any(c => c.ToLower() != MetricsKeys.ServiceProvided_Education.ToLower() || c.ToLower() != MetricsKeys.ServiceProvided_Referral.ToLower() || c.ToLower() != MetricsKeys.ServiceProvided_Medical.ToLower() || c.ToLower() != MetricsKeys.ServiceProvided_Psychosocial.ToLower() || c.ToLower() != MetricsKeys.ServiceProvided_SafeHouse.ToLower() || c.ToLower() != MetricsKeys.ServiceProvided_Livelihood.ToLower() || c.ToLower() != MetricsKeys.ServiceProvided_Legal.ToLower() || c.ToLower() != MetricsKeys.ServiceProvided_PoliceSecurity.ToLower() || c.ToLower() != MetricsKeys.None.ToLower()));
            //var resu = desrSerive.SelectMany(c => c.Select(x => x.ToLower().Equals(!KeyLists.ServiceProvidedList.Any())));


            result.Add(new CaseBySubject
            {
                Name = "Others",
                Count = othersCount
            });


            return new AppResult<List<CaseBySubject>>
            {
                Data = result,
                Message = "Successful",
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<AppResult<List<CaseBySubject>>> CsoCountByStates()
        {
            var allCases = await _context.Organisations.AsNoTracking().Select(c => new { States = c.States }).ToListAsync();

            var states = await _context.States.AsNoTracking().ToListAsync();

            var result = new List<CaseBySubject>();

            foreach (var state in states)
            {
                var orgsInState = allCases.Where(o => o.States.Contains($"{state.Id},") || o.States.Contains($",{state.Id}"));

                result.Add(new CaseBySubject
                {
                    Name = state.Name,
                    Count = orgsInState.Count()
                });
            }

            return new AppResult<List<CaseBySubject>>
            {
                Data = result,
                Message = "Successful",
                StatusCode = StatusCodes.Status200OK
            };
        }

        /// <summary>
        /// Exports the code dicitonary for data representation all options and fields in the database
        /// </summary>
        /// <returns></returns>
        public async Task<AppResult<MemoryStream>> ExportCodeDictionaryInExcel()
        {
            var CodeDictionary = await GetCodeDictionary();

            //use the code dictionary to populate excel sheet
            var package = await GetCodeDictionaryExcelSheet(CodeDictionary);

            var stream = new MemoryStream(package.GetAsByteArray());

            var newFile = $"CodeDictionary-{DateAndTime.Now:yyyy-mm-dd}.xlsx";

            return new AppResult<MemoryStream>
            {
                Data = stream,
                Message = newFile
            };
        }

        /// <summary>
        /// Export case data in Excel either coded or not coded
        /// </summary>
        /// <param name="model"></param>
        /// <param name="IsfullText">if true then the full data is exported</param>
        /// <returns></returns>
        public async Task<AppResult<MemoryStream>> ExportCaseDataInExcel(CaseSearchModel model, bool IsfullText)
        {
            try
            {
                var AppResult = await GetAllReportCases(model);
                var package = await GetCaseExcelSheet(AppResult, IsfullText);

                var stream = new MemoryStream(package.GetAsByteArray());

                var newFile = $"Report-{DateTime.Now:yyyy-MM-dd-hh-mm}.xlsx";

                return new AppResult<MemoryStream>
                {
                    Data = stream,
                    Message = newFile
                };
            }
            catch (Exception e)
            {
                return new AppResult<MemoryStream>
                {
                    Data = null,
                    Message = e.Message,
                    Errors = { e.Message }
                };
            }
        }

        public async Task<AppResult<string>> TestExportCaseDataInExcel(CaseSearchModel model, bool IsfullText, string id)
        {
            var resultModel = new AppResult<string>();
            UserViewModel user = null;
            try
            {
                var AppResult = await GetAllReportCasesWithUserId(model, id);

                if (AppResult.HasError || AppResult.Data.User == null)
                {
                    throw new Exception(AppResult.Errors.First());
                }
                user = AppResult.Data.User;
                var package = await GetCaseExcelSheet(AppResult.Data.Cases, IsfullText);

                var stream = package.GetAsByteArray();

                string newFile;
                if (model.StartDate != null || model.EndDate != null)
                    newFile = $"Cases Report from {model.StartDate:yyyy-MM-dd-hh-mm}-{model.EndDate:yyyy-MM-dd-hh-mm}.xlsx";
                else
                    newFile = $"Cases Report.xlsx";

                var result = await _emailService.SendCases(AppResult.Data.User, stream, newFile);
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

        #region redundant

        //private async Task<CaseViewBackground> GetAllReportCasesWithUserId(CaseSearchModel model, string id)
        //{
        //    if (id is null)
        //    {
        //        return new CaseViewBackground();
        //    }

        //    var User = await _context.Users.AsNoTracking()
        //        .Select(c => new UserViewModel
        //        {
        //            Designation = c.Designation,
        //            Role = c.Type,
        //            Email = c.Email,
        //            FirstName = c.FirstName,
        //            LastLogin = c.LastLogin,
        //            Id = c.Id,
        //            LastName = c.LastName,
        //            Organisation = c.OrganisationId.HasValue ? c.Organisation.Name : null,
        //            OrganisationId = c.OrganisationId,
        //            PhoneNumber = c.PhoneNumber,
        //            StateId = c.StateId,
        //            State = c.StateId.HasValue ? c.State.Name : null,
        //            StateCode = c.StateId.HasValue ? c.State.Code : null,
        //            IsActivated = c.IsActivated,
        //            LocalGovernments = c.LocalGovernments
        //            //LocalGovernmentId = c.LocalGovernmentAreaId
        //        }).FirstOrDefaultAsync(c => c.Id == id);

        //    if (User is null)
        //    {
        //        return new CaseViewBackground();
        //    }

        //    var query = _context.Cases
        //        .Include(c => c.CreatedBy)
        //        .Include(c => c.FollowUpActions)
        //        .AsNoTracking();

        //    //this was added bcos deleted users data are missen in data export
        //    var allUserInCases = _context.Users.Where(u => query.Select(c => c.CreatedBy).Contains(u))
        //        .IgnoreQueryFilters();

        //    //apply filters
        //    if (User.Role == RoleKeys.FederalSupervisor || User.Role == RoleKeys.StateAdministrator || User.Role == RoleKeys.StateSupervisor)
        //    {
        //        query = query.Where(c => c.IsValidated);
        //    }

        //    if (model.StartDate.HasValue)
        //    {
        //        query = query.Where(c => c.DateReported >= model.StartDate);
        //    }

        //    if (model.EndDate.HasValue)
        //    {
        //        query = query.Where(c => c.DateReported <= model.EndDate);
        //    }
        //    if (model.OrganisationId.HasValue && model.OrganisationId.Value > 0)
        //    {
        //        query = query.Where(c => c.OrganisationId == model.OrganisationId);
        //    }
        //    if (model.StateId.HasValue && model.StateId.Value > 0)
        //    {
        //        query = query.Where(c => c.IncidentStateId == model.StateId || !string.IsNullOrWhiteSpace(User.StateCode) && c.IncidentCode.ToLower().Contains(User.StateCode.ToLower()));
        //    }

        //    //if (User.LocalGovernmentId.HasValue && User.Role == RoleKeys.LocalGovernment)
        //    //{
        //    //    query = query.Where(c => c.IncidentLGAId == User.LocalGovernmentId);
        //    //}

        //    //check is user is a local government access
        //    if (User.LocalGovernments.Any() && User.Role == RoleKeys.LocalGovernment)
        //    {
        //        var userLgas = string.Join(",", User.LocalGovernments.Select(l => $",{l.Id},"));
        //        query = query.Where(c => userLgas.Contains("," + c.IncidentLGAId + ",") || c.CreatedById == User.Id);
        //    }

        //    if (model.IncidentLGAId.HasValue && model.IncidentLGAId.Value > 0)
        //    {
        //        var userLgas = string.Join(",", User.LocalGovernments.Select(l => $",{l.Id},"));
        //        query = query.Where(c => c.IncidentLGAId == model.IncidentLGAId || userLgas.Contains("," + c.IncidentLGAId + ","));
        //    }

        //    if (model.TimeOfDay.HasValue)
        //    {
        //        query = query.Where(c => c.TimeOfDay == model.TimeOfDay);
        //    }

        //    //if (model.CaseCategoryId.HasValue && model.CaseCategoryId.Value > 0)
        //    //{
        //    //    query = query.Where(c => c.CaseCategoryId == model.CaseCategoryId);
        //    //}

        //    if (!string.IsNullOrWhiteSpace(model.IncidentCode))
        //    {
        //        query = query.Where(c => c.IncidentCode.ToLower() == model.IncidentCode.ToLower());
        //    }

        //    if (model.IsCaseClosed.HasValue && model.IsCaseClosed.Value != YesOrNo.NotApplicable)
        //    {
        //        query = query.Where(c => c.HasCaseBeenClosed == model.IsCaseClosed);
        //    }
        //    if (model.IsApproved.HasValue)
        //    {
        //        query = query.Where(c => c.IsApproved == model.IsApproved.Value);
        //    }

        //    if (model.IsValidated.HasValue)
        //    {
        //        query = query.Where(c => c.IsValidated == model.IsValidated.Value);
        //    }

        //    if (!string.IsNullOrWhiteSpace(model.Gender))
        //    {
        //        query = query.Where(c => c.SexOfSurvior.Trim().ToLower() == model.Gender.Trim().ToLower());
        //    }

        //    if (model.MinimumAge.HasValue)
        //    {
        //        query = query.Where(c => c.AgeOfSurvior >= model.MinimumAge);
        //    }
        //    if (model.MaximumAge.HasValue)
        //    {
        //        query = query.Where(c => c.AgeOfSurvior <= model.MaximumAge);
        //    }

        //    if (!string.IsNullOrWhiteSpace(model.TypeOfServiceProvided))
        //    {
        //        query = query.Where(c => c.TypeOfServiceProvidedToSurvior.ToLower().Contains(model.TypeOfServiceProvided.Trim().ToLower()));
        //    }

        //    //query = query.Where(c => c.CreatedById == "75fc49a8-d98e-4eae-ae84-63cf98d66e81" && c.IncidentCode == "LA/3401/0819");

        //    var Cases = await query.Select(c => new CaseViewModel
        //    {
        //        Id = c.Id,
        //        CreatedAt = c.CreatedAt,
        //        IncidentCode = c.IncidentCode,
        //        ActualLocationOfIncident = c.ActualLocationOfIncident,
        //        ActualReferralServiceReceived = string.IsNullOrWhiteSpace(c.ActualReferralServiceReceived) ? null : JsonConvert.DeserializeObject<List<string>>(c.ActualReferralServiceReceived),
        //        AgeOfSurvior = c.AgeOfSurvior,
        //        CanBeEdited = c.CanBeEdited,
        //        Category = c.Category.Name,
        //        ContactChannel = c.ContactChannel,
        //        CreatedById = c.CreatedById,
        //        CreatedByName = c == null ? $"{allUserInCases.FirstOrDefault(u => u.Id == c.CreatedById).FirstName} {allUserInCases.FirstOrDefault(u => u.Id == c.CreatedById).LastName}" : $"{c.CreatedBy.FirstName} {c.CreatedBy.LastName}",
        //        DateOfIncident = c.DateOfIncident,
        //        DateReported = c.DateReported,
        //        DoesSurviorLiveAlone = c.DoesSurviorLiveAlone,
        //        DoestheSurviorWantJustice = c.DoestheSurviorWantJustice,
        //        Education = c.Education,
        //        EmploymentStatus = c.EmploymentStatus,
        //        EmploymentStatusOfParentOrGuardian = c.EmploymentStatusOfParentOrGuardian,
        //        FollowUpActionByCSO = string.IsNullOrWhiteSpace(c.FollowUpActionByCSO) ? null : JsonConvert.DeserializeObject<List<string>>(c.FollowUpActionByCSO),
        //        GBV_COVID19_Question1 = c.GBV_COVID19_Question1,
        //        GBV_COVID19_Question2 = c.GBV_COVID19_Question2,
        //        GBV_COVID19_Question3 = c.GBV_COVID19_Question3,
        //        GBV_COVID19_Question4 = c.GBV_COVID19_Question4,
        //        HasCaseBeenClosed = c.HasCaseBeenClosed,
        //        HasSurviorReceivedService = c.HasSurviorReceivedService,
        //        IncidentLGA = c.IncidentLGA.Name,
        //        IncidentLGAId = c.IncidentLGAId,
        //        IncidentState = c.IncidentState.Name,
        //        IncidentStateId = c.IncidentStateId,
        //        //complex check if incident ward is unknown
        //        IncidentWardId = !c.IncidentWardId.HasValue ? c.IncidentWardId : !_configuration.GetValue<bool>(StartupKeys.IsLive) && c.IncidentWardId == MetricsKeys.DevUnknown ? 0 : _configuration.GetValue<bool>(StartupKeys.IsLive) && c.IncidentWardId == MetricsKeys.LiveUnknown ? 0 : c.IncidentWardId,
        //        IncidentWard = c.IncidentWardId.HasValue ? c.IncidentWard.Name : null,
        //        IsSurviorContinuousThreat = c.IsSurviorContinuousThreat,
        //        MaritalStatus = c.MaritalStatus,
        //        NameOfServiceProviderReferredTo = c.NameOfServiceProviderReferredTo,
        //        NumberOfPerpetrators = c.NumberOfPerpetrators,
        //        Organisation = c.Organisation.Name,
        //        OrganisationId = c.OrganisationId,
        //        OrganisationType = c.Organisation.OrganisationType,
        //        OutcomeOfServiceorReferral = c.OutcomeOfServiceorReferral,
        //        SerialNumber = c.SerialNumber,
        //        SexOfSurvior = c.SexOfSurvior,
        //        TimeOfDay = c.TimeOfDay,
        //        PerpetratorsInformationList = c.PerpetratorsInformationList,
        //        TypeOfReferralServiceRequired = string.IsNullOrWhiteSpace(c.TypeOfReferralServiceRequired) ? null : JsonConvert.DeserializeObject<List<string>>(c.TypeOfReferralServiceRequired),
        //        TypeOfServiceProvidedToSurvior = string.IsNullOrWhiteSpace(c.TypeOfServiceProvidedToSurvior) ? null : JsonConvert.DeserializeObject<List<string>>(c.TypeOfServiceProvidedToSurvior),
        //        TypeOfServiceReceivedBySurvior = string.IsNullOrWhiteSpace(c.TypeOfServiceReceivedBySurvior) ? null : JsonConvert.DeserializeObject<List<string>>(c.TypeOfServiceReceivedBySurvior),
        //        VulnerablePopulation = c.VulnerablePopulation,
        //        WasViolenceFatal = c.WasViolenceFatal,
        //        WhoDoesSurviorLiveWith = c.WhoDoesSurviorLiveWith,
        //        WhoReportedIncident = c.WhoReportedIncident,
        //        IsApproved = c.IsApproved,
        //        IsValidated = c.IsValidated,
        //        ValidatedAt = c.ValidatedAt,
        //        ApprovedAt = c.ApprovedAt,
        //        ValidatedById = c.ValidatedById,
        //        ValidatedByName = string.IsNullOrWhiteSpace(c.ValidatedById) ? null : c.ValidatedBy == null ? $"{allUserInCases.FirstOrDefault(u => u.Id == c.ValidatedById).FirstName} {allUserInCases.FirstOrDefault(u => u.Id == c.ValidatedById).LastName}" : $"{c.ValidatedBy.FirstName} {c.ValidatedBy.LastName}",
        //        ApprovedById = c.ApprovedById,
        //        ApprovedByName = string.IsNullOrWhiteSpace(c.ApprovedById) ? null : c.ApprovedBy == null ? $"{allUserInCases.FirstOrDefault(u => u.Id == c.ApprovedById).FirstName} {allUserInCases.FirstOrDefault(u => u.Id == c.ApprovedById).LastName}" : $"{c.ApprovedBy.FirstName} {c.ApprovedBy.LastName}",
        //        WhoClosedTheCase = c.WhoClosedTheCase,
        //        Latitude = c.Latitude,
        //        Longitude = c.Longitude,
        //        UserState = c.CreatedBy.State.Name,
        //        ReferralOutcome = c.ReferralOutcome,
        //        FollowUps = c.FollowUpActions.Select(f => new FollowUpViewModel
        //        {
        //            FinalOutcome = f.FinalOutcome,
        //            CaseClosedDate = f.CaseClosedDate.GetValueOrDefault(),
        //            HasClientReceivedjustice = f.HasClientReceivedjustice,
        //            JusticeReceivedDate = f.JusticeReceivedDate,
        //            HasCaseBeenClosed = f.HasCaseBeenClosed,
        //            WhoClosedTheCase = f.WhoClosedTheCase
        //        }),
        //        LgaValidatedAt = c.LgaValidatedAt.GetValueOrDefault(),
        //        LgaValidatedByName = c.LgaValidatedBy.FirstName + " " + c.LgaValidatedBy.LastName,
        //        CaseCategoriesOthers = c.CaseCategoriesOthers,
        //        CaseCategories = c.CaseCategoriesList
        //    }).OrderBy(c => c.Organisation).AsSplitQuery().ToListAsync();

        //    var result = new CaseViewBackground()
        //    {
        //        Cases = Cases,
        //        User = User
        //    };
        //    return result;
        //}

        #endregion


        private async Task<AppResult<CaseViewBackground>> GetAllReportCasesWithUserId(CaseSearchModel model, string id)
        {
            var result = new AppResult<CaseViewBackground>()
            {
                Data = new CaseViewBackground()
            };
            var User = new UserViewModel();
            try
            {
                if (id is null)
                {
                    return new AppResult<CaseViewBackground>();
                }

                User = await _context.Users.AsNoTracking()
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

                if (User is null)
                {
                    return new AppResult<CaseViewBackground>();
                }

                var query = _context.Cases
                    .Include(c => c.CreatedBy)
                    .Include(c => c.FollowUpActions)
                    .AsNoTracking();

                //this was added bcos deleted users data are missen in data export
                var allUserInCases = _context.Users.Where(u => query.Select(c => c.CreatedBy).Contains(u))
                    .IgnoreQueryFilters();

                //apply filters
                //if (User.Role == RoleKeys.FederalSupervisor || User.Role == RoleKeys.StateAdministrator || User.Role == RoleKeys.StateSupervisor)
                //{
                //    query = query.Where(c => c.IsValidated);
                //}

                if (model.StartDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt.Date == model.StartDate.Value.Date);
                }

                if (model.EndDate.HasValue)
                {
                    query = query.Where(c => c.DateReported <= model.EndDate);
                }
                if (model.OrganisationId.HasValue && model.OrganisationId.Value > 0)
                {
                    query = query.Where(c => c.OrganisationId == model.OrganisationId);
                }
                if (model.StateId.HasValue && model.StateId.Value > 0)
                {
                    query = query.Where(c => c.IncidentStateId == model.StateId || !string.IsNullOrWhiteSpace(User.StateCode) && c.IncidentCode.ToLower().Contains(User.StateCode.ToLower()));
                }

                //if (User.LocalGovernmentId.HasValue && User.Role == RoleKeys.LocalGovernment)
                //{
                //    query = query.Where(c => c.IncidentLGAId == User.LocalGovernmentId);
                //}

                //check is user is a local government access
                if (User.LocalGovernments.Any() && User.Role == RoleKeys.LocalGovernment)
                {
                    var userLgas = string.Join(",", User.LocalGovernments.Select(l => $",{l.Id},"));
                    query = query.Where(c => userLgas.Contains("," + c.IncidentLGAId + ",") || c.CreatedById == User.Id);
                }

                if (model.IncidentLGAId.HasValue && model.IncidentLGAId.Value > 0)
                {
                    var userLgas = string.Join(",", User.LocalGovernments.Select(l => $",{l.Id},"));
                    query = query.Where(c => c.IncidentLGAId == model.IncidentLGAId || userLgas.Contains("," + c.IncidentLGAId + ","));
                }

                if (model.TimeOfDay.HasValue)
                {
                    query = query.Where(c => c.TimeOfDay == model.TimeOfDay);
                }

                //if (model.CaseCategoryId.HasValue && model.CaseCategoryId.Value > 0)
                //{
                //    query = query.Where(c => c.CaseCategoryId == model.CaseCategoryId);
                //}

                if (!string.IsNullOrWhiteSpace(model.IncidentCode))
                {
                    query = query.Where(c => c.IncidentCode.ToLower() == model.IncidentCode.ToLower());
                }

                if (model.IsCaseClosed.HasValue && model.IsCaseClosed.Value != YesOrNo.NotApplicable)
                {
                    query = query.Where(c => c.HasCaseBeenClosed == model.IsCaseClosed);
                }
                if (model.IsApproved.HasValue)
                {
                    query = query.Where(c => c.IsApproved == model.IsApproved.Value);
                }

                if (model.IsValidated.HasValue)
                {
                    query = query.Where(c => c.IsValidated == model.IsValidated.Value);
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

                //query = query.Where(c => c.CreatedById == "75fc49a8-d98e-4eae-ae84-63cf98d66e81" && c.IncidentCode == "LA/3401/0819");
                var LGAS = _context.LocalGovernmentAreas.AsNoTracking();

                var Cases = await query.Select(c => new CaseViewModel
                {
                    Id = c.Id,
                    CreatedAt = c.CreatedAt,
                    IncidentCode = c.IncidentCode,
                    ActualLocationOfIncident = c.ActualLocationOfIncident,
                    //ActualReferralServiceReceived = string.IsNullOrEmpty(c.ActualReferralServiceReceived) ? null : JsonConvert.DeserializeObject<List<string>>(c.ActualReferralServiceReceived),
                    AgeOfSurvior = c.AgeOfSurvior,
                    CanBeEdited = c.CanBeEdited,
                    Category = c.Category.Name,
                    ContactChannel = c.ContactChannel,
                    CreatedById = c.CreatedById,

                    CreatedByName = c == null ? $"{allUserInCases.FirstOrDefault(u => u.Id == c.CreatedById).FirstName} {allUserInCases.FirstOrDefault(u => u.Id == c.CreatedById).LastName}" : $"{c.CreatedBy.FirstName} {c.CreatedBy.LastName}",
                    DateOfIncident = c.DateOfIncident,
                    DateReported = c.DateReported,
                    DoesSurviorLiveAlone = c.DoesSurviorLiveAlone,
                    DoestheSurviorWantJustice = c.DoestheSurviorWantJustice,
                    Education = c.Education,
                    EmploymentStatus = c.EmploymentStatus,
                    EmploymentStatusOfParentOrGuardian = c.EmploymentStatusOfParentOrGuardian,
                    //FollowUpActionByCSO = string.IsNullOrEmpty(c.FollowUpActionByCSO) ? null : JsonConvert.DeserializeObject<List<string>>(c.FollowUpActionByCSO),
                    //GBV_COVID19_Question1 = c.GBV_COVID19_Question1,
                    //GBV_COVID19_Question2 = c.GBV_COVID19_Question2,
                    //GBV_COVID19_Question3 = c.GBV_COVID19_Question3,
                    //GBV_COVID19_Question4 = c.GBV_COVID19_Question4,
                    HasCaseBeenClosed = c.HasCaseBeenClosed,
                    HasSurviorReceivedService = c.HasSurviorReceivedService,

                    IncidentLGA = c.IncidentLGA.Name,
                    IncidentLGAId = c.IncidentLGAId,
                    IncidentState = c.IncidentState.Name,
                    IncidentStateId = c.IncidentStateId,
                    //complex check if incident ward is unknown
                    IncidentWardId = !c.IncidentWardId.HasValue ? c.IncidentWardId : !_configuration.GetValue<bool>(StartupKeys.IsLive) && c.IncidentWardId == MetricsKeys.DevUnknown ? 0 : _configuration.GetValue<bool>(StartupKeys.IsLive) && c.IncidentWardId == MetricsKeys.LiveUnknown ? 0 : c.IncidentWardId,
                    IncidentWard = c.IncidentWardId.HasValue ? c.IncidentWard.Name : null,
                    IsSurviorContinuousThreat = c.IsSurviorContinuousThreat,
                    MaritalStatus = c.MaritalStatus,
                    //NameOfServiceProviderReferredTo = c.NameOfServiceProviderReferredTo,
                    NumberOfPerpetrators = c.NumberOfPerpetrators,
                    Organisation = c.Organisation.Name,
                    OrganisationLGA = LGAS.FirstOrDefault(x => x.Id == c.OrganisationLgaId).Name,
                    //OrganisationLgaId = c.OrganisationLgaId,
                    OrganisationId = c.OrganisationId,
                    OrganisationType = c.Organisation.OrganisationType,
                    //OutcomeOfServiceorReferral = c.OutcomeOfServiceorReferral,
                    SerialNumber = c.SerialNumber,
                    SexOfSurvior = c.SexOfSurvior,
                    TimeOfDay = c.TimeOfDay,
                    PerpetratorsInformationList = c.PerpetratorsInformationList,
                    //TypeOfReferralServiceRequired = string.IsNullOrEmpty(c.TypeOfReferralServiceRequired) ? null : JsonConvert.DeserializeObject<List<string>>(c.TypeOfReferralServiceRequired),
                    //TypeOfServiceProvidedToSurvior = string.IsNullOrEmpty(c.TypeOfServiceProvidedToSurvior) ? null : JsonConvert.DeserializeObject<List<string>>(c.TypeOfServiceProvidedToSurvior),
                    //TypeOfServiceReceivedBySurvior = string.IsNullOrEmpty(c.TypeOfServiceReceivedBySurvior) ? null : JsonConvert.DeserializeObject<List<string>>(c.TypeOfServiceReceivedBySurvior),
                    VulnerablePopulation = string.IsNullOrWhiteSpace(c.VulnerablePopulation) ? null : JsonConvert.DeserializeObject<List<string>>(c.VulnerablePopulation),
                    WasViolenceFatal = c.WasViolenceFatal,
                    WhoDoesSurviorLiveWith = c.WhoDoesSurviorLiveWith,
                    WhoReportedIncident = c.WhoReportedIncident,
                    IsApproved = c.IsApproved,
                    IsValidated = c.IsValidated,
                    ValidatedAt = c.ValidatedAt,
                    ApprovedAt = c.ApprovedAt,
                    ValidatedById = c.ValidatedById,
                    ValidatedByName = string.IsNullOrEmpty(c.ValidatedById) ? null : c.ValidatedBy == null ? $"{allUserInCases.FirstOrDefault(u => u.Id == c.ValidatedById).FirstName} {allUserInCases.FirstOrDefault(u => u.Id == c.ValidatedById).LastName}" : $"{c.ValidatedBy.FirstName} {c.ValidatedBy.LastName}",
                    ApprovedById = c.ApprovedById,
                    ApprovedByName = string.IsNullOrEmpty(c.ApprovedById) ? null : c.ApprovedBy == null ? $"{allUserInCases.FirstOrDefault(u => u.Id == c.ApprovedById).FirstName} {allUserInCases.FirstOrDefault(u => u.Id == c.ApprovedById).LastName}" : $"{c.ApprovedBy.FirstName} {c.ApprovedBy.LastName}",
                    WhoClosedTheCase = c.WhoClosedTheCase,
                    //Latitude = c.Latitude,
                    //Longitude = c.Longitude,
                    UserState = c.CreatedBy.State.Name,
                    ReferralOutcome = c.ReferralOutcome,
                    CaseClosedDate = c.CaseClosedDate,
                    DateJusticeReceived = c.DateJusticeReceived,

                    FollowUps = c.FollowUpActions.Select(f => new FollowUpViewModel
                    {
                        FinalOutcome = f.FinalOutcome,
                        CaseClosedDate = f.CaseClosedDate.GetValueOrDefault(),
                        HasClientReceivedjustice = f.HasClientReceivedjustice,
                        JusticeReceivedDate = f.JusticeReceivedDate,
                        HasCaseBeenClosed = f.HasCaseBeenClosed,
                        WhoClosedTheCase = f.WhoClosedTheCase
                    }),
                    LgaValidatedAt = c.LgaValidatedAt.GetValueOrDefault(),
                    LgaValidatedByName = c.LgaValidatedBy.FirstName + " " + c.LgaValidatedBy.LastName,
                    CaseCategoriesOthers = c.CaseCategoriesOthers,
                    CaseCategories = c.CaseCategoriesList,
                    OutcomeOfProsecution = c.OutcomeOfProsecution
                }).OrderBy(c => c.CreatedAt).AsSplitQuery().ToListAsync();


                result.Data.Cases = Cases;
                result.Data.User = User;
            }
            catch (Exception ex)
            {
                result.AddError($"{ex.Message ?? ex.InnerException.Message}");
                result.Data.User = User;
            }

            return result;
        }

        /// <summary>
        /// For updating entries in cases actual location
        /// </summary>
        /// <returns></returns>
        public async Task<AppResult<string>> UpdateDb()
        {
            var resultModel = new AppResult<string>();
            try
            {
                var cases = _context.Cases.Where(x => x.ActualLocationOfIncident.ToLower().Contains("client"));
                foreach (var item in cases)
                {
                    item.ActualLocationOfIncident = "Survivor/victim's home";
                }
                _context.Cases.UpdateRange(cases);
                await _context.SaveChangesAsync();
                resultModel.Data = "success";
            }
            catch (Exception ex)
            {
                resultModel.AddError($"{ex.Message ?? ex.InnerException.Message}");
                throw;
            }
            return resultModel;
        }
        /// <summary>
        /// For changing Vulnerable population to seriazed List of string
        /// </summary>
        /// <returns></returns>
        public async Task<AppResult<string>> Cron()
        {
            var resultModel = new AppResult<string>();
            try
            {
                var cases = _context.Cases.Where(x => x.CreatedAt.Date == new DateTime(2023, 2, 5, 0, 0, 0));
                //var list = new List<string>();
                foreach (var item in cases)
                {
                    //List<string> list = new List<string>().Add(item.VulnerablePopulation);
                    item.VulnerablePopulation ??= "";
                    var lis = new List<string>() { item.VulnerablePopulation };
                    item.VulnerablePopulation = JsonConvert.SerializeObject(lis);
                    //list.Remove(item.VulnerablePopulation);
                }
                _context.Cases.UpdateRange(cases);
                await _context.SaveChangesAsync();
                resultModel.Data = "success";
            }
            catch (Exception ex)
            {
                resultModel.AddError($"{ex.Message ?? ex.InnerException.Message}");
                throw;
            }
            return resultModel;
        }

        //public async Task<AppResult<string>> Cron()
        //{
        //    BackgroundJob.Enqueue(() => SerializeVulnerablePopulation());
        //    return new AppResult<string>
        //    {
        //        Data = "Started",
        //        Message = "success"
        //    };
        //}
        public async Task<AppResult<string>> ExportCaseDataInExcelBackground(CaseSearchModel model, bool IsfullText)
        {
            BackgroundJob.Enqueue(() => TestExportCaseDataInExcel(model, IsfullText, UserId));

            return new AppResult<string>
            {
                Data = "You will be notified in your email",
                Message = "success"
            };
        }
        /// <summary>
        /// Exports case data in SPSS
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<MemoryStream>> ExportDataInSPSS(CaseSearchModel model)
        {
            try
            {
                var codeDictionary = await GetCodeDictionary();

                var AppResult = await GetAllReportCases(model);

                var variables = SetSPSSVariables(codeDictionary);

                var stream = await WriteSPSSRecord(variables, AppResult, codeDictionary);

                var newFile = $"Report-{DateTime.Now:yyyy-MM-dd-hh-mm}.sav";

                return new AppResult<MemoryStream>
                {
                    Data = stream,
                    Message = newFile,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception e)
            {
                return new AppResult<MemoryStream>
                {
                    Message = e.Message,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = { e.Message }
                };
            }
        }

        private async Task<(bool status, string message, (IQueryable<Case> cases, UserViewModel user) data)> GetCasesForMonthlyReport(DateModel model)
        {
            //check login authorization
            if (UserId is null)
            {
                return (false, "Unauthorized User", default);
            }

            if (!model.StartDate.HasValue) return (false, "Please specify start date", default);

            var User = await _context.Users.AsNoTracking()
                              .Select(c => new UserViewModel
                              {
                                  Designation = c.Designation,
                                  Role = c.Type,
                                  Email = c.Email,
                                  FirstName = c.FirstName,
                                  Id = c.Id,
                                  LastName = c.LastName,
                                  Organisation = c.OrganisationId.HasValue ? c.Organisation.Name : null,
                                  OrganisationId = c.OrganisationId,
                                  PhoneNumber = c.PhoneNumber,
                                  StateId = c.StateId,
                                  State = c.StateId.HasValue ? c.State.Name : null,
                                  StateCode = c.StateId.HasValue ? c.State.Code : null,
                              }).FirstOrDefaultAsync(c => c.Id == UserId);

            if (User is null) return (false, "Unauthorized User", default);

            var AllCases = _context.Cases
                          .Include(c => c.IncidentState)
                          .Include(c => c.Category)
                          .Include(c => c.FollowUpActions)
                          .Include(c => c.IncidentLGA).AsNoTracking();

            //filter if the user belongs to a particular organisation
            if (User.OrganisationId.HasValue)
            {
                AllCases = AllCases.Where(c => c.OrganisationId == User.OrganisationId.Value);
            }

            if (User.Role == RoleKeys.StateAdministrator || User.Role == RoleKeys.StateSupervisor)
            {
                AllCases = AllCases.Where(c => c.IncidentStateId == User.StateId || !string.IsNullOrWhiteSpace(User.StateCode) && c.IncidentCode.ToLower().Contains(User.StateCode.ToLower()));
            }

            if (model.StateId.HasValue)
            {
                AllCases = AllCases.Where(c => c.IncidentStateId == model.StateId);
            }

            if (model.IsApproved.HasValue)
            {
                AllCases = AllCases.Where(c => c.IsApproved == model.IsApproved.Value);
            }

            if (model.IsValidated.HasValue)
            {
                AllCases = AllCases.Where(c => c.IsValidated == model.IsValidated.Value);
            }

            //filter by month and year
            //AllCases = AllCases.Where(c => c.DateReported.Year == model.Year && c.DateReported.Month == model.Month);

            if (!model.EndDate.HasValue)
            {
                model.EndDate = DateTime.Now;
            }

            AllCases = AllCases.Where(c => c.DateReported >= model.StartDate && c.DateReported <= model.EndDate);

            //get all cases
            var cases = AllCases.OrderByDescending(c => c.DateReported);

            return (true, "Success", (cases, User));
        }

        /// <summary>
        /// Exports monthly report
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<MemoryStream>> ExportMonthlyReport(DateModel model)
        {
            var casesQuery = await GetCasesForMonthlyReport(model);

            if (!casesQuery.status) return new AppResult<MemoryStream>
            {
                Message = casesQuery.message,
                StatusCode = StatusCodes.Status400BadRequest,
            };

            var cases = casesQuery.data.cases;

            //get cases by AgeGroup by sex
            //var caseByAgeGroupBySex = GetCaseByAgeGroupBySex(cases); --compose

            //get the vulnerable population count by sex
            //var vulnerablePopulationBySex = GetCaseByVulnerablePopulation(cases).Item2; --compose

            //get case by violence type  by sex

            var typeOfViolenseList = new List<string>
            {
                MetricsKeys.TypeOfViolence_PhysicalAssault,
                MetricsKeys.TypeOfViolence_SexualAssault ,
                MetricsKeys.TypeOfViolence_Defilement ,
                MetricsKeys.TypeOfViolence_ForcedMarriage ,
                MetricsKeys.TypeOfViolence_Rape ,
                MetricsKeys.TypeOfViolence_EmotionalPsychological ,
                MetricsKeys.TypeOfViolence_DenialOfResourcesServices ,
                MetricsKeys.TypeOfViolence_ViolationOfPropertyAndInheritanceRight,
                MetricsKeys.TypeOfViolence_ChildAbuseAndNeglect,
                MetricsKeys.TypeOfViolence_EarlyMarriage,
                MetricsKeys.TypeOfViolence_CyberBullying,
            };

            var caseByType = new List<CaseBySubjectBySex>();
            foreach (var item in typeOfViolenseList)
            {
                var caseType = cases.Where(c => c.Category.Name.ToLower() == item.ToLower());

                caseByType.Add(new CaseBySubjectBySex
                {
                    Subject = item,
                    FemaleCount = caseType.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female),
                    MaleCount = caseType.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                    OtherCount = caseType.Count(c => c.SexOfSurvior.ToLower() != MetricsKeys.Male && c.SexOfSurvior.ToLower() != MetricsKeys.Female),
                    TotalNewCases = caseType.Count(c => c.HasSurviorReceivedService == YesOrNo.No),
                    TotalFollowUpCases = caseType.Count(c => c.HasSurviorReceivedService == YesOrNo.Yes)
                });

                if (!caseByType.Any(c => c.Subject.ToLower() == item.ToLower()))
                {
                    caseByType.Add(new CaseBySubjectBySex
                    {
                        Subject = item
                    });
                }
            }


            { //app owner doesn't want rows the zero data to be excluded so empty rows are added for categries with zero data
                //add empty rows
                foreach (var item in typeOfViolenseList)
                {
                    if (!caseByType.Any(c => c.Subject.ToLower() == item.ToLower()))
                    {
                        caseByType.Add(new CaseBySubjectBySex
                        {
                            Subject = item
                        });
                    }
                }

                //add empty other row
                if (!caseByType.Any(c => c.Subject.ToLower() == "other"))
                {
                    caseByType.Add(new CaseBySubjectBySex
                    {
                        Subject = "Other"
                    });
                }
            }

            var caseByTypesofServiceBySex = GetCaseByTypeOfService(cases).Item2;

            var casebyLocationsOfViolence = GetCaseByLocationOfViolenceBySex(cases);

            var (Convicted, Court) = GetCaseByViolenceBySexForConvicted(cases);

            //create excel document

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            ExcelPackage Excelpackage = new ExcelPackage();
            ExcelWorksheet WorkSheet = Excelpackage.Workbook.Worksheets.Add($"Monthly Report_{DateTime.Now:MMM-dd-yy}");

            //set heading and style
            WorkSheet.Cells["B2:C2"].Style.Border.Top.Style = ExcelBorderStyle.Thick;
            WorkSheet.Cells["B1"].Value = "Rapid Service Provider's GBV COVID-19 Monthly Summary Form";
            WorkSheet.Cells["B1"].Style.Font.Bold = true;
            WorkSheet.Cells["B1:C1"].Merge = true;
            WorkSheet.Cells["B2:C2"].Merge = true;
            WorkSheet.Cells["B3:C3"].Merge = true;
            WorkSheet.Cells["B2"].Value = $"Name of Organisation: {casesQuery.data.user.Organisation}";
            WorkSheet.Cells["B3"].Value = $"State: {casesQuery.data.user.State}";
            //WorkSheet.Cells["B4"].Value = $"Reporting month: {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(model.Month)}";
            //WorkSheet.Cells["C4"].Value = $"Reporting year: {model.Year}";
            //WorkSheet.Cells["B4"].Value = $"Reporting month: {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(model.Month)}";
            WorkSheet.Cells["B4"].Value = $"Reporting period: {model.StartDate.Value.ToLongDateString()} - {model.EndDate.Value.ToLongDateString()}";
            WorkSheet.Column(2).Style.WrapText = true;
            WorkSheet.Column(2).Width = 30;
            WorkSheet.Column(3).Style.WrapText = true;
            WorkSheet.Column(3).Width = 30;

            WorkSheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            WorkSheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            WorkSheet.Column(2).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            WorkSheet.Column(3).Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            WorkSheet.Cells[$"B2:B4"].Style.Border.Left.Style = ExcelBorderStyle.Thick;
            WorkSheet.Cells[$"C2:C4"].Style.Border.Right.Style = ExcelBorderStyle.Thick;

            int startRow = 5;
            Color colFromHex = ColorTranslator.FromHtml("#B3B4B5");

            startRow = DisplayCaseCategory(
                "Type of violence reported",
                WorkSheet,
                startRow,
                caseByType,
                colFromHex);

            startRow = DisplayCaseCategory(
                "Location of violence",
                WorkSheet,
                startRow,
                casebyLocationsOfViolence,
                colFromHex);

            startRow = DisplayCaseCategory(
                "Type of service provided to survivor",
                WorkSheet,
                startRow,
                caseByTypesofServiceBySex,
                colFromHex);

            startRow = DisplayCaseCategory(
                "Number of cases reported to the police that have been brought to court",
                WorkSheet,
                startRow,
                Court,
                colFromHex);

            startRow = DisplayCaseCategory(
                "Number of cases reported to the police that have resulted in conviction of perpetrators",
                WorkSheet,
                startRow,
                Convicted,
                colFromHex);

            WorkSheet.Cells[$"B{startRow}:G{startRow}"].Style.Border.Top.Style = ExcelBorderStyle.Thick;

            WorkSheet.Cells[$"B{startRow}"].Value = "Report completed by: ";
            WorkSheet.Cells[$"D{startRow}"].Value = "Designation: ";
            WorkSheet.Cells[$"F{startRow}"].Value = "Sign:____________________ ";
            WorkSheet.Cells[$"G{startRow}"].Value = "Date:____________________ ";

            WorkSheet.Cells[$"D{startRow}:E{startRow}"].Merge = true;

            WorkSheet.Cells[$"B{++startRow}"].Value = "Validated by: ";
            WorkSheet.Cells[$"D{startRow}"].Value = "Designation: ";
            WorkSheet.Cells[$"F{startRow}"].Value = "Sign:____________________ ";
            WorkSheet.Cells[$"G{startRow}"].Value = "Date:____________________ ";

            WorkSheet.Cells[$"D{startRow}:E{startRow}"].Merge = true;

            await Excelpackage.SaveAsync();

            var stream = new MemoryStream(Excelpackage.GetAsByteArray());

            var newFile = $"Report-{DateTime.Now:yyyy-MM-dd-hh-mm}.xlsx";

            return new AppResult<MemoryStream>
            {
                Data = stream,
                Message = newFile
            };
        }

        public void SummarySheetOutcome(ExcelPackage excelPackage, IEnumerable<AdminDashboardProjectModel> listCases)
        {
            ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets.Add($"Summary Sheet - Outcome");

            int startRow = 3;

            listCases = listCases.Where(c => c.DoestheSurviorWantJustice == YesOrNo.Yes);

            startRow = ByOutcomeBySex(workSheet, listCases, startRow, "Number of GBV cases prosecuted (outcome)");
            startRow = ByAgeRanges(workSheet, listCases, startRow, "Number of GBV cases prosecuted (age/sex)");
            startRow = ByRelationShipBySex(workSheet, listCases, startRow, "Number of GBV cases prosecuted (sex/survivor's relationship with perpetrator) ");
            startRow = ByVulPopulationBySex(workSheet, listCases, startRow, "Number of GBV cases prosecuted (sex/vulnerability population)");
            startRow = ByIncomeBySex(workSheet, listCases, startRow, "Number of GBV cases prosecuted (estimated average monthly income in Naira)");
            startRow = ByEmploymentStatusBySex(workSheet, listCases, startRow, "Number of GBV cases prosecuted (employment status of survivor/victim)");
            startRow = ByEducationBySex(workSheet, listCases, startRow, "Number of GBV cases prosecuted (education)");
            ByTypeOfViolece(workSheet, listCases, startRow, "Type of GBV violence prosecuted");
        }

        public void SummarySheetCases(ExcelPackage excelPackage, IEnumerable<AdminDashboardProjectModel> listCases)
        {
            ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets.Add($"Summary Sheet - Cases");

            int startRow = 3;

            startRow = ByAgeRanges(workSheet, listCases, startRow, "Number of GBV cases reported (age/sex)");
            startRow = ByRelationShipBySex(workSheet, listCases, startRow, "Number of GBV cases reported (sex/survivor's relationship with perpetrator) ");
            startRow = ByVulPopulationBySex(workSheet, listCases, startRow, "Number of GBV cases reported (sex/vulnerability population)");
            startRow = ByIncomeBySex(workSheet, listCases, startRow, "Number of GBV cases reported (estimated average monthly income in Naira)");
            startRow = ByEmploymentStatusBySex(workSheet, listCases, startRow, "Number of GBV cases reported (employment status of survivor/victim)");
            startRow = ByEducationBySex(workSheet, listCases, startRow, "Number of GBV cases reported (education)");
            startRow = ByWhoReportedBySex(workSheet, listCases, startRow, "Number of GBV cases reported (type of report)");
            ByTypeOfViolece(workSheet, listCases, startRow, "Type of GBV violence experienced");
        }

        private int ByOutcomeBySex(ExcelWorksheet workSheet, IEnumerable<AdminDashboardProjectModel> listCases, int startRow, string label)
        {
            var byOutcomeBySex = new List<CaseBySubjectBySex>();
            //Number of GBV cases prosecuted (outcome)
            var recantation = listCases.Where(c => c.OutcomeOfProsecution == MetricsKeys.OutcomeOfProsecutionOptions_Recantation);

            byOutcomeBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Recantation",
                MaleCount = recantation.Where(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male).Count(),
                FemaleCount = recantation.Where(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female).Count(),
            });

            var falses = listCases.Where(c => c.OutcomeOfProsecution == MetricsKeys.OutcomeOfProsecutionOptions_False);

            byOutcomeBySex.Add(new CaseBySubjectBySex
            {
                Subject = "False",
                MaleCount = falses.Where(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male).Count(),
                FemaleCount = falses.Where(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female).Count(),
            });

            var drop = listCases.Where(c => c.OutcomeOfProsecution == MetricsKeys.OutcomeOfProsecutionOptions_DroppedAtInvestigation);

            byOutcomeBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Dropped at Investigation",
                MaleCount = drop.Where(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male).Count(),
                FemaleCount = drop.Where(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female).Count(),
            });

            var disc = listCases.Where(c => c.OutcomeOfProsecution == MetricsKeys.OutcomeOfProsecutionOptions_DiscontinuedByProsecution);

            byOutcomeBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Discontinued by Prosecution",
                MaleCount = disc.Where(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male).Count(),
                FemaleCount = disc.Where(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female).Count(),
            });

            var acquittal = listCases.Where(c => c.OutcomeOfProsecution == MetricsKeys.OutcomeOfProsecutionOptions_Acquittal);

            byOutcomeBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Acquittal",
                MaleCount = acquittal.Where(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male).Count(),
                FemaleCount = acquittal.Where(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female).Count(),
            });

            var conviction = listCases.Where(c => c.OutcomeOfProsecution == MetricsKeys.OutcomeOfProsecutionOptions_Conviction);

            byOutcomeBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Conviction",
                MaleCount = conviction.Where(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male).Count(),
                FemaleCount = conviction.Where(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female).Count(),
            });

            return NewMonthlyDisplayCaseCategory(label, workSheet, startRow, byOutcomeBySex);
        }

        private int ByAgeRanges(ExcelWorksheet workSheet, IEnumerable<AdminDashboardProjectModel> listCases, int startRow, string label)
        {
            var ageRanges = new List<(int? Min, int? Max)>()
            {
                {(0 , 9)},
                {(10 ,14)},
                {(15, 19)},
                {(20 , 24)},
                {(25 , 29)},
                {(30 ,34)},
                {(40 , 44)},
                {(45 , 49)},
                {(50, 1000)}
            };
            var byAgeSex = GroupCaseByAgeBySex(ageRanges, listCases);

            return NewMonthlyDisplayCaseCategory(label, workSheet, startRow, byAgeSex);
        }

        private int ByRelationShipBySex(ExcelWorksheet workSheet, IEnumerable<AdminDashboardProjectModel> listCases, int startRow, string label)
        {
            var byRelationShipBySex = new List<CaseBySubjectBySex>();
            //byRelationShipBySex
            var relation = GetCaseByRelationShipBySex(listCases);

            var intimate = relation
                .Where(r => r.Subject == MetricsKeys.Relationship_curentpartner
               || r.Subject == MetricsKeys.Relationship_formerpartner);
            var notIntimate = relation
                .Where(r => !(r.Subject == MetricsKeys.Relationship_curentpartner
               || r.Subject == MetricsKeys.Relationship_formerpartner) && r.Subject != "Other");

            var otherRelationShip = relation.Where(r => r.Subject.ToLower() == "other");

            byRelationShipBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Intimate",
                MaleCount = intimate.Sum(i => i.MaleCount),
                FemaleCount = intimate.Sum(i => i.FemaleCount)
            });

            byRelationShipBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Non-Intimate Partner",
                MaleCount = notIntimate.Sum(i => i.MaleCount),
                FemaleCount = notIntimate.Sum(i => i.FemaleCount)
            });

            byRelationShipBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Others",
                MaleCount = otherRelationShip.Sum(i => i.MaleCount),
                FemaleCount = otherRelationShip.Sum(i => i.FemaleCount)
            });

            return NewMonthlyDisplayCaseCategory(label, workSheet, startRow, byRelationShipBySex);
        }

        private int ByVulPopulationBySex(ExcelWorksheet workSheet, IEnumerable<AdminDashboardProjectModel> listCases, int startRow, string label)
        {
            var byVulPopulationBySex = new List<CaseBySubjectBySex>();
            //byVulPopulationBySex
            var vulpop = GetCaseByVulnerablePopulation(listCases).bySubjectBySex;

            var personsLivingWithDisability = vulpop.Where(c => c.Subject == MetricsKeys.VulnerablePopulation_PLWD);
            byVulPopulationBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Persons living with disability",
                MaleCount = personsLivingWithDisability.Sum(i => i.MaleCount),
                FemaleCount = personsLivingWithDisability.Sum(i => i.FemaleCount)
            });

            var personsWhoUseDrugs = vulpop.Where(c => c.Subject == MetricsKeys.VulnerablePopulation_DrugUser);
            byVulPopulationBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Persons who use drugs",
                MaleCount = personsLivingWithDisability.Sum(i => i.MaleCount),
                FemaleCount = personsLivingWithDisability.Sum(i => i.FemaleCount)
            });

            var keyPopulation = vulpop.Where(c => c.Subject == MetricsKeys.VulnerablePopulation_FSW);
            byVulPopulationBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Key Population",
                MaleCount = personsLivingWithDisability.Sum(i => i.MaleCount),
                FemaleCount = personsLivingWithDisability.Sum(i => i.FemaleCount)
            });

            var vulnerablePopulation = vulpop.Where(c => c.Subject == MetricsKeys.VulnerablePopulation_Minor
            || c.Subject == MetricsKeys.VulnerablePopulation_OutSchoolChil || c.Subject == MetricsKeys.VulnerablePopulation_Widow
            );
            byVulPopulationBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Vulnerable Population",
                MaleCount = personsLivingWithDisability.Sum(i => i.MaleCount),
                FemaleCount = personsLivingWithDisability.Sum(i => i.FemaleCount)
            });

            var others = vulpop.Where(c => c.Subject.ToLower() == "other");
            byVulPopulationBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Other categories",
                MaleCount = personsLivingWithDisability.Sum(i => i.MaleCount),
                FemaleCount = personsLivingWithDisability.Sum(i => i.FemaleCount)
            });

            return NewMonthlyDisplayCaseCategory(label, workSheet, startRow, byVulPopulationBySex);
        }

        private int ByIncomeBySex(ExcelWorksheet workSheet, IEnumerable<AdminDashboardProjectModel> listCases, int startRow, string label)
        {
            //income

            var byincomeBySex = new List<CaseBySubjectBySex>();

            var noincome = listCases.Where(c => c.SurvivorEstimatedAverageMonthlyIncome == MetricsKeys.SurvivorEstimatedAverageMonthlyIncome_NoIncome);

            byincomeBySex.Add(new CaseBySubjectBySex
            {
                Subject = "No income",
                MaleCount = noincome.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = noincome.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            var SurvivorEstimatedAverageMonthlyIncome_LessThan1000 = listCases.Where(c => c.SurvivorEstimatedAverageMonthlyIncome == MetricsKeys.SurvivorEstimatedAverageMonthlyIncome_LessThan1000);

            byincomeBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Less than 1,000",
                MaleCount = SurvivorEstimatedAverageMonthlyIncome_LessThan1000.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = SurvivorEstimatedAverageMonthlyIncome_LessThan1000.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            var SurvivorEstimatedAverageMonthlyIncome_Between_1001_10000 = listCases.Where(c => c.SurvivorEstimatedAverageMonthlyIncome == MetricsKeys.SurvivorEstimatedAverageMonthlyIncome_Between_1001_10000);

            byincomeBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Between 1,001 – 10,000",
                MaleCount = SurvivorEstimatedAverageMonthlyIncome_Between_1001_10000.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = SurvivorEstimatedAverageMonthlyIncome_Between_1001_10000.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            var SurvivorEstimatedAverageMonthlyIncome_Between100001_100000 = listCases.Where(c => c.SurvivorEstimatedAverageMonthlyIncome == MetricsKeys.SurvivorEstimatedAverageMonthlyIncome_Between100001_100000);

            byincomeBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Between 10,0001 – 100,000",
                MaleCount = SurvivorEstimatedAverageMonthlyIncome_Between100001_100000.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = SurvivorEstimatedAverageMonthlyIncome_Between100001_100000.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            var SurvivorEstimatedAverageMonthlyIncome_Between_1000001_500000 = listCases.Where(c => c.SurvivorEstimatedAverageMonthlyIncome == MetricsKeys.SurvivorEstimatedAverageMonthlyIncome_Between_1000001_500000);

            byincomeBySex.Add(new CaseBySubjectBySex
            {
                Subject = MetricsKeys.SurvivorEstimatedAverageMonthlyIncome_Between_1000001_500000,
                MaleCount = SurvivorEstimatedAverageMonthlyIncome_Between_1000001_500000.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = SurvivorEstimatedAverageMonthlyIncome_Between_1000001_500000.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            var SurvivorEstimatedAverageMonthlyIncome_MoreThan500000 = listCases.Where(c => c.SurvivorEstimatedAverageMonthlyIncome == MetricsKeys.SurvivorEstimatedAverageMonthlyIncome_MoreThan500000);

            byincomeBySex.Add(new CaseBySubjectBySex
            {
                Subject = MetricsKeys.SurvivorEstimatedAverageMonthlyIncome_MoreThan500000,
                MaleCount = SurvivorEstimatedAverageMonthlyIncome_MoreThan500000.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = SurvivorEstimatedAverageMonthlyIncome_MoreThan500000.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            var SurvivorEstimatedAverageMonthlyIncome_NotReported = listCases.Where(c => c.SurvivorEstimatedAverageMonthlyIncome == MetricsKeys.SurvivorEstimatedAverageMonthlyIncome_NotReported);

            byincomeBySex.Add(new CaseBySubjectBySex
            {
                Subject = MetricsKeys.SurvivorEstimatedAverageMonthlyIncome_NotReported,
                MaleCount = SurvivorEstimatedAverageMonthlyIncome_NotReported.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = SurvivorEstimatedAverageMonthlyIncome_NotReported.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            return NewMonthlyDisplayCaseCategory(label, workSheet, startRow, byincomeBySex);
        }

        private int ByEmploymentStatusBySex(ExcelWorksheet workSheet, IEnumerable<AdminDashboardProjectModel> listCases, int startRow, string label)
        {
            //emp status
            var byEmploymentStatusBySex = new List<CaseBySubjectBySex>();

            var notEmployed = listCases.Where(c => c.EmploymentStatus == MetricsKeys.EmploymentStatus_Unemployed);

            byEmploymentStatusBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Not Employed",
                MaleCount = notEmployed.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = notEmployed.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            var employed = listCases.Where(c => c.EmploymentStatus == MetricsKeys.EmploymentStatus_CurrentlyEmployed);

            byEmploymentStatusBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Employed",
                MaleCount = employed.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = employed.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            var notReported = listCases.Where(c => c.EmploymentStatus == MetricsKeys.EmploymentStatus_NotReported);

            byEmploymentStatusBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Not Reported",
                MaleCount = notReported.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = notReported.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            var other = listCases.Where(c =>
            !(c.EmploymentStatus == MetricsKeys.EmploymentStatus_Unemployed ||
            c.EmploymentStatus == MetricsKeys.EmploymentStatus_CurrentlyEmployed ||
            c.EmploymentStatus == MetricsKeys.EmploymentStatus_NotReported));

            byEmploymentStatusBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Other",
                MaleCount = other.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = other.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            return NewMonthlyDisplayCaseCategory(label, workSheet, startRow, byEmploymentStatusBySex);
        }

        private int ByEducationBySex(ExcelWorksheet workSheet, IEnumerable<AdminDashboardProjectModel> listCases, int startRow, string label)
        {
            //education
            var byEducationBySex = new List<CaseBySubjectBySex>();

            var noEdu = listCases.Where(c => c.Education == MetricsKeys.Education_NoEducation);

            byEducationBySex.Add(new CaseBySubjectBySex
            {
                Subject = MetricsKeys.Education_NoEducation,
                MaleCount = noEdu.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = noEdu.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            var quaran = listCases.Where(c => c.Education == "Qur’anic");

            byEducationBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Qur’anic",
                MaleCount = quaran.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = quaran.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            var primary = listCases.Where(c => c.Education == MetricsKeys.Education_CompletedPrimary || c.Education == MetricsKeys.Education_SomePrimarySchool);

            byEducationBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Primary Level",
                MaleCount = primary.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = primary.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            var secondary = listCases.Where(c => c.Education == MetricsKeys.Education_CompletedSecondary || c.Education == MetricsKeys.Education_SomeSecondary);

            byEducationBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Secondary Level",
                MaleCount = secondary.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = secondary.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            var tetiary = listCases.Where(c => c.Education == MetricsKeys.Education_Undergraduate || c.Education == MetricsKeys.Education_Graduate
            || c.Education == MetricsKeys.Education_Postgraduate);

            byEducationBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Tetiary",
                MaleCount = tetiary.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = tetiary.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            var others = listCases.Where(c => !(
            c.Education == MetricsKeys.Education_NoEducation
            || c.Education == "Qur’anic"
            || c.Education == MetricsKeys.Education_CompletedPrimary
            || c.Education == MetricsKeys.Education_SomePrimarySchool
            || c.Education == MetricsKeys.Education_CompletedSecondary
            || c.Education == MetricsKeys.Education_SomeSecondary
            || c.Education == MetricsKeys.Education_Undergraduate
            || c.Education == MetricsKeys.Education_Graduate
            || c.Education == MetricsKeys.Education_Postgraduate));

            byEducationBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Others",
                MaleCount = others.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = others.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            return NewMonthlyDisplayCaseCategory(label, workSheet, startRow, byEducationBySex);
        }

        private int ByWhoReportedBySex(ExcelWorksheet workSheet, IEnumerable<AdminDashboardProjectModel> listCases, int startRow, string label)
        {
            //education
            var byWhoReportedBySex = new List<CaseBySubjectBySex>();

            var self = listCases.Where(c => c.WhoReportedIncident == MetricsKeys.WhoReportedIncident_Self);

            byWhoReportedBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Self Reporting",
                MaleCount = self.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = self.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            var thirdParty = listCases.Where(c => c.WhoReportedIncident == MetricsKeys.WhoReportedIncident_Parent ||
             c.WhoReportedIncident == MetricsKeys.WhoReportedIncident_Guardian
             || c.WhoReportedIncident == MetricsKeys.WhoReportedIncident_Spouse ||
              c.WhoReportedIncident == MetricsKeys.WhoReportedIncident_Witness);

            byWhoReportedBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Third-Party Reporting",
                MaleCount = thirdParty.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = thirdParty.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            var others = listCases.Where(c => !(
            c.WhoReportedIncident == MetricsKeys.WhoReportedIncident_Self ||
            c.WhoReportedIncident == MetricsKeys.WhoReportedIncident_Parent ||
            c.WhoReportedIncident == MetricsKeys.WhoReportedIncident_Guardian
            || c.WhoReportedIncident == MetricsKeys.WhoReportedIncident_Spouse ||
             c.WhoReportedIncident == MetricsKeys.WhoReportedIncident_Witness));

            byWhoReportedBySex.Add(new CaseBySubjectBySex
            {
                Subject = "Other",
                MaleCount = others.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Male),
                FemaleCount = others.Count(c => c.SexOfSurvior.ToLower() == MetricsKeys.Female)
            });

            return NewMonthlyDisplayCaseCategory(label, workSheet, startRow, byWhoReportedBySex);
        }

        private int ByTypeOfViolece(ExcelWorksheet workSheet, IEnumerable<AdminDashboardProjectModel> listCases, int startRow, string label)
        {
            var byTypeOfViolece = new List<CaseBySubject>();

            var vioType = CasesByTypeOfViolenceBySex(listCases);

            var sexual = vioType.Where(c => c.Subject == MetricsKeys.TypeOfViolence_SexualAssault
            || c.Subject == MetricsKeys.TypeOfViolence_Rape || c.Subject == MetricsKeys.TypeOfViolence_Defilement
            || c.Subject == MetricsKeys.TypeOfViolence_FemaleGenitalMutilation || c.Subject == MetricsKeys.TypeOfViolence_ForcedMarriage);

            byTypeOfViolece.Add(new CaseBySubject
            {
                Name = "Sexual",
                Count = sexual.Sum(i => i.MaleCount + i.FemaleCount)
            });

            var physical = vioType.Where(c => c.Subject == MetricsKeys.TypeOfViolence_PhysicalAssault);

            byTypeOfViolece.Add(new CaseBySubject
            {
                Name = "Physical",
                Count = physical.Sum(i => i.MaleCount + i.FemaleCount),
            });

            var emotionalPsychological = vioType.Where(c => c.Subject == MetricsKeys.TypeOfViolence_EmotionalPsychological
                    || c.Subject == MetricsKeys.TypeOfViolence_ChildAbuseAndNeglect);

            byTypeOfViolece.Add(new CaseBySubject
            {
                Name = "Emotional/Psychological",
                Count = emotionalPsychological.Sum(i => i.MaleCount + i.FemaleCount),
            });

            var financialEconomic = vioType.Where(c => c.Subject == MetricsKeys.TypeOfViolence_DenialOfResourcesServices
            || c.Subject == MetricsKeys.TypeOfViolence_ViolationOfPropertyAndInheritanceRight);

            byTypeOfViolece.Add(new CaseBySubject
            {
                Name = "Financial/Economic",
                Count = financialEconomic.Sum(i => i.MaleCount + i.FemaleCount),
            });

            var cyber = vioType.Where(c => c.Subject == MetricsKeys.TypeOfViolence_CyberBullying);

            byTypeOfViolece.Add(new CaseBySubject
            {
                Name = "Cyber",
                Count = cyber.Sum(i => i.MaleCount + i.FemaleCount),
            });

            var other = vioType.Where(c => c.Subject.ToLower() == "other");

            byTypeOfViolece.Add(new CaseBySubject
            {
                Name = "Other",
                Count = other.Sum(i => i.MaleCount + i.FemaleCount),
            });

            return NewMonthlyDisplayCaseCategory(label, workSheet, startRow, byTypeOfViolece);
        }

        #region | Service Reports |

        public class ServiceProvidedReportDto
        {
            public string TypeOfClient { get; set; }

            public string SexOfSurvivorOrVictim { get; set; }

            public int AgeOfSurvivorInYears { get; set; }

            public string TypeOfServiceProvided { get; set; }

            public List<string> TypeOfServiceProvidedList =>
                !string.IsNullOrWhiteSpace(TypeOfServiceProvided)
                    ? JsonConvert.DeserializeObject<List<string>>(TypeOfServiceProvided)
                    : new List<string>();
        }

        private int ServiceByTypeOfClient(ExcelWorksheet workSheet, List<ServiceProvidedReportDto> listCases, int startRow)
        {
            var byType = new List<CaseBySubjectBySex>();
            //Number of GBV cases prosecuted (outcome)
            var provider = listCases.Where(c => c.TypeOfClient == MetricsKeys.TypOfClient_ProviderClient);

            byType.Add(new CaseBySubjectBySex
            {
                Subject = "Provider's client",
                MaleCount = provider.Where(c => c.SexOfSurvivorOrVictim.ToLower() == MetricsKeys.Male).Count(),
                FemaleCount = provider.Where(c => c.SexOfSurvivorOrVictim.ToLower() == MetricsKeys.Female).Count(),
            });

            var walkin = listCases.Where(c => c.TypeOfClient == MetricsKeys.TypOfClient_WalkIn);

            byType.Add(new CaseBySubjectBySex
            {
                Subject = "Walk-in",
                MaleCount = walkin.Where(c => c.SexOfSurvivorOrVictim.ToLower() == MetricsKeys.Male).Count(),
                FemaleCount = walkin.Where(c => c.SexOfSurvivorOrVictim.ToLower() == MetricsKeys.Female).Count(),
            });

            var referred = listCases.Where(c => c.TypeOfClient == MetricsKeys.TypOfClient_Referred);

            byType.Add(new CaseBySubjectBySex
            {
                Subject = "Referred",
                MaleCount = referred.Where(c => c.SexOfSurvivorOrVictim.ToLower() == MetricsKeys.Male).Count(),
                FemaleCount = referred.Where(c => c.SexOfSurvivorOrVictim.ToLower() == MetricsKeys.Female).Count(),
            });

            var disc = listCases.Where(c => string.IsNullOrWhiteSpace(c.TypeOfClient));

            byType.Add(new CaseBySubjectBySex
            {
                Subject = "Others",
                MaleCount = disc.Where(c => c.SexOfSurvivorOrVictim.ToLower() == MetricsKeys.Male).Count(),
                FemaleCount = disc.Where(c => c.SexOfSurvivorOrVictim.ToLower() == MetricsKeys.Female).Count(),
            });

            return NewMonthlyDisplayCaseCategory("Number of survivors of gender-based violence (GBV) who received post-GBV care services ", workSheet, startRow, byType);
        }

        private int ServiceRecievedByAge(ExcelWorksheet workSheet, List<ServiceProvidedReportDto> listCases, int startRow)
        {
            var ageRanges = new List<(int? Min, int? Max)>()
            {
                {(0 , 9)},
                {(10 ,14)},
                {(15, 19)},
                {(20 , 24)},
                {(25 , 29)},
                {(30 ,34)},
                {(40 , 44)},
                {(45 , 49)},
                {(50, 1000)}
            };

            var byincomeBySex = new List<CaseBySubjectBySex>();

            foreach (var range in ageRanges)
            {
                var category = listCases.Where(c => c.AgeOfSurvivorInYears >= range.Min && c.AgeOfSurvivorInYears <= range.Max);

                byincomeBySex.Add(new CaseBySubjectBySex
                {
                    Subject = range.Max >= 50 ? "50+" : $"{range.Min} - {range.Max}",
                    FemaleCount = category.Count(c => c.SexOfSurvivorOrVictim.ToLower() == MetricsKeys.Female),
                    MaleCount = category.Count(c => c.SexOfSurvivorOrVictim.ToLower() == MetricsKeys.Male),
                });
            }

            return NewMonthlyDisplayCaseCategory("Number of survivors of gender-based violence (GBV) who received post-GBV care services", workSheet, startRow, byincomeBySex);
        }

        private int NoOfServiceProvideByType(ExcelWorksheet workSheet, List<ServiceProvidedReportDto> listCases, int startRow)
        {
            var categories = new List<CaseBySubject>();

            var clinicalService = listCases.Where(c => c.TypeOfServiceProvidedList.Contains(MetricsKeys.ServiceProvided_Medical));

            categories.Add(new CaseBySubject
            {
                Name = "Clinical Care",
                Count = clinicalService.Count()
            });

            var psychosocial = listCases.Where(c => c.TypeOfServiceProvidedList.Contains(MetricsKeys.ServiceProvided_Psychosocial));

            categories.Add(new CaseBySubject
            {
                Name = "Psychosocial",
                Count = psychosocial.Count(),
            });

            var shelter = listCases.Where(c => c.TypeOfServiceProvidedList.Contains(MetricsKeys.ServiceProvided_SafeHouse));

            categories.Add(new CaseBySubject
            {
                Name = "Shelter and Safety",
                Count = shelter.Count(),
            });

            var legal = listCases.Where(c => c.TypeOfServiceProvidedList.Contains(MetricsKeys.ServiceProvided_Legal));

            categories.Add(new CaseBySubject
            {
                Name = "Legal and Protection",
                Count = shelter.Count(),
            });

            var enforcement = listCases.Where(c => c.TypeOfServiceProvidedList.Contains(MetricsKeys.ServiceProvided_PoliceSecurity));

            categories.Add(new CaseBySubject
            {
                Name = "Law Enforcement",
                Count = enforcement.Count(),
            });

            var education = listCases.Where(c => c.TypeOfServiceProvidedList.Contains(MetricsKeys.ServiceProvided_Education));

            categories.Add(new CaseBySubject
            {
                Name = "Education",
                Count = education.Count(),
            });

            var livelihood = listCases.Where(c => c.TypeOfServiceProvidedList.Contains(MetricsKeys.ServiceProvided_Livelihood));

            categories.Add(new CaseBySubject
            {
                Name = "Livelihood and Empowerment",
                Count = livelihood.Count(),
            });

            var referral = listCases.Where(c => c.TypeOfServiceProvidedList.Contains(MetricsKeys.ServiceProvided_Referral));

            categories.Add(new CaseBySubject
            {
                Name = "Referral",
                Count = referral.Count(),
            });

            var others = listCases.Where(c => c.TypeOfServiceProvidedList.Any(c => !KeyLists.ServiceProvidedList.Contains(c)));

            categories.Add(new CaseBySubject
            {
                Name = "Others",
                Count = others.Count(),
            });

            return NewMonthlyDisplayCaseCategory("Number of post-GBV service provided (type of service)", workSheet, startRow, categories);
        }

        public async Task SummarySheetServices(ExcelPackage excelPackage, DateModel model)
        {
            ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets.Add($"Summary Sheet - Service");

            var service = _context.ServicesProvided.AsNoTracking().Where(c => c.CreatedAt >= model.StartDate && c.CreatedAt <= model.EndDate); ;

            if (model.IsValidated.HasValue)
            {
                service = service.Where(s => s.Status == ValidationStatus.SpOrCso);
            }

            if (model.IsApproved.HasValue)
            {
                service = service.Where(s => s.Status == ValidationStatus.State);
            }

            if (model.StateId.HasValue)
            {
                service = service.Where(c => c.StateId == model.StateId);
            }

            var dto = await service.Select(s => new ServiceProvidedReportDto
            {
                TypeOfServiceProvided = s.TypeOfServiceProvided,
                TypeOfClient = s.TypeOfClient,
                SexOfSurvivorOrVictim = s.SexOfSurvivorOrVictim,
                AgeOfSurvivorInYears = s.AgeOfSurvivorInYears
            }).ToListAsync();

            int startRow = 3;

            startRow = NoOfServiceProvideByType(workSheet, dto, startRow);
            startRow = ServiceRecievedByAge(workSheet, dto, startRow);
            ServiceByTypeOfClient(workSheet, dto, startRow);
        }

        #endregion | Service Reports |

        /// <summary>
        /// Exports monthly report
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<MemoryStream>> ExportMonthlyReportNew(DateModel model, bool isCase)
        {
            var casesQuery = await GetCasesForMonthlyReport(model);

            if (!casesQuery.status) return new AppResult<MemoryStream>
            {
                Message = casesQuery.message,
                StatusCode = StatusCodes.Status400BadRequest,
            };

            //create excel document
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            ExcelPackage excelpackage = new ExcelPackage();

            string newFile = "";
            switch (isCase)
            {
                case false:
                    await SummarySheetServices(excelpackage, model);
                    newFile = $"Monthly-Services-Report-{DateTime.Now:yyyy-MM-dd-hh-mm}.xlsx";
                    break;
                default:

                    var cases = casesQuery.data.cases;

                    var listCases = await cases.ProjectTo<AdminDashboardProjectModel>(_mapper.ConfigurationProvider).Future().ToListAsync();

                    SummarySheetCases(excelpackage, listCases);
                    SummarySheetOutcome(excelpackage, listCases);
                    newFile = $"Monthly-Cases-Report-{DateTime.Now:yyyy-MM-dd-hh-mm}.xlsx";
                    break;
            }

            await excelpackage.SaveAsync();

            var stream = new MemoryStream(excelpackage.GetAsByteArray());

            // var newFile = $"Monthly-Report-{DateTime.Now:yyyy-MM-dd-hh-mm}.xlsx";

            return new AppResult<MemoryStream>
            {
                Data = stream,
                Message = newFile
            };
        }

        private static int NewMonthlyDisplayCaseCategory(string categoryName, ExcelWorksheet workSheet, int row, IReadOnlyCollection<CaseBySubject> categoryCases)
        {
            var catName = workSheet.Cells[$"C{row}"];

            catName.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            catName.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            catName.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            catName.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            catName.Value = categoryName;

            {
                var empty = workSheet.Cells[$"D{row}"];
                empty.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                empty.Value = "";
            }

            var currentCol = 'E';

            foreach (var cat in categoryCases)
            {
                var cell = workSheet.Cells[$"{currentCol}{row}"];
                cell.Value = cat.Name;

                cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;

                currentCol++;
            }
            {
                var total = workSheet.Cells[$"{currentCol}{row}"];
                total.Value = "Total";
                total.Style.Font.Bold = true;

                total.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                total.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                total.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                total.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                row++;
            }

            {
                var empty = workSheet.Cells[$"C{row}"];

                empty.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                empty.Value = "";
            }

            {
                var empty = workSheet.Cells[$"D{row}"];

                empty.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                empty.Value = "";
            }

            currentCol = 'E';
            foreach (var cat in categoryCases)
            {
                var cell = workSheet.Cells[$"{currentCol}{row}"];
                cell.Value = cat.Count;

                cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;

                currentCol++;
            }
            {
                var total = workSheet.Cells[$"{currentCol}{row}"];
                total.Value = categoryCases.Sum(c => c.Count);

                total.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                total.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                total.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                total.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                row++;
            }

            return ++row;
        }

        private static int NewMonthlyDisplayCaseCategory(string categoryName, ExcelWorksheet workSheet, int row, IReadOnlyCollection<CaseBySubjectBySex> categoryCases)
        {
            var catName = workSheet.Cells[$"C{row}"];

            catName.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            catName.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            catName.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            catName.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            catName.Value = categoryName;

            {
                var empty = workSheet.Cells[$"D{row}"];
                empty.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                empty.Value = "";
            }

            var currentCol = 'E';

            foreach (var cat in categoryCases)
            {
                var cell = workSheet.Cells[$"{currentCol}{row}"];
                cell.Value = cat.Subject;

                cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;

                currentCol++;
            }
            {
                var total = workSheet.Cells[$"{currentCol}{row}"];
                total.Value = "Total";
                total.Style.Font.Bold = true;

                total.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                total.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                total.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                total.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                row++;
            }

            {
                var empty = workSheet.Cells[$"C{row}"];

                empty.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                empty.Value = "";
            }

            {
                var empty = workSheet.Cells[$"D{row}"];

                empty.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                empty.Value = "F";
            }

            currentCol = 'E';
            foreach (var cat in categoryCases)
            {
                var cell = workSheet.Cells[$"{currentCol}{row}"];
                cell.Value = cat.FemaleCount;

                cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;

                currentCol++;
            }
            {
                var total = workSheet.Cells[$"{currentCol}{row}"];
                total.Value = categoryCases.Sum(c => c.FemaleCount);

                total.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                total.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                total.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                total.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                row++;
            }

            {
                var empty = workSheet.Cells[$"C{row}"];

                empty.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                empty.Value = "";
            }

            {
                var empty = workSheet.Cells[$"D{row}"];

                empty.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                empty.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                empty.Value = "M";
            }

            currentCol = 'E';
            foreach (var cat in categoryCases)
            {
                var cell = workSheet.Cells[$"{currentCol}{row}"];
                cell.Value = cat.MaleCount;

                cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;

                currentCol++;
            }
            {
                var total = workSheet.Cells[$"{currentCol}{row}"];
                total.Value = categoryCases.Sum(c => c.MaleCount);

                total.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                total.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                total.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                total.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                row++;
            }

            return ++row;
        }

        private static void DisplayRowTotals(ExcelWorksheet workSheet, int row, IReadOnlyCollection<CaseBySubjectBySex> categoryCases)
        {
            //values
            workSheet.Cells[$"C{row}"].Value = "Total";
            workSheet.Cells[$"D{row}"].Value = categoryCases.Sum(c => c.MaleCount);
            workSheet.Cells[$"E{row}"].Value = categoryCases.Sum(c => c.FemaleCount);
            workSheet.Cells[$"F{row}"].Value = categoryCases.Sum(c => c.OtherCount);
            workSheet.Cells[$"G{row}"].Value = categoryCases.Sum(c => c.MaleCount + c.FemaleCount + c.OtherCount);
            workSheet.Cells[$"H{row}"].Value = categoryCases.Sum(c => c.TotalNewCases);
            workSheet.Cells[$"I{row}"].Value = categoryCases.Sum(c => c.TotalFollowUpCases);

            //style
            workSheet.Cells[$"C{row}:I{row}"].Style.Border.Top.Style = ExcelBorderStyle.Thick;
            workSheet.Cells[$"B{row}"].Style.Border.Left.Style = ExcelBorderStyle.Thick;
            workSheet.Cells[$"C{row}"].Style.Border.Left.Style = ExcelBorderStyle.Thick;
            workSheet.Cells[$"I{row}"].Style.Border.Right.Style = ExcelBorderStyle.Thick;
        }

        private static int DisplayCaseCategory(string categoryName, ExcelWorksheet workSheet, int row, IReadOnlyCollection<CaseBySubjectBySex> categoryCases, Color colFromHex)
        {
            workSheet.Cells[$"B{row}"].Value = categoryName;
            workSheet.Cells[$"D{row}"].Value = "Male";
            workSheet.Cells[$"E{row}"].Value = "Female";
            workSheet.Cells[$"F{row}"].Value = "Other";
            workSheet.Cells[$"G{row}"].Value = "Total";
            workSheet.Cells[$"H{row}"].Value = "Total new cases";
            workSheet.Cells[$"I{row}"].Value = "Total follow-up cases";

            workSheet.Cells[$"C{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Cells[$"C{row}"].Style.Fill.BackgroundColor.SetColor(colFromHex);

            workSheet.Cells[$"C{row}:I{row}"].Style.Font.Bold = true;

            workSheet.Cells[$"B{row}"].Style.Border.Top.Style = ExcelBorderStyle.Thick;
            workSheet.Cells[$"B{row}"].Style.Border.Left.Style = ExcelBorderStyle.Thick;
            workSheet.Cells[$"C{row}:I{row}"].Style.Border.Top.Style = ExcelBorderStyle.Thick;
            workSheet.Cells[$"C{row}:E{row}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[$"I{row}"].Style.Border.Right.Style = ExcelBorderStyle.Thick;
            workSheet.Cells[$"C{row}"].Style.Border.Left.Style = ExcelBorderStyle.Thick;

            var merge = row++;
            foreach (var cat in categoryCases)//.OrderByDescending(c => c.TotalCount))
            {
                workSheet.Cells[$"C{row}"].Value = cat.Subject;
                workSheet.Cells[$"D{row}"].Value = cat.MaleCount;
                workSheet.Cells[$"E{row}"].Value = cat.FemaleCount;
                workSheet.Cells[$"F{row}"].Value = cat.OtherCount;
                workSheet.Cells[$"G{row}"].Value = cat.MaleCount + cat.FemaleCount + cat.OtherCount;
                workSheet.Cells[$"H{row}"].Value = cat.TotalNewCases;
                workSheet.Cells[$"I{row}"].Value = cat.TotalFollowUpCases;

                workSheet.Cells[$"B{row}"].Style.Border.Left.Style = ExcelBorderStyle.Thick;
                workSheet.Cells[$"C{row}"].Style.Border.Left.Style = ExcelBorderStyle.Thick;
                workSheet.Cells[$"I{row}"].Style.Border.Right.Style = ExcelBorderStyle.Thick;
                workSheet.Cells[$"C{row}:I{row}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[$"C{row}:E{row}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                row++;
            }

            DisplayRowTotals(workSheet, row, categoryCases);
            row++;

            workSheet.Cells[$"B{merge}:B{row - 1}"].Merge = true;

            return row;
        }

        private async Task<List<CaseBySubject>> GroupCaseByType((List<(QueryFutureValue<int> id, string name, QueryFutureValue<int> count)> queriesResults, QueryFutureValue<int> othersCount) queries)
        {
            var caseByType = new List<CaseBySubject>();

            foreach (var query in queries.queriesResults)
            {
                var count = query.count.Value;

                if (count != 0)
                {
                    caseByType.Add(new CaseBySubject
                    {
                        Id = query.id,
                        Name = query.name,
                        Count = count
                    });
                }
                else
                {
                    var cat = await _context.CaseCategories.FirstOrDefaultAsync(c =>
                        c.Name.Trim().ToLower().Contains(query.name.Trim().ToLower()));
                    caseByType.Add(new CaseBySubject
                    {
                        Id = cat?.Id ?? 0,
                        Name = query.name,
                        Count = 0
                    });
                }
            }

            var others = new CaseBySubject()
            {
                Id = 0,
                Count = queries.othersCount.Value,// &&   !KeyLists.TypeOfViolence.Any(t => c.Category.Name.Trim().ToLower().Contains(t.Trim().ToLower()))),
                Name = "Others"
            };

            caseByType.Add(others);

            return caseByType.OrderBy(c => c.Count).ToList();
        }

        private async Task<List<CaseBySubject>> GroupCaseByType(IEnumerable<AdminDashboardProjectModel> cases, bool? hasClientReceivedJustice = null)
        {
            if (hasClientReceivedJustice.HasValue)
            {
                var yesOrNo = hasClientReceivedJustice.Value ? YesOrNo.Yes : YesOrNo.No;
                cases = cases.Where(c => c.FollowUpActions.Any(a => a.HasClientReceivedjustice == yesOrNo));
            }

            //case by Incident type

            var caseCategoryList = Enum.GetValues(typeof(CaseCategoryOrTypeOfViolence));
            var caseByType = new List<CaseBySubject>();

            foreach (var type in caseCategoryList)
            {
                var catInfo = cases.Where(c => c.CaseCategoriesList.Contains((int)type));

                var count = catInfo.Count();

                var id = (int)(CaseCategoryOrTypeOfViolence)Enum.Parse(typeof(CaseCategoryOrTypeOfViolence), type.ToString());

                if (count != 0)
                {
                    caseByType.Add(new CaseBySubject
                    {
                        Id = id,
                        Name = type.ToString(),
                        Count = count
                    });
                }
                else
                {
                    caseByType.Add(new CaseBySubject
                    {
                        Id = id,
                        Name = type.ToString(),
                        Count = 0
                    });
                }
            }

            var countOthers = cases.Count(c => !c.CaseCategoriesList.Any() && !string.IsNullOrWhiteSpace(c.CaseCategoriesOthers));

            var others = new CaseBySubject()
            {
                Id = 0,
                Count = countOthers,// &&   !KeyLists.TypeOfViolence.Any(t => c.Category.Name.Trim().ToLower().Contains(t.Trim().ToLower()))),
                Name = "Others"
            };

            caseByType.Add(others);

            return caseByType.OrderBy(c => c.Count).ToList();
        }




    }
}