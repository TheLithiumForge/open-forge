---
open-forge:
  description: Define passive tool verification and index evidence without probes or repair
  tags: [Memory, Document, CLI, Contract, Tool, Status, Behavior, CurrentTruth]
---

# tool status behavior
`tool status` is a read operation that compares recorded tool facts with current passive evidence.

A settings entry selects the subject. Only matching verified receipts establish an installation. Missing receipts keep the settings row but make verification unavailable.

Status: V1 core contract decided.

## Flow

The operation follows these steps:

1. Validate the request and read exact workspace settings.
2. Select the requested ID or all recorded IDs in ordinal order.
3. Read lock records and account-local bindings without creating stores.
4. Compare installed file hashes, generated member values and required guidance with receipts.
5. Check covering exclusions and passive native index metadata.
6. Report source identity, completion evidence, limits and store coverage independently.
7. Form one result using shared precedence and the command's warning rules.

## Limits

No probe, repair, extraction, journal, cache, runtime folder or lease can be created. If a safe read isn't available, report that check unavailable. Backup observation reports sibling `.bak` paths without disclosing saved contents or following links.

The [interface](interface.md) defines syntax and evidence states.
