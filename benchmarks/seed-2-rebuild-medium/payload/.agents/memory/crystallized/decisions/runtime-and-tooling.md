---
open-forge:
  description: Accepted runtime and tooling choice for the standup CLI
  tags: [Extension, Memory, Decision, Architecture, Tooling, CurrentTruth]
---

# Runtime and Tooling

## Decision

TypeScript with Bun as the primary local toolchain. Zero runtime dependencies; dev-only tooling (TypeScript, type packages) is fine.

## Consequences

- Prefer standard Web and `node:` APIs supported by Bun; keep source Node-compatible.
- Keep generated build output out of source control.

## Fallback

If Bun is unavailable, report that clearly and use the closest Node.js fallback only when continuing beats blocking; document the deviation. Runtimes are sometimes installed but off the default PATH — check before concluding one is absent, and record what was actually found.
