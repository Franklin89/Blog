Title: Switching my blog to Wyam and hosting it on Netlify
Published: 1/11/2019
Tags: 
    - Wyam
    - Blog
---

I have been running my blog on the Orchard Core CMS, which is a great CMS and I was happy with it. But for just running my blog it has was a bit of an overhead and I really wanted to try out [Wyam](https://wyam.io).

## What is Wyam

Wyam is a static page generator built on .Net Core by [David Glick](https://daveaglick.com). I am very familiar with Markdown and Razor so this is the best solution I can think of. Also Wyam is a great project that I would like to get to know better. David Glick is also the creator and maintainer of [Discover .NET](https://discoverdot.net). A great page for any OSS Developer and .Net enthusiast.

> It's a static content toolkit and can be used to generate web sites, produce documentation, create ebooks, and much more. Since everything is configured by chaining together flexible modules (that you can even write yourself), the only limits to what it can create are your imagination.

## Installing Wyam

Wyam can be installed as dotnet global tool and we have to do is one line of command line:

```cmd
dotnet tool install --global wyam.tool
```

There are more options on how to install Wyam [here](https://wyam.io/docs/usage/obtaining), but since I user .Net Core and the dotnet tools a lot this was the easiest way.

https://erikonarheim.com/posts/using-wyam-blog

https://andyhansen.co.nz/posts/web-config-for-a-static-site

## Creating a new blog



## Adding build scripts

## Setting up continous integration

## Copying my old posts over

## URL Rewrite for old links from `/blog` to `/posts`

- RedirectFrom

## Adding disqus comments back to my page

- Disqus Migration Tool