# ===== RUNTIME (base) =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8086

# ===== BUILD =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1) Copy các file .csproj để cache restore
COPY ["media_services/media_services.csproj", "media_services/"]
COPY ["infrastructure/infrastructure.csproj", "infrastructure/"]

# 2) Restore nuget cho media_services
RUN dotnet restore "media_services/media_services.csproj"

# 3) Copy toàn bộ source code
COPY . .
WORKDIR /src/media_services

# 4) Build (optional)
RUN dotnet build "media_services.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ===== PUBLISH =====
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "media_services.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ===== FINAL IMAGE =====
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "media_services.dll"]
