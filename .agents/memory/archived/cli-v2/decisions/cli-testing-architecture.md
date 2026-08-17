---
open-forge:
  description: Historical CLI-v2 source: Replacement CLI tests progress from direct function and command evidence to focused integration and built-process journeys
  responsibility: Preserve why the replacement uses a gradual evidence ladder, focused snapshots, real boundaries, and a small built-process suite
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Testing Architecture

## Context

The MVP runner recognizes only `unit` and `closure`. One small unit file forms
the fast tier, while most behavior lives in closure tests, including one file
of approximately 153 KB. A local Windows measurement on 2026-07-30 took about
0.29 seconds for the fast tier and 95.52 seconds for closure.

The replacement needs fast evidence near a failure's cause, independently
testable commands, focused proof at real system boundaries, and complete proof
through the distributed executable. It also needs shared support without one
large fixture or utility layer.

## Decision

The replacement uses an increasing evidence ladder:

1. Direct tests call one exported function, cohesive pure module, named handler,
   or in-memory command registration boundary.
2. Integration tests cross one selected real boundary, such as filesystem,
   Git, process, or package layout.
3. End-to-end tests invoke the built CLI in an isolated operating-system
   temporary workspace for a small set of complete critical journeys.

Most behavior is proved without spawning the CLI process. End-to-end evidence
proves dispatch, rendering, process completion, packaging, runtime portability,
and complete journeys instead of becoming the default home for command logic.

Tests prefer real functions, value objects, filesystems, Git repositories,
processes, and resulting state. A test double is justified only by a specific
boundary or failure that cannot be exercised safely and deterministically with
the real implementation. Assertions prefer returned values and observable
state over collaborator-call observations.

Snapshots have two deliberate boundaries. Direct and integration evidence may
snapshot one focused typed value or local state projection when complete review
is clearer than several assertions. Built-process snapshots retain only stable
public process evidence. Safety invariants remain explicit assertions when they
could disappear inside a snapshot diff.

Replacement tests follow production locality. Shared workspace, process, Git,
fixture, and normalization support moves only to the nearest common scope of
demonstrated consumers. The frozen MVP keeps its existing evidence and supplies
only independently desired scenarios; replacement tests do not assert backward
compatibility.

One production subject expected to remain a one-test subject keeps that test
adjacent. A subject that has or is expected to need several test files starts
with its nearest `__tests__/` directory and splits filenames by production
behavior. Tier suffixes and runners already distinguish direct, integration,
and end-to-end evidence, so additional tier directories would duplicate
identity and fragment one subject's review surface.

Bun hosts the test suite, while portability is exercised at the exported
artifact boundary under Node.js, Bun, and Deno. The accepted [CLI
Architecture](../../documents/cli/architecture.md#testing-and-toolchain) owns the
current verification relationship. The [CLI Tiered Test
Slice](../../../../patterns/open-forge/cli/bun/tiered-test-slice.md) owns filenames,
script shape, snapshot projections, and evidence placement. `package.json`, the
lockfile, configuration, scripts, and focused source are authoritative for
exact executable values. The [Development
Toolchain](../../documents/cli/development-toolchain.md) defines their semantic
relationship and compatibility obligations. The [toolchain
Decision](cli-development-toolchain.md) preserves the rationale for that
configuration.

Replacement test names begin with one tier tag, `@unit`, `@integration`, or
`@e2e`, and a durable subject tag. Runtime cases may add `@node`, `@bun`, or
`@deno`. Native Bun path filtering and `--test-name-pattern` provide focused
selection. Replacement parallel scripts use bare
`--parallel --parallel-delay=0`. Bun therefore resolves the portable CPU-count
ceiling for file workers and starts every resolved worker immediately. This
parallelizes isolated test files, not cases within a file. Cases remain
sequential unless their concurrency is separately proved safe. Each mutable
workspace, home, temporary directory, cache, build output, and port belongs to
one test or invocation. A parallel end-to-end file worker owns one unique
package-manager environment and cache, isolated from other workers and shared
only by that file's sequential cases. Filesystem and process tests use
`createTestWorkspace()`, while end-to-end tests build once before parallel files
run. Ordinary integration and prepared-suite selections exclude
`update-snapshot.integration.test.ts`; updater evidence is selected explicitly
by its exact path and runs serially.

During design-only work, structural validation and focused inexpensive checks
are sufficient. The complete existing suite is recorded once at the transition
to implementation. Process-level evidence then runs at affected integration
milestones and final verification rather than after every local edit.

## Rationale

Direct function and command evidence keeps the main development loop fast and
localizes failures. Focused integration evidence preserves operating-system
confidence without paying process startup and workspace setup for every branch.
A small built-process suite proves what users receive.

Bare `--parallel` adapts the file-worker ceiling to the machine instead of
encoding one machine's CPU count. Removing Bun's worker startup delay lets the
resolved pool begin work immediately without expanding concurrency into test
cases, whose isolation has not been proved.

The snapshot updater rebuilds shared output, so including it in ordinary
parallel integration or prepared evidence would make that mutation contend with
other workers. Its explicit exact-path serial evidence preserves coverage
without changing build-once flows or the one-target snapshot-update contract.
Package-manager installation needs mutable cache and environment state, but a
fresh environment for every sequential case repeats cold installation work.
Owning that state at the parallel file-worker boundary keeps workers isolated
while allowing the cases already kept sequential to reuse their file-local
environment and cache.

Focused snapshot boundaries make broad public changes reviewable without
turning volatile setup and internal execution evidence into approval noise.
Real boundaries keep the suite coupled to observable behavior rather than a
second mocked implementation.

## Rejected Alternatives

- Keeping only MVP-style `unit` and `closure` tiers would preserve the current
  feedback imbalance and oversized closure files.
- Testing most behavior through the executable would make failures slower and
  harder to localize.
- Requiring the source test runner itself to execute under every supported
  runtime would conflate development tooling with exported-package
  compatibility.
- Interaction mocks and universal fake adapters would add test architecture
  without proving the real boundaries that carry the risk.
- Separate `unit/` and `integration/` test directories would repeat the tier
  already encoded by filenames and runners while scattering one production
  subject's evidence.
- A universal snapshot wrapper would repeat inputs, fixtures, timings, and
  unrelated state already owned by each test.
- A fixed file-worker count would encode one machine's CPU topology instead of
  preserving Bun's portable CPU-count resolution.
- Bun's default worker startup delay would preserve the same file-worker ceiling
  but ramp workers instead of starting the resolved pool immediately.
- Case-level parallelism would exceed the current isolation evidence and could
  make cases within one file contend for state.
- A package-manager environment and cache per invocation would preserve
  isolation but repeat cold installation work for every sequential case.
- Running the snapshot updater in ordinary parallel integration or prepared
  selections would allow its shared-output mutation to race other workers.

## Consequences

- Replacement modules expose intentional callable boundaries.
- Command parsing, handlers, application behavior, rendering, and process
  completion remain independently testable.
- Integration tests identify the one real boundary they cross.
- End-to-end tests remain few, complete, and built-artifact based.
- Test infrastructure follows the same locality rules as production source.
- Runtime portability remains an exercised property of the distributed
  artifact.
- Coverage numbers may reveal gaps, but risk and contract evidence determine
  required cases.

## Open Details

- Whether representative pure modules justify a property-testing library.
- Initial performance budgets and the environments allowed to enforce them.

## Evidence And Relationships

- [Open Forge CLI MVP Architecture](../../documents/cli/mvp-architecture.md)
- [CLI direct replacement development](cli-direct-replacement-development.md)
- [Agent-first CLI product contract](cli-agent-first-product-contract.md)
- [CLI command framework](cli-command-framework.md)
- [CLI source locality](cli-source-locality.md)
