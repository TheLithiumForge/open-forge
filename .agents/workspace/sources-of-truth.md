---
open-forge:
  description: Where each kind of truth lives in this repository and when to use each location
  tags: [Workspace, SourceOfTruth, Repository]
---

# Sources Of Truth

Route map for this repository's authoritative locations. Destination files own detailed truth.

- `README.md` - current user-facing truth for what Open Forge is and how to use it.
- `docs/cli.md` - current CLI behavior truth; prefer it over inferring intent from the implementation.
- `docs/framework/` - maintainer and AI governance for this repository; descriptors govern payload files and must not become hidden runtime context for installed users.
- `src/open-forge/` - the installable payload users receive; runtime truth for installed workspaces, including this one.
- `src/cli/cli.ts` - the MVP CLI implementation; `src/cli/cli.test.ts` is its behavior coverage.
- `src/extensions/` - bundled first-party extensions, each with `extension.json` plus `payload/`; `workflow-essentials` is the first one.
- `build.ts` and `dist/` - build script and build output; never edit `dist/` by hand.
- `docs/dev.md` and `docs/extensions.md` - development and extension authoring docs.
- `dogfood-*.md` and `recommendations-*.md` at the repository root - dogfood evidence reports, routed by `.agents/memory/crystallized/documents/dogfood-reports.md`.

Validation commands for this repository: `bun run index`, `bun test`, `bun run build`.
