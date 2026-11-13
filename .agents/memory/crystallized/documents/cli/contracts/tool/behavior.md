---
open-forge:
  description: Define detection, channel order, safe edits and tool operation checks
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool behavior

Tool behavior is the common flow that detects a tool, plans setup and verifies the changes.
For example, a supported existing CodeGraph installation supplies the executable, so Install adds configuration without creating another installation.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

## Detection

Resolve the declared command and run its bounded `--version` probe through an argument vector.
The version parser returns an exact stable version or a failed observation.
A probe has a timeout and output cap. It never requests setup or indexing.
A wrapper that can download during a version probe needs a read-only readiness check before invocation.
If that check can't establish a read-only probe, report unavailable detection.
CodeGraph's first-launch fallback needs the Q12 review answer.
Status detects an upstream self-update by comparing the current version with the last checked version.
A version inside a supported profile offers reapply. A version outside support produces a warning.
Open Forge leaves upstream hooks, permissions, watchers, daemons, indexing and self-update behavior in place.

## Channel choice

Unless `--channel` chooses an advertised channel, use this order:

1. Reuse an existing supported installation.
2. Choose a tool-published package manager that meets all requirements. Prefer the manager the project uses.
3. For an npm tool without a suitable Node, install with a private Node runtime.
4. Choose an admitted GitHub release archive.

Check runtime, manager version, OS, CPU and Linux libc before proposing a channel.
Keep the manager responsible for an existing installation. A preference never creates a second install.
If an existing command is unsupported or conflicting, explain the account-wide replacement before any change.
A failed transfer doesn't silently select another channel.
An override changes channel selection, not integrity, compatibility, collision or consent checks.

## Planning

Resolve the exact version before approval.
A profile covers `>=major.minor.0 <major.(minor+1).0`, with no prereleases.
Profiles don't overlap. Each lists tested versions and the minimum Open Forge version.
Only a known newer catalogue supporting the requested version justifies "update Open Forge first".
Otherwise report that support is unknown.
Show package-manager verification honestly. It is weaker than a reviewed dependency closure.
A GitHub asset requires a pinned SHA-256 or a verified attestation. A mismatch blocks before extraction or execution.
Private Node additionally requires a pinned hash from signed SHASUMS256.

Dry-run performs local read-only probes and forms the same selected plan as apply.
It creates no locks, files or child setup processes and performs no network transfer.
A version that cannot be resolved locally stays unresolved in preview. Apply resolves it and shows the exact version before approval.
This preview resolution rule is a review proposal.

## Configuration

Use only the four change kinds in [Tool records](../shared/tool-records/behavior.md).
Before invoking setup, check recorded drift and back up every declared output.
A verified unchanged setup can be skipped. Reconfiguration still checks its postconditions.
Setup is an argument vector with declared effects, never a shell string.
Unknown installer effects are reported, not described as reversible.
Open Forge adds configuration without rewriting the tool's launch commands or deleting its native behavior.

## Effects

Use existing workspace permissions, exclusions, expected-state checks and the workspace lock for project writes.
The home, launcher, runtime and PATH facts are separate machine facts.
Recheck launcher and runtime use before deleting shared files.
Ask before deleting Open Forge-placed tool bytes or runtimes.
When the last tool is removed, offer PATH removal with confirmation.
A package-manager installation stays installed. Print its native uninstall command.

Use Git only for tracked bytes that match Git's saved content.
Use a sibling `.bak` for other existing bytes.
Git can't recover uncommitted bytes.
Tool backup repair follows the saved target and current bytes. It adds no recovery-v2 format.
The [design](../../technical-designs/tool-setup.md) defines the placement and layer flow.

## Conformance

The implementation lanes need these checks:

- Detection with supported, self-updated, missing, failed and bounded probes.
- Every channel-order step and an override without duplicate installation.
- Runtime, manager, OS, CPU and libc requirement failures.
- Exact version disclosure and hash or attestation mismatch rejection.
- Preview and apply selection, no-op, drift, interruption and partial completion.
- Account-wide replacement disclosure, PATH choice and guarded shared-file removal.
- Direct commands, unchanged upstream hooks and permissions and mandatory guidance.
