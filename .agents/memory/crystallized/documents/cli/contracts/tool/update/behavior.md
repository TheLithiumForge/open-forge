---
open-forge:
  description: Define Tool update checks and effects
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool update behavior

Tool update behavior is the flow that updates tools through their published channel.
Update replaces the active version and rewrites absolute launchers while recording the new exact version.

Status: Round-2 design decided on 14 November 2026. Command implementation is pending.

## Flow

Preflight all selected tools, including config drift. Resolve the target, show the plan and obtain confirmation or --yes. Use the existing channel unless an override or unavailable prerequisite requires a disclosed alternative. Replace bytes, rewrite launchers, run the target's own setup and migrate recorded additions. Keep older supported catalogue profiles. A newer upstream release alone doesn't justify 'update Open Forge first'.
Use the [common Behavior](../behavior.md) for detection, channels, native setup and additive effects.
Keep syntax and reports in the [Interface](interface.md).

## Results

Use the common status precedence and shared schema-3 exits.
Keep actual effects, conflicts and unstarted work separate.
Dry-run changes no Tool configuration or records.

## Conformance

Required cases cover:

- No update, latest support, older explicit spec, known newer catalogue, no duplicate install, changed requirements and failed config migration.
- Stable ordering and unchanged unrelated data.
- Shared text and JSON facts and global flags.
