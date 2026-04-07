# Code Review: Innowise.Music MAUI Client

**Date:** 2026-04-07
**Project:** Innowise.Music (MAUI Client)
**Framework:** .NET 9 MAUI
**Platforms:** Android, iOS, macOS (MacCatalyst), Windows

---

## 1. Architecture Overview

### Project Structure

The project follows a clean **MVVM** architecture with clear separation of concerns:

```
Innowise.Music/
├── Configuration/     — Strongly-typed settings (ApiSettings, GoogleAuthenticationSettings)
├── Controls/          — Reusable XAML controls (InputEntryControl, MiniPlayerControl)
├── Converters/        — Value converters (BoolToColor, BoolToFavoriteIcon, FirstValidationError)
├── Model/             — DTOs (LoginUserDto, UserDto, Track, News, etc.)
├── Services/          — Business logic (Auth, Audio, Navigation, News)
├── Validations/       — Generic validation framework
├── View/              — XAML pages (9 pages)
├── ViewModel/         — Presentation logic (11 ViewModels)
└── Resources/         — Fonts, images, styles
```

### Design Patterns

| Pattern | Usage |
|---------|-------|
| **MVVM** | All pages use View + ViewModel separation with data binding |
| **Dependency Injection** | All services, ViewModels, and Views registered as singletons in `MauiProgram.cs` |
| **Service Layer** | Business logic isolated in services behind interfaces |
| **Observable Pattern** | `ObservableObject` (CommunityToolkit.Mvvm) for INotifyPropertyChanged |
| **Async/Await** | All I/O operations are properly async |

---

## 2. What's Done Well

### 2.1 Clean MVVM Implementation
- Consistent use of `[ObservableProperty]` and `[RelayCommand]` attributes from CommunityToolkit.Mvvm
- Constructor injection throughout — no service locator anti-pattern
- Views and ViewModels are properly decoupled

### 2.2 Validation Framework
The generic `ValidatableObject<T>` with composable `IValidationRule<T>` rules is well-designed:
- Reusable across any type
- Real-time validation on `TextChanged` via `EventToCommandBehavior`
- Clear error display through `FirstValidationErrorConverter`
- Three built-in rules: `IsNotNullOrEmptyRule`, `EmailRule`, `CompareRule`

### 2.3 Authentication Architecture
- JWT tokens stored in `SecureStorage` (platform-secure storage)
- Automatic token refresh with 1-minute buffer before expiry
- Fallback to logout when refresh fails
- Platform-aware API URL configuration (Android uses `10.0.2.2` for localhost)

### 2.4 Google OAuth2 Integration
- Full PKCE flow with SHA-256 code challenge
- Platform-specific flows: Windows (local HTTP listener) and Android (WebAuthenticator)
- Token refresh and revocation support
- Semaphore-based concurrency protection to prevent race conditions during token acquisition

### 2.5 Configuration Management
- `appsettings.json` embedded as a resource
- Strongly-typed settings classes with `IOptions<T>` pattern
- Platform-specific configuration (different API URLs, different Google client IDs)

### 2.6 Nullable Reference Types
- Enabled project-wide (`<Nullable>enable</Nullable>`)
- Helps catch null-reference bugs at compile time

---

## 3. Issues and Concerns

### 3.1 Security: Hardcoded Signing Credentials (HIGH)

**File:** `Innowise.Music.csproj:46-58`

```xml
<AndroidSigningStorePass>123456</AndroidSigningStorePass>
<AndroidSigningKeyPass>123456</AndroidSigningKeyPass>
```

The Android keystore password is hardcoded as `123456` in the project file. This is a very weak password and should be:
- Stored in a secure secrets manager or user prompt at build time
- At minimum, moved to environment variables

### 3.2 Security: Insecure HTTP Handler (MEDIUM)

**File:** `HttpHelper.cs`

The `GetInsecureHandler()` method disables SSL certificate validation globally:

```csharp
// Accepts self-signed certificates for development
```

This is acceptable for development but must be guarded by `#if DEBUG` or replaced with certificate pinning for production to prevent man-in-the-middle attacks.

### 3.3 Security: Google Client IDs in Source (MEDIUM)

**File:** `appsettings.json`

Google OAuth client IDs are committed to source control. While client IDs are technically public (they're embedded in the app binary), best practice is to use a configuration system that separates secrets from code, especially if this repository is public.

### 3.4 Silent Error Handling (MEDIUM)

**File:** `AuthenticationService.cs`, `GoogleAuthService.cs`

Exceptions are caught and only logged via `Debug.WriteLine()`:

```csharp
catch (Exception ex)
{
    System.Diagnostics.Debug.WriteLine($"AuthenticationService Login Error: {ex.Message}");
}
return false;
```

This means:
- Users get no feedback when authentication fails
- Errors silently disappear in release builds (Debug output not visible)
- No structured logging or error telemetry

**Recommendation:** Return meaningful error messages to the UI, or use a user-facing notification system.

### 3.5 Typo in Storage Key (LOW)

**File:** `GoogleAuthService.cs:158`

```csharp
Preferences.Set("access_token_epires_in", accessTokenExpiresIn);
```

The key is misspelled as `epires_in` instead of `expires_in`. This is consistent across the codebase but should be fixed (with a migration strategy for existing installs).

### 3.6 Null Reference Risk in RevokeTokens (MEDIUM)

**File:** `GoogleAuthService.cs:56-63`

```csharp
var access_token = await SecureStorage.GetAsync("access_token");
// ...
new KeyValuePair<string, string>("token", access_token!)
```

The null-forgiving operator `!` is used on `access_token` which could be null. If `SecureStorage` returns null, this will throw at runtime. Should validate before use.

### 3.7 Local HTTP Server on Fixed Port (LOW)

**File:** `GoogleAuthService.cs:165`

```csharp
var localPort = 12345;
```

The Windows OAuth flow uses a hardcoded port. If another process is using port 12345, the auth flow will fail. Should use a random available port or handle port conflicts gracefully.

### 3.8 Mock Data in ViewModels (LOW)

**Files:** `HomePageViewModel.cs`, `SearchPageViewModel.cs`, `LibraryPageViewModel.cs`, `EventsPageViewModel.cs`

These ViewModels contain hardcoded mock data rather than fetching from the API. This is fine for development but should be replaced with real service calls before production.

### 3.9 No Unit Tests

There are no test projects in the solution. The interface-based architecture makes the code testable, but no tests exist to verify behavior. Key areas that need tests:
- `AuthenticationService` (login, register, token refresh)
- `ValidatableObject<T>` validation logic
- `NavigationService` routing

### 3.10 Singleton Lifetime for ViewModels (MEDIUM)

All ViewModels are registered as singletons. This means:
- State persists across navigation (e.g., login form retains entered email after back navigation)
- Multiple users on shared devices could see previous user's data
- Memory usage grows with cached ViewModel state

Consider using scoped or transient lifetimes for ViewModels that hold user-specific state.

### 3.11 Missing iOS Auth Flow (MEDIUM)

**File:** `GoogleAuthService.cs:109-111`

```csharp
else
{
    throw new NotImplementedException($"Auth flow for platform {DeviceInfo.Current.Platform} not implemented.");
}
```

iOS and MacCatalyst platforms are targeted in the `.csproj` but Google auth is not implemented for them. This will crash at runtime if a user attempts Google login on these platforms.

---

## 4. Code Quality Summary

| Category | Rating | Notes |
|----------|--------|-------|
| Architecture | Good | Clean MVVM, proper DI, service layer separation |
| Code Style | Good | Consistent naming, nullable reference types enabled |
| Error Handling | Poor | Silent failures, no user feedback, no structured logging |
| Security | Fair | Weak keystore password, disabled SSL validation, hardcoded client IDs |
| Testability | Good | Interface-based design, but no tests written |
| Platform Support | Fair | iOS/Mac auth flows missing, Android/Windows complete |
| Documentation | Fair | File headers present, but inline comments are sparse |

---

## 5. Recommendations (Priority Order)

1. **Fix the Android signing password** — move to environment variables or secure storage
2. **Add user-facing error notifications** — replace silent `Debug.WriteLine` with actionable feedback
3. **Guard the insecure HTTP handler** — wrap in `#if DEBUG` or implement certificate pinning
4. **Fix the null reference in `RevokeTokens`** — validate `access_token` before use
5. **Implement iOS/Mac Google auth flow** — or gracefully disable the Google login button on those platforms
6. **Use a dynamic port for Windows OAuth** — or handle port conflicts with retry
7. **Fix the `epires_in` typo** — with a migration for existing users
8. **Add unit tests** — start with `AuthenticationService` and the validation framework
9. **Consider ViewModel lifetime** — evaluate whether singleton is appropriate for all ViewModels

---

## 6. Verdict

The Innowise.Music MAUI client is **well-architected** with a solid MVVM foundation, proper dependency injection, and clean separation of concerns. The validation framework is particularly well-designed and reusable.

The primary concerns are in **security** (hardcoded credentials, disabled SSL validation) and **error handling** (silent failures that leave users confused). These are fixable and don't require architectural changes.

The codebase is in good shape for a development/preview stage but needs the security and error handling improvements listed above before production release.
