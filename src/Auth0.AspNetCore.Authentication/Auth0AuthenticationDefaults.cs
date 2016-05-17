using Microsoft.AspNetCore.Builder;

namespace Auth0.AspNetCore.Authentication
{
    /// <summary>
    /// Default values used by the Dropbox authentication middleware.
    /// </summary>
    public class Auth0AuthenticationDefaults
    {
        /// <summary>
        /// Default value for <see cref="Auth0AuthenticationOptions.AuthenticationScheme"/>.
        /// </summary>
        public const string AuthenticationScheme = "Auth0";

        /// <summary>
        /// Default value for <see cref="Auth0AuthenticationOptions.ClaimsIssuer"/>.
        /// </summary>
        public const string Issuer = "Auth0";

        /// <summary>
        /// Default value for <see cref="Auth0AuthenticationOptions.CallbackPath"/>.
        /// </summary>
        public const string CallbackPath = "/signin-auth0";
    }
}