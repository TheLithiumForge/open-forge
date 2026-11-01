import { useRef, useState } from 'react';
import { encodeSnapshot } from '../snapshot.ts';
import type { GameState } from '../game.types.ts';

interface ReproductionProps {
  game: GameState;
  generation: number;
  loadBoard(code: string): boolean;
  loadSnapshot(text: string): boolean;
  notice: string;
  paused: boolean;
  resume(): void;
}

export function Reproduction({
  game,
  generation,
  loadBoard,
  loadSnapshot,
  notice,
  paused,
  resume,
}: ReproductionProps) {
  const [snapshot, setSnapshot] = useState('');
  const loadButton = useRef<HTMLButtonElement>(null);
  return (
    <>
      <details className="reproduction">
        <summary>{ReproductionControl.summary}</summary>
        <div className="reproduction-body">
          <form
            onSubmit={(event) => {
              event.preventDefault();
              const code = new FormData(event.currentTarget).get('boardCode');
              loadButton.current?.focus();
              if (typeof code === 'string') loadBoard(code);
            }}
          >
            <label htmlFor="board-code">{ReproductionControl.board}</label>
            <div className="control-row">
              <input
                key={generation}
                id="board-code"
                name="boardCode"
                defaultValue={game.boardCode}
                maxLength={BOARD_CODE_MAX_CHARACTERS}
                spellCheck={false}
                autoComplete="off"
              />
              <button ref={loadButton} type="submit" aria-describedby="load-notice">
                {ReproductionControl.loadBoard}
              </button>
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
            <button
              type="button"
              aria-describedby="load-notice"
              onClick={() => loadSnapshot(snapshot)}
            >
              {ReproductionControl.loadSnapshot}
            </button>
          </div>
        </div>
      </details>
      <div className="status-row">
        <p id="load-notice" className="load-notice">
          {notice}
        </p>
        {paused && (
          <button type="button" onClick={resume}>
            {ReproductionControl.resume}
          </button>
        )}
      </div>
    </>
  );
}

export const ReproductionControl = {
  summary: 'Reproduce a game',
  board: 'Board code',
  loadBoard: 'Load board',
  snapshot: 'Snapshot JSON',
  exportSnapshot: 'Export snapshot',
  loadSnapshot: 'Load snapshot',
  resume: 'Resume',
} as const;
export const BOARD_CODE_MAX_CHARACTERS = 32;
export const SNAPSHOT_MAX_CHARACTERS = 65_536;
