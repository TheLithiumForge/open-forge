import { useCallback, useEffect, useRef } from 'react';
import type { RefObject } from 'react';

interface ScrollPosition {
  readonly left: number;
  readonly top: number;
}

// Chromium centers a card that Tab or Shift+Tab focuses inside the board before focus handlers
// run. Scroll events arrive a frame later, so the last board position they reported still shows
// the view from before that scroll. Restoring it first lets the card scroll from where the player
// was looking, with nearest alignment in the board. The page scroll is left to the browser.
export function useNearestScroll(viewport: RefObject<HTMLElement | null>) {
  const board = useRef<ScrollPosition>({ left: 0, top: 0 });
  useEffect(() => {
    const element = viewport.current;
    function boardScrolled(): void {
      if (element) board.current = { left: element.scrollLeft, top: element.scrollTop };
    }
    boardScrolled();
    element?.addEventListener('scroll', boardScrolled, { passive: true });
    return () => element?.removeEventListener('scroll', boardScrolled);
  }, [viewport]);

  return useCallback(
    (card: HTMLElement): void => {
      viewport.current?.scrollTo(board.current.left, board.current.top);
      card.scrollIntoView({ block: 'nearest', inline: 'nearest' });
    },
    [viewport],
  );
}
