import { cp, mkdir, readFile, writeFile } from "node:fs/promises";
import { basename, join, resolve, sep } from "node:path";
import { CheckStatus } from "../../../history/statuses.ts";
import { LayerVersion, loadLayer, record } from "./layer.ts";

export const FixtureManifest = new URL("../fixtures/manifest.json", import.meta.url);
export const ReferenceFolder = new URL("../cases/greenfield-baseline/reference/", import.meta.url);
export const RequiredFixtureIds = { reference: "reference", faces: "faces-repeat-32", best: "best-overwritten", moves: "new-game-keeps-moves", keyboard: "keyboard-unreachable" } as const;
interface Edit { readonly path: string; readonly before: string; readonly after: string }
export interface Fixture { readonly id: string; readonly edits: readonly Edit[]; readonly expected: Readonly<Record<string, CheckStatus>> }
const ExcludedFolders = new Set(["node_modules", "dist", "test-results", ".git", ".agents"]);

function relativePath(path: string): string {
  if (!path.startsWith("src/") || path.includes("\\") || path.includes(":") || path.split("/").some((part) => !part || part === ".." || part === ".")) throw new Error("Fixture edit needs a source path.");
  return path;
}

export function applyEdits(text: string, edits: readonly Pick<Edit, "before" | "after">[]): string {
  return edits.reduce((value, edit) => {
    if (!edit.before || value.split(edit.before).length !== 2) throw new Error("Fixture edit no longer matches the reference exactly once.");
    return value.replace(edit.before, edit.after);
  }, text);
}

export async function loadFixtures(): Promise<Fixture[]> {
  const manifest: unknown = JSON.parse(await readFile(FixtureManifest, "utf8"));
  if (!record(manifest) || manifest["gradingVersion"] !== LayerVersion || !Array.isArray(manifest["fixtures"])) throw new Error("Invalid fixture manifest.");
  const layer = await loadLayer();
  const fixtures: Fixture[] = [];
  for (const name of manifest["fixtures"]) {
    if (typeof name !== "string" || basename(name) !== name || !name.endsWith(".json")) throw new Error("Invalid fixture filename.");
    const value: unknown = JSON.parse(await readFile(new URL(`../fixtures/${name}`, import.meta.url), "utf8"));
    if (!record(value) || typeof value["id"] !== "string" || value["gradingVersion"] !== LayerVersion || !Array.isArray(value["edits"]) || !record(value["expected"])) throw new Error("Invalid fixture definition.");
    const edits = value["edits"].map((edit: unknown): Edit => {
      if (!record(edit) || typeof edit["path"] !== "string" || typeof edit["before"] !== "string" || typeof edit["after"] !== "string") throw new Error("Invalid fixture edit.");
      return { path: relativePath(edit["path"]), before: edit["before"], after: edit["after"] };
    });
    const expected: Record<string, CheckStatus> = {};
    for (const check of layer.checks) {
      const status = value["expected"][check.id];
      if (status !== CheckStatus.pass && status !== CheckStatus.fail) throw new Error("Every fixture check needs its expected pass or fail result.");
      expected[check.id] = status;
    }
    if (Object.keys(value["expected"]).length !== layer.checks.length) throw new Error("Fixture expected an undeclared check.");
    fixtures.push({ id: value["id"], edits, expected });
  }
  if (new Set(fixtures.map((fixture) => fixture.id)).size !== fixtures.length) throw new Error("Duplicate fixture ID.");
  if (Object.values(RequiredFixtureIds).some((id) => !fixtures.some((fixture) => fixture.id === id))) throw new Error("The Layer 1 reference or a required mutation is missing.");
  return fixtures;
}

export async function materializeFixture(reference: string, destination: string, fixture: Fixture): Promise<void> {
  const source = resolve(reference), target = resolve(destination);
  if (target === source || target.startsWith(`${source}${sep}`) || source.startsWith(`${target}${sep}`)) throw new Error("Fixture must stay separate from its reference.");
  await mkdir(target);
  await cp(source, target, { recursive: true, filter: (path) => !ExcludedFolders.has(basename(path)) });
  for (const edit of fixture.edits) {
    const path = join(target, relativePath(edit.path));
    const text = (await readFile(path, "utf8")).replace(/\r\n/g, "\n");
    await writeFile(path, applyEdits(text, [edit]));
  }
}
