---
open-forge:
  description: Specialized capabilities provided through native SKILL.md packages
  tags: [LoadNow, Core, Skill]
---

# Skills

## Which specialized capability would help with this work?

A Skill is a native capability package entered through `SKILL.md`. That file defines how to use the Skill and its supporting resources.

## Axioms

- When `open-forge-cli/SKILL.md` is present, read it when this entrypoint loads so command options and context-loading advice are available from the start. Respect an intentional omission and use the loader's CLI summary instead. Use the CLI only when it is available; otherwise read and maintain the files directly.
- Check `Entries` when the work may benefit from a Skill.
- Follow the selected `SKILL.md` for metadata, use, instructions, and resource loading.
- The active agent runtime controls Skill activation, invocation, installation, and execution.

## Entries

- [Use the open-forge CLI to load workspace context, find sources, maintain routes and links, and preview installation changes. Use when the command is available and the task needs Open Forge context, navigation, diagnosis, or workspace maintenance.](open-forge-cli/SKILL.md) - #Skill
