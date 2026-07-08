---
open-forge:
    description: Reusable agent capability packages with clear use cases and expected results
    tags: [OpenForge, Core, Skill, Index]
---

# Skills

Skills are bounded reusable agent capability packages.

## Axioms

- Use `Entries` when current work may benefit from a reusable capability.
- Load routes whose path, description, or tags match the current work.
- Prefer the native skill shape: `.agents/skills/{skill-name}/SKILL.md`.
- Open Forge routes skill packages; the active agent runtime owns skill invocation, activation, packaging, installation, and execution.
- `SKILL.md` defines one bounded capability, its positive applicability, instructions, bundled resources, and expected result.
- Keep runtime-native skill metadata compatible with the active agent runtime.
- Put optional details under the skill folder, such as `references/`, `scripts/`, or `assets/`, and load them only when `SKILL.md` says they are relevant.
- Follow each selected child `entrypoint`'s scope and loading axioms recursively.
- Prefer skills from a narrower selected scope over broader skills when safe and allowed.
- Generated `entries` are navigation and reserved load policy only.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
