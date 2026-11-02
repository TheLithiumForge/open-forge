import type { Plugin } from "@docusaurus/types";
import { loadEvalsData } from "./load-content.ts";
import type { EvalsData } from "../../src/components/evals/data/snapshot.types.ts";
export const EvalsPluginName = "open-forge-evals";
// The build reads the results submodule. Other data, such as the fixture preview, shows through npm run evals -- site --repository.
export default function evals(): Plugin<EvalsData> {
  return {
    name: EvalsPluginName,
    async loadContent() { return loadEvalsData(); },
    async contentLoaded({ content, actions }) { actions.setGlobalData(content); },
  };
}
