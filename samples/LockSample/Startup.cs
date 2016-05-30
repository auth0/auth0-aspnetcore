using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Net.Http;
using System.Security.Claims;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace LockSample
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
            services.AddSingleton<IConfiguration>(Configuration);

            // Add authentication services
            services.AddAuthentication(
                options => options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);

            // Configure OAuth2
            services.Configure<OAuthOptions>(options =>
            {
                options.AutomaticAuthenticate = false;
                options.AutomaticChallenge = false;

                // We need to specify an Authentication Scheme
                options.AuthenticationScheme = "Auth0";

                // Configure the Auth0 Client ID and Client Secret
                options.ClientId = Configuration["auth0:clientId"];
                options.ClientSecret = Configuration["auth0:clientSecret"];

                // Set the callback path, so Auth0 will call back to http://localhost:5000/signin-auth0 
                // Also ensure that you have added the URL as an Allowed Callback URL in your Auth0 dashboard 
                options.CallbackPath = new PathString("/signin-auth0");

                // Configure the Auth0 endpoints                
                options.AuthorizationEndpoint = $"https://{Configuration["auth0:domain"]}/authorize";
                options.TokenEndpoint = $"https://{Configuration["auth0:domain"]}/oauth/token";
                options.UserInformationEndpoint = $"https://{Configuration["auth0:domain"]}/userinfo";

                // Set scope to openid. See https://auth0.com/docs/scopes
                //options.Scope = { "openid" };

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        // Retrieve user info
                        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();

                        // Extract the user info object
                        var user = JObject.Parse(await response.Content.ReadAsStringAsync());

                        // Add the Name Identifier claim
                        var userId = user.Value<string>("user_id");
                        if (!string.IsNullOrEmpty(userId))
                        {
                            context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId,
                                ClaimValueTypes.String, context.Options.ClaimsIssuer));
                        }

                        // Add the Name claim
                        var email = user.Value<string>("email");
                        if (!string.IsNullOrEmpty(email))
                        {
                            context.Identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, email,
                                ClaimValueTypes.String, context.Options.ClaimsIssuer));
                        }
                    }
                };
            });

            // Configure OIDC
            //services.Configure<OpenIdConnectOptions>(options =>
            //{
            //    options.AuthenticationScheme = "Auth0";
                
            //    // Set the authority to your Auth0 domain
            //    options.Authority = $"https://{Configuration["auth0:domain"]}";

            //    // Configure the Auth0 Client ID and Client Secret
            //    options.ClientId = Configuration["auth0:clientId"];
            //    options.ClientSecret = Configuration["auth0:clientSecret"];

            //    // Do not automatically authenticate and challenge
            //    options.AutomaticAuthenticate = false;
            //    options.AutomaticChallenge = false;

            //    // Set response type to code
            //    options.ResponseType = "code";

            //    // Set the callback path, so Auth0 will call back to http://localhost:5000/signin-auth0 
            //    // Also ensure that you have added the URL as an Allowed Callback URL in your Auth0 dashboard 
            //    options.CallbackPath = new PathString("/signin-auth0");

            //    // Configure the Claims Issuer to be Auth0
            //    options.ClaimsIssuer = "Auth0";

            //    options.Events = new OpenIdConnectEvents
            //    {
            //        OnTicketReceived = context =>
            //        {
            //            // Get the ClaimsIdentity
            //            var identity = context.Principal.Identity as ClaimsIdentity;
            //            if (identity != null)
            //            {
            //                // Add the Name ClaimType. This is required if we want User.Identity.Name to actually return something!
            //                if (!context.Principal.HasClaim(c => c.Type == ClaimTypes.Name) &&
            //                    identity.HasClaim(c => c.Type == "name"))
            //                    identity.AddClaim(new Claim(ClaimTypes.Name, identity.FindFirst("name").Value));
            //            }

            //            return Task.FromResult(0);
            //        }
            //    };
            //});

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

            // Add the cookie middleware
            app.UseCookieAuthentication(options: new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                LoginPath = new PathString("/account/login"),
                LogoutPath = new PathString("/account/logout")
            });

            // Add the OAuth2 middleware
            var options = app.ApplicationServices.GetRequiredService<IOptions<OAuthOptions>>();
            app.UseOAuthAuthentication(options.Value);

            // Add the OIDC middleware
            //var options = app.ApplicationServices.GetRequiredService<IOptions<OpenIdConnectOptions>>();
            //app.UseOpenIdConnectAuthentication(options.Value);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}