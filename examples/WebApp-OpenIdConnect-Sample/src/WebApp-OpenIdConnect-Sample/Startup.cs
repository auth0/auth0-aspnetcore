using System;
using System.Threading.Tasks;
using System.Security.Claims;

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.Cookies;

using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Logging.Console;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.OptionsModel;

using WebApp.Properties;

namespace WebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.
            var builder = new ConfigurationBuilder(appEnv.ApplicationBasePath)
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Auth0Settings>(this.Configuration.GetConfigurationSection("Auth0"));
            services.Configure<AppSettings>(this.Configuration.GetConfigurationSection("AppSettings"));
            services.AddMvc();

            // OpenID Connect Authentication Requires Cookie Auth
            services.AddWebEncoders();
            services.AddDataProtection();
            services.Configure<ExternalAuthenticationOptions>(options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseErrorPage();
            }
            else
            {
                app.UseErrorHandler("/Home/Error");
            }
            
            app.UseStaticFiles();

            app.UseCookieAuthentication(options =>
            {
                options.AutomaticAuthentication = true;
                options.LoginPath = new PathString("/Login");
            });

            var logger = loggerFactory.CreateLogger("Auth0");

            app.UseOpenIdConnectAuthentication(options =>
            {
                var settings = app.ApplicationServices.GetService<IOptions<Auth0Settings>>();
                options.ClientId = settings.Options.ClientId;
                options.Authority = $"https://{settings.Options.Domain}";
                if (!String.IsNullOrEmpty(settings.Options.RedirectUri))
                    options.RedirectUri = settings.Options.ClientId;

                options.Notifications = new OpenIdConnectAuthenticationNotifications();
                options.Notifications.SecurityTokenValidated = notification =>
                {
                    var identity = notification.AuthenticationTicket.Principal.Identity as ClaimsIdentity;
                    if (identity.HasClaim(c => c.Type == "name"))
                        identity.AddClaim(new Claim(ClaimTypes.Name, identity.FindFirst("name").Value));
                    identity.AddClaim(new Claim("tenant", "12345"));
                    identity.AddClaim(new Claim("custom-claim", "custom-value"));
                    return Task.FromResult(true);
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
