import { isWon } from '../game.ts';
import { formatElapsed } from '../elapsed-time.ts';
import { StatusLabel } from './status-labels.ts';
import type { GameState } from '../game.types.ts';
import './status.css';

interface GameStatusProps {
  game: GameState;
  best: number | undefined;
  celebration: boolean;
}

export function GameStatus({ game, best, celebration }: GameStatusProps) {
  return (
    <>
      <dl className="stats">
        <div>
          <dt>{StatusLabel.moves}</dt>
          <dd aria-label={StatusLabel.moves}>{game.moves}</dd>
        </div>
        <div>
          <dt>{StatusLabel.pairs}</dt>
          <dd aria-label={StatusLabel.pairs}>
            {game.matched.length / 2} / {game.deck.length / 2}
          </dd>
        </div>
        <div>
          <dt>{StatusLabel.time}</dt>
          <dd aria-label={StatusLabel.time}>{formatElapsed(game.elapsedMs)}</dd>
        </div>
        <div>
          <dt>{StatusLabel.best}</dt>
          <dd aria-label={StatusLabel.best}>{best ?? 'Best: none'}</dd>
        </div>
      </dl>
      {isWon(game) && (
        <p className={`victory${celebration ? ' celebrate' : ''}`}>
          Every pair found in {game.moves} moves and {formatElapsed(game.elapsedMs)}. Well played.
        </p>
      )}
    </>
  );
}
