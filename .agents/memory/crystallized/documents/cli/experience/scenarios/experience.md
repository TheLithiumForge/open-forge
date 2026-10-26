---
open-forge:
  description: "Cross-command experience scenarios"
  tags: [Memory, Document, CLI, Scenario, Evergreen]
---

# Cross-command experience scenarios

These are reviewed user outcomes. Selection does not mean execution passed. Improved targets remain subject to flow validation before new tests. Deferred cases retain only their identity and reason; they are not adopted specifications.

See [scenario conventions](_scenarios.md) and the [complete assessment](../assessment.md).

## X01

**Situation:** Paste a native Skill and index it unchanged

**Disposition:** Added. Worth retaining as an independently checked user outcome: Index the recognized native package, expose it through Skills, and allow its intended source to be inspected/read.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Paste a native Skill and index it unchanged

### Starting point

Copy docs/cli-experience-fixtures/content/native-skill/team-notes, including its plain Markdown reference file and asset, to .agents/skills/team-notes. Its SKILL.md has native name and description only, not an Open Forge metadata block.

Fixture: `W1`.

### Steps

1. open-forge index
2. open-forge route list skills --depth=all
3. open-forge route inspect .agents/skills/team-notes/SKILL.md
4. open-forge context .agents/skills/team-notes/SKILL.md
5. open-forge index

### Expected result

Index the recognized native package, expose it through Skills, and allow its intended source to be inspected/read. Keep its native metadata and all resource bytes unchanged. The repeated index is a no-op.

### What the person sees

Use the Index apply/current rules. A valid native header without Open Forge fields is not a metadata error. No assistance message is emitted for untouched native resources.

### Verification

- Compare every copied file byte-for-byte before and after all five commands.
- Compare the Skills entry against the native package identity and description. Do not independently index package support Markdown as ordinary routed leaves.
- Record actual source IDs before using them; the explicit SKILL.md path prevents a guessed-ID dependency.

Record actual outcome, state, and communication separately from this specification.

## X02

**Situation:** Index fills metadata in a plain note

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

ROUTES has `.agents/guidance/my-note.md` with a heading and no header. Keep the original body bytes.

### Steps

1. Run `open-forge index`.
2. Read the note through Context.
3. Run the printed Doctor command, then repeat Index.

### Expected result

Index inserts heading-derived description and `[Guidance]` in the selected form. It prints `metadata completed, your content was kept`. The note is listed, Doctor is clean and repeat Index changes no bytes.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## X03

**Situation:** Complete only missing metadata

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Separate description-only and tags-only valid headers include comments, an unknown field and distinctive bodies.

### Steps

1. Index each copy.
2. Compare supplied fields and body slices.
3. Run Doctor and repeat Index.

### Expected result

Only absent fields are inserted. Existing description, tag order, unrelated YAML and body bytes remain exact. Both copies converge without missing-metadata warnings.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## X04

**Situation:** Preview metadata and navigation together

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Use X02 before its first Index.

### Steps

1. Run `open-forge index --dry-run`.
2. Compare the entire workspace and data home.
3. Apply Index and compare the intended effects.

### Expected result

Preview shows inserted fields, Entries changes and `No files were changed.` No lock, bundle, directory, temporary file or metadata is created. For unchanged inputs the apply plan matches.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## X05

**Situation:** Keep absent, partial, malformed and unavailable metadata distinct

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Four separate ROUTES copies contain a plain note, a valid partial header, an unclosed header and a genuinely unreadable required source.

### Steps

1. Index each copy.
2. Run Doctor on each resulting state.
3. Correct malformed frontmatter or release the denied read, then follow the printed continuation.

### Expected result

Absent and partial fields are completed. Readable malformed ordinary frontmatter is untouched, has one `index.metadata-skipped` and yields incomplete exit 3 even alone. Required unreadability keeps its distinct strict read boundary. Neither is called a failed Framework update. Doctor names the exact authoring edit and subsequent Index.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state. Prove denied reads with the invoking account, using a real exclusive handle or ACL boundary.

## X06

**Situation:** Subtract startup context without hiding newly selected material

**Disposition:** Added. Worth retaining as an independently checked user outcome: Return selected material not already included at startup, in its defined order; do not drop the new leaf or repeat the startup set.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Subtract startup context without hiding newly selected material

### Starting point

Use a selected branch with some startup-loaded ancestors and one known non-startup leaf; include a valid overwrite pair.

Fixture: `ROUTES`.

### Steps

1. open-forge context guidance/notes --additions-only
2. open-forge context guidance/notes --content=paths --additions-only

### Expected result

Return selected material not already included at startup, in its defined order; do not drop the new leaf or repeat the startup set.

### What the person sees

Use Context output/content-part rules without suppressing the requested additions or adding an unrelated success wrapper.

### Verification

- Independently compute selected closure minus startup closure.
- Compare logical identity deduplication separately from physical layer contribution.

Record actual outcome, state, and communication separately from this specification.

## X07

**Situation:** Preserve authored prose around the index-managed list

**Disposition:** Improved and added. The preservation case is sound but must not import external output-05 or metadata-assistance policy.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Preserve authored prose around the index-managed list

### Starting point

Use docs/cli-experience-fixtures/content/entrypoint-with-prose.md as a valid route entrypoint and bind its actual children. It has prose before and after the first contiguous dash-space list, followed by a separate authored list.

Fixture: `ROUTES`.

### Steps

1. Add one valid child.
2. open-forge index
3. open-forge index

### Expected result

**Reviewed target:** Update only the accepted heading-owned generated Entries list span, preserving all surrounding authored bytes and later authored lists. Repeat indexing without byte changes. Treat any marker migration under the existing repository contract and do not add metadata assistance.

### What the person sees

Explain the independently observed result, useful warning, or genuinely necessary stop. Avoid requiring the user to repair unrelated content. Exact copy remains to be reviewed against the current interface.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.

## X08

**Situation:** Count actual files and directories after install

**Disposition:** Added. Worth retaining as an independently checked user outcome: Every number refers to a named set: installed content files, generated control file, created directories strictly below .agents, and host-file changes are separate.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Count actual files and directories after install

### Starting point

Use both a fresh workspace and one with existing AGENTS.md. Capture a full no-follow inventory before and after install, including hidden entries.

Fixture: `W0`.

### Steps

1. open-forge install --automatic
2. Independently enumerate paths and entry kinds.
3. open-forge status

### Expected result

Every number refers to a named set: installed content files, generated control file, created directories strictly below .agents, and host-file changes are separate. Do not blindly substitute historical 22/19 for 21/20.

### What the person sees

Resolve all count wording from Install output and the shared count populations. The reported file and directory sets must match an independent inventory.

### Verification

- Check the pre/post set difference and path kind for each receipt.
- Verify claims for replaced, kept and created host regions independently.
- Do not use the CLI headline, snapshot title or old source archive file count as the count oracle.

Record actual outcome, state, and communication separately from this specification.

## X09

**Situation:** Tell the truth about interruption before and after effects

**Disposition:** Added. Worth retaining as an independently checked user outcome: Cancellation and ordinary failure retain different event meanings.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Tell the truth about interruption before and after effects

### Starting point

For each selected mutation, run independent pre-effect cancellation, after-one-effect cancellation, and after-one-effect ordinary failure. Preserve the exact synchronized boundary.

Fixture: `FAULT-AFTER-EFFECT`.

### Steps

1. Start the exact scenario command in a disposable environment.
2. Deliver the selected cancellation or fault at the observed boundary.
3. Capture streams, exit, actual filesystem effects, settings writes and recovery inventory.

### Expected result

Cancellation and ordinary failure retain different event meanings. No-change language is allowed only when no persistent effect happened. Partial work, saved permissions and unknown recovery disposition must remain visible.

### What the person sees

Choose pre-effect or post-effect wording from verified stage and event. A saved permission grant prevents a global no-change claim.

### Verification

- A timer alone does not prove the boundary; require deterministic synchronization or mark blocked by fixture.
- Inspect actual effect receipts and settings separately from content.
- Do not assume every command supports rollback or that every failed operation retains a usable bundle.

Record actual outcome, state, and communication separately from this specification.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## X10

**Situation:** Keep a copied Template independent

**Disposition:** Added. Retain Template-copy independence; place the copied scenario under memory/working so its destination role matches its content.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Keep a copied Template independent

### Starting point

Use the generalized scenario Template with distinct metadata and copyable body. memory/working is an existing routed destination for the independent scenario record; the new leaf has its own description and tags.

### Steps

1. open-forge route create memory/working/new-note --description "Scenario for adding team notes" --tag=Scenario --template templates/example
2. Edit only the original Template.
3. Read the created destination.

### Expected result

The created file retains destination metadata and the copied starting body, then evolves independently. Later Template edits do not synchronize into it.

### What the person sees

Use the applicable copy/create/index output rules; no later template change is described as updating an already independent copy.

### Verification

- Compare destination bytes before and after editing only the Template.
- Fill/remove instructional placeholders before treating the copied document as a finished scenario.

Record actual outcome, state, and communication separately from this specification.

## X11

**Situation:** Keep failures in another workspace out of this request

**Disposition:** Added. Worth retaining as an independently checked user outcome: Each operation uses exactly the selected workspace.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Keep failures in another workspace out of this request

### Starting point

Create sibling workspaces A and B; A is healthy, B has a malformed entrypoint. Also create an ordinary subdirectory under A with no local installation.

Fixture: `TWO-WORKSPACES`.

### Steps

1. From the subdirectory, run open-forge status without --workspace.
2. From B, run open-forge status --workspace "$WS_A".
3. Run a dry-run index explicitly against A.

### Expected result

Each operation uses exactly the selected workspace. No parent discovery, hidden root switching or contamination from B is permitted.

### What the person sees

Only the selected workspace appears in reports, findings, limits and continuations.

### Verification

- Compare both trees and source inventories.
- Verify a missing local installation is not quietly replaced by the parent installation.

Record actual outcome, state, and communication separately from this specification.

## X12

**Situation:** Ignore unrelated corrupt old records, but not required safety facts

**Disposition:** Improved and added. An inert .log cannot prove useful work survives unrelated routed corruption.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Ignore unrelated corrupt old records, but not required safety facts

### Starting point

Use a valid custom package with effects confined to .agents/guidance/team and a malformed YAML Markdown file under an untouched patterns branch. Verify no selected projection or required ancestor depends on that malformed file. Keep a separate copy with a genuinely required package input unreadable.

### Steps

1. open-forge extension install toolkit --source "$CAT" --automatic
2. Compare the malformed untouched patterns file byte-for-byte and verify the selected package effects.
3. On a separate copy, make one source or target fact required by this exact operation unavailable and repeat.

### Expected result

**Reviewed target:** Complete the explicitly selected safe package operation despite malformed Markdown in an untouched independent route. Preserve and report the unrelated defect without repairing it. A separate counterpart with an unavailable fact actually required by the selected source, target, permission or generated region must stop only the work that depends on that fact.

### What the person sees

Explain the independently observed result, useful warning, or genuinely necessary stop. Avoid requiring the user to repair unrelated content. Exact copy remains to be reviewed against the current interface.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## X13

**Situation:** Treat an unusable ownership lock as unknown claims, not a universal stop

**Disposition:** Improved and added. The case should specify user-visible unknown ownership, not arbitrate conflicts among external proposed tables.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Treat an unusable ownership lock as unknown claims, not a universal stop

### Starting point

Use separate absent, unreadable, malformed and uninterpretable lock fixtures. Also include a valid readable empty lock and a valid lock with a known different ID.

Fixture: `OWNERSHIP-VARIANTS`.

### Steps

1. Run each selected read-only command on unchanged copies.
2. Preview each applicable mutation with otherwise safe explicit inputs.
3. Compare claims, statuses, unknown values and any publication receipts.

### Expected result

**Reviewed target:** Keep absent, unreadable, malformed and unusable ownership distinct from known empty or known different-ID records. Continue safe read-only information and explicit safe create/install/attach work when its real targets and authority are known. For Library Attach, an absent lock is known empty; invalid or unavailable required ownership blocks before effects even with explicit source and destination. Claim-dependent sync, detach or deletion makes no guessed selection. Never reconstruct ownership from matching files, overwrite conflicts or erase unrelated known claims.

### What the person sees

Explain the independently observed result, useful warning, or genuinely necessary stop. Avoid requiring the user to repair unrelated content. Exact copy remains to be reviewed against the current interface.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.

**Execution constraint:** Use current repository ownership behavior as baseline; any differing target requires user validation. A valid publication must preserve independently known unrelated claims. Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

## X14

**Situation:** Distinguish one-time consent, persistent consent and ownership

**Disposition:** Added. Worth retaining as an independently checked user outcome: Consent is destination-scoped and separate from selection, final confirmation, force and ownership.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Distinguish one-time consent, persistent consent and ownership

### Starting point

Use an external docs/team destination. Prepare independent cases for no grant, once, always, explicit --allow-path, revoked grant and malformed settings.

Fixture: `PERMISSIONS`.

### Steps

1. Attempt without permission in redirected execution.
2. In a real terminal, test once and always on separate copies.
3. Test explicit --allow-path with apply and dry-run.
4. Revoke the grant after a successful owned link or package operation and try the next mutation.

### Expected result

Consent is destination-scoped and separate from selection, final confirmation, force and ownership. Once is not saved; always/explicit grants persist only when actually written; dry-run never writes. Revocation matters to later owned operations.

### What the person sees

Use the permission decision and publication facts, not the same sentence for once, persistent and declined consent.

### Verification

- Compare settings bytes and the exact grant scope.
- A broad allowed parent still does not authorize .git, reserved controls, source trees or unsafe ancestry.
- Malformed settings withhold external grants without destroying implicit .agents admission; never overwrite them to save always approval.

Record actual outcome, state, and communication separately from this specification.

**Execution constraint:** Use a genuine capability-appropriate terminal and preserve actual prompt/signal evidence.

## X15

**Situation:** Keep a copyable next action complete and relevant

**Disposition:** Improved and added. Missing optional description or tags is no longer a required-input failure.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Keep a copyable next action complete and relevant

### Starting point

Use a genuinely missing route-create target, a custom-source package, an Extension scaffold failure and an exact source path containing spaces.

### Steps

1. Capture the minimal response for each concrete state.
2. For any purported runnable next command, bind genuinely missing user choices and run its exact arguments on an identical disposable copy.
3. Verify the command addresses the stated obstacle without changing workspace or granting additional authority.

### Expected result

**Reviewed target:** A suggested runnable action preserves the known subject, workspace, source and required arguments, uses valid quoting and addresses the actual obstacle. Ask for genuinely missing identity or authority; optional metadata must not manufacture a correction step. Do not present placeholders or invented metadata as executable commands.

### What the person sees

Explain the independently observed result, useful warning, or genuinely necessary stop. Avoid requiring the user to repair unrelated content. Exact copy remains to be reviewed against the current interface.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.

## X16

**Situation:** Keep required affected navigation inside the repair gate

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Fork one unrelated Library drift plus one exact relink, and one unsafe generated boundary in the proven rename's affected route.

### Steps

1. Run Doctor.
2. Preview automatic Repair, then apply if its plan is executable.
3. Follow remaining actions and run Doctor.

### Expected result

Unrelated damage remains visible while independent relinks proceed. An unsafe affected navigation target blocks the entire combined relink and navigation plan before effects. A readable malformed unrelated ordinary source uses the explicit skip lane rather than a guessed rewrite.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state. Verify source literals, target expectations and required closure membership, not merely exit code.

## X17

**Situation:** Detach known links when their source folder is unavailable

**Disposition:** Added. Worth retaining as an independently checked user outcome: List reports source availability; inspect/sync cannot establish a complete source inventory.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Detach known links when their source folder is unavailable

### Starting point

Keep a valid team registration and each registered symlink entry. Remove or rename the source root so the links are dangling, not absent.

Fixture: `LIB`.

### Steps

1. open-forge library list
2. open-forge library inspect team
3. open-forge library sync team --automatic
4. open-forge library detach team --automatic

### Expected result

List reports source availability; inspect/sync cannot establish a complete source inventory. Detach uses known recorded link identity without needing source content and removes only those links when all destination checks and permissions pass.

### What the person sees

Use the source-independent detach rule for an existing registered link. Do not conflate a missing source with a missing destination entry.

### Verification

- Inspect links without following them and compare raw relative targets.
- Verify sync performs no inferred retirements.
- Verify detach does not delete or recreate the unavailable source path.

Record actual outcome, state, and communication separately from this specification.

## X18

**Situation:** Preserve a changed Library destination while continuing safe work

**Disposition:** Improved and added. The current sequence makes backup-and-repair mandatory and assumes both commands must stop globally.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Preserve a changed Library destination while continuing safe work

### Starting point

Replace one registered symlink with an ordinary file containing unique user edits. Keep at least one other exact registered link current, and add a separate eligible source file with an unoccupied destination. Prepare independent copies with these same states so sync has a safe pending addition and detach has a safe existing link to remove.

### Steps

1. Prepare three independent copies containing the same replaced ordinary file and known unchanged registered links.
2. On copy A, run open-forge library sync team --automatic and verify the replaced file is preserved.
3. On copy B, run open-forge library detach team --automatic and verify the user file remains while eligible links and registration are released.
4. Only on optional copy C, preserve the user file at a deliberate backup path, then run open-forge library sync team --automatic to restore the missing registered link.

### Expected result

**Reviewed target:** On separate copies, sync preserves the replaced user file and continues independent safe mapping changes while reporting the unresolved path. Detach preserves that file, removes other exact registered links, and releases safely identified registration. An optional separate restore flow may back up the user file and restore its missing link; this is not required to detach.

### What the person sees

Explain the independently observed result, useful warning, or genuinely necessary stop. Avoid requiring the user to repair unrelated content. Exact copy remains to be reviewed against the current interface.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.

## X19

**Situation:** Make a second identical operation genuinely a no-op

**Disposition:** Improved and added. The blanket exclusion of remove/detach conflicts with forgiving known-absence no-op targets.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Make a second identical operation genuinely a no-op

### Starting point

For each selected command, prepare an accepted successful first result with no unhandled warning, then repeat the identical request on that resulting state.

Fixture: `REPEATABLE`.

### Steps

1. Run the selected scenario once and verify its actual result.
2. Capture a full inventory.
3. Run the identical argv again.
4. Compare the resulting inventory and output.

### Expected result

**Reviewed target:** An identical converged request performs no unnecessary file, list, settings or ownership writes. Exact already-absent remove/detach targets with trustworthy resolution return a clear no-op rather than inventing a deletion or requiring repair. Invalid IDs, ambiguous identities and unavailable ownership retain their distinct limits.

### What the person sees

Explain the independently observed result, useful warning, or genuinely necessary stop. Avoid requiring the user to repair unrelated content. Exact copy remains to be reviewed against the current interface.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.

## X20

**Situation:** Keep detail and JSON views about the same outcome

**Disposition:** Added. Worth retaining as an independently checked user outcome: Detail changes explanation, never the selected operation, status, exit, actual effects or underlying counts.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Keep detail and JSON views about the same outcome

### Starting point

Use identical starting copies for a success, a warning, an incomplete result and a typed invalid request. Keep real parser failures as their own separate case.

Fixture: `VIEW-VARIANTS`.

### Steps

1. Capture default text, explicit minimal, standard, full and debug.
2. Capture JSON for each defined level.
3. Repeat with error, warning, info and all detail filters.

### Expected result

Detail changes explanation, never the selected operation, status, exit, actual effects or underlying counts. Full/debug primary payloads agree apart from declared detail metadata; debug diagnostics go to stderr. Typed JSON remains one schema-3 stdout envelope. Per-scenario richer goldens not authored here are deferred.

### What the person sees

Resolve all presentation expectations through the same output rule at every detail/format. Omitted filtering and explicit all have different listing selections.

### Verification

- Parse JSON and compare invariant fields and null-versus-zero meaning.
- Use same pre-state clones for mutation variants; never compare minimal apply with a later no-op full run.
- Record stdout, stderr and exit separately; help/parser errors do not become fake domain envelopes.

Record actual outcome, state, and communication separately from this specification.

## X21

**Situation:** Allow one live writer per workspace without mistaking a lock file for a writer

**Disposition:** Added. Worth retaining as an independently checked user outcome: The second same-workspace writer is blocked while the live lock is held, then can proceed after release.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Allow one live writer per workspace without mistaking a lock file for a writer

### Starting point

Use two live processes in the same disposable workspace and a third process in a different workspace. Retain the persistent lock file after the first process exits.

Fixture: `LOCK`.

### Steps

1. Hold a genuine first mutation at a reproducible boundary.
2. Start the second mutation against the same workspace.
3. Release or terminate the first process safely.
4. Retry the second command with the persistent lock file still present.

### Expected result

The second same-workspace writer is blocked while the live lock is held, then can proceed after release. File existence alone is not contention. An independent workspace must not inherit that lock.

### What the person sees

Describe an observed live lock holder, not merely a lock pathname. The continuation does not tell the user to delete a possibly active lock.

### Verification

- Prove actual OS lock ownership and its release.
- Compare all target sets before and after contention.
- Require a real concurrency fixture; a text file named lock is insufficient.

Record actual outcome, state, and communication separately from this specification.

**Execution constraint:** Demonstrate live same-workspace lock ownership and release; a lock file alone is insufficient.

## X22

**Situation:** Do not confuse external links with unsupported or broken local links

**Disposition:** Added. Worth retaining as an independently checked user outcome: HTTPS is a known unchecked external occurrence without a network request.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Do not confuse external links with unsupported or broken local links

### Starting point

Use one HTTPS URL, one unsupported non-HTTP scheme, one missing local file and one missing local heading fragment in separate controlled cases.

Fixture: `LINKS`.

### Steps

1. open-forge references guidance/notes --direction=out
2. open-forge doctor
3. For the explicitly selected local-link case only, run the supported follow-links context request.

### Expected result

HTTPS is a known unchecked external occurrence without a network request. Unsupported schemes, missing files and missing fragments retain different facts and consequences. No unchecked URL becomes a validated target.

### What the person sees

Keep unvisited external links, unsupported schemes and checked-broken local links distinct in findings, counts and completion.

### Verification

- Verify network-not-attempted behavior and local filesystem/heading facts independently.
- Do not turn generated navigation into authored incoming links.

Record actual outcome, state, and communication separately from this specification.

## X23

**Situation:** Keep literal content literal, including whitespace and Unicode

**Disposition:** Added. Worth retaining as an independently checked user outcome: Generated UI prose follows the writing rules; authored payload is not sanitized to satisfy those rules.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Keep literal content literal, including whitespace and Unicode

### Starting point

Use variants with LF, CRLF, no final newline, Unicode names, spaces, a tab, backticks, literal code-looking text and the words lifecycle and payload inside authored content.

Fixture: `LITERAL-CONTENT`.

### Steps

1. Read the content through context and requested find content.
2. Apply a metadata-only route update.
3. Index a stale parent list.

### Expected result

Generated UI prose follows the writing rules; authored payload is not sanitized to satisfy those rules. Content and unrelated byte regions remain intact through reading and bounded edits.

### What the person sees

Authored content is not a generated message template. Preserve its declared exact bytes through reading and indexing.

### Verification

- Compare decoded structure and raw bytes where their contracts differ.
- Compute UTF-8 byte offsets and one-based Unicode-scalar locations independently.
- Verify no accidental JSON escaping in human content or unsafe truncation of paths.

Record actual outcome, state, and communication separately from this specification.

## X24

**Situation:** Discover commands and correct mistakes without hidden work

**Disposition:** Added. Worth retaining as an independently checked user outcome: Help/version terminate before workspace validation or mutation.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Discover commands and correct mistakes without hidden work

### Starting point

Use a nonexistent workspace, wrong syntax, missing required semantic values and a valid terminal help request as distinct states.

Fixture: `W0`.

### Steps

1. open-forge --help
2. open-forge route create --help
3. open-forge library attach --help
4. open-forge --version
5. Attempt the chosen invalid request, then make the explicit user correction.

### Expected result

Help/version terminate before workspace validation or mutation. Available commands come from actual public availability; missing values are explained together. A corrected request retains already supplied facts and does not switch workspaces.

### What the person sees

Use the command help/input-correction rules without invoking domain work just to display help or version.

### Verification

- Verify no source reads or writes required merely to display terminal help where the contract exempts them.
- Run shown examples only after binding real user inputs, not placeholders.

Record actual outcome, state, and communication separately from this specification.

## X25

**Situation:** Add warning detail without repeating the same instruction

**Disposition:** Improved and added. Historical external captures should not become approved copy or substitute for a real fixture.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Add warning detail without repeating the same instruction

### Starting point

Use focused current fixtures for route-remove link protection, unreadable cleanup inventory, unavailable Library ownership and healthy/warning Status. Historical captures are optional observations only.

### Steps

1. Capture each selected current fixture at minimal, standard, full and debug detail on unchanged copies.
2. Read each complete report and compare meaningful additions.
3. Record duplicated narrative separately from necessary repeated source identities.

### Expected result

**Reviewed target:** Use independently specified fixtures and compare complete reports across detail levels. Additional detail adds useful evidence without duplicating the same cause, consequence or next action. Repeated identities are allowed where separate lists or occurrences need them; no external capture or exact message text is adopted.

### What the person sees

Explain the independently observed result, useful warning, or genuinely necessary stop. Avoid requiring the user to repair unrelated content. Exact copy remains to be reviewed against the current interface.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.

## X26

**Situation:** Test grant persistence independently from later content failure

**Disposition:** Added. Worth retaining as an independently checked user outcome: The report distinguishes saved permission from failed content.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Test grant persistence independently from later content failure

### Starting point

An explicit eligible --allow-path grant can be saved, then a reliably synchronized later content effect fails. Include a counterpart in which saving the grant itself fails.

Fixture: `PERMISSIONS`.

### Steps

1. Apply the grant-and-content request with the selected fault boundary.
2. Inspect settings, target effects, recovery and each receipt.

### Expected result

The report distinguishes saved permission from failed content. It must not say Nothing was changed after a real settings write, and must not say permission was saved when that write failed.

### What the person sees

A verified persistent permission write is reported even when later content work fails or is declined. Once-only consent writes no settings.

### Verification

- Compare settings before/after independently of package/link effects.
- Do not rely on rollback assumptions or collapse all writes into a files-installed count.

Record actual outcome, state, and communication separately from this specification.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## X27

**Situation:** Keep unrelated owners and support files through a full package lifecycle

**Disposition:** Added. Worth retaining as an independently checked user outcome: Only selected eligible ownership effects occur.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Keep unrelated owners and support files through a full package lifecycle

### Starting point

Package content includes routed Markdown and opaque support files. A second owner legitimately shares one file, and an unowned neighbor sits beside the destination. Include a Library source tree in the protected-path counterpart.

Fixture: `PACKAGES`.

### Steps

1. Install the package and index.
2. Update it from a verified changed source.
3. Remove the selected package.

### Expected result

Only selected eligible ownership effects occur. Shared files stay for remaining owners; unowned neighbors and source trees survive; support files are not forced into ordinary routed-frontmatter rules.

### What the person sees

Use owner-specific receipts across install/update/remove and preserve every unrelated owner, resource and unclaimed file.

### Verification

- Compare all three inventories across the complete lifecycle, not only command snapshots.
- Verify registered Library source trees remain protected despite a broad destination grant.

Record actual outcome, state, and communication separately from this specification.

## X28

**Situation:** Tell a zero result from an unknown or not-requested result

**Disposition:** Added. Worth retaining as an independently checked user outcome: Zero is used only for a completed empty set of the stated kind.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Tell a zero result from an unknown or not-requested result

### Starting point

Use separate complete-empty, incomplete-scan, unavailable-ownership and intentionally excluded/not-requested cases.

Fixture: `VIEW-VARIANTS`.

### Steps

1. Run the applicable read-only command in text and JSON.
2. Compare the displayed count, underlying count value and limitation explanation.

### Expected result

Zero is used only for a completed empty set of the stated kind. Unknown and not-requested facts remain distinct; one unavailable record should not produce five identical limitation sentences.

### What the person sees

Use numeric zero for known empty, null plus a limitation for unavailable required facts, and the documented applicability rule for inapplicable values.

### Verification

- Define the counted set before examining the result.
- Compare distinct affected files versus total findings and checked routes versus skipped routes.

Record actual outcome, state, and communication separately from this specification.

## X29

**Situation:** Report verified content separately from failed ownership publication

**Disposition:** Added. Worth retaining as an independently checked user outcome: Best-effort ownership publication must not erase verified content effects or invent saved registrations.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Report verified content separately from failed ownership publication

### Starting point

Explicit safe inputs produce verified content or links, then a reproducible post-effect ownership publication failure occurs. Use a distinct counterpart where there were no safe selected effects.

Fixture: `PUBLICATION-FAILURE`.

### Steps

1. Run the selected explicit operation at the observed publication-failure boundary.
2. Inspect actual content, link identities, ownership lock state and the next read-only list/inspect result.

### Expected result

Best-effort ownership publication must not erase verified content effects or invent saved registrations. The next command must not infer ownership from matching files. The user needs to know when ordinary management cannot rely on a saved receipt.

### What the person sees

Keep verified content and failed ownership publication separate. Do not claim registration succeeded, rollback happened, or no files changed.

### Verification

- Verify the fault really occurred after content verification rather than before application.
- Compare later management behavior with absent/unusable ownership.
- Do not automatically delete successful content or reconstruct claims as a presentation repair.

Record actual outcome, state, and communication separately from this specification.

**Execution constraint:** Establish a deterministic public process or real filesystem boundary and independently verify before/after effects; no timer-only or mocked-result proof.

## X30

**Situation:** Do not claim a local edit when only the intended source changed

**Disposition:** Added. Worth retaining as an independently checked user outcome: A current content difference is not proof that a user changed a file since installation.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Do not claim a local edit when only the intended source changed

### Starting point

Use three separate histories with known bytes: change only the available source; change only descriptive version metadata; and change only an installed target. Keep the ownership receipts and current intended source identifiable.

Fixture: `VERSION-PAIR`.

### Steps

1. Run status and Extension list/inspect on each case.
2. Compare the current intended-source/target differences with the known fixture history.
3. Preview the applicable update without applying it.

### Expected result

A current content difference is not proof that a user changed a file since installation. A version string change is not by itself a file-content change. Explain the actual comparison and do not invent history or semantic-version ordering.

### What the person sees

Use current-comparison wording. A source-only change is not evidence that the user edited an installed file.

### Verification

- Compare current source bytes, target bytes and authored fixture history independently.
- Keep file counts separate from version-only observations.
- Verify comparison wording under Shared Presentation. A current difference alone does not establish a historical user edit.

Record actual outcome, state, and communication separately from this specification.

**Execution constraint:** Bind package IDs and bytes to a verified fixture catalogue/version pair; toolkit/base are fixture IDs, not assumed bundled packages.

## X31

**Situation:** Every delivered Markdown file equals the canonical payload rendered in the workspace form, checked independently

**Disposition:** Added. Verify delivered file bytes against an independent expectation for the selected form.

**Who:** A maintainer completing the user task, without relying on internal delivery reports.

**Goal:** Verify the complete Markdown delivery in the workspace's selected form.

### Starting point

Use W0 for fresh Framework delivery, or W1 for Extension delivery and Configure conversion. Canonical scoped Framework and selected Extension payloads, their manifests, the effective workspace preference, and the intended route topology are independently available. Record user files and edited owned targets separately before conversion.

Fixture: `W0` or `W1`, with an independently enumerated complete selected delivery inventory.

### Steps

1. Carry out the selected Install, Extension Install, or Configure operation on the real workspace state.
2. Enumerate every selected delivered Markdown target from the canonical payload inventory and manifests, independently of printed effects.
3. Construct expected bytes in the workspace form and compare every delivered target byte for byte.
4. Check generated `Entries` against an independent projection of the resulting route topology. Compare kept edited targets and user files separately with their saved bytes.

### Expected result

Every newly delivered or converted Markdown target equals its canonical source rendered under the [Frontmatter Form contract](../../shared-operation-contract.md#frontmatter-form), with independently projected generated `Entries` where the operation owns navigation. Scoped rendering preserves canonical bytes. Root rendering moves only the leading `open-forge` mapping in eligible `.md` files under `.agents/`, excluding `SKILL.md`. Native Skills and ineligible Markdown retain canonical bytes. Body content outside generated navigation remains byte-identical, including fenced examples.

The independent expectation does not call the production renderer or accept hashes copied from the report. Root collisions with foreign description, responsibility, or tags, or conflicting root `applyTo`, make delivery invalid under the existing command boundary. Equivalent root `applyTo` stays in place and the moved copy is dropped. Kept edited files and independently authored files are preservation checks, rather than claimed conversion effects.

### What the person sees

Delivery or conversion identifies the actual selected files and form. Configure reports kept owned paths and reasons. No report substitutes for the independent comparison or implies that kept user content was converted.

### Verification

**State:** Check the entire independently enumerated delivery set, expected form, exact bytes, and projected navigation. Check native and ineligible assets separately. Compare the edited owned file and user note with their pre-conversion byte copies, and verify that inventory hashes continue to identify canonical repository bytes.

**Output:** Compare reported paths, effects, form, and kept reasons with actual delivery and preservation results. Record outcome, state, and communication separately from this specification.

## X32

**Situation:** Root and scoped forms with partial headers

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Run root and scoped workspaces, including a valid partial header in the other form and a flow mapping. The other-form header is completed in its existing form.

### Steps

1. Preview and apply Index.
2. Run Doctor, then repeat Index.

### Expected result

Insert only absent fields and preserve supplied bytes. Complete partial headers in their existing form, including the other-form branch and flow mappings. New headers use the workspace form. Do not move supplied fields or change reader precedence.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## X33

**Situation:** Headingless filename and category derivation

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Create headingless deploy-checklist.md and moneyRounding.md, a note with Setext heading, and one note for every design category and Memory state.

### Steps

1. Preview and apply Index.
2. Inspect descriptions, ordered tags and generated rows.

### Expected result

Use `Deploy checklist`, `Money rounding` or the first supported heading, with the exact design category table. Derive LoadNow only for ordinary Directive files as required by that category. Do not infer acceptance or business-topic tags, and leave child entrypoints on demand.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## X34

**Situation:** Deep note without entrypoints or metadata

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Create memory/crystallized/platform/payments/stripe/node.md with no intermediate entrypoints or frontmatter.

### Steps

1. Run Doctor and preview its Index action.
2. Apply Index.
3. Read through Context, run Doctor and repeat Index.

### Expected result

Create all three canonical scope entrypoints with prefilled metadata, the inherited marker and final direct-child Entries. Complete the note. One apply makes it reachable and Doctor clean.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## X35

**Situation:** Excluded and component-owned sources stay untouched

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Seed empty and resource-only folders, an omitted route, removedFiles and removedDirectories, a private gitIgnoredRoutes record, managed Markdown, an overwrite and a native Skill package containing plain resources.

### Steps

1. Preview and apply Index with default and explicit selections.
2. Repeat with no Git and with a generally ignored file outside recorded private policy.

### Expected result

Never create or complete excluded, native, managed authored or resource-only content. Keep the shared private entrypoint without private rows. General Gitignore alone does not silently change Index policy.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

Repair applies the same protection boundary to link sources and targets during selection, planning and revalidation. Protected managed or native package bytes remain exact, with the managing Update or authoring action reported.

## X36

**Situation:** Unknown roots and competing entrypoints

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Put a deep note outside recognized roots and another below a folder with two recognized entrypoints.

### Steps

1. Run Doctor, then Index for each declared scope.
2. Resolve the precise boundary and retry.

### Expected result

Do not create a root or choose a canonical preference. Print an action that actually resolves the boundary, without a futile Index retry.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## X37

**Situation:** Unchanged tracked file renamed by hand

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

LINKS has a committed non-empty note and an incoming authored link. Rename the note without editing it.

### Steps

1. Run Doctor.
2. Preview and apply automatic Repair.
3. Read References and run Doctor.

### Expected result

One proven identity is relinked without target selection. The proof says `same content`. Repair refreshes affected Entries and Doctor is clean.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## X38

**Situation:** Move across folders and repair outgoing relative links

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Two tracked notes move into new scopes. One has incoming links and an outgoing relative link whose source directory changed.

### Steps

1. Run Doctor.
2. Preview and apply automatic Repair.
3. Inspect both incoming and outgoing References, then Doctor.

### Expected result

Prove each old/new identity, compute destination from each actual containing file, complete new scope chains and keep labels, fragments and unrelated bytes.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## X39

**Situation:** Two strong successors require a choice

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

A tracked missing note has two current byte-identical successors with different paths.

### Steps

1. Run automatic Repair preview and apply.
2. Follow its interactive Repair next step and select one target.
3. Run Doctor.

### Expected result

Automatic Repair does not pick by order, path proximity or score. The link stays untouched until chosen. The guided continuation can finish without an automatic retry loop.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

When Repair cannot ask, list each candidate once under its own link with the strongest proof or readable reason and a copyable command. Preserve the explicit workspace. Automatic choices point to guided Repair, while a non-interactive choice-only request tells the user to run the chosen command.

## X40

**Situation:** One title or filename guess remains a choice

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

No old content or metadata signature is available. Exactly one candidate has the old filename or matching title.

### Steps

1. Run Doctor and automatic Repair.
2. Provide the measured exact `--relink` intent.
3. Run Doctor.

### Expected result

A singleton weak guess is never called a proven rename. An explicit choice changes only the selected destination and affected navigation.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## X41

**Situation:** No Git with and without stale metadata identity

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

No-Git copies retain a unique valid stale description/tags signature, duplicate that signature, or retain only a title.

### Steps

1. Run Doctor and automatic Repair before Index.
2. Follow guided or exact manual actions when proof is absent.
3. Run Doctor.

### Expected result

Only the unique full admitted metadata signature proves identity. Duplicate or title-only evidence remains guided. No Git is not an indexing failure.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## X42

**Situation:** Index runs before repair

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Fork Git and no-Git workspaces after a manual rename. Run Index to remove the old Entries row.

### Steps

1. Run Doctor and automatic Repair.
2. For no-Git without old identity, supply exact relink intent.
3. Run Doctor.

### Expected result

Git HEAD still proves old content or metadata after Index. No-Git history is not invented. The explicit continuation still reaches clean Doctor.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

Doctor and Repair must agree after Index changes current Entries. A generated successor row does not contradict committed proof. Weak hints stay with their occurrence. A Git timeout postpones Doctor's missing-target findings and keeps Repair's unresolved links visible. Both retry the complete request without manual-removal advice.

## X43

**Situation:** Rename and edit content

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Use a committed multi-line note. Make one candidate score exactly 90, another below 90 without metadata proof, and one below 90 with unique recorded metadata proof.

### Steps

1. Preview and apply automatic Repair where proof is strong.
2. Choose explicitly in the weak branch.
3. Inspect proof text, preserved edited bytes and Doctor.

### Expected result

Apply the design's weighted line score and unique metadata rule. Never say same content for changed bytes. Preserve the user's new body.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## X44

**Situation:** Malformed sibling does not stop a deep valid note

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Place a readable malformed ordinary file beside a valid plain note behind a new chain. Also fork malformed-only and malformed-entrypoint copies.

### Steps

1. Preview and apply Index.
2. Follow its exact frontmatter edit and Index action.
3. Run Doctor.

### Expected result

Emit one skip finding per bad source, complete and list independent valid siblings, and keep malformed bytes exact. Malformed entrypoint-dependent work stays untouched. After explicit correction, Index converges.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

Incomplete Index and Repair instructions keep the explicit workspace inside edit-and-index prose. An incomplete Repair preview with safe independent effects points to applying them first. Applied partial headlines state verified effects and remaining choices or edits.

## X45

**Situation:** Deleted note has no successor

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Delete a referenced note, retaining no strong successor.

### Steps

1. Run Doctor and Index.
2. Follow the precise link-edit action, choosing to remove the reference.
3. Run Doctor.

### Expected result

Index removes only stale navigation. Repair never recreates the note, relinks to a nearby guess or deletes authored link text. The user's named edit reaches clean Doctor.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## X46

**Situation:** Whole folder rename or move

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Fork a folder containing compatibility index.md and a canonical _billing.md. Include several tracked children and incoming and outgoing links.

### Steps

1. Rename or move the folder by hand.
2. Run Doctor, preview and apply Repair, then Index if printed.
3. Review local Axioms for the canonical-filename branch and verify Context and Doctor.

### Expected result

Prove each file independently and preserve compatibility entrypoints. With committed-content or near-identical Git proof, rename the old canonical navigation file and keep its description, Axioms and body. Show one rename row. Without proof, create the new entrypoint and preserve the old file. Doctor then directs the user to copy any wanted rules into the new file before deleting the old one.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## X47

**Situation:** Follow every next step after a mixed batch

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Combine unchanged renames, edited moves, deep plain and partial notes, one deleted note and a renamed compatibility-entrypoint folder in one workspace.

### Steps

1. Run Doctor.
2. Follow each printed preview, apply, Index, guided choice or exact manual edit without resetting state.
3. Run final Doctor and repeat Index and automatic Repair.

### Expected result

The continuation has one executable next action at each stage. Safe work completes, deletion meaning is explicit, Doctor ends complete with no warnings or errors, and repeats are byte no-ops.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## X48

**Situation:** Changed expected state and interrupted combined plan

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Form metadata, entrypoint, relink and Entries effects, then synchronize a source edit, candidate disappearance, HEAD change, lock contention or failure after one verified effect.

### Steps

1. Preview the exact plan.
2. Trigger the real boundary during apply.
3. Inspect residual state and replan from Doctor.

### Expected result

No stale effect overwrites the new bytes. Preflight failure writes nothing. Post-effect failure preserves verified effects and exact recovery facts without rollback. Fresh continuation never replays old evidence.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## X49

**Situation:** Additive JSON and exact text at every detail

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

Use a completion plan, a proven rename, a skip finding and an ambiguous successor in separate copies.

### Steps

1. Run text and schema-3 JSON at minimal, standard, full and debug detail.
2. Preview, apply and repeat each plan.

### Expected result

Existing fields and effect kinds remain. New arrays, counts and evidence use only the design's exhaustive values. Text proof and next actions agree with typed JSON and independently observed effects.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## X50

**Situation:** Explicit selection completes only the required chain

**Disposition:** Accepted current Task 79 behavior under D1 to D4.

### Starting point

A deep guidance leaf and an unrelated deep patterns leaf both need completion. Also provide a complete detached explicit entrypoint selection.

### Steps

1. Index the exact guidance leaf path.
2. Inspect patterns, then run default Index.
3. Maintain the detached selection explicitly.

### Expected result

Explicit leaf completion includes its chain and exposing parent only. Default selection completes both recognized roots. Detached maintenance invents no root or completion authority.

### Verification

Compare actual bodies, supplied metadata, comments, unknown keys, directory inventory and generated rows independently of output. Record streams and exit, then run the printed continuation on the same state.

## Task 86 Runtime Expectations

Writing commands acquire `.open-forge-state/workspace.lock` for the selected physical workspace. They prepare new recovery bundles at `.open-forge-state/recovery/v1/<workspace key>/`. The live handle owns the lock. A leftover or nonempty sentinel does not block a writer. Dry runs, refusals, observers and verified no-ops create no runtime state. The legacy per-user lock bridge ends at 1.0. `OPENFORGE_DATA_HOME` controls only that bridge and old bundles. See the mutation and recovery design for the bridge gap and typed failures.
