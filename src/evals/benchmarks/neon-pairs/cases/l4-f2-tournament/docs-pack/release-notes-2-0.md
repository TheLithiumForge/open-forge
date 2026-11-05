# Neon Pairs 2.0: a clearer specification

These release notes describe the move to specification version 2 and explain how explicit state, browser boundaries, and acceptance journeys made the existing game easier to verify.

The game implements spec 2.2, while its package remains version `1.0.0`. The 2.0 release consolidated the product contract without introducing a new serialization generation.

## The problem was ambiguity at the edges

By this point, the game could be demonstrated convincingly. The open questions appeared when demonstrations became reproducible checks. Did a miss wait for wall time or active time? Could a hidden page consume the remaining interval? Did importing a completed position count as winning it? Was a card disabled because it could not act, or missing from the keyboard route altogether?

These questions were expensive because several reasonable implementations could look the same during ordinary play. Version 2 made the boundaries explicit enough that a reviewer could distinguish them without guessing the author's intention. The [rules](rules.md), [input](input.md), [controls](controls.md), and [acceptance journeys](acceptance.md) became complementary views of the same behavior.

The release did not need a new reward system or a larger menu to justify its version number. Its practical benefit was fewer hidden interpretations. A player could leave the page during a miss, return, and continue the same observation interval. An engineer could run a deterministic sequence through the core and compare the result to the browser adapter.

## Pure transitions became the common vocabulary

The current core makes its state transitions direct. `createGame` starts a valid coded deal, `activateCard` processes one position, `advanceGame` advances logical active time, and `isWon` derives completion from matched positions. Those functions sit under `src/game` and operate without rendering a component or reading browser storage.

The selection union has three named cases: closed, one card, and mismatch. A successful second reveal returns to closed selection after adding its positions to `matched`. A failed second reveal stores both positions and remaining milliseconds. There is no independent “animation finished” state that can disagree with whether another card may be opened.

In the review, this was a better explanatory tool than a diagram of every component. Someone could ask what happens to a one-card selection after an ignored activation and get an answer from one branch. Someone could ask what happens when 900 logical milliseconds advance a miss and inspect a different branch. The [game-state model](game-state-model.md) expands on that representation.

## Browser time became an adapter concern

The browser has visibility, delayed tasks, focus, and a monotonic clock. The core has active milliseconds. The current session layer translates between them through `useSessionClock`, `elapsedStep`, and `tickSession`. That boundary lets a replay advance the same rules without simulating a browser timer queue.

The session's generation number also provides a useful identity for pending work. Replacing a deal increments the generation. A tick from another generation returns the session unchanged. The timer effect clears scheduled work and its visibility listener on cleanup, and callback checks guard against work that is no longer current.

This mattered most when starting another game during a mismatch. Clearing the visible selection was not enough if an earlier callback could later act on the new board. Version 2's contract described replacement as invalidating pending work, a behavior illustrated by the [clock incident](incident-clock-drift.md) and the [new-game guide](new-game-and-restart.md).

## Saved preferences and transported progress diverged clearly

The current preference store holds a size and separate best move counts for supported sizes. It catches read, parse, and write failures and leaves the game playable. It does not automatically retain the active attempt through reload. Snapshots provide the explicit progress transport, while board codes provide starting arrangements.

The specification review removed language that treated all of these as “saved game” data. That phrase was convenient in conversation but unhelpful in a bug report. A user who still sees a best after reload has retained a record, not necessarily an unfinished position. A snapshot copied to another browser carries progress but does not copy that browser's preference history.

Validation also has a boundary. The parser checks shape, numeric limits, board consistency, and the specified selection invariants. It does not certify a historical sequence of honest play. The absence of accounts or a trusted leaderboard is consistent with that local model. The [persistence overview](persistence-overview.md) and [import safety page](import-safety-policy.md) explain the current mechanics independently.

## One game could be exercised in three contexts

The core is used for browser play, snapshot reconstruction, and replay. A replay begins with a board code, advances active time to each action, activates its index, and finally advances to `endMs`. Equal timestamps preserve action-array order. A mismatch whose interval expires at an action time is closed by the advance before that action is considered.

That order was important because a browser timer may run late. The game rule is about whether the interval has expired, not whether a particular callback has already painted the closed backs. Current activation synchronizes time first, and replay applies logical time first. The same conceptual ordering makes an overdue miss resolvable before a new valid reveal.

Replay remains a developer command rather than another panel in the game. `npm run --silent replay -- PATH` prints a final snapshot on success and uses stderr with a nonzero exit for invalid input. The [replay format](replay-format.md) and [replay CLI decision](adr-015-replay-cli.md) describe this tool. Version 2's contribution was treating that path as another consumer of the rules, not as a second implementation of them.

## Acceptance became a journey

The team had a long list of isolated checks and a short list of full user paths. Version 2 made the latter easier to name. Finish a game; start again during a miss; enter and leave with the keyboard; restore unfinished progress; load a completed position; open the built HTML without a network.

These journeys connect details that unit tests naturally separate. A valid snapshot can still produce an unusable session if Resume loses focus. Correct card geometry can still leave the last card unreachable by touch. A successful bundle can still be the wrong artifact if a browser test silently uses an older build. The current package scripts build before browser checks, and diagnostic art and session entries are built separately from the game distribution.

The named tool combination in the README is a recorded compatibility context, not a substitute for these behaviors. It identifies Node, npm, Playwright, and Chromium versions, and says which checks passed there. Other browser environments need their own test records.

## Compatibility that remained intact

The current specification states that board codes, snapshots, and replays retain format version 1. Product revisions can tighten explanation or cover lifecycle edges without silently changing a code's deck. The format prefix and object version fields therefore answer different questions from the product specification number.

The next two releases are deliberately smaller. [2.1](release-notes-2-1.md) focuses on reduced motion and visual feedback. [2.2](release-notes-2-2.md) describes the current evidence and edge-case closure. Together they show why a compact game can benefit from a precise contract without turning into a large application framework.
