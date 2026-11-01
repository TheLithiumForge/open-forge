import { runReplay } from './replay-cli.ts';

process.exitCode = await runReplay(process.argv.slice(2));
