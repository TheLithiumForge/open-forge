import type { Assessment, Batch, Checks, Functional, Judge, Outcome, Run, Choice } from "../records/records.types.ts";
export const AnalysisVersion = "screening-v2";
export const BootstrapSamples = 2000;
const ConfidenceZ = 1.959963984540054;
export interface Interval {
  readonly mean: number | null;
  readonly low: number | null;
  readonly high: number | null;
  readonly samples: number;
  readonly missing: number;
}
export interface AssessedRun extends Run {
  readonly judgeChoice?: Choice | null;
  readonly checksAssessment: Checks | null;
  readonly judgeAssessment: Judge | null;
  readonly result: Functional | null;
}
export function latestRun(run: Run, assessments: readonly Assessment[]): AssessedRun {
  let checks: Checks | null = null;
  let judge: Judge | null = null;
  for (const item of assessments) {
    if (item.kind === "pick" || item.runId !== run.id)
      continue;
    if (item.kind === "checks")
      checks = item;
    else
      judge = item;
  }
  const byId = new Map<string, Outcome>();
  for (const outcome of checks?.outcomes ?? [])
    if (!byId.has(outcome.id))
      byId.set(outcome.id, outcome);
  const ids = [...new Set(checks?.selection.applicable ?? [])];
  const passed = ids.filter((id) => byId.get(id)?.status === "pass").length;
  const gate = (required: readonly string[]) => required.length === 0 ? null : required.every((id) => byId.get(id)?.status === "pass");
  const result = checks === null ? run.functionalResult : checks.error !== null ? null : {
    coverage: {
      passed, applicable: ids.length,
      errors: ids.filter((id) => byId.get(id)?.status === "error").length, share: ids.length === 0 ? null : passed / ids.length
    },
    workingGate: gate(checks.selection.working), statedSuccess: gate(checks.selection.stated)
  };
  return { ...run, checksAssessment: checks, judgeAssessment: judge, result };
}
export function interval(values: readonly (number | null)[]): Interval {
  const known = values.filter((value): value is number => value !== null);
  const mean = known.length === 0 ? null : known.reduce((sum, value) => sum + value, 0) / known.length;
  if (known.length < 2)
    return { mean, low: null, high: null, samples: known.length, missing: values.length - known.length };
  let seed = 5489;
  const sample = () => { seed = (Math.imul(seed, 1664525) + 1013904223) >>> 0; return seed / 2 ** 32; };
  const means = Array.from({ length: BootstrapSamples }, () => known.reduce((sum) => sum + (known[Math.floor(sample() * known.length)] ?? 0), 0) / known.length).sort((a, b) => a - b);
  return { mean, low: means[Math.floor(BootstrapSamples * 0.025)] ?? null, high: means[Math.floor(BootstrapSamples * 0.975)] ?? null, samples: known.length, missing: values.length - known.length };
}
export function gradingScope(batch: Batch, run: AssessedRun): string {
  const checks = run.checksAssessment?.versions;
  const judge = run.judgeAssessment?.versions;
  const values = [batch.experiment.config.layer ?? null, batch.experiment.suite, checks?.spec, checks?.specVersion, judge?.spec, judge?.specVersion, run.judgeChoice, run.judgeAssessment?.reportedModel, run.judgeAssessment?.ballots, batch.experiment.fingerprints["benchmarkInputs"], checks?.checker, checks?.rubric, judge?.judge, judge?.rubric,
  batch.experiment.fingerprints["tooling"], run.agent.tool, run.requestedModel, run.reportedModel, run.agent.effort, batch.experiment.config.isolation];
  const required = [run.judgeAssessment?.reportedModel, batch.experiment.fingerprints["benchmarkInputs"], checks?.checker, checks?.rubric, judge?.judge, judge?.rubric, batch.experiment.fingerprints["tooling"], run.reportedModel];
  return JSON.stringify([...values, required.some((value) => value == null) ? batch.experiment.id : null]);
}
export function proportionInterval(values: readonly (boolean | null)[]): Interval {
  const known = values.filter((value): value is boolean => value !== null);
  const count = known.length;
  const mean = count === 0 ? null : known.filter(Boolean).length / count;
  if (count < 2 || mean === null)
    return { mean, low: null, high: null, samples: count, missing: values.length - count };
  const z2 = ConfidenceZ ** 2;
  const denominator = 1 + z2 / count;
  const center = (mean + z2 / (2 * count)) / denominator;
  const radius = ConfidenceZ * Math.sqrt((mean * (1 - mean) + z2 / (4 * count)) / count) / denominator;
  return { mean, low: Math.max(0, center - radius), high: Math.min(1, center + radius), samples: count, missing: values.length - count };
}
export function ballotShare(run: AssessedRun, area: string): number | null {
  const answers = run.judgeAssessment?.error != null ? [] : run.judgeAssessment?.answers.filter((answer) => answer.area === area) ?? [];
  const sure = answers.reduce((sum, answer) => sum + answer.yes + answer.no, 0);
  return sure === 0 ? null : answers.reduce((sum, answer) => sum + answer.yes, 0) / sure;
}
export function tokenTotal(run: Run): number | null {
  const counts = run.telemetry?.tokens;
  return counts == null ? null : counts.input + counts.cachedInput + counts.cacheWrite + counts.output;
}
