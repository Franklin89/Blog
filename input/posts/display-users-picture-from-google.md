Title: Displaying your google profile picture in an ASP.NET Core Application
Published: 12/09/2020 14:00
Tags: 
    - ASP.NET Core 5.0
    - OpenIdConnect
    - Google
    - User Claims
---
In this blog post I want to show you how easy it is to display a user's profile picture when they sign in with an external provider such as google. 

## Where to get the image from? 

If you use the default ASP.NET Core template with the Identity UI you can scaffold the page `Identity/Account/ExternalLogin`. This page handles the callback we get from our external provider. In the case of this blog post I will be using google to demonstrate this. After scaffolding the identity page, it will be added to the folder structure as shown below. 

![image](/posts/images/IdentityPages.PNG)

In the code for this page, we have a method called `OnPostConfirmationAsync` this method gets called once the user has successfully authenticated with the external provider and has posted back the form with all the required data during registration on your website. 

The info object holds the external login info. This is where we find the external claims from the external provider. 

```csharp 
var info = await _signInManager.GetExternalLoginInfoAsync(); 
``` 

After creating the user and its external login we can add this as a user claim. Of course, you could also create a new column on the user object but in my opinion, this is where user claims come in handy.  

```csharp 
// Let’s add the image to the User Claims 
if (info.Principal.HasClaim(c => c.Type == "image")) 
{ 
  await _userManager.AddClaimAsync(user, info.Principal.FindFirst("image")); 
} 
``` 

 But before we get the `picture` claim on our `info` object, we also need to make sure that the `picture` claim gets mapped. This is done at startup where you configure you external providers 

```csharp 
authBuilder.AddGoogle(options => 
{ 
    options.ClientId = _configuration["Authentication:Google:ClientId"]; 
    options.ClientSecret = _configuration["Authentication:Google:ClientSecret"]; 
    options.ClaimActions.MapJsonKey("image", "picture"); 
}); 

``` 

With this set up the `picture` claim on the external claims principal that we get from google will be mapped to the `image` claim on our `info.Principal` claims principal. If you add any other external provider that return the users picture as well, you will have to do nothing more than mapping their `picture` claim (probably named something different) to the `image` claim. 

Now we have successfully stored the user's picture which is nothing more than a URL to the thumbnail in the user claims. 

## Displaying the picture 

Inside of the `ProfileService` you can now map the user claims in the way you wish so that during the runtime you can access this claim. 

Displaying the image is all up to your UI magic and can be as simple as this:  

```razor
<img src="@context.User.FindFirst("image")?.Value" alt="avatar" class="rounded-circle" style="width: 40px" /> 
``` 

That’s all the magic there is to display the user's picture in your ASP.NET Core Application.

![image](/posts/images/ProfilePicture.PNG)

## Summary 

In this blog post I tried to show how easy it is to map a claim for an external provider onto your user, in this case the `picture` claim that can be used to display the user's avatar picture. Don't forget, these claim values can potentially change. So, an update mechanism would be required. This would typically happen during the login functionality. Just check if the user has the claim and add or update it as required. 

If you like this blog post drop a comment or buy me a coffee at the bottom of the page <i class="fa fa-coffee"></i>

