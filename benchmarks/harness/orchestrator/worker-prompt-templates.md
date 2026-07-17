# Worker Prompt Patterns

Freeze one exact prompt file before `prepare`. The orchestration runtime should set the worker's current directory to the prepared workspace, allowing the prompt itself to remain machine-independent. Only the selected prompt is snapshotted and delivered; this orchestrator file never enters the worker workspace.

## Build Mode

> Work only in the current working directory and verify it before your first write. Read AGENTS.md and follow the workspace's routed instructions. Build the project this workspace describes: a genuinely working, reasonably complete version without gold-plating. Treat this as one focused session that may or may not be the last. When you stop, report what you changed, what you verified, what remains, and any assumptions or deviations a cold successor needs to know. Do not ask clarifying questions during this run; make reasonable bounded decisions and state them.

## Vision Mode

> Work only in the current working directory and verify it before your first write. Read AGENTS.md and follow the workspace's routed instructions. I have a rough idea for a small tool and want to determine what it should be before building it. Ask what you need; I am available for this conversation. When we finish, capture the accepted direction so a genuinely cold future session can continue without re-asking settled questions.

The orchestrator then answers only from the frozen seed persona script. A vision-then-build arm must predeclare whether build occurs in the same context or a fresh context.

## Continue Mode

> Work only in the current working directory and verify it before your first write. Read AGENTS.md and follow the workspace's routed instructions. Existing work may be in progress from a previous session, but you have no prior conversation context. Determine the current state, complete the declared task, and report what you found, what helped you resume, what you changed, and what you verified. Do not ask clarifying questions during this run; make reasonable bounded decisions and state them.

## Debrief

Debrief prompts are orchestration traces, not worker setup. Ask which routes and required references were read and when, what was skipped, whether memory closeout preceded the final response, what would have required user input, and whether any substitution or deviation was reported unprompted. Do not use retrospective self-report as the sole evidence for isolation, routing, or directive compliance.
