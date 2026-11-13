---
open-forge:
  description: Define strict definition validation, release admission and paired workspace record checks
  tags: [Memory, Document, CLI, Contract, Shared, Tool, Manifest, Behavior, CurrentTruth]
---

# Tool manifest behavior

Tool manifest resolution is a read capability that validates exact source bytes before selecting a reviewed pin.

Format validity, release support and native testing are separate checks. Passing one doesn't give the others.

Status: V1 core contract decided.

## Resolution

The capability follows these steps:

1. Resolve a supplied source path from the process directory or select only the embedded catalogue.
2. Read exact ordinary source bytes without following catalogue-path links.
3. Decode JSON or YAML through the standard format parser and reject duplicate keys, unknown fields and forbidden YAML constructs.
4. Validate catalogue identities, nonempty version lists and the one default pin.
5. Resolve contained catalogue paths and hash exact manifest bytes before decoding them.
6. Validate all required manifest fields, tokens, paths, artifact references and target closure.
7. Match the requested ID and exact pin with the source's manifest identity.
8. Compare the exact tuple with the compiled release allow-list, then check adapter revision and target tests for activation.
9. Return typed definitions, requirements and coverage without network or filesystem effects.

## Workspace comparison

Read installed entries from settings independently of catalogue resolution. Compare each with lock receipts by exact ID, pin and manifest identity. Keep configured roles separate from verified install facts. Reject duplicate active-role assignments. A partial paired write is incomplete configuration and permits no new changes.

For updates, publish the verified replacement reference before releasing the prior reference. For removal, use stored snapshots and receipts. An unavailable reference set blocks final shared deletion. Never treat an unreadable store as empty.

## Conformance

Required cases are:

- JSON and YAML parity, duplicate keys, unknown fields, aliases and tags.
- Missing required values, invalid finite tokens and every native target.
- Exact SemVer strings, absent defaults and duplicate IDs, versions or paths.
- Digest and identity mismatches, unreadable bytes and escaping catalogue paths.
- Candidate, reserved, unreviewed and unavailable-adapter definitions.
- V1 graph guidance rejects a null, optional or disabled Skill.
- No source merging, directory scanning, network fallback or implicit pin upgrade.
- Omitted settings map, inactive roles and duplicate role rejection.
- Unknown unrelated settings keys kept through paired writes.
- Missing receipts and interrupted paired writes without silent host changes.
- Test-only synthetic definitions isolated from the production allow-list.
- Official upstream sources, pinned signature and attestation evidence and a complete frozen npm lock.
- Private Node paths, no ambient runtime fallback and unsupported musl targets.
- Default-off telemetry with a visible opt-in and disabled self-updates.
- Same-pin reinstallation verifies and repairs bytes and reruns the installer.
- Exact reviewed downgrades and tool and Node reference retention until the last workspace releases them.

## Entry rewrites

After the declared installer completes, require each selected entry's exact original before changing its command parts. Resolve only declared selectors in the selected host and workspace file. Reject duplicate JSON keys, ambiguous pointers, duplicate named TOML tables, missing markers or repeated matching lines. A TOML rewrite accepts only its expected command and argument lines in the named table and edits them as text. It parses no TOML.

Remove a declined optional component only when its complete declared entry matches. For a Claude hook event, locate the exact command across matcher groups and their hook arrays. Reject repeated matches. Edit or remove only that command entry. Keep other hooks and every pre-existing group. An installer-created group can be removed when it becomes empty and its creation receipt still matches. Without creation evidence, keep the group. Skip a declined component's rewrites. Check exclusions and destination permission before the child command or Open Forge edit. Recheck whole-file bytes before writing. Verify rewritten values or declined-entry absence after writing and record those expectations in the lock.

Resolve declared platform overrides from the host's exact target before checking either a hook rewrite or a declined entry. Keep any required uninstaller substring in the replacement. The [hook records](../../../technical-designs/tool-lane-contracts.md#definition-records) define platform selection and group-creation evidence.

Status and Doctor compare current command parts with those recorded expectations and report drift without changing them. A failed or unexpected shape keeps completed installer evidence and unfinished work. It never triggers an independent replacement host registration.

## Shared conformance

The pending implementations need these additional cases:

- Both origins, unavailable stored origin and deterministic grouping.
- All four rewrite forms, unrelated bytes kept and exact post-write validation.
- Unexpected originals, duplicate selectors, changed files and no guessed rewrite.
- Prompt-hook yes, no, saved choice and missing non-interactive input.
- Decline removes one exact hook while keeping sibling hooks.
- Conditional role lines added, switched and cleared without a help pointer.
- Bare Install restores exact declared pins and never substitutes catalogue defaults.
- Missing declarations, unavailable lock evidence and declared-but-missing next steps.
- Run accepts only declared verified IDs and refuses changed cheap file facts.
- A second synthetic provider covers Choose switching and clear without deleting either index.

The [interface](interface.md) defines fields and caller outcomes.
