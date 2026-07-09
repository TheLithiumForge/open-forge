---
open-forge:
  description: Post-MVP rebuild context for the bookmarks CLI seed
  tags: [Extension, Memory, Document, Rebuild, Product, CurrentTruth]
---

# Rebuild Brief

This seed represents a clean restart after an earlier MVP, not a blank greenfield idea.

## What We Learned From The MVP

- The command set was right: add, list, remove, search, and help are enough for the useful core.
- The first build was easy to understand when logic stayed in small pure functions.
- The weakest parts were underspecified edge semantics: duplicate URLs, tag normalization, corrupt JSON, and write safety.
- A README that only lists commands is not enough; users and reviewers need behavior notes.
- Dependency-free runtime code was easier to review and maintain than a package-heavy CLI.
- Test value came from combining pure unit tests with a real CLI smoke path.

## Rebuild Goal

Rebuild the same small product with clearer contracts, not a larger product.

The next implementation should preserve the simplicity of the MVP while tightening the behaviors that were previously ambiguous.

## Non-Goals

- Do not add sync, auth, browser integration, a web UI, or import/export just because the architecture could support it.
- Do not make a plugin framework for a one-tool CLI.
- Do not optimize for hypothetical enterprise use.
