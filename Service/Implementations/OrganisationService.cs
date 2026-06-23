using Core.Data;
using Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OfficeOpenXml;
using Service.Extensions;
using Service.Helpers;
using Service.Interfaces;
using Service.Models;
using Service.StringKeys;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Service.Implementations
{
    /// <summary>
    /// handles all actions for organisations
    /// </summary>
    public partial class OrganisationService : IOrganisationService
    {
        private readonly ApplicationDbContext _context;

        private DateTime CurrentDate => DateTime.Now.ToUniversalTime().AddHours(1);
        private IMemoryCache _cache;

        public OrganisationService(ApplicationDbContext context, IMemoryCache memory)
        {
            _context = context;
            _cache = memory;
        }

        /// <summary>
        /// validates and creates an organisation
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> Create(OrganisationCreationModel model)
        {
            var wrongstates = model.States.Where(s => s.Id > 37 || s.Id < 1).ToList();

            if (wrongstates.Count > 0)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = { $"There is no state with Id: {wrongstates.FirstOrDefault()}" },
                    Message = $"Creation Failed, There is no state with Id: {wrongstates.FirstOrDefault()}"
                };
            }

            var organisation = await _context.Organisations.AsNoTracking()
                                   .FirstOrDefaultAsync(c => c.Name.Trim().ToLower() == model.Name.Trim().ToLower());

            if (organisation != null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = { $"Organisation with name {model.Name} already exist" },
                    Message = $"Creation Failed, Organisation with name {model.Name} already exist"
                };
            }

            var emailExist = await _context.Organisations.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email.ToLower().Equals(model.Email.ToLower()));

            if (emailExist is not null)
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = { $"Organisation with email {model.Email} already exist" },
                    Message = $"Creation Failed, Organisation with email {model.Email} already exist"
                };
            //all operating states ids
            var stateList = JsonConvert.SerializeObject(model.States.Distinct(new StateComparer()));

            //ensure Code is Unique
            string code = "";
            while (true)
            {
                code = new Random().Next(1, 10000).ToString("0000");

                organisation = await _context.Organisations.IgnoreQueryFilters()
                              .AsNoTracking().FirstOrDefaultAsync(c => c.Code == code);

                if (organisation == null)
                {
                    break;
                }
            }

            organisation = new Organisation
            {
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Name = model.Name,
                Address = model.Address,
                States = stateList,
                OrganisationType = model.OrganisationType,
                Code = code,
                HotLine = model.HotLine,
                Acronym = model.Acronym,
                TypeOfService = model.TypeOfService == null ? "[]" : JsonConvert.SerializeObject(model.TypeOfService),
                Website = model.Website,
                SocialMediaData = model.SocialMediaData == null ? "[]" :
                    JsonConvert.SerializeObject(model.SocialMediaData.Where(i => i.Name != string.Empty))
            };

            _context.Add(organisation);

            // add log service.
            await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                StatusCode = StatusCodes.Status201Created,
                Data = organisation.Id.ToString(),
                Message = "Organisation created successfully"
            };
        }

        /// <summary>
        /// update organisation
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> Update(int Id, OrganisationCreationModel model)
        {
            var wrongstates = model.States.Where(s => s.Id > 37 || s.Id < 1).ToList();

            if (wrongstates.Count > 0)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = { $"Organisation with name {model.Name} already exist" },
                    Message = $"Update Failed Organisation with name {model.Name} already exist"
                };
            }

            var organisation = await _context.Organisations.FindAsync(Id);

            if (organisation is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Organisation not found, Update was not completed"
                };
            }
            //all operating states ids

            var stateList = JsonConvert.SerializeObject(model.States.Distinct(new StateComparer()));

            organisation.Address = model.Address;
            organisation.Name = model.Name;
            organisation.States = stateList;
            organisation.OrganisationType = model.OrganisationType;
            organisation.PhoneNumber = model.PhoneNumber;
            organisation.Email = model.Email;
            organisation.ModifiedAt = CurrentDate;
            organisation.HotLine = model.HotLine;
            organisation.Acronym = model.Acronym;
            organisation.TypeOfService = model.TypeOfService == null ? "[]" : JsonConvert.SerializeObject(model.TypeOfService);
            organisation.Website = model.Website;
            organisation.SocialMediaData = model.SocialMediaData == null
                ? "[]"
                : JsonConvert.SerializeObject(model.SocialMediaData.Where(i => i.Name != string.Empty));

            _context.Update(organisation);

            //log this action.

            await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                Data = organisation.Id.ToString(),
                StatusCode = StatusCodes.Status200OK,
                Message = "Updated Successful",
            };
        }

        /// <summary>
        /// get an organisation
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<AppResult<OrganisationViewModel>> Get(int Id)
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

        public async Task<AppResult<OrganisationViewModel>> GetWithUsers(int id)
        {
            var organisation = await Get(id);

            if (organisation.StatusCode != StatusCodes.Status200OK)
                return new AppResult<OrganisationViewModel>
                {
                    StatusCode = organisation.StatusCode,
                    Message = organisation.Message
                };

            var users = _context.Users.Where(u => u.OrganisationId == id);

            var userModels = users.AsNoTracking().Select(c => new UserViewModel
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
                IsActivated = c.IsActivated,
                LocalGovernments = c.LocalGovernments ?? new List<LgaModel>(),
            }).OrderBy(c => c.Organisation)
                .ThenBy(c => c.FirstName);

            organisation.Data.Users = userModels;

            return new AppResult<OrganisationViewModel>
            {
                Data = organisation.Data,
                StatusCode = StatusCodes.Status200OK,
                Message = "Organisation found"
            };
        }

        /// <summary>
        /// Get organsiations
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<PaginatedList<OrganisationViewModel>>> GetAll(OrganisationSearchModel model)
        {
            var query = _context.Organisations
                .OrderByDescending(c => c.CreatedAt).AsNoTracking();

            //apply filters
            if (model.StartDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= model.StartDate);
            }
            if (model.EndDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt <= model.EndDate);
            }
            if (model.OrganisationType.HasValue)
            {
                query = query.Where(c => c.OrganisationType == model.OrganisationType);
            }

            if (!string.IsNullOrWhiteSpace(model.Code))
            {
                query = query.Where(c => c.Code == model.Code);
            }

            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                query = query.Where(c => c.Name.ToLower().StartsWith(model.Name.ToLower()));
            }

            if (model.StateId.HasValue)
            {
                query = query.Where(c => c.States.Contains($":{model.StateId},"));
            }

            var OrganisationQuery = query.OrderBy(c => c.Name)
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
                    });

            var organisations = await OrganisationQuery.PageAsync(model.PageIndex, model.PageSize);

            return new AppResult<PaginatedList<OrganisationViewModel>>
            {
                Data = organisations,
                Message = "Successful",
                StatusCode = StatusCodes.Status200OK,
            };
        }

        /// <summary>
        /// get organisation list this is a thinner list details of all organistations
        /// </summary>
        /// <returns></returns>
        public async Task<AppResult<List<OrganisationListModel>>> GetList()
        {
            var organisations = await _context.Organisations.AsNoTracking()
                             .Select(c => new OrganisationListModel
                             {
                                 Id = c.Id,
                                 Name = c.Name,
                                 OrganisationType = c.OrganisationType,
                                 States = string.IsNullOrWhiteSpace(c.States) ? new List<StateList>() : JsonConvert.DeserializeObject<List<StateList>>(c.States),
                                 SocialMediaData = JsonConvert.DeserializeObject<List<SocialMediaData>>(c.SocialMediaData ?? "[]")
                             }).OrderBy(c => c.Name).ToListAsync();

            return new AppResult<List<OrganisationListModel>>
            {
                Data = organisations,
                Message = "Successful",
                StatusCode = StatusCodes.Status200OK,
            };
        }

        /// <summary>
        /// Delete an organisation
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> SoftDelete(int Id)
        {
            var organisation = await _context.Organisations
                                  .IgnoreQueryFilters()
                                  .Include(c => c.Users)
                               .FirstOrDefaultAsync(c => c.Id == Id);

            var Name = organisation?.Name;

            if (organisation is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Organisation not found"
                };
            }

            //if organisation has users apply soft delete
            if (organisation.Users.Count > 0)
            {
                Guid n = Guid.NewGuid();

                organisation.IsSoftDeleted = true;
                organisation.Name += $"_deleted_{n}";
                organisation.ModifiedAt = CurrentDate;

                foreach (var user in organisation.Users)
                {
                    user.IsSoftDeleted = true;
                    user.Email += $"_deleted_{n}";
                    user.NormalizedEmail += $"_DELETED_{n}";
                    user.UserName += $"_deleted{n}";
                    user.NormalizedUserName += $"_DELETED{n}";
                    user.ModifiedAt = CurrentDate;
                }

                _context.Update(organisation);
            }
            else
            {
                _context.Remove(organisation);
            }

            await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                StatusCode = StatusCodes.Status204NoContent,
                Message = $"Delete Successful",
                Data = Name
            };
        }

        /// <summary>
        /// Returns list of State and local and wards
        /// </summary>
        /// <returns></returns>
        public async Task<AppResult<List<StateListModel>>> GetStateAndLGAList()
        {
            // this query has to be cached
            if (!_cache.TryGetValue(CacheKey.StateList, out List<StateListModel> States))
            {
                States = await _context.States.AsNoTracking()
                        .Select(c => new StateListModel
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Code = c.Code,
                            LocalGovernmentAreas = c.LocalGovernmentAreas.Select(d => new LGAListModel
                            {
                                Id = d.Id,
                                Name = d.Name,
                                Wards = d.Wards.Where(c => c.Key != "0").Select(w => new WardListModel
                                {
                                    Id = w.Id,
                                    Name = w.Name,
                                }).OrderBy(w => w.Name).ToList()
                            }).OrderBy(c => c.Name).ToList()
                        }).OrderBy(c => c.Name).ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddHours(168),
                };

                _cache.Set(CacheKey.StateList, States, cacheEntryOptions);
            }

            return new AppResult<List<StateListModel>>
            {
                Data = States,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successful"
            };
        }
    }

    /// <summary>
    /// redundant implementations
    /// </summary>
    public partial class OrganisationService
    {
        public async Task<AppResult<string>> AddStates()
        {
            var States = StateList2.states.OrderBy(c => c.Item1).ToList();

            States.Add(("Abuja", "FCT"));

            var statecode = 1;
            var lgacode = 1;
            foreach (var item in States)
            {
                var newState = new State()
                {
                    Name = item.Item1,
                    Code = item.Item2,
                    Key = statecode.ToString(),
                };

                var statedetailsURL = $"http://locationsng-api.herokuapp.com/api/v1/states/{item}/details";

                using (var http = new HttpClient())
                {
                    var response = await http.GetAsync(statedetailsURL);

                    var resp = await response.Content.ReadAsStringAsync();

                    var states = JsonConvert.DeserializeObject<stateandlocal>(resp);

                    var lgas = states.lgas.OrderBy(c => c);

                    foreach (var lga in lgas)
                    {
                        newState.LocalGovernmentAreas.Add(new LocalGovernmentArea
                        {
                            Name = lga,
                            Key = lgacode.ToString(),
                        });

                        lgacode++;
                    }
                }
                statecode++;

                _context.Add(newState);
            }

            var num = await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = num + "  "
            };
        }

        public async Task<AppResult<string>> AddWard(IFormFile file)
        {
            var stream = await GetExcelStream(file);
            if (stream is null)
            {
                return new AppResult<string>
                {
                    Errors = { "file is empty" }
                };
            }

            var worksteet = GetWorksheet(stream, 0);

            var lga = await _context.LocalGovernmentAreas.ToListAsync();

            var (wards, off) = Getwards(worksteet, lga);

            _context.AddRange(wards);
            int n = await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                Data = n.ToString(),
                Message = "Successful"
            };
        }

        internal class StateList2
        {
            public static List<(string, string)> states = new List<(string, string)>
        {  ("Abia","AB"),
           ("Adamawa","AD"),
           ("Akwa Ibom","AK"),
           ("Anambra","AN"),
           ("Bauchi","BA"),
           ("Bayelsa","BY"),
           ("Benue","BE"),
           ("Borno","BO"),
           ( "Cross River","CR"),
           ( "Delta","DE"),
           ( "Ebonyi","EB"),
           ("Edo", "ED"),
           ("Ekiti","EK"),
           ( "Enugu","EN"),
           ( "Gombe", "GO"),
           ("Jigawa", "JI"),
           ("Imo", "IM"),
           ("Kaduna","KD"),
           ("Kebbi","KE"),
           ("Kano", "KN"),
           ("Kogi", "KO"),
           ("Lagos","LA"),
           ("Katsina","KT"),
           ("Kwara", "KW"),
           ("Nasarawa", "NA"),
           ("Niger","NG"),
           ("Ogun","OG"),
           ("Rivers","RI"),
           ("Oyo","OY"),
           ("Osun","OS"),
           ("Sokoto","SO"),
           ("Plateau","PL"),
           ("Tara","TA"),
           ("Yobe","YO"),
           ("Zamfara","ZA")
    };
        }

        internal class stateandlocal
        {
            public string Name { get; set; }
            public List<string> lgas { get; set; }
        }

        private ExcelWorksheet GetWorksheet(Stream stream, int index = 0)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var package = new ExcelPackage(stream);

            return package.Workbook.Worksheets[index];
        }

        /// <summary>
        /// Verify ExcelSheet
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>

        private async Task<Stream> GetExcelStream(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                return null;
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var stream = new MemoryStream();

            await file.CopyToAsync(stream);

            return stream;
        }

        private (List<Ward>, List<(string, string)>) Getwards(ExcelWorksheet worksheet, List<LocalGovernmentArea> areas)
        {
            var rowCount = worksheet.Dimension.Rows;

            var wards = new List<Ward>();
            var off = new List<(string, string)>();
            var key = 0;

            for (var row = 1; row <= rowCount; row++)
            {
                var state = worksheet.Cells[$"A{row}"].Text;
                var lga = worksheet.Cells[$"B{row}"].Text;
                var ward = worksheet.Cells[$"C{row}"].Text;

                if (!string.IsNullOrWhiteSpace(lga) && !string.IsNullOrWhiteSpace(ward))
                {
                    var LGA = areas.FirstOrDefault(c => c.Name.Trim().ToLower() == lga.Trim().ToLower());

                    if (LGA != null)
                    {
                        wards.Add(new Ward
                        {
                            Name = ward.Trim(),
                            Key = (++key).ToString(),
                            LocalGovernmentAreaId = LGA.Id
                        });
                    }
                    else
                    {
                        off.Add((lga, ward));
                    }
                }
            }

            return (wards, off);
        }
    }
}