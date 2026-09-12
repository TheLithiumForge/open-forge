---
open-forge:
  description: Review all CLI presentation surfaces and sequence verified fixes for missing detail, duplication, and unclear writing
  tags: [Memory, Working, Contextual, CLI, Task, Presentation, Writing, Dogfood]
---

# CLI Presentation Review

## Current Presentation Direction — 2026-09-12

The user clarified that diagnostic-kind changes are not the intended work.
Keep all diagnostic kinds, typed findings, JSON fields/counts, operation behavior
and exit statuses. Treat deduplication as presentation grouping; discuss its
exact shape later. Withdraw the earlier seven-kind retirement proposal from the
active sequence. No schema or default-severity change blocks presentation analysis.
The immediate priority is legible, friendly, readable output for people and AI,
with less repetition and lower output cost wherever useful information survives.

The initial audit is complete; broad presentation implementation has not started.
All 28 command surfaces and help owners were inspected. Six mutation commands
were compared in compact, expanded and JSON modes. That is a surface audit and
representative execution, not exhaustive testing of every status/view combination.
D1/D2 fixed incorrect diagnosis, not the general output layout or wording.

## Scope And Evidence

This is the first complete surface review requested by the user, following the
[sequential plan](cli-dogfood-follow-up-plan.md). It covers all 28 leaf commands,
their help, human-view selection and rendering entry paths, shared output/stream
rules, and JSON boundaries. It does not claim that every command/status/view
cross-product has been executed or that the proposed fixes are implemented.

The [Writing Standard](../../../crystallized/documents/maintenance/writing.md)
and each command's current Interface/Behavior define the review boundary.
Correctness, affected paths, incomplete checks, blockers, retained changes,
recovery instructions and user decisions must remain visible. Typed JSON and
command behavior do not change merely to simplify human wording.

The native help baseline is source candidate `5bef9a24`: all 28 leaf help screens
exit 0, stderr empty, maximum line width 80. Native preview comparisons use the
frozen D1 candidate `c8d786ed`; six commands run in compact, expanded and JSON
forms with equal requests and unchanged workspace hashes. Receipts are under
`artifacts/task27-doctor-correctness/presentation-baseline/` and
`presentation-previews-validated/`. The initial Library attempt used a managed
Framework route and was ownership-blocked; the validated comparison uses the
existing public Library fixture's ordinary consumer boundary. Do not treat that
initial blocked request as evidence that a valid Attach preview succeeded.

## Confirmed Behavior Defects

1. Extension Install, Update and Remove accept `--view` but never consult it in
   `RenderHuman`. The same native previews produce identical compact/expanded
   output: 81, 66 and 90 lines respectively. They expose inventory fingerprints,
   several repeated path lists, verification phases and internal state labels in
   both views. Restore the shared compact/expanded promise while preserving
   affected paths, dependency blockers, preserved files, permissions and recovery.
2. Library Attach, Sync and Detach also ignore `--view`. Successful native dry
   runs show exactly two lines in both views: an ID/mode/plan/status line and a
   repeated status. Their JSON contains the actual mappings, link/record effects,
   validation, application and recovery facts (3,649 / 4,176 / 2,605 bytes in these
   small fixtures). The existing Library Interfaces require affected paths and
   blockers even in compact output, plus full planning and effect detail in
   expanded output. Implement that behavior; do not weaken those contracts to
   match the current renderer.
3. Doctor candidate explanations are emitted as independent warnings with the
   entire candidate array copied into each. The [parsing/Doctor audit](cli-parsing-doctor-audit.md)
   attributes the repeated arrays and output size. Keep candidate facts once per
   broken-link occurrence. Preserve all exact bases, cardinality, ordering and
   the requirement for a user choice; never rank or auto-select a candidate.

## Command Coverage And Bounded Follow-Ups

All implementation paths below are under `src/cli/core/OpenForge.Cli.Core/Commands`.
A shared issue in several rows requires a common review decision, not necessarily
one shared implementation class.

| Command | Reviewed human owner | Finding or retained boundary |
| --- | --- | --- |
| `context` | `Context/Shared/Rendering/Context{Compact,Expanded}HumanRenderer` | Preserve authored text/projection order exactly; simplify only surrounding selection, coverage and repeated path framing. |
| `find` | `Find/Shared/Rendering/Find{Compact,Expanded}Renderer` | Compact TSV is a public representation, not disposable prose. Expanded output fronts several coverage/universe sections before matches; explain searched files and limitations plainly. |
| `references` | `References/Shared/Rendering/References{Compact,Expanded}Renderer` | Expanded section repeats coverage and uses `Level 1`, nulls and layer terminology. Keep direction, occurrence path/line and incomplete selection exact. |
| `index` | `Index/Shared/Rendering/IndexHumanRenderer` | Useful outcome-first wording and real generated diffs already exist. Preserve dry-run diffs and recovery; simplify help terms such as bounded interiors. |
| `install` | `Install/Shared/Rendering/InstallHumanRenderer` | Fronts source hash, classification and footprint; lead with installed/preview/blocked result and affected files. Preserve force and recovery facts. |
| `update` | `Update/Shared/Rendering/UpdateHumanRenderer` | Outcome-first summary exists; source hash and slash-separated state lists dominate. Keep divergence, force/prune, retained paths and effects visible. |
| `status` | `Status/Shared/Rendering/StatusHumanRenderer` and lifecycle/library helpers | Every generated path and several empty sections are shown by default. Keep actual context measurements and lifecycle uncertainty; group current paths and explain recovery terms. |
| `doctor` | `Doctor/Shared/Rendering/Doctor*HumanRenderer` and library helpers | Correct false findings first. Then prominent severity, file:line, plain cause/action, one candidate set, concise normal output and complete JSON. |
| `repair` | `Repair/Shared/Rendering/RepairPresentation` and library helper | Fronts several count/phase summaries and can put Library output before the command heading. Keep selected versus unselected edits, exact previews, unresolved choices and recovery. |
| `cleanup` | `Cleanup/Shared/Rendering/CleanupPresentation` | Lease/preflight/revalidation phases precede the files to remove; candidate lists repeat in expanded output. Keep deletion eligibility, residual paths and actual effect outcome. |
| `route list` | `Route/List/Shared/Rendering/RouteList{Compact,Expanded}Renderer` | Expanded repeats ID/path/parent/depth/provenance per row; review a concise tree with requested depth and incomplete boundaries retained. |
| `route inspect` | `Route/Inspect/Shared/Rendering/RouteInspect{Compact,Expanded}Renderer` | Preserve loading/rule explanations; reduce empty observation/condition sections and unexplained evidence framing. |
| `route create` | `Route/Create/Shared/Rendering/RouteCreateHumanRenderer` | Outcome-first and dry-run effects exist; compact findings are conditional. Verify the direct failure cause remains useful in both views before changing framing. |
| `route init` | `Route/Init/Shared/Rendering/RouteInitHumanRenderer` | Keeps a direct compact finding and preview effects; simplify scaffold/plan/lifecycle state labels without hiding draft entrypoints. |
| `route update` | `Route/Update/Shared/Rendering/RouteUpdateHumanRenderer` | Useful direct error summary and protected-body message exist. Avoid repeating cause/identity; preserve metadata changes and preview bytes. |
| `route move` | `Route/Move/Shared/Rendering/RouteMoveHumanRenderer` | Preserve source/destination, references, ownership blockers, effects and recovery; explain leaf/category and residual state in ordinary language. |
| `route remove` | `Route/Remove/Shared/Rendering/RouteRemoveHumanRenderer` | Preserve all deleted/protected paths and reference decisions; simplify repeated subject/path and technical phase labels. |
| `extension list` | `Extension/List/Shared/Rendering/ExtensionListHumanRenderer` | Useful package rows; simplify coverage/trust/none-established phrases without treating unavailable inventory as empty. |
| `extension inspect` | `Extension/Inspect/Shared/Rendering/ExtensionInspectHumanRenderer` | Keep installed/available/source distinctions; explain dependency closure and generated content, and avoid repeated source/state fields. |
| `extension create` | `Extension/Create/Shared/Rendering/ExtensionCreateHumanRenderer` | Views differ and effects are explicit. Help is 93 lines with compiler-style terms; simplify interaction, existing destination and partial-output explanations. |
| `extension install` | `Extension/Install/Shared/Rendering/ExtensionInstallPresentation` | Confirmed ignored view; restore concise compact output and explain selected packages, dependencies and actual effects. |
| `extension update` | `Extension/Update/Shared/Rendering/ExtensionUpdatePresentation` | Confirmed ignored view; group comparisons/effects by path, retaining force/prune and source limitations. |
| `extension remove` | `Extension/Remove/Shared/Rendering/ExtensionRemovePresentation` | Confirmed ignored view; explain packages still needed, shared files, preserved edits and unused dependencies left installed. |
| `library list` | `Library/List/Shared/Rendering/LibraryListPresentation` | Views differ mainly in header; simplify dense source/link fields while preserving no-inventory-scan semantics. |
| `library inspect` | `Library/Inspect/Shared/Rendering/LibraryInspectPresentation` | Keep complete source inventory and exact destination/link differences; explain comparison relation and unknown facts. |
| `library attach` | `Library/Attach/Shared/Rendering/LibraryAttachPresentation` | Confirmed missing human mappings/effects and ignored view, contrary to its Interface. |
| `library sync` | `Library/Sync/Shared/Rendering/LibrarySyncPresentation` | Confirmed missing human created/retired paths and ignored view, contrary to its Interface. |
| `library detach` | `Library/Detach/Shared/Rendering/LibraryDetachPresentation` | Confirmed missing human removed/absent paths and ignored view, contrary to its Interface. |

## Frozen Implementation Sequence

The following sequence refines the accepted plan. Each set needs a committed
baseline, explicit contract delta, frozen semantic evidence, final review and
appropriate qualification before squash integration. This report is not an
unqualified blanket rewrite instruction.

1. D1 and D2 are complete: false coverage/source/currentness findings are fixed.
   Next freeze the shared presentation design and representative before/after
   outputs for success, findings, dry-run, blocked and incomplete results.
2. Doctor and Status presentation: clear severity, file:line, plain cause and
   useful next action. Reuse existing typed facts. Keep all diagnostic kinds,
   JSON and exit behavior. Detailed grouping and visibility proposals are a
   separate later discussion; do not retire findings to shorten human output.
3. Library mutation presentation: restore the already required affected-path and
   dry-run detail in both views. Freeze JSON/operation behavior and no-write checks.
4. Extension mutation presentation: make compact meaningful and keep expanded
   planning/effects grouped by path. Preserve selection, dependencies, permissions,
   changed/shared content and recovery information.
5. Remaining read-only and mutation presentation in bounded command families, then
   all help screens. Move common stream/exit reference repetition only after
   checking what each Interface requires. Avoid adding hidden defaults or new
   accepted syntax through a wording edit. Colours remain a later stage.

Exact human wording/layout is a snapshot subject. Use small authored fixtures
and reviewed expected output, with stable workspace placeholders where needed;
do not manufacture scenarios by replacing incidental prose in live Markdown.
Keep separate assertions for actual status, findings, path sets, cardinality,
JSON facts, source safety, effects and unchanged files. A snapshot update never
qualifies changed behavior by itself.

## Next Analysis And Freeze

Start from the existing command inventory, shared-operation contract, global
flags/result-coordinates contracts and Writing Standard. The existing default
view is expanded; compact is explicitly intended for scanning and agent use.
Improve both before inferring that a new default or flag is needed. Keep Find's
compact TSV and Context's selected authored bytes intact.

For representative real results, review the following shape against each owner:

| Surface | Facts the reader needs first | Detail organization |
| --- | --- | --- |
| Read-only list/inspection | What was inspected, useful rows, incomplete checks | Shared context once; stable identities/order; expanded explanations beside the relevant item |
| Doctor findings | Severity, affected file and line, plain cause, available action | Related details under the same occurrence; keep distinct facts and source-edit coordinates available |
| Status | Current state and useful measurements, then items needing attention | Group repeated current paths; preserve uncertainty and the meaning of every measurement |
| Mutation dry-run | Explicit preview, affected paths and proposed effects, blockers | Group by path/package; distinguish would-change from applied, retained or unavailable |
| Applied/blocked/interrupted operation | Actual outcome, retained changes and recovery action when applicable | Preserve operation boundaries; explain technical states in ordinary words |

The design should use stable labels, consistent ordering and plain text that
works when piped or copied into an AI context. Put the outcome before internal
phase names; make severity distinguishable without colour. Reuse a path/header
within one group instead of printing it repeatedly. Avoid decorative tables or
framing that cost space without helping comparison. Keep useful diagnostic codes
as secondary detail, not as the only explanation. Do not silently truncate user
content or hide incomplete checks to reduce output size.

Freeze representative output snapshots on small committed authored fixtures,
with separate assertions for status, selected facts, path sets, JSON, safety and
actual effects. Measure lines and UTF-8 bytes on those fixed cases; claim token
savings only with a stated tokenizer/measurement. Then implement sequentially:
Doctor/Status; Library mutations; Extension mutations; remaining read-only and
mutation command families; help. Review contracts and qualify each set before
squash integration. Keep the final reusable CLI/C# guidance consolidation queued
at the end of the overall sequence.

## Temporary Evidence Cleanup

The task-owned exploratory programs, parser probe and task artifact directories
were removed at the user's request. The measurements above are recorded historical
results. Required regressions remain committed under src/cli/tests; durable
qualification summaries and exact rebuild/test commands are in their Task records.
No removed experiment is a dependency of the next presentation stage.
