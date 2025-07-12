# CI/CD Troubleshooting Guide

## Overview
This document details the CI/CD issues encountered after a complete architecture migration from .NET to Node.js/Express with React, and all troubleshooting attempts made.

## Context
- **Migration**: Complete rewrite from .NET backend to Node.js/Express API with Next.js frontend
- **Branch**: Working on `o3advice` branch
- **Monorepo Structure**: Using pnpm workspaces with Turbo
- **Initial Issue**: GitHub Actions CI/CD failing with "Dependencies lock file is not found"

## Issues Encountered

### 1. Missing pnpm-lock.yaml
**Symptom**: CI/CD failing with "Dependencies lock file is not found in /home/runner/work/jobtrackerv2/jobtrackerv2"

**Root Cause**: After architecture migration, the pnpm-lock.yaml wasn't properly generated

**Fix Applied**: 
- User manually ran `pnpm install` which generated a proper 376KB lockfile
- Committed with hash `fed7059`

### 2. Dependabot Configuration Issues
**Symptom**: 13 open Dependabot PRs targeting wrong branch with major version updates causing conflicts

**Root Cause**: Dependabot still configured for old architecture and targeting main branch

**Fix Applied**:
```yaml
version: 2
updates:
  - package-ecosystem: "npm"
    directory: "/"
    target-branch: "o3advice"
    schedule:
      interval: "monthly"
    open-pull-requests-limit: 5
    ignore:
      - dependency-name: "eslint"
        update-types: ["version-update:semver-major"]
      - dependency-name: "@typescript-eslint/*"
        update-types: ["version-update:semver-major"]
      # ... other major version ignores
```

### 3. ESLint Configuration Conflicts
**Symptom**: 
- ESLint looking for app directory at `/apps/web/app` instead of `/apps/web/src/app`
- Error: "Definition for rule '@typescript-eslint/no-explicit-any' was not found"

**Root Cause**: Conflicting ESLint configurations between base config and Next.js

**Fix Applied**:
- Created `.eslintrc.base.js` for shared rules
- Simplified `/apps/web/.eslintrc.json`:
```json
{
  "extends": "next/core-web-vitals",
  "settings": {
    "next": {
      "rootDir": "src"
    }
  },
  "rules": {
    "@next/next/no-html-link-for-pages": "off"
  }
}
```

### 4. TypeScript Module Resolution Issues
**Symptom**: "Invalid module name in augmentation, module 'express-serve-static-core' does not resolve to an untyped module"

**Root Cause**: Express Request type augmentation not properly configured

**Fix Applied**: Created `/apps/api/src/types/express.d.ts`:
```typescript
declare global {
  namespace Express {
    interface Request {
      user?: {
        id: string
        email: string
      }
    }
  }
}
export {}
```

### 5. Broken Symlinks in Web Directory
**Symptom**: Directory entries showing as `d?????????` with permission errors:
```
d????????? ? ?         ?              ?            ? app
d????????? ? ?         ?              ?            ? components
d????????? ? ?         ?              ?            ? hooks
d????????? ? ?         ?              ?            ? lib
d????????? ? ?         ?              ?            ? public
d????????? ? ?         ?              ?            ? types
```

**Attempted Fixes**:
1. `rm -f app components hooks lib public types` - No effect
2. `find . -maxdepth 1 -type l -exec rm {} \;` - Errors: "No such file or directory"
3. `unlink` commands - Ran without error but symlinks persist
4. `rm -rf app 2>/dev/null; ln -s src/app app` - User rejected this approach

**Current Status**: UNRESOLVED - These broken symlinks remain in the filesystem

### 6. Next.js Build Failures
**Symptom**: `next build` fails with "ENOENT: no such file or directory, scandir '/mnt/c/Code/Job_app_resume_design/apps/web/app'"

**Root Cause**: Next.js looking for app directory in wrong location (expects `/apps/web/app` but it's at `/apps/web/src/app`)

**Attempted Fixes**:
1. Removed `experimental.appDir` from next.config.ts (deprecated in Next.js 13+)
2. Updated ESLint config to set `rootDir: "src"`
3. Attempted to create symlink from app to src/app (failed due to broken symlink issues)

**Current Status**: UNRESOLVED

### 7. Vitest Configuration Issues
**Symptom**: 
- API tests failing: "Failed to load url /apps/api/test/setup.ts"
- Web tests trying to run Playwright e2e tests with Vitest

**Fixes Applied**:
1. Created `/apps/api/vitest.config.ts`
2. Created `/apps/api/test/setup.ts`
3. Updated `/apps/web/vitest.config.ts` to exclude e2e tests:
```typescript
test: {
  exclude: ['**/node_modules/**', '**/e2e/**'],
}
```
4. Fixed test setup JSX syntax by using React.createElement instead

### 8. Missing Public Directory
**Symptom**: Vitest failing with "ENOENT: no such file or directory, scandir '/apps/web/public'"

**Root Cause**: Public directory exists at `/apps/web/src/public` not `/apps/web/public`

**Fix Applied**: Added `publicDir: 'src/public'` to vitest.config.ts

## Current State

### Working Commands:
- ✅ `pnpm install`
- ✅ `pnpm lint`
- ✅ `pnpm type-check`
- ✅ `pnpm test:unit`

### Failing Commands:
- ❌ `pnpm build` - Next.js can't find app directory due to broken symlinks

## Environment Details
- Node.js: v24.1.0
- pnpm: v9.15.9
- Next.js: 15.1.0
- TypeScript: 5.4.0
- Platform: WSL2 on Windows

## Key Findings from Sequential Thinking Analysis

The root cause of all issues stems from:
1. **Incomplete migration cleanup** - Old build artifacts and configuration files left behind
2. **Corrupted filesystem entries** - Broken symlinks that can't be removed normally
3. **Tool version mismatches** - Dependencies updated to incompatible major versions
4. **Configuration drift** - Configs still pointing to old directory structure

## Recommended Next Steps

1. **Filesystem Recovery**: The broken symlinks appear to be filesystem corruption in WSL2. Consider:
   - Restarting WSL2: `wsl --shutdown` then restart
   - Running filesystem check from Windows side
   - As last resort, clone to a fresh directory

2. **Build Configuration**: Need to resolve Next.js app directory location issue:
   - Option A: Move app directory from `src/app` to `app` (matches Next.js default)
   - Option B: Configure Next.js to look in `src/app` (may need custom webpack config)
   - Option C: Fresh Next.js setup with proper structure

3. **CI/CD**: Once local builds work, the GitHub Actions should pass with:
   - Proper pnpm-lock.yaml (already committed)
   - Updated configurations (already fixed)
   - Resolved directory structure

## Files Modified During Troubleshooting

1. `.github/dependabot.yml` - Updated target branch and ignore rules
2. `.eslintrc.base.js` - Created shared ESLint config
3. `/apps/web/.eslintrc.json` - Simplified Next.js ESLint config
4. `/apps/api/.eslintrc.json` - Created API ESLint config
5. `/apps/api/src/types/express.d.ts` - Added Express type declarations
6. `/apps/api/vitest.config.ts` - Created API test configuration
7. `/apps/api/test/setup.ts` - Created test setup file
8. `/apps/web/vitest.config.ts` - Updated to exclude e2e tests and fix public dir
9. `/apps/web/test/setup.ts` - Fixed React Konva mocks
10. `/apps/web/next.config.ts` - Removed deprecated experimental.appDir

## Additional Notes

- The codebase uses React Konva for canvas functionality
- Authentication is handled via JWT tokens
- The project follows the structure outlined in `o3advice.md`
- All TypeScript strict mode checks are passing
- The monorepo uses Turbo for build orchestration