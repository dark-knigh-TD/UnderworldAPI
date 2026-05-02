# ── Stage 1: Build ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution and restore — leverages Docker layer cache
# Each COPY is granular so cache only invalidates when that .csproj changes
COPY *.slnx ./
COPY src/Shared/UnderworldAPI.Shared.Domain/*.csproj ./src/Shared/UnderworldAPI.Shared.Domain/
COPY src/Shared/UnderworldAPI.Shared.Infrastructure/*.csproj ./src/Shared/UnderworldAPI.Shared.Infrastructure/
COPY src/Services/Purchases/UnderworldAPI.Purchases.Domain/*.csproj ./src/Services/Purchases/UnderworldAPI.Purchases.Domain/
COPY src/Services/Purchases/UnderworldAPI.Purchases.Application/*.csproj ./src/Services/Purchases/UnderworldAPI.Purchases.Application/
COPY src/Services/Purchases/UnderworldAPI.Purchases.Infrastructure/*.csproj ./src/Services/Purchases/UnderworldAPI.Purchases.Infrastructure/
COPY src/Services/Purchases/UnderworldAPI.Purchases.API/*.csproj ./src/Services/Purchases/UnderworldAPI.Purchases.API/

RUN dotnet restore src/Services/Purchases/UnderworldAPI.Purchases.API/UnderworldAPI.Purchases.API.csproj

# Copy full source and publish
COPY . .
RUN dotnet publish src/Services/Purchases/UnderworldAPI.Purchases.API/UnderworldAPI.Purchases.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "UnderworldAPI.Purchases.API.dll"]