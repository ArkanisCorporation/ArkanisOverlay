FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

USER root
#RUN apt-get update && \
#    apt-get install -y --no-install-recommends wget && \
#    rm -rf /var/lib/apt/lists/*
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

#USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
RUN dotnet restore "src/Arkanis.Overlay.Host.Server/Arkanis.Overlay.Host.Server.csproj"
WORKDIR "/src/src/Arkanis.Overlay.Host.Server"
RUN dotnet build "./Arkanis.Overlay.Host.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Arkanis.Overlay.Host.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=5s --timeout=3s CMD wget --spider --quiet http://127.0.0.1:8080/healthz || exit 1
ENTRYPOINT ["dotnet", "Arkanis.Overlay.Host.Server.dll"]
