---
open-forge:
  description: Recorded max-reasoning reassessment and the five corrections applied afterward
  tags: [Memory, Working, Contextual, Framework, Review]
---

# Task 28 reassessment at max reasoning

This is an earlier review snapshot retained for the active Task 28 Git review. Its status and paths describe the recorded stage. Use [Task 28](../../cli-development/tasks/source-framework-review.md) for current decisions, completion, and remaining work.

Prepared after the user's request to reconsider the previous changes and intentions, then discuss corrections one at a time. This report supersedes the earlier claim of semantic completeness. It does not apply another source patch.

## Working Boundary

Keep the existing review worktree and its uncommitted changes. Do not merge to `develop`. The user now explicitly requires Astra at max reasoning with the complete accepted context for source changes. Dedicated writers or reviewers are permitted when useful, but they do not acquire permission to change CLI implementation. CLI work belongs to another chat; this review may prepare later tasks after source meaning settles.

A separate Astra/max reviewer read all 23 `src/open-forge` files and every changed or new Extension file against the baseline, accepted notes, and user intent. The primary reviewed the accepted answers, companion framework and maintenance sources, local workflow integration, and the prior reports. This is a semantic reassessment, not another inventory of all 703 Markdown paragraphs. No CLI inspection, build, test, or source mutation was performed in this reassessment. All paths in the previous delivery receipt retained their recorded identities before this report was written.

## Intent That Remains Accepted

- A source answers one question within its role and scope. Memory preserves evidence, accepted knowledge, decisions, and reasoning without acquiring another category's role by containing its words.
- Validation, acceptance, classification, and integration are distinct judgments. Useful outcomes pass through those judgments as needed; no fixed sequence of files or repeated approval for settled direction is required.
- Required behavior, recommended approaches, inspectable shapes, reusable procedures, and starting content retain their distinct roles. Existing authority may permit an exception; a reason alone does not authorize an unapproved departure from an agreed requirement or shape.
- Archival extracts still-current meaning and keeps useful remaining history under the archive's rules. It may be lossy and transformative. It does not preserve a former category's active behavior through metadata or naming.
- Overwrites belong to their base source's role and scope. Scope and loading do not create an unrelated authority rank.
- The plain Markdown framework is complete without the CLI. Keep the loader compact; installation and update are user lifecycle concerns. Diagrams remain deferred.
- Optional planning and orchestration should supply useful predictable structure without importing every local role, budget, record, or model choice. Council value must be demonstrated through useful decision contributions rather than invocation or agreement counts.

## Conclusions Retained

The parent-driven continuity correction, validation/acceptance distinction, Memory knowledge authority, overwrite interpretation, and transformative archival direction remain sound. Building on Planning with one Work Records Pattern is proportionate. A workflows-only Orchestration package is a valid accepted delivery form, and the separate responsibility scopes remain useful even when one person or agent performs them. The Review refinement and the decision to retain Council locally remain supported.

The new conclusions concern incomplete integration and overgeneralization, not a reason to discard those decisions or redesign the framework.

## Reassessment Findings

### T28-RS1: Agreed Pattern departures need the accepted exception boundary

Source: `src/open-forge/.agents/patterns/_patterns.md:24–25`, its dogfood counterpart, and the continuing-reference explanation in `.agents/memory/crystallized/documents/framework/primitives/patterns.md:22`.

The current rules permit another shape for a deliberate reason and require reporting why. They do not explicitly require obtaining a needed exception before relying on an unapproved departure from an agreed shape.

Example: the user agrees on a design-record Pattern containing a distinct failure-behavior section. An agent folds that section into general prose to save space, continues dependent work, and explains the change at closeout. A deliberate reason and late explanation are insufficient under the user's accepted intent.

This was independently identified in both reassessment contexts. It is an unfinished S03 integration, not a regression introduced by P04.

Proposed replacement for the two application rules, for discussion:

> Treat an applicable Pattern as the default shape for its scope. When a different shape is needed, explain why before work depends on it.
>
> Use existing authority for an exception to an agreed shape. If that authority is missing, ask the user before proceeding.

Reconcile the existing Pattern explanation and maintenance contract with this boundary. Do not expand the loader, turn all advice into binding requirements, or ask again for an exception already covered by user direction or delegated authority.

### T28-RS2: The archive introduction excludes never-accepted material

Source: `src/open-forge/.agents/memory/archived/_archived.md:9` and the matching dogfood opening.

It still defines Archived Memory as history after it stops being `CurrentTruth`. A rejected idea or completed contextual transfer may never have been CurrentTruth, yet useful remnants belong in the archive under both the accepted intent and the existing narrower routes.

Proposed opening:

> Archived Memory keeps useful history that no longer controls current work.

Keep the new extraction, metadata, transformation, retention, and acceptance rules. This is an unfinished S05 integration, not a reason to reopen the agreed archival model.

### T28-RS3: A council-specific value test was generalized to all collaboration

Source: newly added `.agents/memory/crystallized/documents/framework/primitives/workflows.md:70`.

The instruction to evaluate optional collaboration by a changed consequential decision or a discovered material problem fits Council and some reviews. It does not cover useful delegated implementation, writing, coordination, or recovery that completes an already-settled task without changing a decision or finding a defect.

Keep the general Workflow value criterion tied to its stated goal. Keep the specific council evidence condition in its method or candidate assessment. This also restores the user's requirement that each source contain its own slice of truth.

### T28-RS4: Local worktree delivery does not express the withheld-integration branch completely

Source: `.agents/workflows/worktree-program-development.md:26–40`.

The new text recognizes withheld integration, but is inserted ahead of instructions that still say to combine commits, reach a passing integrated state, and converge. Completion still requires integrated success. The route also requires clean committed task output earlier in the recipe, so an explicitly uncommitted handoff needs a deliberate branch rather than a general claim that the workflow already supports it.

Represent delivery and a prepared handoff as distinct outcomes. Gate the integration-only steps and completion conditions together. Preserve the local workflow's accepted isolation and review requirements. Do not imply that a blocked or withheld integration satisfies the integrated-delivery goal.

### T28-RS5: Current Extension explanation and historical lifecycle remain mixed

Source: `.agents/memory/crystallized/documents/extensions/architecture.md:3–35` and the public Extension documentation discussed in L1.

The added historical warning coexists with current-authority metadata and a status section claiming the present lifecycle, followed by both old lifecycle detail and the new catalogue. A warning alone does not make the document answer one coherent current question.

Separate the durable Framework/package concept from historical implementation detail and link to the actual lifecycle authority. Any CLI-dependent correction belongs in the other chat's task. Do not use this finding to reopen or implement the CLI here.

## Revised Order

1. Discuss and apply T28-RS1's Pattern exception boundary.
2. Complete T28-RS2's general archive definition.
3. Reconcile the workflow responsibility, branching, and completion issues in T28-RS3–RS4.
4. Reconcile current Framework/package explanation with its proper source responsibilities; hand CLI-specific work off as tasks.
5. Resume the language recommendations over the coherent result.

The prior L2–L4 recommendations remain useful. L1 also remains useful, but command-sensitive lifecycle detail must respect the newly reiterated CLI boundary. Splitting long sentences must follow the necessary meaning corrections rather than conceal them.

## Status Correction

The previous patches are applied and reviewable. Steps 1–4 are not yet semantically complete. Structural and source-package checks remain evidence for the properties they actually checked, not proof that every accepted intention was fully integrated. The earlier receipts preserve their point-in-time results and are not rewritten as if this reassessment had already happened.
