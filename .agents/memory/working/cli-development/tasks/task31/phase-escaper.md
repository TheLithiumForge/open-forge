---
open-forge:
  description: Completed Task 31 M3 escaper subtask delivered with the Task 30 presentation contract
  tags: [Memory, Working, CLI, Task, Subtask, Contextual]
---

# Task 31 — M3 Single Text Escaper

## Status

Complete as part of the Task 30 G4 rendering-system work. The reviewed output
diff, accepted presentation contract, and shared text-escaping boundary were
delivered in the same output-change packet as the renderer rewrites.

## Evidence source

The sealed [Implementation Duplication analysis](../../../../emerging/analysis/cli-experience-audit/implementation-duplication.md)
found eight escapers with four incompatible answers. The [CLI output design
analysis](../../../../emerging/analysis/cli-experience-audit/command-output-design.md)
provided the historical contract input for the completed rewrite.

## Actionable boundary

- Select one accepted owner and one standard escaping behavior through the G4
  contract; delete the other seven only after the baseline is captured, then
  implement M3 with the renderer rewrites.
- Capture the current command outputs before changing the implementation and
  review the intentional output diff afterward.
- Move `DiagnosticValueLimit = 240` with the selected owner; do not let the
  constant move silently create a new shared policy.
- Preserve JSON schema, status, exit, stream, and ordering meaning except for
  the explicitly accepted escaping correction.
- Treat platform line-ending normalization as a separate human-presentation
  concern; do not make a JSON encoder the owner of arbitrary human text.
- During M3, review the shared `CliHumanText.PlatformLineEndings` helper for
  incorporation or improvement: confirm every structural human-output caller
  is covered, retain mixed LF/CRLF/CR tests, and preserve authored content,
  JSON output, and persisted bytes exactly.
