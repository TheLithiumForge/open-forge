export const WslGit = {
  safeDirectory: ["-c", "safe.directory=*"],
  commonDirectory: ["rev-parse", "--git-common-dir"],
  resolveCommit: ["rev-parse", "--verify", "--quiet"],
  commitSuffix: "^{commit}",
  branch: ["symbolic-ref", "--short", "HEAD"],
  clone: ["clone", "--no-hardlinks", "--no-checkout"],
  origin: ["remote", "get-url", "origin"],
  status: ["status", "--porcelain"],
  fetch: ["fetch", "--no-tags", "origin"],
  ancestor: ["merge-base", "--is-ancestor"],
  fetchedHead: "FETCH_HEAD",
  checkout: ["checkout", "--detach"],
} as const;
// A login shell loads the user's profile, where per-user Node managers such as nvm or Volta put node on PATH.
export const WslLoginShell = ["--shell-type", "login", "--"] as const;
export const WslFlag = { source: "--source", branch: "--branch", commit: "--commit", logs: "--logs" } as const;
export const WslDotnet = {
  solution: "OpenForge.Cli.slnx",
  restore: "restore",
  source: "--source",
  noAudit: "-p:NuGetAudit=false",
  build: "build",
  configuration: "--configuration",
  noRestore: "--no-restore",
  assemblySuffix: ".dll",
  reportFile: "--report-xunit-ctrf-filename",
  results: "--results-directory",
} as const;
export const WslTestArguments = [
  "--minimum-expected-tests",
  "1",
  "--parallel",
  "collections",
  "--fail-warns",
  "on",
  "--fail-skips",
  "off",
  "--no-ansi",
  "--progress",
  "off",
  "--report-xunit-ctrf",
] as const;
