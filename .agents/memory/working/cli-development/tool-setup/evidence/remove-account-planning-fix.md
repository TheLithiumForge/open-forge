---
open-forge:
  description: Verify deferred account uninstall planning and project-only Remove
  tags: [Memory, Working, CLI, Tool, Contextual, Evidence]
---

# Account removal planning

Account removal planning is the E-J4 correction that describes an unchosen account uninstall and prepares it only after live confirmation.
For example, project-only Remove can clean Open Forge additions while an unchosen client output binding is unavailable.

Status: READY_FOR_REVIEW UNCOMMITTED.

## Baseline

The baseline is core `885b17db3e56ceb6839ba23609e85a4380c75018` on `feature/tool-setup-d-remove-fix`.
The six local async-entry files matched that core byte-for-byte through `git show` streams before their duplicate local changes were restored.
Their local and core copies, hashes, receipt and duplicate patch are preserved under `artifacts/lane-d-remove-baseline/adf2df87aa8a48169f2b776b60dfab5b`.
Both clean-state guards passed. No other work was restored.
E-J3 was cancelled before edits and stays with E.

## Change

The planner classifies the authored uninstall effects before calling native setup.
Account configuration, shared deletion and PATH changes need the separate live account choice.
Account telemetry alone keeps the project uninstall path.

Before that choice, the setup service only formats the manual command.
This description request has no installed preview invocation, preview arguments, effect paths or possible outputs to prepare.
The local plan keeps the full original request and every supplied independent binding.
Reports show its original effects and conditional output templates without resolving unchosen outputs.

After live confirmation, preparation receives the original uninstall declaration and context unchanged.
The setup service validates full output declarations and inventory, checks backup feasibility and captures expected hashes.
Native application rechecks those hashes and physical containment before backup and execution.
Prepared native facts are kept for reporting. Existing record publication and pending backup completion stay in place.

With `--yes`, unavailable interaction, JSON or a declined account choice, native uninstall stays unchosen.
Project cleanup reports Complete when only those account effects and account settings are kept.
The native uninstall command stays in the result for manual use.
General plan consent still applies. No terminal without `--yes` still requires that consent.

The live account question includes the original binary, agent-entry and persistent user PATH effects.
No real native uninstaller or its dry run runs in this verification.
Tests use synthetic command targets and injected hosts with real owned record, config and backup files.

## Checks

The final managed solution build passes with zero warnings/errors and exit 0.
The four focused class runs measure consent, sequencing and real owned-file effects:

| Class                         | Tier        | Passed | Exit |
| ----------------------------- | ----------- | -----: | ---: |
| ToolWriteOperationTests       | Unit        |     36 |    0 |
| ToolWriteSafetyTests          | Unit        |     15 |    0 |
| ToolWriteRemovePlanningTests  | Unit        |      9 |    0 |
| ToolWriteDeferredRemovalTests | Integration |     11 |    0 |

All 71 cases pass with zero failures, skips or warnings on managed .NET 10.0.1, x64.
The process disables snapshot updates.
`npm run check:dotnet:whitespace` and `npm run check:dotnet:style` both return exit 0 on final source.
The first build needed a test namespace import. The first new Unit run needed the invocation prefix included in its command-vector expectation. Both are corrected.

The patch is `artifacts/lane-d-remove-fix.patch` against the core above.
The 12-file C# source manifest is `artifacts/lane-d-remove-source-manifest.sha256`, SHA-256 `015BAB6F4103EAD84DC07242AE3AAFB5120904DC8A0C8C277CD3C46BF62914C0`.
LF, UTF-8 and alternate-index patch application checks pass. The normal index stays empty.
E owns the actual project-only journey rerun and final help captures.
Bindings, wording, protected Framework types, Root, C Inspector and F hosts stay unchanged.

The [lane packet](../packets/lane-d.md#account-removal) keeps the complete receipt.
