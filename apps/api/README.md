# JobTrackerV2 API

Express.js backend API for the JobTrackerV2 resume builder application.

## Tech Stack

- **Node.js 20** with Express.js
- **TypeScript** for type safety
- **Prisma** ORM for PostgreSQL
- **JWT** for authentication
- **MinIO** for S3-compatible file storage
- **Zod** for request validation
- **Winston** for logging

## Getting Started

### Prerequisites

- Node.js 20+
- PostgreSQL 15+
- MinIO (or S3-compatible storage)
- pnpm package manager

### Installation

1. Install dependencies:
   ```bash
   pnpm install
   ```

2. Set up environment variables:
   ```bash
   cp .env.example .env
   # Edit .env with your configuration
   ```

3. Run database migrations:
   ```bash
   pnpm prisma migrate dev
   ```

4. Seed the database (optional):
   ```bash
   pnpm prisma db seed
   ```

### Development

Start the development server:
```bash
pnpm dev
```

The API will be available at `http://localhost:3001`

### Available Scripts

- `pnpm dev` - Start development server with hot reload
- `pnpm build` - Build for production
- `pnpm start` - Start production server
- `pnpm lint` - Run ESLint
- `pnpm type-check` - Run TypeScript type checking
- `pnpm test` - Run tests
- `pnpm test:unit` - Run unit tests

### API Endpoints

#### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `POST /api/auth/logout` - Logout user
- `POST /api/auth/refresh` - Refresh JWT token

#### Resumes
- `GET /api/resumes` - Get user's resumes
- `GET /api/resumes/:id` - Get specific resume
- `POST /api/resumes` - Create new resume
- `PUT /api/resumes/:id` - Update resume
- `DELETE /api/resumes/:id` - Delete resume
- `GET /api/resumes/:id/export/pdf` - Export resume as PDF

#### Templates
- `GET /api/templates` - Get all templates
- `GET /api/templates/:id` - Get specific template
- `POST /api/templates` - Create template (admin)
- `PUT /api/templates/:id` - Update template (admin)
- `DELETE /api/templates/:id` - Delete template (admin)

#### Users
- `GET /api/users/me` - Get current user profile
- `PUT /api/users/me` - Update user profile
- `POST /api/users/me/change-password` - Change password
- `DELETE /api/users/me` - Delete user account
- `GET /api/users/me/subscription` - Get subscription status

### Database Schema

See `prisma/schema.prisma` for the complete database schema.

### Testing

Run tests with:
```bash
pnpm test
```

### Production Build

Build for production:
```bash
pnpm build
```

Run production server:
```bash
NODE_ENV=production pnpm start
```

### Docker

Build Docker image:
```bash
docker build -t jobtrackerv2-api -f Dockerfile ../..
```

Run with Docker Compose:
```bash
docker-compose up api
```