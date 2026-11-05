# ADR 037: Pause unfinished imported progress

This record explains why an unfinished snapshot waits for Resume before accepting play or active time.

## Context

A snapshot can contain one revealed card or a mismatch with only a few milliseconds left. Loading it is an inspection task as well as a continuation task: the player may still be reading the status, checking the board or closing the reproduction panel. Letting the loaded clock run immediately could consume the remaining observation interval before the player was ready.

The decision is to load unfinished progress into a paused session. Resume clears that pause. Completed progress stays completed and has no Resume action. The pause belongs to the session wrapper, not to the serialized selection union, so the same valid snapshot can be interpreted by pure tools without browser UI state.

## State transition

`replaceGame` computes pause from whether the operation is an import and whether the loaded game is won. It increments generation, resets celebration and updates the selected size. The loaded game itself retains its selection and remaining time. Pausing does not close the mismatch, reset moves or turn a one-card selection into a closed state.

`activateSession` returns the same session while paused. `tickSession` also rejects advancement in that condition. This blocks both click and keyboard reveals through the shared action path while leaving board focus navigation available. The clock's running predicate consults pause as well, avoiding unnecessary scheduling.

The visible status says “Loaded snapshot. Select Resume to continue.” Completed progress has its separate load status. These messages make the session state explicit rather than requiring the player to infer pause from an unchanging clock. [Reproduction panel](reproduction-panel.md) documents the controls.

## Resume and focus

Resume has a special focus obligation because the button disappears. `Game.tsx` first calls `board.current?.focusRemembered()` and then invokes the controller's resume action. The controller resets its observation baseline and clears pause and load feedback. This order gives focus a surviving destination before removing the control that held it.

Other game actions keep focus on their initiating controls. A successful load resets the board's remembered entry position, but does not immediately move browser focus into the board. The distinction between resetting focus memory and moving focus is essential to understanding this interaction. [ADR 024](adr-024-roving-focus.md) covers the board side.

## Alternatives

Immediate continuation would have been simpler but hostile to snapshots near a mismatch edge. Automatically closing the mismatch on import would have changed the saved progress. Restarting its full interval would have changed remaining active time. A separate modal inspection screen would have added another navigation and dialog problem to a small reproduction workflow.

The inline Resume path makes the pause explicit without turning loading into another game mode. Board codes do not need it because they load a fresh unstarted arrangement. Completed snapshots do not need it because no further active interval remains.

## Evidence and consequences

Session tests assert blocked activation and ticks while paused. Browser tests load progress, attempt reveals, move focus, resume and verify the remaining mismatch interval. Completion imports are also checked for a static completed presentation and the absence of Resume. Exact timing tests ensure waiting before Resume contributes no active time.

The pause is not serialized back as an extra snapshot field. Export still describes the game progress, while browser continuation is controlled by the session. This keeps format version 1 independent of focus, disclosure state and temporary import workflow. [ADR 010](adr-010-snapshot-json.md), [Snapshot selection notes](snapshot-selection-notes.md) and [Clock design](clock-design.md) explain those boundaries.

## The session’s time and input guards

Time advancement and activation both consult the session pause. Focus navigation is owned elsewhere and is not suppressed by these guards.

```ts
export function tickSession(session: Session, generation: number, milliseconds: number): Session {
  if (session.generation !== generation || session.paused) return session;
  const game = advanceGame(session.game, milliseconds);
  return game === session.game ? session : { ...session, game };
}

export function activateSession(session: Session, index: number): Session {
  if (session.paused) return session;
  const game = activateCard(session.game, index);
  if (game === session.game) return session;
  if (game.selection.kind === SelectionKind.one) return { ...session, game };
  const won = isWon(game);
  const feedback = won
    ? `Pair found. All pairs found in ${game.moves} moves and ${formatElapsed(game.elapsedMs)}.`
    : game.selection.kind === SelectionKind.mismatch
      ? 'No match. Cards close after a short pause.'
      : `Pair found. ${game.matched.length / 2} ${game.matched.length === 2 ? 'pair' : 'pairs'} matched.`;
  const preferences = won
    ? rememberBest(session.preferences, game.size, game.moves)
    : session.preferences;
  return {
    ...session,
    game,
    feedback,
    announcement: session.announcement + 1,
    preferences,
    celebration: won,
  };
}
```
