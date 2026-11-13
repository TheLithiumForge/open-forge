---
open-forge:
  description: Define desired hosts, contained code roots and stored tool configuration
  tags: [Memory, Document, CLI, Contract, Tool, Configure, Interface, CurrentTruth]
---

# tool configure interface
`tool configure` is a write command that reconciles the stored definition and configuration of one registered tool.

The tool's own installer handles its agent files and native settings. Open Forge records the requested configuration and command results. Configure downloads nothing.

Status: V1 core contract decided.

## Syntax

Proposed:

```text
open-forge tool configure <id> [--host <claude-code|codex>...] [--code-root <path>] [--allow-session-daemon] [--allow-path <path>...] [--automatic] [--dry-run] [global flags]
```

Omitted hosts and code root keep the recorded values. Repeated hosts give the full desired host set. Source and version flags aren't accepted. The help purpose is "Reconcile one registered tool's settings and desired hosts."

## Changes

Use the registered pin's stored definition and receipts. Check declared command outputs against grants and exclusions before invoking the tool. Open Forge never edits agent files or tool guidance.

A changed code root reports the need for an external rebuild. Configure neither builds nor deletes that index. Tool-specific root restrictions belong to the next branch's reviewed definition.

Settings keep the recorded pin and roles while hosts and code root follow the verified result. A settings and lock mismatch is incomplete configuration. It doesn't authorize a silent host change. The shared [tool operations](../../../shared-operation-contract.md#tool-operations) define writer data and safety. `role` is null and `pin` is the registered pin.

## Conformance

Required cases are:

- Omitted configuration values and full repeated-host replacement.
- Removed-host uninstall and desired-host installer invocation order.
- Declared tool output permissions and covering exclusions.
- Contained root validation before effects.
- Root change with rebuild instructions and unchanged index bytes.
- No downloads and no source-catalogue dependency.
- Preview parity, stale-plan rejection and recoverable paired writes.

The [behavior](behavior.md) defines the checked flow.
