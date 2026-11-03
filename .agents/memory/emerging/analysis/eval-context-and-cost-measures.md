---
open-forge:
  description: Spec for measuring what Open Forge promises about context, with token, phase, context and process measures, scoped Open Forge setups and partial-work scenarios for the evals
  tags: [Memory, Analysis, Contextual, Candidate, Evaluation, Measurement, Tokens, Context, Scopes, Proposal]
---

# Eval Context And Cost Measures

## What should the evals measure to test Open Forge's promise about context?

Status: requested by the maintainer on 2026-11-02 for [Task 58](../../working/cli-development/tasks/task58-demo-evals.md). Built: the `usage` command, Claude's real output count, Codex's saved session log and, on 2026-11-03, Full Core, scoped on Layers 3 and 4, `check --missing` and `run --setups`. Everything else here is not built yet.

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

The Neon Pairs scope layout, in [`scopes.json`](../../../../src/evals/benchmarks/neon-pairs/scopes.json):

| Folder | Scope | Documents |
| --- | --- | --- |
| `gameplay` | Gameplay: the board, turns, scoring, the clock, saved sizes and best scores | Rules |
| `interface` | Controls and input, the UI scope: controls, pointer, touch, keyboard, focus and screen reader behavior | Controls, Input |
| `visuals` | Visuals, the VFX: layout, card artwork, colors, motion and zoom | Visuals |
| `sound` | Sound, the SFX: the silent game and feedback without audio | Sound |
| `sharing` | Persistence and sharing: board codes, snapshots and replay formats | Reproducibility |
| `technology` | Technology: language, packages, build, distribution and code structure | Technology, Architecture |

The overview stays at the top of `docs/` and is the only document loaded up front. The acceptance examples stay beside it, on demand.

Each scope is a routed folder with an entrypoint whose description says when to read it, the same shape a monorepo uses for front end and back end. Only a short overview loads up front. Starting points with a full spec, Layers 3 and 4, have room for this: a full spec that is scoped well lets an agent load only the scopes a task touches.

## Scoped Open Forge setups

The maintainer wants progressive disclosure designed explicitly, since it lets a product keep changing at a lower cost. Greenfield work should show that a spec-driven agent is more correct, even at some extra token cost. Brownfield work should show that good scopes load only what a change needs.

Audit, 2026-11-02: the spec has ten documents, 571 lines and 39 KB, about 10 thousand tokens. They are already split by topic: overview, rules, controls, input, visuals, sound, technology, architecture, acceptance and reproducibility. Every Open Forge setup routes them with `--tag LoadNow`, so all ten load at the start and are sent again with every request. The feature case changes only the overview, architecture, controls and reproducibility documents. The repair case changes none of them.

The scoped variant puts each document in a scope folder with its own entrypoint, decided by the maintainer on 2026-11-02, since the layout affects no other eval. Only the overview loads up front. Links between documents are rewritten to the new paths and the bodies otherwise stay the same, so the setup check compares bodies with links normalized. The first scoped setup is Full Core, scoped, as the maintainer asked. Essentials, scoped follows if Full Core shows the effect, since Essentials is the cheapest preset today. Before building it, finish the audit: each document should answer one concern, link instead of repeating, and name its scope clearly enough that an agent can choose it from the description alone. Today's setups stay as the data points for Open Forge without scoping.

Expected results: about the same or slightly more tokens for a full build, fewer documents read and fewer tokens for scoped work.

Built on 2026-11-03 for Layer 3 as the `full-core-scoped` setup. Measured on fresh installs, Full Core puts 38,969 of its 41,668 spec bytes up front, about 9,700 tokens sent again with every request. The scoped setup puts 3,931 of 44,635 up front: the overview and the catalogues. `qualify` passes it with the real CLI, and the tests prove each moved document keeps its text apart from link targets. The same day it joined the three Layer 4 cases, whose changed documents are written to the scoped paths, and passed `qualify` there too.

`open-forge index` gives a new entrypoint the placeholder description "Notes in ./docs/gameplay/_gameplay.md" on purpose, for its author to replace. The runner writes each scope's entrypoint with its real description first, as an author would.

One Open Forge bug found while building it, for the CLI's backlog. In version 0.9.0-beta.18, `index` never refreshes an existing line in a catalogue outside `.agents/`, such as an included `docs/`. After a document or child entrypoint changes its description or tags, `index` reports every Entries section as current and keeps the old line. Inside `.agents/` the same change refreshes the line. `GeneratedNavigationRegionPlanner.ReadChildren` reads every existing line of a catalogue outside the source root as an authored link and skips generating that child's line. The maintainer decided on 2026-11-03 that `index` should rewrite every entry from the current description and tags. Until then, deleting a stale line and running `index` again writes the current one.

## Partial-work scenarios

These scenarios sit inside the existing starting points, so they need a name of their own. The working name is scoped work: a task confined to one area of the product.

- **Greenfield parts.** Start from the reference game with one area removed, such as sound, card faces or keyboard input, give the full spec, and ask the agent to build that area. The agent must find the documents that cover it.
- **Partial builds from a full spec.** Give the whole scoped spec on Layer 3 and ask for only some scopes, such as gameplay and visuals. Check only the clauses of those scopes, which `requirements.json` maps to their documents, and measure the tokens and documents the run used.
- **Brownfield changes.** Keep the three existing-app cases and add changes confined to one scope, such as a new card back (visuals), a sound toggle (sound and controls), a keyboard focus repair (input) or a scoring rule change (rules).
- **Checks.** Each scenario needs checks sharp enough to fail a wrong change, because today's existing-app checks pass every setup. Record changes outside the scenario's area as their own measure.
- **Setups.** Plain docs, Full Core with everything up front, and Full Core, scoped.

### Scoped tasks, built 2026-11-03

The maintainer asked for smaller tasks that each need one or a few scopes while the rest stay on demand. Partial greenfield builds can't be graded: every browser check finds cards by the names the Input scope defines, and the deal, size and restart controls live in the Controls document. So the small tasks start from the reference game instead, as Layer 4 repairs:

| Case | Scope | Seed | Change checks |
| --- | --- | --- | --- |
| `l4-r02` Best score | Gameplay | Any finished game replaces the best, and an unrecorded size shows 0 | R10.1 to R10.7 |
| `l4-r03` Arrow keys | Controls and input | Arrow-key movement removed with its tests | I06.1 to I06.4, I06.9, I07.1, I08.2 |
| `l4-r04` Reduced motion | Visuals | The reduced-motion style rules removed | V17.1, V17.2, V18.1 to V18.3 |
| `l4-r05` Silence | Sound | A Web Audio tone on each match and the win | S01.1, S02.1 to S02.3 |

Each change check is a clause check: it takes its result from the full-spec checker's assertion for one clause, so a task needs no browser worker of its own. Those clauses leave the regression set, and every other full-spec check still runs as regression. Guard clauses that pass at the start, such as normal motion in `l4-r04`, fail a fix that removes too much. The plans are `neon-pairs-l4-tasks-check.json` and `neon-pairs-l4-tasks-full.json`, with Sol against Plain docs, Full Core and Full Core, scoped.

The Results page now keeps each setup's newest runs for a case and model, compared with their own batch's control. A later batch can then add Full Core, scoped beside its control without hiding the setups an earlier batch ran.

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

Proposed on 2026-11-03 and approved by the maintainer the same day, with small tasks that each need one or a few scopes. Those became the scoped tasks, because partial builds can't be graded. No eval starts until the improvements are built and committed.

Build first, without model usage:

1. `check --missing`, so a recheck grades only runs without a valid check. A full Layer 2 recheck took 6.7 hours, about 20 minutes a run. Built on 2026-11-03.
2. A `--setups` flag that narrows a plan's setups, so a scoped comparison runs three setups instead of six. Built on 2026-11-03. It keeps the plan's order and refuses to drop the control.
3. [Scoped tasks](#scoped-tasks-built-2026-11-03), one per scope, graded by full-spec clauses. Built on 2026-11-03.
4. Layer 4 with scopes: map each case's document changes to the scoped paths, and add Full Core, scoped to its setups. Built on 2026-11-03.
5. Make `open-forge index` rebuild every entry from the current description and tags, outside `.agents/` too.

Then grade, with the checker only:

6. `check --missing` on Sol's Layer 2 and Layer 3 batches, with the second title fix.

Then run with Sol, three runs per setup:

7. Layer 3, full build: Plain docs, Full Core and Full Core, scoped. This checks that scoping costs nothing on a full build.
8. The scoped tasks, with the same setups. Scoping should save tokens here.
9. The three earlier Layer 4 cases with `--setups plain-docs,full-core-scoped`, beside the existing batches.

Later: versioned usage records, task-split runs, prompt variants, and Claude runs when Claude usage allows.

## Open questions

- Which price table and date the cost estimate uses, since cached tokens cost far less than new ones.
- Whether a scenario's relevant documents are listed by hand or derived from the requirements a task touches.

Back to [Analysis](_analysis.md).
