import * as ts from "typescript";
import { Evidence } from "./outcomes.ts";
import { reporter } from "./browser-play.ts";
import { authored, compilerOptions, descendants, jsonConfig, jsonObject, syntax, type SourceFile } from "./source.ts";
import { object } from "./requirements.ts";
import { productionFiles } from "./production-graph.ts";

export const Pins = [
  { clause: "T02.1", packages: ["react", "react-dom"], version: "19.3.1" }, { clause: "T02.2", packages: ["typescript"], version: "7.0.2" },
  { clause: "T02.3", packages: ["@types/react", "@types/react-dom"], version: "19.3.0" }, { clause: "T02.4", packages: ["vite"], version: "8.3.5" },
  { clause: "T02.5", packages: ["@vitejs/plugin-react"], version: "6.1.2" }, { clause: "T02.6", packages: ["vite-plugin-singlefile"], version: "2.3.4" },
  { clause: "T02.7", packages: ["vitest"], version: "5.0.5" }, { clause: "T02.8", packages: ["@playwright/test"], version: "1.64.1" },
  { clause: "T02.9", packages: ["@types/node"], version: "24.19.2" },
] as const;
export function dependencies(manifest: Record<string, unknown>): Record<string, unknown> {
  return { ...(object(manifest["dependencies"]) ? manifest["dependencies"] : {}), ...(object(manifest["devDependencies"]) ? manifest["devDependencies"] : {}) };
}
export function staticChecks(files: readonly SourceFile[], evidence: Evidence, full: boolean): void {
  const put = reporter(evidence), manifest = jsonObject(files.find((file) => file.path === "package.json")?.text ?? "{}"), deps = dependencies(manifest);
  const sources = files.filter(authored), nodes = sources.flatMap((file) => descendants(syntax(file))), source = sources.map((file) => file.text).join("\n");
  put("T01.1", typeof deps["react"] === "string" && typeof deps["react-dom"] === "string" && sources.length > 0 && /react-dom(?:\/client)?/.test(source), "Read dependency entries, authored TypeScript and DOM mounting imports.");
  put("T01.2", /<button\b/.test(source) && /<select\b/.test(source), "Authored JSX renders native buttons and select.");
  put("S02.3", !/<(?:button|input|select)[^>]*(?:sound|audio|mute)|(?:aria-label|title)\s*=\s*["'][^"']*(?:sound|mute)/i.test(source), "Authored controls contain no sound toggle.");
  if (!full) return;
  for (const pin of Pins) put(pin.clause, pin.packages.every((name) => deps[name] === pin.version), `Exact dependency pins: ${pin.packages.join(", ")} ${pin.version}.`);
  const lock = files.find((file) => file.path === "package-lock.json");
  put("T02.10", lock !== undefined && object(jsonObject(lock.text)["packages"]), "The package lock is present and has package entries.");
  const engines = manifest["engines"];
  put("T03.1", object(engines) && typeof engines["node"] === "string" && /(?:>=|\^)\s*(?:2[5-9]|[3-9]\d|24\.(?:19|[2-9]\d))\b/.test(engines["node"]) && lock !== undefined, "Read the Node minimum and npm lockfile.");
  const configs = files.filter((file) => /(?:^|\/)tsconfig[^/]*\.json$/.test(file.path));
  const projects = configs.filter((file) => { const config = jsonConfig(file.text); return config["include"] !== undefined || config["files"] !== undefined; });
  const options = projects.map((file) => compilerOptions(files, file.path));
  const browserLibrary = (option: Record<string, unknown>): boolean => JSON.stringify(option["lib"] ?? []).toLowerCase().includes('"dom"');
  put("T03.2", projects.length >= 2 && options.some(browserLibrary) && options.some((option) => JSON.stringify(option["types"] ?? []).includes("node") && !browserLibrary(option)), "Resolved browser and Node project compiler options separately.");
  put("T04.1", options.length >= 2 && options.every((option) => ["strict", "noUncheckedIndexedAccess", "exactOptionalPropertyTypes", "verbatimModuleSyntax", "erasableSyntaxOnly"].every((name) => option[name] === true)), "Every included compiler project inherits the five required strict flags.");
  const scripts = object(manifest["scripts"]) ? manifest["scripts"] : {};
  put("T04.2", typeof scripts["typecheck"] === "string" && /tsc|tsgo/.test(scripts["typecheck"]) && /vite\s+build/.test(String(scripts["build"])), "Type checking has a separate command from Vite bundling.");
  put("T03.3", manifest["type"] === "module" && !nodes.some((node) => ts.isCallExpression(node) && (node.expression.kind === ts.SyntaxKind.ImportKeyword || ts.isIdentifier(node.expression) && node.expression.text === "require")) && options.every((option) => option["verbatimModuleSyntax"] === true), "ESM package, static imports and compiler-enforced type-only imports.");
  put("T06.3", files.filter((file) => /\.tsx?$/.test(file.path)).every((file) => file.text.replace(/\r\n/g, "\n").replace(/\n$/, "").split("\n").length <= 200), "Counted lines in every authored TypeScript file including tests and tools.");
  put("T01.3", files.some((file) => /\.css$/.test(file.path) && /display\s*:\s*grid\b/.test(file.text)), "Authored CSS declares grid layout.");
  put("S02.1", !files.some((file) => /\.(?:mp3|wav|ogg|m4a|aac|flac|webm)$/i.test(file.path)), "Source tree has no audio asset files.");
  put("S02.2", !nodes.some((node) => (ts.isCallExpression(node) || ts.isNewExpression(node)) && /(?:^|\.)(?:Audio|AudioContext|webkitAudioContext|createOscillator|play)$/.test(node.expression.getText())), "AST contains no audio constructors or playback API calls.");
  const shipped = productionFiles(files);
  put("A11.1", !shipped.some((file) => /(?:fixture|diagnostic|solver|test-helper)/i.test(file.path) || /__test|window\.(?:deck|solver|gameState)/i.test(file.text)), "The production import graph excludes fixtures, diagnostics and test globals.");
  put("A11.2", !files.filter((file) => /\.test\./.test(file.path)).some((file) => /window\.(?:deck|solver|gameState)/i.test(file.text)), "Tests use no private board or solver global.");
  put("A11.3", !/<(?:button|input|textarea)[^>]*(?:solver|debug|replay)/i.test(source), "Authored controls expose no replay, solver or debug controls.");
}
