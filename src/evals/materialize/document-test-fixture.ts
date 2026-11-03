import { documentMetadata } from "./frontmatter.ts";
import { inventory, jsonBytes, writeNew } from "./files.ts";
import { ForgeFlags, FrameworkVersion, type ForgeRunner } from "./forge.ts";
import { SetupFiles } from "./setup.ts";
import { QualificationFiles } from "../config/qualify.ts";
import { join } from "node:path";
import { readFile, writeFile } from "node:fs/promises";

export const DocumentFixtureFiles = { catalogue: "docs/_docs.md", foreign: "foreign-setup.md" } as const;
export const FixtureIgnoredContext = "# Fixture CLI context ignore rules\n";

export function documentInstaller(calls: string[][] = [], ignoreContext = false): ForgeRunner {
  return async (args, cwd) => {
    calls.push([...args]);
    if (args[0] === ForgeFlags.version) return { code: 0, stdout: FrameworkVersion, stderr: "" };
    const files: Record<string, string> = {};
    if (args[0] === "install") {
      const preset = args[args.indexOf("--preset") + 1];
      const marker = `.agents/${preset}.md`;
      files[QualificationFiles.agents] = `Read ${QualificationFiles.loader}.\n`;
      files[QualificationFiles.loader] = "# Loader\nRead the product documents.\n";
      files[marker] = `# ${preset}\n`;
      files[QualificationFiles.lock] = JSON.stringify({ framework: { source: { version: FrameworkVersion }, paths: [QualificationFiles.loader, marker] }, extensions: [], libraries: [] });
      if (ignoreContext) files[".gitignore"] = `${await readFile(join(cwd, ".gitignore"), "utf8")}${FixtureIgnoredContext}`;
    } else if (args[0] === "extension" && args[1] === "install") {
      // The fake records each requested Extension in the lock, as a real install does, with no files of its own.
      const flags = args.findIndex((arg, index) => index > 1 && arg.startsWith("--"));
      const ids = args.slice(2, flags < 0 ? undefined : flags);
      const lock = JSON.parse(await readFile(join(cwd, QualificationFiles.lock), "utf8")) as { extensions: unknown[] };
      if (!args.includes(ForgeFlags.dryRun)) await writeFile(join(cwd, QualificationFiles.lock), JSON.stringify({ ...lock, extensions: ids.map((id) => ({ id, dependencies: [], paths: [] })) }));
      return { code: 0, stdout: jsonBytes({ status: "completed", effects: [] }).toString(), stderr: "" };
    } else if (args[0] === "index" && args[1] !== undefined && !args[1].startsWith("--")) {
      const docs = await inventory(join(cwd, SetupFiles.docs));
      files[DocumentFixtureFiles.catalogue] = `# Documents\n\n## Entries\n\n${docs.map((file) => `- [${documentMetadata(file.bytes).description}](${file.path})`).join("\n")}\n`;
    }
    if (!args.includes(ForgeFlags.dryRun)) for (const [path, bytes] of Object.entries(files)) {
      if (ignoreContext && path === ".gitignore") await writeFile(join(cwd, path), bytes);
      else await writeNew(cwd, path, bytes);
    }
    return { code: 0, stdout: jsonBytes({ status: "completed", effects: Object.keys(files).map((path) => ({ path, kind: "file", action: "created" })) }).toString(), stderr: "" };
  };
}

