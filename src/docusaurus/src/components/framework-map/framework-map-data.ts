// Content of the "how it fits together" diagram. Keep it aligned with the
// loader's loading tags, the Core and Memory entrypoints, and what each
// Extension installs. The site renders it directly. After changing it, run
// `npm run diagram` to regenerate the README SVGs.
//
// The diagram uses two states. Anything read at startup is tinted with a solid
// outline. Anything opened on demand is unfilled with a dashed outline. Text in
// backticks renders as code.

export type Loading = "startup" | "demand";

export const loadingLabels: Record<Loading, string> = {
  startup: "Entrypoint at startup",
  demand: "Entrypoint on demand",
};

// The pitch above the diagram, and what a fresh install reads.
export const pitch =
  "At startup your agent reads its instructions, most main folder indexes, and the files their tags or rules require. Everything else opens when a task needs it.";
export const startupFootprint = "A fresh install reads 12 files at startup, about 9.6k estimated tokens, including the CLI guide. What you add can change the count.";

// Shown between the loader and the panels, before the first card.
export const entrypointNote =
  "An entrypoint is a folder's index file: its purpose, its rules, and one line per entry. Linked files open when selected, tagged to load, or required by a loaded rule.";

// A row in a card's lower half: what the entrypoint links to, and when it's read.
export interface FileRow {
  readonly text: string;
  readonly loading: Loading;
}

export const linkedFiles: FileRow = { text: "Linked files: when selected or required", loading: "demand" };

export interface CoreRole {
  readonly name: string;
  readonly role: string;
  readonly example: string;
  readonly loading: Loading;
  // Replaces the standard linked-files row when a category loads some files differently.
  readonly files?: readonly FileRow[];
  // What ships in the category, or what optional Extensions add to it.
  readonly contents?: string;
}

export const coreRoles: readonly CoreRole[] = [
  {
    name: "Directives",
    role: "Rules that must be followed",
    example: "For example: run the tests before calling work done.",
    loading: "startup",
    files: [
      { text: "Root rules: at startup", loading: "startup" },
      { text: "Scoped rules: when their scope is selected", loading: "demand" },
    ],
  },
  {
    name: "Guidance",
    role: "Advice for choices that keep coming up",
    example: "For example: when to split a module.",
    loading: "startup",
    contents: "Extensions add Adaptive Collaboration.",
  },
  {
    name: "Patterns",
    role: "Reusable shapes that make work easy to create and check",
    example: "For example: the shape of an API error.",
    loading: "startup",
    contents: "Extensions add Work Records.",
  },
  {
    name: "Skills",
    role: "Capabilities packaged as SKILL.md",
    example: "For example: a code review Skill.",
    loading: "startup",
    contents: "Starts with open-forge-cli. Extensions add use-workflow.",
    files: [
      { text: "CLI usage Skill: required at startup", loading: "startup" },
      { text: "Other Skills: when selected", loading: "demand" },
    ],
  },
  {
    name: "Templates",
    role: "Starter files to copy and adapt",
    example: "For example: a Decision starter.",
    loading: "demand",
    contents: "Most Extensions add starters.",
  },
  { name: "Maps", role: "Pointers to important sources", example: "For example: your ADRs, wiki, or API docs.", loading: "startup" },
];

// The Memory entrypoint itself, shown under the panel title.
export const memoryLoading: Loading = "startup";

export interface MemoryState {
  readonly name: string;
  readonly tense: string;
  readonly holds: string;
  readonly loading: Loading;
  // A loading fact that applies only to this state.
  readonly loadingNote?: string;
  readonly contents?: string;
  readonly next?: string;
}

export const memoryStates: readonly MemoryState[] = [
  {
    name: "Working",
    tense: "Now",
    holds: "Temporary state for work in progress.",
    loading: "startup",
    contents: "Extensions add Checkpoints and Handoffs.",
    next: "Worth keeping, not settled",
  },
  {
    name: "Emerging",
    tense: "Maybe",
    holds: "Useful material nobody has accepted yet.",
    loading: "startup",
    loadingNote: "Also re-read at refresh points.",
    contents: "Extensions add Ideas, Analysis, and Observations.",
    next: "Accepted",
  },
  {
    name: "Crystallized",
    tense: "True now",
    holds: "Accepted knowledge that stays current.",
    loading: "startup",
    contents: "Extensions add Decisions and Documents.",
    next: "Replaced",
  },
  { name: "Archived", tense: "History", holds: "Kept for reference. It no longer governs current work.", loading: "demand" },
];

export const memoryNote = "A record can skip states. An accepted choice can go straight to Crystallized.";

// Every task. The first two steps repeat the two loading styles.
export interface TaskStep {
  readonly text: string;
  readonly loading?: Loading;
}

export const taskSteps: readonly TaskStep[] = [
  { text: "Read the startup files", loading: "startup" },
  { text: "Open what the task needs", loading: "demand" },
  { text: "Do the work" },
  { text: "Save what's worth keeping" },
];

export interface EntryNode {
  readonly file: string;
  readonly note: string;
}

export const entryNodes: readonly [EntryNode, EntryNode] = [
  { file: "AGENTS.md or CLAUDE.md", note: "Your agent tool reads it automatically. It points to the loader." },
  { file: ".agents/loader.md", note: "Read next, in full: the loading rules and the main folders." },
];

// Captions below the diagram.
export const loadingGuide = {
  label: "How loading works",
  href: "/docs/concepts/loading-and-tags",
} as const;

export const captions: readonly string[] = [
  "Linked files open when selected or required. `#LoadNow` reads a file when its parent index loads; `#KeepInMind` also re-reads it at task start or resume, after a context restore, and before handoff or closeout while its scope is active. Both tags act through a loaded parent. The Skills rules explicitly require the CLI usage guide at startup.",
  "A file with `applyTo` patterns applies only while the task works on a matching file.",
  "`open-forge context skills/open-forge-cli` batches the default loading-tag closure and the required CLI guide. Extensions are optional packages you add.",
];

// The README image's alt text and the SVG title. Keep them identical.
export const diagramDescription =
  "How Open Forge fits together. At startup the agent reads AGENTS.md or CLAUDE.md, the loader, the indexes of Directives, Guidance, Patterns, Skills, Maps, Memory, Working, Emerging, and Crystallized, and the CLI usage Skill explicitly required by the Skills index. Other linked files open when selected, tagged to load, or required by a loaded rule. Templates and Archived open on demand. A file with applyTo patterns applies only while the task works on a matching file. Core holds six categories for how work gets done. Memory holds four states for what's worth remembering. Cards include examples of optional Extension content, such as Checkpoints and Decisions.";
