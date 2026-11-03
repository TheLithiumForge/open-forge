/** The file operands of the commands agents use to read and search, without their options and patterns. */

// Options that take the next word as their value, per command.
const ValueOptions: Readonly<Record<string, ReadonlySet<string>>> = {
  head: new Set(["-n", "-c", "--lines", "--bytes"]),
  tail: new Set(["-n", "-c", "--lines", "--bytes"]),
  sed: new Set(["-e", "-f", "-l", "--expression", "--file"]),
  awk: new Set(["-F", "-v", "-f"]),
  jq: new Set(["--arg", "--argjson", "--slurpfile", "--rawfile", "--indent"]),
  rg: new Set(["-e", "-f", "-g", "-t", "-T", "-A", "-B", "-C", "-m", "-M", "-j", "-r", "-E", "-d", "--glob", "--iglob", "--type", "--type-not",
    "--max-count", "--max-columns", "--threads", "--sort", "--sortr", "--replace", "--encoding", "--max-depth", "--context", "--after-context",
    "--before-context", "--regexp", "--file", "--type-add", "--pre", "--max-filesize"]),
  grep: new Set(["-e", "-f", "-A", "-B", "-C", "-m", "--regexp", "--file", "--context", "--after-context", "--before-context", "--max-count",
    "--include", "--exclude", "--exclude-dir", "--label"]),
};
// Options whose presence means the pattern or program came as an option value, not as the first operand.
const PatternOptions: Readonly<Record<string, ReadonlySet<string>>> = {
  sed: new Set(["-e", "-f", "--expression", "--file"]),
  awk: new Set(["-f"]),
  rg: new Set(["-e", "-f", "--regexp", "--file", "--files", "--type-list"]),
  grep: new Set(["-e", "-f", "--regexp", "--file"]),
};
/** Commands whose first operand is a pattern, script or filter rather than a file. */
const FirstOperandIsPattern = new Set(["sed", "awk", "jq", "rg", "grep", "egrep", "fgrep"]);
const Aliases: Readonly<Record<string, string>> = { egrep: "grep", fgrep: "grep" };

/** A command's operands: its words after the name, without options, option values and, where one comes first, the pattern. */
export function fileOperands(name: string, words: readonly string[]): string[] {
  const base = Aliases[name] ?? name;
  const valued = ValueOptions[base] ?? new Set<string>();
  const patternGiven = words.some((word) => PatternOptions[base]?.has(word.split("=")[0] ?? word) === true);
  const operands: string[] = [];
  let skip = false, optionsEnded = false;
  for (const word of words) {
    if (skip) { skip = false; continue; }
    if (!optionsEnded && word === "--") { optionsEnded = true; continue; }
    if (!optionsEnded && word.startsWith("-") && word.length > 1) {
      if (valued.has(word)) skip = true;
      continue;
    }
    operands.push(word);
  }
  return FirstOperandIsPattern.has(name) && !patternGiven ? operands.slice(1) : operands;
}
