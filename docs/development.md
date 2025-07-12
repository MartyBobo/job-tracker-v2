# Development Guide

## Getting Started

### Prerequisites
- Docker and Docker Compose
- Node.js 20+ (for local development)
- .NET 8.0 SDK (for local backend development)

### Running the Application

1. **Start all services with Docker:**
   ```bash
   cd infrastructure
   docker-compose -f docker-compose.dev.yml up
   ```

2. **Access the application:**
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5250/api
   - API Health Check: http://localhost:5250/health

### Test Credentials

The following test accounts are available for development:

| Email | Password | Description |
|-------|----------|-------------|
| test@test.com | Test123! | Regular test user |
| demo@demo.com | Demo123! | Demo user account |
| admin@admin.com | Admin123! | Admin user account |

To create additional test users, run:
```bash
./backend/scripts/seed-dev-data.sh
```

## Architecture

### Backend (.NET 8)
- Clean Architecture pattern
- Entity Framework Core with SQLite
- JWT authentication
- Minimal APIs

### Frontend (Next.js 15)
- React 19 with App Router
- TypeScript
- Tailwind CSS
- Zustand for state management
- Axios for API calls

## Common Tasks

### Backend Development

1. **Run migrations:**
   ```bash
   cd backend/src/JobTracker.API
   dotnet ef database update
   ```

2. **Add a new migration:**
   ```bash
   cd backend/src/JobTracker.API
   dotnet ef migrations add MigrationName -p ../JobTracker.Infrastructure
   ```

3. **Run backend locally:**
   ```bash
   cd backend
   ./scripts/dev.sh
   ```

### Frontend Development

1. **Install dependencies:**
   ```bash
   cd frontend
   npm install
   ```

2. **Run development server:**
   ```bash
   npm run dev
   ```

3. **Build for production:**
   ```bash
   npm run build
   ```

## Troubleshooting

### Authentication Issues

1. **"Invalid email or password" error:**
   - Make sure you're using one of the test accounts listed above
   - Run the seed script to ensure test users exist
   - Check backend logs for specific errors

2. **"No refresh token" error:**
   - Clear browser cache and cookies
   - Clear localStorage: Open DevTools Console and run `localStorage.clear()`
   - Restart the frontend container

3. **401 Unauthorized errors:**
   - Token may have expired - try logging in again
   - Check that the backend is running and healthy

### Docker Issues

1. **SQLite permission errors:**
   - The backend uses a named volume for the database
   - If you see permission errors, remove the volume and recreate:
     ```bash
     docker-compose -f infrastructure/docker-compose.dev.yml down -v
     docker-compose -f infrastructure/docker-compose.dev.yml up
     ```

2. **Port conflicts:**
   - Frontend runs on port 3000
   - Backend runs on port 5250
   - Make sure these ports are not in use by other applications

### Frontend Build Issues

1. **"Unterminated string literal" errors:**
   - Clear the frontend build cache:
     ```bash
     docker-compose -f infrastructure/docker-compose.dev.yml down
     docker volume prune -f
     docker-compose -f infrastructure/docker-compose.dev.yml up --build
     ```

2. **Hydration errors:**
   - Make sure you're not mixing server and client components incorrectly
   - Check that all client components have the 'use client' directive

## API Documentation

### Authentication Endpoints

- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login with email/password
- `POST /api/auth/refresh` - Refresh access token
- `GET /api/auth/me` - Get current user info (requires auth)

### Application Endpoints

All require authentication:
- `GET /api/applications` - List job applications
- `POST /api/applications` - Create new application
- `GET /api/applications/{id}` - Get application details
- `PUT /api/applications/{id}` - Update application
- `DELETE /api/applications/{id}` - Delete application

See the full API documentation by visiting the Swagger UI at http://localhost:5250/swagger when running in development mode.