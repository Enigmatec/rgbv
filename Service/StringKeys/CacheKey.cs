using Service.Enums;

namespace Service.StringKeys
{
    public class CacheKey
    {
        public const string StateList = "StateList4550213";

        public static string ComposeDashBoardCacheKey(DashboardKeys key) => key + "Dashboard";

        public const string DashboardRequestsListKey = "DashboardRequestsListKey";
    }
}