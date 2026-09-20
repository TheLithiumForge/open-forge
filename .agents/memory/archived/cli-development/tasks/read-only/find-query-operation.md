---
open-forge:
  description: Implement the exact Find query operation over the accepted source catalogue and fixed Markdown facts
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Find, ReadOnly, Query, Markdown, Matching, Projection]
---

# Implement The Find Query Operation

## Task State

- State: Complete and accepted at exact commit `ff7ce3f` (`Accept Find query
operation`). Original
  Preflight through Blue history is accepted through exact
  commit `685e2dd`. Purple exposed a top-down architecture defect: generic YAML
  event parsing remained command-local and Route duplicated Markdown frontmatter
  extraction. Child 2 completed the Mastermind-owned shared-foundation correction
  and corrected Purple. Correction Gray is accepted at exact
  `2cae7a4`, Red at exact `a673ebe`, and Green at exact `d4701ad`. Correction Blue
  is accepted at exact `0006915`, and Purple at exact `2d10474`. Final review found
  one original Green contract mismatch for invalid authored tag scalars. Its
  supplemental Red is accepted at exact `c72dd5e`, and the correction to one
  conflicting inherited Unit expectation at exact `d494adb`. The corresponding
  Integration expectation correction is accepted at exact `23fe39a`. Corrected
  Green is accepted at exact `2337d62`; final evidence and acceptance are recorded
  at exact `ff7ce3f` (`Accept Find query operation`). The Modern C# Task is
  Complete, and
  [Establish The Neutral Find Source Catalogue](find-source-catalogue.md) remains
  accepted at exact commit `96fe413`.
- Responsible role: Mastermind. Delegation is allowed only after the predecessor,
  callable surface, and evidence packet are frozen.
- Parent: [Implement Find](find.md).
- Predecessor: [Establish The Neutral Find Source Catalogue](find-source-catalogue.md).
- Task source: This file.
- Last updated: 2026-08-25.
- Planning baseline: `e77902a`; initial production/source baseline: exact
  `063c59d`; source-catalogue predecessor: exact accepted Child 1 commit
  `96fe413` (`Accept Find source catalogue`); execution baseline: exact accepted
  Modern C# commit `a1cbf09` (`Accept Modern C# improvements`).

## Expected Outcome

One directly callable Find operation resolves the exact grammar and effective
source universe, reads fixed document facts, performs the accepted tag and
heading matching, forms projections and coverage, and returns the typed Find
result defined by the Interface. It does not register the public root or add
a renderer.

## Relationships And Authority

| Relationship             | Link                                                                                                                                                                                                                                                               | Relevance                                                                                     |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------- |
| Parent                   | [Implement Find](find.md)                                                                                                                                                                                                                                          | Defines the complete outcome, sequence, and protected public meaning.                         |
| Predecessor              | [Find Source Catalogue](find-source-catalogue.md)                                                                                                                                                                                                                  | Supplies neutral source identities, layers, issues, and strict selected-layer reads.          |
| Interface                | [Find Interface](../../../../crystallized/documents/cli/contracts/find/interface.md)                                                                                                                                                                               | Defines grammar, exact command-local schema, findings, statuses, projections, and `next`.     |
| Behavior                 | [Find Behavior](../../../../crystallized/documents/cli/contracts/find/behavior.md)                                                                                                                                                                                 | Defines deterministic stages, effective-universe precedence, coverage, and no-write behavior. |
| Technical design         | [Find Technical Design](../../../../crystallized/documents/cli/contracts/find/technical-design.md)                                                                                                                                                                 | Defines Markdig, source-location, YAML, comparison, AOT, and evidence choices.                |
| Shared filters           | [Source-Universe Filters](../../../../crystallized/documents/cli/contracts/shared/source-universe-filters/behavior.md)                                                                                                                                             | Defines selector resolution, expansion, set algebra, safety, and reporting handoff.           |
| Shared operation         | [Shared CLI Operation Contract](../../../../crystallized/documents/cli/shared-operation-contract.md)                                                                                                                                                               | Defines one request/result flow, status precedence, streams, and renderer locality.           |
| Implementation and tests | [CLI Implementation Directive](../../../../../directives/open-forge/cli/implementation.md), [Test Evidence Integrity](../../../../../directives/open-forge/testing/evidence-integrity.md), and [Evidence Tiers](../../../../../patterns/testing/evidence-tiers.md) | Bind project, AOT, isolation, and evidence boundaries.                                        |

## Allowed And Protected Surfaces

### Allowed

- One `Markdig` `PackageReference` in
  `src/cli/core/OpenForge.Cli.Core/OpenForge.Cli.Core.csproj`, with its version
  still central at `1.3.2`; no version change and no new project.
- `src/cli/core/OpenForge.Cli.Core/Framework/Documents/{Markdown,Yaml}/` for the
  fixed neutral frontmatter, Markdown, and YAML syntax boundaries.
- `src/cli/core/OpenForge.Cli.Core/Commands/Find/` for definitions, binding,
  request, result, and operation, with Find-local
  `Shared/{Query,Documents,Selection,Matching,Projection,Result}/` support.
- Existing `CliYamlContext` registrations and metadata models only as needed for
  one static source-generated YAML context.
- Mirrored Find Unit and real-OS Integration tests, plus directly affected test
  support where the accepted evidence boundary requires it.
- `Commands/Route/Shared/Source/RouteMetadataParser.cs` and its mirrored Unit
  evidence solely to consume the shared frontmatter/YAML foundation without
  changing Route metadata meaning.
- The active Working records needed for this child's phase and acceptance.

### Protected

- Root composition, public renderers, `CliJsonContext` registration, EndToEnd
  public wiring, Shell and global parser semantics, Route behavior and JSON beyond
  the behavior-preserving shared parser migration,
  project graph, package versions, generated code, and unrelated commands.
- Any second source catalogue, document/query engine, persistent index, or cache.

## Planned Callables And Document Boundary

Acceptance of the parent planning boundary fixes the ownership below. The
original focused Preflight and Gray contract are accepted history. The active
shared-foundation correction augments their neutral document boundary without
changing the accepted Find callable or result ownership.

- `FindDefinitions` owns Find syntax identities, local finite values, finding
  machine codes, result command identity, and next-action contents.
- `FindSymbols` owns the exact `Command` and seven operation-specific `Option`
  instances. `FindRequestBinder` converts parser-owned values and occurrences into
  one immutable `FindRequest`; it does not scan original arguments.
- `FindQuery`, `FindPredicate`, `FindRegionSelection`, and
  `FindContentSelection` preserve supplied and effective values separately under
  the Interface order and deduplication rules.
- `FindOperationComponents` groups the accepted source catalogue, route-facts,
  selected-layer reader, frontmatter, and Markdown callables. It contains no
  renderer or process writer.
- `FindOperation.ExecuteAsync(FindRequest, CancellationToken)` returns one
  immutable `FindResult`. `FindResultBuilder` is the only aggregate status,
  coverage, finding-order, match-order, projection, and next-action policy. The
  result exposes every domain fact needed by the exact Interface schema without
  depending on JSON DTOs.
- Types with more than three cohesive constructor inputs use one nearest-scope
  named component/input record. New callables have at most four parameters and do
  not add compatibility overloads.

- `MarkdownPipelineFactory` owns one cached Markdig `1.3.2` CommonMark pipeline.
  Plugin discovery and extensions are disabled. There is no second pipeline or
  dynamic configuration.
- `MarkdownFrontmatterParser` returns the exact neutral frontmatter boundary
  without invoking Markdig. `MarkdownDocumentParser` consumes that boundary and
  returns neutral `MarkdownDocumentFacts`, ordered
  `MarkdownHeadingFact` values, and `MarkdownSectionFact` values with the accepted
  visible text and section boundaries. It does not form Find statuses, findings,
  renderers, or public JSON.
- Find-local `Utf8SourceMap`, body-tag scanning, generated-`Entries` exclusion,
  and compatibility adaptation preserve the shared
  `{line,column,byteOffset,byteLength}` location shape. They do not normalize
  Unicode or broaden authored syntax.
- `YamlDocumentParser` owns one neutral source-preserving YamlDotNet event
  traversal under `Framework/Documents/Yaml`. It returns decoded scalar values,
  half-open UTF-16 node spans, sequence and mapping shape, and explicit alias,
  duplicate-key, and non-scalar-key facts without command policy.
- `FindFrontmatterReader` uses the existing source-generated YamlDotNet context
  for semantic values and the shared YAML facts for exact `open-forge.tags`
  scalar spans. It correlates semantic values and scalar facts in source order,
  maps spans through `Utf8SourceMap`, and returns
  unavailable rather than guessing when they disagree. It adds no handwritten
  YAML grammar and does not reuse Route metadata policy as Find meaning.
  Malformed frontmatter makes coverage incomplete only when semantic
  frontmatter matching requires it; raw frontmatter bytes and body-only work
  remain independently usable when their boundaries are established.

## Requirements

- Resolve shared include and exclude selectors through the accepted neutral
  catalogue and shared source-reference parser. Form the effective universe
  before `SourceDocumentReader` reads any selected layer.
- Request neutral route facts only for logical sources in that effective universe
  when `metadata` is selected. A supporting source outside the effective universe
  remains unread and makes the affected metadata projection unavailable rather
  than silently widening the filter.
- Preserve exact `all`/`any` flat predicate logic, first-occurrence deduplication,
  command-line and source ordering, default and explicit regions, body-tag and
  heading rules, and matching across base and overwrite layers.
- Form the exact content projections, candidate/inspected/matched counts,
  matching and projection coverage, finding codes and order, semantic status, and
  `next` values from the Interface. Do not invent a result shape or status.
- Retain cancellation facts and safe matches according to the accepted operation
  boundary. Invoke one operation once; renderers later consume its cached typed
  result and never rerun enumeration, parsing, matching, or projection.
- Keep the operation stateless. It creates no persistent index, cache, receipt,
  network request, or workspace mutation.

## Execution And Evidence

The original Child 2 Gray, Red, Green, and Blue sequence is accepted through
`685e2dd`. The active shared-foundation correction uses its own bounded Gray → Red
→ Green → Blue sequence before corrected Purple. Original evidence includes the
Unit query matrix and real-OS
Integration coverage for Turkish culture, ordinal comparison, UTF-8 byte origins,
block and flow YAML tag sequences, quoted and escaped scalar tags, comments,
Unicode and CRLF parser marks, ATX and Setext headings, visible inline text, raw
HTML/code/link exclusions, sections, selectors, overwrite-layer matching,
ordering, projections, and direct invalid, blocked, incomplete, attention,
failed, and interrupted result paths.
Focused evidence also proves zero candidates and zero matches, cancellation
retention, repeated deterministic invocation, and no writes. Public presentation
and AOT execution belong to Child 3.

## Historical Focused Preflight Adoption

The Mastermind adopts the focused Preflight below under the accepted Find parent,
contracts, Architecture, and user direction to continue. This adoption resolves
constructor, property, callable, evidence, and phase detail only. It does not
change product contracts or authorize Child 3 presentation surfaces.

### Baseline, Isolation, And Beginning Evidence

- Exact Gray baseline: clean `feature/cli-find` at `a1cbf09`. The branch began
  from the accepted Find program line while `develop` remains `e77902a`. The
  parent-authorized Child 1 and Modern C# continuation is the recorded exception
  to creating another branch for this child; no unrelated Task enters the branch.
- Exact source predecessor: `96fe413`. The later Modern C# commits changed
  nullable flow and structure without changing the accepted source-catalogue
  behavior, contracts, tests, projects, packages, or dependencies.
- Beginning executable evidence is the accepted unchanged Modern C# gate from
  clean source commit `6af5fb1`, recorded at `a1cbf09`: Unit `617/617`,
  Integration `241/241`, EndToEnd `57/57`, focused Source/Route Unit `562/562`,
  focused Source/Route Integration `183/183`, Native AOT Integration `241/241`,
  Native AOT EndToEnd `57/57`, zero skips, and clean package, vulnerability,
  artifact, and public no-write audits.
- Preflight remained read-only for repository source, tests, projects, contracts,
  and generated content. The System.CommandLine, Markdig, and YamlDotNet probes
  used temporary paths outside the repository.

### Dependency And Parser Evidence

- Core adds the one already accepted central `Markdig` reference during Gray.
  `Directory.Packages.props` remains unchanged at exact version `[1.3.2]`; no
  package, version, project, solution, or graph decision is reopened.
- Markdig package `1.3.2` records repository commit
  `fc705234fa211d179ee1d5e7656b51ab99f70ca9`. A temporary .NET 10 Native AOT
  probe parsed ATX and Setext headings with
  `new MarkdownPipelineBuilder().UsePreciseSourceLocation().Build()`, reported
  zero registered extensions, and executed successfully. The package declares no
  `IsTrimmable` or `IsAotCompatible` metadata. This is dependency evidence, not
  the integrated Find AOT claim owned by Child 3.
- YamlDotNet `18.1.0` event marks expose a zero-based character index and
  one-based line and column. Scalar end marks are exclusive, quoted scalar spans
  include their source quotes, and semantic values are decoded independently.
  Find maps the character indexes through its UTF-8 source map instead of trusting
  parser-native line or byte coordinates.
- System.CommandLine `2.0.11` accepts spaced, equals, and colon long-option value
  forms. `Option<string[]>` with `ZeroOrMore` and
  `AllowMultipleArgumentsPerToken = false` aggregates repeated scalar values and
  rejects a second value on one occurrence, but an explicit occurrence without a
  value remains visible only through identifier and token facts.
- One bounded Find-local occurrence adapter is required because an aggregated
  `OptionResult` loses the cross-option order between `--tag` and `--heading`.
  The adapter reads only `ParseResult.Tokens`, recognizes the exact option names
  from `FindDefinitions`, associates only parser-classified adjacent argument
  tokens, and cross-checks those values with the typed per-option arrays. It never
  reads process arguments, interprets delimiters, tokenizes text, or changes
  parser diagnostics. Re-evaluate the adapter when the pinned parser version
  changes or exposes an ordered occurrence API.

The temporary probes remain reproducible without becoming repository artifacts:

```bash
dotnet run --project "<temp>/opencode/system-commandline-2.0.11-harness/system-commandline-2.0.11-harness.csproj"
dotnet run --project "<temp>/opencode/cli-yaml-spike/cli-yaml-spike.csproj"
dotnet publish "<temp>/opencode/markdig-1.3.2-research/inspect/inspect.csproj" --configuration Release --runtime win-x64 -p:PublishAot=true --output "<temp>/opencode/markdig-1.3.2-research/publish-aot"
"<temp>/opencode/markdig-1.3.2-research/publish-aot/inspect.exe"
```

The parser probe's mixed input
`--item one --heading=H1 --item:two --heading H2` produced the normalized token
sequence `Option --item`, `Argument one`, `Option --heading`, `Argument H1`,
`Option --item`, `Argument two`, `Option --heading`, `Argument H2`, while each
`OptionResult` retained only its two same-option values. An explicit missing
`--item` retained one identifier and zero value tokens. The YAML probe reported
the first scalar at index `0`, line `1`, column `1`; quoted scalar start/end
indexes enclosed their quotes. The Native AOT Markdig executable reported
`extensions=0` and emitted the expected ATX and Setext heading facts. Focused Red
repeats every behavior on retained repository test surfaces before Green may rely
on it.

### Frozen Production Structure

All new models use immutable construction, read-only snapshots, truthful
nullability, and matching physical namespaces. The exact topical paths below may
split a file when the listed types would otherwise make one class materially
overlarge, but they may not rename, widen, or merge the frozen semantic units.

#### Neutral Markdown Boundary

`Framework/Documents/Markdown/` contains only the fixed parser adapter and
technology-neutral document facts:

| Surface                                | Frozen members and meaning                                                                                                                                                                        |
| -------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `MarkdownTextSpan`                     | `Start`, `Length`, and checked half-open `End`, all UTF-16 character indexes into one exact source string.                                                                                        |
| `MarkdownFrontmatterBoundary`          | `State` (`Missing`, `Complete`, or `Unavailable`), complete block span, inner YAML span, and body-start boundary. Missing frontmatter establishes an empty frontmatter and a whole-document body. |
| `MarkdownHeadingFact`                  | `VisibleText`, `Level`, `Form` (`Atx` or `Setext`), `IsCanonical`, and the complete authored heading-node span. Only ATX is canonical.                                                            |
| `MarkdownSectionFact`                  | Its heading fact and exact section span from that heading through the character boundary before the next heading of the same or higher level, or through body end.                                |
| `MarkdownVisibleTextFact`              | One parser-approved literal-text span from body content. Code, raw HTML blocks and markup, and link destinations never become visible-text facts.                                                 |
| `MarkdownOpaqueSpan`                   | One ordered parser-approved code or raw-HTML source span. Find uses these neutral exclusions to avoid recognizing generated markers inside code examples without importing parser nodes.          |
| `MarkdownDocumentFacts`                | Exact source, frontmatter boundary, optional body span, ordered headings, sections, visible-text facts, and opaque spans. It exposes no Markdig node or parser-native span.                       |
| `MarkdownPipelineFactory.Get()`        | Returns one cached Markdig pipeline built only with `UsePreciseSourceLocation`; `Extensions.Count` remains zero.                                                                                  |
| `MarkdownDocumentParser.Parse(string)` | Splits exact start-of-file frontmatter, parses only the established body, offsets every Markdig span back into the full source, and returns the neutral facts above.                              |

The opening and closing frontmatter delimiter lines are exactly `---`. A complete
frontmatter projection span includes both delimiter lines but not the line break
after the closing delimiter. Its YAML span excludes both delimiters. The body
starts after the closing delimiter's line break. An opening delimiter without a
closing delimiter makes both semantic frontmatter and the body boundary
unavailable; malformed YAML inside complete delimiters does not change the raw
frontmatter or body spans.

Visible heading text traverses Markdig inline nodes without an HTML renderer.
Literal text, decoded entities, soft and hard breaks, inline-code content, and
link or image labels contribute. Formatting markers, destinations, titles, and
raw HTML do not. Unicode whitespace collapses to one ASCII space and outer
whitespace is removed. The adapter performs no Unicode normalization.

#### Find Request And Result Models

`Commands/Find/Models/` contains these exact model families:

| Family       | Frozen shape                                                                                                                                                                                                                                                                            |
| ------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Request      | `FindRequest` contains `CliWorkspace`, `FindUniverseFilter`, `FindQuery`, and `FindPresentationSelection`. `FindUniverseFilter` snapshots supplied include and exclude strings.                                                                                                         |
| Query        | `FindPredicate` contains kind, supplied value, and comparison value. `FindQuery` snapshots supplied and effective predicates, `FindRequirement`, and `FindRegionSelection`. `FindRegion` contains kind, optional name, and canonical value.                                             |
| Presentation | `FindPresentationSelection` contains nullable supplied `CliView`, effective `CliView`, and `FindContentSelection`. `FindContentPart` contains kind, optional name, and canonical value. Supplied parts preserve parsed order; effective parts are deduplicated in canonical part order. |
| Selection    | `FindUniverse`, `FindSelector`, `FindSourceIdentity`, `FindUniverseMode`, `FindSelectorResolution`, `FindSourceKind`, and `FindSelectorExpansion` mirror the exact Interface universe and selector fields. Counts are nullable `int` values.                                            |
| Result       | `FindCoverage`, `FindFinding`, `FindMatch`, `FindEvidence`, `FindHeadingEvidence`, `FindProjection`, `FindMetadata`, `FindMetadataLayer`, `FindProjectedHeading`, and `FindSourceLocation` mirror every Interface field without renderer or JSON DTO meaning.                           |
| Aggregate    | `FindResult` implements `ICliCommandResult`; its command, status, workspace, and next action are the shared process facts, while universe, query, presentation, coverage, findings, and matches are the exact command-local graph.                                                      |
| Operation    | `FindOperationComponents`, stage inputs, `FindSourceReadContext`, and `FindResultInput` group one invocation's dependencies and established facts without becoming a service locator or persistent session.                                                                             |

`FindSourceLocation` uses `int` line and column plus `long` byte offset and byte
length. Its span is half-open. `FindResult` and its nested models validate enum,
null, count, identity, layer, projection-payload, evidence-index, ordering, and
status/coverage invariants at construction, but they do not derive domain results.
Child 3 alone adds concrete JSON projection models.

The result reuses `SourceLayerKind` for base and overwrite identity and shared
`CliSemanticStatus`, `CliView`, `CliWorkspace`, and `CliNextAction` facts. It does
not duplicate Framework source models or import Route command models.

#### Frozen Callables

| Callable                                                                                 | Exact boundary                                                                                                                                                                                                                                                                                                                              |
| ---------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `FindSymbols.Create()`                                                                   | Returns one detached `find` `Command`, four repeatable scalar array options, three singleton scalar options, and no positional argument or delimiter policy.                                                                                                                                                                                |
| `FindRequestBinder.Bind(CliBindingParse, CliInvocation)`                                 | Reads typed parser values and parser-owned occurrence facts and returns one bound request or one concrete invalid result.                                                                                                                                                                                                                   |
| `FindWorkspaceResultFactory.Create(CliInvalidBindingInput)`                              | Rebinds recoverable Find-local values through its closed symbols so local invalid input retains precedence; otherwise converts the Shell workspace-selection failure into one exact `workspace-unavailable` blocked Find result without invoking the operation. Parser and global-semantic failures retain the accepted Shell-invalid path. |
| `FindQueryParser.Parse(FindQueryInput)`                                                  | Validates tag, heading, requirement, escaped region/content, repetition, dependency, deduplication, and compatibility rules and returns one typed resolution.                                                                                                                                                                               |
| `FindSourceBoundaryReader(CliWorkspace, CancellationToken)`                              | Creates one invocation-scoped `SourceDocumentReader`, reads one `.agents` `SourceCatalogue`, resolves the default `.agents` physical selection scope, and returns one `FindSourceReadContext`.                                                                                                                                              |
| `FindPhysicalPathResolver(CliWorkspace, string)`                                         | Adapts one canonical source path to the existing `PhysicalPathResolver` result for selector classification and scopes.                                                                                                                                                                                                                      |
| `FindSelectedLayerReader(SourceDocumentReader, SourceLayer, CancellationToken)`          | Calls the accepted selected-layer reader exactly once per uncached layer request.                                                                                                                                                                                                                                                           |
| `FindRouteFactsReader(SourceRouteFactsRequest, SourceDocumentReader, CancellationToken)` | Calls the accepted neutral route-facts resolver only for requested metadata projections and only with the effective selection.                                                                                                                                                                                                              |
| `FindMarkdownDocumentReader(string)`                                                     | Calls the one neutral `MarkdownDocumentParser`.                                                                                                                                                                                                                                                                                             |
| `FindFrontmatterFactsReader(FindFrontmatterInput)`                                       | Correlates source-generated semantic YAML with shared neutral scalar facts and returns description, ordered tag occurrences, and availability.                                                                                                                                                                                              |
| `FindUniverseResolver.Resolve(FindUniverseInput)`                                        | Resolves selectors, expansions, set algebra, effective selection, issue projection, and candidate count before any selected-layer read.                                                                                                                                                                                                     |
| `FindLayerInspector.InspectAsync(FindLayerInspectionInput, CancellationToken)`           | Maps one selected read into neutral document, frontmatter, body-tag, heading, section, description, and finding facts.                                                                                                                                                                                                                      |
| `FindBodyTagScanner.Scan(FindBodyTagInput)`                                              | Scans only parser-approved literal spans outside generated `Entries` regions and returns exact visible tag occurrences.                                                                                                                                                                                                                     |
| `FindMatcher.Match(FindMatchingInput)`                                                   | Applies effective predicates, regions, `all` or `any`, cross-layer evidence, safe-match retention, and deterministic evidence order.                                                                                                                                                                                                        |
| `FindProjectionBuilder.Build(FindProjectionInput)`                                       | Projects only already matched sources from cached inspection and optional route facts; it performs no read or match.                                                                                                                                                                                                                        |
| `FindResultBuilder.Build(FindResultInput)`                                               | Is the only status, coverage, count, finding order, match order, projection order, and exact next-action authority.                                                                                                                                                                                                                         |
| `FindOperation.ExecuteAsync(FindRequest, CancellationToken)`                             | Coordinates one boundary read, one universe resolution, selected-layer inspection, matching, optional route facts and projection, and one result build.                                                                                                                                                                                     |
| `FindOperationFactory.Create()`                                                          | Closes the concrete components and returns one directly callable `FindOperation`.                                                                                                                                                                                                                                                           |

Constructor dependencies are also frozen: `FindRequestBinder` receives
`FindSymbols` and `FindResultBuilder`; `FindWorkspaceResultFactory` receives
`FindSymbols` and `FindResultBuilder`; `FindUniverseResolver` receives the
physical-path delegate;
`FindLayerInspector` receives the selected-layer, Markdown, and frontmatter
delegates plus one `FindBodyTagScanner`; and `FindOperation` receives one
`FindOperationComponents`. `FindQueryParser`, `FindMatcher`,
`FindProjectionBuilder`, `FindResultBuilder`, `MarkdownFrontmatterParser`,
`MarkdownDocumentParser`, `YamlDocumentParser`, and `FindFrontmatterReader` own no
injected dependency. `Utf8SourceMap` receives one
exact source string. `FindSourceReadContext` contains exactly the catalogue,
invocation-scoped document reader, and nullable default `.agents` selection
scope. `FindQueryInput` contains typed per-option values, parser-owned occurrence
facts, singleton spellings, and supplied/effective view facts. `FindResultInput`
contains the request echo, optional established universe, completed inspections,
safe matches and projections, findings, stage completion, and terminal event.

In the original Child 2 Gray, every domain-behavior callable from request binding
through result building throws one named `NotSupportedException`. Definitions,
detached symbol
construction, immutable model construction, factory/component wiring, and the
package reference compile in Gray but perform no query, read, parse, match,
projection, or aggregate result behavior.

`FindOperationComponents` contains exactly the six boundary callables above plus
one `FindResultBuilder`. Pure query, universe, inspection, matching, and projection
components are concrete nearest-scope collaborators created by the factory. Tests
may replace the six effect/parser boundaries, but no fake filesystem enters
production or Integration evidence.

### Binding And Query Decisions

- Include, exclude, tag, and heading symbols are `Option<string[]>` with
  `ArgumentArity.ZeroOrMore` and `AllowMultipleArgumentsPerToken = false`.
  Binding requires one nonempty typed value for every identifier occurrence.
- Require, within, and content are `Option<string?>` with
  `ArgumentArity.ZeroOrOne`. Binding rejects omission after an explicit
  identifier and rejects more than one identifier, including equal repeats.
  Defaults are formed in the request, not by a second parser.
- The supplied predicate array follows the bounded parser-token occurrence
  adapter. Effective predicates remove later equivalent values while retaining
  the first supplied spelling and position. Tag equivalence removes exactly one
  leading ASCII `#` and compares ordinal-ignore-case. Heading equivalence compares
  the complete untrimmed supplied value ordinal-ignore-case.
- Tag validation operates on Unicode scalar values: the first value is a letter;
  later values are letters, digits, or non-leading, non-trailing, non-repeated
  ASCII hyphens. A comma or surrounding whitespace is invalid.
- The escaped-list parser accepts only `\,` and `\\`, ignores whitespace around
  parts, preserves whitespace inside section names, and rejects empty parts,
  empty section names, unknown parts, trailing escapes, and every other escape.
- `within.supplied` and `content.supplied` retain all parsed occurrences in input
  order. Effective arrays remove equivalent duplicates. `document` subsumes all
  tag regions and all heading body/section regions; `body` subsumes explicit
  sections. Inapplicable frontmatter regions are removed from effective heading
  regions when another supported region remains; an entirely incompatible
  selection is invalid.
- Omitted tag regions are frontmatter, omitted heading regions are body, omitted
  requirement is all, omitted content is empty, and omitted view is represented
  as supplied `null` with effective expanded. Explicit `--view=expanded` remains
  distinguishable through parser-owned occurrence facts. JSON does not alter the
  stored supplied or effective view.
- An early invalid result preserves every safely recoverable supplied value.
  Because the Interface selector schema has no not-started resolution value,
  selector occurrences whose source resolution did not start use `invalid` with
  their parsed form and null identity; they do not receive an
  `find.invalid-selector` finding unless the selector itself is invalid. This
  accepted finite-state reuse and its possible future contract extension are
  recorded as `CLI-EDGE-008`.
- Parser, delimiter, and global-semantic failures stop in the shared Shell before
  command binding and retain its ordinary invalid diagnostic path. Find's
  workspace result factory is used only after the Shell selected the Find binding
  and workspace resolution failed. The current Shell intentionally exposes no
  typed workspace-selection state at that boundary. The factory first validates
  recoverable Find-local values so local invalid input retains shared invalid
  precedence; otherwise the workspace failure maps to
  `find.workspace-unavailable`. `find.workspace-unsafe` remains the mapping for an
  unsafe `.agents` catalogue root after workspace selection succeeds. The factory
  closes over `FindSymbols` and reads only `CliInvalidBindingInput.BindingParse`
  plus typed global input. The accepted Shell classification limitation remains
  recorded in `CLI-EDGE-002`. Find-local query/value errors remain concrete
  invalid Find results from `FindRequestBinder` or this precedence check.

### Document, Frontmatter, And Origin Decisions

- `Utf8SourceMap` maps validated UTF-16 character boundaries to one-based
  Unicode-scalar line and column plus zero-based UTF-8 byte coordinates. It
  treats CRLF as one line break, supports lone CR and LF, validates surrogate
  boundaries, and supports zero-length spans including end-of-file.
- Heading and projected-heading locations cover the complete authored heading
  node, including a Setext underline. Frontmatter and body locations cover their
  exact projected text spans. Section locations cover the exact complete section.
  Body-tag locations cover the authored tag name without its leading `#`.
- The frontmatter reader uses the existing `CliYamlContext`,
  `CliAuthoredMetadata`, `CliOpenForgeMetadata`, and `CliSkillMetadata`; no YAML
  context or DTO change is planned. It parses only a complete inner YAML span
  through the shared neutral syntax parser.
- Semantic tag values come only from `open-forge.tags`. Skill top-level metadata
  supplies description but no Find frontmatter tags. The logical source
  description comes only from the base layer: `open-forge.description` for
  ordinary forms and top-level `description` for `SKILL.md`. Find does not merge
  or replace descriptions through overwrite metadata.
- Shared event traversal accepts block or flow sequences, comments, quoted
  values, and decoded escapes through YamlDotNet. Find correlates the semantic
  array and neutral scalar facts in source order by decoded value. Aliases,
  duplicate paths, wrong value
  shapes, malformed YAML, or any semantic/event disagreement make semantic
  frontmatter unavailable rather than guessing a location.
- Missing frontmatter, missing `open-forge`, and missing or empty tags are complete
  zero-tag facts. Authored scalar values outside the accepted tag grammar cannot
  match any valid query and are ignored for Find matching; structural diagnosis
  remains with `doctor`.
- Semantic frontmatter unavailability emits
  `find.frontmatter-unavailable` only when an effective tag predicate searches
  frontmatter. Raw frontmatter, established body facts, and body-only matching or
  projection remain independently available.
- Body tags are whole visible bare tokens. The scanner uses Unicode letter and
  digit rules plus ASCII hyphens and rejects a token adjacent to another `#`,
  letter, digit, hyphen, or underscore. It scans literal text in paragraphs,
  lists, blockquotes, headings, emphasis, and link/image labels. It does not scan
  inline or block code, raw HTML blocks or markup, destinations, titles,
  autolink URLs, escaped markers, or parser-produced entity text that did not
  contain an authored bare `#`.
- Exact generated-index marker lines are recognized only when Markdig classifies
  them as raw-HTML comment spans, never inside code examples. One ordered pair
  forms an opaque body span for tag matching. An unmatched, nested, reversed, or
  repeated recognized marker boundary makes body-tag matching unavailable for
  that layer instead of guessing which visible list lines are generated. Heading
  matching and raw projections remain independent.

### Universe, Matching, Projection, And Result Decisions

- The default universe is one catalogue over `.agents`. Candidate count is the
  number of effective logical sources plus each selected recognized candidate
  that could not form a logical source. A valid base/overwrite pair counts once.
  An unknown number hidden by an unavailable directory is not guessed.
- A resolved Loader, recognized entrypoint, or `SKILL.md` contributes its exact
  physical parent scope. An ordinary source contributes itself. Includes union,
  excludes union, and exclusion wins. The resolver passes effective sources plus
  included and excluded physical scopes to `SourceCatalogue.Select`, so
  source-less issues outside the effective universe do not leak into coverage.
- Exact-path resolution uses the existing physical resolver when no catalogue
  candidate exists: missing is unknown, a contained file or directory that is
  not an admitted catalogue source is unsupported, and every unsafe or unavailable
  physical boundary is selector-unsafe. The probe classifies the existing finite
  outcome only; it never admits a second source outside the catalogue. The accepted
  `unsupported` reuse for a physically eligible path hidden by unavailable
  directory enumeration, and a possible future `unavailable` selector state, are
  recorded as `CLI-EDGE-008`. ID resolution is exact and noninteractive. A paired
  overwrite path resolves to its base logical source; an orphan overwrite selector
  is unsupported.
- Catalogue issues map as follows: root missing or unavailable to
  `workspace-unavailable`; root unsafe to `workspace-unsafe`; directory or
  candidate unavailability to `inspection-unavailable`; candidate unsafe to
  `candidate-unsafe`; identity unavailable and orphan overwrite to
  `layer-unresolved`; and ID collision to `identity-collision`. Contained
  physical aliases remain distinct safe logical sources and create no Find
  finding by themselves. Cancellation forms `interrupted`. The maintainer-
  accepted `layer-unresolved` reuse for an identity-less recognized candidate,
  and a possible future dedicated finding, are recorded as `CLI-EDGE-009`.
- Selected logical sources are inspected by source ID, base-path tie-break, then
  base and overwrite. Read verification maps unsafe to `candidate-unsafe`,
  missing, changed, unavailable, access, and I/O outcomes to
  `inspection-unavailable`, invalid UTF-8 to `invalid-encoding`, and cancellation
  to `interrupted`.
- A predicate-free inventory safely matches every effective logical source from
  its established identity even when a layer is unreadable; description and
  requested projections remain unavailable and coverage remains incomplete.
  Predicate-bearing searches emit a source only when available evidence proves
  the complete `all` or `any` requirement. Missing facts may prevent a match but
  never remove an independently proved safe match.
- Inspected count is the number of effective logical source units whose existing
  layers established every matching-required read, document boundary, and
  semantic-frontmatter fact. A source-less candidate, an incomplete layer, or
  unavailable required frontmatter is not inspected. Projection-only route or
  content unavailability does not reduce an otherwise established matching
  inspection count.
- A section includes its selecting heading. Zero matching headings in an
  inspected layer contributes no matching candidate and no finding. Several
  matching headings in one layer emit `section-ambiguous`, skip only that
  ambiguous region, and retain evidence from other unambiguous layers and
  sources.
- Evidence order is effective predicate, actual region with frontmatter before
  body/sections, base before overwrite, then source location. Evidence region is
  the actual frontmatter, body, or named-section origin even when `document`
  selected the union. Authored tag evidence omits the marker and retains case;
  authored heading evidence is the exact visible heading text.
- Projection never changes matching. Metadata is resolved once and only after at
  least one match needs it. The route-facts request receives the complete
  effective selection and the same reader. Loader metadata is available as
  unrouted. Ambiguous, unavailable, non-unique, or out-of-filter route support
  makes only the affected metadata projection unavailable.
- Frontmatter, headings, and body produce one projection per physical layer.
  Absent frontmatter is available empty text at a zero-length start-of-file
  location; an empty body and empty heading outline are likewise available.
  Metadata produces one logical projection. Each requested section produces one
  entry per layer: available for one match, missing for none after complete
  inspection, ambiguous for several, and unavailable when inspection could not
  establish it.
- Projection order is logical metadata first, then base and overwrite. Within a
  layer it is frontmatter, headings, body, then requested sections in document
  order; unavailable or missing sections without a document position use their
  first effective supplied order.
- `FindResultBuilder` applies the Interface finding-code order and all selector,
  source, layer, region, and location tie-breakers. It assigns match positions
  after ID/path ordering, recomputes matched count from safe matches, and emits
  exactly the contract next actions.
- Matching and projection coverage remain independent. Invalid uses not-started;
  a pre-universe blocker uses blocked; an unexpected failure uses failed;
  interruption marks only unfinished stages interrupted; candidate uncertainty
  makes matching incomplete; projection uncertainty makes projection incomplete;
  and a known missing section keeps complete projection coverage with attention.
  Content omission is always not-requested.
- Unexpected exceptions are translated to `find.operation-failed` without type,
  stack, or sensitive content. Caller cancellation is translated to
  `find.interrupted`. Both retain previously completed safe matches and bounded
  findings, but neither fabricates unfinished facts.

## Original Child 2 Phase And Evidence Plan (Historical)

This original plan ran through accepted Blue `685e2dd`. Its YAML exclusions and
Find/Markdown-only production scope describe that historical phase, not the active
shared-foundation correction below.

### Gray

Gray executable changes only the Core project reference and the exact neutral
Markdown and Find production contracts above. This Task's same-commit progress
and required active phase/review evidence remain aligned. All behavior
entrypoints throw named `NotSupportedException`. Gray contains no tests, YAML
model change, root composition, renderer, JSON graph, generated source, or domain
behavior. Verification is restore, exact-path format, Release build, package
resolution, diff/path audit, and targeted contract review.

#### Gray Result

- Baseline: exact focused Preflight commit `e24b9fe`; the production/source tree
  at that boundary remained exact `a1cbf09`.
- Changed production: one unversioned Core `Markdig` reference and 43 authored C#
  paths under the exact allowed Markdown and Find roots. No test, root
  composition, Shell, Route, YAML model/context, JSON, renderer, generated,
  package-version, project-graph, or unrelated path changed.
- Contract: The detached seven-option symbol graph, immutable request/result and
  operation models, six boundary delegates, pure collaborators, directly callable
  operation, and concrete factory wiring compile. Every domain behavior entrypoint
  throws a named `NotSupportedException`; no read, parse, match, projection, result
  policy, or workspace mutation is implemented.
- Modern C#: Maintainer-requested reinspection moved immutable facts to sealed
  records, retained invariant-bearing constructors, used primary constructors for
  trusted retained dependencies, used one required-init component model for the
  six independent delegates plus result builder, and kept explicit read-only
  snapshots. The correction also aligned model locality and removed redundant
  legacy-style construction without weakening validation.
- Verification: Locked restore passes. Release solution build passes with zero
  warnings and errors. Format apply and verify pass. Informational `CA1062`,
  `CA1510`, and `CA2264` analysis formats `0/417` files and reports no diagnostics.
  Core resolves Markdig requested `[1.3.2]` to exact `1.3.2`. Diff and exact
  changed-path audits pass.
- Review: The final bounded correctness review is `PASS` after correcting supplied
  versus effective values, source and layer identity, exact Markdown and section
  spans, coverage/status/finding/next invariants, projection cardinality and state,
  partial terminal facts, and deterministic ordering. The independent local-
  improvement review is `NO_MATERIAL_IMPROVEMENTS/PASS`; explicit constructors and
  read-only snapshots remain because they preserve invariant and collection
  semantics. The strongest counterargument is that Gray has no executable behavior
  tests. That is the intentional phase boundary; Red must now compile and reach
  only the named stubs. A Red-discovered callable contradiction returns to Gray.
- Execution evidence: Three bounded Gray implementation attempts reached their
  execution limit before completing the full packet or its final validation. The
  Mastermind inspected the partial artifacts, applied the accepted corrections,
  and reproduced every gate. The strongest future alternative is to split a Gray
  assignment into neutral document models, Find result models, and wiring packets;
  that reduces per-assignment breadth but increases handoff and integration cost.
  This did not consume the Task's exceptional correction cycle because Gray was
  not accepted before the corrections.

### Red

Red changes only mirrored Unit and Integration tests, nearest-scope test fixtures,
and this Task's same-commit progress. Gray production and every callable/model
contract are frozen. Every test uses one readable display name, `Feature` equal to
`find-query` or `markdown-documents`, and its project-tier `Evidence` value.

The complete affected Red declaration matrix is:

| Tier and exact test class                                             | Frozen declarations                                                                                                                                                                                                                                                                                    |
| --------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Unit `Commands/Find/FindBindingRedTests`                              | `SymbolsExposeTheExactDetachedGrammar`; `BinderPreservesDefaultsAndSuppliedView`; `BinderPreservesCrossOptionPredicateOrderAcrossNativeForms`; `BinderRejectsMissingAndRepeatedSingletonValues`; `WorkspaceFailurePreservesValuesLocalInvalidPrecedenceAndUnavailableFinding`                          |
| Unit `Commands/Find/Shared/Query/FindQueryParserRedTests`             | `TagGrammarAndEquivalentPredicatesUseFirstOccurrence`; `RequireAllAndAnyRemainFlatAndDependencyChecked`; `EscapedRegionAndContentListsPreserveSuppliedAndEffectiveOrder`; `RegionCompatibilityAndSubsumptionAreExact`                                                                                  |
| Unit `Framework/Documents/Markdown/MarkdownDocumentParserRedTests`    | `PipelineIsCachedPreciseAndExtensionFree`; `FrontmatterBoundariesPreserveLfCrLfMissingAndUnterminatedInput`; `AtxAndSetextHeadingsPreserveLevelFormAndCanonicality`; `VisibleHeadingTextUsesOnlyReaderVisibleInlineContent`; `SectionsAndVisibleLiteralSpansPreserveDocumentBoundaries`                |
| Unit `Commands/Find/Shared/Documents/Utf8SourceMapRedTests`           | `LocationsUseUnicodeScalarsAndUtf8BytesAcrossCrLf`; `ZeroLengthAndEndOfFileLocationsRemainValid`                                                                                                                                                                                                       |
| Unit `Commands/Find/Shared/Documents/FindFrontmatterReaderRedTests`   | `BlockAndFlowTagSequencesRetainSourceOrderAndMarks`; `QuotedEscapedUnicodeTagsCorrelateSemanticValuesAndScalarSpans`; `MissingEmptyCommentsAndUnknownFieldsRemainComplete`; `MalformedWrongDuplicateAndAliasShapesBecomeUnavailableWithoutGuessing`                                                    |
| Unit `Commands/Find/Shared/Matching/FindBodyTagScannerRedTests`       | `VisibleBareTagsUseWholeUnicodeTokenGrammar`; `CodeHtmlDestinationsEscapesEntitiesAndGeneratedEntriesDoNotMatch`                                                                                                                                                                                       |
| Unit `Commands/Find/Shared/Selection/FindUniverseResolverRedTests`    | `SelectorsPreserveFormsKindsExpansionsAndTypedOutcomes`; `IncludeExcludeAlgebraIsDeduplicatedOrderIndependentAndExclusionFirst`; `EffectiveSelectionProjectsOnlyInScopeCandidatesAndIssues`; `CatalogueIssuesMapToExactFindFindingsAndCandidateCounts`                                                 |
| Unit `Commands/Find/Shared/Matching/FindMatcherRedTests`              | `AllAndAnyAggregateEvidenceAcrossOrderedLayers`; `NaturalExplicitDocumentBodyAndSectionRegionsAreExact`; `MissingAndAmbiguousSectionsRetainOnlySafeEvidence`; `OrdinalIgnoreCaseMatchingIsCultureIndependentWithoutUnicodeNormalization`; `BareAndPredicateSearchesRetainOnlyIndependentlySafeMatches` |
| Unit `Commands/Find/Shared/Projection/FindProjectionBuilderRedTests`  | `RequestedPartsProjectExactLogicalAndLayerPayloads`; `SectionsDistinguishAvailableMissingAmbiguousAndUnavailable`; `MetadataUsesOnlyEffectiveRouteFactsAndDoesNotInferScope`; `ProjectionOrderIsCanonicalAndIndependentOfMatching`                                                                     |
| Unit `Commands/Find/Shared/Result/FindResultBuilderRedTests`          | `EveryFindingCodeMapsToItsFixedStatusAndOrder`; `CoverageCountsAndPayloadInvariantsRemainExact`; `StatusPrecedenceAndEveryNextActionMatchTheInterface`; `FailedInterruptedAndIncompleteResultsRetainSafeFacts`                                                                                         |
| Unit `Commands/Find/FindOperationRedTests`                            | `OperationReadsOneBoundaryAndFormsUniverseBeforeLayers`; `OperationSkipsRouteFactsUnlessMatchedMetadataIsRequested`; `OperationRetainsSafeMatchesAcrossDeterministicMidCancellation`; `OperationConvertsUnexpectedBoundaryFailureWithoutLeakingExceptionIdentity`                                      |
| Integration `Commands/Find/FindParserIntegrationRedTests`             | `PinnedParserPreservesNativeFormsOccurrencesCrossOptionOrderAndMissingValues`                                                                                                                                                                                                                          |
| Integration `Commands/Find/FindSourceUniverseIntegrationRedTests`     | `RealWorkspaceEnumeratesEveryEligibleFormAndAppliesPhysicalFilterExpansion`; `ExcludedUnreadableAreasRemainUnreadWhileUnsafeAndOrphanCandidatesStayVisible`                                                                                                                                            |
| Integration `Commands/Find/FindDocumentInspectionIntegrationRedTests` | `RealYamlMarkdownUnicodeCrLfAndOverwriteLayersProduceExactEvidenceAndLocations`; `InvalidEncodingChangedMissingAndUnavailableLayersProduceTypedIncompleteFacts`                                                                                                                                        |
| Integration `Commands/Find/FindOperationIntegrationRedTests`          | `DirectOperationCoversBarePredicateProjectionZeroAndAttentionResultsWithoutWrites`; `RepeatedAndPreCancelledInvocationsRemainDeterministicFreshAndReadOnly`                                                                                                                                            |

These 50 declarations use theories for approximately 90 to 110 executable cases.
Red records the actual pass/fail/skip counts after execution. Intended failures
must reach only the named Gray stubs; model, definition, and symbol contract cases
may already pass. No EndToEnd evidence belongs to Red.

#### Red Result

- Red contains the exact 50 frozen declarations in 15 classes: 43 Unit
  declarations and seven real-OS Integration declarations. The final bounded
  matrix is exactly 110 executable cases: Unit `90` and Integration `20`, with no
  EndToEnd evidence and zero skips.
- Focused Unit is `90` total: two definition/pipeline contract cases pass and `88`
  cases fail only because the named Gray binding, query, Markdown, UTF-8,
  frontmatter, body-tag, universe, matching, projection, result, or operation stub
  throws `NotSupportedException`. Focused Integration is `20` total: all eight
  pinned-parser contract cases pass and `12` cases fail only at the named Gray
  universe, inspection, or operation stubs.
- Red exposed one Gray callable-construction contradiction: the four repeatable
  option declarations omitted the required nonempty value names, so the actual
  `FindSymbols.Create()` graph failed during `FindDefinitions` initialization.
  The exceptional correction added only the accepted `source-reference`, `tag`,
  and `heading` value names. Unit and Integration evidence use the actual detached
  symbols and directly assert all seven declaration value names; no shadow symbol
  graph remains.
- The reviewed evidence retains supplied selector duplicates while deduplicating
  effective sources, uses segment-safe physical containment, distinct
  post-first-source cancellation, physically coherent Markdown spans, literal
  projection order, shuffled finding order, combined status precedence, every
  fixed next action, and separate incomplete, failed, and interrupted safe-fact
  retention. Real-OS fixtures own their workspaces and preserve no-write
  assertions for Green execution.
- Format apply and verification pass. The Release solution build passes with zero
  warnings and errors. Fresh correctness review is `PASS`; the final material
  improvement finding added direct duplicate-selector reporting assertions without
  increasing the frozen case count. Gray behavior, public composition, renderers,
  JSON, generated content, package versions, project graph, Shell, Route, and
  unrelated commands remain unchanged.

#### Exceptional Gray And Red Correction Result

- The first integrated Green run exposed three contradictions in accepted Gray
  and Red evidence rather than Green behavior. The LF and CRLF frontmatter
  constants ended their bodies before the source ended; matcher fixtures
  normalized paths beneath `.agents/` while two declarations asserted paths
  without that prefix; and result validation rejected a failed or interrupted
  terminal event after matching had completed when projection was correctly not
  requested.
- The one available exceptional correction cycle changed only those exact
  boundaries. Markdown expectations now use source-contained delimiter, YAML,
  and body spans; matcher expectations now use the fixture's canonical source
  paths; and result validation permits complete or incomplete matching with
  `NotRequested` projection only when content was not requested. No declaration,
  case count, fixture topology, status, coverage meaning, or Green implementation
  expectation changed.
- Release build and diff checks pass. Focused Unit improved to `76/90`; all `14`
  residual failures reach only the named binder, workspace-result, or operation
  Gray stubs. Targeted correctness review is `PASS`; the local-improvement review
  produced only explicit boolean grouping, which was applied. The exceptional
  correction cycle is consumed.

#### Maintainer-Authorized Additional Red Correction

- After the Green entrypoints were implemented, the sole remaining Unit failure
  asserted that an otherwise matched source disappeared when content projection
  was omitted. That assertion contradicted the accepted projection-independent
  matching rule and the real-workspace bare-inventory scenario, which retains
  matches with `NotRequested` projection coverage.
- From exact correction baseline `c0af2d1`, the maintainer explicitly authorized
  one additional narrow Red correction after the exceptional cycle was consumed.
  The affected operation test now asserts one safe match in both theory cases
  while independently requiring zero route-facts calls without metadata and one
  call with metadata. Only that match-count assertion changed; declarations, case
  count, fixtures, production contract, and all other expectations remain
  unchanged.
- Release build passes with zero warnings and errors. Focused Unit is `90/90` and
  focused Integration is `20/20`, both with zero skips.

### Green, Blue, Purple, And Acceptance

- Green changes only the frozen production behavior and this Task's progress. It
  does not change Red expectations, fixtures, traits, or snapshots. It makes the
  complete focused Red packet and affected Source/Route regressions pass.
- Blue performs one production-only material structure pass over the changed Find
  and Markdown paths. It may report no justified change. It cannot change the
  contract or tests.
- Purple performs one test-only material evidence pass. It may report no justified
  change. It cannot change production behavior or expectation meaning.
- The directly callable integrated scenario creates one owned real workspace with
  Loader-rooted and unrouted sources plus a base/overwrite pair, filters by an
  entrypoint scope, satisfies a tag and heading across layers, requests metadata,
  headings, and an exact section, invokes `FindOperation.ExecuteAsync` once, and
  proves the complete typed result and unchanged workspace bytes. It is the child
  integration scenario, not a public process claim.
- Final Child 2 acceptance runs the complete managed suite and current published
  process regressions once after the final executable change. Public Find
  registration, Find EndToEnd cases, integrated Markdig trimming/Native AOT
  execution, concrete JSON source generation, and the final no-write public gate
  remain Child 3 responsibilities.
- The one exceptional correction cycle was consumed by the recorded Gray and Red
  correction. The maintainer separately authorized the one additional narrow Red
  assertion correction recorded above. Blue, Purple, review, and acceptance
  cannot conceal an earlier-phase defect.

#### Green Result

- From exact corrected Red baseline `ebbd9d0`, Green changes exactly 14 production
  files under the frozen Find and Markdown roots. It implements binding and
  recoverable invalid results, query parsing, one concrete operation boundary,
  universe formation, document inspection, matching, projection, aggregate result
  construction, and factory wiring. Tests, contracts, packages, projects, Shell,
  Route, root composition, JSON, generated content, and presentation remain
  unchanged.
- The final implementation resolves the effective universe before selected-layer
  reads, preserves parser-owned predicate order and canonical source identity,
  correlates strict YAML and Markdown source facts, scans only visible body text,
  matches across base and overwrite layers, requests route facts only for matched
  metadata, keeps matching and projection coverage independent, and translates
  cancellation or unexpected failures without leaking exception identity or
  discarding independently safe facts. No workspace write path is introduced.
- Green review found and corrected unavailable Markdown boundaries counted as
  complete bare inspections, projection-only section ambiguity lowering matching
  coverage, repeated insertion of the same inspection finding, and explicit
  `open-forge: null` being treated as an absent mapping. Two fresh bounded
  correctness reviews then passed the boundary/document and orchestration/stage
  subsets with no remaining finding.
- Release build and format verification pass with zero warnings or errors. Focused
  Unit is `90/90`; focused Integration is `20/20`; affected Source/Route Unit
  regressions are `562/562`; and affected Source/Route Integration regressions are
  `183/183`, all with zero skips. `git diff --check` passes.

#### Blue Result

- From exact Green commit `02b5f7a`, Blue changes six of the accepted production
  files and no test or contract. Markdown block facts are collected in one
  traversal; the initial operation cancellation branch is flattened; unused
  builder arguments are removed; the identical pre-operation result envelope is
  centralized; and parsed selector kinds plus identical physical-state mappings
  are reused instead of recomputed.
- Release build and format verification pass with zero warnings or errors.
  Focused Unit remains `90/90` and focused Integration remains `20/20`, both with
  zero skips. `git diff --check` passes.
- Fresh correctness review is `PASS`: block ordering, cancellation outcomes,
  result inputs, projection/result ownership, and selector resolution remain
  unchanged. The final local-improvement review is
  `NO_MATERIAL_IMPROVEMENTS/PASS`; minor requested-part caching is not material,
  and the behaviorally non-equivalent UTF-8 consolidation remains deferred.

## Shared YAML And Frontmatter Architecture Correction

- During Purple, the maintainer required the top-down architecture perspective to
  distinguish feature-local semantics from neutral mechanical foundations. The
  review found that `FindFrontmatterReader` owned a generic YamlDotNet event tree
  while `RouteMetadataParser` separately extracted frontmatter, despite accepted
  Find, Route discovery, and later Route mutation consumers. Purple was isolated
  before acceptance, and this correction returned to Architecture rather than
  hiding the defect in test-only work.
- `MarkdownFrontmatterParser.Parse(string)` becomes the single exact delimiter,
  inner-YAML-span, and body-start boundary. `MarkdownDocumentParser` and
  `RouteMetadataParser` consume it; only the former invokes Markdig for body facts.
- `YamlDocumentParser.Parse(string)` returns one `YamlDocumentFacts` with the exact
  source, `Complete` or `Unavailable` syntax state, optional root, and recursive
  alias plus unsupported-mapping facts. Neutral nodes preserve kind, decoded
  scalar value, ordered sequence or mapping children, and half-open UTF-16 spans.
  Unsupported mapping facts identify duplicate scalar keys or non-scalar keys;
  they do not make the parser invent command status.
- `FindFrontmatterReader` retains all Find semantic decisions and uses the shared
  scalar facts for source correlation. `RouteMetadataParser` retains all Route
  required-field, tag, malformed/missing, and source-flag policy while consuming
  the shared boundary and syntax state. `CliYamlContext` and metadata DTOs remain
  unchanged.
- The correction uses a bounded Gray → Red → Green → Blue sequence. Gray freezes
  only the new Framework callable/model surface. Red directly proves boundaries,
  shape, spans, malformed syntax, aliases, and unsupported mappings. Green moves
  the parser mechanics and both consumers. Blue may simplify only within the
  corrected shared boundary. Original Child 2 behavior evidence and the isolated
  Purple packet retain their meaning; full acceptance follows the final executable
  change.

### Correction Gray Result

- From exact prior Blue `685e2dd`, Gray adds only the neutral
  `MarkdownFrontmatterParser`, `YamlDocumentParser`, `YamlDocumentFacts`,
  `YamlNode`, `YamlScalar`, `YamlMappingEntry`, and half-open `YamlTextSpan`
  callable/model surface. Both parser entrypoints throw named
  `NotSupportedException`; no parsing, consumer migration, command behavior, test,
  package, project, composition, serialization, or generated change exists.
- YAML child collections are immutable validated snapshots. Document construction
  recursively proves every node and scalar span is contained by the exact source
  and parent while retaining a complete empty document with no root.
- Release build passes with zero warnings or errors. Format verification and
  `git diff --check` pass. Fresh correctness review passes after the immutable
  snapshot, null-child, and span-containment corrections; final local-improvement
  review is `NO_MATERIAL_IMPROVEMENTS/PASS`, and final writing review passes.

### Correction Red Result

- From exact correction Gray `2cae7a4`, Red adds two mirrored Framework Unit test
  classes and no production change. Six declarations produce seven executable
  cases: exact LF/CRLF/missing/unterminated frontmatter boundaries; ordered YAML
  mapping and sequence shape; decoded scalars with quoted source spans; aliases;
  duplicate and non-scalar keys; empty, malformed, and multiple documents; and
  immutable/null/span model invariants.
- Release build passes with zero warnings or errors. Focused Unit is `97` total:
  `91` pass, including the direct model-invariant case and all original Find and
  Markdown behavior, while `6` fail only at the two named correction Gray parser
  stubs. There are zero skips. No Integration or EndToEnd evidence belongs to this
  correction Red packet.

### Correction Green Result

- From exact correction Red `a673ebe`, Green changes only five production files.
  `MarkdownFrontmatterParser` now owns the exact delimiter/YAML/body boundary and
  `MarkdownDocumentParser` consumes it. `YamlDocumentParser` owns one YamlDotNet
  event traversal that returns neutral ordered nodes, decoded scalars, exact
  source spans, aliases, and unsupported-mapping facts. Find removes its local
  event tree while retaining semantic tag correlation and location policy. Route
  removes duplicate frontmatter extraction while retaining its semantic metadata
  policy and source flags.
- The first focused run exposed one Green implementation omission: YamlDotNet
  represents an empty stream without a `DocumentStart`. The shared parser now
  returns a complete empty document for that standard event sequence. No Red
  expectation, callable contract, or command behavior changed.
- Release build and format verification pass with zero warnings or errors.
  Focused Unit is `97/97`; Find Integration is `20/20`; affected Source/Route Unit
  is `562/562`; and affected Source/Route Integration is `183/183`, all with zero
  skips. `git diff --check` passes.
- Fresh correctness review is `PASS`: event-span math, empty/malformed/multiple
  documents, exact frontmatter boundaries, exception containment, Find locality,
  and Route semantic parity satisfy the accepted correction. Final local-
  improvement review is `NO_MATERIAL_IMPROVEMENTS/PASS`; a second state-only YAML
  API, duplicate policy helper, source-map merge, and cached one-use recursive
  facts would add scope or complexity without material benefit.

### Correction Blue Result

- From exact correction Green `d4701ad`, Blue inspected the five changed
  production files and made no production change. The shared event traversal,
  exact frontmatter boundary, immutable neutral facts, and command-local semantic
  readers are already at the smallest clear structure supported by the accepted
  contracts and evidence.
- The final local-improvement review is
  `NO_MATERIAL_IMPROVEMENTS/PASS`. A state-only YAML parser would widen the frozen
  callable surface; factoring two short Route gates would add indirection;
  consolidating Find source mapping would change validation semantics; and caching
  one-use recursive flags would add state without material benefit.
- Because Blue changes no executable artifact, correction Green's final Release
  build, format, focused `97/97` Unit, `20/20` Find Integration, `562/562`
  Source/Route Unit, and `183/183` Source/Route Integration evidence remains the
  exact executable boundary.

### Corrected Purple Result

- From exact correction Blue `0006915`, Purple changes four Unit test files and no
  production or expectation meaning. It adds two executable Find cases and
  strengthens existing direct evidence for absent versus explicit-null
  `open-forge`, unavailable Markdown-boundary safe matching, independent matching
  and projection coverage, same-instance finding deduplication, exact flow-mapping
  and block-sequence YAML spans, and comment-only YAML completeness.
- Release build and format verification pass with zero warnings or errors.
  Focused Unit is `99/99` and Find Integration remains `20/20`, both with zero
  skips. `git diff --check` passes.
- Fresh correctness review is `PASS`; every addition covers a reviewed Green or
  shared-parser risk without coupling to command presentation. A local-improvement
  review suggested replacing `Assert.Same` with semantic fields, but that would
  weaken the exact same-instance aggregation regression: distinct equal findings
  are not the behavior corrected in Green. The targeted identity assertion is
  retained.

### Final Review Supplemental Red

- Final integrated production review found that `FindFrontmatterReader` made an
  empty or whitespace authored tag scalar unavailable, while the frozen document
  decision requires every scalar outside the accepted tag grammar to be ignored
  for matching. Missing and empty tags must remain complete zero-tag facts.
- Supplemental Red strengthens the existing
  `MissingEmptyCommentsAndUnknownFieldsRemainComplete` declaration with empty,
  whitespace, leading-digit, and repeated-hyphen scalar values. It changes no
  declaration or case count and no production file.
- Executing the first supplement exposed inherited Unit and Integration
  expectations that retained emoji-containing scalars as tag occurrences even
  though they are outside the same accepted grammar. The corrected declarations
  retain decoded Unicode-letter and exact scalar-span evidence while requiring the
  invalid scalars to be ignored.
- Release build and format verification pass with zero warnings or errors.
  Focused Unit is `99` total with `97` passing and two expected failures at the
  incorrect availability and invalid-occurrence branches; there are zero skips.
  Focused Integration is `20` total with `19` passing and one expected invalid-
  occurrence failure; there are zero skips. The first supplemental Red is accepted
  at exact `c72dd5e`, its Unit expectation correction at exact `d494adb`, and its
  Integration correction at exact `23fe39a`. Corrected Green must
  centralize the already identical Find query/authored tag grammar and ignore
  invalid authored values without importing Route policy.

### Final Review Corrected Green Result

- `FindTagGrammar` now owns the one Find-local Unicode-letter/digit and single-
  internal-ASCII-hyphen grammar proven identical for query validation and authored
  frontmatter filtering. It uses direct rune enumeration without a per-tag array.
- `FindFrontmatterReader` still establishes structural shape, count, and decoded-
  value correlation before applying the grammar. Invalid correlated scalar values
  are omitted; structural or semantic/event disagreement remains unavailable.
- `FindQueryParser` consumes the same grammar after optional leading-`#`
  normalization. Route policy and the neutral YAML parser remain unchanged.
- Release build is warning-free; format and selected analyzer verification are
  clean. Focused Unit is `99/99`, Find Integration `20/20`, affected Source/Route
  Unit `562/562`, and affected Source/Route Integration `183/183`, all with zero
  skips. Correctness review passes; its local-improvement partner's sole rune-array
  allocation finding was applied, and fresh correctness review passes the resulting
  exact diff.

### Final Acceptance Result

- Corrected Green is accepted at exact `2337d62`. The final evidence-improvement
  review requested one valid–invalid–valid authored-tag case; the existing Unit
  declaration now proves both surviving tags retain source order and exact scalar
  locations across an ignored middle scalar. Focused and full Unit remain `99/99`
  and `716/716`, and a fresh evidence review passes.
- The final locked restore succeeds. Release build is warning-free; format and the
  selected CA1062/CA1510/CA2264 analyzer gate are clean. Full Integration is
  `261/261`; managed published EndToEnd is `57/57`; all runs have zero skips. The
  transitive vulnerability report lists no vulnerable package for any project.
- Final correction review from corrected Purple `2d10474` through corrected Green
  `2337d62` passes and confirms the expectation changes strengthen the frozen
  invalid-scalar rule. Its sole evidence-ordering improvement is applied and
  reviewed. `git diff --check` and protected-boundary inspection pass.
- Child 2 remains directly callable only. Root registration, public rendering,
  concrete JSON source generation, Native AOT, and public Find no-write evidence
  remain Child 3 responsibilities.

## Active Toolchain And Commands

Run every .NET command from `src/cli/`. Red and later use the independently
runnable project paths below; filters refine project tiers and never replace them.

```bash
dotnet restore OpenForge.Cli.slnx --locked-mode
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore --nologo
dotnet format OpenForge.Cli.slnx --no-restore --verify-no-changes
dotnet test --project tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~Framework.Documents.Markdown|FullyQualifiedName~Framework.Documents.Yaml|FullyQualifiedName~Commands.Find"
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~Commands.Find"
dotnet test --project tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~Framework.Sources|FullyQualifiedName~Commands.Route.Shared|FullyQualifiedName~Commands.Route.List|FullyQualifiedName~Commands.Route.Inspect"
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-progress --filter "FullyQualifiedName~Framework.Sources|FullyQualifiedName~Commands.Route.List|FullyQualifiedName~Commands.Route.Inspect"
```

The final managed gate adds:

```bash
dotnet format OpenForge.Cli.slnx analyzers --no-restore --verify-no-changes --severity info --diagnostics CA1062 CA1510 CA2264 --verbosity diagnostic
dotnet test --project tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-build --no-progress
dotnet test --project tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-progress
dotnet publish root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --no-restore --runtime win-x64 -p:PublishAot=false
OPEN_FORGE_CLI_PATH="$(pwd)/artifacts/publish/OpenForge.Cli/release_win-x64/OpenForge.Cli.exe" OPEN_FORGE_CLI_EXPECTED_VERSION="0.0.0-dev" dotnet test --project tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-build --no-progress
dotnet package list --project OpenForge.Cli.slnx --vulnerable --include-transitive --format json --no-restore
```

From the repository root, every phase also runs `git diff --check`, exact changed-
path and protected-surface audits, and checks that generated regions, contracts,
Shell, Route output, root composition, `CliJsonContext`, package versions, and
project graph remain unchanged. Working Memory prose receives direct Mastermind
inspection; a separate Writing Reviewer is not required. Consequential Gray and
later code receives targeted correctness and local-improvement review against its
exact changed paths.

## Residual Risks And Closed Alternatives

- The parser-token occurrence adapter is the only accepted local accommodation
  for cross-option order. Raw-process rescanning, Shell mutation, a second parser,
  and loss of public predicate order are rejected.
- Markdig's package metadata does not itself prove trimming or Native AOT. The
  successful external probe reduces Preflight risk; Child 3 still owns actual
  repository-integrated Markdig AOT execution and stops if it diverges.
- Deterministic mid-operation cancellation is proved through the accepted
  selected-layer seam. Real-OS Integration proves pre-cancellation and fresh
  invocation without a timing race; it does not claim deterministic
  mid-enumeration cancellation.
- Find-local body scanning, UTF-8 mapping, generated-region handling, and semantic
  tag correlation remain local. Neutral Markdown frontmatter and YAML syntax are
  shared because Route discovery and accepted Route mutation require the same
  mechanical facts. A universal document/query engine and promotion of Find or
  Route semantic policy remain rejected.
- No product decision remains. The completed shared-foundation correction began
  from exact prior Blue `685e2dd`; any need to
  change a product contract, shared source contract, package version, Shell,
  public schema, or Child 3 surface triggers the existing stop conditions. The
  maintainer explicitly accepted the current `invalid` and `unsupported` selector
  state reuse and the `layer-unresolved` identity-failure reuse. `CLI-EDGE-008`
  and `CLI-EDGE-009` preserve the deferred improvements without changing Child 2
  acceptance.

## Stop Conditions

- Stop and return to the parent if Markdig trimming or Native AOT cannot meet the
  accepted CommonMark and source-location semantics.
- Stop if a second Markdown pipeline, dynamic Markdig configuration, competing
  shared document/query engine, persistent state, fuzzy or ranked matching, or
  Route metadata policy is proposed.
- Stop if effective-universe formation occurs after selected-layer reads, if a
  renderer must rerun the operation, or if the exact Interface result model,
  findings, status, coverage, or cancellation meaning cannot be preserved.

## Progress And Completion

- Prior accepted Child 2 history: Focused Preflight is accepted at exact `e24b9fe`, the strict
  production-only Gray contract is accepted at exact `8cef8f7`, and the complete
  reviewed Red packet is accepted at exact `571ee87`. The one exceptional Gray
  and Red correction is accepted at exact `c0af2d1`; the maintainer-authorized
  additional Red correction is accepted at exact `ebbd9d0`, and the complete
  production-only Green implementation is accepted at exact `02b5f7a`. The
  production-only Blue pass is accepted at exact `685e2dd`. Focused and affected
  regressions passed before Purple exposed the shared-parser architecture defect
  and was isolated without acceptance.
- Current downstream Child 3 boundary: Gray is accepted at exact `2cae7a4`, Red at
  exact `a673ebe`, Green at exact `d4701ad`, Blue at exact `0006915`, and Purple at
  exact `2d10474`. Final-review supplemental Red is accepted at exact `c72dd5e`,
  and its Unit evidence correction at exact `d494adb` and Integration correction
  at exact `23fe39a`. Corrected Green is accepted at exact `2337d62`; final
  evidence and Child 2 acceptance are recorded at exact `ff7ce3f` (`Accept Find
query operation`). Child 3 focused Preflight is accepted at exact `28d316a`,
  Gray at exact `a76a217`, original Red at exact `22d3bff`, metadata corrections
  at exact `6a9a0de` and `eea3d59`, and supplemental escaping Red correction at
  exact `a865fd1`. The post-commit Red reproduction from exact `eea3d59` succeeded
  as intentional Red: managed non-AOT `win-x64` publish passed, and published
  Find EndToEnd was `13` total with `13` intentional failures and zero skips, all
  terminating at absent Green root registration. The corrected Red Unit boundary
  is `206` total with `92` pass and `114` intentional Gray-boundary failures,
  zero skips. Child 3 Green is accepted at exact `cb7874c` (`Implement Find
presentation`) over corrected Red `a865fd1`; it is production/root-
  composition-only and adds no test, support, contract, project, package,
  configuration, generated-routing, or Route behavior change. Fresh Green
  evidence passes focused Find/direct-root Unit `206/206`, generated-
  serialization Integration `43/43`, affected Shell+Route Unit `347/347`,
  affected Shell+Route Integration `160/160`, managed non-AOT publish, and
  published Find EndToEnd `13/13`, all with zero skips. Fresh final bounded
  correctness review is `PASS` with no material findings. Blue is accepted at exact
  `3f81e76` (`Simplify Find JSON projection`) after changing production structure only in
  `src/cli/core/OpenForge.Cli.Core/Commands/Find/Shared/Rendering/FindJsonProjection.cs`:
  it uses the canonical status and finding-code readers and removes the two
  duplicate private mapping methods. It changes no behavior, public
  output/order/schema, test/support, contracts, package/project/configuration,
  generated routing, Shell/root, Route, workspace-write, or Native AOT surfaces.
  Its focused evidence, warning-free build,
  format/diff checks, and bounded correctness/local-improvement reviews pass with
  zero skips. The correctness review confirms all 7 status and 17 finding-code
  mappings and undefined-value exception behavior, unchanged JSON model/property
  order/context, and source-generation/AOT-safe static readers. Fresh local
  improvement review is `APPROVED — NO_MATERIAL_IMPROVEMENTS`. No managed
  republish or Native AOT claim is needed. Explicit no-op Purple acceptance is
  recorded from that exact clean Blue with verdict `NO_MATERIAL_IMPROVEMENTS`;
  focused Unit `206/206`, focused Integration/serialization `43/43`, and
  published Find EndToEnd `13/13` pass with zero skips, and source diff/check is
  clean/empty. No test/support, production, contract, project, package,
  configuration, generated, Route, Shell, or root file changed, and no Purple
  code/test commit was manufactured. No Native AOT claim was made, and the
  record-only commit was not a test change. The no-op Purple acceptance is
  recorded at exact `426d4f5`. This does
  not change Child 2.
- Blocker: None for this accepted Child 2 Task or the accepted Find boundary.
  Child 3 and the Find parent are Complete and accepted in the commit containing
  this record update. The final managed/native gate and package, artifact, static,
  public no-write, and protected-surface audits passed. The Read-Only group and
  broader CLI program remain Active; References remains Planned.
- Next action: After the containing acceptance commit, freshly verify `develop`,
  squash-integrate the accepted `feature/cli-find` tip into local `develop`, commit
  that one squash, prove exact tree equality, do not push, and halt. Do not start
  References in this session.

This child is Complete and accepted at exact `ff7ce3f`. Its exact typed operation
and fixed Markdown facts remain the immutable result consumed by Child 3. Do not
register or present the public Find leaf here.
