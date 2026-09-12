import assert from "node:assert/strict";
import { execFileSync, spawnSync } from "node:child_process";
import { copyFileSync, mkdirSync, mkdtempSync, readdirSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import { hashArtifact } from "../../manifest.ts";
import { MainPackageName, PlatformPackages } from "../../package-model.ts";
import { IsolatedNpm } from "../../npm/__tests__/isolated-npm.ts";
import { compileLauncher, stagePackages } from "../../npm/stage.ts";
import { collectPackages } from "../collect-packages.ts";
import { readReleasePublications } from "../release-publications.ts";

const repository = fileURLToPath(new URL("../../../../", import.meta.url));
const version = "0.1.0-beta.1";
const sha = "a".repeat(40);

test("complete release collection preserves seven synchronized packages and six exact native archives", (context) => {
  const root = mkdtempSync(join(tmpdir(), "open-forge-collection-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  const npm = new IsolatedNpm(join(root, "scratch"));
  const launcher = join(root, "launcher");
  compileLauncher(repository, launcher);
  const input = join(root, "input");
  const releaseDirectory = "artifacts/release";
  const output = join(root, releaseDirectory);
  copyFileSync(join(repository, "LICENSE"), join(root, "LICENSE"));
  for (const platform of Object.values(PlatformPackages)) {
    const directory = join(input, `package-${platform.runtime}`);
    const portable = join(root, `portable-${platform.runtime}`);
    mkdirSync(portable, { recursive: true });
    mkdirSync(directory, { recursive: true });
    const native = join(portable, platform.nativeFileName);
    writeFileSync(native, `inert owned ${platform.runtime} native fixture`);
    copyFileSync(join(repository, "LICENSE"), join(portable, "LICENSE"));
    const staged = stagePackages({
      repositoryRoot: repository,
      artifactsRoot: join(root, "stage"),
      runtime: platform.runtime,
      nativeArtifact: native,
      version: { kind: "release", value: version },
      compiledLauncher: launcher,
    });
    const main = npm.pack(staged.mainPackageDirectory, join(root, "packs", platform.runtime, "main"));
    const target = npm.pack(staged.platformPackageDirectory, join(root, "packs", platform.runtime, "target"));
    for (const pack of [main, target]) copyFileSync(pack.path, join(directory, pack.path.split(/[\\/]/u).at(-1) ?? "missing"));
    const archive = `open-forge-${version}-${platform.runtime}.tar.gz`;
    execFileSync("tar", ["-czf", join(directory, archive), "-C", portable, platform.nativeFileName, "LICENSE"]);
    const files = readdirSync(directory).map((path) => ({ path, sha256: hashArtifact(directory, path) }));
    writeFileSync(join(directory, "package.json"), JSON.stringify({ sha, version, rid: platform.runtime, nativeSha256: hashArtifact(portable, platform.nativeFileName), files }));
  }
  collectPackages(input, output, sha, version);
  assert.equal(readdirSync(output).filter((file) => file.endsWith(".tgz")).length, 7);
  assert.equal(readdirSync(output).filter((file) => file.endsWith(".tar.gz")).length, 6);
  assert.equal(readFileSync(join(output, "SHA256SUMS"), "utf8").trim().split("\n").length, 6);
  const source = { sha, dirty: false };
  const publications = readReleasePublications(root, releaseDirectory, source, version);
  assert.deepEqual(
    publications.map((publication) => publication.name),
    [...Object.values(PlatformPackages).map((platform) => platform.packageName), MainPackageName],
  );
  assert.throws(() => readReleasePublications(root, releaseDirectory, { ...source, sha: "b".repeat(40) }, version), /Release source mismatch/u);
  assert.throws(() => readReleasePublications(root, releaseDirectory, source, "0.1.0-beta.2"), /Release version mismatch/u);
  const publisher = new URL("../../npm/publish-packages.ts", import.meta.url).href;
  const preview = spawnSync(
    process.execPath,
    [
      "--input-type=module",
      "-e",
      `import { publishPackages } from ${JSON.stringify(publisher)}; delete process.env.npm_execpath; publishPackages(process.cwd(), JSON.parse(process.argv[1]), "beta", true);`,
      JSON.stringify(publications),
    ],
    { cwd: root, encoding: "utf8" },
  );
  assert.equal(preview.error, undefined);
  assert.equal(preview.status, 0, preview.stderr);
  assert.deepEqual(
    JSON.parse(preview.stdout),
    publications.map((publication) => ({ ...publication, tag: "beta", dryRun: true })),
  );
  const wrapper = publications.at(-1);
  assert.ok(wrapper);
  const originalWrapper = readFileSync(wrapper.tarball);
  writeFileSync(wrapper.tarball, "changed after collection");
  assert.throws(() => readReleasePublications(root, releaseDirectory, source, version), /Package bytes changed/u);
  writeFileSync(wrapper.tarball, originalWrapper);
  rmSync(wrapper.tarball);
  assert.throws(() => readReleasePublications(root, releaseDirectory, source, version));
  assert.throws(() => collectPackages(input, join(root, "wrong-sha"), "b".repeat(40), version));
  assert.throws(() => collectPackages(input, join(root, "wrong-version"), sha, "0.1.0-beta.2"));
  const last = join(input, "package-win-arm64");
  const nativePack = readdirSync(last).find((file) => file.endsWith(".tgz") && file.includes("win-arm64"));
  assert.ok(nativePack);
  writeFileSync(join(last, nativePack), "changed tarball");
  assert.throws(() => collectPackages(input, join(root, "changed"), sha, version));
  rmSync(last, { recursive: true });
  assert.throws(() => collectPackages(input, join(root, "incomplete"), sha, version));
});
