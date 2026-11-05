import { accessChecks } from './access-checks.ts';
import { competitionChecks } from './competition-checks.ts';
import { coreChecks } from './core-checks.ts';
import { exchangeChecks } from './exchange-checks.ts';
import { nameChecks } from './name-checks.ts';
import { persistenceChecks } from './persistence-checks.ts';
import { sourceChecks } from './source-checks.ts';
import { runWorker } from './worker-runtime.ts';
import { writingChecks } from './writing-checks.ts';

export const AllChecks = [...coreChecks, ...competitionChecks, ...nameChecks, ...persistenceChecks, ...exchangeChecks, ...accessChecks, ...writingChecks, ...sourceChecks];

if (import.meta.main) await runWorker(process.argv.slice(2), AllChecks);
