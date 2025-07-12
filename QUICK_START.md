# Quick Start Guide

Due to the .NET SDK not being installed, here are three ways to run the application:

## Option 1: Mock Backend (Recommended for Testing)
This uses a Node.js mock backend that implements the same API endpoints.

```bash
# Install dependencies
npm install

# Run both mock backend and frontend
npm run start:mock
```

- Frontend: http://localhost:3000
- Mock Backend: http://localhost:5250

## Option 2: Docker (Requires Docker Desktop)
If you have Docker installed:

```bash
# Build and start both services
docker-compose up

# Or run in background
npm run docker:up

# Stop services
npm run docker:down
```

## Option 3: Install .NET 8 SDK
To use the real .NET backend:

### On Windows (WSL2):
```bash
# Run as administrator in Windows Terminal
winget install Microsoft.DotNet.SDK.8

# Or download from:
# https://dotnet.microsoft.com/download/dotnet/8.0
```

### On Ubuntu/WSL2:
```bash
# You'll need sudo access
chmod +x setup_dotnet.sh
sudo ./setup_dotnet.sh

# Then run normally
npm start
```

## Testing the Application

1. Open http://localhost:3000 in your browser
2. Click "Sign up" to create a new account
3. Login with your credentials
4. Start tracking job applications!

## Troubleshooting

### CORS Errors
- Make sure the backend is running on port 5250
- Check that no other service is using ports 3000 or 5250

### Port Already in Use
```bash
# Find process using port 5250
lsof -i :5250

# Kill the process
kill -9 <PID>
```

### Frontend Not Loading
- Clear browser cache
- Delete `frontend/.next` folder and restart
- Check console for errors