Title: Calling JavaScript inside an ASP.NET Core 2.2 application
Lead: This enables you to call your favorite NPM package from in ASP.NET Core backend
Published: 1/31/2019 16:45:00
Tags:
    - ASP.NET Core 2.2
    - carbone.io
    - NodeServices
    - JavaScript
---

In this blog post I want to share how I got the [Carbone.io](https://carbone.io) engine running in an ASP.NET Core 2.2 Web App. I was looking for a simple way to generate Word documents from templates. There are libraries for .Net Core like [Aspose](https://aspose.com) but they are very heavy and the licenses are expensive. I then came across [Carbone.io](https://carbone.io) which offer exactly what I was looking for but as a NPM package, or as a standalone server. I could just host the server and use the REST Api, but I wanted to integrate it into the application.

## How to integrate a node service in an ASP.NET Core application

First we have to add a package reference to `Microsoft.AspNetCore.NodeServices`. We need the `NodeServices` package to get all the required dependencies for calling into JavaScript in our backend. By using this we have the possibility to call into most of the npm packages out there.

```xml
<ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.All" />
    <PackageReference Include="Microsoft.AspNetCore.NodeServices" Version="2.2.0" />
</ItemGroup>
```

Next we have to register the `NodeServices` with the DI container. This is done in the `ConfigureServices()` method

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddNodeServices();

    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
}
```

Now we have the possibility to inject `INodeServices` in our application. The `INodeServices` is an API through which .NET code can make calls into JavaScript that will be executed in a Node environment.

```csharp
private readonly INodeServices _nodeServices;

public ReportController(INodeServices nodeServices)
{
    _nodeServices = nodeServices;
}
```

Create a folder where all your server side JavaScript code will go. Then add a JavaScript file, for my example I used `./Node/carbone.js`. Inside this JavaScript file we export the methods that we want to be able to call from the C# code. For my small POC I used the simple example template that was provided inside the `node_modules` folder. The JavaScript method call could be extended to also take in a third parameter with options such as template path or the file output name.

```JavaScript
const carbone = require('carbone');

module.exports = {
    create: function(callback, data) {
        var data = JSON.parse(dataIn);
        carbone.render('./node_modules/carbone/examples/simple.odt', data, function (err, result) {
        if (err) {
            callback(err, null);
        }
        callback(null, result);
    }
};
```

Now inside of our `ReportController` where we injected the `INodeServices` through the constructor we can call the exported method `create`:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([FromForm]string firstname, [FromForm]string lastname)
{
    var result = await _nodeServices.InvokeExportAsync<Document>(
                        "./node/carbone.js", // Path to our JavaScript file
                        "create", // Exported function name
                        JsonConvert.SerializeObject(new { firstname, lastname })); // Arguments, in this case a json string

    return File(result.data, "application/vnd.oasis.opendocument.text");
}
```

Then I added a form to my `Index.cshtml` to enable passing in the `firstname` and `lastname` and a button to submit the form to the `ReportController`.

```html
<form asp-controller="Report" asp-action="Create" method="post">
    <div class="form-group">
        <input class="form-control" name="firstname"placeholder="Firstname" autocomplete="false" />
    </div>
    <div class="form-group">
        <input class="form-control" name="lastname" placeholder="Lastname" autocomplete="false" />
    </div>
    <button class="btn btn-primary" type="submit">Create Report</button>
</form>
```

Now when I run this application and navigate to the start page, I can enter a first- and lastname and click `Create Report`. This will call into the JavaScript world, render the `*.odt` template and download the rendered file.

![gif](/posts/images/carbone.gif)

## Summary

In this blog post I tried to show two things. First I showed the possibility to call into JavaScript and therefor the possibility to call your favorite NPM package which is awesome, but you also have to be careful and make sure you know what you are calling. The second thing I wanted to show with this blog post was how easy you can use word templates and then render them by using the [Carbone.io](https://carbone.io) reporting engine.

There are many possibilities to extend this POC by adding the template path as a parameter and adding other options. I still have to test out the conversion to PDF and other formats  but that should not be an issue. Just have to make sure that LibreOffice is installed because that is used for the conversion.

If you like this blog post drop a comment or buy me a coffee at the bottom of the page...

Source Code is available on my [GitHub](https://github.com/Franklin89/AspNetCoreNodeServices)