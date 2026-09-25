---
open-forge:
  description: Decide the names of statuses, severities, resolution lanes, states, presentation classes and flags before the rendering system uses them
  tags: [Memory, CLI, Task, Plan, G4, Naming, Contextual, Archived, Historical]
---

# 02 — Naming

> Read [00 — G4 conventions](00-conventions.md) first.

## Goal

Every name a user can see in text, JSON, help or a flag, and every
presentation type name an implementer will create in [03](03-rendering-system.md),
is decided before code uses it. This is a decision task: the maintainer
approves the tables below and the accepted column becomes the vocabulary for
every other subtask.

> The `Today` columns preserve the vocabulary observed when this decision was
> recorded. The `Recommended` and `Accepted` columns are the current wire and
> presentation vocabulary; retired names below are not current syntax.

## Depends on / Blocks

- Depends on: nothing.
- Blocks: 03 and every command subtask.

## Statuses (C9)

The seven statuses keep their exits and streams. Their names change so that
each says what happened without a second reading. JSON carries the name; text
never prints it.

| Today         |  Exit | Recommended               | Alternative | Why                                                                |
| ------------- | ----: | ------------------------- | ----------- | ------------------------------------------------------------------ |
| `complete`    |   `0` | `completed`               | `ok`        | Past tense matches the sibling names.                              |
| `attention`   |   `2` | `completed-with-warnings` | `warnings`  | Says the command finished and that warnings exist. Nothing failed. |
| `incomplete`  |   `3` | `incomplete`              | `partial`   | Some facts could not be established. Keep; it is understood.       |
| `invalid`     |   `4` | `invalid-input`           | `invalid`   | Names the cause: the input, not the workspace.                     |
| `blocked`     |   `5` | `blocked`                 |             | Keep.                                                              |
| `failed`      |   `1` | `failed`                  |             | Keep.                                                              |
| `interrupted` | `130` | `cancelled`               |             | What a person says when they press Ctrl+C or answer no.            |

Recommendation: the `Recommended` column. `completed-with-warnings` is long
for a JSON value, and that is acceptable: it is read far more often than typed.

Text never prints these names. It prints the sentence patterns in the
[shared rules](00-conventions.md#shared-presentation-rules). Where a finding
label carried the status today (`REQUIRES ATTENTION:`, `INCOMPLETE:`,
`COMPLETE:` before an observation), it carries a severity word instead.

## Severities (C4)

| Value in JSON | Word in text | Meaning                                                               |
| ------------- | ------------ | --------------------------------------------------------------------- |
| `error`       | `Error`      | The command could not do part of its job, or the workspace is broken. |
| `warning`     | `Warning`    | The job was done, but something needs a look.                         |
| `info`        | `Info`       | Useful context. Nothing to do.                                        |

Mapping from today's per-finding status on non-doctor commands: `invalid`,
`blocked`, `failed`, `interrupted` become `error`; `attention` and
`incomplete` become `warning`; `complete` observations become `info`. Each
command subtask's catalogue records the triage per code, which may depart
from this default when the meaning demands it.

## Resolution lanes (doctor and repair)

Lane names stay in JSON. Text uses the phrase.

| Lane                 | Phrase in text                         |
| -------------------- | -------------------------------------- |
| `safe-exact`         | `can be fixed automatically`           |
| `guided-choice`      | `needs a choice`                       |
| `targeted-operation` | `use <command>` (the command is named) |
| `manual-decision`    | `fix by hand`                          |
| `blocked-repair`     | `cannot be fixed automatically`        |
| `informational`      | (nothing; it is an Info finding)       |

## Vocabulary

Words that must not appear in text, with their replacements. Command
catalogues use the right column.

| Do not print                                    | Print instead                                                           |
| ----------------------------------------------- | ----------------------------------------------------------------------- |
| lifecycle, lifecycle record, lifecycle document | `ownership record` or the file name `.agents/open-forge.lock.json`      |
| installation record: trusted                    | (nothing; it is the normal case)                                        |
| installation record: absent                     | `no ownership record`                                                   |
| managed file, managed target                    | `Framework file` / `file installed by <package>`                        |
| generated navigation, generated region          | `Entries section`                                                       |
| generated entry                                 | `entry`                                                                 |
| residual, residual path                         | `recovery bundle at <path>` / `recovery data`                           |
| recovery bundle (as jargon)                     | `recovery bundle` is kept; `recovery data` when speaking generally      |
| preflight, revalidation, post-condition         | (nothing)                                                               |
| lease, workspace lock lease                     | `workspace lock`                                                        |
| occupant, initial occupant                      | `existing file`                                                         |
| projection (Library)                            | `link`                                                                  |
| projection (context, find)                      | `part` or the part name (`body`, `headings`)                            |
| source root                                     | `source folder`                                                         |
| topology                                        | `route structure`                                                       |
| provenance, read from                           | `read from <source>` only at `full`                                     |
| semantic, semantically                          | (nothing)                                                               |
| payload                                         | `the Framework bundled in this CLI` / `package content`                 |
| intended                                        | `the version this CLI ships` / `the package's version`                  |
| current (for a file state)                      | `unchanged`                                                             |
| changed (Framework target)                      | `changed since it was installed`                                        |
| retired                                         | `no longer part of this release` / `no longer part of the package`      |
| candidate, candidate set                        | `possible target`                                                       |
| evidence                                        | `why this was suggested` (at `full`)                                    |
| coverage complete / incomplete                  | (nothing) / `<N> checks could not finish`                               |
| not-requested, not-applicable, not-established  | (nothing)                                                               |
| verified (bare)                                 | (nothing; success implies it)                                           |
| wizard                                          | `prompt`                                                                |
| selected by: current directory                  | (nothing)                                                               |
| selected by: --workspace                        | `Workspace: <path>`                                                     |
| source occurrence                               | `link` / `<path>:line:column`                                           |
| logical source, physical layer                  | `source` / `file`                                                       |
| overwrite companion                             | `overwrite file` (`<name>.overwrite.md`)                                |
| bridge, bridge registration                     | `the Extension's entry in <parent Entries section>`                     |
| stable ID                                       | `ID`                                                                    |
| catalogue                                       | `the bundled Extensions` / `the package folder at <path>`               |
| embedded catalogue / embedded Framework         | `bundled with this CLI`                                                 |
| fingerprint                                     | `SHA-256` (at `full` only)                                              |
| automatic ID                                    | `ID`                                                                    |
| Loader                                          | `.agents/loader.md` or `the Loader` (a defined Framework term; allowed) |
| entrypoint                                      | allowed; a defined Framework term                                       |

## Flags

| Today       | Accepted                                                 |
| ----------- | -------------------------------------------------------- |
| `--view`    | removed                                                  |
| `--json`    | `--format text\|json`                                    |
| `--verbose` | removed; `--detail debug`                                |
| (new)       | `--detail minimal\|standard\|full\|debug`                |
| (new)       | `--detail-filter error\|warning\|info\|all` (repeatable) |

Open naming question for the maintainer: `--detail-filter` versus a shorter
`--filter`. Recommendation: keep `--detail-filter`, because `--filter` will be
wanted later by `find` and `route list` for data filters.

## Presentation type names (for 03)

| Today                                                                | Proposed                                                                      | Note                                                     |
| -------------------------------------------------------------------- | ----------------------------------------------------------------------------- | -------------------------------------------------------- |
| `CliView`, `CliVerbosity`                                            | `CliDetail` (`Minimal`, `Standard`, `Full`, `Debug`)                          | one enum replaces two                                    |
| `CliOutputFormat` (`Human`, `Json`)                                  | `CliFormat` (`Text`, `Json`)                                                  | "human" is not a format                                  |
| `CliPresentation`                                                    | `CliPresentation` (`Format`, `Detail`, `Filter`, `Colors`)                    | keep the name, change members                            |
| `CliViewRenderers`, `CliRendererSet`                                 | removed                                                                       | one text renderer and one JSON renderer for all commands |
| `*HumanRenderer`, `*JsonRenderer`, `*DiagnosticRenderer` per command | `<Command>ReportSelector`, `<Command>PayloadTextRenderer`                     | see 03                                                   |
| `CliCompactJsonDocument`, `CliCompactJsonProjection`                 | removed                                                                       | one envelope                                             |
| `CliHumanText`, `CliHumanStyle`                                      | `CliText`, `CliTextStyle`                                                     | "human" again                                            |
| `*TextEscaping` (seven)                                              | removed; `CliText.Escape`                                                     | M3                                                       |
| `*WireVocabulary`                                                    | kept for JSON finite values; one per family, not per command, where identical | dedupe                                                   |
| `*HumanVocabulary`                                                   | `*Wording`                                                                    | it is wording, not a vocabulary                          |
| `Commands/<Cmd>/Shared/Rendering/`                                   | `Commands/<Cmd>/Presentation/`                                                | the layer name                                           |
| `Shell/Presentation/`                                                | `Presentation/` (top-level in Core)                                           | the layer's home; see 03                                 |

New types introduced by 03: `CliReport<TPayload>`, `CliFinding`, `CliEffect`,
`CliCount`, `CliNextAction` (exists), `CliSelection`, `CliTextRenderer`,
`CliJsonRenderer`, `CliDiagnosticRenderer`. Names are proposals until 03 is
approved.

## Category and section labels

| Doctor category (today) | Label (keep unless noted) |
| ----------------------- | ------------------------- |
| Workspace               | `Workspace`               |
| Recovery                | `Recovery data`           |
| Routes and navigation   | `Routes and Entries`      |
| Links                   | `Links`                   |
| Framework               | `Framework files`         |
| Extensions              | `Extensions`              |

Route inspect keeps `Where this source belongs`, `When it is read`,
`Context size`.

## Steps

1. [x] Maintainer reviews every table and marks the accepted column.
2. [x] Record the accepted names in this file under "Accepted" and update the
       conventions file's vocabulary pointer if a family name changed.

## Acceptance

- [x] Every table has an accepted choice.
- [ ] 03 and every command subtask reference only accepted names.

## Changes ledger

### Accepted

The recommended statuses, severity table, resolution lanes, replacement
vocabulary, Accepted flags, proposed presentation names and category labels
are accepted for this execution. The concrete layout in 03 resolves the
placement discrepancy recorded below. Shared message-family names do not
change, so the conventions vocabulary pointer needs no edit.

- Execution direction: the maintainer requested implementation of the complete
  G4 packet on 2026-09-14. Use this task's recommended names and the detailed
  layout in 03. The authored tables and CLI output catalogues remain intact.
- Status wire names: `complete` -> `completed`; `attention` ->
  `completed-with-warnings`; `invalid` -> `invalid-input`; `interrupted` ->
  `cancelled`. `incomplete`, `blocked`, `failed`, numeric exits and streams
  retain their specified meanings. This records the names for implementation;
  it does not claim that the runtime has changed yet.
- Flags and presentation vocabulary: use the Accepted flags table and the
  proposed presentation type names, subject to 03's concrete layer layout.
  Keep `--detail-filter`, the three severities, the resolution-lane names and
  the category labels specified above.
- Wording source: the maintainer additionally requested that the specified CLI
  strings remain unchanged and be easy to find and edit through template string
  factories. Use one `<Command>Wording.cs` under
  `Presentation/<command path>/Shared/Wording/` for each command's sentences,
  titles and command-specific questions. Use named static methods with typed
  arguments and interpolated strings; fixed sentences use parameterless
  factories. Shared message families stay in
  `Presentation/Shared/Wording/CliFindingWording.cs`, and shared prompt wording
  stays with the prompt primitives. No localization framework is introduced.
- Wording boundaries: factories return raw semantic strings. Selectors choose
  the sentence; text rendering escapes it once and handles layout; JSON uses
  serializer escaping. Authored content and diff spans bypass these factories.
  Snapshot expectations remain independent of production factories.

## Divergences observed

- The presentation-name table forecasts `Commands/<Cmd>/Presentation/`, while
  03 specifies top-level `Presentation/<Cmd>/` and its dependency rules. Use
  03's concrete layout. Documentation propagation must describe that layout;
  the original table remains preserved as requested.
- The task still describes a separate table-approval step. The subsequent
  maintainer instruction to implement the whole packet supplies execution
  direction, and the recommended choices were stated before dependent work.
  No catalogue string or accepted behavior is being replaced by a new proposal.
