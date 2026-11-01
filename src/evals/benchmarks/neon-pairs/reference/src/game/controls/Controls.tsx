import { GRID_SIZES } from '../deck.ts';
import { DealControl } from './control-labels.ts';
import './controls.css';

interface ControlsProps {
  size: number;
  newGame(size?: number): void;
  restart(): void;
}

export function Controls({ size, newGame, restart }: ControlsProps) {
  return (
    <div className="controls">
      <div className="size-control">
        <label htmlFor="grid-size">{DealControl.size}</label>
        <select
          id="grid-size"
          value={size}
          onChange={(event) => newGame(Number(event.currentTarget.value))}
        >
          {GRID_SIZES.map((option) => (
            <option key={option} value={option}>
              {option}x{option}
            </option>
          ))}
        </select>
      </div>
      <button type="button" onClick={() => newGame()}>
        {DealControl.newGame}
      </button>
      <button type="button" onClick={restart}>
        {DealControl.restart}
      </button>
    </div>
  );
}
