import { memo } from 'react';
import { Face } from './Face.tsx';
import { describeCard } from './card-state.ts';
import type { KeyboardEvent } from 'react';
import './card.css';

interface CardProps {
  index: number;
  size: number;
  face: number | null;
  matched: boolean;
  tabIndex: number;
  activate(index: number): void;
  focus(index: number, card: HTMLElement): void;
  clicked(card: HTMLElement): void;
  navigate(event: KeyboardEvent<HTMLButtonElement>, index: number): void;
}

// Memoized, so a clock tick that changes no card renders none of them.
export const Card = memo(function Card({
  index,
  size,
  face,
  matched,
  tabIndex,
  activate,
  focus,
  clicked,
  navigate,
}: CardProps) {
  const { state, label } = describeCard(index, size, face, matched);
  // Chromium sends no click after a context menu, a long press or a touch scroll, so every click
  // activates.
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
