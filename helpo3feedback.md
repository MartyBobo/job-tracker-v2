1 Fix the corrupted symlinks once and for all (WSL2)
Why it happened

    You cloned or moved the repo across two filesystems (Windows NTFS ↔ WSL2 ext4).

    Those phantom entries (d?????????) are Windows junctions or broken POSIX symlinks that WSL2 can’t interpret—or delete—because the inode is gone.

Safe clean-room approach

    Archive any uncommitted work

git stash push -m "wip-before-hard-clean"

Verify Git is tracking real symlinks

git ls-files -s | grep ^120000   # 120000 is the mode for symlinks

If you see entries pointing at app / components / etc., decide whether you truly need symlinks. In a Next.js project you usually don’t.

Reset the working tree completely

git config core.symlinks true   # ensures any genuine links are recreated
git clean -xfd                  # caution: removes untracked files
git reset --hard HEAD

If the phantom dirs survive (they sometimes do on WSL2), clone fresh on the Linux side:

    cd ~
    git clone --depth 1 https://github.com/<you>/<repo>.git jobtrackerv2-clean
    mv jobtrackerv2/.git ./      # optional shortcut to keep history

That leaves you with a symlink-free, corruption-free checkout.
2 Put the Next.js “app” folder where Next expects it
Option A (recommended) – Let Next have /app at workspace root

Move the directory and delete the unneeded src/ layer; it aligns with Next 15 defaults.

mv apps/web/src/app apps/web/app
mv apps/web/src/public apps/web/public
# update imports if you relied on src alias (rare in Next 13+)

Then delete (or empty) src/ if it only contained app and public.
Option B – Keep src/app and remove the ghost symlink

If you prefer src/app:

    Make sure no symlink or folder named app sits beside src/.

    Ensure your next.config.ts does not contain experimental.appDir—Next 13+ auto-detects src/app.

// next.config.ts
import nextConfig from 'next'
export default nextConfig({
  // no experimental.appDir here
  eslint: { dirs: ['src'] }
})

Build check:

pnpm --filter @apps/web dev      # should compile and launch
pnpm --filter @apps/web build    # should produce .next without ENOENT

3 Lock the directory layout in ESLint & TypeScript

// apps/web/.eslintrc.json
{
  "extends": "next/core-web-vitals",
  "settings": { "next": { "rootDir": "./" } },   // root is now ./ not ./src
  "rules": {
    "@next/next/no-html-link-for-pages": "off"
  }
}

// apps/web/tsconfig.json  (paths excerpt)
{
  "compilerOptions": {
    "baseUrl": ".",
    "paths": {
      "@/*": ["app/*"],            // or "src/app/*" if you kept src/
      "public/*": ["public/*"]
    }
  }
}

Run:

pnpm lint
pnpm type-check

4 Re-enable CI

Commit the folder move + config edits, push to o3advice, and watch the workflow.

Typical successful run order:

    setup-node

    pnpm install --frozen-lockfile (lockfile already in repo)

    pnpm lint → green

    pnpm test (Vitest)

    pnpm --filter @apps/web build → should now pass

    Docker build step

If a step fails:

    Comment @claude summarise the failing step and fix it

    Claude Action will propose a patch PR; runner minutes are tiny and tokens come from your Max plan.

5 Optional post-migration housekeeping
Task	Why
Delete old “app” symlink from Git history	Prevent future NTFS ↔ ext4 issues; use git filter-repo only if repo is small.
Update Turbo pipeline (turbo.json)	Point source globs to apps/web/app/** (or src/app/**).
Dependabot ignore list	Confirm the ESLint / TS rules still ignore only major bumps you truly can’t adopt yet.
TL;DR

    Clean checkout to ditch bad inode entries.

    Move src/app → app (or purge the broken symlink if you keep src/app).

    Update ESLint/TS config to reflect final root.

    CI should now sail through; if not, tag @claude on the failing PR and let the workflow auto-patch.

Give those steps a shot—once you have a clean build locally, it’s almost guaranteed that Actions will go green as well. Let me know exactly where (or if) the errors resurface, and we can zero in further.