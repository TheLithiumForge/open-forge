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
defines the complete spelling and meaning of `--workspace`, `--format json`, `--detail`,
`--detail debug`, `--help`, and `--version`. All six apply to `index` under that
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
- [Shared Result Coordinates](../shared/result-coordinates/interface.md)
  defines the shared exact structured-result schema and numeric process-exit
  mapping.
- [Shared CLI Operation Contract](../../shared-operation-contract.md) defines
  cross-command operation conventions.

## Shared Schema And Process Exits

The accepted [Shared Result Coordinates](../shared/result-coordinates/interface.md)
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

`--dry-run` does not grant authority to repair headings, replace authored
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
safe topology and metadata facts are evaluated, with entries sorted by
canonical containing-file-relative destination using ordinal comparison.

Ambiguous entrypoints, proven incompatible aliases, or two children that cannot
retain distinct safe route identities block the complete selected plan.

Ordinary indexed Markdown and entrypoints contribute their authored
`description` and tags. A recognized native source such as `SKILL.md` contributes
the metadata and classification defined by its source contract.

The command preserves authored description and tag spelling. It does not infer,
summarize, correct, normalize, or invent semantic metadata from filenames,
titles, bodies, generated lines, or overwrite companions.

For an ordinary Markdown or eligible Open Forge entrypoint source, Missing
optional metadata remains routable and projectable through the accepted
automatic-ID fallback and any observed valid tags. It remains Missing rather
than becoming Complete, emits the visible `index.metadata-optional` Attention2
finding on apply, preview, and verified no-op, and never rewrites the source.
Native Skill, Loader, and overwrite-companion forms remain strict and do not
use this fallback. Supplied malformed or otherwise invalid values are not
reclassified as optional Missing.

A readable malformed ordinary or native Skill metadata region may be skipped only when the
complete selected topology, identity, and catalogue readiness are established
and at least one independent available region has a change to apply. The safe
regions remain projectable and executable; the skipped region remains
`not-established`, reports `index.metadata-skipped` with its exact source and
cause, and makes the result `incomplete`. This is not generic best-effort
processing: unreadable or unsafe boundaries, missing native Skill metadata,
and malformed-only selections are not admitted or silently skipped. Whether a
description is useful or accurate remains authored responsibility and may
become a `doctor` finding; `index` does not perform semantic search or rewrite
prose.

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

One top-level canonical ATX `## Entries` heading owns the body from the end of
its heading span to the next top-level heading of level 1 or 2, or EOF. Fenced,
indented-code, quoted, nested-list, Setext, differently cased and differently
leveled lookalikes do not establish this semantic section. Trailing horizontal
heading whitespace and an initial BOM are accepted. Duplicate `## Entries`
headings are diagnosed; no arbitrary first section is selected.

Index replaces only that generated body, including any retired guard comments
found there. Missing, stale or malformed entry lines inside an established body
may be replaced with the expected entries. A missing or ambiguous heading boundary
blocks the plan before writes. The command does not create or repair headings.

Every replacement preserves the heading and all outside bytes. Canonical generated
output uses a blank line after the heading, one entry per line, a final newline,
and a blank separator before a following section. It retains the selected LF or
CRLF line ending. Prettier and repeated Index runs produce stable bytes for
canonical Markdown. Whole-file formatting remains outside Index authority.

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
finding, `completed`. `Failed`/`Retained` is possible only after positive presence
and produces `completed-with-warnings`, `index.recovery-artifact-retained`, the exact residual
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

## Final-Leaf Safety Boundary

Every generated region that `index` would replace has a caller-visible
no-follow observation of its immediate final filesystem component. A filesystem
link, reparse point, or special final leaf, including a relative file link that
is an exact Workspace Library projection, is separately owned and unsafe for
ordinary `index` mutation. The command returns its existing `blocked` target-safety
result, identifies the affected destination, and performs no effect.

This physical-leaf boundary does not depend on Library registration state. A missing,
malformed, stale, or otherwise unreadable Library record neither makes the
final leaf ordinary nor grants `index` mutation authority.

`index` never follows a final filesystem leaf to write generated bytes. It does
not resolve a final leaf and then write its physical target. The guard concerns
the final component addressed by each generated-region replacement; ordinary
directory-ancestry rules remain defined by the filesystem contract.

## Human Output

The command uses the shared native report. The default detail is `minimal`; `standard`, `full` and `debug` add the catalogue-defined facts. `--detail-filter <error|warning|info|all>` is repeatable and changes only the rendered detail. Use `--format text` for this text report. Primary result text for `completed`, `completed-with-warnings` and `incomplete` is on stdout; primary errors for `invalid-input`, `blocked`, `failed` and `cancelled` are on stderr. There is no `Status:` line.

### Statuses and headlines

| Status                  | When                                                | Headline                                                                                       | Exit | Stream |
| ----------------------- | --------------------------------------------------- | ---------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | nothing stale                                       | `Entries sections are current in all <M> files. Nothing to do.`                                |    0 | stdout |
| completed               | rewritten                                           | `Updated the Entries section in <N> of <M> files.`                                             |    0 | stdout |
| completed (dry run)     | changes planned                                     | `Would update the Entries section in <N> of <M> files.`                                        |    0 | stdout |
| completed-with-warnings | optional metadata warning or recovery bundle retained | headline + visible finding rows                                                               |    2 | stdout |
| incomplete              | independent safe regions applied while a readable malformed region was skipped, or a routed file, its metadata, or recovery was unreadable | `The Entries sections could not be rebuilt completely.` + verified update rows and exact limitation rows |    3 | stdout |
| invalid-input           | bad flag, folder operand, unknown source            | `Cannot index <operand>: <reason>.` / family                                                   |    4 | stderr |
| blocked                 | malformed leaf, unsafe target, lock, changed        | `Cannot rebuild the Entries section of <leaf's parent>.` + the leaf row                        |    5 | stderr |
| failed                  | after effects                                       | `Index stopped after <n> of <m> files.`                                                        |    1 | stderr |
| cancelled               | Ctrl+C                                              | `Index was cancelled. Nothing was changed.` or `... Stopped after <n> of <m> files.`           |  130 | stderr |

### Text by level

`minimal`, rewritten:

```text
Updated the Entries section in 2 of 21 files.
  .agents/memory/emerging/ideas/_ideas.md   0 -> 1 entries
  .agents/skills/_skills.md                 3 -> 4 entries
```

`minimal`, dry run:

```text
Would update the Entries section in 1 of 20 files.
  .agents/loader.md   8 -> 7 entries
No files were changed.
```

An optional-metadata finding remains visible at `minimal` for apply, dry-run,
and verified no-op results. It names the source and missing optional metadata;
it never describes that source as unreadable or recommends cleanup. An
`incomplete` partial result names the skipped source, exact reason and cause,
and the verified updates that were applied.

`minimal`, blocked (stderr):

```text
Cannot rebuild the Entries section of .agents/skills/_skills.md.
  Error  .agents/skills/pdf/SKILL.md:1:1  Frontmatter is invalid
         The frontmatter block is not closed.
  Nothing was written. The other 19 sections are current.
Next: open-forge doctor
```

`minimal`, folder operand (stderr):

```text
Cannot index .agents/memory/emerging/ideas/pricing: it is a folder, not a source.
Next: open-forge index memory/emerging/ideas/pricing
```

`standard` adds `Workspace:`, the unchanged files as one count line, and for
a dry run the diff of each changed section:

```text
--- .agents/loader.md  (Entries section)
- - [Reusable default shapes for code, files, APIs, documents, and other work](patterns/_patterns.md) - #LoadNow #Core #Pattern
```

Diff lines are authored content, written byte-exact through the authored span.
Blank list lines are not shown as `- ` lines.

`full` adds the selection (`loader roots` or the explicit sources), every
unchanged file as a row, and recovery facts in words.

### Representative transcripts by status

### Transcript — completed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#index-candidate-completed).

### Transcript — completed-with-warnings

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#index-candidate-completed-with-warnings).

### Transcript — incomplete

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#index-candidate-incomplete).

### Transcript — invalid-input

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#index-candidate-invalid-input).

### Transcript — blocked

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#index-candidate-blocked).

### Transcript — failed

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#index-candidate-failed).

### Transcript — cancelled

[Preserved interface example](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#index-candidate-cancelled).

## Structured Output

`--format json` writes one schema-3 envelope to stdout for every report status. It contains the command, status, workspace when applicable, detail, filter, command data, findings, effects, counts, limitations, recovery facts and next action as applicable. It is the same typed result as the text report; no ordinary text is mixed into the JSON document. If parsing fails before binding, the raw parser diagnostic remains text on stderr and no report envelope exists.

### JSON data by level

| Level    | `data`                                                                                                          |
| -------- | --------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, changes: [ { path, before, after } ] }`                                                                |
| standard | + `diff` per change (lines with `+`/`-` prefixes), `unchanged: [ { path } ]`                                    |
| full     | + `selection { origin, scope, sources: [ { id, path } ] }`, `regions: [ every region with action and outcome ]` |

## Semantic Results

The status and exit mapping above are unchanged by detail or format. Root effects and recovery receipts retain their complete result facts at every detail level; command-owned data follows the catalogue's level rows.

### Effects wording

`<path>  <before> -> <after> entries` for a rewritten section; `unknown ->
<after>` when the section was missing or unreadable before. Dry run: same
row. Partial: `<path>  not started` / `<path>  final state unknown`.

### Counts and limitations

`filesChecked`, `filesUpdated`, `filesCurrent`. They satisfy
`checked = updated + current` in every result; a file whose outcome is
unknown counts as neither and is listed.

### Next rules

Invalid source -> the corrected command; blocked or incomplete ->
`open-forge doctor`; lock -> rerun; retained recovery -> `open-forge cleanup`;
completed -> none.

## Errors And Boundaries

The findings catalogue below is the command's finite error and warning vocabulary. Findings keep their code, severity, family, subject and cause; detail filtering affects display only. A blocked, failed or cancelled result prevents further effects according to the catalogue.

### Findings catalogue

| Code                              | Severity | Family                      | Message                                                                                                     | Next                                                                             |
| --------------------------------- | -------- | --------------------------- | ----------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------- |
| index.invalid-input               | error    | invalid-input               |                                                                                                             |                                                                                  |
| index.invalid-source              | error    | local                       | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Index/Shared/Wording/IndexWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`index.invalid-source`). | `open-forge index <id>` when derivable, else `open-forge route list --depth=all` |
| index.workspace-unavailable       | error    | workspace-unavailable       |                                                                                                             |                                                                                  |
| index.workspace-unsafe            | error    | workspace-unsafe            |                                                                                                             |                                                                                  |
| index.source-ambiguous            | error    | source-ambiguous            |                                                                                                             |                                                                                  |
| index.source-unsafe               | error    | source-unsafe               |                                                                                                             |                                                                                  |
| index.topology-ambiguous          | error    | local                       | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Index/Shared/Wording/IndexWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`index.topology-ambiguous`).                               | `open-forge doctor`                                                              |
| index.target-unexposed            | error    | local                       | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Index/Shared/Wording/IndexWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`index.target-unexposed`).                                  | `open-forge doctor`                                                              |
| index.target-unsafe               | error    | target-unsafe               |                                                                                                             |                                                                                  |
| index.metadata-unsafe             | error    | metadata-unsafe             | the leaf row under the blocked headline                                                                     | edit the file                                                                    |
| index.generated-region-unsafe     | error    | generated-region-unsafe     |                                                                                                             |                                                                                  |
| index.workspace-lock-unavailable  | error    | workspace-lock-unavailable  |                                                                                                             |                                                                                  |
| index.target-changed              | error    | target-changed              |                                                                                                             |                                                                                  |
| index.recovery-conflict           | error    | recovery-conflict           |                                                                                                             |                                                                                  |
| index.discovery-incomplete        | warning  | local                       | [selection](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Index/Shared/Wording/IndexWording.cs); [independent forms](../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`index.discovery-incomplete`).                                             | `open-forge doctor`                                                              |
| index.metadata-optional           | warning  | metadata-optional           |                                                                                                             |                                                                                  |
| index.metadata-skipped            | warning  | metadata-skipped            | the skipped source row with its exact reason and cause                                                     | `open-forge doctor`                                                              |
| index.metadata-incomplete         | warning  | metadata-incomplete         |                                                                                                             |                                                                                  |
| index.projection-incomplete       | warning  | projection-unavailable      |                                                                                                             |                                                                                  |
| index.recovery-unavailable        | warning  | recovery-unavailable        |                                                                                                             |                                                                                  |
| index.recovery-artifact-retained  | warning  | recovery-artifact-retained  |                                                                                                             |                                                                                  |
| index.target-changed-during-apply | error    | target-changed-during-apply |                                                                                                             |                                                                                  |
| index.write-failed                | error    | write-failed                |                                                                                                             |                                                                                  |
| index.verification-failed         | error    | verification-failed         |                                                                                                             |                                                                                  |
| index.recovery-failed             | error    | recovery-failed             |                                                                                                             |                                                                                  |
| index.operation-failed            | error    | operation-failed            |                                                                                                             |                                                                                  |
| index.interrupted                 | error    | cancelled |                                                                                                             |                                                                                  |

## Scenarios

### Catalogue situations

`all-current`, `one-stale`, `dry-run-one-stale`, `explicit-source`,
`optional-metadata`, `folder-operand` (invalid-input), `unknown-source`
(invalid-input), `blocked-malformed-only`, `incomplete-independent-malformed`,
`incomplete-unreadable-child`, `lock-held`, `write-failed-partial`, `cancelled`.

Each status has one representative native text transcript above. JSON uses the same status and command facts under the schema-3 envelope.

### Open maintainer questions

The native all-current minimal report includes a `Workspace: <workspace>` echo, although the catalogue's no-op acceptance says one line. This contract records the native two-line transcript and leaves the precedence question unresolved. **Maintainer decision remains open.**
## Non-Goals

`index` does not:

- Validate every Framework, route, link, overwrite, or lifecycle contract.
- Repair or create semantic headings.
- Rewrite authored metadata, prose, links, headings, or whitespace.
- Format complete files.
- Add, move, remove, or rename routed sources.
- Infer semantic descriptions, tags, loading, scope, or authority.
- Index overwrite companions or non-routed support resources.
- Build a search, vector, graph, or persistent retrieval index.
- Persist a route graph, saved plan, transaction journal, or historical baseline.
- Discover another workspace or Loader.
- Follow a final filesystem link or reparse point, write through a Workspace
  Library projection, or adopt or manage a Library record.
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
- Final filesystem link and reparse-point leaves, including exact Workspace
  Library projections with and without a valid Library record, are reported as
  unsafe and block before effects; `index` never writes through them.
- Human `up to date` and `completed-with-warnings` wording on the assigned result
  stream without default preflight jargon.
- Human and structured output from the same typed result, with primary human
  result and error streams, one JSON document on stdout, and bounded diagnostics
  on stderr.
- Completed, completed-with-warnings, incomplete, invalid-input, blocked,
  failed, and cancelled results, including proof that dry run never produces
  a retained recovery artifact while optional-metadata warnings remain visible.
- Exact human examples, structured bounded-change evidence, accepted idempotent
  Boolean repetition, no-op output, and the no-files-changed dry-run promise.

## Accepted Public Output And Result Rules

Primary human rendering for `completed`, `completed-with-warnings`, and `incomplete` results
goes to stdout. Each primary human typed result stays together on stdout,
including safe incomplete facts and their findings.

Primary human error rendering for `invalid-input`, `blocked`, `failed`, and
`cancelled` results goes to stderr. Each primary human typed error result stays
together on stderr.

A dry run with a safely established complete plan returns `completed`, exposes
every exact bounded diff, and states that no files changed. A safely established
plan with eligible optional Missing metadata retains its visible
`completed-with-warnings`/Attention2 finding in preview, without creating or
retaining a recovery artifact. Other dry-run conditions retain their exact
invalid-input, blocked, incomplete, failed, or cancelled code/status mapping.

The shared [Global CLI Flags](../shared/global-flags/interface.md) contract
remains authoritative for `--format json`: it renders one schema-3 structured result
document to stdout for every semantic status. Separate bounded diagnostics
remain on stderr, and the accepted human stream rules above do not mix ordinary
human text into JSON stdout.



## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`index.help.syntax`](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Index/IndexText.cs).

<!-- @OpenForgeTextRef index.help.syntax -->

## Approved Journey Wording References

The following stable IDs link the approved journey behavior above to its typed
human-wording factories. Independently reviewed snapshots and state assertions
remain the output evidence.

- [IndexPhrases.cs](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Index/IndexPhrases.cs)
  <!-- @OpenForgeTextRef index.phrase.updated-the-entries-section-in-of-other-sources-were-skipped -->
  <!-- @OpenForgeTextRef index.phrase.would-update-the-entries-section-in-of-other-sources-were-skipped -->
  <!-- @OpenForgeTextRef index.phrase.optional-metadata-is-missing-for-observed-values-were-used -->
  <!-- @OpenForgeTextRef index.phrase.skipped-because-authored-metadata-is-malformed -->
- [IndexText.cs](../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Index/IndexText.cs)
  <!-- @OpenForgeTextRef index.title.optional-metadata-is-missing -->
  <!-- @OpenForgeTextRef index.title.metadata-was-skipped -->
