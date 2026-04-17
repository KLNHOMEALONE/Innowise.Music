# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Innowise.Music is a cross-platform music streaming application built with **.NET 9 MAUI**. It consists of three projects:

1. **Innowise.Music** - MAUI client app (iOS, Android, macOS, Windows)
2. **Innowise.MusicIdentityServer** - ASP.NET Core Web API with JWT auth, EF Core, PostgreSQL
3. **Innowise.Music.Admin** - Blazor Server admin dashboard

## Build and Run

### Prerequisites
- .NET 9 SDK
- Docker & Docker Compose (for backend services)

### Running with Docker (recommended for backend)
```bash
# Start PostgreSQL, Identity Server, and Admin Dashboard
docker-compose up --build
```
- Identity Server: `http://localhost:5236` (HTTP), `https://localhost:7008` (HTTPS)
- Admin Dashboard: `http://localhost:5237`
- PostgreSQL: `localhost:5432` (user/password from `.env`)

**Note:** The HTTPS development certificate is auto-generated during the Docker build — no manual certificate setup is required. See `Docs/docker-setup.md` for details.

### Running the MAUI client
Open `Innowise.Music.sln` in Visual Studio and run the `Innowise.Music` project. Select your target platform (Windows, Android, iOS, MacCatalyst).

### Running individual projects without Docker
```bash
dotnet run --project Innowise.MusicIdentityServer
dotnet run --project Innowise.Music.Admin
```

## Architecture

### Client (Innowise.Music) - MVVM Pattern
```
Innowise.Music/
├── View/           - XAML pages
├── ViewModel/      - ViewModels using CommunityToolkit.Mvvm
├── Model/          - Data models
├── Service/        - Business logic (auth, audio, navigation)
├── Validations/    - Custom validation framework
├── Controls/       - Reusable UI controls
├── Converters/     - Value converters
└── Resources/      - Fonts, images, styles
```

- **DI**: All Pages, ViewModels, and Services registered in `MauiProgram.cs` as singletons
- **Navigation**: Shell-based, routes registered in `AppShell.xaml.cs`
- **CommunityToolkit**: Uses `[ObservableObject]`, `[RelayCommand]`, `EventToCommandBehavior`

### Identity Server (Innowise.MusicIdentityServer)
- ASP.NET Core 9 Web API with JWT bearer authentication
- EF Core with PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- Auto-migrates DB on startup
- Serilog for logging
- Controllers in `Controllers/`, services in `Services/`

### Admin Dashboard (Innowise.Music.Admin)
- Blazor Server app with cookie authentication
- Communicates with Identity Server API via `HttpClient`
- Components in `Components/`, pages in `Pages/`, services in `Services/`

## Key Patterns

### Validation (from `Docs/validation.md`)
Properties requiring validation use `ValidatableObject<T>` with `IValidationRule<T>` rules. Validation is triggered via `EventToCommandBehavior` on `TextChanged` and manually before form submission. Errors displayed via `DataTrigger` and `FirstValidationErrorConverter`.

### Naming Conventions
- PascalCase for classes, methods, public members, folders
- `_camelCase` for private fields
- Interfaces prefixed with `I`

### API Communication
The MAUI client communicates with the Identity Server API via `HttpClient`. Base URL configured in `appsettings.json`. See `ApiSettings` and `GoogleAuthenticationSettings` sections.

## Docker Compose Services

| Service | Port | Description |
|---------|------|-------------|
| postgres | 5432 | PostgreSQL database |
| adminer | 8080 | Database admin UI |
| music_identity_server | 5236/7008 | API (HTTP/HTTPS) |
| music_admin_dashboard | 5237 | Blazor admin UI |

Credentials in `.env` file at repo root.

## Documentation

- `Docs/music-architecture.md` - Full system architecture and API design
- `Docs/validation.md` - Validation framework details
- `AGENTS.md` - Comprehensive coding standards, process rules, and development guidelines
