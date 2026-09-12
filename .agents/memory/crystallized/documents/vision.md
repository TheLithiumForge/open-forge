---
open-forge:
  description: Current Open Forge purpose, product promise, scope, success criteria, and non-goals
  tags: [Memory, Document, CurrentTruth, Evergreen, Vision, Product, ACE]
---

# Open Forge Vision

## Adaptive Context Engineering

Adaptive Context Engineering (ACE) means shaping a workspace's knowledge and instructions so useful context is available for each task and stays current as the work changes.

Open Forge builds on progressive disclosure and specification-driven development. It applies those ideas to the workspace itself, so context can evolve alongside the work. It gives information a clear place, connects related sources, makes each source's authority clear, and puts the next useful detail within reach.

Progressive disclosure keeps each task's context focused while making deeper detail available when it is needed. Specification-driven development makes accepted goals, constraints, and expected results explicit enough to guide work and assess its outcome.

People and agents refine that context as goals, accepted decisions, and understanding change. Expectations can begin with clear user direction in a conversation, then become more detailed as decisions are accepted and evidence improves understanding. The aim is enough clarity for the work at hand, with further detail added when it becomes useful.

Context is the current knowledge, authority, constraints, relationships, and working state relevant to understanding and acting on a goal. It is not every file a workspace contains.

For example, work on a feature may need the accepted product direction, relevant architecture, local conventions, and current task state. Unrelated project history remains outside that context unless the work connects to it.

## Vision

Open Forge is a user-owned operating layer for Adaptive Context Engineering, defined in plain files. It makes workspaces understandable, routes the right context and authority, enables confident autonomy, preserves continuity, and deliberately evolves how work gets done. It does this without imposing a universal methodology or requiring a proprietary runtime.

## Why Open Forge

Capable agents still produce inconsistent results when relevant context is missing, stale, duplicated, too broad, or expensive to retrieve. Users then spend limited attention repeating settled direction, reviewing avoidable mistakes, and teaching every new session how the workspace works.

Large predefined methodologies answer this problem with more process, instructions, and required workflows. Their onboarding, context, and review costs grow even when most of that machinery is irrelevant to the work.

Open Forge instead makes accepted context explicit, connected, scoped, and cheap to retrieve. The workspace starts with a small useful foundation. Its self-growing Memory expands through real decisions, corrections, evidence, and recurring needs without putting the whole history into active context.

A user can begin with incomplete natural direction. The agent builds the best current model from available context, responds conversationally unless deep analysis was requested, recommends a coherent direction, surfaces one consequential decision frontier at a time, and offers deeper detail as it becomes useful.

## Scope

Open Forge shapes the working environment around a subject. It provides readable context, explicit relationships and authority, routed retrieval, continuity, reusable starting points, and deterministic assistance. It does not own the subject's goals, domain knowledge, source code, product data, or external systems unless the workspace deliberately records or routes to them.

Development is a proving ground, not a boundary. Open Forge can grow around any person, project, team, discipline, collection of projects, or shared source of truth that benefits from explicit, evolving context.

## Principles

The [Open Forge Principles](principles.md) define the identity-level filters that guide the product as it evolves. Open Forge remains user-owned, relevance-routed, user-directed, agent-enabled, explicitly connected, small at its shared foundation, recursively adaptable, complete in readable files, and honest about nondeterministic reliability.

## Success

Open Forge succeeds when:

- A new agent can enter a workspace and cheaply determine what matters, where it lives, what is authoritative, and what remains uncertain
- Users can begin with natural incomplete intent and are asked only for consequential decisions in language they can understand
- Settled workspace knowledge is understood instead of repeatedly inferred
- Users spend attention on meaningful decisions and outcomes rather than preventable process failures
- Accepted conversations become useful current documents and supporting context without requiring users to operate the Framework taxonomy
- Work survives changes of session, agent, provider, project phase, and contributor without losing accepted direction
- Unselected scopes add almost no active-context cost, while related scopes can be combined deliberately
- Each workspace becomes more capable and personal through use without making the shared foundation more opinionated
- Every part remains inspectable, reviewable, replaceable, and removable by the user

## Non-Goals

Open Forge is not:

- A universal development, product, design, or organizational methodology
- A large catalogue of mandatory prompts, workflows, roles, or ceremonies
- A substitute for user judgment or responsibility
- A hidden knowledge database, agent runtime, or provider-specific orchestrator
- An automatic recorder of every conversation or activity
- A mechanical guarantee that a nondeterministic agent will behave correctly
- A system designed around obsolete agents that require exhaustive instructions for ordinary reasoning

## Related Current Views

- [Open Forge architecture](architecture.md)
- [Open Forge principles](principles.md)

## Decisions And Rationale

- [Product direction](../decisions/product/product-direction.md)
- [Adaptive decision elicitation](../decisions/framework/adaptive-decision-elicitation.md)
