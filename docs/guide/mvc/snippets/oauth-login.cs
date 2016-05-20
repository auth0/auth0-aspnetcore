app.Map("/login", builder =>
{
    builder.Run(async context =>
    {
        // Return a challenge to invoke the Auth0 authentication scheme
        await context.Authentication.ChallengeAsync("Auth0", new AuthenticationProperties() { RedirectUri = "/" });
    });
});