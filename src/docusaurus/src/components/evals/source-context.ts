import { createContext, useContext } from "react";
import { commitSource, SourceKind, type Source } from "./data/sources.ts";

/** The source of the shown records, so any view can link their evidence files. */
export const SourceContext = createContext<Source>(commitSource(SourceKind.published, "Published copy", "unknown"));

export function useSource(): Source {
  return useContext(SourceContext);
}
