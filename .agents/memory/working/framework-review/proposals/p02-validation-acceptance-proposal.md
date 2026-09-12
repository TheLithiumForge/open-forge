---
open-forge:
  description: Original proposal separating evidence validation from scoped acceptance
  tags: [Memory, Working, Contextual, Framework, Review]
---

# P02 — Validation and acceptance wording

This is an earlier review snapshot retained for the active Task 28 Git review. Its status and paths describe the recorded stage. Use [Task 28](../../cli-development/tasks/source-framework-review.md) for current decisions, completion, and remaining work.

Status: approved and applied on 2026-09-11 in the review worktree. The user approved adding the [patch](../evidence/p02-validation-acceptance-proposal.diff) and continuing to the next correction, with Git review afterward. The changes remain uncommitted. Both diagram ideas remain deferred. The [application receipt](../evidence/p02-application-receipt.json) verifies the exact approved candidate contents and preservation of the main checkout.

## Problem and proposed result

The current Contextual definition lists restoration, validation, acceptance, and promotion as alternatives that can establish current state. Emerging Memory similarly lists validation and promotion as alternatives to acceptance. This can turn a supported claim or a moved record into an accepted decision without an applicable source of acceptance.

Place the user's accepted distinction in the loader's authority rules:

> Validation establishes whether evidence supports a claim. Acceptance establishes which knowledge or decisions may be treated as current within their scope. Restoring or moving a record does not establish acceptance by itself.

Replace the Contextual definition with:

> Useful context. Treat it as unaccepted unless applicable authority establishes acceptance within its scope.

Replace the Emerging Memory status rule with:

> Treat Emerging Memory as contextual until accepted within its scope.

The existing acceptance sources remain unchanged: clear user direction, delegated authority, choices necessarily required by authorized action, and declared external authority. Accepted temporary choices can still be recorded with their scope and expected expiration. This adds no requirement to ask the user to approve every verified fact.

## Placement

The patch changes eight existing files for one semantic correction:

| Sources | Proposed change |
| --- | --- |
| Shipped loader and Emerging entrypoint | Apply the three wording changes above. |
| Corresponding repository dogfood files | Keep their authored rules aligned with the shipped source. Generated entries remain untouched. |
| Framework acceptance and Emerging state documents | Explain the distinction and preserve scoped acceptance in their current descriptions. |
| Loader and Emerging payload maintenance contracts | Record the corresponding maintenance requirement and link to its defining explanation. |

The loader and acceptance document both state the accepted distinction because each serves an independent reader: installed workspaces receive the loader, while repository maintainers use the current Framework explanation. The maintenance contract links to that explanation. Other acceptance and archival passages inspected do not identify validation as sufficient acceptance, so they do not need a matching wording change for S02.

No new shipped files, schema, CLI behavior, loading changes, diagram work, or archival policy changes are included. The two shipped files grow by a net 30 whitespace-separated words. This is not a tokenizer measurement; broader editorial consolidation remains part of S08.

## Verification

- Inspected the complete proposed diff for preservation of accepted scope and authority.
- `git apply --check` succeeds against the review worktree without applying the patch.
- All eight target files match the main checkout at proposal creation and remain unchanged.
- Recorded each source and candidate SHA-256 in the [proposal receipt](../evidence/p02-validation-acceptance-identities.json).
- The new maintenance link resolves to the Framework acceptance document and its Acceptance heading.

After approval, the patch was applied only in the review worktree. All eight file hashes match the proposed candidate, and `git diff --check` passes. The corresponding main-checkout files were unchanged by application. No tests were added or run for this prose-only correction. The prohibition on merging to `develop` remains in force.
