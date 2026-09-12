---
open-forge:
  description: Original proposal assigning each source its own role and scope
  tags: [Memory, Working, Contextual, Framework, Review]
---

# P04 — Authority by role and scope

This is an earlier review snapshot retained for the active Task 28 Git review. Its status and paths describe the recorded stage. Use [Task 28](../../cli-development/tasks/source-framework-review.md) for current decisions, completion, and remaining work.

Status: applied on 2026-09-11 within the user's accepted S03 direction. Changes remain uncommitted in the review worktree.

The [eight-file diff](../evidence/p04-scoped-authority.diff) narrows replacement of Open Forge defaults to corresponding content for the same role and accepted scope. It clarifies the knowledge authority of accepted Memory records, preserves their distinction from other categories, and assigns each durable outcome to the source defining its part.

The correction changes the shipped and dogfood loaders and Memory entrypoints, the Memory model, the acceptance document, and corresponding payload maintenance contracts. Memory lifecycle rules remain intact. Current documents and Decisions retain their separate questions and supporting relationship. Clear user direction remains accepted within its scope while durable representations are updated. Existing conflict handling, inherited binding instructions, delegated decisions, and external fact authority remain in force.

Verification: inspected the final diff, checked shipped/dogfood authored-rule parity, preserved generated navigation and metadata, checked the affected model link, and ran `git diff --check`. [Source identities](../evidence/p04-scoped-authority-receipt.json) record the applied files. No CLI code changed and no runtime tests were needed for this prose correction. The main checkout, frozen reports, and deferred diagrams remain unchanged.
