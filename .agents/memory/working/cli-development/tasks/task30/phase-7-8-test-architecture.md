---
open-forge:
  description: Planned Task 30 boundary for layer-shaped test projects, useful-test review, and parallel-safe execution
  tags: [Memory, Working, CLI, Task, Subtask, Contextual, Testing, Architecture, Parallelism]
---

# Task 30 — Phases 7–8 Test Project Architecture And Parallel Execution

## Status

Complete. The maintained delivery path now runs all six test suites with
`--parallel collections`; the former serial default is gone. The Native AOT
qualification is a separate supported-Windows host gate that requires
`vswhere` on `PATH` and cannot be run by a sandboxed worker.

This is the test-architecture companion to the planned folder restructure by
layer and the later project split. It must be executed as one bounded design and
migration sequence, with each project boundary and deletion justified by the
test it leaves behind.

## Outcome And Profile

Make the active test system independently runnable and safely parallelizable.
Reshape test files to mirror the accepted production layers, create an
independent project boundary for each accepted layer or module boundary, and
keep complete process-smoke evidence at the delivered executable boundary.

Every end-to-end test must own a unique child directory under the operating
system temporary folder. The workspace, process current directory, mutable
application data, lock/recovery state, cache state, and other writable fixture
state must be rooted in that test directory. A suite-level directory may hold
only immutable inputs or build artifacts that are explicitly proven read-only;
the default is one isolated directory per test.

The task also audits every existing test for usefulness. A test remains only
when its behavior, contract, artifact, or owned effect is clear, its evidence
tier is the cheapest boundary that proves it, and it adds non-duplicated
confidence. Tests that only repeat lower-tier implementation detail, assert a
third-party behavior, or have no meaningful failure signal are moved, merged,
rewritten, or removed with the disposition recorded.

This is test/build architecture work. It must not change CLI behavior, public
contracts, persisted bytes, production semantics, or a test expectation merely
to make parallel execution convenient.

## Authority And Accepted Direction

- Maintainer direction recorded in this Task: split the test and source tree by
  layer, create independently runnable projects for those layers, make tests
  parallel-safe, isolate each E2E test under the OS temporary folder, and
  review whether every test is useful.
- [Task 30 phases 5–8](phase-5-8.md) owns the broader interaction, content,
  scenario, folder, and project sequence.
- [View Layer And Test Architecture](../../../../emerging/analysis/cli-experience-audit/view-layer-and-test-architecture.md)
  supplies the concern model: pure logic, facts, in-process behavior, and
  delivered artifact.
- [Test Layer Consolidation](../../../../emerging/analysis/cli-experience-audit/test-layer-consolidation.md)
  supplies the boundary between in-process behavior and the small process-smoke
  suite.
- [Layers And Sequencing](../../../../emerging/analysis/cli-experience-audit/layers-and-sequencing.md)
  supplies the prerequisite layer-shaped source tree and the project-split
  rationale. These analyses remain sealed provenance.
- [Test Evidence Integrity](../../../../../directives/open-forge/testing/evidence-integrity.md),
  [Evidence Tiers](../../../../../patterns/testing/evidence-tiers.md), and the CLI
  implementation directives remain binding.

## Current Baseline And Decision Frontier

The current repository has three runnable C# test projects and one shared test
support library. The native test runner supports `none`, `collections`, and
`all` parallel modes, and the delivery script selects `--parallel collections`
for all six suites. The qualified evidence therefore covers collection-level
parallel safety; `all` remains an available runner mode rather than the
delivery default.

Most tests already use unique `TemporaryWorkspace` roots. The published E2E
support still has a Windows path through the real user `%LOCALAPPDATA%` Open
Forge lock/recovery catalogue and protects only part of that state with a
process-local gate. That is the first isolation boundary to redesign. Do not
assume a static semaphore makes shared cross-test or cross-process state safe.

The exact number and names of the replacement projects remain a design gate.
They must follow the accepted layer/module graph and real consumers, not be
created merely because files happen to be numerous. A project split changes
references, build outputs, Native AOT configuration, CI selection, and possibly
the solution graph; those effects must be explicit before mutation.

## Decision — Execution Cost And Runtime Targets

Recorded 2026-09-15 from the maintainer's direction that the suite costs too
much time. Measured on the maintainer's machine, 12 logical cores, at
`220f51d9`.

### What was found

The Native AOT test population already exists and is already wired.
`scripts/delivery/layout.ts` defines six suites, not three: the managed `unit`,
`integration` and `public` assemblies; `native-integration` and `native-public`,
which publish the Integration and EndToEnd projects themselves as native
executables; and `public-native`, the managed EndToEnd project driving the
native CLI. `npm run test` selects the managed three, `npm run test:built`
selects all six, and `PublishedExecutableTarget.Discover()` routes EndToEnd to
the managed `open-forge-dev` artifact or the native publication by build RID.

None of this is named in any Directive or Task convention. The Task 30
conventions document three managed executables only, so every agent on this
Task has treated the "supported Native AOT gate" as an abstraction with no
command behind it. The gap was documentation, not capability.

### What actually costs the time

| Stage | Serial | `--parallel collections` | Share of the full gate |
| ----- | -----: | -----------------------: | ---------------------: |
| `npm run build` | 25-90 s | n/a | 9% |
| Unit, 3523 tests | 8.9 s | 3.8 s (`all`) | 1% |
| Integration, 2300 tests | 4 min 04 s | **1 min 33 s** | 24% |
| EndToEnd, 163 tests | 10 min 15 s | not measured | 59% |
| `npm run check:dotnet` | about 1 min | n/a | 6% |

The integration comparison selected, executed and reported identically: 2300
total, 2259 passed, the same 24 snapshot failures, 17 skips. One run is a data
point, not the Phase D qualification.

`runSuites` in `scripts/delivery/test-suites.ts` passes `--parallel collections`
for all six suites. The former serial argument was the dominant cost, and
EndToEnd was where it hurt most because every test spawns a process.

### Accepted

- Both runtime targets are first class and are one test population with two
  results. Neither substitutes for the other. Recorded in the CLI
  implementation Directive and the testing evidence Directive on 2026-09-15.
- Adding Native AOT coverage does not reduce cost and was never going to. It is
  worth running because it proves trimming, source-generated serialization and
  embedded-resource failures the managed target cannot reach. It belongs behind
  an explicit trigger, not in the inner loop.
- The cost fix is execution mode, not coverage. Do not reach for deleting,
  skipping or down-tiering tests to make the suite faster while the serial
  default is still in place.

### Superseded — Phase D qualified and the default changed

> Superseded on 2026-09-16 by Task 30 G4. The text below is kept as the record
> of what was true when written; it no longer describes the tree.

Changing the `--parallel none` default stays blocked on the Phase D
qualification already required by this record: repeated runs, `collections` and
`all`, identical selected/executed/failure/skip counts, and no dependence on
the real user profile. The EndToEnd `%LOCALAPPDATA%` path named under Current
Baseline is the specific blocker, and it is the suite with the most to gain.

**What actually happened.** The `%LOCALAPPDATA%` blocker was fixed in
`fe1ad1ae`: every suite redirects its own lock and recovery stores into a
temporary data home through `TestSupport/Isolation/TestDataHome.cs`, so no suite
touches the user profile. Phase D then qualified for all six suites, and
`scripts/delivery/test-suites.ts` now passes `--parallel collections`
unconditionally — there is no longer a per-suite choice.

Evidence, from
[40 verification](../task30-g4/40-verification.md): managed unit 3,179/0/0,
integration 2,219 total/0 failed/17 skipped, end-to-end 163/0/0; and the Native
AOT gate green with native integration 2,219/0/17 and native end-to-end 163/0/0,
identical to managed. Full end-to-end runs in about 2 minutes parallel against
about 9 minutes serial.

Until then, agents use targeted `--filter-class` selections in the inner loop
and the documented serial commands for a gate. An agent may use
`--parallel collections` for its own exploratory timing, and must not present
such a run as acceptance evidence.

## Execution Capsule

### Phase A — usefulness and evidence inventory

Inventory every active test declaration and fixture. Record, at minimum:

| Field             | Required decision                                                                             |
| ----------------- | --------------------------------------------------------------------------------------------- |
| Test identity     | Durable behavior/scenario name, feature, and evidence tier                                    |
| Question          | The Open Forge behavior, contract, artifact, or effect that can fail                          |
| Boundary          | Pure callable, real module/filesystem boundary, in-process CLI, published process, or package |
| Fixture ownership | Workspace, home, temp root, process, cache, port, and artifact ownership                      |
| Parallel state    | Read-only shared input, isolated mutable state, or an explicit synchronization need           |
| Disposition       | Keep, move, merge, rewrite, or remove, with replacement coverage when applicable              |

Keep malformed-input vectors, independent byte or serialization oracles, real OS
capability evidence, and safety assertions when they prove a distinct contract.
Do not delete a test solely because another test has a similar name or source
file.

### Phase B — layer and project map

Define the physical source and test map before moving files:

- the production layer folders and their dependency direction;
- one independently runnable test project for each accepted layer or module
  boundary that needs its own integration evidence;
- the pure Unit project(s) for direct, in-memory behavior;
- the in-process behavior/scenario project for argv-to-output journeys that do
  not require a child process;
- the small published process-smoke project for AOT, process, redirection,
  cancellation, relocation, and other genuinely black-box concerns;
- the nearest shared support scope for each fixture, with no generic utility
  bag, base class, fake filesystem, or forwarding type.

Project references must enforce the layer direction. Cross-layer composed
behavior belongs in the explicitly named behavior or process-smoke boundary,
not in every lower-layer project. The final map must state which current test
project, directory, and boundary each surviving test moves to.

### Phase C — per-test OS-temp E2E isolation

Replace shared profile state with a disposable test context that creates a
unique child of `Path.GetTempPath()` for every E2E test. Derive all physical
paths with .NET APIs and keep logical fixture paths relative to the owned root.

The context must isolate, as applicable:

- the test workspace and repository;
- process current directory and explicit workspace arguments;
- Windows local application data and equivalent Unix data-home locations;
- Open Forge locks, recovery bundles, catalogues, and caches;
- process environment, standard streams, cancellation, and child-process
  lifetime;
- mutable reports and test-specific outputs.

The published executable and immutable build artifacts may be shared only after
the task proves they are read-only. Cleanup must be ownership-aware and must
not recursively remove a broad system or repository path. Tests that truly
need a process-global resource require a narrow explicit collection/lock and a
written reason; a global gate is not the default fixture design.

### Phase D — parallel execution qualification

Build shared inputs once, then run independently isolated test projects with
the runner's `collections` and `all` modes. Do not parallelize competing builds
into one artifact root. Exercise repeated runs with bounded and unlimited
thread settings where supported, and compare selected, discovered, executed,
failure, skip, warning, and output-integrity counts.

The official delivery script now uses `--parallel collections` after repeated
qualification proved no cross-test contamination, flakiness, zero-test
selection, stale-artifact use, or hidden shared-state dependency. Keep suites
that cannot meet the isolation contract serialized and document why; do not
weaken their assertions.

### Phase E — layer integration coverage

For every accepted layer/module project, select representative integration
tests at that layer's real boundary. Move detailed branch coverage down to the
cheapest proving project, move composed CLI behavior to the in-process project,
and retain only the delivered-interface concerns in process smoke. Use the
existing write-freedom, AOT, stream, exit, and cancellation evidence where it
still proves a distinct boundary.

## Requirements

- Every active test has a recorded usefulness and disposition decision.
- Every surviving test has a durable selection identity and belongs to the
  project whose boundary it proves.
- Every mutable parallel fixture is owned by exactly one test or protected by a
  narrow, justified synchronization contract.
- Every E2E test uses a unique `Path.GetTempPath()` child for its workspace and
  mutable application state; no E2E test depends on the repository checkout or
  the user's real profile.
- Physical paths use .NET path APIs; logical workspace paths remain relative and
  slash-separated. No POSIX roots, `/private` prefixes, drive letters, or raw
  host separators are embedded in physical fixtures.
- Project references, solution membership, test filters, Native AOT settings,
  delivery scripts, and documentation agree with the final layer map.
- Test support is promoted only when at least two real consumers need identical
  semantics, and it remains outside production.
- No test is deleted, skipped, or weakened solely to obtain a green parallel
  run. OS-specific evidence either remains capability-probed and portable or
  records a precise platform boundary.
- The testing directives are updated with the accepted per-test temp-root,
  project-boundary, and parallel-execution rules after the implementation is
  proven.

## Evidence And Acceptance

Acceptance requires a tracked summary and fresh receipts for:

1. the complete test usefulness/disposition inventory and final layer/project
   mapping;
2. each independently runnable project producing nonzero selected and executed
   counts;
3. managed serialized, collection-parallel, and all-parallel qualification;
4. repeated E2E execution showing per-test temp roots and no shared profile,
   workspace, lock, recovery, cache, or output contamination;
5. affected managed and supported Native AOT/package boundaries after the
   project/build graph changes;
6. unchanged public behavior and preserved direct safety/byte assertions;
7. `git diff --check`, clean test output, and no required evidence left only in
   disposable artifacts.

The acceptance record must state which tests remain intentionally serialized,
why, and whether the delivery default changed. A passing parallel run is not
enough if it used stale artifacts, skipped required tests, or discovered zero
tests.

## Stop Conditions

- Stop before mutation if the proposed project graph cannot express the
  accepted dependency direction or requires an unaccepted production/API,
  package, runtime, or architecture change.
- Stop if a published process still resolves mutable state from the user's real
  profile or another shared location that cannot be redirected safely with
  standard .NET behavior.
- Stop if a test's meaning is unclear, if its only oracle is third-party
  implementation detail, or if deleting it would remove unique safety,
  capability, byte, interruption, or public-contract evidence.
- Stop if parallel execution exposes contamination, flaky results, stale
  artifacts, zero-test selections, or output changes whose source cannot be
  attributed to one test.
- Stop if isolation would require broad cleanup, hidden global mutation,
  timing sleeps, fake filesystem behavior, or a generic cross-layer fixture.

## Next Action

Run the read-only Phase A inventory and Phase B project-map preflight from the
current accepted branch. Do not change the runner default, move tests, split
projects, or delete tests until the inventory, isolation design, project graph,
and evidence ladder are recorded in this subtask.
