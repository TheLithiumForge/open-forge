---
open-forge:
  description: The public results repository, result versions, schema v2 records, evidence in Git and publishing flow
  tags: [Memory, Document, CurrentTruth, Evaluation, Results, History, Publishing]
---

# Eval Results

Eval Results is the public record of Open Forge comparisons, with versioned measurements and saved evidence shown in the main documentation site.

Status: approved on 2026-11-01 and revised by the maintainer on 2026-11-02: evidence lives in the repository's Git history, and the docs site is the only results UI. `results add` is built. No batches are published yet.

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
| `versions/<cli-version>/batches/<batch>/` | `experiment.json`, `setups.jsonl`, `runs.jsonl`, `assessments/`, `artifacts.json` and `evidence/` |
| `index.json` | Generated batch catalogue, including layers, models, grading versions and record paths |
| `scripts/` | Validation, views and publication tools |

Local unpublished runs stay in gitignored `src/evals/runs/`.

## Evidence storage

Each batch keeps its evidence in Git beside its records, in `evidence/` at the paths `artifacts.json` lists: agent code without installed packages or downloaded tools, the built game, screenshots, conversations and event logs. Tool folders the task project's `.gitignore` names and any file over 50 MB stay in the run store, and a run's published manifest and hash cover what is published. Staging verifies every file's size and SHA-256, and validation checks them again. The repository can be rebuilt from saved runs, so adding a batch again replaces it. This replaced the earlier plan of one release asset per batch, because evidence in Git can be browsed, linked from the site and checked by CI.

`results add` removes the user and account fields from each Codex transcript's session header. Evidence that still holds a sign-in value, a token claim, a machine path or the user name stops it before anything is staged.

## Publishing

After a batch is finished, `npm run evals -- results add <batch>` from the repository root exports its sanitized records, checks its evidence, stages both through the results repository's own scripts and validates the repository.

Publishing follows these steps:

1. Add finished batches with `results add` and commit them in the results repository.
2. The maintainer pushes the results repository and updates the submodule pin.
3. CI validates schemas, references, evidence hashes and the catalogue.
4. For a batch published with its checks left for later, `npm run evals -- results grade` starts the Grade published runs workflow, which checks each waiting run in its own job on GitHub's Ubuntu runners, up to 20 at a time, with the graders of Open Forge's pushed commit and a verifier image kept in GitHub's container registry, and pushes the grades to the results repository's `main`, with no pull request. AI judges never run there, since those jobs build agent-written code. The [results repository council](../../../emerging/analysis/results-repository-council.md) records why.

Each publication changes a remote repository and needs the maintainer's go. A failed validation keeps invalid records out of the site. The [proposal's publish flow](../../../emerging/analysis/eval-product-proposal.md#results-repository-and-site) defines these steps and permission.

## Results site

The Evals section lives in Open Forge's main documentation site under `src/docusaurus/`. The results repository is a data store with validation CI and no Pages deployment. The submodule pins the initial data copy read at build time.

The Results page reads four sources through one validator: the copy pinned at build time, the latest published commit, which it fetches on open unless the local site command opened it, local runs served by `npm run evals -- site` with running batches included, and a results folder picked on the reader's computer. It shows each batch's progress, compares every setup with the plan's first setup and links every run's evidence from the active source. Fetch latest results resolves the public results repository's current commit. It then reads `index.json` and records through raw GitHub URLs pinned to that commit. The site validates their schemas and references before replacing the displayed data. A failed fetch keeps the previous copy. Record text is plain data. Each view shows its data commit, copy or fetch time and analysis version. New records can be read without a CLI or docs release.

The site provides layer overviews, setup tables for each implementer and judge pair, charts, attempt evidence, CLI history and the reference trend. Wilson and bootstrap intervals describe screening variation. Different grading versions stay apart. Missing values stay missing.

Keep correctness, rules, cost, wishes and preferences in separate panels. Show regressions and inconclusive results. The [proposal's site design](../../../emerging/analysis/eval-product-proposal.md#results-repository-and-site) defines these views. The current sample and limits on claims follow [Vision](vision.md#claims).

Back to [Evals](_evals.md).
