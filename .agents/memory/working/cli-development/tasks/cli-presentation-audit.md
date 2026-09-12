---
open-forge:
  description: Review all CLI presentation surfaces and sequence verified fixes for missing detail, duplication, and unclear writing
  tags: [Memory, Working, Contextual, CLI, Task, Presentation, Writing, Dogfood]
---

# CLI Presentation Review

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

1. Finish Doctor D1, then D2: separate current generated navigation from persisted
   install-time fingerprints, keeping real stale/missing/unsafe region diagnosis
   under Routes/Index and genuine authored Framework drift under Framework.
2. Doctor presentation and JSON nonduplication: show actionable errors and warnings
   clearly; summarize informational facts and checked domains. Keep broken links
   visible. Use file:line rather than detached coordinates. Machine byte offsets
   remain in JSON for exact edits. Retire redundant candidate finding emissions
   only with an explicit diagnostic-catalogue update and Repair consumer checks.
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

## Presentation Decisions Requiring User Review

The user's latest preservation boundary applies before implementing these
changes. Neither is part of D1.

### Candidate Diagnostic Consolidation

Current output emits the broken-link finding plus candidate-only findings named
`reference.candidate-filename`, `reference.candidate-title`,
`reference.candidate-literal-content`, `reference.candidate-route-neighborhood`,
`reference.candidates-none`, `reference.candidates-one`, and
`reference.candidates-several`. Each carries the same occurrence and full
candidate set. Proposed output emits the broken-link finding once, keeping its
entire candidate set, every basis, exact locations, cardinality and provenance.
No match is selected and no repair capability or source evidence is removed.

This would retire those seven finding kinds from the unreleased catalogue
(120 to 113) and change aggregate finding counts accordingly. Existing JSON
object fields remain present, but consumers relying on the seven redundant
kind emissions would observe a change. That representation change requires the
user's approval, explicit Interface/Behavior catalogue updates, frozen candidate
fact-equivalence evidence and Repair consumer checks. It is not merely wording.

### Normal Human Visibility

Proposed normal human output shows each error and warning with a clear severity,
file:line, cause and next action. It summarizes informational findings rather
than printing every valid link and cycle. All typed findings, details, precise
byte offsets and counts remain in JSON; failures and incomplete checks stay
visible. Compact and expanded still use the same diagnosis and exit status.

This changes default human visibility, so it requires user approval. The
alternative is to keep every finding visible and improve only formatting and
language. No default is changed while that decision is pending. Colours remain
out of this set.
