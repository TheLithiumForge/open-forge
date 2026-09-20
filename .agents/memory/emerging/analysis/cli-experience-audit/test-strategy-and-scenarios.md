---
open-forge:
  description: Assessment of the three existing test layers and a proposed transcript-driven scenarios layer with a self-policing command and status coverage matrix
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Testing, Scenarios, Coverage]
---

# Test Strategy And Scenarios

## Conclusion

There are 2,811 tests across three layers and 133,000 lines of test code. The
coverage is deep and it is aimed almost entirely at the wrong altitude for the
problems in this audit.

**Every layer asserts fragments. No layer asserts the composed output of a
command.** That is the single structural reason every finding in this scope
survived, and it is what a scenarios layer should fix.

## Current state

| Layer       | Files | Lines  | Tests | What it proves                                                   |
| ----------- | ----- | ------ | ----- | ---------------------------------------------------------------- |
| Unit        | 365   | 67,117 | 1,646 | Each renderer fragment, each enum mapping, each planner decision |
| Integration | 313   | 57,738 | 1,059 | Command operations against a real filesystem                     |
| End-to-end  | 58    | 8,686  | 106   | The published binary's exit codes, streams, and JSON shape       |

By area, unit and integration both concentrate on `Route` (82 and 79 files),
`Library` (36 each) and `Extension` (33 and 32). `Install`, `Context` and
`Repair` have 7 integration files each.

### What is genuinely good

- **The write-freedom harness.** Every read-only e2e run base64-snapshots the
  whole tree before and after and asserts equality. Most projects have nothing
  like it.
- **Closed vocabulary tests.** `DoctorHumanVocabularyTests.HumanMappingsAreClosed`
  walks every enum value through every renderer and asserts an undefined value
  throws. That is real rigor and prevents a whole class of silent gaps.
- **Unavailability is modelled, not faked.** `UnknownCountsDoNotBecomeNoFindings`
  proves an unknown count never renders as `Findings: none`. The distinction
  between zero and unknown is respected at the fragment level.
- Integration coverage of `Framework/Recovery` (19 files) and `Framework/Mutation`
  (11) is substantive.

### The structural gap

There are 100 rendering test files in the unit layer and **211 assertions on
human text across all three layers combined** — for a CLI with 20 commands and
7 semantic statuses, i.e. ~140 command/status pairs.

Snapshot testing exists but covers one finding group, not a command:

```csharp
// DoctorHumanGroupingTests.cs:25
Assert.Equal(expanded ? DoctorHumanSnapshots.Expanded : DoctorHumanSnapshots.Compact, text);
```

`DoctorHumanSnapshots` is 30 lines describing two warnings on one link. Nothing
asserts what `doctor` prints for a whole workspace — which is how a 194-line
output for "no problems" became normal.

That snapshot also **enshrines a defect**. It contains:

```
  WARNING  Broken link [reference.target-missing]
    The linked file was not found.
    Resolution: choose a target after reviewing the evidence
  WARNING  One possible target remains unselected [reference.candidates-one]
    ...
    Observed: missing
    Read from: local links
```

`Observed:` and `Read from:` sit at the end of the group, after both warnings,
attached to neither. The detachment bug is the expected value.

### Why each audit finding survived

| Finding                                      | Why no layer caught it                                                  |
| -------------------------------------------- | ----------------------------------------------------------------------- |
| 194-line `doctor` on a healthy workspace     | No layer asserts a whole command's output or its size                   |
| 20 × "Link target is valid"                  | The e2e "no false errors" test permits unlimited informational findings |
| Absolute `workspacePath` blocks a clone      | No test moves or copies an installed workspace                          |
| No upward discovery                          | `RunAsync` always passes the workspace root as cwd                      |
| `route init` poisons the baseline            | Every test exercises one command family in isolation                    |
| `SKILL.md` strict keys                       | No fixture is a real third-party skill                                  |
| `\\`, `\n`, `<`, `0xFA` in human text        | No layer inspects output as bytes or for escaping                       |
| `--view` swaps the JSON schema               | Only schemaVersion 1 is asserted                                        |
| `repair` reports success after doing nothing | Nothing cross-checks `repair` against `doctor`                          |

Seven of nine are **absent scenarios**, not absent assertions. More unit tests
would not have found any of them.

## Proposed: a scenarios layer

A fourth layer at `src/cli/tests/scenarios/`, deliberately shaped so that
someone who did not write the CLI can read, review, and author it.

### Shape

Each scenario is **one Markdown file containing a transcript**. The runner
replays the commands against a seeded workspace and diffs actual output against
what is written. The file is simultaneously the test, the golden output, and the
documentation.

```markdown
---
seed: preexisting-skill
---

# Adopting a workspace that already has a skill

## $ open-forge install --automatic
```

exit 0
Installed the Open Forge Framework.

21 files, 20 directories in .agents
AGENTS.md, CLAUDE.md Open Forge section added; your content kept

1 existing file under .agents is not routed and was left alone:
.agents/skills/pdf/SKILL.md unsupported frontmatter key 'license'

```

## $ open-forge doctor

```

exit 2
1 problem.

.agents/skills/pdf/SKILL.md unsupported frontmatter key 'license'
remove the key, or see: open-forge help skills

```

## files

```

.agents/skills/pdf/SKILL.md unchanged
.agents/open-forge.lifecycle.json created

```

```

Why this shape rather than more xUnit:

- **The whole output is the assertion.** Every noise line, every stray `\n`,
  every wrong count is a diff. This is the property no current layer has.
- **It is reviewable by eye.** The user's stated need — a layer they know
  intimately — is met by a format that reads like a session.
- **It doubles as documentation.** These transcripts are what the README and
  `--help` examples should show, kept honest by CI.
- **Regeneration is cheap.** `--update` rewrites the goldens; the reviewable
  artifact is then the _diff_, which is exactly what a presentation change should
  be reviewed as.

### Normalization

Golden transcripts are brittle unless volatile values are normalized before
comparison. Normalize: absolute paths → `<workspace>`, the version string,
fingerprints → `<sha>`, byte and token counts in prose → `<n>` unless the
scenario is specifically asserting them, timestamps, and line endings.
Keep counts real in the scenarios that exist to assert size budgets.

### Seeds

Each seed is a fixture builder producing a workspace. The set must include every
state that produced a finding in this audit.

| Seed                    | Contents                                                                  |
| ----------------------- | ------------------------------------------------------------------------- |
| `empty`                 | bare directory, not a workspace                                           |
| `not-a-repo`            | files but no `.git`, to prove no git coupling                             |
| `fresh`                 | after `install --automatic`                                               |
| `preexisting-plain`     | `.agents/notes.md`, a loose user file                                     |
| `preexisting-skill`     | native `SKILL.md` with `license`, plus a `references/` folder             |
| `preexisting-agents-md` | a user-authored `AGENTS.md` to prove preservation                         |
| `legacy`                | root `open-forge.extensions.json`, no `.agents/open-forge.lifecycle.json` |
| `relocated`             | `fresh` installed at path A, copied to path B                             |
| `nested`                | `fresh`, with the command run from `<ws>/src/deep/nested`                 |
| `dirty-managed`         | a hand-edited managed file, prose appended after Entries                  |
| `deleted-managed`       | `_patterns.md` removed                                                    |
| `broken-links`          | two dead local links                                                      |
| `malformed-yaml`        | unquoted `": "` in a description, a tag containing a space                |
| `extensions-installed`  | after `extension install development`                                     |
| `library-attached`      | a registered Library with live links                                      |
| `large`                 | 300 generated routes, for budget and timing assertions                    |
| `unicode-paths`         | non-ASCII directory and file names                                        |
| `readonly`              | a read-only file in the tree                                              |

### Journeys

Each journey is a scenario file walking a realistic sequence. Together they must
touch every command.

- **J1 first run** — `empty` → install --dry-run → install → status → context → route list → find → doctor
- **J2 adopt existing** — `preexisting-skill` → install → doctor → repair → index → status
- **J3 author routes** — `fresh` → route init → route create → route inspect → route update → route move → route remove → index → references
- **J4 extensions** — `fresh` → extension list → extension install → extension inspect → extension update → extension remove → doctor
- **J5 libraries** — `fresh` → library attach → library list → library inspect → library sync → library detach
- **J6 recover** — `dirty-managed` + `broken-links` → doctor → repair → cleanup → doctor → status
- **J7 relocate** — `relocated` → status → doctor → repair → update
- **J8 legacy** — `legacy` → extension install → install → doctor → status
- **J9 update** — `fresh` → hand-edit a managed file → update → update --force → status
- **J10 wrong input** — bad command, bad flag, bad source ID, missing required option, `--view` with a bad value
- **J11 nested and explicit** — `nested` → status; then the same from outside with `--workspace`
- **J12 budget** — `large` → status → context → assert startup token attribution and size bounds

### The coverage matrix

The mechanism that makes this layer self-policing, and the reason to build it as
a folder rather than more test methods.

A meta-test enumerates every `(command, semantic status)` pair the CLI can
produce — derived from the same command definitions the CLI itself uses, so it
cannot drift — and asserts each pair appears in at least one scenario
transcript. A new command, or a new status a command can return, fails the build
until a scenario covers it.

```
Coverage: 118 of 140 command/status pairs

  uncovered:
    library sync      blocked
    library sync      incomplete
    extension create  interrupted
    ...
```

This turns "did we test everything" from a judgment call into a report. It is
also the only practical way to keep 20 commands × 7 statuses honest.

### Assertions beyond the transcript

Three checks run on every scenario step, independent of the golden text.

- **Encoding.** Output is valid UTF-8; contains no `\uXXXX`, no `\\` outside a
  code fence, no literal `\n`. Catches four Phase 3 findings at once.
- **Size budget.** A per-command, per-status line ceiling declared once in the
  scenarios folder. Exceeding it fails. This is the only defence against the
  output growing back.
- **Diagnosis agreement.** After any step that returns `blocked` or `incomplete`,
  the runner also invokes `doctor --json` and asserts a finding exists with the
  same code and subject. This encodes the Phase 1 invariant as a property of
  every scenario rather than a single test.

### Relationship to the existing layers

Nothing is removed. The scenarios layer is additive and takes over one job the
others should stop attempting.

- **Unit** keeps fragment rendering, vocabulary closure, and planner decisions.
  It should stop growing snapshot constants of composed output; those move to
  scenarios.
- **Integration** keeps operations against a real filesystem.
- **E2E** keeps process-level concerns that scenarios cannot cover: cancellation,
  signal handling, stream separation, the relocated executable, and the
  write-freedom harness. Its 87-of-91 `--json` bias becomes correct once
  scenarios own human output — e2e should assert the _contract_, scenarios the
  _experience_.

## Sizing

The runner is small: seed builders, a Markdown transcript parser, a normalizer,
a diff, and the coverage meta-test. The volume is in authoring seeds and
journeys, which is spec-driven and parallelizable — and which is exactly the
work the user asked to be able to read and own.

Build order: runner and two seeds (`empty`, `fresh`) with J1, to prove the
format; then the coverage matrix, which will immediately report how much is
missing; then the remaining seeds and journeys against that report.

## The tests describe the model, not the user

A random sample of unit test display names:

```
Context compact fixed rows preserve byte-exact separators, culture, ordering, and final newline
Cleanup stops after a changed candidate and preserves prior deletion and remaining residual facts
Unavailable lifecycle reads retain the admitted failure and its exact direct cause
Neutral source model enums retain their exact cross-topic vocabulary
References source catalogue issue mapping is exhaustive
Route-list catalogue finding policy maps every non-contained candidate state
```

Every one is a property of the internal model — _preserves_, _retains_, _maps
every_, _is exhaustive_. Not one describes a person doing something. There is no
test called "a user whose repository already contains a skill runs install", or
"a broken link is repaired", or "someone types the wrong package name".

This is the same pattern as the 65 `*Human*` types and the 25,477 lines of
presentation code that produce an 8.8 MB `doctor`: **the system is thoroughly
optimised for internal fidelity and barely at all for external outcome.** The
tests prove the model is self-consistent. Nothing proves the tool is usable.

That is also why the effort is distributed the way it is. `Framework/Recovery` is
6,020 lines and `Shell/Interaction` is 50; the test suite mirrors it, with
19 integration files for `Framework/Recovery` and none for whether a command's
output can be read.

### Proposed result

- Scenario names describe a person and an outcome: `install-into-existing-skill-workspace`,
  `repair-a-broken-link`, `wrong-package-name`. The file name is the test name.
- Every scenario is a **happy or unhappy flow with a named actor situation**, not a
  property. Properties stay in the unit layer where they belong and are already good.
- Judge the scenarios layer by whether someone who did not write the CLI can read a
  transcript and say "yes, that is what should happen". No existing test can be read
  that way.
