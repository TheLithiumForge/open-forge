---
open-forge:
  description: Plan for the next evals from the November 2026 concept, with Layer 4 and Layer 3 scenarios, chained milestones, orchestrated runs, context composition, cleanup, docs, phases and open decisions, waiting for the maintainer's approval
  tags: [Memory, Analysis, Contextual, Candidate, Evaluation, Plan, Brownfield, Orchestration, Milestones, Tokens, Scopes, Proposal]
---

# Eval Program Plan

## How do the next evals get from the concept to results without going in circles?

Status: drafted on 2026-11-03 by a planning pass over the [concept](eval-program-concept.md), the harness, the saved Sol records and the docs. The maintainer approved the scenarios, the dropped items and all five decisions the same day. The objective measures moved before the full Layer 4 batch, so the smoke runs prove the data is there. Agent runs wait for Phase 0 to be committed.

The base can carry the plan. The circles came from changing several things inside one batch. So the first Layer 4 batch needs no new harness code, and each later phase adds one mechanism, proven by a check without models and a one-run smoke test before any three-run batch. Everything runs on Codex Sol at high effort.

## Costs from saved Sol runs

Means per run over three runs, from the local batches under `src/evals/runs`:

| Case | Setup | New input | Cached input | Output | Agent time |
| --- | --- | --- | --- | --- | --- |
| l4-r01, repair | Plain docs | 58K | 1.54M | 5.9K | 6.8 min |
| l4-r01 | Full Core | 62K | 1.93M | 6.1K | 6.4 min |
| l4-r01 | Code only | 42K | 0.95M | 4.8K | 5.8 min |
| l4-f01, feature | Plain docs and Full Core | 63K and 89K | 1.64M and 3.95M | 12K and 16K | 8 to 13 min |
| l3-full-spec | Plain docs and Full Core | 62K and 79K | 2.96M and 3.29M | 46K and 42K | about 20 min |

A check takes about 20 minutes per slot.

## Context steps

The concept's four steps map onto existing setups. All ten spec documents already link to each other, so Plain docs is step 2.

1. **Code only.** The reference project alone.
2. **Plain docs.** Code with the linked specs. The control.
3. **Full Core.** Open Forge with the specs moved one to one, `index` run and every document loaded up front.
4. **Full Core, scoped.** Open Forge with the specs routed by scope.

Code only is an information ablation, so it is compared only with Plain docs, as [Layer 4](../../crystallized/documents/evals/neon-pairs/layer-4.md) requires.

## Layer 4 scenarios

| ID | Scenario | Start | Tasks and milestones | Steps | Way of working | Question | Runs per cell | Tokens |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| L4-1 | Scoped tasks | Reference plus one registered defect | Four separate one-milestone tasks: r02 best score (Gameplay), r03 arrow keys (Controls and input), r04 reduced motion (Visuals), r05 silence (Sound) | 1, 2, 3, 4 | One agent | Does each step change correctness and tokens on small work that needs one scope? | 3 | 48 runs, about 75M, of which about 3M new input and 0.3M output |
| L4-2 | Additive chain | M1 reuses L4-1's r02 runs. Each later milestone starts from its own lineage's result plus the next defect. | M1 r02, M2 r03, M3 r04, M4 r05, each a fresh session with the case's request | 2, 3, 4 | One agent | Does scoping keep its saving across tasks on one codebase? Do earlier changes and Open Forge records help or hurt the next task? Each milestone is compared with the same task's standalone L4-1 runs. | 3 lineages | 27 new runs, about 45M |
| L4-3 | Four tasks at once | Reference plus all four defects | One request listing the four symptoms, worked one at a time with tests after each, graded once per task group | 2, 4 | One agent, and an orchestrator handing each task to a subagent | What does an orchestrator with subagents cost against one agent for the same work, and does scoping lower the subagents' cost? | 3 | 12 runs, about 70M, a guess until the smoke run |

L4-3's four cells are the concept's four ways of working: one agent, one agent with Open Forge, an orchestrator without Open Forge and an orchestrator with Open Forge.

Dropped:

- **Rerunning l4-f01, l4-r01 and l4-fr01.** Every setup with docs reaches 99 to 100% coverage there, so they can't separate setups on correctness. Their batches stay as data.
- **Linked AGENTS.md, Minimal and Essentials in new batches.** They aren't context steps. Their data stays.
- **Code only in L4-2 and L4-3.** L4-1 already answers what the docs add.
- **Task Coordination as a fifth L4-3 cell.** One cause at a time. It can follow.
- **Task-split runs in one session.** The chain answers the milestone question with fewer moving parts.

## Layer 3 scenarios

| ID | Scenario | Milestones | Steps | Way of working | Question | Runs per cell | Tokens |
| --- | --- | --- | --- | --- | --- | --- | --- |
| L3-1 | Full build | One | 2, 3, 4 | One agent | Does scoping cost nothing on a build that needs every document? | 3 | 9 runs, about 29M |
| L3-2 | Milestone build | M1 a playable game (Rules, Controls and input, Technology, Sound), M2 Visuals, M3 Persistence and sharing, each a fresh session continuing its lineage, graded after each | 2, 3, 4 | One agent | With milestones, does scoping spend fewer tokens at the same final coverage as L3-1? | 3 lineages | 27 runs, about 45M |
| L3-3 | Orchestrated build | The orchestrator splits the build | 2, 4 | One agent and orchestrated | L4-3's question at greenfield scale | 3 | 12 runs, about 50M, after L4-3 proves the mechanism |

M1 must include Controls and input, because browser checks find cards by the Input document's names.

Layers 2 and 1 get no new runs. They have no full spec, so scoping has little to disclose. The 2026-11-02 Sol batches stay as their baseline.

## Incremental design

Setups already come from one source, `materializeSetup` in `src/evals/materialize/setup.ts`, and a registered start costs no tokens. What's missing is reuse of agent work.

| | Continue from the lineage's own result | Start from a shared checkpoint |
| --- | --- | --- |
| Realism | Earlier edits, Open Forge records and drift carry over | Every milestone starts clean |
| Variance | Builds up over milestones | Independent per milestone |
| Reuse | Earlier runs become starts at no extra tokens | Earlier runs aren't reused |
| Grading | Inherited failures must be separated | Clean result per milestone |
| Accepted rules | Amends `layer-4.md`: "Every attempt starts from the same audited solution or a registered defect copy of it." | Complies |

Recommendation: a shared registered start for every standalone task, every first milestone and L4-3, and the lineage's own result for later milestones. Each chained milestone's twin is the same task's standalone run, and the difference is the effect of accumulated state.

A chained start:

1. Takes the parent run's saved files without what the reference's `.gitignore` ignores, keeping `.agents`, `docs`, `AGENTS.md` and `.open-forge-state`.
2. Applies the next case's `seed.patch`. The four seeds touch separate files. A seed that won't apply ends the lineage as a seed conflict, which is a finding about out-of-area edits.
3. Creates a fresh repository with one commit and a start manifest the scope guard compares against.
4. Starts only if the parent's agent finished and its build passed. Otherwise the child is recorded as not started.
5. Pairs runs by setup and repeat, one child per parent.

Each milestone is its own batch. The batch records its chain and milestone, and each run records its parent.

Grading per milestone: the case's clause checks, every other full-spec check as regression, then the scope and protected-input guards. From saved assessments, without another checker run: inherited failures, new regressions and edits outside the files the case's `solution.patch` touches. L4-3 grades once, with a group tag per task. L3-2 runs the full checker after every milestone and reports coverage for the scopes due so far, using `requirements.json`. Grading never blocks the next milestone, only the build does.

## Orchestration design

Subagents are threads of the same CLI process inside the run's one agent container, sharing `/work`, network rules and limits.

- **Codex 0.166.0** has `multi_agent` features, an `[agents]` table with `max_depth`, `max_concurrent_threads_per_session` and the subagent model and effort, and the `spawn_agent`, `wait_agent`, `send_input` and `close_agent` tools. The harness sets none of them today. Confirm with `codex features list` in the agent image.
- **Claude Code 2.1.314** has the Agent tool, and its telemetry counts helper messages. Claude runs are paused.

The way of working becomes a setup variant: `plain-docs-orchestrated` and `full-core-scoped-orchestrated` in `BrownfieldSetups`, with one optional `workMode` on `SetupDefinition`. Batches, `--setups`, records and Results cells then work unchanged. One agent runs with `--disable multi_agent`. Orchestrated runs enable it with depth 1, one subagent at a time, and the implementer's model and effort for every thread.

Registered instruction lines, appended after the request:

- **Both ways:** "Work through these tasks one at a time, and run the project's tests after each one."
- **Orchestrated adds:** "Plan the work first. Then hand each task to its own subagent with the context it needs, and don't edit the code yourself."

Tokens: today they come from the main thread's `turn.completed` events in `codex-telemetry.ts`. The saved rollout holds every thread, so the new reader splits it by each thread's `session_meta`, sums each thread's requests and records `telemetry.threads` with ID, parent, role, tokens, requests and peak context. `tokens` becomes the system total. The smoke run checks nothing is counted twice. Reports show system tokens against one agent, the orchestrator's share, each subagent's tokens and files read by more than one thread. Both ways share the request, files, answers, model, effort, time limit and checks. The four-task case probably needs a 90-minute limit.

Task Coordination (`src/extensions/orchestration`) is a method, not a runtime, and is usable later. Hierarchical orchestration and Worker Watch need worktrees, a daemon and other workers, so they don't fit the eval container.

## Context composition

Today `history/usage-analysis.ts` classifies reads with one regex over the whole command, so one `cat` of `AGENTS.md` and every spec counted 40,183 characters as framework. It skips Codex `file_change` items, so Codex never gets a first-edit point. No saved run has a rollout yet.

Classes: up-front text, spec documents, framework files, code, searches and listings, test and build output, other commands, the agent's own output, user answers, and for orchestration, delegation briefs and subagent reports.

Method, per thread:

1. The first request's input is the up-front class.
2. Each later request's input growth, less the previous output, splits across the new tool outputs by character share.
3. Each output is classified by its command, with paths resolved against the start manifest. A multi-file `cat` splits by file size, `sed -n` by line range and `rg` by path prefix.
4. Compactions reset the count and are reported.

Two views: context at peak, and input cost by class, where each item's tokens multiply by the requests that resend it. The second sums exactly to new plus cached input, which checks the attribution. An `analyze` stage writes a versioned record per run in `assessments/analysis.jsonl`. Totals are exact and the split is labelled estimated, because characters stand in for tokens, reasoning is hidden and pipelines have no known size.

## Cleanup

What is messy:

1. **Record types exist three times**, in the harness, the results repository and the site, with a copied `schemas/v2`.
2. **Legacy layering.** `ExperimentRecord` extends `LegacyBatchRecord`, `RunView` re-adds legacy fields, `BatchRecord` aliases `ExperimentRecord`, and `ResultFiles.batch` still names `batch.json`.
3. **Names.** `EvalConfig.suite` holds a case ID, plans use `case` and `cases`, and batch and experiment mean the same thing.
4. **Layer dispatch by the `l4-` ID prefix** in three files.
5. **Setups in three shapes**, plus site labels.
6. **`run.json` duplicates grading** beside `assessments/checks.jsonl`, and `gates.ts` writes a separate `brownfield.json`.
7. **Plans don't say what ran.** Layer 4 plans name Claude, but every Layer 4 batch ran Sol through `--implementers`, and two config files carry settings plans override.
8. **Leftovers.** Temporary `experiment.json` files, the retired app's intent files and packages, Claude batches left running or paused, and `experiments/` outside the typecheck, which hides a test passing services `CliServices` no longer has.
9. **The measurement bugs** above.
10. **No fields** for parents, milestones or threads.

Process rule from here: one mechanism per phase, commit before every batch, plan files state exactly what runs, and every batch is reviewed before the next phase.

Before the first Layer 4 run: commit the branch, pass the r02 to r05 validations, fix Codex edit detection, typecheck `experiments/`, remove the retired packages and make the tasks plans honest (Codex Sol, Code only added, judges as decided).

Before L4-2 and L4-3: optional v2 fields for parent, milestone, `workMode` and `telemetry.threads`, start manifests per run, case composition with check groups, and the `analyze` stage.

Can wait: one generated source for record types, dropping legacy types, the suite and case rename, one setup registry, folding `summary` into `progress` and `usage`, removing the config files, and adding local batches to the results repository.

Grading speed, raised by the maintainer on 2026-11-03: one project version is graded in one container with 2 CPU cores, and the checker runs its browser journeys one after another. Each journey opens a fresh browser session, so they are independent. Validations and rechecks can already run in parallel, one container each. Splitting one version's journeys across several containers would shorten each run's grading, keeping timing-sensitive journeys such as performance and clock checks apart. It changes how grading runs, so a checker validation must show identical results first. In a full batch, five check slots already fill about 10 of 12 cores, so it speeds single runs and validations more than whole batches.

Direction from the maintainer the same day: "Each container should take as much resources are available. And it's internals should use all of the available ones." Containers have no fixed core cap and size to the machine, and the checker splits its journeys across the cores it gets. The maintainer will also run evals in a cloud container that opens a pull request with the results, so the harness scales to whatever machine runs it. Deferred the same day at the maintainer's word: the speedup waits until real batch timings show where the time goes.

## Docs

Gaps in `src/docusaurus/docs/evals`: `index.md` says every plan uses Claude and lists three cases, `measures.mdx` lacks the token classes and has a stale judge line, `_definitions/existing-app.mdx` says three cases, "setup ladder" already names the published rungs 0 to 5, and setup counts disagree between pages. `cells.ts` keys cells by case and setup, so a chained batch would hide standalone runs unless the chain joins the key.

Needed: a Scenarios page, context steps and ways of working in `setups.mdx`, milestones and chains in `starting-points.mdx`, system tokens and context composition in `measures.mdx`, and a milestone view and orchestration comparison on the Results page.

## Phases

| Phase | Work | Validated by | Runs and tokens |
| --- | --- | --- | --- |
| 0 Base | The before-the-first-run list | `npm test`, typecheck, case validations, `qualify` on the tasks plan, `smoke` | 0 |
| 1 Spec | Approval, the `layer-4.md` amendment, registered lines, L4-1 docs | Maintainer review | 0 |
| 2 L4-1 smoke | l4-r02 on four steps, one run each | Graded without grader errors, a rollout saved per run, Codex first edit present | 4 runs, about 6M |
| 3 Objective measures | `analyze` on the smoke runs: tokens per run and task, by file type and class, and tool calls by kind | Class totals match each run's input, the smoke runs checked by hand, so missing data shows up before the full batch | 0 |
| 4 L4-1 | Tasks plan, three runs, analyzed as they finish | All runs checked and analyzed, results reviewed with the maintainer | 48 runs, about 75M |
| 5 L4-2 | Chained starts, parent fields, site key | Without models, one child start per step graded, failing only the r03 clauses | 27 runs, about 45M |
| 6 L4-3 | Combined case, groups, work modes, thread telemetry | Case validation, `codex features list`, one orchestrated smoke run counting each subagent once | 13 runs, about 77M |
| 7 L3-1 and L3-2 | Milestone prompts, coverage by scope | One-run smoke of M1 | 36 runs, about 74M |

To the first Layer 4 results: 52 runs and about 81M tokens, about 3M of them new input. All of Layer 4: about 92 runs and 200M tokens.

## Decisions

1. **Chained starts.** Later milestones start from their lineage's result, amending `layer-4.md` for chains only. Approved 2026-11-03.
2. **Step 3 and the name.** Step 3 is today's Full Core, and the four steps are called "context steps" so the published "setup ladder" keeps its meaning. Approved 2026-11-03.
3. **Judging.** `run` stops after checks and objective measures. Layer 4 scenarios skip the AI judges, which `judge --missing` can add later from saved files. Layer 3 is judged later the same way. Approved 2026-11-03. The objective measures Layer 4 needs, in the maintainer's words, are "tokens used, tokens per task, token per task per file type and so on" and tool calls: tokens per run and per task, tokens by file type and class, and tool calls by kind.
4. **The orchestrated Open Forge cell.** Full Core, scoped with the shared lines, without Task Coordination at first. Approved 2026-11-03.
5. **L4-3 size.** Four repairs give 23 clause checks, which raises the clause-only limit from 20 to 30 in `grading/brownfield/change-checks.ts`. Approved 2026-11-03.

## Agent teams

Added on 2026-11-03 at the maintainer's word. A team is part of the implementer, as a model is, so it's a selection of the data and not a scenario of its own. Any scenario can run with any team, and Results filter by team the way they filter by model. For now the teams are Sol alone, Sol leading Sol subagents and Sol leading Luna subagents, since Sol usage is plentiful. Claude counterparts come later: Opus alone, Opus leading Sonnet subagents and Opus leading Opus subagents, with more to follow. They need Claude's subagent switch and per-subagent counting first.

Reasoning levels, set by the maintainer the same day: Sol 6.1 at its usual xhigh and Luna always at max, as implementer, team member and simulated user. For Claude later, the maintainer suggested Opus at xhigh and Sonnet at high, still to confirm.

## L4-1 run of 2026-11-03

The first full L4-1 started at 18:43 with one run per cell, Sol 6.1 at xhigh and judging left for later. The maintainer then narrowed the work to L4-1 for now, so the L4-2 milestones weren't started.

| Repair | Batch | Runner |
| --- | --- | --- |
| r02 best score | `2026-11-03T18-42-54-577Z-b1ab87589ee4` | `evals-runner-5` at 74f8710aa |
| r03 arrow keys | `2026-11-03T18-43-11-226Z-9d089fb9b26f` | `evals-runner-5` |
| r04 reduced motion | `2026-11-03T18-43-25-426Z-a0c364be5337` | `evals-runner-5` |
| r05 silence | `2026-11-03T18-57-43-775Z-3722a33cd622` | `evals-runner-6` at cf40fb0e3 |

Thirty-four parallel runs, with L4-3 beside them, filled Docker's default address pools, so new stage networks failed with "all predefined address pools have been fully subnetted". Each such run got one replacement, and most replacements ran. cf40fb0e3 adds overflow subnets, but only the r05 rerun uses it.

Two cells failed and stay failed at the maintainer's word, not rerun:

- **r04 Full Core, scoped.** The run and its one replacement both failed to create their network.
- **r04 Full Core.** Codex answered "Selected model is at capacity" near the end of the agent's work, and the network then failed too. The run counts as a failed attempt, not infrastructure, so it got no replacement. It isn't the setup's fault, so read it as missing data, not a failure.

The first r05 batch, `2026-11-03T18-43-42-140Z-82ceb1aa931c`, lost five runs and their replacements the same way. It was stopped and replaced by the r05 batch above.

At the maintainer's word the evening of 2026-11-03, only best-score and silence keep grading. The arrow-keys and reduced-motion batches, L4-3 and two L4-2 milestone 2 batches were stopped with their agents done or mid-run, so `check --missing` can grade their saved files later. The two L4-2 batches, `2026-11-03T19-37-55-110Z-b558a1f36762` and `2026-11-03T19-38-31-476Z-feee7ab192f6`, started by mistake: a scratchpad chain script outlived its stop and launched twice. [eval-work-structure.md](eval-work-structure.md) proposes what replaces L4-1, L4-2 and L4-3.
