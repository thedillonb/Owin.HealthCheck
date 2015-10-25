using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Owin.HealthCheck
{
    /// <summary>
    /// The health check middleware which will execute a collection of <see cref="IHealthCheck"/>
    /// </summary>
    public class HealthCheckMiddleware : OwinMiddleware
    {
        private readonly IHealthCheck[] _healthChecks;
        private readonly TimeSpan _timeout;

        public HealthCheckMiddleware(OwinMiddleware next, IEnumerable<IHealthCheck> healthChecks, TimeSpan timeout)
            : base(next)
        {
            _healthChecks = (healthChecks ?? Enumerable.Empty<IHealthCheck>()).ToArray();
            _timeout = timeout;
        }

        public HealthCheckMiddleware(OwinMiddleware next, HealthCheckMiddlewareConfig config)
            : this(next, config.HealthChecks, config.Timeout)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            // Make sure this is an absolute path.
            if (!string.IsNullOrEmpty(context.Request.Path.Value))
            {
                await Next.Invoke(context);
                return;
            }

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
                context.Response.StatusCode = hasFailed ? (int)HttpStatusCode.ServiceUnavailable : (int)HttpStatusCode.OK;

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

    /// <summary>
    /// A configuration object for <see cref="HealthCheckMiddleware"/>
    /// </summary>
    public sealed class HealthCheckMiddlewareConfig
    {
        /// <summary>
        /// The time before the healthcheck is deemed a failure. Defaults to 30 seconds
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// A list of health checks to execute
        /// </summary>
        public IList<IHealthCheck> HealthChecks { get; set; } = new List<IHealthCheck>();
    }

    /// <summary>
    /// Extension methods for <see cref="IAppBuilder"/>
    /// </summary>
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Inserts a <see cref="HealthCheckMiddleware"/> into the current <see cref="IAppBuilder"/> pipeline
        /// </summary>
        /// <param name="appBuilder">The current <see cref="IAppBuilder"/> pipeline</param>
        /// <param name="config">A <see cref="HealthCheckMiddlewareConfig"/> containing configuration options for the middleware</param>
        /// <returns>A <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseHealthCheck(this IAppBuilder appBuilder, string route, HealthCheckMiddlewareConfig config)
            => appBuilder.Map(route, x => x.Use<HealthCheckMiddleware>(config));

        /// <summary>
        /// Inserts a <see cref="HealthCheckMiddleware"/> into the current <see cref="IAppBuilder"/> pipeline
        /// </summary>
        /// <param name="appBuilder">The current <see cref="IAppBuilder"/> pipeline</param>
        /// <param name="config">A <see cref="HealthCheckMiddlewareConfig"/> containing configuration options for the middleware</param>
        /// <returns>A <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseHealthCheck(this IAppBuilder appBuilder, HealthCheckMiddlewareConfig config)
            => appBuilder.Use<HealthCheckMiddleware>(config);
    }
}
