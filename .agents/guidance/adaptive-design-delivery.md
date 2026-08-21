---
open-forge:
  description: Preserve context and deliver creative design and implementation work proportionately across greenfield and brownfield systems
  tags: [Core, Guidance, Design, Implementation, Adaptive, Greenfield, Brownfield, Context, Planning, Authority, Review]
---

# Adaptive Design and Delivery

## Scenario

Use this Guidance when an open-ended creative design or implementation request must become a safe, resumable change. It applies to greenfield work and brownfield work where existing behavior, contracts, projections, or history constrain the result. It helps a maintainer or agent decide what to preserve, investigate, plan, delegate, and review. It does not replace a Workflow or make every Workflow stage mandatory.

## Preferred Approach

Keep the maintainer as the decision-maker. When an agent carries out the work, keep one primary agent responsible for carrying intent, planning, integration, and the final recommendation. Agents and councils may investigate, compare, challenge, and recommend. They do not turn a recommendation into accepted direction.

Use the smallest complete approach that protects the decision at hand:

1. Build a compact authority and reference map.
2. Classify the starting point and integration risk.
3. Choose the depth and existing Workflows that add real value.
4. Turn accepted direction into an executable plan and a useful Checkpoint.
5. Delegate only bounded work, then integrate it through the primary context.
6. Review the actual result and reconcile the evergreen source set.
7. Close the work or create a sealed Handoff when a real transfer needs one.

### Build an authority and reference map

Start with the questions that could change the work. For each relevant source, record its path or link, the question it answers, its status, the authority it has for that question, and whether this work may update it. Use the repository's [Sources Of Truth](../maps/sources-of-truth.md) as a starting map when it applies. The destination still defines its own detail.

For nontrivial work, put a `References and authority` section in the plan or Checkpoint. Include the accepted current sources, relevant candidates, generated or projected surfaces, historical evidence, external authorities, and the links between them. Do not call a file authoritative only because its name or location looks canonical. Investigate a conflict before mutating a surface when the conflict could change scope, behavior, acceptance, or the ability to undo the work.

### Keep knowledge states distinct

Use only the Memory roles that answer a real question. A small reversible change does not need every record type.

| State or surface               | Use it for                                                                                                             | Keep it as                                                                                                                                                                                                                        |
| ------------------------------ | ---------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Accepted current truth         | The accepted result within a stated scope.                                                                             | A current authoritative source, often a [Crystallized Document](../memory/crystallized/documents/_documents.md) or a matching Core route. `#CurrentTruth` does not make every copy authoritative.                                 |
| Accepted rationale             | What was chosen, why, and which consequences matter later.                                                             | A focused [Decision](../memory/crystallized/decisions/_decisions.md). The source that defines the current result remains authoritative for that result.                                                                           |
| Candidate analysis or idea     | A useful unresolved question, comparison, possibility, or experiment.                                                  | [Emerging Analysis](../memory/emerging/analysis/_analysis.md) or [Emerging Ideas](../memory/emerging/ideas/_ideas.md), with evidence, assumptions, uncertainty, and a promotion signal. Keep it `#Contextual`, not current truth. |
| Working plan or checkpoint     | The active goal, state, step, next action, and temporary decisions needed to resume.                                   | A [Working Checkpoint](../memory/working/checkpoints/_checkpoints.md). It is not history or accepted truth.                                                                                                                       |
| Transfer boundary              | Stable state that must survive an actual transfer or explicitly planned resumption while the active work may continue. | A sealed [Handoff](../memory/working/handoffs/_handoffs.md). Do not edit it after sealing.                                                                                                                                        |
| Generated or projected surface | A rendered, packaged, installed, public, or otherwise derived representation.                                          | A projection to validate from its source. Do not hand-edit it as the source of truth. Refresh it only through an authorized process.                                                                                              |
| Historical evidence            | Prior behavior, decisions, documents, logs, or experiments that no longer control current work.                        | [Archived Memory](../memory/archived/_archived.md) or the declared historical system. Validate it before restoring any meaning to a current source.                                                                               |

To resist context loss, preserve analysis and ideas only when they are likely to change or explain a later decision. Record the decision question, evidence, assumptions, alternatives, current conclusion, uncertainty, and the condition that would promote, revisit, archive, or prune the material. Do not preserve a conversation transcript as a substitute for a durable source.

### Assess the starting point and integration risk

Classify the work before choosing design depth:

| Starting point         | Authority and inspection                                                                                                                                                                                                              | Main integration risk                                                                                                                  |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| Greenfield             | Treat accepted user direction and accepted Vision or Architecture as the design boundary. Do not invent an existing implementation. Define the first useful vertical slice and its external boundaries.                               | Speculative structure, missed external constraints, or a projection that has no clear source.                                          |
| Brownfield             | Locate the source that defines each current behavior, then inspect surrounding implementation, contracts, tests, consumers, projections, and declared external systems. Existing implementation is evidence, not automatic authority. | Breaking an implicit dependency, confusing a mirror or generated surface with its source, or reviving historical behavior by accident. |
| Unclear or rescue work | Treat the subject as brownfield until its authority and integration boundaries are clear. Reconcile conflicting sources before overwriting one to make the plan simpler.                                                              | Making an irreversible change while the current system or decision authority is still unknown.                                         |

Increase analysis depth when the change crosses a public or shared contract, persisted data, security or concurrency boundary, external integration, generated or packaged surface, release boundary, or hard-to-undo operation. A local and reversible change with no shared contract usually needs only focused inspection and a micro-plan. Size alone does not determine risk.

### Choose depth and Workflows deliberately

Invoke the smallest existing Workflow that resolves an actual uncertainty or execution risk. Honor an explicit Workflow choice or opt-out. Do not invoke a Workflow merely because it exists, and do not copy its procedure into this Guidance.

| Need                                                                                                       | Workflow to consider                                         | Invoke it when                                                                                                       |
| ---------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------- |
| Purpose, audience, value, first useful version, or success is unsettled.                                   | [Vision](../workflows/vision.md)                             | The product or outcome decision still matters to the implementation.                                                 |
| Structure, boundaries, dependencies, tradeoffs, or transition are unsettled.                               | [Architecture](../workflows/architecture.md)                 | Greenfield shaping or brownfield integration risk needs structural analysis.                                         |
| One consequential decision needs independent perspectives or evidence.                                     | [Council](../workflows/council.md)                           | The decision is high leverage or genuinely uncertain, not merely interesting.                                        |
| Accepted direction needs ordered steps, dependencies, and verification.                                    | [Planning](../workflows/planning.md)                         | The work is nontrivial. Use a micro-plan in the primary context for small reversible work.                           |
| Authorized custom behavior needs proportionate implementation, testing, integration, and review.           | [Adaptive Development](../workflows/adaptive-development.md) | One persistent primary owner can preserve coherence without strict phase separation.                                 |
| Explicit contract, evidence, phase boundaries, or a formal acceptance gate materially protect correctness. | [Development](../workflows/development/_development.md)      | The stricter lifecycle earns its cost. Do not impose it on routine documentation, configuration, or repository work. |
| An observed defect needs a reproducible root cause and verified correction.                                | [Debugging](../workflows/debugging.md)                       | Investigate the failure instead of using speculative product changes as the test.                                    |
| A design, change, or repository state needs an evidence-backed read-only assessment.                       | [Review](../workflows/review.md)                             | An actual diff, durable-source transition, boundary, or residual risk deserves a fresh check.                        |

Use [Adaptive Collaboration](adaptive-collaboration.md) for progressive disclosure and convergence when the desired direction is forming. Use [Calibrated Agent Reasoning](calibrated-agent-reasoning.md) to set the attention budget for bounded delegation. These sources complement the selected Workflow. They do not add mandatory ceremony.

### Make the plan executable

For nontrivial work, the plan should state:

- The accepted outcome, audience, scope, non-goals, and acceptance evidence.
- The greenfield or brownfield classification, integration risk, and `References and authority` map.
- Accepted decisions, unresolved decision points, assumptions, dependencies, and rollback or stop conditions.
- Ordered steps with an observable result and verification for each step, including any safe parallel work.
- Allowed and forbidden paths, sources, generated or projection handling, and local-only or external-effect boundaries.
- The Checkpoint update points, handoff condition, and completion condition.

Use the task source declared by the user or workspace when it is authorized. Otherwise, use the current request or a clearly scoped Working record. Do not create a competing Task source or mutate Task state just to make the work look organized. Before mutation, the maintainer must have accepted any material product, Framework, architecture, scope, tradeoff, or acceptance decision. Routine reversible implementation choices may stay with the agent within that authority.

### Maintain checkpoints and bounded delegation

Create one active [Checkpoint](../memory/working/checkpoints/_checkpoints.md) when durable sources alone are not enough to resume after a pause, context restoration, or transfer. Refresh it after an important state change and after context restoration. Keep its goal, current state, current step, accepted decisions and evidence, unresolved questions, next steps, and durable links concise. Remove its active status and archive or prune it when the active need ends.

Create a [Handoff](../memory/working/handoffs/_handoffs.md) only for an actual transfer or explicitly planned resumption that needs a stable snapshot while the Checkpoint may continue to change. Include the boundary status, next action, blockers, and verification state. Seal it and record later changes elsewhere.

Settle the decisions owned by the caller before delegation. Bound every delegated task with accepted meaning, known facts, its outcome, exact read scope or search boundary, allowed and forbidden surfaces, external-effect limit, required evidence, verification, handoff format, stop conditions, and decisions that remain with the primary agent. State what prior work must not be repeated. Delegate exploration, mechanical transformation, or a closed plan step when a separate context helps, and keep assignments non-overlapping unless independent perspectives are the explicit purpose. Do not delegate an unresolved product, architecture, authority, or acceptance decision.

Give writers decided meaning and structure to express under the writing standards. Give explorers one complete bounded evidence question and ask for compact findings rather than raw search transcripts. Give implementers a closed behavior and test plan. Give reviewers an exact baseline and diff, accepted requirements, direct integration neighborhood, and claimed evidence. The primary agent integrates the result, checks the actual changes and evidence, and performs the final review before acceptance within its delegated scope. Preserve a fresh, read-only context when independent review matters.

### Review the result and update evergreen sources

Inspect the actual changed paths and final filesystem state, not only an agent's report. Compare the result with the accepted plan, authority map, allowed surfaces, and completion condition. Check the behavior or prose, relevant evidence, source and projection relationships, and any accidental changes to current, candidate, Working, generated, or historical material.

Use [Review](../workflows/review.md) when a read-only evidence-backed assessment adds value. Route a material finding to the earliest invalidated assumption, decision, plan step, contract, or implementation surface instead of starting an unbounded review loop.

Keep a good evergreen source set rather than a large set of synchronized copies. Update each affected `#Evergreen` current document, Decision, Core route, or Map before closeout. Put current meaning in the source that defines it, rationale in a Decision, unsettled reasoning in Emerging Memory, and temporary continuation state in Working Memory. Update a generated or projected surface only through its authorized source and process. Preserve historical evidence outside current authority and link it when it explains the result. Report an affected evergreen update that is blocked.

### Close out or hand off

At closeout, compare the requested outcome with the actual result and report the changed paths, verification, remaining uncertainty, residual risk, and any decision still requiring the maintainer. Confirm that the authority map and affected evergreen sources are current, candidates remain visibly unaccepted, and generated or historical surfaces were handled according to their roles.

Keep local-only requests local. Do not stage or commit, contact remotes, change Task state, mutate generated build output, or edit frozen legacy code unless the task explicitly authorizes that exact surface. Stop before an external effect or boundary change that is not authorized.

For a real transfer, create the sealed Handoff and state the next action and blocker clearly. For ordinary completion or a routine pause, do not create a Handoff merely in case it might be useful. Do not present a recommendation, review pass, or council convergence as acceptance.

## Why This Works

An explicit authority map makes context loss visible instead of inviting an agent to guess which familiar file is current. Separate Memory roles preserve useful reasoning without promoting candidates, checkpoints, projections, or history into current truth. Adaptive depth spends analysis where integration or decision risk is real, while a small primary source set keeps maintenance cost bounded. A persistent primary agent and bounded delegation provide additional perspective without creating competing decision-makers. Reviewing the actual result and updating affected evergreen sources closes the gap between an attractive plan and a maintainable repository state.

## Invocation

Use wording such as:

```text
Apply the Adaptive Design and Delivery Guidance to this request. Keep the maintainer as the final decision-maker. Work only within [allowed paths and effects], and do not touch [forbidden paths or effects]. Unless explicitly authorized, keep Task state, Git staging or commits, remotes, generated output, and frozen legacy code outside the scope.

First classify the work as greenfield or brownfield and state the integration risk. Build a compact References and authority map. Keep accepted current truth, candidate Analysis and Ideas, Working Checkpoints or Handoffs, generated or projected surfaces, and historical evidence distinct. Recommend only the existing Workflows that add value, and explain why.

Produce an executable plan with ordered steps, observable verification, decision points, boundaries, and a checkpoint strategy. Delegate only bounded work with explicit evidence and handoff requirements. Inspect the actual diff and final state, update affected evergreen sources, and close out with changed paths, verification, residual risk, and any unresolved maintainer decision.
```

## Tradeoffs

- Durable notes improve recovery after context loss, but every record adds maintenance cost. Save only reasoning, plans, and evidence that will matter beyond the current exchange.
- Deeper design, councils, strict development, and independent review can reduce consequential mistakes, but they cost time and tokens and can create false positives. Match them to the decision and integration risk.
- One primary owner preserves coherence, while bounded delegation adds breadth or independent scrutiny. Delegation becomes harmful when the boundary is open-ended or integration is left to the reports.
- Separating current, projected, and historical surfaces protects authority, but it can make discovery slower. Use explicit links and verification rather than collapsing the distinctions.
