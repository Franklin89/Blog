Title: Extending Client Side Validation with FluentValidation and jQuery Unobtrusive in an ASP.NET Core Application
Published: 10/25/2018
Tags: 
    - ASP.NET CORE 2.1
    - FluentValidation
    - jQuery
    - Client Side
RedirectFrom: blog/extending-client-side-validation-with-fluentvalidation-and-jquery-unobtrusive-in-an-asp-net-core-application
---

FluentValidation is an ["A popular .NET library for building strongly-typed validation rules."](https://fluentvalidation.net/). I have been using it in different projects and I really like that you can keep your view models without any validation attributes on the properties, so that they can be reused where ever possible. FluentValidation has the concept of `RuleSets` that can be used to validate your model depending on your context. Maybe you have a scenario where you validate more properties while editing an object and only need the minimal properties to be validated while creating the object.

In one of my projects I am currently working on, we had the a lot of complex forms with business rules like: *If property A has the value 5 selected then property B is required.* FluentValidation has a method you can define on your rule called `When()`, which I was able to use exactly for this type of validation. **BUT** these are not translated into unobtrusive java script validation rules on the client side. So the issue with this is that the user will hit save and in return get the validation error only after the request was sent to the server where the validation error was caught.

So I had to extend the FluentValidation library and add some java script for the client side validation. In this blog post I will try to show how I did this.

#### Server side property validator
First we have to implement the server side property validator that can be used for creating rules.

```csharp
public class RequiredIfValidator : PropertyValidator
{
    public string DependentProperty { get; set; }
    public object TargetValue { get; set; }

    public RequiredIfValidator(string dependentProperty, object targetValue) : base(new LanguageStringSource(nameof(RequiredIfValidator)))
    {
        DependentProperty = dependentProperty;
        TargetValue = targetValue;
    }

    protected override bool IsValid(PropertyValidatorContext context)
    {
        //This is not a server side validation rule. So, should not effect at the server side
        return true;
    }
}
```

> *Of course it is possible to add the server side validation rule in here as well*

#### Adding the client validator
For the client side we first have to add a client validator which will used by the FluentValidation to add the `data-val...` properties to the html.

```csharp
public class RequiredIfClientValidator : ClientValidatorBase
{
    RequiredIfValidator RequiredIfValidator
    {
        get { return (RequiredIfValidator)Validator; }
    }

    public RequiredIfClientValidator(PropertyRule rule, IPropertyValidator validator) : base(rule, validator)
    {
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-requiredif", GetErrorMessage(context));
        MergeAttribute(context.Attributes, "data-val-requiredif-dependentproperty", RequiredIfValidator.DependentProperty);
        MergeAttribute(context.Attributes, "data-val-requiredif-targetvalue", RequiredIfValidator.TargetValue.ToString());
    }

    private string GetErrorMessage(ClientModelValidationContext context)
    {
        var formatter = ValidatorOptions.MessageFormatterFactory().AppendPropertyName(Rule.GetDisplayName());
        string messageTemplate;
        try
        {
                messageTemplate = Validator.Options.ErrorMessageSource.GetString(null);
        }
        catch (FluentValidationMessageFormatException)
        {
                messageTemplate = ValidatorOptions.LanguageManager.GetStringForValidator<NotEmptyValidator>();
        }
        var message = formatter.BuildMessage(messageTemplate);
        return message;
    }
}
```

It is as simple as this. The `ClientValidatorBase` class already offers methods to `MergeAttributes` and has a `Validator` property that we have to cast into our concrete type so that we can retrieve the `DependentProperty` and the `TargetValue`.

#### Adding the validator to the FluentValidation configuration
Last but not least the FluentValidation library needs to know about the new validator. First we add the FluentValidation library to our project and register it by calling the `AddFluentValidation` extension method. Because we added a new client side validator we need to add it to the client side validation configuration as you see below.

```csharp
services.AddMvc()
.AddFluentValidation(fv =>
{
    fv.RegisterValidatorsFromAssemblyContaining<Startup>(); // Registers all validators from this assembly
    fv.ConfigureClientsideValidation(clientSideValidation =>
    {
        clientSideValidation.Add(typeof(RequiredIfValidator), (context, rule, validator) => new RequiredIfClientValidator(rule, validator));
    });
})
```

We also have to add the `RequestIfValidator` to our `Rule`.

```csharp
RuleFor(x => x.CountryId)
  .SetValidator(new RequiredIfValidator("DivisionId", 11))
  .WithMessage("A country has to be selected");
```

#### Extending jquery unobtrusive
ASP.NET Core MVC relies on the jquery unobtrusive validation for handling the client side error messages. We need to implement the logic for the `RequiredIfValidator`. I created a new java script file called `customValiators.js` where I can add all my validators. Then I just added this script to the `_ValidationScriptsPartial.cshtml`.

```html
<environment include="Development">
    <script src="~/lib/jquery-validation/jquery.validate.js"></script>
    <script src="~/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.js"></script>
    <script src="~/js/customValidators.js"></script>
</environment>
<environment exclude="Development">
    <script src="~/lib/jquery-validation/jquery.validate.min.js"></script>
    <script src="~/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js"></script>
    <script src="~/js/customValidators.min.js"></script>
</environment>
```

We have to register a new adapter for our `RequiredIf` validator and pass in a string array with any additional properties that we want. Inside the function we have to configure a new rule with the name `requiredif` and add the additional properties.
```js
$.validator.unobtrusive.adapters.add('requiredif', ['dependentproperty', 'targetvalue'], function (options) {
    options.rules['requiredif'] = {
        dependentproperty: options.params['dependentproperty'],
        targetvalue: options.params['targetvalue']
    };
    options.messages['requiredif'] = options.message;
});
```

Now we need to implement the client side logic for our validator. We add a new method to the jquery validator object with the name of our validator: `requiredif`. As a second argument we pass in a function with our validation logic. To keep it simple I called the `required` validation method that is inside the default package which will do the proper check for me.
```js
$.validator.addMethod('requiredif', function (value, element, parameters) {

    var dependentProperty = '#' + parameters['dependentproperty'];
    var targetvalue = parameters['targetvalue'];
    targetvalue = (targetvalue == null ? '' : targetvalue).toString();

    var dependentControl = $(dependentProperty);
    var dependentValue = dependentControl.val();

    // if the condition is true, reuse the existing required field validator functionality
    if (targetvalue.toUpperCase() === dependentValue.toUpperCase()) {
        return $.validator.methods.required.call(this, value, element, parameters);
    }

    return true;
});
```

Thats all we need to do to extend and implement our own validation rules and adding client side validation using FluentValidation and jQuery Unobtrusive.