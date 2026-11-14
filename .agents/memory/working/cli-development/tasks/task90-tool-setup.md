---
open-forge:
  description: Implement issue 23 tool setup through the round-2 spec and supervised lanes
  tags: [Memory, Working, CLI, Task, Contextual, Active]
---

# Tool setup

Tool setup is Task 90, which implements published tool installation and additive configuration through supervised lanes.
For example, bare Install restores the exact lock versions while tools run through their direct commands.

Status: Task 90 “Tool setup” (phase 1/3): milestone 1/7.

## Outcome

Implement the [Tool setup design](../../../crystallized/documents/cli/technical-designs/tool-setup.md), updated by the maintainer's [round-2 decisions](https://github.com/TheLithiumForge/open-forge/issues/23#issuecomment-6868316792).
The detailed 14 November 2026 source is `of-tool-setup-decisions-2.md`.
It overrides the earlier respec.
Report and stop after each of the user's three phases.

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

Done: Folded R2-01 through R2-16 into contracts, schemas, lock, compile-only models, planned docs and strict lane assignments. Local checks pass.
Now: Task 90 “Tool setup” (phase 1/3): milestone 1/7. Phase 1 is complete for the maintainer's report.
Next: Stop with the Phase 1 report. On continuation, write final packets and start the Phase 2 Worker Watch swarm.
Blocker: None. Reversible choices are recorded for review.

## Phase 1 checks

The final local checks on 14 November 2026 passed:

- Release solution build with zero warnings and errors. The NU1903 override is invocation-only.
- `ToolSetupDocumentationTests`, 12 passed, zero failed and zero skipped. This includes Markdown links and fresh schema derivation.
- `npm run check:dotnet:whitespace` and `npm run check:dotnet:style`.
- Prettier on changed Markdown and JSON, plus the Docusaurus production build.
- Route Index, followed by a dry run showing all 14 selected catalogues current.
- LF and Git whitespace checks, all 26 copy-source paths and matching CLI Skill reference copies.

No command implementation, real tool installation or payload-snapshot regeneration ran in Phase 1.
The [recorded choices](../../../crystallized/documents/cli/technical-designs/tool-setup-lanes.md#recorded-choices) keep C1 through C7 as maintainer questions.
Codebase Memory native uninstall needs separate live confirmation for its protected effects.
Without it, the planned operation stays pending instead of removing Open Forge additions first.

## Runs

No Worker Watch agent or run has started for this task.
