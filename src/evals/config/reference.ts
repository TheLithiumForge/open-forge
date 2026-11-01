import { Isolation } from "./choices.ts";
import type { EvalConfig } from "./config.types.ts";
import { judgePreset, PresetName } from "./presets.ts";
import { DocumentCase } from "../materialize/delivery.ts";

export const ReferenceLayer = { two: 2, three: 3 } as const;
export function referenceOptions(layer = String(ReferenceLayer.three), judges: string = PresetName.claude) {
  if (layer !== String(ReferenceLayer.two) && layer !== String(ReferenceLayer.three)) throw new Error("Reference grading supports --layer 2 or 3.");
  return { layer: Number(layer), judges: judgePreset(judges) };
}
export type ReferenceOptions = ReturnType<typeof referenceOptions>;

export function referenceConfig(base: EvalConfig, options: ReferenceOptions): EvalConfig {
  const judge = options.judges[0];
  if (judge === undefined) throw new Error("Choose at least one reference judge.");
  const { cases: _cases, protocol: _protocol, ...config } = base;
  return { ...config, suite: options.layer === ReferenceLayer.two ? DocumentCase.two : DocumentCase.three,
    layer: options.layer, setups: [], agents: [], runsPerSetup: 1, parallelRuns: 1, isolation: Isolation.docker,
    development: false, judge, judges: options.judges };
}
