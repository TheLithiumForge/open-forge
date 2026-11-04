---
open-forge:
  description: Plan to test whether code, plain docs, organized docs or scoped Open Forge make a difference, with Stage 1 on Neon Pairs with a large document pack and Stage 2 on a large multi-game app, and the Stage 1 spec with its planted rules and hidden checks
  tags: [Memory, Analysis, Contextual, Candidate, Evaluation, Brownfield, Documents, Routing, Proposal]
---

# Evals at Documentation Scale

## Can the evals tell code, plain docs, organized docs and Open Forge apart?

Not on Neon Pairs as it stood. The [full Sol run of 2026-11-04](eval-program-plan.md#full-sol-run-of-2026-11-04) found no effect beyond noise, for reasons in the project rather than the agents:

- Everything fits. Code and tests are about 70,000 tokens and docs about 11,000, against a 258,000-token window. Layer 4 runs peaked at 89,000 to 120,000 tokens and never compacted.
- The shipped tests give the rules away. Code only agents read about 16,000 tokens of test code per run and worked the rules out from it.
- The one real scenario gap was knowledge only the docs held: Code only failed P2's "Deal name" label.

What agents read also changed. Scoped Open Forge read 7.6K of source and 4.4K of search results per run, against 13.9K and 7.6K with plain docs. They spent that saving on 16.1K of framework, so totals matched at 53K and 55K. The framework's cost looks fixed and its saving looks like it grows with the project, which is the hypothesis this plan tests.

## What a valid test needs

1. **Docs too big to read.** More than the context window, so organization matters.
2. **Rules only in the docs.** Not in code or shipped tests, and some contradicting older code, so copying nearby code is wrong.
3. **The same facts in every docs scenario.** Only organization differs.
4. **A hidden check per rule, and at least 3 runs per cell.**

## Stages

### Stage 1: documents at scale on Neon Pairs

Keep the Neon Pairs code. Add about 300,000 tokens of real documentation with 30 planted rules, and one large task, Tournament mode. Run Code only, Plain docs, Organized docs and Full Core scoped, with one agent, 3 runs each. Accepted by the maintainer on 2026-11-04, with the spec below.

If the scenarios don't differ even here, that is a real result, reached cheaply.

### Stage 2: a large codebase, only if Stage 1 shows a difference

Grow the project into a multi-game app of 60,000 to 80,000 lines, at least three times the window. Add tasks that force compaction, such as a save-format migration across every game, and chains of fresh sessions that must follow earlier decisions. This tests what Stage 1 can't: code too large to read, memory across sessions and rules surviving compaction.

Open Forge's own repository is the tempting alternative, at about 13.3 million tokens with 102 archived tasks. It was built with the framework, which biases the result, and it is slower to build and check. Keep it as a later credibility check.

## Stage 1 spec

### Task

New case `l4-f2-tournament`, F2 Tournament mode. The request gives every scenario the same contract, so Code only can build the feature:

> Please add a Tournament mode for 4 or 8 players sharing one device. A Tournament button next to New Game opens it. Players choose 4 or 8 with a Players select, type names into inputs labelled Player 1 to Player 8, and press Start tournament. It's a knockout: in each match both players play the same deal, one after the other, and fewer moves wins. Show whose turn it is as "Now playing: NAME". A Start turn button begins each player's game. When a game ends a Next turn button appears, and when a match ends a Next match button appears. A Leave tournament button stops it early. Show the bracket with each match's players, their moves and time, and its winner, and "Champion: NAME" at the end with a Back to game button. Finished tournaments stay in a Past tournaments list on the Tournament screen after reload. Every Tournament screen has an Export tournament button that puts JSON in a Tournament JSON box, and a Load tournament button that reads that box back. Please follow the project rules.

### Scoring

Two groups, reported apart. Core checks need only the request, so every scenario can pass them. Rule checks need the docs. **Rule coverage is the test's main measure**, with cost, tokens by class and time beside it, and the frozen Layer 3 regression checks as a floor.

### Planted rules

Each rule lives in one document. Most never mention tournaments, because they are project-wide policies. An agent that searches for "tournament" finds the competition pages and misses the rest.

| ID | Document | Rule | Check |
|---|---|---|---|
| T01 | `competition-pairing.md` | Seeds follow entry order. Four players meet 1v4 then 2v3. Eight meet 1v8, 4v5, 3v6, 2v7 | Browser |
| T02 | `competition-rounds.md` | Rounds are named Quarterfinal, Semifinal and Final, shown as "Round: NAME" | Browser |
| T03 | `competition-boards.md` | Quarterfinals use 4x4, semifinals 6x6 and finals 8x8 | Browser |
| T04 | `competition-deals.md` | Both players of a match share a board code, and every match has its own | Browser |
| T05 | `competition-ties.md` | Equal moves go to the lower active time, then the higher seed | Browser |
| T06 | `competition-scores.md` | Tournament games never change best scores or the saved board size | Browser |
| N01 | `text-entry-policy.md` | Names are trimmed, inner spaces collapse to one, and text is NFC normalized | Browser |
| N02 | `name-length-policy.md` | A name has 2 to 20 code points, or "Use 2 to 20 characters." blocks the action | Browser |
| N03 | `duplicate-names-policy.md` | Names compare without case, or "Each name must be different." blocks the action | Browser |
| P01 | `adr-058-storage-namespace.md` | New stored data uses `np3:THING`. This supersedes `adr-011-storage-keys.md`, which the current code follows | Browser |
| P02 | `persistence-envelope.md` | A stored value is `{"v":3,"items":[...]}` | Browser |
| P03 | `retention-policy.md` | Lists keep the newest 10 records | Browser |
| P04 | `storage-failure-policy.md` | Blocked storage keeps working in memory and shows "Progress won't be saved on this device." | Browser |
| P05 | `corrupt-data-policy.md` | A malformed stored value is ignored without an error and stays untouched until the next save | Browser |
| E01 | `data-exchange-policy.md` | Exports carry `format` as `neon-pairs/KIND` and `version`, with keys in alphabetical order | Browser |
| E02 | `import-safety-policy.md` | An unknown field shows "Unknown field: NAME." and leaves the current state alone | Browser |
| A01 | `tables-policy.md` | Ranked or bracket data is a `table` with a `caption` and `th scope="col"` | Browser |
| A02 | `announcements-policy.md` | There is one live region, and "Now playing: NAME" goes through it | Browser |
| A03 | `focus-policy.md` | A screen change moves focus to that screen's `h2`, with `tabindex="-1"` | Browser |
| A04 | `dialog-policy.md` | Leaving unfinished work needs a native `dialog` with Keep playing, focused first, and Leave. Escape keeps playing | Browser |
| A05 | `target-size-policy.md` | Every new control is at least 44 by 44 CSS pixels at phone width | Browser |
| V01 | `motion-policy.md` | Under reduced motion new elements have no animation and no transition | Browser |
| V02 | `color-policy.md` | State is never color alone, so a winner's row also says "(winner)" | Browser |
| C01 | `voice-guide.md` | Interface text has no exclamation marks | Browser |
| C02 | `numbers-style.md` | Counts read "1 move" and "2 moves", and durations read M:SS | Browser |
| K01 | `randomness-policy.md` | Only `src/game/session/fresh-board.ts` may use a random source | Source |
| K02 | `time-policy.md` | Game code reads time only through the session clock, never `Date` or `performance` outside `src/game/session/` | Source |
| K03 | `module-layout-policy.md` | A feature lives in `src/game/FEATURE/`, with its rules in a `rules.ts` free of React and browser globals | Source |
| K04 | `dependency-policy.md` | A feature adds no dependencies | Source |
| K05 | `test-policy.md` | A feature's `rules.ts` has a test file beside it | Source |

### Document pack

About 300,000 tokens across about 200 documents in 12 scopes: the 10 existing specification documents, module specifications, 58 decision records with 6 superseded, project policies, competition pages, research notes, incident reviews, release notes and a glossary. Everything must be true of the code and consistent with the rules, and written as a real project would. Filler isn't lorem ipsum, since an agent has to tell what matters from what doesn't. A checker fails the pack if a rule's value appears outside its document, except for declared superseded records.

The scenarios use one source:

| Scenario | Layout | What the agent is told |
|---|---|---|
| Code only | No docs | The request |
| Plain docs | One flat folder with no links | Read `docs/README.md` and the other documents in `docs/` |
| Organized docs | Scope folders, an index and links | Read `docs/README.md` and follow its linked requirements |
| Full Core scoped | The same folders with entrypoints, descriptions and tags, routed by Open Forge | The same pointer |

### Clean start

The case's seed patch removes the distractors the first run found. It drops the README's browser download, formats the files that failed `format:check`, and steadies the screenshot test that timed out under load.

### Validity

- The starting project fails every core and rule check.
- A reference solution, written from the docs, passes every check. A second solution written independently also passes, so the checks don't demand one design.
- CI and local grades must match clause by clause on 3 runs before anyone trusts a batch.

### Run

`neon-pairs-f2-tournament`: Sol at xhigh, one agent, Code only, Plain docs, Organized docs and Full Core scoped, 3 runs each, 3 hours per run. Checks in CI. No AI judges, since they don't separate runs yet.

### How to read the result

Fixed before running, so the result can't be argued afterwards:

- A difference needs at least 15 points of mean rule coverage between two scenarios, with no overlap between their runs. Anything less is noise at 3 runs.
- Code only well below the docs scenarios shows the test can see doc knowledge at all. If it doesn't, the test failed and says nothing about the rest.
- Open Forge scoped against Organized docs answers the main question, on rule coverage first and cost second.
- If they match on both, routing adds nothing at this size, and that goes in the report as plainly as a win would.
