export const deliveryCommands = {
  setup: { script: "setup.ts", description: "Install npm dependencies and restore .NET.", options: ["offline"] },
  restore: { script: "restore.ts", description: "Restore .NET dependencies.", options: ["offline"] },
  clean: { script: "clean.ts", description: "Remove owned build and delivery outputs.", options: [] },
  build: { script: "build.ts", description: "Build the managed solution.", options: ["offline", "no-restore"] },
  test: { script: "test.ts", description: "Build and run managed tests on this host.", options: ["offline", "no-restore"] },
  "build:native": { script: "build-native.ts", description: "Build native CLI and test executables for this host.", options: ["rid", "sha", "offline", "no-restore"] },
  "test:built": { script: "test-built.ts", description: "Run this host's six managed/native test modes without rebuilding.", options: ["rid"] },
  pack: { script: "pack.ts", description: "Pack existing native artifacts; optionally skip qualification and installation tests.", options: ["rid", "targets", "skip-tests"] },
  dist: {
    script: "dist.ts",
    description: "Run the build, test and pack pipeline for this host.",
    options: ["rid", "sha", "offline", "no-restore", "skip-tests", "plan", "targets"],
  },
  "dist:wrapper": { script: "npm/wrapper-dist.ts", description: "Build and pack the npm wrapper without .NET.", options: ["sha", "targets"] },
  "publish:native": { script: "npm/publish.ts", arguments: ["native"], description: "Publish the qualified host package, or preview it offline.", options: ["tag", "dry-run"] },
  "publish:wrapper": { script: "npm/publish.ts", arguments: ["wrapper"], description: "Publish the independent wrapper, or preview it offline.", options: ["tag", "dry-run"] },
  "release:select": { script: "release/prepare-release.ts", description: "Select the exact CI release source using workflow inputs.", options: [] },
  "release:collect": {
    script: "release/collect-packages.ts",
    description: "Collect tested packages for selected targets: <input-directory> <output-directory>.",
    options: ["targets"],
  },
  "publish:release": { script: "release/publish-release.ts", description: "Publish the collected target selection, wrapper last.", options: ["from", "tag", "dry-run"] },
  version: { description: "Bump with npm: <patch|minor|major|prerelease|version>; no commit or tag.", options: ["preid"] },
  "version:sync": { script: "version-sync.ts", description: "Project the root version to .NET (npm version lifecycle hook).", options: [] },
} as const;

export type DeliveryCommand = keyof typeof deliveryCommands;
