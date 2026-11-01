import { isWon } from '../game.ts';
import { formatElapsed } from '../elapsed-time.ts';
import { StatusLabel } from './status-labels.ts';
import type { GameState } from '../game.ts';
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
          <dt>{StatusLabel.moves}</dt> <dd>{game.moves}</dd>
        </div>
        <div>
          <dt>{StatusLabel.pairs}</dt>{' '}
          <dd>
            {game.matched.length / 2}/{game.deck.length / 2}
          </dd>
        </div>
        <div>
          <dt>{StatusLabel.time}</dt> <dd>{formatElapsed(game.elapsedMs)}</dd>
        </div>
        <div>
          <dt>{StatusLabel.best}</dt> <dd>{best ?? 'none'}</dd>
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
