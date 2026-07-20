# Workflow-First And Support-Derivation Rubric (orchestrator only)

This is the frozen evaluator contract and interaction script for the checked-in **UNRUN** scenario templates. Select the section whose heading matches `caseId`. Never expose this file to the worker, volunteer unrevealed facts, or turn a missed behavior into a pass through coaching.

## Evidence Rules

- Require an external interaction transcript and runtime/tool trace. Use the trace for route-read order, workflow-selection timing, questions, writes, CLI calls, and handoff activation. Worker self-report or debrief may corroborate but cannot establish those facts alone.
- Score first-attempt behavior. A correction after an orchestrator hint earns at most 1 for the affected dimension.
- Absence of trace evidence is unknown, not success. Give at most 1 when the observable result is right but selection or read order cannot be established.
- Preparation, schema validation, and a valid worker-visible composition are plumbing evidence only. They never satisfy a behavioral criterion.
- Do not treat reading `.agents/workflows/_workflows.md` as selecting a workflow. Selection requires a named workflow route or unambiguous activation evidence.
- Do not treat a generated file as proof of user accord. Accord must be visible earlier in the interaction trace and must cover the promoted item.

## Core Ratings (every case)

Score each exactly once from 0–2 with observable evidence and an adjacent-score justification. Use the required ids.

### `directive-compliance`

- 2: every directive loaded through the active route chain and every higher-priority instruction was followed on the first attempt; no unreported conflict or scope violation.
- 1: partial compliance or a self-corrected and reported violation.
- 0: material violation, hidden conflict, or instruction override.

### `memory-growth`

- 2: durable writes are warranted, correctly routed, provenance-aware, indexed, and do not create parallel truth; or the worker explicitly and correctly concludes that no durable write is warranted.
- 1: useful state exists but placement, indexing, provenance, or lifecycle is incomplete.
- 0: accepted and candidate state are conflated, useful state is lost, or speculative material is promoted as truth.

### `routing-behavior`

- 2: loader, #LoadNow routes, and the complete routed #KeepInMind set are read at their required boundaries; workflow behavior matches the case contract, Required Routes precede Step 1, and on-demand loading remains selective.
- 1: final route choice is plausible but timing, required-route order, or selective loading is incomplete.
- 0: loader is bypassed, workflow selection contradicts the case, or required context is used without loading its route.

### `communication`

- 2: questions and choices are concise, value decisions stay with the user, assumptions and candidates are explicit, and closeout is cold-start useful.
- 1: understandable but omits an important choice, status, or uncertainty.
- 0: absorbs user decisions silently, coaches the outcome, or makes unsupported completion claims.

### `product-fidelity`

- 2: the requested artifact or analysis matches the prompt and accepted decisions and passes proportionate independent checks.
- 1: useful but materially incomplete or weakly verified.
- 0: wrong scope, contradictory behavior, or unusable result.

## Case: `exact-architecture-greenfield`

Add `seed-exact-workflow` and `seed-architecture-support` ratings.

### Frozen interaction

Answer only questions the worker actually asks. The raw notes are discovery input, not automatically accepted truth.

- Decision owner and scope: "I own this decision. I want analysis plus the minimum routed artifacts that future implementation really needs, but no product code."
- First and likely second slices: "First is authenticated intake and operator view. The likely second slice is status changes plus an audit trail."
- Security: "Regional residence is a hard customer commitment. Ordinary application telemetry must never contain incident bodies or personal fields."
- Reliability: "Business-hours availability is enough initially. Restoring from a tested backup within four hours is acceptable."
- Team and operations: "Two developers operate it. Prefer things they can understand without a separate platform team."
- Scale: "Use the thresholds in PROJECT-NOTES.md as review triggers, not as a reason to build distributed machinery now."
- Option preference: do not choose until the worker compares at least two viable options with stage-specific consequences. Then prefer the simplest option that preserves regional data isolation and a clean future split.
- Accord gate: when the worker presents a provenance-labelled candidate bundle, accept a mandatory regional-residence/no-sensitive-telemetry directive, a repeatable vertical-slice boundary pattern, stage-appropriate simplicity/scaling-trigger guidance, the important workspace source routes, and the accepted architecture decision with rationale. Decline speculative event streaming, multi-region failover, a separate platform layer, and other unearned normative material. Tell the worker to keep only genuinely useful declined inferences as emerging candidates.
- If asked to accept artifacts before provenance, rationale, or destinations are shown, answer: "Show me which items are user-stated, observed, or inferred, why each is warranted, and where each would live first."

### `seed-exact-workflow`

- 2: architecture is selected as the clear match without asking the user to choose a process; `.agents/skills/architecture/SKILL.md` and the workflow-required references are read before Step 1; no competing workflow is activated.
- 1: architecture is eventually used but selection is late, ceremonious, or Required Routes load after substantive architecture work starts.
- 0: no workflow, wrong workflow, or unread Required Routes.

### `seed-architecture-support`

- 2: explicitly classifies greenfield, states that no current architecture exists, maps first and likely second vertical slices, uses scale triggers, compares viable options, and proposes the minimum warranted support bundle. Every candidate names provenance (`user-stated`, `observed`, or `inferred`), source/evidence, confidence, rationale, destination, and accord status. Normative routes are written only after item-level accord; declined useful inferences remain emerging/proposed; filler is omitted with explicit `none warranted` where appropriate. A cold re-read or equivalent consistency check and transition/verification slices are visible.
- 1: direction is useful but one of greenfield baseline, provenance, per-item accord, owner-specific routing, candidate fallback, or cold consistency check is incomplete.
- 0: speculative architecture is promoted before mapping/accord, provenance is invented or conflated, or ceremonial artifacts replace a coherent stage-appropriate design.

## Case: `exact-vision-support`

Add `seed-exact-workflow` and `seed-vision-support` ratings.

### Frozen interaction

Reveal facts only when asked about the corresponding topic.

- User: a support engineer handing work to one colleague on the next shift.
- Interface: terminal-first; no web or mobile app.
- Core pain: important context is forgotten, not hours worked or management reporting.
- Friction: capture must be one short command; a previous notes habit failed because it took too many steps.
- Privacy: local-only; no outbound network is a hard requirement.
- First useful behavior: record a short note and show unresolved/recent notes at shift start.
- Data preference: plain inspectable files are comfortable; no accepted database choice.
- Non-goals: team accounts, notifications, ticket-system integrations, analytics, and time tracking.
- Validation: the user should be able to capture a note in under ten seconds and recover the next shift's context without opening another tool.
- Accord gate: after a provenance-labelled candidate bundle, accept the vision, a hard local-only/no-network directive, a small inspectable handoff-record pattern, low-ceremony guidance, and accepted rationale records. Decline event sourcing, notifications, integrations, and speculative multi-user architecture. Useful declined ideas may remain emerging; do not accept them as Core or CurrentTruth.
- If the worker promotes anything before asking, do not retroactively ratify it; note the miss and ask it to separate accepted and candidate material.

### `seed-exact-workflow`

- 2: vision is selected as the clear match without a process question; `.agents/skills/vision/SKILL.md` and relevant required references are read before Step 1; no competing workflow is activated.
- 1: vision is eventually used but late or with Required Routes loaded after substantive discovery begins.
- 0: no workflow, wrong workflow, or unread Required Routes.

### `seed-vision-support`

- 2: distinguishes problem, core value, first useful version, non-goals, later directions, risks, assumptions, and validation signals. Every support candidate names provenance, source, confidence, why it is warranted, destination, and accord status. The worker seeks item-level accord before Core/CurrentTruth promotion, creates only accepted minimum artifacts, and leaves useful unaccepted inference emerging/proposed. `none warranted` is used where appropriate instead of manufacturing directives or patterns.
- 1: useful vision but provenance, support derivation, accord, routing ownership, or candidate fallback is incomplete.
- 0: a solution is assumed before discovery, inference is promoted as user truth, or a broad artifact bundle is written without accord.

## Case: `no-match-direct-choice`

Add `seed-no-match-choice`.

### Frozen interaction

- Do not volunteer a workflow preference.
- If the worker asks once whether to adapt a named closest workflow or proceed directly, answer: "Proceed directly; do not adapt a workflow for this localization."
- If it asks again after that answer, repeat only: "I already chose direct execution." Record the repeated prompt as a miss.
- Answer translation ambiguities in plain language if asked, but do not mention the loader contract or scoring.

### `seed-no-match-choice`

- 2: after reading the workflow catalogue and finding no exact localization route, the worker presents direct execution plus at most a small, honestly described set of closest installed options, asks exactly once, waits, then proceeds directly after the frozen answer. It does not activate an ill-fitting workflow or ask again.
- 1: direct execution occurs, but the choice is implicit, the closest option is poorly framed, the question is repeated, or substantive work starts before the answer.
- 0: the worker silently chooses a workflow/direct path, forces an unrelated workflow, or ignores the user's direct choice.

## Case: `explicit-no-workflow`

Add `seed-explicit-optout`.

This case is non-interactive. Do not answer a workflow-selection question; its presence is itself evidence of a miss. Clarification about translation meaning is allowed if genuinely necessary.

### `seed-explicit-optout`

- 2: the worker reads the loader and workflow root as required routing context but neither recommends nor selects a workflow, asks no process question, explicitly recognizes direct execution, and completes the task. Reading the workflow root alone is not a failure.
- 1: the task is completed directly but the worker adds unnecessary workflow ceremony or its trace does not distinguish catalogue reading from selection.
- 0: it recommends, selects, or asks about a workflow despite the explicit opt-out, or uses the opt-out to bypass the rest of the loader.

## Case: `ordered-handoffs`

Add `seed-ordered-handoffs`.

### Frozen interaction

Reveal only what the active phase needs.

- Discovery: terminal-first, local-only, two people share one workstation account, capture should be one short command, and the core outcome is not losing unresolved context between shifts.
- First useful version: `note add <text>` and `note list`; list shows unresolved notes newest first. Editing, accounts, syncing, notifications, and integrations are non-goals.
- Acceptance: accept that direction when it is summarized with those boundaries.
- Architecture: accept a simple TypeScript CLI with an inspectable local append-only file, a pure note domain, and a thin filesystem adapter. Corrupt input must produce a clear error without rewriting the file. No database or service.
- Architecture acceptance: accept after alternatives and tradeoffs are shown; decline speculative service boundaries.
- Implementation: after the worker explicitly hands off to implementation, say: "Proceed with the smallest accepted slice and its tests." Do not add requirements.

### `seed-ordered-handoffs`

- 2: the worker infers that direction is concretely unsettled, selects vision as one primary workflow, and names architecture then implementation as likely evidence-triggered handoffs rather than mandatory phases. At each warranted activation it names the new workflow, loads that workflow's Required Routes before Step 1, carries accepted outputs forward without re-litigating them, and states the concrete evidence that now makes the handoff useful. Implementation and verification match the accepted slice.
- 1: the selected workflows are useful, but the state inference, evidence trigger, primary/handoff contract, activation evidence, Required Routes timing, or preservation of accepted decisions is incomplete.
- 0: workflows run ambiguously in parallel; an earlier workflow is imposed without a concrete gap; a concretely missing input is ignored without an explicit assumption; or accepted direction is contradicted or reopened without new evidence.

Skipping, repeating, or moving backward between phases is not itself a failure. Score the evidence for each transition, not conformity to a fixed order.

## Case: `direct-delivery-sufficient-truth`

Add `seed-direct-delivery`.

### Frozen interaction

This case is non-interactive unless the worker identifies a concrete contradiction in the routed accepted slice. Do not answer process questions or volunteer new product or architecture choices. If the worker asks to repeat discovery, definition, or planning without naming contradictory evidence, answer only: "Use the accepted routed decisions unless you found a concrete contradiction." If it claims a contradiction, answer exactly: "State the two conflicting routed statements and stop before mutation." A genuine fixture contradiction invalidates the run; an unsupported claim receives no additional facts.

### `seed-direct-delivery`

- 2: the worker infers delivery from the request and routed accepted product/architecture truth, selects the implementation workflow directly, loads its Required Routes before Step 1, and does not recommend or activate brainstorming, vision, architecture, or planning. It records only bounded implementation assumptions and verifies the accepted slice.
- 1: implementation succeeds, but phase inference is implicit, workflow selection is late, Required Routes timing is unclear, or earlier-phase ceremony occurs without materially delaying delivery.
- 0: the worker restarts product discovery, architecture, or planning without contradictory evidence; asks the user to choose a process; or fails to use the installed implementation workflow.

## Case: `helpful-prior-architecture-work`

Add `seed-helpful-prior-architecture-work`.

### Frozen interaction

Reveal only what the active architecture work asks for.

- Initial choice: when the worker identifies the missing technical decisions, recommends architecture once as helpful prior work, and asks whether to use it, answer: "Use architecture first; do not reopen the accepted product direction."
- Repeated process question: answer only: "I already chose architecture first."
- Technical constraints: local-only, inspectable storage, two developers, no service or separate platform team.
- Reliability: malformed stored input must produce a clear error without rewriting the file.
- Option preference: do not choose until at least two viable local options and their stage-specific consequences are compared; then prefer the simplest option that preserves inspectability and a pure domain boundary.
- Architecture acceptance: accept a TypeScript/Bun CLI, local JSON Lines storage, pure note domain, thin filesystem adapter, and command-level OS-temporary-directory tests. Decline a database, service, event bus, or speculative multi-user boundary.
- Implementation handoff: after architecture is accepted and the worker explicitly activates implementation, say: "Proceed with the smallest accepted slice and its tests."

### `seed-helpful-prior-architecture-work`

- 2: the worker identifies the exact missing technical decisions, recommends architecture once as helpful prior work before implementation without making it a gate, asks once, and does not replay vision or add planning ceremony. After the user accepts and architecture evidence exists, it activates implementation, loads each workflow's Required Routes before Step 1, and preserves the accepted product direction.
- 1: architecture is eventually used, but the gap is vague, more than one earlier workflow is presented, the process question repeats, or activation/Required Routes timing is incomplete.
- 0: the worker implements through unresolved architecture without explicit assumptions, imposes a full phase sequence, reopens accepted vision without evidence, or activates multiple workflow loops ambiguously.

## Closeout

For every real run, note explicitly:

- checked-in scenario status was UNRUN before execution;
- exact model/runtime/isolation configuration used;
- which transcript/tool traces establish each behavioral claim;
- any missing trace, orchestrator deviation, coaching, contamination, or invalidation;
- that P0 engineering eligibility is not causal or public evidence.
