using Service.Helpers;
using Service.Models;
using Service.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IDashboardService
    {
        Task<AppResult<StateHomeVM>> ByState(DashboardDetails model, SearchModel searchModel);
        Task<AppResult<StateHomeVM>> IncidentCasesByStateByLgaByWard(StateAndLgaFilter model);
        Task<AppResult<List<CaseBySubject>>> IncidentCasesByStateByLga(StateAndLgaFilter model);
        Task<AppResult<StateHomeVM>> ServicesByStateByLgaByWard(StateAndLgaFilter model);
        Task<AppResult<StateHomeVM>> ServicesByStateByLga(StateAndLgaFilter model);
        //Task<AppResult<List<StateHomeVM>>> ServicesByState();
        Task<AppResult<List<CSOProviders>>> CsoProvidersByState();
        Task<AppResult<TotalNumberofCSOSP>> CsoProvidersByStateByLga(StateAndLgaFilter model);
        Task<AppResult<PaginatedList<CSOSPVM>>> CsoProvidersByStateByLgaByWard(StateAndLgaFilter model);

        AppResult<string> ClearDashboardCache();
        Task<AppResult<ServicesDashboard>> GetServicesDashboard(SearchModel model);
    }
}
