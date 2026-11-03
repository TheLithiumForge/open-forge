import assert from "node:assert/strict";
import { test } from "node:test";
import { parseScript, unwrapShell } from "./shell.ts";

const words = (script: string) => parseScript(script).map((pipeline) => pipeline.map((stage) => stage.words));

test("a shell wrapper gives its script, in Codex's string form and as a command array", () => {
  assert.equal(unwrapShell("/bin/sh -lc 'cat AGENTS.md docs/README.md'"), "cat AGENTS.md docs/README.md");
  assert.equal(unwrapShell(["/bin/bash", "-lc", "npm test"]), "npm test");
  assert.equal(unwrapShell("rg --files"), "rg --files");
});

test("scripts split into pipelines of stages with quoted words kept whole", () => {
  assert.deepEqual(words(`git status --short && cat "a b.md" | head -n 5; ls`), [[["git", "status", "--short"]], [["cat", "a b.md"], ["head", "-n", "5"]], [["ls"]]]);
});

test("redirections leave the words, a file descriptor number goes with them and 2>&1 takes no target", () => {
  const [pipeline] = parseScript("npm test 2>/dev/null > out.log 2>&1 next");
  const stage = pipeline?.[0];
  assert.deepEqual(stage?.words, ["npm", "test", "next"]);
  assert.deepEqual(stage?.redirects.map((item) => item.target), ["/dev/null", "out.log", ""]);
});

test("heredoc bodies are skipped, so their text never reads as commands", () => {
  assert.deepEqual(words("cat > src/a.ts <<'EOF'\nrm -rf everything\nEOF\nnpm test"), [[["cat"]], [["npm", "test"]]]);
});
