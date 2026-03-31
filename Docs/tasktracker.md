# Task Tracker - Innowise.Music

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
    - [x] Added `CommunityToolkit.Maui` NuGet package.
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

## Task: Music Streaming Service Implementation (Phase 1 - MVP)
- **Status**: In progress
- **Description**: Implement core music streaming functionality with 5 essential API endpoints to enable music search, discovery, and playback. Audio files will be stored in PostgreSQL BYTEA fields.
- **Steps**:
    - [x] **Database Setup**
        - [x] Create database models (Artists, Albums, Tracks, Genres) with BYTEA audio storage
        - [x] Update MusicIdentityDbContext with DbSets and full-text search configuration
        - [x] Create database migration (AddMusicTables) with pg_trgm extension
        - [x] Apply migration to PostgreSQL database
    - [x] **Backend API Development**
        - [x] Create MusicController with 5 essential endpoints
        - [x] Implement `GET /music/tracks?query={q}` - Search tracks with pagination
        - [x] Implement `GET /music/tracks/{id}` - Get track details
        - [x] Implement `GET /music/tracks/{id}/stream` - Stream audio with range request support
        - [x] Implement `GET /music/artists/{id}/top-tracks` - Get artist's popular tracks
        - [x] Implement `GET /music/albums/{id}/tracks` - Get album tracks
        - [x] Add audio streaming with proper Content-Type and range request headers
    - [x] **Service Layer**
        - [x] Create IMusicService interface
        - [x] Implement MusicService with database queries and Include for related data
        - [x] Implement search using ILike for case-insensitive matching
        - [x] Register service in Program.cs Dependency Injection
    - [ ] **MAUI Client Integration**
        - [ ] Update Track model to include all metadata fields
        - [ ] Create MusicApiClient service for API communication
        - [ ] Update SearchPage to use real API instead of mock data
        - [ ] Update HomePage to display real featured content
        - [ ] Implement track selection and playback from search results
        - [ ] Handle audio streaming with proper error handling
    - [ ] **Testing & Optimization**
        - [ ] Test audio streaming with various file sizes
        - [ ] Optimize database queries for search performance
        - [ ] Test range requests for seeking in audio player
        - [ ] Verify pagination works correctly
- **Dependencies**: PostgreSQL database, Existing authentication system, Audio streaming infrastructure

## Task: Music Streaming Service (Phase 2+ - Future)
- **Status**: Not started
- **Description**: Advanced features to enhance user experience including user libraries, playlists, recommendations, and social features.
- **Steps**:
    - [ ] **User Library Features**
        - [ ] Implement user playlists CRUD operations
        - [ ] Add like/unlike tracks functionality
        - [ ] Create "Liked Songs" automatic playlist
        - [ ] Implement follow/unfollow artists
    - [ ] **Advanced Search**
        - [ ] Add search autocomplete/suggestions
        - [ ] Implement universal search across all content types
        - [ ] Add search filters (genre, year, explicit content)
    - [ ] **Recommendations**
        - [ ] Create personalized home page based on listening history
        - [ ] Implement featured playlists algorithm
        - [ ] Add "Because you listened to..." recommendations
        - [ ] Create new releases feed
    - [ ] **Social Features**
        - [ ] Record and analyze listening history
        - [ ] Create "Recently Played" functionality
        - [ ] Add play count tracking
        - [ ] Implement shared playlists
- **Dependencies**: Phase 1 completion, User authentication system, Listening history data

## Task: Admin Dashboard Implementation
- **Status**: In progress
- **Description**: Blazor Web admin dashboard for managing music content (artists, albums, tracks, genres). See detailed plan in `Docs/admin-dashboard-plan.md`.
- **Steps**:
    - [x] **Phase 1: Backend API Enhancements**
        - [x] Extend IMusicService with CRUD operations for all entities
        - [x] Implement CRUD methods in MusicService
        - [x] Create AdminMusicController with admin endpoints
        - [x] Add role-based authorization to admin endpoints
        - [x] Fix AdminMusicService endpoint paths to use `api/admin/` prefix
    - [ ] **Phase 2: Blazor Admin Project Creation**
        - [ ] Create Innowise.Music.Admin project
        - [ ] Set up project structure (Components, Services, Models)
        - [ ] Configure dependency injection
        - [ ] Add project to solution file
    - [ ] **Phase 3: Authentication & Authorization**
        - [ ] Implement AuthService for JWT handling
        - [ ] Create login page with role validation
        - [ ] Implement token refresh logic
    - [ ] **Phase 4: CRUD Operations**
        - [ ] Genre management (list, create, edit, delete)
        - [ ] Artist management with image support
        - [ ] Album management with relationships
        - [ ] Track management with metadata
    - [ ] **Phase 5: File Upload**
        - [ ] Create FileUpload component
        - [ ] Implement streaming upload for audio files
        - [ ] Add progress tracking
        - [ ] Validate file types and sizes
    - [ ] **Phase 6: UI/UX Polish**
        - [ ] Create main layout with navigation
        - [ ] Add shared components (dialogs, spinners)
        - [ ] Implement responsive styling
    - [ ] **Phase 7: Docker Integration**
        - [ ] Create Dockerfile for admin app
        - [ ] Update docker-compose.yml
    - [ ] **Phase 8: Testing & Documentation**
        - [ ] Write unit tests for services
        - [ ] Test file upload functionality
        - [ ] Update project documentation
- **Dependencies**: Innowise.MusicIdentityServer (existing), PostgreSQL database

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
