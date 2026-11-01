import { useEffect, useState } from 'react';
import { encodeSnapshot } from '../snapshot.ts';
import {
  BOARD_CODE_MAX_CHARACTERS,
  ReproductionControl,
  SNAPSHOT_MAX_CHARACTERS,
} from './reproduction-controls.ts';
import type { GameState } from '../game.types.ts';

interface ReproductionProps {
  game: GameState;
  generation: number;
  loadBoard(code: string): boolean;
  loadSnapshot(text: string): boolean;
}

export function Reproduction({ game, generation, loadBoard, loadSnapshot }: ReproductionProps) {
  const [code, setCode] = useState(game.boardCode);
  const [snapshot, setSnapshot] = useState('');
  useEffect(() => setCode(game.boardCode), [generation, game.boardCode]);
  return (
    <details className="reproduction">
      <summary>{ReproductionControl.summary}</summary>
      <div className="reproduction-body">
        <form
          onSubmit={(event) => {
            event.preventDefault();
            loadBoard(code);
          }}
        >
          <label htmlFor="board-code">{ReproductionControl.board}</label>
          <div className="control-row">
            <input
              id="board-code"
              value={code}
              maxLength={BOARD_CODE_MAX_CHARACTERS}
              spellCheck={false}
              autoComplete="off"
              onChange={(event) => setCode(event.currentTarget.value)}
            />
            <button type="submit">{ReproductionControl.loadBoard}</button>
          </div>
        </form>
        <label htmlFor="snapshot-json">{ReproductionControl.snapshot}</label>
        <textarea
          id="snapshot-json"
          rows={5}
          value={snapshot}
          maxLength={SNAPSHOT_MAX_CHARACTERS}
          spellCheck={false}
          onChange={(event) => setSnapshot(event.currentTarget.value)}
        />
        <div className="control-row">
          <button type="button" onClick={() => setSnapshot(encodeSnapshot(game))}>
            {ReproductionControl.exportSnapshot}
          </button>
          <button type="button" onClick={() => loadSnapshot(snapshot)}>
            {ReproductionControl.loadSnapshot}
          </button>
        </div>
      </div>
    </details>
  );
}
