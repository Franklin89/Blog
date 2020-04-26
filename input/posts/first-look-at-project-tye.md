Title: First look at Project Tye
Published: 04/26/2020 16:00
Tags: 
    - ASP.NET Core 3.1
    - Project Tye
    - Tye
    - Microservices
---

I am currently working on a project where I have to run multiple projects at the same time, even though I am only really working on one of them. So, you could say it is a microservice architecture and you can argue about why they are so dependent on each other. But that is a whole different story.

On the customers virtual development machine I have no chance to install docker to run all the projects with a single command. This would have been perfect, in the end the projects will be run in Kubernetes anyways. Just a few days back I was thinking of creating some script that will start all the applications for me, but then I remembered the [ASP.NET Community Standup](https://www.youtube.com/watch?v=yzNEHrWM78o&list=PL1rZQsJPBU2St9-Mz1Kaa7rofciyrwWVx&index=8) where they presented a new experimental project called `Project Tye`.

## What is Project Tye?

With Project Tye, Microsoft is building a tool that provides a local orchestrator with a focus on .NET Projects. You can run your local projects as well as spin up container instances and other dependencies. At the end of your development process you can also deploy your application to a Kubernetes instance with tye. But I did not look into this, if you are interested in this, watch the [ASP.NET Community Standup](https://www.youtube.com/watch?v=yzNEHrWM78o&list=PL1rZQsJPBU2St9-Mz1Kaa7rofciyrwWVx&index=8).

> Tye is a tool that makes developing, testing, and deploying microservices and distributed applications easier. Project Tye includes a local orchestrator to make developing microservices easier and the ability to deploy microservices to Kubernetes with minimal configuration.

What I like about Project Tye is that it has a very small footprint and it is so simple to get started with. It is possible to use `Tye` without having a docker installation. Of course it will not be possible to utilize all the features but: it works.

## Getting Started

We can install Project Tye as a global .NET tool. While writing this blog post the current version was: `0.1.0-alpha.20209.5`. The project is still at the very beginning and is an open source experiment. Be aware, as this is an **experiment** it could be discontinued at anytime or major breaking changes could occur.

> You need to have .NET Core 3.1 installed on your machine.

Run the install command:

```ps
> dotnet tool install -g Microsoft.Tye --version 0.1.0-alpha.20209.5
```

To verify that Tye was installed properly you can run the following command that should output the current version installed:

```ps
> tye --version
```

## Next steps

After Tye is successfully installed on your machine you can begin with creating a new configuration file.

This is as simple as calling:

```ps
> tye init
```

Which will create a `tye.yaml` file at the current location. (Hint: if you are in a folder with a `*.sln` or `*.csproj` the command will automatically add the projects to the services list for you)

```yaml
# tye application configuration file
# read all about it at https://github.com/dotnet/tye
#
# when you've given us a try, we'd love to know what you think:
#    https://aka.ms/AA7q20u
#
# define global settings here
# name: exampleapp # application name
# registry: exampleuser # dockerhub username or container registry hostname

# define multiple services here
services:
- name: myservice
  # project: app.csproj # msbuild project path (relative to this file)
  # executable: app.exe # path to an executable (relative to this file)
  # args: --arg1=3 # arguments to pass to the process
  # replicas: 5 # number of times to launch the application
  # env: # array of environment variables
  #  - name: key
  #    value: value
  # bindings: # optional array of bindings (ports, connection strings)
    # - port: 8080 # number port of the binding
```

This file looks pretty similar to a docker compose file, it also works in a similar way. You define your services and your external dependencies. These can be docker images like Redis or Microsoft SQL Server.

In the below example I have a .NET Core web application that requires a Microsoft SQL Database.

```yaml
name: tyetest
services:
- name: tyetest
  project: TyeTest.csproj
- name: mssql
  image: mcr.microsoft.com/mssql/server:2017-latest
  env:
  - name: SA_PASSWORD
    value: "YourStrong@Passw0rd"
  - name: ACCEPT_EULA
    value: "Y"
  bindings:
  - port: 1433
    connectionString: Server=${host}:${port};Database=TyeTest;MultipleActiveResultSets=true;User Id=sa;Password=${env:SA_PASSWORD}
```

We can then build the project:

```ps
> tye build
```

And we will get the following output:

```ps
Processing Service 'tyetest'...
    Compiling Services...
    Publishing Project...
    Building Docker Image...
        Created Docker Image: 'tyetest:1.0.0'
Processing Service 'mssql'...
    Compiling Services...
    Publishing Project...
        Service 'mssql' does not have a project associated. Skipping.
    Building Docker Image...
        Service 'mssql' does not have a project associated. Skipping.
```

Next we can spin up the application by calling:

```ps
> tye run
```

We will get the following console output:

```ps
Loading Application Details...
Launching Tye Host...

[12:20:08 INF] Executing application from C:\dev\TyeTest\tye.yaml
[12:20:08 INF] Dashboard running on http://127.0.0.1:8000
[12:20:08 INF] Docker image mcr.microsoft.com/mssql/server:2017-latest already installed
[12:20:08 INF] Creating docker network tye_network_8e7653e7-a
[12:20:08 INF] Running docker command network create --driver bridge tye_network_8e7653e7-a
[12:20:09 INF] Running image mcr.microsoft.com/mssql/server:2017-latest for mssql_ebadb2f8-a
[12:20:09 INF] Running image mcr.microsoft.com/dotnet/core/sdk:3.1 for tyetest-proxy_e8d897d4-4
[12:20:09 INF] Building project C:\dev\TyeTest\TyeTest.csproj
[12:20:10 INF] Running container tyetest-proxy_e8d897d4-4 with ID 3b08d1d8e073
[12:20:10 INF] Running docker command network connect tye_network_8e7653e7-a tyetest-proxy_e8d897d4-4 --alias tyetest
[12:20:10 INF] Running container mssql_ebadb2f8-a with ID 9e9054e93f5e
[12:20:10 INF] Running docker command network connect tye_network_8e7653e7-a mssql_ebadb2f8-a --alias mssql
[12:20:10 INF] Collecting docker logs for tyetest-proxy_e8d897d4-4.
[12:20:10 INF] Collecting docker logs for mssql_ebadb2f8-a.
[12:20:11 INF] Launching service tyetest_dedb2c6f-1: C:\dev\TyeTest\bin\Debug\netcoreapp5.0\TyeTest.exe
[12:20:11 INF] tyetest_dedb2c6f-1 running on process id 34304 bound to http://localhost:57735, https://localhost:57736
[12:20:11 INF] Listening for event pipe events for tyetest_dedb2c6f-1 on process id 34304
```

In the background `tye` created a new local folder called `.tye` with two files `docker_store` and `process_store`. This is only a temporary folder and the files hold the information about the current docker containers that it started and the process identifiers. So nothing to check into your codebase.

At the start of the console output, tye outputs a line with the URL to the dashboard. If we navigate to the dashboard in a browser, we get a nice looking dashboard that lists all our projects and containers.

From the dashboard we can also look at the logs of our application which is really helpful.

![image](/posts/images/LogOutput.png)

## Service Discovery

In any distributed system service discovery is an essential part of the system. If URLs or connection strings are hard coded it makes it difficult to scale and also to deploy to different infrastructures. Inside of our `tye.yaml` where we registered our services we also specified a name for them. By setting an explicit name we have the possibility to ask the the .NET Core Configuration system for the relevant metadata.

### Accessing connection strings

```csharp
// Get the connection string of the 'mssql' service.
var connectionString = Configuration.GetConnectionString("mssql");
services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
```

### Accessing URIs for other services

```csharp
var uri = Configuration.GetServiceUri("<SERVICE_NAME>");
var httpClient = new HttpClient()
{
    BaseAddress = uri
};
```
To access the `GetServiceUri` extension method add the package with the following command:

```ps
> dotnet add package Microsoft.Tye.Extensions.Configuration --version  0.1.0-alpha.20209.5
```

Tye does nothing magical here. Behind the curtains it is creating environment variables that you can also retrieve yourself, without the extension package. A lot more information can be found in the docs [here](https://github.com/dotnet/tye/blob/master/docs/reference/service_discovery.md).

> This makes it really great for local development also if we do not deploy to Kubernetes, because the service discovery is implemented with the default configuration system. If you deploy to App Service in Azure your application will work, you will just need to set the correct AppSettings.

## Debugging

In my opinion debugging is one of the most important things to get working from the start when building a developer tool like this. I still need to put some more work into this to see if there is an easier way. But it is already simple to get debugging working.

To run our application in the `debug` mode, we need to pass some flags to the tye command:

```ps
> tye run --debug *
```

Inside of Visual Studio go to: `Debug > Attach to Process...` and search for the process to attach to.

![image](/posts/images/AttachToProcess.png)

As soon as the debugger is attached tye will continue executing your application and you will hit your breakpoints and have the default debugging experience.

![image](/posts/images/Debugging.png)

At the moment I did not find a solution to get it working with the `launchSettings.json` and simply hitting F5, but I am sure there will be a solution for this in the future.

## Summary

In this blog post I tried to show how easy it is to get started with [Project Tye](https://github.com/dotnet/tye). How to run a simple web application and adding an external dependency like Microsoft SQL Server to the configuration, accessing the connection string and debugging it. Coming back to the introduction of this post, I used Project Tye for local development of services that will end up running in Kubernetes. But because I could not use Docker on my development machine due to company policies this was a simple way to start all services and dependencies with one single command and not much extra effort, as long as I didn't need any dependent images that should be running as containers this works great.

If you like this blog post drop a comment or buy me a coffee at the bottom of the page <i class="fa fa-coffee"></i>

