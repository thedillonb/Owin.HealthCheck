
namespace Owin.HealthCheck
{
    public sealed class HealthCheckStatus
    {
        public string Message { get; }

        public bool HasFailed { get; }

        public HealthCheckStatus(string message, bool hasFailed)
        {
            Message = message;
            HasFailed = hasFailed;
        }

        public static HealthCheckStatus Failed(string message = null)
            => new HealthCheckStatus(message ?? "Failed", true);

        public static HealthCheckStatus Passed(string message = null)
           => new HealthCheckStatus(message ?? "Success", false);
    }
}
