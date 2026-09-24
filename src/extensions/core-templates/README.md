# Core Templates

Start your own workspace instructions, advice, reusable shapes, capabilities, maps, and memory without inventing each file from scratch.

## What You Get

[Core Templates](content/.agents/templates/core/_core.md) provides one starter for each Core category, plus Memory:

| Starter | Use it to |
| --- | --- |
| [Directive](content/.agents/templates/core/directive.md) | State required behavior for a selected scope. |
| [Guidance](content/.agents/templates/core/guidance.md) | Recommend an approach and explain when it fits. |
| [Pattern](content/.agents/templates/core/pattern.md) | Define a reusable, inspectable shape. |
| [Skill](content/.agents/templates/core/skill.md) | Create a native SKILL.md capability. |
| [Template](content/.agents/templates/core/template.md) | Write another independently reusable starting file. |
| [Map](content/.agents/templates/core/map.md) | Point to useful sources and explain when to read them. |
| [Memory](content/.agents/templates/core/memory.md) | Preserve useful knowledge or work state without a specialized category. |

**Direct dependencies:** None.

## Start Using It

> Use the relevant Core Template to write this workspace rule. Keep it in the narrowest useful scope and follow the destination's loading rules.

Choose a specialized Template instead when one already fits. These starters stay on demand and add no workspace-wide work procedure.

Ordinary routed files can be copied and adapted manually or started with `route create --template`. The CLI copies the body; you supply destination metadata and replace prompts yourself. Initialize missing scopes with `route init`.

The Skill starter is different: copy its fenced native file into `{skill-name}/SKILL.md` under the selected Skills scope. The wrapper is a routed Template, not an installed capability. Follow the active runtime's Skill format and discovery rules; route creation does not create native Skill packages.

## Install And Customize

See the [installation guide](../../../docs/extensions.md). Copy only what helps, replace source metadata and prompts, and rebase links. Template copies are maintained independently; package updates do not update the work created from them.
