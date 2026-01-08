# Task Management API

A simple REST API for managing tasks and users built with .NET 8, Entity Framework Core, and SQLite.

## Prerequisites

- .NET 8 SDK or later
- Visual Studio 2022 / VS Code / Any text editor
- Postman or cURL (for testing)

## Getting Started

### 1. Clone or Download the Project

```bash
cd TaskManagementApi
```

### 2. Install Required Packages

```bash
dotnet restore
```

If packages are missing, install them manually:

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package FluentValidation
dotnet add package FluentValidation.AspNetCore
dotnet add package Swashbuckle.AspNetCore
```

### 3. Install Entity Framework Tools (if not already installed)

```bash
dotnet tool install --global dotnet-ef
```

### 4. Create Database

```bash
# Create migration
dotnet ef migrations add InitialCreate

# Apply migration (creates database)
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run
```

The API will start at: `https://localhost:7xxx` (check console for exact port)

### 6. Access Swagger UI

Open your browser and go to: `https://localhost:7xxx/swagger`

## Project Structure

```
TaskManagementApi/
├── Controllers/        # API endpoints
├── Models/            # Database entities and enums
├── DTOs/              # Data transfer objects
├── Data/              # Database context
├── Validators/        # Input validation
├── Middleware/        # API key authentication
├── Program.cs         # Application configuration
├── appsettings.json   # Configuration settings
└── taskmanagement.db  # SQLite database (created after first run)
```

### Database Issues
```bash
# Delete database and recreate
dotnet ef database drop
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Build Errors
```bash
dotnet clean
dotnet restore
dotnet build
```

## Technologies Used

- .NET 8 / .NET 10 
- ASP.NET Core Web API
- Entity Framework Core
- SQLite Database
- Swagger/OpenAPI
