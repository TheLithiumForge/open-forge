import type { ComponentType } from "react";
import { StartingPoints, type StartingPointId } from "../data/cells.ts";
import { CaseNames, MeasureNames, SetupNames, type CaseId, type MeasureId, type SetupId } from "../data/names.ts";
import Idea from "@site/docs/evals/_definitions/idea.mdx";
import Brief from "@site/docs/evals/_definitions/brief.mdx";
import FullSpec from "@site/docs/evals/_definitions/full-spec.mdx";
import ExistingApp from "@site/docs/evals/_definitions/existing-app.mdx";
import Feature from "@site/docs/evals/_definitions/l4-f01.mdx";
import Repair from "@site/docs/evals/_definitions/l4-r01.mdx";
import FeatureRepair from "@site/docs/evals/_definitions/l4-fr01.mdx";
import BestScore from "@site/docs/evals/_definitions/l4-r02.mdx";
import ArrowKeys from "@site/docs/evals/_definitions/l4-r03.mdx";
import ReducedMotion from "@site/docs/evals/_definitions/l4-r04.mdx";
import Silence from "@site/docs/evals/_definitions/l4-r05.mdx";
import NoSetup from "@site/docs/evals/_definitions/no-setup.mdx";
import PlainDocs from "@site/docs/evals/_definitions/plain-docs.mdx";
import OrganizedDocs from "@site/docs/evals/_definitions/organized-docs.mdx";
import LinkedAgents from "@site/docs/evals/_definitions/linked-agents.mdx";
import SpecInPrompt from "@site/docs/evals/_definitions/spec-in-prompt.mdx";
import CodeOnly from "@site/docs/evals/_definitions/code-only.mdx";
import Minimal from "@site/docs/evals/_definitions/minimal.mdx";
import Essentials from "@site/docs/evals/_definitions/essentials.mdx";
import FullCore from "@site/docs/evals/_definitions/full-core.mdx";
import FullCoreScoped from "@site/docs/evals/_definitions/full-core-scoped.mdx";
import CheckCoverage from "@site/docs/evals/_definitions/check-coverage.mdx";
import JudgeScore from "@site/docs/evals/_definitions/judge-score.mdx";
import Tokens from "@site/docs/evals/_definitions/tokens.mdx";
import Time from "@site/docs/evals/_definitions/time.mdx";

/** The concept pages, whose heading anchors equal the definition ids. */
export const ConceptPages = { setups: "/docs/evals/setups", startingPoints: "/docs/evals/starting-points", measures: "/docs/evals/measures" } as const;

export type DefinitionId = StartingPointId | CaseId | SetupId | MeasureId;
export const DefinitionKind = { startingPoint: "startingPoint", setup: "setup", measure: "measure" } as const;
export type DefinitionKind = (typeof DefinitionKind)[keyof typeof DefinitionKind];

export interface Definition {
  readonly title: string;
  readonly kind: DefinitionKind;
  readonly Content: ComponentType;
}

const startingPoint = (id: StartingPointId) => StartingPoints.find((point) => point.id === id)?.title ?? id;

/** Each snippet in docs/evals/_definitions, shown when a reader clicks a label. */
export const Definitions: Readonly<Record<DefinitionId, Definition>> = {
  idea: { title: startingPoint("idea"), kind: DefinitionKind.startingPoint, Content: Idea },
  brief: { title: startingPoint("brief"), kind: DefinitionKind.startingPoint, Content: Brief },
  "full-spec": { title: startingPoint("full-spec"), kind: DefinitionKind.startingPoint, Content: FullSpec },
  "existing-app": { title: startingPoint("existing-app"), kind: DefinitionKind.startingPoint, Content: ExistingApp },
  "l4-f01": { title: CaseNames["l4-f01"], kind: DefinitionKind.startingPoint, Content: Feature },
  "l4-r01": { title: CaseNames["l4-r01"], kind: DefinitionKind.startingPoint, Content: Repair },
  "l4-fr01": { title: CaseNames["l4-fr01"], kind: DefinitionKind.startingPoint, Content: FeatureRepair },
  "l4-r02": { title: CaseNames["l4-r02"], kind: DefinitionKind.startingPoint, Content: BestScore },
  "l4-r03": { title: CaseNames["l4-r03"], kind: DefinitionKind.startingPoint, Content: ArrowKeys },
  "l4-r04": { title: CaseNames["l4-r04"], kind: DefinitionKind.startingPoint, Content: ReducedMotion },
  "l4-r05": { title: CaseNames["l4-r05"], kind: DefinitionKind.startingPoint, Content: Silence },
  "no-setup": { title: SetupNames["no-setup"], kind: DefinitionKind.setup, Content: NoSetup },
  "plain-docs": { title: SetupNames["plain-docs"], kind: DefinitionKind.setup, Content: PlainDocs },
  "organized-docs": { title: SetupNames["organized-docs"], kind: DefinitionKind.setup, Content: OrganizedDocs },
  "linked-agents": { title: SetupNames["linked-agents"], kind: DefinitionKind.setup, Content: LinkedAgents },
  "spec-in-prompt": { title: SetupNames["spec-in-prompt"], kind: DefinitionKind.setup, Content: SpecInPrompt },
  "code-only": { title: SetupNames["code-only"], kind: DefinitionKind.setup, Content: CodeOnly },
  minimal: { title: SetupNames.minimal, kind: DefinitionKind.setup, Content: Minimal },
  essentials: { title: SetupNames.essentials, kind: DefinitionKind.setup, Content: Essentials },
  "full-core": { title: SetupNames["full-core"], kind: DefinitionKind.setup, Content: FullCore },
  "full-core-scoped": { title: SetupNames["full-core-scoped"], kind: DefinitionKind.setup, Content: FullCoreScoped },
  "check-coverage": { title: MeasureNames["check-coverage"], kind: DefinitionKind.measure, Content: CheckCoverage },
  "judge-score": { title: MeasureNames["judge-score"], kind: DefinitionKind.measure, Content: JudgeScore },
  tokens: { title: MeasureNames.tokens, kind: DefinitionKind.measure, Content: Tokens },
  time: { title: MeasureNames.time, kind: DefinitionKind.measure, Content: Time },
};

const Pages: Readonly<Record<DefinitionKind, string>> = {
  [DefinitionKind.startingPoint]: ConceptPages.startingPoints, [DefinitionKind.setup]: ConceptPages.setups, [DefinitionKind.measure]: ConceptPages.measures,
};

export function readMore(id: DefinitionId): string {
  return `${Pages[Definitions[id].kind]}#${id}`;
}

export function isDefinitionId(id: string): id is DefinitionId {
  return Object.keys(Definitions).includes(id);
}
