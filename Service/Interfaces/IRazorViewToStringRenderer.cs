using Service.Models;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TemplateModel<TModel> model);
    }
}