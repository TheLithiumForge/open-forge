---
open-forge:
  description: Architecture template is used when one current document must define how a subject is structured, how its parts relate, and which boundaries and constraints govern it
  tags: [Template, Document, Architecture, CurrentView]
---

# {Subject} Architecture

<!--
Template selection:
- Need: One coherent current document for a subject's important structure and interactions.
- Primary question: How is this subject structured, how do its parts relate, and which boundaries and constraints govern it?

Instantiation notes:
- Replace this template's frontmatter, title, placeholders, and comments.
- Architecture is a responsibility, not a fixed schema. Merge, rename, reorder, or remove sections to fit the subject.
- Describe the accepted structure completely enough to use. Link to decisions for why it was chosen.
- Keep current structure, known liabilities, and honest limits here. Put unresolved replacement architecture in Emerging Memory until it is accepted.
- Move component internals into scoped architecture views once they require independent authority.
-->

## Scope

{Name the subject, the architectural view for which this document is authoritative, its readers, and the internals expressed by narrower authoritative documents.}

## Architecture Drivers

{Identify the qualities, constraints, and accepted direction that most strongly shape the structure. Link to their authoritative sources.}

## System Model

{Show the major elements and the smallest useful complete picture of the system. A table, diagram, or short model may replace prose.}

## Responsibilities

{State each major element's responsibilities and what it must not be relied on to provide.}

## Relationships And Dependency Direction

{Explain how the elements depend on, communicate with, or constrain one another. Make responsibility, authority, and dependency direction explicit.}

## Boundaries

{Explain what belongs inside and outside the system and where independently responsible concerns meet.}

## Flows

{Describe only the information, control, value, or work flows needed to understand the architecture.}

## Cross-Cutting Invariants

{State structural constraints that must remain true across multiple elements. Put binding work behavior in directives instead.}

## Current Tradeoffs And Limits

{Describe material consequences, liabilities, and honest limits of the accepted design. Link to decisions for detailed rationale and alternatives.}

## Architecture Views

{Link to narrower authoritative architecture documents and state what each one explains.}

## Related Current Views

{Link to the vision, context, principles, strategy, status, external authoritative sources, or other current views that constrain or complete this architecture.}

## Decisions And Rationale

{Link to accepted decisions that explain why important current structures were chosen.}
