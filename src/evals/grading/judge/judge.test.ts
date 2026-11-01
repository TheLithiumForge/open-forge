import assert from "node:assert/strict";
import { mkdir, mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { test, type TestContext } from "node:test";
import { PNG } from "pngjs";
import { Tool } from "../../config/choices.ts";
import type { Suite } from "../../benchmarks/suite.types.ts";
import { judgeEvidence, type JudgeEvidence } from "./evidence.ts";
import { prepareJudgeFolder } from "./judge-folder.ts";
import { judgeRun, type JudgeAssessor } from "./judge.ts";
import { parseBallot } from "./ballots.ts";
import { BallotCount, frozenQuestions, JudgeEvidenceKind, JudgeVerdict } from "./rubric.ts";

const SourcePath = "src/game.ts";
const ImageFile = "board.png";
const suite: Suite = { id: "greenfield-baseline", title: "Neon Pairs", project: "neon-pairs", baseline: "no-setup", setups: {}, prompt: "Build Neon Pairs.", intent: [], rubric: [], fingerprint: "fixture", intentEdited: false };
const choice = { tool: Tool.claude, model: "pinned-judge", effort: "high", repeats: 1 } as const;

async function temporary(t: TestContext): Promise<string> {
  const root = await mkdtemp(join(tmpdir(), "eval-judge-"));
  t.after(async () => {
    if (dirname(resolve(root)) !== resolve(tmpdir())) throw new Error("Judge test cleanup escaped its temporary folder.");
    await rm(root, { recursive: true, force: true });
  });
  return root;
}

async function application(root: string, name: string): Promise<string> {
  const folder = join(root, name);
  await mkdir(join(folder, "src"), { recursive: true });
  await writeFile(join(folder, SourcePath), "export const pairCount = 8;\n");
  await writeFile(join(folder, "package.json"), JSON.stringify({ name, author: name, type: "module", dependencies: { react: "pinned", "@thelithiumforge/open-forge": name }, scripts: { build: "vite build", setup: `open-forge install ${name}` } }));
  for (const path of [".agents/loader.md", "dist/index.html", "test-results/report.json", "src/game.test.ts", "AGENTS.md", ".env.local"]) {
    await mkdir(dirname(join(folder, path)), { recursive: true });
    await writeFile(join(folder, path), name);
  }
  return folder;
}

test("both providers get identical filtered source and image bytes with three complete ballots", async (t) => {
  const root = await temporary(t);
  const images = [join(root, ImageFile)];
  await writeFile(images[0] ?? "", PNG.sync.write(new PNG({ width: 4, height: 4 })));
  const prompts: string[] = [];
  const bundles: JudgeEvidence[] = [];
  const questions = await frozenQuestions();
  const assessor: JudgeAssessor = { supportsImages: true, run: async (_space, _choice, prompt, _options, evidence) => {
    prompts.push(prompt); bundles.push(evidence);
    const answers = questions.map((question) => ({ id: question.id, answer: JudgeVerdict.yes, evidence: "Fixture evidence", citations: question.evidence === JudgeEvidenceKind.source ? [`source:${SourcePath}:1`] : [ImageFile, `source:${SourcePath}:1`] }));
    return { code: 0, timedOut: false, stderr: "", stdout: JSON.stringify({ structured_output: { answers } }) };
  } };
  for (const tool of [Tool.claude, Tool.codex]) {
    const source = await application(root, tool);
    const folder = join(root, `${tool}-judge`);
    await prepareJudgeFolder(source, folder);
    const judged = await judgeRun(suite, { ...choice, tool }, folder, join(root, `${tool}-home`), 1000, { assessor, images });
    assert.equal(judged.length, questions.length);
    assert.ok(judged.every((answer) => answer.yes === BallotCount && answer.no === 0 && answer.unsure === 0));
  }
  assert.equal(prompts.length, BallotCount * 2);
  assert.ok(prompts.every((prompt) => prompt === prompts[0]));
  assert.deepEqual(bundles[0], bundles[BallotCount]);
  const filtered = await readFile(join(root, `${Tool.claude}-judge`, "package.json"), "utf8");
  const manifest: unknown = JSON.parse(filtered);
  assert.deepEqual(manifest, { type: "module", dependencies: { react: "pinned" }, scripts: { build: "vite build" } });
});

test("malformed ballots, invented citations and a missing image transport are judge failures", async (t) => {
  const root = await temporary(t);
  const source = await application(root, "source");
  const folder = join(root, "judge");
  await prepareJudgeFolder(source, folder);
  const evidence = await judgeEvidence(folder);
  const questions = await frozenQuestions();
  const answers = questions.map((question) => ({ id: question.id, answer: JudgeVerdict.unsure, evidence: "Uncertain", citations: [`source:${SourcePath}:1`] }));
  assert.equal(parseBallot({ answers }, questions, evidence).length, questions.length);
  assert.throws(() => parseBallot({ answers: answers.slice(1) }, questions, evidence));
  const first = answers[0];
  assert.ok(first);
  for (const changes of [{ answer: "maybe" }, { citations: [`source:${SourcePath}:9999`] }, { evidence: 1 }]) assert.throws(() => parseBallot({ answers: [{ ...first, ...changes }, ...answers.slice(1)] }, questions, evidence));
  await assert.rejects(judgeRun(suite, choice, folder, join(root, "home"), 1000));
  const failed: JudgeAssessor = { supportsImages: true, run: async () => ({ code: 1, timedOut: false, stdout: "", stderr: "assessor failed" }) };
  await assert.rejects(judgeRun(suite, choice, folder, join(root, "home"), 1000, { assessor: failed }));
});

test("usable partial source survives a malformed manifest while generated HTML stays out", async (t) => {
  const root = await temporary(t);
  const source = await application(root, "partial");
  const generated = "built.html";
  await writeFile(join(source, generated), "<html>Generated output</html>");
  await writeFile(join(source, "package.json"), "{");
  const folder = join(root, "judge");
  await prepareJudgeFolder(source, folder, generated);
  const evidence = await judgeEvidence(folder);
  assert.equal(evidence.sources.some((file) => file.path === generated), false);
  assert.ok(evidence.sources.some((file) => file.path === SourcePath));
});
