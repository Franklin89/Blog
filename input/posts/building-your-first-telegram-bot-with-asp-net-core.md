Title: Building your first Telegram Bot with ASP.NET Core
Published: 01/21/2021 14:00
Tags: 
    - ASP.NET Core 5.0
    - Telegram
    - Bot
---
In this blog post I would like to share a quick start guide on how to implement a telegram bot using ASP.NET Core and offer you a template to get started fast.

There is no limit set for what a bot can be used in today’s world. We see them in every branch of the economy. We most commonly see them on webpages as assistants that can help with customer support for example. Bots can also be used as moderator for chat rooms. We can put it this way: bots are some sort of third-party application that run inside of the chat application, in this case Telegram. Users interact with the bot by sending messages.

## Botfather

![image](/posts/images/botfather.jpg)
 
Before you can communicate with the Telegram API and test your bot, you need to register your bot and obtain an Access Token from the @Botfather. Telegrams Botfather himself is a bot that helps you manage your bots and allows you to generate an access token. Make sure that the access token is kept as a secret. More on how to register your bot is described [here]( https://core.telegram.org/bots#6-botfather).

## How do bots work?

Every platform works a bit different in their way of communicating between their API and your piece of software. In the template the service that is responsible to communicate with the Telegram Bot API using the Telegram.Bot NuGet package has been abstracted so that you could change the platform in a simple and easy way.

Telegram.Bot is the only dependency in this template. It handles the communication with Telegrams Bot API. Messages sent to the bot are passed down to the software developed by you and running on your servers. Telegrams server handles all encryption and communication with the Telegram API. We only communicate with this server through a simple HTTPS interface that offers a very simplified Telegram API. This interface is called the [Bot API]( https://core.telegram.org/bots/api).

## Template
So essentially a bot is nothing than a small piece of software that receives messages or commands and reacts based on the content. To make it easy to start developing your own bot I created a `dotnet new` template that creates a simple project and the base for your commands. The GitHub repository can be found [here](https://github.com/Franklin89/TelegramBotTemplate).

### Installing the template

`dotnet new -i TelegramBotTemplate`

This will install the latest version of the template. After the successful installation you should see the template in your list.

![image](/posts/images/telegram-template.PNG)

To create your new bot project navigate to the folder where you would like your source code to be. Run the following command to create the project:

`dotnet new telegram-bot -n YourComany.Bot`

> Change the ´YourCompany.Bot´ to the name you want the project to be.

Congratulations! You created your first bot. All you need to do now is to add your access token that you obtained from the Botfather to the user secrets. Hit F5 in Visual Studio or run the bot from the command line. You should now be able to chat with the bot. He is not really smart yet and only has one command: `/ping`. He should answer with pong 😉
Next, create your own commands and develop your business process.

## Summary 
Bots can be extended to your wishes and to your creativity. You can hook it up to AI like Azure Cognitive Services to evaluate the sentiment of your chat or to filter offensive words and so on. As you can see there is no limit to what bots can do. As it states on the Telegram docs page: 

> “Do virtually anything else. Except for dishes — bots are terrible at doing the dishes”. 

Currently the `IChatService` is closely coupled with Telegram. This is something that is still an open task and that I want to resolve in the future.

If you like this blog post drop a comment or buy me a coffee at the bottom of the page <i class="fa fa-coffee"></i>

### Links

- [GitHub Project](https://github.com/Franklin89/TelegramBotTemplate)
- [NuGet](https://www.nuget.org/packages/TelegramBotTemplate/)
- [Telegram.Bot Project Link](https://telegrambots.github.io/book/)
- [Telegram Docs Link](https://core.telegram.org/)
