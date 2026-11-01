export const Command = { doctor: "doctor", images: "images", smoke: "smoke", qualify: "qualify", run: "run", ui: "ui", recheck: "recheck", rejudge: "rejudge", export: "export", help: "help" } as const;
export type Command = (typeof Command)[keyof typeof Command];
export const Flag = { help: "--help", plan: "--plan", development: "--development", rebuild: "--rebuild", out: "--out" } as const;
export const CommandHelp = {
  doctor: "Check Docker, Node, pinned tools, sign-ins and image digests. Example: npm run evals -- doctor",
  images: "Build the three images from locked digests. Example: npm run evals -- images",
  smoke: "Run a mock attempt through all stages and saved records, without Docker or models. Example: npm run evals -- smoke",
  qualify: "Check fresh setup files and the pinned Open Forge version without models. Example: npm run evals -- qualify --plan src/evals/experiments/neon-pairs-l1.json",
  run: "Run a saved plan. Local isolation needs --development. Example: npm run evals -- run --plan src/evals/experiments/neon-pairs-l1.json",
  ui: "Build and open the local results app. Example: npm run evals -- ui",
  recheck: "Append checks for saved attempts. Example: npm run evals -- recheck <batch> --rebuild",
  rejudge: "Append judge assessments for saved attempts. Example: npm run evals -- rejudge <batch>",
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
