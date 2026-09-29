---
open-forge:
  description: Specialized capabilities provided through native SKILL.md packages
  tags: [LoadNow, Core, Skill]
---

# Skills

## Which specialized capability would help with this work?

A Skill is a native capability package entered through `SKILL.md`. That file defines how to use the Skill and its supporting resources.

## Axioms

- Check `Entries` when the work may benefit from a Skill.
- Follow the selected `SKILL.md` for metadata, use, instructions, and resource loading.
- The active agent runtime controls Skill activation, invocation, installation, and execution.

## Entries

- [Use the open-forge CLI to see what loads, find sources, keep links and Entries correct, and preview workspace changes. Use when the command is available and the task needs Open Forge context, navigation, diagnosis, or changes to many workspace files.](open-forge-cli/SKILL.md) - #Skill
- [Select and follow an installed workflow for project vision, architecture, planning, implementation, debugging, review, or coordinated delivery. Use when the user requests a workflow or an installed recipe would materially improve the task. Recipes can combine the workspace's configured Skills, tools, and agents. This Skill does not supply their runtime.](use-workflow/SKILL.md) - #Skill
- [Delegate coding or review tasks to supervised Codex workers with per-agent status, persistent threads, provenance, isolated worktrees, and a human-visible monitor. Use when the user asks for monitored or parallel workers, or to delegate through Worker Watch instead of raw codex exec.](worker-watch/SKILL.md) - #Skill
