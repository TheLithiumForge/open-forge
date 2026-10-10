import assert from "node:assert/strict";
import { Configuration, TestAssemblies, DeliveryLogs } from "../layout.ts";
import { WslDotnet, WslTestArguments } from "./wsl-arguments.ts";

export const WslLayout = {
  clone: ".open-forge-gates/open-forge",
  logs: `${DeliveryLogs}/wsl`,
  feed: "empty-feed",
  report: "results.json",
  execution: "execution.log",
  reasons: "skip-reasons.txt",
  summarySuffix: ".summary.txt",
} as const;
export const WslTool = { host: "wsl.exe", node: "node", git: "git", dotnet: "dotnet" } as const;
export const WslEnvironment = { dataHome: "OPENFORGE_DATA_HOME", dataFolder: "artifacts/delivery/wsl-data-home" } as const;
export const WslTestStatus = { passed: "passed", skipped: "skipped" } as const;

export function wslPath(hostPath: string): string {
  const match = /^([a-z]):[\\/](.*)$/iu.exec(hostPath);
  assert.ok(match?.[1] && match[2] !== undefined, "WSL requires an absolute Windows drive path.");
  return `/mnt/${match[1].toLowerCase()}/${match[2].replace(/\\/gu, "/")}`;
}

export function wslBuildPlan(feed: string) {
  return [
    { name: "Restore managed solution", args: [WslDotnet.restore, WslDotnet.solution, WslDotnet.source, feed, WslDotnet.noAudit] },
    { name: "Build managed solution", args: [WslDotnet.build, WslDotnet.solution, WslDotnet.configuration, Configuration, WslDotnet.noRestore] },
  ];
}

export function wslTestPlan(reports: string) {
  return Object.entries(TestAssemblies).map(([name, assembly]) => ({
    name,
    args: [
      `artifacts/bin/${assembly}/release/${assembly}${WslDotnet.assemblySuffix}`,
      ...WslTestArguments,
      WslDotnet.reportFile,
      WslLayout.report,
      WslDotnet.results,
      `${reports}/${name}`,
    ],
  }));
}
