import { CiCommand } from "./command-names.ts";

export const ciCommands = {
  [CiCommand.prepare]: {
    script: "ci/prepare.ts",
    options: [],
    description: "Prepare isolated .NET and macOS runner paths.",
    details: ["Writes DOTNET_INSTALL_DIR, DOTNET_ROOT and macOS TMPDIR to GITHUB_ENV. Without GITHUB_ENV, prints assignments only."],
    examples: ["ci:prepare"],
  },
  [CiCommand.bundle]: {
    script: "ci/bundle.ts",
    options: ["rid"],
    description: "Bundle the published CLI binary and its checksum.",
    details: ["Requires --rid and existing native publish output. Writes artifacts/downloads/<RID>. Requires tar on PATH."],
    examples: ["ci:bundle --rid win-x64"],
  },
  [CiCommand.watch]: {
    script: "ci/watch.ts",
    options: ["sha", "run"],
    description: "Watch GitHub jobs for a commit or workflow run.",
    details: ["Supply --sha <commit> or --run <id>. Uses public GitHub REST requests and GITHUB_TOKEN only when set."],
    examples: ["ci:watch --sha <commit>", "ci:watch --run 38043270935"],
  },
  [CiCommand.smoke]: {
    script: "ci/smoke-public.ts",
    options: ["version"],
    description: "Check an exact published npm version in a fresh workspace.",
    details: ["Requires --version <x.y.z-pre>. Contacts npm and installs into a new temporary directory."],
    examples: ["smoke:public --version 0.9.0-beta.11"],
  },
  [CiCommand.wsl]: {
    script: "ci/gate-wsl.ts",
    options: ["commit"],
    description: "Run the managed test tiers for a commit inside WSL.",
    details: ["Windows only. Requires WSL with Node >=22.18, Git and .NET on PATH. Uses a reusable home clone and writes host logs under artifacts/delivery/logs/wsl/<commit>."],
    examples: ["gate:wsl --commit <sha>"],
  },
} as const;
