import type { Batch } from "./records.types.ts";

function unique(ids: readonly string[], kind: string): void {
  if (new Set(ids).size !== ids.length) throw new Error(`Duplicate ${kind}.`);
}
export function validateReferences(batch: Batch): void {
  const { experiment, runs, setups, assessments, artifacts } = batch;
  if (!["done", "failed"].includes(experiment.status) || runs.length === 0 || runs.some((run) => !["done", "failed", "limited"].includes(run.pipelineStatus) || run.finishedAt === null)) throw new Error("Only finished exported batches can be published.");
  unique(runs.map((run) => run.id), "run"); unique(setups.map((setup) => setup.id), "setup"); unique(assessments.map((record) => record.id), "assessment");
  unique(experiment.runs, "planned run");
  if (experiment.runs.length !== runs.length || runs.some((run) => !experiment.runs.includes(run.id))) throw new Error("The experiment's run list differs from runs.jsonl.");
  if (setups.length !== experiment.config.setups.length || setups.some((setup) => setup.experimentId !== experiment.id || !experiment.config.setups.includes(setup.id))) throw new Error("The experiment's setup list differs from setups.jsonl.");
  const byRun = new Map(runs.map((run) => [run.id, run]));
  const byAssessment = new Map(assessments.map((record) => [record.id, record]));
  for (const run of runs) {
    if (run.batchId !== experiment.id || !setups.some((setup) => setup.id === run.setup)) throw new Error("Run has an unknown experiment or setup.");
    const visited = new Set([run.id]);
    let parent = run.replacesRunId;
    while (parent !== null) {
      if (visited.has(parent)) throw new Error("Replacement runs form a cycle.");
      visited.add(parent);
      const previous = byRun.get(parent);
      if (previous === undefined || previous.setup !== run.setup || previous.repeat !== run.repeat || previous.agent.tool !== run.agent.tool || previous.agent.model !== run.agent.model || previous.agent.effort !== run.agent.effort) throw new Error("Replacement doesn't match the attempt it replaces.");
      parent = previous.replacesRunId;
    }
  }
  const seen = new Set<string>();
  for (const assessment of assessments) {
    if (assessment.experimentId !== experiment.id) throw new Error("Assessment names an unknown experiment.");
    if (assessment.replacesAssessmentId !== null) {
      const previous = byAssessment.get(assessment.replacesAssessmentId);
      if (!seen.has(assessment.replacesAssessmentId) || previous?.kind !== assessment.kind || (previous.kind !== "pick" && assessment.kind !== "pick" && previous.runId !== assessment.runId)) throw new Error("Assessment replacement must name an earlier assessment of the same run and kind.");
    }
    seen.add(assessment.id);
    if (assessment.kind === "pick") {
      const left = byRun.get(assessment.left.runId);
      const right = byRun.get(assessment.right.runId);
      if (left === undefined || right === undefined || left.id === right.id) throw new Error("Pick must name two runs.");
      for (const side of [assessment.left, assessment.right]) {
        const expected = byRun.get(side.runId)?.artifactHash;
        if (expected != null && expected !== side.artifactHash) throw new Error("Pick artifact hash differs from its run.");
      }
    } else {
      if (!byRun.has(assessment.runId)) throw new Error("Assessment names an unknown run.");
      if (assessment.kind === "checks") {
        unique(assessment.outcomes.map((outcome) => outcome.id), "check");
        if ([...assessment.selection.working, ...assessment.selection.stated].some((id) => !assessment.selection.applicable.includes(id))) throw new Error("A gate names a check outside its applicable set.");
      }
    }
  }
  unique(artifacts.map((artifact) => `${artifact.runId}/${artifact.assessmentId}/${artifact.path}`), "artifact");
  for (const artifact of artifacts) {
    if (!byRun.has(artifact.runId)) throw new Error("Artifact names an unknown run.");
    if (artifact.assessmentId !== null) {
      const assessment = byAssessment.get(artifact.assessmentId);
      if (assessment === undefined || assessment.kind === "pick" || assessment.runId !== artifact.runId) throw new Error("Artifact names an unknown assessment.");
    }
  }
}
