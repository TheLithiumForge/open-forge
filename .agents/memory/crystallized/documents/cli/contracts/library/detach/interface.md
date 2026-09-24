---
open-forge:
  description: Define the exact public syntax, source-independent link checks, verified effects, record, and results for `library detach`
  responsibility: Define what detach accepts, removes, preserves, rejects, and reports for one complete registered Workspace Library
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, Detach, Interface, Mutation, Recovery, Safety, CurrentTruth]
---

# library detach Interface Contract

A missing ownership lock is known empty. Invalid or unavailable required ownership blocks the request before any effects.

## Status And Authority

This is the current Crystallized Interface Contract for
`open-forge library detach`. It owns the public purpose, exact syntax, operand
grammar, source-independent link boundary, observable effects, record changes,
statuses, output, errors, examples, non-goals, and public verification.

The sibling [Behavior Contract](behavior.md) defines deterministic
technology-neutral record and mapping facts, verified planning,
preflight, application, verification, and recovery. The shared [Global CLI Flags](../../shared/global-flags/interface.md),
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

`library detach` removes the complete consumer projection for one registered
library and removes that library from the separate consumer record. It removes
only exact registered relative file symlinks and any permitted existing
generated `Entries` effects. It never removes, copies, moves, reads for
recovery, or writes through source bytes.

Detach is source-independent in the first release. It does not require the
recorded source root, source file, or link target to exist. An exact registered
relative file symlink may be removed even when it is dangling, provided its
no-follow parent and leaf identity and raw relative target still match the
recorded mapping. Strong typed recovery can recreate that same dangling link.

Detach is one whole-library request with no selector-based partial form. It
removes every exact registered link that can be proven and may release the
selected registration after safe remaining work. A positively missing
registered destination and a changed ordinary occupant are `Attention2`
observations: their bytes are preserved, but exact other links may be removed
and the selected registration may be released after permission, alternate-link,
directory, alias, conflicting-ownership, and other safety checks. An alternate
link, directory, alias, unsafe or unknown state, separately owned or conflicting
occupant, unavailable fact, or required permission failure blocks the request.
Only an exact registered link, including an exact dangling link, supplies a
deletable projection identity.

Removal also records the Library ID in `removedLibraries` in
`.agents/open-forge.json`. Verify this settings change before link deletion and
publish registration removal last. A valid unregistered ID can be excluded;
an absent, already excluded ID is a no-op. Restore by explicitly clearing the ID
and any covering destination exclusions, then attaching the library again.

## Syntax

```text
open-forge library detach <library-id> [--dry-run] [--automatic] [global flags]
```

The command path selects the detach operation. The shared [Global CLI Flags](../../shared/global-flags/interface.md)
contract defines `--workspace <path>`, `--format <text|json>`,
`--detail <minimal|standard|full|debug>`, repeatable
`--detail-filter <error|warning|info|all>`, `--help`, and `--version`; all six retain their shared spelling, composition, repetition,
terminal behavior, and output meaning.

`--dry-run` is the only preview spelling. `--help` and `--version` are terminal
forms and stop before workspace, record, mapping, or projection work.

Detach has no source-root operand, aliases, extra positional operands, `--force`,
`--yes`, `--apply`, collection selector, remapping flag, glob,
copy mode, saved plan, partial selector, or generic mutation dispatcher.

## Operand And Repetition

| Operand or flag     | Role                                      | Accepted value                                  | Omission and repetition                                                                                        |
| ------------------- | ----------------------------------------- | ----------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| `<library-id>`      | Select one library management identity | One value matching the library-ID grammar below | Required and singleton. An unregistered ID records the exclusion; an absent, already excluded ID is a no-op. |
| `--dry-run`         | Write policy                              | Boolean flag with no value                      | Application is selected when omitted. Repetition is accepted and idempotent.                                   |
| `--automatic`      | Confirmation policy                       | Boolean flag with no value                      | Final confirmation is required when omitted; this flag bypasses that confirmation only. Repetition is accepted and idempotent. |
| Shared global flags | Workspace and presentation                | Defined by the shared global contract           | Shared defaults and repetition rules apply.                                                                    |

The library ID supplies record selection only. It does not select a source
reference, route, or destination outside the selected workspace.

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

An absent lock is known empty and supplies no registration or link deletion
authority. Unreadable, nonordinary, malformed or uninterpretable ownership
prevents effects. The command never falls back to the old record or infers ownership
from matching links. A valid unregistered ID can still be excluded from future
attachment through `removedLibraries` in `.agents/open-forge.json`.
Exact relative-link targets derive from both recorded roots and each suffix.
The lock stores no target bytes or comparison hashes.

## Source Independence And Mapping Boundary

Detach validates recorded roots and source-relative paths without resolving or
enumerating a source root. It derives the recorded destinations and raw relative
links from those strings. Missing sources and exact dangling links are supported.
Current destination permission is still required, bound to the recorded source
identity, and cannot be supplied by recovery bytes or an old grant for another
source.

Each record keeps `sourceRoot`, `destinationRoot` and source-relative `paths`.
For a path `p`, its source is `sourceRoot/p`. Its consumer destination is `p`
when `destinationRoot` is `.`, otherwise `destinationRoot/p`. Derive the exact
raw relative file-link target from the destination parent to that source.
Root-level leaf destinations use the workspace root as their parent.

Detach removes only exact registered relative file symlinks. Every existing
parent must be a real ordinary directory with no linked or reparse ancestry;
a missing destination parent blocks the request. Detach creates no destination parents or links. Local
siblings and destination directories remain untouched.

Validate recorded path grammar and destination protection without resolving or
enumerating source content. Protect
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

## Exact Registered Occupants

For each recorded mapping, detach distinguishes these current destination
facts:

| Destination fact                                                                                                                                  | Detach treatment                                                                        |
| ------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------- |
| Exact registered relative file symlink with the derived raw target                                                                                | Plan one link deletion. The target may exist or be dangling; detach does not follow it. |
| Positively missing leaf with safe no-follow parents                                                                                               | `Attention2`; preserve the path and bytes, remove other exact links, and release the selected registration after remaining checks. |
| Changed ordinary file with safe no-follow parents                                                                                                 | `Attention2`; preserve the ordinary bytes, remove other exact links, and release the selected registration after remaining checks. |
| Directory, different link, junction, special entry, changed raw target, unsafe parent, alias, unknown state, or separately owned/conflicting occupant | Block the request and preserve every occupant and the record. |

An exact registered dangling link is still an exact registered link. Detach may
remove it even when both source and target are absent because raw target
identity, parent identity, and leaf identity are enough to prove the intended
consumer effect without following source bytes.

Detach does not sweep an unregistered link or any unlisted consumer path. A
matching link without the selected record's ownership evidence is not adopted.
Local sibling files remain untouched.

## Generated Navigation And Record Effects

When removing a projected child changes an already established consumer route,
detach may update only the existing consumer-owned generated `Entries` region
that the [Index Behavior Contract](../../index-candidate/behavior.md) permits.
The ordinary route chain and entrypoint must already exist. Authored bytes
outside the bounded generated region remain unchanged. If no route exposes a
projection, detach does not create one.

Detach never projects or materializes source entrypoints, creates route
parents, repairs a missing route chain, rewrites authored entrypoints, rewrites
the Loader, or invents Framework semantics. A required generated region that
is missing, ambiguous, changed, or unsafe blocks the complete request.

The intended lock removes only the selected Library registration and retains
other registrations in sorted ID order. Removing the final Library publishes
an empty Libraries section; the shared lock and its Framework and Extension
sections remain. Publication occurs last after every exact-link and generated
effect verifies. A positively missing or changed ordinary destination supplies
no deletion effect and never authorizes deletion of its bytes; it may still be
retained as an `Attention2` observation while safe remaining effects and the
selected-registration release complete. An unsafe, aliased, conflicting, or
otherwise unavailable occupant cannot authorize any effect.
Library registrations carry identity, source root, destination root, and paths;
link targets remain derived from those roots rather than stored baselines.

## Consumer Permission

The repeatable `--allow-path <path>` explicitly authors shared `allowInstallPaths`
in `.agents/open-forge.json` after safe planning and before permission evaluation.
It persists in non-interactive execution; `--dry-run` never writes it. A refused
explicit write is reported and prevents content application. Eligible interactive
approval offers always, once or cancel. Once saves no permission grant; removal
still records its persistent exclusion. Invalid or unavailable settings block planning before permission approval,
because removal exclusions cannot be determined safely. A missing settings file
means no saved exclusions or external grants. Implicit `.agents/` destinations
still require no permission grant.

This command selects [Workspace Permissions](../../shared/workspace-permissions/interface.md)
for every registered destination in the detach plan, including a positively
missing or changed ordinary destination whose bytes are retained. `.agents/**`
leaves remain implicit.
Requirements are destination paths shared by every Extension and Library; they carry no Library ID or source binding. Permission remains
necessary even for existing owned links; recorded identity makes removal
source-independent, without exempting it from revocation.

Every missing permission proposal is an exact file grant. Detach does not request future-folder authority or revoke saved grants.
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

Permission findings use the `library-detach.` prefix and suffixes
`permission-required`, `permission-declined`, `permission-invalid`,
`permission-unavailable`, `permission-changed` and `permission-write-failed`.
Changed lease-bound permission facts block; failed permission publication is
`failed` with its actual receipt; cancellation uses the existing `cancelled`
finding. No content effect proceeds after an unverified permission write.

## Dry Run And Application

`--dry-run` forms the same typed request, record facts, source-independent
derived targets, destination classifications, generated projection, intended
record, ordered plan, expected-state facts, and effect-free preflight as
application. It reports every exact link deletion, retained missing or changed
ordinary destination warning, generated-region effect, record effect, and
blocker, then writes nothing. It
creates no directory, link, record, generated navigation, recovery artifact,
or lock, and performs no lease or recovery capability probe.

Omitting `--dry-run` selects application. Application completes all mapping
checks and verified preflight before acquiring one same-workspace lease
for the effectful plan. Under that lease it revalidates the record, selected
ID, destination and parent states, generated regions, and expected record.
Every link, generated-region, and record effect receives an immediate
no-follow final-component check immediately before its effect.

The operation removes only exact registered relative file symlinks. It never
resolves a target to delete a source file, follows source bytes, copies a
source, or uses a copy fallback. For an effectful application, typed recovery
is prepared and verified before the first effect; an effect-free plan has no
recovery bundle. Effects are monotonic and are not rolled back or
compensated after verification. Link and generated effects verify before the
lock is published last. Final detach removes the selected registration while
retaining the lock with an empty Libraries section when appropriate. A
positively missing or changed ordinary destination remains untouched and is
reported as an `Attention2` warning.

## Human Output

Every semantic result is rendered by the shared native report. --format text
is the default. The applicable global flags are --workspace <path>, --format
<text|json>, --detail <minimal|standard|full|debug>, repeatable
--detail-filter <error|warning|info|all>, --help, and --version. The default
detail is minimal; standard adds workspace and command context, full adds all
bounded facts, and debug adds bounded diagnostics on stderr. Detail does not
change semantics, effects, counts, or status. Filters select finding severities;
all is the default filter.

Library detach shows a plan review before final confirmation. `--automatic`
bypasses the final confirmation only; without it the prompt is `Remove the <N>
links listed above? [y/N]`. Permission prompts for links outside `.agents`
remain separate. In a noninteractive invocation, when final confirmation is
required and `--automatic` is absent, the result is `Invalid4` with
`library-detach.confirmation-required`; no link or record effect is applied.

The catalogue text by detail level is:

`minimal`:

```text
Detached team-knowledge: removed 12 links under docs. Source files in shared/team were kept.
  Updated the Entries section of docs/_docs.md
```

At minimal detail, a positively absent registered destination produces a
visible warning on stdout alongside the detach headline. Only exact links
actually removed contribute to the removal count; the absent destination has
no delete effect. Selected registration release is reported from its verified
publication.

`standard` adds `Workspace:`, every removed link as a row, and the lock row
(`.agents/open-forge.lock.json  registration removed`).

`full` adds expected states, verification and recovery facts.

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

The command is exactly library detach; data follows the catalogue:

| Level    | `data`                                                                                                       |
| -------- | ------------------------------------------------------------------------------------------------------------ |
| minimal  | `{ mode, id, sourceFolder, destinationFolder, registrationRemoved: bool, permissions { ... } }` plus effects |
| standard | per effect `target`                                                                                          |
| full     | + `expectedStates`, `verification`, `recovery` details                                                       |

Human and JSON output are projections of one typed result. data is null only at
the parser boundary before command binding. There is no alternate JSON
projection.

## Semantic Results

| Status                  | When                                                   | Headline                                                                                    | Exit | Stream |
| ----------------------- | ------------------------------------------------------ | ------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | detached                                               | `Detached <id>: removed <N> links under <destination>. Source files in <source> were kept.` |    0 | stdout |
| completed               | zero links, removal already recorded and ownership readable | `Detached <id>. It had no links.`                                                        |    0 | stdout |
| completed               | zero links and new removal intent                      | `Recorded removal intent for Library <id>.`                                                 |    0 | stdout |
| completed (dry run)     | planned without a retained missing/changed-ordinary warning | `Would detach <id>: remove <N> links under <destination>.`                              |    0 | stdout |
| completed (dry run)     | zero links and new removal intent                      | `Would record removal intent for Library <id>.`                                             |    0 | stdout |
| completed               | no ownership record and ID already excluded            | `No ownership record exists, so <id> cannot be detached. Nothing was changed.`                |    0 | stdout |
| completed-with-warnings | positively missing or changed ordinary destination retained; or recovery bundle retained | `Detached <id>: removed <N> links under <destination>.` + warning rows; retained bytes are named |    2 | stdout |
| completed-with-warnings (dry run) | positively missing or changed ordinary destination observed | `Would detach <id>: remove <N> links under <destination>.` + warning rows; no writes |    2 | stdout |
| incomplete              | record, Entries or recovery unreadable                 | `<id> could not be detached: <limitation>. Nothing was changed.`                            |    3 | stdout |
| invalid-input           | malformed ID, extra operand, required final confirmation unavailable without `--automatic` | `Cannot detach <ref>: <problem>.`                                                           |    4 | stderr |
| blocked                 | alternate/different link, directory, alias, unsafe or unknown state, separately owned/conflicting occupant, permission, lock | `Cannot detach <id>: <reason>. Nothing was changed.`                                        |    5 | stderr |
| failed                  | after effects                                          | `Library detach stopped after <n> of <m> links were removed.`                               |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                               | `Library detach was cancelled. Nothing was changed.`                                        |  130 | stderr |

### Current merged behavior and open questions

The catalogue assigns lock-held to blocked with exit 5. The merged operation
returns failed with exit 1, operation-failed, and an IOException. Maintainer
decision remains open with the corresponding attach and sync questions.

The catalogue assigns record-invalid to incomplete with exit 3. The merged
operation returns blocked with exit 5. Maintainer decision remains open.

The accepted mapping boundary treats a changed ordinary destination as
`Attention2` with preserved bytes and safe remaining effects, while a
directory, alternate link, alias, unsafe state, or conflicting occupant stays
`blocked`. The result should retain the occupant kind; any implementation
qualification remains deferred.

The accepted noninteractive confirmation boundary is `Invalid4` with
`library-detach.confirmation-required` and no effects unless `--automatic` is
present. Any implementation qualification remains deferred.

The interrupted-after-effects native cause differs from the catalogue's
cancellation case. Maintainer decision remains open.

## Errors And Boundaries

The finding catalogue is:

| Code                                           | Severity | Family                     | Message                                                                                                 | Next                              |
| ---------------------------------------------- | -------- | -------------------------- | ------------------------------------------------------------------------------------------------------- | --------------------------------- |
| library-detach.invalid-input                   | error    | invalid-input              |                                                                                                         |                                   |
| library-detach.confirmation-required           | error    | confirmation-required      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Detach/Shared/Wording/LibraryDetachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-detach.confirmation-required`).                                       | `open-forge library detach --automatic` |
| library-detach.invalid-id                      | error    | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Detach/Shared/Wording/LibraryDetachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-detach.invalid-id`).                                                                    | `open-forge library list`         |
| library-detach.unknown-id                      | error    | unknown-id                 |                                                                                                         | `open-forge library list`         |
| library-detach.ownership-observation           | info     | ownership-observation      |                                                                                                         |                                   |
| library-detach.record-invalid                  | error    | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Detach/Shared/Wording/LibraryDetachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-detach.record-invalid`).                             | `open-forge doctor`               |
| library-detach.record-unavailable              | warning  | lifecycle-unavailable      |                                                                                                         |                                   |
| library-detach.record-blocked                  | error    | lifecycle-blocked          |                                                                                                         |                                   |
| library-detach.registered-link-missing         | warning  | local                      | The registered destination is positively absent; no bytes were removed, safe exact links may still be removed, and registration may be released. | `open-forge library inspect <id>` |
| library-detach.mapping-blocked                 | error    | local                      | `<path> is <an ordinary file \| a folder \| a different link> and is not the link the Library created.` Ordinary-file drift is an `Attention2` retained-destination warning; other kinds block. | `open-forge library inspect <id>` |
| library-detach.destination-protected | error | local | `<path> is protected, owned by the source, or registered to another Library.` | `open-forge library list` |
| library-detach.mapping-unavailable             | warning  | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Detach/Shared/Wording/LibraryDetachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-detach.mapping-unavailable`).                                                  | `open-forge doctor`               |
| library-detach.link-capability-unavailable     | error    | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Detach/Shared/Wording/LibraryDetachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-detach.link-capability-unavailable`).                                               | none                              |
| library-detach.consumer-blocked                | error    | local                      | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Detach/Shared/Wording/LibraryDetachWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-detach.consumer-blocked`).                                      | none                              |
| library-detach.ownership-conflict              | error    | ownership-conflict         |                                                                                                         |                                   |
| library-detach.permission-required             | error    | permission-required        |                                                                                                         |                                   |
| library-detach.permission-declined             | error    | permission-declined        |                                                                                                         |                                   |
| library-detach.permission-invalid              | error    | permissions-invalid        |                                                                                                         |                                   |
| library-detach.permission-unavailable          | warning  | permissions-unavailable    |                                                                                                         |                                   |
| library-detach.permission-changed              | error    | permissions-changed        |                                                                                                         |                                   |
| library-detach.permission-write-failed         | error    | permission-write-failed    |                                                                                                         |                                   |
| library-detach.generated-navigation-blocked    | error    | generated-region-unsafe    |                                                                                                         |                                   |
| library-detach.generated-navigation-incomplete | warning  | projection-unavailable     |                                                                                                         |                                   |
| library-detach.lock-unavailable                | error    | workspace-lock-unavailable |                                                                                                         |                                   |
| library-detach.recovery-unavailable            | warning  | recovery-unavailable       |                                                                                                         |                                   |
| library-detach.recovery-retained               | warning  | recovery-artifact-retained |                                                                                                         |                                   |
| library-detach.application-failed              | error    | write-failed               |                                                                                                         |                                   |
| library-detach.verification-failed             | error    | verification-failed        |                                                                                                         |                                   |
| library-detach.operation-failed                | error    | operation-failed           |                                                                                                         |                                   |
| library-detach.interrupted                     | error    | interrupted                |                                                                                                         |                                   |

Findings retain code, severity, family, message, subject, cause, and next
action when available. Counts are:

`linksRemoved`, `sectionsUpdated`.

## Scenarios

`detached`, `detached-no-links`, `dry-run`, unregistered-ID exclusion,
`registered-link-gone` (completed-with-warnings, `Attention2`),
`changed-occupant` (ordinary file: completed-with-warnings, `Attention2`; unsafe
or alternate occupant: blocked),
`permission-required`, `no-ownership-record` (info), `lock-held`,
`write-failed-partial`, `cancelled`.

Prompt rules from the catalogue:

Permission when links outside `.agents` are removed; plan review listing
every link; `Remove the <N> links listed above? [y/N]` unless `--automatic`
(added by 04).

## Representative Transcripts

### completed

~~~text
Detached team-knowledge: removed 1 link under .. Source files in shared/team-knowledge were kept.
~~~

### completed-with-warnings

~~~text
Detached team-knowledge: removed 1 link under ..
  Warning  <recovery-bundle>  Recovery artifact retained
~~~

### incomplete

~~~text
team-knowledge could not be detached: <limitation>. Nothing was changed.
~~~

### invalid-input

Malformed Library IDs are invalid input. A valid unregistered ID records an exclusion without deleting content.

### blocked

~~~text
Cannot detach team-knowledge: .agents/directives/review.md is an ordinary file and is not the link the Library created. Nothing was changed.
Workspace: <workspace>
Next: open-forge library inspect team-knowledge
~~~

### failed

~~~text
Library detach stopped after 0 of 1 links were removed.
Workspace: <workspace>
Next: open-forge library detach team-knowledge --detail debug
~~~

### cancelled

~~~text
Library detach was cancelled. Nothing was changed.
Workspace: <workspace>
Next: open-forge library detach team-knowledge
~~~

## Related Current Sources

- [library detach Contract Set](_detach.md)
- [library detach Interface Contract](interface.md)

## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`library.detach.help.syntax`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Library/Detach/LibraryDetachText.cs).

<!-- @OpenForgeTextRef library.detach.help.syntax -->

## Approved Journey Wording References

The following stable IDs link the approved journey behavior above to its typed
human-wording factories. Independently reviewed snapshots and state assertions
remain the output evidence.

- [LibraryDetachWording.cs](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Library/Detach/LibraryDetachWording.cs)
  <!-- @OpenForgeTextRef library.detach.wording.registered-link-is-already-absent -->
  <!-- @OpenForgeTextRef library.detach.wording.changed-ordinary-destination-was-kept -->
