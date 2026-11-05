# ADR 050: Complete the board in place

This record explains the persistent completion summary and the distinction between a played win and loaded completion.

## Decision

Completion adds a visible banner to the existing game instead of moving to another screen or hiding the board. It states final moves and elapsed time. The matched cards remain available, and the ordinary game controls remain in place for starting or repeating a deal.

A played win receives one 600ms pulse, then the banner stays static. Under reduced motion it is static immediately. A completed snapshot also has a static completed presentation. The summary's informational role is independent of whether a celebration effect is selected.

## Deriving completion

`isWon` compares the number of matched positions with the deck length. There is no separately mutable win flag to disagree with progress. `GameStatus` uses that function to render the completion paragraph and the ordinary statistics. Pair progress therefore reaches the total through the same state that causes the banner to appear.

`Session.celebration` carries the distinction between a newly played completion and other completed state. An activation that wins sets it; replacing the game resets it. `GameStatus` appends the `celebrate` class only when that session value is true. This keeps import workflow and visual effect selection out of the pure definition of a win.

## Why keep the board

The completed arrangement gives context to the result. A player can inspect the faces and positions they just learned, see the final move count and choose Restart if they want the same deal. A separate results screen would require navigation back or another representation of the final board.

An overlay could make the result prominent but would cover information and create dialog focus responsibilities. A transient toast would disappear while the completed state remained. The in-place paragraph keeps the result available without a dismissal task. [ADR 019](adr-019-modal-overlays.md) and [ADR 031](adr-031-toast-announcements.md) preserve related retired ideas rather than governing this banner.

## Feedback and timing

The winning activation produces the completed game state and a turn result that includes the win. The visible summary then reads from that final state. Later clock observations do not advance a won game. This prevents the displayed final time from drifting while the player reads it.

The banner pulse changes background and opacity briefly, with one iteration. It does not shower the board with moving objects or repeatedly request attention. Its reduced-motion override removes the effect while keeping the same text and border. [ADR 025](adr-025-reduced-motion.md) explains that presentation path.

## Alternatives and evidence

A blocking result dialog would have created an extra step before another game. Replacing the entire board with a score card would have discarded useful visual context. Doing nothing except leaving all faces up would have made the endpoint less explicit and harder to discover through a status-oriented journey.

Browser tests complete games, inspect summary text and check the single pulse's duration and iteration count. They also load completed progress, verify no Resume button and inspect the static banner. Constrained-layout checks ensure the summary wraps without overlapping controls or status.

The banner is a result of completion, not a second scoring system. Moves and elapsed time retain their ordinary meanings, and best moves remain scoped to size. [Win conditions](win-conditions.md), [ADR 032](adr-032-best-score-scope.md), [ADR 049](adr-049-zoom-floor.md) and [Status bar](status-bar.md) provide the surrounding behavior.

## The completed-state rendering

The statistics and completion paragraph read the same game. The class distinguishes an eligible visual celebration without introducing another win model.

```tsx
  return (
    <>
      <dl className="stats">
        <div>
          <dt>{StatusLabel.moves}</dt> <dd>{game.moves}</dd>
        </div>
        <div>
          <dt>{StatusLabel.pairs}</dt>{' '}
          <dd>
            {game.matched.length / 2}/{game.deck.length / 2}
          </dd>
        </div>
        <div>
          <dt>{StatusLabel.time}</dt> <dd>{formatElapsed(game.elapsedMs)}</dd>
        </div>
        <div>
          <dt>{StatusLabel.best}</dt> <dd>{best ?? 'none'}</dd>
        </div>
      </dl>
      {isWon(game) && (
        <p className={`victory${celebration ? ' celebrate' : ''}`}>
          Every pair found in {game.moves} moves and {formatElapsed(game.elapsedMs)}. Well played.
        </p>
      )}
    </>
  );
}
```
