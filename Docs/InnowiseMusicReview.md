# Code Review: Innowise.Music MAUI Client

**Date:** 2026-04-22
**Project:** Innowise.Music (MAUI Client)
**Reviewer:** Rocket (Subject 89P13)

---

## 1. Executive Summary
The `Innowise.Music` project has seen significant architectural improvements since the last review. Dependency injection is now consistently applied, and the most glaring typos and hardcoded infrastructure choices (like fixed ports) have been addressed. However, critical security vulnerabilities regarding signing credentials remain, and error handling across the non-authentication ViewModels still relies on silent failures, providing a poor user experience when services are unreachable.

---

## 2. Security Vulnerabilities (High Priority)

### 2.1 Hardcoded Signing Credentials
**File:** `Innowise.Music/Innowise.Music.csproj`
The Android keystore and key passwords remain hardcoded in plaintext:
```xml
<AndroidSigningStorePass>123456</AndroidSigningStorePass>
<AndroidSigningKeyPass>123456</AndroidSigningKeyPass>
```
**Risk:** High. This is a massive red flag. If this repo leaks, the signing identity is gone.
**Recommendation:** Move these to environment variables or use a `Directory.Build.props` file that is ignored by git.

### 2.2 Hardcoded Client IDs
**File:** `Innowise.Music/appsettings.json`
Google OAuth Client IDs for all platforms are still stored in the JSON configuration committed to source control.
**Recommendation:** While Client IDs are public in the binary, keeping them in a centralized, committed config makes rotation difficult and exposes them to scanners.

---

## 3. Error Handling & User Experience

### 3.1 Silent Failures in Services and ViewModels
**Files:** `RecommendationService.cs`, `SearchService.cs`, `SearchPageViewModel.cs`
While `LoginPageViewModel` now utilizes `IDialogService` for user feedback, other critical paths still fail silently.
**Example (SearchPageViewModel):**
```csharp
var response = await _searchService.UnifiedSearchAsync(SearchQuery, CurrentPage, _apiSettings.SearchPageSize);
if (response != null) { ... }
else {
    SearchResults.Clear();
    HasResults = false; // User sees a blank screen instead of "Service Unavailable"
}
```
**Risk:** High UX impact. Users cannot distinguish between "No results found" and "Server is offline."
**Recommendation:** Implement a unified error handling strategy in ViewModels that informs the user when a network or server error occurs.

---

## 4. Google Authentication Implementation

### 4.1 Platform Gaps (Unresolved)
**File:** `Innowise.Music/Services/GoogleAuthService.cs:111`
The authentication flow for **iOS** and **MacCatalyst** still throws a `NotImplementedException`. 
**Status:** This remains the biggest functional gap in the cross-platform story.

### 4.2 Typo Fixed
**File:** `Innowise.Music/Services/GoogleAuthService.cs`
The previous typo `access_token_epires_in` has been **FIXED** to `access_token_expires_in`. 

### 4.3 Dynamic Port Selection
**File:** `Innowise.Music/Services/GoogleAuthService.cs`
The Windows OAuth flow has been improved to use a dynamic unused port instead of the hardcoded port `12345`. **FIXED.**

---

## 5. Architectural Improvements

### 5.1 HttpClient Lifecycle & DTOs
**File:** `Innowise.Music/Services/RecommendationService.cs`
The service now correctly uses constructor injection for `HttpClient` and utilizes shared models from the `Model/` namespace. **FIXED.**

### 5.2 ViewModel Lifetimes
**File:** `Innowise.Music/MauiProgram.cs`
Authentication-related ViewModels (`LoginPageViewModel`, `SignUpPageViewModel`) are now registered as `Transient`, ensuring state is reset upon re-entry. **FIXED.**

### 5.3 Insecure HTTP Handler Protection
**File:** `Innowise.Music/Services/HttpClientHelper.cs`
SSL validation bypass logic for development is now properly wrapped in `#if DEBUG` blocks. **FIXED.**

---

## 6. Final Recommendations (Updated)
1. **IMMEDIATE:** Secure the Android signing passwords. Don't make me say it again!
2. **UX:** Extend the error dialog pattern from `LoginPageViewModel` to the `Search` and `Home` modules.
3. **Cross-Platform:** Prioritize implementing the `WebAuthenticator` flow for iOS and macOS.
4. **Resilience:** Add a retry policy (e.g., using Polly) to the shared `HttpClient` registration in `MauiProgram.cs`.
