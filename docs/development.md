# Developing Open Forge

This guide is for people changing Open Forge itself. Start with the [README](../README.md) when you want to install or use the Framework.

Open Forge currently uses:

- Bun 1.3.11 for repository development, as declared in `package.json`
- Node.js 18 or later for the distributed CLI
- Git for review checkpoints and recovery

Install dependencies:

```sh
bun install
```

## Source Responsibilities

Open Forge dogfoods its own Framework. Before changing the product, enter through `AGENTS.md` and follow the routed workspace contract.

Different sources answer different questions:

| Source | Responsibility |
|---|---|
| Current documents | Accepted product and architectural design |
| Installed source under `src/open-forge/` | Exact agent-facing runtime wording |
| CLI and Extension source | Exact deterministic behavior and package contents |
| Decisions | Why consequential choices were accepted |
| Maintenance contracts | Repository obligations, relationships, and verification for reviewed sources |
| Public documentation | How to understand and use the current product |
| Tests | Mechanical evidence for observable behavior |

Repository-only Maintenance contracts live under `.agents/memory/crystallized/documents/maintenance/`. Users do not need them to understand an installed workspace.

Use the [Sources Of Truth map](../.agents/workspace/sources-of-truth.md) to locate the current source for a product, architecture, implementation, documentation, or release question before editing.

When a contract changes, update every affected representation together. This may include the current document, installable source, dogfood counterpart, Maintenance contract, public documentation, and tests. Do not update unrelated surfaces merely because they link to the changed concept.

## Repository Layout

```text
.agents/                 # Open Forge dogfood, current design, decisions, and maintenance
src/open-forge/          # installable Framework source
src/extensions/          # optional first-party Extension packages
src/cli/cli.ts           # current CLI MVP implementation
src/cli/*.test.ts        # unit and closure tests near the implementation
tests/                   # test runner and shared test support
benchmarks/              # developer-only evaluation harness and scenarios
docs/                    # public user guides and repository development guidance
build.ts                 # release build
dist/                    # generated release output
```

The distributed CLI runs on Node.js. Bun runs repository builds, tests, source indexing, and benchmark helpers.

## Normal Change Flow

1. Locate the accepted current design and the exact implementation source
2. Check related Decisions for rationale and rejected alternatives
3. Make one coherent change across every affected source
4. Rebuild generated `Entries` when routed files changed
5. Run the smallest useful tests while developing
6. Run closure tests when the change crosses a process, filesystem, Git, packaging, or lifecycle boundary
7. Run `doctor` against the dogfood workspace and installable source
8. Review the complete Git diff for unintended generated, packaged, or documentation changes

Useful validation commands:

```sh
bun run index
bun run src/cli/cli.ts doctor
bun run src/cli/cli.ts doctor src/open-forge
git diff --check
```

Use review effort proportionate to the change. Broad or high-risk Framework changes may benefit from separate semantic-loss, writing-quality, and optimization reviews. Routine changes do not require a fixed review panel.

The CLI's write commands normally require a clean Git checkpoint. Add `--pro` during development only when intentionally testing the expert bypass. Lifecycle tests should use fresh real repositories instead.

## Commands

| Command | Purpose |
|---|---|
| `bun run index` | Rebuild generated routes in `src/open-forge/` |
| `bun run test` | Run the routine fast unit suite |
| `bun run test:fast` | Run the same fast unit suite explicitly |
| `bun run test:closure` | Run subprocess, filesystem, Git, packaging, and lifecycle tests |
| `bun run test:ci` | Run the complete unit and closure suite |
| `bun run build` | Build the Node CLI and release artifacts |
| `bun run bench` | Run the developer benchmark harness |

Run the TypeScript source during development:

```sh
bun run src/cli/cli.ts --help
bun run src/cli/cli.ts extend --list
bun run src/cli/cli.ts doctor
```

After building, exercise the distributed Node CLI:

```sh
node ./dist/cli.mjs --help
node ./dist/cli.mjs extend --list
node ./dist/cli.mjs doctor ./dist/open-forge-src
```

## Tests

The routine suite is deliberately fast and storage-light:

```sh
bun run test
```

Files named `*.unit.test.ts` exercise pure algorithms and state transitions without operating-system temporary directories, Git repositories, builds, or child CLI processes.

Files named `*.closure.test.ts` cross a real process, filesystem, Git, packaging, or benchmark boundary:

```sh
bun run test:closure
```

Run closure tests before closing work that changes:

- CLI arguments, output, or exit behavior
- Installation, update, or removal effects
- Planning, rollback, containment, collisions, or ownership
- Git checkpoint behavior
- Packaged layouts or release artifacts
- Context loading, routing, or generated indexes across a real workspace

Run the complete suite with:

```sh
bun run test:ci
```

`bun run test` selects the fast tier through the repository runner. A raw `bun test` follows Bun's default discovery and runs both unit and closure files, so it is not the routine fast command.

`tests/run-tests.ts` resolves the repository from its own location and passes absolute paths. Shared support in `tests/support/index.ts` provides temporary sandboxes, subprocesses, Git fixtures, path checks, tree snapshots, and repository paths.

Closure tests invoke the public CLI as a child process with argument arrays and an explicit working directory. Use real Git repositories when checkpoint behavior is part of the contract. Assert resulting bytes, ownership, route validity, Git state, and failure atomicity where applicable. File existence alone is not sufficient evidence.

The optional `cli-testing-patterns` Extension provides the installable version of this testing approach.

## Documentation Voice

The [Open Forge Writing Standard](../.agents/memory/crystallized/documents/maintenance/writing.md) defines shared voice, clarity, terminology, and punctuation rules for this repository.

Documentation has distinct responsibilities:

| File | Responsibility |
|---|---|
| `README.md` | Present the accepted vision, problem, quick start, main advantages, examples, and progressive product overview |
| `docs/cli.md` | Explain current CLI commands, options, safety behavior, outputs, and limitations |
| `docs/extensions.md` | Explain how to choose, install, author, update, remove, and reason about Extensions |
| `docs/development.md` | Explain how to change, verify, build, and release Open Forge |

The README must begin from the same accepted ACE definition and product summary as the Vision. It should progressively reveal advanced routing, Memory, customization, Extensions, and tooling without becoming their complete reference.

Link to the authoritative detailed source instead of copying its full contract into several guides. Repeat enough meaning at an independent entry point for the reader to understand why and when to follow the link.

## Build Output

Build all release artifacts:

```sh
bun run build
```

The build produces:

```text
dist/
  cli.mjs
  extensions/
  open-forge-src/
  open-forge-src.manifest.json
  open-forge-src.tar.gz
  open-forge-src.tar.gz.sha256
```

`open-forge-src/` is the complete standalone Framework payload. The archive, manifest, and checksum describe that payload. `dist/extensions/` contains the bundled first-party catalogue used by the adjacent built CLI.

## npm Package

The npm package includes:

```text
dist/cli.mjs
docs/cli.md
docs/development.md
docs/extensions.md
src/open-forge/
src/extensions/
README.md
LICENSE
package.json
```

There are no postinstall scripts. The package contains the already-built Node CLI.

Inspect the package before publishing:

```sh
bun run build
npm pack --dry-run
```

Publish only after version metadata, release notes, tests, build output, and package contents are accepted:

```sh
npm publish
```

## GitHub Release Artifacts

Attach these files from `dist/`:

```text
open-forge-src.tar.gz
open-forge-src.tar.gz.sha256
open-forge-src.manifest.json
```

Do not attach `cli.mjs` by itself. The executable resolves payloads from the npm package layout or from adjacent `open-forge-src/` and `extensions/` directories in the complete `dist/` layout.

## Release Checklist

1. Confirm current documents and public documentation describe the accepted release
2. Run `bun run index` and review generated changes
3. Run `bun run test:ci`
4. Run `bun run build`
5. Exercise the built Node CLI against `dist/open-forge-src`
6. Run `npm pack --dry-run` and inspect the included files
7. Confirm the release archive checksum and manifest were generated
8. Publish or attach only the accepted complete artifacts

Consume Open Forge from released artifacts rather than a moving branch.
