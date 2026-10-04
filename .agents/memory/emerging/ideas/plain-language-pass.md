---
open-forge:
  description: Replacing the ceremonial vocabulary across CLI output, contracts, and framework prose with words a tired person can read
  tags: [Idea, Memory, Emerging, Contextual, Candidate, CLI, Language, Writing, Experience]
---

# Plain Language Pass

## The idea

Replace the ceremonial vocabulary throughout Open Forge with words that carry
the same meaning and can be read without decoding. A pass has been made at this
before; the audit shows how much remains.

## Historical audit examples

These strings were reproduced during the original audit. They are dated
evidence, not a claim that each still appears in the current CLI.

| Currently                                                                                                              | Means                                           |
| ---------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------- |
| _"The selected physical layer has no authored frontmatter."_                                                           | This file has no frontmatter.                   |
| _"The lifecycle workspace binding does not match the selected workspace."_                                             | This record was written for a different folder. |
| _"Framework lifecycle absence is established by the complete operational proof."_                                      | Open Forge is not installed here.               |
| _"The bounded local-reference candidate scan did not complete; no cardinality was inferred."_                          | Some links could not be checked.                |
| _"Typed Extension bridge-registration role and observed-state authority is unavailable; no target role was inferred."_ | (nobody knows)                                  |
| _"Confirmed: The selected route roots and requested structural depth were confirmed."_                                 | (nothing — delete it)                           |
| _"Current authored source catalogue facts are unsafe or ambiguous for intended navigation projection."_                | A file below here could not be read.            |
| _"Repair requires an exact `--relink`, or an interactive wizard."_                                                     | Repair needs to know what to fix.               |

The original Repair example raised an interaction question as well as a wording
question. Current command contracts and observed behavior must be checked before
reusing it as a defect.

## Why it matters more than it looks

- **Every one of these is the only thing a user sees when something breaks.** The
  audit repeatedly found that a real, fixable problem was reported in words that
  did not name the file, the cause, or the action.
- **It hides defects.** _"The source frontmatter or metadata shape is malformed"_
  concealed an unquoted colon in 66 files, a tag containing a space, and a strict
  key rule — three unrelated causes behind one sentence.
- **It is not confined to output.** The same register runs through the contracts
  (_"Compact JSON uses the identified schema-v2 projection"_) and the framework
  prose. A rewrite that only touched command output would leave most of it.

## Scope, roughly in order of value

1. **Findings and error messages.** Highest value: these are read at the worst
   moment. Every one should name the file, the cause, and the next step.
2. **Command output labels.** `Selected by`, `Coverage`, `Observed`, `Read from`,
   `not-established`. Check which still appear under the accepted detail selection before
   proposing replacements.
3. **Help text.** _"Establishment"_, _"Write policy"_, _"Results and streams"_.
4. **Type and member names.** The historical naming proposal included `*HumanRenderer*` and `ExtensionInstallFoundationReader`,
   `LifecycleWorkspaceBinding`, `RouteInspectAxiomsProfileBuilder`.
5. **Contracts and framework prose.** Review current wording under the
   accepted source boundaries and Writing Standard.

## Current follow-up

Task 30 G4 and Task 31's selected naming and structure work are closed. They
cannot take new wording changes. Shared reporting now selects four `--detail`
levels and `--format` before rendering. This Idea does not reopen those Tasks
or approve mechanical renaming of code.

Reproduce each remaining wording problem against the current result and
contract. The [current ledger](../../working/cli-development/project-control.md#active-task-ledger)
routes unfinished work: Task 34 and the wider Task 39 audit are paused, while
Tasks 37, 44, 61, 66, and 67 retain their own wording or documentation scopes.
Use the [Writing Standard](../../crystallized/documents/maintenance/writing.md)
for prose and retain the narrowest existing Task for any selected correction.
The historical retrospective remains provenance rather than a gate for every
future sentence edit.

## One rule worth adopting

A test that reads well: **could a tired person, at the moment the message
appears, do the next thing without asking anyone?** If the sentence names a
concept rather than a file, or a state rather than an action, it fails.

That rule is also cheap to enforce in review, which the current register is not.
