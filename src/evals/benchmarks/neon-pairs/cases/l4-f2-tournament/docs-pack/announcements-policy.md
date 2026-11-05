# Announcements policy

This page describes how event feedback is published, repeated and kept separate from routine game rendering.

The game has many visible updates, but only some represent a new result. A clock tick is a changing value; resolving a turn is an event. Treating those as the same kind of update would make a memory game noisy and would bury the information a player needs after selecting two cards.

## Publication point

`Game.tsx` places the game result between the board instructions and the Board itself. Its paragraph has `role="status"`, `aria-live="polite"` and `aria-atomic="true"`, with the current feedback inside a span. The page has exactly one live region, the status paragraph in Game.tsx. New status text, such as whose turn it is, goes through that region. The current game publishes pair results and load outcomes there; it has no turn-owner feature.

The paragraph remains mounted while its content changes. It is inside `.game-result`, which has a minimum height of `56px`, flex alignment and margins. The wrapper reserves a readable area near the board without making the scrollable card content itself the announcement source.

Polite publication allows the browser and assistive technology to schedule the result around other speech. Atomic content indicates that the paragraph's result is treated as a complete update rather than an isolated changed number. These attributes express the intended exposure; the actual spoken experience still needs user acceptance with a screen reader.

## Session data behind the paragraph

The session owns `feedback` and an integer `announcement`. Game renders `<span key={session.announcement}>{session.feedback}</span>`. Updating the key replaces the child span even when its text is identical to the preceding result. This gives repeated events a DOM change without adding a hidden counter to the spoken text.

The counter is event identity, not a statistic presented to the player. It does not track moves, matched pairs or elapsed seconds. A failed load can advance it without adding a move, while the first card reveal changes game state without advancing it.

A broad render counter would be wrong for this job. Session renders occur for time and selection changes that should not repeat the last turn result. The explicit event field makes the distinction inspectable in `session.ts` and `use-game.ts`.

## Turn results

`activateSession` calls the pure `activateCard`. If the game object is unchanged, it returns the current session. If the result has selection kind `one`, it returns the updated game without changing feedback or announcement identity. That covers the first eligible reveal.

After the second eligible reveal, the result is either a mismatch or a match. The session builds the appropriate text and increments `announcement`. Completion is checked at this same boundary, so the winning match gets a combined result rather than two competing event publications.

| Result | Text produced by the session |
| --- | --- |
| First matched pair | `Pair found. 1 pair matched.` |
| Later nonfinal pair | `Pair found. M pairs matched.` |
| Mismatch | `No match. Cards close after a short pause.` |
| Winning pair | `Pair found. All pairs found in N moves and time.` |

Here `M` and `N` are replaced with the actual counts, and `time` is the output of `formatElapsed`. The singular branch is based on `game.matched.length === 2`. The final branch takes precedence over the nonfinal pair count.

A card's accessible name also changes when it reveals or matches. That is the focused control describing its own state, not a second turn-result producer. The text in the event paragraph summarizes what the two-card attempt achieved. [Screen reader notes](screen-reader-notes.md) follows those two sources in a complete game.

## Consecutive identical results

Two misses in a row produce the same sentence. Updating only a string state can leave the DOM text unchanged, which gives no new mutation for the second event. The keyed span avoids relying on a different visible sentence to distinguish them.

The current sequence is concrete. Initial feedback is empty. Revealing index 0 on `np1-4-1` changes selection to `one` without publishing. Revealing index 1 produces a mismatch and advances the event identity. After the interval closes, choosing those two indices again produces the same text with a later identity.

The counter advances once for each resolved attempt in that sequence, not once for each card action or timer update. A third activation during the mismatch is ignored by the game and leaves the event unchanged. The same is true of a repeated activation on the first revealed card.

There is no temporary blanking timeout between identical results. The implementation does not clear text, wait a guessed number of milliseconds and restore it. That avoids adding announcement timing to the game clock or introducing a second lifecycle that can outlive a replacement game.

## Changes that stay quiet

`tickSession` advances the game and returns a new session with that game, leaving feedback and announcement fields intact. Mismatch closure is part of this advancement. It does not publish a new “cards closed” result. The earlier mismatch sentence already told the player that closure would follow a short pause.

Arrow movement is local board focus state. It does not call the session feedback path. Clock renders update Time in GameStatus, while card memoization prevents unchanged cards from rendering again. Neither operation is a new turn result.

New Game and Restart call `replaceGame` with `imported: false`. That replacement clears the prior feedback and load notice while keeping the announcement number. A successful board-code load follows the same replacement path. These operations clear old context; the current source does not add a success sentence for a board-code load.

Clearing text is still a DOM change and may appear in a mutation trace as an empty result. It is different from republishing a turn sentence. The accessibility journey's expected record includes the new-game clearing event, so a raw count of every mutation is not a substitute for reading what each event contains.

## Load outcomes share the publication path

The controller's `notice(feedback)` helper assigns both `feedback` and `loadNotice`, then increments `announcement`. Invalid Board code publishes `Invalid board code.`; invalid Snapshot JSON publishes `Invalid snapshot.`. Repeating the same invalid submission is a new load attempt and advances the identity again.

For valid snapshot import, `replaceGame(..., true)` increments the identity and publishes either `Loaded snapshot. Select Resume to continue.` or `Loaded completed snapshot.`. The former corresponds to a paused unfinished game, the latter to completed progress. Valid board-code loading does not use the imported-snapshot branch.

Reproduction also renders `loadNotice` in its ordinary paragraph with id `load-notice`. The load buttons reference it through `aria-describedby`. That keeps the outcome visible beside the text tools and associates the description with their actions without creating another event publisher in that panel.

Resume clears feedback and loadNotice without increasing the event identity. It also transfers focus to the board before its button disappears. [Reproduction panel](reproduction-panel.md) and [Focus policy](focus-policy.md) describe those effects independently of event publication.

## Completion and persistent reading

GameStatus shows a completion paragraph whenever `isWon(game)` is true. A newly won game can give it the celebration class; an imported completed game shows it statically. That paragraph is persistent reading content containing the final moves and time.

The winning turn's feedback already contains the match and win in one result. The status paragraph does not need to separately announce a matched pair, then publish a second completion sentence. This avoids having two updates compete at the most significant turn in the game.

A completed snapshot instead publishes its load outcome, because import is the event that just occurred. The game is visibly complete, but no new card match happened. This distinction preserves a useful account of what the user actually did.

## Automated evidence

`session.test.ts` checks that first reveal, mismatch closure and repeated mismatch events affect announcement identity as expected. It also checks import outcomes and completion transitions without involving the browser.

`tests/announcements.e2e.test.ts` observes DOM mutations in the status paragraph. It resolves a mismatch, moves focus, runs the mismatch interval and repeats the same attempt. The expected result history contains the mismatch sentence once for each resolved turn, with no extra result from movement or closure.

The same test submits each snapshot load kind and each invalid-load kind twice, checking the publication history and the visible load notice. `tests/status-controls.e2e.test.ts` checks that the result remains beside the board during large-phone play and that the reproduction panel has no competing result role.

The accessibility journey reads Chromium's tree for the polite and atomic properties and the result text at each turn. That establishes exposure in the tested runtime. Spoken scheduling and verbalization are assessed in user acceptance. The repository records spoken review as user acceptance.

## Diagnosing a repeated or missing result

A missing second identical result suggests checking event identity before changing the sentence. A repeated result on every clock tick suggests checking whether routine advancement touches the publication fields. A load notice visible near the textarea but absent from event output suggests checking whether the controller used `notice` or only changed local text.

For a report, the useful sequence is the action, feedback string, announcement number and observed mutations. That evidence can show whether the session failed to publish, React had no relevant change or the browser exposed the expected update but a spoken review behaved differently. The policy gives a single publication boundary; diagnosis follows it from the event to the paragraph rather than adding more announcers around the page.
