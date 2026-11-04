---
open-forge:
  description: How another agent or machine runs the Neon Pairs evals from a fresh clone, what is finished, and what to fix before the next run
  tags: [Memory, Working, Contextual, Active, Evals, Handoff]
---

# Eval runs handover

## Goal

Run the Neon Pairs evals from a fresh clone, publish them and grade them, with the same results a pinned runner on the maintainer's PC gives. The [run guide](../../../src/docusaurus/docs/evals/run.md) and [work guide](../../../src/docusaurus/docs/evals/work.mdx) define the commands and terms. This record adds what the guides don't: the state of the work, what a cloud session can do, and what went wrong the first time.

## Where it stands

State on 2026-11-04.

- Code is on branch `feature/demo-evals`, pushed. No pull request exists yet. Pull requests target `develop` with the `ai-automation` label, and nothing is pushed to `main` or `develop`.
- The results repository `TheLithiumForge/open-forge-eval-results` is the `src/evals/results` submodule. Direct pushes to its `main` are allowed.
- The full Sol run has 182 runs. Agent work, publishing and checks are finished for all of them. L3's AI judges are running on the maintainer's PC, in a clean runner at `evals-runner-10`.
- Layer 4 isn't judged, on purpose. Its small fixes score about 100% everywhere.

## What a cloud session can do

From the [cloud environments page](https://code.claude.com/docs/en/cloud-environments), read on 2026-11-04:

- Each session is an Ubuntu 24.04 VM of about 4 vCPUs, 16 GB of memory and 30 GB of disk, with Docker installed and the Anthropic API, npm, GitHub and Docker registries reachable on the default network level.
- Foreground commands stop after 10 minutes at most. Background commands get up to 30 more minutes, and an idle VM pauses and can be reclaimed, which ends background work. No maximum session length is stated.
- The GitHub proxy rejects GraphQL, so `gh pr` fails.
- Usage is shared with the rest of the account.

Not verified, so the smoke test below finds out:

- Whether `dockerd` runs, or starts, and whether the eval stages' strict containers work inside the VM.
- Whether agent containers reach their model through the VM's network. OpenAI's hosts aren't on the default list, so Codex needs the Full or Custom level.
- Whether a session can push to the results repository, and whether `gh workflow run` passes the proxy.

What fits one session, from the first run's times: a run of a whole packet (15 to 20 minutes) or one Sol alone L3 build (27 minutes median). These don't fit: Sol teams on L3 (45 to 107 minutes), task by task (about 60 minutes in one process, and its steps can't be split across sessions), and the full run (7 hours on a 12-core PC). For those, use a machine with Docker that stays on, such as the maintainer's PC or a rented VM over SSH.

## Step 0: go or no-go

Stop and report at the first failure. Don't work around a container restriction: agent-written code runs only in containers.

```sh
docker info
git submodule update --init src/evals/results
npm ci --prefix src/evals
npm ci --prefix src/evals/results
npm run evals -- images
npm run evals -- run --plan src/evals/experiments/neon-pairs-p1-smoke.json
```

The smoke plan is one agent fixing four bugs, about 5 minutes on an idle machine and about 90,000 new input tokens. Go means its batch finishes, its checks have no errors and its repair checks pass. On the first run it scored 23 of 23.

## Sign-ins

Never commit, print or log these. `src/evals/.env.local` and `src/evals/.auth/` are ignored by Git.

- Claude: `CLAUDE_CODE_OAUTH_TOKEN` from `claude setup-token`, or `ANTHROPIC_API_KEY`, in `src/evals/.env.local` or the environment.
- Codex: `src/evals/.auth/codex/auth.json`, or `OPENAI_API_KEY` or `CODEX_API_KEY`. Cloud environment variables are readable by anyone using that environment. Prefer an API key over copying `auth.json` to several sessions, since each copy can refresh the sign-in and may invalidate the others.

## Plans

Each runs with `npm run evals -- run --plan src/evals/experiments/<plan>.json --scratch <folder>`. All leave checks and judges for later.

| Plan | Runs | What it is |
|---|---|---|
| `neon-pairs-p1-whole-packet` | 14 | Four bugs handed over together, Sol alone |
| `neon-pairs-p1-task-by-task` | 56 | The same bugs one at a time, in one process |
| `neon-pairs-p1-orchestrated` | 28 | The same bugs through subagents, Sol leading Sol and Luna |
| `neon-pairs-p2-whole-packet` | 14 | A feature and a bug together, Sol alone |
| `neon-pairs-p2-orchestrated` | 28 | The same through subagents |
| `neon-pairs-l3-sol` | 42 | A whole game from a full spec, all three implementers |

`--setups` narrows a plan to some scenarios, and `--parallel` sets how many run at once. The first scenario in a plan is its control, so it stays.

## After the agents finish

1. Publish each batch: `npm run evals -- results add <batch>`. Then, in `src/evals/results`, `git add --force versions index.json`, commit and push `main`. The force is needed because agents' own `.gitignore` files sit inside the evidence.
2. Grade in CI: `npm run evals -- results grade`, from a pushed commit. It checks each run in its own job, 20 at a time.
3. **Read the check records.** A green job only means every clause was graded. Re-grade 3 runs with `check <batch> <run>` on a local machine and compare clause by clause. They must match exactly before anyone trusts the batch.
4. Judge on a machine with a sign-in, never in CI, because those jobs build agent code. Use a clean store, since CI's grades live in the results repository and a batch can't be imported twice:

   ```sh
   npm run evals -- results import src/evals/results/versions/<version>/batches/<batch>
   npm run evals -- judge <batch> <one run> --missing
   npm run evals -- judge <batch> --missing --parallel 20
   npm run evals -- results add <batch> --imported --repository <results repository>
   ```

   Read the first run's ballots before judging the rest. A run takes about 20 minutes.

Never publish from a store without CI's grades: adding a batch replaces it, so its grades would be lost.

## Traps the first run hit

- GitHub's slower machines lost races with the page, so four clauses failed on CI and passed locally. It's fixed, and step 3 above is how it was caught.
- Stopping a background job or letting a monitor expire doesn't stop its script on Windows. Find leftover processes by command line.
- Docker's default address pools hold about 31 networks. Stages overflow to `10.213.x.0/24`. Never bare-prune eval networks while anything runs.

## Fix before the next run

Open and unaccepted. The maintainer hasn't chosen yet.

- The task project's README tells agents to download a browser the image already has, so about half the Layer 4 runs spend 6 to 9 minutes on it and their times aren't comparable.
- P2's check demands the label "Deal name", which only its docs state, so Code only fails 13 clauses for a working feature in all 6 runs. Treat P2 Code only's score as a grading artifact until the request names the label.
- Most runs report formatting failures in files they didn't touch, and some edit those files out of scope.
- A screenshot test times out under load in about 1 run in 6.
- Add an idle watchdog. No working agent went quiet for more than 2.6 minutes, but one hung agent held a slot for 2 hours 45 minutes.
- Three of the judges' 11 L3 questions can't be answered from what they're shown, which caps judge scores at about 73%. Judge records also store no tokens, so judging can't be priced.
