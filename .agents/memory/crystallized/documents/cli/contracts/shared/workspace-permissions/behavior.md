---
open-forge:
  description: Define consumer permission admission, explicit approval and recovery-preserving publication
  tags: [Memory, Crystallized, CLI, Contract, Shared, Permission, Behavior, CurrentTruth]
---

# Workspace Permissions Behavior

These are accepted current contracts for the replacement CLI, which does not
ship yet. The consuming Extension Install, Update and Remove and Library Attach, Sync
and Detach contracts select this capability. Library-specific scope policy is
defined by those commands; Extension behavior stays exact-file-only.

## Destination Admission

All prior ownership, collision, Framework-anchor, generated-region, overwrite,
source-disjointness and physical containment checks remain necessary. A matching exact or Library directory
grant only removes the blanket prohibition on external destinations. Existing
`.agents/` destinations require no grant and retain all their existing checks.
Reject the workspace root and `.agents` itself as file destinations.

Always protect Git metadata (`.git` at any path segment), the selected source
tree, `.agents/open-forge.lifecycle.json`, `.agents/open-forge.libraries.json`,
`.agents/open-forge.permissions.json`, `.agents/open-forge.lock`, the selected
recovery storage and operation temporary paths, and the consuming command's protected overwrite companions.
Library external Markdown is opaque: an external `_name.md`,
`*.overwrite.md` or README does not become an Open Forge control by filename.
Recognized source and destination `.agents` controls remain protected. Extension
overwrite protection retains its existing policy.
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
the command uses a source (or recorded source identity for source-independent
Library removal), each subject ID, exact destination and planned effect
(copy, link creation/deletion, delete or ownership release), and state that approval will be
remembered in this workspace. Ask once for the complete displayed set with
`[y/N]`. Accept `y` or `yes`, trimmed and case-insensitive. Any other answer,
empty input or EOF declines; do not loop. Cancellation retains the command's
`interrupted` semantics. Neither decline nor cancellation writes any file.

Use the existing typed interactive session and stderr prompt stream. The
permission question is allowed only for a human apply request with prompting
enabled and without `--automatic`. Dry-run and JSON never ask this question.
This restriction does not change existing command-selection or force questions.
Approval does not imply force or prune. Exact grants do not cover new targets
discovered later. An explicitly approved Library directory grant covers future
descendant leaves for that same Library/source subject, subject to fresh
protected-path, ownership, ancestry and collision checks. Declining and missing permission in unattended execution produce
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
lifecycle or the Library record last under the consuming command contract. Do not roll back permission.

Use the current reversible ordinary-file entry shape for permission replacement
and creation; creation preserves prior absence. No new recovery schema or entry
kind is required. Exact permission restoration is manual, using retained
evidence where necessary. This capability adds no automatic Doctor/Repair proposal. Generic Library
residual recovery must not apply the permission control-file entry automatically. Cleanup may remove positively
recognized bundles only under its existing explicit command contract.

## Library Scope Admission And Revocation

Library requirements remain exact selected external leaves. A same-source exact
file grant or directory grant whose portable path is a strict ancestor admits a
leaf. Compare segment boundaries, not a raw character prefix. A directory grant
does not adopt its directory or any existing occupant. Implicit `.agents` scope
needs no explicit grant. No whole-workspace recursive grant is accepted.

Live missing Library leaves propose the immediate parent directory. Root leaves
and retirement-only/Detach missing leaves propose exact files. Deduplicate
proposals and omit descendants already covered by a proposed parent. Prompts
explicitly describe future descendants and, when needed, old/new source binding.
Rebinding requires explicit replacement approval; concurrent observation changes
invalidate it. A changed ID/source binding cannot inherit old grants implicitly.

Revocation gates selected external live leaves, retirements, Detach and explicit
Library recovery. Recorded source identity suffices for removal and recovery;
source bytes or source availability are not required. Recovery checks current
permissions under its lease and never restores or widens grants. The permission
control-file recovery entry is excluded from Library repair attribution.

## Conformance

Prove strict grammar, exact subject isolation, complete missing sets,
noninteractive no-prompt behavior, explicit approval, expected-state refusal,
prior-byte or prior-absence recovery, and truthful partial outcomes. See the
[Interface](interface.md) for the document and result representation.
