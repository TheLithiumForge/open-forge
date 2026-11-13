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

Run only the recorded executable for the settings pin in its verified store. Keep CWD. Pass raw stdin, stdout and stderr without capture. Add no successful-run output. Raw child streams are an exception to the shared command report envelope. No shell interprets the argument array. Run imposes no execution timeout.

## Signals

The child shares the Windows console or Unix foreground process group. Run keeps that console and process group, so interactive Ctrl+C or Ctrl+Break reaches the child through the OS.

When Run receives Ctrl+C, Ctrl+Break or SIGINT, it cancels its own default termination and keeps waiting. The child decides how to exit, and Run returns that exit code. Unix SIGQUIT follows the same behavior.

When Run alone receives SIGTERM or SIGHUP, it waits one second for the child to exit. If the child is still running, Run calls `Process.Kill(entireProcessTree: true)` and waits for it to exit. Run then returns 143 for SIGTERM or 129 for SIGHUP. Managed `Console.CancelKeyPress` and `PosixSignalRegistration` handle these events. Native interop is prohibited.

For a verified MCP invocation, closing the parent input pipe starts the same one-second grace and process-tree stop, then returns 129. The [MCP lifetime record](behavior.md#mcp-lifetime) identifies those invocations. Ordinary CLI pipe EOF keeps normal child input behavior.

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

After launch, return the child's exit unchanged unless the parent-stop policy applies. Successful dispatch adds no output. JSON or detail selection never wraps child output. Writer and consent flags aren't Run flags.

The [behavior](behavior.md) defines the pre-launch checks.
