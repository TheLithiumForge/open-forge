import { useState } from 'react';
import { SelectionKind } from '../selection.ts';
import { Card } from './Card.tsx';
import { navigationTarget } from './keyboard.ts';
import type { CSSProperties, KeyboardEvent, RefObject } from 'react';
import type { GameState } from '../game.types.ts';
import './board.css';

interface BoardProps {
  game: GameState;
  element: RefObject<HTMLDivElement | null>;
  paused: boolean;
  activate(index: number): void;
}
interface BoardStyle extends CSSProperties {
  '--grid-size': number;
}

export function Board({ game, element, paused, activate }: BoardProps) {
  const [focused, setFocused] = useState(0);

  function navigate(event: KeyboardEvent<HTMLButtonElement>, index: number): void {
    const target = navigationTarget(index, game.size, event.key, event.ctrlKey);
    if (target === null) return;
    event.preventDefault();
    const card = element.current?.querySelectorAll('button').item(target);
    card?.focus();
  }
  function revealed(index: number): boolean {
    const selection = game.selection;
    return (
      game.matched.includes(index) ||
      (selection.kind === SelectionKind.one && selection.index === index) ||
      (selection.kind === SelectionKind.mismatch &&
        (selection.first === index || selection.second === index))
    );
  }
  const style: BoardStyle = { '--grid-size': game.size };
  return (
    <div
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
            face={revealed(index) ? face : null}
            matched={game.matched.includes(index)}
            blocked={paused || game.selection.kind === SelectionKind.mismatch}
            tabIndex={focused === index ? 0 : -1}
            activate={activate}
            focus={setFocused}
            navigate={navigate}
          />
        ))}
      </div>
    </div>
  );
}
