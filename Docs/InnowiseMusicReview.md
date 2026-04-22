# Code Review: Innowise.Music MAUI Client

**Date:** 2026-04-22
**Project:** Innowise.Music (MAUI Client)
**Reviewer:** Rocket (Subject 89P13)

---

## 1. Executive Summary
The `Innowise.Music` project is a well-structured .NET MAUI application utilizing a clean MVVM architecture. It demonstrates strong use of dependency injection, a robust validation framework, and a clever global media playback strategy. However, critical security vulnerabilities regarding credential management and insecure communication must be addressed before production.

---

## 2. Security Vulnerabilities (High Priority)

### 2.1 Hardcoded Signing Credentials
**File:** `Innowise.Music/Innowise.Music.csproj`
The Android keystore and key passwords are hardcoded in plaintext:
```xml
<AndroidSigningStorePass>123456</AndroidSigningStorePass>
<AndroidSigningKeyPass>123456</AndroidSigningKeyPass>
```
**Risk:** High. If the repository is compromised, the app's signing identity is leaked.
**Recommendation:** Move these to environment variables or a secure build-time secret manager.

### 2.2 Insecure HTTP Handler
**File:** `Innowise.Music/Services/HttpClientHelper.cs`
The `GetInsecureHandler()` method bypasses SSL validation for `localhost` and `10.0.2.2`. 
**Risk:** Medium. While useful for local development, this logic is currently active in all build configurations.
**Recommendation:** Wrap the insecure callback logic in `#if DEBUG` to ensure production builds strictly enforce SSL.

### 2.3 Hardcoded Client IDs
**File:** `Innowise.Music/appsettings.json`
Google OAuth Client IDs are stored in a configuration file committed to source control.
**Recommendation:** Use a more secure configuration strategy for production secrets, such as Azure Key Vault or platform-specific secure storage.

---

## 3. Error Handling & User Experience

### 3.1 Silent Failures in Authentication
**Files:** `AuthenticationService.cs`, `LoginPageViewModel.cs`, `GoogleAuthService.cs`
Exceptions and failed responses are caught but only logged to `System.Diagnostics.Debug.WriteLine`. 
**Example:**
```csharp
else {
    // Handle error
    System.Diagnostics.Debug.WriteLine("Login failed");
}
```
**Risk:** High UX impact. Users receive no feedback when a login fails or the network is down.
**Recommendation:** Implement a user-facing notification service (e.g., `IDialogService`) to display error messages.

---

## 4. Google Authentication Implementation

### 4.1 Persistent Typo in Storage Key
**File:** `Innowise.Music/Services/GoogleAuthService.cs`
The key used for storing token expiry is misspelled as `access_token_epires_in` in several locations:
- `RevokeTokens()`
- `AcquireTokenAsync()`
- `RefreshToken()`
- `GetInitialToken()`
**Recommendation:** Standardize on `access_token_expires_in` and implement a migration for existing local data.

### 4.2 Fixed OAuth Port (Windows)
**File:** `Innowise.Music/Services/GoogleAuthService.cs:165`
```csharp
var localPort = 12345;
```
**Risk:** Low/Medium. If port 12345 is occupied by another process, the Windows OAuth flow will fail.
**Recommendation:** Implement a strategy to find an available dynamic port.

### 4.3 Platform Gaps
**File:** `Innowise.Music/Services/GoogleAuthService.cs:111`
Auth flows for iOS and MacCatalyst are not yet implemented, resulting in a `NotImplementedException` if triggered.

---

## 5. Architectural Observations & Code Quality

### 5.1 HttpClient Lifecycle Management
**File:** `Innowise.Music/Services/RecommendationService.cs`
The service creates a new `HttpClient` instance for every request instead of using the shared client registered in `MauiProgram.cs`.
**Risk:** Socket exhaustion under high load.
**Recommendation:** Inject the shared `HttpClient` via constructor injection, consistent with `SearchService`.

### 5.2 DTO Redundancy
**File:** `Innowise.Music/Services/RecommendationService.cs`
The service defines private DTO classes (`TrackDto`, `ArtistDto`, etc.) that mirror models already existing in the `Innowise.Music.Model` namespace.
**Recommendation:** Consolidate data models into the shared `Model/` folder to improve maintainability.

### 5.3 ViewModel Lifetimes
**File:** `Innowise.Music/MauiProgram.cs`
Most ViewModels are registered as `Singletons`. 
**Observation:** This persists state (like half-entered forms) across navigation cycles. 
**Recommendation:** Evaluate using `Transient` lifetimes for ViewModels that should reset when the user re-enters a page (e.g., `LoginPageViewModel`).

---

## 6. Positive Highlights
- **Validation Framework:** The generic `ValidatableObject<T>` and `IValidationRule<T>` system is elegant, reusable, and cleanly implemented.
- **Global Media State:** Hosting `MediaElement` in `AppShell` and initializing `AudioService` with it is a clever way to maintain persistent audio across page transitions.
- **Control Reuse:** `InputEntryControl` effectively encapsulates complex validation UI logic.

---

## 7. Final Recommendations
1. **Secure the signing process** by removing passwords from `.csproj`.
2. **Improve error transparency** by adding user-facing alerts for failed API operations.
3. **Refactor `RecommendationService`** to use the centralized `HttpClient` and shared models.
4. **Fix the `epires_in` typo** across the `GoogleAuthService`.
5. **Implement dynamic port selection** for Windows OAuth.
