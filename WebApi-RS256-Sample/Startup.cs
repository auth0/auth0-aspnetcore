using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Authentication.OAuthBearer;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using System.Threading.Tasks;

namespace WebApiSample
{
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
            services.AddMvc();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory)
        {
            loggerfactory.AddConsole();

            var logger = loggerfactory.CreateLogger("Auth0");
            var settings = app.ApplicationServices.GetService<IOptions<Auth0Settings>>();

            app.UseOAuthBearerAuthentication(options =>
            {
                options.Audience = settings.Options.ClientId;
                options.Authority = $"https://{settings.Options.Domain}";
                options.Notifications = new OAuthBearerAuthenticationNotifications
                {
                    AuthenticationFailed = context =>
                    {
                        logger.LogError("Authentication failed.", context.Exception);
                        return Task.FromResult(0);
                    },
                    // OPTIONAL: you can read/modify the claims that are populated based on the JWT
                    // Check issue status: https://github.com/aspnet/Security/issues/140
                    /*SecurityTokenValidated = context =>
					{
						var claimsIdentity = context.AuthenticationTicket.Principal.Identity as ClaimsIdentity;
						// ensure name claim
						claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, claimsIdentity.FindFirst("name").Value));
						return Task.FromResult(0);
					}*/
                };
            });
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
