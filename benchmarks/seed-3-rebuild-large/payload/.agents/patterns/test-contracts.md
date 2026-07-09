---
open-forge:
  description: What the ledger's tests must cover across the three packages to count as verified
  tags: [Extension, Pattern, Testing, Contract]
---

# Test Contracts

## Required Coverage

- Core: money parse/format/sum exactness (including `12.30` → `1230` with no float intermediate — assert on odd cents), validation field errors, id shape, summary math with reversals netting to zero.
- Server: handlers tested as functions (status + body) for happy paths and every error code in the contract; idempotent POST re-send; import all-or-nothing with row-numbered rejection report; the catch-all producing `internal` shape on a thrown handler.
- CLI: rendering as pure functions; error-contract `code` switching; unreachable-server path (point at a closed port) yielding the friendly message and exit 1.
- One real end-to-end: start the actual server on an ephemeral port against a workspace-local temp data file, drive the actual CLI through add → list → summary → reverse, assert on real stdout.

## Style

Match the runtime's test conventions; keep all fixtures and temp data workspace-local.
