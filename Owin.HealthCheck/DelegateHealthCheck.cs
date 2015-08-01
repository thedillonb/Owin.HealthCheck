using System;
using System.Threading.Tasks;

namespace Owin.HealthCheck
{
    public class DelegateHealthCheck : BaseHealthCheck
    {
        private readonly Func<Task<HealthCheckStatus>> _delegate;

        public DelegateHealthCheck(string name, Func<Task<HealthCheckStatus>> @delegate)
            : base(name)
        {
            if (@delegate == null)
                throw new ArgumentNullException(nameof(@delegate));
            _delegate = @delegate;
        }

        public override Task<HealthCheckStatus> Check() => _delegate();
    }
}
