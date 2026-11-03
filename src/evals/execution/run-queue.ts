/**
 * Works through items with up to `limit` at once, in their order. Once `stop` returns true, no new
 * item starts, and the call returns when the ones already started finish.
 */
export async function runInParallel<T>(items: readonly T[], limit: number, work: (item: T, index: number) => Promise<void>, stop: () => boolean): Promise<void> {
  let next = 0;
  const worker = async (): Promise<void> => {
    while (!stop()) {
      const index = next;
      next += 1;
      const item = items[index];
      if (item === undefined) return;
      // One failed item never stops the others. The work reports its own failure.
      await work(item, index).catch(() => undefined);
    }
  };
  await Promise.all(Array.from({ length: Math.max(1, Math.min(limit, items.length)) }, () => worker()));
}
