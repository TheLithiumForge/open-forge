---
open-forge:
  description: "Disposition of every supplied flow and scenario"
  tags: [Memory, Document, CLI, Review]
---

# Experience Assessment

Reviewed all 438 supplied scenario bodies. **360 added unchanged in outcome; 39 improved and added; 38 deferred; 1 not added.** Editorial normalization, native heading anchors, and replacement of external proposed output links apply throughout. This is selection evidence, not execution coverage.

Source digest (ordered relative filenames and normalized source text): `527fc3e8c0d8e9aaefe95ff7d4a184b483b662c2f862088adb21f429ddbd714d`.

## Flow Selection

All 24 supplied flows were retained. Eleven were improved: F03, F05, F09, F10, F14, F15, F17, F18, F21, F22, and F23. F25 (overwrite customization) and F26 (optional package selection) are recommended additions. See the [flow list](flows/_flows.md) for the complete short definitions.

## Improved And Added

| Scenario | Situation | Disposition reason |
| --- | --- | --- |
| [C01-01](scenarios/commands/c01-status.md#c01-01) | Not installed | The starting state says uninstalled W0 but the fixture label says W1. |
| [C01-09](scenarios/commands/c01-status.md#c01-09) | No ownership record | A forced healthy headline can imply ownership-dependent checks succeeded when they are unknown. |
| [C02-03](scenarios/commands/c02-doctor.md#c02-03) | Warnings only | A default warning count without affected sources or a useful action leaves the person unable to act. |
| [C05-05](scenarios/commands/c05-index.md#c05-05) | Folder operand | An ordinary folder with one known entrypoint is a clear user selection. |
| [C05-07](scenarios/commands/c05-index.md#c05-07) | Blocked malformed leaf | A malformed leaf should not stop independent lists whose inputs and generated boundaries are complete. |
| [C05-08](scenarios/commands/c05-index.md#c05-08) | Incomplete unreadable child | An unreadable child limits its dependent list, not all unrelated indexing. |
| [C07-04](scenarios/commands/c07-cleanup.md#c07-04) | Damaged bundle | One damaged candidate need not veto deletion of independently verified cleanup items. |
| [C10-09](scenarios/commands/c10-references.md#c10-09) | Include with out only | The selected outgoing source remains unambiguous despite an irrelevant incoming filter. |
| [C11-09](scenarios/commands/c11-route-list.md#c11-09) | Metadata missing | Optional missing description or tags must not be described as an unreadable or invalid route. |
| [C14-05](scenarios/commands/c14-route-create.md#c14-05) | Missing description | Omitted optional description or tags should not block a deterministic safe creation. |
| [C14-06](scenarios/commands/c14-route-create.md#c14-06) | Missing tag | Omitted optional description or tags should not block a deterministic safe creation. |
| [C14-07](scenarios/commands/c14-route-create.md#c14-07) | Missing both | Omitted optional description or tags should not block a deterministic safe creation. |
| [C14-09](scenarios/commands/c14-route-create.md#c14-09) | Parent missing | An exact descendant under a recognized root need not require a separate parent-init ritual when all missing intermediate scopes are deterministic. |
| [C15-03](scenarios/commands/c15-route-update.md#c15-03) | Responsibility removed | Removing responsibility should not impose unrelated mandatory metadata requirements. |
| [C15-08](scenarios/commands/c15-route-update.md#c15-08) | No patch | An exact source with no requested patch has a clear harmless no-op meaning. |
| [C16-07](scenarios/commands/c16-route-move.md#c16-07) | Self move | Moving a source to its current identity is a clear converged state. |
| [C17-05](scenarios/commands/c17-route-remove.md#c17-05) | Source not found | A verified already-absent removal target should be a clear no-op. |
| [C17-13](scenarios/commands/c17-route-remove.md#c17-13) | Ownership cannot establish an unmanaged subject | The removal case accidentally executes route move. |
| [C19-06](scenarios/commands/c19-extension-inspect.md#c19-06) | Dependency cycle | A dependency cycle prevents a complete dependency closure but should not erase readable package facts. |
| [C19-08](scenarios/commands/c19-extension-inspect.md#c19-08) | No ownership record | Available-package information remains useful even when installed ownership is unknown. |
| [C23-S05](scenarios/commands/c23-extension-remove.md#c23-s05) | Outcome 05 | Removal must not require the original package source; the generic required-input recipe leaves that distinction unclear. |
| [C26-06](scenarios/commands/c26-library-attach.md#c26-06) | Duplicate id | An exact repeated attach differs from attempting to remap an existing ID. |
| [C26-10](scenarios/commands/c26-library-attach.md#c26-10) | Links unsupported | A real link-creation failure must report any effects already made rather than promise unproved global rollback. |
| [C27-07](scenarios/commands/c27-library-sync.md#c27-07) | Changed occupant | A changed destination must be preserved while independent safe sync effects can remain useful. |
| [C27-16](scenarios/commands/c27-library-sync.md#c27-16) | A retired registration has no deletable link | Positive absence requires no deletion and should not block releasing an obsolete claim. |
| [C28-04](scenarios/commands/c28-library-detach.md#c28-04) | Unknown id | A known absent registration has a harmless repeat-detach no-op meaning. |
| [C28-05](scenarios/commands/c28-library-detach.md#c28-05) | Registered link gone | An already missing link should not force recreation before detach. |
| [C28-06](scenarios/commands/c28-library-detach.md#c28-06) | Changed occupant | Detach can retain the replacement user file while releasing known registration and removing other exact links. |
| [X02](scenarios/experience.md#x02) | Index a plain hand-authored note | Indexing must not insert unsolicited metadata or require an enrichment ritual. |
| [X03](scenarios/experience.md#x03) | Complete partial metadata without replacing valid authored values | Partial metadata already carries authored meaning and should remain untouched by index. |
| [X04](scenarios/experience.md#x04) | Preview metadata assistance without claiming it happened | Preview should describe useful navigation work, not automatic metadata scaffolding. |
| [X05](scenarios/experience.md#x05) | Do not treat missing, partial, malformed and unreadable metadata as one problem | The distinction is valuable, but metadata assistance and whole-operation blocking contradict the forgiving target. |
| [X07](scenarios/experience.md#x07) | Preserve authored prose around the index-managed list | The preservation case is sound but must not import external output-05 or metadata-assistance policy. |
| [X12](scenarios/experience.md#x12) | Ignore unrelated corrupt old records, but not required safety facts | An inert .log cannot prove useful work survives unrelated routed corruption. |
| [X13](scenarios/experience.md#x13) | Treat an unusable ownership lock as unknown claims, not a universal stop | The case should specify user-visible unknown ownership, not arbitrate conflicts among external proposed tables. |
| [X15](scenarios/experience.md#x15) | Keep a copyable next action complete and relevant | Missing optional description or tags is no longer a required-input failure. |
| [X18](scenarios/experience.md#x18) | Preserve a changed Library destination until the user decides | The current sequence makes backup-and-repair mandatory and assumes both commands must stop globally. |
| [X19](scenarios/experience.md#x19) | Make a second identical operation genuinely a no-op | The blanket exclusion of remove/detach conflicts with forgiving known-absence no-op targets. |
| [X25](scenarios/experience.md#x25) | Add warning detail without repeating the same instruction | Historical external captures should not become approved copy or substitute for a real fixture. |

## Deferred, Not Added Yet

| Scenario | Situation | Disposition reason |
| --- | --- | --- |
| [C01-S09](scenarios/commands/c01-status.md#c01-s09) | Outcome 09 | No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet. |
| [C01-S10](scenarios/commands/c01-status.md#c01-s10) | Outcome 10 | Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage. |
| [C02-S08](scenarios/commands/c02-doctor.md#c02-s08) | Outcome 08 | No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet. |
| [C02-S09](scenarios/commands/c02-doctor.md#c02-s09) | Outcome 09 | Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage. |
| [C03-S05](scenarios/commands/c03-install.md#c03-s05) | Outcome 05 | Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault. |
| [C04-S06](scenarios/commands/c04-update.md#c04-s06) | Outcome 06 | Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault. |
| [C05-S04](scenarios/commands/c05-index.md#c05-s04) | Outcome 04 | Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault. |
| [C06-09](scenarios/commands/c06-repair.md#c06-09) | Library recovery step | The rare Library recovery step has no concrete supported fixture or selected state; a real user recovery journey must identify it. |
| [C06-S07](scenarios/commands/c06-repair.md#c06-s07) | Outcome 07 | Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault. |
| [C07-S08](scenarios/commands/c07-cleanup.md#c07-s08) | Outcome 08 | A catalogue race without a reproducible cooperating-process boundary is an artificial state permutation. |
| [C08-S07](scenarios/commands/c08-context.md#c08-s07) | Outcome 07 | No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet. |
| [C08-S08](scenarios/commands/c08-context.md#c08-s08) | Outcome 08 | Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage. |
| [C09-S07](scenarios/commands/c09-find.md#c09-s07) | Outcome 07 | No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet. |
| [C09-S08](scenarios/commands/c09-find.md#c09-s08) | Outcome 08 | Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage. |
| [C10-S07](scenarios/commands/c10-references.md#c10-s07) | Outcome 07 | No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet. |
| [C10-S08](scenarios/commands/c10-references.md#c10-s08) | Outcome 08 | Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage. |
| [C11-S07](scenarios/commands/c11-route-list.md#c11-s07) | Outcome 07 | No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet. |
| [C11-S08](scenarios/commands/c11-route-list.md#c11-s08) | Outcome 08 | Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage. |
| [C12-S06](scenarios/commands/c12-route-inspect.md#c12-s06) | Outcome 06 | No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet. |
| [C12-S07](scenarios/commands/c12-route-inspect.md#c12-s07) | Outcome 07 | Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage. |
| [C13-S04](scenarios/commands/c13-route-init.md#c13-s04) | Outcome 04 | Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault. |
| [C14-S04](scenarios/commands/c14-route-create.md#c14-s04) | Outcome 04 | Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault. |
| [C15-S05](scenarios/commands/c15-route-update.md#c15-s05) | Outcome 05 | Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault. |
| [C16-S04](scenarios/commands/c16-route-move.md#c16-s04) | Outcome 04 | Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault. |
| [C17-07](scenarios/commands/c17-route-remove.md#c17-07) | Unsafe link detach | Protecting link meaning matters, but the case explicitly lacks the offending Markdown form; a capture does not establish the fixture. |
| [C17-S04](scenarios/commands/c17-route-remove.md#c17-s04) | Outcome 04 | Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault. |
| [C18-S07](scenarios/commands/c18-extension-list.md#c18-s07) | Outcome 07 | No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet. |
| [C18-S08](scenarios/commands/c18-extension-list.md#c18-s08) | Outcome 08 | Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage. |
| [C19-S08](scenarios/commands/c19-extension-inspect.md#c19-s08) | Outcome 08 | No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet. |
| [C19-S09](scenarios/commands/c19-extension-inspect.md#c19-s09) | Outcome 09 | Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage. |
| [C24-S08](scenarios/commands/c24-library-list.md#c24-s08) | Outcome 08 | No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet. |
| [C24-S09](scenarios/commands/c24-library-list.md#c24-s09) | Outcome 09 | Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage. |
| [C25-S08](scenarios/commands/c25-library-inspect.md#c25-s08) | Outcome 08 | No concrete unexpected failure mechanism or reproducible fixture is supplied; a generic outcome row adds no executable user scenario yet. |
| [C25-S09](scenarios/commands/c25-library-inspect.md#c25-s09) | Outcome 09 | Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage. |
| [C26-S04](scenarios/commands/c26-library-attach.md#c26-s04) | Outcome 04 | Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault. |
| [C26-S09](scenarios/commands/c26-library-attach.md#c26-s09) | Outcome 09 | Repeated cancellation outcome without a concrete signal boundary; retain X09 as the shared invariant until a distinct fixture earns coverage. |
| [C27-S06](scenarios/commands/c27-library-sync.md#c27-s06) | Outcome 06 | Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault. |
| [C28-S05](scenarios/commands/c28-library-detach.md#c28-s05) | Outcome 05 | Retained recovery matters, but this generic permutation has no demonstrated recovery-requiring plan or reliable post-success cleanup fault. |

## Not Added

| Scenario | Situation | Disposition reason |
| --- | --- | --- |
| [C18-16](scenarios/commands/c18-extension-list.md#c18-16) | Installed files unavailable | This abstract unavailable-comparison umbrella adds no distinct state beyond C18-09, C18-15 and X28; retain those concrete source/target coverage cases. |

## Added With The Original User Outcome

| Scenario | Situation | Disposition reason |
| --- | --- | --- |
| [C01-02](scenarios/commands/c01-status.md#c01-02) | Healthy | Worth retaining as an independently checked user outcome: Say the Framework is installed and current; show measured startup cost, not a diagnostic dump. |
| [C01-03](scenarios/commands/c01-status.md#c01-03) | Healthy with extension | Worth retaining as an independently checked user outcome: Report a healthy installation and the actual installed Extension context, without treating the Extension as drift. |
| [C01-04](scenarios/commands/c01-status.md#c01-04) | Changed managed file | Worth retaining as an independently checked user outcome: Identify the file that differs from the current selected source. |
| [C01-05](scenarios/commands/c01-status.md#c01-05) | Missing managed file | Worth retaining as an independently checked user outcome: Name the missing file and explain the consequence. |
| [C01-06](scenarios/commands/c01-status.md#c01-06) | Stale entries | Worth retaining as an independently checked user outcome: Identify the stale navigation and the applicable indexing action. |
| [C01-07](scenarios/commands/c01-status.md#c01-07) | Recovery bundle present | Worth retaining as an independently checked user outcome: Name the leftover recovery item and the applicable cleanup action. |
| [C01-08](scenarios/commands/c01-status.md#c01-08) | Library link missing | Worth retaining as an independently checked user outcome: Identify the missing Library link and the specific Library sync action. |
| [C01-10](scenarios/commands/c01-status.md#c01-10) | Unreadable entry file | Worth retaining as an independently checked user outcome: Say which checks could not finish and which source could not be read. |
| [C01-11](scenarios/commands/c01-status.md#c01-11) | Blocked workspace | Worth retaining as an independently checked user outcome: Name the selected missing workspace and stop. |
| [C01-12](scenarios/commands/c01-status.md#c01-12) | Invalid input | Worth retaining as an independently checked user outcome: Explain the invalid value before any workspace work. |
| [C01-S06](scenarios/commands/c01-status.md#c01-s06) | Outcome 06 | Worth retaining as an independently checked user outcome: Report both the known attention item and incomplete checks. |
| [C02-01](scenarios/commands/c02-doctor.md#c02-01) | Healthy | Worth retaining as an independently checked user outcome: Say no problems were found. |
| [C02-02](scenarios/commands/c02-doctor.md#c02-02) | Info only | Worth retaining as an independently checked user outcome: Distinguish an informational observation from an error. |
| [C02-04](scenarios/commands/c02-doctor.md#c02-04) | Error and warnings | Worth retaining as an independently checked user outcome: Show the error's subject and action, with correct error and warning counts. |
| [C02-05](scenarios/commands/c02-doctor.md#c02-05) | Incomplete | Worth retaining as an independently checked user outcome: Identify checks that did not finish and retain findings from checks that did. |
| [C02-06](scenarios/commands/c02-doctor.md#c02-06) | Blocked workspace | Worth retaining as an independently checked user outcome: Explain why this workspace cannot be checked, without producing findings for another one. |
| [C02-07](scenarios/commands/c02-doctor.md#c02-07) | Invalid input | Worth retaining as an independently checked user outcome: Explain the flag error and the valid help path. |
| [C02-08](scenarios/commands/c02-doctor.md#c02-08) | Changed extension file | Worth retaining as an independently checked user outcome: Attribute the difference to the correct Extension and point to its inspection or update operation. |
| [C02-09](scenarios/commands/c02-doctor.md#c02-09) | Stale entries | Worth retaining as an independently checked user outcome: Diagnose stale navigation, not a fabricated incomplete installation or update. |
| [C02-10](scenarios/commands/c02-doctor.md#c02-10) | Library drift | Worth retaining as an independently checked user outcome: Explain the missing Library link and its targeted action without silently recreating it. |
| [C02-11](scenarios/commands/c02-doctor.md#c02-11) | Recovery bundle | Worth retaining as an independently checked user outcome: Describe the item and its consequence, with cleanup preview rather than automatic deletion. |
| [C03-01](scenarios/commands/c03-install.md#c03-01) | Fresh directory | Worth retaining as an independently checked user outcome: Install the Framework, identify created host sections and summarize the managed content with truthful counts. |
| [C03-02](scenarios/commands/c03-install.md#c03-02) | Fresh directory dry run | Worth retaining as an independently checked user outcome: Show what would be installed and finish with No files were changed. |
| [C03-03](scenarios/commands/c03-install.md#c03-03) | Already installed | Worth retaining as an independently checked user outcome: Say Open Forge is already installed and current, with nothing to do. |
| [C03-04](scenarios/commands/c03-install.md#c03-04) | Existing agents md | Worth retaining as an independently checked user outcome: Keep the authored instructions and describe the actual host-file treatment. |
| [C03-05](scenarios/commands/c03-install.md#c03-05) | Occupied without force | Worth retaining as an independently checked user outcome: Name the exact conflict before any installation effects and explain the available deliberate choice. |
| [C03-06](scenarios/commands/c03-install.md#c03-06) | Occupied with force | Worth retaining as an independently checked user outcome: Replace only eligible conflicts and report which existing files were replaced. |
| [C03-07](scenarios/commands/c03-install.md#c03-07) | Changed framework file | Worth retaining as an independently checked user outcome: Direct the user to update instead of pretending this is a fresh install conflict. |
| [C03-08](scenarios/commands/c03-install.md#c03-08) | Confirmation unavailable | Worth retaining as an independently checked user outcome: Explain that confirmation is required without waiting forever or printing an unanswered terminal prompt. |
| [C03-09](scenarios/commands/c03-install.md#c03-09) | Recovery store unavailable | Worth retaining as an independently checked user outcome: Explain the required recovery limitation and make no target changes. |
| [C03-10](scenarios/commands/c03-install.md#c03-10) | Write failed partial | Worth retaining as an independently checked user outcome: State that install stopped, distinguish completed and unstarted effects, and name available recovery truthfully. |
| [C03-11](scenarios/commands/c03-install.md#c03-11) | Cancelled | Worth retaining as an independently checked user outcome: Say installation was cancelled and nothing was changed. |
| [C03-12](scenarios/commands/c03-install.md#c03-12) | Invalid input | Worth retaining as an independently checked user outcome: Reject the argument without attempting installation. |
| [C03-S10](scenarios/commands/c03-install.md#c03-s10) | Outcome 10 | Worth retaining as an independently checked user outcome: Explain the exact unsafe boundary and stop before all target effects. |
| [C04-01](scenarios/commands/c04-update.md#c04-01) | Up to date | Worth retaining as an independently checked user outcome: Say the Framework is up to date and there is nothing to do. |
| [C04-02](scenarios/commands/c04-update.md#c04-02) | Changed file replaced | Worth retaining as an independently checked user outcome: Replace the selected changed content and name the replacement. |
| [C04-03](scenarios/commands/c04-update.md#c04-03) | Missing file restored | Worth retaining as an independently checked user outcome: Restore and identify that missing file, without calling it a replacement of existing bytes. |
| [C04-04](scenarios/commands/c04-update.md#c04-04) | Retired kept | Worth retaining as an independently checked user outcome: Keep the retired file by default and explain that it remains from an earlier version. |
| [C04-05](scenarios/commands/c04-update.md#c04-05) | Retired pruned | Worth retaining as an independently checked user outcome: Remove only eligible retired owned content and name it explicitly. |
| [C04-06](scenarios/commands/c04-update.md#c04-06) | Dry run changes | Worth retaining as an independently checked user outcome: Show the concrete proposed effects using future tense and No files were changed. |
| [C04-07](scenarios/commands/c04-update.md#c04-07) | No ownership record | Worth retaining as an independently checked user outcome: Explain that update cannot establish its managed files and changes nothing. |
| [C04-08](scenarios/commands/c04-update.md#c04-08) | Confirmation unavailable | Worth retaining as an independently checked user outcome: Explain the missing confirmation instead of starting or hanging. |
| [C04-09](scenarios/commands/c04-update.md#c04-09) | Write failed partial | Worth retaining as an independently checked user outcome: Report the completed portion, stopped operation and actual recovery location or failure. |
| [C04-10](scenarios/commands/c04-update.md#c04-10) | Cancelled | Worth retaining as an independently checked user outcome: Report cancellation without implying an update happened. |
| [C04-11](scenarios/commands/c04-update.md#c04-11) | Invalid input | Worth retaining as an independently checked user outcome: Reject the argument without selecting an update. |
| [C04-S07](scenarios/commands/c04-update.md#c04-s07) | Outcome 07 | Worth retaining as an independently checked user outcome: Name the unavailable required fact and its consequence. |
| [C04-S09](scenarios/commands/c04-update.md#c04-s09) | Outcome 09 | Worth retaining as an independently checked user outcome: Name the target conflict and preserve the complete selected update. |
| [C05-01](scenarios/commands/c05-index.md#c05-01) | All current | Worth retaining as an independently checked user outcome: Say the checked Entries sections are current and nothing needs doing. |
| [C05-02](scenarios/commands/c05-index.md#c05-02) | One stale | Worth retaining as an independently checked user outcome: Update the one affected list and show the actual changed/checked counts. |
| [C05-03](scenarios/commands/c05-index.md#c05-03) | Dry run one stale | Worth retaining as an independently checked user outcome: Say which Entries section would change, with the correct before/after entry counts and no writes. |
| [C05-04](scenarios/commands/c05-index.md#c05-04) | Explicit source | Worth retaining as an independently checked user outcome: Rebuild exactly the selected source's contract-defined scope and affected direct parent, not unrelated roots. |
| [C05-06](scenarios/commands/c05-index.md#c05-06) | Unknown source | Worth retaining as an independently checked user outcome: Name the unknown reference and do not turn it into an empty successful index. |
| [C05-09](scenarios/commands/c05-index.md#c05-09) | Lock held | Worth retaining as an independently checked user outcome: Explain that another operation is using this workspace without guessing its identity or suggesting deletion of a lock file. |
| [C05-10](scenarios/commands/c05-index.md#c05-10) | Write failed partial | Worth retaining as an independently checked user outcome: Distinguish changed, unchanged, failed and unstarted work; report recovery truthfully. |
| [C05-11](scenarios/commands/c05-index.md#c05-11) | Cancelled | Worth retaining as an independently checked user outcome: Report cancellation and no content changes for this pre-effect case. |
| [C06-01](scenarios/commands/c06-repair.md#c06-01) | Nothing to repair | Worth retaining as an independently checked user outcome: Say nothing needs repair; do not claim a nonzero number of verified fixes. |
| [C06-02](scenarios/commands/c06-repair.md#c06-02) | Automatic two links | Worth retaining as an independently checked user outcome: Apply exactly those two corrections and show source locations and old-to-new destinations. |
| [C06-03](scenarios/commands/c06-repair.md#c06-03) | Automatic nothing safe two guided | Worth retaining as an independently checked user outcome: Apply nothing and explain the two choices still needed. |
| [C06-04](scenarios/commands/c06-repair.md#c06-04) | Dry run automatic | Worth retaining as an independently checked user outcome: Show both proposed relinks without writing them. |
| [C06-05](scenarios/commands/c06-repair.md#c06-05) | Relink one | Worth retaining as an independently checked user outcome: Repair that exact occurrence and no similarly spelled occurrence elsewhere. |
| [C06-06](scenarios/commands/c06-repair.md#c06-06) | Relink invalid | Worth retaining as an independently checked user outcome: Reject the mismatch without changing the link. |
| [C06-07](scenarios/commands/c06-repair.md#c06-07) | Contradictory relinks | Worth retaining as an independently checked user outcome: Explain the contradictory requests before any repair effect. |
| [C06-08](scenarios/commands/c06-repair.md#c06-08) | Non interactive no selection | Worth retaining as an independently checked user outcome: Explain the missing selection and do not start an invisible wizard. |
| [C06-10](scenarios/commands/c06-repair.md#c06-10) | Lock held | Worth retaining as an independently checked user outcome: Explain the active operation and preserve all selected sources. |
| [C06-11](scenarios/commands/c06-repair.md#c06-11) | Write failed partial | Worth retaining as an independently checked user outcome: Say repair stopped after the completed effects and retain precise remaining work. |
| [C06-12](scenarios/commands/c06-repair.md#c06-12) | Cancelled | Worth retaining as an independently checked user outcome: Report cancellation and preserve the source. |
| [C06-13](scenarios/commands/c06-repair.md#c06-13) | Invalid input | Worth retaining as an independently checked user outcome: Explain the missing operand before diagnosis or mutation. |
| [C06-S04](scenarios/commands/c06-repair.md#c06-s04) | Outcome 04 | Worth retaining as an independently checked user outcome: Distinguish the selected safe correction from the unresolved choice. |
| [C06-S05](scenarios/commands/c06-repair.md#c06-s05) | Outcome 05 | Worth retaining as an independently checked user outcome: Distinguish the selected safe correction from the unresolved choice. |
| [C06-S08](scenarios/commands/c06-repair.md#c06-s08) | Outcome 08 | Worth retaining as an independently checked user outcome: Name the unavailable required fact and its consequence. |
| [C07-01](scenarios/commands/c07-cleanup.md#c07-01) | Nothing to remove | Worth retaining as an independently checked user outcome: Say there is nothing to remove. |
| [C07-02](scenarios/commands/c07-cleanup.md#c07-02) | Two bundles one draft | Worth retaining as an independently checked user outcome: List what was removed and use correct bundle/draft counts. |
| [C07-03](scenarios/commands/c07-cleanup.md#c07-03) | Dry run | Worth retaining as an independently checked user outcome: List proposed removals in future tense and finish with No files were changed. |
| [C07-05](scenarios/commands/c07-cleanup.md#c07-05) | Lock held | Worth retaining as an independently checked user outcome: Explain the active workspace operation rather than instructing removal of its lock file. |
| [C07-06](scenarios/commands/c07-cleanup.md#c07-06) | Store unreadable | Worth retaining as an independently checked user outcome: Say the store could not be read completely and nothing was removed, once. |
| [C07-07](scenarios/commands/c07-cleanup.md#c07-07) | Deletion failed partial | Worth retaining as an independently checked user outcome: State how far cleanup got, what remains and what could not be removed. |
| [C07-08](scenarios/commands/c07-cleanup.md#c07-08) | Cancelled partial | Worth retaining as an independently checked user outcome: Report cancellation with the actual removed and remaining items, not Nothing was changed. |
| [C07-09](scenarios/commands/c07-cleanup.md#c07-09) | Invalid input | Worth retaining as an independently checked user outcome: Reject the input without enumerating or removing recovery items. |
| [C08-01](scenarios/commands/c08-context.md#c08-01) | Startup | Worth retaining as an independently checked user outcome: Emit startup content in the specified order, with source delimiters and no redundant success banner. |
| [C08-02](scenarios/commands/c08-context.md#c08-02) | One source | Worth retaining as an independently checked user outcome: Include the selected source and the contract-required context around it, without pulling in unrelated branches. |
| [C08-03](scenarios/commands/c08-context.md#c08-03) | Additions only | Worth retaining as an independently checked user outcome: Explain that there is no additional context instead of printing a duplicate startup dump. |
| [C08-04](scenarios/commands/c08-context.md#c08-04) | Paths | Worth retaining as an independently checked user outcome: Print the requested paths in contract order, one per line, without Markdown code fences or authored body text. |
| [C08-05](scenarios/commands/c08-context.md#c08-05) | Headings | Worth retaining as an independently checked user outcome: Return parsed headings, not arbitrary lines starting with a hash inside opaque content. |
| [C08-06](scenarios/commands/c08-context.md#c08-06) | Section | Worth retaining as an independently checked user outcome: Return the requested section's defined extent without leaking the next peer section. |
| [C08-07](scenarios/commands/c08-context.md#c08-07) | Frontmatter missing host | Worth retaining as an independently checked user outcome: Treat the host's absent frontmatter as normal. |
| [C08-08](scenarios/commands/c08-context.md#c08-08) | Section missing | Worth retaining as an independently checked user outcome: Say the requested section is absent and retain any independently available requested content. |
| [C08-09](scenarios/commands/c08-context.md#c08-09) | Follow links | Worth retaining as an independently checked user outcome: Follow only the permitted depth and report source framing without unbounded recursive expansion. |
| [C08-10](scenarios/commands/c08-context.md#c08-10) | Broken followed link | Worth retaining as an independently checked user outcome: Keep readable selected content and explain the failed followed link and that it was not followed. |
| [C08-11](scenarios/commands/c08-context.md#c08-11) | Unknown source | Worth retaining as an independently checked user outcome: Name the unresolved reference and reject the request without inventing a scope. |
| [C08-12](scenarios/commands/c08-context.md#c08-12) | Ambiguous source | Worth retaining as an independently checked user outcome: Explain the ambiguity and give exact resolvable alternatives. |
| [C08-13](scenarios/commands/c08-context.md#c08-13) | Unreadable source | Worth retaining as an independently checked user outcome: Preserve safe available content and explicitly qualify the missing part. |
| [C08-14](scenarios/commands/c08-context.md#c08-14) | Invalid content | Worth retaining as an independently checked user outcome: Explain the unsupported content value without reading or returning a misleading partial result. |
| [C09-01](scenarios/commands/c09-find.md#c09-01) | Bare inventory | Worth retaining as an independently checked user outcome: List the eligible inventory in stable order; do not silently apply a tag or route-only filter. |
| [C09-02](scenarios/commands/c09-find.md#c09-02) | One tag | Worth retaining as an independently checked user outcome: Return the two genuine tag matches with useful selection information. |
| [C09-03](scenarios/commands/c09-find.md#c09-03) | Two tags all | Worth retaining as an independently checked user outcome: Use the default all requirement and return only the intersection. |
| [C09-04](scenarios/commands/c09-find.md#c09-04) | Heading | Worth retaining as an independently checked user outcome: Match the actual structural heading, not the fenced imitation. |
| [C09-05](scenarios/commands/c09-find.md#c09-05) | No matches | Worth retaining as an independently checked user outcome: Say no sources matched this request. |
| [C09-06](scenarios/commands/c09-find.md#c09-06) | With content headings | Worth retaining as an independently checked user outcome: Preserve the matching source set and add requested headings without changing the query. |
| [C09-07](scenarios/commands/c09-find.md#c09-07) | Include selector | Worth retaining as an independently checked user outcome: Limit the searched universe to the declared guidance subtree, including eligible unrouted Markdown there. |
| [C09-08](scenarios/commands/c09-find.md#c09-08) | Ambiguous selector | Worth retaining as an independently checked user outcome: Report the selector ambiguity without searching an arbitrary candidate. |
| [C09-09](scenarios/commands/c09-find.md#c09-09) | Unreadable source | Worth retaining as an independently checked user outcome: Return known safe matches but clearly say search was incomplete. |
| [C09-10](scenarios/commands/c09-find.md#c09-10) | Section missing | Worth retaining as an independently checked user outcome: Distinguish missing requested regions from readable non-matches and retain valid matches. |
| [C09-11](scenarios/commands/c09-find.md#c09-11) | Invalid selector | Worth retaining as an independently checked user outcome: Name the invalid selector rather than silently searching everything or nothing. |
| [C09-12](scenarios/commands/c09-find.md#c09-12) | Invalid require | Worth retaining as an independently checked user outcome: Explain the invalid require value and the accepted alternatives. |
| [C10-01](scenarios/commands/c10-references.md#c10-01) | Links both | Worth retaining as an independently checked user outcome: Separate incoming and outgoing authored links, with actionable source locations and no generated-navigation noise. |
| [C10-02](scenarios/commands/c10-references.md#c10-02) | No authored links | Worth retaining as an independently checked user outcome: Say no authored incoming or outgoing links exist. |
| [C10-03](scenarios/commands/c10-references.md#c10-03) | Out only | Worth retaining as an independently checked user outcome: Return only outgoing links without pretending incoming links were scanned and found absent. |
| [C10-04](scenarios/commands/c10-references.md#c10-04) | In only with include | Worth retaining as an independently checked user outcome: Search the explicit incoming universe and show only its matching occurrences. |
| [C10-05](scenarios/commands/c10-references.md#c10-05) | Broken outgoing | Worth retaining as an independently checked user outcome: Show the exact broken destination and its location without guessing a replacement. |
| [C10-06](scenarios/commands/c10-references.md#c10-06) | External outgoing | Worth retaining as an independently checked user outcome: Show the external link as not checked, not proven valid and not automatically broken. |
| [C10-07](scenarios/commands/c10-references.md#c10-07) | Unknown source | Worth retaining as an independently checked user outcome: Reject the unknown source, not a successful empty link inventory. |
| [C10-08](scenarios/commands/c10-references.md#c10-08) | Invalid direction | Worth retaining as an independently checked user outcome: Explain the invalid direction and leave source content alone. |
| [C10-10](scenarios/commands/c10-references.md#c10-10) | Ambiguous source | Worth retaining as an independently checked user outcome: Show exact alternatives and stop rather than choosing one. |
| [C10-11](scenarios/commands/c10-references.md#c10-11) | Unreadable source | Worth retaining as an independently checked user outcome: Retain the known link and explain that the unreadable candidate's links were not counted. |
| [C11-01](scenarios/commands/c11-route-list.md#c11-01) | Roots depth 1 | Worth retaining as an independently checked user outcome: Show the default depth-one tree with usable source IDs and only truthful hidden-child hints. |
| [C11-02](scenarios/commands/c11-route-list.md#c11-02) | Subtree | Worth retaining as an independently checked user outcome: List the selected subtree without blending sibling roots into it. |
| [C11-03](scenarios/commands/c11-route-list.md#c11-03) | Depth all | Worth retaining as an independently checked user outcome: Show the full selected tree without an arbitrary display cap. |
| [C11-04](scenarios/commands/c11-route-list.md#c11-04) | Depth 0 | Worth retaining as an independently checked user outcome: Treat zero as a valid depth and show only the contract-defined subject level. |
| [C11-05](scenarios/commands/c11-route-list.md#c11-05) | Empty subtree | Worth retaining as an independently checked user outcome: Explain that the selected scope has no child routes, not that its reference is invalid. |
| [C11-06](scenarios/commands/c11-route-list.md#c11-06) | Unknown source | Worth retaining as an independently checked user outcome: Name the unknown reference and avoid an empty-tree success. |
| [C11-07](scenarios/commands/c11-route-list.md#c11-07) | Invalid depth | Worth retaining as an independently checked user outcome: Explain the invalid depth and accepted forms. |
| [C11-08](scenarios/commands/c11-route-list.md#c11-08) | Ambiguous source | Worth retaining as an independently checked user outcome: Explain the ambiguity instead of combining or selecting trees arbitrarily. |
| [C11-10](scenarios/commands/c11-route-list.md#c11-10) | Unreadable entrypoint | Worth retaining as an independently checked user outcome: Show safe available routes and name the branch whose inspection could not finish. |
| [C11-11](scenarios/commands/c11-route-list.md#c11-11) | Loader malformed | Worth retaining as an independently checked user outcome: Explain the loader problem and avoid inventing a usable root graph. |
| [C11-12](scenarios/commands/c11-route-list.md#c11-12) | Loader cannot establish root declarations | Worth retaining as an independently checked user outcome: Report incomplete root discovery; do not invent the usual standard roots or call the workspace empty. |
| [C12-01](scenarios/commands/c12-route-inspect.md#c12-01) | Entrypoint | Worth retaining as an independently checked user outcome: Explain where the scope belongs, when it is read and the declared context measurements. |
| [C12-02](scenarios/commands/c12-route-inspect.md#c12-02) | Routed file | Worth retaining as an independently checked user outcome: Describe that file's identity and inherited context, not an imaginary child scope. |
| [C12-03](scenarios/commands/c12-route-inspect.md#c12-03) | Load now child | Worth retaining as an independently checked user outcome: Explain the parent's loading condition and the child's resulting automatic inclusion. |
| [C12-04](scenarios/commands/c12-route-inspect.md#c12-04) | Keep in mind | Worth retaining as an independently checked user outcome: Explain continuity and refresh behavior without turning the tag into stronger authority. |
| [C12-05](scenarios/commands/c12-route-inspect.md#c12-05) | Overwrite pair | Worth retaining as an independently checked user outcome: Describe one logical source with both layers and their real reading behavior. |
| [C12-06](scenarios/commands/c12-route-inspect.md#c12-06) | Compatibility entrypoint | Worth retaining as an independently checked user outcome: Resolve it once and explain the same scope without demanding a rename. |
| [C12-07](scenarios/commands/c12-route-inspect.md#c12-07) | Not routed file | Worth retaining as an independently checked user outcome: Describe that it is not currently routed, without inventing a route or treating physical existence as exposure. |
| [C12-08](scenarios/commands/c12-route-inspect.md#c12-08) | Id not unique exact path | Worth retaining as an independently checked user outcome: Inspect the exact file and explain its non-unique automatic ID without blocking a resolved request. |
| [C12-09](scenarios/commands/c12-route-inspect.md#c12-09) | Ambiguous id prompt | Worth retaining as an independently checked user outcome: Present clear alternatives, honor the choice and qualify the shared ID. |
| [C12-10](scenarios/commands/c12-route-inspect.md#c12-10) | Unknown source | Worth retaining as an independently checked user outcome: Say the source cannot be found, not that it has zero content. |
| [C12-11](scenarios/commands/c12-route-inspect.md#c12-11) | Loader subject | Worth retaining as an independently checked user outcome: Explain that this command does not inspect the loader as an ordinary route. |
| [C12-12](scenarios/commands/c12-route-inspect.md#c12-12) | Unreadable source | Worth retaining as an independently checked user outcome: Keep known identity facts and label unavailable measurements and the unreadable layer. |
| [C12-13](scenarios/commands/c12-route-inspect.md#c12-13) | Orphan overwrite | Worth retaining as an independently checked user outcome: Explain the missing base relationship and do not treat the overwrite as an independent source. |
| [C13-01](scenarios/commands/c13-route-init.md#c13-01) | New chain | Worth retaining as an independently checked user outcome: Create the missing entrypoints and navigation in the requested chain, clearly marking any authoring still needed. |
| [C13-02](scenarios/commands/c13-route-init.md#c13-02) | Already initialized | Worth retaining as an independently checked user outcome: Say the route is already initialized and nothing needs changing. |
| [C13-03](scenarios/commands/c13-route-init.md#c13-03) | Dry run | Worth retaining as an independently checked user outcome: Show the entrypoints and parent navigation that would be created or updated, with no writes. |
| [C13-04](scenarios/commands/c13-route-init.md#c13-04) | Framework scaffold | Worth retaining as an independently checked user outcome: Create only the requested scoped Framework scaffold and explain what still needs authoring. |
| [C13-05](scenarios/commands/c13-route-init.md#c13-05) | Explicit metadata | Worth retaining as an independently checked user outcome: Apply supplied metadata to the final target, not as misleading copied descriptions of every ancestor. |
| [C13-06](scenarios/commands/c13-route-init.md#c13-06) | Invalid target | Worth retaining as an independently checked user outcome: Reject the target without normalizing it into a different authorized location. |
| [C13-07](scenarios/commands/c13-route-init.md#c13-07) | Invalid metadata | Worth retaining as an independently checked user outcome: Explain the invalid metadata without creating a partial route chain. |
| [C13-08](scenarios/commands/c13-route-init.md#c13-08) | Framework not installed | Worth retaining as an independently checked user outcome: Explain that Framework installation is required; do not silently install it. |
| [C13-09](scenarios/commands/c13-route-init.md#c13-09) | Lock held | Worth retaining as an independently checked user outcome: Explain that the workspace is in use and preserve the chain. |
| [C13-10](scenarios/commands/c13-route-init.md#c13-10) | Write failed partial | Worth retaining as an independently checked user outcome: Report completed and unfinished parts accurately, with recovery where it exists. |
| [C13-11](scenarios/commands/c13-route-init.md#c13-11) | Cancelled | Worth retaining as an independently checked user outcome: Report cancellation without claiming the chain was created. |
| [C13-S05](scenarios/commands/c13-route-init.md#c13-s05) | Outcome 05 | Worth retaining as an independently checked user outcome: Name the unavailable required fact and its consequence. |
| [C14-01](scenarios/commands/c14-route-create.md#c14-01) | Created | Worth retaining as an independently checked user outcome: Create the new file with supplied metadata and update only its bounded navigation. |
| [C14-02](scenarios/commands/c14-route-create.md#c14-02) | Created from template | Worth retaining as an independently checked user outcome: Copy the Template's body as independent starting content and use the destination's supplied metadata. |
| [C14-03](scenarios/commands/c14-route-create.md#c14-03) | Dry run | Worth retaining as an independently checked user outcome: Show the proposed file and parent-list change without creating either. |
| [C14-04](scenarios/commands/c14-route-create.md#c14-04) | Already matching | Worth retaining as an independently checked user outcome: Say the file already matches and there is nothing to do. |
| [C14-08](scenarios/commands/c14-route-create.md#c14-08) | Invalid target | Worth retaining as an independently checked user outcome: Explain the invalid destination and do not reinterpret it as a valid source. |
| [C14-10](scenarios/commands/c14-route-create.md#c14-10) | Exists with different content | Worth retaining as an independently checked user outcome: Refuse replacement and identify the existing file. |
| [C14-11](scenarios/commands/c14-route-create.md#c14-11) | Template unknown | Worth retaining as an independently checked user outcome: Explain the missing Template, not a successful empty-body creation. |
| [C14-12](scenarios/commands/c14-route-create.md#c14-12) | Lock held | Worth retaining as an independently checked user outcome: Explain the active workspace operation and preserve every path. |
| [C14-13](scenarios/commands/c14-route-create.md#c14-13) | Write failed partial | Worth retaining as an independently checked user outcome: Say creation stopped and name the completed file and unfinished navigation. |
| [C14-14](scenarios/commands/c14-route-create.md#c14-14) | Cancelled | Worth retaining as an independently checked user outcome: Report cancellation and no completed creation. |
| [C14-S05](scenarios/commands/c14-route-create.md#c14-s05) | Outcome 05 | Worth retaining as an independently checked user outcome: Name the unavailable required fact and its consequence. |
| [C15-01](scenarios/commands/c15-route-update.md#c15-01) | Description changed | Worth retaining as an independently checked user outcome: Show the old and new description and the bounded navigation update. |
| [C15-02](scenarios/commands/c15-route-update.md#c15-02) | Tags replaced | Worth retaining as an independently checked user outcome: Replace the complete tag list in the supplied order, rather than appending silently. |
| [C15-04](scenarios/commands/c15-route-update.md#c15-04) | Template applied | Worth retaining as an independently checked user outcome: Copy the Template body while retaining destination metadata and independent ownership. |
| [C15-05](scenarios/commands/c15-route-update.md#c15-05) | Template body protected | Worth retaining as an independently checked user outcome: Apply the allowed metadata change but explain that existing body content was kept and the Template body was not copied. |
| [C15-06](scenarios/commands/c15-route-update.md#c15-06) | No change | Worth retaining as an independently checked user outcome: Say the source already has the requested values and nothing changed. |
| [C15-07](scenarios/commands/c15-route-update.md#c15-07) | Dry run | Worth retaining as an independently checked user outcome: Show old-to-new values in future tense and No files were changed. |
| [C15-09](scenarios/commands/c15-route-update.md#c15-09) | Unknown source | Worth retaining as an independently checked user outcome: Name the missing source without creating it. |
| [C15-10](scenarios/commands/c15-route-update.md#c15-10) | An ambiguous ID requires an exact path | Worth retaining as an independently checked user outcome: Stop without a write. |
| [C15-11](scenarios/commands/c15-route-update.md#c15-11) | Lock held | Worth retaining as an independently checked user outcome: Explain the active operation without modifying metadata or navigation. |
| [C15-12](scenarios/commands/c15-route-update.md#c15-12) | Write failed partial | Worth retaining as an independently checked user outcome: State the completed portion and the real recovery status. |
| [C15-13](scenarios/commands/c15-route-update.md#c15-13) | Cancel before any persistent effect | Worth retaining as an independently checked user outcome: Report cancellation with no changed files. |
| [C15-S06](scenarios/commands/c15-route-update.md#c15-s06) | Outcome 06 | Worth retaining as an independently checked user outcome: Name the unavailable required fact and its consequence. |
| [C16-01](scenarios/commands/c16-route-move.md#c16-01) | Leaf move | Worth retaining as an independently checked user outcome: Move the leaf and update its old/new parent navigation. |
| [C16-02](scenarios/commands/c16-route-move.md#c16-02) | Leaf move with rewritten links | Worth retaining as an independently checked user outcome: Move the source and rewrite exactly the affected link destinations while preserving their visible text. |
| [C16-03](scenarios/commands/c16-route-move.md#c16-03) | Category move | Worth retaining as an independently checked user outcome: Move the complete category and adjust every affected route and authored reference. |
| [C16-04](scenarios/commands/c16-route-move.md#c16-04) | Dry run | Worth retaining as an independently checked user outcome: Show the move and reference/navigation changes without changing any path or byte. |
| [C16-05](scenarios/commands/c16-route-move.md#c16-05) | Destination exists | Worth retaining as an independently checked user outcome: Name the occupied destination and stop without overwriting or deleting either source. |
| [C16-06](scenarios/commands/c16-route-move.md#c16-06) | Destination inside source | Worth retaining as an independently checked user outcome: Explain that a category cannot be moved inside itself. |
| [C16-08](scenarios/commands/c16-route-move.md#c16-08) | Managed source | Worth retaining as an independently checked user outcome: Explain the ownership restriction and use the relevant management action rather than bypassing it. |
| [C16-09](scenarios/commands/c16-route-move.md#c16-09) | Source not found | Worth retaining as an independently checked user outcome: Name the missing source and do not create the destination. |
| [C16-10](scenarios/commands/c16-route-move.md#c16-10) | Ambiguous source prompt | Worth retaining as an independently checked user outcome: Honor the selected physical source and leave the other candidate alone. |
| [C16-11](scenarios/commands/c16-route-move.md#c16-11) | Reference scan incomplete | Worth retaining as an independently checked user outcome: Explain that safe reference updates cannot be established and leave the move unapplied. |
| [C16-12](scenarios/commands/c16-route-move.md#c16-12) | Lock held | Worth retaining as an independently checked user outcome: Explain that another operation is using the workspace and make no move. |
| [C16-13](scenarios/commands/c16-route-move.md#c16-13) | Write failed partial | Worth retaining as an independently checked user outcome: Distinguish completed effects from unfinished work and explain actual recovery. |
| [C16-14](scenarios/commands/c16-route-move.md#c16-14) | Cancelled | Worth retaining as an independently checked user outcome: Report cancellation with no move or link rewrites. |
| [C16-15](scenarios/commands/c16-route-move.md#c16-15) | Ownership cannot establish an unmanaged subject | Worth retaining as an independently checked user outcome: Complete the ownership observation without planning or applying a move/removal. |
| [C17-01](scenarios/commands/c17-route-remove.md#c17-01) | Leaf removed | Worth retaining as an independently checked user outcome: Remove the leaf and its navigation entry, naming the actual removed source. |
| [C17-02](scenarios/commands/c17-route-remove.md#c17-02) | Leaf with detached links | Worth retaining as an independently checked user outcome: Remove the source and detach safe link markup while preserving Team notes as authored text. |
| [C17-03](scenarios/commands/c17-route-remove.md#c17-03) | Category removed | Worth retaining as an independently checked user outcome: Remove the selected category, list the actual effects and handle incoming references under the safe-detach rules. |
| [C17-04](scenarios/commands/c17-route-remove.md#c17-04) | Dry run | Worth retaining as an independently checked user outcome: Show the removal and link changes in future tense, ending with No files were changed. |
| [C17-06](scenarios/commands/c17-route-remove.md#c17-06) | Managed source | Worth retaining as an independently checked user outcome: Explain why this command cannot remove managed content. |
| [C17-08](scenarios/commands/c17-route-remove.md#c17-08) | Ambiguous source prompt | Worth retaining as an independently checked user outcome: Remove only the explicitly selected source after the required confirmation. |
| [C17-09](scenarios/commands/c17-route-remove.md#c17-09) | Reference scan incomplete | Worth retaining as an independently checked user outcome: Explain why safe detachment cannot be checked and make no removal. |
| [C17-10](scenarios/commands/c17-route-remove.md#c17-10) | Lock held | Worth retaining as an independently checked user outcome: Explain contention and preserve every source and link. |
| [C17-11](scenarios/commands/c17-route-remove.md#c17-11) | Write failed partial | Worth retaining as an independently checked user outcome: Say removal stopped after the completed effects and identify remaining work and real recovery. |
| [C17-12](scenarios/commands/c17-route-remove.md#c17-12) | Cancelled | Worth retaining as an independently checked user outcome: Say removal was cancelled and preserve the source and links. |
| [C18-01](scenarios/commands/c18-extension-list.md#c18-01) | None installed | Worth retaining as an independently checked user outcome: Distinguish Installed none from the available packages and describe available choices. |
| [C18-02](scenarios/commands/c18-extension-list.md#c18-02) | One installed | Worth retaining as an independently checked user outcome: List toolkit in installed and available contexts with the correct matching marker. |
| [C18-03](scenarios/commands/c18-extension-list.md#c18-03) | Installed only | Worth retaining as an independently checked user outcome: Show only the installed selection without pretending available packages were absent. |
| [C18-04](scenarios/commands/c18-extension-list.md#c18-04) | Available only | Worth retaining as an independently checked user outcome: Show the available catalogue with useful descriptions. |
| [C18-05](scenarios/commands/c18-extension-list.md#c18-05) | Explicit source | Worth retaining as an independently checked user outcome: Identify the selected source and list its packages, not the bundled defaults. |
| [C18-06](scenarios/commands/c18-extension-list.md#c18-06) | No ownership record | Worth retaining as an independently checked user outcome: Explain that installed packages cannot be established while still listing known available packages. |
| [C18-07](scenarios/commands/c18-extension-list.md#c18-07) | Source unreadable | Worth retaining as an independently checked user outcome: Say available packages could not be listed from that source, preserving any independently known installed rows. |
| [C18-08](scenarios/commands/c18-extension-list.md#c18-08) | Installed source missing | Worth retaining as an independently checked user outcome: Keep the installed identity and say its recorded source is missing. |
| [C18-09](scenarios/commands/c18-extension-list.md#c18-09) | Installed source unavailable | Worth retaining as an independently checked user outcome: Distinguish an unavailable source from a missing one and retain known installed facts. |
| [C18-10](scenarios/commands/c18-extension-list.md#c18-10) | Installed source invalid | Worth retaining as an independently checked user outcome: Name the invalid manifest cause without describing it as an unreadable file. |
| [C18-11](scenarios/commands/c18-extension-list.md#c18-11) | Installed source blocked | Worth retaining as an independently checked user outcome: Explain the unsafe source boundary, without following it or presenting it as ordinary absence. |
| [C18-12](scenarios/commands/c18-extension-list.md#c18-12) | Installed files changed | Worth retaining as an independently checked user outcome: Say the installed file changed and point to inspection of toolkit. |
| [C18-13](scenarios/commands/c18-extension-list.md#c18-13) | Installed files missing | Worth retaining as an independently checked user outcome: Name the missing installed file without calling the whole package uninstalled. |
| [C18-14](scenarios/commands/c18-extension-list.md#c18-14) | Installed target blocked | Worth retaining as an independently checked user outcome: Explain that the target cannot be checked safely and do not follow it. |
| [C18-15](scenarios/commands/c18-extension-list.md#c18-15) | Installed target unavailable | Worth retaining as an independently checked user outcome: Say the installed target could not be read completely and preserve known package identity. |
| [C18-17](scenarios/commands/c18-extension-list.md#c18-17) | Source invalid | Worth retaining as an independently checked user outcome: Explain the exact invalid source shape and required manifest information. |
| [C18-18](scenarios/commands/c18-extension-list.md#c18-18) | Source blocked | Worth retaining as an independently checked user outcome: Explain the source boundary and stop safely. |
| [C18-19](scenarios/commands/c18-extension-list.md#c18-19) | Invalid input | Worth retaining as an independently checked user outcome: Explain the invalid command form. |
| [C19-01](scenarios/commands/c19-extension-inspect.md#c19-01) | Installed matches | Worth retaining as an independently checked user outcome: Explain that the installed package matches the available content, with useful file and dependency facts. |
| [C19-02](scenarios/commands/c19-extension-inspect.md#c19-02) | Installed changed and retired | Worth retaining as an independently checked user outcome: Distinguish changed current content from retired content and preserve both during inspection. |
| [C19-03](scenarios/commands/c19-extension-inspect.md#c19-03) | Available not installed | Worth retaining as an independently checked user outcome: Describe the available package and clearly say it is not installed. |
| [C19-04](scenarios/commands/c19-extension-inspect.md#c19-04) | Installed source missing | Worth retaining as an independently checked user outcome: Preserve known installed identity and explain that full comparison could not finish. |
| [C19-05](scenarios/commands/c19-extension-inspect.md#c19-05) | Newer available | Worth retaining as an independently checked user outcome: Show installed and available versions and the actual content differences without assuming semantic-version ordering proves compatibility. |
| [C19-07](scenarios/commands/c19-extension-inspect.md#c19-07) | Unknown id | Worth retaining as an independently checked user outcome: Explain that this ID is unknown in the selected source and installed set. |
| [C19-09](scenarios/commands/c19-extension-inspect.md#c19-09) | Ambiguous source | Worth retaining as an independently checked user outcome: Explain the ambiguous identity instead of selecting the first manifest. |
| [C19-10](scenarios/commands/c19-extension-inspect.md#c19-10) | Invalid input | Worth retaining as an independently checked user outcome: Explain the invalid ID without silently lowercasing or renaming it. |
| [C20-01](scenarios/commands/c20-extension-create.md#c20-01) | Created | Worth retaining as an independently checked user outcome: Create the manifest and content scaffold under CAT/toolkit and explain the actual location. |
| [C20-02](scenarios/commands/c20-extension-create.md#c20-02) | Created with metadata | Worth retaining as an independently checked user outcome: Preserve supplied descriptive metadata and dependency identity in the manifest. |
| [C20-03](scenarios/commands/c20-extension-create.md#c20-03) | Dry run | Worth retaining as an independently checked user outcome: Show the exact package directory and files that would be created, with no writes. |
| [C20-04](scenarios/commands/c20-extension-create.md#c20-04) | Already present | Worth retaining as an independently checked user outcome: Say the package scaffold is already present and nothing changed. |
| [C20-05](scenarios/commands/c20-extension-create.md#c20-05) | Destination has other content | Worth retaining as an independently checked user outcome: Name the occupied destination and preserve it. |
| [C20-06](scenarios/commands/c20-extension-create.md#c20-06) | Missing id non interactive | Worth retaining as an independently checked user outcome: Explain that the package ID is required without hanging on an unavailable prompt. |
| [C20-07](scenarios/commands/c20-extension-create.md#c20-07) | Prompted id and path | Worth retaining as an independently checked user outcome: Show prompts whose folder meaning matches the created location, then create only after confirmation. |
| [C20-08](scenarios/commands/c20-extension-create.md#c20-08) | Invalid id | Worth retaining as an independently checked user outcome: Explain the ID grammar and preserve the destination. |
| [C20-09](scenarios/commands/c20-extension-create.md#c20-09) | Catalogue unreadable | Worth retaining as an independently checked user outcome: Explain that the selected folder could not be read, not that it is empty. |
| [C20-10](scenarios/commands/c20-extension-create.md#c20-10) | Write failed partial | Worth retaining as an independently checked user outcome: Say what was created and what remains unfinished; do not call a directory a completed file. |
| [C20-11](scenarios/commands/c20-extension-create.md#c20-11) | Cancelled | Worth retaining as an independently checked user outcome: Say package creation was cancelled without creating the destination. |
| [C20-12](scenarios/commands/c20-extension-create.md#c20-12) | End of input before a required answer | Worth retaining as an independently checked user outcome: Return invalid-input with the missing fact, not a cancellation claim or a partially guessed request. |
| [C20-13](scenarios/commands/c20-extension-create.md#c20-13) | Descriptive metadata does not trigger dependency lookup | Worth retaining as an independently checked user outcome: Create the scaffold with the exact descriptive version and dependency declaration. |
| [C21-01](scenarios/commands/c21-extension-install.md#c21-01) | Single package | Worth retaining as an independently checked user outcome: Install the selected package and name actual effects, without an unexplained success dump. |
| [C21-02](scenarios/commands/c21-extension-install.md#c21-02) | With dependencies | Worth retaining as an independently checked user outcome: Explain the selected package and required dependency and install each once. |
| [C21-03](scenarios/commands/c21-extension-install.md#c21-03) | Select from source prompt | Worth retaining as an independently checked user outcome: Show meaningful choices, honor the selected package and confirm the concrete plan. |
| [C21-04](scenarios/commands/c21-extension-install.md#c21-04) | No selection non interactive | Worth retaining as an independently checked user outcome: Explain that a selection is required; do not interpret omission as all packages. |
| [C21-05](scenarios/commands/c21-extension-install.md#c21-05) | Permission required non interactive | Worth retaining as an independently checked user outcome: Name the uncovered destination and required consent, then stop without content effects. |
| [C21-06](scenarios/commands/c21-extension-install.md#c21-06) | Permission prompt always | Worth retaining as an independently checked user outcome: Explain that the scope includes future descendants, save only the chosen grant, and install the package. |
| [C21-07](scenarios/commands/c21-extension-install.md#c21-07) | Permission prompt once | Worth retaining as an independently checked user outcome: Apply this approved operation without persisting a grant. |
| [C21-08](scenarios/commands/c21-extension-install.md#c21-08) | Allow path flag | Worth retaining as an independently checked user outcome: Save the explicit permitted scope and install only eligible planned targets. |
| [C21-09](scenarios/commands/c21-extension-install.md#c21-09) | Existing file without force | Worth retaining as an independently checked user outcome: Name the existing file and explain why it was not replaced. |
| [C21-10](scenarios/commands/c21-extension-install.md#c21-10) | With force | Worth retaining as an independently checked user outcome: Replace only eligible conflicts and say which content was replaced. |
| [C21-11](scenarios/commands/c21-extension-install.md#c21-11) | Already installed | Worth retaining as an independently checked user outcome: Say toolkit is already installed with nothing to do. |
| [C21-12](scenarios/commands/c21-extension-install.md#c21-12) | Changed since install | Worth retaining as an independently checked user outcome: Explain the changed-content boundary and the appropriate update action instead of silently reinstalling over it. |
| [C21-13](scenarios/commands/c21-extension-install.md#c21-13) | No content directory | Worth retaining as an independently checked user outcome: Explain that there was no content to install, without pretending files were created. |
| [C21-14](scenarios/commands/c21-extension-install.md#c21-14) | Dry run | Worth retaining as an independently checked user outcome: Show the complete proposed package and permission effects without saving consent or creating content. |
| [C21-15](scenarios/commands/c21-extension-install.md#c21-15) | Source unreadable | Worth retaining as an independently checked user outcome: Name the unavailable source and do not install a partial dependency closure. |
| [C21-16](scenarios/commands/c21-extension-install.md#c21-16) | Lock held | Worth retaining as an independently checked user outcome: Explain the active operation and make no content changes. |
| [C21-17](scenarios/commands/c21-extension-install.md#c21-17) | Write failed partial | Worth retaining as an independently checked user outcome: Report the installed portion and unfinished operation, with actual recovery and permission-save facts. |
| [C21-18](scenarios/commands/c21-extension-install.md#c21-18) | Cancelled | Worth retaining as an independently checked user outcome: Report cancellation and preserve all target and settings bytes. |
| [C21-S03](scenarios/commands/c21-extension-install.md#c21-s03) | Outcome 03 | Worth retaining as an independently checked user outcome: Install both selected packages once and report each selected identity and actual effects. |
| [C22-01](scenarios/commands/c22-extension-update.md#c22-01) | Up to date | Worth retaining as an independently checked user outcome: Say the package is up to date and there is nothing to do. |
| [C22-02](scenarios/commands/c22-extension-update.md#c22-02) | Files replaced | Worth retaining as an independently checked user outcome: Name the replacement and update to the selected source, without claiming all local edits are preserved. |
| [C22-03](scenarios/commands/c22-extension-update.md#c22-03) | New version with new files | Worth retaining as an independently checked user outcome: Show the version change and new file without treating the version string alone as proof of correctness. |
| [C22-04](scenarios/commands/c22-extension-update.md#c22-04) | Retired kept | Worth retaining as an independently checked user outcome: Keep it by default and explain that it remains from the earlier package version. |
| [C22-05](scenarios/commands/c22-extension-update.md#c22-05) | Retired pruned | Worth retaining as an independently checked user outcome: Remove only eligible retired owned content and list the deletion. |
| [C22-06](scenarios/commands/c22-extension-update.md#c22-06) | All packages | Worth retaining as an independently checked user outcome: Update the intended installed selection, not every available package. |
| [C22-07](scenarios/commands/c22-extension-update.md#c22-07) | Select prompt | Worth retaining as an independently checked user outcome: Present installed choices clearly and honor the selected package and its required dependency scope. |
| [C22-08](scenarios/commands/c22-extension-update.md#c22-08) | No selection non interactive | Worth retaining as an independently checked user outcome: Explain the missing selection and do not default to all installed packages. |
| [C22-09](scenarios/commands/c22-extension-update.md#c22-09) | Permission required | Worth retaining as an independently checked user outcome: Explain the revoked or absent destination permission before updating content. |
| [C22-10](scenarios/commands/c22-extension-update.md#c22-10) | Ownership unknown | Worth retaining as an independently checked user outcome: Explain that ownership cannot be established and do not infer it from matching paths. |
| [C22-11](scenarios/commands/c22-extension-update.md#c22-11) | Dry run | Worth retaining as an independently checked user outcome: Show proposed effects in future tense with no writes or prompts. |
| [C22-12](scenarios/commands/c22-extension-update.md#c22-12) | Source unreadable | Worth retaining as an independently checked user outcome: Explain the missing comparison input and leave all selected package targets unchanged. |
| [C22-13](scenarios/commands/c22-extension-update.md#c22-13) | Lock held | Worth retaining as an independently checked user outcome: Explain contention and preserve content. |
| [C22-14](scenarios/commands/c22-extension-update.md#c22-14) | Write failed partial | Worth retaining as an independently checked user outcome: Report the completed portion, unfinished changes and actual recovery/permission facts. |
| [C22-15](scenarios/commands/c22-extension-update.md#c22-15) | Cancelled | Worth retaining as an independently checked user outcome: Report cancellation without replacing, adding or pruning anything. |
| [C23-01](scenarios/commands/c23-extension-remove.md#c23-01) | Single package | Worth retaining as an independently checked user outcome: Remove the selected package's eligible effects and explain what was removed. |
| [C23-02](scenarios/commands/c23-extension-remove.md#c23-02) | Shared file kept | Worth retaining as an independently checked user outcome: Remove toolkit while explicitly keeping the shared file for its remaining owner. |
| [C23-03](scenarios/commands/c23-extension-remove.md#c23-03) | Missing file released | Worth retaining as an independently checked user outcome: Release the claim and distinguish already missing from physically deleted. |
| [C23-04](scenarios/commands/c23-extension-remove.md#c23-04) | Orphaned dependency | Worth retaining as an independently checked user outcome: Remove toolkit, keep base and explain that the dependency is now unused. |
| [C23-05](scenarios/commands/c23-extension-remove.md#c23-05) | Dependent blocks | Worth retaining as an independently checked user outcome: Name the dependent package and stop removal. |
| [C23-06](scenarios/commands/c23-extension-remove.md#c23-06) | Select prompt | Worth retaining as an independently checked user outcome: Show installed choices and honor the selected removal set. |
| [C23-07](scenarios/commands/c23-extension-remove.md#c23-07) | No selection non interactive | Worth retaining as an independently checked user outcome: Explain that a package selection is needed and do not default to all. |
| [C23-08](scenarios/commands/c23-extension-remove.md#c23-08) | Not installed | Worth retaining as an independently checked user outcome: Say there is no installed toolkit content to remove, without deleting similarly named unowned files. |
| [C23-09](scenarios/commands/c23-extension-remove.md#c23-09) | Dry run | Worth retaining as an independently checked user outcome: Show the removal and kept content in future tense with no writes. |
| [C23-10](scenarios/commands/c23-extension-remove.md#c23-10) | Permission required | Worth retaining as an independently checked user outcome: Explain that removal still needs destination permission. |
| [C23-11](scenarios/commands/c23-extension-remove.md#c23-11) | Lock held | Worth retaining as an independently checked user outcome: Explain contention and preserve content and ownership. |
| [C23-12](scenarios/commands/c23-extension-remove.md#c23-12) | Write failed partial | Worth retaining as an independently checked user outcome: Say removal stopped, identify removed and remaining items, and describe actual recovery. |
| [C23-13](scenarios/commands/c23-extension-remove.md#c23-13) | Cancelled | Worth retaining as an independently checked user outcome: Report cancellation and leave every package effect unapplied. |
| [C24-01](scenarios/commands/c24-library-list.md#c24-01) | None registered | Worth retaining as an independently checked user outcome: Say no Libraries are registered without confusing it with unavailable ownership. |
| [C24-02](scenarios/commands/c24-library-list.md#c24-02) | One current | Worth retaining as an independently checked user outcome: Show the Library identity, source and destination in a concise row. |
| [C24-03](scenarios/commands/c24-library-list.md#c24-03) | Link missing | Worth retaining as an independently checked user outcome: Name the missing link and specific Library action. |
| [C24-04](scenarios/commands/c24-library-list.md#c24-04) | Link changed | Worth retaining as an independently checked user outcome: Explain the changed destination and preserve the user's file. |
| [C24-05](scenarios/commands/c24-library-list.md#c24-05) | No ownership record | Worth retaining as an independently checked user outcome: Report a completed ownership observation, select no claim, and keep unavailable counts unknown. |
| [C24-06](scenarios/commands/c24-library-list.md#c24-06) | Source folder missing | Worth retaining as an independently checked user outcome: Keep the registered identity and describe missing source availability. |
| [C24-07](scenarios/commands/c24-library-list.md#c24-07) | Record invalid | Worth retaining as an independently checked user outcome: Report a completed ownership observation, select no claim, and keep unavailable counts unknown. |
| [C24-08](scenarios/commands/c24-library-list.md#c24-08) | Record unreadable | Worth retaining as an independently checked user outcome: Report a completed ownership observation, select no claim, and keep unavailable counts unknown. |
| [C24-09](scenarios/commands/c24-library-list.md#c24-09) | Link blocked | Worth retaining as an independently checked user outcome: Explain the exact unsafe link check and do not follow the boundary. |
| [C24-10](scenarios/commands/c24-library-list.md#c24-10) | Invalid input | Worth retaining as an independently checked user outcome: Explain the invalid form instead of silently switching to inspect. |
| [C25-01](scenarios/commands/c25-library-inspect.md#c25-01) | Current | Worth retaining as an independently checked user outcome: Say team is current and identify the source, destination and actual linked-file count. |
| [C25-02](scenarios/commands/c25-library-inspect.md#c25-02) | Added source files | Worth retaining as an independently checked user outcome: Show the new source member that needs a link, without creating it. |
| [C25-03](scenarios/commands/c25-library-inspect.md#c25-03) | Retired source files | Worth retaining as an independently checked user outcome: Show the retired member and the corresponding sync difference. |
| [C25-04](scenarios/commands/c25-library-inspect.md#c25-04) | Missing links | Worth retaining as an independently checked user outcome: Explain that the registered link needs restoration. |
| [C25-05](scenarios/commands/c25-library-inspect.md#c25-05) | Changed links | Worth retaining as an independently checked user outcome: Identify the changed destination without implying sync may overwrite it automatically. |
| [C25-06](scenarios/commands/c25-library-inspect.md#c25-06) | Empty source | Worth retaining as an independently checked user outcome: Say the Library is current and has no eligible files, not that its source could not be read. |
| [C25-07](scenarios/commands/c25-library-inspect.md#c25-07) | Source unreadable | Worth retaining as an independently checked user outcome: Say inspection could not finish and do not infer additions or retirements from a partial inventory. |
| [C25-08](scenarios/commands/c25-library-inspect.md#c25-08) | Record invalid | Worth retaining as an independently checked user outcome: Report a completed ownership observation, select no claim, and keep unavailable counts unknown. |
| [C25-09](scenarios/commands/c25-library-inspect.md#c25-09) | Unknown id | Worth retaining as an independently checked user outcome: Say the requested Library ID is unknown. |
| [C25-10](scenarios/commands/c25-library-inspect.md#c25-10) | No ownership record | Worth retaining as an independently checked user outcome: Report a completed ownership observation, select no claim, and keep unavailable counts unknown. |
| [C25-11](scenarios/commands/c25-library-inspect.md#c25-11) | Invalid id | Worth retaining as an independently checked user outcome: Explain the invalid ID without normalizing it into another registration. |
| [C25-12](scenarios/commands/c25-library-inspect.md#c25-12) | Blocked mapping | Worth retaining as an independently checked user outcome: Name the blocked mapping without following an unsafe source or destination. |
| [C26-01](scenarios/commands/c26-library-attach.md#c26-01) | Attached inside agents | Worth retaining as an independently checked user outcome: Register team and create relative file symlinks under the chosen destination, not copies or a directory symlink. |
| [C26-02](scenarios/commands/c26-library-attach.md#c26-02) | Attached outside with flag | Worth retaining as an independently checked user outcome: Save the explicit scope and create only eligible planned links outside .agents. |
| [C26-03](scenarios/commands/c26-library-attach.md#c26-03) | Permission prompt | Worth retaining as an independently checked user outcome: Explain once versus always and the proposed descendants before obtaining approval. |
| [C26-04](scenarios/commands/c26-library-attach.md#c26-04) | Permission required non interactive | Worth retaining as an independently checked user outcome: Name the missing destination permission and stop without creating links. |
| [C26-05](scenarios/commands/c26-library-attach.md#c26-05) | Empty source | Worth retaining as an independently checked user outcome: Register the empty Library and explain that it has no eligible files yet. |
| [C26-07](scenarios/commands/c26-library-attach.md#c26-07) | Source missing | Worth retaining as an independently checked user outcome: Name the missing source folder and create no registration or destination. |
| [C26-08](scenarios/commands/c26-library-attach.md#c26-08) | Destination collision | Worth retaining as an independently checked user outcome: Explain the occupied leaf and preserve it; do not adopt an unregistered link. |
| [C26-09](scenarios/commands/c26-library-attach.md#c26-09) | Dry run | Worth retaining as an independently checked user outcome: Show proposed links, directories and permission scope without creating or saving them. |
| [C26-11](scenarios/commands/c26-library-attach.md#c26-11) | Lock held | Worth retaining as an independently checked user outcome: Explain contention and preserve source, destination and ownership. |
| [C26-12](scenarios/commands/c26-library-attach.md#c26-12) | Record invalid | Worth retaining as an independently checked user outcome: Verify the explicitly requested safe link effects, then the ownership publication. |
| [C26-13](scenarios/commands/c26-library-attach.md#c26-13) | Interrupted partial | Worth retaining as an independently checked user outcome: State completed and remaining links and whether registration/permission publication actually occurred. |
| [C26-14](scenarios/commands/c26-library-attach.md#c26-14) | Invalid input | Worth retaining as an independently checked user outcome: Explain the ID problem without creating links or a registration. |
| [C27-01](scenarios/commands/c27-library-sync.md#c27-01) | Up to date | Worth retaining as an independently checked user outcome: Say team is up to date and nothing needs doing. |
| [C27-02](scenarios/commands/c27-library-sync.md#c27-02) | Links added | Worth retaining as an independently checked user outcome: Add its corresponding relative file link and describe the new link. |
| [C27-03](scenarios/commands/c27-library-sync.md#c27-03) | Links removed | Worth retaining as an independently checked user outcome: Remove the retired destination link only and report that source-driven change. |
| [C27-04](scenarios/commands/c27-library-sync.md#c27-04) | Both | Worth retaining as an independently checked user outcome: Add and remove the respective links in one coherent sync, with accurate unchanged count. |
| [C27-05](scenarios/commands/c27-library-sync.md#c27-05) | Dry run | Worth retaining as an independently checked user outcome: Show the two proposed changes and no writes. |
| [C27-06](scenarios/commands/c27-library-sync.md#c27-06) | Unknown id | Worth retaining as an independently checked user outcome: Explain that team is unknown rather than creating it from a matching folder. |
| [C27-08](scenarios/commands/c27-library-sync.md#c27-08) | Registered link gone | Worth retaining as an independently checked user outcome: Restore the missing link and explain that restoration, with the declared warning outcome. |
| [C27-09](scenarios/commands/c27-library-sync.md#c27-09) | Source unreadable | Worth retaining as an independently checked user outcome: Say sync could not establish the complete source inventory and leave all links unchanged. |
| [C27-10](scenarios/commands/c27-library-sync.md#c27-10) | Permission required | Worth retaining as an independently checked user outcome: Explain that destination permission is required before synchronization. |
| [C27-11](scenarios/commands/c27-library-sync.md#c27-11) | No ownership record | Worth retaining as an independently checked user outcome: Report a completed ownership observation, select no claim, and keep unavailable counts unknown. |
| [C27-12](scenarios/commands/c27-library-sync.md#c27-12) | Lock held | Worth retaining as an independently checked user outcome: Explain contention and make no link or registration changes. |
| [C27-13](scenarios/commands/c27-library-sync.md#c27-13) | Record invalid | Worth retaining as an independently checked user outcome: Report a completed ownership observation, select no claim, and keep unavailable counts unknown. |
| [C27-14](scenarios/commands/c27-library-sync.md#c27-14) | Write failed partial | Worth retaining as an independently checked user outcome: Describe the completed portion, unfinished changes and actual recovery/publication state. |
| [C27-15](scenarios/commands/c27-library-sync.md#c27-15) | Cancelled | Worth retaining as an independently checked user outcome: Report cancellation without changing links or source content. |
| [C28-01](scenarios/commands/c28-library-detach.md#c28-01) | Detached | Worth retaining as an independently checked user outcome: Remove the registered links and team registration, explicitly keeping source content. |
| [C28-02](scenarios/commands/c28-library-detach.md#c28-02) | Detached no links | Worth retaining as an independently checked user outcome: Remove the registration and say it had no links. |
| [C28-03](scenarios/commands/c28-library-detach.md#c28-03) | Dry run | Worth retaining as an independently checked user outcome: Show the proposed link removals without touching links, source or registration. |
| [C28-07](scenarios/commands/c28-library-detach.md#c28-07) | Destination protected | Worth retaining as an independently checked user outcome: Explain the protected destination and make no removal. |
| [C28-08](scenarios/commands/c28-library-detach.md#c28-08) | Permission required | Worth retaining as an independently checked user outcome: Explain that detaching still requires destination permission. |
| [C28-09](scenarios/commands/c28-library-detach.md#c28-09) | No ownership record | Worth retaining as an independently checked user outcome: Report a completed ownership observation, select no claim, and keep unavailable counts unknown. |
| [C28-10](scenarios/commands/c28-library-detach.md#c28-10) | Lock held | Worth retaining as an independently checked user outcome: Explain contention and preserve all registered links and source content. |
| [C28-11](scenarios/commands/c28-library-detach.md#c28-11) | Record invalid | Worth retaining as an independently checked user outcome: Report a completed ownership observation, select no claim, and keep unavailable counts unknown. |
| [C28-12](scenarios/commands/c28-library-detach.md#c28-12) | Write failed partial | Worth retaining as an independently checked user outcome: State which links were removed and which remain, and whether registration publication finished. |
| [C28-13](scenarios/commands/c28-library-detach.md#c28-13) | Cancelled | Worth retaining as an independently checked user outcome: Say detach was cancelled and keep both links and source files. |
| [X01](scenarios/experience.md#x01) | Paste a native Skill and index it unchanged | Worth retaining as an independently checked user outcome: Index the recognized native package, expose it through Skills, and allow its intended source to be inspected/read. |
| [X06](scenarios/experience.md#x06) | Subtract startup context without hiding newly selected material | Worth retaining as an independently checked user outcome: Return selected material not already included at startup, in its defined order; do not drop the new leaf or repeat the startup set. |
| [X08](scenarios/experience.md#x08) | Count actual files and directories after install | Worth retaining as an independently checked user outcome: Every number refers to a named set: installed content files, generated control file, created directories strictly below .agents, and host-file changes are separate. |
| [X09](scenarios/experience.md#x09) | Tell the truth about interruption before and after effects | Worth retaining as an independently checked user outcome: Cancellation and ordinary failure retain different event meanings. |
| [X10](scenarios/experience.md#x10) | Keep a copied Template independent | Retain Template-copy independence; place the copied scenario under memory/working so its destination role matches its content. |
| [X11](scenarios/experience.md#x11) | Keep failures in another workspace out of this request | Worth retaining as an independently checked user outcome: Each operation uses exactly the selected workspace. |
| [X14](scenarios/experience.md#x14) | Distinguish one-time consent, persistent consent and ownership | Worth retaining as an independently checked user outcome: Consent is destination-scoped and separate from selection, final confirmation, force and ownership. |
| [X16](scenarios/experience.md#x16) | Judge partial repair within the actual repair scope | Worth retaining as an independently checked user outcome: Unrelated problems do not automatically invalidate a completed selected repair. |
| [X17](scenarios/experience.md#x17) | Detach known links when their source folder is unavailable | Worth retaining as an independently checked user outcome: List reports source availability; inspect/sync cannot establish a complete source inventory. |
| [X20](scenarios/experience.md#x20) | Keep detail and JSON views about the same outcome | Worth retaining as an independently checked user outcome: Detail changes explanation, never the selected operation, status, exit, actual effects or underlying counts. |
| [X21](scenarios/experience.md#x21) | Allow one live writer per workspace without mistaking a lock file for a writer | Worth retaining as an independently checked user outcome: The second same-workspace writer is blocked while the live lock is held, then can proceed after release. |
| [X22](scenarios/experience.md#x22) | Do not confuse external links with unsupported or broken local links | Worth retaining as an independently checked user outcome: HTTPS is a known unchecked external occurrence without a network request. |
| [X23](scenarios/experience.md#x23) | Keep literal content literal, including whitespace and Unicode | Worth retaining as an independently checked user outcome: Generated UI prose follows the writing rules; authored payload is not sanitized to satisfy those rules. |
| [X24](scenarios/experience.md#x24) | Discover commands and correct mistakes without hidden work | Worth retaining as an independently checked user outcome: Help/version terminate before workspace validation or mutation. |
| [X26](scenarios/experience.md#x26) | Test grant persistence independently from later content failure | Worth retaining as an independently checked user outcome: The report distinguishes saved permission from failed content. |
| [X27](scenarios/experience.md#x27) | Keep unrelated owners and support files through a full package lifecycle | Worth retaining as an independently checked user outcome: Only selected eligible ownership effects occur. |
| [X28](scenarios/experience.md#x28) | Tell a zero result from an unknown or not-requested result | Worth retaining as an independently checked user outcome: Zero is used only for a completed empty set of the stated kind. |
| [X29](scenarios/experience.md#x29) | Report verified content separately from failed ownership publication | Worth retaining as an independently checked user outcome: Best-effort ownership publication must not erase verified content effects or invent saved registrations. |
| [X30](scenarios/experience.md#x30) | Do not claim a local edit when only the intended source changed | Worth retaining as an independently checked user outcome: A current content difference is not proof that a user changed a file since installation. |

