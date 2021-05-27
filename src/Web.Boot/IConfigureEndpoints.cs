using Microsoft.AspNetCore.Routing;

namespace Web.Boot
{
    public interface IConfigureEndpoints : IHasPriority
    {
        void Configure(IEndpointRouteBuilder endpoints);
    }

    public abstract class ConfigureEndpointsBase : IConfigureEndpoints
    {
        public virtual int Priority => 0;

        public abstract void Configure(IEndpointRouteBuilder endpoints);
    }
}