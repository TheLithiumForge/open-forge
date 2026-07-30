# Benchmark Runbook (orchestrator only — never install into the workspace)

## Run Sequence

1. Compose the workspace per `benchmarks/README.md` and commit the baseline.
2. Read the seed's `orchestrator/rubric.md` and this harness's `rubric-core.md` *before* spawning, so evaluation criteria are fixed in advance.
3. Spawn a fresh worker with a prompt from `worker-prompt-templates.md`. Do not paste route content into the prompt. Do not give the worker prior dogfood context.
4. While the worker runs, do not interfere. For seed-0, play the user persona per that seed's script — answer only what is asked, in character.
5. After completion: run `git status` and diff against the baseline; run the project's verification commands yourself, independently; run any seed-specific smoke checks from the seed rubric.
6. Debrief the worker with pointed questions before writing the report — self-reports gloss over substitutions and skips; ask specifically what was read, when, and what was skipped.
7. Write the run report from `report-template.md` into `benchmarks/results/` in the open-forge repo. The worker never writes this report.

## Role Boundaries

- Orchestrator: composes, observes, verifies independently, evaluates, reports.
- Worker: implements and gives a final response. A worker claiming "no subagent facility exists" while being one is a reporting error — score it.
- Persona (seed-0): the orchestrator speaking as the user. Stay in character; do not leak evaluation intent.

## Variables Discipline

Change one variable per comparison pair. Record framework commit, seed commit, model, and overlays in every report. If a seed file must change, change it in the benchmarks folder and commit — never edit seed content inside a run workspace.
