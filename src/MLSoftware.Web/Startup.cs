﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using System.IO;

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
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var root = HostingEnvironment.ContentRootPath;
            root = Path.Combine(root, "App_Data");
            Directory.CreateDirectory(root);

            var connection = Configuration.GetConnectionString("DefaultConnection");
            connection = string.Format(connection, root);

            services.AddDbContext<BlogContext>(options => 
            {
                options.UseSqlite(connection);
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
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
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
