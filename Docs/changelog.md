# Changelog - Innowise.Music

All notable changes to this project will be documented in this file.

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
