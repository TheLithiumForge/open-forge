import { readFile } from "node:fs/promises";
import { basename, join } from "node:path";
import { fileURLToPath } from "node:url";
import { applyEdits, materializeFixture, type Fixture } from "../fixtures.ts";
import { object, SpecVersion } from "./requirements.ts";
import { loadDefinition } from "./load.ts";
import { LayerId } from "./requirements.ts";

export const SpecLayerFixtures = new URL("../../fixtures/spec-layers/", import.meta.url);
export const SpecLayerReference = new URL("../../reference/", import.meta.url);
export async function loadSpecLayerFixtures(): Promise<Fixture[]> {
  const manifest: unknown = JSON.parse(await readFile(new URL("manifest.json", SpecLayerFixtures), "utf8"));
  if (!object(manifest) || manifest["specVersion"] !== SpecVersion || !Array.isArray(manifest["fixtures"])) throw new Error("Invalid spec-layer fixture manifest.");
  const definition = await loadDefinition(LayerId.three), clauses = new Set(definition.checks.flatMap((check) => check.clauses));
  const fixtures: Fixture[] = [];
  for (const filename of manifest["fixtures"]) {
    if (typeof filename !== "string" || basename(filename) !== filename || !filename.endsWith(".json")) throw new Error("Invalid fixture filename.");
    const value: unknown = JSON.parse(await readFile(new URL(filename, SpecLayerFixtures), "utf8"));
    if (!object(value) || typeof value["id"] !== "string" || !/^[a-z][a-z-]*$/.test(value["id"]) || !Array.isArray(value["edits"]) || !object(value["expected"])) throw new Error("Invalid fixture record.");
    const edits = value["edits"].map((edit: unknown) => {
      if (!object(edit) || typeof edit["path"] !== "string" || !/^src\/(?:[\w.-]+\/)*[\w.-]+\.tsx?$/.test(edit["path"]) || edit["path"].split("/").includes("..") || typeof edit["before"] !== "string" || typeof edit["after"] !== "string" || edit["before"] === edit["after"]) throw new Error("Invalid fixture edit.");
      return { path: edit["path"], before: edit["before"], after: edit["after"] };
    });
    const expected: Record<string, "pass" | "fail"> = {};
    for (const [id, status] of Object.entries(value["expected"])) {
      if (!clauses.has(id) || status !== "pass" && status !== "fail") throw new Error("Invalid fixture expectation.");
      expected[id] = status;
    }
    if (edits.length > 0 && !Object.values(expected).includes("fail")) throw new Error("A mutation needs a failed assertion expectation.");
    fixtures.push({ id: value["id"], edits, expected });
  }
  if (new Set(fixtures.map((fixture) => fixture.id)).size !== fixtures.length || !fixtures.some((fixture) => fixture.id === "reference") || fixtures.length < 7) throw new Error("Expected the reference and at least six distinct mutations.");
  return fixtures;
}
export async function auditFixtures(reference = fileURLToPath(SpecLayerReference)): Promise<string[]> {
  const audited: string[] = [];
  for (const fixture of await loadSpecLayerFixtures()) {
    for (const edit of fixture.edits) applyEdits((await readFile(join(reference, edit.path), "utf8")).replace(/\r\n/g, "\n"), [edit]);
    audited.push(fixture.id);
  }
  return audited;
}
export { materializeFixture };
