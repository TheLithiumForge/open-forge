import { posix } from "node:path";
import * as ts from "typescript";
import { compilerOptions, descendants, jsonObject, syntax, type SourceFile } from "./source.ts";
import { object } from "./requirements.ts";

const Extensions = ["", ".ts", ".tsx", ".js", ".jsx", ".json", "/index.ts", "/index.tsx"] as const;
const VirtualRoot = "/production";

export function productionFiles(files: readonly SourceFile[]): SourceFile[] {
  const byPath = new Map(files.map((file) => [file.path, file]));
  const entry = byPath.get("index.html");
  if (!entry) throw new Error("The production HTML entry is missing.");
  const manifest = jsonObject(byPath.get("package.json")?.text ?? "{}");
  const packages = new Set(["dependencies", "devDependencies"].flatMap((name) => object(manifest[name]) ? Object.keys(manifest[name]) : []));
  const configs = files.filter((file) => /(?:^|\/)tsconfig[^/]*\.json$/.test(file.path));
  const options = configs.length ? configs.map((file) => ts.convertCompilerOptionsFromJson({ moduleResolution: "bundler", ...compilerOptions(files, file.path) }, VirtualRoot).options) : [{ moduleResolution: ts.ModuleResolutionKind.Bundler }];
  const logical = (path: string): string => posix.relative(VirtualRoot, path);
  // Resolution reads only the supplied source map, including compiler path aliases.
  const host: ts.ModuleResolutionHost = { fileExists: (path) => byPath.has(logical(path)), readFile: (path) => byPath.get(logical(path))?.text };
  const queue = [...entry.text.matchAll(/<script\b[^>]*\bsrc\s*=\s*["']([^"']+)["']/gi)].map((match) => match[1] ?? "");
  const inline = [...entry.text.matchAll(/<script\b([^>]*)>([\s\S]*?)<\/script\s*>/gi)].flatMap((match, index) => {
    const attributes = match[1] ?? "", text = match[2] ?? "";
    if (/\bsrc\s*=|\btype\s*=\s*["'](?:application\/ld\+json|application\/json)["']/i.test(attributes) || !text.trim()) return [];
    return [{ path: `index.inline-${index}.ts`, text }];
  });
  for (const file of inline) byPath.set(file.path, file);
  const found = new Map<string, SourceFile>([[entry.path, entry]]);
  const resolve = (from: string, specifier: string): string | undefined => {
    if (specifier.startsWith(".") || specifier.startsWith("/")) {
      const path = posix.normalize(specifier.startsWith("/") ? specifier.slice(1) : posix.join(posix.dirname(from), specifier));
      const clean = path.split(/[?#]/)[0] ?? path;
      const resolved = Extensions.map((extension) => clean + extension).find((candidate) => byPath.has(candidate));
      if (resolved) return resolved;
    }
    for (const option of options) {
      const resolved = ts.resolveModuleName(specifier, `${VirtualRoot}/${from}`, option, host).resolvedModule;
      if (resolved && byPath.has(logical(resolved.resolvedFileName))) return logical(resolved.resolvedFileName);
    }
    const name = specifier.split("/").slice(0, specifier.startsWith("@") ? 2 : 1).join("/");
    if (specifier.startsWith("node:") || packages.has(name)) return undefined;
    throw new Error(`Production import cannot be resolved: ${from} -> ${specifier}.`);
  };
  for (const specifier of queue.splice(0)) {
    const path = resolve(entry.path, specifier.startsWith("/") || specifier.startsWith(".") ? specifier : `./${specifier}`);
    if (path) queue.push(path);
  }
  queue.push(...inline.map((file) => file.path));
  while (queue.length) {
    const path = queue.shift();
    if (!path || found.has(path)) continue;
    const file = byPath.get(path);
    if (!file) throw new Error("Production source is missing.");
    found.set(path, file);
    if (!/\.[cm]?[jt]sx?$/.test(path)) continue;
    for (const node of descendants(syntax(file))) {
      let specifier: string | undefined;
      if ((ts.isImportDeclaration(node) || ts.isExportDeclaration(node)) && node.moduleSpecifier && ts.isStringLiteral(node.moduleSpecifier)) {
        if (ts.isImportDeclaration(node) && (node.importClause?.isTypeOnly || node.importClause?.namedBindings && ts.isNamedImports(node.importClause.namedBindings) && !node.importClause.name && node.importClause.namedBindings.elements.length > 0 && node.importClause.namedBindings.elements.every((item) => item.isTypeOnly))) continue;
        if (ts.isExportDeclaration(node) && (node.isTypeOnly || node.exportClause && ts.isNamedExports(node.exportClause) && node.exportClause.elements.length > 0 && node.exportClause.elements.every((item) => item.isTypeOnly))) continue;
        specifier = node.moduleSpecifier.text;
      } else if (ts.isCallExpression(node) && node.expression.kind === ts.SyntaxKind.ImportKeyword && node.arguments[0] && ts.isStringLiteral(node.arguments[0])) specifier = node.arguments[0].text;
      if (specifier) { const resolved = resolve(path, specifier); if (resolved) queue.push(resolved); }
    }
  }
  return [...found.values()];
}
