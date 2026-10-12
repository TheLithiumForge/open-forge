import assert from "node:assert/strict";
import { test } from "node:test";
import { elapsedSeconds, jobName, median, readTimedJobs, timingOutliers, timingReport, timingRows, WholeJob, type TimedJob } from "../timings-model.ts";

const Start = "2026-10-11T18:00:00Z";
const at = (seconds: number) => new Date(Date.parse(Start) + seconds * 1_000).toISOString();

function job(testSeconds: number, conclusion = "success"): unknown {
  return {
    name: "build (linux-arm64, ubuntu-24.04-arm, arm64)",
    conclusion,
    run_attempt: 1,
    started_at: Start,
    completed_at: at(600 + testSeconds),
    steps: [
      { name: "Set up job", conclusion: "success", started_at: Start, completed_at: at(1) },
      { name: "Build native artifacts", conclusion: "success", started_at: at(1), completed_at: at(600) },
      { name: "Test managed and native suites", conclusion, started_at: at(600), completed_at: at(600 + testSeconds) },
    ],
  };
}

function jobs(...entries: readonly [number, unknown][]): TimedJob[] {
  return entries.flatMap(([run, entry]) => readTimedJobs({ jobs: [entry] }, run));
}

test("timed jobs keep RID names, attempts and step durations", () => {
  const [read] = jobs([1, job(300)]);
  assert.equal(read?.job, "build linux-arm64");
  assert.equal(read?.seconds, 900);
  assert.deepEqual(
    read?.steps.map((step) => step.seconds),
    [1, 599, 300],
  );
  assert.equal(jobName("checks"), "checks");
  assert.equal(jobName("build (win-x64)"), "build win-x64");
  assert.equal(elapsedSeconds(Start, null), undefined);
  assert.equal(elapsedSeconds(at(5), Start), undefined);
  assert.throws(() => readTimedJobs({}, 1));
});

test("medians use only successful steps that take real time", () => {
  assert.equal(median([3, 1, 2]), 2);
  assert.equal(median([4, 1, 3, 2]), 2.5);
  assert.throws(() => median([]));
  const rows = timingRows(jobs([1, job(300)], [2, job(360)], [3, job(3_000, "cancelled")]));
  assert.deepEqual(
    rows.map((row) => [row.step, row.samples, row.median, row.max]),
    [
      [WholeJob, 2, 930, 960],
      ["Build native artifacts", 3, 599, 599],
      ["Test managed and native suites", 2, 330, 360],
    ],
  );
});

test("outliers list unsuccessful steps and steps over twice their usual time", () => {
  const timed = jobs([1, job(300)], [2, job(300)], [3, job(300)], [4, job(900)], [5, job(3_000, "cancelled")]);
  const rows = timingRows(timed);
  const outliers = timingOutliers(timed, rows);
  assert.deepEqual(
    outliers.map((item) => [item.run, item.step, item.conclusion]),
    [
      [4, "Test managed and native suites", "success"],
      [5, WholeJob, "cancelled"],
      [5, "Test managed and native suites", "cancelled"],
    ],
  );
  const quick = timed[0] ?? assert.fail("missing job");
  const suddenlySlow = { ...quick, run: 9, steps: [{ name: "Set up job", conclusion: "success", seconds: 120 }] };
  const withSlowSetup = [...timed, suddenlySlow];
  assert.ok(timingOutliers(withSlowSetup, timingRows(withSlowSetup)).some((item) => item.run === 9 && item.step === "Set up job"));
  const report = timingReport(5, rows, outliers);
  assert.match(report, /across 5 completed runs/u);
  assert.match(report, /Test managed and native suites\s+4\s+5\.0m\s+15\.0m/u);
  assert.match(report, /5\s+build linux-arm64\s+Test managed and native suites\s+50\.0m\s+5\.0m\s+cancelled/u);
  assert.match(timingReport(1, [], []), /None\./u);
});
