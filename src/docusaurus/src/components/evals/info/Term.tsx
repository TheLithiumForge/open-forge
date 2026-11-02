import type { ReactNode } from "react";
import type { Batch } from "../records/records.types.ts";
import { Definitions, isDefinitionId } from "./definitions.ts";
import { useInfo } from "./info-context.ts";

/** A setup, starting point, case or measure label that opens its explanation. Unknown ids stay plain text. */
export function Term({ id, batch, children }: {
  readonly id: string;
  readonly batch?: Batch | undefined;
  readonly children?: ReactNode;
}) {
  const show = useInfo();
  if (!isDefinitionId(id)) return <>{children ?? id}</>;
  return <button type="button" className="eval-term" aria-haspopup="dialog"
    onClick={(event) => { event.stopPropagation(); show({ id, batch }); }}>
    {children ?? Definitions[id].title}
  </button>;
}
