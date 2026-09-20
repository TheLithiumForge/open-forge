---
open-forge:
  description: Handoff for resuming Route Create Green and later protected integration
  tags: [Memory, Archived, Contextual, Historical, Handoff, CLI, Route, Development, Evidence, Git]
---

# Route Create Green Handoff — 2026-08-31

## Purpose

Resume the replacement CLI without reconstructing this long Overseer session.
This Handoff records the exact Git/worktree state, accepted product meaning,
development rules that became important late in the session, verified evidence,
uncommitted Green draft, pending maintainer decisions, and the safest next steps.

The Crystallized contracts remain product authority. This file does not add or
remove a feature.

## Start Here

Read these before acting:

1. `.agents/loader.md` and every scope it selects for the intended action.
2. `.apm/agents/overseer.agent.md` when resuming as the user-facing Overseer.
3. `.apm/agents/task-mastermind.agent.md` before delegating Route Create.
4. `.agents/directives/csharp/_csharp.md`.
5. `.agents/directives/csharp/design.md`.
6. `.agents/directives/csharp/style.md`.
7. `.agents/directives/proportional-development.md`.
8. `.agents/directives/open-forge/cli/_cli.md` and `implementation.md`.
9. `.agents/directives/open-forge/testing/_testing.md` and
   `evidence-integrity.md`.
10. `.agents/directives/execution-safety.md`, `source-locality.md`, and
    `hierarchical-orchestration.md`.
11. `.agents/workflows/development/_development.md` and the applicable
    Phase/acceptance workflows.
12. `../cli-development/tasks/route-mutation/route-create.md`.
13. `../../crystallized/documents/cli/contracts/route/create/interface.md`.
14. `../../crystallized/documents/cli/contracts/route/create/behavior.md`.
15. `../cli-development/overseer-memory.md`,
    `../cli-development/plan.md`, and `../checkpoints/cli-development.md` for
    the wider program.

Always load the C# design document for every implementation agent. Do not rely
on a parent summary as a substitute.

## Maintainer Working Rules

### Authority And Continuity

- English only.
- The maintainer owns every architectural decision and every feature addition
  or removal. Agents may identify problems and recommend the smallest option,
  but must not change accepted product meaning without approval.
- A question pauses only the exact affected boundary. Continue all unaffected
  work until the maintainer explicitly asks to halt.
- Surface immediately any design divergence, nonconformity, or need for a
  workaround instead of ordinary modern C#/.NET.
- Preserve sound work. A missing agent self-identification is not a reason to
  discard correct work when the actual model/reasoning can be verified.

### Git And Worktrees

- No push or other remote mutation. The earlier LithSnap push was the sole
  explicit exception and is unrelated to this repository.
- Work only on feature branches in dedicated worktrees outside the primary
  checkout. Keep each worktree isolated under the maintainer's designated
  sibling worktree directory.
- Do not mutate `develop` during feature work. Integrate only after complete
  review and evidence.
- Do not amend, rebase published history, reset destructively, or rewrite
  timestamps.
- Use plain English commit subjects, normally as past-tense sentences. Do not
  use category prefixes such as `feat:`, `fix:`, `docs:`, or `chore:`.
- Give material commits and squashes a comprehensive body that explains what
  changed and why. Do not reduce a material feature squash to a thin one-line
  message.

### Required Commit And Squash Style

Use the maintainer's explicit style for every future commit and squash:

- Write a concise plain English subject, normally in the past tense, such as
  `Implemented Route Create` or `Corrected Route Create result validation`.
- Never add a conventional category prefix. Existing prefixed commits on this
  feature branch are historical implementation checkpoints and do not define
  the required style.
- A concise opening paragraph describing the delivered behavior.
- A `Why:` section explaining the project reason and important design boundary.
- A `Feature history:` or `Candidate history:` section listing the meaningful
  feature-branch commits/messages that the squash represents.
- An `Evidence:` section with exact warning/error and test counts, managed and
  supported Native AOT coverage, review, dogfood, diff, and tree-equality facts.
- Mention exact candidate commit/tree identity when integrating a reviewed
  candidate.
- Preserve important implementation rationale such as reuse of neutral
  primitives, avoided general engines, recovery/locking boundaries, and
  deliberate deferrals.
- Use a later focused docs closeout commit when authoritative ledgers need to
  record the actual integration commit/tree. Do not hide a product squash and
  a closeout correction in an unclear message.

Before authoring the future Route Create squash, inspect the recent
maintainer-authored plain English commits on `develop`, then apply the explicit
rules above. Do not copy a prefixed subject from an older feature commit.

### Agent And Efficiency Rules

- Current execution preference is mainly sequential: one Task Mastermind and
  one continuous implementation owner. Parallelize independent preparation or
  mechanical evidence only when it cannot create overlapping meaning/writes.
- Use only GPT-5.6 Sol/xhigh and GPT-5.6 Luna/max unless the maintainer changes
  this rule.
- Every agent's first message should state task/role, model, and reasoning.
- Tell the maintainer whenever a direct child spawns or retriggers a descendant,
  including its name, role, model, and reasoning.
- Consequential design, important implementation, semantic convergence, and
  fresh correctness review belong to Sol/xhigh.
- Pre-decided builds, focused tests, Native AOT runs, diff/status checks, and
  literal output parsing should use Luna/max to save tokens.
- A mechanical runner may be explicitly told not to load Open Forge context and
  to execute only the supplied commands. This exemption never applies to
  design, implementation, semantic review, integration ownership, or acceptance.
- Apply a latency circuit to a runner that produces no checkpoint or file;
  interrupt it without discarding sound work and let the continuous owner
  resume. Do not create overlapping writers.
- If more than three genuinely independent feature tasks are later reopened in
  parallel, sub-Overseers may own groups of two or three Tasks. That is not the
  current Route Create mode.

## C# And Architecture Rules To Preserve

- Modern, readable, BCL-first, Native-AOT-safe C#. No reflection, dynamic
  serialization, source scanning, custom parser/writer substitute, P/Invoke,
  or speculative framework.
- This is a Markdown-management CLI, not critical infrastructure. Use
  proportionate safety: bounded plans, external lease, exact preflight and
  revalidation, verified recovery artifact for existing-target replacement,
  final verification, and truthful residual reporting. No Git orchestration,
  rollback, restoration, journal, transaction engine, or hostile-process model.
- Promote identical reusable semantics to their nearest honest shared scope.
  Similar-looking command policy stays command-local. Never create a generic
  Route engine merely because Create resembles Init or Index.
- Prefer `required init` object initializers for public/result/state data
  carriers. Prefer named constructor arguments for internal invariant
  constructors. Avoid long positional calls.
- Keep methods and constructors to roughly three to five parameters. When
  several values repeatedly come from one conceptual object/stage, pass that
  cohesive typed object rather than unpacking and forwarding all its fields.
- Use exhaustive switch expressions/statements with every named enum value and
  a default `ArgumentOutOfRangeException`. This is the accepted pragmatic
  exhaustive-switch pattern; do not add analyzer/generator/union machinery.
- Prefer interpolated strings and raw/template string literals. Do not build
  readable output through noisy concatenation or long `StringBuilder.Append`
  chains when an interpolated line expresses the intent.
- Avoid magic strings/numbers. Put stable wire names, marker values, limits,
  and repeated test facts in the nearest constant/finite authority.
- Keep property-only records in the nearest `Models/<topic>` scope. Do not use
  `Shared` as a dumping ground.
- Use immutable collections at durable boundaries. Do not expose lazy or
  mutable enumerable state from result formations.
- Tests use real typed facts, composable seeds/factories, and owned temporary
  workspaces. No mocks unless a future boundary makes the real alternative
  genuinely expensive. Keep tests fast, named, focused, happy/bad-path complete,
  and non-brittle.

## Program Order

The accepted behavior/integration order is:

1. Route Create.
2. Route Update.
3. Route Move.
4. Route Remove.
5. Root Update.
6. Extension Install, Update, and Remove at their recorded later boundary.
7. Status.
8. Doctor.
9. Repair.
10. Cleanup.

Independent preparation may be performed early, but dependent command behavior
must preserve this order. The immediate task is Route Create only.

## Exact Repository State

### Develop

- Worktree: primary repository checkout.
- Branch: `develop`.
- HEAD: `a7fc99fc9675d4020eb864a37098bb84c64d33f5`.
- State at handoff: clean, ahead of `origin/develop` by 55; no push occurred.
- Route Init is complete and integrated. Do not touch `develop` while resuming
  Route Create.

### Route Create Feature

- Worktree: dedicated Route Create feature checkout.
- Branch: `codex/route-create`.
- Merge base with `develop`: `a7fc99fc9675d4020eb864a37098bb84c64d33f5`.
- Committed HEAD: `3ad2b64246f66fc2f36e8dad52800c16fb100c6c`.
- Branch is 10 commits ahead and 0 behind `develop` at handoff.
- No squash, rebase, merge, push, or `develop` mutation has occurred.

Committed feature history in order follows. The descriptions state what each
checkpoint did; the original prefixed subjects do not define future Git style.

1. `6a39cdf5` — Activated the Route Create task.
2. `8a06cb14` — Aligned Route Create metadata authority.
3. `bb993dbc` — Recorded the Route Init design residual.
4. `6ca88173` — Froze the Route Create reuse boundary.
5. `210b33e2` — Shared Markdown writing and source snapshots.
6. `d178a68f` — Prepared the Route Create Gray placement.
7. `88d41950` — Froze the Route Create contracts.
8. `eb0535bb` — Accepted omitted Route Create responsibility.
9. `ed4d3b37` — Enforced Route Create metadata states.
10. `3ad2b642` — Froze the Route Create behavior evidence.

## Verified Gray And Red State

- Gray freezes command grammar, request/result graphs, finite findings/statuses,
  ordered human/JSON projection types, planning/application callables, external
  lock injection, recovery, verification, diagnostics, and help ownership.
- `--responsibility ""` is the accepted exact-empty spelling and normalizes to
  omission. A bare `--responsibility` is invalid. Do not reintroduce the earlier
  incorrect `--responsibility=` test assumption.
- Invalid preflight results may carry unresolved empty attempted metadata.
  Every non-invalid result requires a nonblank description and at least one
  valid unique tag.
- RC-R1 independent Sol/xhigh review found no material Gray/Red defect or
  callable contradiction.
- Final committed Red evidence:
  - Unit Release build: 0 warnings, 0 errors.
  - Integration Release build: 0 warnings, 0 errors.
  - UnitContract: 16/16 pass.
  - UnitBehavior: 7/7 fail only at their named Gray seams.
  - IntegrationBehavior: 20/20 fail only at their named Gray seams.
  - Core, Unit, and Integration whitespace plus diff checks are clean.
- Red covers grammar, repeated singleton options, valid/invalid target forms,
  immutable result invariants, finite mappings, status/next precedence, ordered
  JSON, presentation, real Template resolution, missing/ambiguous parent,
  physical alias, apply/dry-run/no-op/differing target, complete-plan
  revalidation, ordered reported effects, final verification, recovery
  preparation/collision/deletion/retention, and no-write dry-run.

## Uncommitted Green Draft

Eight production files are modified and uncommitted:

- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Create/Shared/Application/RouteCreateApplicationResultFactory.cs`
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Create/Shared/Application/RouteCreatePlanEquivalence.cs`
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Create/Shared/Planning/RouteCreatePlanBuilder.cs`
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Create/Shared/Planning/RouteCreateTargetPlanner.cs`
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Create/Shared/Planning/RouteCreateTemplateResolver.cs`
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Create/Shared/Rendering/RouteCreateDiagnosticRenderer.cs`
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Create/Shared/Rendering/RouteCreateHelpSections.cs`
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Create/Shared/Rendering/RouteCreateHumanRenderer.cs`

Drafted behavior:

- target resolution;
- Template resolution through the shared source/Markdown/metadata boundaries;
- structural complete-plan equivalence rather than record equality over
  `ImmutableArray`-backed snapshots;
- application-result formation;
- human, diagnostic, and command-local help rendering;
- a compile-shaped but unaccepted PlanBuilder draft.

Still stubbed:

- plan revalidation;
- recovery preparation and deletion;
- final applied verification;
- application operation;
- top-level operation;
- JSON renderer.

The last focused partial-Green run occurred before the current PlanBuilder and
Template drafts. It passed 21/23 Unit cases: all 16 contracts and every behavior
case except missing human description and held JSON serialization. The human
description/Next correction is present in the live diff but has not been
reverified. No build or test run covers the current PlanBuilder draft. Treat the
entire uncommitted diff as unreviewed.

### Mandatory First Correction

Do not commit `RouteCreatePlanBuilder.cs` as it stands. It is approximately 728
lines and violates the accepted C# design:

- `Preview` has 10 parameters.
- `Stop` has 7 parameters.
- `Effect` has 6 parameters.
- `BuildMetadataAsync` has 6 parameters.

Regroup repeated stage facts into cohesive required-init input/state records or
split truthful command-local responsibilities at the nearest local scope. Do
not create an options bag, service locator, generic Route engine, Route Init
dependency, or Index subprocess. After restructuring, audit every constructor
and method in the Green diff against the three-to-five rule and direct-object
passing preference.

Also retain these local review notes:

- `RouteCreateHelpSections` should use one raw string literal rather than
  newline concatenation.
- Human/diagnostic `Value` escaping duplicates behavior found in several other
  commands. Do not widen Green casually, but classify during protected
  integration whether identical escaping belongs in one neutral
  `Shell/Presentation` primitive. Record the decision; do not silently add
  permanent duplication.
- Continue auditing record equality where `ImmutableArray<byte>` or snapshot
  facts are involved. Byte-identical fresh plans must compare structurally.

## Pending Maintainer Decisions

### RC-G1 — JSON Source Generation

`RouteCreateJsonRenderer` cannot be implemented Native-AOT-safely until the
existing protected `Shell/Serialization/CliJsonContext.cs` registers
`RouteCreateJsonDocument`.

Recommended smallest edit:

- import the Route Create rendering/document namespace;
- add `[JsonSerializable(typeof(RouteCreateJsonDocument))]` to the existing
  source-generated context;
- change no schema, property order, nullability, composition, or help behavior.

Do not use reflection or a custom JSON writer. This authorization was requested
but not answered before handoff. Continue all unrelated Green work while it is
held.

### Effect Change Nullability

Every Route Create effect always has exact change facts. Recommended final
schema:

- `effects[].change` is a required non-null object;
- `change.before` is nullable for a destination Create;
- `change.expected` is required;
- every concrete effect continues to populate `Change`.

The Gray type remains conservatively nullable and Red deliberately does not
freeze general object nullability. Do not change it without maintainer approval.

## Accepted Route Create Design

- Creates one ordinary Markdown routed file below exactly one existing routable
  parent. No parent initialization, slugification, overwrite, adoption,
  directory creation, lifecycle publication, or generic scaffold engine.
- Required explicit description and ordered tags; optional responsibility;
  optional one-time exact routed Template body copy. Destination metadata is
  authored independently; no Template provenance/continuing authority.
- Uses only the `open-forge` YAML root for supported metadata. The shared reader
  accepts flow or block tag sequences; the canonical writer emits flow-style
  tags. `rune` is not interpreted and remains opaque/preserved elsewhere.
- Prospective target is an intended source, never a fabricated observed
  catalogue entry. Use the neutral Generated Navigation formation/projector.
- Exact complete destination plus navigation is verified no-op. Differing or
  unsupported occupied destination blocks without overwrite/adoption.
- Apply order is destination Create before parent generated-region replacement.
- Create-only/no-op needs no recovery. Any parent replacement requires one
  verified external recovery bundle before effects. Never restore or compensate.
- Lock is the existing persistent zero-byte external workspace lock below
  current-user LocalApplicationData, not `.agents`.
- Final verification checks exact receipt postconditions and converged no-op
  planning. Residual reporting remains truthful.
- Ordered result is `mode`, `target`, `parent`, `metadata`, `template`, `plan`,
  `effects`, `unchangedPaths`, `recovery`, `verification`, `findings` within the
  shared envelope `schemaVersion`, `command`, `status`, `workspace`, `result`,
  `next`.

## Protected Integration Still Later

After command-local Green is accepted and the two decisions above are resolved:

- register Route Create JSON type info in `CliJsonContext`;
- add root composition/binding and delimiter policy in
  `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs`;
- mark Route Create available in shared Route help;
- add the smallest published-process/E2E fixture and journeys;
- run serialization and composition evidence;
- run complete managed evidence, supported `linux-x64` Native AOT evidence, and
  isolated native dogfood in a disposable worktree;
- obtain a fresh Sol/xhigh correctness/design review;
- commit and integrate only within the allowed Git window.

## Next Safe Steps

1. Verify `develop` and the Route Create worktree identities/status exactly as
   recorded. Do not clean or reset the uncommitted draft.
2. Load the authoritative scopes and inspect the eight-file Green diff.
3. Correct PlanBuilder structure first; do not merely suppress the parameter
   violation or hide it behind an untyped context bag.
4. Run a focused Release build and Route Create Unit filters through a
   Luna/max mechanical runner. Fix compile/behavior issues in coherent slices.
5. Finish plan revalidation, recovery lifecycle, final verifier, application,
   and top-level operation sequentially.
6. Keep JSON held until RC-G1 is accepted; keep `Change` populated and its type
   nullability held.
7. Run focused Unit and Integration behavior continuously; then full command-
   local managed evidence.
8. Perform a fresh Sol/xhigh Green semantic/C# review before committing.
9. Commit Green only at a natural allowed time. Use a plain English, normally
   past-tense subject and an explanatory body. Do not amend or push.
10. Complete protected integration, process/Native AOT/dogfood evidence, final
    review, then author a detailed PR-style squash using the maintainer's
    explicit plain English style. Verify exact candidate-tree equality before
    integration.

## Stop Conditions

Return immediately to the maintainer if completing Route Create would require:

- a new feature or changed accepted behavior;
- reflection, custom JSON/YAML/Markdown writing, raw parsing, or dynamic runtime
  discovery;
- a generic Route/mutation/filesystem engine;
- Git interaction, rollback, restoration, journaling, or stronger hostile
  process guarantees;
- a public schema/nullability choice not already accepted;
- a workaround for a normal modern C#/.NET capability;
- changing a protected shared boundary beyond the smallest approved
  integration edit.
