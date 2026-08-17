---
open-forge:
  description: Historical CLI-v2 source: Accepted historical replacement CLI status Task with phase progress, evidence, decisions, and acceptance state
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Status Task

## Archive State

Accepted on 2026-08-04 by explicit user disposition after final whole-Task
Acceptance identified only the tracked `GIT_TRACE` edge case. This record is
archived with its complete audit history. Active sequencing returns to the
[CLI replacement backlog](../working/cli-replacement-backlog.md).

## Outcome

Complete the replacement `status` command as one shallow, deterministic,
read-only operation with exact workspace, CLI, Framework, managed lifecycle,
Git, recovery, suggestion, semantic-result, and exhaustive redaction behavior.

## Authority

- [CLI replacement backlog](../working/cli-replacement-backlog.md#ordered-work)
- [CLI Architecture](../crystallized/documents/cli/architecture.md)
- [CLI Interface status contract](../crystallized/documents/cli/interface.md#status)
- [Status Result Contract](../crystallized/documents/cli/contracts/status-results.md)
- [Operation Prerequisites](../crystallized/documents/cli/contracts/operation-prerequisites.md)
- [Managed Lifecycle Contract](../crystallized/documents/cli/contracts/managed-lifecycle.md)
- [Development Workflow](../../workflows/development/_development.md)
- [CLI Interface Consistency Directive](../../directives/open-forge/cli/cli-interface-consistency.md)
- [TypeScript Source Structure Directive](../../directives/open-forge/typescript/typescript-source-structure.md)
- [Command Slice Pattern](../../patterns/open-forge/cli/commands/command-slice.md)
- [Predictable Command Surface Pattern](../../patterns/open-forge/cli/commands/predictable-command-surface.md)
- [Result And Display Boundary Pattern](../../patterns/open-forge/cli/commands/result-display-boundary.md)
- [CLI Tiered Test Slice Pattern](../../patterns/open-forge/cli/bun/tiered-test-slice.md)

## Baseline

- Source branch: `feature/cli-overhaul`.
- Immutable source commit: `9f1423ec3958a8665b8228a79f3c0bd36efa4e00`.
- Task branch: `agents/feature/cli-overhaul-status`.
- The source and Task branches were clean at isolation.
- Baseline `bun run check` passed with 50 direct, 8 integration, and 36 end-to-end tests.
- Root and `src/open-forge` frozen-MVP Doctor checks passed with no problems.

## Scope

### Allowed Surfaces

- A focused `src/cli/status/` command slice and the minimum shared typed support
  promoted to an existing nearest common scope.
- Existing root command registration, operation metadata, presentation, and
  build-asset seams required to expose the accepted command.
- Direct, integration, and built-process status evidence under `src/cli/`.
- Exact current documentation or generated snapshots only when implementation
  proves they must change to remain current.

### Forbidden Surfaces

- Route or local-reference inventory behavior, complete Doctor diagnosis,
  mutation planning or execution, and public behavior for another queued leaf.
- Frozen production or test behavior under `src/cli-mvp/`.
- Framework or Extension payload behavior under `src/open-forge/` or
  `src/extensions/`.
- Guessed lifecycle migration, inferred ownership, executable suggestions,
  broad filesystem scans, or best-effort string redaction.
- Task state, backlog state, commits, staging, or another phase's owned surface
  from a phase agent.

## Progress

| Stage      | State             | Commit               | Evidence                                                                                   |
| ---------- | ----------------- | -------------------- | ------------------------------------------------------------------------------------------ |
| Task setup | Complete          | `3118aaf`            | Branch isolated from clean `9f1423e`; indexed and formatted Task route; root Doctor passed |
| Contract   | Repeat 1 complete | `6b19d69`            | Named input contracts and accepted recovery-sidecar grammar compile and pass strict gates  |
| Red        | Repeat 3 complete | `53b7be7`            | Seven existing Git cases pass; two authorized edge cases fail at production behavior       |
| Green      | Repeat 2 complete | `fa5fc11`            | All nine focused Git cases and touched production checks pass                              |
| Blue       | Complete          | `a84170d`            | Status-local filesystem error handling is named once; all 225 tests remain green           |
| Purple     | Skipped           | None                 | User confirmed no material test-structure refactor remains                                 |
| Review     | Skipped           | None                 | User explicitly waived the independent phase Review                                        |
| Acceptance | Complete          | This closeout commit | User accepted the tracked Git trace edge after fresh whole-Task review                     |

## Decisions Needed

- None.

## Evidence

- Baseline: `bun run check` passed on the tree integrated as `9f1423e`.
- Baseline: `bun run cli:old -- doctor .` passed with no problems.
- Baseline: `bun run cli:old -- doctor ./src/open-forge` passed with no problems.
- Task setup: `bun run cli:old -- index .`, `bun run format:check`, root
  Doctor, and `git diff --check` passed.
- Contract: `bun run typecheck`, `bun run lint`, `bun run test`, `bun run build`,
  `bun run format:check`, root Doctor, and `git diff --check` passed. Built
  probes returned typed `cli.parse` for invalid status syntax and the explicit
  not-implemented boundary for a valid invocation.
- Current architecture: `6499560` makes named-value policy direct and starts
  expected multi-test subjects under one local `__tests__/` directory without
  tier subdirectories.
- Red return: typecheck, lint, formatting, and diff checks pass. Existing 50
  direct, 8 integration, and 36 end-to-end tests remain green; the organized
  Status evidence reports 12 direct, 12 integration, and one end-to-end failure
  at explicit missing behavior. Positive anchor, lifecycle, recovery, and Git
  expectations await production named owners from Contract repeat.
- Contract repeat 1: exact Framework anchor, managed-record, recovery-artifact,
  and relevant-Git-path values now have focused production owners. The accepted
  `.open-forge.next` and `.open-forge.previous` sidecars are distinct from
  `.bak`; typecheck, lint, formatting, root Doctor, and diff checks pass.
- Red repeat 1: 50 direct, 80 integration, and one built-process Status cases
  use production named values and the accepted `__tests__/` layout. Typecheck,
  lint, and formatting pass. Complete tiers retain 50 direct, 8 integration,
  and 36 end-to-end baseline passes; 13 direct, 80 integration, and one
  end-to-end case fail only at explicit missing behavior.
- Red setup correction: Bun's focused snapshot generator requires one trailing
  space in its multiline stream representation. `.gitattributes` exempts only
  generated `*.snap` files from `blank-at-eol`; authored files retain normal
  whitespace checks, and the regenerated snapshot changes no result content.
- Green: exact workspace, Framework, lifecycle, Git, recovery, suggestion,
  redaction, rendering, completion, and parser behavior passes `bun run check`:
  100 direct, 88 integration, and 37 end-to-end tests, 225 total.
- Blue: five duplicate `ENOENT` definitions and classifiers moved to one
  status-local named boundary; the same 225-test complete gate remains green.
- User acceptance: an ordinary nonempty `.agents/loader.md` is recognizable;
  symlinked anchor and managed-target ancestors may resolve outside the physical
  workspace; a valid managed record may suppress `install` when the Framework
  is uninstalled; and persisted exclusions may cover recorded Framework files.
- User acceptance: the adjacent `<target>.open-forge.next` and
  `<target>.open-forge.previous` residual sidecars remain settled, Purple is
  skipped for lack of material test-structure scope, and phase Review is waived.
- Whole-Task Acceptance: `USER DECISION`. `git status` is not protected by
  `--no-optional-locks` or equivalent read-only process control, and inherited
  repository-selection environment can make discovery report `not-repository`
  successfully instead of failing. Existing evidence does not cover either
  boundary.
- User authorization: a third Red execution may freeze both missing Git
  boundaries before the required Green correction and repeated Acceptance.
- Red repeat 2: four existing Git integration cases pass and three new cases
  fail only because inspection changes index bytes, honors malformed `GIT_DIR`,
  or follows inherited alternate-repository selection. Direct and end-to-end
  tiers, typecheck, lint, formatting, and diff checks remain green.
- Green repeat 1: every Git child disables optional locks and removes only the
  seven frozen repository-selection variables while preserving the remaining
  environment. The focused seven-case Git suite and complete `bun run check`
  pass with 100 direct, 91 integration, and 37 end-to-end tests, 228 total.
- Repeated whole-Task Acceptance: `USER DECISION`. Windows permits mixed-case
  spellings such as `git_dir`, while the current environment filter matches
  only uppercase keys. A malformed preserved Git configuration can also make
  repository discovery fail while the command reports successful
  `not-repository`. The reviewer separately identified one stale development
  guide statement that still describes `status` as unimplemented.
- User authorization: another Red and Green correction may address those two
  exact Git boundaries. Verification now targets Task evidence and other
  touched surfaces instead of automatically rerunning the complete repository
  suite.
- Red repeat 3: the focused Git integration file has seven existing passes and
  two expected failures. Lowercase `git_dir` bypasses Windows filtering, and
  malformed preserved Git configuration returns successful `not-repository`
  instead of the existing failed result and message. Touched-file typecheck,
  lint, formatting, and diff checks pass.
- Green repeat 2: Windows repository-selection variables are filtered
  case-insensitively while POSIX filtering remains exact. Git diagnostics use a
  stable locale so ordinary non-repositories remain distinct from failed
  probes. All nine focused Git cases, typecheck, and touched-file lint,
  formatting, and diff checks pass.
- Final whole-Task Acceptance: `USER DECISION`. With `GIT_TRACE=1`, preserved
  trace lines precede the ordinary non-repository diagnostic, so the current
  prefix classifier returns `failed` instead of `not-repository`. The reviewer
  found no other blocking issue and did not require broader verification.
- User disposition: preserve that external Git trace interaction in
  [Development Edge Cases](../../../docs/edge-cases.md) and stop expanding
  implementation scope for small arbitrary external-tool behaviors. This
  resolves the final Acceptance decision as an accepted residual risk.
- Required Task evidence: focused direct and integration Status suites, built
  `--json --redact` process evidence, and proportionate checks for every touched
  surface.

## Completion

- The exact accepted `status [--redact]` grammar and typed result are exposed
  without implementing another queued operation.
- Every named state, discriminated branch, count, semantic outcome, suggestion,
  no-suggestion case, shallow real-workspace boundary, and exhaustive redaction
  guarantee has executable evidence at its owning tier.
- Contract, Red, Green, Blue, explicit Purple and Review dispositions, and
  whole-Task Acceptance satisfy the accepted lifecycle with reviewable
  orchestrator-only commits.
- Required Task and touched-surface checks pass, no material finding remains,
  the accepted external-tool residual is recorded, the Task is archived, and
  the branch is ready for local squash into `feature/cli-overhaul` without
  pushing.
