import assert from "node:assert/strict";
import { test } from "node:test";
import { bundleNames, bundleReadme, checksumLine } from "../bundle-model.ts";
import { AllTargets } from "../../targets.ts";
import { ArchiveSuffix, CliExecutable, CliAssembly, DownloadContent, DownloadsRoot, WindowsExecutableSuffix } from "../../layout.ts";
import { PlatformPackages } from "../../package-model.ts";

const FixtureHash = "0123456789abcdef".repeat(4);

test("bundle names preserve each RID and executable suffix", () => {
  for (const rid of AllTargets) {
    const suffix = PlatformPackages[rid].nodePlatform === "win32" ? WindowsExecutableSuffix : "";
    const names = bundleNames(rid);
    assert.equal(names.directory, `${DownloadsRoot}/${rid}`);
    assert.equal(names.content, `${names.directory}/${DownloadContent}`);
    assert.equal(names.executable, `${CliExecutable}${suffix}`);
    assert.ok(names.source.endsWith(`/${CliAssembly}${suffix}`));
    assert.equal(names.file, `${CliExecutable}-${rid}${ArchiveSuffix}`);
    assert.equal(names.archive, `${names.directory}/${names.file}`);
    assert.equal(checksumLine(FixtureHash, names.file), `${FixtureHash}  ${names.file}\n`);
    assert.equal(bundleReadme(rid), `Development build for ${rid}. Check this workflow run for test results before use.\n`);
  }
});
