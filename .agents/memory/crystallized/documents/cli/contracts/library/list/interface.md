---
open-forge:
  description: Accepted public interface for bounded read-only Library list observations
  responsibility: Define Library list syntax, lock ownership input, source-root and link states, results, and conformance
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, List, Interface, ReadOnly, Workspace, CurrentTruth]
---

# library list Interface Contract

Unavailable ownership is reported as `library-list.ownership-observation`
with `completed` status. This finding grants no ownership or mutation permission.

## Status And Authority

This is the accepted current Crystallized Interface Contract for read-only
`open-forge library list`. It owns the public syntax, lock ownership input,
bounded observation result, semantic statuses, output, errors, examples, and
caller-visible conformance for this command. The command is implemented in the merged CLI.

The sibling [Behavior Contract](behavior.md) defines technology-neutral
resolution and result formation. The [Library group entrypoint](../_library.md)
defines group routing and the exact command order. The shared [Global CLI Flags](../../shared/global-flags/interface.md)
define the six unchanged global flags. The shared [Result Coordinates](../../shared/result-coordinates/interface.md)
define the schema-3 envelope, status exits, and output streams. The shared [CLI
Source References](../../shared/source-references/interface.md) define automatic
source IDs and canonical `.agents` paths; this command does not create another
source-ID grammar.

The [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
defines shared realization detail without changing this public contract.

## Purpose And Boundary

`library list` answers which consumer-local Library records are readable and
which registered destination links are currently observable. It reads one
bounded `.agents/open-forge.lock.json` record, checks each recorded source
root, and observes each registered destination entry without reading its link
target's file bytes.

The command does not perform a complete source inventory. It cannot report
source additions or retirements that are absent from the record. A safely
observed missing or changed registered link is projection drift and yields
`completed-with-warnings`; it is not a request to repair that link. An unavailable or
incomplete fact yields `incomplete`, and an unsafe identity or ambiguous record
or link yields `blocked`.

The operation is read-only, deterministic, and stateless. It does not acquire a
lock, create a recovery artifact, write any file, follow a link to inspect its
target, or invoke another command.

## Syntax

The complete public command form is:

```text
open-forge library list [global flags]
```

There are no domain operands and no operation-specific flags. The command does
not accept a Library ID, a source ID, a source-reference path, a source-root
operand, or a write-policy flag. The six shared global flags are:

```text
--workspace <path>
--format <text|json>
--detail <minimal|standard|full|debug>
--detail-filter <error|warning|info|all>
--help
--version
```

Their spelling, values, defaults, repetition, terminal behavior, and no-op
rules remain in [Global CLI Flags](../../shared/global-flags/interface.md).
`--help` and `--version` stop before domain work. A well-formed shared flag with
no applicable behavior remains a shared no-op.

## Workspace And Record

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
provides no usable registrations and yields a `completed` ownership observation.
It is never reported as a valid empty record: the record state remains `missing`,
`unavailable`, or `invalid-input`, with unavailable counts and no selected paths.
List returns no registrations; Inspect, Sync, and Detach select nothing and do
not invent an unknown-ID error. Their `ownership-observation` finding explains
why. A valid readable lock with no matching requested ID still yields `invalid-input`.
Attach may verify new effects from its explicit source and destination inputs
and publish ownership best-effort after verification. Read-only operations never
reconstruct or write a lock. Matching files and old records create no claims.

## Source-Root State

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

## Registered-Link Observation

For every path in a Library's `paths` array, the command observes only the
destination directory entry derived from that source suffix and the recorded destination root. It does not follow the
destination to read source bytes or to discover unregistered files.

The observation is one of:

| State         | Meaning                                                                                                               |
| ------------- | --------------------------------------------------------------------------------------------------------------------- |
| `current`     | The destination is a relative file link and its stored target text exactly equals the derived expected relative link. |
| `missing`     | No destination entry is present.                                                                                      |
| `changed`     | A destination entry is safely observable but is not the expected relative file link.                                  |
| `unavailable` | The destination fact cannot be read completely.                                                                       |
| `blocked`     | Link type, identity, containment, or alias meaning is unsafe or ambiguous.                                            |
| `not-started` | An earlier boundary prevented observation.                                                                            |

`sourceId` in a path observation is the normal destination-derived automatic
source ID when the destination is an eligible `.agents` file. It is retained
separately from `library.id`; the Library ID is never substituted for it.

## Human Output

Every semantic result is rendered by the shared native report. --format text
is the default. The applicable global flags are --workspace <path>, --format
<text|json>, --detail <minimal|standard|full|debug>, repeatable
--detail-filter <error|warning|info|all>, --help, and --version. The default
detail is minimal; standard adds workspace and per-record state, full adds
expected and observed targets, and debug adds bounded diagnostics on stderr.
Detail does not change semantics, counts, ordering, or status. Filters select
finding severities; all is the default filter.



The catalogue text by detail level is:

`minimal`, current:

```text
team-knowledge    shared/team -> docs   12 links current
```

`minimal`, attention:

```text
1 Library registered. 1 link needs attention.
  team-knowledge    shared/team -> docs   11 links current, 1 missing
    Warning  docs/review.md   missing
Next: open-forge library sync team-knowledge
```

`standard` adds `Workspace:` and every registered link with its state.

`full` adds expected and observed link targets per link and the record
coverage sentence.

Completed and completed-with-warnings results use stdout; incomplete results
also use stdout. Invalid-input, blocked, failed, and cancelled results use
stderr. A parser failure is text on stderr without a result envelope.

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

The command is exactly library list; data follows the catalogue:

| Level    | `data`                                                                                                         |
| -------- | -------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ libraries: [ { id, sourceFolder, destinationFolder, links { current, missing, changed, unavailable } } ] }` |
| standard | + per Library `links: [ { path, state } ]`                                                                     |
| full     | + per link `expectedTarget`, `observedTarget`, `sourceId`; `recordCoverage`                                    |

Human and JSON output are projections of one typed result. data is null only at
the parser boundary before command binding. There is no alternate JSON
projection.

## Semantic Results

| Status                  | When                                                                       | Text                                                                                    | Exit | Stream |
| ----------------------- | -------------------------------------------------------------------------- | --------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | none registered                                                            | `No Libraries are registered.` + `Next: open-forge library attach <id> <source-folder>` |    0 | stdout |
| completed               | all links current                                                          | rows                                                                                    |    0 | stdout |
| completed               | no ownership record                                                        | `No ownership record exists, so Libraries cannot be listed from it.` (Info; exit 0)     |    0 | stdout |
| completed-with-warnings | a registered link is missing or changed, or a source folder cannot be read | `<N> Libraries registered. <K> links need attention.` then rows                         |    2 | stdout |
| incomplete              | record or a link fact unreadable                                           | rows plus warning rows                                                                  |    3 | stdout |
| invalid-input           | bad flag, invalid record shape                                             | `Cannot list Libraries: <problem>.`                                                     |    4 | stderr |
| blocked                 | unsafe identity, containment or link                                       | `Cannot list Libraries: <reason>.`                                                      |    5 | stderr |
| failed                  | unexpected error                                                           | `Library list stopped because of an unexpected error: <reason>.`                        |    1 | stderr |
| cancelled               | Ctrl+C                                                                     | `Library list was cancelled.`                                                           |  130 | stderr |



## Errors And Boundaries

The finding catalogue is:

| Code                                 | Severity | Family                | Message                                                                                              | Next                              |
| ------------------------------------ | -------- | --------------------- | ---------------------------------------------------------------------------------------------------- | --------------------------------- |
| library-list.invalid-record          | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/List/Shared/Wording/LibraryListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-list.invalid-record`).                          | `open-forge doctor`               |
| library-list.record-unavailable      | warning  | lifecycle-unavailable | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/List/Shared/Wording/LibraryListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-list.record-unavailable`). | `open-forge doctor`               |
| library-list.record-blocked          | error    | lifecycle-blocked     |                                                                                                      |                                   |
| library-list.ownership-observation   | info     | ownership-observation |                                                                                                      |                                   |
| library-list.source-root-invalid     | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/List/Shared/Wording/LibraryListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-list.source-root-invalid`).                           | `open-forge library inspect <id>` |
| library-list.source-root-unavailable | warning  | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/List/Shared/Wording/LibraryListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-list.source-root-unavailable`).                                                 | `open-forge library inspect <id>` |
| library-list.source-root-blocked     | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/List/Shared/Wording/LibraryListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-list.source-root-blocked`).                                 | none                              |
| library-list.link-missing            | warning  | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/List/Shared/Wording/LibraryListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-list.link-missing`).                                                                                    | `open-forge library sync <id>`    |
| library-list.link-changed            | warning  | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/List/Shared/Wording/LibraryListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-list.link-changed`).                                                         | `open-forge library inspect <id>` |
| library-list.link-unavailable        | warning  | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/List/Shared/Wording/LibraryListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-list.link-unavailable`).                                                                       | `open-forge doctor`               |
| library-list.link-blocked            | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/List/Shared/Wording/LibraryListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-list.link-blocked`).                                                      | `open-forge doctor`               |
| library-list.operation-failed        | error    | operation-failed      |                                                                                                      |                                   |
| library-list.interrupted             | error    | interrupted           |                                                                                                      |                                   |

Findings retain code, severity, family, message, subject, cause, and next
action when available. Counts are:

`libraries`, `linksCurrent`, `linksMissing`, `linksChanged`, `linksUnavailable`.

## Scenarios

`none-registered`, `one-current`, `link-missing`, `link-changed`,
`no-ownership-record` (info), `source-folder-missing` (warnings),
`record-invalid` (invalid), `record-unreadable` (incomplete), `link-blocked`
(blocked), `invalid-input`.

## Representative Transcripts

### completed

~~~text
Workspace: <workspace>
  team-knowledge  shared/team -> .  1 link current
~~~

### completed-with-warnings

~~~text
1 Library registered. 1 link needs attention.
Workspace: <workspace>
  Warning  .agents/directives/review.md  Registered link is missing
         .agents/directives/review.md  missing
  team-knowledge  shared/team -> .  0 links current, 1 missing
Next: open-forge library sync team-knowledge
~~~

### incomplete

~~~text
Library registration could not be checked completely.
Workspace: <workspace>
  Warning  .agents/open-forge.lock.json  Library record is unavailable
         The Library section of .agents/open-forge.lock.json could not be read.
  libraries: .agents/open-forge.lock.json could not be read.
  links current: .agents/open-forge.lock.json could not be read.
  links missing: .agents/open-forge.lock.json could not be read.
  links changed: .agents/open-forge.lock.json could not be read.
  links unavailable: .agents/open-forge.lock.json could not be read.
Next: open-forge doctor
~~~

### invalid-input

~~~text
Cannot list Libraries: 'm' is an invalid start of a value. LineNumber: 0 | BytePositionInLine: 0.
Workspace: <workspace>
  Error  .agents/open-forge.lock.json  Library record is invalid
         The Library section of .agents/open-forge.lock.json is invalid: 'm' is an invalid start of a value. LineNumber: 0 | BytePositionInLine: 0.
  libraries: 'm' is an invalid start of a value. LineNumber: 0 | BytePositionInLine: 0.
  links current: 'm' is an invalid start of a value. LineNumber: 0 | BytePositionInLine: 0.
  links missing: 'm' is an invalid start of a value. LineNumber: 0 | BytePositionInLine: 0.
  links changed: 'm' is an invalid start of a value. LineNumber: 0 | BytePositionInLine: 0.
  links unavailable: 'm' is an invalid start of a value. LineNumber: 0 | BytePositionInLine: 0.
Next: open-forge doctor
~~~

### blocked

~~~text
Cannot list Libraries: The Library destination parent is not a real ordinary directory.
Workspace: <workspace>
  Error  .agents/linked/review.md  Registered link is blocked
         .agents/linked/review.md  could not be checked safely: The Library destination parent is not a real ordinary directory.
  team-knowledge  shared/team -> .  0 links current, 1 unavailable
Next: open-forge doctor
~~~

### failed

~~~text
Library list stopped because of an unexpected error: <reason>.
~~~

### cancelled

~~~text
Library list was cancelled.
~~~

## Related Current Sources

- [library list Behavior Contract](behavior.md)
- [Library group entrypoint](../_library.md)
- [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
- [Global CLI Flags Interface Contract](../../shared/global-flags/interface.md)
- [CLI Source References Interface Contract](../../shared/source-references/interface.md)
- [Shared Result Coordinates Interface Contract](../../shared/result-coordinates/interface.md)
- [CLI Architecture](../../../architecture.md)

## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`library.list.help.syntax`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Library/List/LibraryListText.cs).

<!-- @OpenForgeTextRef library.list.help.syntax -->
