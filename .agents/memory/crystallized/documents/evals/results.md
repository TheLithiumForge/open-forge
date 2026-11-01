---
open-forge:
  description: The public results repository, result versions, schema v2 records, evidence assets and publishing flow
  tags: [Memory, Document, CurrentTruth, Evaluation, Results, History, Publishing]
---

# Eval Results

Eval Results is the public record of Open Forge comparisons, with versioned measurements, saved evidence and a separate results site.

Status: approved on 2026-11-01. The results repository and publishing system are Not built yet.

The public `open-forge-eval-results` repository will hold the records, evidence manifests and its Docusaurus site. It will be a submodule at `src/evals/results/`. The runner, specs and local UI stay in `src/evals/`. The [approval](../../../emerging/analysis/eval-product-proposal.md#approved) makes the repository public.

## Versions

Address each result by CLI version, then batch, then run. Record benchmark, grading, runner, model and schema versions apart. An unreleased CLI gets a candidate label with source and package hashes. The No setup control records the CLI version it is compared with.

Capture separate fingerprints for benchmark inputs, setup content, grading and execution tools at batch start. Record the installed Open Forge file hash and actual source reads. A changed model needs a new matched comparison. Keep historical G1 inputs and grading versions separate. The [proposal's results contract](../../../emerging/analysis/eval-product-proposal.md#results-repository-and-site) defines versioning, and its [delivery plan](../../../emerging/analysis/eval-product-proposal.md#delivery-phases) requires separate fingerprints at batch start.

## Records

Schema v2 has five record types:

| Record | Contains |
| --- | --- |
| Experiment | Registered inputs, comparison plan, versions, limits and order |
| Setup | Generated files, installed file hashes, commands and setup effort |
| Run | Attempt outcome, usage and links to saved evidence |
| Assessment | Check results, judge ballots, audits or a blind pick |
| Artifact | Saved file references and hashes |

JSONL is the source, validated by JSON Schema. CSV, chart data and tables are generated views. Missing values stay `null`. Keep failures, interruptions and retries. Recheck and rejudge append assessments with their checker, rubric and judge versions. They never overwrite a grade.

A blind pick assesses two attempts. It records both artifact hashes, their sides, the pick or tie and whether the setups had been revealed. The [proposal's record design](../../../emerging/analysis/eval-product-proposal.md#results-repository-and-site) defines these records.

The planned repository layout is:

| Path in the results repository | Contents |
| --- | --- |
| `schemas/v2/` | JSON Schemas |
| `benchmarks/<input-hash>/manifest.json` | Frozen input manifest |
| `versions/<cli-version>/batches/<batch>/` | `experiment.json`, `setups.jsonl`, `runs.jsonl`, `assessments/` and `artifacts.json` |
| `site/` | Docusaurus source |
| `scripts/` | Validation, views and publication tools |

Local unpublished runs stay in gitignored `src/evals/runs/`.

## Evidence storage

Store each kind of evidence in its chosen place:

| Place | Contents |
| --- | --- |
| Git | Records, schemas, manifests, site source and lockfiles |
| Release assets | Compressed agent source, built games, screenshots, transcripts and tooling snapshots |

Use one immutable release per batch. Checksum the assets and split them at 1 GiB. Remove credentials and machine paths during export. A correction creates a new release linked to the earlier release. The [proposal's storage rules](../../../emerging/analysis/eval-product-proposal.md#results-repository-and-site) define this split.

## Publishing

After the batch is complete, the planned command is `npm run evals -- publish --batch <batch>` from the repository root. It will produce frozen, sanitized records and bundled files for staging.

Publishing follows these steps:

1. Check batch completeness, freeze the records, remove private data and bundle evidence.
2. Open a results change and a draft release with the staged files.
3. Validate schemas, references and comparison groups in CI.
4. On merge, verify uploaded asset hashes, publish the release and rebuild the site.

Each publication changes a remote repository and needs the maintainer's go. A failed check or deploy leaves the previous site up. The [proposal's publish flow](../../../emerging/analysis/eval-product-proposal.md#results-repository-and-site) defines these steps and permission.

## Results site

The site publishes to GitHub Pages from its repository's `main` on every push. New results need neither a CLI release nor a docs release. Open Forge's docs link to it under "Evaluation results". The submodule pins a reviewed snapshot for local use, and publishing doesn't need a new pin.

The site provides an overview, CLI history with the model fixed, model history with Open Forge and its control side by side, batch comparisons with intervals, single attempts and run details with downloads. Every chart shows its data commit and analysis version.

Keep correctness, rules, cost, wishes and preferences in separate panels. Show regressions and inconclusive results. The [proposal's site design](../../../emerging/analysis/eval-product-proposal.md#results-repository-and-site) defines these views. The current sample and limits on claims follow [Vision](vision.md#claims).

Back to [Evals](_evals.md).
