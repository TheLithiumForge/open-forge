import { mkdir, mkdtemp, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join, resolve } from "node:path";
import type { TestContext } from "node:test";
import { addMetadata } from "./frontmatter.ts";
import { contained, jsonBytes } from "./files.ts";
import { FrameworkVersion } from "./forge.ts";
import { FullSpecification, ProjectionVersion } from "./projection.ts";
import { SpecsFiles } from "./specs.ts";

export const SampleFiles = { rules: "rules.md" } as const;

export async function sample(t: TestContext) {
  const root = await mkdtemp(join(tmpdir(), "eval-materialize-"));
  t.after(async () => {
    if (!contained(tmpdir(), resolve(root))) throw new Error("Cleanup escaped its temporary folder.");
    await rm(root, { recursive: true, force: true });
  });
  const source = join(root, "source");
  const destination = join(root, "specs");
  const projection = join(root, "projection.json");
  await mkdir(source);
  const documents = {
    [SpecsFiles.overview]: { description: "Game overview", tags: ["LoadNow", "Game"] },
    [SampleFiles.rules]: { description: "Matching and moves", tags: ["LoadNow", "Rules"] },
  };
  await writeFile(projection, jsonBytes({ schemaVersion: ProjectionVersion, fixture: "sample", level: FullSpecification, frameworkVersion: FrameworkVersion, documents }));
  await writeFile(join(source, SpecsFiles.overview), addMetadata(Buffer.from(`# Game\nFind every pair. Read [rules](${SampleFiles.rules}).\n`), documents[SpecsFiles.overview]));
  await writeFile(join(source, SampleFiles.rules), addMetadata(Buffer.from(`# Rules\r\nMatch two cards. See [Game](${SpecsFiles.overview}).\r\n`), documents[SampleFiles.rules]));
  await writeFile(join(source, SpecsFiles.entrypoint), addMetadata(Buffer.from("# Pack\nEditable game pack.\n"), documents[SpecsFiles.overview]));
  return { root, source, destination, projection };
}
