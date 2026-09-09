import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { mkdirSync, readdirSync, realpathSync, writeFileSync } from "node:fs";
import { machine, release } from "node:os";
import { dirname, join } from "node:path";

import { createArtifactManifest, readCandidate } from "./artifact-manifest.ts";

const expectedNode = "v24.19.0";
const expectedBun = "1.3.14";
const expectedSdk = "10.0.111";

export interface HostFacts {
  readonly rid: string;
  readonly platform: string;
  readonly architecture: string;
  readonly node: string;
  readonly bun: string;
  readonly bunArchitecture: string;
  readonly sdk: string;
  readonly sdkArchitecture: string;
  readonly dotnetRoot: string;
  readonly dotnetExecutable: string;
  readonly installationDirectory: string;
}

export function qualifyHost(facts: HostFacts): HostFacts {
  const [operatingSystem, architecture, ...extra] = facts.rid.split("-");
  const platform = operatingSystem === "osx" ? "darwin" : operatingSystem === "win" ? "win32" : operatingSystem;
  assert.ok(
    extra.length === 0 &&
      ["linux", "darwin", "win32"].includes(platform ?? "") &&
      (architecture === "x64" || architecture === "arm64") &&
      facts.platform === platform &&
      facts.architecture === architecture,
    "host-target-mismatch",
  );
  assert.ok(facts.node === expectedNode && facts.bun === expectedBun && facts.sdk === expectedSdk, "host-tool-mismatch");
  assert.ok(facts.bunArchitecture === architecture && facts.sdkArchitecture === architecture, "host-tool-mismatch");
  assert.ok(facts.dotnetRoot === facts.installationDirectory && dirname(facts.dotnetExecutable) === facts.installationDirectory, "host-tool-mismatch");
  return facts;
}

if (import.meta.main) {
  const [rid, stage, ...extra] = process.argv.slice(2);
  assert.ok(
    rid && (stage === "build" || stage === "test" || stage === "package" || stage === "collect") && extra.length === 0,
    "Provide RID and build|test|package|collect stage.",
  );
  const directory = `artifacts/ci/receipts/${rid}/${stage}`;
  mkdirSync(directory, { recursive: true });
  const dotnetInfo = capture("dotnet", ["--info"], "dotnet-info.txt");
  const installation = process.env["DOTNET_INSTALL_DIR"];
  const dotnetRoot = process.env["DOTNET_ROOT"];
  assert.ok(installation && dotnetRoot, "host-tool-mismatch");
  const dotnetLocation = capture(process.platform === "win32" ? "where.exe" : "which", ["dotnet"], "dotnet-path.txt")
    .trim()
    .split(/\r?\n/)[0];
  assert.ok(dotnetLocation, "host-tool-mismatch");
  const facts = qualifyHost({
    rid,
    platform: process.platform,
    architecture: process.arch,
    node: process.version,
    bun: capture("bun", ["--version"], "bun-version.txt").trim(),
    bunArchitecture: capture("bun", ["-e", "process.stdout.write(process.arch)"], "bun-architecture.txt").trim(),
    sdk: capture("dotnet", ["--version"], "dotnet-version.txt").trim(),
    sdkArchitecture: /Architecture:\s+(\S+)/.exec(dotnetInfo)?.[1] ?? "",
    dotnetRoot: realpathSync(dotnetRoot),
    dotnetExecutable: realpathSync(dotnetLocation),
    installationDirectory: realpathSync(installation),
  });
  const kernelArchitecture = machine().toLowerCase();
  assert.ok(
    facts.architecture === "x64" ? kernelArchitecture === "x86_64" || kernelArchitecture === "amd64" : kernelArchitecture === "arm64" || kernelArchitecture === "aarch64",
    "host-target-mismatch",
  );
  assert.ok(process.env["RUNNER_ARCH"]?.toLowerCase() === facts.architecture, "host-target-mismatch");
  capture(process.platform === "win32" ? "where.exe" : "which", ["bun"], "bun-path.txt");
  capture(process.platform === "win32" ? "where.exe" : "which", ["npm"], "npm-path.txt");
  capture("dotnet", ["--list-sdks"], "dotnet-sdks.txt");
  capture("dotnet", ["--list-runtimes"], "dotnet-runtimes.txt");
  if (process.platform === "linux") {
    assert.match(capture("ldd", ["--version"], "glibc.txt"), /GLIBC|GNU libc/);
    capture("clang", ["--version"], "clang.txt");
    capture("ld", ["--version"], "linker.txt");
  } else if (process.platform === "darwin") {
    capture("sw_vers", [], "macos.txt");
    capture("xcodebuild", ["-version"], "xcode.txt");
    capture("clang", ["--version"], "clang.txt");
  } else {
    const programFiles = process.env["ProgramFiles(x86)"];
    assert.ok(programFiles, "host-tool-mismatch");
    const components = ["Microsoft.VisualStudio.Component.VC.Tools.x86.x64"];
    if (facts.architecture === "arm64") components.push("Microsoft.VisualStudio.Component.VC.Tools.ARM64");
    const installations: unknown = JSON.parse(
      capture(join(programFiles, "Microsoft Visual Studio/Installer/vswhere.exe"), ["-all", "-products", "*", "-requires", ...components, "-format", "json"], "visual-studio.json"),
    );
    assert.ok(Array.isArray(installations) && installations.length > 0, "host-tool-mismatch");
    const sdkLibraries = readdirSync(join(programFiles, "Windows Kits/10/Lib"));
    assert.ok(sdkLibraries.length > 0, "host-tool-mismatch");
    write("windows-sdk-libraries.json", sdkLibraries);
  }
  const commit = capture("git", ["rev-parse", "HEAD"], "commit.txt").trim();
  assert.equal(commit, process.env["GITHUB_SHA"], "candidate-mismatch");
  assert.equal(capture("git", ["status", "--porcelain", "--untracked-files=no"], "source-status.txt"), "", "candidate-mismatch");
  const candidate = readCandidate({ commit, tree: capture("git", ["rev-parse", "HEAD^{tree}"], "tree.txt").trim(), version: `0.0.0-dev.sha-${commit}` });
  const tracked = capture("git", ["ls-files", "-z"], "source-paths.txt").split("\0").filter(Boolean);
  write("source-manifest.json", createArtifactManifest(".", candidate, tracked));
  write("host.json", {
    schemaVersion: 1,
    candidate,
    ...facts,
    kernelArchitecture,
    operatingSystemRelease: release(),
    nodeExecutable: realpathSync(process.execPath),
    runnerImage: process.env["ImageOS"],
    runnerImageVersion: process.env["ImageVersion"],
    runnerName: process.env["RUNNER_NAME"],
    runId: process.env["GITHUB_RUN_ID"],
    runAttempt: process.env["GITHUB_RUN_ATTEMPT"],
  });
  writeFileSync("artifacts/ci/candidate.json", `${JSON.stringify(candidate, null, 2)}\n`, { flag: "wx" });

  function write(name: string, value: unknown): void {
    writeFileSync(join(directory, name), `${JSON.stringify(value, null, 2)}\n`, { flag: "wx" });
  }

  function capture(command: string, arguments_: readonly string[], filename: string): string {
    const result = spawnSync(command, arguments_, { shell: false, encoding: "utf8", maxBuffer: 16 * 1024 * 1024 });
    writeFileSync(join(directory, filename), result.stdout ?? "", { flag: "wx" });
    writeFileSync(join(directory, `${filename}.stderr`), result.stderr ?? "", { flag: "wx" });
    assert.ok(!result.error && result.status === 0 && result.signal === null, `Host probe failed: ${command} ${arguments_.join(" ")}`);
    return result.stdout;
  }
}
