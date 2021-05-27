using Microsoft.AspNetCore.Builder;

namespace Web.Boot.Actions
{
    public class UseStaticFilesAction : ConfigureAppBase
    {
        public override int Priority => (int) KnownMiddlewarePriorities.StaticFiles;

        public override void Configure(IApplicationBuilder app)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}