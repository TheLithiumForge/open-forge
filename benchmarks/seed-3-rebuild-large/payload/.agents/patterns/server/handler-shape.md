---
open-forge:
  description: One handler per route, wired through a small hand-rolled router
  tags: [Extension, Pattern, Server, Http]
---

# Handler Shape

## Shape

- A small router maps `(method, path pattern)` to handler functions; one handler module per resource.
- Handlers parse and validate input at the top (via core validation), call core logic, and return `(status, body)` data — they do not write to the response object directly; one adapter does the actual `node:http` response writing.
- Body parsing enforces content type and size limits before JSON.parse.

## Why

Handlers that return data instead of touching the socket are testable without a live server, which is where most of the server test coverage should sit.
