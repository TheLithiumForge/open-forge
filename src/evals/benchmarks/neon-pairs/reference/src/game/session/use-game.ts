import { useEffect, useRef, useState } from 'react';
import { createGame } from '../game.ts';
import { parseBoardCode, validSize } from '../deck.ts';
import { decodeSnapshot } from '../snapshot.ts';
import { SelectionKind } from '../selection.ts';
import { freshBoardCode } from './fresh-board.ts';
import { activateSession, initialSession, replaceGame, SessionNotice } from './session.ts';
import { readPreferences, writePreferences } from './storage.ts';
import { useSessionClock } from './use-session-clock.ts';
import type { PreferenceStore } from './storage.ts';
import type { Session } from './session.types.ts';

function browserStore(): PreferenceStore | null {
  try {
    return window.localStorage;
  } catch {
    return null;
  }
}

export function useGame() {
  const [store] = useState(browserStore);
  const [session, setSession] = useState(() => {
    const preferences = readPreferences(store);
    return initialSession(freshBoardCode(preferences.size), preferences);
  });
  const current = useRef(session);

  function commit(next: Session): void {
    if (next === current.current) return;
    current.current = next;
    setSession(next);
  }

  const clock = useSessionClock(session, current, commit);
  useEffect(() => writePreferences(store, session.preferences), [session.preferences, store]);

  function newGame(size = current.current.game.size): void {
    if (!validSize(size)) return;
    const next = createGame(freshBoardCode(size, current.current.game.seed));
    clock.reset();
    commit(replaceGame(current.current, next, false));
  }

  function restart(): void {
    clock.reset();
    commit(replaceGame(current.current, createGame(current.current.game.boardCode), false));
  }

  function notice(feedback: string): void {
    commit({ ...current.current, feedback, announcement: current.current.announcement + 1 });
  }

  function loadBoard(code: string): boolean {
    if (!parseBoardCode(code)) {
      notice(SessionNotice.invalidBoard);
      return false;
    }
    clock.reset();
    commit(replaceGame(current.current, createGame(code), false));
    return true;
  }

  function loadSnapshot(text: string): boolean {
    const game = decodeSnapshot(text);
    if (!game) {
      notice(SessionNotice.invalidSnapshot);
      return false;
    }
    clock.reset();
    commit(replaceGame(current.current, game, true));
    return true;
  }

  function activate(index: number): void {
    clock.synchronize();
    const previous = current.current;
    const next = activateSession(previous, index);
    if (
      previous.game.selection.kind !== SelectionKind.mismatch &&
      next.game.selection.kind === SelectionKind.mismatch
    )
      clock.beginMismatch();
    commit(next);
  }

  function resume(): void {
    clock.reset();
    commit({
      ...current.current,
      paused: false,
      feedback: SessionNotice.resumed,
      announcement: current.current.announcement + 1,
    });
  }

  return { session, newGame, restart, loadBoard, loadSnapshot, activate, resume };
}
