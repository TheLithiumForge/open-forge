# Worker Prompt Templates (orchestrator only)

Keep prompts minimal — the routes must carry the context. Fill `<target>` only.

## Build mode (seeds 1–3)

> Work in the directory `<target>` (it exists; operate there for all file and git commands — do not touch any other directory). Verify your working directory resolves to `<target>` before your first file operation; patch and edit tools may default elsewhere. There is existing project direction recorded in this workspace. There is an AGENTS.md file at the repo root — take a look if it seems relevant to how you should work. Build the project this workspace describes: a genuinely working, reasonably complete version, without gold-plating. Treat this as one focused work session that may or may not be the last one on this project. When you stop, give me a concise status report of what you did and did not do, plus anything relevant for whoever picks this up next (they will start cold). Do not ask me clarifying questions — I'm not available during this run; make reasonable calls and note assumptions in your final report.

## Vision mode (seed-0)

> Work in the directory `<target>`; verify your working directory resolves there before writing anything. I have a rough idea for a small tool and I'd like your help figuring out what it should actually be before anything gets built. There is an AGENTS.md file at the repo root — take a look if it seems relevant to how you should work. Ask me whatever you need to; I'm available for this conversation. When we're done, make sure whatever we agreed on is captured somewhere a future session could pick up.

Then converse per the seed-0 script. For a vision-then-build run, follow acceptance with the Build-mode prompt (same session or fresh session — record which).

## Continue mode (any seed, multi-session runs)

> Work in the directory `<target>`; verify your working directory resolves there before your first file operation. There's existing work in progress from a previous session (I have no memory of it — you're on your own to figure out the current state). <one-line task>. There is an AGENTS.md file at the repo root — take a look if it seems relevant. When done, report: what state you found, what helped you understand it quickly, what you changed, what you verified. Do not ask me clarifying questions.

## Debrief prompts (after any run)

Ask, at minimum: which routes were actually read and when; which listed required packages/references were opened before steps vs late vs never; whether closeout memory was written before the final response; what would have been asked of the user if available; whether any instruction was substituted or deviated from, and whether that substitution was flagged unprompted.
