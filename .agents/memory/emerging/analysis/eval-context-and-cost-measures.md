---
open-forge:
  description: Spec for measuring what Open Forge promises about context, with token, phase, context and process measures, scoped Open Forge setups and partial-work scenarios for the evals
  tags: [Memory, Analysis, Contextual, Candidate, Evaluation, Measurement, Tokens, Context, Scopes, Proposal]
---

# Eval Context And Cost Measures

## What should the evals measure to test Open Forge's promise about context?

Status: requested by the maintainer on 2026-11-02 for [Task 58](../../working/cli-development/tasks/task58-demo-evals.md). The `usage` command, Claude's real output count and Codex's saved session log are built. Everything else here is not built yet.

Open Forge promises that scoped context costs more up front and less later. A project whose documents are routed by scope lets an agent read only what a task needs. A full build of a well-specified product needs every document, so it shouldn't save tokens. Partial work, such as a bug fix or one new feature, should. The evals currently can't show either effect, because they measure only total tokens on full builds and on three existing-app cases.

## What the saved runs show

These findings come from the 2026-11-02 batches, with one to three runs per cell:

- **Nothing is scoped.** Every Open Forge setup installs the spec with `route include ./docs --tag LoadNow`, so all ten spec documents load at the start of every task. The documents are already split by topic: rules, controls, input, visuals, sound, technology, architecture, acceptance and reproducibility.
- **Cached input dominates.** A typical Sol run used 2.79 million cached input tokens, 71 thousand new input tokens and 44 thousand output tokens. Each request sends the whole context again, so the up-front context is paid on every step.
- **Open Forge setups read much more framework text.** On the short brief with Sol, they read 76 to 117 thousand characters through framework files, against 7 to 15 thousand for plain docs, and their cached input was 2.4 to 4.8 million tokens against 1.9 million.
- **Claude's output was undercounted** about 200 times until 2026-11-02, because stream events carry a message's usage from its start. The telemetry now takes output from each turn's result, and `usage` recovers it for saved runs.

## Measures

Each measure is per run, then summarized per setup and compared with the control. "Built" means `npm run evals -- usage` reports it today.

| Group | Measure | Source | State |
| --- | --- | --- | --- |
| Tokens | New input, cached input, cache writes, output and thinking | Telemetry, each turn's result | Built |
| Tokens | Cache hit rate: cached input among all input | Telemetry | Recommended |
| Tokens | Estimated cost from a versioned price table per model and token class | Telemetry and prices | Recommended |
| Phases | Input tokens before the first edit (analysis) and after it (implementation) | Claude events. Codex session log from 2026-11-02 on | Built for Claude |
| Phases | Steps before the first edit, and the share of all steps | Events | Built |
| Context given | Characters, lines and estimated tokens of the prompt, of text loaded up front and of text available on demand | The generated setup and Open Forge's startup context | Requested |
| Context read | Characters read from spec documents, framework files and code | Events | Built, approximate |
| Context read | Which spec documents were read, against the documents the task needs | Events and a per-scenario list of relevant documents | Recommended |
| Context read | Repeated reads of the same file | Events | Recommended |
| Context read | Characters and estimated tokens read and searched by file type: Markdown documents against code, such as TypeScript, HTML and CSS. Reads and searches are counted apart | Events | Requested |
| Framework use | Which framework files the agent read, such as the loader, Directives and Guidance, and whether the judge saw their rules followed | Events and judge answers | Requested |
| Context growth | Context size per request, its peak and its growth over the run | Claude events, Codex session log | Recommended |
| Process | Tool calls by kind: read, search, edit, test or build, other, and failed calls | Events | Recommended |
| Process | Test and build runs by the agent, and how many passed | Events | Recommended |
| Process | Rework: files edited many times | Events | Recommended |
| Process | Questions to the simulated user and compactions | Conversation and telemetry | Recorded, not shown |
| Output | Files added, changed and removed, and lines added and removed against the starting files | Saved files and setup manifests | Recommended |
| Output | Changes outside the requested area, for partial work | Saved files and the scenario's expected area | Recommended |
| Quality | Check coverage, gates and judge score | Checks and judges | Built |
| Ratios | Tokens per passed check and output tokens per changed line | Derived | Recommended |

Mixed reads are a known limit: one command that reads `AGENTS.md` and `docs/` counts toward framework, and `open-forge context` prints the up-front spec but counts as framework. Splitting a command's output by source would remove this limit.

## Early results, 2026-11-02

Progressive disclosure is the framework's main selling point, so these results matter mainly as a baseline for Open Forge used without scoping. The maintainer keeps the current batches as that first set and builds the scoped evals beside them.

| Starting point | What the newest batches support, at one to three runs per cell |
| --- | --- |
| An idea | Not yet usable for Claude, whose checks wait on a recheck after timeouts. Sol: coverage 94 to 100% everywhere, judge score 4 to 8 points higher with Open Forge, 17 to 26% fewer tokens |
| A short brief | Sol coverage waits on a recheck with the title fix. Claude: Essentials 7 points higher with 26% fewer tokens |
| A full spec | Sol: Essentials matches plain docs at 95% with 7% fewer tokens. Minimal and Full Core lose about 3 points with 11 to 13% more tokens |
| An existing app | Every setup reaches 100% coverage, so the checks can't separate them. Tokens: Essentials 7% fewer, Minimal 71% and Full Core 84% more |

Essentials is the cheapest Open Forge preset in every starting point so far. The existing-app checks have a ceiling: they need sharper case checks before they can show a difference in correctness.

## Scopes come first

The maintainer, 2026-11-02: scopes are as important to Open Forge as the framework itself, and everything Open Forge does is progressive disclosure. The current evals don't show that. They use Open Forge as a plain tool with an extra CLI and rules, with every document marked `LoadNow`, and that matches the baseline without beating it. Scoped Open Forge is the normal usage, so the evals and the Results page present it first. Today's setups are the baseline for unscoped use and stay as data points.

An example scope layout for Neon Pairs, to refine during the audit:

| Scope | Covers |
| --- | --- |
| Gameplay | Rules, scoring, the deal and win conditions |
| Visuals | Layout, card faces, colors and animation, the VFX |
| Sound | Sound effects and their controls, the SFX |
| Input and accessibility | Mouse, touch and keyboard input, focus and screen reader behavior |
| Persistence and sharing | Saved progress, board codes, snapshots and replay |
| Technology | Stack, build and delivery |

Each scope is a routed folder with an entrypoint whose description says when to read it, the same shape a monorepo uses for front end and back end. Only a short overview loads up front. Starting points with a full spec, Layers 3 and 4, have room for this: a full spec that is scoped well lets an agent load only the scopes a task touches.

## Scoped Open Forge setups

The maintainer wants progressive disclosure designed explicitly, since it lets a product keep changing at a lower cost. Greenfield work should show that a spec-driven agent is more correct, even at some extra token cost. Brownfield work should show that good scopes load only what a change needs.

Audit, 2026-11-02: the spec has ten documents, 571 lines and 39 KB, about 10 thousand tokens. They are already split by topic: overview, rules, controls, input, visuals, sound, technology, architecture, acceptance and reproducibility. Every Open Forge setup routes them with `--tag LoadNow`, so all ten load at the start and are sent again with every request. The feature case changes only the overview, architecture, controls and reproducibility documents. The repair case changes none of them.

The scoped variant routes the same documents without `LoadNow`, as on-demand entries whose descriptions say what each covers. Only the overview loads up front. The first scoped setup is Full Core, scoped, as the maintainer asked. Essentials, scoped follows if Full Core shows the effect, since Essentials is the cheapest preset today. Before building it, finish the audit: each document should answer one concern, link instead of repeating, and name its scope clearly enough that an agent can choose it from the description alone. Today's setups stay as the data points for Open Forge without scoping.

Expected results: about the same or slightly more tokens for a full build, fewer documents read and fewer tokens for scoped work.

## Partial-work scenarios

These scenarios sit inside the existing starting points, so they need a name of their own. The working name is scoped work: a task confined to one area of the product.

- **Greenfield parts.** Start from the reference game with one area removed, such as sound, card faces or keyboard input, give the full spec, and ask the agent to build that area. The agent must find the documents that cover it.
- **Partial builds from a full spec.** Give the whole scoped spec on Layer 3 and ask for only some scopes, such as gameplay and visuals. Check only the clauses of those scopes, which `requirements.json` maps to their documents, and measure the tokens and documents the run used.
- **Brownfield changes.** Keep the three existing-app cases and add changes confined to one scope, such as a new card back (visuals), a sound toggle (sound and controls), a keyboard focus repair (input) or a scoring rule change (rules).
- **Checks.** Each scenario needs checks sharp enough to fail a wrong change, because today's existing-app checks pass every setup. Record changes outside the scenario's area as their own measure.
- **Setups.** Plain docs, Full Core with everything up front, and Full Core, scoped.

## Task-split runs

Every run today is full auto: one request, and the agent does the whole job. Add a variant where the simulated user gives the same work as a sequence of tasks, one after another, as a person working with an agent would. Compare tokens, correctness and context growth with the full-auto run on the same setup. This also shows whether scoped context pays off over several tasks in one session.

## Prompt variants

Later, try prompts that ask the agent to read only what the task needs, and compare them with the default prompt on the same setups.

## Judging as its own stage

Today `run` judges each run right after its checks, and `judge` can grade any saved run later. Since 2026-11-02 a judge's usage limit leaves a gap instead of pausing the batch. The recommended next step, waiting for the maintainer's decision, is that `run` stops after checks and usage analysis, and `judge` becomes the only way to judge. A batch's runs then finish sooner, Claude judging can wait for usage, and every judge grades the same finished set.

## Results page

The page shows new and cached input apart, with output, and puts phase, context and process measures beside the token totals. Every measure keeps the control comparison and the model and judge filters. Measures marked approximate say so where they appear.

Cost and correctness must read together. A table shown by default puts each setup's token change beside its coverage and judge score change, with the points gained per 10% more tokens. A multi-axis chart lets the reader choose the measure on each axis, the bubble size and the color, with its data table shown under it.

## Storage

Usage measures are derived from saved evidence, so they are stored as versioned analysis records appended per run, like checks. A new analysis version can recompute every saved run without running an agent again. This needs an analysis record kind in the results schema.

## Order

1. Recheck the idea and short brief runs with the fixed checker, so the baseline is clean.
2. Finish the document audit, then add Full Core, scoped.
3. Add scoped work: greenfield parts and scoped brownfield changes, with sharper checks.
4. Store the usage measures as versioned records and measure the context each setup gives.
5. Add task-split runs.
6. Try the prompt variants.

## Open questions

- Which price table and date the cost estimate uses, since cached tokens cost far less than new ones.
- Whether a scenario's relevant documents are listed by hand or derived from the requirements a task touches.
- Whether the scoped variant changes only the routing or also splits the documents further.

Back to [Analysis](_analysis.md).
