Title: Update - Implementing a custom ITicketStore for ASP.NET Core Identity
Lead: This adds the ability for a user to remotely log out from his sessions or invalidate any session server side
Published: 2/28/2019 11:00
Tags: 
    - ASP.NET Core 2.2
    - Authentication
    - ASP.NET Core Identity
---

After my last [blog post](https://ml-software.ch/posts/implementing-a-custom-iticketstore-for-asp-net-core-identity) about implementing a custom `ITicketStore` to handle remote logout, I got some interesting input from friends on how I could implement the missing pieces I had at the end of my last post.

I wanted to add some more metadata to an `AuthenticationTicket` like the browser or operating the system associated with the session. While writing the MVP it was unsure what the best way was to actually get that data.

What I ended up implementing: instead of passing in the `DbContextOptionsBuilder` to the `CustomTicketStore`, I passed in the `IServiceCollection` while configuring the application cookie. Inside of the `CustomTicketStore` I build the service provider and create a scope on which we can get the required services. It is possible to get an instance of the `IHttpContextAccessor` which allows access to the current `HttpContext`. From this we can get all sorts of metadata and associate them with the `AuthenticationTicket`. In the implementation below I get the remote IP address and the User-Agent header from the `HttpContext`. By using a NuGet package called `UAParser` it makes it simple to parse the information that interests us from the User-Agent header like: browser, browser version and operating system. With the remote IP address we could now do a GeoLocation lookup and also display that information to the user.

## Updated CustomTicketStore

Now we pass in the `IServiceCollection` that allows us to resolve the `HttpContext` and the database context.

```csharp
public class CustomTicketStore : ITicketStore
{
    private readonly IServiceCollection _services;

    public CustomTicketStore(IServiceCollection services)
    {
        _services = services;
    }

    public async Task RemoveAsync(string key)
    {
        using (var scope = _services.BuildServiceProvider().CreateScope())
        {
            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            if (Guid.TryParse(key, out var id))
            {
                var ticket = await context.AuthenticationTickets.SingleOrDefaultAsync(x => x.Id == id);
                if (ticket != null)
                {
                    context.AuthenticationTickets.Remove(ticket);
                    await context.SaveChangesAsync();
                }
            }
        }
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        using (var scope = _services.BuildServiceProvider().CreateScope())
        {
            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            if (Guid.TryParse(key, out var id))
            {
                var authenticationTicket = await context.AuthenticationTickets.FindAsync(id);
                if (authenticationTicket != null)
                {
                    authenticationTicket.Value = SerializeToBytes(ticket);
                    authenticationTicket.LastActivity = DateTimeOffset.UtcNow;
                    authenticationTicket.Expires = ticket.Properties.ExpiresUtc;
                    await context.SaveChangesAsync();
                }
            }
        }
    }

    public async Task<AuthenticationTicket> RetrieveAsync(string key)
    {
        using (var scope = _services.BuildServiceProvider().CreateScope())
        {
            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            if (Guid.TryParse(key, out var id))
            {
                var authenticationTicket = await context.AuthenticationTickets.FindAsync(id);
                if (authenticationTicket != null)
                {
                    authenticationTicket.LastActivity = DateTimeOffset.UtcNow;
                    await context.SaveChangesAsync();

                    return DeserializeFromBytes(authenticationTicket.Value);
                }
            }
        }

        return null;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        using (var scope = _services.BuildServiceProvider().CreateScope())
        {
            var userId = string.Empty;
            var nameIdentifier = ticket.Principal.GetNameIdentifier();
            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

            if (ticket.AuthenticationScheme == "Identity.Application")
            {
                userId = nameIdentifier;
            }
            else if (ticket.AuthenticationScheme == "Identity.External")
            {
                userId = (await context.UserLogins.SingleAsync(x => x.ProviderKey == nameIdentifier)).UserId;
            }

            var authenticationTicket = new Entities.AuthenticationTicket()
            {
                UserId = userId,
                LastActivity = DateTimeOffset.UtcNow,
                Value = SerializeToBytes(ticket)
            };

            var expiresUtc = ticket.Properties.ExpiresUtc;
            if (expiresUtc.HasValue)
            {
                authenticationTicket.Expires = expiresUtc.Value;
            }

            var httpContextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();
            var httpContext = httpContextAccessor?.HttpContext;
            if (httpContext != null)
            {
                var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
                if (remoteIpAddress != null)
                {
                    authenticationTicket.RemoteIpAddress = remoteIpAddress.ToString();
                }

                var userAgent = httpContext.Request.Headers["User-Agent"];
                if (!string.IsNullOrEmpty(userAgent))
                {
                    var uaParser = UAParser.Parser.GetDefault();
                    var clientInfo = uaParser.Parse(userAgent);
                    authenticationTicket.OperatingSystem = clientInfo.OS.ToString();
                    authenticationTicket.UserAgentFamily = clientInfo.UserAgent.Family;
                    authenticationTicket.UserAgentVersion = $"{clientInfo.UserAgent.Major}.{clientInfo.UserAgent.Minor}.{clientInfo.UserAgent.Patch}";
                }
            }

            context.AuthenticationTickets.Add(authenticationTicket);
            await context.SaveChangesAsync();

            return authenticationTicket.Id.ToString();
        }
    }

    private byte[] SerializeToBytes(AuthenticationTicket source)
        => TicketSerializer.Default.Serialize(source);

    private AuthenticationTicket DeserializeFromBytes(byte[] source)
        => source == null ? null : TicketSerializer.Default.Deserialize(source);
}
```

Configure the application cookie to use the custom implementation and pass in the `IServiceCollection`:

```csharp
services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    options.SessionStore = new CustomTicketStore(services);
});
```

## User Interface

We can now update the user interface to something more compelling for the user:

![image](/posts/images/UpdatedSessionList.png)

## Summary

After this small update I am happy with what I have. But I am still unsure if this will scale without a cache in place. We are still hitting the database many times in our `RetrieveAsync` method. We could implement a cache with an absolute expiration? This might cause some other issues, but I will have to do some load tests and see what they show. I will update this post if I have that in place. Thank you @Stephan for the inputs and feedback.

If you like this blog post drop a comment or buy me a coffee at the bottom of the page...
