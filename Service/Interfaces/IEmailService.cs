using Service.Models;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IEmailService
    {
        Task<(bool, string)> TemplateSendAsync<T>(string recipient, string subject, string templateName, TemplateModel<T> m);
        Task<(bool, string)> SendCases(UserViewModel recipient, byte[] stream, string filename);
        Task<(bool, string)> SendErrorToAdmin(UserViewModel recipient, string errorMessage);
    }
}