---
open-forge:
  description: Task 62 command slices for authoring, Doctor validation, Context, Find, and Route Inspect
  tags: [Memory, Working, Task, Plan, CLI, Contextual]
---

# Task 62 command plan

## Goal

Let callers author file conditions and obtain deterministic context, discovery,
and inspection for a supplied set of working files.

## Authority and prerequisites

Read [Execution](execution.md), the [foundation plan](foundation-plan.md), and
the selected command's current Interface and Behavior contracts under
`.agents/memory/crystallized/documents/cli/contracts/`. W4 in the
[documentation plan](documentation-plan.md) updates those contracts in parallel.
The accepted behavior in Execution controls the change. Report a material
contract discrepancy to root rather than making another command policy.

Every worker independently reads the complete current
`.agents/directives/csharp/_csharp.md`, `design.md`, and `style.md`, and loads
the applicable CLI and testing routes. Workers have separate branches/worktrees,
do not revert other edits, and do not modify another slice's files.

| Prefix | Exact repository-relative path |
| --- | --- |
| O | `src/cli/operations/OpenForge.Cli.Operations/Commands` |
| R | `src/cli/rendering/OpenForge.Cli.Rendering/Presentation` |
| F | `src/cli/framework/OpenForge.Cli.Framework/Framework` |
| U | `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands` |
| I | `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands` |
| E | `src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests` |

Paths expand literally. `:1` selects the complete existing file or a new file's
first line. A leaf owns its directly required models, composition, rendering,
help and mirrored tests inside the declared leaf boundaries, not sibling leaves.
Root records any additional directly required integration-neighborhood file.

## Common preflight and evidence

- [ ] `git status --short` is explained and root has recorded base and worktree.
- [ ] F1–F4 files needed by the slice were copied by root, with exact identities.
- [ ] The dependent Framework build passes before local editing.
- [ ] The worker reads the actual owned production and test sources before editing.

Use the following managed commands from the worktree. Each slice supplies its
literal class filter below. Do not rerun the complete native gate per leaf.

```powershell
dotnet build src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --no-restore
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-restore -- --filter-class '*RouteCreate*'
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-restore -- --filter-class '*RouteCreate*'
dotnet run --project src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-restore -- --filter-class '*PublishedRouteCreate*'
```

Replace only the quoted filter with the literal selection named by the slice.
New tests use the same command prefix in their class names so these selections
include them. If the prepared runner rejects an option or a dependency is missing,
return the exact diagnostic to root. Do not fetch dependencies or improvise a
different evidence claim. Root runs complete managed and supported host Native
AOT gates after the final integrated executable change.

## Shared observable contract

All authoring leaves use repeated `--apply-to <pattern>` and F1 parsing. They
reject the whole request before effects if any supplied pattern is invalid.
The parser library owns repeated occurrences and native option forms. New fields
are scoped quoted lists. No comma splitting is added.

Context, Find, and Inspect use repeated `--for <path>` and F4 normalization.
They accept planned paths without existence checks. The selected workspace is
the base. Outside-workspace/root-only inputs take each command's existing invalid
request path, before reading selected source content or creating state.

Each retrieval command owns an optional wire projection with this shape at
standard/full detail, and therefore debug's full primary result:

```json
{
  "applicability": {
    "state": "matched",
    "conditions": [
      { "source": ".agents/directives/csharp/_csharp.md", "patterns": ["**/*.cs"] }
    ],
    "matchingPaths": ["src/A.cs"]
  }
}
```

Use the frozen states `unconditioned`, `matched`, `unmatched`, `pending`, and
`invalid`. Source provenance uses the command's canonical source path convention.
Do not serialize the shared Framework facts directly. Preserve omission for
legacy unconditioned sources when no `--for` was supplied. Keep source-generated
JSON graphs complete, existing detail monotonicity, and exact authored content.
Do not silently bump or reinterpret a schema version. Root resolves any concrete
schema-contract conflict before acceptance.

## A1: Route Create

Depends on: F1, F2, F3. Blocks: authoring milestone.

Own `O/Route/Create`, `R/Route/Create`, `U/Route/Create`, `I/Route/Create`,
`E/PublishedRouteCreateProcessTests.cs:1`, and
`E/PublishedRouteCreateWorkspace.cs:1`. Principal edit sites:

- `O/Route/Create/RouteCreateDefinitions.cs:1`
- `O/Route/Create/Models/Binding/RouteCreateSymbols.cs:1`
- `O/Route/Create/Shared/Binding/RouteCreateRequestBinder.cs:1`
- `O/Route/Create/Models/Request/RouteCreateRequest.cs:1`
- `O/Route/Create/Shared/Planning/RouteCreateTargetPlanner.cs:1`
- `O/Route/Create/Shared/Planning/RouteCreateGeneratedNavigationPlanner.cs:1`

1. Add the repeated option to definitions, symbols, binding validation and the
   immutable request. Parse once through F1 and carry typed patterns to planning.
   Test repeated values, bad patterns and normal native option spellings with
   `'*RouteCreate*'` Unit tests.
2. Pass patterns to F2's emitter at the existing new-file composition site.
   Keep existing create target, overwrite, approval and expected-state behavior.
   Prove created frontmatter and the F3-generated parent row in Integration tests.
3. Update leaf help and result/presentation only where the authored field is
   reported. Add a public process scenario for repeated patterns, emitted quoted
   list, generated suffix, and invalid-input unchanged bytes. Run the three A1
   filters from the common commands.

Acceptance: absent option preserves current output and effects. Invalid input
does not create a file or alter a parent's Entries.

## A2: Route Init

Depends on: F1, F2, F3. Blocks: authoring milestone.

Own `O/Route/Init`, `R/Route/Init`, `U/Route/Init`, `I/Route/Init`,
`E/PublishedRouteInitProcessTests.cs:1`, and `E/PublishedRouteInitWorkspace.cs:1`.
Principal edit sites:

- `O/Route/Init/RouteInitDefinitions.cs:1`
- `O/Route/Init/RouteInitDefinitions.Values.cs:1`
- `O/Route/Init/RouteInitBinding.cs:1`
- `O/Route/Init/Models/Binding/RouteInitSymbols.cs:1`
- `O/Route/Init/Models/Request/RouteInitRequest.cs:1`
- `O/Route/Init/Shared/Planning/RouteInitMetadataResolver.cs:1`
- `O/Route/Init/Shared/Planning/RouteInitScaffoldComposer.cs:1`

1. Add the same typed repeated option and request field as A1, consuming F1.
   Include it in the existing metadata-option restrictions. Preserve restrictions
   for `--framework` and an existing final target. Unit tests must reject both
   forbidden combinations before planning.
2. Emit the optional scoped list on the intended newly authored target and use
   F3 for generated navigation. Preserve scaffold ordering and route ownership.
   Integration tests prove the target metadata and parent row.
3. Update leaf help and add a published CLI case. Run Unit/Integration filters
   `'*RouteInit*'` and EndToEnd `'*PublishedRouteInit*'`, including existing
   scaffold snapshots. Do not refresh unrelated payload snapshots to hide drift.

Acceptance: the option does not widen which existing files Init may rewrite.

## A3: Route Update

Depends on: F1, F2, F3. Blocks: authoring milestone.

Own `O/Route/Update`, `R/Route/Update`, `U/Route/Update`, `I/Route/Update`,
`E/PublishedRouteUpdateProcessTests.cs:1`, and
`E/PublishedRouteUpdateWorkspace.cs:1`. Principal edit sites:

- `O/Route/Update/RouteUpdateDefinitions.cs:1`
- `O/Route/Update/RouteUpdateBinding.Input.cs:1`
- `O/Route/Update/Shared/Binding/RouteUpdateBindingValidator.cs:1`
- `O/Route/Update/Models/Request/RouteUpdateRequest.cs:1`
- `O/Route/Update/Models/Planning/RouteUpdateMetadataSyntax.cs:1`
- `O/Route/Update/Models/Planning/RouteUpdateMetadataEdits.cs:1`
- `O/Route/Update/Shared/Planning/RouteUpdateMetadataLayoutReader.cs:8`
- `O/Route/Update/Shared/Planning/RouteUpdateMetadataEditPlanner.cs:1`
- `O/Route/Update/Shared/Planning/RouteUpdateMetadataByteEditor.cs:1`
- `O/Route/Update/Shared/Planning/RouteUpdateMetadataBoundaryProjector.cs:1`
- `O/Route/Update/Shared/Planning/RouteUpdateMetadataPatcher.cs:5`

1. Add repeated `--apply-to` and boolean `--clear-apply-to`, make them mutually
   exclusive, and include both in the existing “has requested changes” decision.
   Parse patterns through F1. Test each option, the conflict, and a no-op clear.
2. Extend the existing parsed layout and exact byte-span edit mechanism using
   F2 declarations. Replace root in place, replace scoped in place, and replace
   both equivalent declarations together. When no declaration exists, add the
   scoped quoted list. Clear removes both declared fields while preserving
   unrelated metadata and body bytes. Keep malformed/conflicting declarations
   on the existing invalid-metadata boundary.
3. Retain the existing preservation-unsafe failure when parsed locations cannot
   safely support the requested edit. Do not serialize the whole frontmatter,
   use regex YAML edits, or delete an unrelated mapping/comment. Prove BOM,
   newline style, comments, unrelated fields and body bytes remain exact in
   `RouteUpdateMetadataPreservationTests` and `RouteUpdateMetadataPatcherTests`.
4. Feed updated facts to existing navigation planning so new/cleared suffixes
   are reflected. Update help and published tests. Run Unit/Integration
   `'*RouteUpdate*'` and EndToEnd `'*PublishedRouteUpdate*'`.

Acceptance: both equivalent locations stay equivalent, clearing leaves no
residual declaration, rejected edits change zero bytes, and existing locking,
revalidation, recovery and plan approval remain intact.

## V1: Doctor validates the normal source universe

Depends on: F2, F3. Blocks: final qualification.

Own `O/Doctor/Shared/Domains/WorkspaceSourceDoctorInspector.cs:9` and its focused
Doctor tests in `U/Doctor`, `I/Doctor`, and `E/PublishedDoctorProcessTests.cs:1`.
Use the existing finding model and rendering. No new public option is needed.

1. Check independent `ApplyTo` state for every source normally examined, including
   native Skills and sources whose required ordinary metadata is missing.
   Emit existing `WorkspaceFrontmatterMalformed` for invalid optional metadata.
   Avoid duplicate reports for the same malformed document cause.
2. Test malformed pattern, empty value, conflicting dual declarations, valid
   root-only declaration, and malformed Skill declaration. A valid pattern that
   matches no current disk file is not a defect. Tests must show a malformed
   unrelated-language source remains reported.
3. Run Unit/Integration `'*Doctor*'` and the published Doctor selection. Compare
   baseline finding counts only within owned fixtures, not the repository's
   pre-existing reference-alias defects.

Acceptance: no task file filter, no filesystem glob enumeration, and no required
Status schema or behavior change.

## C1: Context selection and pending conditions

Depends on: F1–F4. Blocks: retrieval milestone and architecture checkpoint.

Own `O/Context`, `R/Context`, `U/Context`, `I/Context`,
`E/PublishedContextProcessTests.cs:1`, `E/PublishedContextWorkspace.cs:1`, and
these explicitly assigned shared startup sites:

- `F/Sources/Loading/SourceLoadingClosureResolver.cs:10`
- `F/Sources/Models/Loading/SourceLoadingClosureModels.cs:1`
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Sources/Loading/SourceLoadingClosureResolverTests.cs:1`

Principal Context sites:

- `O/Context/ContextDefinitions.cs:1`
- `O/Context/Shared/Binding/ContextBindingInputReader.cs:1`
- `O/Context/Shared/Binding/ContextRequestParser.cs:1`
- `O/Context/Models/Request/ContextRequest.cs:1`
- `O/Context/Shared/Selection/ContextClosureResolver.cs:1`
- `O/Context/Shared/Selection/ContextLoadingClosureResolver.cs:18`
- `O/Context/Shared/Links/ContextLinkExpander.cs:1`
- `O/Context/Shared/Links/ContextLinkedSourceReader.cs:1`
- `O/Context/Shared/Result/ContextResultBuilder.cs:1`
- `O/Context/Models/Result/ContextSourceResultModels.cs:1`
- `O/Context/Models/Result/ContextCoverageResultModels.cs:1`
- `R/Context/Models/ContextData.cs:1`
- `R/Context/Shared/Rendering/ContextDataJsonConverter.cs:1`
- `R/Context/Shared/Rendering/ContextDataJsonContext.cs:1`

1. Bind repeated paths and normalize once using F4. Keep `--additions-only`
   dependent on explicit source operands. Test `--for` alone does not satisfy
   that requirement. Unit request tests also cover planned paths and rejection.
2. Add an opt-in applicability mode to shared startup loading, default off.
   Context opts in even without working paths so conditional routes become
   pending. Status retains the old default and requires no request or rendering
   edits. Preserve startup entry order and overwrite adjacency.
3. Apply F4 to the startup traversal and the separate explicit-source traversal.
   A visible matching conditioned entry activates before work regardless of an
   absent loading tag. `LoadNow` and `KeepInMind` retain their normal meaning
   behind the condition. Never scan hidden descendants to activate ancestors.
   Evaluate each full chain against the same normalized path set.
4. Explicit source operands establish their ancestor chain and may inspect an
   unmatched source. Link following may also inspect an unmatched source and
   must inspect necessary ancestor facts. Preserve the distinction between
   inspected and automatically active branches. Neither path activates automatic
   children of a nonmatching condition. Reuse F4, not a Context-local matcher.
5. Compute additions-only as filtered combined minus filtered startup, with the
   identical complete path set for both. Preserve canonical identity and existing
   source ordering. Test mixed conditions and explicit nonmatch together.
6. Project applicability in command-owned result/rendering models. Deferred
   conditions make the result incomplete with exit 3, a visible limitation, and
   `pendingConditions: [{source, patterns}]` retained at every detail level.
   Preserve exact selected content and existing output/error boundaries.
7. Add public scenarios for visible match, hidden ancestor, same-file AND,
   unknown paths, conflicting metadata, explicit/reference nonmatch, overwrite
   adjacency, additions-only and an entirely unconditioned workspace. Run
   `'*Context*'` Unit/Integration, `'*SourceLoadingClosureResolverTests*'` Unit,
   `'*Status*'` Unit/Integration and `'*PublishedContext*'` EndToEnd.

Acceptance: no hidden activation, no false complete result for pending/invalid
conditions, no Status regression, and no implicit addition of inspected Markdown
paths to the working set. Root inspects this first connected shared/public slice
before accepting repeated consumer patterns elsewhere.

## C2: Find filters without expanding inventory

Depends on: F1–F4. Blocks: retrieval milestone.

Own `O/Find`, `R/Find`, `U/Find`, `I/Find`,
`E/PublishedFindProcessTests.cs:1`, and `E/PublishedFindWorkspace.cs:1`.
Principal sites:

- `O/Find/FindDefinitions.cs:1`
- `O/Find/FindRequestBinder.cs:1`
- `O/Find/Models/Request/FindRequest.cs:1`
- `O/Find/FindOperationFactory.cs:19`
- `O/Find/Shared/Selection/FindUniverseResolver.cs:1`
- `O/Find/Shared/Matching/FindMatcher.cs:1`
- `O/Find/Shared/Matching/FindSourceMatcher.cs:1`
- `O/Find/Shared/Projection/FindProjectionSourceResolver.cs:1`
- `O/Find/Models/Result/FindMatch.cs:1`
- `R/Find/Models/FindData.cs:1`
- `R/Find/Shared/Rendering/FindDataJsonContext.cs:1`

1. Bind and normalize `--for` through F4. Keep existing tag/heading requirements
   for `--require` and `--within`. `--for` is not another tag/heading predicate.
   Test that those invalid combinations remain invalid.
2. Move the required route-facts read ahead of applicability filtering. Today
   route facts are read for post-match metadata projection. Resolve ancestor
   conditions before the new filter, including an unconditioned leaf under a
   conditioned parent. Keep include/exclude rules, candidate counts and source
   universe boundaries intact. Reading ancestor facts does not return ancestors.
3. Evaluate compatibility through F4. No effective condition is compatible.
   Apply the file filter independently of existing all/any tag and heading
   matching. Keep malformed-fact findings on the existing incomplete coverage
   path rather than silently returning a successful empty inventory.
4. Add the command-owned applicability projection and help. Test same-file AND,
   inherited-only conditions, planned paths, unchanged `--require` semantics,
   within boundaries and unconditioned legacy JSON. Run Unit/Integration
   `'*Find*'` and EndToEnd `'*PublishedFind*'`.

Acceptance: Find can discover candidates in its existing universe without
activating them, and Context-only helper namespaces are absent from Find.

## C3: Route Inspect explains applicability

Depends on: F1–F4. Blocks: retrieval milestone.

Own `O/Route/Inspect`, `R/Route/Inspect`, `U/Route/Inspect`, `I/Route/Inspect`,
`E/PublishedRouteInspectProcessTests.cs:1`, and
`E/PublishedRouteInspectWorkspace.cs:1`. Principal sites:

- `O/Route/Inspect/RouteInspectDefinitions.cs:1`
- `O/Route/Inspect/Models/Operation/RouteInspectRequest.cs:1`
- `O/Route/Inspect/Shared/Profile/RouteInspectChainReader.cs:1`
- `O/Route/Inspect/Shared/Profile/RouteInspectLoadingFactsBuilder.Traversal.cs:8`
- `O/Route/Inspect/Shared/Resolution/RouteInspectSourceFactsResolver.cs:1`
- `O/Route/Inspect/Models/Result/RouteInspectObservation.cs:1`
- `O/Route/Inspect/Shared/Result/RouteInspectResultBuilder.cs:1`
- `R/Route/Inspect/Models/RouteInspectData.cs:1`
- `R/Route/Inspect/Shared/Rendering/RouteInspectDataJsonContext.cs:1`

1. Bind/normalize paths with F4 and retain the explicit target even when it does
   not match. Extend the existing Inspect chain/facts traversal with source
   provenance, declared patterns and F4's effective evaluation.
2. Keep Inspect's traversal in its own leaf. Do not import Context's closure or
   link helpers. Explain unknown paths as pending, not unmatched. Inspection of
   a nonmatch does not activate that source's automatic children.
3. Project optional applicability through result, selector, JSON and text.
   Explain declared versus inherited conditions and matching supplied paths at
   standard/full detail. Preserve existing target, route, metadata and stale
   navigation diagnostics.
4. Test root and inherited declarations, same-file AND, no paths, explicit
   nonmatch, malformed metadata, and unconditioned legacy output. Run
   Unit/Integration `'*RouteInspect*'` and EndToEnd `'*PublishedRouteInspect*'`.

Acceptance: Inspect answers why supplied paths match using Framework facts while
retaining its existing read-only inspection scope.

## Integration acceptance

- [ ] Root integrates F before command outputs, then command-owned rendering/tests.
- [ ] No command duplicates YAML, Entries, pattern, condition, or concrete-path rules.
- [ ] Public help, text, JSON, exits and mutation bytes match the updated contracts.
- [ ] Index keeps complete navigation and Doctor reports malformed unrelated sources.
- [ ] The integrated build passes before full managed and Native AOT qualification.

## Divergences observed

None recorded. Return narrow changes to root for this shared plan.

## Rollback

Root retains the owned diff and removes only this slice's exact integrated
changes if abandoned. Workers never reset shared history or restore whole
directories. Preserve dependency copies and unrelated artifacts.
