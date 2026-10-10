export const releaseCommands = {
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
      "Actual upload needs committed matching source and npm credentials/registry. Uploads are not atomic; rerun to fill missing versions. Prereleases cannot use latest, but become latest until the first stable release.",
    ],
    examples: ["publish:release --tag preview --dry-run", "publish:release --from artifacts/release --tag latest --dry-run"],
  },
} as const;
