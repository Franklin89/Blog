Title: Adding a third party datetime picker to your ASP.NET Core MVC Application
Lead: 
Published: 3/8/2019 12:00:00
Tags:
    - ASP.NET Core 2.2
    - Bootstrap
    - DateTime
    - MVC
    - UX
---

In this blog post I want to show something that is trivial and yet always causes me headaches. DateTime pickers have always been a pain to implement, and there are so many ways to accomplish them. For my own reference and maybe for someone out there that is looking for a solution this is how I implement DateTime pickers using [tempusdominus-bootstrap-4](https://tempusdominus.github.io/bootstrap-4/Usage/) including localization and some of its problems it brings.

## Installing the required package 
We need to install the required packages. In most of my projects I use `libman` to install client side packages, but you could also use `npm` or a `cdn`. The tempusdominus-bootstrap-4 package requires to also include `moment.js` which is a very useful package if you are working with dates in JavaScript.

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

Add the packages to the `_Layout.cshtml`, inside the `head` tag:

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
I changed the default `input` to the same `input group` that is shown on the usage page of the library (see [here](https://tempusdominus.github.io/bootstrap-4/Usage/)). It appends a small calendar symbol to the input box which I find a nice touch. A next step could be, to encapsulated this into a `TagHelper` to make it easier for developers.

```html
<div class="form-group">
    <label asp-for="DateOfBirth"></label>
    <div class="form-group">
        <div class="input-group date" id="dateofbirth" data-target-input="nearest">
            <input asp-for="DateOfBirth" name="DateOfBirth" type="text" class="form-control datetimepicker-input" data-target="#dateofbirth"/>
            <div class="input-group-append" data-target="#dateofbirth" data-toggle="datetimepicker">
                <div class="input-group-text"><i class="fa fa-calendar"></i></div>
            </div>
        </div>
    </div>
</div>
```

## Configure the datepicker
We need to add a little piece of javscript to the page to add the functionality to the input field for the datepicker. We call the `datetimepicker` method on all items with the class `date`. There are many options that you can pass as a parameter to the method. Details about them can be found [here](https://tempusdominus.github.io/bootstrap-4/Options/).

```js
$(function(){
    $('.date').datetimepicker({
        format: 'L'
    });
});
```

## Adding Localization
If your app requires localized dates and you want to show the date in the regions date format we need to specify the current locale to the options of the datetimepicker.

```js
$(function(){
    $('.date').datetimepicker({
        locale: '@System.Threading.Thread.CurrentThread.CurrentUICulture.Name',
        fromat: 'L'
    });
});
```

Since I am working with Razor Pages we can simply get the current threads ui culture and pass it to the locale property of the options object.

After this I thought I was done but if I had the locale set to `de-CH` the client side validation failed if I selected a date like `28.02.2019`. This is because I am using the jQuery unobtrusive validation and it does not validate a datetime properly if they are in a non US format. But there is an easy fix to this, and since `moment.js` is already in place it is really simple. All that we have to do is update the `date` validation method.

```js
$.validator.methods.date = function (value, element) {
    return this.optional(element) || moment(value, window.currentDateTimeFormat, true).isValid();
}
```

I am setting a global JavaScript variable for the current date time format:

```
window.currentDateTimeFormat = '@System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern.ToUpper()';
```

Everything worked at this point, except the `Edit` page. If there was a datetime set and that value should be loaded into the input, there were strange exceptions in the browsers console.

I figured out that the parsing method of the library was not working properly. I used the option to override the `parseInputDate` function as shown below. 

```js
$(function(){
    var parseInputDate = function (inputDate) {
        var resultDate;
        if (moment.isMoment(inputDate) || inputDate instanceof Date) {
            resultDate = moment(inputDate);
        } else {
            resultDate = moment(inputDate, window.currentDateTimeFormat);
        }
        return resultDate;
    }

    $('.date').datetimepicker({
        locale: '@System.Threading.Thread.CurrentThread.CurrentUICulture.Name',
        fromat: 'L',
        parseInputDate: parseInputDate
    });
});
```

## Custom model binder
If you are using the regions date format as a query string parameter strange things might happen. For example if you choose the `01.02.2019` the model binding binds using the format `month day year`, and the result is `02.01.2019` on the server which is wrong. It gets even better. If you choose `28.02.2019` it silently fails and sets the value to null (if you have a nullable DateTime type). To fix this issue I created a custom implementation of a `DateTimeModelBinder`.

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
