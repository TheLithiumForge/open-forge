import assert from "node:assert/strict";
import { test } from "node:test";
import { passedTests } from "./test-summary.ts";
import { productionFiles } from "./production-graph.ts";
import { allowedRequest } from "./delivery-request.ts";
import { hoverChanges } from "./hover-style.ts";
import { checkMark, diamond, sameBacks } from "./drawn-shapes.ts";
import type { SourceFile } from "./source.ts";

test("test counts accept Vitest, Node and Playwright and reject empty summaries", () => {
  for (const summary of ["Tests  23 passed (23)", "# pass 23", "ℹ pass 23", "  23 passed (2.1s)", "\u001b[32mTests\u001b[39m  23 passed (23)"]) assert.equal(passedTests(summary), true, summary);
  for (const summary of ["Tests 0 passed (0)", "# pass 0", "ℹ pass 0", "0 passed", "Test Files 3 passed", "0 passing", ""]) assert.equal(passedTests(summary), false, summary);
});

test("production inclusion follows imports, reexports and cycles from the HTML entry", () => {
  const files: SourceFile[] = [
    { path: "index.html", text: '<script src="/src/main.tsx" type="module"></script>' },
    { path: "src/main.tsx", text: 'import "./board"; import type { Fixture } from "./unused.fixture.ts";' },
    { path: "src/board.ts", text: 'export { game } from "./game.ts"; import "./board.css";' },
    { path: "src/game.ts", text: 'import "./board.ts"; export const game = 1;' },
    { path: "src/board.css", text: "" },
    { path: "src/unused.fixture.ts", text: "export type Fixture = string;" },
  ];
  assert.deepEqual(productionFiles(files).map((file) => file.path), ["index.html", "src/main.tsx", "src/board.ts", "src/game.ts", "src/board.css"]);
  const imported = files.map((file) => file.path === "src/main.tsx" ? { ...file, text: 'import "./unused.fixture.ts";' } : file);
  assert.ok(productionFiles(imported).some((file) => file.path.endsWith(".fixture.ts")));
  assert.throws(() => productionFiles(files.slice(1)));
  assert.throws(() => productionFiles(files.filter((file) => file.path !== "src/game.ts")));
  const aliased = [...files, { path: "tsconfig.json", text: '{"compilerOptions":{"baseUrl":".","paths":{"@/*":["src/*"]}}}' }].map((file) => file.path === "src/main.tsx" ? { ...file, text: 'import "@/unused.fixture.ts";' } : file);
  assert.ok(productionFiles(aliased).some((file) => file.path.endsWith(".fixture.ts")));
  const inline = files.map((file) => file.path === "index.html" ? { ...file, text: '<script type="module">import "./src/unused.fixture.ts";</script>' } : file);
  assert.ok(productionFiles(inline).some((file) => file.path.endsWith(".fixture.ts")));
});

test("offline delivery allows only its main document and inline data", () => {
  const entry = "file:///project/dist/index.html";
  assert.equal(allowedRequest(entry, entry), true);
  for (const url of ["file:///project/dist/other.js", "file:///project/dist/index.html?other", "https://example.invalid/file", "http://127.0.0.1:4318/other"]) assert.equal(allowedRequest(url, entry), false);
  for (const url of ["data:image/svg+xml,svg", "blob:local"]) assert.equal(allowedRequest(url, entry), true);
});

test("hover permits border colors and catches transforms, geometry and shadows", () => {
  const before = { "border-color": "gray", "border-inline-start-color": "gray", transform: "none", width: "44px", "box-shadow": "none" };
  assert.deepEqual(hoverChanges(before, { ...before, "border-color": "white", "border-inline-start-color": "white" }), []);
  for (const [property, value] of [["transform", "matrix(1.1,0,0,1.1,0,0)"], ["width", "48px"], ["box-shadow", "0 0 4px red"]]) assert.deepEqual(hoverChanges(before, { ...before, [property ?? ""]: value ?? "" }), [property]);
});

test("drawn contours distinguish diamonds and checks from labels and unrelated paths", () => {
  const back = [{ x: 12, y: 0 }, { x: 24, y: 12 }, { x: 12, y: 24 }, { x: 0, y: 12 }, { x: 12, y: 0 }];
  const tick = [{ x: 2, y: 6 }, { x: 5, y: 9 }, { x: 10, y: 3 }];
  assert.equal(diamond(back), true);
  const neutral = { points: back, color: "#B8BDD2" };
  assert.equal(sameBacks([[neutral], [{ ...neutral, points: back.map((point) => ({ x: point.x + 44, y: point.y + 44 })) }]]), true);
  assert.equal(sameBacks([[neutral], [{ ...neutral, color: "#FF71D6" }]]), false);
  assert.equal(sameBacks([[neutral], [{ ...neutral, points: back.map((point) => ({ x: point.x * 2, y: point.y * 2 })) }]]), false);
  assert.equal(sameBacks([]), false);
  assert.equal(checkMark(tick), true);
  assert.equal(checkMark(tick.toReversed()), true);
  for (const points of [[], [{ x: NaN, y: 0 }], [{ x: 0, y: 0 }, { x: 1, y: 0 }, { x: 1, y: 1 }, { x: 0, y: 1 }]]) assert.equal(diamond(points), false);
  for (const points of [[], back, [{ x: 0, y: 0 }, { x: 1, y: 1 }, { x: 2, y: 0 }]]) assert.equal(checkMark(points), false);
});
