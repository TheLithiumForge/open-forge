export const publicationCommands = {
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
      "Existing versions warn and skip without retagging. Prerelease versions cannot use latest, but become latest until the first stable release.",
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
      "Existing matching versions warn and skip. Different published dependencies require a new version. Prereleases cannot use latest, but become latest until the first stable release.",
    ],
    examples: ["publish:wrapper --tag preview --dry-run"],
  },
} as const;
