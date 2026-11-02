import { resolve } from "node:path";
import type { LoadContext, Plugin } from "@docusaurus/types";
import { loadEvalsData } from "./load-content.ts";
import { resultsRoot } from "./prepare.ts";
import type { EvalsData } from "../../src/components/evals/data/snapshot.types.ts";
export const EvalsPluginName = "open-forge-evals";
export default function evals(context: LoadContext): Plugin<EvalsData> {
  return {
    name: EvalsPluginName,
    async loadContent() {
      const records = process.env["RESULTS_RECORDS"];
      return loadEvalsData(resultsRoot, records === undefined ? undefined : resolve(context.siteDir, records));
    },
    async contentLoaded({ content, actions }) { actions.setGlobalData(content); },
  };
}
