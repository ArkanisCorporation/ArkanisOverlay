FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

USER root

RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

#USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Arkanis.Overlay.Host.Server/Arkanis.Overlay.Host.Server.csproj", "src/Arkanis.Overlay.Host.Server/"]
COPY ["src/Arkanis.Overlay.Host.Server/packages.lock.json", "src/Arkanis.Overlay.Host.Server/"]
COPY ["src/Arkanis.Overlay.Components/Arkanis.Overlay.Components.csproj", "src/Arkanis.Overlay.Components/"]
COPY ["src/Arkanis.Overlay.Components/packages.lock.json", "src/Arkanis.Overlay.Components/"]
COPY ["src/Arkanis.Overlay.Domain/Arkanis.Overlay.Domain.csproj", "src/Arkanis.Overlay.Domain/"]
COPY ["src/Arkanis.Overlay.Domain/packages.lock.json", "src/Arkanis.Overlay.Domain/"]
COPY ["src/Arkanis.Overlay.Common/Arkanis.Overlay.Common.csproj", "src/Arkanis.Overlay.Common/"]
COPY ["src/Arkanis.Overlay.Common/packages.lock.json", "src/Arkanis.Overlay.Common/"]
COPY ["src/Arkanis.Overlay.Infrastructure/Arkanis.Overlay.Infrastructure.csproj", "src/Arkanis.Overlay.Infrastructure/"]
COPY ["src/Arkanis.Overlay.Infrastructure/packages.lock.json", "src/Arkanis.Overlay.Infrastructure/"]
COPY ["src/Arkanis.Overlay.External.UEX/Arkanis.Overlay.External.UEX.csproj", "src/Arkanis.Overlay.External.UEX/"]
COPY ["src/Arkanis.Overlay.External.UEX/packages.lock.json", "src/Arkanis.Overlay.External.UEX/"]
COPY ["src/Arkanis.Overlay.LocalLink/Arkanis.Overlay.LocalLink.csproj", "src/Arkanis.Overlay.LocalLink/"]
COPY ["src/Arkanis.Overlay.LocalLink/packages.lock.json", "src/Arkanis.Overlay.LocalLink/"]
COPY ["Directory.Packages.props", "."]
RUN dotnet restore "src/Arkanis.Overlay.Host.Server/Arkanis.Overlay.Host.Server.csproj"

FROM restore AS publish
ARG BUILD_CONFIGURATION=Release
COPY . .
RUN rm global.json
WORKDIR "/src/src/Arkanis.Overlay.Host.Server"
RUN dotnet tool restore
RUN dotnet publish "./Arkanis.Overlay.Host.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=5s --timeout=3s CMD wget --spider --quiet http://127.0.0.1:8080/healthz || exit 1
ENTRYPOINT ["dotnet", "Arkanis.Overlay.Host.Server.dll"]
