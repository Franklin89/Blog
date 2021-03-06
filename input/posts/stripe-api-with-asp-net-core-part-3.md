Title: Stripe API with ASP.NET Core - Part 3 WebHooks
Lead: Integrating Stripe with ASP.NET Core to provide secure invoicing and subscription processing
Published: 4/28/2019 07:11
Tags: 
    - ASP.NET Core 2.2
    - Stripe
    - Stripe.net
    - Payment
    - WebHooks
---

In the previous posts of this mini-series I showed how to set up an ASP.NET Core 2.2 Application to connect to the Stripe API using Stripe.NET. At the end of the first post I had everything set up so that Products and Pricing Plans are automatically created at application startup if they do not already exist. In the second part I showed how to register a new customer with stripe, adding payment details and signing up the customer to a subscription. You can find the full post [here](https://ml-software.ch/posts/stripe-api-with-asp-net-core-part-2).

In this post I will focus on how to integrate events that are published by Stripe. Stripe offers a series of WebHooks that can be integrated into an application to react on events. For example to handle failed payments or to inform users to update their card details days before the card actually expires. This all helps with the experience a user has with your SaaS platform. In this post I will focus on implementing an event that is triggered 3 days before the trial ends. In case that a user wants to cancel the subscription before they get invoiced or to inform a user that they need to enter their payment details if they want to continue to use the service.

- [Part 1: Setup and Configuration](https://ml-software.ch/posts/stripe-api-with-asp-net-core-part-1)
- [Part 2: Subscription handling](https://ml-software.ch/posts/stripe-api-with-asp-net-core-part-2)
- **Part 3: WebHooks - This Blog Post**

## Stripe WebHooks

Stripe offers a series of WebHooks that enables an application developer to react to events that occur on their Stripe account. This is model is getting more and more popular in the asynchronous world that we are. A developer shall no longer be programming recurring tasks or poll for checking if a payment has succeeded or failed. Stripe will raise an event and send the data through a POST request to the application. The [Stripe documentation](https://stripe.com/docs/webhooks) is very detailed and has great examples on how it works.

## Setting up the WebHooks on the dashboard

Under the _developer settings_ there is a submenu called _WebHooks_. This is where the WebHooks are configured. You can specify which events you want to be sent and the endpoint that they will be sent to.

![image](/posts/images/StripeWebHookSetup.png)

## Testing WebHooks & Architecture decisions

As described in the previous posts, Stripe offers a test environment. This test environment also offers the full support for WebHooks. On top of that they offer sending test requests on demand. This is great while developing the application.

![image](/posts/images/StripeSendTestWebHook.png)

But there is one problem to be solved. While debugging on your local machine, the application is not accessible through a URL. There are different solutions to this problem. You could use a tool like `nGronk` that will offer a public URL that then can be used as an endpoint. Other drawbacks to allowing Stripe sending requests directly to the application is that there needs to be some sort of security mechanism in place so that the endpoint is not open to the public, and you should add some sort of throttling so that the application is not vulnerable to DDoS attacks. All these points made me think. What other solutions does Azure offer. Then something that I wanted to try since a long time but never saw a use case in came to my mind: Azure Functions.

Azure Functions offer security through function keys, they offer a high SLA and through deployment slots we could also leverage different endpoints for the two different environments in Stripe. An Azure Function receives the POST request and parses the JSON Body and adds a message to a Service Bus Queue. With the Queue in place it is possible to register any application desired to listen for a new message and handle it appropriately. This might be a bit over engineered, but in my opinion it offers a secure and simple way of handling the WebHooks.

![image](/posts/images/StripeWebHookArchitecture.png)

## Receiving request from Stripe

As explained above I am receiving the request in a HttpTriggered function. In the constructor I also connect to the service bus. By setting the queue name parameter to `%QueueName%` I can set the name as an application settings.

```csharp
[FunctionName("StripeWebHook")]
public static async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
    [ServiceBus("%QueueName%", Connection = "AzureWebJobsServiceBus", EntityType = EntityType.Queue)] ICollector<Message> messages,
    ILogger log)
{
```

With this in place I can deploy the function to different deployment slots and publish the message to a different queue.

The logic of the function is straight forward. There is something that is special about handling the content of the request. Normally I would use `JsonConvert.DeserializeObject` to deserialize the Json content. But Stripe offers a security feature that allows a developer to check a signature against a secret. With this in place a developer can verify that events were sent by Stripe and not by anyone else. More about this in the [docs](https://stripe.com/docs/webhooks/signatures). The Stripe.NET library that I already used in my previous posts offers a simple method to check the signature.

```csharp
try
{
    var secret = Environment.GetEnvironmentVariable("StripeSecret");
    string json = await new StreamReader(req.Body).ReadToEndAsync();

    var @event = EventUtility.ConstructEvent(json, req.Headers["Stripe-Signature"], secret);
    if (@event == null)
    {
        log.LogError("Unable to construct the event from the body that was sent");
        return new BadRequestObjectResult("Invalid content");
    }

    log.LogInformation($"Processing: {@event.Type}");
    messages.Add(ProcessEvent(@event));

    return new OkResult();
}
catch (StripeException e)
{
    log.LogError(e.Message);
    return new BadRequestResult();
}
```

Inside of the `ProcessEvent` method I do nothing more than constructing my custom payload with only the required properties for my SaaS application. The Stripe events are very detailed and have a lot of information in them.

```csharp
private static Message ProcessEvent(Event @event)
{
    if (@event.Type == Events.CustomerSubscriptionTrialWillEnd)
    {
        var subscription = Mapper<Subscription>.MapFromJson(@event.Data.ToJson(), "object");
        return new Message
        {
            Label = "StripeTrailEndEvent",
            Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
            {
                Type = @event.Type,
                LiveMode = @event.Livemode,
                CustomerId = subscription.CustomerId,
                TrialEnd = subscription.TrialEnd
            }))
        };
    }
    else
    {
        throw new UnhandledEventTypeException($"Unhandled event type: {@event.Type}");
    }
}
```

The message that will be pushed into the queue will be small and only contain the data that will be required by the SaaS application. That is all the magic for receiving a Stripe event through a WebHook and pushing a message into a queue to process it further.

## Handling new messages

Inside of the SaaS application I will handle any new messages that get published to the Service Bus Queue. I will implement an event handler for all the various events that I want to handle and register them at startup.

>If you want more information about how to get started with Service Bus Queues, checkout the following [blog post by Damien Bowden](https://damienbod.com/2019/04/23/using-azure-service-bus-queues-with-asp-net-core-services/). At the moment of writing this blog post he is composing a series of interesting posts about different aspects of the Azure Service Bus.

```csharp
public async Task Handle(StripeTrialEndEvent @event)
{
    var tenant = await _dbContext.Tenants.SingleOrDefaultAsync(x => x.StripeCustomerId == @event.CustomerId);

    if(tenant != null)
    {
        _emailSender.SendTrialEndEmail(tenant.Email, @event.TrialEnd);
    }
    else
    {
        _logger.LogError($"Unable to find a tenant with the following Stripe CustomerId: {@event.CustomerId}");
    }
}
```

## Summary

In this blog post I tried to show one way to handle Stripe WebHooks and how they can be tested also locally while debugging without the need of `nGronk`. Setting up these Azure Services and integrating them with each other has been very easy. Someone might think that this is over engineered but in my opinion since I am hosting all the production code on Azure this is a great way to also learn about other products of Azure.

That was the last part of this mini-series about integrating Stripe with ASP.NET Core to provide secure invoicing and subscription processing. There will be more on this in the future, but for now I have all the features required to handle subscriptions for my SaaS application.

If you like this blog post drop a comment or buy me a coffee at the bottom of the page <i class="fa fa-coffee"></i>
