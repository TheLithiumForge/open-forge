---
open-forge:
  description: Historical CLI-v2 source: Exact accepted replacement CLI development tools, strict checking, test configuration, portable artifact boundary, runtime floors, and embedded-asset build shape
  responsibility: Define accepted development and build relationships, quality and portability obligations, and how exact executable values from package metadata, the lockfile, configuration, scripts, and focused source fit together
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Development Toolchain

## Authority And Transition

This document owns the accepted meaning of replacement development and build
configuration. The [toolchain
Decision](../../decisions/cli/cli-development-toolchain.md) preserves why these
choices were accepted.

[`package.json`](../../../../../package.json), `bun.lock`, `tsconfig.json`,
`tsconfig.test.json`, [`eslint.config.mjs`](../../../../../eslint.config.mjs),
[`/.prettierrc.json`](../../../../../.prettierrc.json), `build.ts`, and the
focused source under `src/cli/build/` and `src/cli/testing/` now own their exact
executable values. This document retains their semantic relationship and
compatibility obligations rather than maintaining a second option inventory.

## Development Orchestration

Bun is the single development orchestrator:

- `bun install` manages dependencies.
- `bun run` launches repository scripts.
- `bun test` runs direct, integration, and end-to-end tests.
- `bun:test` supplies test declarations, assertions, and snapshots.
- `Bun.build` produces the distributed CLI bundle.
- Native TypeScript 7 `tsc` performs separate strict type checking.
- ESLint performs separate read-only source validation.
- Prettier performs separate repository formatting and format checking.

## Formatting, Linting, And Type Checking

Prettier formats the repository independently of ESLint. The root
[`/.prettierrc.json`](../../../../../.prettierrc.json) defines formatting,
while the package scripts expose `format` for writing and `format:check` for
read-only verification. `eslint-config-prettier` is the final flat ESLint
configuration so formatting rules remain disabled there. Formatting is not a
shipped CLI behavior and does not format user workspaces.

ESLint is the lint engine. `typescript-eslint` supplies its TypeScript parser,
plugin, flat-config helper, and typed presets. `@eslint/js` supplies ESLint's
JavaScript rules. ESLint's recommended rules and the type-aware
`strictTypeChecked` and `stylisticTypeChecked` presets apply to replacement
source and tests under `src/cli/**/*.ts`. A lightweight authored-source layer
separately enforces the accepted import policy across repository JavaScript and
TypeScript, including frozen `src/cli-mvp/`. This policy coverage does not
reopen frozen MVP behavior or authorize running its retained suites. ESLint
excludes generated output, dependencies, coverage, and archived evidence.
`lint` never applies fixes.

TypeScript 7 currently has no programmatic compiler API, while
`typescript-eslint` requires one. Use TypeScript side by side:

```json
{
  "devDependencies": {
    "@typescript/native": "npm:typescript@^7",
    "typescript": "6.0.2"
  }
}
```

The `@typescript/native` alias owns the native `tsc` executable invoked
explicitly by `typecheck`. The `typescript` package name exposes the
TypeScript 6 compatibility API used by `typescript-eslint`; its `tsc`
executable is not a normal gate. Keep configuration within the intentional
TypeScript 6 and 7 intersection. Remove the bridge when TypeScript 7 exposes a
stable API and `typescript-eslint` supports it. The dependency lock records
exact versions.

## TypeScript Projects

Production and test ambient types remain separate:

| Project              | Includes                                                             | Ambient types   |
| -------------------- | -------------------------------------------------------------------- | --------------- |
| `tsconfig.json`      | Replacement `src/cli/**/*.ts`, excluding tests and frozen MVP source | Node.js only    |
| `tsconfig.test.json` | Replacement `*.test.ts` files and focused replacement test helpers   | Node.js and Bun |

`tsconfig.test.json` extends the production configuration, replaces its
`include` and `types`, and imports production modules normally. `typecheck`
runs native TypeScript 7 against both projects in order.

Both projects use `ES2023`, `ESNext` modules, `Bundler` module resolution,
forced module detection, verbatim module syntax, isolated-module compatibility,
no emit, and an explicit repository root. They enable `strict`,
`exactOptionalPropertyTypes`, `noUncheckedIndexedAccess`,
`noPropertyAccessFromIndexSignature`, `noImplicitOverride`,
`noImplicitReturns`, `noFallthroughCasesInSwitch`,
`noUncheckedSideEffectImports`, `noUnusedLocals`, `noUnusedParameters`,
`forceConsistentCasingInFileNames`, `allowUnreachableCode: false`,
`allowUnusedLabels: false`, and `skipLibCheck: false`.

Do not add incremental cache files initially. Introduce them only when measured
cost justifies their lifecycle state.

## Snapshots

Use Bun's ordinary snapshot workflow. Tests pass normalized values to
`toMatchSnapshot()`, and Bun stores colocated `.snap` files. The repository
exposes `test:snapshot:update -- <exact-test-file>` for replacement tests. Its
focused snapshot-update script validates one direct, integration, or end-to-end
test path and invokes Bun's `--update-snapshots` mode only for that file. Do not
add an update-all shortcut or repository-owned snapshot framework.

Snapshot boundaries and evidence placement follow the [CLI Tiered Test
Slice](../../../../patterns/open-forge/cli/bun/tiered-test-slice.md).

## Portable Artifact

Development tooling may depend on Bun. Distributed production code may not
depend on the `Bun` global, `bun:` modules, Bun-only import metadata, or another
Bun-only runtime behavior.

Build `src/cli/cli.ts` as ESM with the Node target. Do not use
`bun build --compile`, bytecode, or a Bun-targeted bundle. Bun-hosted
end-to-end tests spawn the same built artifact under every supported runtime
and verify representative read and mutation journeys.

The artifact keeps the standard `#!/usr/bin/env node` shebang. A conventional
installed command is `open-forge`; Bun and Deno may launch or install the same
artifact through explicit runtime mechanisms. A polyglot shebang and
runtime-specific bundles are outside this project.

## Runtime Compatibility

Initial floors are Node.js `22.12.0`, Bun `1.3.0`, and Deno `2.8.0`. Node.js
and Bun are fully supported. Deno exposes the same public CLI and application
contract, while an operation blocks when its runtime and filesystem cannot
prove a required capability. The [Runtime Compatibility
contract](contracts/runtime-compatibility.md) owns invocation, parity,
capability gating, and release evidence.

## Embedded Assets

`build:assets` generates `.temp/cli/embedded-assets.generated.ts`. `build`
invokes `build:assets` before producing the candidate. After checkout or payload
changes, the mastermind prepares generated assets before static validation.
`typecheck`, `lint`, and `check:fast` validate an already-prepared workspace.

Production modules use standards-based APIs and the deliberately supported
`node:` compatibility surface. Build-time generated modules embed the exact
Framework payload, first-party Extension catalogue, and their fingerprints in
`dist/cli.mjs`. The build does not copy standalone source trees or catalogue
trees into `dist/` and does not create a repository-owned archive, manifest,
or checksum for them. The executable never depends on adjacent runtime
resources, development paths, or Bun-only helpers. The source repository
remains the inspectable source for the authored Framework and Extension
catalogue.

The temporary typed generated module exports readonly `BuildIdentity` and
`EmbeddedAssets` values containing canonical paths, exact UTF-8 text, per-file
SHA-256 values, and aggregate fingerprints. The composition root injects them
into `main()`; tests inject compact fixtures through the same interfaces.
Aggregate fingerprints use the domain-separated sorted path/checksum
serialization defined by the [CLI Architecture](architecture.md), not
filesystem enumeration order or naive concatenation.

## Verification

The source-owned `check` gate checks formatting, builds once, runs strict
TypeScript and read-only ESLint against the prepared workspace, then runs all
replacement tests natively without another build. `test:e2e` and `test:all`
also build once before parallel files run. Focused evidence uses native Bun paths
and `--test-name-pattern`.

Replacement test names begin with one `@unit`, `@integration`, or `@e2e` tier
tag and a durable subject tag. Runtime cases may add `@node`, `@bun`, or `@deno`.
The parallel selections in `test`, `test:integration`, `test:e2e`, and
`test:all:prepared` pass bare `--parallel --parallel-delay=0`. Bare `--parallel`
lets Bun resolve the portable CPU-count ceiling for file workers, while zero
delay starts all resolved workers immediately. `test:all` and `check` preserve
that selection by delegating to `test:all:prepared` after their single build.
`test:integration` and `test:all:prepared` pass
`--path-ignore-patterns="**/update-snapshot.integration.test.ts"`; updater
evidence instead runs explicitly against that exact path without parallel
workers. These flags do not make cases within a file concurrent. Cases remain
sequential unless that separate concurrency boundary is explicitly proved.
Mutable test and invocation state is isolated; filesystem and process tests use
`createTestWorkspace()`. A parallel end-to-end file worker owns one unique
package-manager environment and cache, isolated between workers and shared only
by its sequential cases. The updater exclusion does not change the focused
one-target `test:snapshot:update` contract.

## Related Current Sources

- [CLI Architecture](architecture.md)
- [CLI Runtime Compatibility Contract](contracts/runtime-compatibility.md)
- [CLI Tiered Test Slice](../../../../patterns/open-forge/cli/bun/tiered-test-slice.md)
- [CLI Testing Architecture Decision](../../decisions/cli/cli-testing-architecture.md)
