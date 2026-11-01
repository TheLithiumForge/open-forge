import { useState, useRef, useImperativeHandle } from 'react';
import { isFaceUp } from '../game.ts';
import { SelectionKind } from '../selection.ts';
import { Card } from './Card.tsx';
import { navigationTarget } from './keyboard.ts';
import { useNearestScroll } from './nearest-scroll.ts';
import type { CSSProperties, KeyboardEvent, RefObject } from 'react';
import type { GameState } from '../game.ts';
import './board.css';

interface BoardProps {
  game: GameState;
  ref: RefObject<BoardFocus | null>;
  paused: boolean;
  activate(index: number): void;
}
const KEYBOARD_FOCUS = ':focus-visible';
const NEAREST: ScrollIntoViewOptions = { block: 'nearest', inline: 'nearest' };
interface BoardStyle extends CSSProperties {
  '--grid-size': number;
}

export interface BoardFocus {
  focusRemembered(): void;
}

export function Board({ game, ref, paused, activate }: BoardProps) {
  const viewport = useRef<HTMLDivElement>(null);
  const element = useRef<HTMLDivElement>(null);
  const [focused, setFocused] = useState(0);
  const scrollToNearest = useNearestScroll(viewport);
  // Focus calls skip the browser's centering scroll and then scroll with nearest alignment.
  useImperativeHandle(
    ref,
    () => ({
      focusRemembered: () => {
        const card = element.current?.querySelectorAll('button').item(focused);
        card?.focus({ preventScroll: true });
        if (card) scrollToNearest(card);
      },
    }),
    [focused, scrollToNearest],
  );

  function navigate(event: KeyboardEvent<HTMLButtonElement>, index: number): void {
    const target = navigationTarget(index, game.size, event.key, event.ctrlKey);
    if (target === null) return;
    event.preventDefault();
    const card = element.current?.querySelectorAll('button').item(target);
    // Chromium hides the outline after a modified key like Ctrl+End, so keyboard moves ask for it.
    card?.focus({ preventScroll: true, focusVisible: true });
  }
  function cardFocused(index: number, card: HTMLElement): void {
    setFocused(index);
    // Moving a card under a pressed pointer would send the release elsewhere and lose the click.
    if (card.matches(KEYBOARD_FOCUS)) scrollToNearest(card);
  }
  // Once a click or tap completes, the focused card can move fully into view.
  function cardClicked(card: HTMLElement): void {
    card.scrollIntoView(NEAREST);
  }
  const style: BoardStyle = { '--grid-size': game.size };
  return (
    <div
      ref={viewport}
      className="board-viewport"
      role="region"
      aria-label="Game board"
      aria-describedby="board-help"
    >
      <div className="board" ref={element} style={style}>
        {game.deck.map((face, index) => (
          <Card
            key={index}
            index={index}
            size={game.size}
            face={isFaceUp(game, index) ? face : null}
            matched={game.matched.includes(index)}
            blocked={paused || game.selection.kind === SelectionKind.mismatch}
            tabIndex={focused === index ? 0 : -1}
            activate={activate}
            focus={cardFocused}
            clicked={cardClicked}
            navigate={navigate}
          />
        ))}
      </div>
    </div>
  );
}
