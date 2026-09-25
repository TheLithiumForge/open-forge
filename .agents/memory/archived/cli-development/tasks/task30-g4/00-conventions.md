---
open-forge:
  description: Shared rules, verification commands, message style, the shared message families, and the record-only instruction every G4 subtask follows
  tags: [Memory, CLI, Task, Plan, G4, Contextual, Archived, Historical]
---

# 00 — G4 conventions

Read once before any G4 subtask. Every subtask assumes these and does not
repeat them. Where this file and the older
[Task 30 slice conventions](../../../../working/cli-development/tasks/task30/00-conventions.md) differ, this file
wins for G4 subtasks. The verification commands, pass conditions, house rules
and the known flaky test in that older file still apply.

## What an implementer may and may not change

- You may change files under `src/cli/` and your own task file. Nothing else.
- You may not edit contracts under `.agents/memory/crystallized/`, public
  documentation under `docs/`, `README.md`, Directives, Patterns, other task
  records, or the shipped payload under `src/open-forge/`. The
  [documentation propagation task](41-documentation-propagation.md) does that
  from your ledger. This supersedes the older same-commit contract rule.
- The shipped payload is an exception only when a subtask says so explicitly
  (the index region fix may need a payload change; it says so).

## Two sections you must fill

Every subtask ends with these two sections. They are the hand-off to the
documentation pass and to review. An empty section at closeout is a defect.

**Changes ledger.** One bullet per observable change you made, in this shape:
`<surface>: <what it said or did before> -> <what it says or does now>`.
Surfaces are: a message (quote the old and new text), a JSON member (name,
type, level), a status or exit for a condition, a flag or help line, a stream,
a file layout or type name that a document names, a test that was deleted or
whose expectation changed. Contract or documentation sentences you noticed to
be wrong go here too, prefixed `doc:`, with the file and line.

**Divergences observed.** One bullet per place where the plan and reality
differed: what the plan said, what you found, what you did, and whether the
plan or a durable record needs a change. A named symbol that no longer exists
is a divergence. A line number that moved is not.

## Stopping

Stop and record under Divergences when a step needs a decision the plan does
not make: a message the catalogue does not cover, a status the contract does
not reach, a code with no severity, an ownership or naming choice, a safety
question. Do not infer it. Continue with the parts that do not depend on it.

## Verification

The commands and pass conditions in the
[older conventions](../../../../working/cli-development/tasks/task30/00-conventions.md#verification) apply. In
addition, for output-changing subtasks:

- Regenerate the snapshots for your command with `OPENFORGE_SNAPSHOT_UPDATE=1`
  (introduced by [01](01-before-snapshots.md)) and review the diff against the
  catalogue in your task file. Every difference must be one the catalogue
  asks for. Anything else is a divergence.
- Run the cross-command invariants from [40](40-verification.md) if they exist
  on your branch. They are added by 03 and must stay green.

## Message style

Every user-facing sentence follows the
[writing standard](../../../../crystallized/documents/maintenance/writing.md):

- Say what happened, name the affected item, give the next action when useful.
- Short sentences. Familiar words. No internal vocabulary: never `lifecycle`,
  `residual`, `preflight`, `projection`, `lease`, `occupant`, `provenance`,
  `topology`, `semantic`, `trusted`, `payload` in text. The
  [naming task](02-naming.md#vocabulary) lists the replacement words.
- Name paths workspace-relative, with `:line:column` when known.
- Never print an enum value as a word. `not-requested`, `not-applicable`,
  `residual: none`, `verified` as a bare word are all defects.
- Counts are words and numbers: `3 files`, `1 file`. Never `1 files`.
- Dry runs say `Would ...` and end with `No files were changed.`
- A no-op says `Nothing to do.`
- `Next:` is one line, normally last, and runs as typed. The merged Extension
  Install catalogue currently emits one required advisory after its `Next:`
  line; that placement is an open maintainer question recorded in [41](41-documentation-propagation.md)
  and is not a general rule. When no command applies it is a short sentence.
  Never `--help` for an identity error.

## Shared presentation rules

These are the rules the [rendering system](03-rendering-system.md) enforces
once. Command catalogues rely on them and do not restate them.

| Rule            | Content                                                                                                                                                                                                                                                                                                    |
| --------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Headline        | The first line is one complete sentence stating the outcome. There is no `Status:` line, ever.                                                                                                                                                                                                             |
| Levels          | `minimal` (default), `standard`, `full`, `debug`. Each level adds to the previous. `debug` adds only stderr diagnostics.                                                                                                                                                                                   |
| Listing ladder  | `minimal` lists errors and, except for `doctor`, warnings. `standard` lists warnings for `doctor` too. `full` and `debug` list info. Everything not listed is counted.                                                                                                                                     |
| Filter          | `--detail-filter <severity>` repeatable replaces the listing ladder with exactly the given severities at any level. `all` lists everything. The per-finding depth still follows the level. Counts are never affected.                                                                                      |
| Depth per level | `minimal`: subject, one-line cause, one action. `standard`: adds the reason for the action and per-finding actions. `full`: adds evidence, candidates with why they were included, provenance, hashes, codes. `debug`: adds run diagnostics on stderr.                                                     |
| Payload         | Requested rows, content and mutation receipts are never shortened by a level. Every replaced, restored, deleted, kept or rewritten path is listed at `minimal`. Created paths are listed at `minimal` except in Framework `install`, which summarizes them by count and directory and names the lock file. |
| Ordering        | Headline, workspace line (when shown), errors, warnings, what changed, what was kept, what could not be checked, counts, `Next`. Within one severity by path, then line, then column.                                                                                                                      |
| Subjects        | Every listed finding names a workspace-relative path with `:line:column` when known, or an identifier. Nothing without either is listed or blocks.                                                                                                                                                         |
| Empty           | Zero counts are not printed unless zero is the answer, said in words. `not-applicable`, `not-requested`, `none observed` and similar never appear. A fact that could not be obtained is one sentence naming the cause. An empty listing is one sentence repeating the query.                               |
| Workspace echo  | Assumption (C12): `Workspace: <path>` is printed at `minimal` when `--workspace` was given or the status is blocked, failed or cancelled; always at `standard` and above; never `Selected by:` in text. JSON always carries the workspace.                                                                 |
| Codes           | Finding codes appear in text only at `full` and `debug`, in brackets after the title. JSON carries them at every level.                                                                                                                                                                                    |
| Streams         | Text for completed, completed-with-warnings and incomplete results goes to stdout; text for invalid-input, blocked, failed and cancelled results goes to stderr. JSON is one schema-3 envelope on stdout for every report status. Diagnostics and prompts go to stderr. Parser failures before binding stay text on stderr with no envelope. |
| Exits           | Unchanged: 0, 2, 3, 4, 5, 1, 130 in the shared table. Two corrections: `route init` scaffold is complete (0). Read-only commands never print `No files changed.`                                                                                                                                           |
| Formatting      | Two-space indentation, aligned columns for rows, ASCII framing only, paths and identifiers never truncated, no JSON escapes in text, colour only on supported terminals, none in JSON or authored content.                                                                                                 |

## One code, one situation

**A finding code names one situation and renders one sentence.** When a code is
reached from several situations that need different sentences, no message can be
right for all of them, and the wording ends up hedging — a generic fallback, a
cause passed through raw, or an alternation standing in for facts the result does
not carry. That hedging is the symptom; the overloaded code is the defect.

Two different commands may share a sentence — that is what the families below are
for, and a held workspace lock reads identically in thirteen commands. **Two codes
of the same command sharing a sentence is the signature of an overload**, and
`CliReportInvariantsTests.NoTwoFindingCodesOfOneCommandRenderTheSameMessage`
fails on it across every captured situation.

Note what that test cannot see: a situation with no capture is not checked. When a
finding code has no fixture, the invariant is silent about it, so record the gap
rather than assuming it is covered.

Splitting an overloaded code is a behaviour change. It adds codes to JSON and can
move an exit code where the severity differs, so it is a maintainer decision, not
an implementer's.

## Shared message families

Most finding codes across the 28 commands belong to a family with one meaning.
The family's sentence is written once here, and command catalogues reference
it by family name with the command's subject filled in. `<command>` is the
command's verb phrase ("install", "update the Framework", "move the route").
`<path>` is the affected workspace-relative path.

| Family                       | Severity | Sentence                                                                                                                             | Next                                                                              |
| ---------------------------- | -------- | ------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------- |
| invalid-input                | error    | `Cannot <command>: <the exact input problem>.`                                                                                       | The corrected command when it can be formed, otherwise `open-forge <cmd> --help`. |
| confirmation-required        | error    | `<Command> needs confirmation, and this session cannot ask.`                                                                         | `open-forge <cmd> --automatic` (or `--dry-run` to preview)                        |
| workspace-unavailable        | error    | `Cannot use <path> as the workspace: it does not exist or cannot be read.`                                                           | none                                                                              |
| workspace-not-directory      | error    | `Cannot use <path> as the workspace: it is not a directory.`                                                                         | none                                                                              |
| workspace-unsafe             | error    | `Cannot use <path> as the workspace: its location could not be verified (<a link leaves it \| its identity is ambiguous>).`          | none                                                                              |
| workspace-lock-unavailable   | error    | `Another Open Forge command holds the workspace lock. Nothing was changed.`                                                          | `open-forge <cmd> ...` (retry when it finishes)                                   |
| target-changed               | error    | `<path> changed after the plan was made. Nothing was changed.`                                                                       | rerun the same command                                                            |
| target-changed-during-apply  | error    | `<path> changed while changes were being written. Stopped after <n> of <m> changes.`                                                 | `open-forge doctor`                                                               |
| target-unsafe                | error    | `<path> cannot be written safely: <it is a link \| its location could not be verified \| it is reserved>.`                           | none                                                                              |
| target-occupied              | error    | `<path> already exists and is not managed by Open Forge.`                                                                            | `--force --dry-run` where the command supports force                              |
| generated-region-unsafe      | error    | `The Entries section of <path> could not be identified: <it is missing \| there is more than one \| it is malformed>.`               | `open-forge doctor`                                                               |
| projection-unavailable       | warning  | `The Entries content for <path> could not be computed because <child metadata could not be read>.`                                   | `open-forge doctor`                                                               |
| metadata-incomplete          | warning  | `The frontmatter of <path> could not be read completely.`                                                                            | `open-forge doctor`                                                               |
| metadata-unsafe              | error    | `The frontmatter of <path> cannot be used: <reason>.`                                                                                | edit the file                                                                     |
| inspection-incomplete        | warning  | `<path> could not be read completely.`                                                                                               | `open-forge doctor`                                                               |
| recovery-unavailable         | warning  | `Recovery data could not be prepared at <store>. Nothing was changed.`                                                               | check the recovery store location                                                 |
| recovery-conflict            | error    | `Recovery data from an earlier run exists at <path> and blocks this change. Nothing was changed.`                                    | `open-forge cleanup --dry-run`                                                    |
| recovery-artifact-retained   | warning  | `The changes were applied, but the recovery bundle at <path> could not be removed.`                                                  | `open-forge cleanup`                                                              |
| recovery-failed              | error    | `The changes were applied, but the final state of the recovery bundle is unknown.`                                                   | `open-forge doctor`                                                               |
| write-failed                 | error    | `Writing <path> failed. Stopped after <n> of <m> changes. Recovery data: <path>.`                                                    | `open-forge doctor`                                                               |
| verification-failed          | error    | `<path> did not verify after it was written. Recovery data: <path>.`                                                                 | `open-forge doctor`                                                               |
| lifecycle-publication-failed | error    | `The changes were applied, but the ownership record .agents/open-forge.lock.json could not be written.`                              | `open-forge doctor`                                                               |
| lifecycle-unavailable        | warning  | `.agents/open-forge.lock.json could not be read completely.`                                                                         | `open-forge doctor`                                                               |
| lifecycle-blocked            | error    | `.agents/open-forge.lock.json is invalid: <reason>.`                                                                                 | fix or remove the file                                                            |
| ownership-observation        | info     | `No ownership record exists, so <what cannot be listed> cannot be read from it.`                                                     | none                                                                              |
| ownership-conflict           | error    | `<path> is owned by <owner>, so <command> cannot change it.`                                                                         | none                                                                              |
| ownership-claimed            | error    | `<path> is managed by <the Framework \| the <id> Extension \| the <id> Library>, so <command> cannot <verb> it.`                     | the owning command                                                                |
| managed-divergence           | error    | `<path> has changed since it was installed.`                                                                                         | `open-forge update` or `open-forge extension update <id>`                         |
| permission-required          | error    | `Cannot <command>: it writes outside .agents and no grant allows that.` then one line per path                                       | `open-forge <cmd> ... --allow-path <path>`; also the settings file                |
| permission-declined          | error    | `You declined the destinations, so nothing was changed.`                                                                             | none                                                                              |
| permissions-invalid          | error    | `.agents/open-forge.json cannot be used: <reason>.`                                                                                  | fix the file                                                                      |
| permissions-unavailable      | warning  | `.agents/open-forge.json could not be read.`                                                                                         | none                                                                              |
| permissions-changed          | error    | `.agents/open-forge.json changed after the plan was made. Nothing was changed.`                                                      | rerun                                                                             |
| permission-write-failed      | error    | `The grant could not be saved to .agents/open-forge.json.`                                                                           | `open-forge doctor`                                                               |
| identity-collision           | warning  | `The ID <id> matches more than one file. Use the exact path.` then one line per path                                                 | none                                                                              |
| route-ambiguous              | error    | `<id> could match more than one route.` then one line per path                                                                       | use the exact path                                                                |
| source-ambiguous             | error    | `<reference> matches more than one source. Use the exact path.` then one line per path                                               | use the exact path                                                                |
| source-unsafe                | error    | `<path> could not be verified to be inside the workspace.`                                                                           | none                                                                              |
| selector-ambiguous           | error    | `--include or --exclude <value> matches more than one source. Use the exact path.`                                                   | none                                                                              |
| selector-unsafe              | error    | `--include or --exclude <value> points outside the workspace.`                                                                       | none                                                                              |
| payload-unavailable          | warning  | `The Framework bundled in this CLI could not be read completely.`                                                                    | reinstall the CLI                                                                 |
| payload-invalid              | error    | `The Framework bundled in this CLI is invalid.`                                                                                      | reinstall the CLI                                                                 |
| framework-unavailable        | warning  | `The Framework files this command needs could not be read completely.`                                                               | `open-forge doctor`                                                               |
| framework-unsafe             | error    | `The Framework files this command needs could not be verified.`                                                                      | `open-forge doctor`                                                               |
| selection-required           | error    | `<Command> needs to know which packages. Pass their IDs or --all.` (add `This session cannot ask.` when a prompt would have applied) | `open-forge extension list`                                                       |
| interaction-ended            | error    | `Input ended before a choice was made. Nothing was changed.`                                                                         | none                                                                              |
| operation-failed             | error    | `<Command> stopped because of an unexpected error: <bounded reason>.`                                                                | `open-forge <cmd> ... --detail debug`                                             |
| interrupted                  | error    | `<Command> was cancelled. Nothing was changed.` or `... Stopped after <n> of <m> changes.`                                           | rerun                                                                             |
| unknown-id                   | error    | `No <Library \| Extension> has the ID <id>.`                                                                                         | the list command                                                                  |
| unknown-source               | error    | `No source has the ID <id>.`                                                                                                         | `open-forge route list --depth=all`                                               |

The family names above are the vocabulary used in the `Family` column of each
command catalogue. A code whose family is `local` has its complete sentence in
the command file.

## Snapshot naming

Snapshot files are named for situations, one per command, status and level:
`doctor/healthy.minimal.txt`, `doctor/warnings-only.standard.txt`,
`install/fresh-directory.minimal.txt`, `install/fresh-directory.json.minimal.txt`.
The catalogue's "Situations" list in each command file is the list of
snapshot names that must exist for that command.
