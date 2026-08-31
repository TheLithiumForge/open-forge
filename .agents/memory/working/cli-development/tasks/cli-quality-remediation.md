---
open-forge:
  description: Remediate the accepted first-pass CLI architecture, C# design, authority, and test-evidence findings after Route Create and before Route Update
  tags: [Memory, Working, CLI, Task, Audit, Architecture, Refactoring, Testing, Review, Contextual]
---

# CLI Quality Remediation

## Task State

- State: Active immediately after Route Create integration
  `19412d2a562ae66d1b4642256d854438df75366f`, exact tree
  `2bbba7e216e75809e99213e0ccd155bffe720d1f`, and before Route Update. It is a
  separate root Task; no remediation implementation has begun.
- Responsible role: Overseer-managed Task Mastermind.
- Task source: Maintainer-accepted synthesis of the strategic first-pass PR-style audit of the replacement CLI `development` branch.
- Last updated: 2026-08-31.

Route Create is Complete and squash-integrated at `19412d2`. It supplies two
command-local predecessor slices: the `RouteCreateJsonContext` slice, with no
legacy JSON-context migration, and the Route half of group-help cleanup. This
Task is now Active; it owns the residual eleven command-local contexts/Shell
aggregate removal, the Extension help half, and findings
`CLI-DESIGN-003` through `CLI-TEST-013`. It consumes and revalidates those
predecessor slices rather than duplicating their correction. Nothing here
changes the sealed Route Create handoff.

## Problem And Expected Outcome

- Problem: A strategic first-pass PR-style audit of the complete replacement
  CLI found thirteen architecture, design, authority, and test-evidence issues
  or candidates. The audit was intended to resemble direct PR review, not an
  exhaustive deep scrub.
- Known or suspected cause: Some implementation and test surfaces have grown
  past their clearest ownership or evidence boundary, while a few shared
  boundaries still duplicate authority or catalogues. The final synthesis
  distinguishes proven defects from candidate improvements.
- Expected outcome: Correct the eleven actual defects and evaluate the two
  candidate improvements represented by the thirteen stable finding IDs,
  without changing accepted product behavior, public contracts, lifecycle or
  safety meaning, dependency direction, or the shared Architecture. Candidates
  may close as no-op after fresh proof.
- Execution profile: Assured. The work crosses serialization and help
  integration, command-local structure, shared authority, test evidence tiers,
  and durable test identity. It therefore needs sequential sub-batches,
  affected-boundary evidence, one grouped correction cycle, and a final
  independent review.

## Relationships

| Relationship | Link | Relevance |
| --- | --- | --- |
| Parent program | [Complete The Replacement CLI](00-cli-development.md) | Provides the accepted CLI outcome and project authority. |
| Completed predecessor | [Route Mutation Commands](route-mutation/_route-mutation.md) and [Route Create](route-mutation/route-create.md) | Route Create is Complete at `19412d2` and supplies the `RouteCreateJsonContext` and Route-help predecessor slices. This Task consumes/revalidates that evidence and owns the residual serializer/help work. |
| Next command | [Route Update](route-mutation/route-update.md) | This Task must complete before Route Update behavior. |
| Plan | [Development Plan](../plan.md) | Records the program sequence and gates. |
| Checkpoint | [CLI Development Checkpoint](../../checkpoints/cli-development.md) | Records resumable program state. |
| Overseer continuity | [CLI Overseer Memory](../overseer-memory.md) | Records the live orchestration horizon. |
| Candidate workflow idea | [Continuous Targeted Review Orchestration](../../../emerging/ideas/continuous-targeted-review-orchestration.md) | Explores a review-only parallel overlay; it is not a current workflow rule. |
| Audit source | Maintainer-accepted audit synthesis in the assigning packet | Supplies the final finding IDs, classifications, evidence scopes, and order preserved here. |

The audit snapshot is the exact clean `development` baseline
`a7fc99fc9675d4020eb864a37098bb84c64d33f5`. The original line locations below
are retained as audit-baseline evidence. Current symbol anchors are added where
line drift would otherwise make a future recheck brittle.

## References And Authority

| Source | Question it answers | Status or authority | May this task change it? |
| --- | --- | --- | --- |
| [Replacement CLI Architecture](../../../crystallized/documents/cli/architecture.md) | What structure, dependency direction, locality, and evidence boundaries are accepted? | Accepted current architecture | No; findings may be corrected against it. |
| [CLI contract set](../../../crystallized/documents/cli/_cli.md) | Which replacement command contracts and authority map apply? | Accepted current contract route | No. |
| [C# Directives](../../../../directives/csharp/_csharp.md), [Callable Design](../../../../directives/csharp/design.md), and [C# Style](../../../../directives/csharp/style.md) | Which source and test design rules apply? | Binding workspace directives | No; future agents must read all three completely. |
| [Review Evidence](../../../../directives/review-evidence.md) | How are stable findings, independent review, correction, and recheck handled? | Binding workspace directive | No. |
| [Source Locality](../../../../directives/source-locality.md) | When does behavior remain local or move to a nearest shared scope? | Binding workspace directive | No. |
| [CLI Implementation](../../../../directives/open-forge/cli/implementation.md) | Which replacement CLI, AOT, mutation, and test rules apply? | Binding CLI directive | No. |
| [Test Evidence Integrity](../../../../directives/open-forge/testing/evidence-integrity.md) | Which evidence tier and test-isolation rules apply? | Binding testing directive | No. |
| [Route Create Green Handoff](../../handoffs/2026-08-31_cli-route-create-green.md) | What accepted and sealed Route Create Green boundary must be preserved? | Sealed contextual handoff | No; it is protected and must not be edited. |
| [Development Plan](../plan.md) | What is the accepted order and full-gate policy? | Active working plan | Only its minimal sequencing language may be updated by the responsible Overseer. |
| [CLI Development Checkpoint](../../checkpoints/cli-development.md) | What is the current route-mutation state? | Active working checkpoint | Only its minimal sequencing language may be updated by the responsible Overseer. |

## Accepted Architecture And Decisions

- The remediation task changes implementation structure and evidence
  classification only. It is not authority to change product behavior, public
  wire schemas, command contracts, lifecycle ownership, safety guarantees,
  dependency direction, or the shared Architecture.
- Route order remains Route Create, then this separate quality-remediation
  Task, then Route Update, followed by Route Move, Route Remove, and root
  Update M3. Route Create integration is accepted at `19412d2`; this Task is
  Active next.
- `CLI-ARCH-001` and `CLI-ARCH-002` required Route Create predecessor slices
  before this Task could start. Route Create owns the accepted
  `RouteCreateJsonContext` predecessor slice (with no legacy JSON-context
  migration) and the Route half of group-help cleanup. After Route Create
  integrates, this Task owns the residual eleven command-local
  contexts/Shell aggregate removal and Extension help half, then the remaining
  findings. It consumes and revalidates predecessor evidence rather than
  duplicating correction. Exact protected serializer/help decisions remain
  with the responsible integration owner.
- Command-local JSON contexts and symbol-graph help ownership are the smallest
  stated correction directions. They do not authorize a new serializer
  framework, a second command catalogue, or a change to public output.
- Local command refactors must keep semantic policy local. Promote only a
  genuinely shared neutral capability or authority after a second real
  consumer proves identical meaning.
- `CLI-SHARED-009` and `CLI-TEST-013` are candidates, not proven defects. They
  require fresh proof and may close as no-op. They must not be reported as
  defects merely because similar code or test structure exists.
- There is no six-RID configuration finding. The final synthesis declined the
  subordinate-only candidate because existing accepted scaffolding already
  covers that concern.
- Every future C# implementation or review agent must independently read
  `.agents/directives/csharp/_csharp.md`, `.agents/directives/csharp/design.md`,
  and `.agents/directives/csharp/style.md` completely before acting. A parent
  summary does not replace those files.

### Placement And Ownership Map

| Finding family | Semantic owner | Placement direction |
| --- | --- | --- |
| `CLI-ARCH-001` | Shared Shell serialization boundary with command-owned graphs | Keep Shell metadata Shell-owned; register concrete graphs through command-local source-generated contexts at the protected integration boundary. |
| `CLI-ARCH-002` | Shared command-symbol/help relationship | Let standard help derive live children from the symbol graph; keep bounded unavailable/future notes local to the relevant help section. |
| `CLI-DESIGN-003` | Command-local Find, References, and Route List behavior | Split the service-bag records into cohesive typed capabilities at the nearest command scope; Route List receives only this finding. Do not create a repository-wide service bag or generic operation engine. |
| `CLI-DESIGN-004` | Command-local Route Init and Install planner behavior | Split each planner into thin local orchestration over cohesive capabilities; do not create a generic route or install planner. |
| `CLI-DESIGN-005` | Command-local Extension Inspect behavior | Split topical result/model and builder/projection capabilities while preserving command locality. |
| `CLI-DESIGN-006` | Command-local Find and References callable design | Replace long argument/reference trains with typed local stage inputs/results and cohesive capabilities. |
| `CLI-DESIGN-007` | Route Init, References, Find, and Framework Markdown C# conformance | Correct each local suppression/conditional at its nearest owner; `MarkdownDocumentParser` remains Framework Markdown-owned and gets focused parser evidence. |
| `CLI-AUTH-008` | Shared lifecycle schema authority | Consume `LifecycleSchema.RelativePath`; do not create a second path authority. |
| `CLI-SHARED-009` | Extension-family rendering support | Consider promotion only to `Commands/Extension/Shared/Rendering/ExtensionTextEscaping` after identical semantics are proved. |
| `CLI-TEST-010` through `CLI-TEST-013` | Test-project boundary and command-local test structures | Correct project tier, deterministic evidence, and durable names in the nearest test scope; keep pure Unit and real-boundary Integration evidence distinct. |

## Scope And Paths

### Included

- Correct the actual architecture, design, authority, C# conformance, test
  evidence-tier, cancellation-determinism, and durable-identity findings below.
- Evaluate the two candidates with fresh evidence and retain or close them
  without upgrading their classification.
- Preserve stable finding IDs, exact locations, evidence boundaries,
  consequences, correction directions, ownership, dependencies, and recheck
  results in the Task and its final acceptance record.
- Keep focused behavior unchanged and prove affected regressions, source
  generation/help integration where applicable, and the material final managed
  and supported `linux-x64` Native AOT boundary.

### Excluded

- Route Create production/test work currently in progress, its sealed Handoff,
  or its protected integration decision.
- New product behavior, public contract or wire changes, lifecycle ownership,
  stronger filesystem threat guarantees, dependency changes, new projects,
  new test tiers, or a generic route/query/refactoring framework.
- Root Discovery changes governed by `CLI-D076`; it is explicitly outside
  `CLI-ARCH-002`.
- A six-RID configuration rewrite or any other delivery expansion.
- Automated recovery, rollback, restoration, compensation, or changes to
  current safety semantics.
- Hand-editing generated `Entries` regions or running `open-forge index` as
  part of this writer action. The responsible Task Mastermind owns any later
  regeneration after semantic review.

### Expected Paths

Expected paths are forecasts for the future remediation Task, not an allowlist.
The implementation owner must report any directly required neighboring path.

- Shared and integration boundaries:
  - `src/cli/core/OpenForge.Cli.Core/Shell/Serialization/CliJsonContext.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Route/Shared/Rendering/RouteHelpSections.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Shared/Rendering/ExtensionHelpSections.cs`
  - `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs` only if an
    accepted serializer/help integration directly requires it.
- Command-local production structure:
  - `src/cli/core/OpenForge.Cli.Core/Commands/Find/**`
  - `src/cli/core/OpenForge.Cli.Core/Commands/References/**`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Route/List/Models/Operation/RouteListOperationComponents.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Route/Init/**`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Install/**`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Inspect/**`
- Exact conformance and authority paths named by the audit:
  - `src/cli/core/OpenForge.Cli.Core/Commands/References/ReferencesRequestBinder.cs`
  - `src/cli/core/OpenForge.Cli.Core/Framework/Documents/Markdown/MarkdownDocumentParser.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Route/Init/RouteInitBinding.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Route/List/Models/Operation/RouteListOperationComponents.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Route/Init/Shared/Planning/RouteInitMetadataResolver.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Route/Init/Shared/Planning/RouteInitPlanBuilder.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/References/Shared/Binding/ReferencesSelectorOccurrenceReader.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Find/FindRequestBinder.cs`
  - `src/cli/core/OpenForge.Cli.Core/Framework/Lifecycle/LifecycleSchema.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Route/Init/Shared/Application/RouteInitApplicationOperation.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Route/Init/Shared/Application/RouteInitApplicationResultFactory.cs`
- Extension-family candidate paths:
  - `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Create/Shared/Rendering/ExtensionCreateTextEscaping.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Inspect/Shared/Rendering/ExtensionInspectTextEscaping.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Extension/List/Shared/Rendering/ExtensionListTextEscaping.cs`
- Test evidence and identity paths:
  - `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Lifecycle/FrameworkLifecycleCurrentnessReaderRedTests.cs`
  - `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Mutation/Locking/WorkspaceLockContractTests.cs`
  - `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Init/RouteInitPlanBuilderRedTests.cs`
  - `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Init/RouteInitApplicationIntegrityTests.cs`
  - `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Init/Framework/RouteInitFrameworkSafetyIntegrationRedTests.cs`
  - `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Find/Shared/Selection/FindUniverseResolverRedTests.cs`
  - `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Find/Shared/Matching/FindMatcherRedTests.cs`
  - Their nearest active test-project consumers when a mechanical rename or
    truthful tier move requires the corresponding references.

### Direct Consumer And Test Mapping

| Finding | Direct production consumers | Direct evidence neighborhood |
| --- | --- | --- |
| `CLI-DESIGN-003` | `FindOperationComponents`, `ReferencesOperationComponents`, and `RouteListOperationComponents` | `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/{Find,References,Route/List}/**` and matching Integration consumers; Route List is in scope for this finding only. |
| `CLI-DESIGN-006` | Find matcher/result/projection stages and `ReferencesFindingFactory.AddFinding` | `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/{Find,References}/**` and matching Integration consumers; it does not apply to Route List. |
| `CLI-DESIGN-007` | Route Init binding/planning, References selector/binding, Find binding, and Framework Markdown parsing | Route Init, References, and Find focused regressions plus `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Documents/Markdown/**` parser evidence; it does not apply to Install or Route List. |

### Protected Paths And Authorities

- Integrated Route Create source and tests remain outside remediation ownership
  except for direct revalidation of the accepted `RouteCreateJsonContext` and
  Route-help predecessor slices. This Task consumes their accepted evidence
  without reopening Route Create behavior.
- The sealed `.agents/memory/working/handoffs/2026-08-31_cli-route-create-green.md`
  must not change.
- Accepted Architecture, command contracts, shared operation contracts,
  Framework, mutation, lock, recovery, lifecycle, Generated Navigation, and
  current product meaning remain protected semantic authorities.
- Root composition, source-generated serialization registration, public help,
  published-process fixtures, and Native AOT delivery surfaces are protected
  integration neighborhood paths. They may change only when accepted meaning
  directly requires it and the expansion is reported.
- Generated `Entries` regions in all routed Markdown files are protected from
  hand edits. The responsible Task Mastermind owns any later index generation.
- Unrelated production, tests, projects, packages, configuration, delivery, and
  historical or archived records remain outside this Task unless a named
  finding directly requires a path and the owner reports it.

## Assumptions, Prerequisites, Resources, And Recovery

| ID | Kind | Claim, required state, resource, or risk | Validation, availability, or signal | Owner or source | Response if false or triggered |
| --- | --- | --- | --- | --- | --- |
| A1 | Prerequisite | Route Create must reach its accepted command-local Green/focused evidence and protected integration boundary before this separate Task starts. | Satisfied at integration `19412d2`, exact tree `2bbba7e`; Route Create supplies the `RouteCreateJsonContext` and Route-help predecessor slices. | Route Create Task, Checkpoint, and Git state | Reopen only if exact baseline inspection contradicts the recorded integration. |
| A2 | Prerequisite | The current Architecture, contracts, mutation foundation, and project graph remain accepted. | Read the linked current sources and inspect the actual baseline before mutation. | Overseer and Task Mastermind | Return a project change request before changing shared meaning. |
| A3 | Boundary | The audit is a strategic first-pass PR-style audit, not exhaustive deep scrubbing. | Finding scope and no-findings caveat below remain visible. | Accepted audit synthesis | Do not expand the task into a general codebase cleanup. |
| A4 | Resource | Every C# implementer and reviewer can independently read the complete three C# directive files. | The future packet names all three exact paths and requires acknowledgement before work. | Task Mastermind | Stop that C# boundary and return a task gap if the rules cannot be loaded. |
| A5 | Evidence | Serialization/help and test-tier changes are material boundaries. | Applicability check selects affected Unit/Integration/public evidence and the complete managed plus supported `linux-x64` Native AOT gate at the material trigger. | Task Mastermind | Do not accept from source inspection or partial tests alone. |
| A6 | Candidate | Similar Extension escaping and Find test scenarios may not justify promotion by themselves. | Fresh proof must show identical semantic ownership or measurable evidence value. | Task Mastermind and reviewer | Close `CLI-SHARED-009` or `CLI-TEST-013` as no-op when proof is absent. |
| A7 | Safety | No remote, destructive, dependency-installation, publication, or delivery action is part of this task. | Execution-safety inspection and exact-path local edits. | All delegated owners | Return `AUTHORIZATION_REQUIRED` or stop at the boundary. |
| A8 | Recovery | A refactor or tier correction must preserve behavior and leave no unverified partial state. | Focused regressions, diff inspection, and full-gate trigger evidence. | Original implementation owner | Group the finding for correction and recheck; do not broaden scope silently. |

## Findings And Corrections

The following records preserve the final synthesis exactly at the level needed
for execution. “Actual defect” means the audit found a concrete violation in the
stated scope. “Candidate improvement” does not establish a defect.

### `CLI-ARCH-001` — BLOCKING

- Classification: Actual architecture defect.
- Evidence scope: `Shell/Serialization/CliJsonContext.cs` imports and registers
  concrete command JSON graphs, including the current concrete command JSON
  registrations.
- Consequence: Shell depends on concrete Commands. Route Create cannot safely
  extend this inversion.
- Smallest correction direction: Use command-local source-generated JSON
  contexts. Shell retains only Shell-owned serialization metadata.
- Local/shared and ownership classification: Shared Shell-to-Commands
  serialization boundary; integration-owned correction with command-local graph
  ownership. Do not add reflection, a custom JSON writer, or a second global
  serializer framework.
- Order and dependency: First active residual serialization batch on integrated
  baseline `19412d2`. Route Create supplied only the `RouteCreateJsonContext`
  predecessor slice, with no legacy JSON-context
  migration. This Task owns the residual eleven command-local contexts/Shell
  aggregate removal, consumes and revalidates the predecessor evidence, and
  does not duplicate its correction. The accepted Route Create integration
  decision remains unchanged.
- Recheck: Source dependency direction, source-generated registration, concrete
  serialization, managed process JSON, and supported Native AOT execution.

### `CLI-ARCH-002` — BLOCKING

- Classification: Actual architecture defect.
- Evidence scope: `RouteHelpSections` and the same defect in
  `ExtensionHelpSections` manually duplicate the live child-command inventory,
  while standard help derives it from the child graph. Root Discovery is
  explicitly excluded because `CLI-D076` governs it.
- Consequence: Two command catalogues can drift. Current Route Create help
  cannot be fixed safely by only toggling Create availability.
- Smallest correction direction: Let standard help own available children.
  Custom bounded notes describe only unavailable or future children without
  duplicating live inventory.
- Local/shared and ownership classification: Shared command-group help and
  exact symbol-graph ownership; integration-owned Route and Extension help
  surfaces. Root Discovery remains outside scope.
- Order and dependency: Second active residual group-help batch on integrated
  baseline `19412d2`. Route Create supplied only the Route half of the cleanup;
  this Task owns the Extension help half, consumes and revalidates
  the Route predecessor evidence, and preserves the exact composed child
  graph.
- Recheck: Composed standard help, bounded custom sections, Route/Extension
  help output, and affected published process evidence.

### `CLI-DESIGN-003` — HIGH

- Classification: Actual design defect.
- Evidence scope: `FindOperationComponents`, `ReferencesOperationComponents`,
  and `RouteListOperationComponents` are service-bag records hiding six to
  eight behavioral dependencies.
- Consequence: Ownership is weak, call surfaces are long and hidden, and
  composition and testing are harder to reason about.
- Smallest correction direction: Use cohesive typed stages or capabilities
  with small constructors and call surfaces.
- Local/shared and ownership classification: Command-local Find, References,
  and Route List structure. Keep semantic policy local; do not create a
  repository-wide service bag or generic operation engine.
- Order and dependency: Find/References/Route List structural sub-batch, after
  the planner and Extension Inspect batches in the accepted sequence.
- Recheck: Constructor/method call surfaces, direct consumers, namespaces,
  affected Unit/Integration behavior, and no dependency-direction drift.

### `CLI-DESIGN-004` — HIGH

- Classification: Actual mixed-responsibility defect.
- Evidence scope: `RouteInitPlanBuilder` is approximately 1,235 lines with 13
  collaborators; `InstallPlanBuilder` is approximately 910 lines.
- Consequence: Orchestration, discovery, validation, planning, and projection
  are entangled, increasing change amplification and obscuring ownership.
- Smallest correction direction: Make thin local orchestrators over cohesive
  command-local capabilities or stages. Promote only genuinely shared policy.
- Local/shared and ownership classification: Local Route Init and Install
  planner ownership, with any repeated neutral mechanism proved separately.
  The existing Route Init integration is not permission to copy its shape into
  other commands.
- Order and dependency: Route Init planner first, then Install planner, after
  serialization/help and test-tier sub-batches.
- Recheck: Planner graph, constructor/call surfaces, command-local policy,
  affected behavior and integration evidence, and unchanged public meaning.

### `CLI-DESIGN-005` — HIGH

- Classification: Actual Extension Inspect locality/cohesion defect.
- Evidence scope: Result file is approximately 679 lines and 53 types; result
  builder approximately 1,249 lines; comparison builder approximately 907
  lines; JSON projection approximately 610 lines.
- Consequence: Many topical responsibilities are concentrated in a few files,
  creating change amplification and making ownership difficult to discover.
- Smallest correction direction: Form topical result/model clusters and
  cohesive builder/projection capabilities while preserving Extension Inspect
  command locality.
- Local/shared and ownership classification: Extension Inspect-local cohesion
  correction. Do not promote an Extension Inspect policy or create a broad
  result framework without another real identical consumer.
- Order and dependency: Extension Inspect sub-batch, after the two blocking
  integration corrections and test-tier correction.
- Recheck: Type/topic ownership, consumers, source-generated JSON graph,
  behavior-preserving Unit/Integration/public evidence, and AOT compatibility.

### `CLI-DESIGN-006` — HIGH

- Classification: Actual Find/References callable and cohesion defect.
- Evidence scope: `FindMatcher` is approximately 704 lines with six to nine
  argument/reference trains; `FindResultBuilder` approximately 935 lines;
  `FindProjectionBuilder` approximately 518 lines including an eight-parameter
  method; `ReferencesFindingFactory.AddFinding` has 14 parameters.
- Consequence: Call surfaces are unsafe and stages conflate unrelated work.
- Smallest correction direction: Use typed local stage inputs and results with
  cohesive smaller capabilities.
- Local/shared and ownership classification: Command-local Find and References
  callable design. Keep shared source/document facts as their existing neutral
  authorities; do not build a generic query or result engine.
- Order and dependency: Find/References/Route List sub-batch (batch 7), after
  the Extension Inspect sub-batch and alongside the related
  `CLI-DESIGN-003` correction. `RouteListOperationComponents` is covered by
  `CLI-DESIGN-003` only; `CLI-DESIGN-006` remains Find/References-local.
- Recheck: Parameter counts and semantics, direct stage ownership, affected
  Find/References matching/result/projection behavior, and
  source-generation/AOT constraints. The companion `CLI-DESIGN-003` recheck
  owns Route List operation consumers.

### `CLI-DESIGN-007` — MEDIUM

- Classification: Actual C# conformance defects.
- Evidence scope: Postfix suppressions occur at
  audit-baseline `ReferencesRequestBinder:40-41`, audit-baseline
  `MarkdownDocumentParser:57`, and audit-baseline `RouteInitBinding:136`.
  Nested or chained conditionals occur at audit-baseline
  `RouteInitMetadataResolver:41-51`, audit-baseline
  `RouteInitPlanBuilder:1155-1160`, with current symbol anchor
  `RouteInitPlanBuilder.ReadTopologyFinding` (currently `1196-1201`),
  audit-baseline `ReferencesSelectorOccurrenceReader:69-73`, and
  audit-baseline `FindRequestBinder:374-376`.
- Consequence: The code violates accepted design/style rules and obscures
  invariants and control flow.
- Smallest correction direction: Remove suppressions through explicit flow and
  validation. Flatten conditional logic with named decisions or stages.
- Local/shared and ownership classification: Per-file C# design/style
  correction at the nearest existing Route Init, References, Find, or
  Framework Markdown owner. `MarkdownDocumentParser` is explicitly owned by
  Framework Documents/Markdown and receives focused parser evidence. Do not
  add an analyzer, warning-suppression framework, or architecture-wide
  abstraction.
- Order and dependency: Apply with the affected Route Init and
  Find/References sub-batches, not the Install batch, and perform a focused
  Framework Markdown conformance sweep plus one final conformance sweep after
  structural corrections.
- Recheck: Complete C# directive audit, focused Markdown parser evidence,
  Release warning count, format/diff checks, and directly affected tests.

### `CLI-AUTH-008` — MEDIUM

- Classification: Actual shared-authority duplication.
- Evidence scope: `LifecycleSchema.RelativePath` exists, but raw
  `.agents/open-forge.lifecycle.json` is repeated at
  audit-baseline `RouteInitPlanBuilder:1165`, current symbol anchor
  `RouteInitPlanBuilder.IsLifecyclePath` (currently `1203-1206`), and at
  audit-baseline `RouteInitApplicationOperation:490` and
  `RouteInitApplicationResultFactory:205`. Use the stable anchors
  `RouteInitApplicationOperation.IsLifecyclePath` and
  `RouteInitApplicationResultFactory.IsLifecyclePath` for those application
  sites.
- Consequence: Lifecycle path authority can drift.
- Smallest correction direction: Consume the canonical shared authority.
- Local/shared and ownership classification: Shared lifecycle-schema authority;
  the canonical `LifecycleSchema.RelativePath` remains authoritative. Route
  Create has no lifecycle publication. This is a guardrail, not scope expansion.
- Order and dependency: Route Init only; correct with its affected planner or
  application sub-batch, before final conformance acceptance. It never owns
  Install behavior.
- Recheck: Single-authority search, affected lifecycle/path behavior, and
  unchanged Route Create lifecycle non-ownership.

### `CLI-SHARED-009` — MEDIUM

- Classification: Candidate improvement, not a proven defect.
- Evidence scope: Three identical Extension text-escaping files exist.
- Consequence if confirmed: Duplicated identical semantic authority could drift
  and amplify maintenance.
- Smallest correction direction: Consider promotion only to
  `Commands/Extension/Shared/Rendering/ExtensionTextEscaping`.
- Local/shared and ownership classification: Candidate Extension-family shared
  mechanism. Do not promote beyond the Extension family without evidence of an
  identical consumer and accepted ownership.
- Order and dependency: Evaluate during the Extension Inspect sub-batch. It
  may close as no-op after fresh comparison.
- Recheck: Exact semantic comparison, consumer audit, focused rendering tests,
  and source-locality review.

### `CLI-TEST-010` — HIGH

- Classification: Actual wrong evidence-tier defect.
- Evidence scope: Real filesystem/OS tests are labelled Unit in
  `FrameworkLifecycleCurrentnessReaderRedTests:88-201`,
  `WorkspaceLockContractTests:105-174`,
  `RouteInitPlanBuilderRedTests:13-98,153-169`, and
  `RouteInitApplicationIntegrityTests:19-124`.
- Consequence: The Unit tier is not boundary-pure, and the taxonomy misleads
  readers about what the evidence proves.
- Smallest correction direction: Move real-boundary cases to Integration using
  `TemporaryWorkspace`. Retain pure callable cases in Unit.
- Local/shared and ownership classification: Test-project evidence boundary;
  test-local classification and fixture placement. `TemporaryWorkspace` is the
  accepted real-boundary authority; do not fake the filesystem.
- Order and dependency: Third, before accepting later refactor evidence.
- Recheck: Project placement, traits/display names, real temporary workspace
  ownership, focused Integration and Unit counts, and no duplicated lower-tier
  branch coverage at the system boundary.

### `CLI-TEST-011` — MEDIUM

- Classification: Actual nondeterministic evidence defect.
- Evidence scope: `RouteInitFrameworkSafetyIntegrationRedTests:154-195` uses
  `FileSystemWatcher` cancellation.
- Consequence: The evidence is timing- and environment-sensitive and can race
  with watcher delivery.
- Smallest correction direction: Use a deterministic boundary or barrier, or
  lower-level cancellation evidence without watcher races.
- Local/shared and ownership classification: Route Init Integration test-local
  evidence. Do not change production cancellation behavior or introduce a
  general watcher abstraction.
- Order and dependency: Deterministic-cancellation sub-batch after structural
  and tier corrections.
- Recheck: Repeated deterministic execution, cancellation outcome, no-write or
  residual assertions, and zero timing sleeps or watcher races.

### `CLI-TEST-012` — MEDIUM

- Classification: Actual durable-identity defect.
- Evidence scope: Fifty-two active test files/classes retain the `*RedTests`
  postfix.
- Consequence: Test identity encodes a temporary TDD phase rather than the
  durable capability or behavior under test.
- Smallest correction direction: Mechanically rename them to stable
  capability/behavior names with no semantic change.
- Local/shared and ownership classification: Test-local identity cleanup across
  the active Unit/Integration scopes. It does not change test tier or behavior.
- Order and dependency: Final durable-naming sub-batch after structural and
  evidence corrections, so names reflect the resulting stable tests.
- Recheck: Exact rename inventory, project references, discovered test count,
  stable display names/traits/evidence, and no expectation changes.

### `CLI-TEST-013` — MEDIUM

- Classification: Candidate test-structure improvement.
- Evidence scope: `FindUniverseResolverRedTests` and `FindMatcherRedTests` use
  string scenarios, branchy assertions, and large positional expectations.
- Consequence if confirmed: Scenarios and expected facts may be harder to read,
  evolve, or diagnose than typed focused cases.
- Smallest correction direction: Consider typed scenario data and/or focused
  tests while preserving coverage and behavior.
- Local/shared and ownership classification: Candidate Find test-local
  structure. Do not add a universal scenario framework or change production
  Find semantics.
- Order and dependency: Evaluate with the final durable-naming/Find test
  restructuring sub-batch. It may close as no-op after fresh proof.
- Recheck: Test readability, equivalent branch/coverage behavior, stable
  counts, focused execution, and no hidden fixture sharing.

## Audit Boundary And No-Findings Caveat

The audit covered 715 production C# files and 88,403 production lines, plus 285
test C# files and 68,416 test lines. It was a strategic first-pass PR-style
audit, not exhaustive deep scrubbing.

The final synthesis found no Framework-to-Commands/Shell inversion, sibling-
private command dependency, bad project reference, service locator, dependency
injection, second parser, dynamic dispatcher, custom conversion, or prohibited
general reflection. Source-generated JSON/YAML is acceptable. Root composition
is large but cohesive. Tests generally have `DisplayName`, `Feature`, and
`Evidence`; no mocks, sleeps, network access, or shared mutable fixtures were
found. `TemporaryWorkspace` is good authority.

These no-findings are boundary statements, not a guarantee that the audit
exhausted every possible defect. They do not reopen architecture or product
contracts.

## Accepted Order And Sub-Batches

The separate remediation Task is Active after Route Create integration. Route
Create supplied the accepted `RouteCreateJsonContext` predecessor slice and the
Route half of group-help cleanup. This Task now consumes and revalidates those
slices, owns the residual serializer/help corrections, and then executes the
remaining batches before Route Update. Execute the
remediation in this order, preserving the distinction between actual defects
and candidates:

| Order | Sub-batch | Finding IDs | Dependency and completion boundary |
| --- | --- | --- | --- |
| 1 | Serialization | `CLI-ARCH-001` | On integrated baseline `19412d2`, remove the residual eleven command-local contexts/Shell aggregate; consume and revalidate the supplied `RouteCreateJsonContext` predecessor slice without duplicating correction. |
| 2 | Group help | `CLI-ARCH-002` | On integrated baseline `19412d2`, correct the residual Extension help half; consume and revalidate the supplied Route half, preserve the exact child symbol graph, and exclude Root Discovery/`CLI-D076`. |
| 3 | Test evidence tiers | `CLI-TEST-010` | Correct Unit versus Integration claims before later evidence is accepted. |
| 4 | Route Init planner | `CLI-DESIGN-004`, affected `CLI-DESIGN-007`, `CLI-AUTH-008` | Split local planner responsibilities, remove affected conformance defects, and consume canonical lifecycle authority. |
| 5 | Install planner | `CLI-DESIGN-004` | Split the Install planner locally; do not attribute Route Init/Framework Markdown conformance or lifecycle authority work to Install. |
| 6 | Extension Inspect | `CLI-DESIGN-005`, `CLI-SHARED-009` | Refactor topical local clusters; evaluate Extension-family escaping promotion and close the candidate if proof is insufficient. |
| 7 | Find/References/Route List | `CLI-DESIGN-003`, `CLI-DESIGN-006`, affected `CLI-DESIGN-007` | Reduce service bags and long call surfaces with typed local stages and capabilities; Route List receives `CLI-DESIGN-003` only, while `CLI-DESIGN-006` and affected `CLI-DESIGN-007` remain Find/References-only. |
| 8 | Deterministic cancellation | `CLI-TEST-011` | Replace watcher-race evidence with deterministic boundary/barrier evidence. |
| 9 | Durable test naming and Find test restructuring | `CLI-TEST-012`, `CLI-TEST-013` | Rename active `*RedTests`; evaluate typed scenarios/focused assertions and close the candidate if no material gain is proved. |

The repeated `CLI-DESIGN-004` in sub-batches 4 and 5, and the affected C#
conformance locations in sub-batch 7, are one finding with multiple local
consumers, not new findings. `CLI-AUTH-008` is Route Init-only and is a
guardrail applied at its affected planner/application boundary, not a new
lifecycle feature. `CLI-DESIGN-003` is the only listed finding applied to
Route List. Each sub-batch keeps its own local behavior and evidence; shared or
protected integration is reviewed sequentially.

## Evidence And Acceptance Matrix

| Evidence class | Findings | Cheapest decisive evidence | Additional acceptance evidence |
| --- | --- | --- | --- |
| Shared serializer/help architecture | `CLI-ARCH-001`, `CLI-ARCH-002` | Source/dependency/locality inspection plus focused serializer/help tests | Published managed process JSON/help, affected regressions, and supported `linux-x64` Native AOT execution. |
| Route List component design | `CLI-DESIGN-003` | Direct consumer/call-surface and namespace audit for `RouteListOperationComponents` plus Route List Unit evidence | Affected Route List Integration/public regressions and no dependency-direction drift. |
| Find/References design and conformance | `CLI-DESIGN-003`, `CLI-DESIGN-006`, affected `CLI-DESIGN-007` | Direct call-surface, namespace, ownership, and C# conformance audits plus affected Unit tests | Affected Find/References Integration/public regressions; full managed/AOT gate when a shared or composition boundary is materially changed. |
| Route Init design, conformance, and authority | `CLI-DESIGN-004`, affected `CLI-DESIGN-007`, `CLI-AUTH-008` | Planner ownership, C# conformance, and canonical-authority audits plus affected Unit tests | Affected Route Init Integration/public regressions and lifecycle/path behavior; `CLI-AUTH-008` remains Route Init-only. |
| Install planner design | `CLI-DESIGN-004` | Install planner ownership and call-surface audit plus affected Unit tests | Affected Install Integration/public regressions; no Install attribution for `CLI-DESIGN-007` or `CLI-AUTH-008`. |
| Framework Markdown conformance | affected `CLI-DESIGN-007` | Focused `MarkdownDocumentParser` source and parser evidence | Affected Framework Markdown regressions and final C# conformance sweep. |
| Candidate Extension sharing | `CLI-SHARED-009` | Exact semantic and consumer comparison | Focused rendering regression and locality review; no-op is valid. |
| Test evidence tiers | `CLI-TEST-010` | Test-project/path/fixture audit and focused Unit/Integration selections | Real `TemporaryWorkspace` boundary evidence, traits, display names, counts, and affected regressions. |
| Deterministic cancellation | `CLI-TEST-011` | Repeated focused Integration evidence at a deterministic barrier | Residual/no-write and affected Route Init regressions. |
| Durable test identity | `CLI-TEST-012` | Exact rename and discovery audit | Focused project test execution with unchanged counts and behavior. |
| Candidate Find test structure | `CLI-TEST-013` | Focused scenario/readability/coverage comparison | No-op is valid; if changed, run affected focused tests and inspect fixture ownership. |

The future Task Mastermind must record its per-Task applicability check before
mutation. Serialization, help, test-tier, or shared-capability changes trigger
the complete managed and supported `linux-x64` Native AOT gates at the recorded
material boundary. Focused evidence remains the default between those gates.
No current document-only authoring action claims any executable evidence.

## Execution Capsule

- Current owner: Overseer pending assignment to one dedicated Task Mastermind.
- Current boundary: Active task preflight; no production or test mutation has
  started.
- Dependencies: Accepted Route Create integration after its protected lane has
  supplied the `RouteCreateJsonContext` and Route-help predecessor slices;
  exact residual serializer/help decisions; current Architecture and
  contracts; complete C# directive reading by every C# implementer/reviewer.
- Focused evidence: Per-sub-batch source, call-surface, test-tier, focused
  behavior, and affected-boundary checks listed above.
- Integration or full gate: Sequential sub-batches; complete managed and
  supported `linux-x64` Native AOT at the material serialization/help,
  shared-capability, test-architecture, or final acceptance trigger.
- Review budget: Maximum one independent final Sol/xhigh architecture and
  correctness review, `QR-R1` (unconsumed). Targeted rechecks of accepted
  findings are part of the one correction cycle, not extra review units.
- Council budget: Zero; no council round is authorized or consumed.
- Correction budget: One grouped correction cycle, `QR-C1` (unconsumed),
  returned to the original implementation owner where possible.
- Consumed IDs: This document authoring action consumed no implementation,
  review, council, or correction ID. `QR-R1` and `QR-C1` remain available for
  the future remediation Task.
- Stop conditions: An unresolved product/public-contract/lifecycle/safety or
  cross-task architecture choice; a protected Route Create integration change
  without an accepted decision; a required new dependency/project/test tier;
  reflection, dynamic dispatch, custom parser/writer, generic route engine, or
  stronger hostile-process guarantee; inability to prove a test tier; a
  destructive/remote/publishing action; or exhaustion of distinct correction
  strategies.
- Next action: Independently inspect the exact integrated baseline and assigned
  worktree, read all three C# directive files,
  consume and revalidate the two protected predecessor slices, and issue one
  closed packet for the residual serializer/help work followed by each
  sequential sub-batch. The Task Mastermind supervises any delegated
  implementers or reviewers, preserves one implementation owner per coherent
  sub-batch, and returns compact evidence. Reviewers remain read-only and
  return stable findings to the Task Mastermind.

## Progress And Evidence

- Current result: The accepted thirteen-finding audit is now one Active
  remediation Task. No remediation production or test source has changed.
- Evidence: Route Create is integrated at `19412d2`, exact tree `2bbba7e`. The
  finding identities, scopes, consequences, corrections,
  ownership, caveats, order, and acceptance ladder above preserve the accepted
  audit synthesis. The sealed Route Create handoff remains untouched.
- Blockers: None currently. Route Update remains gated on this Task's accepted
  completion.
- Residual risk: The actual findings remain unresolved until the future
  remediation Task is executed and accepted. Candidate findings may be closed
  without code after fresh proof.

## Completion And Closeout

The remediation Task is complete only when every actual finding has a verified
behavior-preserving correction or an explicitly accepted disposition, both
candidates have fresh proof or a recorded no-op disposition, and the final
review finds no material regression. The owner must verify the exact changed
paths, dependency direction, local/shared placement, canonical authority,
truthful Unit/Integration classification, deterministic cancellation, durable
test names, focused regressions, and the required managed and supported
`linux-x64` Native AOT gates. The owner must update the affected Task,
Checkpoint, Plan, and Overseer continuity records without hand-editing generated
`Entries`, and must report any direct integration-neighborhood expansion.
