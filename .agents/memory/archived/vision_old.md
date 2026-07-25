---
open-forge:
  description: Superseded pre-reset Open Forge vision preserved for historical context
  tags: [Memory, Archived, Contextual, Historical, Vision, Product]
---

# Open Forge Vision (Superseded)

This vision was superseded on 2026-07-26 by the clean [ACE product vision](../crystallized/documents/vision.md). It remains historical context, not accepted current truth.

## Purpose

Open Forge is a small, inspectable substrate for effective human-agent work in plain repository files. It helps agents find the right context, respect accepted direction, preserve uncertainty and learning, and apply the user's way of working without imposing somebody else's complete methodology.

## Product Promise

Open Forge intentionally starts generic and usable. Native agent competence performs ordinary work; Open Forge adds context, authority, timing, coordination, persistence, and user-specific behavior where those provide leverage.

Users gradually teach the workspace through corrections, accepted decisions, and local practice. Those preferences can become directives, patterns, guidance, skills, workflows, workspace routes, and Memory until the workspace increasingly behaves like its user.

> Open Forge provides the substrate. The user's accumulated decisions provide the personality and methodology.

## Principles

- Use the minimal information needed to enable maximal useful work.
- Keep Core generic, vendor-agnostic, and small enough to review in full.
- Route strictly top-down: an already-loaded parent exposes enough information to select a child before the child is opened.
- Rely on native agent competence for ordinary work.
- Add framework or extension behavior only when it makes something possible, more reliable, or differently timed than a capable agent would provide adequately on its own.
- Keep the human in control of accepted direction without asking for redundant confirmation.
- Keep plain Markdown complete without requiring a hidden runtime or the CLI.
- Prefer explicit owners, relative links, and reviewable diffs over duplicated truth, hidden state, or orchestration.

## Success

Open Forge succeeds when an agent can cheaply determine:

- where to look;
- what is authoritative;
- what remains uncertain;
- what must stay synchronized;
- which optional capability would materially improve the work;
- where accepted learning should be preserved.

It succeeds over time when each workspace becomes more personal without making Core more opinionated.

## Non-Goals

- A universal software-development or product methodology.
- A collection of the author's preferred prompts presented as general truth.
- A hidden agent runtime or mechanical guarantee of compliant behavior.
- Mandatory provider-specific orchestration.
- Automatic approval of ambiguous product or technical choices.

## Related Current Views

- [Current product vision](../crystallized/documents/vision.md)
- [Current framework architecture](../crystallized/documents/architecture.md)
- [Accepted product-direction rationale](../crystallized/decisions/product-direction.md)
