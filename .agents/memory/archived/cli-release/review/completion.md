---
open-forge:
  description: Settled Queue 33 contextual history for the rejected shell-completion candidate
  responsibility: Preserve the rejected D018 candidate and evidence rationale without becoming current command authority
  tags: [Memory, Archived, CLI, Release, Review, History, Settled, Rejected, Contextual, Completion, Shell, AOT, Historical]
---

# Completion Review History

**Packet:** Queue 33, Packet 6 of 6; `rejected/settled`

**Classification:** `#Contextual` settled rejection history. This file is not an
accepted Interface Contract, Behavior Contract, Decision, Architecture, or
implementation record. Queue 33 is no longer active review. The [Decision
Agenda](../../../working/cli-release/decision-agenda.md), especially D018, is the current authority for
the rejection. The [CLI Release Program](../_cli-release.md) and [Release
Plan](../../../working/cli-release/release-plan.md) record the resulting gate state.

## Settled disposition

D018 rejects shell completion from the product. There is no `open-forge completion`
command, shell value set, generated-script output, profile editing,
install/remove lifecycle, package-manager completion responsibility, completion
library dependency, custom generator, second runtime, or future completion
implementation target. Normal CLI usage is unchanged.

Queue 33 is settled and rejected. All Gate 2 command dispositions are complete,
so Gate 2 is complete and non-shipping. Gate 3 Architecture is the active next
gate and is open for discussion, not accepted or complete. Implementation
remains blocked until Gate 3 Architecture is accepted and Gate 4
crystallization/readiness is complete.

## Rejected script-only candidate

The following candidate was considered before D018 was rejected. It is
preserved as contextual Queue 33 history only and is not current command syntax:

```text
open-forge completion <shell>
```

The candidate's deliberately narrow boundary was:

- one exact finite shell value, with no inference from the environment,
  executable, workspace, profile, or package manager;
- generated script bytes only on stdout, with bounded diagnostics on stderr and
  no heading, status wrapper, JSON envelope, output path, or workspace mutation;
- no profile, automatic-configuration, install, remove, `--automatic`, `--yes`,
  `--force`, or `--apply` behavior; and
- external ownership of saving, sourcing, installing, updating, and removing
  the script by the shell, package manager, or user.

Shell-profile editing and a completion install/remove lifecycle were rejected
throughout. The candidate also could not add a package-manager integration,
receipt, profile manager, or hidden workspace operation.

## Evidence rationale preserved from Queue 33

Retaining the script-only candidate would have required Architecture evidence
for the selected command library and its transitive dependencies, including:

- Native AOT and trimming publication;
- deterministic generation for every accepted shell, command operand, flag,
  alias, quoting rule, escaping rule, and public hierarchy;
- stable stdout-only script bytes, stderr diagnostics, non-interactive behavior,
  and failure handling;
- canonical-executable generation in package and CI evidence; and
- public-artifact redaction of local paths, user or machine identifiers,
  secrets, tokens, hidden metadata, and runtime-orchestration identifiers.

The Queue 33 recommendation was to reject D018 if a standard, small, safe, and
maintainable library could not provide that boundary without an unearned
dependency cost. The rejected disposition avoids that dependency, custom
completion framework, profile manager, and second runtime. A convenience-based
dissent supported a small built-in generator, but it did not outweigh the
unproven evidence and maintenance cost. These are historical reasons for the
rejection, not current implementation requirements.

## Sources consulted

- [Decision Agenda](../../../working/cli-release/decision-agenda.md), especially D018 and the accepted
  full-delivery and implementation-readiness decisions.
- [CLI Release Program](../_cli-release.md) and [Release Plan](../../../working/cli-release/release-plan.md)
  for the current Queue and gate state.
- [Global Flags Interface](../../../crystallized/documents/cli/contracts/shared/global-flags/interface.md) and
  [Behavior](../../../crystallized/documents/cli/contracts/shared/global-flags/behavior.md).
- [Open Forge CLI implementation directive](../../../../directives/open-forge/cli/implementation.md)
  for the Native AOT and public-artifact boundary.
- [Queue 30 historical analysis](gate-2-closeout.md), retained as contextual
  evidence only.

## Historical closeout checks

- [x] The maintainer's rejection is recorded in the Decision Agenda.
- [x] The script-only candidate and its evidence rationale remain contextual
      history only.
- [x] No completion command, contract, dependency, profile responsibility, or
      future implementation target remains in the current product surface.
- [x] Queue 33 is settled/rejected, Gate 2 is complete, and Gate 3 is the open
      next gate without acceptance or implementation.
