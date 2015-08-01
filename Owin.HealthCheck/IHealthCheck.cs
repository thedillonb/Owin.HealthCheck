using System.Threading.Tasks;

namespace Owin.HealthCheck
{
    public interface IHealthCheck
    {
        string Name { get; }

        Task<HealthCheckStatus> Check();
    }
}
