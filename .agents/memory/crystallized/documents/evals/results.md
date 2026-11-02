---
open-forge:
  description: The public results repository, result versions, schema v2 records, evidence assets and publishing flow
  tags: [Memory, Document, CurrentTruth, Evaluation, Results, History, Publishing]
---

# Eval Results

Eval Results is the public record of Open Forge comparisons, with versioned measurements and saved evidence shown in the main documentation site.

Status: approved on 2026-11-01. The repository, its original site and the submodule were created the same day, with no batches published yet. The publish command is Not built yet.

The public [`open-forge-eval-results`](https://github.com/TheLithiumForge/open-forge-eval-results) repository holds the records, schemas, generated `index.json` and evidence manifests. The [Evals pages](https://thelithiumforge.github.io/open-forge/docs/evals) in the main Docusaurus site show them. It's a submodule at `src/evals/results/`. The runner, specs and local UI stay in `src/evals/`. The [approval](../../../emerging/analysis/eval-product-proposal.md#approved) makes the repository public.

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
| `index.json` | Generated batch catalogue, including layers, models, grading versions and record paths |
| `scripts/` | Validation, views and publication tools |

Local unpublished runs stay in gitignored `src/evals/runs/`.

## Evidence storage

Store each kind of evidence in its chosen place:

| Place | Contents |
| --- | --- |
| Git | Records, schemas, manifests, catalogue and lockfiles |
| Release assets | Compressed agent source, built games, screenshots, transcripts and tooling snapshots |

Use one immutable release per batch. Checksum the assets and split them at 1 GiB. Remove credentials and machine paths during export. A correction creates a new release linked to the earlier release. The [proposal's storage rules](../../../emerging/analysis/eval-product-proposal.md#results-repository-and-site) define this split.

## Publishing

After the batch is complete, the planned command is `npm run evals -- publish --batch <batch>` from the repository root. It will produce frozen, sanitized records and bundled files for staging.

Publishing follows these steps:

1. Check batch completeness, freeze the records, remove private data and bundle evidence.
2. Open a results change and a draft release with the staged files.
3. Validate schemas, references and comparison groups in CI.
4. On merge, verify uploaded asset hashes, publish the release and update the catalogue.

Each publication changes a remote repository and needs the maintainer's go. A failed validation keeps invalid records out of the site. The [proposal's publish flow](../../../emerging/analysis/eval-product-proposal.md#results-repository-and-site) defines these steps and permission.

## Results site

The Evals section lives in Open Forge's main documentation site under `src/docusaurus/`. The results repository is a data store with validation CI and no Pages deployment. The submodule pins the initial data copy read at build time.

Fetch latest results resolves the public results repository's current commit. It then reads `index.json` and records through raw GitHub URLs pinned to that commit. The site validates their schemas and references before replacing the displayed data. A failed fetch keeps the previous copy. Record text is plain data. Each view shows its data commit, copy or fetch time and analysis version. New records can be read without a CLI or docs release.

The site provides layer overviews, setup tables for each implementer and judge pair, charts, attempt evidence, CLI history and the reference trend. Wilson and bootstrap intervals describe screening variation. Different grading versions stay apart. Missing values stay missing.

Keep correctness, rules, cost, wishes and preferences in separate panels. Show regressions and inconclusive results. The [proposal's site design](../../../emerging/analysis/eval-product-proposal.md#results-repository-and-site) defines these views. The current sample and limits on claims follow [Vision](vision.md#claims).

Back to [Evals](_evals.md).
