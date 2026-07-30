# Benchmark Results

This directory contains append-only current-protocol publications and earlier hand-authored benchmark history.

## Current Publications

Each terminal run set is published once at:

```text
<UTC-filesystem-safe-date-time>/<run-name>/
  summary.md
  raw/
    manifest.json
    input/
    set-record/
    comparison.md            when present
    arms/
      <arm-key>/
        record/
        workspace/
```

The UTC directory uses a Windows-safe value such as `YYYY-MM-DDTHH-mm-ss-SSSZ`. `run-name` is the filled template's `RUN_SET`: a lowercase slug containing letters, digits, and single hyphens, with no separators, traversal, or Windows reserved device basename.

The final timestamp destination is resolved and verified below this directory before writing. A complete timestamp tree containing the run-name child is copied to unique unpublished staging outside this directory, verified on the same filesystem, and atomically renamed here without replacement. An existing publication is never merged, overwritten, or revised. A correction or expanded record receives a new timestamp.

Publication trees contain only ordinary directories and regular files. A symbolic link, junction, or other reparse entry blocks publication.

`summary.md` synthesizes:

- Setup, meta-scenarios, treatments, and intended comparison.
- Arms, actual models, runtimes, settings, and tool surfaces.
- Observed behavior and actual outcomes.
- Worker self-review versus orchestrator findings.
- Cross-arm comparison when one exists.
- Preparation and final validator results.
- Limitations, missing evidence, partial state, and blockers.

Material claims link to relative evidence under `raw/`.

Raw evidence preserves the complete external run-set input and set record, `comparison.md` when present, every available arm's complete `record/`, and its canonical final or explicitly partial source snapshot. A finished snapshot is materialized from `finalCommit`, not copied byte-for-byte from the live directory; material ignored runtime artifacts are retained separately in the arm record. `raw/manifest.json` records logical source roles, paths relative to the external run-set root, terminal state, missing material, snapshot boundaries, and the relative path, SHA-256, and byte length of every other raw file. It maps every planned neutral arm key to its state, treatment, model, runtime, replicate, and source run UUID when one was allocated.

Successful, partial, and failed sets are published after meaningful evidence exists and every worker context is done. Published evidence cannot feed another arm. Projected Git-ignored paths are recorded in the manifest and handoff rather than dropped; ignored loose files are not described as Git-durable until later user-controlled inclusion verifies the exact file set, all manifest-listed hashes, and direct equality of `summary.md` and the manifest with their external originals. If any part of the external or staged publication contains secrets or unrelated private data, publication stops rather than silently redacting or rewriting the evidence.

Publication is a plain-file orchestrator action described by the [runbook](../harness/orchestrator/runbook.md). There is no CLI publish command.

A timestamp-shaped directory is current-protocol evidence only when it satisfies this complete `summary.md` plus `raw/` contract. Existing bundles explicitly labeled conclusions-only or lacking `raw/` remain transitional history; their directory shape does not upgrade their evidence status.

## Earlier Results

The top-level dated Markdown reports, [pre-harness reports](pre-harness/), and retained conclusions-only bundles predate or do not satisfy the complete current publication contract. They remain design history because they contain useful product defects, worker overstatements, and framework friction.

Earlier reports are not current run inputs, comparable replications, or causal evidence. Legacy local artifacts may still exist under ignored repository paths, but current execution requires a non-overlapping external runs root.

For a complete current example, follow the [benchmark tutorial](../TUTORIAL.md).
