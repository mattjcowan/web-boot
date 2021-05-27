using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Boot.Actions
{
    public class UseCorsAction : ConfigureAppBase, IConfigureServices
    {
        public override int Priority => (int) KnownMiddlewarePriorities.Cors;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IWebHostEnvironment env)
        {
            var bootConfig = configuration.GetBootConfig();
            if (!bootConfig.EnableCors) return;

            var corsPolicyName = configuration.AsString("boot:cors:PolicyName");

            var allowAnyOrigin = configuration.AsBool("boot:cors:AllowAnyOrigin", true);
            var allowAnyHeader = configuration.AsBool("boot:cors:AllowAnyHeader", true);
            var allowAnyMethod = configuration.AsBool("boot:cors:AllowAnyMethod", true);
            var allowCredentials = configuration.AsBool("boot:cors:AllowCredentials", true);

            var origins = configuration.AsStringArray("boot:cors:Origins", defaultValue: new[] {"*"});
            var headers = configuration.AsStringArray("boot:cors:Headers", defaultValue: new[] {"*"});
            var methods = configuration.AsStringArray("boot:cors:Methods", defaultValue: new[] {"*"});
            var exposedHeaders =
                configuration.AsStringArray("boot:cors:ExposedHeaders", defaultValue: new[] {"Content-Type"});

            void PolicyBuilder(CorsPolicyBuilder builder)
            {
                if (allowAnyOrigin)
                    builder.AllowAnyOrigin();
                else if (origins.Any())
                    builder.WithOrigins(origins);

                if (allowAnyHeader)
                    builder.AllowAnyHeader();
                else if (headers.Any())
                    builder.WithHeaders(headers);

                if (allowAnyMethod)
                    builder.AllowAnyMethod();
                else if (methods.Any())
                    builder.WithMethods(methods);

                builder.WithExposedHeaders(exposedHeaders);

                if (allowCredentials && !allowAnyOrigin && origins.All(o => o.Trim() != "*"))
                    builder.AllowCredentials();
            }

            services.AddCors(options =>
            {
                if (string.IsNullOrWhiteSpace(corsPolicyName) ||
                    corsPolicyName.Equals("default", StringComparison.InvariantCultureIgnoreCase))
                    options.AddDefaultPolicy(PolicyBuilder);
                else
                    options.AddPolicy(corsPolicyName, PolicyBuilder);
            });
        }

        public override void Configure(IApplicationBuilder app)
        {
            var bootConfig = app.ApplicationServices.GetRequiredService<BootConfig>();
            if (!bootConfig.EnableCors) return;

            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
            var corsPolicyName = configuration.AsString("boot:cors:PolicyName");

            if (string.IsNullOrWhiteSpace(corsPolicyName) ||
                corsPolicyName.Equals("default", StringComparison.InvariantCultureIgnoreCase))
                app.UseCors();
            else
                app.UseCors(corsPolicyName);
        }
    }
}