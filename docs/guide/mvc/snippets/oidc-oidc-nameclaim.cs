app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions("Auth0")
{
    ClaimsIssuer = "Auth0"

    Authority = "https://YOUR_AUTH0_DOMAIN",
    ClientId = "APP_CLIENT_ID",
    ClientSecret = "APP_CLIENT_SECRET",

    AutomaticAuthenticate = true, 
    AutomaticChallenge = true,

    ResponseType = "code",

    CallbackPath = new PathString("/signin-auth0"),
    
    Events = new OpenIdConnectEvents
    {
        OnTicketReceived = context =>
        {
            // Get the ClaimsIdentity
            var identity = context.Principal.Identity as ClaimsIdentity;
            if (identity != null)
            {
                if (!context.Principal.HasClaim(c => c.Type == ClaimTypes.Name) &&
                    identity.HasClaim(c => c.Type == "name"))
                    identity.AddClaim(new Claim(ClaimTypes.Name, identity.FindFirst("name").Value));
            }

            return Task.FromResult(0);
        }
    }
});