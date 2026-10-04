---
open-forge:
  description: "Historical record: Currency audit of Working and Emerging Memory, with confirmed corrections and stale-record candidates"
  tags: [Memory, Contextual, Audit, Maintenance, Archived, Historical]
---

# Memory Currency Audit

## Archive Status

Archived on 2026-10-04 from `.agents/memory/working/memory-currency-audit.md` after the maintainer selected Memory cleanup. 162-file currency audit and corrections completed; current remaining open-task leads are preserved in their live records. Subsequent user-selected retirement is a new task.

This record preserves historical evidence. The [current CLI development route](../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

## Scope and status

This audit checks all 162 Markdown files present in Working and Emerging Memory
at intake on 2026-10-04. It compares current claims with accepted Tasks,
Decisions, documentation and implementation. Age alone does not make a record
stale. Historical evidence, unresolved ideas and paused work remain useful.

All 162 files were read completely, covering 30,333 lines at intake. The four
packets cover 53 Working files, 30 analysis files, 25 task-review files and 54
other Emerging files. Their original hashes and line counts match the intake
inventory, with no missing, extra or duplicate coverage rows.

The reader returns contain 81 leads across 70 files. These are not 70 obsolete
documents. The lists below distinguish corrected current claims, remaining
currency candidates, superseded historical passages and unresolved questions.
Exact intake locations, replacement evidence and coverage for every file are
retained in `artifacts/memory-currency/audit-result.json` and the four return
packets. Line references in those packets describe intake unless explicitly
marked as a later correction.

The implementation in `docs/onboarding-and-presets` is locally accepted,
uncommitted, unmerged and unpublished. Its final qualification receipts are
separate from this prose audit. The recorded Task 70 release hold remains in
force. No remote publication state was refreshed for this audit.

## Confirmed corrections

- Updated the [Backlog](../working/backlog.md), [Development Plan](../working/cli-development/plan.md),
  [Project Control](../working/cli-development/project-control.md),
  [Task index](../working/cli-development/tasks/_tasks.md) and
  [1.0 polish wave](../working/cli-development/one-zero-polish-wave.md). Their current
  summaries now distinguish the release hold from local Task 72/73 acceptance.
  Earlier noon publication instructions and pre-publication claims are marked
  historical rather than offered as the next action.
- Added a current orientation to the
  [CLI Experience Audit](cli-development/analysis/cli-experience-audit/_cli-experience-audit.md).
  Tasks 30 and 31 are closed and archived. The original observations and
  proposals remain unchanged, with current task state linked separately.
- Marked the [Open Task Review](cli-development/analysis/open-task-review/_open-task-review.md)
  as the preserved 2026-09-28 snapshot and linked its later outcomes separately.
- Updated [Task 61](../working/cli-development/tasks/task61-documentation-accuracy-and-voice.md):
  installer choices and combined qualification are accepted locally, and its
  original defect list points to Task 64's later dispositions. The broader
  maintainer review remains pending.
- Repaired the broken current-evidence fragment in
  [Task 47](cli-development/tasks/task47-entrypoint-reachability.md).
- Updated the baselines in
  [CLI Diagnostic Refinements](../emerging/ideas/cli-debug-diagnostics.md),
  [Distribution Channels](../emerging/ideas/cli-distribution-channels.md) and
  [Perspective Lenses](../emerging/ideas/perspective-lenses.md). Diagnostics use
  `--detail debug`, npm/native packaging is defined, and reusable perspective
  Guidance and Task/Plan starters exist. Their broader proposals remain open.

These corrections affect 12 existing records. The Working and Ideas generated
navigation is maintained through bounded Index plans. Original analytical
bodies, task receipts and protected notebook passages are preserved.

## Working records needing a currency follow-up

These are high-confidence conflicts with newer accepted state, except the
Planning inventory row, which needs a scope check. A small current-state note
or a historical label is usually sufficient. The paused and frozen records were
left intact for their current workstreams.

| File                                                                               | Possibly stale portion                                                                                                                   | Newer evidence and next action                                                                                                                                                                                                                                                                                                                                                                                   |
| ---------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `cli-development/tasks/beta-follow-ups.md`                                         | Skill catalogue traversal is still unspecified, six-host/archive checks are still pending, and the suggested order starts paused Task 39 | [Task 47](cli-development/tasks/task47-default-skill-indexing.md) implements the bounded catalogue bridge. [Task 69](cli-development/tasks/task69-next-beta-stabilization-release.md) records completed platform/archive checks. Route current work to [Project Control](../working/cli-development/project-control.md), preserving Task 70's hold. The literal path avoids adding another known alias conflict. |
| [Task 34](../working/cli-development/tasks/task34-interpolated-value-markup.md)    | Opening state still says open and not started                                                                                            | The [ledger](../working/cli-development/project-control.md#active-task-ledger) records the user pause and saved phase 2/3 checkpoint. Add that current boundary when reconciling the task record, without resuming it.                                                                                                                                                                                           |
| [Task 39](../working/cli-development/tasks/task39-output-audit.md)                 | Current-state section omits the later pause and wider-audit checkpoint                                                                   | The ledger records the pause. Keep its completed representative correction separate from the unfinished wider audit.                                                                                                                                                                                                                                                                                             |
| [Task 48](../working/cli-development/tasks/task48-scoping-for-extension-routes.md) | Opening state says not started                                                                                                           | The [wave](../working/cli-development/one-zero-polish-wave.md) records frozen A/B1 work, deferral beyond beta5 and a requirement before 1.0. Preserve the unfinished scope and saved decisions.                                                                                                                                                                                                                  |
| [Task 66](../working/cli-development/tasks/task66-council-polish.md)               | Current next action still requests review, merge and push of the original branches                                                       | The ledger retains committed review material and marks the review deferred. Label the branch handoff historical. Maintainer review is not complete.                                                                                                                                                                                                                                                              |
| [Task 67](../working/cli-development/tasks/task67-diagram-labels.md)               | Original diagram branch review/merge/push remains the next action                                                                        | The ledger marks review deferred. Preserve the original council/browser evidence and link the current boundary.                                                                                                                                                                                                                                                                                                  |
| [Task 62 command plan](cli-development/tasks/task62/command-plan.md)               | A matched condition automatically activates an untagged entry                                                                            | The accepted [execution follow-up](../working/cli-development/tasks/task62/execution.md) makes `applyTo` a filter, never a trigger. Keep the frozen plan as history with a replacement link.                                                                                                                                                                                                                     |
| [Task 62 foundation plan](cli-development/tasks/task62/foundation-plan.md)         | Braces/classes are rejected and scalar commas stay literal                                                                               | The later [glob dialect](../working/cli-development/tasks/task62/glob-dialect-plan.md) accepts lists, top-level comma separation, classes and braces. Preserve the original specification and later accepted exception separately.                                                                                                                                                                               |
| [Task 62 documentation plan](cli-development/tasks/task62/documentation-plan.md)   | Matching conditions alone activate visible entries                                                                                       | Link the executed filter-only loading decision instead of reusing this original authoring packet.                                                                                                                                                                                                                                                                                                                |
| [Local Planning](../working/local-planning.md)                                     | Package inventory still describes four Templates                                                                                         | [Planning](../../../src/extensions/planning/README.md) now lists seven starters: four work records plus Idea, Analysis and Decision. Refresh the inventory while checking whether the accepted review scope remains the four work-record starters. Do not expand the task automatically.                                                                                                                         |

## Emerging ideas with an outdated baseline

Each row identifies a portion to reconcile, not a decision to reject the whole
Idea. These differences are supported by current local sources.

| File                                                                                                                 | Outdated portion                                                                        | Current outcome and remaining question                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| -------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [Extension overhaul](../emerging/ideas/extensions-overhaul.md)                                                       | `.agents`-only assumptions, unsettled `content/` spelling and queued Tasks 24–26        | [Extension contracts](../crystallized/documents/cli/contracts/extension/_extension.md) define `content/`. [Library contracts](../crystallized/documents/cli/contracts/library/_library.md) define selected-root destinations. Tasks [24](cli-development/tasks/extensions-evolution.md), [25](cli-development/tasks/workspace-library-destination-projections.md) and [26](cli-development/tasks/extension-internal-consolidation.md) are complete. Compatibility, richer dependencies and multi-manager possibilities remain unaccepted. |
| [Task Work Modes](../emerging/ideas/task-work-modes.md)                                                              | Starter Templates remain an unshipped trial                                             | Planning already supplies Task/Plan starters. Simple/Sprint execution modes and external tracker exchange remain proposals.                                                                                                                                                                                                                                                                                                                                                                                                               |
| [Composable Workflow entrypoints](../emerging/ideas/composable-workflow-entrypoints.md)                              | Development Toolkit ships six direct Workflow files                                     | [Toolkit](../../../src/extensions/development-toolkit/README.md) is now a dependency bundle. [Development](../../../src/extensions/development/README.md) supplies methods through the shared selector Skill. Nested specialization remains an independent proposal.                                                                                                                                                                                                                                                                      |
| [Plain-language pass](../emerging/ideas/plain-language-pass.md)                                                      | Current wording work should enter Task 30 G4 and Task 31 Phase 3                        | Those Tasks are closed. Current wording/documentation work belongs to the selected Tasks in the ledger. This does not establish that all prose work is complete.                                                                                                                                                                                                                                                                                                                                                                          |
| [Browser WASM playground](../emerging/ideas/cli-browser-wasm-playground.md)                                          | The documentation site is still a future premise                                        | [Task 52](cli-development/tasks/task52-documentation-site.md) completed the site. Browser execution and its proof remain unaccepted.                                                                                                                                                                                                                                                                                                                                                                                                      |
| [Durable program records](../emerging/ideas/durable-program-records.md)                                              | A former CLI release program is the current promotion trial                             | The [CLI document entrypoint](../crystallized/documents/cli/_cli.md) records that program's retirement. The optional reusable record pack remains open and needs a future trial if pursued.                                                                                                                                                                                                                                                                                                                                               |
| [Agent/workflow audit observation](../emerging/observations/2026-09-02_agent-and-workflow-change-audit.md)           | Follow-up still asks whether review tooling should move and points at unfinished Task 9 | Its own later occurrence records the move, and the [Task 9 audit](cli-development/tasks/cli-architecture-authority-audit.md) is complete. Keep the original occurrences and reconcile the remaining follow-up wording.                                                                                                                                                                                                                                                                                                                    |
| [Review-rationale observation](../emerging/observations/2026-08-18_cli-review-rationale-and-dogfooding-anomalies.md) | Every review must provide the full rationale/alternatives/tradeoff format               | The current [Review Evidence Directive](../../directives/review-evidence.md) reserves that detail for consequential or reusable findings. Routine passes need a concise conclusion, coverage and residual risk. Preserve the historical occurrence and link the current policy.                                                                                                                                                                                                                                                           |

## Historical analysis passages whose outcomes changed

These files remain useful provenance. Their original reproductions and proposed
designs are not current implementation instructions. The corrected collection
orientation links this audit, and the outcome links below make later reuse
safer. No conclusion here closes a broader open task.

| File                                                                                                                      | Changed outcome or assumption                                                                                                                                                                                                                             |
| ------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [Glob-scoped loading](cli-development/analysis/glob-scoped-loading.md)                                                    | Matching alone no longer triggers loading. Braces/classes, expression lists and CLI applicability flags are implemented in the accepted [Task 62 dialect](../working/cli-development/tasks/task62/glob-dialect-plan.md).                                  |
| [1.0 readiness](cli-development/analysis/one-zero-release-readiness.md)                                                   | Its dated queue predates Task 55 closure, Task 47's frozen implementation and local Task 73 acceptance. Use the current ledger for sequence and release state.                                                                                            |
| [Command output design](cli-development/analysis/cli-experience-audit/command-output-design.md)                           | The proposed three-view `--view`/`--json` surface was replaced by four `--detail` levels and `--format`. See the [shared reporting contract](../crystallized/documents/cli/shared-operation-contract.md).                                                 |
| [Finding model](cli-development/analysis/cli-experience-audit/finding-model.md)                                           | Later schema-3 reporting and detail selection differ from the proposed exact nested three-view schema. That old schema was not accepted verbatim.                                                                                                         |
| [C# rules and structure](cli-development/analysis/cli-experience-audit/csharp-directives-and-structure.md)                | Shared-owner rules were strengthened, and [Task 31](cli-development/tasks/task31-implementation-duplication.md) closed selected duplication work. Its withdrawn naming suggestion stays withdrawn.                                                        |
| [Implementation duplication](cli-development/analysis/cli-experience-audit/implementation-duplication.md)                 | Task 31 deliberately retained Move/Remove selection differences. Their similarity is not an unresolved instruction to merge them.                                                                                                                         |
| [Hand-rolled parsing](cli-development/analysis/cli-experience-audit/hand-rolled-parsing.md)                               | BOM/fence-whitespace and native Skill-key defects were corrected. A bounded fence scanner remains, so wholesale parser replacement is not an implemented outcome.                                                                                         |
| [Interaction layer](cli-development/analysis/cli-experience-audit/interaction-layer.md)                                   | The wizard exists and plans precede confirmation. [Task 72](cli-development/tasks/task72-extension-wizard-terminal-layout.md) adds locally accepted bounded viewport behavior. Other error/help proposals need their actual owner.                        |
| [Interoperability and diagnosis](cli-development/analysis/cli-experience-audit/interoperability-and-diagnosis.md)         | Native Skill keys outside the owned fields are ignored. [Task 30](cli-development/tasks/task30-cli-experience-remediation.md) closed the selected remediation. This does not accept skipping every malformed source.                                      |
| [Layer adherence](cli-development/analysis/cli-experience-audit/layer-adherence.md)                                       | The [Architecture](../crystallized/documents/cli/architecture.md) now defines the layers and boundary evidence. [Task 38](cli-development/tasks/task38-project-and-test-split.md) completed the physical split.                                           |
| [Layers and sequencing](cli-development/analysis/cli-experience-audit/layers-and-sequencing.md)                           | The Architecture explicitly defines selection before rendering and separates Framework subject boundaries.                                                                                                                                                |
| [Lifecycle baselines](cli-development/analysis/cli-experience-audit/lifecycle-baselines-and-architecture.md)              | Task 30 G1 replaced redundant baselines with author configuration and ownership state. The all-roots template proposal was not selected unchanged.                                                                                                        |
| [Loading discipline](cli-development/analysis/cli-experience-audit/loading-and-scope-discipline.md)                       | Startup still includes multiple root roles and now requires the CLI Skill when present. References intentionally excludes valid generated Entries interiors. Broader Task 53 loading questions remain open.                                               |
| [Model snapshots](cli-development/analysis/cli-experience-audit/model-level-snapshot-testing.md)                          | Current snapshot support uses Imprint and Integration owns complete host composition. The earlier bespoke-mechanism premise is superseded.                                                                                                                |
| [Presentation field audit](cli-development/analysis/cli-experience-audit/presentation-field-audit.md)                     | Old test populations and JSON-biased coverage describe the audited version. Current Architecture and qualification include composed output and published-process scenarios. This does not close Task 39's wider audit.                                    |
| [Repository dogfood/configuration](cli-development/analysis/cli-experience-audit/repository-dogfood-and-configuration.md) | The selected lifecycle uses two files, not the proposed single file. Exact-path permission grants exist. The broader Git-advisory proposal has no verified disposition here.                                                                              |
| [Severity and command division](cli-development/analysis/cli-experience-audit/severity-and-command-division.md)           | Debug is a detail level, not a fourth finding severity. [Index](../crystallized/documents/cli/contracts/index-candidate/behavior.md) writes generated interiors and discovers existing eligible catalogues rather than inventing metadata or entrypoints. |
| [Structural requirements/markers](cli-development/analysis/cli-experience-audit/structural-requirements-and-markers.md)   | Existing missing/empty local Axioms are allowed by the Loader. New-entrypoint scaffolding remains distinct. Entries marker retirement and CLI Skill delivery are bounded outcomes, not acceptance of every proposed loader change.                        |
| [Taxonomy and adoption](cli-development/analysis/cli-experience-audit/taxonomy-and-adoption.md)                           | Workflows use Skills and the Skills category is populated. The concrete [Task 73 presets](cli-development/tasks/task73-layered-adoption-and-installation-choices.md) replace the older tier proposal and are accepted locally.                            |
| [Test-layer consolidation](cli-development/analysis/cli-experience-audit/test-layer-consolidation.md)                     | Published tests are not universally redirected: Task 72 has real Windows ConPTY evidence. Real Unix TTY qualification remains a limitation.                                                                                                               |
| [Test strategy/scenarios](cli-development/analysis/cli-experience-audit/test-strategy-and-scenarios.md)                   | The accepted topology keeps three test projects and shared support rather than adding a fourth scenarios project.                                                                                                                                         |
| [View/test architecture](cli-development/analysis/cli-experience-audit/view-layer-and-test-architecture.md)               | Model selection exists, and current Architecture defines the retained test boundaries. The replacement taxonomy was not accepted verbatim.                                                                                                                |
| [Category wording retrospective](cli-development/analysis/cli-design-retrospective/category-boundaries-and-wording.md)    | The Directive scope gate exists, and [Task 43](cli-development/tasks/task43-workflows-as-skill.md) implements Workflows through Skills. Keep the document's own revision of the Maps suggestion.                                                          |
| [Contract/code retrospective](cli-development/analysis/cli-design-retrospective/contract-versus-code.md)                  | Its September comparison predates the selected presentation stage and Task 30 G4 reporting. Preserve reset-era measurements as dated evidence.                                                                                                            |
| [Original-design assessment](cli-development/analysis/cli-design-retrospective/original-design-assessment.md)             | Entries markers were retired, but generated Entries remain persisted. Calculate-on-read was not the selected outcome.                                                                                                                                     |

## Historical task reviews with later outcomes

The entire collection is a dated snapshot. These are the clearest outcomes to
consult instead of re-executing an old recommendation.

| Review                                                                                       | Later outcome                                                                                                                                                                                                           |
| -------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [Task 30](cli-development/analysis/open-task-review/task30-cli-experience-remediation.md)    | Closure was accepted. [The archived Task](cli-development/tasks/task30-cli-experience-remediation.md) assigns the remaining residues to their current owners.                                                           |
| [Task 32](cli-development/analysis/open-task-review/task32-minimal-output-sweep.md)          | The keep/remove policy and bounded fixes were accepted and independently qualified. Current ledger integration state does not imply release closure.                                                                    |
| [Task 35](cli-development/analysis/open-task-review/task35-removal-and-suppression-model.md) | The removal [Decision](../crystallized/decisions/framework/workspace-state-files.md) and current shared-presentation rules exist. Task 63's broader direct-edit policy remains open.                                    |
| [Task 36](cli-development/analysis/open-task-review/task36-extension-merge-and-guards.md)    | [Question 2](../working/cli-development/tasks/task36-extension-merge-and-guards.md) uses a visible heading and named closing paragraph, implemented and locally accepted. Partial merging remains deferred.             |
| [Task 44](cli-development/analysis/open-task-review/task44-template-content.md)              | The [loader maintenance baseline](../crystallized/documents/maintenance/payload/agents/loader.md) is 100 authored non-empty lines for review, not the old hard 35–80-line ceiling. The wider content review stays open. |
| [Task 47](cli-development/analysis/open-task-review/task47-entrypoint-reachability.md)       | The bounded Index-only Skill catalogue bridge is implemented and qualified. It does not change activation or shared runtime topology.                                                                                   |
| [Task 48](cli-development/analysis/open-task-review/task48-scoping-for-extension-routes.md)  | Later selection requires it before 1.0, with scoped-copy direction chosen and A/B1 frozen. It is still unfinished.                                                                                                      |
| [Task 54](cli-development/analysis/open-task-review/task54-tag-trimming.md)                  | Independent implementation and qualification are complete and integrated as recorded by the ledger. Release closure remains separate.                                                                                   |
| [Task 55](cli-development/analysis/open-task-review/task55-alternative-root.md)              | [Task 55](cli-development/tasks/task55-alternative-root-decision.md) was restored and closed with `.agents` retained. The earlier fold/interop trial is superseded without an APM certification claim.                  |
| [Task 59](cli-development/analysis/open-task-review/task59-beta-2-release.md)                | [The release receipt](cli-development/tasks/task59-beta-2-release.md) records completed beta2 publication and checks. Red-CI/tag statements were historical blockers.                                                   |
| [Task 60](cli-development/analysis/open-task-review/task60-cli-skill.md)                     | Current local Task 73 requires the complete CLI Skill when present. The short loader pointer remains. The earlier pointer-only reduction is superseded.                                                                 |

## Leads that need revalidation rather than a stale verdict

- Product-name collision research (removed by maintainer direction on 2026-10-04)
  contains a naming shortlist. Current public prose uses ACE for the method, but
  this does not reject TRACE/GRACE possibilities or establish new collision
  research. Keep the dated evidence and revisit the precise naming question.
- [Scope capsules and critical loading](../emerging/ideas/scope-capsules-and-critical-loader-loop.md)
  overlaps accepted capsule/child-validation discipline. Its CLI assistance and
  critical-loader experiment remain unimplemented proposals.
- [Architectural delegation-gap evidence](../emerging/observations/2026-08-21_architectural-context-delegation-gap.md)
  and [future-consumer evidence](../emerging/observations/2026-08-25_shared-cli-capability-horizon.md)
  have promoted safeguards in the current Program Architecture Directive.
  That does not prove every specific follow-up audit ran or every risk vanished.
- [Memory authority retrospective](../emerging/analysis/cli-design-retrospective/memory-authority-boundary.md)
  is an unaccepted reinterpretation. Current rules distinguish agent behavior
  from accepted subject requirements. Keep the proposal contextual and do not
  use it to relocate contracts.
- [Authors' Findings](../emerging/authors-findings/open-forge-findings-log.md)
  has preferences with some accepted follow-ups, including Task 65 placement.
  The broader Local Planning questions remain open. The original notebook is
  protected and unchanged, as is the provider-comparison record.

## Old drafts to reconcile before reuse

These seven files are historical alternatives, not live documentation. They
need comparison with today's vocabulary, CLI flags, package model and onboarding
if selected for reuse. Their historical role is already valid.

- README-v2 (removed by maintainer direction on 2026-10-04)
- CLI draft (removed by maintainer direction on 2026-10-04)
- Development draft (removed by maintainer direction on 2026-10-04)
- Extensions draft (removed by maintainer direction on 2026-10-04)
- Open Forge fable (removed by maintainer direction on 2026-10-04)
- Second fable (removed by maintainer direction on 2026-10-04)
- Fable deliberation (removed by maintainer direction on 2026-10-04)

Other open Ideas were retained. Deferred implementation, subjective comparisons,
historical trial results and unverified external-library readiness were not
declared stale merely because they are old. Completed Task receipts do not
automatically require archival, and no completion-grace rule was changed.

## Related current-document leads outside this inventory

The [Index contract entrypoint](../crystallized/documents/cli/contracts/index-candidate/_index-candidate.md)
retains non-shipping wording despite current CLI exposure. Its shipping wording
needs a separate correction. The
[References entrypoint](../crystallized/documents/cli/contracts/references-candidate/_references-candidate.md)
does not make that non-shipping claim. Both retain staging paths deliberately,
as the [contract-set document](../crystallized/documents/cli/command-contract-set.md)
states. This audit selected no contract-route migration.

## Subsequent retirement selection, 2026-10-04

The maintainer subsequently selected deletion of the seven old drafts and the
product-name collision note, archival of completed work and superseded
analyses, and updates to unfinished Working and Emerging records. The eight
deletions and selected currency corrections are applied.

The reviewed plan applied 70 archive moves. Seven additional historical
snapshots preserve the detail removed from live plans and mixed packets.
Task 62's later Glob UX and polish
review is still phase 2/2, milestone 2/3, with M3 pending. Its parent, execution,
dialect plan, and routing entrypoint remain Working. Only the completed initial
planning packets retire. Tasks 34 and 39 remain paused, Task 48 stays frozen,
and Tasks 61, 66, and 67 retain unfinished review.

`route move` refused the checkout's unrelated dependency junction before any
move. The maintainer approved the bounded exact-path fallback in the subsequent
conversation. The 70 ordinary Markdown files moved without touching that
junction, and 636 affected links were repaired. The source and destination list
and exact application receipt are in `artifacts/memory-retirement/`.
Original Markdown bytes have ignored local backups. The original audit and
verification below remain dated receipts rather than current lifecycle
instructions. The later retirement closeout records its separate final checks.

## Verification and continuation

Coverage reconciliation passed for all 162 intake files. Original reader
returns and receipts are retained. Missing ignored Task 47/70 evidence files in
this worktree are an availability limit, not proof of global loss. Public release
outcomes use dated repository receipts rather than a new remote check.

Changed-file formatting and generated-navigation idempotence pass. The final
repository Markdown gate passes 13/13 with no failures or skips. Doctor finds
no new issues relative to the recorded baseline. Its 12 existing alias errors
remain, warnings decrease from 803 to 802 after the Task 47 fragment repair,
and info findings remain at 118. Doctor therefore remains blocked, not clean.
Exact results and final file hashes are retained in
`artifacts/memory-currency/closeout.json`.

Use the current-record rows first for any later documentation reconciliation.
Link accepted outcomes to historical collections rather than rewriting their
original bodies. Lifecycle moves need a separate selection and must retain
useful provenance. Preserve Tasks 34/39 pauses, Task 48's frozen boundary,
Task 70's release hold and the protected notebook. This audit selects no
archival, deletion, task reopening or new product implementation.
