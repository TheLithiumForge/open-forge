---
open-forge:
  description: Current Crystallized public interface for rebuilding bounded generated `Entries` from routed topology and authored metadata
  responsibility: Define the current public surface and observable result for the non-shipping `index` command
  tags: [Memory, Crystallized, CLI, Release, Command, Index, Interface, CurrentTruth]
---

# Index Interface Contract

## Status

This is the current Crystallized Interface Contract for the non-shipping
`index` command. The [Behavior Contract](behavior.md) defines the deterministic
operation behind this public surface. The [Technical Design](technical-design.md)
records the accepted realization under the contracts and the accepted
Architecture. This file defines the complete public syntax, inputs, observable
outputs, semantic results, errors, scenarios, and public verification for
`index`.

The public contract defines repetition of the Boolean write-policy flags, human
stream allocation, and structured presentation. Value-bearing repetition
remains governed by its defining contract.

The command does not ship, and this Interface Contract does not claim an
implementation.

## Purpose

`index` regenerates bounded generated `Entries` from authoritative filesystem
topology and authored routing metadata.

The command makes derived navigation match current routed sources without
changing authored meaning.

Given the same workspace bytes and explicit input, the command selects the same
regions, produces the same generated lines and ordering, and returns the same
semantic result. Repeating a successful application against unchanged input
returns a verified no-op.

## Related Contracts And Sources

The shared [Global CLI Flags](../shared/global-flags/interface.md) contract
defines the complete spelling and meaning of `--workspace`, `--json`, `--view`,
`--verbose`, `--help`, and `--version`. All six apply to `index` under that
contract.

The shared [CLI Source References](../shared/source-references/interface.md)
contract defines the complete source-ID and exact-path grammar, quoting,
collisions, disambiguation, and overwrite identity. Every source operand in
this command follows that contract.

The following sources define related accepted behavior:

- [Routed Markdown Representation](../../../framework/markdown/routes.md)
  defines generated `Entries` syntax and ownership.
- [Routing Model](../../../framework/routing/model.md)
  defines direct routed children.
- [Overwrite Contract](../../../framework/routing/overwrites.md)
  defines why overwrite companions are never indexed independently.
- [Open Forge CLI Architecture](../../architecture.md#result-json-coordinates-and-process-status)
  defines the shared exact structured-result schema and numeric process-exit
  mapping.
- [Shared CLI Operation Contract](../../shared-operation-contract.md) defines
  cross-command operation conventions.

## Shared Schema And Process Exits

The accepted [Open Forge CLI Architecture](../../architecture.md#result-json-coordinates-and-process-status)
defines the shared exact structured-result schema, schema version and compatibility
rules, and numeric process-exit mapping. `index` uses those shared definitions;
it does not add a command-specific schema or exit mapping. The [Technical
Design](technical-design.md#json-and-presentation) describes their accepted
serialization realization without changing their authority.

## Syntax

The complete accepted command form is:

```text
open-forge index [source-reference...]
  [--dry-run]
  [global flags]
```

`source-reference...` is an optional positional sequence. The one
command-specific flag is an optional Boolean write-policy flag. Repeating it is
accepted and idempotent. The shared global flags are
optional when their shared contract permits them.

The command has no `--all`, `--yes`, or `--force` flag, no directory operand, no
saved-plan mode, and no aliases.

## Workspace

`index` uses the exact current working directory, or the exact
`--workspace <path>` value when supplied.

The command never searches parent directories, substitutes a Git root, or
infers another workspace from a source operand.

A missing, unavailable, or non-directory workspace is blocked. Workspace
selection alone does not establish a valid Loader or routed topology.

## Operands And Flags

### Source operands

Zero source operands are valid. Omitting all source operands selects the
complete Loader-rooted topology described in [Rooted selection](#rooted-selection).

One or more source operands use the complete shared `source-reference` contract.
The command accepts source IDs and exact `.agents/...` paths through that
contract, including its quoting, exact-match, collision, path, and overwrite
rules.

Each source operand is a selection input, not a directory operand. A directory
path is invalid; use the entrypoint ID or exact entrypoint file path when
selecting an entrypoint.

Each source operand is resolved independently through the shared
source-reference contract. Several operands, duplicate operands, and
overlapping selections compose through the target-closure rules in [Several
selections](#several-selections).

### `--dry-run`

`--dry-run` is an optional Boolean write-policy flag with no value. Its omission
selects application. Its presence selects a preview that uses the complete
application plan and reports that no files changed.

Repeating `--dry-run` is accepted and idempotent. A second or later occurrence
has no additional effect. Repetition does not multiply application authority or
create another bypass. This matches repeated Boolean global flags.
Value-bearing repetition rules are unchanged under their defining contract.

`--dry-run` does not grant authority to repair markers, replace authored
content, modify overwrites, escape the workspace, invent metadata, or perform
another repair operation.

### Global flags

The six global flags keep their shared spelling, value grammar, default,
meaning, omission, repetition, composition, terminal behavior, and errors from
[Global CLI Flags](../shared/global-flags/interface.md). This command declares
only that all six apply; it does not define another global-flag table.

`index` adds no command-specific terminal-mode rule. Help, version, conflicts,
domain-input handling, and no-op behavior remain defined only by the shared
Global CLI Flags contract.

`index` adds no command-specific global-flag composition rule. Applicable global
context and presentation flags compose only as defined by the shared Global CLI
Flags contract.

## Selection

### Rooted selection

The smallest valid invocation is:

```text
open-forge index
```

With no source operand, the command starts from the exact `.agents/loader.md` in
the selected workspace.

When that Loader is present, each intended root is one structurally valid,
physically unique recognized entrypoint that directly represents one
`.agents/<slug>` folder. More than one recognized entrypoint representing the
same root folder is ambiguous and blocks the request. The operand-free form
selects the Loader's generated region and every entrypoint region reachable from
those intended roots through the current routed topology.

A missing Loader forms zero intended roots and blocks the operand-free form. It
does not make explicit sources invalid: an explicit selection whose detached
closure is complete may proceed. The command never adopts another Loader or
silently includes an unreachable detached tree.

Current generated lines do not add, hide, or order routed sources during rooted
selection. Selection follows the current topology rather than stale generated
navigation.

### Entrypoint selection

Representative forms are:

```text
open-forge index memory
open-forge index .agents/memory/_memory.md
```

An entrypoint operand selects the selected entrypoint's generated region, every
entrypoint region reachable below it through direct routed children, and its
direct exposing parent region when that parent exists.

The direct exposing parent is included because its generated entry reflects the
selected entrypoint's authored description and tags. Higher ancestors are not
included merely because they are ancestors; their direct-child metadata does
not depend on the selected entrypoint.

The entrypoint descendant closure is complete. The caller does not need to list
every descendant.

### Routed-leaf selection

Representative form:

```text
open-forge index skills/experience-design
```

A routed leaf selects only the direct entrypoint that exposes it. Regenerating
that parent projects all of its current direct children, not only the selected
leaf.

A source with no mechanically established exposing entrypoint has no valid
generated target. The command blocks and reports a useful next action.

### Detached entrypoint selection

An explicitly selected entrypoint may be maintained as a detached source when
its local entrypoint and descendant topology is complete and unambiguous.

A detached selection includes its local subtree and any direct parent present in
that same topology. It does not invent a missing Loader or parent and does not
classify the source as an installed Framework.

A missing intermediate entrypoint leaves the lower topology detached. Explicit
selection may maintain that complete detached topology, but it never bridges the
gap or invents the absent intermediate parent.

### Overwrite selection

A source reference naming a valid overwrite companion resolves to its base
logical source under the shared source-reference contract. Target selection uses
the base source's route relationship.

Overwrite content, frontmatter, descriptions, and tags never contribute to a
generated entry. `index` never modifies an overwrite companion. An orphan or
ambiguous overwrite cannot establish a valid logical source and blocks
selection.

### Several selections

Representative form:

```text
open-forge index memory skills/experience-design
```

Several source operands union every selected target closure and process each
generated region once. Duplicate operands and overlapping subtrees do not
duplicate work, diffs, or result entries.

Target regions are ordered by canonical workspace-relative containing-file path
using ordinal comparison. Filesystem enumeration order, locale, operand
overlap, and discovery timing do not change that order.

### Physical identity conformance

Physical aliases proven on the current host collapse to one target only when
their route identity and recognized document-form identity are compatible. A
proven incompatible physical alias blocks selection. If compatible traversal
paths expose the same proven physical entrypoint, the command produces one
logical selection, one target region, one plan item, and at most one effect for
that identity.

This contract does not claim broader portable equivalence for case folding,
Unicode normalization, or device-name rules that the current host has not
proved. Those cross-host equivalence rules are deferred. The formation and
selection rules here do not change current-visible Route or Context facts.

## Authoritative Projection

Filesystem topology and authored source metadata define expected generated
navigation. Current generated `Entries` are comparison input only; they never
become a second route inventory or a metadata fallback.

For each selected Loader or entrypoint, the observable expected projection
contains one canonical generated entry per current direct routed child, after
required metadata is validated and entries are sorted by canonical
containing-file-relative destination using ordinal comparison.

Ambiguous entrypoints, proven incompatible aliases, or two children that cannot
retain distinct safe route identities block the complete selected plan.

Ordinary indexed Markdown and entrypoints contribute their authored
`description` and tags. A recognized native source such as `SKILL.md` contributes
the metadata and classification defined by its source contract.

The command preserves authored description and tag spelling. It does not infer,
summarize, correct, normalize, or invent semantic metadata from filenames,
titles, bodies, generated lines, or overwrite companions.

Missing, empty, malformed, ambiguous, or mechanically inconsistent required
metadata blocks the complete selected plan. Whether a description is useful or
accurate remains authored responsibility and may become a `doctor` finding;
`index` does not perform semantic search or rewrite prose.

### Generated lines

Every non-empty generated body uses this exact canonical line shape:

```md
- [Description](relative/path.md) - #Type #Scope
```

A generated line has the accepted description, containing-file-relative
destination, separator, and useful bare tags shown above.

A destination resolves relative to the Loader or entrypoint containing the
generated region. It identifies a direct routed source, remains contained in the
selected workspace, omits query strings and fragments, and uses canonical
encoding.

An entrypoint with no direct routed children uses this exact canonical empty
body:

```md
- none - No entries - #Empty
```

The empty body is the generated result for an entrypoint with no direct routed
children. It is not a link to an overwrite or support resource.

Exact Markdown parsing and compatible-input behavior, destination encoding,
line endings, and serialization are realized by the accepted [Technical
Design](technical-design.md#markdown-yaml-and-byte-boundaries). The accepted
observable shapes and stable-byte requirement remain in force.

## Generated Boundary And Bounded Effects

The only generated region eligible for replacement has this exact shape:

```md
## Entries

<!-- open-forge:generated-index:start -->

...

<!-- open-forge:generated-index:end -->
```

`index` changes only the body between the exact markers in one valid final
`Entries` section.

A valid generated boundary has exactly one final `Entries` section, exactly one
complete and ordered marker pair, no nested or overlapping marker pair, and the
generated region as the final inline region in the file's final section.

Stale, missing, duplicate, or malformed generated entry lines inside a valid
marker pair may be replaced with the complete expected body.

The command does not create, move, or repair missing, duplicate, misplaced,
reversed, nested, or otherwise ambiguous markers. It blocks before writes when
the generated ownership boundary cannot be established.

Every replacement preserves the marker tokens and all bytes outside the
generated interior. `index` never formats the complete file or normalizes
authored frontmatter, headings, prose, links, or whitespace outside that body.

Before replacing an existing generated target, application preparation uses only
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and its application-owned
`OpenForge/recovery/v1` subtree. There is no temporary-directory, repository,
`HOME`, or custom-platform fallback; unavailable storage is a
pre-effect `incomplete` result. When the operation has one or more existing
targets to replace, it gets exactly one immutable ZIP bundle
outside the workspace. An operation containing only Create effects or no-ops
creates no bundle. Its source-generated
schema-v1 `manifest.json` and streamed ordinal payload entries record
command/operation/workspace identity, ordered relative targets, change kinds,
exact prior bytes/lengths/hashes, and intended final absence or length/hash.
`Create` and byte/semantic no-op effects add no bundle entry. A CreateNew draft is
closed and reopened for semantic manifest, exact ordered entry, length, hash,
and payload-byte validation, moved within the same directory to its deterministic
final name, and reopened and verified. Only the valid final ZIP forms the opaque
`RecoveryBundlePreparation`; the draft remains `Incomplete`. All preparation
completes before the first target effect, and `FileChangeApplier` requires the
matching preparation for every existing-target effect (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`). Successful preparation begins the apply
phase: target drift discovered after it is
`index.target-changed-during-apply`, the affected region remains `not-started`,
and the final bundle is retained. After final
verification, only the positively recognized bundle created by this command is
deleted. `Deleted`/`Removed` produces recovery `removed` and, absent another
finding, `complete`. `Failed`/`Retained` is possible only after positive presence
and produces `attention`, `index.recovery-artifact-retained`, the exact residual
path, and cleanup guidance. `Failed`/`Unknown` produces `failed`,
`index.recovery-failed`, and recovery `unknown`; it carries the exact expected
path only when M1 returns it. `Blocked` and `Cancelled` retain their neutral M1
truth for later operation mapping. A handled failure or cancellation after
preparation but before post-verification deletion reports the exact final path;
that positively verified final remains because deletion has not begun. A
deletion result with disposition `Unknown` makes no retention claim.
A closed final ZIP may remain after abrupt process termination, without an
executable crash or power-loss guarantee. The command never restores a target
automatically or derives current target state from recovery provenance. Cleanup
owns exact named final and draft deletion under its separate lease-bound
contract.

## Operation Modes And Observable Output

### Application authority

Omitting `--dry-run` selects application. The explicit invocation confirms
replacement of every changed body inside the selected machine-owned generated
boundary. The command does not prompt again and does not accept `--yes`.

This application authority does not permit marker repair, authored-content
replacement, whole-file formatting, overwrite mutation, path escape, metadata
invention, or another repair operation.

### Dry run

Representative form:

```text
open-forge index --dry-run
```

Dry-run output represents every planned update with the exact bounded
generated-region diff. It does not expose unrelated authored bytes or private
recovery material.

Each human diff block begins with this exact header, where all three values are
JSON strings using JSON escaping:

```text
@@ {"id":<JSON string>,"path":<JSON string>,"scope":<JSON string>} @@
```

The before body is tokenized first and every token is emitted with `- `; the
expected body follows and every token is emitted with `+`. Tokenization splits
after every newline and retains that newline, including exact LF versus CRLF. A
final non-newline tail is its own token, and an empty body is one `""` token.
Decoding and concatenating the before or expected tokens reproduces its exact
generated-interior body. The diff contains no context, whole-file bytes,
authored prefix or suffix, heuristics, elision, size limit, or truncation.
Compact and expanded views both include every exact dry-run diff block. JSON
stores the before and expected generated-interior bodies as typed change facts;
it does not store this textual diff.

A dry run with changes is `complete` when the complete plan and application
preconditions were established safely. It says that regions would be updated
and that no files changed. A dry run does not claim on-disk verification of bytes
that were not written. No current dry-run condition produces `attention`; the
only current `attention` producer is a retained recovery artifact after a
verified application.

### Human output

Human output leads with what happened or would happen. Primary human rendering
for `complete`, `attention`, and `incomplete` results goes to stdout. Primary
human error rendering for `invalid`, `blocked`, `failed`, and `interrupted`
results goes to stderr. Each primary human typed result stays together on its
assigned stream; an `incomplete` result does not split safe facts from its
findings. Separate bounded diagnostics use stderr. Human output avoids internal
stage terms when the user only needs the outcome.

The default expanded view uses the complete examples below and includes
per-target counts, evidence, provenance, and next actions. Compact view keeps
the semantic result, changed and unchanged counts, affected paths, required
safety findings, and next actions. A compact dry run still includes every exact
bounded diff because view selection cannot weaken effect review.

When the typed semantic status is `attention`, human output renders it as
`requires attention` because the phrase explains the relationship more clearly
on first read.

Default human output does not name successful internal planning stages.
`--verbose` may add bounded command diagnostics, but it cannot add structured
schema members or expose GN or M1 stage objects.

A verified no-op uses this output:

```text
Generated Entries are up to date.
Checked 12 regions. No files changed.
```

A successful application begins with this summary:

```text
Generated Entries were updated.
Checked 12 regions: 2 updated, 10 already up to date.
All 12 regions match the routed sources.
```

The default successful-application result lists each changed path and its
before-and-after entry count. It does not list every unchanged path unless
`--verbose` is selected.

A successful dry run uses this output:

```text
Generated Entries would be updated.
Checked 12 regions: 2 need updates, 10 are up to date.

<exact bounded diffs>

No files changed (--dry-run).
```

A blocked result uses this output:

```text
Generated Entries were not updated.

Blocked: the generated markers are ambiguous.
  .agents/memory/_memory.md

No files changed.
Next: keep one complete generated marker pair in the final Entries section,
then rerun open-forge index.
```

Every ordinary error names the `index` operation, affected source or region,
direct cause, and useful next action when one exists. Its primary human
rendering is kept together on stderr for `invalid`, `blocked`, `failed`, and
`interrupted` results.

### Structured output

`--json` returns one complete structured result document to stdout for every
semantic status from the same typed result used by human rendering. It never
prompts and never reruns planning, application, or verification. Separate
bounded diagnostics use stderr, and ordinary human text is never mixed into
structured JSON stdout.

The command-owned `result` object is reduced to these members in exactly this
order:

```text
mode,
selection,
regions,
recovery,
findings,
counts
```

`mode` is a required JSON string exactly `apply` or `dry-run`.
`IndexSelectionV1` has members exactly `origin`, `scope`, `sources`, in that
order. `origin` and `scope` are required JSON strings. `origin` is exactly
`automatic-loader` or `explicit-sources`; `scope` is exactly `not-established`,
`rooted`, `detached`, or `mixed`; and `sources` is a never-null array.

Every `sources` member is an `IndexLogicalSourceV1` with members exactly `id`,
`path`, `scope`, in that order. All three are required non-null JSON strings;
`scope` is exactly `rooted` or `detached`. The selection contains safe normalized
logical sources only: `automatic-loader` contains exactly the normalized Loader,
when that Loader is safely established. A pre-selection workspace or Loader
failure retains origin `automatic-loader`, scope `not-established`, and an empty
`sources` array. `explicit-sources` contains only safely resolved sources and may
be empty when none was established. Overwrite references are normalized to the
base, proven physical aliases are deduplicated, and final ordering is by
canonical path then ID using ordinal comparison. Raw operands are never retained
in the result.

Selection coherence is exact: `not-established` means no safe source scope was
established and requires `sources = []`; `rooted` means every normalized source
has logical scope `rooted` and `sources` is non-empty; `detached` means every
normalized source has logical scope `detached` and `sources` is non-empty; and
`mixed` means the normalized sources contain both logical scopes.

Each `IndexRegionV1` has members in exactly this order:

```text
source,
action,
beforeEntryCount,
expectedEntryCount,
change,
outcome
```

`action` is exactly `not-established`, `unchanged`, or `update`. `outcome` is
exactly `not-established`, `already-current`, `not-requested`, `not-started`,
`applied`, `verified`, or `unknown`. `source` is a required non-null
`IndexLogicalSourceV1`. `action` and `outcome` are required non-null JSON strings.
The two entry counts and `change` are required members that serialize explicit
`null` where the coherence rules below say the value is absent; a non-null entry
count is a nonnegative JSON integer.

Non-null `change` is an `IndexChangeV1` with members exactly `beforeBody`,
`expectedBody`, in that order. Both are required non-null JSON strings containing
the exact generated-interior bodies. It does not contain whole-file bytes or a
textual diff.

The region coherence rules are exact:

- `not-established` has no entry counts or change and has outcome
  `not-established`.
- `unchanged` has both non-null entry counts, no change, and outcome
  `already-current`.
- `update` has a non-null `expectedEntryCount` and one change.
  `beforeEntryCount` is the count only when the complete bounded interior parses
  through the sole generated-Entries parser; it is `null` when a valid
  replaceable interior is unparseable. Human output renders that null as
  `unknown`, never `0`. Dry run uses
  `not-requested`; an apply stopped before that target uses `not-started`; a
  known applied but unverified target uses `applied`; a known applied and
  verified target uses `verified`; and unprovable target disposition uses
  `unknown`.

`IndexRecoveryV1` has members exactly `state`, `residualPath`, in that order.
Both members are present; `residualPath` serializes explicit `null` where no exact
path is available and otherwise is a JSON string. `state` is a required non-null
JSON string exactly `not-required`, `not-created`, `removed`, `retained`, or
`unknown`. It is `not-required` for dry run, no-op, or a plan with no existing
target; `not-created` when recovery was required but no artifact was positively
created; `removed` when the verified operation artifact was positively removed;
`retained` only when the support candidate is positively present; and `unknown`
when its disposition is unprovable. `residualPath` is non-null for `retained` and
is the exact path. It may also be non-null for `unknown` when the Mutation
Foundation truthfully reports one exact observed or expected support path. It is
null whenever no exact path is known.

`IndexFindingV1` has members exactly `code`, `status`, `sourceOccurrence`,
`source`, `cause`, `candidates`, in that order. All members are present;
`code`, `status`, and `cause` are required non-null JSON strings.
`sourceOccurrence` is `null` or a one-based positive JSON integer; `source` is
`null` or an `IndexLogicalSourceV1`. Both serialize explicit `null` where they do
not apply. A non-null `source` and every `candidates` member use
`IndexLogicalSourceV1`. `candidates` is a never-null array of safe normalized
logical sources ordered by path then ID ordinal.

`IndexCountsV1` has members in exactly this order: `regions`, `updates`, `unchanged`,
`applied`, `verified`. All five are required nonnegative JSON integers.
`regions` equals the `regions` array length; `updates` equals the number of
regions with action `update`; `unchanged` equals the number with action
`unchanged`; `applied` counts update outcomes `applied` and `verified`;
`verified` counts `already-current` and `verified`; and
`updates + unchanged <= regions`.

The `selection.sources`, `regions`, `findings`, and each finding's `candidates`
arrays are never `null`. The nullable members are region `beforeEntryCount`,
`expectedEntryCount`, and `change`; recovery `residualPath`; and finding
`sourceOccurrence` and `source`. They serialize explicit `null` where not
applicable; no member is omitted.

Result, human output, and diagnostics expose no raw absolute operand, physical
source path, complete document or surrounding authored bytes, GN or M1 internal
record or enum, hash, lock or operation ID, manifest or payload, temporary or
staging path, or raw exception. The exact recovery `residualPath` is the sole
accepted absolute-path exception. Finding `cause` is bounded command-authored
text; it is never a forwarded exception or raw absolute operand.

For the current attention result—a retained recovery artifact after verified
application—the structured status value is `attention`; only human presentation
uses `requires attention`.

The exact field names, schema versioning, compatibility rules, and numeric exit
mapping are defined by the accepted [Open Forge CLI Architecture](../../architecture.md#result-json-coordinates-and-process-status).
The [Technical Design](technical-design.md#json-and-presentation) describes
the source-generated serialization path without changing that shared authority.

## Semantic Results

| Result        | Meaning                                                                                                                                                                                                                                                                                |
| ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | Dry run formed and preflighted the complete plan, or application and final verification completed, including a verified no-op. |
| `attention`   | Application and verification completed, but the command-owned recovery artifact is positively retained. Human output says `requires attention`. |
| `incomplete`  | Safe inspection facts are available, but complete target discovery or projection coverage could not finish. No mutation begins, and human facts and findings remain one result.                                                                                                        |
| `invalid`     | Command input, a flag value, or a source reference does not follow the accepted interface.                                                                                                                                                                                             |
| `blocked`     | A valid request cannot establish or apply one safe complete plan. No mutation begins.                                                                                                                                                                                                  |
| `failed`      | Application, verification, or bundle handling failed to complete the selected operation.                                                                                                                                                                                               |
| `interrupted` | The caller cancelled or interrupted the operation before completion and no unexpected application or verification failure changes the result.                                                                                                                                                |

Changes and verified no-ops are ordinary `complete` results. They do not
require `attention` merely because bytes changed or no effect was needed.

When more than one condition is present, semantic status uses this exact
precedence: `failed` > `interrupted` > `invalid` > `blocked` > `incomplete` >
`attention` > `complete`. Every finding's status matches its fixed code mapping.
A `complete` result has no findings; the only currently accepted `attention`
producer is a retained recovery artifact.

### Finding codes, order, and status

The complete Index finding vocabulary is fixed in this order:

| Order | Code | Status | Public condition |
| ---: | --- | --- | --- |
| 1 | `index.invalid-input` | `invalid` | An option or binding combination is invalid. |
| 2 | `index.invalid-source` | `invalid` | An explicit source is malformed, unknown, or unsupported. |
| 3 | `index.workspace-unavailable` | `blocked` | The selected workspace cannot be established. |
| 4 | `index.workspace-unsafe` | `blocked` | Workspace identity or containment is unsafe. |
| 5 | `index.source-ambiguous` | `blocked` | One explicit source has multiple normalized candidates. |
| 6 | `index.source-unsafe` | `blocked` | A selected source crosses an unsafe boundary. |
| 7 | `index.topology-ambiguous` | `blocked` | The selected topology closure is not unique. |
| 8 | `index.target-unexposed` | `blocked` | A selected routed leaf has no safe exposing entrypoint or parent. |
| 9 | `index.target-unsafe` | `blocked` | Target identity, containment, or write boundary is unsafe. |
| 10 | `index.metadata-unsafe` | `blocked` | Admitted metadata makes the complete plan unsafe. |
| 11 | `index.generated-region-unsafe` | `blocked` | The generated boundary is missing, malformed, ambiguous, or unsafe. |
| 12 | `index.workspace-lock-unavailable` | `blocked` | The workspace lock could not be acquired; no contention claim is inferred. |
| 13 | `index.target-changed` | `blocked` | A required target changed before any effect. |
| 14 | `index.recovery-conflict` | `blocked` | Recovery collision or recognition blocks preparation. |
| 15 | `index.discovery-incomplete` | `incomplete` | Required source or topology facts are unavailable. |
| 16 | `index.metadata-incomplete` | `incomplete` | Required metadata is unavailable. |
| 17 | `index.projection-incomplete` | `incomplete` | Exact expected generated bodies are unavailable. |
| 18 | `index.recovery-unavailable` | `incomplete` | Writable recovery storage is unavailable before effects. |
| 19 | `index.recovery-artifact-retained` | `attention` | Verified effects succeeded, but cleanup leaves the recognized artifact. |
| 20 | `index.target-changed-during-apply` | `failed` | A required target changed after successful recovery preparation began the apply phase. |
| 21 | `index.write-failed` | `failed` | A target effect failed. |
| 22 | `index.verification-failed` | `failed` | Applied content could not be verified. |
| 23 | `index.recovery-failed` | `failed` | Recovery handling failed after effects began. |
| 24 | `index.operation-failed` | `failed` | An unexpected operation failure falls outside every named condition. |
| 25 | `index.interrupted` | `interrupted` | Cancellation occurred without a higher-priority failure. |

Findings are ordered by this table, then by `sourceOccurrence` with `null` first
and positive integers in numeric ascending order, followed by `source.path`,
`source.id`, and `cause` using ordinal comparison.

### Exact next actions

`next` is deterministic and never interpolates a source operand:

| Result condition | `next.command` | `next.reason` |
| --- | --- | --- |
| `complete` | `null` | `null` |
| `invalid` | `open-forge index --help` | `Correct the named Index input, then rerun the request.` |
| `blocked`, first finding `index.source-ambiguous` | `open-forge index` | `Replace every ambiguous source with one listed exact path, then rerun the same Index request.` |
| `blocked`, first finding `index.workspace-lock-unavailable` | `open-forge index` | `Wait for the workspace lock to become available or inspect lock availability, then rerun Index from a fresh plan.` |
| `blocked`, first finding `index.target-changed` | `open-forge index` | `Inspect the changed target, then rerun Index from a fresh plan.` |
| other `blocked` | `open-forge doctor` | `Inspect the blocked workspace, topology, metadata, generated-region, or recovery boundary before rerunning Index.` |
| `incomplete` | `open-forge doctor` | `Inspect the unavailable discovery, metadata, projection, or recovery facts before relying on this Index result.` |
| `attention` | `open-forge cleanup` | `Review and remove the reported recovery artifact after confirming the verified Index result.` |
| `failed` | `open-forge index --verbose` | `Report the failure and retry the same Index request with bounded diagnostics.` |
| `interrupted` | `open-forge index` | `Rerun the same Index request.` |

Specialized blocked guidance uses the first finding of the overall result status.

The numeric process-exit mapping is the shared mapping defined by the accepted
Architecture. This command adds no command-specific exits.

## Errors And Boundaries

Primary human errors for `invalid`, `blocked`, `failed`, and `interrupted`
results use stderr under the [accepted public output rules](#accepted-public-output-and-result-rules).
Under `--json`, the complete structured result for any of these statuses
remains the single stdout document under [Structured output](#structured-output);
separate bounded diagnostics use stderr.

An empty, unknown, missing, or unsupported source reference is invalid.

A directory path is invalid because `index` operands identify sources. Use the
entrypoint ID or exact entrypoint file path.

An ambiguous source ID is blocked unless interactive source disambiguation
resolves it under the shared contract.

A structurally ambiguous route relationship remains blocked even when an exact
path identifies the file.

A path escape, unsafe physical identity, or proven current-host target collision is
blocked.

A missing Loader blocks the operand-free form but does not block an otherwise
complete explicit detached selection.

A selected source without an exposing region is blocked.

Missing or invalid required routing metadata blocks the complete plan.

Missing, duplicate, misplaced, reversed, nested, or ambiguous generated
markers block the complete plan.

A missing, unverified, or colliding required recovery bundle blocks before the
first write. Unavailable or unsafe bundle storage is `incomplete` before the
first write.

A source or destination change detected before successful recovery preparation
blocks the plan. Successful preparation begins the apply phase. A later
per-target drift fails the operation as `index.target-changed-during-apply`,
keeps that region `not-started`, retains the final recovery bundle, and stops
new effects before stale intent can write that target.

`doctor` owns complete structural diagnosis and recommendations. `index` reports
only findings needed to select, project, apply, and verify generated navigation.

## Representative Scenarios

The scenarios below cover each selection form, each omission state, each
Boolean write-policy state, accepted Boolean repetition, each global terminal
mode, the assigned human and structured streams, and each semantic result
without enumerating every compatible combination.

| Invocation or state                                                                                                                                   | Observable result                                                                                                                                                                                       |
| ----------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `open-forge index` with a valid Loader and rooted topology                                                                                            | The Loader and every reachable entrypoint region are selected.                                                                                                                                          |
| `open-forge index memory`                                                                                                                             | The entrypoint, its routed descendant closure, and its direct exposing parent when present are selected.                                                                                                |
| `open-forge index .agents/memory/_memory.md`                                                                                                          | The exact entrypoint path selects the same logical entrypoint as its source ID.                                                                                                                         |
| `open-forge index skills/experience-design`                                                                                                           | Only the direct exposing parent is selected, and that parent projects all current direct children.                                                                                                      |
| An explicit detached entrypoint with complete local topology                                                                                          | Its local subtree and any parent present in that topology are maintained without inventing a Loader or installed Framework route.                                                                       |
| An ID, base path, or overwrite path for a valid pair                                                                                                  | The base logical source supplies route identity; overwrite content never becomes a generated entry.                                                                                                     |
| `open-forge index memory skills/experience-design` with duplicate or overlapping closures                                                             | Each target region is processed once in canonical path order.                                                                                                                                           |
| A compatible recognized entrypoint reached through more than one current-host physical-alias path                                                     | The physical entrypoint is processed exactly once; a proven incompatible alias blocks.                                                                                                                  |
| A selected entrypoint has no direct routed children                                                                                                   | Its expected generated body is `- none - No entries - #Empty`.                                                                                                                                          |
| A valid target has stale generated lines                                                                                                              | Application replaces only the bounded generated interior.                                                                                                                                               |
| `open-forge index --dry-run` with changes                                                                                                              | The result is `complete`; human output shows exact bounded diffs and says no files changed, and structured output carries exact before/expected bodies.                                                  |
| `open-forge index --dry-run --dry-run`                                                                                                             | Repeated Boolean occurrences are accepted with no additional effect, and the preview still writes nothing.                                                                                             |
| `open-forge index --workspace ../another-workspace`                                                                                                   | The exact supplied workspace is used; no parent or Git-root discovery occurs.                                                                                                                           |
| `open-forge index --json`                                                                                                                             | One complete structured result for every semantic status is written to stdout, preserving the typed status. Separate bounded diagnostics use stderr; ordinary human text is not mixed into JSON stdout. |
| `open-forge index --verbose`                                                                                                                          | Bounded diagnostics are added without changing operation behavior or status.                                                                                                                            |
| `open-forge index --help`                                                                                                                             | Help for `index` is shown without workspace resolution or domain execution.                                                                                                                             |
| `open-forge index --version`                                                                                                                          | The distributed CLI version is shown without workspace resolution or a domain operation.                                                                                                                |
| A valid unchanged target set after a successful prior application                                                                                     | The result is `complete`, reports a verified no-op, and performs no write or bundle preparation.                                                                                                       |
| A valid empty target projection                                                                                                                       | The result can be a complete update or verified no-op with the exact empty body, depending on current bytes.                                                                                            |
| Safe facts exist but complete discovery or projection coverage cannot finish                                                                          | The result is `incomplete`, no mutation begins, and safe facts remain together with their findings in the primary human result.                                                                         |
| An unknown source, invalid flag value, or conflicting terminal input                                                                                  | The result is `invalid`; the primary human error is on stderr and identifies the useful correction when one exists.                                                                                     |
| Missing Loader, invalid metadata, unsafe boundary, missing/unverified/colliding recovery bundle, or another unsafe complete-plan condition       | The result is `blocked`, no mutation begins, and the primary human error is on stderr.                                                                                                                  |
| Unavailable or unsafe recovery-bundle storage                                                                                                     | The result is `incomplete`, no mutation begins, and the primary human result retains the safe facts and storage limitation.                                                                                |
| Application, verification, or bundle handling cannot complete                                                                                         | The result is `failed` or `interrupted` according to the semantic-result definitions, the primary human error is on stderr, and residual bundle state is reported when present.                          |
| `open-forge index --view=compact`                                                                                                                     | Human output retains semantic result, effect counts, affected paths, safety findings, required next actions, and every dry-run diff while omitting optional explanation and provenance.                 |
| `open-forge index --view=expanded` or omitted `--view`                                                                                                | Human output uses the default expanded examples and includes complete ordinary evidence and provenance.                                                                                                 |

## Non-Goals

`index` does not:

- Validate every Framework, route, link, overwrite, or lifecycle contract.
- Repair or create generated markers.
- Rewrite authored metadata, prose, links, headings, or whitespace.
- Format complete files.
- Add, move, remove, or rename routed sources.
- Infer semantic descriptions, tags, loading, scope, or authority.
- Index overwrite companions or non-routed support resources.
- Build a search, vector, graph, or persistent retrieval index.
- Persist a route graph, saved plan, transaction journal, or historical baseline.
- Discover another workspace or Loader.
- Create a Git commit.

## Public Verification

The caller-visible and process-boundary concerns allocated to this contract are
mandatory evidence concerns. Eventual implementation evidence must cover:

- Exact CWD and `--workspace` selection without discovery.
- Operand-free Loader-rooted selection.
- Entrypoint subtree plus direct-parent selection.
- Routed-leaf parent selection.
- Detached entrypoint selection without invented roots or parents.
- Several operands, duplicates, overlaps, source-ID collisions, and exact-path
  disambiguation.
- Base and overwrite references, orphan overwrites, and proof that overwrite
  content never enters generated navigation.
- Compatible current-host physical aliases processed once, proven incompatible
  aliases blocked, and no unproved portable case/Unicode/device equivalence.
- Human `up to date` and `requires attention` wording on the assigned result
  stream without default preflight jargon.
- Human and structured output from the same typed result, with primary human
  result and error streams, one JSON document on stdout, and bounded diagnostics
  on stderr.
- Complete, attention, incomplete, invalid, blocked, failed, and interrupted
  results, including proof that dry run never produces the current retained-
  artifact attention condition.
- Exact human examples, structured bounded-change evidence, accepted idempotent
  Boolean repetition, no-op output, and the no-files-changed dry-run promise.

## Accepted Public Output And Result Rules

Primary human rendering for `complete`, `attention`, and `incomplete` results
goes to stdout. Each primary human typed result stays together on stdout,
including safe incomplete facts and their findings.

Primary human error rendering for `invalid`, `blocked`, `failed`, and
`interrupted` results goes to stderr. Each primary human typed error result stays
together on stderr.

A dry run with a safely established complete plan returns `complete`, exposes
every exact bounded diff, and states that no files changed. Other dry-run
conditions retain their exact invalid, blocked, incomplete, failed, or
interrupted code/status mapping. Dry run never creates or retains a recovery
artifact and therefore has no accepted `attention` producer.

The shared [Global CLI Flags](../shared/global-flags/interface.md) contract
remains authoritative for `--json`: it renders one complete structured result
document to stdout for every semantic status. Separate bounded diagnostics
remain on stderr, and the accepted human stream rules above do not mix ordinary
human text into JSON stdout.
