# ===== RUNTIME (base) =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8087

# ===== BUILD =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1) Copy các file .csproj để cache restore
COPY ["notification_service/notification_service.csproj", "notification_service/"]
COPY ["infrastructure/infrastructure.csproj", "infrastructure/"]

# 2) Restore nuget cho notification_service
RUN dotnet restore "notification_service/notification_service.csproj"

# 3) Copy toàn bộ source code
COPY . .
WORKDIR /src/notification_service

# 4) Build (optional)
RUN dotnet build "notification_service.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ===== PUBLISH =====
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "notification_service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ===== FINAL IMAGE =====
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "notification_service.dll"]
