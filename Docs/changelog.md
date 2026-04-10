# Changelog - Innowise.Music

All notable changes to this project will be documented in this file.

## [Unreleased] - Favorites Feature

### Added
- **Favorite tracks**: Users can now mark tracks as favorites with a heart icon in the mini player
  - `UserFavoriteTracks` table with unique constraint on `(UserId, TrackId)`
  - `POST /api/Music/tracks/{id}/favorite` — toggle favorite (add if not favorited, remove if already)
  - `GET /api/Music/tracks/{id}/is-favorite` — check if a track is favorited by current user
  - `GET /api/Music/favorites` — get all favorite tracks for current user
- `IFavoriteService` / `FavoriteService` on the MAUI client for toggling and checking favorite status
- Mini player heart icon: filled heart (♥) with red background when favorited, outlined heart (♡) with transparent background when not
- Favorite status is checked automatically when a track starts playing
- `FavoriteTextConverter` (uses Unicode text variant selector for cross-platform heart rendering) and `FavoriteBackgroundConverter`
- Quick Access section on homepage now displays user's favorite tracks (6 random if > 6, all if ≤ 6) with clickable play support

### Changed
- Mini player's static checkmark replaced with interactive heart toggle (40x40, matching play/pause button size)
- `BoolToFavoriteIconConverter` now returns outlined heart (`favorite_outline_icon.svg`) instead of add icon for unfavorited state
- Added `favorite_outline_icon.svg` image resource
- `IFavoriteService` extended with `GetAllFavoritesAsync()` for fetching all user favorites
- `HomePageViewModel` loads favorite tracks into Quick Access items on login, replacing static mock data

## [Previous] - Listening History Feature

### Added
- **Per-user listening history**: New `UserRecentTracks` table tracks the 5 most recently played tracks per user
  - `POST /api/Music/tracks/{id}/history` — records a play (upsert, trims to 5 most recent)
  - `GET /api/Music/history/recent?count=N` — fetches user's N most recent tracks
- `IHistoryService` / `HistoryService` on the MAUI client for recording and fetching listening history
- `RecentItems` on HomePage now populated from real backend data instead of mock data
- `MessagingCenter` pub/sub between `MiniPlayerViewModel` and `HomePageViewModel` — recent items refresh automatically when any track plays
- `RecentItems` cleared on logout to prevent cross-user data leakage

### Fixed
- **JWT `sub` claim using wrong value**: The `sub` claim was set to `user.UserName` (email string) instead of `user.Id` (GUID), causing foreign key violations when the history controller read `ClaimTypes.NameIdentifier` as the user ID for `UserRecentTracks.UserId`. Fixed in `AuthenticationController.GenerateToken()`.
- **New track while paused played old track**: When a track was paused and the user clicked a different track, the `AudioService.Play()` method saw `CurrentState == Paused` and unconditionally called `_mediaElement.Play()` to resume — but this resumed the **old** track from its paused position, not the new track. Fixed by checking if the source URL matches the requested URL before taking the resume fast-path. If the URL differs, the method now stops the old track and assigns the new source.

## [2026-04-06] - Fixed Dashboard Statistics, JSON Serialization & Login Styling

### Fixed
- **Dashboard showing incorrect counts (0) for Artists, Genres, Albums, Tracks**: The root cause was a claim type mismatch in `AuthService.cs`
  - The Identity Server JWT token uses a custom claim type `"uid"` for the user ID (defined in `CustomClaimTypes.Uid`)
  - The Admin Dashboard's `AuthService` was looking for `ClaimTypes.NameIdentifier` which doesn't exist in the token
  - This caused `userId` to be `null` during login, so the token was **never cached** in memory
  - Subsequent API calls to fetch dashboard stats failed with 401 Unauthorized because no token was available
- Updated `AuthService.LoginAndGetPrincipalAsync()` to extract user ID using `claimsPrincipal.FindFirst("uid")?.Value` instead of `FindFirstValue(ClaimTypes.NameIdentifier)`
- Updated `AuthService.GetTokenAsync()` to use `user.FindFirst("uid")?.Value` for consistent claim extraction
- Added proper error handling with try-catch blocks in `AdminMusicService` methods (`GetAllArtistsAsync`, `GetAllAlbumsAsync`, `GetAllTracksAsync`, `GetAllGenresAsync`) to gracefully handle API failures
- Added `ILogger<Dashboard>` injection to `Dashboard.razor` for proper logging instead of `Console.WriteLine`

- **JSON serialization cycle error for Albums API**: The Identity Server was throwing `System.Text.Json.JsonException: A possible object cycle was detected` when returning albums with included Artist navigation property
  - The `Artist` model has a `ICollection<Album> Albums` navigation property
  - The `Album` model has an `Artist` navigation property
  - When serializing albums with included artist, EF Core would create an infinite loop: Album -> Artist -> Albums -> Artist -> ...
  - Fixed by configuring JSON serialization in `Program.cs` to use `ReferenceHandler.IgnoreCycles` which breaks the cycle by ignoring duplicate references

- **Login page styling inconsistent with dashboard**: The login page was using a light theme with inline styles that didn't match the dark theme of the admin dashboard
  - Removed inline `<style>` block from `Login.cshtml`
  - Updated markup to use the dark theme CSS classes already defined in `app.css` (`.login-page`, `.login-wrapper`, `.login-card`, `.login-form`, `.form-group`, `.form-control`, `.btn-login`)
  - Login page now matches the dashboard's dark theme with gradient background, dark card, and red accent button

### Changed
- **`Innowise.MusicIdentityServer/Program.cs`**: Added JSON serialization configuration:
  ```csharp
  builder.Services.AddControllers()
      .AddJsonOptions(options =>
      {
          options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
          options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
      });
  ```

- **`Innowise.Music.Admin/Pages/Login.cshtml`**: Removed inline styles and restructured to use dark theme CSS classes from `app.css`

### Technical Details
- The JWT token generated by Identity Server includes claims: `sub`, `jti`, `email`, and `uid` (custom claim for user ID)
- The `ClaimsIdentity` in `CreateClaimsPrincipalFromToken` preserves all claims from the token
- The fix ensures the token is properly cached during login and retrieved for subsequent API calls
- Dashboard now correctly displays: 3 artists, 1 genre, 1 album, 2 tracks (matching database state)
- Login page now uses the same design system as the rest of the admin dashboard

## [2026-04-02] - Admin Dashboard Authentication Refactor

### Changed
- **Complete authentication system rewrite** following the proven BookStore Blazor Server pattern using `Blazored.LocalStorage` and Blazor's built-in `AuthorizeRouteView`

### Added
- **`Blazored.LocalStorage` (v4.3.0)**: Added NuGet package for browser `localStorage` access, replacing raw `IJSRuntime` calls
- **`Auth/ApiAuthenticationStateProvider.cs`**: New JWT-based `AuthenticationStateProvider` using `Blazored.LocalStorage` with:
  - `GetAuthenticationStateAsync()` — reads token from `localStorage`, validates expiry, extracts claims
  - `LoggedIn()` — re-reads claims from stored token, calls `NotifyAuthenticationStateChanged()`
  - `LoggedOut()` — removes token from `localStorage`, notifies anonymous auth state
  - `GetTokenAsync()` — async token retrieval for bearer header injection
- **`Components/Pages/Logout.razor`**: Dedicated logout page that calls `AuthService.LogoutAsync()` then navigates to `/login`
- **`@attribute [Authorize]`**: Added to all 11 protected pages (Dashboard, Artists, Albums, Tracks, Genres and their forms) so `AuthorizeRouteView` enforces authentication

### Changed
- **`Components/App.razor`**: Wrapped router in `<CascadingAuthenticationState>`, replaced `<RouteView>` with `<AuthorizeRouteView>` containing `<NotAuthorized>` template that renders `<RedirectToLogin />` for unauthenticated users
- **`Pages/_Host.cshtml`**: Changed `render-mode` from `ServerPrerendered` to `Server` to ensure Blazor circuit is established before JS interop calls (required for `Blazored.LocalStorage`)
- **`Program.cs`**: Dual registration pattern for `ApiAuthenticationStateProvider` — registered as both concrete type and as `AuthenticationStateProvider` interface so both Blazor infrastructure and `AuthService` resolve the same scoped instance. Added `AddBlazoredLocalStorage()`. Removed unused session/distributed cache configuration
- **`Services/AuthService.cs`**: Rewritten to use `Blazored.LocalStorage` and `ApiAuthenticationStateProvider`. Login stores token via `localStorage.SetItemAsync()` then calls `LoggedIn()`. Logout delegates to `LoggedOut()`. Removed in-memory `ConcurrentDictionary` token cache and `IHttpContextAccessor` dependency
- **`Services/IAuthService.cs`**: Added `GetTokenAsync()` method, removed `OnAuthenticationStateChanged` event (replaced by `NotifyAuthenticationStateChanged`)
- **`Services/AdminMusicService.cs`**: `AddAuthHeaderAsync()` now uses `await GetTokenAsync()` instead of synchronous `GetToken()`
- **`Components/Pages/Login.razor`**: Simplified — uses `NavigateTo("/")` without `forceLoad` since `NotifyAuthenticationStateChanged` triggers `AuthorizeRouteView` re-evaluation automatically
- **`Components/Layout/MainLayout.razor`**: Logout is now an `<a href="/logout">` link navigating to the Logout page. Uses `GetTokenAsync()` for user info
- **`Components/_Imports.razor`**: Added `@using Microsoft.AspNetCore.Components.Authorization` and `@using Innowise.Music.Admin.Auth`

### Removed
- **`Auth/PersistentAuthenticationStateProvider.cs`**: Deleted — replaced by `ApiAuthenticationStateProvider`
- **`Components/AuthorizeRouteView.razor`**: Deleted custom component — replaced by Blazor's built-in `AuthorizeRouteView`
- **`Components/Shared/AuthorizeView.razor`**: Deleted custom component — replaced by Blazor's built-in `AuthorizeView`
- **`@rendermode InteractiveServer`**: Removed from `LoginForm.razor`, `LogoutButton.razor`, `MultiTrackUpload.razor` — not applicable in Blazor Server (all components are already interactive)
- **Session/distributed cache services**: Removed from `Program.cs` — no longer needed with `localStorage` persistence

### Fixed
- **Token not persisting across app restarts**: Previously tokens were stored in server-side memory (`ConcurrentDictionary`) tied to HTTP session IDs, which were lost on app restart. Now stored in browser `localStorage` via `Blazored.LocalStorage`
- **Logout not clearing token**: Previously `forceLoad: true` navigation killed the SignalR circuit before `RemoveItemAsync` could complete. Now logout navigates within the Blazor circuit, ensuring the token removal completes before any page transition
- **Dashboard accessible without login after restart**: Pages lacked `@attribute [Authorize]`, so `AuthorizeRouteView` treated them as public. Added `[Authorize]` to all 11 protected pages
- **Prerendering JS interop failures**: Changed `_Host.cshtml` from `ServerPrerendered` to `Server` to avoid `localStorage` access during prerendering when JS interop is unavailable

### Authentication Flow (Final)
1. User navigates to `/` → `AuthorizeRouteView` checks auth → `GetAuthenticationStateAsync()` reads `localStorage` → no token → renders `<RedirectToLogin />` → navigates to `/login`
2. User enters credentials → `AuthService.LoginAsync()` → POST to Identity Server → stores JWT in `localStorage` → calls `LoggedIn()` → `NotifyAuthenticationStateChanged()` → `NavigateTo("/")`
3. `AuthorizeRouteView` re-evaluates → user authenticated → renders Dashboard
4. User clicks Logout → navigates to `/logout` page → `LogoutAsync()` → `LoggedOut()` removes token from `localStorage` → `NotifyAuthenticationStateChanged()` → `NavigateTo("/login")`
5. App restart → fresh request → `GetAuthenticationStateAsync()` → no token in `localStorage` → redirected to `/login`

## [2026-04-01] - Fixed Admin Dashboard Black Screen Issue

### Fixed
- **Admin Dashboard Black Screen**: Resolved the black screen issue when running the admin dashboard
  - Created custom `Routes.razor` component to properly integrate authentication routing
  - Updated `AuthorizeRouteView.razor` to show loading state during authentication checks instead of rendering page then redirecting
  - Fixed `Shared/AuthorizeView.razor` to show loading indicator when redirecting to login instead of showing empty content
- **Login Form Submit Not Firing**: Fixed the `HandleLogin` method not being triggered on form submission
  - Changed `@onsubmit="HandleLogin"` to `@onsubmit.prevent="HandleLogin"` in `Login.razor` to prevent browser's default form submission

### Technical Details
- The root cause was the auto-generated `Routes` component in .NET 9 not using our custom `AuthorizeRouteView`
- The new `Routes.razor` uses Blazor's `Router` component with our `AuthorizeRouteView` for proper auth flow
- Authentication checks now happen in `OnInitializedAsync` before rendering, preventing the black screen flash

## [2026-04-01] - Music Tracks Batch Upload Implementation

### Added

- **Batch Track Upload Feature**: Implemented complete batch upload functionality for music tracks with automatic metadata extraction using TagLibSharp.

- **Backend API (Identity Server)**:
  - Created `TrackUploadDto.cs` model with batch upload DTOs and result classes
  - Added `UploadTracksAsync()` method to `IMusicService` for batch processing
  - Implemented `GetOrCreateArtistAsync()`, `GetOrCreateAlbumAsync()`, `GetOrCreateGenresAsync()` methods
  - Added `POST /api/admin/tracks/upload-batch` endpoint to `AdminMusicController` with 500MB size limit
  - Batch upload automatically creates missing artists, albums, and genres

- **Admin Dashboard**:
  - Created `ExtractedTrackMetadata.cs` model for preview data with UI-specific properties
  - Created `TrackUploadDto.cs` in Admin project for upload data transfer
  - Implemented `IMetadataExtractionService` interface
  - Implemented `MetadataExtractionService` using TagLibSharp for metadata extraction
  - Added `UploadTracksBatchAsync()` method to `IAdminMusicService` and `AdminMusicService`
  - Created `MultiTrackUpload.razor` component with drag-and-drop file selection
  - Added file validation (MP3, WAV, FLAC, AAC only; 50MB per file; 30 files max)
  - Implemented metadata preview with editable fields
  - Added dropdown selectors for existing artists/albums/genres
  - Updated `TracksList.razor` with "Add Tracks" button
  - Registered `IMetadataExtractionService` in `Program.cs`

### Technical Details

- Metadata extraction supports: Title, Artists, Album, Genres, Year, Track Number, Duration, Bitrate, Sample Rate
- Auto-matching of existing entities by name (case-insensitive)
- Option to create new entities if not found
- Comprehensive error handling with detailed feedback
- Progress indication during extraction and upload

### Fixed
- **Blazor Server Interop Issues in MultiTrackUpload.razor**:
  - Changed render mode in `_Host.cshtml` from `ServerPrerendered` to `Server` to ensure SignalR connection is established immediately
  - Fixed `disabled` attribute syntax: changed `disabled="!CanExtract"` to `disabled="@(!CanExtract)"` to properly evaluate the expression
  - Added `await InvokeAsync(StateHasChanged)` to all event handlers (`RemoveFile`, `ResetUpload`, `UploadTracks`, `ExtractMetadata`) to ensure UI updates
  - Removed CSS that was hiding the file input's `::file-selector-button`, which was blocking user interaction
  - Simplified file input styling to use standard browser appearance

## [2026-03-31] - Fixed AdminMusicService API Endpoints & Dashboard Statistics

### Fixed
- **Corrected all API endpoint paths in `AdminMusicService.cs`**:
  - The `AdminMusicController` uses `[Route("api/admin")]` attribute, making all endpoints prefixed with `api/admin/`
  - The service was incorrectly using just `admin/` as the prefix, causing 404 errors for all music CRUD operations
  - Updated all 21 endpoint calls to use the correct `api/admin/` prefix:
    - Genres: `api/admin/genres` (5 endpoints)
    - Artists: `api/admin/artists` (5 endpoints)
    - Albums: `api/admin/albums` (5 endpoints)
    - Tracks: `api/admin/tracks` (6 endpoints including upload)

- **Fixed JSON deserialization mismatch for paginated endpoints**:
  - The controller's list endpoints (`GET /api/admin/artists`, `/albums`, `/tracks`) return `PagedResponse<T>` not `List<T>`
  - Removed duplicate `GetArtistsAsync()`, `GetAlbumsAsync()`, `GetTracksAsync()` methods that expected `List<T>`
  - Updated all list pages and form pages to use `GetAllArtistsAsync()`, `GetAllAlbumsAsync()`, `GetAllTracksAsync()` which correctly return `PagedResponse<T>`
  - Updated `ArtistsList.razor`, `AlbumsList.razor`, `TracksList.razor` to extract `.Items` from paged responses
  - Updated `AlbumForm.razor` and `TrackForm.razor` to use paginated methods for dropdown data

- **Implemented real-time statistics in Dashboard page**:
  - `Dashboard.razor` was showing placeholder values (all zeros) for Artists, Albums, Tracks, and Genres counts
  - Implemented `LoadDashboardStats()` method that calls the API to fetch actual counts:
    - Uses `GetAllArtistsAsync()`, `GetAllAlbumsAsync()`, `GetAllTracksAsync()` with `pageSize=1` to efficiently get `TotalCount`
    - Uses `GetAllGenresAsync()` to get the list and count genres
  - Dashboard now displays accurate statistics from the database

## [2026-03-31] - Admin Dashboard Deployment Configuration & Login Fix

### Fixed
- **Resolved 404 error when logging into Admin Dashboard**:
  - The `LoginAsync` method was calling `authentication/login` (relative path) which resulted in incorrect URL construction
  - Fixed by changing the endpoint to `api/authentication/login` (absolute path) in `AuthService.cs`
  - Login now works correctly in both development mode (VS Debug) and Docker Compose deployment

### Changed
- **Updated API endpoint configuration to use consistent HTTPS**:
  - `appsettings.json` - Changed BaseUrl from `http://music_identity_server:8080/api` to `https://music_identity_server:8081/api`
  - `appsettings.Development.json` - Changed BaseUrl from `http://localhost:5236/api/` to `https://localhost:7008/api`
  - `docker-compose.yml` - Updated `ApiSettings__BaseUrl` to use HTTPS: `https://music_identity_server:8081/`
  - `AuthService.cs` - Changed `PostAsJsonAsync("authentication/login", ...)` to `PostAsJsonAsync("api/authentication/login", ...)`

### Added
- **Comprehensive deployment documentation in `Docs/admin-dashboard.md`**:
  - Added "Deployment Modes" section explaining two deployment scenarios:
    - Mode 1: Development (Visual Studio Debug) - Admin runs from VS, Identity Server in Docker
    - Mode 2: Docker Compose - Both run in containers
  - Added configuration reference table with ports and URLs
  - Documented environment variable override mechanism
  - Updated troubleshooting section with HTTPS configuration details

## [2026-03-30] - Admin Dashboard Docker Authentication Fix

### Fixed
- **Resolved "Invalid email or password" error when running Admin Dashboard in Docker**:
  - Root cause: Docker DNS resolution issue - the Admin container was trying to reach `music_identity_server` (container name) but Docker's internal DNS uses service names
  - The Identity Server service name is `innowise.musicidentityserver`, but the Admin was configured to use `music_identity_server`
  - Added network alias `music_identity_server` to the Identity Server service for backward compatibility
  - Credentials work correctly from mobile app and Postman because they use `localhost` (host machine) not Docker internal networking

### Changed
- `docker-compose.yml` - Added network alias `music_identity_server` to `innowise.musicidentityserver` service

### Added
- `Docs/admin-dashboard.md` - Added "Troubleshooting & Common Issues" section with:
  - Root cause analysis of Docker DNS resolution issues
  - Two solution options (update Base URL or add network aliases)
  - Diagnostic commands for verifying connectivity
  - Configuration reference table

## [2026-03-30] - Fixed HTTPS/SSL Configuration for Identity Server

### Fixed
- **Resolved SSL error `WRONG_VERSION_NUMBER` when accessing Identity Server via HTTPS on port 7008**:
  - The Identity Server container was listening on HTTP only for both ports 8080 and 8081, despite Docker mapping port 7008 to 8081 expecting HTTPS
  - Root cause: `ConfigureKestrel()` in `Program.cs` was overriding the environment variables and binding both ports as HTTP
  - Fixed by adding `.UseHttps()` to port 8081 configuration in Kestrel
  - Updated `docker-compose.yml` to use `ASPNETCORE_HTTPS_PORTS=8081` instead of custom certificate path
  - The server now correctly listens on `https://[::]:8081` and Postman can successfully connect via HTTPS

### Changed
- `Innowise.MusicIdentityServer/Program.cs` - Added HTTPS configuration to Kestrel for port 8081
- `docker-compose.yml` - Simplified HTTPS configuration by using environment variables instead of custom certificate path

## [2026-03-29] - Fixed Logout Functionality in Admin Dashboard

### Fixed
- **Logout button now properly redirects to login page**: The logout functionality in `MainLayout.razor` was not working because it was using JavaScript interop (`window.location.replace`) which didn't properly reset the Blazor circuit.
- Changed from `IJSRuntime` to `NavigationManager` for navigation
- Using `Navigation.NavigateTo("/login", forceLoad: true)` ensures a full page reload that properly clears the session and redirects to login
- Updated `AuthorizeRouteView.razor` to also use `forceLoad: true` for consistent behavior

### Changed
- `MainLayout.razor` - Replaced `@inject IJSRuntime JS` with `@inject NavigationManager Navigation`
- `MainLayout.razor` - Changed logout method from `JS.InvokeVoidAsync("window.location.replace", "/login")` to `Navigation.NavigateTo("/login", forceLoad: true)`
- `AuthorizeRouteView.razor` - Changed redirect from `forceLoad: false` to `forceLoad: true`

---

## [2026-03-29] - Fixed Unauthenticated Redirect to Login Page

### Fixed
- **Unauthenticated users now properly redirected to login page**: The previous implementation in `Routes.razor` used `OnAfterRenderAsync` which ran too late - after the page had already rendered. This caused the dashboard to briefly appear before redirecting.
- Created new `AuthorizeRouteView.razor` component that wraps `RouteView` and checks authentication in `OnParametersSetAsync` before rendering any protected page
- The `Routes.razor` now uses `AuthorizeRouteView` instead of the standard `RouteView` component
- Login page is explicitly allowed without authentication; all other pages require valid session token

### Changed
- `Routes.razor` - Simplified to use `AuthorizeRouteView` instead of custom authentication logic
- `AuthorizeRouteView.razor` (new) - Component that intercepts routing to check authentication status before rendering pages

---

## [2026-03-29] - Complete Authentication Workflow Implementation

### Fixed
- **Complete authentication workflow now working correctly**:
  1. User navigates to admin dashboard URL
  2. `Routes.razor` checks authentication status via `IAuthService.IsAuthenticatedAsync()`
  3. If not authenticated → redirect to `/login` with `forceLoad: true`
  4. User enters credentials on `Login.razor`
  5. `AuthService.LoginAsync()` validates with Identity Server
  6. `AuthService.IsAdminAsync()` checks for "Administrator" role
  7. If admin → navigate to dashboard (`/`)
  8. If not admin → show "Access Denied" error
  9. User clicks Logout button → `AuthService.LogoutAsync()` clears session → redirect to `/login`

### Added
- **Authentication state change listener in Routes.razor**: Added event subscription to `AuthService.OnAuthenticationStateChanged` to handle logout redirects
- **Proper logout flow**: When user logs out, the authentication state changes and triggers automatic redirect to login page

### Changed
- Updated `Routes.razor` to subscribe to authentication state changes and redirect to login when user logs out
- Removed unused `IJSRuntime` injection from `MainLayout.razor`

---

## [2026-03-29] - Fixed Logout Redirect Issue

### Fixed
- **Logout button now properly redirects to login page**: Changed from JavaScript interop (`JSRuntime.InvokeVoidAsync("location.replace", "/login")`) to `NavigationManager.NavigateTo("/login", forceLoad: true)` in `MainLayout.razor`
- The previous JS interop approach was not working reliably in Blazor Server context
- Using `forceLoad: true` ensures a full page reload, which clears the Blazor circuit and forces re-authentication
- Removed unused `IJSRuntime` injection from `MainLayout.razor`

### Summary of Authentication Workflow
The complete authentication flow is now working correctly:
1. User navigates to admin dashboard URL
2. `Routes.razor` checks authentication status via `IAuthService.IsAuthenticatedAsync()`
3. If not authenticated → redirect to `/login` with `forceLoad: true`
4. User enters credentials on `Login.razor`
5. `AuthService.LoginAsync()` validates with Identity Server
6. `AuthService.IsAdminAsync()` checks for "Administrator" role
7. If admin → navigate to dashboard (`/`)
8. If not admin → show "Access Denied" error
9. **User clicks Logout button** → `AuthService.LogoutAsync()` clears session → redirect to `/login` with `forceLoad: true`

---

## [2026-03-29] - Verify and Fix Admin Dashboard Login Workflow

### Verified
- Confirmed the complete authentication flow from app startup to dashboard access
- Verified that only admin users can access the dashboard after successful login

### Fixed
- Added authentication check in `Routes.razor` to redirect unauthenticated users to `/login` page on initial app load
- The `Routes.razor` component now checks `IAuthService.IsAuthenticatedAsync()` on first render
- If user is not authenticated and not already on the login page, they are redirected to `/login` with `forceLoad: true`

### Workflow Summary
1. User navigates to admin dashboard URL
2. `Routes.razor` checks authentication status
3. If not authenticated → redirect to `/login`
4. User enters credentials on `Login.razor`
5. `AuthService.LoginAsync()` validates with Identity Server
6. `AuthService.IsAdminAsync()` checks for "Administrator" role
7. If admin → navigate to dashboard (`/`)
8. If not admin → show "Access Denied" error

---

## [2026-03-29] - Fix Login Redirection to Dashboard

### Fixed
- Fixed login redirection issue: Added `forceLoad: true` to navigation in `AuthorizeView.razor`
- The `AuthorizeView` component was redirecting unauthenticated users to `/login` without `forceLoad: true`, which could cause navigation issues in Blazor Server
- Updated line 15 in `AuthorizeView.razor` to use `Navigation.NavigateTo("/login", forceLoad: true)`

---

## [2026-03-29] - Docker Containers Rebuild & Deployment

### Fixed
- Fixed admin dashboard startup error: Added missing `AddRazorPages()` service to `Program.cs`
- Resolved `InvalidOperationException` caused by `MapFallbackToPage("/_Host")` requiring Razor Pages services
- Created proper Razor Pages host file at `Pages/_Host.cshtml` (not `.razor`) for fallback routing

### Added
- `Pages/_Host.cshtml` - Razor Pages host file serving as the entry point for Blazor Server app

### Changed
- Rebuilt all Docker containers with latest code changes
- Successfully deployed: PostgreSQL, Adminer, Identity Server, Admin Dashboard

---

## [2026-03-29] - Admin Dashboard Authentication & Authorization

### Added
- Created `AuthorizeView` component for role-based access control in Blazor
- Added loading and access denied states with CSS styles
- Implemented admin-only access check on login

### Changed
- Updated all admin pages (Dashboard, Artists, Albums, Tracks, Genres) to require authentication
- Updated `MainLayout.razor` to integrate with `IAuthService` and display user info
- Updated `Login.razor` to verify admin role before granting access
- Added `@layout MainLayout` directive to all protected pages

### Fixed
- Fixed malformed AuthorizeView tags by properly closing all HTML elements
- Ensured consistent layout and navigation across all admin pages

---

## [YYYY-MM-DD] - Admin Dashboard Planning

### Added

- **Admin Dashboard Plan**: Created comprehensive implementation plan in `Docs/admin-dashboard-plan.md`
  - Detailed 8-phase implementation strategy
  - Backend API enhancements for CRUD operations
  - Blazor Web App project structure
  - Authentication and authorization flow
  - File upload specifications
  - Docker integration plan
  - Timeline estimates (28-38 hours total)

### Documentation

- Updated `Docs/tasktracker.md` with Admin Dashboard task breakdown
- Created detailed technical specifications for all admin endpoints
- Documented project structure and component hierarchy

---

## [2026-03-28] - Music Streaming Backend Implementation (Phase 1)

### Added

- **Music Database Models**: Created comprehensive data models for music streaming:
  - `Artist.cs` - Artist entity with Name, Biography, ImageUrl, Verified status, MonthlyListeners
  - `Album.cs` - Album entity with Title, ArtistId, ReleaseDate, CoverImageUrl, Genre, Duration
  - `Track.cs` - Track entity with Title, ArtistId, AlbumId, Duration, **AudioData (BYTEA)**, AudioFormat, Bitrate, SampleRate, FileSize, ISRC, Explicit, PlayCount
  - `Genre.cs` - Genre entity with Name, Description, ImageUrl, Color

- **Full-Text Search Support**: Configured PostgreSQL GIN indexes with trigram extension (`pg_trgm`) for efficient fuzzy searching on:
  - Artists.Name
  - Albums.Title
  - Tracks.Title

- **Music Service Layer**:
  - `IMusicService.cs` - Interface defining music data access methods
  - `MusicService.cs` - Implementation with EF Core queries, Include for related data, ILike for case-insensitive search

- **Music API Controller**:
  - `MusicController.cs` with 5 essential endpoints:
    - `GET /api/music/tracks?query={q}` - Search tracks with pagination
    - `GET /api/music/tracks/{id}` - Get track details with full metadata
    - `GET /api/music/tracks/{id}/stream` - Stream audio with range request support
    - `GET /api/music/artists/{id}/top-tracks` - Get artist's popular tracks
    - `GET /api/music/albums/{id}/tracks` - Get album tracks with total duration

- **Database Migration**: `AddMusicTables` migration with:
  - All music tables (Artists, Albums, Tracks, Genres, TrackGenres junction)
  - Foreign key relationships and cascade delete rules
  - Full-text search indexes with pg_trgm extension
  - PlayCount descending index for popular tracks

### Changed

- **MusicIdentityDbContext**: Updated with DbSets for Artists, Albums, Tracks, Genres and full-text search configuration in OnModelCreating

- **Program.cs**: Registered `IMusicService` and `MusicService` in dependency injection container

### Technical Details

- Audio files stored as PostgreSQL BYTEA (binary data)
- Range request support for audio streaming with proper Content-Type headers
- Pagination support with configurable page size (capped at 50)
- Play count tracking incremented on audio stream access

## [2026-03-27] - Fix audio playback and progress display in MiniPlayer

### Fixed
- The selected song would not play and the progress bar was not updating.
- The `AudioService` was not being initialized with a `MediaElement`, as the `MediaElement` was missing from `MiniPlayerControl.xaml`.
- `AudioService` `Play` method was not robust and could cause a `NullReferenceException`.
- `AudioService` `Pause` method was using a non-existent `CanPause` property.
- `MiniPlayerViewModel` did not correctly update its properties when the `AudioService` state changed, leading to an unresponsive progress bar.

### Added
- A `Stop()` method to the `IAudioService` and `AudioService` for completeness.
- The missing `MediaElement` to `MiniPlayerControl.xaml`.
- Initialization logic in `MiniPlayerControl.xaml.cs` to connect the `AudioService` to the `MediaElement`.
- `Position` and `Duration` properties to `MiniPlayerViewModel` for better UI binding.

### Changed
- Refactored `AudioService` to correctly handle playback using `ShouldAutoPlay` and proper state checks.
- Refactored `MiniPlayerViewModel` to ensure all player-related properties update in sync with the `AudioService`.

## [2026-03-26] - Implemented Input Validation & UI Enhancements

### Added

- **Input Validation System**: Implemented the project's custom input validation system across Login and Sign-up pages.
    - Properties in ViewModels (`LoginPageViewModel`, `SignUpPageViewModel`) refactored to `ValidatableObject<T>`.
    - Custom validation rules (`EmailRule`, `IsNotNullOrEmptyRule`, `CompareRule`) applied to relevant fields.
- **`FirstValidationErrorConverter`**: New converter added to display the first validation error message.
- **Automatic Validation**: Integrated `CommunityToolkit.Maui` to enable automatic, as-you-type validation feedback.
    - Added `TextChangedCommand` to `InputEntryControl`.
    - Implemented `EventToCommandBehavior` in `InputEntryControl.xaml` to trigger validation commands in ViewModels.

### Changed

- **`InputEntryControl` UI**: Modified `InputEntryControl.xaml` to include a `Label` for error messages and dynamic `Border` styling based on validation status.
    - Layout changed from `VerticalStackLayout` to `Grid` within `InputEntryControl.xaml` to resolve Android UI rendering issues.
- **`LoginPageViewModel`**: Updated to use `ValidatableObject<string>` for Email and Password, and added validation commands.
- **`SignUpPageViewModel`**: Updated to use `ValidatableObject<string>` for Email, Password, Repeat Password, First Name, and Last Name, and added validation commands including a `CompareRule` for password matching.
- **`LoginPage.xaml`**: Bindings updated to reflect `ValidatableObject<T>` properties and to utilize `FirstValidationErrorConverter` and automatic validation commands.
- **`SignUpPage.xaml`**: Bindings updated to reflect `ValidatableObject<T>` properties, to include `FirstName` and `LastName` input controls, and to utilize `FirstValidationErrorConverter` and automatic validation commands.
- **Validation Rule Consistency**: Renamed `IsNullOrEmptyRule.cs` to `IsNotNullOrEmptyRule.cs` to match the class name (`IsNotNullOrEmptyRule`).

### Fixed

- **Android UI Issue**: Resolved a UI rendering issue on Android by changing the internal layout of `InputEntryControl.xaml` from `VerticalStackLayout` to `Grid`.
- **`MVVMTK0034` Warnings**: Fixed warnings in `LoginPageViewModel` and `SignUpPageViewModel` by referencing generated properties instead of private fields for `[ObservableProperty]` attributes.
- **`CommunityToolkit.Maui` Compatibility**: Successfully integrated `CommunityToolkit.Maui` by using a compatible version for the .NET 9 framework.

## [2026-03-23] - Fixed 500 Error on Login

### Fixed

- Resolved a 500 Internal Server Error that occurred on the login page.
- The root cause was the Google authentication middleware crashing the application on startup due to a missing `ClientId` in the configuration.
- The fix involved temporarily disabling the Google authentication middleware in `Program.cs` to allow the application to start and the login endpoint to function.

## [2026-03-23] - Fixed Android Google Authentication Loop

### Fixed

- Investigated and resolved an infinite loop during Google authentication on the Android platform. The root cause was determined to be a corrupted or stale state within the Android emulator, not a code defect.
- The debugging process involved multiple attempts to align the `redirect_uri` in `GoogleAuthService.cs` with the `[IntentFilter]` in `WebAuthenticationCallbackActivity.cs`.
- An attempt to use a hierarchical URI (`com.klnhomealone.innomusic://oauth2redirect`) with a `DataHost` filter resolved the loop but resulted in `400 invalid_request` and `access_blocked` policy errors from Google's OAuth service.
- Further investigation into Google Cloud Console settings (SHA-1 keys, test users) did not resolve the policy errors.
- Code was reverted to its original state.
- The final solution was to reset the Android emulator to factory settings, which cleared the corrupted state and allowed the original code to function correctly.

## [2026-03-18] - Google Authentication Re-implementation

### Added
- **WebPage**: Created a new `WebPage.xaml` and `WebPage.xaml.cs` to host a `WebView` for the Google authentication flow.
- **WebPageViewModel**: Created a new `WebPageViewModel` to manage the state of the `WebPage`.

### Changed
- **GoogleAuthService**: Updated to use the `WebPage` to host the Google authentication flow instead of `WebAuthenticator`.
- **AppShell**: Updated to handle the authentication result from the `WebPage`.

## [2026-03-18] - Google Authentication Implementation

### Added
- **Google Authentication Service**: Created `IGoogleAuthService` and `GoogleAuthService` to handle the Google login flow using `WebAuthenticator`.
- **Dependency Injection**: Registered `IGoogleAuthService` and `GoogleAuthService` in `MauiProgram.cs`.
- **ViewModel Update**: Updated `LoginPageViewModel` to use the `GoogleAuthService` and added a `GoogleLogin` command.
- **Identity Server Endpoint**: Added a new `google-login` endpoint to the `AuthenticationController` to handle Google token validation.
- **Google.Apis.Auth**: Added the `Google.Apis.Auth` nuget package to the `Innowise.MusicIdentityServer` project.
- **GoogleTokenDto**: Created a new DTO to transfer the Google token from the client to the server.

### Changed
- **AuthenticationController**: Updated the `google-login` endpoint to validate the Google token and create a new user if one doesn't exist.

## [2026-03-05] - XAML Resource Resolution Fixes

### Fixed
- Resolved `StaticResource` not found issues by replacing `PrimaryRed` with its hex value `#D90429` in:
    - `LibraryPage.xaml` (BackgroundColor of Border)
    - `SignUpPage.xaml` (TextColor in Span, BackgroundColor of Border)
    - `LoginPage.xaml` (TextColor in Span, BackgroundColor of Border)
    - `EventsPage.xaml` (Color of BoxViews, BackgroundColor of Button)
    - `Resources/Styles/Styles.xaml` (Dark theme `Shell.TabBarForegroundColor` and `Shell.TabBarTitleColor`)
    - `Controls/MiniPlayerControl.xaml` (BackgroundColor of Border)
    - `AppShell.xaml` (Shell.TabBarTitleColor and Shell.TabBarForegroundColor)
    - `App.xaml` (BackgroundColor in LoginButtonStyle)

## [2026-03-04] - Main Navigation & Home Page Implementation

### Added
- **TabBar Redesign**: Updated `AppShell.xaml` TabBar properties to match the design mockup exactly. Set `TabBarBackgroundColor` to pure black (`PageBackgroundColor`), `TabBarTitleColor` and `TabBarForegroundColor` to `PrimaryRed` for the active tab, and `TabBarUnselectedColor` to `White` for inactive tabs.
- **TabBar Navigation**: Replaced basic Shell navigation with a full `TabBar` containing Home, Search, Library, and Events sections.
- **New Icons**: Added SVG icons for the bottom navigation bar (`home_icon.svg`, `search_icon.svg`, etc.).
- **Main Pages**: Created `HomePage`, `SearchPage`, `LibraryPage`, and `EventsPage` with their respective ViewModels.
- **Rich Layouts**: Implemented detailed, dark-themed layouts for all main sections:
    - **HomePage**: Personalized dashboard with quick access, featured cards, and horizontal collections.
    - **SearchPage**: Functional search bar, filter chips, and a "Browse All" genre grid.
    - **LibraryPage**: List of playlists, artists, and albums with a special "Liked Songs" tile.
    - **EventsPage**: Upcoming shows list with date badges and featured event cards.
- **Persistent Mini Player**: Added a consistent sticky playback control bar at the bottom of all main pages.
- **Mock Data**: Populated all ViewModels with realistic mock data to facilitate UI development and testing.

### Changed
- **Auth Flow**: Updated `LoginPageViewModel` to route users directly to the new `HomePage` upon successful login.
- **DI Registration**: Registered all new pages and viewmodels in `MauiProgram.cs`.

### Fixed
- **Android ANR Crash**: Fixed an "Application Not Responding" issue on Android caused by nesting `CollectionView` controls within a `ScrollView`. Replaced `CollectionView` with `BindableLayout` (using `FlexLayout` and `HorizontalStackLayout`) to resolve the layout measurement loop while maintaining the same design.

## [2026-03-01] - UI Refactoring & Critical Authentication Fixes

### Added
- **InputEntryControl**: Created a reusable user control for text and password inputs to centralize styling and reduce boilerplate in `LoginPage.xaml` and `SignUpPage.xaml`.
- **PrimaryRedMuted**: Added a new color resource (`#99D90429`) for the "music" part of the logo to match design specs without using `Opacity` on individual labels.
- **Database Migration**: Added `UpdateSeedData` migration to correct seeded user credentials and normalized fields in the Identity Server database.

### Changed
- **Logo Refactoring**: Updated logos in `LoginPage` and `SignUpPage` to use a single `Label` with `FormattedString` instead of `HorizontalStackLayout`, improving layout performance and code readability.
- **Auth Flow Debugging**: Identified and resolved a critical issue where seeded users (`admin@innowisemusic.com`) were stuck with old `bookstore.com` normalized identities and incorrect email spellings.
- **Docker Cleanup**: Forced a full database volume wipe (`docker compose down -v`) to ensure the fixed Entity Framework seed data was correctly applied to the Postgres instance.

### Fixed
- **Authentication Failure**: Fixed 401 Unauthorized errors caused by mismatched `NormalizedEmail` and `NormalizedUserName` fields in the Identity Server's seed data.
- **XAML Boilerplate**: Reduced code duplication by abstracting `Border` and `Entry` combinations into the new `InputEntryControl`.

### Added
- **Docker Compose**: Created `docker-compose.yml` to orchestrate PostgreSQL, Seq (logging), and Identity Server containers.
- **Dockerfile**: Added multi-stage `Dockerfile` and `.dockerignore` to containerize the `Innowise.MusicIdentityServer` project.

---

## [2026-02-27] - JWT Authentication Implementation

### Added
- **Auth Models**: Added `LoginUserDto`, `UserDto`, and `AuthenticationResponse` to the MAUI project to match IdentityServer models.
- **IAuthService / AuthService**: Implemented authentication service using `HttpClient` and `SecureStorage` for token management.

### Changed
- **HttpHelper**: Updated to automatically include JWT Bearer token in the `Authorization` header if available in `SecureStorage`.
- **MauiProgram.cs**: Registered `IAuthService` and `AuthService`.
- **LoginPageViewModel**: Hooked up `AuthService` to the Login command.
- **SignUpPageViewModel**: Hooked up `AuthService` to the Sign Up command.

### Removed
- **ApiAthenticationStateProvider**: Deleted the Blazor-specific authentication provider as it is not applicable for pure XAML MAUI projects.

---

## [2026-02-25] - Auth Pages Design Improvements

### Added
- **Google Logo**: Integrated Google logo asset in `LoginPage.xaml` and `SignUpPage.xaml`.
- **NavigationService**: Created `INavigationService` and `NavigationService` to abstract MAUI Shell navigation, improving testability and adhering to MVVM best practices. Registered in `MauiProgram.cs`.

### Changed
#### ViewModels
- Refactored `LoginPageViewModel`, `SignUpPageViewModel`, and `NewsPageViewModel` to use `INavigationService` instead of hardcoded `Shell.Current.GoToAsync` calls.

#### LoginPage.xaml
- **Google Button**: Replaced standard `Button` with `Border` control matching design (white stroke, rounded corners, Google icon).
- **Entry Fields**: Wrapped Email and Password entries in `Border` controls for rounded corners matching design mockup.
- **Logo**: Changed from single `Label` to `HorizontalStackLayout` for two-tone effect.
- **Login Button**: Replaced standard `Button` with `Border` control for styled text spans.

#### SignUpPage.xaml
- Updated to match `LoginPage.xaml` design and architecture:
    - Wrapped `Entry` controls in `Border` for consistent styling.
    - Updated Logo to use `HorizontalStackLayout` for two-tone effect.
    - Updated Sign Up and Google buttons to use `Border` style for consistent branding.

### Fixed
- Entry fields now have proper rounded corners as per design specification.
- Logo text now visually distinguishes "inno" vs "music" portions.

---

## [2026-02-23] - Initial Development Phase

### Added

#### Project Infrastructure
- .NET 9 MAUI project setup
- MVVM architecture with CommunityToolkit.Mvvm
- Dependency Injection configuration in `MauiProgram.cs`
- Shell-based navigation in `AppShell.xaml.cs`

#### Models
- `News` model with Id, Title, Content, ImageUrl properties

#### Services
- `INewsService` - Interface for news data access
- `WebNewsService` - REST API client for news retrieval
- `MockNewsService` - Mock implementation for testing
- `IHttpHelper` / `HttpHelper` - HTTP client configuration with SSL bypass for localhost

#### Views
- `LoginPage` - Login screen with email/password entries, Google SSO placeholder
- `SignUpPage` - Registration screen with email/password/repeat-password entries
- `NewsPage` - News list with CollectionView
- `NewsDetailedPage` - News detail view with image and content

#### ViewModels
- `LoginPageViewModel` - Navigation to SignUp page
- `SignUpPageViewModel` - Navigation to Login page
- `NewsPageViewModel` - News collection management and navigation to details
- `NewsDetailedPageViewModel` - News detail display with QueryProperty support

#### Resources & Styles (App.xaml)
- Color palette: PrimaryRed, PrimaryRedLight, PageBackgroundColor, InputBackgroundColor, etc.
- LinearGradientBrush for backgrounds and buttons
- Reusable styles: EmailEntryStyle, PasswordEntryStyle, LoginButtonStyle, GoogleButtonStyle, SignUpLabelStyle, LogoLabelStyle

### Changed
- N/A (Initial implementation)

### Fixed
- N/A (Initial implementation)

---

## Summary of Completed Features

### Authentication Module
- Login page with email/password input
- Sign-up page with email/password/repeat-password input
- Navigation between login and sign-up flows
- Google SSO button (UI only, integration pending)

### News Module
- News list view with image thumbnails
- News detail view with full content
- REST API integration (endpoint: `/getnews`)
- Mock data service for offline/development use
- Platform-specific URL handling (Android emulator vs. desktop)

### Architecture
- MVVM pattern with ObservableObject and RelayCommand
- Dependency Injection for all Pages and ViewModels
- Shell navigation with route registration
- Compiled bindings with x:DataType for performance
