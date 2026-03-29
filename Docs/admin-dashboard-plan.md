# Admin Dashboard Implementation Plan

## Overview

This document outlines the detailed implementation plan for the Innowise.Music Admin Dashboard - a Blazor Web application for managing music content (artists, albums, tracks, genres).

## Current State Analysis

### Existing Solution Structure
- **Innowise.Music** - MAUI client application (existing)
- **Innowise.MusicIdentityServer** - ASP.NET Core Web API backend (existing)
- **docker-compose** - Docker orchestration (existing)

### Existing Backend Capabilities
- ✅ JWT authentication with refresh tokens
- ✅ Role-based authorization (User, Administrator roles exist)
- ✅ Music entities: Artist, Album, Track, Genre (with DB context)
- ✅ Read-only music API endpoints in MusicController
- ✅ Default admin user: `admin@innowisemusic.com` / `P@ssword1`

### Required Backend Enhancements
- ❌ Admin-specific CRUD endpoints for all music entities
- ❌ File upload endpoint for audio files
- ❌ Admin role authorization on endpoints

---

## Implementation Phases

### Phase 1: Backend API Enhancements (Innowise.MusicIdentityServer)

#### 1.1 Extend IMusicService Interface
Add CRUD operations for all entities:

```csharp
// Artist operations
Task<IEnumerable<Artist>> GetAllArtistsAsync(int page, int pageSize);
Task<Artist?> GetArtistAsync(Guid id);
Task<Artist> CreateArtistAsync(Artist artist);
Task<Artist?> UpdateArtistAsync(Guid id, Artist artist);
Task<bool> DeleteArtistAsync(Guid id);

// Album operations
Task<IEnumerable<Album>> GetAllAlbumsAsync(int page, int pageSize);
Task<Album?> GetAlbumAsync(Guid id);
Task<Album> CreateAlbumAsync(Album album);
Task<Album?> UpdateAlbumAsync(Guid id, Album album);
Task<bool> DeleteAlbumAsync(Guid id);
Task<IEnumerable<Album>> GetAlbumsByArtistAsync(Guid artistId);

// Track operations
Task<IEnumerable<Track>> GetAllTracksAsync(int page, int pageSize);
Task<Track?> GetTrackAsync(Guid id);
Task<Track> CreateTrackAsync(Track track);
Task<Track?> UpdateTrackAsync(Guid id, Track track);
Task<bool> DeleteTrackAsync(Guid id);
Task<Stream?> GetTrackAudioAsync(Guid trackId);
Task<bool> UploadTrackAudioAsync(Guid trackId, Stream audioStream, string fileName);

// Genre operations
Task<IEnumerable<Genre>> GetAllGenresAsync();
Task<Genre?> GetGenreAsync(Guid id);
Task<Genre> CreateGenreAsync(Genre genre);
Task<Genre?> UpdateGenreAsync(Guid id, Genre genre);
Task<bool> DeleteGenreAsync(Guid id);
```

#### 1.2 Update MusicService Implementation
Implement all new methods in `MusicService.cs` with proper EF Core operations.

#### 1.3 Create AdminMusicController
New controller at `/api/admin` with endpoints:

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/admin/artists` | Paginated list of artists |
| GET | `/api/admin/artists/{id}` | Get artist by ID |
| POST | `/api/admin/artists` | Create artist |
| PUT | `/api/admin/artists/{id}` | Update artist |
| DELETE | `/api/admin/artists/{id}` | Delete artist |
| GET | `/api/admin/albums` | Paginated list of albums |
| GET | `/api/admin/albums/{id}` | Get album by ID |
| POST | `/api/admin/albums` | Create album |
| PUT | `/api/admin/albums/{id}` | Update album |
| DELETE | `/api/admin/albums/{id}` | Delete album |
| GET | `/api/admin/tracks` | Paginated list of tracks |
| GET | `/api/admin/tracks/{id}` | Get track by ID |
| POST | `/api/admin/tracks` | Create track metadata |
| PUT | `/api/admin/tracks/{id}` | Update track |
| DELETE | `/api/admin/tracks/{id}` | Delete track |
| POST | `/api/admin/tracks/{id}/upload` | Upload audio file |
| GET | `/api/admin/genres` | List all genres |
| GET | `/api/admin/genres/{id}` | Get genre by ID |
| POST | `/api/admin/genres` | Create genre |
| PUT | `/api/admin/genres/{id}` | Update genre |
| DELETE | `/api/admin/genres/{id}` | Delete genre |

#### 1.4 Add Admin Authorization
- Add `[Authorize(Roles = "Administrator")]` to all admin endpoints
- Ensure existing endpoints remain unchanged

---

### Phase 2: Blazor Admin Project Creation

#### 2.1 Create Project Structure
```
Innowise.Music.Admin/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   ├── NavMenu.razor
│   │   └── LoginLayout.razor
│   ├── Pages/
│   │   ├── Login.razor
│   │   ├── Dashboard.razor
│   │   ├── Artists/
│   │   │   ├── ArtistsList.razor
│   │   │   └── ArtistForm.razor
│   │   ├── Albums/
│   │   │   ├── AlbumsList.razor
│   │   │   └── AlbumForm.razor
│   │   ├── Tracks/
│   │   │   ├── TracksList.razor
│   │   │   ├── TrackForm.razor
│   │   │   └── TrackUpload.razor
│   │   └── Genres/
│   │       ├── GenresList.razor
│   │       └── GenreForm.razor
│   └── Shared/
│       ├── ConfirmDialog.razor
│       ├── FileUpload.razor
│       └── LoadingSpinner.razor
├── Services/
│   ├── AuthService.cs
│   ├── IAdminMusicService.cs
│   ├── AdminMusicService.cs
│   └── FileUploadService.cs
├── Models/
│   ├── AdminArtistDto.cs
│   ├── AdminAlbumDto.cs
│   ├── AdminTrackDto.cs
│   ├── AdminGenreDto.cs
│   └── UploadTrackRequest.cs
├── wwwroot/
│   ├── css/
│   │   └── admin-styles.css
│   └── images/
├── Program.cs
└── Innowise.Music.Admin.csproj
```

#### 2.2 Project File Configuration
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="9.0.0" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.0.0" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\Innowise.MusicIdentityServer\Innowise.MusicIdentityServer.csproj" />
  </ItemGroup>
</Project>
```

#### 2.3 Service Registration (Program.cs)
- Configure Blazor Server rendering
- Register HttpClient with JWT authentication
- Register custom services (AuthService, AdminMusicService)
- Configure authentication with JWT Bearer

---

### Phase 3: Authentication & Authorization

#### 3.1 AuthService Implementation
```csharp
public interface IAuthService
{
    Task<bool> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<bool> IsAdminAsync();
    event Action? OnAuthenticationStateChanged;
}
```

#### 3.2 JWT Token Management
- Store tokens in protected browser storage
- Implement token refresh logic
- Add JWT to all API requests via HttpMessageHandler

#### 3.3 Login Page
- Email/password form
- Validation with error display
- Redirect to dashboard on success
- Role check for admin access

---

### Phase 4: CRUD Operations Implementation

#### 4.1 Genres Management (Simplest - Start Here)
- **GenresList.razor**: Table view with search, pagination, edit/delete actions
- **GenreForm.razor**: Create/Edit form with validation
- Shared form component for create and edit modes

#### 4.2 Artists Management
- **ArtistsList.razor**: Table with image preview, search, pagination
- **ArtistForm.razor**: Form with name, biography, image URL, verified checkbox

#### 4.3 Albums Management
- **AlbumsList.razor**: Grid/table view with cover images
- **AlbumForm.razor**: Form with artist dropdown, release date picker, cover image URL

#### 4.4 Tracks Management
- **TracksList.razor**: Table with audio preview capability
- **TrackForm.razor**: Form with artist/album dropdowns, duration, explicit flag
- **TrackUpload.razor**: File upload component with progress indicator

---

### Phase 5: File Upload Implementation

#### 5.1 FileUpload.razor Component
```razor
<InputFile OnChange="HandleFileSelected" accept=".mp3,.wav,.flac,.aac" />
<ProgressBar Value="uploadProgress" />
<Button OnClick="UploadFile" Disabled="isUploading">Upload</Button>
```

#### 5.2 Streaming Upload Service
- Chunk large files (5MB chunks)
- Show progress percentage
- Handle cancellation
- Validate file type and size (max 50MB)

#### 5.3 Backend Upload Endpoint
```csharp
[HttpPost("{id}/upload")]
[RequestSizeLimit(50 * 1024 * 1024)] // 50MB
public async Task<IActionResult> UploadAudio(Guid id, IFormFile file)
```

---

### Phase 6: UI/UX Polish

#### 6.1 Layout Components
- **MainLayout.razor**: Sidebar navigation, header with user info
- **NavMenu.razor**: Navigation links with active state
- **LoginLayout.razor**: Centered login page layout

#### 6.2 Shared Components
- **ConfirmDialog.razor**: Reusable confirmation modal
- **LoadingSpinner.razor**: Loading indicator
- **PaginatedTable.razor**: Reusable table with pagination

#### 6.3 Styling
- CSS in `wwwroot/css/admin-styles.css`
- Consistent color scheme matching main app
- Responsive design for various screen sizes

---

### Phase 7: Docker Integration

#### 7.1 Docker Compose Service Configuration

The Admin Dashboard service has been added to `docker-compose.yml`:

```yaml
innowise.music.admin:
  build:
    context: .
    dockerfile: Innowise.Music.Admin/Dockerfile
  container_name: music_admin_dashboard
  depends_on:
    - innowise.musicidentityserver
  environment:
    - ASPNETCORE_ENVIRONMENT=Development
    - ASPNETCORE_HTTP_PORTS=8080
    - ApiSettings__BaseUrl=http://music_identity_server:8080/api
  ports:
    - "5237:8080"
  restart: unless-stopped
```

#### 7.2 Dockerfile (Already Created)

The Dockerfile at `Innowise.Music.Admin/Dockerfile` uses a multi-stage build:

```dockerfile
# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Innowise.Music.Admin/Innowise.Music.Admin.csproj", "Innowise.Music.Admin/"]
COPY ["Innowise.MusicIdentityServer/Innowise.MusicIdentityServer.csproj", "Innowise.MusicIdentityServer/"]
RUN dotnet restore "./Innowise.Music.Admin/Innowise.Music.Admin.csproj"
COPY . .
WORKDIR "/src/Innowise.Music.Admin"
RUN dotnet build "./Innowise.Music.Admin.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Innowise.Music.Admin.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Innowise.Music.Admin.dll"]
```

#### 7.3 Container Network Configuration

| Container | Service Name | Port | Purpose |
|-----------|--------------|------|---------|
| `music_postgres` | `postgres` | 5432 | Database |
| `music_identity_server` | `innowise.musicidentityserver` | 8080 | Backend API |
| `music_admin_dashboard` | `innowise.music.admin` | 8080 (5237 external) | Admin UI |

#### 7.4 Running with Docker Compose

```bash
# Build and start all services
docker-compose up --build

# Access Admin Dashboard at http://localhost:5237
# Access Identity Server API at http://localhost:5236
```

---

### Phase 8: Testing & Documentation

#### 8.1 Unit Tests
- Test services with mocked HttpClient
- Test validation logic
- Test authentication flow

#### 8.2 Integration Tests
- Test API endpoints with test database
- Test file upload functionality

#### 8.3 Documentation
- Update project.md with admin dashboard architecture
- Add API documentation for admin endpoints
- Create user guide for admin operations

---

## Dependencies & Prerequisites

### NuGet Packages (Admin Project)
- `Microsoft.AspNetCore.Components.Web` (9.0.0)
- `System.IdentityModel.Tokens.Jwt` (8.0.0)
- `Microsoft.AspNetCore.Authorization` (9.0.0)

### NuGet Packages (Identity Server - Already Present)
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Microsoft.AspNetCore.Authorization`

---

## Implementation Order

1. **Backend CRUD Services** - Extend IMusicService and MusicService
2. **Admin API Controller** - Create AdminMusicController
3. **Blazor Project Setup** - Create project and basic structure
4. **Authentication Service** - Implement login and token management
5. **Login Page** - Create login UI
6. **Genre Management** - Simplest CRUD (proof of concept)
7. **Artist Management** - Full CRUD with image support
8. **Album Management** - CRUD with relationships
9. **Track Management** - CRUD with file upload
10. **File Upload Component** - Streaming upload implementation
11. **Docker Integration** - Add to compose file
12. **Testing & Polish** - Unit tests and UI improvements

---

## Estimated Timeline

| Phase | Description | Estimated Time |
|-------|-------------|----------------|
| 1 | Backend API Enhancements | 4-6 hours |
| 2 | Blazor Project Creation | 2-3 hours |
| 3 | Authentication & Authorization | 2-3 hours |
| 4 | CRUD Operations | 8-10 hours |
| 5 | File Upload Implementation | 4-5 hours |
| 6 | UI/UX Polish | 3-4 hours |
| 7 | Docker Integration | 1-2 hours |
| 8 | Testing & Documentation | 4-5 hours |
| **Total** | | **28-38 hours** |

---

## Risk Assessment

| Risk | Mitigation |
|------|------------|
| Large file uploads causing memory issues | Use streaming upload with chunks |
| JWT token expiration during operations | Implement automatic token refresh |
| Database constraints causing delete failures | Handle foreign key relationships properly |
| Browser compatibility issues | Test on major browsers, use standard APIs |

---

## Success Criteria

1. ✅ Admin can log in with admin credentials
2. ✅ Admin can perform CRUD operations on all entities
3. ✅ Audio files can be uploaded up to 50MB
4. ✅ UI is responsive and user-friendly
5. ✅ All operations have proper error handling
6. ✅ Application runs in Docker container
7. ✅ Code follows project conventions
