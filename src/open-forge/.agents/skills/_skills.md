---
open-forge:
  description: Reusable agent capabilities with clear use cases and expected results
  tags: [OpenForge, Core, Skill, Index]
---

# Skills

Skills are bounded reusable agent capabilities.

## Axioms

- Use `Entries` when current work may benefit from a reusable capability.
- Load routes whose path, description, or tags match the current work.
- Open Forge routes skills; the active agent runtime owns skill invocation, activation, packaging, installation, and execution.
- Every skill file defines one bounded capability, its positive applicability, and its expected result.
- Skill files may include steps, inputs, tool requirements, outputs, examples, or references needed to use the capability reliably.
- Follow the skill format and invocation model of the active agent runtime.
- Follow each selected child `entrypoint`'s scope and loading axioms recursively.
- Prefer skills from a narrower selected scope over broader skills when safe and allowed.
- Generated `entries` are navigation and reserved load policy only.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
