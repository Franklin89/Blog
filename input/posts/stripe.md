Title: Adding Stripe to a Saas Application
Lead:
Published: 2/28/2019 11:00
Tags:
    - ASP.NET Core 2.2
    - Identity
    - Stripe
    - Payment
---

In this blog post I want to show how I integrated [Stripe](https://stripe.com), a very popular and well known payment gateway, into an ASP.NET Core MVC Application. The application that I have built offers three subscription plans: Basic, Professional and Enterprise. Depending on the subscription plan selected at sign up,

# Stripe

Stripe is very popular not only because of their well documented APIs, simple and straight forward approach for developers, but also because of their clear [pricing model](TODO: Link to the pricing model).

> We have to watch out here: we are working with very sensitive data (Credit card numbers and other personal information), to be allowed to receive and store that data on my server, I would need to be [PCI Compliant](). But this is not an easy task for small companies or a single person. Most payment gateways offer a solution for this: As an application developer you display the order details to the customer and if they agree to purchase, they get redirected to a page hosted by the payment gateway. After the purchase is complete the customer gets redirected back.

Stripe offers multiple ways on handling this. The one I will take a look at is using their JavaScript API. By using this API we can send the credit card information directly to the Stripe's servers. With this in place we will not need to be PCI Compliant.

# Setup Stripe

// TODO: Write about the product setup

# Integrate with ASP.NET Core Identity

The is only one piece of data that we really needed to store in our application. We need to connect 