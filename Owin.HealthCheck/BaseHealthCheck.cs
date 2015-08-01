using System;
using System.Threading.Tasks;

namespace Owin.HealthCheck
{
    public abstract class BaseHealthCheck : IHealthCheck
    {
        public string Name { get; }

        public BaseHealthCheck(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Invalid health check name", nameof(name));

            Name = name;
        }

        public abstract Task<HealthCheckStatus> Check();
    }
}
