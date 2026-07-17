# Run Plan - Generation 10 (orchestrator only)

Paste `orchestrator-prompt.md` first, then this plan. This generation is an A/B against the gen8/gen9 no-variables baseline; see `benchmarks/results/*gen8*`, `*gen9*`, and `.agents/memory/crystallized/documents/evaluations/v9-synthesis.md`.

## Composition Per Scenario (overrides the default sweep)

1. seed-1, seed-2, seed-3 (build mode): extend `benchmarks/harness`, the seed, and `benchmarks/variable-closeout-command`. Variables label in the report filename: `closeout-command`.
2. seed-0 (vision mode): extend `workflow-essentials` (bundled id), `benchmarks/harness`, and the seed. Variables label: `workflow-essentials`. Do not install the closeout overlay here.

Only one loader-overwrite variable per run; this plan never combines them.

## Measured Questions

- Build seeds: closeout compliance. Did the worker run `open-forge find --tag KeepInMind --bodies` before finishing, and did it then perform the follow-ups - the recheck, a session record, and an observation where a grounded finding existed? Score first-attempt behavior. Add one comparison line per report against the roughly 50% gen8/gen9 closeout baseline.
- seed-0: discovery depth with the richer vision skills installed. Does the worker ask the recall-window question (Monday covers Friday plus weekend) and resist the time-tracking curveball? Two vision workflows will be present (`worker-vision.md` from the harness and the workflow-essentials vision workflow); record which one the worker selects and whether it reads that workflow's Required Routes - the choice and the loading are both signal.

## Debrief Additions

- Build seeds: ask exactly when the closeout command was run, what it printed, and what the worker did with the output; ask for the command invocation verbatim.
- seed-0: ask which vision workflow was selected and why, and whether the vision skill references (clarify-intent, fit-and-risk) were opened before questioning.

## Provenance Note

The source repo intentionally carries staged, uncommitted changes during this generation; record the framework commit plus "staged changes present" as prior generations did. Reports use only workspace-relative paths.
