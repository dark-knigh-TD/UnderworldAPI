# ── Stage 1: Build ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution and restore — leverages Docker layer cache
COPY *.slnx ./
COPY src/Shared/UnderworldAPI.Shared.Domain/*.csproj ./src/Shared/UnderworldAPI.Shared.Domain/
COPY src/Shared/UnderworldAPI.Shared.Infrastructure/*.csproj ./src/Shared/UnderworldAPI.Shared.Infrastructure/
COPY src/Services/Sales/UnderworldAPI.Sales.Domain/*.csproj ./src/Services/Sales/UnderworldAPI.Sales.Domain/
COPY src/Services/Sales/UnderworldAPI.Sales.Application/*.csproj ./src/Services/Sales/UnderworldAPI.Sales.Application/
COPY src/Services/Sales/UnderworldAPI.Sales.Infrastructure/*.csproj ./src/Services/Sales/UnderworldAPI.Sales.Infrastructure/
COPY src/Services/Sales/UnderworldAPI.Sales.API/*.csproj ./src/Services/Sales/UnderworldAPI.Sales.API/

RUN dotnet restore src/Services/Sales/UnderworldAPI.Sales.API/UnderworldAPI.Sales.API.csproj

# Copy full source and publish
COPY . .
RUN dotnet publish src/Services/Sales/UnderworldAPI.Sales.API/UnderworldAPI.Sales.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "UnderworldAPI.Sales.API.dll"]