# Open Forge CLI

The accepted replacement Open Forge CLI design specifies one future production
executable for an optional, stateless, deterministic, and idempotent native tool
for the human-readable Framework. Its development implementation contains
read-only and mutating commands, but it has no
accepted shipping executable and is not released. This document summarizes its
accepted command interface. The accepted shared
implementation choices are defined in the [CLI Architecture](../.agents/memory/crystallized/documents/cli/architecture.md);
the linked command contracts define exact public behavior.

The frozen TypeScript MVP is available as `open-forge-old`. Its interface is
separate from the replacement CLI.

An `entrypoint` is a Markdown file that makes its folder routable.
A `route` is a navigable path exposed through entrypoints. Generated `Entries`
are navigation lines for direct routed files and child entrypoints. A `Template`
is reusable source content used to start an independently maintained artifact.
`Axioms` are required rules from the Loader or a recognized entrypoint. Loaded
descendants inherit them from their ancestors.

## Framework Lifecycle

The accepted Framework lifecycle has two direct root operations:

```text
open-forge install [--force] [--automatic] [--dry-run] [global flags]
open-forge update [--force] [--prune] [--automatic] [--dry-run] [global flags]
```

`install` establishes management in one exact workspace or verifies an exact
trusted managed no-op. It does not reconcile managed divergence. A changed,
missing, retired, or source-divergent managed state is `blocked` and directs the
caller to `open-forge update`. Initial `--force` may replace only an eligible
exact current occupant before management is established. It does not adopt the
occupant's old bytes, bypass ownership, or become update authority.

The running CLI carries the closed canonical `src/open-forge/` distribution as
ordinary .NET embedded resources. Root Install reads that neutral exact-byte
inventory; it never reads the development checkout. Install owns the closed base
Framework subset. A trusted lifecycle section may also contain dynamically added
scoped Framework targets from `route init --framework`; Install validates and
preserves those records but does not select them as root effects or treat their
presence as root divergence.

A prompt-capable human Install that would write asks once after complete
preflight and before the workspace lease or any effect. Refusal, end of input,
or cancellation is a no-write `interrupted` result. Dry-run, verified no-op,
`--automatic`, JSON, and redirected invocations never prompt. A redirected human
application that would write is `invalid` unless `--automatic` is explicit.
Automatic adds no force or safety authority.

Install plans missing directories separately from file effects. The workspace
lease uses a persistent zero-byte file in the current user's application-owned
`LocalApplicationData/OpenForge/locks/v1` catalogue; it does not require or
create workspace content. Under the held lease, Install revalidates and creates
`.agents` as the first ordinary planned directory effect when it is missing,
then creates only missing descendants parent-first with ordinary .NET filesystem
APIs and verifies them. A created directory remains as reported residual state
after contention or a later failure; it has no recovery entry and is not rolled
back, compensated for, or removed. Recovery bundles use the separate
`LocalApplicationData/OpenForge/recovery/v1` catalogue.

`update` requires trusted existing Framework lifecycle state. Normal mode applies
baseline-unchanged and genuinely new safe content and preserves changed, missing,
and retired divergence as `attention`. `--force` replaces or restores only
changed or missing current expected content. `--prune` deletes only eligible
retired managed content. `--force --prune` composes those two named boundaries.
Automatic mode suppresses interaction but adds no package, replacement, deletion,
adoption, ownership, or safety authority.

Both operations use one complete baseline/current/intended plan, current Index
projection, and one external recovery bundle covering every existing-target
effect (`Replace`, `ReplaceGeneratedRegion`, or `Delete`). An operation containing only creates and no-ops
creates no bundle. Only a
semantically verified final ZIP forms preparation, and all preparation completes
before the first target effect. Handled failure or cancellation reports the
actual residual draft or final path without restoration, rollback, compensation,
or recovery-derived current-target classification. The persistent workspace lock
preserves its bytes and is owned only by a `FileShare.None` handle. There is no Framework group, root
`init`, reinstall/replace/restore/recover alias, Framework uninstall/remove
leaf, generic apply, saved plan, or semver source update.

The replacement's only lifecycle document is:

```text
.agents/open-forge.lifecycle.json
```

It uses schema version 1 with one common envelope and isolated `framework` and
`extensions` sections. Each lifecycle operation reads and writes only its own
section while preserving the common envelope and unrelated section. Supported
parseable kinds use the accepted syntax-aware semantic fingerprint; exact bytes
remain fresh operation-time facts. The replacement does not execute a formatter
or persist formatter state. Every Framework target has required nullable
`sourceAssetPath`: payload files and managed root/provider blocks identify their
canonical embedded source asset, while derived generated regions use `null`.
Schema v1 gains no instance collection or migration engine.

The replacement does not read, recognize, migrate, alias, or fall back to an old
lifecycle or Extension file, including `open-forge.extensions.json`. Old-format
files remain untouched ordinary workspace content outside replacement authority.
The [CLI Architecture](../.agents/memory/crystallized/documents/cli/architecture.md)
defines the exact lifecycle, parser, serialization, filesystem, recovery, and
Native AOT choices. Gate 5 must provide their executable proof.

See the [Install contract set](../.agents/memory/crystallized/documents/cli/contracts/install/_install.md),
[Install Interface](../.agents/memory/crystallized/documents/cli/contracts/install/interface.md),
[Update contract set](../.agents/memory/crystallized/documents/cli/contracts/update/_update.md),
[Update Interface](../.agents/memory/crystallized/documents/cli/contracts/update/interface.md),
and [Update Behavior](../.agents/memory/crystallized/documents/cli/contracts/update/behavior.md)
for the complete contracts.

## Cleanup

The non-shipping root `cleanup` operation forms one operand-free, default-all
catalogue of positively recognized Open Forge recovery bundles and drafts
associated with the selected workspace:

```text
open-forge cleanup [--dry-run] [global flags]
```

Bare `cleanup` has no operands, IDs, paths, selectors, wizard, prompt, or
confirmation flow. It forms one current catalogue of exact deterministic final
and draft names directly under the selected workspace bucket in the current
user's `LocalApplicationData/OpenForge/recovery/v1` directory. A final ZIP must
pass semantic manifest/schema, exact ordered entry, length, hash, and payload-
byte validation; a draft is `Incomplete` support data. Unknown names and content
outside that bucket remain untouched. The operation does not recursively scan
the workspace, extract a bundle, restore a target, or create a replacement
bundle for its own deletion.

`--dry-run` uses the same catalogue, ordering, plan, and preflight as application,
writes nothing, and acquires no lease. Every proposed deletion remains contingent
on application acquiring the persistent same-workspace `WorkspaceLockLease`
through `FileShare.None`, re-enumerating the selected bucket once, and repeating
exact path/kind and final semantic validation while holding that lease. Cleanup
then uses ordinary file deletion and verifies absence. Lease contention causes
no deletion.

With an empty catalogue, cleanup returns a verified complete no-op without
acquiring a lease or prompting. It makes no activity inference and writes no
marker, PID, journal, or lock metadata. Each repeat forms a fresh catalogue.
Once deletion begins, verified deletions remain valid
effects: cleanup creates no replacement bundle, receipt, journal, or tombstone
merely for this support-artifact deletion. Partial failure or interruption
reports every deleted and remaining artifact so a later invocation can converge.
Human and structured results use the shared seven statuses and streams;
recognized-artifact deletion failure is reported with exact residual state and
guidance.

See the [cleanup Interface](../.agents/memory/crystallized/documents/cli/contracts/cleanup/interface.md)
and [cleanup Behavior](../.agents/memory/crystallized/documents/cli/contracts/cleanup/behavior.md)
for the complete current contracts.

## Extension Operations

The accepted Extension family is grouped under one subject with six actual
operations:

```text
open-forge extension list [--installed] [--available] [--source <package-or-catalogue-path>] [global flags]
open-forge extension inspect <stable-id> [--source <package-or-catalogue-path>] [global flags]
open-forge extension create [<stable-id>] [--path <catalogue-path>] [--name <text>] [--description <text>] [--package-version <text>] [--dependency <stable-id>]... [--automatic] [--dry-run] [global flags]
open-forge extension install [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--automatic] [--dry-run] [global flags]
open-forge extension update [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--prune] [--automatic] [--dry-run] [global flags]
open-forge extension remove [<stable-id>...] [--prune] [--automatic] [--dry-run] [global flags]
```

The bare group shows help and performs no operation or wizard. `list` and
`inspect` are read-only. `create` writes only
`<catalogue>/<id>/extension.json` and `payload/.agents/` under its distinct
`--path` destination; `--workspace` is a no-op for create. Create uses a
separate exact-destination, collision, and revalidation path with no workspace
lease, none of `Replace`, `ReplaceGeneratedRegion`, or `Delete`, and no recovery
bundle.

Prompt-capable human Create asks only for required facts not supplied explicitly:
stable ID and destination catalogue parent. Blank or invalid input may be
explained and asked again locally without an attempt limit. End of input is a
no-write `invalid` result; cancellation is no-write `interrupted`. JSON,
automatic, and redirected invocations never prompt and must provide both inputs.
Omitted manifest values are deterministic: the name is the
hyphen-split ID with each segment's first ASCII letter uppercased, description is
`Open Forge Extension package <stable-id>.`, version is `0.1.0`, and dependencies
are empty. The four manifest options override only those values; dependencies
are validated, reject duplicates and self-reference, and serialize in ordinal ID
order without availability resolution.

The catalogue parent may be any existing safely resolved directory, including an
empty marker-free directory. Create never creates that parent and does not inspect
or classify unrelated siblings; only `<catalogue>/<id>` determines the package
collision/no-op result. Its command-local JSON result orders catalogue,
destination, ID, manifest, mode, intended/applied effects, verification, and
`workspaceLifecycleChanged: false` without repeating shared envelope fields.

Install and update use the embedded catalogue or one exact external package or
catalogue source. The source is read-only and must be lexically and physically
disjoint from the target workspace. Dependencies resolve offline and
transitively within one source universe. `--automatic` never means `--all` and
never selects force or prune.

Installed and available facts remain separate. New-CLI installed facts are
classified as `absent`, `trusted`, `untrusted`, or `incomplete`; unsafe ambiguity
is `blocked` under the contracts. Source unavailability preserves safely readable
installed facts but cannot form an update plan or promote lifecycle trust.
Managed ownership requires trusted lifecycle evidence; matching paths, bytes,
fingerprints, or route placement never adopt content. Remove releases trusted
selected ownership, retains shared files, deletes safe unchanged final-owner
files, preserves changed final-owner files by default, and permits their deletion
only with same-request `--prune`. It never removes the package source or
Framework-owned content.

All workspace-mutating Extension operations use one complete plan, dry-run
parity, and one external recovery bundle covering every existing-target effect
(`Replace`, `ReplaceGeneratedRegion`, or `Delete`),
plus expected-state revalidation and verification. All preparation completes before
the first target effect; handled failure or cancellation reports the actual
residual draft or final path without restoration, rollback, compensation, or
current-target classification. Whole-command
success removes the recognized bundle only after final verification; cleanup
failure is `attention`. The seven statuses and human/JSON stream rules are
shared. All six Extension commands are implemented in the development CLI;
the replacement CLI remains unreleased.

See the [Extension documentation](extensions.md) and the
[Extension contract group](../.agents/memory/crystallized/documents/cli/contracts/extension/_extension.md)
for complete details.

## Status

`status` gives a quick, read-only summary of the selected workspace:

```sh
open-forge status [global flags]
```

The applicable global flags are `--workspace <path>`, `--json`,
`--view=compact|expanded`, `--verbose`, `--help`, and `--version`. The command
uses the exact current directory unless `--workspace` selects another exact
directory. It does not search parent directories or infer a workspace from
nearby files. Primary human `complete`, `attention`, and `incomplete` results use
stdout. Primary human `invalid`, `blocked`, `failed`, and `interrupted` results
use stderr. `--json` writes one complete result to stdout for every status;
separate bounded diagnostics use stderr.

It compares startup context from the Framework shipped in the running CLI with
startup context from the current workspace. It also reports total available
context, context that may load again at continuity boundaries, root-category
customization, managed Extensions, and recognized recovery bundles.

### Context Measurements

The startup table uses the following rows:

- `Initial (shipped)` is startup context resolved from the Framework embedded in
  the running CLI. It is not a saved installation snapshot.
- `Current workspace` is startup context resolved from the selected workspace
  now.
- `Difference` is current minus initial for every metric.

Startup context contains the workspace entry, Loader, visible `#LoadNow`
closure, and applicable `#KeepInMind` continuity context. Total available
context contains every current context file, including startup and on-demand
files.

For this inventory, a context file is the ordinary UTF-8 `AGENTS.md` entry or an
ordinary UTF-8 Markdown file below `.agents`. Overwrite companions count when
their base exists. Provider bridges, files reached only through ordinary links,
non-Markdown support files, receipts, and recovery bundles do not count as
context.

Continuity context is the part that may load again after a handoff, context
restoration, before closeout, or at another defined continuity boundary. It does
not mean that the content loads on every model request.

Measurements use:

- Exact physical file count
- Exact Unicode character count
- Exact UTF-8 size
- A deterministic token estimate of approximately one token per four characters

Estimated tokens are marked with `~`. They are not model-specific tokenizer,
billing, latency, or context-window values.

Numeric-numeric differences are signed, including zero. Unavailable and
not-applicable values are not rendered as zero. Startup percentage is numeric
only with numeric byte operands and a positive total; both zero bytes are
not-applicable, while a positive current value with zero total is incomplete.

### Workspace Structure

Root categories are the direct root routes exposed by the Loader. `Added
categories` exist in the current workspace but not the Framework shipped in the
running CLI. `Removed categories` exist in that shipped Framework but not the
current workspace. These are neutral customization facts, not health conditions.

Status reads `.agents/open-forge.lifecycle.json`, schema v1, as isolated
Framework and Extension sections. Each section reports `absent`, `trusted`,
`untrusted`, or `incomplete` facts without merging authority; unsafe ambiguity is
`blocked`. The replacement does not read old-format lifecycle or Extension files.

`Extensions` counts distinct installed IDs from safely readable lifecycle facts.
An absent section shows `0 recorded` Extensions, while a trusted empty section
shows `0`; both show `none recorded` managed files without changing status.
Untrusted, incomplete, or unavailable facts are not converted to trusted empty
counts.
Installed IDs, ownership, and recorded paths remain reportable when package
source bytes are unavailable, while source-dependent comparison is incomplete.
Matching paths, bytes, or fingerprints never establish ownership. Managed files
are counted once as current, changed, or missing relative to readable lifecycle
facts. Recovery reporting includes exact named final ZIPs with semantic integrity
conditions and exact named drafts as `Incomplete` support data.

The expanded result shows at most three of the largest continuity sources, and
compact omits that section. JSON carries every contribution in deterministic
order. The list ranks sources by exact UTF-8 contribution, then by source ID when
sizes match. It reports size, not importance or a recommendation.

### Example

```text
Open Forge status
Workspace: <workspace-path>
Selected by: current directory
Result: requires attention

Startup context
                         Files   Characters      Size   Est. tokens
Initial (shipped)           14       18,420  18.0 KiB        ~4,600
Current workspace           21       33,960  33.2 KiB        ~8,500
Difference                  +7      +15,540 +15.2 KiB       ~+3,900

Total available context
  205 files · 1,516,560 characters · 1.45 MiB · ~379,100 tokens
  Startup context: 2.2% of total available context

Continuity context (may load again)
  4 files · 6,120 characters · 6.0 KiB · ~1,500 tokens

Largest continuity sources
  2.4 KiB   memory/working/checkpoints
  1.9 KiB   directives/public-facing-writing
  1.1 KiB   memory/crystallized/documents

Workspace structure
  Root categories:    8
  Added categories:   workspace
  Removed categories: templates
  Extensions:         1
  Managed files:      9 current, 11 changed, 1 missing
  Recovery bundles:   0
```

The values are illustrative. A larger context or added or removed category does
not produce `attention` by itself. Changed or missing managed files and
recognized recovery bundles do. Human output renders this semantic status as
`requires attention`; JSON keeps the value `attention`. Complete structural
diagnosis and recommendations belong to `doctor` rather than `status`. Status
does not list repair or lifecycle proposals.

Compact output uses one operation-level `Next:` only when useful: no line for
`complete`; `open-forge doctor` for `attention`; Doctor for `incomplete` unless a
more direct safe correction is known; `Next: correct the named input` for
`invalid`; `Next: correct the named workspace or safety boundary and rerun` for
`blocked`; `Next: report the failure and retry with bounded diagnostics` for
`failed`; and `Next: rerun the same request` for `interrupted`.

For a safely established uninstalled workspace, the result can be `complete`.
The initial shipped measurement remains measured when the embedded payload is
available. Current startup, Difference, startup percentage, continuity, and
root-category facts are not-applicable, while total available physical context
remains numeric when safely measurable. An applicable fact that cannot be
measured is unavailable and makes the result incomplete.

`status` does not build the complete content graph of route, link, and section
relationships, validate every route or link, count scopes by guessing their
meaning, or modify anything. JSON output contains the same structured facts as
the human result. See the
[status contract set](../.agents/memory/crystallized/documents/cli/contracts/status/_status.md)
for the complete interface.

## Context

`context` retrieves selected ordered content without inference, sessions, or
mutation:

```text
open-forge context [source-reference...] [--additions-only]
  [--content=<part>[,<part>...]] [--follow-links=<positive-depth|all>]
  [global flags]
```

With no source reference it returns the startup-required closure. Explicit
references add their current route or exact-path closures, and
`--additions-only` returns only sources added beyond startup. Repeating
`--additions-only` is idempotent. Repeating `--follow-links` or `--content` is
invalid; repeated parts in one `--content` value are idempotent. The default
content is authored `frontmatter` and `body`. Projection order is stable
regardless of flag order: operation-level `paths` first when selected, then
resolved sources and physical layers, then metadata, frontmatter, headings,
body, and requested sections in document order. Authored bytes remain exact.

`--follow-links` expands contained local Markdown links to the requested depth.
External HTTP/HTTPS URLs are reported as unchecked, never fetched or selected,
and do not make an otherwise complete result incomplete. A fully resolved empty
additions result and an empty heading outline are complete. Unsafe or ambiguous
source or local-target identity, containment, and overwrite boundaries are
blocked; missing or unreadable required content is incomplete; safe observations
such as a proven absent section require attention. Human
complete/attention/incomplete results use stdout, other primary human statuses
use stderr, and JSON writes one structured result to stdout for every status
with bounded diagnostics on stderr.

`status` measures the no-source closure but does not render it. `find` discovers
inventory and predicates, `references` reports direct edges without content,
`route inspect` profiles one route without authored content, and `doctor`
diagnoses. `context` retrieves the selected ordered content. See the
[Context contract set](../.agents/memory/crystallized/documents/cli/contracts/context/_context.md)
for the complete contract.

## Find

`find` returns a flat inventory of Markdown sources below `.agents`. Tag and
heading predicates filter that same inventory:

```text
open-forge find
  [--include=<source-reference>]...
  [--exclude=<source-reference>]...
  [--tag=<tag>]...
  [--heading=<heading>]...
  [--require=all|any]
  [--within=<part>[,<part>...]]
  [--view=compact|expanded]
  [--content=<part>[,<part>...]]
  [global flags]
```

Bare `find` lists every eligible logical source in the normal complete `.agents`
Markdown universe. Omitted `--include` starts from that universe, and omitted
`--exclude` subtracts nothing, so an invocation with neither flag behaves as
before. Source filtering is valid without predicates and happens before tag or
heading matching.

### Source-Universe Filters

`--include` and `--exclude` are repeatable Find-specific source-universe filters:

```sh
open-forge find --include=memory/crystallized/documents
open-forge find \
  --include=memory/crystallized/documents \
  --include=skills/experience-design \
  --exclude=memory/crystallized/documents/architecture
```

Each occurrence accepts exactly one shared source reference. Repeated includes
and excludes form unions, and exclusion wins overlap regardless of argument
order. A Loader, recognized entrypoint, or `SKILL.md` reference expands to all
eligible Markdown physically below its folder, including unrouted sources. An
ordinary source selects one logical source; a valid base/overwrite pair is
selected or excluded together. Expansion does not follow routes or links and
does not infer authority or lifecycle.

The shared source-reference ID and exact `.agents/...` path grammar applies.
Comma lists, globs, arbitrary directories, positional operands, and new
qualifier syntax are not accepted. `--require` and `--within` do not control
source filtering.

`--exclude` removes sources from the effective universe before inspection, so
excluded areas need not be parsed. A valid filtered universe with zero
candidates or zero matches is still complete. Expanded and JSON results report
the supplied and resolved selectors, the default or filtered universe, and
effective candidate, inspected, and matched counts. Compact output marks the
filtered state without repeating the full selector detail.

The command does not follow links, build the complete context graph, or use a
persistent search index.

### Tags And Headings

`--tag` matches a complete authored frontmatter tag value or visible body tag
token while ignoring case. A query may include one optional leading `#`:

```sh
open-forge find --tag=Architecture
open-forge find --tag="#architecture"
```

Both match authored `Architecture`, `architecture`, or another case variant.
The result preserves authored spelling. Longer tags such as
`ArchitectureNotes` do not match. Tag matching does not normalize Unicode,
correct spelling, remove accents, or perform fuzzy or semantic matching.

`--heading` matches the complete visible text of a structural Markdown heading
while ignoring case:

```sh
open-forge find --heading=Axioms
open-forge find --heading="current state"
```

The accepted Markdown parser supplies CommonMark ATX and Setext heading nodes.
Parser extensions do not add other public heading forms unless a later
compatibility decision names them. Open Forge still authors canonical semantic
sections with ATX headings. Parser recognition alone does not make a heading
canonical or semantically active.

Repeat one scalar flag for each predicate. Commas in headings remain literal,
and comma-separated tag lists are invalid:

```sh
open-forge find --tag=Memory --tag=CurrentTruth
open-forge find --heading="Current State" --heading="Next Steps"
```

### Predicate Logic

The default is `--require=all`. Every tag and heading predicate must match the
same logical source:

```sh
open-forge find \
  --tag=Directive \
  --heading=Instructions
```

Use `any` for a flat union:

```sh
open-forge find \
  --tag=Architecture \
  --tag=Principles \
  --heading=Axioms \
  --require=any
```

The predicate logic has no grouping, predicate negation, precedence, regular
expressions, or query language. Source-universe exclusion remains a separate
explicit filter.

### Search Regions

`--within` controls where predicates are evaluated:

| Part             | Search region                                                                               |
| ---------------- | ------------------------------------------------------------------------------------------- |
| `document`       | Complete authored document: frontmatter plus body                                           |
| `frontmatter`    | Parsed Open Forge frontmatter                                                               |
| `body`           | Parsed Markdown after frontmatter                                                           |
| `section:<name>` | Parsed body section under a structural heading whose complete visible text matches `<name>` |

Without `--within`, tags search frontmatter and headings search the body. Use an
explicit region to search body tags or one exact section:

```sh
open-forge find --tag=Architecture --within=body
open-forge find --tag=Architecture --within=document
open-forge find --tag=Architecture --within="section:Current State"
open-forge find --heading=Decision --within=section:History
```

Several regions form a union and use the same comma and escape grammar as
`--content`:

```sh
open-forge find \
  --tag=Architecture \
  --within="frontmatter,section:Current State"
```

### Output

The expanded human view is the default. Compact view emits one tab-separated ID
and canonical path per result, preceded by a summary line:

```text
result=complete	coverage=complete	universe=default	matches=2
directives/public-facing-writing	.agents/directives/public-facing-writing.md
directives/security	.agents/directives/security.md
```

A filtered compact result uses `universe=filtered` in that summary line without
repeating the full selector detail.

Use `--view=expanded` for the workspace, query, coverage, descriptions, and
match evidence:

```text
Workspace: <workspace-path>
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
Matches: 2

directives/public-facing-writing
  Path: .agents/directives/public-facing-writing.md
  Description: Write clear, consistent user communication and source prose
  Matched:
    Directive — frontmatter, base
    Instructions — heading, base, line 9

directives/security
  Path: .agents/directives/security.md
  Description: Apply required workspace security boundaries
  Matched:
    Directive — frontmatter, base
    Instructions — heading, base, line 11
```

`--content` independently selects result content through the values
`metadata`, `frontmatter`, `headings`, `body`, and `section:<name>`.

```sh
open-forge find \
  --tag=Directive \
  --heading=Instructions \
  --content=section:Instructions

open-forge find \
  --heading=Axioms \
  --heading=Instructions \
  --require=any \
  --content=section:Axioms,section:Instructions
```

`--within` controls matching, `--view` controls human match details,
`--content` controls result content, and `--verbose` adds diagnostics. JSON
returns the complete structured result. A well-formed `--view` with `--json` is
accepted as a no-op because it applies only to human output. `paths` is invalid
for Find.

A complete search with no matches succeeds and prints `No matches.` in compact
human output. Incomplete coverage may return independently verified safe
matches, but it reports that additional matches may exist and returns a
non-success semantic result.

See the
[find contract set](../.agents/memory/crystallized/documents/cli/contracts/find/_find.md)
for the complete interface and additional examples.

## Routes

The `route` group contains separate operations for inspection, route chains,
ordinary routed Markdown files, and bounded updates. Running `open-forge route`
shows help and performs no mutation.

The `route init`, `route create`, and `route update` write operations support
`--dry-run`. They use the exact current directory or `--workspace`, plan
automatic generated `Entries` changes in the same mutation, and never run a
hidden `index` subprocess.

Those three metadata/scaffolding write commands use the same interface shape.
Singleton one-value flags reject repetition, including equal values. Explicitly
multi-value `--tag` flags keep each command's local order, replacement, and
duplicate rules.
Applicable `--dry-run` flags repeat idempotently and do not add authority. Each
command uses `complete`, `attention`, `incomplete`,
`invalid`, `blocked`, `failed`, and `interrupted`; ordinary conditions use
`blocked` > `incomplete` > `attention` > `complete`, invalid input stops before
operation resolution, and failed or interrupted results retain their event
meaning. These are seven semantic statuses. Human complete/attention/incomplete results use stdout. Human
invalid/blocked/failed/interrupted results use stderr. JSON writes one complete
typed result to stdout for every status, with bounded diagnostics on stderr.
Compact and structured results retain at most one required `Next:` action.

### Inspect A Route

`route inspect` explains one known source's route behavior without returning its
authored content:

```text
open-forge route inspect <source-reference>
  [global flags]
```

The exact public shape is `open-forge route inspect <source-reference> [global flags]`.

The command reports:

- Whether the source is read at task start or resume.
- The parent or event that can cause it to be read automatically.
- Whether it may be read again after context restoration, before handoff or
  closeout, or after a change that may affect its follow-up work.
- Its own physical size and estimated tokens.
- Context added when the route is selected beyond task-start context.
- Descendant context below an entrypoint that is read automatically through
  `#LoadNow`.
- Root route, route chain, depth, parent, direct children, and descendants.
- Sources that contribute inherited `Axioms` (required rules from the Loader and
  ancestor entrypoints), whether the source defines local `Axioms`, and the
  overwrite relationship.

Routed entrypoints, routed leaves and native sources, accepted compatibility
entrypoints, valid overwrite pairs, detached entrypoints, and known supported
unrouted sources can be `complete`. A safely resolved non-unique automatic ID is
`attention` only when exact-path or interactive selection made the physical
source safe and the route meaning complete. Unreadable required sources,
incomplete route chains, and unmeasurable applicable facts are `incomplete`.
Orphan or ambiguous overwrites, ambiguous routes, unsafe identity, and
containment failures are `blocked`; zero or several operands, the Loader, and
unknown, missing, or unsupported sources are `invalid`. Failed and interrupted
events retain their own statuses.

Primary human `complete`, `attention`, and `incomplete` results use stdout.
Primary human `invalid`, `blocked`, `failed`, and `interrupted` results use
stderr. `--json` writes one complete structured result to stdout for every
status, with bounded diagnostics on stderr and no human text mixed into JSON.
Both human views retain workspace, selection method, source ID, and canonical
path.

Compact output keeps ID and path, source and route state, route chain, reading
behavior, own/addition/`#LoadNow`-descendant measurements, applicable topology,
overwrite state, status, completeness, safety, and at most one required
`Next:` line. Measured zero, unavailable, and not-applicable remain distinct. A
zero-source selection addition and an entrypoint with no automatic descendants
are measured zero; an ordinary routed leaf's descendant measure and detached or
unrouted route-dependent facts are not-applicable. An applicable unmeasurable
fact is unavailable and incomplete.

`Next:` appears only when required: an interactive ID collision names the exact
path for non-interactive use; incomplete or structural blocked results name a
known direct safe correction or `open-forge doctor`; invalid input names the
source or input to correct; complete results have none. Failed and interrupted
results keep ordinary retry guidance. Exact-path collision results preserve the
non-unique-ID observation without inventing an action. `route inspect` emits no
route mutation, health, content-placement, or diagnostic recommendations.
An unresolved non-interactive ID collision is blocked, retains every candidate
path, and tells the caller to rerun with one listed exact path.

When both standard input and stderr are terminal-capable, a human ID collision
lists the canonical candidate paths on stderr and asks once. The answer may be
the one-based displayed number or one exact displayed path. An invalid answer or
end of input retains the same blocked collision; cancellation is interrupted.
JSON and redirected invocations never prompt.

Human output explains reading events in ordinary language. It does not replace
them with labels such as `target-sensitive` or `continuity boundary`:

```text
Route: memory/working/checkpoints
Path: .agents/memory/working/checkpoints/_checkpoints.md
Entrypoint: canonical

Reading behavior
  Read at task start or resume: yes
  Why: memory/working exposes it as context that should be revisited during the task
  May be read again: yes
  When: after context restoration, before handoff or closeout, or after a change that may affect its follow-up work

Context cost
  This source: 3.7 KiB · ~955 tokens
  Selecting this route adds: none; its required context is already read at task start
  Automatically read below it through #LoadNow: 1 file · 2.1 KiB · ~525 tokens

Route structure
  Parent: memory/working
  Route chain: memory → working → checkpoints
  Depth: 3
  Direct children: 7 files, 1 child entrypoint
  All descendants: 12 files, 2 descendant entrypoints

Rules and customization
  Axioms inherited from: loader → memory → working
  Local Axioms: none
  Overwrite: none
```

The values are illustrative. Context size never produces a health grade,
heaviness score, or recommendation. Human output omits character counts;
structured output retains the shared exact measurement record.

The command does not report a generic scope count. Scope is a narrowing role,
not a separate file type that can be counted safely from folders or tags. The
exact route chain, depth, children, and descendants provide the mechanically
reliable structure instead.

`route inspect` does not search, list workspace routes, return bodies or
sections, follow links, compare generated `Entries`, diagnose health, emit route
or content-placement recommendations, or modify anything. Use `find` to
discover sources, `context` to return content, `status` for the workspace
overview, and `doctor` for diagnosis. See the
[route inspect contract set](../.agents/memory/crystallized/documents/cli/contracts/route/inspect/_inspect.md)
for the complete interface.

### List Route Topology

`route list` enumerates the current authored routed topology without changing
files or rebuilding generated `Entries`:

```text
open-forge route list [source-reference]
  [--depth=<non-negative-integer|all>]
  [global flags]
```

With no operand, it selects every current root route mechanically exposed by the
exact Loader, including workspace-defined roots; the Loader itself is never a
row. Omitted depth is `1`, so each selected root and its direct routed children
are returned. `--depth=0` returns roots only, and `--depth=all` returns the
complete routed descendant closure. A selected entrypoint enumerates its
subtree, a routed leaf returns itself, and a detached tree requires explicit
entrypoint selection.

Rows come from current authored filesystem topology and source contracts, not
generated `Entries`. They include routed entrypoints, routed leaves, and routed
native sources such as `SKILL.md`; unrouted sources and overwrite companions as
independent rows are excluded. Compact output is an indented ID/path and exact
authored description/tag list. Expanded output adds parent, depths, kind, child
counts, and provenance; JSON retains all typed facts.

`route list` is structural topology, not flat search, route inspection, context
content, reference lookup, graph traversal, diagnosis, or repair. See the
[route list contract set](../.agents/memory/crystallized/documents/cli/contracts/route/list/_list.md)
for the complete interface and behavior.

### Initialize A Route Chain

`route init` creates every missing entrypoint in one route chain:

```text
open-forge route init <route-target>
  [--framework]
  [--description <text>]
  [--responsibility <text>]
  [--tag=<tag>]...
  [--dry-run]
  [global flags]
```

A target may be an ID for the intended folder under `.agents` or an exact
canonical entrypoint path:

```sh
open-forge route init memory/project-alpha/documents
open-forge route init \
  .agents/memory/project-alpha/documents/_documents.md
```

The first form maps to the second path. Every missing folder receives one
canonical `_{folder-name}.md` entrypoint. The command does not create the Loader
and does not use a Template.

If the final target already exists as a recognized compatibility entrypoint and
only ancestors are missing, its exact path is also accepted and preserved.

The fixed scaffold contains canonical frontmatter, a literal slug title, a
compact route description, the inherited `Axioms` sentinel, and a final valid
generated `Entries` region. Missing folders receive an honest draft description
and `NeedsAuthoring`. For the missing final target, supplying both an explicit
description and one or more tags suppresses automatic addition of
`NeedsAuthoring`; an explicitly supplied `NeedsAuthoring` tag remains present.
Optional metadata flags affect only that final target.

Existing canonical and accepted compatibility entrypoints remain in place.
`index.md`, `_index.md`, `references.md`, and `_references.md` count as existing
entrypoints when exactly one recognized entrypoint exists in the folder. New
entrypoints always use the canonical filename.

If the complete chain already exists, the command succeeds without writing.
Supplying metadata for an existing final target is invalid; use `route update`.

`--framework` instead treats the operand as one desired concrete Framework route
and reuses the embedded topology owned by root Install. The first segment is an
installed root route; exact case-sensitive canonical Framework segments must
align uniquely and remain in canonical order, while inserted ID-form segments
are scope labels converted to deterministic lowercase hyphenated slugs. Exact
`.agents/...` target paths are already concrete and are never slugged. The final
segment must be a canonical non-root Framework route, and root recreation,
reordering, ambiguous alignment, unsafe collisions, or absent/outdated trusted
Install state blocks before any write.

Framework mode creates only the requested sparse chain. Missing canonical
Framework entrypoints copy their exact embedded assets, missing inserted scope
entrypoints use the generic draft scaffold, and generated `Entries` are projected
for their concrete destinations. Scope entrypoints remain user-owned; only copied
canonical assets and derived generated regions enter Framework lifecycle. There
is no `install --route`, `--scope`, blueprint, or general Template engine, and
generic metadata flags are invalid with `--framework`.

In either mode, missing directories are separate effects. The external workspace
lease is acquired before any workspace effect. A generic plan reports and creates
missing `.agents` as its first ordinary directory effect; Framework mode requires
an existing trusted Install. Under the lease, remaining descendants are created
parent-first with immediate parent and target revalidation. A created directory
remains as reported residual state after contention or a later failure; it has no
recovery entry and is not rolled back or removed.
See the
[route init contract set](../.agents/memory/crystallized/documents/cli/contracts/route/init/_init.md)
for the complete target, scaffold, collision, and recovery behavior.

### Create A Routed File

`route create` creates one ordinary Markdown file below an existing routable
folder:

```text
open-forge route create <file-target>
  --description <text>
  --tag=<tag>...
  [--responsibility <text>]
  [--template <template-reference>]
  [--dry-run]
  [global flags]
```

The target may be an intended source ID or exact ordinary Markdown path:

```sh
open-forge route create \
  memory/crystallized/decisions/cache-policy \
  --description "Why the cache policy was chosen" \
  --tag=Memory \
  --tag=Decision
```

The parent folder must already have exactly one recognized entrypoint. The
command does not initialize missing folders or entrypoints.

Destination description and tags are required. Responsibility is optional;
`--responsibility ""` omits it. With no Template, the new source contains only
canonical destination frontmatter. `--template` accepts one routed Template ID
or exact path and copies its body as starting content:

```sh
open-forge route create \
  memory/crystallized/decisions/cache-policy \
  --template templates/memory/decision \
  --description "Why the cache policy was chosen" \
  --tag=Memory \
  --tag=Decision
```

The selected source must be ordinary routed Markdown with the exact canonical
`Template` tag. A Template with an overwrite companion blocks because this
operation has no accepted rule for collapsing two authored layers into one
destination body.

Template frontmatter describes the Template and never transfers to the
destination. The command does not substitute placeholders, store Template
provenance, or create a continuing update relationship. The created file is
independently maintained.

An identical existing target is a verified no-op. A different existing target
blocks instead of being overwritten. See the
[route create contract set](../.agents/memory/crystallized/documents/cli/contracts/route/create/_create.md)
for the complete Template, metadata, target, and recovery behavior.

### Update A Routed Source

`route update` patches selected metadata fields on one existing routed Markdown
file or entrypoint:

```text
open-forge route update <source-reference>
  [--description <text>]
  [--responsibility <text>]
  [--tag=<tag>]...
  [--template <template-reference>]
  [--dry-run]
  [global flags]
```

At least one metadata flag or `--template` is required. Supplied fields replace
their current values; omitted fields remain unchanged. Repeated `--tag` values
replace the complete tag list. `--responsibility ""` removes that key. There is
no `--no-responsibility` flag.

```sh
open-forge route update \
  memory/crystallized/decisions/cache-policy \
  --description "Accepted cache policy and the reasoning behind it" \
  --responsibility ""
```

When `--template` is supplied, the command copies the Template body only if the
target contains valid frontmatter followed by an empty or whitespace-only body.
Any authored non-whitespace body byte causes the complete target body to remain
byte-for-byte unchanged. Explicit metadata patches still apply:

```sh
open-forge route update \
  memory/crystallized/decisions/cache-policy \
  --template templates/memory/decision \
  --description "Why the cache policy was chosen"
```

Template frontmatter never changes destination metadata. Existing
compatibility entrypoint filenames and overwrite companions remain in place.
Description and tag changes update the exposing parent's generated entry in the
same operation. See the
[route update contract set](../.agents/memory/crystallized/documents/cli/contracts/route/update/_update.md)
for the complete field-patch, body-preservation, Template, and recovery
behavior.

### Move or remove a routed subject

The structural mutation commands are implemented as shallow grouped command
leaves in the development CLI, which remains unreleased:

```text
open-forge route move <source-reference> <destination-target>
  [--dry-run] [global flags]
open-forge route remove <source-reference>
  [--dry-run] [global flags]
```

Each command accepts exactly one eligible ordinary unmanaged logical leaf or one
eligible ordinary unmanaged category. A leaf is one ordinary routed Markdown
source with its valid overwrite companion, when present. A category is selected
through one recognized entrypoint source reference and is the complete
physically contained folder tree: its root entrypoint, overwrite companion,
descendant entrypoints and leaves, routed or unrouted Markdown, native or binary
resources, ordinary support files, and every other regular contained file and
directory. Every item must pass containment,
identity, ownership, lifecycle, collision, and recovery checks. A category is
one complete operation, not a batch of independently committed leaf commands.

Move uses an exact ordinary routed file target under an existing valid route for
a leaf. For a category, its exact destination entrypoint is inside a new category
folder whose parent is an existing valid route; it defines the new category root,
and descendant relative layout is preserved. The commands reject self-moves,
destinations inside the source, aliases, collisions, overwrite conflicts, unsafe
containment, and implicit parent initialization. Lifecycle-managed content is
not adopted or released. Unmanaged status requires complete trusted ownership
evidence from the Framework baseline and every applicable Extension receipt or
manager claim; a missing receipt, path, tag, generated entry, or matching bytes
is not proof.

Move scans all supported workspace Markdown, both inside and outside `.agents`.
It rewrites each exact supported local authored reference whose existing
destination would no longer resolve to the same intended target after the move,
including references into the moved subject and references from moved content to
targets outside it. It preserves labels, fragments, valid encoding, and unrelated
bytes. Internal links that remain valid are not rewritten, and external URLs are
unchanged. Generated `Entries` are projected from the post-move topology rather
than edited as authored references.

Remove performs the same complete reference pass. Each incoming exact supported
Markdown link from outside the removed subject is detached by replacing it with
its visible label as plain authored text. Surrounding prose is preserved, and
every detachment appears in dry-run and final human and JSON results. References
originating inside the removed subject disappear with it. Unsupported or
ambiguous references, prose-losing detachments, and incomplete coverage prevent
the operation; a supported broken link is never silently left behind.

Both commands form one complete plan, include the affected parent projections
(old and new for move, old for remove) and the Loader when applicable, revalidate
expected state, and verify every effect. One immutable, semantically verified
external recovery bundle covers every existing-target effect (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`) and is prepared before the first target
effect; creates and no-ops have none. `--dry-run`
is the only preview and shows every planned path, reference effect, generated
effect, and bundle disposition without writing. Handled failure or cancellation
reports the actual residual draft or final path without restoration, rollback,
compensation, or current-target classification. There is no `--force`,
`--automatic`, `--yes`, `--apply`, root
move/remove, or batch operand. Human results keep complete, attention, and
incomplete results on stdout and invalid, blocked, failed, and interrupted
results on stderr. JSON emits one complete structured result to stdout for every
status.

Repeating a move with its consumed old source is a non-mutating exact
`source-not-found`/`invalid` result, not a claimed no-op. Repeating a remove is a
verified no-op only when complete trusted ownership, topology, and reference
evidence proves the exact intended absence.

See the [route move contract set](../.agents/memory/crystallized/documents/cli/contracts/route/move/_move.md)
and [route remove contract set](../.agents/memory/crystallized/documents/cli/contracts/route/remove/_remove.md)
for the complete current interfaces and behavior.

### Route Write Safety

Across `route init`, `route create`, and `route update`, singleton value flags
reject repetition even when values match, explicitly multi-value `--tag` inputs
retain their command-local ordering and duplicate rules, and repeated `--dry-run`
flags are idempotent. These three commands use `complete`,
`attention`, `incomplete`,
`invalid`, `blocked`, `failed`, and `interrupted`, although a command emits
`attention` only for its finite local condition.

Primary human `complete`, `attention`, and `incomplete` results use stdout;
primary human `invalid`, `blocked`, `failed`, and `interrupted` results use
stderr. JSON emits one complete result to stdout for every status, while bounded
diagnostics use stderr.

Dry-run and application use the same intended state, generated projection,
safety checks, and expected-state facts. Dry-run shows every complete new file
and exact bounded existing-file diff, then writes nothing.

Application prepares and verifies one external immutable recovery bundle covering
the complete operation before any existing-target effect (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`). It does not permit overwrite, ambiguous
route selection, unsafe paths, invalid metadata, malformed generated boundaries,
or authored-body replacement. Creates and verified no-ops produce no bundle.

The commands recheck facts immediately before writing, verify each effect and
the complete route result, and preserve unknown, user-owned, concurrently
changed, or otherwise divergent content. A handled partial failure stops new
effects and reports the actual residual draft or final path; it never restores,
compensates for an earlier effect, or classifies current target state from
recovery provenance.

Dry-run and application use the same request, current facts, intended state,
generated projection, plan, preflight, and status conditions. Dry-run writes
nothing, and planned changes alone do not create `attention`.

The command-local `attention` boundaries are:

- `route init`: a newly planned entrypoint whose intended tags contain exact
  `NeedsAuthoring`. An unchanged existing marker does not change a complete
  no-op.
- `route create`: no current finite attention condition. The status remains in
  the uniform vocabulary but is unreachable until a condition is accepted.
- `route update`: an explicit Template is safely not applied because authored
  non-whitespace body content is protected. This includes a Template-only
  byte-level no-op and dry-run. The command never suggests overwriting the body.

## Index

In this section, an `entrypoint` is a Markdown file that makes its folder
routable.

`index` regenerates generated `Entries` from authoritative filesystem topology
and authored routing metadata:

```text
open-forge index [source-reference...]
  [--dry-run]
  [global flags]
```

The applicable global flags are `--workspace <path>`, `--json`,
`--view=compact|expanded`, `--verbose`, `--help`, and `--version`.

`--dry-run` is a Boolean flag. Repeating it is accepted and idempotent: a second
or later occurrence has no additional effect and does not multiply authority.
Value-bearing repetition follows the defining flag contract.

With no source operand, the command starts from the selected workspace's exact
`.agents/loader.md` and regenerates every reachable entrypoint region. It does
not use current generated lines to discover or order routed sources.

An entrypoint operand selects that entrypoint, every reachable descendant
entrypoint, and its direct exposing parent when present. A routed leaf selects
the direct entrypoint that exposes it:

```sh
open-forge index memory
open-forge index .agents/memory/_memory.md
open-forge index skills/experience-design
```

Operands use the shared automatic source-ID or exact `.agents/...` path grammar.
Directories are not operands. Repeated and overlapping scopes are deduplicated.
An explicitly selected detached entrypoint may maintain its unambiguous local
subtree without inventing a Loader or parent.

### Generated Boundary

The command changes only the body between one valid generated marker pair in
the final `Entries` section. It preserves the markers and every byte outside
that body.

Stale or malformed generated lines inside a valid pair may be replaced because
the bounded body is derived. Missing, duplicate, misplaced, reversed, nested,
or otherwise ambiguous markers block before any write. Missing or malformed
required descriptions, tags, destinations, or route relationships also block.
The command never invents metadata, indexes an overwrite companion, or formats
the complete file.

### Dry Run And Apply

`--dry-run` uses the same complete plan and safety checks as application. It
shows the exact bounded diff for every region that would change and writes
nothing:

```text
Generated Entries would be updated.
Checked 12 regions: 2 need updates, 10 are up to date.

<exact bounded diffs>

No files changed (--dry-run).
```

When a dry run safely establishes planned changes but also has a non-blocking
finding, its semantic result is `attention`. It still shows the complete plan
and exact bounded diffs, says no files changed, and human output says
`requires attention`.

Omitting `--dry-run` selects application of the planned replacement inside the
selected generated boundaries. The command does not prompt. It prepares and
verifies one immutable external recovery bundle covering every existing
generated target that will be replaced before the first target effect; a no-op
creates no bundle. Application rechecks source and destination state, writes complete
planned bytes, verifies every changed file and the complete generated
projection, and reports the actual residual draft or final path on handled
failure or cancellation. A closed final ZIP may remain after abrupt process
termination, without an executable crash or power-loss guarantee. It never
restores or compensates for an earlier effect.
After final verification, successful effects remain successful even if bundle
deletion fails; the result is `attention` with the exact residual path and
cleanup guidance. No persistent transaction journal or saved plan is created.

### Results

A verified no-op is a successful result:

```text
Generated Entries are up to date.
Checked 12 regions. No files changed.
```

A successful update says what changed without exposing internal stage terms:

```text
Generated Entries were updated.
Checked 12 regions: 2 updated, 10 already up to date.
All 12 regions match the routed sources.
```

Normal output states what happened without naming internal planning stages.
Verbose diagnostics and structured results retain those facts. Human output
renders the semantic status `attention` as `requires attention`; JSON keeps
`attention`.

Primary human rendering for `complete`, `attention`, and `incomplete` results
goes to stdout. Primary human error rendering for `invalid`, `blocked`, `failed`,
and `interrupted` results goes to stderr. Each primary human typed result stays
together on its assigned stream. `--json` writes one complete structured result
to stdout for every semantic status; separate bounded diagnostics use stderr,
and ordinary human text is not mixed into JSON stdout.

Every CLI write that can change routing or indexed metadata plans the same
generated-navigation changes against its intended post-write state. The parent
dry run, recovery, verification, and result include those changes instead of
running a hidden follow-up command.

See the
[index contract set](../.agents/memory/crystallized/documents/cli/contracts/index-candidate/_index-candidate.md)
for the complete interface.

## References

`references` reports direct one-hop authored references for one selected source.
It keeps incoming and outgoing sections separate and does not modify files,
load target bodies into context, or fetch external URLs:

```text
open-forge references <source-reference>
  [--direction=in|out|both]
  [--include=<source-reference>]...
  [--exclude=<source-reference>]...
  [global flags]
```

Direction omission requests both. `in` scans the default eligible `.agents`
Markdown universe; `--include` and `--exclude` use the shared source-universe
filter contract and apply to incoming work only. With `both`, outgoing inspection
is unchanged. Filters are invalid with `--direction=out`. Outgoing results include
contained local references and raw external HTTP/HTTPS facts, which are marked
unchecked and never fetched. A complete empty incoming result is possible only
after the effective scan completes.

This is different from `context --follow-links`: Context expands selected content
with reachable link targets, while `references` reports edge facts only. It is
different from `route list`, which reports authored route topology, and from
`find`, which returns a flat Markdown inventory and predicate matches. It does not
diagnose or repair broken references; diagnosis belongs to `doctor`, and repair
requires its separate mutation authority. See the
[references contract set](../.agents/memory/crystallized/documents/cli/contracts/references-candidate/_references-candidate.md)
for the complete interface and behavior.

## Doctor

`doctor` performs complete known-workspace diagnosis without changing anything:

```text
open-forge doctor [global flags]
```

It accepts no operands or doctor-specific flags. The six shared global flags
apply: `--workspace <path>`, `--json`, `--view=compact|expanded`, `--verbose`,
`--help`, and `--version`. Doctor never prompts, writes a plan, creates a
recovery bundle, changes lifecycle state, or invokes Repair. It does not inspect
or report repository state.

Doctor checks these domains in order:

1. Workspace and entry.
2. Recovery and residual state.
3. Routes, metadata, overwrites, and generated navigation.
4. Local references.
5. Framework lifecycle.
6. Extension lifecycle.

Each domain reports its coverage, limitations, counts, findings, and typed next
actions. Complete coverage means that the checks ran, not that the workspace is
healthy. Findings keep severity separate from their resolution, which may be
safe-exact, guided-choice, targeted-operation, manual-decision,
blocked-repair, or informational. External links are not fetched. JSON is
complete and non-interactive.

```text
Open Forge doctor
Workspace: <workspace-path>
Mode: read-only; no files changed
Status: requires attention
Coverage: complete; 6 domains complete

Immediate actions
  Preview safe exact repairs: open-forge repair --automatic --dry-run
  Review 1 guided local-reference candidate
```

The values are illustrative. `doctor` reports `complete` only when all six
domains have complete coverage and no actionable warning or error remains. It
reports `attention` for complete coverage with actionable findings,
`incomplete` for trustworthy partial coverage, and `blocked` for an unsafe
required boundary. Informational facts alone do not produce `attention`.
See the [Doctor contract set](../.agents/memory/crystallized/documents/cli/contracts/doctor/_doctor.md)
for the complete finite finding catalogue and output rules.

## Repair

`repair` is the separate, constrained mutation operation for current exact local
reference repairs:

```text
open-forge repair [--automatic] [--relink <source-location> <expected-destination> <target-path>]... [--dry-run] [global flags]
```

It has no positional operands, generic proposal references, `--yes`, `--preview`,
`--suggestions`, `--all`, `--force`, or `--apply`. The six global flags are the
same six listed above. `--automatic` and `--dry-run` are repeatable and
idempotent. `--relink` repeats as exact triples; identical tuples
deduplicate and contradictory tuples are invalid.

The simplest human invocation opens a wizard. It reruns all six Doctor domains,
shows safe-exact proposals, presents bounded guided candidates without selecting
an uncertain candidate, builds one plan, shows exact effects, and asks for final
confirmation with No as the default. `--automatic` suppresses the wizard and
selects only current safe-exact proposals. The preferred structured preview is:

```sh
open-forge repair --automatic --dry-run --json
```

An explicit relink supplies a source occurrence, its exact current authored
destination, and an exact contained target path. The CLI computes the correct
authored relative destination and leaves the label and unrelated bytes alone.
The explicit values bypass the wizard and run directly unless `--dry-run`
selects preview:

```sh
open-forge repair \
  --relink ".agents/docs/guide.md@12:8" \
  "../old.md#Old" \
  ".agents/docs/new.md#New" \
  --dry-run
```

The result identifies the selected mode, diagnosis coverage, selected and
remaining findings, affected paths, exact bounded effects, and whether files
changed:

```text
Open Forge repair
Selection: automatic
Application: preview
Diagnosis coverage: complete for selected edits
Selected: 2 safe-exact effects
Remaining: 1 guided candidate set
No files changed (--dry-run).
Status: requires attention
```

`--dry-run` is the only preview spelling and writes nothing. Every request uses
fresh facts, one conflict-free plan, preflight, expected-state revalidation,
one external recovery-bundle preparation covering every existing-target effect
(`Replace`, `ReplaceGeneratedRegion`, or `Delete`),
verification, and fresh relevant-domain post-diagnosis. All bundle preparation
completes before the first target effect; handled failure or cancellation reports
the actual residual draft or final path without restoration, rollback,
compensation, or current-target classification. Only incomplete or blocked
diagnosis facts actually required by the selected edits block Repair; unrelated
lifecycle or recovery-observer unavailability remains visible but non-blocking.
Repair never chooses external, fuzzy, semantic, authored, generated-navigation,
route, recovery, Framework, or Extension changes. Generated drift belongs to
`index`; known route intent belongs to route operations; lease-validated
recognized recovery-bundle or draft deletion belongs to the separate `cleanup`
operation.
See the
[Repair contract set](../.agents/memory/crystallized/documents/cli/contracts/repair/_repair.md)
for the complete catalogue, wizard, direct modes, and result meanings.

## Source References

Commands that identify existing Open Forge content accept either an automatic
source ID or the exact workspace-relative `.agents` path.

```text
ID:   memory/crystallized/documents/architecture
Path: .agents/memory/crystallized/documents/architecture.md
```

The CLI calculates IDs from current workspace paths. IDs are not stored in
frontmatter or another registry.

### Automatic IDs

| Path                                                    | ID                                           |
| ------------------------------------------------------- | -------------------------------------------- |
| `.agents/loader.md`                                     | `loader`                                     |
| `.agents/memory/_memory.md`                             | `memory`                                     |
| `.agents/memory/crystallized/_crystallized.md`          | `memory/crystallized`                        |
| `.agents/memory/crystallized/documents/architecture.md` | `memory/crystallized/documents/architecture` |
| `.agents/skills/experience-design/SKILL.md`             | `skills/experience-design`                   |

ID rules:

- Remove the leading `.agents/`.
- Use `/` between segments on every operating system.
- Remove `.md` from Markdown files.
- Use the containing folder ID for recognized entrypoints, accepted
  compatibility entrypoint filenames, and `SKILL.md`.
- Preserve exact case, spaces, and Unicode.
- Keep a meaningful extension for supported non-Markdown resources.
- Reject `.` and `..` ID segments.

An ID identifies a source. It does not make that source routed, indexed,
authoritative, or managed.

### IDs And Paths

The operand form is explicit:

- `.agents/...` or `./.agents/...` is an exact workspace path.
- Every other value is an ID.

```sh
open-forge context memory/crystallized/documents
open-forge context .agents/memory/crystallized/documents/_documents.md
```

The CLI does not guess between an ID and path. A value such as `src/file.md` is
an ID, not a path. Source-reference paths stay below `.agents` unless a command
defines a separate external-source operand.

The CLI matches the complete ID exactly. It does not correct case, use fuzzy or
synonym matching, decode percent-encoded text, or use semantic search.

Exact paths resolve from the selected workspace. They must stay lexically and
physically inside that workspace and identify a source kind supported by the
command. A path that escapes the workspace is blocked. A missing path or an
unsupported source kind is invalid. Results use the canonical `.agents/...`
path with `/` separators.

### Spaces And Quotes

Quote the complete ID or path when it contains spaces:

```sh
open-forge context "memory/project alpha/documents"
open-forge context ".agents/memory/project alpha/documents/_documents.md"
```

Single quotes also work in shells that support them:

```sh
open-forge context 'memory/project alpha/documents'
```

Quotes belong to the shell and are not part of the source ID. Documentation
uses double quotes because they work across more common shells. Windows
`cmd.exe` users must use double quotes.

### Collisions

Unusual structures may produce the same ID:

```text
.agents/guidance/style.md
.agents/guidance/style/_style.md
```

Both produce `guidance/style`. Interactive use may ask which source the user
means. JSON and non-interactive use block and list every candidate. An exact path
resolves the collision:

```sh
open-forge context .agents/guidance/style.md
```

The CLI does not choose by file kind, generated order, modification time,
directory depth, or likely intent.

If one folder contains several recognized entrypoints, an exact path can select
one file for inspection. A command that needs valid route meaning remains
blocked until that structural ambiguity is fixed.

### Overwrites

A base and adjacent overwrite share one source ID. Selecting the ID, base path,
or overwrite path selects the complete logical source. The base is read first
and the overwrite second. An overwrite is never returned as an independent
source. An orphan overwrite may be reported as broken evidence through its exact
path, but it is not a valid standalone source.

### Results

Every result that emits or identifies a resolved `.agents` source shows both its
automatic ID and canonical workspace-relative path. A local link target outside
`.agents` has no automatic ID, so its result uses `ID: none` and keeps the
canonical path.

## Legacy CLI

The frozen TypeScript MVP is available as `open-forge-old`. It is an optional
deterministic helper for the human-readable Open Forge Framework.

The legacy CLI exists so agents and maintainers can still use established
routing assistance. Its commands and flags do not define the new CLI.

### Commands

| Command   | Legacy responsibility                         | Writes    |
| --------- | --------------------------------------------- | --------- |
| `install` | Install or reconcile the Framework            | Yes       |
| `extend`  | Inspect or manage Extension packages          | Sometimes |
| `index`   | Rebuild generated `Entries`                   | Yes       |
| `load`    | Emit broad baseline and continuity context    | No        |
| `find`    | Select routed files by tag or route           | No        |
| `chain`   | Inspect inherited context for one routed file | No        |
| `doctor`  | Validate deterministic Framework structure    | No        |
| `create`  | Scaffold a route chain or Extension package   | Yes       |

Use the executable's help for exact frozen syntax:

```sh
open-forge-old --help
```

Common repository assistance:

```sh
open-forge-old load --bodies
open-forge-old find --tag Architecture --paths
open-forge-old chain .agents/memory/crystallized/documents/_documents.md
open-forge-old index
open-forge-old doctor
```

`load` performs the MVP's broad `#KeepInMind` audit. It does not implement the
current Framework's target-sensitive entrypoint contract.

### Legacy Implementation

`src/cli-mvp/` contains the frozen source for `open-forge-old`. Neither
`src/cli-mvp/` nor `open-forge-old` is replacement implementation or contract
authority. Keep this source frozen during new-CLI development. Do not modify,
build, test, repair, or extend it.

The implementation and its legacy architecture document define its exact
behavior:

- [`src/cli-mvp/cli.ts`](../src/cli-mvp/cli.ts)
- [CLI MVP Architecture](../.agents/memory/crystallized/documents/cli/mvp-architecture.md)

Plain Markdown remains complete without either CLI.
