app.UseOAuthAuthentication(new OAuthOptions
{
    AuthenticationScheme = "Auth0",

    AutomaticAuthenticate = true,
    AutomaticChallenge = true

    ClientId = "APP_CLIENT_ID",
    ClientSecret = "APP_CLIENT_SECRET",

    CallbackPath = new PathString("/signin-auth0"),

    AuthorizationEndpoint = "https://YOUR_AUTH0_DOMAIN/authorize",
    TokenEndpoint = "https://YOUR_AUTH0_DOMAIN/oauth/token",
    UserInformationEndpoint = "https://YOUR_AUTH0_DOMAIN/userinfo",

    Scope = { "openid" },
});
