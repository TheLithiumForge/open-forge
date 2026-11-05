# ADR 043: Use the same action across input methods

This record explains keyboard parity for revealing cards while keeping navigation separate from play.

## Decision

A primary click, completed tap, Enter and Space reach the same card activation. Keyboard movement is a separate operation that changes focus without revealing a face. The choice gives different input methods the same game rules while preserving the browser's familiar controls.

The implementation uses native card buttons with one click handler calling the controller. It does not maintain a second keyboard-only move counter or a separate touch selection. Eligibility belongs to `activateCard`, which handles repeated first-card activation, matched cards and pending mismatches consistently for all callers.

## What parity means here

Parity does not mean every physical gesture has an identical event sequence. A tap can become a pan, a context-menu gesture has its own browser behavior, and Enter and Space follow native button activation timing. The shared result is the game action after a valid activation, not a demand to intercept every low-level event.

Hover and pointer cancellation leave game state unchanged. Right-click and long-press retain the context menu. Plain arrow keys move focus spatially; modifier combinations and other keys remain with the browser. This bounded key handling avoids turning the board into a page-wide keyboard interceptor. [Input](input.md) is the interaction definition.

## Focus is not selection

A focused card can be closed, revealed or matched. Moving the outline does not change the turn. During a mismatch, the player can still navigate although a reveal attempt is ignored. Matched cards stay focusable so known information remains inspectable.

The remembered tab stop makes entering a large board manageable, and native Tab leaves it. New deals reset that memory without moving focus away from the action button. Resume moves focus before removing its own button because it is a different lifecycle case. [ADR 024](adr-024-roving-focus.md) and [Focus policy](focus-policy.md) cover those details.

## Alternatives

Separate handlers that implement matching directly for each input method would make drift likely. One might count a repeated selection differently, or a touch path might reveal before a scroll gesture completed. Handling pointer down for fast visual response would also change the meaning of cancellation and context menus.

A keyboard-only “select” mode was unnecessary because native buttons already provide activation and the rules already describe a first and second reveal. A canvas board could simulate parity but would need custom focus and semantics before it reached the same starting point as DOM controls.

## Evidence

Focused rule tests prove that an activation has one meaning. Browser input tests then prove that the supported controls reach that action and that focus movement does not. The complete accessibility journey starts with controls, enters a 4x4 board, exposes positions and face names, completes the board, leaves with Tab and starts again.

The tree record is automated evidence about available names and states. Spoken output with a screen reader remains acceptance work. Touch scrolling is tested in browser automation, while real-device long-press remains an explicit manual observation. These limits keep the parity claim tied to the documented actions rather than every conceivable gesture.

[Pointer and touch](pointer-and-touch.md), [Board keyboard navigation](board-keyboard-navigation.md), [ADR 022](adr-022-card-names.md) and [ADR 047](adr-047-touch-scroll.md) provide the related interaction rationale. The central outcome is one game transition path, surrounded by input-specific browser behavior rather than duplicated game rules.

## The card event boundary

Focus and navigation callbacks remain distinct from the click callback. The latter calls the shared activation before its completed-click scroll adjustment.

```tsx
return (
    <button
      type="button"
      className={`card ${state}`}
      aria-label={label}
      aria-disabled={matched}
      tabIndex={tabIndex}
      onFocus={(event) => focus(index, event.currentTarget)}
      onKeyDown={(event) => navigate(event, index)}
      onClick={(event) => {
        activate(index);
        clicked(event.currentTarget);
      }}
    >
      {face === null ? <span className="card-back" aria-hidden="true" /> : <Face face={face} />}
      {matched && (
        <svg className="matched-mark" viewBox="0 0 12 12" aria-hidden="true" focusable="false">
          <path d="m2 6 3 3 5-6" fill="none" stroke="currentColor" strokeWidth="2" />
        </svg>
      )}
    </button>
  );
});
```
