---
open-forge:
  description: Starting structure for a current document that explains a subject's parts, relationships, boundaries, and constraints
  tags: [Extension, Template, Document, Architecture, CurrentView]
---

# {Subject} Architecture

{
Template selection:

- Need: One current document that explains a subject's important structure and interactions.
- Primary question: How is this subject structured, how do its parts relate, and which boundaries and constraints govern it?

How to use:

- Replace the frontmatter, title, and placeholders, then remove this braced guidance.
- This is a starting structure, not a fixed schema. Merge, rename, reorder, or remove sections to fit the subject.
- Explain the accepted structure completely enough to use. Link to Decisions for why it was chosen.
- Keep current structure, known liabilities, and honest limits here. Put unresolved replacement architecture in Emerging Memory until it is accepted.
- Move component internals into scoped Architecture documents when they need their own complete current explanation.
  }

## Scope

{Name the subject, the architectural view this document defines, its readers, and the internals defined by narrower Architecture documents.}

## Architecture Drivers

{Identify the qualities, constraints, and accepted direction that most strongly shape the structure. Link to the sources that define them.}

## System Model

{Show the major elements and the smallest useful complete picture of the system. A table, diagram, or short model may replace prose.}

## Responsibilities

{State each major element's responsibilities and what it must not be relied on to provide.}

## Relationships And Dependency Direction

{Explain how the elements depend on, communicate with, or constrain one another. Make responsibility, authority, and dependency direction explicit.}

## Boundaries

{Explain what belongs inside and outside the system and where independently responsible concerns meet.}

## Flows

{Describe only the information, control, value, or work flows needed to understand the Architecture.}

## Cross-Cutting Invariants

{State structural constraints that must remain true across multiple elements. Put binding work behavior in Directives instead.}

## Current Tradeoffs And Limits

{Describe important consequences, liabilities, and honest limits of the accepted design. Link to Decisions for detailed reasoning and alternatives.}

## Architecture Views

{Link to narrower Architecture documents and state what each one defines.}

## Related Current Views

{Link to the Vision, context, Principles, Strategy, status, external sources, or other current views that constrain or complete this Architecture.}

## Decisions And Rationale

{Link to accepted Decisions that explain why important current structures were chosen.}
