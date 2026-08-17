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

The public contract defines repetition of the Boolean write-policy flags,
human stream allocation, structured presentation, and the dry-run result when a
non-blocking finding is present in the [write-policy](#operands-and-flags),
[human output](#human-output), [structured output](#structured-output), and
[accepted public output](#accepted-public-output-and-result-rules) rules.
Value-bearing repetition remains governed by its defining contract.

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
  [--skip-git-check]
  [global flags]
```

`source-reference...` is an optional positional sequence. The two
command-specific flags are optional Boolean write-policy flags. Repeating
either Boolean flag is accepted and idempotent. The shared global flags are
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

### `--skip-git-check`

`--skip-git-check` is an optional Boolean write-policy flag with no value. Its
omission keeps the relevant-path Git cleanliness check. Its presence bypasses
only that check for an actual update.

Repeating `--skip-git-check` is accepted and idempotent. A second or later
occurrence has no additional effect. Repetition bypasses only the relevant-path
Git cleanliness check once; it does not multiply bypasses or grant another
authority. This matches repeated Boolean global flags. Value-bearing repetition
rules are unchanged under their defining contract.

`--skip-git-check` does not bypass route, metadata, marker, containment,
expected-state, verification, or recovery requirements. It does not grant
permission to overwrite, delete, force an operation, or take ownership of a
file.

`--dry-run` and `--skip-git-check` are compatible write-policy choices. Their
combination, including repeated occurrences of either flag, still stops before
persistent effects, and the skip flag cannot bypass any requirement other than
the relevant-path Git cleanliness check.

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

The operand-free form selects the Loader's generated region and every
entrypoint region reachable through the current routed topology.

The operand-free form never selects a detached tree. A missing or structurally
ambiguous Loader blocks it. The command does not adopt another Loader or
silently include an unreachable detached tree.

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

A recognized `_index.md` entrypoint is canonicalized by physical identity and
processed once. If more than one traversal path exposes the same recognized
physical entrypoint, the command produces one target region, one plan item, and
at most one effect for that identity. This is a conformance requirement, not a
staging or migration rule.

## Authoritative Projection

Filesystem topology and authored source metadata define expected generated
navigation. Current generated `Entries` are comparison input only; they never
become a second route inventory or a metadata fallback.

For each selected Loader or entrypoint, the observable expected projection
contains one canonical generated entry per current direct routed child, after
required metadata is validated and entries are sorted by canonical
containing-file-relative destination using ordinal comparison.

Portable path aliases, ambiguous entrypoints, or two children that cannot retain
distinct safe route identities block the complete selected plan.

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

A dry run with changes and no non-blocking finding is `complete` when the
complete plan and application preconditions were established safely. It says
that regions would be updated and that no files changed. A dry run with safely
established planned changes and a non-blocking finding is `attention` under the
[Accepted Public Output And Result Rules](#accepted-public-output-and-result-rules).
It still exposes the complete plan and exact bounded diffs, states that no files
changed, and renders the human result as `requires attention`. A dry run does
not claim on-disk verification of bytes that were not written.

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
`--verbose` and structured output may expose the named preflight stage for
diagnostics and automation.

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

When safely established planned changes also have a non-blocking finding, the
same complete plan and exact bounded diffs are shown, the result is rendered as
`requires attention`, and the output still states:

```text
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

The structured result exposes:

- Workspace and selection method.
- Explicit or automatic invocation origin.
- Apply or dry-run mode.
- Requested source references and resolved IDs and paths.
- Rooted or detached scope and ordered target closure.
- Authoritative topology and metadata coverage.
- Per-region containing-file ID and path, selection reasons, action, entry
  counts, bounded change evidence, and final effect state.
- Preflight, application, verification, and recovery facts.
- Changed, unchanged, reverted, and residual targets.
- Findings, semantic status, and useful next actions.

For an attention result, including a dry run with safely established planned
changes and a non-blocking finding, the structured status value is `attention`;
only human presentation uses `requires attention`.

The exact field names, schema versioning, compatibility rules, and numeric exit
mapping are defined by the accepted [Open Forge CLI Architecture](../../architecture.md#result-json-coordinates-and-process-status).
The [Technical Design](technical-design.md#json-and-presentation) describes
the source-generated serialization path without changing that shared authority.

## Semantic Results

| Result        | Meaning                                                                                                                                                                                                                                                                                |
| ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | Dry-run formed and preflighted the complete plan with no non-blocking finding, or application and final verification completed, including a verified no-op.                                                                                                                            |
| `attention`   | Application and verification completed, or a dry run safely established its complete plan and preconditions, but a non-blocking finding remains. A dry run exposes the complete plan and exact bounded diffs and states that no files changed. Human output says `requires attention`. |
| `incomplete`  | Safe inspection facts are available, but complete target discovery or projection coverage could not finish. No mutation begins, and human facts and findings remain one result.                                                                                                        |
| `invalid`     | Command input, a flag value, or a source reference does not follow the accepted interface.                                                                                                                                                                                             |
| `blocked`     | A valid request cannot establish or apply one safe complete plan. No mutation begins.                                                                                                                                                                                                  |
| `failed`      | Application, verification, or recovery failed to complete the selected operation.                                                                                                                                                                                                      |
| `interrupted` | The caller cancelled or interrupted the operation before completion and no residual recovery failure remains.                                                                                                                                                                          |

Changes and verified no-ops are ordinary `complete` results. They do not
require `attention` merely because bytes changed or no effect was needed.

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

A path escape, unsafe physical identity, or portable target collision is
blocked.

A missing Loader blocks the operand-free form.

A selected source without an exposing region is blocked.

Missing or invalid required routing metadata blocks the complete plan.

Missing, duplicate, misplaced, reversed, nested, or ambiguous generated
markers block the complete plan.

A dirty planned target path blocks unless `--skip-git-check` applies.

An unavailable or colliding required backup blocks before the first write.

A source or destination change detected after planning but before the first
write blocks the plan. A change detected after application begins fails the
operation before stale intent can write that target and follows the accepted
recovery rules.

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
| A recognized `_index.md` entrypoint reached through more than one traversal path                                                                      | The physical entrypoint is processed exactly once by physical identity.                                                                                                                                 |
| A selected entrypoint has no direct routed children                                                                                                   | Its expected generated body is `- none - No entries - #Empty`.                                                                                                                                          |
| A valid target has stale generated lines                                                                                                              | Application replaces only the bounded generated interior.                                                                                                                                               |
| `open-forge index --dry-run` with changes and no non-blocking finding                                                                                 | The result is `complete`; human output shows exact bounded diffs and says no files changed, and structured output carries equivalent bounded before-and-after evidence.                                 |
| `open-forge index --dry-run --dry-run --skip-git-check --skip-git-check` or an application with repeated `--skip-git-check` and dirty planned targets | Repeated Boolean occurrences are accepted with no additional effect. The relevant-path cleanliness check is bypassed only for application, while all other safety and recovery requirements remain.     |
| `open-forge index --workspace ../another-workspace`                                                                                                   | The exact supplied workspace is used; no parent or Git-root discovery occurs.                                                                                                                           |
| `open-forge index --json`                                                                                                                             | One complete structured result for every semantic status is written to stdout, preserving the typed status. Separate bounded diagnostics use stderr; ordinary human text is not mixed into JSON stdout. |
| `open-forge index --verbose`                                                                                                                          | Bounded diagnostics are added without changing operation behavior or status.                                                                                                                            |
| `open-forge index --help`                                                                                                                             | Help for `index` is shown without workspace resolution or domain execution.                                                                                                                             |
| `open-forge index --version`                                                                                                                          | The distributed CLI version is shown without workspace resolution or a domain operation.                                                                                                                |
| A valid unchanged target set after a successful prior application                                                                                     | The result is `complete`, reports a verified no-op, and performs no write or Git cleanliness check.                                                                                                     |
| A valid empty target projection                                                                                                                       | The result can be a complete update or verified no-op with the exact empty body, depending on current bytes.                                                                                            |
| A dry run with safely established planned changes and a non-blocking finding                                                                          | The semantic result is `attention`; the complete plan and exact bounded diffs remain visible, human output says `requires attention`, and it says no files changed.                                     |
| Safe facts exist but complete discovery or projection coverage cannot finish                                                                          | The result is `incomplete`, no mutation begins, and safe facts remain together with their findings in the primary human result.                                                                         |
| An unknown source, invalid flag value, or conflicting terminal input                                                                                  | The result is `invalid`; the primary human error is on stderr and identifies the useful correction when one exists.                                                                                     |
| Missing Loader, invalid metadata, unsafe boundary, dirty target without skip, or another unsafe complete-plan condition                               | The result is `blocked`, no mutation begins, and the primary human error is on stderr.                                                                                                                  |
| Application, verification, or recovery cannot complete                                                                                                | The result is `failed` or `interrupted` according to the semantic-result definitions, the primary human error is on stderr, and residual recovery state is reported when present.                       |
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
- Recognized `_index.md` entrypoints processed once by physical identity.
- Human `up to date` and `requires attention` wording on the assigned result
  stream without default preflight jargon.
- Human and structured output from the same typed result, with primary human
  result and error streams, one JSON document on stdout, and bounded diagnostics
  on stderr.
- Complete, attention, incomplete, invalid, blocked, failed, and interrupted
  results, including dry-run attention formation.
- Exact human examples, structured bounded-change evidence, accepted idempotent
  Boolean repetition, no-op output, and the no-files-changed dry-run promise.

## Accepted Public Output And Result Rules

Primary human rendering for `complete`, `attention`, and `incomplete` results
goes to stdout. Each primary human typed result stays together on stdout,
including safe incomplete facts and their findings.

Primary human error rendering for `invalid`, `blocked`, `failed`, and
`interrupted` results goes to stderr. Each primary human typed error result stays
together on stderr.

A dry run with safely established planned changes and a non-blocking finding
returns `attention`. It exposes the complete plan and exact bounded diffs, states
that no files changed, and human output says `requires attention`. It does not
claim on-disk verification of bytes that were not written.

The shared [Global CLI Flags](../shared/global-flags/interface.md) contract
remains authoritative for `--json`: it renders one complete structured result
document to stdout for every semantic status. Separate bounded diagnostics
remain on stderr, and the accepted human stream rules above do not mix ordinary
human text into JSON stdout.
