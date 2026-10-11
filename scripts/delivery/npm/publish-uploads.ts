import { setImmediate } from "node:timers/promises";
import type { Publication } from "./publication-package.ts";

export interface PublicationProgress {
  published: string[];
  existing: string[];
  interrupted: string[];
  notStarted: string[];
  stopped: boolean;
}

// Inject the upload boundary so stop requests can be tested without a registry.
export async function publishUploads(
  publications: readonly Publication[],
  existing: readonly boolean[],
  stopRequested: () => boolean,
  upload: (publication: Publication) => Promise<void>,
  promote: (publication: Publication) => Promise<void>,
): Promise<PublicationProgress> {
  const progress: PublicationProgress = { published: [], existing: [], interrupted: [], notStarted: [], stopped: false };
  for (const [index, publication] of publications.entries()) {
    // Deliver any signals deferred by synchronous registry preflight or the previous effect.
    await setImmediate();
    if (stopRequested()) break;
    if (existing[index]) {
      progress.existing.push(publication.name);
      process.stderr.write(`Warning: ${publication.name}@${publication.version} already exists; skipping publication and leaving its npm tags unchanged.\n`);
      continue;
    }
    try {
      await upload(publication);
    } catch (error) {
      if (!stopRequested()) throw error;
      progress.interrupted.push(publication.name);
      break;
    }
    progress.published.push(publication.name);
    await setImmediate();
    if (stopRequested()) break;
    try {
      await promote(publication);
    } catch (error) {
      if (!stopRequested()) throw error;
      break;
    }
  }
  progress.stopped = stopRequested();
  const started = new Set([...progress.published, ...progress.existing, ...progress.interrupted]);
  progress.notStarted = publications.filter((publication) => !started.has(publication.name)).map((publication) => publication.name);
  return progress;
}

export function cancellationReport(progress: PublicationProgress): string {
  return [
    "Publication stopped after a cancellation request.",
    `Published: ${progress.published.join(", ") || "none"}`,
    `Interrupted (final state unknown): ${progress.interrupted.join(", ") || "none"}`,
    `Not started: ${progress.notStarted.join(", ") || "none"}`,
    `Already existed: ${progress.existing.join(", ") || "none"}`,
  ].join("\n");
}
