import { GRID_SIZES } from '../rules.ts';
import './controls.css';

interface ControlsProps { size: number; newGame(size?: number): void; restart(): void }

export function Controls({ size, newGame, restart }: ControlsProps) {
  return (
    <div className="controls">
      <div className="size-control"><label htmlFor="grid-size">Grid size</label>
        <select id="grid-size" value={size} onChange={(event) => newGame(Number(event.currentTarget.value))}>
          {GRID_SIZES.map((option) => <option key={option} value={option}>{option}x{option}</option>)}
        </select>
      </div>
      <button type="button" onClick={() => newGame()}>New Game</button>
      <button type="button" onClick={restart}>Restart</button>
    </div>
  );
}
