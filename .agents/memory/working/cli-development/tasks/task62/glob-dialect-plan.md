---
open-forge:
  description: Frozen applyTo glob-dialect contract and parallel packets that align Open Forge with APM and Copilot glob syntax
  tags: [Memory, Working, Task, Plan, CLI, Framework, Glob, Contextual, Active]
---

# applyTo glob-dialect alignment

This follow-up to [Task 62](../task62-glob-scoped-loading.md) aligns `applyTo`
syntax with APM and GitHub Copilot. [Execution](execution.md) records why and the
maintainer direction. This file is the frozen contract every packet follows.
Packets explain or implement it. They never reinterpret it. Report a genuine
contradiction to the Overseer instead of choosing another meaning.

Status: Task 62 “Glob-scoped loading” (phase 2/2): milestone 2/3. M3 maintainer review is active.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Evidence

APM, VS Code Copilot and Claude Code all accept comma-separated pattern lists,
brace alternatives and character classes. Open Forge rejected all three. The
tools disagree on anchoring and case: VS Code prepends `**/` and ignores case,
APM matches bare names at any depth and inherits host case rules, and Claude
documents root-relative matching. Open Forge keeps its deterministic model for
those two points. Sources: APM `src/apm_cli/utils/patterns.py`,
`compilation/context_optimizer.py` and `utils/exclude.py`, VS Code
`computeAutomaticInstructions.ts` and `base/common/glob.ts`, and the Claude Code
path-specific rules documentation.

No .NET capability implements this syntax. `Microsoft.Extensions.FileSystemGlobbing`
lacks `?`, braces and classes. `DotNet.Glob` lacks braces and comma lists. The
accepted Task 62 matcher is extended instead, as a bounded BCL-only parser and
matcher exception. It adds no package, regex engine, native code or filesystem
enumeration. Reconsider it if a maintained AOT-compatible .NET library adopts
this syntax.

## Frozen dialect

### Field forms

- `applyTo` stays at the frontmatter root or under `open-forge:`, as one quoted
  string or a list of quoted strings. Both locations keep their meaning, and
  equivalence compares the sets of atomic pattern texts.
- **A string is an expression.** Split it at top-level commas, meaning commas
  outside `{...}` and `[...]`. Inside an expression, `\,` is a literal comma and
  `\\` is a literal backslash. Trim white space around each fragment and drop
  empty fragments. An expression with no fragments fails with `Empty`.
  Decoding happens before brace parsing, as in APM, so `\,` escapes only the
  list separator. `{a\,b,c}` becomes `{a,b,c}` with three alternatives. Write
  `[,]` for a literal comma inside a brace alternative, as in `{a[,]b,c}`.
- **A list entry is one atomic pattern.** Trim it. Do not split it at commas and
  do not decode `\,`. An entry that is empty after trimming fails with `Empty`.
- A string containing two patterns equals a two-item list of the same patterns.
  Brace shorthand is not equated with its expansion.

### Atomic pattern syntax

- Workspace-relative with `/` separators. Keep the existing failures: leading
  `/` or drive prefix is `AbsolutePath`, `.` or `..` segments are `Traversal`,
  empty segments (including a trailing `/`) are `Empty`, and backslashes,
  control characters and a leading `!` are `UnsupportedSyntax`. A leading `!`
  stays rejected because picomatch-based tools treat it as negation while APM
  and VS Code treat it as a literal.
- `**` as a whole segment matches zero or more segments. `**` inside a longer
  segment, such as `a**b` or `***`, behaves as `*`.
- `*` matches any run of characters within one segment, including an empty run
  and a leading dot. `?` matches exactly one character within one segment.
- `[...]` matches one character from a set. Ranges use `a-z`. `[!...]` and
  `[^...]` negate. A `]` directly after `[`, `[!` or `[^` is a literal member. A
  `-` first or last is literal. Classes never match `/`. Bracket forms such as
  `[*]`, `[?]`, `[[]`, `[{]` and `[,]` express literal metacharacters. An
  unterminated class fails with `UnsupportedSyntax`.
- `{a,b}` expands to alternatives before segments are parsed, so an alternative
  may contain `/`, as in `{src,test}/**/*.cs`. Several groups multiply. Empty
  alternatives are allowed. Commas inside a class inside a group do not split
  it. Nested groups, unbalanced braces, and more than 1000 total alternatives
  fail with `UnsupportedSyntax`. A group with one alternative is that
  alternative.
- Every expanded alternative must pass the path rules above. Matching succeeds
  when any alternative matches.
- Matching is anchored at the workspace root, ordinal and case-sensitive on
  every host. Dot-prefixed names match normally. Working paths are matched
  lexically and need not exist. Use `**/*.py`, not `*.py`, for any depth.

### Unchanged behavior

Keep the applicability algebra (OR within one field, AND across inherited
conditions on the same working file), pending, invalid and unmatched states, all
JSON field names and state strings, `ApplyToPatternFailure` members and values,
metadata states and failure kinds, and every existing error message.

## Frozen code contract

Paths below are relative to `src/cli/framework/OpenForge.Cli.Framework/Framework/`.

- Keep `ApplyToPatternMatcher.Parse(string pattern) -> ApplyToPatternParseResult`
  as the **atomic** parser. It never splits commas. Keep
  `ApplyToPatternMatcher.IsMatch(ApplyToPattern pattern, string workspaceRelativePath) -> bool`.
- Keep `ApplyToPattern.Text` (the authored atomic text) and `ApplyToPattern.Segments`
  (the raw `Text.Split('/')`, used by callers only for equality). The matcher may
  add internal members for expanded alternatives.
- Add `Documents/Shared/Applicability/ApplyToPatternExpressionParser.cs` with
  `internal static ApplyToPatternExpressionParseResult Parse(string expression)`.
- Add `Documents/Shared/Applicability/Models/ApplyToPatternExpressionParseResult.cs`,
  a sealed record with `ImmutableArray<ApplyToPattern> Patterns` and
  `ApplyToPatternFailure? Failure`, plus static `Succeeded(ImmutableArray<ApplyToPattern>)`
  and `Failed(ApplyToPatternFailure)`. Success has nonempty `Patterns` and null
  `Failure`. Failure has empty `Patterns`. No partial success.
- `ApplyToMetadataReader` reads a string through the expression parser and each
  list entry through the atomic parser after trimming.
- Emitted metadata stays the quoted scoped list of atomic patterns. Entries keep
  one code span per atomic pattern, with braces and classes unexpanded.

## Frozen CLI contract

- Each `--apply-to` occurrence is an expression. Bind it once through
  `ApplyToPatternExpressionParser.Parse` and pass atomic patterns onward.
  Planning never splits again. Create and Init keep sorted distinct output.
  Update keeps first-seen order when deduplicating.
- Option descriptions:
  - Create: `Apply the destination using workspace-relative glob patterns. Separate patterns with commas or repeat this option.`
  - Init: `Set glob patterns for the missing generic final target. Separate patterns with commas or repeat this option.`
  - Update: `Replace the complete local applyTo list. Separate patterns with commas or repeat this option.`
- Error messages are unchanged.

## Packets

`A` marks the Astra semantic owner, `L` a Luna worker. Owned paths are hard
boundaries. Workers do not commit. The Overseer integrates diffs.

### Wave A (base: this plan's commit)

| ID | Owner | Owned files | Change |
|---|---|---|---|
| K0 | A | `ApplyToPatternMatcher.cs`, new `ApplyToPatternExpressionParser.cs`, new `Models/ApplyToPatternExpressionParseResult.cs`, `Documents/Metadata/Shared/Applicability/ApplyToMetadataReader.cs`, and their Unit tests `ApplyToPatternMatcherTests.cs`, new `ApplyToPatternExpressionParserTests.cs`, `ApplyToMetadataReaderTests.cs`, `Documents/Metadata/DocumentMetadataParserTests.cs` | Implement the frozen dialect and its conformance tests |
| P4 | L | `Commands/Route/Create/RouteCreateDefinitions.cs` | Frozen Create description |
| P5 | L | `Commands/Route/Init/RouteInitDefinitions.cs` | Frozen Init description |
| P6 | L | `Commands/Route/Update/RouteUpdateDefinitions.cs` | Frozen Update description |
| D1 | L | `framework/markdown/syntax.md` | Replace the File Conditions grammar |
| D2 | L | `framework/markdown/compatibility.md` | Expression versus list semantics and the literal-comma migration |
| D3 | L | `framework/markdown/routes.md` | Entries show atomic patterns, one code span each |
| D4 | L | `cli/contracts/context/technical-design.md` | Expression parsing plus atomic matching |
| D5–D11 | L | `cli/contracts/route/{create,init,update}/{interface,behavior}.md`, `route/update/technical-design.md` | One file each: expression input, flatten once, unchanged failures |
| D12 | L | `cli/contracts/index-candidate/interface.md` | One atomic pattern per code span |
| D15 | L | `.agents/skills/open-forge-cli/SKILL.md` and its identical shipped copy | Replace the literal-comma claim and update examples |
| D17 | L | `docs/cli.md` | Syntax, examples, literal-comma migration |
| D18 | L | `src/docusaurus/docs/concepts/loading-and-tags.md` | File conditions syntax paragraph |
| D13 | Overseer | both loader copies and the loader maintenance contract | File Conditions syntax bullet, in the section the loader now calls Frontmatter |

`O` = `src/cli/operations/OpenForge.Cli.Operations/`. Framework contract and
CLI contract paths are relative to `.agents/memory/crystallized/documents/`.

### Wave B (base: integrated Wave A)

| ID | Owner | Owned files |
|---|---|---|
| P1 | L | `O Commands/Route/Create/Shared/Binding/RouteCreateRequestBinder.cs` and Unit `RouteCreateDefinitionsAndBindingContractTests.cs` |
| P2 | L | `O Commands/Route/Init/RouteInitBinding.cs` and Unit `RouteInitDefinitionsAndBindingTests.cs` |
| P3 | L | `O Commands/Route/Update/Shared/Binding/RouteUpdateBindingValidator.cs` and Unit `RouteUpdateDefinitionsAndBindingContractTests.cs` |
| T4–T7 | L | Unit `DocumentMetadataEmitterTests.cs`, `MarkdownEntriesBlockTests.cs`, `GeneratedNavigationProjectorTests.cs`, `SourceApplicabilityEvaluatorTests.cs`, one each |
| T8–T12 | L | Integration `ContextApplyToIntegrationTests.cs`, `FindApplyToIntegrationTests.cs`, `WorkspaceApplyToValidationIntegrationTests.cs`, `RouteInspectOperationApplicabilityIntegrationTests.cs`, `IndexEntriesPreservationTests.cs`, one each |
| T14–T15 | L | EndToEnd `PublishedContextApplyToProcessTests.cs`, `PublishedFindProcessTests.cs`, one each |

### Wave C (base: integrated Wave B)

| ID | Owner | Owned files |
|---|---|---|
| T13 | L | Integration `RouteUpdateApplicationIntegrationTests.cs` |
| T16–T18 | L | EndToEnd `PublishedRouteCreateProcessTests.cs`, `PublishedRouteInitProcessTests.cs`, `PublishedRouteUpdateProcessTests.cs`, one each |

## Follow-up: filter-only applicability and literal `!`

The maintainer reviewed the first result on 2026-09-29 and decided two more
rules. They supersede any earlier statement in this plan or in Task 62 that a
file match loads an entry, and the earlier rejection of a leading `!`.

### Frozen rules

- **`applyTo` is a filter, never a trigger.** Loading comes only from #LoadNow,
  #KeepInMind, explicit selection, and relevance selection. A condition never
  selects or loads a source by itself, whether it is declared locally or
  inherited.
- When working paths are known and no working file satisfies a source's
  effective condition (its own plus every conditioned ancestor on the same
  path), that source does not load automatically, refresh, or apply, whatever
  its tags or category. A matching source behaves exactly as it would without a
  condition. An untagged matching entry therefore stays on demand.
- When working paths are unknown, only an entry that would otherwise load
  automatically (a #LoadNow or #KeepInMind entry exposed by a loaded parent)
  is deferred and reported as a pending condition. An untagged conditioned
  entry is not reported as pending, because nothing would load it automatically.
- Remove the `Applicability` inclusion reason from Framework loading and from
  Context, including its JSON value and text mapping. Keep the applicability
  facts (`state`, `conditions`, `matchingPaths`) on included sources and in
  Route Inspect.
- **A leading `!` is a literal character**, as in POSIX globbing, Python
  `fnmatch`, VS Code, APM and Copilot. `!` keeps its negation meaning only as
  the first member of a class, as in `[!abc]`. Remove `ApplyToPatternFailure`
  use for a leading `!`, not the enum member.
- Everything else in the frozen dialect and code contract is unchanged.

### Wave D packets (base: this addendum's commit)

| ID | Owner | Owned files | Change |
|---|---|---|---|
| S1 | A | `F Sources/Loading/SourceLoadingClosureResolver.cs`, `F Sources/Models/Loading/SourceLoadingClosureModels.cs`, `O Commands/Context/Shared/Selection/ContextLoadingClosureResolver.cs`, `O Commands/Context/Models/Result/ContextInclusionResultModels.cs`, `O Commands/Route/Inspect/Shared/Profile/RouteInspectLoadingFactsBuilder.Entries.cs`, `src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Context/Shared/Wording/ContextWording.cs`, and Unit `Framework/Sources/Loading/SourceLoadingClosureResolverTests.cs` plus any Context or Route Inspect Unit test file that asserts the trigger | Apply the filter-only rule and the pending rule in all three loading decisions. Prefer one shared decision over three copies when that keeps each command's reporting local. Remove the reason kind |
| L1 | L | `F Documents/Shared/Applicability/ApplyToPatternMatcher.cs` and Unit `ApplyToPatternMatcherTests.cs` | Accept a leading `!` as a literal character and test it |
| D20 | L | `README.md` | Filter-only wording in the `applyTo` paragraph |
| D21 | L | `src/docusaurus/docs/concepts/loading-and-tags.md` | Filter-only wording and literal `!` |
| D22 | L | `src/docusaurus/docs/getting-started/first-task.md` | Filter-only wording in step 5 |
| D23 | L | `src/docusaurus/src/components/framework-map/framework-map-data.ts` and both regenerated SVGs | Filter-only caption, then regenerate |
| D24 | L | `docs/cli.md` | Filter-only Context behavior and literal `!` |
| D25 | Overseer | both loaders, `.agents` contracts, decisions and this plan | Filter-only and literal `!` wording |

Wave E (base: integrated Wave D) assigns one Luna worker to each Integration or
EndToEnd test file that fails because it asserted the old trigger or the `!`
rejection.

## Milestones

1. Contract frozen and Wave A dispatched
2. K0 and Wave A integrated, Framework builds, K0 tests pass
3. Waves B and C integrated with focused tests passing
4. Complete managed suite and Windows Native AOT gate pass
5. Documentation reconciled and maintainer review

Filter-only follow-up milestones:

1. Wave D integrated with focused tests passing
2. Wave E integrated, complete managed suite and Windows Native AOT gate pass
3. Maintainer review
