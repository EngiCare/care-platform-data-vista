# ============================================================================
# CarePlatform Data.CPRS API
# Exposes: HTTP 8080
# Connects to: VistA RPC Server (TCP 9200) via VistaConnection
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy all referenced project files for restore
COPY CarePlatform.Models/CarePlatform.Models.csproj CarePlatform.Models/
COPY CarePlatform.Data.CPRS/CarePlatform.Data.CPRS.csproj CarePlatform.Data.CPRS/
RUN dotnet restore CarePlatform.Data.CPRS/CarePlatform.Data.CPRS.csproj

# Copy all source
COPY CarePlatform.Models/ CarePlatform.Models/
COPY CarePlatform.Data.CPRS/ CarePlatform.Data.CPRS/
WORKDIR /src/CarePlatform.Data.CPRS
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Install curl for healthcheck and create non-root user
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/* \
    && groupadd -r appuser && useradd -r -g appuser -d /app -s /sbin/nologin appuser

COPY --from=build /app/publish .
RUN chown -R appuser:appuser /app

USER appuser

EXPOSE 8080

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=15s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -sf http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "CarePlatform.Data.CPRS.dll"]
