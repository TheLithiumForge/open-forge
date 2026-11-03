import { repairChecks } from './repair.ts';
import { runWorker } from './worker-runtime.ts';

if (import.meta.main) await runWorker(process.argv.slice(2), repairChecks);
