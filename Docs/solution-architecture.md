# Innowise.Music - Complete Solution Architecture

## Overview

Innowise.Music is a full-stack .NET 9 music streaming platform with three projects:

1. **Innowise.Music** - MAUI cross-platform client (Android, iOS, macOS, Windows)
2. **Innowise.MusicIdentityServer** - ASP.NET Core Web API with JWT auth and PostgreSQL
3. **Innowise.Music.Admin** - Blazor Server admin dashboard

---

## Project Structures

### MAUI Client (Innowise.Music)

**Path:** `Innowise.Music/Innowise.Music.csproj`

**Targets:** `net9.0-android`, `net9.0-ios`, `net9.0-maccatalyst`, `net9.0-windows10.0.19041.0`

**Key Dependencies:**
- CommunityToolkit.Maui 9.0.0-preview4
- CommunityToolkit.Mvvm 8.4.0
- System.IdentityModel.Tokens.Jwt 8.16.0
- Google.Apis.Auth 1.73.0
- Microsoft.Extensions.Configuration.Json 9.0.0

**Structure:**
```
Innowise.Music/
├── Configuration/          # ApiSettings, GoogleAuthenticationSettings
├── Controls/              # InputEntryControl, MiniPlayerControl
├── Converters/            # BoolToColor, BoolToFavoriteIcon, Favorite converters
├── Model/                 # DTOs: Album, Artist, Track, AuthenticationResponse
├── Platforms/             # Android, iOS, MacCatalyst, Windows platform code
├── Services/              # Authentication, Audio, Stream, Search, History, Favorites
├── Validations/           # ValidatableObject<T>, IValidationRule<T>, rules
├── View/                  # Login, SignUp, Home, Search, Library, Events, Web pages
├── ViewModel/             # All page ViewModels with CommunityToolkit.Mvvm
├── Resources/             # Fonts, Images, App Icons, Splash
└── MauiProgram.cs         # DI configuration, app entry point
```

### Identity Server API (Innowise.MusicIdentityServer)

**Path:** `Innowise.MusicIdentityServer/Innowise.MusicIdentityServer.csproj`

**Target:** `net9.0`

**Key Dependencies:**
- Microsoft.AspNetCore.Identity.EntityFrameworkCore 9.0.13
- Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4
- AutoMapper 12.0.1
- Serilog.AspNetCore 10.0.0
- Microsoft.AspNetCore.Authentication.JwtBearer 9.0.13
- System.IdentityModel.Tokens.Jwt 8.16.0
- Google.Apis.Auth 1.73.0

**Structure:**
```
Innowise.MusicIdentityServer/
├── Configurations/        # MapperConfig, MusicSettings
├── Controllers/           # Authentication, Music, AdminMusic
├── Data/                  # MusicIdentityDbContext, ApiUser
├── Migrations/            # EF Core migrations
├── Models/
│   ├── Music/            # Track, Artist, Album, Genre, UserRecentTrack, UserFavoriteTrack
│   └── User/             # UserDto, AuthenticationResponse, LoginUserDto
├── Services/              # IMusicService/MusicService, IStreamTokenService/StreamTokenService
├── Static/                # CustomClaimTypes
├── Program.cs             # DI, JWT, EF Core, middleware pipeline
└── appsettings.json       # JWT, Google Auth, DB, Serilog
```

### Admin Dashboard (Innowise.Music.Admin)

**Path:** `Innowise.Music.Admin/Innowise.Music.Admin.csproj`

**Target:** `net9.0`

**Key Dependencies:**
- Microsoft.AspNetCore.Components.Web 9.0.0
- System.IdentityModel.Tokens.Jwt 8.3.0
- TagLibSharp 2.3.0 (audio metadata extraction)

**Structure:**
```
Innowise.Music.Admin/
├── Auth/                  # Authentication state handling
├── Components/
│   ├── Layout/           # MainLayout, NavMenu
│   ├── Pages/            # Genres, Artists, Albums, Tracks (list + form pages)
│   │   └── Tracks/       # MultiTrackUpload, TrackUpload components
│   └── Shared/           # ConfirmDialog, LoadingSpinner, etc.
├── Models/               # Album, Artist, Track, Genre, TrackUploadDto, PagedResponse
├── Pages/                # Razor pages (Login.cshtml, Logout.cshtml)
├── Services/             # IAuthService/AuthService, IAdminMusicService/AdminMusicService
├── wwwroot/css/          # app.css (centralized dark theme styles)
└── Program.cs            # Blazor server, cookie auth, HTTP client setup
```

---

## Database Schema

**Provider:** PostgreSQL 15
**Context:** `MusicIdentityDbContext` at `Innowise.MusicIdentityServer/Data/MusicIdentityDbContext.cs`

### Identity Tables (ASP.NET Identity)

| Table | Description |
|-------|-------------|
| AspNetUsers (ApiUser) | Extended with FirstName, LastName, RefreshToken, RefreshTokenExpiryTime |
| AspNetRoles | User, Administrator roles (seeded) |
| AspNetUserRoles | User-role mappings |

### Music Tables

**Artists**
- Id (Guid, PK), Name (string, 255, GIN indexed), Biography (string, 1000), ImageUrl (string, 500), Verified (bool), MonthlyListeners (long), CreatedAt, UpdatedAt
- Navigation: Albums, Tracks

**Albums**
- Id (Guid, PK), Title (string, 255, GIN indexed), ArtistId (Guid, FK), ReleaseDate (DateOnly?), CoverImageUrl (string, 500), Genre (string, 100), Label (string, 255), TotalTracks (int), Duration (int?), CreatedAt, UpdatedAt
- Navigation: Artist, Tracks

**Tracks**
- Id (Guid, PK), Title (string, 255, GIN indexed), ArtistId (Guid?, FK), AlbumId (Guid?, FK), TrackNumber (int?), Duration (int), AudioData (byte[]), AudioFormat (string, 12), Bitrate (int?), SampleRate (int?), FileSize (long?), ISRC (string, 12), Explicit (bool), PlayCount (long, indexed desc), CreatedAt, UpdatedAt
- Navigation: Artist, Album, Genres (many-to-many via TrackGenres)

**Genres**
- Id (Guid, PK), Name (string, 100, unique), Description (string, 500), ImageUrl (string, 500), Color (string, 7)
- Navigation: Tracks (many-to-many)

**TrackGenres** (Join Table)
- TrackId (Guid, FK), GenreId (Guid, FK), Composite PK

**UserRecentTracks**
- Id (Guid, PK), UserId (string, FK, indexed), TrackId (Guid, FK), PlayedAt (DateTime, indexed with UserId)
- Auto-trimmed to 5 most recent per user

**UserFavoriteTracks**
- Id (Guid, PK), UserId (string, FK, indexed), TrackId (Guid, FK), CreatedAt (DateTime)
- Unique index on (UserId, TrackId)

### Seed Data

**Roles:**
- User (Id: `0e543f8c-0093-4aa1-ad0b-18368c9b099d`)
- Administrator (Id: `95c93ace-7651-44c4-8737-52851d614f32`)

**Users:**
- `admin@innowisemusic.com` / `P@ssword1` (Administrator)
- `user@innowisemusic.com` / `P@ssword1` (User)

---

## API Endpoints

### Authentication Controller (`/api/Authentication`)

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/register` | Create new user account |
| POST | `/login` | Email/password authentication |
| POST | `/google-login` | Google OAuth authentication |
| POST | `/refresh` | Refresh access token |

### Music Controller (`/api/Music`)

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/search` | Unified search across tracks, artists, albums |
| GET | `/tracks` | Search tracks with pagination |
| GET | `/tracks/{id}` | Get track details |
| GET | `/tracks/{id}/stream-token` | Generate 5-minute signed stream token |
| GET | `/tracks/{id}/stream` | Stream audio (supports range requests) |
| GET | `/artists/{id}/top-tracks` | Get artist's most popular tracks |
| GET | `/recommendations` | Get personalized recommendations |
| GET | `/recommendations/artists` | Get user's top 3 artists from history |
| GET | `/albums/{id}/tracks` | Get all tracks from an album |
| POST | `/tracks/{id}/history` | Record track play in listening history |
| GET | `/history/recent` | Get user's recent tracks (max 10) |
| POST | `/tracks/{id}/favorite` | Toggle track as favorite |
| GET | `/tracks/{id}/is-favorite` | Check if track is favorited |
| GET | `/favorites` | Get all user's favorite tracks |

### Admin Music Controller (`/api/admin`)

All endpoints require `[Authorize(Roles = "Administrator")]`.

**Genres:** `GET/POST/PUT/DELETE /genres`, `GET /genres/{id}`

**Artists:** `GET /artists` (paginated), `GET/POST/PUT/DELETE /artists/{id}`, `GET /artists/{artistId}/albums`

**Albums:** `GET /albums` (paginated), `GET/POST/PUT/DELETE /albums/{id}`

**Tracks:** `GET /tracks` (paginated), `GET/POST/PUT/DELETE /tracks/{id}`, `POST /tracks/{id}/upload` (single file, max 50MB), `POST /tracks/upload-batch` (batch, max 30 files, 500MB)

---

## Authentication & Authorization

### JWT Configuration

**File:** `Innowise.MusicIdentityServer/appsettings.json`

```json
"JwtSettings": {
  "Issuer": "Innowise.MusicIdentityServer",
  "Audience": "Innowise.Music",
  "Duration": 1,
  "Key": "075ee246-d7d6-4089-84ea-1246f95b9014"
}
```

- Algorithm: HS256
- Token lifetime: 1 hour
- Refresh token lifetime: 7 days
- ClockSkew: Zero

### MAUI Client Auth Flow

1. User enters credentials, `AuthenticationService.LoginAsync()` POSTs to `/api/Authentication/login`
2. Receives `AuthenticationResponse` with Token + RefreshToken
3. Stores token in `SecureStorage.Default` (platform-encrypted), refresh token separately
4. Before each API call, `IsAuthenticatedAsync()` checks if token expires in < 1 minute
5. If expiring, auto-refreshes via POST `/api/Authentication/refresh`
6. On refresh failure, logs user out

### Admin Dashboard Auth Flow

1. Admin enters credentials, `AuthService.LoginAndGetPrincipalAsync()` POSTs to `/api/Authentication/login`
2. Validates user has "Administrator" role
3. Signs in via cookie authentication (`CookieAuthenticationDefaults`)
4. Caches JWT + refresh token in `IMemoryCache` (8-hour sliding expiration)
5. On each API call, `GetTokenAsync()` retrieves token from cache and adds Bearer header
6. If missing, attempts refresh from cached refresh token

### Stream Token System

**File:** `Innowise.MusicIdentityServer/Services/StreamTokenService.cs`

- Generates 5-minute JWT tokens with claims: `track_id`, `user_id`, `role="stream"`
- Client requests token via `GET /api/Music/tracks/{id}/stream-token`
- Client streams audio with `GET /api/Music/tracks/{id}/stream?token={streamToken}`
- Stream endpoint validates stream token OR falls back to main JWT Bearer
- Supports HTTP range requests for seeking

---

## Key Services

### IMusicService / MusicService

**File:** `Innowise.MusicIdentityServer/Services/MusicService.cs` (853 lines)

**Search:** `SearchTracksAsync`, `SearchArtistsAsync`, `SearchAlbumsAsync` — uses ILIKE with wildcards, GIN trigram indexes

**Track Operations:** `GetTrackAsync`, `GetTrackAudioAsync` (increments PlayCount), `GetArtistTopTracksAsync`, `GetAlbumTracksAsync`, `GetRecommendedTracksAsync`

**CRUD:** Full CRUD for Artists, Albums, Tracks, Genres

**Batch Upload:** `UploadTracksAsync` with `GetOrCreateArtistAsync`, `GetOrCreateAlbumAsync`, `GetOrCreateGenresAsync`

**History:** `RecordListeningHistoryAsync` (upsert, trims to 5), `GetRecentTracksAsync`, `GetTopArtistsAsync`

**Favorites:** `ToggleFavoriteAsync`, `IsFavoriteAsync`, `GetFavoritesAsync`

### MAUI Client Services

| Service | Key Methods |
|---------|-------------|
| `IAuthenticationService` | `LoginAsync`, `GoogleLoginAsync`, `RegisterAsync`, `LogoutAsync`, `GetTokenAsync`, `IsAuthenticatedAsync` |
| `IStreamTokenService` | `GetStreamTokenAsync` |
| `IAudioService` | `Initialize`, `Play`, `Pause`, `Stop`, `Resume` (uses MediaElement) |
| `ISearchService` | Unified search across entities |
| `IRecommendationService` | Track recommendations |
| `IHistoryService` | Listening history management |
| `IFavoriteService` | Favorite toggle/check/list |
| `INavigationService` | Shell-based navigation |

### Admin Services

| Service | Key Methods |
|---------|-------------|
| `IAuthService` | `LoginAndGetPrincipalAsync`, `LogoutAsync`, `IsAuthenticatedAsync`, `IsAdminAsync`, `GetTokenAsync` |
| `IAdminMusicService` | HTTP client wrapper for all admin API endpoints with auto auth header injection |

---

## Cross-Cutting Concerns

### Logging

**Framework:** Serilog (Serilog.AspNetCore 10.0.0)

**Sinks:** Console, File (daily rolling at `./logs/log-.txt`), Seq (optional)

**Minimum Levels:** Information (default), Warning (Microsoft namespaces)

### Validation

**MAUI Client:** Custom generic framework with `ValidatableObject<T>`, `IValidationRule<T>`, and rules: `IsNotNullOrEmptyRule`, `EmailRule`, `CompareRule`. Real-time validation via `EventToCommandBehavior` on `TextChanged`.

**Server:** ASP.NET Identity validation + ModelState validation in controllers.

### Configuration

**MAUI:** `appsettings.json` with `ApiSettings` (BaseUrl, AndroidBaseUrl for emulator) and `GoogleAuthentication` (multiple client IDs per platform).

**Identity Server:** `ConnectionStrings`, `JwtSettings`, `MusicSettings`, `GoogleAuthentication`, `Serilog`.

**Admin:** `ApiSettings` with Docker service name (`http://music_identity_server:8080/api/`).

---

## Docker Setup

**File:** `docker-compose.yml`

| Service | Container | Port | Description |
|---------|-----------|------|-------------|
| PostgreSQL | `music_postgres` | 5432 | Database |
| Identity Server | `music_identity_server` | 5236 (HTTP), 7008 (HTTPS) | API with auto-generated HTTPS cert |
| Admin Dashboard | `music_admin_dashboard` | 5237 | Blazor admin UI |
| Adminer | `Adminer` | 8080 | Database UI |

**Key Detail:** HTTPS dev certificate is auto-generated in the Dockerfile during build — no manual cert setup required.

**Network:** Docker Compose default bridge. Services communicate via service names (e.g., `postgres`, `innowise.musicidentityserver`).

---

## Important Implementation Details

### Audio Storage
- Stored as `byte[]` in `Tracks.AudioData` (recommended max 10MB)
- Streaming supports HTTP range requests
- Content-Type set from `AudioFormat` field

### Search
- PostgreSQL `ILIKE` with wildcards (`%query%`)
- GIN trigram indexes on Artist.Name, Album.Title, Track.Title
- Unified search returns paginated results across all entity types

### Listening History
- `UserRecentTracks` table, auto-trimmed to 5 most recent per user
- Upsert behavior: existing track moves to top
- Powers recommendations and "top artists" features

### Refresh Token Strategy
- **MAUI:** SecureStorage (encrypted), 7-day expiry, auto-refresh on API calls
- **Admin:** IMemoryCache (8-hour sliding), auto-refresh from cache, lost on restart

---

## File Paths Reference

| Component | File Path |
|-----------|-----------|
| Solution | `Innowise.Music.sln` |
| Docker Compose | `docker-compose.yml` |
| Identity Server DI | `Innowise.MusicIdentityServer/Program.cs` |
| DbContext | `Innowise.MusicIdentityServer/Data/MusicIdentityDbContext.cs` |
| Music Service | `Innowise.MusicIdentityServer/Services/MusicService.cs` |
| Auth Controller | `Innowise.MusicIdentityServer/Controllers/AuthenticationController.cs` |
| Music Controller | `Innowise.MusicIdentityServer/Controllers/MusicController.cs` |
| Admin Controller | `Innowise.MusicIdentityServer/Controllers/AdminMusicController.cs` |
| MAUI DI | `Innowise.Music/MauiProgram.cs` |
| MAUI Auth Service | `Innowise.Music/Services/AuthenticationService.cs` |
| MAUI Audio Service | `Innowise.Music/Services/AudioService.cs` |
| Admin DI | `Innowise.Music.Admin/Program.cs` |
| Admin Auth Service | `Innowise.Music.Admin/Services/AuthService.cs` |
| Admin Music Service | `Innowise.Music.Admin/Services/AdminMusicService.cs` |
