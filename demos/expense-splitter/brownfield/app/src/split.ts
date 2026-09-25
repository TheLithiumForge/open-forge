export interface Share {
  readonly person: string;
  readonly cents: number;
}

// Splits a total evenly. Cents that don't divide evenly go one each to the
// people at the front of the list. The payer is always listed first, so the
// payer absorbs the rounding. See NOTES.md for why.
export function splitEvenly(totalCents: number, people: readonly string[]): Share[] {
  if (people.length === 0) {
    throw new Error("A split needs at least one person.");
  }
  const base = Math.floor(totalCents / people.length);
  const leftover = totalCents - base * people.length;
  return people.map((person, index) => ({ person, cents: base + (index < leftover ? 1 : 0) }));
}
