import { useRef } from 'react';
import { Board } from './board/Board.tsx';
import { Controls } from './controls/Controls.tsx';
import { Reproduction } from './controls/Reproduction.tsx';
import { ReproductionControl } from './controls/reproduction-controls.ts';
import { GameStatus } from './status/GameStatus.tsx';
import { useGame } from './session/use-game.ts';
import './game.css';

export function Game() {
  const controller = useGame();
  const { session } = controller;
  const { game } = session;
  const board = useRef<HTMLDivElement>(null);
  function resume(): void {
    board.current?.querySelector<HTMLButtonElement>('[tabindex="0"]')?.focus();
    controller.resume();
  }
  return (
    <main>
      <header>
        <p className="eyebrow">A little focus. A perfect pair.</p>
        <h1>Neon Pairs</h1>
      </header>
      <p className="intro">Reveal two cards. Match their shape and marker to keep the pair.</p>
      <Controls size={game.size} newGame={controller.newGame} restart={controller.restart} />
      <GameStatus
        game={game}
        best={session.preferences.bestMoves[String(game.size)]}
        celebration={session.celebration}
      />
      <p id="board-help" className="help">
        Use a pointer or Tab to enter the board. Arrow keys move between cards. Enter or Space
        reveals a card.
      </p>
      <div className="status-row">
        <p role="status" aria-live="polite" aria-atomic="true">
          <span key={session.announcement}>{session.feedback}</span>
        </p>
        {session.paused && (
          <button type="button" onClick={resume}>
            {ReproductionControl.resume}
          </button>
        )}
      </div>
      <Board
        key={session.generation}
        game={game}
        paused={session.paused}
        activate={controller.activate}
        element={board}
      />
      <Reproduction
        game={game}
        generation={session.generation}
        loadBoard={controller.loadBoard}
        loadSnapshot={controller.loadSnapshot}
      />
    </main>
  );
}
