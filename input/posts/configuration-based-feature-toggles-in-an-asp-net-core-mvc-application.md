Title: Configuration based feature toggles in an ASP.NET Core MVC Application
Published: 11/30/2018
Image: /images/home-bg.jpg
Tags: 
    - ASP.NET CORE 2.1
    - Configuration
    - Feature Toggle
---

In this post I want to show a very simple way of implementing a feature toggle mechanism, that is based on the `appsettings.json` file. A feature toggle is used to disable or enable a feature during runtime without changing the code. For example toggling a certain feature in your software based on region, tenant, user or a simple boolean.

The implementation in this post will be very simple. I will configure my features in my `appsettings.json` file.

```json
{
 "Features": {
    "Messaging": "true",
    "Notifications": "false"
  }
}
```

I have two features that I can toggle in my application. One is called _Messaging_ that is set to `true`, and the other one is named _Notifications_ which is set to `false`.

Next I created the `FeatureService` that implements the `IFeatureService`. With this abstraction we can change the implementation later on, without too much effort.

```csharp
public interface IFeature
{
  bool IsFeatureEnabled(string name);
}
```

The implementation of the `FeatureService` is very simple.

```csharp
public class FeatureService : IFeatureService
{
    private readonly IConfiguration _configuration;

    public FeatureService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsFeatureEnabled(string name)
    {
        var featureValue = _configuration[$"Features:{name}"];
        if(string.IsNullOrEmpty(featureValue))
        {
        throw new FeatureException(name);
        }
        return bool.Parse(featureValue);
    }
}
```

In my `FeatureService` I am simply checking the value of the feature from the `appsettings.json` file. If there is a value with the corresponding feature name provided it will be parsed into a boolean and the value will be returned. If the feature name was not found a `FeatureException` will be thrown. This implementation can be changed depending on your requirements. For example the information could be retrieved from a configuration database, external web service or even from user claims.

Now it is time to hook this service into the application by simply registering it with the ASP.NET Core DI Container like this.

```csharp
services.AddScoped<IFeatureService, FeatureService>();
```

Now you have the possibility to use this `FeatureService` in your Controllers like this.

```csharp
public class MessagingController : Controller
{
    private readonly IFeatureService _featureService;

    public MessagingController(IFeatureService featureService)
    {
        _featureService = featureService;
    }
    
    public IActionResult Index()
    {
        if(_featureService.IsFeatureEnables())
        {
            return View();
        }

        return NotFound();
    }
}
```

In your Views you can do the feature check in a similar way. First inject the `IFeatureService` and then wrap the code you want to toggle with an `If` statement.

```csharp
@inject IFeatureService FeatureService

@if(FeatureService.IsFeaturedEnabled("Notifications"))
{
  <div class="alert">My Notifications</div>
}
```

In my opinion this is okay but I prefer to use another solution. In ASP.NET Core there is the concept of **Tag Helpers**. Writing a small tag helper that can be added directly to the HTML Tag and suppresses the output in case the feature is disabled is a much cleaner way.

Implementing this tag helper is really simple and looks like this.

```csharp
[HtmlTargetElement(Attributes = "asp-feature")]
public class FeatureTagHelper : TagHelper
{
    private readonly IFeatureService _featureService;

    [HtmlAttributeName("asp-feature-name")]
    public string Name { get; set; }

    public FeatureTagHelper(IFeatureService featureService)
    {
        _featureService = featureService;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!_featureService.IsFeatureEnabled(Name))
        {
            output.SuppressOutput();
        }
    }
}
```

After recompiling the solution, the tag helper is picked up by the intellisense, and can be applied to any HTML tag.

```html
<h1 asp-feature asp-feature-name="Notifications">Notifications are not enabled</h1>
```

This blog post showed a very simple way of implementing a feature toggle mechanism in your ASP.NET Core Application. The `IFeatureService.IsFeatureEnabled` can be extended to any wish. I am thinking of extending it to a claims based feature toggle.