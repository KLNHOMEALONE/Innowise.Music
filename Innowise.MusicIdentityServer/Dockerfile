# File: Dockerfile
# Description: Docker configuration for the Identity Server
# Dependencies: .NET 9 SDK and ASP.NET runtime images
# Created: 2026-03-01

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Innowise.MusicIdentityServer.csproj", "."]
RUN dotnet restore "./Innowise.MusicIdentityServer.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./Innowise.MusicIdentityServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Innowise.MusicIdentityServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Innowise.MusicIdentityServer.dll"]
