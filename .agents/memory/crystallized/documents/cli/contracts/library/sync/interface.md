---
open-forge:
  description: Define the exact public syntax, complete-inventory requirement, reconciliation effects, record, and results for `library sync`
  responsibility: Define what sync accepts, creates, retires, preserves, rejects, and reports for one registered Workspace Library
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, Sync, Interface, Mutation, Recovery, Safety, CurrentTruth]
---

# library sync Interface Contract

A missing ownership lock is known empty. Invalid or unavailable required ownership blocks the request before any effects.

## Status And Authority

This is the current Crystallized Interface Contract for
`open-forge library sync`. It owns the public purpose, exact syntax, operand
grammar, record and source-availability boundary, reconciliation effects,
statuses, output, errors, examples, non-goals, and public verification.

The sibling [Behavior Contract](behavior.md) defines deterministic
technology-neutral resolution, complete source inventory, set reconciliation,
planning, preflight, application, verification, and recovery. The shared
[Global CLI Flags](../../shared/global-flags/interface.md),
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
record is read, written, converted, or deleted. A planned lock publication is
required for successful completion. If publication fails after earlier effects,
the result reports the partial state and retains recovery evidence.
Recovery protects the exact prior lock bytes before any effects; the planned
lock publication remains last after verified link and generated-region effects.

## Purpose And Operation Boundary

`library sync` reconciles one registered consumer-local library with the
complete current eligible ordinary-file inventory below its recorded source
root. It creates missing links for current registered
or newly discovered eligible paths, removes retired projections only when the
registered relative-link identity is proven, updates permitted existing
generated `Entries` regions, and publishes the sorted consumer record last.

Sync is one complete mutation for one library. It must establish the complete
source inventory before planning any retirement. A source that is unavailable
or incompletely enumerable prevents every addition, retirement, generated
change, and record effect and returns `incomplete` with no effects; it says the
source is unavailable rather than treating the observation as an empty source.
A changed ordinary destination is eligible for retention only when it is the
current destination of a registered mapping. If independent safe mapping
effects exist, Sync applies exactly those effects and returns `incomplete`,
naming the retained destination. If no independent safe effect exists, the
changed mapping remains the prior `blocked` refusal. Other unsafe or unavailable
mapping facts do not acquire a continuation bypass.

Sync changes projection links and the consumer record only. It never copies,
moves, deletes, or writes through source bytes. Changes to the bytes of an
ordinary source file are visible through its existing link and do not create a
copy or a source mutation.

## Syntax

```text
open-forge library sync <library-id> [--dry-run] [--automatic] [global flags]
```

The command path selects the sync operation. The shared [Global CLI Flags](../../shared/global-flags/interface.md)
contract defines `--workspace <path>`, `--format <text|json>`,
`--detail <minimal|standard|full|debug>`, repeatable
`--detail-filter <error|warning|info|all>`, `--help`, and `--version`; all six retain their shared spelling, composition, repetition,
terminal behavior, and output meaning.

`--dry-run` is the only preview spelling. `--help` and `--version` are terminal
forms and stop before workspace, record, source, or projection work.

Sync has no source-root operand, aliases, extra positional operands, `--force`,
`--yes`, `--apply`, `--prune`, collection selector, remapping
flag, glob, copy mode, saved plan, partial-detach selector, or generic mutation
dispatcher. The recorded `sourceRoot` is the only source selection for the
requested library.

## Operand And Repetition

| Operand or flag     | Role                                      | Accepted value                                  | Omission and repetition                                                                                         |
| ------------------- | ----------------------------------------- | ----------------------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| `<library-id>`      | Select one registered management identity | One value matching the library-ID grammar below | Required and singleton. An unknown ID in readable ownership is invalid; unavailable required ownership blocks effects. |
| `--dry-run`         | Write policy                              | Boolean flag with no value                      | Application is selected when omitted. Repetition is accepted and idempotent.                                    |
| `--automatic`      | Confirmation policy                       | Boolean flag with no value                      | Final confirmation is required when omitted; this flag bypasses that confirmation only. Repetition is accepted and idempotent. |
| Shared global flags | Workspace and presentation                | Defined by the shared global contract           | Shared defaults and repetition rules apply.                                                                     |

No flag adds retirement authority, bypasses expected-link proof, chooses a
source root, or changes record ownership. Sync never treats a recommendation
or a matching unregistered link as permission to adopt it.

## Library-ID Grammar And Record Selection

`<library-id>` must contain 1–128 characters and match exactly:

```text
[a-z0-9]+(-[a-z0-9]+)*
```

The grammar uses lowercase ASCII letters and digits in non-empty segments
separated by one hyphen. Uppercase, empty segments, leading or trailing
hyphens, repeated hyphens, whitespace, slashes, backslashes, and other
characters are invalid. The ID is a management identity in the consumer
library-record namespace. It is not derived from the source root and is not an
automatic source ID or source-reference operand.

Selection reads `libraries` claims in `.agents/open-forge.lock.json`. A usable
claim supplies `id`, `sourceRoot`, `destinationRoot`, and source-relative `paths`.
The shared codec reads understood fields without requiring exact member sets,
member order, or a matching schema version. Typed portable roots, eligible source
suffixes, and unambiguous destinations remain required before using a claim.

A missing lock supplies no registrations. Invalid or unavailable required
ownership blocks synchronization before effects. Sync never falls back to the
old record or infers ownership from matching links. An unknown ID in a readable
lock remains `invalid-input`.
Exact relative-link targets derive from both recorded roots and each suffix.
The lock stores no target bytes or comparison hashes.

## Source Availability And Complete Inventory

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
bytes are never copied, rewritten or deleted. The result is `incomplete` (exit
3) with no effects and says the source is unavailable; it never presents the
incomplete observation as an empty source. Only eligible leaf membership and
physical path facts feed projection planning.

## Reconciliation Effects

Let the complete current eligible path set be `P` and the selected record's
registered path set be `R`. Both sets use the same portable source-relative
strings. Both sets use the recorded destination root; Sync cannot change it.

| Relationship | Destination fact                                                                                                                   | Sync effect                                                                       |
| ------------ | ---------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------- |
| `P ∩ R`      | Exact registered relative file symlink remains at the mapped destination and its raw target is the derived target                  | Preserve the link and record path.                                                |
| `P ∩ R`      | Destination leaf is missing and its parent path is safe                                                                            | Create the exact relative file symlink and retain a `registered-link-restored` Attention finding. The warning also applies to a dry run, which writes nothing. |
| `P ∩ R`      | Destination is an ordinary file for the current registered mapping                                                                  | Preserve the ordinary bytes and retain the mapping. Continue only independent safe mapping effects; with such effects the result is `incomplete` and names the retained destination, otherwise block the whole Sync. |
| `P ∩ R`      | Destination is a directory, different link, special entry, unsafe path, unknown state, or separately owned path                   | Block the whole Sync.                                                             |
| `P \ R`      | Destination leaf is exactly missing and its parent path is safe                                                                    | Create a new exact relative file symlink and add the path to the record.          |
| `P \ R`      | Any destination occupant exists or is unsafe, including an unregistered matching link                                              | Block the whole Sync.                                                             |
| `R \ P`      | Destination is the exact registered relative symlink with the derived raw target, whether its source target is present or dangling | Delete only that exact link and remove the path from the record.                  |
| `R \ P`      | Destination is positively missing, even with safe no-follow parents                                                                | Block the whole Sync because the registered link cannot be proven for retirement. |
| `R \ P`      | Destination is an ordinary file, directory, different link, special entry, unsafe path, unknown state, or separately owned path    | Block the whole Sync.                                                             |

The source inventory must be complete before the `R \ P` set is formed. A
source file becoming excluded is absent from `P` and follows the same exact
retirement proof. A missing current link in `P ∩ R` is safe drift that Sync may
repair after complete preflight, with an Attention2 warning; a missing retired
link in `R \ P` is unverifiable and blocks. A changed ordinary occupant is
retained only for a current registered mapping and only alongside independent
safe effects; otherwise it remains blocked. Every other changed, unsafe, or
unavailable mapping remains a blocker or incomplete no-effect result according
to its fact. Sync never uses a warning to bypass an unsafe mutation
precondition.

Sync does not sweep unregistered consumer files or adopt a matching symlink.
Local sibling files remain untouched. Source file byte changes do not create a
retirement or replacement effect because links project live source content.

## Generated Navigation And Record

When a source addition or retirement changes an already established consumer
route, Sync may update only the existing consumer-owned generated `Entries`
region that the [Index Behavior Contract](../../index-candidate/behavior.md)
allows. The ordinary route chain and entrypoint must already exist. Authored
bytes outside the bounded generated region remain unchanged.

Sync never projects a source entrypoint, creates a route parent, materializes a
missing route chain, rewrites the Loader, or invents Framework semantics. If a
required existing generated region is missing, ambiguous, or unsafe, the
complete request blocks or is incomplete according to the established fact.

The intended record preserves the selected library's source root and replaces
its `paths` with the sorted current eligible set `P`, while retaining other
library records in sorted ID order. It has no extra field, expected target,
source bytes, absolute path, Git fact, dependency, collection, per-file remapping, glob,
or exclusion metadata. The record is published after all link and generated
effects verify.

## Consumer Permission

The repeatable `--allow-path <path>` explicitly authors shared `allowInstallPaths`
in `.agents/open-forge.json` after safe planning and before permission evaluation.
It persists in non-interactive execution; `--dry-run` never writes it. A refused
explicit write is reported and prevents content application. Eligible interactive
approval offers always, once or cancel. Once adds no persistent permission grant.
Invalid or unavailable required settings block the request, including destinations
inside `.agents/`.

This command selects [Workspace Permissions](../../shared/workspace-permissions/interface.md)
for the union of complete current mapped inventory and registered destinations, including unchanged links and retirements. `.agents/**` leaves remain implicit.
Requirements are destination paths shared by every Extension and Library; they carry no Library ID or source binding. Permission remains
necessary even for existing owned links; recorded identity makes removal
source-independent, without exempting it from revocation.

Live uncovered leaves propose their immediate parent folder; root and retirement-only leaves use exact grants.
Directory proposals explicitly include future descendants and never cover the
workspace root. Changing a Library source does not change destination permission. Protected paths,
source trees, ancestry, ownership and collision checks still apply per leaf.

Human prompt-capable application can approve the displayed scopes once or always; explicit `--allow-path` can persist shared grants without a prompt. JSON,
redirected execution and dry-run never prompt; missing or declined approval is
`blocked` and cancellation is `cancelled`, without effects. Invalid or unavailable settings block planning before approval because removal exclusions cannot be determined safely. Missing settings supply no saved exclusions or external grants; implicit paths require no grant.

`result.permissions` appears after `plan` and before `application`. It uses the
shared destination-string, scope and receipt coordinates exactly. Required
and missing arrays are concrete destinations; proposed/approved scopes expose
the requested scope using only `{kind,path}`. No rebinding field exists. Only a verified
outcome says permission was saved; later content failure retains that outcome.

Permission findings use the `library-sync.` prefix and suffixes
`permission-required`, `permission-declined`, `permission-invalid`,
`permission-unavailable`, `permission-changed` and `permission-write-failed`.
Changed lease-bound permission facts block; failed permission publication is
`failed` with its actual receipt; cancellation uses the existing `cancelled`
finding. No content effect proceeds after an unverified permission write.

## Dry Run And Application

`--dry-run` forms the same typed request, record and source facts, complete
inventory, current-versus-registered reconciliation, collision checks,
generated projection, intended record, ordered plan, expected-state facts, and
effect-free preflight as application. It reports every create, retirement,
generated-region change, record change, preserved drift, and blocker, then
writes nothing. It creates no directory, link, record, generated navigation,
recovery artifact, or lock, and performs no lease or recovery capability probe.

Omitting `--dry-run` selects application. Application completes source
inventory, all collision checks, and preflight before acquiring one
same-workspace lease for the effectful plan. Under that lease it revalidates
the record, source-root boundary, complete inventory, set relationship,
destination and parent states, generated regions, and expected record. Every
link, generated-region, and record effect receives an immediate no-follow
final-component check immediately before its effect.

Sync creates only missing exact relative file symlinks and removes only exact
registered raw-target links. It never resolves a link to delete its target and
never copies or writes source bytes. For an effectful application, typed
recovery is prepared and verified before the first effect; an effect-free plan
has no recovery bundle. Effects are monotonic and are not rolled back or
compensated after verification. A retained changed ordinary destination is not
an effect target and its bytes are never replaced or adopted. Independent safe
link and generated effects verify before the record is published last. A
noninteractive or redirected invocation that reaches final confirmation without
`--automatic` returns `invalid-input` (Invalid4) with
`library-sync.confirmation-required` and performs no effect; it never silently
applies the plan.

## Human Output

Every semantic result is rendered by the shared native report. --format text
is the default. The applicable global flags are --workspace <path>, --format
<text|json>, --detail <minimal|standard|full|debug>, repeatable
--detail-filter <error|warning|info|all>, --help, and --version. The default
detail is minimal; standard adds workspace and command context, full adds all
bounded facts, and debug adds bounded diagnostics on stderr. Detail does not
change semantics, effects, counts, or status. Filters select finding severities;
all is the default filter.

Library sync shows a plan review before final confirmation. --automatic bypasses the final confirmation only; without it the confirmation is Apply these changes? [y/N]. Permission prompts for new links remain separate.

When the final confirmation is required but the invocation is noninteractive or
redirected, Sync returns `invalid-input` (Invalid4), reports
`library-sync.confirmation-required`, and writes nothing. `--automatic` does not
bypass destination permission, ownership, collision, source-inventory, or
safety checks.

The catalogue text by detail level is:

`minimal`:

```text
Synchronized team-knowledge: 2 links added, 1 removed, 10 unchanged.
  Added    docs/new-a.md
  Added    docs/new-b.md
  Removed  docs/old.md   (its source file is gone)
  Updated the Entries section of docs/_docs.md
```

`minimal`, changed destination (stderr):

```text
Cannot synchronize team-knowledge: docs/review.md is no longer the link the Library created. Nothing was changed.
  It is now an ordinary file. Move it away or restore the link, then rerun.
Next: open-forge library inspect team-knowledge
```

At minimal detail, an incomplete result with independent safe effects names
the retained destination and shows those effects. Its headline is
`<id> could not be fully synchronized: <limitation>.`; it does not claim that
nothing changed. A restored registered link retains its warning alongside the
normal synchronization headline. Preview reports planned effects without claiming
application.

`standard` adds `Workspace:`, the unchanged links as one count line, targets
per changed link, and the lock row.

`full` adds the inventory, expected states, verification and recovery facts.

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

The command is exactly library sync; data follows the catalogue:

| Level    | `data`                                                                                |
| -------- | ------------------------------------------------------------------------------------- |
| minimal  | `{ mode, id, sourceFolder, destinationFolder, permissions { ... } }` plus the effects |
| standard | + `unchanged: [ path ]`, per effect `target`                                          |
| full     | + `inventory`, `expectedStates`, `verification`, `recovery` details                   |

Human and JSON output are projections of one typed result. data is null only at
the parser boundary before command binding. There is no alternate JSON
projection.

## Semantic Results

| Status                  | When                                                                      | Headline                                                                            | Exit | Stream |
| ----------------------- | ------------------------------------------------------------------------- | ----------------------------------------------------------------------------------- | ---: | ------ |
| completed               | nothing to change                                                         | `The <id> Library is up to date. Nothing to do.`                                    |    0 | stdout |
| completed               | changes other than a repaired current registered link                     | `Synchronized <id>: <A> links added, <R> removed, <U> unchanged.` (omit zero parts) |    0 | stdout |
| completed (dry run)     | planned without a warning                                                 | `Would synchronize <id>: <A> links to add, <R> to remove.`                          |    0 | stdout |
| completed               | no ownership record                                                       | `No ownership record exists, so <id> cannot be synchronized. Nothing was changed.`  |    0 | stdout |
| completed-with-warnings | a current registered link was missing and was restored; dry run retains the same warning without writes | `Synchronized <id>: <A> links added, <R> removed, <U> unchanged.` plus the retained warning row |    2 | stdout |
| completed-with-warnings | recovery bundle retained                                                  | + family row                                                                        |    2 | stdout |
| incomplete              | changed ordinary current registered destination retained while independent safe effects applied | `<id> could not be fully synchronized: <limitation>.` plus effect rows |    3 | stdout |
| incomplete              | source unavailable or inventory/fact unreadable                           | `<id> could not be synchronized: <source unavailable or limitation>. Nothing was changed.` |    3 | stdout |
| invalid-input           | bad or unknown ID, extra operand, or required final confirmation unavailable without `--automatic` | `Cannot synchronize <ref>: <problem>.`                              |    4 | stderr |
| blocked                 | changed destination with no independent safe effect, retired link missing, collision, permission, lock, or other unsafe mapping | `Cannot synchronize <id>: <reason>. Nothing was changed.`                           |    5 | stderr |
| failed                  | after effects                                                             | `Library sync stopped after <n> of <m> changes.`                                    |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                                                  | `Library sync was cancelled. Nothing was changed.`                                  |  130 | stderr |

### Current merged behavior and open questions

The accepted meaning treats a missing current registered link as safe repair:
the link may be recreated after complete preflight and the result is
completed-with-warnings (Attention2), including a dry-run warning with no
writes. A missing retired link remains blocked because its deletion cannot be
proven. The implementation qualification remains deferred.

The catalogue assigns lock-held to blocked with exit 5. The merged operation
returns failed with exit 1, operation-failed, and an IOException. This
implementation discrepancy remains deferred; it does not change the accepted
contract above.

## Errors And Boundaries

The finding catalogue is:

| Code                                         | Severity | Family                     | Message                                                                                                             | Next                              |
| -------------------------------------------- | -------- | -------------------------- | ------------------------------------------------------------------------------------------------------------------- | --------------------------------- |
| library-sync.invalid-input                   | error    | invalid-input              |                                                                                                                     |                                   |
| library-sync.confirmation-required           | error    | confirmation-required      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Sync/Shared/Wording/LibrarySyncWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-sync.confirmation-required`).                                                     | `open-forge library sync --automatic` |
| library-sync.invalid-id                      | error    | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Sync/Shared/Wording/LibrarySyncWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-sync.invalid-id`).                                                                                | `open-forge library list`         |
| library-sync.library-removed | error | local | The selected Library ID is excluded by workspace settings. | Remove the named ID from `removedLibraries` in `.agents/open-forge.json`, then rerun the command. |
| library-sync.path-excluded | info | local | The mapped destination is excluded and remains untouched. | none |
| library-sync.unknown-id                      | error    | unknown-id                 |                                                                                                                     | `open-forge library list`         |
| library-sync.ownership-observation           | info     | ownership-observation      |                                                                                                                     |                                   |
| library-sync.record-invalid                  | error    | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Sync/Shared/Wording/LibrarySyncWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-sync.record-invalid`).                                         | `open-forge doctor`               |
| library-sync.record-unavailable              | warning  | lifecycle-unavailable      |                                                                                                                     |                                   |
| library-sync.record-blocked                  | error    | lifecycle-blocked          |                                                                                                                     |                                   |
| library-sync.source-root-invalid             | error    | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Sync/Shared/Wording/LibrarySyncWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-sync.source-root-invalid`).                                                    | none                              |
| library-sync.source-root-unavailable         | warning  | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Sync/Shared/Wording/LibrarySyncWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-sync.source-root-unavailable`).                                                  | none                              |
| library-sync.source-root-blocked             | error    | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Sync/Shared/Wording/LibrarySyncWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-sync.source-root-blocked`).                                                          | none                              |
| library-sync.inventory-incomplete            | warning  | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Sync/Shared/Wording/LibrarySyncWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-sync.inventory-incomplete`).                                            | none                              |
| library-sync.mapping-unavailable             | warning  | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Sync/Shared/Wording/LibrarySyncWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-sync.mapping-unavailable`).                                                                   | `open-forge doctor`               |
| library-sync.mapping-blocked                 | error    | local                      | `<destination path> is <an ordinary file \| a folder \| a different link> and is not the link the Library created.` | `open-forge library inspect <id>` |
| library-sync.destination-collision           | error   | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Sync/Shared/Wording/LibrarySyncWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-sync.destination-collision`). | none                              |
| library-sync.retired-link-missing            | error   | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Sync/Shared/Wording/LibrarySyncWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-sync.retired-link-missing`).                                                       | `open-forge library inspect <id>` |
| library-sync.registered-link-restored       | warning | local                      |                                                                                                                     |                                   |
| library-sync.link-capability-unavailable     | error   | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Sync/Shared/Wording/LibrarySyncWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-sync.link-capability-unavailable`).                                                                                         | none                              |
| library-sync.consumer-blocked                | error   | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Sync/Shared/Wording/LibrarySyncWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-sync.consumer-blocked`).                                                                                | none                              |
| library-sync.ownership-conflict              | error   | ownership-conflict         |                                                                                                                                                    |                                   |
| library-sync.permission-required             | error   | permission-required        |                                                                                                                                                    |                                   |
| library-sync.permission-declined             | error   | permission-declined        |                                                                                                                                                    |                                   |
| library-sync.permission-invalid              | error   | permissions-invalid        |                                                                                                                                                    |                                   |
| library-sync.permission-unavailable          | warning | permissions-unavailable    |                                                                                                                                                    |                                   |
| library-sync.permission-changed              | error   | permissions-changed        |                                                                                                                                                    |                                   |
| library-sync.permission-write-failed         | error   | permission-write-failed    |                                                                                                                                                    |                                   |
| library-sync.generated-navigation-blocked    | error   | generated-region-unsafe    |                                                                                                                                                    |                                   |
| library-sync.generated-navigation-incomplete | warning | projection-unavailable     |                                                                                                                                                    |                                   |
| library-sync.lock-unavailable                | error   | workspace-lock-unavailable |                                                                                                                                                    |                                   |
| library-sync.recovery-unavailable            | warning | recovery-unavailable       |                                                                                                                                                    |                                   |
| library-sync.recovery-retained               | warning | recovery-artifact-retained |                                                                                                                                                    |                                   |
| library-sync.application-failed              | error   | write-failed               |                                                                                                                                                    |                                   |
| library-sync.verification-failed             | error   | verification-failed        |                                                                                                                                                    |                                   |
| library-sync.operation-failed                | error   | operation-failed           |                                                                                                                                                    |                                   |
| library-sync.interrupted                     | error   | interrupted                |                                                                                                                                                    |                                   |

Findings retain code, severity, family, message, subject, cause, and next
action when available. Counts are:

`linksAdded`, `linksRemoved`, `linksUnchanged`, `sectionsUpdated`.

## Scenarios

`up-to-date`, `links-added`, `links-removed`, `both`, `dry-run`,
`unknown-id` (invalid), `changed-occupant` (blocked when no independent safe
effect; incomplete when a current registered mapping is retained alongside
safe effects), `registered-link-gone` (completed-with-warnings),
`source-unreadable` (incomplete with no effects), `permission-required`,
`no-ownership-record` (info), `lock-held`, `write-failed-partial`, `cancelled`.

Prompt rules from the catalogue:

Permission for new links outside `.agents`; plan review; `Apply these
changes? [y/N]` unless `--automatic` (added by 04).

## Representative Transcripts

### completed

~~~text
The team-knowledge Library is up to date. Nothing to do.
~~~

### completed-with-warnings

~~~text
Synchronized team-knowledge: 1 link added.
  Warning  <recovery-bundle>  Recovery artifact retained
~~~

### incomplete

~~~text
team-knowledge could not be synchronized: some files under shared/team-knowledge could not be listed. Nothing was changed.
~~~

### invalid-input

~~~text
Cannot synchronize unknown: The Library ID is not registered.
Next: open-forge library list
~~~

### blocked

~~~text
Cannot synchronize team-knowledge: .agents/directives/review.md is no longer the link the Library created. Nothing was changed.
  It is now an ordinary file. Move it away or restore the link, then rerun.
Next: open-forge library inspect team-knowledge
~~~

### failed

~~~text
Library sync stopped after 1 of 2 changes.
  Added    .agents/directives/review.md
~~~

### cancelled

~~~text
Library sync was cancelled. Nothing was changed.
~~~

## Related Current Sources

- [library sync Contract Set](_sync.md)
- [library sync Interface Contract](interface.md)

## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`library.sync.help.syntax`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Library/Sync/LibrarySyncText.cs).

<!-- @OpenForgeTextRef library.sync.help.syntax -->

## Approved Journey Wording References

The following stable IDs link the approved journey behavior above to its typed
human-wording factories. Independently reviewed snapshots and state assertions
remain the output evidence.

- [LibrarySyncPhrases.cs](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Library/Sync/LibrarySyncPhrases.cs)
  <!-- @OpenForgeTextRef library.sync.phrase.could-not-be-fully-synchronized -->
