import { useCallback, useRef, useState } from 'react';
import { navigationTarget } from './keyboard.ts';
import { useNearestScroll } from './nearest-scroll.ts';
import type { KeyboardEvent } from 'react';

const KEYBOARD_FOCUS = ':focus-visible';

// Keeps the board's remembered tab stop, moves focus with the arrow keys and scrolls keyboard
// focus into view with nearest alignment.
export function useBoardFocus(size: number) {
  const viewport = useRef<HTMLDivElement>(null);
  const element = useRef<HTMLDivElement>(null);
  const [focused, setFocused] = useState(0);
  const scrollToNearest = useNearestScroll(viewport);
  // Focus calls skip the browser's centering scroll and then scroll with nearest alignment.
  const focusRemembered = useCallback(() => {
    const card = element.current?.querySelectorAll('button').item(focused);
    card?.focus({ preventScroll: true });
    if (card) scrollToNearest(card);
  }, [focused, scrollToNearest]);
  const navigate = useCallback(
    (event: KeyboardEvent<HTMLButtonElement>, index: number): void => {
      const target = navigationTarget(index, size, event.key, event);
      if (target === null) return;
      event.preventDefault();
      const card = element.current?.querySelectorAll('button').item(target);
      card?.focus({ preventScroll: true });
    },
    [size],
  );
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
