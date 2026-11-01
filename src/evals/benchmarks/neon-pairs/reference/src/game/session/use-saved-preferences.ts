import { useEffect } from 'react';
import { mergeSavedBests, writePreferences } from './storage.ts';
import type { RefObject } from 'react';
import type { Session } from './session.ts';
import type { Preferences, PreferenceStore } from './storage.ts';

// Saves preferences after each change and shows the merged bests, including records another tab
// saved since this one loaded.
export function useSavedPreferences(
  store: PreferenceStore | null,
  session: Session,
  current: RefObject<Session>,
  commit: (next: Session) => void,
) {
  function show(merged: Preferences): void {
    if (merged !== current.current.preferences) commit({ ...current.current, preferences: merged });
  }

  useEffect(() => {
    show(writePreferences(store, current.current.preferences));
  }, [session.preferences, store]);

  useEffect(() => {
    function savedElsewhere(): void {
      show(mergeSavedBests(store, current.current.preferences));
    }
    window.addEventListener('storage', savedElsewhere);
    return () => window.removeEventListener('storage', savedElsewhere);
  }, [store]);
}
