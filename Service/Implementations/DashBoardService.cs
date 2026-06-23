using Coravel.Queuing.Interfaces;
using Core.Data;
using Core.Entities;
using Core.Enums;
using CSharpFunctionalExtensions;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Service.AppServices;
using Service.Enums;
using Service.Extensions;
using Service.Helpers;
using Service.Interfaces;
using Service.Invocables;
using Service.Models;
using Service.Models.ViewModels;
using Service.StringKeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implementations
{
    public class DashBoardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        private readonly HttpContext _httpContext;

        private readonly IAppCachingService _appCachingService;
        private readonly IQueue _queue;

        private readonly IConfiguration _configuration;

        //check if the user is authenicated
        private string UserId => _httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        public string IpAddress => _httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        public DashBoardService(ApplicationDbContext context, IHttpContextAccessor accessor, IAppCachingService appCachingService, IQueue queue, IConfiguration configuration)
        {
            _context = context;
            _httpContext = accessor.HttpContext;
            _appCachingService = appCachingService;
            _queue = queue;
            _configuration = configuration;
        }


        public async Task<AppResult<StateHomeVM>> ByState(DashboardDetails model, SearchModel searchModel)
        {
            var result = new AppResult<StateHomeVM> { Data = new StateHomeVM() };
            //string key = string.Empty;
            //if (string.IsNullOrEmpty(UserId))
            //    key = _appCachingService.ComposeMetricsCacheKey(searchModel, CacheKey.ComposeDashBoardCacheKey(DashboardKeys.HomePageMetrics), model);
            //else
            //    key = _appCachingService.ComposeMetricsCacheKey(searchModel, UserId, model);
            switch (model)
            {
                case DashboardDetails.Incident:
                    //var getCaseFromCache = await _appCachingService.GetMetrics<StateHomeVM>(key);
                    //if (getCaseFromCache.status)
                    //{
                    //    result.Data = getCaseFromCache.data;
                    //    result.Message = "Successful";
                    //    result.StatusCode = StatusCodes.Status200OK;
                    //    return result;
                    //}
                    var allCases = GetAllCasesNoInclude(searchModel);
                    var casesByState = await allCases
                        .GroupBy(x => x.IncidentState.Name)
                        .Select(x => new StateDashVM
                        {
                            Name = x.Key,
                            TotalNumber = x.Count()
                        }).ToListAsync();
                    result.Data.StateDashVMs = casesByState;
                    break;
                case DashboardDetails.Services:

                    //var getFromCache = await _appCachingService.GetMetrics<StateHomeVM>(key);
                    //if (getFromCache.status)
                    //{
                    //    result.Data = getFromCache.data;
                    //    result.Message = "Successful";
                    //    result.StatusCode = StatusCodes.Status200OK;
                    //    return result;
                    //}
                    var allServices = GetAllServicesNoInclude(searchModel);
                    var servicesByState = await allServices
                        .GroupBy(x => x.State.Name)
                        .Select(c => new StateDashVM
                        {
                            Name = c.Key,
                            TotalNumber = c.Count()
                        }).ToListAsync();
                    result.Data.StateDashVMs = servicesByState;

                    break;
                default:
                    break;
            }
            //_queue.QueueAsyncTask(async () =>
            //{
            //    await _appCachingService.AddMetrics(key, result.Data);
            //});
            result.Message = "Successful";
            result.StatusCode = StatusCodes.Status200OK;

            return result;
        }

        public AppResult<string> ClearDashboardCache()
        {
            _queue.QueueInvocable<ClearDashboardCacheInvocable>();
            return new AppResult<string>();
        }
        public async Task<AppResult<List<CaseBySubject>>> IncidentCasesByStateByLga(StateAndLgaFilter model)
        {
            var result = new AppResult<List<CaseBySubject>>();
            var allCases = GetAllCases();

            if (model.State != null)
            {

                allCases = allCases.Where(x => x.IncidentState.Name.ToLower() == model.State.ToLower());
                if (model.Lga != null)
                {
                    allCases = allCases.Where(x => x.IncidentState.Name.ToLower() == model.State.ToLower() && x.IncidentLGA.Name.ToLower() == model.Lga.ToLower());
                }
            }

            var cases = await allCases.Select(c => c.HasCaseBeenClosed).ToListAsync();
            var list = cases.Where(c => c != YesOrNo.NotApplicable).GroupBy(c => c)
                           .Select(p => new CaseBySubject
                           {
                               Id = (int)p.Key,
                               Name = p.FirstOrDefault() == YesOrNo.Yes ? "Closed Cases" : "Open Cases",
                               Count = p.Count()
                           }).ToList();

            result.Data = list;
            if (result.Data.Count == 0)
            {
                result.AddError("No record");
                result.Data = null;
            }
            result.Message = "Successful";
            result.StatusCode = StatusCodes.Status200OK;

            return result;
        }

        public async Task<AppResult<StateHomeVM>> IncidentCasesByStateByLgaByWard(StateAndLgaFilter model)
        {
            var result = new AppResult<StateHomeVM>();
            var allCases = GetAllCases();

            if (model.State != null)
            {

                allCases = allCases.Where(x => x.IncidentState.Name.ToLower() == model.State.ToLower());
                if (model.Lga != null)
                {
                    allCases = allCases.Where(x => x.IncidentState.Name.ToLower() == model.State.ToLower() && x.IncidentLGA.Name.ToLower() == model.Lga.ToLower());
                }
                if (model.Ward != null)
                {
                    allCases = allCases.Where(x => x.IncidentState.Name.ToLower() == model.State.ToLower() && x.IncidentLGA.Name.ToLower() == model.Lga.ToLower() && x.IncidentWard.Name.ToLower() == model.Ward.ToLower());
                }
            }
            var today = DateTime.Now;
            var cases = await allCases.Where(c => c.CreatedAt.Year == today.Year).ToListAsync();
            var list = new StateHomeVM
            {
                //TotalNumber = cases.Count
            };

            result.Data = list;
            result.Message = "Successful";
            result.StatusCode = StatusCodes.Status200OK;

            return result;
        }

        //public async Task<AppResult<List<StateHomeVM>>> ServicesByState()
        //{
        //    var result = new AppResult<List<StateHomeVM>>();
        //    var allCases = await GetAllServicesNoInclude().ToListAsync();
        //    var casesByState = allCases.GroupBy(x => x.State.Name).Select(x => new StateHomeVM
        //    {
        //        Name = x.Key,
        //        TotalNumber = x.Select(x => x.State).Count()
        //    }).ToList();

        //    result.Data = casesByState;
        //    result.Message = "Successful";
        //    result.StatusCode = StatusCodes.Status200OK;

        //    return result;
        //}
        public async Task<AppResult<StateHomeVM>> ServicesByStateByLga(StateAndLgaFilter model)
        {
            var result = new AppResult<StateHomeVM>();
            var allCases = GetAllServices();
            //int count = 0;
            //var lga = await _context.LocalGovernmentAreas.FirstOrDefaultAsync(x => x.Name.ToLower() == model.Lga);
            if (model.State != null)
            {

                allCases = allCases.Where(x => x.State.Name.ToLower() == model.State.ToLower());
                if (model.Lga != null)
                {

                    var lga = await _context.LocalGovernmentAreas.FirstOrDefaultAsync(x => x.Name.ToLower() == model.Lga);
                    allCases = allCases.Where(x => x.State.Name.ToLower() == model.State.ToLower() && x.OrganisationLgaId == lga.Id);

                    //allCases = allCases.Where(x => x.State.LocalGovernmentAreas.All(x => x.Id == lga.Id));
                    //foreach (var item in allCases)
                    //{
                    //    foreach (var lg in item.State.LocalGovernmentAreas)
                    //    {
                    //        if (lg.Id == lga.Id)
                    //            count++;
                    //    }
                    //}
                    //allCases = allCases.Where(x => x.State.Id == lga.StateId);
                }
            }

            var cases = new StateHomeVM()
            {
                //TotalNumber = allCases.Count()
            };


            result.Data = cases;
            result.Message = "Successful";
            result.StatusCode = StatusCodes.Status200OK;

            return result;
        }

        public async Task<AppResult<StateHomeVM>> ServicesByStateByLgaByWard(StateAndLgaFilter model)
        {
            var result = new AppResult<StateHomeVM>();
            var allCases = GetAllServices();

            if (model.State != null)
            {

                allCases = allCases.Where(x => x.State.Name.ToLower() == model.State.ToLower());
                var lga = await _context.LocalGovernmentAreas.FirstOrDefaultAsync(x => x.Name.ToLower() == model.Lga);
                if (model.Lga != null)
                {
                    allCases = allCases.Where(x => x.State.Name.ToLower() == model.State.ToLower() && x.OrganisationLgaId == lga.Id);
                }
                //if (model.Ward != null)
                //{
                //    var ward = await _context.Wards.FirstOrDefaultAsync(x => x.Name.ToLower() == model.Ward);
                //    allCases = allCases.Where(x => x.State.Name.ToLower() == model.State.ToLower() && x.OrganisationLgaId == lga.Id);
                //}
            }
            var today = DateTime.Now;
            var cases = await allCases.Where(c => c.CreatedAt.Year == today.Year).ToListAsync();
            var list = new StateHomeVM
            {
                //TotalNumber = cases.Count
            };

            result.Data = list;
            result.Message = "Successful";
            result.StatusCode = StatusCodes.Status200OK;

            return result;
        }

        public async Task<AppResult<List<CSOProviders>>> CsoProvidersByState()
        {
            var allOrganisations = await GetAllOrganisation().Select(c => new { States = c.States }).ToListAsync();

            var states = await GetAllStates().ToListAsync();

            var result = new List<CSOProviders>();


            foreach (var state in states)
            {
                var orgsInState = allOrganisations.Where(o => o.States.Contains($"{state.Id},") || o.States.Contains($",{state.Id}"));

                result.Add(new CSOProviders
                {
                    State = state.Name,
                    CSOsProviders = orgsInState.Count()
                });
            }
            return new AppResult<List<CSOProviders>>
            {
                Data = result,
                Message = "Successful",
                StatusCode = StatusCodes.Status200OK
            };
        }
        public async Task<AppResult<TotalNumberofCSOSP>> CsoProvidersByStateByLga(StateAndLgaFilter model)
        {
            var result = new AppResult<TotalNumberofCSOSP>();
            var allOrganisations = await GetAllOrganisation().Select(c => new { States = c.States }).ToListAsync();

            var states = await GetAllStates().ToListAsync();
            int count = 0;
            if (model.State != null)
            {
                var stateToFilter = states.FirstOrDefault(x => x.Name.ToLower() == model.State.ToLower());
                var org = allOrganisations.Where(o => o.States.Contains($"{stateToFilter.Id},") || o.States.Contains($",{stateToFilter.Id}"));
                result.Data = new TotalNumberofCSOSP
                {
                    Total = org.Count()
                };
                result.Message = "Successful";
                result.StatusCode = StatusCodes.Status200OK;

                return result;

            }

            foreach (var state in states)
            {
                var orgsInState = allOrganisations.Where(o => o.States.Contains($"{state.Id},") || o.States.Contains($",{state.Id}"));
                if (orgsInState.Any())
                    count++;
            }

            result.Data = new TotalNumberofCSOSP
            {
                Total = count
            };
            result.Message = "Successful";
            result.StatusCode = StatusCodes.Status200OK;
            //int count = 0;
            //var lga = await _context.LocalGovernmentAreas.FirstOrDefaultAsync(x => x.Name.ToLower() == model.Lga);
            //if (model.State != null)
            //{
            //    allUsers = allUsers.Where(x => x.State.Name.ToLower() == model.State.ToLower());
            //    if (model.Lga != null)
            //    {
            //        //var lga = await _context.LocalGovernmentAreas.FirstOrDefaultAsync(x => x.Name.ToLower() == model.Lga);
            //        allUsers = allUsers.Where(x => x.LocalGovernmentArea.Name.ToLower() == model.Lga.ToLower());
            //        //allCases = allCases.Where(x => x.State.Id == lga.StateId);
            //    }
            //}
            //var users = await allUsers.ToListAsync();
            //var usersByState = users.Where(x => x.Type == RoleKeys.CSO || x.Type == RoleKeys.ServiceProvider).ToList();
            ////var cases = usersByState.Select(x => new CSOSPVM
            ////{
            ////     CSO=x.Where(x=>x.Type== RoleKeys.CSO).Select(x=>new CSOVM
            ////     {
            ////          Name=x.FirstName
            ////     }).ToList(), 
            ////      SP= x.Where(x => x.Type == RoleKeys.ServiceProvider).Select(x => new SPVM
            ////      {
            ////          Name = x.FirstName
            ////      }).ToList(),
            ////      LocalGovernment=x.Key.LocalGovernmentArea.Name
            ////}).ToList();

            //var totalUsers = new TotalNumberofCSOSP
            //{
            //    Total = usersByState.Count
            //}; 


            // result.Data = totalUsers;
            //result.Message = "Successful";
            //result.StatusCode = StatusCodes.Status200OK;

            return result;
        }

        public async Task<AppResult<PaginatedList<CSOSPVM>>> CsoProvidersByStateByLgaByWard(StateAndLgaFilter model)
        {
            var result = new AppResult<PaginatedList<CSOSPVM>>();
            var allOrganisations = GetAllOrganisation();
            //var allOrganisations = await GetAllOrganisation().Select(c => new { States = c.States }).ToListAsync();

            var states = await GetAllStates().ToListAsync();

            //var result = new List<CSOProviders>();

            if (model.State == null)
            {
                var data = allOrganisations.Select(x => new CSOSPVM
                {
                    Name = x.Name,
                    Address = x.Address ?? "",
                    PhoneNumber = x.PhoneNumber ?? "",
                    Type = x.OrganisationType.ToString()
                });

                result.Data = await data.PageAsync(model.PageIndex, model.PageSize);

                result.Message = "Successful";
                result.StatusCode = StatusCodes.Status200OK;
                return result;

            }

            var stateToFilter = states.Find(x => x.Name.ToLower() == model.State.ToLower());
            var org = allOrganisations.Where(o => o.States.Contains($"{stateToFilter.Id},") || o.States.Contains($",{stateToFilter.Id}"));
            var dataByState = org.Select(x => new CSOSPVM
            {
                Name = x.Name,
                Address = x.Address ?? "",
                PhoneNumber = x.PhoneNumber ?? "",
                Type = x.OrganisationType.ToString()
            });
            #region For Users
            //var allUsers = GetAllUsers();
            ////int count = 0;
            ////var lga = await _context.LocalGovernmentAreas.FirstOrDefaultAsync(x => x.Name.ToLower() == model.Lga);
            //if (model.State != null)
            //{
            //    allUsers = allUsers.Where(x => x.State.Name.ToLower() == model.State.ToLower());
            //    if (model.Lga != null)
            //    {
            //        //var lga = await _context.LocalGovernmentAreas.FirstOrDefaultAsync(x => x.Name.ToLower() == model.Lga);
            //        allUsers = allUsers.Where(x => x.LocalGovernmentArea.Name.ToLower() == model.Lga.ToLower());
            //        //allCases = allCases.Where(x => x.State.Id == lga.StateId);
            //    }
            //    //if(model.Ward !=null)
            //    //{
            //    //    allUsers = allUsers.Where(x => x.LocalGovernmentArea.Name.ToLower() == model.Lga.ToLower());
            //    //}
            //}
            ////var users = await allUsers.ToListAsync();
            //var users = allUsers.Where(x => x.Type == RoleKeys.CSO || x.Type == RoleKeys.ServiceProvider);
            //var data = users.Select(x => new CSOSPVM
            //{
            //    Name = x.FullName,
            //    Address = x.Organisation.Address,
            //    PhoneNumber = x.PhoneNumber,
            //    Type = x.Type.ToString()
            //});
            #endregion

            result.Data = await dataByState.PageAsync(model.PageIndex, model.PageSize); ;
            result.Message = "Successful";
            result.StatusCode = StatusCodes.Status200OK;

            return result;
        }

        public async Task<AppResult<ServicesDashboard>> GetServicesDashboard(SearchModel model)
        {
            //string key = string.Empty;
            //if (string.IsNullOrEmpty(UserId))
            //    key = _appCachingService.ComposeMetricsCacheKey(model, CacheKey.ComposeDashBoardCacheKey(DashboardKeys.HomePageMetrics), DashboardDetails.Services);
            //else
            //    key = _appCachingService.ComposeMetricsCacheKey(model, UserId, DashboardDetails.Services);
            //var getFromCache = await _appCachingService.GetMetrics<ServicesDashboard>(key);
            //if (getFromCache.status)
            //{
            //    return new AppResult<ServicesDashboard>()
            //    {
            //        Data = getFromCache.data,
            //        Message = "Successful",
            //        StatusCode = StatusCodes.Status200OK
            //    };
            //}
            var query = FilterServices(model);
            var (serviceBySubject, serviceBySex) = await GetServicesByTypeOfServiceAndSex(query);
            var result = new AppResult<ServicesDashboard>()
            {
                Data = new ServicesDashboard { ServicesBySex = serviceBySex, ServicesBySubject = serviceBySubject },
                Message = "Successful",
                StatusCode = StatusCodes.Status200OK
            };
            //_queue.QueueAsyncTask(async () =>
            //{
            //    await _appCachingService.AddMetrics(key, result.Data);
            //});
            return result;

        }
        private static async Task<(List<CaseBySubject>, List<CaseBySubjectBySex>)> GetServicesByTypeOfServiceAndSex(IQueryable<ServiceProvided> query)
        {
            //List of service provider options from string keys
            List<string> typeOfServiceList = new()
            {
                MetricsKeys.ServiceProvided_Referral,
                MetricsKeys.ServiceProvided_Education,
                MetricsKeys.ServiceProvided_Psychosocial,
                MetricsKeys.ServiceProvided_Medical,
                MetricsKeys.ServiceProvided_SafeHouse,
                MetricsKeys.ServiceProvided_Livelihood,
                MetricsKeys.ServiceProvided_Legal,
                MetricsKeys.ServiceProvided_PoliceSecurity
            };
            var typeOfService = query.Where(c => !string.IsNullOrWhiteSpace(c.TypeOfServiceProvided));
            var typeOfSex = query.Where(c => !string.IsNullOrWhiteSpace(c.TypeOfServiceReferredFor));
            var serviceBySex = new List<(string, string)>();
            var serviceBySubject = new List<string>();
            //loops through the service types to pull the required data if its contained
            foreach (var serviceObject in typeOfService)
            {
                //filters the list replaces those not listed with "Other"
                foreach (var service in serviceObject.TypeOfServiceProvidedList)
                {
                    var value = typeOfServiceList.Exists(c => c.Trim().Equals(service, StringComparison.OrdinalIgnoreCase));

                    if (!value)
                    {
                        //replace with Other if not found in list
                        serviceBySubject.Add("Other");
                    }
                    else
                    {
                        var data = service;
                        if (serviceBySubject.Any())
                        {
                            var exist = serviceBySubject.Find(x => x.Trim().Equals(data.Trim(), StringComparison.OrdinalIgnoreCase));
                            if (!string.IsNullOrEmpty(exist))
                            {
                                data = exist;
                            }
                        }
                        serviceBySubject.Add(data);
                    }
                }

            }

            foreach (var serviceObject in typeOfSex)
            {
                //filters the list replaces those not listed with "Other"
                foreach (var service in serviceObject.TypeOfServiceReferredForList)
                {
                    var value = typeOfServiceList.Exists(c => c.Trim().Equals(service.Trim(), StringComparison.OrdinalIgnoreCase));

                    if (!value)
                    {
                        //replace with Other if not found in list
                        serviceBySex.Add(("Other", serviceObject.SexOfSurvivorOrVictim));
                    }
                    else
                    {
                        var data = service;
                        if (serviceBySubject.Any())
                        {
                            var exist = serviceBySubject.Find(x => x.Trim().Equals(data.Trim(), StringComparison.OrdinalIgnoreCase));
                            if (!string.IsNullOrEmpty(exist))
                            {
                                data = exist;
                            }
                        }
                        serviceBySex.Add((data, serviceObject.SexOfSurvivorOrVictim));
                    }
                }

            }

            var group = serviceBySex.GroupBy(c => c.Item1).ToList();

            //groups the services by common string
            var servicesByTypeofService = serviceBySubject
                .GroupBy(x => x)
                .Select(p => new CaseBySubject
                {
                    Id = 0,
                    Name = p.Key,
                    Count = p.Count()
                }).ToList();

            //groups the services by sex here item2 is Sex , item1 is typeofService
            var serviceByTypeofServiceBySex = group.Select(p => new CaseBySubjectBySex
            {
                Subject = p.Key,
                FemaleCount = p.Count(c => c.Item2.ToLower() == MetricsKeys.Female),
                MaleCount = p.Count(c => c.Item2.ToLower() == MetricsKeys.Male),
                OtherCount = p.Count(c =>
                    c.Item2.ToLower() != MetricsKeys.Male && c.Item2.ToLower() != MetricsKeys.Female)
            }).ToList();

            //app owner doesn't want rows the zero data to be excluded so empty rows are added for categries with zero data
            // add empty rows
            foreach (var item in typeOfServiceList)
            {
                if (!serviceByTypeofServiceBySex.Exists(c => c.Subject.ToLower() == item.ToLower()))
                {
                    serviceByTypeofServiceBySex.Add(new CaseBySubjectBySex
                    {
                        Subject = item
                    });
                }
            }

            //add empty other row
            if (!serviceByTypeofServiceBySex.Exists(c => c.Subject.ToLower() == "other"))
            {
                serviceByTypeofServiceBySex.Add(new CaseBySubjectBySex
                {
                    Subject = "Other"
                });
            }

            //add empty rows
            foreach (var item in typeOfServiceList)
            {
                if (!servicesByTypeofService.Exists(c => c.Name.ToLower() == item.ToLower()))
                {
                    serviceByTypeofServiceBySex.Add(new CaseBySubjectBySex
                    {
                        Subject = item
                    });
                }
            }

            return (servicesByTypeofService, serviceByTypeofServiceBySex);
        }



        private IQueryable<Case> GetAllCases() => _context.Cases.AsNoTracking().AsQueryable().Include(x => x.IncidentState)
                                                              .Include(x => x.IncidentLGA).Include(x => x.IncidentWard);
        private IQueryable<Case> GetAllCasesNoInclude(SearchModel model)
        {
            var query = _context.Cases.AsNoTracking();
            if (model.StartDate.HasValue && model.EndDate.HasValue)
            {
                query = query.Where(c => c.DateReported >= model.StartDate.Value.Date && c.DateReported <= model.EndDate.Value);
            }
            return query;
        }
        private IQueryable<ServiceProvided> GetAllServices() => _context.ServicesProvided.Include(x => x.State)
                                                              .ThenInclude(x => x.LocalGovernmentAreas).ThenInclude(x => x.Wards).AsNoTracking().AsSplitQuery();
        private IQueryable<ServiceProvided> GetAllServicesNoInclude(SearchModel model)
        {
            var query = _context.ServicesProvided.AsNoTracking();
            if (model.StartDate.HasValue && model.EndDate.HasValue)
            {
                query = query.Where(c => c.DateOfServiceProvision >= model.StartDate.Value.Date && c.DateOfServiceProvision <= model.EndDate.Value);
            }
            return query;
        }
        private IQueryable<Organisation> GetAllOrganisation() => _context.Organisations.AsNoTracking().AsQueryable();
        private IQueryable<State> GetAllStates() => _context.States.AsNoTracking();

        private IQueryable<ApplicationUser> GetAllUsers() => _context.Users.AsNoTracking().Include(x => x.State).Include(x => x.LocalGovernmentArea).Include(x => x.ApprovedCases).Include(x => x.Organisation).AsQueryable();
        private IQueryable<ServiceProvided> FilterServices(SearchModel model)
        {
            var query = _context.ServicesProvided.AsNoTracking();
            if (model.StartDate.HasValue && model.EndDate.HasValue)
            {
                query = query.Where(c => c.DateOfServiceProvision >= model.StartDate.Value.Date && c.DateOfServiceProvision <= model.EndDate.Value);
            }
            if (model.States is not null)
            {
                query = query.Where(c => model.States.Contains(c.StateId));
            }

            if (model.Lgas is not null)
            {
                query = query.Where(c => model.Lgas.Contains(c.OrganisationLgaId.Value));
            }
            if (model.OrganisationType.HasValue)
            {
                query = query.Where(c => c.Organisation.OrganisationType == model.OrganisationType);
            }

            return query.AsNoTracking();
        }
    }
}
