Title: Stripe API with ASP.NET Core - Part 2 Subscription handling 
Lead: Integrating Stripe with ASP.NET Core to provide secure invoicing and subscription processing
Published: 4/4/2019
Tags:
    - ASP.NET Core 2.2
    - Stripe
    - Stripe.net
    - Payment
---

In the first post I showed how to set up a ASP.NET Core 2.2 Application to connect to the Stripe API using Stripe.NET. At the end of the first post I had everything set up so that Products and Pricing Plans are automatically created at application startup if they do not already exist. If you have not read my last blog post you can find it [here](https://ml-software.ch/posts/stripe-api-with-asp-net-core-part-1).

In this post I will focus on how to create a new customer in the application database and also add it to Stripe. The platform does not require a credit card at sign up because the pricing plans offer a free trail for 14 days. The tenant signs up, tests the application and before the trail ends they will need to enter their credit card so that the subscription continues.

- [Part 1: Setup and Configuration](https://ml-software.ch/posts/stripe-api-with-asp-net-core-part-1)
- Part 2: Subscription handling - This Blog Post
- Part 3: WebHooks - Work in progress

## Creating a new Stripe Customer

Before we can start processing payments we need to register a new customer with Stripe. When a new user or in my case a new tenant signs up for the SaaS platform we create a new Stripe customer. In the POST handler of the register tenant razor page we are first creating a new Tenant object that, then creating the customer in Stripe and then saving the Tenant object with the Stripe CustomerId in our database. We need to store the CustomerId to have a link between our Tenant and the Customer stored inside Stripe.

```csharp
var tenant = new Tenant
{
    SubscriptionPlan = Input.Tenant.SubscriptionPlan,
    Name = Input.Tenant.Name,
    Street = Input.Tenant.Street,
    Zip = Input.Tenant.Zip,
    City = Input.Tenant.City,
    Country = Input.Tenant.Country,
    Email = Input.Tenant.Email,
    WebAddress = Input.Tenant.WebAddress,
    Phone = Input.Tenant.Phone,
    Mobile = Input.Tenant.Mobile
};

// This creates the new stripe customer using the Stripe.NET nuget package
tenant.StripeCustomerId = await _stripeService.CreateCustomer(tenant.Email, tenant.Name);

_dbContext.Tenants.Add(tenant);
await _dbContext.SaveChangesAsync();
```

We have abstracted the Stripe calls into a `StripeService` and use that service in our POST handle. The Stripe.NET library has a `CustomerService` class that exposes the method `Create` and `CreateAsync`. We can use those methods to create the customer. All that is required is the email address for the customer. You can add more properties and also metadata similar to the pricing plans (all objects in Stripe have the metadata concept which makes it very dynamic and flexible).

```csharp
public async Task<string> CreateCustomer(string email, string fullName)
{
    var response = await _customerService.CreateAsync(new CustomerCreateOptions
    {
        Email = email,
        Description = fullName
    });

    return response.Id;
}
```

So now we have our Stripe customer and the link to our tenant. But during the registration process you can choose between the different pricing plans. Now we have to create a new subscription for the customer that we created above.

## Creating a new Subscription

All we need to create a new subscription for a customer is the Stripe customer Id that we stored along the tenant and some way to identify the pricing plan that customer wants to subscribe to.

So back in the POST handler of the register page we add the following line of code after creating the customer.

```csharp
await _stripeService.AddSubscription(tenant.StripeCustomerId, tenant.SubscriptionPlanId);
```

The UI of the registration page has a simple dropdown where the user chooses from the different pricing plans, which displays the name of the pricing plan and the Id as a value. 

Then we use the `SubscriptionService` from Stripe.NET to call the Stripe API and create the subscription.

```csharp
public async Task AddSubscription(string customerId, string planId)
{
    _subscriptionService.Create(new SubscriptionCreateOptions
    {
        CustomerId = customerId,
        Items = new List<SubscriptionItemOption>
            {
                new SubscriptionItemOption
                {
                    PlanId = planId,
                    Quantity = 1
                }
            },
        TrialFromPlan = true
    });
}
```

> Again there are many more properties that could be set or changed here. I am just showing a very simple approach with only the required properties set. For more information check out Stripe API reference. It is very well documented.

Wow, that was simple. So now we have created a customer with a subscription. Inside of the Stripe Dashboard you should now see the customer and the subscription that we created.

![image](/posts/images/StripeCustomer.png)

In our SaaS application we created a billing page where an admin user can view the current state of the subscription and also add payment details.

![image](/posts/images/SettingsBilling.png)

## Adding payment details

> As I already explained in the first part of this series NEVER store or handle any payment details on your own server.

