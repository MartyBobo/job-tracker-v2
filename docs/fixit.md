# Fix It - Known Issues and Solutions

## Active Issues

### 1. React Server Components Bundler Error
**Status**: 🔴 Active  
**First Seen**: 2025-07-11  
**Error**: "Could not find the module in the React Client Manifest"

**Symptoms**:
- Error when accessing http://localhost:3000/auth/login
- Server component trying to import from client manifest

**Potential Fixes**:
1. Check for missing 'use client' directives
2. Verify all client components are properly marked
3. Clear .next cache and rebuild
4. Check for circular dependencies

---

## Resolved Issues

### 2. Authentication Cache Issues
**Status**: ✅ Resolved  
**Resolved**: 2025-07-11  

**Problem**: Browser serving cached JavaScript with old API endpoints
- Old code calling `/api/auth/me` after login
- Stale console.logs appearing

**Solution**:
1. Deleted `.next` directory
2. Cleared browser cache
3. Updated authentication flow to use login response data

### 3. Port 5250 Already in Use
**Status**: ✅ Resolved  
**Resolved**: 2025-07-11  

**Problem**: Backend service already running when trying to start

**Solution**:
Created helper scripts:
- `check-status.sh` - Check if services are running
- `stop-backend.bat` - Stop backend on Windows
- `run-backend.sh` - Start backend with proper checks

### 4. Monorepo Confusion
**Status**: ✅ Resolved  
**Resolved**: 2025-07-11  

**Problem**: Mixed .NET and Node.js concerns at root level
- Confusing package.json files
- Unclear project boundaries
- Difficult development workflow

**Solution**:
Complete project reorganization:
- Separated into `backend/`, `frontend/`, `infrastructure/`
- Clear development commands for each project
- Independent package management

### 5. SQLite Database Access in Docker
**Status**: ✅ Resolved  
**Resolved**: 2025-07-11  

**Problem**: SQLite Error 14: 'unable to open database file' in Docker container
- Container couldn't create database in mounted Windows directory
- Permission issues between Windows host and Linux container
- WSL mount permission conflicts
- SQLite file locking incompatible with Windows filesystem
- Volume mounting conflicts with bind mounts

**Solution**:
Complete redesign with clean separation:
1. **Source Code**: Mounted read-only to `/source`
2. **Workspace**: Copied to Docker volume at `/workspace` 
3. **Database**: Stored in separate volume at `/var/lib/jobtracker`
4. **Frontend**: Custom Dockerfile.dev with selective mounting

Key changes:
```yaml
# Backend volumes
- ../backend/src:/source/src:ro  # Read-only source
- backend-workspace:/workspace    # Build workspace
- backend-data:/var/lib/jobtracker # Database location

# Frontend volumes  
- ../frontend/app:/app/app       # Only mount specific dirs
- frontend-modules:/app/node_modules  # Preserve in volume
```

This eliminates all mounting conflicts and permission issues.

### 6. Authentication Login Failures
**Status**: ✅ Resolved  
**Resolved**: 2025-07-11  

**Problem**: Users unable to login with 401 Unauthorized errors
- Attempting to login with non-existent accounts (test@example.com)
- Database only had one user (jd@gmail.com) with unknown password
- No documentation of test credentials
- Registration flow making redundant API calls

**Solution**:
1. Created seed script `/backend/scripts/seed-dev-data.sh` with test accounts:
   - test@test.com / Test123!
   - demo@demo.com / Demo123!
   - admin@admin.com / Admin123!
2. Fixed registration flow to use response data directly (removed redundant login call)
3. Created development documentation with test credentials
4. Confirmed icon.svg exists (500 error was transient)

---

## Prevention Strategies

### Code Organization
1. **Clear Boundaries**: Keep backend and frontend completely separate
2. **Consistent Structure**: Follow established patterns in each project
3. **Documentation**: Keep README files updated in each directory

### Development Workflow
1. **Cache Management**: Regular cleanup of build artifacts
2. **Port Management**: Use scripts to check port availability
3. **Environment Variables**: Use .env files for configuration

### Testing
1. **E2E Tests**: Catch integration issues early
2. **Type Safety**: Use TypeScript strictly
3. **API Contracts**: Keep frontend and backend in sync

---

## Quick Fixes Checklist

When encountering issues:

1. **Build Issues**
   ```bash
   # Frontend
   cd frontend && rm -rf .next node_modules
   npm install && npm run dev
   
   # Backend
   cd backend && dotnet clean
   dotnet restore && ./run dev
   ```

2. **Port Conflicts**
   ```bash
   # Check what's using the port
   lsof -i :5250  # Linux/Mac
   netstat -ano | findstr :5250  # Windows
   ```

3. **Authentication Issues**
   - Check browser DevTools Network tab
   - Verify tokens in Application > Local Storage
   - Check axios interceptors are working
   - Use test accounts: test@test.com / Test123!

4. **Docker Issues**
   ```bash
   cd infrastructure
   docker-compose down -v
   docker-compose build --no-cache
   docker-compose up
   ```