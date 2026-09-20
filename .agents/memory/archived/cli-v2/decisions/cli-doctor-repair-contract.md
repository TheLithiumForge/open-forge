---
open-forge:
  description: "Historical CLI-v2 source: Doctor diagnoses workspace health without writing, while an explicit repair mode fixes every mechanically safe finding through one plan and reports the rest"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Doctor And Repair Contract

## Context

Agents, users, and continuous integration need one reliable way to determine whether an Open Forge workspace is healthy. A general suggestion system would mix deterministic defects with contextual advice and could present policy or opinion as framework truth.

Many structural defects have one mechanically correct repair. Others require semantic judgment, lifecycle intent, ownership authority, or a potentially destructive choice. Treating both groups as automatically fixable would make a convenient health command unsafe.

Diagnosis also appears in read-only workflows, preflight checks, tests, and continuous integration. A command that writes merely because it found a defect would be difficult to trust and reproduce.

## Decision

Bare `doctor` is strictly read-only. It inspects the complete applicable Open
Forge surface and reports deterministic findings without changing files or
managed state. Completeness begins from exact Framework anchors, routed
topology, `.agents/open-forge.json`, adjacent backups, visible residual state,
and their contained local-reference closure. It never means recursively
scanning every workspace file.

Repair always requires the explicit `repair` command. `--dry-run` previews the
same complete plan. Repair follows one accepted experience:

1. Collect all findings.
2. Classify each finding as safe repair, manual decision, blocked repair, or informational.
3. Build one inspectable repair plan containing every effect for every safe-repair finding.
4. Revalidate the plan's assumptions immediately before mutation.
5. Apply the plan through the shared recoverable mutation path.
6. Run the diagnostics again.
7. Report repaired, still-invalid, manual-decision, and blocked-repair findings.

A finding receives a safe repair only when the intended result is
deterministic, preserves authored meaning, stays within existing lifecycle
authority, and satisfies the ordinary safety boundary. Rebuilding stale
derived indexes is a safe-repair example.

Doctor does not automatically rewrite ambiguous authored content, replace customization with distribution defaults, adopt unowned files, choose between conflicting authorities, or infer install, upgrade, restoration, removal, or dependency intent. It reports those findings with their evidence and the reason explicit intervention is required.

Doctor provides one health-and-repair experience without becoming one
implementation module. Each Framework or Extensions domain supplies its
diagnostics and typed safe repair proposals. The root coordinator calls domains
directly; diagnostic codes never resolve checks or fixes. Doctor aggregates
their findings and composes eligible repairs through the shared planning and
mutation contracts.

Doctor reports adjacent backups, temporary residuals, lifecycle disagreement,
and post-stop mixed state without writing. Repair restores or removes that
evidence only when one exact result is mechanically proven and explicitly
authorized. Ambiguous state remains a manual decision. Exact-external
Completion recovery requires its own explicit selection contract and is not
inferred by workspace repair.

## Rationale

A read-only default preserves trust across interactive, agent, test, and automation use. Explicit repair intent grants clear mutation authority without adding a second confirmation ceremony after the caller has already selected the operation.

One repair plan makes the common goal cheap: return the workspace to every
mechanically knowable invariant. Classification prevents convenience from
silently consuming semantic or lifecycle decisions.

Domain-owned checks and repairs keep policy near the contract it validates. Aggregation gives users and agents a single entry point without recreating the monolithic CLI behind one command.

## Alternatives And Tradeoffs

- Mutating bare `doctor` would minimize command count but violate read-only expectations and make diagnostics unsafe in continuous integration.
- Reporting commands for each defect type would make intent explicit but increase discovery, tool calls, and repair cost.
- A general recommendation engine could cover broader situations but would blur deterministic diagnosis with contextual judgment.
- Letting doctor invoke upgrades, restoration, or package removal could fix more states automatically but would hide materially different lifecycle authority behind a health command.

The accepted direction requires every diagnostic domain to define resolution
precisely. A safe repair may therefore leave issues unresolved even when a
human could describe a plausible change.

## Consequences

- `doctor` remains safe to run repeatedly in any read-only context.
- Repair is non-interactive and automatable after explicit invocation.
- Findings expose stable code, severity, typed subject and evidence, named
  resolution class, next actions, and blocking reasons.
- Repair uses the common plan, application, rollback, result, and verification contracts.
- Backup or residual state is repaired only when its intended result is exact.
- A successful repair run may still report manual-decision or blocked-repair findings.
- Installation, upgrade, restoration, extension lifecycle, and semantic choices remain separate explicit operations.
- Finding codes remain serialized evidence and never become behavior lookup keys.

## Authoritative Sources

- [Open Forge CLI Architecture](../../documents/cli/architecture.md)
- [Open Forge CLI Interface](../../documents/cli/interface.md)
- [Open Forge Principles](../../documents/principles.md)
- [Planned CLI mutation Pattern](../../../../patterns/open-forge/cli/filesystem/planned-mutation.md)
- [CLI diagnosis and repair contract](../../documents/cli/contracts/diagnosis-and-repair.md)
- [Diagnostic domain slice Pattern](../../../../patterns/open-forge/cli/diagnostics/diagnostic-domain-slice.md)

## Decision Relationships

- [Agent-first CLI product contract](cli-agent-first-product-contract.md)
- [Product direction](../product/product-direction.md)
- [Extension package boundary](../extensions/extension-package-boundary.md)
