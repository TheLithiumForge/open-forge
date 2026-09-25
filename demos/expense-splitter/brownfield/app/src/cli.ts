import { parseArgs } from "node:util";
import { add, list, showBalances } from "./commands.ts";

const DEFAULT_FILE = "expenses.json";
const FILE_VARIABLE = "EXPENSES_FILE";
const USAGE = `Usage:
  node src/cli.ts add <description> <amount> --paid-by <name> --with <name,name>
  node src/cli.ts list
  node src/cli.ts balances

Expenses are stored in ${DEFAULT_FILE}, or in the file named by ${FILE_VARIABLE}.`;

function run(argv: readonly string[]): string {
  const { positionals, values } = parseArgs({
    args: [...argv],
    allowPositionals: true,
    options: { "paid-by": { type: "string" }, with: { type: "string" } },
  });
  const [command, ...rest] = positionals;
  const file = process.env[FILE_VARIABLE] ?? DEFAULT_FILE;

  switch (command) {
    case "add": {
      const [description, amount] = rest;
      const paidBy = values["paid-by"];
      if (!description || !amount || !paidBy) {
        throw new Error(USAGE);
      }
      const others = (values.with ?? "")
        .split(",")
        .map((name) => name.trim())
        .filter(Boolean);
      return add(file, { description, amount, paidBy, with: others });
    }
    case "list":
      return list(file);
    case "balances":
      return showBalances(file);
    default:
      return USAGE;
  }
}

try {
  console.log(run(process.argv.slice(2)));
} catch (error) {
  console.error(error instanceof Error ? error.message : String(error));
  process.exitCode = 1;
}
