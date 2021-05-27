using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.JSInterop;

namespace Web.Boot.Actions
{
    public class EndpointGetFallbackAction : ConfigureEndpointsBase
    {
        public override int Priority => int.MaxValue;

        public override void Configure(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/{**pathInfo}", async context =>
            {
                var pathInfo = context.Request.RouteValues["pathInfo"];
                await context.Response.WriteAsync($"Hello from boot! (at /{pathInfo})");
            });
        }
    }
}