---
open-forge:
  description: Accepted ledger removal contract requiring a remove command with hard-delete semantics
  tags: [Memory, Document, Product, CurrentTruth, Ledger, Removal]
---

# Ledger Removal Contract

The accepted next behavior is `ledger remove <id>`. It permanently deletes a mistaken transaction so subsequent list and summary results behave as if the transaction never existed. The server exposes `DELETE /transactions/{id}` for this behavior.

The delivery plan must cover the core behavior, server route, CLI command and help, persistence, errors, tests, and documentation.
