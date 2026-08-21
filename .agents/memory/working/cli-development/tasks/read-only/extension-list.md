---
open-forge:
  description: Implement Extension catalogue and source listing without lifecycle inference
  tags: [Memory, Working, CLI, Task, Extension, List, ReadOnly, Contextual]
---

# Implement Extension List

## Task State

- State: Planned after Foundation; may proceed independently of source-query
  commands once shared package-source contracts freeze.
- Parent: [Read-Only Commands](_read-only.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/extension/list/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/extension/list/behavior.md).

## Expected Outcome

`extension list` reports accepted Extension catalogue identities and source facts
deterministically without installing packages, reading legacy lifecycle state, or
guessing workspace ownership.

## Architecture

- Keep command source at `Commands/Extension/List/`.
- Establish `Commands/Extension/Shared/Catalogue/` only for package identity,
  manifest facts, and source selection known to be consumed by Inspect and later
  lifecycle commands.
- Keep list filters, rows, findings, result, renderers, and help local.
- Reuse shared physical safety and strict reads for local package sources.

## Evidence

Cover catalogue ordering, duplicate identities, malformed manifests, unavailable
sources, local/external no-fetch boundaries, filters, human/JSON/help/diagnostics,
workspace absence where contracted, streams, exits, no writes, and AOT.

## Stop Conditions

Stop before lifecycle inference, package download, dynamic plug-in loading,
registry scanning outside accepted sources, or promotion of list rows as shared
Extension models.
