---
open-forge:
  description: Define passive exact tool definition resolution and requirement reporting
  tags: [Memory, Document, CLI, Contract, Tool, Inspect, Behavior, CurrentTruth]
---

# tool inspect behavior
`tool inspect` is a read operation that resolves one definition and projects its requirements.

Definition admission and current installation are separate observations. Describing a definition never installs or activates it.

Status: V1 core contract decided.

## Flow

The operation follows these steps:

1. Validate one ID, optional source and optional exact pin.
2. Read the exact source and validate its bytes, schema and identities.
3. Resolve the default or requested exact definition without merging or fallback.
4. Report the current native target and the definition's compatibility and activation requirements.
5. Read independently available installed settings and lock details. Report the saved telemetry choice, defaulting to off when omitted. If settings are unreadable, report the choice as unavailable.
6. Form one result with artifacts, destinations, process policy, limits and external instructions.

## Limits

Inspect takes no writing lease, launches no version probe and creates no state. Required future artifact or process checks stay conditional. It doesn't contact an artifact server to fill missing facts.

The [interface](interface.md) defines syntax and caller results.
