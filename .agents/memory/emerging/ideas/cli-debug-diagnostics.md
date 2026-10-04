---
open-forge:
  description: Explore diagnostic refinements beyond accepted debug-detail output through observed CLI failures
  tags: [Memory, Idea, Contextual, Candidate, CLI, Debug, Diagnostics, Dogfood, Brownfield]
---

# CLI Diagnostic Refinements

## Current Direction

The current CLI requests diagnostics with `--detail debug`. The
[global-flags contract](../../crystallized/documents/cli/contracts/shared/global-flags/interface.md)
defines bounded diagnostics on stderr alongside the full primary result.
There is no separate `--verbose` flag. Ordinary failures remain complete and
human readable without debug detail.

The former `--verbose` premise is superseded. The questions below remain
candidate refinements to evaluate through observed failures. They do not change
the accepted diagnostic contract or authorize another activation mechanism.

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

- Whether debug detail needs an environment-based equivalent, which would
  require a separate accepted change to the current activation contract.
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

Re-examine a detail when an observed failure in an existing workspace shows a
gap in the accepted diagnostics. Promotion requires focused interface and safety
evidence rather than a speculative implementation hook.
