---
open-forge:
  description: Sealed continuation state for Find Child 2 after Gray acceptance and during incomplete Red evidence authoring
  tags: [Memory, Archived, Contextual, Historical, Handoff, CLI, Find, Red]
---

# CLI Find Query Red Start Handoff

## Seal

Sealed on 2026-08-24 at the maintainer's requested agent-transfer boundary. Do
not edit this record. The mutable program state remains in the [CLI Development
Checkpoint](../checkpoints/cli-development.md), and the authoritative Child 2
phase record remains [Implement The Find Query
Operation](../cli-development/tasks/read-only/find-query-operation.md).

This is a contextual transfer snapshot, not product or architecture authority.
It records the exact worktree state at transfer because Red authoring is partial
and uncommitted. It supersedes chat summaries for resumption but does not replace
the linked contracts, Task, Plan, or repository rules.

## Wake-Up Order

Before editing, read these in order:

1. [`AGENTS.md`](../../../../AGENTS.md), then [`.agents/loader.md`](../../../loader.md).
   Use the Loader to select all applicable scopes; do not rely on this handoff as
   a substitute for loading current rules.
2. This sealed handoff and the mutable [CLI Development
   Checkpoint](../checkpoints/cli-development.md).
3. The authoritative [Find query operation
   Task](../cli-development/tasks/read-only/find-query-operation.md), especially
   `Allowed And Protected Surfaces`, `Focused Preflight Adoption`, `Frozen Phase
And Evidence Plan`, `Active Toolchain And Commands`, and `Stop Conditions`.
4. The [Find parent](../cli-development/tasks/read-only/find.md), [Read-Only
   group](../cli-development/tasks/read-only/_read-only.md), and [CLI Development
   Plan](../cli-development/plan.md) for sequence and parent boundaries.
5. Current product authority: [Find Interface](../../crystallized/documents/cli/contracts/find/interface.md),
   [Find Behavior](../../crystallized/documents/cli/contracts/find/behavior.md),
   [Find Technical Design](../../crystallized/documents/cli/contracts/find/technical-design.md),
   [Source-Universe Filters](../../crystallized/documents/cli/contracts/shared/source-universe-filters/behavior.md),
   and the [Shared CLI Operation Contract](../../crystallized/documents/cli/shared-operation-contract.md).
6. Applicable working rules: [CLI Implementation](../../../directives/open-forge/cli/implementation.md),
   [C# Design](../../../directives/open-forge/csharp/csharp-design.md), [Modern
   C#](../../../patterns/open-forge/csharp/modern-csharp.md), [Test Evidence
   Integrity](../../../directives/open-forge/testing/evidence-integrity.md), and
   [Evidence Tiers](../../../patterns/testing/evidence-tiers.md).

The Task and contracts control any disagreement with this snapshot.

## Git And Accepted Boundary

- Repository: `<workspace>\open-forge`.
- Branch: `feature/cli-find`.
- Current `HEAD`: exact `8cef8f7` (`Freeze Find query operation Gray contract`).
- Focused Preflight: exact `e24b9fe` (`Freeze Find query operation preflight`).
- Accepted Modern C# boundary: exact `a1cbf09`.
- Accepted Find source-catalogue predecessor: exact `96fe413`.
- `develop` remained `e77902a` during this work.
- Do not push. No branch change, merge, rebase, amend, or generated-index refresh
  is authorized.
- Tracked files are clean at transfer. Nine intended Red Unit files and this
  handoff are untracked. Nothing is staged.

The accepted Gray commit contains 54 paths: the Core Markdig reference, 43
authored compile-only C# files under the exact Find and neutral Markdown roots,
and aligned Working Memory. Locked restore, format, warning-free Release build,
informational `CA1062`/`CA1510`/`CA2264`, exact Markdig `1.3.2` resolution,
changed-path audits, correctness review, and improvement review passed before
commit. Gray production is now frozen for Red.

## Current Open Forge Flow

This child uses the repository's accepted phase flow:

1. **Preflight** froze contracts, exact callables, allowed paths, evidence, and
   commands. It is accepted at `e24b9fe`.
2. **Gray** added only compile-only production contracts. It is accepted at
   `8cef8f7`. Every domain behavior entrypoint still throws its named
   `NotSupportedException`.
3. **Red** is current. It may change only mirrored Unit and Integration tests,
   justified nearest-scope test fixtures, and the Task's same-commit Red progress.
   It must not change Gray production or expectations after acceptance.
4. **Green** starts only after Red is complete, reviewed, and committed. It makes
   the frozen evidence pass with the smallest production behavior change.
5. **Blue** is a production-only local structure pass after Green.
6. **Purple** is a test-only evidence-quality pass after Green/Blue.
7. **Acceptance** runs the complete managed and required public regression gate.
   Child 3, not Child 2 Red, owns public Find registration, renderers,
   `CliJsonContext`, EndToEnd Find tests, and integrated Markdig Native AOT proof.

Mastermind owns architecture, scope, phase acceptance, synthesis, and commits.
Use helpers only for closed assignments. For consequential completed changes,
send the exact changed paths to a fresh `reviewer` for correctness and an
`improvement-reviewer` for local maintainability. Do not ask either to perform a
repository-wide review. `purple-evidence-improver` belongs to Purple, not current
Red. `green-behavior-implementer` must not be used before Red acceptance.

No available Skill is required for this task. `customize-opencode` is unrelated,
and `experience-design` is not part of this non-presentation child.

## Frozen Gray Surface

The definitive signatures and meanings are in the Task's `Frozen Callables` and
`Neutral Markdown Boundary` tables. The important resumption boundary is:

- Detached syntax: `FindSymbols.Create()` owns `find` plus exactly seven options:
  `--include`, `--exclude`, `--tag`, `--heading`, `--require`, `--within`, and
  `--content`.
- Binding/result entrypoints: `FindRequestBinder.Bind(...)` and
  `FindWorkspaceResultFactory.Create(...)`.
- Pure stages: `FindQueryParser`, `FindUniverseResolver`, `FindBodyTagScanner`,
  `FindMatcher`, `FindProjectionBuilder`, and `FindResultBuilder`.
- Document stages: `MarkdownPipelineFactory`, `MarkdownDocumentParser`,
  `Utf8SourceMap`, `FindFrontmatterReader`, and `FindLayerInspector`.
- Orchestration: `FindOperation.ExecuteAsync(...)` and
  `FindOperationFactory.Create()`.
- `FindOperationComponents` contains exactly six boundary delegates plus one
  result builder. `FindSourceReadContext` contains exactly the catalogue,
  invocation-scoped `SourceDocumentReader`, and nullable default `.agents`
  selection scope.
- Markdig is centrally pinned to `[1.3.2]` and the Core project requests it
  without a local version.

Definitions, symbols, immutable models, and factory wiring may already satisfy
contract assertions. Behavior tests must call the intended surface and naturally
fail at the named Gray stub. Do not write tests that merely assert that
`NotSupportedException` is thrown. A setup, constructor, compilation, or unrelated
exception is not valid Red. A genuine callable contradiction returns to Gray
under the Task's one exceptional correction cycle; do not silently redesign the
contract in tests.

## Protected Surfaces

Do not modify during Red:

- `src/cli/core/**`, including every accepted Find and Markdown Gray path;
- root composition, public renderers, `CliJsonContext`, Shell/global parser
  semantics, Route behavior/output/JSON, or unrelated commands;
- projects, package versions, dependency graph, generated code/regions, EndToEnd
  tests, contracts, or crystallized documents;
- a second parser, source catalogue, Markdown/query engine, cache, persistent
  index, or fake filesystem.

Red Integration must use real owned temporary workspaces and real parser,
filesystem, YAML, Markdown, and selected-read boundaries. Direct operation tests
use `FindOperationFactory.Create()`; they do not register the root command.

## Frozen Red Matrix And Current Coverage

The Task's Red table is authoritative. It freezes exactly 50 declarations and
approximately 90–110 executable cases across independently runnable Unit and
Integration projects. Every test needs one readable `DisplayName`, `Feature`
equal to `find-query` or `markdown-documents`, and its project-tier `Evidence`
value. No EndToEnd evidence belongs to Red.

| Tier and class                                                        | Frozen declarations | Transfer state                                          |
| --------------------------------------------------------------------- | ------------------: | ------------------------------------------------------- |
| Unit `Commands/Find/FindBindingRedTests`                              |                   5 | Missing                                                 |
| Unit `Commands/Find/Shared/Query/FindQueryParserRedTests`             |                   4 | Missing                                                 |
| Unit `Framework/Documents/Markdown/MarkdownDocumentParserRedTests`    |                   5 | Authored, untracked, unverified                         |
| Unit `Commands/Find/Shared/Documents/Utf8SourceMapRedTests`           |                   2 | Authored, untracked, unverified                         |
| Unit `Commands/Find/Shared/Documents/FindFrontmatterReaderRedTests`   |                   4 | Authored, untracked, unverified                         |
| Unit `Commands/Find/Shared/Matching/FindBodyTagScannerRedTests`       |                   2 | Authored, untracked, unverified                         |
| Unit `Commands/Find/Shared/Selection/FindUniverseResolverRedTests`    |                   4 | Authored, untracked, does not compile yet               |
| Unit `Commands/Find/Shared/Matching/FindMatcherRedTests`              |                   5 | Authored, untracked, does not compile yet               |
| Unit `Commands/Find/Shared/Projection/FindProjectionBuilderRedTests`  |                   4 | Authored, untracked, does not compile yet               |
| Unit `Commands/Find/Shared/Result/FindResultBuilderRedTests`          |                   4 | Authored, untracked, does not compile yet               |
| Unit `Commands/Find/FindOperationRedTests`                            |                   4 | Authored, untracked, unverified behind compile blockers |
| Integration `Commands/Find/FindParserIntegrationRedTests`             |                   1 | Missing                                                 |
| Integration `Commands/Find/FindSourceUniverseIntegrationRedTests`     |                   2 | Missing                                                 |
| Integration `Commands/Find/FindDocumentInspectionIntegrationRedTests` |                   2 | Missing                                                 |
| Integration `Commands/Find/FindOperationIntegrationRedTests`          |                   2 | Missing                                                 |

At transfer, 34 of 50 declaration names were authored in nine Unit files; 16
declarations in two Unit and four Integration files remain missing. The document
packet helper reported 22 executable cases for its 13 declarations. The stage
packet helper reported 103 cases for its 21 declarations, but that count has not
been independently accepted and would exceed the complete Task target before the
missing declarations are added. Inspect and reduce over-parameterization while
preserving meaningful boundary coverage. Do not treat helper counts or summaries
as acceptance evidence.

## Exact Untracked Test Paths

Preserve and inspect these intended Red files; do not delete them as unrelated:

- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Documents/Markdown/MarkdownDocumentParserRedTests.cs`
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Find/Shared/Documents/Utf8SourceMapRedTests.cs`
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Find/Shared/Documents/FindFrontmatterReaderRedTests.cs`
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Find/Shared/Matching/FindBodyTagScannerRedTests.cs`
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Find/Shared/Selection/FindUniverseResolverRedTests.cs`
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Find/Shared/Matching/FindMatcherRedTests.cs`
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Find/Shared/Projection/FindProjectionBuilderRedTests.cs`
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Find/Shared/Result/FindResultBuilderRedTests.cs`
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Find/FindOperationRedTests.cs`

No binding/query Red file, Integration Find file, shared fixture, production file,
or Task progress edit is currently uncommitted.

## Verified Current Blocker

The following command was run from `src/cli/` at transfer:

```bash
dotnet build tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-restore --nologo
```

It failed with 14 `CS0051` errors and zero warnings. Public xUnit methods expose
internal production enum types in theory parameters. The affected declarations
are:

- `FindMatcherRedTests.MissingAndAmbiguousSectionsRetainOnlySafeEvidence`
- `FindMatcherRedTests.BareAndPredicateSearchesRetainOnlyIndependentlySafeMatches`
- `FindProjectionBuilderRedTests.SectionsDistinguishAvailableMissingAmbiguousAndUnavailable`
- `FindProjectionBuilderRedTests.MetadataUsesOnlyEffectiveRouteFactsAndDoesNotInferScope`
- `FindResultBuilderRedTests.EveryFindingCodeMapsToItsFixedStatusAndOrder`
- `FindResultBuilderRedTests.FailedInterruptedAndIncompleteResultsRetainSafeFacts`
- `FindUniverseResolverRedTests.CatalogueIssuesMapToExactFindFindingsAndCandidateCounts`

Fix this in tests only, normally by using public primitive/string theory inputs
and converting to internal enums inside the method. Do not widen production type
accessibility. After these errors, expect more ordinary compile findings until
the full authored packet is built; none have yet been observed or ruled out.

No Red test execution has completed. There are no accepted Red pass/fail/skip
counts, and no proof yet that failures reach only named Gray stubs.

## Useful Existing Test Patterns

- Unit syntax/binding:
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/List/RouteListDefinitionsSyntaxAndBindingTests.cs`.
- Unit invalid workspace/context composition:
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Inspect/RouteInspectBindingAndCompositionTests.cs`.
- Catalogue model factories:
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Sources/Inventory/SourceCatalogueRedTests.cs`.
- Source test data:
  `Framework/Sources/Models/Inventory/SourceInventoryTestData.cs` and
  `Commands/Route/Shared/Models/Source/RouteSourceTestData.cs` under the Unit
  project.
- Real source fixtures under Integration:
  `Framework/Sources/Inventory/SourceCatalogueIntegrationWorkspace.cs`,
  `Framework/Sources/Reading/SourceDocumentReaderIntegrationWorkspace.cs`,
  `Framework/Sources/Routing/SourceRouteFactsIntegrationWorkspace.cs`, and
  `Framework/Sources/Shared/SourceIntegrationWorkspace.cs`.
- Direct-operation and unchanged-workspace pattern:
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Inspect/Shared/Profile/RouteInspectProfileIntegrationWorkspace.cs`.
- Real temporary ownership:
  `src/cli/tests/support/OpenForge.Cli.TestSupport/TemporaryWorkspace.cs`.
- Generated Loader body construction:
  `src/cli/tests/support/OpenForge.Cli.TestSupport/GeneratedLoaderDocumentBuilder.cs`.

Keep any new fixture at the nearest mirrored Find scope. Do not create a generic
`Utils` bag or fake filesystem.

## Helper Sessions

Helper output is evidence only; inspect files directly before acceptance. Session
IDs are retained if resuming a focused helper is cheaper than restarting:

- Mapping explorer `ses_fcd761c2affeB7hcpSFXwx5H9H`: completed read-only map;
  found Gray callable coverage sufficient and the 15 exact Red files above.
- Binding/query Red author `ses_fcd6fc1f6ffe06S12JlMaJUfWa`: reached its limit
  before edits. Resume only for the two missing Unit files, or author them
  directly.
- Document Red author `ses_fcd6fc1d8ffeGEFNvEpH33rnCU`: wrote the four document
  files. It did not verify them because the parallel stage files blocked build.
- Stage Red author `ses_fcd6fc1bfffeGIbzDHqwLoUXnN`: wrote the five stage files.
  It did not compile or run them.
- Integration Red author `ses_fcd6fc1a7ffeEiKmHsv6vKjbEA`: reached its limit
  before edits. Resume only for the four missing Integration files, or author
  them directly.

The previous large Gray assignments and these Red attempts repeatedly reached
agent execution limits. Prefer small exact path packets and sequential validation;
do not launch another broad parallel write/build batch over the same test project.

## Exact Next Actions

1. Re-read current guidance and confirm `HEAD` and worktree status. Preserve all
   nine intended untracked files and this sealed handoff.
2. Inspect the actual nine files against the 34 frozen declaration names. Correct
   only test-local compilation and evidence-shape defects. Fix the seven public
   theory signatures causing `CS0051` without changing production.
3. Build the Unit project. Resolve any newly exposed test-only compile errors.
4. Count actual declarations and executable cases. Reduce redundant theory rows
   so the complete eventual packet remains approximately 90–110 cases while each
   frozen declaration retains material coverage.
5. Author the two missing Unit files and four missing Integration files, preferably
   in small sequential packets. Do not add extra declaration names.
6. Run the Task's focused Unit and Integration filters. Record exact pass, fail,
   and skip counts. Passing definition/model cases are allowed; every intended
   failure must terminate at its named Gray `NotSupportedException` and nowhere
   else.
7. Run locked restore, Release build, format verification, `git diff --check`,
   exact changed-path/protected-surface audits, and affected Source/Route
   regression filters from the Task. Red does not run Find EndToEnd evidence.
8. Update only the authoritative Task's same-commit Red progress unless current
   loaded guidance proves another active record has distinct meaning that must be
   aligned.
9. Review the exact Red diff with fresh targeted correctness and local-improvement
   lenses. Stage only intended tests/fixtures and the required Task progress.
   Inspect the complete cached diff because new files are otherwise absent from
   ordinary `git diff`, then commit accepted Red before Green changes production.

All .NET commands run from `src/cli/`. The exact accepted command list is in the
Task at `Active Toolchain And Commands`.

## Environment Notes

- Platform is Windows, but the available command shell is Bash.
- During this session, the specialized `Glob` and `Grep` tools failed because
  `powershell.exe` was unavailable, and Bash `rg` was also unavailable. `Read`,
  patching, Git, and .NET commands worked. Recheck tool availability rather than
  assuming the failure is permanent.
- The failed Unit build wrote only ignored build artifacts; `git status` still
  showed the nine intended Unit files as the only test worktree changes before
  this handoff was created.
- This handoff is intentionally not inserted by hand into the generated
  `_handoffs.md` Entries region. The known legacy generated-index issue remains
  outside Child 2 Red. Use this direct file path for transfer.

## Stop Conditions

Stop and return to the authoritative Task/parent rather than improvising if:

- a test proves a genuine frozen callable/signature contradiction;
- production, a project/package version, Shell, Route, root composition, JSON,
  generated content, or Child 3 presentation/AOT must change during Red;
- a second parser, Markdown pipeline, source catalogue, cache, persistent index,
  or fake filesystem is proposed;
- effective-universe formation would happen after selected-layer reads, metadata
  would widen the effective universe, or result/status/coverage meaning cannot be
  preserved.

`CLI-EDGE-008` and `CLI-EDGE-009` remain accepted deferred selector/identity
contract limitations; `CLI-EDGE-002` remains the Shell workspace-classification
limitation. Do not silently “improve” them in Child 2. Generated `## Entries`
refresh remains outside this transfer boundary.
