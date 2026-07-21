# Benchmark Harness

The harness resolves reusable benchmark recipes into disposable worker workspaces and records dual-reviewed runs. It prepares and captures; it does not schedule agents, provision provider-native tools, or determine behavioral truth.

## Commands

```powershell
bun run bench -- list [meta-scenarios|scenarios|primitives]
bun run bench -- prepare <meta-scenario-id> --runs-root <external-directory> [--variant <path>]... [--extension <id>]... [--orchestrator <file>]
bun run bench -- finish <run-directory>
```

`list` defaults to runnable meta-scenarios. The explicit kinds expose the underlying building blocks.

`prepare` requires a runs root that neither contains nor sits inside the source repository. It resolves one exact meta-scenario, composes its worker-visible sources, freezes the resolved composition and orchestrator-only material, validates the complete workspace, and creates an initial Git baseline.

`finish` requires the completed worker and orchestrator reviews, captures the final delta and checks, and writes the finished result. Validator failures remain outcome evidence instead of making a failed run impossible to retain.

Package scripts are the public CLI interface. Direct invocation of `runner.ts` is for harness development.

## Composition

The corpus is hierarchical:

```text
benchmarks/
  building-blocks/
    scenarios/
    primitives/
  meta-scenarios/
```

A scenario directory contains `scenario.json` with an explicit string `id`, the exact worker prompt, hidden scenario review focus, and any scenario-owned task material. It remains independent of Open Forge primitives and extensions.

A primitive directory contains `primitive.json` with an explicit string `id` and `kind`, plus one small additive package and its review focus. The supported kinds are `directive`, `pattern`, `memory`, `guidance`, `workspace`, `skill`, `workflow`, and `tool`.

A meta-scenario directory contains `meta.json` with an explicit string `id`, one scenario id, an ordered primitive id list, and a bundled extension id list. It is an exact stable recipe. A base and a trap are separate meta-scenarios, even when they share the same scenario.

The [building-block catalogue](../building-blocks/README.md) and [meta-scenario catalogue](../meta-scenarios/README.md) expose the checked-in set.

Folders organize the corpus; ids define references. Preparation is deterministic, path-safe, and collision-aware.

## Treatments

Repeat `--variant <path>` to add external primitive packages without editing the stable corpus. The runner validates each package through the same primitive contract and freezes its exact content with the run. Variant packages are additive; they do not rewrite the selected recipe.

Repeat `--extension <id>` to install bundled extensions as an explicit treatment in addition to those named by the meta-scenario. The resolved extension set is frozen before worker launch.

Use `--orchestrator <file>` only for extra observation, non-mutating checks, or recordkeeping. The addendum remains outside the worker workspace and cannot introduce worker-visible instructions, primitives, extensions, tools, or task changes.

A primitive with `kind: tool` delivers at least one ordinary local-tool file inside the workspace and may also carry a `.agents/workspace/**` route so the tool is naturally discoverable. Provider- or runtime-native tool availability is a runtime binding, not a file the runner can truthfully provision. Freeze the requested runtime tool surface in the filled launch prompt, record the actual binding in the trace manifest, and declare that surface as a treatment delta when it differs between arms.

## Prepared Run

```text
<run-directory>/
  workspace/                 worker-visible composition
  record/
    input/                   frozen recipe, composition, prompts, and review inputs
    trace/
      manifest.json          actual observable boundary and known gaps
      raw/                   runtime-native trace artifacts
    worker-review.md         worker account, saved verbatim
    orchestrator-review.md   independent review and comparison
    changes.patch            baseline-to-final workspace delta
    result.json              compact finished-run index
```

The runner may add validator output, Git state, and other capture files under `record/`. The [trace contract](orchestrator/trace.md) defines what the orchestrator preserves during execution.

`record/input/composition.json`, `record/run.json`, and the runner result include `workerPromptSha256` and `baselineTree`. Those values provide a direct byte-identity check for equal model or replicate arms; ids alone are not treated as proof that dirty-source inputs matched.

## Run Sets

Comparisons use an external run-set directory so shared inputs and independent arms remain visible:

```text
<run-set>/
  input/
    launch-prompt.md
    resolved-compositions/
    variants/                optional frozen external packages or runtime bindings
  arms/
    <arm-id>/                one independent prepared run
  comparison.md
```

The filled launch prompt and every resolved composition are frozen before any worker starts. Each arm retains its own workspace, trace, checks, and dual review. `comparison.md` is written only after the independent arm reviews are complete.

The harness does not need to create or own this grouping. An orchestrator may place runner-created arm directories into the external set or preserve their exact paths in the set record.

## Worker And Orchestrator Boundary

The orchestrator launches each worker in its prepared workspace with only the exact worker prompt and intended runtime surface. Review criteria, orchestration material, other arms, prior outcomes, and the source repository remain outside the worker context.

When task work ends, the orchestrator freezes the outcome and drafts Behavior, Outcome, and Limits from the trace, final workspace, delta, and independent checks. Only then does it send the [worker self-review prompt](orchestrator/worker-review-prompt.md) to the same worker as a response-only request. The complete response is saved verbatim before the orchestrator completes Comparison With Worker Review.

Unknown behavior stays unknown. A worker statement, orchestrator statement, prepared workspace, or successful validator does not prove an isolation boundary the runtime did not expose.

## Checks

Preparation and finish run both complete-workspace validators: `open-forge doctor --json` and `open-forge find --follow-required --json`. Preparation requires both to succeed. Finish records both results even when the final workspace is invalid.

Independent outcome checks must not change the frozen task outcome. Run a potentially mutating check against a disposable copy and retain its output with the arm record.

## Manual Equivalent

The CLI is optional. A plain-file operator can resolve `meta.json`, copy the selected scenario and ordered primitives, install the declared extensions, add any frozen variants, index and validate the workspace, and establish a Git baseline outside the source repository. Preserve the exact worker prompt, its hash, the baseline tree id, hidden reviews, filled launch prompt, resolved composition, trace boundary, checks, and dual reviews outside the worker workspace.

The manual method must preserve the same worker/orchestrator separation and describe any preparation or capture property it did not establish.

## Orchestrator Material

- [Runbook](orchestrator/runbook.md)
- [Invariant prompt](orchestrator/orchestrator-prompt.md)
- [Launch templates](orchestrator/templates/README.md)
- [Trace contract](orchestrator/trace.md)
- [Worker self-review prompt](orchestrator/worker-review-prompt.md)
