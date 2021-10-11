using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Owin.HealthCheck
{
    using System.Diagnostics;
    using System.Dynamic;

    /// <summary>
    /// The health check middleware which will execute a collection of <see cref="IHealthCheck"/>
    /// </summary>
    public class HealthCheckMiddleware : OwinMiddleware
    {
        private readonly IResponseWriter _responseWriter;
        private readonly IHealthCheck[] _healthChecks;
        private readonly TimeSpan _timeout;

        public HealthCheckMiddleware(OwinMiddleware next, IEnumerable<IHealthCheck> healthChecks, TimeSpan timeout, IResponseWriter responseWriter = null)
            : base(next)
        {
            _healthChecks = (healthChecks ?? Enumerable.Empty<IHealthCheck>()).ToArray();
            _timeout = timeout;
            _responseWriter = responseWriter ?? new SimpleResponseWriter();
        }

        public HealthCheckMiddleware(OwinMiddleware next, HealthCheckMiddlewareConfig config)
            : this(next, config.HealthChecks, config.Timeout, config.ResponseWriter)
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
                var stopwatch = new Stopwatch();
                dynamic result = new ExpandoObject();
                result.Name = x.Name;
                try
                {
                    stopwatch.Start();
                    var status = await x.Check().ConfigureAwait(false);
                    stopwatch.Stop();
                    result.Status = status;
                    result.Duration = stopwatch.Elapsed;
                }
                catch (Exception e)
                {
                    stopwatch.Stop();
                    result.Status = HealthCheckStatus.Failed(e.Message);
                    result.Duration = stopwatch.Elapsed;
                }

                return result;
            });

            var allTasks = Task.WhenAll(checkTasks.ToArray());
            if (allTasks != await Task.WhenAny(allTasks, Task.Delay(_timeout)).ConfigureAwait(false))
            {
                context.Response.StatusCode = (int)HttpStatusCode.GatewayTimeout;
            }
            else
            {
                var results = await allTasks.ConfigureAwait(false);
                var hasFailed = results.Any(x => x.Status.HasFailed);
                context.Response.StatusCode = hasFailed ? (int)HttpStatusCode.ServiceUnavailable : (int)HttpStatusCode.OK;

                if (debug)
                {
                    var result = this._responseWriter.WriteResponse(results);
                    context.Response.ContentType = this._responseWriter.ContentType;
                    await context.Response.WriteAsync(result);
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

        /// <summary>
        /// The response writer used to format the response (if debug = true).
        /// </summary>
        public IResponseWriter ResponseWriter = new SimpleResponseWriter();
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
