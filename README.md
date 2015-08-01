# OWIN HealthCheck Middleware

This library provides a collection of health check mechanisms for the OWIN pipeline. It is recommended that 
HTTP server applications build a health check endpoint so that external resources can determine the health of the 
running application. A common usecase for health checks are load balanced applications as the load balancer should
seek to eliminate unhealthy applications from the pool of potential servers.

## Current Health Check Mechanisms

The following is a list of built-in health check mechanisms that are available for use:

- SQL
- ICMP Ping
- HTTP request
- Custom (via a c# `delegate`)

## Use

Using this middleware is simple:

```
// app is IAppBuilder

// Create an 'always successful' healthcheck
app.UseHealthCheck();

// Create an 'always successful' health check at a specific route
app.UseHealthCheck('/healthcheck');

// Create a health check endpoint with two health checks.
app.UseHealthCheck("/healthcheck", new HealthCheckMiddlewareConfig
{
    HealthChecks = new List<IHealthCheck>
    {
        new HttpHealthCheck("Google Check", new Uri("https://www.google.com")),
        new PingHealthCheck("Local Ping", "localhost", TimeSpan.FromSeconds(10))
    }
});

```

