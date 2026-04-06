# Stage 1: Base image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build image for restoring and building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the project files and restore dependencies
COPY ["Innowise.Music.Admin/Innowise.Music.Admin.csproj", "Innowise.Music.Admin/"]
RUN dotnet restore "Innowise.Music.Admin/Innowise.Music.Admin.csproj"

# Copy the rest of the source for this project and build
COPY Innowise.Music.Admin/ Innowise.Music.Admin/
WORKDIR "/src/Innowise.Music.Admin"
RUN dotnet build "Innowise.Music.Admin.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish image
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Innowise.Music.Admin.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Innowise.Music.Admin.dll"]
