---
open-forge:
  description: Accepted current public interface for removing one routed leaf or complete routed category and remembering the removal
  responsibility: Define what `route remove` accepts, removes, reports, rejects, and leaves unchanged
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Remove, Interface, Mutation, Reference, Safety, CurrentTruth]
---

# route remove Interface Contract

## Status And Authority

This is the accepted current Crystallized authority for the caller-visible
Interface Contract for `route remove`. The command is implemented in the merged CLI;
implementation and executable evidence are tracked in
[CLI Development](../../../../../../working/cli-development/_cli-development.md). It is one explicit
mutation operation, not a generic batch or apply surface.

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

`route remove` removes one eligible ordinary logical leaf or one
eligible ordinary category, including Framework- or Extension-managed content.
It removes the complete selected physical
subject, updates affected generated navigation in the same operation, and
detaches every supported incoming authored Markdown link from outside the
removed subject by replacing that link with its visible label as plain authored
text.

A leaf is one ordinary routed Markdown base source and its valid adjacent
overwrite companion, when present. A category is one recognized entrypoint and
the complete physically contained folder tree below it. A category contains all
physically contained regular files and directories, including descendant
entrypoints, routed or unrouted Markdown, native or binary resources, ordinary
support files, and overwrite companions. Generated `Entries` interiors are
derived projections rather than authored authority.

The command performs one operation for one subject. A category plan may
enumerate many contained logical sources and resources, but it has one atomic
plan, one recovery boundary, and one final verification boundary. It is never a
series of independently committed leaf removals and never exposes a generic
batch or saved-plan surface.

The command is deterministic for unchanged workspace bytes and
explicit input. A repeated remove is a verified no-op only when exact intended
absence is independently established with complete trusted ownership, topology,
reference, and generated-projection evidence, with no orphan companion, residual
incoming reference, or stale generated region. Otherwise a missing, invalid,
incomplete, or blocked result is returned as applicable; absence alone does not
prove that an earlier remove succeeded. A proven absent target completes with no
effects, workspace writes, lock or recovery preparation, or finding, and uses
the source-specific headline `Nothing to do for <source>.`. Persistent exclusions
record what managers must leave removed; they do not prove current absence or
successful completion. A missing exclusion is a settings effect, not a no-op.

Leaf removal records exact file paths, including an existing overwrite. Category
removal records its directory, covering future descendants; a root category also
records its category name. These authored settings are verified before content
changes. Restore content by clearing all covering exclusions from
`.agents/open-forge.json` and explicitly running its installer or updater.

## Syntax

```text
open-forge route remove <source-reference>
  [--dry-run]
  [--automatic]
  [global flags]
```

The command path selects the remove operation. It requires exactly one source
reference. The shared [Global CLI Flags](../../shared/global-flags/interface.md)
contract defines `--workspace <path>`, `--format <text|json>`, `--detail <minimal|standard|full|debug>`, repeatable `--detail-filter <error|warning|info|all>`, `--help`, and `--version`; all six apply under that contract.

`--dry-run` is the only preview spelling. The command does not inspect or report
repository state. It does not select a subject, add authority,
or change the operation.

The command has no `--force`, `--yes`, `--apply`, `--all`,
`--batch`, `--recursive`, root remove mode, alias, saved plan, receipt, or
generic mutation dispatcher. It has no operand-free wizard because the primary
subject cannot be safely selected by enumeration. JSON and other
non-interactive use therefore use the same explicit request; a final confirmation
is still required unless `--automatic` is supplied.

## Operand And Repetition

| Operand              | Role                                                                        | Accepted value                                                                                       | Omission and repetition              |
| -------------------- | --------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- | ------------------------------------ |
| `<source-reference>` | Select one existing eligible logical leaf or recognized category entrypoint | One shared automatic source ID or exact `.agents/...` path, narrowed by this command's subject rules | Required; exactly one source subject |

The source operand follows the shared source-reference classification: `.agents/`
and `./.agents/` prefixes request exact paths, and every other value is an
automatic source ID. The CLI does not guess between the two forms. An ID that
does not resolve to exactly one eligible subject is not repaired by ranking,
basename matching, or route proximity.

`--dry-run` is a Boolean write-policy flag. Repeating it is accepted and
idempotent. Repetition does not multiply preview, consent, recovery, or mutation
authority. Shared global-flag
repetition, ordering, terminal behavior, and composition remain defined only by
the shared contract.

## Source Subject Selection

The source reference must resolve to one of these two subject forms:

1. One ordinary routed Markdown base source, optionally with its valid adjacent
   overwrite companion. This is the leaf form.
2. One recognized entrypoint that identifies one category folder. The source
   reference must resolve to that entrypoint itself, not merely to an arbitrary
   directory. This is the category form.

The Loader and the `.agents` workspace root are never remove subjects. A
Loader-exposed category may be eligible; when it is removed, the affected Loader
projection is part of the same plan. A recognized compatibility entrypoint can
be selected only when the route structure is otherwise unambiguous.

An automatic-ID collision is blocked in non-interactive and JSON use and does
not select a candidate. An exact path can identify the intended physical source,
but it cannot make an ambiguous route, overwrite pair, or category inventory
safe.

## Eligible Leaf

An eligible leaf is one ordinary routed Markdown logical source below an existing
valid entrypoint. Its base file and valid adjacent overwrite companion are one
logical subject and are removed together, preserving the pair's identity until
the complete effect is applied.

The selected leaf is not eligible when it is any of the following:

- an entrypoint, the Loader, or the `.agents` root;
- a routed native source such as `SKILL.md`, a non-Markdown resource, or an
  unsupported source kind;
- an orphan or ambiguous overwrite companion;
- a source with any trusted Framework or Extension lifecycle ownership claim;
- a source whose identity, route, containment, collision, reference, or recovery
  boundary cannot be established completely; or
- a source whose removal would leave a supported incoming reference that cannot
  be safely detached.

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

Derived generated regions are projected from the intended post-remove topology.
They are not treated as authored source authority or as independent category
items.

The category is eligible only when every contained item has complete safe
containment, physical identity, ownership, lifecycle, collision, reference, and
recovery classification. An item that is neither a regular file nor a directory,
or that is missing, malformed, conflicting, stale, unreadable, externally
resolving, ambiguous, incomplete, or unsafe, prevents the category operation as
a whole. An unfamiliar file extension does not exclude an otherwise safe regular
file from the category. The command does not skip an item, remove a safe subset,
or reinterpret a folder as a batch of leaf commands.

An entrypoint nested inside the selected category is part of the category, not a
second operation. A category's internal relative layout is removed as one
complete subject. The command does not flatten, reorder, or independently
commit descendants. The Loader itself and any item whose physical identity
cannot remain contained are outside this boundary.

A nested `.git` directory or worktree-pointer file blocks removal of the whole
category. Ordinary files such as `.gitignore` do not trigger this guard.

## Final-Leaf Safety Boundary

Every file leaf that `route remove` would change or remove, including the
selected subject, incoming-reference, and generated-navigation leaves, has a
caller-visible no-follow observation of its immediate final filesystem component.
A filesystem link, reparse point, or special final leaf, including a relative
file link that is an exact Workspace Library projection, is separately owned and
unsafe for ordinary Route Remove mutation. The command keeps its existing
`blocked` target-safety result, identifies the affected path, and performs no
effect.

This physical-leaf boundary does not depend on Library registration state. A missing,
malformed, stale, or otherwise unreadable Library record neither makes the
final leaf ordinary nor grants Route Remove mutation authority. Route Remove
never resolves a final filesystem leaf and then deletes its physical source
target. The guard concerns each final component addressed by the remove plan;
ordinary directory-ancestry rules remain defined by the filesystem contract.

## Ownership And Persistent Removal

The command reads ownership from `.agents/open-forge.lock.json`. A complete,
interpretable inventory identifies every Framework and Extension claim to the
selected files and generated regions. Explicit route removal releases all such
claims to the deleted paths, even when several packages share a file. Package
registrations and unrelated claims remain intact. Removing an Extension by ID
retains files shared with another owner under its separate lifecycle contract.

A missing lock is known empty and supplies no claims. Unreadable or
uninterpretable ownership cannot authorize removal.
It produces `ownership-unavailable` with blocked status, no plan or effects,
and ownership shown as not-established. The summary explicitly says that no
route changed. Schema/release metadata and stale content hashes do not establish
or invalidate an otherwise interpretable claim. Physical safety, unknown
ownership, route and reference conflicts remain
blocking boundaries. Settings and the exact lock expectation are revalidated
under the workspace lease. The intended ownership release is published only
after content and navigation effects have been verified. No legacy record is
read, migrated or deleted.

Path names, routing tags, generated lines, matching bytes and prior command
results do not establish ownership. Library projections remain outside this
operation; select an individual owned link with root `remove --kind path`.

## Complete Reference Pass And Detachment

Remove performs one complete physically contained pass over the supported
Markdown catalogue in the selected workspace, both inside and outside `.agents`.
The catalogue includes every supported Markdown source and physical layer that
can contain an authored local reference, including base files and overwrite
companions. The command does not silently narrow this pass to routed files or to
the selected category.

Generated `Entries` interiors are derived navigation. The pass does not treat
their links as authored reference authority or detach them as ordinary prose;
affected generated regions are projected separately.

For every exact supported incoming local Markdown link from outside the selected
subject to the removed leaf, category member, or resource, the operation
replaces the complete link with its visible label as plain authored text. It
preserves the surrounding prose, whitespace, punctuation, and unrelated bytes.
Each detachment is a planned effect and is surfaced in dry-run and final human
and JSON results with its source location, original target, and visible label.

References originating inside the removed subject disappear with that subject.
They are not rewritten or detached in a surviving source. References that do
not target the removed subject remain unchanged.

External URLs are not local incoming references and are not changed. Unsupported
or ambiguous link forms that could target the removed subject, a transformation
that would lose surrounding prose, or an unsafe target identity blocks the
operation with no writes. If the supported Markdown catalogue cannot be
completely enumerated or inspected, the result is `incomplete` and no write
begins. The command never silently leaves a supported incoming link pointing at
the removed subject.

## Generated Navigation

Remove projects generated navigation against the hypothetical post-remove
workspace through the accepted [Index Behavior Contract](../../index-candidate/behavior.md).
It includes the old exposing parent projection and includes the Loader
projection when the removed subject is a Loader-exposed category. Any other
generated region is included only when its direct-child projection changes under
the removed topology.

The projection uses current authored topology and metadata, not current generated
lines, to derive entries. It preserves the Entries heading and every byte
outside the heading-owned generated body. It never starts a hidden `index`
subprocess. A generated boundary or required sibling projection that cannot be
established safely prevents the complete plan.

Generated effects, incoming-link detachments, and subject removal are part of the
same plan. The result reports them together, and final verification checks the
complete post-remove projection rather than treating navigation as a later
cleanup step.

## Planning And Effects

Remove follows one complete typed mutation flow:

```text
validated source
  -> complete leaf or category inventory
  -> complete ownership and persistent-exclusion plan
  -> complete reference catalogue and intended detachments
  -> post-remove route and generated projection
  -> one ordered complete mutation plan
  -> preflight
  -> dry-run or application
  -> verification and retained partial-state reporting
  -> one typed result
```

The plan includes every selected source/resource path, incoming-reference
detachment, affected generated region, expected state, verification condition,
and recovery requirement. Compatible changes to one physical file coalesce into
one complete-file effect. A category plan remains one operation even when it
contains many file effects; it is not independently committed per descendant.

One invalid, blocked, incomplete, conflicting, or unsafe required fact prevents
all effects. There is no partial, best-effort, silently narrowed, or
independently committed leaf application.

## Dry Run And Apply

`--dry-run` uses the same request, current facts, trusted ownership inventory,
complete category inventory, reference catalogue, intended detachments, generated
projection, plan, expected-state checks, and preflight as application. It shows
every removed, detached, and generated effect needed to review the complete
operation, then writes nothing. Planned changes alone do not create `completed-with-warnings`.

When the final-leaf safety boundary fails, dry-run and application retain the
same existing `blocked` target-safety result and produce no effect.

Omitting `--dry-run` selects application. The command path and the exact source
subject select the operation, but a prompt-capable terminal receives a plan
review followed by `Delete the <N> files listed above? [y/N]` unless
`--automatic` is supplied. JSON, redirected, and other non-interactive requests
without `--automatic` form the command-specific confirmation-required
invalid-input result before effects. The command does not accept `--yes`.

The explicit consent covers only the selected leaf or complete category, its
incoming-link detachments, and generated projections in the complete plan. It
does not grant ownership, lifecycle, collision, containment, heading-repair, or
recovery bypass authority.

For an actual application, the complete plan checks every existing path it may
change or remove through ordinary workspace facts. It does not inspect or report
repository state.

Before the first target effect, orchestration uses only
`Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
Environment.SpecialFolderOption.Create)` and its application-owned
`OpenForge/recovery/v1` subtree. There is no temporary-directory, repository,
`HOME`, or custom-platform fallback; unavailable storage is a
pre-effect `incomplete` result. When the operation has one or more existing-target
effects (`Replace`, `ReplaceGeneratedRegion`, or `Delete`), it prepares exactly
one immutable ZIP bundle outside the workspace. An operation containing only Create effects or
no-ops creates no bundle. Its source-generated
versioned `manifest.json` and streamed ordinal payload entries record
command/operation/workspace identity, ordered relative targets, change kinds,
exact prior bytes/lengths/hashes, and intended final absence or length/hash.
`Create` and semantic/byte no-op effects have no entry. A CreateNew draft is
closed and reopened for semantic manifest, exact ordered entry, length, hash,
and payload-byte validation, moved within the same directory to its deterministic
final name, and reopened and verified. Only the valid final ZIP forms the opaque
`RecoveryBundlePreparation`; the draft remains `Incomplete`.
`FileChangeApplier` requires the matching preparation for every existing-target effect
and performs one final effect per target. All preparation completes before the
first target effect; unknown, malformed, mismatched, or colliding bundles block.

After final verification, delete only the positively recognized bundle created
by this operation. `Deleted`/`Removed` permits normal completion.
`Failed`/positively observed `Retained` keeps target effects successful and
produces `completed-with-warnings`, the exact residual path, and
cleanup guidance. `Failed`/`Unknown` produces `failed` and reports an exact expected path only when the deletion result
provides one. Before post-verification deletion begins, a handled application,
verification, or cancellation outcome reports the actual residual draft or
final path; a valid final remains when preparation completed. A closed final ZIP may remain after
abrupt process termination, without an executable crash or power-loss guarantee.
Recovery provenance does not classify current target state, and no target is
restored automatically. Cleanup owns exact named final and draft deletion under
its separate lease-bound contract.

## Human Output

Every semantic result is rendered by the shared native report. --format text
is the default. The applicable global flags are --workspace <path>, --format
<text|json>, --detail <minimal|standard|full|debug>, repeatable
--detail-filter <error|warning|info|all>, --help, and --version. The default
detail is minimal; standard adds workspace and command context, full adds all
bounded facts, and debug adds bounded diagnostics on stderr. Detail does not
change semantics, effects, counts, or status. Filters select finding severities;
all is the default filter.

Route remove shows a plan review before final confirmation. --automatic bypasses the final confirmation only; without it the exact prompt is Delete the <N> files listed above? [y/N].

The catalogue text by detail level is:

`minimal`:

```text
Removed .agents/memory/emerging/ideas/pricing/tiers.md
  Entry removed from .agents/memory/emerging/ideas/pricing/_pricing.md
  Detached 1 link that pointed at it; the link text was kept:
    .agents/maps/_maps.md:12:3
  The deleted file is kept in a recovery bundle at <recovery-path>.
Next: open-forge cleanup  (after reviewing the bundle)
```

`minimal`, category:

```text
Removed the route memory/projects/alpha  (6 files)
  .agents/memory/projects/alpha/_alpha.md
  .agents/memory/projects/alpha/plan.md
  ...
  Entry removed from .agents/memory/projects/_projects.md
  The deleted files are kept in a recovery bundle at <recovery-path>.
Next: open-forge cleanup  (after reviewing the bundle)
```

When the bundle was removed after verification (the ordinary case for route
remove), the bundle sentence and `Next` are omitted. Record in the ledger
which case applies; the contract says the bundle is deleted after
verification.

`standard` adds `Workspace:` and per detached link the text that remained
(`[Tiers](tiers.md) -> Tiers`).

`full` adds hashes and the scan summary.

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

The command is exactly route remove; data follows the catalogue:

| Level    | `data`                                                                                                                |
| -------- | --------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, subject: "file" \| "route", source { id, path }, removed: [ path ], detachedLinks: [ { path, location } ] }` |
| standard | + per link `before`, `after` text                                                                                     |
| full     | + per effect `before`, `after`, `scan { filesScanned, occurrences }`                                                  |

Human and JSON output are projections of one typed result. data is null only at
the parser boundary before command binding. There is no alternate JSON
projection.

The schema is unchanged for a proven absence. Its `removed`,
`detachedLinks`, `effects`, `findings`, and `recovery` values are empty or
none as defined by the shared result schema, and the result records no
workspace write or recovery bundle.

## Semantic Results

| Status                  | When                                                                     | Headline                                                           | Exit | Stream |
| ----------------------- | ------------------------------------------------------------------------ | ------------------------------------------------------------------ | ---: | ------ |
| completed               | target known absent under complete safe coverage                         | `Nothing to do for <source>.`                                    |    0 | stdout |
| completed               | leaf removed                                                             | `Removed <path>`                                                   |    0 | stdout |
| completed               | category removed                                                         | `Removed the route <id>  (<N> files)`                              |    0 | stdout |
| completed (dry run)     | planned                                                                  | `Would remove <path>` / `Would remove the route <id>  (<N> files)` |    0 | stdout |
| completed-with-warnings | recovery bundle retained after success                                   | + family row                                                       |    2 | stdout |
| incomplete              | catalogue, scan or record unreadable                                     | `<id> could not be removed: <limitation>. Nothing was changed.`    |    3 | stdout |
| invalid-input           | bad source, the Loader, an overwrite file, or missing source without complete absence proof | `Cannot remove <ref>: <problem>.`                 |    4 | stderr |
| blocked                 | unavailable required ownership, a link that cannot be detached safely, unsafe path, lock | `Cannot remove <id>: <reason>.`                                    |    5 | stderr |
| failed                  | after effects                                                            | `Route remove stopped after <n> of <m> changes.`                   |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                                                 | `Route remove was cancelled. Nothing was changed.`                 |  130 | stderr |

### Known absent target

When complete safe coverage proves that the requested target is absent, Route
Remove returns `completed` at exit 0 with the source-specific headline
`Nothing to do for <source>.`. It has no effects, workspace writes, lock
infrastructure, recovery bundle, or finding. The source is the requested
identity, or its resolved source ID when one is available. An unknown or
incompletely acquired target, ambiguous or unsafe path, ownership boundary, or
other unproven absence retains the applicable invalid-input, incomplete, or
blocked result; it is never converted into absence or success.

## Errors And Boundaries

The finding catalogue is:

| Code                                       | Severity | Family                      | Message                                                                                            | Next                                                     |
| ------------------------------------------ | -------- | --------------------------- | -------------------------------------------------------------------------------------------------- | -------------------------------------------------------- |
| route-remove.invalid-input                 | error    | invalid-input               |                                                                                                    |                                                          |
| route-remove.invalid-source                | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Remove/Shared/Wording/RouteRemoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-remove.invalid-source`).                                                | `open-forge route list --depth=all`                      |
| route-remove.source-not-found              | error    | unknown-source              | retained for unknown or unproven absence; a proven no-op emits no finding    |                                                          |
| route-remove.invalid-subject               | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Remove/Shared/Wording/RouteRemoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-remove.invalid-subject`). | none                                                     |
| route-remove.workspace-unavailable         | error    | workspace-unavailable       |                                                                                                    |                                                          |
| route-remove.workspace-unsafe              | error    | workspace-unsafe            |                                                                                                    |                                                          |
| route-remove.source-unsafe                 | error    | source-unsafe               |                                                                                                    |                                                          |
| route-remove.category-unsafe               | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Remove/Shared/Wording/RouteRemoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-remove.category-unsafe`).                       | none                                                     |
| route-remove.route-ambiguous               | error    | route-ambiguous             |                                                                                                    |                                                          |
| route-remove.identity-collision            | error    | identity-collision          | (the prompt resolves it in a terminal)                                                             |                                                          |
| route-remove.overwrite-ambiguous           | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Remove/Shared/Wording/RouteRemoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-remove.overwrite-ambiguous`).                                     | fix by hand                                              |
| route-remove.ownership-claimed             | error    | ownership-claimed           |                                                                                                    | `open-forge update` / `open-forge extension remove <id>` |
| route-remove.reference-unsafe              | error    | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Remove/Shared/Wording/RouteRemoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-remove.reference-unsafe`).                                      | fix by hand                                              |
| route-remove.generated-region-unsafe       | error    | generated-region-unsafe     |                                                                                                    |                                                          |
| route-remove.workspace-lock-unavailable    | error    | workspace-lock-unavailable  |                                                                                                    |                                                          |
| route-remove.target-changed                | error    | target-changed              |                                                                                                    |                                                          |
| route-remove.recovery-conflict             | error    | recovery-conflict           |                                                                                                    |                                                          |
| route-remove.ownership-unavailable | error | lifecycle-unavailable | Required ownership could not be established; no route changed. | Repair the ownership record, then rerun. |
| route-remove.settings-unavailable | error | local | Required workspace settings could not be established; no route changed. | Repair the settings, then rerun. |
| route-remove.protected-target | error | local | The selected route overlaps protected control state or a registered Library source. | Select an unprotected route. |
| route-remove.inspection-incomplete         | warning  | inspection-incomplete       |                                                                                                    |                                                          |
| route-remove.category-inventory-incomplete | warning  | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Remove/Shared/Wording/RouteRemoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-remove.category-inventory-incomplete`).                   | `open-forge doctor`                                      |
| route-remove.reference-coverage-incomplete | warning  | local                       | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Route/Remove/Shared/Wording/RouteRemoveWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`route-remove.reference-coverage-incomplete`).             | `open-forge doctor`                                      |
| route-remove.projection-incomplete         | warning  | projection-unavailable      |                                                                                                    |                                                          |
| route-remove.recovery-unavailable          | warning  | recovery-unavailable        |                                                                                                    |                                                          |
| route-remove.recovery-artifact-retained    | warning  | recovery-artifact-retained  |                                                                                                    |                                                          |
| route-remove.target-changed-during-apply   | error    | target-changed-during-apply |                                                                                                    |                                                          |
| route-remove.write-failed                  | error    | write-failed                |                                                                                                    |                                                          |
| route-remove.verification-failed           | error    | verification-failed         |                                                                                                    |                                                          |
| route-remove.recovery-failed               | error    | recovery-failed             |                                                                                                    |                                                          |
| route-remove.operation-failed              | error    | operation-failed            |                                                                                                    |                                                          |
| route-remove.interrupted                   | error    | interrupted                 |                                                                                                    |                                                          |

Findings retain code, severity, family, message, subject, cause, and next
action when available. Counts are:

`filesRemoved`, `sectionsUpdated`, `linksDetached`, `filesScanned`.

## Scenarios

`leaf-removed`, `leaf-with-detached-links`, `category-removed`, `dry-run`,
`source-not-found` (proven absence -> completed no-op; otherwise refusal),
`managed-source` (removed with persistent exclusion and claim release), `unsafe-link-detach`
(blocked), `ambiguous-source-prompt`, `reference-scan-incomplete`, `lock-held`,
`write-failed-partial`, `cancelled`.

Prompt rules from the catalogue:

Select when the source ID matches several files; plan review listing every
deletion, then `Delete the <N> files listed above? [y/N]` in a terminal
without `--automatic` (04 adds `--automatic` to this command).

## Representative Transcripts

### completed

~~~text
Removed .agents/guidance/old guide.md
  Removed .agents/guidance/old guide.overwrite.md
  Entry removed from .agents/guidance/_guidance.md
  Detached 1 link that pointed at it; the link text was kept:
    README.md:1:8
~~~

The proven missing-source no-op is also completed:
`Nothing to do for guidance/old guide.`

### completed-with-warnings

~~~text
Removed .agents/guidance/old guide.md
  Warning  <recovery-bundle>  Recovery artifact retained
~~~

### incomplete

~~~text
guidance/old guide could not be removed: Some files could not be scanned for links to .agents/guidance/old guide.md, so the removal was not planned. Nothing was changed.
Next: open-forge doctor
~~~

### invalid-input

~~~text
Cannot remove guidance/old guide: The source is not a source ID or a path under .agents.
~~~

### blocked

~~~text
Cannot remove guidance/old guide: .agents/open-forge.lock.json could not be read completely.
Workspace: <workspace>
  Error  <workspace>/.agents/open-forge.lock.json  Ownership record is unavailable
         .agents/open-forge.lock.json could not be read completely.
~~~

### failed

~~~text
Route remove stopped after 2 of 4 changes.
Workspace: <workspace>
  .agents/guidance/old guide.md  not started
  .agents/guidance/old guide.overwrite.md  not started
  Entry removed from .agents/guidance/_guidance.md
  Detached 1 link that pointed at it; the link text was kept:
    README.md:1:8
  The deleted files are kept in a recovery bundle at <recovery-bundle>.
Next: open-forge cleanup  (after reviewing the bundle)
~~~

### cancelled

~~~text
Route remove was cancelled. Nothing was changed.
Workspace: <workspace>
Next: open-forge route remove
~~~

## Related Current Sources

- [route remove Interface Contract](interface.md)
- [route remove Command Contract Set](_remove.md)
- [Route group entrypoint](../_route.md)
- [Index Behavior Contract](../../index-candidate/behavior.md)
- [Index Interface Contract](../../index-candidate/interface.md)
- [CLI Architecture](../../../architecture.md)
- [Global CLI Flags Behavior Contract](../../shared/global-flags/behavior.md)
- [CLI Source References Behavior Contract](../../shared/source-references/behavior.md)
- [References Behavior Contract](../../references-candidate/behavior.md)
- [Doctor Behavior Contract](../../doctor/behavior.md)
- [Route Update Behavior Contract](../update/behavior.md)
- [CLI Command Contract Set — Interface Contract](../../../command-contract-set.md#interface-contract)
- [Historical CLI Decision Agenda](../../../../../../archived/cli-release/decision-agenda-2026-08-21.md)
- [Historical CLI Release Plan](../../../../../../archived/cli-release/release-plan-2026-08-21.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
- [Routing Model](../../../../framework/routing/model.md)
- [Routing Paths And Identity](../../../../framework/routing/paths.md)
- [Overwrite Customization](../../../../framework/routing/overwrites.md)
- [Routed Markdown Representation](../../../../framework/markdown/routes.md)
- [Markdown Compatibility Boundary](../../../../framework/markdown/compatibility.md)

## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`route.remove.help.syntax`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Route/Remove/RouteRemoveText.cs).

<!-- @OpenForgeTextRef route.remove.help.syntax -->

## Approved Journey Wording References

The following stable IDs link the approved journey behavior above to its typed
human-wording factories. Independently reviewed snapshots and state assertions
remain the output evidence.

- [RouteRemoveText.cs](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Route/Remove/RouteRemoveText.cs)
  <!-- @OpenForgeTextRef route.remove.message.nothing-to-do-for-source -->
