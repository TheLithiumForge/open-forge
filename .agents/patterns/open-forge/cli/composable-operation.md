---
open-forge:
  description: Reusable default shape for one inspectable CLI operation
  responsibility: Define a product-agnostic command, input, stage, result, and review shape for predictable CLI operations
  tags: [Pattern, CLI, Interface, Command, Flag, Composition, Result, Testing, Predictability]
---

# Composable CLI Operation Pattern

## Shape

Schematic command grammar. Names are illustrative. Subjects repeat only where the operation accepts several. Modifiers are optional and compose in compatible roles.

```text
<cli> <operation> [subjects] [modifiers]
<cli> <group> <operation> [subjects] [modifiers]

inspect <subject>
move <source> <destination>
search <field-value>
```

| Part                              | Expected result                                                               | Status                                                    |
| --------------------------------- | ----------------------------------------------------------------------------- | --------------------------------------------------------- |
| Leaf                              | One complete coherent operation                                               | Required                                                  |
| Group                             | Several actual operations sharing subject or lifecycle, bare group shows help | Optional                                                  |
| Subjects                          | Explicit operands, independent repeated subjects                              | Required unless one documented safe finite default exists |
| Modifiers                         | Context, selection, guided input, projection, write policy, named authority   | Optional compatible dimensions                            |
| Guided answers and explicit input | One complete typed request, conflicts rejected                                | Guided input optional                                     |
| Preview                           | Same request, facts, planner and preflight, no writes                         | Optional for meaningful mutations                         |

```text
validated input
  -> complete typed request
  -> current facts
  -> read result -----------------------------------+
  -> complete mutation plan                         |
     -> preflight                                  |
     -> preview result ----------------------------+
     -> application consent                        |
     -> revalidation and apply                     |
     -> verification or local recovery ------------+
  -> typed operation result
     -> human rendering OR structured rendering
  -> process completion
```

Arrows show dependency order, not literal API names. The read branch omits mutation stages. Preview omits apply-only effects. Both renderings consume the same typed result.

| Result part                                             | Required relationship                                                          |
| ------------------------------------------------------- | ------------------------------------------------------------------------------ |
| Status vocabulary, precedence, process mapping, streams | Defined by product contract                                                    |
| Primary result and bounded diagnostics                  | Separate                                                                       |
| Error                                                   | Operation, subject, cause and useful next action                               |
| Same request and relevant facts                         | Same semantic result                                                           |
| Idempotent write already converged                      | Verified no-op                                                                 |
| Non-idempotent operation                                | Explicit product variation, consent and result meaning                         |
| Recovery                                                | Local effect-family verification, interruption, concurrency and residual facts |
| Syntax, resolver, handler, planner, renderer, tests     | Narrowest useful local scope                                                   |

## Applicability And Sources

For inspectable CLI operations. Shallow paths are the default. Domain-required depth is a deliberate review variation. Structured output should render one complete result from the same typed value. Any compatibility exception is a product variation.

The reason for domain-required depth should be visible in review.

- [Composable CLI Operations Directive](../../../directives/cli/composable-operation.md) binds input, authority, modifiers, stage boundaries, results and evidence.
- [CLI Design Guidance](../../../guidance/cli-design.md#operation-variation-review) supplies tradeoffs and the variation review procedure.
- [Source Locality](../../../directives/source-locality.md) binds promotion and placement.
