---
open-forge:
  description: "Historical record: F1 through F4 execution plans for applyTo matching, metadata, Entries, and source applicability"
  tags: [Memory, Task, Plan, CLI, Contextual, Archived, Historical]
---

# Task 62 foundation plan

## Archive Status

Archived on 2026-10-04 from `.agents/memory/working/cli-development/tasks/task62/foundation-plan.md` after the maintainer selected Memory cleanup. Original implementation packet binds the initial frozen decisions. Execution labels original horizon complete 3/3,6/6 and the capsule/decisions historical and replaced; current later horizon has its own execution and glob-dialect plan.

This record preserves historical evidence. The [current CLI development route](../../../../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

## Goal

Provide one Framework interpretation of `applyTo` for authoring, navigation,
selection, inspection, and diagnostics.

## Authority and path notation

[Execution](../../../../working/cli-development/tasks/task62/execution.md#frozen-behavioral-decisions) freezes behavior.
[Task 62](../../../../working/cli-development/tasks/task62-glob-scoped-loading.md) defines the outcome. Read those before
this packet. This plan does not reopen the accepted grammar or dependency choice.

The following prefixes expand literally from the repository root. A path listed
below is exact after substitution. `:1` means the whole current file, or line 1
of a new file. Member locations are supplied for the principal integration sites.

| Prefix | Repository-relative path                                    |
| ------ | ----------------------------------------------------------- |
| F      | `src/cli/framework/OpenForge.Cli.Framework/Framework`       |
| U      | `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework` |
| I      | `src/cli/tests/integration/OpenForge.Cli.IntegrationTests`  |

## Preconditions and commands

- [ ] Root records the worker's exact base, branch, worktree, and dependency copies.
- [ ] `git status --short` shows no unexplained changes in the assigned worktree.
- [ ] Read `.agents/directives/csharp/_csharp.md`, `design.md`, and `style.md` in full,
      plus the selected CLI and testing instructions. Do not use this packet as a
      replacement for those sources.
- [ ] Existing packages are restored locally. A missing package is a blocker to
      report, not authority to fetch it.

Use these commands from the assigned worktree. Each step below names its evidence
selection. `--filter-class` is the xUnit selection forwarded after `--`.

```powershell
dotnet build src/cli/framework/OpenForge.Cli.Framework/OpenForge.Cli.Framework.csproj --configuration Release --no-restore
dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-restore -- --filter-class '*ApplyTo*'
```

For an existing affected class, replace only the quoted class filter with the
literal filter named in the step. If the prepared runner rejects this interface,
return that diagnostic to root before choosing another command. Tests must have
the repository's required display name, feature trait, and evidence trait.

## F1: Parse and match patterns

Depends on: none. Blocks: F2, F3, F4.

Own only these new files and their mirrored Unit tests:

- `F/Documents/Shared/Applicability/ApplyToPatternMatcher.cs:1`
- `F/Documents/Shared/Applicability/Models/ApplyToPattern.cs:1`
- `F/Documents/Shared/Applicability/Models/ApplyToPatternParseResult.cs:1`
- `F/Documents/Shared/Applicability/Models/ApplyToPatternFailure.cs:1`
- `U/Documents/Shared/Applicability/ApplyToPatternMatcherTests.cs:1`

1. Add `ApplyToPatternMatcher.Parse(string)` returning `ApplyToPatternParseResult` with nullable
   `Pattern` and `Failure`. Success contains exactly one `ApplyToPattern` with
   `Text` and `ImmutableArray<string> Segments`. Failure contains no pattern.
   Validate the frozen grammar before constructing a successful result. Prove
   scalar text stays one pattern, including literal commas and spaces, through
   `'*ApplyToPatternMatcherTests*'`.
2. Add `IsMatch(pattern, path)` over normalized workspace-relative concrete paths.
   Match ordinary segments with `FileSystemName.MatchesSimpleExpression` and
   explicit case-sensitive behavior. Use bounded dynamic programming over path
   and pattern segment indices for standalone `**`, including its zero-segment
   transition. Do not enumerate the filesystem. Prove `**/*.cs` matches `A.cs`
   and `src/A.cs`, `src/**` crosses folders, `*`/`?` never cross `/`, and casing
   differs on every host through `'*ApplyToPatternMatcherTests*'`.
3. Cover rejected absolute, drive, backslash, empty, dot, parent, control,
   bracket, brace, leading-negation, and embedded-double-star patterns. Cover
   consecutive standalone `**` without exponential recursion. Run the Framework
   build and the F1 test filter.

Acceptance: no new package, native API, regex glob engine, command reference,
filesystem access, or output schema appears in this capability. Root copies the
exact F1 files into F2 before dispatch and records their identities.

## F2: Read, retain, and emit optional metadata

Depends on: F1. Blocks: F3, F4, A1–A3, V1.

Existing edit sites:

- `F/Documents/Metadata/FrameworkDocumentMetadataParser.cs:14`
- `F/Documents/Metadata/Models/FrameworkDocumentMetadataFacts.cs:1`
- `F/Documents/Metadata/Models/FrameworkDocumentMetadata.cs:1`
- `F/Documents/Metadata/Models/FrameworkDocumentMetadataEmission.cs:1`
- `F/Documents/Metadata/Models/FrameworkMetadataYamlModels.cs:1`
- `F/Documents/Metadata/FrameworkMetadataYamlContext.cs:1`
- `F/Documents/Metadata/FrameworkDocumentMetadataEmitter.cs:1`
- `F/Sources/Metadata/SourceOpenForgeMetadataParser.cs:12`
- `F/Sources/Metadata/SourceAuthoredMetadataParser.cs:1`
- `F/Sources/Models/Metadata/SourceOpenForgeMetadataFacts.cs:1`
- `F/Sources/Models/Metadata/SourceAuthoredMetadataFacts.cs:1`
- `U/Documents/Metadata/DocumentMetadataParserTests.cs:1`
- `U/Documents/Metadata/DocumentMetadataEmitterTests.cs:1`
- `U/Sources/Metadata/SourceAuthoredMetadataParserTests.cs:1`

Add the reader at `F/Documents/Metadata/Shared/Applicability/ApplyToMetadataReader.cs:1`.
Place its state, declaration, and facts models in the adjacent `Models/` folder.
Add `U/Documents/Metadata/Shared/Applicability/ApplyToMetadataReaderTests.cs:1`.

1. Implement `Read(YamlDocumentFacts)` returning `ApplyToMetadataFacts` with
   `State` (`Absent`, `Valid`, `Invalid`), `Patterns`, `Declarations`, and
   `Failure`. A declaration retains `Root` or `OpenForge` location and parsed
   source spans for later edits. Consume the existing YAML nodes and spans.
   Do not reparse frontmatter in Sources or commands. Test both locations, a root
   field beside a scoped block, equivalent normalized dual sets, conflicting
   sets, empty/null/non-string values, and malformed patterns with the new filter.
2. Add an independently initialized `ApplyTo` property, defaulting to absent,
   to all three facts models. Compute the field before required-description/tag
   early returns and retain it through every projection. `Missing` required
   metadata must not mean absent applicability. Preserve the native Skill
   description/name contract while reading its `applyTo` independently. Test
   missing metadata, complete ordinary metadata, native Skills, malformed optional
   metadata, and every projection with the three existing metadata test filters.
3. Extend metadata emission models with optional patterns and emit a scoped
   quoted-string list only when supplied. Preserve existing no-field output.
   Use the accepted YAML serializer and its emitter customization, not raw YAML
   concatenation. Assert parse/emit round trips and exact unconditioned output
   with `'*DocumentMetadataEmitterTests*'`.
4. Build the Framework and run all metadata filters above. Include the existing
   route metadata and Skill characterization consumers in the receipt. Adding
   the independent property must not force unrelated constructor rewrites or
   change existing required metadata state merely to carry the optional field.

Acceptance: malformed applicability remains visible independently of ordinary
metadata completeness. Declarations provide sufficient exact positions for A3.
If the existing parsed spans cannot describe a supported authored shape safely,
record it for A3's preservation failure path rather than broadening the parser.

## F3: One Entries grammar and projection

Depends on: F1, F2. Blocks: authoring/navigation acceptance and retrieval dispatch.

Own these integration sites and matching focused tests:

- `F/Sources/Loading/SourceGeneratedEntriesParser.cs:1`
- `F/Sources/Routing/SourceLoaderDeclarationParser.cs:1`
- `F/Sources/Models/Loading/SourceGeneratedEntriesFacts.cs:1`
- `F/GeneratedNavigation/GeneratedNavigationRegionPlanner.cs:17`
- `F/GeneratedNavigation/Models/GeneratedNavigationMetadata.cs:1`
- `F/Sources/Operational/RouteGeneratedEntryComparisonReader.cs:1`
- `U/Sources/Routing/SourceLoaderParserTests.cs:1`
- `U/Sources/Routing/SourceLoaderEntriesParserListRegressionTests.cs:1`
- `U/GeneratedNavigation/GeneratedNavigationProjectorTests.cs:1`
- `U/GeneratedNavigation/GeneratedNavigationSkillProjectionTests.cs:1`
- `U/Sources/Operational/Shared/Routes/RouteGeneratedNavigationProjectionTests.cs:1`

Use the F3 implementation's single product row grammar and formatter:

- `F/Documents/Shared/Entries/MarkdownEntryRowParser.cs:1`
- `F/Documents/Shared/Entries/Models/MarkdownEntryRow.cs:1`
- `F/Documents/Shared/Entries/MarkdownEntryRowFormatter.cs:1`

Its additional focused evidence lives at:

- `U/Documents/Markdown/MarkdownEntriesBlockTests.cs:1`
- `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Doctor/DoctorWorkspaceRouteMappingTests.cs:1`
- `I/Commands/Index/IndexEntriesPreservationTests.cs:1`

1. Have both existing readers consume the same row grammar. Preserve their
   existing surrounding contracts, including Loader's required tags and the
   generated-region empty sentinel. Extend the row model with declared patterns.
   Accept only the frozen suffix after the existing tag slot. Test a row with
   tags, a generated row without tags, multiple code-span patterns, and malformed
   suffixes with `'*MarkdownEntriesBlockTests*'` and both Loader filters.
2. Project only the target's declared patterns, never an inherited synthesized
   expression. Emit ` - applies to` followed by comma-separated code spans after
   tags. Preserve ordinary rows exactly. Feed the declaration from F2 through
   `GeneratedNavigationMetadata` into the region planner. Run the two generated
   navigation test filters with conditioned ordinary files and Skills.
3. Make stale comparison include declared pattern changes. Index must retain
   every normal inventory entry regardless of a task's working files. Run
   `'*RouteGeneratedNavigationProjectionTests*'` and
   `'*DoctorWorkspaceRouteMappingTests*'`, then the Framework build. Prove Index
   idempotence through the managed Integration filter
   `'*IndexEntriesPreservationTests*'` using the Integration command in the
   [command plan](command-plan.md#common-preflight-and-evidence).

Acceptance: a freshly generated Loader or entrypoint parses successfully, repeat
generation is unchanged, and editing only a declaration makes the old row stale.
The authoritative metadata remains the source declaration, not a trusted stale
row. Stop for root if required changes extend outside these neighboring models.

## F4: Evaluate a source chain

Depends on: F1, F2. Blocks: C1, C2, C3.

Own the new capabilities
`F/Sources/Shared/Applicability/SourceApplicabilityEvaluator.cs:1`, its adjacent
`SourceWorkingPathNormalizer.cs:1`, their adjacent `Models/` facts,
`U/Sources/Shared/Applicability/SourceApplicabilityEvaluatorTests.cs:1`, and
`U/Sources/Shared/Applicability/SourceWorkingPathNormalizerTests.cs:1`.
Do not edit command traversal, rendering, or `SourceLoadingClosureResolver` here.

1. Implement `SourceApplicabilityEvaluator.Evaluate(IReadOnlyList<SourceApplyToCondition> chain,
IReadOnlyList<string> workspaceRelativePaths)` returning `SourceApplicabilityResult`.
   `SourceApplyToCondition` carries `CanonicalSourcePath` and
   `ApplyToMetadataFacts Metadata`. `SourceApplicabilityResult` carries `State`,
   `Conditions`, and `MatchingPaths`. `SourceApplicabilityState` has `Unconditioned`, `Matched`,
   `Unmatched`, `Pending`, and `Invalid` states. Conditions retain source
   provenance and declared patterns. Matching paths contain only concrete paths
   that satisfy every non-absent condition in that chain. Keep immutable ordered
   facts for command projections. Empty known input means unknown working files
   for this task's CLI contract, so a nonempty valid condition chain is pending.
2. Test the truth table: no conditions is unconditioned, invalid declaration is
   invalid, valid conditions without paths are pending, one satisfying file is
   matched, and no satisfying file is unmatched. OR applies inside a declaration
   and across files. AND applies across conditions on the same file. The chain
   `src/**` then `**/*.cs` must reject paths `src/readme.md` and `tests/A.cs`
   together, and accept `src/A.cs`. An unconditioned leaf retains its parent's
   conditions. Equivalent dual declarations remain one condition.
3. Implement `SourceWorkingPathNormalizer.Normalize(string workspaceRoot,
IReadOnlyList<string> paths)` returning `SourceWorkingPathResult` with `Paths`
   and `InvalidPaths`. Use lexical `Path.GetFullPath(input, root)` followed by
   `Path.GetRelativePath`, reject outside-workspace and root-only results, and
   produce canonical slash paths. Do not require disk existence. Test relative,
   absolute in-workspace, nonexistent, outside, and root inputs with
   `'*SourceWorkingPathNormalizerTests*'`.
4. Run `'*SourceApplicabilityEvaluatorTests*'` and the Framework build. Return
   the exact model signatures to root before C1–C3 dispatch. Do not add shared
   JSON models. Each command forms its own wire projection.

Acceptance: no evaluator code discovers routes, reads files, follows links,
selects hidden ancestors, normalizes process arguments, or changes metadata.
Those actions retain their existing layers and command policies.

## Evidence and stop conditions

- [ ] All new tests and the named directly affected tests pass on the managed target.
- [ ] Every new source file has a namespace matching its physical ownership.
- [ ] Root receives the exact callable signatures and copied source identities.
- [ ] No command policy or second YAML, Entries, or glob grammar appears locally.

Stop when a consumer needs a materially different shared contract. Return the
exact caller, missing fact, and smallest proposed adjustment to root. Root owns
the architecture checkpoint before derivative implementation proceeds.

## Divergences observed

None recorded. Executors return differences to root for this shared packet.

## Rollback

Preserve a diff of owned edits and delete or restore only the exact assigned
files through root's integration process. Do not reset the worktree, remove
unrelated artifacts, or undo another worker's dependency copies.
