using System;
using System.Threading.Tasks;
using System.Security.Claims;

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;

using WebApp.Properties;

using Microsoft.Dnx.Runtime;

namespace WebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IApplicationEnvironment app)
        {
            // Setup configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(app.ApplicationBasePath)
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddWebEncoders();
            services.AddDataProtection();

            services.Configure<Auth0Settings>(Configuration.GetSection("Auth0"));
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<SharedAuthenticationOptions>(options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });


            var settings = Configuration.Get<Auth0Settings>("Auth0");
            services.ConfigureAuth0(settings.Domain, settings.ClientId, settings.ClientSecret, settings.RedirectUri, notification =>
            {
                var identity = notification.AuthenticationTicket.Principal.Identity as ClaimsIdentity;
                if (identity.HasClaim(c => c.Type == "name"))
                    identity.AddClaim(new Claim(ClaimTypes.Name, identity.FindFirst("name").Value));
                identity.AddClaim(new Claim("tenant", "12345"));
                identity.AddClaim(new Claim("custom-claim", "custom-value"));
                return Task.FromResult(true);
            });
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            
            app.UseStaticFiles();

            app.UseCookieAuthentication(options =>
            {
                options.AutomaticAuthentication = true;
                options.LoginPath = new PathString("/Login");
            });

            app.UseAuth0();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
