---
open-forge:
  description: Define independent tool catalogue and installed observations without effects
  tags: [Memory, Document, CLI, Contract, Tool, List, Behavior, CurrentTruth]
---

# tool list behavior

`tool list` is a read operation that combines independent catalogue and workspace settings facts.

It keeps readable installed facts when catalogue observation fails. It never substitutes an empty set for unavailable settings or receipt coverage.

Status: V1 core contract decided.

## Flow

The operation follows these steps:

1. Validate the complete request before resolving sources.
2. Resolve the exact workspace and selected catalogue under the shared manifest contract.
3. Read settings independently, then read lock details for each recorded tool.
4. Match records by exact ID and pin. Record missing or mismatched verification separately.
5. Apply the requested role capability filter without inventing unavailable capabilities.
6. Group first-party then third-party, sort unknown origin last, then sort IDs and versions ordinally and form one result with complete counts or unknown counts.

## Limits

List launches nothing, downloads nothing and takes no writing lease. It creates no runtime, recovery or cache state. Settings edits aren't installation evidence. Catalogue order never selects a default or active provider.

The [interface](interface.md) defines syntax, fields and caller results.
