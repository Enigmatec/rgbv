using Core.Entities;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Service.Interfaces;
using Service.Models;
using Service.StringKeys;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Service.Implementation
{
    public class NotificationService : INotification
    {
        private readonly HttpContext _httpContext;

        private readonly IEmailService _emailService;

        public NotificationService(IHttpContextAccessor accessor, IEmailService email)
        {
            _httpContext = accessor.HttpContext;
            _emailService = email;
        }

        public async Task<(bool, string)> SendConfirmationEmail(ApplicationUser user, string Code)
        {
            Code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(Code));

            var callbackUrl = $"{_httpContext.Request.Scheme}://{_httpContext.Request.Host}/api/account/confirmemail/{user.Id}?Code={Code}";

            var Model = new RegistrationMailModel
            {
                ConfirmationUrl = HtmlEncoder.Default.Encode(callbackUrl),
                UserName = $"{ user.FirstName }  {user.LastName}"
            };
            var template = new TemplateModel<RegistrationMailModel>(Model).Add("Title", "Email Confirmation");

            return await _emailService.TemplateSendAsync(user.Email, "Email Confirmation", EmailKeys.RegistrationEmail, template);
        }

        public async Task<(bool, string)> SendPassWordEmail(ApplicationUser User, string password)
        {
            var Model = new PasswordMailModel
            {
                Email = User.Email,
                Password = password,
                Name = $"{User.FirstName}  {User.LastName}"
            };

            var template = new TemplateModel<PasswordMailModel>(Model).Add("Title", "Account Login Details");

            return await _emailService.TemplateSendAsync(User.Email, "Account Login Details for National SGBV Data Collation Tool", EmailKeys.PasswordEmail, template);
        }

        public async Task<(bool, string)> SendCaseRejectedEmail(string userEmail, string userName, string incidentCode, string note)
        {
            var Model = new CaseRejectedEmailModel
            {
                UserName = userName,
                IncidentCode = incidentCode,
                Note = note
            };

            var template = new TemplateModel<CaseRejectedEmailModel>(Model).Add("Title", "Case Rejected");

            return await _emailService.TemplateSendAsync(userEmail, $"Case Rejected [{incidentCode}]", EmailKeys.CaseRejectedEmail, template);
        }

        public async Task<(bool, string)> SendForgotPasswordEmail(ApplicationUser user, string Code, string Url)
        {
            Code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(Code));

            //change the frontend change password link
            var callbackUrl = $"{Url}/{user.Id}?Code={Code}";

            var Model = new RegistrationMailModel
            {
                ConfirmationUrl = HtmlEncoder.Default.Encode(callbackUrl),
                UserName = $"{ user.FirstName }  {user.LastName}"
            };
            var template = new TemplateModel<RegistrationMailModel>(Model).Add("Title", "Password Reset");

            return await _emailService.TemplateSendAsync(user.Email, "Password Reset", EmailKeys.PasswordResetEmail, template);
        }

        public async Task<(bool, string)> SendPasswordChangeEmail(ApplicationUser User)
        {
            var Model = new PasswordMailModel
            {
                Email = User.Email,
                Name = $"{User.FirstName}  {User.LastName}"
            };

            var template = new TemplateModel<PasswordMailModel>(Model).Add("Title", "Password Change Notification");

            return await _emailService.TemplateSendAsync(User.Email, "Password Change Notification", EmailKeys.PasswordChangeEmail, template);
        }

        public async Task SendCaseUpdateNotification(List<ApplicationUser> Users, string Editor, string State, string IncidentCode, string Organisation)
        {
            foreach (var User in Users)
            {
                var Model = new CaseUpdateMailModel
                {
                    Name = $"{User.FirstName}  {User.LastName}",
                    Editor = Editor,
                    IncidentCode = IncidentCode,
                    Organisation = Organisation,
                    State = State
                };

                var template = new TemplateModel<CaseUpdateMailModel>(Model).Add("Title", "Case Update Notification");

                await _emailService.TemplateSendAsync(User.Email, "Case Update Notification", EmailKeys.CaseUpdateEmail, template);
            }
        }

        public async Task SendDailyReportSummary(List<ApplicationUser> Users, CaseReportModel report)
        {
            foreach (var user in Users)
            {
                var model = new ReportMailModel
                {
                    Email = user.Email,
                    Name = $"{user.FirstName}  {user.LastName}",
                    CaseReport = report,
                };

                var template = new TemplateModel<ReportMailModel>(model).Add("Title", $"{model.Date} Weekly Incident Report Summary");

                var (status, message) = await _emailService.TemplateSendAsync(user.Email, $"{model.Date} Weekly Incident Report Summary", EmailKeys.DailyReportSummary, template);

                //Can do check if mail fails
            }
        }

        public async Task SendDailyStateReportSummary(ApplicationUser User, CaseReportModel report, string State)
        {
            var model = new ReportMailModel
            {
                Email = User.Email,
                Name = $"{User.FirstName}  {User.LastName}",
                CaseReport = report,
                State = State
            };

            var template = new TemplateModel<ReportMailModel>(model).Add("Title", $"{model.Date} Weekly Incident Report Summary");

            await _emailService.TemplateSendAsync(User.Email, $"{model.Date} Weekly Incident Report Summary", EmailKeys.DailyStateReportSummary, template);
        }
    }
}