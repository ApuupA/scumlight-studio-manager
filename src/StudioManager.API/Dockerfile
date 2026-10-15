# Build context is the "src" folder (see docker-compose.yml)

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# Folder for uploaded files (mounted as volume). It must belong to the non-root "app" user.
RUN mkdir -p /data/files && chown $APP_UID /data/files
USER $APP_UID
EXPOSE 8080

# Build image: restore first (cached as long as no .csproj changes), then publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["StudioManager.API/StudioManager.API.csproj", "StudioManager.API/"]
COPY ["StudioManager.DataLayer/StudioManager.DataLayer.csproj", "StudioManager.DataLayer/"]
COPY ["StudioManager.RepositoryLayer/StudioManager.RepositoryLayer.csproj", "StudioManager.RepositoryLayer/"]
COPY ["StudioManager.ServiceLayer/StudioManager.ServiceLayer.csproj", "StudioManager.ServiceLayer/"]
RUN dotnet restore "StudioManager.API/StudioManager.API.csproj"
COPY . .
RUN dotnet publish "StudioManager.API/StudioManager.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StudioManager.API.dll"]
