// Keep this flow aligned with Tags And Loading and Frontmatter in the loader.
export const diagramLabel = "How context loads, from startup through file conditions to refresh";
export const diagramTitle = "How loading works";
export const diagramCaption = "Follow only selected scopes. Loading tags and matching patterns never open an unselected ancestor or scope.";

export interface LoadingStep {
  readonly title: string;
  readonly text: string;
  readonly note?: string;
  readonly gate?: boolean;
  readonly links?: readonly { readonly label: string; readonly href: string }[];
}

export const loadingSteps: readonly LoadingStep[] = [
  {
    title: "Start with the loader",
    text: "Read AGENTS.md, then the loader. Begin with the entries it lists.",
  },
  {
    title: "Would this entry load?",
    text: "#LoadNow and #KeepInMind entries load when their parent loads. Untagged entries stay on demand unless the task selects them or a loaded rule requires reading them.",
  },
  {
    title: "Check applyTo",
    text: "No conditions? Continue. Otherwise, the file loads, refreshes, and applies only while one working file matches its own patterns and every ancestor's patterns. applyTo never loads a file by itself.",
    note: "No match: the file doesn't apply, and its children don't load automatically. You can still open it to read it. Working files not known yet: conditioned #LoadNow and #KeepInMind entries stay pending.",
    gate: true,
    links: [{ label: "Working files", href: "/docs/glossary#working-file" }],
  },
  {
    title: "Read and follow children",
    text: "Read the file, then its overwrite companion if it has one. If it's an entrypoint, go through its entries in listed order, starting again at step 2.",
    note: "Follow required-read instructions in the source. For example, the Skills entrypoint requires reading open-forge-cli/SKILL.md.",
    links: [{ label: "Overwrite companions", href: "/docs/glossary#overwrite-companion" }],
  },
  {
    title: "Refresh active context",
    text: "#KeepInMind content is read again at task start or resume, after context restoration, and before handoff or closeout. Only active scopes refresh, and the applyTo check from step 3 still applies.",
  },
];
