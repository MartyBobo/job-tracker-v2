# JobTracker Frontend

Next.js 15 application for the Job Application Tracker system.

## Tech Stack

- **Framework**: Next.js 15 with App Router
- **UI**: React 19, Tailwind CSS, Radix UI
- **State**: Zustand with persistence
- **Forms**: React Hook Form with Zod validation
- **API**: Axios with interceptors, React Query
- **Testing**: Playwright (E2E)

## Quick Start

### Development

```bash
# Install dependencies
npm install

# Run development server
npm run dev
```

The app will start on http://localhost:3000

### Build

```bash
npm run build
```

### Production

```bash
npm run build
npm start
```

## Environment Variables

Create `.env.local` for local development:

```env
NEXT_PUBLIC_API_URL=http://localhost:5250/api
```

## Testing

```bash
# Run E2E tests
npm run test:e2e

# Run E2E tests with UI
npm run test:e2e:ui

# Debug E2E tests
npm run test:e2e:debug
```

## Project Structure

```
frontend/
├── app/              # Next.js app router pages
├── components/       # Reusable React components
├── lib/             # Utilities, API clients, stores
├── hooks/           # Custom React hooks
├── types/           # TypeScript type definitions
├── public/          # Static assets
└── e2e/            # Playwright E2E tests
```

## Docker

```bash
docker build -t jobtracker-frontend .
docker run -p 3000:3000 -e NEXT_PUBLIC_API_URL=http://localhost:5250/api jobtracker-frontend
```