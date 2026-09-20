---
open-forge:
  description: Remediate the accepted first-pass CLI architecture, C# design, authority, and test-evidence findings after Route Create and before Route Update
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Audit, Architecture, Refactoring, Testing, Review]
---

# CLI Quality Remediation

## Task State

- State: Complete and squash-integrated at
  `862cbf2a847b5adac77c6923e51f2c28c315415f`, exact tree
  `571f104f507c3f72b7404cc0dacd86c818ec0a2e`, from reviewed implementation
  `a4ccf19a3489d06f20a0fd940219be1acb22646d`, exact tree
  `97254e655f51ab421dacc8eff7a8c93f726f2625`. All thirteen findings and
  candidates have accepted corrections or dispositions. Final Sol/xhigh
  `QR-R1` reported one evidence-tier regression, `QR-R1-001`; its bounded
  correction and same-reviewer narrow revalidation are accepted. Route Update
  is the next sequential mutation Task.
- Responsible role: Dedicated Overseer-managed Task Mastermind in isolated
  branch `codex/cli-quality-remediation`.
- Task source: Maintainer-accepted synthesis of the strategic first-pass PR-style audit of the replacement CLI `development` branch.
- Last updated: 2026-09-01.

Route Create is Complete and squash-integrated at `19412d2`. It supplies two
command-local predecessor slices: the `RouteCreateJsonContext` slice, with no
legacy JSON-context migration, and the Route half of group-help cleanup. This
Task consumed and revalidated those predecessor slices, corrected the residual
eleven command-local contexts/Shell aggregate and Extension help half, and
closed findings `CLI-DESIGN-003` through `CLI-TEST-013`. Nothing changed the
sealed Route Create handoff.

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

| Relationship            | Link                                                                                                            | Relevance                                                                                                                                                                                                   |
| ----------------------- | --------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Parent program          | [Complete The Replacement CLI](00-cli-development.md)                                                           | Provides the accepted CLI outcome and project authority.                                                                                                                                                    |
| Completed predecessor   | [Route Mutation Commands](route-mutation/_route-mutation.md) and [Route Create](route-mutation/route-create.md) | Route Create is Complete at `19412d2` and supplies the `RouteCreateJsonContext` and Route-help predecessor slices. This Task consumes/revalidates that evidence and owns the residual serializer/help work. |
| Next command            | [Route Update](route-mutation/route-update.md)                                                                  | This Task must complete before Route Update behavior.                                                                                                                                                       |
| Plan                    | [Development Plan](../plan.md)                                                                                  | Records the program sequence and gates.                                                                                                                                                                     |
| Checkpoint              | [CLI Development Checkpoint](../../checkpoints/cli-development.md)                                              | Records resumable program state.                                                                                                                                                                            |
| Overseer continuity     | [CLI Overseer Memory](../overseer-memory.md)                                                                    | Records the live orchestration horizon.                                                                                                                                                                     |
| Candidate workflow idea | [Continuous Targeted Review Orchestration](../../../emerging/ideas/continuous-targeted-review-orchestration.md) | Explores a review-only parallel overlay; it is not a current workflow rule.                                                                                                                                 |
| Audit source            | Maintainer-accepted audit synthesis in the assigning packet                                                     | Supplies the final finding IDs, classifications, evidence scopes, and order preserved here.                                                                                                                 |

The audit snapshot is the exact clean `development` baseline
`a7fc99fc9675d4020eb864a37098bb84c64d33f5`. The original line locations below
are retained as audit-baseline evidence. Current symbol anchors are added where
line drift would otherwise make a future recheck brittle.

## References And Authority

| Source                                                                                                                                                                        | Question it answers                                                                   | Status or authority             | May this task change it?                                                         |
| ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------- | ------------------------------- | -------------------------------------------------------------------------------- |
| [Replacement CLI Architecture](../../../crystallized/documents/cli/architecture.md)                                                                                           | What structure, dependency direction, locality, and evidence boundaries are accepted? | Accepted current architecture   | No; findings may be corrected against it.                                        |
| [CLI contract set](../../../crystallized/documents/cli/_cli.md)                                                                                                               | Which replacement command contracts and authority map apply?                          | Accepted current contract route | No.                                                                              |
| [C# Directives](../../../../directives/csharp/_csharp.md), [Callable Design](../../../../directives/csharp/design.md), and [C# Style](../../../../directives/csharp/style.md) | Which source and test design rules apply?                                             | Binding workspace directives    | No; future agents must read all three completely.                                |
| [Review Evidence](../../../../directives/review-evidence.md)                                                                                                                  | How are stable findings, independent review, correction, and recheck handled?         | Binding workspace directive     | No.                                                                              |
| [Source Locality](../../../../directives/source-locality.md)                                                                                                                  | When does behavior remain local or move to a nearest shared scope?                    | Binding workspace directive     | No.                                                                              |
| [CLI Implementation](../../../../directives/open-forge/cli/implementation.md)                                                                                                 | Which replacement CLI, AOT, mutation, and test rules apply?                           | Binding CLI directive           | No.                                                                              |
| [Test Evidence Integrity](../../../../directives/open-forge/testing/evidence-integrity.md)                                                                                    | Which evidence tier and test-isolation rules apply?                                   | Binding testing directive       | No.                                                                              |
| [Route Create Green Handoff](../../handoffs/2026-08-31_cli-route-create-green.md)                                                                                             | What accepted and sealed Route Create Green boundary must be preserved?               | Sealed contextual handoff       | No; it is protected and must not be edited.                                      |
| [Development Plan](../plan.md)                                                                                                                                                | What is the accepted order and full-gate policy?                                      | Active working plan             | Only its minimal sequencing language may be updated by the responsible Overseer. |
| [CLI Development Checkpoint](../../checkpoints/cli-development.md)                                                                                                            | What is the current route-mutation state?                                             | Active working checkpoint       | Only its minimal sequencing language may be updated by the responsible Overseer. |

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

| Finding family                        | Semantic owner                                                      | Placement direction                                                                                                                                                                                          |
| ------------------------------------- | ------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `CLI-ARCH-001`                        | Shared Shell serialization boundary with command-owned graphs       | Keep Shell metadata Shell-owned; register concrete graphs through command-local source-generated contexts at the protected integration boundary.                                                             |
| `CLI-ARCH-002`                        | Shared command-symbol/help relationship                             | Let standard help derive live children from the symbol graph; keep bounded unavailable/future notes local to the relevant help section.                                                                      |
| `CLI-DESIGN-003`                      | Command-local Find, References, and Route List behavior             | Split the service-bag records into cohesive typed capabilities at the nearest command scope; Route List receives only this finding. Do not create a repository-wide service bag or generic operation engine. |
| `CLI-DESIGN-004`                      | Command-local Route Init and Install planner behavior               | Split each planner into thin local orchestration over cohesive capabilities; do not create a generic route or install planner.                                                                               |
| `CLI-DESIGN-005`                      | Command-local Extension Inspect behavior                            | Split topical result/model and builder/projection capabilities while preserving command locality.                                                                                                            |
| `CLI-DESIGN-006`                      | Command-local Find and References callable design                   | Replace long argument/reference trains with typed local stage inputs/results and cohesive capabilities.                                                                                                      |
| `CLI-DESIGN-007`                      | Route Init, References, Find, and Framework Markdown C# conformance | Correct each local suppression/conditional at its nearest owner; `MarkdownDocumentParser` remains Framework Markdown-owned and gets focused parser evidence.                                                 |
| `CLI-AUTH-008`                        | Shared lifecycle schema authority                                   | Consume `LifecycleSchema.RelativePath`; do not create a second path authority.                                                                                                                               |
| `CLI-SHARED-009`                      | Extension-family rendering support                                  | Consider promotion only to `Commands/Extension/Shared/Rendering/ExtensionTextEscaping` after identical semantics are proved.                                                                                 |
| `CLI-TEST-010` through `CLI-TEST-013` | Test-project boundary and command-local test structures             | Correct project tier, deterministic evidence, and durable names in the nearest test scope; keep pure Unit and real-boundary Integration evidence distinct.                                                   |

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

- Completed Route Create production/tests, its sealed Handoff, or its protected
  integration decision, except for the maintainer-authorized physical
  co-location of `RouteCreateJsonContext` in its matching renderer after QR7.
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

Expected paths are forecasts for this remediation Task, not an allowlist.
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

| Finding          | Direct production consumers                                                                            | Direct evidence neighborhood                                                                                                                                                                             |
| ---------------- | ------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `CLI-DESIGN-003` | `FindOperationComponents`, `ReferencesOperationComponents`, and `RouteListOperationComponents`         | `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/{Find,References,Route/List}/**` and matching Integration consumers; Route List is in scope for this finding only.                             |
| `CLI-DESIGN-006` | Find matcher/result/projection stages and `ReferencesFindingFactory.AddFinding`                        | `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/{Find,References}/**` and matching Integration consumers; it does not apply to Route List.                                                     |
| `CLI-DESIGN-007` | Route Init binding/planning, References selector/binding, Find binding, and Framework Markdown parsing | Route Init, References, and Find focused regressions plus `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Documents/Markdown/**` parser evidence; it does not apply to Install or Route List. |

### Protected Paths And Authorities

- Integrated Route Create source and tests remain outside remediation ownership
  except for direct revalidation of the accepted `RouteCreateJsonContext` and
  Route-help predecessor slices, plus the maintainer-authorized physical
  co-location of that context in `RouteCreateJsonRenderer.cs` after QR7. This
  does not reopen Route Create behavior or wire meaning.
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

| ID  | Kind         | Claim, required state, resource, or risk                                                                                                       | Validation, availability, or signal                                                                                                                                | Owner or source                              | Response if false or triggered                                                 |
| --- | ------------ | ---------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------- | ------------------------------------------------------------------------------ |
| A1  | Prerequisite | Route Create must reach its accepted command-local Green/focused evidence and protected integration boundary before this separate Task starts. | Satisfied at integration `19412d2`, exact tree `2bbba7e`; Route Create supplies the `RouteCreateJsonContext` and Route-help predecessor slices.                    | Route Create Task, Checkpoint, and Git state | Reopen only if exact baseline inspection contradicts the recorded integration. |
| A2  | Prerequisite | The current Architecture, contracts, mutation foundation, and project graph remain accepted.                                                   | Read the linked current sources and inspect the actual baseline before mutation.                                                                                   | Overseer and Task Mastermind                 | Return a project change request before changing shared meaning.                |
| A3  | Boundary     | The audit is a strategic first-pass PR-style audit, not exhaustive deep scrubbing.                                                             | Finding scope and no-findings caveat below remain visible.                                                                                                         | Accepted audit synthesis                     | Do not expand the task into a general codebase cleanup.                        |
| A4  | Resource     | Every C# implementer and reviewer can independently read the complete three C# directive files.                                                | Every implementation or review packet names all three exact paths and requires acknowledgement before work.                                                        | Task Mastermind                              | Stop that C# boundary and return a task gap if the rules cannot be loaded.     |
| A5  | Evidence     | Serialization/help and test-tier changes are material boundaries.                                                                              | Applicability check selects affected Unit/Integration/public evidence and the complete managed plus supported `linux-x64` Native AOT gate at the material trigger. | Task Mastermind                              | Do not accept from source inspection or partial tests alone.                   |
| A6  | Candidate    | Similar Extension escaping and Find test scenarios may not justify promotion by themselves.                                                    | Fresh proof must show identical semantic ownership or measurable evidence value.                                                                                   | Task Mastermind and reviewer                 | Close `CLI-SHARED-009` or `CLI-TEST-013` as no-op when proof is absent.        |
| A7  | Safety       | No remote, destructive, dependency-installation, publication, or delivery action is part of this task.                                         | Execution-safety inspection and exact-path local edits.                                                                                                            | All delegated owners                         | Return `AUTHORIZATION_REQUIRED` or stop at the boundary.                       |
| A8  | Recovery     | A refactor or tier correction must preserve behavior and leave no unverified partial state.                                                    | Focused regressions, diff inspection, and full-gate trigger evidence.                                                                                              | Original implementation owner                | Group the finding for correction and recheck; do not broaden scope silently.   |

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

The separate remediation Task ran after Route Create integration. Route Create
supplied the accepted `RouteCreateJsonContext` predecessor slice and the Route
half of group-help cleanup. This Task consumed and revalidated those slices,
owned the residual serializer/help corrections, and executed the remaining
batches before Route Update. The remediation followed this order while
preserving the distinction between actual defects and candidates:

| Order | Sub-batch                                       | Finding IDs                                                   | Dependency and completion boundary                                                                                                                                                                                                                                                                                                                                                                                                         |
| ----- | ----------------------------------------------- | ------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1     | Serialization                                   | `CLI-ARCH-001`                                                | On integrated baseline `19412d2`, remove the residual eleven command-local contexts/Shell aggregate; consume and revalidate the supplied `RouteCreateJsonContext` predecessor slice without duplicating correction.                                                                                                                                                                                                                        |
| 2     | Group help                                      | `CLI-ARCH-002`                                                | On integrated baseline `19412d2`, correct the residual Extension help half; consume and revalidate the supplied Route half, preserve the exact child symbol graph, and exclude Root Discovery/`CLI-D076`.                                                                                                                                                                                                                                  |
| 3     | Test evidence tiers                             | `CLI-TEST-010`                                                | Correct Unit versus Integration claims before later evidence is accepted.                                                                                                                                                                                                                                                                                                                                                                  |
| 4     | Route Init planner                              | `CLI-DESIGN-004`, affected `CLI-DESIGN-007`, `CLI-AUTH-008`   | Split local planner responsibilities, remove affected conformance defects, and consume canonical lifecycle authority.                                                                                                                                                                                                                                                                                                                      |
| 5     | Install planner                                 | `CLI-DESIGN-004`                                              | Split the Install planner locally; do not attribute Route Init/Framework Markdown conformance or lifecycle authority work to Install.                                                                                                                                                                                                                                                                                                      |
| 6     | Extension Inspect                               | `CLI-DESIGN-005`, `CLI-SHARED-009`                            | Refactor topical local clusters; evaluate Extension-family escaping promotion and close the candidate if proof is insufficient.                                                                                                                                                                                                                                                                                                            |
| 7     | Find/References/Route List                      | `CLI-DESIGN-003`, `CLI-DESIGN-006`, affected `CLI-DESIGN-007` | Reduce service bags and long call surfaces with typed local stages and capabilities; Route List receives `CLI-DESIGN-003` only, while `CLI-DESIGN-006` and affected `CLI-DESIGN-007` remain Find/References-only.                                                                                                                                                                                                                          |
| 7a    | Serialization context co-location follow-up     | `CLI-ARCH-001` physical-quality correction                    | After QR7 and before QR8, co-locate each of the twelve command-local `*JsonContext` declarations in its matching `*JsonRenderer.cs`, delete the standalone context files, and re-run full serialization, managed, and supported AOT evidence. Preserve exactly one command context, type info, options, schema, and renderer behavior; no global, family, or generic context. The narrow Route Create context/renderer move is authorized. |
| 8     | Deterministic cancellation                      | `CLI-TEST-011`                                                | Replace watcher-race evidence with deterministic boundary/barrier evidence.                                                                                                                                                                                                                                                                                                                                                                |
| 9     | Durable test naming and Find test restructuring | `CLI-TEST-012`, `CLI-TEST-013`                                | Rename active `*RedTests`; evaluate typed scenarios/focused assertions and close the candidate if no material gain is proved.                                                                                                                                                                                                                                                                                                              |

The repeated `CLI-DESIGN-004` in sub-batches 4 and 5, and the affected C#
conformance locations in sub-batch 7, are one finding with multiple local
consumers, not new findings. `CLI-AUTH-008` is Route Init-only and is a
guardrail applied at its affected planner/application boundary, not a new
lifecycle feature. `CLI-DESIGN-003` is the only listed finding applied to
Route List. Each sub-batch keeps its own local behavior and evidence; shared or
protected integration is reviewed sequentially.

## Evidence And Acceptance Matrix

| Evidence class                                | Findings                                                      | Cheapest decisive evidence                                                                                        | Additional acceptance evidence                                                                                                              |
| --------------------------------------------- | ------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| Shared serializer/help architecture           | `CLI-ARCH-001`, `CLI-ARCH-002`                                | Source/dependency/locality inspection plus focused serializer/help tests                                          | Published managed process JSON/help, affected regressions, and supported `linux-x64` Native AOT execution.                                  |
| Route List component design                   | `CLI-DESIGN-003`                                              | Direct consumer/call-surface and namespace audit for `RouteListOperationComponents` plus Route List Unit evidence | Affected Route List Integration/public regressions and no dependency-direction drift.                                                       |
| Find/References design and conformance        | `CLI-DESIGN-003`, `CLI-DESIGN-006`, affected `CLI-DESIGN-007` | Direct call-surface, namespace, ownership, and C# conformance audits plus affected Unit tests                     | Affected Find/References Integration/public regressions; full managed/AOT gate when a shared or composition boundary is materially changed. |
| Route Init design, conformance, and authority | `CLI-DESIGN-004`, affected `CLI-DESIGN-007`, `CLI-AUTH-008`   | Planner ownership, C# conformance, and canonical-authority audits plus affected Unit tests                        | Affected Route Init Integration/public regressions and lifecycle/path behavior; `CLI-AUTH-008` remains Route Init-only.                     |
| Install planner design                        | `CLI-DESIGN-004`                                              | Install planner ownership and call-surface audit plus affected Unit tests                                         | Affected Install Integration/public regressions; no Install attribution for `CLI-DESIGN-007` or `CLI-AUTH-008`.                             |
| Framework Markdown conformance                | affected `CLI-DESIGN-007`                                     | Focused `MarkdownDocumentParser` source and parser evidence                                                       | Affected Framework Markdown regressions and final C# conformance sweep.                                                                     |
| Candidate Extension sharing                   | `CLI-SHARED-009`                                              | Exact semantic and consumer comparison                                                                            | Focused rendering regression and locality review; no-op is valid.                                                                           |
| Test evidence tiers                           | `CLI-TEST-010`                                                | Test-project/path/fixture audit and focused Unit/Integration selections                                           | Real `TemporaryWorkspace` boundary evidence, traits, display names, counts, and affected regressions.                                       |
| Deterministic cancellation                    | `CLI-TEST-011`                                                | Repeated focused Integration evidence at a deterministic barrier                                                  | Residual/no-write and affected Route Init regressions.                                                                                      |
| Durable test identity                         | `CLI-TEST-012`                                                | Exact rename and discovery audit                                                                                  | Focused project test execution with unchanged counts and behavior.                                                                          |
| Candidate Find test structure                 | `CLI-TEST-013`                                                | Focused scenario/readability/coverage comparison                                                                  | No-op is valid; if changed, run affected focused tests and inspect fixture ownership.                                                       |

The Task Mastermind recorded the per-Task applicability check below before
production or test mutation. Serialization, help, test-tier, or shared-capability changes trigger
the complete managed and supported `linux-x64` Native AOT gates at the recorded
material boundary. Focused evidence remains the default between those gates.
No current document-only authoring action claims any executable evidence.

## Execution Capsule

- Current owner: Dedicated CLI Quality Remediation Task Mastermind.
- QR7B3 direct-owner checkpoint: The Task Mastermind's complete independent
  C# directive acknowledgement remained current before References binding
  conformance mutation. The two postfix suppressions are replaced with
  explicit invariant patterns, and the selector reader's nested missing/empty
  cause decision is flat and named. The preliminary packet said the invariant
  patterns preceded workspace handling; implementation correctly preserves
  the actual old evaluation order: typed invalid-result formation, bound
  workspace invariant, nullable direction invariant, source invariant, then
  request construction. Invalid-result formation still consumes the already
  established invocation workspace exactly as before but performs no later
  bound-workspace invariant or domain request. Exception type, message, and
  parameter name remain exact. No test or other production file changed.
- QR7B2 direct-owner checkpoint: The Task Mastermind's complete independent
  C# directive acknowledgement remained current before finding-formation
  mutation. The fourteen-parameter `ReferencesFindingFactory.AddFinding`
  surface now consumes one immutable finding input with required code and
  cause plus named optional facts. Seven direct callers and the `AddEvent`
  delegate use the typed input; all nineteen `AddEvent` callers remain
  unchanged. Candidate, selector, location, status, nullability, validation,
  append-order, and result semantics remain owned by the existing
  `ReferencesFinding`. QR7B3 binder/selector conformance remains untouched.
- QR7B1 direct-owner checkpoint: The Task Mastermind's complete independent
  acknowledgement of `.agents/directives/csharp/_csharp.md`,
  `.agents/directives/csharp/design.md`, and
  `.agents/directives/csharp/style.md` remained current before References
  mutation. The eight-service `ReferencesOperationComponents` bag is removed.
  Boundary/reference/universe resolution, layer inspection, and destination
  resolution now own their exact delegates and facts behind cohesive
  References-local capabilities; one shared Markdown-parser callable retains
  the identical authority consumed by inspection and destination resolution.
  `ReferencesOperation` consumes exactly four capabilities and retains the
  original orchestration, coverage, cancellation, failure, and final result
  boundary. QR7B2 finding formation and QR7B3 binder/selector conformance
  remain untouched.
- QR7A direct-owner checkpoint: The Task Mastermind's complete independent
  acknowledgement of `.agents/directives/csharp/_csharp.md`,
  `.agents/directives/csharp/design.md`, and
  `.agents/directives/csharp/style.md` remained current before Route List
  mutation. The six-service `RouteListOperationComponents` bag is removed.
  Source inventory, route facts, primary selection, and optional loader-root
  selection now belong to one Route List-local source resolver; topology,
  coverage, and result formation belong to one Route List-local result
  composer. The coordinator has two dependencies and retains the original
  attempted-selection update point, cancellation rethrow, and failure-result
  boundary. No generic route engine, other command, public surface, or test was
  changed.
- QR6-S3B direct-owner checkpoint: Under the active normalized-owner efficiency
  circuit, the Task Mastermind directly moved path comparison, dependency
  comparison, and root mode/side/state projection behind three cohesive local
  capabilities. Path findings still precede dependency findings; installed
  closure is read once for the root and both comparison sides. The comparison
  facade retains exactly eight temporary S3C surfaces. The Task Mastermind's
  complete independent C# directive acknowledgement remained current; no
  separate owner or protected consumer was introduced.
- QR6-S3A direct-owner checkpoint: The Task Mastermind's complete independent
  acknowledgement of `.agents/directives/csharp/_csharp.md`,
  `.agents/directives/csharp/design.md`, and
  `.agents/directives/csharp/style.md` remained current before direct S3A
  mutation. One continuous Sol/xhigh implementation owner independently
  acknowledged all three files and returned the exact comparison map, but was
  circuited with zero scoped edits after its accepted gate produced no
  implementation progress. The Task Mastermind then moved installed-closure,
  fingerprint, and generated-Markdown facts directly behind cohesive local
  capabilities while retaining compile-green comparison forwards. Exact
  fingerprint, generated-state, finding, order, and nullable policy remains
  unchanged.
- QR6-S2D direct-owner checkpoint: The final top-down result-formation audit
  found the `178`-line facade free of private helpers and temporary wrappers;
  its three boundary forwards remain the explicitly frozen S2A compatibility
  surfaces. S2B and S2C remain cohesive typed stages rather than speculative
  line-count splits. The only bounded findings were one unread
  `AvailableMatches` stage property and source/available naming drift at the
  S2C connection. Under the accepted efficiency circuit, the Task Mastermind
  removed that dead seam and normalized only stage-local `AvailablePackage`
  and `AvailableClosure` names directly. No product, result, JSON, branch,
  order, nullability, or consumer meaning changed.
- QR6-S2C direct-owner checkpoint: The Task Mastermind's complete independent
  acknowledgement of `.agents/directives/csharp/_csharp.md`,
  `.agents/directives/csharp/design.md`, and
  `.agents/directives/csharp/style.md` remained current before S2C mutation.
  The frozen graph moved dependency traversal, source closure, declared-path
  projection, current-path findings, aggregate path state, and event path facts
  behind one capability-specific typed stage input/result. Traversal, finding,
  source/path order, conflict, state-precedence, and nullable-event semantics
  remain unchanged. The S2B source-identity capability is now consumed directly;
  the temporary facade delegate and obsolete dependency input are absent.
  Repeated delegated structure-owner gate/execution stalls triggered the
  accepted efficiency circuit, so this normalized slice was implemented
  directly without another owner.
- QR6-S2B direct-owner checkpoint: The Task Mastermind's complete independent
  acknowledgement of `.agents/directives/csharp/_csharp.md`,
  `.agents/directives/csharp/design.md`, and
  `.agents/directives/csharp/style.md` remained current before S2B mutation.
  The frozen graph moved subject, source, lifecycle, installed, available, and
  boundary-finding formation behind two capability-specific typed stage inputs
  and results. Public `Build`/`Event` signatures, nullable event defaults,
  package selection, branch/finding order, and the existing local findings
  append semantics remain unchanged. All dependency/closure/path bodies stayed
  protected; their two existing source-identity calls consume one temporary
  facade delegate until the next slice. The delegated S2B structure owner was
  circuited without a scoped edit after failing to return the mandatory gate,
  so the Task Mastermind completed the accepted slice directly without a
  replacement owner.
- QR6-S2A direct-owner checkpoint: Before the first direct S2A production edit,
  the Task Mastermind independently read and acknowledged the complete
  `.agents/directives/csharp/_csharp.md`,
  `.agents/directives/csharp/design.md`, and
  `.agents/directives/csharp/style.md` files. The frozen Extension Inspect-only
  map is (1) a boundary result factory for invalid, workspace-blocked, and typed
  empty results behind the existing facade; (2) one honest standalone finding
  policy owning sort, status, add, create, and normalization; and (3) finite
  framework, source, lifecycle, package, path, and side mappings. `Event`
  remains facade-owned, nullable source/business match-context decisions remain
  local, and the comparison builder, S1 result models, JSON projection,
  escaping, tests, and S2B/S2C methods remain protected. The delegated S2A
  structure owner was circuited after acknowledging this exact map but before
  returning a connected change; the Task Mastermind completed the frozen slice
  directly without a replacement owner.
- QR5 direct-owner checkpoint: Before the first Install production edit, the
  Task Mastermind independently read and acknowledged the complete
  `.agents/directives/csharp/_csharp.md`,
  `.agents/directives/csharp/design.md`, and
  `.agents/directives/csharp/style.md` files. The frozen Install-only graph is
  (1) inspection/basis for payload, intended state, lifecycle, recovery,
  targets, and `.agents` expectation; (2) managed-current evaluation for
  intended lifecycle, exactness, and preservation; (3) establishment planning
  for occupant, managed-block, file/lifecycle-effect, and directory decisions;
  and (4) result projection for boundaries, findings, completed plans,
  evidence, and effect identities. The primary
  `(PhysicalPathResolver, LifecycleStore, RecoveryBundleCatalogue)` constructor,
  `InstallOperationFactory` composition, and `InstallOperation.BuildAsync`
  consumer remain unchanged. Focused acceptance owns existing Install Unit
  contracts, Install Integration planning/operation/preservation/revalidation,
  and `PublishedInstallProcessTests`; prior QR1 renderer/context and QR3 tier
  edits remain protected. Two delegated owners were circuited before QR5 edits,
  so the Task Mastermind continues directly from this frozen map.
- Current boundary: `CLI-ARCH-001` serialization, `CLI-ARCH-002` group help,
  `CLI-TEST-010` evidence tiers, and the Route Init planner sub-batch for
  `CLI-DESIGN-004`, affected `CLI-DESIGN-007`, and `CLI-AUTH-008` are corrected
  and revalidated. The Install planner consumer of `CLI-DESIGN-004` is also
  corrected and revalidated. Extension Inspect `CLI-DESIGN-005` result-model
  structure and its first two result-formation slices are corrected and
  revalidated, as is its dependency/closure/path slice. The final
  result-formation facade slice is now also corrected and revalidated;
  comparison formation, counts, actionability, and the final one-build facade
  are corrected and revalidated. JSON document projection and finite wire
  vocabulary are also corrected and revalidated, closing `CLI-DESIGN-005`.
  The three byte-identical Extension-family text escapers now consume one
  Extension-shared neutral mechanism, closing candidate `CLI-SHARED-009` and
  the Extension Inspect sub-batch. The Route List portion of
  `CLI-DESIGN-003` is corrected and revalidated. The References portion is now
  also corrected and revalidated; only its Find portion remains open. The
  References long finding-call portion of `CLI-DESIGN-006` is corrected and
  revalidated; its Find portion remains open. The References portion of
  affected `CLI-DESIGN-007` is corrected and revalidated, closing the
  References sub-batch.
- Applicability: This assured, behavior-preserving refactor affects the
  non-shipping local CLI and its tests. Git provides the practical recovery
  boundary. Accepted public wire, lifecycle, filesystem-safety, and threat
  meaning remain unchanged. The pinned compiler, source generator, serializer,
  parser, BCL, and test platform provide the required capabilities through
  their ordinary supported behavior. Existing Shell, command, Framework,
  `TemporaryWorkspace`, and source-generated serialization authorities remain
  in use; command semantics stay local. Exceptional machinery: none. Each
  sub-batch uses its cheapest decisive Unit, Integration, EndToEnd, and static
  evidence, with complete managed and supported `linux-x64` Native AOT gates at
  the material serialization/help, test-architecture, shared-capability, and
  final acceptance boundaries.
- Dependencies: Accepted Route Create integration after its protected lane has
  supplied the `RouteCreateJsonContext` and Route-help predecessor slices;
  exact residual serializer/help decisions; current Architecture and
  contracts; complete C# directive reading by every C# implementer/reviewer.
- Focused evidence: Per-sub-batch source, call-surface, test-tier, focused
  behavior, and affected-boundary checks listed above.
- Integration or full gate: Sequential sub-batches; complete managed and
  supported `linux-x64` Native AOT at the material serialization/help,
  shared-capability, test-architecture, or final acceptance trigger.
- Review budget: The independent final Sol/xhigh architecture and correctness
  review, `QR-R1`, is consumed over exact candidate `e11b225e`, tree
  `a19fa69b`; same-reviewer narrow revalidation accepts corrected candidate
  `a4ccf19a`, tree `97254e65`, after closing High `QR-R1-001`. A
  maintainer-authorized adherence trial
  added four independent topic units over exact checkpoint `f324beae`, tree
  `0def1aba`, to `1f8708f`, tree `306324e2`: `QR-R2` C# conformance,
  `QR-R3` architecture/ownership/refactoring, `QR-R4` behavior/contract
  equivalence, and `QR-R5` test/evidence quality. Each unit is valid only after
  its reviewer confirms Git-object evidence, corrected provenance, full
  independent reads of `_csharp.md`, `design.md`, and `style.md`, and no
  dirty-filesystem or LSP evidence. The coordinator consumes no unit because it
  only schedules and synthesizes; the Task Mastermind retains disposition and
  repair ownership. Targeted rechecks of accepted findings remain part of the
  one grouped correction cycle rather than additional review units.
- Council budget: Zero; no council round is authorized or consumed.
- Correction budget: One grouped correction cycle, `QR-C1` (closed), returned
  to one continuous Sol/xhigh implementation owner across C1A through C1D and
  a bounded Luna/max mechanical owner for C1E. The single bounded `QR-R1-001`
  correction returned to the original C1 owner does not consume another review
  or grouped correction unit.
- Consumed IDs: Preflight consumed no review, council, or correction ID.
  Checkpoint topic reviews `QR-R2` through `QR-R5` are consumed over exact
  checkpoint `f324beae`, tree `0def1aba`, to `1f8708f`, tree `306324e2`.
  Holistic final review `QR-R1` is consumed over `e11b225e`, tree `a19fa69b`,
  and reported High `QR-R1-001`; its narrow revalidation accepts the correction
  in `a4ccf19a`, tree `97254e65`, with no new material finding. Grouped
  correction `QR-C1` is consumed and closed; C1A through C1E are closed.
- Accepted checkpoint dispositions: Revalidate every item against the latest
  snapshot, then address them together in phase 10 after phase 9 commits.
  `ARCH-001`/`CSHARP-003` move state-only types from Shared behavior files into
  nearest `Models/<Topic>` owners, organize Extension Inspect model/result
  topics, and place `ReferencesMarkdownParser` under explicit Documents/Parsing
  ownership. `ARCH-002` splits only the two identified dual-responsibility
  Extension Inspect builders; size alone is not a defect. `ARCH-004` replaces
  References `InspectLayersAsync`'s eight-parameter surface with one cohesive
  immutable scan input while keeping mutable outputs and cancellation explicit.
  `CSHARP-001` corrects the raw multiline Extension group-help literal;
  `CSHARP-002` flattens the named dependency-path conditional; and `CSHARP-004`
  adds explicit named enum mappings, undefined-value throws, and evidence at the
  identified Extension Inspect, Find, References, and Route Init sites.
  `ARCH-005` is satisfied by `25e7e62` and closes after revalidation without a
  duplicate edit.
- Accepted shared-source decision: `ARCH-003` may add one small neutral
  `Framework/Sources` source-read session fact and reader shared by Find and
  References because their catalogue, document-reader, and default-selection-
  scope boundary is identical. Source-reference resolution, universe/filter
  logic, findings, and command policy remain local. No generic query engine,
  service bag, dependency injection, or parameterized policy is allowed. The
  correction requires complete managed and supported Native AOT evidence.
  Phase 10 also closes the durable Red-name and Find test-structure candidates;
  `QR-R1` remains reserved for the exact final candidate.
- Stop conditions: An unresolved product/public-contract/lifecycle/safety or
  cross-task architecture choice; a protected Route Create integration change
  without an accepted decision; a required new dependency/project/test tier;
  reflection, dynamic dispatch, custom parser/writer, generic route engine, or
  stronger hostile-process guarantee; inability to prove a test tier; a
  destructive/remote/publishing action; or exhaustion of distinct correction
  strategies.
- Next action: Begin Route Update from integrated baseline `862cbf2a`, exact
  tree `571f104f`.

## Progress And Evidence

- Current result: `CLI-ARCH-001` behavior and ownership are corrected. Shell
  source-generated JSON metadata now registers only Shell-owned
  `CliProcessCompletion`. Twelve command graphs use command-local contexts, and
  the maintainer-directed physical-quality follow-up now co-locates each
  declaration in its matching renderer without changing that graph.
  `CLI-ARCH-002` is also corrected: standard help owns the exact live Route and Extension child
  graphs, while custom group content contains only bounded no-operation and
  unavailable-operation notes. Root Discovery remains excluded.
  `CLI-TEST-010` is corrected: fourteen real filesystem or operating-system
  cases moved from the Unit assembly to four nearest Integration files backed
  by `TemporaryWorkspace`; nine pure Unit methods remain in the original
  three Unit files, and the empty application-integrity Unit file was removed.
  The Route Init portion of `CLI-DESIGN-004`, its affected `CLI-DESIGN-007`
  sites, and `CLI-AUTH-008` are corrected. `RouteInitPlanBuilder` is now an
  83-line command-local orchestrator over inspection, intended-chain,
  prospective-plan, finalization, and result-projection capabilities. The
  original recovery-catalogue constructor seam and direct consumers remain
  unchanged. Route Init suppressions, nested conditional sites, and raw
  lifecycle-path literals are removed; the companion Markdown leaf-inline
  suppression has focused evidence.
  `CLI-DESIGN-004` is fully corrected across its two command consumers.
  `InstallPlanBuilder` is now a 75-line command-local orchestrator over typed
  inspection/basis, managed-current evaluation, establishment planning, and
  result-projection responsibilities. Its original three-dependency
  constructor, `InstallOperationFactory` composition, and `InstallOperation`
  behavior remain unchanged; no generic planner or shared Install policy was
  introduced.
  The first Extension Inspect `CLI-DESIGN-005` slice replaces the 679-line,
  53-type result-model monolith with five same-namespace topical files for
  subject/source/lifecycle, package/dependency, paths,
  comparison/generated/fingerprint, and finding/count/root-result facts.
  Every declaration remains internal and unchanged; no builder, projection,
  source-generation, or consumer file changed in this slice.
  The next Extension Inspect result-formation slice makes the result builder a
  non-partial facade over a dedicated boundary-result factory, a standalone
  finding policy, and finite vocabulary mappings. The original boundary facade
  signatures and `Event` ownership remain unchanged, nullable source and
  match-context policy stay local, and the comparison builder is untouched.
  The legacy five-parameter empty-result call and partial-file concealment are
  absent. `CLI-DESIGN-005` remains open for its later result-formation,
  comparison, and JSON projection slices.
  Subject/source/lifecycle/installed/available facts and boundary findings now
  form in a command-local subject/package capability behind two typed stage
  inputs. The result-builder facade is `630` lines and keeps its original
  `Build` and `Event` signatures; both consumers use the new capability while
  nullable event evidence continues to retain the boundary-result defaults.
  One narrow source-identity delegate remains temporarily so the protected
  dependency/closure/path bodies are unchanged until their own slice.
  Dependency traversal, source closure, declared/current path projection, and
  aggregate/event path facts now form behind one typed dependency/path stage.
  The result-builder facade is `178` lines, all former formation bodies are
  absent, and it orchestrates the subject/package, dependency/path, comparison,
  finding, count, next-action, and final result capabilities. Final S2D still
  owns the frozen old-vs-new facade/call-surface audit and any bounded
  reconnection refinement; `CLI-DESIGN-005` is not yet closed.
  The final S2D audit removed the only dead result-formation stage property and
  normalized the selected-source stage names to `AvailablePackage` and
  `AvailableClosure`. The facade remains `178` lines with its original
  `Build`, `Event`, and three boundary signatures. Result formation is closed;
  `CLI-DESIGN-005` remains open only for comparison and JSON projection.
  The first comparison sub-slice extracts installed-package closure,
  baseline/current/intended fingerprint formation, and generated Markdown
  region facts. `ExtensionInspectComparisonBuilder` is `657` lines and retains
  its five temporary comparison forwards until the final S3 reconnection.
  Path comparison, dependency comparison, and root mode/side/state projection
  now live behind typed local stages. `ExtensionInspectComparisonBuilder` is
  `164` lines and retains exactly eight temporary surfaces for final S3C
  reconnection. The final comparison sub-slice moves count formation and
  actionability policy behind typed command-local inputs, reconnects result
  formation to one comparison `Build` surface, and places comparison/count
  inputs beside their owning capabilities. `ExtensionInspectComparisonBuilder`
  is `82` lines and `ExtensionInspectResultBuilder` is `177` lines; none of the
  eight temporary surfaces remains. Comparison cohesion is closed, and
  `CLI-DESIGN-005` remains open only for its JSON projection slice. The final
  JSON slice replaces the 610-line mixed projection/vocabulary type with a
  10-line stable `Create` facade, a 280-line DTO document projector, and a
  341-line finite wire-vocabulary authority. JSON rendering still consumes the
  stable facade; the exact fourteen Human/Diagnostic vocabulary calls now
  name the vocabulary owner directly. Schema-v1, presentation DTOs,
  source-generation context, renderer behavior, and escaping remain unchanged.
  `CLI-DESIGN-005` is fully corrected. Fresh identity proof for
  `CLI-SHARED-009` shows the Create, List, and Inspect text escapers normalize
  to the same SHA-256 `53dc840d66673ba556124a17dcc3d74686b82515e5e7d00be7f4982d19f72fc9`.
  Their exact 53-line mechanism now lives at
  `Commands/Extension/Shared/Rendering/ExtensionTextEscaping.cs`; the three
  local copies are removed and the six renderers use exactly fifteen shared
  references. Consumer-specific outer diagnostic limits remain local and
  unchanged. The Extension Inspect sub-batch is closed.
  Route List operation composition no longer uses
  `RouteListOperationComponents`: one 75-line source resolver and one 35-line
  result composer sit behind a 65-line factory/coordinator surface. The
  coordinator consumes exactly two command-local capabilities; resolution and
  composition capability constructors each consume three cohesive
  collaborators. Inventory, route-fact, selection, loader-root, topology,
  coverage, attempted-selection, cancellation, and failure-result order remain
  unchanged. No Route List test or public/schema/source-generation surface was
  changed.
  References operation composition no longer uses
  `ReferencesOperationComponents`. A 79-line source resolver owns boundary,
  reference, and universe formation; the existing layer inspector owns its
  reader and parser behind one typed layer-inspection input; the existing
  destination resolver owns physical-path and strict-UTF8 callables; the
  result builder remains its own capability. The operation constructor has
  four cohesive dependencies, while source, layer, and destination capability
  constructors have three, two, and three dependencies. Only the two direct
  Unit construction sites changed; rendering, JSON, source generation, public
  schemas, finding formation, and binder/selector conformance remain
  protected.
  References finding formation now uses one immutable
  `ReferencesFindingInput` behind the two-parameter `AddFinding` surface.
  Required code/cause and named direction, subject, selector, source/layer/path,
  location, candidate, and conditional-status facts replace positional
  omission across seven direct callers. `AddEvent` remains a stable
  three-parameter boundary with nineteen unchanged callers. No test, public
  result, rendering, JSON, source-generation, B1 composition, or B3 binding
  surface changed.
  References binding now establishes validated source and direction locals
  without postfix suppressions. Invalid findings still form a typed result
  before the later bound-workspace invariant; direction and source invariant
  failure precedence remains exact. Selector occurrence cause formation is a
  flat missing-then-empty decision with unchanged tokens, queues, positions,
  strings, and null-success behavior. The References sub-batch is closed.
- Evidence: The clean accepted baseline is `868860e0`, exact tree `0def1aba`.
  This isolated lane began at the tree-equivalent predecessor object
  `f324beae` and was intentionally not rebased while evidence was active.
  Route Create remains integrated at `19412d2`, exact tree `2bbba7e`. The
  finding identities, scopes, consequences, corrections,
  ownership, caveats, order, and acceptance ladder above preserve the accepted
  audit synthesis. Static inspection finds no Shell-to-Commands import in
  `CliJsonContext` and no command renderer or direct metadata test consuming a
  command graph from that context. Focused generated-serialization Integration
  evidence passes `19/19`. Format verification and a warning-free Release build
  pass. Complete managed Unit `1507/1507`, Integration `720/720`, and EndToEnd
  `146/146` pass with zero skips. Supported `linux-x64` Native AOT root
  publication produces the expected ELF and version; native Integration
  `720/720` and EndToEnd `146/146` pass with zero skips. The sealed Route Create
  handoff and protected Route Create source/tests remain untouched. Focused
  Extension help Integration `8/8` and published EndToEnd `7/7` pass. The final
  public help contains live `list`, `inspect`, and `create` entries from the
  composed graph, no custom `Operations` catalogue, and bounded unavailable
  `install`, `update`, and `remove` notes. Post-help format, warning-free Release,
  complete managed `1507/720/146`, root ELF/version, and native `720/146` gates
  pass with zero failures or skips. For `CLI-TEST-010`, the Unit and Integration
  Release builds pass with zero warnings or errors; Unit passes `1493/1493`,
  the four moved Integration classes pass `14/14`, and EndToEnd passes
  `146/146`, all with zero skips. The first unchanged complete Integration run
  passed `733/734` and failed only the already-recorded `CLI-TEST-011`
  `FileSystemWatcher` race; the immediate unchanged rerun passed `734/734`,
  confirming that finding remains nondeterministic and unresolved rather than
  resolving it. Format verification passes. Supported `linux-x64` Native AOT
  root publication produces the expected ELF and version; native Integration
  passes `734/734` and native EndToEnd passes `146/146`, both with zero skips.
  For the corrected Route Init boundary, Core, Unit, Integration, and EndToEnd
  Release builds pass with zero warnings or errors. Focused Markdown Unit
  passes `67/67`, Route Init Unit `23/23`, Route Init Integration `85/85`, and
  published Route Init EndToEnd `6/6`, all with zero skips. Final format and
  diff verification pass. The complete managed gate passes Unit `1494/1494`,
  Integration `734/734`, and EndToEnd `146/146`; supported `linux-x64` Native
  AOT root publication is an x86-64 ELF and reports `0.0.0-dev`, and native
  Integration `734/734` and EndToEnd `146/146` pass with zero skips. This clean
  Integration observation does not close `CLI-TEST-011`; its recorded failed
  first run and unchanged passing rerun remain the controlling race evidence.
  For the corrected Install boundary, the Release solution build passes with
  zero warnings and errors; focused Install Unit `31/31`, Integration `23/23`,
  and published EndToEnd `4/4` pass with zero skips on their first run. The
  original and extracted graphs have identical finding-code, management-state,
  and user-facing string multisets; only two new unreachable-stage diagnostic
  strings exist. Boundary evidence, plan/findings/effects result shapes,
  branch/effect ordering, constructor, and consumers are unchanged. Format and
  diff verification pass, and the Install Planning graph contains no
  suppression, raw lifecycle-path literal, reflection, or dynamic dispatch.
  For the first Extension Inspect slice, Core, Unit, and Integration Release
  builds pass with zero warnings and errors. Extension Inspect Unit contract
  evidence passes `5/5`, and generated serialization Integration evidence
  passes `4/4`, both with zero skips on their first run. All 53 result
  declarations exist exactly once across files of `208`, `132`, `150`, `69`,
  and `124` lines; the operation factory, operation, result builders, JSON
  projection, and source-generation consumer graph have no slice-local diff.
  Format and diff verification pass. For Extension Inspect S2A, the first Core
  compile exposed one bounded causal error: a nullable source-kind call had
  been routed to the non-null finite mapping. Restoring that single call to the
  retained nullable local overload, which delegates only non-null values to
  the mapping capability, was the only post-failure semantic correction. The
  corrected Core and Unit Release builds pass with zero warnings and errors,
  and Extension Inspect Unit contract evidence passes `5/5` with zero skips on
  its first run. Original and corrected user-facing string,
  `ExtensionInspectFindingCode`, and `CliSemanticStatus` occurrence multisets
  are identical. The result builder, finding policy, boundary factory, and
  mapping files are respectively `987`, `234`, `173`, and `132` lines. Static
  inspection finds no partial result builder or legacy five-parameter empty
  call; comparison, operation, operation-factory, and public result DTO paths
  have no S2A diff. Format verification completes in `55194ms`, and final diff
  verification passes. For Extension Inspect S2B, the first connected Core
  compile failed only because the final next-action call retained the removed
  local `subject` name. Changing that single reference to the typed stage
  result was the only post-failure correction; the corrected Core, Unit,
  Integration, and EndToEnd Release builds pass with zero warnings or errors.
  Focused Extension Inspect Unit passes `5/5`, Integration `22/22`, and
  published EndToEnd `5/5`, all with zero skips on their first run. Original
  and corrected user-facing string, finding-code, and semantic-status
  occurrence multisets are identical. Static inspection finds no moved S2B
  body in the facade, no null-forgiving flow or new mutable capability state,
  and exactly one temporary source-identity delegate; public `Build`/`Event`
  signatures and protected comparison/dependency/path call surfaces remain.
  The subject/package capability is `494` lines. Format verification completes
  in `54110ms`, and final diff verification passes. Extension Inspect S2C's
  first connected Core compile passes with zero warnings and errors, requiring
  no correction. Unit, Integration, and EndToEnd Release builds also pass with
  zero warnings and errors; focused Unit passes `5/5`, Integration `22/22`,
  and published EndToEnd `5/5`, all with zero skips on their first run. Original
  and corrected user-facing string, finding-code, semantic-status, and
  dependency/path-state occurrence multisets are identical. Static branch and
  order inspection preserves dependency traversal, source-closure ordering,
  finding append order, declared/current path ordering, conflict and state
  precedence, and nullable event defaults. The result facade and dependency/path
  capability are respectively `178` and `508` lines; no moved body, obsolete
  dependency input, temporary source-identity delegate, null-forgiving flow, or
  new mutable capability state remains. Format verification completes in
  `64836ms`, and final diff verification passes. S2D Core, Unit, Integration,
  and EndToEnd Release builds pass with zero warnings and errors. Focused Unit
  passes `5/5`, Integration `22/22`, and published EndToEnd `5/5`, all with zero
  skips on their first run. The obsolete `AvailableMatches`, `SourcePackage`,
  and `SourceClosure` stage names are absent. Original and corrected strings,
  finding codes, semantic statuses, dependency/path states, result shape,
  branch/order, and nullable-event audits remain identical. Format verification
  completes in `58514ms`, and final diff verification passes. S3A's first Core
  compile passes with zero warnings and errors. Unit and Integration Release
  builds also pass with zero warnings and errors; focused Unit passes `5/5` and
  focused Integration `22/22`, both with zero skips on their first run.
  Original and corrected string, finding-code, fingerprint-kind/origin,
  Markdown/generated-state, installed-closure order, and
  current-before-intended-before-generated occurrence and branch audits are
  identical. The installed-closure reader, fingerprint builder, and generated
  builder are respectively `38`, `179`, and `102` lines. Format verification
  completes in `54425ms`, and final diff verification passes. S3B's first Core
  compile passes with zero warnings and errors. Unit and Integration Release
  builds also pass with zero warnings and errors; focused Unit passes `5/5` and
  focused Integration `22/22`, both with zero skips on their first run.
  Original and corrected strings, finding codes, modes, side/root states, path
  and dependency relations, finding order, result shape, and nullability audits
  are identical. The path builder, dependency builder, and root projector are
  respectively `233`, `108`, and `177` lines; the old bodies and private
  finding-factory forward are absent. Format verification completes in
  `56586ms`, and final diff verification passes. S3C's first connected Core
  compile failed with one missing existing lifecycle-model import in the new
  actionability-policy file; adding only that import was the sole correction.
  Corrected Core, Unit, Integration, and EndToEnd Release builds pass with zero
  warnings and errors. Focused Unit passes `5/5`, Integration `22/22`,
  published EndToEnd `5/5`, and generated-serialization Integration `4/4`, all
  with zero skips. The first Unit test command used unsupported positional
  project syntax and executed no tests; the corrected supported `--project`
  invocation produced the recorded `5/5` result. Static inspection preserves
  baseline-current-intended-generated-path-dependency formation order,
  finding-sort/status/count/actionability timing, count null-versus-zero rules,
  the complete actionability truth table, result shape, nullability, and the
  constructor/consumer graph. Exactly one comparison input and one counts input
  exist beside their owners, and no temporary comparison forward remains.
  Two earlier formatter invocations exited zero but emitted required-reference
  workspace-load warnings; those warning-bearing results remain recorded. The
  controlling run sets `DOTNET_ROOT=/usr/lib/dotnet`, restores the locked
  unchanged solution from a fresh empty local source with audit disabled, and
  then loads and verifies the complete solution without warnings. The restore
  exits zero in `2.46s`, format exits zero in `60504ms`, and final
  `git diff --check` passes. S4's first connected Core compile failed with one
  missing existing Presentation-model import in the thin JSON facade; adding
  only that import was the sole correction. Corrected Core, Unit, Integration,
  and EndToEnd Release builds pass with zero warnings and errors. Focused Unit
  passes `5/5`, Integration `22/22`, published EndToEnd `5/5`, and
  generated-serialization Integration `4/4`, all with zero skips on their first
  run. Old/new string-literal and enum-token multisets, DTO initializer/member
  order, list materialization/order, nullability branches, and exception
  messages are identical. The extracted workspace-selection mapper preserves
  the original `workspace` exception parameter name and actual enum value.
  Static inspection finds one stable JSON facade consumer, exactly fourteen
  Human/Diagnostic vocabulary calls, no old projection/vocabulary body in the
  facade, and no source-generation context or presentation-DTO change. The
  controlling `DOTNET_ROOT=/usr/lib/dotnet` locked offline restore from a fresh
  empty local source exits zero in `2.57s`; warning-free full-solution format
  exits zero in `59669ms`, and final `git diff --check` passes. The shared
  escaping promotion's first Core compile passes with zero warnings and errors;
  Unit, Integration, and EndToEnd Release builds also pass with zero warnings
  and errors. Focused Create/List/Inspect presentation-contract Unit evidence
  passes `47/47`, the direct List Integration consumer passes `10/10`, and the
  three published Extension process classes pass `28/28`, all with zero skips
  on their first run. Static inspection confirms the exact normalized hash,
  fifteen shared sites, zero old local-owner sites, the preserved 240-character
  value limit and consumer-owned outer limits, and no test or non-Extension
  widening. The controlling locked offline restore exits zero in `2.57s`;
  warning-free full-solution format exits zero in `61815ms`, and final
  `git diff --check` passes. For QR7A, the first Core compile failed only
  because the new result composer imported a nonexistent `.Models.Result`
  namespace; removing that import lets `RouteListResult` resolve from its
  existing parent command namespace and was the only correction. The corrected
  Core, Integration, and EndToEnd Release builds pass with zero warnings and
  errors. The exact Route List Unit namespace passes `122/122`, the exact Route
  List Integration namespace passes `88/88`, and the published
  `CliProcessTests` plus `PublishedRouteInitNeighborProcessTests` pass `27/27`,
  all with zero failures or skips. A separate exact-class Unit selector passed
  `6/6`. Two unsupported `dotnet test` filter attempts and one list-discovery
  attempt each executed zero tests and remain recorded separately from the
  decisive direct Microsoft.Testing.Platform runs. Static inspection confirms
  zero old component references, the preserved inventory-to-result order,
  attempted-selection timing, cancellation rethrow, failure-result selection,
  and call surfaces of `2`, `2/3`, and `4/2` parameters. The controlling
  locked offline restore from a fresh empty source exits zero in `2.74s`;
  warning-free full-solution format exits zero in `64168ms`, and final
  `git diff --check` passes. For QR7B1, the first Core compile failed only
  because the relocated layer-reader delegate lacked the existing
  `Framework.Sources.Reading` import. Adding that import was the sole
  correction; the corrected Core, Unit, Integration, and EndToEnd Release
  builds pass with zero warnings and errors. The exact References Unit
  namespace passes `37/37`, the exact References Integration namespace passes
  `21/21`, generated-serialization Integration passes `2/2`, and the exact
  published References process class passes `12/12`, all with zero failures or
  skips. Static inspection finds no service bag, obsolete component/namespace
  reference, or hidden service field. Production and compile-required test
  string-literal multisets are identical; finding, status, coverage, direction,
  order, cancellation, failure, and nullability/result-shape audits are
  unchanged. The QR7B2 finding factory and QR7B3 binder/selector files have no
  diff. The controlling locked offline restore from a fresh empty source exits
  zero in `2.54s`; warning-free full-solution format exits zero in `59171ms`,
  and final `git diff --check` passes. QR7B2's first Core compile passes with
  zero warnings and errors and requires no correction. Unit, Integration, and
  EndToEnd Release builds also pass with zero warnings and errors. The exact
  References Unit namespace passes `37/37`, the exact References Integration
  namespace passes `21/21`, generated-serialization Integration passes `2/2`,
  and the exact published References process class passes `12/12`, all with
  zero failures or skips on their first runs. Static inspection finds exactly
  eight typed finding-input constructions, nine `AddFinding` occurrences
  including its definition, and twenty `AddEvent` occurrences including its
  definition, proving seven direct calls plus the delegate and nineteen stable
  event callers. Old/new production string literals are identical; candidate
  dedup/order, selector validation, status override, direction, nullability,
  append order, and exception semantics remain delegated unchanged to
  `ReferencesFinding`. QR7B3 files have no diff. The controlling locked offline
  restore from a fresh empty source exits zero in `2.46s`; warning-free
  full-solution format exits zero in `61094ms`, and final `git diff --check`
  passes. QR7B3's first Core compile passes with zero warnings and errors and
  requires no correction. Unit, Integration, and EndToEnd Release builds also
  pass with zero warnings and errors. The exact binding Unit class passes
  `13/13`, the exact application Integration class passes `9/9`, and the exact
  published References process class passes `12/12`, all with zero failures or
  skips on their first runs. Static inspection confirms no postfix suppression
  or nested cause conditional remains; missing evidence still precedes empty
  evidence and retains both exact cause strings. Joint-null inspection proves
  invalid-result formation, workspace, direction, source, and request order,
  with the same workspace exception, nullable-value `InvalidOperationException`
  message, and source-null `ArgumentNullException` parameter name. The
  controlling locked offline restore from a fresh empty source exits zero in
  `2.50s`; warning-free full-solution format exits zero in `61083ms`, and final
  `git diff --check` passes. QR7C1's first Core compile failed only because the
  moved paired-overwrite probe lacked the existing
  `Framework.Sources.Models.Identity` import. Adding that import was the sole
  correction; the corrected Core, Unit, Integration, and EndToEnd Release
  builds pass with zero warnings and errors. The exact Find operation,
  presentation-binding, binding, and query-parser Unit classes pass `32/32`;
  the exact operation, source-universe, and application Integration classes
  pass `31/31`; and the exact published Find process class passes `14/14`, all
  with zero failures or skips. Static inspection finds no service bag or
  obsolete helper, proves the operation's five cohesive dependencies and the
  source resolver's four dependencies with `2/3/1/2` call surfaces, and
  preserves boundary-to-result stage order, include-before-exclude probe order
  and deduplication, cancellation/failure timing, coverage, nullability,
  findings, status, and result shape. The binder's named queue choice preserves
  predicate-token order, typed-value agreement, and both exact invariant
  messages. Matcher, projection, and result-builder bodies have no QR7C1 diff.
  The controlling locked offline restore from a fresh empty source exits zero
  in `2.55s`; warning-free full-solution format exits zero in `63791ms`, and
  final `git diff --check` passes. QR7C2A's first Core compile passes with zero
  warnings and errors and requires no correction. Unit, Integration, and
  EndToEnd Release builds also pass with zero warnings and errors. The exact
  matcher and operation Unit classes pass `14/14`; the exact operation and
  document-inspection Integration classes pass `10/10`; generated Find
  serialization passes `2/2`; and the exact published Find process class passes
  `14/14`, all with zero failures or skips on their first runs. Static
  inspection proves byte-identical user-facing string and finding-code
  multisets and preserves base-before-overwrite inspection, missing/body
  finding formation, deduplication keys, finding order, coverage mapping/rank,
  candidate-count coverage, description selection, and nullability/exception
  behavior. The matcher is reduced from `704` to `523` lines over an `88`-line
  typed layer-facts builder and `128`-line finding policy; both long layer
  overloads, reference-coverage mutation, nested layer facts, and old policy
  bodies are absent. Regional matching and source/result truth tables remain
  behaviorally unchanged. The controlling locked offline restore from a fresh
  empty source exits zero in `2.58s`; warning-free full-solution format exits
  zero in `61056ms`, and final `git diff --check` passes. QR7C2B's first Core
  compile failed only because the connected source matcher retained the moved
  private evidence-candidate type name. Replacing that reference with the new
  immutable `FindEvidenceCandidate` name was the sole correction; the corrected
  Core, Unit, Integration, and EndToEnd Release builds pass with zero warnings
  and errors. The exact matcher and operation Unit classes pass `14/14`; the
  exact operation and document-inspection Integration classes pass `10/10`;
  generated Find serialization passes `2/2`; and the exact published Find
  process class passes `14/14`, all with zero failures or skips. Static
  inspection proves byte-identical user-facing strings and finding codes and
  preserves predicate/layer/region order, Document frontmatter-before-body
  dispatch, unknown propagation, first-occurrence finding deduplication,
  section ambiguity/missing precedence, ordinal-ignore-case comparison, UTF-8
  containment and locations, evidence ordering, nullability, and exception
  behavior. The matcher is now `151` lines over a `325`-line stateless typed
  predicate matcher and `66`-line immutable predicate/evidence facts file; the
  old `5/7/9/8/8` trains, reference-unknown mutation, regional bodies, and
  nested predicate/evidence facts are absent. The controlling locked offline
  restore from a fresh empty source exits zero in `2.52s`; warning-free
  full-solution format exits zero in `62887ms`, and final `git diff --check`
  passes. QR7C2C's first Core compile failed only because the bare-query branch
  and later predicate branch used the same local coverage name across C# local
  declaration scopes. Renaming the bare-query fact to `bareCoverage` was the
  sole correction; the corrected Core, Unit, Integration, and EndToEnd Release
  builds pass with zero warnings and errors. The exact matcher and operation
  Unit classes pass `14/14`; the exact operation and document-inspection
  Integration classes pass `10/10`; generated Find serialization passes `2/2`;
  and the exact published Find process class passes `14/14`, all with zero
  failures or skips. Static inspection proves byte-identical user-facing
  strings and finding codes and preserves source grouping, layer-before-
  predicate finding order, bare and All/Any truth tables, unknown and candidate
  coverage, evidence order, matched-source filtering, ID/path ordering,
  one-based result indices, nullability, and result shape. The public-local
  parameterless matcher facade is `32` lines over a `95`-line source matcher,
  `48`-line matching result builder, and `55`-line immutable source/result facts
  file; no old source/result body or nested facts remain. The controlling
  locked offline restore from a fresh empty source exits zero in `2.48s`;
  warning-free full-solution format exits zero in `61718ms`, and final
  `git diff --check` passes. QR7C3A's first Core compile failed on six missing
  `FindSourceIdentity` references because the moved projection fact and source
  resolver files lacked the existing Selection-model import. After that import
  correction, the second Core compile failed because the projection facade had
  lost its existing Shell status import. Restoring that import was the sole
  second correction. The corrected Core, Unit, Integration, and EndToEnd
  Release builds pass with zero warnings and errors. The exact projection and
  operation Unit classes pass `14/14`; the exact operation and
  document-inspection Integration classes pass `10/10`; generated Find
  serialization passes `2/2`; and the exact published Find process class passes
  `14/14`, all with zero failures or skips. Static inspection proves identical
  user-facing strings and finding-code multisets and preserves source matching,
  route selection, layer order, missing-section finding order and deduplication,
  coverage, nullability, and result shape. The projection facade is reduced to
  `361` lines over a `114`-line source resolver, `86`-line finding policy,
  `28`-line immutable source-facts file, and `61`-line immutable build-facts
  file; all moved source, route, layer, finding, rank, and overwrite-path bodies
  are absent from the facade. The controlling locked offline restore from a
  fresh empty source exits zero in `2.68s`; warning-free full-solution format
  exits zero in `61072ms` (`61.53s` wall time), and final `git diff --check`
  passes. QR7C3B's first Core compile passes with zero warnings and errors and
  requires no correction. Unit, Integration, and EndToEnd Release builds also
  pass with zero warnings and errors. The exact projection and operation Unit
  classes pass `14/14`; the exact operation and document-inspection Integration
  classes pass `10/10`; generated Find serialization passes `2/2`; and the
  exact published Find process class passes `14/14`, all with zero failures or
  skips. Static inspection proves identical user-facing string and finding-code
  multisets, one-typed-input content call surfaces, and the absence of all moved
  content bodies from the facade. Metadata-before-physical, layer and requested-
  part ordering, missing-section timing, unavailable and incomplete-inspection
  behavior, frontmatter/headings/body/section precedence, UTF-8 location
  mapping, exception details, finding order, coverage, nullability, and result
  shape remain unchanged. The complete projection facade is now `75` lines over
  a `301`-line stateless content builder plus the closed C3A source resolver and
  finding policy; the facade owns only requested-part, match, and stage
  orchestration. The controlling locked offline restore from a fresh empty
  source exits zero in `2.84s`; warning-free full-solution format exits zero in
  `57054ms` (`57.50s` wall time), and final `git diff --check` passes. QR7C4A's
  first Core compile failed only because the extracted match builder lacked the
  existing Query-model import. After that correction, the second compile
  exposed the still-protected inspected-count body's use of the former nested
  `MatchKey`; restoring that same facade-local key as an explicit temporary C4C
  seam was the sole second correction. The corrected Core, Unit, and
  Integration Release builds pass with zero warnings and errors. The direct
  result-builder and operation Unit classes pass `31/31`, and the application
  Integration class passes `21/21`, with zero failures or skips. Static
  inspection proves identical user-facing strings and finding codes and
  preserves first ID/path occurrence, ordinal match order, one-based positions,
  evidence order, requested projection filtering, overwrite-path normalization,
  explicit/nearby/free metadata attachment, projection order, and exact
  exceptions. All moved attachment bodies are absent from the facade, which is
  reduced from `935` to `703` lines over a `246`-line match builder and
  `8`-line immutable one-input fact. The controlling locked offline restore
  from a fresh empty source exits zero in `2.94s`; warning-free full-solution
  format exits zero in `63284ms` (`63.74s` wall time), and final
  `git diff --check` passes. QR7C4B's first Core compile failed only because the
  new typed input file lacked the existing Documents-model import. After that
  correction, the second compile failed because the status policy lacked the
  existing Result-model import that owns both coverage enums. Adding that import
  was the sole second correction. The corrected Core, Unit, and Integration
  Release builds pass with zero warnings and errors. The direct result-builder
  and operation Unit classes pass `31/31`, and the application Integration class
  passes `21/21`, with zero failures or skips. Static inspection proves
  identical user-facing strings and finding-code multisets, the exact
  reference-identity inspection deduplication, projection/stage/terminal append
  and suppression order, terminal/status precedence, retained-status matrix,
  synthesized status-finding conditions, final finding sort, and exceptions.
  All moved finding/status bodies are absent from the facade, which is reduced
  to `328` lines over a `354`-line finding builder, `65`-line status policy, and
  `25`-line immutable typed-input file. The remaining coverage, universe,
  inspected-count, next-action, conditional match-clear, and temporary
  `MatchKey` seams are unchanged for C4C. The controlling locked offline restore
  from a fresh empty source exits zero in `2.69s`; warning-free full-solution
  format exits zero in `63244ms` (`63.67s` wall time), and final
  `git diff --check` passes. QR7C4C's first Core compile failed only because the
  expanded typed-input file lacked the existing Selection-model import for
  `FindUniverse`. After that correction, the second compile failed because the
  inspected-count builder lacked the existing Matching-model import for body-
  tag availability. Adding that import was the sole second correction. The
  corrected Core, Unit, Integration, and EndToEnd Release builds pass with zero
  warnings and errors. The direct result-builder and operation Unit classes
  pass `31/31`, and the application Integration class passes `21/21`. The final
  projection and operation Unit classes pass `14/14`; operation and document-
  inspection Integration pass `10/10`; generated Find serialization passes
  `2/2`; and the published Find process class passes `14/14`, all with zero
  failures or skips. Static inspection proves identical user-facing strings
  and finding-code multisets and preserves matching/projection/overall coverage
  truth tables, invalid/blocked match-clear timing, unresolved-selector and
  universe nullability, inspected-source grouping and completeness, candidate
  clamp, next-action selection, exact exceptions, and final result shape. The
  temporary facade `MatchKey` and every moved C4C body are absent. The complete
  result facade is now `69` lines over the closed `246`-line match builder,
  `354`-line finding builder, `65`-line status policy, `124`-line coverage
  builder, `59`-line universe builder, `90`-line inspected-count builder,
  `28`-line next-action policy, and `50`-line typed-input file. The controlling
  locked offline restore from a fresh empty source exits zero in `2.79s`;
  warning-free full-solution format exits zero in `65128ms` (`65.58s` wall
  time), and final `git diff --check` passes. The accepted composition decision
  remains explicit concrete
  construction rather than dependency injection: the removed Find service bag
  was the violation, while the current typed `5/4/4` surfaces comply. DI is
  reconsidered only if long-lived scope or three-family variability/churn
  appears. Before QR7 closes, dense Find factory construction will be rewritten
  with named local capabilities or private factory-local helpers and named
  arguments; References, Index, Install, and Route List receive the same
  readability inspection, but only comparable direct smells may change and
  small factories are not rewritten mechanically. QR7D applies that decision:
  Find and References now use named capability locals, named constructor
  arguments, and factory-local boundary/path delegate helpers while preserving
  their single physical resolver and shared delegate identities. Route List now
  names its source, result, and coordinator graphs with named arguments. Index
  and Install were inspected and intentionally remain unchanged because their
  already named local composition has no comparable dense-construction smell.
  The first Core build passes with zero warnings and errors. Unit and
  Integration Release builds pass with zero warnings and errors; focused Find
  Unit and Integration pass `31/31` and `21/21`, References pass `37/37` and
  `21/21`, and Route List passes `122/122` and `88/88`. A preliminary exact
  References Unit namespace selector ran `22/22` green but failed its `37`
  minimum; the accepted wildcard namespace selector's `37/37` result is the
  decisive evidence and both outcomes remain recorded. EndToEnd Release passes;
  published Find, References, and Route List neighbor/process evidence passes
  `14/14`, `12/12`, and `27/27`. Static inspection proves the same factory
  instance counts, closure targets, Markdown parser sharing, catalogue/read
  order, selection scope, operation dependency graph, and zero Index/Install
  diff. The controlling locked offline restore from a fresh empty source exits
  zero in `3.17s`; warning-free full-solution format exits zero in `70678ms`
  (`71.24s` wall time), and final `git diff --check` passes. QR7 is closed.
  The coherent Task 1 stages 1–7 snapshot is commit
  `1f8708f6e3720b7f654cf4479714fe14de27f3eb`, tree
  `306324e2ffe0238448a46a4545e7295cd9bc060e`, with subject
  `Improved the CLI architecture`.
  The QR7a physical-quality correction moves the twelve command-local context
  declarations into their matching JSON renderers and deletes the twelve
  standalone context files. Exact old/new declaration-block comparison proves
  unchanged namespaces, attributes and order, source-generation options,
  registered document types, generated properties, modifiers, context names,
  and renderer call sites. The Framework ExtensionPackage, Lifecycle, and
  Recovery contexts and Shell `CliJsonContext` have zero diff. The implementation
  owner and independent reviewer each fully read and acknowledged `_csharp.md`,
  `design.md`, and `style.md`; the reviewer reports no material finding and made
  zero edits. Release solution build passes with zero warnings and errors;
  serialization selections pass `18/18` for the exact Serialization namespace
  and `19/19` for the wider `FullyQualifiedName~Serialization` selector. Complete
  managed Unit, Integration, and EndToEnd pass `1494/1494`, `734/734`, and
  `146/146`. Supported `linux-x64` Native AOT root publication produces the
  expected x86-64 ELF and version `0.0.0-dev`; native EndToEnd passes `146/146`.
  The first native Integration run passes `733/734` and fails only the already
  recorded Route Init cancellation/residual race; its immediate unchanged rerun
  passes `734/734`. Both outcomes remain controlling evidence and
  `CLI-TEST-011` stays open. The controlling fresh-empty-source locked offline
  restore exits zero in `3.05s`; warning-free full-solution format exits zero in
  `69333ms` (`69.86s` wall time), final `git diff --check` passes, and QR7a is
  closed.
  `CLI-TEST-011` is now corrected. The frozen Red replaces the asynchronous
  `FileSystemWatcher`/`ManualResetEventSlim` race with a synchronous observer
  call and fails once, without retry, at the exact missing seam with `CS1501`,
  zero warnings, and one error. Green adds one internal Route Init-local named
  `RouteInitDirectoryCreationObserver`, threads its nullable/default value only
  through `RouteInitOperationFactory` and `RouteInitApplicationOperation`, and
  invokes it after a directory receipt is recorded and verified and before the
  next planned effect. The production null path and every existing caller are
  unchanged; Framework appliers, public surfaces, and other commands have zero
  diff. The deterministic containing class passes `9/9`, and affected Route
  Init application/Framework Integration passes `48/48`. Complete Release
  build passes with zero warnings and errors; managed Unit, Integration, and
  EndToEnd pass `1494/1494`, `734/734`, and `146/146` on their first corrected
  runs. Fresh supported `linux-x64` Native AOT root publication produces an
  x86-64 ELF and version `0.0.0-dev`; the first corrected native Integration
  run passes `734/734`, and native EndToEnd passes `146/146`, with no retry.
  The preserved pre-correction native `733/734` failure and immediate unchanged
  `734/734` pass remain the defect receipts rather than being flattened into a
  clean first run. Red and Green owners and the independent reviewer each fully
  read and acknowledged `_csharp.md`, `design.md`, and `style.md`; review passes
  with zero edits. The controlling fresh-empty-source locked offline restore
  exits zero in `2.69s`; warning-free full-solution format exits zero in
  `71441ms` (`71.94s` wall time), and final `git diff --check` passes. QR8 is
  closed.
  Phase 10 revalidated every accepted checkpoint finding against QR8 commit
  `a81745df1126d9d869567d92f972166b944f050f`, tree
  `040ef0b720632433861e3ce95bb5b6ef93e2223d`, before mutation. C1A is closed:
  twenty Extension Inspect property-only formation records now live exactly
  once in four `Models/Result` topic files, and `Shared/Result` retains no
  top-level sealed record declarations. The subject/package facade is `81`
  lines over `173`-line subject/source and `252`-line package/lifecycle
  capabilities; the dependency/path facade is `34` lines over `211`-line
  dependency/closure and `276`-line path-facts capabilities. Stable facade call
  sites, public result/JSON/source-generation shape, finding and string order,
  nullability, and all non-Extension behavior remain unchanged. The Extension
  group Notes text is now one byte-equivalent raw multiline literal; event path
  state uses named flat decisions; comparison mode and source failure mappings
  enumerate every named value and throw for undefined values. The first
  connected Core Release build passes with zero warnings and errors. Focused
  Extension Inspect Unit, Integration, published process, and serialization
  evidence passes `5/5`, `22/22`, `5/5`, and `4/4`; exact group-help Integration
  and published evidence passes `4/4` and `4/4`. A preliminary direct artifact
  invocation without the controlling `DOTNET_ROOT` exited `150` before app
  execution; the corrected controlling invocation exits zero with exact Notes
  output. Warning-free full-solution format exits zero after formatting `0` of
  `1155` files in `62.948s`, and final diff checks pass. The mandatory bounded
  follow-up exposes exactly two internal command-local mapping seams at their
  existing semantic owners and adds two direct Unit facts. They exercise all
  four named comparison modes across their complete, incomplete, and
  not-started outcomes; all eleven named source-failure kinds across exact and
  fallback outcomes; and one undefined value per mapping with exact
  `ParamName` and `ActualValue`. Core and Unit Release builds pass with zero
  warnings and errors. Direct mapping evidence passes `2/2`; affected Unit,
  Integration, published process, and generated-serialization evidence passes
  `5/5`, `22/22`, `5/5`, and `4/4`. A broad preliminary Unit selector passed
  `12/12` before exact FQNs proved the intended `5/5`; a preliminary
  serialization command targeted the Unit project and selected zero tests with
  exit `8` before the required Integration selector passed `4/4`. Both operator
  receipts remain explicit. Warning-free full-solution format exits zero after
  formatting `0` of `1155` files in `60.120s`, and final diff checks pass.
  C1B now provides one neutral Framework `SourceReadSession` fact and reader
  for the identical Find and References catalogue, document-reader, and
  default-contained-scope boundary. Each command factory shares one
  `PhysicalPathResolver` instance with that reader and its command-local path
  delegates; reference resolution, universe policy, findings, and result
  formation remain local. The duplicated command contexts, readers, and
  delegates are absent. References state-only inspection, resolution, and
  result inputs now live under truthful Models topics, its markdown parser is
  under Documents/Parsing, and `InspectLayersAsync` is reduced from eight
  parameters to one immutable five-fact scan input plus the two mutable outputs
  and explicit cancellation. Five References enum seams enumerate every named
  value, nullable `null` where applicable, and undefined throws; direct session
  and enum evidence passes `3/3` and `5/5`. Focused Find evidence passes
  `31/31`, `21/21`, `14/14`, and `2/2`; References evidence passes stable
  `37/37` Unit plus direct `5/5`, then `21/21`, `12/12`, and `2/2`. The first
  connected Core checkpoint preserved one missed-rename `CS0103`; the first
  Unit compile preserved one missing test import and one invalid test-only path
  constant before the exact corrections. Full Release builds pass with zero
  warnings and errors; managed Unit, Integration, and EndToEnd pass
  `1504/1504`, `734/734`, and `146/146`. Supported `linux-x64` Native AOT CLI,
  Integration, and EndToEnd publication succeeds; exact native Integration and
  EndToEnd runs pass `734/734` and `146/146`. One preliminary passing native
  Integration run reported a deprecated `--no-progress` option before the
  exact no-argument run passed cleanly. Warning-free full-solution format exits
  zero after formatting `0` of `1162` files in `55.339s`, and final static and
  diff checks pass. C1C makes nine nearest-owner Find enum seams explicit and
  exhaustive while retaining every prior guard, cause string, status/coverage
  truth table, order, and valid result. Nine direct Unit facts cover every
  named value, nullable `null` and guarded alternatives where applicable, and
  one undefined input per seam with exact `ParamName` and `ActualValue`;
  evidence passes `9/9`. The accepted Find test-structure candidate is
  confirmed and corrected: immutable compiler-checked scenario facts replace
  string discriminators for exactly eleven Universe and seven Matcher theory
  rows, with fixture recreation, physical-probe counts, assertions, discovery
  labels, and the total eighteen-row coverage unchanged. Exact changed theory,
  two-class, and frozen Unit neighborhoods pass `18/18`, `20/20`, and `31/31`;
  affected Integration, published process, and serialization evidence passes
  `21/21`, `14/14`, and `2/2`. The preliminary broad Unit trait selector passes
  `86/86` but is retained only as broader evidence, and a provisional
  four-class Integration selection passes `20/20` without replacing the exact
  `21/21` receipt. Core, Unit, and full-solution Release builds pass with zero
  warnings and errors. Preserved operator/compile receipts cover a wrong SDK-9
  root, one test import, typed-carrier accessibility/initializer shape, one
  `IReadOnlyList.Length` use, and one wrong forecast solution path; none is a
  behavioral failure. Warning-free full-solution format exits zero after
  formatting `0` of `1163` files in `62.952s`, and final static and diff checks
  pass. C1D makes all eleven accepted Route Init enum sites explicit and
  exhaustive while preserving existing guards, effect/receipt order, cause
  strings, findings, residual formation, nullability, and every valid-but-
  incoherent tuple fallback. Eleven direct Unit facts cover each original site,
  every named component value and valid tuple fallback, and undefined values
  with exact exception message, `ParamName`, and `ActualValue`; evidence passes
  `11/11`. Route Init Unit grows by exactly eleven and passes `34/34`; affected
  Integration, published process, and serialization evidence passes `85/85`,
  `6/6`, and `1/1`. Full Release builds pass with zero warnings and errors;
  managed Unit, Integration, and EndToEnd pass the exact expected `1524/1524`,
  `734/734`, and `146/146`. Selector-only preliminary receipts selected zero,
  seven, or zero tests before the decisive MTP class, namespace-wildcard, and
  trait selectors passed; no behavior test failed. Warning-free full-solution
  format exits zero after formatting `0` of `1164` files in `54.88s`, only the
  two intended valid-tuple policy fallbacks remain nonthrowing behind exhaustive
  component validation, and final static and diff checks pass.
  C1E closes the durable test-identity defect through exactly fifty-six
  filesystem-safe file moves and matching declaration-only class renames from
  temporary `*RedTests` identities to stable `*Tests` identities. Normalized
  old/new blobs are identical except for their containing class names; active
  old paths and symbols are zero, all fifty-six new classes discover, and no
  project, namespace, directory, fixture, display name, trait, body, tier,
  production path, or historical/working prose changes. Discovery remains
  exactly `1502` Unit identities producing `1524` cases, `734` Integration
  cases, and `146` EndToEnd cases. Exact post-rename FQN execution across all
  fifty-six classes passes `633/633` with zero failures or skips. Release builds
  pass with zero warnings and errors; the fresh-empty-source locked restore
  exits zero in `2.45s`. A preliminary unsupported formatter `--nologo`
  invocation exits `1` as an operator receipt; the supported warning-free
  formatter exits zero with no changes in `54.36s`, and final static and diff
  checks pass.
  `QR-R1` reviewed exact candidate `e11b225e`, tree `a19fa69b`, and reported
  one High evidence-tier finding, `QR-R1-001`: two `SourceReadSessionReader`
  tests performed real temporary-workspace, catalogue, and filesystem work in
  Unit. The bounded correction moves exactly those two unchanged test bodies,
  display names, and `source-catalogue` features to mirrored
  `Framework/Sources/Reading` Integration ownership, changing only the class,
  namespace, and `Evidence` trait; the pure session-fact test remains in Unit,
  and each tier retains the identical local workspace helper it needs. The
  first Unit build preserved one missed required `Sources.Reading` import and
  its cascading xUnit overload diagnostic; the exact import correction then
  builds warning-free. Focused Unit and Integration pass `1/1` and `2/2`.
  Discovery/execution parity is exact: Unit has `1500` identities and passes
  `1522/1522`, Integration passes `736/736`, and EndToEnd passes `146/146`, with
  zero failures or skips. Supported `linux-x64` Native AOT Integration publish
  succeeds and the published runner passes `736/736`. The fresh-empty-source
  locked restore exits zero in `2.49s`; warning-free format changes `0` of
  `1165` files and exits zero in `52.81s`. The original reviewer narrowly
  revalidated exact range `e11b225e..a4ccf19a`, closed `QR-R1-001`, found no
  new material issue, and accepts corrected candidate `a4ccf19a`, tree
  `97254e65`.
- Blockers: None. Route Update may begin from the integrated remediation
  baseline.
- Residual risk: The Route List, References, and Find portions of
  `CLI-DESIGN-003`, `CLI-DESIGN-006`, and `CLI-DESIGN-007` are closed.
  `CLI-TEST-011` is closed. C1A closed the Extension Inspect portions of
  `ARCH-001`, `ARCH-002`, `CSHARP-001`, `CSHARP-002`, and `CSHARP-004`. C1B
  closed accepted `ARCH-003`, References `ARCH-001`, `ARCH-004`, and its five
  `CSHARP-004` sites. C1C closed the nine Find `CSHARP-004` sites and confirmed
  and corrected `CLI-TEST-013`. C1D closed all eleven Route Init `CSHARP-004`
  sites. C1E closed `CLI-TEST-012`. Grouped correction `QR-C1` is closed;
  holistic final review `QR-R1` is consumed and accepted after `QR-R1-001`
  correction and narrow revalidation. No material finding remains.

## Completion And Closeout

Complete. Every actual finding has a verified behavior-preserving correction or
accepted disposition; both candidates have fresh proof; final review and its
single bounded correction revalidation accept exact implementation candidate
`a4ccf19a`, tree `97254e65`, now squash-integrated at `862cbf2a`, exact tree
`571f104f`. Exact paths, dependency direction, local/shared placement, canonical
authority, evidence tiers, deterministic cancellation, durable names, managed
regressions, and supported `linux-x64` Native AOT gates are recorded above.
Route Update is next.
