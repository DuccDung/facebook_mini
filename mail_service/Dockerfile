# ===== base (runtime) =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8082   # đổi lại 8080 nếu bạn muốn

# ===== build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1) COPY các .csproj trước để cache restore
#    Đường dẫn bên trái tính từ ROOT repo (context phải là root).
COPY ["platform_services/mail_service/mail_service.csproj", "platform_services/mail_service/"]

# Nếu mail_service có ProjectReference tới infrastructure, giữ dòng dưới (chọn đúng đường dẫn):
COPY ["infrastructure/infrastructure.csproj", "infrastructure/"]
# nếu infra của bạn nằm ở shared/infrastructure thì dùng:
# COPY ["shared/infrastructure/infrastructure.csproj", "shared/infrastructure/"]

# 2) restore
RUN dotnet restore "platform_services/mail_service/mail_service.csproj"

# 3) COPY toàn bộ source
COPY . .
WORKDIR /src/platform_services/mail_service

# 4) build
RUN dotnet build "mail_service.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ===== publish =====
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "mail_service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ===== final =====
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet","mail_service.dll"]
