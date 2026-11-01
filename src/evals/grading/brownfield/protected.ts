import { join, relative } from "node:path";
import { contained, inventory, sha256 } from "../../materialize/files.ts";
import type { BrownfieldCase } from "../../materialize/brownfield-case.ts";
import type { CheckOutcome } from "../grading.types.ts";
import { CheckStatus } from "../../history/statuses.ts";
import { BrownfieldGuard, NeverSource, TransientRootFolders } from "./scope.ts";

export async function protectedHashes(roots: readonly string[], project: string): Promise<readonly string[]> {
  const hashes: string[] = [];
  for (const [index, root] of roots.entries()) {
    if (contained(project, root) || contained(root, project)) throw new Error("Grader inputs must stay outside the submitted project.");
    for (const file of await inventory(root, NeverSource, TransientRootFolders)) hashes.push(`${index}/${file.path}:${sha256(file.bytes)}`);
  }
  return hashes;
}

export async function protectedOutcome(item: BrownfieldCase, project: string, before: readonly string[], after: readonly string[]): Promise<CheckOutcome> {
  const privateFiles = item.inputs.filter((file) => file.path !== "request.md" && !file.path.startsWith(`${item.specDelta}/`));
  const privateHashes = new Set(privateFiles.map((file) => file.sha256));
  const leaked: string[] = [];
  for (const file of await inventory(project, NeverSource, TransientRootFolders)) {
    const reserved = /^(?:checks\/|case\.json$|seed\.patch$|solution\.patch$|requirements\.json$|trace\.json$)/.test(file.path);
    if (reserved || file.bytes.length > 16 && privateHashes.has(sha256(file.bytes))) leaked.push(relative(project, join(project, file.path)).replaceAll("\\", "/"));
  }
  const changed = JSON.stringify(before) !== JSON.stringify(after);
  return { id: BrownfieldGuard.protected, area: "protected", status: changed || leaked.length ? CheckStatus.fail : CheckStatus.pass,
    evidence: [changed ? "Grader inputs changed during verification." : "Grader input hashes stayed unchanged.",
      leaked.length ? `Private grader inputs found in the project: ${leaked.join(", ")}.` : "The project contains no private grader inputs."].join("\n") };
}
