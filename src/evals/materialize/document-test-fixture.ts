import { documentMetadata } from "./frontmatter.ts";
import { inventory, jsonBytes, writeNew } from "./files.ts";
import { ForgeFlags, FrameworkVersion, type ForgeRunner } from "./forge.ts";
import { SetupFiles } from "./setup.ts";
import { QualificationFiles } from "../config/qualify.ts";
import { join } from "node:path";

export const DocumentFixtureFiles = { catalogue: "docs/_docs.md", foreign: "foreign-setup.md" } as const;

export function documentInstaller(calls: string[][] = []): ForgeRunner {
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
    } else if (args[0] === "index") {
      const docs = await inventory(join(cwd, SetupFiles.docs));
      files[DocumentFixtureFiles.catalogue] = `# Documents\n\n## Entries\n\n${docs.map((file) => `- [${documentMetadata(file.bytes).description}](${file.path})`).join("\n")}\n`;
    }
    if (!args.includes(ForgeFlags.dryRun)) for (const [path, bytes] of Object.entries(files)) await writeNew(cwd, path, bytes);
    return { code: 0, stdout: jsonBytes({ status: "completed", effects: Object.keys(files).map((path) => ({ path, kind: "file", action: "created" })) }).toString(), stderr: "" };
  };
}

