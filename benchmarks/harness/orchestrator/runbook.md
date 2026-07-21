# Benchmark Runbook

Orchestrator-only material. Never place this file, a filled launch prompt, review criteria, personas, variants under construction, or another arm's record in a worker workspace.

## 1. Choose The Run Set

Start from one [launch template](templates/README.md). Edit only its uppercase assignments unless the run intentionally needs a new orchestration instruction, then preserve the complete filled prompt as `<run-set>/input/launch-prompt.md`.

The assignments are plain prompt text. References such as `{META_SCENARIO}` mean the exact value assigned at the top; no template engine or CLI parser is involved.

Record the requested model, runtime, settings, and runtime-native tool surface before preparation. An unavailable selector or tool is a blocker, not permission to substitute silently.

## 2. Resolve Every Arm

Discover the stable inputs:

```powershell
bun run bench -- list
bun run bench -- list meta-scenarios
bun run bench -- list scenarios
bun run bench -- list primitives
```

Each arm starts from one exact meta-scenario. A meta-scenario names one agnostic scenario, an ordered primitive list, and bundled extensions. A baseline and a trap are separate recipes rather than modes applied to one mutable recipe.

Resolve all treatment differences before launching any worker:

- A repeatable `--variant <path>` adds an external primitive package.
- A repeatable `--extension <id>` adds a bundled extension.
- `--orchestrator <file>` adds review-only observation, checking, or recording instructions.
- A `kind: tool` primitive delivers ordinary local-tool files and may include a `.agents/workspace/**` discovery route.
- A provider- or runtime-native tool is bound by the runtime and recorded in the launch prompt and trace boundary; the runner does not provision it.

Freeze every resolved composition under `<run-set>/input/resolved-compositions/`. Freeze external packages and runtime-binding declarations under `<run-set>/input/variants/`. Do not generate or revise a treatment after any arm outcome is visible.

## 3. Prepare Every Arm

Prepare each arm before starting any worker:

```powershell
bun run bench -- prepare <meta-scenario-id> `
  --runs-root <run-set>/arms `
  [--variant <external-primitive-package>]... `
  [--extension <bundled-extension-id>]... `
  [--orchestrator <review-only-addendum>]
```

Preparation composes the worker workspace, freezes the resolved inputs outside it, runs `doctor` and `find --follow-required`, and establishes the Git baseline. Retain every reported path.

Before launch, confirm that every workspace contains only its intended scenario, recipe, and treatment. For a model or replicate comparison, both reported `workerPromptSha256` and `baselineTree` must match across arms. For a treatment comparison, `workerPromptSha256` must match while the resolved manifests and baseline trees show only the declared worker-visible delta.

If preparation fails, preserve the error and stop that set. Do not improvise a weaker recipe under the same set identity.

## 4. Launch Independent Workers

Launch a fresh worker context for every arm when the platform supports it. Set its current directory to the prepared workspace and give it only the exact prepared worker prompt plus the declared runtime tool surface.

Do not expose hidden reviews, orchestration prompts or addenda, arm labels, other arm inputs or traces, prior benchmark conversation, historical results, or the source repository used for preparation.

Parallel launch is preferred for direct comparisons because no arm can influence another through operator adaptation. If the runtime forces sequential launch, freeze all arms first and state that limitation.

Observe without coaching. Answer only interactions declared by the frozen scenario or persona, and preserve the exchange exactly.

## 5. Preserve The Observable Trace

Follow the [trace contract](trace.md) for each arm. Save `record/trace/manifest.json` and the runtime-native raw artifacts under `record/trace/raw/`.

The trace includes everything the runtime exposes to the orchestrator: worker and orchestrator messages, tool calls and results, interactions, status events, and exposed reasoning summaries. It does not include invisible private chain-of-thought.

Keep traces isolated by arm. Do not normalize away ordering, failed calls, retries, or uncertainty merely to simplify comparison.

## 6. Review Each Arm Independently

When an arm's task work ends, freeze its outcome and keep the worker context available. Do not read or request the worker self-review yet.

Review Behavior from the observable trace. Identify reads, actions, decisions, interactions, tool use, adherence, conflict handling, escalation, deviations, and unknowns supported by that record.

Review Outcome from the final Git diff, relevant files, and proportionate independent checks. Distinguish verified facts from inference. Run a potentially mutating check against a disposable copy and preserve its output with the arm record.

Draft these sections in `record/orchestrator-review.md`:

```markdown
# Orchestrator Review

## Behavior

## Outcome

## Comparison With Worker Review

## Limits
```

Complete Behavior, Outcome, and Limits before the worker review. Leave Comparison With Worker Review empty until the next step.

## 7. Capture Worker Self-Review

Send the exact [worker self-review prompt](worker-review-prompt.md) to the same worker context.

This phase is response-only. Permit no tools, file inspection, commands, tests, edits, fixes, or other corrective work. Save the complete response verbatim as `record/worker-review.md`.

Compare the worker account with the already-formed findings. Complete Comparison With Worker Review with material claims confirmed, contradicted, or not independently verifiable. Self-awareness does not repair an outcome, and confidence does not verify one.

## 8. Finish Every Arm

```powershell
bun run bench -- finish <run-directory>
```

Finish captures the final evidence, reruns both complete-workspace validators, and records validator failures as part of the result. Preserve failed and partial outcomes; do not repair their evidence after seeing the result.

## 9. Compare The Set

Write `<run-set>/comparison.md` only after every included arm has a completed independent review and worker comparison.

For model variance, compare arms whose `workerPromptSha256` and `baselineTree` match. State any actual runtime, system-context, setting, or tool-surface difference rather than calling the environments identical.

For treatment variance, compare arms that share the scenario and base recipe. Name the minimal declared primitive, extension, or runtime-tool delta and check the resolved compositions before attributing an observed difference to it.

Separate outcome differences, behavior differences, self-review calibration, and unknown trace gaps. Replication supports an observation about variance; it does not by itself establish why the variance occurred.

## Manual Plain-File Run

The CLI is optional. Resolve the selected `meta.json`, compose each disposable workspace from Core, the scenario, ordered primitives, extensions, and frozen variants, then index, validate, and establish a Git baseline. Preserve the same external run-set inputs, independent traces, checks, and dual reviews.

State which preparation, runtime, or capture properties were manual and which were not independently established.

## Roles

- Scenario author owns a reusable framework-agnostic task.
- Primitive author owns one small composable worker-visible input and its review focus.
- Meta-scenario author owns one stable exact recipe.
- Orchestrator owns run-set freezing, scheduling, trace capture, independent review, worker self-review, and cross-arm comparison.
- Worker performs one arm and later provides a response-only self-review.
- Runner discovers, composes, validates, baselines, and captures. It does not schedule workers or determine behavioral truth.
