Title: Getting the base URL for an ASP.NET Core MVC web application in your static JavaScript files
Published: 12/11/2018
Image: /images/home-bg.jpg
Tags: 
    - ASP.NET CORE 2.1
    - Client Side
RedirectFrom: blog/getting-the-base-url-for-an-asp-net-core-mvc-web-application-in-your-static-javascript-files
---

In this post I want to show a simple way for getting the base URL for an ASP.NET Core MVC web application in your static JavaScript files.

### What is the actual problem
I was writing a web application that had more than just a few lines of JavaScript that could be directly embedded into the `cshtml`. But I found my self in the situation that I had to reference static content from my JavaScript files. This is no problem as long as you are **not** hosting the app in a subfolder. While developing the app on your localhost everything might work great, but during production you might get random 404 errors.

### Solution for Views
Inside razor views this is simple and nothing new to use the `UrlHelper` class. For example to get the proper URL for an image we would type `Url.Content("~/images/logo.png")`. This would automatically generate the proper URL no matter if the application is hosted in a subfolder or not.

### Solution for JavaScript files
There actually turns out to be a [HTML Element](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/base) for exactly this purpose - the `<base>` HTML Element.

So my solution looks like this, inside your `_Layout.json`:
```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <base href='@Url.AbsoluteContent("~/")'>
    <title>@ViewBag.Title - ASP.NET Core Web Application</title>
    <!-- ... -->
</head>
<body>
```

The `AbsoluteContent` is an extension method that I added to my application to get the full URL and not only the path.
<script src="https://gist.github.com/Franklin89/a0467977f56dd68eb32d25c24cfbb88f.js"></script>

Then inside of the JavaScript you can access the base URL by calling:
```js
var baseUrl = document.baseURI;
```

### Summary
1. Insert a `<base>` element with a `href` attribute into the the `<head>` section.
1. Get that attribute value from the script

In my opinion this is a clean and simple solution.


