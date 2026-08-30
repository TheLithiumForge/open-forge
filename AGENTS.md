<!-- open-forge:start -->

# Open Forge

Open Forge provides the working rules and context for this workspace.

Before starting a task, read `.agents/loader.md`.
Use it to select every relevant scope, including nested scopes.
Follow the loaded rules throughout the task.
<!-- open-forge:end -->

## Exact Mechanical Execution Exception

When the user or assigning agent explicitly labels an exact mechanical task `no Open Forge context` (or uses clear equivalent wording), the assigned worker may skip `.agents/loader.md` and all Open Forge task/scoped materials for that task only, and only for exact pre-decided mechanical command execution and literal evidence/output parsing. The assignment must supply an exact, pre-decided read-only command set, with only already-authorized deterministic build, test, AOT, or tool execution artifacts permitted. It must require no command selection, file edits, external state mutation beyond those authorized execution effects, product, architecture, or acceptance decisions, or semantic interpretation, and it must require an exact evidence-only return.

Semantic analysis, investigation requiring project meaning, design, implementation, integration, and code, product, architecture, acceptance, or correctness review always use normal Open Forge loading, including focused semantic review.

System and developer rules, repository permissions, user authorization, sandbox and external-effect boundaries, and the literal assignment remain binding. The exception ends with the task. If the label, commands, or eligibility are ambiguous, load the normal Open Forge context.
