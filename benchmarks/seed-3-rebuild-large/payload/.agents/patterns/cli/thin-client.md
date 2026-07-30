---
open-forge:
  description: The CLI renders and relays; it never computes business results
  tags: [Extension, Pattern, Cli, Boundary]
---

# Thin Client

## Shape

- The CLI parses argv, calls the API, switches on the error contract's `code`, and renders. It performs no summary math, no filtering the API can do, and no direct storage access.
- One module owns HTTP calls (base URL from env, default per the localhost decision) and translates transport failures into the "server unreachable" user error with exit 1.
- Output formatting (including money via core's formatter) is separated from command wiring so it can be tested as pure functions.

## Why

The MVP's client-side summary drifted from the server's the week rounding was fixed in one place only. Rendering-only clients cannot drift.
