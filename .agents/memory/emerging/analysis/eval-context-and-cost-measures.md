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

## Scoped Open Forge setups

Add a scoped variant beside today's everything-up-front Open Forge setups. In the scoped variant, the spec documents are on-demand routed entries with clear descriptions and tags, and only a short overview loads up front. Before building it, audit the Neon Pairs documents: each should answer one concern, link instead of repeating, and be small enough to load alone. The maintainer suspects they aren't well scoped yet. Today's setups stay as data points for Open Forge used without scoping.

Expected results: about the same tokens for a full build, more reading up front, fewer tokens and fewer documents read for partial work.

## Partial-work scenarios

Add tasks that touch one part of the game, such as sound only, card faces only or keyboard controls only. Each starts from the reference game with that part removed and gives the full spec, so the agent must find the relevant part. Run each against plain docs, up-front Open Forge and scoped Open Forge. The existing-app cases, a feature, a repair and both, are partial work already and gain a scoped variant too.

## Prompt variants

Later, try prompts that ask the agent to read only what the task needs, and compare them with the default prompt on the same setups.

## Results page

The page shows new and cached input apart, with output, and puts phase, context and process measures beside the token totals. Every measure keeps the control comparison and the model and judge filters. Measures marked approximate say so where they appear.

## Storage

Usage measures are derived from saved evidence, so they are stored as versioned analysis records appended per run, like checks. A new analysis version can recompute every saved run without running an agent again. This needs an analysis record kind in the results schema.

## Order

1. Record the recommended measures that saved events already support, and store them as versioned records.
2. Show token classes, phases and context reads on the Results page.
3. Measure the context each setup gives, up front and on demand.
4. Audit the Neon Pairs documents, then add the scoped setups.
5. Add the partial-work scenarios.
6. Try the prompt variants.

## Open questions

- Which price table and date the cost estimate uses, since cached tokens cost far less than new ones.
- Whether a scenario's relevant documents are listed by hand or derived from the requirements a task touches.
- Whether the scoped variant changes only the routing or also splits the documents further.

Back to [Analysis](_analysis.md).
