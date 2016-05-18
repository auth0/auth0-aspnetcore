// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{
    // Add Auth0 services
    services.AddAuth0();

    // Add framework services.
    services.AddMvc();
}