Title: Implementing a custom ITicketStore for ASP.NET Core Identity
Lead: This adds the ability for a user to remotely log out from his sessions or invalidate any session server side
Published: 2/21/2019 08:00
Tags: 
    - ASP.NET Core 2.2
    - Authentication
    - ASP.NET Core Identity
---

In this blog post I want to show how to implement a custom `ITicketStore` for ASP.NET Core Identity. The `ITicketStore` implementation is responsible for storing the authentication tickets. By default, the tickets are stored in a cookie which is then sent to the user.

## Why implement a custom ITicketStore?

Out of the box ASP.NET Core Identity stores the authentication tickets inside a cookie that lives on the client. If for any reason the requirement is to store the authentication tickets on the server then we have the possibility to implement a custom `ITicketStore`. What I had to achieve was that a user should be able to see all his logged in sessions and log them out if required. This is similar to Facebooks scenario that you can see in the image below. To achieve this behavior ASP.NET Core Identity offers the developer to override the `SessionStore`. With this in place we can also offer an admin user to invalidate any session server side.

![image](/posts/images/FacebookWhereYoureLoggedIn.PNG)

I have decided to save the authentication tickets inside my database, next to the ASP.NET Core Identity tables. It would also be possible to use a distributed cache like Redis or even just store them in Memory.

## Create the database object

Let’s think about what we need in our object to store the authentication ticket. We need to be able to store the authentication ticket, the user id that owns the token and an id that identifies the ticket. The id will be returned and will be included in the cookie that is returned to the client. Another benefit from this is that the cookie will be reduced in size.

```csharp
public class AuthenticationTicket
{
    public Guid Id { get; set; }

    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public byte[] Value { get; set; } 

    public DateTimeOffset? LastActivity { get; set; }

    public DateTimeOffset? Expires { get; set; }
}
```

Create the entity framework core migration and run the migration against the database.

## Create the CustomTicketStore

We create a new class that implements the `ITicketStore` and implement the required methods:

- `Task RemoveAsync(string key)`
- `Task RenewAsync(string key, AuthenticationTicket ticket) `
- `Task<AuthenticationTicket> RetrieveAsync(string key) `
- `Task<string> StoreAsync(AuthenticationTicket ticket) `

At the time of writing this blog post, `ITicketStore` is not being initialized by the framework through dependency injection, so we cannot use constructor injection like we usually would. You could make use of the `IPostConfigureOptions` concept but since we will need the `DbContext`, which is a scoped service, and the `ITicketStore` is a singleton we have a conflict anyways. I went the old way and created a new instance of the `CustomTicketStore`, passed in the `DbContextOptions` and created a new instance of the `DbContext` when required.

```csharp
public class CustomTicketStore : ITicketStore
{
    private readonly DbContextOptionsBuilder<ApplicationDbContext> _optionsBuilder;

    public CustomTicketStore(DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder)
    {
        _optionsBuilder = optionsBuilder;
    }

    public async Task RemoveAsync(string key)
    {
        using (var context = new ApplicationDbContext(_optionsBuilder.Options))
        {
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
        using (var context = new ApplicationDbContext(_optionsBuilder.Options))
        {
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
        using (var context = new ApplicationDbContext(_optionsBuilder.Options))
        {
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
        var userId = string.Empty;
        var nameIdentifier = ticket.Principal.GetNameIdentifier();

        using (var context = new ApplicationDbContext(_optionsBuilder.Options))
        {
            if (ticket.AuthenticationScheme == "Identity.Application")
            {
                userId = nameIdentifier;
            }
            // If using a external login provider like google we need to resolve the userid through the Userlogins
            else if (ticket.AuthenticationScheme == "Identity.External") 
            {
                userId = (await context.UserLogins.SingleAsync(x => x.ProviderKey == nameIdentifier)).UserId;
            }

            var authenticationTicket = new Entities.AuthenticationTicket()
            {
                UserId = userId,
                LastActivity = DateTimeOffset.UtcNow,
                Value = SerializeToBytes(ticket),
            };

            var expiresUtc = ticket.Properties.ExpiresUtc;
            if (expiresUtc.HasValue)
            {
                authenticationTicket.Expires = expiresUtc.Value;
            }

            await context.AuthenticationTickets.AddAsync(authenticationTicket);
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

Configure the Application Cookie to use the custom implementation:

```csharp
services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    options.SessionStore = new CustomTicketStore(optionsBuilder);
});
```

## User Interface

I created a simple UI with a list of the sessions for the user and a logout button. If the user clicks logout, all we need to do is delete the entry from the database and the user is no longer signed in. At this point the cookie is still in the browser, but it is not valid anymore.

![image](/posts/images/SessionList.png)

## Summary

Let’s talk about the pros and cons of implementing a custom `ITicketStore`:

### PROS:
- List sessions per user
- Remote logout
- Invalidate or blacklist a session server side

### CONS: 
- Depending on the implementation users might get logged out while restarting the service
- Server now has session information
- More difficult to scale because the session state needs to be shared between instances

At this time, I think I got the MVP working but there is still work to be done. Like the Facebooks user interface, I want to add some metadata to the user's session. For example, the browser they used to login. But currently, I am not sure where and how I shall attack this. I am not sure about the performance of this solution, since we hit the database a lot in this implementation. We would probably need some caching mechanism in place.

This was the first time I implemented this feature and it seems to work very well. But I am interested in your opinions, if you have any suggestions or improvements please let me know.

If you like this blog post drop a comment or buy me a coffee at the bottom of the page ;-)
