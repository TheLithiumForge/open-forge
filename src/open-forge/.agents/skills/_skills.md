---
open-forge:
  description: Capabilities to check before specialized work
  tags: [Core, Skill]
---

# Skills

Optional. Native capability packages, each entered through its `SKILL.md`. Nothing else depends on this folder.

## Axioms

- Check these Entries when the work may benefit from a capability.
- Read the matching `SKILL.md` before using a capability, and follow it for metadata, use, instructions and resource loading. Your agent runtime controls activation, invocation, installation and execution.
- When `open-forge-cli/SKILL.md` is present, read it before running `open-forge` commands other than `context`. Use the CLI only when it is available. Otherwise read and maintain the files directly.

## Entries

- [Use the open-forge CLI to load workspace context, find sources, maintain routes and links, and preview installation changes. Use when the command is available and the task needs Open Forge context, navigation, diagnosis, or workspace maintenance.](open-forge-cli/SKILL.md) - #Skill
