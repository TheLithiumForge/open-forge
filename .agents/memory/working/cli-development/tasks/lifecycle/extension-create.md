---
open-forge:
  description: Implement creation of one reviewable Extension package outside a workspace
  tags: [Memory, Working, CLI, Task, Extension, Create, Lifecycle, Contextual]
---

# Implement Extension Create

## Task State

- State: Candidate Complete in the Assured profile; the command-local/core and
  mirrored Unit/Integration boundary is ready for protected sequential
  integration, and all product decisions remain closed.
- Parent: [Lifecycle Commands](_lifecycle.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/extension/create/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/extension/create/behavior.md).
- Rebased baseline: `e823f846ae1f7cd13a668346b3fc0ecd2a64956d` on
  `codex/extension-create` in the isolated Extension Create worktree.

## Expected Outcome

`extension create` creates one exact reviewable Extension package at an explicit
catalogue destination without a workspace subject or workspace lock.

## Architecture

- Keep exact package identity, manifest model, fixed empty payload scaffold,
  destination observation, `ExtensionCreatePlan`, result, and presentation local.
- Expose optional `--name`, `--description`, `--package-version`, and repeatable
  `--dependency`. Preserve accepted nonblank override text exactly and keep the
  version descriptive rather than adding SemVer or compatibility policy.
  Defaults are exact ID-derived display name,
  `Open Forge Extension package <stable-id>.`, `0.1.0`, and an empty dependency
  array. Sort explicit dependencies ordinally and reject invalid, duplicate, or
  self IDs without resolving availability.
- Keep the wizard command-local and ask only for missing required facts, currently
  stable ID and catalogue parent. Allow local correction of blank or invalid
  input without an attempt limit; EOF is no-write `invalid`, cancellation is
  no-write `interrupted`, and optional metadata never adds questions. Show
  resolved metadata in the plan; JSON, automatic, and redirected flows never
  prompt.
- Accept any existing safely resolved directory as the catalogue parent,
  including empty and marker-free directories. Do not create it or inspect
  unrelated siblings; classify only the exact `<catalogue>/<id>` destination.
- Emit the exact ordered command-local JSON result: catalogue, destination, ID,
  manifest, mode, intended/applied effects, verification, and
  `workspaceLifecycleChanged=false`, without duplicating shared envelope fields.
- Reuse Extension manifest/source facts and strict path validation. Use a
  separate create-only destination writer with exact collision and revalidation
  checks; do not call the workspace `FileChangeApplier`, acquire a workspace
  lease, or prepare a recovery bundle.
- Replace workspace-lock and recovery safeguards with exact catalogue destination
  identity, expected-state revalidation, and collision refusal. This command has
  no workspace lease, no Replace/Delete, and no recovery bundle.

## Evidence

Cover valid and invalid package IDs, exact destination, every manifest default
and override, native option value forms, singleton repetition, dependency order,
duplicate/self rejection, zero/one/all current missing required facts, local invalid
correction, EOF/cancellation, and direct equivalence, existing/colliding content,
empty/populated catalogue parents, unrelated siblings,
missing-parent refusal, case/Unicode aliases, ordered JSON, payload formation,
dry run, no workspace,
revalidation race, create-only partial failures, manifest/payload verification,
second run, no unrelated changes, process, and AOT.

## Execution Capsule

- Profile: Assured. This is a new public command archetype and a create-only
  filesystem mutation. Recovery is practical through deletion or correction of
  the new package directory, but the command must never overwrite or adopt an
  existing occupant.
- Supported boundary: malformed input, ordinary filesystem failures,
  interruption, static observable aliases, stale plans, accidental races, and
  cooperating Open Forge processes. A malicious same-user namespace swap remains
  outside the accepted CLI threat model.
- Foundations: consume the accepted Shell interaction transport, Extension
  stable-ID and strict manifest facts, physical-path resolution, shared typed
  statuses, and real `System.IO`. Add no dependency, reflection, native interop,
  fake filesystem, workspace lease, recovery bundle, or shared retry framework.
- Ownership: this task owns `Commands/Extension/Create/**`, its one-to-one Unit
  and Integration mirrors, directly required Extension manifest serialization
  support, and this task record. Shared root composition, `CliJsonContext`, group
  and root help, process and EndToEnd evidence, and unrelated commands remain
  protected for sequential integration.
- Direct neighborhood: Extension group binding and definitions, Shell binding
  and interaction contracts, Framework Extension identity and manifest readers,
  physical-path resolution, and temporary-workspace test support.
- Evidence ladder: Unit proves syntax, binding, defaults, validation, ordering,
  wizard policy, typed result, human output, and JSON projection. Integration
  proves real catalogue identity, exact-destination inspection, dry-run, create,
  verified no-op, collisions, revalidation, retained partial state, cancellation,
  and absence of workspace, lock, recovery, and unrelated effects. The task also
  runs the complete managed suites and direct supported `linux-x64` Native AOT
  module evidence. Registered process, shared serialization, help, and dogfood
  evidence wait for the protected sequential integration task.
- Budgets consumed: zero council passes; two independent whole-task reviews
  (`C1-REVIEW-01`, `C1-REVIEW-02`); one grouped correction cycle
  (`C1-CORRECTION-01`) and its bounded same-reviewer recheck.
- Current owner: Extension Create Task Mastermind. Gray, Red, Green, correction,
  exact-tip evidence, and independent review are complete. The next owner is the
  sequential integration owner for the protected composition, serialization,
  help, process, and dogfood boundary.

## Behavior And Acceptance Matrix

| Class | Accepted outcome | Cheapest decisive evidence |
| --- | --- | --- |
| Required input | Prompt-capable human flows obtain only missing ID and catalogue facts; JSON, automatic, redirected, EOF, and cancellation never invent them or write. | Unit plus direct Integration |
| Metadata | Exact deterministic defaults apply; singleton nonblank overrides replace one field; dependencies are valid, distinct, non-self, and ordinally sorted. | Unit |
| Destination | One existing safe directory is the catalogue parent; only `<catalogue>/<id>` is inspected; missing parents are invalid and unsafe identity or collisions fail closed. | Integration |
| Planning | Dry-run and apply share one manifest and exact two-path scaffold plan; dry-run writes nothing. | Unit plus Integration |
| Application | Revalidate immediately before create-only effects; never replace, delete, lease, bundle, restore, roll back, or compensate. | Integration |
| Verification | Absent destinations become exact scaffolds, exact scaffolds are verified no-ops, and divergent, partial, additional, unknown, or colliding occupants block. | Integration |
| Result | One typed workspace-free result drives human and JSON projections with the accepted field order and all seven status mappings; `attention` remains unreachable. | Unit; shared serialization waits for integration |
| Isolation | Unrelated siblings and workspace, lifecycle, lock, recovery, generated navigation, and package-source state remain unchanged. | Integration |

## Progress, Findings, And Corrections

- Preflight confirmed the exact clean worktree, accepted D0 contracts, integrated
  native interaction foundation, source placement, and protected sequential
  integration seams.
- Gray froze the definitions, symbols, absent-workspace binding, immutable
  request, manifest, plan, result, JSON projection, rendering, operation, and
  interaction-injection call surfaces under the command leaf. A focused Release
  Core build passes with zero warnings and errors. No domain behavior or test
  expectation is implemented yet.
- Red froze 59 Unit cases and 25 Integration cases under the command's mirrored
  test paths. Both Release test projects build with zero warnings and errors;
  44 Unit cases and all 25 Integration cases fail only at the named Gray
  behavior stubs, while 15 immutable symbol/model cases already pass. The matrix
  covers binding, metadata, interaction, results, presentation, real catalogue
  roots and root aliases, exact scaffolds, no-op/collision states, dry-run,
  workspace isolation, and cancellation. A focused Gray/Red follow-up freezes
  real command-local planning and individual create-effect stages so Integration
  evidence can prove same-plan revalidation and deterministic retention of an
  earlier real BCL effect when a later effect collides. No fake filesystem seam
  is authorized or introduced.
- Green now implements the complete command-local request, wizard, manifest,
  catalogue observation, planning, create-only effect, revalidation,
  verification, result, and presentation boundary. Final focused Release
  evidence is green at 72 Unit cases and 54 Integration cases with zero failures
  or skips; both projects build with zero warnings and errors.
- Two Red test setup errors were corrected without changing accepted meaning:
  the missing-path wizard cases now actually omit the path, and Unit planning
  scenarios use dry-run instead of asserting that a successful apply writes
  nothing. The retained-partial-state test also uses direct BCL cleanup because
  the temporary-workspace ownership helper correctly refuses to adopt the
  production-created directory.
- `C1-REVIEW-01` found three material gaps. `C1-R1` now re-prompts for missing or
  non-directory catalogue answers with actionable guidance while preserving
  unsafe/unavailable terminal states. `C1-R2` now includes direct cause and next
  action in compact output and complete identity, mode, verification, and
  finding-subject facts in expanded output. `C1-R3` moved real-filesystem
  operation evidence to Integration and directly proves that the result factory
  rejects unreachable `attention`.
- `C1-CORRECTION-01` also accepted all eight Overseer conformance findings:
  cohesive required-init request/result/stage carriers replace long positional
  and repeated-scalar calls; finite mappings and destination failure use clear
  exhaustive switches; repeated command-local constants have nearest owners;
  public/persisted mappings use named cases, declared-member coverage, and
  undefined rejection; wizard retries explain valid input; direct interaction
  evidence no longer claims JSON, automatic, or redirection ownership; pre-effect
  cancellation reports not-started/planned verification truthfully; and focused
  test factories remove repeated setup without a shared helper bucket.
- `C1-REVIEW-02` required fresh final-tip full/AOT evidence, retention of a
  successfully prompted ID through catalogue EOF/cancellation, and fail-closed
  request/plan mode handling. The bounded correction at executable commit
  `f7c16e3679869c04e7341429bf650fd3b52ed5a8` preserves accumulated terminal
  request facts, rejects undefined modes before effects, exhaustively maps every
  destination-failure state, and ensures an undefined effect kind escapes the
  writer's supported application-failure translation. Direct Unit and real-BCL
  no-write Integration evidence cover every correction. The same independent
  Sol/xhigh reviewer rechecked the pre-rebase equivalent
  `9690fd7f703c9e8ba6096442d1fa2cd1a09b4bf3` and returned `PASS` with no
  material findings; all owned production and test blobs remain identical after
  the rebase.
- Exact rebased executable source `f7c16e3679869c04e7341429bf650fd3b52ed5a8`
  passes the warning-free Release solution build, all 1,356 managed Unit tests,
  all 565 managed Integration tests, and all 125 unchanged managed EndToEnd
  regression tests, with zero failures or skips. The development process
  publication uses only unchanged protected composition and is regression
  evidence, not a registered public Create claim.
- Direct `linux-x64` Native AOT publication of the Integration executable from
  that same source passes all 54 Extension Create cases and the full 565-case
  Integration suite with zero failures or skips. Registered Create process,
  shared JSON serialization, help, and dogfood remain intentionally deferred to
  the protected sequential integration owner.
- No exceptional machinery is justified. Ordinary .NET 10, System.CommandLine
  2.0.11, source-generated JSON, and real BCL filesystem behavior cover the
  accepted task boundary.

## Completion And Integration Handoff

- The isolated command-local/core candidate is complete. No root Extension
  composition, shared `CliJsonContext`, group/root help, process harness, or
  EndToEnd Create path changed in this lane.
- Sequential integration must register the command and source-generated JSON
  projection, compose `CliInteractiveSession` only for prompt-capable human
  flows, add truthful group/root help, and prove registered managed and Native
  AOT process/redirected/JSON/dogfood behavior before program acceptance.
- Preserve the accepted one-workspace-free create-only operation: no workspace
  inference, lease, recovery bundle, rollback, dependency availability lookup,
  installation, or implicit registration.

## Stop Conditions

Stop before inferring a workspace, acquiring `.agents/open-forge.lock`, registering
the package automatically, resolving dependency availability, installing it,
adding optional-metadata questions, an attempt limit or generic retry framework,
or using a generic package generator.
