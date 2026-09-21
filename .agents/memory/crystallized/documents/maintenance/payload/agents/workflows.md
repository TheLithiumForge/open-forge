---
open-forge:
  description: Current maintenance contract for the optional Workflow Support Skill and its recipe catalogue
  responsibility: Preserve Workflow selection, recipe validation, composition, stopping rules, source alignment, and `route` boundaries
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Extension, Workflow]
---

# Workflow Support Skill Maintenance Contract

## Source

The optional Workflow Support package provides these separate installed sources:

- Native Skill: [`src/extensions/workflows/content/.agents/skills/use-workflow/SKILL.md`](../../../../../../../src/extensions/workflows/content/.agents/skills/use-workflow/SKILL.md)
- Reference catalogue: [`src/extensions/workflows/content/.agents/skills/use-workflow/references/_references.md`](../../../../../../../src/extensions/workflows/content/.agents/skills/use-workflow/references/_references.md)

The repository counterparts are [`.agents/skills/use-workflow/SKILL.md`](../../../../../../../.agents/skills/use-workflow/SKILL.md) and [`.agents/skills/use-workflow/references/_references.md`](../../../../../../../.agents/skills/use-workflow/references/_references.md). Keep each counterpart aligned with its packaged source where shared authored content is intended.

The optional [catalogue Template](../../../../../../../src/extensions/workflows/content/.agents/templates/workflows/_workflows.md) provides starting content for a method. Its instructions explicitly avoid introducing a new root primitive.

## Contract

- Native Skill metadata uses `name` and `description`. The recipe catalogue carries its own `Extension` and `Workflow` metadata and explicit selection rules.
- Workflow selection is optional. Read the catalogue to identify a method from its descriptions and scopes, honor an explicit selection or opt-out, load only the relevant scope entrypoints and selected recipe, and check the recipe's `Goal` before proceeding. Work directly when no installed method adds value.
- A complete recipe has one non-empty level-2 `Goal`, `Steps`, and `Completion` section, in that order. `Goal` states when the method fits and the outcome it serves. `Steps` states the method, including dependencies and conditional work. `Completion` states what establishes the result or an honest blocked boundary. Extra sections remain optional, and an entrypoint may organize recipes without being a recipe.
- A selected recipe may compose existing Skills, tools, and agents through its `Steps`. Workflow Support does not supply the runtime for those capabilities, and recipe sources remain separate from the sources they compose.
- Execute required steps. Keep optional steps optional. Resolve missing capabilities before the step that needs them; use a substitute only when it is permitted and preserves the requirement. Otherwise report the blocked step and continue only independent authorized work. Do not restart completed work or recurse into the same unchanged request.
- Check `Completion` against the actual result and report unmet conditions or unavailable evidence. Finishing a recipe does not authorize new scope, accept its output, or authorize commits, merges, publication, or contact with remotes.
- The Skill and its catalogue define workflow selection. The Skill mechanism does not make its resources loader-reachable ordinary routes, establish a Core Workflow route, or create baseline loading behavior.

## Verification

- Package verification inspects the installed Workflow Support package and its declared dependency composition, including the [`extension.json`](../../../../../../../src/extensions/workflows/extension.json) manifest, the native Skill, the separate catalogue, and any contributed recipe scopes. It checks relative resource links and local parity where shared content is intended.
- Validate each present recipe's `Goal`, `Steps`, and `Completion` sections, their order, explicit dependencies, selection rules, stopping conditions, and completion boundary. Do not claim that Core installs a workflow route or baseline workflow loading.
- Review specialized and experimental recipes individually. Record why each is revised, retained locally, or promoted; do not infer effectiveness from its existence.
