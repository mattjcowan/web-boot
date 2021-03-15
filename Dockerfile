# Build with: 
# - docker build . -t mattjcowan/web-boot:0.0.1
# - docker run --rm -it -p 3000:80 mattjcowan/web-boot:0.0.1
# - docker push mattjcowan/web-boot:0.0.1
# - docker tag mattjcowan/web-boot:0.0.1 mattjcowan/web-boot:latest
# - docker push mattjcowan/web-boot:latest

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