---
open-forge:
  description: Implement one-file route creation below an existing routable parent
  tags: [Memory, Working, CLI, Task, Route, Create, Mutation, Contextual]
---

# Implement Route Create

## Task State

- State: Active after Route Init and Mutation Foundation. Preflight began from
  clean isolated branch `codex/route-create` at exact integrated baseline
  `a7fc99fc9675d4020eb864a37098bb84c64d33f5`, tree
  `9c2f357526756dd962c7a531a115a5c89d377498`. The complete reviewed acceptance
  candidate is pending its coherent local commit and integration.
- Parent: [Route Mutation Commands](_route-mutation.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/route/create/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/route/create/behavior.md).

## Expected Outcome

`route create` creates exactly one ordinary route file below one existing
routable parent, optionally using one accepted body Template, without initializing
a scope, copying route categories, or taking subtree lifecycle ownership.

## Architecture

- Keep one concrete `RouteCreatePlan` with target identity, expected parent,
  intended bytes, generated-navigation change, and preconditions.
- Use shared route-segment validation, source identity, parent topology, Template
  reading, lock, atomic apply, verification, and external recovery-bundle
  primitives. Prepare one verified bundle covering every existing Replace/Delete
  before the first target effect; Create and no-op plans create none.
- Keep Template choice, content formation, collision policy, result, and rendering
  local.

## Active Task Capsule

- Profile: Assured. This is a public mutating command with source identity,
  generated-region replacement, recovery, process-schema, and Native AOT
  boundaries. Independent Gray, Red, Green, protected integration, and final
  review are proportionate; Blue or Purple requires a named material trigger.
- Outcome: implement and prove exactly one `open-forge route create` operation
  over one existing routable parent, with explicit metadata, an optional
  one-time Template body, one prospective Generated Navigation projection, and
  no lifecycle ownership.
- Worktree: dedicated Route Create feature checkout; branch
  `codex/route-create`; base `a7fc99fc9675d4020eb864a37098bb84c64d33f5`.
- Implementation owner: Route Create Task Mastermind, with one continuous
  implementation owner after Gray and Red are accepted.
- Expected production paths:
  `src/cli/core/OpenForge.Cli.Core/Commands/Route/Create/**`.
- Expected evidence paths:
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Create/**`,
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Create/**`,
  and the smallest command-local published-process evidence below
  `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/`.
- Protected integration neighborhood: root command composition,
  `CliJsonContext`, Route group help, public-process fixtures, and generated
  serialization registration may change only after the command-local callable
  and result graphs are frozen and focused evidence passes. Framework source,
  Markdown, metadata, Generated Navigation, mutation, lock, recovery, projects,
  packages, preceding commands, and lifecycle meaning remain protected unless
  direct evidence proves one already-accepted identical shared mechanism is
  missing.
- Evidence ladder: callable/definition/result Unit evidence; real owned-temp
  filesystem Integration evidence for identity, Template, prospective
  projection, dry-run, lock, apply, verification, residuals, and recovery;
  published managed process evidence for syntax, streams, JSON, exits, no-op,
  dry-run, and apply; affected regression filters; then one final warning-free
  Release build, complete managed suites, supported `linux-x64` Native AOT
  serialization/Integration/published evidence, isolated dogfood, and exact
  source/diff audits.
- Review budget: `RC-R1` Gray/Red contract-and-evidence review, `RC-R2` one
  independent final Sol correctness/architecture review, and one grouped
  correction cycle per accepted material review packet. Additional review or
  improvement phases require a distinct named risk.
- Commit policy: coherent natural-time local commits only, on weekdays between
  19:00 and 09:00 Europe/Zurich; never push, contact remotes, amend, rewrite
  history, or manipulate timestamps.

### Frozen Accepted Behavior Matrix

| Boundary | Accepted behavior |
| --- | --- |
| Target | One ID-mapped or exact-path ordinary Markdown file below `.agents`; no Loader, entrypoint, overwrite, Skill, resource, directory, parent creation, or inferred slug/scope |
| Parent | Exactly one existing recognized routable parent; missing chain directs to `route init`; ambiguous or unsafe topology blocks |
| Metadata | Explicit nonblank description, one or more ordered unique canonical tags, optional omitted-or-nonblank responsibility; canonical shared writer only |
| Template | Optional exact existing ordinary routed Markdown base carrying exact `Template`; remove only its frontmatter and copy its body once; no overwrite composition, substitution, origin receipt, or continuing authority |
| Navigation | Form from observed catalogue plus the one intended source; project the complete parent region from destination description/link/tags; no hidden Index invocation |
| Collision | Identical complete destination plus navigation is verified no-op; a differing or unsupported occupied target blocks without overwrite or adoption |
| Mutation | Dry-run and apply share one plan; apply may create the destination and replace only the parent machine-owned generated interior under one external lease |
| Recovery | No bundle for create-only or no-op; prepare one verified external bundle before any existing-target replacement; never restore or compensate target effects |
| Ownership | Route Create publishes no Framework or Extension lifecycle state and does not adopt an identical existing source |
| Result | Exactly one typed result feeds human and source-generated JSON presentation; all seven semantic statuses and accepted stream/exit policy apply |

### Accepted Wire Contract

The maintainer accepted one ordered Route Create envelope:
`schemaVersion`, `command`, `status`, nullable `workspace`, `result`, and nullable
`next`. The ordered `result` contains `mode`, `target`, nullable `parent`,
`metadata`, nullable `template`, `plan`, `effects`, `unchangedPaths`, `recovery`,
`verification`, and `findings`. It deliberately omits lifecycle, adoption, and
application duplicates because Route Create owns none of them.

- `target` contains `requested`, nullable `id`, and nullable `path`; `parent`
  contains `id`, `path`, and `form`; `metadata` contains `description`, nullable
  `responsibility`, and ordered `tags`.
- `template` contains `requested`, `id`, `path`, `classification`, and
  `bodyByteLength`; `plan` contains `completeness` and `safety`.
- Each effect contains `path`, `kind`, `action`, a required `change` object with
  nullable `before` and required `expected`, `outcome`, and `residual`; recovery
  contains `state` and nullable `residualPath`.
- The accepted finite finding order is `invalid-input`, `invalid-target`,
  `invalid-metadata`, `invalid-template`, `workspace-unavailable`,
  `workspace-unsafe`, `target-unsafe`, `target-content-differs`,
  `parent-missing`, `route-ambiguous`, `identity-collision`, `template-unsafe`,
  `metadata-unsafe`, `generated-region-unsafe`, `workspace-lock-unavailable`,
  `target-changed`, `recovery-conflict`, `inspection-incomplete`,
  `template-unavailable`, `metadata-incomplete`, `projection-incomplete`,
  `recovery-unavailable`, `recovery-artifact-retained`,
  `target-changed-during-apply`, `write-failed`, `verification-failed`,
  `recovery-failed`, `operation-failed`, and `interrupted`, each prefixed with
  `route-create.` on the wire.
- Invalid input/target/metadata/Template findings map to `invalid`; workspace
  through recovery-conflict findings map to `blocked`; inspection through
  recovery-unavailable findings map to `incomplete`; retained recovery maps to
  `attention`; apply/write/verification/recovery/operation failures map to
  `failed`; interruption maps to `interrupted`; no findings maps to `complete`.
- `next` precedence is missing parent -> `route init`, differing target ->
  `route update`, positively retained recovery artifact -> `cleanup`, invalid ->
  Create help, lock/changed target -> a fresh Create request, other blocked or
  incomplete -> `doctor`, failed -> verbose Create, and interrupted -> Create.

The maintainer accepted the final nullability decision: every Route Create
effect and JSON effect has one required non-null `change` object because no
absent-change lifecycle exists. Only `change.before` is nullable for destination
Create. Schema version 1 member names, order, and valid wire values remain
unchanged.

### Predecessor Conformance Residual

The integrated Route Init implementation still contains long positional data
construction for some `RouteInitEntrypoint`, `RouteInitEffect`, and
`RouteInitMetadata` values, plus nested ternary logic in
`RouteInitMetadataResolver`. Those shapes conflict with the current C# design
directive, but they do not change Route Create's accepted product meaning and
are outside this Task's ownership. Route Create must not copy them: use
required-init or named data construction where appropriate, cohesive typed
inputs, ordered guards, and exhaustive switches with a default throw. Repair
Route Init separately after the current shared Markdown/source-reader promotion
settles, when one bounded predecessor-conformance change can be reviewed and
proved without overlapping Route Create behavior.

### Preflight Reuse Boundary

The current source audit confirms that Route Create needs no shared Route
mutation engine and must not import Route Init or Index command-local planners.
It constructs one intended `SourceLogicalSource` locally, combines it with the
observed catalogue through `GeneratedNavigationFormationBuilder`, supplies the
new destination metadata alongside the direct siblings, and projects the
existing parent through the neutral Generated Navigation boundary. Source
identity/reference, catalogue, topology, Markdown and metadata, mutation,
external lock, and recovery types remain the reusable semantic authorities.

Target mapping, exact-parent policy, Template selection/body extraction,
collision and no-op classification, complete-plan equivalence, final
verification, result formation, and application choreography remain local to
Route Create. The prospective target is an intended source, never a fabricated
catalogue candidate. When the parent generated region needs replacement, its
verified recovery bundle is prepared before the destination Create effect.
Create-only and no-op plans require no bundle.

### Prepared Gray Placement Packet

Gray stays entirely below `Commands/Route/Create/**` until its accepted local
callable and result graph pass focused evidence. It adds no dependency on
`Commands/Index/**` or `Commands/Route/Init/**`.

- Root command files: `RouteCreateDefinitions`, `RouteCreateBinding`,
  `RouteCreateOperation`, and `RouteCreateOperationFactory`.
- Binding models: `RouteCreateSymbols` and required-init
  `RouteCreateBindingComponents`.
- Request models: finite `RouteCreateMode`, cohesive
  `RouteCreateMetadataInput`, and `RouteCreateRequest`. The request retains the
  selected `CliWorkspace`, original file target, explicit metadata, optional
  Template reference, and apply/dry-run mode only.
- Planning models: one required-init `RouteCreatePlan` and
  `RouteCreatePlanBuild`. The plan retains the original request, exact target,
  parent and optional Template sources, intended target bytes, ordered file
  changes, recovery targets, and one preview formation. It has no directory or
  lifecycle fields.
- Local planning callables: `RouteCreateTargetPlanner` for ID/exact ordinary
  Markdown mapping, `RouteCreateTemplateResolver` for existing routed Template
  selection and exact body extraction, and `RouteCreatePlanBuilder` for one
  complete preflighted plan. The builder consumes the neutral Generated
  Navigation projector directly rather than adding a pass-through command-local
  wrapper. These are local policy over the neutral catalogue, topology,
  Markdown/metadata, snapshot, Generated Navigation, mutation, and recovery
  boundaries.
- Local application callables: `RouteCreatePlanEquivalence`,
  `RouteCreatePlanRevalidator`, `RouteCreateApplicationOperation`, the static
  `RouteCreateApplicationResultFactory`, `RouteCreateAppliedVerifier`, and
  `RouteCreateRecoveryLifecycle`. Progress carries exact receipts and a directly
  typed optional uncertain `PlannedFileChange`; application returns the result
  formation directly rather than wrapping either value in one-member records.
  Application acquires the external workspace lease, rebuilds and compares the
  complete plan, revalidates every file expectation, prepares parent replacement
  recovery, applies destination before parent exposure, verifies exact receipt
  postconditions and final no-op convergence, then performs guarded recovery
  deletion.
- Result/presentation types: command-local finding, finite state, fact,
  formation, result, JSON document/projection, human/JSON/diagnostic renderer,
  and help types. Their exact public property and finding names follow the
  accepted wire contract above; source-generated serializer registration stays
  protected until public integration.

The callable chain is:

```text
RouteCreateBinding.Close
  -> RouteCreateOperation.ExecuteAsync
  -> RouteCreatePlanBuilder.BuildAsync
  -> RouteCreateApplicationOperation.ExecuteAsync
  -> RouteCreateResultBuilder.Build
```

`RouteCreateOperationFactory.Create` accepts
only the already-established optional `WorkspaceLockStoreRoot` integration seam;
recovery continues to use the accepted current-user LocalApplicationData
boundary. There is no service locator, reflection, runtime scan, fake
filesystem, generic Route engine, or broad options/context bag.

### Gray Contract State

Gray is authored entirely below `Commands/Route/Create/**` from clean predecessor
tip `d178a68f`. It freezes the accepted syntax, semantic request invariants,
finite states and findings, ordered result/JSON graph, typed planning and
application seams, external-lock injection point, exact receipt verification
boundary, recovery prepare/delete boundary, and presentation ownership. It uses
required-init immutable data carriers under the nearest `Models/**` topic,
meaningful invariant constructors, named four/five-argument construction, and
exhaustive finite switches with a default throw.

Planning, mutation, human rendering, diagnostics, help, and JSON serialization
remain explicit unreachable Gray shells. They do not fabricate success or
introduce placeholder async work. The warning-free Release Core build is the
Gray compile boundary; contract assertions and intended behavior failures begin
in the separate Red commit.

Forecast focused evidence is placed below
`Core.UnitTests/Commands/Route/Create/**` for definitions, binding, model
invariants, result/presentation, plan equivalence, and pure planning decisions;
below `IntegrationTests/Commands/Route/Create/**` for real workspace planning,
Template bytes, prospective navigation, collision/no-op, revalidation,
application, recovery, and residual state; and in the smallest published Route
Create process fixture after protected integration.

## Evidence

Cover valid Unicode/case segments, invalid/reserved segments, missing or ambiguous
parent, existing target, physical alias, Template absent/valid/malformed, dry run,
lock race, generated navigation, exact bytes, idempotent refusal,
bundle-preparation failure/retention, no unrelated changes, streams, exits, and
AOT.

## Preparation Closeout

Read-only preparation on clean no-op branch `codex/route-create` at exact base
`33913dfe7f8f80598ca4765c516d308ed179c3ab` produced no commit, Gray, Red, or
Green change. The accepted contract correction is integrated at
`cd01b8a71cec399a17409428835d18f589335623`; Route Create is not ready before
integrated Route Init.

- Expected implementation paths are
  `src/cli/core/OpenForge.Cli.Core/Commands/Route/Create/**`,
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Create/**`, and
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Create/**`.
- `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs`,
  `src/cli/core/OpenForge.Cli.Core/Shell/Serialization/CliJsonContext.cs`,
  `src/cli/core/OpenForge.Cli.Core/Framework/Documents/Metadata/**`,
  `src/cli/core/OpenForge.Cli.Core/Commands/Route/Shared/Rendering/RouteHelpSections.cs`,
  `src/cli/core/OpenForge.Cli.Core/Framework/**`, EndToEnd/Native AOT evidence,
  preceding commands, and shared program ledgers remain protected integration or
  predecessor surfaces.
- Decisive evidence must cover binding, exact parent/target resolution, content
  and Template formation, intended topology, dry-run/no-op, application and
  recovery, all statuses and presentations, process behavior, and Native AOT.

### Accepted Correction Integrated

The maintainer accepted the repeatedly specified result/recovery meaning:
`attention` occurs only after successful target verification when recovery
deletion is `Failed` and the artifact is positively observed `Retained`.
The protected contract correction, including removal of `currently reserved`,
is squash-integrated at `cd01b8a71cec399a17409428835d18f589335623` with exact
tree equality. Route Create Gray and Red remain blocked until Route Init is
integrated.

Preparation also crossed its no-restore verification boundary. The worktree
remained clean, and its warning-free Release build plus focused 695 Unit and 229
Integration passes remain useful evidence, but they are not final acceptance
evidence for this preparation closeout.

## Acceptance Candidate

Route Create Green, protected integration, the command-local
`RouteCreateJsonContext` predecessor slice, and the Route-help predecessor slice
form one reviewed candidate on `codex/route-create`. The final correction makes
`RouteCreateEffect.Change` and `RouteCreateJsonEffect.Change` required while
keeping `Before` nullable. Focused post-correction Unit `24/24` and Integration
`23/23` pass, and the affected Core, Unit, and Integration Release builds have
zero warnings and errors.

A locked restore against a fresh empty local source under the repository's .NET
10 SDK root restored all six projects without external access. The subsequent
no-restore format verification passes. Full Release managed evidence passes with
zero warnings or errors: Unit `1507/1507`, Integration `720/720`, and EndToEnd
`146/146`, all with zero failures or skips.

Supported `linux-x64` Native AOT evidence passes for the root executable and the
published Integration and EndToEnd executables. The native root is a stripped
x86-64 ELF PIE and reports `0.0.0-dev`; managed EndToEnd against that root is
`146/146`, native Integration is `720/720`, and native EndToEnd is `146/146`,
all with zero failures or skips.

Isolated native dogfood proves Route/Create help, JSON dry-run with no fixture or
data mutation, apply, verified destination and parent hashes, persistent
zero-byte external lock, and repeat no-op with unchanged hashes. The final
independent Sol/xhigh `RC-R2` review returns `PASS` at `0.91` confidence with no
material finding. `git diff --check` is clean. This Task remains Active until
the reviewed candidate and acceptance ledger are committed and integrated; the
separate CLI Quality Remediation Task remains Planned until then.

## Stop Conditions

Stop before adding `--scope`, automatic slugification, parent creation, chain
initialization, managed subtree behavior, or a generic create engine.
