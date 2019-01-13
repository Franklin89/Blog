Title: Extending Client Side Validation with DataAnnotations and jQuery Unobtrusive in an ASP.NET Core Application
Published: 11/07/2018
Image: /images/home-bg.jpg
Tags: 
    - ASP.NET CORE 2.1
    - DataAnnotations
    - jQuery
    - Client Side
---

In my last blog post [Extending Client Side Validation with FluentValidation and jQuery Unobtrusive in an ASP.NET Core Application](https://ml-software.ch/blog/extending-client-side-validation-with-fluentvalidation-and-jquery-unobtrusive-in-an-asp-net-core-application), I showed how to extend the client side validation with FluentValidation. In this blog post I want to continue this path, but instead of using FluentValidation I will show how to do this by writing a custom DataAnnotation attribute.

If you are using DataAnnotations for your validation you have probably experienced the need for a custom attribute. For example for validating a `boolean` flag to be true. This is simple if you just need the server side validation but if you also want the unobtrusive client side validation you have to do some work (good news: not much at all).

We will look at the example of validating a checkbox that states that the user has accepted our privacy policy.

#### Creating the attribute
We need to create the attribute that we then add to the property we want to validate. At the end it should look like this:

```cs
public class RegistrationViewModel
{
  [Required]
  public string Username { get; set; }
	
  [MustBeTrue]
  public bool AcceptedPrivacyPolicy { get; set; }
}
```

We need to create a class called `MustBeTrueAttribute` and inherit from `ValidationAttribute`. This class is located in the `System.CompontentModel.DataAnnotations` namespace.

```cs
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class MustBeTrueAttribute : ValidationAttribute
{
    public override string FormatErrorMessage(string name)
    {
        return "The " + name + " field must be checked in order to continue.";
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
        }

        var boolValue = value as bool?;
        if (boolValue != null && boolValue == true)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
    }
}
```

We overried the `IsValid` method and implement our own logic. In this case we check if the value is a boolean and if it is true then we return a `ValidationResult.Success`. In case the validation logic failed we create a new `ValidationResult` and pass in the error message we would like to be displayed.

Now we can add this attribute to our view model and the validation would already work on the server side. Simple. But now we need to extend this to also work on the client so that the user can not post invalid data to the server and to enhance the user experience.

#### Creating the attribute adapter
An `AttributeAdapter` is required for the client side validation only. It gets called while rendering your view and enables you as a developer to add custom html attributes to the element which we can later on check in your javascript code when extending the jQuery Unobtrusive library.

We create a class called `MustBeTrueAttributeAdapter` and inherit from the generic class `AttributeAdapterBase<T>` where `T` is our attribute - `MustBeTrueAttribute`  

```cs
public class MustBeTrueAttributeAdapter : AttributeAdapterBase<MustBeTrueAttribute>
{
    public MustBeTrueAttributeAdapter(MustBeTrueAttribute attribute, IStringLocalizer stringLocalizer)
        : base(attribute, stringLocalizer)
    {
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-must-be-true", GetErrorMessage(context));
    }

    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    {
        if (validationContext == null)
        {
            throw new ArgumentNullException(nameof(validationContext));
        }

        return GetErrorMessage(validationContext.ModelMetadata, validationContext.ModelMetadata.GetDisplayName());
    }
}
```

We then override the `AddValidation` method where we can add the html attributes mentioned above. The base class offers a method called `MergeAttribute` that we can use to add the html attributes.

#### Creating the attribute adapter provider
Somehow we need a way to register our attribute adapter with the DI framework that ASP.NET Core offers. In order to do this we can implement a provider class that implements the `IValidationAttributeAdapterProvider` interface. We need to implement the method `GetAttributeAdapter` where we check the type of the attribute and in case it is our custom `MustBeTrueAttribute` we return our matching attribute adapter: `MustBeTrueAttributeAdapter` and if not we let the base provider take over.

```cs
public class MustBeTrueAdapterProvider : IValidationAttributeAdapterProvider
{
    private readonly IValidationAttributeAdapterProvider _baseProvider = new ValidationAttributeAdapterProvider();

    public IAttributeAdapter GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer stringLocalizer)
    {
        if (attribute is MustBeTrueAttribute)
        {
            return new MustBeTrueAttributeAdapter(attribute as MustBeTrueAttribute, stringLocalizer);
        }
        else
        {
            return _baseProvider.GetAttributeAdapter(attribute, stringLocalizer);
        }
    }
}
```

> You could also write a generic provider and add all your logic in that provider class. This makes sense if you have more than one custom validation attribute to keep your code clean.

#### Registering the attribute adapter provider to the DI framework
As you probably already know ASP.NET Core, it is very simple to register something with the DI framework. Simply add the following line of code to your `ConfigureServices` method inside your `Startup` class.

```cs
services.AddSingleton<IValidationAttributeAdapterProvider, MustBeTrueAdapterProvider>();
```

#### Extending the jQuery Unobtrusive with our custom validator
In this example I am adding the validator logic into the `_ValidationScriptsPartial.cshtml` partial view as an inline script. If you check my [other blog post](https://ml-software.ch/blog/extending-client-side-validation-with-fluentvalidation-and-jquery-unobtrusive-in-an-asp-net-core-application) you can see that it is also possible to add a custom javascript file. Compared with the other validator that I created in the other blog post, this is very simple and has not much logic at all.

```js
<script>
    $.validator.addMethod('must-be-true', function (value, element, params) {
        return element.checked;
    });

    $.validator.unobtrusive.adapters.add('must-be-true', [], function (options) {
        options.rules['must-be-true'] = {};
        options.messages['must-be-true'] = options.message;
    });
</script>
```

So that is it. We have successfully added a custom validation attribute and extended the client side validation with our own logic.