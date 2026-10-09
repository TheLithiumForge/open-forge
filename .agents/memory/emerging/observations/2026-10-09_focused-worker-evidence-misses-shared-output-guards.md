---
open-forge:
  description: Output-wording slices passed their focused tests but broke shared report guards and terminal journeys that live outside the command they changed
  tags: [Memory, Observation, AgentLearning, Contextual, Candidate, CLI, Testing, Orchestration, Dogfood]
---

# Observation: Focused Evidence Misses Shared Output Guards

## Expected Behavior

A slice that changes a command's text output should pass when it runs that command's Unit, Integration and EndToEnd selections and the complete Unit project.

## Observed Failure

Task 76 changed prompt layout and change labels across Install, Update, Remove, Route and Extension commands in four implementation slices. Each slice reported green focused evidence. After integration, the complete managed suite failed three times on tests no slice had selected:

- The cross-command report invariants in `Presentation/Invariants/CliReportInvariantsTests`, which check text and JSON parity, the snapshot naming ladder and live snapshot owners. Their namespace differs from their project, so a `Commands.<Command>*` selection never includes them. They caught a dropped effect row, a new snapshot class missing from a documented exception, misplaced text captures, and unfinished link rewrites removed from the text.
- EndToEnd terminal journeys in other commands' folders that asserted the old position line, the old conversion wording or a permission heading.
- Load-related flakes in two published-terminal journeys that passed on rerun.

## Correction And Evidence

The Overseer fixed each case after integration and reran the complete managed suite. Later packets required a search of every test project for each changed string, the invariant class by its full name, and the complete EndToEnd project. The S4 slice then caught its own cross-command assertions before returning.

## Promotion Signal

When this recurs, add the invariant class and the complete EndToEnd project to the standard evidence of any slice that changes user-facing output, in the worker packet template or the CLI testing guidance. A `Presentation*` selection alone does not find the invariants, because their namespace starts with `OpenForge.Cli.Core.UnitTests`.
