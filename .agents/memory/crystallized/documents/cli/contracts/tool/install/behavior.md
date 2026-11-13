---
open-forge:
  description: Define Tool install checks and effects
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool install behavior

Tool install behavior is the flow that installs tools and adds their project configuration.
With CodeGraph 1.6.x declared, a teammate runs bare Install and receives a working direct `codegraph` command plus project configuration.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

## Flow

Validate declarations and local facts, choose a channel, resolve the exact version, present the plan and obtain required consent. Install only when needed. Verify the direct command, run the declared setup, apply config changes and guidance, then verify and publish records. A repeat reconfigures and returns a no-op for values already desired. Team baseline selection is a review proposal in the records contract.
Use the [common Behavior](../behavior.md) for detection, channel selection, safe edits and verification.
Keep this command's public meaning in its [Interface](interface.md).

## Results

Read-only checks create no state. Writers publish only verified facts.
An independently completed effect stays in the result if a later step fails.
Use the shared semantic statuses without hiding unavailable facts or unfinished work.

## Conformance

Required cases cover:

- Named and bare requests, earlier ranges, channel override, no second install, repeated configuration, consent missing and supplied, and failure before and after package installation.
- Stable ID order, no duplicate effects and unchanged unrelated bytes.
- Shared text and JSON facts, global flags and schema-3 exits.
