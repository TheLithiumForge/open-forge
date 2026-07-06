# Current State

Date: 2026-07-06

This is the active continuation snapshot for Open Forge cleanup before formal dogfooding.

## Source Of Truth

- `README.md` is the current user-facing truth.
- `docs/cli.md` is the current CLI behavior truth.
- `docs/framework/` is maintainer and AI governance for this repository.
- `src/open-forge/` is the installable payload users receive.
- `src/cli/cli.ts` is the MVP CLI implementation.
- `src/cli/cli.test.ts` is the current CLI behavior coverage.

## Current Product Shape

Open Forge is a small markdown-first routing framework for AI-assisted work. It stays customizable, reviewable, and diffable instead of shipping a large opinionated default methodology.

Installed users receive only the implementation payload from `src/open-forge/`. Maintainer descriptors in `docs/framework/` must not become hidden runtime context.

The installed `.agents/` payload currently contains:

- `loader.md`
- `directives/`
- `guidance/`
- `memory/`
- `patterns/`
- `skills/`
- `workflows/`
- `workspace/`

Memory currently installs these states:

- `working/`
- `emerging/`
- `crystallized/`
- `archived/`

Memory currently installs these governed child routes:

- `working/handoffs/`
- `working/sessions/`
- `emerging/analysis/`
- `emerging/ideas/`
- `emerging/observations/`
- `crystallized/decisions/`
- `crystallized/documents/`

## Current CLI Shape

The MVP CLI supports:

- `install [target]`
- `extend <extension-source> [target]`
- `extend <bundled-extension-id> [target]`
- `extend --list`
- `index [target]`

Current CLI behavior:

- installs managed payload files
- patches the Open Forge block in `AGENTS.md`
- rebuilds generated index regions
- accepts `open-forge:` and `rune:` frontmatter for generated entries
- accepts compatibility entrypoints `_index.md`, `index.md`, `_references.md`, and `references.md`
- updates recognized scoped framework entrypoints by path shape during install
- installs local extension overlays through `extend` as a dogfooding MVP
- installs bundled first-party extensions from `src/extensions/{id}/payload` when the CLI package contains them

The CLI is still MVP. The final extension registry, manifests, previews, wizards, update/remove behavior, and route-template scaffolding are not designed yet.

## Current Work In Progress

The active uncommitted pass tightened descriptions, handoff wording, workflow wording, crystallized memory wording, and MVP extension overlay support.

Validated during the pass:

- `bun run index`
- `bun test`
- `bun run build`
- `git diff --check`
- `git diff --cached --check`

Do not assume this cleanup memory replaces formal dogfooding. It is only a compact bridge so the old session and idea files can be archived.
