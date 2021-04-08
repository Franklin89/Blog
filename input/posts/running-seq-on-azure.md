Title: Running Seq on Azure
Published: 04/08/2021 14:00
Tags: 
    - Azure
    - Docker
    - Seq
---
In this blog post I would like to share a quick how to on how you can run a Seq instance on Azure.

[Seq](https://datalust.co/Seq) is one of my favorite logging tools out there at the moment. The User interface is simple and straight forward to use. For a small PoC that I was working on, that was hosted on Azure I needed a way to display log messages and a simple metric dashboards. Running Seq locally on your desktop has gotten really simple with their docker images. Utilizing the same image, it is possible to run Seq on Azure in just a few simple steps.

## Step 1

First we need to create a new App Service Web App. Make sure to choose `Publish` -> `Container` and the `Operating System` should be `Linux`. You can set the `Sku and size` to what ever you require.

![image](/posts/images/portal-appservice-docker-1.PNG)

## Step 2

On the next step we need to configure the `Image Source` and `Image and tag` that we would like to run. You can specify an explicit version or just use the `latest` tag.

![image](/posts/images/portal-appservice-docker-2.PNG)

## Step 3

Since we are not running our software but rather a third party application we do not need to `Enable Application Insights`.

![image](/posts/images/portal-appservice-docker-3.PNG)

## Step 4

Nothing to be done here.

![image](/posts/images/portal-appservice-docker-4.PNG)

## Step 5

After the Web App has been created you will not be able to access it yet. We need to set some `Application settings` under the `Configuration` blade so that the container can start up and that we can access it. First add the following settings
- `ACCEPT_EULA` => `Y` 
- `WEBSITES_PORT` => `80:5341`.

![image](/posts/images/portal-appservice-docker-configuration.PNG)

Now restart the Web App and you should be able to access it.

## Step 6

If you have your application configured correctly you should see log messages arriving. But the data is only being stored inside of the container. We need to mount some storage. From the `Configuration` blade choose `Path mappings` and configure a new azure storage mount. You might have to configure this first in your storage account. Make sure to choose `Azure Files` and not `Azure Blob`.

![image](/posts/images/portal-appservice-docker-pathmappings.PNG)

This is all the magic there is to running Seq on Azure. Now you can configure your Seq instance the way you want, with keys and metrics dashboards.

If you like this blog post drop a comment or buy me a coffee at the bottom of the page <i class="fa fa-coffee"></i>
