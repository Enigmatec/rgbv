using Core.Entities;
using Service.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface INotification
    {
        Task SendCaseUpdateNotification(List<ApplicationUser> Users, string Editor, string State, string IncidentCode, string Organisation);

        Task<(bool, string)> SendConfirmationEmail(ApplicationUser User, string Code);

        Task SendDailyReportSummary(List<ApplicationUser> Users, CaseReportModel report);

        Task SendDailyStateReportSummary(ApplicationUser User, CaseReportModel report, string State);

        Task<(bool, string)> SendForgotPasswordEmail(ApplicationUser user, string Code, string Url);

        Task<(bool, string)> SendPasswordChangeEmail(ApplicationUser User);

        Task<(bool, string)> SendPassWordEmail(ApplicationUser User, string password);

        Task<(bool, string)> SendCaseRejectedEmail(string userEmail, string userName, string incidentCode, string note);
    }
}