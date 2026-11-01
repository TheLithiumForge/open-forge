import { nameChecks } from './names.ts';
import { snapshotChecks } from './snapshots.ts';
import { runWorker } from './worker-runtime.ts';

if (import.meta.main) await runWorker(process.argv.slice(2), [...nameChecks, ...snapshotChecks]);
