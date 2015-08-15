using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Owin.HealthCheck
{
    public class PingHealthCheck : BaseHealthCheck
    {
        private readonly string _host;
        private readonly TimeSpan _timeout;

        public PingHealthCheck(string name, string host, TimeSpan timeout)
            : base(name)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentException("host cannot be empty", nameof(host));
            _host = host;
            _timeout = timeout;
        }

        public override async Task<HealthCheckStatus> Check()
        {
            var ping = new Ping();
            var result = await ping.SendPingAsync(_host, (int)_timeout.TotalMilliseconds).ConfigureAwait(false);
            return new HealthCheckStatus(result.Status.ToString(), result.Status != IPStatus.Success);
        }
    }
}
