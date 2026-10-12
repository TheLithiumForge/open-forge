import assert from "node:assert/strict";

export const TimingConclusion = { success: "success", skipped: "skipped" } as const;
export const TimingDefaults = {
  runs: 10,
  maximumRuns: 50,
  // Steps faster than this in every run are setup noise, not delivery cost.
  minimumSeconds: 30,
  // A step this many times slower than its usual time is listed as an outlier.
  outlierFactor: 2,
} as const;
const SecondsPerMinute = 60;
const MillisecondsPerSecond = 1_000;

export interface TimedStep {
  name: string;
  conclusion: string | null;
  seconds: number | undefined;
}
export interface TimedJob {
  run: number;
  attempt: number;
  job: string;
  conclusion: string | null;
  seconds: number | undefined;
  steps: TimedStep[];
}
export interface TimingRow {
  job: string;
  step: string;
  samples: number;
  median: number;
  max: number;
}
export interface TimingOutlier {
  run: number;
  attempt: number;
  job: string;
  step: string;
  seconds: number;
  conclusion: string | null;
  median: number | undefined;
}

/** The job total uses this step name, so jobs and steps share one table. */
export const WholeJob = "(whole job)";

function record(value: unknown): Record<string, unknown> {
  assert.ok(typeof value === "object" && value !== null && !Array.isArray(value), "Invalid GitHub response.");
  return Object.fromEntries(Object.entries(value));
}

/** Seconds between two GitHub timestamps, or undefined while either is missing. */
export function elapsedSeconds(started: unknown, completed: unknown): number | undefined {
  if (typeof started !== "string" || typeof completed !== "string") return undefined;
  const seconds = (Date.parse(completed) - Date.parse(started)) / MillisecondsPerSecond;
  return Number.isFinite(seconds) && seconds >= 0 ? seconds : undefined;
}

/** Names a matrix job by its RID, so runner image changes do not split its history. */
export function jobName(name: string): string {
  const matrix = /^(?<job>[^ (]+) \((?<rid>[^,)]+)[,)]/u.exec(name);
  return matrix?.groups ? `${matrix.groups["job"]} ${matrix.groups["rid"]}` : name;
}

function readStep(value: unknown): TimedStep {
  const step = record(value);
  assert.ok(typeof step["name"] === "string", "Invalid GitHub step.");
  const conclusion = step["conclusion"];
  assert.ok(conclusion === null || typeof conclusion === "string", "Invalid GitHub step conclusion.");
  return { name: step["name"], conclusion, seconds: elapsedSeconds(step["started_at"], step["completed_at"]) };
}

/** Reads a jobs response, keeping every attempt and each step's duration. */
export function readTimedJobs(value: unknown, run: number): TimedJob[] {
  const jobs = record(value)["jobs"];
  assert.ok(Array.isArray(jobs), "Missing GitHub jobs.");
  return jobs.map((entry: unknown) => {
    const job = record(entry);
    assert.ok(typeof job["name"] === "string", "Invalid GitHub job.");
    const conclusion = job["conclusion"];
    assert.ok(conclusion === null || typeof conclusion === "string", "Invalid GitHub job conclusion.");
    const steps = job["steps"] ?? [];
    assert.ok(Array.isArray(steps), "Invalid GitHub job steps.");
    const attempt = typeof job["run_attempt"] === "number" ? job["run_attempt"] : 1;
    return { run, attempt, job: jobName(job["name"]), conclusion, seconds: elapsedSeconds(job["started_at"], job["completed_at"]), steps: steps.map(readStep) };
  });
}

export function median(values: readonly number[]): number {
  assert.ok(values.length > 0, "A median needs at least one value.");
  const sorted = values.toSorted((left, right) => left - right);
  const middle = Math.floor(sorted.length / 2);
  return sorted.length % 2 ? (sorted[middle] ?? 0) : ((sorted[middle - 1] ?? 0) + (sorted[middle] ?? 0)) / 2;
}

/** Every measured step of a job that ran, with the job total first. */
function measured(job: TimedJob): TimedStep[] {
  if (job.conclusion === TimingConclusion.skipped) return [];
  return [{ name: WholeJob, conclusion: job.conclusion, seconds: job.seconds }, ...job.steps].filter((step) => step.seconds !== undefined);
}

function key(job: string, step: string): string {
  return `${job}\u0000${step}`;
}

/** Summarizes successful step durations per job, keeping steps that usually take real time. */
export function timingRows(jobs: readonly TimedJob[], minimumSeconds: number = TimingDefaults.minimumSeconds): TimingRow[] {
  const samples = new Map<string, { job: string; step: string; seconds: number[] }>();
  for (const job of jobs)
    for (const step of measured(job).filter((current) => current.conclusion === TimingConclusion.success)) {
      const entry = samples.get(key(job.job, step.name)) ?? { job: job.job, step: step.name, seconds: [] };
      entry.seconds.push(step.seconds ?? 0);
      samples.set(key(job.job, step.name), entry);
    }
  return [...samples.values()]
    .map((entry) => ({ job: entry.job, step: entry.step, samples: entry.seconds.length, median: median(entry.seconds), max: Math.max(...entry.seconds) }))
    .filter((row) => row.step === WholeJob || row.max >= minimumSeconds)
    .toSorted((left, right) => left.job.localeCompare(right.job) || Number(right.step === WholeJob) - Number(left.step === WholeJob));
}

/** Lists unsuccessful steps and steps far slower than their usual successful time. */
export function timingOutliers(jobs: readonly TimedJob[], rows: readonly TimingRow[], factor: number = TimingDefaults.outlierFactor): TimingOutlier[] {
  const usual = new Map(rows.map((row) => [key(row.job, row.step), row.median]));
  return jobs.flatMap((job) =>
    measured(job)
      .map((step) => ({
        run: job.run,
        attempt: job.attempt,
        job: job.job,
        step: step.name,
        seconds: step.seconds ?? 0,
        conclusion: step.conclusion,
        median: usual.get(key(job.job, step.name)),
      }))
      .filter(
        (step) =>
          (step.conclusion !== TimingConclusion.success && step.conclusion !== TimingConclusion.skipped) ||
          (step.median !== undefined && step.seconds >= TimingDefaults.minimumSeconds && step.seconds > step.median * factor),
      ),
  );
}

export function minutes(seconds: number): string {
  return `${(seconds / SecondsPerMinute).toFixed(1)}m`;
}

function table(header: readonly string[], rows: readonly (readonly string[])[]): string {
  const widths = header.map((title, column) => Math.max(title.length, ...rows.map((row) => row[column]?.length ?? 0)));
  const line = (cells: readonly string[]) =>
    cells
      .map((cell, column) => cell.padEnd(widths[column] ?? 0))
      .join("  ")
      .trimEnd();
  return [line(header), line(widths.map((width) => "-".repeat(width))), ...rows.map(line)].join("\n");
}

/** Renders the timing table and outlier list as plain text. */
export function timingReport(runs: number, rows: readonly TimingRow[], outliers: readonly TimingOutlier[]): string {
  const summary = table(
    ["Job", "Step", "Runs", "Median", "Max"],
    rows.map((row) => [row.job, row.step, String(row.samples), minutes(row.median), minutes(row.max)]),
  );
  const unusual = outliers.length
    ? table(
        ["Run", "Job", "Step", "Time", "Usual", "Result"],
        outliers.map((item) => [
          `${item.run}${item.attempt > 1 ? ` #${item.attempt}` : ""}`,
          item.job,
          item.step,
          minutes(item.seconds),
          item.median === undefined ? "-" : minutes(item.median),
          item.conclusion ?? "unfinished",
        ]),
      )
    : "None.";
  return `Build timings across ${runs} completed runs (successful steps only):\n\n${summary}\n\nFailed, cancelled or unusually slow steps:\n\n${unusual}\n`;
}
