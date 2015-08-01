using System.Threading.Tasks;
using Xunit;

namespace Owin.HealthCheck.UnitTests
{
    class DelegatedHealthCheckTests
    {
        [Fact]
        public void TestDelegation()
        {
            var healthCheck = new DelegateHealthCheck("Test", () => Task.FromResult(HealthCheckStatus.Passed()));
            Assert.Equal("Test", healthCheck.Name);

            var result = healthCheck.Check().Result;
            Assert.False(result.HasFailed);
        }
    }
}
