# Job Application Tracker

A modern job application tracking system with separate backend and frontend applications.

## Architecture

```
Job_app_resume_design/
├── backend/          # .NET 8 Web API (Clean Architecture)
├── frontend/         # Next.js 15 Application
├── infrastructure/   # Docker and deployment configurations
└── docs/            # Project documentation
```

## Quick Start

### Prerequisites
- .NET 8 SDK
- Node.js 20+
- Docker (optional)

### Development

Run each service independently:

```bash
# Terminal 1: Backend
cd backend
./run dev    # or: dotnet run --project src/JobTracker.API

# Terminal 2: Frontend
cd frontend
npm install
npm run dev
```

### Docker Development

```bash
cd infrastructure
docker-compose -f docker-compose.dev.yml up
```

## Services

- **Backend API**: http://localhost:5250
- **Frontend App**: http://localhost:3000
- **API Documentation**: http://localhost:5250/swagger (dev only)

## Technology Stack

### Backend
- .NET 8 with Clean Architecture
- Entity Framework Core 8 with SQLite
- CQRS with MediatR
- JWT Authentication
- ASP.NET Core Identity

### Frontend
- Next.js 15 with App Router
- React 19 with TypeScript
- Tailwind CSS
- React Query & Zustand
- React Hook Form + Zod

## Features

- **Job Application Management**: Track job applications with status updates
- **Interview Scheduling**: Schedule and manage interviews
- **Resume Templates**: Create and manage resume templates
- **Resume Generation**: Generate resumes from templates
- **File Upload**: Upload and manage documents
- **Authentication**: Secure JWT-based authentication
- **Dashboard Analytics**: View application statistics and upcoming interviews

## Project Structure

### Backend (`/backend`)
```
backend/
├── src/
│   ├── JobTracker.API/          # Web API project
│   ├── JobTracker.Domain/       # Domain entities and interfaces
│   ├── JobTracker.Application/  # CQRS commands, queries, DTOs
│   ├── JobTracker.Infrastructure/ # Data access, external services
│   └── Shared/                  # Shared utilities
├── scripts/                     # Development scripts
└── JobTracker.sln              # Solution file
```

### Frontend (`/frontend`)
```
frontend/
├── app/               # Next.js 15 app directory
├── components/        # Reusable UI components
├── lib/              # API client, stores, utilities
├── hooks/            # Custom React hooks
├── types/            # TypeScript definitions
└── e2e/              # Playwright E2E tests
```

## Development Commands

### Backend
```bash
cd backend
./run dev                                    # Start development server
dotnet build                                # Build project
dotnet test                                 # Run tests
dotnet ef migrations add <name> --project src/JobTracker.Infrastructure --startup-project src/JobTracker.API
```

### Frontend
```bash
cd frontend
npm install                                 # Install dependencies
npm run dev                                # Start development server
npm run build                              # Build for production
npm run test:e2e                          # Run E2E tests
```

## Documentation

- [Backend README](./backend/README.md) - .NET API documentation
- [Frontend README](./frontend/README.md) - Next.js app documentation
- [Updates](./docs/updates.md) - Project changelog and updates
- [Fix It](./docs/fixit.md) - Known issues and solutions

## Database

The application uses SQLite for development. The database file is created at:
`backend/data/job_tracker.db`

## Environment Variables

### Frontend (.env.local or .env.development)
```
NEXT_PUBLIC_API_URL=http://localhost:5250/api
```

### Backend (appsettings.Development.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=data/job_tracker.db"
  }
}
```

## Contributing

1. Follow the established patterns in each project
2. Keep backend and frontend changes separate
3. Update documentation when making changes
4. Run tests before committing

## License

MIT