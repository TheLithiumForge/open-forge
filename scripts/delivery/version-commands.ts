export const versionCommands = {
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
