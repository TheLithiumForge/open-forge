export const deliveryCommands = {
  setup: {
    script: "setup.ts",
    description: "Install npm dependencies and restore .NET.",
    options: ["offline"],
    details: [
      "Requires Node/npm and the .NET SDK from global.json. Native builds also need the host's native toolchain.",
      "Runs npm ci (replaces node_modules), then dotnet restore. --offline needs already-cached packages.",
    ],
    examples: ["setup", "setup --offline"],
  },
  restore: {
    script: "restore.ts",
    description: "Restore .NET dependencies.",
    options: ["offline"],
    details: ["Does not install npm dependencies; use setup for a fresh checkout.", "Run again after clean or changes to .NET dependencies."],
    examples: ["restore", "restore --offline"],
  },
  clean: {
    script: "clean.ts",
    description: "Remove owned build and delivery outputs.",
    options: [],
    details: [
      "Removes artifacts/bin, artifacts/obj, artifacts/publish and owned host/wrapper delivery outputs and managed reports.",
      "Keeps npm dependencies, offline caches, logs, collected releases and local npm links. Restore before using --no-restore again.",
    ],
    examples: ["clean", "restore"],
  },
  build: {
    script: "build.ts",
    description: "Build the managed solution.",
    options: ["offline", "no-restore"],
    details: [
      "Builds Release artifacts and the managed development CLI. Does not run tests or produce npm packages.",
      "Restores by default. After setup, use --no-restore; use build:native for Native AOT artifacts.",
    ],
    examples: ["build --no-restore"],
  },
  test: {
    script: "test.ts",
    description: "Build and run managed tests on this host.",
    options: ["offline", "no-restore"],
    details: [
      "Runs managed unit, integration and public-command suites. Reports: artifacts/delivery/managed-reports.",
      "For the six managed/native execution modes, use build:native followed by test:built.",
    ],
    examples: ["test --no-restore"],
  },
  "build:native": {
    script: "build-native.ts",
    description: "Build native CLI and test executables for this host.",
    options: ["rid", "sha", "offline", "no-restore"],
    details: [
      "Run setup first; requires the host's native toolchain. Restores by default; does not run tests.",
      "Writes native output under artifacts/publish/<RID> and build metadata under artifacts/delivery/<RID>.",
      "Next: test:built, then pack. For local packaging despite failing tests, use pack --skip-tests.",
    ],
    examples: ["build:native --no-restore", "build:native --sha --offline"],
  },
  "test:built": {
    script: "test-built.ts",
    description: "Run this host's six managed/native test modes without rebuilding.",
    options: ["rid"],
    details: [
      "Requires current build:native artifacts. Runs unit tests once, integration twice and public-command tests three times.",
      "These are six execution modes on this host, not tests for six operating-system targets.",
      "Writes reports under artifacts/delivery/<RID>/reports. Next: pack.",
    ],
    examples: ["test:built"],
  },
  pack: {
    script: "pack.ts",
    description: "Pack existing native artifacts and check npm installation.",
    options: ["rid", "targets", "skip-tests"],
    details: [
      "Requires current build:native artifacts; normally also requires passing test:built reports. Does not rebuild or run .NET tests.",
      "Creates a portable archive, native npm tarball, wrapper npm tarball and checksums in artifacts/delivery/<RID>/packages. Licenses are included.",
      "--skip-tests bypasses qualification and the npm installation check. Untested native packages cannot be published or collected for release.",
      "--targets changes the wrapper's dependencies, not the native build target; the list must include this host.",
    ],
    examples: ["pack", "pack --skip-tests", "pack --targets linux-x64,osx-x64,win-x64"],
  },
  dist: {
    script: "dist.ts",
    description: "Run build:native, test:built and pack for this host.",
    options: ["rid", "sha", "offline", "no-restore", "skip-tests", "plan", "targets"],
    details: [
      "Prints each stage and its effective arguments. Stops on the first failed stage and reports its name and command/log.",
      "--plan prints the pipeline without building, testing or packing. --skip-tests still compiles test executables, but does not execute tests.",
      "--targets selects wrapper dependencies and must include this host. Output: artifacts/delivery/<RID>/packages.",
    ],
    examples: ["dist --no-restore --plan", "dist --no-restore", "dist --skip-tests --no-restore"],
  },
  "dist:wrapper": {
    script: "npm/wrapper-dist.ts",
    description: "Build and pack the npm wrapper without .NET.",
    options: ["sha", "targets"],
    details: [
      "Requires npm dependencies (npm ci --ignore-scripts), but no .NET SDK or native artifacts.",
      "Includes the license and exactly the selected versioned dependencies. Output: artifacts/delivery/wrapper/packages.",
      "Publish every selected native package at the same version before publishing the wrapper.",
    ],
    examples: ["dist:wrapper --targets linux-x64,osx-x64,win-x64", "publish:wrapper --tag preview --dry-run"],
  },
  "publish:native": {
    script: "npm/publish.ts",
    arguments: ["native"],
    description: "Publish the qualified host package, or preview it offline.",
    options: ["tag", "dry-run"],
    usage: "publish:native --tag <channel> [--dry-run]",
    details: [
      "Requires tested pack output for this host. Uploads only its native package; the wrapper is separate.",
      "Actual upload needs committed matching source and configured npm credentials/registry. --dry-run stays offline.",
      "Existing versions warn and skip without retagging. Prerelease versions cannot use latest.",
    ],
    examples: ["publish:native --tag preview --dry-run"],
  },
  "publish:wrapper": {
    script: "npm/publish.ts",
    arguments: ["wrapper"],
    description: "Publish the packed wrapper, or preview it offline.",
    options: ["tag", "dry-run"],
    usage: "publish:wrapper --tag <channel> [--dry-run]",
    details: [
      "Requires dist:wrapper or pack output. Does not need native artifacts locally; uses the already-packed target selection.",
      "Actual upload needs committed matching source and configured npm credentials/registry. --dry-run stays offline.",
      "Existing matching versions warn and skip. Different published dependencies require a new version. Prereleases cannot use latest.",
    ],
    examples: ["publish:wrapper --tag preview --dry-run"],
  },
  "release:select": {
    script: "release/prepare-release.ts",
    description: "CI only: select the exact release source from workflow inputs.",
    options: [],
    details: [
      "Runs only inside GitHub Actions; normally called by release.yml. Reads GitHub release/build metadata.",
      "Uses GITHUB_ACTIONS, GITHUB_REPOSITORY, GITHUB_OUTPUT, GITHUB_TOKEN and GITHUB_API_URL.",
      "Optional inputs: RELEASE_TAG, RELEASE_TARGET (github/npm/all; default all), BUILD_RUN_ID. Writes selected values to GITHUB_OUTPUT.",
    ],
    examples: ["release:select --help"],
  },
  "release:collect": {
    script: "release/collect-packages.ts",
    description: "Collect tested packages for the selected release targets.",
    options: ["targets"],
    usage: "release:collect <input-directory> <output-directory> [--targets <rids>]",
    details: [
      "Requires committed source and input/package-<RID>/package.json plus its three archives for every selected target.",
      "Use the same --targets when packing on each selected host. All versions, source identities, hashes and wrapper dependencies must match.",
      "Replaces the output directory with selected archives, checksums and release.json. Keep input/output separate; output must be inside artifacts/.",
    ],
    examples: ["release:collect artifacts/release-input artifacts/release", "release:collect artifacts/release-input artifacts/release --targets osx-x64,win-x64"],
  },
  "publish:release": {
    script: "release/publish-release.ts",
    description: "Publish the collected target selection, wrapper last.",
    options: ["from", "tag", "dry-run"],
    usage: "publish:release --tag <channel> [--from <directory>] [--dry-run]",
    details: [
      "Requires release:collect output matching the current source/version. Uses the exact selection recorded in release.json.",
      "Checks every selected package before uploads. Existing versions warn and skip; a different published wrapper graph rejects the run.",
      "Actual upload needs committed matching source and npm credentials/registry. Uploads are not atomic; rerun to fill missing versions. Prereleases cannot use latest.",
    ],
    examples: ["publish:release --tag preview --dry-run", "publish:release --from artifacts/release --tag latest --dry-run"],
  },
  version: {
    description: "Bump the root package version and synchronize .NET; no commit or tag.",
    options: ["preid"],
    usage: "version <increment|version> [--preid <id>]",
    details: [
      "Uses npm version, updating package.json, package-lock.json and Directory.Build.props through its lifecycle hook.",
      "Increments: patch, minor, major, prepatch, preminor, premajor, prerelease. An explicit SemVer is also accepted.",
      "Review and commit the changed version files, then rebuild/repack. A published wrapper version's target list cannot be changed.",
    ],
    examples: ["version patch", "version prerelease --preid beta", "version 0.1.0-beta.1"],
  },
  "version:sync": {
    script: "version-sync.ts",
    description: "Project the root version to .NET (npm lifecycle helper).",
    options: [],
    details: [
      "Normally invoked automatically by version. Reads package.json and updates OpenForgeCliVersion in Directory.Build.props.",
      "Does not bump the root version or update the lockfile; prefer version for ordinary version changes.",
    ],
    examples: ["version:sync"],
  },
} as const;

export type DeliveryCommand = keyof typeof deliveryCommands;
