---
open-forge:
  description: Accepted Interface for creating a local Extension scaffold in a catalogue without installing it
  responsibility: Define create's exact syntax, catalogue destination, wizard and automatic input, workspace no-op, effects, results, and errors
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Create, Interface, Catalogue, Mutation, CurrentTruth]
---

# extension create Interface Contract

## Status And Authority

This is the accepted current Crystallized Interface Contract for
`open-forge extension create`. It owns the exact public syntax, stable-ID and
catalogue destination inputs, wizard/direct behavior, scaffold effects, global
flag applicability, statuses, output, errors, examples, non-goals, and public
conformance. The command does not ship yet.

The sibling [Behavior Contract](behavior.md) defines the technology-neutral
scaffold plan and safe application. The [Extension group entrypoint](../_extension.md)
defines group routing only. No Technical Design exists.

## Purpose And Boundary

`create` gives an author a stable local package boundary. It writes a scaffold
and deterministic manifest under a catalogue parent. It may record explicit
dependency IDs but does not resolve their availability or install them. It does
not install files into a workspace, update generated navigation, write the
ownership lock, or publish Framework or Extension lifecycle state.

`extension create` is the accepted no-workspace mutation exception. The catalogue
destination is the sole operation subject. Because `--workspace` is a no-op for
this operation, create does not acquire the external workspace mutation lock or
mutate workspace state.

The create destination is distinct from package source selection used by other
Extension operations. `--path` names the destination catalogue parent; it is not
an external package or catalogue source.

## Syntax

```text
open-forge extension create [<stable-id>]
  [--path <catalogue-path>]
  [--name <text>]
  [--description <text>]
  [--package-version <text>]
  [--dependency <stable-id>]...
  [--automatic]
  [--dry-run]
  [global flags]
```

The command-local human wizard can obtain whichever required facts are missing:
the stable ID and destination catalogue parent. A prompt-capable request may
supply neither, either, or both explicitly; the wizard asks only for missing
facts. JSON, `--automatic`, and non-prompt-capable use must provide both. `--path`
is a singleton value and repeated values are invalid. A stable ID is one exact
package identity and repeated positional IDs are invalid.

`--name`, `--description`, and `--package-version` are singleton nonblank
manifest overrides. `--package-version` remains distinct from the global
terminal `--version`. Accepted override text is preserved exactly after nonblank
validation; the descriptive package version does not gain a SemVer parser or
compatibility policy. `--dependency` is repeatable. Each value must be a valid
stable ID, must not equal the new package ID, and must not repeat. Accepted
dependencies serialize in ordinal stable-ID order, independent of option order.

The shared flags are:

```text
--workspace <path>
--format <text|json>
--detail <minimal|standard|full|debug>
--detail debug
--help
--version
```

`--workspace` is accepted as a global flag but is a no-op for create. The
catalogue destination, not the workspace, is the operation subject, and create
does not acquire the external workspace mutation lock. Other global grammar, repetition,
terminal, and presentation rules remain in [Global CLI
Flags](../../shared/global-flags/interface.md).

Create has no `--source`, `--all`, `--force`, `--prune`, `--yes`, package
selection, dependency installation, or workspace operand.

## Wizard, Direct, And Automatic Behavior

A prompt-capable human `create` asks for each missing required fact: stable ID
and destination catalogue parent. The argumentless form asks for both, a partial
explicit request asks only for the missing fact, and a complete explicit request
asks none. Each question gives clear local guidance. A blank or invalid answer
may be explained and asked again while input remains available; the command has
no arbitrary attempt limit or shared retry abstraction. End of input leaves a
required fact missing and returns `invalid-input` without writes. Caller cancellation
returns `cancelled` without writes. Explicit operands and `--path` answer the
same questions in one typed request. Conflicting or repeated explicit inputs are
invalid.

After those two required inputs resolve, omitted manifest fields use these exact
deterministic defaults:

| Field          | Default                                                                                                  |
| -------------- | -------------------------------------------------------------------------------------------------------- |
| `id`           | The exact resolved stable ID.                                                                            |
| `name`         | Split the ID on `-`, uppercase the first ASCII letter of each segment, and join segments with one space. |
| `description`  | `Open Forge Extension package <stable-id>.`                                                              |
| `version`      | `0.1.0`                                                                                                  |
| `dependencies` | An empty array.                                                                                          |

Explicit manifest options replace only their corresponding defaults. Optional
manifest metadata never adds a wizard question. Human planning shows the
resolved manifest before application.

JSON and other non-interactive modes never prompt. Missing ID or destination is
`invalid-input`. `--automatic` suppresses the wizard only after both semantic inputs
are explicit. It selects no package, source, workspace, dependency, or
authority by inference. Repeating it is idempotent.

The human flow validates the ID and destination, presents the scaffold plan, and
uses the explicit create invocation as the operation authority. `--dry-run`
previews the same plan and writes nothing. There is no saved plan or second
confirmation operation.

## Catalogue Destination And Scaffold

`--path` names one exact catalogue parent. Any existing safely resolved ordinary
directory is eligible, including an empty directory; it needs no catalogue
marker. Create never creates the parent. Unrelated sibling files or package
directories neither validate nor invalidate it and are not inspected. Create
inspects only the exact package destination `<catalogue>/<id>/`. It may be absent or contain the exact
intended scaffold below. An absent destination is eligible for creation, and an
exact matching scaffold is a verified no-op. Any divergent, partial, additional,
unknown, or colliding occupant blocks. The catalogue parent and destination must
retain their exact physical identities when present and satisfy safe lexical and
physical containment. A source or workspace selection is not used for this
operation.

The exact scaffold writes only:

```text
<catalogue>/<id>/extension.json
<catalogue>/<id>/content/.agents/
```

It does not install a README, payload content, Framework files, Extension files,
generated `Entries`, a lifecycle section, or a dependency closure.
The package remains a separate authored source location.

`extension.json` contains exactly `id`, `name`, `description`, `version`, and
`dependencies` in that order. All five are present. The command validates the
accepted manifest shape but performs no source lookup or dependency closure.

## Human Output

The command uses the shared native report. The default detail is `minimal`; `standard`, `full` and `debug` add the catalogue-defined facts. `--detail-filter <error|warning|info|all>` is repeatable and changes only the rendered detail. Use `--format text` for this text report. Primary result text for `completed`, `completed-with-warnings` and `incomplete` is on stdout; primary errors for `invalid-input`, `blocked`, `failed` and `cancelled` are on stderr. There is no `Status:` line.

### Statuses and headlines

| Status              | When                                                                           | Headline                                                                | Exit | Stream |
| ------------------- | ------------------------------------------------------------------------------ | ----------------------------------------------------------------------- | ---: | ------ |
| completed           | created                                                                        | `Created the <id> Extension scaffold at <folder>`                       |    0 | stdout |
| completed           | identical scaffold present                                                     | `The <id> scaffold at <folder> already matches. Nothing to do.`         |    0 | stdout |
| completed (dry run) | planned                                                                        | `Would create the <id> Extension scaffold at <folder>`                  |    0 | stdout |
| incomplete          | folder unreadable                                                              | `The scaffold could not be created: <limitation>. Nothing was changed.` |    3 | stdout |
| invalid-input       | missing ID or folder outside a terminal, invalid ID, bad version               | `Cannot create the Extension: <problem>.`                               |    4 | stderr |
| blocked             | folder inside the workspace's `.agents`, destination has other content, unsafe | `Cannot create <id> at <folder>: <reason>.`                             |    5 | stderr |
| failed              | after effects                                                                  | `Extension create stopped after <n> of <m> files.`                      |    1 | stderr |
| cancelled           | prompt cancelled, end of input                                                 | `Extension create was cancelled. Nothing was changed.`                  |  130 | stderr |

### Text by level

`minimal`:

```text
Created the my-tools Extension scaffold at packages/my-tools
  packages/my-tools/extension.json
  packages/my-tools/content/.agents/
  Edit extension.json, then add files under content/.agents/.
```

`minimal`, missing ID outside a terminal (stderr):

```text
Cannot create the Extension: no ID was given, and this session cannot ask.
Next: open-forge extension create <id> --path <folder>
```

`standard` adds the manifest values written (`name`, `description`,
`version`, `dependencies`).

`full` adds the manifest content verbatim.

### Prompts

Text input for the ID (`Extension ID (lowercase, digits and hyphens):`) and
the folder (`Package folder:`) when missing, then plan review and
`Create these files? [y/N]` unless `--automatic`.

### Representative transcripts by status

### Transcript — completed

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-create-completed). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Create/__snapshots__/ExtensionCreateBeforeOutputSnapshotTests/prompted-id-and-path/prompted-id-and-path.minimal.txt).

### Transcript — incomplete

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-create-incomplete). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Create/__snapshots__/ExtensionCreateBeforeOutputSnapshotTests/CatalogueUnreadable/catalogue-unreadable.minimal.txt).

### Transcript — invalid-input

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-create-invalid-input). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Create/__snapshots__/ExtensionCreateBeforeOutputSnapshotTests/invalid-id/invalid-id.minimal.txt).

### Transcript — blocked

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-create-blocked). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Create/__snapshots__/ExtensionCreateBeforeOutputSnapshotTests/destination-has-other-content/destination-has-other-content.minimal.txt).

### Transcript — failed

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-create-failed). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Create/__snapshots__/ExtensionCreateBeforeOutputSnapshotTests/PartialWriteFailure/write-failed-partial.minimal.txt).

### Transcript — cancelled

[Preserved interface example](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractTranscripts.md#extension-create-cancelled). [Matching reviewed capture](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Create/__snapshots__/ExtensionCreateBeforeOutputSnapshotTests/cancelled/cancelled.minimal.txt).

## Structured Output

`--format json` writes one schema-3 envelope to stdout for every report status. It contains the command, status, workspace when applicable, detail, filter, command data, findings, effects, counts, limitations, recovery facts and next action as applicable. It is the same typed result as the text report; no ordinary text is mixed into the JSON document. If parsing fails before binding, the raw parser diagnostic remains text on stderr and no report envelope exists.

### JSON data by level

| Level    | `data`                                                         |
| -------- | -------------------------------------------------------------- |
| minimal  | `{ mode, id, folder, packagePath, manifestPath, contentPath }` |
| standard | + `manifest { name, description, version, dependencies }`      |
| full     | + `manifestContent`                                            |

## Semantic Results

The status and exit mapping above are unchanged by detail or format. Root effects and recovery receipts retain their complete result facts at every detail level; command-owned data follows the catalogue's level rows.

### Effects wording

`<path>  created` / `Would create <path>`; partial: `created`, `not started`.

### Counts and limitations

`filesCreated`, `directoriesCreated`.

## Errors And Boundaries

The findings catalogue below is the command's finite error and warning vocabulary. Findings keep their code, severity, family, subject and cause; detail filtering affects display only. A blocked, failed or cancelled result prevents further effects according to the catalogue.

### Findings catalogue

| Code                                   | Severity | Family              | Message                                                                                                                                                                                                                                                                                     | Next                        |
| -------------------------------------- | -------- | ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------- |
| extension-create.invalid-input         | error    | local               | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Create/Shared/Wording/ExtensionCreateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-create.invalid-input`). | corrected command           |
| extension-create.catalogue-unavailable | warning  | local               | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Create/Shared/Wording/ExtensionCreateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-create.catalogue-unavailable`).                                                                                                                                                                                                                                                               | none                        |
| extension-create.catalogue-unsafe      | error    | local               | `<folder> cannot be used: <it is inside the workspace's .agents \| it resolves to an unsafe location>.`                                                                                                                                                                                     | choose another folder       |
| extension-create.destination-collision | error    | local               | [selection](../../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Extension/Create/Shared/Wording/ExtensionCreateWording.cs); [independent forms](../../../../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Presentation/Invariants/Fixtures/ContractMessageTemplates.json) (`extension-create.destination-collision`).                                                                                                                                                                                                                                      | choose another ID or folder |
| extension-create.destination-changed   | error    | target-changed      |                                                                                                                                                                                                                                                                                             |                             |
| extension-create.application-failed    | error    | write-failed        | (no recovery bundle: `Extension create stopped after <n> of <m> files. Created files were left in place.`)                                                                                                                                                                                  | none                        |
| extension-create.verification-failed   | error    | verification-failed | (no recovery bundle)                                                                                                                                                                                                                                                                        |                             |
| extension-create.interrupted           | error    | cancelled |                                                                                                                                                                                                                                                                                             |                             |

## Scenarios

### Catalogue situations

`created`, `created-with-metadata`, `dry-run`, `already-present` (no-op),
`destination-has-other-content` (blocked), `missing-id-non-interactive`,
`prompted-id-and-path`, `invalid-id`, `catalogue-unreadable` (incomplete),
`write-failed-partial`, `cancelled`.

Each status has one representative native text transcript above. JSON uses the same status and command facts under the schema-3 envelope.

### Open maintainer questions

The catalogue does not specify how malformed versions or unresolvable dependencies are classified, whether creating inside the workspace's `.agents` is allowed, the wording for a completed effect, or the split between the published `folder` and `packagePath` members. The current behavior is recorded without filling those gaps. **Maintainer decisions remain open.**

## Non-Goals And Public Conformance

Create does not use `--workspace` to choose a destination, read a package source,
install or update an Extension, resolve dependencies, mutate a target workspace,
write lifecycle state, project generated navigation, adopt an existing package,
run a formatter, or remove the package source.

Conformance must cover zero, one, and all currently missing required human facts;
command-local correction of blank/invalid input without an attempt limit;
no-write end of input and caller cancellation; direct, JSON,
automatic, and redirected omission states; exact ID
and catalogue-parent validation, empty and populated marker-free parents,
unrelated sibling preservation, exact-destination-only inspection, refusal to
create a missing parent, catalogue destination distinction, workspace
no-op and the absence of workspace-lock acquisition, scaffold-only
effects, an absent destination, an exact-scaffold no-op, and divergent, partial,
additional, unknown, or colliding occupants blocking. It must also cover exact
catalogue and destination physical identity, expected-state revalidation
immediately before effects, the separate create-only path with no Replace/Delete,
no recovery bundle and no workspace lease, dry-run parity, all five deterministic
manifest defaults, every singleton override and native option-value form,
repeatable dependency ordering, duplicate/self/invalid dependency rejection,
exact manifest property order, no dependency availability resolution, no
optional-metadata wizard questions, exact command-local JSON result/property order without
shared-envelope duplication, all seven statuses
and streams, JSON parity, and no workspace lifecycle effect. The [Shared Result
Coordinates](../../shared/result-coordinates/interface.md) define the exact JSON
result schema and exit mapping. Gate 5 must prove source-generated serialization,
fixed Markdig where used, real `System.IO`, Native AOT, isolated tests, and
package journeys.




## Executable Wording References

Exact wording is owned by the linked typed factories. Selection, output coordinates and behavioral requirements remain in this contract and its existing semantic owners. The independent fixture preserves the original reviewed message forms.

CLI help syntax: [`extension.create.help.syntax`](../../../../../../../../src/cli/output-text/OpenForge.Cli.OutputText/Extension/Create/ExtensionCreateText.cs).

<!-- @OpenForgeTextRef extension.create.help.syntax -->
