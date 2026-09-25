---
open-forge:
  description: Prepare Framework, Extension, and public documentation for release and reconcile current non-CLI agent context
  tags: [Memory, Archived, Contextual, Historical, Task, Framework, Extension, Release, Writing, Review]
---

# Task 28: Framework Release Preparation

## Task State

- State: Complete for the accepted source and documentation scope; the user authorized squash integration into local `develop` on 2026-09-12. Remaining personal wording refinements belong to the user.
- Permanent identity: Task 28, previously named Source Framework Wording and Logic Review. This continues that task with the user's expanded release-preparation scope.
- Authority: The user's 2026-09-11 direction to update the task, discuss the public language fully, preserve accepted semantics, and reconcile current repository context.
- Responsibility: This task handles Framework and Extension content and all public documentation, including CLI and setup documentation. Astra/max with the complete accepted context handles source changes. CLI implementation, contracts, tests, and runtime qualification remain with the user's implementers on separate branches.
- Current step: The generic content pass is complete. The user then explicitly authorized the focused package split and Experience Design removal. Five focused packages plus the dependency-only Toolkit bundle are prepared, with retained payload bytes and installed paths unchanged. CLI catalogue refresh and ownership-transition qualification remain assigned to the separate task.
- Integration: The user authorized a local squash merge of reviewed tip `e245aa3b` onto current `develop`. Preserve newer CLI implementation, delivery tooling, and unrelated task state. Publication and global installation are outside this action.

The established task path exists in the newer shared checkout but was absent from this older worktree. This updated record is prepared at that same path for later reconciliation. Preserve the permanent identity and unrelated ledger state during integration; do not replace the newer project ledger with this worktree's older copy.

## Outcome

Prepare public Framework and Extension content that is natural to read, easy to understand, easy to review, and enjoyable to return to. Preserve the accepted semantics, logic, rules, conditions, and distinctions. The language discussion must establish how that experience should be achieved before the general rewrite begins.

Reconcile the repository's current non-CLI `.agents` content so it accurately represents accepted knowledge and applicable rules. Each source must answer its own question. Current guidance must not depend on obsolete explanations or conflicting copies of the same meaning.

This is preparation for release. Completion of this source task does not establish CLI qualification or authorize a release.

## Scope

| Surface                                  | Work                                                                                                                                                                                                                                                   |
| ---------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `src/open-forge/` and `src/extensions/`  | Read all public prose, including hidden Markdown files, native Skill references, descriptions, examples, and manifest descriptions. Streamline language and structure according to the agreed approach. Preserve machine-readable syntax and behavior. |
| Root README and all public `docs/` files | Provide a quick introduction, setup, use, and overview with deeper current documentation. Rewrite public CLI and setup docs from verified source facts. Remove legacy explanations and project-status clutter.                                         |
| Current non-CLI `.agents/` sources       | Deep review of accuracy, currency, contradictions, duplication, source responsibility, references, and discoverability. Include Directives, Guidance, Patterns, Templates, Workflows, Maps, native Skill content, and current Crystallized knowledge.  |
| Observation Template                     | Adopt the packaged starting shape in the local copy if the comparison shows it is better. Preserve continuing requirements in the sources that define them.                                                                                            |
| Task records and review evidence         | Keep this task, its navigation, and the accepted review directions current enough to support review and later integration.                                                                                                                             |

The current-content scrub excludes records in Working, Emerging, and Archived Memory. The user may remove archives later; this task does not authorize that deletion. Templates for these states and current documents explaining their behavior remain in scope because they define present content rather than storing excluded records.

CLI implementation, contracts, CLI-specific Directives, Patterns, Templates, tests, and runtime verification remain assigned to other tasks. The user subsequently authorized all public documentation here, including CLI and setup docs. That authorization does not transfer CLI behavior changes into this task. Inspect implementation and contracts read-only to verify documentation, and record any remaining mismatch for the implementer. Repository-only agent tooling and authored APM/runtime agent sources remain outside this rewrite.

Both diagram drafts remain deferred and are not production-ready. They are retained in [Framework review ideas](../../../emerging/ideas/framework-review/_framework-review.md).

## Language And Meaning

Discuss voice, audience, terminology, explanations, sentence flow, examples, and document structure with the user. Use actual before-and-after passages to make choices concrete. Preserve earlier accepted direction unless the user explicitly changes it. The existing [Writing Standard](../../../crystallized/documents/maintenance/writing.md) and [Dictionary](../../../crystallized/documents/maintenance/helpers/dictionary.md) are starting sources for this discussion.

The user accepted the natural, conversational direction shown in the [three larger examples](../../framework-review/proposals/release-language-examples.md), with one explicit correction: retain “self-growing” as a representative descriptor for Memory. They authorized recording that approach, committing the existing work on this branch, and performing the language update as a separate commit. No further general style approval is required for routine edits within that accepted meaning.

The user accepted this distinction: a description helps a reader decide whether to open a file; a responsibility helps an editor decide what belongs in it. Preserve that explanation in the existing Markdown maintenance sources, README, and self-contained shipped context. Responsibility remains optional when it adds a distinct boundary. No metadata rename or new question field is needed.

The user chose primary questions in category openings and delegated the exact presentation. Their later example places the question before its answer. Keep the category name as the level-1 title, use the primary question as a level-2 heading without extra bold formatting, and follow it with the definition and any necessary explanation. This ordinary heading adds no metadata field or parser-recognized Framework behavior. Preserve all category meaning and Axioms, including deliberate self-growing Memory, Template independence, and native Skill rules. The [category-opening proposal](../../framework-review/proposals/category-openings-proposal.md) preserves the comparison that led to this choice.

The README is supporting introductory material. Explain what Open Forge is, what it provides, and how it is used. The user wants ACE to have a definition of its own, building on ideas from progressive disclosure and spec-driven development. Explain what deliberate organization and evolution of workspace context add, using professional, friendly language with character. Preserve relevant context selection, explicit accepted expectations, broad applicability, and proportionate specification. Describe conceptual development without claiming measured superiority. The Framework must remain understandable through its own files without requiring the README.

Treat easier reading and easier review as outcomes, not a target word count. Do not remove a condition, exception, scope, relationship, or required behavior merely to shorten a passage. Keep required instructions, recommendations, defaults, optional methods, and contextual knowledge distinguishable.

For the current-content scrub, distinguish an obsolete explanation from an unresolved change to accepted meaning. Correct the former from its defining sources. Surface the latter before work depends on an invented answer. Preserve why a prior Decision was accepted while linking the source that defines the present result.

The earlier L1–L4 analysis remains useful evidence. It is not the complete release-preparation scope. Workflow readability belongs within the language work, not an additional task outside the plan.

### Latest Review Direction

The README should move quickly from what Open Forge and ACE are to their value, setup, everyday use, composition, and useful CLI commands. Preserve the invitation to grow a custom Framework around a project's own needs and tools. Explain how narrow scopes make progressive disclosure useful as the workspace grows. Keep the Framework self-contained, model and harness agnostic, and complete without the CLI. Use a professional, friendly voice with light developer humor. Avoid calling people “human.”

Use the same primary questions in the README and the category files. Check each source question and answer for logical and stylistic agreement. Explain Memory's evolution positively while preserving deliberate capture, useful growth, and its authority boundaries. Prefer a specific noun such as records, knowledge, findings, or content when the broader word “material” obscures the subject.

Measure the actual shipped startup context and complete base Framework with a named tokenizer. Record the included files and counting method; exclude Extensions, project-specific content, and harness overhead from the baseline claim. Recount the final source bytes before committing a number to the README.

Audit source tags and their generated navigation. Discuss any proposed tag addition or removal with the user before applying it. Review inherited Axiom duplication, concrete loader ambiguities, the `KeepInMind` name, and whether the repository's exact mechanical execution exception belongs in the shipped Framework. Preserve unique conditions and intentional local behavior when removing repetition.

The user accepted aligning the repository Workflow category with the shipped authored text while retaining the local risk-based Workflow switching rule in an adjacent overwrite. Preserve local generated Entries. The change restores the shared selection and recipe guidance and makes the one accepted customization explicit.

The user subsequently rejected the README phrase “for working with AI” and the teaching tone of introductory prose. Keep shipped rules precise and non-pompous, but give READMEs and similar introductions a distinct voice: friendly, natural, quietly proud of the project, and inviting without marketing language or arrogance. The user accepted the example as a starter they may refine manually. Their subsequent corrections reject “We started with ideas” as forced and favor a personal origin story beginning “Inspired by…”. They ask for more eloquent, varied phrasing, including passive constructions where natural, and fewer dense comma-separated feature lists. Record this approach in a separate Project Voice guide and reconcile the Writing Standard so rules and introductions have distinct voices with shared accuracy requirements.

The user's later clarification assigns the CLI behavior task (referred to as Task 29) to a different branch. Keep the prepared scoped-continuity and catalogue records here until integration is authorized; do not reserve an identity or start that implementation from this task.

## Plan And Dependencies

| User item | Work and completion condition                                                                                                                                                                                    | Dependency                                                                                                                                     |
| --------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| 1         | Apply the agreed public language. Read and streamline every public Framework/Extension prose file. Then complete the current non-CLI `.agents` scrub, recording each file's disposition and unresolved question. | Commit the prior source changes and writing standard first. Keep the language update in a separate commit. Each cleanup uses accepted meaning. |
| 2         | Compare the local and packaged Observation Templates. Update the local copy if the packaged shape is better and verify the resulting alignment.                                                                  | This bounded comparison can proceed while the language discussion is open.                                                                     |
| 4         | Prepare the accepted changes and task records for integration. Reconcile baseline drift, preserve review evidence, verify the final candidate, and identify the baseline the CLI implementers should use.        | Follows items 1 and 2. Actual integration still respects the user's Git authorization.                                                         |
| 3         | Other agents complete scoped continuity loading, embedded catalogue synchronization and diagnostic follow-up, and lifecycle documentation alignment.                                                             | Starts from the agreed baseline after the preceding content and integration work. This task does not dispatch those agents.                    |

The three prepared CLI tasks are [Scoped Continuity Loading](scoped-continuity-loading.md), [Extension Catalogue Synchronization](extension-catalogue-synchronization.md), and [Extension Lifecycle Documentation Alignment](extension-lifecycle-documentation.md).

## Current State And Evidence

The initial independent source review and local Extension comparison are complete. Commit `0eea5e65` records the accepted P02–P08 changes, T28-RS1–RS5 corrections, Extensions, task records, Observation Template alignment, and writing approach.

Commit `2b31f4e4` contains the separate language rewrite. All 51 public source files were read: 31 improved and 20 retained. The README and 21 dogfood counterparts also changed. A focused review of all 53 changed paths found no material regressions. Metadata, headings, examples, step order, generated navigation, and local specializations were preserved. Static checks verified 139 reference destinations; CLI-owned fragments and runtime behavior remain outside this task. The loader remains within its existing budget at 79 authored non-empty lines, excluding generated Entries.

The [review follow-up notes](../../framework-review/review-followup-instructions.md) preserve the accepted answers and link to the original reports and application receipts. The earlier Markdown inventory was a mechanical scan with targeted close reading; it was not a complete editorial review of every file.

The current-content audit covered 182 files for their non-CLI meaning. One additional candidate file was classified as CLI-owned and excluded. Mixed CLI sections were deferred explicitly. Most content remains current. Commit `e91d2934` aligns the old loading Decision with scoped continuity, distinguishes routed Extension files from native/support files, classifies current explanatory documents by their own state, corrects a Maps link and an obsolete evaluation reference, and removes misleading helper and empty-scope claims. A contradictory C# inheritance sentinel was removed without changing its local rules. Generated entries follow the corrected metadata.

Three broken references to CLI history are recorded in the separate [lifecycle documentation task](extension-lifecycle-documentation.md#reference-repairs-already-prepared). CLI-owned content was not rewritten by this scrub. Stored Working, Emerging, and Archived records were excluded; current rules and Templates for those states were reviewed separately.

The final follow-up applies the question-first opening to all 21 public categories and their local counterparts. Each keeps its category title, uses the primary question as a level-2 heading, and places the definition immediately below it. Both loaders and the current Markdown, writing, maintenance, and terminology sources explain the reader/editor distinction. The README and Vision give ACE its own definition, building on progressive disclosure and specification-driven development through deliberate context shaping and maintenance.

All 52 paths in that content pack were reviewed. The focused review resolved one mismatch in the canonical category-shape list and passed the final wording. Static checks preserve Axioms, metadata, generated Entries, examples, existing references, and intentional local differences. The known historical CLI reference remains assigned to the separate documentation task. No CLI runtime qualification is claimed. A file may remain unchanged when inspection establishes that it already meets the agreed standard.

The local Observation Template now matches the packaged source exactly. Its extra review prompts repeated the current Review Evidence Directive, and its broader identifier ban could obstruct traceable findings. The continuing review rules remain in their defining Directive. The [alignment receipt](../../framework-review/evidence/observation-template-alignment.json) records the comparison, removed additions, exact parity, and verification.

The [Framework review scope](../../framework-review/_framework-review.md) now holds the discussion, earlier proposals, and review evidence in tracked Working Memory. Deferred diagrams and the retained review draft live in routed Emerging Memory. Generated build and test output is disposable and does not define task state or accepted meaning.

## Current Follow-Up Receipt

Commit `40e55138` records the accepted category, inherited-rule, loader-threshold, Memory wording, and local Workflow customization corrections, with their defining current sources. The shared Workflow entrypoint now matches the shipped source, and the accepted local switching rule lives in `_workflows.overwrite.md`. The companion is tracked and remains outside generated Entries.

The final source review found no material issue. The audit covered all 21 public category openings and all 44 Markdown/native Skill files under source `.agents` directories. Existing source tags, frontmatter, and generated Entries remain unchanged. All 29 generated links match their destination metadata. The loader keeps 79 authored non-empty lines, including its two generated-region boundary markers. Source and local authored loaders match.

The complete Framework source has 23 Markdown files and measures 8,267 tokens with `o200k_base` or 8,309 with `cl100k_base`. Default startup follows the canonical AGENTS handoff, loader, and exposed loading tags through loaded parents: 19 files, measuring 6,823 or 6,863 tokens respectively. These are per-file raw-text sums using tiktoken 0.14.0, including metadata and navigation, excluding Extensions, project content, and harness overhead. The [measurement record](../../framework-review/evidence/context-token-counts.json) identifies every measured source file and hash.

The tag audit recommends keeping the existing vocabulary, including `KeepInMind`. No tag was added or removed. The mechanical execution exception remains local; extending the installed default would require a separate distribution decision. The public guides now explain the active-scope meaning of loading tags without claiming that the separate CLI implementation fix is complete.

The README, all public `docs/` files, and the three Extension READMEs have current-content drafts. Static review checked their examples against CLI source and contracts at `develop` commit `2b54fdc598a48a12e44772a5cb323cf45e9e5a73`. Link checks across the candidate covered 540 destinations with no remaining issue. A native CLI was not executed for this pass. The [CLI documentation verification task](extension-lifecycle-documentation.md) retains the remaining executable and integration checks.

The user approved the introductory example as a starter, then refined its rhythm and wording. The README and three package READMEs now follow the separate [Project Voice](../../../crystallized/documents/maintenance/project-voice.md) guide. The Writing Standard keeps shared accuracy requirements and the precise voice of rules and reference text. The coherent public pack passed writing review. The primary owner also reviewed the final user-directed origin-story and sentence-flow refinements. Static checks cover 140 relative links and anchors, all 12 README category questions, unchanged token-measurement source hashes, and the new guide's exact navigation metadata. The writing policy and public language changes are prepared separately. CLI executable qualification remains with the linked verification task. Both diagrams remain deferred, and no merge to `develop` is authorized.

## Extension Reassessment

The user deferred Extension work while the READMEs and Project Voice guide were completed, then resumed discussion after the branch logic review and quick fixes. Earlier reports that E01–E04 were applied do not establish that every accepted capability is complete.

The original E02 discussion accepted an optional minimal planning capability with predictable Task, Plan, Backlog, and current-work state where useful. A small task can keep its outcome, short plan, and current state together. Each mutable fact has one defining source; separate records must answer distinct questions. Use existing task systems when applicable. Keep procedures in Workflows, inspectable shapes in Patterns, and copy-ready starting content in Templates. These are optional Extension capabilities, not new Core categories or a mandatory ledger.

The prior content pass added Task, Plan, Backlog, and Checkpoint starters alongside the Planning Workflow and Work Records Pattern. These complete E02 without requiring a fixed record collection or replacing existing task systems.

The user then explicitly accepted the reduced split and Experience Design removal. The [decision](../../../crystallized/decisions/extensions/focused-extension-packages.md) records the rationale. The [catalogue](../../../../../src/extensions/README.md) now defines five focused packages plus the dependency-only Toolkit bundle. Development, Debugging, and Review remain together. Orchestration depends only on Planning and Development. The shipped Skill and matching repository copies are removed.

The [split verification](../../framework-review/reviews/extension-package-split.md) records retained payload identity, isolated dependency closure checks, local alignment, and limits. Shipped content must resolve from its own package, declared dependencies, and the base Framework. No repository-local dogfood source is required. No APM agent package is introduced.

The [original review notes](../../framework-review/review-followup-instructions.md) retain the detailed E02 and E04 discussions. The linked notes and their supporting review records are now tracked in routed Memory.

## Generic Extension Content Follow-Up

The latest user direction is preserved in the repository's Framework change instructions and [Extension architecture](../../../crystallized/documents/extensions/architecture.md#reusable-content-and-project-context). Generic methods and starting shapes use selected project sources for actual facts, technologies, commands, conventions, required evidence, and accepted decisions. They do not import this repository's task bureaucracy into other projects.

The [content review](../../framework-review/reviews/extension-generalization.md) records the changed and retained sources, new planning Templates, static checks, and limits. Shared Template and Pattern copies are aligned. Existing local Workflow profiles and specialized Task/Plan Templates remain intact. The [Experience Design analysis](../../framework-review/analysis/experience-design-value.md) records the separate distribution recommendation and the absence of measured comparison evidence.

## Branch Logic Review Follow-Up

The [review](../../framework-review/reviews/branch-logic-review.md) found no material Framework logic defect within its scope. It disclosed prior local-loader exposure and used reasoned scenarios and static checks rather than runtime trials. BLR-01 identified eight historical-support links whose targets were present locally but absent from Git. The [correction record](../../framework-review/reviews/branch-logic-review-followup.md) preserves the finding, support-file identities, and targeted verification. The original review and its frozen phase-one section remain unchanged.

## Integration

The user accepted the branch for local squash integration on 2026-09-12, reserving remaining personal wording refinements for manual edits. This supersedes the earlier no-merge boundary retained in historical review records.

Reviewed source tip: `e245aa3b602428168e020974583a64bb0afd9a6f`. Integration parent: `f54ee64b4ceb808ff996d62d29e2c15f3c53675a`. The squash commit containing this record combines the reviewed Framework and Extensions with current develop. Newer CLI implementation and delivery tooling remain unchanged. Public development instructions retain the newer Node/npm delivery commands and script locations.

Integration verification preserves exact reviewed `src/open-forge/` and `src/extensions/` trees, current develop CLI and delivery tooling, all six isolated package closures, thirteen Template copies, and 1,262 authored relative links and anchors. Whitespace findings are limited to six unchanged historical evidence files whose reviewed bytes are preserved. No CLI build, runtime qualification, global refresh, or publication was performed for this merge.

The task's substantive changes begin after `8a52ede13`. Earlier CLI correction commits in the branch ancestry do not replace the newer implementation. The current project ledger and unrelated task results are preserved. The three prepared CLI follow-ups are integrated as task records; their implementation is not performed here.

## Verification And Closeout

- Record the complete public prose inventory and which current `.agents` sources were reviewed, changed, retained, or deferred to another task. A text scan alone does not complete the review.
- Compare rewritten meaning with the accepted source. Check requirement strength, scope, authority, state, dependencies, exceptions, and source relationships.
- Check affected links, headings, metadata, generated navigation, examples, and source/dogfood alignment. Preserve intentional local specialization when it remains justified.
- Recheck corrected findings and their affected neighborhood. CLI runtime evidence belongs to the separate implementers and is not replaced by prose checks.
- Report the final reviewable result, remaining limitations, excluded content, and integration status. Keep the source task, CLI implementation, and release qualification distinct.
