---
open-forge:
  description: Explain the tag delimiter defect, inventory manual parsing across CLI commands, and propose actionable Doctor diagnostics
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Audit, Parsing, Doctor, Dogfood]
---

# CLI Parsing And Doctor Audit

## Current Reading Of This Audit

The delimiter, catalogue and Doctor correctness issues below were fixed in the
completed stages recorded in the sequential plan. Measurements describe their
named earlier baselines. P2–P6 are backlog by explicit user direction; the follow-up plan owns their queue. The temporary
parser probe and task artifact directories have been removed; no experimental
program was committed. Presentation analysis now preserves diagnostic kinds and
JSON, concentrating on understandable output and presentation-only grouping.

## Continuation Findings

The native-delimiter candidate `5bef9a24` preserves Doctor behavior. Fresh
Framework, Development, Toolkit and all-package installations were captured in
`artifacts/task27-native-delimiters/doctor-baseline/`. Each installation succeeds,
and Doctor leaves file hashes unchanged. Framework-only Doctor is `incomplete`;
all three Extension cases are `blocked`.

- T27-DOG05: Extension Install persists `source: "embedded catalogue"`, but
  `ExtensionSourceObservationReader` treats every non-null recorded source as an
  explicit path. Doctor therefore resolves that display identity beneath the
  selected workspace and reports a source-overlap boundary. Reconcile the reader
  with the actual persisted source identity; keep real explicit-source
  disjointness and unavailable-source checks strict.
- T30-DOG02 also occurs immediately after fresh package installation: generated
  Workflow, Pattern and Template host indexes differ from their old Framework
  generated-region baselines. Doctor reports managed changes and mixed lifecycle
  state despite the package operation's verified generated projection. Establish
  current generated navigation from its authoritative authored topology while
  preserving authored managed-file drift and unsafe-region detection. The
  [catalogue Task](extension-catalogue-synchronization.md) retains the related
  old Toolkit removal/reinstallation controls.

These are additional correctness inputs for the frozen Doctor stage. They are
not fixed by clearer messages, lower severity, or hidden findings. The original
baseline audit below remains historical evidence for its named executable.

## Result And Scope

The spaced `--tag` failure is our delimiter guard overriding System.CommandLine.
Route's equals-only policy leaks into Find because the shell collects policies
globally by option name. Markdown and YAML already use Markdig and YamlDotNet,
but several callers reconstruct syntax or discard facts and then parse them
again. The clearest additional targets are native Skill YAML properties and
Route Inspect's separate Markdown fence scanner.

Doctor's size comes primarily from repeated candidate lists and informational
reference findings. Its default view exposes implementation vocabulary. Fresh
installation also reveals incorrect or permanently incomplete diagnoses that
presentation changes cannot fix.

This is the report requested by the user, under the
[sequential plan](cli-dogfood-follow-up-plan.md). Proposed parser changes,
diagnostic defaults and machine-result changes are not yet implemented or
accepted by this report. The first fixture repair set is verified separately.

Baseline: worktree `<temp>/open-forge-cli-refactor-sequential`, production commit
`50a36f351086c43f649fd228fd2419c1946a6ddd`. The installed native CLI reports that
exact version. Its earlier baseline at `4c115f8e` produced the recorded Doctor
measurements; the intervening production commit changes only References helper
ownership and an Update recovery helper. No Doctor or parsing code changed.

## Fixture Repairs And Remaining Failures

The Update fixtures now construct earlier authored content explicitly instead
of replacing a sentence in the current Framework payload. The coalescing fixture
uses the shared Markdown parser to locate the machine-generated Entries region,
requires it to be populated, and replaces its content with an explicit empty
region. Deliberate authored text is inserted at the body start, leaving Entries
last. Every existing behavioral assertion is preserved.

The Integration project now copies all source Extension packages into its parity
fixture instead of only `development-toolkit`. The two strict catalogue tests
still fail correctly: the embedded archive and inventory contain the older
Toolkit package, while accepted source contains six packages. This is shipped
data drift, not an outdated expected result. Regenerating it also needs package
dependency and ownership-transition verification described by
[Extension Catalogue Synchronization](extension-catalogue-synchronization.md).

Final fixture verification: Release build has zero warnings/errors; Integration
has 1,722 passes and those two failures out of 1,724, with no skips. The preceding
full run passed all 3,231 Unit tests. Independently executed public CLI tests
passed all 111 cases after the normal wrapper stopped at Integration. Delivery,
agent tooling and package-layout tests passed 16, 8 and 7 cases respectively.
The fixture changes do not affect those other test projects. Changed C# whitespace
verification passes. No production expectation was weakened to obtain a pass.

The [Observation](../../../emerging/observations/2026-09-12_brittle-markdown-test-fixtures.md)
records the user's snapshot preference: use explicit behavioral fixtures and
reviewed snapshots where exact rendered wording is the subject. Preserve direct
state, safety and effect assertions. Catalogue parity must continue to compare
the actual shipped bytes against source.

## Tag Delimiters

The installed executable was exercised with the same Find selection:

| Syntax              | Result                                                    |
| ------------------- | --------------------------------------------------------- |
| `--tag Refactoring` | Exit 4, no JSON, `--tag requires the --tag=<value> form.` |
| `--tag=Refactoring` | Exit 0, complete JSON result                              |
| `--tag:Refactoring` | Exit 4, the same delimiter error                          |

Find help advertises the spaced form. A standalone probe against the pinned
System.CommandLine **2.0.11**, using Find's exact option arity and repeat settings,
accepts all three forms with zero errors and the same typed value. This agrees
with [Microsoft's documented native syntax](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax#option-argument-delimiters).

The concrete call path is:

1. `Commands/Route/{Create,Init,Update}/*Binding.cs` registers `--tag` with
   `CliDelimiterShape.Equals`; `Route/List/RouteListBinding.cs` similarly registers
   `--depth`.
2. `Shell/Parsing/CliRootDefinitionFactory.Create` concatenates all branch and
   leaf policies into one root collection, without the owning Command identity.
3. `CliCommandTree.Parse` returns that collection with every parse.
4. `CliTerminalValidator.Validate` calls `CliDelimiterGuard.Validate` over the
   original arguments. The guard rejects matching option spellings even when
   another command owns the selected option. Find defines no delimiter policy.

All four production registrations require equals. The guard's separate-token
mode has no production registration. It also rejects colon syntax. This is an
extra lexical parser, not a limitation in System.CommandLine.

**Recommended change P1:** remove the obsolete equals-only restrictions and the
unused policy plumbing, accepting the pinned library's three native forms.
The current CLI Implementation Directive already requires this default. The
Architecture still records Route Update and Route List exceptions; reconcile
those exact contracts and their rejection tests with the accepted change.
If an exception must remain, bind it to the selected Command identity; a global
option-name guard is incorrect even under the old exception.

Evidence for P1 must cover Find and all four Route registrations, repeated
options, empty/missing values, mixed tag/heading occurrence order, unknown
options, `--`, and help examples. Keep command semantic validation after parsing.
Include the published Linux executable because this affects the root shell.

## Parsing Inventory

Paths in the following tables are relative to
[`src/cli/core/OpenForge.Cli.Core/`](../../../../../src/cli/core/OpenForge.Cli.Core/).
The audit searched every production command and the shared document/source
capabilities, then inspected the parsing owners and their consumers. Ignored
receipts retain the 183 lexical candidate files, 169 consumer references and
85 command scan matches. Those are search inventories, not counts of defects.
Path splitting, output formatting, byte copying and typed enum validation are
distinguished from parsing document syntax.

### Parsing Owners And Proposed Changes

| Location                                                                                                                                                  | What it does                                                                                                              | Disposition                                                                                                                                                                                                                                               |
| --------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Shell/Parsing/CliDelimiterGuard.cs` and registration path above                                                                                          | Rescans raw arguments and rejects native delimiters                                                                       | P1: remove obsolete restrictions; resolve the recorded exceptions explicitly                                                                                                                                                                              |
| `Commands/References/Shared/Binding/ReferencesSelectorOccurrenceReader.cs`                                                                                | Reads library tokens to retain include/exclude order, but also manually looks for attached `=` and `:` values             | P2: use normalized parser-owned option/argument tokens and typed values; prove the attached-token fallback is unnecessary before removing it                                                                                                              |
| `Commands/Find/FindRequestBinder.cs`                                                                                                                      | Reads typed values, counts and token order to interleave tags and headings                                                | Retain required interleaving; simplify only if the pinned API exposes equivalent ordered occurrence facts. Token traversal alone is not a second tokenizer                                                                                                |
| `Shell/Parsing/CliOptionResultFactsReader.cs`                                                                                                             | Uses `OptionResult.IdentifierTokenCount` and library argument tokens                                                      | Keep: ordinary library adaptation                                                                                                                                                                                                                         |
| `Commands/Library/{Attach,Inspect,Sync,Detach}/Shared/Binding/*RequestBinder.cs` invalid-result factories                                                 | `ReadOperands` or `ReadAttemptedId` filters raw strings by their leading dash                                             | P6: replace this fallback with typed argument facts. These paths are exercised by direct invalid-result tests; the current root handles parser errors before reaching them. Do not claim a public misidentification without an executable reproduction    |
| `Commands/Find/Shared/Query/FindQueryParser.cs`, `Commands/Context/Shared/Binding/ContextRequestParser.cs`                                                | Interprets typed values such as section selectors and explicit comma-list choices                                         | Keep product-owned value grammar; System.CommandLine does not define these meanings                                                                                                                                                                       |
| `Framework/Documents/Markdown/MarkdownDocumentParser.cs`, `MarkdownInlineFactCollector.cs`, `MarkdownInlineTextReader.cs`, `MarkdownPipelineFactory.cs`   | Markdig AST, precise spans, headings, visible text, links and opaque code/HTML                                            | Keep as the shared library adapter. No independent general Markdown tokenizer found here                                                                                                                                                                  |
| `Framework/Documents/Markdown/MarkdownFrontmatterParser.cs`                                                                                               | Scans the exact leading `---` pair and retains body/YAML boundaries, including incomplete input                           | P5: evaluate Markdig's YAML frontmatter extension against the malformed-boundary and exact-span contract. Do not replace this finite boundary scanner merely because it has a loop                                                                        |
| `Framework/Documents/Yaml/YamlDocumentParser.cs`                                                                                                          | Uses YamlDotNet events to construct neutral nodes with spans, duplicates and alias presence                               | Keep the event parser. P3: retain more facts already supplied by events so consumers do not recover them from source text                                                                                                                                 |
| `Framework/Documents/Metadata/FrameworkDocumentMetadataParser.cs`, `FrameworkDocumentMetadataValueReader.cs`                                              | Selects Open Forge keys and validates product metadata over parsed YAML nodes                                             | Keep semantic validation and duplicate-key evidence. This does not tokenize YAML                                                                                                                                                                          |
| `Framework/Documents/Metadata/FrameworkDocumentMetadataEmitter.cs`, `Framework/Documents/Markdown/FrameworkMarkdownDocumentWriter.cs`                     | Source-generated YamlDotNet serialization and document assembly                                                           | Keep library serialization. This is writing, not a competing parser                                                                                                                                                                                       |
| `Framework/Sources/Metadata/SourceAuthoredMetadataParser.cs`                                                                                              | Native Skill handling manually recovers anchor names, aliases, explicit tags and null spellings from YAML source spans    | P3: carry `NodeEvent.Anchor`, `NodeEvent.Tag`, `AnchorAlias.Value` and `Scalar.Style` from YamlDotNet instead of rescanning `&`, `*`, `!` and property boundaries. Preserve accepted scalar/alias and duplicate semantics                                 |
| `Commands/Find/Shared/Documents/FindFrontmatterReader.cs`                                                                                                 | Open Forge metadata uses shared facts; Skill metadata first parses neutral YAML and then invokes a static deserializer    | P3 follow-up: unify reusable Skill syntax facts with the shared reader where semantics agree. Find rejects aliases; the shared Skill reader resolves some scalar aliases. Resolve that policy difference explicitly                                       |
| `Commands/Route/Inspect/Shared/Profile/RouteInspectMarkdownStructure.cs` and `RouteInspectAxiomsProfileBuilder.Parsing.cs`                                | Reimplements fenced-code tracking and splits raw lines to find `## Axioms`                                                | P4: consume shared Markdig heading/section/opaque facts. Preserve canonical heading, duplicate section, inherited sentinel and CRLF policies. Doctor already uses AST section facts through `RouteSourceStructureReader`                                  |
| `Framework/Documents/Markdown/MarkdownGeneratedRegionParser.cs`                                                                                           | Recognizes exact generated comments and the final canonical Entries section over parsed Markdown facts                    | Keep the product marker grammar and exact editing boundaries                                                                                                                                                                                              |
| `Commands/Find/Shared/Matching/FindBodyTagScanner.cs`                                                                                                     | Scans hashtags only in Markdig-visible text; independently recognizes generated marker pairs                              | Keep hashtag semantics. P4 follow-up: reconcile the looser marker-exclusion policy with shared region facts before sharing it; a valid pair and a valid final Entries section are currently different conditions                                          |
| `Framework/Sources/Loading/SourceGeneratedEntriesParser.cs`, `Framework/Sources/Routing/SourceLoaderEntriesParser.cs`, `SourceLoaderDeclarationParser.cs` | Splits canonical generated entry lines and reads link labels, destinations and tags                                       | P4 follow-up: consolidate their common canonical-entry recognizer. Preserve Loader's required tag suffix, root-destination rules and useful partial results; ordinary Entries allows no tag suffix. These are deliberately stricter than general Markdown |
| `Commands/Route/Inspect/Shared/Profile/RouteInspectGeneratedEntriesReader.cs`                                                                             | Reuses shared parsing, with another outer-line-shape check                                                                | Consolidate compatible shape checks with Loader handling after proving exact accepted differences                                                                                                                                                         |
| `Commands/Route/Update/Shared/Planning/RouteUpdateMetadataLayoutReader.cs`, `RouteUpdateMetadataEditPlanner.cs`, `RouteUpdateMetadataByteEditor.cs`       | Reads parsed YAML nodes, then checks line/spacing/flow style and edits exact source bytes                                 | Keep byte-preserving edits and refusal of unsupported layouts. P3 may replace raw flow/tag checks with retained event facts. A serialize-and-replace rewrite would lose comments and unrelated formatting                                                 |
| `Commands/References/Shared/Extraction/ReferencesLinkExtractor.cs`, Route Move/Remove reference scanners, Repair target reader                            | Consumes shared Markdown links and exact destination spans; splits destination fragments or reads an affected source line | Keep source-location and edit logic; reuse shared destination policies where they express the same operation. These do not discover links with a separate Markdown parser                                                                                 |
| `Framework/Sources/References/SourceLinkDestinationResolver.cs`, source identity/path helpers                                                             | Classifies local/external destinations, path forms, fragments and containment                                             | Keep product destination/path grammar and BCL URI/path operations; Markdown/YAML libraries do not own workspace containment                                                                                                                               |

The pinned YamlDotNet **18.1.0** XML API reference confirms those event fields;
the neutral adapter currently discards them. The pinned Markdig version is
**1.3.2**. Neither P3 nor P4 requires another dependency. Exact source-coordinate
and Native AOT evidence is required before changing these shared adapters.

### Command Coverage

| Commands                                                    | Document parsing path                                                                                                    |
| ----------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| `context`                                                   | Shared source reading, metadata, Loader/Entries and Markdown facts; local projection and typed selector grammar          |
| `find`                                                      | Shared Markdown/Open Forge metadata, command-local Skill reader and visible-body hashtag scanner                         |
| `references`                                                | Shared Markdown parser and destination resolution; local occurrence/filter projection                                    |
| `index`                                                     | Shared generated-navigation formation and Markdown region projection                                                     |
| `route list`                                                | Shared source metadata, topology and Loader declarations; extra `--depth` guard                                          |
| `route inspect`                                             | Shared source metadata and Entries; separate Axioms/fence reader identified above                                        |
| `route create`, `route init`                                | Shared YAML emitter, Markdown writer and generated navigation; extra `--tag` guards                                      |
| `route update`                                              | Shared Markdown/YAML syntax plus source-preserving metadata layout/edit logic; extra `--tag` guard                       |
| `route move`, `route remove`                                | Shared Markdown catalogue and link facts, destination planning and exact reference/line edits                            |
| `install`, `update`                                         | Shared embedded payload, metadata and generated navigation; lifecycle JSON and guarded byte effects                      |
| `status`, `doctor`                                          | Shared operational source/route/reference contributors; Doctor projects facts into findings, not another document parser |
| `repair`                                                    | Doctor-derived proposals, shared Markdown target reads and exact validated byte edits                                    |
| `cleanup`                                                   | Recovery records and filesystem validation; no independent YAML/Markdown syntax parser found                             |
| `extension list`, `extension inspect`, `extension create`   | JSON package manifests/catalogue; Inspect also uses shared content fingerprints; Create serializes JSON                  |
| `extension install`, `extension update`, `extension remove` | Shared source metadata, topology, generated navigation and fingerprints around JSON package/lifecycle records            |
| `library list`, `library inspect`                           | Library records/inventory and typed request values; no independent YAML/Markdown tokenizer found                         |
| `library attach`, `library sync`, `library detach`          | Shared Markdown/metadata/generated navigation for projected sources, with path and permission validation                 |

This is a source audit and a concrete tag reproduction. It is not a claim that
every proposed parser replacement has been behaviorally qualified. Each selected
change needs its own focused cases and immutable before/after boundary.

## Doctor Diagnosis

### Why The Output Is So Large

The repository result was **168,127,363 bytes** of indented JSON. Re-serializing
the local-reference domain without whitespace still yields **79,674,182 bytes**;
indentation is therefore only part of the problem.

There are 10,986 reference findings: 8,050 informational and 2,936 warnings.
The 591 missing destinations generate 22,794 candidate entries. Doctor then
attaches full candidate lists to the missing-target finding and to additional
findings for filename, title, literal-content, route-neighborhood and candidate
cardinality. Across those records there are **111,215 serialized candidate
entries**. The additional candidate records alone create **2,294 warnings** and
about **44.3 MB** of compact JSON. These describe possible fixes to existing
problems; they are not 2,294 independent broken links.

This follows directly from `LocalReferenceDoctorInspector.CreateFindings`,
`LocalReferenceDoctorCandidateInspector.Project` and
`LocalReferenceDoctorFindingFactory.Create`. Candidate lists are retained on
each emitted record. `LocalReferenceCandidateReader` also admits nearby route
sources as candidates, explaining the long parent/sibling lists in the user's
example. Separately, 1,686 informational cycle records occupy about 17.5 MB of
compact JSON, and 5,365 valid-target findings occupy about 5.2 MB.

`Shell/Definitions/CliSyntaxDefinitions.cs` sets the shared default to `expanded`.
`DoctorFindingHumanRenderer.Append` emits every finding and candidate path in
both views. Expanded adds each candidate's matching reasons, repeated source
coordinates, evidence and origin. The lowercase severity is buried in this
hierarchy. Compact omits the affected line coordinates along with extra detail,
even though a source path and line are among the most useful facts for repair.

### Why Fresh Installation Looks Broken

Install, repeat Install, Update and Status all complete in an isolated fresh
workspace. Doctor returns incomplete with one Extension error, twenty valid-link
informational findings, and route/Extension coverage limitations. Its Extension
domain simultaneously reports lifecycle `trusted` and an `extension.lifecycle-untrusted`
error. The source explains three separate issues:

1. **Absent optional Extensions become an error.**
   `ExtensionLifecycleDoctorReader.ReadSection` maps a complete lifecycle read to
   `Present`. `ExtensionLifecycleDoctorInspector.AddLifecycle` accepts only raw
   `LifecycleExtensionTrust.Trusted` for that branch; `Absent` falls into the
   untrusted error. In contrast, `ExtensionLifecycleEvaluation.ReadLifecycleState`
   correctly treats a complete read with no Extensions as trusted.
2. **Extension coverage is incomplete unconditionally.**
   `ExtensionObservationHorizonDoctorInspector.AddLimitations` always adds the
   unavailable manifest-scan limitation and lowers coverage. Fixing the false
   error alone cannot make fresh Doctor complete. This is an unfinished
   observation capability, not evidence that the user broke a file. Define when
   the scan is applicable, implement it where required, and retain truthful
   limits where it remains unavailable.
3. **Doctor projects generated navigation onto ordinary leaves.**
   `RouteDoctorGeneratedNavigationReader.Read` passes all `formation.Sources`
   into region projection. `GeneratedNavigationRegionPlanner` supports only the
   Loader and entrypoints; ordinary leaves become `RegionSourceUnsupported`,
   mapped to unavailable. `RouteDoctorInspector` lowers coverage for these but
   emits no individual finding, leaving only the opaque general limitation.
   Status instead selects expected region paths. Keep leaves as child metadata
   inputs, but select eligible region targets separately for Doctor.

These source paths explain the observed contradictions. Correctness changes
need fresh-install regression cases plus damaged-install controls; this audit
does not claim the fixes have run.

## Proposed Doctor Presentation

Apply the repository [Writing Standard](../../../crystallized/documents/maintenance/writing.md):
say what is wrong, where it is, and what the user can do. Keep severity and
uncertainty accurate. A broken destination should read approximately:

```text
WARNING  Broken link
.agents/memory/working/handoffs/2026-08-24_cli-find-query-red-start.md:44

The linked C# design file no longer exists.
Update or remove this link:
../../../directives/open-forge/csharp/csharp-design.md
```

The line/column are source-file coordinates, not memory locations. Present them
beside the path. Byte offsets remain useful machine/edit evidence but should not
dominate the human view. Replace `basis` with a concrete matching reason only
when candidates are requested; replace `provenance` with the source that
established the fact only when that distinction helps the user. Do not display
“bounded reference” as the explanation of a broken link.

**Recommended default:** concise actionable errors and warnings, grouped by
file, one primary issue per broken occurrence, with informational facts hidden.
Show counts and incomplete checks clearly. The user's errors-only idea remains
a valid alternative, but today it would hide all 591 broken-link warnings.
Do not solve that by silently relabeling warnings as errors. An errors-only view
must still report hidden warning counts and how to display them.

Keep candidate suggestions and technical evidence in explicit expanded detail.
Colour can be added later; labels, spacing and file grouping must work in plain
text. Formatting filters must not silently change health status or exit codes.
Deduplicating the public JSON candidate model is a separate reviewed schema
change; simply changing the human renderer would not reduce that machine payload.

## Recommended Next Sets

1. P1: restore native delimiter behavior, reconcile old exceptions, then verify
   and squash the isolated fix when its acceptance boundary is green.
2. P2: remove redundant attached-token handling after a pinned-parser token probe.
3. P3: retain YAML event properties and remove native Skill lexical recovery;
   separately resolve Find/shared Skill policy differences.
4. P4: replace Route Inspect's fence parser and consolidate compatible generated
   entry/marker policies. P5 remains an evaluation, not an automatic rewrite.
5. Doctor correctness: fix absent-Extension and region-target classification,
   then resolve the unconditional missing observation.
6. Doctor presentation: implement the reviewed default, concise messages and
   grouping; review any machine-schema deduplication separately. Colours follow.

The existing catalogue synchronization task remains the prerequisite for an
entirely green suite. The user requested only fully qualified sets on `develop`;
no merge is represented as clean while that boundary remains unresolved.

## Evidence Location

Ignored receipts are in `artifacts/task27-sequential-set1/`: full managed/public
and tooling logs, `fixture-integration-final.json`, `fixture-build-final.log`,
`fixture-format.log`, `parser-probe.log`, `installed-tag-reproduction.json`,
`doctor-size-analysis.json`, and the `dogfood/` command outputs. The 168 MB Doctor
capture is intentionally not tracked. The standalone parser probe was never committed and has now been removed.
