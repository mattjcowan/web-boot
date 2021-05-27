using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Web.Boot.Actions
{
    public class UseHstsAction : ConfigureAppBase, IConfigureServices
    {
        public override int Priority => (int) KnownMiddlewarePriorities.Hsts;

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration,
            IWebHostEnvironment env)
        {
            var bootConfig = configuration.GetBootConfig();
            if (env.IsDevelopment() || !bootConfig.EnableHsts) return;

            var includeSubDomains =
                (configuration["boot:hsts:IncludeSubDomains"] ?? bool.TrueString).Equals(bool.TrueString,
                    StringComparison.InvariantCultureIgnoreCase);
            var preload =
                (configuration["boot:hsts:Preload"] ?? bool.TrueString).Equals(bool.TrueString,
                    StringComparison.InvariantCultureIgnoreCase);
            if (!int.TryParse(configuration["boot:hsts:MaxAgeInDays"] ?? "365", out var maxAgeInDays))
                maxAgeInDays = 365;
            var excludedHosts = configuration["boot:hsts:ExcludedHosts"].SplitAndTrim();

            services.AddHsts(options =>
            {
                options.IncludeSubDomains = includeSubDomains;
                options.MaxAge = TimeSpan.FromDays(maxAgeInDays);
                options.Preload = preload;
                if (excludedHosts.Any())
                    foreach (var excludedHost in excludedHosts)
                        if (!options.ExcludedHosts.Contains(excludedHost))
                            options.ExcludedHosts.Add(excludedHost);
            });
        }

        public override void Configure(IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            var bootConfig = app.ApplicationServices.GetRequiredService<BootConfig>();
            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();

            if (!env.IsDevelopment() && bootConfig.EnableHsts) app.UseHsts();

            // additional HEADERS
            var xFrameOptions = configuration["boot:headers:XFrameOptions"] ?? "SAMEORIGIN";
            var xXssProtection = configuration["boot:headers:XXssProtection"] ?? "1; mode-block";
            var xContentTypeOptions = configuration["boot:headers:XContentTypeOptions"] ?? "nosniff";
            var referrerPolicy = configuration["boot:headers:ReferrerPolicy"] ?? "no-referrer";
            var xPermittedCrossDomainPolicies = configuration["boot:headers:XPermittedCrossDomainPolicies"] ?? "";
            var permissionsPolicy = configuration["boot:headers:PermissionsPolicy"] ??
                                    "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";
            var contentSecurityPolicy = configuration["boot:headers:ContentSecurityPolicy"] ?? "default-src 'self'";

            // ReSharper disable once IdentifierTypo
            var vals = new[]
            {
                xFrameOptions, xXssProtection, xContentTypeOptions, referrerPolicy, xPermittedCrossDomainPolicies,
                permissionsPolicy, contentSecurityPolicy
            };
            if (vals.Any(x => !string.IsNullOrWhiteSpace(x)))
                app.Use(async (context, next) =>
                {
                    if (!string.IsNullOrWhiteSpace(xFrameOptions))
                        context.Response.Headers.Add("X-Frame-Options", xFrameOptions);
                    if (!string.IsNullOrWhiteSpace(xXssProtection))
                        context.Response.Headers.Add("X-Xss-Protection", xXssProtection);
                    if (!string.IsNullOrWhiteSpace(xContentTypeOptions))
                        context.Response.Headers.Add("X-Content-Type-Options", xContentTypeOptions);
                    if (!string.IsNullOrWhiteSpace(referrerPolicy))
                        context.Response.Headers.Add("Referrer-Policy", referrerPolicy);
                    if (!string.IsNullOrWhiteSpace(xPermittedCrossDomainPolicies))
                        context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies",
                            xPermittedCrossDomainPolicies);
                    if (!string.IsNullOrWhiteSpace(permissionsPolicy))
                        context.Response.Headers.Add("Permissions-Policy", permissionsPolicy);
                    if (!string.IsNullOrWhiteSpace(contentSecurityPolicy))
                        context.Response.Headers.Add("Content-Security-Policy", contentSecurityPolicy);
                    await next();
                });
        }
    }
}