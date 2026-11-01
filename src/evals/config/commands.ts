import { ImplementerPresets, JudgePresets, UserPresets } from "./presets.ts";

export const Command = { doctor: "doctor", images: "images", smoke: "smoke", qualify: "qualify", validateCase: "validate-case", run: "run", reference: "reference", summary: "summary", ui: "ui", recheck: "recheck", rejudge: "rejudge", export: "export", help: "help" } as const;
export type Command = (typeof Command)[keyof typeof Command];
export const Flag = { help: "--help", plan: "--plan", implementers: "--implementers", judges: "--judges", user: "--user", layer: "--layer", case: "--case", all: "--all", batch: "--batch", development: "--development", rebuild: "--rebuild", out: "--out" } as const;
export const CommandHelp = {
  reference: "Grade the tracked Neon Pairs reference without an agent. Defaults to --layer 3 --judges claude. Example: npm run evals -- reference --layer 3 --judges both",
  doctor: "Check Docker, Node, pinned tools, sign-ins and image digests. Example: npm run evals -- doctor",
  images: "Build the three images from locked digests. Example: npm run evals -- images",
  smoke: "Run a mock attempt through all stages and saved records, without Docker or models. Example: npm run evals -- smoke",
  qualify: "Check fresh setup files and the pinned Open Forge version without models. Example: npm run evals -- qualify --plan src/evals/experiments/neon-pairs-l1.json",
  [Command.validateCase]: "Validate a Layer 4 starting project and solution in verifiers without models. Examples: npm run evals -- validate-case --case l4-f01 | npm run evals -- validate-case --all --plan src/evals/experiments/neon-pairs-l4-check.json",
  run: `Run a saved plan. --implementers: ${Object.keys(ImplementerPresets).join(", ")}. --judges: ${Object.keys(JudgePresets).join(", ")}. --user: ${Object.keys(UserPresets).join(", ")}. Each replaces that role in the plan. Local isolation needs --development. Example: npm run evals -- run --plan src/evals/experiments/neon-pairs-l1-check.json --implementers both --judges both --user claude`,
  summary: "Print reference history, or implementer and judge pairs for --batch. Example: npm run evals -- summary --batch <id>",
  ui: "Build and open the local results app. Example: npm run evals -- ui",
  recheck: "Append checks for saved attempts. Example: npm run evals -- recheck <batch> --rebuild",
  rejudge: `Append judge assessments for saved attempts. --judges: ${Object.keys(JudgePresets).join(", ")}. Defaults to the batch's saved judges. Example: npm run evals -- rejudge <batch> [attempt] --judges both`,
  export: "Export sanitized finished records to a new folder. Example: npm run evals -- export <batch> --out ./eval-export",
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
  return found === undefined ? Object.entries(CommandHelp).map(([name, text]) => `${name} - ${text}`).join("\n") : CommandHelp[found];
}

