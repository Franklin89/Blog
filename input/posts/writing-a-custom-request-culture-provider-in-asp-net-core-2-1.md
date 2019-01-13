Title: Writing a custom request culture provider in ASP.NET Core 2.1
Published: 8/23/2018
Image: /images/home-bg.jpg
Tags: 
    - ASP.NET Core 2.1
    - Web
    - Localization
RedirectFrom: blog/writing-a-custom-request-culture-provider-in-asp-net-core-2-1
---

ASP.NET Core already delivers a handful of request culture providers. But what if you do not want to use one of the default request culture provider like: `CookieRequestCultureProvider`. With just a little bit of code it is possible to implement your custom way of setting the request culture. We had the requirement that a user can store his desired culture in his profile and so we had to create a custom culture provider.

But first we have to set up localization which is really simple in ASP.NET Core.

#### Step 1
Localized strings can be partitioned by controller, area or you can have one single container. Out of simplicity we will use one container. Create a dummy class in the root of your project called `SharedResource.cs`.

```csharp
namespace MLSoftware.Localization
{
    public class SharedResource
    {
    }
}
```

#### Step 2
Next we will have to configure localization inside of our `Startup.cs` class. Add the following lines to your `ConfigureServices` method.

```csharp
services.AddLocalization(options => options.ResourcesPath = "Resources"); // Adds the localization services and sets the resources path to "Resources"

var supportedCultures = new[]
{
    new CultureInfo("en-US"),
    new CultureInfo("de-CH"),
};

services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(culture: "de-CH", uiCulture: "de-CH");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

services.AddMvc()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();
```

#### Step 3
We have to configure the ASP.NET Core middleware to use the request localization. Add the following lines inside the `Configure` method.

```csharp
app.UseRequestLocalization();
```

Done. Request localization has now been set up. Resource (.resx) files have to be added to the project under */Resources/SharedResource.[language].resx*. From now on, on every request the list of culture providers that are configured will be enumerated and the first provider that can successfully resolve the culture will be used.

### Creating the custom request culture provider
Microsoft provides three default culture providers that are automatically configured.

1. `QueryStringRequestCultureProvider`
1. `CookieRequestCultureProvider`
1. `AcceptLanguageHeaderRequestCultureProvider`

If you want to create a custom culture provider it is best to clear the list of culture providers and then add your custom culture provider. If required add the default ones back to the list. Update your `RequestLocalizationOptions` to the following:

```csharp
services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(culture: "de-CH", uiCulture: "de-CH");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
		options.RequestCultureProviders.Clear(); // Clears all the default culture providers from the list
    options.RequestCultureProviders.Add(new UserProfileRequestCultureProvider()); // Add your custom culture provider back to the list
});
```

Now it is time for the last piece of code. The custom culture provider. This is a simple class that inherits from `RequestCultureProvider` and needs to implement one single method called `DetermineProviderCultureResult`. Inside this method you can do everything you need to resolve the culture for the request. **Remember: This will get called on every request to your server, so don't put any heavy compute calls in here.**

```csharp
public class UserProfileRequestCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
    {
        if (httpContext == null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        if (!httpContext.User.Identity.IsAuthenticated)
        {
            return Task.FromResult((ProviderCultureResult)null);
        }

        var culture = httpContext.User.GetCulture();
        if (culture == null)
        {
            return Task.FromResult((ProviderCultureResult)null);
        }

        return Task.FromResult(new ProviderCultureResult(culture));
    }
}
```