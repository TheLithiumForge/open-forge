---
open-forge:
  description: Accepted current public interface for moving one eligible unmanaged routed leaf or complete routed category
  responsibility: Define what `route move` accepts, changes, reports, rejects, and leaves unchanged
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Move, Interface, Mutation, Reference, Safety, CurrentTruth]
---

# route move Interface Contract

## Status And Authority

This is the accepted current Crystallized authority for the caller-visible
Interface Contract for `route move`. The command does not ship yet. Its
implementation and complete executable proof are squash-integrated by the
commit containing this record; replacement-CLI delivery remains pending. It is
one explicit mutation operation, not a generic batch or apply surface.

The sibling [Behavior Contract](behavior.md) defines the deterministic,
technology-neutral resolution, planning, effects, safety, recovery, and
conformance behind this public surface. The shared [Global CLI Flags](../../shared/global-flags/interface.md)
and [CLI Source References](../../shared/source-references/interface.md)
contracts retain their shared meanings.

The [Routed Markdown Representation](../../../../framework/markdown/routes.md),
[Routing Model](../../../../framework/routing/model.md),
[Routing Paths And Identity](../../../../framework/routing/paths.md),
[Overwrite Customization](../../../../framework/routing/overwrites.md),
and [Markdown Compatibility Boundary](../../../../framework/markdown/compatibility.md)
sources remain authoritative for the Framework meaning consumed by this
operation. The [Index Interface Contract](../../index-candidate/interface.md)
remains authoritative for generated-navigation projection and bounded generated
regions.

The [Shared Result Coordinates](../../shared/result-coordinates/interface.md)
define the accepted shared structured schema and process-status mapping. The [CLI
Architecture](../../../architecture.md) defines parser and serializer roles,
filesystem and physical-identity, workspace-lock and recovery boundaries, test
evidence, runtime, Native AOT, and source-layout choices. This command
contract adds no competing implementation choice and preserves the observable
boundaries below.

## Purpose And Operation Boundary

`route move` moves one eligible ordinary unmanaged logical leaf or one eligible
ordinary unmanaged category to one exact destination. It preserves the selected
subject's authored meaning and complete relative layout, updates every affected
supported local Markdown reference whose existing destination would no longer
resolve to the same intended target after the move, and projects affected
generated navigation in the same operation.

A leaf is one ordinary routed Markdown base source and its valid adjacent
overwrite companion, when present. A category is one recognized entrypoint and
the complete physically contained folder tree below it. A category contains all
physically contained regular files and directories, including descendant
entrypoints, routed or unrouted Markdown, native or binary resources, ordinary
support files, and overwrite companions. Generated `Entries` interiors are
derived projections rather than authored authority.

The command performs one operation for one subject and one destination. A
category plan may enumerate many contained logical sources and resources, but it
has one atomic plan, one recovery boundary, and one final verification boundary.
It is never a series of independently committed leaf moves and never exposes a
generic batch or saved-plan surface.

The command is stateless and deterministic for unchanged workspace bytes and
explicit input. A successful move changes the old source identity into a new
destination identity. Repeating the same move with the consumed old source is
not a verified no-op: it produces the non-mutating exact source-not-found
`invalid` result. The operation creates no receipt, tombstone, journal, saved plan, or
history used to manufacture provenance.

## Syntax

```text
open-forge route move <source-reference> <destination-target>
  [--dry-run]
  [global flags]
```

The command path selects the move operation. It requires exactly one source
reference and exactly one destination target. The shared [Global CLI Flags](../../shared/global-flags/interface.md)
contract defines `--workspace`, `--json`, `--view`, `--verbose`, `--help`, and
`--version`; all six apply under that contract.

Omitting the source or destination still selects `route move` and produces its
typed `invalid` result without workspace mutation. Supplying a third positional
operand is different: the shared shell parser rejects that unmatched input as
`cli.parser.invalid` before Route Move binding, workspace selection, or domain
execution. That shell-owned
failure returns exit `4`, writes no stdout, writes a parser diagnostic to stderr,
and produces no Route Move result, status, finding, or `Next:` envelope. The
diagnostic wording is not part of this command contract.

`--dry-run` is the only preview spelling. The command does not inspect or report
repository state. It does not select a subject, add authority,
or change the operation.

The command has no `--force`, `--automatic`, `--yes`, `--apply`, `--all`,
`--batch`, `--recursive`, root move mode, alias, saved plan, receipt, or generic
mutation dispatcher. It has no operand-free wizard because the primary subject
cannot be safely selected by enumeration. JSON and other non-interactive use
therefore use the same explicit two-operand request and never prompt.

## Operands And Repetition

| Operand                | Role                                                                        | Accepted value                                                                                       | Omission and repetition                   |
| ---------------------- | --------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- | ----------------------------------------- |
| `<source-reference>`   | Select one existing eligible logical leaf or recognized category entrypoint | One shared automatic source ID or exact `.agents/...` path, narrowed by this command's subject rules | Required; exactly one source subject      |
| `<destination-target>` | Select one exact new leaf target or category entrypoint target              | One command-specific exact workspace-contained ordinary routed file target or entrypoint path        | Required; exactly one destination subject |

The source operand follows the shared source-reference classification: `.agents/`
and `./.agents/` prefixes request exact paths, and every other value is an
automatic source ID. The CLI does not guess between the two forms. An ID that
does not resolve to exactly one eligible subject is not repaired by ranking,
basename matching, or route proximity.

The destination target is not a source reference. It is an exact workspace-
relative target under `.agents`, with the file or entrypoint shape required by
the selected subject kind. It does not use a directory operand, a fuzzy target,
an automatic destination inferred from a name, or a target outside the selected
workspace. The parser and physical-identity realization follow the accepted [CLI
Architecture](../../../architecture.md); the exact subject and destination
meaning remain this contract's public boundary.

`--dry-run` is a Boolean write-policy flag. Repeating it is accepted and
idempotent. Repetition does not multiply preview, consent, recovery, or mutation
authority. Shared global-flag
repetition, ordering, terminal behavior, and composition remain defined only by
the shared contract.

Ordinary `--help` and `--version` invocations retain their successful shared
terminal short-circuits. Unmatched positional input, unknown symbols, and other
shell-invalid input remain terminal failures under the shared parser policy;
terminal flags do not turn that input into a Route Move result.

## Source Subject Selection

The source reference must resolve to one of these two subject forms:

1. One ordinary routed Markdown base source, optionally with its valid adjacent
   overwrite companion. This is the leaf form.
2. One recognized entrypoint that identifies one category folder. The source
   reference must resolve to that entrypoint itself, not merely to an arbitrary
   directory. This is the category form.

The Loader and the `.agents` workspace root are never move subjects. A
Loader-exposed category may be eligible; when it is moved, the affected Loader
projection is part of the same plan. A recognized compatibility entrypoint can
be selected only when the route structure is otherwise unambiguous, and its
actual filename is preserved unless the exact destination contract requires a
different accepted entrypoint path.

An automatic-ID collision is blocked in non-interactive and JSON use and does
not select a candidate. An exact path can identify the intended physical source,
but it cannot make an ambiguous route, overwrite pair, or category inventory
safe.

## Eligible Leaf

An eligible leaf is one ordinary routed Markdown logical source below an existing
valid entrypoint. Its base file and valid adjacent overwrite companion are one
logical subject and move together, preserving base-first layering and adjacency.

The selected leaf is not eligible when it is any of the following:

- an entrypoint, the Loader, or the `.agents` root;
- a routed native source such as `SKILL.md`, a non-Markdown resource, or an
  unsupported source kind;
- an orphan or ambiguous overwrite companion;
- a source with any trusted Framework or Extension lifecycle ownership claim;
- a source whose identity, route, containment, collision, or recovery boundary
  cannot be established completely; or
- a source that would create a destination collision, alias, self-move, or
  unsafe containment relationship.

The command does not adopt an existing file, release lifecycle ownership, repair
route meaning, or reinterpret a generated entry as an authored source.

## Eligible Category

An eligible category is selected through one recognized entrypoint source
reference. Its subject is the complete physically contained folder tree rooted at
that entrypoint, including:

- the root entrypoint and its overwrite companion, when present;
- every descendant entrypoint and routed or unrouted Markdown source;
- every native, binary, ordinary, or support resource regardless of whether it
  is independently routable;
- every overwrite companion belonging to a contained base; and
- every other physically contained regular file and directory.

Derived generated regions are projected from the intended post-move topology.
They are not treated as authored source authority or as independent category
items.

The category is eligible only when every contained item has complete safe
containment, physical identity, ownership, lifecycle, collision, and recovery
classification. An item that is neither a regular file nor a directory, or that
is missing, malformed, conflicting, stale, unreadable, externally resolving,
ambiguous, incomplete, or unsafe, prevents the category operation as a whole. An
unfamiliar file extension does not exclude an otherwise safe regular file from
the category. The command does not skip an item, move a safe subset, or
reinterpret a folder as a batch of leaf commands.

An entrypoint nested inside the selected category is part of the category, not a
second operation. A category's internal relative layout is preserved. The
operation does not flatten descendants, reorder them, merge independent logical
sources, or create an alternate route root. The Loader itself and any item whose
physical identity cannot remain contained are outside this boundary.

## Positive Unmanaged Proof

Before a leaf or category can enter a mutation plan, the command must successfully
load a complete trusted lifecycle-ownership inventory for the selected workspace.
The inventory includes the Framework baseline and every applicable Extension
receipt or manager claim. It must establish that no trusted lifecycle source
claims any selected logical source or resource.

For a category, this proof covers every selected item, not only the root
entrypoint. A single claim, ownership conflict, stale claim, or unresolved item
blocks the complete category plan.

The following do not prove unmanaged status by themselves:

- failing to find one receipt or looking in one lifecycle source;
- a path, route placement, tag, generated entry, or familiar folder name;
- matching bytes, matching fingerprints, or an apparently initial file; or
- a previous command result, recommendation, or absence of a marker.

Missing, malformed, conflicting, stale, or incomplete lifecycle-ownership
inventory is a blocking authority condition. The command does not adopt content,
release ownership, repair lifecycle records, migrate receipts, or continue on a
partial inventory.

## Destination Target

The destination kind must match the selected subject:

- A leaf destination is one exact ordinary routed Markdown file target below an
  existing valid parent route. Its base path must be unoccupied, and an existing
  file, directory, entrypoint, native source, overwrite companion, alias, or
  unsupported resource at that target blocks the move.
- A category destination is one exact destination entrypoint path inside a new
  category folder whose parent is an existing valid route. For example, with
  existing parent route `.agents/archive/_archive.md`, destination
  `.agents/archive/guides/_guides.md` identifies new category folder
  `.agents/archive/guides/` and its root entrypoint. Every descendant keeps its
  relative layout below that root. The destination root and its complete
  contained layout must be unoccupied except for paths created by this one move
  plan.

The existing parent route must already be valid and have exactly one recognized
entrypoint. The command does not initialize an implicit parent chain, create a
missing parent route, or infer a destination from a familiar slug. A category
root folder may be created as the direct move effect, but no missing ancestor
route is initialized as a side effect.

The command rejects:

- a self-move or a destination that is already the selected source identity;
- a destination inside the selected category source tree;
- lexical or physical aliases, path escapes, or unsafe containment;
- an existing destination, orphan companion, route-identity collision, or
  overwrite conflict;
- a destination whose source kind does not match the selected leaf or category;
  and
- a destination whose parent route or required generated boundary is missing,
  malformed, duplicate, nested, reversed, or otherwise ambiguous.

The operation never silently replaces a destination. It does not normalize a
compatibility filename, merge an overwrite into a base, or use current generated
lines as proof that the destination is free.

## Final-Leaf Safety Boundary

Every file leaf that `route move` would change or remove, including the selected
source, destination, authored-reference, and generated-navigation leaves, has a
caller-visible no-follow observation of its immediate final filesystem component.
A filesystem link, reparse point, or special final leaf, including a relative
file link that is an exact Workspace Library projection, is separately owned and
unsafe for ordinary Route Move mutation. The command keeps its existing
`blocked` target-safety result, identifies the affected path, and performs no
effect.

This boundary does not consult `.agents/open-forge.libraries.json`. A missing,
malformed, stale, or otherwise unreadable Library record neither makes the
final leaf ordinary nor grants Route Move mutation authority. Route Move never
resolves a final filesystem leaf and then deletes or moves its physical source
target. The guard concerns each final component addressed by the move plan;
ordinary directory-ancestry rules remain defined by the filesystem contract.

## Complete Reference Pass

Move performs one complete physically contained pass over the supported Markdown
catalogue in the selected workspace, both inside and outside `.agents`. The
catalogue includes every supported Markdown source and physical layer that can
contain an authored local reference, including base files and overwrite
companions. The command does not silently narrow this pass to routed files or to
the selected category.

Generated `Entries` interiors are derived navigation. The pass does not treat
their links as authored reference authority or rewrite them as ordinary prose;
affected generated regions are projected separately.

For every exact supported, resolvable local authored reference, the operation
rewrites the destination only when the existing destination would no longer
resolve to the same intended target after the move. The complete rewrite
boundary includes:

- references from outside the moved subject to the moved leaf, category member,
  or resource;
- references from the moved subject to a target outside the moved subject, when
  the source location changes the authored destination needed to reach that
  target; and
- references within the moved subject when the source or target relative layout
  changes and the existing destination would no longer identify the same target.

References between moved items that remain valid under the preserved relative
layout are not rewritten. The operation does not rewrite external URLs, semantic
or fuzzy matches, generated lines as authored prose, or unsupported reference
forms.

Each rewrite preserves the authored label, fragment, valid encoding, surrounding
prose, and unrelated bytes. A local target outside `.agents` remains eligible
when it is a supported target physically contained by the selected workspace.
The command does not turn such a target into an Open Forge source ID.

The catalogue must be completely enumerated and inspected. If a supported
Markdown source cannot be inspected completely, or a potentially applicable
reference is unsupported or ambiguous, the result is `incomplete` and no write
begins. An unsafe path, physical alias, or containment boundary is `blocked`.
The command never silently leaves a supported reference stale.

## Generated Navigation

Move projects generated navigation against the hypothetical post-move workspace
through the accepted [Index Behavior Contract](../../index-candidate/behavior.md).
It includes the old and new exposing parent projections and includes the Loader
projection when the moved subject is a Loader-exposed category. Any other
generated region is included only when its direct-child projection changes under
the moved topology.

The projection uses current authored topology and metadata, not current generated
lines, to derive entries. It preserves each valid marker pair and every byte
outside the bounded generated interior. It never starts a hidden `index`
subprocess. A generated boundary or required sibling projection that cannot be
established safely prevents the complete plan.

Generated effects and authored reference rewrites are part of the same move plan.
The result reports them together, and final verification checks the complete
post-move projection rather than treating navigation as a later cleanup step.

## Planning And Effects

Move follows one complete typed mutation flow:

```text
validated source and destination
  -> complete leaf or category inventory
  -> trusted unmanaged proof
  -> complete reference catalogue and intended rewrites
  -> post-move route and generated projection
  -> one ordered complete mutation plan
  -> preflight
  -> dry-run or application
  -> verification and retained partial-state reporting
  -> one typed result
```

The plan includes every selected source/resource path, destination path,
reference-source replacement, affected generated region, expected state,
verification condition, and recovery requirement. Compatible changes to one
physical file coalesce into one complete-file effect. A category plan remains one
operation even when it contains many file effects; it is not independently
committed per descendant.

One invalid, blocked, incomplete, conflicting, or unsafe required fact prevents
all effects. There is no partial, best-effort, silently narrowed, or
independently committed leaf application.

## Dry Run And Apply

`--dry-run` uses the same request, current facts, trusted ownership inventory,
complete category inventory, reference catalogue, intended rewrites, generated
projection, plan, expected-state checks, and preflight as application. It shows
every moved, created, removed, rewritten, detached, and generated effect needed
to review the complete operation, then writes nothing. Planned changes alone do
not create `attention`.

When the final-leaf safety boundary fails, dry-run and application retain the
same existing `blocked` target-safety result and produce no effect.

Omitting `--dry-run` selects application. The command path and the exact source
and destination subjects are sufficient consent in human, JSON, and other
non-interactive use. The command does not prompt for a second confirmation and
does not accept `--yes`.

The explicit consent covers only the selected leaf or complete category, its
exact destination, supported reference rewrites, and generated projections in
the complete plan. It does not grant ownership, lifecycle, collision,
containment, marker-repair, or recovery bypass authority.

For an actual application, the complete plan checks every existing path it may
change or remove through ordinary workspace facts. It does not inspect or report
repository state.

When the operation has one or more existing-target effects (`Replace`,
`ReplaceGeneratedRegion`, or `Delete`),
orchestration uses only
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and its application-owned
`OpenForge/recovery/v1` subtree. There is no temporary-directory, repository,
`HOME`, or custom-platform fallback; unavailable storage is a
pre-effect `incomplete` result. The complete move operation prepares exactly
one immutable ZIP bundle outside the workspace. Its
source-generated schema-v1 `manifest.json` and streamed ordinal payload
entries record command/operation/workspace identity, ordered relative targets,
change kinds, exact prior bytes/lengths/hashes, and intended final absence or
length/hash. `Create` effects and semantic/byte no-ops have no entry. A CreateNew
draft is closed and reopened for semantic manifest, exact ordered entry,
length, hash, and payload-byte validation, moved within the same directory to
its deterministic final name, and reopened and verified. Only the valid final
ZIP forms the opaque `RecoveryBundlePreparation`; the draft remains
`Incomplete`. `FileChangeApplier` requires the matching preparation for every
existing-target effect and performs one final effect per target.
All preparation completes before the first target effect; unknown, malformed,
mismatched, or colliding bundles block.

After all effects and final verification succeed, delete only the positively
recognized bundle created by this operation. `Deleted`/`Removed` permits normal
completion. `Failed`/positively observed `Retained` keeps target effects
successful and produces `attention`, the exact
residual path, and cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion
result provides one. Before post-verification deletion begins, a handled
application, verification, or cancellation outcome stops new effects and reports
the actual residual draft or final path; a valid final remains when preparation
completed. A closed final ZIP may remain after abrupt process termination, without an
executable crash or power-loss guarantee. Recovery provenance does not classify
current target state. Cleanup owns exact named final and draft deletion under its
separate lease-bound contract.

## Human Output

Human output comes from one typed result. The default expanded view includes the
workspace, selection method, selected subject kind, source and destination
identity, complete effect summary, affected paths, reference coverage and
rewrites, generated projections, verification, bundle provenance, and retained
partial-state facts, and semantic status. Compact view retains the identity,
leaf/category kind, mode, status,
completeness and safety, every affected path, every reference effect, every
generated effect, and at most one required `Next:` action.

Dry-run compact and expanded output still shows every exact planned effect or
bounded diff. A successful result does not name internal planning stages. The
primary human result for `complete`, `attention`, and `incomplete` goes to
stdout. The primary human result for `invalid`, `blocked`, `failed`, and
`interrupted` goes to stderr. Each primary result stays together on its assigned
stream. Bounded diagnostics use stderr.

### Successful leaf move

```text
The routed leaf was moved.
Workspace: <workspace-path>
Selected by: current directory
Source: docs/old-guide
Destination: .agents/docs/new-guide.md
Rewritten references: 2
Updated generated regions: 2
```

### Successful category move

```text
The routed category was moved.
Category: .agents/guides/_guides.md
Destination: .agents/archive/guides/_guides.md
Moved items: 6
Rewritten references: 4
Updated generated regions: 3
```

### Dry run

```text
The routed category would be moved.
Category: .agents/guides/_guides.md
Destination: .agents/archive/guides/_guides.md

<complete moved-path, reference, and generated-region effects>

No files changed (--dry-run).
```

The exact examples use illustrative paths. A complete result has no required
`Next:` action. Incomplete, invalid, and blocked results name the direct
correction when it is known. Failed and interrupted results identify retained
bundle/partial-state or retry guidance without inventing provenance.

## Structured Output

`--json` emits one complete structured result to stdout for every semantic status
from the same typed result used by human output. It never prompts and never
reruns resolution, planning, application, verification, or retained-state
reporting. Human text
is not mixed into JSON stdout; bounded diagnostics use stderr.

The structured result exposes the concrete command result under the exact shared
schema defined by the [Shared Result
Coordinates](../../shared/result-coordinates/interface.md):

- workspace and selection method;
- requested and resolved source and destination identities and canonical paths;
- selected subject kind, logical layers, and complete category item inventory;
- trusted Framework and Extension ownership-evidence state;
- reference catalogue coverage, every rewritten occurrence, and its old and new
  target meaning;
- generated-region selection, projection, and bounded effect evidence;
- dry-run or application mode, completeness, safety, recovery-bundle facts;
- expected-state, changed, unchanged, and verified effect facts, plus any actual
  residual draft or final recovery path, without classifying current target state;
- application, verification, bundle provenance, and retained partial-state facts; and
- semantic status and at most one required `Next:` action.

The structured result keeps the exact source and destination subjects visible. It
does not hide a category behind a count or omit a reference effect merely because
the human compact view is selected.

## Semantic Results

| Result        | Meaning                                                                                                                                                                                                                                                                                                         |
| ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `complete`    | A complete safe dry-run plan was established, or application and final verification completed, for a leaf or category move. This includes a valid logical base/overwrite move and every complete reference and generated effect.                                                                                |
| `attention`   | Post-verification recovery deletion returns `Failed` with positively observed disposition `Retained`; target effects remain successful with the exact residual path and cleanup guidance. Planned changes and reference rewrites do not create it.                                                              |
| `incomplete`  | Safe identity and facts exist, but the complete supported-Markdown catalogue, reference pass, category inventory, or another required coverage boundary cannot be enumerated or inspected. No write begins.                                                                                                     |
| `invalid`     | A shell-accepted Route Move invocation omits its required source or destination, or its source kind, destination shape, flag use, or exact source reference does not follow this interface. A repeated move using the consumed old source is the exact source-not-found `invalid` result, not a verified no-op. |
| `blocked`     | A valid request cannot establish one safe complete move because ownership, lifecycle, route, identity, containment, collision, destination, generated boundary, expected state, or recovery is unsafe or ambiguous. No write begins.                                                                            |
| `failed`      | An unexpected application or verification failure occurs after a persistent effect begins, or recovery deletion returns `Failed`/`Unknown`; `Failed`/positively observed `Retained` recovery is the distinct `attention` case.                                                                                  |
| `interrupted` | The caller cancels before completion; an unexpected application or verification failure remains `failed`.                                                                                                                                                                                                       |

For ordinary conditions, status precedence is `blocked` > `incomplete` >
`attention` > `complete`. Invalid input stops before operation resolution.
Failed and interrupted preserve their event meanings. The shared numeric
process-status mapping is defined by the [Shared Result
Coordinates](../../shared/result-coordinates/interface.md).

## Errors And Boundaries

The command rejects or blocks:

- a missing source or destination, which produces the typed Route Move
  `invalid` result;
- a third positional operand, which the shared shell rejects as
  `cli.parser.invalid` before Route Move result formation;
- an unknown, missing, ambiguous, Loader, root, entrypoint, native, resource,
  orphan, or otherwise ineligible leaf subject;
- a category reference that is not exactly one recognized entrypoint or whose
  physically contained inventory is incomplete or unsafe;
- missing, malformed, stale, conflicting, or incomplete trusted ownership and
  lifecycle evidence;
- a destination that is not the exact matching leaf or category target, whose
  parent route is missing or invalid, or whose path is occupied or aliased;
- a self-move, destination-inside-source request, overwrite conflict, route
  collision, unsafe containment, or implicit parent initialization;
- incomplete supported-Markdown enumeration or inspection;
- an unsupported or ambiguous potentially applicable move reference;
- an invalid or ambiguous generated boundary or required Index projection;
- a filesystem link or reparse point at any planned final file leaf, including
  an exact Workspace Library projection with or without a valid Library record;
  this is the existing `blocked` target-safety result and prevents every effect;
- unavailable or unsafe recovery-bundle storage (`incomplete`), an unverified
  bundle, or a bundle collision (`blocked`); or
- a changed expected source, destination, reference, generated region, ownership
  fact, or category item before application.

Every ordinary error names `route move`, the affected subject or path, the direct
cause, and one useful next action when one exists. The command does not diagnose
authoring quality, infer semantic intent, or propose a different destination.
Shell parser diagnostics are outside that Route Move result rule and do not
carry Route Move status, finding, or next-action meaning.

## Scenarios

Move one ordinary leaf to an exact target under an existing route:

```text
open-forge route move \
  ".agents/docs/old-guide.md" \
  ".agents/docs/new-guide.md"
```

Move one recognized category while retaining its descendant layout:

```text
open-forge route move \
  ".agents/guides/_guides.md" \
  ".agents/archive/guides/_guides.md"
```

Preview a leaf move and inspect all effects as structured output:

```text
open-forge route move \
  docs/old-guide \
  ".agents/docs/new-guide.md" \
  --dry-run \
  --json
```

The first operand in the last example is an automatic source ID. It is valid
only when it resolves to one eligible logical leaf. An exact path is required
when the ID is colliding or the category/leaf shape is otherwise ambiguous.

## Non-Goals

`route move` does not:

- move or remove the Loader or `.agents` workspace root;
- accept a generic directory operand, batch operands, repeated mutation
  requests, or independently committed descendant moves;
- move a lifecycle-managed, generated-only, native-only, unsupported, or
  ambiguous subject;
- initialize missing parent routes or invent route, metadata, ownership, or
  lifecycle meaning;
- adopt, release, migrate, or repair Framework or Extension lifecycle state;
- overwrite, merge, or silently delete a destination or overwrite companion;
- rewrite external URLs, unsupported or ambiguous reference forms, or unrelated
  authored prose;
- resolve a final filesystem link or reparse point and then delete or move its
  physical source target, write through a Workspace Library projection, or adopt
  or manage a Library record;
- treat generated `Entries` as authored authority or run a hidden `index` command;
- create a receipt, tombstone, journal, saved plan, session, or automatic
  recovery history;
- create a Git commit; or
- redefine the shared libraries, parser boundary, physical identity, lock,
  concurrency, test, Native AOT, or C# source-layout choices accepted by the [CLI
  Architecture](../../../architecture.md), or the exact recovery-bundle mechanics
  accepted by the [Mutation And Recovery Technical
  Design](../../../technical-designs/mutation-and-recovery.md).

Use `route update` for an authored field patch, `index` for standalone generated
navigation, `references` for read-only direct reference facts, and `doctor` for
diagnosis. This move contract owns only the exact structural move described here.

## Verification Requirements

Gate 5 executable proof must cover:

- exact source-ID and exact-path resolution, quoting, spaces, Unicode, collision,
  containment, and base/overwrite identity;
- one eligible ordinary leaf, one eligible recognized category, and rejection of
  Loader, workspace-root, entrypoint-as-leaf, native, unsupported, orphan, and
  ambiguous subjects;
- complete category physical inventory, relative-layout preservation, descendant
  classification, and all-or-nothing planning;
- Final filesystem link and reparse-point leaves, including exact Workspace
  Library projections with and without a valid Library record, are reported as
  unsafe and block before effects; Route Move never resolves then deletes or
  moves their physical source targets.
- complete trusted Framework and Extension lifecycle-ownership proof, including
  missing, malformed, conflicting, stale, incomplete, and source-claim cases;
- exact leaf and category destination shapes, existing-parent requirement,
  self-move, destination-inside-source, alias, collision, overwrite, and
  implicit-initialization rejection;
- complete supported Markdown catalogue coverage inside and outside `.agents`,
  base/overwrite layers, exact resolvable rewrites, internal-link preservation,
  label/fragment/encoding/byte preservation, and external URL preservation;
- incomplete unsupported or ambiguous potentially applicable move references and
  blocked unsafe identity or containment boundaries;
- old and new parent generated projections, Loader projection when applicable,
  generated-boundary preservation, and no hidden `index` invocation;
- one complete plan, no partial category application, exact dry-run parity, no
  persistent dry-run effects, and explicit-subject consent in every mode;
- recovery-bundle storage/readiness and collision handling, expected-state
  revalidation, all-effects verification, retained partial state without
  restoration, residual preservation, and fresh-plan rerun;
- `complete`, reserved `attention`, `incomplete`, `invalid`, `blocked`, `failed`,
  and `interrupted` results, including consumed-source `invalid` repetition;
- missing source and destination as typed Route Move `invalid` results, plus a
  third positional operand as a write-free `cli.parser.invalid` shell failure
  with exit `4`, empty stdout, nonempty stderr, and no Route Move result
  envelope;
- human stream allocation, compact retention, every planned dry-run effect,
  structured JSON parity, and no mixed human text in JSON stdout; and
- one typed result consumed by both human and structured renderers without
  rerunning the operation.

The proof must exercise the accepted CLI Architecture boundaries rather than
relying on source-level or managed-build claims. It must include the real
filesystem, workspace lock, recovery, Native AOT, and package/process evidence
required by that Architecture.

## Related Current Sources

- [route move Command Contract Set](_move.md)
- [route move Behavior Contract](behavior.md)
- [Route group entrypoint](../_route.md)
- [Route Update Interface Contract](../update/interface.md)
- [Route Create Interface Contract](../create/interface.md)
- [Route Init Interface Contract](../init/interface.md)
- [Route Inspect Interface Contract](../inspect/interface.md)
- [Route List Interface Contract](../list/interface.md)
- [CLI Architecture](../../../architecture.md)
- [Index Interface Contract](../../index-candidate/interface.md)
- [Index Behavior Contract](../../index-candidate/behavior.md)
- [References Interface Contract](../../references-candidate/interface.md)
- [Doctor Interface Contract](../../doctor/interface.md)
- [Global CLI Flags](../../shared/global-flags/interface.md)
- [CLI Source References](../../shared/source-references/interface.md)
- [Historical CLI Decision Agenda](../../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [CLI Command Contract Set — Interface Contract](../../../command-contract-set.md#interface-contract)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
- [Routing Model](../../../../framework/routing/model.md)
- [Routing Paths And Identity](../../../../framework/routing/paths.md)
- [Overwrite Customization](../../../../framework/routing/overwrites.md)
- [Routed Markdown Representation](../../../../framework/markdown/routes.md)
- [Markdown Compatibility Boundary](../../../../framework/markdown/compatibility.md)
