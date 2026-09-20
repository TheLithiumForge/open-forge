---
open-forge:
  description: Task 30 phase 7-S slice 72 confirming captures normalize only environment-dependent representation and hide no real difference
  tags: [Memory, Working, CLI, Task, Subtask, Testing, Snapshots, Contextual, Active]
---

# 72 — Normalization audit

## Outcome

Confidence that the capture corpus proves what it appears to prove: that
normalization removes only what varies by machine, and never a difference the
contract promises to show.

## Depends on

Nothing.

## Why this is worth doing

Normalization is necessary and it is also the one mechanism that can silently
weaken 3,433 captures at once. Two things already observed make this concrete
rather than theoretical:

- The capture redaction that replaces machine-specific roots is **required** —
  without it, absolute temporary paths leak into snapshots and the corpus stops
  being reproducible. An attempt on 2026-09-17 to normalise path separators
  inside one message broke that redaction and had to be reverted.
- The comparison policy already sets `IgnoreLineEndings = true` deliberately,
  because the repository stores captures as LF through `.gitattributes`. That is
  a justified exception with a recorded reason — the audit's job is to confirm
  every other exception has one too.

## Actionable boundary

Audit what is normalized, from the code that does it, and classify each rule:

- **Environment-dependent representation** — machine paths, temporary roots,
  version and fingerprint values the contract marks unstable, line endings, and
  other named platform fields. These are legitimate.
- **Anything else** — counts, ordering, encoding defects, severity, finding
  codes, and diagnostic content the contract promises. **These must not be
  normalized**, and any rule that touches them is a finding.

Then check the redaction cannot over-match. A pattern that replaces a
machine-specific root is safe; a pattern broad enough to also replace a real
path difference is not. Demonstrate the distinction rather than asserting it —
the cheapest proof is a case where two genuinely different paths must stay
different after redaction.

- `IgnoreStringCase = false` and `IgnoreTrailingWhitespace = false` are current
  policy. Confirm they still hold and that nothing bypasses them.
- If a normalization rule is justified but undocumented, **record the reason
  where the rule lives**, not only here. The snapshot root override survived for
  months precisely because its reason was never written down.
- This slice changes no capture and no command output. If it finds a rule that
  should change, that is a finding to report, not a regeneration to perform.

## Acceptance

- Every normalization rule classified as environment-dependent or not, with the
  code location.
- Any rule that normalizes a contract-promised difference reported as a finding.
- Evidence that the redaction cannot collapse two genuinely different values.
- Justified rules documented where they live.
- No capture regenerated, no output changed, all four gates green and all counts
  unchanged.

## Audit record

### Measurement before starting

- Measured 2026-09-17 at `ae10520a` (`Specified the normalization audit slice`);
  the worktree was clean. The capture inventory was 3,433 `.txt` files under
  `src/cli/tests`: 1,705 JSON-named captures (`.json.`) and 1,728 text-named
  captures. `OPENFORGE_SNAPSHOT_UPDATE` was not set.
- The current tree still has one shared integration capture normalizer,
  `CommandOutputNormalization`, and one shared `SnapshotComparison` in
  `CommandOutputSnapshot`; both `MatchSnapshot` and `MatchDetailSnapshot` use
  that comparison. No other comparison override, snapshot configuration, or
  root-directory override was found. The 23 direct unit `AssertSnapshot`
  calls use the pinned Imprint defaults, which are also case-sensitive,
  trailing-whitespace-sensitive, array-order-sensitive, and have zero numeric
  tolerance; line endings alone are ignored by that library default.
- The recorded current state therefore held for the shared policy and corpus,
  but not for the old gate receipt: the full run is 3,206 unit tests and 2,224
  integration tests (2,207 passed, 17 skipped), rather than the older 3,191 /
  2,222 packet. The 3,206 unit count was already recorded by slice 54. The
  additional integration test is the committed Repair truthfulness journey in
  `191a0182`; slice 72 added no test.

### Rule classification

| Rule and location | Classification and evidence |
| --- | --- |
| `CommandOutputNormalization.Normalize` -> `ReplacePath` for the caller-supplied workspace, extension-source, and recovery-bundle roots; raw and `JsonEncodedText` forms are handled by `ReplaceDelimited`. | Legitimate machine-dependent representation. Matching is ordinal and delimiter-bounded; descendants are replaced, while an adjacent identity is not. The existing focused oracle proves both raw/JSON forms and the `source`/`source-other` distinction. |
| `NormalizeRecoveryPaths` assigns `<recovery-bundle-1>`, `<recovery-bundle-2>`, and so on, plus `<recovery-store>`. | Legitimate run-specific recovery locations. Separate placeholders preserve candidate identity and ordering; the focused oracle keeps `recovery-other` unchanged. |
| `NormalizeCleanupLeaseIdentity` replaces only the two observed JSON spellings of the generated `operationId`. | Legitimate run-specific identity. The existing oracle leaves candidate IDs, other operation IDs, case changes, and longer values exact. |
| The caller-supplied `CliBuildVersion.InformationalVersion` is replaced with `<version>` in `Normalize`. | Legitimate only for the contract-marked unstable build version. No current capture contains `<version>`; callers pass the build identity, not an arbitrary authored value. |
| `Normalize` converts CRLF and lone CR to LF. | Legitimate line-ending representation. The reason is recorded beside `IgnoreLineEndings = true` in `CommandOutputSnapshot`: checked-in captures are LF through `.gitattributes`, while content/case/trailing whitespace remain exact. |
| `CommandOutputSnapshot.OutputComparison` uses `IgnoreStringCase = false`, `IgnoreLineEndings = true`, and `IgnoreTrailingWhitespace = false`; no other override was found. | Legitimate and fully documented. Array order remains significant and Imprint's numeric tolerance remains zero. `JsonDetailComparison` and `CommandOutputDetailComparison` parse only for semantic cross-detail assertions; they require the fixed property order, exact counts/recovery/next/data, and change only the permitted presentation-detail coordinate in memory. |
| `CommandOutputSnapshotFormatting.PrettyPrintJson` parses valid JSON and writes indented UTF-8 with `NewLine = "\n"` and a final newline. | Legitimate storage formatting, not a contract scrub: valid JSON values, property order, and array order remain; malformed JSON/encoding cannot pass the parse. The LF reason is now recorded at the formatter. Valid escape spelling may be canonicalized, but no invalid encoding defect is hidden. |
| `CommandOutputRenderers` and `ReadCommandOutputCapture` pass `normalizeTextPaths: true` for text and debug diagnostics and `false` for JSON. `NormalizeTextPaths` then converts every remaining backslash except a few escape spellings. | **Finding F-01.** This is broader than machine-path representation. For example, promised text `diagnostic=literal\marker` becomes `diagnostic=literal/marker`; an invalid or authored backslash sequence is evidence, not a path. The current four absolute-path leaks below also show that separator rewriting does not safely replace an unobserved root. No change or regeneration was made. |
| `Sha256()` replaces any standalone 64-hex token anywhere in the normalized string. | **Finding F-02 (uncaptured).** A fingerprint field may be unstable, but this regex is not field- or contract-scoped; it can hide a promised diagnostic token, finding content, or another stable 64-hex value. Current captures contain `<sha256>` only where the existing corpus treats the value as a hash, and no test exercises a non-hash 64-hex token. Narrowing it is a maintainer decision; no code or capture change was made. |
| The two unit Extension Install snapshot helpers use direct unbounded `String.Replace` for the fixture workspace root; Doctor unit snapshots pre-replace CRLF before calling the shared helper. | The intent is documented in the two Install helpers and the CRLF operation is a legitimate line-ending rule, but the Install replacements lack `ReplaceDelimited`'s boundary proof. **Finding F-03 (uncaptured unit-harness risk):** a distinct fixture value whose text merely contains that root would also be scrubbed. The current deterministic fixtures show no collision; no test was added. |

### Required distinct-value proof

The focused existing test `OpenForge.Cli.Core.UnitTests.TestSupport.Snapshots.CommandOutputNormalizationTests`
passed 5/5. Its literal oracle supplies:

```text
before: C:\owned\source
        C:\owned\source-other\extension.json
after:  <extension-source>
        C:\owned\source-other\extension.json
```

It also supplies `C:\owned\recovery\11111111.zip` and
`C:\owned\recovery-other\33333333.zip`; only the observed bundle becomes
`<recovery-bundle-1>`, while `recovery-other` stays byte-for-byte distinct.
The reason is visible in `ReplaceDelimited`: after the candidate root, `-` is
not a permitted path delimiter. This demonstrates the distinction instead of
inferring it from the placeholder counts.

### Findings and stops

1. **F-01 — broad text separator normalization.** The rule can alter promised
   diagnostic content and must not be widened or regenerated around. The exact
   before/after proof is `literal\marker` -> `literal/marker` when the text path
   switch is enabled.
2. **F-02 — unscoped 64-hex normalization.** The code cannot establish that
   every matching token is a contract-marked fingerprint. A future fix needs a
   maintainer ruling because it can change captured values; no finding code or
   wording was invented.
3. **F-03 — unbounded unit-only root replacement.** No captured collision was
   observed, but the rule has no boundary guard. It remains a recorded
   uncaptured risk rather than an assumed pass.
4. **F-04 — four committed debug captures leak an absolute temporary prefix.**
   The current literal evidence is:

   ```text
   ExtensionCreateBeforeOutputSnapshotTests/PartialWriteFailure/write-failed-partial.debug.diagnostics.txt:9
   finding=extension-create.application-failed:subject=<extension-source>/toolkit/content/.agents/:cause=Access to the path 'C:/Users/<user>/AppData/Local/Temp/ope...

   ExtensionCreateBeforeOutputSnapshotTests/PartialWriteFailure/write-failed-partial.json.debug.diagnostics.txt:9
   finding=extension-create.application-failed:subject=<extension-source>\toolkit\content\.agents\:cause=Access to the path 'C:\Users\<user>\AppData\Local\Temp\ope...

   ExtensionCreateBeforeOutputSnapshotTests/CatalogueUnreadable/catalogue-unreadable.debug.diagnostics.txt:9
   finding=extension-create.catalogue-unavailable:subject=<extension-source>/toolkit:cause=Access to the path 'C:/Users/<user>/AppData/Local/Temp/open-forge-extension-cr...

   ExtensionCreateBeforeOutputSnapshotTests/CatalogueUnreadable/catalogue-unreadable.json.debug.diagnostics.txt:9
   finding=extension-create.catalogue-unavailable:subject=<extension-source>\toolkit:cause=Access to the path 'C:\Users\<user>\AppData\Local\Temp\open-forge-extension-cr...
   ```

   The callers provide the catalogue root (or its `packages` child) to the
   redactor, but the bounded exception cause contains a different temporary
   target. There is no safe after-state in this slice: adding a guessed
   placeholder or regenerating these captures would change evidence. The
   result/capture seam needs a maintainer decision about carrying or bounding
   that extra path. This is a leakage/coverage finding, not proof that the
   required exact root redaction should be removed.

No situation moved, so there are no literal command-output before/after pairs
to report as changes. The four captures above are findings left unchanged; no
capture was regenerated and no command output was changed.

### Gate receipt

| Gate | Result |
| --- | --- |
| Release build | Passed with 0 warnings and 0 errors using the task-local NuGet home, locked restore, `UseSharedCompilation=false`, and a serial artifact build after the prescribed artifact path hit the sandbox's access-denied/parallel-output issue. |
| Focused normalization unit class | 5 total, 5 passed, 0 failed, 0 skipped. |
| Full unit runner | 3,206 total, 3,206 passed, 0 failed, 0 skipped. Change from the user packet: +15 is pre-existing; change from the slice-start measurement: 0. |
| Full integration runner | 2,224 total, 2,207 passed, 0 failed, 17 skipped. Change from the user packet: +2 is pre-existing; change from slice start: 0. |
| Whitespace verification | The expected nonzero result reported exactly the 5 pre-existing diagnostics in `ReferencesOperation.cs` and `ExtensionListApplicationIntegrationTests.cs`; no touched file was among them. |

The source comments added during this audit are documentation only. They do
not change tests, captures, comparison values, command strings, finding codes,
severity, ordering, counts, or exit behavior.

## Changes ledger

- audit record: empty slice ledger -> recorded the measured corpus, every
  capture normalization/comparison rule, the distinct-path proof, four findings,
  gate counts, and the no-regeneration boundary above.
- documentation: undocumented legitimate exact-path, line-ending, recovery
  identity, cleanup-lease, and JSON storage-format reasons -> comments at the
  rules' implementation sites in `CommandOutputNormalization` and
  `CommandOutputSnapshotFormatting`; runtime behavior is unchanged.

## Divergences observed

- state: the slice says the 2026-09-17 separator attempt was reverted -> the
  current tree still contains the active `normalizeTextPaths` switch and four
  committed absolute-path leaks. Reconcile this record before treating F-01 as
  closed; no regeneration was performed here.
- F-04 closed in `8c5b5d96`. The cause was not the caller passing the wrong
  root: `CliDiagnosticRenderer` clamps each debug value at 240 characters, which
  cut the path so the whole-path match could not see the remainder.
  `CommandOutputNormalization` now matches the longest prefix the clamp left, so
  an attributable fragment becomes its own placeholder, collapses the temporary
  root when the clamp landed too early to attribute, and throws rather than
  committing a fragment neither rule reaches. The four captures were regenerated
  and carry no absolute path.
- F-01 closed. The broad pass that rewrote every remaining backslash in a
  capture is gone, so `literal\marker` stays as the command printed it. Only the
  path following a placeholder is rewritten now, and `<temp>` joined that list
  because it was the one placeholder the broad pass had been covering. Removing
  the pass moved exactly one capture, which the added placeholder puts back, so
  the whole corpus regenerates unchanged. `CommandOutputNormalizationTests`
  proves a backslash outside a redacted path survives.
- F-02 and F-03 closed. The fingerprint redaction now recognises a value by the
  member or label carrying it rather than by its shape, so a 64-character hex id
  a command prints in any other position survives as evidence. The anchors were
  read from the corpus and proved complete the only way that counts: every
  capture regenerates byte for byte unchanged. The two Extension Install unit
  helpers now call the shared delimited replacement, so one definition of where
  a root ends covers every capture, and a sibling such as `<root>-other` can no
  longer be consumed. Both boundaries are asserted in
  `CommandOutputNormalizationTests`.
- coverage: the existing normalization tests prove adjacent-path preservation
  but do not exercise arbitrary text backslashes, a non-hash 64-hex token, or
  the two unbounded unit redactors. These are recorded as uncaptured findings,
  not assumed passes, because this slice forbids test changes.
- gate baseline: slice 54 recorded 3,206 / 2,223 after its measurement, while
  this tree reports 3,206 / 2,224; the one-test difference is the later
  committed `DiagnosisOwnershipIntegrationTests` change in `191a0182`, not
  this slice. The user packet's 3,191 / 2,222 is older still.
- environment: the prescribed build initially could not read the user NuGet
  config and then hit artifact-directory access/parallel-output failures. A
  task-local `NUGET_CLI_HOME`, explicit locked restore, serial build, and
  isolated artifact path produced the passing receipt; the source worktree
  remained clean apart from the documented comments and this slice record.
