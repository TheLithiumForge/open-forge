---
open-forge:
  description: Define consumer permission admission, explicit approval and recovery-preserving publication
  tags: [Memory, Crystallized, CLI, Contract, Shared, Permission, Behavior, CurrentTruth]
---

# Workspace Permissions Behavior

These are accepted current contracts for the replacement CLI, which does not
ship yet. The consuming Extension Install, Update and Remove contracts select
this capability. Library grants have a defined document representation; Library
projection behavior remains governed by its separate command contracts.

## Destination Admission

All prior ownership, collision, Framework-anchor, generated-region, overwrite,
source-disjointness and physical containment checks remain necessary. An exact
grant only removes the blanket prohibition on external destinations. Existing
`.agents/` destinations require no grant and retain all their existing checks.
Reject the workspace root and `.agents` itself as file destinations.

Always protect Git metadata (`.git` at any path segment), the selected source
tree, `.agents/open-forge.lifecycle.json`, `.agents/open-forge.libraries.json`,
`.agents/open-forge.permissions.json`, `.agents/open-forge.lock`, the selected
recovery storage and operation temporary paths, and overwrite companions.
Protect Framework-owned files/regions and registered Library destinations from
Extension effects through their existing observed ownership policies. These
checks also use portable identity. Never interpret an allowlist entry as an
override of a collision or a protected path.

External files remain opaque content. A Markdown file under `.apm/agents/`
does not join Open Forge routes or generated navigation. Other managers' known
control files remain protected where the existing ownership/boundary model
recognizes them. Do not claim universal detection of unknown manager formats or
invent a runtime registry. Ordinary unowned external files can be proposed;
existing occupants retain the command's explicit force and preservation rules.

Each consuming command derives its complete required subject/path pairs from
its own plan and trusted ownership. It never derives approval from source bytes.

## Permission Question

Finish complete source, ownership, destination and structural preflight first.
Then collect the unique missing subject/path pairs. Show source identity when
the command uses a source, each subject ID, exact destination and planned effect
(copy, delete or ownership release), and state that approval will be
remembered in this workspace. Ask once for the complete displayed set with
`[y/N]`. Accept `y` or `yes`, trimmed and case-insensitive. Any other answer,
empty input or EOF declines; do not loop. Cancellation retains the command's
`interrupted` semantics. Neither decline nor cancellation writes any file.

Use the existing typed interactive session and stderr prompt stream. The
permission question is allowed only for a human apply request with prompting
enabled and without `--automatic`. Dry-run and JSON never ask this question.
This restriction does not change existing command-selection or force questions.
Approval does not imply force, prune, or approval for new targets discovered
later. Declining and missing permission in unattended execution produce
`blocked`; dry-run with missing permission also reports `blocked` and the
complete missing set, without effect. Explicit user cancellation is interrupted.

## Application And Recovery

Approval is an immutable decision attached to the reviewed plan, not a write.
Acquire the normal workspace lease and reobserve sources, permission bytes or
absence, lifecycle, targets and parent identities. Any relevant change stops
before effects; do not merge new concurrent grants or transfer approval into a
recomputed wider plan. A wholly new invocation may ask again.

Prepare and verify one existing recovery bundle covering all required recovery entries,
including prior permission-file bytes or its proven absence. Only then apply
the atomic permission-file create/replace and verify intended bytes. The grant
effect precedes content, directory and lifecycle effects. If it fails, do not
start content effects. If a later effect fails or is interrupted, keep the
approved grant and report its actual outcome separately. Publish Extension
lifecycle last under the existing contract. Do not roll back permission.

Use the current reversible ordinary-file entry shape for permission replacement
and creation; creation preserves prior absence. No new recovery schema or entry
kind is required. Exact permission restoration is manual, using retained
evidence where necessary. This capability adds no automatic Doctor/Repair proposal. Generic Library
residual recovery must not apply the permission control-file entry automatically. Cleanup may remove positively
recognized bundles only under its existing explicit command contract.

## Conformance

Prove strict grammar, exact subject isolation, complete missing sets,
noninteractive no-prompt behavior, explicit approval, expected-state refusal,
prior-byte or prior-absence recovery, and truthful partial outcomes. See the
[Interface](interface.md) for the document and result representation.
