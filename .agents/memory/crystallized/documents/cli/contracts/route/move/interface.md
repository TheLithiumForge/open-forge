---
open-forge:
  description: Accepted current public interface for moving one eligible unmanaged routed leaf or complete routed category
  responsibility: Define what `route move` accepts, changes, reports, rejects, and leaves unchanged
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Move, Interface, Mutation, Reference, Safety, CurrentTruth]
---

# route move Interface Contract

## Status And Authority

This is the accepted current Crystallized authority for the caller-visible
Interface Contract for `route move`. The command is implemented in the merged
CLI. Its implementation and complete executable proof are squash-integrated by
the commit containing this record. It is
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
`invalid-input` result. The operation creates no receipt, tombstone, journal, saved plan, or
history used to manufacture provenance.

## Syntax

```text
open-forge route move <source-reference> <destination-target>
  [--dry-run]
  [global flags]
```

The command path selects the move operation. It requires exactly one source
reference and exactly one destination target. The shared [Global CLI Flags](../../shared/global-flags/interface.md)
contract defines `--workspace <path>`, `--format <text|json>`,
`--detail <minimal|standard|full|debug>`, repeatable
`--detail-filter <error|warning|info|all>`, `--help`, and `--version`; all six
apply under that contract.

Omitting the source or destination still selects `route move` and produces its
typed `invalid-input` result without workspace mutation. Supplying a third positional
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
| `<destination-target>` | Select one exact new leaf target or category entrypoint target              | One exact filesystem target or accepted logical leaf ID for a leaf, or an exact category entrypoint path | Required; exactly one destination subject |

The source operand follows the shared source-reference classification: `.agents/`
and `./.agents/` prefixes request exact paths, and every other value is an
automatic source ID. The CLI does not guess between the two forms. An ID that
does not resolve to exactly one eligible subject is not repaired by ranking,
basename matching, or route proximity.

The destination target is not a source reference. A leaf destination may be an
exact workspace-relative filesystem path under `.agents`, preserving the
explicit path grammar, or an accepted logical leaf ID resolved by the canonical
route rules. For example, `guidance/team/moved-note` maps to
`.agents/guidance/team/moved-note.md`. A logical leaf ID has no `.agents/`
prefix or `.md` suffix, uses slash-separated route segments, and contains no
`.` or `..` segment. Category destinations remain exact filesystem
entrypoint paths; the logical leaf form never invents a category or route.
The parent route must already exist after canonicalization. The destination
does not use a directory operand, a fuzzy target, an automatic destination
inferred from a name, or a target outside the selected workspace. The parser
and physical-identity realization follow the accepted [CLI
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

The command reads Framework and Extension ownership from the forgiving
`.agents/open-forge.lock.json` reader. A complete interpretable inventory must
establish that none of the selected logical sources or resources is claimed
before a mutation plan can form. Both whole-file and region receipts protect
their hosts, including portable case aliases. One claim protects the whole
selected category; the operation never skips a claimed member.

Missing, unreadable or uninterpretable ownership does not infer unmanaged state.
It produces `ownership-unavailable` with complete informational status, no plan
or effects, and ownership shown as not-established. The summary explicitly says
that no route changed. Schema/release metadata and stale content hashes are not
gates. Actual ownership, physical safety, route and reference conflicts remain
blocking boundaries. The exact lock expectation is revalidated before and after
mutation. No legacy record is read, migrated or deleted; these commands neither
adopt current content nor release or rewrite ownership.

Path names, routing tags, generated lines, matching bytes and prior command
results cannot independently establish unmanaged status. Framework-aware Route
Init's region receipts remain positive ownership even in a user-authored host.

## Destination Target

The destination kind must match the selected subject:

- A leaf destination is one exact ordinary routed Markdown file target below an
  existing valid parent route, expressed either as an explicit filesystem path
  or as an accepted logical leaf ID that canonicalizes to that path. Its base
  path must be unoccupied, and an existing file, directory, entrypoint, native
  source, overwrite companion, alias, or unsupported resource at that target
  blocks the move.
- A category destination is one exact destination entrypoint path inside a new
  category folder whose parent is an existing valid route. For example, with
  existing parent route `.agents/archive/_archive.md`, destination
  `.agents/archive/guides/_guides.md` identifies new category folder
  `.agents/archive/guides/` and its root entrypoint. Every descendant keeps its
  relative layout below that root. The destination root and its complete
  contained layout must be unoccupied except for paths created by this one move
  plan.

The existing parent route must already be valid and have exactly one recognized
entrypoint. The command canonicalizes an accepted logical leaf ID before these
parent, occupancy, and safety checks. It does not initialize an implicit parent
chain, create a missing parent route, or infer a destination from a familiar
slug. A category root folder may be created as the direct move effect, but no
missing ancestor route is initialized as a side effect.

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

This physical-leaf boundary does not depend on Library registration state. A missing,
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
lines, to derive entries. It preserves the Entries heading and every byte
outside the heading-owned generated body. It never starts a hidden `index`
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
not create `completed-with-warnings`.

When the final-leaf safety boundary fails, dry-run and application retain the
same existing `blocked` target-safety result and produce no effect.

Omitting `--dry-run` selects application. The command path and the exact source
and destination subjects are sufficient consent in human, JSON, and other
non-interactive use. The command does not prompt for a second confirmation and
does not accept `--yes`.

The explicit consent covers only the selected leaf or complete category, its
exact destination, supported reference rewrites, and generated projections in
the complete plan. It does not grant ownership, lifecycle, collision,
containment, heading-repair, or recovery bypass authority.

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
source-generated versioned `manifest.json` and streamed ordinal payload
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
successful and produces `completed-with-warnings`, the exact
residual path, and cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion
result provides one. Before post-verification deletion begins, a handled
application, verification, or cancellation outcome stops new effects and reports
the actual residual draft or final path; a valid final remains when preparation
completed. A closed final ZIP may remain after abrupt process termination, without an
executable crash or power-loss guarantee. Recovery provenance does not classify
current target state. Cleanup owns exact named final and draft deletion under its
separate lease-bound contract.

## Human Output

Every semantic result is rendered by the shared native report. --format text
is the default. The applicable global flags are --workspace <path>, --format
<text|json>, --detail <minimal|standard|full|debug>, repeatable
--detail-filter <error|warning|info|all>, --help, and --version. The default
detail is minimal; standard adds workspace and command context, full adds all
bounded facts, and debug adds bounded diagnostics on stderr. Detail does not
change semantics, effects, counts, or status. Filters select finding severities;
all is the default filter.

Route move does not ask for confirmation and has no --automatic flag. The complete reference pass is planned before any effect.

The catalogue text by detail level is:

`minimal`, leaf with rewritten links:

```text
Moved memory/emerging/ideas/pricing/tiers to .agents/memory/emerging/ideas/pricing/tier-options.md
  Entry updated in .agents/memory/emerging/ideas/pricing/_pricing.md
  Rewrote 2 links that pointed at the old path:
    .agents/maps/_maps.md:12:3
    .agents/guidance/team.md:40:5
```

`minimal`, category:

```text
Moved the route memory/projects/alpha to .agents/memory/archived/alpha  (6 files)
  .agents/memory/projects/alpha/_alpha.md        -> .agents/memory/archived/alpha/_alpha.md
  .agents/memory/projects/alpha/plan.md          -> .agents/memory/archived/alpha/plan.md
  ...
  Entry removed from .agents/memory/projects/_projects.md
  Entry added to .agents/memory/archived/_archived.md
  Rewrote 1 link that pointed at the old paths: .agents/maps/_maps.md:20:3
```

`standard` adds `Workspace:` and per rewritten link `<old destination> ->
<new destination>`, plus the overwrite file rows when a pair moved.

`full` adds hashes per effect and the reference scan summary (`28 files
scanned`).

Results with completed, completed-with-warnings, or incomplete status use
stdout. Invalid-input, blocked, failed, and cancelled results use stderr.
A parser failure is text on stderr without a result envelope.

## Structured Output

--format json emits one schema-3 envelope on stdout for each semantic result.
The envelope has exactly these fields:

~~~text
{
  schemaVersion: 3,
  command,
  status,
  detail,
  filter,
  workspace,
  summary,
  findings,
  effects,
  counts,
  limitations,
  data,
  recovery,
  next
}
~~~

The command is exactly route move; data follows the catalogue:

| Level    | `data`                                                                                                                                                 |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| minimal  | `{ mode, subject: "file" \| "route", source { id, path }, destination { id, path }, moved: [ { from, to } ], rewrittenLinks: [ { path, location } ] }` |
| standard | + per link `from`, `to` destinations, `overwrite { from, to }`                                                                                         |
| full     | + per effect `before`, `after`, `scan { filesScanned, occurrences }`                                                                                   |

Human and JSON output are projections of one typed result. data is null only at
the parser boundary before command binding. There is no alternate JSON
projection.

For a logical leaf request, `destination.id` retains the accepted logical ID
and `destination.path` reports its canonical physical Markdown path. Human
output likewise reports the canonical destination path. An explicit filesystem
request still goes through the same canonical route and safety checks.

## Semantic Results

| Status                  | When                                                                             | Headline                                                      | Exit | Stream |
| ----------------------- | -------------------------------------------------------------------------------- | ------------------------------------------------------------- | ---: | ------ |
| completed               | leaf moved                                                                       | `Moved <id> to <new path>`                                    |    0 | stdout |
| completed               | category moved                                                                   | `Moved the route <id> to <new folder>  (<N> files)`           |    0 | stdout |
| completed (dry run)     | planned                                                                          | `Would move <id> to <new path>`                               |    0 | stdout |
| completed-with-warnings | recovery bundle retained                                                         | + family row                                                  |    2 | stdout |
| incomplete              | catalogue, reference scan or record unreadable                                   | `<id> could not be moved: <limitation>. Nothing was changed.` |    3 | stdout |
| invalid-input           | bad source or destination, self move, destination inside source, consumed source | `Cannot move <ref>: <problem>.`                               |    4 | stderr |
| blocked                 | destination exists, managed source, unsafe, lock                                 | `Cannot move <id>: <reason>.`                                 |    5 | stderr |
| failed                  | after effects                                                                    | `Route move stopped after <n> of <m> changes.`                |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                                                         | `Route move was cancelled. Nothing was changed.`              |  130 | stderr |

### Current merged behavior and open questions

The catalogue assigns self-move and destination-inside-source to invalid-input
(exit 4). The merged operation returns blocked (exit 5) for both. Maintainer
decisions remain open.

This command's identity-collision finding is error in the command catalogue but
warning in the shared finding table. The existing error severity was preserved.
Maintainer decision remains open.

The reference-unsafe catalogue requires file:line:column, but the result carries
no coordinates and renders only the path. Maintainer decision remains open.
The reference-scan-incomplete next action also includes internal wording in the
native result although the catalogue gives open-forge doctor alone. Maintainer
decision remains open.

The native path emits the ordinary move or would-move headline plus a warning
row for ownership-unavailable; the retired renderer had a dedicated no-op
headline. Maintainer decision remains open.

At the parser boundary, a missing operand can publish data: null even though
formed command data is an object. Maintainer decision remains open.

## Errors And Boundaries

The finding catalogue is:

| Code                                     | Severity | Family                      | Message                                                                                                | Next                                |
| ---------------------------------------- | -------- | --------------------------- | ------------------------------------------------------------------------------------------------------ | ----------------------------------- |
| route-move.invalid-input                 | error    | invalid-input               |                                                                                                        |                                     |
| route-move.invalid-source                | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Move/Shared/Wording/RouteMoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-move.invalid-source`).                                                    | `open-forge route list --depth=all` |
| route-move.source-not-found              | error    | unknown-source              | (also the repeated-move case)                                                                          |                                     |
| route-move.invalid-subject               | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Move/Shared/Wording/RouteMoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-move.invalid-subject`).         | none                                |
| route-move.invalid-destination           | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Move/Shared/Wording/RouteMoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-move.invalid-destination`). | `open-forge route move --help`      |
| route-move.self-move                     | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Move/Shared/Wording/RouteMoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-move.self-move`).                                                         | none                                |
| route-move.destination-inside-source     | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Move/Shared/Wording/RouteMoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-move.destination-inside-source`).                                                    | none                                |
| route-move.workspace-unavailable         | error    | workspace-unavailable       |                                                                                                        |                                     |
| route-move.workspace-unsafe              | error    | workspace-unsafe            |                                                                                                        |                                     |
| route-move.source-unsafe                 | error    | source-unsafe               |                                                                                                        |                                     |
| route-move.destination-unsafe            | error    | target-unsafe               |                                                                                                        |                                     |
| route-move.destination-occupied          | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Move/Shared/Wording/RouteMoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-move.destination-occupied`).                                                                               | choose another destination          |
| route-move.destination-parent-missing    | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Move/Shared/Wording/RouteMoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-move.destination-parent-missing`).                 | `open-forge route init <id>`        |
| route-move.category-unsafe               | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Move/Shared/Wording/RouteMoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-move.category-unsafe`).                             | none                                |
| route-move.route-ambiguous               | error    | route-ambiguous             |                                                                                                        |                                     |
| route-move.identity-collision            | error    | identity-collision          | (blocking when the new ID would collide; the prompt resolves source ambiguity)                         |                                     |
| route-move.overwrite-ambiguous           | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Move/Shared/Wording/RouteMoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-move.overwrite-ambiguous`).                                         | fix by hand                         |
| route-move.ownership-claimed             | error    | ownership-claimed           |                                                                                                        | `open-forge update` / the Extension |
| route-move.reference-unsafe              | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Move/Shared/Wording/RouteMoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-move.reference-unsafe`).                                         | fix by hand                         |
| route-move.generated-region-unsafe       | error    | generated-region-unsafe     |                                                                                                        |                                     |
| route-move.workspace-lock-unavailable    | error    | workspace-lock-unavailable  |                                                                                                        |                                     |
| route-move.target-changed                | error    | target-changed              |                                                                                                        |                                     |
| route-move.recovery-conflict             | error    | recovery-conflict           |                                                                                                        |                                     |
| route-move.ownership-unavailable         | warning  | lifecycle-unavailable       |                                                                                                        |                                     |
| route-move.inspection-incomplete         | warning  | inspection-incomplete       |                                                                                                        |                                     |
| route-move.category-inventory-incomplete | warning  | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Move/Shared/Wording/RouteMoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-move.category-inventory-incomplete`).                          | `open-forge doctor`                 |
| route-move.reference-coverage-incomplete | warning  | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Move/Shared/Wording/RouteMoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-move.reference-coverage-incomplete`).                    | `open-forge doctor`                 |
| route-move.projection-incomplete         | warning  | projection-unavailable      |                                                                                                        |                                     |
| route-move.recovery-unavailable          | warning  | recovery-unavailable        |                                                                                                        |                                     |
| route-move.recovery-artifact-retained    | warning  | recovery-artifact-retained  |                                                                                                        |                                     |
| route-move.target-changed-during-apply   | error    | target-changed-during-apply |                                                                                                        |                                     |
| route-move.write-failed                  | error    | write-failed                |                                                                                                        |                                     |
| route-move.verification-failed           | error    | verification-failed         |                                                                                                        |                                     |
| route-move.recovery-failed               | error    | recovery-failed             |                                                                                                        |                                     |
| route-move.operation-failed              | error    | operation-failed            |                                                                                                        |                                     |
| route-move.interrupted                   | error    | interrupted                 |                                                                                                        |                                     |

Findings retain code, severity, family, message, subject, cause, and next
action when available. Counts are:

`filesMoved`, `sectionsUpdated`, `linksRewritten`, `filesScanned`.

## Scenarios

`leaf-move`, `leaf-move-with-rewritten-links`, `category-move`, `dry-run`,
`destination-exists` (blocked), `destination-inside-source` (invalid),
`self-move` (invalid), `managed-source` (blocked), `source-not-found`,
`ambiguous-source-prompt`, `reference-scan-incomplete` (incomplete),
`lock-held`, `write-failed-partial`, `cancelled`.

Prompt rules from the catalogue:

Select when the source ID matches several files.

## Representative Transcripts

### completed

~~~text
Moved guidance/old guide to .agents/archive/new guide.md
Workspace: <workspace>
  Entry updated in .agents/archive/_archive.md
  Entry updated in .agents/guidance/_guidance.md
  Rewrote 5 links that pointed at the old path:
  .agents/guidance/topics/child.md:8:13
  .agents/guidance/topics/child.overwrite.md:1:35
  README.md:3:13
  README.md:5:23
  notes.md:3:11
~~~

### completed-with-warnings

~~~text
Moved guidance/old guide to .agents/guidance/new guide.md
  Warning  <recovery-bundle>  Recovery artifact retained
~~~

### incomplete

~~~text
guidance/old guide could not be moved: Some files could not be scanned for links to invalid.md, so the move was not planned. Nothing was changed.
Workspace: <workspace>
  .agents/guidance/old guide.md -> .agents/guidance/new guide.md
Next: open-forge doctor
~~~

### invalid-input

~~~text
Cannot move missing: No source has the ID missing.
Workspace: <workspace>
  files scanned: the reference scan was not completed
~~~

### blocked

~~~text
Cannot move guidance/old guide: The source and the destination are the same.
Workspace: <workspace>
  files scanned: the reference scan was not completed
~~~

### failed

~~~text
Route move stopped after 5 of 9 changes.
Workspace: <workspace>
  Error  .agents/guidance/new guide.md  Write failed
         Writing .agents/guidance/new guide.md failed. Stopped after 5 of 9 changes. Recovery data: <recovery-bundle>.
  .agents/guidance/old guide.md -> .agents/guidance/new guide.md
  Entry updated in .agents/guidance/_guidance.md
  Rewrote 5 links that pointed at the old path:
  .agents/guidance/topics/child.md:8:13
  .agents/guidance/topics/child.overwrite.md:1:35
  README.md:3:13
  README.md:5:23
  notes.md:3:11
Next: open-forge route move --detail debug
~~~

### cancelled

~~~text
Route move was cancelled. Nothing was changed.
Workspace: <workspace>
  Error  guidance/old guide  Route move was cancelled
         Route move was cancelled. Nothing was changed.
  files scanned: the reference scan was not completed
Next: open-forge route move
~~~

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

## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`route.move.help.syntax`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Route/Move/RouteMoveText.cs).

<!-- @OpenForgeTextRef route.move.help.syntax -->
