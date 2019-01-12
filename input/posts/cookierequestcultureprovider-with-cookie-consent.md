Title: CookieRequestCultureProvider with GDPR Cookie Consent
Published: 8/30/2018
Image: /images/home-bg.jpg
Tags: 
    - GDPR
    - ASP.NET Core 2.1
---

A few times I came across this small issue that can be very time consuming because it is not the first thing you think of why something isn't working. Since GDPR and the cookie policy feature came in ASP.NET Core 2.1, the `CookieRequestCultureProvider` in combination with the sample code from Microsoft (that you can fine [here](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-2.1#set-the-culture-programmatically) can cause some side effects that might cost you some time before realizing why it is not working. The sample code creates a cookie for the user on the server and appends it to the response. 

```cs
[HttpPost]
public IActionResult SetLanguage(string culture, string returnUrl)
{
    Response.Cookies.Append(
        CookieRequestCultureProvider.DefaultCookieName,
        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
    );

    return LocalRedirect(returnUrl);
}
```

As long as the user has not confirmed to the consent this cookie will be striped away, and the user will not be able to switch the language. So don't search for hours and just make sure that the cookie consent banner works correctly at first and then this shouldn't be an issue. In case you still want to be able to switch the language for the user you'll have to use some other mechanism and do not relay on the `CookieRequestCultureProvider`. Other possibilities could be to store the requested culture inside a query string parameter or on the user profile and implement your own culture provider similar to [this](https://ml-software.ch/blog/writing-a-custom-request-culture-provider).