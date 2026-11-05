# ADR 014: Exclude hidden-page time

This record explains why hiding the page suspends both elapsed time and a pending mismatch interval.

## Context

A player can switch tabs, lock a device or move to another application while two different faces are visible. If the interval continued in the background, the player would return to a closed pair without receiving the intended observation time. The elapsed clock would also charge time during which the board was unavailable.

The decision was to count active time only while the page is shown, after play starts and before completion. A hidden interval contributes neither to total elapsed time nor to a mismatch countdown. This uses one definition of activity instead of separate pause arithmetic for the visible clock and card closure.

## Boundary handling

`useSessionClock` keeps a visibility reference initialized from `document.hidden`. When a `visibilitychange` event arrives, it synchronizes using the previous visibility value, then updates visibility and reschedules. The ordering matters. Hiding first credits the final visible slice; returning first discards the hidden slice. Reversing those operations would charge or omit the wrong interval.

`running()` also checks that the game has started, is not complete and is not paused after an import. Visibility is therefore one part of a session activity decision. It is not stored in snapshots or added as another game selection variant. [Session module](session-module.md) describes those session-only fields.

## Example

Suppose a mismatch has consumed 200ms of its 800ms interval when the page becomes hidden. After a long absence, the remaining interval is still 600ms. Returning lets that remaining active time continue. The already recorded elapsed time also stays fixed throughout the absence.

The test in `browser-environment.e2e.test.ts` makes this edge concrete. It uses a controlled browser clock, hides the document at 200ms, advances 2000ms, and checks unchanged progress. After showing it, 599ms leaves both cards revealed and the next millisecond closes them. This verifies a shared active-time boundary rather than merely checking that the display appears paused.

## Alternatives

Leaving time running would have made the clock easy to implement but would have treated background time as play. Pausing only the elapsed display would have concealed a mismatch that still expired. Restarting the full mismatch interval on return would have granted a fresh observation window rather than preserving the actual remaining interval.

A focus-based pause was also less suitable. Focus can move between a card, the size select and reproduction controls while the page remains fully available. The decision follows document visibility rather than whether a particular element owns focus. Keyboard navigation during an active mismatch remains useful and does not itself imply a pause.

## Lifecycle consequences

The clock stops scheduling while it is not running and resumes observation from its updated baseline. Replacing the game increments the session generation and resets the clock. Old scheduled work checks its captured generation before applying time. Hiding a page and replacing a deal are different events, but both depend on callbacks consulting the current session rather than an obsolete closure.

Browser automation cannot assume that an ordinary headless tab switch produces the same visibility behavior as an interactive desktop. The test explicitly controls `document.hidden` and dispatches the visibility event at the adapter boundary. It is evidence about the game's visibility handling, with that limitation visible in the source. [ADR 013](adr-013-monotonic-clock.md), [Mismatch interval](mismatch-interval.md) and [ADR 037](adr-037-snapshot-pause.md) cover the neighboring timing choices.

## The visibility listener and cleanup

Synchronization happens before the stored visibility changes. Cleanup removes both future timer work and the listener owned by this effect.

```ts
function visibilityChanged(): void {
      if (!active || generation !== current.current.generation) return;
      synchronize(generation);
      visible.current = !document.hidden;
      schedule();
    }
    schedule();
    document.addEventListener('visibilitychange', visibilityChanged);
    return () => {
      active = false;
      window.clearTimeout(timer);
      document.removeEventListener('visibilitychange', visibilityChanged);
    };
  }, [session.generation, session.game.started, session.paused, won]);
```
