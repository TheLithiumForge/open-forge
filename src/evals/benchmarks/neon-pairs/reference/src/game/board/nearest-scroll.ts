import { useEffect, useRef } from 'react';
import type { RefObject } from 'react';

interface ScrollPosition {
  readonly left: number;
  readonly top: number;
}

// Chromium centers a card that Tab, Shift+Tab or another browser action focuses, and it does so
// before focus handlers run. Scroll events arrive a frame later, so the last positions they
// reported still show the view from before that scroll. Restoring them first lets the card
// scroll from where the player was looking, with nearest alignment.
export function useNearestScroll(viewport: RefObject<HTMLElement | null>) {
  const board = useRef<ScrollPosition>({ left: 0, top: 0 });
  const page = useRef<ScrollPosition>({ left: 0, top: 0 });
  useEffect(() => {
    const element = viewport.current;
    function boardScrolled(): void {
      if (element) board.current = { left: element.scrollLeft, top: element.scrollTop };
    }
    function pageScrolled(): void {
      page.current = { left: window.scrollX, top: window.scrollY };
    }
    boardScrolled();
    pageScrolled();
    element?.addEventListener('scroll', boardScrolled, { passive: true });
    window.addEventListener('scroll', pageScrolled, { passive: true });
    return () => {
      element?.removeEventListener('scroll', boardScrolled);
      window.removeEventListener('scroll', pageScrolled);
    };
  }, [viewport]);

  return (card: HTMLElement): void => {
    viewport.current?.scrollTo(board.current.left, board.current.top);
    window.scrollTo(page.current.left, page.current.top);
    card.scrollIntoView({ block: 'nearest', inline: 'nearest' });
  };
}
