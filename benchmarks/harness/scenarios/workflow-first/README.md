# Workflow-First And Support-Derivation Scenarios

Status: **UNRUN**. These are frozen engineering-scenario templates, not benchmark results. No agent-compliance claim may be made from their presence or from the mechanical preparation test.

The seven cases isolate the behavioral questions introduced by the workflow-first, phase-aware loader contract:

| Case | Installed workflow surface | Behavioral question |
| --- | --- | --- |
| `exact-architecture-greenfield` | architecture | Does an exact architecture request select the architecture workflow, load its required skill first, and derive a greenfield support bundle with provenance and per-item accord? |
| `exact-vision-support` | vision | Does an exact vision request select the vision workflow and keep inferred support material candidate until the user accords it? |
| `no-match-direct-choice` | vision + architecture | With no matching localization workflow, does the worker ask once whether to adapt a closest route or proceed directly, then honor the user's direct choice? |
| `explicit-no-workflow` | vision + architecture | Does an explicit workflow opt-out bypass recommendation and selection without bypassing the rest of the loader? |
| `ordered-handoffs` | vision + architecture + implementation | Does a cross-phase request infer the unsettled state, choose one primary workflow, and activate other workflows only through evidence-triggered handoffs rather than a mandatory phase order? |
| `direct-delivery-sufficient-truth` | representative workflows from all five phases | When routed current truth already establishes product and technical direction, does the worker start with the delivery workflow without replaying earlier phases? |
| `helpful-prior-architecture-work` | representative workflows from all five phases | When product direction is accepted but technical direction is concretely absent, does the worker recommend architecture once as helpful prior work before delivery without treating it as a gate or imposing a full phase sequence? |

Each run-spec template uses Core plus only the capability/workflow payloads needed for its catalogue. The architecture and localization cases also include a small raw worker-visible input. The runner's composition CLI is snapshotted from `src/cli/cli.ts`; it is not automatically provisioned to the worker.

## What The Automated Test Establishes

`benchmarks/harness/runner.closure.test.ts` parses every template through the executable run-spec validator, checks every declared input exists, prepares each composition in an OS temporary directory through the real CLI invocation, authenticates the prepared evidence, runs `doctor`, and checks that expected workflow files, phase tags, and case-defining current truth are worker-visible and routed.

That establishes only that the cases are structurally valid and mechanically runnable. It does **not** establish workflow selection, route-read order, user interaction, artifact quality, or agent compliance.

## How To Produce Behavioral Evidence

1. Copy a template to a frozen experiment location and replace the deliberately unassigned model/runtime fields and conservative isolation declarations before `prepare`. Preserve resolved component, prompt, and rubric paths.
2. Use a genuinely fresh worker and the exact snapshotted prompt. For interactive cases, follow only the matching frozen interaction script in `rubric.md`; do not volunteer hidden facts or coach the worker.
3. Capture an external interaction transcript and runtime/tool trace. Route-read order, selection timing, prompts to the user, writes, and handoff activation must come from those traces; worker debrief is corroboration only.
4. Evaluate the matching `seed-*` dimensions in `rubric.md`, plus all five core dimensions. A missing or ambiguous trace earns at most partial credit; prose self-report alone cannot earn a clean score.
5. Finalize and authenticate the run through the normal harness lifecycle. Keep `scenarioStatus: unrun` only in these checked-in templates; a copied run spec should record its actual frozen model/runtime configuration.

Do not add a result or update this status merely because preparation passed. Change it only after a real external worker run has finalized, validated, and been linked with its run id and evidence location.
