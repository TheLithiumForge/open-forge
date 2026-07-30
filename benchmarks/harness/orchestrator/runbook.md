# Benchmark Runbook

Orchestrator-only material. Never place this file, a filled launch prompt, review criteria, personas, variants under construction, or another arm's record in a worker workspace.

## 1. Choose The Run Set

Start from one [launch template](templates/README.md). Edit only its uppercase assignments unless the run intentionally needs a new orchestration instruction, then preserve the complete filled prompt as `<run-set>/input/launch-prompt.md`.

The assignments are plain prompt text. References such as `{META_SCENARIO}` mean the exact value assigned at the top; no template engine or CLI parser is involved.

`RUN_SET` is both the external set identity and eventual publication run name. Use a portable slug containing only lowercase ASCII letters, digits, and single hyphens. Do not use spaces, dots, separators, empty segments, traversal, or a Windows reserved device basename such as `con`, `prn`, `aux`, `nul`, `com1` through `com9`, or `lpt1` through `lpt9`.

Record the requested model, runtime, settings, and runtime-native tool surface before preparation. An unavailable selector or tool is a blocker, not permission to substitute silently.

Create `<run-set>/record/` before preparation. Preserve verbatim set-level preparation, launch, orchestration, and comparison outputs there, including failures whose runner-created arm directory is removed or was never created. Freeze this record before assembling the publication; the terminal handoff reports the later publication transaction itself.

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

Assign every planned arm a unique portable publication key before preparation, using a neutral value such as `arm-001`. Preserve its intended treatment, requested model and runtime, and replicate under the set record; do not expose that mapping to a worker.

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

After each successful preparation, add the runner's run UUID and prepared path to that arm key's set-record mapping. A preparation failure retains the planned key without inventing a UUID that was never allocated.

Before launch, confirm that every workspace contains only its intended scenario, recipe, and treatment. For a model or replicate comparison, both reported `workerPromptSha256` and `baselineTree` must match across arms. For a treatment comparison, `workerPromptSha256` must match while the resolved manifests and baseline trees show only the declared worker-visible delta.

If preparation fails, preserve its complete command output under the set record and stop worker execution for that set. Do not improvise a weaker recipe under the same set identity. Close any worker contexts already created, then proceed to terminal publication when meaningful evidence exists and publication is safe.

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

## 10. Publish The Terminal Set

Publication is the only source-repository mutation phase in the benchmark protocol. It happens after every worker context is done, every available arm is finished or preserved as partial, and the comparison is complete when one can be produced. Published material cannot inform another arm.

A successful, partial, or failed set is publishable once meaningful evidence exists. Missing arms, incomplete finish steps, validator failures, trace gaps, and the exact blocker remain visible.

Assemble this tree completely under the external run set before touching the source repository:

```text
publication/
  summary.md
  raw/
    manifest.json
    input/                   complete frozen run-set input
    set-record/              set-level orchestration and failure evidence
    comparison.md            when present
    arms/
      <arm-key>/
        record/              complete arm record
        workspace/           canonical source snapshot; partial when declared
```

`summary.md` synthesizes the setup, arms, actual models and treatment, observed behavior and outcome, worker-versus-orchestrator calibration, cross-arm comparison when present, validator results, and limitations or blockers. Link each material statement to the relevant relative path under `raw/`.

`raw/manifest.json` records logical source roles and paths relative to the declared external run-set root, never absolute host paths. It records terminal set state, missing material, and the workspace-snapshot boundaries. It also records the relative path, SHA-256, and byte length of every other regular file under `raw/` so the retained evidence can be checked independently. The listed and actual raw-file sets must match exactly, with no duplicate, extra, or missing path. Run inputs and records are copied without selective cleanup or rewriting.

Use each publication `<arm-key>` consistently under `raw/arms/`. In the manifest, map every planned key to its logical treatment, actual or requested model and runtime, replicate, terminal state, and source run UUID when one was allocated.

For each arm with a finished `result.json`, verify that the workspace `HEAD` equals its `finalCommit` and that `git status --short --untracked-files=all` is empty. Materialize `workspace/` from `finalCommit`. This is the canonical final source snapshot, not a byte-complete copy of the live worker directory; ignored and transient files are excluded. For a partial arm, snapshot the current tracked and non-ignored untracked source state, preserve its Git status and diff, and label it partial rather than claiming it was finished.

If an ignored runtime artifact materially supports the review, preserve it after task freeze under that arm's `record/runtime-artifacts/`. Record its original workspace-relative path, SHA-256, byte length, and reason for retention; cite it separately from the source snapshot.

Reject every symbolic link, junction, or other reparse entry anywhere in the external publication. Confirm that every trace path named in `record/trace/manifest.json` remains inside the externally assembled arm record and resolves to a regular copied file. Revalidate those paths after staging.

At publication time, generate a Windows-safe UTC timestamp such as `YYYY-MM-DDTHH-mm-ss-SSSZ`. Resolve the repository root, results root, an unpublished staging path outside the results root but on the same filesystem, and the final timestamp destination. Reject any existing destination or staging ancestor that is a symbolic link, junction, or other reparse point. Verify the final run destination has this shape:

```text
benchmarks/results/<UTC-filesystem-safe-date-time>/<RUN_SET>/
```

Generate a fresh timestamp until the complete `benchmarks/results/<timestamp>/` destination does not exist. Before finalizing the manifest, enumerate every existing regular external-publication file plus the required future `raw/manifest.json`, including dotfiles, and map it to its projected final repository-relative path. Check those paths against Git ignore rules. Preserve every match for the raw manifest and terminal handoff so an ignored file cannot disappear silently; an ignore match does not permit dropping the file and does not block the on-disk publication. Do not call `git add`, force-add files, edit ignore rules, or otherwise mutate the index.

After the raw evidence tree and set record are frozen, generate `raw/manifest.json` exactly once. Exclude the manifest itself from its file list, include the projected ignore matches, and require the listed and actual raw-file sets to match. Do not mutate the finalized external publication afterward. Inspect the entire publication, including `summary.md` and the manifest, for secrets and unrelated private data before any repository-local copy. If either is present, stop and report a publication blocker rather than silently redacting evidence while still calling it raw.

In the unique staging directory, create exactly one child named `RUN_SET`, with no sibling entry, and copy the externally assembled publication into it. Recompute and verify every manifest hash and byte length, require an exact manifest-to-raw-file match, and compare staged `summary.md` and `raw/manifest.json` bytes, sizes, and hashes directly with their finalized external originals. Resolve all retained evidence links, revalidate trace artifacts, reject every staged reparse entry, and rescan the staged publication for private material.

A missing, changed, unsafe, or privately sensitive file is a publication blocker; leave the external evidence intact. Retain the external run set until any later user-controlled version-control inclusion contains the exact publication file set, every manifest-listed raw hash matches, and `summary.md` plus `raw/manifest.json` equal their retained external originals. Do not describe ignored loose files as Git-durable before that verification.

After every check passes, atomically rename the staging directory to the new timestamp destination with no replacement allowed. The rename must fail if that destination now exists. On a destination race, leave that staging tree unpublished and create a fresh external publication assembly from the frozen run-set evidence under a new timestamp; never merge into the winner. An interrupted copy may leave only an unpublished staging directory, never a partial final result. Never merge, overwrite, or revise a publication. A correction or expanded record receives a new UTC timestamp. Remove abandoned staging only after verifying that it belongs to the current publication attempt. There is no benchmark publish command.

## Manual Plain-File Run

The CLI is optional. Resolve the selected `meta.json`, compose each disposable workspace from Core, the scenario, ordered primitives, extensions, and frozen variants, then index, validate, and establish a Git baseline. Preserve the same external run-set inputs, independent traces, checks, and dual reviews.

State which preparation, runtime, or capture properties were manual and which were not independently established. Terminal publication uses the same externally staged, append-only file contract; it does not require a CLI.

## Roles

- Scenario author owns a reusable framework-agnostic task.
- Primitive author owns one small composable worker-visible input and its review focus.
- Meta-scenario author owns one stable exact recipe.
- Orchestrator owns run-set freezing, scheduling, trace capture, independent review, worker self-review, cross-arm comparison, and terminal publication.
- Worker performs one arm and later provides a response-only self-review.
- Runner discovers, composes, validates, baselines, and captures. It does not schedule workers or determine behavioral truth.
