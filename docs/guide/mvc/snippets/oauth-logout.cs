app.Map("/logout", builder =>
{
    builder.Run(async context =>
    {
        // Sign the user out of the authentication middleware (i.e. it will clear the Auth cookie)
        await context.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        // Redirect the user to the home page after signing out
        context.Response.Redirect("/");
    });
});
