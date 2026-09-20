---
open-forge:
  description: "Historical CLI-v2 source: Active Task for adding Development Preflight, scoped parallel Bun evidence, isolated test workspaces and builds, and faster phase verification"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Development Preflight And Test Isolation

## Outcome

Make the local Development Workflow faster and more reliable by concentrating
deep reasoning in read-only Preflight and Whole-Task Review, keeping middle
phases bounded, selecting Task evidence through native Bun paths and stable test
tags, and making replacement tests safe for file-level parallel execution.

## Authority

- [Development Workflow](../../workflows/development/_development.md)
- [Task Lifecycle](../../workflows/development/task-lifecycle.md)
- [Calibrated Agent Reasoning](../../guidance/calibrated-agent-reasoning.md)
- [CLI Tiered Test Slice](../../patterns/open-forge/cli/bun/tiered-test-slice.md)
- [CLI Development Toolchain](../crystallized/documents/cli/development-toolchain.md)
- [CLI Testing Architecture](../crystallized/decisions/cli/cli-testing-architecture.md)
- [Developing Open Forge](../../../docs/development.md)
- User direction in the active session for the phase matrix, correction budget,
  native Bun selection, stable tags, unique OS-temporary test workspaces, and
  APM exclusion.

## Baseline

- Branch: `agents/feature/cli-overhaul-development-preflight-test-isolation`
- Baseline branch: `feature/cli-overhaul`
- Baseline commit: `dc6005818780b4fcd30239af9b33bba69ef800c9`
- Worktree: `<workspace>/open-forge-worktrees/cli-overhaul-development-preflight-test-isolation`
- The separate route-inventory Task remains paused on its own continuation
  branch after Review blockers and is not part of this baseline.

## Dependencies

1. Freeze test-tag, temporary-workspace, isolated-build, native-runner, and
   `check:fast` boundaries. Complete in `e6c171a` and `510a2b9`.
2. Isolate replacement workspaces and process environments. Complete in
   `e571ef1` and `189dafe`.
3. Migrate replacement tests to compact durable tags without changing behavior.
   Complete in `8c92364` through `4f141bf`.
4. Replace custom discovery with native Bun selection and minimal build-first
   orchestration. Complete in `5fdf81a`.
5. Start the portable CPU-count file-worker pool immediately without enabling
   case concurrency. Complete in `21941e5`.
6. Align workflow, reasoning, testing, toolchain, generated routes, and public
   documentation. Complete through `6021bb7`.
7. Correct the three Red isolation and evidence-selection blockers returned by
   Whole-Task Review. Complete and independently verified.
8. Run the complete gate through independent Terra and Luna agents, then stop
   before integration for user review. Complete.

## Scope

Allowed:

- Local `.agents/workflows/development/` recipes and their routed entries.
- Applicable local Guidance, Pattern, current CLI toolchain and testing sources,
  public development documentation, and contextual workflow references.
- `package.json` replacement test and verification scripts.
- Replacement CLI build and test support under `src/cli/build/` and
  `src/cli/testing/`.
- Replacement direct, integration, and E2E test structure, fixtures, and
  snapshots only as required for compact durable tags and isolation.

Forbidden:

- `.apm/`, `.opencode/`, `.codex/`, `apm.yml`, `apm.lock.yaml`, and APM scripts.
- Fast or nano model variants.
- Generic shipped Development Toolkit behavior under `src/extensions/`.
- Frozen MVP production or evidence under `src/cli-mvp/` and `tests/`.
- Route inventory behavior from the separately paused Task.
- Provider-specific always-loaded Directives.

## Obligations

### Workflow

- Add read-only Phase 0 Preflight. It proposes populated Task splits, authority,
  obligations, matrices, edge classes, evidence, and material user decisions;
  the mastermind creates authoritative Tasks and Git state.
- Record the desired matrix: mastermind Sol/high, Preflight Sol/xhigh, Gray
  Terra/medium, Red Terra/medium, Green Terra/medium, Blue Terra/high, Purple
  Terra/high, and Whole-Task Review Sol/xhigh.
- Permit Preflight to escalate a middle phase to Sol/high for security,
  concurrency, filesystem safety, migration, external integration, public
  compatibility, or cross-domain architecture risk.
- Run Blue and Purple once total. One automatic corrective cycle may rerun Gray,
  Red, and Green. Whole-Task Review repeats after correction; a second material
  correction returns to the user.
- Phase agents use `check:fast` and scoped tagged tests only. The mastermind owns
  formatting, derived state, real-surface proof, the complete gate, and commits.

### Evidence Selection

- Every replacement test full name starts with exactly one stable `@unit`,
  `@integration`, or `@e2e` tag and immediately includes at least one durable
  subject tag such as `@status`. Runtime matrix cases may add `@node`, `@bun`,
  or `@deno`.
- Tags describe durable evidence scope, never a temporary Task identity.
- Native Bun owns path filtering, `--test-name-pattern`, and file-level
  `--parallel` execution.
- Replacement parallel scripts use bare `--parallel --parallel-delay=0`, so Bun
  resolves the portable CPU-count file-worker ceiling and starts every resolved
  worker immediately. Cases within a file remain sequential.
- Delete custom discovery when native Bun covers it. Retain only the minimum
  isolated build-first boundary required for E2E, all-suite, and one-target
  snapshot execution.

### Isolation And Speed

- Add one ordinary `createTestWorkspace()` function using a unique
  `mkdtemp(os.tmpdir())` root and deterministic idempotent cleanup.
- Every mutable workspace, home, cache, temporary directory, generated module,
  build artifact, package-manager state, and future port belongs to one test or
  one isolated runner invocation.
- Normal test execution never mutates snapshots. Focused unit and integration
  evidence does not build; E2E, all-suite, and complete checks serialize one
  explicit build before parallel test files run.
- Keep `check:fast`, `typecheck`, and `lint` generation-free. The complete
  `check` builds once before strict validation and prepared native tests.
- Measure discovery and focused tier timings. Investigate rather than accept a
  focused path whose cost is disproportionate to its boundary.

## Agent Matrix

| Stage             | Default      | Authority                                               |
| ----------------- | ------------ | ------------------------------------------------------- |
| Mastermind        | Sol/high     | Task state, orchestration, Git, formatting, final gates |
| Preflight         | Sol/xhigh    | Read-only Task blueprint and material decisions         |
| Gray              | Terra/medium | Contracts and compilable skeletons                      |
| Red               | Terra/medium | Tests, fixtures, snapshots, and failing evidence        |
| Green             | Terra/medium | Minimum production implementation                       |
| Blue              | Terra/high   | One production-only review and material refactor        |
| Purple            | Terra/high   | One test-only review and material refactor              |
| Whole-Task Review | Sol/xhigh    | Fresh read-only final assessment                        |

APM and runtime profile creation are handled separately and are not deliverables
of this Task. If an accepted full Sol or Terra agent is unavailable, pause rather
than substituting a fast or nano variant.

## Progress

This infrastructure Task used direct execution with proportionate evidence
because running the full phased Development ceremony to improve that ceremony
would not have added useful isolation. The table records factual milestones and
does not retroactively claim Gray, Red, Green, Blue, or Purple agent runs.

| Milestone                | State     | Commit(s)                   | Evidence                                                                |
| ------------------------ | --------- | --------------------------- | ----------------------------------------------------------------------- |
| Task setup               | Complete  | `42554e8`                   | Dedicated worktree from `dc60058`; excluded state preserved             |
| Fast verification        | Complete  | `e6c171a`, `510a2b9`        | Import-safe generation and a generation-free strict check path          |
| Workspace isolation      | Complete  | `e571ef1`, `189dafe`        | Unique OS-temporary roots and isolated process environments             |
| Compact test tags        | Complete  | `8c92364` through `4f141bf` | All 62 replacement test files use compact durable tags                  |
| Native Bun selection     | Complete  | `5fdf81a`                   | Native paths, name patterns, and file-level parallel execution          |
| Immediate worker startup | Complete  | `21941e5`                   | Portable CPU-count pool starts without Bun's default ramp               |
| Documentation alignment  | Complete  | `5747fb8` through `6021bb7` | Workflow, testing, toolchain, public docs, and generated Entries        |
| Isolation correction     | Complete  | `14ecc5e` through `08a2e18` | Generator, updater, package state, E2E callers, and Windows tag fixed   |
| Correction documentation | Complete  | `806dc77`, `39142cf`        | Directive, Decision, Pattern, toolchain, and public guide aligned       |
| Whole-Task Review        | Corrected | None                        | The three initial Red blockers are corrected and independently verified |
| Acceptance               | Complete  | None                        | Terra and Luna gates pass; integration is paused for user review        |

## Correction Budget

- State: `consumed and closed` by the verified Red correction after Whole-Task
  Review.
- One material correction may rerun only the earliest invalidated Gray, Red, and
  Green work.
- Blue and Purple do not repeat.
- Whole-Task Review repeats after correction.
- Environment-only retries that change no tracked source do not consume the
  budget.
- The correction must isolate snapshot/build evidence, isolate every npm/npx E2E
  invocation, and restore the omitted Windows integration case to native tier
  selection. A second material correction returns to the user.

## Decisions Needed

The user accepted the phase matrix, one-run Blue and Purple, native Bun
selection, compact durable test tags, unique OS-temporary workspace function,
and APM exclusion. The user later directed prerequisite closeout to stop for
review instead of integrating or resuming the paused main Task. Nothing is
pushed.

## Evidence

- Baseline `check:fast` passes in 9.33 seconds after running `build:assets`
  twice through the composed `typecheck` and `lint` scripts.
- The optimized `check:fast` passed in 6.75 seconds without implicit asset
  generation.
- The initial implementation checkpoint passed 100 unit, 92 integration, and 37
  E2E tests. Status-only parallel evidence passed 135 tests across 39 files;
  adding the built status E2E case passed 136 tests across 40 files. These
  reproduced counts supersede an earlier session summary whose 147-test
  aggregate did not retain its exact command provenance.
- Maximum-portable startup Red failed only because four scripts omitted
  `--parallel-delay=0`; focused Green then passed. Fresh post-change evidence
  passed `check:fast`, 100 unit, 93 integration, 37 E2E, and 231 complete native
  tests across 62 files. Bun reported `12x PARALLEL` on 12 available processors.
- The built absolute artifact ran from a separate OS-temporary working directory
  and returned a pure successful `cli.version` JSON result for version `0.0.0`.
- Fresh Whole-Task Review blocked acceptance at Red because the prepared parallel
  suite can rebuild shared `dist/` and rewrite shared generated assets, parallel
  E2E npm/npx processes inherit shared mutable state, and one Windows
  `test.skipIf` case lacks its `@integration @status` prefix.
- Targeted indexing refreshed the Testing Directive, Development Workflow, and
  Preflight routes. Both routing Doctors pass. The frozen MVP indexer removes
  Markdown list spacing that repository Prettier restores, so semantic Doctor
  validation replaces a false byte-idempotence claim for those generated blocks.
- The mastermind still formats exact Task paths, updates derived routing, and
  runs one isolated built CLI scenario before Whole-Task Review.
- After Review passes, `bun run check`, applicable Doctors, index idempotence,
  and diff hygiene run once as the complete read-only gate.
- The correction moved six updater-file tests out of ordinary parallel
  selection and added three parallel-safe script and package-process tests. The
  resulting prepared count is therefore 228 rather than the earlier 231; all
  three independent Luna stress runs and the final `check` reported 228 tests
  across 63 files with 726 assertions.
- Independent Terra and Luna agents both passed formatting, strict TypeScript
  and ESLint checks, focused generator/script/package-process/Windows/updater
  evidence, one canonical build, three prepared-suite stress passes, full
  `check`, both Doctors, and Git diff hygiene.
- The Luna stress passes completed in 58.52, 56.69, and 55.50 seconds. The final
  `check` suite completed in 59.83 seconds. `dist/cli.mjs` remained
  `8eccc7d19eb30d51b169bc8d612838f8634627d1dde8b5a2aab78d7a245acc0d`,
  and `.temp/cli/embedded-assets.generated.ts` remained
  `808320847d1d1e072ad6b5a4c8e9843af8b6a122374eaee011686ef49b3e7dc8`
  across every stress sample.
- The branch is review-ready. No local integration, push, route-inventory
  restart, or main-Task workflow execution is authorized before user direction.

## Completion

- The local Development Workflow exposes Preflight, the accepted phase matrix,
  one correction budget, one Blue, one Purple, and one Whole-Task Review.
- Replacement tests have compact tier and durable subject tags and pass with
  file-level parallel execution.
- Mutable test and build state is isolated per test or runner invocation.
- Native Bun owns discovery and selection; custom orchestration is minimal and
  evidence-backed.
- `check:fast` validates an already-prepared workspace; the complete gate builds
  once before static validation and prepared native tests.
- Current documentation, generated routes, and public development guidance agree.
- No APM, OpenCode, Codex, fast/nano profile, generic Extension, frozen MVP, or
  paused route-inventory behavior enters the diff.
