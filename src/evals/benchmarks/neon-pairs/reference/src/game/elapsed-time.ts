const MILLISECONDS_PER_SECOND = 1000;
const SECONDS_PER_MINUTE = 60;

export function formatElapsed(milliseconds: number): string {
  const seconds = Math.floor(milliseconds / MILLISECONDS_PER_SECOND);
  const minutes = Math.floor(seconds / SECONDS_PER_MINUTE);
  return `${minutes}:${String(seconds % SECONDS_PER_MINUTE).padStart(2, '0')}`;
}
