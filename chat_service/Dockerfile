# ===== RUNTIME (base) =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8083   

# ===== BUILD =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1) Copy các file .csproj để cache restore
COPY ["chat_service/chat_service.csproj", "chat_service/"]
COPY ["infrastructure/infrastructure.csproj", "infrastructure/"]

# 2) Restore nuget cho chat_service
RUN dotnet restore "chat_service/chat_service.csproj"

# 3) Copy toàn bộ source code
COPY . .
WORKDIR /src/chat_service

# 4) Build (tuỳ chọn, chủ yếu để check compile)
RUN dotnet build "chat_service.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ===== PUBLISH =====
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "chat_service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ===== FINAL IMAGE =====
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Tên DLL phải đúng tên project (chat_service.dll)
ENTRYPOINT ["dotnet", "chat_service.dll"]
