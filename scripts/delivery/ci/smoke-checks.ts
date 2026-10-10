import { ProcessExit } from "../process.ts";

export const SmokeCheck = { version: "Published version", install: "Install Essentials", status: "Installed workspace status", configure: "Configure guidance preview" } as const;
export const SmokeText = { current: "Open Forge is installed and current.", entries: "Entries would be updated", replaced: "replaced" } as const;
export const SmokeArguments = {
  version: ["--version"],
  install: ["install", "--preset", "essentials", "--automatic"],
  status: ["status"],
  configure: ["install", "--configure", "--preset", "custom", "--route", "guidance=add", "--dry-run"],
} as const;
export interface CommandOutput {
  status: number | null;
  stdout: string;
  stderr: string;
}

export function evaluateSmoke(name: (typeof SmokeCheck)[keyof typeof SmokeCheck], output: CommandOutput, version: string): boolean {
  if (output.status !== ProcessExit.success) return false;
  switch (name) {
    case SmokeCheck.version:
      return output.stdout.trim() === version;
    case SmokeCheck.install:
      return true;
    case SmokeCheck.status:
      return output.stdout.includes(SmokeText.current);
    case SmokeCheck.configure:
      return output.stdout.includes(SmokeText.entries) && !`${output.stdout}\n${output.stderr}`.toLowerCase().includes(SmokeText.replaced);
  }
}
