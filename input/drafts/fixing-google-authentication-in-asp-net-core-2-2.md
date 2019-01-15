Title: Fixing Google Authentication in ASP.NET Core 2.2
<!-- Published: 1/30/2019 -->
Image: /images/home-bg.jpg
Tags: 
    - Google+
    - ASP.NET Core 2.2
    - Authentication
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

Since the [pull request](https://github.com/aspnet/AspNetCore/pull/6338) has not yet been merged I will need to use a different approach. The next option I have is to use the OIDC provider to connect to google.


