---
open-forge:
  description: Redesign the rendering system around one report model, one selection stage, generic text and JSON renderers, one escaper, and the physical layout for the later project split
  tags: [Memory, Working, CLI, Task, Plan, G4, Presentation, Architecture, Contextual, Active]
---

# 03 — Rendering system

> Read [00 — G4 conventions](00-conventions.md) first. This is the first
> priority of the packet. Every command subtask builds on it.

## Goal

One data model, the report, carries every command result from the operation
layer to rendering. One selection stage turns a report and the requested
detail into the facts a reader sees. One text renderer and one JSON renderer
write every command. Command-local presentation shrinks to two small pieces:
a selector that maps the command's result into a report, and a text renderer
for the command's own data rows. Every finding in the CLI has one severity,
one code, one subject and one sentence. The 25,000 lines of per-command
renderers, the retired JSON variants, the seven escapers, and the former
presentation axes are gone. The layout lets the rendering layer move into its
own project later by moving one folder.

## Depends on / Blocks

- Depends on: [01](01-before-snapshots.md), [02](02-naming.md).
- Blocks: [04](04-interaction-system.md), every command subtask, [40](40-verification.md).

## Why this shape

The [Presentation layer record](../../../../crystallized/documents/cli/layers/presentation.md)
names the selection stage and says renderers must not decide what to include.
The [Shell layer record](../../../../crystallized/documents/cli/layers/shell.md)
provides the pipeline boundary. The merged implementation now routes all 28
commands through one report model and one selection stage, so a healthy
`doctor` report and machine output share the same typed facts.

The maintainer's direction (C5, C7): one model from the data layer to
rendering, no duplication anywhere, and the detail level applies to JSON too.

## The three layers and where files live

Three layers, mirroring the future project split (C18), without creating
projects now:

```text
Shell             src/cli/core/OpenForge.Cli.Core/Shell/          arguments in, exit code out
Data processing   src/cli/core/OpenForge.Cli.Core/Framework/      facts
                  src/cli/core/OpenForge.Cli.Core/Commands/       operations and result models
Rendering         src/cli/core/OpenForge.Cli.Core/Presentation/   report model, selection, renderers, text, prompts
```

Rules, checkable from the `using` graph and added as a unit test in step 12:

- `Presentation/Shared/**` references `Shell/Definitions` (statuses, exits)
  and nothing under `Commands/` or `Framework/`.
- `Presentation/<Command>/**` references `Presentation/Shared/**` and that
  command's `Commands/<Command>/Models/**`. Nothing else.
- Nothing under `Commands/` or `Framework/` references `Presentation/`.
- `Shell/Pipeline` references `Presentation/Shared` to run selection and
  rendering. `Shell/Composition` wires command selectors.

Per-command presentation moves from `Commands/<Cmd>/Shared/Rendering/` to
`Presentation/<Cmd>/`, mirroring the `Commands/` tree one to one, the way the
test projects mirror production. This is a deliberate departure from
command-folder locality, accepted by the maintainer for the split. The later
split moves `Presentation/` into `OpenForge.Cli.Rendering`, which references
the data project for result models; the data project never references it.

## The report model

Schematic C#. Names follow [02](02-naming.md) and are final only when that
task is accepted.

```csharp
namespace OpenForge.Cli.Core.Presentation.Shared.Models;

internal sealed record CliReport<TData>(
    string Command,                                // "doctor", "extension install"
    CliStatus Status,                              // the renamed seven statuses
    CliHeadline Headline,                          // sentence + kind
    CliWorkspaceEcho? Workspace,                   // path + how selected
    IReadOnlyList<CliFinding> Findings,            // every finding, all severities
    IReadOnlyList<CliEffect> Effects,              // every effect of a mutation
    IReadOnlyList<CliCount> Counts,                // named counts, typed unknowns
    IReadOnlyList<CliLimitation> Limitations,      // checks that could not finish
    TData Data,                                    // command-owned rows or content
    CliRecovery? Recovery,                         // bundle path + disposition
    CliNextAction? Next,                           // one command or one sentence
    IReadOnlyList<string> Diagnostics)             // debug-only, bounded
    where TData : class;

internal sealed record CliHeadline(string Sentence, CliHeadlineKind Kind);
internal enum CliHeadlineKind { Done, NothingToDo, Preview, Warnings, Incomplete, CannotStart, Blocked, Failed, Cancelled }

internal sealed record CliFinding(
    CliSeverity Severity,                          // Error, Warning, Info
    string Code,                                   // "reference.target-missing"
    string Title,                                  // "Broken link"
    string Message,                                // one sentence, may name the cause
    CliSubject Subject,                            // path or id, location
    string? Category,                              // doctor category, else null
    CliResolution? Resolution,                     // lane, doctor and repair only
    IReadOnlyList<CliNextAction> Actions,          // per-finding actions
    IReadOnlyList<CliCandidate> Candidates,        // possible targets, full only
    IReadOnlyList<CliEvidence> Evidence,           // label + value, full only
    CliProvenance? Provenance);                    // read from, full only

internal sealed record CliSubject(CliSubjectKind Kind, string? Path, string? Id, SourceLocation? Location);
internal sealed record CliCandidate(CliSubject Subject, IReadOnlyList<string> Reasons);
internal sealed record CliEvidence(string Label, string Value);
internal sealed record CliProvenance(string Source, string? Path, SourceLocation? Location);

internal sealed record CliEffect(
    string Path,
    CliEffectKind Kind,                            // File, Directory, Section, Link, Record, Setting
    CliEffectAction Action,                        // Created, Replaced, Restored, Deleted, Kept, Rewritten, Detached, Released
    CliEffectOutcome Outcome,                      // Planned, Done, NotStarted, Unknown, Failed
    string? Reason,                                // "you changed it", "no longer part of this release"
    string? Owner,                                 // package or Library id
    string? Before,                                // SHA-256, full only
    string? After);                                // SHA-256, full only

internal sealed record CliCount(string Name, string Label, long? Value, string? UnavailableReason);
internal sealed record CliLimitation(string What, string Why, CliSubject? Subject);
internal sealed record CliRecovery(string? Path, CliRecoveryDisposition Disposition);
internal sealed record CliWorkspaceEcho(string Path, bool Explicit);
```

`CliNextAction` already exists and is reused: `Command` is either a runnable
command line or a short sentence, and `Reason` explains it. Add a `Kind`
(`Command`, `Sentence`) so renderers know whether to prefix `Next:` with a
command.

What is deliberately not in the model: view or level (the report is
complete), status labels as text, any pre-rendered line, any enum spelled as
a string except `Code`.

## The selection stage

```csharp
internal sealed record CliSelection(CliDetail Detail, IReadOnlySet<CliSeverity>? Filter);

internal delegate CliReport<TData> CliReportSelector<TResult, TData>(TResult result, CliSelection selection)
    where TResult : ICliCommandResult where TData : class;

internal static class CliReportTrimmer
{
    // Always runs after the command selector. Enforces the shared rules so a
    // command cannot leak past its level.
    internal static CliReport<TData> Trim<TData>(CliReport<TData> report, CliSelection selection, CliCommandShape shape) where TData : class;
}
```

The command selector builds the report from the concrete result. It may
consult `selection.Detail` to avoid building rows the level will not show
(this is how `doctor` at `minimal` never materializes finding rows). The
trimmer then applies the shared rules once, for every command:

| Rule           | Trimmer behavior                                                                                                                                                                                                                                                                    |
| -------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Listing ladder | Keeps findings whose severity is listed at the level for the command's shape (`Diagnosis` shape counts warnings at `minimal`; every other shape lists them). `Filter` replaces the ladder. Counts of dropped findings are added as `CliCount`s named `errors`, `warnings`, `infos`. |
| Depth          | Below `full`: clears `Candidates`, `Evidence`, `Provenance`; below `standard`: clears per-finding `Actions` except the first and clears `Resolution`. `Code` is kept in the model (JSON needs it); the text renderer hides it below `full`.                                         |
| Effects        | Below `full`: clears `Before` and `After`. Never drops an effect.                                                                                                                                                                                                                   |
| Diagnostics    | Below `debug`: cleared.                                                                                                                                                                                                                                                             |
| Workspace echo | Below `standard`: kept only when `Explicit` or the status is blocked, failed or cancelled.                                                                                                                                                                                          |
| Ordering       | Sorts findings by severity, then path, then line, then column. Effects keep the command's order.                                                                                                                                                                                    |

`CliCommandShape` is `Summary`, `Diagnosis`, `Data`, `ChangeReport`, declared
by the binding.

## The renderers

Two generic renderers replace 84 command renderers.

**Text.** `CliTextRenderer.Render(report, selection, style, dataRenderer)`
writes, in this order: headline; `Workspace:` line when present; findings
(errors, then warnings, then infos when listed), grouped under category
headings only when categories exist and the level is `standard` or above;
effects as aligned rows; the command's data through `dataRenderer`; kept and
limitation sentences; counts as one sentence; `Next:`. Each finding row is
`  <Severity>  <path:line:column>  <Title>` with the message on the next
line, indented under the title, the first action on the line after, and at
`full` the code in brackets after the title. Effect rows are
`  <path>  <action phrase>[ (<reason>)]`. Data renderers are command-local
and receive `style` and `selection` so they can align columns and omit detail.

**JSON.** `CliJsonRenderer.Render(report, selection, dataContext)` writes the
envelope:

```text
{
  "schemaVersion": 3,
  "command": "...",
  "status": "...",
  "detail": "minimal" | "standard" | "full" | "debug",
  "filter": ["warning"] | null,
  "workspace": { "path": "...", "selectedBy": "current-directory" | "explicit-workspace" } | null,
  "summary": { "headline": "...", "kind": "done" | ... },
  "findings": [ { "severity", "code", "title", "message", "subject": { "kind", "path", "id", "location" }, "category", "resolution", "actions": [...], "candidates": [...], "evidence": [...], "provenance" } ],
  "effects": [ { "path", "kind", "action", "outcome", "reason", "owner", "before", "after" } ],
  "counts": { "<name>": number | null, ... },
  "limitations": [ { "what", "why", "subject" } ],
  "data": { command-owned },
  "recovery": { "path", "disposition" } | null,
  "next": { "kind", "command", "reason" } | null
}
```

Minified. Members omitted by the level are absent, not `null` and not `[]`:
`candidates`, `evidence`, `provenance` appear only at `full` and `debug`;
`before` and `after` only at `full` and `debug`; `resolution` from `standard`.
`counts` values are plain numbers or `null` with the reason in `limitations`.
The `data` member is serialized through the command's source-generated
context; each command subtask documents its `data` membership per level.
Parser failures before binding keep their current stderr text path.

**Diagnostics.** `CliDiagnosticRenderer` writes `report.Diagnostics` to
stderr at `debug`, each value bounded to 240 characters, total bounded to
4,096, escaped through `CliText.Escape`. It replaces every
`*DiagnosticRenderer`.

## Text primitives (Task 31 M3)

`Presentation/Shared/Text/`:

- `CliText.Escape(string)`: printable characters unchanged, including `\`,
  `"`, `<`, `>`, non-ASCII. `\n`, `\r`, `\t` become the visible two-character
  sequences. Other control characters and lone surrogates become `\uXXXX`.
  This is the only escaper in the tree.
- `CliText.Clamp(string, int)`: Unicode-scalar truncation with ASCII `...`,
  used only by the diagnostic renderer.
- `CliText.Plural(long, string singular, string? plural = null)`.
- `CliText.Tokens(long)`: `about 8.0k tokens`; `CliText.Bytes(long)`: IEC.
- `CliTable`: rows of cells, computed widths, two-space gutter, no
  truncation.
- `CliTextStyle`: the existing palette plus bold for subject paths and dim
  for `Next` reasons and counts, enabled per stream from the host's
  capability record. Never applied to data rows that are authored content.
- Line endings: renderers build with `\n`. The output stage converts framing
  to the platform newline once. Authored content spans (context bodies, index
  diff lines, template bodies) are passed through as `CliAuthoredSpan` values
  that the output stage writes byte-exact. `PlatformLineEndings` moves here
  and keeps its LF, CRLF, CR and mixed tests.
- JSON escaping is the serializer's, with `JavaScriptEncoder.UnsafeRelaxedJsonEscaping`
  on every context. No hand-written JSON escaper remains.

## Pipeline change

```text
CliInvocationResolution
  -> CliOperationRequest<TRequest>
  -> CliOperationResult<TResult>
  -> CliReport<TData>            (command selector, then trimmer)
  -> CliRenderedOutput           (text or JSON, plus diagnostics)
  -> CliOutputReceipt
  -> CliProcessCompletion
```

`CliCommandBindingComponents<TRequest, TResult>` gains `Selector`,
`DataTextRenderer`, `DataJsonTypeInfo` and `Shape`, and loses `Renderers` and
`DiagnosticRenderer`. `CliPresentation` becomes `(Format, Detail, Filter, Colors)`.
`CliSyntaxDefinitions` registers `--detail`, `--detail-filter` (repeatable,
typed enum, union, `all` wins) and `--format`. Repeating `--detail` or
`--format` stays invalid.

## Migration bridge — historical

> Superseded by the completed G4 migration. The bridge description below is
> retained as implementation history; the current CLI has no bridge or legacy
> compact/expanded renderer.

To keep every command working while lanes migrate in parallel, 03 ships a
bridge and deletes it in [40](40-verification.md):

- `LegacyReportSelector<TResult>` produces a report with the headline taken
  from the old renderer's first line, no findings, no effects, and
  `Data = result`.
- `LegacyDataTextRenderer<TResult>` calls the old human renderer with
  `minimal -> compact` and every other level `-> expanded`.
- The JSON renderer serializes `Data` with the old result context under the
  new envelope.

An unmigrated command therefore already speaks the new flags and envelope,
with its old body. A command subtask replaces its bridge registration with
its real selector and data renderer, and deletes its old renderers.

## Golden slice

`index` is migrated inside this task as the representative slice: it has a
headline, effects, findings, a data renderer (the dry-run diff), counts and a
`Next`. [14](14-index.md) then completes its catalogue. The early architecture
checkpoint required by the Program Architecture Directive happens after this
slice and before any lane starts.

## References

- `src/cli/core/OpenForge.Cli.Core/Shell/Pipeline/CliPipelineStages.cs` — add the selection stage between operation and rendering.
- `src/cli/core/OpenForge.Cli.Core/Shell/Pipeline/CliCommandPipeline.cs` — thread the selector and the two renderers.
- `src/cli/core/OpenForge.Cli.Core/Shell/Composition/Models/CliCommandBindingComponents.cs` — new members.
- `src/cli/core/OpenForge.Cli.Core/Shell/Definitions/CliPresentationDefinitions.cs`, `CliSyntaxDefinitions.cs` — enums and options.
- `src/cli/core/OpenForge.Cli.Core/Shell/Presentation/**` — moves to `Presentation/Shared/**`.
- `src/cli/core/OpenForge.Cli.Core/Shell/Presentation/Shared/Rendering/CliHumanText.cs:66` — `PlatformLineEndings` moves to `CliText`.
- The seven escapers listed in [Task 31 M3](../task31/phase-escaper.md) — deleted.
- `src/cli/core/OpenForge.Cli.Core/Shell/Presentation/Models/CliCompactJsonDocument.cs`, `CliCompactJsonProjection.cs` — deleted.
- `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs` and the four composers — register selectors and shapes.
- `src/cli/root/OpenForge.Cli/Hosting/CliHost.cs:17` — colour capabilities unchanged.
- `src/cli/core/OpenForge.Cli.Core/Commands/Index/Shared/Rendering/*` — golden slice.
- Contracts affected (record in the ledger, do not edit): global flags, result coordinates, the shared operation contract's `Next:` and stream paragraphs, every command's output sections, the CLI implementation Directive's verbose sentence.

## Preconditions

- [x] 01 committed. 02 accepted.
- [x] Three suites green.

## Steps

1. [x] Create `Presentation/Shared/{Models,Selection,Text,Rendering}` and move
       `Shell/Presentation/**` there, updating namespaces. Verify: build.
2. [x] Add the report model records and `CliDetail`, `CliFormat`,
       `CliSeverity`, `CliCommandShape`. Verify: build.
3. [x] Add `CliText` with `Escape`, `Clamp`, `Plural`, `Tokens`, `Bytes`,
       `PlatformLineEndings` and their unit tests: `"`, `\`, tab, CR, LF,
       CRLF, mixed, lone surrogate, astral pair, a value at and over 240.
       Verify: unit green.
4. [x] Add `CliTable` and `CliTextStyle` with unit tests for alignment,
       plain versus styled output, and no styling on authored spans.
5. [x] Add `CliReportTrimmer` with unit tests over synthetic reports for every
       rule in the table above, including `Diagnosis` versus other shapes and
       `--detail-filter` unions.
6. [x] Add `CliTextRenderer`, `CliJsonRenderer`, `CliDiagnosticRenderer` with
       unit tests over synthetic reports at all four levels and both formats.
       Assert text and JSON list the same finding and effect identities in the
       same order.
7. [x] Use `--detail`, `--detail-filter`, and `--format` in
       `CliSyntaxDefinitions` and the global input reader. Update help option
       text. Verify: parser unit tests, e2e help snapshot.
8. [x] Change the pipeline and binding components; register every command
       through its selector and data renderer. Verify: the suites are green
       with the before snapshots regenerated only for accepted report changes.
       Review that diff: it must show the new envelope and the flag rename
       and nothing about message wording.
9. [x] Migrate `index` as the golden slice: selector, data renderer for the
       diff, deletion of its three old renderers. Verify: index snapshots
       regenerated and reviewed against [14](14-index.md).
10. [x] Delete the retired compact-result types, legacy renderer set, seven
        escapers, and former view/verbosity uses. Verify: build and the scoped
        repository search return none.
11. [x] Set the relaxed JSON encoder on every source-generated context.
        Verify: a unit test serializes `<`, `&`, a newline and an emoji.
12. [x] Add the dependency-direction unit test over the `using` graph for the
        four rules above. Verify: green.
13. [x] Run the complete managed suite and the supported Native AOT gate
        (shared boundary change). Record counts.

## Expected result

Every command accepts `--detail`, `--detail-filter`, and `--format`, renders
through the shared native report, and emits the schema-3 envelope. The
invariants from [40](40-verification.md) that concern the envelope and flags
are green.

## Acceptance

- [x] No file under `Commands/` or `Framework/` references `Presentation/`.
- [x] One escaper, one trimmer, two renderers, one envelope.
- [x] `index` renders its [14](14-index.md) catalogue at all four levels in both formats.
- [x] Every command renders through the shared report pipeline with the new
      envelope.
- [x] Managed suite and Native AOT gate green; counts recorded.

## Changes ledger

> The migration evidence below is retained as a historical receipt. The bridge
> and its legacy renderers were removed during the completed G4 qualification;
> current behavior is the shared native report described above.

### Execution applicability

- This is a shared presentation and composition change in the unreleased local
  workspace CLI. It changes output and selection, not mutation authority or
  filesystem safety; source changes are reversible through the task worktree.
- Reuse the pinned BCL, System.CommandLine and generated System.Text.Json
  metadata. No dependency, reflection, native or custom parser is introduced.
  The former temporary legacy presentation bridge was removed in 40.
- Unit evidence owns selection, escaping, report rendering, parser and typed
  composition contracts; existing integration and EndToEnd suites cover real
  command effects and public output. Complete managed and supported Native AOT
  gates are required because shared composition and serialization change.
- Beginning baseline: 01 commit `5c388e7c5747a8a229591807f1b3693e4dd7b14e`,
  tree `878b646ed748662f9be7cd0bcb4da7dd0f5f3441`; Unit 3382 passed,
  Integration 2180 passed / 17 expected skips, EndToEnd 131 passed. The five
  recorded formatter errors predate this implementation.

### Accepted implementation

- Review corrections add present `Before:` and `After:` hash rows below shared
  effects at full/debug, preserving raw JSON values and escaping text once.
  Finding resolution phrases follow the message at full/debug; standard JSON
  still retains the resolution lane. Targeted-operation findings require an
  established action with `Kind.Command`; the first such action supplies the
  named command, skipping sentence actions. Selection rejects a missing command
  before filtering or trimming, so every format/detail enforces this invariant.
  Informational findings add no resolution phrase. Independent literal tests
  cover all six lanes, optional hashes, escaping and the malformed report.
- Index's one-file and one-section sentences now use singular grammar under
  00's message rule. Plural catalogue strings remain exact. Existing Index
  Unit, Integration and process literals are aligned, and explicit zero/one/
  plural factory evidence uses independent literal expectations. These review
  corrections initially awaited the next compiled runtime gate; the final
  accepted gate and reviewed snapshots are recorded below.
- Native Index passes raw section paths, joined selection paths and recovery
  paths into its wording factories. The text renderer escapes the complete
  returned sentence once at the write boundary; authored diff spans bypass it.
- Native Index's entry-count table now styles its subject-path column. The
  shared table accepts optional cell styling after escaping and visible-width
  measurement, so ANSI sequences do not alter alignment or pass through the
  escaper. Independent table and all-four-detail Index preview evidence checks
  exact alignment, plain/colour parity and unchanged plain authored diff bytes.
- Snapshot review corrected two output-order defects without changing words:
  Index's dry-run closing sentence follows its diff, selection and recovery
  details, and the shared Next reason precedes the final one-line Next command.
  Minimal continues to omit the reason; legacy bodies with `ShowNext=false`
  remain untouched. Independent tests cover all four detail levels, the exact
  closing sequence and retained JSON Next facts; authored diff bytes stay exact.
- Added the generic complete report, severity/detail selection and one shared
  text, JSON and diagnostic rendering path. JSON is minified schema 3 with
  `detail`, `filter`, `data`, findings, effects, counts, limitations and recovery.
  Active data contexts use generated metadata and the relaxed JSON encoder.
- Added neutral `CliRequestBinding<TRequest,TResult>`; executable composers close
  it with `CliReportRendering<TResult,TData>`. All 28 registrations use the
  three-type pipeline through rendered output, output receipt and completion.
  Parser, help and output capability contracts remain neutral Shell contracts.
- Added the four detail levels, explicit text/JSON format and repeatable severity
  filter. The old view, JSON switch and verbosity syntax is removed. The public
  status names are completed, completed-with-warnings, invalid-input and
  cancelled where their older names differed; exit codes remain unchanged.
- Moved command rendering implementations into the temporary Core legacy
  presentation scope. Removed the two-type pipeline, renderer-set/view dispatch,
  generic compact envelope and compact envelope projector. Legacy JSON entry
  points delegate the common rendering stage; the legacy typed projection
  factories remain until their command migrations.
- Added `CliText`, table, style and authored spans; removed all eight duplicated
  text escaper classes (the original seven plus the shared command helper).
  Legacy diagnostic providers now return raw value lists instead of formatted
  strings, so user newlines cannot be mistaken for row boundaries. Only the
  final diagnostic renderer escapes and clamps those values.
- Index uses its concrete selected data model, report selector, generated data
  context, entry-count/diff renderer and `IndexWording` factory. Its old text,
  JSON, diff and diagnostic renderers and old JSON model are removed. Required
  family sentences are shared in `CliFindingWording`; tests use independent
  literal expectations rather than reading either wording factory.
- `IndexFindingDetails` retains the actual metadata child, target parent and
  the parser-established unclosed frontmatter location, plus unresolved input
  value/state. `IndexFindingCode` now resides with the result models. The primary
  cause is no longer truncated at 256 characters before presentation.
- Canonical directory-shaped source paths already pass the source parser; no
  binder grammar expansion was necessary. Index selection proves a folder only
  from observed catalogue entrypoint parents, retains its corrected source ID
  when unique, and still returns invalid input before mutation or recovery.
  Unknown sources and malformed paths retain their separate boundaries.
- Index focused evidence covers all four details, exact no-op/changed/blocked
  wording, partial unknown receipts, count invariants and authored mixed-newline
  diffs. Shared evidence covers ladders, filters, depth, count merging, workspace
  visibility, JSON omissions, stream colours, diagnostics and dependency
  direction. These additions are not claimed passing until their runner gate.

### Compilation checkpoints

- Final acceptance: all six execution modes passed against one unchanged
  source/build identity, with zero failed, pending or other tests and zero
  suite errors. The 17 Integration skips are the same Unix/Linux-only cases
  in both runtimes. Snapshot review and focused semantic review are complete.

  | Mode | Passed | Skipped | Total |
  | --- | ---: | ---: | ---: |
  | Managed Unit | 3459 | 0 | 3459 |
  | Managed Integration | 2189 | 17 | 2206 |
  | Managed EndToEnd, managed CLI | 163 | 0 | 163 |
  | Native Integration | 2189 | 17 | 2206 |
  | Native EndToEnd, native CLI | 163 | 0 | 163 |
  | Managed EndToEnd, native CLI | 163 | 0 | 163 |

  Evidence: `artifacts/g4-03-six-mode-evidence.json` and
  `artifacts/g4-03-native-remaining.log`, with per-mode CTRF reports referenced
  by the combined evidence. The final build log is
  `artifacts/g4-03-native-build-2.log`. Build HEAD is
  `e7415ceb9876635de6c0ccd53f60658bb0d9965b`; the staged source-change SHA-256 is
  `540da4b4ae7fb67c526df7b6987c69ac96a34cc8a85d27dac01de277afb4e481`.
  Native executable SHA-256 values:
  - CLI: `718630524833feb365bc1f505e1b926fb3c36dcdb52fbd7ac2f50cd71cc2de41`.
  - Integration: `acf53276418c3e74845235e15ff518b68515fb32303bd4016479615592d59d7a`.
  - EndToEnd: `6c121d482aaa489c3baf38bac4125f7e17884986ae582aed4270cac087fa1f8b`.
  These are qualified local development artifacts; the delivery wrapper's
  zero-skip packaging condition remains distinct, as explained below.
- The rebuilt native artifacts passed every build stage in
  `artifacts/g4-03-native-build-2.log`. The next test run passed all 3,459 Unit
  cases and all 2,189 runnable Integration cases, with the 17 expected
  Unix/Linux-only skips and no test or suite errors. The delivery wrapper then
  rejected `2189 !== 2206`: its report qualifier requires zero skips even
  though its runner passes `--fail-skips off`. This is the Windows wrapper
  limitation already identified by the older Task 30 conventions, which require
  direct execution with exactly 17 Integration skips. The remaining four modes
  therefore run directly against the same built artifacts. The task-local
  evidence wrapper checks build/source identity before and after, every report
  entry and suite error, zero failures/pending/other, and the exact same 17
  platform skip identities in managed and native Integration. It does not
  modify delivery scripts or mark the packaging manifest tested. Results are
  recorded separately; a green delivery-wrapper exit is not claimed.
- The deterministic cancellation repair compiled with zero warnings/errors and
  passed all 32 cases in the two Route Update application test classes.
  Focused semantic review passed. The first attempted wildcard class filter
  was rejected before running tests; the successful run used both exact class
  names. Evidence: `artifacts/g4-03-cancellation-compile.log` and
  `artifacts/g4-03-cancellation-tests-2.log`. The final formatter check still
  reports only the two known References diagnostics.
- The first valid six-mode run passed all 3,459 Unit tests and stopped at
  Integration with 2,188 passes, 17 expected skips and the documented Route
  Update cancellation-monitor failure. The permitted rerun failed identically.
  Its polling monitor could miss the first replacement despite a large filler
  file. The repaired integration test decorates the real command-local file
  application callable, cancels after its first actual applied-and-verified
  receipt, and returns that receipt unchanged. Later calls still use the real
  applier. It retains the original result/recovery checks and now asserts
  exact changed and unchanged file bytes plus the actual second cancelled
  receipt. The obsolete polling and 16 MiB filler are removed. Production
  composition supplies the same real applier method group; there is no global
  hook, fake filesystem or fabricated receipt. Rebuilt verification passed in
  the final gate above.
- A separate raw-byte audit reconfirmed that all 371 terminal-framing-only
  files equal the raw baseline plus exactly one LF. Eight other Unit text
  snapshots contain platform CRLF on disk and use Imprint's pre-existing
  default line-ending-insensitive comparison. Read-only Git filter checks
  established that their committed LF bytes preserve that comparison; they
  are not included in the 371 raw-byte assertion.
- The supported win-x64 native build passed all stages: managed solution,
  native CLI, native Integration and EndToEnd executables, and managed tests
  targeting the native CLI. The build log is
  `artifacts/g4-03-native-build.log`; managed compilation reported no warnings
  or errors. The first six-mode test attempt stopped at Unit because the
  runner environment supplied an empty Imprint update setting. No production
  or test source changed for this correction: the gate wrapper now explicitly
  selects `IMPRINT_UPDATE=verify`. Its rerun is recorded separately.
- The production symbol audit found no retired view/verbosity/renderer-set
  types and no Presentation reference from Commands or Framework. There is
  one escaping implementation, `CliText.Escape`. Two temporary legacy methods
  forward directly to it; Context's temporary diagnostic helper is an identity
  function because the shared diagnostic renderer now owns escaping. These
  adapters contain no separate escaping policy and disappear with their
  command migrations and the bridge-removal gate.
- The final ordering corrections passed 67 focused presentation cases and all
  32 Hosting colour cases. Index recapture passed all 11 cases, and review
  confirmed the closing dry-run sentence and final Next line in all affected
  captures. No snapshot-review findings remain; complete gate results are
  still recorded separately.
- The first authorized snapshot capture passed 99 Unit and 329 Integration
  cases. Review against the committed 01 baseline covered 1,179 changed files:
  371 terminal-framing-only, 78 other legacy text, 686 legacy JSON, and 44 Index
  captures. Every terminal-only file was independently checked byte-for-byte
  as the baseline plus one LF. Legacy text differences were limited to the
  new flags, shared help terminology and M3 escaping; no legacy wording
  regression was found.
- The JSON audit compared all 686 legacy envelopes with their baseline expanded
  projection, including the newly complete minimal data. All envelope keys,
  command identities, status renames, workspaces and minification checks passed.
  In 418 captures the data was identical; the remaining differences consisted
  only of finite status renames and Context/Find's view-to-detail echo. This
  establishes that restoring data omitted by the retired compact projection
  did not invent or lose command facts.
  The separate Next audit retained every reason and changed only 22 command
  strings from `--verbose` to `--detail debug`; all other command text remained
  identical, and every new Kind used the accepted finite vocabulary.
- Index review covered all 44 captures and found matching identities, receipts
  and count invariants. All nine standard diff arrays retained baseline content
  exactly after the specified omission of blank diff lines. It identified the
  two final-line ordering corrections recorded below; those captures were
  recaptured and reviewed after the corrected renderer was rebuilt. Snapshot
  review is separate from the complete managed and Native AOT gates above.
- Formatting after the migration reports two remaining baseline diagnostics in
  `ReferencesOperation.cs` at lines 459 and 460. The other three baseline
  diagnostics belonged to the nested `CliRendererSet` construction in
  `ExtensionListApplicationIntegrationTests`; replacing that retired surface
  with the typed report pipeline necessarily removed those lines. This explains
  the change from five diagnostics to two; no unrelated cleanup is claimed.
- All 28 bridge registrations and new flags compiled with zero warnings/errors
  in `artifacts/g4-03-flags-repair-build.log`.
- The first Index concrete selector/data/wording slice compiled with zero
  warnings/errors in `artifacts/g4-03-index-build.log`.
- M3 and staged output compiled with zero warnings/errors in
  `artifacts/g4-03-m3-repair-build.log`. Subsequent unit compilation inventories
  are migration feedback, not completed managed-suite acceptance.
- Unit compilation checkpoint 5 passed with zero warnings/errors. Its focused
  presentation run passed 24 of 36 cases; eight failures identified generated
  metadata for ignored required Index text properties, and four identified an
  ANSI absence assertion using culture-sensitive comparison. Both were repaired;
  generated creation also required removing the ignored properties' required
  modifier. The selector explicitly supplies these internal text facts.
- Solution compilation checkpoint 2 built production and Unit successfully and
  stopped at an unrelated integration-test syntax error. The focused compiled
  Unit binary then passed 38 of 39 cases; the remaining folder case used an
  invalid rooted empty-source fixture, subsequently corrected to NotEstablished.
  Integration project compilation then passed with zero warnings/errors.
- Full solution compilation checkpoint 3 passed with zero warnings/errors.
  The first complete Unit run passed 3221 of 3428, with 48 snapshot mismatches
  and 159 other failures. Focused integration passed 21 of 45; 22 failures
  identified missing generated property metadata in legacy nested projections,
  while two Index assertions omitted the required blocked workspace line or
  assumed help would not wrap. These were corrective runtime inventories,
  not final acceptance gates.




### Shared wording propagation

- Added the accepted missing shared finding factories to
  `Presentation/Shared/Wording/CliFindingWording.cs`, preserving the 00
  catalogue literals, raw arguments, renderer-owned escaping, and invariant
  counted interruption formatting.
- Added independent Unit literal coverage for the new family messages, both
  `SelectionRequired` variants, raw argument preservation, and counted
  `Interrupted` culture invariance.
- Focused evidence is recorded in
  `artifacts/g4-05-iu-wording-build.log` and
  `artifacts/g4-05-iu-wording-tests.log`; the byte-correct wording patch was
  applied with SHA-256
  `caecb2354d89d18e5ad3e2dad86350569f3e12f31db7b828ebaba9ccdb20c5cd`.

- Shared count numeric widening is integrated from the reviewed two-file
  decimal patch. `CliCount.Value` is now `decimal?`, preserving exact numeric
  `counts.startupShare` percentage points such as `80.87`, integral values
  including `long.MaxValue`, null/unavailable limitation behavior, trimmer
  maximum/null propagation, invariant text formatting and numeric JSON. The
  focused child evidence passed 5/5 numeric cases and 34/34 existing shared
  rendering cases. The byte-safe patch, blob manifest and corrected draft are
  recorded under `artifacts/g4-05-cli-count-decimal`; patch SHA-256 is
  `4dac6c916201b024daefbd635aa737c549e0ad56e879fc3f15f1cc8048d65fb6`.

- wave 0 repair: the text headline was written plain at every status -> it is
  written through `CliTextStyle.Status`, which leaves a completed headline
  plain, writes completed-with-warnings and incomplete headlines in the
  warning colour, and invalid-input, blocked, failed and cancelled headlines
  in the error colour. Plain text is unchanged byte for byte, so no snapshot
  moves and no frozen sentence changes; only a colour-capable terminal sees
  the accent. Accepted by the maintainer on 2026-09-15 after
  `CommandColorIntegrationTests` showed that a minimal blocked result whose
  only finding is folded into the headline by `HeadlineFindingCode` left no
  coloured span anywhere in its output.

## Divergences observed

- Task10's exact `counts.startupShare` contract requires decimal percentage
  points. The shared model therefore widens `CliCount.Value` from `long?` to
  `decimal?`; no rescaling, truncation, string encoding or serializer
  workaround is introduced. Main integration is source-complete but remains
  pending the combined qualification gate.

- A legacy text body already includes its workspace and Next rows. The bridge
  keeps these complete facts in the report and JSON and explicitly disables
  their second text emission (`ShowWorkspace` and `ShowNext`). This preserves
  JSON Next without duplicating existing legacy text.
- The specified legacy summary still takes the old renderer's first line.
  This is rendered text, rather than native selector semantics, and is an
  explicit temporary bridge limitation until 40. The bridge suppresses shared
  headline text and emits the complete legacy text span, preserving structural
  tabs without double escaping. Its generated terminal newline is a separate
  framing span. Minimal selects old compact text; all higher levels select old
  expanded text, while diagnostic providers retain the actual Debug request.
- Pre-snapshot review corrected an overly broad status-name migration: 03's
  bridge preserves legacy human body wording, while 02's renamed status tokens
  belong to JSON, diagnostics and shared help. Against the 01 baseline, the
  legacy-only text mapping now retains `complete`, `invalid`, `interrupted` and
  the unchanged other tokens. Descriptive Attention text remains `requires
  attention`; Find's compact token remains `attention`. Shared headers, uppercase
  finding labels, Library Inspect's headline and References' inline section
  statuses use that legacy mapping. Native Index retains its final catalogue.
  No shared status definition, exit, JSON status or diagnostic token is reverted.
- At the 01 baseline, `CommandOutputRenderers.Match` captured the raw legacy
  renderer text before the output stage appended its terminal newline. The new
  harness captures `CliRenderingStage.PrimaryContent`, including explicit
  framing spans. A newly visible terminal framing newline in these snapshots
  therefore records the changed capture boundary, where the old capture omitted
  it; it is not evidence by itself of a process-output change. Published process
  tests verify actual parity. Authored spans are not trimmed to hide this delta.
- A uniquely identified finding already answered by Index's folder headline is
  omitted only from minimal text rows through `HeadlineFindingCode` and the
  common text finding selection. The complete selected report, JSON finding and
  pre-filter counts remain intact; full text lists the finding identity. Exact
  independent minimal-folder and full/JSON tests cover this extension.
- Unknown count reasons become limitations once during common selection, so
  both formats retain the same limitation. Diagnostic bounds reserve platform
  newline expansion and the final newline before output; authored data remains
  exempt from escaping and diagnostic limits.
- Index distinguishes a region whose plan was not established from an Update
  whose attempted write has an unknown outcome. Only Update creates a rewrite
  effect and a minimal receipt row. Unavailable planning remains in full region
  data and its blocking finding; an actual unknown write still lists its receipt
  and counts as neither updated nor current. The blocked-leaf catalogue example
  is asserted as a complete literal, including nineteen current sections.
- Context and Find's legacy payload detail echoes now cover all four accepted
  detail names. Their typed result retains Full/Debug, so extending only the old
  human-renderer mapping was insufficient for generated JSON projection. Shared
  help lists format, detail and repeatable detail-filter separately.
- Runtime verification identified the BCL requirement for a resolver even when
  serializing an explicitly supplied value-type metadata object. The legacy
  metadata options now contain an empty `JsonTypeInfoResolver.Combine()`; the
  write-only converter still uses only the explicit generated projection type
  info, with no resolver or reflection fallback for the result graph.
- Twenty-two legacy contexts previously generated only optimized serialization
  code. The bridge serializes their nested result as the typed projection root,
  which also requires generated property metadata. Those contexts now generate
  both metadata and serialization (`Default`); five existing metadata-mode
  contexts remain unchanged. All registrations still use concrete generated
  projection type information.
- The first published run then exposed four ignored nullable Doctor helper
  properties marked required. They now retain `JsonIgnore`, null defaults and
  existing projector assignments without that required modifier. This avoids
  invalid required-member metadata without adding those helpers to JSON or
  changing the actual Library data projection. The failed startup run cannot
  establish process compatibility; a rebuilt published gate is required.
- Platform newline conversion now normalizes only CR, LF and CRLF framing.
  BCL `ReplaceLineEndings` also rewrote printable U+2028/U+2029 inside paths;
  those scalars remain unchanged, with new focused and existing populated
  Install/Update output evidence. Blocked Index exact-output evidence includes
  the workspace line required by the shared convention, including cwd selection.
- Retained child metadata findings are carried by
  `IndexProjectionAssembly.MetadataFindings`, separately from acquisition
  readiness. They join findings after the unchanged generated-navigation
  projector runs; this preserves readiness's unavailable-target invariant while
  replacing the generic parent metadata finding with actual child facts. An
  unclosed-child application test checks the exact text, JSON path/location and
  unchanged workspace/recovery state.
- The actual help renderer and shared result wording moved to Presentation's
  shared help/wording scopes. The old human text/style helpers moved to the
  temporary legacy scope. Neutral invocation, color and help metadata remain
  in Shell, as does the neutral workspace-selection wire vocabulary.
- Native Index's remaining help helper moved out of the legacy scope into
  `Presentation/Index/Shared/Help`; its unchanged command-specific bodies now
  come from parameterless `IndexWording` factories. There are no remaining
  native Index references to the temporary legacy presentation scope.

- The shared effect/count renderer would duplicate Index's entry-count rows and
  its headline totals. The accepted implementation keeps text effect/count
  selections beside the complete report in `CliSelectedReport`, as it already
  does for workspace visibility. Index's data renderer owns its path and entry
  count rows; JSON keeps every effect and count. No command-specific fields or
  rendered lines are added to the shared effect model.
- For Index's partial outcomes, `filesChecked` counts the established updated
  and current outcomes. Unknown outcomes count as neither and remain explicit
  rows, preserving the catalogue's count invariant without classifying an
  unknown file as current. Operation facts remain unchanged.
- The accepted typed legacy bridge retains the concrete result for legacy text
  rendering. Its write-only `JsonConverter<TResult>` invokes an explicit
  projection factory and matching generated `JsonTypeInfo<TProjection>`;
  `Read` is unsupported. It never serializes the raw result graph or falls back
  to reflection. Migrated commands use their concrete data type directly.
- The rendering move exposed serializers consumed by command models and domain
  wire vocabulary used outside rendering. Those neutral serializers remain
  with their command owners under `Shared/Serialization`; their consumers do
  not import the temporary presentation scope.
- Foundation checkpoint: solution build passed with zero warnings and errors;
  the `cli-presentation` Unit selection passed 13 tests with no failures or
  skips. The report pipeline was not yet wired at that checkpoint, so this is
  foundation evidence rather than whole-task acceptance.

- Preparation found that command binding helpers such as `IndexBinding.Close`
  currently accept renderer sets and close the complete pipeline. Moving those
  rendering types without changing the binding boundary would introduce the
  prohibited `Commands` -> `Presentation` dependency. Keep request binding facts
  in neutral Shell contracts and close the operation, selector and renderers in
  the existing executable composers. Preserve typed request, result and data
  parameters through the pipeline and the existing non-generic registry boundary.
- Moving every type presently under `Shell/Presentation` would also move
  invocation options, output capabilities and help metadata that request binders
  consume. Keep these neutral contracts in Shell. The shared presentation rule
  is read together with this task's explicit `ICliCommandResult` constraint and
  04's `CliTerminal` contract: presentation may consume those neutral Shell
  contracts, while the ban on shared presentation importing Commands or
  Framework remains complete. The dependency test must name the actual allowed
  Shell contract scopes. Documentation propagation must record their final paths.
- Existing generated JSON contexts serialize command-specific projection models,
  not the raw operation result proposed by the bridge sketch. The temporary
  bridge must use each existing typed projection and its matching generated
  `JsonTypeInfo`; do not add reflection serialization or infer facts from JSON
  output. Preserve the concrete legacy data shape until its command migrates.
- Legacy renderers currently depend on command helpers and Framework types.
  Their temporary location must be distinct from the final command presentation
  folders so their removal can be checked in 40. Moving them into the executable
  would invalidate unit tests that intentionally reference only Core. Keep the
  migration bridge and legacy implementations together in a temporary
  `Presentation/Legacy/` scope within Core, with that temporary exception limited
  to the bridge. The final presentation folders retain the strict dependency
  rule, and no Commands or Framework file may import the temporary scope.
- The maintainer's subsequent wording-factory instruction is recorded in
  [02](02-naming.md#changes-ledger). Preserve the catalogue strings in named
  factories; keep shared families shared and escape only when writing text.
- The trimmer table's workspace omission must apply to text visibility, not
  erase the workspace from the shared report: 00 requires JSON to carry the
  workspace at every level. Preserve its facts and carry the selected text
  visibility separately. Verify both default and explicit workspace selection
  in text and JSON before copying the pattern to commands.
- Finding totals must be established before filtering and merged by count name,
  never counted again from the remaining rows. Doctor's optimization that avoids
  materializing hidden rows must use the effective severity filter as well as
  the detail level; `minimal --detail-filter all` still needs those rows.
- The preparation findings above were accepted before implementation. Passing
  compilation checkpoints are recorded separately from final suite acceptance;
  01's committed snapshots supplied the required baseline.
- Shared capture verification now has one required rich renderer, one shared
  four-detail vocabulary and a command-owned process capture model. `Match`
  retains its compact/expanded output path and custom normalizers; `MatchDetails`
  captures minimal, standard, full and debug text/JSON with caller identity.
  Rich full/debug JSON is compared from raw primary content before custom
  normalization and differs only at the root `detail` coordinate. New JSON
  snapshot bodies are then pretty-printed with BCL `JsonDocument`/
  `Utf8JsonWriter`, preserving property and array order and all values.
- A process shell diagnostic remains a plain stderr stream even when the
  request includes `--format json`; the detail capture records that primary
  stream as text and deliberately excludes it from JSON full/debug parsing.
  When a normal process command writes debug diagnostics to stderr alongside a
  stdout primary, the diagnostic snapshot remains separate. A stderr-primary
  debug stream retains the combined primary content because the output stage
  supplies no delimiter and the capture does not parse or split it.
- The focused integration build passed with zero warnings and errors. The new
  all-detail Index composition capture and Status, Index, Cleanup and Repair
  legacy/custom-normalizer snapshot classes passed 46/46; no existing
  snapshots were updated. This amendment changes only shared capture support,
  its new Index evidence snapshots and this ledger; catalogue steps remain
  unchanged.
- The new-detail capture follow-up uses Imprint's typed `RootDirectory` and
  explicit `SnapshotTestIdentity` under
  `src/cli/tests/integration/snapshots/<suite>/<test>/<capture>.txt`. The
  suite is the caller source file name and the test is the caller method, so
  all detail captures share one method folder without duplicating the capture
  name as a directory. Existing `MatchSnapshot` calls and before baselines
  retain their prior paths. The 29 authored Index detail files moved from the
  source-adjacent capture tree with unchanged SHA-256 content hashes. The
  moved tree's longest path is 193 characters in this worktree; across the six
  retained worktree prefixes, the longest Extension Install candidate is 229
  characters, below the Windows 260-character threshold. No catalogue or
  authored output content changed.

## Rollback

Revert the branch. The before snapshots and the bridge make partial states
reviewable, but the physical move is one commit and reverts as one.
