using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owin.HealthCheck.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var app = WebApp.Start("http://localhost:3333", Configure))
            {
                Console.WriteLine("Waiting for requests on port 3333... Press any key to exit.");
                Console.ReadKey();
            }
        }

        static void Configure(IAppBuilder builder)
        {
            builder.UseHealthCheck("/healthcheck", new HealthCheckMiddlewareConfig
            {
                HealthChecks = new List<IHealthCheck>
                {
                    new HttpHealthCheck("Google Check", new Uri("https://www.google.com"))
                        .WithCache(TimeSpan.FromMinutes(1)),

                    new PingHealthCheck("Local Ping", "localhost", TimeSpan.FromSeconds(10))
                        .WithCache(TimeSpan.FromMinutes(1))
                }
            });

            builder.Run(x =>
            {
                x.Response.Redirect("/healthcheck");
                return Task.FromResult(0);
            });
        }
    }
}
