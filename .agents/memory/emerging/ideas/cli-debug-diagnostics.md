---
open-forge:
  description: Define the accepted `--verbose` diagnostic content after ordinary human-readable failures and dogfood evidence exist
  tags: [Memory, Idea, Contextual, Candidate, CLI, Debug, Diagnostics, Dogfood, Brownfield]
---

# CLI Verbose Diagnostics Details

## Current Direction

The new CLI will provide `--verbose`. Ordinary failures remain complete and
human readable without it. The exact diagnostic content, redaction, and
unavailable-stream behavior remain open until the CLI can dogfood real failures.

Deleted CLI v2 explored a safe failure boundary. Its material now lives in the
[CLI-v2 archive](../../archived/cli-v2/_cli-v2.md) and is raw input only. This
idea does not accept a detailed diagnostic contract.

## Candidate Boundary

The executable result and rendering boundary is the likely integration point.
It already knows the failure stage, selected operation, presentation mode, and
whether ordinary rendering or writing failed. Exploration should determine
whether one opt-in diagnostic hook can observe failures before safe rendering
without coupling handlers to process streams or caught exceptions.

Questions to answer through dogfooding include:

- Whether `--verbose` needs an environment-based equivalent.
- Which unexpected failure stages may expose stacks and which failures have no
  safe diagnostic stream left.
- Whether requested diagnostics use stderr in both human and JSON modes while
  structured stdout remains one valid result document.
- How paths, command input, environment values, and secrets are warned about,
  redacted, or excluded.
- Whether callback and error-event failures retain one useful originating
  cause without exposing a second implementation-specific event.
- How direct, built-process, and brownfield evidence prove opt-in behavior and
  unchanged ordinary failure safety.

## Revisit Point

Re-examine these details when the new CLI can be exercised against an existing
Open Forge workspace. Promotion requires focused interface and safety evidence
rather than a speculative implementation hook.
