---
open-forge:
  description: Show supported tools by category with their install method and state
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool list interface

Tool list interface is the syntax and report of `tool list`.
For example, the CodeGraph row shows an `npm` tag, "configured" and "installed 1.6.2".

Status: Lean design decided on 17 November 2026.

## Syntax

```text
open-forge tool list [global flags]
```

The help purpose is "List supported tools, their install method and their state."

## Report

Text groups tools by category, in catalogue order. A category with no tools isn't shown.
Each tool is one indented row with:

- its ID
- its install-method tag, `npm` or `script`, colored by the shared color policy
- its state: "configured" when the project settings list it, and "installed" with its version or "not installed"
- its one-line purpose

`standard` detail adds the name, upstream URL and licence under each row.
JSON always carries every fact for every tool: ID, name, category, purpose, method, upstream, licence, configured, integrated, installed and version.
`integrated` says whether Open Forge's integration changes are present in this project.


## Findings catalogue

| Code | Meaning |
| --- | --- |
| `tool-list.settings-unavailable` | Fix `.agents/open-forge.json`, which can't be read or parsed (warning) |
| `tool-list.integration-conflict` | Fix the named tool file. Open Forge left it unchanged (warning) |
| `tool-list.invalid-input` | Correct the named operand or option |
| `tool-list.interrupted` | The command was cancelled. Nothing more ran or changed |

## Example

```text
open-forge tool list
```
