---
open-forge:
  description: Historical mutable transfer log for the completed Development Preflight and test isolation work
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Development Preflight And Test Isolation Transfer Log

Archived because this record predates the sealed Handoff contract and accumulated state across many transfers. The [active Task](../working/development-preflight-test-isolation.md) contains the current review-ready state. The remaining content is preserved as historical transfer evidence and does not govern current Handoff behavior.

## Transfer

- Sender: Sisyphus mastermind.
- Receiver: the user reviewing the completed prerequisite branch.
- The receiver reads this file before acting. Sisyphus records the returned
  result here before another subagent transfer when the receiver is read-only.

## Goal

Close the active infrastructure Task, keep every subagent transfer mediated by
Working Memory handoffs under the existing Handoff Axioms, and configure native
Bun test files to use the maximum safe machine parallelism without enabling
unproved case concurrency.

## Authority

- [Active Task](../working/development-preflight-test-isolation.md)
- [Testing Directive](../../directives/open-forge/testing/bun/cli/cli-testing.md)
- [Development Workflow](../../workflows/development/_development.md)
- [CLI Tiered Test Slice](../../patterns/open-forge/cli/bun/tiered-test-slice.md)
- [CLI Development Toolchain](../crystallized/documents/cli/development-toolchain.md)
- User direction in the active session: do not add a handover Directive; rely on
  normal Axioms, always use hard-written handoffs, and maximize test
  parallelization.

## Task-Scoped User Rules

- Complete testing infrastructure and documentation prerequisites before using
  the Development Workflow or resuming the paused main Task. This temporary
  restriction expires only after user review and restart direction.
- Full targeted Terra and Luna agents are allowed; fast and nano variants are
  prohibited. A separate Luna agent runs the complete final test contract.
- Working Memory and handoffs need not be committed after every transfer; commit
  them when they form a useful review or resume boundary.
- Git commits use one plain repository-style subject only. Do not add a body,
  footer, coauthor, attribution, generated-by marker, or unrelated project link.
- Keep the external `.gitignore` change unstaged and out of prerequisite
  history. Do not push.
- After prerequisites pass, stop for user review. Do not integrate, resume the
  route-inventory Task, or start another workflow without user direction.
- Do not add a handover Directive. Use the existing Handoff Axioms and explicit
  written handoffs.

## Baseline And State

- Branch: `agents/feature/cli-overhaul-development-preflight-test-isolation`.
- Baseline: `dc6005818780b4fcd30239af9b33bba69ef800c9`.
- All accepted prerequisite implementation, documentation, Task, and handoff
  content is committed before the requested final history squash.
- Fresh evidence passes 100 unit, 92 integration, 37 E2E, 135 status-only
  parallel tests across 39 files, and 136 tests across 40 files when the built
  status E2E case is included.
- The active Task and this handoff contain the final unstaged acceptance record.
  `.gitignore` is an external unstaged change and remains excluded.
- The current native Bun scripts use `--parallel` and report `12x PARALLEL` on
  this machine. The exact supported mechanism for explicitly maximizing worker
  concurrency still requires current Bun documentation and implementation
  inspection.

## Allowed

- `package.json` test and verification scripts.
- Focused replacement test-tooling evidence when required before script changes.
- Current testing Directive, Pattern, toolchain document, public development
  guide, active Task, this handoff, and generated Entries affected by those
  routed files.

## Forbidden

- Case-level concurrency without isolation proof.
- Fast or nano model variants.
- `.gitignore`, APM, OpenCode, Codex, frozen MVP behavior, generic shipped
  Extension behavior, and paused route-inventory implementation.
- Pushes.

## Next Action

Review the completed prerequisite changes and direct any correction or restart.
Do not integrate, push, or resume the paused route-inventory Task without new
user direction.

## Returned State

- Installed Bun 1.3.14 help states that `--parallel[=<N>]` distributes test
  files across worker processes, implies isolation, and defaults to the CPU core
  count. This machine exposes 12 available and 12 logical processors, matching
  the observed `12x PARALLEL` output.
- `--parallel-delay` controls worker startup and defaults to 5 milliseconds;
  `--parallel-delay=0` spawns every available file worker immediately.
- Official Bun documentation distinguishes file-worker `--parallel` from
  case-level `--concurrent`. `--max-concurrency` limits concurrent test cases
  and does not increase file-worker parallelism.
- Current recommendation: retain portable core-count discovery through bare
  `--parallel`, add `--parallel-delay=0` to each replacement parallel test
  command, and do not hardcode this machine's worker count or enable
  `--concurrent`.
- External librarian verification confirmed from Bun 1.3.14 source that bare
  `--parallel` resolves to the runtime CPU/thread count, workers are capped by
  discovered file count, and `12x PARALLEL` is the resolved file-worker ceiling
  on this machine.
- The librarian recommends no worker-count override because bare `--parallel`
  is already the maximum portable ceiling. It confirms that
  `--parallel-delay=0` is supported but changes immediate worker startup rather
  than the ceiling.
- `--max-concurrency` applies only to case-level concurrency with
  `--concurrent` or `test.concurrent`; neither belongs in this change.
- Official evidence:
  [CLI declaration](https://github.com/oven-sh/bun/blob/0d9b296af33f2b851fcbf4df3e9ec89751734ba4/src/cli/Arguments.zig#L247-L252),
  [worker resolution](https://github.com/oven-sh/bun/blob/0d9b296af33f2b851fcbf4df3e9ec89751734ba4/src/cli/Arguments.zig#L662-L676),
  [file-count cap](https://github.com/oven-sh/bun/blob/0d9b296af33f2b851fcbf4df3e9ec89751734ba4/src/cli/test/parallel/runner.zig#L21-L30),
  and [parallel documentation](https://bun.com/docs/test/parallel).
- Next transfer: determine whether the user's explicit maximum-parallelization
  direction warrants `--parallel-delay=0` as the only meaningful safe change,
  and define its Red evidence and documentation boundary.
- The Sol planning transfer selected `--parallel-delay=0`. A no-op would retain
  the maximum portable worker ceiling but would not enact the user's explicit
  request because Bun would keep its 5 millisecond worker ramp.
- Red evidence adds one independent package-script contract test in the existing
  focused snapshot-update integration surface. It requires exact values for
  `test`, `test:integration`, `test:e2e`, and `test:all:prepared`, preserving
  bare `--parallel` and appending `--parallel-delay=0`.
- Green changes only those four `package.json` scripts. Fixed worker counts,
  `--concurrent`, `--max-concurrency`, dependencies, lockfiles, snapshot
  scripts, and frozen-MVP scripts remain unchanged.
- The implementation receiver returns the failing Red command and reason,
  passing focused Green evidence, `check:fast`, changed paths, and any blocker.
  It must not edit documentation, Task state, this handoff, stage, commit, or
  push.
- The full Terra implementation transfer changed only `package.json` and
  `src/cli/testing/update-snapshot.integration.test.ts`.
- Red failed solely because the four replacement scripts lacked
  `--parallel-delay=0`. Green passed the exact selected package-script contract
  test: 1 pass, 5 filtered, 0 failures.
- Independent mastermind reproduction passed the same selected test in 42
  milliseconds and passed `check:fast` through strict production/test
  typechecking and ESLint.
- The documentation receiver must edit only the five named documentation files,
  preserve their distinct responsibilities, format exact changed paths, and
  return changed paths plus any semantic conflict. It does not stage, commit,
  push, edit the Task, or edit this handoff.
- The Sol documentation transfer changed exactly the Testing Directive, CLI
  Testing Architecture Decision, CLI Development Toolchain, Tiered Test Slice
  Pattern, and public development guide.
- Each source now preserves its role while documenting bare
  `--parallel --parallel-delay=0`, portable CPU-count file workers, immediate
  startup, sequential cases, build-once flows, and serial snapshot mutation.
- Exact-path Prettier write and check passed for all five files. Independent
  mastermind full-file inspection and `git diff --check` found no conflict or
  unauthorized path.
- No next subagent transfer is authorized until the mastermind records fresh
  unit, integration, E2E, complete native, static, Doctor, and built-surface
  evidence in this file.
- Maximum-parallelism commits are `21941e5`, `34a3872`, and `6021bb7`.
- Fresh script execution reported `12x PARALLEL` and passed 100 unit, 93
  integration, and 37 E2E tests. `test:all:prepared` passed 231 tests across 62
  files with 702 assertions.
- `check:fast` passed strict production/test typechecking and ESLint. Exact-path
  formatting and diff whitespace checks passed. The two routing Doctors passed
  before this maximum-parallelism body-only update; no route metadata changed.
- The absolute built `dist/cli.mjs` ran from a separate OS-temporary working
  directory and returned only
  `{"schemaVersion":1,"operation":"cli.version","status":"success","messages":[],"data":{"name":"open-forge","version":"0.0.0"}}`.
- The external `.gitignore` modification remains unstaged and excluded. No APM,
  OpenCode, Codex, frozen-MVP behavior, generic Extension behavior,
  route-inventory implementation, or push entered the change.
- The reviewer must read this handoff first, remain read-only, use full Sol/xhigh
  rather than a fast or nano variant, and identify the earliest invalidated
  Gray, Red, or Green boundary for every blocking finding. The final accepted
  gate runs only after Review reports no blocking finding.
- Whole-Task Review used full Sol and returned three blocking Red findings:
  1. `test:all:prepared` includes snapshot-update evidence that rebuilds shared
     `dist/` and generated-module evidence that rewrites shared `.temp/` while
     other file workers run.
  2. Parallel E2E npm/npx invocations inherit shared user cache, logs, temporary
     directories, and package-manager configuration instead of a unique
     `createTestWorkspace()` environment.
  3. The Windows lowercase Git-environment `test.skipIf` case has no
     `@integration @status` prefix, so the integration tier filters it out and
     explains the 230 tagged versus 231 prepared count.
- The authoritative Task records the single correction budget as consumed at
  Red. Blue and Purple do not repeat. Whole-Task Review and the final gate repeat
  after correction; a second material correction returns to the user.
- The planning receiver must inspect every cited caller and shared helper before
  proposing changes. It remains read-only, runs no build-producing command, and
  writes no file or Git state.
- The first correction plan proposes:
  - Exclude `update-snapshot.integration.test.ts` from ordinary integration and
    prepared parallel scripts, move package-script assertions into a new
    parallel-safe test, and keep updater evidence explicitly serial.
  - Refactor `generate-build-module.integration.test.ts` to invoke the existing
    parameterized `generateBuildModule()` API against a unique mirrored test
    workspace instead of repository `.temp/`.
  - Add a nearest-shared E2E package-process helper that creates one
    `createTestWorkspace()` environment per npm/npx call, prove cleanup, and
    route the complete npm/npx caller inventory through it.
  - Add the missing `@integration @status` prefix and align five testing
    documentation roles.
  - Stress the corrected prepared suite three times while proving stable
    repository artifact hashes and absent parent package-manager state.
- TDD refinement required before transfer: importing a nonexistent helper is an
  invalid Red failure, and the generator refactor needs persistent evidence that
  would fail if repository-target generation were restored. The same planner
  must resolve both seams before any Red or Green mutation begins.
- The refined plan resolves both seams:
  - Generator Red uses the final isolated fixture and unchanged state assertions
    while retaining the current repository-target script invocation. It fails
    because the required workspace-local output is absent. Green changes only
    the invocation to the existing parameterized `generateBuildModule()` API.
  - Package-process Red may add a minimal compiling final-interface skeleton
    that delegates directly to `runProcess()` without isolation. Real probes
    then fail because they inherit parent paths. Green changes only the helper
    body/imports to create and clean one workspace per invocation.
  - Ordinary-script Red adds a new package-script contract requiring
    `--path-ignore-patterns="**/update-snapshot.integration.test.ts"` in only
    `test:integration` and `test:all:prepared`.
- Exclusive Red path ownership:
  1. Generator receiver: `src/cli/build/generate-build-module.integration.test.ts`.
  2. Script receiver: new `src/cli/testing/package-scripts.integration.test.ts`.
  3. Package-process receiver: new `src/cli/e2e/package-manager-process.ts` and
     new `src/cli/e2e/package-manager-process.integration.test.ts`.
- Windows selection Red is already captured: the required tagged regex matched
  zero tests with a nonzero command result, while the old untagged exact name
  selected one passing case with eight filtered tests.
- Generator Red used its final unique-workspace fixture and failed with `ENOENT`
  reading the required workspace-local generated module because the retained
  script invocation still wrote repository `.temp/`.
- Ordinary-script Red failed only on the two missing updater ignore flags, but
  `check:fast` found one setup defect: its validated
  `Record<string, unknown>` was returned as `Record<string, string>` without a
  typed accumulator. This must be corrected before Red acceptance.
- Package-process Red compiled and failed because overlapping real probes both
  inherited `HOME=C:\Users\Tedy`. Its minimal helper skeleton exposes the final
  interface and delegates directly to `runProcess()` without isolation.
- No Green change, stage, commit, snapshot mutation, retry, or full gate ran.
- Green now passes for isolated generator output, ordinary-script exclusion of
  the serial updater, the serial updater's four exact-path cases, the package
  script contract, the package-process helper itself, and the Windows tagged
  case.
- A real four-file maximum-parallel E2E run exposed the remaining design issue:
  per-invocation fresh npm/npx caches force repeated cold installs. Multiple
  package operations hit Bun's 5-second default timeout, killed children
  returned `-1`, one Foundation cleanup reported `EBUSY`, and the command
  exceeded 120 seconds while the first minimum-runtime case took 69.5 seconds.
- Accepted correction: package-manager mutable state belongs to one parallel
  test-file worker. Cases in that file remain sequential and share only that
  file's unique cache. This preserves cross-file isolation and practical cold
  package installation.
- Exclusive implementation ownership:
  1. Runtime agent: package-manager helper and test,
     `supported-runtime-fixture.ts`, `foundation.e2e.test.ts`, and
     `final-stream-failure.e2e.test.ts`.
  2. Package-boundary agent: `minimum-runtimes.e2e.test.ts` and
     `distribution.e2e.test.ts`.
- No handoff or Working Memory commit is required until both implementations,
  documentation alignment, and independent QA complete.
- Two full Terra implementation agents completed exclusive lanes without Git
  mutation. The runtime lane changed the package helper contract, its evidence,
  supported-runtime fixture, Foundation, and stream-safety files. The package
  boundary lane changed only minimum-runtime and distribution files.
- Sisyphus read every changed file, removed the obsolete per-invocation `label`
  parameter, formatted exact paths, and passed `check:fast`.
- One build followed by the four affected E2E files under
  `--parallel --parallel-delay=0` passed 35 tests across 4 files with 223
  assertions in 55.33 seconds. Cold Node, Bun, Deno, npm packaging, and stream
  failure cases all passed; no killed child, timeout, or cleanup error remained.
- Documentation receiver owns exactly `.agents/directives/testing.md`,
  `.agents/memory/crystallized/decisions/cli-testing-architecture.md`,
  `.agents/memory/crystallized/documents/cli/development-toolchain.md`,
  `.agents/patterns/open-forge/cli/tiered-test-slice.md`, and
  `docs/development.md`. It does not edit implementation, Task/handoff/journal,
  generated Entries, Git state, or any other path.
- A full Terra documentation agent changed exactly the five authorized sources.
  Sisyphus read all five and confirmed they match package scripts and the
  file-worker-owned package environment design.
- QA must run, in order:
  1. `bun run format:check` and `bun run check:fast`.
  2. Focused generator, package-script, package-process, tagged Windows, and
     explicit serial updater tests.
  3. `bun run build`, then hash `dist/cli.mjs` and
     `.temp/cli/embedded-assets.generated.ts`.
  4. Three sequential `bun run test:all:prepared` passes, requiring unchanged
     hashes after each.
  5. `bun run check`.
  6. Root and `src/open-forge` Doctors, `git diff --check`, and final status.
- QA may write only ordinary generated build output through accepted build/test
  commands. It must not edit authored files, update snapshots, stage, commit,
  push, or touch `.gitignore`, Task, handoff, journal, or main-task state.
- Independent Terra QA passed every focused test, three prepared-suite stress
  runs, full `check`, both Doctors, stable artifact hashes, and Git diff hygiene.
- The separately required Luna QA independently passed the same contract. Its
  three prepared runs reported 228 tests across 63 files with 726 assertions in
  58.52, 56.69, and 55.50 seconds; final `check` reported the same count in
  59.83 seconds.
- The stable 228 count is expected: six updater-file tests left ordinary
  parallel selection and three parallel-safe script/package-process tests were
  added, changing the earlier 231 count by minus three.
- Across every Luna stress sample, `dist/cli.mjs` remained
  `8eccc7d19eb30d51b169bc8d612838f8634627d1dde8b5a2aab78d7a245acc0d`
  and `.temp/cli/embedded-assets.generated.ts` remained
  `808320847d1d1e072ad6b5a4c8e9843af8b6a122374eaee011686ef49b3e7dc8`.
- The temporary debugging journal and its local `.git/info/exclude` entry were
  removed after evidence was recorded. The external `.gitignore` modification
  remains untouched and excluded from prerequisite commits.
- User direction supersedes the earlier local-integration completion step: stop
  for review after prerequisite commits, with no push or main-task restart.
- Final branch history is intentionally squashed to one local prerequisite
  commit from the recorded baseline, with the external `.gitignore` change
  preserved outside that commit.
