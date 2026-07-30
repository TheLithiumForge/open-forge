---
open-forge:
  description: Every API error is one JSON shape with a stable machine-readable code
  tags: [Extension, Memory, Decision, Api, Contract, CurrentTruth]
---

# API Error Contract

## Decision

Every non-2xx response has the body `{"error": {"code": "<stable-code>", "message": "<human text>"}}` and nothing else. Codes are lowercase kebab, stable across versions, and enumerated in the server routes: `validation-failed`, `not-found`, `invalid-json`, `import-rejected`, `internal`.

Status mapping: 400 `validation-failed`/`invalid-json`, 404 `not-found`, 422 `import-rejected`, 500 `internal`.

## Rationale

The MVP returned strings, HTML, and three different JSON shapes depending on the code path; the CLI grew special cases for each. One shape, one parser, one switch on `code`.

## Consequences

- The CLI switches on `code`, never parses `message` text.
- Validation failures list field-level details under `error.details` (array of `{field, problem}`) — the one permitted extension.
- Unexpected server exceptions must still produce the contract shape (500 `internal`), never a stack trace body.
