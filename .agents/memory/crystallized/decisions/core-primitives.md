---
open-forge:
  description: Core uses distinct reusable content roles instead of one generic knowledge bucket, and every new primitive must earn nonduplicative semantics
  tags: [Memory, Decision, CurrentTruth, Core, Primitive]
---

# Distinct Core Primitive Roles

## Context

Open Forge needs several forms of reusable context with different selection and authority semantics. Treating all of them as generic knowledge would make authority, applicability, and maintenance ambiguous. Treating every content variation as a primitive would make Core large and opinionated.

## Decision

Core uses a small set of semantically distinct reusable content roles instead of one generic knowledge bucket.

Each primitive must answer a different primary question, carry meaning that cannot be expressed clearly by an existing primitive, and justify its routing and maintenance cost. The current primitive set and complete relationships belong to the [Core primitive model](../documents/framework/primitives/model.md) and installed entrypoints.

Framework contracts refer to #Core collectively when any suitable Core route may satisfy a requirement. They name a specific primitive when its distinct semantics matter and enumerate concrete routes when the exact standard routes are the subject.

## Rationale

Distinct roles make the correct authoritative route and semantics cheaper to determine before content is loaded. They let a workspace add only the capabilities it needs while keeping Framework relationships inspectable.

The admission threshold prevents Core from becoming a catalogue of the author's preferred artifact types or workflows.

## Alternatives And Tradeoffs

- One generic knowledge route would make Core smaller physically but move recurring semantic classification into every agent decision
- A separate primitive for every document or workflow variation would improve naming specificity at the cost of overlap, baseline complexity, and universal methodology

The accepted model requires clearer primitive definitions and careful review when a new role is proposed.

## Consequences

- Primitive entry descriptions and current architecture must make their distinct questions and authority visible
- A proposed primitive must demonstrate reusable value and a nonduplicative semantic role
- Local scopes and Extensions may introduce specialized content without expanding the shared primitive set
- Users may remove unused standard routes without invalidating the remaining Framework

## Authoritative Sources

These sources express the accepted result. They are not the rationale backing this decision:

- [Core primitive model](../documents/framework/primitives/model.md)
- [Framework Architecture](../documents/framework/architecture.md#core-primitives)
- [Installed Framework loader](../../../loader.md)

## Decision Relationships

- [Templates as a Core primitive](template-primitive.md)
- [Product direction](product-direction.md)
- [Routing model](routing-model.md)
- [Workflow shape](workflow-shape.md)
- [Typed authority and role terminology](authoritative-source-terminology.md)
