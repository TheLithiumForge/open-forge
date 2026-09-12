import assert from "node:assert/strict";
import { parseArgs } from "node:util";
import { readPublication } from "./publication.ts";
import { publishPackages } from "./publish-packages.ts";
import { reportFailure } from "../process.ts";
import { repositoryRoot } from "../repository.ts";

try {
  const { values, positionals } = parseArgs({ allowPositionals: true, options: { tag: { type: "string" }, "dry-run": { type: "boolean" } } });
  const [kind] = positionals;
  assert.ok(positionals.length === 1 && (kind === "native" || kind === "wrapper"), "Select native or wrapper.");
  assert.ok(values.tag, "Publication requires an explicit --tag.");
  const publication = readPublication(repositoryRoot, kind);
  publishPackages(repositoryRoot, [publication], values.tag, values["dry-run"] === true);
} catch (error) {
  reportFailure(error);
}
