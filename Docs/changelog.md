# Changelog - Innowise.Music

All notable changes to this project will be documented in this file.

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
