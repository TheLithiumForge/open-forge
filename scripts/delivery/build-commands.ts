export const buildCommands = {
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
} as const;
