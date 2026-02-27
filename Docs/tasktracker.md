# Task Tracker - Innowise.Music

## Task: Project Setup and Core Infrastructure
- **Status**: Completed
- **Description**: Initial MAUI project setup with .NET 9, MVVM pattern, and dependency injection
- **Steps**:
  - [x] Create MAUI project structure
  - [x] Configure DI in MauiProgram.cs
  - [x] Set up AppShell with routing
  - [x] Define shared resources in App.xaml

## Task: Authentication Flow (Login/SignUp)
- **Status**: In progress
- **Description**: Implement login and sign-up pages with MVVM pattern and JWT authentication.
- **Steps**:
  - [x] Create LoginPage.xaml with ViewModel
  - [x] Create SignUpPage.xaml with ViewModel
  - [x] Implement navigation between Login and SignUp
  - [x] Apply shared styles and gradients
  - [x] Match UI design with mockups (Borders, Logos, Branded Buttons)
  - [ ] Create Auth models (DTOs) in the MAUI project
  - [ ] Implement IAuthService and AuthService using SecureStorage
  - [ ] Update HttpHelper to include Bearer token
  - [ ] Register services in MauiProgram.cs
  - [ ] Update LoginPageViewModel and SignUpPageViewModel
  - [ ] Implement startup navigation logic based on auth state
  - [x] Remove Blazor-specific AuthenticationStateProvider
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
