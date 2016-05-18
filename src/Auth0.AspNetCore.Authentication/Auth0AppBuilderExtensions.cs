// ReSharper disable CheckNamespace

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Auth0.AspNetCore.Authentication;
using Auth0.AspNetCore.Authentication.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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
            if (options.Events == null)
            {
                options.Events = new Auth0Events();
            }

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
                AutomaticAuthenticate = false,
                AutomaticChallenge = false,
                CallbackPath = options.CallbackPath,
                ClientId = options.ClientId,
                ClientSecret = options.ClientSecret,
                ClaimsIssuer = options.ClaimsIssuer,
                GetClaimsFromUserInfoEndpoint = options.GetClaimsFromUserInfoEndpoint,
                ResponseType = "code",
                SaveTokens = options.SaveTokens,
                Events = new OpenIdConnectEvents
                {
                    OnAuthenticationFailed = context => options.Events.AuthenticationFailed(context),
                    OnAuthorizationCodeReceived = context => options.Events.AuthorizationCodeReceived(context),
                    OnMessageReceived = context => options.Events.MessageReceived(context),
                    OnRedirectToIdentityProvider = context => options.Events.RedirectToIdentityProvider(context),
                    OnRedirectToIdentityProviderForSignOut = context => options.Events.RedirectToIdentityProviderForSignOut(context),
                    OnRemoteFailure = context => options.Events.RemoteFailure(context),
                    OnTicketReceived = context =>
                    {
                        var identity = context.Principal.Identity as ClaimsIdentity;

                        if (identity != null)
                        {
                            if (!context.Principal.HasClaim(c => c.Type == ClaimTypes.Name) &&
                                identity.HasClaim(c => c.Type == "name"))
                                identity.AddClaim(new Claim(ClaimTypes.Name, identity.FindFirst("name").Value));
                        }

                        return options.Events.TicketReceived(context);
                    },
                    OnTokenResponseReceived = context => options.Events.TokenResponseReceived(context),
                    OnTokenValidated = context => options.Events.TokenValidated(context),
                    OnUserInformationReceived = context => options.Events.UserInformationReceived(context)
                }
            });

            app.Map("/login", builder =>
            {
                builder.Run(async context =>
                {
                    await context.Authentication.ChallengeAsync("Auth0", new AuthenticationProperties() { RedirectUri = "/" });
                });
            });

            app.Map("/logout", builder =>
            {
                builder.Run(async context =>
                {
                    await context.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    context.Response.Redirect("/");
                });
            });

            return app;
        }
    }
}