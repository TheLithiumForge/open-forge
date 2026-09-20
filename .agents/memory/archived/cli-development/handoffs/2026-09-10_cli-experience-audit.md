---
open-forge:
  description: State after the CLI experience audit and the first phase of fixes, with what is decided, what is open, and where the next session starts
  tags: [Memory, Archived, Contextual, Historical, Handoff, CLI, Audit, Remediation]
---

# CLI Experience Audit Handoff

## Where this stands

The CLI was driven across nine workspace states and audited against its own
contracts. The findings, the retrospective on how they arose, and a sequenced
remediation backlog are recorded. Phase 0 of that backlog is seven-elevenths
done, verified, and committed.

Two commits, neither pushed:

- `5c608516` — the audit, the retrospective, the backlog, six new C# directive
  clauses. No production code.
- `1fd29dbc` — seven release-gating fixes, with unit-suite parity proven.

## Read these, in this order

- [CLI Experience Remediation](../cli-development/tasks/cli-experience-remediation.md) —
  the backlog. Its **Execution order** section is the entry point; everything
  else is reference.
- [CLI Experience Audit](../../../emerging/analysis/cli-experience-audit/_cli-experience-audit.md) —
  what is broken, 19 documents.
- [CLI Design Retrospective](../../../emerging/analysis/cli-design-retrospective/_cli-design-retrospective.md) —
  how it arose, separating the original design from the contracts from what
  agents built. Not needed to do the work.

Do not load all of it. Each phase names the one or two documents it needs.

## What was fixed

Each reproduced before and verified after.

- **Extensions were uninstallable** in any workspace that had had a route added.
  Install fingerprinted a generated `Entries` region from the _payload_ bytes,
  but the file on disk has that region generated — so the recorded baseline never
  matched what install itself wrote.
- **Every clone was blocked.** The lifecycle record pinned an absolute path and
  compared it to the current one.
- **A standard `SKILL.md` blocked the workspace** if it carried `license:` or
  `allowed-tools:`.
- **`repair` crashed.** Two bugs: the catch discarded the message, and candidates
  were emitted per evidence-basis, so one file found two ways produced two
  identical targets and tripped a uniqueness check.
- **A byte order mark or trailing space on a fence** made valid frontmatter
  invisible. Windows editors write the mark by default.
- **The CLI only worked from the repository root.** It now walks up to `.agents`.
- **66 committed files had an unquoted colon** in a description, which blocked
  `index` on this repository. `index` here is now `incomplete`, not `blocked`.

Unit suite: 18 pre-existing failures of 3,347 before, 18 of 3,355 after, **0
newly failing**. Four tests asserted the defects being fixed and were rewritten;
eight cases added.

## What is decided

Recorded so these are not reopened.

- **`references` is correct as built.** It reports _authored_ references only.
  The generated `Entries` tree is `route list`'s job. Only the wording is wrong —
  moved to G4.
- **`AGENTS.md` and `CLAUDE.md` stay untouched.** They are the user's files and
  shared with other tools, so the footprint stays minimal. The _rule_ changes
  instead: an entry file is not a routed source and needs no route metadata.
- **Workspace discovery** walks up to the first `.agents`; `--workspace` is never
  redirected.
- **No git-cleanliness gate.** Commands run on a dirty tree; one advisory line
  after a write, never a block.
- **Keep Workflows.** A Workflow is followable without a runtime; a Skill is not.
- **Do not add a `changes` category.** It is workstream state.
- **Commit messages**: past tense, actionable, no `type(scope):` prefix, subject
  only. _"Improved X by Y."_

## What is open

**Two Phase 0 items remain**, both real work rather than decisions:

- **`context` exits 2 on a pristine install.** Exempt entry files from route
  metadata. Small; the decision above settles the approach.
- **External Extension sources fail silently.** Payload outside `content/` gives
  `Status: complete`, `Effects: 0`, nothing installed, no explanation. `Effects:
0` on an apply should be `attention` with a cause, and a bad manifest key
  should name the accepted keys instead of leaking a .NET serializer message.

Both were left rather than rushed at the end of a long session.

**Then Phase 1** — deduplication and folder collapse. It depends on nothing and
is the force multiplier for everything after: 12 files reimplement a shared
mapping that already exists, 10 of 22 `*HelpSections` bypass `CliResultHelp`
(a hard blocker for G5), and 168 of 624 folders hold exactly one file.

**Before Phase 1**, capture a throwaway characterization baseline — full output
for every command against two or three seeded workspaces, into files, diffed
after. Phase 1's whole claim is _nothing changes_, and nothing currently proves
that: 202 human-text assertions across 2,811 tests, almost all `Assert.Contains`,
which cannot fail on noise. This is _not_ the permanent snapshot suite, which
waits for G4.

## Two things worth knowing

**Most defects were specified, not introduced.** `expanded` as default, the dual
JSON schema, `reference.target-valid` as an informational finding, the absolute
`workspacePath` — all required by the accepted contracts. The implementers built
what they were told. The gap is between the contracts and what a user needs, so
better implementers would not have helped. Detail in the retrospective.

**Tests enshrine defects here.** Four unit tests asserted the exact behaviour
being fixed; `DoctorHumanSnapshots` still contains a rendering bug as its
expected value. Expect this in every phase and budget for rewriting tests as part
of each fix, not as a surprise.

## Environment notes

- `npm run cli:link` needs `vswhere.exe` on `PATH`:
  `C:\Program Files (x86)\Microsoft Visual Studio\Installer`.
- A stale global `open-forge` shim can make `cli:link` fail with `EEXIST`.
- `npm run test` is the suite. `dotnet test --project ...` reports zero tests.
- Five `dotnet format` whitespace errors are pre-existing, in untouched files.
- Test playgrounds are outside the repository under
  `<workspace>\open-forge-test\`.
