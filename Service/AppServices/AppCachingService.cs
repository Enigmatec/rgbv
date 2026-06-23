using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Service.Enums;
using Service.Models.ViewModels;
using Service.StringKeys;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Service.AppServices
{
    public interface IAppCachingService
    {
        string ComposeMetricsCacheKey<TModel>(DashboardKeys dashboard, TModel model, string userRole = null, int? userOrganisationId = null, int? userStateId = null);
        string ComposeMetricsCacheKey<TModel>(TModel model, string userId,DashboardDetails dashboardDetails);
        Task<(bool status, string message, TData data)> GetMetrics<TData>(string key);

        Task<(bool status, string message)> AddMetrics<TData>(string key, TData data);

        Task<(bool status, string message)> RemoveAllMetrics();
    }

    public class AppCachingService : IAppCachingService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IConfiguration _configuration;
        private readonly IRedisCacheClient _redisCacheClient;

        public AppCachingService(IDistributedCache distributedCache, IConfiguration configuration, IRedisCacheClient redisCacheClient)
        {
            _distributedCache = distributedCache;
            _configuration = configuration;
            _redisCacheClient = redisCacheClient;
        }

        public string ComposeMetricsCacheKey<TModel>(DashboardKeys dashboard, TModel model, string userRole = null, int? userOrganisationId = null, int? userStateId = null)
        {
            var alphaNumericOnly = new Regex("[^a-zA-Z0-9 -]");

            var modelString = alphaNumericOnly.Replace(JsonConvert.SerializeObject(model).Trim(), "");

            var key = $"{dashboard}_{userRole}_{userOrganisationId}_{userStateId}_{modelString}";

            return key.GetHashCode().ToString();
        }

        public string ComposeMetricsCacheKey<TModel>(TModel model, string userId, DashboardDetails dashboardDetails)
        {
            var alphaNumericOnly = new Regex("[^a-zA-Z0-9 -]");

            var modelString = alphaNumericOnly.Replace(JsonConvert.SerializeObject(model).Trim(), "");

            var key = $"{userId}_{dashboardDetails}_{modelString}";

            return key.GetHashCode().ToString();
        }

        public async Task<(bool status, string message, TData data)> GetMetrics<TData>(string key)
        {
            try
            {
                var cacheValue = await _distributedCache.GetStringAsync(key);

                return string.IsNullOrWhiteSpace(cacheValue) ? (false, "Item not found in cache", default) : (true, "Success", JsonConvert.DeserializeObject<TData>(cacheValue));
            }
            catch (Exception ex)
            {
                return (false, ex.Message, default);
            }
        }

        public async Task<(bool status, string message)> AddMetrics<TData>(string key, TData data)
        {
            try
            {
                await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(data), new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddDays(10),
                    SlidingExpiration = TimeSpan.FromDays(10),
                });

                return (true, "Success");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool status, string message)> RemoveAllMetrics()
        {
            try
            {
                var allKeys = (await _redisCacheClient.GetDbFromConfiguration().SearchKeysAsync(_configuration.GetValue<bool>(StartupKeys.IsLive) ? "live_*" : "test_*")).ToList();

                await _redisCacheClient.GetDbFromConfiguration().RemoveAllAsync(allKeys);

                return (true, "Success");
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
        }
    }
}