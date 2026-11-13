---
open-forge:
  description: Define desired hosts, contained code roots and receipt-based tool configuration
  tags: [Memory, Document, CLI, Contract, Tool, Configure, Interface, CurrentTruth]
---

# tool configure interface
`tool configure` is a write command that reconciles the stored definition and configuration of one registered tool.

Only an active provider receives launchable MCP members and its required usage Skill. Removing a host releases only matching managed members. Configure downloads nothing.

Status: V1 core contract decided.

## Syntax

Proposed:

```text
open-forge tool configure <id> [--host <claude-code|codex>...] [--code-root <path>] [--allow-session-daemon] [--allow-path <path>...] [--automatic] [--dry-run] [global flags]
```

Omitted hosts and code root keep the recorded values. Repeated hosts give the full desired host set. Source and version flags aren't accepted. The help purpose is "Reconcile one registered tool's settings and desired hosts."

## Changes

Use the registered pin's stored definition and receipts. Validate required guidance before publishing launchable members. Safely missing guidance can be restored only without a covering exclusion. Changed or excluded guidance blocks.

A changed CodeGraph root marks its index unusable and prints a rebuild action. Configure neither builds nor deletes that index. CBM accepts only the physical workspace root `.` in V1. A nested root is invalid input before effects.

Settings keep the recorded pin and roles while hosts and code root follow the verified result. A settings and lock mismatch is incomplete configuration. It doesn't authorize a silent host change. The shared [tool operations](../../../shared-operation-contract.md#tool-operations) define writer data and safety. `role` is null and `pin` is the registered pin.

## Conformance

Required cases are:

- Omitted configuration values and full repeated-host replacement.
- Removed host members with unrelated content kept byte-for-byte.
- Missing, edited and excluded required guidance.
- Nested CBM root rejection before effects.
- Root change with rebuild instructions and unchanged index bytes.
- No downloads and no source-catalogue dependency.
- Preview parity, stale-plan rejection and recoverable paired writes.

The [behavior](behavior.md) defines the checked flow.
