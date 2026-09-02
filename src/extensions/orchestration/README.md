# Orchestration

This extension will be developed later.

For now, this directory intentionally contains only this planning file. Do not add an `extension.json`, payload, catalogue entry, receipt entry, or install behavior until the orchestration design has been validated through real sequential and parallel project work.

The repository-local coordinated-review trial does not change this boundary. Its Review Mastermind and four standing topic sources remain authored under `.apm/agents/` for local validation. Do not copy them into an Extension payload or claim that APM permissions are enforced identically by every generated runtime.

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

## Repository-Local Review Trial

The following experimental sources are not Extension payload candidates yet:

- `.apm/agents/review-mastermind.agent.md`
- `.apm/agents/topics/csharp-conformance.agent.md`
- `.apm/agents/topics/architecture-ownership-refactoring.agent.md`
- `.apm/agents/topics/behavior-contracts.agent.md`
- `.apm/agents/topics/test-evidence.agent.md`

Keep the coordinator and topics read-only and object-only. Their authored APM permissions deny Bash, native filesystem inspection, and language-server inspection, then allow only the named `inspect-git-objects` custom tool. OpenCode loads its tracked adapter from `.opencode/tools/inspect-git-objects.ts`; the adapter accepts structured fields and delegates without a shell to the fixed repository engine in `scripts/inspect-git-objects.ts`. A non-identical runtime projection must receive bounded immutable content in its packet or report a gap; do not claim identical enforcement across runtimes. The coordinator validates a bounded list of independently owned immutable snapshot records, routes only the four named topics, and groups joined returns by snapshot and original writer without disposition. Each topic consumes one named review-budget unit. Coordinator validation and synthesis consume none. Topic passes remain advisory and do not replace a separately required fresh holistic review.

The structured tool exposes only these operations:

- `object-type <object>` and `object-content <object>`;
- `tree-list <tree>` and `tree-path <tree> <path>`;
- `changed-paths <ancestor> <candidate>`, `diff <ancestor> <candidate>`, and `diff-check <ancestor> <candidate>`; and
- `is-ancestor <ancestor-commit> <candidate-commit>`.

Every request carries `operation` and `object`; `tree-path` also carries `path`, while two-object operations carry `otherObject`. Every object argument must be a complete 40- or 64-character hexadecimal object ID. Tree paths must be explicit nonempty repository-relative POSIX paths. The tool rejects refs, selectors, revision expressions, ranges, abbreviations, arbitrary options, unknown fields, output targets, unsafe paths, unknown operations, and nonexistent objects. It invokes only fixed local read-only Git plumbing with bounded output and no shell, pager, color, external diff, or text conversion.

Extension packaging, dependency placement, installation behavior, and cross-runtime permission guarantees remain future hypotheses until the trial provides evidence and the maintainer accepts those boundaries.

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
