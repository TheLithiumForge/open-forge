#!/usr/bin/env node

import { access, readdir, readFile, writeFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { AgentModelPolicyError, mergeAgentModelPolicies, parseAgentModelOverrides, parseApmAgentModelPolicy, type AgentModelPolicyMap } from "./agent-model-policy.ts";
import { generatedCodexAgentMismatches, readGeneratedCodexAgentName, updateGeneratedCodexAgentDocument } from "./generated-codex-agent-document.ts";

type Options = { apmAgentsDir: string; codexAgentsDir: string; overridesPath: string; checkOnly: boolean };

const EntryPath = "scripts/agent-tooling/agent-projections/patch-codex-agent-models.ts";
const AgentSuffix = ".agent.md";
const GeneratedAgentSuffix = ".toml";
const RejectedExit = 2;

function fail(message: string): never {
  throw new AgentModelPolicyError(message);
}

function parseArgs(argv: readonly string[]): Options {
  let apmAgentsDir = ".apm/agents";
  let codexAgentsDir = ".codex/agents";
  let overridesPath = ".apm/codex-agent-models.json";
  let checkOnly = false;

  for (let index = 0; index < argv.length; index++) {
    const argument = argv[index];
    if (argument === undefined) break;
    const next = (): string => {
      const value = argv[index + 1];
      if (value === undefined) fail(`Missing value after ${argument}`);
      index++;
      return value;
    };
    if (argument === "--apm-agents") apmAgentsDir = next();
    else if (argument === "--codex-agents") codexAgentsDir = next();
    else if (argument === "--overrides") overridesPath = next();
    else if (argument === "--check") checkOnly = true;
    else if (argument === "--help" || argument === "-h") {
      console.log(`Usage:
  node --experimental-strip-types ${EntryPath} [options]

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
    } else fail(`Unknown argument: ${argument}`);
  }
  return { apmAgentsDir: path.resolve(apmAgentsDir), codexAgentsDir: path.resolve(codexAgentsDir), overridesPath: path.resolve(overridesPath), checkOnly };
}

async function exists(file: string): Promise<boolean> {
  try {
    await access(file);
    return true;
  } catch {
    return false;
  }
}

async function collectFiles(directory: string, suffix: string): Promise<string[]> {
  const files: string[] = [];
  for (const entry of await readdir(directory, { withFileTypes: true })) {
    const fullPath = path.join(directory, entry.name);
    if (entry.isDirectory()) files.push(...(await collectFiles(fullPath, suffix)));
    else if (entry.isFile() && entry.name.endsWith(suffix)) files.push(fullPath);
  }
  return files.sort();
}

async function loadApmPolicies(directory: string): Promise<AgentModelPolicyMap> {
  if (!(await exists(directory))) return {};
  const policies: AgentModelPolicyMap = {};
  for (const file of await collectFiles(directory, AgentSuffix)) {
    const parsed = parseApmAgentModelPolicy(file, await readFile(file, "utf8"));
    if (parsed !== undefined) policies[parsed[0]] = parsed[1];
  }
  return policies;
}

async function loadOverrides(file: string): Promise<AgentModelPolicyMap> {
  return (await exists(file)) ? parseAgentModelOverrides(file, await readFile(file, "utf8")) : {};
}

async function main(): Promise<void> {
  const options = parseArgs(process.argv.slice(2));
  const overrides = await loadOverrides(options.overridesPath);
  const policies = mergeAgentModelPolicies(await loadApmPolicies(options.apmAgentsDir), overrides);
  if (Object.keys(policies).length === 0) {
    fail("No per-agent model policies found. Add model:/reasoningEffort: to .apm/agents/*.agent.md or create .apm/codex-agent-models.json.");
  }
  if (!(await exists(options.codexAgentsDir))) fail(`Missing ${options.codexAgentsDir}. Run APM for the Codex target first.`);

  const found = new Set<string>();
  const mismatches: string[] = [];
  let changed = 0;
  for (const file of await collectFiles(options.codexAgentsDir, GeneratedAgentSuffix)) {
    const original = await readFile(file, "utf8");
    const name = readGeneratedCodexAgentName(original);
    if (name === undefined || policies[name] === undefined) continue;
    const policy = policies[name];
    found.add(name);
    if (options.checkOnly) mismatches.push(...generatedCodexAgentMismatches(name, original, policy));
    else {
      const updated = updateGeneratedCodexAgentDocument(original, policy);
      if (updated !== original) {
        await writeFile(file, updated, "utf8");
        changed++;
        console.log(`patched ${path.relative(process.cwd(), file)} (${name})`);
      }
    }
  }

  const unmatchedOverrides = Object.keys(overrides).filter((name) => !found.has(name));
  if (unmatchedOverrides.length > 0) fail(`Codex override names not found in generated agents: ${unmatchedOverrides.join(", ")}`);
  if (options.checkOnly && mismatches.length > 0) {
    console.error(mismatches.join("\n"));
    process.exitCode = 1;
  } else if (options.checkOnly) console.log(`OK: ${found.size} generated Codex agent model policies match.`);
  else console.log(`Done: ${changed} file(s) patched, ${found.size} configured agent(s) matched.`);
}

if (process.argv[1] !== undefined && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  main().catch((error: unknown) => {
    console.error(error instanceof Error ? error.message : String(error));
    process.exitCode = error instanceof AgentModelPolicyError ? RejectedExit : 1;
  });
}
