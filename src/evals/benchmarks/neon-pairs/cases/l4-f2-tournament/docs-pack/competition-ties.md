# Comparing tied competition results

This page describes competition result comparison and explains the active-time information available from the current game.

A result comparison uses information from completed play. The game core can tell whether a board is complete and can expose its moves and elapsed time. It does not compare two participants or contain a bracket seed, so the competition decision here sits outside the current single-game transitions.

## Comparison order

Moves are the first useful distinction between two completed attempts, but they can be equal. When both players of a match finish in the same number of moves, the lower active time wins, and if that is also equal the higher seed wins. Seed 1 is the highest seed. The comparison is ordered: an earlier distinction settles the result before a later one is considered.

These example results illustrate the first two levels. The elapsed values are milliseconds in game data.

| Participant | Moves | Active elapsed milliseconds | Result in this example |
| --- | --- | --- | --- |
| Ada | 12 | 70200 | Ahead on moves |
| Bo | 13 | 58100 | Faster, but more moves |

| Participant | Moves | Active elapsed milliseconds | Result in this example |
| --- | --- | --- | --- |
| Cora | 15 | 65200 | Ahead on active time |
| Dev | 15 | 65900 | Same moves, greater active time |

The examples deliberately separate efficiency from speed. An implementation that adds moves and seconds together, or sorts by time first, would produce a different comparison. There is no weighted combined score in the existing core or in the decision above.

## When every recorded value looks equal

Two equal move counts and equal active times lead to the seed branch described above.

A fixture with two completely equal game results exercises the seed branch, whereas examples with unequal times never reach it. [Competition pairing](competition-pairing.md) supplies the bracket context.

The seed settles the equality case without an extra round of play, shared outcome or random choice. The existing code knows nothing about an opponent or an entrant's seed.

## Active time in the current game

`GameState.elapsedMs` is advanced by `advanceGame` in `src/game/game.ts`. That function does nothing before play has started or after `isWon` becomes true. The first valid reveal sets `started: true`; ignored activations do not create another start event.

In the browser, `src/game/session/use-session-clock.ts` observes `performance.now()`. Its `running()` check combines visibility, pause state, the started flag and completion. `elapsedStep` in `browser-clock.ts` calculates the interval since the previous observation and contributes it only when active. This is elapsed active play, not a stopwatch started when the HTML file was opened.

The clock hook synchronizes before applying a card activation. As a result, the final valid activation is evaluated after accounting for the elapsed interval leading up to it. The source comment on snapshot export also points out that rendered state can trail the clock, which is why export synchronizes before encoding.

The 40ms scheduling constant is an update cadence, not the unit of the stored elapsed value. `performance.now()` can return fractional milliseconds, and the core preserves positive finite deltas. `browser-clock.test.ts` exercises fractional values and compares many observations with one observation over the same overall span.

## Display precision is a separate concern

`formatElapsed` in `src/game/elapsed-time.ts` discards sub-second detail for presentation. It is therefore possible for two visibly identical clock strings to correspond to different `elapsedMs` values. A screenshot of the ordinary status area cannot establish exact equality of active time.

The example with Cora and Dev demonstrates this issue: their stored times differ by 700ms even though a coarser presentation can hide that difference. Reading the rendered text back into a result comparison would throw away information the game already holds.

Snapshot serialization changes precision too, although less dramatically. `encodeSnapshot` floors `elapsedMs` to a whole millisecond. It rounds a pending mismatch's remaining interval upward so that interval remains representable. The game in memory may have fractional time that the snapshot no longer contains.

Current in-memory values, exported values and visible text have different precision, so an implementation review needs to identify which representation a prototype actually compares. Treating these as interchangeable would make exact equality depend on an undocumented conversion.

## Pauses and interruptions

The current browser session excludes hidden-page time and time spent waiting for Resume after an unfinished snapshot import. It does not offer an ordinary manual pause control. Sitting with the visible started board while talking still contributes active elapsed time, even when no cards are activated during that conversation.

A miss also contributes time. The two exposed nonmatching faces remain visible for 800 active milliseconds, during which activation is blocked. That wait is part of the same active timeline as elapsed play. Hiding the page pauses both the elapsed clock and the remaining mismatch interval.

These facts explain some apparent discrepancies in human timing. An observer measuring from the moment the device is handed over will include time before the first reveal. A wall stopwatch will include time while the page is hidden. A person looking only at clicks may overlook the visible thinking intervals between them. Those observations can all differ from `elapsedMs` without demonstrating a clock defect.

[Elapsed time rules](elapsed-time-rules.md), [clock design](clock-design.md) and [mismatch interval](mismatch-interval.md) describe the existing mechanisms.

## Completion and boundary values

`isWon` checks whether matched positions cover the deck. Before that condition holds, a board can have many matched pairs and a plausible-looking move count while still being unfinished. A current status reading is not equivalent to a completed result merely because the player has stopped interacting.

The core caps moves at 999999 and elapsed time at 359999999ms. Matching and completion continue at those caps. These limits can increase the number of equal numeric values in artificial boundary fixtures. They do not create an uncapped hidden counter from which a comparison could recover a distinction.

`src/game/move-cap.test.ts` covers completion at the move boundary. The browser and core clock tests cover elapsed behavior separately. Those tests are evidence about single-game values; there is no competition result comparator test in the current source.

## Useful evidence for a disagreement

A disputed outcome can involve several separate questions: whether both attempts completed, which values were captured, how those values were compared and whether the participants understood the result. A screenshot may help with the last question, but it is weak evidence for sub-second equality. A snapshot gives structured game values but applies the integer export precision described above.

A replay can reconstruct card actions on a logical active timeline. In the current core it begins with a code, advances time before each action and advances again to its ending timestamp. It provides no record of the human circumstances around those actions. [Replay format](replay-format.md) explains the available evidence without turning it into a competition log.

For a prototype review, the move-first example, the time-second example and an exact-equality example exercise distinct branches. Together they are more informative than many examples where one participant is better on every measure. They also make comparison errors visible before a polished result screen hides them.
