---
open-forge:
  description: Investigation behind the accepted CLI modularization direction and its remaining implementation questions
  tags: [Memory, Working, CLI, Architecture, Testing, Contextual, Active]
---

# CLI Modularization Review

## Status And Scope

Discussion and source investigation on 2026-09-19, in `codex/extensions-experience-review` at checkpoint `7b4745f7` plus the existing experience-review changes. The maintainer subsequently accepted the four-library split and later fifth OutputText library, and requested planning materials. The [Task 38 migration packet](cli-development/tasks/task38/_task38.md) now owns execution sequencing and detailed readiness. This record preserves the investigation, not a competing plan. No production split, new scenario tests, filesystem abstraction or output extraction has been implemented.

[Task 38](cli-development/tasks/task38-project-and-test-split.md) owns the eventual project/test split. This record preserves the discussion and evidence rather than creating a second execution plan. The [current architecture](../crystallized/documents/cli/architecture.md) remains the implementation baseline until changed deliberately. Related evidence is already in the [flow/scenario collection](../crystallized/documents/cli/experience/_experience.md), [manual run](cli-experience-run.md) and [coverage audit](cli-experience-coverage/_cli-experience-coverage.md); do not copy those inventories here.

## Maintainer Direction

- Improve reviewability: a reviewer should be able to follow a user flow to a scenario, its expected behavior, output wording and reviewed output views using stable identities.
- Use ordinary typed C# text factory methods, grouped by command. Resource files and the surveyed general template engines are unwanted. The maintainer clarified that `@OpenForge` comments are document-path references. The [identity design](cli-development/tasks/task38/output-identities.md) uses separate `@OpenForgeText` and `@OpenForgeTextRef` markers for dotted wording IDs.
- Reduce independently maintained Markdown/code copies and arbitrary prose-literal assertions. Reviewed snapshots are useful evidence; multiple named snapshots per test are allowed.
- Reusing a deliberately chosen production text factory is acceptable for checking message selection and argument substitution. Independent snapshots must still protect the wording itself.
- Prefer data inputs and outputs at meaningful boundaries, and reduce unnecessary test filesystem activity. Investigate a simple interchangeable memory-backed dependency without constructing a virtual operating system.
- Sequence structural changes separately from test restructuring, text extraction and behavior changes. Preserve existing tests initially.
- The maintainer is still reviewing scenarios. **Do not add the proposed scenario E2Es or implement their behavior changes yet.** After approval, desired scenarios may first expose failures, followed by the corresponding behavior fixes and expectation updates.
- Luna at maximum reasoning is preferred for precisely specified, independent work. Shared architecture, integration and acceptance remain centrally owned.

## Project Recommendation

Accepted target: **four libraries replacing Core**, plus the existing executable host. The accepted later OutputText extraction is additional and is excluded from the three-versus-four comparison. This means five production projects initially and six after OutputText. Test projects are counted separately. Exact visibility and callable seams still require the packet's bounded investigation before implementation.

| Proposed library | Owns | Direct production references |
| --- | --- | --- |
| `OpenForge.Cli.Framework` | Workspace/document observation, routing facts, physical filesystem operations, ownership/recovery and embedded package access | No other CLI project |
| `OpenForge.Cli.Shell` | Shared argument parsing, invocation/binding contracts, status/stream definitions, interaction contracts and process-completion policy | Framework, for the existing workspace-selection boundary |
| `OpenForge.Cli.Operations` | Existing Commands: typed requests/results, planning/application and command-local input adapters | Framework and Shell |
| `OpenForge.Cli.Rendering` | Report selection, detail/filter rules, text/JSON renderers, help/prompt presentation and render-ready output | Operations result models and neutral Shell contracts |
| Existing executable `OpenForge.Cli` | Composition, concrete command registration, complete execution pipeline, stream writing and hosting | Explicit references to the four libraries it composes |

References point from consumer to dependency. Operations never references Rendering. Framework never references Shell or Operations. Shared Shell has no reference to concrete Operations or Rendering. Rendering must not consume Framework facts or operation behavior through an indirect reference. Preserve command-local folders; do not create one assembly per command.

This keeps the already-demonstrated Framework/Operations boundary and makes generic Shell independently selectable without turning Rendering into an operation runner. Command input adapters may remain beside their command during the first split; this is not a claim that the entire Operations assembly is a parser-free functional core. Its operation entry points still accept typed requests.

### Alternatives

| Option | Benefit | Cost / reason not preferred |
| --- | --- | --- |
| Keep one Core assembly | Least immediate movement; existing typed seams can already improve tests | Does not provide the requested independent project boundaries; using-based checks currently miss real dependencies |
| Three libraries: Shell, combined Data (Framework + Operations), Rendering | Fewer projects and much of the current placement remains recognizable | Framework-to-Operations dependency prohibition stays an intra-assembly convention; combines reusable facts with command policy |
| Three libraries: Framework, Operations, Rendering; put Shell in the executable | A legitimate smaller graph with separate domain and output assemblies | Command bindings currently consume Shell contracts. Avoiding Operations-to-executable references requires moving command input adapters and rehoming shared result/status contracts. Generic Shell and concrete composition share a compiler boundary |
| Four libraries plus existing host | Preserves both meaningful boundaries with less semantic redistribution; Shell and Framework remain separately testable | One extra library; requires explicit cycle repair, API visibility and build configuration work |

The fourth library earns its cost through the current dependency and review goals, not merely the number of source files. A separate generic Contracts/Common project is not justified by the inspected dependencies. Keep genuinely neutral CLI contracts with Shell and command result contracts with their Operations owner.

The two bounded Luna reviews agreed on this graph and the host-owned orchestration, but offered different placements for generic binding/result contracts and small pipeline stages. Moving command-facing generic contracts into Operations is also acyclic. This recommendation keeps the neutral existing contracts in Shell for the first split, with rendering logic in Rendering and completion policy in Shell; the host composes them. These are proposed ownership choices, not proven compilation requirements. Freeze the exact public/friend API and type-placement map before parallel moves.

## Concrete Repairs Before Mechanical Moves

The current tree cannot simply become four projects unchanged. Six Shell files import Presentation, while Presentation and Commands also use Shell.

| Current surface | Recommended destination / treatment |
| --- | --- |
| [CliReportBinding](../../../src/cli/core/OpenForge.Cli.Core/Shell/Composition/CliReportBinding.cs) and [CliReportCommandBinding](../../../src/cli/core/OpenForge.Cli.Core/Shell/Composition/CliReportCommandBinding.cs) | Host composition: these join a command binding to its concrete presentation functions. Keep `ICliCommandBinding` and neutral request-binding contracts in Shell |
| [CliReportPipeline](../../../src/cli/core/OpenForge.Cli.Core/Shell/Pipeline/CliReportPipeline.cs) | Host pipeline: it invokes the supplied operation, renders its result and writes output; it is not a renderer responsibility |
| [CliPipelineStages](../../../src/cli/core/OpenForge.Cli.Core/Shell/Pipeline/CliPipelineStages.cs) | Separate the existing classes by owner. Keep generic invocation/result validation and completion policy in Shell; move `CliRenderingStage` to Rendering and concrete stream writing to the host. Do not recreate the pipeline as a registry or universal engine |
| [CliCoreApplication](../../../src/cli/core/OpenForge.Cli.Core/Shell/Composition/CliCoreApplication.cs) | Host application orchestration, because it combines parsing/binding with the concrete help renderer. Shared parsing and invocation services remain in Shell |
| [CliRenderedOutput](../../../src/cli/core/OpenForge.Cli.Core/Shell/Pipeline/Models/Presentation/CliRenderedOutput.cs) | Rendering, alongside the text-document/span representation it exposes; the host consumes it |

When splitting stages, remove the output writer's incidental dependency on `CliRenderingStage.MaximumDiagnosticLength`: the limit is shared output policy and should have one owner in Shell. Preserve authored versus generated spans and their different newline handling; flattening everything to an ordinary string would change behavior.

Root already performs composition in [CliCompositionRoot](../../../src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs) and closes bindings in [CliStandaloneComposer](../../../src/cli/root/OpenForge.Cli/Composition/CliStandaloneComposer.cs). Moving the small cross-layer glue there does not move command planning or rendering policy into the host. Existing integration tests already reference the root project, so the full in-process pipeline remains directly testable.

### A Boundary Violation The Current Check Misses

[LibraryInspectReportSelector](../../../src/cli/core/OpenForge.Cli.Core/Presentation/Library/Inspect/Shared/Selection/LibraryInspectReportSelector.cs), around line 60, names `Framework.Workspace.Models.CliWorkspaceSelectionMethod` directly. Other selectors read `result.Workspace.LexicalRoot` or `PhysicalRoot` through inferred types. [ICliCommandResult](../../../src/cli/core/OpenForge.Cli.Core/Shell/Pipeline/Models/Operation/ICliCommandResult.cs) exposes the Framework `CliWorkspace` type.

[LayerBoundaryTests](../../../src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Architecture/LayerBoundaryTests.cs) scans matching `using` directives. It does not prove absence of fully qualified or inferred dependencies. Consequently the earlier claim that all forbidden edges already hold was too strong.

Project the exact facts each renderer needs into its command-owned result view, preserving lexical versus physical path semantics and unavailable/null states. [StatusResult](../../../src/cli/core/OpenForge.Cli.Core/Commands/Status/Models/Result/StatusResult.cs) already exposes `WorkspacePath` and `WorkspaceExplicit` as a precedent. Do not expose physical resolution or mutable filesystem capabilities merely to display a path. The generic rendering contract should require only the result facts it actually consumes; do not accidentally expand its Framework-typed surface.

[UpdateReportSelector.PreviousContent](../../../src/cli/core/OpenForge.Cli.Core/Presentation/Update/Shared/Selection/UpdateReportSelector.cs) also calls `Directory.Exists` on the physical workspace's `.git` directory to choose the `git-diff` hint. This is a live observation inside rendering. Move the observation behind the Framework/Operations boundary and pass its result if rendering is to become deterministic from data alone. Preserve its current directory-based meaning, unavailable behavior and relevant observation timing; changing Git detection semantics belongs to a separate behavior change.

.NET SDK references are transitive by default. [DisableTransitiveProjectReferences](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#disabletransitiveprojectreferences) supports an explicit-reference policy. Evaluate that policy for the new libraries and qualify the resulting build; project names alone are not an access boundary. Retain architecture checks for allowed API subsets, command ownership and intra-assembly dependencies. A Rendering reference to Operations does not entitle it to invoke planners or mutation code.

Task 38's blanket expectation that project references can replace every current architecture assertion needs refinement before implementation. Retire a check only when an actual compiler-enforced restriction replaces the particular rule.

## Assembly And Build Constraints

- Most existing types are internal. Define small cross-assembly surfaces deliberately; do not make the whole tree public. Any friend assemblies must be intentional, and they do not replace semantic dependency checks.
- [EmbeddedFrameworkPayloadReader](../../../src/cli/core/OpenForge.Cli.Core/Framework/Distribution/EmbeddedFrameworkPayloadReader.cs) and [EmbeddedExtensionCatalogueAssets](../../../src/cli/core/OpenForge.Cli.Core/Framework/Extensions/Embedded/EmbeddedExtensionCatalogueAssets.cs) read resources from their own assembly. Keep payloads with their readers in Framework and preserve logical resource names and bytes. Assembly-sensitive tests need structural maintenance without weakening byte-parity checks.
- Keep source-generated JSON/YAML metadata with the owning types and required visibility. Reflection-disabled serialization, trimming and Native AOT must continue working.
- Update the solution, explicit project references, root publication, delivery suite registration and resource inclusion coherently. Namespace changes are not required to prove the first assembly boundary; avoid combining unnecessary renaming with it.
- Task 31 M5 also forecasts file relocation. Reconcile the destination map once so files do not move twice under competing plans.
- No compile/publish experiment was performed for this investigation. The accepted graph is source-backed, not a verified successful build. Exact exported types and the full relocation manifest are G0 outputs in the migration packet.

## Output And Knowledge Ownership

The existing pipeline already has typed results, reports, selectors and text/JSON rendering. Reuse it. A dictionary of command, severity and detail alone omits mode, effect progress, recovery, unknown counts and optional collections. Finding severity and overall command status are different facts.

Use one authority per fact and one review index:

| Fact | Owner |
| --- | --- |
| User goal, flow sequence and desired behavior | Existing flow/scenario documents, retaining Fxx/Cxx-xx/Xxx identities |
| Wording and typed substitution | C# factories, with the accepted later extraction into `OpenForge.Cli.OutputText` |
| Which facts to show and how to compose lists/sections | Typed selectors and renderers |
| Approved concrete output | Independently reviewed named snapshots, addressed by scenario, format, detail and stream |
| Actual resulting files, links, claims and effects | Independent semantic assertions |
| Navigation across these owners | A generated or validated review index containing references rather than copied transcripts |

Message IDs such as `library.sync.partial` identify reusable wording, not entire scenarios. Keep detail/format as output-view facets rather than inventing a new semantic scenario for every view. Markdown should reference stable keys/anchors, not source line numbers. Delete duplicated manually authored transcripts only after their authoritative replacement and links exist. Do not make production factories the sole oracle for exact wording.

There are 32 existing `*Wording.cs` files, but some shared wording code classifies exceptions and extracts paths. Extract text/formatting factories, not that domain/diagnostic behavior. Human wording, machine finding codes and JSON property names have different compatibility roles.

The preceding research rejected a general template engine for this scope: .resx does not provide typed placeholder/list contracts; RazorSlices adds ASP.NET/HTML assumptions; Scriban's runtime interpretation and surveyed generators do not supply the desired typed runtime-message contract; Handlebars.Net's runtime compilation is a poor fit. Ordinary C# already checks method arguments. A Roslyn generator is optional only if external template authoring or generated identity tooling later justifies it. Relevant primary sources: [RazorSlices](https://github.com/DamianEdwards/RazorSlices), [Scriban AOT guidance](https://scriban.github.io/docs/runtime/aot-support/), [Roslyn incremental generator cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md).

## Test And Filesystem Direction

Keep current tests during the production split. Later reorganize by the behavior/boundary actually exercised: pure rules; argument-to-request integration; observed-input-to-plan/result integration; typed-result-to-output integration; real OS integration; and complete published-CLI user journeys. A test need not correspond one-to-one with a production method or assembly.

Prefer one Integration project initially, with selectable input, processing, output and system sections and command-local ownership. Separate integration projects can be considered if enforced reference isolation or independent execution earns their extra delivery/AOT publication cost. Production project count does not imply one test assembly per tier per production assembly.

Existing overlap includes the unavailable-workspace/detail matrix in both Integration and E2E. A unit report-invariant test reads integration snapshots. Move or consolidate only after replacement evidence preserves the relevant boundary; do not delete tests merely because their names overlap. Snapshot paths use caller-file location, so move existing baselines with their owner and verify without update mode.

[PipelineTests](../../../src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Shell/PipelineTests.cs) mixes isolated operation-stage tests with full operation-to-rendering-to-writer tests. Keep isolated cases in Unit; the complete pipeline cases belong in Integration, which already references Root. Preserving current tests during the assembly split means preserving their cases and expectations, allowing necessary reference/owner changes. It does not mean retaining every old file location or adding a permanent Root dependency to Unit.

Factory reuse is valid when the test independently chooses the expected factory and arguments to verify selection/composition. It is circular when the test uses the same production selection process to derive its expected answer. Reviewed output snapshots protect wording; direct assertions protect meaningful state and effects. Keep useful typed expected values instead of replacing every assertion with a snapshot.

No interchangeable filesystem abstraction exists today. The physical resolver, lock manager, staging/rename, link applier and recovery store use real OS APIs. Start by passing existing immutable observations and bytes into pure planning/projection tests. Add a narrow ordinary-content dependency only where mutable memory-backed state is useful. Constructor substitution needs no service locator or DI container. Do not build a fake physical-identity/locking system.

Real OS integration remains necessary for symlink/reparse identity, containment/casing, locks and sharing, permissions, atomic replacement, recovery persistence and relevant failure boundaries. Invalid encoding itself can be checked from bytes in memory. [System.IO.Abstractions](https://github.com/TestableIO/System.IO.Abstractions) explicitly warns that its mock is not a complete filesystem copy; no new dependency or Native AOT qualification was adopted here.

The current architecture/testing guidance prefers real boundaries and contains a no-fake-filesystem restriction. A future approved memory-backed testing seam would require a deliberate update to that guidance. The maintainer's request authorizes investigating it now; the old wording is not a reason to dismiss the proposal.

## Migration And Observability

The discussed order is: establish the current baseline and dependency map; split projects without behavior changes; reshape tests around boundaries; extract wording without changing it; separately reduce unnecessary filesystem dependence; then, only after scenario approval, introduce desired E2E failures and implement the corresponding behavior changes. The [migration plan](cli-development/tasks/task38/plan.md) now owns this sequence and its checkpoints. This investigation does not independently dispatch work.

During structural stages preserve output bytes, JSON names/schema, findings/statuses, exits/streams and actual filesystem outcomes. Keep snapshot-update mode off. Record pre-existing failures separately. Compare snapshot paths/hashes and selected/discovered/executed populations; unchanged counts or a successful build alone do not prove behavior preservation. Qualify managed and Native AOT artifacts after assembly/resource changes.

Reviewability should expose the existing typed inputs, observations, plans, receipts, reports and rendered outputs with named evidence. Measure duration and temporary-file activity before promising speed or SSD savings. More projects do not themselves make tests faster.

The earlier scoped inventory measured 712,616 eligible text lines: 206,506 production C#, 145,619 tests/support, 142,719 snapshots, 108,444 historical/emerging Memory, 50,370 framework/contract Markdown, 32,871 Working Memory/audits and 14,684 experience records, plus other text. It excluded build/dependency/scratch directories and included the relevant current untracked Markdown. These are repository-review measurements, not runtime payload or active-context size. Centralizing prose alone will not remove the largest categories; avoid turning this record into another duplicated specification.

Parallel Luna work should follow one accepted placement/API map. Assign non-overlapping command families or cohesive file sets. One owner handles shared contracts, project/solution files, friend/public access, resource/source-generator ownership, composition and final gates. Mechanical moves can parallelize after those decisions; shared architecture cannot be independently invented by each worker.
