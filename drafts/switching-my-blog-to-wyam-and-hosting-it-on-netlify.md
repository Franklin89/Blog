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

## Creating a new blog

Wyam has the concept of recipes for creating new sites. There is a detailed page about the [blog recipe]( https://wyam.io/recipes/blog/overview) that I have used. But all I really had to do was:

```cmd
mkdir blog
cd blog
wyam new –r Blog
```

The last command above `wyam new –r Blog` will generate the site in the current directory. To look at the page in the browser we first need to build the static html files by calling `wyam build –r Blog –t CleanBlog`. By calling the `build` command with the `-r` and `-t` pararmeter we tell wyam to build the Blog Recipe with the CleanBlog theme. Those parameters can also be added into the `config.wyam`. Then all you have to call is `wyam build`.

```yaml
#recipe Blog
# Wyam.Markdown
#t CleanBlog
```

To preview the site on the local computer run the `preview` command on the command line:

```cmd
wyam preview
```

This will start up a webserver where the site will be hosted locally.

## Creating a new post

Creating a new post is as easy as dropping a markdown file into the `/posts` folder and adding the proper frontmatter to the file.

```markdown
Title: …
Published: …
Tags:
-	…
-	…
-	…
-	…
---
Blog content …
```

## Changing the default layout

// TODO

## Adding disqus comments

// TODO
Two options: 
-- Create a custom meta-tag
-- Use the disqus migration tool to migrate to the new url

## Migrating previous posts

All my posts until now (well not that many) were already stored as Markdown text inside the Orchard Core eco system. So all I had to do was to copy and paste the content from the Admin interface of my Orchard Core blog and drop them into Markdown files inside my `/posts` folder. Then I had to add the required metadata to the files.

That was it – well no entirely…

My old blog posts had the following url `https://ml-software/blog/{slug}` and for the default setup of wyam they will be located at `https://ml-software/posts/{slug}`. I had the option of either adjusting the pipeline of the wyam build or even simpler add the one line of metadata to my old blog posts. Wyam handles through the `RedirectFrom` meta tag like so:

```markdown
Title: …
Published: …
Tags:
-	…
-	…
-	…
RedirectFrom: blog/{slug}
```

Simple, right?

## Setting up continuous integration with Azure DevOps

I am a big fan of **Azure DevOps**. I have been using Azure DevOps for multiple projects and I really enjoy that it is a platform for all languages. Since I am using cake build scripts for this, my `azure-pipeline.yaml` definition will be as simple as:

```yaml
// TODO: drop azure-pipeline.yaml here
```

### Cake script

// TODO

### Build

// TODO

### Deployment

My blog that was built with Orchard Core was hosted on Azure. Which was quite expensive if you wanted to use a custom domain and the always on feature. Since Wyam creates a static page it can be hosted anywhere really. I saw that [discoverdot.net](//TODO…) was hosted on [Netlify]() I had a closer look at them. They offer a free plan. WOW. So it was possible to reduce my cost to nearly $0 per month by moving my blog from Orchard Core and Azure hosting to Wyam and Netlify.

// TODO
-- Deploying to Netlify

## Summary

That was all for now and my new blog is live! Wyam is very simple to use static page generator. I will definitely have to look deeper into its source code and enhance my blog. I hope this post was useful, let me know what you think or if you have any questions.
