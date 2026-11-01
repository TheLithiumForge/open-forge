# Materialize
Materialize is a generator that freezes the Neon Pairs specification and builds equal project setups from it.

The editable pack lives in `.agents/memory/crystallized/documents/evals/neon-pairs/`. For example, changing a rule there changes the source hash when you freeze the pack again. The generated copy lives in `src/evals/benchmarks/neon-pairs/specs/`.

## Requirements

Use Node 24 or later. From `src/evals`, run `npm ci` to install the locked tools. Setup generation also needs Git. The Open Forge setup uses the pinned CLI from this package.

## Freeze specs

From `src/evals`, run:

```sh
node materialize/cli.ts specs
```

This strips frontmatter while keeping the body bytes. It writes the ten documents and `manifest.json`, then refreshes `projection.json` from the source descriptions and tags. The command prints the source hash. Repeating it without source changes gives the same hashes.

## Build setups

Choose an absent folder or an empty folder outside the spec source and frozen pack. For example, run:

```sh
node materialize/cli.ts setup linked-agents ./runs/project
```

This writes the project, starts its Git repository and makes one initial commit. The setup manifest stays beside the project at `./runs/project.manifest.json`.

The available setups are:

- `plain-docs` - Game documents under `docs/` and a README pointer.
- `linked-agents` - The same documents and an `AGENTS.md` with each document's description.
- `open-forge` - The same documents with frontmatter, routed through a real Open Forge install.

Every project README has the same acceptance line naming `docs/`. Tests inject runners for Open Forge and Git, so they run without invoking either command.

## Strip setup

`stripSetup()` exports plain documents and keeps added application files. Pass the setup manifest kept outside the project. It checks recorded routing files before removing them from the export.

## Check

From `src/evals`, run:

```sh
npm run typecheck
npm test
```

The product test command excludes reference game tests. Those keep their own compiler projects and test tools.
