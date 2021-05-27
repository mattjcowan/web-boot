using Microsoft.AspNetCore.Authorization;

namespace Web.Boot
{
    public interface IConfigureAuthorizationOptions : IHasPriority
    {
        void Configure(AuthorizationOptions options);
    }

    public abstract class ConfigureAuthorizationOptionsBase : IConfigureAuthorizationOptions
    {
        public virtual int Priority => 0;

        public abstract void Configure(AuthorizationOptions options);
    }
}