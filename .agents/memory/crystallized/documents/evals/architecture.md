---
open-forge:
  description: The approved Evals product folders, three images, container rules and stage inputs and outputs
  tags: [Memory, Document, CurrentTruth, Evaluation, Architecture, Harness, Isolation]
---

# Eval Architecture

Open Forge Evals is a product under `src/evals/` that prepares cases, runs agents, grades frozen files and prepares results for publication.

Status: approved design on 2026-11-01. The complete product is Not built yet.

One command, `npm run evals -- <command>` from the repository root, will drive the stages. For example, a saved Layer 1 plan will produce frozen game files, check evidence and separate judge assessments. The [proposal's product design](../../../emerging/analysis/eval-product-proposal.md#product) defines this structure. The overseer revises this document after the build.

## Product folders

The planned folders have these jobs:

| Path under `src/evals/` | Job |
| --- | --- |
| `cli.ts` | One entry point for every command |
| `experiments/` | Saved cases, setups, models, limits and versions |
| `execution/containers/` | Docker stages, file copies and cleanup |
| `execution/providers/` | Codex and Claude sessions, sign-in and telemetry |
| `execution/conversation/` | Message protocol and simulated user |
| `materialize/` | Setups generated from one source |
| `grading/` | Checks, judge, rubrics and appended assessments |
| `benchmarks/neon-pairs/` | Frozen benchmark inputs and reference |
| `history/` | Versioned records, progress and export to the results repository |
| `site/` | Read-only data server that shows local runs in the docs site's Results page |
| `images/` | Agent, verifier and assessor Dockerfiles |
| `results/` | Results repository as a submodule |

The benchmark contains `specs/`, `reference/`, `requirements.json`, `checks.json`, `layers/`, `cases/`, `fixtures/`, `trace.json` and `legacy/`. Requirements keep each clause's source and first layer. Checks keep their clauses, method and expected result. The trace maps rules to code to checks. Legacy G1 inputs and checks stay frozen.

The editable Neon Pairs documents generate the frozen, hashed `specs/` pack. The [proposal's layout](../../../emerging/analysis/eval-product-proposal.md#where-the-work-lives) defines the product folders, and its [document plan](../../../emerging/analysis/eval-product-proposal.md#open-forge-documents) defines the spec source.

## Images

The three images have separate jobs:

| Image | Contents and rules |
| --- | --- |
| `agent` | Pinned Codex and Claude tools, a fresh home per attempt and only the chosen provider's sign-in |
| `verifier` | Playwright `1.64.1-noble`, no credentials and no network after install. The build account can't read grading files |
| `assessor` | Simulated user and judge, each in a fresh instance with model tools turned off |

Images use locked digests. Containers have no host mounts, Docker socket or published ports. Files move by Docker copy. One attempt runs at a time by default. Up to five can run together after concurrency is tested. Browser checks always run one at a time.

The local comparison gives each game its own origin in a sandboxed frame. Games can't read each other's storage or the UI. Setup names stay hidden until Reveal. The [proposal's isolation rules](../../../emerging/analysis/eval-product-proposal.md#product) define these requirements.

## Stage contracts

Each stage gives the next stage frozen inputs or recorded evidence:

| Stage | Input | Output |
| --- | --- | --- |
| Materialize | Registered source, CLI package and setup definitions | Setup files, install evidence and content hashes |
| Check setup | Materialized setup and expected entry files | Setup delivery result outside the scored sample |
| Agent | Frozen case, setup, limits and answer bank | Frozen submitted files, transcript and usage |
| Verifier | Frozen files and registered checks | Build result, check statuses, screenshots and evidence |
| Judge | Filtered evidence and frozen rubric | Cited ballots and a separate collaboration result |
| Record | Stage outcomes, versions and artifacts | Run record and appended assessments |
| Publish | Finished batch and saved evidence | Sanitized records and checksummed bundles for staging |

Pipeline completion and functional success are separate results. A usable partial submission still reaches the judge. A grader failure doesn't become a game failure. Recheck and rejudge use saved files and append assessments. Their [status and counting rules](grading.md) keep the first result available.

The [run protocol](protocol.md) defines what crosses each stage. The [results contract](results.md) defines records and publication. The [proposal's delivery plan](../../../emerging/analysis/eval-product-proposal.md#delivery-phases) requires these contracts before dependent build work.

## Build

The product uses strict ESM TypeScript run directly by Node `24.19.0`. Type checking is a separate step. Tests use `node:test`. The UI keeps React, Vite and shadcn components.

The pilot runner is ported into these modules. Promptfoo is left out. An optional export can be added later if its viewer is wanted. The [approved product choice](../../../emerging/analysis/eval-product-proposal.md#approved) records this decision.

Back to [Evals](_evals.md).
