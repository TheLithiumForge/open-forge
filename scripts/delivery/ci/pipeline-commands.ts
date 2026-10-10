import { PipelineCommand } from "./command-names.ts";

const PlanOptions = ["plan"] as const;
const PipelineDetails = ["Prints ordered stages and stops at the first failure. --plan lists stages without executing them."];

export const pipelineCommands = {
  [PipelineCommand.verify]: {
    script: "ci/pipeline.ts",
    arguments: [PipelineCommand.verify],
    options: PlanOptions,
    description: "Run delivery, package, .NET and documentation checks.",
    details: PipelineDetails,
    examples: ["verify --plan"],
  },
  [PipelineCommand.check]: {
    script: "ci/pipeline.ts",
    arguments: [PipelineCommand.check],
    options: PlanOptions,
    description: "Check formatting, TypeScript and lint.",
    details: PipelineDetails,
    examples: ["check"],
  },
  [PipelineCommand.fast]: {
    script: "ci/pipeline.ts",
    arguments: [PipelineCommand.fast],
    options: PlanOptions,
    description: "Check TypeScript and lint.",
    details: PipelineDetails,
    examples: ["check:fast"],
  },
  [PipelineCommand.delivery]: {
    script: "ci/pipeline.ts",
    arguments: [PipelineCommand.delivery],
    options: PlanOptions,
    description: "Check delivery TypeScript, lint and formatting.",
    details: PipelineDetails,
    examples: ["check:delivery"],
  },
  [PipelineCommand.checks]: {
    script: "ci/pipeline.ts",
    arguments: [PipelineCommand.checks],
    options: ["offline", "plan"],
    description: "Run the Build workflow's shared checks locally.",
    details: PipelineDetails,
    examples: ["ci:checks --plan"],
  },
  [PipelineCommand.job]: {
    script: "ci/pipeline.ts",
    arguments: [PipelineCommand.job],
    options: ["rid", "offline", "no-restore", "plan"],
    description: "Run one Build matrix job with its workflow logs.",
    details: [...PipelineDetails, "Runs setup, delivery plan, build, bundle, tests and pack. --no-restore reuses setup from an earlier run."],
    examples: ["ci:job --rid win-x64 --plan", "ci:job --rid linux-x64 --no-restore"],
  },
  [PipelineCommand.docs]: {
    script: "ci/pipeline.ts",
    arguments: [PipelineCommand.docs],
    options: PlanOptions,
    description: "Install, type-check and build the documentation site.",
    details: [...PipelineDetails, "Runs npm ci, npm run typecheck and npm run build in src/docusaurus."],
    examples: ["docs:build --plan"],
  },
} as const;
