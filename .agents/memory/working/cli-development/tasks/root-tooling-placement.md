---
open-forge:
  description: Place live repository-owned agent tooling in explicit source scopes and remove the unexplained root scripts bucket
  tags: [Memory, Working, Contextual, CLI, Task, Tooling, TypeScript, Testing]
---

# Task 11: Root Tooling Placement Remediation

## Task State

- State: Complete, squash-integrated, and dequeued after the ledger's
  completion-update grace.
- Permanent mapping: Task 11 “Root Tooling Placement Remediation” in the
  [project control ledger](../project-control.md).
- Profile: Standard; maintainer-selected simplified single-owner flow with one
  fresh whole-task review.
- Review budget: maximum 1; `T11-R1` consumed by the M5 fresh whole-task
  review.
- Correction budget: maximum 1; `T11-C1` consumed by the M6 grouped
  correction.
- Council budget: 0.
- Completed owner: Root Tooling Placement Task Mastermind, one Brilliant
  Implementer, and one fresh reviewer.
- Worktree: `open-forge-worktree/root-tooling-placement` on branch
  `codex/root-tooling-placement`.
- Exact base: commit `416ea6ce59e019b7dec0f32e8262640133c6dca5`, tree
  `0ee2597a1ca69656e941d2c88e1035cf6e992b4b`.
- Current phase and milestone: phase 5 of 5, milestone 8 of 8 is complete.
- Current-state suffix: M8 complete; accepted closeout `f309f29f`, tree
  `c05c2ed6`, is squash-integrated as commit `f8377094f5cb4bfa056cf9e286c1cf8efd386986`,
  exact tree `c05c2ed6d1adef05bc6cdc3cb486fc29899f2c97`. The project control ledger
  records the same immutable mapping and completed queue disposition.

This Task record defines its phase and milestone horizon, execution capsule,
budgets, findings, and evidence. The project control ledger defines permanent
identity, queue state, completion grace, worktree mapping, and integration state.

## Expected Outcome

The two live repository-owned agent-tooling capabilities have explicit source
scopes below `src/agent-tooling/`. The coordinated-review Git inspection engine
and its evidence remain together under `review/`. The APM-to-Codex generated
projection patcher remains together under `agent-projections/` and is split only
where the current TypeScript size and responsibility rules require it. Every
active consumer, test, command, and current path reference uses those sources.
The unexplained root `scripts/` directory no longer exists.

The Task preserves behavior. It does not extract an orchestration Extension,
change agent or review policy, change npm packaging, change the replacement CLI,
or add JavaScript or MJS source.

## Authority And Placement

- The [Open Forge Architecture](../../../crystallized/documents/architecture.md)
  defines deterministic tools as a distinct system area that consumes visible
  source without becoming an authority source.
- The [Source Locality Directive](../../../../directives/source-locality.md)
  requires source, contracts, tests, fixtures, and focused support to remain at
  the narrowest useful capability scope.
- The [agent and workflow audit](../../../emerging/observations/2026-09-02_agent-and-workflow-change-audit.md)
  proves that all three root scripts are live, names their consumers, and records
  the root placement as provisional rather than authoritative.
- The [orchestration planning boundary](../../../../../src/extensions/orchestration/README.md)
  remains planning-only. Repository-local agent tooling does not become an
  Extension payload in this Task.
- `src/agent-tooling/review/` is the narrow source scope for the fixed immutable
  Git-object engine used by the tracked OpenCode adapter.
- `src/agent-tooling/agent-projections/` is the narrow source scope for the
  Open Forge-owned correction from authored APM agent policy to generated Codex
  agent projections.

## Frozen Callable And Path Contract

| ID  | Surface               | Preserved contract                                                                                                                                                                                                   |
| --- | --------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| G1  | Review engine         | `GitObjectOperations`, `GitObjectOperation`, `GitObjectResult`, `GitObjectInspectionError`, and `inspectGitObjects` retain their names and behavior from the new review source path.                                 |
| G2  | OpenCode adapter      | `.opencode/tools/inspect-git-objects.ts` remains the only tracked OpenCode tool and keeps its structured, shell-free adapter plus `executeInspectionRequest`.                                                        |
| G3  | Projection patcher    | `node --experimental-strip-types src/agent-tooling/agent-projections/patch-codex-agent-models.ts [options]` preserves the current options, defaults, exit meanings, generated-target writes, and `--check` behavior. |
| G4  | Root package consumer | `apm:install` still runs frozen APM installation before the projection patcher, with only the patcher path changed.                                                                                                  |
| G5  | Source shape          | Root `scripts/` is removed. Authored source remains TypeScript ESM. Production files stay below 200 lines and are split by capability rather than into generic helpers.                                              |

Expected production and evidence paths are a forecast, not an allowlist:

| Path                                                                                 | Responsibility                                                                                                                                  |
| ------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| `src/agent-tooling/README.md`                                                        | Explain the repository-only agent-tooling boundary and its two capability scopes without creating Framework or Extension authority.             |
| `src/agent-tooling/review/inspect-git-objects.ts`                                    | Remain the sole provider-neutral review entry CLI and exported fixed Git-object inspection capability.                                          |
| `src/agent-tooling/review/inspect-git-objects.integration.test.ts`                   | Preserve the existing seven owned gateway, immutability, adapter, permission, and tracked-bridge cases through real temporary Git repositories. |
| `src/agent-tooling/agent-projections/patch-codex-agent-models.ts`                    | Remain the sole projection entry CLI; parse arguments, read/write exact files, coordinate policy and document modules, and report results.      |
| `src/agent-tooling/agent-projections/agent-model-policy.ts`                          | Parse authored APM agent frontmatter and optional Open Forge overrides, validate owned policy values, and form the merged policy map.           |
| `src/agent-tooling/agent-projections/generated-codex-agent-document.ts`              | Read a generated Codex agent's owned name/model fields, form exact TOML field updates, and report owned check mismatches.                       |
| `src/agent-tooling/agent-projections/patch-codex-agent-models.integration.test.ts`   | Use one owned temporary fixture to prove Open Forge policy extraction and merge plus generated TOML update/check behavior.                      |
| `.opencode/tools/inspect-git-objects.ts`                                             | Keep the one exact tracked OpenCode bridge and import the provider-neutral review capability from its new path.                                 |
| `package.json`                                                                       | Keep `apm:install` behavior and change only its patcher source path.                                                                            |
| `tsconfig.json`                                                                      | Typecheck the new production agent-tooling sources while keeping Bun test globals outside the production compiler context.                      |
| `src/extensions/orchestration/README.md`                                             | Point the current repository-local trial description at the new review source without changing the planning-only Extension boundary.            |
| `.agents/memory/emerging/ideas/review-orchestration-trial-controls.md`               | Update only current engine and projection-validation paths; preserve immutable historical receipts.                                             |
| `.agents/memory/emerging/observations/2026-09-02_agent-and-workflow-change-audit.md` | Add one later occurrence that records the accepted placement resolution without rewriting the earlier audit evidence.                           |
| This Task record, `tasks/_tasks.md`, and `project-control.md`                        | Keep Task-owned progress and project mapping current.                                                                                           |

No `.gitignore` change is expected. The tracked OpenCode bridge path is
unchanged, and ordinary source/test files below `src/` need no exception.

Protected paths include C# source and tests, CLI package-manager source,
`src/open-forge/`, `.apm/agents/`, generated agent projections, `apm.lock.yaml`,
public command contracts, and every orchestration policy or workflow meaning.

## Evidence Matrix

| ID  | Owned behavior or condition                                       | Evidence                                                                         | Expected result                                                                                                                                    |
| --- | ----------------------------------------------------------------- | -------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| R1  | Review inspection remains fixed, bounded, local, and read-only    | Relocated Bun integration evidence over a real owned Git fixture                 | The existing seven tests pass without lazy fetch, worktree/index/object/config/ref mutation, unsafe input acceptance, or adapter/permission drift. |
| R2  | Projection patcher remains reachable through its package consumer | One small black-box integration test with owned temporary APM and Codex fixtures | Model and reasoning policy are patched, and `--check` accepts the resulting generated projection without exercising APM or Codex as test subjects. |
| R3  | Canonical source compiles and remains focused                     | Root TypeScript check plus file-size and import audit                            | Production tooling is strict ESM, warning-free, below 200 lines per file, and free of unchecked workspace-owned casts.                             |
| R4  | Consumers and current references follow the move                  | Exact `rg`, package JSON parse, and tracked-path inventory                       | The adapter, package command, current trial documentation, and tests point only to canonical paths; historical receipts remain truthful.           |
| R5  | Root debris is gone                                               | Git inventory and source search                                                  | `scripts/` has no tracked or untracked Task-owned file; no authored tracked JS, MJS, or CJS is added.                                              |
| R6  | Task scope is bounded                                             | Diff, formatting, lint, and protected-path audit                                 | Only the accepted tooling, its focused evidence, exact consumers/configuration/current path references, and Task state change.                     |

Baseline evidence at the exact base selected and passed the existing review
inspection suite `7/7`, with zero failures. The root `bun run typecheck` command
could not start because this fresh worktree has no local `node_modules`; final
verification may use the already-prepared dependency tree from the primary local
checkout without installing or downloading anything.

M3 and M4 moved both capabilities, removed the root `scripts/` bucket, updated
the exact current consumers and references, and passed the review integration
suite `7/7` with 103 assertions plus the projection integration case `1/1` with
4 assertions. Strict production TypeScript, targeted ESLint, targeted Prettier,
patcher help, path classification, protected-surface, file-size, source-kind,
and diff checks passed. One parallel review-test attempt exceeded Bun's
five-second timeout under contention; its immediate isolated and post-commit
reruns passed `7/7`. The accepted implementation range is
`bde9d688088546b22ca0f029794b716f6e12d673..9f5aed2e2010c47b58649aecc509d6b36369b718`,
resulting tree `2ec59e11a2584a3406fbcc9b0426e5bdfc04e4ce`.

The Task Mastermind independently read the complete current C# Directive files
before freezing this packet. Their SHA-256 fingerprints are:

- `_csharp.md`:
  `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`
- `design.md`:
  `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`
- `style.md`:
  `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`

The accepted alternative analysis rejected root `scripts/` and root `tooling/`
as broad buckets, `.opencode/` as a provider bridge rather than shared source,
`.apm/` as authored role-source authority rather than general executable source,
and `src/extensions/orchestration/` because its extraction remains unvalidated
and planning-only.

## Review Finding

| ID        | Severity | Reproduced evidence                                                                                                                                                               | Disposition and correction                                                                                                                                                                                                                               |
| --------- | -------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| T11-R1-F1 | Medium   | The Idea's candidate-bound receipt attests only `87f2acb`, which contains the recorded old patcher blob and no `src/agent-tooling/` patcher, but two receipt commands were moved. | Fixed by `T11-C1` at `f211af8d`, tree `e2c06967`. The two historical commands are restored; live current-path references remain in the package consumer, agent-tooling source, live Idea evidence, observation addendum, Task, and orchestration README. |

The fresh reviewer otherwise accepted behavior, placement, TypeScript
structure, owned evidence, protected surfaces, and Task-state meaning. Its
review integration rerun passed `7/7` with 103 assertions; projection evidence
passed `1/1` with 4 assertions; strict TypeScript, targeted ESLint, diff,
source-kind, path, and patcher-help checks passed. No second review is budgeted.

## Final Evidence And Flow Observation

M7 verified the complete candidate `dfc044f2436754aad1dcb5aab87d25f15baa1079`,
tree `9032f9c98c0ea37c5c78f86e0c6d7103e0e8acb1`, from exact base
`416ea6ce59e019b7dec0f32e8262640133c6dca5`, tree
`0ee2597a1ca69656e941d2c88e1035cf6e992b4b`:

- review integration selected, executed, and passed `7/7` with 103 assertions;
  projection integration selected, executed, and passed `1/1` with 4
  assertions; failures and skips were zero;
- strict production TypeScript, targeted ESLint over seven Task TypeScript
  files, targeted Prettier over fifteen owned current files, and relocated CLI
  help passed without reported warnings or errors;
- the range contains 17 paths, the relocated review engine retains the exact
  original blob `6738c7b2`, protected-path changes are zero, root `scripts/` is
  absent, tracked authored JavaScript/MJS/CJS is zero, and the production
  TypeScript maximum is 196 lines;
- six old-path matches are historical evidence only, six operative new-path
  matches remain, current old-path consumers are zero, package JSON and
  TypeScript configuration parse, ancestry and range diff checks pass, and the
  worktree is clean;
- the exact prepared dependency-tree symlink used for local TypeScript and lint
  gates was removed. No dependency installation, network, APM installation,
  generated-agent mutation, remote action, or publication occurred.

The generated Entries section in `tasks/_tasks.md` retains its exact M2 hash.
Its existing blank-line convention is not Prettier-clean at the base or this
candidate, so it was excluded from the fifteen-file targeted format gate rather
than restated as a new warning.

The simplified flow worked well for this bounded placement Task: one
implementation owner preserved context through implementation and the single
grouped repair, while one fresh reviewer caught the only material issue, a
historical-receipt misclassification that preflight and implementation both
missed. The Task needed no parallel semantic fan-out, transfer, or merge repair.
This is one favorable narrow-task sample, not evidence that the same allocation
is better for broad or cross-cutting work.

## Execution Horizon

1. Phase 1, Preflight.
   - M1 verifies the base, authority, inventory, consumers, and C# Directive
     fingerprints.
   - M2 freezes the placement, callable/path contract, and minimal evidence.
2. Phase 2, implementation.
   - M3 moves and focuses the two capabilities and updates exact consumers.
   - M4 makes the focused evidence and static checks pass.
3. Phase 3, review.
   - M5 is one fresh Task Mastermind whole-task review of behavior, source
     placement, TypeScript structure, and evidence quality.
4. Phase 4, correction and gates.
   - M6 is one grouped repair by the same Brilliant Implementer when accepted
     findings exist.
   - M7 runs affected integration, TypeScript, formatting, lint, consumer,
     protected-path, and source-inventory gates.
5. Phase 5, closeout.
   - M8 records the final commit, evidence, simplified-flow observation, and
     integration handoff.

## Stop Conditions

Stop before changing agent behavior, review semantics, Extension packaging,
cross-runtime guarantees, generated agent content as authored source, npm or CLI
behavior, C# source, dependencies, remotes, publication, deployment, global
installation, destructive Git state, or history. Return a project change request
if the move requires a new provider integration or a broader deterministic-tools
architecture than the two proven live capabilities.
