# ADR 044: Keep the game on one page

This record explains why the current game has no router, route state or navigation layer.

## Context

The interface has one continuous activity: choose a board, reveal pairs, inspect status and optionally open reproduction tools. Completion adds a summary to the same board. Importing progress replaces session data. None of those operations requires a separate document location to explain what the user is doing.

The decision is to keep the application as one page with direct component composition and no router. `Game.tsx` renders controls, status, feedback, board and reproduction in a single `main`. The architecture specification explicitly avoids a routing framework for this scope.

## Session transitions are not routes

New Game and Restart replace the game state and increment generation. Loading a board or snapshot validates text and then replaces the state. These transitions have clock and focus consequences, but they do not create URL history entries. The board code is an explicit field and format rather than a path segment.

The reproduction tools use a native `details` disclosure. Opening that panel is a local presentation change, not navigation to a tools screen. The completed banner similarly remains alongside the board and its final statistics. [ADR 050](adr-050-completion-banner.md) explains why completion keeps that context.

## Why this fits delivery

The artifact opens directly from disk and can also be served from a local origin. Route-based launch would need to account for both path handling and server fallback behavior. A plain code loaded through a control has the same meaning in either context. A copied HTML file does not need a host configured to rewrite route requests.

Browser Back and modifier combinations also retain ordinary browser meanings because the board handles only its stated keys. Introducing a navigation framework would add another consumer of history without a required screen transition to justify it. [ADR 003](adr-003-single-file.md) covers the delivery boundary.

## Alternatives

Separate play, results and reproduction routes might make sense in a larger application with distinct workflows. Here they would force state transfer and focus decisions between screens for information already visible in one small tree. A URL hash for every current selection would also confuse reproducible deal identity with transient progress.

A query parameter for the initial board could be a separate feature, but it is not implemented in the game. The documented control is the way to load a code in the current game.

## Consequences

There are no deep links to an in-progress selection or completed summary. A snapshot is the explicit progress representation, and replay is the developer action trace. That keeps the browser URL out of the consistency problem between deck, matches, selection and time.

All current UI remains in one React tree, so state ownership and rendering dependencies are easy to trace from `Game`. The controller can replace the game without a route resolver or navigation event bus. The board's generation key handles local focus-memory reset, while the action control keeps focus.

This is a scope decision, not a prediction that no future product could benefit from navigation. For the current behavior, routing would supply infrastructure without a corresponding user journey. [ADR 016](adr-016-no-state-library.md), [Reproduction panel](reproduction-panel.md), [React boundaries](react-boundaries.md) and [Architecture](architecture.md) explain the neighboring structure.

## The single-page composition boundary

Game obtains one controller and renders the activity directly. Resume is a local focus-and-session operation rather than navigation.

```tsx
export function Game() {
  const controller = useGame();
  const { session } = controller;
  const { game } = session;
  const board = useRef<BoardFocus>(null);
  function resume(): void {
    board.current?.focusRemembered();
    controller.resume();
  }
  return (
    <main>
      <header>
        <p className="eyebrow">A little focus. A perfect pair.</p>
        <h1>Neon Pairs</h1>
      </header>
      <p className="intro">Reveal two cards. Match their shape and marker to keep the pair.</p>
      <Controls size={game.size} newGame={controller.newGame} restart={controller.restart} />
      <GameStatus
        game={game}
        best={session.preferences.bestMoves[String(game.size)]}
        celebration={session.celebration}
      />
      <p id="board-help" className="help">
        Use a pointer or Tab to enter the board. Arrow keys move between cards. Enter or Space
        reveals a card.
      </p>
      <div className="game-result">
        <p role="status" aria-live="polite" aria-atomic="true">
          <span key={session.announcement}>{session.feedback}</span>
        </p>
      </div>
      <Board key={session.generation} game={game} activate={controller.activate} ref={board} />
      <Reproduction
        game={game}
        generation={session.generation}
        exportSnapshot={controller.exportSnapshot}
        loadBoard={controller.loadBoard}
        loadSnapshot={controller.loadSnapshot}
        notice={session.loadNotice}
        paused={session.paused}
        resume={resume}
      />
    </main>
  );
}
```
