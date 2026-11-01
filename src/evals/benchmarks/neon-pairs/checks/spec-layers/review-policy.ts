import { LayerTwoCapReview } from "./cap-policy.ts";
import { LayerId, Method, type Clause } from "./requirements.ts";

export const SpokenJourneyRequirement = "I17";

export function reviewReason(clause: Clause, layer: LayerId): string | undefined {
  if (clause.requirement === SpokenJourneyRequirement) return "Accessibility-tree checks support names, focus and live-region payloads. The spec notes still require person confirmation of spoken output and a screen reader version. Tree evidence alone leaves this review pending.";
  if (layer === LayerId.two && LayerTwoCapReview[clause.id]) return LayerTwoCapReview[clause.id];
  return clause.method === Method.review ? "The inventory assigns this clause to cited review. Automated outcomes give it no credit." : undefined;
}
