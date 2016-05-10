using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System;
using Microsoft.AspNetCore.Authorization;

namespace WebApiSample
{
    public class Startup
    {
        private IConfigurationRoot Configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
                    .AddJsonFormatters()
                    .AddAuthorization(auth =>
                    {
                        auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                            .RequireAuthenticatedUser().Build());
                    });
                    
            var builder = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();
                
            Configuration = builder.Build();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IHostingEnvironment env)
        {
            var certificateData = this.Configuration.GetSection("Auth0:Certificate")?.Value ?? "";
            var certificate = new X509Certificate2(Convert.FromBase64String(certificateData));
            
            var options = new JwtBearerOptions();
            
            options.Audience = this.Configuration.GetSection("Auth0:ClientId")?.Value ?? "";
            options.Authority = this.Configuration.GetSection("Auth0:Domain")?.Value ?? "";
            
            options.AutomaticChallenge = true;
            options.AutomaticAuthenticate = true;
            
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context => 
                {
                    System.Console.WriteLine("error" + context.Exception);
                    return Task.FromResult(0);
                },
                OnTokenValidated = context =>
                {
                    var claimsIdentity = context.Ticket.Principal.Identity as ClaimsIdentity;
                    claimsIdentity.AddClaim(new Claim("id_token", 
                        context.Request.Headers["Authorization"][0].Substring(context.Ticket.AuthenticationScheme.Length + 1)));
                    
                    return Task.FromResult(0);
                }
            };
            
            app.UseJwtBearerAuthentication(options);
            app.UseMvc();
        }
    }
}
