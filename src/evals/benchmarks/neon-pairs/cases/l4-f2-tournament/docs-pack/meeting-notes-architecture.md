# Architecture review notes

These are meeting notes from a review of the Neon Pairs source boundaries, with observations tied to the current implementation.

The review covered the existing structure and questions left open under [Architecture](architecture.md) and [Technology](technology.md).

## Opening question: what owns an attempt?

The review began with a walkthrough of one card activation. `Game.tsx` obtains the session controller, passes an activation callback to the board, and renders status and controls around it. A card click reaches `useGame.activate`, which synchronizes browser time and then calls `activateSession`. The session calls the pure `activateCard` transition and attaches result feedback and preference effects when appropriate.

The reviewer acting as core maintainer wanted to know whether a rule could run without React. The answer in the current source is yes: deck generation, card transitions, logical time, snapshot decoding, and replay are functions under `src/game`. A replay calls the same transitions without mounting a component.

The review's agreement was that this boundary was more useful than organizing everything around generic controller and service layers. The game has a small vocabulary and direct calls explain it well. [Game-state model](game-state-model.md) and [react boundaries](react-boundaries.md) provide the more systematic descriptions.

## Why the session is not just another copy of game state

The current `Session` wraps `game` with imported pause, generation, feedback, load notice, announcement counter, celebration, and preferences. These values describe browser-session meaning around the pure position. They are not all serialized into a snapshot because they do not all belong to transported progress.

A completed snapshot illustrates the distinction. Its game state is won, but replacing the session does not perform a winning action. `replaceGame` clears celebration and publishes the appropriate load notice. A later final pair in an unfinished imported attempt goes through `activateSession`, which can update local records and mark the new celebration.

The discussion considered moving all these flags into `GameState` so components would receive one large object. That would make a pure replay carry UI publication state and browser preferences it did not need. Keeping a wrapper made the difference between position and event visible.

The same distinction appears in time. A snapshot carries active elapsed milliseconds and mismatch remainder. It does not carry a `performance.now()` reading, a scheduled timeout handle, or document visibility. Those belong to the adapter in the browser.

## Replacement has an identity boundary

The reviewer responsible for lifecycle checks walked through starting a new deal during a pending miss. The current controller resets its clock and replaces the game. Replacement increments `generation`; the board key uses that generation, and pending session ticks compare against it.

This combines several consequences without making them accidental. The old timer cannot update the new session. The board's remembered focus state starts again at its first position. The code input can receive the new active code through its generation key. The action's own button remains focused because the surrounding controls are not remounted with the board.

The agreement was to understand generation as the identity of a replaced attempt, not as a general counter for every render. Timer advancement does not increment it. A first reveal does not recreate the board. This keeps replacement effects tied to actual replacement rather than to ordinary play updates.

The [session module](session-module.md) and [clock design](clock-design.md) explain these current mechanisms. The [focus incident](incident-focus-trap.md) shows what went wrong when every action is treated as a new focus destination.

## Rendering work was inspected before abstraction

The largest board has 256 buttons. A concern raised in the review was whether a changing clock would force every card to render at each tick. Current `Board.tsx` memoizes face-up and matched sets, while `Card` is memoized. The board maintains a stable activation callback that calls the latest controller handler through a ref.

An ordinary tick preserves matched and selection references, so card props can remain equal. A pending mismatch does change its selection remainder, but this still does not require a new rendering technology. The source optimizes the concrete repeated work without introducing a virtualized board or canvas input model.

Its useful outcome was explanatory: a proposed optimization should identify the state that changes and the components that actually consume it. The [large-board rendering page](large-board-rendering.md) and [performance budget](performance-budget.md) hold the related current documentation.

## Unknown input had local readers

Snapshot, replay, and preference readers share a few small JSON checks in `saved-format.ts`: object shape, safe nonnegative integers, exact key presence, byte length, and parsing. The behavior-specific readers then apply their own meanings. Snapshot selection validates positions against the derived deck; preference validation checks supported size keys and plausible move bounds; replay validates ordered action timestamps.

The review asked whether this should become a schema framework. There was no demonstrated need in the current format set. The existing shared functions encode common mechanics without hiding the different semantic checks behind an opaque generic layer.

The decoder's acceptance set also remained distinct from the encoder's preferred formatting. `encodeSnapshot` sorts matched indices for output, but `readMatched` accepts valid complete pairs in another order. The [snapshot parsing incident](incident-snapshot-rejects.md) captures the cost of confusing those responsibilities.

## Compiler and build boundaries were visible

The current base TypeScript configuration enables strict checking, indexed-access checks, exact optional properties, verbatim module syntax, and erasable syntax. It uses no emit for checking. The application project includes browser libraries and React JSX; the Node project includes tools, tests, fixtures, and build configuration with Node types.

The build script type-checks before Vite bundles. Browser tests build the current game and diagnostic entries before running journeys. Vite's game configuration uses the React and single-file plugins, targets ES2023, and disables module preload because the inline distribution has no chunks to fetch.

These observations explain why a successful development server is insufficient evidence for the delivered artifact. The actual HTML has to work from disk without companion files or requests. Diagnostic art and session entries are separate builds and do not belong in the game distribution. [Build pipeline](build-pipeline.md) and [offline delivery](offline-delivery.md) describe that existing flow.

## Open discussion, without new machinery

The review left future feature organization to actual feature work. Hints, themes, deadlines, and connected sessions all raise different questions; none justifies preparing unused state branches today. It also left broader browser evidence to explicit test work rather than inferring it from the named Chromium run.

The closing observation was that the current structure stays readable because each boundary has a concrete reason: core transitions for reusable rules, session code for browser effects and persistent meaning, board code for spatial focus, and local controls for drafts. The review recorded those reasons so later refactors can assess what they preserve.
