# Single Run Template

```text
RUN_SET = "single-example"
SCENARIO = "scenario-id"
META_SCENARIO = "meta-scenario-id"
MODEL = "model-selector"
RUNTIME = "runtime-selector"
RUNTIME_TOOLS = []
VARIANTS = []
EXTENSIONS = []
ORCHESTRATOR_ADDENDUM = "NONE"
RUNS_ROOT = "external-runs-root"

You are the benchmark orchestrator. Read and follow
benchmarks/harness/orchestrator/orchestrator-prompt.md and
benchmarks/harness/orchestrator/runbook.md.

Treat every assignment above as a literal value. Verify that
`{META_SCENARIO}` resolves exactly to `{SCENARIO}`. Stop on a mismatch.

Create the external run set `{RUN_SET}` under `{RUNS_ROOT}`. Preserve this
filled launch prompt and the resolved composition before worker launch.

Prepare `{META_SCENARIO}` once. Pass every path in `{VARIANTS}` through a
repeatable `--variant` option, every id in `{EXTENSIONS}` through a repeatable
`--extension` option, and `{ORCHESTRATOR_ADDENDUM}` through `--orchestrator`
only when it is not `NONE`.

Launch one independent worker with `{MODEL}` through `{RUNTIME}` and request
the runtime tool surface `{RUNTIME_TOOLS}`. The runner does not provision
provider- or runtime-native tools; record the actual binding in the trace.

Preserve the full runtime-observable trace, complete the independent arm
review, obtain the response-only worker self-review, finish the run, and write
the concise run-set result. Do not claim invisible private chain-of-thought.
```
