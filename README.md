# web-boot

dotnet web bootup utility

## Publish

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

## Docker

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

