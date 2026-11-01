import { useEffect } from 'react';
import { writePreferences } from './storage.ts';
import type { Preferences, PreferenceStore } from './storage.ts';

// Reading localStorage can throw when the browser blocks storage, and play continues without it.
export function browserStore(): PreferenceStore | null {
  try {
    return window.localStorage;
  } catch {
    return null;
  }
}

// Saves this tab's size and bests after each change.
export function useSavedPreferences(store: PreferenceStore | null, preferences: Preferences) {
  useEffect(() => {
    writePreferences(store, preferences);
  }, [preferences, store]);
}
