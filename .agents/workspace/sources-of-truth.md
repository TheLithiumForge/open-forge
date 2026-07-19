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
- `src/cli/cli.ts` - the MVP CLI implementation; `src/cli/cli.unit.test.ts`, `src/cli/cli.closure.test.ts`, and `src/cli/extensions.integration.closure.test.ts` cover its pure and command-boundary behavior, with shared infrastructure in `tests/support/` and `tests/run-tests.ts`.
- `src/extensions/` - bundled first-party extensions, each with `extension.json` plus `payload/`; `workflow-essentials` is the first one.
- `benchmarks/` - the reproducible evaluation instrument: harness, seeds, optional variable overlays, and one report per run under `benchmarks/results/` named `<date>-<seed>-<model>-<variables>.md`.
- `build.ts` and `dist/` - build script and build output; never edit `dist/` by hand.
- `docs/dev.md` and `docs/extensions.md` - development and extension authoring docs.
- `.agents/memory/crystallized/documents/evaluations/` - accepted evaluation syntheses; raw run reports live in `benchmarks/results/` and pre-harness reports in `benchmarks/results/pre-harness/`.

Fast development validation: `bun run index`, `bun run test`, and `bun run build`. Use `bun run test:closure` for affected OS/process boundaries and `bun run test:ci` for the complete unit-plus-closure suite before relevant closeout; raw `bun test` discovers both tiers.
