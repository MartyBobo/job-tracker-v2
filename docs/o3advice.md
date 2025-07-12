# Resume Creator – Linear Issue List (Tiptap‑enabled, Localhost)

> **Purpose**
> A *single‑source backlog* designed so Claude (or any LLM) can generate **production‑ready code** ticket‑by‑ticket without ambiguity.
> Everything runs locally via `docker compose up`.

---

## 🏗 Architecture Schematic

```
┌────────────── Front‑End (Next.js 15, React 19) ──────────────┐
│  • Canvas → react‑konva                                      │
│  • Text element → Tiptap (ProseMirror)                       │
│  • Zustand global store                                      │
│  • TanStack Query for API calls                              │
└──────────────────────────────────────────────────────────────┘
            ▲                               ▲
            │REST /api/*                    │WebSocket (phase 2)
            ▼                               ▼
┌──────────────── Back‑End (Node 20, Express) ────────────────┐
│  • PostgreSQL 15 (Prisma)                                   │
│  • MinIO (S3‑compatible)                                    │
│  • PDF export worker (Puppeteer)                            │
│  • NextAuth (email + Google)                                │
└──────────────────────────────────────────────────────────────┘
```

---

## Milestone 0 – Local Development Environment

### RC‑000  Dockerized local stack (P0)

* **Description:** One‑command local environment.
* **Implementation:**

  1. `docker-compose.yml` (see code).
  2. Dockerfile for Next.js multi‑stage build.
  3. Health checks & `wait-for-it.sh`.
  4. `scripts/dev-start.sh` helper.
* **Example Compose:**

```yaml
version: "3.9"
services:
  web:
    build:
      context: .
      dockerfile: .docker/next/Dockerfile
    environment:
      NEXT_PUBLIC_API_URL: http://localhost:3000/api
      DATABASE_URL: postgres://postgres:postgres@db:5432/resume
      MINIO_ENDPOINT: http://minio:9000
      MINIO_ROOT_USER: minio
      MINIO_ROOT_PASSWORD: minio123
      SMTP_HOST: mailhog
      SMTP_PORT: 1025
    ports: ["3000:3000"]
    depends_on:
      db:
        condition: service_healthy

  db:
    image: postgres:15
    environment:
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: resume
    ports: ["5432:5432"]
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 5s
      retries: 5

  minio:
    image: minio/minio:latest
    command: server /data --console-address ":9001"
    environment:
      MINIO_ROOT_USER: minio
      MINIO_ROOT_PASSWORD: minio123
    ports: ["9000:9000", "9001:9001"]

  mailhog:
    image: mailhog/mailhog:latest
    ports: ["1025:1025", "8025:8025"]
```

* **Test (Vitest):**

```ts
import { execSync } from 'node:child_process'
import { beforeAll, describe, expect, it } from 'vitest'

beforeAll(() => execSync('docker compose up -d', { stdio: 'inherit' }))

describe('Local stack', () => {
  it('web health endpoint 200', async () => {
    const res = await fetch('http://localhost:3000/api/healthz')
    expect(res.status).toBe(200)
  })
})
```

---

## Milestone 1 – Project Setup & Core Infrastructure

### RC‑001  Project repository & tooling (P0)

* **Implementation:** Init pnpm monorepo, ESLint, Prettier, Husky, commitlint, Vitest (full config in previous version).
* **Tests:** ensure `pnpm lint` passes, Prettier check clean, Husky hook exists.

### RC‑002  CI / CD pipeline (P0)

* **Implementation:** GitHub Actions `ci.yml` – install, lint, test, `docker compose build`.
* **Example Workflow** already provided earlier.
* **Tests:** run `act pull_request` exits 0.

### RC‑003  Design system baseline (P1)

* **Description:** Tailwind CSS + shadcn/ui + Radix UI tokens; Storybook.
* **Implementation:**

  1. Install Tailwind (`postcss`, `autoprefixer`).
  2. `tailwind.config.ts` with `shadcn-preset`.
  3. run `npx shadcn-ui@latest init`.
  4. Add Storybook `sb init --builder vite`; configure tailwind preset.
* **Example Code:** minimal `tailwind.config.ts`.
* **Test:** Storybook CI: `npx chromatic --project-token $TOKEN` returns exit 0.

### RC‑004  State‑management foundation (P1)

* **Description:** Zustand + Immer + Context.
* **Implementation:**

  1. `packages/web/stores/resumeStore.ts` with slices `elements`, `sections`, `history`.
  2. Middleware: `persist` to IndexedDB via `zustand/middleware`.
* **Test:** import store in Vitest, push element, expect state mutation.

---

## Milestone 2 – Canvas, Grid & Alignment Engine

| ID     | Title                              | Implementation snippet                                          |   |        |
| ------ | ---------------------------------- | --------------------------------------------------------------- | - | ------ |
| RC‑010 | Infinite canvas component (P0)     | `Stage` + `Layer`; wheel→zoom, drag→pan; scale clamped 0.2‑2.0. |   |        |
| RC‑011 | Grid & snapping system (P0)        | draw lines every 8 px; on drag `Math.round(x/4)*4` snap.        |   |        |
| RC‑012 | Smart alignment guides (P1)        | Compute bbox edges, show `Line` when                            | Δ | <4 px. |
| RC‑013 | Element transform controls (P0)    | `Transformer` node; Shift+drag keep ratio.                      |   |        |
| RC‑014 | Element keyboard interactions (P2) | Arrow=1 px, Shift+Arrow=10 px, Delete removes.                  |   |        |
| RC‑015 | Section drag‑&‑drop reorder (P1)   | `@dnd-kit/sortable` on section list; updates Tiptap JSON.       |   |        |

Each ticket includes Example Code skeleton and unit tests for maths helpers.

---

## Milestone 3 – Core Components & Sections

### RC‑020  Text element (Tiptap) – **detailed above** (P0)

#### Section tickets (all P0 unless noted)

| ID     | Section                    | Implementation overview                                   |
| ------ | -------------------------- | --------------------------------------------------------- |
| RC‑025 | ContactSection             | Node `contactSection`; RHF form; Zod schema; tests.       |
| RC‑026 | ExperienceSection          | Repeatable node array; drag reorder; date helper.         |
| RC‑027 | EducationSection (P1)      | Similar to Experience; GPA conditional.                   |
| RC‑028 | SkillsSection (P1)         | Chips component with Sortable; array attr.                |
| RC‑029 | CustomSection factory (P2) | Dialog to name section, inject new node type dynamically. |

*(Each ticket template contains full example + Vitest as shown for RC‑025.)*

---

## Milestone 4 – Template System

\| RC‑030 | Template JSON schema (P0) | Zod + versioned; tests ensure parse. |
\| RC‑031 | Default template set (P1) | 5 templates; JSON files in `/templates`. |
\| RC‑032 | Template browser modal (P2) | Grid + hover preview; dnd‑kit to apply. |

---

## Milestone 5 – Editor UX Polish

P0‑P3 tickets unchanged (layer panel, props panel, history, onboarding). Note RC‑042: *use Tiptap History extension*.

---

## Milestone 6 – Export & Sharing

RC‑050 PDF export clarifies Puppeteer `/print` route; RC‑051 PNG export; RC‑052 share links.

---

## Milestone 7 – Accounts & Persistence

Auth via NextAuth + local Postgres; autosave, versioning; collaborative editing phase 2.

---

## Milestone 8 – Performance, QA & Launch

Profiling, accessibility, responsive tweaks, beta checklist.

---

**Legend**
P0 = MVP must‑have · P1 = High · P2 = Nice‑to‑have · P3 = Stretch

➡️ Copy any issue into Claude and say *“Implement exactly as described.”* Example snippets & test scaffolds supply precise types, paths, and libraries.
