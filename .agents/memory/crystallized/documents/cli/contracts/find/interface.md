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

The [CLI Architecture](../../architecture.md#result-json-coordinates-and-process-status)
defines only the shared JSON envelope, source-location primitive, and
status/process coordinates. This Interface owns the exact command-local
`find.result` object, its findings and finite values, and every `find`-specific
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
- The six shared global flags, `--workspace`, `--json`,
  `--view`, `--verbose`, `--help`, and `--version`, apply to `find` under the shared
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

- The shared `--view` flag applies to `find`. Its values,
  default, repetition, composition, and JSON relationship remain defined only by
  the [Global CLI Flags](../shared/global-flags/interface.md) contract. The Find-specific compact
  and expanded result content appears under [Human Result View](#human-result-view).

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

### Human Result View

- The shared `--view` flag selects the Find-specific human
  result shapes below. Under the shared contract, changing the view does not
  change candidate selection, matching, evidence collection, or status. The
  shared contract remains the sole owner of the flag's values, default,
  repetition, composition, and JSON relationship.

- `compact` begins with one tab-separated result line, followed
  by one source identity per line. The summary visibly identifies whether the
  effective source universe is the default or filtered universe:

  ```text
  result=complete	coverage=complete	universe=default	matches=2
  <source-id>	<canonical-path>
  ```

  Example:

  ```text
  result=complete	coverage=complete	universe=default	matches=2
  directives/security	.agents/directives/security.md
  directives/writing	.agents/directives/writing.md
  ```

  The compact stream has no heading, workspace block, query explanation,
  selector detail, description, or optional evidence. A filtered invocation uses
  `universe=filtered` in the same summary position without repeating its full
  selectors. A result with a required next action follows its summary with the
  concise finding and next action. An `attention` result has no `Next:` line. It
  is deterministic, understandable without color,
  and exposes both ID and path as required by the shared source-reference
  contract.

  When `--content` is present, the summary adds projection coverage immediately
  after match coverage:

  ```text
  result=attention	coverage=complete	projection=complete	universe=default	matches=1
  ```

  `projection` uses `not-started`, `complete`, `incomplete`, `blocked`, `failed`,
  or `interrupted`, matching structured projection coverage. A known missing
  section keeps `projection=complete` and produces `attention`; unavailable or
  ambiguous requested content uses `projection=incomplete`. The projection field
  is absent from compact output only when `--content` is omitted.

- `expanded` emits the complete human-readable query and match
  explanation. Its representative shape is:

  ```text
  Workspace: D:/Repositories/open-forge
  Selected by: current directory

  Filters:
    Tag:     Directive
    Heading: Instructions
  Require: all
  Within:
    Tag:     frontmatter
    Heading: body
  Source universe:
    Mode:       default
    Include:    omitted
    Exclude:    omitted
    Candidates: <effective-candidate-count>
    Inspected:  <effective-inspected-count>
  Coverage: complete
  Projection coverage: not requested
  Matches: 2

  directives/security
    Path: .agents/directives/security.md
    Description: Apply required workspace security boundaries
    Matched:
      Directive — frontmatter, base
      Instructions — heading, base, line 11

  directives/writing
    Path: .agents/directives/writing.md
    Description: Write clear, consistent user communication and source prose
    Matched:
      Directive — frontmatter, base
      Instructions — heading, base, line 9
  ```

  For a filtered universe, the same block uses `Mode: filtered` and shows the
  supplied include and exclude occurrences together with their resolved
  selector identities. Its candidate, inspected, and matched counts describe
  only the effective universe. Expanded output always shows `Projection
  coverage:` as `not requested`, `not started`, `complete`, `incomplete`,
  `blocked`, `failed`, or `interrupted`; the corresponding structured values use
  hyphens where shown in the exact schema, including `not-requested` and
  `not-started`.

- `--verbose` retains its shared diagnostic meaning. It does
  not select expanded view. The command never changes view based on terminal
  interactivity or output redirection.

### Content Projection

- `--content` reuses the per-source result-content parts and
  section grammar from [`context` Interface Contract](../context/interface.md):

  ```text
  --content=<part>[,<part>...]
  ```

- The supported `--content` parts are:

  | Part             | Result                                        |
  | ---------------- | --------------------------------------------- |
  | `metadata`       | Complete Find source metadata block           |
  | `frontmatter`    | Complete authored YAML frontmatter            |
  | `headings`       | Parsed heading outline without section bodies |
  | `body`           | Complete Markdown after frontmatter           |
  | `section:<name>` | Exact parsed Markdown section                 |

  The context-specific `paths` part is invalid for `find`. It represents context
  resolver order, physical layers, and inclusion reasons, while Find orders
  matched logical sources by its inventory contract. Compact Find view already
  supplies the token-friendly matched ID-and-path list.

  Find metadata contains the logical result position, automatic ID, canonical
  base path, routed or unrouted state, mechanically established route when
  present, and each physical base or overwrite layer path and kind. `route` is
  the automatic ID when one unambiguous Loader-rooted route reaches that source;
  an unrouted source reports null route. When routing facts are ambiguous or
  unavailable, the metadata projection is unavailable and projection coverage is
  incomplete rather than guessing. A source-universe filter is not widened to
  read route-supporting sources outside the effective universe; if those facts
  are required, the affected metadata projection is unavailable. Find does not
  infer a scope because the Framework scope role is authored meaning, not a
  mechanically identifiable path segment. It does not invent context inclusion
  reasons or context order. Match evidence remains part of the expanded Find
  result rather than source metadata. Authored YAML remains available through
  `frontmatter`.

- Unlike `context`, omitting `--content` in `find` emits no
  content projection. Compact or expanded match output still appears according
  to `--view`. Supplying `--content` appends selected source blocks and authored
  parts in result order.
- Matching and presentation remain independent:

  ```text
  # Search frontmatter tags; return compact identities
  open-forge find --tag=Directive --view=compact

  # Search body tags; return complete bodies
  open-forge find \
    --tag=Architecture \
    --within=body \
    --content=body

  # Return CLI-generated metadata for matched sources
  open-forge find --tag=Directive --content=metadata

  # Return heading outlines for matched sources
  open-forge find --tag=Directive --content=headings

  # Find tagged sources with Instructions and return those sections
  open-forge find \
    --tag=Directive \
    --heading=Instructions \
    --content=section:Instructions

  # Find either heading and return both exact sections when present
  open-forge find \
    --heading=Axioms \
    --heading=Instructions \
    --require=any \
    --content=section:Axioms,section:Instructions

  # Expanded match evidence plus frontmatter
  open-forge find \
    --tag=CurrentTruth \
    --view=expanded \
    --content=frontmatter
  ```

- When neither physical layer contains a requested section, the
  source remains present with a named missing-section finding. A known absence
  after complete inspection produces `attention`; an ambiguous section or
  incomplete layer inspection produces `incomplete`. Compact output retains
  result and projection coverage, source identity, and the affected section
  name. Expanded and structured output add layer, location, evidence, and every
  independently available section. Section projection never changes whether the
  source matched the Find predicates.

### Structured Result

- `--json` serializes the complete typed result rather than the
  compact or expanded human projection. A well-formed `--view` is accepted as a
  no-op because JSON always returns the complete structured result. `--content`
  continues to select result content in the structured result.
- Repeating `--view`, `--content`, or `--within` is invalid.
  Several parts must be composed inside one comma-separated value for that
  dimension.
- The structured result exposes the complete typed public result described in
  [Structured Result Fields](#structured-result-fields) and the source-universe
  facts in [Filtered Coverage, Results, And Ordering](#filtered-coverage-results-and-ordering).
  The exact command-local schema is defined below. The Architecture supplies
  only the shared top-level envelope, location, and status coordinates.
- The JSON document preserves the complete typed facts above and the semantic
  status through the shared top-level envelope. Its command-local members are
  not duplicated in the Architecture document.

#### Exact Command-Local Schema

The following camel-case grammar lists every command-local member in wire order.
No member in this grammar is omitted. `SourceLocation` is the shared primitive
defined by the [CLI Architecture](../../architecture.md#result-json-coordinates-and-process-status).
The shared envelope's `command` member is exactly `find`; the complete public
command form remains `open-forge find`.

```text
type FindResult = {
  universe: Universe;
  query: Query;
  presentation: Presentation;
  coverage: Coverage;
  findings: Finding[];
  matches: Match[];
};

type Universe = {
  mode: "default" | "filtered";
  include: Selector[];
  exclude: Selector[];
  candidateCount: nonnegative-integer | null;
  inspectedCount: nonnegative-integer | null;
  matchedCount: nonnegative-integer | null;
};

type Selector = {
  value: string;
  form: "id" | "path" | null;
  resolution: "resolved" | "invalid" | "unknown" | "unsupported" | "ambiguous" | "unsafe";
  identity: { id: string; path: string } | null;
  sourceKind: "loader" | "entrypoint" | "skill" | "ordinary" | null;
  expansion: "folder" | "source" | null;
  candidates: { id: string; path: string }[];
};

type Query = {
  predicates: Predicate[];
  effectivePredicates: Predicate[];
  require: "all" | "any";
  within: Within;
};

type Predicate = {
  kind: "tag" | "heading";
  value: string;
};

type Within = {
  supplied: CanonicalRegion[];
  tag: CanonicalRegion[];
  heading: CanonicalRegion[];
};

type Presentation = {
  view: {
    supplied: "compact" | "expanded" | null;
    effective: "compact" | "expanded";
  };
  content: {
    supplied: CanonicalPart[];
    effective: CanonicalPart[];
  };
};

type Coverage = {
  state: CoverageState;
  matching: CoverageState;
  projection: ProjectionCoverageState;
};

type Finding = {
  code: FindFindingCode;
  status: SharedStatus;
  subject: bounded-string | null;
  cause: bounded-string;
  selectorRole: "include" | "exclude" | null;
  selectorOccurrence: positive-integer | null;
  source: { id: string; path: string } | null;
  layer: "base" | "overwrite" | null;
  path: string | null;
  region: CanonicalRegion | null;
  location: SourceLocation | null;
  candidates: { id: string; path: string }[];
};

type Match = {
  position: positive-integer;
  id: string;
  path: string;
  description: string | null;
  evidence: Evidence[];
  projections: Projection[];
};

type Evidence = {
  predicate: positive-integer;
  kind: "tag" | "heading";
  query: string;
  authored: string;
  region: CanonicalRegion;
  layer: "base" | "overwrite";
  path: string;
  location: SourceLocation;
  occurrence: positive-integer;
  heading: HeadingEvidence | null;
};

type HeadingEvidence = {
  level: positive-integer;
  form: "atx" | "setext";
  canonical: boolean;
};

type Projection = {
  part: "metadata" | "frontmatter" | "headings" | "body" | "section";
  name: string | null;
  layer: "base" | "overwrite" | null;
  path: string | null;
  state: "available" | "missing" | "unavailable" | "ambiguous";
  metadata: Metadata | null;
  text: string | null;
  headings: ProjectedHeading[];
  location: SourceLocation | null;
};

type Metadata = {
  position: positive-integer;
  id: string;
  path: string;
  routeState: "routed" | "unrouted";
  route: string | null;
  layers: MetadataLayer[];
};

type MetadataLayer = {
  kind: "base" | "overwrite";
  path: string;
};

type ProjectedHeading = {
  text: string;
  level: positive-integer;
  form: "atx" | "setext";
  location: SourceLocation;
  canonical: boolean;
};

type SharedStatus =
  "complete" | "attention" | "incomplete" | "invalid" | "blocked" | "failed" | "interrupted";

type CoverageState =
  "not-started" | "complete" | "incomplete" | "blocked" | "failed" | "interrupted";

type ProjectionCoverageState = CoverageState | "not-requested";

type CanonicalRegion = "document" | "frontmatter" | "body" | "section:<name>";

type CanonicalPart =
  "metadata" | "frontmatter" | "headings" | "body" | "section:<name>";
```

`include` and `exclude` are always-present selector arrays. Within each role,
they retain supplied command-line order. Counts are nonnegative integers when
established, `null` when not established, and `0` for a known empty count.
`candidates` is always present in every selector record and is used for
ambiguity; it is otherwise an empty array. `identity`, `sourceKind`, and
`expansion` are `null` when the corresponding fact is not established.

`predicates` contains every supplied predicate occurrence in cross-flag
command-line order. `effectivePredicates` contains the deduplicated first
occurrences in public order. `require` is always `all` or `any`, including the
default `all`. `within.supplied` is empty when `--within` is omitted. Its
`tag` and `heading` arrays contain the effective regions after defaults. All
three arrays contain canonical strings: `document`, `frontmatter`, `body`, or
`section:<name>`.

`view.supplied` is `null` when omitted, and `view.effective` is always
`compact` or `expanded`. `content.supplied` preserves parsed input order, while
`content.effective` uses canonical projection order. Both content arrays are
empty when `--content` is omitted. Their values are canonical part strings:
`metadata`, `frontmatter`, `headings`, `body`, or `section:<name>`.

`coverage.state` is the overall established coverage. `coverage.matching` is
predicate and inventory coverage. `coverage.projection` is `not-requested`
when content is omitted. The state and matching values are
`not-started`, `complete`, `incomplete`, `blocked`, `failed`, or `interrupted`;
projection accepts those values plus `not-requested`. A known missing requested
section has complete projection coverage and an `attention` finding. Requested
content that is unavailable or ambiguous has incomplete projection coverage.
Find has no truncation coverage state.

`subject` and `cause` are bounded strings. `subject` may be `null`; `cause` is
required. `selectorOccurrence` is 1-based within its selector role. The
`selectorRole`, `source`, `layer`, `path`, and `region` members are `null` when
they do not apply or are unavailable. A `location` is `null` when it does not
apply; when it applies but is unavailable, the finding itself explains that
unavailability. `candidates` is always present. A finding's `status` is one of
the seven shared status names.

`position` is the 1-based match result order. `description` may be `null`, and
`evidence` and `projections` are always arrays. An evidence `predicate` is a
1-based index into `effectivePredicates`. `occurrence` is the 1-based
source-order occurrence for that predicate, region, and layer. `heading` is
`null` when it has no heading value; otherwise it has `level`, `form`, and
`canonical` in that order. `form` is `atx` or `setext`, and `canonical` means
eligible for canonical Framework semantic-section syntax.

Projection `part` uses the finite values `metadata`, `frontmatter`, `headings`,
`body`, and `section`; a section entry carries its requested name in `name`.
`name` is non-`null` only for `section`. `layer` and `path` are `null` only for
the logical `metadata` entry. `state` is `available`, `missing`, `unavailable`,
or `ambiguous`. Only requested projection entries occur. Metadata produces one
logical entry. `frontmatter`, `headings`, and `body` produce one entry per
physical layer. Each requested section produces one entry per layer. `missing`
means only a known absent section; `ambiguous` means repeated matching headings
in one layer; `unavailable` means that the selected content could not be
established. Projection `location` applies to available `frontmatter`, `body`,
and `section` text. It is `null` for metadata and heading-outline entries because
metadata has no authored span and each projected heading owns its location.

`metadata` is non-`null` only for an available metadata projection. `text` is
non-`null` only for an available `frontmatter`, `body`, or `section` projection;
an empty string is valid. `headings` is populated only for available headings
projection and is otherwise `[]`. For unavailable, ambiguous, or missing
projections, only the payload members `metadata`, `text`, `headings`, and
`location` remain `null` or empty. The discriminator and identity members
`part`, `name`, `layer`, `path`, and `state` retain their established values.
Arrays are always present, and no command-local schema member is omitted.

Metadata records keep `layers` in base-then-overwrite order as `{ kind, path }`
records. Projected heading records use `text`, `level`, `form`, `location`, and
`canonical` in that order. The top-level `workspace` and `next` members remain
the Architecture envelope members; Find supplies the command-local `next`
values below.

#### Finding Codes And Ordering

Find has exactly this finding vocabulary. Each code has only the status shown:

| Machine code                    | Finding status |
| ------------------------------ | -------------- |
| `find.invalid-input`           | `invalid`      |
| `find.invalid-selector`        | `invalid`      |
| `find.workspace-unavailable`   | `blocked`      |
| `find.workspace-unsafe`        | `blocked`      |
| `find.selector-ambiguous`      | `blocked`      |
| `find.selector-unsafe`         | `blocked`      |
| `find.identity-collision`      | `attention`    |
| `find.candidate-unsafe`        | `incomplete`   |
| `find.layer-unresolved`        | `incomplete`   |
| `find.inspection-unavailable`  | `incomplete`   |
| `find.invalid-encoding`        | `incomplete`   |
| `find.frontmatter-unavailable` | `incomplete`   |
| `find.section-ambiguous`       | `incomplete`   |
| `find.projection-missing`      | `attention`    |
| `find.projection-unavailable`  | `incomplete`   |
| `find.operation-failed`        | `failed`       |
| `find.interrupted`             | `interrupted`  |

The codes mean:

- `find.invalid-input` covers Find command grammar, flag, non-selector value, and
  dependency errors. Selector value and form errors use
  `find.invalid-selector`; neither code exposes parser exception identity.
- `find.invalid-selector` covers missing, empty, unknown, unsupported, comma,
  glob, and arbitrary-directory selectors.
- `find.workspace-unavailable` and `find.workspace-unsafe` are pre-universe
  workspace blockers.
- `find.selector-ambiguous` and `find.selector-unsafe` are unresolved filter
  blockers.
- `find.identity-collision` retains all exact paths with complete coverage and
  therefore has `attention` status.
- `find.candidate-unsafe` means an effective candidate cannot be safely
  inspected while the workspace boundary remains established. It is
  `incomplete`, not `blocked`.
- `find.layer-unresolved` covers an orphan or ambiguous overwrite.
- `find.inspection-unavailable` covers a missing race, access failure,
  directory-enumeration failure, or I/O failure. It does not expose exception
  names.
- `find.invalid-encoding` means strict UTF-8 decoding failed.
- `find.frontmatter-unavailable` is emitted only when semantic frontmatter is
  needed for matching and cannot be established. Raw authored frontmatter may
  still be projected when its boundary and bytes are available. Body-only work
  does not become incomplete merely because optional metadata is unavailable.
- `find.section-ambiguous` applies only when one requested `section:<name>` maps
  to several headings in one layer during region matching or section projection.
  Duplicate headings remain ordinary ordered evidence and heading-outline items
  when no singular section boundary is requested.
- `find.projection-missing` means a requested section is known to be absent
  after complete inspection. It has `attention` status.
- `find.projection-unavailable` is used only when no more specific inspection,
  frontmatter, or section finding explains a requested projection failure. It
  also represents unavailable metadata route facts when no more specific
  candidate finding applies.
- `find.operation-failed` and `find.interrupted` preserve their event meaning.

A `complete` result has no findings. A higher aggregate result may retain lower-
severity findings that were already established. Accepted Setext,
compatibility, and other noncanonical forms do not create a Find finding merely
for their form. `doctor` owns that diagnosis.

Findings use the fixed code and stage order shown in the table first. Within
selector codes, `include` precedes `exclude`, followed by supplied occurrence.
Within source codes, order is source ID ordinal, path tie-break, base then overwrite,
frontmatter, then body in document order, and then location. Failure and
interruption are last. Finding order never follows filesystem order, discovery
order, or exception order.

#### Next Actions

The top-level `next` member uses the Architecture shape `{ command, reason }` or
`null`, with these exact Find values. The compact human line is shown in the
last column. `next.command` is a canonical action command line, not the result's
command identity and not a reconstruction of the caller's original arguments.
It includes a fixed option only when that option is the action itself; a reason
that says to rerun the same request requires the caller to preserve the original
Find arguments.

| Condition | Top-level `next` | Compact human line |
| --------- | ---------------- | ------------------ |
| `complete` | `null` | no line |
| `attention` | `null` | no line |
| `incomplete` | `{ command: "open-forge doctor", reason: "Inspect the unavailable source or projection facts before relying on this Find result." }` | `Next: open-forge doctor` |
| `invalid` | `{ command: "open-forge find --help", reason: "Correct the named Find input, then rerun the request." }` | `Next: correct the named Find input.` |
| `blocked` where `find.selector-ambiguous` is the only blocking finding | `{ command: "open-forge find", reason: "Replace every ambiguous selector with one listed exact path, then rerun the same request." }` | `Next: rerun with one listed exact path for each ambiguous selector.` |
| other `blocked` | `{ command: "open-forge doctor", reason: "Inspect the blocked workspace or source boundary before rerunning Find." }` | `Next: open-forge doctor` |
| `failed` | `{ command: "open-forge find --verbose", reason: "Report the failure and retry the same request with bounded diagnostics." }` | `Next: report the failure and retry with bounded diagnostics.` |
| `interrupted` | `{ command: "open-forge find", reason: "Rerun the same Find request." }` | `Next: rerun the same request.` |

Find never recommends mutation, repair, or content rewriting. These values are
status-deterministic except for the selector-ambiguity-only blocked branch.

## Semantic Results

| Result        | Meaning                                                                                                                                                     |
| ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | The complete effective source universe was inspected and the returned sources are exactly those that satisfy the request under the accepted matching rules. |
| `attention`   | Match coverage is complete, but an established identity-collision or known missing-projection finding remains.                                              |
| `incomplete`  | Safe matches are available, but candidate inspection or a requested projection is ambiguous or incomplete.                                                  |
| `invalid`     | Command input or one filter, region, view, or projection value is invalid.                                                                                  |
| `blocked`     | The command cannot establish the selected workspace or a safe source-universe boundary.                                                                     |
| `failed`      | An unexpected internal failure prevents normal completion.                                                                                                  |
| `interrupted` | The caller cancels or interrupts the operation before completion.                                                                                           |

- A complete search with zero matches is successful. Compact
  human output states `result=complete`, `coverage=complete`, the default or
  filtered `universe`, and `matches=0`, followed by `No matches.` Expanded output
  states `Matches: 0`. JSON returns an empty match array with complete coverage.
- An incomplete search may emit independently verified safe
  matches. Compact mode starts with `result=incomplete` and
  `coverage=incomplete`, marks the default or filtered `universe`, writes safe
  rows to stdout, and adds a concise required finding and next action. The
  semantic result remains non-success so automation cannot mistake the rows for a
  complete set. Expanded and JSON output include complete typed findings.
- `attention` never hides a candidate that might match. Any
  uncertainty that can change the result set is `incomplete` or `blocked`.
- The exact numeric process-status mapping is defined by the
  [CLI Architecture](../../architecture.md#result-json-coordinates-and-process-status). The semantic result names and
  conditions above remain the public `find` meanings.

## Errors

- Every error names the `find` operation, affected query value,
  source, or boundary, states the direct cause, and gives a useful next action
  when one exists.
- A missing predicate value is invalid.
- An invalid tag grammar is invalid.
- An empty heading value is invalid.
- A comma-separated tag list is invalid and recommends repeating
  `--tag`.
- A repeated `--require`, invalid `--view` value, or malformed
  list grammar is invalid.
- `--view` with `--json` is accepted as a no-op under the shared
  global-flag contract.
- `--require` or `--within` without a predicate is invalid.
- An entirely incompatible predicate and region selection is
  invalid.
- A missing, unavailable, or non-directory workspace is blocked.
- An unsafe filesystem identity or containment boundary is
  blocked when the source universe cannot be established safely.
- An unreadable, non-UTF-8, orphaned, or otherwise
  uninspectable candidate makes coverage incomplete when safe results remain
  available.
- `doctor` owns complete diagnosis and recommendations. `find`
  reports only the facts needed to establish its result and coverage.

## Complete Usage Scenarios

The following scenarios preserve the complete representative usage set from the
authoritative source. They demonstrate omission, every finite predicate and view
choice, region and content composition, another workspace, and structured output
without enumerating every compatible Cartesian combination.

### Flat Inventory

```text
open-forge find
```

Returns every logical Markdown source below `.agents` in the default expanded
form with workspace, coverage, count, and source identity.

### One Tag

```text
open-forge find --tag=Architecture
```

Matches the complete authored frontmatter tag value while ignoring case.

### Tag With A Hash And Different Case

```text
open-forge find --tag="#architecture"
```

Matches authored `Architecture`, `architecture`, or another case variant and
preserves each authored spelling in expanded evidence.

### Require Several Tags

```text
open-forge find \
  --tag=Memory \
  --tag=CurrentTruth \
  --tag=Architecture
```

Returns sources that contain all three tags.

### Match Any Tag

```text
open-forge find \
  --tag=Architecture \
  --tag=Principles \
  --tag=Vision \
  --require=any
```

Returns sources that contain at least one requested tag.

### One Heading

```text
open-forge find --heading=Axioms
```

Returns sources containing a parsed structural heading whose complete visible
text equals `Axioms` while ignoring case.

### Heading With Spaces

```text
open-forge find --heading="Current State"
```

Quotes keep the complete heading value in one shell argument.

### Require Several Headings

```text
open-forge find \
  --heading="Current State" \
  --heading="Next Steps" \
  --heading=Blockers
```

Returns sources containing all three headings.

### Match Any Heading

```text
open-forge find \
  --heading=Axioms \
  --heading=Instructions \
  --require=any
```

Returns sources containing either heading.

### Tag And Heading

```text
open-forge find \
  --tag=Directive \
  --heading=Instructions
```

Returns sources with the Directive frontmatter tag and an Instructions heading.

### Several Tags And Headings

```text
open-forge find \
  --tag=Memory \
  --tag=Working \
  --heading="Current State" \
  --heading="Next Steps"
```

Returns sources satisfying every tag and heading predicate.

### Any Tag Or Heading

```text
open-forge find \
  --tag=Architecture \
  --tag=Principles \
  --heading=Axioms \
  --heading=Instructions \
  --require=any
```

Returns sources satisfying at least one of the four predicates.

### Search Body Tags

```text
open-forge find \
  --tag=Architecture \
  --within=body
```

Searches exact visible bare-tag tokens in parsed body text instead of
frontmatter tags.

### Search The Complete Authored Document

```text
open-forge find \
  --tag=Architecture \
  --within=document
```

Searches frontmatter tags and body tag tokens.

### Search One Section

```text
open-forge find \
  --tag=Architecture \
  --within="section:Current State"
```

Searches body tag tokens only within the exact parsed Current State section.

### Search Several Regions

```text
open-forge find \
  --tag=Architecture \
  --within="frontmatter,section:Current State"
```

Searches the frontmatter tag list and the named body section.

### Compact Output From Another Workspace

```text
open-forge find \
  --tag=CurrentTruth \
  --view=compact \
  --workspace ../another-workspace
```

Uses the exact selected workspace and emits compact ID-and-path rows.

### Expanded Match Evidence

```text
open-forge find \
  --tag=Directive \
  --heading=Instructions \
  --view=expanded
```

Shows workspace, effective filters, locations, coverage, counts, descriptions,
and match evidence.

### Return Exact Sections

```text
open-forge find \
  --tag=Directive \
  --heading=Instructions \
  --content=section:Instructions
```

Returns the exact Instructions section from each matched source.

### Return Several Possible Sections

```text
open-forge find \
  --heading=Axioms \
  --heading=Instructions \
  --require=any \
  --content=section:Axioms,section:Instructions
```

Returns each requested section that exists and reports the missing projection
for matched sources that contain only the other heading.

### Structured Result

```text
open-forge find \
  --tag=Memory \
  --tag=CurrentTruth \
  --heading=Scope \
  --json
```

Returns one structured result with the complete query, coverage, ordered source
matches, evidence, and findings.

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
- Verify bare inventory in the default expanded view and
  explicit compact ID-and-path output.
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
- Verify compact, expanded, content-projected, verbose, and
  structured output from one typed result.
- Verify source ID collisions, canonical result ordering, layer
  ordering, evidence ordering, and stable repeat invocation.
- Verify complete matches, complete zero matches, attention,
  incomplete safe matches, invalid input, blocked boundaries, failed execution,
  and interruption.
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
- The typed structured result exposes the requested human view
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
  workspace-relative path of its base file. Compact output emits that base ID and
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
- Every expanded and structured layer record identifies whether
  it is the `base` or `overwrite` layer.
- Every expanded and structured layer record includes that
  layer's canonical physical workspace-relative path.
- Every expanded and structured layer record includes its match
  region and source location.
- Every matched tag or heading in an expanded or structured layer
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
  1-based line, and occurrence count. The shared CLI Architecture defines the
  exact column and source-span representation; each occurrence retains the
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
- Expanded human results echo the supplied include and exclude
  occurrences, their resolved selector identities, whether the default or
  filtered universe was used, and the effective candidate, inspected, and
  matched counts. The selector detail appears in the source-universe portion of
  the result, not as predicate evidence.
- JSON results expose the same supplied and resolved selector
  facts, default-or-filtered universe state, and effective candidate, inspected,
  and matched counts in the complete typed result. A well-formed `--view` remains
  an accepted no-op with JSON under the shared global contract.
- Compact output visibly marks the universe as `default` or
  `filtered` in its summary line. A filtered compact result does not repeat the
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

  The effective universe is empty, so the complete result reports zero
  candidates and zero matches.

- Presentation exposes the filter state at the appropriate
  density:

  ```text
  open-forge find \
    --include=memory/crystallized/documents \
    --view=compact

  open-forge find \
    --include=memory/crystallized/documents \
    --json \
    --view=compact
  ```

  Compact output marks `universe=filtered`. JSON echoes supplied and resolved
  selectors, and the well-formed `--view` is accepted as a no-op.

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
- Verify expanded and JSON selector echo, default-or-filtered
  state, effective candidate/inspected/matched counts, compact filtered marking,
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
