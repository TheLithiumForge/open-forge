---
open-forge:
  description: Propose compact and expanded output for each of the 28 CLI commands for user review before contract or renderer changes
  tags: [Memory, Working, Contextual, CLI, Task, Presentation, Proposal]
---

# Command Output Proposals

## Review Boundary

These are recommendations for user approval, not new command contracts. They
refine the [presentation analysis](cli-presentation-design.md) and its
[28-command audit](cli-presentation-audit.md). The user clarified that
warning/error/info was an example of filtering, not a required vocabulary or
flag. Preserve existing statuses and diagnosis facts. Withdraw the recommendation
to make a Doctor severity flag the next public-interface step.

After approval, update each affected command's `interface.md` before implementing
its frozen set. Align Behavior only where it constrains the approved presentation;
update technical design if the implementation boundary changes. Do not convert
this Working proposal wholesale into accepted authority. The
[Public Facing Writing Directive](../../../../directives/public-facing-writing.md),
[Writing Standard](../../../crystallized/documents/maintenance/writing.md) and
[Dictionary](../../../crystallized/documents/maintenance/helpers/dictionary.md)
define the wording. No Interface, Behavior, renderer, flag or JSON change is made
in this analysis set.

The [review gallery](cli-command-output-examples.md) provides a proposed example
for every command, with compact/expanded notes and shared unsuccessful outcomes.

## Recommended Common Shape

For ordinary human output, use this reading order: outcome, exact subject and
workspace, useful results or effects, incomplete checks and remaining problems,
then a required next action when supported. Put a blocker or failed check before
rows that could otherwise look like a complete result. Retain workspace selection
method where required. Avoid repeating status in the heading and footer.

Keep the existing semantic statuses. Render `attention` as `requires attention`,
with the concrete reason immediately nearby. `incomplete` means some required
facts are unavailable; it must not read as a successful empty result. `blocked`
means a required condition prevented progress. Distinguish invalid input, failure
and interruption, including effects already made. A complete preview is a
complete plan, not a completed write. Doctor's existing finding severities remain
valid details; there is no need to invent them for other commands.

Compact shows the useful answer with short rows. Expanded, still the default,
adds the explanation needed to understand the same answer. Both retain required
identities, coverage, blockers, affected paths and recovery. Long results can be
long when the details are distinct; remove duplicated framing rather than impose
arbitrary truncation. Keep existing exact output forms where they are intentional,
particularly Find TSV, selected content and mutation preview diffs.

Use paths, IDs and commands without destructive wrapping or truncation. Show
available file coordinates inline. Explain why something matters instead of
exposing labels such as `provenance`, `basis`, `bounded` or phase names without
context. Preserve precise qualification: a possible target is not a recommended
target; a file that could not be read is not known to be missing. Suggestions
must come from supported result facts or accepted command guidance.

## Filtering Recommendation

Improve grouping and both views first; do not add a global filter in the first
set. Existing `--view compact` is the right initial way to request less framing
while keeping relevant information. It currently needs real implementation work
in the six Library/Extension mutation renderers identified by the audit.

| Possible filter | Where it helps | Why it is not the initial recommendation |
| --- | --- | --- |
| Overall command status | Automation deciding what to do with a result | Already available through status/exit/JSON. Filtering a single report by its own status mostly hides the whole answer. |
| Per-finding or per-item status | Commands whose collections already carry meaningful statuses | Useful locally, but collection status is not consistent across all commands; preserving `attention` does not require a universal status filter. |
| Doctor category | Focusing on links, routes or another area in a large diagnosis | Strong candidate after category grouping is reviewed. A display filter must still disclose overall coverage and omitted categories. |
| Doctor severity | Inspecting one existing severity within diagnosis | Valid possible option, but the user's example does not select this syntax or make it a priority. |

If the revised large reports still need a filter, recommend a narrowly scoped
Doctor category filter first for task focus, then evaluate a status/severity
selector where the result actually has that field. Keep display selection
separate from limiting which checks execute. Before adding either, freeze the
included facts, full versus displayed counts, empty-match message, required
actions, JSON behavior and native parser binding. Do not hide failed checks or
change status/exit because displayed rows were filtered. No spelling is selected
by this proposal.

## Read And Diagnose Commands

Each command name links to the Interface that must be reviewed and updated after
approval. The compact column identifies the primary answer, not an exhaustive
permission to omit other currently required facts. Shared safety and output rules
above apply to every row.

| Command | Proposed compact presentation | What expanded adds |
| --- | --- | --- |
| [context](../../../crystallized/documents/cli/contracts/context/interface.md) | Keep selected authored content in exact projection order, with minimal source/section framing and explicit missing selections. Do not summarize bodies or add a dashboard before them. | Explain selection and loading relationships once per source; retain all selected content bytes and incomplete-reading facts. |
| [find](../../../crystallized/documents/cli/contracts/find/interface.md) | Keep the defined TSV summary and ID/path rows. Preserve requested projections, findings and no-match versus incomplete-search distinction. | Put matches near the top, after any limitations that affect their interpretation. Show why each match qualifies, locations and requested content; describe the searched set once. |
| [references](../../../crystallized/documents/cli/contracts/references-candidate/interface.md) | Use separate `Incoming links` and `Outgoing links` sections with their counts and coverage. Use source-to-target rows and concise unresolved reasons. Explain once that only direct links are included. | Add exact occurrence coordinates, authored destination, resolved identity and base/overwrite source. State `External URL; not checked` when applicable; omit meaningless local-ID null fields. Replacing the required `Level 1` label needs Interface approval. |
| [index](../../../crystallized/documents/cli/contracts/index-candidate/interface.md) | Lead with `Generated navigation is current`, `Updated generated navigation`, or `Preview of generated navigation changes`, only when the typed outcome supports it. Keep affected paths, unchanged counts and every required preview diff. | Put entry counts and explanation beside each changed file. Keep recovery facts when relevant; do not print successful planning phases. |
| [install](../../../crystallized/documents/cli/contracts/install/interface.md) | State installed/already installed/preview/blocked, exact workspace, mode and every affected or preserved path. Identify force effects and generated navigation. | Group source and footprint facts once, then explain file/region effects, lifecycle publication and verification. Keep unresolved recovery explicit. |
| [update](../../../crystallized/documents/cli/contracts/update/interface.md) | State updated/already current/preview and group paths into changed, preserved and removed where applicable. Explain remaining local edits and active force/prune choices. | Show baseline/current/intended comparisons beside each relevant path, with source and lifecycle detail once. Distinguish planned effects from verified or uncertain effects. |
| [status](../../../crystallized/documents/cli/contracts/status/interface.md) | A workspace summary: installation, startup/continuity totals, route changes/navigation, Extensions, Libraries and recovery. Combine healthy navigation rows into a count; list exceptions and every required recovery candidate. | Add context measurements/largest sources and full navigation/lifecycle explanation. Keep unavailable measurements and absent versus empty records distinct; do not repeat Doctor's full diagnosis. |
| [doctor](../../../crystallized/documents/cli/contracts/doctor/interface.md) | Outcome, read-only workspace, coverage/counts, then Workspace, Recovery, Routes and navigation, Links, Framework and Extensions. Group related findings by subject/occurrence; show existing severity/status facts, cause and required action. | Show evidence and candidate reasons once per justified group, preserving member identities and counts. Libraries remain inside Workspace. Retain all diagnostic kinds; no severity-policy change. |
| [repair](../../../crystallized/documents/cli/contracts/repair/interface.md) | State preview/applied/blocked, selected work, exact affected paths/diffs, work left unresolved and required recovery. Keep selected versus unselected counts and diagnosis coverage. | Explain choices and evidence beside their effects, then verification and post-repair diagnosis. Display Library residual attribution in the related recovery section, after the command outcome. |
| [cleanup](../../../crystallized/documents/cli/contracts/cleanup/interface.md) | Lead with what would be removed, what was removed and what remains. List every exact candidate/effect and eligibility problem; a count cannot replace the list. | Explain candidate kind, integrity, deletion eligibility and final validation once beside the relevant path. Preserve lease/revalidation facts without making the reader understand phases to find the outcome. |

## Route Commands

| Command | Proposed compact presentation | What expanded adds |
| --- | --- | --- |
| [route list](../../../crystallized/documents/cli/contracts/route/list/interface.md) | Summary of selected roots, depth, coverage and rows, followed by an indented route list retaining exact authored descriptions/tags and IDs/paths. State a required finding before safe rows. | Explain selection/depth once, then add distinct structural facts. Avoid repeating a row's path as parent/path/provenance framing where the tree already expresses that relationship. Preserve absolute/relative depth facts where required. |
| [route inspect](../../../crystallized/documents/cli/contracts/route/inspect/interface.md) | Source identity, route chain, when it is read, own/added context measurements and overwrite state. Preserve zero/unavailable/not-applicable and any condition explaining non-complete status. | Organize around `Where this source belongs`, `When it is read`, `Context size` and `Applicable rules`; explain inherited Axioms and evidence. This command explains a route and must not invent diagnosis or recommend edits. |
| [route create](../../../crystallized/documents/cli/contracts/route/create/interface.md) | State created/already present/preview with target, Template when supplied, affected paths and generated-navigation effects. Retain exact preview changes. | Explain the created content and verification beside each path. State a collision or invalid Template directly; keep ordinary recovery guidance when needed. |
| [route init](../../../crystallized/documents/cli/contracts/route/init/interface.md) | State initialized/already initialized/preview. List created and unchanged entrypoints, draft paths and generated changes. | Explain which folders were initialized and any incomplete structure. A draft must remain visibly a draft, not a completed route. |
| [route update](../../../crystallized/documents/cli/contracts/route/update/interface.md) | Show target, changed fields with before/after values, affected paths and preview effects. State plainly when an authored body prevented Template application. | Add unchanged selected fields, Template identity and body-protection evidence. Recovery owns the one required next action when it takes precedence; keep the protected-body fact visible. |
| [route move](../../../crystallized/documents/cli/contracts/route/move/interface.md) | Source-to-destination identity, moved file or route category, every reference rewrite, generated effect and preview diff. | Explain reference coverage, verification and retained partial effects beside their paths. Do not report a move as complete when completion is unknown. |
| [route remove](../../../crystallized/documents/cli/contracts/route/remove/interface.md) | Selected file or category, deleted paths, every reference detachment, generated effect and any protected content. Keep exact preview effects. | Explain detachment decisions, coverage and verification. Distinguish retained files from successfully deleted files and expose partial-state recovery. |

## Extension Commands

| Command | Proposed compact presentation | What expanded adds |
| --- | --- | --- |
| [extension list](../../../crystallized/documents/cli/contracts/extension/list/interface.md) | Separate Installed and Available rows with stable IDs and versions when known. Put catalogue source and coverage beside section headings. Say installed state is unavailable when it cannot be read, rather than presenting an unqualified zero. | Add dependency/package counts, managed paths and reasons a record/source could not be read. Keep absent, empty and untrusted state distinct. |
| [extension inspect](../../../crystallized/documents/cli/contracts/extension/inspect/interface.md) | Selected ID, installed and available state, source, comparison mode and path counts; show significant differences and required actions. | Explain dependencies and path comparisons as installed baseline, current workspace and selected package content. Show generated navigation as derived content; retain the full comparison facts. |
| [extension create](../../../crystallized/documents/cli/contracts/extension/create/interface.md) | State package created/preview, ID, catalogue/destination, manifest metadata, dependency IDs and scaffold paths. State that workspace installation was not changed. | Describe each generated file and verification. Preserve a collision, invalid dependency or partial-output condition without suggesting that creating a package installs it. |
| [extension install](../../../crystallized/documents/cli/contracts/extension/install/interface.md) | State selected packages and required dependencies, source, preview/applied mode and affected/preserved paths. Keep permission decisions and blockers prominent. | Group effects by package/path, then explain Framework anchor, lifecycle and verification once. Moving human output away from JSON property order explicitly requires changing this Interface. |
| [extension update](../../../crystallized/documents/cli/contracts/extension/update/interface.md) | Show selected packages, what changes and what local content stays. Distinguish normal, force and prune effects, including shared paths and remaining divergence. | Add per-path baseline/current/intended explanation and package dependency context, followed by verified effects and recovery. Preserve dependency ordering in operation and JSON. |
| [extension remove](../../../crystallized/documents/cli/contracts/extension/remove/interface.md) | Show selected packages, dependents preventing removal, paths deleted or kept, and packages/dependencies left installed. Make keeping a file unmanaged distinct from keeping package ownership. | Explain shared/final ownership and selected keep/delete decisions beside each path, then lifecycle and recovery. Do not introduce legacy migration guidance. |

## Library Commands

| Command | Proposed compact presentation | What expanded adds |
| --- | --- | --- |
| [library list](../../../crystallized/documents/cli/contracts/library/list/interface.md) | One row per Library with ID, source availability and registered-link state/counts. For a missing record, say `No Libraries are registered` and identify the record as missing. State that source inventory was not scanned. | Source roots, destination/link observations and distinct record problems. Do not turn this cheap listing into a full inventory scan. |
| [library inspect](../../../crystallized/documents/cli/contracts/library/inspect/interface.md) | Library ID, source state, inventory coverage and comparison counts, followed by meaningful destination/link differences and required findings. | Full eligible/registered path inventory with observed link and comparison relation per path. Distinguish changed, missing, blocked and unavailable rather than treating all as missing. |
| [library attach](../../../crystallized/documents/cli/contracts/library/attach/interface.md) | State attached/preview, source and destination roots, every affected mapping, record effect and blocker. Explicitly identify links as links, not copied content. | Eligible/excluded/incomplete inventory facts, collision explanation, generated changes and verification/recovery. This restores detail already required by the Interface but absent from current output. |
| [library sync](../../../crystallized/documents/cli/contracts/library/sync/interface.md) | State synchronized/already synchronized/preview and list created/retired links, preserved occupants, source/destination roots and record effect. | Explain source/registered comparison and exact link-target checks beside each affected path, then generated effects and recovery. Never describe removal of a link as deletion of its external source. |
| [library detach](../../../crystallized/documents/cli/contracts/library/detach/interface.md) | State detached/preview, removed or safely absent registered links, blockers and record effect. Preserve the source-independent nature of the operation. | Explain missing/dangling/changed link classifications, exact target checks, generated effects and recovery. Do not imply that source content was removed or that a changed local occupant was safely deleted. |

## Concrete Review Examples

These are proposed layouts. The first two use facts observed with the installed
CLI in this worktree; subsequent excerpts use placeholders and are not execution
receipts. The tables and current contracts define remaining required facts.

### Library List: Empty Record

Current compact output repeats its status and presents internal field names:

```text
Library list: record=missing; inventory=not-requested; coverage=complete; libraries=0; status=complete
Status: complete
```

Proposed compact output:

```text
No Libraries are registered.
Workspace: /tmp/open-forge-cli-refactor-sequential
Selected by: current directory
Record: .agents/open-forge.libraries.json (missing)
Source inventory was not scanned.
Status: complete
```

This is longer than two lines because it restores useful context. Concision is
not a line-count target at the expense of meaning. Expanded can use the same
shape here because there is no additional inventory to explain.

### Extension List: Installed State Unavailable

Current compact starts with `installed=0` even though a later finding says the
missing lifecycle document prevents proving that the installed set is empty.
Proposed compact:

```text
Installed Extensions could not be determined.
Workspace: /tmp/open-forge-cli-refactor-sequential
Selected by: current directory
Status: incomplete

Installed: unavailable
  .agents/open-forge.lifecycle.json was not found.
Available from the embedded catalogue: 6
  development          0.1.0
  development-toolkit  0.1.0
  memory-starters      0.1.0
  orchestration        0.1.0
  planning             0.1.0
  project-documents    0.1.0

Next: open-forge doctor
```

This changes the explanation using existing availability facts. It does not
change the stored count, JSON, observed installation state or exit 3. Expanded
adds the available package/dependency details and the precise missing-record
finding once, without repeating the same cause in several sections.

### Doctor: One Finding Group

Proposed excerpt, with the report's outcome, workspace, coverage and counts above:

```text
Links
  WARNING  Broken link
  <source-path>:<line>:<column>
  The linked file was not found: <authored-destination>

  Possible targets: <count>; a choice is required.
    <candidate-path>
      Why listed: <plain explanation of the existing candidate basis>

  Next: <supported action for this occurrence>
```

Compact keeps candidate count, distinct finding facts and required action;
expanded shows candidate details once. Preserve actual member severities and
identities; the excerpt does not collapse multiple findings into one count.
Omit the candidate section when none exists, and never invent a next action.

### Mutation Preview: Path Effects

Proposed excerpt for Library Attach, after Library/source/destination identity:

```text
The Library would be attached.
Would create links:
  <destination-path> -> <source-path>
Would update the Library record:
  .agents/open-forge.libraries.json

No files changed (--dry-run).
Status: complete
```

Use `create` instead of `update` when the record does not exist. Include actual
generated effects and blockers. Expanded adds eligibility, collision checks and
verification facts that are available for the preview. Applied output uses the
observed application outcome, not this plan relabelled as success.

### Failure After Effects Begin

Proposed reusable arrangement, not a new recovery policy:

```text
<Operation> did not finish.
Status: failed
<Exact subject and workspace>

Completed and verified:
  <path and effect>
Not completed or completion unknown:
  <path and actual observed condition>

Recovery data: <observed retained path and condition>
Next: <required action from the result>
```

Print only established categories and facts. Never assert that no files changed,
that all completed effects were verified, or that recovery data exists without
support. Interrupted and blocked outcomes use their actual status and cause.

## Contract Changes And Implementation Order

Approval is for the proposed presentation, not a new diagnosis or mutation
policy. For each approved set, update the linked command Interfaces' human-output
sections, examples and help requirements. Preserve JSON schemas and exact data
semantics. Specifically review the shared global-view description, Status's
stable sections, References' `Level 1` requirement, Doctor's human hierarchy,
and Extension Install's human/JSON ordering requirement. These are concrete
contract changes, not permission to disregard today's contract during coding.

Freeze a small set of representative typed-result fixtures and proposed snapshots
before renderer changes. Keep independent assertions for findings, status, exit,
JSON, required actions, selected content bytes, exact preview diffs and effects.
Cover success, empty, no-op, incomplete, blocked, invalid, failed/interrupted with
partial effects where supported. These shared cases apply to every applicable
command row; they are not replaced by the successful examples above.

After user approval: Doctor/Status, Library mutations, Extension mutations,
remaining read/inspect families, remaining mutations and help. Keep each set
sequential, verified and locally squash-integrated. Consolidate reusable CLI
UX/development Guidance and CLI/C# Directives last. The parsing backlog remains
separate. A new display filter can be considered after the first diagnosis output
is reviewed on a large workspace; it is not a prerequisite or selected flag.

## Checkpoint And Evidence

Baseline: local `develop` `b94f8359`; installed native source `29de40a0`.
Reviewed the human-output sections of the 28 linked Interfaces and the existing
renderer audit; refreshed Status, Find and References renderer reads. The earlier
audit owns the wider renderer inventory and mutation-preview comparisons.
This is a per-command proposal, not exhaustive review of every message or a new
runtime qualification claim.

Direct installed CLI checks in the existing worktree: Library List compact exited
0 with the two lines quoted above; Extension List compact exited 3 with six
available packages and the missing lifecycle finding; Route List compact for the
presentation-analysis source exited 0 and repeated selection/depth/provenance
framing around one row. All three had empty stderr. The lifecycle finding is an
observed workspace condition, not a newly proved operation defect; the unqualified
zero in human output is the presentation problem. No experiment file is retained.

Next: user review of this per-command proposal. Do not change command Interfaces
or implement the proposed layouts before approval. Tracked plans/checkpoints own
this boundary; nothing required for recovery lives in artifacts.

Validation: installed References returned complete for this proposal (34 outgoing
references), the gallery (2) and the revised design analysis (8), with no findings
or stderr. The gallery contains all 28 numbered commands. Generated task
navigation was refreshed with the installed Index command; its unrelated
two-blank-line delivery-index normalization was verified and reverted.
`git diff --check` passes. No source/tests/contracts changed, so no production
suites were rerun. This Working documentation set may be locally squash-integrated
without treating the proposed presentation as approved.
