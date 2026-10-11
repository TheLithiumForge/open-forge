export const releaseCommands = {
  "release:select": {
    script: "release/prepare-release.ts",
    description: "CI only: select the exact release source from workflow inputs.",
    options: [],
    details: [
      "Runs only inside GitHub Actions; normally called by release.yml. Reads GitHub release/build metadata.",
      "Uses GITHUB_ACTIONS, GITHUB_REPOSITORY, GITHUB_OUTPUT, GITHUB_TOKEN and GITHUB_API_URL.",
      "Optional inputs: RELEASE_TAG, RELEASE_TARGET (github/npm/all; default all), BUILD_RUN_ID. Writes selected values to GITHUB_OUTPUT.",
      "Without BUILD_RUN_ID, reuses the newest successful exact-commit Build with all six unexpired package artifacts. Supplied runs pass the same checks.",
    ],
    examples: ["release:select --help"],
  },
  "release:checkpoint": {
    script: "release/checkpoint.ts",
    description: "Print the complete publication plan and wait before uploads.",
    options: ["from", "target", "tag", "wait"],
    usage: "release:checkpoint [--wait <seconds>] [--from <directory>] [--target <destination>] [--tag <channel>]",
    details: [
      "Reads collected release packages matching the current source/version. Defaults to artifacts/release, all destinations and the version's channel.",
      "Prints every npm package and GitHub release file, then waits 60 seconds by default. Cancelling during the wait publishes nothing.",
    ],
    examples: ["release:checkpoint --wait 60", "release:checkpoint --target npm --wait 0"],
  },
  "release:restore-channels": {
    script: "release/restore-channels.ts",
    description: "Preview or apply npm channel restoration for all seven packages.",
    options: ["version", "to", "channels", "apply"],
    usage: "release:restore-channels --version <bad> --to <previous> [--channels <tags>] [--apply]",
    details: [
      "Prints npm dist-tag commands offline. Defaults to latest plus the bad version's prerelease channel.",
      "--apply checks that the previous version exists for all seven packages before changing any channel. Requires npm credentials. Never unpublishes or deprecates a version.",
    ],
    examples: ["release:restore-channels --version 0.9.0-beta.11 --to 0.9.0-beta.10", "release:restore-channels --version 1.0.1 --to 1.0.0 --channels latest"],
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
      "SIGINT or SIGTERM stops further uploads and tag changes after the current effect settles. Cancellation reports published, interrupted and unstarted packages and exits non-zero.",
      "Actual upload needs committed matching source and npm credentials/registry. Uploads are not atomic; rerun to fill missing versions. Prereleases cannot use latest, but become latest until the first stable release.",
    ],
    examples: ["publish:release --tag preview --dry-run", "publish:release --from artifacts/release --tag latest --dry-run"],
  },
} as const;
