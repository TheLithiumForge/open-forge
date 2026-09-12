import path from "node:path";

export type AgentModelPolicy = {
  model?: string;
  model_reasoning_effort?: string;
};

export type AgentModelPolicyMap = Record<string, AgentModelPolicy>;

export class AgentModelPolicyError extends Error {
  override readonly name = "AgentModelPolicyError";
}

const ReasoningEffortKeys = ["reasoningEffort", "reasoning_effort", "modelReasoningEffort", "model_reasoning_effort"] as const;

function yamlScalar(raw: string): string {
  const value = raw
    .trim()
    .replace(/\s+#.*$/, "")
    .trim();
  if ((value.startsWith('"') && value.endsWith('"')) || (value.startsWith("'") && value.endsWith("'"))) return value.slice(1, -1);
  return value;
}

function frontmatterValue(frontmatter: string, keys: readonly string[]): string | undefined {
  for (const key of keys) {
    const escaped = key.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    const match = frontmatter.match(new RegExp(`^${escaped}\\s*:\\s*(.+)$`, "m"));
    if (match?.[1] !== undefined) return yamlScalar(match[1]);
  }
  return undefined;
}

export function parseApmAgentModelPolicy(file: string, text: string): readonly [string, AgentModelPolicy] | undefined {
  const frontmatter = text.match(/^---\s*\r?\n([\s\S]*?)\r?\n---\s*(?:\r?\n|$)/)?.[1];
  if (frontmatter === undefined) return undefined;

  const modelValue = frontmatterValue(frontmatter, ["model"]);
  const effortValue = frontmatterValue(frontmatter, ReasoningEffortKeys);
  if (!modelValue && !effortValue) return undefined;

  const name = frontmatterValue(frontmatter, ["name"]) || path.basename(file).replace(/\.agent\.md$/, "");
  const policy: AgentModelPolicy = {};
  if (modelValue) policy.model = modelValue.startsWith("openai/") ? modelValue.slice("openai/".length) : modelValue;
  if (effortValue) {
    const effort = effortValue.trim().toLowerCase();
    if (!effort) throw new AgentModelPolicyError(`${file}: reasoning effort must not be empty.`);
    policy.model_reasoning_effort = effort;
  }
  return [name, policy];
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

export function parseAgentModelOverrides(file: string, text: string): AgentModelPolicyMap {
  const value: unknown = JSON.parse(text);
  if (!isRecord(value)) throw new AgentModelPolicyError(`${file} must contain a JSON object keyed by agent name.`);

  const policies: AgentModelPolicyMap = {};
  for (const [name, policyValue] of Object.entries(value)) {
    if (!isRecord(policyValue)) throw new AgentModelPolicyError(`Invalid policy for ${name}.`);
    const model = policyValue["model"];
    const effort = policyValue["model_reasoning_effort"];
    if (model !== undefined && (typeof model !== "string" || !model.trim())) {
      throw new AgentModelPolicyError(`${name}.model must be a non-empty string.`);
    }
    if (effort !== undefined && (typeof effort !== "string" || !effort.trim())) {
      throw new AgentModelPolicyError(`${name}.model_reasoning_effort must be a non-empty string.`);
    }
    const policy: AgentModelPolicy = {};
    if (typeof model === "string") policy.model = model;
    if (typeof effort === "string") policy.model_reasoning_effort = effort;
    policies[name] = policy;
  }
  return policies;
}

export function mergeAgentModelPolicies(base: AgentModelPolicyMap, overrides: AgentModelPolicyMap): AgentModelPolicyMap {
  const policies: AgentModelPolicyMap = { ...base };
  for (const [name, policy] of Object.entries(overrides)) policies[name] = { ...(policies[name] ?? {}), ...policy };
  return policies;
}
