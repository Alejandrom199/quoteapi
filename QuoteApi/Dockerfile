# ── Stage 1: BUILD ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src

# Copia el csproj y restaura (aprovecha el cache de capas)
COPY QuoteApi.csproj .
RUN dotnet restore

# Copia el resto y publica
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Stage 2: RUNTIME ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime
WORKDIR /app

# Usuario no-root por seguridad
RUN groupadd -r appgroup \
    && useradd -r -g appgroup --no-create-home appuser

COPY --from=build /app/publish .
RUN chown -R appuser:appgroup /app

USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "QuoteApi.dll"]