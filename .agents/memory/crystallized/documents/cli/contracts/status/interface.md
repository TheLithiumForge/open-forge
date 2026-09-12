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
Flags](../shared/global-flags/interface.md) contract defines `--workspace`, `--json`,
`--view`, `--verbose`, `--help`, and `--version`. All six apply to `status` under
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
- The exact `.agents/open-forge.lifecycle.json` lifecycle document, schema v1,
  with isolated `framework` and `extensions` sections.
- The exact `.agents/open-forge.libraries.json` Workspace Library record, schema
  v1, including its typed Library IDs, source-root facts, and registered path
  mappings.
- The current user's external recovery store at
  `Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.None)/OpenForge/recovery/v1`, limited to
  exact-name final and draft candidates for the selected normalized physical
  workspace path.

The command may enumerate and stream supported context files to measure their
content. It does not build the complete context graph, parse ordinary links or
named sections, validate every route, inspect unrelated workspace files, or
construct a mutation plan.

### Unified Lifecycle Facts

Status reads `.agents/open-forge.lifecycle.json`, schema v1, as a common envelope
with isolated `framework` and `extensions` sections. Co-location does not merge
their authority. Framework source, target, and region facts remain separate from
Extension package, dependency, ownership, and baseline facts. Status reports
only facts it can read safely and never writes, rebaselines, repairs, or publishes
a lifecycle section. The document stores no plan, runtime history, journal,
recovery-bundle evidence, or session. Files outside this exact path are ordinary
workspace content, not lifecycle input.

For a trusted Framework section with available source evidence, a valid derived
Entries target is current when the safely observed generated region matches the
current authored topology at that exact path. A different recorded generated
fingerprint alone is not drift after Extension installation or indexing. This
read-only comparison preserves the recorded fingerprint and does not rebaseline
lifecycle state or authorize mutation. Authored targets, stale or malformed
navigation, missing targets and unavailable or blocked evidence retain their
separate checks.

Each section retains its finite state:

| State        | Status meaning                                                                                                                                                                 |
| ------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `absent`     | Complete inspection proves that no expected managed state, managed boundary, or recovery-bundle residual exists. It does not claim that unmanaged or idless content is absent. |
| `trusted`    | Supported document and section facts bind the exact workspace and managed identities, preserve internal consistency, and provide complete verifiable coverage.                 |
| `untrusted`  | Some lifecycle facts are readable, but provenance, integrity, compatibility, identity, or coverage cannot establish current trust.                                             |
| `incomplete` | Safe required lifecycle or source coverage is unavailable.                                                                                                                     |
| `blocked`    | Malformed, ambiguous, colliding, or unsafe lifecycle identity prevents a safe classification.                                                                                  |

The absence of `.agents/open-forge.lifecycle.json` alone does not establish an
unmanaged or empty workspace. `absent` requires complete inspection of the
expected managed footprint, managed boundaries, and exact-name recovery
candidates. Unsupported or ambiguous schema facts are `incomplete` when safely
unavailable and `blocked` when unsafe. The persistent external workspace-lock
file is not lifecycle authority or recovery-bundle evidence and does not affect
these states.

An invalid or unavailable package source does not erase independently readable
installed Extension IDs, ownership, recorded paths, or lifecycle facts. Status
marks source-dependent comparison as unavailable or incomplete instead of
claiming a current source, update plan, or managed no-op. A path, matching bytes,
matching fingerprint, or familiar route never promotes an untrusted state.

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
- Do not count provider bridges, the lifecycle document, recovery bundles,
  drafts, or other operational metadata as context.
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

The expanded human result shows at most the three logical continuity sources
with the largest UTF-8 contribution. Compact human output omits this section.
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

| State     | Meaning                                                         |
| --------- | --------------------------------------------------------------- |
| `current` | Current semantic identity matches the recorded baseline         |
| `changed` | The path exists but its semantic identity differs from baseline |
| `missing` | The recorded path does not exist                                |

Shared owner sets do not multiply the file count. These are lifecycle facts, not
claims about runtime meaning, user intent, safe replacement, removal authority,
or repairability. Unmanaged files do not enter this summary.

Supported Markdown lifecycle comparisons use the conservative
`open-forge-markdown-v1` fingerprint policy. The CLI distribution embeds
Framework and first-party Extension assets with deterministic inventory and hash
proof; that proof is distributed-source identity, not evidence of a selected
workspace's current installation or of a proven runtime implementation.

### Workspace Libraries

Status reads the exact consumer-owned `.agents/open-forge.libraries.json`
record, schema v1, as a bounded projection catalogue. The record is separate
from `.agents/open-forge.lifecycle.json` and does not grant Framework or
Extension ownership. A readable record reports its exact `id`, `sourceRoot`, `destinationRoot`, and
ordered `paths` entries. Status derives bounded mapping facts for each entry—
`sourcePath`, `destinationPath`, and `expectedRelativeLink`—without changing the
record. A missing record is an absent Library record, not an inference that
source content or projections are absent; malformed, unavailable, or unsafe
record identity is reported as the corresponding bounded condition.

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
`attention`; unavailable coverage maps to `incomplete`; unsafe or ambiguous
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

Human output starts with the observed installation outcome, status, workspace
identity and selection method. Findings that explain a non-complete result appear
before the summaries, grouped by exact code, status and cause. Each distinct
subject remains visible. Generated-navigation findings may refer to Routes
instead of repeating paths already present there as non-current observations;
unmatched subjects remain beside their finding. The sections are Context, Routes, Framework, Extensions,
Libraries and Recovery, followed by the supported next action.

Expanded is the default. Compact retains startup and continuity measurements,
root changes, lifecycle and Library state, recovery candidates and required next
actions. Current generated-navigation paths are summarized by count in compact;
every non-current path remains visible. Expanded adds all observed navigation
paths, total context and the at-most-three largest continuity sources. Both views
show navigation counts by actual state; a check that could not finish is never
reported as current or as a successful empty result. Paths are not truncated.

Both views remain understandable without colour. Zero values stay visible when
omission would make absence ambiguous. Expanded explains the same observed
facts; it does not run extra checks.

Both views show separate verified-final and incomplete-draft counts and report
every exact-name candidate's path and integrity condition. A malformed,
unsupported, or unavailable final remains visible as an issue and is never folded
into the verified-final count.

Primary human rendering for `complete`, `attention`, and `incomplete` goes to
stdout. Primary human rendering for `invalid`, `blocked`, `failed`, and
`interrupted` goes to stderr. `--json` writes one complete structured result to
stdout for every semantic result. Separate bounded diagnostics go to stderr, and
human text is not mixed into JSON stdout.

Expanded human output retains lifecycle trust and coverage state. Compact output
may combine that state with its Extension and managed-file summaries. Zero values
remain visible. An absent section uses `0 recorded` Extensions, while a trusted
empty section uses `0`; both use `none recorded` managed files.

Both views retain the bounded Workspace Library record state, IDs, source-root
availability, registered-link states, and the current/missing/changed/blocked/
unavailable counts. They do not render a complete source inventory or turn a
registered-link drift observation into a repair action.

When the typed semantic status is `attention`, human output renders it as
`requires attention` because the phrase is clearer on first read.

Compact view uses at most one operation-level `Next:` line, and only when it is
useful. It uses these rules:

- `complete`: no `Next:` line.
- `attention`: `Next: open-forge doctor`.
- `incomplete`: `Next: open-forge doctor`, unless a more direct safe correction
  is known, in which case it names that correction.
- `invalid`: `Next: correct the named input`.
- `blocked`: `Next: correct the named workspace or safety boundary and rerun`.
- `failed`: `Next: report the failure and retry with bounded diagnostics`.
- `interrupted`: `Next: rerun the same request`.

Status never lists repair or lifecycle proposals in `Next:` or elsewhere in its
summary.

Illustrative excerpt from expanded output:

```text
Open Forge is installed.
Status: requires attention
Workspace: /work/demo
Selected by: current directory

Context
  Shipped startup: 2 files, 9 characters, 11 bytes, ~3 tokens
  Startup: 4 files, 21 characters, 25 bytes, ~6 tokens
  Difference: 2 files, 12 characters, 14 bytes, ~3 tokens
  Continuity (may load again): 2 files, 12 characters, 15 bytes, ~3 tokens
Startup share: 25%
  Total available context: 7 files, 81 characters, 100 bytes, ~21 tokens
Largest continuity sources
  memory/alpha: 10 bytes
  memory/beta: 10 bytes

Routes
Root categories: 3
  Added: custom
  Removed: patterns
Generated navigation:
  1 need updating
  .agents/directives/_directives.md: need updating
```

The values are illustrative. They do not claim to be a current measurement of
this repository.

Empty lists use `none`; an empty observed navigation list uses `none observed`.
Absent lifecycle sections use `none recorded` for managed files. An uninstalled workspace states
that Open Forge is not installed and marks current startup, Difference, startup
percentage, continuity, and root-category facts as not-applicable. It keeps the
initial measurement when the embedded payload is available and keeps total
available context numeric when that physical inventory is safely measurable. It
never fabricates zero for an unavailable or not-applicable fact.

## Structured Output

`--json` exposes the same typed facts used by human output:

- Workspace and selection method
- Framework installation state
- Initial, current, and difference context measurements
- Total available and continuity measurements
- Deterministically ordered continuity-source contributions
- Root-category count, additions, and removals
- Extension and managed-file states
- Framework and Extension lifecycle trust, ownership, and source-
  availability observations
- Bounded Workspace Library record state, IDs, source-root availability,
  registered-link observations, destination-derived source IDs, and partitioned
  current/missing/changed/blocked/unavailable counts
- Separate verified-final and incomplete-draft counts, plus every exact-name
  candidate's path, kind, and integrity condition; malformed, unsupported, and
  unavailable finals remain issue items and are not counted as verified
- Measurement availability and semantic result

The shared schema-v1 envelope remains exactly as defined by the [Shared Result
Coordinates](../shared/result-coordinates/interface.md). Its non-null
command-local `result` object uses camel-case members in exactly this order:

```text
result: {
  installation,
  context,
  structure,
  lifecycle,
  library,
  recovery,
  findings
}
```

The command-local graph is:

```text
installation: {
  state,
  entryPath,
  loaderPath
}

context: {
  tokenEstimator,
  startup: {
    initial,
    current,
    difference
  },
  totalAvailable,
  startupPercentage,
  continuity,
  continuitySources: [{
    sourceId,
    utf8Bytes,
    layers: [{ path, utf8Bytes }]
  }]
}

structure: {
  rootCategories: {
    count,
    added,
    removed
  },
  generatedNavigation: [{ path, state }]
}

lifecycle: {
  framework: {
    state,
    sourceAvailability,
    targets: [{
      path,
      kind,
      sourceAssetPath,
      region,
      baselineFingerprint,
      fingerprintKind,
      state
    }]
  },
  extensions: {
    state,
    sourceAvailability,
    installed: [{
      id,
      version,
      source,
      sourceAvailability,
      dependencies,
      paths
    }],
    managedFiles: {
      counts: {
        current,
        changed,
        missing,
        unavailable,
        blocked
      },
      targets: [{
        path,
        owners,
        baselineFingerprint,
        fingerprintKind,
        state
      }]
    }
  }
}

library: {
  state,
  record: { path, state },
  records: [{
    id,
    sourceRoot,
    destinationRoot,
    sourceRootState,
    sourceAvailability,
    registeredLinks: {
      registered,
      counts: { current, missing, changed, blocked, unavailable },
      links: [{
        sourcePath,
        destinationPath,
        expectedRelativeLink,
        sourceId,
        state
      }]
    }
  }],
  counts: { registered, current, missing, changed, blocked, unavailable }
}

recovery: {
  verifiedFinals,
  incompleteDrafts,
  candidates: [{ path, kind, integrity }]
}

findings: [{
  code,
  status,
  subject,
  cause
}]
```

Every array is present and non-null, including an empty array. Every object in
this graph is present and non-null. Only `installation.entryPath`,
`installation.loaderPath`, a Framework target's `sourceAssetPath` and `region`,
an installed Extension's `version` and `source`, a finding's `subject`, and the
`value` member of the typed numeric values below may be `null`.

Each measurement uses the exact member order `files`, `characters`,
`utf8Bytes`, and `estimatedTokens`. Each member is one integer value object:

```text
{ state: "available" | "unavailable" | "not-applicable", value: integer | null }
```

`startupPercentage` uses the same member order and availability states, with a
finite decimal JSON number or `null` as `value`. A numeric `value` is present
exactly when `state` is `available`; it is `null` for `unavailable` and
`not-applicable`. Signed Difference values remain numeric and may be negative.
All other count and byte values are nonnegative. Zero is an available numeric
value and is never used for either unavailable state. `tokenEstimator` is the
exact value `ceiling-characters-divided-by-four`.

The remaining command-local finite values are:

| Coordinate                     | Values                                                                      |
| ------------------------------ | --------------------------------------------------------------------------- |
| `installation.state`           | `installed`, `uninstalled`, `incomplete`, `blocked`                         |
| lifecycle `state`              | `absent`, `trusted`, `untrusted`, `incomplete`, `blocked`                   |
| `sourceAvailability`           | `available`, `unavailable`, `not-applicable`                                |
| Framework target `kind`        | `file`, `managed-region`, `generated-region`                                |
| managed target `state`         | `current`, `changed`, `missing`, `unavailable`, `blocked`                   |
| generated-navigation `state`   | `current`, `changed`, `missing`, `unavailable`, `blocked`, `not-applicable` |
| recovery candidate `kind`      | `final`, `draft`                                                            |
| recovery candidate `integrity` | `verified`, `incomplete`, `malformed`, `unsupported`, `unavailable`         |
| Library `state`                | `absent`, `trusted`, `incomplete`, `blocked`                                |
| Library record `state`         | `missing`, `complete`, `invalid`, `unavailable`, `blocked`                  |
| Library `sourceRootState`      | `available`, `missing`, `unavailable`, `invalid`, `blocked`                 |
| Library `sourceAvailability`   | `available`, `unavailable`, `not-applicable`                                |
| registered-link `state`        | `current`, `missing`, `changed`, `blocked`, `unavailable`                   |
| finding `status`               | the seven exact shared semantic status values                               |

Continuity sources follow their contracted contribution order. Their layers
remain in base-then-overwrite order. Root `added` and `removed` arrays retain
their contracted source order. Framework and Extension targets are ordered by
canonical path. Installed Extensions are ordered by ID; their dependencies,
paths, and target owners use deterministic ordinal order. Recovery candidates
are ordered by path, then kind, then integrity. Library records are ordered by
Library ID; registered links are ordered by canonical destination path, then
source path. Findings are ordered by finding code, subject, and cause after
semantic precedence is formed.

The exact finite Status finding codes and their status are:

| Code                               | Status        |
| ---------------------------------- | ------------- |
| `invalid-input`                    | `invalid`     |
| `workspace-unavailable`            | `blocked`     |
| `workspace-not-directory`          | `blocked`     |
| `workspace-unsafe`                 | `blocked`     |
| `entry-unavailable`                | `incomplete`  |
| `embedded-framework-unavailable`   | `incomplete`  |
| `context-inventory-incomplete`     | `incomplete`  |
| `startup-context-unavailable`      | `incomplete`  |
| `continuity-context-unavailable`   | `incomplete`  |
| `root-categories-unavailable`      | `incomplete`  |
| `generated-navigation-changed`     | `attention`   |
| `generated-navigation-missing`     | `attention`   |
| `generated-navigation-unavailable` | `incomplete`  |
| `generated-navigation-blocked`     | `blocked`     |
| `framework-lifecycle-untrusted`    | `incomplete`  |
| `framework-lifecycle-incomplete`   | `incomplete`  |
| `framework-lifecycle-blocked`      | `blocked`     |
| `framework-target-changed`         | `attention`   |
| `framework-target-missing`         | `attention`   |
| `framework-target-unavailable`     | `incomplete`  |
| `framework-target-blocked`         | `blocked`     |
| `extension-lifecycle-untrusted`    | `incomplete`  |
| `extension-lifecycle-incomplete`   | `incomplete`  |
| `extension-lifecycle-blocked`      | `blocked`     |
| `extension-source-unavailable`     | `incomplete`  |
| `extension-target-changed`         | `attention`   |
| `extension-target-missing`         | `attention`   |
| `extension-target-unavailable`     | `incomplete`  |
| `extension-target-blocked`         | `blocked`     |
| `recovery-candidate-verified`      | `attention`   |
| `recovery-draft-incomplete`        | `incomplete`  |
| `recovery-final-malformed`         | `incomplete`  |
| `recovery-final-unsupported`       | `incomplete`  |
| `recovery-final-unavailable`       | `incomplete`  |
| `recovery-catalogue-unavailable`   | `incomplete`  |
| `library-record-malformed`         | `blocked`     |
| `library-record-unavailable`       | `incomplete`  |
| `library-source-root-invalid`      | `blocked`     |
| `library-source-root-aliased`      | `blocked`     |
| `library-source-root-unavailable`  | `incomplete`  |
| `library-projection-missing`       | `attention`   |
| `library-projection-changed`       | `attention`   |
| `library-projection-unavailable`   | `incomplete`  |
| `library-projection-blocked`       | `blocked`     |
| `library-extension-collision`      | `blocked`     |
| `operation-failed`                 | `failed`      |
| `interrupted`                      | `interrupted` |

The shared `command`, `status`, `workspace`, and `next` coordinates are not
duplicated under `result`. Numeric fields remain numeric. Signed differences
remain derived from the two measured inputs. Unavailable, zero, and
not-applicable values remain distinct and are never substituted for one another.
The Shared Result Coordinates continue to define shared envelope compatibility
and exit mapping. This Interface owns compatibility for the exact Status-local
graph, field order, presence, nullability, and finite values above.

For an attention result, structured output keeps the semantic status value
`attention`; only human presentation uses `requires attention`.

`--verbose` may add bounded diagnostic evidence under the shared [Global CLI
Flags](../shared/global-flags/interface.md) contract. Those diagnostics use
stderr. It does not change collection, measurements, ordering, semantic result,
or exit behavior, and it does not add Doctor diagnosis or recommendations. The
exact Status diagnostic fields and redaction remain bounded command-local
implementation details under the accepted CLI Architecture and Gate 5 evidence.

## Architecture Boundary

These implementation boundaries are routed to their exact current authorities
and are not duplicated by this contract:

- **Structured schema and compatibility.** JSON exposes the same typed facts as
  human output and keeps numeric, zero, unavailable, and not-applicable
  distinctions. Exact field names, schema version, compatibility rules, and JSON
  representations follow the [Shared Result
  Coordinates](../shared/result-coordinates/interface.md); concrete
  source-generated serialization remains in the [CLI
  Architecture](../../architecture.md).
- **Numeric process exits.** Semantic result categories are accepted and
  structured `attention` remains `attention`; human output says `requires
attention`. Numeric process-exit mapping follows the [Shared Result
  Coordinates](../shared/result-coordinates/interface.md).
- **Token-estimation implementation.** The planning estimate is
  `ceiling(characters / 4)` and is explicitly not a model tokenizer, billing
  value, context guarantee, latency estimate, or provider count. The
  implementation and rounding details beyond the stated display rule follow the
  accepted Architecture.
- **Recovery-bundle identity and cleanup boundary.** Status reports every
  exact-name final and draft candidate in the external
  LocalApplicationData recovery store for the selected normalized physical
  workspace path, with verified finals and incomplete drafts counted separately
  and invalid finals retained as issue items. It does not read live targets or
  infer activity, does not scan target-adjacent files, and validates payload
  lengths and hashes only by bounded streaming without extracting, disclosing,
  retaining, or materializing payload bytes. It does not acquire the workspace
  lease or report or infer activity. The separate [cleanup
  contract](../cleanup/interface.md) owns lease-validated candidate deletion.
  Exact identity, storage, and bounded-validation implementation follow the
  [Mutation And Recovery Technical
  Design](../../technical-designs/mutation-and-recovery.md); the shared schema
  follows the [Shared Result Coordinates](../shared/result-coordinates/interface.md).
- **Diagnostics and redaction.** `--verbose` may add bounded diagnostic evidence
  without changing collection, ordering, semantic result, or exit behavior. The
  exact Status diagnostic fields and redaction remain bounded command-local
  implementation details under the accepted Architecture and Gate 5 evidence.
  Their bounded diagnostic stream is stderr as stated above.
- **.NET source boundaries.** .NET Native AOT is the accepted canonical
  implementation direction for the CLI. Status has no command-local Technical
  Design; its concrete module, parser, serializer, filesystem, and source
  boundaries follow the accepted Architecture.

## Semantic Results

| Result        | Meaning                                                                                                                                                                                                                       | Process completion status        |
| ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------- |
| `complete`    | Every applicable status fact was measured and no attention condition exists                                                                                                                                                   | Shared result-coordinate mapping |
| `attention`   | Measurement completed, but trusted managed files are changed or missing, a registered Library projection is safely missing or changed, a verified recovery final is present, or a finite lifecycle/source observation remains | Shared result-coordinate mapping |
| `incomplete`  | Safe facts are available, but one or more applicable measurements or bounded Library facts are unavailable or incomplete, including an incomplete draft or a safely reportable invalid final                                  | Shared result-coordinate mapping |
| `invalid`     | Command input does not follow the accepted grammar                                                                                                                                                                            | Shared result-coordinate mapping |
| `blocked`     | The command cannot establish the selected workspace or a safe inspection boundary                                                                                                                                             | Shared result-coordinate mapping |
| `failed`      | An unexpected internal failure prevents normal completion                                                                                                                                                                     | Shared result-coordinate mapping |
| `interrupted` | The caller cancels or interrupts the operation before completion                                                                                                                                                              | Shared result-coordinate mapping |

An uninstalled workspace is a valid completed state when its absence can be
established safely. Differences in context size and added or removed root
categories do not produce `attention` by themselves.

A safely observed Library projection drift (`missing` or `changed`) produces
`attention` only when the bounded record, source-root, and link coverage is
complete. Unavailable Library facts produce `incomplete`; malformed, aliased,
colliding, or otherwise unsafe Library identity produces `blocked`. Status does
not treat the bounded record view as a complete source inventory.

Recovery follows the same existing result precedence: an unavailable recovery
fact, an exact-name draft, or a safely bounded malformed or unsupported final
selects `incomplete`; an unsafe or ambiguous recovery inspection boundary selects
`blocked`. Invalid input, unexpected failure, and interruption retain their
existing precedence.

## Errors

Every error names the status operation, affected workspace or fact, direct cause,
and a useful next action when one exists.

- Unexpected operands or operation-specific flags are invalid.
- A missing `--workspace` value is invalid under the global flag contract.
- A missing, unavailable, or non-directory selected workspace is blocked.
- Unsafe containment or physical identity blocks affected inspection.
- A malformed startup route or unreadable required context produces an
  incomplete result when safe facts remain available.
- A malformed, unsupported, or unavailable lifecycle section produces an
  incomplete or blocked managed-state summary rather than guessed counts.
- A malformed or unavailable `.agents/open-forge.libraries.json` record, source
  root, or registered destination produces the typed Library `blocked` or
  `incomplete` finding; a safely observed missing or changed projection is
  reported as `attention` and never repaired or adopted by Status.
- Installed Extension facts remain reportable as source-unavailable facts when
  package source bytes cannot be read; they are not presented as
  trusted current source or mutation authority.

Primary human results use stdout for `complete`, `attention`, and `incomplete`;
primary human errors use stderr for `invalid`, `blocked`, `failed`, and
`interrupted`. `--json` writes one complete result to stdout for every semantic
status, and separate bounded diagnostics use stderr.

## Scenarios

These scenarios cover the smallest valid invocation, exact workspace selection,
human-density and structured presentation, valid uninstalled state, and the
meaningful semantic boundaries without adding implementation mechanics to the
command contract. The [Shared Result
Coordinates](../shared/result-coordinates/interface.md) define the exact schema
and exits; the [Mutation And Recovery Technical
Design](../../technical-designs/mutation-and-recovery.md) defines exact recovery
identity; and the [CLI Architecture](../../architecture.md) defines .NET
boundaries while keeping exact Status diagnostic fields and redaction as bounded
command-local implementation details subject to Gate 5 evidence.

### Default current workspace

```text
open-forge status
```

The command uses the exact current working directory and the default expanded
human presentation. It returns the workspace, startup comparison, total and
continuity measurements, root changes, Framework and Extension lifecycle trust
and managed-file summary, the bounded Workspace Library record and registered-
link summary, separate verified-final and incomplete-draft counts, every
exact-name recovery candidate, and the applicable semantic result.

### Explicit workspace

```text
open-forge status --workspace ../another-workspace
```

The command uses that exact workspace value and reports the normalized selected
workspace and `--workspace` selection method. It does not search for another
workspace.

### Compact human presentation

```text
open-forge status --view=compact
```

The command keeps the public compact content listed under [Human Output](#human-output)
and omits optional explanation and largest-source detail. It omits the largest
source section and uses at most one operation-level `Next:` line under the
accepted result-specific rules.

### Structured presentation

```text
open-forge status --json
```

The command exposes one structured result containing the typed facts listed under
[Structured Output](#structured-output). The exact schema and numeric process
exit follow the [Shared Result
Coordinates](../shared/result-coordinates/interface.md). `--view` selects compact
or expanded JSON detail under the shared Global CLI Flags contract.

### Uninstalled workspace

```text
open-forge status --workspace ../uninstalled-workspace
```

When the selected directory is valid but Open Forge is not installed, the command
reports that state. If the absence is established safely, it can return
`complete`: the initial shipped measurement remains measured when the embedded
payload is available; current startup, Difference, startup percentage,
continuity, and root-category facts are not-applicable; and total available
context remains numeric when its physical inventory is safely measurable. An
applicable fact that cannot be measured is unavailable and makes the result
`incomplete`.

### Attention and incomplete states

When measurement completes but trusted managed files are changed or missing, a
verified recovery final is present, or a finite lifecycle/source observation
remains, the semantic result is `attention` and human output uses `requires
attention`. An incomplete draft, unavailable recovery fact, or safely reportable
malformed or unsupported final selects `incomplete`; an unsafe or ambiguous
recovery boundary selects `blocked`. The command does not replace an unavailable
fact with a partial or trusted count.

A safely observed missing or changed registered Library projection is also
`attention` when its bounded record, source-root, and link observations are
complete. Unavailable Library coverage is `incomplete`; malformed, aliased,
colliding, or unsafe Library identity is `blocked`. Status does not enumerate a
complete source tree or infer unregistered mappings.

### Invalid and blocked states

Unexpected operands or operation-specific flags produce `invalid`. A missing,
unavailable, or non-directory selected workspace, or an unsafe inspection
boundary, produces `blocked` under the conditions above. The error names the
operation, affected subject or fact, cause, and useful next action when one
exists.

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
  cases, including every ordered contribution in JSON and omission in compact
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
- Complete, attention, incomplete, invalid, blocked, failed, and interrupted
  outcomes.
- Human and structured output from the same typed result.
- Human result and error stream assignment, one complete JSON result for every
  status, bounded diagnostics on stderr, and compact one-line `Next:` behavior.
- Repeat invocations producing the same semantic result for unchanged CLI and
  workspace bytes.
- Evidence that status does not parse ordinary links, build the complete content
  graph, inspect unrelated workspace files, or mutate anything.
- Bounded Workspace Library record, source-root, and registered-link facts,
  including destination-derived source IDs kept separate from Library IDs,
  complete safely observed drift as `attention`, unavailable coverage as
  `incomplete`, unsafe ambiguity or Library/Extension collision as `blocked`,
  and no complete source inventory or adoption.

Direct tests should prove measurement, comparison, ordering, classifications, and
semantic results. Focused integration tests should use real temporary workspaces,
embedded assets, the exact lifecycle document, and filesystem state. A small built Native AOT process suite
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

## Compact JSON Output

Normal `--json` uses expanded output and the full schema-v1 document. Explicit
`--json --view=compact` uses the [shared compact envelope](../shared/result-coordinates/interface.md#compact-json-envelope):
`schemaVersion: 2`, `view: "compact"`, then `command`, `status`, `workspace`,
`result` and `next`.
It is minified through the serializer. The command/status/workspace/next values
and process exit remain unchanged; expanded remains the default.

The result retains every field except context.continuitySources. Context
measurements, coverage states, findings, lifecycle targets, Library records and
recovery paths remain complete. Omitting the source breakdown does not alter
its measured totals or imply that no continuity sources exist.

Compact omissions are defined field membership, distinct from unavailable data,
null values, empty collections or incomplete inspection. No collection is
truncated and no finding is filtered. Counts describe the original operation.
Select expanded on the original invocation when supporting evidence is needed.
The complete structured schema and examples elsewhere in this contract describe
expanded output unless explicitly labelled compact.
