Title: Writing to Azure Diagnostics Log Stream using Serilog
Published: 9/10/2018
Image: /images/home-bg.jpg
Tags: 
    - Azure
    - Serilog
    - Diagnostics
---

Inside the Azure Portal it is possible to stream your log from an Azure App Service directly to a console, which is really handy in my opinion if you need to quickly debug something and you want to see in "real time" what is being written to your log. But first we have to turn on the feature.

On your App Service Blade go to `Monitoring -> Diagnostics Log`

![image](/posts/images/Monitoring.png)
 
Enable the `Application Logging (Filesystem)` *The good thing about this feature is, that it will automatically turn off after 12 hours.*

![image](/posts/images/ApplicationLogging.png)

Now we have to get our log output from our ASP.NET Core application onto the Filesystem so that the `Log stream` can pick it up and be displayed. The easiest way to write the log output to differnet providers is by using [Serilog](https://serilog.net/) and attaching the different sinks that it offers. One of them being a File sink which fits perfect for us here. (If you have never used Serilog before, go check it out [here](https://serilog.net/))

**Start with,** installing the required Serilog packages from Nuget:
```
dotnet add package Serilog --verison 2.7.1
dotnet add package Serilog.AspNetCore --version 2.1.1
dotnet add package Serilog.Sinks.File --version 4.0.0
```

**Next,** update your applications `Program.cs` and add `UseSerilog`.

```csharp
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog();
```

**Then,** update your `Startup.cs` where you will configure Serilog. Add the following to your `ConfigureServices` method. This will configure Serilog and the Serilog File Sink. The File Sink will write to `D:\home\LogFiles\Application\...` where the Azure Diagnostics Log Stream will pick up events written to any file located there.

```csharp
var logConfiguration = new LoggerConfiguration()
  .MinimumLevel.Debug()
  .Enrich.FromLogContext();

  if (Environment.IsProduction())
  {
    logConfiguration.WriteTo.File(
      $@"D:\home\LogFiles\Application\{Environment.ApplicationName}.txt",
      fileSizeLimitBytes: 1_000_000,
      rollOnFileSizeLimit: true,
      shared: true,
      flushToDiskInterval: TimeSpan.FromSeconds(1));
  }

  Log.Logger = logConfiguration.CreateLogger();
```

Deploy the application to Azure and you will see the log messages appear inside the Log Stream output.

![image](/posts/images/LogStream.png)
