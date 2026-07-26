---
open-forge:
  description: Clean-context brief for establishing Open Forge vision and architecture before resuming source migration
  tags: [KeepInMind, Memory, Handoff, Contextual, Framework, Vision, Architecture, Migration]
---

# Vision And Architecture Reset

## Status

- Source migration is paused before [`src/open-forge/.agents/loader.md`](../../../../src/open-forge/.agents/loader.md).
- The `AGENTS.md` gate was accepted in `0ff4725`; the `CLAUDE.md` gate was accepted in `d881453`.
- No loader implementation from the paused review was kept.
- Speculative edits to the current vision, architecture, directives, backlog, and migration considerations were removed before this handoff.
- The clean [ACE product vision](../../crystallized/documents/vision.md) was accepted and committed by the maintainer as `cde98b5`.
- The superseded pre-reset vision is preserved as [historical context](../../archived/vision_old.md).
- The clean [top Open Forge architecture](../../crystallized/documents/architecture.md) was accepted by the maintainer for progression on 2026-07-26.
- The superseded pre-reset architecture is preserved as [historical context](../../archived/architecture_old.md).
- The approved discussion baseline is preserved in the [Open Forge Design Baseline](../sessions/2026-07-26_open-forge-design-baseline.md).
- The [Framework Architecture](../../crystallized/documents/framework/architecture.md), [CLI MVP Architecture](../../crystallized/documents/cli/architecture.md), and [Extensions MVP Architecture](../../crystallized/documents/extensions/architecture.md) were drafted from that baseline on 2026-07-26.
- The complete vision and architecture set is now the open approval gate.

## Next Task

Review the accepted [vision](../../crystallized/documents/vision.md), [top architecture](../../crystallized/documents/architecture.md), and drafted [Framework](../../crystallized/documents/framework/architecture.md), [CLI MVP](../../crystallized/documents/cli/architecture.md), and [Extensions MVP](../../crystallized/documents/extensions/architecture.md) architecture views together.

Treat the [design baseline](../sessions/2026-07-26_open-forge-design-baseline.md), accepted [vision](../../crystallized/documents/vision.md), accepted [top architecture](../../crystallized/documents/architecture.md), and current maintainer direction as the architecture authority for this pass. Treat older decisions, governance, source, and implementation as migration inputs rather than proof or constraints.

Do not resume source migration until the architecture set is approved. After approval, perform any requested ownership tightening, then resume the source migration at the paused loader gate.

This handoff temporarily preserves accepted constraints that do not yet have trusted permanent owners. The vision and architecture pass must assign each accepted absolute to exactly one appropriate loader Axiom, scoped Axiom, directive, pattern, current document, or decision, then remove it from this temporary handoff when the transfer is complete.

## Discovery State

### Accepted Direction

- Open Forge is the larger adaptive human-agent working environment; its small generic Core is the substrate at the center rather than the whole product.
- The current candidate summary is: "Open Forge is a user-owned, human-readable, file-native operating layer for Adaptive Context Engineering. It makes workspaces understandable, routes the right context and authority, enables confident autonomy, preserves continuity, and deliberately evolves how work gets done. It does this without imposing a universal methodology or requiring a proprietary runtime."
- Define ACE immediately before that summary: "Adaptive Context Engineering is the deliberate design of a workspace's information, relationships, authority, and retrieval paths so the right context is available at the right time and the environment evolves through use."
- It grows with a project and its needs instead of installing a large opinionated methodology, default workflow catalogue, or review surface.
- The human operator owns direction and decisions. Agents provide informed suggestions grounded in the workspace's accumulated knowledge and perform accepted work without turning normal execution into approval bureaucracy.
- Explicit definitions, stable anchors, inherited meaning, and relationships should make recurring reasoning cheaper and more consistent. Prefer references, anchored relative links, and established tags to restating content whenever possible.
- Give detailed truth one authoritative owner. Permit only small controlled mirrors when an independently complete boundary such as the README, vision, or root harness needs the same exact contract; keep those mirrors visibly synchronized.
- Human-readable Markdown remains the complete semantic contract. The CLI is a first-class deterministic reasoning accelerator and safety tool that makes correct loading, navigation, validation, and composition cheap enough to become the default behavior without privately owning meaning.
- Design for capable contemporary agents that can reason over routed context, inheritance, tools, and declared authority. Do not inflate the framework to compensate for obsolete low-capability models.
- Development is the proving ground, not the product boundary; the design must support other file-representable disciplines without imposing a development lifecycle on them.
- Structure shapes probability rather than guaranteeing agent behavior. Deterministic tools validate and cheaply expose the contract without becoming its semantic runtime.
- Use the simplest explicit relationships first: short explanations and relative Markdown links from one authoritative owner. Do not make semantic search or inferred relevance part of the current critical path.
- Generated `Entries` already provide the initial relationship surface: a frontmatter description becomes the linked selection label, the relative path identifies the owner, and tags provide cheap classification and conceptual anchors.
- Encourage established descriptive tags in frontmatter and prose when they improve routing, search, or conceptual association. Tags remain signals rather than a second authority system.
- Future tools such as Rune may exploit the same links, anchors, and tags for exceptionally cheap retrieval without requiring a new relationship model or changing authoritative ownership.
- Growth is structured and logical: capture useful context, preserve its state and authority, promote or demote it deliberately, keep one owner, link instead of duplicating, and prune or archive superseded material.
- The framework must compose for a person, project, team, multi-project workspace, or shared multi-repository source of truth without adding a separate organizational-conflict model.
- Aim for no fixed structural expansion ceiling: route selection and context cost should grow primarily with the depth and number of selected scopes and relationships rather than the total number of projects, repositories, or files.
- Unselected sibling scopes should add almost no active-context cost. Two separately routed projects may share one Open Forge environment without polluting each other's ordinary work, while an integration task can deliberately select both.
- Install sensible removable starter routes that provide expected value for most users and discoveries they might not design independently. Customization may delete, replace, scope, or override them without the normal installer silently restoring defaults.
- Rebuild the README after vision and architecture converge. It should first sell the vision, then provide the quickest useful start, explain advantages and representative workflows or examples, cover advanced and maintainer material later, and show credible adopters or usage evidence when available.
- The README and vision should lead with the same exact accepted vision summary, not simplified alternate promises. Subsequent sections may unpack it.
- Prefer `human-readable`, `file-native`, and `Markdown-first` or `Markdown-native` over `plain-file` in reader-facing language.
- Prefer `evolves` over `improves` when describing adaptation because evolution does not presume every change is intrinsically better.
- Avoid unnecessary audience labels such as `capable humans and agents` in public copy. Describe what Open Forge makes possible, and name operators or agents only when their distinct responsibilities matter.
- Do not use em dashes in Open Forge-authored wording. Use ordinary punctuation and sentence structure.
- Marketing may playfully call Open Forge infinitely expandable or a grow-your-own framework when the surrounding explanation states the precise claim: routing has no fixed structural expansion ceiling, and active context grows mainly with selected scopes and relationships.
- Rebuild the current MVP CLI after the vision, architecture, and source contracts converge. The detailed candidate is owned by [CLI Overhaul](../../emerging/ideas/cli-overhaul.md); the active backlog links to it.
- Treat the current extensions system as an MVP and unexplored architecture that will receive its own overhaul after the top-level product boundaries converge.
- Preserve this discovery's reasoning as well as its outcomes: concise current documents should own accepted state, scoped decisions should preserve valuable why, and an archived session or equivalent source record should retain the richer discussion after the pass completes.

### Architecture Document Direction

- Do not restrict a workspace or scope to one architecture document.
- The top Open Forge architecture owns the complete system map, component boundaries, interactions, and cross-cutting invariants without absorbing every component's internal design.
- The framework architecture includes Memory because Memory is part of the baseline ACE environment.
- Framework, extensions, and CLI may each own a scoped architecture document. Create one when its subject has enough independent design, ownership, or evolution to justify a coherent current view.
- Write the top architecture first. It may name and bound the CLI and extensions while linking to scoped architecture documents once they exist.
- Provisional MVP architecture documents are useful because they make current behavior and redesign boundaries inspectable without presenting that behavior as the accepted final design. Label the current-state and intended-overhaul boundary explicitly.
- Review exactly one architecture or current document per message and approval gate so each document receives focused scrutiny before the next begins.
- The future CLI architecture should own CLI-only commands, flags, lifecycle, transactions, safety, performance, packaging, implementation boundaries, and verification. The top architecture owns only how the CLI relates to human-readable truth and the rest of Open Forge.
- A provisional CLI MVP architecture may first document current behavior and liabilities; a later accepted CLI architecture owns the redesigned command and implementation contract.
- A provisional extensions MVP architecture may first document current composition, ownership, installation, and limitations; a later accepted extensions architecture owns the redesigned capability system.
- Prefer genuine scoped document routes over a fixed mandatory architecture-document taxonomy so the same pattern works for user projects, components, repositories, and disciplines.

### Memory Authority Direction

- Agents may freely maintain working and emerging memory within the task's authority; those states are designed primarily to preserve agent-visible context and candidates.
- Bias toward recording useful contextual information in working or emerging memory when loss would be more costly than a reviewable Git diff, while avoiding indiscriminate raw accumulation.
- Encourage active use of working memory for plans, resumability, and handovers, including bounded transfers among coordinating agents. Its expected expiration requires extraction, archival, or pruning when the active need ends.
- In distributed work, record a concrete observation after one occurrence when it is plausibly reusable, surprising, or costly enough to preserve; later agents should extend the existing observation rather than independently rediscovering it. Require recurrence for promotion, not necessarily for initial capture.
- Durable promotion is governed by accepted direction, not by a ritual approval command. A clear definitive user choice or request may implicitly authorize the matching crystallized decision and affected evergreen current document.
- Tentative language, alternatives under consideration, and unresolved analysis remain emerging rather than crystallized.
- Agents may always suggest durable changes. When acceptance is unclear and dependent work would rely on the result, ask; otherwise preserve the candidate with visible uncertainty.
- Inform the user about durable changes made from clear implicit authority even when redundant reconfirmation is unnecessary.

### Candidate Vocabulary

- `Adaptive Context Engineering` (`ACE`) is the engineering discipline and product category Open Forge establishes and implements.
- `TRACE` and `GRACE` remain deferred family names for possible operating modes; their semantics do not need resolution for the current vision and architecture.
- Keep `adaptive` as the leading expansion of `A`; `agnostic` and `generic` describe design properties rather than the central value proposition.

### Current Questions

- Apply and validate the accepted boundary among scoped decisions, coherent evergreen documents, directives, and archived rationale during migration. The active analysis is owned by [Maintenance Migration Considerations](../../emerging/ideas/maintenance-migration-considerations.md).
- Apply the Framework Architecture boundary between universal mechanics, shipped standard routes, local customization, and extensions during source migration.
- Defer semantic CLI relevance inference; the present requirement is to provide the best practical helper for agents while explicit routes remain authoritative.

## Design Rules To Carry

### Understanding

- A concept is not understood well enough until it can be explained in one or a few short sentences, or one compact bullet list.
- Define each concept once at its clearest owner. Elsewhere, use a short anchor and a relative link instead of restating the definition.
- Prefer associations between a small number of stable terms, tags, and owners over many repeated rules.
- Use one short definition or one compact list, not an introductory paragraph followed by bullets that repeat it.
- Order content from the most important meaning to supporting detail. Introduce every term before dependent rules and make each section build naturally on what precedes it.
- Clarity and unambiguous interpretation outrank brevity. After clarity is achieved, use the minimum words and baseline context needed for maximal useful output.
- State binding rules directly, with explicit scope and required behavior. Do not define a rule or tag only by saying that it behaves like another one.
- Installed wording must be self-sufficient. Do not require readers to remember unnecessary qualifiers such as where a defined tag was defined.
- Use technical terminology only when it adds necessary precision, and define it before use. Prefer ordinary language when a framework term adds no value.

### Product

- Open Forge is a generic, plain-file, grow-your-own framework, not the author's completed methodology.
- Core stays small and generic; native agent competence performs ordinary work.
- Installed routes are starting anchors, not a fixed taxonomy. Users and agents may create scopes, initialize the categories they need below the correct owner, and reuse inherited Axioms instead of copying rules.
- The README must strongly and early communicate recursive customizability, scalability, and user ownership.
- Plain Markdown remains complete without the CLI; deterministic tools make the same contract cheaper and safer.
- The human controls accepted direction. Framework structure should improve agent behavior without turning Open Forge into an approval bureaucracy or claiming mechanical control over nondeterministic agents.
- Add framework behavior only when it is generic or required by Open Forge mechanics; specialized methodology belongs in local routes or extensions.

### Authority, Truth, And Memory

- Clear user direction is accepted within its stated scope without redundant confirmation. Material ambiguity remains #Contextual until dependent work requires clarification.
- A clear request to implement or apply an already settled choice does not require the same choice to be approved again. The broader boundary of operational acceptance still needs concise wording.
- #CurrentTruth identifies accepted current state. #Evergreen independently identifies material that must stay aligned with accepted state; it creates neither authority nor loading.
- Generic #Evergreen behavior belongs with its loader tag definition, not in a seeded truth-maintenance directive and not only in document routes.
- Memory may record any subject, including processes, without activating that behavior. Accepted behavior that should guide future work belongs in the matching #Core route.
- Current documents present coherent current state. Decisions preserve discrete accepted choices and useful rationale without becoming duplicate owners of current behavior.
- Before archiving material, move useful current content to its owning route or external system. Archive only the historical context that remains useful.
- Memory states own their local capture, consolidation, movement, archival, and restoration invariants; generic truth behavior should not be repeated in every state.
- Do not restore a redundant #SourceOfTruth tag.

### Routing And Loading

- Routing is strictly top-down: an already-loaded parent provides enough path, description, tags, and inherited meaning to select a child before its body is opened.
- A loaded ancestor's Axioms remain active below it. A child adds only what is specific to its scope and does not restate inherited rules.
- Every folder in a visible nested route has one entrypoint. Generated `Entries` expose direct children and remain navigation metadata plus explicitly defined loading behavior.
- Custom scopes are a primary scalability mechanism, not an edge case. Decisions, documents, archived memory, directives, patterns, and other categories may be placed below the owner that needs them when the route chain makes the meaning explicit.
- When existing routes would mix distinct ownership or meaning, create or propose a clearer scope instead of forcing the material into an unsuitable default location.
- Relative Markdown links are the preferred ownership and association mechanism and use containing-file-relative semantics like generated `Entries`.
- The accepted direction for #KeepInMind is immediate parent-visible loading plus continuity rechecks. Every #KeepInMind route must be reachable from the loader through #LoadNow or #KeepInMind ancestors; its exact concise wording is not yet approved.
- [`open-forge load --bodies`](../../../../docs/cli.md) should remain the one-command ordered context stream for the loader, baseline routes, continuity routes, and adjacent overwrites.
- The loader should present `open-forge load --bodies` first among its CLI entries and explain that it returns those files as one ordered, route-labelled context block.
- Doctor should detect invalid route ancestry and broken local Markdown paths, name the exact defect, and suggest bounded fixes. Automatic mutation must be explicit and limited to unambiguous safe corrections.
- Whether every installed root category deserves #LoadNow must be reviewed at that category's own source gate.

### Root Entry And Harnesses

- `AGENTS.md` is the canonical workspace entry and must remain extremely small: identify Open Forge as the operating contract, require the loader before any task, and require applicable rules throughout the task.
- Detailed authority, routing, loading, tag, and conflict behavior belongs in the loader rather than being repeated across harness entry files.
- Harness files such as `CLAUDE.md` are minimal bridges to the canonical entry, not independent policy owners.
- Managed blocks must preserve workspace-owned content outside their markers.

### Source And Governance

- Users receive `src/open-forge/`; runtime behavior must be understandable from installed files alone.
- The repository dogfoods accepted shared behavior so later agents can recover it.
- The accepted product vision and architecture are current documents owned under `.agents/memory/crystallized/documents/`, not source payload files.
- Current maintenance documents belong under crystallized documents and follow the existing [maintenance contract pattern](../../../patterns/open-forge/maintenance-contract.md).
- Apply #CurrentTruth and #Evergreen to maintenance documents only when each tag's independent meaning is actually true.
- Governance describes a source's contract, relationships, and verification. It links to runtime owners and does not copy source bodies or become a second wording owner.
- Maintenance contracts use `Source`, `Contract`, and `Verification`, with concern-specific subsections only when they clarify a real additional contract.
- Relationships belong beside the contract they affect and should be included only when they create a real maintenance consequence.
- State complete positive requirements where practical. Use negative constraints only when they close a specific risk more precisely.
- Current documents, maintenance documents, decisions, source files, code-owned truth, README material, and archives must have distinct ownership roles rather than becoming parallel summaries.
- Create only current documents that provide a coherent valuable view; do not fill a taxonomy with sparse documents.
- The framework should make directive, pattern, decision, and current-document ownership natural by design rather than requiring another classification reminder.

### Migration And Review

- After vision and architecture are accepted, review `src/open-forge/` top-down, one source file per approval gate.
- For each file, inspect its complete contents, at least its two most relevant recent commits, references, consumers, tests, current owners, nearby routes, and relevant legacy governance before proposing changes.
- Before editing, show which files refer to the source, which files would change or be removed, what information would move, and the recommended destination.
- Discuss whether every existing rule, term, section, and format earns its cost; do not treat the previous handover or current implementation as gospel.
- Use a migrate, improve, extract, and remove flow that leaves the workspace cleaner after every gate.
- Keep one coherent diff under review, stop at the gate, and wait for maintainer approval before continuing.
- Register accepted shared changes in the repository's own routes so later agents can recover and dogfood them.
- The maintainer's completed cross-project trials are evidence; do not create a separate baseline exercise merely to repeat those checks.
- Communicate directly and briefly; separate accepted direction from recommendations and unresolved questions.

## Rules Directory Decision

Do not add a dedicated `rules/` primitive. It would duplicate existing binding owners:

- loader and entrypoint Axioms own universal or inherited Framework mechanics;
- directives own independently routed binding behavior.

Patterns, guidance, current documents, and decisions retain their distinct roles. A linked rules map may be created for discovery if useful, but it points to authoritative owners rather than reproducing their contents.

## Existing Owners To Inspect

- [Current product vision](../../crystallized/documents/vision.md)
- [Current architecture](../../crystallized/documents/architecture.md)
- [Framework essence directive](../../../directives/framework-essence.md)
- [Deliberate framework change directive](../../../directives/deliberate-framework-change.md)
- [Product-direction rationale](../../crystallized/decisions/product-direction.md)
- [Routing decisions](../../crystallized/decisions/routing-model.md)
- [Tag decisions](../../crystallized/decisions/tags.md)
- [Source and packaging decision](../../crystallized/decisions/source-and-packaging.md)
- [Active migration backlog](../backlog.md)
- [Migration considerations](../../emerging/ideas/maintenance-migration-considerations.md)

## Unresolved

- Maintainer review and acceptance of the complete vision and architecture set.
- The final loader structure, terminology, authority wording, and #KeepInMind wording.
- How strongly recursive framework-route initialization should be supported by the CLI.
- Doctor's Markdown scan boundary, suggestion ranking, and explicit fix interface.
- The justified baseline-loading budget and which routes earn it.
