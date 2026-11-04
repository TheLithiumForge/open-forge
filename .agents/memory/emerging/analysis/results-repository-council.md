---
open-forge:
  description: Council record from 2026-11-04 on the public results repository's shape and the CI grading pipeline, keeping it a data store with the main site as the only results view, with the changes made before its first push
  tags: [Memory, Analysis, Contextual, Candidate, Evaluation, Results, Publishing, CI]
---

# Results Repository Council

## What shape should the public results repository take, and how should grades reach readers?

The maintainer asked on 2026-11-04 whether `TheLithiumForge/open-forge-eval-results` should keep a Docusaurus site of its own or port the results UI there, with checks run in GitHub Actions on GitHub's runners and two or three self-hosted ones. Its public `main` held one commit from three days before, with an old `site/` and a Pages workflow. The local copy was 13 commits ahead and had removed both, keeping the [results](../../crystallized/documents/evals/results.md) decision of 2026-11-02.

## Method

Three read-only members with different personas answered one brief: the CI Engineer (pipeline safety and parallel runners), the Reader Advocate (one view, fresh numbers) and the Steward (maintenance cost and one source of truth).

## Recommendation

All three chose the same shape: keep the results repository a data store, with records, schemas, the validator, evidence, `index.json` and the validation and grading workflows, and show results only on the main site's Evals pages.

- **One view.** The old `site/` had its own results page, panels and analysis code beside `src/components/evals/`, so two interfaces would drift.
- **Freshness needs no second site.** The Evals pages already read any public commit through `fetchLatest`, so they now fetch the latest results on open.
- **Less attack surface.** A Pages deployment in the repository that receives pull requests built from agent-code jobs adds permissions for nothing.

## Pipeline

1. `npm run evals -- results add <batch>` publishes a batch whose checks were left for later, and the maintainer pushes it.
2. `npm run evals -- results grade` starts the Grade published runs workflow from the Open Forge repository with the graders of its pushed commit. The workflow builds the verifier image once or pulls it from GitHub's container registry, then checks each waiting run in its own job on GitHub's Ubuntu runners, up to 20 at a time, and republishes its batch with `--imported`.
3. A publish job copies only the expected grades under append-only rules, validates and pushes them to `main`.
4. The Evals pages show the grades on their next open.

The council proposed a pull request per grading run. The maintainer chose on 2026-11-04 to start grading from the Open Forge repository and push the grades straight to `main` instead. No review sits between a grading job and `main` now, so the append-only rules are the only guard against a job that ran agent code.

## Changes before the first push

Made on 2026-11-04 in the results repository's `ed099c1` and the main repository:

- The catalogue no longer lists a batch that was never published. It moved to `D:\eval-workspaces\results-unpublished`.
- `versions/** -text` keeps evidence bytes equal to their hashes.
- Grading runs only when started with a commit for its graders, force-adds evidence that agent `.gitignore` files would hide, pushes to `main` with a rebase when it moved, and never runs judges beside agent code.
- The main docs workflow checks out the results submodule and rebuilds when its pin moves.

## Still open

- Push the results repository before the main repository, since the main one pins results commits that aren't public yet.
- In the results repository settings, turn the old Pages site off, and let GitHub Actions push to `main` if it's protected.
- The maintainer chose GitHub's runners over the self-hosted ones on 2026-11-04, since public repositories run 20 jobs at a time for free. Make the verifier package public after its first push.
- Grading runs one at a time, so two runs never push grades for the same batch.
- Watch repository size. One batch of 12 runs took about 17 MB. Before it nears 1 GB, move screenshots to release assets referenced by hash, and add a summary file so the pages make fewer requests.
