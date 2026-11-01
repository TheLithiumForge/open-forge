import type { GameState } from '../game.types.ts';
import type { Preferences } from './storage.ts';

export interface Session {
  readonly game: GameState;
  readonly paused: boolean;
  readonly generation: number;
  readonly feedback: string;
  readonly celebration: boolean;
  readonly preferences: Preferences;
}
