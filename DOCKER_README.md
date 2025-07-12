# Docker Setup for Job Tracker

## Quick Start

```bash
# Build and start both services
docker-compose up --build

# Or run in background
docker-compose up -d --build
```

## Services

- **Backend**: .NET 8 API running on http://localhost:5250
- **Frontend**: Next.js app running on http://localhost:3000

## Common Commands

```bash
# Start services
docker-compose up

# Start in background
docker-compose up -d

# Stop services
docker-compose down

# Rebuild after code changes
docker-compose up --build

# View logs
docker-compose logs backend
docker-compose logs frontend
docker-compose logs -f  # Follow all logs

# Access backend container
docker-compose exec backend sh

# Access frontend container  
docker-compose exec frontend sh

# Clean up everything (including volumes)
docker-compose down -v
```

## Troubleshooting

### Port already in use
```bash
# Find what's using the port
netstat -ano | findstr :5250
netstat -ano | findstr :3000

# Kill the process (Windows)
taskkill /F /PID <PID>
```

### Database issues
The database is stored in the `./data` directory. To reset:
```bash
docker-compose down
rm -rf data/
docker-compose up --build
```

### Frontend not loading
If the frontend takes a long time to start, it's installing dependencies. Check logs:
```bash
docker-compose logs frontend
```

## What's Included

- ✅ .NET 8 SDK for backend
- ✅ Node.js 20 for frontend
- ✅ SQLite database (auto-created)
- ✅ Hot reload for both services
- ✅ Persistent data volumes
- ✅ CORS configured

## File Structure

```
/app (in backend container)
├── JobTracker.API.dll     # Main application
├── data/                  # SQLite database
│   └── job_tracker.db
└── uploads/              # File uploads

/app (in frontend container)
├── package.json
├── node_modules/         # Isolated from host
└── .next/               # Build output
```