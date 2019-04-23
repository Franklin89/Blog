Title: Stripe API with ASP.NET Core - Part 3 WebHooks
Lead: Integrating Stripe with ASP.NET Core to provide secure invoicing and subscription processing
Published: 4/23/2019 12:00
Tags:
    - ASP.NET Core 2.2
    - Stripe
    - Stripe.net
    - Payment
    - WebHooks
---

In the previous posts of this mini-series I showed how to set up a ASP.NET Core 2.2 Application to connect to the Stripe API using Stripe.NET. At the end of the first post I had everything set up so that Products and Pricing Plans are automatically created at application startup if they do not already exist. In the second part I showed how to register a new customer with stripe, adding payment details and signing up the customer to a subscription. You can find the full post [here](https://ml-software.ch/posts/stripe-api-with-asp-net-core-part-2).

In this post I will focus on how to integrate events that are published by Stripe. Stripe offers a series of WebHooks that can be integrated into an application to react on events. For example to handle failed payments or to inform users to update their card details days before the card actually expires. This all helps with the experience a user has with your SaaS platform. In this post I will focus on implementing an event that is triggered 3 days before the trial ends. In case that a user wants to cancel the subscription before they get invoiced or to inform a user that they need to enter their payment details if they want to continue to use the service.

- [Part 1: Setup and Configuration](https://ml-software.ch/posts/stripe-api-with-asp-net-core-part-1)
- [Part 2: Subscription handling](https://ml-software.ch/posts/stripe-api-with-asp-net-core-part-2)
- **Part 3: WebHooks - This Blog Post**

## TODO LIST
1. Write Intro
1. Describe architecture
1. Azure Functions => To make it easier to develop without the need of nGronk or other tools
1. Service Bus => Ref to Damiens Blog Post 
1.

## Summary

TODO: Write Summary

If you like this blog post drop a comment or buy me a coffee at the bottom of the page <i class="fa fa-coffee"></i>