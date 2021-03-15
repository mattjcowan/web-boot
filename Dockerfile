FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
#FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
#FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src
COPY ["src/web-boot/nuget.config", "src/web-boot/"]
COPY ["src/web-boot/web-boot.csproj", "src/web-boot/"]
COPY ["src/Web.Boot/Web.Boot.csproj", "src/Web.Boot/"]
RUN dotnet restore "src/web-boot/web-boot.csproj"
COPY . .
WORKDIR "/src/src/web-boot"
RUN dotnet build "web-boot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "web-boot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "web-boot.dll"]