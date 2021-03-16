# web-boot

dotnet web bootup utility

## Usage

Compile an assembly with a `Startup` class in it and hook it up to the `BOOT__STARTUPASSEMBLY` environment
variable or `boot:startupAssembly` key in an `appsettings.Release.json` file.

You can also use an `[assembly: HostingStartup(typeof(MyApp.HostingStartupEnhancement))]` 
attribute in the assembly, a modular way of enhancing the startup process built-into the aspnet core framework.

### nuget

Reference the `Web.Boot` package in your `web` project.

```
dotnet add package Web.Boot -v 0.0.3
```

Hook it up in your `Program.cs` file:

```
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Web.Boot;

namespace MyApp
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
./scripts/bump-version.csx -- --force $WEB_BOOT_VERSION
```

Build and pack, and push to Nuget

```
dotnet build -c Release src/Web.Boot/Web.Boot.csproj
```

Publish to nuget

```
NUGET_API_KEY=...

dotnet nuget push src/Web.Boot/bin/Release/Web.Boot.$WEB_BOOT_VERSION.nupkg -s https://api.nuget.org/v3/index.json -k $NUGET_API_KEY --interactive
```

### Docker

Set version

```
WEB_BOOT_REPO=mattjcowan/web-boot
WEB_BOOT_VERSION=0.0.1
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

