---
open-forge:
  description: Define Tool Run operands, raw streams and strict executable selection
  tags: [Memory, Document, CLI, Contract, Tool, Run, CurrentTruth]
---

# tool run interface

`tool run` is a command that forwards a call to one installed workspace tool.

The first operand is a declared tool ID. It never selects a path, shell string or unknown name.

Status: V1 contract decided. Implementation pending.

## Syntax

```text
open-forge tool run [--workspace <dir>] <tool> [--] <args...>
```

All arguments after the tool name are forwarded verbatim as separate arguments. One optional delimiter immediately after the ID is consumed. Open Forge options are read only before the ID. Child help and child options stay child input.

## Process

Run only the recorded executable for the settings pin in its verified store. Keep CWD. Forward stdin, stdout, stderr, exit code and signals unchanged. Add no successful-run output. Raw child streams are an exception to the shared command report envelope. No shell interprets the argument array.

## Workspace

An explicit workspace before the ID selects that directory. Otherwise use `CLAUDE_PROJECT_DIR` when set, then walk upward from CWD. A supplied but invalid selection fails without falling back to another workspace. Keep the child's CWD unchanged.

## Failures

Reject undeclared IDs, absent or mismatched pins, unavailable receipts and missing or changed store files before launching a process. A declared missing tool gives the exact next step `open-forge tool install`. Interactive Run offers installation through the same consent flow. Non-interactive Run fails clearly with blocked exit 5. It never installs silently.

Pre-launch failures write a diagnostic to stderr without a schema-3 envelope. They use these exits:

| Condition                                                            | Exit |
| -------------------------------------------------------------------- | ---- |
| Invalid ID, undeclared or unknown tool, invalid input                | 4    |
| Declared pin missing locally, pin mismatch or integrity check failed | 5    |
| Workspace selection or verification evidence unavailable             | 3    |
| Child could not start                                                | 1    |
| Cancelled before launch                                              | 130  |

After launch, forward the child's exit unchanged. Successful dispatch adds no output. JSON or detail selection never wraps child output. Writer and consent flags aren't Run flags.

The [behavior](behavior.md) defines the pre-launch checks.
