---
open-forge:
  description: All error paths flow through one response helper implementing the error contract
  tags: [Extension, Pattern, Server, Error, Contract]
---

# Error Responses

## Shape

One helper owns the api-error-contract shape; every error path — validation, not-found, import rejection, and the top-level catch-all — goes through it. The catch-all wraps the entire request handling so an unexpected exception still emits `{"error":{"code":"internal",...}}` with status 500 and logs the real error server-side only.

## Why

The contract survives only if there is exactly one place that can emit an error body. The MVP had four, and they disagreed within a month.
