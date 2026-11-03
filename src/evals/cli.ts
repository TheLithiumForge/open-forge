import { join, resolve } from "node:path";
import { Command, CommandHelp, Flag, ResultsAction, commandHelp, option, parseCommand } from "./config/commands.ts";
import { loadPlan } from "./config/plans.ts";
import type { ChainConfig } from "./config/config.types.ts";
import { configuredDoctor } from "./config/doctor.ts";
import { smokeAttempt } from "./config/smoke.ts";
import { qualifySetups } from "./config/qualify.ts";
import { resumePlan, runPlan } from "./config/run-plan.ts";
import { CliScripts, launchNode } from "./config/launch.ts";
import { runSite, SiteDefaults, type SiteOptions } from "./site/site.ts";
import { loadLocalEnv } from "./config/auth.ts";
import { loadSuite } from "./benchmarks/suite.ts";
import { buildEvalImages } from "./execution/containers/docker-image.ts";
import { HistoryStore } from "./history/store.ts";
import { addResults } from "./history/results-add.ts";
import { EvalPaths, evalsRoot } from "./paths.ts";
import { validateCase } from "./grading/brownfield/validate.ts";
import { checkChain } from "./execution/chain-check.ts";
import { BrownfieldFiles } from "./materialize/brownfield-case.ts";
import { batchMatrixSummary } from "./config/summary.ts";
import { savedAnalysis, savedProgress, savedUsage } from "./history/saved-batches.ts";
import { judgeOptions, runPresetOptions } from "./config/preset-options.ts";
import { referenceOptions } from "./config/reference.ts";
import { runReference } from "./execution/reference.ts";
import { applyMachineOptions, splitMachineOptions } from "./config/machine-options.ts";
import { DiagnosticEnabled, DiagnosticEnvironment } from "./benchmarks/neon-pairs/checks/diagnostics.ts";

export interface CliServices {
  readonly print: (text: string) => void;
  readonly doctor: typeof configuredDoctor;
  readonly images: () => Promise<void>;
  readonly smoke: typeof smokeAttempt;
  readonly qualify: typeof qualifySetups;
  readonly plan: typeof loadPlan;
  readonly suite: typeof loadSuite;
  readonly run: typeof runPlan;
  readonly resume?: typeof resumePlan;
  readonly site: (options: SiteOptions) => Promise<number>;
  readonly launch: typeof launchNode;
  readonly results: (batch: string, repository: string) => Promise<string>;
  readonly validateCase: typeof validateCase;
  readonly chainCheck?: typeof checkChain;
  readonly summary?: typeof batchMatrixSummary;
  readonly progress?: (batches: readonly string[]) => Promise<string[]>;
  readonly usage?: (batches: readonly string[]) => Promise<string[]>;
  readonly analyze?: (batches: readonly string[], missing: boolean) => Promise<string[]>;
  readonly reference?: typeof runReference;
}
const defaults: CliServices = {
  print: console.log, doctor: configuredDoctor, images: buildEvalImages, smoke: smokeAttempt, qualify: qualifySetups,
  plan: loadPlan, suite: loadSuite, run: runPlan, site: (options) => runSite(options, console.log), launch: (file, args) => { loadLocalEnv(); return launchNode(file, args); },
  results: (batch, repository) => addResults(new HistoryStore(EvalPaths.runs), batch, repository, { run: (script, args, cwd) => launchNode(join(cwd, script), args, cwd) }),
  validateCase,
};

function checkedArgs(args: readonly string[], flags: readonly string[], values: readonly string[], positions: number): void {
  let operands = 0;
  for (let index = 0; index < args.length; index += 1) {
    const arg = args[index];
    if (arg === undefined) continue;
    if (values.includes(arg)) { option(args, arg); index += 1; }
    else if (flags.includes(arg)) { if (args.indexOf(arg) !== args.lastIndexOf(arg)) throw new Error("Repeated flag."); }
    else if (arg.startsWith("--")) throw new Error(`Unknown option ${arg}.`);
    else operands += 1;
  }
  if (operands !== positions) throw new Error("Wrong number of operands. Use --help for an example.");
}

/** How many attempts may run at once. A plan's own setting applies unless this caps it. */
function parallelOption(args: readonly string[]): number {
  const value = Number(option(args, Flag.parallel));
  if (!Number.isInteger(value) || value < 1) throw new Error("Give --parallel a whole number of at least 1.");
  return value;
}

/** Narrows a plan to some of its setups, in the plan's order. The plan's first setup is every comparison's control, so it stays. */
function planSetups<T extends { readonly setups: readonly string[] }>(config: T, value: string): T {
  const chosen = value.split(",").map((id) => id.trim());
  const unknown = chosen.find((id) => !config.setups.includes(id));
  if (unknown !== undefined) throw new Error(`The plan has no setup ${JSON.stringify(unknown)}.`);
  if (config.setups[0] !== undefined && !chosen.includes(config.setups[0])) throw new Error(`Keep the control, ${config.setups[0]}, so each setup has its baseline.`);
  return { ...config, setups: config.setups.filter((id) => chosen.includes(id)) };
}

/** Gives a chain plan the batch of its previous milestone. Only a chain plan takes one, and it always needs one. */
function planParent<T extends { readonly chain?: ChainConfig }>(config: T, parent: string | null): T {
  if (config.chain === undefined) {
    if (parent !== null) throw new Error("Only a chain plan takes --parent.");
    return config;
  }
  if (parent === null) throw new Error(`Chain ${config.chain.id} needs --parent <batch>, the batch of milestone ${config.chain.milestone - 1}.`);
  return { ...config, chain: { ...config.chain, parentBatchId: parent } };
}

/** Narrows a Layer 4 plan to one of its cases. */
function planCase<T extends { readonly cases?: readonly string[] }>(config: T, id: string): T {
  if (!config.cases?.includes(id)) throw new Error(`The plan has no case ${JSON.stringify(id)}.`);
  return { ...config, cases: [id] };
}

/** Machine flags are already applied in this process and travel on to the scripts the CLI starts. */
export async function runCli(argv: readonly string[], services: CliServices = defaults, machine: readonly string[] = []): Promise<number> {
  const request = parseCommand(argv);
  const { command, args } = request;
  if (request.help) { services.print(commandHelp(command === Command.help ? args[0] : command)); return 0; }
  switch (command) {
    case Command.reference: {
      checkedArgs(args, [], [Flag.layer, Flag.judges], 0);
      const selected = referenceOptions(args.includes(Flag.layer) ? option(args, Flag.layer) : undefined,
        args.includes(Flag.judges) ? option(args, Flag.judges) : undefined);
      return (services.reference ?? runReference)(selected, services.print);
    }
    case Command.doctor: {
      checkedArgs(args, [], [], 0);
      const checks = await services.doctor();
      for (const check of checks) services.print(`${check.passed ? "pass" : "fail"} ${check.name}: ${check.detail}`);
      return checks.every((check) => check.passed) ? 0 : 1;
    }
    case Command.images: checkedArgs(args, [], [], 0); await services.images(); return 0;
    case Command.smoke: checkedArgs(args, [], [], 0); services.print(`Smoke passed. ${JSON.stringify(await services.smoke())}`); return 0;
    case Command.progress: {
      if (args.some((arg) => arg.startsWith("--"))) throw new Error(CommandHelp.progress);
      services.print((await (services.progress ?? savedProgress)(args)).join("\n"));
      return 0;
    }
    case Command.usage: {
      if (args.some((arg) => arg.startsWith("--"))) throw new Error(CommandHelp.usage);
      services.print((await (services.usage ?? savedUsage)(args)).join("\n"));
      return 0;
    }
    case Command.analyze: {
      if (args.some((arg) => arg.startsWith("--") && arg !== Flag.missing)) throw new Error(CommandHelp.analyze);
      services.print((await (services.analyze ?? savedAnalysis)(args.filter((arg) => arg !== Flag.missing), args.includes(Flag.missing))).join("\n"));
      return 0;
    }
    case Command.summary: {
      checkedArgs(args, [], [Flag.batch], 0);
      services.print(JSON.stringify(await (services.summary ?? batchMatrixSummary)(args.includes(Flag.batch) ? option(args, Flag.batch) : undefined), null, 2));
      return 0;
    }
    case Command.chainCheck: {
      checkedArgs(args, [], [Flag.plan, Flag.parent], 0);
      if (!args.includes(Flag.plan) || !args.includes(Flag.parent)) throw new Error(CommandHelp[Command.chainCheck]);
      const config = planParent(await services.plan(option(args, Flag.plan), true), option(args, Flag.parent));
      const parent = config.chain?.parentBatchId;
      if (parent === undefined) throw new Error(CommandHelp[Command.chainCheck]);
      for (const row of await (services.chainCheck ?? checkChain)(config, await services.suite(config.suite), parent)) {
        services.print(`${row.setup} ${row.repeat} from ${row.parentRunId}: build ${row.buildPassed ? "passed" : "failed"}`);
        services.print(`  new task: ${row.target.map((check) => `${check.id} ${check.status}`).join(", ") || "none"}`);
        services.print(`  changed from the parent: ${row.changed.map((check) => `${check.id} ${check.parent ?? "absent"} to ${check.child}`).join(", ") || "none"}`);
      }
      return 0;
    }
    case Command.validateCase: {
      checkedArgs(args, [Flag.all], [Flag.case, Flag.plan], 0);
      const all = args.includes(Flag.all);
      if (all ? args.includes(Flag.case) || !args.includes(Flag.plan) : !args.includes(Flag.case) || args.includes(Flag.plan)) throw new Error(CommandHelp[Command.validateCase]);
      const plan = all ? await services.plan(option(args, Flag.plan), true) : undefined;
      const ids = plan === undefined ? [option(args, Flag.case)] : plan.cases ?? [plan.suite];
      const suites = await Promise.all(ids.map((id) => services.suite(id)));
      if (suites.some((suite) => suite.brownfield === undefined)) throw new Error("validate-case accepts Layer 4 cases only.");
      let passed = true;
      for (const suite of suites) {
        try {
          const report = await services.validateCase(suite);
          services.print(`${report.passed ? "pass" : "fail"} ${suite.id}: ${suite.brownfield?.case.checks}/${BrownfieldFiles.validation}`);
          for (const stage of report.stages) {
            for (const check of stage.outcomes.filter((check) => !check.matches)) services.print(`${stage.phase}: ${check.id} ${check.actual}, expected ${check.expected}`);
            if (stage.error !== null) services.print(`${stage.phase}: ${stage.error}`);
          }
          passed = report.passed && passed;
        } catch (error) {
          services.print(`fail ${suite.id}: ${error instanceof Error ? error.message : String(error)}`);
          passed = false;
        }
      }
      return passed ? 0 : 1;
    }
    case Command.run:
    case Command.qualify: {
      checkedArgs(args, command === Command.run ? [Flag.development] : [], command === Command.run ? [Flag.plan, Flag.implementers, Flag.judges, Flag.user, Flag.case, Flag.setups, Flag.parallel, Flag.parent] : [Flag.plan], 0);
      const overrides = command === Command.run ? { ...runPresetOptions(args), ...(args.includes(Flag.parallel) ? { parallelRuns: parallelOption(args) } : {}) } : {};
      const planned = { ...await services.plan(option(args, Flag.plan), command === Command.qualify || args.includes(Flag.development)), ...overrides };
      const narrowed = args.includes(Flag.setups) ? planSetups(planned, option(args, Flag.setups)) : planned;
      const config = command === Command.run ? planParent(narrowed, args.includes(Flag.parent) ? option(args, Flag.parent) : null) : narrowed;
      if (command === Command.run) return services.run(args.includes(Flag.case) ? planCase(config, option(args, Flag.case)) : config, services.print);
      const suites = await Promise.all((config.cases ?? [config.suite]).map((id) => services.suite(id)));
      for (const suite of suites) services.print(JSON.stringify(await services.qualify(suite, config.setups), null, 2));
      return 0;
    }
    case Command.resume: {
      checkedArgs(args, [Flag.acceptHarness], [Flag.parallel], 1);
      const [batch] = args;
      if (batch === undefined || batch.startsWith("--")) throw new Error(CommandHelp.resume);
      return (services.resume ?? resumePlan)(batch, services.print, args.includes(Flag.parallel) ? parallelOption(args) : undefined, args.includes(Flag.acceptHarness));
    }
    case Command.site: {
      checkedArgs(args, [Flag.noBrowser], [Flag.port, Flag.repository], 0);
      const port = args.includes(Flag.port) ? Number(option(args, Flag.port)) : SiteDefaults.port;
      if (!Number.isInteger(port) || port < 1024 || port > 65534) throw new Error("Give --port a whole number from 1024 to 65534, since the data server takes the next port.");
      return services.site({ port, repository: args.includes(Flag.repository) ? resolve(option(args, Flag.repository)) : null, browser: !args.includes(Flag.noBrowser) });
    }
    case Command.judge: {
      judgeOptions(args);
      return services.launch(join(evalsRoot, CliScripts.judge), [...args, ...machine]);
    }
    case Command.check: {
      const operands = args.filter((arg) => arg !== Flag.rebuild && arg !== Flag.missing);
      if (operands.length < 1 || operands.length > 2) throw new Error(CommandHelp.check);
      checkedArgs(args, [Flag.rebuild, Flag.missing], [], operands.length);
      return services.launch(join(evalsRoot, CliScripts.check), [...args, ...machine]);
    }
    case Command.results: {
      const [action, batch] = args;
      if (action !== ResultsAction.add || batch === undefined || batch.startsWith("--")) throw new Error(CommandHelp.results);
      checkedArgs(args.slice(1), [], [Flag.repository], 1);
      const repository = args.includes(Flag.repository) ? resolve(option(args, Flag.repository)) : EvalPaths.resultsRepository;
      services.print(`Added the batch at ${await services.results(batch, repository)}`);
      return 0;
    }
    case Command.help: return 0;
  }
}

if (import.meta.main) {
  try {
    const { args, machine, options } = splitMachineOptions(process.argv.slice(2));
    applyMachineOptions(options);
    // Every check saves its diagnostics. The checker reads this inside its container, so its own files and version stay
    // the same. Scripts the CLI starts, such as check, inherit it.
    process.env[DiagnosticEnvironment] = DiagnosticEnabled;
    process.exitCode = await runCli(args, defaults, machine);
  }
  catch (error) { console.error(error instanceof Error ? error.message : String(error)); process.exitCode = 1; }
}
