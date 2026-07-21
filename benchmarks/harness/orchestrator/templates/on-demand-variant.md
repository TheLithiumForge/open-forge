# On-Demand Treatment Template

```text
RUN_SET = "on-demand-variant-example"
SCENARIO = "scenario-id"
BASE_META_SCENARIO = "base-meta-scenario-id"
MODEL = "model-selector"
RUNTIME = "runtime-selector"
BASE_RUNTIME_TOOLS = []
VARIANT_PATHS = []
VARIANT_REQUEST = "NONE"
EXTENSIONS = []
ADDED_RUNTIME_TOOLS = []
REPLICATES_PER_ARM = 1
ORCHESTRATOR_ADDENDUM = "NONE"
RUNS_ROOT = "external-runs-root"

You are the benchmark orchestrator. Read and follow
benchmarks/harness/orchestrator/orchestrator-prompt.md and
benchmarks/harness/orchestrator/runbook.md.

Treat every assignment above as a literal value. Verify that
`{BASE_META_SCENARIO}` resolves exactly to `{SCENARIO}`. Stop on a mismatch.

Create the external run set `{RUN_SET}` under `{RUNS_ROOT}` and preserve this
filled launch prompt. Resolve the base composition once.

When `{VARIANT_REQUEST}` is not `NONE`, create the requested additive primitive
package only under the external run-set input. Use the normal primitive package
shape and preserve the request, exact generated files, and validation output.
Append its path to the frozen treatment paths derived from `{VARIANT_PATHS}`.
Do not edit benchmark, framework, scenario, or meta-scenario sources.

A local tool treatment is a `kind: tool` primitive supplied through
`--variant`. Provider- or runtime-native additions named in
`{ADDED_RUNTIME_TOOLS}`
must be bound by `{RUNTIME}` to the treatment workers; naming them does not
provision them. Record the actual binding in each trace manifest.

Freeze the exact control and treatment compositions before launching workers.
The control uses `{BASE_META_SCENARIO}` with `{BASE_RUNTIME_TOOLS}`. The
treatment uses the same base plus every frozen variant path, every bundled id
in `{EXTENSIONS}`, and the union of `{BASE_RUNTIME_TOOLS}` with
`{ADDED_RUNTIME_TOOLS}`. Confirm that these declared additions are the only
intended deltas; treatment tools add to rather than replace the base surface.
The prepared `workerPromptSha256` values must match, and the baseline tree
difference must be explainable by the file-based treatment additions.

Prepare `{REPLICATES_PER_ARM}` control arms and the same number of treatment
arms. Apply the same `{MODEL}`, `{RUNTIME}`, and review-only
`{ORCHESTRATOR_ADDENDUM}` to both sides. Launch independent pairs in parallel
when supported without sharing context or outcomes.

Complete each arm's trace, independent review, response-only worker
self-review, and finish step before comparison. The frozen generated package,
not `{VARIANT_REQUEST}` alone, is the reproducible treatment. Reusing the exact
package repeats the treatment; regenerating it creates a new one.
```
