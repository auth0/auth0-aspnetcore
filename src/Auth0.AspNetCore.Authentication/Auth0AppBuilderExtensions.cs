// ReSharper disable CheckNamespace

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods to add Auth0 authentication capabilities to an HTTP application pipeline.
    /// </summary>
    public static class Auth0AppBuilderExtensions
    {
        public static IApplicationBuilder UseAuth0(this IApplicationBuilder app, Action<Auth0AuthenticationOptions> configureOptions = null)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            Auth0AuthenticationOptions options = new Auth0AuthenticationOptions();

            if (configureOptions != null)
                configureOptions(options);

            return app.UseAuth0(options);
        }

        public static IApplicationBuilder UseAuth0(this IApplicationBuilder app, Auth0AuthenticationOptions options)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                LoginPath = new PathString("/login"),
                LogoutPath = new PathString("/logout")
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions("Auth0")
            {
                Authority = $"https://{options.Domain}",
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                CallbackPath = options.CallbackPath,
                ClientId = options.ClientId,
                ClientSecret = options.ClientSecret,
                ClaimsIssuer = options.ClaimsIssuer,
                GetClaimsFromUserInfoEndpoint = options.GetClaimsFromUserInfoEndpoint,
                ResponseType = "code",
                SaveTokens = options.SaveTokens,
                Events = new OpenIdConnectEvents
                {
                    OnTicketReceived = context =>
                    {
                        var identity = context.Principal.Identity as ClaimsIdentity;

                        if (identity != null)
                        {
                            if (!context.Principal.HasClaim(c => c.Type == ClaimTypes.Name) &&
                                identity.HasClaim(c => c.Type == "name"))
                                identity.AddClaim(new Claim(ClaimTypes.Name, identity.FindFirst("name").Value));
                        }

                        return Task.FromResult(true);
                    }
                }
            });

            return app;
        }
    }
}