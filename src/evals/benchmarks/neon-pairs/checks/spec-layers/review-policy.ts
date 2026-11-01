import { Method, type Clause, type LayerId } from "./requirements.ts";

export const SpokenJourneyRequirement = "I17";

export function reviewReason(clause: Clause, _layer: LayerId): string | undefined {
  return clause.method === Method.review ? "The inventory assigns this clause to cited review. Automated outcomes give it no credit." : undefined;
}
