import { isWon } from './game.ts';
import { Board } from './board/Board.tsx';
import { Controls } from './controls/Controls.tsx';
import { Reproduction } from './controls/Reproduction.tsx';
import { useGame } from './session/use-game.ts';
import './game.css';

export function Game() {
  const controller = useGame();
  const { session } = controller;
  const { game } = session;
  const won = isWon(game);
  const best = session.preferences.bestMoves[String(game.size)];
  return (
    <main>
      <header><p className="eyebrow">A little focus. A perfect pair.</p><h1>Neon Pairs</h1></header>
      <p className="intro">Reveal two cards. Match their shape and marker to keep the pair.</p>
      <Controls size={game.size} newGame={controller.newGame} restart={controller.restart} />
      <dl className="stats">
        <div><dt>Moves</dt><dd aria-label="Moves">{game.moves}</dd></div>
        <div><dt>Pairs</dt><dd aria-label="Pairs">{game.matched.length / 2} / {game.deck.length / 2}</dd></div>
        <div><dt>Active time</dt><dd aria-label="Active time">{Math.floor(game.elapsedMs / 1000)}s</dd></div>
        <div><dt>Best moves</dt><dd aria-label="Best moves">{best ?? '—'}</dd></div>
      </dl>
      <p id="board-help" className="help">Use a pointer or Tab to enter the board. Arrow keys move between cards. Enter or Space reveals a card.</p>
      <div className="status-row">
        <p role="status" aria-live="polite" aria-atomic="true">{session.feedback}</p>
        {session.paused && <button type="button" onClick={controller.resume}>Resume</button>}
      </div>
      {won && <p className={`victory${session.celebration ? ' celebrate' : ''}`}>Every pair found. Well played.</p>}
      <Board game={game} generation={session.generation} paused={session.paused} activate={controller.activate} />
      <Reproduction game={game} generation={session.generation} loadBoard={controller.loadBoard} loadSnapshot={controller.loadSnapshot} />
    </main>
  );
}
