using Microsoft.AspNetCore.Builder;

namespace Web.Boot.Actions
{
    public class UseEndpointsHandlerAction : ConfigureAppBase
    {
        public override int Priority => (int) KnownMiddlewarePriorities.Endpoints;

        public override void Configure(IApplicationBuilder app)
        {
            if (!Startup.ContainsMatchingContributingTypes<IConfigureEndpoints>())
                return;

            app.UseEndpoints(endpoints =>
            {
                Startup.ConfigureWithContributingTypesInternal<IConfigureEndpoints>(e =>
                {
                    e.Configure(endpoints);
                });
            });
        }
    }
}