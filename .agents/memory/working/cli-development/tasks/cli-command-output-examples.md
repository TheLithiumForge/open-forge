---
open-forge:
  description: Review proposed human output examples for all 28 CLI commands before approving presentation changes
  tags: [Memory, Working, Contextual, CLI, Task, Presentation, Examples]
---

# CLI Output Review Gallery

## Latest View And Colour Analysis

The user endorses the natural human style. The
[views, JSON and colour analysis](cli-view-format-color-analysis.md) now compares
compact output for people and AI, explicit compact JSON projections and optional
terminal colour. These additions remain proposals. Earlier promises to preserve
full JSON apply to the human-only implementation stages; a JSON-view change
requires its own approved contract and qualification. There is no automatic view
fallback and no separate AI view is recommended.

## How To Review

These are proposed human layouts for all 28 commands, using small illustrative
results in `/work/demo`. They are not captured output or claims about that
workspace. Each example shows the recommended reading order and wording for a
representative result. They are not exhaustive output snapshots: detailed source
fingerprints, evidence and optional result branches must be placed and frozen
with the actual fixtures before implementation. No implementation, Interface or
Behavior change is made by this gallery.

The [per-command proposal](cli-command-output-proposals.md) defines compact versus
expanded detail, retained behavior and the Interface link for each command.
The [analysis](cli-presentation-design.md) explains implementation boundaries.
Read the examples as expanded unless marked compact. A short complete result can
use the same layout in both views when there is no additional explanation.

Common rules: status appears once; `requires attention` retains the existing
`attention` meaning; incomplete checks stay visible; previews say that no files
changed. Actual paths, IDs and commands stay complete. Existing finding severity
is displayed where the result has it, without inventing a common severity model.
A next action is printed only when the result or accepted guidance supports it.

## Read And Diagnose

### 1. context

Selected source content remains verbatim. Only the generated framing changes.
For this example the selected source's complete authored body is the three lines
under its Markdown heading; this is not replacement wording for a real source.

```text
Context: directives/review
Workspace: /work/demo
Selected by: current directory
Status: complete

Source: .agents/directives/review.md
# Review

Check the changed behavior and its tests.
```

Compact keeps the selected body and source boundary. Expanded explains why the
source was selected and its loading relationships when those facts apply, without
injecting that explanation into the authored body.

### 2. find

```text
Found 2 matching sources.
Workspace: /work/demo
Selected by: current directory
Search: default source set; complete
Status: complete

directives/review
  .agents/directives/review.md
  Matched tag: Testing

workflows/release
  .agents/workflows/release.md
  Matched tag: Testing
```

Compact retains the existing tab-separated summary and full ID/path rows,
not this expanded layout. Requested content stays verbatim.
An incomplete search says so before the matches; zero matches is not proof that
an incomplete search found everything.

### 3. references

```text
Direct links for directives/review
Workspace: /work/demo
Selected by: current directory
Status: complete

Incoming links: 1; scan complete
  .agents/workflows/release.md:18:4
    Links to: .agents/directives/review.md
    Written as: ../directives/review.md
    Source: base file

Outgoing links: 1; inspection complete
  .agents/directives/review.md:7:3
    Links to: https://example.com/review
    External URL; not checked over the network.
    Source: base file
```

Compact uses short source-to-target rows, retaining requested directions and
coverage. Expanded retains exact source identity, destination and resolution
facts. Removing the `Level 1` wording requires the proposed Interface update.

### 4. index

```text
Preview of generated navigation changes
Workspace: /work/demo
Selected by: current directory
Status: complete

Would update: .agents/directives/_directives.md
Entries: 1 -> 2
@@ -1 +1,2 @@
 - [Review changes](review.md)
+- [Check tests](testing.md)

Other selected entrypoints: 3 already current
No files changed (--dry-run).
```

The block above illustrates placement; executable snapshots must use the exact
actual bounded diff, including its real prefix and spacing. Compact also retains
every required diff. Applied output names verified changes instead of a preview.

### 5. install

```text
Open Forge was installed.
Workspace: /work/demo
Selected by: current directory
Mode: apply; normal; automatic
Source: embedded Framework
Status: complete

Created files and directories:
  <every created path>
Updated managed regions:
  <path and actual region effect, when present>
Generated navigation:
  <every generated effect>

Lifecycle record: saved
Verification: all planned effects verified
Recovery data: removed
```

Place source identity and footprint counts together in expanded detail. Compact
retains exact affected/preserved paths, mode and safety facts. Never infer all
verification succeeded merely because some writes completed.

### 6. update

```text
Open Forge was updated. One local edit was kept.
Workspace: /work/demo
Selected by: current directory
Mode: apply; normal; automatic; prune disabled
Source: embedded Framework
Status: requires attention

Updated and verified:
  <updated path>
Kept local changes:
  <preserved path>
    The workspace content differs from its installed baseline.
Generated navigation:
  <actual generated effects>

Lifecycle record: updated
Recovery data: removed
```

Expanded places baseline/current/selected-source comparisons beside each relevant
path. Compact retains preserved changes and active force/prune choices. A next
action is not invented just because the result needs attention.

### 7. status

```text
Open Forge is installed. Some generated navigation needs updating.
Workspace: /work/demo
Selected by: current directory
Status: requires attention

Context
  Startup: <files>, <characters>, <bytes>, approximately <tokens>
  Shipped startup: <same available measurements>
  Difference: <measured difference>
  Continuity: <same available measurements>
  Startup share: <available percentage>

Routes
  Root categories: <count>; added: <IDs or none>; removed: <IDs or none>
  Generated navigation: 12 current, 1 needs updating
    .agents/directives/_directives.md

Extensions: <record state and managed-content summary>
Libraries: <record/source/link states and counts>
Recovery: 0 verified records, 0 incomplete drafts

Next: open-forge doctor
```

Expanded adds total context/largest sources, full navigation detail and lifecycle
explanations. Compact keeps the required measurements, root changes, state
summaries and every recovery candidate. Unavailable measurements are labelled,
never replaced with zero. The placeholders avoid inventing measurement values.

### 8. doctor

```text
The workspace needs attention. No files changed.
Workspace: /work/demo
Selected by: current directory
Status: requires attention
Checks: complete in all 6 categories
Findings: 0 errors, 1 warning, 0 information
Resolution: 1 manual decision

Workspace: checks complete; no findings
Recovery: checks complete; no findings
Routes and navigation: checks complete; no findings

Links: checks complete; 1 warning
  WARNING  Broken link
  .agents/directives/review.md:12:4
  The linked file was not found: ../guidance/testing.md
  Action: check the destination, then update or remove the link.
  Resolution: manual decision
  Code: reference.target-missing

Framework: checks complete; no findings
Extensions: checks complete; no findings
```

This example intentionally contains one finding with no candidate facts. For a
result with several related findings, list distinct explanations and retain their
count; show shared candidate evidence once. Do not relabel a group as one finding.
Expanded adds actual evidence; compact keeps identity, resolution and actions.

### 9. repair

```text
Preview of selected repairs
Workspace: /work/demo
Selected by: current directory
Selection: automatic
Status: complete
Diagnosis: <actual coverage>
Selected checks: complete

Would repair:
  <path>
    <selected exact change and bounded diff>

Selected: <count>; not selected: <count>
Remaining: <guided/manual/blocked work, when present>
No files changed (--dry-run).
```

Expanded explains selection, evidence and preflight facts. Applied output adds
actual verification, remaining/new findings from post-repair diagnosis and
recovery. A preview must not claim that post-repair checks have run.

### 10. cleanup

```text
Preview of recovery-data cleanup
Workspace: /work/demo
Selected by: current directory
Status: complete
Candidate check: complete

Would remove:
  <exact recognized recovery-artifact path>
    <actual kind and integrity condition>
    Eligible for removal after the final workspace check.

Would keep:
  <exact path and actual reason, if present>

No files changed (--dry-run).
```

Expanded explains lease and final validation where applicable. Compact keeps
every candidate/effect and required safety condition; it does not replace the
paths with a count. Applied output reports verified removal and anything remaining.

## Routes

### 11. route list

```text
Routes under guidance
Workspace: /work/demo
Selected by: current directory
Status: complete
Coverage: complete; depth: 1; routes: 2

<full root ID>  .agents/guidance/_guidance.md
  <exact authored description>; tags: <exact authored tags>
  <full child ID>  .agents/guidance/review.md
    <exact authored description>; tags: <exact authored tags>
```

Compact retains the existing hierarchical rows and exact metadata. Expanded adds
distinct structure and selection facts without repeating the same path/parent
relationship in every explanatory field. The tree never invents an ancestor row.

### 12. route inspect

```text
Route: guidance/review
Workspace: /work/demo
Selected by: current directory
Path: .agents/guidance/review.md
Status: complete

Where this source belongs
  Parent: guidance
  Route chain: guidance -> guidance/review
  Depth: <actual depth>; children: <actual state/count>

When it is read
  <actual task-start, automatic and later-reading facts>

Context size
  This source: <available measurements>
  Added by selecting it: <available measurements>
  Automatically loaded descendants: <available measurements>

Overwrite: <actual overwrite state>
Applicable rules: <actual inherited and local Axioms>
```

Compact keeps the required route/loading/measurement facts. Expanded explains
why those relationships apply. This is an explanation of a route, not a Doctor
report, and it does not invent advice to edit content.

### 13. route create

```text
The route was created.
Workspace: /work/demo
Selected by: current directory
Target: guidance/review
Path: .agents/guidance/review.md
Status: complete

Created and verified:
  .agents/guidance/review.md
Updated generated navigation:
  .agents/guidance/_guidance.md

Recovery data: removed
```

Expanded includes a supplied Template's identity and actual effect evidence.
Compact previews still show every exact planned effect/diff. An existing valid
route can report a verified no-op without claiming it was created again.

### 14. route init

```text
The route was initialized.
Workspace: /work/demo
Selected by: current directory
Target: guidance
Path: .agents/guidance/_guidance.md
Status: complete

Created and verified:
  .agents/guidance/_guidance.md
Already initialized:
  <unchanged entrypoints, when present>
Generated navigation:
  <actual generated effects>
Draft entrypoints: none

Recovery data: removed
```

Expanded explains affected folders and verification. Both views name any real
draft paths and incomplete structure; neither turns a draft into a ready route.

### 15. route update

```text
The route metadata was updated.
Workspace: /work/demo
Selected by: current directory
Target: guidance/review
Path: .agents/guidance/review.md
Status: complete

Description:
  Before: Review
  After: Review code changes and their tests
Updated generated navigation:
  .agents/guidance/_guidance.md

Verification: selected changes verified
Recovery data: removed
```

Show every selected field's actual changed/unchanged state. If a supplied Template
body was protected, say that directly and retain its required action; do not use
this successful example for a partially applied Template request.

### 16. route move

```text
The routed file was moved.
Workspace: /work/demo
Selected by: current directory
Status: complete

From: guidance/review — .agents/guidance/review.md
To: guidance/code-review — .agents/guidance/code-review.md
Updated references: 1; reference scan complete
  .agents/workflows/release.md:18:4
    ../guidance/review.md -> ../guidance/code-review.md
Updated generated navigation:
  .agents/guidance/_guidance.md

Verification: all planned effects verified
Recovery data: removed
```

Expanded explains reference and verification evidence. Compact keeps every
rewrite and effect, and every exact diff in preview mode. Category moves must
identify the category and its affected paths, not reuse a single-file claim.

### 17. route remove

```text
The routed file was removed.
Workspace: /work/demo
Selected by: current directory
Target: guidance/review
Status: complete

Removed and verified:
  .agents/guidance/review.md
Detached references: 1; reference scan complete
  .agents/workflows/release.md:18:4
    <exact detachment effect>
Updated generated navigation:
  .agents/guidance/_guidance.md

Recovery data: removed
```

Expanded explains the accepted detachment decision and verification. Both views
show preserved/protected paths and actual partial effects when they exist.

## Extensions

### 18. extension list

```text
Installed Extensions could not be determined.
Workspace: /work/demo
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

This compact example uses the package/availability facts observed during analysis,
with an illustrative workspace. Expanded adds package/dependency counts and
managed-state facts when available. It must not present unknown installation as
an empty installed set.

### 19. extension inspect

```text
Extension: <package ID>
Workspace: /work/demo
Selected by: current directory
Source: embedded catalogue
Status: requires attention

Installed: yes; recorded state can be read
Available: yes; version <selected version>
Comparison: installed baseline, workspace, selected package
Dependencies: <declared IDs and resolved package count>
Paths: <current count>, <changed count>, <unavailable count>

Differences:
  <affected path>
    Installed baseline: <fact>
    Workspace: <fact>
    Selected package: <fact>

Generated navigation: <actual derived-content observation>
Next: <supported next action, if present>
```

Compact retains key state, comparison mode, counts and required findings.
Expanded includes complete path comparisons and dependency/source explanations.
No update command is invented merely because a comparison differs.

### 20. extension create

```text
The Extension package was created.
Package: review-tools
Catalogue: /work/demo/extensions
Destination: /work/demo/extensions/review-tools
Status: complete

Manifest
  Name: Review tools
  Description: Guidance for reviewing changes
  Version: 0.1.0
  Dependencies: none
Created and verified:
  <manifest path>
  <every scaffold path>

Workspace installation: unchanged
```

Compact keeps metadata, IDs and scaffold effects. Expanded explains generated
files and verification. Creating package source is not installing the package.

### 21. extension install

```text
The selected Extensions were installed.
Workspace: /work/demo
Selected by: current directory
Source: embedded catalogue
Mode: apply; normal; automatic
Status: complete

Selected:
  <requested package ID>
Required dependencies:
  <dependency IDs, in the result's order>
Affected paths:
  <path>: <verified file/region effect>
Generated navigation:
  <actual generated effects>
Permissions: <actual decisions>

Lifecycle record: updated
Verification: all planned effects verified
Recovery data: removed
```

Expanded groups package/path explanations, Framework anchor and source/footprint
facts instead of following JSON property order. Compact keeps all required
selection, permission, preserved-content and effect facts.

### 22. extension update

```text
The selected Extensions were updated. Local changes were kept.
Workspace: /work/demo
Selected by: current directory
Packages: <selected package IDs>
Source: embedded catalogue
Mode: apply; normal; automatic; prune disabled
Status: requires attention

Updated and verified:
  <path>: <actual effect>
Kept local changes:
  <path>: <actual divergence>
Shared paths:
  <path>: <actual shared-owner outcome, when present>
Generated navigation:
  <actual generated effects>

Lifecycle record: updated
Recovery data: removed
```

Expanded adds per-path baseline/current/intended facts and dependency context.
Compact retains remaining divergence and force/prune effects. A recovery failure
must remain visible even when the package changes succeeded.

### 23. extension remove

```text
The selected Extension ownership was removed.
Workspace: /work/demo
Selected by: current directory
Packages: <selected package IDs>
Mode: apply; automatic
Status: complete

Deleted and verified:
  <path>
Kept as unmanaged files:
  <path>: <accepted keep decision>
Still managed by another package:
  <path>: <remaining owner IDs>
Dependencies left installed: <IDs or none>
Generated navigation: <actual effects>

Lifecycle record: updated
Source package files: unchanged
Recovery data: removed
```

Only print categories supported by the actual result. Expanded explains ownership
and keep/delete decisions. A retained dependent can block removal entirely; use
a blocked outcome with that exact dependent, not this applied layout.

## Libraries

### 24. library list

```text
Registered Libraries: 1
Workspace: /work/demo
Selected by: current directory
Record: .agents/open-forge.libraries.json; complete
Source inventory: not scanned
Status: complete

team-knowledge
  Source: shared/team-knowledge; available
  Destination: <recorded destination root>
  Registered links: 2 current, 0 missing, 0 changed, 0 blocked, 0 unavailable
```

Compact uses one short row per Library plus shared context. Expanded adds the
registered-link observations. Neither view fetches an inventory that List does
not inspect. For a missing record, say `No Libraries are registered` and retain
the missing-record and unscanned-inventory facts.

### 25. library inspect

```text
Library: team-knowledge
Workspace: /work/demo
Selected by: current directory
Record: .agents/open-forge.libraries.json; complete
Source: shared/team-knowledge; available
Inventory: complete
Status: complete

Paths: 1 eligible, 1 registered
  .agents/directives/review.md
    Source ID: directives/review
    Registered: yes
    Observed link: current
    Comparison: current
```

Compact summarizes comparison counts and retains significant differences and
required findings. Expanded includes every eligible/registered path and its link
comparison facts. Availability and link state remain separate concepts.

### 26. library attach

```text
The Library would be attached.
Workspace: /work/demo
Selected by: current directory
Library: team-knowledge
Source: shared/team-knowledge
Destination: <selected destination root>
Inventory: complete
Status: complete

Would create links:
  <destination-path> -> <source-path>
Would create the Library record:
  .agents/open-forge.libraries.json
Generated navigation:
  <actual planned effects>

No files changed (--dry-run).
```

Compact retains every mapping/effect or blocker. Expanded explains eligibility,
excluded/incomplete inventory, collisions and available verification/recovery
facts. Use `update` for an existing record. Links must never be described as copies.

### 27. library sync

```text
The Library would be synchronized.
Workspace: /work/demo
Selected by: current directory
Library: team-knowledge
Source: shared/team-knowledge
Destination: <recorded destination root>
Inventory: complete
Status: complete

Would create links:
  <new destination> -> <source path>
Would retire registered links:
  <retired destination>
Would update the Library record:
  .agents/open-forge.libraries.json
Generated navigation:
  <actual planned effects>

No files changed (--dry-run).
```

Expanded explains current/registered comparisons and exact target checks. Both
views keep local occupants and blockers visible. Retiring a link is not deleting
its source content. A verified no-op says `The Library is already synchronized`.

### 28. library detach

```text
The Library would be detached.
Workspace: /work/demo
Selected by: current directory
Library: team-knowledge
Mode: preview; does not require reading the source inventory
Status: complete

Would remove registered links:
  <exact destination path>
Already absent:
  <safely absent registered path, if present>
Library record:
  <actual planned record effect>
Generated navigation:
  <actual planned effects>

No files changed (--dry-run).
```

Expanded explains exact target and no-follow checks, dangling/missing/changed
states and recovery. Keep changed occupants and blockers visible. Detaching
removes the observed registered links, not external source content.

## Shared Unsuccessful Outcomes

These examples apply only where supported by the command's actual result.
Do not use one generic sentence to replace the direct cause.

```text
The route could not be moved.
Workspace: /work/demo
Selected by: current directory
Status: blocked
Destination: .agents/guidance/code-review.md

A file already occupies the destination.
No files changed.
Next: <supported correction from the actual result>
```

```text
The update did not finish.
Workspace: /work/demo
Selected by: current directory
Status: failed

Completed and verified:
  <path and actual verified effect>
Completion unknown:
  <path and observed failure>
Recovery data: <observed path and condition>
Next: <actual required recovery action>
```

```text
The search could not check every selected source.
Workspace: /work/demo
Selected by: current directory
Status: incomplete

Could not read: <source path>
Reason: <actual cause>
Matches from the sources checked:
  <safe matches, if any>
```

Invalid input identifies the exact argument/option and accepted form. Interrupted
operations retain their actual completed, unstarted and unknown effects. Empty
results, no-ops and unavailable observations each get their own accurate wording.

## Approval And Checkpoint

Review the per-command layouts and compact/expanded choices together. After user
approval, update affected Interfaces and freeze exact snapshots for each bounded
implementation set. Do not treat the gallery's illustrative counts, placeholders,
source content or diffs as executable fixtures or accepted machine output.
The analysis continues to preserve diagnostic kinds, statuses, JSON and effects;
manual-parsing simplifications remain backlog and reusable guidance is last.
