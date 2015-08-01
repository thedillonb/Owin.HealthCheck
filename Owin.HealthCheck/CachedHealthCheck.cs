using System;
using System.Threading.Tasks;

namespace Owin.HealthCheck
{
    public sealed class CachedHealthCheck : IHealthCheck
    {
        private readonly IHealthCheck _healthCheck;
        private readonly TimeSpan _cacheTime;
        private readonly bool _cacheFailures;
        private DateTimeOffset _lastChecked = DateTimeOffset.MinValue;
        private HealthCheckStatus _lastStatus;

        public string Name => _healthCheck.Name;

        public CachedHealthCheck(IHealthCheck healthCheck, TimeSpan cacheTime, bool cacheFailures)
        {
            _cacheFailures = cacheFailures;
            _healthCheck = healthCheck;
            _cacheTime = cacheTime;
        }

        public async Task<HealthCheckStatus> Check()
        {
            var now = DateTimeOffset.Now;
            if (_lastStatus == null || (_cacheFailures && _lastStatus.HasFailed) || _lastChecked + _cacheTime < now)
            {
                _lastStatus = await _healthCheck.Check();
                _lastChecked = now;
                return _lastStatus;
            }

            return _lastStatus;
        }
    }

    public static class HealthCheckCacheExtensions
    {
        public static IHealthCheck WithCache(this IHealthCheck healthCheck, TimeSpan cacheTime, bool cacheFailures = false)
            => new CachedHealthCheck(healthCheck, cacheTime, cacheFailures);
    }
}
