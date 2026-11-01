import assert from "node:assert/strict";
import { appendFile, mkdir, mkdtemp, readFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { MatrixJudges } from "../../config/matrix.ts";
import { temporaryHistory, v2Fixture, FixtureCheckId } from "../../history/test-fixtures.ts";
import { plannedRun } from "../../history/run-records.ts";
import { CheckStatus, FailureKind, RunStatus } from "../../history/statuses.ts";
import { ResultFiles } from "../../paths.ts";
import { writeNew } from "../../materialize/files.ts";
import { executeRun, type RunServices } from "../run.ts";
import { TurnOutcome } from "../providers/agent-session.ts";
import { ConversationFile } from "./answer-bank.ts";
import { converse } from "./conversation.ts";
import { classifyQuestions, ClassifierFormatRetries } from "./model-classifier.ts";
import { ConversationRole, type ConversationReport } from "./report.ts";
import { classification, Question, TestConfig, TestSuite, TestTelemetry } from "./test-fixtures.ts";

const FailedStages = ["setup", "classifier", "cleanup", "build", "checks", "judge"] as const;

test("failed attempts keep their conversation and event data from every stage", async (t) => {
  for (const stage of FailedStages) await t.test(stage, async (t) => {
    const history = await temporaryHistory(); t.after(history.cleanup);
    const original = v2Fixture(), config = { ...TestConfig, simulatedUser: MatrixJudges[0] };
    const batch = { ...original.batch, suite: TestSuite.id, config };
    const run = plannedRun(original.run.id, batch.id, original.run.setup, original.run.agent, original.run.repeat);
    await history.store.saveBatch(batch); await history.store.saveRun(run);
    const agentText = stage === "classifier" ? Question.sound : Question.done;
    const event = `${JSON.stringify({ text: agentText })}\n`;
    const rawReply = JSON.stringify({ questions: [{ text: Question.other, topicIds: [], documented: false }] });
    let classifierCalls = 0;
    const fail = (): never => { throw new Error(`Fixture ${stage} failure.`); };
    const services: RunServices = { store: history.store, stop: async () => {}, scratch: () => mkdtemp(join(history.root, "judge-")),
      createSpace: async (project) => {
        if (stage === "setup") fail();
        const root = join(history.root, "attempt"), work = join(root, project), home = join(root, "home");
        await mkdir(home, { recursive: true });
        await writeNew(work, "game.ts", "export const game = true;\n");
        return { root, work, home, project };
      },
      conversation: (context, suite, config) => converse(context, suite, config, {
        open: () => ({
          turn: async () => { await appendFile(context.eventsPath, event); return { outcome: TurnOutcome.answered, message: agentText, error: null }; },
          finish: async () => { if (stage === "cleanup") fail(); return TestTelemetry; },
        }),
        classify: stage === "classifier" ? (input) => classifyQuestions(input, async () => {
          classifierCalls += 1;
          return { code: 0, timedOut: false, stderr: "", stdout: JSON.stringify({ result: rawReply }) };
        }) : async (input) => classification(input.message),
      }),
      build: async () => { if (stage === "build") fail(); return { passed: true, typecheck: true, seconds: 0, packages: 0, html: null, bytes: null, log: "Fixture build." }; },
      checks: async () => { if (stage === "checks") fail(); return { checks: [{ id: FixtureCheckId, area: "play", status: CheckStatus.pass, evidence: "Fixture check." }], performance: null, shots: [] }; },
      judge: async () => { if (stage === "judge") fail(); return []; },
    };
    const result = await executeRun({ run, suite: TestSuite, config, snapshot: "", position: "fixture", services });
    assert.equal(result.status, RunStatus.failed);
    const folder = history.store.runFolder(batch.id, run.id);
    const report: ConversationReport = JSON.parse(await readFile(join(folder, ConversationFile), "utf8"));
    assert.equal(await readFile(join(folder, ResultFiles.events), "utf8"), stage === "setup" ? "" : event);
    assert.equal(report.messages?.findLast((message) => message.role === ConversationRole.agent)?.text, stage === "setup" ? undefined : agentText);
    const paths = (await history.store.readArtifacts(batch)).map((artifact) => artifact.path);
    assert.ok(paths.includes(ConversationFile) && paths.includes(ResultFiles.events));
    if (stage === "classifier") {
      assert.equal(result.failureKind, FailureKind.infrastructure);
      assert.equal(classifierCalls, ClassifierFormatRetries + 1);
      assert.equal(report.calls.at(-1)?.reply, rawReply);
      assert.ok(report.calls.at(-1)?.error);
      assert.equal(result.finalMessage, agentText);
    }
  });
});
