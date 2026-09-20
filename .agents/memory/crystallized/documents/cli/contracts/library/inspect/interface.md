---
open-forge:
  description: Accepted public interface for complete Library inventory and exact projection comparison
  responsibility: Define Library inspect syntax, lock ownership subject, inventory, comparisons, results, errors, and conformance
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, Inspect, Interface, ReadOnly, Workspace, CurrentTruth]
---

# library inspect Interface Contract

Unavailable ownership is reported as `library-inspect.ownership-observation`
with `completed` status. This finding grants no ownership or mutation permission.

## Status And Authority

This is the accepted current Crystallized Interface Contract for read-only
`open-forge library inspect`. It owns the public syntax, Library-ID subject,
ownership claim shape, complete source inventory, registered/observed comparison,
semantic statuses, output, errors, examples, and caller-visible conformance.
The command is implemented in the merged CLI.

The sibling [Behavior Contract](behavior.md) defines technology-neutral
resolution and result formation. The [Library group entrypoint](../_library.md)
defines group routing and exact help order. The [Library list Interface](../list/interface.md)
defines the shared record shape and bounded list observation. The shared [Global
CLI Flags](../../shared/global-flags/interface.md) define the six unchanged
global flags. The shared [Result Coordinates](../../shared/result-coordinates/interface.md)
define the schema-3 envelope, status exits, and output streams. The shared [CLI
Source References](../../shared/source-references/interface.md) define automatic
source-ID derivation; a Library ID remains outside that source-reference grammar.

The [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
defines shared realization detail without changing this public contract.

## Purpose And Boundary

`library inspect` explains one exact consumer-local Library record. It performs
a complete inventory of eligible ordinary files below that record's real
selected source root, derives their consumer-relative destinations, and
compares the complete observed projection with every registered path and its
expected relative file link.

Inspect distinguishes a healthy exact projection from safely observed additions,
retirements, missing links, and changed links. A complete safe comparison with
any such drift yields `completed-with-warnings`. An unavailable or incomplete record, source
root, inventory, or projection fact yields `incomplete`. Unsafe identity or
record or link ambiguity yields `blocked`. A missing or unknown inspect ID is
`invalid-input`.

The operation is read-only, deterministic, and stateless. It does not acquire a
lock, create recovery state, write any file, follow a destination link to read
source bytes, or invoke another command.

## Syntax And Subject

The complete public command form is:

```text
open-forge library inspect <library-id> [global flags]
```

Exactly one Library ID is required in domain mode. It matches:

```text
[a-z0-9]+(-[a-z0-9]+)*
```

The complete ID length is 1–128 characters. Matching is exact, case-sensitive,
and limited to the `id` members of the selected workspace record. A malformed,
missing, or unknown ID is `invalid-input`; duplicate or unsafe identity is `blocked`.

The operand is a Library management identity, not an automatic source ID and
not a source-reference operand. `.agents/...` path values, source IDs, and a
source-root path are not alternate forms for this subject. There is no `--all`,
source selector, inventory filter, link selector, or mutation flag.

The shared flags are:

```text
--workspace <path>
--format <text|json>
--detail <minimal|standard|full|debug>
--detail-filter <error|warning|info|all>
--help
--version
```

Their grammar, defaults, repetition, composition, terminal behavior, and no-op
rules remain in [Global CLI Flags](../../shared/global-flags/interface.md).

## Workspace And Ownership Record

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

## Source-Root Preconditions

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

## Registered And Observed Projection

The registered projection is the exact `paths` array for the selected Library.
The observed projection contains the complete eligible inventory's destination
paths and the no-follow observation of each corresponding consumer entry.

For each destination, Inspect records whether:

| Relation      | Meaning                                                                                                                               |
| ------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| `current`     | The eligible source exists, the path is registered, and the destination is the expected relative file link.                           |
| `added`       | The complete eligible source inventory contains a path absent from the registered record.                                             |
| `retired`     | The registered record contains a path absent from the complete eligible source inventory.                                             |
| `missing`     | The source path is eligible and registered, but its destination entry is absent.                                                      |
| `changed`     | The source path is eligible and registered, but the destination occupant or link target differs from the expected relative file link. |
| `unavailable` | A required source or destination fact cannot be established completely.                                                               |
| `blocked`     | Source, destination, link, containment, or mapping identity is unsafe or ambiguous.                                                   |

The exact comparison is set equality over canonical source and destination paths
plus exact expected-link text and observed relative-link identity. A complete
comparison has no `added`, `retired`, `missing`, or `changed` relation. It does
not rely on an order supplied by directory enumeration.

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

`minimal`, attention:

```text
team-knowledge needs a sync: 3 files differ between shared/team and docs.
  Warning  docs/new-a.md      not linked yet; new in the source folder
  Warning  docs/old.md        linked, but its source file is gone
  Warning  docs/review.md     missing
Next: open-forge library sync team-knowledge --dry-run
```

`standard` adds `Workspace:` and every file with its relation (`current`,
`new in the source folder`, `source file gone`, `missing`, `changed`).

`full` adds expected and observed link targets and the inventory count.

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

The command is exactly library inspect; data follows the catalogue:

| Level    | `data`                                                                                                                                                              |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ id, sourceFolder, destinationFolder, current: bool, files: [ { sourcePath, destinationPath, relation } ] }` (only non-current files at minimal; all at standard) |
| standard | all files                                                                                                                                                           |
| full     | + per file `expectedTarget`, `observedTarget`, `inventory { eligible, excluded }`                                                                                   |

Human and JSON output are projections of one typed result. data is null only at
the parser boundary before command binding. There is no alternate JSON
projection.

## Semantic Results

| Status                  | When                                             | Headline                                                                                         | Exit | Stream |
| ----------------------- | ------------------------------------------------ | ------------------------------------------------------------------------------------------------ | ---: | ------ |
| completed               | inventory equals the links                       | `<id> is current: <N> files from <source> are linked under <destination>.`                       |    0 | stdout |
| completed               | empty source and no links                        | `<id> is current. The source folder <source> has no eligible files and no links are registered.` |    0 | stdout |
| completed               | no ownership record                              | `No ownership record exists, so <id> cannot be inspected.`                                       |    0 | stdout |
| completed-with-warnings | additions, retirements, missing or changed links | `<id> needs a sync: <K> files differ between <source> and <destination>.`                        |    2 | stdout |
| incomplete              | source or record could not be scanned completely | `<id> could not be inspected completely: <limitation>.`                                          |    3 | stdout |
| invalid-input           | bad or unknown ID                                | `Cannot inspect <ref>: <problem>.`                                                               |    4 | stderr |
| blocked                 | unsafe mapping, containment or link              | `Cannot inspect <id>: <reason>.`                                                                 |    5 | stderr |
| failed                  | unexpected error                                 | `Library inspect stopped because of an unexpected error: <reason>.`                              |    1 | stderr |
| cancelled               | Ctrl+C                                           | `Library inspect was cancelled.`                                                                 |  130 | stderr |

### Current merged behavior and open questions

For a Library whose destination is the workspace root, the merged minimal
headline is team-knowledge is current: 1 file from shared/team is linked under ..
The catalogue examples use a named destination folder such as docs and do not
specify root-destination wording. Maintainer decision remains open.

The shared native renderer emits a title line and a message line for findings;
the catalogue examples show short-form finding lines. The shared renderer is the
current behavior and this contract does not choose alternate wording.

An unreadable source currently produces two inventory-incomplete findings. The
catalogue does not decide whether those observations should be collapsed.
Maintainer decision remains open.

## Errors And Boundaries

The finding catalogue is:

| Code                                    | Severity | Family                | Message                                                                              | Next                                     |
| --------------------------------------- | -------- | --------------------- | ------------------------------------------------------------------------------------ | ---------------------------------------- |
| library-inspect.invalid-id              | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Inspect/Shared/Wording/LibraryInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-inspect.invalid-id`).      | `open-forge library list`                |
| library-inspect.unknown-id              | error    | unknown-id            |                                                                                      | `open-forge library list`                |
| library-inspect.record-invalid          | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Inspect/Shared/Wording/LibraryInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-inspect.record-invalid`).          | `open-forge doctor`                      |
| library-inspect.record-unavailable      | warning  | lifecycle-unavailable |                                                                                      |                                          |
| library-inspect.record-blocked          | error    | lifecycle-blocked     |                                                                                      |                                          |
| library-inspect.ownership-observation   | info     | ownership-observation |                                                                                      |                                          |
| library-inspect.source-root-invalid     | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Inspect/Shared/Wording/LibraryInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-inspect.source-root-invalid`).                     | none                                     |
| library-inspect.source-root-unavailable | warning  | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Inspect/Shared/Wording/LibraryInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-inspect.source-root-unavailable`).       | none                                     |
| library-inspect.source-root-blocked     | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Inspect/Shared/Wording/LibraryInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-inspect.source-root-blocked`).                           | none                                     |
| library-inspect.inventory-incomplete    | warning  | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Inspect/Shared/Wording/LibraryInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-inspect.inventory-incomplete`). | none                                     |
| library-inspect.path-added              | warning  | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Inspect/Shared/Wording/LibraryInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-inspect.path-added`).                       | `open-forge library sync <id> --dry-run` |
| library-inspect.path-retired            | warning  | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Inspect/Shared/Wording/LibraryInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-inspect.path-retired`).                            | `open-forge library sync <id> --dry-run` |
| library-inspect.link-missing            | warning  | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Inspect/Shared/Wording/LibraryInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-inspect.link-missing`).                                                        | `open-forge library sync <id> --dry-run` |
| library-inspect.link-changed            | warning  | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Inspect/Shared/Wording/LibraryInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-inspect.link-changed`).                             | fix by hand                              |
| library-inspect.link-blocked            | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Library/Inspect/Shared/Wording/LibraryInspectWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`library-inspect.link-blocked`).                          | `open-forge doctor`                      |
| library-inspect.operation-failed        | error    | operation-failed      |                                                                                      |                                          |
| library-inspect.interrupted             | error    | interrupted           |                                                                                      |                                          |

Findings retain code, severity, family, message, subject, cause, and next
action when available. Counts are:

`sourceFiles`, `linksCurrent`, `filesAdded`, `filesRetired`, `linksMissing`, `linksChanged`.

## Scenarios

`current`, `added-source-files`, `retired-source-files`, `missing-links`,
`changed-links`, `empty-source`, `source-unreadable` (incomplete),
`unknown-id` (invalid), `no-ownership-record` (info), `invalid-id`, `blocked-mapping`.

## Representative Transcripts

### completed

~~~text
team-knowledge is current: 1 file from shared/team is linked under ..
Workspace: <workspace>
~~~

### completed-with-warnings

~~~text
team-knowledge needs a sync: 1 file differs between shared/team and ..
Workspace: <workspace>
  Warning  .agents/directives/review.md  Registered link is missing
         .agents/directives/review.md  missing
Next: open-forge library sync team-knowledge --dry-run
~~~

### incomplete

~~~text
team-knowledge could not be inspected completely: Some files under shared/team could not be listed, so the comparison could not finish.
Workspace: <workspace>
  Warning  .agents/directives/review.md  Source inventory is incomplete
         Some files under shared/team could not be listed, so the comparison could not finish.
  Warning  shared/team  Source inventory is incomplete
         Some files under shared/team could not be listed, so the comparison could not finish.
~~~

### invalid-input

~~~text
Cannot inspect unknown: No Library has the ID unknown.
Next: open-forge library list
~~~

### blocked

~~~text
Cannot inspect team-knowledge: .agents/linked/review.md  could not be checked safely: The Library destination parent is not a real ordinary directory.
Workspace: <workspace>
  Error  .agents/linked/review.md  Registered link is blocked
         .agents/linked/review.md  could not be checked safely: The Library destination parent is not a real ordinary directory.
~~~

### failed

~~~text
Library inspect stopped because of an unexpected error: <reason>.
~~~

### cancelled

~~~text
Library inspect was cancelled.
~~~

## Related Current Sources

- [library inspect Behavior Contract](behavior.md)
- [Library group entrypoint](../_library.md)
- [Library list Interface Contract](../list/interface.md)
- [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
- [Global CLI Flags Interface Contract](../../shared/global-flags/interface.md)
- [CLI Source References Interface Contract](../../shared/source-references/interface.md)
- [Shared Result Coordinates Interface Contract](../../shared/result-coordinates/interface.md)
- [CLI Architecture](../../../architecture.md)

## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`library.inspect.help.syntax`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Library/Inspect/LibraryInspectText.cs).

<!-- @OpenForgeTextRef library.inspect.help.syntax -->
