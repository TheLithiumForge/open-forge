# Open Forge Benchmarks

Reproducible dogfood scenarios for testing how agents behave under Open Forge, across framework versions, models, and variables.

These are local extension packages. They are versioned with the framework but deliberately excluded from the npm package — they exist for testing, not distribution.

## Design Rules

- **Fixed seeds, variable overlays.** A seed's content never changes between comparison runs. Anything you want to vary (tooling, loader hints, extra routes) is a separate overlay package applied on top. This keeps A/B runs honest.
- **Worker payload vs orchestrator materials.** Each package's `payload/` is the only content installed into the test workspace — it is everything the worker is allowed to see. Files under `orchestrator/` (personas, scripts, rubrics, prompt templates) are read by the person or agent running the benchmark and must never be copied into the workspace. This is contamination control: a worker that can read its own evaluation rubric is not being tested.
- **One baseline commit before the worker starts.** Everything the worker changes must be visible in `git status` afterward.
- **The worker prompt stays minimal.** Point at `AGENTS.md`, state the run mode, nothing else. The whole point is testing whether the routes carry the context.

## Composition Recipe

```powershell
mkdir <target>
open-forge install <target>
open-forge extend <extension-pack-if-testing-one> <target>   # e.g. workflow-essentials
open-forge extend .\benchmarks\harness <target>
open-forge extend .\benchmarks\seed-<N>-... <target>
open-forge extend .\benchmarks\variable-... <target>   # optional overlays
open-forge index <target>
cd <target> ; git init ; git add -A ; git commit -m "Benchmark baseline: <seed> + <variables>"
```

Then follow `harness/orchestrator/runbook.md`. To start an orchestrator session, use the copy-paste prompt in `harness/orchestrator/orchestrator-prompt.md`.

## Levels

- `seed-0-greenfield` — no product truth at all. The orchestrator role-plays a user with a fuzzy idea, following a staged conversation script; the worker must drive vision, tradeoffs, and MVP boundary, and grow memory from nothing. Tests: vision/architecture workflows, clarify-intent quality, asking-vs-assuming, memory creation from scratch, promotion-with-confirmation. Two run modes: vision-only, or vision-then-build.
- `seed-1-rebuild-small` — rebuild after a small MVP (bookmarks CLI). Compact seeded truth: vision with behavior contract, rebuild brief, 5 decisions, analyses, observations, patterns. Tests: basic routing fidelity, directive compliance, whether seeded semantics reach the implementation.
- `seed-2-rebuild-medium` — rebuild after a medium MVP (standup work-journal CLI). Same domain family that seed-0's conversation converges toward, so greenfield-conceived and seeded-rebuild outcomes can be compared. Richer decision surface: dates/timezones, append-only storage, immutable corrections, id schemes. Tests: selective loading among more routes than any one step needs, following non-obvious seeded opinions over instinct.
- `seed-3-rebuild-large` — rebuild after a larger MVP (ledger: pure core + HTTP API server + CLI client, three packages). Opinions about everything: money handling, API error contracts, import rules, security posture, per-package scoped pattern routes, and **one deliberately planted spec contradiction** (documented only in the orchestrator rubric) that a diligent worker should surface rather than silently resolve. Tests: routing at scale, scope routes (first coverage), narrower-scope precedence, conflict reporting, token discipline. Still an afternoon for a strong agent.

## Variables

- `variable-dump-tool` — adds `tools/forge-dump.mjs` plus a `loader.overwrite.md` announcing it (also the first real test of the overwrite mechanism). Compare context-loading behavior with vs without. Superseded as a product feature by `open-forge find`; kept as a historical variable.
- `variable-closeout-command` — a `loader.overwrite.md` announcing `open-forge find --tag KeepInMind --bodies` as the one-command closeout recheck. Compare closeout compliance against the gen8/gen9 no-variables baseline. Install only one loader-overwrite variable per run; they share `loader.overwrite.md`.
- `variable-sessions-keepinmind` — replaces the sessions entrypoint with a #KeepInMind-tagged version carrying an explicit "write a session record before ending meaningful work" axiom. Tests the gen10 hypothesis that the closeout printout is the compliance surface; combine with `variable-closeout-command` (different files, no collision) against the gen10 closeout-command-only control.
- Model — same seed, different worker model (Fable vs Sonnet vs codex/GPT). The seed makes this a controlled comparison.
- Framework version — same seed, different `open-forge` commit. This is the main regression harness for future framework changes.
- Extension packs — with vs without `workflow-essentials`, etc.

## Recording Results

One markdown file per run under `benchmarks/results/`, named `<date>-<seed>-<model>-<generation>-<variables>.md`, containing: framework commit, seed folder + repo commit, model, variables applied, rubric scores from the harness rubric plus the seed's own rubric, and a short narrative. Keep results committed — they are the longitudinal value of this folder.

### Summary

What now exists under benchmarks/ in the open-forge repo
benchmarks/README.md — the runbook: composition recipe (install → extend harness → extend seed-N → optional variable overlays → index → baseline commit), the variables matrix, and a results-log convention (benchmarks/results/<date>-<seed>-<model>-<variables>.md, committed — that's the longitudinal value).

harness/ — the shared, product-agnostic machinery, split along the v6 lesson: payload/ (worker-visible: scope-control, code-safety, evidence-first-closeout directives + a generic worker workflow with report-ownership rules) vs orchestrator/ (never installed: runbook, minimal worker-prompt templates, debrief question set, core rubric, report template). The worker physically cannot read its own evaluation criteria anymore — that's the structural fix for both v6 role-boundary failures.

Four seeds, one ladder:

seed-0-greenfield — your best idea, built out fully: no product truth at all; the orchestrator plays "Alex" from a persona fact-sheet (reveal only what's asked), through a staged script with a planted curveball (time-tracking Alex doesn't really want) and a boundary push (git integration). Rubric scores question coverage, scope defense, tradeoff quality, and — the core — whether accepted vision/decisions/non-goals get written to memory with user confirmation before promotion, well enough that a cold build session needs zero re-asks. Deliberately in the same domain as seed-2, so greenfield-conceived vs seeded-rebuild outcomes are directly comparable.
seed-1-rebuild-small — the existing validated seed, migrated with my review fixes applied (fictional MVP handoff added, runtime off-PATH premise corrected, orchestrator-facing content stripped from worker payload, meta-README replaced).
seed-2-rebuild-medium — new: standup, a work-journal CLI whose entire difficulty is dates (Monday-covers-Friday-plus-weekend, UTC-storage/local-resolution, DST edges) plus non-obvious seeded opinions that fight agent instinct: append-only NDJSON, immutable corrections (amend appends, never edits), no delete command, hand-rolled sortable ids, no date libraries.
seed-3-rebuild-large — new: ledger (pure core + node:http API server + thin CLI client, three packages). ~25 routes of opinions about everything: integer-cents money, uniform error contract, idempotent client-id POSTs, all-or-nothing CSV import, localhost-only security. First-ever coverage of scope routes (per-package pattern folders — validated indexing correctly). And one deliberately planted contradiction — the vision requires ledger remove <id>, the immutable-ledger decision forbids hard deletes — both tagged CurrentTruth, documented only in the orchestrator rubric. Whether a worker surfaces that conflict unprompted is the single most important score in the seed, and it's exactly your "AI should ask when things are not OK" requirement made measurable.
variable-dump-tool/ — the forge-dump tool as a clean overlay, announced via loader.overwrite.md — which doubles as the first real test of the overwrite mechanism.

All compositions verified end-to-end: every seed installs via the real open-forge extend, indexes cleanly, scope routes appear, the fictional handoff routes, and the dump tool runs against the composed tree. Everything lives in benchmarks/ (versioned with the framework, outside the npm files allow-list, so nothing ships to users).

Improvements on the original ask worth flagging: the harness/orchestrator split (contamination control — workers can't read rubrics or personas); fixed-seeds-vs-variable-overlays as a hard rule so A/B runs stay honest; planted traps with pre-committed scoring (seed-0's curveball, seed-3's contradiction) so "docility vs judgment" is scored, not vibes; the seed-0↔seed-2 same-domain pairing; and per-seed "agent temptations" tuned to each project's specific instinct-vs-recorded-truth tensions. When you next change the framework, the regression test is now one recipe: same seed, same model, new framework commit, diff the rubric scores.
