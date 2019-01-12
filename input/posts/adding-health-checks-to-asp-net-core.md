Title: Adding health checks to ASP.NET Core
Lead: Health checks are now a feature of ASP.NET Core 2.2
Published: 8/29/2018
Image: /images/home-bg.jpg
Tags: 
    - ASP.NET Core 2.2
    - Health Checks
---

Before ASP.NET Core 2.2 (which is in *preview 1* and can be found [here](https://www.microsoft.com/net/download/dotnet-core/2.2) there was no built in feature for configuring health checks on your web application or API. Health checks or lets say having the possibility to offer an endpoint that shows the state of the application can be very useful. For example for load balancers to check if the application is running as expected. Azure Traffic Manager also supports such a feature where you can hook it up to a health check endpoint of your application and it will automatically unregister the endpoint if it is no longer healthy.

### How I implemented HealthChecks until now

The first time I had to implement this I simply created a controller with an action that looked like this:

```cs
[HttpGet("/hc")]
public async Task<IActionResult> HealthCheck()
{
  if(!IsApplicationHealthy())
  {
    return ServiceUnavailableObjectResult("unhealthy"); 
  }
	
  return Ok("healthy");
}
```

If the method `IsApplicationHealthy` returned false a status code `503 Service Unavailable` was returned, else if the method returned true a status code `200 Ok` would be returned. This is really simple but there is also the option of moving this code out of the controller into a middleware which makes it more reusable.

##### Refactor into Middleware

```cs
public class HealthCheckMiddleware
{
  private readonly RequestDelegate _next;
  private readonly string _path;
	
  public HealthCheckMiddleware (RequestDelegate next, string path)
  {
    _next = next;
    _path = path;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    // Check if the requested path matches the configured endpoint path
    if(context.Request.Path.Value.ToLower() == _path.ToLower())
    {
      if(IsApplicationHealthy())
      {
        context.Response.StatusCode = 200;
        context.Response.ContentLength = 7;
        await context.Response.WriteAsync("healthy");
      }
      else
      {
        context.Response.StatusCode = 503;
        context.Response.ContentLength = 9;
        await context.Response.WriteAsync("unhealthy");
      }
    }
    else
    {
      await _next(context);
    }
  }
}


public static class HealthCheckMiddlewareExtensions()
{
  public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder builder, string path)
  {
    return builder.UseMiddleware<HealthCheckMiddleware>(path);
  }
}
```

Now add this middleware to the pipeline in the `Configure` method of the `Startup` class.

```cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHealthCheck("/status");

    app.Run(async (context) =>
    {
        await context.Response.WriteAsync("Hello World!");
    });
}
```

### Health checks in ASP.NET Core 2.2

In ASP.NET Core 2.2 there is built in support for health checks. The implementation is similar to the middleware approach above. The code can be found in the [ASP.NET Core Diagnostics Repository](https://github.com/aspnet/Diagnostics). 

#### Register health checks

First we have to register our health checks inside the `ConfigureServices` method. Register all the required services by calling `AddHealthChecks()` which will return an `IHealthChecksBuilder` where we can add our checks by calling `AddCheck(...)`.

```cs
public void ConfigureServices(IServiceCollection services)
{
  services.AddHealthChecks()
          .AddCheck("application", () => Task.FromResult(HealthCheckResult.Healthy()));
}
```

The checks will be executed inside the `HealthCheckMiddleware` so we will have to make sure the middleware is being called in our execution pipeline.

```cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
  app.UseHealthChecks("/status");

  app.Run(async (context) =>
  {
    await context.Response.WriteAsync("Go to /status to see the health status");
  });
}
```

Now you can run the application and navigate to `/status` and should see that the application is healthy. By modifying the `AddCheck` to `HealCheckResult.Unhealthy()` you can simulate an unhealthy system. 

### Summary
This is a great and simple feature that has been long awaited for. In todays distributed systems this a must have feature. Until now most applications created a controller with an endpoint to do these checks. But with the feature that Microsoft implemented this is no longer required. Also what I think is a great extension is to have the possibility to filter health checks and run the more expensive health checks on a different url than the quick ones. For example a liveness endpoint that only checks if the system is reachable and a readiness endpoint that does some more checks. [This](https://github.com/aspnet/Diagnostics/blob/release%2F2.2/samples/HealthChecksSample/LivenessProbeStartup.cs) sample shows how it could be done. 

In my opinion there are great samples in the [ASP.NET Core Diagnostics Repository](https://github.com/aspnet/Diagnostics). With some great extensions like `SqlConnectionHealthCheck` that can be adopted into your library for reuse.