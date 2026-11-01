# Rules
Rules is a specification that defines requirements for Neon Pairs.

## Board

- R01.1 - Support side lengths 4, 6, 8, 10, 12, 14 and 16.
- R01.2 - Each board is square.
- R01.3 - Every face occurs exactly twice.

## Turn

- R02.1 - Reveal the first eligible card.
- R02.2 - Reactivating that card has no game effect.
- R02.3 - Activating a matched card has no game effect.

- R03.1 - Reveal a different unmatched card on the second valid activation.
- R03.2 - Keep a matching pair revealed.
- R03.3 - Add one matched pair immediately.

- R04.1 - Show mismatching faces for 800 active milliseconds.
- R04.2 - Close both cards together when that interval ends.
- R04.3 - Ignore every card activation during the interval.
- R04.4 - Pause the interval while the page is hidden.

- R05.1 - Add one move on the second valid reveal until the move cap is reached.
- R05.2 - The first reveal adds no move.
- R05.3 - Ignored activation adds no move.
- R05.4 - A match and a miss each cost one move until the move cap is reached.
- R05.5 - Stop increasing the move count at 999999.
- R05.6 - Play continues normally at the move cap.
- R05.7 - Pairs still match at the move cap.
- R05.8 - The game can still be won at the move cap.

## Completion

- R06.1 - Give one point per matched pair and no other score.
- R06.2 - Show matched pairs against the total throughout play.
- R06.3 - Complete the game when every pair is matched.
- R06.4 - Show final moves in the completion banner.
- R06.5 - Show final elapsed time in the completion banner.
- R06.6 - Apply the completion motion in [V18](visuals.md#completion).

## Clock

- R07.1 - Start elapsed time on the first valid reveal.
- R07.2 - Count active milliseconds while the page is shown.
- R07.3 - Pause elapsed time while the page is hidden.
- R07.5 - Stop elapsed time at completion.
- R07.6 - Rendering frequency doesn't change elapsed time.
- R07.7 - Use the same active time for the clock and mismatch interval.
- R07.8 - Continue a hidden-page mismatch from its remaining time when the page is shown.
- R07.9 - Cap elapsed time at 359999999 milliseconds.
- R07.10 - Play continues normally at the elapsed-time cap.
- R07.11 - The clock shows the capped elapsed time.

## Session

- R08.1 - Start at 4x4 when there is no supported saved size.
- R08.2 - Use a supported saved size on reload.
- R08.3 - Remember the last chosen size after reload.
- R08.4 - Start a fresh deal on page load.

- R09.1 - New Game starts another deal at the current size.
- R09.2 - Restart repeats the same deal.
- R09.3 - Changing size starts another deal at that size.
- R09.4 - Each of these actions clears selection, matches, moves and elapsed time.

- R10.1 - Record only completed games as a best score for their size.
- R10.2 - Replace the best only with fewer moves.
- R10.3 - Keep the best after reload.
- R10.4 - Equal or worse results keep the saved best.
- R10.5 - Starting or restarting keeps best scores.
- R10.6 - Show the current size's best moves during play and after completion.
- R10.7 - Show Best: none before a size has a record.

- R11.3 - Ignore invalid saved preferences.
- R11.4 - Continue playing without saving when storage is unavailable.

- R12.1 - Starting, restarting, replacing or leaving a game invalidates pending timer updates.
- R12.2 - An earlier game or turn's update can't change the current game.

