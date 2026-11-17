---
open-forge:
  description: Define tool IDs, consent and shared result coordinates
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool interface

Tool interface is the shared input, consent and report shape of the three tool commands.
For example, `tool install codegraph --dry-run` shows the exact commands and file changes without running anything.

Status: Lean design decided on 17 November 2026.

## Inputs

The three leaves are defined by the [group](_tool.md).
All leaves accept the existing [global flags](../shared/global-flags/interface.md).
Install and Remove also accept `--yes` and `--dry-run`.

A tool ID is an exact catalogue key, such as `codegraph` or `codebase-memory`.
An unknown ID, an empty ID or an extra operand is invalid input.
There is no version selector. Each catalogue entry has one tested version.
Repeated Boolean flags are idempotent.

## Consent

Nothing runs without consent, and every command is shown exactly before it runs.
Install shows its plan and asks `Continue? [y/N]`. The default answer is no.
`--yes` accepts that plan.
Remove applies Open Forge's own changes without a plan question, then asks about the tool's own uninstall. That question defaults to no, and `--yes` takes the default.

`--format json` never prompts.
If Install needs consent and neither a live terminal nor `--yes` is available, it runs nothing and reports the shared confirmation family: `<Command> needs confirmation, and this session cannot ask.` It has the `invalid-input` status and exit 4.
A declined plan is `cancelled`, exit 130.
`--dry-run` shows the same plan, asks nothing and changes nothing.

## Reports

Use the existing schema-3 envelope, statuses, exits, detail levels and streams from [Result coordinates](../shared/result-coordinates/interface.md).
The command identity is `tool <leaf>`.
Every leaf reports `data.tools`, an array in catalogue order.
List groups its rows by category in text and carries the category on each JSON entry.
Install and Remove report `preview` and, for each tool, its ordered steps with their command, plain effect and outcome, and Open Forge's file changes with their path, plain purpose and outcome.
A failed step adds its exit code, the tail of its output and a plain next step.
Text and JSON consume the same facts.

## Results

Cancellation has priority once invocation starts.
Otherwise precedence is invalid input, blocked, failed, incomplete, completed with warnings and completed.
Use the exact shared status spellings and exits.
Install checks every required package manager or runner before it runs anything. If one is missing, it runs nothing.
Once effects start, stop at the first failure and report completed and not-started work separately.
No result promises package-manager rollback.
