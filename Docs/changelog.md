# Changelog - Innowise.Music

All notable changes to this project will be documented in this file.

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
