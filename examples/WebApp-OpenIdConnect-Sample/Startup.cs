namespace WebApp
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Microsoft.AspNet.Authentication;
    using Microsoft.AspNet.Authentication.Cookies;
    using Microsoft.AspNet.Authentication.OpenIdConnect;
    using Microsoft.AspNet.Builder;
    using Microsoft.AspNet.Diagnostics;
    using Microsoft.AspNet.Hosting;
    using Microsoft.AspNet.Http;
    using Microsoft.Framework.ConfigurationModel;
    using Microsoft.Framework.DependencyInjection;
    using Microsoft.Framework.Logging;
    using Microsoft.Framework.OptionsModel;

    using WebApp.Properties;

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            this.Configuration = new Configuration()
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Auth0Settings>(this.Configuration.GetSubKey("Auth0"));
            services.Configure<AppSettings>(this.Configuration.GetSubKey("AppSettings"));
            services.AddMvc();

            // OpenID Connect Authentication Requires Cookie Auth
            services.AddWebEncoders();
            services.AddDataProtection();
            services.Configure<ExternalAuthenticationOptions>(options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory)
        {
            loggerfactory.AddConsole();

            // Add the following to the request pipeline only in development environment.
            if (env.IsEnvironment("Development"))
            {
                app.UseBrowserLink();
                app.UseErrorPage(ErrorPageOptions.ShowAll);
            }
            else
            {
                app.UseErrorHandler("/Home/Error");
            }

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            
            app.UseCookieAuthentication(options =>
            {
                options.AutomaticAuthentication = true;
                options.LoginPath = new PathString("/Login");
            });

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

            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
