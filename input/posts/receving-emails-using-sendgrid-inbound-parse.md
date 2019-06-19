Title: Receiving and processing emails using SendGrid Inbound Parse and Azure Functions
Published: 6/19/2019 05:00
Tags:
    - ASP.NET Core 2.2
    - SendGrid
    - Inbound Parse
    - E-Mails
    - Azure Function
    - Azure Service Bus
---

In this blog post I want to show how I integrated _SendGrid's Inbound Parse_ into an ASP.NET Core Application to allow your application to receive emails from users. Many applications like _GitHub_ or _Zendesk_ allow users to answer to system generated emails. These emails are then processed by the application and the content is added to the issue for example. I also had this feature request for a hotel management application. To reduce the usage of outlook and having information in more than one place it would be great if the front desk can send emails to a guest through the application, the guest then answers to this email and instead of having to copy the information from an email client into the system, the system receives the email and adds it to the application as required.

## Prerequisites

First you need to create an account with [SendGrid](https://sendgrid.com) or create a [SendGrid](https://sendgrid.com) resource from your Azure Portal. The only difference is the pricing models. Choose the one that better fits your application. Since I wanted to have everything manageable from inside the Azure Portal I went and created a new resource. The Free Pricing Tier allows you to send 25'000 emails/month for free.

## SendGrid Inbound Parse

[SendGrid](https://sendgrid.com) is not only great for sending emails, but they can also process incoming emails. The Inbound Parse WebHook processes all incoming emails for a specific domain that is set in your DNS, parses the contents and the attachments and POSTs them as `multipart/form-data` to the defined URL.

How to setup SendGrid Inbound Parse can be found [here](https://sendgrid.com/docs/for-developers/parsing-email/setting-up-the-inbound-parse-webhook/). It is also required to add a `MX` record to the DNS of the domain that is setup with SendGrid.

## Handling Incoming WebHooks

I will be using the same approach that I have been using in [this blog post](https://www.ml-software.ch/posts/stripe-api-with-asp-net-core-part-3). If SendGrid receives an email on the configured domain it will create a POST request to a defined endpoint. Instead of implementing this on a controller method inside my ASP.NET Core Application I use an Azure Function to handle this for me. The main benefit is the availability and to encapsulate the logic. After the function has done its magic it creates an entry in an Azure Queue that then is handled by the ASP.NET Core Application. This has proven to be very stable and a good pattern to use for these kind of cases.

![image](/posts/images/SendGridWebHookArchitecture.png)

Inside of the Azure Function (That is setup the same way as in [this blog post](https://www.ml-software.ch/posts/stripe-api-with-asp-net-core-part-3)) I use the [StrongGrid](https://github.com/Jericho/StrongGrid) library that has a WebhookParser that parses the request body into a strongly typed object.

The whole code of the function looks like this. First I parse the request body with the mentioned library [StrongGrid](https://github.com/Jericho/StrongGrid). This will return a strongly typed object with all the data that was sent from SendGrid. Then I use another library to parse the text of the email. In this application I only want the reply that the user sent, without the history. For this I am using the following [NuGet package](https://github.com/jokokko/emailreplyparser). This project is not very active so I might have to change it later on, but for now it does what it needs to. Then the final step for now is to create an anonymous object, pack all the data that is required into it and then create a new entry on the Azure Queue. Inside of the application I handle messages that are being added to the queue and extract the information required.

```csharp
try
{
    // Use StrongGrid to parse the request body (handling multipart/form-data is not so simple)
    var parser = new WebhookParser();
    var inboundMail = parser.ParseInboundEmailWebhook(req.Body);

    // Use  an email parser to get only the visible text. The visible text will be the text that the user replied and not the whole text of the email with the original email.
    var email = EmailParser.Parse(inboundMail.Text);

    var data = new
    {
        To = inboundMail.To.FirstOrDefault()?.Email,
        From = ExtractEmail(inboundMail.From.Email),
        BookingNumber = GetBookingNumberFromEmail(inboundMail.To.FirstOrDefault()?.Email),
        Text = email.GetVisibleText(),
        Html = inboundMail.Html
    };

    log.LogInformation(JsonConvert.SerializeObject(data, Formatting.Indented));

    messages.Add(new Message
    {
        Label = "NewEmailFromGuestEvent",
        Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data))
    });

    return new OkResult();
}
catch (Exception ex)
{
    log.LogError(ex.Message);
    return new BadRequestResult();
}
```

## Summary

When I started thinking about this feature and how I will implement it I thought this will take much more time and effort. But with the help of two great opensource projects, for parsing the `multipard/form-data` and for parsing the email content I got this feature implemented in half the time.
The SendGrid Inbound Parse is a great way of receiving emails and the processing them further on in a Azure Function is very practical. The easy setup and the clear documentation helps a lot in setting this up.

If you like this blog post drop a comment or buy me a coffee at the bottom of the page <i class="fa fa-coffee"></i>