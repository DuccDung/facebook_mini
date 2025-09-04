# ===== base (runtime) =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# ===== build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# 1) COPY chỉ các .csproj trước (để cache layer restore)
#    ĐƯỜNG DẪN bên trái là từ ROOT SOLUTION (build context = root)
COPY ["mail_service/mail_service.csproj", "mail_service/"]
# Nếu project có tham chiếu đến shared/infrastructure, thêm dòng này:
# (nếu không có ProjectReference đó thì bỏ dòng này đi)
COPY ["mail_service/mail_service.csproj", "mail_service/"]
COPY ["infrastructure/infrastructure.csproj", "infrastructure/"]

# 2) restore
RUN dotnet restore "mail_service/mail_service.csproj"

# 3) COPY toàn bộ source
COPY . .
WORKDIR /src/mail_service

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