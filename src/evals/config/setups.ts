import { mkdir, mkdtemp, rm } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import type { Suite } from "../benchmarks/suite.types.ts";
import type { Snapshot } from "../execution/snapshot.ts";
import { createForgeRunner, type ForgeRunner } from "../materialize/forge.ts";
import { inventory, safeRelative, sha256 } from "../materialize/files.ts";
import { ImageVersions } from "../execution/containers/docker-image.ts";
import { object, text } from "./validation.ts";
import { documentPath, materializeSetup, scopeEntrypoints, SetupFiles, SetupKind, type SetupManifest } from "../materialize/setup.ts";
import { loadScopeLayout } from "../materialize/scopes.ts";
import { evalsRoot } from "../paths.ts";

export const SetupCliFlags = { version: "--version", preview: "--dry-run", output: ["--format", "json", "--detail", "full"] } as const;
export const SetupFileKinds = new Set(["file", "setting", "record", "section"]);
export interface GeneratedSetup extends Snapshot { readonly expected: readonly string[]; readonly frameworkVersion: string | null; readonly materialized?: SetupManifest }

export function plannedFiles(stdout: string): string[] {
  const result = object(JSON.parse(stdout));
  if (result["status"] !== "completed" || !Array.isArray(result["effects"])) throw new Error("Open Forge did not return a completed setup plan.");
  const files = result["effects"].flatMap((value: unknown) => {
    const effect = object(value);
    return typeof effect["kind"] === "string" && SetupFileKinds.has(effect["kind"]) && effect["action"] !== "deleted" ? [safeRelative(text(effect["path"]))] : [];
  });
  return [...new Set(files)].sort();
}

export async function generateSetup(suite: Suite, setup: string, run: ForgeRunner = createForgeRunner()): Promise<GeneratedSetup> {
  const definition = suite.setups[setup];
  if (definition === undefined || !/^[\w-]+$/.test(suite.project)) throw new Error("Unknown setup or invalid project folder.");
  const root = await mkdtemp(join(tmpdir(), "eval-setup-"));
  const folder = join(root, suite.project);
  const expected = new Set<string>();
  let frameworkVersion: string | null = null;
  const output: string[] = [];
  try {
    await mkdir(folder);
    if (definition.delivery !== undefined) {
      if ((!suite.specs && !suite.brownfield) || definition.commands.length) throw new Error("Document setups need one frozen pack and no separate setup commands.");
      const planned = new Set<string>();
      const tracked: ForgeRunner = async (args, cwd) => {
        const result = await run(args, cwd);
        if (args.includes(SetupCliFlags.preview) && result.code === 0) for (const path of plannedFiles(result.stdout)) planned.add(path);
        return result;
      };
      const scopes = definition.delivery.scopes === undefined ? undefined : await loadScopeLayout(join(evalsRoot, safeRelative(definition.delivery.scopes)));
      const result = await materializeSetup({ kind: definition.delivery.kind, destination: folder, ...(suite.specs === undefined ? {} : { specs: join(evalsRoot, safeRelative(suite.specs)) }),
        ...(suite.brownfield === undefined ? {} : { frozen: suite.brownfield.specifications, projectFiles: suite.brownfield.reference, seedFiles: suite.brownfield.seed }),
        ...(definition.delivery.preset === undefined ? {} : { preset: definition.delivery.preset }), ...(scopes === undefined ? {} : { scopes }),
        ...(definition.delivery.links === false ? { links: false } : {}), runForge: tracked });
      expected.add(SetupFiles.readme);
      if (definition.delivery.kind !== SetupKind.inline && definition.delivery.kind !== SetupKind.code) {
        for (const file of result.manifest.specifications.files) expected.add(`${SetupFiles.docs}/${documentPath(result.manifest, file.path)}`);
        for (const path of scopeEntrypoints(result.manifest)) expected.add(`${SetupFiles.docs}/${path}`);
      }
      for (const file of suite.brownfield?.seed ?? []) expected.add(file.path);
      if (definition.delivery.kind === SetupKind.linked) for (const file of [SetupFiles.agents, SetupFiles.claude]) expected.add(file);
      for (const path of planned) expected.add(path);
      const files = (await inventory(folder)).map((file) => ({ path: file.path, bytes: file.bytes.length, sha256: sha256(file.bytes) }));
      return { folder, expected: [...expected].sort(), frameworkVersion: result.manifest.frameworkVersion, materialized: result.manifest,
        record: { setup, label: definition.label, commands: result.manifest.frameworkCommands.filter((command) => !command.args.includes(SetupCliFlags.preview)).map((command) => command.args), output: "", files } };
    }
    if (definition.commands.length > 0) {
      const version = await run([SetupCliFlags.version], folder);
      if (version.code !== 0 || version.stdout.trim() !== ImageVersions.openForge) throw new Error("The setup CLI differs from its pinned version.");
      frameworkVersion = version.stdout.trim();
    }
    for (const command of definition.commands) {
      const args = [...command, ...SetupCliFlags.output];
      const preview = await run([...args, SetupCliFlags.preview], folder);
      if (preview.code !== 0) throw new Error("Open Forge could not preview the setup.");
      for (const path of plannedFiles(preview.stdout)) expected.add(path);
      const result = await run(args, folder);
      output.push(`$ open-forge ${command.join(" ")}\n${result.stdout}${result.stderr}`);
      if (result.code !== 0) throw new Error("Open Forge could not install the setup.");
      plannedFiles(result.stdout);
    }
    const files = (await inventory(folder)).map((file) => ({ path: file.path, bytes: file.bytes.length, sha256: sha256(file.bytes) }));
    return { folder, expected: [...expected].sort(), frameworkVersion,
      record: { setup, label: definition.label, commands: definition.commands, output: output.join("\n"), files } };
  } catch (error) { await rm(root, { recursive: true, force: true }); throw error; }
}
