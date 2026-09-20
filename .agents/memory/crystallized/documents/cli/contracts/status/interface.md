---
open-forge:
  description: Current accepted interface for workspace status, context-size comparison, root customization, managed Extensions, and recognized recovery bundles
  responsibility: Define the new CLI `status` command without turning orientation into diagnosis or duplicating Architecture mechanics
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Status, Interface, Context, Measurement, Extension, Recovery, CurrentTruth]
---

# Status Interface Contract

## Status And Boundary

This is the current Crystallized Interface Contract for the accepted `status`
command. This file is authoritative for the command's public syntax, observable
facts, output, semantic result names, errors, non-goals, examples, and public
verification. The command does not ship yet. The sibling Behavior Contract
defines the technology-neutral operation behind this public surface.

The accepted [Shared Result
Coordinates](../shared/result-coordinates/interface.md) define the exact shared
JSON result schema and numeric exit mapping. Status uses those definitions without duplicating
implementation mechanics. Gate 5 must prove source-generated YamlDotNet and STJ
serialization, fixed Markdig where used, real `System.IO`, Native AOT, OS
locking, isolated tests, and package journeys. Token estimation, recovery-bundle
identity, and Status-specific source placement remain subordinate implementation
details rather than new contract authority.

The [Framework loading contract](../../../framework/routing/loading.md)
defines startup and continuity loading. The [routing model](../../../framework/routing/model.md),
[scope rules](../../../framework/routing/scope.md), and
[overwrite rules](../../../framework/routing/overwrites.md)
define the Framework meaning measured by this command. Those sources remain the
authorities for their detailed Framework meanings; this Interface defines only
what `status` exposes about them.

## Purpose

`status` gives a quick, read-only summary of one exact workspace. It makes the
Framework's context behavior and local customization visible without performing
a complete structural diagnosis.

It answers:

- How much context would the Framework shipped with this CLI load at startup?
- How much context does the Framework load for this workspace at startup now?
- How much context is available in total?
- Which context may load again at continuity boundaries?
- Which continuity sources contribute the most content?
- Which root categories were added or removed relative to the shipped Framework?
- What Framework and Extension lifecycle sections are readable, and which trust
  or lifecycle-coverage state do they report?
- How many Extensions and managed files are recorded, including installed facts
  whose package source is unavailable?
- What bounded Workspace Library record, source-root availability, and registered
  projection-link facts are readable, including their IDs and current, missing,
  changed, blocked, and unavailable counts?
- Which exact-name recovery candidates are present, what path and integrity
  condition does each have, and how many verified finals and incomplete drafts
  are there?

Given the same CLI payload, workspace bytes, and explicit input, `status` returns
the same facts, ordering, measurements, and semantic result.

## Syntax

The complete public command form is:

```text
open-forge status [global flags]
```

`status` has no operands or operation-specific flags. The shared [Global CLI
Flags](../shared/global-flags/interface.md) contract defines `--workspace`, `--format json`,
`--detail`, `--detail debug`, `--help`, and `--version`. All six apply to `status` under
that contract. Their complete spelling, value grammar, defaults, repetition,
composition, terminal behavior, and shared errors remain in that contract rather
than being redefined here.

## Workspace

The command uses the exact current working directory or exact
`--workspace <path>` value. It does not search upward, select a Git root, or infer
another workspace from an `AGENTS.md` or `.agents` directory.

A selected directory without Open Forge installed is a valid status subject. If
the absence can be established safely, that uninstalled state is complete. The
`Initial (shipped)` measurement remains measured whenever the
embedded Framework payload is available. For that uninstalled subject, current
startup, Difference, startup percentage, continuity, and root-category facts are
not-applicable, not unavailable. Total available context remains a numeric
physical inventory when it can be measured safely. Lifecycle and recovery facts
retain their own rules. An applicable fact that cannot be measured is
unavailable and makes the result incomplete. A missing, unavailable, or
non-directory workspace is blocked.

## Inspection Boundary

`status` inspects only the facts needed for its summary:

- The canonical workspace entry and supported context files below `.agents`.
- The embedded Framework payload distributed with the running CLI.
- Entrypoints, generated `Entries`, loading tags, and overwrite companions needed
  to calculate startup and continuity context.
- The current `Loader`'s direct root categories.
- One shared read of `.agents/open-forge.lock.json`, with separate Framework,
  Extension, and Library claims, including typed Library roots and mappings.
- The current user's external recovery store at
  `Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.None)/OpenForge/recovery/v1`, limited to
  exact-name final and draft candidates for the selected normalized physical
  workspace path.

The command may enumerate and stream supported context files to measure their
content. It does not build the complete context graph, parse ordinary links or
named sections, validate every route, inspect unrelated workspace files, or
construct a mutation plan.

### Ownership And Current Target Facts

Ownership is read from `.agents/open-forge.lock.json` through the shared forgiving
reader. Framework, Extension, and Library claims remain separate. Neither the
old lifecycle document nor the old Library record supplies ownership facts.
An absent, unreadable, nonordinary, malformed, or uninterpretable lock supplies
no usable claims and produces an informational ownership observation, without
blocking the command or reconstructing ownership from files. Read-only commands
never create or repair the lock. Actual source, target, route, and recovery
boundaries still determine their own coverage and findings.

Target comparison uses actual disk content against current intended content:
the running embedded Framework payload, or the currently read exact Extension
source recorded by its owner. A recorded version or stored content hash does not
gate comparison. Missing targets remain `missing`; unavailable reads or intended
sources remain `unavailable`; unsafe physical or Markdown boundaries remain
`blocked`. Comparable content is `current` when equal and `changed` otherwise.
The existing immutable `open-forge-markdown-v1` policy normalizes line endings
and eligible generated content only; authored whitespace and final-newline
choices remain significant. Non-Markdown Extension payloads use exact bytes.
Comparison evidence is computed during the invocation and stores no baseline.

Root managed hosts compare only their `open-forge` region. Scoped Framework
entrypoints use the existing canonical payload alignment; ambiguous or missing
alignment makes intended comparison unavailable. Generated Entries compare with
the current authored route projection, without consulting stored fingerprints.

The existing lifecycle vocabulary remains a presentation of observed coverage.
`absent` still requires independent complete footprint and recovery inspection;
no lock or matching file alone proves absence or ownership. Readable claims may
remain reportable while their source is unavailable. An unavailable lock yields
an observation, not an installation error or a trusted empty inventory.

Workspace Library records and registered projections remain a separate
consumer-local authority from Framework and Extension lifecycle state. Status
observes only the exact record and the bounded source-root and destination-entry
facts needed for its Library summary. It does not adopt an unregistered link,
infer a record or mapping from paths or bytes, read source-target bytes, or
perform a complete current source inventory when the bounded profile does not
allow it.

## Context Inventory

### Context File

The human display uses `Files`. The status inventory has one closed content
boundary:

- The canonical workspace `AGENTS.md` entry when it is an ordinary UTF-8 file.
- Every ordinary UTF-8 Markdown file with a `.md` suffix that is physically
  contained below the selected workspace's `.agents` directory.

The inventory follows these rules:

- Count each canonical workspace-relative path once in an inventory.
- Count a base and its valid overwrite companion as two files because both
  contribute content.
- Exclude an orphan overwrite because it cannot be selected as context by itself.
  Its presence makes the total inventory incomplete rather than silently treating
  it as available context.
- Do not count generated `Entries` separately from the entrypoint that contains
  them.
- Do not count ordinary local link targets outside `.agents` merely because
  another file links to them.
- Do not count provider bridges, workspace settings, the ownership lock, recovery
  bundles, drafts, or other operational metadata as context.
- Do not count non-Markdown Skill resources or other support files as context.

Malformed Markdown remains measurable when its UTF-8 bytes are readable, even
when route-dependent startup resolution is incomplete. A non-UTF-8 Markdown file
makes the affected inventory incomplete rather than being converted or ignored.

Structured results may use `sources` and ordered source layers where that
precision is needed. Human output keeps `Files` because the displayed counts are
physical files.

### Startup Context

Startup context is the complete context required when work starts or resumes:

1. Read the canonical workspace entry and Loader.
2. Follow visible #LoadNow and #KeepInMind entries through loaded parents in
   generated order. For a loaded entrypoint, apply its child loading rules.
3. Place each valid overwrite companion immediately after its base.

Both tags use the same parent and scope boundaries. Neither activates an
otherwise unselected ancestor or scope. Inactive continuity metadata does not
make this closure incomplete solely because it exists elsewhere in the workspace.

This is the same startup-required closure returned by `open-forge context` with
no explicit source. Status measures that closure without rendering its content.
It follows current target-sensitive #KeepInMind behavior and does not reproduce
the frozen MVP's broad traversal. The [Context Interface Contract](../context/interface.md)
defines the shared closure operation, while the Framework [loading contract](../../../framework/routing/loading.md)
defines its loading meaning.

### Initial And Current

The startup table has three rows:

| Row                 | Meaning                                                                         |
| ------------------- | ------------------------------------------------------------------------------- |
| `Initial (shipped)` | Startup context resolved from the Framework payload embedded in the running CLI |
| `Current workspace` | Startup context resolved from the selected workspace now                        |
| `Difference`        | Current workspace minus Initial for each metric                                 |

`Initial` is not a saved installation snapshot or previous invocation. Status is
stateless and stores no historical baseline. Updating the CLI may change the
embedded Framework used for this comparison.

The Difference row uses signed values. A larger, smaller, added, removed, or
changed context is not inherently good or bad and does not by itself change the
semantic result.

When both operands for a metric are numeric, Difference is the signed numeric
current-minus-initial value, including zero. If a required operand is unavailable,
the derived value is unavailable. If the comparison does not apply, the derived
value is not-applicable.

### Total Available Context

Total available context is the unique current context-file inventory, including
both startup and on-demand files. It reports files, characters, UTF-8 size, and
estimated tokens.

The displayed startup percentage is:

```text
current startup UTF-8 bytes / total available UTF-8 bytes
```

It shows how much of the available context loads at startup. The remainder is
available for explicit or routed selection. Total available context does not
claim that every file is relevant to one task or that an agent will load all of
it.

The percentage is numeric when current startup and total available UTF-8 bytes
are numeric and the total is greater than zero. When both byte values are zero,
the percentage is not-applicable. A positive current value with a zero total is
an incomplete accounting invariant, so the percentage is unavailable and the
result is incomplete. An unavailable required operand makes the percentage
unavailable. Status never renders unavailable or not-applicable as zero. The
exact JSON representation of these values follows the [Shared Result
Coordinates](../shared/result-coordinates/interface.md).

### Continuity Context

Continuity context is the subset of current startup context that may load again
at a #KeepInMind boundary while the scope remains active. It includes exposed
tagged sources and their applicable visible child closure, with adjacent
overwrites. It does not add untagged ancestors solely for refresh.

The human label is:

```text
Continuity context (may load again)
```

It may load again after a handoff, context restoration, before closeout, or at
another defined continuity boundary. It is not claimed to load on every model
request, prompt, message, or tool call. Its metrics are a subset of current
startup metrics and must not be added to the startup total.

### Largest Continuity Sources

The standard human result shows at most the three logical continuity sources
with the largest UTF-8 contribution. Minimal human output omits this section.
JSON carries every logical continuity-source contribution in deterministic order.
A logical source with a base and overwrite companion contributes the combined
size of both ordered layers and appears once under its automatic source ID. The
[CLI Source References](../shared/source-references/interface.md) contract
defines that automatic source identity; `status` does not accept source-reference
operands or create another identity scheme.

Order sources by:

1. Descending UTF-8 size.
2. Canonical source ID as the deterministic tie-breaker.

Show fewer than three when fewer exist. This ranking reports contribution, not
importance, quality, or a recommendation to move the source. `doctor` owns any
future diagnosis or recommendation about narrower placement.

## Measurements

Every context row uses the same measurements:

| Metric        | Meaning                                                                          |
| ------------- | -------------------------------------------------------------------------------- |
| `Files`       | Exact context-file path count under the inventory rules above                    |
| `Characters`  | Exact Unicode scalar-value count in the measured file text                       |
| `Size`        | Exact UTF-8 byte length of the same measured file text, displayed with IEC units |
| `Est. tokens` | Deterministic rough estimate from the exact character count                      |

Measurements include complete authored file text, including frontmatter,
generated regions, and overwrite content. They do not include CLI identity
blocks, headings, labels, separators, or other presentation added by a renderer.

The accepted planning estimate is:

```text
estimated tokens = ceiling(characters / 4)
```

Human output prefixes the value with `~` and may round it for display. Structured
output keeps the unrounded estimate and identifies the estimator. This estimate
is not a model tokenizer, billing amount, context-window guarantee, latency
estimate, or provider-specific token count.

If exact characters or UTF-8 size cannot be measured safely, the affected value
is unavailable. Status never substitutes a partial count without marking the
result incomplete. Zero, unavailable, and not-applicable remain distinct; an
unavailable or not-applicable value is never rendered as zero.

## Workspace Structure

### Root Categories

A root category is one direct root route exposed by the Loader. Status compares
the current Loader with the Loader embedded in the running CLI:

- `Root categories` is the current count.
- `Added categories` exist in the current Loader but not the embedded Loader.
- `Removed categories` exist in the embedded Loader but not the current Loader.

Added categories follow current Loader order. Removed categories follow embedded
Loader order. Category identity uses its exact root route ID.

Added and removed categories are neutral customization facts. Users may add or
remove root routes, and missing shipped categories remain absent unless a
separate lifecycle operation explicitly restores them.

Status does not report a scope count. Scope is a narrowing role performed by a
routed `slug` in context, not a separately declared file type that can be
counted reliably as a shallow workspace statistic. The [routing model](../../../framework/routing/model.md)
and [scope rules](../../../framework/routing/scope.md)
define those Framework meanings.

### Generated Navigation

Generated `Entries` remains a structural projection fact. When a generated
region is `unavailable` only because exact typed evidence proves that its direct
ordinary Markdown or recognized-entrypoint metadata is readable but malformed,
Status retains that structural `unavailable` fact and records one
`status.generated-navigation-metadata-invalid` Warning finding for each actual
malformed source path and observed cause. Status may make this distinction only
when all direct dependency observations are readable, contained, and otherwise
projectable, and a pure counterfactual re-projection that treats only those
malformed ordinary metadata facts as missing, with the eligible automatic-ID
fallback, confirms that no other projection blocker exists. Status never
publishes substitute metadata or generated entries and remains read-only.

Unreadable or coverage-incomplete facts remain `incomplete`; unsafe or
ambiguous facts remain `blocked`; native Skill metadata and other native
failures remain strict. Known installed, toolkit, context, lifecycle, and other
independently projectable facts remain in the result. This exception is not a
healthy or complete generated-projection claim.

### Extensions And Managed Files

`Extensions` counts distinct installed Extension IDs in trusted `extensions`
lifecycle facts. The lifecycle state remains part of the result:

| Lifecycle state                         | Extensions                      | Managed files                     | Status effect                           |
| --------------------------------------- | ------------------------------- | --------------------------------- | --------------------------------------- |
| `absent`                                | `0 recorded`                    | `none recorded`                   | No status effect alone                  |
| Trusted and empty                       | `0`                             | `none recorded`                   | No status effect alone                  |
| Trusted with records                    | Distinct recorded Extension IDs | States for recorded managed paths | Follows the recorded states             |
| `untrusted`, `incomplete`, or `blocked` | Safe facts or unavailable       | Safe facts or unavailable         | `incomplete` or `blocked` as applicable |

An absent lifecycle section makes no claim about unmanaged extension-like files.
Untrusted or unavailable facts are not converted to empty or trusted counts.

Each lifecycle-recorded managed path contributes once to exactly one state:

| State     | Meaning                                                           |
| --------- | ----------------------------------------------------------------- |
| `current` | Current semantic identity matches the current intended payload    |
| `changed` | Comparable disk content differs from the current intended payload |
| `missing` | The recorded path does not exist                                  |

Shared owner sets do not multiply the file count. These are lifecycle facts, not
claims about runtime meaning, user intent, safe replacement, removal authority,
or repairability. Unmanaged files do not enter this summary.

Supported Markdown lifecycle comparisons use the conservative
`open-forge-markdown-v1` fingerprint policy. The CLI distribution embeds
Framework and first-party Extension assets with deterministic inventory and hash
proof; that proof is distributed-source identity, not evidence of a selected
workspace's current installation or of a proven runtime implementation.

### Workspace Libraries

Status reads the `libraries` claims from the same lock snapshot supplied to the
Framework and Extension contributors. Typed IDs, source roots, destination roots,
and source-relative paths derive bounded mapping facts. An unavailable lock or
uninterpretable Library claims produce `library-ownership-observation` with
complete command status, no registrations, and no inferred ownership. Record
state remains missing, invalid, or unavailable rather than a valid empty record.

For each recorded Library, Status observes the source root only far enough to
establish workspace-relative lexical and physical containment, an ordinary
directory, and safe real ancestry. No specially named source child is required. It does not enumerate that
source tree to discover unregistered files. `sourceAvailability` is
`available`, `unavailable`, or `not-applicable` and remains separate from
record state.

For each registered mapping, Status observes the destination directory entry
without following it. `current` means a relative file link with the exact raw
target derived from the recorded mapping; `missing` means no destination entry;
`changed` means a safely observed occupant or different target; `blocked` means
unsafe or ambiguous identity, containment, alias, or collision; and
`unavailable` means a required fact could not be read. The observation retains
the destination-derived automatic source ID separately from the Library ID. It
never reads source bytes,
follows a source target, creates or removes a link, or adopts an exact-looking
unregistered link.

The bounded summary exposes the record state, Library IDs, source-root and
source-availability facts, registered-link count, and counts partitioned into
`current`, `missing`, `changed`, `blocked`, and `unavailable`. A safely observed
missing or changed registered link is projection drift and maps to
`completed-with-warnings`; unavailable coverage maps to `incomplete`; unsafe or ambiguous
identity maps to `blocked`. These counts do not claim complete source inventory,
source additions, retirements, or adoption.

### Recovery Bundles

Recovery accounting considers only exact-name final and draft candidates in the
current user's external
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.None)/OpenForge/recovery/v1` store. Status uses
this observer-only lookup and never creates the OS application-data root or the
Open Forge subtree. The store is keyed by the selected normalized physical
workspace path, and Status never
falls back to the workspace, a repository, a temporary directory, or a target's
adjacent files. Unknown, lookalike, mismatched, or differently keyed items do not
enter the candidate catalogue. A malformed exact-name final remains a reported
candidate; it is not omitted merely because it is not verified.

If the application-data root or its `OpenForge/recovery/v1` store is absent,
Status reports zero verified finals and zero incomplete drafts. If an existing
root or selected workspace bucket cannot be read, the recovery fact is
unavailable and Status forms its locally contracted `incomplete` result; it does
not treat access failure as absence.

Status enumerates only exact deterministic final and draft names directly under
the selected workspace bucket. A final ZIP is `Verified` only when its
source-generated schema-v1 manifest decodes semantically and its exact ordered
entry names and counts, declared lengths and hashes, and exact payload bytes all
validate. A malformed, unsupported, or unreadable exact named final is reported
with its path and `Malformed`, `Unsupported`, or `Unavailable` condition instead
of becoming verified or entering the verified-final count. An exact named draft
is reported with its path as `Incomplete`, enters only the incomplete-draft
count, and never forms a recovery preparation. Unknown names and target-adjacent
files do not enter the result.

For each exact-name candidate, Status exposes only its kind, path, and integrity
condition. Its counts distinguish verified finals from incomplete drafts;
malformed, unsupported, and unavailable finals remain reported issues and never
inflate either count. It does not acquire the workspace lease, read live targets,
derive a current-target classification, or infer activity. Payload validation
uses fixed bounded buffers and never extracts, discloses, renders, logs, returns,
retains, or materializes payload bytes. A bundle keyed to an original workspace
path after a workspace move is not auto-bound to the newly selected path.

Status does not remove, clean, restore, roll back, or otherwise mutate bundles or
workspace targets. The accepted [cleanup contract](../cleanup/interface.md) owns
lease-validated candidate deletion; the [Mutation And Recovery Technical
Design](../../technical-designs/mutation-and-recovery.md) defines exact bundle
identity and storage mechanics. Status does not treat the visible
workspace lock as a recovery bundle.

## Human Output

The command uses the shared native report. The default detail is `minimal`; `standard`, `full` and `debug` add the catalogue-defined facts. `--detail-filter <error|warning|info|all>` is repeatable and changes only the rendered detail. Use `--format text` for this text report. Primary result text for `completed`, `completed-with-warnings` and `incomplete` is on stdout; primary errors for `invalid-input`, `blocked`, `failed` and `cancelled` are on stderr. There is no `Status:` line.

### Statuses and headlines

| Status                  | When                                                                                                           | Headline                                                                                                                  | Exit | Stream |
| ----------------------- | -------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | installed, every check complete, nothing needs a look                                                          | `Open Forge is installed and current.`                                                                                    |    0 | stdout |
| completed               | not installed (no `.agents/loader.md`)                                                                         | `Open Forge is not installed in <workspace path>.`                                                                        |    0 | stdout |
| completed               | installed, only Info findings (no ownership record)                                                            | `Open Forge is installed and current.` then the Info rows at `full`                                                       |    0 | stdout |
| completed-with-warnings | changed or missing Framework or Extension files, stale Entries, a proven readable ordinary metadata-invalid generated region, missing Library links, recovery bundle present | `Open Forge is installed, but <N> files need attention.` or `... <N> things need attention.` when a warning is not a file |    2 | stdout |
| incomplete              | a measurement or record could not be read                                                                      | `Open Forge is installed, but some checks could not finish.`                                                              |    3 | stdout |
| incomplete + warnings   | both                                                                                                           | `Open Forge is installed, but <N> files need attention and some checks could not finish.`                                 |    3 | stdout |
| invalid-input           | bad flag or operand                                                                                            | family `invalid-input`: `Cannot check status: <problem>.`                                                                 |    4 | stderr |
| blocked                 | workspace missing, not a directory, unsafe, or a boundary cannot be checked safely                             | `Cannot check this workspace: <reason>.`                                                                                  |    5 | stderr |
| failed                  | unexpected error                                                                                               | `Status stopped because of an unexpected error: <reason>.`                                                                |    1 | stderr |
| cancelled               | Ctrl+C                                                                                                         | `Status was cancelled.`                                                                                                   |  130 | stderr |

### Text by level

`minimal`, healthy:

```text
Open Forge is installed and current.
  Startup reads 19 of 22 routed files, about 8.0k tokens.
```

`minimal`, healthy with Extensions and a Library:

```text
Open Forge is installed and current.
  Startup reads 19 of 22 routed files, about 8.0k tokens.
  Extensions: development 0.1.0, planning 0.1.0
  Libraries: team-knowledge (12 links)
```

`minimal`, warnings:

```text
Open Forge is installed, but 2 files need attention.
  Warning  .agents/maps/_maps.md         changed since it was installed
  Warning  .agents/patterns/_patterns.md  missing; it was installed by the Framework
  Startup reads 19 of 22 routed files, about 8.0k tokens.
Next: open-forge update
```

`minimal`, incomplete:

```text
Open Forge is installed, but some checks could not finish.
  Startup context could not be measured: .agents/maps/_maps.md could not be read completely.
Next: open-forge doctor
```

`minimal`, not installed:

```text
Open Forge is not installed in D:/work/myrepo.
Next: open-forge install --dry-run
```

`standard` adds, in this order after the findings: `Workspace: <path>`, then

```text
Startup context
  Shipped by this CLI:  19 files, about 8.0k tokens
  This workspace:       19 files, about 8.0k tokens
  May load again later:  4 files, about 1.0k tokens
  All routed files:     22 files, about 9.9k tokens (startup is 81%)
Routes: 8 root categories, 20 Entries sections current
Framework files: 21 current
Extensions: development 0.1.0 (3 files current)
Libraries: team-knowledge, shared/team -> docs (12 links current)
Recovery data: none
```

Lines whose count is zero are omitted (`Recovery data: none` is shown only at
`full`). `Difference` appears only when non-zero: `Added since shipped: 2
files, about 400 tokens`. Root categories added or removed appear as
`added: custom` and `removed: patterns` on the Routes line.

`full` adds every Framework file with its state, every Entries section with
its state, every Extension file, every Library link with expected and
observed targets, the three largest may-load-again sources, and the recovery
bundle paths with their integrity.

`debug` adds the operational contributor coverage on stderr.

### Representative transcripts by status

### Transcript — completed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#status-completed).

### Transcript — completed-with-warnings

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#status-completed-with-warnings).

### Transcript — incomplete

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#status-incomplete). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Status/__snapshots__/StatusBeforeOutputSnapshotTests/InstalledWorkspace/unreadable-entry-file.minimal.txt).

### Transcript — invalid-input

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#status-invalid-input). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Status/__snapshots__/StatusBeforeOutputSnapshotTests/WorkspaceBoundary/invalid-input.standard.txt).

### Transcript — blocked

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#status-blocked). [Matching reviewed capture](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Status/__snapshots__/StatusBeforeOutputSnapshotTests/WorkspaceBoundary/blocked-workspace.minimal.txt).

### Transcript — failed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#status-failed).

### Transcript — cancelled

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#status-cancelled).

## Structured Output

`--format json` writes one schema-3 envelope to stdout for every report status. It contains the command, status, workspace when applicable, detail, filter, command data, findings, effects, counts, limitations, recovery facts and next action as applicable. It is the same typed result as the text report; no ordinary text is mixed into the JSON document. If parsing fails before binding, the raw parser diagnostic remains text on stderr and no report envelope exists.

### JSON data by level

| Level    | `data` members                                                                                                                                                                                                                                                                                 |
| -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `installation { state, entryPath, loaderPath }`, `context { startup { shipped, current, difference, mayLoadAgain } each { files, characters, bytes, tokens }, allRouted { same }, startupShare }`, `extensions [ { id, version } ]`, `libraries [ { id, sourceRoot, destinationRoot } ]`       |
| standard | adds `structure { rootCategories { count, added, removed } }`, per-Extension `files { current, changed, missing }`, per-Library `links { current, missing, changed }`                                                                                                                          |
| full     | adds `frameworkFiles [ { path, state } ]`, `entriesSections [ { path, state } ]`, per-Extension `files [ { path, state } ]`, per-Library `links [ { path, state, expectedTarget, observedTarget } ]`, `context.mayLoadAgainSources [...]`, `recovery.candidates [ { path, kind, integrity } ]` |

Scalars are plain numbers or `null`. Every non-current file is also a finding.
A proven readable malformed ordinary metadata region keeps its `entriesSections`
state `unavailable`; its finding subject and cause carry the actual malformed
source path and observed cause. No new JSON member or structural state is
introduced for this distinction.

## Semantic Results

The status and exit mapping above are unchanged by detail or format. Root effects and recovery receipts retain their complete result facts at every detail level; command-owned data follows the catalogue's level rows.

### Counts and limitations

Counts (JSON `counts`, text sentence at `standard`): `routedFiles`,
`startupFiles`, `startupTokens`, `mayLoadAgainFiles`, `mayLoadAgainTokens`,
`allTokens`, `startupShare`, `rootCategories`, `entriesSectionsCurrent`,
`entriesSectionsStale`, `entriesSectionsMissing`, `frameworkFilesCurrent`,
`frameworkFilesChanged`, `frameworkFilesMissing`, `extensionsInstalled`,
`librariesRegistered`, `libraryLinksCurrent`, `libraryLinksMissing`,
`libraryLinksChanged`, `recoveryBundles`, `recoveryDrafts`. Unavailable
measurements are `null` with a limitation naming why.

Limitations render at every level as one sentence each, under the findings.

### Next rules

One line, chosen in this order: any Framework file warning ->
`open-forge update`; any Extension file warning -> `open-forge extension
update <id>` (first id); any Library link warning -> `open-forge library sync
<id>`; stale Entries -> `open-forge index`; recovery bundle ->
`open-forge cleanup`; any limitation or blocked -> `open-forge doctor`; not
installed -> `open-forge install --dry-run`; otherwise none.

## Errors And Boundaries

The findings catalogue below is the command's finite error and warning vocabulary. Findings keep their code, severity, family, subject and cause; detail filtering affects display only. A blocked, failed or cancelled result prevents further effects according to the catalogue.

### Findings catalogue

Codes gain the `status.` prefix (ledger). Subjects are paths unless stated.

| Code (new)                              | Severity | Family                  | Message                                                                                                   | Next                                |
| --------------------------------------- | -------- | ----------------------- | --------------------------------------------------------------------------------------------------------- | ----------------------------------- |
| status.invalid-input                    | error    | invalid-input           |                                                                                                           |                                     |
| status.workspace-unavailable            | error    | workspace-unavailable   |                                                                                                           |                                     |
| status.workspace-not-directory          | error    | workspace-not-directory |                                                                                                           |                                     |
| status.workspace-unsafe                 | error    | workspace-unsafe        |                                                                                                           |                                     |
| status.entry-unavailable                | warning  | local                   | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.entry-unavailable`).                                                                            | `open-forge doctor`                 |
| status.embedded-framework-unavailable   | warning  | payload-unavailable     |                                                                                                           |                                     |
| status.context-inventory-incomplete     | warning  | local                   | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.context-inventory-incomplete`).                  | `open-forge doctor`                 |
| status.startup-context-unavailable      | warning  | local                   | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.startup-context-unavailable`).                                                      | `open-forge doctor`                 |
| status.continuity-context-unavailable   | warning  | local                   | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.continuity-context-unavailable`).                                                       | `open-forge doctor`                 |
| status.root-categories-unavailable      | warning  | local                   | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.root-categories-unavailable`).                                           | `open-forge doctor`                 |
| status.generated-navigation-changed     | warning  | local                   | [`status.message.rebuild-the-entries-section`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs), [`status.message.the-entries-section-is-stale`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs), [`status.phrase.the-entries-section-of-is-stale`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.generated-navigation-changed`).                                                                 | `open-forge index`                  |
| status.generated-navigation-missing     | warning  | local                   | [`shared.phrase.has-no-entries-section`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedPhrases.cs), [`status.message.rebuild-the-entries-section`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs), [`status.message.there-is-no-entries-section`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.generated-navigation-missing`).                                                                          | `open-forge index`                  |
| status.generated-navigation-unavailable | warning  | local                   | [`status.message.the-entries-section-could-not-be-read`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs), [`status.phrase.the-entries-section-of-could-not-be-read`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.generated-navigation-unavailable`).                                                        | `open-forge doctor`                 |
| status.generated-navigation-metadata-invalid | warning  | local                   | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); existing generated-navigation selection with the actual malformed source path and observed cause. | `open-forge doctor`                 |
| status.generated-navigation-blocked     | error    | generated-region-unsafe |                                                                                                           |                                     |
| status.framework-ownership-observation  | info     | ownership-observation   | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.framework-ownership-observation`).                            |                                     |
| status.extension-ownership-observation  | info     | ownership-observation   | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.extension-ownership-observation`).                           |                                     |
| status.library-ownership-observation    | info     | ownership-observation   | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.library-ownership-observation`).                                      |                                     |
| status.framework-lifecycle-untrusted    | warning  | lifecycle-unavailable   | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.framework-lifecycle-untrusted`).                              | `open-forge doctor`                 |
| status.framework-lifecycle-incomplete   | warning  | lifecycle-unavailable   |                                                                                                           |                                     |
| status.framework-lifecycle-blocked      | error    | lifecycle-blocked       |                                                                                                           |                                     |
| status.framework-target-changed         | warning  | local                   | [`shared.label.changed-since-it-was-installed`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedText.cs), [`status.message.update-the-framework-files-that-need-attention`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.framework-target-changed`).                                                                  | `open-forge update`                 |
| status.framework-target-missing         | warning  | local                   | [`status.label.missing-it-was-installed-by-the-framework`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs), [`status.message.update-the-framework-files-that-need-attention`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.framework-target-missing`).                                                      | `open-forge update`                 |
| status.framework-target-unavailable     | warning  | local                   | [`shared.label.could-not-be-read`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedText.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.framework-target-unavailable`).                                                                               | `open-forge doctor`                 |
| status.framework-target-blocked         | error    | local                   | `<path>  could not be checked safely: <it is a link \| its location could not be verified>`               | `open-forge doctor`                 |
| status.extension-lifecycle-untrusted    | warning  | lifecycle-unavailable   | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.extension-lifecycle-untrusted`).                                   | `open-forge doctor`                 |
| status.extension-lifecycle-incomplete   | warning  | lifecycle-unavailable   |                                                                                                           |                                     |
| status.extension-lifecycle-blocked      | error    | lifecycle-blocked       |                                                                                                           |                                     |
| status.extension-source-unavailable     | warning  | local                   | `The source of the <id> Extension, <path>, cannot be read, so its files were not compared.` (subject: id) | `open-forge extension inspect <id>` |
| status.extension-target-changed         | warning  | local                   | [`status.message.update-the-extension-file-that-needs-attention`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs), [`status.phrase.changed-since-it-was-installed-by`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.extension-target-changed`).                                                          | `open-forge extension update <id>`  |
| status.extension-target-missing         | warning  | local                   | [`shared.phrase.missing-it-was-installed-by`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedPhrases.cs), [`status.message.update-the-extension-file-that-needs-attention`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.extension-target-missing`).                                                               | `open-forge extension update <id>`  |
| status.extension-target-unavailable     | warning  | local                   | [`shared.label.could-not-be-read`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedText.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.extension-target-unavailable`).                                                                               | `open-forge doctor`                 |
| status.extension-target-blocked         | error    | local                   | [`status.phrase.could-not-be-checked-safely`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.extension-target-blocked`).                                                           | `open-forge doctor`                 |
| status.recovery-candidate-verified      | warning  | local                   | [`shared.phrase.a-recovery-bundle-from-an-earlier-command-is-kept-at`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedPhrases.cs), [`status.message.a-recovery-bundle-from-an-earlier-command-is-kept`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs), [`status.message.review-and-remove-the-recovery-data`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.recovery-candidate-verified`).                                            | `open-forge cleanup`                |
| status.recovery-draft-incomplete        | warning  | local                   | [`shared.phrase.an-unfinished-recovery-draft-is-at-a-command-did-not-finish`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedPhrases.cs), [`status.message.an-unfinished-recovery-draft-is-present-a-command-did-not-finish`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs), [`status.message.review-and-remove-the-recovery-data`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.recovery-draft-incomplete`).                                    | `open-forge cleanup --dry-run`      |
| status.recovery-final-malformed         | warning  | local                   | [`status.message.review-and-remove-the-recovery-data`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs), [`status.message.the-recovery-bundle-is-damaged-and-cannot-be-used`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs), [`status.phrase.the-recovery-bundle-at-is-damaged-and-cannot-be-used`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.recovery-final-malformed`).                                            | `open-forge cleanup --dry-run`      |
| status.recovery-final-unsupported       | warning  | local                   | [`status.message.review-and-remove-the-recovery-data`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs), [`status.message.the-recovery-bundle-was-written-by-an-unsupported-version`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs), [`status.phrase.the-recovery-bundle-at-was-written-by-an-unsupported-version`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.recovery-final-unsupported`).                                    | `open-forge cleanup --dry-run`      |
| status.recovery-final-unavailable       | warning  | local                   | [`status.message.the-recovery-bundle-could-not-be-read`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs), [`status.phrase.the-recovery-bundle-at-could-not-be-read`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.recovery-final-unavailable`).                                                        | `open-forge doctor`                 |
| status.recovery-catalogue-unavailable   | warning  | local                   | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.recovery-catalogue-unavailable`).                                                         | `open-forge doctor`                 |
| status.library-record-malformed         | error    | local                   | [`shared.phrase.the-library-section-of-agents-open-forge-lock-json-is-invalid`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.library-record-malformed`).                               | `open-forge doctor`                 |
| status.library-record-unavailable       | warning  | lifecycle-unavailable   | [`status.message.the-library-section-of-agents-open-forge-lock-json-could-not-be-read`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.library-record-unavailable`).                                  | `open-forge doctor`                 |
| status.library-source-root-invalid      | error    | local                   | `The source folder of the <id> Library, <path>, is not a folder inside the workspace.` (subject: id)      | `open-forge library inspect <id>`   |
| status.library-source-root-aliased      | error    | local                   | [`status.message.inspect-the-library-source-and-link-boundary`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs), [`status.phrase.the-source-folder-of-the-library-resolves-to-an-ambiguous-location`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.library-source-root-aliased`).                       | `open-forge library inspect <id>`   |
| status.library-source-root-unavailable  | warning  | local                   | [`status.message.inspect-the-library-source-and-link-boundary`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs), [`status.phrase.the-source-folder-of-the-library-cannot-be-read`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.library-source-root-unavailable`).                                          | `open-forge library inspect <id>`   |
| status.library-projection-missing       | warning  | local                   | [`status.message.synchronize-the-missing-library-link`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs), [`status.phrase.missing-it-is-a-link-of-the-library`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.library-projection-missing`).                                                       | `open-forge library sync <id>`      |
| status.library-projection-changed       | warning  | local                   | [`status.message.inspect-the-library-source-and-link-boundary`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs), [`status.phrase.is-no-longer-the-link-the-library-created`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.library-projection-changed`).                                                  | `open-forge library inspect <id>`   |
| status.library-projection-unavailable   | warning  | local                   | [`shared.label.could-not-be-checked`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Shared/SharedText.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.library-projection-unavailable`).                                                                            | `open-forge doctor`                 |
| status.library-projection-blocked       | error    | local                   | [`status.phrase.could-not-be-checked-safely`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusPhrases.cs); [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.library-projection-blocked`).                                                           | `open-forge doctor`                 |
| status.library-extension-collision      | error    | local                   | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Status/Shared/Wording/StatusWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`status.library-extension-collision`).                                      | `open-forge doctor`                 |
| status.operation-failed                 | error    | operation-failed        |                                                                                                           |                                     |
| status.interrupted                      | error    | cancelled |                                                                                                           |                                     |

Rows written as `<path>  <phrase>` are rendered as finding rows with the
phrase as the message; the title is the phrase's first words capitalized
(`Changed since it was installed`).
For `status.generated-navigation-metadata-invalid`, the subject is the actual
malformed source path and the cause is the observed metadata error. The finding
is emitted from the typed observation and not from message-text parsing.

## Scenarios

### Catalogue situations

`not-installed`, `healthy`, `healthy-with-extension`, `changed-managed-file`,
`missing-managed-file`, `stale-entries`, `recovery-bundle-present`,
`library-link-missing`, `no-ownership-record`, `unreadable-entry-file`,
`blocked-workspace`, `invalid-input`. Each at `minimal`, `standard`, `full`,
text and JSON.

Each status has one representative native text transcript above. JSON uses the same status and command facts under the schema-3 envelope.

### Open maintainer questions

The native renderer and the catalogue still disagree on four frozen wording points: the lifecycle-unavailable message, the generated-navigation-blocked message, the cancelled/interrupted message, and the recovery-catalogue-unavailable message. This contract records the current report shape without choosing which spelling is authoritative. **Maintainer decision remains open.**
## Non-Goals

`status` does not:

- Diagnose complete Framework health.
- Recommend moving content into a scope.
- Assign an optimization score, grade, or context budget.
- Treat a larger context, added category, or removed category as a problem.
- Count scopes by guessing which routed `slugs` perform that role.
- Validate every route, link, fragment, generated region, or overwrite.
- List every on-demand source or every managed-file observation.
- Claim that continuity content reloads on every request.
- Use a model-specific tokenizer, billing calculation, or performance estimate.
- Mutate, repair, clean, install, or restore anything.
- Enumerate a complete Library source tree, discover unregistered mappings, adopt
  an existing link, or infer Library ownership from filenames, paths, or bytes.
- Inspect target-adjacent files or report version-control facts.
- Auto-bind a bundle from an original workspace path after the workspace moves.
- Roll back or restore a target from a recovery bundle.

Complete diagnosis and recommendations belong to `doctor`. Status never lists
repair or lifecycle proposals and never runs cleanup. Exact repair and the
separate [cleanup operation](../cleanup/interface.md) remain distinct mutating
operations.

## Public Verification

Implementation evidence must cover:

- Exact CWD and `--workspace` selection without discovery.
- Installed, uninstalled, incomplete, and unsafe workspace states.
- Initial startup measurement from the embedded Framework payload.
- Current target-sensitive startup and continuity closure measurement.
- Physical file counting, base and overwrite layers, characters, UTF-8 size, and
  deterministic token estimates.
- Signed current-minus-initial differences, zero differences, unavailable
  operands, non-applicable comparisons, and non-fabricated rendering.
- Total available context and startup percentage, including both-zero,
  unavailable-operand, positive-current/zero-total, and non-applicable cases.
- Continuity as a non-additive startup subset.
- Largest continuity-source combination, ordering, ties, and fewer-than-three
  cases, including every ordered contribution in JSON and omission in minimal
  human output.
- Root-category additions, removals, reordering, and neutral result behavior.
- Absent, trusted, untrusted, incomplete, and blocked lifecycle sections,
  including state retention, absent `0 recorded`, trusted-empty `0`, and
  `none recorded` managed-file summaries.
- Installed Extension IDs, ownership, and recorded paths that remain readable
  when their package source is unavailable.
- Current, changed, missing, and shared managed-file counts.
- Zero and present verified-final and incomplete-draft counts; every exact-name
  candidate's path and integrity condition; malformed, unsupported, and
  unavailable finals excluded from the verified count; and result precedence for
  incomplete drafts, invalid finals, unavailable facts, and unsafe boundaries.
- Empty, arbitrary binary, and large payload-entry validation with exact declared
  lengths and hashes, bounded buffers and memory independent of entry size, and
  no extraction, disclosure, retention, or materialization.
- No live-target hashing, target-state classification, or activity inference
  from bundle contents or the persistent external lock file.
- Completed, completed-with-warnings, incomplete, invalid-input, blocked,
  failed, and cancelled outcomes.
- Human and structured output from the same typed result.
- Human result and error stream assignment, one complete JSON result for every
  status, bounded diagnostics on stderr, and minimal one-line `Next:` behavior.
- Repeat invocations producing the same semantic result for unchanged CLI and
  workspace bytes.
- Evidence that status does not parse ordinary links, build the complete content
  graph, inspect unrelated workspace files, or mutate anything.
- Readable ordinary malformed metadata in an otherwise projectable generated
  region remains a warning/Attention2 result with structural `unavailable`, the
  actual source path and cause, and retained known facts; unreadable or
  coverage-incomplete input remains `incomplete`, unsafe or ambiguous input
  remains `blocked`, and native failures remain strict.
- Bounded Workspace Library record, source-root, and registered-link facts,
  including destination-derived source IDs kept separate from Library IDs,
  complete safely observed drift as `completed-with-warnings`, unavailable coverage as
  `incomplete`, unsafe ambiguity or Library/Extension collision as `blocked`,
  and no complete source inventory or adoption.

Direct tests should prove measurement, comparison, ordering, classifications, and
semantic results. Focused integration tests should use real temporary workspaces,
embedded assets, the shared ownership lock, and filesystem state. A small built Native AOT process suite
should prove parsing, output, exit behavior, and packaged payload comparison.

The [Shared Result Coordinates](../shared/result-coordinates/interface.md) define
the exact numeric exit values, structured schema, and JSON value representation.
The [Mutation And Recovery Technical
Design](../../technical-designs/mutation-and-recovery.md) defines exact
recovery-bundle identities and retention mechanics. The [CLI
Architecture](../../architecture.md) defines .NET source boundaries and keeps
exact Status diagnostic fields and redaction as bounded command-local
implementation details. Gate 5 must prove those details, source-generated
YamlDotNet and STJ serialization, fixed Markdig where used, real `System.IO`,
Native AOT, OS locking, isolated tests, and package journeys.

## Related Current Sources

- [Status Command Contract Set](_status.md)
- [Status Behavior Contract](behavior.md)
- [CLI Command Contract Set — Interface Contract](../../command-contract-set.md#interface-contract)
- [Context Interface Contract](../context/interface.md)
- [Global CLI Flags Interface Contract](../shared/global-flags/interface.md)
- [CLI Source References Interface Contract](../shared/source-references/interface.md)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)
- [Framework loading contract](../../../framework/routing/loading.md)
- [Routing model](../../../framework/routing/model.md)
- [Route scope and inheritance](../../../framework/routing/scope.md)
- [Overwrite customization](../../../framework/routing/overwrites.md)

Library registered links retain separate source-relative and mapped destination
paths. A destination outside the existing `.agents` source-reference contract has
`sourceId: null`; neither the management ID nor an empty string replaces it.





## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`status.help.syntax`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusText.cs).

<!-- @OpenForgeTextRef shared.label.changed-since-it-was-installed -->
<!-- @OpenForgeTextRef shared.label.could-not-be-checked -->
<!-- @OpenForgeTextRef shared.label.could-not-be-read -->
<!-- @OpenForgeTextRef shared.phrase.a-recovery-bundle-from-an-earlier-command-is-kept-at -->
<!-- @OpenForgeTextRef shared.phrase.an-unfinished-recovery-draft-is-at-a-command-did-not-finish -->
<!-- @OpenForgeTextRef shared.phrase.has-no-entries-section -->
<!-- @OpenForgeTextRef shared.phrase.missing-it-was-installed-by -->
<!-- @OpenForgeTextRef shared.phrase.the-library-section-of-agents-open-forge-lock-json-is-invalid -->
<!-- @OpenForgeTextRef status.help.syntax -->
<!-- @OpenForgeTextRef status.label.missing-it-was-installed-by-the-framework -->
<!-- @OpenForgeTextRef status.message.a-recovery-bundle-from-an-earlier-command-is-kept -->
<!-- @OpenForgeTextRef status.message.an-unfinished-recovery-draft-is-present-a-command-did-not-finish -->
<!-- @OpenForgeTextRef status.message.inspect-the-library-source-and-link-boundary -->
<!-- @OpenForgeTextRef status.message.rebuild-the-entries-section -->
<!-- @OpenForgeTextRef status.message.review-and-remove-the-recovery-data -->
<!-- @OpenForgeTextRef status.message.synchronize-the-missing-library-link -->
<!-- @OpenForgeTextRef status.message.the-entries-section-could-not-be-read -->
<!-- @OpenForgeTextRef status.message.the-entries-section-is-stale -->
<!-- @OpenForgeTextRef status.message.the-library-section-of-agents-open-forge-lock-json-could-not-be-read -->
<!-- @OpenForgeTextRef status.message.the-recovery-bundle-could-not-be-read -->
<!-- @OpenForgeTextRef status.message.the-recovery-bundle-is-damaged-and-cannot-be-used -->
<!-- @OpenForgeTextRef status.message.the-recovery-bundle-was-written-by-an-unsupported-version -->
<!-- @OpenForgeTextRef status.message.there-is-no-entries-section -->
<!-- @OpenForgeTextRef status.message.update-the-extension-file-that-needs-attention -->
<!-- @OpenForgeTextRef status.message.update-the-framework-files-that-need-attention -->
<!-- @OpenForgeTextRef status.phrase.changed-since-it-was-installed-by -->
<!-- @OpenForgeTextRef status.phrase.could-not-be-checked-safely -->
<!-- @OpenForgeTextRef status.phrase.is-no-longer-the-link-the-library-created -->
<!-- @OpenForgeTextRef status.phrase.missing-it-is-a-link-of-the-library -->
<!-- @OpenForgeTextRef status.phrase.the-entries-section-of-could-not-be-read -->
<!-- @OpenForgeTextRef status.phrase.the-entries-section-of-is-stale -->
<!-- @OpenForgeTextRef status.phrase.the-recovery-bundle-at-could-not-be-read -->
<!-- @OpenForgeTextRef status.phrase.the-recovery-bundle-at-is-damaged-and-cannot-be-used -->
<!-- @OpenForgeTextRef status.phrase.the-recovery-bundle-at-was-written-by-an-unsupported-version -->
<!-- @OpenForgeTextRef status.phrase.the-source-folder-of-the-library-cannot-be-read -->
<!-- @OpenForgeTextRef status.phrase.the-source-folder-of-the-library-resolves-to-an-ambiguous-location -->

## Approved Journey Wording References

The following stable IDs link the approved journey behavior above to its typed
human-wording factories. Independently reviewed snapshots and state assertions
remain the output evidence.

- [StatusWording.cs](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Status/StatusWording.cs)
  <!-- @OpenForgeTextRef status.wording.generated-navigation-metadata-invalid -->
  <!-- @OpenForgeTextRef status.title.generated-navigation-metadata-invalid -->
