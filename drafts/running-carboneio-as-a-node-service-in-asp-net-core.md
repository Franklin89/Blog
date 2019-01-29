Title: Running carbone.io as a node service in an ASP.NET Core Application
Tags:
    - ASP.NET Core 2.2
    - carbone.io
    - NodeServices
---

In this blog post I want to share how I got the [Carbone.io](https://carbone.io) engine running in an ASP.NET Core 2.2 Web App. 
// TODO: Short explanation of what carbone is and what it can be used for

## How to integrate a node service in an ASP.NET Core application

First we have to add a package reference to `Microsoft.AspNetCore.NodeServices`. We need the `NodeServices` package to get all the required dependencies for calling into JavaScript in our backend. By using this we most of the npm packages out there.

```xml
<ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.All" />
    <PackageReference Include="Microsoft.AspNetCore.NodeServices" Version="2.2.0" />
</ItemGroup>
```

Next we have to register the Node Services with the DI container. This can be done in the `ConfigureServices()`

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddNodeServices();

    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
}
```

Now we have the possibility to inject an `INodeServices` in our application. The `INodeServices` is an API through which .NET code can make calls into JavaScript that runs in a Node environment.

```csharp
private readonly INodeServices _nodeServices;

public IndexModel(INodeServices nodeServices)
{
    _nodeServices = nodeServices;
}
```

We need to create a JavaScript module and export a function that we can then call from our C# code.

//TODO: Show module implementation

## Summary

// TODO: Sum up what we have achieved here