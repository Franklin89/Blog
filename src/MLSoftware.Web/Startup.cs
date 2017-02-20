using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using DbUp;
using System.Reflection;
using System;
using Microsoft.AspNetCore.Rewrite;
using MLSoftware.Web.Services;

namespace MLSoftware.Web
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        public Startup(IHostingEnvironment env)
        {
            HostingEnvironment = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(HostingEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{HostingEnvironment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if(HostingEnvironment.IsDevelopment())
            {
                builder.AddUserSecrets();
            }

            Configuration = builder.Build();

            // Configure Database
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var upgrader =
                DeployChanges.To
                    .SQLiteDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(typeof(Startup).GetTypeInfo().Assembly)
                    .LogToConsole()
                    .Build();

            var result = upgrader.PerformUpgrade();
            if (!result.Successful)
            {
                throw new Exception("Failed to migrate the database.");
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Adds services required for using options.
            services.AddOptions();

            // Register the IConfiguration instance which MyOptions binds against.
            services.Configure<MailSettings>(Configuration.GetSection(nameof(MailSettings)));

            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<BlogContext>(options =>
            {
                options.UseSqlite(connectionString);
            });

            services.AddAuthentication(options => options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
            services.AddAuthorization(options =>
                options.AddPolicy("Admin", policyBuilder =>
                    policyBuilder.RequireClaim(
                        ClaimTypes.Email,
                        Configuration["Authorization:AdminUsers"].Split(',')
                    )
                )
            );

            // Add caching
            services.AddMemoryCache();

            // Add framework services
            services.AddMvc();

            // Register application services
            services.AddSingleton<IObjectMapper, SimpleObjectMapper>();
            services.AddSingleton<IFeedService, FeedService>();

            var markdown = new Markdown.Markdown();
            markdown.UseExtensions();
            services.AddSingleton<Markdown.IMarkdown>(markdown);

            services.AddScoped<IEmailService, EmailService>();

            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseRewriter(new RewriteOptions()
                .AddIISUrlRewrite(env.ContentRootFileProvider, "urlRewrite.config"));

            app.UseStaticFiles();
            app.UseStatusCodePages();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                ClientId = Configuration["Authentication:AzureAd:ClientId"],
                Authority = Configuration["Authentication:AzureAd:AADInstance"] + Configuration["Authentication:AzureAd:TenantId"],
                ResponseType = OpenIdConnectResponseType.IdToken,
                SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme
            });

            app.Use((context, next) => context.Request.Path.StartsWithSegments("/ping")
                ? context.Response.WriteAsync("pong")
                : next()
            );

            app.UseMvc();
        }
    }
}
