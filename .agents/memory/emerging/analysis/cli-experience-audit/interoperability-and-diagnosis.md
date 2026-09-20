---
open-forge:
  description: Why a standard SKILL.md package blocks the workspace, traced to the exact source rule, and why every diagnostic command reports a different cause
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Interoperability, Diagnosis, Skill, Routing]
---

# Interoperability And Diagnosis

## Conclusion

A workspace containing one ordinary Agent Skills package reaches a state where
`install`, `index`, and `extension install` all refuse to run, `repair` reports
success while changing nothing, and `doctor` reports either the wrong files or
no findings at all.

The underlying rule is one line of source. The experience failure is that no
command that blocks ever names the offending file or the offending key, while
the one command that does know the answer — `route list` — is never suggested.

The design intent is already correct: native `SKILL.md` is a first-class routed
source and needs no Open Forge metadata. The failure is that the system tells
users the opposite, and following its advice destroys a working configuration.

## The rule

`SourceAuthoredMetadataParser.TryReadSkillValues` accepts exactly two
frontmatter keys and rejects the document on any third one.

```csharp
// src/cli/core/OpenForge.Cli.Core/Framework/Sources/Metadata/SourceAuthoredMetadataParser.cs:120
switch (key)
{
    case "name" when name is null:        name = value;        break;
    case "description" when description is null: description = value; break;
    default:
        return false;   // -> SourceAuthoredMetadataState.Malformed
}
```

`Malformed` propagates to `index.metadata-unsafe` and
`install.generated-region-unsafe`, both of which are **blocked**, exit 5.

The published Agent Skills frontmatter carries more than `name` and
`description`. `license`, `allowed-tools`, and `metadata` are all normal. Every
skill that has ever been shared publicly therefore hard-blocks the workspace.

### Measured

Same workspace, same skill, only the frontmatter changed.

| `SKILL.md` frontmatter                          | `index`           | Reported cause                                  |
| ----------------------------------------------- | ----------------- | ----------------------------------------------- |
| `name` + `description`                          | **exit 0**        | —                                               |
| `name` + `description` + `license`              | exit 5 blocked    | `frontmatter or metadata shape is malformed`    |
| `name` + `description` + `allowed-tools`        | exit 5 blocked    | `frontmatter or metadata shape is malformed`    |
| none at all                                     | exit 3 incomplete | `Required authored metadata ... is unavailable` |
| `open-forge:` block with `description` + `tags` | exit 5 blocked    | `cannot be safely projected`                    |
| `open-forge:` block with `description`, no tags | exit 5 blocked    | `frontmatter ... is malformed`                  |

Two things to read off that table.

**Adding the metadata the CLI asks for makes the failure strictly worse.** A
skill with no frontmatter is `incomplete` (exit 3). Add the `open-forge:` block
the error message points you toward and it becomes `blocked` (exit 5). There is
no message anywhere explaining that `SKILL.md` uses its own native frontmatter
and must not carry an `open-forge:` block.

**The message names the wrong thing.** `license` is a valid, well-formed YAML
key. Nothing about the frontmatter's _shape_ is malformed. The document is
well-formed and the parser simply does not accept that key.

### A second rule with the same shape

`RouteSource.cs:93` requires a Skill to have **zero** tags while requiring every
other routed source to have **at least one**:

```csharp
if (metadata.State == RouteSourceMetadataState.Complete
    && (form == SourceDocumentForm.Loader
        || form == SourceDocumentForm.Skill && metadata.Tags.Count != 0
        || form != SourceDocumentForm.Skill && metadata.Tags.Count == 0))
{
    throw new ArgumentException("The complete metadata values do not match the source form.", ...);
}
```

Two opposite requirements behind one message: _"Required route metadata is
missing."_

### Proposed result

- Accept and ignore unknown keys in `SKILL.md` frontmatter. `name` and
  `description` stay required; everything else passes through.
- When a key genuinely cannot be accepted, name it:
  `.agents/skills/pdf/SKILL.md: frontmatter key 'license' is not supported here.`
- Never let a single unreadable leaf escalate to `blocked`. An unreadable source
  is `attention` on that source; it should not stop `index` from updating the
  other nineteen regions or stop `install` from running at all.
- Treat "cannot be classified" as **skip with a note**, not as a fatal condition.
  A user's `.agents` folder will always contain files Open Forge did not author.

## The circular dependency

Installing into a workspace that already has `.agents/skills/pdf-processing/`
fails before anything is written:

```
$ open-forge install --automatic
Status: blocked                                                    exit 5
BLOCKED: Every direct routed child requires complete authored source
         metadata. [install.generated-region-unsafe]
Next: open-forge doctor
```

`doctor` then reports, among 12 warnings:

```
Route: .agents/skills/pdf-processing/SKILL.md
WARNING  Route cannot be reached [route.unreachable]
WARNING  Source is outside the loaded routes [route.detached]
```

Both are true and neither is actionable, because the route is unreachable and
detached _for the same reason install is blocked_: `.agents/loader.md` does not
exist yet. Install refuses to run until the routes validate; the routes cannot
validate until install has created the Loader.

`--force` does not break the loop. Neither does `repair`.

### Proposed result

Install must validate only what it is about to write plus its own collision
targets. Pre-existing user content under `.agents/` is not install's business
and must not gate it. Report it afterwards as an advisory:

```
Installed the Framework (21 files).

3 existing files under .agents are not routed:
  .agents/skills/pdf-processing/SKILL.md            unsupported frontmatter key 'license'
  .agents/skills/pdf-processing/references/*.md     no frontmatter

They are left untouched and Open Forge ignores them.
Run `open-forge route adopt .agents/skills` to bring them in.
```

## The blocking commands name the wrong file

Install a clean workspace first, then drop the same skill in. Now `index` blocks:

```
$ open-forge index
Generated navigation update is blocked.                            exit 5
BLOCKED: Authored metadata for one direct routed child cannot be safely
         projected. [index.metadata-unsafe]
  .agents/skills/_skills.md
```

The named file, `.agents/skills/_skills.md`, is a pristine Framework file. It is
byte-identical to a fresh install. It is the **parent** of the problem.

`route list` on the same workspace, at the same instant:

```
$ open-forge route list skills --depth=all --view compact
Unresolved: .agents/skills/pdf-processing/SKILL.md:
            The source frontmatter or metadata shape is malformed.
```

The correct diagnosis exists inside the system. `route list` prints it.
`index`, `install`, `extension install`, and `doctor` do not, and none of them
suggests running it.

### Proposed result

Every blocking finding must carry the leaf it came from, not the projection
target. `index.metadata-unsafe` should read:

```
Cannot rebuild .agents/skills/_skills.md — one of its children is unreadable:
  .agents/skills/pdf-processing/SKILL.md   unsupported frontmatter key 'license'

Nothing was written. 19 other regions are already current.
```

## The blocking finding carries no file at all

The `preexisting` install failure has `target: null` in every view and every
format, including JSON and `--verbose`:

```
$ open-forge install --automatic --json --view compact
[{ "code": "install.generated-region-unsafe",
   "target": null,
   "cause": "Current authored source catalogue facts are unsafe or ambiguous
             for intended navigation projection." }]
```

There is no path to the answer. The data model has no answer to give.

### Proposed result

Make `target` non-nullable for every finding that blocks a mutation. A finding
that cannot name its subject is not a finding a user can act on, and should not
be allowed to stop the command.

## Diagnosis disagrees with itself

One workspace, one skill with `open-forge:` frontmatter, five commands:

| Command              | Exit  | What it says                                          |
| -------------------- | ----- | ----------------------------------------------------- |
| `index`              | 5     | blocked, names `.agents/skills/_skills.md`            |
| `route list`         | 3     | names `SKILL.md`, says `malformed`                    |
| `doctor`             | 3     | **`Findings: none`** plus one `Check incomplete` note |
| `repair --automatic` | **0** | `Status: complete`, `Remaining: 0`, `verified`        |
| `extension install`  | 5     | blocked, names `_workflows.md`                        |

`doctor` reporting `Findings: none` on a workspace that four other commands
refuse to operate on is the most serious defect in this document. `doctor` is
what every blocked command tells you to run.

On the `preexisting` workspace the same divergence appears with different
numbers: `doctor` produces 17 findings and never mentions frontmatter; `index`
names `.agents/loader.md`; `route list` names `.agents/loader.md: The Loader
file is missing.`

### Proposed result

There should be exactly one diagnosis engine. `doctor` should be that engine,
and every other command should render a filtered view of it. Concretely:

- If a command blocks, `doctor` must have a finding for it, with the same code
  and the same subject.
- If `doctor` reports zero findings, no command may block.
- `repair` must never report `complete` when `doctor` still has unresolved
  findings. Today `repair` prints `Remaining: 0; manual 0; guided 0; blocked 0`
  on a workspace where `doctor` reports 12 warnings.

## Blast radius

One dropped-in skill folder, measured on an otherwise healthy install:

| Command             | Exit | State      |
| ------------------- | ---- | ---------- |
| `index`             | 5    | blocked    |
| `extension install` | 5    | blocked    |
| `status`            | 3    | incomplete |
| `doctor`            | 3    | incomplete |
| `route list`        | 3    | incomplete |
| `context`           | 2    | attention  |
| `find`              | 0    | works      |
| `cleanup`           | 0    | works      |

Six of eight commands degrade because of one unrecognised file in a folder whose
own entrypoint description reads _"Specialized capabilities provided through
native SKILL.md packages."_

## What does work

Worth recording, because the design is closer than the experience suggests.

A `SKILL.md` carrying exactly `name` and `description` routes correctly with no
Open Forge metadata at all:

```
$ open-forge route list skills --depth=all --view compact
skills  .agents/skills/_skills.md
  skills/t  .agents/skills/t/SKILL.md
    A PDF thing; tags: []

$ grep -A2 generated-index:start .agents/skills/_skills.md
- [A PDF thing](t/SKILL.md) - #Skill
```

`SourceIdentity` already gives `SKILL.md` the folder's own ID, so the skill acts
as its own entrypoint exactly as it should. Pre-existing `AGENTS.md` content is
also preserved correctly — install appends into a managed region rather than
overwriting.

The interoperability story is one accepted-keys change and a set of message
rewrites away from working.

## Reproductions

All against `0.0.0-dev.sha-62b0e23e`, win-x64.

```sh
# 1. Strict frontmatter blocks the workspace
mkdir repro && cd repro
open-forge install --automatic
mkdir -p .agents/skills/pdf
printf -- '---\nname: pdf\ndescription: Extract text\nlicense: Apache-2.0\n---\n\n# PDF\n' \
  > .agents/skills/pdf/SKILL.md
open-forge index          # exit 5, names .agents/skills/_skills.md
open-forge doctor         # does not mention frontmatter
open-forge route list skills --depth=all --view compact   # names the real file

# 2. Remove `license:` — everything works
# 3. Circular dependency: same skill present before install
mkdir repro2 && cd repro2
mkdir -p .agents/skills/pdf && cp ../repro/.agents/skills/pdf/SKILL.md .agents/skills/pdf/
open-forge install --automatic          # exit 5, target: null
open-forge install --automatic --force  # identical
```

## The same rule blocks this repository

Found while writing this audit, by running `open-forge index` on
`<workspace>\open-forge` itself:

```
$ open-forge index
Generated navigation update is blocked.                             exit 5
Regions: 137; updates: 16; already current: 105; verified: 105
BLOCKED: Authored metadata for one direct routed child cannot be safely
         projected. [index.metadata-unsafe]
  .agents/memory/archived/cli-v2/decisions/_decisions.md
  ... 13 more
```

**`open-forge index` cannot run on the Open Forge repository.** Fourteen
regions are blocked, on committed content, before any change of mine.

`route list` again supplies the diagnosis the blocking command withholds:

```
$ open-forge route list memory/archived/cli-v2 --depth=all --view compact
Unresolved: .agents/memory/archived/cli-v2/decisions/cli-agent-first-product-contract.md:
            The source frontmatter or metadata shape is malformed.
... 66 in total
```

The cause is one unquoted colon:

```yaml
open-forge:
  description: Historical CLI-v2 source: Reusable Bun-specific test-placement shapes
                                       ^ unquoted ": " ends the scalar
```

`grep -rl "description: Historical CLI-v2 source:" .agents/memory/archived/cli-v2`
returns **66** files. `route list` reports **66** unresolved sources. Exact
correlation, and reproducible in isolation with a one-line scratch file.

The same defect hit this audit. `command-output-design.md` was first written
with `tags: [..., Progressive Disclosure]` — a tag containing a space. The
report was `malformed`; nothing said which key, which line, or what was wrong
with it. Diagnosing it took a `route list` and a guess.

### Proposed result

A malformed-frontmatter finding must name the key and the position:

```
.agents/memory/archived/cli-v2/decisions/cli-command-surface.md:3
  description: Historical CLI-v2 source: Reusable Bun-specific ...
                                       ^ unquoted ':' — wrap the value in quotes
```

Three further points this makes concrete.

- **The bar for `blocked` is far too low.** One unparseable scalar in one
  archived note stops `index` for the whole workspace, including 16 regions that
  were ready to update. Skip the file, note it, update the rest.
- **`doctor` should have caught this.** It is a plain content defect in
  committed files, in the repository that ships the tool, and `doctor` on this
  workspace does not report it as an error.
- **A YAML parse failure is not a "shape" problem.** The message describes the
  wrong layer, which is why the real cause is invisible.

## Deadlock: Open Forge cannot be installed into the Open Forge repository

Three commands, run in `<workspace>\open-forge`:

```
$ open-forge extension install development --automatic --dry-run
Status: incomplete                                                  exit 3
INCOMPLETE: The lifecycle document is missing. [extension-install.framework-unavailable]

$ open-forge install --automatic --dry-run
Status: blocked                                                     exit 5
BLOCKED: Every direct routed child requires complete authored source metadata.
         [install.generated-region-unsafe]          (target: null)

$ open-forge index
Status: blocked                                                     exit 5
  14 blocked regions
```

`extension install` needs `.agents/open-forge.lifecycle.json`. `install` is the
command that creates it. `install` is blocked by the 66 malformed archived files.
There is no way out with the CLI.

The repository root does contain `open-forge.extensions.json` — but that is a
**legacy receipt from `open-forge-old`**, declaring 21 paths under a `schema: 2`
format. Nothing in `src/cli` reads it; the current record is
`.agents/open-forge.lifecycle.json`. So the workspace looks provisioned and is
not.

Three separate defects here.

**The message names a document, not a situation.** _"The lifecycle document is
missing"_ does not say which file, where it belongs, why it is absent, or that
`install` creates it. The correct message is:

```
This workspace has no Open Forge installation record.

  Expected: .agents/open-forge.lifecycle.json

  open-forge install --automatic     create it
```

**`incomplete` is the wrong status.** Exit 3 reads as "a check could not finish".
A workspace that was never installed is a known, complete, final fact. This
should be `attention` with a next command, or `invalid` — not a partial result.

**The legacy receipt is invisible.** `open-forge.extensions.json` is recognised
in the archived contracts as a legacy artifact to report with an explicit
`legacy` state, but no command mentions it. `status` and `doctor` should say:

```
A legacy Extension receipt from open-forge-old is present.
  open-forge.extensions.json    21 paths, not read by this CLI

  It is ignored. Remove it, or run `open-forge extension migrate` when available.
```

### Proposed result

- Name the file and the command in `framework-unavailable`.
- Reclassify "never installed" from `incomplete` to a terminal status.
- Report a detected legacy receipt in `status` and `doctor`, once, as
  information.
- Fix the 66 malformed files so this repository can install itself. Add a CI
  check that runs `open-forge install --dry-run` and `open-forge index --dry-run`
  against the repository's own `.agents`, so this cannot recur.

## There is no dirty-git-tree gate in this CLI

Recorded because it was raised as a suspected cause and is not one.

`.git` appears four times in non-test source, in all cases as a directory to
**exclude** from eligible paths:

```
ExtensionDestinationPolicy.cs:14   private const string GitDirectoryName = ".git";
LibraryDestinationPolicy.cs:15     private const string GitDirectory = ".git";
LibraryEligiblePathPolicy.cs:12    private const string GitMetadataDirectoryName = ".git";
```

No command inspects git status, and `install --help` states plainly that install
_"does not discover another workspace, fetch content, manipulate Git, repair
markers..."_. The only mentions of a dirty tree in the workspace are in
**archived** `cli-v2` contracts — _"Changed managed bytes and dirty Git do not
produce a suggestion by themselves"_ — and in a developer handoff about the
author's own working copy.

So the behaviour remembered as "install refuses on a dirty git tree" belongs to
`open-forge-old`. What is actually being hit is
`install.generated-region-unsafe`, which correlates with a dirty tree only
because both are present during active development.

This is itself a finding about the messages: an error opaque enough that the
author of the system attributed it to a feature that does not exist.

### Proposed result

Decide deliberately whether a git-cleanliness gate is wanted, and record it.

If yes, it must be explicit and overridable:

```
Refusing to install into a workspace with uncommitted changes.

  12 modified, 3 untracked files

  Install writes 21 files; a clean tree makes it reviewable and revertable.

  git stash                        set them aside
  open-forge install --allow-dirty proceed anyway
```

If no, the current absence is correct and nothing changes — but the blocking
messages must improve enough that this confusion cannot recur.
