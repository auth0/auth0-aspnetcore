using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Auth0.AspNetCore.Authentication.Events;

namespace AspNetCoreMvcSample
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddUserSecrets()
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Auth0 services
            services.AddAuth0();

            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuth0(options =>
            {
                options.Domain = Configuration["auth0:domain"];
                options.ClientId = Configuration["auth0:clientId"];
                options.ClientSecret = Configuration["auth0:clientSecret"];
                options.SaveTokens = true;
                options.Events = new Auth0Events
                {
                    OnAuthenticationFailed = context =>
                    {
                        return Task.FromResult(0);
                    },
                    OnAuthorizationCodeReceived = context =>
                    {
                        return Task.FromResult(0);
                    },
                    OnMessageReceived = context =>
                    {
                        return Task.FromResult(0);
                    },
                    OnRedirectToIdentityProvider = context =>
                    {
                        return Task.FromResult(0);
                    },
                    OnRedirectToIdentityProviderForSignOut = context =>
                    {
                        return Task.FromResult(0);
                    },
                    OnRemoteFailure = context =>
                    {
                        return Task.FromResult(0);
                    },
                    OnTicketReceived = context =>
                    {
                        return Task.FromResult(0);
                    },
                    OnTokenResponseReceived = context =>
                    {
                        return Task.FromResult(0);
                    },
                    OnTokenValidated = context =>
                    {
                        return Task.FromResult(0);
                    },
                    OnUserInformationReceived = context =>
                    {
                        return Task.FromResult(0);
                    }
                };
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
