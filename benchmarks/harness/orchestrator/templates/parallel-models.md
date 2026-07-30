# Parallel Models Or Replicates Template

```text
RUN_SET = "parallel-models-example"
SCENARIO = "scenario-id"
META_SCENARIO = "meta-scenario-id"
MODELS = ["model-a", "model-b", "model-c"]
RUNTIME = "runtime-selector"
RUNTIME_TOOLS = []
REPLICATES_PER_MODEL = 1
VARIANTS = []
EXTENSIONS = []
ORCHESTRATOR_ADDENDUM = "NONE"
RUNS_ROOT = "external-runs-root"

You are the benchmark orchestrator. Read and follow
benchmarks/harness/orchestrator/orchestrator-prompt.md and
benchmarks/harness/orchestrator/runbook.md.

Treat every assignment above as a literal value. Verify that
`{META_SCENARIO}` resolves exactly to `{SCENARIO}`. Stop on a mismatch.

Create the external run set `{RUN_SET}` under `{RUNS_ROOT}`. Freeze this filled
launch prompt and one resolved worker-visible composition. Apply every path in
`{VARIANTS}`, every id in `{EXTENSIONS}`, and the review-only
`{ORCHESTRATOR_ADDENDUM}` identically to every arm.

Prepare all `{META_SCENARIO}` arms before launching any worker. Create
`{REPLICATES_PER_MODEL}` arms for every selector in `{MODELS}`. Keep the task,
workspace composition, runtime tool request `{RUNTIME_TOOLS}`, and controllable
settings identical. Verify that every prepared arm reports the same
`workerPromptSha256` and `baselineTree`. The only intended difference is the
declared model or required runtime selector.

Launch independent workers through `{RUNTIME}` in parallel when supported.
Never expose one arm's context, trace, review, or outcome to another. Record
the actual model, runtime, system-context limitation, and tool binding for each
arm rather than assuming they are identical.

Complete every arm's trace, independent review, response-only worker
self-review, and finish step before writing the run-set comparison. With one
model and multiple replicates, compare same-model variance. With multiple
models, compare observed model variance without claiming a cause from one set.
```
