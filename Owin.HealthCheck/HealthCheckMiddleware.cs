using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Owin.HealthCheck
{
    public class HealthCheckMiddleware : OwinMiddleware
    {
        private readonly IHealthCheck[] _healthChecks;
        private readonly TimeSpan _timeout;

        public HealthCheckMiddleware(OwinMiddleware next, IEnumerable<IHealthCheck> healthChecks, TimeSpan timeout)
            : base(next)
        {
            _healthChecks = (healthChecks ?? Enumerable.Empty<IHealthCheck>()).ToArray();
            _timeout = timeout == TimeSpan.Zero ? TimeSpan.FromSeconds(30) : timeout;
        }

        public HealthCheckMiddleware(OwinMiddleware next, HealthCheckMiddlewareConfig config)
            : this(next, config.HealthChecks, config.Timeout)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var queryParameters = context.Request.GetQueryParameters();
            var debug = false;
            if (queryParameters.ContainsKey("debug"))
                bool.TryParse(queryParameters["debug"], out debug);

            var checkTasks = _healthChecks.Select(async x =>
            {
                try
                {
                    return new
                    {
                        Name = x.Name,
                        Status = await x.Check().ConfigureAwait(false)
                    };
                }
                catch (Exception e)
                {
                    return new
                    {
                        Name = x.Name,
                        Status = HealthCheckStatus.Failed(e.Message)
                    };
                }
            });

            var allTasks = Task.WhenAll(checkTasks.ToArray());
            if (allTasks != await Task.WhenAny(allTasks, Task.Delay(_timeout)).ConfigureAwait(false))
            {
                context.Response.StatusCode = (int)HttpStatusCode.GatewayTimeout;
            }
            else
            {
                var hasFailed = checkTasks.Select(x => x.Result).Any(x => x.Status.HasFailed);
                context.Response.StatusCode = hasFailed ? (int)HttpStatusCode.InternalServerError : (int)HttpStatusCode.OK;
                if (debug)
                {
                    var sb = new StringBuilder();
                    foreach (var r in allTasks.Result)
                        sb.AppendLine(r.Name + ": " + r.Status.Message);
                    await context.Response.WriteAsync(sb.ToString());
                }
            }
        }
    }

    public sealed class HealthCheckMiddlewareConfig
    {
        public TimeSpan Timeout { get; set; }

        public IList<IHealthCheck> HealthChecks { get; set; }
    }

    public static class AppBuilderExtensions
    {
        public static IAppBuilder UseHealthCheck(this IAppBuilder appBuilder, string route, HealthCheckMiddlewareConfig config)
            => appBuilder.Map(route, x => x.UseHealthCheck(config));

        public static IAppBuilder UseHealthCheck(this IAppBuilder appBuilder, HealthCheckMiddlewareConfig config)
            => appBuilder.Use<HealthCheckMiddleware>(config);
    }
}
