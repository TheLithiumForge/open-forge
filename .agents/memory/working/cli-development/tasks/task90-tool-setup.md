---
open-forge:
  description: Active Task 90 to rebuild open-forge tool as three lean commands that use each tool's own install and setup
  tags: [Memory, Working, CLI, Task, Tool, Contextual, Active]
---

# Task 90: Lean tool setup

Status: Spec in `f84711f9b` and implementation in `b8046c0a7` on `feature/tool-setup-lean`, 18 November 2026. Focused local evidence passed. Hosted CI and maintainer review remain. Draft PR #32 keeps the fuller round-2 work as a parked record.

## Outcome

`open-forge tool list`, `tool install` and `tool remove` help a person install supported tools with each tool's documented method and configure the project to use them. Open Forge adds only the changes it needs for integration: keeping archived Open Forge memory out of each tool's index, and a `tools` list in the project settings.

## Maintainer direction

On 17 November 2026 the maintainer asked to streamline: tag each tool with its package manager, show the install command and Open Forge's own changes, and explain in simple terms what to do when something doesn't work. Open Forge doesn't manage tool settings, private runtimes or PATH. The goals are usefulness, automatic configuration and safety. Earlier direction still holds: "no damage to the user path ever", and removal is "local first, then a question that defaults to Keep".

The maintainer approved the lean plan the same day with these choices:

- Each catalogue entry carries a category, the install method and its runner, the tool's own setup and a one-sentence uninstall description.
- Configured tools live in a `tools` list in `.agents/open-forge.json`, with the smallest schema bump.
- Bare `tool install` installs the configured tools missing on this machine and never reruns setup.
- Remove undoes Open Forge's changes, then asks "Run it too?" about the tool's uninstall, defaulting to no.
- The lock `tools` map, Tool findings in `doctor` and `status`, and the Codebase Memory `LICENSE` notice go. The `CliPrompts` Enter-choice change stays.

## Sources

- The [Tool contracts](../../../crystallized/documents/cli/contracts/tool/_tool.md) define the commands.
- The [Tool setup design](../../../crystallized/documents/cli/technical-designs/tool-setup.md) defines the catalogue, the verified upstream commands and the parked ideas.
- The [entry review checklist](../../../crystallized/documents/maintenance/tools.md) controls later catalogue updates.

## Evidence

Focused local evidence is the build, `check:dotnet:whitespace`, `check:dotnet:style` and filtered runs of the regenerated snapshot classes. CI runs the full suites.

The managed Windows run on 18 November 2026 passed 789 of 801 selected Integration tests, with 12 skipped, across the Tool, help, Install, Status, Update, Route Include and Exclude, Distribution, Settings, Architecture and Cleanup classes. It also passed 729 of 729 selected Unit tests across Settings, Recovery, Tools and shared Presentation. The docs site typecheck and build passed. Native AOT and EndToEnd weren't run locally.
