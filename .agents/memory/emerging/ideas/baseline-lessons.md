---
open-forge:
  description: What the first real Greenfield Baseline batch taught, kept as context and specs for Greenfield 2 and 3 and as fixes for the eval tool, never as changes to the Baseline itself
  tags: [Memory, Idea, Contextual, Candidate, Evaluation, Greenfield, Baseline, Specs, Harness]
---

# Baseline Lessons

Baseline Lessons collects what the first real [Greenfield Baseline](../../crystallized/documents/evals/neon-pairs/layer-1.md) batch taught us: which choices the agents made differently, and what broke in the eval tool.

The Baseline stays a prompt with no context, so none of this goes back into it. The maintainer asked on 2026-10-31 to keep these lessons as context for Greenfield 2 and 3, where the specs are the point. For example, no agent remembered the last grid size, because the prompt never asked for it. Greenfield 2 and 3 can state it in their specs and check whether each setup's agent follows it.

Status: recorded on 2026-10-31 from the first real batch: Codex `gpt-6.1-sol` at high effort, one run for each of the five setups, local isolation. Results per setup stay out of this note until the maintainer finishes the blind picks.

## What the agents did

Across all five runs:

- No agent asked a question, even with "If you have any questions, ask me" in the prompt. So the simulated user never answered, and Helpful didn't make the agent ask either.
- No game remembered the last grid size picked, which was one of the hidden wishes.
- Every game built offline into one `index.html`, at 250 KB give or take. Three wrote it to `dist/`, two to the project root. Packages ranged from 36 to 78.
- Every agent wrote its own tests or browser checks before finishing, and each run took 6 to 8 minutes.
- Every agent read some Open Forge files when they were installed, and ran the `open-forge` command, which came from the maintainer's machine instead of the pinned version.

## Specs for Greenfield 2 and 3

Each choice below varied between agents or went unmet. A spec settles it, and a check or the judge can then score it:

| Subject       | Spec to state                                                                                   |
| ------------- | ----------------------------------------------------------------------------------------------- |
| Grid size     | Start on 4x4, remember the last size picked, and offer only even sizes                          |
| Mismatch      | A mismatched pair turns back by itself after about a second, and clicks during it do nothing    |
| Match         | A matched pair stays face up and looks matched                                                  |
| New Game      | Starts over at once, even during the flip-back delay, and resets the moves                      |
| Moves         | One move per pair turned                                                                        |
| Best score    | Fewest moves, kept per grid size, shown for the current size                                    |
| Keyboard      | Tab reaches the grid, the arrow keys move between cards, and Enter or Space turns a card        |
| Accessibility | Each card has a name, such as "Row 1, column 1, face down", that says its face once it's up     |
| Faces         | 128 faces stay easy to tell apart at 16x16 on a phone, by shape or color, not only by a tiny mark |
| Look          | Cyan and magenta on near-black, a short celebration on a win, no sound                         |
| Build         | `npm run build` writes one `dist/index.html` with nothing else to load                          |
| Tooling       | Stop every dev server and watcher you start before you finish                                   |
| Tests         | Game rules covered by tests that run with `npm test`                                             |
| Code          | Each component keeps its logic, markup and styles together                                      |

Greenfield 2 gives a part of these, and Greenfield 3 all of them, in the same three places each time: a plain `docs` folder, `AGENTS.md` with links, and Open Forge's own scopes.

## What the eval tool needed

Each real game showed a design the checks hadn't allowed for. Every fix keeps the checks to what a player sees:

- Matched cards drawn in a matched style. A pair counts as matched when the game keeps it up, whatever it looks like.
- Mismatches that turn back only on the next click. A pair that stays up and then turns back is a mismatch after all.
- Hover and focus tints on a face-down card, so "still face down" allows more than "the same face".
- Cards in one container per row, found as the largest group of similar elements on the page.
- Cards rebuilt on New Game, found again whenever they're missing.
- Size controls as radio buttons, or labeled "6 by 6" with more text after it.
- A New Game button named after size buttons that also "start a new game".
- Keyboard grids where Tab reaches one card and the arrow keys move within the grid.
- Different faces that look alike to the checks, such as a triangle and a pentagon with the same color and pattern. The solver now never retries a pair the game turned back, and lets such a pair settle before the next click.
- A second click on the same card that puts it back face down. The check only requires that the card isn't paired with itself.
- A best score shown on the line below its label, after a size such as "Best on 4x4: 14", or near descriptions such as "A personal best for every grid". The checks read the number on the label's line, or a value-only line under it.
- Faces that differ only by a small mark, which screenshots can't resolve at 16x16. The check now fails only when more than 5% of cards share a face with two or more others, as when a set of faces repeats. Whether faces are easy to tell apart is for people and the judge.

The runner needed these too:

- A build helper left running held the agent's output open. A process now counts as done when it exits, and the tool stops anything still running from the run's folder.
- A stopped server left setup snapshots behind. Each snapshot now starts from an empty folder.
- Parallel runs, with the hidden checks one at a time, since they time real flips.
- Codex can stay alive after its turn while a dev server it started runs, which held two runs until the 30-minute limit. After Codex reports the turn complete, the tool stops the run's processes and ends Codex.
- The timer that ends a lingering Codex still fired after Codex exited normally, and stopped the offline build in the same folder. Three games went unbuilt. The timer now ends with Codex, and `npm run recheck -- <batch> <run> --rebuild` builds a saved project again from its lockfile.
- A folder Windows still had locked failed a run, and the error stopped the whole parallel batch. Deleting scratch folders is now best effort, and one run's error never stops the others.
- `npm run recheck`, which scores saved games again after the checks change.

## Not decided yet

- Whether Greenfield 2 and 3 keep the Baseline prompt word for word, or add "Follow the project's docs".
- Which part of the specs Greenfield 2 gives.
- Whether local runs get a clean `PATH`, or the evals move to Docker, so agents can't reach tools on the maintainer's machine.

Back to [Ideas](_ideas.md).
