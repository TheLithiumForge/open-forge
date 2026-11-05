# React boundaries

This page explains where React participates in Neon Pairs and where ordinary data functions remain independent of rendering.

React mounts the application, holds the session for presentation, owns local UI state and manages browser-effect lifetimes. The game rules are callable outside React. That separation lets the browser UI and replay CLI share behavior without teaching the core about components, effects or DOM events.

## The mounted tree

`src/main.tsx` calls `createRoot` after checking the `root` element and renders `Game` inside `StrictMode`. `Game.tsx` composes deal controls, status, feedback, board and reproduction controls. It calls `useGame` once to obtain the current session and actions.

The component tree is intentionally direct. There is no React context carrying a global game store and no router coordinating separate screens. `Controls` receives size and two actions. `GameStatus` receives a game, an optional best and a celebration flag. `Board` receives the game and activation action. `Reproduction` receives current code context, exchange actions and pause-related presentation.

These props describe the existing contracts. A component can render data it does not own without gaining responsibility for deciding a turn. The core still computes matches, move counts, elapsed advancement and victory.

## The data-only side

`deck.ts`, `game.ts`, `selection.ts`, `elapsed-time.ts`, `saved-format.ts`, `snapshot.ts`, `snapshot-selection.ts` and `replay.ts` do not import React. They accept values and return values. `session/session.ts` is also a data transformation module, although it lives beside hooks that import React.

The board has data-only helpers too. `keyboard.ts` computes a target from keys and coordinates. `card-state.ts` computes a state description. `face-design.ts` maps an integer identity to design values. These helpers can be tested without rendering `Board` or building a synthetic browser tree.

This is the meaningful interpretation of the React boundary: importing a rule in a CLI does not start an effect or require a mounted component. It is not a claim that every file ending in `.ts` is independent of React. `use-board-focus.ts`, `nearest-scroll.ts` and the session hooks are TypeScript modules with hooks and browser behavior.

## React imports in application code

| Module | React mechanisms used |
| --- | --- |
| `main.tsx` | `StrictMode`; root rendering through React DOM |
| `Game.tsx` | Ref for the board's focus interface |
| `Board.tsx` | Memoized derivations, refs, stable callback, layout effect and imperative handle |
| `Card.tsx` | `memo` and a keyboard event type |
| `use-board-focus.ts` | Local state, refs and callbacks |
| `nearest-scroll.ts` | Ref, effect and callback |
| `BoardCodeForm.tsx` | Ref for the successful-load focus target |
| `Reproduction.tsx` | Local snapshot draft state |
| `use-game.ts` | Session state and current-value ref |
| `use-session-clock.ts` | Clock/visibility refs and timer effect |
| `use-saved-preferences.ts` | Persistence effect |

`Controls`, `GameStatus` and `Face` contain JSX without importing a React runtime object. The compiler uses `jsx: react-jsx`, so those files do not need the older `import React` pattern. JSX participation and explicit hook imports are related but different observations.

The diagnostic entries also use React DOM. The face sheet renders real cards, while the session harness mounts and unmounts the real game. Their entrypoints are outside `src` and built separately, so they do not become extra runtime controls in the distributed game.

## One action path for activation

`Card` renders a native button and invokes its supplied activation callback from `onClick`. Native keyboard activation and ordinary pointer activation therefore reach the same controller action. The keydown handler delegates navigation; it does not implement another matching algorithm.

The controller synchronizes active time and calls `activateSession`, which calls `activateCard`. Components receive the resulting state and render it. A class such as `matched` is an outcome of the data, not the authoritative record of a matched pair. Reading CSS classes back into the core would reverse that relationship.

Matched buttons use `aria-disabled`, keeping them in the board's focus model. The rule function still rejects activating matched positions. During a mismatch, the session does not mark every other button disabled; the core rejects new activations while that selection is pending. Semantic presentation and transition legality are both visible, with legality centralized in the rules.

## Latest values without another store

`useGame` keeps React state for rendering and a ref for immediate event-time access. `commit` updates the ref before setting React state. A clock synchronization can therefore commit a new game, and the immediately following activation can consume that new game in the same task.

The board has a similar but narrower pattern for callback identity. `Game` supplies a new activation function as it renders. `Board` stores that function in `latestActivate`, updates the ref in a layout effect and passes a stable `useCallback` wrapper to cards. Memoized cards see a stable function prop while it calls the latest controller closure.

These refs do not bypass the rules. They keep the correct data/action visible across render scheduling. The data transition still returns a new immutable description, and React still receives the committed session. [Session module](session-module.md) explains why the current ref matters for export and activation boundaries.

## Render cost on a large board

A 16 by 16 board has 256 cards. Active time can update on the 40ms observation cadence, but most ticks do not change any card's appearance. `advanceGame` retains the deck and matched array and, outside a mismatch change, retains the selection object. `Board` memoizes face-up and matched sets from those references.

`Card` is wrapped in `memo`. Its props are primitive presentation values and stable callbacks. On an elapsed-only update, face, matched, tabIndex, size and handlers stay equal, so the cards do not all need to rerender. The parent can still update the statistic.

A pending mismatch changes selection as its remainder decreases, so the face-up set can be recomputed. The resulting card props can nevertheless remain equal until the selection actually closes. This distinction explains why immutable reference stability is useful even though the board's parent renders more often than its cards.

Memoization is not a substitute for correct state ownership. Mutating the matched array in place could keep its reference stable while changing its meaning, causing derived sets to miss an update. The transition code returns a new matched array when a pair is found, matching the assumptions of the board's memoization.

## Concealed identity is a prop boundary

`Board` knows the immutable deck, but it passes `face={null}` for every position outside the derived face-up set. `Card` then renders only a decorative back. It does not mount `Face` and hide it with CSS. Its accessible description omits face design when the supplied face is null.

When revealed or matched, a card gets the face number and renders inline SVG. `Face` marks the SVG as decorative, while the button's label describes the visible design. The board can therefore hold authoritative identity without exposing concealed identity through every child element.

This behavior is also why tests use documented board codes rather than a diagnostic global. A test can reconstruct a deal independently and activate matching positions through the public interface. There is no application solver endpoint that a production user or test needs to query.

## Local focus and DOM work

`useBoardFocus` holds a focused index initialized to zero and refs to the viewport and grid. Arrow handling calls `navigationTarget`; a non-null result prevents the browser default and focuses the target button with `preventScroll: true`. Focus events update the remembered index.

`useNearestScroll` listens to the board viewport's scroll events and remembers the reported position. For keyboard focus it restores that position before calling `scrollIntoView` with nearest alignment. This accounts for Chromium's automatic centering on focus. A click scrolls after activation so the pressed card is not moved under a held pointer before release.

These browser operations belong with the board presentation, not `GameState`. A replay contains positions and logical times, not a scroll offset. A snapshot can load into a differently sized viewport. Keeping focus and scrolling local preserves that independence.

## Replacement and imperative focus

`Game` gives `Board` a ref typed as `BoardFocus`. The board exposes only `focusRemembered()` through `useImperativeHandle`. The parent does not receive the whole grid DOM or a collection of card nodes.

Resume uses that limited operation, then calls the controller's resume action. A replacement instead increments generation and changes the board key, remounting its local focus state. `BoardCodeForm` keys its uncontrolled input by that same generation, so successful loading replaces the old draft with the current code.

A generation is consequently a lifecycle boundary shared by adapters and components. It is not a reason to put DOM state in the session core. The rule functions remain unaware of keyed mounts, refs or focus methods.

## Effects and cleanup

Browser effects pair setup with cleanup: the clock removes its listener and timeout, and nearest scrolling removes its viewport listener. The separate session harness lets tests unmount the real game and measure that cleanup. [Clock design](clock-design.md) details the active flag, generation checks and scheduling lifetime.

## Keeping a change on the right side

An incorrect pair count starts in the game state and derivation, not in JSX text. An incorrect load-pause state starts in session replacement, not a disabled style. An incorrect arrow stop starts in the numerical navigation helper, while an incorrect scroll alignment starts in the DOM hook. The boundary narrows where an explanation should begin.

The architecture specification permits shared support when two actual behaviors need the same meaning. It does not introduce a generic hooks or utilities layer merely because several files use React. Current direct imports keep ownership legible, and the module policy describes feature placement. Together they make small components useful without making rendering the owner of the game rules.
