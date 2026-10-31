---
open-forge:
  description: Full proposal for the Task 58 eval product from council round 5, covering location, run protocol, grading per layer, Helpful scoring, sample sizes, the results repository and site, documents and delivery phases, waiting for the maintainer's approval
  tags: [Memory, Analysis, Contextual, Candidate, Evaluation, Council, Harness, Grading, Results, Proposal]
---

# Eval Product Proposal

The Eval Product Proposal is the complete plan for the [Task 58](../../working/cli-development/tasks/task58-demo-evals.md) evals. It combines Claude's harness and the Codex pilot into one product under `src/evals/`, runs the four layers through one protocol, grades them as deterministically as each layer allows and publishes results on their own site.

It builds on [Eval Council Round 4](eval-council-round4.md) and the maintainer's answers of 2026-11-01. Five Codex `gpt-6.1-sol` members at `xhigh` each wrote one part: grading, product, protocol, results and delivery. Claude merged them, settled where they differed and checked the run counts against the first 25-run batch. The same members then reviewed the merge, and their corrections are in.

Status: approved by the maintainer on 2026-11-01, with the changes under [Approved](#approved). This record keeps the proposal as it was approved. The accepted parts move into the [Evals](../../crystallized/documents/evals/_evals.md) documents as they're built.

## Goal and claims

The evals show whether Open Forge is at least not worse than no setup, and whether it keeps its edge as models improve. They run on Claude Code and Codex subscriptions.

- Claim nothing about Open Forge until the evidence holds. Every published CLI stays beta until then.
- Layer 1 can show that adding Open Forge stays within registered margins on this task and model: the share of stated checks passed is at most 10 points lower, and coding tokens are at most 20% higher. Whole-game success is reported, not claimed. Showing that Open Forge is a good addition needs a measured benefit, which the later layers add.
- Results are compared within one tool and model. When the model changes, Open Forge and the linked `AGENTS.md` control run again together.

## Where the work lives

- **Branch.** `feature/demo-evals`, the branch Claude works on now. The pilot's files exist only as untracked files on `feature/eval-tooling-pilot`, from the same starting commit, so they come over by an inventory with file hashes, not a merge. The inventory leaves out the pilot's run folders, which hold sign-in caches, and its local results. The branch is first brought up to date with `develop`.
- **Product.** `src/evals/`, next to the CLI and the docs. The current `evals/` folder and the pilot's `src/mvp/eval-tooling/` are removed once the new commands work and their saved results are imported.
- **Results.** A new repository added as a submodule at `src/evals/results/`, described under [Results](#results-repository-and-site).

```text
src/evals/
  cli.ts                  one command for every step
  experiments/            saved plans: cases, setups, models, limits, versions
  execution/
    containers/           Docker stages, file copies, cleanup
    providers/            Codex and Claude sessions, sign-in, telemetry
    conversation/         the message protocol and the simulated user
  materialize/            every setup generated from one source
  grading/                check runner, judge, rubrics, append-only assessments
  benchmarks/neon-pairs/
    specs/                frozen spec pack, generated from the Open Forge documents
    reference/            the reference game
    requirements.json     requirement clauses, their source and first layer
    checks.json           each check, its clauses, method and expected result
    layers/               L1.json to L4.json, the frozen check sets
    cases/                Layer 4 feature and repair cases
    fixtures/             board codes, snapshots, replays, broken copies
    trace.json            rule to code to check
    legacy/               the frozen G1 inputs and checks
  history/                schema-versioned records and export
  web/                    local server, blind comparison and results UI
  images/                 agent, verifier and assessor Dockerfiles
  results/                the results repository, as a submodule
```

## Product

One command from the repository root: `npm run evals -- <command>`.

| Command | Does |
| --- | --- |
| `doctor` | Checks Docker, Node, the tools and sign-ins |
| `images` | Builds the three images from locked digests |
| `smoke` | Runs a mock attempt end to end without a model |
| `qualify` | Checks that each setup delivers what it should, outside the scored sample |
| `run` | Runs a saved plan, for example `run --plan experiments/neon-pairs-l1.json` |
| `ui` | Opens the local blind comparison and results, with no model calls |
| `recheck`, `rejudge` | Grade saved attempts again by appending an assessment |
| `publish` | Prepares a finished batch for the results repository |

- **Language.** Strict ESM TypeScript run by Node 24.19.0, the version the docs workflow uses, with type checking as its own step and tests through `node --test`. The pilot's `.mjs` modules are ported. The UI keeps Claude's React, Vite and shadcn components.
- **Images.** `agent` has the pinned Codex and Claude tools, a fresh home per attempt and only the chosen provider's sign-in. `verifier` is Playwright `1.64.1-noble` with no credentials and no network after install, building as an account that can't read grading files. `assessor` runs the simulated user and the judge, each in a fresh instance, with model tools turned off.
- **Isolation.** The pilot's rules stay: no host mounts, no Docker socket, no published ports, files moved by Docker copy. One attempt at a time by default, up to five after concurrency is tested, and browser checks always one at a time.
- **Blind comparison.** Each submitted game plays on its own origin in a sandboxed frame, so games can't read each other's storage or the UI. Setup names stay hidden until Reveal.
- **Promptfoo** is left out. The pilot's runner is ported into the modules above, taking over Promptfoo's scheduling, cancellation and accounting of every attempt, so Promptfoo would only be a second viewer and a second archive format. Two members suggested keeping it as an optional export. It can be added later if someone wants its viewer.

## Run protocol

Every attempt at every layer follows the same steps:

1. Restore the frozen inputs into a fresh agent container and tool home.
2. Send one task prompt.
3. Answer questions through the simulated user, in the same session.
4. Stop at completion or the registered limit, then freeze the files.
5. Run the verifier, then the judge. The agent never sees any grading.

**Simulated user.** It's available at every layer and runs in the assessor container. A fixed classifier finds what each question asks about, and the answer comes from a frozen answer bank for that case. A choice the bank doesn't cover gets "Your call, within the supplied requirements." It gives no coding advice and volunteers nothing. It allows three question rounds and counts each decision in a bundled message as its own question. A question after the third answer gets the fallback answer and is recorded as unanswered. If it fails, the attempt records an infrastructure error, never "no question".

| Layer | The simulated user knows | Grading |
| --- | --- | --- |
| 1, an idea | The prompt and frozen private preferences | Playability and the stated request |
| 2, a defined idea | The supplied spec subset and answers for declared open choices | Layer 1 plus every supplied requirement |
| 3, a full spec | The full spec and nothing private | Every executable requirement |
| 4, brownfield | The project rules, the request and the visible symptoms | No regressions plus the change itself |

At Layer 2 the answer bank never reveals the rest of Layer 3. At Layer 4 the user never knows where a defect is, how to fix it or what the private checks test.

**Judge.** A pinned judge runs after every attempt, including usable partial ones. Both providers get the same evidence: the filtered source without setup files, generated output or test reports, the neutral spec for the layer, the change request at Layer 4, and the same screenshots and face sheets for questions about the look. It answers frozen yes, no or unsure questions with cited evidence, three ballots each. Rules, code, wishes and planning are scored apart. A judge failure never lowers a check result. Collaboration is judged from the transcript apart from the code, because the transcript can reveal the setup. People audit every unsure answer and a registered random sample, blind.

## Grading per layer

Each layer keeps the previous layer's checks and adds stricter ones as more is stated. Every check returns `pass`, `fail`, `error` or `blocked` with evidence. A grader error is never a game failure, and neither counts as a pass. Each rule is split into clauses, and a clause passes only when all its assertions pass. Before a check set is frozen, it must pass on the reference and fail on known broken copies. A retry keeps the first outcome.

**Counting.** Each check counts once, and coverage is the checks passed out of all applicable checks:

- A `blocked` check counts as not passed, because an earlier failure stopped it.
- A grader `error` is graded again on the saved files, without a new agent run. If it stays unresolved, it counts as not passed and the count is reported.
- An attempt that fails, times out or builds nothing stays in, with what it passed.
- An infrastructure failure, such as a container or simulated user failure, is replaced by one new attempt under the registered rule, and both are recorded.
- Missing token counts are reported as missing in the token result, never dropped silently.

| Layer | Checks | Passing | Judge questions |
| --- | --- | --- | --- |
| 1 | 6 working checks, 16 stated checks, 3 wishes apart | "Kind of worked" is the 6. Stated success is all 16 | About 8 |
| 2 | About 48: Layer 1's 16 plus about 32 from the supplied clauses | Every supplied clause | About 10 |
| 3 | About 160 assertions covering every clause of the 94 requirement IDs | Every executable clause, with review clauses reported apart | About 12 |
| 4 | The full Layer 3 suite, 10 to 20 change checks and 3 guards | The change works and no earlier check fails | Layer 3's plus 4 |

- **Layer 1** uses grading version `L1-v2`: the pilot's 16 stated checks and 3 wishes, with the check fixes Claude validated. The working checks are: the built page opens offline, a 4x4 game finishes by pointer, by phone tap and by keyboard, the win is shown, and New Game clears the board and moves, all without an uncaught error. Starting size, remembered size and no sound stay in the wishes. Asking is scored in the collaboration result. Earlier G1 records keep their own grading version.
- **Layer 2** adds the player-facing clauses: sizes, saved size, mismatch closing, exact moves, best scores, reset, keyboard movement, accessible names, the stated look and delivery. Board codes, snapshots and replay stay out. Controlled clocks test the stated timing. Like Layer 1, it finds cards through visible play and never needs a hook the docs didn't ask for.
- **Layer 3** uses board codes for known deals, snapshots for states and the replay command for exact moves, plus static checks for packages, compiler settings, sizes and source rules. Architecture and legibility stay review questions, and an unsure review leaves compliance pending.
- **Layer 4** first confirms the reference passes and, for a repair, that the planted defect fails its checks. The guards check that grader inputs weren't touched, that compatibility holds and that the diff stays within the task. A case can replace an earlier expectation only through a published requirement change.

Every layer records tokens, tool calls, failed tool calls, time, peak context, compactions, Open Forge reads and user effort, with simulated user and judge usage apart.

## Questions and the Helpful setup

- Each case pre-registers its question opportunities: the choices where asking is the right move.
- An opportunity scores when the agent asked, got the answer and used it. The number of questions earns nothing. A question the documents already answer counts as avoidable user effort.
- A stated assumption counts as planning only when it names a relevant choice before the affected work, stays within what the user left open and matches the result. A summary at the end earns nothing.
- The Helpful setup gets its own planning result. A useful question or a relevant stated assumption shows planning. If neither occurs, it records a planning failure, which counts against Open Forge. Zero questions always records zero question-driven help, even when assumptions show planning.
- This result sits beside correctness. A working game can still show no Helpful benefit. If Helpful keeps failing it, the [Plan-Ahead Skill](../ideas/plan-ahead-skill.md) idea is the next step.

## Fair comparison

- Within a comparison, the prompt, project facts, document descriptions, instruction authority, starting code and answer bank are the same. Answers differ only because agents ask different things.
- Every setup is generated from one source, and every extra instruction is registered. `qualify` checks each setup's entry files and Extension files before scored runs.
- The round-4 fixes apply: the manifest leaves the agent's folder, eval wording leaves the specs, linked `AGENTS.md` gets the same descriptions as Entries, `applyTo` leaves greenfield, and every setup starts as a Git repository with one commit.
- Runs go in balanced random order with the same model, effort, tools, network, resources and limits. Reads are recorded without reminders, and not using Open Forge stays in the main result.
- Helpful is compared with its parent preset apart from the routing comparisons.

## How many runs

The members proposed a 5-point success margin and a 10% token margin with about 50 runs per setup. The first 25-run batch shows that those margins need far more runs. The table gives the runs per setup needed to show "no worse than the margin" with one-sided 95% confidence and 80% power, from that batch's spread:

| Measure | Spread in the batch | 5 points or 10% | 10 points or 20% |
| --- | --- | --- | --- |
| All stated checks pass | 32% passed | About 1,080 | About 270 |
| Share of stated checks passed | 19 points | About 185 | About 47 |
| Tokens | 42% | About 240 | About 66 |
| Tool calls | 26% | About 94 | About 26 |

Five runs per setup and the check gaps found since make these spreads rough. Containers and frozen checks may shrink them, but the plan doesn't assume it. The proposal:

- Screen with five runs per setup and model, outside the claim.
- The first claim needs both results to pass against No setup: the share of stated checks passed within 10 points, and the geometric mean of coding tokens per attempt within 20%.
- The primary comparison is Essentials, the recommended preset, against No setup. The other presets are secondary, with a Holm adjustment.
- Plan for about 66 runs per setup, what the token result needs. The final number comes from the spread measured in qualification, by the registered method, before scored runs.
- Report whole-game success and the working gate as descriptive results.
- Tighten the margins later when the measured spread allows.
- Pre-register inputs, grading, setups, answer banks, question opportunities, judge settings, limits, order, sample size, margins, methods and wording before scored runs. Publish inconclusive results, and never add runs because a result nearly passes.

## Results repository and site

A new repository, `open-forge-eval-results`, holds the published records, their evidence manifests and a Docusaurus site of its own. It's added to Open Forge as a submodule at `src/evals/results/`. The runner, specs and local UI stay in `src/evals/`.

- **Site.** The results site publishes to GitHub Pages from its own `main` on every push, so new results need neither a CLI release nor a docs release. Open Forge's docs only publish when `main` changes, so they link to it under "Evaluation results". The submodule pins a reviewed snapshot for local use, and publishing doesn't need a new pin.
- **Versions.** Each result is addressed by CLI version, batch and run, for example `0.9.0-beta.18`, then the batch, then the run. Benchmark, grading, runner, model and schema versions are recorded apart. An unreleased CLI gets a candidate label with its source and package hashes. No-setup runs record the CLI version they're compared with.
- **Layout.** `schemas/v2/`, `benchmarks/<input-hash>/manifest.json`, `versions/<cli-version>/batches/<batch>/` with `experiment.json`, `setups.jsonl`, `runs.jsonl`, `assessments/` and `artifacts.json`, then `site/` and `scripts/`.
- **Records.** Schema v2 has five record types: experiment, setup, run, assessment and artifact. JSONL is the source, validated by JSON Schema. CSV, chart data and tables are generated. Missing values stay `null`, and failures, interruptions and retries stay in. A blind pick is an assessment of two attempts: both artifact hashes, which side each was on, the pick or a tie, and whether the setups had been revealed.
- **Git or release assets.** Records, schemas, manifests, site source and lockfiles are committed. Agent source, built games, screenshots, transcripts and tooling snapshots go in compressed, checksummed release assets, one immutable release per batch, split at 1 GiB. Credentials and machine paths are removed on export. A correction is a new release linked to the old one.
- **Pages.** Overview, CLI history with the model fixed, model history with Open Forge and its control side by side, batch comparisons with intervals and single attempts, and run details with checks, judge evidence, usage and downloads. Correctness, rules, cost, wishes and preferences stay in separate panels, and regressions and inconclusive results are shown.
- **Publishing.** `npm run evals -- publish --batch <batch>` checks the batch is complete, freezes and sanitizes its records and bundles its files. Staging opens a results change with a draft release. CI validates schemas, references and comparison groups. Merging verifies the uploaded asset hashes, publishes the release and rebuilds the site. A failed check or deploy leaves the previous site up. The site shows the data commit and analysis version behind each chart. Each publish changes a remote repository, so it runs only with the maintainer's go.
- **Local runs** stay in a gitignored `src/evals/runs/` until published.

Rejected: importing the results components through the submodule, because every new batch would need a docs release. Fetching results from GitHub on every visit, because availability, caching and schema changes would land in every page. An npm package, because it adds releases and still needs consumers to update. Release assets alone, because they can't be searched or compared.

## Task 90

[Task 90](../../working/cli-development/tasks/task90-extensions-install-their-routes.md) affects the Helpful setup. Screening runs may use the manual route setup, labeled as a workaround. Scored Helpful runs wait for a CLI build in which installing the Extension adds its routes, checked on Minimal, Essentials and Full Core. A route the setup excluded on purpose makes it an invalid setup, never silently missing Guidance. Nothing else waits for Task 90.

## Open Forge documents

After approval, the accepted parts move into `.agents/memory/crystallized/documents/evals/`. They use repository-relative links and no identifiers or user paths. Run, batch and machine identifiers live only in result records.

| Document | Answers |
| --- | --- |
| `vision.md` | What the evals must show, what we may claim, the four layers and their axes |
| `architecture.md` | The product structure, containers, isolation and stage contracts |
| `protocol.md` | The run steps, the simulated user, fair comparison and pre-registration |
| `grading.md` | Checks per layer, the judge, statuses and the Helpful planning result. Replaces `judging.md` |
| `data-points.md` | What is measured and how it's counted |
| `running-evals.md` | Commands, prerequisites, sign-in and recovery |
| `results.md` | The results repository, versions, schema and publishing |
| `neon-pairs/` | The game's spec pack and its cases per layer. Absorbs `greenfield-baseline.md` |

The frozen specs under `src/evals/benchmarks/neon-pairs/specs/` are generated from `neon-pairs/` and hashed, so there is one editable source. The [Eval Ladder](../ideas/eval-ladder.md) and [Baseline Lessons](../ideas/baseline-lessons.md) feed `vision.md` and `neon-pairs/`.

## Delivery phases

Sizes are rough worker-days, without subscription waits or review.

| Phase | Task | Size | Needs |
| --- | --- | --- | --- |
| 1, Consolidation | Contracts and documents: configuration, stage outcomes, result schema, document scopes | 1 to 2 | Approval |
| | Source import: both efforts by inventory, package scripts, a reader for saved results, a model-free `smoke` | 1 to 2 | Contracts |
| 2, Reliable execution | Results and replay: functional success apart from pipeline completion, append-only grades, separate fingerprints captured at batch start | 2 to 3 | Phase 1 |
| | Provider execution: instructions load for both providers, equal judge evidence, browser checks one at a time, telemetry, scheduling, cancellation and accounting of every attempt | 2 to 3 | Phase 1 |
| 3, Fixture correction | Setup generation with the round-4 fixes | 2 | Phase 1 |
| | Reference qualification: spec contradictions and reference breaches fixed, every clause mapped to evidence, mutation checks | 3 to 5 | The corrected spec |
| 4, Layers | Conversation and judge at every layer | 2 to 3 | Phase 2 |
| | Layer adapters: `L1-v2`, then Layer 3, then Layer 4 feature and repair cases, then Layer 2 and the richer-context Layer 4 | 3 to 4 | Phases 2 and 3 |
| 5, Results | Viewer: blind comparison and charts on saved files, isolated game origins, the pick record | 2 | Phase 4 |
| | Archive and publication: export, the results repository, its site, staged evidence and the publish flow | 2 to 3 | Phase 2 |
| 6, Qualification | Clean clones on each claimed platform, both providers, two concurrent attempts with refresh and interruption, then five screening runs per setup | 1 to 2, plus runs | Phases 4 and 5 |

About 21 to 31 worker-days in all. Phase 3 can run beside phase 2 once phase 1 is done. Phase 5 makes publishing possible and stages evidence, and new comparisons are published only after qualification. Each phase has focused unit tests and a real Docker smoke run, covering isolation between attempts, instruction loading, cancellation, failed-attempt accounting, append-only grades, secret exclusion and hidden setup names. After approval, each row becomes a worker task under Task 58 with exact files, dependencies, acceptance checks and rollback.

## Approved

On 2026-11-01 the maintainer approved the proposal and the recommended answer to each question below, with these changes:

- **Helpful and Extensions wait.** Testing Extensions, the Helpful setup among them, is a later part of Task 58. The Helpful planning result, the Plan-Ahead Skill idea and the dependency on Task 90 wait with it. The current setups are No setup and the three presets.
- **Five runs per setup.** Each case runs five times per setup, model and tool. That screens and compares, but it can't support the margins under [How many runs](#how-many-runs), so no claim is made from it and the CLI stays beta.
- **Layer by layer.** First the product, production ready, with Layer 1 tested end to end. Then the documents for Layers 2, 3 and 4.
- **Layer 4 starts from our own solution.** A council builds a reference solution to the full Layer 3 spec, and that solution seeds Layer 4. Code from a Layer 3 run isn't used.
- **The results repository is public**, and Claude may create it.

The questions as asked, with the recommended answers the maintainer accepted:

1. **Margins.** Both must pass: the share of stated checks passed within 10 points, and tokens within 20%, with Essentials against No setup as the primary comparison. Plan about 66 runs per setup, fixed from qualification before scored runs. The alternative, 5 points and 10%, needs about 185 to 240 runs per setup.
2. **Stated assumptions.** A relevant assumption stated before the work counts as Helpful planning, with zero question-driven help still recorded apart. The alternative is that only questions count.
3. **Results repository.** It holds the results, evidence and site, and the runner stays in `src/evals/`. Its name is `open-forge-eval-results`.
4. **Layer 2 scope.** The player-facing clauses without board codes, snapshots or replay, then freeze the exact clause list.
5. **Promptfoo.** Left out, with an export added only if wanted.

Back to [Analysis](_analysis.md).
