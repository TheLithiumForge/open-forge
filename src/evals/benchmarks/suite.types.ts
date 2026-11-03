import type { WorkMode } from "../config/choices.ts";
import type { JudgedBy, RubricArea } from "./suite-values.ts";
import type { DocumentDelivery } from "../materialize/delivery.ts";
import type { BrownfieldInputs } from "../materialize/brownfield-inputs.ts";

/** Something the user wants but didn't say. Only the simulated user, the checks and the judges see it. */
export interface IntentItem {
  readonly id: string;
  readonly text: string;
  readonly judgedBy: JudgedBy;
}

export interface RubricQuestion {
  readonly id: string;
  readonly area: RubricArea;
  readonly question: string;
}

export interface SetupDefinition {
  readonly label: string;
  /** Open Forge CLI arguments, run in order in an empty project folder. */
  readonly commands: readonly (readonly string[])[];
  readonly delivery?: DocumentDelivery;
  /** How the agent works. Absent means one agent, with Codex's subagents switched off. */
  readonly workMode?: WorkMode;
}

export interface Suite {
  readonly id: string;
  readonly title: string;
  readonly project: string;
  readonly baseline: string;
  readonly setups: Readonly<Record<string, SetupDefinition>>;
  readonly prompt: string;
  readonly intent: readonly IntentItem[];
  readonly rubric: readonly RubricQuestion[];
  /** Changes whenever the prompt, the hidden intent, the rubric or the checks change. */
  readonly fingerprint: string;
  readonly intentEdited: boolean;
  readonly specs?: string;
  readonly documents?: readonly string[];
  readonly runMinutes?: number;
  readonly comparisons?: Readonly<Record<string, readonly string[]>>;
  readonly brownfield?: BrownfieldInputs;
}
