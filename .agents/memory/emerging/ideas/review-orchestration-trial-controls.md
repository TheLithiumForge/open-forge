---
open-forge:
  description: Explore truthful progress reporting, immutable review snapshots, topic review coordination, and adherence evidence for the selected orchestration trial
  tags: [Memory, Idea, Contextual, Candidate, Review, Orchestration, Workflow, Agent, Progress, Evidence, Efficiency]
---

# Review Orchestration Trial Controls

## Purpose

This record refines the [Continuous Targeted Review Orchestration](continuous-targeted-review-orchestration.md)
idea with the maintainer's operating directions and the first real trial. The
linked Directives and Workflows now define the accepted general controls. This
Emerging record keeps trial-specific capability, observed harness behavior, and
open infrastructure questions without making coordinated review mandatory.

The maintainer selected a bounded experiment with higher review parallelism when
it improves adherence. One read-only coordinator may route several focused
reviewers over immutable snapshots. The writer remains the only task owner that
decides finding dispositions, chooses repairs, changes task state, or accepts
the result.

## Stable Progress Display

Current policy uses a stable display layer that lets the maintainer see task and
phase progress without replacing semantic task, finding, evidence, or Git
identities.

- Give every top-level task one permanent repository-global numeric ID and its
  mandatory actual name. The project control ledger retains that mapping across
  completion, reopening, and follow-up work and never reuses the ID.
- Use `Task X “<actual task name>” (phase A/B): milestone C/D`. Add `/Y` after
  `X` only when the ledger declares a stable repository-global task horizon;
  never derive it from the active or visible queue.
- Treat `phase A/B` as the current active phase ordinal and declared phase
  count, starting at `1/B`. Treat milestone `C` as completed milestones,
  including zero, and `D` as the fixed milestone count. Keep the phase ordinal
  and completed milestone count non-regressing inside each Task-owned accepted
  horizon. Name the active milestone only in the suffix. For example,
  `(phase 3/3): milestone 3/4 — M4 review active` means the task is active in
  its final phase and three milestones are complete. The final phase may remain
  `B/B` while work continues. Reserve `C=D` for task completion.
- Disclose accepted scope change, reopening, or follow-up work as a new
  Task-owned horizon. Start the new phase at `1/<new B>` and milestone progress
  at `0/<new D>` unless the Task record can state preserved milestone progress
  truthfully. Optional local letter labels such as `Phase A` remain separate
  and never become task or phase identities.
- Keep durable semantic IDs, dependency edges, finding IDs, commit IDs, and tree
  IDs authoritative beneath this display. Permanent task identity never grants
  authority, sets execution order, or proves acceptance.
- Every progress-bearing Overseer update renders the nonempty `Active`,
  `Recently completed`, and `Queued` sections. Queue order follows project
  priority and dependencies rather than task ID order. Queued tasks show their
  permanent ID and name without invented phase or milestone horizons.
- A completed task appears on its completion-bearing update and exactly two
  subsequent progress-bearing Overseer updates, then leaves the visible queue
  before the third. Non-progress Overseer messages and descendant updates do
  not consume this grace. Reopened or follow-up work reuses the same ID and name
  and clears stale completion grace.
- The ledger owns permanent identity, name, queue state, and grace. The Task
  record owns phase and milestone. The Overseer renders and advances the grace
  counter. Project Status and Checkpoint records derive and link these facts.

Task, Integration, and Review Masterminds must report checkpoints in compact
caveman form:

```text
Done: <completed evidence or commit>
Now: Task X[/Y] “<actual task name>” (phase A/B): milestone C/D — <active operation>
Next: <next meaningful milestone>
Blocker: <none or one real blocker>
```

Omit incidental transcript detail. A completion packet may link to durable
evidence, but its summary should preserve this shape.

### Progress Scenarios

These scenarios are focused contract evidence for the display policy. They do
not add trial-only behavior.

| Scenario                             | Derived status                                                                                                                                                                                     | Queue and grace evidence                                                                                                           |
| ------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| Before the first milestone completes | `Task 2 “Review Orchestration Workflow” (phase 1/3): milestone 0/4 — M1 active`                                                                                                                    | The task is `ACTIVE`; phase starts at one and zero completed milestones is valid.                                                  |
| Final phase and milestone active     | `Task 2 “Review Orchestration Workflow” (phase 3/3): milestone 3/4 — M4 active`                                                                                                                    | The task remains `ACTIVE`; the final phase ordinal is visible and the active final milestone is not counted as complete.           |
| Completion-bearing update            | `Task 2 “Review Orchestration Workflow” (phase 3/3): milestone 4/4 — Completed`                                                                                                                    | The task enters `RECENTLY_COMPLETED` with two subsequent progress-bearing updates remaining; this update does not decrement grace. |
| First subsequent progress update     | The same completed status remains visible.                                                                                                                                                         | Render, then decrement remaining grace from two to one.                                                                            |
| Second subsequent progress update    | The same completed status remains visible.                                                                                                                                                         | Render, then decrement remaining grace from one to zero; dequeue before the next progress update.                                  |
| Non-progress or descendant update    | No progress section is required.                                                                                                                                                                   | Do not consume completion grace.                                                                                                   |
| Reopened or follow-up horizon        | Reuse `Task 2 “Review Orchestration Workflow”`; declare `phase 1/<new B>` and `milestone 0/<new D>` unless truthful milestone progress is preserved, then name the active milestone in the suffix. | Return to `ACTIVE` and clear stale completion grace.                                                                               |

Review Mastermind mapping has a separate restricted-input check:

| Scenario                                                                                                                        | Required result                                                                                                                                                                                                                                                                                                                                        |
| ------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Valid task-mapped checkpoint or return                                                                                          | Validate the supplied ledger ID, name, and optional global horizon together with the current repository-relative Task-record locator, content identity, freshness basis, current phase ordinal, completed milestone count, current-state suffix, and bounded immutable content. Render the validated mapping without reading mutable filesystem state. |
| Missing or inconsistent Task record, identity, freshness, horizon, phase, completed milestone progress, or current-state suffix | Return `REVIEW_GAP` for that snapshot before topic launch. Do not read or invent mutable filesystem state.                                                                                                                                                                                                                                             |
| Cross-task intake without one return mapping                                                                                    | Join by snapshot and writer without fabricating one combined task status.                                                                                                                                                                                                                                                                              |

## Observable Agent State

Missing optional progress commentary is not failure evidence. When the runtime
does not expose the child's current activity, report `progress unobserved`.
Do not call the child healthy, hung, or failed from silence alone.

The supervisor should distinguish these conditions when direct evidence exists:

| Condition                         | Evidence boundary                                                                            | Safe response                                                                         |
| --------------------------------- | -------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| Queued                            | The runtime explicitly reports queueing or capacity wait                                     | Preserve ownership and wait or reduce capacity pressure.                              |
| Loading context                   | The child or runtime explicitly reports loading                                              | Do not require another blocking acknowledgement.                                      |
| Tool active                       | A known owned process or runtime event is active                                             | Monitor the exact operation and avoid overlapping mutation.                           |
| Approval pending                  | The runtime exposes a pending approval                                                       | Resolve authority or keep the affected boundary paused.                               |
| Progress unobserved               | The child is active but no finer signal is available                                         | Inspect observable status, Git state, and owned processes. Do not cancel for silence. |
| Completed, synthesis not observed | A completed result exists but the owning context has not incorporated it                     | Collect and inspect the result before starting replacement work.                      |
| Confirmed failure                 | A returned error, failed process, explicit blocker, or confirmed lost context proves failure | Classify the cause and use one changed fallback.                                      |
| Confirmed interruption            | The runtime confirms interruption and owned mutating processes have stopped                  | Inspect partial artifacts and transfer ownership explicitly.                          |

A quiet interval triggers inspection, not cancellation. A failed command repeated
with unchanged input is stronger circuit-breaker evidence than a writer that has
not emitted a file or message.

The current runtime persists completed results and completed-but-open child
relationships, but no parent-side collection acknowledgement was found. Unless
the supervisor can identify incorporation in its own state, report
`completed-open; collection unknown` rather than claiming `uncollected`.

Use an explicit handshake only when a real boundary needs it. Make the handshake
one short completed turn, then resume the work in a separate turn. Do not make
every child acknowledge sources before it can begin. Required first-hand reads
remain mandatory, and result conformance remains separate evidence.

Before another writer takes over a mutable boundary:

1. Request interruption and record the runtime's returned state.
2. Confirm the child is interrupted rather than assuming the request succeeded.
3. Identify and stop only the exact mutating processes owned by that child.
4. Inspect commits, changed and untracked paths, partial artifacts, and claimed
   evidence.
5. Record the ownership transfer and give the new writer the preserved artifact
   identities and remaining boundary.

Do not hard-code a historical concurrency limit. Inspect the current runtime.
Open child relationships are not active-compute counts. Account from the
runtime's current active states and declared cap, prefer reuse when a child's
context remains suitable, and close completed or dormant children when the
runtime supports confirmed closure. Exact capacity refusal and closure behavior
remain harness-specific until exercised.

## Writer Checkpoints And Commit Discipline

Writer Task Masterminds should commit at coherent green boundaries when local
commits are authorized, the project commit window is open, and a commit improves
review, recovery, or integration. This is a preference for useful named
snapshots, not a commit for every keystroke, file, or formal phase.

A reviewable commit should:

- represent one coherent capability or correction boundary;
- pass its focused evidence without known false-green conditions;
- include every tracked and previously untracked file in the review horizon;
- use a plain natural past-tense subject without a Conventional Commit prefix;
- preserve natural time and the active project commit window; and
- identify the exact commit and tree used by reviewers.

The active task packet supplies any stricter message-review rule. The Overseer
retains integration order, squash messages, final acceptance, and baseline
advancement. Integrators and reviewers consume exact reviewed commits or ranges,
never a moving branch head.

## Immutable Review Intake

One review intake may contain several snapshots from branches or worktrees in
the same repository. A worktree or branch is only a locator. Each snapshot must
resolve to immutable Git objects before review begins.

The intake should contain one independently validated record per snapshot. Each
record contains:

- a stable snapshot key, working-root or worktree locator, task and lane display
  labels, semantic task owner, and original writer that receives the return;
- for each task-mapped checkpoint or return, the current repository-relative
  Task-record locator, its content identity and freshness basis, bounded
  Task-record content or an exact immutable object locator sufficient for the
  coordinator's named object tool, and the supplied Task-owned current phase
  ordinal, completed milestone count, and current-state suffix; missing content,
  object, or suffix input is `REVIEW_GAP`;
- actual ancestor commit and tree;
- candidate commit and tree, plus candidate parent commit and tree;
- accepted authority commit and tree when it differs from the code ancestor;
- exact changed paths and direct integration neighborhood;
- explicit confirmation that every relevant formerly untracked artifact is
  committed in the candidate;
- accepted outcome, contracts, invariants, protected paths, and non-goals;
- claimed evidence with command, artifact freshness, counts, failures, skips,
  warnings, and limitations; and
- selected topic, review-budget unit, routing reason, and whether the pass is
  independent or a targeted recheck.

Validate every record independently before forming its ordinary range. A
non-ancestral accepted baseline may substitute only when its tree is proven
exactly equal to that record's candidate-parent tree and the packet records the
different provenance. Other non-ancestral comparisons require an explicit
reconciliation purpose and merge-base analysis. Never describe tree equivalence
as commit ancestry or let one Task Mastermind own a peer record's semantics.

Reviewers inspect the named commit and tree, not current worktree files. A writer
may continue on a later commit while review runs, but findings remain bound to
the inspected snapshot. Before repair, the writer revalidates each finding
against current relevant content.

## Coordinator And Topic Reviewers

The proposed authored agent sources are:

| Agent source                                                     | Model     | Focus                                                                                                                                                  |
| ---------------------------------------------------------------- | --------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `.apm/agents/review-mastermind.agent.md`                         | Sol/xhigh | Validate snapshots and process conformance, select topics, preserve dissent, link likely duplicates, and return one joined report without disposition. |
| `.apm/agents/topics/csharp-conformance.agent.md`                 | Luna/max  | C# nullability, callable design, construction, style, namespace, and folder conformance.                                                               |
| `.apm/agents/topics/architecture-ownership-refactoring.agent.md` | Sol/xhigh | Dependency direction, responsibility boundaries, nearest shared scope, refactoring scope, and integration fit.                                         |
| `.apm/agents/topics/behavior-contracts.agent.md`                 | Luna/max  | Accepted behavior, public representation, errors, state, safety, lifecycle, and compatibility.                                                         |
| `.apm/agents/topics/test-evidence.agent.md`                      | Luna/max  | Test tiers, independence, selection, receipts, artifact freshness, coverage claims, and false-green risk.                                              |

Any topic that reviews C# source or tests must independently read the complete:

- `.agents/directives/csharp/_csharp.md`
- `.agents/directives/csharp/design.md`
- `.agents/directives/csharp/style.md`

The coordinator does not become a second architect or task owner. Workflow and
repository conformance are coordinator intake and process validation, not a
fifth standing topic. It allocates runtime capacity across records, keeps at
most one active wave per task snapshot, and groups joined returns by snapshot
and original writer. The coordinator may link likely duplicates, but it
preserves material disagreement. Each original writer alone decides accepted,
rejected, duplicate, preference, false-positive, fixed, and deferred
dispositions for its record and applies one grouped repair packet.

The authored trial permissions deny native filesystem reading, searching,
listing, editing, language-server use, and Bash for the coordinator and topics.
They allow only the named `inspect-git-objects` custom tool. OpenCode loads its
tracked adapter from `.opencode/tools/inspect-git-objects.ts`; structured
`operation`, `object`, `otherObject`, and `path` fields delegate without a shell
to the fixed repository engine in
`src/agent-tooling/review/inspect-git-objects.ts`. The engine
accepts only fixed object, tree, path, diff, diff-check, and ancestry operations
over complete object IDs; it rejects refs, selectors, revision syntax,
arbitrary or unknown fields, output targets, and unsafe tree paths. The
coordinator retains its exact four-topic task allowlist. A non-identical Codex
or other runtime projection receives bounded immutable content in its packet or
returns a gap; this authored OpenCode policy is not a cross-runtime enforcement
claim.

Each material finding carries a stable topic-scoped ID, inspected commit and
tree, severity, category, exact location, evidence, consequence, smallest
credible correction, earliest invalidated boundary, confidence, and missing
verification. A fresh first pass does not receive earlier conclusions. A
correction pass receives only the affected finding IDs and criteria.

Route topics by changed responsibility rather than invoking every reviewer:

- A coordinated snapshot with changed C# production or test code normally
  selects C# conformance, behavior contracts, and test evidence as their
  separate questions apply.
- A move, new shared capability, project reference, module boundary, or task
  boundary may add architecture/ownership.
- `.apm`, `.agents`, generated navigation, package metadata, or workflow changes
  require coordinator repository and workflow conformance validation.
- A correction selects only topics that own affected finding IDs.

Use at most one active review wave per task snapshot. Coalesce lower checkpoints
into the next coherent green commit. A higher task boundary subsumes queued
lower-boundary reviews over the same content. Every topic consumes a named
budget unit. Topic passes remain advisory and do not replace a fresh holistic
review when the task profile requires one.

## Adherence And Evidence

Add an architecture checkpoint after the first representative connected slice
crosses a shared or public boundary. Inspect the actual ownership, dependency
direction, call surfaces, consumers, output shape, and applicable rules before
similar code multiplies.

Behavior-preserving refactors need semantic or differential evidence in addition
to compilation and test counts. Compare consequential output facts, presence and
absence, ordering, nullability, effects, findings, recovery state, public bytes,
and interruption behavior where they apply.

Each task should name repository-verified execution recipes. An evidence receipt
records the working root, source and configuration identity, exact command,
toolchain, fresh artifact identity, selected, discovered, and executed test
counts, failures, skips, warnings, and limits. Exit zero alone is not a pass:

- zero discovered or executed tests do not prove a positive gate;
- warning-bearing or partially loaded format output is not a warning-free pass;
- `--no-build` does not prove an edit unless the tested artifact was built from
  the current inputs; and
- an expected negative search with no matches is evidence only for that exact
  negative claim.

Invalidate evidence in proportion to changed inputs. Local extraction first
invalidates focused consumers. Shared contracts, composition, serialization,
public help, packages, generated resources, or native paths require broader
evidence. Documentation may affect packaging or hashes and is not automatically
evidence-neutral.

Reduce mutable Memory duplication by giving each changing question one source:

| Question                                                                                          | Candidate source                                |
| ------------------------------------------------------------------------------------------------- | ----------------------------------------------- |
| Permanent task identity, queue, and grace                                                         | Project ledger                                  |
| Accepted task horizons, current phase, completed milestones, current state, blocker, and evidence | Task record                                     |
| Task dependencies and schedule                                                                    | Program plan                                    |
| Active lane, branch, worktree, and owner                                                          | Project ledger                                  |
| Resumption state                                                                                  | Checkpoint that links to the sources above      |
| Fixed transfer boundary                                                                           | Sealed Handoff                                  |
| Accepted product or Framework behavior                                                            | Current contract or designated current document |

Keep commit presentation separate from technical evidence. A message-only
identity change does not invalidate an unchanged tree, while an evidence input
change does even when the commit label looks similar.

## Source Data And Current Evidence

The maintainer-provided raw analysis attachment is source data and a set of
hypotheses, not current Framework authority.

Read-only preparation confirmed:

- `.apm/agents/**/*.agent.md` is the tracked authored agent source selected by
  `apm.yml`. `.codex/agents` and `.opencode/agents` are ignored generated
  projections. `apm.lock.yaml` is derived deployment inventory.
- APM validation recognizes the current recursive agent source set. The local
  Codex model patcher at
  `src/agent-tooling/agent-projections/patch-codex-agent-models.ts` also scans
  nested topic folders recursively.
- The installed runtime persists turn status, tool/process activity, completed
  results, interrupted turns, queue state, and open child relationships. It does
  not expose a parent-side collection acknowledgement, and open relationships
  are not active-compute counts. Queue acknowledgement, capacity refusal, and
  confirmed closure still require controlled exercises.
- Preparation ran `apm compile --validate --local-only` successfully over the
  then-current 22 authored agent sources. The trial adds five authored sources;
  their derived lock and runtime projections remain a later Task Mastermind
  boundary. `open-forge doctor` reported one pre-existing warning in the C#
  entrypoint because it combines substantive local `Axioms` with the inherited
  sentinel. Future implementation must not call that warning-free.
- APM dry-run is not a no-network guarantee. One delegated dry-run attempted
  read-only organization-policy discovery. Future local validation must disable
  policy discovery explicitly and avoid network-capable paths.

The first real Task 1 review trial uses four focused read-only lenses over:

- actual ancestor `f324beaeaf9314d5efd6503bca2148e800710a9a`, tree
  `0def1aba230d1871b89736d8921a23d776efc04a`;
- candidate `1f8708f6e3720b7f654cf4479714fe14de27f3eb`, tree
  `306324e2ffe0238448a46a4545e7295cd9bc060e`; and
- integrated authority identity `868860e046c954b0f1d654833afb20052fda18dd`,
  also tree `0def1aba230d1871b89736d8921a23d776efc04a`.

The integrated authority commit is not an ancestor of the candidate. Its tree is
exactly equal to the candidate parent's tree, so it is valid only as an explicit
tree-equivalent authority identity. The ordinary review range remains the actual
ancestor to candidate range. The trial outcome and reviewer yield are still
pending.

During the trial, the named CLI Quality Remediation worktree retained the
candidate as `HEAD` but gained later dirty JSON-context and task-record changes.
This is a concrete stale-state probe: topic reviewers must use Git object reads
for the supplied commit and tree. Filesystem and LSP views of that live worktree
belong to a different snapshot.

At trial start, the CLI Quality Remediation record named one final review budget
unit. The four independent topic lenses cannot share that unit merely because
one coordinator joins their reports. The Overseer must record four stable topic
consumption IDs, or explicitly revise the internal review budget with the
maintainer-authorized adherence experiment as the reason. The coordinator
consumes no review unit when it only validates, schedules, and synthesizes.

### Dogfood Wave `T2-M5-W1`

This is trial evidence, not new binding policy. The wave inspected immutable
snapshot `08d2746fd490257d595210276aa1ce5f831783d3`, tree
`2f1402106e7e8dfedeb786dbbf4d78c76eea3e43`.

| Finding             | Disposition and result                                                                                                                                                                                                   |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `T2-M5-ARCH-01-F01` | Accepted and fixed in grouped-repair candidate `87f2acb961750dc06f9fd574d82edbe2ac0a6f46`, tree `17fdf3c8fb4f57ecc26221b87c3650b969e09e2f`.                                                                              |
| `T2-M5-ARCH-01-F02` | Accepted and fixed in the same grouped-repair candidate and tree.                                                                                                                                                        |
| `T2-M5-ARCH-01-F03` | Accepted and fixed in the same grouped-repair candidate and tree.                                                                                                                                                        |
| `T2-M5-BEH-01-F1`   | Accepted. The grouped repair was partial: targeted recheck `T2-M5-BEH-R1-01` found the remaining Task Lifecycle ambiguity, which the later two-path repair closes.                                                       |
| `T2-M5-EVID-01-F3`  | Accepted and fixed in the grouped-repair candidate and tree.                                                                                                                                                             |
| `T2-M5-EVID-01-F1`  | The receipt-completeness gap was accepted; the canonical candidate-bound receipt below closes it in the later evidence carrier. The stale-lock-timestamp subclaim remained rejected as unsupported and a false positive. |
| `T2-M5-EVID-01-F2`  | Accepted; the canonical receipt below closes it by separating historical cached scope from reproduced immutable-range proof.                                                                                             |

Related non-duplicates were `T2-M5-ARCH-01-F03` ↔
`T2-M5-EVID-01-F1` for permission contract versus enforcement evidence, and
`T2-M5-ARCH-01-F01` ↔ `T2-M5-BEH-01-F1` for coordinator intake shape versus
ordinary-path compatibility. The joined wave contained no duplicates or
dissent.

#### Targeted Recheck `T2-M5-R1`

All recheck topics inspected candidate
`87f2acb961750dc06f9fd574d82edbe2ac0a6f46`, tree
`17fdf3c8fb4f57ecc26221b87c3650b969e09e2f`, over parent, ancestor, and
correction authority `08d2746fd490257d595210276aa1ce5f831783d3`, tree
`2f1402106e7e8dfedeb786dbbf4d78c76eea3e43`.

| Recheck unit       | Result and lineage                                                                                                                                                                                                                                                                  |
| ------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `T2-M5-ARCH-R1-01` | `TOPIC_PASS`: original `T2-M5-ARCH-01-F01`, `T2-M5-ARCH-01-F02`, and `T2-M5-ARCH-01-F03` were fixed with no new finding or regression.                                                                                                                                              |
| `T2-M5-BEH-R1-01`  | Original `T2-M5-BEH-01-F1` was partially fixed. New finding `T2-M5-BEH-R1-01-F1` (`BEH-R1-F1`) was accepted for the Task Lifecycle Step 7 ordinary/coordinated ambiguity and is fixed by the later two-path repair.                                                                 |
| `T2-M5-EVID-R1-01` | Original `T2-M5-EVID-01-F1` and `T2-M5-EVID-01-F2` were not fixed in the inspected candidate because canonical candidate-bound receipts were absent. `T2-M5-EVID-01-F3` was fixed, with no new evidence-regression ID. The receipt below closes the two original evidence findings. |

The recheck preserved the non-duplicate links
`T2-M5-ARCH-01-F03` ↔ `T2-M5-EVID-01-F1` and
`T2-M5-ARCH-01-F01` ↔ `T2-M5-BEH-01-F1`; it reported no duplicates or
dissent.

#### Canonical Receipt For Candidate `87f2acb`

This receipt was reproduced with local and offline controls before the later
two-path evidence/behavior repair. It attests only the inspected candidate and
range below.

Identity and configuration:

- portable working-root identity: repository root of the named Git worktree
  `review-orchestration-workflow`, with `HEAD` at the candidate commit and tree
  recorded below;
- candidate `87f2acb961750dc06f9fd574d82edbe2ac0a6f46`, tree
  `17fdf3c8fb4f57ecc26221b87c3650b969e09e2f`;
- parent, ancestor, and correction authority
  `08d2746fd490257d595210276aa1ce5f831783d3`, tree
  `2f1402106e7e8dfedeb786dbbf4d78c76eea3e43`;
- range path-manifest SHA-256
  `b0a76145ac827d56e3c54dd1b25e937725224d6737467b1665d097bb51a3bb49`
  and binary-diff SHA-256
  `0ef552a9c7b344eb6b7d0f220e00a05b43d06c23b2d50f0ec3dd4b5333fd816e`;
- `apm.yml` blob `7cb4b65603c2a7490853dfc094384acc7b7b3a4c`, authored
  `.apm/agents` tree `23fb8ef12e28a5743fe1b491c7b2884d48666028`, and
  lock blob `888f1479d4cd93084404e1b6c7c7bba08c9de279` with SHA-256
  `e6729b3530769ca7239fca993622050e8acf947fff3256238e0d46f4dcb75afd`;
- model patcher blob `9703eee4ab3592b54056c159fc39b3e69e1fec93` with
  SHA-256 `2f5b75bee586c397806f08d39734e02115b92d34d2cf1d1d2cfb8fb920596cf2`;
- formatting configuration blobs: `package.json`
  `c2ed655ce20a427d3468a79c809d3dc4a0d5e564`, `bun.lock`
  `b4b167cbbb27c733435e0d2b2650675ff6a53e1c`, `.prettierrc.json`
  `2576c35b5c02c13a7530c0fa055fe187a9328b96`, and `.prettierignore`
  `98639cd86577c5d6c6020beb4d88f535b9fcde48`.

Toolchain identities were Git 2.43.0, APM 0.28.0 (`e041462`), Node
v24.19.0, Prettier 3.9.6, and Open Forge launcher SHA-256
`20613244603f6c1399c99b842dcdba929996723418c63d5a847da289c542d27a`.

The immutable static checks were:

```sh
git merge-base --is-ancestor 08d2746fd490257d595210276aa1ce5f831783d3 87f2acb961750dc06f9fd574d82edbe2ac0a6f46
git diff --check 08d2746fd490257d595210276aa1ce5f831783d3 87f2acb961750dc06f9fd574d82edbe2ac0a6f46
git diff --name-only 08d2746fd490257d595210276aa1ce5f831783d3 87f2acb961750dc06f9fd574d82edbe2ac0a6f46
git diff --name-only 08d2746fd490257d595210276aa1ce5f831783d3 87f2acb961750dc06f9fd574d82edbe2ac0a6f46 -- .agents/directives/csharp ':(glob)**/*.cs' ':(glob)**/*.csproj' ':(glob)**/*.sln' .apm/agents/reviewer.agent.md .apm/agents/reviewer-terra.agent.md .apm/agents/improvement-reviewer.agent.md .apm/agents/writing-reviewer.agent.md apm.yml package.json scripts/patch-codex-agent-models.ts .codex/config.toml src/open-forge .agents/templates/memory/plan.md .agents/templates/memory/project-status.md .agents/templates/memory/checkpoint.md | wc -l
sha256sum .agents/directives/csharp/_csharp.md .agents/directives/csharp/design.md .agents/directives/csharp/style.md
```

All commands exited 0 with zero failures, skips, or warnings. Selected,
discovered, and executed test counts are not applicable to static Git checks.
The range contained exactly these 18 paths:

- `.agents/directives/review-evidence.md`
- `.agents/memory/emerging/ideas/continuous-targeted-review-orchestration.md`
- `.agents/memory/emerging/ideas/review-orchestration-trial-controls.md`
- `.agents/templates/memory/project-control-ledger.md`
- `.agents/templates/memory/task.md`
- `.agents/workflows/development/task-lifecycle.md`
- `.agents/workflows/program-development.md`
- `.agents/workflows/review.md`
- `.agents/workflows/worktree-program-development.md`
- `.apm/agents/overseer.agent.md`
- `.apm/agents/review-mastermind.agent.md`
- `.apm/agents/task-mastermind.agent.md`
- `.apm/agents/topics/architecture-ownership-refactoring.agent.md`
- `.apm/agents/topics/behavior-contracts.agent.md`
- `.apm/agents/topics/csharp-conformance.agent.md`
- `.apm/agents/topics/test-evidence.agent.md`
- `apm.lock.yaml`
- `src/extensions/orchestration/README.md`

The protected-path result was zero. The C# SHA-256 fingerprints were
`31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`,
`76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`,
and `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`
for `_csharp.md`, `design.md`, and `style.md`, respectively. The protected
negative assertion is limited to the explicit pathspecs in the command.

Cached and immutable static scopes remain distinct. The candidate commit records
that cached validation passed for its exact 18-path correction, but the original
precommit invocation was not reproduced after commit and is not restated as a
new command. Running a cached check against the now-empty index would prove only
that empty index. The reproduced parent-to-candidate checks above are the
authoritative immutable proof.

Open Forge freshness used:

```sh
open-forge index
rg -l 'open-forge:generated-index:start' .agents --glob '*.md' | sort | wc -l
rg -l 'open-forge:generated-index:start' .agents --glob '*.md' | sort | xargs sha256sum | sha256sum
open-forge doctor --json
```

All commands exited 0 with zero failures or skips. Indexing left tracked state
clean and covered 129 generated-index sources with combined SHA-256
`ccc7cad85424c2f13772b2ac5f44d2cb8c51fe9be018cc15ebd9d27bb6ad9659`.
Doctor reported zero errors and the one known pre-existing mixed-Axioms warning
in the C# Directive entrypoint; selected, discovered, and executed test counts
are not applicable. No new warning occurred.

APM and projection validation used:

```sh
apm compile --validate --local-only
apm install --frozen --no-policy --no-audit --parallel-downloads 0 --target opencode,codex
node --experimental-strip-types scripts/patch-codex-agent-models.ts
node --experimental-strip-types scripts/patch-codex-agent-models.ts --check
find .apm/agents -type f -name '*.agent.md' | wc -l
find .codex/agents -type f -name '*.toml' | wc -l
find .codex/agents -type f -name '*.toml' -print0 | sort -z | xargs -0 sha256sum | sha256sum
find .opencode/agents -type f -name '*.md' | wc -l
find .opencode/agents -type f -name '*.md' -print0 | sort -z | xargs -0 sha256sum | sha256sum
rg -c '^- kind: project-relative$' apm.lock.yaml
sha256sum apm.lock.yaml
```

All commands exited 0 with zero failures or skips. Compile validated all 27
authored agents. Frozen installation integrated and adopted 27 Codex and 27
OpenCode projections and verified 54 lock deployments. The model
patcher patched and then checked 27 of 27 configured Codex agents. Fresh artifact
assertions independently returned 27 authored `.agent.md` sources, 27 Codex
`.toml` projections, 27 OpenCode `.md` projections, and 54 lock deployment
records. Their exact artifact identities were Codex manifest SHA-256
`4a807c698e99c8fdb8a21472a88125d97085f845e7019fb2f09a8648e813d20b`
and OpenCode manifest SHA-256
`36d7e6b19118143429d5698345841fb02e22ce19420eb99786ee2b06924f1b2f`;
the lock assertion returned SHA-256
`e6729b3530769ca7239fca993622050e8acf947fff3256238e0d46f4dcb75afd`.
Each count and hash assertion exited 0 with zero failures, skips, or warnings;
selected, discovered, and executed test counts are not applicable to artifact
assertions. The expected `--no-policy` notice was the only APM notice. No network
or policy discovery was observed. These checks were limited to local compile
and frozen generated-projection consistency; audit, cross-runtime permission
enforcement, and product tests were not claimed.

Formatting used Prettier 3.9.6 through this exact command:

```sh
bunx --no-install prettier --check .agents/directives/review-evidence.md .agents/memory/emerging/ideas/continuous-targeted-review-orchestration.md .agents/memory/emerging/ideas/review-orchestration-trial-controls.md .agents/templates/memory/project-control-ledger.md .agents/templates/memory/task.md .agents/workflows/development/task-lifecycle.md .agents/workflows/program-development.md .agents/workflows/review.md .agents/workflows/worktree-program-development.md .apm/agents/overseer.agent.md .apm/agents/review-mastermind.agent.md .apm/agents/task-mastermind.agent.md .apm/agents/topics/architecture-ownership-refactoring.agent.md .apm/agents/topics/behavior-contracts.agent.md .apm/agents/topics/csharp-conformance.agent.md .apm/agents/topics/test-evidence.agent.md src/extensions/orchestration/README.md
```

It matched 17 of 17 selected authored Markdown files, with zero failures or
skips and exit 0. The launcher warned, "Using a stale installation of prettier
because --no-install was passed"; this was not a warning-free pass. The resolved
binary was the repository's exact Prettier 3.9.6. Generated lock content is not
a Prettier target. No tests apply to this prose and agent-configuration range.

The lock-timestamp subclaim remains rejected. Parent lock blob
`5942ac62dde56a0653b7d28b62d1d526608f8b7a` had SHA-256
`d73010793ff5aea9b9881d7e179ee32db1012eda5daf3da97e43306133f82638`;
the candidate lock blob and hash changed as recorded above, with 20 inserted and
20 removed lines. Frozen generation passed while APM retained the same
generator-owned `generated_at` value. The timestamp was not hand-edited and is
not evidence that the changed lock was stale.

This canonical receipt does not self-attest its later evidence/behavior carrier.
The receipt binds candidate `87f2acb` and tree `17fdf3c8` only. The carrier's own
two-path diff receives a targeted live gate packet and final holistic review,
not another self-referential stored hash.

### Holistic Integrity Correction `T2-M6-HOLISTIC-01-F01`

`T2-M6-HOLISTIC-01-F01` was accepted and is fixed and closed. The holistic
review found that the five trial roles' named Git-family Bash allows still
admitted caller-selected Git options. An intermediate exact-command Bash allow
remained unsafe because OpenCode's parsed permission boundary could omit
surrounding redirection nodes.

The first correction, commit
`e182a1ce7ef3b40941638cab41b82ef041122e4e`, tree
`c51ac0af7ae9aeb776a13fbc4db0d66241f928fc`, denied Bash completely and exposed
one structured OpenCode custom tool backed by the fixed repository Git-object
engine. The final lazy-fetch correction, commit
`fa281c0440a7927a149b9119e4cb1303f47cadd9`, tree
`48b3e00b9dce9ef560a91aa773b8e2441f1d281d`, disabled Git lazy fetches and added
an offline partial-clone regression. The targeted correction recheck returned
`PASS` after inspecting the final two-path immutable range and accepting the
supplied live receipt: seven focused tests and 103 assertions plus strict
TypeScript, ESLint, Prettier, diff, protected-path, and C# authority checks.

Holistic unit `T2-M6-HOLISTIC-01` was consumed by the review. This later prose
carrier records the immutable correction evidence but does not attest its own
tree. No recursive carrier hash is required.

## Unverified Trial Questions

### Task 15 Status Seam Runtime Observation

The first Task 15 production seam tested two standing topic roles against
immutable commit `44104f03f1d596b47f395eeb7798b974fb88877c`, tree
`71a19f8563a5c4c5e3b99fd8dd223a0849c79617`, while the original Brilliant
Implementer continued later Green work. The test-evidence and
architecture-ownership-refactoring roles both returned a topic gap before
semantic inspection because the active Codex runtime did not expose their
required `inspect-git-objects` tool and their authored policy forbids ordinary
filesystem or Git fallback. They consumed no semantic review unit and produced
no finding.

The supervising Task Mastermind correctly treated this as a review-transport
gap rather than an implementation or agent failure. It replaced the two roles
with one Luna/max general Reviewer and one Sol/xhigh Challenger that have normal
read-only Git and filesystem authority over the same immutable commit. This
preserved the implementation owner and avoided blocking Green.

This sample strengthens the existing cross-runtime caveat: topic-role routing
must preflight the named inspection capability, or the packet must carry the
bounded immutable content that the role can inspect. Until that condition is
met, the standing topic files are not a reliable default in Codex. A prompt
cannot compensate for a deliberately denied inspection surface.

The following remain experimental:

- automatic or per-file fan-out rather than explicit coherent checkpoints;
- a fifth workflow and repository topic instead of coordinator validation;
- dirty or untracked snapshot transport when a coherent commit is unavailable;
- cross-runtime permission enforcement and exact collection, closure, queue,
  capacity-refusal, and hot-role-reload guarantees;
- any hard concurrency number independent of current runtime evidence;
- optimal reviewer economics, metrics, topic combinations, and comparative
  yield; and
- packaging the coordinator and topics as an Extension.

Promote only the smallest rules supported by the trial. Narrow or reject the
coordinator if it blurs writer authority, loses dissent, applies findings to the
wrong lineage, creates review storms, or produces mostly duplicate, preference,
or false-positive findings.

## Related Sources

- [Continuous Targeted Review Orchestration](continuous-targeted-review-orchestration.md)
- [Review Evidence](../../../directives/review-evidence.md)
- [Hierarchical Project Orchestration](../../../directives/hierarchical-orchestration.md)
- [Adaptive Development](../../../workflows/adaptive-development.md)
- [Review Workflow](../../../workflows/review.md)
- [Managed Worktree Project Development](../../../workflows/worktree-program-development.md)
