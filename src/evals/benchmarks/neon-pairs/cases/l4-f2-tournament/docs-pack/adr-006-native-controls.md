# ADR 006: Use native controls for game actions

This record explains why cards and controls use browser elements rather than custom interaction widgets.

## Decision and context

The game uses native buttons for cards and actions, a native select for board size, and ordinary text inputs for reproduction data. The board is CSS Grid containing DOM buttons. The reproduction section uses `details` and `summary`. These elements already participate in focus, keyboard activation, forms and browser accessibility APIs.

A custom control library could have supplied consistent styling, but the project would still have needed to verify its interaction behavior and package the additional runtime code. A hand-built div button would have been smaller in appearance while requiring the application to reconstruct semantics the browser already provides. The native approach keeps the interesting custom work at the board-navigation level.

## Native activation, game eligibility

A card button dispatches its click through `activate(index)`. Enter and Space use the browser button behavior. A matched card remains a focus destination and carries `aria-disabled`; the game rule ignores its activation. This is different from removing the card from the keyboard path with the native `disabled` attribute. Players can still inspect its position and face while navigating a completed part of the board.

Likewise, input blocking during a mismatch lives in the rules. The UI does not need to unmount every card or temporarily replace buttons with decorative elements. Focus movement can continue while `activateCard` returns the same state for attempted reveals. [Disabled state guide](disabled-state-guide.md) and [ADR 024](adr-024-roving-focus.md) explain these distinctions.

## Forms and drafts

`BoardCodeForm` uses a real form. Submission reads `boardCode` through `FormData`, prevents navigation and asks the controller to validate it. A successful replacement remounts the code input with the new generation; the submit button remains available as the focus destination. Invalid input keeps the draft available for correction.

The snapshot textarea is a controlled field inside `Reproduction`. Export writes readable JSON there, where browser text selection and copy already work. The implementation does not need a privileged clipboard operation or a custom text editor. The collapsed reproduction panel is similarly a native disclosure, rather than an application-specific open/close keyboard widget.

## Browser behavior retained

Right-click and long-press keep their context-menu meaning. The card does not reveal on pointer down or hover. A completed click performs the game action. This lets touch scrolling and cancellation cooperate with ordinary browser behavior instead of competing with an eager reveal handler. [ADR 047](adr-047-touch-scroll.md) records the scrolling consequence.

Native does not mean unstyled. Shared controls use the panel background and border tokens, and keyboard focus has a visible outline. Card sizing and face rendering are specific to the game. The decision concerns the foundation beneath that presentation, not a promise to preserve every browser default margin.

## Limits and evidence

A native button does not automatically make a board easy to navigate. The project still implements a remembered tab stop, positional names, nearest scrolling and turn feedback. Browser tests check those behaviors against the built game. The complete keyboard journey includes leaving the board and starting another game, so evidence extends beyond pressing Enter on a single isolated element.

The decision reduced the amount of input machinery the team had to author. It did not eliminate the need to inspect the accessibility tree or validate touch and focus behavior in a real browser. See [Native controls guide](native-controls-guide.md), [Input](input.md) and [ADR 043](adr-043-keyboard-parity.md).

## The size selector and action buttons

Controls maps the supported sizes directly into native options and passes actions to the session controller.

```tsx
export function Controls({ size, newGame, restart }: ControlsProps) {
  return (
    <div className="controls">
      <div className="size-control">
        <label htmlFor="grid-size">{DealControl.size}</label>
        <select
          id="grid-size"
          className="control"
          value={size}
          onChange={(event) => newGame(Number(event.currentTarget.value))}
        >
          {GRID_SIZES.map((option) => (
            <option key={option} value={option}>
              {option}x{option}
            </option>
          ))}
        </select>
      </div>
      <button type="button" className="control" onClick={() => newGame()}>
        {DealControl.newGame}
      </button>
      <button type="button" className="control" onClick={restart}>
        {DealControl.restart}
      </button>
    </div>
  );
}
```
