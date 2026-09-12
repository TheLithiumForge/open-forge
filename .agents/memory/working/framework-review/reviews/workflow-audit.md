---
open-forge:
  description: Recorded Workflow dispositions and package checks before later source and CLI changes
  tags: [Memory, Working, Contextual, Framework, Review]
---

# Workflow audit and extension delivery

This is an earlier review snapshot retained for the active Task 28 Git review. Its status and paths describe the recorded stage. Use [Task 28](../../cli-development/tasks/source-framework-review.md) for current decisions, completion, and remaining work.

Completed 2026-09-11 in `codex/task28-framework-review`. Changes remain uncommitted and unmerged. This records the result of the user's sequential step 4, not a claim that the CLI distribution snapshot has been updated.

## Decisions applied

- E01: refine the existing portable Review recipe using the recorded dogfood evidence. Add target identity, stable material findings, dispositions, and targeted correction checks. No second review workflow or fixed review rounds.
- E02: extend Planning with one Work Records Pattern, also available in dogfood. Task outcome, plan, current state, and backlog have predictable relationships. No new Directive or mandatory set of files.
- E03: retain Council locally. The lean-batching observation records six advisors and two council rounds, but does not isolate an argument that changed the accepted decision. That supports further observation, not a claim of demonstrated decision improvement. Council now asks for that evidence explicitly. No council package was created.
- E04: add an optional workflows-only Orchestration package depending on Toolkit. Project coordination, task execution, and conditional integration are responsibilities that can be performed by one person or agent. Sequential use is complete; parallel use requires authority and actual isolation. No APM agents, model roster, fixed hierarchy, or runtime permission claims were added.
- Retain local assured, batch, and experimental profiles within their accepted scope. Their budgets and specialized roles do not become portable defaults.

The initial inventory covers 28 current workflow files. Two new copies of Managed Delivery bring the checked total to 30. Every pre-existing workflow is accounted for below. Retention means the existing boundary was inspected and no material correction was identified, not that every recipe was rewritten.

## Per-file disposition

| File | Disposition | Result or rationale |
| --- | --- | --- |
| `.agents/workflows/_workflows.md` | Revised | Retain the compact category contract; refresh visible entries for changed or new recipes. |
| `.agents/workflows/adaptive-development.md` | Revised | Check delegation capability and leave blocking work incomplete when a correction budget is exhausted. |
| `.agents/workflows/architecture.md` | Revised | Return unresolved decisions and preserve proposed status. |
| `.agents/workflows/council.md` | Revised | Record the argument or evidence that changed a decision; invocation and agreement are insufficient. |
| `.agents/workflows/debugging.md` | Revised | Permit an honest inconclusive investigation without claiming a verified cause. |
| `.agents/workflows/development/_development.md` | Revised | Preserve complete assurance coverage while allowing existing behavior to have passing baseline evidence. |
| `.agents/workflows/development/phase-0-preflight.md` | Retained | Retain: selection, read-only boundary, blueprint, and completion are explicit. |
| `.agents/workflows/development/phase-1-contract.md` | Retained | Retain: separate contract freeze is conditional and missing behavior fails explicitly. |
| `.agents/workflows/development/phase-2-red.md` | Revised | Require intended failures for missing behavior and explicit passing evidence for preserved behavior. |
| `.agents/workflows/development/phase-3-green.md` | Retained | Retain: protected tests and contract-defect return prevent silent expectation changes. |
| `.agents/workflows/development/phase-4-blue.md` | Retained | Retain: one named structural trigger, explicit skip, and behavior-preserving verification. |
| `.agents/workflows/development/phase-5-purple.md` | Retained | Retain: one named evidence-structure trigger and return to Red for expectation changes. |
| `.agents/workflows/development/phase-6-review.md` | Revised | Revalidate findings before repair and record dispositions. |
| `.agents/workflows/development/task-acceptance.md` | Revised | Distinguish evidence validation from acceptance by applicable authority. |
| `.agents/workflows/development/task-lifecycle.md` | Revised | Reuse record relationships while retaining the local profile-specific fields. |
| `.agents/workflows/experimental-development.md` | Revised | Keep a compatibility forwarding recipe; reference migration stays inside authorized edits. |
| `.agents/workflows/planning.md` | Revised | Use one Work Records Pattern; update dependencies and resumption state without duplicate records. |
| `.agents/workflows/program-development.md` | Revised | Support sequential execution and preserve authority over required review and gates. |
| `.agents/workflows/review.md` | Revised | Identify the exact target and stable findings; revalidate authorized corrections and affected evidence. |
| `.agents/workflows/supervised-luna-preparation-trial.md` | Revised | Distinguish observed measurements from estimates and useful output from model/team advantage. |
| `.agents/workflows/vision.md` | Revised | Separate observed validation from candidate checks and acceptance. |
| `.agents/workflows/worktree-program-development.md` | Revised | Respect withheld integration and preserve unfinished work during recovery. |
| `src/extensions/development-toolkit/content/.agents/workflows/architecture.md` | Revised | Return unresolved decisions and preserve proposed status. |
| `src/extensions/development-toolkit/content/.agents/workflows/debugging.md` | Revised | Permit an honest inconclusive investigation without claiming a verified cause. |
| `src/extensions/development-toolkit/content/.agents/workflows/development.md` | Revised | Carry existing authorization forward and rerun only invalidated evidence. |
| `src/extensions/development-toolkit/content/.agents/workflows/planning.md` | Revised | Use one Work Records Pattern; update dependencies and resumption state without duplicate records. |
| `src/extensions/development-toolkit/content/.agents/workflows/review.md` | Revised | Identify the exact target and stable findings; revalidate authorized corrections and affected evidence. |
| `src/extensions/development-toolkit/content/.agents/workflows/vision.md` | Revised | Separate observed validation from candidate checks and acceptance. |

## Package and catalogue changes

Toolkit adds one Pattern and retains six Workflows, one native Skill, and nine leaf Templates. Orchestration adds one Workflow and declares Toolkit as its dependency. Their links resolve in the assembled dependency closure. The repository copy of Managed Delivery calls Adaptive Development because the repository's implementation profile differs from the portable Development recipe.

The two planning-only package directories made the native CLI reject the source catalogue. Their unchanged notes moved to `docs/extension-candidates/`. The catalogue now contains only manifested packages. Current setup examples use `--source src/extensions` because rebuilding the CLI does not regenerate its separate compressed catalogue snapshot.

The [catalogue synchronization task](../../cli-development/tasks/extension-catalogue-synchronization.md) records that implementation work for a separate task after review. The previously prepared scoped-continuity task remains separate. No CLI implementation or generated runtime agent definitions changed.

## Verification and limits

- Native CLI build from this worktree: passed, zero warnings and errors. This is a managed development build, not new Native AOT evidence.
- Source-catalogue preview and dependency installation: passed. Preview changed no workspace files.
- Authored package content matches installed content after excluding the CLI-maintained generated index regions. Recipe structure and links were checked separately.
- Repeated installation: complete and unchanged.
- Orchestration removal: correctly reports attention because Toolkit remains as an orphan dependency. An explicit subsequent Toolkit removal completed. Preview remained read-only.
- Doctor: incomplete both with the revised package and the original Toolkit from the review base. It reports native Skill files as missing routed metadata and Framework lifecycle differences after generated-index updates. These observations are preserved for CLI triage; they are not a clean diagnostic result.
- Static review: all 30 workflow files satisfy the applicable recipe shape. Changed source links resolve, including cross-package links after assembly. Shared changed Core prefixes, changed leaf Templates, the new Pattern, and added route labels agree.
- One unchanged pre-existing Template difference remains: dogfood Observation contains repository-specific evidence sanitation and conclusion fields absent from the package. Both sides are byte-identical to their baseline versions. Do not claim complete nine-template parity.

See `content-verification.json`, `package-verification.json`, `baseline-package-verification.json`, the individual command JSON results, and the P08 diffs and receipts. The new portable orchestration method still needs use outside this repository before claiming effectiveness or efficiency gains.
