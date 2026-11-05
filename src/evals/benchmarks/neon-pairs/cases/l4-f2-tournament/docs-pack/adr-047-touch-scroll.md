# ADR 047: Let a touch gesture become scrolling

This record explains completed-click activation and the focus-scrolling order that preserves pointer and touch input.

## Context

Large boards need internal scrolling, and a finger often starts a pan on a card. Revealing immediately on pointer down would turn exploration into unintended play. Moving a focused target during a press can also cause release to land elsewhere, losing what should have been an ordinary click.

The decision is to rely on completed native click activation and to delay pointer-related nearest scrolling until the click completes. Keyboard focus still scrolls into view at focus time because no pressed pointer needs to remain over the target. This preserves a common game action while respecting different input sequences.

## The event path

`Card` registers `onClick`, `onFocus` and `onKeyDown`; it does not reveal from a pointer-down handler. The click calls the controller and then the board's `cardClicked` callback, which applies nearest alignment. The card's CSS uses `touch-action: manipulation` and disables text selection on the card surface.

`useBoardFocus.cardFocused` always remembers the index. It applies nearest scrolling immediately only when the card matches `:focus-visible`. The source comment explains why: moving a card beneath a pressed pointer can send the release elsewhere. Remembering focus and moving the viewport are therefore distinct operations.

## Browser defaults retained

A pan can remain a pan because no reveal has happened on press. Pointer cancellation and hover do not change game state. The context menu remains available on right-click or long-press and does not reveal a card. These are native browser interactions rather than gestures reconstructed by the game.

The implementation comment records Chromium's click behavior after context menu, long press and touch scrolling. The automated browser suite exercises touch panning and click activation. A real-device long-press context menu is still a user-acceptance observation because automated browsers cannot produce that gesture equivalently. [Pointer and touch](pointer-and-touch.md) documents the evidence boundary.

## Scrolling geometry

The board's viewport permits overflow in both axes and contains overscroll. Minimum card geometry remains stable while the viewport moves. Cards carry a 6px scroll margin to preserve the focus outline, and nearest alignment avoids gratuitously centering each newly focused position.

The nearest-scroll helper restores the prior board offsets when needed to counter Chromium's automatic centering on Tab entry. That concern belongs to keyboard entry, while completed-click adjustment belongs to pointer activation. The same viewport can serve both without forcing them to scroll at the same event boundary. [ADR 024](adr-024-roving-focus.md) explains the focus model.

## Alternatives and tradeoffs

Pointer capture and a custom tap recognizer could have tracked movement thresholds and cancellations. That would introduce another gesture system around native buttons. Revealing on release unconditionally would still risk turning a pan into a move unless it carefully reproduced browser cancellation rules. The native click path avoids that duplication.

Suppressing all focus scrolling would keep pointer targets still but make keyboard navigation on large boards unusable. Scrolling every focus event immediately would solve visibility while losing clicks near clipped edges. The chosen distinction is small in code because it follows the actual reason for each scroll.

Browser tests check completed taps, touch scrolling, pointer focus and first/last-card reachability. The specification deliberately leaves dragged-off pointer behavior and held keys outside its tested scope. [ADR 035](adr-035-scroll-large-boards.md), [ADR 043](adr-043-keyboard-parity.md) and [Browser test guide](browser-test-guide.md) cover the related behavior.

## Remembering focus without moving a pressed target

The hook always updates the remembered index. Only keyboard-visible focus triggers its focus-time nearest scroll.

```ts
const cardFocused = useCallback(
    (index: number, card: HTMLElement): void => {
      setFocused(index);
      // Moving a card under a pressed pointer would send the release elsewhere and lose the click.
      if (card.matches(KEYBOARD_FOCUS)) scrollToNearest(card);
    },
    [scrollToNearest],
  );
  return { viewport, element, focused, focusRemembered, navigate, cardFocused };
}
```
