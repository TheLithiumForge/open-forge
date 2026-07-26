---
open-forge:
  description: Current Open Forge purpose, product promise, principles, success criteria, and non-goals
  tags: [Memory, Document, CurrentTruth, Evergreen, Vision, Product, ACE]
---

# Open Forge Vision

## Adaptive Context Engineering

Adaptive Context Engineering (ACE) is the deliberate design of a workspace's information, relationships, authority, and retrieval paths so the right context is available at the right time and the environment evolves through use.

Context is the current knowledge, authority, constraints, relationships, and working state relevant to understanding and acting on a goal. It is not every file a workspace contains.

For example, work on a feature may need the accepted product direction, relevant architecture, local conventions, and current task state. Unrelated project history remains outside that context unless the work connects to it.

## Vision

Open Forge is a user-owned, human-readable, file-native operating layer for Adaptive Context Engineering. It makes workspaces understandable, routes the right context and authority, enables confident autonomy, preserves continuity, and deliberately evolves how work gets done. It does this without imposing a universal methodology or requiring a proprietary runtime.

## Why Open Forge

Capable agents still produce inconsistent results when relevant context is missing, stale, duplicated, too broad, or expensive to retrieve. Operators then spend limited attention repeating settled direction, reviewing avoidable mistakes, and teaching every new session how the workspace works.

Large predefined methodologies answer this problem with more process, instructions, and required workflows. Their onboarding, context, and review costs grow even when most of that machinery is irrelevant to the work.

Open Forge instead makes accepted context explicit, connected, scoped, and cheap to retrieve. The workspace starts with a small useful foundation, then develops its own methods from real decisions, corrections, evidence, and recurring needs.

## Scope

Open Forge shapes the working environment around a subject. It provides human-readable context, explicit relationships and authority, routed retrieval, continuity, reusable starting points, and deterministic assistance. It does not own the subject's goals, domain knowledge, source code, product data, or external systems unless the workspace deliberately records or routes to them.

Development is a proving ground, not a boundary. Open Forge can grow around any person, project, team, discipline, collection of projects, or shared source of truth that benefits from explicit, evolving context.

## Principles

### User-Owned And Human-Readable

The workspace's meaning lives in human-readable files that users can inspect, edit, move, replace, or remove. No hidden service or proprietary runtime is required to understand how it works.

### Operator-Led, Agent-Enabled

The operator owns goals, priorities, consequential tradeoffs, and accepted direction. Agents investigate, suggest, challenge, execute, and verify within that direction without turning ordinary work into approval ceremony.

### Start Small, Evolve Deliberately

Open Forge ships a small set of sensible removable defaults instead of a complete methodology. Useful working state, candidate learning, accepted knowledge, and history evolve through explicit ownership and deliberate transitions rather than uncontrolled accumulation.

### Context That Scales By Relevance

Open Forge has no fixed structural expansion ceiling. Active context grows primarily with selected route depth, scopes, and relationships, not with the total size of the workspace. For example, two projects can share one Open Forge environment while ordinary work in one does not load the other's routed context. Work that integrates them can deliberately select both.

### One Owner, Visible Relationships

Each detailed definition, decision, and contract has one authoritative owner. References, anchored relative links, descriptions, and established tags connect related material without maintaining competing copies.
Each important subject has one file or routed system that owns its current meaning. For example, `vision.md` owns what Open Forge is now, so other files link to it instead of restating the vision. When an accepted decision explains the current state, the current owner states the result and links to that decision for rationale. The decision links forward to the current owner.

### Harmonize With Native Capability

Open Forge assumes contemporary agents can reason, inspect files, follow scoped authority, and use tools. It harmonizes with and reuses these native capabilities, adding workspace-specific context and settled conventions rather than redefining how an agent should reason.

### Complete In Markdown, Exceptional With Tools

Human-readable Markdown contains the complete semantic contract. Deterministic tools make correct loading, navigation, validation, and change safer and cheaper without privately owning meaning.

### Honest Reliability

Structure and deterministic tooling increase the probability of correct behavior from nondeterministic agents. Open Forge validates what can be made deterministic and does not claim mechanical control over reasoning or compliance.

## Success

Open Forge succeeds when:

- A new agent can enter a workspace and cheaply determine what matters, where it lives, what is authoritative, and what remains uncertain
- Settled workspace knowledge is understood instead of repeatedly inferred
- Operators spend attention on meaningful decisions and outcomes rather than preventable process failures
- Work survives changes of session, agent, provider, project phase, and contributor without losing accepted direction
- Unselected scopes add almost no active-context cost, while related scopes can be combined deliberately
- Each workspace becomes more capable and personal through use without making the shared foundation more opinionated
- Every part remains inspectable, reviewable, replaceable, and removable by its owner

## Non-Goals

Open Forge is not:

- A universal development, product, design, or organizational methodology
- A large catalogue of mandatory prompts, workflows, roles, or ceremonies
- A substitute for operator judgment or responsibility
- A hidden knowledge database, agent runtime, or provider-specific orchestrator
- An automatic recorder of every conversation or activity
- A mechanical guarantee that a nondeterministic agent will behave correctly
- A system designed around obsolete agents that require exhaustive instructions for ordinary reasoning

## Related Current Views

- [Open Forge architecture](architecture.md)

## Decisions And Rationale

- [Product direction](../decisions/product-direction.md)
