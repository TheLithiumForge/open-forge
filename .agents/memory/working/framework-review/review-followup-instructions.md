---
open-forge:
  description: Recorded user answers and review discussion for Task 28, with current status defined by its Task record
  tags: [Memory, Working, Contextual, Framework, Review]
---

# Task 28 — User review follow-up instructions

This record preserves the user's answers and the sequence of review proposals. Dated status statements describe those earlier stages. The [Task 28 record](../cli-development/tasks/source-framework-review.md) defines current progress and deferred work. Accepted Framework meaning is maintained in the defining sources linked from that task.

These notes record the user's subsequent review of the proposals. Accepted directions recorded here are authoritative user input for this review and its later integration, within their stated scope. Proposed wording and unresolved choices remain proposals unless explicitly accepted. The notes supplement the frozen [source-only report](reviews/source-only-report.md) without rewriting its original findings.

The initial phase gathered the user's answers as a batch. After that review, the agent is to reconcile overlapping answers and integrate accepted concepts into the appropriate maintenance, framework, and public documentation sources. The user should not have to repeat settled decisions or direct every placement. Preserve the requirement for individual proposal approval and the prohibition on merging to `develop`; preparing review artifacts does not apply unapproved source changes.

## Reassessment and current review mode

### Expanded Release-Preparation Scope

On 2026-09-11, the user expanded Task 28 to preparation for release. The [updated Task 28 record](../cli-development/tasks/source-framework-review.md) now defines its current outcome, scope, sequence, and completion conditions. The established task path was absent from this older worktree and is prepared here for later reconciliation with the newer shared checkout. The original source review remains completed historical evidence.

The user wants all public Framework and Extension source prose to be streamlined, easy to understand and review, and enjoyable to read. A full discussion of language must precede the general rewrite. Semantics, logic, rules, and their conditions must remain intact. The earlier L2–L4 recommendations are useful examples within this broader work; they are not its complete scope. The workflow readability recommendation was part of the previous language item, despite the unclear final phrasing in chat.

In the language discussion, the user said the proposed predictable structure and natural, conversational prose is close to the desired voice. They strongly preferred “Use examples that are valid for the APIs, formats, and tools they use. If an example is intentionally incomplete, label it as schematic.” They requested two or three larger examples to judge the same approach over substantial passages. Record this as a clear preference for that example and a positive direction for the broader voice, not final acceptance of every proposed rewrite. The [three larger discussion drafts](proposals/release-language-examples.md) compare current Memory, Template, and Managed Delivery passages with proposed wording. Public source remains unchanged by this example work.

The user also authorized a deep scrub of current non-CLI `.agents` content so it reflects accepted current meaning. Working and Emerging records are excluded. In a follow-up answer, the user excluded all Archived records too because useful information has largely been extracted and they may remove the remainder later. This is not deletion authorization. Current rules and templates about these states remain in scope. CLI-owned content remains with other chats even when it lives under `.agents` or `src`.

The user authorized updating the local Observation Template if the packaged shape is better. The comparison found that the local additions repeat current review requirements and add an unsupported broad identifier prohibition. Astra/max aligned the local copy exactly with the packaged Template. Review Evidence retains the continuing requirements. The [alignment receipt](evidence/observation-template-alignment.json) records the exact change and checks. This is explicit template alignment, not merely a language rewrite, and it changes no existing Observation record.

The user confirmed that other agents in other tasks will handle the three CLI follow-ups. Integration preparation follows the source work and template alignment so those implementers can then use the agreed baseline. The no-merge boundary remains in force. The original generated scope inventory was a planning input rather than completed editorial evidence. The retained [Task scope](../cli-development/tasks/source-framework-review.md#scope) defines public prose, current repository review, CLI-owned sources, and excluded Memory records. The [review evidence](evidence/_evidence.md#current-review-summary) records the coverage actually completed.

The user subsequently accepted the larger examples as the desired writing direction while explicitly retaining “self-growing” as a representative descriptor for Memory. They authorized recording the writing approach in its appropriate current source, committing the existing changes on this branch, and making the language update in a separate commit. This supersedes the former no-commit boundary. It does not authorize merging to `develop`, publishing, or taking over the CLI tasks. The created extensions and earlier corrections remain available for the user's Git review.

The user also asked whether category files should state responsibilities or questions and whether opening description prose should be removed when frontmatter already provides descriptions and responsibilities. The [category-opening proposal](proposals/category-openings-proposal.md) records the source convention, comparison, and accepted choice. The user chose option A: show the primary question in each category opening. They subsequently proposed placing the question before its answer. Keep the category name as the level-1 title, then the question as a level-2 heading, followed by the definition and any necessary explanation. The exact presentation is delegated. The ordinary heading adds no metadata field or parser-recognized Framework behavior.

The user explicitly accepted this distinction: a description helps a reader decide whether to open a file; a responsibility helps an editor decide what belongs in it. Preserve the explanation in existing Markdown and maintenance sources, the README, and the loader. Responsibility remains optional when it adds a distinct file boundary.

The README should introduce what Open Forge is, provides, and does. The user initially described ACE as Open Forge's meeting of progressive disclosure and spec-driven development, then asked for a stronger definition of its own, inspired by or improving on those ideas, with more character. Explain the deliberate organization and evolution of workspace context while preserving relevant context selection, explicit accepted expectations, broad applicability, and proportionate specification. Wording such as “builds on” expresses conceptual development without claiming measured superiority. The README supports the Framework; the installed files remain understandable through their own definitions and rules. Both diagrams remain deferred.

### Prior Reassessment And Applied Corrections

The user subsequently requested max-reasoning reconsideration of the prior changes against the complete accepted intent, followed by discussion one item at a time. Source changes must be handled by Astra/max with that context. Dedicated authors or reviewers are allowed where useful. No agent in this chat may take over CLI work; only later implementer tasks may be prepared after source changes settle.

The [max-reasoning reassessment](reviews/max-reassessment.md) finds that the applied changes remain broadly sound, but the earlier claim that steps 1–4 were complete was premature. No source edits were made during that reassessment. T28-RS1 through T28-RS5 have since been approved for worktree application and applied. The identified reassessment corrections are complete within their reviewed scope. User Git review and the planned CLI tasks remain pending. The expanded release-preparation task now supersedes the narrower remaining language pass.

On 2026-09-11, the user approved the exact four-file T28-RS1 proposal for application in this worktree and subsequent Git review. Astra/max applied the Pattern exception boundary to the shipped and dogfood entrypoints, Framework explanation, and maintenance contract. Departures must be explained before dependent work; an exception to an agreed shape uses existing authority or requires a user decision before proceeding. The [application receipt](evidence/t28-rs1-pattern-exception-application.json) records matching approved hashes, inspected prose, authored source/dogfood parity, preserved generated Entries, and a passing `git diff --check`. No CLI work, commits, or merges occurred.

The user then approved T28-RS2. Astra/max applied the exact opening, “Archived Memory keeps useful history that does not govern current work,” to the shipped and dogfood archive entrypoints. The [application receipt](evidence/t28-rs2-archive-definition-application.json) records matching approved hashes, inspected prose, authored parity, unchanged generated Entries, and a passing `git diff --check`. Existing archival rules remain intact.

The user subsequently approved T28-RS3. Astra/max applied the exact paragraph, “Evaluate a Workflow by how its method helps achieve its stated goal. Any collaboration it uses should contribute to that result,” in the general Workflow explanation. The [application receipt](evidence/t28-rs3-workflow-value-application.json) records matching approved hashes, inspected prose, and a passing `git diff --check`. The Council recipe retains its own decision-improvement criterion.

The user then approved T28-RS4. Astra/max applied the reviewed local worktree recipe correction. It preserves uncommitted task output when commits are withheld, keeps required snapshot reviews pending when their prerequisites are missing, gates integration and baseline advancement, and distinguishes integrated delivery from a reviewable handoff. The [application receipt](evidence/t28-rs4-worktree-delivery-application.json) records matching approved hashes, inspected branching and completion conditions, valid recipe headings and step numbering, and a passing `git diff --check`. No CLI work, commits, or merges occurred.

The user then approved applying T28-RS5 so they could inspect the changes in Git afterward. Astra/max applied the exact reviewed [15-file proposal](evidence/t28-rs5-extension-source-split-proposal.diff). It keeps the current Extension architecture at its existing path, extracts useful frozen MVP mechanics into one contextual archive record, aligns current descriptions and references, and scopes the Toolkit Decision to its earlier consolidation while linking the later accepted catalogue. Routed content retains Framework roles; native formats and support files retain their consumers' meaning. The new [CLI documentation task](../cli-development/tasks/extension-lifecycle-documentation.md) remains planned for the user's implementer in another chat. It has not been dispatched. The two new records and their navigation are present in the worktree.

Astra/max drafted the two substantive documents and reviewed the assembled pack. Its companion-architecture finding and the primary's consumer-permission source-scope finding were corrected and rechecked. The [application receipt](evidence/t28-rs5-extension-source-split-application.json) records exact candidate hashes and application checks. The primary also verified all 15 applied hashes and inspected the current architecture, archive, Toolkit Decision, Framework boundary, and new task. The [proposal receipt](evidence/t28-rs5-extension-source-split-proposal.json) preserves the earlier static checks: 275 local link destinations, eight non-CLI fragments, five generated navigation relationships, and archive metadata, with no new or changed link failure. That evidence carries forward through the verified byte identity. The unchanged generated `dist/` reference remains absent in the unbuilt worktree. CLI destination existence was checked, but CLI bodies, behavior, and two referenced CLI fragments were not reverified. No CLI work, commits, merges, or publication occurred during RS5.

## Current implementation state

Updated 2026-09-12. This table records current implementation status; earlier dated discussion and proposal receipts preserve their historical state.

| Item | Current state |
| --- | --- |
| S01 scoped loading | Framework wording applied through P03. Separate CLI implementation task created and indexed; implementation pending with the user's implementer. |
| S02 validation and acceptance | Applied through P02. |
| S03 authority by responsibility | P04 and the approved T28-RS1 correction are applied. The four Pattern sources now preserve the authority boundary for exceptions to agreed shapes. |
| S04 additional authoring example | Declined by the user. No new guide is required. |
| S05 archival | P05 and the approved T28-RS2 correction are applied. Both archive openings now include useful history that was never accepted as current truth. |
| S06 overwrite interpretation | Applied through P06 and architecture alignment: the overwrite shares its base source's role and scope. |
| S07 public orientation and S08 consolidation | Applied through P07 and catalogue integration. README explains the model, links setup, and supports manual use. No diagrams added. |
| Workflow audit and E01–E04 extensions | P08 and the approved T28-RS3–RS5 corrections are applied; see [workflow audit](reviews/workflow-audit.md). Planning/Orchestration architecture remains supported; Council remains local. The reviewed current Extension explanation and historical lifecycle split are applied. |
| Diagrams | Both retained as deferred ideas, explicitly not production-ready. |
| Release language | The user accepted the larger examples and writing standard, retaining “self-growing.” Commit `2b31f4e4` contains the separate rewrite: all 51 public source files read, 31 improved, 20 retained; README and 21 local counterparts also changed. Focused review and static checks passed. |
| Current-content review | 182 files assessed for non-CLI meaning, with one additional candidate excluded as CLI-owned. Commit `e91d2934` contains the current-content corrections and task records. Mixed CLI sections and stored Working, Emerging, and Archived records remain excluded. Three CLI-history link repairs are assigned to the lifecycle documentation task. |
| Category questions and introductory explanation | Applied to 21 public categories and their local counterparts: category title, question heading, then definition. Description serves selection and responsibility serves editing boundaries. README and Vision give ACE its own definition through deliberate context shaping and maintenance, building on progressive disclosure and specification-driven development. The 52-file pack passed focused review after one canonical-list correction. |
| Local Observation Template | Compared and aligned exactly with the packaged source under the user's specific instruction. Continuing review requirements remain in Review Evidence. |

Commit `0eea5e65` records the earlier accepted source corrections, Extensions, Observation Template alignment, and writing approach. Commit `2b31f4e4` contains only the language pass and its matching local copies. Commit `e91d2934` reconciles current repository guidance and knowledge. Commit `28cac0fc` records the category/metadata/ACE follow-up and final task bookkeeping against that private baseline; its [review receipt](evidence/category-metadata-ace-review.json) records the final result. Nothing has been merged to `develop`.

The user's earlier batch instruction authorized completing the accepted substantive corrections sequentially. The latest direction establishes the expanded release-preparation work above. It requires a language discussion before the general rewrite and retains the boundaries on diagrams, CLI implementation, and merging.

Before the user's strengthened no-CLI boundary, the native CLI built successfully and source-package installation, no-op repetition, and explicit removal were exercised. Doctor remained incomplete, also reproduced with the original Toolkit. The [workflow audit](reviews/workflow-audit.md) records that historical evidence and its limits. The catalogue synchronization task records embedded catalogue work and the observed diagnostic gaps. CLI implementation and the new documentation task remain pending outside this review; RS1–RS5 did not repeat those CLI checks.

Review notes, proposals, and selected review evidence now live in routed Memory and are tracked for Git review. The Task 28 record and its CLI follow-up records define current work. Generated logs and temporary test workspaces remain disposable output.

On 2026-09-10, the user authorized proceeding through concrete proposals one at a time and explicitly requested stopping whenever a question needs their answer. The initial sequence began with consolidated framework meaning and a data-flow diagram, followed by existing Workflows and extension designs. The first model draft is now retained as the [detailed diagram idea](../../emerging/ideas/framework-review/detailed-diagram.md), with its subsequent deferral recorded below. Accepted earlier directions remain authoritative within their scope.

The user's P01 correction requests an expanded diagram with transparent dotted group boundaries and named categories, arranged from general groupings to specialized content. Working, Emerging, Crystallized, and Archived all belong to Memory. Avoid “higher Memory” and “lower Memory” as category labels or an authority ranking. This refines the terminology used in the earlier S03 discussion without removing the accepted distinction between validation, acceptance, and integration. Show the default category names and relationships explicitly while preserving their adaptability.

The user then accepted retaining the expanded diagram for now and requested a much simpler companion, understandable on first viewing without reading the README. The [simplified diagram idea](../../emerging/ideas/framework-review/simplified-diagram.md) preserves that draft separately. The simplification must retain the separation of Memory from reusable content, evidence validation from acceptance, and recorded knowledge from category-specific behavior. Its scope is an accessible standalone explanation, not a second exhaustive category map or approval of source changes.

The user subsequently deferred both diagrams and explicitly stated that none are production-ready. Keep their Markdown drafts and Mermaid sources under this review's `ideas/` folder. Earlier approval to retain the detailed diagram must not be treated as production approval. Pause diagram refinement and do not insert either draft into the README, maintenance documents, or shipped Framework. The earlier desire for an eventual data-flow diagram remains recorded, but it does not require completing diagram design before the remaining review work. Accepted conceptual decisions remain in these notes independently of the deferred visual drafts.

On 2026-09-11, the user approved adding P02 and proceeding to the next correction, with Git review afterward. P02 was applied in the isolated review worktree and remains uncommitted. Continue within accepted direction and stop for material questions. The original restriction on CLI edits has not been explicitly lifted, and the prohibition on merging to `develop` remains in force.

## S01 — Scoped eager loading

Recorded 2026-09-09. The user accepted the direction below and said the proposed wording is mostly fine. Exact wording and integration remain to be reviewed.

### Direction for later work

- `LoadNow` and `KeepInMind` share the same scope and parent-loading boundaries. Their distinction concerns reading and continuity refresh timing.
- A `KeepInMind` tag must not cause automatic loading through an unopened parent or inactive scope. Remove the global exception for tagged non-entrypoint files under inactive ancestors.
- Keep eager loading deliberate and minimal. Prefer narrow, on-demand scopes; select them when relevant, then activate their applicable child loading rules.
- Refresh `KeepInMind` content at continuity boundaries while its scope remains active. Do not use continuity refresh to reactivate unrelated scopes.

This direction replaces the original S01 recommendation to retain global leaf loading while suppressing sibling loads. The frozen report remains historical evidence of that earlier proposal.

### Working wording

> `LoadNow` and `KeepInMind` operate through loaded parent routes. Neither tag activates an otherwise unselected ancestor or scope. Keep scoped routes on demand until relevant; once selected, apply their child loading rules. Use eager loading sparingly and place specialized instructions in the narrowest useful scope.

### Integration instruction

Do not simply append this paragraph. Review the existing routing, scope-selection, `LoadNow`, and complete `KeepInMind` rules together. Identify which phrases require replacement, removal, or consolidation so the final text expresses one consistent rule without duplication. In particular, reconcile global leaf reach, ancestor traversal, initial loading, refresh timing, and the activation of a deliberately selected on-demand scope. Check directly affected Memory descriptions and examples for assumptions about global continuity.

Preserve applicable ancestor-rule inheritance, overwrite loading, and scoped authority. Prepare the smallest coherent wording change for the user's review before editing framework source. The user has not yet approved an exact patch.

### Prepared correction and CLI dependency

On 2026-09-11, [P03 — Scoped loading and the CLI boundary](proposals/p03-scoped-loading-proposal.md) prepared the concrete six-file wording patch. Static inspection confirmed that the CLI implements and tests the old global continuity behavior. The user explicitly instructed this chat to apply the wording and create a new task for their implementer in a different chat, keeping the task in this worktree until integration. P03 is now applied with exact candidate hashes verified. The [Scoped Continuity Loading task](../cli-development/tasks/scoped-continuity-loading.md) and its catalog entry are present in the worktree. The known CLI mismatch is recorded as pending implementation. No CLI edits, task dispatch, commits, or merges occurred. This resolves the scope question without reopening the accepted parent and scope boundary.

## S02 — Validation and acceptance

Recorded 2026-09-09. The user accepted the following proposed correction for later integration:

> Validation establishes whether evidence supports a claim. Acceptance establishes which knowledge or decisions may be treated as current within their scope. Restoring or moving a record does not establish acceptance by itself.

This wording supersedes the longer proposed S02 correction in the frozen report. Preserve the existing sources of acceptance, including delegated authority and choices necessarily entailed by an authorized action; this distinction does not require separate user approval for every verified fact.

For later integration, reconcile the `Contextual` definition and Emerging Memory wording with this distinction. Keep the frozen report unchanged. The current request authorizes recording this wording in notes only; no framework source edit or merge is authorized.

### Concrete proposal after diagram deferral

After the user authorized continuing with the remaining work, [P02 — Validation and acceptance wording](proposals/p02-validation-acceptance-proposal.md) prepared the S02 patch. The user approved its application on 2026-09-11. It now places the accepted distinction in the loader, replaces the Contextual and Emerging status lines, and aligns the corresponding dogfood, Framework explanation, and maintenance contracts in the review worktree. All eight applied file hashes match the approved proposal and `git diff --check` passes. The main checkout was not changed. Diagram design remains deferred independently of this correction.

## S03 — Authority by responsibility and deliberate refinement

Recorded and expanded 2026-09-09. The user's explanations settle the conceptual direction below. This replaces the earlier S03 framing and clarifies the difference between Memory preserving accepted knowledge and the root categories defining active behavior. The integrated correction was subsequently applied through P04 on 2026-09-11.

On 2026-09-11, the user expressed general agreement with the concrete recommendation to narrow default replacement to the corresponding role and accepted scope, clarify Memory records' knowledge authority, and integrate each durable outcome into the source defining that part. Preserve immediate scoped authority of recorded user direction without turning the record into another category. This supports proceeding with the coordinated correction; it does not make unprepared source patches or deferred diagrams final.

[P04 — Authority by role and scope](proposals/p04-scoped-authority.md) now applies that coordinated correction in the review worktree, including shipped rules, dogfood, the Memory and acceptance explanations, and maintenance contracts. The main checkout is unchanged. Exact file identities and the bounded diff are retained with the application record.

### Accepted direction

- Information flows from lower levels of Memory through higher levels of Memory. Relevant outcomes from higher Memory then flow into the appropriate root categories, where the category gives each source a precise meaning. Refine and classify the outcomes rather than copying the same record into every destination.
- Memory backs up useful knowledge, evidence, choices, and reasoning. A description of a Directive stored in Memory remains Memory; it is not an active Directive. Required behavior must reach the applicable Directives source. The same distinction applies to Guidance, Patterns, and other categories.
- Memory can preserve authoritative accepted knowledge and user direction within the question it answers. A Decision records what was accepted and why and backs the corresponding Evergreen documents. These review notes are authoritative for the user's accepted answers. That authority does not turn a Memory record into a Directive or give it another category's behavioral force.
- Authority is multi-tiered and specific to responsibility and scope. A Directive has authority as a Directive, not as a Pattern. Each source defines only its own slice; related sources support and link to one another without becoming competing complete accounts.
- Keep category meaning narrow and consistent even when its subject scope is broad or reusable across domains. Directives define required behavior (`must`). Guidance defines recommended approaches (`should`). Patterns define reusable, inspectable shapes that make related work easier to write and review.
- A Pattern's predetermined shape helps reveal omissions, logical mistakes, and departures from established structure. It also gives an explicit basis for deciding whether an exception is justified. Its role must remain distinguishable from a Directive's required behavior.
- Knowledge passes through multiple deliberate sieves as it is evaluated and integrated. Preserve evidence validation, acceptance, and extraction into current knowledge, required behavior, advice, or reusable shape as distinct judgments.

### Questions owned by each source

| Source | Its slice of meaning |
| --- | --- |
| Decision | What was accepted and why; the accepted choice and supporting rationale. |
| Evergreen document | What is currently true about its defined subject and must stay aligned with accepted meaning. |
| Directive | What behavior is required in its applicable scope. |
| Guidance | What approach is recommended, when it fits, and relevant reasons or tradeoffs. |
| Pattern | What reusable shape should guide related work so results are easy to write, inspect, compare, and review. |

Decisions and Evergreen documents may themselves be higher-level Memory. Their knowledge authority and supporting relationship must remain clear without giving them the runtime role of another category.

### Conflicts and exceptions

Make conflicts and departures from applicable accepted sources visible. At minimum, inform the user and request the necessary exception before proceeding with work that depends on an unresolved conflict or unapproved departure. Do not silently ignore an established requirement or agreed shape. Preserve the difference between mandatory Directives, recommended Guidance, and Pattern defaults when explaining what is in conflict and which exception is needed.

Where tooling can identify a concrete structural violation, an error may be appropriate. The user has not selected a new CLI error contract or mechanical enforcement mechanism; do not invent one from this general conflict-handling direction.

### Framework data-flow diagram and documentation

- Include a diagram of the framework's information flow in the README during later integration.
- Show refinement through lower and higher Memory, the supporting relationship between Decisions and Evergreen documents, and the flow of relevant accepted outcomes into the precisely defined root categories.
- Make category meaning and scoped responsibility visible. Do not depict moving or tagging a file as sufficient to establish acceptance or turn a Memory record into an active instruction.
- Derive the README explanation from the integrated framework concept and the appropriate maintenance sources. Consider a corresponding diagram or its defining explanation in maintenance documentation where it belongs; exact placement and reuse remain an integration choice.
- Keep each maintenance document within its own slice of truth. Reconcile overlapping answers before updating public explanations so the diagram and prose describe one consistent model.

### Integration instruction

Rework S03 around the responsibility of each source and the refinement flow between sources. Explain how a Decision supports an Evergreen document, how accepted behavioral outcomes reach Directives, Guidance, or Patterns as appropriate, and how affected sources remain aligned. Preserve the authority of accepted knowledge and user direction while making clear that Memory does not activate another category's rules by storing them.

Describe the sieves and their conditions proportionately. The exact transition wording remains a proposal; this note does not establish a fixed sequence through every route, additional acceptance gates, or repeated user approval for the same decision. Do not copy raw records into every destination or make one document define unrelated slices of truth.

After the answer-gathering batch, reconcile these directions with S01 and S02 and prepare the smallest coherent changes across the affected authority, Memory, source-ownership, maintenance, and README passages. Integrate each accepted concept into the source that owns it; do not append the entire discussion to every file. Keep the frozen source report unchanged. The current action is recording accepted direction only; no framework source edit, commit, or merge occurs in this phase.

## S04 — Authoring and repair example

Recorded 2026-09-09. The user confirmed full manual route maintenance and declined the proposed additional authoring/example file.

### Accepted direction

- Manual route maintenance is fully supported. The framework must remain complete and usable without the CLI.
- The CLI is a framework accelerator. Encourage its use for better results and less manual work, while keeping it optional and preserving the framework's independent meaning and usability.
- Keep the shipped file set small. Do not add the proposed on-demand authoring guide or a separate route-creation/repair example file for S04.
- The loader should explain the basic framework sufficiently while remaining minimal. Prefer clear existing definitions and compact scoping explanations over repeating their combinations in a new tutorial.
- Protect the startup context budget when integrating the accepted review changes. The user estimates the framework's current `LoadNow` context at roughly 8K tokens and considers it acceptable but already toward the upper end of the desired size. This is the user's qualitative assessment, not a measured receipt or a newly imposed exact token ceiling.
- Existing concepts and metadata are composable. The user reports that agents have successfully assembled them in practice, and CLI route-creation support already helps with navigation maintenance. A further example does not currently justify its file and context cost.

### Disposition and integration

The additional example/guide recommendation in the frozen S04 finding is declined. Preserve the report as the original assessment; do not treat the absence of that example as an unresolved implementation requirement.

During later integration, retain explicit manual support and the CLI's optional accelerator role in the appropriate existing sources. Reconcile and condense wording instead of adding a tutorial or expanding the loader by default. No new metadata, routing mechanism, command behavior, or recovery policy is approved here.

## S05 — Archival and category transitions

Recorded 2026-09-09. The user accepted the general archival direction below and rejected the proposed special treatment of sealed handoffs. This replaces that part of the frozen S05 proposal. S01 already removes the original concern about global loading of archived continuity files.

### Accepted direction

- Archive material that should no longer be current. Before archiving, extract what remains useful as current knowledge into the appropriate current sources. Update one or more existing sources where they fit; create a new source when its distinct slice of truth requires one. Archive the remaining material worth retaining.
- Archival does not require keeping every record intact or preserving every detail. Depending on the material and its future value, retaining it as it stands may be useful, or extraction, consolidation, reduction, and transformation may be appropriate. Do not make lossless preservation the universal archival rule.
- A category transition changes the rules that apply to the record. Once archived, the record is governed by the archive category. Behavioral restrictions from its former category do not remain active merely because the record originated there.
- Historical content may be transformed within the archive's rules. In the user's example, sealing constrains a record while it serves its transfer role; it does not make the subsequently archived material permanently immutable. This example explains the decision and must not become a named special case in the general archival rule.
- Prune metadata that would continue to activate former behavior or assert current authority after archival, whether defined by Open Forge or by the user. Keep metadata appropriate to the archived role. The user left open whether this consequence needs an explicit metadata sentence or is sufficiently expressed by the general category-transition rule; decide that wording during integration.
- Archived material may also be deleted when no longer needed, according to the user's direction or accepted retention preferences. Archiving does not create an obligation to retain everything forever or blanket permission for indiscriminate deletion.
- Users may customize their use of Memory, including ignoring or actively maintaining candidate material. Do not impose one universal retention or capture practice regardless of their chosen categories and needs.

### Generality and writing

Keep general rules independent of particular removable or extensible record types. Category meanings are precise while applicable, but a named default record type must not become a protected permanent concept merely because another rule mentions it. Express the archival rule through current meaning, extraction, destination, retention, and the rules of the destination category.

The public-facing writing directive and its Writing Standard were reread for this follow-up. In particular, the standard limits references to named routes to relationships that materially affect behavior or selection. Apply that discipline when integrating the user's direction. Editorial rewrites preserve accepted meaning; an authorized archival transformation is a lifecycle operation and need not preserve every detail of the former record.

The user did not settle whether the loader is an exception to the broader removability principle. Do not use the tentative remark about the loader to introduce a new immutability guarantee. It does not block the accepted archival direction.

### CLI and deferred idea

The user stated that the CLI currently has no archival command and does not favor adding one. No new archival command is requested or approved. Possible assistance with metadata triage was mentioned only as an idea; keep it contextual and do not expand this review into CLI implementation. The statement about the current command surface was not independently verified in this follow-up.

### Integration instruction

Replace the earlier immutable-handoff exception proposal with concise, general archival and category-transition wording in the existing appropriate sources. Align current-content extraction, archive retention, metadata, and the ending of former category behavior. Reconcile any directly affected narrower lifecycle rules so they do not silently preserve a former role after transition. Preserve each source's own slice of meaning and avoid duplicating the complete archival policy across record types.

Keep the frozen report unchanged. Record and batch these accepted directions now; source changes, CLI work, commits, and merges are not part of this follow-up.

## S06 — Overwrite interpretation

Recorded 2026-09-10. The user explicitly accepted this wording and confirmed that it expresses the intended existing meaning:

> Interpret an overwrite as part of its base source, within that source’s role and scope.

Treat this as a clarification of the existing overwrite mechanism, not a new capability or authority layer. Integrate it concisely with the existing overwrite rules rather than adding a separate file, special category, or repeated explanation. Preserve the existing limits on which corresponding content an overwrite replaces. No source edit or merge occurs during this note-gathering phase.

## S07 — Public orientation and setup documentation

Recorded 2026-09-10. The user accepted handling orientation within the already planned README work: explain the framework, show its data flow, and link to the existing setup documentation after verifying it. No additional shipped guide or loader expansion is needed.

Installation is a one-time user setup concern; updates likewise belong to user-directed lifecycle maintenance. These are not routine agent-context concerns and should not consume the framework's normal startup loading. Keep their explanations in the relevant user-facing documentation.

During later integration, reconcile this direction with S03's diagram and maintenance-source requirements and S04's optional CLI accelerator model. Verify actual setup links and consolidate existing documentation rather than inventing commands or duplicating installation instructions in the installed framework. The original source-only evidence gap is not proof that repository-wide setup documentation is missing.

This resolves S07's conceptual direction. The current phase records the answer only; it does not perform installation, updates, source edits, or merges.

## S08 — Concise wording and duplication

Recorded 2026-09-10. The user accepted the editorial consolidation approach:

- State each rule in the source responsible for it.
- Keep the context each independently loaded source needs.
- Remove repetitions that add no meaning.
- Preserve requirements, recommendations, conditions, exceptions, and scope.
- Replace development-history language such as “replacement CLI” where it has no continuing user-facing meaning.

Apply this pass while integrating the accepted answers, with particular attention to the startup context budget, scoped loading, precise category meanings, and general lifecycle wording. The original sample rewrites are illustrative rather than approved final patches; reconcile them with the subsequent accepted directions before use. Concision must not weaken required behavior or obscure a source's independently necessary meaning.

### Final Markdown language and comprehension pass

Added by explicit user request on 2026-09-11. After the substantive changes in this review batch, perform a separate final pass over the repository's Markdown files for language, expressiveness, and ease of understanding. This extends beyond checking only the lines changed during implementation.

Inventory repository-owned `.md` files, including hidden scopes: public documentation, shipped Framework and Extension content, dogfood rules and roles, maintenance explanations, and current working records. Use each source's audience and purpose to judge how much context it needs. Account explicitly for generated content, frozen review evidence, historical records, third-party files, and build output rather than treating all discovered Markdown as freely rewritable authored prose. Preserve frozen evidence and historical quotations; maintain generated regions through their defining sources and supported mechanisms.

Make the meaning easy to understand on first reading. Check direct language, precise and expressive wording, consistent terminology, sentence structure, headings, useful examples, ambiguous references, unnecessary repetition, and the context each independently loaded source needs. Apply the public-facing writing standard. Preserve each source's own slice of truth, requirement strength, authority, scope, conditions, exceptions, uncertainty, and accepted decisions. Surface an unresolved meaning question before rewriting text that depends on its answer.

Perform this final review after substantive changes settle so it evaluates their combined result. It supplements ordinary care while editing and must not silently introduce new behavior, reinstate deferred diagrams, or inflate the loader. Keep changes in the review worktree for the user's Git review, verify affected links and examples proportionately, and retain the prohibition on merging to `develop`. Status: required later in this batch, not started.

### Source-review status

The conceptual dispositions for S01–S08 are now collected. They include accepted corrections, replacements of earlier proposals, and the declined additional S04 guide. Keep the frozen source report as evidence of the initial assessment and use these notes for the user's accepted integration direction. No framework source change or merge has been made during this discussion.

## E01 — Optional review workflow

Recorded 2026-09-10. The user supports trying to develop a review workflow if evidence in the dogfood supports it. This is conditional support for a trial, not unconditional approval to ship a new package or adopt the local orchestration system.

### Evidence assessment

The follow-up inspected concrete dogfood observations, a command-audit finding register, and the subsequent correction/recheck record. They support a bounded trial: reviews found semantic and evidence gaps, stable finding IDs connected corrections and rechecks, and one documented review correctly returned no actionable findings. They do not establish comparative cost, a general quality guarantee, or the need for coordinated multi-agent review.

The actual development toolkit already includes a review workflow covering most of the proposed behavior. The smallest supported refinement makes the reviewed baseline explicit, adds stable IDs for material findings, and describes disposition, revalidation, and targeted rechecks after authorized corrections. Preserve the existing evidence requirements for all findings. Do not add fixed correction limits, model assignments, budgets, automatic review stages, or a separate evidence Pattern without additional justification.

The [dogfood evidence assessment](../../emerging/analysis/framework-review/e01-dogfood-evidence.md) records source locators, observed results, limitations, and the proposed trial. A [complete draft](../../emerging/ideas/framework-review/review-workflow-draft.md) and [small diff](../../emerging/ideas/framework-review/review-workflow-proposal.diff) make the candidate concrete for later integration and review. They remain artifacts; package source, installed dogfood, roles, and Core are unchanged. No trial execution, publication, or merge occurred.

Retain this candidate for the later development batch under the user's conditional direction. Further conclusions must follow actual trial evidence; the draft is not evidence of its own effectiveness.

### Proposal retained for refinement

Recorded 2026-09-10. After reviewing the evidence assessment, the user explicitly asked to keep the small proposal and said they will also try to refine it. Retain the current draft and diff as the starting candidate. This accepts retaining the proposal for refinement, not treating its exact wording as final or expanding its scope. No trial or package-source change has occurred.

## E02 — Compact task and plan starters

Recorded 2026-09-10. The user first proposed a good, boring, predictable structure that agents use for plans, tasks, backlogs, and task working memory if repository use supports it. After discussing the evidence and responsibilities below, the user explicitly accepted adding this as a minimal planning workflow in an extension and delegated analysis of whether supporting Patterns or Directives are needed. The accepted capability is broader than two isolated templates; exact implementation details remain to be developed within this direction.

### Dogfood context inspected

The current working catalogs expose a backlog, a program plan, task records, and a resumption checkpoint. The program plan records actual dependencies and changed execution direction; Task records preserve bounded scope, evidence, findings, and corrections; the checkpoint identifies ongoing work and what comes next. The E01 evidence assessment also traces a material finding through two corrections in its Task record. These are concrete uses of durable work state, although they do not establish that the present file layout or full field set is optimal.

Sources inspected for this follow-up: `.agents/memory/working/_working.md`, `.agents/memory/working/cli-development/_cli-development.md`, `.agents/memory/working/backlog.md`, the opening/current-continuation sections of `.agents/memory/working/cli-development/plan.md` and `.agents/memory/working/checkpoints/cli-development.md`, and the checkpoint catalog. The Plan and Checkpoint repeat lengthy completed-task summaries, so extracting a useful structure must also address duplicate mutable state and accumulating history. This is a targeted reading, not a complete review of the active program records or evidence of measured efficiency gains.

### Initial recommendation and accepted responsibilities

Develop one optional work-management Pattern, with matching copy-ready Templates where they help adoption. The Pattern defines each record's inspectable shape, relationship, and source of current state; Templates supply starting content. Keep these roles consistent with S03. A package boundary, additional Workflow, or fixed number of shipped files is not decided here.

| Record or section | Question it answers |
| --- | --- |
| Backlog | What work remains to consider or select, and what is its current priority? |
| Task | What outcome is required, within what scope, and what establishes completion? |
| Plan | What steps and dependencies lead to the outcome, and how will they be verified? |
| Current task state | What has been done, what evidence and unresolved questions matter now, and what is the next action? |

For small work, keep the outcome, short plan, and current state in one Task record. Separate a Plan, backlog, or resumption record only when it answers a distinct question usefully. Every mutable fact has one defining source; other records link to it rather than maintaining competing copies. Keep completed history from overwhelming the current execution view, and integrate durable outcomes using the accepted Memory and archival rules.

The agent should use the predictable structure when the optional method is selected and the work warrants durable state. Users may adapt, extend, or remove it. Do not make these record types permanent Core categories, require every task to create all records, duplicate an existing external task system, or import this repository's model roster, numeric identity scheme, completion-grace counters, or orchestration ledgers by default.

Verify that another agent can locate the current outcome, dependencies, evidence, and next action without reconstructing the conversation or reconciling duplicate state. The user has now approved adding the minimal planning capability to the later development batch. No template, Pattern, or package source was changed during the discussion.

### Role and package analysis

The actual `development-toolkit` already contains `content/.agents/workflows/planning.md`. Its seven steps cover accepted scope, dependencies, executable steps, verification, and one authoritative task-state source. The inspected template catalog has no Task, Plan, or Backlog leaf. Build on the existing Planning Workflow rather than introducing a competing planning recipe or assuming a new package ID is required.

Use the Workflow for the process: select the records needed, plan the work, update the relevant state, and close or transition records. Use one concise Pattern for the shared inspectable record shapes and relationships that planning, execution, resumption, and review must understand. Add matching Templates only where copy-ready starting content saves work; do not duplicate the full Pattern in each Template.

A new Directive is not presently justified. Existing Core rules define authority, scope, and Memory lifecycle, while the selected Workflow and Pattern supply this optional method's process and shapes. Do not promote optional work-management practices into universal binding instructions. If development exposes a distinct mandatory rule that the existing sources cannot express, assess that concrete requirement separately instead of adding a Directive preemptively.

Keep the accepted small-work form: one Task can contain a short plan and current state. Separate records only for useful distinct responsibilities, and keep each mutable fact in one defining source. The user delegated this structural analysis; routine placement and proportionate file selection need not be sent back as repeated confirmation questions. Publication, installation, and merges remain outside this approval.

## E03 — Bounded independent deliberation

Recorded 2026-09-10. The user agreed with the proposed optional council method and the condition that dogfood should show materially improved decisions. Assess actual contributions such as a useful counterexample, changed recommendation, or clarified consequential tradeoff; council invocations and agreement counts alone do not establish value.

Keep the method bounded to one consequential decision, with genuinely independent initial perspectives when the runtime supports them, evidence-based comparison, preserved material disagreement, and acceptance by the authorized decision-maker. The exact recipe and distribution remain subject to the evidence assessment and broader workflow review below. No council was run or packaged during this discussion.

## All current Workflows — Review and improvement scope

Recorded 2026-09-10. The user explicitly extended the later development work to reviewing all current workflows and improving them where warranted. They expect substantial improvements may be needed. This includes current shipped workflows and repository dogfood workflows, not only E01–E03.

Inventory the current sources and compare shipped content with local variants before changing them. Establish the purpose and reusable value of each workflow, its selection conditions, steps, completion criteria, evidence, dependencies, and relationship to other categories. Check for duplicate recipes, unnecessary stages, model/runtime coupling, drift, and process that adds little beyond normal agent behavior.

Use the accepted framework directions and actual dogfood evidence to decide what to retain, simplify, revise, consolidate, or propose retiring. The expectation of substantial work is not a required amount of rewriting or an assumption that every current workflow is defective. Local experiments and archived workflows remain evidence in their stated roles and do not become shipped defaults automatically.

Preserve the small E01 proposal for the user's further refinement. Include the accepted minimal planning capability from E02 and the evidence-conditioned council direction from E03. Keep workflow procedures, reusable shapes, copy-ready content, and binding instructions in their respective categories. Do not duplicate Core authority or introduce unnecessary fixed record types.

This broadens the agreed development batch; the present phase still records the user's answers. Publication, global installation changes, and merges are not authorized. The prohibition on merging to `develop` remains in force.

### Workflow value and relationship to Skills

Recorded 2026-09-10. After discussing the overlap between Skills and Workflows, the user agreed to improve the Workflows while retaining the category. Keep the existing Goal, Steps, and Completion shape. Evaluate each recipe by asking what useful behavior would become less predictable if it were removed. A useful Workflow preserves a deliberate method or addresses a recurring execution problem through meaningful decisions, conditions, and observable completion criteria. Generic restatements of ordinary competent agent behavior may not justify a separate recipe.

Skills can also contain procedures, so do not invent a strict distinction based on the presence of steps. Review the responsibility and delivery purpose of each source. Where a Skill and Workflow serve the same method, prefer one defining procedure with appropriate references rather than independently maintained copies. No new Skill wrapper is required merely because a Workflow exists. These criteria guide the agreed audit; they do not establish that a particular current recipe should be retired before inspection.

## E04 — Orchestration extension

Recorded 2026-09-10. The user accepted an optional orchestration extension, combining managed worktree delivery with useful orchestration roles such as Overseer and Mastermind. They left open whether those roles should remain separate, be combined, or share a common definition with specializations. This accepts the capability and its development direction; it does not approve the existing local hierarchy wholesale or settle the role architecture.

Include explicit task ownership, shared requirements before dependent work begins, isolated workspaces where needed, preservation of unfinished work across interruptions, a defined integration boundary, and verification of the combined result. Evaluate the roles and workflow together in the broader workflow improvement work.

### Role analysis and provisional recommendation

The inspected local role boundaries distinguish project authority from task-local execution. `.apm/agents/overseer.agent.md`, under “Internal Hierarchy” and “Authority And Change Control,” owns cross-task contracts, priorities, integration policy, and acceptance within the user's authority, while allowing ordinary work to proceed directly. `.apm/agents/task-mastermind.agent.md`, under “Task Mastermind” and “Start,” owns one bounded task and its local architecture without changing accepted project meaning. `.apm/agents/integration-mastermind.agent.md`, under “Integration Mastermind” and “Start,” owns one integration boundary without acquiring project-wide authority. These are source-defined responsibilities, not evidence that the current number of agents or prompt layout is optimal.

The current recommendation is to preserve these distinct scopes while sharing the common orchestration method. A project role maintains accepted direction, cross-task responsibilities, and integration decisions. A task role owns one delegated outcome, implementation continuity, and evidence within that boundary. One agent can perform small work directly; separate task owners become useful when delegation, isolation, or parallel work warrants them. A separate integration role should be conditional on the integration work benefiting from its own owner.

Compare separate roles with shared guidance against a common role with explicit specializations during architecture work. Do not assume an inheritance mechanism or new runtime adapter is needed. Whatever representation is chosen must make each active role's authority and escalation boundary explicit and avoid repeating common process across large role definitions. Exact names, files, package boundaries, and runtime projections remain design questions.

Reuse E02's planning and current-state structures and E01's evidence-focused review where applicable. Avoid a second task-state system, mandatory coordinator layers for small work, or importing local model assignments, budgets, counters, and experimental stages without a reusable reason. Keep the orchestration capability optional and its content aligned with each framework category's narrow responsibility.

### Accepted delivery flexibility

Recorded 2026-09-10. The user agreed to try the proposed shared orchestration method with distinct responsibility scopes. Delivering the method entirely through Workflows is acceptable. Including agent roles is also acceptable, with those agents supplied through APM. Agents are an available delivery option, not a required component of the extension.

Choose between these forms during architecture work based on the useful responsibility boundaries and what the existing mechanisms can support. Keep the method's process in Workflows; if APM agents are included, give them their scoped role responsibilities and references to the shared method rather than maintaining competing copies of its procedure. This records acceptance of the design direction and both delivery options, not final approval of particular role definitions or an instruction to launch agents now.

This completes the initial extension discussion dispositions. E01 remains a retained draft for refinement, E02 is accepted for development, E03 remains conditioned on useful decision evidence, and E04 is accepted with role composition unresolved. Continue to present concrete proposed changes for the user's individual review. No agents were launched, roles or package sources changed, or integration performed in this discussion. The prohibition on merging to `develop` remains in force.

## Follow-up: README Voice Accepted; Extension Reassessment Deferred

The user accepted the natural creator-voice introduction as a starter they may edit manually. They subsequently found “We started with ideas” forced and asked for more eloquent, varied phrasing, including passive constructions where natural. The separate Project Voice maintenance guide records that refinement alongside shared Writing Standard accuracy requirements.

The user explicitly asked to save Extension concerns for later and finish READMEs and the public-facing writing guide now. Earlier broad completion statements do not close the missing E02 starters: the Toolkit has Planning and Work Records, but no Task, Plan, Backlog, or Checkpoint/current-work Templates and no distinct Planning package. The tracked Task 28 record now preserves these gaps, the installed-layout dependency explanation for Managed Delivery's links, the pending dependency-closure audit, and the request to consider smaller targeted packages. No package split or new boundary has been accepted.

The latest refinement favors a personal origin story beginning “Inspired by…” and asks for fewer comma-separated feature lists. The README and Project Voice guide now develop this into connected prose.
