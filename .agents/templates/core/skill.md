---
open-forge:
  description: Start a native SKILL.md capability without changing its runtime format
  tags: [Extension, Template, Skill]
---

# Native Skill Starter

{
Use when a bounded reusable capability needs its own instructions or supporting resources.
Copy only the fenced file below to the selected Skills scope as {skill-name}/SKILL.md. Replace its native metadata and prompts, then remove source guidance from the result.
Do not copy this wrapper or use route create --template to create SKILL.md: that command creates ordinary routed files with Open Forge metadata.
Follow the active runtime's supported Skill format and discovery rules. Open Forge routing alone does not activate a Skill or grant tools and permissions.
Add scripts, references, or assets only when the capability needs them. Update the containing Skills entrypoint and verify native discovery separately.
}

```markdown
---
name: "{skill-name}"
description: "{Describe the capability, when to use it, and the boundary that distinguishes it from nearby capabilities.}"
---

# {Skill Name}

## Purpose

{State the useful result and when this capability applies.}

## Required Inputs And Resources

{Identify the inputs, tools, and resources actually needed. Use package-relative resource paths and explain when each resource must be read. Remove this section if unnecessary.}

## Instructions

1. {Inspect the relevant input and applicable workspace context.}
2. {Perform the capability's specific work with the available tools and authority.}
3. {Check the result using evidence appropriate to this capability.}

{State the useful response when required input or a capability is unavailable. Do not invent tool access, permission, or accepted project direction.}

## Result

{Describe the output, evidence, and any limitations the recipient needs. Remove these prompts after writing the Skill.}
```
