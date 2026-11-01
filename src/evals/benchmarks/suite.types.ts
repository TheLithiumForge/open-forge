import type { JudgedBy, RubricArea } from "./suite-values.ts";

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
}
