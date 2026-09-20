---
open-forge:
  description: Evidence and limits behind the proposed review method, without claiming measured effectiveness
  tags: [Memory, Analysis, Contextual, Candidate, Framework, Review]
---

# E01 — Dogfood evidence for a bounded review-workflow trial

This assessment preserves the evidence available during the original comparison. The [retained draft](../../ideas/framework-review/review-workflow-draft.md) remains available for refinement. Later implementation and deferral are recorded in [Task 28](../../../archived/cli-development/tasks/source-framework-review.md).

Recorded 2026-09-10. The user is willing to try developing a review workflow if dogfood evidence supports it. The inspected records support a bounded refinement of the existing toolkit workflow. They do not establish that a new package, mandatory review process, or coordinated agent system improves outcomes.

## Observed results recorded in dogfood

| Evidence                                                                                        | Recorded result                                                                                                                                                                                                                                                                                                                              | Supported implication                                                                                                                                                                                          |
| ----------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `.agents/memory/emerging/observations/2026-08-15_lean-agent-batching.md:85–106`                 | An eight-file contract pack used one writer and one end reviewer. The review reported no actionable findings, no post-review correction was needed, and the listed deterministic checks passed.                                                                                                                                              | A useful review may end with no findings. A coherent review target does not require one review per file.                                                                                                       |
| Same observation, lines 108–145 and 179–211                                                     | A discovery pack's end review identified stale evidence; a later lifecycle pack's review identified two high findings and one medium finding, concerning inconsistent identity inference, missing dry-run event boundaries, and stale contextual wording. The record describes focused corrections.                                          | Review can expose semantic and evidence problems across related files. Inspect the actual target and reported evidence, then correct the affected responsibilities.                                            |
| `.agents/memory/working/cli-development/tasks/cli-command-surface-audit-report.md:9–54,56–101`  | A bounded 28-command audit retained seven validated IDs: three behavior defects, two evidence-allocation findings, and two structural improvements. One concrete example traced overwrite pairing that existing tests asserted incorrectly. The audit explicitly identifies its evidence as static source/test tracing, not fresh execution. | Passing tests do not establish agreement with accepted behavior. Classify findings and retain their source, consequence, and identity. Do not generalize this workspace's specific test-allocation preference. |
| `.agents/memory/working/cli-development/tasks/cli-command-surface-remediation.md:46–97,135–161` | Finding `T21-R1-1` tracked assertion-preservation and evidence gaps. A focused recheck found a remaining serialized-value assertion and inaccurate evidence mappings. A second bounded correction and recheck closed the same finding without reopening the full review scope.                                                               | Stable identity, current-target revalidation, and targeted rechecks have a concrete local use. A fixed rule allowing only one correction would not fit this occurrence.                                        |

These are recorded dogfood observations and task dispositions. This follow-up inspected their content and source identities; it did not replay the historical reviews, rebuild the historical candidates, inspect every underlying execution artifact, or independently reproduce the defects.

The batching records concern different tasks with different scopes and user involvement. They are not a controlled comparison. Exact token data is unavailable. The later agent/workflow audit also states that comparative quality, latency, and reviewer economics remain unestablished (`.agents/memory/emerging/observations/2026-09-02_agent-and-workflow-change-audit.md:239–258`). No percentage saving, defect-detection rate, or general performance guarantee is justified.

## Existing package and smallest supported change

The current `development-toolkit` package already contains `content/.agents/workflows/review.md`. It already covers contextual inspection, reconciliation of durable knowledge, high-risk paths, suitable verification, evidence-backed findings, false-positive checks, read-only state accounting, uncertainty, and separately authorized fixes. Its manifest declares no dependencies.

The proposed trial changes only three concerns in that existing recipe:

1. Identify the baseline, included changes or untracked material, and claimed verification explicitly.
2. Give material findings stable IDs while preserving evidence and correction guidance for every finding.
3. When authorized corrections follow, track dispositions, revalidate findings against the changed target, and recheck affected context. Repeat the full review only when the changes materially alter its scope.

See [the complete trial draft](../../ideas/framework-review/review-workflow-draft.md) and [the proposed diff](../../ideas/framework-review/review-workflow-proposal.diff). They are review artifacts, not installed or package-source changes. The draft preserves the existing recipe and adds no agent-role dependency, fixed model, test-tier quota, mandatory budget ledger, new root rule, or separate Pattern. Broader editorial integration remains subject to the accepted source-review directions.

## How to assess the trial later

Use the draft on a bounded, authorized change with an explicit baseline and accepted outcome. Preserve material findings and their dispositions, any missed or incorrectly reported issue discovered during correction, recheck scope, and unnecessary repeated review work. Record time or token cost only when actually available. Accept a no-findings result when supported; do not manufacture defects to demonstrate value.

Keep, adjust, or discard the additions based on whether they improve traceability and correction without imposing disproportionate bookkeeping. One favorable run would support continued experimentation, not prove universal benefit. No trial review or independent agent invocation was run in this follow-up.

[e01-evidence-identities.json](../../../archived/framework-review/evidence/e01-evidence-identities.json) records the inspected source hashes, review extents, repository head at capture, and draft identity. The earlier frozen reports remain unchanged; this follow-up adds evidence obtained after their freeze.
