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
COPY ["Innowise.MusicIdentityServer/Innowise.MusicIdentityServer.csproj", "Innowise.MusicIdentityServer/"]
RUN dotnet restore "./Innowise.MusicIdentityServer/Innowise.MusicIdentityServer.csproj"
COPY . .
WORKDIR "/src/Innowise.MusicIdentityServer"
RUN dotnet build "./Innowise.MusicIdentityServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# Generate HTTPS development certificate for the container (PFX format)
RUN dotnet dev-certs https -ep /app/https-dev.pfx -p "devcert"
# Convert PFX to separate clean PEM files for certificate and private key
RUN openssl pkcs12 -in /app/https-dev.pfx -passin pass:devcert -clcerts -nokeys 2>/dev/null | openssl x509 > /app/https-dev-cert.pem && \
    openssl pkcs12 -in /app/https-dev.pfx -passin pass:devcert -nocerts -nodes 2>/dev/null | openssl pkey > /app/https-dev-key.pem
RUN dotnet publish "./Innowise.MusicIdentityServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /app/https-dev-cert.pem /home/app/.aspnet/https/https-dev-cert.pem
COPY --from=publish /app/https-dev-key.pem /home/app/.aspnet/https/https-dev-key.pem
# Make the certificates readable by the non-root app user
USER root
RUN chmod 644 /home/app/.aspnet/https/https-dev-*.pem && chown app:app /home/app/.aspnet/https/https-dev-*.pem
USER $APP_UID
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/home/app/.aspnet/https/https-dev-cert.pem
ENV ASPNETCORE_Kestrel__Certificates__Default__KeyPath=/home/app/.aspnet/https/https-dev-key.pem
ENTRYPOINT ["dotnet", "Innowise.MusicIdentityServer.dll"]