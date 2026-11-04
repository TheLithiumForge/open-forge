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
2. The Grade published runs workflow finds the waiting batches, gives each a runner from `GRADE_RUNNERS` in turn, imports it into an Open Forge checkout pinned by `OPEN_FORGE_REF`, runs `check --missing` and republishes it with `--imported`.
3. A publish job copies only the expected grades under append-only rules, validates and opens one pull request, then dispatches validation on its branch.
4. After the merge, the Evals pages show the grades on their next open.

## Changes before the first push

Made on 2026-11-04 in the results repository's `ed099c1` and the main repository:

- The catalogue no longer lists a batch that was never published. It moved to `D:\eval-workspaces\results-unpublished`.
- `versions/** -text` keeps evidence bytes equal to their hashes.
- Grading pins its graders to a required commit, ignores pushes that only add assessments, force-adds evidence that agent `.gitignore` files would hide, dispatches validation on the grade branch and never runs judges beside agent code.
- The main docs workflow checks out the results submodule and rebuilds when its pin moves.

## Still open

- Push the results repository before the main repository, since the main one pins results commits that aren't public yet.
- In the results repository settings, allow Actions to create pull requests and turn the old Pages site off.
- Set `OPEN_FORGE_REF` to a public Open Forge commit that has `results import` and `results add --imported`.
- Keep self-hosted runners out of pull request workflows, wipe their Docker state between jobs and limit their runner group to this repository.
- Skip batches that already have an open grade pull request, so overlapping runs don't conflict.
- Watch repository size. One batch of 12 runs took about 17 MB. Before it nears 1 GB, move screenshots to release assets referenced by hash, and add a summary file so the pages make fewer requests.
