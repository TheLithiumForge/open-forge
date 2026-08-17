---
open-forge:
  description: Current accepted interface for workspace status, context-size comparison, root customization, managed Extensions, and recovery evidence
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

The accepted CLI Architecture defines the exact shared JSON result schema and
numeric exit mapping. Status uses those definitions without duplicating
implementation mechanics. Gate 5 must prove source-generated YamlDotNet and STJ
serialization, fixed Markdig where used, real `System.IO`, Native AOT, OS
locking, isolated tests, and package journeys. Token estimation, recovery-file
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
- Are recognized recovery files present?

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
- Recognized Open Forge CLI recovery files adjacent to known Open Forge targets.

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
recovery evidence, or session. Files outside this exact path are ordinary
workspace content, not lifecycle input.

Each section retains its finite state:

| State        | Status meaning                                                                                                                                                          |
| ------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `absent`     | Complete inspection proves that no expected managed state, managed boundary, or recovery residual exists. It does not claim that unmanaged or idless content is absent. |
| `trusted`    | Supported document and section facts bind the exact workspace and managed identities, preserve internal consistency, and provide complete verifiable coverage.          |
| `untrusted`  | Some lifecycle facts are readable, but provenance, integrity, compatibility, identity, or coverage cannot establish current trust.                                      |
| `incomplete` | Safe required lifecycle or source coverage is unavailable.                                                                                                              |
| `blocked`    | Malformed, ambiguous, colliding, or unsafe lifecycle identity prevents a safe classification.                                                                           |

The absence of `.agents/open-forge.lifecycle.json` alone does not establish an
unmanaged or empty workspace. `absent` requires complete inspection of the
expected managed footprint, managed boundaries, and recognized recovery
residuals. Unsupported or ambiguous schema facts are `incomplete` when safely
unavailable and `blocked` when unsafe. The visible `.agents/open-forge.lock` file
is not lifecycle authority or recovery evidence and does not affect these states.

An invalid or unavailable package source does not erase independently readable
installed Extension IDs, ownership, recorded paths, or lifecycle facts. Status
marks source-dependent comparison as unavailable or incomplete instead of
claiming a current source, update plan, or managed no-op. A path, matching bytes,
matching fingerprint, or familiar route never promotes an untrusted state.

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
- Do not count provider bridges, the lifecycle document, recovery files, or other
  operational metadata as context.
- Do not count non-Markdown Skill resources or other support files as context.

Malformed Markdown remains measurable when its UTF-8 bytes are readable, even
when route-dependent startup resolution is incomplete. A non-UTF-8 Markdown file
makes the affected inventory incomplete rather than being converted or ignored.

Structured results may use `sources` and ordered source layers where that
precision is needed. Human output keeps `Files` because the displayed counts are
physical files.

### Startup Context

Startup context is the complete context required when work starts or resumes:

1. The canonical workspace entry and Loader.
2. The visible #LoadNow closure in generated order.
3. Every applicable #KeepInMind entrypoint and routed #KeepInMind file, including
   parent entrypoints not already selected when needed to establish its route.
4. The visible #LoadNow closure exposed by those continuity sources.
5. Each valid overwrite companion immediately after its base.

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
exact JSON representation of these values follows the shared CLI Architecture.

### Continuity Context

Continuity context is the subset of current startup context that may load again
at a #KeepInMind boundary, including the parent and #LoadNow files required to
load that complete continuity set.

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

### Recovery Files

`Recovery files` counts only artifacts the running CLI can identify as known Open
Forge recovery evidence around exact Open Forge targets. Unknown adjacent files
are not counted. The count does not claim that recovery is required, complete, or
guaranteed to succeed.

Status does not remove or inspect private recovery bytes. The accepted
[cleanup contract](../cleanup/interface.md) owns recognized-artifact deletion;
the accepted CLI Architecture defines exact artifact identity and implementation
mechanics. Status does not treat the visible workspace lock as recovery evidence.

## Human Output

The default expanded result uses the stable sections and labels below and adds
ordinary explanation and provenance. Compact view keeps the semantic result,
workspace identity, startup and continuity totals, root changes, Extension and
managed-state summaries, recovery counts, and required next actions while
omitting optional explanation and largest-source detail. Both remain
understandable without color. Zero values remain visible wherever omission would
make absence ambiguous.

Primary human rendering for `complete`, `attention`, and `incomplete` goes to
stdout. Primary human rendering for `invalid`, `blocked`, `failed`, and
`interrupted` goes to stderr. `--json` writes one complete structured result to
stdout for every semantic result. Separate bounded diagnostics go to stderr, and
human text is not mixed into JSON stdout.

Expanded human output retains lifecycle trust and coverage state. Compact output
may combine that state with its Extension and managed-file summaries. Zero values
remain visible. An absent section uses `0 recorded` Extensions, while a trusted
empty section uses `0`; both use `none recorded` managed files.

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

Illustrative output:

```text
Open Forge status
Workspace: D:/Repositories/open-forge
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
  2.4 KiB   memory/crystallized/documents/cli
  1.9 KiB   directives/writing
  1.1 KiB   memory/crystallized/documents

Workspace structure
  Root categories:    8
  Added categories:   workspace
  Removed categories: templates
  Extensions:         1
  Managed files:      9 current, 11 changed, 1 missing
  Recovery files:     0
```

The values are illustrative. They do not claim to be a current measurement of
this repository.

When a list is empty, human output uses `none`, except that absent lifecycle
sections use `none recorded` for managed files. An uninstalled workspace states
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
- Recovery-file count
- Measurement availability and semantic result

Numeric fields remain numeric. Signed differences remain derived from the two
measured inputs. Unavailable, zero, and not-applicable values remain distinct and
are never substituted for one another. The shared CLI Architecture defines the
exact JSON representation, result schema, compatibility rules, and exit mapping.

For an attention result, structured output keeps the semantic status value
`attention`; only human presentation uses `requires attention`.

`--verbose` may add bounded diagnostic evidence under the shared [Global CLI
Flags](../shared/global-flags/interface.md) contract. Those diagnostics use
stderr. It does not change collection, measurements, ordering, semantic result,
or exit behavior, and it does not add Doctor diagnosis or recommendations. The
accepted CLI Architecture defines diagnostic fields and redaction.

## Architecture Boundary

These implementation details are defined by the accepted CLI Architecture and are
not duplicated by this contract:

- **Structured schema and compatibility.** JSON exposes the same typed facts as
  human output and keeps numeric, zero, unavailable, and not-applicable
  distinctions. Exact field names, schema version, compatibility rules, JSON
  representations, and serialization details follow the shared Architecture.
- **Numeric process exits.** Semantic result categories are accepted and
  structured `attention` remains `attention`; human output says `requires
attention`. Numeric process-exit mapping follows the shared Architecture.
- **Token-estimation implementation.** The planning estimate is
  `ceiling(characters / 4)` and is explicitly not a model tokenizer, billing
  value, context guarantee, latency estimate, or provider count. The
  implementation and rounding details beyond the stated display rule follow the
  accepted Architecture.
- **Recovery artifact identity and cleanup boundary.** Status counts only
  recovery evidence the running CLI can identify around exact Open Forge targets
  and does not remove or inspect private bytes. The separate [cleanup
  contract](../cleanup/interface.md) owns recognized-artifact deletion. Exact
  artifact names, storage, schema, and implementation follow the accepted
  Architecture.
- **Diagnostics and redaction.** `--verbose` may add bounded diagnostic evidence
  without changing collection, ordering, semantic result, or exit behavior. Exact
  Status diagnostic fields and redaction follow the accepted Architecture. Their bounded
  diagnostic stream is stderr as stated above.
- **.NET source boundaries.** .NET Native AOT is the accepted canonical
  implementation direction for the CLI. Status has no command-local Technical
  Design; its concrete module, parser, serializer, filesystem, and source
  boundaries follow the accepted Architecture.

## Semantic Results

| Result        | Meaning                                                                                                                                                          | Process completion status       |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------- |
| `complete`    | Every applicable status fact was measured and no attention condition exists                                                                                      | Shared CLI Architecture mapping |
| `attention`   | Measurement completed, but trusted managed files are changed or missing, recognized recovery files are present, or a finite lifecycle/source observation remains | Shared CLI Architecture mapping |
| `incomplete`  | Safe facts are available, but one or more applicable status measurements could not be completed                                                                  | Shared CLI Architecture mapping |
| `invalid`     | Command input does not follow the accepted grammar                                                                                                               | Shared CLI Architecture mapping |
| `blocked`     | The command cannot establish the selected workspace or a safe inspection boundary                                                                                | Shared CLI Architecture mapping |
| `failed`      | An unexpected internal failure prevents normal completion                                                                                                        | Shared CLI Architecture mapping |
| `interrupted` | The caller cancels or interrupts the operation before completion                                                                                                 | Shared CLI Architecture mapping |

An uninstalled workspace is a valid completed state when its absence can be
established safely. Differences in context size and added or removed root
categories do not produce `attention` by themselves.

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
command contract. The shared CLI Architecture defines the exact schema, exits,
diagnostic fields, recovery identity, and .NET boundaries.

### Default current workspace

```text
open-forge status
```

The command uses the exact current working directory and the default expanded
human presentation. It returns the workspace, startup comparison, total and
continuity measurements, root changes, Framework and Extension lifecycle trust
and managed-file summary, recovery count, and the applicable semantic result.

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
exit follow the shared CLI Architecture. A well-formed `--view` is accepted as a
no-op under the shared Global CLI Flags contract.

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

When measurement completes but trusted managed files are changed or
missing, recognized recovery files are present, or a finite lifecycle/source
observation remains, the semantic result is `attention` and human output uses
`requires attention`. When safe facts remain but an applicable measurement or
lifecycle fact cannot be completed, the semantic result is `incomplete`; the
command does not replace the unavailable fact with a partial or trusted count.

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
- Zero, present, unknown, and unsafe recovery-file evidence.
- Complete, attention, incomplete, invalid, blocked, failed, and interrupted
  outcomes.
- Human and structured output from the same typed result.
- Human result and error stream assignment, one complete JSON result for every
  status, bounded diagnostics on stderr, and compact one-line `Next:` behavior.
- Repeat invocations producing the same semantic result for unchanged CLI and
  workspace bytes.
- Evidence that status does not parse ordinary links, build the complete content
  graph, inspect unrelated workspace files, or mutate anything.

Direct tests should prove measurement, comparison, ordering, classifications, and
semantic results. Focused integration tests should use real temporary workspaces,
embedded assets, the exact lifecycle document, and filesystem state. A small built Native AOT process suite
should prove parsing, output, exit behavior, and packaged payload comparison.

The shared CLI Architecture defines the exact numeric exit values, structured
schema and JSON value representation, diagnostic fields, redaction, recovery
artifact identities and retention, and .NET source boundaries. Gate 5 must prove
source-generated YamlDotNet and STJ serialization, fixed Markdig where used,
real `System.IO`, Native AOT, OS locking, isolated tests, and package journeys.

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
