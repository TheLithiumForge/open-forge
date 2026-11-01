import { useEffect } from 'react';
import { writePreferences } from './storage.ts';
import type { Preferences, PreferenceStore } from './storage.ts';

// Saves this tab's size and bests after each change.
export function useSavedPreferences(store: PreferenceStore | null, preferences: Preferences) {
  useEffect(() => {
    writePreferences(store, preferences);
  }, [preferences, store]);
}
