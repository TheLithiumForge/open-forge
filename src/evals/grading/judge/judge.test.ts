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
import { JudgeImageManifestLabel, judgeRun, retryPrompt, type JudgeAssessor } from "./judge.ts";
import { containerFixture } from "../../execution/containers/test-docker.ts";
import { ContainerPaths } from "../../execution/containers/settings.ts";
import { AssessorImageLimits } from "../../execution/containers/files.ts";
import { AssessorImagePrefix } from "../../execution/containers/assessor.ts";
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
    return { code: 0, timedOut: false, stderr: "", stdout: _choice.tool === Tool.codex
      ? JSON.stringify({ type: "item.completed", item: { type: "agent_message", text: JSON.stringify({ answers }) } })
      : JSON.stringify({ structured_output: { answers } }) };
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
  const lines = evidence.sources.find((source) => source.path === SourcePath)?.lines;
  const overshoot = parseBallot({ answers: [{ ...first, citations: [`source:${SourcePath}:1-9999`] }, ...answers.slice(1)] }, questions, evidence);
  assert.deepEqual(overshoot[0]?.citations, [`source:${SourcePath}:1-${lines}`]);
  const noImages: JudgeAssessor = { supportsImages: false, run: async () => { throw new Error("Unreachable assessor."); } };
  await assert.rejects(judgeRun(suite, choice, folder, join(root, "home"), 1000, { assessor: noImages }));
  const failed: JudgeAssessor = { supportsImages: true, run: async () => ({ code: 1, timedOut: false, stdout: "", stderr: "assessor failed" }) };
  await assert.rejects(judgeRun(suite, choice, folder, join(root, "home"), 1000, { assessor: failed }));
});

test("a rejected reply is retried with the reason it was rejected", async (t) => {
  const root = await temporary(t);
  const folder = join(root, "judge");
  await prepareJudgeFolder(await application(root, "source"), folder);
  const evidence = await judgeEvidence(folder);
  const questions = await frozenQuestions();
  const answers = questions.map((question) => ({ id: question.id, answer: JudgeVerdict.unsure, evidence: "Uncertain", citations: [`source:${SourcePath}:1`] }));
  const invalid = { answers: answers.map((answer) => ({ ...answer, citations: [`source:${SourcePath}:9999`] })) };
  let rejection = "";
  try { parseBallot(invalid, questions, evidence); } catch (error) { rejection = error instanceof Error ? error.message : String(error); }
  assert.ok(rejection);
  const prompts: string[] = [];
  const assessor: JudgeAssessor = { supportsImages: true, run: async (_space, _choice, prompt) => {
    prompts.push(prompt);
    return { code: 0, timedOut: false, stderr: "", stdout: JSON.stringify({ structured_output: prompts.length === 1 ? invalid : { answers } }) };
  } };
  const judged = await judgeRun(suite, choice, folder, join(root, "home"), 1000, { assessor });
  assert.equal(judged.length, questions.length);
  assert.equal(prompts.length, BallotCount + 1);
  assert.equal(prompts[1], retryPrompt(prompts[0] ?? "", rejection));
  assert.ok(prompts.slice(2).every((prompt) => prompt === prompts[0]));
});

test("the container adapter sends identical board and face sheet images and records their manifest", async () => {
  const questions = await frozenQuestions();
  const received: Buffer[][] = [];
  for (const tool of [Tool.claude, Tool.codex]) {
    const fixture = await containerFixture();
    try {
      await mkdir(join(fixture.space.work, "src"));
      await writeFile(join(fixture.space.work, SourcePath), "export const pairCount = 8;\n");
      const names = [ImageFile, "faces.png"];
      const bytes = names.map((_, index) => PNG.sync.write(new PNG({ width: 4 + index, height: 4 })));
      const images: string[] = [];
      for (const [index, name] of names.entries()) {
        const path = join(fixture.space.root, name);
        await writeFile(path, bytes[index] ?? Buffer.alloc(0));
        images.push(path);
      }
      const answers = questions.map((question) => ({ id: question.id, answer: JudgeVerdict.yes, evidence: "Fixture evidence", citations: [ImageFile, `source:${SourcePath}:1`] }));
      fixture.client.spawnOutput = tool === Tool.codex
        ? JSON.stringify({ type: "item.completed", item: { type: "agent_message", text: JSON.stringify({ answers }) } })
        : JSON.stringify({ result: JSON.stringify({ answers }) });
      const judged = await judgeRun(suite, { ...choice, tool }, fixture.space.work, fixture.space.home, 10_000, { images, stage: { docker: fixture.client, env: {} } });
      const bundle = await judgeEvidence(fixture.space.work, images);
      const manifest = judged[0]?.evidence.at(-1);
      assert.ok(manifest);
      assert.deepEqual(JSON.parse(manifest.slice(`${JudgeImageManifestLabel}: `.length)), bundle.images.map(({ id, sha256, png, width, height }) => ({ id, sha256, bytes: png.length, width, height })));
      assert.equal(fixture.client.names.length, BallotCount);
      const delivered: Buffer[] = [];
      if (tool === Tool.claude) {
        for (const input of fixture.client.inputs) {
          const message: { message: { content: { type: string; source?: { data: string } }[] } } = JSON.parse(input);
          const attachments = message.message.content.filter((block) => block.type === "image").map((block) => Buffer.from(block.source?.data ?? "", "base64"));
          assert.deepEqual(attachments, bytes);
          delivered.push(...attachments);
        }
      } else for (const name of fixture.client.names) {
        const attachments = await Promise.all(bytes.map((_, index) => readFile(fixture.client.path(name, `${ContainerPaths.work}/${AssessorImagePrefix}${index}.png`))));
        assert.deepEqual(attachments, bytes);
        delivered.push(...attachments);
      }
      received.push(delivered);
    } finally { await fixture.dispose(); }
  }
  assert.deepEqual(received[0], received[1]);
});

test("invalid judge image input fails before any assessor call", async (t) => {
  const root = await temporary(t);
  const folder = await application(root, "source");
  const path = join(root, ImageFile);
  await writeFile(path, "invalid image");
  let calls = 0;
  const assessor: JudgeAssessor = { supportsImages: true, run: async () => { calls += 1; throw new Error("Unreachable assessor."); } };
  for (const images of [[path], Array.from({ length: AssessorImageLimits.count + 1 }, () => path)]) {
    await assert.rejects(judgeRun(suite, choice, folder, join(root, "home"), 1000, { images, assessor }));
  }
  assert.equal(calls, 0);
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
