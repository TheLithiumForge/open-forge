import { buildCommands } from "./build-commands.ts";
import { publicationCommands } from "./publication-commands.ts";
import { releaseCommands } from "./release/commands.ts";
import { versionCommands } from "./version-commands.ts";
import { ciCommands } from "./ci/commands.ts";
import { pipelineCommands } from "./ci/pipeline-commands.ts";

export const deliveryCommands = {
  ...buildCommands,
  ...publicationCommands,
  ...releaseCommands,
  ...versionCommands,
  ...pipelineCommands,
  ...ciCommands,
  "smoke:aliases": {
    script: "aliases/smoke-command.ts",
    options: ["from", "rid", "version"],
    description: "Install packed alias and wheel locally and run their version command.",
    details: ["Uses local tarballs and a temporary Python venv. Skips the wheel only when Python is unavailable. Defaults to this host's pack output."],
    examples: ["smoke:aliases", "smoke:aliases --from artifacts/delivery/win-x64/packages"],
  },
  "release:pypi": {
    script: "pypi/prepare-release.ts",
    options: ["from"],
    description: "Validate collected wheels and prepare artifacts/pypi for trusted publishing.",
    details: ["Requires matching committed release source and all selected wheels. Does not contact PyPI."],
    examples: ["release:pypi --from artifacts/release"],
  },
} as const;
export type DeliveryCommand = keyof typeof deliveryCommands;
