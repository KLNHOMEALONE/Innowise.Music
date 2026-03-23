# Task Tracker - Innowise.Music

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
- **Status**: Completed
- **Description**: News listing and detailed view with API integration
- **Steps**:
    - [x] Create News model
    - [x] Implement INewsService interface
    - [x] Create WebNewsService for API calls
    - [x] Create MockNewsService for testing
    - [x] Implement HttpHelper for SSL handling
    - [x] Create NewsPage with CollectionView
    - [x] Create NewsDetailedPage
    - [x] Implement navigation with QueryProperty
    - [ ] Add error handling for API failures
    - [ ] Add loading states

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

## Task: Audio Streaming Core
- **Status**: Not started
- **Description**: Implement audio playback functionality
- **Steps**:
    - [ ] Set up MediaElement or audio plugin
    - [ ] Create audio player service
    - [ ] Implement play/pause controls
    - [ ] Add track progress tracking
    - [ ] Create background playback service

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
- **Status**: Not started
- **Description**: Allow users to save favorites and create playlists
- **Steps**:
    - [ ] Create favorite tracks feature
    - [ ] Implement playlist CRUD operations
    - [ ] Build playlist UI

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
    - [ ] Update project.md with architecture
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
    - [x] Setup `EventsPage.xaml` placeholder.
    - [x] Populate ViewModels with mock data (Initial mock data for all main pages).
    - [x] Register new pages and ViewModels in `MauiProgram.cs`.
    - [x] Ensure dark mode colors (`App.xaml`) and accents match designs.

## Task: Address PrimaryRed StaticResource Resolution Issues
- **Status**: Completed
- **Description**: Replaced all instances of `StaticResource PrimaryRed` with its direct hex value `#D90429` across affected XAML files to resolve resource resolution issues.
- **Steps**:
    - [x] Changed `BackgroundColor` in `LibraryPage.xaml` from `StaticResource PrimaryRed` to `#D90429`.
    - [x] Changed `TextColor` and `BackgroundColor` in `SignUpPage.xaml` from `Static-resource PrimaryRed` to `#D90429`.
    - [x] Changed `TextColor` and `BackgroundColor` in `LoginPage.xaml` from `StaticResource PrimaryRed` to `#D90429`.
    - [x] Changed `Color` and `BackgroundColor` in `EventsPage.xaml` from `StaticResource PrimaryRed` to `#D90429`.
    - [x] Changed `Dark` theme `Shell.TabBarForegroundColor` and `Shell.TabBarTitleColor` in `Resources/Styles/Styles.xaml` from `StaticResource PrimaryRed` to `#D90429`.
    - [x] Changed `BackgroundColor` in `Controls/MiniPlayerControl.xaml` from `StaticResource PrimaryRed` to `#D90429`.
    - [x] Changed `Shell.TabBarTitleColor` and `Shell.TabBarForegroundColor` in `AppShell.xaml` from `StaticResource PrimaryRed` to `#D90429`.
    - [x] Changed `BackgroundColor` in `App.xaml` from `StaticResource PrimaryRed` to `#D90429`.