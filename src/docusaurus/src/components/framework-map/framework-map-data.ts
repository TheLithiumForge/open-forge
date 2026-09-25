// Content of the "how it fits together" diagram. Keep it aligned with the
// loader's loading tags and the Core and Memory entrypoints. The site renders it
// directly. After changing it, run `npm run diagram` to regenerate the README SVGs.

export type Loading = "startup" | "listed" | "checkpoints" | "demand";

export const loadingLabels: Record<Loading, string> = {
  startup: "Loaded at startup",
  listed: "Listed at startup",
  checkpoints: "Re-checked at checkpoints",
  demand: "On demand",
};

// One short explanation per loading label, shown in the diagram's legend.
export const loadingNotes: Record<Loading, string> = {
  startup: "Read in full before the task begins.",
  listed: "One line per file. Each opens only when needed.",
  checkpoints: "Listed, then read again at resume and handoff.",
  demand: "Nothing is read until a task selects it.",
};

export const legendOrder: readonly Loading[] = ["startup", "listed", "checkpoints", "demand"];

export const childRouteNote = "Child routes stay closed: a folder or file below a loaded route opens only when the task needs it, unless its parent marks it to load.";

export interface CoreRole {
  readonly name: string;
  readonly role: string;
  readonly example: string;
  readonly loading: Loading;
}

export const coreRoles: readonly CoreRole[] = [
  { name: "Directives", role: "Rules that must be followed", example: "Run the tests before calling work done.", loading: "startup" },
  { name: "Guidance", role: "Advice for choices that keep coming up", example: "When to split a module in two.", loading: "listed" },
  { name: "Patterns", role: "Shapes worth repeating", example: "How an API error response looks.", loading: "listed" },
  { name: "Skills", role: "Capabilities the agent can use", example: "A SKILL.md package, like use-workflow.", loading: "listed" },
  { name: "Templates", role: "Starting files to copy", example: "A Decision or a Task starter.", loading: "demand" },
  { name: "Maps", role: "Where important sources live", example: "Your ADRs, wiki, or API docs.", loading: "listed" },
];

export interface MemoryState {
  readonly name: string;
  readonly tense: string;
  readonly holds: string;
  readonly loading: Loading;
  readonly next?: string;
}

export const memoryStates: readonly MemoryState[] = [
  { name: "Working", tense: "Now", holds: "Where the current work stands: checkpoints and handoffs.", loading: "listed", next: "Worth keeping, not settled" },
  { name: "Emerging", tense: "Maybe", holds: "Findings, ideas, and analysis nobody has accepted yet.", loading: "checkpoints", next: "Accepted" },
  { name: "Crystallized", tense: "True now", holds: "Decisions and current documents: what holds, and why.", loading: "listed", next: "Replaced" },
  { name: "Archived", tense: "History", holds: "Kept for reference. No longer in charge.", loading: "demand" },
];

export const taskSteps: readonly string[] = ["Read the loader", "Follow the routes the task needs", "Do the work", "Save what's worth keeping"];
