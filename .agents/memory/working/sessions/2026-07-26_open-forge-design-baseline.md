---
open-forge:
  description: Approved design baseline distilled from the Open Forge vision and architecture discussion before the Framework Architecture
  tags: [Memory, Session, WorkHistory, Contextual, Framework, Vision, Architecture, ACE, Migration]
---

# Open Forge Design Baseline

This session checkpoint preserves the design baseline approved by the maintainer on 2026-07-26 before the detailed Framework Architecture was written. It is a dated synthesis, not a competing Evergreen owner. The [vision](../../crystallized/documents/vision.md), [top architecture](../../crystallized/documents/architecture.md), and later scoped current documents own the state that continues to evolve.

## Authority For This Pass

The repository before this pass is the implementation and conceptual starting point being migrated. It is not evidence that the new design is correct.

The authority order for the vision and architecture pass is:

1. Current maintainer direction and corrections
2. The accepted vision
3. Accepted parts of the top architecture
4. The discussion and its handoff as the richest reasoning record
5. Existing decisions, governance, source files, and behavior as migration input rather than constraints

A link to an older file means that the subject is currently represented there and must be reconciled. It does not prove the design or permanently assign ownership. Migration must update links and ownership end to end.

## Adaptive Context Engineering

Adaptive Context Engineering is the deliberate design of a workspace's information, relationships, authority, and retrieval paths so the right context is available at the right time and the environment evolves through use.

ACE is an engineering category broader than spec-driven development. It asks:

> How do we shape the working environment so the right information and constraints reliably guide whatever work is needed?

Open Forge is an implementation of that category, not merely another SDD workflow.

TRACE and GRACE may later describe operating modes or family concepts, but they currently add no architectural distinction and remain deferred.

## Open Forge

The accepted summary is:

> Open Forge is a user-owned, human-readable, file-native operating layer for Adaptive Context Engineering. It makes workspaces understandable, routes the right context and authority, enables confident autonomy, preserves continuity, and deliberately evolves how work gets done. It does this without imposing a universal methodology or requiring a proprietary runtime.

This remains the central promise in the vision and future README. Explanations may follow it, but the public introduction should not replace it with a simplified and weaker summary.

## The Problem

Capable agents still behave inconsistently when relevant information is:

- Missing
- Expensive to retrieve
- Duplicated across competing owners
- Mixed with irrelevant context
- Stale or historically superseded
- Ambiguous about authority
- Lost between sessions or agents

The operator then wastes attention repeating settled direction, reviewing avoidable mistakes, and rebuilding workspace understanding.

Large predefined frameworks often respond with extensive processes, roles, prompts, workflows, and required documents. This creates its own onboarding, context, maintenance, and review burden.

Open Forge takes the opposite direction: start small, make relevant context cheap and explicit, and allow the environment to develop its own methodology through actual use.

## Grow Your Own Framework

Open Forge ships a small common foundation and sensible removable defaults. It does not ship the author's completed methodology.

Every installation can evolve differently through:

- Local knowledge and accepted decisions
- Scoped directives and conventions
- Reusable patterns and skills
- Custom workflows
- Project and discipline scopes
- Personal or organizational preferences
- Working history and learned practices
- Extensions where optional reusable capabilities are justified

This growth must be structured, scoped, minimal, and nonduplicative. Adaptive does not mean uncontrolled accumulation or magical self-modification.

The standard routes are the product Open Forge ships. Their expected value should be positive for most users and provide useful structures that many users would not independently design. Users remain free to delete, replace, scope, override, or reorganize them. Normal installation or update must not silently restore removed defaults unless restoration is explicitly requested.

## Broad And Recursively Scalable

Development is the proving ground, not the product boundary.

The same system must be usable for a person, project, team, design discipline, collection of projects, multi-repository workspace, or shared source of truth. A repository containing Open Forge as a submodule for several interacting projects is a first-class use case.

Scopes can recursively contain narrower scopes and whichever framework routes they need.

There is no promised constant-time or literally unlimited performance. The precise architectural claim is:

> Open Forge has no fixed structural expansion ceiling. Active context grows primarily with selected route depth, scopes, and relationships, not with the total size of the workspace.

Two routed projects can coexist without ordinary work in one loading the other. An integration task can deliberately select both.

Marketing may playfully call this infinite scaling or a grow-your-own framework when the exact claim is explained nearby.

## Context And Routing

Context is not everything stored in the workspace. It is the goal-relative selection of knowledge, authority, constraints, relationships, and working state needed to understand and act.

The three-part model is:

- Baseline context establishes how to enter, route, interpret authority, and locate more information
- Continuity context preserves active commitments, handoffs, approval gates, and resumability
- Selected context contains the detailed scopes and relationships relevant to the current goal

Routing remains explicit and top-down:

1. A known entrypoint exposes its direct children
2. Each child provides a relative path, useful description, and tags
3. The agent decides which child is relevant before loading its body
4. Loaded ancestor rules remain applicable
5. Narrower scopes add local meaning without copying inherited meaning

The description acts as the visible relationship label, the relative path identifies the destination, and tags provide cheap classification and conceptual anchors.

The CLI should provide the best practical context helper possible. Whether it later infers likely routes is deferred. Explicit routes remain authoritative even if inference is added as assistance.

## Ownership And Relationships

Every important concept has one authoritative owner.

Related material should use:

- Relative Markdown links
- Links to specific headings when helpful
- Frontmatter descriptions
- Established descriptive tags
- Short local explanations of why the relationship matters

This is sufficient for the initial relationship model. Open Forge does not need a generalized knowledge graph.

Future tools such as Rune may derive semantic, vector, or graph-like retrieval from the same files, links, anchors, and tags. Those derived systems accelerate discovery but do not become semantic authorities.

Duplication is strongly discouraged where a reference can preserve one owner. Small intentional mirrors remain valid at independently complete boundaries such as the vision, README, or root agent entry contract.

### Current Documents And Decisions

A current document answers: "What is true now, and how does it work?"

A decision answers: "What was chosen, and why?"

A current document must explain the accepted concept well enough to use without opening its decisions. When useful rationale exists, it links back to the supporting decision.

A decision records the choice, alternatives, tradeoffs, consequences, and reasoning. It links forward to the current owner that integrates the accepted outcome.

There is limited intentional overlap because both identify the accepted choice. The current document owns the complete current concept. The decision owns the reason.

Directives own binding behavior. Archives preserve useful superseded history without governing current work.

Existing decisions are not automatically accepted rationale during this migration. Each must be revalidated.

## Operator And Agent Authority

The operator owns goals, priorities, consequential tradeoffs, and accepted direction.

Agents should:

- Investigate and gather relevant context
- Offer informed suggestions
- Challenge weak or inconsistent ideas
- Execute accepted work confidently
- Verify results
- Preserve useful discoveries and continuity
- Avoid turning ordinary execution into approval bureaucracy

Clear direction is accepted within its stated scope without redundant confirmation.

Examples:

- "Consider architecture B" remains Emerging
- "Both are possible; investigate B" remains Emerging
- "I definitely prefer B" accepts B within the expressed scope
- "Use architecture B; that is our direction" supports a Crystallized decision and updates to affected current documents
- "Use B only for this experiment" remains scoped Working state rather than universal truth

The framework governs repository truth after people have converged. Solving organizational disagreement among people is outside its responsibility.

## Native Capability

Open Forge assumes contemporary agents can reason, inspect files, follow scoped authority, navigate links, and use tools.

It harmonizes with these capabilities and reuses them. It does not redefine ordinary reasoning or provide exhaustive instructions merely to accommodate obsolete low-capability models.

Additional procedure earns its place only when it makes something:

- Possible
- More reliable
- Cheaper to reason about
- Safer to perform
- Available at the correct time
- Specific to the workspace rather than native competence

## Framework Boundary

The Open Forge Framework contains two baseline areas:

- Core, which owns routing and reusable agent-facing primitives
- Memory, which owns continuity, candidate learning, accepted records, and history

Workspace-specific content uses these mechanisms but is not shared framework truth.

Extensions are optional reusable capabilities built on framework routes.

The CLI and future tools operate deterministically over the human-readable contract. They do not own meaning.

Agent providers and execution runtimes remain external. Files such as `AGENTS.md` or `CLAUDE.md` are minimal bridges into the canonical framework entry, not separate policy owners.

### Likely Core Primitives

The conceptual primitives currently appear to be:

- Loader and entrypoint Axioms for routing, inheritance, loading, and authority
- Directives for binding behavior
- Guidance for contextual judgment
- Patterns for reusable inspectable shapes
- Skills for specialized capability and procedure
- Workflows for repeatable goal-oriented recipes without imposing a universal lifecycle
- Workspace routes for discovering important project-owned locations and external owners

Their exact schemas and installed wording remain migration subjects.

A separate `rules/` primitive is rejected because it would duplicate directives without adding a distinct role. A linked discovery map remains possible if it points to rule owners rather than copying them.

## Memory

The four-state model is accepted:

| State | Meaning |
|---|---|
| Working | Temporary information needed for active work or resumability |
| Emerging | Potentially reusable information that is not accepted truth |
| Crystallized | Accepted durable truth within a declared scope |
| Archived | Useful history that no longer governs current work |

These states express semantic roles, not a mandatory linear pipeline. Any transition is legitimate when meaning and authority justify it.

### Working

Working Memory should be used actively for plans, sessions, handoffs, current priorities, intermediate state, and agent coordination.

Its defining property is expected expiration. When its active purpose ends, useful content is extracted, promoted, archived, consolidated, or pruned.

### Emerging

An explicitly stated idea to explore should be recorded immediately.

An agent should record an observation after one occurrence when it is plausibly reusable, surprising, or costly enough to preserve. Later agents should search for the existing observation and add evidence there.

Recurrence is mainly a threshold for consolidation or promotion, not necessarily for initial capture.

This encourages distributed learning without turning every activity into Memory.

### Crystallized

Crystallized Memory contains accepted durable state. Clear user direction may authorize this directly without a ceremonial approval command.

Agents should inform the operator about durable changes made from clear implicit authority, but should not request the same decision twice.

### Archived

Archives preserve useful superseded history. Current meaning must be extracted to its new owner before archival.

The starter child routes such as sessions, handoffs, ideas, observations, decisions, and documents remain useful removable defaults. They are scoped routes within the broader state model, not additional lifecycle states.

## Markdown And Deterministic Tools

The intended product language is:

> Markdown makes Open Forge complete. The CLI makes it exceptional.

Everything semantically important remains understandable and usable through human-readable files.

The CLI is a deterministic reasoning accelerator. It should make correct behavior the cheapest path by batching context, navigating routes, validating structure, previewing changes, and safely applying mechanical operations.

The architectural invariant is:

> Deterministic tools reduce the cost of obtaining and applying context; they do not privately own its meaning.

The current CLI is an MVP and will be redesigned after the shared contracts converge. Extensions are also an MVP and largely unexplored architecture.

## Architecture Documents

The intended document structure is:

- Top Open Forge architecture: complete system map, component boundaries, interactions, dependency direction, and cross-cutting invariants
- Framework architecture: Core and Memory internals
- Extensions MVP architecture: current optional capability system and redesign boundary
- CLI MVP architecture: current commands, implementation, lifecycle, safety, and redesign boundary

This is not a fixed taxonomy. Any sufficiently independent scope may have its own architecture document.

Major current documents receive focused review and approval.

After the Framework Architecture is accepted, revisit the top architecture. Anything that belongs internally to Core or Memory moves to the scoped owner, leaving the top document as the clear system-level view with links.

## Migration Philosophy

The Framework Architecture describes the architecture Open Forge wants rather than rationalizing what happens to exist.

Older files are handled through:

1. Migrate useful current meaning
2. Improve unclear or weak concepts
3. Extract material to its correct owner
4. Remove duplication or obsolete structures
5. Redirect links and references end to end
6. Preserve useful historical reasoning separately

The root `.agents/` installation dogfoods the framework. `src/open-forge/` is the user-facing payload. Governance explains contracts and verification but does not become hidden runtime truth or a second owner of source wording.

After architecture converges, source migration resumes top-down, one file per review gate.

The README is rebuilt later around:

1. The exact accepted vision
2. The fastest useful start
3. Advantages, workflows, and examples
4. Advanced and maintainer material
5. Credible usage and adopters when available

## Accepted Drafting Assumptions

1. The next document is the Open Forge Framework Architecture because it owns both Core and Memory.
2. It explains the intended framework completely without absorbing CLI internals, extension internals, or the complete contents of every runtime Axiom.
3. Existing files and decisions are migration inputs. Links to them are useful relationships rather than proof of the design.
4. The standard installed routes are what Open Forge ships, while users remain free to reshape or remove them.
5. Links to unstable owners are provisional and migration redirects them when a concept moves.
6. A separate `rules/` primitive is not introduced because directives already own that role.
7. Accepted current state belongs in current documents, useful why belongs in decisions, and this broader design process remains preserved as historical context rather than only in chat.
