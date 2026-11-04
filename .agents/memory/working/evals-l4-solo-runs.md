---
open-forge:
  description: Run the two Layer 4 packets with one agent, on Claude Opus and Sonnet in cloud chats and on Codex Terra and Luna locally
  tags: [Memory, Working, Contextual, Active, Evals]
---

# L4 solo runs

L4 solo runs are eval runs that do the two Layer 4 packets with one agent, on Claude Opus and Sonnet in cloud chats and on Codex Terra and Luna on the maintainer's PC.

The Sol whole-packet batches reached nearly full coverage in every scenario, so they say little about what the scenario changes. These runs repeat the same packets on other models to see whether the scenarios differ there.

Status: steps tested up to the implementer. No run has started yet.

## Scope

Every run does one packet as a whole packet, with every task in one request:

- `l4-p1-four-bugs` - P1 Four bugs. Time limit 60 minutes.
- `l4-p2-name-and-pasted-saves` - P2 Name and pasted saves. Time limit 90 minutes.

Each packet runs once per scenario and model, in four scenarios: `code-only`, `plain-docs`, `organized-docs` and `full-core-scoped`. These are the same packets and limits as `neon-pairs-p1-whole-packet.json` and `neon-pairs-p2-whole-packet.json`. Checks and judges come later.

## Claude chat runs

Each cloud chat runs one scenario on one model. The maintainer picks the model and puts the scenario on the first line of the message. The chat starts on `TheLithiumForge/open-forge-eval-results`, which has no agent instructions, so the chat's own context holds no Open Forge rules.

The chat is the harness. It builds the start, hands the prompt to one fresh subagent, then saves and pushes the result. The subagent is the implementer, and it sees only its prompt. Both packets run in the same chat, each with its own subagent.

The implementer runs at high effort on both models. High is the runner's level for Claude implementers, so the model is the only difference between the two chats of a scenario.

### Prepare

Run these steps:

1. Read the scenario from the first line of the message. If it isn't one of the four, stop and ask.
2. Call `get_session`. Note `session_context.model` and `external_metadata.last_served_model`. If the model isn't Opus or Sonnet, stop and ask.
3. Add `TheLithiumForge/open-forge` with `add_repo`, read access. Clone branch `ccr-17a2a289-jmta3y` to `/home/user/harness`.
4. Run `npm ci --prefix /home/user/harness/src/evals`.
5. Save the prepare script below as `/home/user/harness/src/evals/prepare-run.tmp.ts`.
6. For each packet, run it from `/home/user/harness/src/evals`:

   ```sh
   node prepare-run.tmp.ts <packet> <scenario> /home/user/eval-run/<packet>
   ```

   It writes the starting project to `project/`, the implementer's prompt to `prompt.md` and the starting files to `start.json`.

7. Note `git -C /home/user/harness rev-parse HEAD`, then delete `/home/user/harness`. This keeps the checks and solutions out of the implementer's reach.

### Implement

For each packet, P1 first:

1. Note the UTC start time.
2. Start one `general-purpose` subagent in the foreground. Its prompt is the content of `prompt.md`, unchanged. Set `model` to the chat's model family, `opus` or `sonnet`, and `effort` to `high`.
3. If the implementer ends with a question, send it this once: "I have no more details. Use your judgment and finish the work." Count each such reply.
4. Note the UTC end time, and the total tokens, tool uses and duration from the subagent's result.

While it works:

- Don't help the implementer, review its work or fix it.
- Don't run checks or judges.
- If a run fails or passes its time limit, save it as it is and note it.

### Save

Each run gets the folder `chat-runs/l4/<scenario>/<model>-high-<start as YYYYMMDDTHHMMZ>/<packet>/` in the results repository, with `<model>` from `session_context.model`. Fill it from `/home/user/eval-run/<packet>`:

```sh
cd /home/user/eval-run/<packet>/project
base=$(git rev-list --max-parents=0 HEAD)
git add -A && git diff --cached --binary "$base" > ../changes.patch
mkdir -p "$D/project"
git ls-files -z -c | tar --null -T - -cf - | tar -xf - -C "$D/project"
cp ../prompt.md ../start.json ../changes.patch "$D/"
```

`$D` is the run's folder. The copy leaves out installed packages and build output, since the project's own `.gitignore` covers them. Then write two files there:

- `report.md` - the implementer's final message, unchanged.
- `run.json` - `scenario`, `packet`, `workMode` (`whole-packet`), `harness` (`cloud chat, one subagent`), `model` (`configured` and `lastServed`), `effort`, `openForgeCommit`, `startedAt`, `finishedAt`, `timeLimitMinutes`, `implementer` (`totalTokens`, `toolUses`, `durationMs`), `questionsAnswered` and `notes`.

### Push

Push to branch `claude-chat-runs` of the results repository. You have permission to push there, even if your session names another branch:

1. Check out `claude-chat-runs` from `origin`. If it doesn't exist yet, create it from `main`.
2. Run `git add --force chat-runs/l4/<scenario>`. The force is needed because the project's own `.gitignore` sits inside the folder.
3. Commit with the message `results: L4 <scenario> on <model>, solo chat`, with no AI attribution lines.
4. Run `git pull --rebase origin claude-chat-runs` if the branch exists, then `git push origin HEAD:claude-chat-runs`. Other chats push other folders, so the rebase doesn't conflict. Retry a network failure up to 4 times.

### Prepare script

Save this as `prepare-run.tmp.ts`:

```ts
import { cp, mkdir, readFile, rm, writeFile } from "node:fs/promises";
import { existsSync } from "node:fs";
import { dirname, join } from "node:path";
import { loadSuite } from "./benchmarks/suite.ts";
import { generateSetup } from "./config/setups.ts";
import { setupPrompt } from "./materialize/delivery.ts";

const [packet, scenario, run] = process.argv.slice(2);
if (!packet || !scenario || !run || existsSync(run)) throw new Error("Give the packet, the scenario and an absent run folder.");
const suite = await loadSuite(packet);
const setup = await generateSetup(suite, scenario);
const project = join(run, "project");
await mkdir(run, { recursive: true });
await cp(setup.folder, project, { recursive: true });
await rm(dirname(setup.folder), { recursive: true, force: true });
// Claude Code loads CLAUDE.md and its @imports when it starts in a project, so the prompt carries them the same way.
const instructions: string[] = [];
if (existsSync(join(project, "CLAUDE.md"))) {
  const claude = await readFile(join(project, "CLAUDE.md"), "utf8");
  const files = ["CLAUDE.md", ...[...claude.matchAll(/^@(\S+)$/gm)].map((match) => match[1] ?? "")];
  for (const file of files)
    instructions.push(`Contents of ${join(project, file)} (project instructions, checked into the codebase):\n\n${await readFile(join(project, file), "utf8")}`);
}
const prompt = [`Your project is the folder ${project}. Work only inside it, and run every command from it.`, ...instructions, setupPrompt(suite, scenario)].join("\n\n");
await writeFile(join(run, "prompt.md"), prompt);
await writeFile(
  join(run, "start.json"),
  JSON.stringify({ packet, scenario, label: setup.record.label, frameworkVersion: setup.frameworkVersion, files: setup.record.files }, null, 2) + "\n",
);
process.stdout.write(`${packet} ${scenario}: ${setup.record.files.length} starting files, ${instructions.length} instruction files\n`);
```

## Codex runs

The Codex runs go through the runner on the maintainer's PC, where Docker and the Codex sign-in work. Their results are batches like the Sol ones. Terra runs at `xhigh`, Sol's level, and Luna at `max`, the maintainer's level for Luna.

Run these steps:

1. Find Terra's model ID in Codex's model list and confirm it with the maintainer. The plans use `<terra-model-id>`.
2. Save the two plans below outside the repository.
3. Run `npm run evals -- doctor`.
4. Run each plan, P1 first, in the background:

   ```sh
   npm run evals -- run --plan <plan> --parallel 2
   ```

5. Follow it with `npm run evals -- progress`. If a batch pauses at a usage limit, tell the maintainer and resume only when they say.
6. Publish each batch as [Run evals](../../../src/docusaurus/docs/evals/run.md) describes under Add results. Then, in `src/evals/results`, run `git add --force versions index.json`, commit, `git pull --rebase origin main` and `git push origin HEAD:main`. If `results add` reports a leak, stop.

The P1 plan:

```json
{
  "schemaVersion": 1,
  "layer": 4,
  "packet": "l4-p1-four-bugs",
  "workMode": "whole-packet",
  "setups": ["organized-docs", "code-only", "plain-docs", "full-core-scoped"],
  "runsPerSetup": 1,
  "parallelRuns": 2,
  "agents": [
    { "tool": "codex", "model": "<terra-model-id>", "effort": "xhigh" },
    { "tool": "codex", "model": "gpt-6-luna", "effort": "max" }
  ],
  "judges": "codex",
  "judging": "later",
  "checking": "later",
  "simulatedUser": "codex",
  "isolation": "docker",
  "limits": { "questionRounds": 3, "runMinutes": 60, "checkMinutes": 90 }
}
```

The P2 plan is the same with `"packet": "l4-p2-name-and-pasted-saves"` and `"runMinutes": 90`.

## Limits

The Claude chat runs differ from runner runs in these ways:

- The implementer is a subagent, so its system prompt differs from `claude -p` in a container.
- One fixed reply replaces the simulated user.
- Tokens and time come from the subagent's result, not from the runner's telemetry.
- The chat's repository holds published runs, Sol's included. The prompt keeps the implementer in its project, but nothing enforces that.

## Not decided yet

- How the chat runs get checked. They're folders, not runner batches, so `check` can't read them yet.

## Next step

The maintainer starts the chats and the local Codex agent. Retire this record once every run is pushed.
