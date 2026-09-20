---
open-forge:
  description: "Historical CLI-v2 source: The Open Forge CLI primarily serves any capable agent while preserving human authority, provider neutrality, and trustworthy workspace management"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Agent-First CLI Product Contract

## Context

Open Forge deliberately treats agents as the primary users of routine Framework operations. They benefit from compact context, explicit provenance, deterministic validation, and safe mechanical changes that reduce tool calls, tokens, latency, rediscovery, and avoidable errors.

People install and manage Open Forge less frequently, but those operations carry greater ownership, preservation, and recovery risk. Scripts, continuous integration, editors, and future integrations also need reliable access to the same behavior.

Designing only for an interactive human terminal would make ordinary agent work unnecessarily expensive. Designing only for agent efficiency could make consequential workspace changes opaque or difficult for people to review and recover.

## Decision

Any capable agent is the primary CLI user, independent of model, provider, editor, or agent runtime. The person or team directing the workspace remains the beneficiary and consequential decision authority. People are also first-class CLI users for installation, inspection, repair, upgrade, restoration, extension management, and customization. Scripts and integrations consume the same application contracts.

The CLI makes framework-aware decisions and actions cheaper, safer, and more reliable. It turns human-readable workspace state into compact ordered context, explicit provenance, deterministic findings, and inspectable change plans. It assists every Open Forge task where deterministic retrieval, inspection, validation, planning, scaffolding, or mutation provides meaningful leverage.

The CLI supports decisions without becoming the semantic decision-maker. Agents reason from the evidence it returns, and users govern consequential direction. The CLI does not privately decide product intent, accepted truth, or priority.

Every applicable operation must support non-interactive use and a stable versioned structured result. Human output and interactive experiences present the same underlying operations. Results remain concise by default, expandable when needed, provider-neutral, and explicit about errors, causes, effects, provenance, and recovery.

Human lifecycle operations are not secondary in importance. They must preserve user-owned meaning, expose consequential effects before mutation, and provide trustworthy cancellation and recovery.

## Rationale

Agent-first design follows the chosen primary use and makes the CLI a practical deterministic reasoning accelerator rather than a terminal utility that agents happen to invoke.

One shared operation and result model prevents interactive, agent, and automation paths from developing different behavior or safety. Provider neutrality keeps Open Forge broadly usable and consistent with its file-native identity.

Separating decision support from semantic authority preserves user direction and agent autonomy. The CLI supplies mechanically trustworthy evidence and effects without becoming another agent runtime or hidden source of workspace truth.

## Alternatives And Tradeoffs

- A human-first interactive CLI would make onboarding familiar but increase agent tool calls, parsing cost, and dependence on terminal presentation.
- An agent-only machine interface would optimize frequent execution but weaken human comprehension, trust, and recovery during consequential lifecycle work.
- CLI-generated semantic recommendations could reduce some reasoning work but would move contextual judgment into deterministic policy and risk presenting opinion as framework truth.
- Limiting the CLI to current commands would keep the boundary narrow but leave valuable deterministic framework tasks unnecessarily expensive.

The accepted direction requires deliberate output design. Structured results must be compact enough for agents, complete enough for automation, and explainable enough to render trustworthy human experiences.

## Consequences

- Agent cost, including tool calls, returned tokens, latency, retries, and avoidable decision errors, becomes a product success dimension.
- Every operation needs a non-interactive path, structured errors, stable exit behavior, and provenance-visible results.
- Interactive wizards remain presentations over deterministic operations rather than separate implementations.
- New capabilities qualify by deterministic leverage, not merely by being related to Open Forge.
- Installation and management flows receive proportionate review, preservation, and recovery safeguards despite lower usage frequency.
- Semantic reasoning, product judgment, and acceptance remain outside CLI authority.

## Authoritative Sources

- [Open Forge CLI Architecture](../../documents/cli/architecture.md)
- [Open Forge CLI Interface](../../documents/cli/interface.md)
- [Open Forge Principles](../../documents/principles.md)
- [Open Forge Vision](../../documents/vision.md)

## Decision Relationships

- [Product direction](../product/product-direction.md)
- [Adaptive decision elicitation](../framework/adaptive-decision-elicitation.md)
- [Extension package boundary](../extensions/extension-package-boundary.md)
