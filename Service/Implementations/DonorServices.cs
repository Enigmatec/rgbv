using Core.Data;
using Core.Entities;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Extensions;
using Service.Helpers;
using Service.Interfaces;
using Service.Models;
using Service.StringKeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implementations
{
    public class DonorServices : IDonorService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpContext _httpContext;
        private readonly IUserService _userService;

        private string UserId => _httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private DateTime CurrentDate => DateTime.Now.ToUniversalTime().AddHours(1);

        public DonorServices(
            ApplicationDbContext context,
            IHttpContextAccessor accessor,
            IUserService userService)
        {
            _context = context;
            _httpContext = accessor.HttpContext;
            _userService = userService;
        }

        public async Task<AppResult<string>> CreateDonor(CreateDonorVM model)
        {
            var resultModel = new AppResult<string>();
            try
            {
                var donorExist = await _context.Donors.AnyAsync(x => x.Name.ToLower().Equals(model.Name));
                if (donorExist)
                {
                    return new AppResult<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Errors = { $"Donor {model.Name} already exists" },
                        Message = $"Donor creation failed"
                    };
                }

                var donor = new Donor
                {
                    Name = model.Name,
                    Acronym = model.Acronym,
                    CreatedById = UserId
                };

                foreach (var organisationId in model.OrganisationIds)
                {
                    var organisation = await _context.Organisations.FirstOrDefaultAsync(x => x.Id == organisationId);
                    if (organisation != null)
                    {
                        donor.Organisations.Add(organisation);
                    }
                }

                _context.Donors.Add(donor);
                await _context.SaveChangesAsync();
                return new AppResult<string>
                {
                    Data = donor.Id.ToString(),
                    Message = "Donor created successfully",
                    StatusCode = StatusCodes.Status201Created
                };
            }
            catch (Exception ex)
            {
                resultModel.AddError($"{ex.Message ?? ex.InnerException.Message}");
                return resultModel;
            }

        }

        public async Task<AppResult<string>> EditDonor(EditDonorVM model)
        {
            var resultModel = new AppResult<string>();
            try
            {
                var donorExist = await _context.Donors.FirstOrDefaultAsync(x => x.Id.Equals(model.DonorId));
                if (donorExist is null)
                {
                    return new AppResult<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Errors = { $"Donor does not exists" },
                        Message = $"Donor update failed"
                    };
                }

                donorExist.Name = string.IsNullOrEmpty(model.Name) ? donorExist.Name :model.Name ;
                donorExist.Acronym = string.IsNullOrEmpty(model.Acronym) ? donorExist.Acronym : model.Acronym;

                if (model.OrganisationIds.Count > 0)
                {
                    foreach (var organisationId in model.OrganisationIds)
                    {
                        var organisationExists = donorExist.Organisations.FirstOrDefault(x => x.Id.Equals(organisationId));
                        if (organisationExists is null)
                        {
                            var organisation = await _context.Organisations.FirstOrDefaultAsync(x => x.Id == organisationId);
                            if (organisation != null)
                            {
                                donorExist.Organisations.Add(organisation);
                            }
                        }

                    }
                }


                _context.Donors.Update(donorExist);
                await _context.SaveChangesAsync();
                return new AppResult<string>
                {
                    Data = donorExist.Id.ToString(),
                    Message = "Donor updated successfully",
                    StatusCode = StatusCodes.Status201Created
                };
            }
            catch (Exception ex)
            {
                resultModel.AddError($"{ex.Message ?? ex.InnerException.Message}");
                return resultModel;
            }

        }

        public async Task<AppResult<string>> AddUser(AddUserToDonor model)
        {
            var resultModel = new AppResult<string>();
            try
            {
                var donor = await _context.Donors.FirstOrDefaultAsync(x => x.Id == model.DonorId);
                if (donor == null)
                {
                    return new AppResult<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Errors = { $"Donor does not exists" },
                        Message = $"User creation failed"
                    };
                }

                var userCreationModel = new UserCreationModel
                {
                    FirstName = model.Name,
                    Designation = model.Designation,
                    Email = model.Email,
                    Role = RoleKeys.Donor
                };

                var userCreated = await _userService.CreateUser(userCreationModel);

                if (userCreated.StatusCode == StatusCodes.Status201Created)
                {
                    var user = await _userService.GetUserByEmail(userCreated.Data);
                    donor.Users.Add(user);
                    _context.Donors.Update(donor);
                    await _context.SaveChangesAsync();
                    resultModel.Data = userCreated.Data;
                    resultModel.StatusCode = userCreated.StatusCode;
                    resultModel.Message = userCreated.Message;

                }
                else
                {
                    resultModel.Data = userCreated.Data;
                    resultModel.StatusCode = userCreated.StatusCode;
                    resultModel.Message = userCreated.Message;
                    resultModel.Errors = userCreated.Errors;
                }
            }
            catch (Exception ex)
            {
                resultModel.AddError($"{ex.Message ?? ex.InnerException.Message}");
                return resultModel;
            }
            return resultModel;
        }

        public async Task<AppResult<string>> UpdateUser(EditUserDonor model)
        {
            var resultModel = new AppResult<string>();
            try
            {
                var userCreationModel = new UserCreationModel
                {
                    FirstName = model.Name,
                    Designation = model.Designation,
                    Email = model.Email,
                    Role = RoleKeys.Donor
                };

                var userUpdated = await _userService.UpdateUser(model.userId, userCreationModel);

                if (userUpdated.StatusCode == StatusCodes.Status200OK)
                {
                    resultModel.Data = userUpdated.Data;
                    resultModel.StatusCode = userUpdated.StatusCode;
                    resultModel.Message = userUpdated.Message;

                }
                else
                {
                    resultModel.Data = userUpdated.Data;
                    resultModel.StatusCode = userUpdated.StatusCode;
                    resultModel.Message = userUpdated.Message;
                    resultModel.Errors = userUpdated.Errors;
                }
            }
            catch (Exception ex)
            {
                resultModel.AddError($"{ex.Message ?? ex.InnerException.Message}");
                return resultModel;
            }
            return resultModel;
        }
        public async Task<AppResult<PaginatedList<DonorVM>>> GetAllDonors(DonoSearchModel model)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return new AppResult<PaginatedList<DonorVM>>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized"
                };
            }

            var query = _context.Donors.OrderByDescending(x => x.CreatedAt).AsQueryable();

            if (model.Acronym != null)
            {
                query = query.Where(x => x.Acronym.Contains(model.Acronym));
            }
            if (model.Name != null)
            {
                query = query.Where(x => x.Name.Contains(model.Name));
            }

            var donorsList = query.Select(x => new DonorVM
            {
                Name = x.Name,
                Acronym = x.Acronym,
                DateCreated = x.CreatedAt,
                Id = x.Id,
                NumberOfOrganisations = x.Organisations.Count,
                DonorUsers = x.Users.Select(c => new DonorUsersVM
                {
                    DateCreated = c.CreatedAt,
                    Designation = c.Designation,
                    Email = c.Email,
                    Name = c.FirstName,
                    Id = c.Id
                }).ToList(),
                OrganisationIds = x.Organisations.Select(x => x.Id).ToList()
            });
            //Add all the ids of the organisations
            var data = await donorsList.PageAsync(model.PageIndex, model.PageSize);

            return new AppResult<PaginatedList<DonorVM>>
            {
                Data = data,
                Message = "Successful",
                StatusCode = StatusCodes.Status200OK
            };
        }
        public async Task<AppResult<DonorUsersScreenVM>> GetScreenMetric(int? donorId)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return new AppResult<DonorUsersScreenVM>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized"
                };
            }
            ApplicationUser user;
            Donor donor;
            if (donorId == 0 || donorId == null)
            {
                user = await _userService.GetUserById(UserId);
                if (!user.DonorId.HasValue)
                {
                    return new AppResult<DonorUsersScreenVM>
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "Forbidden"
                    };
                }
                donor = await _context.Donors.Include(x => x.Organisations).FirstOrDefaultAsync(x => x.Id.Equals(user.DonorId));
            }
            else
            {
                donor = await _context.Donors.Include(x => x.Organisations).Include(x => x.Users).FirstOrDefaultAsync(x => x.Id.Equals(donorId));
            }




            var cases = _context.Cases.AsQueryable();
            var services = _context.ServicesProvided.AsQueryable();
            int numberOfIncidentsFromAllOrganisations = 0;
            int numberOfServicesFromAllOrganisations = 0;
            foreach (var item in donor.Organisations)
            {
                var numberOfIncidentsFromOrganisation = cases.Where(x => x.OrganisationId == item.Id).Count();
                var numberOfServicesFromOrganisation = services.Where(x => x.OrganisationId == item.Id).Count();

                numberOfIncidentsFromAllOrganisations += numberOfIncidentsFromOrganisation;
                numberOfServicesFromAllOrganisations += numberOfServicesFromOrganisation;
            }


            var data = new DonorUsersScreenVM
            {
                NumberOfOrganisations = donor.Organisations.Count,
                NumberOfIncidentsFromAllOrganisations = numberOfIncidentsFromAllOrganisations,
                NumberOfServicesFromAllOrganisations = numberOfServicesFromAllOrganisations,
                Acronym = donor.Acronym,
                Name = donor.Name,
                MembersCount = donor.Users.Count,
                DateCreated = donor.CreatedAt
            };

            return new AppResult<DonorUsersScreenVM>
            {
                Data = data,
                Message = "Successful",
                StatusCode = StatusCodes.Status200OK
            };

        }
        public async Task<AppResult<PaginatedList<DonorInformationVM>>> GetScreenPaginated(DonoSearchModel model, int? donorId)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return new AppResult<PaginatedList<DonorInformationVM>>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized"
                };
            }
            ApplicationUser user;
            Donor donor;
            if (donorId == 0 || donorId == null)
            {
                user = await _userService.GetUserById(UserId);
                if (!user.DonorId.HasValue)
                {
                    return new AppResult<PaginatedList<DonorInformationVM>>
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "Forbidden"
                    };
                }
                donor = await _context.Donors.Include(x => x.Organisations).FirstOrDefaultAsync(x => x.Id.Equals(user.DonorId));
            }
            else
            {
                donor = await _context.Donors.Include(x => x.Organisations).Include(x => x.Users).FirstOrDefaultAsync(x => x.Id.Equals(donorId));
            }




            var cases = _context.Cases.AsQueryable();
            var services = _context.ServicesProvided.AsQueryable();
            var donorInformations = new List<DonorInformationVM>();
            foreach (var item in donor.Organisations)
            {
                var numberOfIncidentsFromOrganisation = cases.Where(x => x.OrganisationId == item.Id).Count();
                var numberOfServicesFromOrganisation = services.Where(x => x.OrganisationId == item.Id).Count();

                var donorInformation = new DonorInformationVM
                {
                    OrganisationId = item.Id,
                    OrganisationName = item.Name,
                    NoOfServices = numberOfServicesFromOrganisation,
                    NumberOfReportedIncidents = numberOfIncidentsFromOrganisation,
                    DateCreated = item.CreatedAt
                };
                donorInformations.Add(donorInformation);
            }

            var infoResult = donorInformations.AsQueryable();

            if (!string.IsNullOrEmpty(model.Name))
            {
                infoResult = infoResult.Where(x => x.OrganisationName.Contains(model.Name));
            }

            var data = infoResult.Page(model.PageIndex, model.PageSize);

            return new AppResult<PaginatedList<DonorInformationVM>>
            {
                Data = data,
                Message = "Successful",
                StatusCode = StatusCodes.Status200OK
            };

        }


    }
}
