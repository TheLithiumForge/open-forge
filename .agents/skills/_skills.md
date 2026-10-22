---
open-forge:
  description: Capabilities to check before specialized work
  tags: [Core, Skill]
---

# Skills

## Which specialized capability would help with this work?

This folder holds native capability packages, each entered through its `SKILL.md`.

## Axioms

- Check these Entries when the work may benefit from a capability.
- Read the matching `SKILL.md` before using a capability, and follow it for metadata, use, instructions and resource loading. Your agent runtime controls activation, invocation, installation and execution.
- When `open-forge-cli/SKILL.md` is present, read it before running `open-forge` commands other than `context`.

## Entries

- [Use the open-forge CLI to load workspace context, find sources, maintain routes and links, and preview installation changes. Use when the command is available and the task needs Open Forge context, navigation, diagnosis, or workspace maintenance.](open-forge-cli/SKILL.md) - #Skill
- [Select and follow an installed workflow for project vision, architecture, planning, implementation, debugging, review, or coordinated delivery. Use when the user requests a workflow or an installed recipe would materially improve the task. Recipes can combine the workspace's configured Skills, tools, and agents. This Skill does not supply their runtime.](use-workflow/SKILL.md) - #Skill
- [Delegate coding or review tasks to supervised Codex workers with per-agent status, persistent threads, provenance, isolated worktrees, and a human-visible monitor. Use when the user asks for monitored or parallel workers, or to delegate through Worker Watch instead of raw codex exec.](worker-watch/SKILL.md) - #Skill
