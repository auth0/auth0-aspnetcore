using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Auth0.AspNetCore.Authentication.Events
{
    public class Auth0Events
    {
        /// <summary>
        /// Invoked if exceptions are thrown during request processing. The exceptions will be re-thrown after this event unless suppressed.
        /// </summary>
        public Func<AuthenticationFailedContext, Task> OnAuthenticationFailed { get; set; } =
            context => Task.FromResult(0);

        /// <summary>
        /// Invoked after security token validation if an authorization code is present in the protocol message.
        /// </summary>
        public Func<AuthorizationCodeReceivedContext, Task> OnAuthorizationCodeReceived { get; set; } =
            context => Task.FromResult(0);

        /// <summary>
        /// Invoked when a protocol message is first received.
        /// </summary>
        public Func<MessageReceivedContext, Task> OnMessageReceived { get; set; } = context => Task.FromResult(0);

        /// <summary>
        /// Invoked before redirecting to the identity provider to authenticate.
        /// </summary>
        public Func<RedirectContext, Task> OnRedirectToIdentityProvider { get; set; } = context => Task.FromResult(0);

        /// <summary>
        /// Invoked before redirecting to the identity provider to sign out.
        /// </summary>
        public Func<RedirectContext, Task> OnRedirectToIdentityProviderForSignOut { get; set; } =
            context => Task.FromResult(0);

        /// <summary>
        /// Invoked when there is a remote failure
        /// </summary>
        public Func<FailureContext, Task> OnRemoteFailure { get; set; } = context => Task.FromResult(0);

        /// <summary>
        /// Invoked after the ticket has been received.
        /// </summary>
        public Func<TicketReceivedContext, Task> OnTicketReceived { get; set; } = context => Task.FromResult(0);

        /// <summary>
        /// Invoked after "authorization code" is redeemed for tokens at the token endpoint.
        /// </summary>
        public Func<TokenResponseReceivedContext, Task> OnTokenResponseReceived { get; set; } =
            context => Task.FromResult(0);

        /// <summary>
        /// Invoked when an IdToken has been validated and produced an AuthenticationTicket.
        /// </summary>
        public Func<TokenValidatedContext, Task> OnTokenValidated { get; set; } = context => Task.FromResult(0);

        /// <summary>
        /// Invoked when user information is retrieved from the UserInfoEndpoint.
        /// </summary>
        public Func<UserInformationReceivedContext, Task> OnUserInformationReceived { get; set; } =
            context => Task.FromResult(0);

        public virtual Task AuthenticationFailed(AuthenticationFailedContext context) => OnAuthenticationFailed(context);

        public virtual Task AuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
            => OnAuthorizationCodeReceived(context);

        public virtual Task MessageReceived(MessageReceivedContext context) => OnMessageReceived(context);

        public virtual Task RedirectToIdentityProvider(RedirectContext context) => OnRedirectToIdentityProvider(context);

        public virtual Task RedirectToIdentityProviderForSignOut(RedirectContext context)
            => OnRedirectToIdentityProviderForSignOut(context);

        public virtual Task RemoteFailure(FailureContext context) => OnRemoteFailure(context);

        public virtual Task TicketReceived(TicketReceivedContext context) => OnTicketReceived(context);

        public virtual Task TokenResponseReceived(TokenResponseReceivedContext context)
            => OnTokenResponseReceived(context);

        public virtual Task TokenValidated(TokenValidatedContext context) => OnTokenValidated(context);

        public virtual Task UserInformationReceived(UserInformationReceivedContext context)
            => OnUserInformationReceived(context);
    }
}