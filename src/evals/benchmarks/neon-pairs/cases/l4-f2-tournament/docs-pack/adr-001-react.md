# ADR 001: Render the game with React

This record explains why React owns the browser view while plain functions own the game rules.

## Context

The board looks simple until time and input start interacting. A second reveal can match immediately or remain visible during a mismatch. Starting another deal can happen during that interval. Keyboard focus has a remembered position, reproduction controls have editable drafts, and status text changes independently of the deck. Directly updating all of those DOM fragments would make each game action responsible for remembering every presentation consequence.

React gives those changes a common rendering boundary. The decision was to use React with DOM rendering, rather than to build a bespoke collection of DOM mutation procedures. The current entry, `src/main.tsx`, obtains the root element and renders `Game` inside `StrictMode`. There is no second application root for a score display or reproduction form.

## Decision and division of work

React represents the current session; it does not define what a match means. `src/game/game.ts` implements `activateCard` and `advanceGame` without React. `useGame` adapts those operations to browser state, and `Game.tsx` composes `Controls`, `GameStatus`, `Board` and `Reproduction`. This boundary lets replay call the same rules without constructing a document or mounting a component.

The view also retains presentation state that is inappropriate for the game model. `useBoardFocus` remembers the tab stop. `Reproduction` keeps its snapshot textarea draft. Neither belongs in exported progress. Conversely, matched positions and active elapsed time belong to game state even when no component is currently looking at them. [React boundaries](react-boundaries.md) explains that split in more detail.

A useful consequence is visible in `Card.tsx`: a hidden card receives `face: null`, and the component renders the neutral back instead of mounting its SVG. Visibility is a rendering choice derived from the game state, rather than a CSS class applied to a permanently present secret face. React is helpful here because the conditional tree closely expresses the information the player can access.

## Alternatives discussed

A small imperative DOM implementation would have reduced framework code in the distribution. It would also have needed its own lifecycle discipline for timers, listeners, focus and replacing the board. That trade was reasonable for a static prototype and less attractive for the acceptance journeys involving imports and interrupted mismatches.

A canvas board was another option. It would have replaced native buttons, names and focus with custom hit testing and accessibility plumbing. That did not solve the main problem. The project needs a board whose controls participate in the browser's input model; the rendering framework sits above those controls.

A larger application framework would have supplied routing and server integration that the game does not use. React and React DOM are the runtime packages in the manifest. Vite handles the distribution separately; [ADR 005](adr-005-vite.md) records that build choice.

## Cost and evidence

Framework rendering can be wasteful if every clock observation redraws 256 faces. `Board` memoizes sets of face-up and matched positions, keeps a stable activation callback, and renders memoized `Card` components. This is a targeted response to an actual repeating update, not a reason to move the rules into component internals.

`StrictMode` makes effect cleanup particularly relevant during development. The session clock effect clears its timer and removes its visibility listener. Browser lifecycle checks cover replacement and unmount behavior. Those checks supply stronger evidence than the fact that a small board appears correctly on first load. See [Session module](session-module.md) and [ADR 016](adr-016-no-state-library.md) for the related state ownership decision.

## The browser entry

The entry is small enough to show in full. The rules are reached through Game rather than initialized by a second browser script.

```tsx
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { Game } from './game/Game.tsx';

const root = document.getElementById('root');
if (!root) throw new Error('Missing application root.');
createRoot(root).render(
  <StrictMode>
    <Game />
  </StrictMode>,
);
```
