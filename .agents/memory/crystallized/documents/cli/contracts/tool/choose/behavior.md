---
open-forge:
  description: Define checked provider switches and role clearing without fallback or index changes
  tags: [Memory, Document, CLI, Contract, Tool, Choose, Behavior, CurrentTruth]
---

# tool choose behavior
`tool choose` is a write operation that reconciles one role assignment and the host members that assignment needs.

Choice and installation are separate facts. The complete plan keeps installed binaries and indexes while changing only role-dependent configuration and guidance.

Status: V1 core contract decided.

## Flow

The operation follows these steps:

1. Validate one role and an exact tool ID or `none`.
2. Read all role assignments and the affected providers' settings and receipts.
3. Validate the target capability, install, root, process compatibility and local consent.
4. Read required guidance, host files, grants, exclusions and unmanaged duplicate entries.
5. Form one plan for the old and new managed members, guidance, role settings and lock values.
6. Resolve process consent, grants and final confirmation.
7. Acquire user-store then workspace leases and revalidate the plan's facts.
8. Prepare and verify sibling backups before changing existing files.
9. Prepare and verify required guidance, then publish launchable members and matching role records. Release old role-dependent members only through the checked plan.
10. Verify the single-provider mapping, kept installs and index bytes, then report host restarts and recovery disposition.

## Clearing

For `none`, remove only the role's recognized host members and guidance. Keep content another active role still references. Clearing an already empty verified role is a no-op. Missing or changed evidence never becomes a verified no-op.

The [interface](interface.md) defines caller input and output.
