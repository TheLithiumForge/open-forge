# Materialize
Materialize is a generator that freezes the Neon Pairs specification and builds equal project setups from it.

The editable pack lives in `.agents/memory/crystallized/documents/evals/neon-pairs/`. For example, changing a rule there changes the source hash when you freeze the pack again. The generated copy lives in `src/evals/benchmarks/neon-pairs/specs/`.

## Requirements

Use Node 24 or later. From `src/evals`, run `npm ci` to install the locked tools. The Open Forge setup uses the pinned CLI from this package.

## Freeze specs

From `src/evals`, run:

```sh
node materialize/cli.ts specs
```

This strips frontmatter while keeping the body bytes. It writes the ten documents and `manifest.json`, then refreshes `projection.json` from the source descriptions and tags. The command prints the source hash. Repeating it without source changes gives the same hashes.

Layer 2 uses the frozen player pack in `benchmarks/neon-pairs/cases/l2-defined-idea/specs/`. `materializeSubset()` selects only clauses marked `L2` in `requirements.json`, keeps their defining headings and writes a separate manifest. Tests compare all 187 clauses across 60 requirement IDs with the frozen files.

`node materialize/cli.ts subset` regenerates a validated player pack when its clause selection and text stay the same. To accept changed clause IDs, order or text, give the reason:

```sh
node materialize/cli.ts subset --accept-selection-change "Accept the spec v2 counter caps."
```

The manifest's `selectionChanges` records the reason, old and new fingerprints and counts, added and removed IDs and IDs with updated text. Regeneration keeps earlier records. The flag keeps the existing file and hash checks, so edited packs and unrelated files still stop the command.

## Build setups

Choose an absent folder or an empty folder outside the spec source and frozen pack. For example, run:

```sh
node materialize/cli.ts setup linked-agents ./runs/project
```

This writes the project and its Git repository with one initial commit. It writes repository objects and the index directly, so it needs no Git process or global Git settings. The setup manifest stays beside the project at `./runs/project.manifest.json`.

The available setups are:

- `plain-docs` - Game documents under `docs/` and a README pointer.
- `linked-agents` - The same documents and an `AGENTS.md` with each document's description.
- `open-forge` - The same documents with frontmatter, routed through a real Open Forge install.
- `spec-in-prompt` - The Layer 2 player pack in the task prompt, with an empty project apart from its README.

Document forms use the same acceptance line naming `docs/`. The inline form says to follow the requirements supplied with the task. `setupPrompt()` appends either the neutral document pointer or the player pack in document and clause order.

`materializeSetup()` defaults to Minimal for Open Forge. The runner passes `preset` to build Minimal, Essentials and Full Core as separate cells. Tests inject the Open Forge runner and check repository objects without invoking Git.

## Strip setup

`stripSetup()` exports plain documents and keeps added application files. Pass the setup manifest kept outside the project. It checks recorded routing files before removing them from the export.

## Check

From `src/evals`, run:

```sh
npm run typecheck
npm test
```

The product test command excludes reference game tests. Those keep their own compiler projects and test tools.
