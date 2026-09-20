---
open-forge:
  description: The Presentation layer - the complete report, report-selection stage, and renderers that turn selected facts into text or JSON
  responsibility: Define report formation, detail selection, renderer ownership, help, diagnostics, prompts, and the boundary between deciding what to show and deciding how to write it
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, Architecture, Presentation, Selection, Rendering]
---

# Presentation Layer

Presentation answers **"of everything the operation found, what does this
reader need, and how is it written?"**

The operation supplies a complete command result. Presentation first selects
the facts for the requested detail, then writes that selection as text or JSON.
Selection and rendering are separate responsibilities: a renderer never
decides which facts to include.

The [land Architecture](../architecture.md) records how this layer sits against
the others.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Report Model And Report Selection

One `CliReport<TData>` carries every command result from Operations to the
rendering pipeline. It contains the command identity, renamed semantic status,
headline and kind, optional workspace echo, all findings, effects, counts,
limitations, command-owned data, recovery, one next action, and bounded debug
diagnostics. A finding has one `CliSeverity` (`Error`, `Warning`, or `Info`),
one code, one title, one sentence, and one path-or-identifier subject. Effects
retain their path, kind, action, outcome, reason, owner, and optional hashes.
Counts use plain numeric values or an unavailable value with its limitation.

The report is complete; it carries no presentation level and no pre-rendered
line. `CliNextAction` carries a `Kind` (`Command` or `Sentence`) so the shared
renderer can write one appropriate `Next:` line.

The named selection stage is **report selection**. Its input is the complete
operation result and a `CliSelection` containing `CliDetail` and the effective
severity filter:

```text
CliOperationResult<TResult>
  -> CliReport<TData>       (CliReportSelector, then CliReportTrimmer)
  -> CliRenderedOutput      (text or JSON, plus diagnostics)
```

`CliReportSelector<TResult, TData>` projects the concrete command result into
the command-owned report and data model. `CliReportTrimmer` applies the shared
rules once after the selector. The selector and trimmer format nothing, read no
file, invoke no operation, and never change an established fact. Given the same
result, detail and filter they select the same facts.

The four detail levels are `minimal` (the default), `standard`, `full`, and
`debug`. `--detail-filter <error|warning|info|all>` is repeatable and replaces
the normal severity listing ladder at any level; `all` selects every severity.
The level still controls per-finding depth. `minimal` lists errors and lists
warnings for ordinary command shapes; the `Diagnosis` shape counts Doctor's
warnings at `minimal` and lists them at `standard`. `full` adds info findings,
evidence, possible targets, provenance, hashes and finding codes. `debug` keeps
the full primary result and adds bounded diagnostics to stderr.

The trimmer also keeps every effect, clears effect hashes below `full`, clears
full-only finding detail below `full`, keeps only the first finding action below
`standard`, and keeps the workspace echo below `standard` only when the
workspace was explicit or the status is blocked, failed or cancelled. Counts
are established before filtering and report everything not listed. The command
shape is one of `Summary`, `Diagnosis`, `Data`, or `ChangeReport`.

## Physical Layout

The Rendering project owns the top-level Presentation tree. Each
command has a command-owned presentation folder:

```text
Presentation/
  Shared/
    Models/       CliReport and shared report values
    Selection/    shared report selection and trimming
    Rendering/    shared text, JSON and diagnostic renderers
    Text/         escaping, tables, styles and authored spans
    Prompts/      shared prompt primitives
    Wording/      shared finding families
    Help/         shared result and option help
  <Command>/
    Models/<Command>Data
    Shared/
      Selection/<Command>ReportSelector
      Rendering/<Command>DataTextRenderer
                  <Command>DataJsonContext
      Wording/
      Help/
```

The command folder mirrors the command path, including nested Route,
Extension and Library verbs. The command selector owns the mapping from the
concrete result to `<Command>Data`; the command data text renderer owns only
the command's rows and authored spans. Each command's data is serialized by
its source-generated `<Command>DataJsonContext`. Rendering references Operations
result models and neutral Shell contracts, with no Framework reference. Command
results supply workspace, permission, enum and complete location display facts;
selectors do not infer those facts through Framework objects. Update's previous
content observation is captured during operation result formation.

## Dependency Direction

The dependency rules are mechanical and enforced by `LayerBoundaryTests`:

- `Presentation/**` may not import any `Framework.*` namespace.
- `Presentation/Shared/**` may use only neutral Shell contracts needed for
  statuses, exits, terminal capabilities and output coordinates; it does not
  import command or Framework namespaces.
- Only `Presentation/<Owner>/**` may import `Commands.<Owner>.Models.*`, and
  only for its matching owner. It may use shared Presentation types, but it may
  not import another command's private `Shared` namespace or behavior-heavy
  command code.
- No file under `Commands/` or `Framework/` references `Presentation/`.

If a command result exposes a Framework type, the operation must first project
that fact into a command-owned value under `Commands/<Owner>/Models/`. The
Presentation selector consumes that value; it never imports a Framework model
to reach through the boundary. This keeps the report shape command-owned and
allows the future rendering project to depend on data without reversing the
direction.

The legacy bridge is gone. `Presentation/Legacy/` retains exactly these three
composer-bound help-section files:

- `ExtensionHelpSections.cs`
- `LibraryHelpSections.cs`
- `RouteHelpSections.cs`

No command binds to `Presentation/Legacy/`, and no file under `Commands/` or
`Framework/` references `Presentation/`. The surviving help-section helpers
are not command renderers or selectors.

## Rendering

The shared text renderer writes the headline, optional workspace line, listed
findings in severity and subject order, effects, command data, kept and
limitation sentences, counts, and the next action. The first line is always a
complete outcome sentence; there is no status line. Finding codes are shown in
text only at `full` and `debug`, while JSON carries them at every level. Paths
and identifiers are never truncated.

The shared JSON renderer writes one minified schema-3 envelope for every
semantic status. It carries `command`, `status`, `detail`, `filter`,
`workspace`, `summary`, root `findings`, root `effects`, `counts`,
`limitations`, command-owned `data`, `recovery`, and `next`. Members omitted by
the selected detail are absent rather than empty placeholders. The `data`
member uses the command's source-generated JSON context and the relaxed JSON
encoder. JSON is never hand-written and authored content is serialized only by
its owning context.

`CliText.Escape` is the sole text escaper. It leaves printable characters,
including non-ASCII, unchanged; makes line breaks and tabs visible; and emits
Unicode escapes only for other control characters and lone surrogates. Tables
measure visible widths after escaping and never truncate. Authored content
spans, including context bodies, template bodies and Index diff lines, bypass
generated-text escaping and keep their original bytes. Framing is built with
LF and converted to the platform newline once at output.

## Help, Diagnostics, And Interaction

Help is composed from the exact `System.CommandLine` symbol graph. Shared help
owns the format, detail, filter, result, stream and exit explanations;
command-owned help owns only command questions and rows. It is not a second
command catalogue.

`CliDiagnosticRenderer` writes bounded, escaped diagnostics only to stderr at
`debug`. Diagnostics do not alter the report, primary output, status or exit.
Text for completed, completed-with-warnings and incomplete results goes to
stdout. Text for invalid-input, blocked, failed and cancelled results goes to
stderr. JSON is one document on stdout for every status. Prompts and parser
failures before binding are text on stderr; a parser failure has no report
envelope.

`Presentation/Shared/Prompts/` contains the `CliPrompts` primitives. In a
terminal, single select uses arrow keys, Enter, digits and Escape. Checkbox
multi-select uses Space, dependency marks and a legend; required rows cannot
be removed while a chosen package needs them. When the terminal cannot read
keys, the same questions use numbered line-based input. Plan review renders
the already-established minimal report before every confirmation, and the
report is not printed twice. When the terminal cannot prompt, no prompt is
issued and the command reports the required flag or permission finding.
`--format json` and `--automatic` never prompt.

The host supplies terminal capabilities and writers. The libraries do not inspect
ambient Console state, and JSON and authored content never receive colour.

## Enforced Guarantees

`CliReportInvariantsTests` checks these cross-command guarantees over the full
native snapshot corpus and synthetic reports:

- text grows monotonically from `minimal` to `standard` to `full`;
- `debug` stdout equals `full` stdout, with diagnostics on stderr;
- errors precede warnings, which precede infos, in text and JSON;
- text and JSON at the same level list the same finding and effect identities in
  the same order;
- human primary text has no legacy status/selection lines, internal omission
  labels, or retired planning headings;
- generated text contains no JSON-style escaping outside the escaper's visible
  sequences;
- text snapshots are valid UTF-8 with ASCII-only framing, with non-ASCII only
  in authored spans and descriptions;
- every listed finding has a path or identifier;
- there is at most one `Next:` line, and the report places it last; the current
  Extension Install continuation is the recorded exception;
- a detail level does not emit empty JSON arrays for members it omits;
- count values are numbers or null, and null values have a recorded limitation
  except for the accepted finite nullable-count allow-list;
- every JSON finding code is checked against the command catalogue, with a
  missing row recorded as a review gap;
- no file under `Commands/` or `Framework/` references `Presentation/`.

The current evidence also records these maintainer questions without deciding
them: whether the `--detail-filter` name should be shortened; the shared
renderer's repeated action and reason lines; the Extension Install continuation
that follows its `Next:` action; the finite nullable-count allow-list versus
per-count limitations; whether the human-primary vocabulary check should
include machine diagnostics; the `extension-list.installed-source-missing`
code's missing catalogue row; and the explicit-workspace echo in minimal
examples. The implementation keeps the observed behavior until those
questions are settled.
