import assert from "node:assert/strict";
import { DocumentationRoot, deliveryLog } from "../layout.ts";
import { PipelineCommand, LeafCommand, CiCommand, NpmCommand, PipelineFlag } from "./command-names.ts";
import type { SupportedRuntime } from "../package-model.ts";

export interface PipelineStep {
  name: string;
  args: readonly string[];
  directory?: string;
}
export interface PipelineOptions {
  offline?: boolean;
  "no-restore"?: boolean;
}
export const WorkflowLogStage = { build: "build", bundle: "bundle", test: "test", pack: "pack" } as const;

function scriptStep(script: string, args: readonly string[] = []): PipelineStep {
  return { name: script, args: [NpmCommand.run, script, ...(args.length ? [NpmCommand.separator, ...args] : [])] };
}

export function checkPlan(command: PipelineCommand): PipelineStep[] {
  switch (command) {
    case PipelineCommand.verify:
      return [PipelineCommand.delivery, LeafCommand.deliveryTests, LeafCommand.packageLayout, LeafCommand.dotnet, LeafCommand.docs].map((script) => scriptStep(script));
    case PipelineCommand.check:
      return [LeafCommand.format, LeafCommand.typecheck, LeafCommand.lint].map((script) => scriptStep(script));
    case PipelineCommand.fast:
      return [LeafCommand.typecheck, LeafCommand.lint].map((script) => scriptStep(script));
    case PipelineCommand.delivery:
      return [LeafCommand.deliveryTypes, LeafCommand.deliveryLint, LeafCommand.deliveryFormat].map((script) => scriptStep(script));
    case PipelineCommand.checks:
    case PipelineCommand.job:
    case PipelineCommand.docs:
      throw new Error(`Use the dedicated plan for ${command}.`);
    default: {
      const unknown: never = command;
      throw new Error(`Unknown pipeline: ${unknown}`);
    }
  }
}

export function checksPlan(options: PipelineOptions = {}): PipelineStep[] {
  return [scriptStep(LeafCommand.setup, options.offline ? [PipelineFlag.offline] : []), scriptStep(PipelineCommand.verify)];
}

export function jobPlan(rid: SupportedRuntime, options: PipelineOptions = {}): PipelineStep[] {
  assert.ok(!(options.offline && options["no-restore"]), "Choose --offline or --no-restore, not both.");
  const target = [PipelineFlag.rid, rid];
  const restore = options.offline ? PipelineFlag.offline : PipelineFlag.noRestore;
  const logged = (script: string, stage: string, args: readonly string[] = []) => scriptStep(script, [...target, ...args, PipelineFlag.log, deliveryLog(rid, stage)]);
  return [
    ...(options["no-restore"] ? [] : [scriptStep(LeafCommand.setup, options.offline ? [PipelineFlag.offline] : [])]),
    scriptStep(LeafCommand.dist, [...target, restore, PipelineFlag.plan]),
    logged(LeafCommand.native, WorkflowLogStage.build, [restore]),
    logged(CiCommand.bundle, WorkflowLogStage.bundle),
    logged(LeafCommand.testBuilt, WorkflowLogStage.test),
    logged(LeafCommand.pack, WorkflowLogStage.pack),
  ];
}

export function docsPlan(): PipelineStep[] {
  return [
    { name: "Install documentation dependencies", args: [NpmCommand.install], directory: DocumentationRoot },
    { name: "Type-check documentation", args: [NpmCommand.run, NpmCommand.typecheck], directory: DocumentationRoot },
    { name: "Build documentation", args: [NpmCommand.run, NpmCommand.build], directory: DocumentationRoot },
  ];
}

export function describeStep(step: PipelineStep): string {
  return `${step.name}: npm ${step.args.join(" ")}${step.directory ? ` (in ${step.directory})` : ""}`;
}
