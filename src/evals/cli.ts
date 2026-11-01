import { join } from "node:path";
import { Command, CommandHelp, Flag, commandHelp, option, parseCommand } from "./config/commands.ts";
import { loadPlan } from "./config/plans.ts";
import { configuredDoctor } from "./config/doctor.ts";
import { smokeAttempt } from "./config/smoke.ts";
import { qualifySetups } from "./config/qualify.ts";
import { runPlan } from "./config/run-plan.ts";
import { CliScripts, launchNode, launchUi } from "./config/launch.ts";
import { loadLocalEnv } from "./config/auth.ts";
import { loadSuite } from "./benchmarks/suite.ts";
import { buildEvalImages } from "./execution/containers/docker-image.ts";
import { HistoryStore } from "./history/store.ts";
import { exportBatch } from "./history/export.ts";
import { EvalPaths, evalsRoot } from "./paths.ts";
import { validateCase } from "./grading/brownfield/validate.ts";
import { BrownfieldFiles } from "./materialize/brownfield-case.ts";
import { batchMatrixSummary } from "./config/summary.ts";
import { rejudgeOptions, runPresetOptions } from "./config/preset-options.ts";

export interface CliServices {
  readonly print: (text: string) => void;
  readonly doctor: typeof configuredDoctor;
  readonly images: () => Promise<void>;
  readonly smoke: typeof smokeAttempt;
  readonly qualify: typeof qualifySetups;
  readonly plan: typeof loadPlan;
  readonly suite: typeof loadSuite;
  readonly run: typeof runPlan;
  readonly ui: typeof launchUi;
  readonly launch: typeof launchNode;
  readonly export: (batch: string, target: string) => Promise<string>;
  readonly validateCase: typeof validateCase;
  readonly summary?: typeof batchMatrixSummary;
}
const defaults: CliServices = {
  print: console.log, doctor: configuredDoctor, images: buildEvalImages, smoke: smokeAttempt, qualify: qualifySetups,
  plan: loadPlan, suite: loadSuite, run: runPlan, ui: launchUi, launch: (file, args) => { loadLocalEnv(); return launchNode(file, args); },
  export: (batch, target) => exportBatch(new HistoryStore(EvalPaths.results), batch, target),
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

export async function runCli(argv: readonly string[], services: CliServices = defaults): Promise<number> {
  const request = parseCommand(argv);
  const { command, args } = request;
  if (request.help) { services.print(commandHelp(command === Command.help ? args[0] : command)); return 0; }
  switch (command) {
    case Command.doctor: {
      checkedArgs(args, [], [], 0);
      const checks = await services.doctor();
      for (const check of checks) services.print(`${check.passed ? "pass" : "fail"} ${check.name}: ${check.detail}`);
      return checks.every((check) => check.passed) ? 0 : 1;
    }
    case Command.images: checkedArgs(args, [], [], 0); await services.images(); return 0;
    case Command.smoke: checkedArgs(args, [], [], 0); services.print(`Smoke passed. ${JSON.stringify(await services.smoke())}`); return 0;
    case Command.summary: {
      checkedArgs(args, [], [Flag.batch], 0);
      services.print(JSON.stringify(await (services.summary ?? batchMatrixSummary)(option(args, Flag.batch)), null, 2));
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
      checkedArgs(args, command === Command.run ? [Flag.development] : [], command === Command.run ? [Flag.plan, Flag.implementers, Flag.judges, Flag.user] : [Flag.plan], 0);
      const overrides = command === Command.run ? runPresetOptions(args) : {};
      const config = { ...await services.plan(option(args, Flag.plan), command === Command.qualify || args.includes(Flag.development)), ...overrides };
      if (command === Command.run) return services.run(config, services.print);
      const suites = await Promise.all((config.cases ?? [config.suite]).map((id) => services.suite(id)));
      for (const suite of suites) services.print(JSON.stringify(await services.qualify(suite, config.setups), null, 2));
      return 0;
    }
    case Command.ui: checkedArgs(args, [], [], 0); return services.ui();
    case Command.rejudge: {
      rejudgeOptions(args);
      return services.launch(join(evalsRoot, CliScripts.rejudge), args);
    }
    case Command.recheck: {
      const operands = args.filter((arg) => arg !== Flag.rebuild);
      if (operands.length < 1 || operands.length > 2) throw new Error(CommandHelp.recheck);
      checkedArgs(args, [Flag.rebuild], [], operands.length);
      return services.launch(join(evalsRoot, CliScripts.recheck), args);
    }
    case Command.export: {
      checkedArgs(args, [], [Flag.out], 1);
      if (args[0]?.startsWith("--") || args[0] === undefined) throw new Error(CommandHelp.export);
      services.print(await services.export(args[0], option(args, Flag.out)));
      return 0;
    }
    case Command.help: return 0;
  }
}

if (import.meta.main) {
  try { process.exitCode = await runCli(process.argv.slice(2)); }
  catch (error) { console.error(error instanceof Error ? error.message : String(error)); process.exitCode = 1; }
}
