# ===== RUNTIME (base) =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8084

# ===== BUILD =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1) Copy các file .csproj để cache restore
COPY ["profile_service/profile_service.csproj", "profile_service/"]
COPY ["infrastructure/infrastructure.csproj", "infrastructure/"]

# 2) Restore nuget cho profile_service
RUN dotnet restore "profile_service/profile_service.csproj"

# 3) Copy toàn bộ source code
COPY . .
WORKDIR /src/profile_service

# 4) Build (optional, chủ yếu check compile)
RUN dotnet build "profile_service.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ===== PUBLISH =====
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "profile_service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ===== FINAL IMAGE =====
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "profile_service.dll"]
