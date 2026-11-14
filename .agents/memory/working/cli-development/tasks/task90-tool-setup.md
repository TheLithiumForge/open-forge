---
open-forge:
  description: Implement issue 23 tool setup through the round-2 spec and supervised lanes
  tags: [Memory, Working, CLI, Task, Contextual, Active]
---

# Tool setup

Tool setup is Task 90, which implements published tool installation and additive configuration through supervised lanes.
For example, bare Install restores the exact lock versions while tools run through their direct commands.

Status: Task 90 “Tool setup” (phase 2/3): milestone 1/7.

## Outcome

Implement the [Tool setup design](../../../crystallized/documents/cli/technical-designs/tool-setup.md), updated by the maintainer's [round-2 decisions](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6868316792).
The detailed 14 November 2026 source is `of-tool-setup-decisions-2.md`.
It overrides the earlier respec.
The maintainer accepted Phase 1 and authorized Phases 2 and 3 on 14 November 2026.
Report at each phase boundary and stop for the overseer's CI results at PUSH REQUEST.

## Milestones

Complete these milestones in order:

1. Fold round 2 into the spec, schemas, shared types and outlines, verify and commit.
2. Freeze strict lane packets and create one worktree per worker.
3. Start the Worker Watch lanes and report Phase 2.
4. Integrate A through D with local no-fast-forward merges and focused checks.
5. Complete E's catalogue entries, Windows journeys and user docs.
6. Run one fresh adherence review and close every gap.
7. Request hosted CI through the overseer and resolve every reported failure.

## Execution

Phase 1 is direct work by the mastermind.
Phase 2 uses `gpt-6.1-sol` with `xhigh` effort through Worker Watch in PowerShell.
The user's model choice overrides the older workspace roster.
Each worker has its own worktree and strict file assignment.
The [lane packets](../tool-setup/packets/_packets.md) define their scope and focused checks.
Workers return a checked candidate before the coordinator sends their commit continuation.
The mastermind integrates shared composition and final payload snapshots.
The user authorizes local lane merges, overriding the lane file's earlier no-merge rule.
No worker pushes, opens a PR or changes GitHub.

## Evidence

Phase 1 changes documents, schemas and compile-only types. Git keeps the previous tracked files.
The normal BCL and existing dependencies cover these data shapes. No exceptional mechanism is chosen.
Use a managed build, touched tests, formatting, Markdown links, the site build and both .NET checks.
Full suites and Native AOT run in CI. A local NU1903 override stays on the invocation.
Implementation handles developer files and native account effects.
Tests use owned temporary homes. Windows journeys use temporary repositories under D:.
Never modify real shell profiles or persistent user PATH during those journeys.
Tools add no workspace state, lease or recovery dependency under `.open-forge-state/`.

## Checkpoint

Done: Phase 1 is accepted. Recorded the maintainer's C1 through C7 decisions, including project-only unattended removal and kept account-wide settings.
Now: Task 90 “Tool setup” (phase 2/3): milestone 1/7. Freezing worker packets and the implementation base.
Next: Verify and commit the updated base, create isolated worktrees and start the Sol xhigh swarm.
Blocker: None. C1 through C7 are decided.

## Phase 1 checks

The final local checks on 14 November 2026 passed:

- Release solution build with zero warnings and errors. The NU1903 override is invocation-only.
- `ToolSetupDocumentationTests`, 12 passed, zero failed and zero skipped. This includes Markdown links and fresh schema derivation.
- `npm run check:dotnet:whitespace` and `npm run check:dotnet:style`.
- Prettier on changed Markdown and JSON, plus the Docusaurus production build.
- Route Index, followed by a dry run showing all 14 selected catalogues current.
- LF and Git whitespace checks, all 26 copy-source paths and matching CLI Skill reference copies.

No command implementation, real tool installation or payload-snapshot regeneration ran in Phase 1.
The [recorded choices](../../../crystallized/documents/cli/technical-designs/tool-setup-lanes.md#recorded-choices) now contain the maintainer's C1 through C7 answers.
Codebase Memory account uninstall needs live interactive confirmation naming its effects.
With `--yes` or no terminal, remove only Open Forge project additions and print that command.
Project Remove keeps account-wide settings and prints their manual change commands.

## Runs

The managed Worker Watch session is `s_b74431d77d3a`.
No worker agent or run has started yet.

## Phase 2 base

The C1 through C7 contract update and five lane packets passed these local checks:

- Release solution build with zero warnings and errors.
- `ToolSetupDocumentationTests`, 12 passed, zero failed and zero skipped.
- Both required .NET formatting checks, Prettier and the Docusaurus production build.
- Markdown links, current packet catalogues, LF and matching CLI Skill copies.

The frozen interfaces now carry the entry and workspace for channel installation, the invocation for native config, file hash expectations and backup completion.
Native setting change arguments are named as manual instructions.
These are compile-only changes before the worker execution base is created.
