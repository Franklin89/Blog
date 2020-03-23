Title: Use NodaTime to replace C# DateTime
Published: 03/23/2020 08:30
Lead: It is hard to get your date and times right
Tags: 
    - .NET Core
    - EF Core
    - NodaTime
    - C#
    - DateTime
---

As a developer you have probably had many discussions in your teams all around date and times. In this blog post I wanted to share some insights of how to make sure you get your time zones right in a globally distributed system. In my opinion it is one of the hardest tasks to get right, also due to the cloud, you do not know where your servers will be hosted and their local time settings. Also you might not know from where the client will access your application and you want to present the date and time in his local time zone. If you want to show the correct timestamp to the user according to his time zone setting, the C# DateTime construct would be enough for most of the scenarios, but it does not supply you with a nice and understandable interface. If you are interested in more details, you should read the following blog post by Jon Skeet ([https://codeblog.jonskeet.uk/2019/03/27/storing-utc-is-not-a-silver-bullet/](https://codeblog.jonskeet.uk/2019/03/27/storing-utc-is-not-a-silver-bullet/)).

### What is the main issue with DateTime?

> Noda Time exists for .NET for the same reason that Joda Time exists for Java: the built-in libraries for handling dates and times are inadequate. Both platforms provide far too few types to represent date and time values in a way which encourages the developer to really consider what kind of data they're dealing with... which in turn makes it hard to treat the data consistently.

So basically the main problem is not the construct itself but how developers are using it and that it is not powerful and meaningful enough.

### Why should we consider switching to NodaTime?

I was working on a project that had many date and time properties all over the place, especially also dates and times in the future. If you have read the blog post from Jon Skeet, this can lead to the biggest issues. Most developers knew that dates should be stored in UTC not everyone was doing it this way and also as Jon Skeet wrote: _"Storing UTC is not a silver bullet"_. Another issue is that if you are using DateTime and EF Core, by default it always creates a `datetime2` column type even though sometimes you only need the date or time part. Of course you can configure this, but how many times have you forgotten that? You will lose any information in the database of the type of DateTime, so when you get an object from EF Core the DateTime kind might not be specified as UTC, even though you might have stored it this way. Many things can go wrong here. The other issue that I faced in this project that some used the extension methods `ToUniversalTime` or `ToLocalTime`, which can have effects that if not properly handled will break your application.

### NodaTime

The first thing every engineer will do is try and create an abstraction for the C# DateTime and most likely succeed sooner or later. But why reinvent the wheel if there is a great solution out there.

NodaTime has different types to represent different types of dates and times. This is a big difference to the C# DateTime construct. The [NodaTime documentation](https://nodatime.org/2.4.x/userguide/rationale) is very thorough and detailed and should be your main guidance.

#### When to choose which type?

If you are wondering which type to use for which kind of property, [this](https://nodatime.org/2.4.x/userguide/type-choices) docs page has all the relevant information.

To summarize it in a few words, there are two sources of date and time data: system clocks, and the user. System generated values should be represented as `Instant` values. On the other hand user generated values should be represented as one of the following:

- ZonedDateTime
- OffsetDateTime
- LocalDateTime
- LocalDate
- LocalTime
- OffsetDate
- OffsetTime

### An example what changed in our source code

#### Before

```csharp
public class Event
{
    ...

    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    
    ...

    public DateTime Created { get; set; }
    public string CreatedBy { get; set; }

    public DateTime? Modified { get; set; }
    public string ModifiedBy { get; set; }
}
```

#### After

```csharp
public class Event
{
    ...

    public LocalDateTime? Start { get; set; }
    public LocalDateTime? End { get; set; }

    ...

    public Instant Created { get; set; }
    public string CreatedBy { get; set; }

    public Instant? Modified { get; set; }
    public string ModifiedBy { get; set; }
}
```

### Custom Clock Service

To reduce the mistakes done by developers, it is simple to create a custom clock service that can be injected into the code, that holds a few meaningful methods so that they can work with date and times.

```csharp
public interface IClockService
{
    DateTimeZone TimeZone { get; }

    Instant Now { get; }

    LocalDateTime LocalNow { get; }

    Instant ToInstant(LocalDateTime local);

    LocalDateTime ToLocal(Instant instant);
}
```

With this in place we can have a single service that is responsible for all date and time conversions and also for a developer to get the `Now` and the `LocalNow`. Which we can depend on the logged in user or configuration of the application.

```csharp
public class ClockService : IClockService
{
    public class ClockService : IClockService
    {
        private readonly IClock _clock;

        public DateTimeZone TimeZone { get; private set; }

        public ClockService()
            : this(SystemClock.Instance)
        {
        }

        public ClockService(IClock clock)
        {
            _clock = clock;

            // NOTE: Get the current users timezone here instead of hard coding it...
            TimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Zurich");
        }

        public Instant Now
            => _clock.GetCurrentInstant();

        public LocalDateTime LocalNow
            => Now.InZone(TimeZone).LocalDateTime;

        public Instant? ToInstant(LocalDateTime? local)
            => local?.InZone(TimeZone, Resolvers.LenientResolver).ToInstant();

        public LocalDateTime? ToLocal(Instant? instant)
            => instant?.InZone(TimeZone).LocalDateTime;
    }
}
```

### Extension methods

Create some extension methods which fit your use case and offer them to the developers in your team. This will make the code much more readable and it will make sure formats and so on will be the same across your applications. Here I have some samples of what you might need.

```csharp
public static class NodaTimeExtensions
{
    public static string ToDateTimeString(this LocalDateTime? local)
    {
        if (local == null)
        {
            return null;
        }

        return local.Value.ToDateTimeString();
    }

    public static string ToDateTimeString(this LocalDateTime local)
    {
        var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
        return local.ToString($"{culture.DateTimeFormat.ShortDatePattern} {culture.DateTimeFormat.ShortTimePattern}", culture);
    }

    public static string ToShortDateString(this LocalDate? local)
    {
        if (local == null)
        {
            return null;
        }

        return local.Value.ToShortDateString();
    }

    public static string ToShortDateString(this LocalDate local)
    {
        var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
        return local.ToString(culture.DateTimeFormat.ShortDatePattern, culture);
    }
}
```

### NodaTime with EF Core and SQL Server

If you are using SQL Server and EF Core to store your data you need a way to convert the NodaTime types into SQL Server types. For example `LocalDate` should be a `date` type in the database. Fortunately there is an [open source package](https://github.com/StevenRasmussen/EFCore.SqlServer.NodaTime) that can be used for this.

Add the following packed to your project:

> SimplerSoftware.EntityFrameworkCore.SqlServer.NodaTime

After the NuGet Package has been restored all you need to do is update your SQL Server configuration as follows:

```csharp
using Microsoft.EntityFrameworkCore.SqlServer.NodaTime.Extensions;

options.UseSqlServer("your DB Connection",
                    x => x.UseNodaTime());
```

Then create your migrations, update the database and also you might have to update the queries. Depending how far along you are with your project.

### Summary
NodaTime is a great library to use if you are building applications where you are handling a lot of date and times. Not only if they are across different time zones, but also if you need to things like time travel and display data at a certain time of the day. With using a service like the `IClockService` you will make the developers life easier because it will provide one central point for the developer. I have used these patterns successfully in different projects and  will continue using them to make sure that I no longer have to chase UTC and Local date and time issues. I always knew this subject is not easy to get right. I still have a lot to learn in this area, any feedback is appreciated.

If you like this blog post drop a comment or buy me a coffee at the bottom of the page <i class="fa fa-coffee"></i>