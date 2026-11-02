import type { DefinitionId } from "./definitions.ts";

/** The coding agent whose older output counts were summed from stream values. */
export const ClaudeTool = "claude";

export const TokenNote = "Claude output tokens recorded before the telemetry fix on 2 November 2026 are undercounted, because the old telemetry summed stream values. Runs recorded after the fix are correct.";

/** Facts the page adds to a definition snippet, shown in its dialog. */
export const DefinitionNotes: Partial<Readonly<Record<DefinitionId, string>>> = {
  tokens: `Most of a total is cached input, because every request sends the whole context again. New input is input the provider hadn't cached, and output is what the agent wrote. ${TokenNote}`,
};
