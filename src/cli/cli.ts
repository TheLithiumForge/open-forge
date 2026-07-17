import { constants as fsConstants, existsSync, type Dirent } from "node:fs";
import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const repoRoot = resolveRepoRoot();
const sourceRoot = path.join(repoRoot, "src", "open-forge");
const bundledExtensionsRoot = process.env.OPEN_FORGE_EXTENSIONS_ROOT
  ? path.resolve(process.env.OPEN_FORGE_EXTENSIONS_ROOT)
  : path.join(repoRoot, "src", "extensions");
const ignoredDirectoryNames = new Set([".git", ".obsidian", "node_modules"]);
const compatibilityEntrypointNames = ["_index.md", "index.md", "_references.md", "references.md"];
const skillEntrypointNames = ["SKILL.md", "Skill.md"];
const portablePathSeparator = "/";
const agentsDirectoryName = ".agents";
const skillsDirectoryName = "skills";
const scopedCoreEntrypointFolders = new Set(["directives", "guidance", "patterns", "skills"]);
const entriesHeading = "## Entries";
const generatedIndexStartMarker = "<!-- open-forge:generated-index:start -->";
const generatedIndexEndMarker = "<!-- open-forge:generated-index:end -->";
const retiredLoadPolicyTags = new Set(["openforge", "loadwithparententrypoint", "loadforpostworkreview"]);
const categoryTypeTags: Record<string, string> = {
  directives: "Directive",
  guidance: "Guidance",
  patterns: "Pattern",
  skills: "Skill",
  workflows: "Workflow",
  workspace: "Workspace",
  memory: "Memory"
};
const args = process.argv.slice(2);
const command = args[0] ?? "help";

try {
  if (command === "install") {
    await install(args[1] ?? process.cwd());
  } else if (command === "extend") {
    await extend(args.slice(1));
  } else if (command === "index") {
    await generateIndexes(path.resolve(args[1] ?? process.cwd()));
  } else if (command === "find") {
    await find(args.slice(1));
  } else if (command === "doctor") {
    await doctor(args.slice(1));
  } else if (command === "create") {
    await create(args.slice(1));
  } else {
    printHelp();
  }
} catch (error) {
  console.error(`open-forge: ${error instanceof Error ? error.message : String(error)}`);
  process.exitCode = 1;
}

async function updateAgents(targetArg: string): Promise<void> {
  const targetRoot = path.resolve(targetArg);
  const sourceFile = path.join(sourceRoot, "AGENTS.md");
  const targetFile = path.join(targetRoot, "AGENTS.md");

  await ensureDir(targetRoot);

  const sourceText = await fs.readFile(sourceFile, "utf8");
  const targetText = await readTextIfExists(targetFile);
  const nextText = targetText == null ? sourceText : patchMarkedBlock(targetText, sourceText, "open-forge");

  await fs.writeFile(targetFile, nextText);
}

async function install(targetArg: string): Promise<void> {
  const targetRoot = path.resolve(targetArg);
  await ensureDir(targetRoot);

  const files = await listFiles(sourceRoot);
  const frameworkTemplates = files.flatMap((file) => createFrameworkEntrypointTemplate(sourceRoot, file));
  let copied = 0;
  let patched = 0;

  for (const sourceFile of files) {
    const relativePath = toPosix(path.relative(sourceRoot, sourceFile));
    const targetFile = path.join(targetRoot, relativePath);
    await ensureDir(path.dirname(targetFile));

    if (relativePath === "AGENTS.md") {
      await updateAgents(targetRoot);
      patched += 1;
      continue;
    }

    const sourceText = await fs.readFile(sourceFile, "utf8");
    const targetText = await readTextIfExists(targetFile);
    const nextText = targetText == null ? sourceText : preserveLocalBlocks(sourceText, targetText);
    await fs.writeFile(targetFile, nextText);
    copied += 1;
  }

  const scopedCopied = await updateScopedFrameworkEntrypoints(targetRoot, frameworkTemplates);
  const generatedRegions = await generateIndexes(targetRoot);
  console.log(`Installed Open Forge into ${targetRoot}`);
  console.log(`Updated ${copied} managed files, updated ${scopedCopied} scoped framework route files, patched ${patched} entry files, rebuilt ${generatedRegions} generated regions.`);
}

async function extend(extendArgs: string[]): Promise<void> {
  if (extendArgs[0] === "--list") {
    assertNoExtraArgs(extendArgs, 1, "Usage: open-forge extend --list");
    await listBundledExtensions();
    return;
  }

  if (extendArgs[0] === "--select" || extendArgs.length === 0) {
    const target = extendArgs[0] === "--select" ? extendArgs[1] ?? process.cwd() : process.cwd();
    assertNoExtraArgs(extendArgs, extendArgs[0] === "--select" ? 2 : 0, "Usage: open-forge extend --select [target]");
    const ids = await selectBundledExtensionIds();
    if (ids.length === 0) {
      console.log("No extensions selected.");
      return;
    }

    await installExtensions(ids, target);
    return;
  }

  const idsValue = readIdsValue(extendArgs);
  if (idsValue) {
    const { ids, consumed } = idsValue;
    const target = extendArgs[consumed] ?? process.cwd();
    assertNoExtraArgs(extendArgs, consumed + (extendArgs[consumed] ? 1 : 0), "Usage: open-forge extend --ids <id[,id...]> [target]");
    await installExtensions(ids, target);
    return;
  }

  const extensionArg = extendArgs[0];
  const targetArg = extendArgs[1] ?? process.cwd();
  assertNoExtraArgs(extendArgs, 2, "Usage: open-forge extend <extension-source-or-id> [target]");
  await installExtensions([extensionArg], targetArg);
}

function assertNoExtraArgs(args: string[], allowedCount: number, usage: string): void {
  if (args.length > allowedCount) {
    throw new Error(usage);
  }
}

function readIdsValue(args: string[]): { ids: string[]; consumed: number } | null {
  const first = args[0];
  if (first === "--ids") {
    const value = args[1];
    if (!value) {
      throw new Error("Usage: open-forge extend --ids <id[,id...]> [target]");
    }

    return { ids: splitExtensionIds(value), consumed: 2 };
  }

  if (first.startsWith("--ids=")) {
    return { ids: splitExtensionIds(first.slice("--ids=".length)), consumed: 1 };
  }

  return null;
}

function splitExtensionIds(value: string): string[] {
  const ids = [...new Set(value.split(",").map((id) => id.trim()).filter(Boolean))];
  if (ids.length === 0 || ids.some((id) => !isBundledExtensionId(id))) {
    throw new Error("Extension ids must be comma-separated lowercase ids such as vision-workflow,implementation-workflow");
  }

  return ids;
}

async function installExtensions(extensionArgs: string[], targetArg: string): Promise<void> {
  const extensions = await Promise.all(extensionArgs.map(resolveExtensionSource));
  const targetRoot = path.resolve(targetArg);

  for (const extension of extensions) {
    if (samePath(extension.root, targetRoot)) {
      throw new Error("Extension source and target must be different directories");
    }
  }

  await ensureDir(targetRoot);

  let copied = 0;
  for (const extension of extensions) {
    copied += await copyExtension(extension, targetRoot);
  }

  const generatedRegions = await generateIndexes(targetRoot);
  const labels = extensions.map((extension) => `${extension.kind}:${extension.label}`).join(", ");
  console.log(`Installed Open Forge extensions ${labels} into ${targetRoot}`);
  console.log(`Copied ${copied} extension files and rebuilt ${generatedRegions} generated regions.`);
}

async function copyExtension(extension: ExtensionSource, targetRoot: string): Promise<number> {
  const files = await listFiles(extension.root);
  let copied = 0;

  for (const sourceFile of files) {
    const relativePath = toPosix(path.relative(extension.root, sourceFile));
    const targetFile = path.join(targetRoot, relativePath);
    await ensureDir(path.dirname(targetFile));

    if (isMarkdownFile(sourceFile)) {
      const sourceText = await fs.readFile(sourceFile, "utf8");
      const targetText = await readTextIfExists(targetFile);
      const nextText = targetText == null ? sourceText : preserveLocalBlocks(sourceText, targetText);
      await fs.writeFile(targetFile, nextText);
    } else {
      await fs.copyFile(sourceFile, targetFile);
    }

    copied += 1;
  }

  return copied;
}

type ExtensionSource = {
  root: string;
  label: string;
  kind: "local" | "bundled";
};

type BundledExtensionInfo = {
  id: string;
  name: string;
  description: string;
};

async function resolveExtensionSource(value: string): Promise<ExtensionSource> {
  const localRoot = path.resolve(value);
  if (await isDirectory(localRoot)) {
    const localPayloadRoot = path.join(localRoot, "payload");
    return {
      root: await isDirectory(localPayloadRoot) ? localPayloadRoot : localRoot,
      label: localRoot,
      kind: "local"
    };
  }

  if (!isBundledExtensionId(value)) {
    throw new Error(`Extension source does not exist or is not a directory: ${localRoot}`);
  }

  const bundledRoot = path.join(bundledExtensionsRoot, value, "payload");
  if (await isDirectory(bundledRoot)) {
    return { root: bundledRoot, label: value, kind: "bundled" };
  }

  const available = await listBundledExtensionIds();
  const suffix = available.length > 0 ? ` Available bundled extensions: ${available.join(", ")}.` : " No bundled extensions are installed in this CLI package.";
  throw new Error(`Unknown bundled Open Forge extension: ${value}.${suffix}`);
}

function isBundledExtensionId(value: string): boolean {
  return /^[a-z0-9][a-z0-9-]*$/.test(value);
}

async function listBundledExtensions(): Promise<void> {
  const extensions = await listBundledExtensionInfos();

  if (extensions.length === 0) {
    console.log("No bundled Open Forge extensions are installed in this CLI package.");
    return;
  }

  console.log("Bundled Open Forge extensions:");
  for (const extension of extensions) {
    const description = extension.description ? ` - ${extension.description}` : "";
    console.log(`- ${extension.id}${description}`);
  }
}

async function listBundledExtensionIds(): Promise<string[]> {
  return (await listBundledExtensionInfos()).map((extension) => extension.id);
}

async function listBundledExtensionInfos(): Promise<BundledExtensionInfo[]> {
  if (!(await isDirectory(bundledExtensionsRoot))) {
    return [];
  }

  const entries = await fs.readdir(bundledExtensionsRoot, { withFileTypes: true });
  const extensions: BundledExtensionInfo[] = [];

  for (const entry of entries) {
    if (!entry.isDirectory() || !isBundledExtensionId(entry.name)) {
      continue;
    }

    if (await isDirectory(path.join(bundledExtensionsRoot, entry.name, "payload"))) {
      extensions.push(await readBundledExtensionInfo(entry.name));
    }
  }

  return extensions.sort((left, right) => left.id.localeCompare(right.id));
}

async function readBundledExtensionInfo(id: string): Promise<BundledExtensionInfo> {
  const metadataFile = path.join(bundledExtensionsRoot, id, "extension.json");
  const fallback = { id, name: id, description: "" };
  const text = await readTextIfExists(metadataFile);
  if (!text) {
    return fallback;
  }

  const metadata = JSON.parse(text) as Partial<BundledExtensionInfo>;
  return {
    id,
    name: typeof metadata.name === "string" && metadata.name.trim() ? metadata.name.trim() : fallback.name,
    description: typeof metadata.description === "string" ? metadata.description.trim() : fallback.description
  };
}

async function selectBundledExtensionIds(): Promise<string[]> {
  const extensions = await listBundledExtensionInfos();
  if (extensions.length === 0) {
    console.log("No bundled Open Forge extensions are installed in this CLI package.");
    return [];
  }

  if (!process.stdin.isTTY || !process.stdout.isTTY || typeof process.stdin.setRawMode !== "function") {
    throw new Error("Interactive extension selection requires a TTY. Use open-forge extend --list or open-forge extend --ids <id[,id...]>.");
  }

  let cursor = 0;
  let renderedLines = 0;
  const selected = new Set<string>();
  const stdin = process.stdin;
  const stdout = process.stdout;

  const render = (): void => {
    if (renderedLines > 0) {
      stdout.write(`\x1b[${renderedLines}A`);
    }

    const lines = [
      "Select bundled Open Forge extensions. Space toggles, Enter installs, q cancels.",
      ...extensions.map((extension, index) => {
        const pointer = index === cursor ? ">" : " ";
        const mark = selected.has(extension.id) ? "[x]" : "[ ]";
        const description = extension.description ? ` - ${extension.description}` : "";
        return `${pointer} ${mark} ${extension.id}${description}`;
      })
    ];

    stdout.write(`${lines.map((line) => `\x1b[2K${line}`).join("\n")}\n`);
    renderedLines = lines.length;
  };

  return await new Promise<string[]>((resolve, reject) => {
    const cleanup = (): void => {
      stdin.off("data", onData);
      stdin.setRawMode(false);
      stdin.pause();
      stdout.write("\x1b[?25h");
    };

    const finish = (ids: string[]): void => {
      cleanup();
      stdout.write("\n");
      resolve(ids);
    };

    const cancel = (): void => {
      cleanup();
      stdout.write("\n");
      reject(new Error("Extension selection cancelled"));
    };

    const move = (offset: number): void => {
      cursor = (cursor + offset + extensions.length) % extensions.length;
      render();
    };

    const toggle = (): void => {
      const id = extensions[cursor].id;
      if (selected.has(id)) {
        selected.delete(id);
      } else {
        selected.add(id);
      }
      render();
    };

    const onData = (chunk: Buffer): void => {
      const key = chunk.toString("utf8");
      if (key === "\u0003" || key === "\u001b" || key === "q") {
        cancel();
      } else if (key === "\r" || key === "\n") {
        finish([...selected]);
      } else if (key === " ") {
        toggle();
      } else if (key === "\u001b[A" || key === "k") {
        move(-1);
      } else if (key === "\u001b[B" || key === "j") {
        move(1);
      }
    };

    stdout.write("\x1b[?25l");
    stdin.setRawMode(true);
    stdin.resume();
    stdin.on("data", onData);
    render();
  });
}

type FindOptions = {
  tags: string[];
  route: string | null;
  depth: number;
  followRequired: boolean;
  output: "entries" | "paths" | "bodies" | "json";
  target: string;
};

async function find(findArgs: string[]): Promise<void> {
  const options = parseFindArgs(findArgs);
  const targetRoot = path.resolve(options.target);
  const scanRoot = await isDirectory(path.join(targetRoot, agentsDirectoryName))
    ? path.join(targetRoot, agentsDirectoryName)
    : targetRoot;

  let selected: string[];
  if (options.route) {
    const routeFile = await resolveRouteArg(options.route, targetRoot, scanRoot);
    selected = await expandRouteByDepth(routeFile, options.depth, targetRoot);
  } else {
    selected = await collectRoutedFiles(scanRoot);
  }

  if (options.tags.length > 0) {
    const wanted = options.tags.map((tag) => tag.toLowerCase());
    const filtered: string[] = [];
    for (const file of selected) {
      const effective = (await effectiveTags(file)).map((tag) => tag.toLowerCase());
      if (wanted.every((tag) => effective.includes(tag))) {
        filtered.push(file);
      }
    }
    selected = filtered;
  }

  if (options.followRequired) {
    const missing: string[] = [];
    const required: string[] = [];
    for (const file of selected) {
      const parsed = readRequiredRoutes(await fs.readFile(file, "utf8"));
      for (const requiredPath of parsed.paths) {
        const resolved = path.join(targetRoot, requiredPath);
        if (await isFile(resolved)) {
          required.push(resolved);
        } else {
          missing.push(`${workspaceRoute(targetRoot, file)} -> ${requiredPath}`);
        }
      }
    }
    if (missing.length > 0) {
      throw new Error(`Required routes could not be read (blocker, not a skip):\n${missing.map((line) => `  ${line}`).join("\n")}`);
    }
    selected = dedupePaths([...selected, ...required]);
  }

  selected = dedupePaths(selected);
  if (selected.length === 0) {
    console.log("No routed files matched.");
    return;
  }

  if (options.output === "json") {
    const items = [];
    for (const file of selected) {
      const text = await fs.readFile(file, "utf8");
      const metadata = readMetadata(text);
      items.push({
        route: workspaceRoute(targetRoot, file),
        description: metadata.description || readMarkdownDescription(text) || "No description",
        tags: metadata.tags.length > 0 ? metadata.tags : defaultTagsForIndexEntry(file)
      });
    }
    console.log(JSON.stringify(items, null, 2));
    return;
  }

  for (const file of selected) {
    const route = workspaceRoute(targetRoot, file);
    if (options.output === "paths") {
      console.log(route);
    } else if (options.output === "bodies") {
      const text = await fs.readFile(file, "utf8");
      console.log(`----- ${route} -----`);
      console.log(text.trimEnd());
      console.log("");
    } else {
      console.log(await createGeneratedEntry(file, route));
    }
  }
}

function parseFindArgs(findArgs: string[]): FindOptions {
  const usage = "Usage: open-forge find [--tag <Tag>]... [--route <path>] [--depth <n>] [--follow-required] [--bodies|--paths|--json] [target]";
  const options: FindOptions = { tags: [], route: null, depth: 0, followRequired: false, output: "entries", target: process.cwd() };
  let positional: string | null = null;

  for (let index = 0; index < findArgs.length; index += 1) {
    const value = findArgs[index];
    if (value === "--tag") {
      const tag = findArgs[++index];
      if (!tag) throw new Error(usage);
      options.tags.push(tag.replace(/^#/, ""));
    } else if (value === "--route") {
      options.route = findArgs[++index] ?? null;
      if (!options.route) throw new Error(usage);
    } else if (value === "--depth") {
      const depth = Number(findArgs[++index]);
      if (!Number.isInteger(depth) || depth < 0) throw new Error(usage);
      options.depth = depth;
    } else if (value === "--follow-required") {
      options.followRequired = true;
    } else if (value === "--bodies") {
      options.output = "bodies";
    } else if (value === "--paths") {
      options.output = "paths";
    } else if (value === "--json") {
      options.output = "json";
    } else if (value.startsWith("--")) {
      throw new Error(usage);
    } else if (positional == null) {
      positional = value;
    } else {
      throw new Error(usage);
    }
  }

  if (options.depth > 0 && !options.route) {
    throw new Error("open-forge find: --depth requires --route");
  }

  options.target = positional ?? process.cwd();
  return options;
}

async function resolveRouteArg(routeArg: string, targetRoot: string, scanRoot: string): Promise<string> {
  const normalized = toPosix(routeArg).replace(/^\.\//, "");
  const candidates = [
    path.join(targetRoot, normalized),
    path.join(scanRoot, normalized)
  ];

  for (const candidate of candidates) {
    if (await isFile(candidate)) {
      return candidate;
    }
    if (await isDirectory(candidate)) {
      const entrypoint = await findCategoryEntrypoint(candidate);
      if (entrypoint) {
        return entrypoint;
      }
    }
  }

  throw new Error(`Route not found or not routable: ${routeArg}`);
}

async function expandRouteByDepth(routeFile: string, depth: number, targetRoot: string): Promise<string[]> {
  const visited = new Set<string>([path.resolve(routeFile)]);
  const ordered = [routeFile];
  let frontier = [routeFile];

  for (let level = 0; level < depth; level += 1) {
    const next: string[] = [];
    for (const file of frontier) {
      for (const entryPath of readGeneratedEntryPaths(await fs.readFile(file, "utf8"))) {
        const resolved = entryPath.startsWith(`${agentsDirectoryName}/`)
          ? path.join(targetRoot, entryPath)
          : path.join(path.dirname(file), entryPath);
        const key = path.resolve(resolved);
        if (!visited.has(key) && await isFile(resolved)) {
          visited.add(key);
          ordered.push(resolved);
          next.push(resolved);
        }
      }
    }
    frontier = next;
  }

  return ordered;
}

async function collectRoutedFiles(scanRoot: string): Promise<string[]> {
  const files: string[] = [];
  const loaderFile = path.join(scanRoot, "loader.md");
  if (await isFile(loaderFile)) {
    files.push(loaderFile);
  }

  const rootCategories = (await listIndexEntryFiles(scanRoot)).filter(isIndexFile);
  for (const entrypoint of rootCategories) {
    files.push(entrypoint);
    await collectRoutedFolder(path.dirname(entrypoint), files);
  }

  return files;
}

async function collectRoutedFolder(folder: string, out: string[]): Promise<void> {
  for (const file of await listIndexEntryFiles(folder)) {
    out.push(file);
    if (isIndexFile(file) && !samePath(path.dirname(file), folder)) {
      await collectRoutedFolder(path.dirname(file), out);
    }
  }
}

async function effectiveTags(file: string): Promise<string[]> {
  const metadata = readMetadata(await fs.readFile(file, "utf8"));
  return metadata.tags.length > 0 ? metadata.tags : defaultTagsForIndexEntry(file);
}

function readGeneratedEntryPaths(text: string): string[] {
  const region = readGeneratedRegion(text);
  if (region.status !== "ok") {
    return [];
  }

  const paths: string[] = [];
  for (const line of region.body.split(/\r?\n/)) {
    const match = line.match(/^- `([^`]+)`/);
    if (match) {
      paths.push(match[1]);
    }
  }

  return paths;
}

type RequiredRoutes = {
  paths: string[];
  none: boolean;
  invalid: string[];
  present: boolean;
};

function readRequiredRoutes(rawText: string): RequiredRoutes {
  const result: RequiredRoutes = { paths: [], none: false, invalid: [], present: false };
  const text = stripFencedCodeBlocks(rawText);
  const match = text.match(/^## Required Routes\s*$/m);
  if (!match || match.index == null) {
    return result;
  }

  result.present = true;
  const after = text.slice(match.index + match[0].length);
  const nextHeading = after.search(/^## /m);
  const section = nextHeading === -1 ? after : after.slice(0, nextHeading);

  for (const raw of section.split(/\r?\n/)) {
    const line = raw.trim();
    if (!line) {
      continue;
    }

    const entry = line.match(/^- `([^`]+)`/);
    if (entry) {
      result.paths.push(toPosix(entry[1]));
      continue;
    }

    if (/^(- )?none\b/i.test(line)) {
      result.none = true;
      continue;
    }

    if (line.startsWith("- ")) {
      result.invalid.push(line);
    }
  }

  return result;
}

function stripFencedCodeBlocks(text: string): string {
  const lines = text.split(/\r?\n/);
  const kept: string[] = [];
  let inFence = false;

  for (const line of lines) {
    if (line.trimStart().startsWith("```")) {
      inFence = !inFence;
      continue;
    }
    if (!inFence) {
      kept.push(line);
    }
  }

  return kept.join("\n");
}

type GeneratedRegion =
  | { status: "ok"; body: string }
  | { status: "none" }
  | { status: "malformed"; reason: string };

function readGeneratedRegion(text: string): GeneratedRegion {
  const startMarkers = findAllOccurrences(text, generatedIndexStartMarker);
  const endMarkers = findAllOccurrences(text, generatedIndexEndMarker);

  if (startMarkers.length === 0 && endMarkers.length === 0) {
    return { status: "none" };
  }

  if (startMarkers.length !== 1 || endMarkers.length !== 1) {
    return { status: "malformed", reason: "the generated index markers are incomplete or duplicated" };
  }

  if (endMarkers[0] <= startMarkers[0]) {
    return { status: "malformed", reason: "the generated index markers are reversed" };
  }

  const body = text.slice(startMarkers[0] + generatedIndexStartMarker.length, endMarkers[0]).trim();
  return { status: "ok", body };
}

function workspaceRoute(targetRoot: string, file: string): string {
  return toPosix(path.relative(targetRoot, file));
}

function dedupePaths(files: string[]): string[] {
  const seen = new Set<string>();
  const result: string[] = [];
  for (const file of files) {
    const key = path.resolve(file);
    if (!seen.has(key)) {
      seen.add(key);
      result.push(file);
    }
  }
  return result;
}

type DoctorFinding = {
  level: "error" | "warning";
  route: string;
  message: string;
};

async function doctor(doctorArgs: string[]): Promise<void> {
  const usage = "Usage: open-forge doctor [--json] [target]";
  let json = false;
  let positional: string | null = null;
  for (const value of doctorArgs) {
    if (value === "--json") {
      json = true;
    } else if (value.startsWith("--")) {
      throw new Error(usage);
    } else if (positional == null) {
      positional = value;
    } else {
      throw new Error(usage);
    }
  }

  const targetRoot = path.resolve(positional ?? process.cwd());
  const scanRoot = await isDirectory(path.join(targetRoot, agentsDirectoryName))
    ? path.join(targetRoot, agentsDirectoryName)
    : targetRoot;
  const findings: DoctorFinding[] = [];
  const report = (level: DoctorFinding["level"], file: string, message: string): void => {
    findings.push({ level, route: workspaceRoute(targetRoot, file), message });
  };

  const markdownFiles = await listFiles(scanRoot, isMarkdownFile);

  for (const [directory, names] of await groupEntrypointCandidates(markdownFiles)) {
    if (names.length > 1) {
      report("error", directory, `multiple recognized entrypoints: ${names.sort().join(", ")}; keep exactly one`);
    }
  }

  let routedFiles: string[] = [];
  try {
    routedFiles = dedupePaths(await collectRoutedFiles(scanRoot));
  } catch (error) {
    report("error", scanRoot, error instanceof Error ? error.message : String(error));
  }

  const loaderFile = path.join(scanRoot, "loader.md");
  const regionOwners = routedFiles.filter((file) => isIndexFile(file));

  for (const owner of [...(await isFile(loaderFile) ? [loaderFile] : []), ...regionOwners]) {
    const text = await fs.readFile(owner, "utf8");
    const region = readGeneratedRegion(text);
    if (region.status === "malformed") {
      report("error", owner, region.reason);
      continue;
    }
    if (region.status === "none") {
      report("warning", owner, "no generated index region; run open-forge index");
      continue;
    }

    const expected = samePath(owner, loaderFile)
      ? await computeLoaderBody(scanRoot)
      : await computeIndexBody(owner, path.dirname(owner));
    if (region.body !== expected.trim()) {
      report("warning", owner, "generated region is stale; run open-forge index");
    }

    for (const entryPath of readGeneratedEntryPaths(text)) {
      const resolved = entryPath.startsWith(`${agentsDirectoryName}/`)
        ? path.join(targetRoot, entryPath)
        : path.join(path.dirname(owner), entryPath);
      if (!(await isFile(resolved))) {
        report("error", owner, `generated entry does not resolve: ${entryPath}`);
      }
    }
  }

  for (const file of markdownFiles) {
    const text = await fs.readFile(file, "utf8");
    const metadata = readMetadata(text);
    for (const tag of metadata.tags) {
      if (retiredLoadPolicyTags.has(tag.toLowerCase())) {
        report("warning", file, `retired load-policy tag in metadata: ${tag}`);
      }
    }

    if (file.endsWith(".overwrite.md")) {
      const base = file.slice(0, -".overwrite.md".length) + ".md";
      if (!(await isFile(base))) {
        report("warning", file, "overwrite companion has no base file");
      }
    }

    const required = readRequiredRoutes(text);
    if (required.present) {
      if (!required.none && required.paths.length === 0) {
        report("warning", file, "Required Routes section has no parseable routes and does not state none");
      }
      for (const requiredPath of required.paths) {
        if (!(await isFile(path.join(targetRoot, requiredPath)))) {
          report("error", file, `required route does not resolve: ${requiredPath}`);
        }
      }
      for (const invalid of required.invalid) {
        report("warning", file, `Required Routes line is not in entry format: ${invalid}`);
      }
    }
  }

  const routedSet = new Set(routedFiles.map((file) => path.resolve(file)));
  for (const file of markdownFiles) {
    if (routedSet.has(path.resolve(file))) continue;
    if (samePath(file, loaderFile)) continue;
    if (file.endsWith(".overwrite.md")) continue;
    if (await insideSkillPackage(file, scanRoot)) continue;
    if (samePath(path.dirname(file), scanRoot)) continue;
    report("warning", file, "not reachable through generated routing");
  }

  const errors = findings.filter((finding) => finding.level === "error");
  if (json) {
    console.log(JSON.stringify({ errors: errors.length, warnings: findings.length - errors.length, findings }, null, 2));
  } else if (findings.length === 0) {
    console.log(`open-forge doctor: no problems found in ${targetRoot}`);
  } else {
    for (const finding of findings) {
      console.log(`${finding.level}: ${finding.route} - ${finding.message}`);
    }
    console.log(`open-forge doctor: ${errors.length} error(s), ${findings.length - errors.length} warning(s)`);
  }

  if (errors.length > 0) {
    process.exitCode = 1;
  }
}

async function groupEntrypointCandidates(markdownFiles: string[]): Promise<Map<string, string[]>> {
  const byDirectory = new Map<string, string[]>();
  for (const file of markdownFiles) {
    if (!isIndexFile(file)) continue;
    const directory = path.dirname(file);
    const names = byDirectory.get(directory) ?? [];
    names.push(path.basename(file));
    byDirectory.set(directory, names);
  }
  return byDirectory;
}

async function insideSkillPackage(file: string, scanRoot: string): Promise<boolean> {
  let current = path.dirname(file);
  while (!samePath(current, scanRoot) && current !== path.dirname(current)) {
    for (const name of skillEntrypointNames) {
      if (await isFile(path.join(current, name))) {
        return true;
      }
    }
    current = path.dirname(current);
  }
  return false;
}

async function create(createArgs: string[]): Promise<void> {
  const kind = createArgs[0];
  if (kind === "category") {
    await createCategoryRoute(createArgs[1], createArgs[2] ?? process.cwd());
    return;
  }
  if (kind === "extension") {
    await createExtensionScaffold(createArgs[1], createArgs[2] ?? process.cwd());
    return;
  }
  throw new Error("Usage: open-forge create category <route-path> [target] | open-forge create extension <id> [directory]");
}

async function createCategoryRoute(routeArg: string | undefined, targetArg: string): Promise<void> {
  if (!routeArg) {
    throw new Error("Usage: open-forge create category <route-path> [target]");
  }

  const targetRoot = path.resolve(targetArg);
  const normalized = toPosix(routeArg).replace(/^\.\//, "").replace(/\/+$/, "");
  const routePath = normalized.startsWith(`${agentsDirectoryName}/`) ? normalized : `${agentsDirectoryName}/${normalized}`;
  const segments = routePath.split(portablePathSeparator).slice(1);

  if (segments.length === 0 || segments.some((segment) => !/^[A-Za-z0-9][A-Za-z0-9._-]*$/.test(segment))) {
    throw new Error(`Route path segments must be concrete slug folders: ${routeArg}`);
  }

  const created: string[] = [];
  let currentFolder = path.join(targetRoot, agentsDirectoryName);
  for (const segment of segments) {
    currentFolder = path.join(currentFolder, segment);
    await ensureDir(currentFolder);
    const existing = await findCategoryEntrypoint(currentFolder);
    if (existing) {
      continue;
    }

    const entrypointFile = path.join(currentFolder, `_${segment}.md`);
    await fs.writeFile(entrypointFile, categoryEntrypointTemplate(segment, segments[0]));
    created.push(workspaceRoute(targetRoot, entrypointFile));
  }

  if (created.length === 0) {
    throw new Error(`Route is already routable: ${routePath}`);
  }

  const generatedRegions = await generateIndexes(targetRoot);
  console.log(`Created ${created.length} category entrypoint(s):`);
  for (const file of created) {
    console.log(`- ${file}`);
  }
  console.log(`Rebuilt ${generatedRegions} generated regions. Fill in the TODO descriptions, then run open-forge index again.`);
}

function categoryEntrypointTemplate(segment: string, topSegment: string): string {
  const typeTag = categoryTypeTags[topSegment];
  const tagsLine = typeTag ? `\n  tags: [${typeTag}]` : "";
  const title = segment
    .split(/[-_.]/)
    .filter(Boolean)
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(" ");

  return `---
open-forge:
  description: TODO - when to select this route and what it provides${tagsLine}
---

# ${title}

TODO - one short definition of this category.

## Entries

${generatedIndexStartMarker}
- none - No entries - #Empty
${generatedIndexEndMarker}
`;
}

async function createExtensionScaffold(idArg: string | undefined, directoryArg: string): Promise<void> {
  if (!idArg || !isBundledExtensionId(idArg)) {
    throw new Error("Usage: open-forge create extension <id> [directory]; ids are lowercase kebab-case such as my-patterns");
  }

  const baseDirectory = path.join(path.resolve(directoryArg), idArg);
  if (await isDirectory(baseDirectory) || await isFile(baseDirectory)) {
    throw new Error(`Extension directory already exists: ${baseDirectory}`);
  }

  const name = idArg
    .split("-")
    .filter(Boolean)
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(" ");

  await ensureDir(path.join(baseDirectory, "payload", agentsDirectoryName));
  await fs.writeFile(
    path.join(baseDirectory, "extension.json"),
    `${JSON.stringify({ name, description: "TODO - one line shown by open-forge extend --list" }, null, 2)}\n`
  );
  await fs.writeFile(
    path.join(baseDirectory, "README.md"),
    `# ${name}

TODO - what this extension installs and when to use it.

Installable runtime content lives under \`payload/.agents/\`. Only \`payload/\` is copied on install. Use #Extension plus route type and scope tags in Open Forge-authored payload files; use a load-policy tag only when baseline loading is deliberate.

Install with:

\`\`\`sh
open-forge extend ${idArg} <target>
\`\`\`
`
  );

  console.log(`Created extension scaffold at ${baseDirectory}`);
  console.log("Add routed files under payload/.agents/, then install with open-forge extend.");
}

type FrameworkEntrypointTemplate = {
  relativePath: string;
  sourceFile: string;
  folderSegments: string[];
  anchor: "start" | "any";
};

function createFrameworkEntrypointTemplate(root: string, sourceFile: string): FrameworkEntrypointTemplate[] {
  const relativePath = toPosix(path.relative(root, sourceFile));

  if (!relativePath.startsWith(`${agentsDirectoryName}/`)) {
    return [];
  }

  const segments = relativePath.slice(`${agentsDirectoryName}/`.length).split(portablePathSeparator);
  if (!isCanonicalCategoryEntrypointSegments(segments)) {
    return [];
  }

  const folderSegments = segments.slice(0, -1);
  const anchor = folderSegments[0] === "memory" || !scopedCoreEntrypointFolders.has(folderSegments[0]) ? "start" : "any";
  return [{ relativePath, sourceFile, folderSegments, anchor }];
}

async function updateScopedFrameworkEntrypoints(targetRoot: string, templates: FrameworkEntrypointTemplate[]): Promise<number> {
  const agentsRoot = path.join(targetRoot, agentsDirectoryName);
  if (!(await isDirectory(agentsRoot))) {
    return 0;
  }

  const markdownFiles = await listFiles(agentsRoot, (file) => file.endsWith(".md"));
  let count = 0;

  for (const targetFile of markdownFiles) {
    const relativePath = toPosix(path.relative(targetRoot, targetFile));
    const template = findFrameworkEntrypointTemplate(relativePath, templates);
    if (!template) {
      continue;
    }

    await fs.writeFile(targetFile, await fs.readFile(template.sourceFile, "utf8"));
    count += 1;
  }

  return count;
}

function findFrameworkEntrypointTemplate(relativePath: string, templates: FrameworkEntrypointTemplate[]): FrameworkEntrypointTemplate | null {
  if (!relativePath.startsWith(`${agentsDirectoryName}/`)) {
    return null;
  }

  const routePath = relativePath.slice(`${agentsDirectoryName}/`.length);
  const targetSegments = routePath.split(portablePathSeparator);
  if (!isCanonicalCategoryEntrypointSegments(targetSegments)) {
    return null;
  }

  const matches = templates
    .filter((template) => template.relativePath !== relativePath && matchesFrameworkEntrypointShape(targetSegments, template))
    .sort((left, right) => right.folderSegments.length - left.folderSegments.length);

  return matches[0] ?? null;
}

function matchesFrameworkEntrypointShape(targetSegments: string[], template: FrameworkEntrypointTemplate): boolean {
  const targetFolders = targetSegments.slice(0, -1);
  const targetFile = targetSegments[targetSegments.length - 1];
  const templateFolders = template.folderSegments;
  const terminalFolder = templateFolders[templateFolders.length - 1];

  if (targetFile !== `_${terminalFolder}.md` || targetFolders[targetFolders.length - 1] !== terminalFolder) {
    return false;
  }

  const searchableFolders = targetFolders.slice(0, -1);
  const fixedPrefix = templateFolders.slice(0, -1);
  if (template.anchor === "start") {
    if (searchableFolders[0] !== fixedPrefix[0]) {
      return false;
    }

    return containsOrderedSegments(searchableFolders.slice(1), fixedPrefix.slice(1));
  }

  return containsOrderedSegments(searchableFolders, fixedPrefix);
}

function containsOrderedSegments(haystack: string[], needles: string[]): boolean {
  let offset = 0;

  for (const needle of needles) {
    const index = haystack.indexOf(needle, offset);
    if (index === -1) {
      return false;
    }

    offset = index + 1;
  }

  return true;
}

function isCanonicalCategoryEntrypointSegments(segments: string[]): boolean {
  if (segments.length < 2) {
    return false;
  }

  const folderName = segments[segments.length - 2];
  const fileName = segments[segments.length - 1];
  return fileName === `_${folderName}.md`;
}

async function generateIndexes(root: string): Promise<number> {
  const agentsRoot = path.join(root, ".agents");
  const scanRoot = await isDirectory(agentsRoot) ? agentsRoot : root;
  const markdownFiles = await listFiles(scanRoot, (file) => isIndexFile(file));
  assertUnambiguousCategoryEntrypoints(markdownFiles);
  let count = 0;

  for (const indexFile of markdownFiles) {
    const directory = path.dirname(indexFile);

    if (!(await isDirectory(directory))) {
      continue;
    }

    await generateIndex(indexFile, directory);
    count += 1;
  }

  const loaderFile = path.join(scanRoot, "loader.md");
  if (await isFile(loaderFile)) {
    await generateLoaderRegistry(loaderFile, scanRoot);
    count += 1;
  }

  return count;
}

async function generateIndex(indexFile: string, folder: string): Promise<void> {
  const current = await fs.readFile(indexFile, "utf8");
  const body = await computeIndexBody(indexFile, folder);
  await fs.writeFile(indexFile, updateGeneratedIndexRegion(current, body, indexFile));
}

async function computeIndexBody(indexFile: string, folder: string): Promise<string> {
  const entries: string[] = [];
  const files = await listIndexEntryFiles(folder);

  for (const file of files) {
    const relativeFile = toPosix(path.relative(path.dirname(indexFile), file));
    entries.push(await createGeneratedEntry(file, relativeFile));
  }

  return entries.length > 0 ? entries.join("\n") : "- none - No entries - #Empty";
}

async function generateLoaderRegistry(loaderFile: string, agentsRoot: string): Promise<void> {
  const current = await fs.readFile(loaderFile, "utf8");
  const body = await computeLoaderBody(agentsRoot);
  await fs.writeFile(loaderFile, updateGeneratedIndexRegion(current, body, loaderFile));
}

async function computeLoaderBody(agentsRoot: string): Promise<string> {
  const entries: string[] = [];
  const categoryFiles = (await listIndexEntryFiles(agentsRoot)).filter(isIndexFile);

  for (const file of categoryFiles) {
    entries.push(await createGeneratedEntry(file, loaderRoutePath(agentsRoot, file)));
  }

  return entries.length > 0 ? entries.join("\n") : "- none - No active categories - #Empty";
}

function loaderRoutePath(agentsRoot: string, file: string): string {
  const relativeFile = toPosix(path.relative(agentsRoot, file));
  return path.basename(agentsRoot) === ".agents" ? `.agents/${relativeFile}` : relativeFile;
}

async function createGeneratedEntry(file: string, route: string): Promise<string> {
  const text = await fs.readFile(file, "utf8");
  const metadata = readMetadata(text);
  const description = metadata.description || readMarkdownDescription(text) || "No description";
  const tags = metadata.tags.length > 0 ? metadata.tags : defaultTagsForIndexEntry(file);
  return `- \`${route}\` - ${description} - ${formatTags(tags)}`;
}

function updateGeneratedIndexRegion(text: string, body: string, indexFile: string): string {
  const newline = text.includes("\r\n") ? "\r\n" : "\n";
  const startMarkers = findAllOccurrences(text, generatedIndexStartMarker);
  const endMarkers = findAllOccurrences(text, generatedIndexEndMarker);

  if (startMarkers.length === 1 && endMarkers.length === 1) {
    const start = startMarkers[0];
    const end = endMarkers[0];

    if (end <= start) {
      throw malformedIndexError(indexFile, "the generated index markers are reversed");
    }

    assertGeneratedIndexLayout(text, start, end, indexFile);
    const bodyStart = start + generatedIndexStartMarker.length;
    return `${text.slice(0, bodyStart)}${newline}${body}${newline}${text.slice(end)}`;
  }

  if (startMarkers.length !== 0 || endMarkers.length !== 0) {
    throw malformedIndexError(indexFile, "the generated index markers are incomplete or duplicated");
  }

  return migrateLegacyIndexRegion(text, body, indexFile, newline);
}

function assertGeneratedIndexLayout(text: string, start: number, end: number, indexFile: string): void {
  const beforeMarker = text.slice(0, start);
  const headings = findEntriesHeadings(beforeMarker);

  if (headings.length !== 1) {
    throw malformedIndexError(indexFile, `expected exactly one ${entriesHeading} heading before the generated region`);
  }

  const heading = headings[0];
  const headingEnd = heading + entriesHeading.length;
  if (beforeMarker.slice(headingEnd).trim() !== "") {
    throw malformedIndexError(indexFile, `the generated region must immediately follow ${entriesHeading}`);
  }

  const afterMarker = text.slice(end + generatedIndexEndMarker.length);
  if (afterMarker.trim() !== "") {
    throw malformedIndexError(indexFile, "the generated region must be the final section");
  }
}

function migrateLegacyIndexRegion(text: string, body: string, indexFile: string, newline: string): string {
  const headings = findEntriesHeadings(text);

  if (headings.length > 1) {
    throw malformedIndexError(indexFile, `found multiple ${entriesHeading} headings without generated-region markers`);
  }

  if (headings.length === 1) {
    const legacyBody = text.slice(headings[0] + entriesHeading.length);
    const invalidLine = legacyBody
      .split(/\r?\n/)
      .map((line) => line.trim())
      .find((line) => line && !line.startsWith("- "));

    if (invalidLine) {
      throw malformedIndexError(indexFile, `the legacy ${entriesHeading} section contains authored content`);
    }
  }

  const prefix = headings.length === 1 ? text.slice(0, headings[0]).trimEnd() : text.trimEnd();
  const separator = prefix ? `${newline}${newline}` : "";
  return `${prefix}${separator}${entriesHeading}${newline}${newline}${generatedIndexStartMarker}${newline}${body}${newline}${generatedIndexEndMarker}${newline}`;
}

function findEntriesHeadings(text: string): number[] {
  const indexes: number[] = [];
  const pattern = /^## Entries\s*$/gm;

  for (const match of text.matchAll(pattern)) {
    if (match.index != null) {
      indexes.push(match.index);
    }
  }

  return indexes;
}

function findAllOccurrences(text: string, value: string): number[] {
  const indexes: number[] = [];
  let offset = 0;

  while (offset < text.length) {
    const index = text.indexOf(value, offset);
    if (index === -1) {
      break;
    }

    indexes.push(index);
    offset = index + value.length;
  }

  return indexes;
}

function malformedIndexError(indexFile: string, reason: string): Error {
  return new Error(`Cannot rebuild ${indexFile}: ${reason}. No changes were written.`);
}

function isIndexFile(file: string): boolean {
  const basename = path.basename(file);
  const folderName = path.basename(path.dirname(file));
  return categoryEntrypointNames(folderName).includes(basename);
}

function assertUnambiguousCategoryEntrypoints(files: string[]): void {
  const byDirectory = new Map<string, string[]>();

  for (const file of files) {
    const directory = path.dirname(file);
    const matches = byDirectory.get(directory) ?? [];
    matches.push(path.basename(file));
    byDirectory.set(directory, matches);
  }

  for (const [directory, matches] of byDirectory) {
    if (matches.length > 1) {
      throw new Error(`Multiple category entrypoints found in ${directory}: ${matches.sort().join(", ")}. Keep exactly one.`);
    }
  }
}

function categoryEntrypointNames(folderName: string): string[] {
  return [`_${folderName}.md`, ...compatibilityEntrypointNames];
}

async function listIndexEntryFiles(current: string): Promise<string[]> {
  const entries = await fs.readdir(current, { withFileTypes: true });
  const files: string[] = [];

  for (const entry of entries.sort(compareIndexEntries)) {
    const fullPath = path.join(current, entry.name);

    if (entry.isDirectory()) {
      if (ignoredDirectoryNames.has(entry.name)) {
        continue;
      }

      const childIndex = await findCategoryEntrypoint(fullPath);
      const skillEntrypoint = isSkillsRouteFolder(current) ? await findSkillEntrypoint(fullPath) : null;
      if (childIndex && skillEntrypoint) {
        throw new Error(`Both category and skill entrypoints found in ${fullPath}. Keep either a category entrypoint or a skill ${path.basename(skillEntrypoint)}.`);
      }

      if (childIndex) {
        files.push(childIndex);
      } else if (skillEntrypoint) {
        files.push(skillEntrypoint);
      }
      continue;
    }

    if (!isSkillsRouteFolder(current) && entry.isFile() && isIndexEntryFile(entry.name, path.basename(current))) {
      files.push(fullPath);
    }
  }

  return files;
}

async function findCategoryEntrypoint(directory: string): Promise<string | null> {
  const folderName = path.basename(directory);
  const matches: string[] = [];

  for (const name of categoryEntrypointNames(folderName)) {
    const candidate = path.join(directory, name);
    if (await isFile(candidate)) {
      matches.push(candidate);
    }
  }

  if (matches.length > 1) {
    throw new Error(`Multiple category entrypoints found in ${directory}: ${matches.map((file) => path.basename(file)).sort().join(", ")}. Keep exactly one.`);
  }

  return matches[0] ?? null;
}

async function findSkillEntrypoint(directory: string): Promise<string | null> {
  const entries = await fs.readdir(directory, { withFileTypes: true });
  const matches = entries
    .filter((entry) => entry.isFile() && skillEntrypointNames.includes(entry.name))
    .map((entry) => path.join(directory, entry.name));

  if (matches.length > 1) {
    throw new Error(`Multiple skill entrypoints found in ${directory}: ${matches.map((file) => path.basename(file)).sort().join(", ")}. Keep exactly one.`);
  }

  return matches[0] ?? null;
}

function isSkillsRouteFolder(directory: string): boolean {
  const segments = toPosix(path.resolve(directory)).split(portablePathSeparator);
  const agentsIndex = segments.lastIndexOf(agentsDirectoryName);
  return agentsIndex !== -1 && segments[agentsIndex + 1] === skillsDirectoryName;
}

function compareIndexEntries(left: Dirent, right: Dirent): number {
  const leftKey = indexSortKey(left);
  const rightKey = indexSortKey(right);
  const keyComparison = leftKey.localeCompare(rightKey);

  if (keyComparison !== 0) {
    return keyComparison;
  }

  if (left.isFile() !== right.isFile()) {
    return left.isFile() ? -1 : 1;
  }

  return left.name.localeCompare(right.name);
}

function indexSortKey(entry: Dirent): string {
  return entry.isFile() && entry.name.endsWith(".md") ? entry.name.slice(0, -3) : entry.name;
}

function isIndexEntryFile(name: string, folderName: string): boolean {
  return (
    name.endsWith(".md") &&
    !name.endsWith(".overwrite.md") &&
    !skillEntrypointNames.includes(name) &&
    !categoryEntrypointNames(folderName).includes(name)
  );
}

function isMarkdownFile(file: string): boolean {
  return path.extname(file).toLowerCase() === ".md";
}

function defaultTagsForIndexEntry(file: string): string[] {
  if (skillEntrypointNames.includes(path.basename(file))) {
    return ["Skill"];
  }

  return isIndexFile(file) ? ["Index"] : ["Untagged"];
}

type Metadata = {
  description: string;
  tags: string[];
};

function readMetadata(text: string): Metadata {
  const frontmatter = readFrontmatter(text);
  if (!frontmatter) {
    return { description: "", tags: [] };
  }

  const section = readSection(frontmatter, "open-forge");
  if (section) {
    const description = readScalar(section, "description");
    const tags = readTags(section);
    return { description, tags };
  }

  const runeSection = readSection(frontmatter, "rune");
  if (runeSection) {
    const description = readScalar(runeSection, "description");
    const tags = readTags(runeSection);
    return { description, tags };
  }

  return {
    description: readScalar(frontmatter, "description", true),
    tags: readTags(frontmatter, true)
  };
}

function readFrontmatter(text: string): string {
  const normalized = text.replace(/^\uFEFF/, "").trimStart();
  if (!normalized.startsWith("---")) {
    return "";
  }

  const match = normalized.match(/^---\r?\n([\s\S]*?)\r?\n---/);
  return match ? match[1] : "";
}

function readMarkdownDescription(text: string): string {
  const body = stripFrontmatter(text);
  let inFence = false;

  for (const line of body.split(/\r?\n/)) {
    const trimmed = line.trim();

    if (trimmed.startsWith("```")) {
      inFence = !inFence;
      continue;
    }

    if (inFence || !trimmed || trimmed.startsWith("#") || trimmed.startsWith("<!--") || trimmed.startsWith("- ")) {
      continue;
    }

    return trimmed;
  }

  return "";
}

function stripFrontmatter(text: string): string {
  const normalized = text.replace(/^\uFEFF/, "").trimStart();
  if (!normalized.startsWith("---")) {
    return normalized;
  }

  return normalized.replace(/^---\r?\n[\s\S]*?\r?\n---/, "").trimStart();
}

function readSection(frontmatter: string, name: string): string {
  const lines = frontmatter.split(/\r?\n/);
  const start = lines.findIndex((line) => line.trim() === `${name}:`);

  if (start === -1) {
    return "";
  }

  const section: string[] = [];
  for (const line of lines.slice(start + 1)) {
    if (line.length > 0 && !/^\s/.test(line)) {
      break;
    }

    section.push(line.replace(/^\s{2}/, ""));
  }

  return section.join("\n");
}

function readScalar(text: string, key: string, rootOnly = false): string {
  const prefix = rootOnly ? "" : "\\s*";
  const match = text.match(new RegExp(`^${prefix}${escapeRegex(key)}:\\s*(.+)$`, "m"));
  return match ? cleanValue(match[1]) : "";
}

function readTags(text: string, rootOnly = false): string[] {
  const lines = text.split(/\r?\n/);

  for (let index = 0; index < lines.length; index += 1) {
    const line = lines[index];
    const match = line.match(/^(\s*)tags:\s*(.*)$/);

    if (!match || (rootOnly && match[1].length > 0)) {
      continue;
    }

    const indent = match[1].length;
    const inline = match[2].trim();

    if (inline) {
      return splitTags(inline);
    }

    const tags: string[] = [];
    for (const next of lines.slice(index + 1)) {
      const nextIndent = next.match(/^(\s*)/)?.[1].length ?? 0;
      if (next.trim() && nextIndent <= indent) {
        break;
      }

      const item = next.match(/^\s*-\s*(.+)$/);
      if (item) {
        tags.push(cleanTag(item[1]));
      }
    }

    return tags.filter(Boolean);
  }

  return [];
}

function splitTags(value: string): string[] {
  const cleaned = value.trim();
  const inner = cleaned.startsWith("[") && cleaned.endsWith("]") ? cleaned.slice(1, -1) : cleaned;
  return inner.split(",").map(cleanTag).filter(Boolean);
}

function cleanValue(value: string): string {
  return value.trim().replace(/^["']|["']$/g, "");
}

function cleanTag(value: string): string {
  return cleanValue(value).replace(/^#/, "");
}

function formatTags(tags: string[]): string {
  return tags.map((tag) => `#${tag}`).join(" ");
}

function patchMarkedBlock(targetText: string, sourceText: string, markerName: string): string {
  const sourceBlock = findMarkedBlock(sourceText, markerName);
  if (!sourceBlock) {
    return targetText;
  }

  const targetPattern = markedBlockRegex(markerName);
  if (targetPattern.test(targetText)) {
    return targetText.replace(targetPattern, sourceBlock);
  }

  const trimmed = targetText.trimEnd();
  return `${trimmed}\n\n${sourceBlock}\n`;
}

function preserveLocalBlocks(sourceText: string, targetText: string): string {
  let nextText = sourceText;
  const markers = [...sourceText.matchAll(/<!--\s*([a-z0-9_.-]+):start\s*-->/gi)].map((match) => match[1]);

  for (const marker of markers) {
    const targetBlock = findMarkedBlock(targetText, marker);
    if (targetBlock) {
      nextText = nextText.replace(markedBlockRegex(marker), targetBlock);
    }
  }

  return nextText;
}

function findMarkedBlock(text: string, markerName: string): string {
  return text.match(markedBlockRegex(markerName))?.[0] ?? "";
}

function markedBlockRegex(markerName: string): RegExp {
  const escaped = escapeRegex(markerName);
  return new RegExp(`<!--\\s*${escaped}:start\\s*-->[\\s\\S]*?<!--\\s*${escaped}:end\\s*-->`);
}

async function listFiles(root: string, predicate: (file: string) => boolean = () => true): Promise<string[]> {
  const entries = await fs.readdir(root, { withFileTypes: true });
  const files: string[] = [];

  for (const entry of entries) {
    const fullPath = path.join(root, entry.name);

    if (entry.isDirectory()) {
      if (ignoredDirectoryNames.has(entry.name)) {
        continue;
      }

      files.push(...await listFiles(fullPath, predicate));
    } else if (entry.isFile() && predicate(fullPath)) {
      files.push(fullPath);
    }
  }

  return files;
}

async function readTextIfExists(file: string): Promise<string | null> {
  try {
    return await fs.readFile(file, "utf8");
  } catch (error) {
    if (isNodeError(error) && error.code === "ENOENT") {
      return null;
    }

    throw error;
  }
}

async function ensureDir(directory: string): Promise<void> {
  await fs.mkdir(directory, { recursive: true });
}

async function isFile(file: string): Promise<boolean> {
  try {
    await fs.access(file, fsConstants.F_OK);
    return (await fs.stat(file)).isFile();
  } catch {
    return false;
  }
}

async function isDirectory(directory: string): Promise<boolean> {
  try {
    await fs.access(directory, fsConstants.F_OK);
    return (await fs.stat(directory)).isDirectory();
  } catch {
    return false;
  }
}

function toPosix(value: string): string {
  return value.split(path.sep).join("/");
}

function samePath(left: string, right: string): boolean {
  const normalize = (value: string) => process.platform === "win32" ? path.resolve(value).toLowerCase() : path.resolve(value);
  return normalize(left) === normalize(right);
}

function escapeRegex(value: string): string {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function isNodeError(error: unknown): error is NodeJS.ErrnoException {
  return error instanceof Error && "code" in error;
}

function resolveRepoRoot(): string {
  const entryDirectory = path.dirname(fileURLToPath(import.meta.url));
  const candidates = [
    path.resolve(entryDirectory, ".."),
    path.resolve(entryDirectory, "../..")
  ];

  for (const candidate of candidates) {
    if (existsSync(path.join(candidate, "src", "open-forge", "AGENTS.md"))) {
      return candidate;
    }
  }

  return candidates[0];
}

function printHelp(): void {
  console.log(`open-forge

Usage:
  open-forge install [target]
  open-forge extend
  open-forge extend --list
  open-forge extend --select [target]
  open-forge extend --ids <id[,id...]> [target]
  open-forge extend <extension-source-or-id> [target]
  open-forge index [target]
  open-forge find [--tag <Tag>]... [--route <path>] [--depth <n>] [--follow-required] [--bodies|--paths|--json] [target]
  open-forge doctor [--json] [target]
  open-forge create category <route-path> [target]
  open-forge create extension <id> [directory]

Commands:
  install  Copy files into target, update AGENTS.md, and rebuild generated index regions.
  extend   Copy a local or bundled extension overlay into target and rebuild generated index regions.
  index    Rebuild the loader registry and category generated regions.
  find     List routed files by tag or route; --bodies prints contents, --follow-required includes Required Routes.
  doctor   Validate route integrity: entrypoints, generated regions, entry resolution, retired tags, required routes.
  create   Scaffold a category route chain with entrypoints, or a new extension package.
`);
}
