using Microsoft.AspNetCore.Authentication;

namespace Web.Boot
{
    public interface IConfigureAuthenticationOptions : IHasPriority
    {
        void Configure(AuthenticationOptions options);
    }

    public abstract class ConfigureAuthenticationOptionsBase : IConfigureAuthenticationOptions
    {
        public virtual int Priority => 0;

        public abstract void Configure(AuthenticationOptions options);
    }
}