# Orchestration

This extension will be developed later.

For now, this directory intentionally contains only this planning file. Do not add an `extension.json`, payload, catalogue entry, receipt entry, or install behavior until the orchestration design has been validated through real sequential and parallel project work.

## Candidate Files After Validation

Move or add these orchestration-owned sources to the extension payload after validation:

- `.apm/agents/overseer.agent.md`
- `.apm/agents/task-mastermind.agent.md`
- `.apm/agents/integration-mastermind.agent.md`
- `.agents/directives/hierarchical-orchestration.md`
- `.agents/workflows/worktree-program-development.md`
- `.agents/templates/memory/project-control-ledger.md`

## Referenced Agent Dependencies

The orchestration roles currently reference the following bounded roles. Before packaging, decide whether each role moves into this extension or remains in a shared Core package declared as a dependency. Keep one authoritative source; do not copy the same role into both locations.

- `.apm/agents/advisor.agent.md`
- `.apm/agents/architect.agent.md`
- `.apm/agents/blue-structure-improver.agent.md`
- `.apm/agents/brilliant-implementer.agent.md`
- `.apm/agents/challenger.agent.md`
- `.apm/agents/challenger-two.agent.md`
- `.apm/agents/explorer.agent.md`
- `.apm/agents/gray-contract-implementer.agent.md`
- `.apm/agents/green-behavior-implementer.agent.md`
- `.apm/agents/implementer.agent.md`
- `.apm/agents/improvement-reviewer.agent.md`
- `.apm/agents/purple-evidence-improver.agent.md`
- `.apm/agents/red-evidence-author.agent.md`
- `.apm/agents/researcher.agent.md`
- `.apm/agents/reviewer.agent.md`
- `.apm/agents/reviewer-terra.agent.md`
- `.apm/agents/workspace-operator.agent.md`
- `.apm/agents/writer.agent.md`
- `.apm/agents/writing-reviewer.agent.md`

## Repository Consumers To Reconcile

These files consume or index the orchestration sources. Update them during extraction, but do not move project-specific state into the extension:

- `.agents/directives/_directives.md`
- `.agents/templates/memory/_memory.md`
- `.agents/workflows/_workflows.md`
- `.agents/directives/open-forge/cli/implementation.md`
- `.agents/memory/working/cli-development/_cli-development.md`
- `.agents/templates/memory/task.md`
- `.agents/templates/memory/plan.md`
- `.agents/workflows/adaptive-development.md`
- `.agents/workflows/development/task-lifecycle.md`
- `.agents/workflows/planning.md`

The installed `.opencode/agents/*.md` and `.codex/agents/*.toml` files and the deployment entries in `apm.lock.yaml` are generated runtime state. Regenerate them from the final extension sources; do not copy them into the extension payload.

## Validation Before Extraction

- Exercise discussion, decision, direct-work, sequential managed-execution, explicitly authorized parallel-worktree, integration, recovery, and final-acceptance paths.
- Confirm that internal agent spawning is non-interactive and that ordinary Bash, Python, local Git, build, test, and `dotnet publish` commands remain low friction.
- Confirm that deletion, destructive Git, remote mutation, publication, deployment, credential, and privileged-system effects remain blocked or approval-gated at the correct owner boundary.
- Confirm that execution profiles and review, council, and correction budgets remain Markdown state rather than frontmatter.
- Confirm that legacy `Mastermind` records retain provenance while active project authority resolves to the Overseer.
- Freeze ownership, dependency, installation, update, removal, and generated-runtime behavior before creating the manifest and payload.
