Title: Using Razor Components in Razor Pages or MVC Views
Lead: Razor Components are a new way of implementing Partial Views or View Components
Published: 12/17/2019 12:00
Tags: 
    - ASP.NET Core 3.1
    - Razor Components
    - Razor Pages
    - MVC
---

It has been a while since my last blog post. But during this time I have learned many new things and in this blog post I would like to share how I am making use of Razor Components in Razor Pages or MVC Views, without any use of the *Server Side Blazor* programming model. So in other words use the components for reusability and simplicity over view components, partial views or tag helpers.

I am a big fan of Razor Pages and in my opinion Razor Pages is one of the best programming models to get started in the ASP.NET Core world. [Blazor](https://blazor.net) is the new kid on the block and no doubt I do like the way Blazor (Client-Side) is being developed. But it is still in preview and therefore no where close of being used in production. Blazor Server-Side has been shipped with ASP.NET Core 3.0 and could be used but in my opinion Blazor Server-Side is not a programming model I want to build an app on. But this is my opinion and others might think different which is okay. But with all this in place, also Razor Components have been introduced and in this blog post I want to show how you can use Razor Components in your Razor Pages or MVC Application.

### What is a Razor Component?
A razor component is nothing more than some embedded C# syntax called *Razor* and HTML Markup to define the UI. More can be found [here in the Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/blazor/components?view=aspnetcore-3.1#component-classes). Components in my case define reusable UI Parts, such as list items or similar. But also a footer can be created as a component instead of the traditional way of using partial views.

### Create a simple component
In a new Razor Pages project create a folder called **Components**. Right click on the folder and select *Add* -> *New Item*. From the New Item Dialog select the Razor Component list item and give it a name.

![image](/posts/images/NewItemDialog.PNG)

Open the new *.razor file and you will see something like this.
```cshtml
<h3>Component</h3>

@code {

}
```

The razor file is divided into the View with HTML and the @code section where the C# code will go. You can have parameters that can be passed into the component and also add some "business" logic. But do not forget in this scenario we will render the html statically on the server, so no fancy Blazor functionality like event handling of clicks etc will work.

For example we can make use of the lifecycle events `OnInitialized` which gets called when the component is instantiated.

```cshtml
<h3>@Text : @counter</h3>

@code {

    [Parameter]
    public string Text { get; set; }

    static int counter = 0;

    protected override void OnInitialized()
    {
        counter++;
    }
}
```

If we now add this component to one of our Razor Pages we expect the `<h3>` to be rendered.

```cshtml
@for (var i = 0; i < 6; i++)
{
    <component type="typeof(Components.Component)" render-mode="Static" param-text='"Hello Universe"' />
}
```

And so it is. You

![image](/posts/images/RazorComponentsOutput.PNG)

### Summary

In this blog post I showed how a very simple Razor Component can be used inside of a Razor Pages application. Components make it very easy to give developers the chance to component based web development. You can also create a component class library, create a NuGet package and distribute it throughout your company. The down side is that with this technique all the fancy Blazor things like Data Binding or Event Handling does not work. But on the other side if you start creating Razor Components, when Blazor Client Side is out of preview you will have a head start. Also in my opinion it is easier and more clear for a developer to create a Razor Component than creating a View Components, Partials or Tag Helpers.

You can find more information on the official Microsoft Docs Page: [https://docs.microsoft.com/en-us/aspnet/core/blazor/components?view=aspnetcore-3.1](https://docs.microsoft.com/en-us/aspnet/core/blazor/components?view=aspnetcore-3.1)

If you like this blog post drop a comment or buy me a coffee at the bottom of the page <i class="fa fa-coffee"></i>