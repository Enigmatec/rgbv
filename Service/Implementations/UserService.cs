using Core.Data;
using Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

namespace Service.Implementation
{
    /// <summary>
    /// Manages the all service related to Users create update delete
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly INotification _notificationService;
        private readonly HttpContext _httpContext;

        private string UserId => _httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private DateTime CurrentDate => DateTime.Now.ToUniversalTime().AddHours(1);

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> role,
        ApplicationDbContext context, INotification notification, IHttpContextAccessor accessor)
        {
            _roleManager = role;
            _userManager = userManager;
            _context = context;
            _notificationService = notification;
            _httpContext = accessor.HttpContext;
        }

        /// <summary>
        /// Returns the list of roles
        /// </summary>
        /// <returns></returns>
        public async Task<AppResult<List<RolesListModel>>> GetRoles()
        {
            var roles = await _roleManager.Roles.AsNoTracking()
                        .Select(c => new RolesListModel
                        {
                            Id = c.Id,
                            Name = c.Name,
                        }).ToListAsync();
            return new AppResult<List<RolesListModel>>
            {
                Data = roles,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successful"
            };
        }

        /// <summary>
        /// Handles User validation and creation
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> CreateUser(UserCreationModel model)
        {
            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = { $"Role {model.Role} does not exist" },
                    Message = $"User Creation failed, Role {model.Role} does not exist"
                };
            }

            //Validates the model to ensure the parameters for the creation of a user are consistent
            if (!model.OrganisationId.HasValue)
            {
                if (model.Role == RoleKeys.CSO || model.Role == RoleKeys.ServiceProvider || model.Role == RoleKeys.CSOSupervior
                    || model.Role == RoleKeys.ServiceProviderSupervior || model.Role == RoleKeys.ImplementingPartner)
                {
                    return new AppResult<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Errors = { $"Individuals with role CSO or Service Provider or Implementing Partner must belong to an Organisation, Kindly assign an organisation or create one" },
                        Message = "Creation Failed, Individuals with role CSO or Service Provider or Implementing Partner must belong to an Organisation, Kindly assign an organisation or create one"
                    };
                }
            }
            else
            {
                if (!(model.Role == RoleKeys.CSO || model.Role == RoleKeys.ServiceProvider || model.Role == RoleKeys.CSOSupervior
                    || model.Role == RoleKeys.ServiceProviderSupervior || model.Role == RoleKeys.ImplementingPartner))
                {
                    return new AppResult<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Creation Failed, Individuals assigned to an Organisation must have role CSO or Service Provider or Implementing Partner",
                        Errors = { $"Individuals assigned to an Organisation must have role CSO or Service Provider or Implementing Partner" }
                    };
                }

                var organisation = await _context.Organisations.AsNoTracking()
                                   .FirstOrDefaultAsync(c => c.Id == model.OrganisationId);

                if (organisation is null)
                {
                    return new AppResult<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Creation Failed, Organisation was not found",
                        Errors = { $"Organisation was not found" }
                    };
                }

                if (!model.StateId.HasValue)
                {
                    return new AppResult<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Creation Failed, user must be assigned a state",
                        Errors = { $"user must be assigned a state" }
                    };
                }
                var states = JsonConvert.DeserializeObject<List<StateList>>(organisation.States);

                if (!states.Any(c => c.Id == model.StateId.Value))
                {
                    return new AppResult<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Creation Failed, user must be assigned a state that the parent organisation operates",
                        Errors = { $"user must be assigned a state that the parent organisation operates" }
                    };
                }
            }

            if ((model.Role == RoleKeys.StateSupervisor || model.Role == RoleKeys.StateAdministrator || model.Role == RoleKeys.LocalGovernment) && !model.StateId.HasValue)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Creation Failed, Individuals with role State Supervisor or StateAdministrator or Local Government must be assigned a state",
                    Errors = { $"Individuals with role State Supervisor must have a state assigned" }
                };
            }

            if ((model.Role == RoleKeys.LocalGovernment && !model.LocalGovernmentIds.Any()))
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Creation Failed, Individuals with role Local Government must be assigned a local Government",
                    Errors = { $"Individuals with role Local Government must be assigned a local Government" }
                };
            }

            var User = new ApplicationUser();

            //set user code
            string code = "";
            while (true)
            {
                code = Guid.NewGuid().ToString().Substring(0, 4).Concat(new Random().Next(0, 100).ToString("00"));

                User = await _context.Users.AsNoTracking().FirstOrDefaultAsync(c => c.Code == code);

                if (User == null)
                {
                    break;
                }
            }

            User = new ApplicationUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                PhoneNumber = model.PhoneNumber,
                SecurityStamp = Guid.NewGuid().ToString(),
                OrganisationId = model.OrganisationId,
                Type = model.Role,
                Code = code,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Designation = model.Designation,
                StateId = model.StateId,
                EmailConfirmed = true,
                IsActivated = true,
            };

            var lgas = new List<LgaModel>();
            if (model.LocalGovernmentIds != null)
            {
                foreach (var lgaId in model.LocalGovernmentIds)
                {
                    var lga = await _context.LocalGovernmentAreas.FirstOrDefaultAsync(l => l.Id == lgaId);

                    if (lga is null) return new AppResult<string>
                    {
                        Data = User.Id,
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Updated Successful",
                    };

                    lgas.Add(new LgaModel { Id = lga.Id, Name = lga.Name });
                }

                User.LocalGovernmentAreas = JsonConvert.SerializeObject(lgas);
            }
            else
            {
                User.LocalGovernmentAreas = "[]";
            }

            var password = Guid.NewGuid().ToString().Substring(0, 4) + Guid.NewGuid().ToString().Substring(0, 4);
            if(model.DonorId.HasValue)
                User.DonorId = model.DonorId.Value;
            var result = await _userManager.CreateAsync(User, password);

            if (!result.Succeeded)
            {
                return new AppResult<string>
                {
                    Errors = result.Errors.Select(c => c.Description).ToList(),
                    StatusCode = StatusCodes.Status417ExpectationFailed,
                    Message = $"Registration Failed, {result.Errors.Select(c => c.Description).FirstOrDefault()}"
                };
            }

            var roleResult = await _userManager.AddToRoleAsync(User, model.Role);

            if (!roleResult.Succeeded)
            {
                var deleteResult = await _userManager.DeleteAsync(User);

                return new AppResult<string>
                {
                    Errors = roleResult.Errors.Select(c => c.Description).ToList(),
                    StatusCode = StatusCodes.Status417ExpectationFailed,
                    Message = $"Registration Failed, {result.Errors.Select(c => c.Description).FirstOrDefault()}"
                };
            }

            // var Code = await _userManager.GenerateEmailConfirmationTokenAsync(User);

            var (status, message) = await _notificationService.SendPassWordEmail(User, password);

            if (!status)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status417ExpectationFailed,
                    Errors = { $"Registration Successful, but confirmation {message}", "Kindly Contact Admin" },
                    Message = $"Registration Successful, but confirmation {message}, Kindly Contact Admin"
                };
            }

            return new AppResult<string>
            {
                Data = User.Email,
                Message = $"Registration Successful, A confirmation link has been sent to {User.Email}",
                StatusCode = StatusCodes.Status201Created,
            };
        }

        /// <summary>
        /// returns a user with id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<AppResult<UserViewModel>> GetUser(string Id)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                return new AppResult<UserViewModel>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = { "User Id was not provided" }
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
                           IsActivated = c.IsActivated,
                           LocalGovernments = c.LocalGovernments,
                       })
                       .FirstOrDefaultAsync(c => c.Id == Id);

            if (user is null)
            {
                return new AppResult<UserViewModel>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Errors = { "User not found" }
                };
            }

            return new AppResult<UserViewModel>
            {
                Data = user,
                Message = "Successful",
                StatusCode = StatusCodes.Status200OK
            };
        }

        /// <summary>
        /// returns a user with Email
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<ApplicationUser> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user;

        }

        public async Task<ApplicationUser> GetUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user;

        }

        /// <summary>
        /// Get the list of users with pagination
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<PaginatedList<UserViewModel>>> GetUsers(UserSearchModel model)
        {
            var query = _context.Users
                .OrderByDescending(c => c.CreatedAt).AsNoTracking();

            //apply filter
            if (model.StateId.HasValue)
            {
                query = query.Where(c => c.StateId == model.StateId);
            }

            if (model.OrganisationId.HasValue)
            {
                query = query.Where(c => c.OrganisationId == model.OrganisationId);
            }

            if (!string.IsNullOrWhiteSpace(model.Role))
            {
                query = query.Where(c => c.Type.ToLower() == model.Role.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(model.Organisation))
            {
                query = query.Where(c => c.Organisation.Name.Contains(model.Organisation.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var name = model.Email.Trim().ToLower();

                query = query.Where(c => c.Email.Trim().ToLower().StartsWith(name) || c.LastName.Trim().ToLower().StartsWith(name) || c.FirstName.Trim().ToLower().StartsWith(name));
            }

            var userQuery = query.Select(c => new UserViewModel
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
                LocalGovernments = c.LocalGovernments != null ? c.LocalGovernments : new List<LgaModel>(),
            }).OrderBy(c => c.Organisation)
                               .ThenBy(c => c.FirstName);

            var Users = await userQuery.PageAsync(model.PageIndex, model.PageSize);

            return new AppResult<PaginatedList<UserViewModel>>
            {
                Data = Users,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successful"
            };
        }

        /// <summary>
        /// updates the user with with id
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> UpdateUser(string Id, UserCreationModel model)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = { "User Id was not provided" },
                    Message = "User Id was not provided"
                };
            }
            var user = await _userManager.FindByIdAsync(Id);

            if (user is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User not found, Update was not completed"
                };
            }

            if (!model.OrganisationId.HasValue)
            {
                if (model.Role == RoleKeys.CSO || model.Role == RoleKeys.ServiceProvider || model.Role == RoleKeys.CSOSupervior || model.Role == RoleKeys.ServiceProviderSupervior)
                {
                    return new AppResult<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Errors = { $"Individuals with role CSO or Service Provider must belong to an Organisation, Kindly assign an organisation or create one" },
                        Message = $"Update Failed, Individuals with role CSO or Service Provider must belong to an Organisation, Kindly assign an organisation or create one"
                    };
                }
            }
            else
            {
                if (!(model.Role == RoleKeys.CSO || model.Role == RoleKeys.ServiceProvider || model.Role == RoleKeys.CSOSupervior || model.Role == RoleKeys.ServiceProviderSupervior))
                {
                    return new AppResult<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = " Update Failed, Individual with assigned to an Organisation must have role CSO or Service Provider",
                        Errors = { $"Individual with assigned to an Organisation must have role CSO or Service Provider" }
                    };
                }

                var organisation = await _context.Organisations.AsNoTracking()
                                   .FirstOrDefaultAsync(c => c.Id == model.OrganisationId);

                if (organisation is null)
                {
                    return new AppResult<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "User Update Failed, Organisation was not found",
                        Errors = { $"Organisation was not found" }
                    };
                }

                if (!model.StateId.HasValue)
                {
                    return new AppResult<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "User update Failed, user must be assigned a state",
                        Errors = { $"user must be assigned a state" }
                    };
                }

                var states = JsonConvert.DeserializeObject<List<StateList>>(organisation.States);

                if (!states.Any(c => c.Id == model.StateId.Value))
                {
                    return new AppResult<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "User Update Failed, user must be assigned a state that the parent organisation operates",
                        Errors = { $"user must be assigned a state that the parent organisation operates" }
                    };
                }
            }

            if ((model.Role == RoleKeys.StateSupervisor || model.Role == RoleKeys.StateAdministrator || model.Role == RoleKeys.LocalGovernment) && !model.StateId.HasValue)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Creation Failed, Individuals with role State Supervisor or Administrator or Local Government must be assigned a state",
                    Errors = { $"Individuals with role State Supervisor must have a state assigned" }
                };
            }

            if (user.Type.ToLower() != model.Role.ToLower())
            {
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    return new AppResult<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Errors = { $"Role {model.Role} does not exist" },
                        Message = "User Update failed, " + $"Role {model.Role} does not exist"
                    };
                }

                var result2 = await _userManager.RemoveFromRoleAsync(user, user.Type);

                if (!result2.Succeeded)
                {
                    return new AppResult<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Errors = { $"Role {model.Role} could not be assigned" },
                        Message = "User Update failed, " + $"Role {model.Role} could not be assigned"
                    };
                }

                result2 = await _userManager.AddToRoleAsync(user, model.Role);

                if (!result2.Succeeded)
                {
                    return new AppResult<string>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Errors = { $"Role {model.Role} could not be assigned" },
                        Message = "User Update failed, " + $"Role {model.Role} could not be assigned"
                    };
                }

                user.Type = model.Role;
            }

            //update values
            user.Email = model.Email;
            user.UserName = model.Email;
            user.NormalizedEmail = model.Email.ToUpper();
            user.NormalizedUserName = model.Email.ToUpper();
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.StateId = model.StateId;
            user.Designation = model.Designation;
            user.OrganisationId = model.OrganisationId;
            user.ModifiedAt = CurrentDate;

            var lgas = new List<LgaModel>();
            if (model.LocalGovernmentIds != null)
            {
                foreach (var lgaId in model.LocalGovernmentIds)
                {
                    var lga = await _context.LocalGovernmentAreas.FirstOrDefaultAsync(l => l.Id == lgaId);

                    if (lga is null) return new AppResult<string>
                    {
                        Data = user.Id,
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Updated Successful",
                    };

                    lgas.Add(new LgaModel { Id = lga.Id, Name = lga.Name });
                }

                user.LocalGovernmentAreas = JsonConvert.SerializeObject(lgas);
            }
            else
            {
                user.LocalGovernmentAreas = "[]";
            }

            var result3 = await _userManager.UpdateAsync(user);

            if (!result3.Succeeded)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = result3.Errors.Select(c => c.Description).ToList(),
                    Message = $"User Update failed {result3.Errors.Select(c => c.Description).FirstOrDefault()}"
                };
            }

            return new AppResult<string>
            {
                Data = user.Id,
                StatusCode = StatusCodes.Status200OK,
                Message = "Updated Successful",
            };
        }

        /// <summary>
        /// gets the user profile
        /// </summary>
        /// <returns></returns>
        public async Task<AppResult<UserProfileViewModel>> GetProfile()
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return new AppResult<UserProfileViewModel>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Errors = { "Not authorised to view this resource" },
                    Message = "Not authorised"
                };
            }

            var User = await _context.Users.AsNoTracking()
                             .Where(c => c.Id == UserId)
                             .Select(c => new UserProfileViewModel
                             {
                                 Id = c.Id,
                                 UserName = c.UserName,
                                 FirstName = c.FirstName,
                                 LastLogin = c.LastLogin,
                                 LastName = c.LastName,
                                 PhoneNumber = c.PhoneNumber,
                                 Email = c.Email,
                                 Role = c.Type,
                                 Designation = c.Designation,
                                 StateId = c.StateId,
                                 State = c.StateId.HasValue ? c.State.Name : null,
                                 OrganisationId = c.OrganisationId,
                                 Organisation = c.OrganisationId.HasValue ? c.Organisation.Name : null,
                                 ProfileUrl = c.ProfileUrl,
                                 ModifiedAt = c.ModifiedAt,
                                 NumberOfCaseEntered = c.CreatedCases.Count(),
                                 LocalGovernments = c.LocalGovernments,
                             }).FirstOrDefaultAsync();

            if (User is null)
            {
                return new AppResult<UserProfileViewModel>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User not found"
                };
            }

            return new AppResult<UserProfileViewModel>
            {
                Data = User,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successful"
            };
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> UpdateProfile(UserProfileUpdateModel model)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Errors = { "Not authorised to view this resource" },
                    Message = "Not authorised"
                };
            }

            var User = await _userManager.FindByIdAsync(UserId);

            if (User is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User not found"
                };
            }

            User.FirstName = model.FirstName;
            User.LastName = model.LastName;
            User.PhoneNumber = model.PhoneNumber;
            User.Designation = model.Designation;
            User.ModifiedAt = CurrentDate;

            _context.Update(User);

            await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                Data = User.Id,
                Message = " Update Successful",
                StatusCode = StatusCodes.Status200OK
            };
        }

        /// <summary>
        /// activates ot deactivates user
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> ActivateOrDeactivateUser(string Id)
        {
            var User = await _userManager.FindByIdAsync(Id);

            if (User is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User not found, Update was not completed"
                };
            }

            User.IsActivated = !User.IsActivated;
            User.ModifiedAt = CurrentDate;

            var result = await _userManager.UpdateAsync(User);

            if (!result.Succeeded)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status417ExpectationFailed,
                    Errors = result.Errors.Select(c => c.Description).ToList(),
                    Message = "User update failed"
                };
            }

            return new AppResult<string>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = $"{User.Email}",
                // Message = User.IsActivated ? "User Activated" : "User Deactivated"
            };
        }

        /// <summary>
        /// changes the password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> ChangePassword(ChangePasswordModel model)
        {
            if (string.IsNullOrWhiteSpace(UserId))
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Errors = { "Not authorised to view this resource" },
                    Message = "Not authorised"
                };
            }

            var User = await _userManager.FindByIdAsync(UserId);

            if (User is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User not found"
                };
            }
            User.ModifiedAt = CurrentDate;

            var result = await _userManager.ChangePasswordAsync(User, model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status417ExpectationFailed,
                    Errors = result.Errors.Select(c => c.Description).ToList(),
                    Message = "Password update failed"
                };
            }

            //send notification email
            await _notificationService.SendPasswordChangeEmail(User);

            return new AppResult<string>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Password update Successful",
                Data = User.Email
            };
        }

        /// <summary>
        /// delete the user but performs a soft delete
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<AppResult<string>> SoftDelete(string Id)
        {
            //check the users while ignoring query filters
            var user = await _context.Users.IgnoreQueryFilters()
                        //.Include(c => c.CreatedCases)
                        //.Include(c => c.ApprovedCases)
                        //.Include(c => c.ValidatedCases)
                        .FirstOrDefaultAsync(c => c.Id == Id);

            var username = $"{user.FirstName} {user.LastName}";
            if (user is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"user not found"
                };
            }

            //checks if the users have cases under its Id and if true, uses softdelete
            //  if (user.CreatedCases.Count > 0 || user.ApprovedCases.Count > 0 || user.ValidatedCases.Count > 0)
            //{
            string id = Guid.NewGuid().ToString();
            user.IsSoftDeleted = true;
            user.Email += $"{id}_deleted";
            user.NormalizedEmail += $"{id}_DELETED";
            user.UserName += $"{id}_deleted";
            user.NormalizedUserName += $"{id}_DELETED";
            user.ModifiedAt = CurrentDate;
            _context.Update(user);
            //}
            //else // if no cases then a hard delete is used
            //{
            //    _context.Remove(user);
            //}

            await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                StatusCode = StatusCodes.Status204NoContent,
                Data = username,
                Message = "Delete Successful"
            };
        }

        /// <summary>
        /// redundant method used to reset all users paswords
        /// </summary>
        /// <returns></returns>
        public async Task<AppResult<string>> ResetUser()
        {
            var users = await _context.Users.Where(c => c.Type != RoleKeys.Administrator)
                       .ToListAsync();

            foreach (var user in users)
            {
                var password = Guid.NewGuid().ToString().Substring(0, 4) + Guid.NewGuid().ToString().Substring(0, 4);

                user.UserName = user.Email.Trim();
                user.NormalizedUserName = user.Email.Trim().ToUpper();
                user.Email = user.Email.Trim();
                user.NormalizedEmail = user.Email.Trim().ToUpper();

                var result = await _userManager.RemovePasswordAsync(user);
                if (!result.Succeeded)
                {
                    return new AppResult<string>
                    {
                        Errors = result.Errors.Select(c => c.Description).ToList(),
                        Message = "password remove failed " + user.FirstName,
                        StatusCode = StatusCodes.Status400BadRequest,
                    };
                }

                result = await _userManager.AddPasswordAsync(user, password);

                if (!result.Succeeded)
                {
                    return new AppResult<string>
                    {
                        Errors = result.Errors.Select(c => c.Description).ToList(),
                        Message = "password remove failed " + user.FirstName,
                        StatusCode = StatusCodes.Status400BadRequest,
                    };
                }

                var (status, message) = await _notificationService.SendPassWordEmail(user, password);
            }

            var i = await _context.SaveChangesAsync();

            return new AppResult<string>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = "Success",
                Message = i.ToString()
            };
        }
    }
}