# Project Updates

## 2025-07-11: Major Architecture Overhaul

### Overview
Complete separation of backend (.NET) and frontend (Next.js) into independent projects with clear boundaries.

### Changes Made

#### 1. Directory Restructuring
- **Before**: Monorepo with mixed concerns at root level
- **After**: Clean separation into `backend/`, `frontend/`, and `infrastructure/`

```
Job_app_resume_design/
├── backend/          # .NET 8 API (Clean Architecture)
├── frontend/         # Next.js 15 Application  
├── infrastructure/   # Docker, scripts, shared tools
└── docs/            # Documentation
```

#### 2. Backend Changes
- Moved all .NET code to `backend/` directory
- Created development scripts:
  - `backend/run dev` - Simple development command
  - `backend/scripts/dev.sh` - Linux/WSL development script
  - `backend/scripts/dev.ps1` - Windows PowerShell script
- Updated Dockerfile for new structure
- Added comprehensive README.md

#### 3. Frontend Changes  
- Kept frontend in its existing location
- Added `.env.development` for API configuration
- Created production-ready Dockerfile
- Updated README.md with current tech stack

#### 4. Infrastructure
- Moved docker-compose files to `infrastructure/`
- Created separate configs for production and development
- Moved helper scripts and utilities

#### 5. Root Cleanup
- Simplified root package.json to workspace manager only
- Removed mixed Node.js dependencies
- Clear separation of concerns

### Development Workflow

#### Local Development
```bash
# Backend (from project root)
cd backend && ./run dev

# Frontend (from project root) 
cd frontend && npm run dev
```

#### Docker Development
```bash
cd infrastructure
docker-compose -f docker-compose.dev.yml up
```

### Benefits
1. **Clear Separation**: Each project can be developed, tested, and deployed independently
2. **Technology Focus**: Backend developers work in .NET ecosystem, frontend in Node.js
3. **Simplified Dependencies**: No confusion about which package.json manages what
4. **Better CI/CD**: Each project can have its own pipeline
5. **Easier Onboarding**: New developers immediately understand the structure

### Migration Notes
- Database files remain in `backend/data/`
- Upload directory moved to `backend/uploads/`
- All backend scripts updated for new paths
- Docker configurations updated for new structure

### Next Steps
- Test new development workflow
- Update CI/CD pipelines for new structure
- Create separate deployment strategies