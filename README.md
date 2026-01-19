# Garage 3.0

An ASP.NET Core web application for managing a shared garage.

## Tech stack

- .NET 9
- C# 13
- ASP.NET Core (MVC)
- Bootstrap + jQuery

## Features

- **Vehicles**: Register and view vehicles.
- **Members overview**: View vehicles per member.
- **Parking**: Manage parking spots and view parking history.
- **Garage statistics**: Overview page for statistics.
- **Authentication/Authorization**: UI adapts based on role (for example, `Admin`).
- **Theme toggle**: Light/dark/auto theme selector.

## Roles

- **Admin**: Access to administrative pages (Vehicles dropdown, Parking management/history, Admin role management, Garage statistics).
- **Non-admin**: Access to registered vehicles, members overview, and parking history.

## Prerequisites

- .NET SDK 9 installed.
- A configured database connection string in `appsettings.json` (or user-secrets) if the project uses a database.

## Getting started (Visual Studio 2022)

1. Open the solution in Visual Studio.
2. Restore NuGet packages (this happens automatically on build).
3. Update configuration in `appsettings.json` as needed.
4. Run the project:
   - Press **F5** to run with debugging, or
   - Use **Debug > Start Without Debugging**.

## Getting started (CLI)

From the repo root, execute the following commands:

dotnet restore
dotnet run

Then open the URL shown in the console.


