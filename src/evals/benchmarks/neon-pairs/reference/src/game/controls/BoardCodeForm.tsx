import { useRef } from 'react';
import { ReproductionControl } from './control-labels.ts';
import './control.css';
import './reproduction.css';

const BOARD_CODE_MAX_CHARACTERS = 32;
interface BoardCodeFormProps {
  code: string;
  generation: number;
  loadBoard(code: string): boolean;
}

export function BoardCodeForm({ code, generation, loadBoard }: BoardCodeFormProps) {
  const loadButton = useRef<HTMLButtonElement>(null);
  return (
    <form
      onSubmit={(event) => {
        event.preventDefault();
        const draft = new FormData(event.currentTarget).get('boardCode');
        // A loaded deal replaces the input, so focus moves to Load board. An invalid code
        // keeps focus in the draft for correction.
        if (typeof draft === 'string' && loadBoard(draft)) loadButton.current?.focus();
      }}
    >
      <label htmlFor="board-code">{ReproductionControl.board}</label>
      <div className="control-row">
        <input
          key={generation}
          id="board-code"
          className="control"
          name="boardCode"
          defaultValue={code}
          maxLength={BOARD_CODE_MAX_CHARACTERS}
          spellCheck={false}
          autoComplete="off"
        />
        <button ref={loadButton} type="submit" className="control" aria-describedby="load-notice">
          {ReproductionControl.loadBoard}
        </button>
      </div>
    </form>
  );
}
