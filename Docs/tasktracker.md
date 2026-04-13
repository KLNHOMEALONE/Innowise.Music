# Task Tracker - Innowise.Music

## Project Overview
**Innowise.Music** — Cross-platform music streaming app (.NET 9 MAUI)
- **Client:** Innowise.Music (iOS/Android/macOS/Windows)
- **API:** Innowise.MusicIdentityServer (ASP.NET Core 9, PostgreSQL, JWT)
- **Admin:** Innowise.Music.Admin (Blazor Server)

## Key Files Reference

| Component | File |
|-----------|------|
| MAUI DI Setup | `Innowise.Music/MauiProgram.cs` |
| Audio Playback | `Innowise.Music/Services/AudioService.cs` |
| History Service | `Innowise.Music/Services/HistoryService.cs` |
| Favorite Service | `Innowise.Music/Services/FavoriteService.cs` |
| Recommendations | `Innowise.Music/Services/RecommendationService.cs` |
| Home Page | `Innowise.Music/View/HomePage.xaml.cs` |
| Home VM | `Innowise.Music/ViewModel/HomePageViewModel.cs` |
| MiniPlayer VM | `Innowise.Music/ViewModel/MiniPlayerViewModel.cs` |
| Music Controller | `Innowise.MusicIdentityServer/Controllers/MusicController.cs` |
| Auth Controller | `Innowise.MusicIdentityServer/Controllers/AuthenticationController.cs` |
| Admin Music Controller | `Innowise.MusicIdentityServer/Controllers/AdminMusicController.cs` |
| Music Service | `Innowise.MusicIdentityServer/Services/MusicService.cs` |
| DB Context | `Innowise.MusicIdentityServer/Data/MusicIdentityDbContext.cs` |
| UserRecentTrack Entity | `Innowise.MusicIdentityServer/Models/Music/UserRecentTrack.cs` |
| UserFavoriteTrack Entity | `Innowise.MusicIdentityServer/Models/Music/UserFavoriteTrack.cs` |
| Admin Service | `Innowise.Music.Admin/Services/AdminMusicService.cs` |
| Docker Compose | `docker-compose.yml` |
| App Settings | `Innowise.Music/appsettings.json` |

---

## Task: Listening History - Recently Played Tracks
- **Status**: Completed
- **Description**: Implemented per-user listening history. When a user plays any track, it's recorded in the `UserRecentTracks` table (max 5 most recent per user). The "Get Back to Listening" section on the HomePage shows these tracks in real-time.
- **Steps**:
  - [x] Created `UserRecentTrack` entity with `UserId`, `TrackId`, and `PlayedAt` fields
  - [x] Added `DbSet<UserRecentTracks>` to `MusicIdentityDbContext` with indexes on `UserId` and `(UserId, PlayedAt)`
  - [x] Created EF migration `AddUserRecentTracks` and applied to database
  - [x] Added `RecordListeningHistoryAsync` and `GetRecentTracksAsync` to `IMusicService` / `MusicService`
  - [x] Added `POST /api/Music/tracks/{id}/history` endpoint — upserts track, trims to 5 most recent
  - [x] Added `GET /api/Music/history/recent?count=N` endpoint — returns user's N most recent tracks
  - [x] Created `IHistoryService` / `HistoryService` on MAUI client
  - [x] Registered `IHistoryService` in `MauiProgram.cs`
  - [x] Wired `MiniPlayerViewModel.PlayTrack()` to record history via `RefreshHistoryAsync()`
  - [x] Added `MessagingCenter` pub/sub between `MiniPlayerViewModel` and `HomePageViewModel` for real-time UI refresh
  - [x] Added `LoadRecentItemsAsync()` to `HomePageViewModel` — fetches real data from backend
  - [x] Called `LoadRecentItemsAsync()` from `HomePage.OnAppearing()`
  - [x] Fixed JWT `sub` claim bug — was set to `user.UserName` (email) instead of `user.Id` (GUID), causing FK violations
  - [x] Removed mock data fallback from `RecentItems` — shows only real user data
  - [x] Clear `RecentItems` on logout to prevent cross-user data leakage
  - [x] Fixed bug where clicking a new track while paused played the old track — `AudioService.Play()` now checks if the source URL changed before resuming
- **Files**: `Innowise.MusicIdentityServer/Models/Music/UserRecentTrack.cs`, `Innowise.MusicIdentityServer/Data/MusicIdentityDbContext.cs`, `Innowise.MusicIdentityServer/Services/MusicService.cs`, `Innowise.MusicIdentityServer/Controllers/MusicController.cs`, `Innowise.MusicIdentityServer/Controllers/AuthenticationController.cs`, `Innowise.Music/Services/HistoryService.cs`, `Innowise.Music/Services/AudioService.cs`, `Innowise.Music/ViewModel/HomePageViewModel.cs`, `Innowise.Music/ViewModel/MiniPlayerViewModel.cs`, `Innowise.Music/View/HomePage.xaml.cs`, `Innowise.Music/MauiProgram.cs`

## Task: Fix Audio Resume After Pause
- **Status**: Completed
- **Description**: When a track was paused and then play was pressed again, the audio would restart from the beginning instead of resuming from where it was paused. The root cause was `AudioService.Play()` unconditionally calling `Stop()` and reassigning the source when the player was in `Playing` or `Paused` state.
- **Steps**:
  - [x] Modified `AudioService.Play()` to check if player is `Paused` — if so, call `_mediaElement.Play()` directly to resume
  - [x] Kept stop-and-reassign logic only for `Playing` state (changing tracks) or `Failed` state
- **Files**: `Innowise.Music/Services/AudioService.cs`

## Task: Fix Featuring Songs Loading from Backend
- **Status**: Completed
- **Description**: The "Featuring songs you like" section was not loading recommendations from the backend. The root cause was `HomePageViewModel` (a singleton) calling `LoadRecommendationsAsync()` in its constructor — which ran at app startup before the user was logged in. The API returned 401 (no auth token) and recommendations were never fetched again after login. Additionally, `RecommendationService` and `StreamTokenService` shared a singleton `HttpClient` and both mutated its `Authorization` header, causing race conditions.
- **Steps**:
  - [x] Moved `LoadRecommendationsAsync()` from `HomePageViewModel` constructor to `HomePage.OnAppearing()` — ensures recommendations load after authentication
  - [x] Made `LoadRecommendationsAsync()` public so it can be called from the view
  - [x] Replaced shared singleton `HttpClient` in `RecommendationService` with per-call `HttpClient` creation via `HttpHelper.GetInsecureHandler()`
  - [x] Same fix applied to `StreamTokenService`
  - [x] Reverted `MauiProgram.cs` to original singleton `HttpClient` registration (still used by `AuthenticationService` and `GoogleAuthService`)
- **Files**: `Innowise.Music/ViewModel/HomePageViewModel.cs`, `Innowise.Music/View/HomePage.xaml.cs`, `Innowise.Music/Services/RecommendationService.cs`, `Innowise.Music/Services/StreamTokenService.cs`, `Innowise.Music/MauiProgram.cs`

## Task: Fix Dashboard Statistics, JSON Serialization & Login Styling
- **Status**: Completed
- **Description**: Fixed three issues: (1) claim type mismatch causing 401 errors, (2) JSON circular reference in Albums API, (3) login page styling inconsistent with dashboard dark theme.
- **Steps**:
  - [x] Fixed claim type mismatch - changed from `ClaimTypes.NameIdentifier` to `"uid"` in `AuthService`
  - [x] Added error handling in `AdminMusicService` methods
  - [x] Added `ILogger<Dashboard>` to `Dashboard.razor`
  - [x] Fixed JSON serialization cycle by adding `ReferenceHandler.IgnoreCycles` to `Program.cs` in Identity Server
  - [x] Updated `Login.cshtml` to remove inline styles and use dark theme CSS classes from `app.css`
  - [x] Rebuilt Docker containers and verified deployment
- **Dependencies**: Innowise.MusicIdentityServer

## Task: Fix Dashboard Statistics & JSON Serialization
- **Status**: Completed
- **Description**: Fixed two issues with the dashboard: (1) claim type mismatch causing 401 errors and (2) JSON circular reference in Albums API.
- **Steps**:
  - [x] Fixed claim type mismatch - changed from `ClaimTypes.NameIdentifier` to `"uid"` in `AuthService`
  - [x] Added error handling in `AdminMusicService` methods
  - [x] Added `ILogger<Dashboard>` to `Dashboard.razor`
  - [x] Fixed JSON serialization cycle by adding `ReferenceHandler.IgnoreCycles` to `Program.cs` in Identity Server
  - [x] Rebuilt Docker containers and verified deployment
- **Dependencies**: Innowise.MusicIdentityServer

## Task: Fix Dashboard Statistics Authentication Issue
- **Status**: Completed
- **Description**: Fixed the dashboard showing incorrect counts (0) for Artists, Genres, Albums, and Tracks. The root cause was a claim type mismatch - the Identity Server uses `"uid"` for user ID but the Admin Dashboard was looking for `ClaimTypes.NameIdentifier`, causing the token to never be cached and API calls to fail with 401.
- **Steps**:
  - [x] Identified claim type mismatch between Identity Server (`"uid"`) and Admin Dashboard (`ClaimTypes.NameIdentifier`)
  - [x] Updated `AuthService.LoginAndGetPrincipalAsync()` to extract user ID using `FindFirst("uid")`
  - [x] Updated `AuthService.GetTokenAsync()` to use `FindFirst("uid")` for consistent claim extraction
  - [x] Added try-catch error handling in `AdminMusicService` methods for graceful API failure handling
  - [x] Added `ILogger<Dashboard>` to `Dashboard.razor` for proper logging
  - [x] Rebuilt Docker containers and verified deployment
- **Dependencies**: Innowise.MusicIdentityServer

## Task: Admin Dashboard Authentication Refactor
- **Status**: Completed
- **Description**: Complete rewrite of the admin dashboard authentication system using standard ASP.NET Core Identity Cookies and server-side `IMemoryCache` for JWT token storage. This replaced the previous custom in-memory storage and moved away from the browser local storage approach.
- **Steps**:
  - [x] Configured standard `CookieAuthenticationDefaults` in `Program.cs`
  - [x] Implemented `IMemoryCache` for server-side JWT token persistence
  - [x] Deleted old `PersistentAuthenticationStateProvider`, custom `AuthorizeRouteView.razor`, custom `AuthorizeView.razor`
  - [x] Updated `App.razor` with `<CascadingAuthenticationState>` + built-in `<AuthorizeRouteView>` + `<RedirectToLogin />`
  - [x] Changed `_Host.cshtml` from `ServerPrerendered` to `Server` to ensure a stable Blazor circuit for auth
  - [x] Updated `Program.cs` with standard `AuthenticationStateProvider` registration
  - [x] Rewrote `AuthService.cs` to use `HttpContext.SignInAsync` and `IMemoryCache`
  - [x] Updated `Login.razor` to handle the cookie-based sign-in flow
  - [x] Created `Logout.razor` page with async logout and navigation to `/login`
  - [x] Updated `MainLayout.razor` with logout link and async `GetTokenAsync()`
  - [x] Added `@attribute [Authorize]` to all 11 protected pages
  - [x] Removed `@rendermode InteractiveServer` from components (handled by Blazor Server defaults)
  - [x] Updated `AdminMusicService.cs` to use `GetTokenAsync()` for bearer token injection
  - [x] Updated `_Imports.razor` with auth-related usings
  - [x] Verified build succeeds with 0 errors
  - [x] Updated changelog.md, tasktracker.md, admin-dashboard.md
- **Dependencies**: Innowise.MusicIdentityServer

## Task: Fix Admin Dashboard Black Screen
- **Status**: Completed
- **Description**: The admin dashboard was showing a black screen when accessed. The root cause was the auto-generated `Routes` component in .NET 9 not using our custom `AuthorizeRouteView`, the authentication check happening after the page rendered (causing a black screen flash), and the login form submit not firing.
- **Steps**:
  - [x] Investigated the routing system and identified that `Routes` component was auto-generated and not using `AuthorizeRouteView`
  - [x] Created custom `Routes.razor` using Blazor's `Router` component with `AuthorizeRouteView`
  - [x] Updated `AuthorizeRouteView.razor` to check authentication in `OnInitializedAsync` before rendering
  - [x] Added loading state display during authentication checks
  - [x] Fixed `Shared/AuthorizeView.razor` to show loading indicator when redirecting to login
  - [x] Fixed login form submission by changing `@onsubmit` to `@onsubmit.prevent` in `Login.razor`
  - [x] Verified build succeeds with no errors
- **Dependencies**: None

## Task: Fix Audio Player
- **Status**: Completed
- **Description**: The mini audio player was not playing selected tracks, and the progress bar was not updating. The root cause was a missing `MediaElement` in the `MiniPlayerControl` and a lack of initialization in the `AudioService`.
- **Steps**:
  - [x] Investigated `AudioService` and identified incorrect playback logic.
  - [x] Refactored `AudioService` to use `ShouldAutoPlay` and proper state handling.
  - [x] Investigated `MiniPlayerViewModel` and improved its state synchronization with the audio service.
  - [x] Discovered the missing `MediaElement` in `MiniPlayerControl.xaml`.
  - [x] Added the `MediaElement` to the XAML and implemented the `AudioService.Initialize` call in the code-behind.
  - [x] Verified the fix with the user.
- **Dependencies**: None

## Task: Implement Input Validation System
- **Status**: Completed
- **Description**: Implemented a comprehensive input validation system for the Login and Sign-up pages, including UI feedback and automatic validation on text change.
- **Steps**:
    - [x] Enhanced `InputEntryControl` with error message display and dynamic border styling.
    - [x] Created `FirstValidationErrorConverter` to extract the first error message from validation results.
    - [x] Updated `LoginPageViewModel` to use `ValidatableObject<string>` for Email and Password, and added validation rules (`EmailRule`, `IsNotNullOrEmptyRule`).
    - [x] Updated `LoginPage.xaml` to bind to `ValidatableObject<string>` properties and display validation feedback.
    - [x] Created `CompareRule` for password confirmation.
    - [x] Updated `SignUpPageViewModel` to use `ValidatableObject<string>` for Email, Password, Repeat Password, First Name, and Last Name, and added validation rules (`EmailRule`, `IsNotNullOrEmptyRule`, `CompareRule`).
    - [x] Updated `SignUpPage.xaml` to bind to `ValidatableObject<string>` properties, include `FirstName` and `LastName` input controls, and display validation feedback.
    - [x] Renamed `IsNullOrEmptyRule.cs` to `IsNotNullOrEmptyRule.cs` for consistency.
    - [x] Added `CommunityToolkit.Maui` Nuget package.
    - [x] Initialized `CommunityToolkit.Maui` in `MauiProgram.cs`.
    - [x] Added `TextChangedCommand` to `InputEntryControl`'s code-behind.
    - [x] Integrated `EventToCommandBehavior` in `InputEntryControl.xaml` to trigger ViewModel validation commands on text changes.
    - [x] Added individual validation commands to `LoginPageViewModel` and `SignUpPageViewModel`.
    - [x] Resolved `MVVMTK0034` warnings in ViewModels.
    - [x] Fixed Android UI rendering issue in `InputEntryControl.xaml` by changing `VerticalStackLayout` to `Grid`.
- **Dependencies**: `CommunityToolkit.Mvvm`, `CommunityToolkit.Maui`

## Task: Fix Login 500 Error
- **Status**: Completed
- **Description**: The login process was failing with a 500 Internal Server Error. The root cause was the Google authentication middleware crashing the application on startup due to a missing `ClientId` in the configuration.
- **Steps**:
    - [x] Investigated the `Login` method in the `AuthenticationController`.
    - [x] Analyzed the server logs to identify the `System.ArgumentNullException: Value cannot be null. (Parameter 'ClientId')` error.
    - [x] Identified that the Google authentication middleware was being loaded on startup, even when not explicitly used.
    - [x] Temporarily disabled the Google authentication middleware in `Program.cs` to prevent the application from crashing.
    - [x] Confirmed with the user that the login process is now working as expected.
- **Dependencies**: None

## Task: Debug Google Authentication Infinite Loop on Android
- **Status**: Completed
- **Description**: User reported that the Google login flow on Android would result in an infinite loop, where the browser would not correctly return control to the application after authentication. The investigation pointed to a mismatch between the URI format Google expects and the format the Android Intent Filter could parse. However, all code-based solutions led to other configuration or policy errors from Google. The final resolution was discovered to be an environmental issue.
- **Steps**:
    - [x] Analyzed `GoogleAuthService.cs` and `WebAuthenticationCallbackActivity.cs`.
    - [x] Hypothesized a mismatch between the opaque `redirectUri` (`scheme:/path`) and the `IntentFilter`'s `DataPathPrefix`.
    - [x] Attempted changing the `redirectUri` to a hierarchical format (`scheme://host`) and updating the filter to use `DataHost`.
    - [x] Diagnosed subsequent Google policy errors (`400 invalid_request`, `access_blocked`) related to client configuration and app verification status.
    - [x] Guided user to check SHA-1 fingerprint and OAuth consent screen test users.
    - [x] Reverted code to its original state upon user request.
    - [x] The issue was ultimately resolved by the user resetting the Android emulator to factory defaults, which cleared a corrupted state cache.
- **Dependencies**: None

## Task: Google Authentication
- **Status**: Completed
- **Description**: Implement Google authentication using a WebView and a new endpoint on the Identity Server.
- **Steps**:
    - [x] Create `WebPage.xaml` and `WebPage.xaml.cs` to host a `WebView`
    - [x] Create `WebPageViewModel` to manage the state of the `WebPage`
    - [x] Register `WebPage` and `WebPageViewModel` in `MauiProgram.cs`
    - [x] Register route for `WebPage` in `AppShell.xaml.cs`
    - [x] Update `GoogleAuthService` to use the `WebPage`
    - [x] Update `AppShell` to handle the authentication result from the `WebPage`
    - [x] Update `WebPage.xaml.cs` to set the authentication result
    - [x] Update `GoogleAuthService` to use `Shell.Current.GoToAsync`
    - [x] Add `Google.Apis.Auth` nuget package to `Innowise.MusicIdentityServer`
    - [x] Create `GoogleTokenDto`
    - [x] Update `AuthenticationController` to handle Google login
    - **Dependencies**: None

## Task: Project Reorganization
- **Status**: Completed
- **Description**: Relocate solution file and verify paths for multi-project support.
- **Steps**:
    - [x] Move `Innowise.Music.sln` to root
    - [x] Verify project references in `.sln`
    - [x] Verify Docker Compose paths
    - [x] Update documentation (project.md, changelog.md)

## Task: Project Setup and Core Infrastructure
- **Status**: Completed
- **Description**: Initial MAUI project setup with .NET 9, MVVM pattern, and dependency injection
- **Steps**:
    - [x] Create MAUI project structure
    - [x] Configure DI in MauiProgram.cs
    - [x] Set up AppShell with routing
    - [x] Define shared resources in App.xaml

## Task: Authentication Flow (Login/SignUp)
- **Status**: Completed
- **Description**: Implement login and sign-up pages with MVVM pattern and JWT authentication.
- **Steps**:
    - [x] Create LoginPage.xaml with ViewModel
    - [x] Create SignUpPage.xaml with ViewModel
    - [x] Implement navigation between Login and SignUp
    - [x] Apply shared styles and gradients
    - [x] Match UI design with mockups (Borders, Logos, Branded Buttons)
    - [x] Create Auth models (DTOs) in the MAUI project
    - [x] Implement IAuthService and AuthService using SecureStorage
    - [x] Update HttpHelper to include Bearer token
    - [x] Register services in MauiProgram.cs
    - [x] Update LoginPageViewModel and SignUpPageViewModel
    - [x] Implement startup navigation logic based on auth state
    - [x] Remove Blazor-specific AuthenticationStateProvider
    - [x] Create reusable `InputEntryControl` for form fields
    - [x] Fix IdentityServer seed data typos and normalization issues
    - [x] Implement Refresh Token functionality (IdentityServer & MAUI Client)
- **Dependencies**: Innowise.MusicIdentityServer

## Task: News Feature
- **Status**: Completed (feature removed — dead code cleanup)
- **Description**: News listing and detailed view with API integration. Removed as part of dead code cleanup since the feature was not wired into any navigation UI.
- **Steps**:
    - [x] Create News model
    - [x] Implement INewsService interface
    - [x] Create WebNewsService for API calls
    - [x] Create MockNewsService for testing
    - [x] Implement HttpHelper for SSL handling
    - [x] Create NewsPage with CollectionView
    - [x] Create NewsDetailedPage
    - [x] Implement navigation with QueryProperty
    - [x] ~~Add error handling for API failures~~
    - [x] ~~Add loading states~~
    - [x] **Removed**: All News feature files deleted (dead code — not integrated into app navigation)

## Task: Backend Dockerization
- **Status**: Completed
- **Description**: Containerize backend services (Postgres, Seq, Identity Server) for local development using Docker Compose.
- **Steps**:
    - [x] Create Dockerfile for Innowise.MusicIdentityServer
    - [x] Create .dockerignore for Innowise.MusicIdentityServer
    - [x] Create docker-compose.yml at workspace root
    - [x] Configure environment variables and network overrides for containers
    - [x] Fix service naming for Visual Studio compatibility
    - [x] Configure HTTPS port mapping (7008)
    - [x] Verify build and context paths

## Task: Implement Audio Streaming Core
- **Status**: Completed
- **Description**: Implemented core audio playback functionality using `MediaElement` from `CommunityToolkit.Maui`. Integrated with `MiniPlayerControl` for continuous playback across the application.
- **Notes**:
    - Used `CommunityToolkit.Maui.MediaElement` version `3.0.0` for .NET 9 compatibility.
    - Integrated `MediaElement` into `AppShell.xaml` for continuous background playback.
    - Implemented `IAudioService` and `AudioService` for centralized playback control.
    - Refactored `MiniPlayerViewModel` to expose playback state and control commands.
    - Updated `MiniPlayerControl.xaml` to display track information, playback controls, and progress bar dynamically.
    - Resolved XAML binding issues within `DataTemplate` for command invocation from `HomeItem` by introducing a `Parent` reference in `HomeItem` back to `HomePageViewModel`.
    - Refactored `MiniPlayerControl.xaml.cs` to correctly instantiate `MiniPlayerViewModel` using `Handler.MauiContext.Services.GetService<MiniPlayerViewModel>()` in the `Loaded` event, resolving XAML compilation errors.
- **Dependencies**: `CommunityToolkit.Maui.MediaElement`

## Task: Batch Track Upload Feature (Admin Dashboard)
- **Status**: Completed
- **Description**: Implemented complete batch upload functionality for music tracks with automatic metadata extraction using TagLibSharp.
- **Steps**:
    - [x] Created `TrackUploadDto.cs` models in both Identity Server and Admin projects
    - [x] Implemented `IMetadataExtractionService` and `MetadataExtractionService` using TagLibSharp
    - [x] Added batch upload methods to `IMusicService` and `MusicService` (GetOrCreateArtist, GetOrCreateAlbum, GetOrCreateGenres)
    - [x] Added `POST /api/admin/tracks/upload-batch` endpoint to `AdminMusicController`
    - [x] Added `UploadTracksBatchAsync()` to `IAdminMusicService` and `AdminMusicService`
    - [x] Created `MultiTrackUpload.razor` component with file selection, metadata preview, and upload
    - [x] Updated `TracksList.razor` with "Add Tracks" button
    - [x] Fixed Blazor Server interop issues (render mode, StateHasChanged, disabled attribute syntax)
    - [x] Updated documentation (changelog.md)
- **Dependencies**: TagLibSharp, Identity Server API

## Task: Admin Dashboard CRUD Operations
- **Status**: Completed
- **Description**: Implemented full CRUD operations in the Blazor Admin dashboard for all four entity types: Genres, Artists, Albums, and Tracks.
- **Steps**:
    - [x] **Backend API** — Created `AdminMusicController` with full CRUD endpoints for all entities (GET list, GET by id, POST create, PUT update, DELETE)
    - [x] **Service Layer** — Implemented CRUD methods in `MusicService` and `AdminMusicService`
    - [x] **Authorization** — Added `[Authorize(Roles = "Administrator")]` to all admin endpoints
    - [x] **Genres UI** — Created `GenresList.razor` (paginated list with delete) and `GenreForm.razor` (create/edit form)
    - [x] **Artists UI** — Created `ArtistsList.razor` (paginated list with delete) and `ArtistForm.razor` (create/edit with validation)
    - [x] **Albums UI** — Created `AlbumsList.razor` (paginated list with delete) and `AlbumForm.razor` (create/edit with artist dropdown)
    - [x] **Tracks UI** — Created `TracksList.razor` (paginated list with delete), `TrackForm.razor` (create/edit with artist/album dropdowns, genre checkboxes), `TrackUpload.razor` (single track audio upload)
    - [x] **Pagination** — Added pagination support for all list views
    - [x] **Validation** — Added form validation for all create/edit forms
- **Dependencies**: Innowise.MusicIdentityServer

## Task: Music Streaming Service Implementation (Phase 1 - MVP)
- **Status**: Completed
- **Description**: Implemented core music streaming functionality with API endpoints for music search, discovery, and playback.
- **Steps**:
    - [x] **Database Setup**
        - [x] Create database models (Artists, Albums, Tracks, Genres) with BYTEA audio storage
        - [x] Update MusicIdentityDbContext with DbSets and full-text search configuration
        - [x] Create database migration (AddMusicTables) with pg_trgm extension
        - [x] Apply migration to PostgreSQL database
    - [x] **Backend API Development**
        - [x] Create MusicController with essential endpoints
        - [x] Implement `GET /music/recommendations` — Personalized recommendations
        - [x] Implement `GET /music/tracks/{id}/stream` — Stream audio with range request support
        - [x] Implement `GET /music/tracks/{id}/stream-token` — Signed streaming token
        - [x] Implement `GET /music/artists/{id}/top-tracks` — Artist's popular tracks
        - [x] Implement `GET /music/albums/{id}/tracks` — Album tracks
        - [x] Add audio streaming with proper Content-Type and range request headers
    - [x] **Service Layer**
        - [x] Create IMusicService interface
        - [x] Implement MusicService with database queries and Include for related data
        - [x] Implement search using ILike for case-insensitive matching
        - [x] Register service in Program.cs Dependency Injection
    - [x] **MAUI Client Integration**
        - [x] Update Track model to include all metadata fields
        - [x] Create RecommendationService for recommendations API
        - [x] Create StreamTokenService for signed token streaming
        - [x] Update HomePage to fetch and display real recommendations
        - [x] Implement track selection and playback from recommendations
        - [x] Handle audio streaming with signed tokens and proper error handling
- **Dependencies**: PostgreSQL database, Existing authentication system

## Task: Music Library
- **Status**: Not started
- **Description**: Track and playlist management
- **Steps**:
    - [ ] Create Track and Playlist models
    - [ ] Implement database context (PostgreSQL)
    - [ ] Create repository pattern for data access
    - [ ] Build library UI

## Task: Search Functionality
- **Status**: Not started
- **Description**: Search for tracks, artists, albums
- **Steps**:
    - [ ] Create search service
    - [ ] Implement search UI
    - [ ] Add filters and sorting

## Task: User Profile
- **Status**: Not started
- **Description**: User settings and preferences
- **Steps**:
    - [ ] Create profile page
    - [ ] Implement settings storage
    - [ ] Add theme customization

## Task: Favorites & Playlists
- **Status**: In progress (Favorites implemented, Playlists pending)
- **Description**: Allow users to save favorites and create playlists
- **Steps**:
    - [x] Create favorite tracks feature
    - [ ] Implement playlist CRUD operations
    - [ ] Build playlist UI

### Completed: Favorite Tracks Feature

- **Description**: Implemented a toggle favorite feature for tracks. The mini player displays a heart icon that is filled (♥) with red background when the track is favorited, or outlined (♡) with transparent background when not. Clicking the heart toggles the status.
- **Backend changes**:
  - [x] Created `UserFavoriteTrack` entity with `Id`, `UserId`, `TrackId`, `CreatedAt`
  - [x] Added `DbSet<UserFavoriteTrack>` and unique index on `(UserId, TrackId)` to `MusicIdentityDbContext`
  - [x] Added `ToggleFavoriteAsync`, `IsFavoriteAsync`, `GetFavoritesAsync` to `IMusicService` / `MusicService`
  - [x] Added `POST /api/Music/tracks/{id}/favorite` — toggles favorite, returns `{ isFavorite: bool }`
  - [x] Added `GET /api/Music/tracks/{id}/is-favorite` — returns `{ isFavorite: bool }`
  - [x] Added `GET /api/Music/favorites` — returns user's favorite tracks
  - [x] Created and applied EF Core migration `AddUserFavoriteTracks`
- **Frontend changes**:
  - [x] Created `IFavoriteService` / `FavoriteService` with `ToggleFavoriteAsync`, `IsFavoriteAsync`, and `GetAllFavoritesAsync`
  - [x] Registered `IFavoriteService` in `MauiProgram.cs`
  - [x] Added `IsFavorite` property and `ToggleFavoriteCommand` to `MiniPlayerViewModel`
  - [x] `PlayTrack()` now checks favorite status via `RefreshFavoriteStatusAsync()`
  - [x] Created `FavoriteTextConverter` (uses U+FE0E text variant selector for cross-platform heart rendering) and `FavoriteBackgroundConverter`
  - [x] Created `favorite_outline_icon.svg` image resource
  - [x] Updated `MiniPlayerControl.xaml` — replaced static checkmark with interactive heart toggle (40x40, matching play/pause button)
  - [x] Updated `BoolToFavoriteIconConverter` to return outlined heart for unfavorited state (used by SearchPage)
  - [x] Quick Access section on homepage displays user's favorite tracks (6 random if > 6, all if ≤ 6)
  - [x] Quick Access items are clickable and play the selected track

## Task: Testing
- **Status**: Not started
- **Description**: Unit and integration tests
- **Steps**:
    - [ ] Set up test project
    - [ ] Write ViewModel tests
    - [ ] Write service tests
    - [ ] Integration tests for API

## Task: Documentation
- **Status**: In progress
- **Description**: Project documentation and code comments
- **Steps**:
    - [x] Create tasktracker.md
    - [x] Create changelog.md
    - [x] Update project.md with architecture (Validation system added)
    - [ ] Add API documentation

## Task: Architectural Improvements (Navigation)
- **Status**: Completed
- **Description**: Abstract Shell navigation into an injectable service.
- **Steps**:
    - [x] Create INavigationService interface
    - [x] Implement NavigationService
    - [x] Register in Dependency Injection container
    - [x] Refactor existing ViewModels to use the new service

## Task: TabBar and Main Pages Implementation
- **Status**: Completed
- **Description**: Implement main navigation using a TabBar, create Home, Search, Library, and Events pages with ViewModels, and update login navigation flow to point to Home.
- **Steps**:
    - [x] Update `AppShell.xaml` to use `<TabBar>` with four tabs.
    - [x] Update `LoginPageViewModel` to navigate to `///HomePage` after login.
    - [x] Create basic XAML Views for Home, Search, Library, and Events.
    - [x] Implement `HomePage.xaml` layout (header, pills, featured card, horizontal collections, sticky mini-player).
    - [x] Implement `SearchPage.xaml` layout (search bar, filter chips, recents grid, sticky mini-player).
    - [x] Implement `LibraryPage.xaml` layout (list view with play buttons/menus, sticky mini-player).
    - [x] Implement `EventsPage.xaml` placeholder.
    - [x] Setup `EventsPage.xaml` placeholder.
    - [x] Populate ViewModels with mock data (Initial mock data for all main pages).
    - [x] Register new pages and ViewModels in `MauiProgram.cs`.
    - [x] Ensure dark mode colors (`App.xaml`) and accents match designs.

## Task: Address PrimaryRed StaticResource Resolution Issues
- **Status**: Completed
- **Description**: Replaced all instances of `StaticResource PrimaryRed` with its direct hex value `#D90429` across affected XAML files to resolve resource resolution issues.
- **Steps**:
    - [x] Changed `BackgroundColor` in `LibraryPage.xaml` from `StaticResource PrimaryRed` to `#D90429`.
    - [x] Changed `TextColor` and `BackgroundColor` in `SignUpPage.xaml` from `StaticResource PrimaryRed` to `#D90429`.
    - [x] Changed `TextColor` and `BackgroundColor` in `LoginPage.xaml` from `StaticResource PrimaryRed` to `#D90429`.
    - [x] Changed `Color` and `BackgroundColor` in `EventsPage.xaml` from `StaticResource PrimaryRed` to `#D90429`.
    - [x] Changed `Dark` theme `Shell.TabBarForegroundColor` and `Shell.TabBarTitleColor` in `Resources/Styles/Styles.xaml` from `StaticResource PrimaryRed` to `#D90429`.
    - [x] Changed `BackgroundColor` in `Controls/MiniPlayerControl.xaml` from `StaticResource PrimaryRed` to `#D90429`.
    - [x] Changed `Shell.TabBarTitleColor` and `Shell.TabBarForegroundColor` in `AppShell.xaml` from `StaticResource PrimaryRed` to `#D90429`.
    - [x] Changed `BackgroundColor` in `App.xaml` from `StaticResource PrimaryRed` to `#D90429`.

## Task: Performance & Scalability
- **Status**: Not started
- **Description**: Optimize system for production scale with caching, CDN, and monitoring.
- **Steps**:
    - [ ] Implement Redis caching layer for frequently accessed data
    - [ ] Set up CDN for audio streaming (Azure Blob Storage + CDN)
    - [ ] Add database read replicas for load distribution
    - [ ] Implement monitoring and alerting (Application Insights)
    - [ ] Add rate limiting to prevent abuse
    - [ ] Optimize audio transcoding for multiple bitrates
    - [ ] Set up automated backup and recovery
    - [ ] Implement load testing and performance tuning
- **Dependencies**: Production deployment, Monitoring infrastructure

---

## Known Issues / Tech Debt

- [ ] `HttpClient` shared between `AuthenticationService` and `GoogleAuthService` — potential race condition on DefaultRequestHeaders
- [ ] No loading states / spinners in most pages
- [ ] No offline support or caching
- [ ] Stream URLs hardcoded with port numbers — should be configurable
- [ ] No unit or integration tests
- [ ] `DateOnly` serialization may cause issues with some JSON serializers

## Documentation Files

- `CLAUDE.md` — Quick reference for Claude Code
- `AGENTS.md` — Coding standards and development guidelines
- `Docs/music-architecture.md` — Full system architecture
- `Docs/validation.md` — Validation framework details
- `Docs/changelog.md` — Change history
- `Docs/project.md` — Project architecture overview
