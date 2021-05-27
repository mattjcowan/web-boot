using Microsoft.AspNetCore.Builder;

namespace Web.Boot
{
    public interface IConfigureApp : IHasPriority
    {
        void Configure(IApplicationBuilder app);
    }

    public abstract class ConfigureAppBase : IConfigureApp
    {
        public virtual int Priority => 0;

        public abstract void Configure(IApplicationBuilder app);
    }
}