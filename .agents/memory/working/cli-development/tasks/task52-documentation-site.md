---
open-forge:
  description: Task 52 Docusaurus documentation site with getting started, concepts, a file-by-file Extension reference, and GitHub Pages publication
  tags: [Memory, Working, Task, Documentation, Site, Docusaurus, Contextual, Active]
---

# Task 52 — Documentation site

## Outcome

Requested by the maintainer on 2026-09-25, after the public beta. Open Forge
gets a public documentation site built with Docusaurus and published to GitHub
Pages. It is themed on a darker neon orange and structured so a user can tell
what every installed file is for, even where CLI output explains it poorly.

**Direction:** the maintainer chose `src/docusaurus/` as the location, asked for
suitable ignore rules, and authorized downloading Docusaurus from npm for local
builds. Merging to `main` is the maintainer's decision.

**In scope:**

- A Docusaurus project in `src/docusaurus/` with its own `package.json` and lockfile.
- Site-owned pages: introduction, getting started, concepts, a file-by-file
  page for each of the ten first-party Extensions, a glossary, and limits.
- Publishing `docs/cli.md`, `docs/extensions.md`, and `docs/development.md`
  from their existing location, so each guide keeps one source.
- A GitHub Actions workflow that builds on pull requests and deploys from `main`.

**Preserve / out of scope:** the shipped Framework and Extension payloads, CLI
behavior, and the existing guides' meaning. No push, pull request, Pages
setting change, or deployment is authorized by this task.

**Done when:**

- [x] `npm run build` passes with broken links and anchors set to fail.
- [x] `npm run typecheck` passes under the repository's strict compiler flags.
- [x] The site renders in dark and light themes with the orange palette.
- [x] The maintainer reviewed the site and chose integration into `develop`, then a normal merge to `main`.
- [ ] Pages is set to GitHub Actions and the first deployment succeeds.

## Plan

1. Scaffold the site and theme. Complete.
2. Write site-owned pages from the shipped sources, following the Writing
   Standard and Project Voice. Complete.
3. Publish the existing guides through a second docs instance, rewriting links
   that leave the published set into GitHub URLs. Complete.
4. Add ignores, the Pages workflow, the development guide section, and the
   sources-of-truth map entry. Complete.
5. Maintainer review and integration into `develop`. Complete.
6. Maintainer merge to `main`, push, and Pages enablement.

## Current State

**Now:** committed as one commit and fast-forwarded into local `develop` on
2026-09-25. The maintainer merges `develop` into `main`, pushes, and sets the
Pages source to GitHub Actions.

**Design notes:**

- `future.v4` is enabled with `faster: false`, keeping the webpack bundler and
  avoiding the optional `@docusaurus/faster` native dependency.
- The site package is CommonJS-typed. Declaring `"type": "module"` made the
  server bundle fail with `require.resolveWeak is not a function`.
- `src/remark/repository-links.ts` rewrites relative links outside the published
  pages to `blob` or `tree` URLs on `main`, before Docusaurus resolves links.
- The home page CSS module has an exact `home.module.d.css.ts` declaration so
  the strict `noPropertyAccessFromIndexSignature` flag can stay on.
- The publishing build takes its origin and base path from
  `actions/configure-pages`, through `DOCS_SITE_ORIGIN` and
  `DOCS_SITE_BASE_PATH`. Without them, the site uses the project-site address
  `/open-forge/`. A custom-domain build served from `/` was verified locally.
- `docusaurus build` and `docusaurus start` share the `.docusaurus` cache. A
  build with a different address breaks a running dev server until it restarts.

**Maintenance duty:** the Extension and concept pages describe shipped files.
[Task 53](task53-loading-and-scoping-audit.md) and
[Task 54](task54-tag-trimming.md) must update the affected pages when they
change loading tags or tag sets.
