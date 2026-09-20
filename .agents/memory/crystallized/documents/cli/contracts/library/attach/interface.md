---
open-forge:
  description: Define the exact public syntax, source boundary, collision policy, projections, record, and results for `library attach`
  responsibility: Define what attach accepts, creates, preserves, rejects, and reports for one Workspace Library
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, Attach, Interface, Mutation, Recovery, Safety, CurrentTruth]
---

# library attach Interface Contract

Unavailable ownership is reported as `library-attach.ownership-observation`
with `completed` status. This finding grants no ownership or mutation permission.

## Status And Authority

This is the current Crystallized Interface Contract for
`open-forge library attach`. It owns the public purpose, exact syntax, operand
grammar, source-root and consumer boundaries, projection and record effects,
statuses, output, errors, examples, non-goals, and public verification.

The sibling [Behavior Contract](behavior.md) defines deterministic
technology-neutral resolution, complete inventory, planning, preflight,
application, verification, and recovery. The shared [Global CLI Flags](../../shared/global-flags/interface.md),
[Shared Result Coordinates](../../shared/result-coordinates/interface.md), and
[Shared CLI Operation Contract](../../../shared-operation-contract.md) retain
their shared meanings.

The [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
and [Mutation And Recovery Technical Design](../../../technical-designs/mutation-and-recovery.md)
define accepted shared realization boundaries. The [Index Behavior Contract](../../index-candidate/behavior.md)
defines existing generated-navigation projection. This Interface Contract adds
no callable or implementation choice. The active Task records implementation and executable evidence.

The sole generated state publication is `.agents/open-forge.lock.json`.
The existing public record-effect and publication fields describe that lock
write. Its Libraries section contains validated registration identities and
source-relative paths; other ownership sections are preserved. No retired
record is read, written, converted, or deleted. A missing ownership lock may be
created after the explicit attach effects verify. If the whole lock is a
readable ordinary malformed file, Attach may replace that unusable snapshot
with only the newly verified claim; no prior claim is inferred, deleted, or
updated from malformed bytes. An unreadable, aliased, nonordinary, or otherwise
unavailable publication remains unavailable and causes no link,
generated-region, permission, or record effect. Missing or unknown ownership
observations remain present on a successful new attach and are not
presented as a valid empty record. Recovery protects the exact prior lock bytes
before any effect; the planned lock publication remains last after verified
link and generated-region effects.

## Purpose And Operation Boundary

`library attach` registers one new consumer-local library ID and projects the
complete current eligible ordinary-file inventory below one contained source
root into the selected consumer workspace. Each source
path preserves its suffix below the chosen destination root as a
real relative file symlink. The source remains at its source-root path and is
never copied, moved, deleted, or written through by this operation.

Attach is one complete mutation for one library. It establishes the source
root, inventories every eligible file, detects every destination collision,
forms any permitted generated-region changes, and publishes the consumer
record as one plan. A readable ordinary malformed ownership lock is an
observation, not a source of old claims: explicit source and destination
inputs may produce only the new claim after the same inventory, mapping,
permission, and safety checks. Attach never applies a safe subset after a
collision, incomplete source inventory, unsafe boundary, or other preflight
blocker, and it never infers, deletes, or updates an old claim.

The managed library identity is separate from automatic source identity. A
projected file keeps the normal identity of its consumer destination when a
read-only command later observes it. The library ID is never a source-reference
operand and never creates a second route identity.

## Syntax

```text
open-forge library attach <library-id> <source-root> [--to <workspace-relative-directory>] [--dry-run] [--automatic] [global flags]
```

The command path selects the attach operation. The shared [Global CLI Flags](../../shared/global-flags/interface.md)
contract defines `--workspace <path>`, `--format <text|json>`,
`--detail <minimal|standard|full|debug>`, repeatable
`--detail-filter <error|warning|info|all>`, `--help`, and `--version`; all six retain their shared spelling, composition, repetition,
terminal behavior, and output meaning.

`--dry-run` is the only preview spelling. `--help` and `--version` are terminal
forms and stop before workspace, source-root, record, or projection work.

Attach has no aliases, extra operands, `--force`, `--yes`,
`--apply`, collection selector, per-file remapping flag, glob, copy mode, saved plan,
or generic mutation dispatcher.

## Operands And Repetition

| Operand or flag                       | Role                                                           | Accepted value                                                                 | Omission and repetition                                                                                                         |
| ------------------------------------- | -------------------------------------------------------------- | ------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------- |
| `<library-id>`                        | Select the new management identity                             | One value matching the library-ID grammar below                                | Required and singleton. A repeated positional value is invalid. An already registered ID is blocked, not last-wins.             |
| `<source-root>`                       | Select the source directory relative to the selected workspace | One portable workspace-relative path satisfying the source-root boundary below | Required and singleton. A repeated positional value is invalid.                                                                 |
| `--to <workspace-relative-directory>` | Destination root                                               | `.` or a canonical portable child directory                                    | Defaults to `.`. Singleton; repetition is invalid. Native spaced, equals and colon option-value forms follow the pinned parser. |
| `--dry-run`                           | Write policy                                                   | Boolean flag with no value                                                     | Application is selected when omitted. Repetition is accepted and idempotent.                                                    |
| `--automatic`                         | Confirmation policy                                            | Boolean flag with no value                                                     | Final confirmation is required when omitted; this flag bypasses that confirmation only. Repetition is accepted and idempotent. |
| Shared global flags                   | Workspace and presentation                                     | Defined by the shared global contract                                          | Shared defaults and repetition rules apply.                                                                                     |

`--to` selects the recorded destination root. No flag changes source selection, ownership, collision,
containment, record, recovery, or route authority. A dry run does not grant
application authority.

## Library-ID Grammar And Identity

`<library-id>` must contain 1–128 characters and match exactly:

```text
[a-z0-9]+(-[a-z0-9]+)*
```

The grammar uses lowercase ASCII letters and digits in non-empty segments
separated by one hyphen. Uppercase, empty segments, leading or trailing
hyphens, repeated hyphens, whitespace, slashes, backslashes, and other
characters are invalid. The ID is caller-supplied, is not derived from
`<source-root>`, and is unique within the consumer library-record namespace.

Automatic source IDs remain a separate namespace. A projected eligible
`.agents/...` destination retains its ordinary destination-derived automatic
source ID. The library ID is never passed to a source-reference operation,
treated as a route identity, or confused with the Extension `--source` value.

## Source-Root Boundary

The source root is a non-empty canonical portable workspace-relative directory,
strictly contained by the selected workspace lexically and physically. It has
no absolute, empty, backslash, `.` or `..` segment and no portable alias. Every
ancestor and the selected root must be a real ordinary directory, without
symlink, junction or reparse ancestry. No specially named child is required.
The selected directory itself scopes the recursively discovered eligible files.

An absent or non-directory source root is `invalid-input` for Attach. For an existing
registration, unavailable or missing source facts make Inspect or Sync
`incomplete`; a readable non-directory root is `invalid-input`. Unsafe containment,
linked ancestry or ambiguous identity is `blocked`. List reports only bounded
root availability and does not enumerate descendants. An incomplete source is
never an empty source inventory.

The consumer workspace and its existing ordinary `.agents` control directory
remain consumer-owned. The operation does not create or replace either root.
The destination root may be an ancestor of a contained source root, including
`.`. Actual destination leaves and every mutation target must remain outside
all selected and registered source trees. This per-leaf check preserves source
contents without forbidding workspace-root projection.

## Complete Eligible Inventory

Recursively enumerate the selected real source root, recording each eligible
ordinary file by its canonical portable path relative to that root. A source
containing no `.agents` or `content` child is valid. Empty eligible inventory is
complete when the entire selected tree was safely observed.

Exclude Git metadata at any path segment, known manager controls, symlinks,
junctions, reparse points and special entries. Recognize Open Forge Loader,
entrypoint and overwrite controls at their original `.agents` source coordinates
before remapping. A remap cannot make those controls eligible. External
`_name.md`, `*.overwrite.md` and README remain ordinary opaque Library content. Inspect excluded entries without
following them and never descend into excluded metadata or linked directories.
Existing source classification and protected-control rules remain applicable.

An inaccessible directory, enumeration failure, unavailable eligible ordinary
file or unsafe required boundary prevents complete inventory. Retain known safe
facts as partial evidence, never as permission to delete retired links. Source
bytes are never copied, rewritten or deleted. Only eligible leaf membership and
physical path facts feed projection planning.

## Destination Mapping And Projection

Each record keeps `sourceRoot`, `destinationRoot` and source-relative `paths`.
For a path `p`, its source is `sourceRoot/p`. Its consumer destination is `p`
when `destinationRoot` is `.`, otherwise `destinationRoot/p`. Derive the exact
raw relative file-link target from the destination parent to that source.
Root-level leaf destinations use the workspace root as their parent.

Projection creates individual relative file symlinks. Required missing parents
are separate real ordinary directory effects, including first-level parents.
Existing parents must be real ordinary directories with no linked or reparse
ancestry. Local sibling files remain untouched; no directory symlink, copied
file fallback or directory ownership is introduced.

Validate source eligibility and final destination protection separately. Protect
Git metadata, Framework and recognized manager controls, `.agents` Loader,
entrypoint and overwrite controls, authored settings and generated ownership controls, recovery and temporary
storage, and every selected or registered Library source tree. A grant covering
a containing directory never overrides these leaf checks. Compare portable
identity and physical containment. Different source-relative paths and different
Libraries may share ordinary directories but never the same destination leaf.
An unregistered link, including an exact-looking link, is an existing occupant
and is never adopted.

Only mapped `.agents/**` leaves may participate in an existing consumer route
chain and its bounded generated `Entries` projection under the Index contract.
The region and route chain must already exist and authored bytes remain intact.
External Markdown remains opaque content. No source entrypoint, Loader, missing
route or generated region is created.

## Consumer Record

Library selection reads the `libraries` claims in `.agents/open-forge.lock.json`.
The shared ownership codec accepts understood keys without requiring an exact
schema version or member set. It never reads the old Library or lifecycle file
for selection. Each usable Library claim supplies `id`, `sourceRoot`,
`destinationRoot`, and source-relative `paths`. IDs and paths are presented in
ordinal order; typed portable identities and unambiguous mapped destinations
remain required before using a claim. The destination root may be `.`; a source
root may not. Link identity derives from the two roots and each source suffix.
Permissions remain separate from ownership.

An absent, unreadable, nonordinary, malformed, or uninterpretable ownership lock
provides no usable registrations and yields a truthful ownership observation.
It is never reported as a valid empty record: the record state remains `missing`,
`unavailable`, or `invalid-input`, with unavailable counts and no selected paths.
List returns no registrations; Inspect, Sync, and Detach select nothing and do
not invent an unknown-ID error. Their `ownership-observation` finding explains
why. A valid readable lock with no matching requested ID still yields `invalid-input`
for those selected-record operations.

Attach is the narrow exception for explicit new knowledge. A missing lock, or a
whole lock that is a readable ordinary malformed file, may receive a newly
verified claim from the explicit source and destination after complete source,
mapping, ancestor, alias, permission, and safety checks. The malformed bytes do
not supply prior claims and are not reconciled. The resulting `completed`
result remains Complete0 and retains the ownership observation.
An unreadable, aliased, nonordinary, or otherwise unavailable publication has
no effects. Read-only operations never reconstruct or write a lock. Matching
files, matching links, legacy records, and unusable old bytes create no claims.

A successful attach from a readable invalid ownership file keeps Complete0
while rendering that ownership observation as a visible warning at minimal
detail. This warning does not imply that prior ownership was reconstructed.

## Consumer Permission

The repeatable `--allow-path <path>` explicitly authors shared `allowInstallPaths`
in `.agents/open-forge.json` after safe planning and before permission evaluation.
It persists in non-interactive execution; `--dry-run` never writes it. A refused
explicit write is reported and prevents content application. Eligible interactive
approval offers always, once or cancel. Once changes no settings. Unknown or
malformed settings withhold external grants while implicit `.agents/` admission
remains independent; an always choice cannot overwrite malformed settings.

This command selects [Workspace Permissions](../../shared/workspace-permissions/interface.md)
for the complete eligible mapped inventory. `.agents/**` leaves remain implicit.
Requirements are destination paths shared by every Extension and Library; they carry no Library ID or source binding. Permission remains
necessary even for existing owned links; recorded identity makes removal
source-independent, without exempting it from revocation.

Live uncovered leaves propose their immediate parent folder; root leaves use exact grants.
Directory proposals explicitly include future descendants and never cover the
workspace root. Changing a Library source does not change destination permission. Protected paths,
source trees, ancestry, ownership and collision checks still apply per leaf.

Human prompt-capable application can approve the displayed scopes once or always; explicit `--allow-path` can persist shared grants without a prompt. JSON,
redirected execution and dry-run never prompt; missing or declined approval is
`blocked` and cancellation is `cancelled`, without effects. Invalid or unsafe settings supply no external grants. A refused always approval reports its existing permission finding; implicit paths require no grant.

`result.permissions` appears after `plan` and before `application`. It uses the
shared destination-string, scope and receipt coordinates exactly. Required
and missing arrays are concrete destinations; proposed/approved scopes expose
the requested scope using only `{kind,path}`. No rebinding field exists. Only a verified
outcome says permission was saved; later content failure retains that outcome.

Permission findings use the `library-attach.` prefix and suffixes
`permission-required`, `permission-declined`, `permission-invalid`,
`permission-unavailable`, `permission-changed` and `permission-write-failed`.
Changed lease-bound permission facts block; failed permission publication is
`failed` with its actual receipt; cancellation uses the existing `cancelled`
finding. No content effect proceeds after an unverified permission write.
Malformed `--to` uses `library-attach.destination-root-invalid` and `invalid-input`.

## Dry Run And Application

`--dry-run` forms the same typed request, source facts, complete inventory,
mapping set, destination collision checks, generated-region projection,
record bytes, ordered plan, expected-state facts, and effect-free preflight as
application. It reports every projected path, generated-region change, record
change, and blocker, then writes nothing. It creates no directories, links,
record, generated navigation, recovery artifact, or lock. It performs no lease
or recovery capability probe. Any missing, unknown, or malformed ownership
observation remains visible in the dry-run result; the preview never turns an
observation into an unqualified success headline.

Omitting `--dry-run` selects application. Application completes preflight and
all collision checks before acquiring one workspace lease for the effectful
plan. Under that lease it revalidates the workspace, record, source/destination
set, and expected states. Each link or record effect has an immediate
no-follow final-component check immediately before its effect. For an effectful
application, typed recovery is prepared and verified before the first effect;
an effect-free plan has no recovery bundle. Effects are monotonic:
the operation never rolls back or compensates for a verified effect.

The plan may create declared real parent directories, create exact relative
file symlinks, update permitted existing generated regions, and publish the
consumer record last. It never writes source bytes. If an application or
verification failure occurs after an effect, already verified effects remain
true effects, the record is not published early, and the result reports exact
residual state and retained recovery evidence. A later invocation plans from
current facts; it does not replay a saved plan or claim an automatic rollback.

Library recovery distinguishes a prior-missing ordinary consumer-record
`Create` from relative-file-link `Create` and `Delete` entries. It retains
consumer record bytes and exact relative-link identity only. It never stores,
opens, follows, restores, or deletes source bytes. Strong no-follow recovery
can remove an exact created link or recreate an exact deleted link when its
recorded relative target is still the same, including a dangling target. The
shared [Mutation And Recovery Technical Design](../../../technical-designs/mutation-and-recovery.md)
defines the external preparation, verification, and residual boundary.

## Human Output

Every semantic result is rendered by the shared native report. --format text
is the default. The applicable global flags are --workspace <path>, --format
<text|json>, --detail <minimal|standard|full|debug>, repeatable
--detail-filter <error|warning|info|all>, --help, and --version. The default
detail is minimal; standard adds workspace and command context, full adds all
bounded facts, and debug adds bounded diagnostics on stderr. Detail does not
change semantics, effects, counts, or status. Filters select finding severities;
all is the default filter.

Library attach shows a plan review before final confirmation. --automatic bypasses the final confirmation only; without it the confirmation is Apply these changes? [y/N]. Permission prompts remain separate and use Allow always / Allow once / Cancel.

When the final confirmation is required but the invocation is noninteractive or
redirected, Attach returns `invalid-input` (Invalid4) with
`library-attach.confirmation-required` and applies no effect. It never silently
applies a plan. `--automatic` bypasses this final confirmation only; it does not
bypass destination permission, ownership, collision, or safety checks.

The catalogue text by detail level is:

`minimal`:

```text
Registered the team-knowledge Library from shared/team.
  Created 12 links under docs
  Updated the Entries section of docs/_docs.md
  Saved a grant for docs to .agents/open-forge.json
```

`minimal`, permission required outside a terminal (stderr):

```text
Cannot attach team-knowledge: docs is outside .agents and no grant allows writing there.
Next: open-forge library attach team-knowledge shared/team --to docs --allow-path docs
```

`minimal`, interrupted after one link (stderr):

```text
Library attach was cancelled. Stopped after 1 of 2 links were created.
  docs/first.md    created
  docs/second.md   not started
  The Library was not recorded. Recovery data: <path>
Next: open-forge doctor
```

`standard` adds `Workspace:`, every link as a row with its target, the
inventory count, and the lock row.

`full` adds the permission evaluation, expected states, and verification
and recovery facts in words.

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

The command is exactly library attach; data follows the catalogue:

| Level    | `data`                                                                                                              |
| -------- | ------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, id, sourceFolder, destinationFolder, recorded: bool, permissions { decision, required, missing, saved } }` |
| standard | + `links: [ { path, target } ]`, `inventory { eligible, excluded }`                                                 |
| full     | + `expectedStates`, `verification`, `recovery` details                                                              |

Human and JSON output are projections of one typed result. data is null only at
the parser boundary before command binding. There is no alternate JSON
projection.

## Semantic Results

| Status                  | When                                                                          | Headline                                                                                     | Exit | Stream |
| ----------------------- | ----------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | registered and linked, including a new claim formed from a missing or readable ordinary malformed ownership observation | `Registered the <id> Library from <source>.`                         |    0 | stdout |
| completed               | registered, source empty                                                      | `Registered the <id> Library from <source>. It has no eligible files yet.`                   |    0 | stdout |
| completed (dry run)     | planned                                                                       | `Would register the <id> Library from <source>.`                                             |    0 | stdout |
| completed-with-warnings | recovery bundle retained                                                      | + family row                                                                                 |    2 | stdout |
| incomplete              | source, inventory, Entries or recovery unreadable                             | `The <id> Library could not be attached: <limitation>. Nothing was changed.`                 |    3 | stdout |
| invalid-input           | bad ID, missing or non-folder source, bad `--to`, or required final confirmation unavailable without `--automatic` | `Cannot attach <id>: <problem>.`                                             |    4 | stderr |
| blocked                 | ID already registered, collision, unsafe, links unsupported, permission, lock | `Cannot attach <id>: <reason>.`                                                              |    5 | stderr |
| failed                  | after effects                                                                 | `Library attach stopped after <n> of <m> links were created.`                                |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                                                      | `Library attach was cancelled. Nothing was changed.` / `... Stopped after <n> of <m> links.` |  130 | stderr |

### Current merged behavior and open questions

The catalogue assigns lock-held to blocked with exit 5. The merged operation
returns failed with exit 1, operation-failed, and an IOException. This
implementation discrepancy remains deferred; it does not change the accepted
contract above.

The links-unsupported situation has no deterministic fixture and remains
unevidenced. Test-owner decision remains open: build a fixture or drop the
situation.

The native interrupted fixture reports 1 of 1 links, while the catalogue's
illustrative text says 1 of 2. Confirm whether the fixture or only the
illustrative count should change.

## Errors And Boundaries

The finding catalogue is:

| Code                                           | Severity | Family                     | Message                                                                         | Next                              |
| ---------------------------------------------- | -------- | -------------------------- | ------------------------------------------------------------------------------- | --------------------------------- |
| library-attach.invalid-input                   | error    | invalid-input              |                                                                                 |                                   |
| library-attach.confirmation-required           | error    | confirmation-required      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Attach/Shared/Wording/LibraryAttachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-attach.confirmation-required`).              | `open-forge library attach --automatic` |
| library-attach.invalid-id                      | error    | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Attach/Shared/Wording/LibraryAttachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-attach.invalid-id`). | none                              |
| library-attach.duplicate-id                    | error    | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Attach/Shared/Wording/LibraryAttachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-attach.duplicate-id`).                             | `open-forge library inspect <id>` |
| library-attach.source-root-invalid             | error    | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Attach/Shared/Wording/LibraryAttachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-attach.source-root-invalid`).                                | none                              |
| library-attach.source-root-unavailable         | warning  | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Attach/Shared/Wording/LibraryAttachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-attach.source-root-unavailable`).                                                      | none                              |
| library-attach.source-root-blocked             | error    | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Attach/Shared/Wording/LibraryAttachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-attach.source-root-blocked`).                                      | none                              |
| library-attach.destination-root-invalid        | error    | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Attach/Shared/Wording/LibraryAttachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-attach.destination-root-invalid`).                           | none                              |
| library-attach.destination-collision           | error    | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Attach/Shared/Wording/LibraryAttachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-attach.destination-collision`).            | choose another `--to` folder      |
| library-attach.inventory-incomplete            | warning  | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Attach/Shared/Wording/LibraryAttachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-attach.inventory-incomplete`).                                | none                              |
| library-attach.mapping-unavailable             | warning  | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Attach/Shared/Wording/LibraryAttachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-attach.mapping-unavailable`).                  | `open-forge doctor`               |
| library-attach.mapping-blocked                 | error    | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Attach/Shared/Wording/LibraryAttachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-attach.mapping-blocked`).                                     | none                              |
| library-attach.link-capability-unavailable     | error    | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Attach/Shared/Wording/LibraryAttachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-attach.link-capability-unavailable`).                      | none                              |
| library-attach.consumer-blocked                | error    | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Attach/Shared/Wording/LibraryAttachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-attach.consumer-blocked`).             | none                              |
| library-attach.ownership-conflict              | error    | ownership-conflict         |                                                                                 |                                   |
| library-attach.ownership-observation           | info     | ownership-observation      |                                                                                 |                                   |
| library-attach.record-invalid                  | error    | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Attach/Shared/Wording/LibraryAttachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-attach.record-invalid`).     | `open-forge doctor`               |
| library-attach.record-unavailable              | warning  | lifecycle-unavailable      |                                                                                 |                                   |
| library-attach.record-blocked                  | error    | lifecycle-blocked          |                                                                                 |                                   |
| library-attach.permission-required             | error    | permission-required        |                                                                                 |                                   |
| library-attach.permission-declined             | error    | permission-declined        |                                                                                 |                                   |
| library-attach.permission-invalid              | error    | permissions-invalid        |                                                                                 |                                   |
| library-attach.permission-unavailable          | warning  | permissions-unavailable    |                                                                                 |                                   |
| library-attach.permission-changed              | error    | permissions-changed        |                                                                                 |                                   |
| library-attach.permission-write-failed         | error    | permission-write-failed    |                                                                                 |                                   |
| library-attach.generated-navigation-blocked    | error    | generated-region-unsafe    |                                                                                 |                                   |
| library-attach.generated-navigation-incomplete | warning  | projection-unavailable     |                                                                                 |                                   |
| library-attach.lock-unavailable                | error    | workspace-lock-unavailable |                                                                                 |                                   |
| library-attach.recovery-unavailable            | warning  | recovery-unavailable       |                                                                                 |                                   |
| library-attach.recovery-retained               | warning  | recovery-artifact-retained |                                                                                 |                                   |
| library-attach.application-failed              | error    | write-failed               | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Attach/Shared/Wording/LibraryAttachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-attach.application-failed`).              |                                   |
| library-attach.verification-failed             | error    | verification-failed        |                                                                                 |                                   |
| library-attach.operation-failed                | error    | operation-failed           |                                                                                 |                                   |
| library-attach.interrupted                     | error    | interrupted                |                                                                                 |                                   |

Findings retain code, severity, family, message, subject, cause, and next
action when available. Counts are:

`linksCreated`, `sectionsUpdated`, `grantsSaved`, `sourceFiles`.

## Scenarios

`attached-inside-agents`, `attached-outside-with-flag`, `permission-prompt`,
`permission-required-non-interactive`, `empty-source`, `duplicate-id` (blocked),
`source-missing` (invalid), `destination-collision` (blocked), `dry-run`,
`links-unsupported` (blocked), `lock-held`, `interrupted-partial`, `invalid-input`.

Prompt rules from the catalogue:

Permission (`Allow always / Allow once / Cancel`) for destinations outside
`.agents`; plan review; `Apply these changes? [y/N]` unless `--automatic`
(added by 04).

## Representative Transcripts

### completed

~~~text
Registered the team-knowledge Library from shared/team-knowledge.
  Created 1 link under .agents/directives
~~~

### completed-with-warnings

~~~text
Registered the team-knowledge Library from shared/team-knowledge.
  Warning  <recovery-bundle>  Recovery artifact retained
~~~

### incomplete

~~~text
The team-knowledge Library could not be attached: <limitation>. Nothing was changed.
~~~

### invalid-input

~~~text
Cannot attach team-knowledge: shared/missing is not a folder inside the workspace.
~~~

### blocked

~~~text
Cannot attach team-knowledge: A Library with the ID team-knowledge is already registered.
Workspace: <workspace>
Next: open-forge library inspect team-knowledge
~~~

### failed

~~~text
Library attach stopped after 0 of 1 links were created.
Workspace: <workspace>
  .agents/directives/review.md  not started
~~~

### cancelled

~~~text
Library attach was cancelled. Stopped after 1 of 1 links were created.
Workspace: <workspace>
  .agents/directives/review.md  created
  The Library was not recorded. Recovery data: <recovery-bundle>
Next: open-forge doctor
~~~

## Related Current Sources

- [library attach Contract Set](_attach.md)
- [library attach Behavior Contract](behavior.md)

## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`library.attach.help.syntax`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Library/Attach/LibraryAttachText.cs).

<!-- @OpenForgeTextRef library.attach.help.syntax -->
