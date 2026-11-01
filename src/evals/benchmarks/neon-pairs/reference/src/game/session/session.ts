import { activateCard, advanceGame, createGame, isWon } from '../game.ts';
import { SelectionKind } from '../rules.ts';
import { rememberBest } from './storage.ts';
import type { GameState } from '../game.types.ts';
import type { Preferences } from './storage.ts';
import type { Session } from './session.types.ts';

export function initialSession(code: string, preferences: Preferences): Session {
  return { game: createGame(code), paused: false, generation: 0, feedback: 'Find every matching pair.', celebration: false, preferences };
}

export function replaceGame(session: Session, game: GameState, paused: boolean): Session {
  return {
    ...session, game, paused, generation: session.generation + 1, celebration: false,
    preferences: { ...session.preferences, size: game.size },
    feedback: paused ? 'Loaded snapshot. Select Resume to continue.' : 'New game ready.',
  };
}

export function tickSession(session: Session, generation: number, milliseconds: number): Session {
  if (session.generation !== generation || session.paused) return session;
  const game = advanceGame(session.game, milliseconds);
  if (game === session.game) return session;
  const closedMismatch = session.game.selection.kind === SelectionKind.mismatch && game.selection.kind === SelectionKind.closed;
  return { ...session, game, feedback: closedMismatch ? 'No match. Try another pair.' : session.feedback };
}

export function activateSession(session: Session, index: number): Session {
  if (session.paused) return session;
  const game = activateCard(session.game, index);
  if (game === session.game) return session;
  const won = isWon(game);
  const feedback = won ? `All pairs found in ${game.moves} moves.`
    : game.selection.kind === SelectionKind.one ? 'One card revealed. Choose another card.'
    : game.selection.kind === SelectionKind.mismatch ? 'No match. Cards close after a short pause.'
    : `Pair found. ${game.matched.length / 2} pairs matched.`;
  const preferences = won ? rememberBest(session.preferences, game.size, game.moves) : session.preferences;
  return { ...session, game, feedback, preferences, celebration: won };
}
