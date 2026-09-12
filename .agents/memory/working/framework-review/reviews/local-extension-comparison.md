---
open-forge:
  description: Original local capability comparison and Extension shortlist before later user decisions
  tags: [Memory, Working, Contextual, Framework, Review]
---

# Task 28 — Local practices and optional extension candidates

This is an earlier review snapshot retained for the active Task 28 Git review. Its status and paths describe the recorded stage. Use [Task 28](../../cli-development/tasks/source-framework-review.md) for current decisions, completion, and remaining work.

Status: recommendations for individual user review. No candidate is approved, packaged, installed, committed, or merged.

Recommend three focused capability improvements for consideration within the existing development toolkit: evidence-focused review, compact task/plan templates, and bounded independent deliberation. Keep managed worktree orchestration as a later pilot. Preserve Experience Design as an existing capability rather than presenting it as a new discovery.

## Boundary and evidence

This comparison began only after [source-only-report.md](source-only-report.md) and its source identities were frozen in [source-freeze.json](../evidence/source-freeze.json). Local evidence does not amend or supply intent for the Stage 1 findings.

The user subsequently required individual approval of proposed changes, prohibited merging to `develop`, and requested a worktree. Reports were moved without content changes into `/home/tedy/dev/open-forge-worktree/task28-framework-review`, branch `codex/task28-framework-review`, based on the reviewed commit `8a52ede13ab6d622578a6cfae04031a9a5b2eb2d`. [worktree-receipt.json](../evidence/worktree-receipt.json) records that limited Git authorization. No implementation or commit was made.

Local comparison evidence comes from the working `.agents/` and `.apm/` trees in `/home/tedy/dev/open-forge`. Those files were treated as comparison material, including their roles and Skill text, rather than instructions or capabilities to activate for this review. This matters because the local copy contains stronger orchestration policies and provider-specific machinery than the source framework.

The local path inventory contains **641 Markdown files**. Selection used root catalogs, relevant extension decisions, local workflows, focused directives and guidance, testing patterns, task/plan templates, Experience Design, and relevant agent roles. This was a targeted capability assessment, not a full audit of 641 files. [local-reviewed-files.json](../evidence/local-reviewed-files.json) gives content hashes and line counts for fully read files. [local-agent-metadata.json](../evidence/local-agent-metadata.json) distinguishes the first-nine-line scan of all 27 role files from full reads of selected roles.

The extension package source under `src/extensions/`, runtime adapters, CLI implementation, build output, project Task/control records, and archived implementation histories were not inspected. Consequently, this report identifies candidates and ownership requirements; it does not establish payload parity, adapter support, performance gains, or readiness to release.

## Existing ownership changes the recommendation

Local accepted records describe **one** first-party `development-toolkit` package containing six workflows, Experience Design with three references, and nine templates. They explicitly favor one small package over a large dependency catalog: `.agents/memory/crystallized/decisions/extensions/development-toolkit.md:17–43,59–65`.

The package boundary is whole-file installation through existing routes. Installed content must remain understandable without package metadata or the CLI: `.agents/memory/crystallized/decisions/extensions/extension-package-boundary.md:19–35,90–101`. The local architecture distinguishes historical MVP `payload/` mechanics from replacement `content/` mechanics at `.agents/memory/crystallized/documents/extensions/architecture.md:10–18`. No manifest or payload layout is proposed here.

Therefore, the ranked items below are **capability candidates**, not a recommendation to create four package IDs. For E01–E03, first compare the accepted toolkit payload with the proposed content and update that owner if approved. A separate package needs evidence of independent demand and a deliberate revision of the one-package catalog choice. E04 may eventually justify that decision, but does not establish it now.

## Ranked shortlist

| Rank / ID | Capability | Recommendation | Why this rank |
| --- | --- | --- | --- |
| 1 / T28-E01 | Evidence-focused review | Consider a lean toolkit improvement | Concrete review outputs and targeted rechecks are broadly reusable, with modest runtime dependence. |
| 2 / T28-E02 | Compact task and plan starters | Consider toolkit templates after removing local orchestration requirements | Helps resume nontrivial work while retaining existing task-system ownership. |
| 3 / T28-E03 | Bounded independent deliberation | Consider one optional council recipe | Useful for consequential uncertainty, but requires real context isolation and justified added cost. |
| 4 / T28-E04 | Managed worktree delivery | Defer to a bounded opt-in pilot | Useful ownership and integration method, but substantial runtime, permission, and maintenance prerequisites. |

### T28-E01 — Evidence-focused review

**Purpose and audience:** help software and document maintainers receive findings they can assess, correct, and recheck without repeated full reviews or inflated defect lists.

**Local evidence:** `.agents/workflows/review.md:15–23,27–32`; `.agents/directives/review-evidence.md:11–22`; `.apm/agents/reviewer.agent.md:55–66,70–88`; `.agents/patterns/testing/evidence-tiers.md:15–25,36–53`.

**Reusable behavior:** name the target and its baseline; give material findings stable IDs, exact evidence, consequences, confidence, and the smallest correction; challenge findings against safeguards; distinguish defects from preferences and missing evidence; revalidate findings after changes; recheck affected boundaries. For software, distinguish focused tests, integrated boundaries, delivered journeys, and packaged journeys by what each proves.

**Prerequisites:** an explicit review target and accepted outcome; access to the target and claimed evidence; a read-only review boundary. Git is useful for a code baseline but must not become mandatory for prose or an ordinary mutable-target review. Independent review additionally needs an isolated context; a second pass in the same context must be described accurately.

**Smallest coherent content and owned files:** revise the existing toolkit-owned `.agents/workflows/review.md`, after verifying its actual package owner and current bytes. Put one companion evidence Pattern under `.agents/patterns/testing/` only if it adds reusable test-specific judgment that would overburden the recipe. The package may own a new scoped entrypoint if needed; it must not take ownership of pre-existing root catalogs or a user's local file just because the path matches. Native reviewer-role installation is not needed for the first useful version.

**Boundaries and why optional:** activate for a requested or materially useful review. Do not install the local root `LoadNow` Review Evidence Directive unchanged; that would make a specialized method universal. Core already defines authority, context, and optional Workflow selection. The extension adds review output and correction mechanics, not a new acceptance authority or a requirement that all work use a reviewer.

**Representative draft:**

> Review the named target against its accepted outcome. For each material finding, give a stable ID, exact location, evidence, consequence, smallest correction, and any missing verification. Check existing safeguards before calling it a defect. After the target changes, revalidate the affected findings; repeat the full review only when the change materially expands the review scope.

**Do not carry over:** mandatory named review-budget units, the four-topic coordinator, candidate-parent/authority-tree equivalence rules, or automatic independent invocation. These belong to the separately selected coordinated-review method. The local reviewer role also hard-codes a provider/model and tool permissions; those are adapter policy, not portable review semantics.

**Decision for later individual approval:** accept the general review behavior and decide whether the test-evidence Pattern earns inclusion. Do not approve the entire local review/orchestration apparatus by approving this candidate.

**What would establish usefulness:** a bounded comparison showing fewer duplicate/preference findings or less repeated review effort while material defects and uncertainty remain visible. This review did not run that evaluation.

### T28-E02 — Compact task and plan starters

**Purpose and audience:** give maintainers of multi-session or dependency-bearing work a useful starting structure without requiring a new project-management system.

**Local evidence:** `.agents/templates/memory/task.md:9–10,27–57,59–102`; `.agents/templates/memory/plan.md:9–10,51–84,99–108`; `.agents/templates/memory/project-status.md:12–17,40–50`; `.agents/templates/memory/_memory.md:15–23`; `.agents/workflows/planning.md:15–22`.

**Reusable behavior:** one source defines what must be accomplished; a separate Plan exists only when sequencing or dependencies need it; distinguish expected change paths from protected boundaries; state observable completion, evidence, dependencies, stop conditions, and the next action; link durable authority rather than copying it.

**Prerequisites:** work whose resumption or sequencing needs exceed the current request or external task source. If an issue tracker already owns a field, the template links to it and records only the additional context needed here.

**Smallest coherent content and owned files:** two optional template leaves, `.agents/templates/memory/task.md` and `.agents/templates/memory/plan.md`, under the existing toolkit's template route if approved and not already owned. Match parent navigation during normal installation. Keep project-status and project-control-ledger templates out of the first increment: a Task or Checkpoint can answer ordinary continuation questions without another mutable view.

**Boundaries and why optional:** these are copy-ready starters. They create no new Memory state, mandatory task record, status schema, queue, or authority. Instantiated user records remain independent of template updates and package lifecycle. Core continues to own Memory acceptance, continuity, and archive semantics.

**Representative minimum draft:**

> Use this record only when the current request or external task cannot preserve the work sufficiently. Record the outcome, accepted scope, owner, relevant sources, completion evidence, current state, and next action. Add a linked Plan only when dependencies or sequencing need more detail. Remove unused sections; do not copy state already owned elsewhere.

**Do not carry over:** permanent numeric task IDs, exact phase/milestone notation, completion-grace counters, required agent/model displays, mandatory budgets, or a project ledger dependency. The current local templates include those features at `task.md:16–25,98–104`, `project-status.md:22–38`, and `project-control-ledger.md:47–79`; they require adaptation before distribution as general starters.

**Decision for later individual approval:** whether these two starters add enough value beyond the existing toolkit's nine templates. Their smaller proposed forms should be reviewed independently; approving “templates” does not approve every local field.

**What would establish usefulness:** another owner can resume a representative task and identify the next action and completion evidence without reconstructing the conversation; the record does not duplicate its external tracker or require continual bookkeeping.

### T28-E03 — Bounded independent deliberation

**Purpose and audience:** help a decision-maker test one consequential uncertainty with materially different perspectives while retaining responsibility for the decision.

**Local evidence:** `.agents/workflows/council.md:15–23,27–31`; `.apm/agents/advisor.agent.md:55–67,71–87`; `.agents/directives/decision-authority.md:11–15`.

**Reusable behavior:** resolve cheap factual questions first; select a small number of distinct lenses; keep first positions independent; separate evidence, assumptions, preferences, counterarguments, and change conditions; synthesize reasons and dissent instead of voting; stop when further discussion is unlikely to change the decision.

**Prerequisites:** one consequential decision, defined criteria, a decision-maker, and authorization for any extra agent expense. Actual independent contexts are required to claim independent positions. If unavailable, offer a clearly labeled structured second pass rather than simulating independent agents.

**Smallest coherent content and owned files:** one `.agents/workflows/council.md` leaf containing a compact advisor assignment/return shape. The primary runtime can invoke generic bounded agents when supported. A provider-specific `.apm/agents/advisor.agent.md` projection is a separate adapter choice and is not required to make the Markdown method understandable.

**Boundaries and why optional:** use for a defined high-value uncertainty, not routine implementation or every design question. Source Adaptive Collaboration already covers conversational depth and when independent review can help. This recipe contributes the distinct-lens assignment, dissent-preserving synthesis, and stopping rule; it must link to applicable Core authority rather than duplicate it.

**Representative draft:**

> Frame one decision and its criteria. Resolve inexpensive factual uncertainty first. If independent perspectives justify their cost, obtain a few distinct positions without sharing the preferred answer or earlier conclusions. Compare their evidence and strongest objections, preserve unresolved tradeoffs, and recommend a direction without voting. The authorized decision-maker retains acceptance.

**Do not carry over:** exact model names, a mandatory three-advisor roster, repository-specific budget ledgers, or an automatic rebuttal round. There is no evidence here that a fixed provider mix or larger council is generally better.

**Decision for later individual approval:** whether to add this distinct recipe to the intentionally small toolkit. Decide runtime projection separately from the method.

**What would establish usefulness:** a real decision gains a material counterexample, revised option, or explicit tradeoff that ordinary single-owner reasoning had missed, at an acceptable cost. Agreement count is not success evidence.

### T28-E04 — Managed worktree delivery, later pilot

**Purpose and audience:** support experienced maintainers running explicitly authorized parallel implementation across genuinely disjoint responsibilities in one Git repository.

**Local evidence:** `.agents/workflows/worktree-program-development.md:11–33,37–44`; `.agents/directives/hierarchical-orchestration.md:14–26,35–56,71–78`; `.agents/templates/memory/project-control-ledger.md:9,40–53,66–100`. The `.apm` catalog supplies separate task and integration roles; their complete bodies were not reviewed for release readiness.

**Reusable behavior:** establish one accepted base; separate mutable ownership; provision each lane in a distinct worktree and correctly rooted session; keep one owner through implementation and correction; return changes to one integration owner; obtain working integration evidence before considering broader refactoring; preserve actual process and Git state during ownership transfer.

**Prerequisites:** Git worktrees, worktree-rooted child sessions, observable execution state, bounded delegation, a shared-contract and ownership model, and explicit authority for the wave and its integration destination. User approval for parallel work is not blanket permission to push or merge. If safe child-session isolation is unavailable, the method must stay sequential.

**Smallest coherent pilot and owned files:** one workflow under `.agents/workflows/`, one reduced project-ledger Template under `.agents/templates/memory/`, and provider-neutral task and integration assignment/return contracts carried by those files or focused on-demand support. Native role adapters, if needed, own only their declared `.apm/agents/` paths and must name actual runtime dependencies. Do not copy the entire role roster or claim the host enforces a permission that it cannot represent.

**Boundaries and why optional:** orchestration is a specialized execution method with real cost. It must not replace Core routing, loading, Memory, or acceptance rules. A recipe may govern its selected wave; installing it must not load the local workspace-wide orchestration Directive for every task. It must not gain authority to edit project ledgers, perform integration, or clean worktrees merely because the package is present.

**Representative draft:**

> Use this method only for an authorized parallel wave with disjoint mutable responsibilities. Start each lane from the accepted base in its own worktree and session. Give each lane one outcome, owner, protected boundaries, evidence, and integration destination. Integrate only within the granted authority, verify the combined result, and preserve unfinished work before transferring ownership or retiring resources.

**Do not carry over:** exact two/four-lane limits, an extra hierarchy after three tasks, fixed status/grace rules, provider assignments, model-specific preparation trials, or compulsory coordinated topic review. Those are local choices or experiments, not prerequisites of worktree isolation.

**Decision for later individual approval:** first approve a narrow pilot with one target runtime and a precise permission/ownership boundary. A separate optional advanced package might later be justified by independent demand, but would need an explicit catalog decision. Do not ship a partial copy that omits required runtime adapters or quietly imports all local Core directives.

**What would establish usefulness:** an isolated trial preserves disjoint ownership, catches integration failures, supports interruption/recovery, and costs less total coordination than sequential execution for comparable work. Existing local instructions demonstrate an intended method, not that evidence.

## Retain, keep local, or treat as core work

| Material | Classification and reason |
| --- | --- |
| Experience Design | Retain existing optional capability. The 22-line Skill and three 13-line references form a compact unit covering journeys, states, evidence, accessibility, and handoff. Its local catalog decision already includes it; do not make a duplicate package or generic replacement. Source: `.agents/skills/experience-design/SKILL.md:8–22` and its three references. |
| Debugging | Existing toolkit capability, not a new extension proposal. The local recipe has a coherent reproduce–hypothesize–discriminate–verify sequence (`.agents/workflows/debugging.md:15–30`). Whether local changes improve the package requires a separate actual payload comparison. |
| Proportionate development | General design judgment worth retaining, with selective optional development guidance. Actual consequences, available recovery, and demonstrated need for complexity are reusable (`.agents/directives/proportional-development.md:13–30,34–51,74–83`). Do not ship its full workspace-wide approval policy or five-profile process as universal Core behavior. E01/E02 can adopt only the evidence/scale guidance they need. |
| Source locality | An opinionated software-structure method that can be offered within software development content. Core already requires narrow useful scope; a mandate for mirrored test trees and specific promotion rules is additional practice, not a missing universal framework rule (`.agents/directives/source-locality.md:11–35`). No separate package is justified by this assessment. |
| Runtime/model calibration | Keep exact model and reasoning assignments in runtime configuration or local policy. Preserve the idea of matching effort to bounded risk, but do not distribute today's provider roster as framework semantics (`.agents/guidance/calibrated-agent-reasoning.md:15–24,28–43`). |
| Coordinated immutable topic review | Keep as an explicit experiment until dependencies and benefit are established. The local coordinator requires the named `inspect-git-objects` tool or supplied immutable content, and returns a gap rather than falling back (`.apm/agents/review-mastermind.agent.md:37–48`). Its intake is tied to local task/status/budget rules. It is not a portable drop-in reviewer. |
| Supervised preparation trial | Local experiment, not a shipped default. Its own text calls it an opt-in, preparation-only trial and defines specific model, supervision, review, and measurement conditions (`.agents/workflows/supervised-luna-preparation-trial.md:11–23,35–73`). No success claim is inferred from its existence. |
| Task numbering and progress display | Local preference. Permanent numeric IDs, exact phase/milestone formatting, two-update completion grace, four-line internal checkpoints, and agent/model displays can serve this workspace without becoming package requirements (`.agents/directives/hierarchical-orchestration.md:27–34`). |
| Decision authority and execution safety | Core authority improvements must be considered as core proposals when they generalize; local external-effect and permission profiles are workspace policy. Do not sell duplicate authority/safety rules as an extension or use optional packaging to weaken mandatory platform constraints. |
| CLI language, migration, and project histories | Local/project evidence. Directory presence and role metadata do not justify distributing an entire C# policy, migration history, or active project ledger. Those bodies were not audited here and no general claim about their correctness is made. |
| Frozen source findings S01–S08 | Core wording, behavior, authoring, and orientation proposals. They remain separate from extension selection; installing a toolkit cannot repair an ambiguous loader contract. |

## Package and approval boundaries

All candidates must use normal installed routes and whole-file ownership. They must avoid replacing the loader or root catalogs, owning user `.overwrite.md` companions, copying private project history, installing external dependencies implicitly, or asserting runtime permission enforcement from prose. A file collision with local customized content requires the normal deliberate ownership decision; identical intent is not permission to overwrite it.

Before any approved candidate is implemented, compare the actual package source with these local inputs, identify every owned installed path, close relative links and runtime dependencies, and show the smallest coherent content change. Review the candidate's behavior separately from any package-boundary, native-role, or permission change. This report recommends no publication, global installation refresh, or merge.

The user can approve or reject **T28-E01**, **T28-E02**, **T28-E03**, and **T28-E04** independently. Approval of an extension candidate does not approve the source findings or the excluded local policies. The current state of every proposal is **awaiting individual user review**.
