---
open-forge:
  description: "Explain the system model, responsibilities, dependency direction, important flows, and limits"
  tags: [Extension, Template, Document, Architecture]
---

# {Subject} Architecture

{
Describe a coherent current or explicitly proposed architecture. Use only views needed for this subject. Narrower components may have their own sources.
Replace {prompts}. Remove this source guidance and optional sections that add no value.
}

## Overview And Scope

{Explain the system in a short paragraph. State the question this view answers, its reader, and the boundary between current structure and any proposal.}

## Drivers

{The few qualities, constraints, and accepted decisions that actually shape the architecture. Link to their defining sources.}

## System Model

{Show the major parts and their relationships with a short model, diagram, or table. It should orient a new reader before component detail.}

| Part | Responsibility | Boundary or dependency |
| --- | --- | --- |
| {Component or external system} | {What it owns} | {What it relies on, and what belongs elsewhere} |

## Important Flows

{Trace the information, control, or work flows needed to understand the system. Make dependency direction and ownership changes explicit. Describe relevant failure and recovery paths, not just the success path.}

## Boundaries And Invariants

{What must remain true across parts? Include trust, consistency, compatibility, or lifetime boundaries only where relevant. Link to rules that govern agent conduct rather than repeating them as system architecture.}

## Tradeoffs And Limits

{Explain the accepted compromises, current liabilities, and scale or change triggers. Keep an unresolved replacement design visibly separate from current structure.}

## Related Views And Rationale

{Link to narrower architecture views and what they define. Link to the Vision, Principles, external contracts, and Decisions that explain this structure without reproducing their history.}
