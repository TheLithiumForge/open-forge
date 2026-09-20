---
open-forge:
  description: Unaccepted G4 output proposal covering shared presentation rules, per-command transcripts for all 28 commands, the Task 31 M3 escaping decision, JSON, streams, exits, and an implementation handoff
  tags: [Memory, Working, CLI, Task, Proposal, Presentation, Contextual, Candidate]
---

# Task 30 G4 — Output Proposal (Fable)

## Status

This is an unaccepted design proposal for the [G4 view layer](phase-4b-g4.md).
Nothing in it is approved. It changes no renderer, escaper, contract, test or
document. "Approved" below means the maintainer has said so explicitly.

Written 2026-09-13 against `d5824851` on `feature/development-2`.

Evidence boundary:

- Captured transcripts come from a managed build of the current tree
  (`dotnet build` of `src/cli/root/OpenForge.Cli`, 2026-09-13 23:50, version
  `0.0.0`), run inside a scratch workspace outside the repository. Every block
  labelled **Captured** is real output from that build with the absolute
  workspace path shortened to `<ws>`. Every block labelled **Proposed** is
  authored here and has never been produced by the CLI.
- `doctor` was not run anywhere, per maintainer instruction. Doctor transcripts
  use the unit snapshot fixtures under `src/cli/tests/unit/.../__snapshots__/`
  (labelled **Fixture**) and constructed examples.
- The `open-forge` executable on `PATH` and the native artifact under
  `artifacts/publish/win-x64/` are the audit-era build `0.0.0-dev.sha-62b0e23e`
  (before G1 and B1). They were not used. The managed dev artifact under
  `artifacts/publish/open-forge-dev/` was also stale (it still wrote the retired
  lifecycle file) and was rebuilt before capture.
- Provenance note: a text search for `--view` across the task folder, made to
  find the analysis that retires that flag, surfaced two lines of the sibling
  proposals: the `--detail brief/normal/full` spelling and a schema-version-3
  mention. Nothing else from those files was read before this proposal was
  written. The same spelling is already proposed in the Emerging
  [naming analysis](../../../../emerging/analysis/cli-experience-audit/lifecycle-baselines-and-architecture.md#--view-is-granularity),
  so convergence on it is expected rather than borrowed.

## Sources used

- Task records: [G4](phase-4b-g4.md), [second-gate review](review-second-gate-pre-g4.md),
  [G1](phase-4a-g1.md), [structural follow-up](phase-4a-structural.md),
  [Task 31 M3](../task31/phase-escaper.md), [phase 7 scenarios](phase-7-scenarios.md).
- Contracts: [shared operation contract](../../../../crystallized/documents/cli/shared-operation-contract.md),
  [result coordinates](../../../../crystallized/documents/cli/contracts/shared/result-coordinates/interface.md),
  [global flags](../../../../crystallized/documents/cli/contracts/shared/global-flags/interface.md),
  [workspace permissions](../../../../crystallized/documents/cli/contracts/shared/workspace-permissions/interface.md),
  the output sections of the Status, Doctor, Index and Install interfaces, and
  [docs/cli.md](../../../../../../docs/cli.md).
- Directives and guidance: [CLI implementation](../../../../../directives/open-forge/cli/implementation.md),
  [CLI design](../../../../../guidance/cli-design.md), the
  [writing standard](../../../../crystallized/documents/maintenance/writing.md)
  and [dictionary](../../../../crystallized/documents/maintenance/helpers/dictionary.md).
- Emerging evidence: the five analyses G4 lists, plus the naming, interaction,
  severity and dogfood analyses. Their transcripts predate G1 and were treated
  as ideas, not decisions, as G4 requires.
- Code: the shell presentation pipeline, all 28 human renderers, the eight
  escapers, `CliHumanText`, the doctor and status models and vocabularies.

## Part 1 — Shared rules

Every rule below is proposed. Each names its recommendation and, where a
different choice would change what the user sees, the alternative.

### R1. One detail axis: `--detail brief|normal|full`

`--view compact|expanded` is retired. It is replaced by one global option:

```text
--detail <level>    How much of the result to show: brief (default), normal, or full.
```

| Level    | Who it serves                          | What it contains                                                                                                                                                                          |
| -------- | -------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `brief`  | Everyone, by default. Agents, scripts. | The outcome in one sentence, every item that needs an action or that changed existing content, counts for everything healthy, and at most one next action.                                |
| `normal` | A person reviewing the result.         | Everything in brief, plus the complete list of what was checked or changed, warnings that do not block, kept or unchanged items as counts, the reason behind the next action.             |
| `full`   | Investigation and bug reports.         | Everything the result model contains that a reader could act on: informational findings, evidence, candidates and why they were included, provenance, hashes, rosters of unchanged files. |

Rules:

- The default is `brief` for text and JSON alike.
- `--detail` changes presentation only. It never changes what the command
  inspects, plans, applies, finds, its status or its exit code.
- Lower levels omit whole facts, never truncate lists. A list is shown complete
  or replaced by its count. Paths, identifiers, commands and authored content
  are never shortened.
- `lines(brief) <= lines(normal) <= lines(full)` for the same result.
- `--verbose` keeps its current meaning: bounded implementation diagnostics on
  stderr, independent of `--detail`. The CLI implementation Directive requires
  that separation. A command with no diagnostic renderer accepts the flag and
  writes nothing, which the global-flag contract already allows.
- There is no compatibility alias for `--view`, `compact` or `expanded`. The
  CLI is unreleased.

Recommendation: three levels named `brief`, `normal`, `full`.

Alternative that changes the experience: two levels (`brief` and `full`).
Simpler to implement and document, but a person reviewing an `update` then
gets either three lines or the fingerprint roster. The middle level is where
review happens.

### R2. The four output shapes

Every command result is one of four shapes. The shape decides what brief must
keep.

| Shape                | Commands                                                                                                                                                          | Brief must keep                                                                                                                             |
| -------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| Report               | `status`, `route inspect`, `extension inspect`, `library inspect`, `library list`, `extension list`                                                               | The answer to the command's question and anything that needs attention.                                                                     |
| Listing              | `find`, `route list`, `context`                                                                                                                                   | One row or block per hit. Nothing else between the rows.                                                                                    |
| Diagnosis            | `doctor`                                                                                                                                                          | Counts by severity, every error with its subject, checks that could not finish, one next action.                                            |
| Mutation and preview | `install`, `update`, `index`, `repair`, `cleanup`, `route init/create/update/move/remove`, `extension create/install/update/remove`, `library attach/sync/detach` | What changed or would change. Every replaced, deleted or kept-divergent path is listed. New files may be summarized by count and directory. |

### R3. Status presentation

The first line is always a complete sentence that states the outcome. There is
no `Status:` line. The exit code and the sentence carry the status.

| Status        | Sentence shape                                                                   | Example                                                           |
| ------------- | -------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| `complete`    | What was done or found.                                                          | `Updated Entries in 2 of 21 files.`                               |
| no change     | What is already true, then `Nothing to do.`                                      | `The Framework is up to date. Nothing to do.`                     |
| dry run       | `Would ...`, then the plan, then `No files were changed.`                        | `Would install the Open Forge Framework into <ws>.`               |
| `attention`   | What was done, then how many things need a look.                                 | `Installed the development Extension. 1 file needs attention.`    |
| `incomplete`  | What was established, then what could not be checked and why.                    | `Some workspace checks could not finish.`                         |
| `invalid`     | `Cannot <verb> ...` naming the input, then the correction.                       | `Cannot create the routed file: --description is missing.`        |
| `blocked`     | `Cannot <verb> ...` naming the boundary, then what to resolve.                   | `Cannot install: 2 existing files are in the way.`                |
| `failed`      | What was attempted, how far it got, what is left in place.                       | `Update stopped after 2 of 5 files were written.`                 |
| `interrupted` | Same as failed, with the interruption named.                                     | `Library attach was interrupted after 1 of 2 links were created.` |
| partial       | Applies to failed and interrupted. Lists applied, not started and unknown paths. | see `library attach` below                                        |

Rules:

- `attention` is never rendered as the word `attention` or `requires attention`
  in text. The sentence says what needs a look.
- A no-op never claims verification of nothing. `repair` with zero selected
  effects says `Nothing to repair.` and never `verified`.
- A partial result names each path whose final state is unknown. The headline
  never claims that no files changed when a file may have changed.

### R4. Ordering

1. Headline sentence.
2. Workspace line, only when [R9](#r9-context-echo) requires it.
3. Findings that block or need action, errors before warnings, then by path,
   then by line and column.
4. What changed, then what was kept, then what could not be checked.
5. Counts for healthy or unchanged items.
6. `Next:` as the last line.

Within one severity, order is by path (ordinal), then location. This is the
model's deterministic order for every command already.

### R5. Subjects, locations and next actions

- Every listed finding names a workspace-relative path. When a line and column
  exist, they follow the path as `path:line:column`. Byte offsets stay in JSON.
- A finding that cannot name a path names its identifier (a route ID, a package
  ID, a Library ID). A finding with neither cannot be listed and cannot block.
- `Next:` is one line, last, holding one command that runs as typed. It is
  omitted when nothing useful follows. It never points at `--help` for an
  identity error, and never at a command that is missing a required operand.
  When the right action is manual, the line is a short sentence instead of a
  command, for example `Next: edit the description and tags in <path>.`
- At `normal` the reason follows the command in parentheses.
- Per-finding actions appear beside their finding at `normal`. `brief` keeps
  the one overall action.

### R6. Empty, zero, not applicable, unavailable

- Zero counts are not printed unless zero is the answer, in which case the
  headline states it in words (`No problems found.`, `No recovery data to
remove.`).
- `not-applicable`, `not-requested`, `residual: none`, `none observed`,
  `unavailable` and similar enum echoes never appear in text. A fact that does
  not apply is omitted. A fact that could not be obtained is stated once, as a
  sentence naming the cause, at the point where the reader would expect it.
- An empty listing is one sentence that repeats the query: `No sources match
--tag Zzz.`
- A section with nothing to report is omitted rather than shown with `none`.

### R7. Wording

Text uses short sentences and familiar words, following the writing standard.
The current vocabulary maps as follows. The left column is what renderers say
today; the right column is what they say after G4.

| Today                                                                                                                             | Proposed                                                                             |
| --------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| `Status: complete` / `requires attention` / `blocked`                                                                             | dropped, carried by the sentence                                                     |
| `Installation record: trusted` / `absent` / `incomplete`                                                                          | dropped when trusted; `The lock file is missing.` when absent, only when it matters  |
| `Source: available` / `not-applicable`                                                                                            | dropped                                                                              |
| `Lifecycle: publish / verified`, `Recovery: not-required`, `Verification: verified`, `Preflight: complete`, `Post-diagnosis: ...` | dropped from brief and normal; `full` states verification and recovery in words      |
| `residual: none` / `remaining state none`                                                                                         | dropped; a retained residual is a listed path                                        |
| `Mode: apply; force: true; automatic: true`                                                                                       | dropped; dry run is stated in the headline, `--force` in the effect lines it enabled |
| `Effects: 44`                                                                                                                     | `Created 21 files and 20 directories under .agents`                                  |
| `replace generated-region`                                                                                                        | `Updated the Entries section of <path>`                                              |
| `need updating` / `changed` (generated navigation)                                                                                | `Entries section is stale` / `changed after install`                                 |
| `Managed file: <path>` + `WARNING  Managed Extension file changed [extension.managed-changed]`                                    | `<path>    changed since it was installed by <package>`                              |
| `Resolution: use the indicated command`                                                                                           | dropped; the command is the Next line                                                |
| `Findings: 0 errors, 1 warning, 0 informational findings`                                                                         | `1 warning.` inside the headline                                                     |
| `Possible targets: 1; none selected`                                                                                              | `Possible target (not chosen): <path>`                                               |
| `interactive-wizard`                                                                                                              | `prompt`                                                                             |
| `Package selection: explicit-ids`                                                                                                 | dropped                                                                              |
| `1 files`, `1 sources`                                                                                                            | `1 file`, `1 source`                                                                 |
| `~7993 tokens`                                                                                                                    | `about 8.0k tokens` in prose, `~7993` in tables                                      |

Stable finding codes such as `[reference.target-missing]` stay in text at
`normal` and `full` (they are searchable and appear in JSON), and are omitted
at `brief` where the title and path already identify the problem. This is a
decision point, see [D5](#d5-finding-codes-in-brief-text).

### R8. Findings model in presentation

Doctor's severity ladder is presented as follows. This is presentation only.
No finding kind, severity or occurrence leaves the model or JSON at `full`.

| Severity      | `brief`                       | `normal` | `full`            |
| ------------- | ----------------------------- | -------- | ----------------- |
| error         | listed with subject and cause | listed   | listed + evidence |
| warning       | counted                       | listed   | listed + evidence |
| informational | counted                       | counted  | listed            |

Informational kinds that describe coverage rather than a workspace fact
(`reference.target-valid`, `reference.cycle`, `reference.repeat`,
`reference.external-unchecked`, `*.ownership-observation`) are presented as
coverage counts at `brief` and `normal` (`21 links checked, 3 external links
not checked`) and appear as findings only at `full`. Their count is not mixed
into "informational findings" so that number stays meaningful. This is a
grouping rule and grants no permission to delete a category; see [D4](#d4-coverage-kind-findings-presented-as-counts).

Candidates and evidence are properties of the finding they belong to. They are
never printed as separate findings.

### R9. Context echo

`Workspace: <path>` and the selection method are printed only when:

- `--workspace` was given, at every level; or
- the level is `normal` or `full`.

Otherwise the headline names the directory only when the reader's next question
is "which directory?", for example `Open Forge is not installed in <ws>.` JSON
always carries `workspace`.

### R10. JSON

One schema replaces the two current ones. Every command emits:

```text
CliJsonEnvelopeV3 {
  schemaVersion: integer(3),
  command: string,
  status: "complete" | "attention" | "incomplete" | "invalid" | "blocked" | "failed" | "interrupted",
  detail: "brief" | "normal" | "full",
  workspace: { path, selectedBy } | null,
  result: command-owned object,
  next: { command, reason } | null
}
```

Rules:

- Always minified. Readers pretty-print with their own tools.
- `--detail` selects field membership in `result`. Each command contract lists
  which members appear at each level. A member omitted at a level is absent,
  never `null` and never an empty array. `detail` in the envelope tells a
  consumer what to expect.
- Every level keeps: identities, statuses, coverage and limitations, all counts,
  listed findings with subject, code, cause, location and resolution, `next`,
  and for mutations every effect with path, action, outcome and residual. A
  mutation receipt is complete at every level because it cannot be recovered by
  rerunning the operation.
- `brief` omits: evidence, provenance, candidates (their count stays on the
  finding), rosters of unchanged or inspected paths, per-path hashes and
  fingerprints, source-asset paths, generated-navigation observations for
  unchanged files, continuity sources, healthy target rosters.
- `normal` adds candidates, rosters of unchanged paths and per-path comparison
  summaries.
- `full` is the complete current model.
- Doctor JSON follows [R8](#r8-findings-model-in-presentation): `brief` carries
  errors, `normal` adds warnings, `full` adds informational findings. Counts are
  complete at every level.
- Scalars are plain: a measurement that is available is a number. One that is
  not is `null`, and the reason is a listed finding or limitation. The
  `{ "state": "available", "value": 19 }` wrapper is removed. See [D6](#d6-plain-json-scalars).
- The serializer owns escaping (see [R13](#r13-escaping-task-31-m3)). JSON never
  contains colour or human prose.

### R11. Streams and exit codes

Preserved exactly:

| Status        |  Exit | Text stream |
| ------------- | ----: | ----------- |
| `complete`    |   `0` | stdout      |
| `attention`   |   `2` | stdout      |
| `incomplete`  |   `3` | stdout      |
| `invalid`     |   `4` | stderr      |
| `blocked`     |   `5` | stderr      |
| `failed`      |   `1` | stderr      |
| `interrupted` | `130` | stderr      |

JSON is one document on stdout for every status. Diagnostics from `--verbose`
are on stderr. Help and version are text on stdout with exit `0`.

Status corrections that this proposal asks for, because the current mapping
makes ordinary success non-zero:

- `route init` returns `complete` when it creates a scaffold that needs
  authoring. That is the command's purpose. The advisory stays in the text and
  in JSON (`needsAuthoring: true` on the entrypoint).
- `status` in a directory without Open Forge stays `complete` with exit `0`.
  Scripts read `installation.state` from JSON. An alternative exit for "not
  installed" would make `status` in a fresh checkout fail under `set -e`.
- `find` with zero matches stays `complete`. The sentence carries the answer.

### R12. Formatting

- Two-space indentation for every nested line. No deeper than three levels.
- Row output (`route list`, `find` at `normal`, doctor problem lists) aligns
  columns with spaces so identifiers and paths line up. Alignment is computed
  per result; nothing is truncated to fit.
- Generated framing uses ASCII only: `->` between old and new values, `,` and
  parentheses for grouping, `:` after labels. No middle dots or other non-ASCII
  separators, so Windows consoles with legacy code pages print the framing
  correctly. Authored content and paths keep their Unicode.
- Text never contains JSON escapes. `D:\work\repo` prints as typed.
- Numbers use invariant digits, a thin separator only for token counts in
  prose (`8.0k`), and correct singular and plural forms.
- Colour is unchanged in scope: green, yellow, red, cyan on supported terminals,
  applied to the headline sentence and to severity words. Bold is added to
  subject paths and dim to `Next` reasons and coverage counts. `NO_COLOR`,
  redirection, Windows and `dumb` terminals stay plain. JSON, TSV rows, authored
  content and diffs never receive styling.

### R13. Escaping (Task 31 M3)

Today eight types answer "how is text escaped" with four different answers, and
two of them (`RouteTextEscaping`, `ExtensionTextEscaping`) leak JSON escaping
into human text. Proposed:

- One human text function, `CliHumanText.Escape(string)` in the Shell
  presentation scope, is the only owner. It keeps every printable character as
  is, including `\`, `"`, `<`, `>` and non-ASCII. It replaces `\n`, `\r` and
  `\t` with the visible two-character sequences `\n`, `\r`, `\t`, so one value
  always stays on one line and a tab cannot break a TSV row. It replaces every
  other control character and lone surrogate with `\uXXXX`.
- Human primary output never truncates. `DiagnosticValueLimit = 240` moves into
  the same owner as the single clamp for `--verbose` stderr diagnostics, which
  are the only bounded output.
- JSON is escaped by `System.Text.Json` with `JavaScriptEncoder.UnsafeRelaxedJsonEscaping`
  on every context, so `<`, `>`, `&`, `+` and non-ASCII characters stay literal
  and newlines are `\n`. No hand-written JSON escaper remains. The name of the
  encoder refers to HTML embedding, which does not apply to a file or pipe.
- The seven command-local escapers and their `Clamp` variants are deleted after
  the before-snapshots are committed. Their tests move to the single owner.
- Authored content selected by a projection (`context` bodies and sections,
  `index` diff lines, template bodies) bypasses the escaper entirely and is
  written byte-exact.

This is the one escaping decision M3 requires. It is a caller-visible JSON
change (`route list --json` stops emitting `\u003c`, `extension list --json`
stops emitting `\\n`) and is reviewed through the same output diffs as the
renderer changes, as the plan requires.

### R14. Line endings

- Renderers build framing with `\n`. The shared human framing writer converts
  framing to the platform newline once, at the output stage, using the existing
  `PlatformLineEndings` helper. It is tested for LF, CRLF, CR and mixed input.
- Authored content embedded in human output keeps its own bytes and line
  endings. The writer never touches it.
- JSON values are never line-ending normalized. Persisted files are never
  touched by presentation.

### R15. Size budgets

Proposed acceptance budgets for text at `brief`, per command per status, on a
healthy or lightly changed workspace:

| Situation               | Lines                                                    |
| ----------------------- | -------------------------------------------------------- |
| Healthy report or no-op | 1 to 3                                                   |
| Successful mutation     | 1 headline + 1 line per changed path, + `Next`           |
| Problems                | 1 headline + 2 lines per listed problem + `Next`         |
| Listing                 | 1 line per hit, + 1 trailer line when truncated by depth |

These are tested as invariants in the phase 7 snapshot layer, not as hard
caps in renderers. `normal` and `full` have no budget beyond the ordering
invariant.

### R16. Help changes that follow from G4

Only what the flag change forces:

- The global option lines become `--detail <level>  How much of the result to
show: brief (default), normal, or full.` and `--json  Write one JSON document
to stdout instead of text.` The `[default: Expanded]` defect disappears with
  the flag.
- Every command's `Global options` paragraph names `--detail` instead of
  `--view`.
- The 20-line `Results and streams` block repeated in every command help is
  replaced by one line, `Exit codes and streams: open-forge --help`, and the
  table moves to root help. This is optional in G4; it is listed because the
  block mentions `Expanded JSON` and must change anyway.

Other help improvements (suggestions for typos, option grouping) belong to
phase 5.

## Part 2 — Per-command specification

Each command records: the question it answers, brief, what normal and full add,
the empty and problem cases, `Next`, JSON membership by level, and transcripts.
Exit and stream follow [R11](#r11-streams-and-exit-codes) unless stated.

### `status`

Question: Is Open Forge installed here, is it current, and how much does
startup load?

Captured, healthy, current default (104 lines):

```text
Open Forge is installed.
Status: complete
Workspace: <ws>
Selected by: current directory

Context
  Shipped startup: 19 files, 31972 characters, 31972 bytes, ~7993 tokens
  Startup: 19 files, 31972 characters, 31972 bytes, ~7993 tokens
  Difference: 0 files, 0 characters, 0 bytes, ~0 tokens
  Continuity (may load again): 4 files, 4110 characters, 4110 bytes, ~1028 tokens
Startup share: 80.94%
  Total available context: 22 files, 39503 characters, 39503 bytes, ~9876 tokens
Largest continuity sources
  memory/emerging: 1669 bytes
  ...
Framework
  Installation record: trusted
  Source: available
  .agents/directives/_directives.md: current
  .agents/directives/_directives.md: current
  ... (every managed file twice, 43 lines)
Extensions: 0
  Installation record: trusted
  Source: not-applicable
Managed files: none recorded

Libraries
  State: trusted
  Record: complete (.agents/open-forge.lock.json)
  Links: 0 registered, 0 current, 0 missing, 0 changed, 0 blocked, 0 unavailable
  none

Recovery
Verified recovery records: 0
Incomplete drafts: 0
```

Proposed, healthy, brief:

```text
Open Forge is installed and current.
  Startup reads 19 of 22 routed files, about 8.0k tokens.
```

Proposed, healthy, brief, with one Extension installed:

```text
Open Forge is installed and current.
  Startup reads 19 of 22 routed files, about 8.0k tokens.
  Extensions: development 0.1.0
```

Proposed, healthy, normal (adds the measurements a person compares over time):

```text
Open Forge is installed and current.
Workspace: <ws>

Startup context
  Shipped:     19 files, ~7993 tokens
  Current:     19 files, ~7993 tokens (81% of the 22 routed files)
  May load again later: 4 files, ~1028 tokens
Routes: 8 root categories, 20 Entries sections current
Framework: 21 managed files current
Extensions: development 0.1.0
```

`full` adds every managed file with its state, every Entries section with its
state, the three largest continuity sources, and the recovery counts even when
zero.

Captured, one authored line appended after `## Entries` in `_maps.md`, current
compact (exit 3, 47 lines):

```text
Open Forge is installed.
Status: incomplete
Workspace: <ws>
Selected by: current directory
INCOMPLETE: The context inventory observation is incomplete. [context-inventory-incomplete]
INCOMPLETE: The continuity context measurement is unavailable. [continuity-context-unavailable]
REQUIRES ATTENTION: The managed lifecycle target is not current. [framework-target-changed]
  .agents/maps/_maps.md
REQUIRES ATTENTION: The generated navigation target is not current. [generated-navigation-changed]
  Affected navigation paths are listed under Routes.
INCOMPLETE: The current root-category observation is unavailable. [root-categories-unavailable]
INCOMPLETE: The current startup context measurement is unavailable. [startup-context-unavailable]
...
Next: open-forge doctor
```

Proposed, same state, brief (exit 3):

```text
Open Forge is installed, but 1 file needs attention and startup context could not be measured.
  .agents/maps/_maps.md    changed after install; its Entries section is stale
  Startup context, continuity and root categories were not measured because that file could not be read completely.
Next: open-forge doctor
```

Captured, directory without Open Forge (59 lines, 41 of them `not-applicable`):

```text
Open Forge is not installed.
Status: complete
Workspace: <empty>
Selected by: current directory
COMPLETE: The ownership lock is absent; no Extension ownership claims are recorded. [extension-ownership-observation]
COMPLETE: The ownership lock is absent; no Framework ownership claims are available. [framework-ownership-observation]
COMPLETE: The ownership lock is absent; no Library registrations are recorded. [library-ownership-observation]
  .agents/open-forge.lock.json
...
Generated navigation:
  20 not-applicable
  .agents/directives/_directives.md: not-applicable
  ...
```

Proposed, brief (exit 0):

```text
Open Forge is not installed in <empty>.
Next: open-forge install --dry-run
```

JSON: `brief` keeps `installation`, the four startup measurements as plain
numbers, `startupPercentage`, counts of Entries sections and managed files by
state, installed Extension ids and versions, Library and recovery counts, and
findings. `normal` adds the non-current target lists. `full` adds every target
roster and `continuitySources`.

### `doctor`

Question: Is anything wrong in this workspace, and what do I do about it?

Fixture, one changed Extension file, current expanded (21 lines):

```text
The workspace needs attention. No files changed.
Status: requires attention
Workspace: unavailable
Selected by: unavailable
Checks: complete
Findings: 0 errors, 1 warning, 0 informational findings
Resolution: 0 exact repairs, 0 choices, 1 targeted command
  0 manual decisions, 0 blocked repairs, 0 informational findings

Extensions: checks complete
  Installation record: trusted
  Source: available
  Findings: 0 errors, 1 warning, 0 informational findings
  Resolution: 0 exact repairs, 0 choices, 1 targeted command
    0 manual decisions, 0 blocked repairs, 0 informational findings

  Managed file: .agents/toolkit/example.md
  WARNING  Managed Extension file changed [extension.managed-changed]
    An Extension-managed target differs from its current intended payload.
    Resolution: use the indicated command
    Expected: current-intended-fingerprint; observed: current-fingerprint
    Read from: Extension installation record
    Next: use the indicated command: open-forge extension update
      Extension update owns managed-target reconciliation.
```

Proposed, healthy, brief (exit 0):

```text
No problems found.
  6 checks complete. 21 links checked.
```

Proposed, warnings only, brief (exit 2):

```text
No errors. 2 warnings and 3 informational findings were recorded.
  To list them: open-forge doctor --detail normal
```

Proposed, one error and two warnings, brief (exit 2):

```text
1 error, 2 warnings and 3 informational findings.
  .agents/skills/pdf/SKILL.md:1:1    Frontmatter is invalid
                                     The frontmatter block is not closed.
Next: edit .agents/skills/pdf/SKILL.md, then rerun open-forge doctor
```

Proposed, same, normal (adds the warnings, categories only where something is
listed, codes, and per-finding actions):

```text
1 error, 2 warnings and 3 informational findings.
Workspace: <ws>

Routes and navigation
  .agents/skills/pdf/SKILL.md:1:1    Frontmatter is invalid  [workspace.frontmatter-malformed]
                                     The frontmatter block is not closed.
                                     Edit the file by hand.

Links
  .agents/loader.md:105:3            Broken link  [reference.target-missing]
                                     The linked file was not found: patterns/_patterns.md
                                     Possible target (not chosen): .agents/patterns/_patterns.md
                                     Choose it: open-forge repair --relink .agents/loader.md:105:3 patterns/_patterns.md .agents/patterns/_patterns.md
  .agents/maps/_maps.md:32:3         Broken link  [reference.target-missing]
                                     The linked file was not found: nowhere/_nope.md
                                     No possible target was found. Fix the link by hand.

  21 links checked, 3 external links not checked.
Next: open-forge repair --dry-run  (preview the 1 repair that can be applied automatically)
```

`full` adds informational findings as rows, evidence lines (`Observed:`,
`Expected:`, `Written value:`), why each candidate was included (`filename
match`), `Read from:` provenance, proposal verification and recovery details,
and the resolution counts.

Checks that could not finish appear at every level, because they change the
status:

```text
2 warnings. 1 check could not finish.
  Extensions were not checked: the package source ./packages/toolkit cannot be read.
  To list the warnings: open-forge doctor --detail normal
```

The `Next` line at brief is chosen from the resolution lanes in this order:
safe-exact repairs exist (`open-forge repair --dry-run`), a targeted command
exists (that command), guided choices exist (`open-forge repair`), otherwise a
manual sentence. Never `--help`.

JSON: as [R8](#r8-findings-model-in-presentation) and [R10](#r10-json). `brief`
keeps `coverage`, `counts`, `limitations`, listed errors with subject, message,
kind, location, resolution and actions, plus overall `actions`. `normal` adds
warnings and candidates. `full` is the current v1 graph with plain scalars.

### `install`

Question: What did install create, and did it touch anything that was already
there?

Captured, fresh directory, current (57 lines): headline `Open Forge install`,
`Mode: apply; force: false; automatic: true`, the inventory fingerprint, then
44 effect lines each ending `verified; residual: none`, then `Findings: 0`,
`Lifecycle: publish / verified`, `Recovery: not-required`, `Verification: verified`.

Proposed, brief:

```text
Installed the Open Forge Framework into <ws>.
  Created 21 files and 20 directories under .agents.
  Created AGENTS.md and CLAUDE.md with an Open Forge section.
```

Proposed, existing `AGENTS.md`, brief (the managed region is appended, the
file is a replacement, so it is listed):

```text
Installed the Open Forge Framework into <ws>.
  Created 21 files and 20 directories under .agents.
  Added an Open Forge section to AGENTS.md. Your existing content was kept.
  Created CLAUDE.md.
```

Captured, second run (14 lines), proposed:

```text
Open Forge is already installed and current. Nothing to do.
```

Proposed, dry run, brief:

```text
Would install the Open Forge Framework into <ws>.
  Would create 21 files and 20 directories under .agents, plus AGENTS.md and CLAUDE.md.
  Nothing that already exists would be changed.
No files were changed.
```

Proposed, existing occupants without `--force`, brief (exit 5, stderr):

```text
Cannot install: 2 files already exist where the Framework would write.
  .agents/loader.md
  .agents/maps/_maps.md
Next: open-forge install --force --dry-run  (preview replacing them)
```

Proposed, terminal cannot prompt and `--automatic` absent (exit 4, stderr):

```text
Install needs confirmation, and this session cannot ask.
Next: open-forge install --automatic  (or --dry-run to see the plan first)
```

`normal` lists every file effect with its action (`created`, `replaced`,
`section added`), summarizes directories by count, and states where the lock
was written. `full` adds directories, source asset paths, the inventory
fingerprint, and recovery and verification facts in words.

JSON: `brief` keeps `mode`, `force`, `automatic`, `classification`,
`footprint`, all `effects` (path, kind, action, outcome, residual),
`findings`, `recovery.state` and `residualPath`. `normal` adds `sourceAssetPath`
per effect. `full` adds `source.inventoryFingerprint` and `lifecycle`.

### `update`

Question: What changed, what did you keep because I changed it, and where is
the previous content?

Captured, clean workspace, dry run, current (16,371 bytes): `Effects: 0`,
then `Inspected paths without effects:` listing all 21 files with two
`Comparison:` lines and two SHA-256 pairs each, then the generated navigation
roster, then `Lifecycle: trust=trusted / coverage=complete / action=preserve /
outcome=already-current`.

Proposed, no change, brief:

```text
The Framework is up to date. Nothing to do.
```

Proposed, changes applied, brief:

```text
Updated 3 managed files to the bundled Framework version.
  .agents/guidance/_guidance.md
  .agents/patterns/_patterns.md
  .agents/workflows/_workflows.md
  Kept 2 files that you changed:
  .agents/loader.md
  .agents/maps/_maps.md
  Previous content: git diff
Next: open-forge update --force  (replace the kept files with the bundled version)
```

The `Previous content` line names `git diff` only when a `.git` directory is
present. When it is absent and a recovery bundle was retained, it names the
bundle path. When neither applies the line is omitted. See [D7](#d7-previous-content-pointer-after-update).

Proposed, missing managed file restored, brief:

```text
Restored 1 missing managed file.
  .agents/patterns/_patterns.md
```

Proposed, dry run with prune, brief:

```text
Would update 3 managed files and delete 1 retired file.
  .agents/guidance/_guidance.md            update
  .agents/patterns/_patterns.md            update
  .agents/workflows/_workflows.md          update
  .agents/guidance/old-advice.md           delete (retired, --prune)
  Would keep 2 files that you changed. Add --force to replace them.
No files were changed.
```

`normal` lists every managed file with `current`, `updated`, `kept (changed)`,
`restored`, `deleted`, and the Entries sections that were rewritten. `full`
adds the current and intended SHA-256 per path and the source asset path.

JSON: `brief` keeps mode flags, `effects`, `findings`, `generatedNavigation`
entries that changed, `recovery`, `verification`. `normal` adds the unchanged
comparisons with their `current` and `intended` relation. `full` adds
fingerprints and `source`.

### `index`

Question: Which Entries sections were rewritten?

Captured, no change (9 lines):

```text
Generated Entries are up to date.
Status: complete
Workspace: <ws>
Selected by: current directory
Mode: apply
Regions: 20; updates: 0; already current: 20; verified: 20
Selection: automatic-loader / rooted / 1 sources

No files changed.
```

Proposed, brief:

```text
Entries sections are current in all 20 files. Nothing to do.
```

Captured, one stale section rewritten (10 lines), proposed brief:

```text
Updated the Entries section in 1 of 21 files.
  .agents/maps/_maps.md    0 entries
```

Captured, dry run with one stale section (25 lines, `@@ {"id":"loader",...} @@`
header and two blank `- ` lines), proposed brief:

```text
Would update the Entries section in 1 of 20 files.
  .agents/loader.md    8 -> 7 entries
No files were changed.
```

`normal` shows the diff for each changed section, with a plain header:

```text
--- .agents/loader.md  (Entries section)
- - [Reusable default shapes for code, files, APIs, documents, and other work](patterns/_patterns.md) - #LoadNow #Core #Pattern
```

Diff lines are authored content and are written byte-exact. `full` adds the
selection facts (origin, scope, selected sources).

Proposed, blocked by a malformed leaf (exit 5, stderr):

```text
Cannot rebuild the Entries section of .agents/skills/_skills.md.
  .agents/skills/pdf/SKILL.md:1:1    frontmatter is invalid: the block is not closed
  Nothing was written. The other 19 sections are current.
Next: open-forge doctor
```

Captured, folder path given as operand (exit 4):

```text
Generated navigation update could not start because the input is invalid.
...
INVALID: The exact path is not an admitted logical source. [index.invalid-source]
Next: open-forge index --help
```

Proposed:

```text
Cannot index .agents/memory/emerging/ideas/pricing: it is a folder, not a source.
Next: open-forge index memory/emerging/ideas/pricing
```

JSON: `brief` keeps `mode`, `counts`, regions with `action` other than
`unchanged` (path, before and expected counts, outcome), `findings`,
`recovery`. `normal` adds the `change` diff bodies. `full` adds unchanged
regions and `selection`.

### `repair`

Question: What did you fix, and what still needs me?

Captured, healthy workspace, dry run (20 lines of phase names and zero counts):

```text
Preview of selected repairs
Status: complete
...
Selected: 0 findings / 0 effects
Unselected: 0 findings
Remaining: 0; manual 0; guided 0; blocked 0
Repaired: 0; new findings: 0; verified effects: 0
Affected paths: none
Plan: 0 steps / 0 effects / 0 no-ops
Selection details: automatic
Preflight: ready / recovery not-required
Application: not-requested (0 effects)
Verification: targets not-requested; bytes not-requested; post-conditions not-requested
Recovery: not-required; residual none (none)
Post-diagnosis: not-requested / workspace/path=not-requested, ...
No files changed (--dry-run).
```

Proposed, nothing to repair, brief:

```text
Nothing to repair.
```

Proposed, nothing automatic but problems remain, brief (exit 0; the status is
`complete` because the requested selection was empty, and the text says why):

```text
Nothing could be repaired automatically. 2 problems need a choice.
Next: open-forge repair  (choose a target for each, or see them with open-forge doctor --detail normal)
```

Proposed, repaired, brief:

```text
Repaired 2 links.
  .agents/loader.md:105:3    ../patterns/_patterns.md -> patterns/_patterns.md
  .agents/maps/_maps.md:32:3   nowhere/_Nope.md -> nowhere/_nope.md
```

Proposed, dry run, brief:

```text
Would repair 2 links.
  .agents/loader.md:105:3    ../patterns/_patterns.md -> patterns/_patterns.md
  .agents/maps/_maps.md:32:3   nowhere/_Nope.md -> nowhere/_nope.md
No files were changed.
```

Proposed, partial (exit 1, stderr):

```text
Repair stopped after 1 of 2 links were rewritten.
  Rewritten:    .agents/loader.md:105:3
  Not started:  .agents/maps/_maps.md:32:3
  A recovery bundle is kept at <recovery-path>.
Next: open-forge doctor
```

Rule preserved from the audit: `repair` never reports `verified` for zero
effects, and a workspace that doctor still reports as blocked is never
reported as repaired.

`normal` adds each finding that was not selected with the reason (needs a
choice, manual, blocked), and the Library recovery selection when present.
`full` adds the plan steps, expected and intended states, preflight, recovery
and post-diagnosis facts in words.

JSON: `brief` keeps `mode`, `selectionMode`, `counts`, `affectedPaths`,
`effects`, `findings`, `recovery`. `normal` adds the unselected findings and
the plan. `full` adds preflight, verification and post-diagnosis.

### `cleanup`

Question: What recovery data did you remove?

Captured, nothing to remove, apply (11 lines): `Recovery-data cleanup`,
`Mode: apply`, `Candidate check: complete; plan: safe`, `Preflight: complete`,
`Workspace lock: not-requested`, `Final workspace check: not-requested`,
`Verification: verified`.

Proposed, brief:

```text
No recovery data to remove.
```

Proposed, removed, brief:

```text
Removed 2 recovery bundles and 1 incomplete draft.
  <store>/ws1-2026-09-13T21-04-11.zip
  <store>/ws1-2026-09-13T21-09-52.zip
  <store>/ws1-2026-09-13T21-11-30.draft
```

Proposed, dry run, brief:

```text
Would remove 2 recovery bundles and 1 incomplete draft.
  <paths>
No files were changed.
```

Proposed, lock held (exit 5, stderr):

```text
Cannot clean up: another Open Forge command holds the workspace lock. Nothing was removed.
Next: open-forge cleanup  (retry when the other command finishes)
```

Deletion candidates are always listed at brief because they are deletions.
`normal` adds each candidate's kind and integrity, and candidates that were
seen but not eligible. `full` adds lease and revalidation facts in words.

### `context`

Question: Give me the documents, in order.

Captured, default (36,573 bytes, 845 lines): a nine-line header, then per
source `Path:`, `ID:`, `Route:`, `Scope:`, `Order:`, `Layer:`, `Included
because:`, `Frontmatter: missing`, `Body: available`, then the body.

Proposed, brief:

```text
=== AGENTS.md ===
<body, byte-exact>

=== .agents/loader.md (loader) ===
<body, byte-exact>

=== .agents/directives/_directives.md (directives) ===
<body, byte-exact>
```

Rules for this command, because its output is a payload read by a model:

- One delimiter line per source layer: `=== <path> (<id>) ===`, or
  `=== <path> (<id>, overwrite) ===`. No other generated line between
  delimiters. A missing frontmatter or an unavailable projection is not
  announced inside the stream; an unavailable body becomes a finding above
  the stream.
- The body is written byte-exact, including managed-region comment markers
  that belong to the file.
- `--content=paths` prints one path per line and nothing else.
- `--content=headings` prints the delimiter and one line per heading in
  Markdown form (`## Axioms`).
- Findings (an unresolved requested source, a link that could not be
  followed) print before the first delimiter, or on stderr when the status is
  `invalid` or `blocked`.
- The startup summary is not printed at brief. Agents calling `context` at
  task start pay for nothing but the documents.

`normal` prints one summary line first (`19 sources, about 8.0k tokens`) and
adds `included because` under each delimiter. `full` adds route, scope, order,
layer, headings with line numbers and the followed-link table.

JSON: unchanged structure with plain scalars. `brief` omits per-heading
locations and link evidence.

### `find`

Question: Which sources match?

Captured, `--tag Memory`, default (77 lines): four to five lines per hit and
an eleven-line `Search details:` block echoing the query.

Captured, `--tag Memory --view compact` (TSV with a header row):

```text
result=complete	coverage=complete	universe=default	matches=12
memory	.agents/memory/_memory.md
memory/archived	.agents/memory/archived/_archived.md
...
```

Proposed, brief (tab-separated, no header, nothing after the rows):

```text
memory	.agents/memory/_memory.md
memory/archived	.agents/memory/archived/_archived.md
memory/crystallized	.agents/memory/crystallized/_crystallized.md
```

Proposed, no matches, brief (exit 0):

```text
No sources match --tag Zzz.
```

Proposed, normal (aligned, with descriptions, and the count):

```text
12 sources match --tag Memory.
memory                          .agents/memory/_memory.md                        Self-growing Markdown memory for active work, coordination, accepted knowledge, candidates, and history
memory/archived                 .agents/memory/archived/_archived.md             Useful history that no longer controls current work
```

`full` adds the match evidence per source (`Memory: frontmatter tag`) and the
search details (filters, regions, universe, inspected count). `--content`
projections print their blocks after the rows at every level, as today.

A finding that limits coverage (an unreadable source, an ambiguous include)
prints before the rows at brief as one sentence; the rows follow.

JSON: `brief` keeps `matches` with `id`, `path`, `description`, `coverage`
and `findings`. `normal` adds `evidence`. `full` adds `query`, `universe` and
`presentation`.

### `references`

Question: What links into this source, and what does it link to?

Captured, `references memory`, default (34 lines): 21 `Inspected:` lines,
then `Incoming links: 0; coverage complete; status complete` and `Outgoing
links: 0; coverage complete; status complete` for a file that carries four
generated Entries links.

Proposed, brief, authored links present:

```text
.agents/memory/_memory.md
  in   1    .agents/loader.md:104:3
  out  4    archived/_archived.md, crystallized/_crystallized.md, emerging/_emerging.md, working/_working.md
```

Proposed, brief, no authored links:

```text
.agents/memory/_memory.md has no authored links in or out.
  Generated Entries links are not counted.
```

The second line is required until the operation counts generated links. The
current contract scope is authored links, and the wording must say so instead
of `No direct links found.` Whether generated links should be counted is a
phase 5 correctness question, recorded in [D8](#d8-references-and-generated-links).

`normal` lists each occurrence on its own row with `path:line:column` and the
written destination. `full` adds the inspected sources and the filter
selectors.

JSON: `brief` keeps `source`, `direction`, both sections with count, coverage
and occurrences (path, location, destination). `full` adds `incomingSelection`.

### `route list`

Question: What routes exist?

Captured, default (99 lines for 13 routes, six metadata lines each, tags as
JSON arrays, backslashes doubled in the workspace path). Captured compact (31
lines):

```text
Routes
Status: complete
Workspace: C:\\Users\\...\\ws1
Selected by: current directory
Coverage: complete; roots: 8; depth: 1; routes: 13

directives  .agents/directives/_directives.md
  Required instructions loaded through selected routes; tags: ["LoadNow", "Core", "Directive"]
```

Proposed, brief:

```text
directives                      Required instructions loaded through selected routes
guidance                        Advice for recurring choices, tradeoffs, and work situations
  guidance/adaptive-collaboration   Explore ideas, match the depth to the decision, integrate accepted outcomes, and offer useful independent review
maps                            Concise maps to important local and external sources and when to use them
memory                          Self-growing Markdown memory for active work, coordination, accepted knowledge, candidates, and history
  memory/archived               Useful history that no longer controls current work
  memory/crystallized           Accepted knowledge that should remain current
  memory/emerging               Useful material that is not accepted yet
  memory/working                Temporary memory that helps agents continue or resume active work
patterns                        Reusable default shapes for code, files, APIs, documents, and other work
skills                          Specialized capabilities provided through native SKILL.md packages
templates                       Copy-ready files for starting independently maintained workspace content
workflows                       Repeatable Markdown recipes for reaching a defined goal
13 routes to depth 1. Deeper routes: open-forge route list --depth=all
```

Full IDs are used on child rows so a row can be pasted into `route inspect`.
Indentation shows the hierarchy. The trailer line appears only when the
requested depth stopped the listing. Descriptions are authored content and are
not shortened.

Captured, unknown ID (exit 4, 12 lines including `Selection: source ID
memries -> none at none`). Proposed:

```text
No source has the ID 'memries'.
Next: open-forge route list --depth=all  (list every ID)
```

`normal` adds the path and the tags (`#LoadNow #Core #Directive`) under each
row and the workspace line. `full` adds parent, absolute and relative depth,
kind, direct child count, how the row was selected, and overwrite presence.

JSON: `brief` keeps `coverage`, `effectiveDepth`, rows with `id`, `path`,
`description`, `tags`, `relativeDepth`. `full` adds `parentId`, `kind`,
`directChildCount` and `provenance`.

### `route inspect`

Question: Where does this source sit, when is it read, and what does reading
it cost?

Captured, default (35 lines). The three question blocks are already right and
are kept. The header block and two trailing blocks change.

Proposed, brief:

```text
memory  .agents/memory/_memory.md

Where this source belongs
  Route chain: memory
  Parent: none (Loader root)
  Direct children: 4 entrypoints
  Descendants: 11 entrypoints

When it is read
  At task start or resume: yes
  Read automatically when the Loader is read
  May be read again later: no

Context size
  This file: 3.36 KiB, about 861 tokens
  Selecting this route adds: nothing (already in startup context)
  Read automatically below it through #LoadNow: 6 files, 7.03 KiB, about 1801 tokens
```

Rules: `1 files` becomes `1 file` or is dropped when the count is one. The
ASCII comma replaces the middle dot. Selection method, source state, route
state, entrypoint form and physical layers are shown only when unusual: a
compatibility entrypoint name, an overwrite companion, an ambiguous ID.

`normal` adds the Axioms block (`Inherited from: loader`, `Local: substantive
local Axioms`) and the workspace line. `full` adds the explanation of the
status, the selected-closure measurements and the selection facts.

Ambiguous ID (`attention`, exit 2) at brief:

```text
memory/notes  matches 2 sources. Use the exact path.
  .agents/memory/notes.md
  .agents/memory/notes/_notes.md
```

JSON: unchanged fields with plain scalars. `brief` omits `explanations` and
the selected-closure measurements.

### `route init`

Question: Which entrypoints were created?

Captured, apply (exit 2, 33 lines) including `Before: \n\n- none - No entries

- #Empty\n`and`Expected: ---\nopen-forge:\n description: ...`as one
escaped line each, an`Unchanged:`roster,`Lifecycle: none / not-requested`and`Next: open-forge route update` without its required operand.

Proposed, brief (exit 0):

```text
Created .agents/memory/emerging/ideas/pricing/_pricing.md
  Listed in .agents/memory/emerging/ideas/_ideas.md
  Its description and tags are placeholders. Edit them before relying on this route.
```

Proposed, dry run, brief:

```text
Would create .agents/memory/emerging/ideas/pricing/_pricing.md
  Would list it in .agents/memory/emerging/ideas/_ideas.md
No files were changed.
```

Proposed, already initialized:

```text
memory/emerging/ideas/pricing is already initialized. Nothing to do.
```

`normal` adds every entrypoint in the chain with `created` or `already
present`, and the `--framework` or generic scaffold choice. `full` shows the
content of each created file verbatim with real line breaks, the before and
after hashes of rewritten sections, and the recovery facts in words.

Status change: `complete` instead of `attention` for a scaffold that needs
authoring. JSON keeps `needsAuthoring` on each created entrypoint.

### `route create`

Question: Was the file created and listed?

Captured, apply (43 lines) with SHA-256 `Before:`/`Expected:` pairs and a
21-line `Unchanged:` roster.

Proposed, brief:

```text
Created .agents/memory/emerging/ideas/pricing/tiers.md  (memory/emerging/ideas/pricing/tiers)
  Listed in .agents/memory/emerging/ideas/pricing/_pricing.md
```

Captured, missing description (exit 4, 15 lines, `Description: ` empty,
`Next: open-forge route create --help`). Proposed:

```text
Cannot create the routed file: --description is missing.
Next: open-forge route create memory/emerging/ideas/pricing/plans --description "<one sentence>" --tag <Tag>
```

When several inputs are missing, all are named in one run. This needs the
binder to report every missing value at once, which the interaction phase owns.
The presentation contract requires the sentence to list every reported cause.

`normal` adds the metadata that was written (description, tags,
responsibility, template). `full` adds the hashes and the unchanged roster.

### `route update`

Captured, dry run (18 lines) with hashes and `metadata-field: description:
Pricing tier options -> description: Pricing tiers and their tradeoffs`.

Proposed, brief:

```text
Updated memory/emerging/ideas/pricing/tiers
  description: "Pricing tier options" -> "Pricing tiers and their tradeoffs"
  Entry updated in .agents/memory/emerging/ideas/pricing/_pricing.md
```

Template applied to a file that already has a body (`attention`, exit 2):

```text
Updated memory/emerging/ideas/pricing/tiers, but the Template body was not copied.
  The file already has authored content, which was kept.
```

No change: `memory/emerging/ideas/pricing/tiers already has these values. Nothing to do.`

### `route move`

Captured, dry run (26 lines) with `Ownership: unmanaged / framework=trusted /
extensions=trusted`, `References: coverage=complete, scanned=28, inspected=28,
occurrences=1` and effects in `file:<hash> -> file:<hash>` form.

Proposed, brief:

```text
Moved memory/emerging/ideas/pricing/tiers to .agents/memory/emerging/ideas/pricing/tier-options.md
  Entry updated in .agents/memory/emerging/ideas/pricing/_pricing.md
  1 link to the old path was found and left unchanged: .agents/maps/_maps.md:12:3
```

The third line appears only when the reference scan found occurrences outside
the generated entry. Moves and removals always list every deleted path.

Blocked by managed ownership (exit 5, stderr):

```text
Cannot move .agents/guidance/_guidance.md: it is managed by the Framework.
  Managed files are moved by update, not by route move.
```

`normal` adds the reference scan summary and each generated section touched.
`full` adds the before and after hashes.

### `route remove`

Proposed, brief:

```text
Removed .agents/memory/emerging/ideas/pricing/tiers.md
  Entry removed from .agents/memory/emerging/ideas/pricing/_pricing.md
```

Category removal lists every deleted file. A retained recovery bundle is
listed with its path and `Next: open-forge cleanup`.

### `extension list`

Captured, default (32 lines, `Source kind: embedded-catalogue`, `Installed:
coverage complete; record absent`, three lines per available package).

Proposed, brief:

```text
Installed
  development 0.1.0

Available
  development-toolkit 0.1.0   An optional bundle of project documents, Memory starters, planning, and development packages (5 packages)
  memory-starters 0.1.0       Copy-ready Memory Templates for decisions, ideas, analyses, observations, and handoffs
  orchestration 0.1.0         Coordinate dependent tasks through one optional managed-delivery workflow (3 packages)
  planning 0.1.0              An optional planning Workflow, Work Records Pattern, and Templates for tasks, plans, backlogs, and checkpoints
  project-documents 0.1.0     Optional Vision and Architecture Workflows with document Templates for a project's direction and structure
Next: open-forge extension install <id>
```

`Installed: none` when nothing is installed. Descriptions are shown at brief
because they are how a person chooses. An installed package whose source is no
longer readable, or whose files changed, gets a note on its row.

`normal` adds the source path when `--source` was used and each package's
dependency list. `full` adds managed path counts and lock coverage.

### `extension inspect`

Captured, `--view compact` (17 lines) including `Paths summarized: 3. Use
--view expanded for their observations.`

Proposed, brief, unchanged:

```text
development 0.1.0 is installed and matches the bundled package.
  3 files under .agents/workflows
```

Proposed, brief, one changed and one retired path (fixture case):

```text
toolkit 1.0.0 is installed. Version 2.0.0 is available. 2 files need attention.
  .agents/changed.md    changed since it was installed
  .agents/retired.md    no longer part of the package
Next: open-forge extension update toolkit --dry-run
```

`normal` lists every managed path with its relation (`unchanged`, `changed`,
`retired`, `missing`). `full` adds both SHA-256 values per path, the manifest
path, dependencies and resolution order.

### `extension create`

Not captured. Proposed, brief:

```text
Created the Extension scaffold at ./packages/my-tools
  Edit packages/my-tools/extension.json, then add files under packages/my-tools/content/.agents/
```

Dry run uses `Would create`. A collision at the destination is `blocked` and
names the occupied path.

### `extension install`

Captured, `development`, apply (79 lines): mode, force, automatic, selection
method, `Permissions: not-required; settings none; not-requested`, four effect
blocks, 21 `Generated navigation observed for installation.` pairs, record,
recovery, verification and the Framework fingerprint.

Proposed, brief:

```text
Installed the development Extension.
  Created 3 files under .agents/workflows
  Updated the Entries section of .agents/workflows/_workflows.md
```

Proposed, with dependencies, brief:

```text
Installed the orchestration Extension and 2 packages it requires: planning, project-documents.
  Created 9 files under .agents/workflows, .agents/patterns and .agents/templates
  Updated the Entries section of 3 files
```

Proposed, permission required, non-interactive (exit 5, stderr):

```text
Cannot install team-tools: it writes outside .agents and no grant allows that.
  tools/review (directory)
Next: open-forge extension install team-tools --allow-path tools/review
  Or add "tools/review" to allowInstallPaths in .agents/open-forge.json.
```

Proposed, package has no content (`attention`, exit 2), which today is a
silent `Effects: 0` success:

```text
Nothing was installed from ./mypkg: the package has no content directory.
  Package files belong under ./mypkg/content/.agents/
```

Dry run: `Would install ...` with the same lines and `No files were changed.`

`normal` lists every created file, the Entries sections rewritten, and the
dependency order. `full` adds the unchanged navigation observations, the
permission evaluation, recovery paths and verification facts.

JSON: `brief` keeps selection, packages, permissions decision, all effects,
findings, lifecycle outcome and recovery. `normal` adds per-effect kinds.
`full` adds unchanged navigation observations and the Framework fingerprint.

### `extension update`

Captured, dry run (76 lines) with three fingerprint pairs and the unchanged
navigation roster.

Proposed, no change, brief:

```text
development 0.1.0 is up to date. Nothing to do.
```

Proposed, changes, brief:

```text
Updated the development Extension to 0.2.0.
  .agents/workflows/review.md        updated
  .agents/workflows/development.md   kept (you changed it; add --force to replace)
  .agents/workflows/old.md           kept (retired; add --prune to delete)
```

### `extension remove`

Captured, dry run (68 lines).

Proposed, brief:

```text
Removed the development Extension.
  Deleted 3 files under .agents/workflows
  Updated the Entries section of .agents/workflows/_workflows.md
  The deleted files are kept in a recovery bundle at <recovery-path>.
Next: open-forge cleanup  (after reviewing the bundle)
```

Proposed, shared file kept, brief:

```text
Removed the toolkit Extension.
  Deleted .agents/changed.md and .agents/unchanged.md
  Kept .agents/shared.md (still owned by survivor)
  .agents/missing.md was already gone; its ownership was released.
```

Every deleted path is listed at brief, always. `normal` adds owners per path.

### `library list`

Captured, none registered (8 lines):

```text
No Libraries are registered.
Status: complete
Workspace: <ws>
Selected by: current directory
Record: complete (.agents/open-forge.lock.json)
Registered Libraries: 0
Source inventory: not scanned (library list checks registered links only)
Checks: complete
```

Proposed, brief:

```text
No Libraries are registered.
Next: open-forge library attach <id> <source-root>
```

Fixture, one Library with a missing link (`attention`). Proposed, brief:

```text
1 Library registered. 1 link needs attention.
  team-knowledge    shared/team -> .
    .agents/directives/review.md    missing
Next: open-forge library sync team-knowledge
```

`normal` lists every registered link with its state. `full` adds expected and
observed link targets.

### `library inspect`

Proposed, brief:

```text
team-knowledge links 12 files from shared/team under docs/. 11 current, 1 missing.
  docs/review.md    missing
Next: open-forge library sync team-knowledge
```

### `library attach`

Proposed, brief:

```text
Registered the team-knowledge Library from shared/team.
  Created 12 links under docs/
```

Proposed, permission required, non-interactive (exit 5, stderr):

```text
Cannot attach team-knowledge: docs/ is outside .agents and no grant allows writing there.
Next: open-forge library attach team-knowledge shared/team --to docs --allow-path docs
```

Fixture, interrupted after one of two links (exit 130, stderr). Captured
expanded (35 lines). Proposed, brief:

```text
Library attach was interrupted after 1 of 2 links were created.
  Created:      docs/first.md
  Not created:  docs/second.md
  The Library record was not written. Recovery data is kept at .agents/recovery/pending.json.
Next: open-forge doctor
```

### `library sync`

Proposed, brief:

```text
Synchronized team-knowledge: 2 links added, 1 removed, 10 unchanged.
  Added:    docs/new-a.md, docs/new-b.md
  Removed:  docs/old.md
```

No change: `team-knowledge is up to date. Nothing to do.`

### `library detach`

Proposed, brief:

```text
Detached team-knowledge: removed 12 links under docs/. Source files in shared/team were kept.
```

### Shared error transcripts

Captured, unknown command (exit 4): `Unrecognized command or argument 'statuss'.`
Proposed (typo suggestions are a phase 5 addition):

```text
Unknown command 'statuss'. Run open-forge --help to list commands.
```

Captured, invalid enum value (exit 4, raw tab):

```text
Argument 'Expanded' not recognized. Must be one of:
	'compact'
	'expanded'
```

Proposed:

```text
'Expanded' is not a valid --detail value. Use brief, normal, or full.
```

Every `invalid` result is at most three lines: the sentence, the subject, the
corrected command.

## Part 3 — Decisions

Each decision states the recommendation. Nothing here is approved.

### D1. Retire `--view` for `--detail brief|normal|full`, default `brief`

Recommendation: yes, as [R1](#r1-one-detail-axis---detail-briefnormalfull).
Consequence: the global-flag contract, every command's help, `docs/cli.md`,
and every `--view` mention in 20 interface contracts change. No alias.

### D2. Where doctor warnings sit

Recommendation: warnings are counted at `brief` and listed at `normal`, per
the maintainer's stated expectation that a default `doctor` says "no errors"
and gives counts. Alternative: list warnings at `brief` too. That matches
"show what changed the exit code" and saves a second invocation, but on this
repository it prints roughly 700 rows by default, which is the problem being
solved. The recommended shape keeps a one-line hint naming the command that
lists them.

### D3. `--verbose` stays separate

Recommendation: keep it, unchanged, as the directive requires. Alternative:
fold diagnostics into `full`. Rejected because stderr diagnostics are for
debugging the CLI and would contaminate stdout results.

### D4. Coverage-kind findings presented as counts

Recommendation: the five coverage kinds in [R8](#r8-findings-model-in-presentation)
render as counts below `full` and stay findings in the model and in `full`
JSON. This is grouping, not deletion. Removing them from the model is a
phase 5 decision.

### D5. Finding codes in brief text

Recommendation: omit the bracketed code at `brief`, keep it at `normal` and
`full`. Alternative: always print it. Codes are the stable search key and cost
little; the maintainer may prefer them everywhere.

### D6. Plain JSON scalars

Recommendation: replace `{ state, value }` wrappers with a number or `null`,
and carry the reason for `null` as a finding or limitation that already
exists. This is a schema change and belongs in the same schema-3 bump.
Alternative: keep wrappers. Cheaper now, but the wrapper is 35 times 47 bytes
in `status --json` alone and every consumer must unwrap.

### D7. Previous-content pointer after `update`

The accepted state-files decision says `update` points at `git diff` when
`.git` exists and otherwise at the recovery bundle. Successful commands delete
their bundle after verification, so the second branch has nothing to point at.
Recommendation: print `Previous content: git diff` when `.git` exists,
otherwise print nothing, and record the gap against the decision. Alternative:
retain the bundle after `update` replaces changed content, which is a
behavior change outside G4.

### D8. `references` and generated links

`references` reports zero links for stock entrypoints because it counts
authored links only. Recommendation: G4 fixes the wording (`no authored links`
plus the generated-links note) and phase 5 decides whether generated links
are counted. Presentation cannot fix a coverage question.

### D9. Minified JSON always

Recommendation: yes. Alternative: pretty at `full`. Rejected to keep one shape.

### D10. Status exit codes to correct

Recommendation: `route init` scaffold is `complete`. `status` not installed
stays `complete`. Both are contract changes for their commands.

### D11. Wizard wording

No interactive selector exists. Recommendation: text and help say `prompt`,
never `wizard`, and the extension selection prompt says what it accepts. The
selector itself is phase 5.

### D12. Confirmation before the plan

`install` and `update` ask `Apply this Install plan? [y/N]` before printing the
plan. Recommendation: render the brief plan, then ask. This changes the
interactive sequence and is listed for the maintainer to place in G4 or
phase 5.

## Part 4 — Implementation handoff

This is the packet for the implementing agent, to be executed only after the
rules and transcripts above are approved.

### Affected commands and contracts

All 28 commands. Shared: `contracts/shared/global-flags/interface.md` and
`behavior.md`, `contracts/shared/result-coordinates/interface.md` and
`behavior.md`, `shared-operation-contract.md` (the `Next:` and stream
paragraphs), `docs/cli.md`, `README.md` where it shows output. Per command:
the `Human Output` (or `Output`) and `Structured Output` sections and the
`Compact JSON Output` section of every interface contract, and the scenarios
that quote output.

### Preserved behavior

- The seven statuses, their exit codes and the stdout/stderr mapping.
- One operation, one result, one primary output, one exit. Renderers never
  rerun anything.
- Every finding kind and code. Deterministic finding and effect order.
- Counts and coverage describe the whole operation at every level.
- Selected authored content is byte-exact in every view.
- Mutation receipts are complete in JSON at every level. Every replaced,
  deleted or kept-divergent path is visible in text at every level.
- `--dry-run` semantics, `--automatic`, `--force`, `--prune`, `--allow-path`.
- Colour capability detection and `NO_COLOR`.
- Paths, identifiers and commands are never truncated.

### Intended output changes

- `--view` removed. `--detail` added. `brief` default.
- Headline sentences replace the `Status:` header block.
- Workspace line only under [R9](#r9-context-echo).
- Per-command content by level as in Part 2.
- One JSON schema (3) with `detail`, minified, plain scalars, level-scoped
  membership.
- One human escaper, serializer-owned JSON escaping (M3).
- ASCII framing, platform line endings applied once to framing.
- `route init` status change.
- `find` brief rows without header; `context` brief stream without labels.
- Help option text for `--detail` and `--json`.

### Verification requirements

- Before any renderer changes: capture and commit the current output of every
  command at both current views, text and JSON, for the seeded states listed in
  Part 2, using the AOT-safe in-process snapshot tool from phase 7. Review and
  commit that capture separately. The scratch capture used for this proposal is
  evidence for design, not the committed baseline.
- After each slice: regenerate snapshots, review the diff against the approved
  transcripts, commit the contract update in the same commit as the behavior.
- Invariants, added once and run across all commands: `lines(brief) <=
lines(normal) <= lines(full)`, errors before warnings before informational,
  no `\uXXXX`, `\\` or literal `\n` in human text outside the escaper's own
  visible sequences, valid UTF-8 and ASCII framing, no `Status:` line, zero
  findings never render a findings block, every listed finding has a path or
  identifier, `Next:` at most once and last.
- Stream and exit tests for the two status corrections.
- The complete managed suite and the supported Native AOT gate at the end of
  the renderer wave, because presentation is a public boundary.

### Slices

| Slice | Content                                                                                                                                                                                                      | Changes output |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------- |
| G4-0  | Snapshot tool finished. Before-capture of all 28 commands, both views, text and JSON, for the seeded states. Reviewed and committed alone.                                                                   | no             |
| G4-1  | Shell: `CliDetail` replaces `CliView`; `--detail` option; `CliPresentation` carries detail; renderer set takes one renderer per format that receives the level; envelope v3 with `detail`; help option text. | yes            |
| G4-2  | Shared human text: one escaper (M3), `DiagnosticValueLimit` moved, framing writer with platform line endings, headline, finding-row, count and `Next` helpers. Delete the seven escapers.                    | yes            |
| G4-3  | `status`, `doctor`, `index`, `repair`, `cleanup`. Contracts updated in the same commits.                                                                                                                     | yes            |
| G4-4  | `install`, `update`, `context`, `find`, `references`.                                                                                                                                                        | yes            |
| G4-5  | `route` family (7).                                                                                                                                                                                          | yes            |
| G4-6  | `extension` family (6) and `library` family (5).                                                                                                                                                             | yes            |
| G4-7  | Invariant tests, snapshot regeneration and review, `docs/cli.md`, README output samples, deletion of `DoctorHumanSnapshots` and verbatim copy assertions. Full managed and AOT gate.                         | no             |

Each output-changing slice commits the reviewed snapshot diff together with
its contract change.

## Part 5 — Discoveries and deviations

Recorded here so they are not lost. None is a G4 presentation decision.

- **`index` deleted authored prose after `## Entries`.** In the scratch
  workspace, one line was appended after the Entries section of
  `.agents/maps/_maps.md`. `status` reported the file as changed and its
  section as stale. `index` then rewrote the section body to `- none - No
entries - #Empty` and the appended line was gone. The recovery bundle was
  removed after verification, so nothing preserves it outside Git. The
  heading-based section from B1 extends to end of file, so any text a person
  writes after the generated list is treated as generated interior. This is a
  data-loss edge case for the maintainer to decide on, separate from G4.
- **`status` and `update` disagree about that file.** `status` called it
  `changed`; `update --dry-run` reported `current: same; intended: same`
  because the semantic fingerprint excludes the region interior. One verdict
  per state is a phase 5 item.
- **`route init` still exits 2 on success**, and `route list` still doubles
  backslashes in the workspace path, `references` still reports zero links for
  stock entrypoints, and `find --view compact` still mixes findings into TSV.
  All confirmed on the current build and covered above.
- **The dev executables were stale.** Both the linked CLI on `PATH` and the
  managed dev artifact predated G1. The artifact was rebuilt from the tree
  before capture. Nothing was linked or published.
- **The 258 MB doctor result is unmeasured here.** Doctor was not run.
  Selection happens after the model is built, so `brief` reduces output, not
  memory. If the size is a memory problem as well, that is a separate
  measurement.
- **`--view` was never used in the proposed invocations**, per direction. The
  captured blocks use it because the current build only accepts it.

## Corrections after reading the sibling proposals

Recorded on 2026-09-14 after the astra and opus proposals were read. The
transcripts above are kept as written so the independent proposal stays
reviewable. The [consolidated proposal](phase-4b-g4-output-proposal-consolidated.md)
carries the corrected wording.

- **`update` replaces changed owned files without `--force`.** `UpdatePlanningPolicy`
  maps a changed current file to `Replace` and a missing one to `Restore`, and
  the Update interface says `--force` grants no additional authority. The
  "Kept 2 files that you changed" transcript above is wrong. The only kept case
  is retired content, which needs `--prune`.
- **`route move` rewrites references that would break, and `route remove`
  detaches incoming links**, per their interface contracts' complete reference
  pass. The move transcript above says a link was left unchanged. It is
  rewritten.
- **`--verbose` and `--json`.** The maintainer had already directed, in the
  astra session, that diagnostic verbosity folds into the top detail level and
  that a format option replaces `--json`. D3 and the choice to keep `--json`
  above are superseded by the consolidated proposal.
- **Parser failures never emit a JSON envelope.** `CliCoreApplication` writes
  them to stderr with exit 4 before binding. The proposal above did not state
  that exception.
