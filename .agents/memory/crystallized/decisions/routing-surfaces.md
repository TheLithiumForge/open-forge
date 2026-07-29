---
open-forge:
  description: Entry descriptions support pre-load selection while routed bodies provide complete role-specific meaning
  tags: [Memory, Decision, CurrentTruth, Routing, Formatting]
---

# Routing Surfaces

## Context

Agents need enough information to select or skip a route before paying to read its body. Earlier designs mixed selection wording, execution instructions, dependencies, and applicability inside the same body, making routing expensive and sometimes asking the same question twice.

## Decision

The linked entry description is the pre-load selection surface. Its path identifies the routed destination, tags provide cheap additional signals, and the selected body contains the complete role-specific meaning.

Descriptions expose enough trigger, purpose, or outcome to select or skip a route without opening it. A body may confirm its goal or purpose for direct readers, but it does not repeat a large selection contract.

Primitive-specific contracts may add narrower semantics. Directives become binding after their route establishes scope, #KeepInMind uses its defined continuity discovery boundary, and Workflow descriptions and topical tags support selection without becoming the execution recipe.

## Rationale

Separating selection from detailed meaning lowers exploratory context cost and lets generated Entries remain useful navigation. Natural descriptions are easier to inspect and improve than hidden relevance metadata.

Keeping primitive-specific exceptions in their own contracts avoids forcing one execution grammar onto every routed file.

## Alternatives And Tradeoffs

- Selection prose only inside the body would require opening every candidate
- Repeating the full description in the body would increase maintenance and context cost
- Requiring every routed body to open with one Goal would impose Workflow language on documents, routes, and other roles
- A second directive applicability gate would weaken the scope decision already made by routing
- Adding a fixed Workflow phase vocabulary would duplicate descriptions and topical tags while suggesting a lifecycle the Framework does not impose

Descriptions require careful writing because they are decision surfaces rather than decorative summaries.

## Consequences

- Frontmatter descriptions and generated Entries stay compact while remaining sufficient for pre-load selection
- Bodies provide complete role-specific content after selection
- Route-specific tags supplement rather than replace readable descriptions
- The matching primitive or loading contract is authoritative for specialized selection and execution semantics

## Authoritative Sources

- [Current routing model](../documents/framework/routing/model.md)
- [Routed Markdown representation](../documents/framework/markdown/routes.md)
- [Directive contract](../documents/framework/primitives/directives.md)
- [Workflow contract](../documents/framework/primitives/workflows.md)
- [Open Forge loader](../../../loader.md)

## Decision Relationships

- [Routing model](routing-model.md)
- [Loading reliability](loading-reliability.md)
- [Canonical Markdown authoring](canonical-markdown.md)
- [User-facing writing](user-facing-writing.md)
- [Workflow shape](workflow-shape.md)
