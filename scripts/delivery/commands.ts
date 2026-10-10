import { buildCommands } from "./build-commands.ts";
import { publicationCommands } from "./publication-commands.ts";
import { releaseCommands } from "./release/commands.ts";
import { versionCommands } from "./version-commands.ts";
import { ciCommands } from "./ci/commands.ts";
import { pipelineCommands } from "./ci/pipeline-commands.ts";

export const deliveryCommands = { ...buildCommands, ...publicationCommands, ...releaseCommands, ...versionCommands, ...pipelineCommands, ...ciCommands } as const;
export type DeliveryCommand = keyof typeof deliveryCommands;
