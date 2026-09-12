---
open-forge:
  description: Analyze readable command output, category grouping, severity filtering, implementation boundaries, and sequential evidence
  tags: [Memory, Working, Contextual, CLI, Task, Presentation, Design]
---

# CLI Presentation Design Analysis

## Status And Scope

Analysis completed on 2026-09-12 against local `develop` `36d043a9` and the
installed native CLI from source `29de40a0`. This is a proposal under the
[sequential plan](cli-dogfood-follow-up-plan.md), not an implemented or accepted
replacement for the current command contracts. The [surface audit](cli-presentation-audit.md)
owns the inventory of all 28 commands and earlier executable comparisons.

The user requests readable, friendly, concise output for people and AI, grouping
without diagnostic-kind changes, and analysis of a severity filter. Remaining
manual-parsing simplifications are backlog. Useful functionality, typed results,
status, exit codes, effects and recovery guarantees remain intact. No production
code, public contract, output snapshot or installed binary changes in this set.

## Clarification And Review Gallery — 2026-09-12

The user's warning/error/info example was an idea, not a required classification
or spelling. Existing statuses remain. The [per-command proposal](cli-command-output-proposals.md)
and [example gallery](cli-command-output-examples.md) now provide recommendations
for all 28 commands. User approval comes before changing command Interfaces or
implementing the proposed layouts. The earlier Doctor-only severity-flag priority
is withdrawn; no new flag blocks clearer presentation.

## Main Finding

Most of the work belongs in rendering over existing typed results. Commands
already finish their operation before selecting human or JSON rendering. A human
renderer can group facts, order them, choose clear labels and print shared details
once without changing diagnosis or planning.

The intended path is:

```text
Completed typed result + presentation options
    -> human selection and grouping -> compact or expanded text
    -> existing JSON serializer     -> complete JSON

Original result -> unchanged status, exit code and stream selection
```

This is text rendering; it does not require a graphical rasterizer or a terminal
UI framework. Start with command-local formatting and small projections. Share
only neutral formatting with proven consumers. Do not introduce one universal
view-model tree or move command knowledge into Shell.

Two additions exceed a renderer-only edit: a new flag needs parser/binding and
public-interface work; reducing repeated JSON data needs a separate machine-output
design. Neither requires changing diagnostic kinds to improve human output.

## Source Evidence

Paths below are relative to `src/cli/core/OpenForge.Cli.Core`.

| Existing owner | What it establishes |
| --- | --- |
| `Shell/Pipeline/Models/Presentation/CliPresentationRequest.cs`, `Shell/Pipeline/CliRendererSet.cs` | Renderers receive a completed typed result and presentation settings; human and JSON delegates are separate. |
| `Shell/Pipeline/CliPipelineStages.cs` | Rendering preserves the original semantic status; stream selection belongs to the pipeline. Operations need not run again. |
| `Shell/Presentation/Models/CliPresentation.cs` | Current presentation settings are format, view and verbosity. There is no severity selection. |
| `Shell/Composition/CliCommandBinding.cs` and `Models/CliCommandBindingComponents.cs` | Binding transports an operation request and the existing presentation settings. A command-local presentation option needs an explicit transport path. |
| `Commands/Doctor/Models/Result/DoctorDomainModels.cs`, `DoctorFindingModels.cs`, `DoctorSubjectModels.cs` | Six domains, explicit information/warning/error severity, typed subjects, optional locations, candidates and actions already exist. |
| `Commands/Doctor/Shared/Rendering/DoctorHumanRenderer.cs`, `DoctorFindingHumanRenderer.cs`, `DoctorActionHumanRenderer.cs` | Human output repeats global/domain/finding actions and candidate detail. Wire vocabulary appears as human labels. The shared text helper silently cuts strings after 512 characters. |
| Doctor's `DoctorFindingAggregation.Order` | Findings are ordered by kind before subject. Reorder only the human projection; keep result and JSON ordering. |
| `Commands/Status/Models/Result/StatusResult.cs`, Library Attach and Extension Install finding models | These findings carry semantic statuses such as `attention` or `blocked`, rather than Doctor's severity enum. A universal severity filter has no established mapping. |

The [global flags Interface](../../../crystallized/documents/cli/contracts/shared/global-flags/interface.md)
currently makes expanded the default, compact the concise human view, and JSON a
complete result independent of view. Verbose is a separate bounded diagnostic
stream, not another name for expanded. The
[Doctor Interface](../../../crystallized/documents/cli/contracts/doctor/interface.md)
already requires six domain groups and defines facts retained by each view.
Changes to that human hierarchy still need Interface/Behavior alignment even
when the typed result is untouched. The
[Writing Standard](../../../crystallized/documents/maintenance/writing.md)
governs revised text; clearer wording must preserve certainty and meaning.

## Grouping Recommendation

Use category first for Doctor, then affected file or other typed subject, then
exact occurrence. The current implementation already has categories; the change
is readable category names and useful organization inside them.

| Organization | Benefit | Limitation |
| --- | --- | --- |
| Severity first | Errors are immediately visible. | One category or file is scattered across several sections. |
| Finding kind first | Easy to inspect every occurrence of one diagnostic code. | Repeats the same file and candidate list; resembles an internal catalogue. |
| Category, subject, occurrence | Keeps related work together using existing facts. | Needs prominent severity totals and labels so errors remain easy to find. |

Recommended category titles are Workspace, Recovery, Routes and navigation,
Links, Framework, and Extensions. Keep Libraries within Workspace using the
existing typed subcatalogue. Retain deterministic category order. Inside a
category, put subjects with errors first, then warnings, then information; use
stable path/identifier and occurrence ordering within those groups. This affects
human ordering only. There is no demonstrated need for a `--group-by` option.

Group by full typed identity, domain and exact occurrence, never by rendered
message, truncated path, or line number alone. Two links on one line remain two
occurrences. Different candidate sets remain distinct. Missing paths or locations
are absent facts, not permission to invent them. When existing typed facts cannot
establish that records share an occurrence, keep them separate; do not guess the
relationship from their messages.

A group can present shared candidates and actions once while retaining every
member finding's severity, identity and distinct explanation. Deduplicate only
identical facts, including their evidence and qualification; matching text alone
is insufficient. A heading may show the highest member severity, but it must not
reclassify its members. Report counts as findings, not unique problems: grouping
three findings does not turn the result count into one. Preserve candidate order,
reasons, uncertainty and the need for a user choice. Never choose a target merely
to simplify the output.

Keep overall severity counts near the outcome. Show each category's coverage and
limitations concisely, including categories with no findings. An unavailable check
must not become a healthy or empty category. Show a shared required action once
at the nearest useful common level; do not repeat it at all three hierarchy levels.

## Human Output Shape

Lead with the result and facts needed to act. Keep uppercase severity labels
legible without colour. Print real source coordinates as `path:line:column` when
available; byte offsets are edit coordinates and belong in expanded evidence,
not a prominent unexplained `location` section. Keep paths and copyable commands
complete. Replace the existing silent string cutoff with a deliberate policy
that preserves identity and terminal-safe escaping.

Use a short title, concrete explanation and supported next action. Prefer
“Possible targets” to “bounded reference candidates”, and “Why this target is
listed” to “basis”. Name what a source proves instead of printing an unexplained
“provenance” label. Keep exact diagnostic codes available as secondary detail.
Where the result only has a free-text cause, retain it faithfully; do not parse
English to recover semantics or infer a repair command the result does not support.
A human-owned label can differ from the wire identifier. Editing producer
messages shared with JSON would exceed a human-only change.

Illustrative finding excerpt, not captured output or a complete Doctor result:

```text
Links
  WARNING  Broken link
  .agents/memory/working/handoffs/example.md:44:16

  The linked file was not found:
    ../../../directives/open-forge/csharp/csharp-design.md
  Check the destination, then update or remove the link.
```

“Was not found” describes the observation. “No longer exists” would assert history
that a missing target alone does not prove. Missing access or incomplete inspection
must have different wording. The surrounding report retains coverage, counts,
resolution and any actual candidate/action facts.

Compact retains outcome, identity, completeness, useful findings and required
actions, with short rows and minimal repeated framing. Expanded adds explanations
and complete related evidence once. Keep expanded as the existing default while
improving both. Do not invent a third view or use verbose to repair an unusable
normal result. Readable compact text serves agent scanning; JSON remains the
complete structured interface.

## Applying The Design Across Commands

Common principles do not require identical layouts for unlike results.

| Command family | Natural primary content | Work and retained boundaries |
| --- | --- | --- |
| Doctor, Status | Category and affected subject | Group diagnosis and state; explain incomplete checks. Status does not inherit invented severities. |
| Library Attach/Sync/Detach | Mapping and affected path | Expose existing plan/application facts currently missing from human output. Distinguish proposed, completed, absent and retained effects. |
| Extension Install/Update/Remove | Package, then affected path | Make compact meaningful; expanded shows dependency and effect detail once. Preserve blockers, permissions, edits and recovery. |
| Library/Extension List and Inspect | Library or package | Scannable rows and clear installed/available/source distinctions. Preserve unknown and uninspected states. |
| Find, Route List, References, Route Inspect | Match, route or source occurrence | Put requested results first and retain order/coverage. Keep Find compact TSV and exact reference identity. |
| Context | Selected authored content | Simplify generated framing only. Preserve selected content bytes and projection order. |
| Install, Update, Index, Repair, Cleanup, Route mutations, Extension Create | Outcome and planned/applied effects | Retain preview diffs, selection decisions, protected/deleted paths and recovery. A preview must never read as a completed write. |
| All command help | Purpose, usage, options, practical examples | Explain public concepts; keep parser-owned syntax, accepted options and required safety guidance. |

These rows cover the 28 owners in the surface audit. They are a design inventory,
not a claim that every diagnostic message or execution outcome has been reviewed.
The implementation pass must cover all rendered branches, not only successful
examples. Use the existing result wherever it contains the required fact. If a
needed fact is absent, record that gap before expanding the operation or schema.

## Filtering After The User Clarification

Keep the existing status and finding models. The user's example does not require
a global warning/error/info model or a Doctor severity flag. The
[per-command proposal](cli-command-output-proposals.md) compares overall status,
collection status, category and severity filtering and recommends improving
existing views first. A scoped Doctor category filter is a later candidate if
large grouped reports still need focused selection; no flag spelling is selected.

Any future display filter must preserve full diagnosis, status/exit and coverage,
show full versus selected counts and retain required recovery. Its JSON behavior
needs an explicit contract. A new presentation option also needs typed binding
transport: current presentation settings have only format/view/verbosity. Keep
command types out of Shell and display options out of domain diagnosis. That
internal design can wait until a filter is actually selected.

## What Changes And What Does Not

| Change | Required work | Boundary |
| --- | --- | --- |
| Labels, layout, human grouping, repeated-detail removal | Command renderers, output snapshots, affected Interface/Behavior text | Same typed findings, JSON, counts, status, exits and effects. |
| Restore missing Library detail and distinct Extension views | Render existing plan/application data; verify public contract branches | No new planning or mutation behavior. |
| A future scoped display filter, if selected | Native parser registration, validation, typed presentation transport, help/contracts and tests | Separate public interaction proposal; no filter is currently selected. |
| Reduce repeated JSON payload | Separate schema/serialization analysis and acceptance | Human grouping alone cannot shrink the prior 168 MB JSON result or remove its underlying allocations. |
| Change diagnosis, retire kinds, change default visibility, add colour | Separate explicit decision | Outside this presentation implementation proposal. |

Measure lines and UTF-8 bytes for representative fixed results in both views.
Do not claim a token reduction percentage without selecting a tokenizer and
measuring it. Prefer removing repetition to truncating content. Expanded evidence
may remain large when the evidence is distinct.

## Frozen Sequence And Evidence

1. Obtain user approval of the per-command proposals, update affected Interfaces,
   then freeze representative typed results and proposed output for Doctor/Status:
   success, mixed findings, incomplete coverage, unavailable source and failure.
   Freeze the affected Interface/Behavior changes and snapshots before rendering
   edits. Implement the first bounded presentation set and qualify it.
2. Do not add a filter by default. Review the first grouped diagnosis output on
   a large workspace, then consider a scoped display filter if it remains useful.
   Close public semantics and typed presentation transport before any such slice.
3. Library mutation presentation, then Extension mutation presentation, each with
   preview/applied/blocked/retained/recovery cases and its own integration boundary.
4. Remaining command families and help, one bounded set at a time. Check every
   rendered branch against the command Interface/Behavior and maintenance writing.
5. Consolidate the reusable CLI UX/development Guidance and CLI/C# Directives last,
   using the verified decisions rather than speculative rules.

Use small authored fixtures and reviewed snapshots when exact human output is
under test. Keep independent assertions for JSON, status, exit, coverage, required
actions and filesystem effects. Cover same-file distinct occurrences, differing
candidate sets, mixed severities, missing locations, long identities and terminal
control characters. Filter tests must cover union, duplicates, invalid input,
no matches, incomplete coverage and output combinations. Do not replace semantic
assertions with snapshot acceptance.

Run the cheapest decisive renderer evidence and affected regressions for each
slice. The first representative implementation and material shared binding or
public-composition changes trigger the applicable managed/native qualification.
Freeze each source candidate and record exact commands/results before local
squash integration. Required tests and fixtures live in tracked source, not
artifacts or disposable probe programs.

## Analysis Checkpoint

This set reviews source and current contracts, proposes grouping/filter semantics,
and records the implementation boundaries. Installed `open-forge doctor --help`
was checked again: expanded remains default; no severity option exists. Prior
qualification counts belong to the earlier source candidate, not this analysis.
No new executable experiment or script is retained. Documentation validation is
the installed References command, generated navigation and `git diff --check`;
no production test rerun is necessary for this prose-only set.

Validation receipt: `open-forge references` on this file with `--direction out
--json` returned complete, exit 0, five resolved outgoing references and no
findings or stderr. `open-forge index` on `tasks/_tasks.md` returned complete,
exit 0 and refreshed its 43 entries. It also normalized two blank lines in the
nested delivery index; that unrelated tool-owned diff was reverted. The generated
navigation entry for this analysis is retained. Required plans, evidence summaries
and reproduction commands remain tracked. Eleven analysis references and new
navigation/checkpoint links were checked through the installed CLI; all resolve.
The reviewed documentation set is ready for local squash integration. There are
no CLI source or test changes to qualify in this set.
