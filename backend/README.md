# JobTracker Backend

.NET 8 Web API for the Job Application Tracker system.

## Architecture

Clean Architecture with:
- **Domain**: Core business entities and interfaces
- **Application**: Use cases and business logic
- **Infrastructure**: Data access, external services
- **API**: HTTP endpoints and controllers

## Quick Start

### Development

```bash
# From WSL/Linux/Mac
./run dev

# From Windows PowerShell
.\scripts\dev.ps1

# Or directly with dotnet
dotnet run --project src/JobTracker.API
```

The API will start on http://localhost:5250

### Build

```bash
dotnet build
```

### Test

```bash
dotnet test
```

### Database Migrations

```bash
# Create a new migration
dotnet ef migrations add MigrationName --project src/JobTracker.Infrastructure --startup-project src/JobTracker.API

# Update database
dotnet ef database update --project src/JobTracker.Infrastructure --startup-project src/JobTracker.API
```

## API Documentation

When running in development, Swagger UI is available at:
http://localhost:5250/swagger

## Configuration

- `appsettings.json`: Base configuration
- `appsettings.Development.json`: Development overrides
- Environment variables: Production configuration

## Docker

```bash
docker build -t jobtracker-backend .
docker run -p 5250:5250 jobtracker-backend
```