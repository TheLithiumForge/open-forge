---
open-forge:
  description: Recorded source identities, audit coverage, and verification supporting the Task 28 Git review
  tags: [Memory, Working, Contextual, Framework, Review]
---

# Framework Review Evidence

These records support the active [Task 28 Git review](../../cli-development/tasks/source-framework-review.md). Each JSON file is an unchanged evidence snapshot from its recorded stage. A passing check establishes only the property and source revision it inspected. Earlier completion claims do not close the deferred planning starters or establish final CLI behavior.

## Current Review Summary

- Accepted source and policy work is recorded in commits `0eea5e65`, `2b31f4e4`, `e91d2934`, `28cac0fc`, and `40e55138`.
- Commits `7a68a3b9` and `ab01ec4b` record Project Voice and the completed public documentation pass.
- The public source language pass read 51 files. The current non-CLI audit assessed 182 files within its recorded exclusions.
- The later category/tag review inspected all 21 public category openings and 44 Markdown/native Skill files. All 29 generated links matched target metadata. No source tag changes were applied.
- The base Framework contains 23 Markdown files. tiktoken 0.14.0 counted 8,267 `o200k_base` or 8,309 `cl100k_base` tokens. Default startup includes 19 files and counts 6,823 or 6,863 tokens respectively. These raw-file sums exclude project context, Extensions, and harness overhead.
- The final public prose pack passed writing review. The primary owner checked the subsequent user-directed voice refinement. Static checks covered 140 relative links and 12 README category questions. The measured source hashes remained unchanged.
- The original source-only review disclosed prior exposure to the local loader. Its source evidence remains useful, but perfect context isolation was not established.
- Earlier package installation checks predate the stricter CLI boundary and do not qualify the final integrated CLI. Executable verification belongs to the separate CLI task.

## Recorded Evidence

- [baseline-package-verification.json](baseline-package-verification.json)
- [category-metadata-ace-review-target.json](category-metadata-ace-review-target.json)
- [category-metadata-ace-review.json](category-metadata-ace-review.json)
- [content-verification.json](content-verification.json)
- [context-token-counts-before-nitpicks.json](context-token-counts-before-nitpicks.json)
- [context-token-counts.json](context-token-counts.json)
- [current-cli-development-facts.json](current-cli-development-facts.json)
- [current-cli-doc-facts.json](current-cli-doc-facts.json)
- [current-content-cleanup.json](current-content-cleanup.json)
- [current-entrypoint-static-audit.json](current-entrypoint-static-audit.json)
- [current-entrypoints-audit.json](current-entrypoints-audit.json)
- [current-knowledge-audit.json](current-knowledge-audit.json)
- [current-operations-audit.json](current-operations-audit.json)
- [e01-evidence-identities.json](e01-evidence-identities.json)
- [integration-preparation.json](integration-preparation.json)
- [local-agent-metadata.json](local-agent-metadata.json)
- [local-reviewed-files.json](local-reviewed-files.json)
- [local-workflow-overwrite-review.json](local-workflow-overwrite-review.json)
- [nitpick-content-verification.json](nitpick-content-verification.json)
- [nitpick-final-invariants.json](nitpick-final-invariants.json)
- [observation-template-alignment.json](observation-template-alignment.json)
- [p02-application-receipt.json](p02-application-receipt.json)
- [p03-application-receipt.json](p03-application-receipt.json)
- [p04-scoped-authority-receipt.json](p04-scoped-authority-receipt.json)
- [p05-archival-receipt.json](p05-archival-receipt.json)
- [p06-overwrites-receipt.json](p06-overwrites-receipt.json)
- [p07-readme-orientation-receipt.json](p07-readme-orientation-receipt.json)
- [p08-catalogue-corrections-receipt.json](p08-catalogue-corrections-receipt.json)
- [p08-workflow-docs-receipt.json](p08-workflow-docs-receipt.json)
- [p08-workflows-receipt.json](p08-workflows-receipt.json)
- [package-verification.json](package-verification.json)
- [public-language-pass.json](public-language-pass.json)
- [public-language-review-target.json](public-language-review-target.json)
- [public-language-review.json](public-language-review.json)
- [public-voice-final-refinement-review.json](public-voice-final-refinement-review.json)
- [public-voice-review.json](public-voice-review.json)
- [public-voice-verification.json](public-voice-verification.json)
- [source-clarifications-review.json](source-clarifications-review.json)
- [source-freeze.json](source-freeze.json)
- [source-inventory.json](source-inventory.json)
- [source-static-checks.json](source-static-checks.json)
- [source-tag-audit.json](source-tag-audit.json)
- [t28-rs1-pattern-exception-application.json](t28-rs1-pattern-exception-application.json)
- [t28-rs2-archive-definition-application.json](t28-rs2-archive-definition-application.json)
- [t28-rs3-workflow-value-application.json](t28-rs3-workflow-value-application.json)
- [t28-rs4-worktree-delivery-application.json](t28-rs4-worktree-delivery-application.json)
- [t28-rs5-extension-source-split-application.json](t28-rs5-extension-source-split-application.json)
- [worktree-receipt.json](worktree-receipt.json)

## Original Proposal Support

These exact proposal patches and identity records are retained for the active Git review. They preserve the original candidate evidence and remain distinct from the later application receipts. BLR-01 repaired the links to these files; it did not reconstruct proposals from the applied result.

- [p02-validation-acceptance-proposal.diff](p02-validation-acceptance-proposal.diff)
- [p02-validation-acceptance-identities.json](p02-validation-acceptance-identities.json)
- [p03-scoped-loading-proposal.diff](p03-scoped-loading-proposal.diff)
- [p03-scoped-loading-identities.json](p03-scoped-loading-identities.json)
- [p04-scoped-authority.diff](p04-scoped-authority.diff)
- [t28-rs5-extension-source-split-proposal.diff](t28-rs5-extension-source-split-proposal.diff)
- [t28-rs5-extension-source-split-proposal.json](t28-rs5-extension-source-split-proposal.json)

The [relocation manifest](memory-relocation.json) records original and relocated identities. Generated logs, bulk discovery inventories, command output, candidate checkouts, and temporary scripts remain outside Memory. The useful authored reports and their current review evidence are tracked here.


## Memory Placement Correction

The user's correction moved 21 authored Markdown records and 51 supporting files from the review output directory into routed Memory. Working records retain the task discussion and Git review evidence. Emerging records retain the deferred diagrams, review draft, and its supporting analysis. The relocation manifest preserves original identities; evidence files and draft support were moved byte-for-byte. Markdown changes add classification, distinguish earlier snapshots from current task state, and repair navigable references.

The placement check inspected 31 Markdown files and 217 relative links against the local filesystem. It did not establish that every linked destination was tracked; the later BLR-01 finding identified eight references that would fail after checkout. Each new scope has an entrypoint, and its entries match destination descriptions and tags. The source payload, CLI implementation, and `develop` were unchanged.

The [BLR-01 follow-up](../reviews/branch-logic-review-followup.md) records the targeted correction and Git-backed link checks. The original relocation manifest remains a snapshot of the earlier move.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->

<!-- open-forge:generated-index:end -->
