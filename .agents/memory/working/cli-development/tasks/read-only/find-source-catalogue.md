---
open-forge:
  description: Establish the neutral Framework source catalogue and migrate Route List and Inspect without public behavior change
  tags: [Memory, Working, CLI, Task, Find, ReadOnly, Sources, Route, Framework, Architecture, Contextual]
---

# Establish The Neutral Find Source Catalogue

## Task State

- State: Complete. Child 1 is accepted at exact commit `96fe413` (`Accept Find
  source catalogue`). The parent Find Task and Child 3 are Complete and accepted
  in the commit containing this record update. Child 2 query behavior
  is accepted at exact `ff7ce3f`. Child 3 Green is accepted at exact commit
  `cb7874c` (`Implement Find presentation`) over corrected Red `a865fd1`. It is
  production/root-composition-only, with no test, support, contract, project,
  package, configuration, generated-routing, or Route behavior change. Blue is
  accepted at exact `3f81e76` (`Simplify Find JSON projection`) after changing production
  structure only in
  `src/cli/core/OpenForge.Cli.Core/Commands/Find/Shared/Rendering/FindJsonProjection.cs`:
  it replaces the duplicate local finite status and finding-code switches with
  `CliStatusDefinitions.Read(...).MachineName` and
  `FindDefinitions.ReadFindingCode(...)`, then removes the two duplicate private
  mapping methods. No behavior, public output/order/schema, test/support,
  contract, package/project/configuration, generated routing, Shell/root, Route,
  workspace-write, or Native AOT change occurs. Focused Unit `206/206` and
  generated-serialization Integration `43/43`, warning-free build, format/diff
  checks, and both bounded reviews pass with zero skips; no managed republish or
  Native AOT claim is needed. Explicit no-op Purple acceptance is recorded from
  that exact clean Blue with verdict `NO_MATERIAL_IMPROVEMENTS`; focused Unit
  `206/206`, focused Integration/serialization `43/43`, and published Find
  EndToEnd `13/13` pass with zero skips, and source diff/check is clean/empty.
  No test/support, production, contract, project, package, configuration,
  generated, Route, Shell, or root file changed, and no Purple code/test commit is
  manufactured. No Native AOT claim was made, and the record-only commit was not a
  test change. Final acceptance is recorded in the commit containing this record
  update.
- Modern C# Improvements is Complete and accepted at exact `a1cbf09`. Its
  Preflight was accepted at exact `55eb82e`; Framework, Shell/root,
  Route Inspect/family, and Route List modernization batches at exact `a90af59`,
  `fe10525`, `62a1dd9`, and `273eb45`; and Tests/support at exact clean source
  commit `6af5fb1` (`Modernize test support nullable flow`). All five ordered
  mutation batches and the final gate are accepted, with no task-local correction
  pass consumed. Find Child 2 is Complete at exact `ff7ce3f`. Child 3 focused
  Preflight is accepted at exact `28d316a`, Gray at exact `a76a217`, original Red
  at exact `22d3bff`, metadata corrections at exact `6a9a0de` and `eea3d59`, and
  supplemental escaping Red correction at exact `a865fd1`. The corrected Red
  Unit boundary is `206` total with `92` pass and `114` intentional Gray-boundary
  failures, zero skips. Child 3 Green is accepted at exact commit `cb7874c`
  (`Implement Find presentation`) over corrected Red `a865fd1`. It is
  production/root-composition-only, with no test, support, contract, project,
  package, configuration, generated-routing, or Route behavior change. Its final
  bounded correctness review is `PASS` with no material findings. Blue is
  accepted at exact `3f81e76` (`Simplify Find JSON projection`) after changing production
  structure only in
  `src/cli/core/OpenForge.Cli.Core/Commands/Find/Shared/Rendering/FindJsonProjection.cs`:
  it uses the canonical status and finding-code readers and removes the two
  duplicate private mapping methods. Its focused evidence and both bounded
  reviews pass with zero skips; fresh bounded Blue correctness review is `PASS`,
  confirming all 7 status and 17 finding-code mappings plus undefined-value
  exception behavior, unchanged JSON model/property order/context, and
  source-generation/AOT-safe static readers. Fresh local improvement review is
  `APPROVED — NO_MATERIAL_IMPROVEMENTS`. No managed republish or Native AOT claim
  is needed. Explicit no-op Purple acceptance is recorded from that exact clean
  Blue with verdict `NO_MATERIAL_IMPROVEMENTS`; the no-op Purple acceptance is
  recorded at exact `426d4f5`, and final acceptance is recorded in the commit
  containing this record update.
- Responsible role: Mastermind. This child owns a cross-cutting architecture
  increment; delegation is allowed only after the callable and evidence packet is
  frozen.
- Parent: [Implement Find](find.md).
- Task source: This file.
- Last updated: 2026-08-25.
- Parent planning boundary: `b2e3106` (`Plan and freeze CLI find`) on
  `feature/cli-find`; `develop` remains `e77902a`. Child 1 Preflight commit
  `7e081ef` (`Freeze Find source catalogue preflight`) is the exact Gray
  predecessor. Gray commit `7b34cc7` (`Freeze Find source catalogue contracts`)
  is the frozen production predecessor for Red. The intervening `0b77019`
  records C# design rules and does not change `src/cli/`.
- Production/source baseline: exact `063c59d` (`Improve and accept generic CLI
  structure`). The planning boundary and later Child 1 execution baselines remain
  distinct.
- Accepted boundary: Green completed the neutral Framework migration and Route
  consumer correction. The detailed evidence and residual limitation are recorded
  below; the containing commit is the accepted Child 1 boundary.

## Accepted Child 1 Result

- `Framework/Sources/` is the sole neutral authority for source identity, forms,
  references, catalogue, selected reads, Loader facts, topology, and route facts.
  Route List and Route Inspect retain command-local projections and policy.
- Obsolete Route-prefixed identity, catalogue, Loader, and topology authorities,
  and every legacy overload or runtime path are deleted. No wrappers or dual
  wiring remain. One invocation-scoped `SourceDocumentReader` is shared, and
  catalogue formation remains body-free.
- Final review corrections are accepted: Inspect physical layers derive from the
  actual Route projection and honor `BaseOnly`; Route List respects requested
  logical roots; filtered admitted child entrypoints report excluded parent
  support; filtered multi-member physical-alias projections retain the selected
  equivalence; neutral model path validation uses `SourceLogicalPath`; and the
  dead compatibility parameter is removed.
- Green changed no tests, project or package files, root, generated, Shell, Find,
  public schema, or output surfaces.
- Release build, format, diff, and audits pass with zero warnings or errors.
  Focused Unit is `562/562` and Integration is `183/183`; complete Unit is
  `617/617` and Integration is `241/241`; every run has zero skips. Managed
  published EndToEnd is `57/57`, managed EndToEnd against the Native AOT root is
  `57/57`, Native AOT Integration is `241/241`, and Native AOT EndToEnd is
  `57/57`. The package vulnerability audit is clear.
- The public no-write gate ran both managed and Native AOT published executables
  with `route list --json` and `route inspect root --json` against one owned
  8-entry workspace containing a Loader root, ID collision, and paired overwrite.
  Workspace snapshots were exact after every run. Route List returned blocked,
  exit `5`; Inspect returned incomplete, exit `3`, because the intentional combined
  collision affects coverage. JSON remained typed and stderr was empty.
- Split Framework, Route List, and Route Inspect correctness reviews and the local
  improvement review are complete. The listed findings are resolved, and the final
  bounded follow-up passed. Deterministic neutral mid-traversal cancellation
  remains a recorded evidence limitation, not a blocker.
- Next action: This Child 1 record remains Complete. Find Child 3 and the Find
  parent are Complete and accepted in the commit containing this record update.
  The final managed/native gate and package, artifact, static, public no-write,
  and protected-surface audits passed. The sole immediate continuation is to
  freshly verify `develop`, squash-integrate the accepted `feature/cli-find` tip
  into local `develop`, commit that one squash, prove exact tree equality, do not
  push, and halt. References remains Planned and must not start in this session.
  `CLI-EDGE-001` remains non-product only.

## Expected Outcome

`Framework/Sources/` exposes one fresh neutral catalogue of physical source facts
without reading file bodies, one selected-layer strict reader, and neutral
on-demand route facts. Route List and Route Inspect consume those boundaries
through adapters, with no Find query, renderer, Markdig dependency, or public
Route behavior change.

## Relationships And Authority

| Relationship | Link | Relevance |
| --- | --- | --- |
| Parent | [Implement Find](find.md) | Defines the Find outcome, sequence, and protected contract. |
| Route consumers | [Route Discovery](../route-discovery/_route-discovery.md) and [Route Inspect](../route-discovery/route-inspect.md) | Supply the existing consumers whose behavior must remain unchanged. |
| Architecture | [CLI Architecture](../../../../crystallized/documents/cli/architecture.md) | Defines Framework direction, filesystem identity, source locality, and evidence boundaries. |
| Source references | [Interface](../../../../crystallized/documents/cli/contracts/shared/source-references/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/shared/source-references/behavior.md) | Define IDs, paths, collisions, overwrite identity, containment, and typed resolution meaning. |
| Find design | [Find Technical Design](../../../../crystallized/documents/cli/contracts/find/technical-design.md) | Defines the Find source boundary and no-body-read direction. |
| Implementation rules | [CLI Implementation Directive](../../../../../directives/open-forge/cli/implementation.md) and [Source Locality](../../../../../directives/source-locality.md) | Bind BCL-first safety, dependency direction, locality, and no-forwarding rules. |
| Evidence | [Test Evidence Integrity](../../../../../directives/open-forge/testing/evidence-integrity.md) and [Evidence Tiers](../../../../../patterns/testing/evidence-tiers.md) | Define real-OS integration, isolation, and tier placement. |
| Phase workflow | [Task Lifecycle](../../../../../workflows/development/task-lifecycle.md) | Defines the Preflight, Gray, Red, Green, review, and commit boundaries. |

## Allowed And Protected Surfaces

### Allowed

- New neutral Framework source capabilities under the following exact paths,
  relative to `src/cli/core/OpenForge.Cli.Core/`, with these exact namespaces:
  - `Framework/Sources/Identity/` — `OpenForge.Cli.Core.Framework.Sources.Identity`;
  - `Framework/Sources/Inventory/` — `OpenForge.Cli.Core.Framework.Sources.Inventory`;
  - `Framework/Sources/Reading/` — `OpenForge.Cli.Core.Framework.Sources.Reading`;
  - `Framework/Sources/Routing/` — `OpenForge.Cli.Core.Framework.Sources.Routing`; and
  - `Framework/Sources/Models/{Identity,Inventory,Reading,Routing}/` —
    `OpenForge.Cli.Core.Framework.Sources.Models.{Identity,Inventory,Reading,Routing}`.
- Existing
  `Commands/Route/Shared/Source/`,
  `Commands/Route/Shared/Models/{Source,Loader,Topology}/`,
  `Commands/Route/Shared/Loader/`, and
  `Commands/Route/Shared/Topology/` only as needed to move identical source and
  route facts into Framework. Route-specific metadata and command policy remain
  under Route. Route-prefixed duplicate authorities may be removed; forwarding
  wrappers are not allowed.
- Existing Route List and Route Inspect inventory, catalogue, resolution, and
  projection adapters, plus their directly affected Unit and real-OS Integration
  tests under `src/cli/tests/`.
- The active Working records needed to record this child's phase and acceptance.

### Protected

- Shell, root composition, rendering, JSON, public outputs, package graph,
  project files, generated code, and every unrelated command.
- The Find query, Markdown parsing, presentation, and EndToEnd registration.
- Route status, findings, topology policy, result formation, human output, JSON,
  and every accepted public Route behavior.
- Crystallized authority remains protected. During this Preflight's authoring and
  recording, no files outside the seven named Working Markdown paths may change.
  Later Child 1 phases may change only the phase-specific production and test
  paths listed above.

## Exact Child 1 Preflight Contract

This section closes the detailed Preflight boundary. It records callable names,
data shapes, invariants, migration behavior, and evidence before Gray. It does
not claim that any of these types, tests, or behaviors are implemented.

### A. Framework paths and namespaces

The exact Framework paths are the four source capability paths and the four
topic paths under `Framework/Sources/Models/` listed above. No project, package,
Shell, root-host, or presentation change is part of Child 1.

### B. Identity and reference surface

The following names and meanings move mechanically from the current Route-
prefixed authorities. Existing signatures remain the same unless the neutral
name is the mechanical replacement. No forwarding wrapper is permitted at Green
completion.

- `SourceLogicalPath` retains these exact members: `AgentsRoot`, `IsCanonical`,
  `IsCanonicalRoot`, `IsCanonicalSource`, `IsCanonicalSegment`, `ReadParent`,
  `ReadFileName`, `Combine`, and `ToLexicalPath`. `IsCanonicalSource` and the
  compatibility member `IsCanonical` accept only `.agents/<segments>` source
  paths. `IsCanonicalRoot` additionally accepts `.agents` itself. `Combine` and
  `ToLexicalPath` accept that root, while `ReadParent` requires a source path.
- `SourceDocumentForm` retains exactly these values:
  `Loader`, `CanonicalEntrypoint`, `IndexEntrypoint`,
  `UnderscoreIndexEntrypoint`, `ReferencesEntrypoint`,
  `UnderscoreReferencesEntrypoint`, `Skill`, `Markdown`, and
  `OverwriteCompanion`.
- `SourceFormClassifier` retains `TryClassify`, `Matches`, `IsEntrypoint`,
  `IsCompatibilityEntrypoint`, and `IsCompatibilityFileName`.
- `SourceIdentity` retains `DeriveId`, `IsValidId`, and
  `IsRecognizedEntrypointPath`.
- `SourceReferenceKind` is exactly `SourceId` or `SourcePath`.
- `SourceReferenceParseState` is exactly `Valid` or `Invalid`.
- `SourceReferenceParseResult` retains the properties `State`, `Kind`,
  `AttemptedId`, `AttemptedPath`, and `Cause`, plus the factories `ValidId`,
  `ValidPath`, `InvalidId`, and `InvalidPath`.
- `SourceReferenceParser.Parse(string)` is the neutral parser entrypoint.

Identity, form classification, and reference parsing remain Framework facts. They
do not carry command status, findings, rendering, or next-action policy.

### C. Inventory surface

- `SourceCatalogueRequest(CliWorkspace, IEnumerable<string> logicalRoots)` has
  `Workspace` and `LogicalRoots`. Its roots are nonempty and satisfy
  `SourceLogicalPath.IsCanonicalRoot`; `.agents` is the Find and Route default.
  Duplicate and overlapping roots are accepted; the reader deduplicates their
  canonical candidates without dropping a distinct root outcome.
- `SourceCandidate(canonicalPath, SourceDocumentForm? form, string? automaticId,
  PhysicalPathState physicalState, string? physicalPath,
  string? physicalParentPath)` retains the attempted canonical path, recognized
  form and lexical automatic ID when available, physical-resolution state,
  resolved physical path when contained, and the contained physical parent from
  which it was encountered. Unsafe recognized candidates therefore remain
  addressable evidence without becoming logical sources.
- `SourceLayer(canonicalPath, physicalPath, form, SourceLayerKind)` retains a
  contained, absolute, normalized physical path, its canonical logical path, its
  form, and its layer kind. `SourceLayerKind` is exactly `Base` or `Overwrite`,
  and the constructor validates the corresponding layer shape.
- `SourceLogicalIdentity(automaticId, canonicalBasePath)` exposes those exact
  properties. It does not infer route, scope, or command meaning.
- `SourceLogicalSource(identity, base, overwrite?)` retains one identity, one
  base layer, and an optional adjacent overwrite layer. It validates the exact
  adjacent pair.
- `SourceCatalogueIssueStage` is exactly `Root`, `Directory`, `Candidate`,
  `Identity`, or `Pairing`.
- `SourceCatalogueIssueCode` is exactly `RootMissing`, `RootUnsafe`,
  `RootUnavailable`, `DirectoryUnavailable`, `CandidateUnsafe`,
  `CandidateUnavailable`, `IdentityUnavailable`, `IdentityCollision`,
  `PhysicalAlias`, and `OrphanOverwrite`. The neutral catalogue does not expose
  `AmbiguousOverwrite`.
- `SourceCatalogueIssue(stage, code, attemptedCanonicalPath, relatedPaths,
  string? scopePhysicalPath, FilesystemFailure?)` retains only the typed catalogue
  issue facts. The stage must match the code. `ScopePhysicalPath` is the nearest
  proven-contained physical directory that establishes where a non-root issue was
  encountered; root issues carry `null`. It carries no CLI status or prose-output
  policy.
- `SourceCatalogue(CliWorkspace, candidates, sources, issues, isCancelled)`
  exposes `Workspace`, ordered `Candidates`, `Sources`, `Issues`, and
  `IsCancelled`, plus
  `FindCandidateByPath`, `FindAllCandidatesById`, `FindAllById`, `FindByPath`,
  `SelectAll`, and `Select`. `FindAllCandidatesById` includes unsafe recognized
  candidates with the requested lexical ID. `FindAllById` returns only retained
  logical sources. `FindByPath` resolves either a base path or a valid overwrite
  path to one logical source.
- Immutable `SourceCatalogueSelection` is the explicit post-filter handoff. It
  exposes `Sources`, `Candidates`, `Issues`, and `RootIssues`.
  `SourceCatalogueSelectionScope(canonicalDirectoryPath, physicalDirectoryPath)`
  is one proven-contained physical folder expansion boundary.
  `SourceCatalogueSelectionRequest(IEnumerable<SourceLogicalSource>,
  IEnumerable<SourceCatalogueSelectionScope> includedScopes,
  IEnumerable<SourceCatalogueSelectionScope> excludedScopes)` carries the final
  effective logical sources and the include/default and exclude physical-folder
  expansions that produced them. Scopes are unique and ordinally ordered.
  `SourceCatalogue.Select(request)` validates catalogue membership and
  canonical-base uniqueness. It includes both candidates of each selected
  base/overwrite source. It also includes a source-less candidate only when the
  candidate's proven physical parent is inside an included scope and outside
  every excluded scope.
- Selection derives non-root issues rather than accepting a caller-chosen issue
  list. A directory issue is relevant when its `ScopePhysicalPath` is inside an
  included scope and outside every excluded scope. Candidate,
  `IdentityUnavailable`, and `OrphanOverwrite` issues are relevant when their
  attempted candidate is selected. `IdentityCollision` and `PhysicalAlias`
  related paths are projected to selected sources/candidates and retained only
  while at least two selected paths participate. `SelectAll` selects all
  catalogue facts. Root-stage issues always enter `RootIssues`; every excluded
  directory, candidate, identity, pairing, collision, and alias issue is absent
  from matching and projection coverage. Selection performs no body read.
- `SourceCatalogueReader.ReadAsync(SourceCatalogueRequest, CancellationToken)`
  is fresh, body-free, and deterministic. It enumerates every requested root,
  resolves containment and cycles through the existing filesystem primitives,
  classifies Markdown candidates, forms base/overwrite sources, deduplicates
  repeated or overlapping roots by canonical candidate path, records typed
  issues, persists nothing, and retains safe accumulated facts on cancellation.

### D. Issue and invariant decisions

The issue codes have these exact meanings:

| Stage | Code | Meaning |
| --- | --- | --- |
| `Root` | `RootMissing` | The requested logical root is missing. |
| `Root` | `RootUnsafe` | The requested root is external, dangling, cyclic, or in another unsupported safety state. |
| `Root` | `RootUnavailable` | The requested root is contained but is not a directory or cannot be inspected through an access or I/O failure. |
| `Directory` | `DirectoryUnavailable` | Contained directory enumeration failed through an access or I/O failure. |
| `Candidate` | `CandidateUnsafe` | A child is external, dangling, cyclic, or in another unsupported safety state. |
| `Candidate` | `CandidateUnavailable` | A contained candidate encountered a race, attribute failure, or read-entry failure. |
| `Identity` | `IdentityUnavailable` | Only a recognized source path has no derivable automatic ID. |
| `Identity` | `IdentityCollision` | More than one retained logical source has the same automatic ID. |
| `Identity` | `PhysicalAlias` | Distinct contained canonical candidates resolve to the same physical identity. |
| `Pairing` | `OrphanOverwrite` | An overwrite companion has no exact adjacent base and is not a logical source. |

Candidates sort by canonical candidate path with ordinal comparison. Sources sort
by automatic ID with ordinal comparison and then canonical base path with ordinal
comparison. Issues sort by stage declaration order, code declaration order,
attempted canonical path, and then the ordinal sequence of already sorted related
paths. No filesystem enumeration timing or hash order becomes public ordering.

Duplicate and overlapping roots deduplicate by canonical logical candidate and
never hide a separate root failure. Contained physical aliases remain distinct
candidates and valid logical sources, with an ordered `PhysicalAlias` issue.
Unsafe aliases are not admitted. The reader compares resolved physical paths
with the existing physical identity comparer, and every alias read result keeps
the canonical logical path requested by that layer.

Identity collisions retain every logical source and their ordered related base
paths. An overwrite is paired only with its exact adjacent base. An orphan is
never a logical source. The neutral catalogue does not reproduce Inspect's old
ID-based ambiguity. The Route Inspect adapter recreates that accepted ambiguity
locally when identity-collision facts and overwrite presence meet the existing
Inspect policy. Route List and Find use exact adjacent pairing.

### E. Selected-layer reader

- `SourceLayerVerificationState` is exactly `Verified`, `Missing`, `Unsafe`,
  `Unavailable`, `Changed`, or `Cancelled`. `SourceLayerVerification` retains the
  requested layer, state, current physical path when known, and direct
  `FilesystemFailure` when present. `Changed` means the current contained physical
  identity no longer equals the catalogue layer identity.
- `SourceDocumentReadResult` exposes `Layer`, `Verification`, and nullable `Read`.
  `Read` is a logically rebound `FileReadResult<string>` only after verified
  physical identity; missing verification is represented as a missing read, while
  unsafe, unavailable, changed, and cancelled verification never opens the file.
- `SourceDocumentReader(CliWorkspace)` is invocation-scoped and exposes
  `Workspace`. Its `Verify(SourceLayer, CancellationToken) ->
  SourceLayerVerification` entrypoint re-resolves the layer from its canonical
  logical path through the existing component-wise physical resolver and compares
  the result with the captured layer identity.
  `ReadAsync(SourceLayer, CancellationToken) -> ValueTask<SourceDocumentReadResult>`
  verifies before the first read and delegates byte decoding only to the existing
  `StrictUtf8FileReader`. It is not a second decoder or a compatibility wrapper.
- Verification is memoized once per canonical layer path, and one raw typed read
  is memoized per verified physical path for the reader's operation lifetime.
  Each returned read is rebound to the requesting layer's canonical logical path,
  so aliases never inherit another requested path.
- Cancellation, verification, and read outcomes are not retried. No persistent
  or session artifact is created.
- One reader instance is shared by neutral route resolution and the Route
  metadata/profile adapters. The shared instance prevents rereads while keeping
  the catalogue body-free.

### F. Neutral Loader, topology, and route facts

The following mechanical Route algorithms and models become neutral Framework
authorities:

- `SourceLoaderDeclarationParser.TryParse`.
- `SourceLoaderDestinationParseState` with exactly `Valid`, `Malformed`, and
  `Unsafe`; `SourceLoaderDestinationParseResult` exposes
  `AttemptedDestination`, `DecodedDestination`, `CanonicalPath`, and `Cause`.
- `SourceLoaderEntriesParseState` with exactly `Valid` and `Malformed`;
  `SourceLoaderEntriesParseResult` exposes `Destinations`, `Cause`, and
  `AttemptedDestination`.
- `SourceLoaderDestinationParser.Parse` and `SourceLoaderEntriesParser.Parse`.
- `SourceRouteTopologyBuilder.Build(IEnumerable<SourceLogicalSource>,
  IReadOnlyList<string> loaderRootPaths)` admits every selected entrypoint;
  selected Markdown only when its containing directory has a selected entrypoint;
  and selected Skill source only when the parent of its containing directory has
  a selected entrypoint. It excludes Loader and overwrite layers. Generated
  `Entries` never add topology input.
- `SourceRouteState` with exactly `Routed`, `Unrouted`, `Ambiguous`, and
  `Unavailable`.
- `SourceRouteParentState` with exactly `None`, `Resolved`, and `Ambiguous`.
- `SourceRouteNode`, which retains `Identity`, `ParentState`, ordered
  `ParentPaths`, and ordered `ChildPaths`.
- `SourceRouteTopology`, which exposes ordered `Nodes`, ordered
  `LoaderRootPaths`, exact-path `FindByPath`, `ReadAbsoluteDepth`, and
  `TryReadRelativeDepth`. These members retain the exact planned meanings and
  ordinal ordering.
- `SourceRouteFact`, which retains `Identity`, `State`, and `Route`.
  It also exposes `IsIdentityUnique`, computed among selected logical sources.
  `Route` is the automatic source ID only for `Routed` with unique selected
  identity; it is `null` for every other fact. A structurally routed identity
  collision therefore remains `Routed` for Route compatibility but is unavailable
  as Find route metadata. The neutral fact does not claim or infer scope.
- `SourceRouteIssueCode`, exactly `LoaderUnavailable`, `LoaderMalformed`,
  `LoaderUnsafe`, `RouteAmbiguous`, and `RouteSupportUnavailable`.
- `SourceRouteIssue`, which retains code, canonical path, ordered related paths,
  a non-negative stable occurrence assigned from Loader declaration or sorted
  topology-source order, and a bounded direct cause without command status,
  finding code, or next-action policy.
- `SourceRouteFacts`, which contains `Topology`, `RouteFacts`, `Issues`,
  `AreLoaderRootFactsComplete`, and `IsCancelled`.

Route facts sort by source automatic ID and then canonical base path, both
ordinal. Route issues sort by issue-code declaration order, canonical path, the
ordinal sequence of already sorted related paths, and then stable occurrence.
Loader root and topology ordering remain as defined by their models. Route adapters may
translate issue meaning but may not expose discovery timing or hash order.

`SourceRouteFactsRequest(SourceCatalogue, SourceCatalogueSelection)` uses the
selection's sources as the complete topology,
physical-verification, and read allowlist, not merely as the direct subject.
Route List and Route Inspect pass `SelectAll()`. Find later passes only its
effective selection. The resolver never widens this allowlist.

`SourceRouteFactsResolver.ResolveAsync(SourceRouteFactsRequest, SourceDocumentReader, CancellationToken)`
is the neutral route-facts entrypoint. It validates that the catalogue,
selection, and reader belong to the same lexical and physical workspace before
any read or physical verification.

The resolver builds topology body-free from selected forms and paths. It reads
only a selected Loader layer for Loader facts. Entrypoint and other source bodies
remain available to Route metadata/profile adapters through the same
`SourceDocumentReader`. A catalogue Loader candidate outside the selection emits
`LoaderUnavailable` and leaves root facts incomplete. A selected Loader is
physically reverified before its memoized read. A destination or expected parent
that exists in the catalogue but lies outside the selection emits
`RouteSupportUnavailable`; affected route facts are `Unavailable`, never silently
`Unrouted`.

Missing Loader is complete neutral route evidence: `LoaderRootPaths` is empty,
`AreLoaderRootFactsComplete` is `true`, and no issue is emitted. Route Inspect
maps that fact to its current complete empty-root behavior. Route List's
no-operand adapter must instead emit its current `LoaderUnavailable`, incomplete
coverage, and unchanged next action.

For a present Loader, an unreadable layer produces `LoaderUnavailable`; malformed
Entries retain earlier parsed destinations and produce `LoaderMalformed`;
duplicate declarations are detected after canonical destination resolution,
retain the first root, and produce `LoaderMalformed`; an unsafe destination or
changed identity produces `LoaderUnsafe`; and ambiguous authored parentage
produces `RouteAmbiguous`. Valid roots discovered before a later malformed,
duplicate, unsafe, unavailable, or cancelled destination remain ordered safe
facts while `AreLoaderRootFactsComplete` is `false`. Loader roots sort by canonical
path after first-occurrence duplicate detection.

The Loader receives no `SourceRouteFact` and no topology node. Every selected
non-Loader source receives one fact. A selected source is `Routed` when its
unambiguous parent chain reaches a retained Loader root, including when later
Loader evidence is incomplete. It is `Ambiguous` when its admitted parent chain
is ambiguous. It is `Unavailable` when required selected support is missing or
Loader facts are incomplete and no retained root proves the route. Otherwise it
is `Unrouted`. Catalogue and route cancellation retain safe accumulated facts and
expose only the cancellation boolean; commands map interruption at their own
boundary.

### G. Route-family projections and adapters

- `RouteSourceDocument`, `RouteSourceMetadata`, and `RouteSource` remain
  Route-family projections. Their constructors consume neutral forms, identities,
  and an explicit projected base plus optional projected overwrite layer. They may
  not derive identity, classify forms, or pair layers independently. The projected
  overwrite presence, not the neutral source's available layer, sets
  `RouteSourceMetadata.IsOverwritePresent`.
- Gray places `RouteSourceProjection`, `RouteSourceProjectionBuildResult`, and
  `RouteSourceProjectionSet` under
  `Commands/Route/Shared/Models/Source/`; `RouteSourceProjector` under
  `Commands/Route/Shared/Source/`; `RouteListSourceProjectionBuilder` under
  `Commands/Route/List/Shared/Filesystem/`; and
  `RouteInspectSourceProjectionBuilder` under
  `Commands/Route/Inspect/Shared/Resolution/`, with matching namespaces.
- `RouteSourceProjection(SourceLogicalSource, RouteSource?,
  SourceDocumentReadResult baseRead, SourceDocumentReadResult? overwriteRead)`
  exposes `LogicalSource`, nullable `Source`, `BaseRead`, and nullable
  `OverwriteRead`. Reads must reference the logical source's exact layers. A
  projection has a `RouteSource` when layer verification permits the existing
  Route document/read-state model; otherwise the typed neutral read outcomes
  remain associated for command-local issue mapping.
- `RouteSourceLayerProjection` is exactly `ExactLayers` or `BaseOnly`.
  `ExactLayers` projects the neutral source's adjacent overwrite when present.
  `BaseOnly` reads and projects only the base and forms Route metadata with no
  overwrite present; it exists only for Inspect's accepted ambiguous-overwrite
  compatibility.
- `RouteSourceCatalogue` is replaced by `RouteSourceProjectionSet`, which
  takes `IEnumerable<RouteSourceProjection>` and
  `IEnumerable<RouteOverwriteFact>`, retains current canonical-path ordering and
  projected `RouteSource` reference identity, and exposes `Sources`,
  `IdentityCollisions`, `OverwriteFacts`, `FindAllById`, `FindByPath`, and
  `FindOverwriteByPath` with their current Route meanings. `FindByPath` maps every
  projected base and only a projected paired overwrite; an ambiguous or orphan
  overwrite resolves only through `FindOverwriteByPath`. The set is not
  source-identity authority; each projection retains its neutral logical source.
- `RouteSourceProjectionBuildResult(RouteSourceProjectionSet,
  IEnumerable<RouteSourceProjection>,
  IEnumerable<SourceDocumentReadResult> sourceLessReads, bool isCancelled)`
  exposes `ProjectionSet`, ordered `Projections`, ordered source-less overwrite
  `ReadResults`, and `IsCancelled`. Every projection belongs to one selected
  logical source. Every source-less read's `Layer.CanonicalPath` belongs to one
  selected contained overwrite candidate and has no logical source. Command
  adapters translate these associated typed outcomes through their existing issue
  and status policies.
- `RouteSourceProjector.ReadAsync(SourceLogicalSource,
  RouteSourceLayerProjection, SourceDocumentReader, CancellationToken)` returns
  `ValueTask<RouteSourceProjection>`, forms Route-family body, read, and metadata
  projections, and reuses the reader's memoized reads. Metadata is parsed only
  from a verified base read and receives the selected projection mode before
  construction. Existing Route profile stages consume the projected documents
  and may not call `StrictUtf8FileReader` directly.
- `RouteListSourceProjectionBuilder.ReadAsync(SourceCatalogueSelection,
  SourceDocumentReader, CancellationToken) -> ValueTask<RouteSourceProjectionBuildResult>`
  reads the complete `SelectAll()` selection once, keeps neutral exact adjacent
  pairs, and projects paired and orphan overwrite facts plus identity collisions
  into current Route models.
- `RouteInspectSourceProjectionBuilder.ReadAsync(SourceCatalogueSelection,
  SourceDocumentReader, CancellationToken) -> ValueTask<RouteSourceProjectionBuildResult>`
  reads the complete `SelectAll()` selection once and applies this exact local
  compatibility rule for each neutral overwrite candidate: two or more same-ID
  bases produce ordered `Ambiguous`, call the projector in `BaseOnly` mode for
  every candidate base, and record every candidate base path in ordinal order; a
  neutral exact-adjacent pair with no same-ID collision produces `Paired`; every
  other overwrite produces `Orphan`, including a non-adjacent overwrite with one
  same-ID base. Each ambiguous or orphan overwrite is read once as a source-less
  layer result for its fact. An ambiguous overwrite path and every candidate base
  path remain blocked by the current Inspect policy.
- Route List uses the full neutral catalogue and route topology, then applies its
  current metadata eligibility, Loader/depth/row/finding/coverage policy.
  `PhysicalAlias` is carried into existing alias facts and produces no new public
  finding unless the existing selected-path policy already does. `IdentityUnavailable`
  remains the existing unsupported/authored-form command policy.
- Route Inspect maps neutral route states exactly: `Routed` to `Routed`, an
  `Unrouted` entrypoint to `Detached`, another `Unrouted` source to `NotRouted`,
  `Ambiguous` to `Ambiguous`, and `Unavailable` to `Unresolved`. It retains
  Loader roots and topology for its existing graph and profile. Its existing
  identity-collision-plus-overwrite policy recreates route-inspect ambiguous
  overwrite locally; neutral exact pairing is unchanged.
- Route Inspect preserves its current identity-collision observation separately
  from structural route state. Find requires both a structurally unambiguous fact
  and `IsIdentityUnique`; otherwise its route metadata projection is unavailable.
- Route Inspect forms its full Route projection set before resolution/profile
  formation. Loader parsing, visible `Entries`, metadata, reading reasons,
  inherited and local Axioms, and physical measurements reuse those projected
  documents and the one invocation reader. The adapter therefore includes every
  current profile source in `SelectAll()` and never creates a second body reader.
- Route Inspect preserves its existing exact-path terminal path: it resolves and
  classifies a requested path before catalogue or projection formation, and a
  missing or unsafe path returns the current invalid or blocked resolution without
  enumerating or reading the source universe. ID selection and a contained exact
  path continue through one catalogue and one full projection set.
- Route List and Route Inspect retain every public status, finding, result, help,
  JSON, and `next` value. No adapter reparses or re-enumerates the source
  universe. Find later consumes neutral route facts only for its metadata
  projection: an unambiguous routed source exposes its automatic ID as `route`,
  an unrouted source exposes `null`, and ambiguous or unavailable metadata makes
  that projection unavailable and coverage incomplete. Find does not infer scope.

### H. Gray, Red, and Green boundaries

This detailed Preflight is reviewed and becomes the recorded boundary in its
feature commit. Gray adds compile-only neutral models, callables, and Route
adapter contracts. Every new behavior entrypoint throws `NotSupportedException`.
Gray adds no tests, behavior, or migration. Temporary coexistence with the old Route
authorities is allowed through Gray and production-free Red and must be named
explicitly in both phase records. It may not create dual runtime wiring. Green
removes the old authorities only after List, Inspect resolution, Inspect profile,
and presentation consumers use the new boundary. No forwarding wrapper is
allowed.

Gray acceptance requires the build, format, diff, and targeted correctness and
improvement reviews. Red starts only after accepted Gray. Red is production-free
and completes all affected Route regressions and new evidence. Existing accepted
Route tests remain green; new neutral and adapter tests fail only at the Gray
`NotSupportedException` entrypoints.

The exact Red test roots and adapted paths are:

- New Unit paths:
  - `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Sources/Identity/`;
  - `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Sources/Models/`;
  - `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Sources/Models/Identity/`;
  - `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Sources/Models/Inventory/`;
  - `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Sources/Models/Reading/`;
  - `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Sources/Models/Routing/`;
  - `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Sources/Routing/`.
- New Integration paths:
  - `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Sources/Inventory/`;
  - `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Sources/Reading/`;
  - `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Sources/Routing/`.
- `RouteLogicalPath*`, `RouteSourceIdentity*`, and
  `RouteSourceReferenceParser*` Unit tests move from
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Shared/Source/`
  to `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Sources/Identity/`.
  `RouteMetadataParserListRegressionTests.cs` and
  `RouteMetadataParserRedTests.cs` remain under the Route shared source path and
  adapt only to neutral forms/documents.
  Moved Loader and topology Unit tests move from
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Shared/Loader/`
  and
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Shared/Topology/`
  to `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Sources/Routing/`.
  New neutral Loader/topology model tests are authored under
  `Framework/Sources/Models/Routing/`. Existing
  `RouteSourceCatalogueModelTests.cs`,
  `RouteSourceDocumentAndMetadataModelTests.cs`, `RouteSourceTestData.cs`, and
  `RouteSourceValidationModelTests.cs` move from the flat Route shared Models
  path into its `Models/Source/` child, become projection-focused, and adapt to
  neutral inputs. The reusable real-OS inventory cases move from
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/List/Shared/Filesystem/`
  to
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Sources/Inventory/`
  while Route List finding/status mapping cases remain under the Route List path.
  Current Route List Loader destination cases remain Route List regressions; new
  neutral Loader/route-facts evidence is authored under
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Sources/Routing/`.
- Adapted Route List Unit paths are
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/List/`,
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/List/Shared/Filesystem/`,
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/List/Shared/Selection/`,
  and
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/List/Shared/Topology/`.
- Adapted Route List Integration paths are
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/List/Shared/Filesystem/`,
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/List/Shared/Loader/`,
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/List/Shared/Selection/`,
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/List/Shared/Topology/`,
  and
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/List/RouteListApplicationIntegrationTests.cs`.
- Adapted Route Inspect Unit paths are
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Inspect/` and
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Inspect/Shared/Resolution/`,
  including its profile and result-builder evidence.
- Adapted Route Inspect Integration paths are
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Inspect/Resolution/`,
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Inspect/Profile/`,
  and
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Inspect/RouteInspectApplicationIntegrationTests.cs`.

Red covers duplicate and overlapping roots, a no-body-read catalogue probe,
aliases with logical-path rebinding, identity collisions, adjacent and orphan
overwrites plus Inspect's legacy ambiguity, missing/unreadable/malformed/unsafe
Loader cases, partial safe Loader roots, duplicate canonical Loader roots,
complete and filtered allowlists, excluded support, post-catalogue physical
identity changes, Inspect ambiguous-base metadata with no projected overwrite, a
one-ID-candidate non-adjacent Inspect orphan, cancellation, deterministic
ordering, profile-body reuse, and no writes. Unit and Integration projects remain
independently selectable, and each real-OS fixture owns its mutable workspace.

The independently runnable projects are
`src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj`
and
`src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj`.
New neutral tests use durable traits `Feature=source-catalogue` with
`Evidence=Unit` or `Evidence=Integration`; adapted Route tests retain
`Feature=route-list` or `Feature=route-inspect`. After a Release build, the focused
inner-loop commands run from `src/cli/`:

```bash
dotnet test --project tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~Framework.Sources|FullyQualifiedName~Commands.Route.Shared|FullyQualifiedName~Commands.Route.List|FullyQualifiedName~Commands.Route.Inspect"
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~Framework.Sources|FullyQualifiedName~Commands.Route.List|FullyQualifiedName~Commands.Route.Inspect"
```

Red records the total, passing, and intentional failing cases for each command.
Every intentional failure must reach a named Gray `NotSupportedException`
entrypoint; existing Route regressions remain green.

Green implements the frozen Red evidence, migrates Route consumers, deletes
obsolete duplicate authorities and temporary Gray coexistence, and preserves the
accepted tests. Blue and Purple occur only through separately reviewed bounded
changes. The one final Child 1 gate runs after the last Child 1 change and
requires a warning-free build, format and diff checks, complete Unit and
Integration suites, affected managed EndToEnd Route regressions when source
wiring reaches them, the local `win-x64` Native AOT root with affected Route List
and Route Inspect Integration/EndToEnd regressions, and no-write,
source-locality, and dependency audits. This gate does not run or claim a Find
command, six-RID parity, or Find-specific Native AOT acceptance.

The Gray and every later phase first run these exact coherence commands from
`src/cli/`:

```bash
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore --nologo
dotnet format OpenForge.Cli.slnx --no-restore --verify-no-changes
```

The final Child 1 gate reruns both complete managed test projects, publishes the
managed and Native AOT `win-x64` root, runs the complete EndToEnd project against
each root, publishes and runs the Native AOT Integration and EndToEnd projects,
and runs the package vulnerability command already recorded by the Find parent.
Its public no-write scenarios are `route list --json` and
`route inspect <source> --json`
against one owned workspace containing a Loader root, an ID collision, and a
paired overwrite. Exact executable paths, counts, zero skips, workspace snapshots,
and the one-RID limitation are recorded at the gate.

After the coherence commands, the exact final commands from `src/cli/` are:

```bash
dotnet test --project tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-progress
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-progress
dotnet publish root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --no-restore --runtime win-x64 -p:PublishAot=false
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/OpenForge.Cli/release_win-x64/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" dotnet test --project tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-build --no-progress
dotnet publish root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --runtime win-x64 --no-restore --output artifacts/publish/win-x64/open-forge
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/win-x64/open-forge/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" dotnet test --project tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-build --no-progress
dotnet publish tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --runtime win-x64 --no-restore --output artifacts/publish/win-x64/integration
"./artifacts/publish/win-x64/integration/OpenForge.Cli.IntegrationTests.exe" --progress off
dotnet publish tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --runtime win-x64 --no-restore --output artifacts/publish/win-x64/end-to-end
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/win-x64/open-forge/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" "./artifacts/publish/win-x64/end-to-end/OpenForge.Cli.EndToEndTests.exe" --progress off
dotnet package list --project OpenForge.Cli.slnx --vulnerable --include-transitive --format json --no-restore
```

`git diff --check`, exact changed-path review, `git diff --quiet 063c59d --
src/cli` before Gray, and the no-write/source-locality/dependency audits run from
the repository root or their recorded owning path.

### I. Stop conditions and resolved gaps

The explorations' earlier gaps are closed by this packet: root/source path
semantics, candidate-aware filtered selection, issue ordering, memoized path
rebinding and physical revalidation, Loader parsers and stages, topology admission
and unavailable support, missing Loader, aliases, overwrite divergence, the
metadata/profile projector, graph mapping, phase coexistence, and exact evidence
projects and commands are all decided above.

Stop and return to the parent if implementation contradicts any current Route
behavior or the accepted decisions above. Do not alter tests or public Route
behavior to fit an implementation. Also stop if managed containment or physical
identity cannot prove the boundary on the supported target, if a command policy
enters Framework, or if a third source inventory or forwarding type becomes
necessary. Stop before body reads in catalogue formation, Find matching,
rendering, Markdown/YAML parsing, or any dependency, project, package, Shell,
root-host, or unrelated command change.

## Execution And Evidence

Use the strict child cycle: Gray freezes production call and fact contracts
without behavior; Red completes affected Route regressions and new real-OS
neutral evidence; Green implements the boundary; optional bounded Blue or Purple
passes occur only when separately reviewed. One final child gate follows.

Evidence covers canonical paths, forms, IDs, references, all eligible Markdown
including unrouted, Skill, Loader, and Template sources, root outcomes,
aliases/links, escape and cycle blocking, identity collisions, overwrite pairs
and orphans, directory outcomes, cancellation retention, strict reads only after
an explicit layer is selected, routed/unrouted/ambiguous/unavailable route facts,
on-demand route-supporting reads, deterministic repeat reads, no writes, and the
full focused Route List and Route Inspect regressions.

## Historical Progress And Completion

- Initial packet result: The detailed neutral catalogue, selected-layer reader,
  neutral route-facts, Route projection, migration, Gray/Red/Green, and evidence
  packet is reviewed and recorded at `7e081ef`. Its `src/cli/` tree matched
  `063c59d` at that boundary. Gray production contracts are committed at `7b34cc7`.
  Production-free Red now completes the affected evidence against that frozen
  production surface.
- Exploration result: Read-only explorers and grounded advisors inspected the
  current Route List, Route Inspect, Framework filesystem, accepted filters, and
  affected evidence from exact `b2e3106`. They made no edits. Their findings are
  recorded in the closed decisions and test paths above.
- Initial review result: A targeted correctness review returned changes required
  because overwrite compatibility, missing Loader mapping, `.agents` root
  validity, topology admission, filtered issue selection, issue ordering, Loader
  duplicate handling, and exact evidence commands were incomplete. An independent
  adversarial architecture lens also found physical revalidation, profile reads,
  allowlist support, and Gray/Red coexistence gaps. The corrections above adopt
  the strongest viable alternative: keep neutral intrinsic facts exact, preserve
  divergent Inspect meaning in explicit Route-local projection, and keep command
  findings/status local. This adds adapter surface but prevents public Route drift
  and a later Find filtering dead end.
- Final review result: The fresh correctness review passed after corrections for
  Native AOT EndToEnd evidence, scope-derived issue membership, route-issue
  ordering, Inspect's early exact-path terminal behavior, typed projection
  association, exact test disposition, Route metadata/overwrite invariants, and
  the non-adjacent overwrite edge. Its decisive evidence is that only a neutral
  adjacent non-colliding pair becomes a paired Route source; Inspect collision
  ambiguity uses `BaseOnly`, and every other overwrite remains a source-less
  orphan fact. The strongest counterargument is the legacy ID-based pairing path;
  retaining only its multiple-candidate ambiguity preserves compatibility without
  violating current Route model invariants. Reopen Preflight if implementation
  would pair a non-adjacent overwrite or expose an orphan/ambiguous overwrite
  through `FindByPath`.
- Gray result: From exact clean predecessor `7e081ef`, 39 new production files add
  the neutral Identity, Inventory, Reading, and Routing models/callables plus the
  Route projection and List/Inspect adapter contracts. Every behavior entrypoint
  throws `NotSupportedException`. Existing Route authorities and runtime wiring
  remain unchanged; no tests, project, package, generated, Shell, root, Find
  query, or presentation file changed. The Release solution build passes with
  zero warnings/errors, `dotnet format --verify-no-changes` passes, and
  `git diff --check` passes. No tests ran, as required for Gray.
- Gray correctness review: Initial review found incomplete projection-set
  overwrite validation, reference-set equality, and source-less read association.
  Constructor validation now matches the existing Route catalogue boundary, and
  fresh review passes. The strongest counterargument was to rely on builders
  alone; constructor validation was retained because these models are directly
  callable and must not represent invalid Route facts. Focused Red evidence must
  still execute these invariants.
- Gray improvement review: The only material recommendation was to route repeated
  model path-shape checks through `SourceLogicalPath`. That change is deferred to
  Green because the accepted Gray identity callables intentionally throw and
  using them now would replace structural argument validation with Gray behavior.
  Reconsider the local duplication after identity behavior exists; no other local
  improvement justified changing Gray.
- Dogfooding note: One adversarial review role failed to start because its runtime
  role configuration was unavailable. It produced no artifact or conclusion. A
  separate grounded advisor supplied the independent lens without inheriting the
  correctness review's result.
- Gray dogfooding note: The bounded Gray implementer reached its execution limit
  after creating the compile-only files and running build/format checks but before
  its final audit. The Mastermind inspected every untracked file, corrected model
  associations, reran all Gray checks, and completed the independent reviews.
- Red result: The packet moves neutral identity, Loader, topology, and model
  evidence to `Framework/Sources/` and Route source projections, adapts the
  affected Route List evidence, and adds owned real-filesystem Inventory,
  Reading, Routing, Route List projection, and Route Inspect projection cases.
  A fresh Release build passes with zero warnings/errors. Format verification and
  `git diff --check` pass, and every protected production, project, package, root,
  and generated surface remains unchanged from `7b34cc7`.
- Red focused evidence: Unit is `562` total with `403` passing and `159`
  intentional Gray failures. Integration is `183` total with `155` passing and
  `28` intentional Gray failures. Both projects report zero skips. Every new
  failure reaches a named Gray `NotSupportedException` entrypoint; the one future
  invalid-argument assertion retains that Gray exception as its xUnit inner
  cause. Existing Route regressions pass separately: Route List Unit `116/116`,
  Route Inspect Unit `101/101`, Route List Integration `87/87`, and Route Inspect
  Integration `68/68` after excluding only the new projection-builder cases.
- Red correctness review: Initial bounded review required corrections for
  selected orphan candidates, physical alias identities, an existing-file
  mutation, valid Loader setup, exact unsafe-path assertions, compatibility path
  coverage, and BaseOnly/non-adjacent Inspect evidence. Fresh review passes with
  the two recorded limitations below. The decisive evidence is that fixtures now
  carry the exact physical and selected identities the Green behavior must
  consume. The strongest counterargument is that Gray throws before later
  assertions execute; direct setup inspection, existing predecessor evidence,
  and exact fixture assertions reduce that risk without introducing a fake
  filesystem or scheduling seam. Green must reopen Red if a corrected fixture
  cannot reach its asserted state.
- Red improvement review: Fresh bounded review finds no required change. It
  defers only optional Purple consolidation of duplicated topology and projection
  selection fixture construction. Keeping the current local setup costs some
  duplication but preserves failure locality during Green; repeat consumers may
  justify consolidation in Purple.
- Recorded Red limitations: Physical-resolution
  `SourceLayerVerificationState.Unavailable` is covered by exact model shape with
  a typed `FilesystemFailure`; real locked-file evidence separately proves a
  verified identity followed by body-read `AccessDenied`. The frozen real reader
  has no safe portable fixture for physical-resolution unavailability. Catalogue
  pre-cancellation and no access are directly proved, while accumulated neutral
  mid-traversal cancellation is not triggered because a deterministic test would
  require a timing race, injection seam, or implementation scheduling artifact.
  Existing accepted Route inventory evidence proves accumulated cancellation for
  the predecessor. Green and final review must retain these limitations and must
  not claim direct neutral mid-traversal evidence.
- Red dogfooding note: Bounded Red helpers reached execution limits before a
  complete trustworthy packet. The Mastermind inspected and corrected the actual
  test tree, ran the focused evidence, and completed fresh correctness and local
  improvement reviews.
- Green correction trigger: Green implemented the neutral Framework and Route
  projection behavior and made the initial focused Red packet pass, but runtime
  migration proved that active Route policy evidence still compiled against the
  obsolete Route catalogue, identity, Loader, and topology authorities. Those
  tests could not move to neutral inputs because the initial Gray contract did not
  expose final neutral call surfaces on the command-local inventory, selection,
  topology, and Inspect graph boundaries. Continuing Green would either change
  frozen tests, leave duplicate authorities, or add wrappers. The correction
  therefore returns to Gray, which is the earliest invalidated phase.
- Correction decision: Use the workflow's one exceptional correction cycle in
  the order corrected Gray, corrected Red, and corrected Green. The strongest
  alternative was a combined production-and-test migration, which would avoid
  temporary call-surface coexistence but would break the frozen-evidence boundary.
  A second alternative was leaving obsolete authorities until Purple, which
  conflicts with Green's migration boundary and Purple's protected production
  surface. Corrected Gray keeps old behavior only for unchanged runtime wiring and
  adds no forwarding behavior. This conclusion changes only if a required neutral
  seam introduces command policy into Framework, overload ambiguity, or dual
  runtime wiring; any such finding returns to the parent because no third cycle is
  available.
- Corrected Gray result: Five production files add compile-only final-neutral
  seams. `RouteListInventoryReader` accepts the invocation-scoped
  `SourceDocumentReader`; `RouteListInventoryFacts` exposes neutral catalogue and
  projection results through neutral factories; `RouteListSelectionResolver`
  accepts the neutral catalogue, Route projection set, and route facts;
  `RouteListTopologySelector.SelectSources` accepts route facts; and
  `RouteInspectGraph` accepts a Route projection set and route facts. Every new
  entrypoint throws a named `NotSupportedException`. Existing runtime composition
  and legacy behavior remain unchanged. Release build and format pass with zero
  warnings/errors, and `git diff --check` passes. No tests ran, as required for
  Gray.
- Corrected Gray reviews: Initial correctness review found one overload ambiguity
  for legacy null-validation calls. Naming the neutral topology method
  `SelectSources` removes that ambiguity, and fresh correctness review passes.
  Local improvement review finds no material reduction: the transitional arity
  keeps catalogue, projection, route-fact, and command facts distinct without a
  temporary context bag. Green must remove every legacy overload, property, and
  obsolete authority after corrected Red binds the final seams.
- Corrected Red result: Active reference and Loader parser regressions target the
  neutral Framework authorities. Route List inventory retains the
  neutral catalogue and projection result with one invocation reader. Route List
  Loader, selection, and topology evidence uses the real neutral catalogue,
  projection, and route-facts chain plus the corrected command seams. Route
  Inspect graph and resolution evidence uses the projection set and neutral route
  facts. One direct Route List pre-cancellation case raises Integration from `183`
  to `184` cases. At corrected Red review, the executable-reference audit was
  recorded as clear except for the historical `RouteSourceCatalogueModelTests`
  class name, which already tests `RouteSourceProjectionSet`. Corrected Green's
  deletion audit later disproved that broad conclusion for the Route projection
  form, Route List path wrapper, and one-argument inventory entrypoint; the return
  record below supersedes that audit claim.
- Corrected Red focused evidence: Release build passes with zero warnings/errors,
  format and `git diff --check` pass, and production/project/root surfaces remain
  unchanged from `6a9b143`. Unit is `562` total with `251` passing and `311`
  intentional Gray failures. Integration is `184` total with `79` passing and
  `105` intentional Gray failures. Both projects report zero skips. Failures reach
  named neutral or corrected-Gray `NotSupportedException` entrypoints, including
  upstream real catalogue formation, corrected inventory/selection/topology
  seams, and corrected Inspect graph construction/properties.
- Corrected Red reviews: Initial review questioned empty Loader completeness,
  duplicate-entrypoint policy, projection ownership, and neutral Loader traits.
  Empty Loader roots now remain complete; predecessor Route List evidence confirms
  duplicate same-folder entrypoints retain command-local `RouteAmbiguous` policy;
  every selected Route source is reference-identical to its projection-set member;
  and every moved neutral parser case uses `Feature=source-catalogue`. Fresh
  correctness review passes. The strongest counterargument is that upstream Gray
  failures mask downstream fixture execution; direct fixture inspection, exact
  identity assertions, and the predecessor policy evidence keep the expected
  behavior explicit. Any Green mismatch returns to corrected Red, and no third
  complete cycle is available.
- Corrected Red improvement review: No required change remains. It defers only
  Purple consolidation of the large Inspect graph, inventory, and topology test
  fixture builders. Keeping them local during Green costs duplication but avoids
  hiding boundary-specific invalid states.

### Corrected Green return to parent

- Green implementation now passes a warning-free Release build. The last complete
  focused Unit run is `562/562`. The last complete focused Integration run was
  `181/184`; its two neutral Loader external-boundary mismatches were corrected and
  their exact targeted rerun passes `2/2`. The remaining case is the predecessor
  Route List mid-enumeration cancellation assertion that requires the returned
  `ValueTask` to remain incomplete while a real 32 MiB read is active. A complete
  Integration rerun after the Loader correction is not claimed.
- Active Route List and Route Inspect composition now use a shared invocation-
  scoped `SourceDocumentReader`, neutral catalogue/projection/route facts, and the
  corrected selection, topology, and Inspect graph seams. Inspect profile/result
  consumers use neutral graph boundaries. This production is unaccepted WIP and
  remains subject to deletion, audit, review, and full evidence.
- Final deletion exposed a contradiction in corrected Red. Active frozen tests
  still compile against `RouteSourceForm` throughout Route projection fixtures,
  against `RouteListLogicalPath` in topology fixtures, and against the one-argument
  `RouteListInventoryReader.ReadAsync` entrypoint. The accepted production contract
  requires `RouteSourceDocument` to consume neutral `SourceDocumentForm`, requires
  Route-prefixed duplicate identity/form authorities and every legacy overload to
  be removed, forbids forwarding wrappers, and freezes Green tests. Removing those
  production surfaces therefore makes the frozen test projects fail to compile;
  retaining them violates the accepted Green boundary.
- The attempted production-only form migration was reverted immediately; the
  worktree again builds with zero warnings and errors. Tests were not changed.
  Because the one exceptional correction cycle is already consumed and no third
  cycle is available, Child 1 has returned to the parent rather than choosing a
  compatibility exception or silently changing corrected Red.
- Parent disposition: The maintainer's continuation direction authorizes the
  bounded combined correction recorded in the Find parent. Compatibility APIs are
  not retained. The correction may mechanically migrate `RouteSourceForm` and
  direct consumers/fixtures to neutral `SourceDocumentForm`, replace active-test
  `RouteListLogicalPath` use with `SourceLogicalPath`, and move the three inventory
  boundary calls to the final reader seam. It may disposition the timing-dependent
  predecessor cancellation assertion only within the already accepted neutral
  evidence limitation. It changes no source behavior or public Route expectation.
### Historical Parent Refinement Result

- The combined correction changes exactly 27 production and active-test paths.
  `RouteSourceDocument.Form` and its constructor now use neutral
  `SourceDocumentForm`; the Route form enum and classifier are deleted; direct
  retained and temporary List/Inspect consumers use `SourceFormClassifier`.
  `SourceLogicalPath` and `SourceFormClassifier` receive their behavior now because
  the mechanically migrated Route constructors require those neutral callables to
  preserve accepted existing behavior before the remaining Green is restored.
- Eight Unit files and three Integration files now use the final neutral
  form and path types without conversion helpers. Each of the two retained real-
  workspace Route List inventory boundary cases supplies one invocation-scoped
  `SourceDocumentReader` bound to its workspace. The old
  `ValueTask.IsCompleted`/32 MiB timing case is removed rather than replaced by a
  scheduling seam; the already recorded neutral mid-traversal evidence limitation
  remains unchanged.
- Release build passes with zero warnings and errors. Format and `git diff --check`
  pass. Focused Unit is `562` total, `295` passing, `267` intentional Gray failures,
  and zero skips. Focused Integration is `183` total, `76` passing, `107`
  intentional Gray failures, and zero skips. Scripted inspection of the complete
  logs finds zero failed blocks without a named Gray `NotSupportedException`.
- Exact audits find no `RouteSourceForm` or `RouteSourceFormClassifier` under
  `src/cli/` and no active-test `RouteListLogicalPath`. Fresh correctness review
  passes after complete failure attribution. Local-improvement review finds no
  material improvement; deterministic cancellation synchronization remains a
  future evidence option, not part of this correction.
- Historical boundary: The correction commit became corrected Green's new frozen
  predecessor. Any later contract contradiction returned to the Find parent.
- Historical next action: Commit the parent refinement, restore the isolated
  corrected Green worktree, resolve only mechanical overlaps with the type
  migration, and complete legacy-authority deletion.

## Current Progress And Next Action

Child 1 remains Complete. The neutral catalogue and selected-layer read boundary
are accepted, both Route consumers use them without public drift, and the focused,
complete, Native AOT, no-write, package, and audit evidence passes. Child 2 is
Complete and accepted at exact `ff7ce3f`. Child 3 and the Find parent are Complete
and accepted in the commit containing this record update; no-op Purple is recorded
at exact `426d4f5`. The final managed/native gate and package, artifact, static,
public no-write, and protected-surface audits passed.

The sole immediate continuation is to freshly verify `develop`, squash-integrate the
accepted `feature/cli-find` tip into local `develop`, commit that one squash, prove
exact tree equality, do not push, and halt. References remains Planned and must not
start in this session. `CLI-EDGE-001` remains non-product only.
