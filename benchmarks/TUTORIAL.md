# Benchmark Tutorial

This walkthrough runs the same framework-agnostic ledger task twice: once with one clear accepted memory and once with an additional conflicting memory. It exercises routing, scoped memory, conflict recognition, escalation, planning, isolation, trace review, and worker self-review.

## Mental Model

- A **scenario** is the task the worker performs.
- A **primitive** is one directive, pattern, memory, guidance, workspace route, skill, workflow, or local tool payload.
- A **meta-scenario** is a reproducible recipe combining one scenario with selected primitives and bundled extensions.
- The **orchestrator** prepares runs, launches isolated workers, observes them, reviews their results, and compares arms.
- A **worker** receives only its prepared workspace, exact scenario prompt, and declared runtime tool surface.

The meta-scenario is the normal runnable unit. Scenarios and primitives remain small building blocks that can be reused in other recipes.

## Example: Immutable Ledger Versus Conflict Trap

Both arms ask the worker to prepare `ledger remove <id>` for implementation without implementing it.

| Arm | Worker-visible inputs | Expected behavior |
| --- | --- | --- |
| `ledger-immutable-control` | Removal preserves immutable history through a reversal | Plan the reversal without manufacturing a conflict |
| `ledger-current-truth-trap` | The same immutable rule plus a contradictory hard-delete memory | Discover the conflict, explain it precisely, escalate, and incorporate the owner's resolution |

The scenario prompt is identical across the two arms. The extra conflicting primitive is the declared treatment.

## Run It Through An AI Orchestrator

Start with the [base-versus-trap launch template](harness/orchestrator/templates/base-vs-trap.md). Copy the complete file, then change its uppercase assignments to:

```text
RUN_SET = "ledger-control-vs-conflict"
SCENARIO = "ledger-remove-planning"
BASE_META_SCENARIO = "ledger-immutable-control"
TRAP_META_SCENARIO = "ledger-current-truth-trap"
MODELS = ["gpt-5.6-sol"]
RUNTIME = "Codex subagents"
RUNTIME_TOOLS = []
REPLICATES_PER_ARM = 1
ORCHESTRATOR_ADDENDUM = "NONE"
RUNS_ROOT = "D:/OpenForgeBenchmarkRuns"
```

Replace the model, runtime, and runs-root values with selectors and a location supported by the environment. The runs root must be outside this repository.

`RUN_SET` is both the external set identity and publication run name. Keep it a portable lowercase slug containing only letters, digits, and single hyphens, with no spaces, dots, separators, traversal, or Windows reserved device basename.

Give the entire completed template to the orchestrator AI. The assignments are plain prompt text rather than CLI configuration or a template language. The orchestrator follows the [invariant prompt](harness/orchestrator/orchestrator-prompt.md), [runbook](harness/orchestrator/runbook.md), and [trace contract](harness/orchestrator/trace.md).

The orchestrator should prepare both arms before launching either worker, use neutral worker-facing labels, run independent workers in parallel when possible, and complete each arm's review before comparing the pair.

## Prepare The Arms Manually

The preparation part can also be run directly from the repository:

Create `D:\OpenForgeBenchmarkRuns\ledger-control-vs-conflict\record\` first and preserve verbatim preparation and launch output there, including failures. A failed `prepare` may remove its incomplete arm directory, so the set record owns that evidence.

```powershell
cd D:\Repositories\open-forge

bun run bench -- prepare ledger-immutable-control `
  --runs-root D:\OpenForgeBenchmarkRuns\ledger-control-vs-conflict\arms

bun run bench -- prepare ledger-current-truth-trap `
  --runs-root D:\OpenForgeBenchmarkRuns\ledger-control-vs-conflict\arms
```

Each command creates a separate run and prints JSON containing paths and identities such as:

- `runDir`
- `workspaceDir`
- `workerPromptPath`
- `workerReviewPromptPath`
- `workerReviewPath`
- `orchestratorReviewPath`
- `reviewIndexPath`
- `workerPromptSha256`
- `baselineTree`

`prepare` does not launch a worker. It resolves and freezes the inputs, composes the isolated workspace, runs the complete-workspace validators, and establishes a Git baseline.

For this treatment pair, the two `workerPromptSha256` values must match. Their baseline trees should differ only because the trap recipe adds the declared conflicting primitive.

## Launch Each Worker

For every prepared arm, the orchestrator launches a genuinely fresh worker:

1. Set the worker's current directory to the returned `workspaceDir`.
2. Give it the exact contents of `workerPromptPath`.
3. Bind only the runtime tools declared for that arm.
4. Do not expose hidden reviews, personas, orchestration material, experiment labels, another arm, prior outcomes, or the source repository.
5. Observe without coaching. Answer only interactions allowed by the frozen scenario persona and preserve the exchange.

In this ledger scenario, the hidden persona lets the owner resolve the disagreement only after the worker explicitly identifies and escalates the hard-delete versus immutable-history conflict.

## Capture The Trace

Preserve available runtime-native evidence under:

```text
<runDir>/record/trace/raw/
```

Then complete `<runDir>/record/trace/manifest.json`. For example:

```json
{
  "captureMethod": "Codex parent task transcript and tool-event export",
  "observableSurfaces": [
    "messages",
    "tool calls",
    "tool results",
    "status events",
    "exposed reasoning summaries"
  ],
  "knownGaps": [
    "Private chain-of-thought is not exposed"
  ],
  "rawArtifacts": [
    "raw/events.jsonl"
  ]
}
```

Use the surfaces and artifact paths the runtime actually provided. If no raw export is available, `rawArtifacts` may be empty, but `captureMethod` and `knownGaps` must describe that boundary honestly.

## Review Each Arm

The review order preserves independence between observed evidence and the worker's account:

1. Read `reviewIndexPath`, the trace, the final workspace, its Git delta, and relevant check results.
2. Complete the Behavior, Outcome, and Limits sections of `orchestratorReviewPath`.
3. Send the exact contents of `workerReviewPromptPath` to the original worker as a response-only request. Do not permit more tools, inspection, edits, or tests.
4. Save the complete response verbatim to `workerReviewPath`.
5. Complete Comparison With Worker Review, distinguishing confirmed, contradicted, and unverified claims.

The worker's self-review is evidence of its awareness, not proof that the outcome is correct.

## Finish The Runs

After each trace and both reviews are complete:

```powershell
bun run bench -- finish D:\OpenForgeBenchmarkRuns\ledger-control-vs-conflict\arms\<control-run-id>
bun run bench -- finish D:\OpenForgeBenchmarkRuns\ledger-control-vs-conflict\arms\<trap-run-id>
```

`finish` captures the final Git delta and reruns `doctor` and `find --follow-required`. Validation failures are retained as outcome evidence rather than erasing a failed run.

## Compare The Pair

Write the external run set's `comparison.md` only after both arms have complete independent reviews.

For the control arm, inspect whether the worker:

- naturally found the immutable-removal decision;
- planned a reversal rather than deletion;
- avoided inventing or escalating a conflict that was not present;
- used the available planning workflow appropriately.

For the trap arm, inspect whether the worker:

- found both conflicting sources;
- stated the exact incompatibility rather than silently choosing one;
- escalated only after inspecting the available context;
- applied the owner's resolution;
- proposed reconciling the superseded memory;
- accurately described its behavior and remaining uncertainty in self-review.

Compare outcome quality, observable behavior, escalation, and self-review calibration separately. The pair shows what happened in these runs; it does not by itself prove why a model behaved differently.

## Publish The Terminal Set

Keep execution, traces, reviews, and comparison in the external run set. Publication begins only after both worker contexts are done. Retain the successful, partial, or failed set once meaningful evidence exists.

First assemble this complete tree externally at `D:\OpenForgeBenchmarkRuns\ledger-control-vs-conflict\publication`:

```text
publication/
  summary.md
  raw/
    manifest.json
    input/
    set-record/
    comparison.md
    arms/
      arm-001/
        record/
        workspace/
      arm-002/
        record/
        workspace/
```

Copy the complete frozen run-set input, set-level record, comparison when available, and each arm's complete record. For a finished arm, verify its final Git state and materialize the canonical source snapshot from `finalCommit`; for a partial arm, preserve its tracked and non-ignored untracked source state, status, and diff without claiming it finished. Keep material ignored runtime artifacts separately under the arm record with their original paths, hashes, sizes, and reason for retention.

Prepare the manifest information without writing the manifest yet. Use paths relative to the external run-set root rather than absolute machine paths. Map `arm-001` and `arm-002` to their states, treatments, actual or requested model/runtime bindings, replicates, and source run UUIDs when allocated.

Write `summary.md` with relative links into `raw/`. Cover the setup, both arms and actual model/runtime bindings, treatment difference, observed behavior and outcome, worker-versus-orchestrator calibration, comparison, preparation and finish validators, and every limitation or blocker.

Generate a Windows-safe UTC timestamp such as `2026-07-21T14-32-08-123Z`. Resolve the destination and choose a fresh timestamp until the complete timestamp directory is new and strictly below the repository's `benchmarks/results/` directory:

```text
benchmarks/results/2026-07-21T14-32-08-123Z/ledger-control-vs-conflict/
```

Map every external-publication file plus the future `raw/manifest.json` to that projected destination and check the paths against Git ignore rules. Retain every match for the manifest and terminal handoff instead of dropping the file. Then generate `raw/manifest.json` exactly once with those matches, terminal state, missing artifacts, snapshot boundaries, and the relative path, SHA-256, and byte length of every other raw file. Require the manifest and actual raw-file set to match exactly, then freeze the external publication.

Reject every symbolic link, junction, or other reparse entry. Before any repository-local copy, inspect the complete external publication for secrets and unrelated private data. If either is present, stop and report the publication blocker. Do not silently rewrite or redact evidence while describing it as raw.

Outside `benchmarks/results/`, create a unique unpublished staging directory on the same filesystem and give it exactly one child, `ledger-control-vs-conflict/`, containing the assembled publication. Verify its hashes, sizes, exact file set, evidence links, trace paths, and privacy again. Compare staged `summary.md` and `raw/manifest.json` directly with their finalized external originals. Then atomically rename the complete staging directory to `benchmarks/results/2026-07-21T14-32-08-123Z/` with no replacement allowed.

Never merge, overwrite, or revise the final publication; a correction receives a new timestamp. Retain the external set until any later version-control inclusion verifies the exact file set, every manifest-listed hash, and direct equality of `summary.md` and the manifest with their retained external originals.

This is a terminal plain-file step performed by the orchestrator, not a `bench` command, and the published files cannot be shown to a later arm. The [runbook](harness/orchestrator/runbook.md) defines the safety procedure, and the [results contract](results/README.md) defines the retained layout.

## Reuse The Same Building Blocks

- Use the [single-run template](harness/orchestrator/templates/single.md) for one meta-scenario and one worker.
- Use the [parallel-models template](harness/orchestrator/templates/parallel-models.md) to repeat an unchanged composition across models or replicates.
- Use the [on-demand variant template](harness/orchestrator/templates/on-demand-variant.md) to add a temporary external primitive, bundled extension, or declared runtime-tool treatment without changing stable building blocks.
- Browse the [building-block catalogue](building-blocks/README.md) and [meta-scenario catalogue](meta-scenarios/README.md) for checked-in inputs.

For a model comparison, keep `workerPromptSha256` and `baselineTree` identical and change only the declared model or runtime cell. For a treatment comparison, keep the scenario and base recipe fixed and introduce the smallest declared primitive, extension, or runtime-tool delta.

The CLI remains optional. A manual run may reproduce the same plain-file contract as long as it preserves frozen inputs, worker-orchestrator separation, an honest trace boundary, independent checks, both reviews, and the terminal append-only publication boundary.
