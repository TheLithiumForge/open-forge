---
open-forge:
  description: Define the shared source contracts and integration points for the pending Tool implementation lanes
  tags: [Memory, Document, CLI, TechnicalDesign, Tool, CurrentTruth]
---

# Tool lane contracts

Tool lane contracts are shared records and callable interfaces that let the Tool implementation lanes use the same definitions, requests, results and findings.

For example, the registration lane plans a declared hook rewrite. The writer lane includes that edit in consent and recovery. The Run lane launches the same verified workspace pin after the writer records it.

Status: V1 contracts decided. Command implementation pending.

## Definition records

Framework Tools defines the exact source types under `Models/Registration`. `ToolRegistration` groups declared command rewrites, optional components, role guidance and activation commands. `ToolManifest.Origin` uses the `ToolOrigin` tokens. Every definition declares first-party or third-party origin. This core has no production definition.

`ToolEntryRewrite` has four concrete forms. JSON and TOML forms carry an expected command and argument array plus a replacement command and argument array. A hook form carries an exact expected command string and replacement. A marked-line form carries both markers and one exact expected and replacement line. Each rewrite has an ID, host, workspace-relative file and optional component ID. JSON uses an exact JSON Pointer. TOML uses one exact table name. Markers, entry names and unrelated content stay unchanged.

`ToolOptionalComponent` names its explanation, default, finite CLI flag and exact JSON entries to remove when declined. Decline checks the complete expected entry before removing it. It never removes another hook or its containing group. A rewrite linked to a declined component is skipped. Prompt-hook choices map to `--prompt-hook yes|no`. Definitions don't register arbitrary CLI flags.

`ToolRoleGuidance` declares one role and its conditional line. The shared block renderer selects lines from the active role mapping in ordinal role order. It adds each line once to both managed agent blocks and removes it when the role is cleared. Keep authored content and the tool's separate marked block. Add no help pointer.

`ToolRoleGuidanceRequest` carries the workspace layout and complete desired active-line set. An empty set clears conditional lines. `ToolRoleGuidanceServices` exposes planning and passive observation. Its plan carries expected prior bytes, including an absent file, and typed line checks. Writers apply those edits under normal permission, exclusion and backup rules. The required Skill stays a separate content effect.

`ToolRegistration.Activate` and `Deactivate` declare role-integration commands. Choose uses these reviewed commands to replace or clear recognized role integrations without deleting either provider's stored bytes or index. Missing required declarations block switching. Test this with a second synthetic provider. No Codebase Memory definition is needed for this test.

## Rewrite contract

Framework `Models/Registration` defines the request, edit, observation and result types. Operations supplies the selected declarations, component choices and resolved template values. Framework plans exact edits and returns typed checks. It grants no destination permission and runs no child command.

After an installer runs, require its exact declared original before changing an entry. Reject duplicate or ambiguous selectors. For TOML, replace only the `command` and `args` lines in the named table. Require the expected simple shape. Use text checks with no TOML parser. A changed, missing or unexpected entry stops further writes and keeps the installer outcome in the result.

Recheck expected whole-file bytes before each write. Validate the edited document and selected values afterwards. Record the resolved replacement values actually verified in `ToolRewriteReceipt`, with no unexpanded templates and without claiming whole-file ownership. Status and Doctor compare those parts for drift. Component-decline receipts record the expected absence. Consent and Inspect disclose each entry, rewrite and removal.

`${openForge}` resolves to the Open Forge command available on PATH or the absolute running executable when it isn't on PATH. The host supplies that fact. It doesn't change PATH. JSON and TOML arguments stay arrays. Hook and marked-line replacements use their reviewed host command syntax and keep the substring needed by the tool's own uninstaller.

## Records and verification

Settings declare pins, roles, telemetry and component choices. A declaration copied through Git doesn't establish a local installation. The lock records the exact recipe, verified file hashes, declared rewrites, component choices, required Skill and command outcomes. Account-local `ToolLaunchBinding` records the verified executable, fixed launcher arguments, environment and file stamps for that physical store. Account paths don't enter portable settings.

Run requires matching settings, lock and account-local pin identity. It checks every recorded immutable member's size and UTC write timestamp. Missing or changed facts block dispatch. Status performs full hashes. A successful full verification can refresh account-local launch stamps only through an authorized writer. Status remains read-only. A matching timestamp is a cheap check within the Architecture's ordinary-edit threat model, not a defence against a malicious same-user process.

The verified executable can be the bundled runtime with a fixed launcher script argument. Both belong to the verified store. This keeps Windows launchers shell-free. A definition never chooses the user's runtime or a PATH-resolved tool executable.

`ToolTarget.Launcher` declares the contained executable and fixed argument array when the entry needs a bundled runtime. Omission keeps the direct target-entry form. `ToolLifecycle.RemovalOrder` declares both uninstall and index cleanup exactly once. A writer requires that order before removal. Reading an older definition with no registration or removal-order declaration doesn't authorize a write.

Optional `TelemetryOn` and `TelemetryOff` commands declare the tool's native setting calls. The command evidence vocabulary also covers activation, deactivation and these telemetry calls. The lifecycle-preparation record carries registration declarations, fixed launcher arguments and the Open Forge launch value. Consuming these additions in the preparer and writers is pending. No new command runs in this contract step.

Git workspaces create no `.bak`. Outside Git, Open Forge's file edits use sibling backups. These cover declared entry changes, required Skills and conditional block lines as well as records and ignores. Tool-owned child effects aren't rolled back through guessed inverse commands. Keep recovery v1 unchanged.

## Command requests

Each writing leaf has its concrete request and result under its own `Models` folder in Operations. `ToolWriteOptions` carries preview, final-confirmation, process-consent, interaction permission, lock-retry policy and destination-grant input. Artifact consent stays on Install and Update. Component choices stay on Install, Configure and Update. Choose takes an exact role and provider or `none`.

`ToolInstallRequest.Id` is nullable for team setup. Without an ID, select every settings declaration in ordinal ID order at its exact lock-verified pin. Reject source, pin, host and root overrides in this form. Use saved component choices. Require an unresolved component's flag non-interactively. An empty declaration set completes without effects. Bare Install returns one per-pin result in `tools`, and aggregate status follows the existing precedence. No default catalogue pin replaces a team declaration.

`ToolWriteData` defines the shared family payload. Each single-tool result gives mode, ID, role, pin, plan, permissions, consents, verification, remaining work and instructions. Install additionally carries per-tool results for team setup. Commands form their own concrete results and renderers consume them. No universal mutation engine or cross-command result type is added.

Writer previews use `ToolLifecycleDescription` and `ToolProcessDescription` before artifact members have verified hashes. These carry the exact command paths, arguments, environment, CWD and timeout. A description can't be passed to a process host. Execution still uses `ToolLifecyclePreparation` and requires the verified executable hash. Both paths use the same command-description logic.

`ToolWriteReport` projects each concrete writer result into `Commands.Tool.Models.Writing`. Rendering reads those report facts, including per-pin Install items and resolved machine finding codes. The frozen command requests, results and payload records stay unchanged. This projection adds no operation, effect or detail policy.

For bare Install, the aggregate ID and pin are null. Each item in `tools` carries its exact ID, status and single-tool data. Applied effects stay separate from proposed changes. Consent records can carry an exact artifact descriptor or account-local process-scope digest. The result reports which backup path was used.

## Run dispatch

`ToolRunRequest` separates an optional workspace selector, current directory, host project directory, declared tool ID and forwarded arguments. Only options before the ID are Open Forge input. Strip at most one delimiter immediately after the ID. Everything after that is child input, including `--help`.

Workspace selection uses explicit `--workspace`, then `CLAUDE_PROJECT_DIR`, then an upward search from CWD. Keep the process CWD for the child. The first operand must be a declared tool ID with installed verified bytes. Reject paths, shell strings and undeclared or unknown names. Resolve only the account-local executable for the workspace's lock-verified pin.

`ToolDispatchProcess` is a separate host capability from bounded lifecycle execution. It forwards raw stdin, stdout, stderr, signals and exit code without output capture, a timeout or success text. It receives an absolute executable and an argument array, never a shell command. The root host implements process and signal handling. Framework and Operations don't write streams.

`ToolRunResolution` gives either a verified launch or a typed failure. Every failure after workspace selection carries that same resolved workspace so an installation offer never selects it again. The [Run interface](../contracts/tool/run/interface.md#failures) defines pre-launch exits. Once launched, the child owns the exit code. No schema-3 envelope wraps its streams. Interactive missing-tool handling offers normal Install consent. Non-interactive dispatch never installs silently.

## Findings

The exact machine suffixes live in `ToolReadIssueCodes`, `ToolContractCodes` and `ToolRunFailureCodes`. Writer prefixes are `tool-install`, `tool-configure`, `tool-choose`, `tool-update` and `tool-remove`. Run uses `tool-run`. Root Status and Doctor use their existing prefixes and the Tool-specific constants in `ToolContractCodes`. These situations stay separate:

- Declared tool missing - Tool Status, workspace Status, Doctor and Run give the exact next step `open-forge tool install`.
- Rewrite drift - the recorded rewritten command or declined entry differs from current files.
- Component choice missing - a required non-interactive choice has no saved or supplied value.
- Unexpected entry shape - a post-installer target doesn't match its declared original.
- Launch integrity mismatch - a recorded immutable member's cheap facts or full hash differ.
- Role guidance drift - the conditional managed line differs from the active mapping.
- Excluded output - a required command output, Skill, rewrite or role line is covered by a removal exclusion.

Read findings keep existing command prefixes. Writer findings keep the writing leaf's prefix. Run has its own finite failure codes. Root Status and Doctor adapt the same missing and drift facts without starting a process.

Writer findings also distinguish missing final confirmation, unavailable workspace selection and unavailable workspace locking. These add separate `ToolWriteIssueCode` members. They don't reuse download consent, destination permission or store-coverage findings.

## Implementation lanes

The remaining implementation can use three independent areas:

- Dispatch - Tool Run parsing, workspace resolution, verified launch, raw process host and missing-tool install offer.
- Registration - exact post-install rewrites, optional components, passive drift observations and conditional managed role lines.
- Writers - Install, Configure, Choose, Update and Remove planning, consent, records, store references, managed ignores and exclusions.

Root composition and rendering integration need a named integrator. Preserve the accepted dependency direction. General private Node runtime preparation waits for the maintainer and isn't part of these slices.

The [external-tools design](external-tools.md#decisions-2026-11-13) records the decisions and issue history.
