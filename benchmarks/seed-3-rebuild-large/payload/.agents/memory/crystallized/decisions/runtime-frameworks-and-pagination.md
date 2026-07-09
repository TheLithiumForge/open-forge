---
open-forge:
  description: TypeScript with Bun tooling, node:http with no frameworks, and deliberately simple pagination
  tags: [Extension, Memory, Decision, Architecture, Tooling, CurrentTruth]
---

# Runtime, Frameworks, And Pagination

## Decision

TypeScript across all three packages, Bun as the local toolchain, zero runtime dependencies. The server uses `node:http` directly — no Express, no router library, no ORM, no validation library. `GET /transactions` supports `limit`/`offset` query params and nothing fancier.

## Rationale

The MVP's framework stack was more code than the product. At personal-ledger scale, a hand-rolled router over `node:http` is a screen of code, and limit/offset is honest pagination — cursors would be resume-driven engineering.

## Consequences

- Hand-rolled request routing and body parsing, with strict content-type and JSON validation at the boundary.
- Keep source Node-compatible (`node:` imports, erasable TypeScript); dev-only type packages are fine.
- If Bun is unavailable, report it and use the closest Node fallback only when continuing beats blocking; runtimes are sometimes installed but off the default PATH — check before concluding absence.
