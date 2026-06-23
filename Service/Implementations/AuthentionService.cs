using Core.Data;
using Core.Entities;
using Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Service.Helpers;
using Service.Interfaces;
using Service.Models;
using Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implementation
{
    public class AuthenticationService : IAuthentication
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtSettings _jwt;
        private readonly INotification _notificationService;
        private readonly ApplicationDbContext _context;

        public AuthenticationService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
                IOptions<JwtSettings> options, INotification notification, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwt = options.Value;
            _notificationService = notification;
            _context = context;
        }

        public async Task<AppResult<LoginResponse>> Login(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Email);

            if (user is null)
            {
                user = await _userManager.FindByEmailAsync(model.Email);

                if (user is null)
                {
                    return new AppResult<LoginResponse>
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = "Login Failed, Invalid Login Credentials"
                    };
                }
            }
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return new AppResult<LoginResponse>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Email not Verified or Confirmed"
                };
            }

            var siginResult = await _signInManager.PasswordSignInAsync(user, model.Password, true, false);

            if (!siginResult.Succeeded)
            {
                return new AppResult<LoginResponse>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Login Failed, Invalid Login Credentials"
                };
            }

            var role = await _userManager.GetRolesAsync(user);

            var state = user.StateId.HasValue ? await _context.States.FindAsync(user.StateId) : null;
            var organisation = user.OrganisationId.HasValue ? await _context.Organisations.FindAsync(user.OrganisationId) : null;

            var (token, time) = TokenGenerator.GetToken(user, _jwt, role.FirstOrDefault());

            user.LastLogin = DateTime.Now;

            _context.Update(user);

            await _context.SaveChangesAsync();
            Donor donor = null;
            List<int> donorOrgsId = new List<int>();
            if (user.DonorId.HasValue)
            {
                donor = await _context.Donors.Include(x => x.Organisations).FirstOrDefaultAsync(x => x.Id == user.DonorId);
            }
            if (donor != null)
            {
                donorOrgsId = donor.Organisations.Select(donor => donor.Id).ToList();
            }

            return new AppResult<LoginResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = new LoginResponse
                {
                    Token = token,
                    ExpiryTime = time,
                    Role = role.FirstOrDefault(),
                    UserName = user.UserName,
                    Email = user.Email,
                    UserType = user.Type,
                    Id = user.Id,
                    StateId = user.StateId,
                    OrganisationId = user.OrganisationId,
                    State = state?.Name,
                    Organisation = organisation?.Name,
                    Designation = user.Designation,
                    DonorOrgsId = donorOrgsId,
                    LocalGovernments = user.LocalGovernments//?.Values ?? new List<LocalGovernmentVo.Data>(),

                },
                Message = "Login Successful"
            };
        }

        public async Task<AppResult<string>> ConfirmEmail(string UserId, string Code)
        {
            if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(Code))
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = { "Userid or Code not provided" }
                };
            }
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Unable to load user with Id '{UserId}'."
                };
            }
            Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));
            var result = await _userManager.ConfirmEmailAsync(user, Code);

            if (!result.Succeeded)
            {
                return new AppResult<string>
                {
                    Errors = result.Errors.Select(c => c.Description).ToList(),
                    StatusCode = StatusCodes.Status417ExpectationFailed,
                    Message = "Email Confirmation Failed"
                };
            }

            return new AppResult<string>()
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Email Confirmation Successful",
                Data = user.UserName
            };
        }

        public async Task<AppResult<string>> Logout()
        {
            await _signInManager.SignOutAsync();

            return new AppResult<string>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = "Sign out successful",
                Message = "Successful"
            };
        }

        public async Task<AppResult<string>> ForgotPassword(ForgotPasswordModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return new AppResult<string>
                {
                    Errors = { "Email must be provided" },
                    StatusCode = StatusCodes.Status400BadRequest,
                };
            }

            var user = await _userManager.FindByNameAsync(model.Email);

            if (user is null)
            {
                user = await _userManager.FindByEmailAsync(model.Email);

                if (user is null)
                {
                    return new AppResult<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "User not found"
                    };
                }
            }

            var Code = await _userManager.GeneratePasswordResetTokenAsync(user);

            var (status, message) = await _notificationService.SendForgotPasswordEmail(user, Code, model.Url);

            var adminEmail = string.Empty;

            if (!string.IsNullOrWhiteSpace(model.AdminUserId))
            {
                var admin = await _userManager.FindByIdAsync(model.AdminUserId);
                adminEmail = admin.Email;
                _ = await _notificationService.SendForgotPasswordEmail(admin, Code, model.Url);
            }

            if (!status)
            {
                return new AppResult<string>
                {
                    Data = user.UserName,
                    Errors = { $" Password Reset {message}" },
                    StatusCode = StatusCodes.Status417ExpectationFailed
                };
            }
            return new AppResult<string>
            {
                Data = user.UserName,
                Message = $"A password reset link has been sent to {(string.IsNullOrWhiteSpace(model.AdminUserId) ? user.Email : adminEmail)}",
                StatusCode = StatusCodes.Status200OK,
            };
        }

        public async Task<AppResult<string>> ResetPassword(string Id, string Code, string Password)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"Unable to load user with Id '{Id}'."
                };
            }

            Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));

            var result = await _userManager.ResetPasswordAsync(user, Code, Password);

            if (!result.Succeeded)
            {
                return new AppResult<string>
                {
                    Errors = result.Errors.Select(c => c.Description).ToList(),
                    StatusCode = StatusCodes.Status417ExpectationFailed,
                    Message = "Password Reset Failed"
                };
            }

            return new AppResult<string>()
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Password Reset Successful",
                Data = user.UserName
            };
        }

        public async Task<AppResult<string>> ResendConfirmationMail(string Email)
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                return new AppResult<string>
                {
                    Errors = { "Email or User name must be Provided" },
                    StatusCode = StatusCodes.Status400BadRequest,
                };
            }

            var user = await _userManager.FindByEmailAsync(Email);

            if (user is null)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "No recognised account for this email"
                };
            }
            if (user.EmailConfirmed)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status406NotAcceptable,
                    Message = "User Account has been verified, Login"
                };
            }

            var Code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var (status, message) = await _notificationService.SendConfirmationEmail(user, Code);

            if (!status)
            {
                return new AppResult<string>
                {
                    StatusCode = StatusCodes.Status417ExpectationFailed,
                    Errors = { message },
                };
            }

            return new AppResult<string>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = $"A confirmation mail has been sent to {user.Email}",
                Data = user.UserName,
            };
        }

        public async Task UpdateCases()
        {
            var cases = _context.Cases;

            foreach (var cs in cases)
            {
                if (cs.CaseCategoryId.HasValue) cs.CaseCategories = JsonConvert.SerializeObject(new List<string> { ((CaseCategoryOrTypeOfViolence)cs.CaseCategoryId).ToString() });
            }

            await _context.SaveChangesAsync();
        }

    }
}