---
open-forge:
  description: Define fast transparent execution of the current workspace tool pin
  tags: [Memory, Document, CLI, Contract, Tool, Exec, Interface, CurrentTruth]
---

# tool exec interface

`tool exec` is the command that runs the current workspace's pinned tool through its own verified store launcher.

Agent entries use this path so a project uses its recorded version without system PATH changes. It is also the shell form for index maintenance and queries.

Status: V1 contract decided. Implementation waits for the core integration.

## Syntax

Proposed implementation:

```text
open-forge tool exec [--workspace <dir>] <tool> [--] <args...>
```

All arguments after the tool name pass unchanged to the tool. One literal leading `--` is optional. Open Forge's workspace option goes before the tool name. In particular, the hook form is `open-forge tool exec codegraph prompt-hook` so native uninstall recognizes `codegraph prompt-hook`.

## Workspace

An explicit workspace selects that directory. Otherwise accept `CLAUDE_PROJECT_DIR` for hosts that start servers elsewhere. Without either, walk up from the current directory to the Open Forge workspace. Read its recorded exact pin and verified store reference. Don't choose a catalogue default, a different version or a global binary.

## Process

Run that version's own launcher with the reviewed process environment and physical workspace CWD. Forward stdin, stdout, stderr, exit code and signals unchanged. On success add no output or result envelope. Start quickly because the Claude hook runs on prompts. No downloads, configuration repair, indexing decisions or version fallback happen in Exec.

If no tool is installed, or the workspace pin's verified version is missing, fail clearly with the workspace, tool, pin and useful next action. Report Open Forge launch failures separately from forwarded child output. Help stops before workspace resolution.

## Conformance

Use local fixture launchers to check these cases:

- Parent workspace detection, `CLAUDE_PROJECT_DIR` and explicit workspace selection.
- Exact pin selection and failure for uninstalled or missing versions.
- Literal argument forwarding, optional delimiter and downstream flags.
- Inherited streams, exit codes, cancellation and signals.
- Silent successful startup and no writes or downloads by the dispatcher.
- Launcher paths with spaces and Unicode.

The [CodeGraph V1 design](../../../technical-designs/codegraph-v1.md) defines agent entry generation and its one command resolver.
