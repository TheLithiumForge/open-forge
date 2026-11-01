import { useRef } from 'react';
import { Face } from './Face.tsx';
import { describeCard } from './card-state.ts';
import { ActivationKey } from './keyboard.ts';
import type { KeyboardEvent } from 'react';

const DRAG_DISTANCE_PX = 8;
interface CardProps {
  index: number;
  size: number;
  face: number | null;
  matched: boolean;
  blocked: boolean;
  tabIndex: number;
  activate(index: number): void;
  focus(index: number): void;
  navigate(event: KeyboardEvent<HTMLButtonElement>, index: number): void;
}

export function Card({
  index,
  size,
  face,
  matched,
  blocked,
  tabIndex,
  activate,
  focus,
  navigate,
}: CardProps) {
  const pointer = useRef<{ x: number; y: number; cancelled: boolean } | null>(null);
  const { state, label } = describeCard(index, size, face, matched);
  return (
    <button
      type="button"
      className={`card ${state}`}
      aria-label={label}
      aria-disabled={blocked || matched}
      tabIndex={tabIndex}
      onFocus={(event) => {
        focus(index);
        event.currentTarget.scrollIntoView({ block: 'nearest', inline: 'nearest' });
      }}
      onKeyDown={(event) => {
        if (
          event.repeat &&
          (event.key === ActivationKey.enter || event.key === ActivationKey.space)
        )
          event.preventDefault();
        navigate(event, index);
      }}
      onPointerDown={(event) => {
        pointer.current = { x: event.clientX, y: event.clientY, cancelled: false };
      }}
      onPointerMove={(event) => {
        const start = pointer.current;
        if (
          start &&
          Math.hypot(event.clientX - start.x, event.clientY - start.y) >= DRAG_DISTANCE_PX
        )
          start.cancelled = true;
      }}
      onPointerCancel={() => {
        if (pointer.current) pointer.current.cancelled = true;
      }}
      onContextMenu={() => {
        if (pointer.current) pointer.current.cancelled = true;
      }}
      onClick={(event) => {
        if (!(event.detail > 0 && pointer.current?.cancelled)) activate(index);
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
}
