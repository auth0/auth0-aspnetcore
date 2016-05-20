app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions("Auth0")
{
    ClaimsIssuer = "Auth0"

    Authority = "https://YOUR_AUTH0_DOMAIN",
    ClientId = "APP_CLIENT_ID",
    ClientSecret = "APP_CLIENT_SECRET",

    AutomaticAuthenticate = false, 
    AutomaticChallenge = false,

    ResponseType = "code",

    CallbackPath = new PathString("/signin-auth0")
});