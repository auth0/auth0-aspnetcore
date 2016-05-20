app.UseCookieAuthentication(new CookieAuthenticationOptions
{
    AutomaticAuthenticate = true,
    AutomaticChallenge = true,
    LoginPath = new PathString("/login"),
    LogoutPath = new PathString("/logout")
});