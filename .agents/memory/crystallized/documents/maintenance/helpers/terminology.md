---
open-forge:
  description: Repository-only maintainer helper for choosing natural role, authority, and ownership terminology
  responsibility: Keep Open Forge wording consistent while favoring explicit relationships over role jargon
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Helper, Internal, Terminology]
---

# Terminology Helper

## Purpose

This internal helper keeps role-bearing Open Forge language consistent. It is not a glossary users must learn.

Current documents, runtime entries, Templates, and public documentation should internalize these distinctions and express them naturally enough that their meaning is obvious in place.

## Prefer The Relationship

Prefer stating the relationship or action directly when naming an actor adds no useful information.

| Meaning | Prefer |
|---|---|
| Goals and priorities come from a request | `user direction establishes the goal` or `the request establishes the goal` |
| A choice must remain with the user | `requires explicit user direction` or `remains under user control` |
| An agent may decide within delegated scope | `delegated authority` and the scope of that delegation |
| A person is accountable for a choice | `decision-maker`, `responsible person`, or `responsible role` |
| A disagreement needs resolution | `surface the disagreement for discussion` |

## Role Terms

| Term | Use when |
|---|---|
| User | A person or group uses, adopts, owns, installs, customizes, directs, or consumes an Open Forge environment, and naming that relationship is useful |
| Maintainer | The role changes the Open Forge distribution, repository contracts, source payload, tooling, tests, or release surfaces |
| Agent | An AI system or runtime investigates, suggests, challenges, coordinates, executes, verifies, or preserves context within applicable direction |
| Contributor | A person or agent changes a shared project while product responsibility or decision authority is not the point |
| Decision-maker | The sentence specifically concerns responsibility for a consequential choice |
| Person or people | No Open Forge-specific responsibility needs to be distinguished |

An orchestrator and its subagents are all agents. An orchestrator may relay user direction or exercise delegated authority, but that relationship should be stated directly when it matters. Do not use `operator` as a generic technical label for either the user or an orchestrating agent.

The same person may be a user, maintainer, contributor, and decision-maker. Select a term for the responsibility expressed by the sentence rather than treating it as a permanent identity.

## Authority Terms

| Term | Use when |
|---|---|
| Authoritative source | The type is unknown or irrelevant, and one file, route, system, person, or group authoritatively expresses the subject |
| Authoritative document | A current document authoritatively explains an accepted concept |
| Authoritative route | A routed file or entrypoint authoritatively expresses the applicable Framework or workspace meaning |
| Authoritative system | An external system authoritatively contains source code, issue state, product data, or another live subject |
| Responsible person or role | Human responsibility or accountability is meant rather than semantic authority |

Use the most specific natural term that the sentence supports. `Authoritative source` is the generic fallback, not a phrase every sentence must repeat.

Retain `ownership` when possession or managed lifecycle is the actual subject, including user ownership of installed files, Extension ownership of managed bytes, and ownership transfer during Template instantiation.

## Usage Checks

- Prefer direct wording over assigning a named role when the actor is already obvious
- Use `user` when ownership, adoption, customization, direction, or product-facing capability makes the relationship useful
- Use `maintainer` when a requirement exists because Open Forge itself is being changed or distributed
- Use `agent` for orchestrators and subagents, then state any delegated authority that affects the work
- Use a typed authoritative-source term for semantic authority
- Use `responsible person`, `responsible role`, or `decision-maker` for human accountability
- Use `ownership` only when possession or managed lifecycle is the intended meaning
- Use ordinary person-centered language when the domain distinction adds no useful precision

## Related Helpers And Authoritative Sources

- [Knowledge role helper](knowledge-roles.md)
- [Open Forge Principles](../../principles.md)
- [Top Open Forge Architecture](../../architecture.md)
- [Open Forge Framework Architecture](../../framework/architecture.md)
- [Typed authority and role terminology decision](../../../decisions/authoritative-source-terminology.md)
- [Historical knowledge-role boundary analysis](../../../../archived/analysis/2026-07-26_knowledge-role-boundaries.md)
