# OWIN HealthCheck Middleware [![Build status](https://ci.appveyor.com/api/projects/status/0gyk1mdissqs29lj?svg=true)](https://ci.appveyor.com/project/thedillonb/owin-healthcheck)

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

## Nuget

Nuget packaged for your pleasure

```
Install-Package Owin.HealthCheck
```

## License 

The MIT License (MIT)

Copyright (c) 2015 Dillon Buchanan

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
