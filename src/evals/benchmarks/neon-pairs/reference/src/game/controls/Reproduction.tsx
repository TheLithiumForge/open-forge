import { useRef, useState } from 'react';
import {
  BOARD_CODE_MAX_CHARACTERS,
  ReproductionControl,
  SNAPSHOT_MAX_CHARACTERS,
} from './control-labels.ts';
import type { GameState } from '../game.ts';
import '../status-row.css';
import './control.css';
import './reproduction.css';

interface ReproductionProps {
  game: GameState;
  generation: number;
  exportSnapshot(): string;
  loadBoard(code: string): boolean;
  loadSnapshot(text: string): boolean;
  notice: string;
  paused: boolean;
  resume(): void;
}

export function Reproduction({
  game,
  generation,
  exportSnapshot,
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
                className="control"
                name="boardCode"
                defaultValue={game.boardCode}
                maxLength={BOARD_CODE_MAX_CHARACTERS}
                spellCheck={false}
                autoComplete="off"
              />
              <button
                ref={loadButton}
                type="submit"
                className="control"
                aria-describedby="load-notice"
              >
                {ReproductionControl.loadBoard}
              </button>
            </div>
          </form>
          <label htmlFor="snapshot-json">{ReproductionControl.snapshot}</label>
          <textarea
            id="snapshot-json"
            className="control"
            rows={5}
            value={snapshot}
            maxLength={SNAPSHOT_MAX_CHARACTERS}
            spellCheck={false}
            onChange={(event) => setSnapshot(event.currentTarget.value)}
          />
          <div className="control-row">
            <button type="button" className="control" onClick={() => setSnapshot(exportSnapshot())}>
              {ReproductionControl.exportSnapshot}
            </button>
            <button
              type="button"
              className="control"
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
          <button type="button" className="control" onClick={resume}>
            {ReproductionControl.resume}
          </button>
        )}
      </div>
    </>
  );
}
