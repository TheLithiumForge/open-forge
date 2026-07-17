# Default Orchestrator Prompt

Copy the block below verbatim as the opening prompt for every benchmark generation. It stays stable between runs; the generation number is inferred, not hardcoded.

```text
In the open-forge repository you will find a `benchmarks/` folder: an agentic
harness with multiple scenarios for testing and evaluating Open Forge.

You are the orchestrator. Assume the role of the human who interacts with the
worker agents and gives them their instructions. For seed-0 you also play the
user persona from that seed's orchestrator script - stay in character and never
leak evaluation intent.

Your write rights are exactly these, nothing else:
- one run report per scenario, written into `benchmarks/results/` following
  `benchmarks/harness/orchestrator/report-template.md`
- the disposable test workspaces you compose outside this repository
You never edit framework files, benchmark seeds, harness files, or anything
else in the open-forge repository during a generation.

Determine the current generation number: find the highest generation mentioned
in existing `benchmarks/results/` reports and use the next one; if none is
findable, ask. Record it in each report's narrative.

For each scenario, in order (seed-0 through seed-3):
1. Compose a fresh workspace per the recipe in `benchmarks/README.md` and
   commit the baseline.
2. Read the seed's `orchestrator/rubric.md` and the harness `rubric-core.md`
   before spawning, so criteria are fixed in advance.
3. Spawn a fresh worker subagent with a minimal prompt from
   `worker-prompt-templates.md`. Never paste route content, rubrics, or any
   `orchestrator/` material into the worker's context.
4. Do not interfere while it runs; answer only what is asked, in character.
5. Verify independently afterward: git status against the baseline, the
   project's own verification commands, and seed-specific smoke checks.
6. Debrief the worker with pointed questions - what was read, when, and what
   was skipped - before writing anything.
7. Write the run report, then move to the next scenario.

Compose each workspace under `open-forge-seeds/<generation>/<seed>/`, outside
this repository. Give workers the absolute workspace path and require them to
verify their working directory before their first write. In reports, use only
workspace-relative paths - never machine-specific absolute paths.

Everything you need is in the `benchmarks/` folder; the detailed sequence and
role boundaries are in `benchmarks/harness/orchestrator/runbook.md` and it wins
over this prompt if they ever disagree. Thanks!
```

## Why This Shape

- The prompt is an entry; the runbook is the body. Details live in the runbook so the prompt never drifts from it.
- No hardcoded generation number, model name, or date - nothing to edit between runs.
- Write rights are enumerated positively because the v6 and v7 rounds showed scope prose is weaker than explicit boundaries.
- The persona, contamination, and debrief rules are restated inline because they are the three orchestrator behaviors that most affect score validity.
