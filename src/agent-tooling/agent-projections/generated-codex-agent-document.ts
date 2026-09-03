import type { AgentModelPolicy } from "./agent-model-policy.ts";

const ModelFields = ["model", "model_reasoning_effort"] as const;

function parseTomlString(raw: string): string {
  try {
    const value: unknown = JSON.parse(`"${raw}"`);
    if (typeof value === "string") return value;
  } catch {
    // Fall back to the same narrow unescaping used before the capability move.
  }
  return raw.replace(/\\"/g, '"').replace(/\\\\/g, "\\");
}

function tomlValue(text: string, key: string): string | undefined {
  const match = text.match(new RegExp(`^${key}\\s*=\\s*"((?:\\\\.|[^"\\\\])*)"\\s*$`, "m"));
  return match?.[1] === undefined ? undefined : parseTomlString(match[1]);
}

export function readGeneratedCodexAgentName(text: string): string | undefined {
  return tomlValue(text, "name");
}

function upsert(text: string, key: (typeof ModelFields)[number], value: string): string {
  const line = `${key} = ${JSON.stringify(value)}`;
  const current = new RegExp(`^${key}\\s*=.*$`, "m");
  if (current.test(text)) return text.replace(current, line);

  const lines = text.split(/\r?\n/);
  let anchor = key === "model_reasoning_effort" ? lines.findIndex((candidate) => /^model\s*=/.test(candidate)) : -1;
  if (anchor < 0) {
    anchor = Math.max(
      lines.findIndex((candidate) => /^name\s*=/.test(candidate)),
      lines.findIndex((candidate) => /^description\s*=/.test(candidate)),
    );
  }
  lines.splice(anchor >= 0 ? anchor + 1 : 0, 0, line);
  return lines.join("\n");
}

export function updateGeneratedCodexAgentDocument(text: string, policy: AgentModelPolicy): string {
  let updated = text;
  if (policy.model) updated = upsert(updated, "model", policy.model);
  if (policy.model_reasoning_effort) updated = upsert(updated, "model_reasoning_effort", policy.model_reasoning_effort);
  return updated;
}

export function generatedCodexAgentMismatches(name: string, text: string, policy: AgentModelPolicy): string[] {
  const mismatches: string[] = [];
  for (const key of ModelFields) {
    const expected = policy[key];
    const actual = tomlValue(text, key);
    if (expected !== undefined && actual !== expected) mismatches.push(`${name}: ${key} expected ${JSON.stringify(expected)}, got ${JSON.stringify(actual)}`);
  }
  return mismatches;
}
