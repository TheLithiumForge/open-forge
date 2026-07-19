---
open-forge:
    description: Reusable agent capability packages with clear use cases and expected results
    tags: [LoadNow, Core, Skill]
---

# Skills

Skills are bounded reusable agent capability packages. //Hn: At most here we can explain that our skills sre just skills but i feel this would be overreaching too. 

## Axioms

- Use `Entries` when current work may benefit from a reusable capability.
- Prefer the native skill shape: `.agents/skills/{skill-name}/SKILL.md`.
- Open Forge routes skill packages; the active agent runtime owns skill invocation, activation, packaging, installation, and execution.
- `SKILL.md` defines one bounded capability, its positive applicability, instructions, bundled resources, and expected result.
- Keep runtime-native skill metadata compatible with the active agent runtime.
- Put optional details under the skill folder, such as `references/`, `scripts/`, or `assets/`, and load them only when `SKILL.md` says they are relevant.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
