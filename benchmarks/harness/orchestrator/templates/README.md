# Orchestrator Launch Templates

These files are direct-feed prompts for a benchmark orchestrator:

- [Single run](single.md)
- [Parallel models or replicates](parallel-models.md)
- [Stable base versus trap](base-vs-trap.md)
- [On-demand treatment versus control](on-demand-variant.md)

Copy one template, edit the values in its uppercase assignments, and feed the complete result to the orchestrator. Preserve that filled prompt under the external run-set input before any arm is prepared.

Assignments are plain prompt text, not a template language or CLI configuration. A reference such as `{META_SCENARIO}` means the exact value written in `META_SCENARIO = "..."`. Keep assignment and reference casing identical. Use `NONE` or an empty list to state that an optional input is deliberately absent; the orchestrator must not invent a value.

`RUN_SET` is the run name. Use a portable slug containing only lowercase ASCII letters, digits, and single hyphens, with no spaces, dots, separators, empty segments, traversal, or Windows reserved device basename. The same value names the terminal publication.

The template chooses the run set. The [invariant orchestrator prompt](../orchestrator-prompt.md), [runbook](../runbook.md), and [trace contract](../trace.md) keep execution and review behavior stable.

Every comparison freezes all arm compositions before worker launch. Model comparisons keep task and worker-visible composition fixed while declaring the model or runtime-selector difference. Treatment comparisons keep the scenario and base recipe fixed while declaring the smallest primitive, extension, or runtime-tool delta.

`VARIANTS` and `VARIANT_PATHS` refer to external additive primitive packages passed through repeatable `--variant` options. `EXTENSIONS` refers to bundled treatment extensions passed through repeatable `--extension` options. `ORCHESTRATOR_ADDENDUM` is review-only and must never change worker-visible inputs.

Requested provider- or runtime-native tools are prompt assignments, not proof that a runtime bound them. The orchestrator records the actual binding and known gaps in each arm's trace manifest.

Execution stays in the external run set. After every worker context is done, the orchestrator publishes `summary.md` and complete `raw/` evidence under `benchmarks/results/<UTC-filesystem-safe-date-time>/<RUN_SET>/` through the runbook's verified staging procedure. Successful, partial, and failed sets retain their actual state. Publication is a plain-file terminal step, not a CLI command, and cannot feed another arm.
