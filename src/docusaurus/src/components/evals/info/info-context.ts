import { createContext, useContext } from "react";
import type { Batch } from "../records/records.types.ts";
import type { DefinitionId } from "./definitions.ts";

/** A label the reader clicked. A row's batch adds what that setup meant in that batch. */
export interface InfoRequest {
  readonly id: DefinitionId;
  readonly batch?: Batch | undefined;
}

export const InfoContext = createContext<(request: InfoRequest) => void>(() => undefined);

export function useInfo(): (request: InfoRequest) => void {
  return useContext(InfoContext);
}
