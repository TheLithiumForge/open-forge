# ADR 057: Split the view by behavior

This record explains the component boundaries and the targeted rendering work that keeps large boards responsive.

## Context

A single game component could contain the select, status, 256 buttons, snapshot editor, timers and storage effects. That would be quick to start and difficult to review once input, import and visibility interact. The project instead separates the view by behavior while keeping the composition easy to follow.

`Game.tsx` is the assembly point. `Controls` owns deal actions, `GameStatus` renders progress and completion, `Board` renders and navigates cards, and `Reproduction` owns the disclosure and drafts. Browser session behavior lives behind `useGame`. These boundaries correspond to real responsibilities rather than a generic component taxonomy.

## Small does not mean responsibility-free

`Card` is a leaf button with the state descriptor, face renderer and event callbacks it needs. `Board` calculates face-up and matched membership and supplies positional props. `useBoardFocus` owns remembered focus and navigation. The session controller decides how an activation changes the game and when browser time is synchronized.

This arrangement prevents a card from reading storage or deciding whether a snapshot load should pause. It also prevents the controller from querying every button to paint its revealed state. Each layer can be understood in terms of the data or browser effect it actually owns. [React boundaries](react-boundaries.md) explains those connections.

## A concrete rendering concern

The active clock can update often, and a 16x16 board contains many buttons. The board memoizes the face-up set from matched positions and selection, and separately memoizes matched membership. It passes a stable activation callback that reads the latest handler through a ref. `Card` is memoized.

A pure clock tick that preserves selection and matched arrays therefore does not force every unchanged card to render again. The optimization follows an observed repeating update. It is not a reason to make every value global or to hide the data flow behind a subscription framework.

## Styles and tests follow the boundary

Card styles sit with `Card`, board layout with `Board`, status styles with `GameStatus`, and control styles with controls. The diagnostic face sheet renders the actual card outside the main controls to verify that its appearance does not depend on unrelated parent styling. That is useful evidence that the component owns its own presentation.

Focused tests sit near the associated behavior, while browser journeys span the assembled application. The source-size specification aims for 100 lines per TypeScript file and limits authored files to 200 lines. The audit checks that ceiling. These constraints keep authored files reviewable.

## Alternatives

A monolithic component would reduce imports but mix unrelated reasons to change. Splitting every few lines into a new component would create indirection without a stable behavior boundary. A generic reusable-control framework could add configuration for cases this game does not have. The chosen middle ground uses small named pieces with concrete jobs.

Props do repeat some data, but that repetition is visible and type checked. A reviewer can trace how a closed card receives `null` instead of a face ID. Hiding that relationship behind an ambient store would make the information boundary harder to inspect.

## Consequences

More files mean more navigation, so direct imports and meaningful ownership matter. The payoff is that a focus bug, parser fix or status-layout change usually has an identifiable home. [ADR 001](adr-001-react.md), [ADR 016](adr-016-no-state-library.md), [CSS organization](css-organization.md) and [Source layout](source-layout.md) are the companion pages. The aim is understandable behavior with targeted rendering work, not file count as an end in itself.

## Stable card inputs in the board

The board keeps membership calculations and callback identity stable across clock-only renders, while the latest controller callback remains reachable.

```tsx
const { size, deck, matched, selection } = game;
  const { viewport, element, focused, focusRemembered, navigate, cardFocused } =
    useBoardFocus(size);
  useImperativeHandle(ref, () => ({ focusRemembered }), [focusRemembered]);
  // A clock tick keeps both arrays, so the cards' props stay equal and no card renders again.
  const faceUp = useMemo(() => faceUpCards({ matched, selection }), [matched, selection]);
  const matchedCards = useMemo(() => new Set(matched), [matched]);
  // The game passes a new handler on each render. Cards keep one callback that calls the latest.
  const latestActivate = useRef(activate);
  useLayoutEffect(() => {
    latestActivate.current = activate;
  });
  const activateCard = useCallback((index: number) => latestActivate.current(index), []);
  const style: BoardStyle = { '--grid-size': size };
```
