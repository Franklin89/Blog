Title: Adding a third party datetime picker to your ASP.NET Core MVC Application
Lead: 
Published: 3/4/2019 18:45:00
Tags:
    - ASP.NET Core 2.2
    - Bootstrap
    - DateTime
    - MVC
    - UX
---

In this blog post I want to show something that is trivial and yet always causes me headaches. DateTime pickers have always been a pain to implement, and there are so many ways to accomplish them. For my own reference and maybe for someone out there that is looking for a solution this is how I implement DateTime pickers using tempusdominus-bootstrap-4 including localization and some of its problems it brings.

## Installing the required package 
We need to install the required packages. In most of my projects I use libman but you could also use npm or a cdn. The tempusdominus-bootstrap-4 package requires us to also include `moment.js` which is a very useful package if you are working with DateTime in JavaScript and you most likly already know it anyways.

```json 
{
    "library": "tempusdominus-bootstrap-4@5.1.2",
    "destination": "wwwroot/lib/datepicker"
},
{
    "library": "moment.js@2.24.0",
    "destination": "wwwroot/lib/momentjs",
    "files": [ "locale/de-ch.js", "moment.min.js" ]
}
```

Add the packages to the `_Layout.cshtml`:

Inside the `head` tag:
```html
<link rel="stylesheet" href="~/lib/datepicker/css/bootstrap-datepicker.min.css" />
```

At the bottom of you `_Layout.cshtml`:

```html
<script src="~/lib/momentjs/moment.min.js"></script>
<script src="~/lib/momentjs/locale/de-CH.js"></script>
<script src="~/lib/datepicker/js/tempusdominus-bootstrap-4.min.js"></script>
```

## Updating the HTML
There is not much that I had to change on the HTML code. Just had to add the `datepicker` class to the input fields.

```html
<div class="form-group">
    <label asp-for="DateOfBirth"></label>
    <input class="form-control datepicker" asp-for="DateOfBirth" autocomplete="off">
</div>
```

## Configure bootstrap-datepicker
We need to add a little piece of javscript to the page to add the functionality to the input field for the datepicker. We call the datepicker method on all items with the class `datepicker`. There are many options that you can pass in. Details about them can be found [here](https://bootstrap-datepicker.readthedocs.io/en/latest/options.html)

```js
$(function(){
    $('.datepicker').datepicker({
        orientation: 'bottom',
        autoclose: true
    });
});
```

After this I thought I was done. But when ever I selected a Date from the datepicker I had the following error inside my browser console:

```
The specified value "02/13/2019" does not conform to the required format, "yyyy-MM-dd".
```

How to fix this?

Well the ASP.NET Core `Input TagHelper` sets the type of the input field to `date` which then requires the format to be `yyyy-MM-dd`, which doesn't correspond with the datepicker. To fix this add the `type="text"` attribute to the input.

```html
<input class="form-control datepicker" asp-for="DateOfBirth" type="text" autocomplete="off">
```

So now it seems to be working.

## Add Localization
If your app is localized and you also want to show the date in the regions date format we need to specify the current language to the options of the datepicker.

```js
$(function(){
    $('.datepicker').datepicker({
        orientation: 'bottom',
        autoclose: true,
        language: '@System.Threading.Thread.CurrentThread.CurrentUICulture.Name'
    });
});
```

Since we are working with Razor Pages we can simply get the current threads ui culture and pass it to language property of the options object. But we also need to add the locales script provided by the datepicker library.

```html
<script src="~/lib/bootstrap-datepicker/locales/bootstrap-datepicker.de.min.js"></script>
```

## Custom model binder
If you are using the regions date format inside with a query string parameter strange things might happen. For example if you choose the `01.02.2019` the model binding binds using the format month day year, and the result ist `02.01.2019` which is wrong. It gets even better. If you choose `28.02.2019` it silently fails and sets the value to null (if you have a nullable DateTime type).

```csharp
public class DateTimeModelBinder : IModelBinder 
{ 
    public Task BindModelAsync(ModelBindingContext bindingContext) 
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext)); 
        }

        var modelName = bindingContext.ModelName; 
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName); 

        if (valueProviderResult == ValueProviderResult.None) 
        {
            return Task.CompletedTask; 
        }

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult); 

        var value = valueProviderResult.FirstValue; 

        if (string.IsNullOrEmpty(value)) 
        { 
            return Task.CompletedTask; 
        }

        if (!DateTime.TryParse(value, out var dateTime)) 
        { 
            bindingContext.ModelState.TryAddModelError(modelName, $"Unable to parse {value} to datetime"); 
            return Task.CompletedTask; 
        } 

        bindingContext.Result = ModelBindingResult.Success(dateTime); 
        return Task.CompletedTask; 
    } 
}
```

We have to register our `DateTimeModelBinder` either by adding an attribute to the property:

```csharp
[BindProperty]
[DataType(DataType.Date)]
[ModelBinder(typeof(DateTimeModelBinder))]
public DateTime? DateOfBirth { get; set; }
```

or globally by adding a `ModelBinderProvider` to the MvcOptions:

```csharp
public class CustomModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(DateTime) || context.Metadata.ModelType == typeof(DateTime?))
        {
            return new DateTimeModelBinder();
        }

        return null;
    }
}
```

and registering it:

```csharp
services.AddMvc()
    .AddMvcOptions(options =>
    {
        options.ModelBinderProviders.Insert(0, new CustomModelBinderProvider());
    })
    .AddNewtonsoftJson();
```

## Summary
As we all now DateTime is one of the hardest things to get right in software development. And there are also some issues I know that we can get into with the approach I used above. For example if we register the `DateTimeModelBinder` globally, use a query string parameter and you would like to copy the url and send it to a colleague and he uses a different locale this approach will not work. A solution to this would be to use a special format and model provider for query string parameters e.g. `yyyy-MM-dd`.

If you like this blog post drop a comment or buy me a coffee at the bottom of the page...
