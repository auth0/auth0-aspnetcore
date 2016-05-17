// ReSharper disable CheckNamespace

using System;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods to <see cref="IServiceCollection"/> for configuring Auth0 authentication.
    /// </summary>
    public static class Auth0ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuth0(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddAuthentication(
                options => options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);

            return services;
        }
    }
}