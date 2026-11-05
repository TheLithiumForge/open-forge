# ADR 016: Use direct session state

This record explains why the game uses React state and explicit transitions without a global state library.

## Context

Several components need to observe the same game, but they do not independently own it. The board requests card activation, controls request session changes, status reads progress and reproduction tools request loads or exports. Their common parent can supply that data and those actions directly.

The decision was to keep the session in `useGame` and pass props through `Game.tsx`, without adding a global store or event bus. The pure rule functions already describe transitions. A second action-dispatch framework would have added another vocabulary for operations that are direct calls in the current design.

## Two kinds of state

`GameState` contains reproducible progress: the board, matched positions, selection, moves, elapsed milliseconds and the started flag. `Session` adds browser-facing concerns such as pause, generation, feedback, celebration and preferences. This distinction prevents clock lifecycle and presentation messages from becoming part of snapshot JSON.

Local presentation state stays nearer its use. The focus hook remembers an index in the board. The snapshot textarea lives in `Reproduction`. The code-input draft remains in its native input until a game generation replaces it. These pieces do not need a global subscription channel because no unrelated feature is consuming them.

## Keeping current state current

`useGame` pairs React state with a `current` ref. Its `commit` function updates the ref immediately and asks React to render the new session. Clock synchronization and successive actions consult the ref, so they operate on the latest committed session rather than a value captured by an earlier render.

This matters when an action arrives between scheduled clock observations. The controller synchronizes, obtains the current session and then applies activation. It does not dispatch time and input to separate stores that might observe them in a different order. [ADR 013](adr-013-monotonic-clock.md) explains why synchronization precedes activation.

The implementation also returns identical objects for ignored transitions when possible. `commit` can avoid a render when nothing changed. That is useful for repeated first-card activation and blocked mismatch input, and it keeps identity meaningful for memoized descendants.

## Alternatives

A state library could have provided devtools, selector subscriptions or a formal reducer setup. Those facilities were not required to express the current tree. Introducing them would not remove the need for pure game tests, timer cleanup or parser validation. A context-based store would likewise have hidden straightforward dependencies that props make visible today.

Putting every transition directly inside `Game.tsx` was the opposite alternative. It would have reduced file count while mixing DOM composition, clock observation, storage and turn rules. The chosen controller boundary keeps those responsibilities legible without building a general application framework.

## Consequences

The controller has to be deliberate about action ordering and stale callbacks. The session generation is part of that discipline: replacement increments it, and ticks from another generation do nothing. Focus memory resets through the board's React key rather than an event broadcast. These mechanisms are small enough to inspect and have focused tests.

Format helpers serve both snapshot and replay validation; the board computes face-up positions for rendering. The reason to share is a concrete common meaning, not an expectation that every future feature needs a store. See [Architecture](architecture.md), [ADR 057](adr-057-small-components.md) and [Game state model](game-state-model.md).

## The controller’s commit boundary

The ref is updated in the same operation that schedules rendering. Browser callbacks and actions consult the same current session.

```ts
const [store] = useState(browserStore);
  const [session, setSession] = useState(() => {
    const preferences = readPreferences(store);
    return initialSession(freshBoardCode(preferences.size), preferences);
  });
  const current = useRef(session);

  function commit(next: Session): void {
    if (next === current.current) return;
    current.current = next;
    setSession(next);
  }
```
