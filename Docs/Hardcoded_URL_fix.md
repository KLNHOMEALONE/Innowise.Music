# Plan: Fix Hardcoded URLs and Centralize Configuration

## Overview
Currently, several services and ViewModels in the MAUI application have hardcoded fallback URLs for audio streaming (e.g., `http://localhost:5236`). This makes the application fragile and difficult to deploy across different environments. This plan outlines the steps to move these configurations to `appsettings.json` and update the codebase to use centralized settings.

## Step 1: Update Configuration Model
Modify `Innowise.Music/Configuration/ApiSettings.cs` to include properties for streaming base URLs:
- `StreamBaseUrl`
- `AndroidStreamBaseUrl`

## Step 2: Update Application Settings
Update `Innowise.Music/appsettings.json` to include the new settings:
```json
"ApiSettings": {
  "BaseUrl": "https://localhost:7008",
  "AndroidBaseUrl": "https://10.0.2.2:7008",
  "StreamBaseUrl": "http://localhost:5236",
  "AndroidStreamBaseUrl": "http://10.0.2.2:5236",
  "SearchPageSize": 8
}
```
*Note: We use HTTP (5236) for streaming because MediaElement often struggles with self-signed HTTPS certificates on local dev machines.*

## Step 3: Update ViewModels
Update `Innowise.Music/ViewModel/SearchPageViewModel.cs`:
- Store the full `ApiSettings` object instead of just `_pageSize`.
- Update `PlayTrack` command to use `_apiSettings.StreamBaseUrl` or `_apiSettings.AndroidStreamBaseUrl` based on the platform.

## Step 4: Update Services
Update the following services to use the new settings from `_apiSettings`:
- `Innowise.Music/Services/HistoryService.cs`
- `Innowise.Music/Services/FavoriteService.cs`
- `Innowise.Music/Services/RecommendationService.cs`

## Step 5: Verification
- Run the MAUI application.
- Perform a search and play a track.
- Verify "Recently Played" and "Favorites" sections still function correctly and play music.
- Ensure no hardcoded `5236` or `10.0.2.2` strings remain in the service/ViewModel logic.
