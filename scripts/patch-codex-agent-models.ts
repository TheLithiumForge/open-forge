#!/usr/bin/env node

import { access, readdir, readFile, writeFile } from "node:fs/promises";
import path from "node:path";

type AgentPolicy = {
  model?: string;
  model_reasoning_effort?: string;
};

type PolicyMap = Record<string, AgentPolicy>;

type Options = {
  apmAgentsDir: string;
  codexAgentsDir: string;
  overridesPath: string;
  checkOnly: boolean;
};

function fail(message: string): never {
  console.error(message);
  process.exit(2);
}

function parseArgs(argv: string[]): Options {
  let apmAgentsDir = ".apm/agents";
  let codexAgentsDir = ".codex/agents";
  let overridesPath = ".apm/codex-agent-models.json";
  let checkOnly = false;

  for (let i = 0; i < argv.length; i++) {
    const arg = argv[i];
    const next = () => argv[++i] ?? fail(`Missing value after ${arg}`);

    if (arg === "--apm-agents") apmAgentsDir = next();
    else if (arg === "--codex-agents") codexAgentsDir = next();
    else if (arg === "--overrides") overridesPath = next();
    else if (arg === "--check") checkOnly = true;
    else if (arg === "--help" || arg === "-h") {
      console.log(`Usage:
  node --experimental-strip-types scripts/patch-codex-agent-models.ts [options]

Options:
  --apm-agents <dir>     Default: .apm/agents
  --codex-agents <dir>   Default: .codex/agents
  --overrides <file>     Default: .apm/codex-agent-models.json
  --check                Verify only, do not write

Behavior:
  1. Reads name/model/reasoning effort from .apm/agents/**/*.agent.md.
     Supported effort keys:
       reasoningEffort
       reasoning_effort
       modelReasoningEffort
       model_reasoning_effort
  2. Strips an OpenCode-style "openai/" prefix from model names.
  3. Optionally merges Codex-only overrides from .apm/codex-agent-models.json.
     JSON overrides win over .agent.md frontmatter.
  4. Patches model/model_reasoning_effort into generated .codex/agents/**/*.toml.
`);
      process.exit(0);
    } else fail(`Unknown argument: ${arg}`);
  }

  return {
    apmAgentsDir: path.resolve(apmAgentsDir),
    codexAgentsDir: path.resolve(codexAgentsDir),
    overridesPath: path.resolve(overridesPath),
    checkOnly,
  };
}

async function exists(file: string): Promise<boolean> {
  try {
    await access(file);
    return true;
  } catch {
    return false;
  }
}

async function collectFiles(dir: string, suffix: string): Promise<string[]> {
  const entries = await readdir(dir, { withFileTypes: true });
  const out: string[] = [];

  for (const entry of entries) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) out.push(...(await collectFiles(full, suffix)));
    else if (entry.isFile() && entry.name.endsWith(suffix)) out.push(full);
  }

  return out.sort();
}

function yamlScalar(raw: string): string {
  const value = raw
    .trim()
    .replace(/\s+#.*$/, "")
    .trim();

  if ((value.startsWith('"') && value.endsWith('"')) || (value.startsWith("'") && value.endsWith("'"))) {
    return value.slice(1, -1);
  }

  return value;
}

function frontmatterValue(fm: string, keys: string[]): string | undefined {
  for (const key of keys) {
    const escaped = key.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    const match = fm.match(new RegExp(`^${escaped}\\s*:\\s*(.+)$`, "m"));
    if (match?.[1] !== undefined) return yamlScalar(match[1]);
  }

  return undefined;
}

function apmFrontmatterPolicy(file: string, text: string): [string, AgentPolicy] | undefined {
  const fm = text.match(/^---\s*\r?\n([\s\S]*?)\r?\n---\s*(?:\r?\n|$)/)?.[1];
  if (!fm) return undefined;

  const nameValue = frontmatterValue(fm, ["name"]);
  const modelValue = frontmatterValue(fm, ["model"]);
  const effortValue = frontmatterValue(fm, ["reasoningEffort", "reasoning_effort", "modelReasoningEffort", "model_reasoning_effort"]);

  if (!modelValue && !effortValue) return undefined;

  const fallbackName = path.basename(file).replace(/\.agent\.md$/, "");
  const name = nameValue || fallbackName;
  const policy: AgentPolicy = {};

  if (modelValue) {
    let model = modelValue;
    if (model.startsWith("openai/")) model = model.slice("openai/".length);
    policy.model = model;
  }

  if (effortValue) {
    const effort = effortValue.trim().toLowerCase();
    if (!effort) fail(`${file}: reasoning effort must not be empty.`);
    policy.model_reasoning_effort = effort;
  }

  return [name, policy];
}

async function loadApmPolicies(dir: string): Promise<PolicyMap> {
  if (!(await exists(dir))) return {};

  const policies: PolicyMap = {};
  for (const file of await collectFiles(dir, ".agent.md")) {
    const parsed = apmFrontmatterPolicy(file, await readFile(file, "utf8"));
    if (parsed) policies[parsed[0]] = parsed[1];
  }

  return policies;
}

async function loadOverrides(file: string): Promise<PolicyMap> {
  if (!(await exists(file))) return {};

  const value = JSON.parse(await readFile(file, "utf8")) as unknown;
  if (!value || typeof value !== "object" || Array.isArray(value)) {
    fail(`${file} must contain a JSON object keyed by agent name.`);
  }

  const policies = value as PolicyMap;
  for (const [name, policy] of Object.entries(policies)) {
    if (!policy || typeof policy !== "object" || Array.isArray(policy)) {
      fail(`Invalid policy for ${name}.`);
    }

    if (policy.model !== undefined && (typeof policy.model !== "string" || !policy.model.trim())) {
      fail(`${name}.model must be a non-empty string.`);
    }

    if (policy.model_reasoning_effort !== undefined && (typeof policy.model_reasoning_effort !== "string" || !policy.model_reasoning_effort.trim())) {
      fail(`${name}.model_reasoning_effort must be a non-empty string.`);
    }
  }

  return policies;
}

function mergePolicies(base: PolicyMap, overrides: PolicyMap): PolicyMap {
  const out: PolicyMap = { ...base };

  for (const [name, policy] of Object.entries(overrides)) {
    out[name] = { ...(out[name] ?? {}), ...policy };
  }

  return out;
}

function parseTomlString(raw: string): string {
  try {
    return JSON.parse(`"${raw}"`);
  } catch {
    return raw.replace(/\\"/g, '"').replace(/\\\\/g, "\\");
  }
}

function tomlValue(text: string, key: string): string | undefined {
  const match = text.match(new RegExp(`^${key}\\s*=\\s*"((?:\\\\.|[^"\\\\])*)"\\s*$`, "m"));
  return match ? parseTomlString(match[1]) : undefined;
}

function upsert(text: string, key: string, value: string): string {
  const line = `${key} = ${JSON.stringify(value)}`;
  const current = new RegExp(`^${key}\\s*=.*$`, "m");

  if (current.test(text)) return text.replace(current, line);

  const lines = text.split(/\r?\n/);
  let anchor = -1;

  if (key === "model_reasoning_effort") {
    anchor = lines.findIndex((line) => /^model\s*=/.test(line));
  }

  if (anchor < 0) {
    anchor = Math.max(
      lines.findIndex((line) => /^name\s*=/.test(line)),
      lines.findIndex((line) => /^description\s*=/.test(line)),
    );
  }

  lines.splice(anchor >= 0 ? anchor + 1 : 0, 0, line);
  return lines.join("\n");
}

async function main() {
  const options = parseArgs(process.argv.slice(2));

  const apmPolicies = await loadApmPolicies(options.apmAgentsDir);
  const overrides = await loadOverrides(options.overridesPath);
  const policies = mergePolicies(apmPolicies, overrides);

  if (Object.keys(policies).length === 0) {
    fail("No per-agent model policies found. Add model:/reasoningEffort: to .apm/agents/*.agent.md or create .apm/codex-agent-models.json.");
  }

  if (!(await exists(options.codexAgentsDir))) {
    fail(`Missing ${options.codexAgentsDir}. Run APM for the Codex target first.`);
  }

  const found = new Set<string>();
  const mismatches: string[] = [];
  let changed = 0;

  for (const file of await collectFiles(options.codexAgentsDir, ".toml")) {
    const original = await readFile(file, "utf8");
    const name = tomlValue(original, "name");

    if (!name || !policies[name]) continue;

    const policy = policies[name];
    found.add(name);

    if (options.checkOnly) {
      for (const key of ["model", "model_reasoning_effort"] as const) {
        const expected = policy[key];
        if (expected !== undefined && tomlValue(original, key) !== expected) {
          mismatches.push(`${name}: ${key} expected ${JSON.stringify(expected)}, got ${JSON.stringify(tomlValue(original, key))}`);
        }
      }
      continue;
    }

    let updated = original;

    if (policy.model) {
      updated = upsert(updated, "model", policy.model);
    }

    if (policy.model_reasoning_effort) {
      updated = upsert(updated, "model_reasoning_effort", policy.model_reasoning_effort);
    }

    if (updated !== original) {
      await writeFile(file, updated, "utf8");
      changed++;
      console.log(`patched ${path.relative(process.cwd(), file)} (${name})`);
    }
  }

  const unmatchedOverrides = Object.keys(overrides).filter((name) => !found.has(name));

  if (unmatchedOverrides.length) {
    fail(`Codex override names not found in generated agents: ${unmatchedOverrides.join(", ")}`);
  }

  if (options.checkOnly) {
    if (mismatches.length) {
      console.error(mismatches.join("\n"));
      process.exitCode = 1;
    } else {
      console.log(`OK: ${found.size} generated Codex agent model policies match.`);
    }
  } else {
    console.log(`Done: ${changed} file(s) patched, ${found.size} configured agent(s) matched.`);
  }
}

main().catch((error) => {
  console.error(error instanceof Error ? error.message : String(error));
  process.exitCode = 1;
});
