public void ConfigureServices(IServiceCollection services)
{
    // Add Auth0 services
    services.AddAuth0();

    // Add framework services.
    services.AddMvc();
}