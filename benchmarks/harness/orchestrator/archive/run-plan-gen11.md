# Run Plan - Generation 11 (orchestrator only)

> Historical and frozen. This predates the P0 runner and documents the already-completed generation 11 treatment; do not use it as the current runbook or as causal evidence. Quantitative wording below is preserved as-run and is corrected by `.agents/memory/emerging/observations/2026-07-12_benchmark-validity-gaps.md`.

Paste `orchestrator-prompt.md` first, then this plan. This generation isolates one variable against gen10 as the control; see `.agents/memory/crystallized/documents/evaluations/v10-synthesis.md`.

## Composition Per Scenario

1. seed-1, seed-2, seed-3 (build mode): extend `benchmarks/harness`, the seed, `benchmarks/variable-closeout-command`, and `benchmarks/variable-sessions-keepinmind`. The two overlays touch different files and combine safely. Variables label: `closeout-sessions`.
2. seed-0: skip this generation unless running the model comparison below; its framework questions were answered in gen10.

## Measured Question

Does tagging sessions #KeepInMind (so they appear in the closeout printout) plus the explicit write axiom close the full closeout loop? Gen10 control: three of three ran the recheck command, two of three wrote a session record. Success is three of three full loops - command run, session record, handoff, grounded observation where warranted, reindex. Add one control-comparison line per report.

## Debrief Additions

- Ask for the verbatim closeout command invocation and what the printout contained; specifically whether the sessions route body appeared and what the worker did next.
- Ask whether the session-record axiom was noticed at load time or only via the closeout printout - this separates the tag's effect from the axiom's effect.

## Optional: Model Comparison

If a second model (Fable or Sonnet) is available for workers, additionally rerun seed-0 (vision mode, `workflow-essentials` variable) and seed-2 (build mode, no variables) with that model. Label: `model-<name>`. The discriminators: seed-0's time-tracking curveball and recall-window question (missed four of four by codex-gpt5), and seed-2's closeout discipline without any overlay.

## Provenance Note

The source repo intentionally carries staged, uncommitted changes; record the framework commit plus "staged changes present". Reports use only workspace-relative paths.
