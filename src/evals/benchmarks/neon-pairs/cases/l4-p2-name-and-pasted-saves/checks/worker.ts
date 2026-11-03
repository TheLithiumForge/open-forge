import { nameChecks } from './names.ts';
import { snapshotChecks } from './snapshots.ts';
import { repairChecks } from './repair.ts';
import { compatibilityChecks } from './compatibility.ts';
import { runWorker } from './worker-runtime.ts';

if (import.meta.main) await runWorker(process.argv.slice(2), [...nameChecks, ...snapshotChecks, ...repairChecks.slice(0, -1), ...compatibilityChecks]);
