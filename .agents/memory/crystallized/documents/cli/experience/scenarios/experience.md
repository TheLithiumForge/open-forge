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

**Situation:** Index a plain note without rewriting it

**Disposition:** Improved and added. Indexing must not insert unsolicited metadata or require an enrichment ritual.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Index a plain note without rewriting it

### Starting point

Copy docs/cli-experience-fixtures/content/plain-note.md to .agents/guidance/my-note.md without adding frontmatter. Keep the body bytes as the authorship oracle.

Fixture: `ROUTES`.

### Steps

1. Copy the plain-note fixture to .agents/guidance/my-note.md without frontmatter.
2. open-forge index
3. Inspect the note bytes and generated guidance Entries.
4. open-forge index
5. Optionally, in a separate branch, author a description or tags and reindex. This is not required for the note to be usable.

### Expected result

**Reviewed target:** Index the plain readable note with a real source identity and a concise missing-metadata warning. Preserve the entire file byte-for-byte. A repeated index is a navigation no-op even while optional metadata remains absent; optional later enrichment changes only fields the user authors.

### What the person sees

Explain the independently observed result, useful warning, or genuinely necessary stop. Avoid requiring the user to repair unrelated content. Exact copy remains to be reviewed against the current interface.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## X03

**Situation:** Index partial metadata without changing authored values

**Disposition:** Improved and added. Partial metadata already carries authored meaning and should remain untouched by index.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Index partial metadata without changing authored values

### Starting point

Run two separate copies: partial-description.md has an authored description but no tags; partial-tags.md has authored tags but no description. Both have distinctive body text and an unrelated metadata field.

Fixture: `ROUTES`.

### Steps

1. Run independent copies for a description-only header and a tags-only header.
2. Copy the selected fixture to .agents/guidance/my-note.md.
3. open-forge index
4. Compare the complete note bytes and the generated navigation.

### Expected result

**Reviewed target:** Index both partial-metadata notes using available authored values and their actual source identity. Warn only about the missing optional fields. Preserve all existing YAML, unrelated fields and body bytes; do not insert fields or normalize values.

### What the person sees

Explain the independently observed result, useful warning, or genuinely necessary stop. Avoid requiring the user to repair unrelated content. Exact copy remains to be reviewed against the current interface.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## X04

**Situation:** Preview indexing a plain note without metadata edits

**Disposition:** Improved and added. Preview should describe useful navigation work, not automatic metadata scaffolding.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Preview indexing a plain note without metadata edits

### Starting point

Use the X02 plain note, with no metadata or list updates applied yet.

Fixture: `ROUTES`.

### Steps

1. Use the plain note from X02 before indexing.
2. open-forge index --dry-run
3. Compare every file, directory, settings and recovery artifact.

### Expected result

**Reviewed target:** Preview the generated navigation for the plain note, explain missing optional metadata, and write nothing. Do not propose metadata insertion or imply that the authored note must be repaired before indexing.

### What the person sees

Explain the independently observed result, useful warning, or genuinely necessary stop. Avoid requiring the user to repair unrelated content. Exact copy remains to be reviewed against the current interface.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


## X05

**Situation:** Do not treat missing, partial, malformed and unreadable metadata as one problem

**Disposition:** Improved and added. The distinction is valuable, but metadata assistance and whole-operation blocking contradict the forgiving target.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Do not treat missing, partial, malformed and unreadable metadata as one problem

### Starting point

Use four isolated copies: no header, valid partial header, unclosed YAML header, and genuine account-level read denial. Keep the intended user operation identical.

Fixture: `ROUTES`.

### Steps

1. Run open-forge index in each copy.
2. Run open-forge doctor in each resulting copy.
3. Compare cause, status, affected file and next action across the four cases.

### Expected result

**Reviewed target:** Missing or partial optional metadata yields usable indexing with a warning and no authored-file edits. Malformed syntax and genuine read denial have distinct causes and limit only work that needs those facts. Continue independent complete checks/lists, preserve affected authored files, and never invent a failed Framework update.

### What the person sees

Explain the independently observed result, useful warning, or genuinely necessary stop. Avoid requiring the user to repair unrelated content. Exact copy remains to be reviewed against the current interface.

### Verification

Observe whether the reviewed target is achieved. Compare affected paths, relevant ownership and source bytes independently of the report. Preserve content outside the requested change. Record current disagreement as a failure or unresolved contract difference, not a new expectation.


**Execution constraint:** Prove any denied read genuinely fails under the invoking account; missing or malformed metadata is not an I/O failure.

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

**Situation:** Judge partial repair within the actual repair scope

**Disposition:** Added. Worth retaining as an independently checked user outcome: Unrelated problems do not automatically invalidate a completed selected repair.

**Who:** A maintainer completing the user task, not a developer invoking internal CLI methods.

**Goal:** Judge partial repair within the actual repair scope

### Starting point

Fork two cases: an unrelated Library drift finding plus one exact repairable link; and a malformed required route/navigation boundary plus that same link.

Fixture: `LINKS`.

### Steps

1. open-forge doctor
2. open-forge repair --automatic --dry-run
3. open-forge repair --automatic
4. open-forge doctor

### Expected result

Unrelated problems do not automatically invalidate a completed selected repair. A required unsafe route boundary still prevents the operation. The final diagnosis must show the remaining real problems without claiming repair promised to fix everything.

### What the person sees

Report the selected repair effects and remaining independent findings. Do not imply that a bounded repair guarantees a globally clean workspace.





### Verification

- Compare repaired spans and untouched unrelated defects.
- Verify the blocked counterpart performs no edit.
- Keep outcome, diagnostic severity and residual issue set separate.

Record actual outcome, state, and communication separately from this specification.

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
