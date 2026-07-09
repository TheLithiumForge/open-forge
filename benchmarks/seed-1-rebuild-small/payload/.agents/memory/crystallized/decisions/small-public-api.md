---
open-forge:
  description: Keep the internal module API small and boring
  tags: [Extension, Memory, Decision, Architecture, API, CurrentTruth]
---

# Small Public API

## Decision

The implementation should expose only the minimum internal module surface needed by the CLI and tests.

## Rationale

The earlier MVP showed that a neat file split is useful, but a library-like API is premature. This project is a CLI first. The internal modules should make behavior testable without pretending to be a published SDK.

## Consequence

Prefer direct modules such as core behavior, storage, command parsing, and executable entry point. Avoid generic abstractions like repositories, service containers, plugins, or adapters unless the code has already earned them.
