---
open-forge:
  description: Current technology-neutral Status operation, closed inspection accounting, result formation, and conformance
  responsibility: Define how a conforming implementation resolves, measures, compares, and reports Status without selecting implementation technology
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Status, Behavior, Determinism, Measurement, Safety, CurrentTruth]
---

# Status Behavior Contract

## Status And Boundary

This is the current Crystallized Behavior Contract for the accepted `status`
command. This file is authoritative for deterministic, technology-neutral
request resolution, inspection accounting, result formation, read-only safety,
and conformance. The command does not ship yet. The sibling Interface Contract
defines the complete public surface behind this behavior.

The [Status Interface Contract](interface.md) defines the complete public syntax,
observable facts, output, semantic result names, errors, non-goals, examples, and
public verification. This file defines only the deterministic operation behind
that surface. It does not add operands, flags, aliases, output shapes, semantic
results, or technology requirements. The [Shared Result
Coordinates](../shared/result-coordinates/interface.md) define the exact shared
JSON result schema and exit mapping, while the accepted [CLI
Architecture](../../architecture.md) defines the implementation boundary. This
behavior does not duplicate those mechanics or claim their Gate 5 proof.

The [Framework loading contract](../../../framework/routing/loading.md),
[routing model](../../../framework/routing/model.md),
[scope rules](../../../framework/routing/scope.md), and
[overwrite rules](../../../framework/routing/overwrites.md)
remain authoritative for Framework meaning. The [Context Behavior Contract](../context/behavior.md)
and shared CLI contracts remain authoritative for their own detailed command
meaning. Links here preserve those boundaries; they do not merge authority.

## Operation Boundary And Invariants

`status` is one complete, read-only orientation operation. It establishes one
exact workspace boundary, inspects only the closed facts needed by the
[Interface purpose](interface.md#purpose) and [inspection boundary](interface.md#inspection-boundary),
forms one typed result, and stops. It does not turn a flag into another operation
or acquire mutation authority while collecting or presenting the result.

For unchanged CLI payload, workspace bytes, and explicit input, the operation is
deterministic. It resolves the same request, inspects the same admissible facts,
produces the same measurements, comparisons, ordering, observations, and semantic
result, and gives both presentations the same typed result.

The operation is stateless. `Initial` comes from the embedded Framework asset set
for that invocation; it is not a saved snapshot or a stored historical baseline.
The operation creates no session, persistent graph, historical baseline, or
mutation plan. The CLI distribution embeds Framework and first-party Extension
assets with deterministic inventory and hash proof. That proof identifies
distributed source assets; it is not evidence of a selected workspace's current
installation or of a proven runtime implementation.

The operation remains an orientation summary. It does not perform the complete
diagnosis, recommendation, complete context-graph construction, route or link
validation, or mutation excluded by the [Interface non-goals](interface.md#non-goals).

## Request Resolution

Request resolution validates the command path, the absence of operands and
operation-specific flags, and the shared global inputs according to the
[Interface syntax](interface.md#syntax) and [Global CLI Flags](../shared/global-flags/behavior.md)
contract. It preserves the shared contract's defaults, repetition, composition,
terminal modes, and errors instead of defining another global-flag grammar.

`--help` and `--version` remain terminal informational modes under the shared
contract. They stop before the Status domain operation resolves a workspace or
performs inspection. Other well-formed shared flags retain their shared
applicability and no-op behavior where that contract says they do.

Unexpected operands or operation-specific flags form the public `invalid` result.
A missing value for `--workspace` is invalid under the shared contract. The
resolver does not reinterpret a path-like value as another operation or infer a
workspace from route, file, Git, `AGENTS.md`, or `.agents` structure.

## Workspace Resolution

The resolver selects exactly the process current working directory when
`--workspace` is omitted, or exactly the supplied `--workspace <path>` value when
it is present. Relative workspace values follow the shared global contract. The
selected identity is normalized for reporting, while the request retains the
selection method needed by the public result.

Before claiming any inspection fact, the resolver establishes that the selected
target is available, is a directory, and provides a safe lexical and physical
containment boundary for the operation. It does not search upward, substitute a
Git root, or choose a nearby workspace. A missing, unavailable, or non-directory
target forms the public `blocked` result described by [Interface Errors](interface.md#errors).

The resolver does not require an installed Framework to recognize a directory as
a valid Status subject. It records an uninstalled state. If that absence can be
established safely, that uninstalled state may be `complete`: the embedded payload's
Initial measurement remains measured when available, current startup, Difference,
startup percentage, continuity, and root-category facts are not-applicable, and
the total physical context inventory remains numeric when safely measurable. An
applicable fact that cannot be measured is unavailable and makes the result
incomplete. Lifecycle and recovery-bundle facts retain their own accounting
rules.

## Inspection Boundary And Completeness

The operation creates a fresh per-invocation inspection ledger whose admissible
universe is exactly the one in [Interface Inspection Boundary](interface.md#inspection-boundary)
and [Context Inventory](interface.md#context-inventory): the canonical workspace
entry and supported `.agents` Markdown, the embedded Framework payload, the
route and loading facts needed for startup and continuity, direct Loader root
categories, the exact `.agents/open-forge.lifecycle.json` document, schema v1,
and the current user's external recovery store containing exact-name final and
draft candidates for the selected normalized physical workspace path.

Enumeration is closed. The ledger records each canonical workspace-relative path,
its physical layer and logical-source relationship where applicable, its readable
content facts, and its inspection disposition. Files outside that declared
universe cannot enter the result merely because a local link names them, and
eligible files cannot be silently removed because they are not exposed by stale
generated navigation.

The operation may read supported context bytes for measurement. It does not parse
ordinary links or named sections, build the complete context graph, validate every
route, inspect unrelated workspace files, or construct a mutation plan. An
incomplete route-dependent closure therefore cannot be hidden by a broader scan
or by a complete-graph fallback.

Readable UTF-8 bytes remain measurable even when Markdown structure is malformed.
Non-UTF-8 Markdown is retained as an incomplete inspection disposition rather
than converted or ignored. An orphan overwrite is not admitted as independently
available context; its presence prevents a complete inventory. Any unsafe
containment or physical-identity condition blocks the affected inspection rather
than allowing an out-of-bound fact into the result.

Safe facts may remain available beside an incomplete boundary. The result cannot
claim complete coverage while an unresolved applicable fact could change a
measurement, comparison, ordering, status, or required summary field.

## Unified Lifecycle Fact Resolution

The lifecycle stage reads `.agents/open-forge.lifecycle.json`, schema v1, as a
common envelope with isolated `framework` and `extensions` sections. It never
combines their authority or uses one section to reconstruct the other. It
preserves independently readable facts from an unaffected section when the other
section is malformed, while retaining the malformed section's coverage state.
The document stores no plan, runtime history, journal, recovery-bundle evidence,
or session. Files outside this exact path are ordinary workspace content, not
lifecycle input.

The stage classifies each section as:

- `absent` only after complete inspection proves that no expected managed state,
  managed boundary, or recovery residual exists;
- `trusted` only when schema v1 and `open-forge-markdown-v1`, exact workspace
  and managed identities, internal consistency, reciprocal package/path/owner/
  dependency facts, and complete verifiable coverage hold;
- `untrusted` when safe facts exist but provenance, integrity, compatibility,
  identity, or coverage cannot establish current trust;
- `incomplete` when safe required lifecycle or source coverage is unavailable;
  or
- `blocked` when malformed, ambiguous, colliding, or unsafe identity prevents a
  safe classification.

The stage never treats a missing expected section as empty and never promotes a
state because a path, byte sequence, fingerprint, source, or force flag matches.
An absent document or section is not, by itself, proof of unmanaged state. An
unavailable Extension package source leaves readable installed ID, ownership,
and baseline observations visible, but source-dependent comparison remains
unavailable and cannot become update or no-op authority. Unsupported or
ambiguous schema facts are `incomplete` when safely unavailable and `blocked`
when unsafe.

## Context Inventory Accounting

### Context File Enumeration

The enumerator counts each canonical path once in the physical inventory. It
counts a base and valid adjacent overwrite companion as two contributing files,
does not count generated `Entries` as a separate file, and excludes operational
metadata, support resources, and ordinary local link targets outside `.agents` as
specified by [Interface Context File](interface.md#context-file). It retains
base and overwrite layers in their required order for measurements and logical
source calculations.

The same ledger distinguishes a measurable malformed Markdown file from a
non-UTF-8 file and from an orphan overwrite. It never changes an unavailable
fact into an available file or a partial count.

### Startup And Continuity Closure

For the selected workspace, the resolver measures the same startup-required
closure that `open-forge context` returns without an explicit source. It follows
the current target-sensitive Framework loading model rather than the frozen MVP's
broad traversal. The closure is resolved in this order:

1. Establish the canonical workspace entry and Loader.
2. Follow visible #LoadNow entries in generated order.
3. Resolve every applicable #KeepInMind entrypoint and routed #KeepInMind file,
   including parent entrypoints not already selected when needed to establish the
   route.
4. Follow the visible #LoadNow closure exposed by those continuity sources.
5. Place each valid overwrite companion immediately after its base.

The [Framework loading contract](../../../framework/routing/loading.md)
defines the loading and continuity relationships, and the [Context Behavior Contract](../context/behavior.md)
defines the shared startup closure. Status consumes those relationships for
measurement; it does not render context content or import Context's projection,
operand, or link-expansion surface.

Continuity accounting selects the subset of current startup context that may load
again at a defined #KeepInMind boundary. Required parents and #LoadNow files are
included in that subset. The subset is measured separately and is never added to
the startup total. A continuity source is not treated as loading on every model
request, prompt, message, or tool call.

### Initial And Current Comparison

The operation resolves the `Initial (shipped)` view from the embedded Framework
payload and the `Current workspace` view from the selected workspace using the
same measurement definitions. It forms `Difference` as current workspace minus
Initial for each metric and retains the signed value.

No previous invocation participates in the comparison. Updating the CLI may
change the embedded Initial payload. Added, removed, larger, smaller, or changed
context remains a neutral customization fact and does not become an attention
condition merely because the arithmetic differs.

For each metric, numeric-numeric operands produce the signed numeric
current-minus-initial Difference, including zero. If a required operand is
unavailable, the derived Difference is unavailable. If the comparison does not
apply, the derived value is not-applicable.

### Total And Startup Percentage

Total available context is computed from the unique current context-file
inventory, including startup and on-demand files. The operation measures its
files, Unicode scalar-value characters, UTF-8 bytes, and deterministic estimate
using the definitions in [Interface Total Available Context](interface.md#total-available-context)
and [Interface Measurements](interface.md#measurements).

The startup percentage uses current startup UTF-8 bytes divided by total available
UTF-8 bytes. It is numeric when both byte operands are numeric and total bytes are
greater than zero. Both zero byte values produce not-applicable. A positive
current value with a zero total is an incomplete accounting invariant, so the
percentage is unavailable and the result is incomplete. An unavailable required
operand makes the percentage unavailable. The operation never renders unavailable
or not-applicable as zero and does not claim relevance or future loading for every
available file. Exact JSON representation follows the [Shared Result
Coordinates](../shared/result-coordinates/interface.md).

### Largest Continuity Source Ordering

The ranker groups a base and valid overwrite companion as one logical continuity
source while retaining the ordered physical layers. Its contribution is the
combined UTF-8 size of those layers, and the source appears once under the
automatic source ID defined by [CLI Source References](../shared/source-references/behavior.md).
It orders contributions by descending UTF-8 size and then canonical source ID.

Expanded human presentation consumes the public at-most-three result and shows
fewer when fewer exist. Compact human presentation omits the largest-source
section. Structured output retains every ordered continuity-source contribution;
it does not inherit the expanded human limit.

## Measurement Availability And Arithmetic

The measurement stage reads complete authored file text, including frontmatter,
generated regions, and overwrite content. It excludes renderer-added CLI identity
blocks, headings, labels, separators, and other presentation framing. It counts
Unicode scalar values and exact UTF-8 bytes, displays byte sizes with IEC units,
and applies the accepted planning estimate `ceiling(characters / 4)`. Human
rendering may prefix and round the estimate for display; structured rendering
retains the unrounded estimate and estimator identity, as specified by
[Interface Measurements](interface.md#measurements).

If exact characters or UTF-8 bytes cannot be measured safely, the stage records
the affected value as unavailable and marks the applicable result incomplete
instead of substituting a partial count. Zero, unavailable, and not-applicable
values remain distinct facts and are never rendered as one another. When the
inputs needed for a signed Difference are numeric, the stage derives the signed
value including zero; an unavailable required operand makes the derived value
unavailable, and a comparison that does not apply makes it not-applicable. For
startup percentage, both zero byte operands make the value not-applicable, a
positive current value with zero total records an incomplete accounting invariant
and an unavailable percentage, and any unavailable required operand makes the
percentage unavailable. The stage does not perform partial or fabricated
arithmetic.

The formula is the accepted observable planning estimate. Its implementation,
rounding details beyond the stated display rule, and any provider, library, or
.NET boundary follow the accepted CLI Architecture and do not become a separate
technology requirement of this Behavior Contract.

## Workspace Structure Accounting

### Root Category Comparison

The operation obtains the current Loader's direct root categories and compares
them with the direct root categories in the embedded Loader. It retains exact
root route identity and the source order of each side. Current-only categories
form `Added categories` in current order; embedded-only categories form `Removed
categories` in embedded order; the current count forms `Root categories`.

The comparison does not count scopes. It treats additions and removals as neutral
customization facts and does not infer health, repair need, or a lifecycle action
from them. The [Framework routing model](../../../framework/routing/model.md)
and [scope rules](../../../framework/routing/scope.md)
remain the source of root and scope meaning.

### Extension And Managed-File Accounting

The lifecycle stage distinguishes absent, trusted, untrusted, incomplete, and
blocked sections. An absent Extension section contributes `0 recorded`
Extensions and `none recorded` managed files, has no status effect alone, and
makes no claim about unmanaged Extension-like files. A trusted empty section
contributes `0` Extensions and `none recorded` managed files. Untrusted,
incomplete, or blocked evidence withholds facts it cannot establish rather than
producing a trusted empty count.

For trusted facts, deduplicate distinct installed Extension IDs and assign each
recorded managed path to one state from fresh current evidence: `current`,
`changed`, or `missing`. The `open-forge-markdown-v1` policy supplies semantic
fingerprints for supported parseable kinds. Shared owner sets do not multiply the path count,
and unmanaged files do not enter the result. These are lifecycle observations,
not runtime, replacement, removal, repair, or ownership authority.

Expanded human and structured results retain lifecycle trust and coverage state.
Compact human output may combine that state with Extension and managed-file
summaries. If package source bytes are unavailable, installed facts remain
reportable and source-dependent comparisons are unavailable or incomplete.

### Recovery-Bundle Accounting

The recovery stage resolves only the current user's
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.None)/OpenForge/recovery/v1` store. This
observer-only lookup never creates the OS application-data root or the Open Forge
subtree. It uses the deterministic key for the selected normalized physical
workspace path and reports every exact-name final and draft candidate. It never
falls back to the workspace, a repository, a temporary directory, or a target's
adjacent files. Unknown, lookalike, mismatched, and differently keyed items do
not enter the candidate catalogue. A malformed exact-name final remains a
reported candidate.

If the application-data root or recovery store is absent, the stage reports zero
verified finals and zero incomplete drafts. If an existing root or selected
workspace bucket cannot be read, it records the recovery fact as unavailable and
forms the locally contracted `incomplete` result. It never treats access failure
as absence.

The stage enumerates only exact deterministic final and draft names directly
under the selected workspace bucket. A final ZIP is `Verified` only after
semantic source-generated manifest/schema validation and exact ordered entry
names and counts, declared lengths and hashes, and exact payload-byte validation.
An exact named final that cannot satisfy that check remains a reported
`Malformed`, `Unsupported`, or `Unavailable` issue and never enters the
verified-final count. An exact named draft is reported as `Incomplete`, enters
only the incomplete-draft count, and never forms preparation. Every candidate
item retains its path, kind, and integrity condition. Unknown names, locations
elsewhere, and target proximity do not establish a result item.

The stage does not acquire `WorkspaceLockLease`, inspect live targets, classify
current target state, or infer activity. Payload validation uses fixed bounded
buffers and never extracts, discloses, renders, logs, returns, retains, or
materializes payload bytes. A bundle keyed to an original workspace path after a
workspace move is not auto-bound to the newly selected path.

The stage does not remove, clean, restore, roll back, or otherwise mutate a
bundle or target. The separate [cleanup contract](../cleanup/interface.md) owns
lease-validated candidate deletion. Status's boundary remains
defined by [Interface Recovery Bundles](interface.md#recovery-bundles) and
[Interface Architecture Boundary](interface.md#architecture-boundary).

## Result Formation And Presentation

### Typed Result

After request resolution and safe inspection, the operation forms one typed result
containing the workspace and selection method, Framework installation state,
Initial/current/Difference measurements, total and continuity measurements,
ordered continuity-source contributions, root-category facts, Framework and
Extension lifecycle trust and managed-file states, source-availability
observations, separate verified-final and incomplete-draft counts, every
exact-name recovery candidate's path, kind, and integrity condition, measurement
availability, observations or attention conditions, and the public semantic
result. The field meanings are
owned by [Interface Structured Output](interface.md#structured-output);
this stage does not create a second schema.

The typed result is formed once. Human and structured renderers consume it without
rerunning workspace resolution, closure collection, enumeration, measurement,
comparison, ordering, or status formation.

The operation obtains those facts through the six explicitly composed typed
operational contributors for workspace and entry, recovery and residuals,
routes and generated navigation, local references, Framework lifecycle, and
Extension lifecycle. Status invokes the first, second, third, fifth, and sixth
contributors once for its narrow Status view. It never invokes the local-
reference contributor. The catalogue retains that local-reference domain for Doctor,
which consumes the separate narrow Doctor views rather than Status output or
command-private implementation.

Direct root composition preserves the exact leaf order `find`, `index`,
`status`, `context`, `references`, `install`. The Status leaf occupies the
position between `index` and `context`; it is not appended after `install`.

Each contributor view is a fresh immutable projection of observations made for
the selected workspace in the current invocation. A view carries producer facts,
not command status, recommendations, rendering, or mutation authority. Status
owns the joining, derived arithmetic, deterministic ordering, finding mapping,
overall result, and presentation. Neither command discovers contributors at
runtime or receives an enumerable registry, service collection, ambient
registration, broad context bag, or another command's result.

Result formation projects the exact command-local graph and member order in
[Interface Structured Output](interface.md#structured-output). It materializes
every collection, including empty collections, before forming the concrete
result. It preserves nullable installed package facts when source bytes are
unavailable, counts a shared Extension target once while retaining its complete
ordered owner set, and projects recovery candidates to path, kind, and integrity
only. It never exposes a verified bundle's command, operation identity, entry
manifest, payload facts, target state, activity, lock, fallback, preparation, or
payload bytes.

Each numeric result first receives an explicit `available`, `unavailable`, or
`not-applicable` state. Only an available value carries a number. Difference is
then derived independently for each metric from available current and Initial
operands. Startup percentage is formed only under the Interface arithmetic
rules and must be a finite decimal value. Count summaries are unavailable when
the contributing boundary cannot establish completeness; an empty observation
array never turns that state into an available zero.

Generated-navigation observations use only `current`, `changed`, `missing`,
`unavailable`, `blocked`, and `not-applicable`. Framework and Extension managed
targets use only `current`, `changed`, `missing`, `unavailable`, and `blocked`.
The result preserves every per-target observation and does not replace it with
one aggregate currentness fact. Finding formation uses only the exact finite
codes and code-to-status mapping in the Interface. The overall selector then
applies the public semantic precedence without reclassifying a finding from its
message text.

### Semantic Result Formation

The result selector uses only the seven public semantic states in
[Interface Semantic Results](interface.md#semantic-results):

- Invalid request input selects `invalid`.
- Failure to establish the selected workspace or a safe inspection boundary
  selects `blocked`.
- Safe facts with one or more unavailable or incomplete applicable measurements,
  including an exact-name draft or safely bounded malformed or unsupported final,
  select `incomplete` rather than claiming complete coverage.
- An unsafe or ambiguous recovery inspection boundary selects `blocked` under
  the existing inspection-boundary precedence.
- An unexpected internal failure selects `failed`.
- Cancellation or interruption before completion selects `interrupted`.
- Complete measurement with changed or missing trusted managed files, a verified
  recovery final, or a finite lifecycle/source observation selects `attention`.
- Complete applicable measurement without an attention condition selects
  `complete`, including a safely established uninstalled workspace.

Context-size differences and root-category additions or removals do not select
`attention` by themselves. An absent or trusted-empty lifecycle section does not
change status alone. Untrusted or unavailable lifecycle coverage selects
`incomplete` when safe and `blocked` when unsafe. Source-unavailable installed
facts remain visible and do not become trusted current source. Numeric exits and
the exact JSON schema follow the [Shared Result
Coordinates](../shared/result-coordinates/interface.md).

### Human Rendering

The human renderer uses the stable sections, exact labels, zero visibility, list
`none` rule, and compact-versus-expanded relationship in [Interface Human Output](interface.md#human-output).
It renders the typed `attention` value as `requires attention` and does not
reinterpret any measurement or ranking.

Compact rendering retains the public status, workspace identity, startup and
continuity totals, root changes, Extension and managed summaries, separate
verified-final and incomplete-draft counts, recovery issue visibility, and
required next-action information. It emits at most one operation-level
`Next:` line, only under the rules in [Interface Human Output](interface.md#human-output):
complete has none; attention uses `open-forge doctor`; incomplete uses Doctor
unless a more direct safe correction is known; invalid uses `Next: correct the
named input`; blocked uses `Next: correct the named workspace or safety boundary
and rerun`; failed uses `Next: report the failure and retry with bounded
diagnostics`; and interrupted uses `Next: rerun the same request`. It never lists
repair or lifecycle proposals.

Primary human rendering for `complete`, `attention`, and `incomplete` goes to
stdout. Primary human rendering for `invalid`, `blocked`, `failed`, and
`interrupted` goes to stderr. `--json` writes one complete structured result to
stdout for every semantic status, and separate bounded diagnostics go to stderr.
Human text is not mixed into JSON stdout.

### Structured Rendering

The structured renderer serializes the same typed result used by human rendering.
It preserves numeric values as numeric, signed Difference as derived from its two
measured inputs, and the distinction among unavailable, zero, and not-applicable
values without substituting one for another. It retains every continuity-source
contribution in the deterministic order defined by Interface and retains
Framework/Extension lifecycle trust, ownership, and source-availability state.
Exact JSON representations and schema details follow the [Shared Result
Coordinates](../shared/result-coordinates/interface.md).

`--json` does not rerun collection or allow `--view` to change the structured
result. `--verbose` may add bounded diagnostics without changing collection,
measurements, ordering, semantic result, or exit behavior. Those diagnostics use
stderr. Exact diagnostic fields and redaction follow the accepted CLI
Architecture.

### Error Formation

Error formation preserves the public error boundary in [Interface Errors](interface.md#errors).
Every error identifies the Status operation, affected workspace or fact, direct
cause, and useful next action when one exists. A malformed startup route or
unreadable required context becomes an incomplete result when safe facts remain;
a malformed or unavailable lifecycle section becomes an incomplete or blocked
managed-state summary rather than a guessed count. The behavior never uses a
guessed value to avoid reporting an error or incomplete boundary.

## Read-Only Effects And Safety

Status performs no persistent mutation. It does not write workspace files,
lifecycle sections, generated navigation, snapshots, recovery bundles or drafts,
or persistent diagnostics;
it does not remove, extract, disclose, retain, or materialize private recovery
bytes; and it does not create an empty mutation plan. Bounded payload streaming
serves only strict length/hash validation. Read, comparison, ranking, and
rendering stages cannot acquire write authority.

The operation derives all facts per invocation from the selected workspace,
embedded payload, and explicit input. Repeating it with unchanged inputs produces
the same semantic result and does not create a synthetic effect. It does not use
an alternate workspace, broaden its inspection universe, build persistent graph
state, or fall back from an unsafe boundary to an unbounded scan.

## Behavioral Conformance

The mandatory public evidence list in [Interface Public Verification](interface.md#public-verification)
is the complete coverage boundary. A conforming implementation must preserve
that list's installed, uninstalled, incomplete, unsafe, closure, inventory,
measurement, comparison, ordering, lifecycle trust, source-availability,
managed-file, recovery, semantic result, presentation, repeatability, and
non-mutation evidence without changing
the source's modality.

Direct tests should prove measurement, comparison, ordering, classifications, and
semantic results. Focused integration tests should use real temporary workspaces,
embedded or supplied assets, the exact lifecycle document, and filesystem state.
A small built Native AOT process suite should prove parsing, output, exit
behavior, and packaged payload comparison. The [Shared Result
Coordinates](../shared/result-coordinates/interface.md) define the exact result
schema and exits. Exact recovery identity mechanics live in the [Mutation And
Recovery Technical Design](../../technical-designs/mutation-and-recovery.md).
The [CLI Architecture](../../architecture.md) defines .NET boundaries and keeps
exact Status diagnostic fields and redaction as bounded command-local
implementation details. Gate 5 must prove those details, source-generated
YamlDotNet and STJ serialization, fixed Markdig where used, real `System.IO`,
Native AOT, OS locking, isolated tests, and package journeys.

Behavioral conformance must also show that human and structured renderers consume
one typed result, that Status does not parse ordinary links or named sections,
build the complete context graph, inspect unrelated workspace files, or mutate
anything, and that unsafe or incomplete facts are never silently discarded. It
must prove bounded empty, binary, and large payload validation without extraction
or materialization, and that Status neither inspects nor infers recovery activity.

## Related Current Sources

- [Status Interface Contract](interface.md)
- [Status Command Contract Set](_status.md)
- [CLI Command Contract Set — Behavior Contract](../../command-contract-set.md#behavior-contract)
- [Shared CLI Operation Contract](../../shared-operation-contract.md)
- [Context Behavior Contract](../context/behavior.md)
- [Global CLI Flags Behavior Contract](../shared/global-flags/behavior.md)
- [CLI Source References Behavior Contract](../shared/source-references/behavior.md)
- [Framework loading contract](../../../framework/routing/loading.md)
- [Routing model](../../../framework/routing/model.md)
- [Route scope and inheritance](../../../framework/routing/scope.md)
- [Overwrite customization](../../../framework/routing/overwrites.md)
