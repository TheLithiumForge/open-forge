import assert from "node:assert/strict";
import { test } from "node:test";
import { ContextClass, ToolKind } from "./classes.ts";
import { commandFacts } from "./command-facts.ts";
import { fileClass, fileType, projectPath } from "./paths.ts";

const Files = { spec: "docs/rules.md", scoped: "docs/gameplay/rules.md", entrypoint: "docs/gameplay/_gameplay.md", agents: "AGENTS.md",
  loader: ".agents/loader.md", code: "src/game/rules.ts", test: "src/game/rules.test.ts", e2e: "tests/play.e2e.test.ts" } as const;

test("project paths drop the work folder and resolve against the current folder", () => {
  assert.equal(projectPath(`/work/${Files.code}`), Files.code);
  assert.equal(projectPath("./rules.ts", "src/game"), Files.code);
  assert.equal(projectPath("/usr/lib/node"), "/usr/lib/node");
});

test("files sort into spec documents, framework files, tests and code by path", () => {
  assert.equal(fileClass(Files.spec), ContextClass.spec);
  assert.equal(fileClass(Files.scoped), ContextClass.spec);
  assert.equal(fileClass(Files.entrypoint), ContextClass.framework);
  assert.equal(fileClass(Files.agents), ContextClass.framework);
  assert.equal(fileClass(Files.loader), ContextClass.framework);
  assert.equal(fileClass(Files.test), ContextClass.tests);
  assert.equal(fileClass(Files.e2e), ContextClass.tests);
  assert.equal(fileClass(Files.code), ContextClass.code);
  assert.equal(fileType(Files.code), "ts");
  assert.equal(fileType("Makefile"), null);
});

test("a multi-file read names every file it printed, each with its own class", () => {
  const facts = commandFacts(`/bin/sh -lc 'cat ${Files.agents} ${Files.spec} ${Files.code}'`);
  assert.equal(facts.kind, ToolKind.read);
  assert.deepEqual(facts.sources.map((source) => source.class), [ContextClass.framework, ContextClass.spec, ContextClass.code]);
  assert.deepEqual(facts.reads, [Files.agents, Files.spec, Files.code]);
});

test("patterns and options are not files, and a pipeline's output comes from its first stage", () => {
  assert.deepEqual(commandFacts(`sed -n '1,80p' ${Files.code}`).reads, [Files.code]);
  assert.deepEqual(commandFacts(`nl -ba ${Files.code} | sed -n '1,40p'`).sources.map((source) => source.path), [Files.code]);
  const search = commandFacts(`rg -n "bestScore" src -g '*.ts'`);
  assert.equal(search.kind, ToolKind.search);
  assert.equal(search.search, true);
  assert.deepEqual(search.reads, []);
});

test("test and build runs, setup commands, framework reads and edits each get their kind", () => {
  assert.equal(commandFacts("npm test -- src/game").kind, ToolKind.run);
  assert.equal(commandFacts("npm run typecheck").kind, ToolKind.run);
  assert.equal(commandFacts("npx playwright test").kind, ToolKind.run);
  assert.equal(commandFacts("npx playwright install chromium").kind, ToolKind.other);
  assert.equal(commandFacts("npm ci").kind, ToolKind.other);
  assert.equal(commandFacts("npx open-forge context").sources[0]?.class, ContextClass.framework);
  assert.deepEqual(commandFacts(`cat > ${Files.code} <<'EOF'\nexport {}\nEOF`).writes, [Files.code]);
  assert.equal(commandFacts(`apply_patch <<'EOF'\n*** Begin Patch\nEOF`).kind, ToolKind.edit);
  assert.equal(commandFacts("npm test 2>&1 | tee out.log").kind, ToolKind.edit);
  assert.equal(commandFacts("npm test > /dev/null").kind, ToolKind.run);
});

test("cd moves where later relative paths point", () => {
  assert.deepEqual(commandFacts("cd src/game && cat rules.ts").reads, [Files.code]);
});
