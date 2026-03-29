# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Innowise.Music.Admin/Innowise.Music.Admin.csproj", "Innowise.Music.Admin/"]
COPY ["Innowise.MusicIdentityServer/Innowise.MusicIdentityServer.csproj", "Innowise.MusicIdentityServer/"]
RUN dotnet restore "./Innowise.Music.Admin/Innowise.Music.Admin.csproj"
COPY . .
WORKDIR "/src/Innowise.Music.Admin"
RUN dotnet build "./Innowise.Music.Admin.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/Innowise.Music.Admin
# Publish with a custom target to avoid appsettings conflict
RUN dotnet publish "./Innowise.Music.Admin.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:CopyStaticWebAssets=false || true
# If publish failed due to appsettings conflict, manually copy the files
RUN if [ ! -f /app/publish/Innowise.Music.Admin.dll ]; then \
      mkdir -p /app/publish && \
      cp -r /app/build/* /app/publish/ && \
      cp -r /src/Innowise.Music.Admin/wwwroot /app/publish/ 2>/dev/null || true && \
      rm -f /app/publish/appsettings.json /app/publish/appsettings.Development.json && \
      cp /src/Innowise.Music.Admin/appsettings.json /app/publish/ 2>/dev/null || true && \
      cp /src/Innowise.Music.Admin/appsettings.Development.json /app/publish/ 2>/dev/null || true; \
    fi
# Remove the staticwebassets.runtime.json file which contains incorrect paths for the container
RUN rm -f /app/publish/Innowise.Music.Admin.staticwebassets.runtime.json

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish --chown=$APP_UID:$APP_UID /app/publish .
ENTRYPOINT ["dotnet", "Innowise.Music.Admin.dll"]
