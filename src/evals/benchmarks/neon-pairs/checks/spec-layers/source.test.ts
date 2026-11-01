import assert from "node:assert/strict";
import { mkdtemp, mkdir, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import * as ts from "typescript";
import { authored, compilerOptions, descendants, jsonConfig, jsonObject, sourceFiles, SourceExcluded, syntax } from "./source.ts";
import { formula, numeric } from "./numeric.ts";
import { dependencies, Pins, staticChecks } from "./static-checks.ts";
import { Evidence } from "./outcomes.ts";
import { metric } from "./browser-session.ts";
import { rowOrder } from "./browser-cards.ts";
import { removeTemporary, temporaryPath } from "./temporary.ts";
import { Rules } from "./rules.ts";

test("source parsers validate JSON, JSONC, inherited options and cycles", () => {
  assert.deepEqual(jsonObject('{"a":1}'), { a: 1 });
  assert.throws(() => jsonObject("[]"));
  assert.deepEqual(jsonConfig('{ // comment\n "compilerOptions": {"strict":true,},}'), { compilerOptions: { strict: true } });
  assert.throws(() => jsonConfig("{"));
  const files = [{ path: "tsconfig.base.json", text: '{"compilerOptions":{"strict":true}}' }, { path: "tsconfig.app.json", text: '{"extends":"./tsconfig.base.json","compilerOptions":{"noEmit":true}}' }];
  assert.deepEqual(compilerOptions(files, "tsconfig.app.json"), { strict: true, noEmit: true });
  assert.throws(() => compilerOptions([{ path: "a.json", text: '{"extends":"./a.json"}' }], "a.json"));
  assert.throws(() => compilerOptions([{ path: "a.json", text: '{"extends":"../outside.json"}' }], "a.json"));
  assert.equal(authored({ path: "src/game/game.test.ts", text: "" }), false);
  assert.equal(authored({ path: "src/game/game.ts", text: "" }), true);
});

test("static numeric evaluation accepts supported arithmetic without executing candidate code", () => {
  const file = { path: "face.ts", text: "const SHAPES = ['a','b','c','d','e','f','g','h'] as const; const value = Math.floor(id / SHAPES.length) % 8;" };
  assert.equal(formula([file], (id) => Math.floor(id / 8) % 8), "Math.floor(id / SHAPES.length) % 8");
  assert.equal(formula([{ ...file, text: file.text.replace("% 8", "% 7") }], (id) => Math.floor(id / 8) % 8), null);
  const tree = syntax({ path: "x.ts", text: "const x = 5 * (7 - 2) + 1; const y = unknown();" });
  const expressions = descendants(tree).filter(ts.isVariableDeclaration).flatMap((node) => node.initializer ? [node.initializer] : []);
  assert.equal(numeric(expressions[0] ?? tree, new Map()), 26);
  assert.equal(numeric(expressions[1] ?? tree, new Map()), null);
});

test("static package checks keep exact pins and classify malformed source as checker input errors", () => {
  assert.deepEqual(dependencies({ dependencies: { react: "a" }, devDependencies: { vite: "b" } }), { react: "a", vite: "b" });
  const deps = Object.fromEntries(Pins.flatMap((pin) => pin.packages.map((name) => [name, pin.version])));
  const files = [{ path: "package.json", text: JSON.stringify({ type: "module", dependencies: deps }) }, { path: "src/Game.tsx", text: 'import {createRoot} from "react-dom/client"; export const Game = () => <><button>New Game</button><select /></>;' }];
  const evidence = new Evidence(); staticChecks(files, evidence, false);
  assert.equal(evidence.outcomes.find((item) => item.clause === "T01.1")?.status, "pass");
  const full = new Evidence(); staticChecks(files, full, true);
  for (const pin of Pins) assert.equal(full.outcomes.find((item) => item.clause === pin.clause)?.status, "pass");
  assert.equal(full.outcomes.find((item) => item.clause === "T04.1")?.status, "fail");
});

test("source collection skips excluded folders without loading their code", async (t) => {
  const root = await mkdtemp(join(tmpdir(), "eval-source-reader-"));
  t.after(() => removeTemporary(root, "eval-source-reader-"));
  await writeFile(join(root, "package.json"), "{}");
  for (const folder of SourceExcluded) { await mkdir(join(root, folder)); await writeFile(join(root, folder, "ignored.ts"), "throw new Error('must never execute');"); }
  assert.deepEqual((await sourceFiles(root)).map((file) => file.path), ["package.json"]);
});

test("row-order comparison uses visual positions and cleanup stays inside its named temporary folder", () => {
  assert.deepEqual(rowOrder([{ x: 50, y: 0, index: 0 }, { x: 0, y: 50, index: 1 }, { x: 0, y: 0.1, index: 2 }]), [2, 0, 1]);
  assert.throws(() => rowOrder([{ x: NaN, y: 0, index: 0 }]));
  assert.throws(() => temporaryPath(tmpdir(), "eval-test-"));
  assert.throws(() => temporaryPath(join(tmpdir(), "different-name"), "eval-test-"));
  assert.equal(temporaryPath(join(tmpdir(), "eval-test-123"), "eval-test-"), join(tmpdir(), "eval-test-123"));
});

test("status text parser separates moves from best and reads named values", () => {
  const text = "Best moves\n8\nMoves\n2\nPairs\n1 / 8\nActive time\n3s";
  assert.equal(metric(text, "moves"), 2);
  assert.equal(metric(text, "best"), 8);
  assert.equal(metric(text, "pairs"), 1);
  assert.equal(metric(text, "time"), 3);
  assert.equal(metric("Active time\n1:02:03", "time"), 3723);
  assert.equal(metric("Active time\n5999:59", "time"), Math.floor(Rules.elapsedMaximumMs / 1000));
  assert.equal(metric("Moves: none", "moves"), null);
});
