using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Auth0.AspNetCore.Authentication
{
    public class Auth0AuthenticationOptions
    {
        public Auth0AuthenticationOptions()
            : this(Auth0AuthenticationDefaults.AuthenticationScheme)
        {
        }

        public Auth0AuthenticationOptions(string authenticationScheme)
        {
            AuthenticationScheme = authenticationScheme;
            CallbackPath = new PathString(Auth0AuthenticationDefaults.CallbackPath);

            Scope.Add("openid");
        }


        /// <summary>
        /// The AuthenticationScheme in the options corresponds to the logical name for a particular authentication scheme. A different
        /// value may be assigned in order to use the same authentication middleware type more than once in a pipeline.
        /// </summary>
        public string AuthenticationScheme { get; set; }

        /// <summary>
        /// The request path within the application's base path where the user-agent will be returned.
        /// The middleware will process this request when it arrives.
        /// </summary>
        public PathString CallbackPath { get; set; }

        /// <summary>
        /// Gets or sets the issuer that should be used for any claims that are created
        /// </summary>
        public string ClaimsIssuer { get;  set; }
        
        /// <summary>
        /// Get or sets the Client ID of the Auth0 Application.
        /// </summary>
        /// <remarks>
        /// To find the Auth0 Client ID, go to the <see href="https://manage.auth0.com/#/applications">Applications section of the Auth0 Dashboard</see>, select the Application you
        /// want to use and copy the value of the Client ID field.
        /// </remarks>
        public string ClientId { get; set; }

        /// <summary>
        /// Get or sets the Client Secret of the Auth0 Application.
        /// </summary>
        /// <remarks>
        /// To find the Auth0 Client Secret, go to the <see href="https://manage.auth0.com/#/applications">Applications section of the Auth0 Dashboard</see>, select the Application you
        /// want to use and copy the value of the Client Secret field.
        /// </remarks>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Get or sets the Domain of the Auth0 Application.
        /// </summary>
        /// <remarks>
        /// To find the Auth0 Domain, go to the <see href="https://manage.auth0.com/#/applications">Applications section of the Auth0 Dashboard</see>, select the Application you
        /// want to use and copy the value of the Domain field.
        /// </remarks>
        public object Domain { get; set; }

        /// <summary>
        /// Boolean to set whether the middleware should go to user info endpoint to retrieve additional claims or not after creating an identity from id_token received from token endpoint.
        /// </summary>
        public bool GetClaimsFromUserInfoEndpoint { get; set; }

        /// <summary>
        /// Defines whether access and refresh tokens should be stored in the
        /// <see cref="Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties"/> after a successful authorization.
        /// This property is set to <c>false</c> by default to reduce
        /// the size of the final authentication cookie.
        /// </summary>
        public bool SaveTokens { get; set; }

        /// <summary>
        /// Gets the list of permissions to request.
        /// </summary>
        public ICollection<string> Scope { get; } = new HashSet<string>();
    }
}