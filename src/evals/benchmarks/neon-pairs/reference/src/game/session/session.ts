import { activateCard, advanceGame, createGame, isWon } from '../game.ts';
import { SelectionKind } from '../selection.ts';
import { formatElapsed } from '../elapsed-time.ts';
import { rememberBest } from './storage.ts';
import type { GameState } from '../game.types.ts';
import type { Preferences } from './storage.ts';
import type { Session } from './session.types.ts';

export const SessionNotice = {
  ready: 'Find every matching pair.',
  newGame: 'New game ready.',
  loaded: 'Loaded snapshot. Select Resume to continue.',
  invalidBoard: 'Invalid board code.',
  invalidSnapshot: 'Invalid snapshot.',
  resumed: 'Game resumed.',
} as const;

export function initialSession(code: string, preferences: Preferences): Session {
  return {
    game: createGame(code),
    paused: false,
    generation: 0,
    feedback: SessionNotice.ready,
    announcement: 0,
    celebration: false,
    preferences,
  };
}

export function replaceGame(session: Session, game: GameState, paused: boolean): Session {
  return {
    ...session,
    game,
    paused,
    generation: session.generation + 1,
    celebration: false,
    preferences: { ...session.preferences, size: game.size },
    announcement: session.announcement + 1,
    feedback: paused ? SessionNotice.loaded : SessionNotice.newGame,
  };
}

export function tickSession(session: Session, generation: number, milliseconds: number): Session {
  if (session.generation !== generation || session.paused) return session;
  const game = advanceGame(session.game, milliseconds);
  return game === session.game ? session : { ...session, game };
}

export function activateSession(session: Session, index: number): Session {
  if (session.paused) return session;
  const game = activateCard(session.game, index);
  if (game === session.game) return session;
  if (game.selection.kind === SelectionKind.one) return { ...session, game };
  const won = isWon(game);
  const feedback = won
    ? `Pair found. All pairs found in ${game.moves} moves and ${formatElapsed(game.elapsedMs)}.`
    : game.selection.kind === SelectionKind.mismatch
      ? 'No match. Cards close after a short pause.'
      : `Pair found. ${game.matched.length / 2} ${game.matched.length === 2 ? 'pair' : 'pairs'} matched.`;
  const preferences = won
    ? rememberBest(session.preferences, game.size, game.moves)
    : session.preferences;
  return {
    ...session,
    game,
    feedback,
    announcement: session.announcement + 1,
    preferences,
    celebration: won,
  };
}
