# web-boot

The [mattjcowan/web-boot](https://hub.docker.com/r/mattjcowan/web-boot) docker image and the [Web.Boot](https://www.nuget.org/packages/Web.Boot/) nuget package
provide a simple mechanism for booting up a dotnet application with some opinionated conventions.

## Usage

Compile a class library assembly with a `Startup` class in it.

```csharp
namespace MyApp
{
  public class Startup
  {
    public void ConfigureServices(IServiceCollection services)
    {      
    }
    public void Configure(IApplicationBuilder app)
    {      
    }
  }
}
```

Configure the assembly reference in the `BOOT__STARTUPASSEMBLY` environment
variable, or the `boot:startupAssembly` configuration key (typically in your `appsettings.json` file).

```json
{
  "boot": {
    "startupAssembly": "MyApp"
  }
}
```

You can also enhance the application at startup with a class in this same assembly that implements the [IHostingStartup](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.hosting.ihostingstartup) interface. Make sure to include the [HostingStartup](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/platform-specific-configuration#hostingstartup-attribute) assembly attribute as well.

```csharp
[assembly: HostingStartup(typeof(MyApp.HostingStartupEnhancement))]

namespace MyApp
{
    public class HostingStartupEnhancement : IHostingStartup
    {
        private const string ConfigDir = @"../config";

        public void Configure(IWebHostBuilder builder)
        {
            builder
                .ConfigureAppConfiguration((context, config) =>
                {
                    if (Directory.Exists(ConfigDir))
                      config.AddKeyPerFile(ConfigDir, false, true);
                });
        }
    }
}
```

### Configuration keys

A number of configuration keys are available to you when using this package or container:

- **boot:startupAssembly**: The path to the assembly dll with the Startup class, or the assembly name (if used by reference in the calling web project). When using a path to an assembly file, all *.dll assemblies in the same directory are also dynamically loaded to minimize dependency conflicts.
- **boot:dataDir**: (created if missing at startup) A path to a generic data directory available to the application, and created mostly for convenience as a generic place where a volume can be mapped and application extensions can read/write persisted data to.
- **boot:extensions:installDir**: (created if missing at startup) A path to a directory tree of assemblies and/or published class libraries that are intended to be dynamically loaded at startup. When using this, the startup assembly can use the assembly name convention, instead of the full path to the assembly as the assembly will already have been loaded into the context of the application.
- **boot:extensions:activeDir**: (cleared or created if missing at startup) A path to a directory to which the entire tree of files from the `boot:extensions:installDir` directory is copied to. This allows an application to prep changes in the install directory for the next reboot of the application, without worrying about assembly locking.

Here are the defaults used in the Docker container:

```json
{
  "boot": {
    "dataDir": "../data",
    "startupAssembly": "Web.Boot",
    "extensions": {
      "installDir": "../data/extensions",
      "activeDir": "../data/extensions-active"
    }
  }
}
```

Notice that if the `boot:startupAssembly` is not configured, a default `Startup` class in the Web.Boot project is used, which renders `Hello from boot! (at /{pathInfo})` for any route of the application. You can also call the `/healthz` for a quick health check to validate if the docker application is running.

Here are a few ways you can configure your application.

### Using nuget

Create a standard dotnet web application:

```sh
dotnet new web -n MyWebApp
cd MyWebApp
```

Add the `Web.Boot` nuget package to your project.

```sh
dotnet add package Web.Boot -v 0.0.4
```

Delete the `Startup` class in the `MyWebApp` project, and modify the `Program.cs` file as follows:

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Web.Boot;

namespace MyWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            HostBuilderFactory.CreateHostBuilder(args).Build().Run();
        }
    }
}
```

Add a reference to the `MyApp` project (or publish the `MyApp` project to the `boot:extensions:installDir` directory):

```sh
dotnet add reference ../MyApp/MyApp.csproj
```

### Using Docker

Just to get it up and running quickly at `http://localhost:3000`:

```sh
docker run -d -p 3000:80 --name myapp mattjcowan/web-boot
```

To configure the container to use a custom `Startup` class dynamically loaded from a `MyApp` assembly published to an extensions directory, you could use the following command:

```sh
docker run -d -p 3000:80 --name myapp \
  -e BOOT__STARTUPASSEMBLY=MyApp \
  -v /mnt/c/projects/myapp/data:/data \
  -v /mnt/c/projects/myapp/dist:/data/extensions:ro \
  mattjcowan/web-boot
```

### docker-compose

In the following example, publish your assembly with a `Startup` class to the `./data/boot/extensions` directory, and recycle the `boot` service.

```
version: "3.9"

services:

  boot:
    image: mattjcowan/web-boot:latest
    container_name: boot
    restart: unless-stopped
    environment: 
      - BOOT__DATADIR=/data
      - BOOT__STARTUPASSEMBLY=/extensions/myapp/myapp.dll
    # OR
    # env_file: ./boot.env
    ports:
      - "8080:80"
    volumes:
      - ./data/boot/data:/data
      - ./data/boot/extensions:/extensions:ro
```

## Notes to self

Notes to self, for maintenance purposes.

### Publish

Set version

```
dotnet tool install -g dotnet-script

WEB_BOOT_VERSION=0.0.3
dotnet-script scripts/bump-version.csx -- --force $WEB_BOOT_VERSION

# OR, just bump the major, minor, build versions
dotnet-script scripts/bump-version.csx -- --major
dotnet-script scripts/bump-version.csx -- --minor
dotnet-script scripts/bump-version.csx -- --build
```

Build and pack

```
dotnet build -c Release src/Web.Boot/Web.Boot.csproj
```

Publish to nuget

```
WEB_BOOT_VERSION=0.0.3
NUGET_API_KEY=...

dotnet nuget push src/Web.Boot/bin/Release/Web.Boot.$WEB_BOOT_VERSION.nupkg -s https://api.nuget.org/v3/index.json -k $NUGET_API_KEY --interactive

# or on Windows
set WEB_BOOT_VERSION=0.0.3
set NUGET_API_KEY=..
dotnet nuget push src/Web.Boot/bin/Release/Web.Boot.%WEB_BOOT_VERSION%.nupkg -s https://api.nuget.org/v3/index.json -k %NUGET_API_KEY% --interactive
```

### Docker

Set version

```
WEB_BOOT_REPO=mattjcowan/web-boot
WEB_BOOT_VERSION=0.0.3
```

Build an image

```
docker build . -t $WEB_BOOT_REPO:$WEB_BOOT_VERSION
```

Run it

```
docker run --rm -it -p 3000:80 $WEB_BOOT_REPO:$WEB_BOOT_VERSION
```

Tag and push

```
docker tag $WEB_BOOT_REPO:$WEB_BOOT_VERSION $WEB_BOOT_REPO:latest
docker push $WEB_BOOT_REPO:$WEB_BOOT_VERSION
docker push $WEB_BOOT_REPO:latest
```

