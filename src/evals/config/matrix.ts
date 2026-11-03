import { Tool } from "./choices.ts";
import type { JudgeChoice, ModelChoice } from "./config.types.ts";

export const MatrixModels = { codex: "gpt-6.1-sol", claude: "opus", claudeUser: "sonnet", codexUser: "gpt-6-luna" } as const;
export const MatrixEffort = { max: "max", xhigh: "xhigh", high: "high", medium: "medium", low: "low" } as const;
export const MatrixBallots = 3;
export const MatrixAgents = [
  // Sol 6.1 at xhigh is the maintainer's usual level, set on 2026-11-03. Claude keeps high until its runs resume.
  { tool: Tool.codex, model: MatrixModels.codex, effort: MatrixEffort.xhigh },
  { tool: Tool.claude, model: MatrixModels.claude, effort: MatrixEffort.high },
] as const satisfies readonly ModelChoice[];
export const MatrixJudges = [
  { tool: Tool.claude, model: MatrixModels.claude, effort: MatrixEffort.high, repeats: MatrixBallots },
  { tool: Tool.codex, model: MatrixModels.codex, effort: MatrixEffort.xhigh, repeats: MatrixBallots },
] as const satisfies readonly JudgeChoice[];
export const MatrixPlanLayers = [1, 2, 3, 4] as const;
export const MatrixPlanVariants = ["check", "full"] as const;
