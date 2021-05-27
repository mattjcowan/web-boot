// ReSharper disable IdentifierTypo

namespace Web.Boot
{
    /// <summary>
    ///     The order in which middleware components should be added in the Startup.Configure method.
    ///     See: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-5.0#middleware-order
    /// </summary>
    public enum KnownMiddlewarePriorities
    {
        ExceptionHandler = 0,
        Hsts = 10,
        HttpsRedirection = 20,
        StaticFiles = 30,
        CookiePolicy = 40,
        Routing = 50,
        RequestLocalization = 60,
        Cors = 70,
        Authentication = 100,
        Authorization = 110,
        Session = 120,
        ResponseCompression = 130,
        ResponseCaching = 140,
        Endpoints = int.MaxValue - 1000
    }
}