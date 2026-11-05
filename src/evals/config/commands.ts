import { ImplementerPresets, JudgePresets, UserPresets } from "./presets.ts";
import { MachineFlag, MachineHelp } from "./machine-options.ts";

export const Command = { doctor: "doctor", images: "images", smoke: "smoke", qualify: "qualify", validateCase: "validate-case", chainCheck: "chain-check", run: "run", resume: "resume", progress: "progress", usage: "usage", analyze: "analyze", reference: "reference", summary: "summary", site: "site", check: "check", judge: "judge", results: "results", help: "help" } as const;
export type Command = (typeof Command)[keyof typeof Command];
export const Flag = { tag: "--tag", ref: "--ref", imported: "--imported", help: "--help", plan: "--plan", implementers: "--implementers", judges: "--judges", user: "--user", layer: "--layer", case: "--case", all: "--all", batch: "--batch", parallel: "--parallel", acceptHarness: "--accept-harness", port: "--port", noBrowser: "--no-browser", development: "--development", rebuild: "--rebuild", missing: "--missing", repository: "--repository", setups: "--setups", parent: "--parent" } as const;
export const ResultsAction = { add: "add", import: "import", grade: "grade" } as const;
export const CommandHelp = {
  reference: "Grade the tracked Neon Pairs reference without an agent. Defaults to --layer 3 --judges claude. Example: npm run evals -- reference --layer 3 --judges both",
  doctor: "Check Docker, Node, pinned tools, sign-ins and image digests. Example: npm run evals -- doctor",
  images: "Build the three images from locked digests, or only the kinds named: agent, verifier or assessor. --tag prints each image's local name instead of building it. Example: npm run evals -- images verifier",
  smoke: "Run a mock implementation run through every stage and saved records, without Docker or models. Example: npm run evals -- smoke",
  qualify: "Check fresh setup files and the pinned Open Forge version without models. Example: npm run evals -- qualify --plan src/evals/experiments/neon-pairs-l1.json",
  [Command.validateCase]: "Validate a Layer 4 starting project and solution in verifiers without models. Examples: npm run evals -- validate-case --case l4-f1-name-saves | npm run evals -- validate-case --all --plan src/evals/experiments/neon-pairs-l4-check.json",
  [Command.chainCheck]: "Before a later task of a task-by-task packet runs, build one child start per setup from the previous task's runs, grade it as it is in verifiers without models, and list every check whose result differs from the parent's. Example: npm run evals -- chain-check --plan src/evals/experiments/neon-pairs-p1-task-by-task.json --case l4-b2-arrow-keys --parent <batch>",
  run: `Run a saved plan. --implementers: ${Object.keys(ImplementerPresets).join(", ")}. --judges: ${Object.keys(JudgePresets).join(", ")}. --user: ${Object.keys(UserPresets).join(", ")}. Each replaces that role in the plan. --case runs one case of a Layer 4 plan, so each case can run as its own process. --setups runs some of the plan's setups, always with its control. --parent names the previous milestone's batch for a chain plan. --parallel caps how many runs go at once. Local isolation needs --development. Example: npm run evals -- run --plan src/evals/experiments/neon-pairs-l1-check.json --implementers both --judges both --user claude`,
  resume: "Continue a batch that paused at a usage limit or a rejected sign-in, or stopped early. Finished runs and judgments are kept, and a run stopped while it was graded is graded from its saved files. --parallel caps how many runs go at once. --accept-harness continues on changed runner code and records it in the batch, while changed inputs, checks, rubric, tooling or models still need a new batch. Example: npm run evals -- resume <batch> --parallel 2 --accept-harness",
  progress: "Show how far each batch has come: implemented, checked and judged runs, then each setup's mean check coverage and each judge's yes share. These are preliminary scores while a batch runs. Names batches, or shows every model batch. Example: npm run evals -- progress <batch> <batch>",
  usage: "Show where each setup's tokens went, from saved runs and without saving anything: exact new, cached and output tokens, requests, peak context and the share of input before the first edit, then estimated input tokens by class and file type and tool calls by kind. It reads the same analysis analyze saves. Names batches, or shows every model batch. Example: npm run evals -- usage <batch>",
  analyze: "Work out each saved run's objective measures and append them as an analysis record: exact token totals, tokens by class and file type, tool calls by kind and the steps before the first edit. Codex's session log and Claude's stream give each request's exact input, so only the split of each request's growth is estimated. Codex runs saved without a session log get a rougher estimate from their events. Names batches, or analyzes every model batch. --missing analyzes only runs without a current analysis, such as runs from before this analyzer version. Example: npm run evals -- analyze <batch> --missing",
  summary: "Print reference history, or implementer and judge pairs for --batch. Example: npm run evals -- summary --batch <id>",
  site: "Start the docs site on this machine and open its Results page with your local runs, including batches still running. A data server on the next port serves them to the page only. --repository shows a folder in the results repository's layout instead, such as src/evals/results. --port picks the docs port, 4320 by default. --no-browser starts both without opening a browser. Example: npm run evals -- site --port 4320",
  check: "Check saved runs in the verifier and append the results, as many at once as the check slots allow. Name a batch, and optionally one run. --rebuild builds the saved project again first. --missing checks only runs without a check free of errors, so it finishes a batch that checked later, or a fixed checker finishes one without grading it all again. Every check also saves its diagnostics, such as Layer 1 card crops and look comparisons. Example: npm run evals -- check <batch> --missing",
  judge: `Judge saved runs and append the ballots. Name a batch, and optionally one run. --judges: ${Object.keys(JudgePresets).join(", ")}. Defaults to the batch's saved judges. --missing judges only runs that judge hasn't judged without an error, so it can continue after a usage limit. Example: npm run evals -- judge <batch> --judges claude --missing`,
  results: "Add a finished batch, with its code, game, screenshots and conversations, to the results repository, replacing an earlier copy, then validate the repository. Evidence holding a sign-in, a machine path or the user name stops it. --repository picks another results repository than src/evals/results. results grade starts the results repository's grading workflow, which checks every published run waiting for its checks with this repository's pushed graders and pushes the grades to its main branch. results import, given a published batch folder, rebuilds that batch in the run store, so check --missing and judge --missing can grade it on any machine before results add publishes it again. Example: npm run evals -- results add <batch>",
  help: "Show command help. Example: npm run evals -- help run",
} as const satisfies Record<Command, string>;

export interface CliRequest { readonly command: Command; readonly args: readonly string[]; readonly help: boolean }

export function parseCommand(argv: readonly string[]): CliRequest {
  const [name = Command.help, ...args] = argv;
  const command = Object.values(Command).find((value) => value === name);
  if (name === Flag.help) return { command: Command.help, args: [], help: true };
  if (command === undefined) throw new Error(`Unknown evals command ${JSON.stringify(name)}. Run npm run evals -- help.`);
  return { command, args: args.filter((arg) => arg !== Flag.help), help: command === Command.help || args.includes(Flag.help) };
}

export function option(args: readonly string[], flag: string): string {
  const position = args.indexOf(flag);
  const value = args[position + 1];
  if (position < 0 || value === undefined || value.startsWith("--") || args.lastIndexOf(flag) !== position) throw new Error(`Give one value for ${flag}.`);
  return value;
}

export function commandHelp(command?: string): string {
  const found = Object.values(Command).find((value) => value === command);
  if (command !== undefined && found === undefined) throw new Error("Unknown help command.");
  return found === undefined ? [...Object.entries(CommandHelp).map(([name, text]) => `${name} - ${text}`), `Machine flags for any command - ${MachineHelp}`].join("\n") : CommandHelp[found];
}


export interface FlagDoc { readonly flag: string; readonly commands: readonly Command[] | "any"; readonly effect: string }
/** What each flag does and which commands take it. The CLI reference page is generated from this and CommandHelp. */
export const FlagHelp: readonly FlagDoc[] = [
  { flag: `${Flag.help}`, commands: "any", effect: "Show the command's help and example without running it" },
  { flag: `${Flag.plan} <file>`, commands: [Command.run, Command.qualify, Command.validateCase, Command.chainCheck], effect: "The saved plan to run or check" },
  { flag: `${Flag.implementers} <preset>`, commands: [Command.run], effect: `Replace the plan's implementers with ${Object.keys(ImplementerPresets).join(", ")}` },
  { flag: `${Flag.judges} <preset>`, commands: [Command.run, Command.judge, Command.reference], effect: `Replace the judges with ${Object.keys(JudgePresets).join(", ")}` },
  { flag: `${Flag.user} <preset>`, commands: [Command.run], effect: `Replace the simulated user with ${Object.keys(UserPresets).join(", ")}` },
  { flag: `${Flag.case} <id>`, commands: [Command.run, Command.validateCase, Command.chainCheck], effect: "Run or validate one Layer 4 case, so each case can run as its own process, or name the task chain-check plants next in a task-by-task packet" },
  { flag: `${Flag.setups} <ids>`, commands: [Command.run, Command.judge], effect: "Run or judge only these setups, comma-separated. For a run they keep the plan's order, and the plan's first setup is the control and must stay" },
  { flag: `${Flag.parent} <batch>`, commands: [Command.run, Command.chainCheck], effect: "Name the previous task's batch. Each run starts from the parent run with the same setup and repeat" },
  { flag: Flag.all, commands: [Command.validateCase], effect: "Validate every case of the plan" },
  { flag: `${Flag.layer} <n>`, commands: [Command.reference], effect: "Grade the reference against Layer 2 or 3" },
  { flag: `${Flag.parallel} <n>`, commands: [Command.run, Command.resume, Command.judge], effect: "How many runs go at once. The full plans run two, because every run spends the signed-in account's own model limits" },
  { flag: Flag.acceptHarness, commands: [Command.resume], effect: "Continue a batch on changed runner code and list the change in the batch record. Changed inputs, checks, rubric, tooling or models still need a new batch" },
  { flag: Flag.development, commands: [Command.run, Command.qualify], effect: "Allow local isolation, which is never fit for scoring" },
  { flag: Flag.rebuild, commands: [Command.check], effect: "Build the saved project again before checking it" },
  { flag: Flag.missing, commands: [Command.judge, Command.check, Command.analyze], effect: "Grade or analyze only runs without a usable result. For judge, that's a run without a judgment from that judge or one that ended in an error. For check, it's a run whose latest check graded nothing or ended a clause in an error. For analyze, it's a run without an analysis from the current analyzer or one that ended in an error" },
  { flag: `${Flag.batch} <id>`, commands: [Command.summary], effect: "The batch to summarize" },
  { flag: Flag.tag, commands: [Command.images], effect: "Print each named image's local name instead of building it, so a pulled image can be tagged for the checks" },
  { flag: `${Flag.ref} <commit>`, commands: [Command.results], effect: "The Open Forge commit whose graders results grade runs, HEAD by default. It must be pushed, since the workflow checks it out from GitHub" },
  { flag: Flag.imported, commands: [Command.results], effect: "Republish a batch that results import brought in. It was scrubbed when first published, so this machine's user name is neither scrubbed nor treated as a leak" },
  { flag: `${Flag.repository} <folder>`, commands: [Command.results, Command.site], effect: "The results repository to add to, src/evals/results by default, or a folder in its layout for site to show" },
  { flag: `${Flag.port} <n>`, commands: [Command.site], effect: "Serve the docs site on another port than 4320. Its data server takes the next port" },
  { flag: Flag.noBrowser, commands: [Command.site], effect: "Start the docs site without opening a browser" },
  { flag: `${MachineFlag.scratch} <folder>`, commands: "any", effect: "Put run workspaces on a roomier drive, outside any Git repository" },
  { flag: `${MachineFlag.checkSlots} <n>`, commands: "any", effect: "How many check stages run at once across every eval process on this machine. 1 by default, because browser checks are timed" },
];
