# Usage Tracker

A Windows desktop application that tracks how long each window is in the foreground, grouped by application and window title hierarchy.

## Features

- Tracks active window time in real-time
- Groups window titles into a collapsible hierarchy (e.g. Google Chrome → Twitch → xQc)
- Sorts processes by total elapsed time, updating every second
- Pauses automatically when the desktop is shown, all windows are closed/minimized, or the screen is locked
- Skips tracking when the Usage Tracker window itself is focused
- Session timer displayed in the header

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Windows 10 or later (the app uses Windows-only APIs for window detection)
- PostgreSQL (optional — login is currently short-circuited; any credentials are accepted)

## Installation

```bash
git clone <repository-url>
cd UsageTracker
dotnet restore
```

## Running the App

```bash
dotnet run --project UsageTracker.csproj
```

Or build a release binary:

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

The output will be in `bin/Release/net9.0-windows/win-x64/publish/`.

## Running the Tests

```bash
dotnet test UsageTracker.Tests/UsageTracker.Tests.csproj
```

## Project Structure

```
UsageTracker/
├── Entities/               # Plain C# models (TrackedProcess, User)
├── Services/               # Core logic (WindowMonitor, ProcessTrackingService, AuthService)
├── Data/                   # Database access (UserRepository)
├── ViewModels/             # ReactiveUI ViewModels (Login, Tracking, ProcessGroup)
├── Views/                  # Avalonia AXAML views
├── Interfaces/             # Shared interfaces (IUser, IStorage)
└── Resources/              # App icon

UsageTracker.Tests/
├── TrackedProcessTests.cs
├── ProcessTrackingServiceTests.cs
├── AuthServiceTests.cs
└── LoginViewModelTests.cs
```

## Tech Stack

| Concern | Library |
|---|---|
| UI framework | Avalonia 11.2.3 |
| MVVM / reactive | ReactiveUI + System.Reactive |
| Dependency injection | Microsoft.Extensions.DependencyInjection |
| Database | PostgreSQL via Npgsql 8 |
| Password hashing | BCrypt.Net-Next 4 |
| Unit testing | xUnit + NSubstitute + FluentAssertions |

## Notes

- **Login is currently stubbed** — any non-empty email and password will work. Real PostgreSQL authentication is wired up in `IAuthService` and ready to be implemented in `AuthService.cs`.
- The app targets `net9.0-windows` and will not run on macOS or Linux due to its use of Win32 APIs (`GetForegroundWindow`, `IsIconic`, etc.).
