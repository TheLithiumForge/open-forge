import {
  useCallback,
  useImperativeHandle,
  useLayoutEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { faceUpCards } from '../game.ts';
import { Card } from './Card.tsx';
import { navigationTarget } from './keyboard.ts';
import { useNearestScroll } from './nearest-scroll.ts';
import type { CSSProperties, KeyboardEvent, RefObject } from 'react';
import type { GameState } from '../game.ts';
import './board.css';

interface BoardProps {
  game: GameState;
  ref: RefObject<BoardFocus | null>;
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

// Once a click or tap completes, the focused card can move fully into view.
function cardClicked(card: HTMLElement): void {
  card.scrollIntoView(NEAREST);
}

export function Board({ game, ref, activate }: BoardProps) {
  const viewport = useRef<HTMLDivElement>(null);
  const element = useRef<HTMLDivElement>(null);
  const [focused, setFocused] = useState(0);
  const scrollToNearest = useNearestScroll(viewport);
  const { size, deck, matched, selection } = game;
  // A clock tick keeps both arrays, so the cards' props stay equal and no card renders again.
  const faceUp = useMemo(() => faceUpCards({ matched, selection }), [matched, selection]);
  const matchedCards = useMemo(() => new Set(matched), [matched]);
  // The game passes a new handler on each render. Cards keep one callback that calls the latest.
  const latestActivate = useRef(activate);
  useLayoutEffect(() => {
    latestActivate.current = activate;
  });
  const activateCard = useCallback((index: number) => latestActivate.current(index), []);
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
  const style: BoardStyle = { '--grid-size': size };
  return (
    <div
      ref={viewport}
      className="board-viewport"
      role="region"
      aria-label="Game board"
      aria-describedby="board-help"
    >
      <div className="board" ref={element} style={style}>
        {deck.map((face, index) => (
          <Card
            key={index}
            index={index}
            size={size}
            face={faceUp.has(index) ? face : null}
            matched={matchedCards.has(index)}
            tabIndex={focused === index ? 0 : -1}
            activate={activateCard}
            focus={cardFocused}
            clicked={cardClicked}
            navigate={navigate}
          />
        ))}
      </div>
    </div>
  );
}
