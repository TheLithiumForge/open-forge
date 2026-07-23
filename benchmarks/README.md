# Open Forge Benchmarks

Reusable building blocks for observing how agents follow Open Forge across tasks, compositions, treatments, and models.

## Design

The corpus has three stable layers:

```text
benchmarks/
  building-blocks/
    scenarios/       framework-agnostic task archetypes
    primitives/      small directives, patterns, memories, tools, and other inputs
  meta-scenarios/    exact reusable recipes
  harness/           preparation, recording, and orchestration support
```

A scenario owns the task. It does not select framework primitives or extensions.

A primitive is one small worker-visible input with an explicit kind. Primitives may be useful alone, while deliberate combinations may introduce ambiguity, conflict, distraction, or another trap worth observing.

A meta-scenario is an exact recipe naming one scenario, an ordered primitive set, and bundled extensions. A clean baseline and a trap are separate recipes so either can be repeated without reconstructing prior choices.

Browse the [building-block catalogue](building-blocks/README.md) and [runnable meta-scenarios](meta-scenarios/README.md) directly or through the CLI.

For a complete first run, follow the [ledger control-versus-trap tutorial](TUTORIAL.md).

## Discover And Prepare

```powershell
bun run bench -- list
bun run bench -- list meta-scenarios
bun run bench -- list scenarios
bun run bench -- list primitives
bun run bench -- prepare <meta-scenario-id> --runs-root <external-directory>
```

`list` defaults to meta-scenarios because they are the runnable stable units. A pure scenario still runs through a meta-scenario whose primitive and extension lists are empty.

Treatments may be added without editing the stable recipe:

```powershell
bun run bench -- prepare <meta-scenario-id> `
  --runs-root <external-directory> `
  --variant <external-primitive-package> `
  --variant <another-primitive-package> `
  --extension <bundled-extension-id> `
  --extension <another-extension-id> `
  --orchestrator <review-only-addendum>
```

Each repeatable `--variant` path supplies an additive primitive package. Each repeatable `--extension` id adds a bundled extension as a declared treatment. `--orchestrator` adds only observation, checking, or recordkeeping instructions and never changes the worker workspace.

`--runs-root` is required and must not contain or sit inside the source repository. This keeps prepared workers and traces outside the framework sources they evaluate.

## Run

An AI or human orchestrator prepares every comparison arm before any worker starts, then launches independent workers with the exact frozen prompt and composition. It preserves the full runtime-observable trace, reviews each arm independently, obtains the worker's response-only self-review, and finishes each run.

```powershell
bun run bench -- finish <run-directory>
```

Cross-arm comparison happens only after every arm has its own completed review. Model comparisons keep the composition frozen and vary only the declared model or runtime selector. Treatment comparisons keep the scenario and base recipe frozen and introduce only the declared primitive, extension, or runtime-tool delta.

Prepared arms expose a worker-prompt SHA-256 and baseline Git tree id. Equal model or replicate cells compare both values directly; treatment cells use them alongside the declared resolved-composition delta.

The orchestrator can retain messages, tool calls and results, interactions, status events, and reasoning summaries exposed by the runtime. It cannot claim access to invisible private chain-of-thought. The [trace contract](harness/orchestrator/trace.md) makes that boundary explicit.

## Publish

Execution and isolation remain under the external run set. After every worker context is done, publish the terminal successful, partial, or failed set with evidence once to:

```text
benchmarks/results/<UTC-filesystem-safe-date-time>/<RUN_SET>/
```

`RUN_SET` becomes the safe publication run name, and the UTC directory uses a Windows-safe form such as `YYYY-MM-DDTHH-mm-ss-SSSZ`. The [runbook](harness/orchestrator/runbook.md) owns verified same-filesystem staging and atomic publication; the [results contract](results/README.md) owns the evidence-linked `summary.md` and complete `raw/` contents.

Successful, partial, and failed sets retain their actual evidence and limits. Never overwrite a publication, and never expose one to a later arm. Publication is a terminal plain-file action; there is no CLI publish command.
Publications should not include the full raw data that might contain private information, the publication should be the conclusions of the agents and orchestrator and a summary of them and of the evidence.

## Reuse

The [launch templates](harness/orchestrator/templates/README.md) cover a single run, parallel model or replicate runs, a stable base-versus-trap pair, and an on-demand treatment pair. They are plain prompts with editable assignments, not a template language.

The CLI is optional. The same protocol remains complete as plain files: resolve an exact recipe, compose disposable workspaces, freeze all inputs outside them, run independent workers, retain traces and checks, and save the two reviews.

One run shows what happened in that run. Replication and comparisons reveal observed variance, but neither the runner nor an orchestrator assertion proves isolation or causality.

## References

- [Harness contract](harness/README.md)
- [Orchestrator runbook](harness/orchestrator/runbook.md)
- [Invariant orchestrator prompt](harness/orchestrator/orchestrator-prompt.md)
- [Worker self-review prompt](harness/orchestrator/worker-review-prompt.md)
- [Published and historical results](results/README.md)
