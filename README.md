# FinalLabSystem — نظام إدارة المختبرات الطبية

## Prerequisites
- .NET 8 SDK
- SQL Server Express (instance name: SQLEXPRESS)
- Visual Studio 2022 or Rider

## Setup
1. Clone the repository.
2. Copy `FinalLabSystem/appsettings.Development.json` (template in appsettings.json) and set your connection string.
3. Run migrations:
   dotnet ef database update --project FinalLabSystem/FinalLabSystem.csproj
4. Run the application from Visual Studio or:
   dotnet run --project FinalLabSystem/FinalLabSystem.csproj

## Project Structure
- FinalLabSystem/ — Main WPF application (MVVM, .NET 8)
- FinalLabSystem.Tests/ — xUnit test project
- FinalLabSystem/Docs/ — Arabic functional documentation (~450 KB)

## Architecture
MVVM pattern. ViewModelBase in Infrastructure/ViewModelBase.cs.
Navigation via INavigationService (Infrastructure/Navigation/).
Security: PBKDF2-SHA256 password hashing (do not modify).

## Running Tests
dotnet test FinalLabSystem.Tests/FinalLabSystem.Tests.csproj
