---
open-forge:
  description: Complete accepted current public surface and observable result for deterministic `find` discovery
  responsibility: Define what a caller may enter and observe from `find` without selecting implementation technology
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Find, Interface, Discovery, Tag, Heading, Filter, Projection, CurrentTruth]
---

# Find Interface Contract

## Status And Authority

This file is the current Crystallized authority for the caller-visible `find`
Interface Contract. The command is a current non-shipping contract and does not
claim executable behavior.

The [Shared Result Coordinates](../shared/result-coordinates/interface.md)
defines only the shared JSON envelope, source-location primitive, and
status/process coordinates. This Interface owns the exact command-local
`find` command-data object, its findings and finite values, and every `find`-specific
`next` value. The [Technical Design](technical-design.md) records accepted
implementation choices without changing this contract.

The current [Markdown syntax](../../../framework/markdown/syntax.md) defines
authored tags and canonical headings. The [Markdown compatibility
boundary](../../../framework/markdown/compatibility.md) distinguishes canonical
authored syntax from structural heading input accepted by deterministic readers.
The shared [CLI Source References](../shared/source-references/interface.md)
contract defines automatic source IDs, canonical paths, overwrite identity, and
result display. The shared [Global CLI Flags](../shared/global-flags/interface.md)
contract defines the global flags used here.

## Purpose

- `find` returns a deterministic flat inventory of Open Forge
  Markdown sources.
- Typed tag and heading predicates narrow that same inventory.
  They do not select hidden child operations.
- Bare `find` is a complete operation. Predicate flags filter
  one established result set rather than choosing a different operation.
- `find` answers which Open Forge Markdown sources exist in the
  selected workspace.
- `find` answers which sources have one or more authored tags.
- `find` answers which sources contain one or more structural
  headings.
- `find` answers which tagged sources also contain a required
  heading.
- `find` answers which sources satisfy any one of several
  explicit alternatives.
- Given the same workspace bytes, include and exclude selector
  occurrences, predicates, locations, and projections, `find` resolves the same
  effective source universe and returns the same candidates, matches, order,
  evidence, findings, and semantic result.

## Syntax And Inputs

### Exact Command Form

- The complete public command form is:

  ```text
  open-forge find
    [--include=<source-reference>]...
    [--exclude=<source-reference>]...
    [--tag=<tag>]...
    [--heading=<heading>]...
    [--require=all|any]
    [--within=<part>[,<part>...]]
    [--content=<part>[,<part>...]]
    [global flags]
  ```

- `find` has no positional operands. Every query value names
  the field or source-universe dimension it constrains, so source references,
  tag values, and heading values use named flags.
- The six shared global flags, `--workspace`, `--format json`,
  `--detail`, `--detail debug`, `--help`, and `--version`, apply to `find` under the shared
  [Global CLI Flags](../shared/global-flags/interface.md) contract. That contract supplies their
  complete spelling, grammar, defaults, repetition rules, terminal behavior,
  and errors; this command does not restate them.

### Operation-Specific Flags

| Flag                           | Role       | Accepted value                            | Omission                                                            | Repetition, ordering, and composition                                                                |
| ------------------------------ | ---------- | ----------------------------------------- | ------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| `--include=<source-reference>` | Selection  | One shared source reference               | Start from the normal complete eligible `.agents` Markdown universe | Repeatable. Occurrences form a union of source selections.                                           |
| `--exclude=<source-reference>` | Selection  | One shared source reference               | Subtract nothing                                                    | Repeatable. Occurrences form a union of exclusions, and exclusions win overlap.                      |
| `--tag=<tag>`                  | Selection  | One tag query value                       | No tag predicate is added                                           | Repeatable. Each occurrence adds one scalar predicate in command-line order.                         |
| `--heading=<heading>`          | Selection  | One complete heading query value          | No heading predicate is added                                       | Repeatable. Each occurrence adds one scalar predicate in command-line order.                         |
| `--require=all\|any`           | Selection  | `all` or `any`                            | `all`                                                               | Repeating is invalid. Its position among predicate flags has no effect.                              |
| `--within=<part>[,<part>...]`  | Selection  | One escaped comma-separated region union  | Each predicate uses its natural authored region                     | Repeating the flag is invalid. Parts compose inside one value; duplicate parts have no extra effect. |
| `--content=<part>[,<part>...]` | Projection | One escaped comma-separated content union | No content projection                                               | Repeating the flag is invalid. Parts compose inside one value in canonical result order.             |

- The shared `--detail` flag applies to `find`. Its values,
  default, repetition, composition, and JSON relationship remain defined only by
  the [Global CLI Flags](../shared/global-flags/interface.md) contract. The Find-specific minimal
  and standard result content appears under [Human Output](#human-output).

The [Behavior Contract](behavior.md) records only the deterministic operation
stages and conformance mechanics behind these public facts. This file owns the
complete public selection, matching, projection, and result meaning.

### Bare Inventory

- The smallest valid invocation is:

  ```text
  open-forge find
  ```

  With no predicates, it returns every logical source in the effective source
  universe. A source-universe filter may therefore be used with no predicate.

- `--require` and `--within` are invalid without at least one
  `--tag` or `--heading` predicate because neither would modify an active query.
  `--include` and `--exclude` remain valid in that bare-inventory form because
  they establish the source universe.
- Bare inventory remains flat. It does not display route
  hierarchy, inherited rules, loading relationships, or relevance. `context`
  owns selected content and route-aware context, and structural checking and
  recommendations belong to `doctor`.

### Tag Query Values

- `--tag` is repeatable and each occurrence consumes exactly one
  tag value:

  ```text
  --tag=<tag>
  ```

  ```text
  open-forge find --tag=Architecture
  open-forge find --tag=Architecture --tag=CurrentTruth
  open-forge find --tag="#architecture"
  ```

- A comma-separated tag list is not supported. Repeat
  `--tag` instead:

  ```text
  # Invalid
  open-forge find --tag=Architecture,CurrentTruth

  # Valid
  open-forge find --tag=Architecture --tag=CurrentTruth
  ```

- Tags cannot contain commas under the authored tag grammar.
  Repetition keeps each predicate explicit and gives tag and heading flags the
  same scalar-value shape.
- A tag query accepts either the stored value or one optional
  leading ASCII `#`:

  ```text
  Architecture
  #Architecture
  #architecture
  ```

- Tag normalization and validation use the independently testable rules in
  [Section Findings And Public Ordering](#section-findings-and-public-ordering).
- PascalCase and singular names remain canonical authoring
  guidance, but query casing does not need to follow that guidance.
- These tag values are invalid:

  ```text
  #
  ##Architecture
  Architecture#
  -Architecture
  Architecture-
  Architecture--Current
  1Architecture
  Architecture_Name
  Architecture Current
  ```

- The CLI does not trim surrounding whitespace or accept another
  hash character. Shell quotes are removed before the CLI receives the value.

### Heading Query Values

- `--heading` is repeatable and each occurrence consumes one
  complete heading value:

  ```text
  --heading=<heading>
  ```

  ```text
  open-forge find --heading=Axioms
  open-forge find --heading=Axioms --heading=Instructions
  open-forge find --heading="Current State"
  open-forge find --heading="Release, Notes"
  ```

- A comma in a heading is literal, not a list delimiter. Repeat
  `--heading` for several heading predicates. An empty heading value is invalid.

### Predicate Requirement

- `--require` accepts exactly `all` or `any`, and its omission
  defaults to the flat `all` requirement. Source-universe filters do not require
  an explicit `--require` mode:

  ```text
  --require=all
  --require=any
  ```

- Every `--tag` and `--heading` occurrence is one predicate.
  `all` requires every predicate to match the same logical source. `any` requires
  at least one predicate to match.

  | A   | B   | `all`    | `any`    |
  | --- | --- | -------- | -------- |
  | no  | no  | no match | no match |
  | no  | yes | no match | match    |
  | yes | no  | no match | match    |
  | yes | yes | match    | match    |

- `all` is the default because adding a predicate should
  normally narrow a result. It prevents `--tag=Directive --heading=Instructions`
  from silently returning every Directive plus every source with an Instructions
  heading.
- Predicate logic is flat and order-independent. The interface
  does not support parentheses, grouping, negation, precedence, or an expression
  language. It cannot express `(Tag A OR Tag B) AND Heading H` in one invocation.
  `--require=any` is the explicit flat alternative, not an implicit group. Use
  separate invocations unless repeated evidence later justifies an explicit
  grouping contract.
- Repeating `--require` is invalid. Its position among predicate
  flags does not change its meaning.

### Search Regions

- `--within` selects the authored regions where predicates are
  evaluated. It does not select emitted content:

  ```text
  --within=<part>[,<part>...]
  ```

- The supported `--within` parts are:

  | Part             | Meaning                                                                                     |
  | ---------------- | ------------------------------------------------------------------------------------------- |
  | `document`       | Complete authored document: `frontmatter` plus `body`                                       |
  | `frontmatter`    | Parsed authored YAML frontmatter                                                            |
  | `body`           | Parsed Markdown after frontmatter                                                           |
  | `section:<name>` | Parsed body section under a structural heading whose complete visible text matches `<name>` |

- `document` does not include the generated source identity
  block, files outside the logical source, receipts, recovery data, or ignored
  parser content. A generated `Entries` region remains part of its containing
  body, but its derived lines are excluded from body-tag matching.
- `metadata` is not a complete-document `--within` value. In the
  shared `--content` vocabulary, `metadata` means CLI-generated source metadata.
  Authored YAML remains `frontmatter`.
- When `--within` is omitted, each predicate uses its natural
  authored region. `--tag` defaults to `frontmatter`, and `--heading` defaults to
  `body`. A mixed query uses both defaults, so
  `open-forge find --tag=Directive --heading=Instructions` tests the tag in
  frontmatter and the heading in the body.
- An explicit `--within` applies to every predicate:

  ```text
  # Bare body tags only
  open-forge find --tag=Architecture --within=body

  # Frontmatter or body tags
  open-forge find --tag=Architecture --within=document

  # Tag inside one exact section
  open-forge find \
    --tag=Architecture \
    --within="section:Current State"

  # Headings anywhere in the authored body
  open-forge find --heading=Axioms --within=document

  # Nested heading inside one exact section
  open-forge find \
    --heading=Decision \
    --within=section:History

  # Frontmatter tag or tag in one body section
  open-forge find \
    --tag=Architecture \
    --within="frontmatter,section:Current State"
  ```

- Several `--within` parts form one union. Duplicate parts have
  no additional effect. `document` subsumes `frontmatter`, `body`, and every
  section part, so combining them is accepted but redundant.
- Parts use the same comma and backslash escaping grammar as
  `--content`:

  ```text
  --within="section:Rules\, Limits,frontmatter"
  ```

  This selects the exact `Rules, Limits` section and frontmatter.

- Unknown parts, empty values, an empty section name, a trailing
  escape, or an unsupported escape are invalid. A predicate that cannot occur in
  any selected region is also invalid rather than a successful empty search:

  ```text
  # Invalid: structural headings do not occur in frontmatter
  open-forge find --heading=Axioms --within=frontmatter
  ```

- When at least one selected region supports a predicate,
  inapplicable members of the region union contribute no candidates and do not
  invalidate the query.

## Human Output

The command uses the shared native report. The default detail is `minimal`; `standard`, `full` and `debug` add the catalogue-defined facts. `--detail-filter <error|warning|info|all>` is repeatable and changes only the rendered detail. Use `--format text` for this text report. Primary result text for `completed`, `completed-with-warnings` and `incomplete` is on stdout; primary errors for `invalid-input`, `blocked`, `failed` and `cancelled` are on stderr. There is no `Status:` line.

### Statuses and text

| Status                  | When                                   | Text                                                                                                                               | Exit | Stream |
| ----------------------- | -------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | matches                                | rows                                                                                                                               |    0 | stdout |
| completed               | no matches                             | `No sources match <query>.` (`No sources match --tag Zzz.`; bare inventory empty: `No Markdown sources were found under .agents.`) |    0 | stdout |
| completed-with-warnings | requested section absent, ID collision | warning rows, blank line, match rows                                                                                               |    2 | stdout |
| incomplete              | a source could not be inspected        | warning rows, blank line, the safe rows; `standard` headline says `The search is incomplete.`                                      |    3 | stdout |
| invalid-input           | bad selector, part, require value      | `Cannot search: <problem>.`                                                                                                        |    4 | stderr |
| blocked                 | ambiguous or unsafe selector           | `Cannot search: <reason>.`                                                                                                         |    5 | stderr |
| failed                  | unexpected error                       | `Find stopped because of an unexpected error: <reason>.`                                                                           |    1 | stderr |
| cancelled               | Ctrl+C                                 | `Find was cancelled.`                                                                                                              |  130 | stderr |

### Text by level

`minimal`:

```text
memory                          .agents/memory/_memory.md
memory/archived                 .agents/memory/archived/_archived.md
memory/crystallized             .agents/memory/crystallized/_crystallized.md
```

Two columns, aligned with spaces, id then path. IDs may contain spaces, so
nothing promises that whitespace splits the row; JSON is the machine form.

`standard`:

```text
12 sources match --tag Memory.
memory                          .agents/memory/_memory.md                        Self-growing Markdown memory for active work, coordination, accepted knowledge, candidates, and history
memory/archived                 .agents/memory/archived/_archived.md             Useful history that no longer controls current work
```

`full` adds under each row the match evidence (`  matched tag Memory in
frontmatter`) and, after the rows, the search details: filters, require,
regions searched, source set, `21 of 21 sources inspected`.

`--content` parts print after the rows through the shared
`ContentPartsTextRenderer` from [17](../../../../../working/cli-development/tasks/task30-g4/17-context.md), with the same delimiter
form, at every level.

### Representative transcripts by status

### Transcript — completed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#find-completed).

### Transcript — completed-with-warnings

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#find-completed-with-warnings).

### Transcript — incomplete

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#find-incomplete).

### Transcript — invalid-input

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#find-invalid-input).

### Transcript — blocked

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#find-blocked).

### Transcript — failed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#find-failed).

### Transcript — cancelled

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#find-cancelled).
### Content Projection

`--content=<part>[,<part>...]` requests exact per-source content after the match rows. Supported parts are `metadata`, `frontmatter`, `headings`, `body`, and `section:<name>`. The value is a singleton input; compose several parts in its comma-separated value. The context-specific `paths` part is invalid for `find`.

When a requested section is known to be absent, the source remains in the result with `find.projection-missing` and completed-with-warnings status. An ambiguous or unavailable requested projection keeps the safe matches and forms incomplete status. Find does not widen the effective source universe to resolve route facts or infer a Framework scope.
## Structured Output

`--format json` writes one schema-3 envelope to stdout for every report status. The envelope carries the command, status, workspace, detail, filter, command data, findings, effects, counts, limitations, recovery facts and next action as applicable. It is the same typed result as the text report; no ordinary text is mixed into the JSON document. If parsing fails before binding, the raw parser diagnostic remains text on stderr and no report envelope exists.

### JSON data by level

| Level    | `data`                                                                                                                      |
| -------- | --------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ matches: [ { id, path, description, parts: [ ... ] when requested } ] }`                                                 |
| standard | + per match `evidence: [ { kind: "tag" \| "heading", value, region, layer } ]`, `query { tags, headings, require, within }` |
| full     | + `sourceSet { mode, include: [...], exclude: [...], inspected, candidates }`                                               |
## Semantic Results

The status and exit mapping above are unchanged by detail or format. A
completed search with zero matches is successful. Safe matches remain visible
for incomplete results, but the non-success status prevents automation from
treating them as a complete set.

### Counts and limitations

`matches`, `sourcesInspected`, `sourcesCandidates`.

### Next rules

Invalid selector -> `open-forge route list --depth=all`; incomplete ->
`open-forge doctor`; otherwise none.

## Errors And Boundaries

The findings catalogue below is the command's finite error and warning vocabulary. Findings keep their code, severity, family, subject and cause; detail filtering affects display only. A missing, unavailable or unsafe workspace or source boundary remains invalid-input, incomplete or blocked according to the catalogue.

### Findings catalogue

| Code                         | Severity | Family                | Message                                                                              | Next                                |
| ---------------------------- | -------- | --------------------- | ------------------------------------------------------------------------------------ | ----------------------------------- |
| find.invalid-input           | error    | invalid-input         | examples: `--require must be all or any.`, `--within <value> is not a known region.` |                                     |
| find.invalid-selector        | error    | local                 | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Find/Shared/Wording/FindWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`find.invalid-selector`).                      | `open-forge route list --depth=all` |
| find.workspace-unavailable   | error    | workspace-unavailable |                                                                                      |                                     |
| find.workspace-unsafe        | error    | workspace-unsafe      |                                                                                      |                                     |
| find.selector-ambiguous      | error    | selector-ambiguous    |                                                                                      |                                     |
| find.selector-unsafe         | error    | selector-unsafe       |                                                                                      |                                     |
| find.identity-collision      | warning  | identity-collision    |                                                                                      |                                     |
| find.candidate-unsafe        | warning  | local                 | [`find.phrase.could-not-be-checked-safely-and-was-skipped`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Find/FindPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Find/Shared/Wording/FindWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`find.candidate-unsafe`).                                | `open-forge doctor`                 |
| find.layer-unresolved        | warning  | local                 | [`find.phrase.has-no-base-file-and-was-skipped`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Find/FindPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Find/Shared/Wording/FindWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`find.layer-unresolved`).                              | `open-forge doctor`                 |
| find.inspection-unavailable  | warning  | local                 | [`find.phrase.could-not-be-read-and-was-skipped`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Find/FindPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Find/Shared/Wording/FindWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`find.inspection-unavailable`).                                          | `open-forge doctor`                 |
| find.invalid-encoding        | warning  | local                 | [`find.phrase.is-not-valid-utf-8-and-was-skipped`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Find/FindPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Find/Shared/Wording/FindWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`find.invalid-encoding`).                                         | fix the file                        |
| find.frontmatter-unavailable | warning  | local                 | [`find.phrase.the-frontmatter-of-could-not-be-read-so-its-tags-were-not-matched`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Find/FindPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Find/Shared/Wording/FindWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`find.frontmatter-unavailable`).         | `open-forge doctor`                 |
| find.section-ambiguous       | warning  | local                 | [`find.phrase.has-more-than-one-section-named-none-was-returned`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Find/FindPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Find/Shared/Wording/FindWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`find.section-ambiguous`).                  | fix by hand                         |
| find.projection-missing      | warning  | local                 | [`shared.phrase.has-no-section-named`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Find/Shared/Wording/FindWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`find.projection-missing`).                                                | none                                |
| find.projection-unavailable  | warning  | local                 | [`shared.phrase.the-of-could-not-be-produced`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Find/Shared/Wording/FindWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`find.projection-unavailable`).                              | `open-forge doctor`                 |
| find.operation-failed        | error    | operation-failed      |                                                                                      |                                     |
| find.interrupted             | error    | cancelled |                                                                                      |                                     |

## Scenarios

### Catalogue situations

`bare-inventory`, `one-tag`, `two-tags-all`, `heading`, `no-matches`,
`with-content-headings`, `include-selector`, `ambiguous-selector` (blocked),
`unreadable-source` (incomplete), `section-missing` (warnings), `invalid-selector`,
`invalid-require`.

Each status has one representative native text transcript above. JSON uses the same status and command facts under the schema-3 envelope.
## Non-Goals

- `find` does not search arbitrary words or phrases in
  frontmatter or bodies.
- `find` does not match partial tokens, prefixes, regular
  expressions, fuzzy terms, synonyms, semantic meaning, vectors, or relevance.
- `find` does not correct spelling, remove accents, or normalize
  Unicode representations.
- `find` does not infer inherited, effective, or authoritative
  tags.
- `find` does not infer applicability, scope, authority,
  loading, or current truth from a match.
- `find` does not follow links or search local files outside
  `.agents`.
- `find` does not build the complete context graph.
- `find` does not support grouped Boolean expressions,
  precedence, or negation.
- `find` does not use comma-separated predicate values.
- `find` does not diagnose, repair, mutate, install, or clean
  anything.
- A match proves only that one accepted predicate occurred in
  one declared source region. It does not prove that the source is relevant,
  active, authoritative, or healthy.

## Public Verification

The implementation must expose the following evidence through the public
interface. Detailed technology-neutral semantic evidence is mapped in the
[Behavior Contract](behavior.md), and built-process evidence is mapped in the
[Technical Design](technical-design.md).

- Verify exact current-directory and `--workspace` selection
  without workspace discovery.
- Verify bare inventory in the default standard view and
  explicit minimal ID-and-path output.
- Verify tag and heading flag parsing, no operands, omission
  defaults, repeatability, ordering, conflicts, and dependencies.
- Verify optional leading `#`, tag grammar, case variants,
  duplicate predicates, Unicode variants, whole-token boundaries, and invalid
  comma lists.
- Verify frontmatter tag values and body tag tokens in every
  included and excluded Markdown region.
- Verify parser-recognized headings, all levels, formatted and
  linked visible text, code-containing headings, duplicate headings, malformed
  headings, and heading query values.
- Verify shared heading and section boundaries between find
  evidence and `context` projection.
- Verify repeated tags, repeated headings, mixed predicates,
  `all`, `any`, duplicate inputs, and flat-logic limitations.
- Verify default regions, `document`, `frontmatter`, `body`,
  exact sections, unions, redundancy, escaping, and incompatible selections.
- Verify minimal, standard, content-projected, debug, and
  structured output from one typed result.
- Verify source ID collisions, canonical result ordering, layer
  ordering, evidence ordering, and stable repeat invocation.
- Verify completed matches, completed zero matches, completed-with-warnings,
  incomplete safe matches, invalid-input, blocked boundaries, failed execution,
  and cancellation.
- Verify that `find` does not use a persistent index, follow
  links, search arbitrary text, rank results, build the complete graph, or mutate
  anything.
- Direct public-facing tests should prove predicate parsing,
  region selection, projection, rendering, and semantic-result presentation.
- Focused integration tests should use real temporary workspaces
  and filesystem state to prove the public command boundary.

## Complete Public Selection And Result Details

### Structured Result Fields

- The structured JSON document exposes the selected workspace and
  the method used to select it through the shared top-level `workspace` member.
- The typed structured result exposes the declared source
  universe used for the invocation, including whether it is the default or a
  filtered effective universe.
- The typed structured result exposes predicate occurrences in
  command-line order and their authored query values.
- The typed structured result exposes the effective predicates
  after equivalent query duplicates have been removed.
- The typed structured result exposes the requirement and the
  selected or default search regions.
- The typed structured result exposes the requested detail level
  and content projection.
- The typed structured result exposes effective candidate,
  inspected, and matched source counts. Excluded sources are outside those
  counts.
- The typed structured result exposes coverage state.
- Each returned source exposes its ordered automatic source ID
  and canonical base path.
- A returned source exposes its description when one is
  available.
- Match evidence is exposed by predicate, region, physical
  layer, and source location.
- The typed structured result exposes the requested content
  projection.
- The typed command-local result exposes findings. The shared top-level envelope
  exposes the semantic status.

### Workspace And Source Universe

- `find` uses exactly the current working directory or the exact
  `--workspace <path>` value. It does not search parent directories, choose a Git
  root, or infer another workspace from nearby files.
- The candidate universe covers every ordinary file with a
  `.md` suffix physically contained below the selected workspace's `.agents`
  directory. A candidate must contain valid UTF-8 before it can become an
  inspectable Markdown layer.
- The universe includes routed and unrouted Markdown files.
- The universe includes the Loader and `entrypoints`.
- The universe includes `SKILL.md` and Markdown Skill resources.
- The universe includes Templates and other Markdown sources.
- Valid overwrite companions are included as ordered layers of
  their base source.
- The root `AGENTS.md` entry and provider bridge files are
  excluded.
- Non-Markdown Skill resources and other non-Markdown support
  files are excluded.
- Extension receipts, recovery files, and operational metadata
  are excluded.
- Local Markdown outside `.agents`, including ordinary link
  targets, is excluded.
- Symlinks, junctions, aliases, and other identities that escape
  the selected workspace are not valid sources. An encountered unsafe candidate
  affects coverage.
- Generated `Entries` remain part of the bytes of their
  containing entrypoint, but their derived lines are excluded from body-tag
  matching. They do not define the candidate universe, add source identities, or
  establish search completeness.
- The CLI enumerates the source universe for every invocation. It
  does not require a persistent search index or the complete context graph.
- Routed navigation may accelerate or cross-check enumeration,
  but it cannot hide an eligible Markdown source.

### Logical Sources And Overwrite Framing

- A base file and a valid adjacent overwrite companion form one
  logical result. Both physical layers remain independently inspectable match
  locations in this order: base, then overwrite.
- The logical source uses the automatic ID and canonical
  workspace-relative path of its base file. Minimal output emits that base ID and
  path, and the logical source is emitted only once.
- Predicates may be satisfied across the base and overwrite
  layers. Evidence records which layer supplied each match.
- This is authored-occurrence discovery. It does not calculate
  merged effective metadata or erase a base occurrence because the overwrite
  answers the same question differently.
- An `all` query may be satisfied by occurrences across the two
  ordered layers. Evidence keeps that composition visible. `context` remains
  responsible for presenting the ordered layers and their scoped precedence when
  a caller reads the source.
- Every standard and structured layer record identifies whether
  it is the `base` or `overwrite` layer.
- Every standard and structured layer record includes that
  layer's canonical physical workspace-relative path.
- Every standard and structured layer record includes its match
  region and source location.
- Every matched tag or heading in an standard or structured layer
  record retains its exact authored spelling.
- Content projection preserves the `context` framing: the base
  layer is emitted first, followed by the overwrite layer and its own physical
  path. Content from the two layers is never presented as one unlabeled Markdown
  document.
- An orphan or ambiguous overwrite cannot form a complete logical
  source. Safe matches from unrelated sources remain available, but coverage is
  `incomplete`.

### Public Tag Matching

- A tag query matches a complete authored frontmatter tag value
  or complete visible body-tag token. Tag equality ignores case:

  ```text
  Architecture = architecture = ARCHITECTURE
  Architecture != ArchitectureNotes
  ```

- Tag matching performs no Unicode normalization, accent folding,
  whitespace folding, substring matching, prefix matching, spelling correction,
  synonym matching, fuzzy matching, semantic matching, or relevance ranking.
  Canonically equivalent Unicode sequences with different scalar
  representations may therefore remain different.
- Results preserve authored spelling. When one query matches
  several authored case variants, each occurrence retains its exact source
  spelling and location.
- The command does not create a global tag registry or choose
  one workspace-wide canonical spelling. `doctor` may report noncanonical or
  confusing authored variants under its own validation contract.
- In `frontmatter`, `--tag` tests complete values in the authored
  `open-forge.tags` list. It does not search serialized YAML text,
  descriptions, responsibilities, unknown fields, or generated `Entries` copies.
- In `body` or a selected section, `--tag` tests complete visible
  bare-tag tokens in parsed Markdown text.
- Visible body-tag matching includes parsed text in paragraphs.
- Visible body-tag matching includes parsed text in lists.
- Visible body-tag matching includes parsed text in blockquotes.
- Visible body-tag matching includes parsed heading text.
- Visible body-tag matching includes link labels, emphasis, and
  other visible inline text.
- Body-tag matching excludes frontmatter.
- Body-tag matching excludes generated `Entries` regions and
  their derived lines.
- Body-tag matching excludes inline code and fenced or indented
  code blocks.
- Body-tag matching excludes link destinations, image
  destinations, titles, and URL fragments.
- Body-tag matching excludes raw HTML markup, attributes,
  comments, declarations, and raw blocks.
- Body-tag matching excludes escaped forms such as
  `\#Architecture`.
- Body-tag matching excludes the structural marker characters
  that introduce a heading. Thus `#Architecture` is a visible tag token, while
  `# Architecture` contains a heading marker and no tag token.

  ```md
  #Architecture <!-- match -->
  Review #architecture. <!-- match -->

  ## Review #ARCHITECTURE <!-- match -->

  # Architecture <!-- no tag: heading marker -->

  `#Architecture` <!-- no match: code -->
  [link](page.md#Architecture) <!-- no match: destination -->
  \#Architecture <!-- no match: escaped -->
  #ArchitectureNotes <!-- no match: larger token -->
  ```

- Each body occurrence records authored spelling, source layer,
  1-based line, and occurrence count. The [Shared Result
  Coordinates](../shared/result-coordinates/interface.md) define the exact column
  and source-span representation; each occurrence retains the
  required line, count, layer, and authored-spelling facts above.

### Public Heading Matching

- `--heading` matches complete visible heading text represented
  by a CommonMark ATX or Setext heading node in the configured Markdown parser's
  abstract syntax tree. It is not tied only to raw `#` characters. Parser
  extensions do not add other public heading forms unless a later compatibility
  decision names them.
- Heading matching uses the complete visible heading text,
  formed by concatenating parsed inline content as a reader sees it, rather than
  raw source-marker text.
- Text and decoded character entities in a heading contribute to
  visible heading text.
- Soft breaks and hard breaks in heading inline content
  contribute to visible heading text.
- Inline code content contributes to visible heading text.
- Emphasis and strong markers do not contribute their marker
  characters to visible heading text.
- Consecutive whitespace created by heading inline structure is
  collapsed to one ASCII space, and leading or trailing whitespace is removed
  before comparison.
- Link and image labels contribute visible heading text, while
  destinations and titles do not.
- Raw inline HTML and HTML blocks do not contribute visible
  heading text.
- Malformed text that the parser does not represent as an ATX or
  Setext heading node is not guessed into one.

  For example:

  ```md
  ## **Current State**
  ```

  matches:

  ```text
  --heading="current state"
  ```

- Heading equality ignores case.
- Heading matching does not apply Unicode normalization or
  approximation, and does not match slugs, fragments, substrings, prefixes,
  alternate names, or semantic equivalents.
- All heading levels are eligible. A separate `--title`
  predicate is unnecessary because an H1 title is already a structural heading.
- Each heading occurrence records its level, ATX or Setext
  source form, physical layer, physical path, source location, and whether it is
  eligible for the canonical Framework semantic-section contracts.
- Duplicate headings produce several evidence occurrences under
  one logical source result. They do not duplicate the source.
- Open Forge authors canonical semantic sections with ATX
  headings. Broader structural discovery does not make Setext headings
  canonical, and it does not make a heading semantically active.
- Only the component contract can make `Axioms`, `Instructions`,
  or another named section carry Framework behavior.
- The `context section:<name>` projection uses the same accepted
  parser heading nodes and section boundaries for structural retrieval. Its
  result continues to identify whether the selected section satisfies canonical
  Framework syntax.

### Section Findings And Public Ordering

- A missing named section contributes no candidates for that
  physical layer.
- A matching section in the base and a matching section in the
  overwrite are both valid, separately framed search regions. They do not make
  the logical source ambiguous.
- When several headings within the same physical layer match the
  same requested section name, that layer's region is ambiguous. Safe matches
  from unambiguous layers and unrelated sources remain available, but coverage
  is `incomplete` rather than choosing one section.
- Logical source results sort by automatic source ID using
  ordinal comparison.
- Canonical base workspace-relative path uses ordinal comparison
  as the tie-breaker for equal source IDs.
- ID collisions retain every distinct logical source. Result
  display shows each exact base path, and match evidence and content layers show
  their exact physical base or overwrite paths.
- Predicate evidence follows command-line predicate order after
  deduplication.
- Regions use `frontmatter`, then `body` document order.
- Layers use base, then overwrite order.
- Heading and body occurrences use source order.
- Authored content follows the accepted `context` projection
  order.
- Filesystem enumeration order, generated `Entries`, route order,
  and discovery timing do not change output order.
- Repeating an equivalent predicate or region does not duplicate
  results or evidence.
- Query predicates that differ only by case or one optional
  leading `#` are duplicates. The command keeps the first occurrence and
  discards later duplicates without changing the result.
- The CLI removes exactly one optional leading ASCII `#` before
  it validates a tag query.
- The first character of the remaining tag value must be a
  letter.
- Every later character must be a letter, a number, or an
  internal hyphen.
- A hyphen cannot lead, trail, or repeat as an empty segment.

## Source-Universe Filters

Find explicitly applies the shared [Source-Universe Filters Interface
Contract](../shared/source-universe-filters/interface.md) to `--include` and
`--exclude`. That shared contract is the authority for the reusable scalar
source-reference grammar, identity, collision and disambiguation behavior,
physical safety, expansion, set algebra, and supplied/resolved selector
reporting. The shared [CLI Source References Interface](../shared/source-references/interface.md)
remains the related authority for the source-reference vocabulary. The
Find-specific rules below preserve Find's applicability, default universe,
coverage, output, examples, and verification without redefining the shared
contract.

### Filter Composition And Expansion

- Find explicitly declares `--include` and `--exclude` as
  repeatable scalar source-universe filters and applies the shared Interface
  Contract. They narrow candidates before predicate matching. They are not
  predicates, global flags, content filters, or result caps.
- Each occurrence accepts exactly one shared source reference
  under the [shared filter Interface](../shared/source-universe-filters/interface.md).
  Find adds no comma-list, glob, arbitrary-directory, positional-operand,
  tag-inference, lifecycle-inference, or new qualifier grammar.
- Find uses the shared union, deduplication, and exclusion-wins
  set algebra: repeated includes form one union, repeated excludes form one
  union, and the exclude union is subtracted after the included or default
  universe regardless of argument order. Equivalent or overlapping selectors
  have no additional effect.
- When `--include` is omitted, Find starts from its existing
  normal complete eligible `.agents` Markdown universe defined in [Workspace
  And Source Universe](#workspace-and-source-universe). When `--exclude` is
  omitted, it subtracts nothing. An invocation with neither flag therefore
  selects the same universe and operation as before these filters were added.
  The shared contract defines the omission and composition relationship, not
  Find's default.
- Find applies the shared physical expansion rule: a source
  reference resolving to the Loader, a recognized `entrypoint`, or `SKILL.md`
  selects that source's containing folder as a physical folder root and
  includes every eligible Markdown source physically contained below it,
  including eligible unrouted sources.
- Find's folder-root expansion is physical containment under
  the shared contract. It is not routed-descendant selection, authority
  inference, lifecycle inference, scope inference, or link following. A routed
  child outside the selected physical folder is not added, and a physically
  contained unrouted source is not removed for lacking a route.
- A source reference resolving to an ordinary source selects
  that one logical source under the shared identity rule. A valid base and
  overwrite companion pair is always selected or excluded together with both
  physical layers, whether the reference resolves through its ID, base path, or
  overwrite path. The overwrite is not an independent candidate.
- Find resolves and composes the effective source universe
  before inspecting candidates or matching predicates. `--require` and
  `--within` do not govern source filtering. Source filtering is valid for bare
  inventory with no `--tag` or `--heading` predicate; predicates then operate
  only on the effective universe.
- Every selector uses the shared exact source-ID or `.agents/...`
  path grammar and its normalization, identity, collision, disambiguation, and
  safety rules. Find does not guess between an ID and a path or add another
  selector syntax.
- A missing, empty, unknown, or unsupported selector is
  invalid under the shared filter Interface. An ambiguous ID is blocked unless
  the shared interactive disambiguation flow resolves that ID. JSON and other
  non-interactive uses do not prompt. An unsafe or escaping physical boundary
  is blocked; no selector resolution guesses or bypasses it.

### Filtered Coverage, Results, And Ordering

- Coverage is complete when every candidate in the explicitly
  effective source universe has been accounted for under the existing Find
  inspection rules. Excluded sources and excluded physical areas are outside
  that universe and need not be parsed. A valid effective universe with zero
  candidates or zero matches is complete.
- Standard human results echo the supplied include and exclude
  occurrences, their resolved selector identities, whether the default or
  filtered universe was used, and the effective candidate, inspected, and
  matched counts. The selector detail appears in the source-universe portion of
  the result, not as predicate evidence.
- JSON results expose the same supplied and resolved selector
  facts, default-or-filtered universe state, and effective candidate, inspected,
  and matched counts in either JSON view. `--detail` selects the structured
  projection under the shared global contract.
- Minimal output visibly marks the universe as `default` or
  `filtered` in its summary line. A filtered minimal result does not repeat the
  full include and exclude selector detail; it still keeps the result, coverage,
  source identity, and required next-action facts.
- Selector occurrences are echoed in supplied command-line
  order. Deduplication applies to resolved selector sets and effective sources,
  not to the supplied occurrence record. Selector order does not become result
  order. Result sources continue to use the existing automatic-ID and
  canonical-path order, and each logical source appears once.
- Source-universe filters do not change the flat predicate
  requirement. Omitted `--require` is `all`, and `--require=any` is the explicit
  flat alternative. Neither mode groups source selectors with predicates, and
  neither changes the meaning of `--within`.
- Find does not add `--limit`, `--max-results`, truncation,
  pagination, or another result-cap behavior. A filtered universe changes which
  eligible candidates are considered, not how many matching results may be
  returned from that universe.

### Filter Scenarios

- The existing bare invocation remains unchanged:

  ```text
  open-forge find
  ```

  It uses the default complete eligible universe. A predicate-free filtered
  inventory is also valid:

  ```text
  open-forge find --include=memory/crystallized/documents
  ```

- An ordinary source selector narrows the universe to one
  logical source:

  ```text
  open-forge find \
    --include=memory/crystallized/documents/architecture
  ```

  If that source has a valid overwrite companion, both layers remain part of
  the one selected logical source.

- An entrypoint selector expands by physical folder rather than
  route traversal:

  ```text
  open-forge find --include=memory/crystallized/documents
  ```

  The same expansion rule applies to `loader` and a Skill source such as
  `skills/experience-design`. Each includes eligible Markdown below its folder,
  including sources with no generated route entry.

- Include and exclude unions compose independently, and the
  exclude union wins their overlap:

  ```text
  open-forge find \
    --include=memory/crystallized/documents \
    --include=memory/crystallized/documents/cli/contracts/find \
    --exclude=memory/crystallized/documents/architecture \
    --exclude=memory/crystallized/documents/architecture
  ```

  Repeating the same exclusion and moving it before the includes produces the
  same effective universe.

- A source filter composes with predicates without changing
  their roles:

  ```text
  open-forge find \
    --include=memory/crystallized/documents \
    --tag=CurrentTruth \
    --require=any \
    --within=frontmatter
  ```

  The filter establishes candidates first. `any` remains a flat predicate
  requirement, and `frontmatter` controls matching rather than source selection.

- Valid filters can produce no candidates or no matches without
  becoming incomplete:

  ```text
  open-forge find \
    --include=memory/crystallized/documents/architecture \
    --exclude=memory/crystallized/documents/architecture
  ```

  The effective universe is empty, so the completed result reports zero
  candidates and zero matches.

- Presentation exposes the filter state at the appropriate
  density:

  ```text
  open-forge find \
    --include=memory/crystallized/documents \
    --detail minimal

  open-forge find \
    --include=memory/crystallized/documents \
    --format json \
    --detail minimal
  ```

  Minimal output marks `universe=filtered`. JSON echoes supplied and resolved
  selectors, with the well-formed `--detail` selecting the JSON projection.

### Filter-Specific Errors And Non-Goals

- These are invalid rather than alternate selector forms:

  ```text
  open-forge find --include=memory/crystallized/documents,skills/experience-design
  open-forge find --include=memory/**/architecture
  open-forge find .agents/memory/crystallized/documents/
  ```

  The first attempts a comma list, the second a glob, and the third an
  arbitrary directory operand. Use one shared source reference per occurrence;
  use an entrypoint source reference when folder-root expansion is intended.

- Source filters do not infer tags, lifecycle state, authority,
  route descendants, scope, relevance, or link relationships. They do not follow
  links or turn a physical folder into a Framework route.
- Source filters do not cap, truncate, paginate, rank, or
  otherwise limit the results after matching. They do not add a persistent index,
  session, receipt, or mutation authority.

### Filter-Specific Public Verification

- Verify that omitted filters use the prior complete eligible
  `.agents` Markdown universe and that filtered bare inventory remains valid.
- Verify one-scalar-per-occurrence parsing, rejection of comma
  lists, globs, arbitrary directories, positional operands, and unaccepted
  qualifier or inference forms.
- Verify include union, exclude union, duplicate and overlapping
  selectors, exclude-wins behavior, and argument-order independence.
- Verify Loader, recognized entrypoint, `SKILL.md`, ordinary
  source, unrouted descendant, and valid base/overwrite expansion and exclusion
  behavior by physical containment.
- Verify shared ID/path classification, exact matching,
  collisions, interactive disambiguation, non-interactive blocking, unknown and
  unsupported selectors, and unsafe or escaping physical boundaries.
- Verify that effective-universe formation precedes inspection
  and matching, excluded areas need not be parsed, and effective zero-candidate
  and zero-match results are complete.
- Verify standard and JSON selector echo, default-or-filtered
  state, effective candidate/inspected/matched counts, minimal filtered marking,
  stable source ordering, flat `all`/`any` behavior, and absence of result caps.

## Related Current Sources

- [Find Behavior Contract](behavior.md)
- [Find Technical Design](technical-design.md)
- [CLI Command Contract Set — Interface Contract](../../command-contract-set.md#interface-contract)
- [Context Interface Contract](../context/interface.md)
- [Status Interface Contract](../status/interface.md)
- [Global CLI Flags](../shared/global-flags/interface.md)
- [Shared Source-Universe Filters Interface Contract](../shared/source-universe-filters/interface.md)
- [Shared Source-Universe Filters Behavior Contract](../shared/source-universe-filters/behavior.md)
- [CLI Source References](../shared/source-references/interface.md)
- [CLI Source Reference Forms](../shared/source-references/interface.md#accepted-forms)
- [CLI Source Reference Collisions](../shared/source-references/interface.md#collisions-and-disambiguation)
- [Framework Path Identity And Containment](../../../framework/routing/paths.md)
- [Framework Overwrite Customization](../../../framework/routing/overwrites.md)
- [Historical CLI Decision Agenda](../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Historical CLI Release Plan](../../../../../archived/cli-release/release-plan-2026-08-21.md)
- [CLI Implementation Reset](../../../../../archived/cli-release/implementation-reset-2026-08-21.md)
- [CLI Architecture](../../architecture.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)

## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`find.help.syntax`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Find/FindText.cs).

<!-- @OpenForgeTextRef find.help.syntax -->
<!-- @OpenForgeTextRef find.phrase.could-not-be-checked-safely-and-was-skipped -->
<!-- @OpenForgeTextRef find.phrase.could-not-be-read-and-was-skipped -->
<!-- @OpenForgeTextRef find.phrase.has-more-than-one-section-named-none-was-returned -->
<!-- @OpenForgeTextRef find.phrase.has-no-base-file-and-was-skipped -->
<!-- @OpenForgeTextRef find.phrase.is-not-valid-utf-8-and-was-skipped -->
<!-- @OpenForgeTextRef find.phrase.the-frontmatter-of-could-not-be-read-so-its-tags-were-not-matched -->
<!-- @OpenForgeTextRef shared.phrase.has-no-section-named -->
<!-- @OpenForgeTextRef shared.phrase.the-of-could-not-be-produced -->
