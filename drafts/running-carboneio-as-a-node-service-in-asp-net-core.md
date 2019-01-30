Title: Calling JavaScript from ASP.NET Core 2.2
Lead: This enables you to call your favorite NPM package from your ASP.NET Core backend
Published: 1/30/2019 07:00:00
Tags:
    - ASP.NET Core 2.2
    - carbone.io
    - NodeServices
---

In this blog post I want to share how I got the [Carbone.io](https://carbone.io) engine running in an ASP.NET Core 2.2 Web App. I was looking for a simple way to generate Word documents from templates. There are libraries for .Net Core like [Aspose](https://aspose.com) but they are very heavy and the licenses are expensive. I then came across [Carbone.io](https://carbone.io) which offer exactly what I was looking for but as a NPM package, or as a standalone server. I could just host the server and use the REST Api, but I wanted to integrate it into the application.

## How to integrate a node service in an ASP.NET Core application

First we have to add a package reference to `Microsoft.AspNetCore.NodeServices`. We need the `NodeServices` package to get all the required dependencies for calling into JavaScript in our backend. By using this we most of the npm packages out there.

```xml
<ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.All" />
    <PackageReference Include="Microsoft.AspNetCore.NodeServices" Version="2.2.0" />
</ItemGroup>
```

Next we have to register the `NodeServices` with the DI container. This can be done in the `ConfigureServices()`

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

Create a folder where all your server side JavaScript code will go. Then add a JavaScript file, for my example I used `./Node/carbone.js`. Inside this JavaScript file we export the methods that we want to be able to call from the C# code.

```JavaScript
var require = require('carbone');

module.exports = {
    create: function(callback, data, options) {
        // TODO: ...
    }
};
```

Now inside of our `IndexModel` where we injected the `INodeServies` through the constructor we can call the exported method `create`:

```csharp
public async Task<IActionResult> CreateInvoice(int id)
{
    var invoice = await _invoiceService.Get(id);

    var result = await _nodeServices...
}
```

## Summary

// TODO: Sum up what we have achieved here