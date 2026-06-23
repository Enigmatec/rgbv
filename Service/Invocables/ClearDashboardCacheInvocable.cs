using System.Threading.Tasks;
using Coravel.Invocable;
using Service.AppServices;

namespace Service.Invocables
{
    public class ClearDashboardCacheInvocable : IInvocable
    {
        private readonly IAppCachingService _appCachingService;

        public ClearDashboardCacheInvocable(IAppCachingService appCachingService)
        {
            _appCachingService = appCachingService;
        }

        public async Task Invoke()
        {
            await _appCachingService.RemoveAllMetrics();
        }
    }
}