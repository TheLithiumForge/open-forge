# Neon Pairs 1.0

These are release notes for the first Neon Pairs release, explaining the original play loop and the choices that survived into the current game.

Product release numbers are separate from the package's `1.0.0` and the current specification's 2.2.

The first useful release was a complete attempt. Open the page, reveal cards, find every pair, inspect the result, and begin again. There was no account setup or separate results destination between opening the game and playing it.

## What landed

- A two-reveal turn: open one eligible card, then a different unmatched card.
- Immediate matches that stay visible in their original positions.
- A short observation interval for misses, followed by both cards closing together.
- Progress expressed as moves, found pairs, and elapsed time.
- A final result that remains on the page.
- A local best move count as a reason to try another attempt.

The small four-column board supplied the release's first impression: sixteen positions, eight pairs, and a plausible finish in a short sitting. That historical emphasis does not limit today's game to one size. The complete range is covered in the [large-board release](release-notes-1-2.md).

## A click was not a move

The early review used “turn” loosely enough that people disagreed about what the counter meant. The lasting definition became concrete: the second eligible reveal adds a move. Opening the first card prepares the attempt. Clicking that same card again has no effect, and selecting a matched card has no effect. A correct and incorrect pair each cost one move.

This choice made the result understandable without a separate reward formula. A person could improve by remembering positions without being charged for a harmless repeated activation. Pair count answered how much remained; moves answered how many pair attempts had been used. Elapsed time described the attempt rather than adding another weighted score.

The current transition is in `src/game/game.ts`. `activateCard` handles the closed, one-card, and mismatch selection cases directly. It adds matched positions when deck values agree and stores a pending miss when they differ. Current caps are described in [2.2](release-notes-2-2.md); the basic counting meaning has stayed small. [Moves and scoring](scoring-and-moves.md) gives the present rule reference.

## The pause was information

An early sketch closed an incorrect pair as soon as the next card was selected. That looked quick in a demonstration, but observation time then depended on how quickly someone acted. Another sketch required a separate dismissal. That gave unlimited inspection but added an action unrelated to choosing a pair.

The surviving design uses a short automatic interval. Today's value is 800 active milliseconds, with further card activation ignored during the miss. The interval gives the player a chance to connect two identities with two positions. It is part of remembering, rather than a penalty animation.

Matches remain visible at full opacity. They do not disappear and cause the remaining cards to move inward. A green border and check distinguish solved positions, preserving the coordinates the player is trying to remember. The [mismatch page](mismatch-interval.md) and [card artwork](card-artwork.md) describe the current behavior in detail.

## A result worth looking at

In the release review, people paused after the final pair before choosing another action. A transient success message would have required them to catch the exact moment. The current completion banner retains moves and elapsed time after its optional pulse, while all solved cards remain in place.

Silence was deliberate. Visible state and accessible result feedback carry the event information; no sound toggle was added to make a silent game appear configurable. The geometric artwork also avoided making color the only identity cue. Today's complete catalogue combines shape, marker direction, and fill, with palette reinforcing the marker distinction. See [Sound](sound.md) and the [face catalogue](face-catalogue.md).

The local best was intentionally modest: a lowest completed move count for each size on the current browser origin. It did not imply an account, device synchronization, or automatic restoration of an unfinished board. Today's preferences retain size and best moves; a reload starts a fresh deal. Explicit progress transport arrived in the [snapshot release](release-notes-1-4.md).

## What the release did not settle

The first pointer demonstration concealed gaps that became visible only when someone tried to navigate without a mouse or leave during a pending miss. A board could look complete while lacking a clear keyboard exit, and a timer could appear correct while the foreground tab was quiet.

Those questions became later work, rather than reasons to enlarge the first release's interface. [1.1](release-notes-1-1.md) follows keyboard access, while the [clock incident](incident-clock-drift.md) explains a lifecycle failure. The durable first-release outcome was a recognizable loop with an obvious end: reveal two eligible cards, learn from the result, and continue until every pair is found.
