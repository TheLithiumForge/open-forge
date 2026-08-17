---
open-forge:
  description: Historical CLI-v2 source: Use Bun for replacement development while strict TypeScript, ESLint, and built-artifact tests preserve quality and runtime portability
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Development Toolchain

## Context

Selecting separate tools for test execution, snapshots, bundling, and runtime
portability created decision cost without corresponding value for the expected
small CLI. Development simplicity and exported-package portability are
different concerns.

## Decision

Use Bun as the single development orchestrator for dependencies, scripts,
tests, snapshots, and ESM bundling. Keep strict type checking and type-aware
linting as separate read-only gates. Verify portability through the one built
artifact under Node.js, Bun, and Deno rather than requiring the source test
runner to be runtime-neutral.

Use native TypeScript 7 for checking with the official temporary TypeScript 6
programmatic-API bridge required by `typescript-eslint`. Keep production and
test ambient types separate. Use Bun's focused snapshot workflow and a
Node-targeted ESM bundle with the ordinary Node shebang.

`package.json`, the lockfile, configuration, scripts, and focused source are
authoritative for exact executable values. The current [CLI Development
Toolchain](../../documents/cli/development-toolchain.md) defines how those values
fit together and which quality, portability, and compatibility obligations
they preserve.

## Rationale

One cohesive orchestrator reduces commands and cognitive overhead. Bun
provides a fast test runner, snapshots, TypeScript execution, and bundling.
Native TypeScript 7 shortens strict checking, while mature typed ESLint rules
still require the temporary compatibility API.

Keeping portability at the artifact boundary tests the requirement users
actually depend on without forcing every development tool to support every
runtime.

## Rejected Alternatives

- A runtime-portable source test runner would constrain development without proving the distributed artifact.
- A repository-owned snapshot framework would duplicate Bun's focused workflow.
- Oxlint's incomplete type-aware rule coverage does not yet replace ESLint and `typescript-eslint`.
- Bun-compiled, bytecode, or Bun-targeted output would make Bun part of the runtime contract.
- Multiple runtime-specific bundles or a polyglot shebang would increase release complexity without user value.

## Consequences

- Contributors use Bun for replacement development and builds.
- Type checking, linting, tests, and bundling remain separate gates.
- TypeScript 6 exists only as a temporary lint API dependency.
- Runtime portability is exercised through the distributed artifact.
- Package metadata, the lockfile, configuration, scripts, and focused source
  are now authoritative for exact executable values.

## Current Source And Evidence

- [CLI Development Toolchain](../../documents/cli/development-toolchain.md)
- [CLI Architecture](../../documents/cli/architecture.md)
- [CLI Testing Architecture](cli-testing-architecture.md)
- [Archived CLI toolchain comparison](../../../archived/analysis/2026-07-30_cli-toolchain.md)
