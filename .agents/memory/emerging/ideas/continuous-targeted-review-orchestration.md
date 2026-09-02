---
open-forge:
  description: Explore a review-only Mastermind that routes targeted Luna reviewers at coherent CLI file, module, and task checkpoints
  tags: [Memory, Idea, Contextual, Candidate, CLI, Review, Orchestration, Workflow, Agent, Efficiency]
---

# Continuous Targeted Review Orchestration

## Opportunity

The CLI development flow could use a continuous review overlay that catches
architecture, design, C# conformance, refactoring, and evidence problems while
the main implementation flow is still local and reversible. The proposed shape
is a review-only Mastermind supervising targeted Luna/max reviewers in parallel
with the main flow. The reviewer Mastermind would route one reviewer per
relevant responsibility after a coherent file or diff checkpoint, and would
route different top-down lenses after a module or task boundary.

The maintainer is willing to spend more tokens when that buys better adherence
and earlier detection. This record captures that possibility for evaluation. It
does not change the current hierarchical orchestration Directives or authorize
continuous review in every task.

## Current Understanding

The current model keeps one user-facing Overseer responsible for project
architecture, sequencing, integration, and acceptance. A bounded Task
Mastermind owns one task, normally keeps one implementation owner through the
coherent tests/production/repair loop, and uses independent review only when a
named risk justifies it. The Review Evidence Directive requires stable finding
IDs, independent first passes, grouped correction packets, and targeted
rechecks. The C# Directives already require every C# source or test author and
reviewer to read the complete `_csharp.md`, `design.md`, and `style.md` files.
The project ledger owns each task's permanent repository-global numeric ID,
actual name, queue state, and completion grace. Task records define accepted
phase and milestone horizons, the current phase ordinal, completed milestone
count, and separate current state. Progress and checkpoint surfaces derive
those facts.

The proposed overlay is useful because different questions are easy to miss in
one local implementation context:

- A file or diff reviewer can check direct C# design, style, nullability,
  callable surfaces, and source locality.
- A contract-neighborhood reviewer can check the changed behavior against its
  accepted command, serialization, help, lifecycle, or safety contract.
- A module-boundary reviewer can check ownership, dependency direction,
  refactoring scope, and whether an abstraction was promoted too far.
- A task-boundary reviewer can check integration, evidence tiers, residual
  risk, protected paths, and whether the next task remains ready.

The strongest version is not a reviewer on every keystroke. It is a
trigger-based review loop over coherent saved diffs and task boundaries. A
review on every file event would repeatedly inspect incomplete code, produce
duplicates, and compete with the implementation owner for the same changing
state. A review-only Mastermind should therefore select the smallest relevant
set of lenses from the change class, wait for a stable checkpoint, and make
the result easy for the original owner to act on.

This idea remains contextual. The existing architecture, hierarchy, review
budgets, and owner responsibilities remain current until an explicit decision
promotes a smaller workflow or guidance change.

## Candidate Operating Model

The recommended candidate is a review-only Mastermind with no edit authority:

1. One or more implementation owners announce a bounded list of coherent
   immutable checkpoint records. Each record names its semantic owner and
   original return writer, permanent task ID and actual name plus any lane
   display, current Task-record locator, content identity and freshness basis,
   bounded Task-record content or an exact immutable object locator sufficient
   for the coordinator's named object tool, supplied Task-owned current phase
   ordinal, completed milestone count, and current-state suffix, actual ancestor, candidate,
   candidate parent, and any separate authority commit and tree identities;
   exact changed and formerly untracked paths; accepted outcome; protected
   paths; direct integration neighborhood; claimed evidence; topic units and
   prefixes; and current execution state.
   Missing Task-record content, object, or suffix input is `REVIEW_GAP`.
2. The review Mastermind validates each immutable Git record independently and
   dispatches only relevant standing topics. C# conformance, architecture and
   ownership, behavior contracts, and test evidence remain non-overlapping.
   Workflow and repository conformance stays with coordinator intake rather
   than becoming a fifth topic.
3. Each reviewer reads the complete applicable context, including all three
   C# design files for any C# review, and returns stable finding IDs, severity,
   evidence, consequence, smallest correction, and earliest invalidated
   boundary. Reviewers do not edit, commit, or accept architecture.
4. The Mastermind links likely duplicates and preserves material dissent without
   disposition, then groups joined returns by snapshot and original writer.
   Each writer revalidates its findings against current relevant content,
   records dispositions, and owns grouped correction.
5. At one or more module or task checkpoints, the Mastermind may allocate
   capacity across the relevant standing topics while keeping at most one wave
   per task snapshot. These topic passes remain advisory and do not replace a
   separately budgeted fresh holistic review required by the task profile.
6. The original writer revalidates and dispositions the findings, corrects the
   accepted packet, and may ask the Mastermind to route affected topic rechecks.
   A complete new review is reserved for a materially changed review horizon.

The model should use narrow context capsules rather than copy the entire
program history into each reviewer. Capsules name the exact routes, contracts,
changed paths, protected authorities, evidence obligations, and stop
conditions. A capsule is a navigation aid, not a replacement for the routed
authorities it cites.

Every C# implementation or review packet must explicitly name and require
complete independent reading of:

- `.agents/directives/csharp/_csharp.md`
- `.agents/directives/csharp/design.md`
- `.agents/directives/csharp/style.md`

This requirement is a standing adherence guard in the candidate workflow, not
permission to copy those directives into every packet.

## Review Triggers And Lenses

Use saved checkpoints that represent a meaningful state transition:

| Trigger                                           | Targeted lenses                                                                                                          | Why it is useful                                                                    |
| ------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------- |
| Coherent file/diff checkpoint                     | C# conformance, callable surface, source locality, directly affected contract, and focused test-evidence lens            | Catches local PR smells before they spread while the changed neighborhood is small. |
| Capability or module completion                   | Architecture/dependency direction, refactoring ownership, shared-versus-local promotion, and affected evidence-tier lens | Tests whether local code still fits the accepted top-down model.                    |
| Task completion or protected integration boundary | Fresh architecture, contract, integration, public output, lifecycle/safety, and release/AOT lens as applicable           | Checks cross-surface consequences before the next dependent task or integration.    |
| Material correction                               | Recheck only changed finding IDs and affected consumers by default                                                       | Avoids paying for a full repeated review when the horizon is unchanged.             |

The trigger is a coherent immutable commit or state boundary, not an editor
keystroke. A review Mastermind may coalesce frequent events and decline a review
when no relevant unit exists or the recorded review budget is exhausted. It
does not cancel or replace an owner because progress commentary is quiet.

Escalate only consequential design or review questions to the higher-reasoning
Sol/xhigh role. Keep Luna/max for targeted semantic reviews, direct evidence
inspection, and bounded implementation-context checks. Use exact pre-decided
mechanical commands only for mechanical evidence roles; semantic reviewers must
load normal Open Forge context.

## Benefits And Tradeoffs

Potential benefits include earlier detection of architecture drift, more
reliable C# directive adherence, truthful test-tier classification, reduced
refactoring rework, independent challenge of a persistent implementation
owner, and better continuity when a task crosses several boundaries. Stable
finding IDs and grouped packets can make corrections more focused than a late
whole-task review.

The costs are material:

- Review storms can consume more tokens and wall-clock time than the defect
  prevention justifies, especially when every small file change triggers a
  complete lens set.
- Parallel reviewers can rediscover the same issue, disagree on severity, or
  report preferences as defects. The coordinator must link likely duplicates
  and preserve dissent, while disposition remains with the original writer.
- A reviewer can inspect a stale partial diff while the implementation owner
  is still changing the file. Stale findings add noise and can send a correct
  owner toward an obsolete correction.
- Too many responsibility lenses can create overlap. C# design, architecture,
  refactoring, and contract reviews may each claim the same call surface.
- Waiting for review results can increase integration latency and tempt the
  implementation owner to continue across an unresolved decision frontier.
- Independent reviewers can produce false positives when they lack the
  command-local context, accepted contract, or realistic threat boundary.
- A second Mastermind can blur authority if it starts deciding architecture,
  changing task state, editing files, or directing the user instead of
  returning a bounded review packet.
- More stored review records can become a second task system unless the
  Mastermind consolidates findings into the active Task and retains only
  reusable evidence.

The review value should therefore be measured by accepted material findings,
prevented rework, and decision impact, not by reviewer count or raw finding
count.

## Possibilities

The idea has several forms:

1. **Checkpoint overlay (recommended):** One review-only Mastermind receives
   coalesced file/diff and module/task completion events, dispatches targeted
   lenses, and returns one correction packet. It preserves the current task
   owner and hierarchy while adding independent scrutiny at named boundaries.
2. **Task-boundary reviewer:** Run targeted reviewers only after a complete
   module or task. This is cheaper and less noisy, but it may find structural
   defects after more code depends on them.
3. **Periodic batch reviewer:** Review a bounded batch on a schedule or after a
   set of coherent commits. This reduces invocation overhead but weakens the
   feedback loop for local design mistakes.
4. **Per-file event reviewer:** Start a reviewer for every changed file. This
   gives fine-grained feedback but has the highest storm, duplicate,
   stale-state, and latency risk. It should remain an experiment at most, with
   aggressive coalescing and circuit breakers.
5. **Specialist councils:** Run several reviewers and a synthesis pass for a
   consequential architecture frontier only. This is appropriate when the
   decision is uncertain, not as a default replacement for task ownership.

The first candidate should combine the checkpoint overlay with task-boundary
fresh review. It should not add a new Framework primitive, a new authority
role, or a mandatory review phase until repeated evidence shows that existing
Guidance and Task packets cannot express the useful behavior.

## Evidence

Existing repository evidence supports exploring this candidate but does not
prove its productivity or token economics:

- The [CLI development flow evaluation](../observations/2026-08-21_cli-development-flow-evaluation.md)
  records that the earlier implementation delegated local behavior before the
  structural foundation was closed, missed an intermediate physical escape,
  and lost reasoning when review returns were incomplete. It also records that
  the current architecture-first flow has higher planning and closeout cost
  without an empirical speed or token measure.
- The same observation records three Find Gray attempts reaching their
  execution limit before a broad contract/wiring packet was complete. It
  supports smaller bounded packets and fresh integration review, but does not
  show that delegation itself is unsuitable.
- The [Architectural Perspectives](../../../guidance/architectural-perspectives.md)
  Guidance already separates architect, task-master, advisor, reviewer, and
  implementer questions. The [Review Evidence](../../../directives/review-evidence.md)
  Directive already defines stable findings, independent first passes, grouped
  corrections, and targeted rechecks.
- The current [CLI quality-remediation Task](../../working/cli-development/tasks/cli-quality-remediation.md)
  captures eleven actual defects and two candidates from a strategic first-pass
  audit. Those findings show the value of architecture, refactoring, C# design,
  and test-evidence lenses, but they do not establish that continuous review
  would have found them earlier or at lower total cost.
- The [Perspective Lenses](perspective-lenses.md) and [Scope Capsules And A
  Critical Loader Loop](scope-capsules-and-critical-loader-loop.md) ideas
  already treat named lenses and compact context as candidates rather than
  accepted Framework primitives.

No current evidence supports reviewing every keystroke, spawning one reviewer
for every possible responsibility on every change, or claiming that more
reviewer invocations automatically improve quality.

## Proposed Experiment

When the maintainer explicitly selects an experiment, compare the existing
bounded-review flow with the checkpoint overlay on several comparable coherent
tasks. A possible treatment is:

- The implementation owner emits one stable file/diff checkpoint after each
  meaningful local increment.
- A review-only Mastermind dispatches only relevant topics, with capacity
  derived from current runtime evidence and coherent-checkpoint coalescing.
- At task completion, a fresh Sol/xhigh top-down review checks architecture,
  refactoring/locality, evidence tiers, integration, and public/AOT boundaries
  as applicable.
- Reviewers remain read-only. The original Task Mastermind retains correction,
  task-state, integration, and acceptance authority.
- Every topic that touches C# reads and fingerprints the three complete current
  C# Directive files. Each finding has a stable unit-derived ID. The coordinator
  links likely duplicates without erasing dissent.

Collect, for each comparable task:

- material findings caught before integration;
- accepted versus rejected, duplicate, preference, and false-positive rates;
- prevented or introduced rework cycles and correction latency;
- reviewer tokens, wall-clock latency, queue depth, cancellation, and stale
  checkpoint rates;
- missed protected-surface, contract, evidence-tier, or integration issues;
- owner-reported usefulness and interruption cost; and
- whether the overlay changed architecture authority, task ownership, or
  closeout quality.

Compare the treatment with a baseline that keeps the existing review budget and
one coherent final review. Do not compare raw finding totals without their
dispositions. Record enough context to distinguish a true review catch from an
issue that the implementation owner would have found in the ordinary flow.

## Circuit Breakers And Operating Limits

Any future trial should include explicit internal limits and stop conditions:

- Coalesce repeated lower checkpoints, and let a higher coherent checkpoint
  subsume queued review of identical content.
- Stop a lens after repeated infrastructure failure or after it returns no
  usable checkpoint, following the existing failure-circuit rule.
- Do not exceed the task's recorded review, council, correction, or concurrency
  budgets. Additional lenses require a named distinct risk and an updated
  authority decision.
- Do not dispatch all lenses when the changed paths and contract neighborhood
  do not justify them.
- Do not allow reviewers to edit, commit, change generated routing, mutate
  remote state, or accept a product, architecture, or lifecycle decision.
- Pause only the affected boundary when review exposes an unresolved choice;
  continue unrelated safe work.
- Keep one authoritative synthesis in the active Task or checkpoint. Retain
  an Emerging Observation only when recurrence, surprise, cost, or decision
  value justifies it.
- Keep Mastermind-to-Overseer execution checkpoints extremely short:
  `Done:` concrete change or evidence, `Now:` one active action, `Next:` one
  boundary, and `Blocker:` only when a real blocker exists. Exact pre-edit
  architecture packets may retain necessary detail, but should lead with the
  same compact status shape. When a task mapping applies, `Now:` derives the
  permanent ID and actual name from the ledger and phase and milestone from the
  Task record. The phase numerator is the current active ordinal, while the
  milestone numerator counts completed milestones and the active milestone is
  named only in the suffix. The checkpoint does not own queue or completion grace. The QR
  execution showed that this makes latency circuits and unfinished boundaries
  visible without repeatedly transmitting the accepted context.

## Open Questions

- What is the smallest stable checkpoint: an edited file, a saved diff, a
  coherent local capability, a phase commit, or a Task boundary?
- Which responsibility taxonomy avoids overlap between C# design, architecture,
  refactoring, contract, safety, and evidence lenses?
- How should the review Mastermind route lenses from paths and changed symbols
  without building a second semantic dispatcher?
- What context capsule is sufficient for independence without making each
  reviewer rediscover the complete project?
- How should stale partial reviews be recorded and excluded from correction
  packets without cancelling active ownership from silence?
- What maximum parallelism and latency preserve flow without creating a review
  queue or storm?
- When should Luna/max suffice, and when does a consequential question justify
  Sol/xhigh escalation?
- How should findings be merged when two reviewers disagree about severity or
  earliest invalidated boundary?
- Where should reusable review evidence live without creating a second task or
  architecture system?
- Which metrics best capture prevented rework and quality, rather than merely
  review activity?
- Does the current hierarchical model need any change at all, or can Guidance,
  Task packets, and existing reviewer roles express the experiment completely?

## Future Hypotheses

Automatic or per-file fan-out, a fifth workflow topic, dirty or untracked
snapshot transport, cross-runtime permission and collection guarantees, hard
concurrency numbers, optimal reviewer economics and metrics, and Extension
packaging remain Emerging hypotheses. The selected repository-local trial does
not claim or require them.

## Promotion Signals

Consider promoting a small workflow or Guidance change only after several
comparable tasks show that the overlay catches material issues before
integration, reduces rework or decision delay, and does so with acceptable
token, latency, and maintenance cost. Positive evidence should also show that
reviewers remain independent, likely duplicates are linked without losing
dissent, stale partial state is controlled, and the original owner understands
and can apply the grouped packet.

Promote the smallest useful change. A short checkpoint rule or targeted-lens
section is preferable to a new Framework primitive unless repeated tasks prove
that selection, loading, inheritance, or lifecycle behavior cannot be expressed
through current Guidance and Task context.

Reject or narrow the idea if most findings are duplicates, preferences, or false
positives; if review latency blocks coherent implementation; if stale context
causes rework; if the overlay does not reduce missed architecture or evidence
issues; if context-packet maintenance costs exceed prevented rework; or if the
second Mastermind blurs authority. A no-op result is valid.

## Related Records And Sources

- [CLI Quality Remediation](../../working/cli-development/tasks/cli-quality-remediation.md)
- [CLI Development Flow Evaluation](../observations/2026-08-21_cli-development-flow-evaluation.md)
- [Architectural Perspectives](../../../guidance/architectural-perspectives.md)
- [Adaptive Collaboration](../../../guidance/adaptive-collaboration.md)
- [Calibrated Agent Reasoning](../../../guidance/calibrated-agent-reasoning.md)
- [Hierarchical Project Orchestration](../../../directives/hierarchical-orchestration.md)
- [Program Architecture And Delegation](../../../directives/program-architecture.md)
- [Review Evidence](../../../directives/review-evidence.md)
- [Review Workflow](../../../workflows/review.md)
- [Adaptive Development Workflow](../../../workflows/adaptive-development.md)
- [Perspective Lenses](perspective-lenses.md)
- [Scope Capsules And A Critical Loader Loop](scope-capsules-and-critical-loader-loop.md)
