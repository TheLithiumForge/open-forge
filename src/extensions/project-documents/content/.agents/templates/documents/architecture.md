---
open-forge:
  description: "Explain the system model, responsibilities, dependency direction, important flows, and limits"
  tags: [Extension, Template, Document, Architecture]
---

# {Subject} Architecture

{
Describe a coherent current or explicitly proposed architecture using only the views needed to understand it. Narrower components may have their own sources.
Replace {prompts}, then remove this guidance and unused optional sections.
}

## Overview And Scope

{Explain the system in a short paragraph. State the question this view answers, its reader, and the boundary between current structure and any proposal.}

## Drivers

{The few qualities, constraints, and accepted decisions that actually shape the architecture. Link to their defining sources.}

## System Model

{Choose a diagram, outline, or the table below to show the major parts and relationships before component detail. Use more than one only when each adds distinct information.}

| Part                           | Responsibility            | Boundary or dependency                          |
| ------------------------------ | ------------------------- | ----------------------------------------------- |
| {Component or external system} | {What it defines or does} | {What it relies on, and what belongs elsewhere} |

## Important Flows

{Trace the information, control, or work flows needed to understand the system. Make dependency direction and ownership changes explicit. Describe relevant failure and recovery paths, not just the success path.}

## Boundaries And Invariants

{What must remain true across parts? Include trust, consistency, compatibility, or lifetime boundaries only where relevant. Link to rules that govern agent conduct rather than repeating them as system architecture.}

## Tradeoffs And Limits

{Explain the accepted compromises, current liabilities, and scale or change triggers. Keep an unresolved replacement design visibly separate from current structure.}

## Related Views And Rationale

{Link to parent and narrower architecture views when they exist, with each view's defining question. Add related requirements or rationale only when useful.}
