Title: Switching Google Authentication to OIDC in ASP.NET Core 2.2
<!-- Published: 1/30/2019 -->
Image: /images/home-bg.jpg
Tags: 
    - Google+
    - ASP.NET Core 2.2
    - Authentication
    - OpenID Connect
---

Google has announced to shutdown the Google+ API due to previous data leaks. They have [announced](https://developers.google.com/+/api-shutdown) that the will shutdown the Google+ APIs on March 7, 2019. This is pretty soon and I have an application in production that uses the Google+ API for authentication. In the post by Google it even says that the shutdown is beginning at the end of January 2019 and that API calls might fail. That is just a few days from now. So I better hurry and start updating that project.

## What to do next?

Since I am using the simple ASP.NET Core extension method provided by Microsoft, I thought the easiest way to would be to see if there is an open issue or pull request in their repository. This is one of the aspects why Open Source Software development is great :-)

After searching the GitHub repository I found the following [pull request](https://github.com/aspnet/AspNetCore/pull/6338) that pretty much describes the problem and provides the solution as well.

## What I need to change

Until now I have been using the `AddGoogle()` extension method provided in the `Microsoft.AspNetCore.Authentication.Google`.

```chsharp
var clientId = configuration["Authentication:Google:ClientId"];
var clientSecret = configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
    });
}
```

Since the [pull request](https://github.com/aspnet/AspNetCore/pull/6338) has not yet been merged I will need to use a different approach. The next option I have is to use the OIDC provider to connect to Google. In the long term this might even be better, because I will be using the generic OIDC Provider.

### Switch to the OpenID Connect Provider to Authenticate with Google

At first I thought this might be more work but in the end it only took me about 5 minutes to implement the change.

First I had to install the `Microsoft.AspNetCore.Authentication.OpenIdConnect` NuGet package to the project. Next I had to remove the snippet above with the built-in Google provider and replace it with the `AddOpenIdConnect()` extension method.

```csharp
var authBuilder = services.AddAuthentication();

var clientId = configuration["Authentication:Google:ClientId"];
if (!string.IsNullOrEmpty(clientId))
{
    authBuilder.AddOpenIdConnect(
        authenticationScheme: "Google",
        displayName: "Google",
        options =>
        {
            options.Authority = "https://accounts.google.com/";
            options.ClientId = clientId;

            // Change the callback path to match the google app configuration
            options.CallbackPath = "/signin-google";

            // Add email scope
            options.Scope.Add("email");
        });
}
```

Changing the callback paths is not required but in my case it was easier than changing the redirect paths in the Google App settings.

```csharp
options.CallbackPath = "/signin-google";
```

The default configuration includes the `openid` and `profile` scopes but I also required the `email` address of the user that is signing up. So I added the required scope to the options.

```csharp
options.Scope.Add("email");
```

Since the default implementation uses the implicit flow I did not have to set the `ClientSecret` because an `id_token` is provided to the `redirect_uri` and there is no need to call an API.

Thats all I had to do to switch from the built-in Google provider to use the generic OIDC provider.