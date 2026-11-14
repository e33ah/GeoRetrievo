FROM node:22-alpine AS ui-build
WORKDIR /src/GeoRetrievo.UI

COPY GeoRetrievo.UI/package.json GeoRetrievo.UI/package-lock.json ./
RUN npm ci

COPY GeoRetrievo.UI/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS publish
WORKDIR /src
COPY GeoRetrievo.Proxy/GeoRetrievo.Proxy.csproj GeoRetrievo.Proxy/
RUN dotnet restore GeoRetrievo.Proxy/GeoRetrievo.Proxy.csproj
COPY GeoRetrievo.Proxy/ GeoRetrievo.Proxy/
COPY --from=ui-build /src/GeoRetrievo.UI/dist GeoRetrievo.Proxy/wwwroot/
RUN dotnet publish GeoRetrievo.Proxy/GeoRetrievo.Proxy.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=publish /out .
EXPOSE 8080
USER $APP_UID
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl --fail --silent --max-time 2 http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "GeoRetrievo.Proxy.dll"]
