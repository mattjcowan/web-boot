using Microsoft.AspNetCore.Builder;

namespace Web.Boot.Actions
{
    public class UseRoutingAction : ConfigureAppBase
    {
        public override int Priority => (int) KnownMiddlewarePriorities.Routing;

        public override void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
        }
    }
}