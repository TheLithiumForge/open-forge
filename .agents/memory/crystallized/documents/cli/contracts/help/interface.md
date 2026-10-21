---
open-forge:
  description: List help topics and print concepts in text or JSON
  responsibility: Define Help inputs, output, errors and public evidence
  tags: [Memory, Crystallized, CLI, Command, Contract, Help, Interface, CurrentTruth, Evergreen]
---

# Help Interface Contract

## Status And Authority

This contract defines the public `help` command. The sibling
[Behavior](behavior.md) defines its operation. The
[Shared CLI Operation Contract](../../shared-operation-contract.md) and
[Shared Result Coordinates](../shared/result-coordinates/interface.md) define shared conventions.
Exact command symbols live in
[HelpDefinitions](../../../../../../../src/cli/operations/OpenForge.Cli.Operations/Commands/Help/HelpDefinitions.cs).

## Purpose

Read built-in explanations of Open Forge without installing or selecting a workspace.

## Syntax And Inputs

```text
open-forge help [<topic>] [global options]
```

The optional operand names one topic, matched exactly. The only topic is `concepts`.
There are no command-specific options. Shared format, detail and finding filters apply.
`--workspace` has no effect because Help reads no workspace files.

`open-forge help --help` shows the command's own usage and options.
Root `open-forge --help` lists Help. Every existing command keeps its own `--help` behavior.

## Human Output

With no topic, Help lists available topic names, one per line, then says
`open-forge <command> --help` shows a command's options.

`help concepts` prints the built-in Markdown unchanged, followed by
`## Writing workspace files` and the complete shipped authoring reference without its title line.
Before the reference, a short note says that a workspace's `.agents/authoring.md`
and `authoring.overwrite.md` are the version that applies and that
`open-forge context .agents/authoring.md` returns them.
Markdown is neither wrapped nor styled. All detail levels retain the complete topic.

## Structured Output

JSON uses the shared schema-3 envelope. The concrete
[HelpData](../../../../../../../src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Help/Models/HelpData.cs)
defines command data. `topic` names the requested topic or is null when omitted.
`markdown` contains the complete text for a valid topic and is null otherwise.
`topics` lists available topic names. These members remain at every detail level.
The workspace is null and effects are empty.

## Semantic Results

A topic list or valid topic returns `completed`, exit 0.
An unknown topic returns `invalid-input`, exit 4. Its cause lists the available topics.
If the topic names a top-level command, the cause also suggests that command's `--help`.
For example, `help install` suggests `open-forge install --help`.
Help never prompts or acquires a workspace lock.

Successful text goes to stdout. Invalid text goes to stderr.
JSON goes to stdout for every status. Bounded debug diagnostics use stderr.
The shared result contract governs unexpected failures and cancellation.

## Errors And Boundaries

### Findings catalogue

| Code | Severity | Family | Meaning | Next |
| --- | --- | --- | --- | --- |
| help.invalid-input | error | invalid-input | The topic or other input is invalid. An unknown topic's cause lists available topics and suggests command help when applicable. | Run `open-forge help`, the suggested command help, or `open-forge help --help` for invalid syntax. |
| help.content-unavailable | error | failed | The embedded content could not be read. | Report the failure and retry with `open-forge help concepts --detail debug`. |
| help.interrupted | error | cancelled | Help was cancelled before completion. | Rerun `open-forge help`. |

## Public Verification

- Snapshot the topic list, concepts and an unknown topic in text and JSON.
- Prove that `help install` suggests `open-forge install --help`.
- Compare the printed concepts prefix byte for byte with its embedded resource.
- Compare the appended authoring body byte for byte with the embedded Framework payload minus its title line.
- Prove that concepts works without a workspace and ignores workspace authoring changes.
- Snapshot root help and `help --help`, and retain existing command help snapshots.
- Prove no persistent effects, correct streams, statuses, exits and complete content at every detail level.

## Related Current Sources

- [Help Command Contract Set](_help.md)
- [Help Behavior Contract](behavior.md)
- [CLI Command Contract Set](../../command-contract-set.md)
