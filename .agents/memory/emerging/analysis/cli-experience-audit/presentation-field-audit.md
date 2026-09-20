---
open-forge:
  description: First-pass field audit of the presentation layer and the end-to-end suite, with measured output sizes and eight functional bugs
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Presentation, Testing, Audit]
---

# Presentation Field Audit

## Conclusion

The expanded view is a debug view, and it is the default. Compact is not a
summary — it is the same view with a few fields removed, and on `doctor` it is
longer than expanded `status`.

Four of the findings below are not presentation. They are functional bugs a
normal user hits on day one, and the end-to-end suite structurally cannot catch
them: 87 of its 91 runs go through `--json`.

Measured against `0.0.0-dev.sha-62b0e23e`, win-x64, ~60 invocations across an
empty non-workspace, a fresh install, a hand-dirtied workspace, a byte-identical
copy at a new path, and a nested subdirectory.

## Measured cost of a healthy workspace

| Command             | Expanded (default)   | Compact                  | Useful content          |
| ------------------- | -------------------- | ------------------------ | ----------------------- |
| `status`            | 104 lines · 4,238 B  | 36 lines · 962 B         | "everything is current" |
| `doctor`            | 194 lines · 6,491 B  | **114 lines · 4,207 B**  | "no problems"           |
| `route list`        | 99 lines · 5,018 B   | 31 lines · 2,411 B       | 13 routes               |
| `context`           | 845 lines · 37,925 B | **675 lines · 35,197 B** | 19 documents            |
| `references memory` | 34 lines · 2,428 B   | 10 lines · 408 B         | 2 lines, and wrong      |
| `status --json`     | 25,203 B             | 15,193 B                 | ~40 facts               |
| `doctor --json`     | —                    | 20,535 B                 | "no problems"           |

`doctor --view compact` is longer than `status --view expanded`. On `context`,
compact saves 7%.

The tell that expanded is a debug view is that it prints _work performed_, not
_results_: `references` lists all 24 files it opened before the answer; `find`
echoes the query back as a `Search details:` block; `doctor` reports every
healthy link as a finding; `install` prints all 44 planned effects before
telling you it refused to run.

## Functional bugs

### 1 — The workspace's absolute path is baked into the committed lifecycle record

`.agents/open-forge.lifecycle.json` stores
`"workspacePath":"D:\\...\\clean"`. A byte-identical copy at another path —
a clone, a rename, a second machine — hard-blocks every lifecycle command.

```
$ open-forge status --view compact          # untouched copy at a new path
Open Forge is installed.
Status: blocked                                                     exit 5
Framework
  Installation record: blocked
  Source: unavailable
```

Compact view never says why. `doctor` says _"The lifecycle workspace binding
does not match the selected workspace"_, twice, and offers no repair. `update`
says `Next: open-forge doctor`. `repair --automatic` fixes nothing. **No
command recovers a moved workspace.** This is the first thing that happens to a
public user: they clone the repository.

### 2 — `repair --automatic` reports success and "verified" while repairing nothing

```
$ open-forge repair --automatic                                     exit 0
Status: complete
Repaired: 0; new findings: 0; verified effects: 0
Verification: targets verified; bytes verified; post-conditions verified

$ open-forge status --view compact                                  exit 5
Status: blocked
```

Nineteen lines, every one a zero or a `not-requested`, headlined **complete**
and closed with **verified**. On a workspace where `doctor` reports 12
warnings, `repair` prints `Remaining: 0; manual 0; guided 0; blocked 0`.

### 3 — `route init` poisons the Framework baseline

```
$ open-forge install --automatic
$ open-forge extension install development --automatic --dry-run    exit 0
$ open-forge route init memory/emerging/ideas/x
$ open-forge extension install development --automatic --dry-run    exit 5
BLOCKED: The lifecycle Framework target differs from its persisted baseline.
  Target: .agents/workflows/_workflows.md
```

`_workflows.md` is byte-identical to a pristine install — diffed and confirmed.
The error names a file that did not change, while the file that did
(`_ideas.md`, whose Entries `route init` legitimately rewrote) goes unmentioned.
`doctor` reports `Status: complete`, **0 errors, 0 warnings** on this workspace.

### 4 — No upward workspace discovery

```
$ cd <workspace>/src/deep/nested
$ open-forge status
Open Forge is not installed.                                        exit 0
```

`git`, `npm`, `cargo` and `dotnet` all walk up to find their root. Open Forge
does not, and reports the miss as exit 0. Anyone whose editor terminal opens in
`src/` concludes the install failed.

### 5 — `context` exits 2 on a pristine install

The two files Open Forge itself installs trip its own warning, on the first run,
in the command the Loader tells every agent to call:

```
REQUIRES ATTENTION: The selected physical layer has no authored frontmatter.
  AGENTS.md
  .agents/loader.md
```

### 6 — `references` reports zero links for every source in a stock workspace

```
Incoming links: 0; coverage complete; status complete
  No direct links found.
Outgoing links: 0; coverage complete; status complete
```

`.agents/memory/_memory.md` contains four links; `doctor` finds them at
`:52:3` through `:55:3`. `references` only sees _authored_ links and silently
ignores generated Entries — which is the entire navigation graph — while
asserting `coverage complete`.

### 7 — One appended line of prose blocks the whole workspace

```
$ echo "A note I wanted to add." >> .agents/maps/_maps.md
$ open-forge status --view compact                                  exit 5
Open Forge is installed.
Status: blocked
  .agents/maps/_maps.md: unavailable
  .agents/maps/_maps.md: blocked
  .agents/maps/_maps.md: blocked
```

Because Entries must be the final section, one sentence invalidates the file —
and the blast radius is the whole installation record. The file is named three
times, twice on duplicate lines, and the cause is never stated.

### 8 — `doctor` does not report a deleted managed file

Deleting `.agents/patterns/_patterns.md` produces no "managed file is missing"
finding in 244 lines. It surfaces only as the second-to-last of 21 link results,
because `loader.md:105` happens to point at it.

## Output that is objectively wrong

| #   | Defect                                                                                          | Commands                              |
| --- | ----------------------------------------------------------------------------------------------- | ------------------------------------- |
| 9   | JSON escaping in human text: `<workspace>\\...` (correct elsewhere)                        | `route list`, `route inspect`, `find` |
| 10  | Literal `\n` and `\u003C` / `\u003E` escapes in the effects payload                             | `route init`                          |
| 11  | Byte `0xFA` (CP437, invalid UTF-8) used as a separator: `1 files ú 1.71 KiB`                    | `route inspect`                       |
| 12  | Every managed file listed twice, consecutively; 43 lifecycle targets for 21 payload files       | `status`                              |
| 13  | Same `Effects` field renders as escaped file bodies in one command, raw SHA-256 in the next     | `route init`, `route create`          |
| 14  | `Unchanged:` lists every untouched file in the workspace; unbounded                             | `route create`                        |
| 15  | `_ideas.md` appears in both `Effects` and `Unchanged`                                           | `route init`                          |
| 16  | `Description:` echoes empty even when `--description` was supplied                              | `route create`                        |
| 17  | `move <source-reference>` / `<destination-target>` wraps into the description column            | `route --help`                        |
| 18  | `Written value:23:118:` — no space, and a different line:col than the header's `23:3`           | `doctor`                              |
| 19  | `Body: available` and `[Frontmatter: missing]` injected into the document stream                | `context`                             |
| 20  | A file labelled `unavailable` whose body is then printed in full                                | `context --view compact`              |
| 21  | Double blank line then three label alignments in eleven lines                                   | `find` (no matches)                   |
| 22  | `1 files`, `1 sources`                                                                          | `route inspect`, `index`              |
| 23  | `characters` and `bytes` are the same number, printed as two facts                              | `status`                              |
| 24  | `--view` silently switches the JSON schema: expanded = v1 pretty-printed, compact = v2 minified | `--json`                              |
| 25  | Every scalar wrapped as `{"state":"available","value":19}` — 47 bytes to say 19, 35 times       | `--json`                              |
| 26  | 43 × 64-char `baselineFingerprint` plus `sourceAssetPath` duplicating `path` — ~10 KB           | `status --json`                       |
| 27  | `--verbose` documented everywhere, produces 0 bytes                                             | `status`, `doctor`, `library list`    |
| 28  | Help prints `[default: Expanded]`; `--view Expanded` is rejected                                | every command                         |
| 29  | Help claims _"The equals form is required for --depth"_; `--depth 2` works                      | `route list`                          |

`install --help` also promises _"Expanded JSON writes one schema-version-1
envelope"_ — describing one of the two schemas and never mentioning the other.

## Noise and friction

The full list is in [command-output-design.md](command-output-design.md), which
proposes replacements. The highest-value items:

- **`doctor` reports every healthy link as a finding.** On a healthy workspace:
  `0 errors, 0 warnings, 20 informational findings`, all "Link target is valid",
  five lines each. On a dirty workspace the 19 valid links print **before** the
  2 broken ones — severity order is inverted, and the thing you ran `doctor` to
  find is 218 lines down.
- **`status` in a non-workspace prints 59 lines, 41 saying `not-applicable`**,
  including 20 named paths for files in a directory that is not a workspace.
  `~not-applicable tokens` is a phrase that should not be constructible.
- **`install` prints all 44 planned effects, then refuses to run.** Every line
  reads `not-started`; the decision was already made.
- **`references` prints a 24-line trace before a 2-line answer.**
- **`doctor` prints the same two-line resolution tally six times**, once at the
  top and again in each of six domains, including domains with `Findings: none`.
- **Prose no user can act on**: _"Framework lifecycle absence is established by
  the complete operational proof"_ (meaning: not installed); _"The bounded
  local-reference candidate scan did not complete; no cardinality was
  inferred"_; _"Confirmed: The selected route roots and requested structural
  depth were confirmed"_ — a tautology printed on every successful `route list`.

## The end-to-end suite

91 tests, 7,564 lines. Rigorous about process isolation, write-freedom and exit
streams — and structurally unable to catch anything above.

**87 of 91 runs go through `--json`. Only 21 touch `--help`.** Eleven command
test files contain zero assertions on non-help human stdout: both Doctor suites
beyond one line, all five Library suites, Route Init, Extension Update,
EmbeddedPayload and ShellBoundary. The entire human output of `doctor` is
untested.

| #   | Gap                                                                                                                                      | Consequence                                                    |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------- |
| T1  | The "no false errors" test permits unlimited informational findings: `Assert.DoesNotContain(... severity is "warning" or "error")`       | The 20 "Link target is valid" findings are inside the contract |
| T2  | Tests assert the unreadable copy verbatim, e.g. _"Typed Extension bridge-registration role and observed-state authority is unavailable"_ | Rewriting it into English is now a test failure                |
| T3  | No test moves or copies an installed workspace — the "Relocated…" tests relocate the **executable**                                      | Bug 1 shipped                                                  |
| T4  | `RunAsync` always passes `workspace.Path` as the working directory                                                                       | Bug 4 shipped                                                  |
| T5  | Every test exercises one command family in isolation                                                                                     | Bug 3 shipped                                                  |
| T6  | No size budget anywhere except two `Assert.InRange(diagnostic.Length, 1, 4096)`                                                          | `doctor --view compact` at 114 lines is unconstrained          |
| T7  | `PublishedStatusProcessTests` asserts `schemaVersion == 1`; nothing covers the v2 compact contract                                       | The dual schema is untested                                    |
| T8  | Nothing guards against escaping or encoding defects in human output                                                                      | Findings 9–11 shipped                                          |
| T9  | `references` fixtures are all hand-built workspaces with authored links; none is a stock install                                         | Bug 6 shipped                                                  |

### Proposed tests

- One shared assertion over every human stdout in the suite: valid UTF-8, no
  `\uXXXX`, no `\\`, no literal `\n`. Catches four findings.
- Install to `A/`, copy to `B/`, run `status`. Four lines, catches bug 1.
- Run one command from a subdirectory. Catches bug 4.
- `install` → `route init` → `extension install`. Catches bug 3.
- A line budget per command per status, asserted. Catches the whole of §4.
- `doctor` and every mutating command must agree: if a command blocks, `doctor`
  has that finding.

## What is already right

- **`route inspect` is the model.** Plain-language section headings —
  _Where this source belongs_, _When it is read_, _Context size_ — each
  answering something a person actually wondered. Every other command should be
  restructured this way.
- `index` (8 lines) and `library list` (7 lines) are correctly sized; the house
  style can be terse.
- The root help's **Getting started** block is the best onboarding text in the
  tool.
- The seven-value semantic exit-code scheme is a good design, documented in the
  wrong place and applied inconsistently.
- `install` preserves pre-existing `AGENTS.md` content by appending into a
  managed region rather than overwriting.
- The write-freedom test harness — base64 snapshots of every file before and
  after each run — is better than most projects have. It is roughly half of each
  test file, which is the ratio problem, not a reason to remove it.
