import { Tool } from "./choices.ts";
import type { JudgeChoice, ModelChoice } from "./config.types.ts";

export const MatrixModels = { codex: "gpt-6.1-sol", claude: "opus" } as const;
export const MatrixEffort = { high: "high", xhigh: "xhigh" } as const;
export const MatrixBallots = 3;
export const MatrixAgents = [
  { tool: Tool.codex, model: MatrixModels.codex, effort: MatrixEffort.high },
  { tool: Tool.claude, model: MatrixModels.claude, effort: MatrixEffort.high },
] as const satisfies readonly ModelChoice[];
export const MatrixJudges = [
  { tool: Tool.claude, model: MatrixModels.claude, effort: MatrixEffort.high, repeats: MatrixBallots },
  { tool: Tool.codex, model: MatrixModels.codex, effort: MatrixEffort.xhigh, repeats: MatrixBallots },
] as const satisfies readonly JudgeChoice[];
// The saved plans run Claude alone for now. The matrix stays supported for when Codex returns to the plans.
export const PlanAgents = [MatrixAgents[1]] as const satisfies readonly ModelChoice[];
export const PlanJudges = [MatrixJudges[0]] as const satisfies readonly JudgeChoice[];
export const MatrixPlanLayers = [1, 2, 3, 4] as const;
export const MatrixPlanVariants = ["check", "full"] as const;
