---
open-forge:
  description: Implement exact Extension package inspection without mutation or installation behavior
  tags: [Memory, Working, CLI, Task, Extension, Inspect, ReadOnly, Contextual]
---

# Implement Extension Inspect

## Task State

- State: Planned after Extension List.
- Parent: [Read-Only Commands](_read-only.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/extension/inspect/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/extension/inspect/behavior.md).

## Expected Outcome

`extension inspect` resolves one exact package identity and reports manifest,
payload, compatibility, source, and safety facts without installation or lifecycle
effects.

## Architecture And Promotion

- Keep command source at `Commands/Extension/Inspect/`.
- Compare catalogue identity, manifest parsing, source safety, and package facts
  with Extension List. Promote only identical units to
  `Commands/Extension/Shared/<Capability>/`.
- Keep inspect profile, availability, findings, result, renderers, and help local.
- Package payload inspection uses contained real filesystem reads and never
  executes package content.

## Evidence

Cover exact ID/path selection, ambiguity, malformed or unsupported package,
payload inventory, compatibility facts, source provenance, unavailable data,
human views, JSON, diagnostics, help, no writes, process exits, and AOT. Run all
Extension List regressions and record each promotion.

## Stop Conditions

Stop before code execution, package download, lifecycle interpretation, source
review decisions owned by installation, or speculative shared package models.
