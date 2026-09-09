---
open-forge:
  description: Record the immutable 28-command CLI audit, validated findings, retained boundaries, coverage, and remediation decisions
  tags: [Memory, Working, Contextual, CLI, Audit, Review, Architecture, Refactoring, Testing]
---

# CLI Command Surface Audit Report

## Status And Evidence Boundary

Both sequential cohorts are complete: all 28 commands have one primary review
and a coverage disposition. Root validated the seven stable finding IDs below.
The [Task 10 capsule](cli-command-surface-audit.md) owns scope, budgets and
acceptance. This report owns findings and their disposition. Source is
`d5b77fcec6b31d7cc004c27858a24e978237a531`, tree
`d5dc23de257987be099da92c45ddaeeba0802bb3`. Review authority began at
`9be5af14`; the second cohort uses the control-only update `9da5f7db`.

This is a strategic PR-level audit of primary paths and selected direct
helpers, projections, contracts and tests. It is not an exhaustive line review
or a claim that uninspected branches are defect-free. Both cohort reviewers
use GPT-6 Astra/high. They personally read the complete C# Directive, Design
and Style files; fingerprints are retained in the Task receipt. Root validates
findings against source and authority, then owns final dispositions.

No executable or public surface changes and no new build, test or behavioral
reproduction occur in this audit. Same-worktree CLI use refreshes only required
Memory navigation. Source traces and existing test assertions are identified as
such. The unchanged Task 26 predecessor supplies managed 2837/1570/188,
native 1570/188 and managed-on-native 188 passing cases, with zero failures or
skips, plus 300 exact raw managed/native comparison files. Passing predecessor
evidence does not invalidate a contract conflict encoded by an existing test.

## Finding Register

| ID       | Priority | Classification             | Root disposition                                                              |
| -------- | -------- | -------------------------- | ----------------------------------------------------------------------------- |
| T10-R1-1 | P2       | Behavior defect            | Confirmed; recommend correcting exact-pair handling.                          |
| T10-R1-2 | P2       | Evidence-allocation defect | Confirmed; recommend three simple public journeys per affected command.       |
| T10-R1-3 | P3       | Local callable improvement | Confirmed structural concern; selected for bounded remediation.               |
| T10-R2-1 | P1       | Behavior defect            | Confirmed; correct ordinary-create recovery forwarding in Update.             |
| T10-R2-2 | P2       | Behavior defect            | Confirmed; preserve Library preparation classification and residual evidence. |
| T10-R2-3 | P2       | Evidence-allocation defect | Confirmed with narrowed Install disposition; coordinate with R1-2.            |
| T10-R2-4 | P3       | Local callable improvement | Confirmed; consolidate a narrow shared Library observation view.              |

The Overseer selects these seven IDs under the user's standing instruction to
finish the remaining corrections and refactoring, with full internal execution
authority. The three behavior corrections restore existing contracts; the two
evidence findings implement the already accepted three-journey direction; the
two local callable improvements follow the C# design and streamlined-workflow
direction. No item changes accepted product choices, support, external effects
or publication. Task 21 freezes this exact set and its invariants before work.
Earlier pending-finding wording does not impose another approval for these
routine in-scope corrections; a newly exposed consequential choice still stops
its dependent work.

### T10-R1-1: Preserve Exact Overwrite Pairs In Route Inspect

Location: `src/cli/core/OpenForge.Cli.Core/Commands/Route/Inspect/Shared/Resolution/RouteInspectSourceProjectionBuilder.cs`,
`ReadAsync`, lines 28–46. The same folder's
`RouteInspectSourceSelectionResolver.cs`, lines 79–84, and
`RouteInspectOverwriteResolutionPolicy.cs`, lines 18–37, consume the synthetic
ambiguity and block exact-path selection.

Observed source trace: with the following files, the first two form one exact
pair. The third source shares the automatic ID, not the overwrite companion.

```text
.agents/root/ambiguous.md
.agents/root/ambiguous.overwrite.md
.agents/root/ambiguous/_ambiguous.md
```

`Framework/Sources/Inventory/SourceCatalogueReader.cs`, lines 455–468 under the
Core project, already establishes pairing by exact adjacent path. Inspect
instead groups overwrite candidates by automatic ID, removes the paired layer
from both source projections, and forms `RouteOverwriteState.Ambiguous`.
Either exact base path is then blocked. Root independently traced both the
producer and command-local transformation.

The existing real-filesystem test
`src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Inspect/Resolution/RouteInspectResolverOverwriteAmbiguityIntegrationTests.cs`,
lines 8–47, asserts the conflicting behavior. The neighboring
`RouteInspectSourceProjectionBuilderIntegrationTests.cs` also asserts the
removed overwrite. Neither expectation proves agreement with the contract.

Authority: [shared source-reference overwrite resolution](../../../crystallized/documents/cli/contracts/shared/source-references/behavior.md#overwrite-resolution)
keeps an adjacent valid pair as one logical source selected by base or overwrite
path. [Route Inspect interface](../../../crystallized/documents/cli/contracts/route/inspect/interface.md)
allows exact-path identity resolution and reports a remaining non-unique ID as
attention when the route is safe and complete.
[Framework overwrite identity](../../../crystallized/documents/framework/routing/overwrites.md#identity-and-loading)
uses the shared filename stem and containing folder. The generic blocked
ambiguous-overwrite state does not justify discarding a pair already proved by
the catalogue.

Correction: preserve catalogue-owned exact pairs and retain automatic-ID
collision as a separate fact. Do not introduce a new Doctor finding, infer a
pair by ID, or create another source-identity authority. Keep unresolved ID
selection, genuine orphans, unsafe paths and actual structural route ambiguity
on their existing refusal paths.

Dependencies and evidence: correct the local projection/resolution and their
contradictory expectations. Use real catalogue fixtures for both exact base
paths and the overwrite path; retain orphan and true route-ambiguity controls.
Shared source references and Route List are direct regression neighbors. Keep
exact-path attention, interactive selection and next-action policy unchanged.
This is a high-confidence source/test trace, not a newly executed reproduction.

### T10-R1-2: Allocate Three Simple Public Journeys Per Command

The finding concerns command-specific test subjects, not method or theory-row
counts. Root independently inspected the concrete public test bodies. Under
`src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/`:

| Source                                           | Detailed command branches retained at the process tier                                                                          |
| ------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------- |
| `PublishedContextProcessTests.cs:74–115,213–259` | Native Skill validity, malformed paths, link-depth rows and semantic-error cases beyond startup and expanded-graph journeys.    |
| `PublishedFindProcessTests.cs:145–203`           | Content-validation presence, blocked-workspace variants and detailed finding cases beyond inventory/filter/projection journeys. |
| `PublishedReferencesProcessTests.cs:159–164`     | Missing source, invalid/repeated direction and filter-with-out validation matrix.                                               |
| `PublishedRouteInspectProcessTests.cs:399–525`   | Complete, attention, collision, incomplete, invalid-source and cardinality branches alongside detailed presentation tests.      |
| `CliProcessTests.cs:200–340`                     | Route List depth syntax, numeric boundaries, invalid values and repeated occurrences.                                           |

The accepted three-journey direction and the
[testing evidence tiers](../../../../patterns/testing/evidence-tiers.md)
place detailed owned branches at the cheapest proving tier. Lower-tier
neighbors already include Context operation/presentation, Find application/
document-inspection/rendering, References application/rendering, Route Inspect
presentation-status/resolution, and Route List syntax/binding/application tests.
Their coverage must be checked before removing any unique assertion.

Consequence: repeated process/fixture work and duplicate semantic expectations
increase maintenance and evidence-selection effort. No measured timing or cost
saving is claimed.

Correction: explicitly choose three simple public journeys for each affected
command. Retain owned branch assertions in Unit or Integration evidence;
move only those not already represented. Do not concatenate a matrix into three
large test methods to satisfy a count. Preserve real executable dispatch,
streams, exit codes and no-write/effect assertions in the retained journeys.

Shell-wide version, terminal, cancellation, parser integration and package
subjects retain their own evidence. A command used to exercise the Shell does
not automatically make that test a command-domain journey; conversely, naming
a domain matrix a Shell test does not change its actual subject.

Exclusions: Index's five methods can represent three domain journeys (dry-run,
apply/idempotence, invalid input) plus separate Shell/composition checks. No
Index mismatch is asserted from its count. Extension Inspect's two methods
contain three substantive public scenarios (help, semantic baseline, exact-byte
baseline). Doctor, Status, Extension List and Library List/Inspect already
have three identifiable public scenarios.

Dependencies and evidence: test-only ownership; an explicit retained-journey and
migrated-assertion inventory; affected lower-tier and public execution with
selected/discovered/executed identities. Production and accepted output remain
unchanged. The over-allocation is confirmed; exact retained cases belong in the
remediation capsule, selected from accepted user journeys.

### T10-R1-3: Make Library Inspect Comparisons One Cohesive Stage

Location: `src/cli/core/OpenForge.Cli.Core/Commands/Library/Inspect/LibraryInspectOperation.cs`,
`ObserveComparisons`, call at lines 122–130 and signature at lines 164–172.

The behavioral method takes eight arguments: resolver, request, selected
record, registered projection, eligible projection, separately extracted
inventory-complete Boolean, mutable findings and cancellation. Root confirmed
that registered/eligible projections and completeness belong to the same
selected-record/inventory stage. The method observes filesystem state, forms
comparisons and mutates findings through a second output. The caller currently
supplies consistent facts; no runtime defect is alleged.

The [C# callable-design directive](../../../../directives/csharp/design.md)
favors cohesive stage inputs and normally limits behavioral surfaces to four
or five parameters. An independently supplied completeness Boolean makes the
rule that retirement requires complete inventory harder to follow.

Correction: keep one leaf-local comparison capability under
`Library/Inspect/Shared/`, consuming cohesive selected-record/inventory facts
and returning comparisons/findings together. First reuse existing meaningful
records where appropriate; add only a genuinely cohesive stage view if needed.
Keep any model in its nearest `Models/` scope. Do not extract Library List
policy, create a generic inspection engine, or hide unrelated dependencies in
an argument bag.

Protected meaning: destination mapping, no-follow observation, unknown versus
empty inventory, incomplete-inventory retirement exclusion, finding precedence
and wire bytes. Recheck existing Library Inspect coverage/comparison,
presentation and three public journeys; add evidence only for an otherwise
unproved invariant. This is a bounded improvement recommendation, not a claim
of user-visible failure or a general source migration.

## Second Cohort Findings

### T10-R2-1: Apply Ordinary Creates In Update Without Unrelated Recovery

Location: `src/cli/core/OpenForge.Cli.Core/Commands/Update/Shared/Application/UpdateEffectApplication.cs`,
private `ApplyAsync`, lines 85–98, particularly preparation forwarding at line 96.

The adapter forwards the operation's preparation to every change.
`Commands/Update/Shared/Recovery/UpdateRecoveryOperation.cs:14` uses ordinary
recovery targets. `Framework/Recovery/Models/RecoveryBundleTarget.cs:48`
excludes ordinary Create from recovery, and `RecoveryBundleInput.cs:75` plus
`Framework/Recovery/RecoveryBundleStore.cs:228` omit those entries. These paths
are under `src/cli/core/OpenForge.Cli.Core/`.

The shared `Framework/Mutation/Application/FileChangeApplier.cs:256–276`
accepts an ordinary create with null preparation but requires an exact entry
when a non-null preparation is supplied. Therefore Update introducing a new
target while replacing lifecycle state rejects the new file. Earlier effects
may have applied, leaving partial state and retained recovery. Restoration
mixed with replacement reaches the same composition. Root independently
traced the caller, target filter, entry lookup and rejection.

Authority: [Update behavior](../../../crystallized/documents/cli/contracts/update/behavior.md#preflight-application-verification-and-recovery),
lines 245–263, excludes ordinary creates from recovery while accepting new
targets and force restoration. The reviewer initially named `interface.md` for
this line locator; root corrected it to the actual `behavior.md` authority.
Install and Route Init/Create already pass null for ordinary creates.

Correction: change Update's adapter to pass null for ordinary creates and exact
preparation for recoverable effects. Keep the shared guard for reversible
Library creates and all existing-target mutations. Do not widen ordinary-create
recovery policy or weaken shared validation.

The Integration case `Commands/Update/UpdatePlanningIntegrationTests.cs:43`,
`PlansGenuinelyNewTargetCreation`, only plans and asserts a Create decision.
Its force-restoration case is also planning evidence; the inspected operation
tests do not execute this composition. This explains the gap without claiming
all uninspected Update tests are absent.

Dependencies and evidence: real temporary-workspace Update application for a
new target plus lifecycle replacement, and mixed restoration/replacement.
Independently assert final bytes, lifecycle publication and recovery cleanup.
Run focused Update Integration and retain shared reversible-create guard
controls. This is a high-confidence source trace, not an executed reproduction.

### T10-R2-2: Retain Library Recovery Preparation Outcomes

Locations under `src/cli/core/OpenForge.Cli.Core/Commands/`:

- `Library/Attach/LibraryAttachOperation.cs:250`.
- `Library/Sync/LibrarySyncOperation.cs:252`.
- `Library/Detach/LibraryDetachOperation.cs:232`.
- `Library/Shared/Application/LibraryMutationOperationSupport.cs:126`, `Empty`.
- `Library/Shared/Completion/LibraryMutationCompletionProjection.cs:484`,
  `Recovery`, and `:325`, `Status`.

All three unsuccessful-preparation branches reduce Cancelled to cancellation
and every other failure to `LibraryUnexpectedFailureFact`. They discard both
the finite preparation state and `ResidualPath`. Empty evidence has no
preparation/cleanup value, so shared completion projects recovery NotRequested
and null path. The shared store actually returns known draft/final residuals
for cancellation and unavailable preparation, including
`Framework/Recovery/RecoveryBundleStore.cs:73–78,110–118` under Core.

Classification is also lost: shared Status lines 333–337 and Attach completion
lines 120–129 make UnexpectedFailure produce Failed. The
[Attach interface](../../../crystallized/documents/cli/contracts/library/attach/interface.md),
lines 365–368, requires Incomplete for unavailable preparation, preserves
Blocked preconditions and reserves unexpected Failed application outcomes for
the stated effect boundary. Cancellation remains Interrupted but loses the
residual. The shared [mutation/recovery design](../../../crystallized/documents/cli/technical-designs/mutation-and-recovery.md),
lines 416–418, requires the actual residual draft/final path after handled
failure or cancellation. Root verified the three callers and both projections.

Correction: carry unsuccessful preparation facts through the existing Library
execution/completion boundary. Map Blocked, Incomplete and Cancelled without
inventing an opaque successful preparation. Preserve a reported residual path
and its actual certainty; do not claim an unknown path is positively retained.
No target/record effects begin and interruption does not automatically remove
retained recovery. Keep successful preparation, cleanup and record-last rules.

Dependencies and evidence: completion cases for every finite preparation
outcome, with and without a known residual; an operation-boundary case using
a deterministic owned preparation failure/interruption seam; exact status,
recovery/residual output and absence of target/record publication. Existing
cleanup and post-preparation application tests do not by themselves qualify
these operation branches. Coordinate this correction with R2-4 at the shared
Library completion boundary.

### T10-R2-3: Remove Detailed Mutation Matrices From Public Journeys

This extends R1-2 to the second cohort without duplicating its rationale.
Root inspected the named bodies under
`src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/`:

| Source                                        | Subject to relocate or simplify                                                                                                |
| --------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| `PublishedRouteUpdateProcessTests.cs:193,231` | Attached-empty syntax spellings and a five-fixture semantic-status matrix.                                                     |
| `PublishedRouteMoveProcessTests.cs:364`       | Occupied destination, missing ownership and invalid-UTF8 safety branches alongside separate detailed schema/application cases. |
| `PublishedRouteInitProcessTests.cs:327`       | Deterministic status matrix beyond generic and Framework application journeys.                                                 |
| `PublishedInstallProcessTests.cs:65`          | Detailed ordered domain-schema assertions embedded in a relocated-artifact check.                                              |
| `PublishedRouteCreateProcessTests.cs:54`      | Detailed schema/rendering checks separately allocated from preview/apply and refusal journeys.                                 |

Use the same accepted three-simple-journey and cheapest-tier rules as R1-2.
Choose coherent journeys and preserve essential effect, stream, exit and
structured-output proof. Keep lower-tier unique assertions; do not concatenate
matrices into three large methods.

Root narrowed the Install disposition: relocated embedded-payload reachability
is a separate delivered-artifact subject and must remain. Move its duplicate
domain-schema detail to the appropriate tier; four methods alone prove no
journey-count defect. Likewise Route Move's third-positional case at line 84
is a Shell subject, and Route Init's neighboring List/Inspect case at line 407
belongs to observation commands. Coordinate those neighbors with R1-2.

Dependencies and evidence: one explicit command/Shell/artifact subject
inventory, retained journeys and unique-assertion mapping; affected lower-tier
and public execution with method/scenario/row identities kept separate.
Production and accepted CLI behavior remain unchanged. This finding shares
one remediation slice with R1-2; both discovery IDs remain traceable.

### T10-R2-4: Pass One Library Observation Stage To Completion

Location: `src/cli/core/OpenForge.Cli.Core/Commands/Library/Shared/Completion/LibraryMutationCompletionProjection.cs:139`,
`Projection`. Attach completion lines 68–75 and Sync lines 72–79 repeatedly
unpack the same four observation members; Detach lines 70–77 supplies the
corresponding source-independent form. Root verified the seven-argument
surface and all three caller shapes.

The C# callable-design directive favors cohesive immutable stage facts rather
than repeated member forwarding. This is a concrete three-consumer maintenance
concern, with no claimed behavior failure.

Correction: reuse or introduce a narrow immutable observation view at the
existing shared Library completion owner. Retain genuinely unavailable facts,
ordering and Detach's source independence. Keep the model at its nearest
`Models/` scope and avoid a general service/context bag or promotion beyond
Library. Coordinate with R2-2 because both touch completion; R1-3 remains a
separate Inspect-local comparison policy.

Evidence: existing pure completion cases for absent observations, ordering and
source-independent Detach, with focused shared-projection parity. Add tests
only where the preservation invariant otherwise lacks evidence.

## First Cohort Coverage

Each row names files actually inspected by T10-R1. Prefixes:

- P: `src/cli/core/OpenForge.Cli.Core/Commands/`
- U: `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/`
- I: `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/`
- C: `.agents/memory/crystallized/documents/cli/contracts/`

| Command           | Production and direct evidence                                                                                                                                                                                                                                                                                               | Contract and disposition                                                                                                        |
| ----------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| context           | P `Context/ContextOperation.cs`, `Context/Shared/Graph/ContextGraphBuilder.cs`; U `Context/ContextPresentationTests.cs`; I `Context/ContextOperationFindingTests.cs`                                                                                                                                                         | C `context/behavior.md`; retain local closure/projection; R1-2.                                                                 |
| doctor            | P `Doctor/DoctorOperation.cs`, `Doctor/DoctorDiagnosisReader.cs`, `Doctor/Shared/Rendering/DoctorJsonProjection.cs`; U `Doctor/DoctorProjectionParityTests.cs`; I `Doctor/DoctorApplicationIntegrationTests.cs`                                                                                                              | C `doctor/behavior.md`; fixed-domain scheduling, typed projection and read-only composition retained.                           |
| find              | P `Find/FindOperation.cs`, `Find/FindOperationFactory.cs`; U `Find/Shared/Rendering/FindHumanRenderingTests.cs`; I `Find/FindDocumentInspectionIntegrationTests.cs`                                                                                                                                                          | C `find/behavior.md`; matching/projection coverage and partial facts retained; R1-2.                                            |
| index             | P `Index/IndexOperation.cs`, `Index/Shared/Planning/IndexPlanBuilder.cs`, `Index/Shared/Operation/IndexApplicationOperation.cs`, `Index/Shared/Operation/IndexRecoveryLifecycle.cs`; U `Index/Shared/Planning/IndexPlanBuilderTests.cs`; I `Index/LibraryProjectionIndexGuardIntegrationTests.cs`; published Index test file | C `index-candidate/behavior.md`; planning, mutation, recovery, dry-run and no-op boundaries retained.                           |
| references        | P `References/ReferencesOperationFactory.cs`, operation portions, `References/Shared/Resolution/ReferencesDestinationResolver.cs`; U `References/Shared/Rendering/ReferencesJsonRenderingTests.cs`; I `References/ReferencesApplicationIntegrationTests.cs`                                                                  | C `references-candidate/behavior.md`; neutral destination resolution, local occurrence/coverage policy retained; R1-2.          |
| status            | P `Status/StatusOperation.cs`, `Status/Shared/Rendering/StatusJsonProjection.cs`; U `Status/StatusMeasurementAndPolicyTests.cs`; I `Status/StatusOperationLifecycleSnapshotIntegrationTests.cs`                                                                                                                              | C `status/behavior.md`; invocation-local snapshot and availability measurements retained.                                       |
| route list        | P `Route/List/RouteListOperationFactory.cs`, `Route/List/Shared/Filesystem/RouteListSourceProjectionBuilder.cs`; U `Route/List/RouteListDefinitionsSyntaxAndBindingTests.cs`; I `Route/List/RouteListApplicationIntegrationTests.cs`                                                                                         | C `route/list/behavior.md`; authored topology and exact pairs retained; R1-2.                                                   |
| route inspect     | P resolution/projection files in R1-1; U `Route/Inspect/RouteInspectPresentationStatusTests.cs`; I both overwrite tests in R1-1                                                                                                                                                                                              | C `route/inspect/behavior.md`, `route/inspect/interface.md`, shared source references; R1-1/R1-2.                               |
| extension list    | P `Extension/List/ExtensionListOperation.cs`; U `Extension/List/ExtensionListBindingAndPresentationTests.cs`; I `Extension/List/ExtensionListApplicationIntegrationTests.cs`                                                                                                                                                 | C `extension/list/behavior.md`; Installed/Available selection and local result retained.                                        |
| extension inspect | P `Extension/Inspect/ExtensionInspectOperation.cs`; U `Extension/Inspect/ExtensionInspectContractTests.cs`; I `Extension/Inspect/ExtensionInspectApplicationIntegrationTests.cs`; published test file                                                                                                                        | C `extension/inspect/behavior.md`; semantic versus exact-byte baseline retained; Task26 not repeated.                           |
| library list      | P `Library/List/LibraryListOperation.cs`; U `Library/List/Shared/Rendering/LibraryListPresentationTests.cs`; I `Library/List/LibraryListObservationTests.cs`                                                                                                                                                                 | C `library/list/behavior.md`, `library/list/interface.md`; registered-link observation intentionally excludes source inventory. |
| library inspect   | P `Library/Inspect/LibraryInspectOperation.cs`; U `Library/Inspect/Shared/Rendering/LibraryInspectPresentationTests.cs`; I `Library/Inspect/LibraryInspectCoverageTests.cs`                                                                                                                                                  | C `library/inspect/behavior.md`; complete inventory/comparison policy retained; R1-3.                                           |

## Shared Boundaries And Limits

Root composition explicitly registers 28 concrete bindings and seven typed
operational contributors. The reviewed Shell tree, parser, delimiter guard,
pipeline and stages preserve exact command identity, one operation/result,
selected rendering, fixed completion and cancellation-safe presentation.
Route List's special delimiter guard implements its accepted syntax; it is
not automatically an unaccepted second parser.

Immutable searches found no concrete-command imports in Shell and no command
or parser imports in Framework. This supports sampled dependency direction;
it is not a complete type-dependency proof. SourceCatalogueReader,
RouteSourceInspector and OperationalContributorCatalogue retain neutral
pairing, identity, typed reads and destination resolution. R1-1 is a local
departure from that ownership.

Sampled typed projections and exact-wire tests preserve nullable facts, finite
mappings and concrete serializer graphs. Similar envelopes do not justify a
universal result or merged serializers. Keep Context closure, References
occurrences, Route profiles, Status orientation, Doctor diagnosis and Library
List/Inspect policies distinct. File counts alone justify no migration.

Sampled tests prove Open Forge-owned mappings, composition, filesystem
observations and artifacts. Literal status/schema expectations and real owned
workspaces provide independent evidence. The audit proposes no test of an
upstream runtime, library or package manager's own behavior.

T10-R1 did not deeply inspect mutation foundations or package delivery; T10-R2
owns those. Uninspected helper branches and exhaustive serializer/test-oracle
coverage remain outside this strategic pass. No broad deep scrub or generic
framework is implicitly recommended.

## Second Cohort Coverage

The P/U/I/C prefixes above apply. Each command's concrete binding was also
inspected. Contracts supplied behavior and applicable interface sections.

| Command           | Production and direct evidence                                                                                                                                                                                                                                                           | Contract and disposition                                                                                 |
| ----------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- |
| install           | P `Install/InstallOperation.cs`, `Install/Shared/Operation/InstallApplicationOperation.cs`, `Install/Shared/Operation/InstallApplicationEffectApplier.cs`; U `Install/InstallBindingTests.cs`; I `Install/InstallCompositionIntegrationTests.cs`                                         | C `install/behavior.md`; ordinary-create distinction retained; narrowed R2-3.                            |
| update            | P `Update/UpdateOperation.cs`, `Update/Shared/Application/UpdateApplicationOperation.cs`, `Update/Shared/Application/UpdateEffectApplication.cs`; U `Update/UpdateComparisonContractTests.cs`; I `Update/UpdateOperationIntegrationTests.cs`, `Update/UpdatePlanningIntegrationTests.cs` | C `update/behavior.md`; R2-1; retain existing-target recovery and final lifecycle publication.           |
| repair            | P `Repair/RepairOperation.cs`, `Repair/Shared/Application/RepairApplicationOperation.cs`; U `Repair/LibraryRepairCompletionTests.cs`; I `Repair/LibraryRepairRecoveryIntegrationTests.cs`                                                                                                | C `repair/behavior.md`; exact selected-bundle inverse effects and source preservation retained.          |
| cleanup           | P `Cleanup/CleanupOperation.cs`, `Cleanup/Shared/Application/CleanupApplicationOperation.cs`, `Cleanup/Shared/Rendering/CleanupJsonProjection.cs`; U `Cleanup/CleanupDefinitionsAndBindingContractTests.cs`; I `Cleanup/CleanupApplicationIntegrationTests.cs`                           | C `cleanup/behavior.md`; monotonic guarded artifact deletion without new recovery retained.              |
| route init        | P `Route/Init/Shared/Application/RouteInitApplicationOperation.cs`; U `Route/Init/RouteInitDefinitionsAndBindingTests.cs`; I `Route/Init/Framework/RouteInitFrameworkAlignmentIntegrationTests.cs`                                                                                       | C `route/init/behavior.md`; local creation/alignment authority retained; R2-3.                           |
| route create      | P `Route/Create/Shared/Application/RouteCreateApplicationOperation.cs`, `Route/Create/Shared/Application/RouteCreateEffectApplication.cs`; U `Route/Create/RouteCreateDefinitionsAndBindingContractTests.cs`; I `Route/Create/RouteCreateApplicationIntegrationTests.cs`                 | C `route/create/behavior.md`; bounded source and parent-interior ownership retained; R2-3.               |
| route update      | P `Route/Update/Shared/Application/RouteUpdateApplicationOperation.cs`; U `Route/Update/RouteUpdateBodyPlannerTests.cs`; I `Route/Update/LibraryProjectionUpdateGuardIntegrationTests.cs`                                                                                                | C `route/update/behavior.md`; Library ownership guards retained; R2-3.                                   |
| route move        | P `Route/Move/Shared/Application/RouteMoveApplicationOperation.cs`; U `Route/Move/RouteMoveBindingTests.cs`; I `Route/Move/LibraryProjectionMoveGuardIntegrationTests.cs`                                                                                                                | C `route/move/behavior.md`; owned move boundary retained; R2-3.                                          |
| route remove      | P `Route/Remove/Shared/Application/RouteRemoveApplicationOperation.cs`; U `Route/Remove/RouteRemoveDefinitionsAndBindingContractTests.cs`; I `Route/Remove/LibraryProjectionRemoveGuardIntegrationTests.cs`                                                                              | C `route/remove/behavior.md`; Library exclusion and exact deletion authority retained.                   |
| extension create  | P `Extension/Create/ExtensionCreateOperation.cs`, `Extension/Create/Shared/Application/ExtensionCreateDestinationWriter.cs`; U `Extension/Create/ExtensionCreateBindingTests.cs`; I `Extension/Create/ExtensionCreateCatalogueIntegrationTests.cs`                                       | C `extension/create/behavior.md`; workspace-free create-only exception retained.                         |
| extension install | P `Extension/Install/Shared/Application/ExtensionInstallApplicationOperation.cs`; U `Extension/Install/ExtensionInstallApplicationMappingTests.cs`; I `Extension/Install/ExtensionInstallInteractionIntegrationTests.cs`                                                                 | C `extension/install/behavior.md`; exact permission/content ownership retained.                          |
| extension update  | P `Extension/Update/Shared/Application/ExtensionUpdateApplicationOperation.cs`; U `Extension/Update/ExtensionUpdateBindingTests.cs`; I `Extension/Update/ExtensionUpdateMutationIntegrationTests.cs`                                                                                     | C `extension/update/behavior.md`; command-local reconciliation and shared permission mechanism retained. |
| extension remove  | P `Extension/Remove/Shared/Application/ExtensionRemoveApplicationOperation.cs`; U `Extension/Remove/ExtensionRemoveApplicationResultTruthTests.cs`; I `Extension/Remove/ExtensionRemoveApplicationIntegrationTests.cs`                                                                   | C `extension/remove/behavior.md`; selected managed content and truthful results retained.                |
| library attach    | P `Library/Attach/LibraryAttachOperation.cs`, `Library/Attach/Shared/Completion/LibraryAttachCompletion.cs`; U `Library/Attach/Shared/Completion/LibraryAttachCompletionTests.cs`; I `Library/Attach/LibraryAttachApplicationIntegrationTests.cs`                                        | C `library/attach/behavior.md`; R2-2/R2-4; source protection and record-last effects retained.           |
| library sync      | P `Library/Sync/LibrarySyncOperation.cs`, `Library/Sync/Shared/Completion/LibrarySyncCompletion.cs`; U `Library/Sync/Shared/Completion/LibrarySyncCompletionTests.cs`; I `Library/Sync/LibrarySyncApplicationIntegrationTests.cs`                                                        | C `library/sync/behavior.md`; R2-2/R2-4; link membership reconciliation retained.                        |
| library detach    | P `Library/Detach/LibraryDetachOperation.cs`, `Library/Detach/Shared/Completion/LibraryDetachCompletion.cs`; U `Library/Detach/Shared/Completion/LibraryDetachCompletionTests.cs`; I `Library/Detach/LibraryDetachApplicationIntegrationTests.cs`                                        | C `library/detach/behavior.md`; R2-2/R2-4; source-independent exact-link removal retained.               |

The second cohort additionally inspected WorkspaceLockManager, FileChangeApplier,
relative-link application, recovery input/target/preparation/store and command
adapters. Keep the cooperating-process lease and prepared-before-state guards.
Library's shared runner retains lease identity, protected source scope,
recovery coverage, permission application and record publication last. R2-2
is missing completion evidence, not an observed source mutation.

Framework permissions and family-specific adapters retain distinct Extension
and Library policy. Command-specific planning/completion stays local. Extension
Create's workspace-free writer and Cleanup's monotonic deletion are deliberate
contract exceptions, not reasons for a generic command lifecycle.

Sampled Update source-generated JSON, Cleanup projection and Library completion
render typed facts. R2-2 loses information before rendering. Program, CliHost,
CliCompositionRoot, the root project and npm launcher/manifest/package test were
inspected for reachability. Linux/macOS/Windows x64 packages are present. The
owned shell stand-in proves forwarding composition, not native command behavior.
Later Task 13/22 remote, platform and release proof remains separately scoped.

The second cohort sampled several tests and primary implementations, expanding
only concrete finding call chains. It did not inspect every planner, renderer,
metadata branch, Route Move partial or filesystem failure permutation. No
additional security promise against malicious same-user interference is implied.

## Remediation Order And Acceptance

Task 21 receives the three behavior corrections first: R2-1 Update creates,
R1-1 exact overwrite pairs, and R2-2 Library preparation evidence. Freeze
independent failing evidence before Green. R1-3 and R2-4 then improve their
named local callable boundaries, with the latter coordinated with R2-2.
R1-2 and R2-3 share one test-allocation slice after functional evidence is
stable. This is one sequential implementation with one fresh whole-task review
and proportional correction/evidence gates defined in Task 21, not parallel
mutation of shared Library sources.

Root retains all seven discovery IDs, the narrowed Install disposition and
corrected source locators. No finding justifies a general framework, broader
support floor, compatibility mechanism or public schema expansion. Final audit
acceptance verifies coverage, links, protected objects and added-file identity;
its Task receipt records the exact feature/integration identities.
