---
open-forge:
  description: Accepted read-only Interface for listing installed and available Extension packages from one exact source universe
  responsibility: Define extension list syntax, Installed and Available sections, trust states, source handling, results, and errors
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, List, Interface, ReadOnly, Source, Lifecycle, CurrentTruth]
---

# extension list Interface Contract

## Status And Authority

This is the accepted current Crystallized Interface Contract for
`open-forge extension list`. It owns the public syntax, source selection,
section filters, installed lifecycle facts, available package facts, output,
semantic results, errors, examples, non-goals, and public conformance. The
command does not ship yet.

The sibling [Behavior Contract](behavior.md) defines deterministic,
technology-neutral read-only behavior. The [Extension group entrypoint](../_extension.md)
defines only routing and help. Shared Global Flags define workspace and
presentation flags once.

## Purpose And Boundary

`list` answers which Extension packages are installed and which are available.
It keeps those facts in separate sections. Availability never implies
installation, a matching ID never proves ownership, and an installed fact does
not disappear because its package source is unavailable.

`list` is stateless, read-only, deterministic, and non-shipping. It does not
inspect package contents beyond the selected source facts needed for the list,
does not form a mutation plan, and does not replace `status` or `doctor`.

## Syntax

```text
open-forge extension list [--installed] [--available] [--source <package-or-catalogue-path>] [global flags]
```

`list` has no operands, `--all`, `--automatic`, `--dry-run`, `--force`,
`--prune`, wizard, or mutation flag. The shared flags are:

```text
--workspace <path>
--format <text|json>
--detail <minimal|standard|full|debug>
--detail debug
--help
--version
```

All shared grammar, defaults, repetition, composition, terminal, and error rules
remain in [Global CLI Flags](../../shared/global-flags/interface.md). A
well-formed global flag with no applicable behavior is a shared no-op.

## Source And Workspace

The workspace is the exact current directory unless `--workspace` selects one
exact directory. Installed facts come from the selected workspace's recognized
ownership receipts in `.agents/open-forge.lock.json`. The operation
does not discover another root. A missing document or section is not, by itself,
proof of an empty installed set; complete inspection is required before
reporting safe absence.

`--source` is one exact external read location. Structural package and catalogue
facts distinguish one package directory from one catalogue directory. The CLI
does not use ambient search, a network, registry, cache, glob, fuzzy matching,
resemblance, or a fallback source. The source is read-only and must be lexically
and physically disjoint from the target workspace. Source overlap, aliases, and
containment in either direction are blocked.

When `--source` is omitted, available facts come from the embedded catalogue.
Installed facts still come from the exact selected workspace and do not require
the selected package source to be available.

The CLI distribution embeds Framework and first-party Extension assets with
deterministic inventory and hash proof. That proof identifies distributed source
assets; it is not evidence of a selected workspace's current installation or of
a proven runtime implementation.

## Section Filters

| Input         | Meaning                                   | Omission and composition                                                                                                                                                                                                                                               |
| ------------- | ----------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `--installed` | Render only the Installed section         | Omitted means include Installed unless `--available` is the only filter. Repeating is invalid under singleton value-free selection rules unless the command's Boolean repetition rule accepts idempotence; the command accepts repeated Boolean presence idempotently. |
| `--available` | Render only the Available section         | Omitted means include Available unless `--installed` is the only filter. It composes with `--installed` back to both sections.                                                                                                                                         |
| `--source`    | Select one exact available-package source | Omitted selects the embedded catalogue. It is a singleton and repetition is invalid.                                                                                                                                                                                   |

With neither section flag, both sections are rendered. `--installed` alone
renders Installed. `--available` alone renders Available. Both flags together
render both. There is no last-wins rule.

## Ownership Observation

The existing `coverage.lifecycleTrust` field reports the lock observation:

- `trusted`: a complete, unambiguous Extension ownership observation;
- `incomplete`: missing, malformed, unreadable or ambiguous ownership; and
- `absent`: a complete observation contains no recorded Extension packages.

Only established receipts produce Installed rows. An unavailable lock retains
incomplete installed coverage and empty rows, with the informational
`extension-list.ownership-observation` finding (`severity: info`). Its subject
is `.agents/open-forge.lock.json`; its cause explains the unavailable claims.
The finding does not raise aggregate status or select a recovery action. Empty
rows do not establish that manually copied packages are absent. Counts count
emitted rows; coverage remains independent.

The lock is the only state input. Old state files are unrelated user content,
and List neither reads nor changes them. No persisted hashes, workspace binding,
or payload-derived adoption participate in the observation. An unavailable
package source never erases an installed row and keeps its existing finding.

## Human Output

The command uses the shared native report. The default detail is `minimal`; `standard`, `full` and `debug` add the catalogue-defined facts. `--detail-filter <error|warning|info|all>` is repeatable and changes only the rendered detail. Use `--format text` for this text report. Primary result text for `completed`, `completed-with-warnings` and `incomplete` is on stdout; primary errors for `invalid-input`, `blocked`, `failed` and `cancelled` are on stderr. There is no `Status:` line.

### Statuses and headlines

| Status                  | When                                                                          | Text                                                                                                                                       | Exit | Stream |
| ----------------------- | ----------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ | ---: | ------ |
| completed               | sections listed                                                               | the two blocks below                                                                                                                       |    0 | stdout |
| completed               | no ownership record                                                           | blocks; the Installed block reads `Installed  (no ownership record, so installed packages cannot be listed)` and an Info finding at `full` |    0 | stdout |
| completed-with-warnings | an installed package's source is missing, or its files changed or are missing | rows with a note, warning rows                                                                                                             |    2 | stdout |
| incomplete              | the source or a manifest could not be read                                    | the safe rows plus warning rows                                                                                                            |    3 | stdout |
| invalid-input           | bad flag, invalid `--source` package                                          | `Cannot list Extensions: <problem>.`                                                                                                       |    4 | stderr |
| blocked                 | source overlaps the workspace, unsafe path                                    | `Cannot list Extensions: <reason>.`                                                                                                        |    5 | stderr |
| failed                  | unexpected error                                                              | `Extension list stopped because of an unexpected error: <reason>.`                                                                         |    1 | stderr |
| cancelled               | Ctrl+C                                                                        | `Extension list was cancelled.`                                                                                                            |  130 | stderr |

### Selected manifest failures

When the selected package or catalogue cannot be read because one manifest
fails, keep the selected source identity and name the exact failed manifest in
the error. When Available is selected, the headline is
`Available Extensions could not be listed from <path>.` Installed-only selection
keeps its existing headline; the precise finding and recovery instruction remain.
The finding and available-packages explanation use these known facts:

| Reason            | Message                                                           | Next instruction                                             |
| ----------------- | ----------------------------------------------------------------- | ------------------------------------------------------------ |
| Invalid manifest  | `<path> is not a valid Extension manifest.`                       | Check the manifest's required fields and values, then retry. |
| Invalid UTF-8     | `<path> is not valid UTF-8.`                                      | Save the manifest as UTF-8, then retry.                      |
| Access denied     | `<path> could not be read: permission was denied.`                | Check read access to the named file, then retry.             |
| File in use       | `<path> could not be read because it is in use.`                  | Close the program holding the file, then retry.              |
| Other I/O failure | `<path> could not be read because a filesystem operation failed.` | Check that the file is accessible, then retry.               |

These instructions are sentence actions, not executable commands. Invalid
manifest includes required-field/value failures, not only invalid JSON syntax.
Do not infer a sharing failure from arbitrary I/O errors. Keep raw diagnostic
causes at their existing full/debug surfaces. Existing codes, statuses, selected
source fields, streams, JSON schema and read-only behavior remain unchanged.
Failures without these manifest facts retain their existing messages.

### Text by level

`minimal`:

```text
Installed
  development 0.1.0

Available (bundled with this CLI)
  development-toolkit 0.1.0   An optional bundle of project documents, Memory starters, planning, and development packages   (5 packages)
  memory-starters 0.1.0       Copy-ready Memory Templates for decisions, ideas, analyses, observations, and handoffs
  orchestration 0.1.0         Coordinate dependent tasks through one optional managed-delivery workflow   (3 packages)
  planning 0.1.0              An optional planning Workflow, Work Records Pattern, and Templates for tasks, plans, backlogs, and checkpoints
  project-documents 0.1.0     Optional Vision and Architecture Workflows with document Templates for a project's direction and structure
Next: open-forge extension install <id>
```

`Installed  none` when nothing is installed. The `Available` heading names
the source: `(bundled with this CLI)` or `(from <path>)`. An installed
package already in the available list is shown once in each block; the
Available row adds `installed` when versions match or `installed: 0.1.0`
when they differ. A package with `(N packages)` bundles that many.

`standard` adds `Workspace:`, the source path, and per Available row its
dependencies (`needs: planning, project-documents`).

`full` adds per Installed row the file count and the recorded source, and
the lock coverage sentence.

### Representative transcripts by status

### Transcript — completed

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-list-completed).

### Transcript — completed-with-warnings

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-list-completed-with-warnings).

### Transcript — incomplete

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-list-incomplete).

### Transcript — invalid-input

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-list-invalid-input). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/List/__snapshots__/ExtensionListBeforeOutputSnapshotTests/InvalidBoundary/invalid-input.minimal.txt).

### Transcript — blocked

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-list-blocked). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/List/__snapshots__/ExtensionListBeforeOutputSnapshotTests/InvalidBoundary/source-blocked.minimal.txt).

### Transcript — failed

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-list-failed).

### Transcript — cancelled

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-list-cancelled).

## Structured Output

`--format json` writes one schema-3 envelope to stdout for every report status. It contains the command, status, workspace when applicable, detail, filter, command data, findings, effects, counts, limitations, recovery facts and next action as applicable. It is the same typed result as the text report; no ordinary text is mixed into the JSON document. If parsing fails before binding, the raw parser diagnostic remains text on stderr and no report envelope exists.

### JSON data by level

| Level    | `data`                                                                                                                         |
| -------- | ------------------------------------------------------------------------------------------------------------------------------ |
| minimal  | `{ source { kind, path }, installed: [ { id, version, note } ], available: [ { id, version, name, description, packages } ] }` |
| standard | + per available `dependencies: [...]`                                                                                          |
| full     | + per installed `files`, `recordedSource`, `coverage`                                                                          |

## Semantic Results

The status and exit mapping above are unchanged by detail or format. Root effects and recovery receipts retain their complete result facts at every detail level; command-owned data follows the catalogue's level rows.

### Counts and limitations

`installed`, `available`.

## Errors And Boundaries

The findings catalogue below is the command's finite error and warning vocabulary. Findings keep their code, severity, family, subject and cause; detail filtering affects display only. A blocked, failed or cancelled result prevents further effects according to the catalogue.

### Findings catalogue

| Code                                 | Severity | Family                | Message                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | Next                                                                                      |
| ------------------------------------ | -------- | --------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| extension-list.invalid-input         | error    | invalid-input         |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |                                                                                           |
| extension-list.workspace-unavailable | error    | workspace-unavailable |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |                                                                                           |
| extension-list.source-invalid        | error    | local                 | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/List/Shared/Wording/ExtensionListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-list.source-invalid`).                                                                                                                                                            | the matching manifest instruction above when manifest facts are available; otherwise none |
| extension-list.source-blocked        | error    | local                 | `<path> cannot be used as a source: <it is inside the workspace \| it resolves to an unsafe location>.`                                                                                                                                                                                                                                                                                                                                                                                                               | none                                                                                      |
| extension-list.source-unavailable    | warning  | local                 | [`extension.list.label.source-unavailable`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/List/ExtensionListText.cs); [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/List/Shared/Wording/ExtensionListWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-list.source-unavailable`). | the matching manifest instruction above when manifest facts are available; otherwise none |
| extension-list.ownership-observation | info     | ownership-observation |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |                                                                                           |
| extension-list.operation-failed      | error    | operation-failed      |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |                                                                                           |
| extension-list.interrupted           | error    | cancelled             |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |                                                                                           |

Row notes for installed packages come from the lock and target checks:
`source missing` (the recorded source cannot be read), `files changed`,
`files missing`. Each also produces a warning finding with the package as
subject: `The <id> Extension's source <path> cannot be read.`, `<N> files
installed by <id> have changed since installation.`, `<N> files installed by
<id> are missing.` with `Next: open-forge extension inspect <id>`.

## Scenarios

### Catalogue situations

`none-installed`, `one-installed`, `installed-only`, `available-only`,
`explicit-source`, `no-ownership-record` (info), `source-unreadable`
(incomplete), `installed-source-missing` (warnings), `source-invalid`,
`source-blocked`, `invalid-input`.

Each status has one representative native text transcript above. JSON uses the same status and command facts under the schema-3 envelope.

### Open maintainer questions

The native command emits `extension-list.installed-source-missing` for an installed package whose recorded source is missing, but the catalogue has no row for that code. The contract records the current warning and does not decide whether to add the row or change the code. **Maintainer decision remains open.**

## Non-Goals And Public Conformance

`list` does not select package IDs for a lifecycle operation, infer ownership
from path or fingerprint, resolve dependencies for installation, mutate a
source or workspace, repair lifecycle evidence, run `status` or `doctor`, or
execute an index operation.

Conformance must cover exact workspace and source selection, embedded versus
explicit source, package/catalogue distinction, source disjointness, both
section filters and their composition, trusted/incomplete/absent states, source-unavailable installed facts, deterministic ordering, all
seven statuses, human/JSON parity and streams, no prompts, and no persistent
effect. The [Shared Result
Coordinates](../../shared/result-coordinates/interface.md) define the exact JSON
result schema and exit mapping. Gate 5 must prove source-generated serialization, fixed Markdig where
used, real `System.IO`, Native AOT, isolated tests, and package journeys.

## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`extension.list.help.syntax`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/List/ExtensionListText.cs).

<!-- @OpenForgeTextRef extension.list.help.syntax -->
<!-- @OpenForgeTextRef extension.list.label.source-unavailable -->
