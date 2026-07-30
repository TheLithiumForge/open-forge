---
open-forge:
  description: The server binds 127.0.0.1 only; no auth, and that is only acceptable because of the bind
  tags: [Extension, Memory, Decision, Security, CurrentTruth]
---

# Localhost Only

## Decision

The server binds `127.0.0.1` exclusively (port from env, default 4317). There is no authentication, and that is acceptable *only* because the bind guarantees local-machine access. Binding `0.0.0.0` is forbidden without adding auth first — the two decisions are one decision.

## Rationale

Single-user local tool; auth would be ceremony. But an unauthenticated service on a network interface is an incident, not a tool.

## Consequences

- The bind address is not configurable; only the port is.
- Request bodies are still validated strictly (localhost is not a trust boundary against buggy clients).
- Path handling must reject traversal in any file-touching endpoint (`/import` receives content in the body, not a server-side path, partly for this reason).
