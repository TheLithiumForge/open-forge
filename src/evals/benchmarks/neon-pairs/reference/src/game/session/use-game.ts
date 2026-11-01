import { useEffect, useRef, useState } from 'react';
import { createGame } from '../game.ts';
import { parseBoardCode } from '../deck.ts';
import { decodeSnapshot } from '../snapshot.ts';
import { validSize } from '../rules.ts';
import { CLOCK_INTERVAL_MS, elapsedStep } from './browser-clock.ts';
import { freshBoardCode } from './fresh-board.ts';
import { activateSession, initialSession, replaceGame, tickSession } from './session.ts';
import { readPreferences, writePreferences } from './storage.ts';
import type { PreferenceStore } from './storage.ts';
import type { Session } from './session.types.ts';

function browserStore(): PreferenceStore | null {
  try { return window.localStorage; } catch { return null; }
}

export function useGame() {
  const [store] = useState(browserStore);
  const [session, setSession] = useState(() => {
    const preferences = readPreferences(store);
    return initialSession(freshBoardCode(preferences.size), preferences);
  });
  const current = useRef(session);
  const clock = useRef(performance.now());
  const visible = useRef(!document.hidden);

  function commit(next: Session): void {
    if (next === current.current) return;
    current.current = next;
    setSession(next);
  }

  function synchronize(generation = current.current.generation): void {
    if (current.current.generation !== generation) return;
    const step = elapsedStep(clock.current, performance.now());
    clock.current = step.next;
    if (visible.current) commit(tickSession(current.current, generation, step.milliseconds));
  }

  useEffect(() => {
    const generation = session.generation;
    let active = true;
    clock.current = performance.now();
    const timer = window.setInterval(() => { if (active) synchronize(generation); }, CLOCK_INTERVAL_MS);
    function visibilityChanged(): void {
      if (!active || current.current.generation !== generation) return;
      synchronize(generation);
      visible.current = !document.hidden;
      clock.current = performance.now();
    }
    document.addEventListener('visibilitychange', visibilityChanged);
    return () => {
      active = false;
      window.clearInterval(timer);
      document.removeEventListener('visibilitychange', visibilityChanged);
    };
  }, [session.generation]);

  useEffect(() => writePreferences(store, session.preferences), [session.preferences, store]);

  function newGame(size = current.current.game.size): void {
    if (!validSize(size)) return;
    const next = createGame(freshBoardCode(size, current.current.game.seed));
    clock.current = performance.now();
    commit(replaceGame(current.current, next, false));
  }

  function restart(): void {
    clock.current = performance.now();
    commit(replaceGame(current.current, createGame(current.current.game.boardCode), false));
  }

  function loadBoard(code: string): boolean {
    if (!parseBoardCode(code)) { commit({ ...current.current, feedback: 'Invalid board code.' }); return false; }
    commit(replaceGame(current.current, createGame(code), false));
    clock.current = performance.now();
    return true;
  }

  function loadSnapshot(text: string): boolean {
    const game = decodeSnapshot(text);
    if (!game) { commit({ ...current.current, feedback: 'Invalid snapshot.' }); return false; }
    commit(replaceGame(current.current, game, true));
    clock.current = performance.now();
    return true;
  }

  function activate(index: number): void {
    synchronize();
    commit(activateSession(current.current, index));
  }

  function resume(): void {
    clock.current = performance.now();
    commit({ ...current.current, paused: false, feedback: 'Game resumed.' });
  }

  return { session, newGame, restart, loadBoard, loadSnapshot, activate, resume };
}
