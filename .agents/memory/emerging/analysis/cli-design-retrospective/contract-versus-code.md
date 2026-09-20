---
open-forge:
  description: Where the shipped CLI diverges from its accepted contracts, which defects were specified rather than introduced, what the overseer model actually cost, and whether to rebuild
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Architecture, Contracts, Process, Assessment]
---

# Contract Versus Code

## Conclusion

**Most of what this audit found was specified, not introduced.** The
implementers largely built what the contracts told them to build. The divergence
is not between the contracts and the code — it is between the contracts and what
a user needs.

That reframes the process question. The overseer/implementer model did not fail
at execution. It failed at the one thing it could not check: whether the thing
being specified in exhaustive detail was worth having. Preflight and review
verify conformance to a contract; nothing verified the contract.

On rebuilding: **do not restart.** The model layer, the parsing layer and the
test harness are sound and represent real encoded knowledge. Three subsystems
should be deleted rather than repaired, and one command family is worth
rebuilding. That is a large refactor, not a greenfield.

## Method and boundary

The greenfield reset is `768bd51a` _"Redesign Open Forge and define replacement
CLI"_, 2026-08-17. HEAD is 191 commits later, 2026-09-13 — **27 days**.

| Area                                     | Lines added since reset | Present now |
| ---------------------------------------- | ----------------------- | ----------- |
| `src/cli/core`                           | +237,483                | 196,370     |
| `src/cli/tests`                          | +153,265                | 133,541     |
| `.agents` (contracts, memory, framework) | +107,673                | —           |
| `scripts`                                | +6,105                  | —           |

~505,000 lines added in 27 days. The accepted CLI contract set alone is
**40,540 lines of Markdown** against 196,370 lines of core code — a 1:5 ratio,
with `find/interface.md` at 1,741 lines for a search command.

## Contract versus code, in three categories

### 1. Specified defects — the implementers complied

Each of these is a finding from this audit that the accepted contracts require.

| Finding                                          | Contract                                                                                                                                                                                                      |
| ------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `expanded` is the default view                   | `shared/global-flags/behavior.md:95` — _"expanded remains the default"_                                                                                                                                       |
| `--view` silently swaps the JSON schema          | `global-flags/behavior.md:99-100` — _"Expanded JSON uses the complete schema-v1 projection. Compact JSON uses the identified schema-v2 projection"_                                                           |
| `--view compact` sometimes returns expanded      | `global-flags/behavior.md:194-196` — _"Each output format has a required expanded renderer and an **optional** compact renderer… Explicit compact selection uses compact when available, otherwise expanded"_ |
| 5,550 "link is valid" findings                   | `doctor/behavior.md:417` — _"`reference.target-valid` records a complete local resolution as information"_                                                                                                    |
| 1,817 "links form a cycle" findings              | `doctor/behavior.md:438` and `doctor/interface.md:396` — _"informational; cycles do not select a repair"_                                                                                                     |
| Absolute `workspacePath` in the lifecycle record | `route/init/behavior.md:262`, `interface.md:228` — listed as a required field                                                                                                                                 |
| 41 lines of `not-applicable` in `status`         | `status/interface.md:89` — _"not-applicable, not unavailable"_ as a required rendered distinction                                                                                                             |
| The "wizard" is a typed-word dialogue            | `repair/behavior.md:101-102` — _"Select, skip, and back change the in-memory typed request"_. Arrow keys, a rendered list and multi-select are **never** mentioned                                            |

The wizard is the clearest case. `RepairWizard.cs` implements `select`, `skip`,
`back`, `cancel` — precisely the contract. The word "wizard" set an expectation
**the contract itself never made good on**. No implementer erred.

### 2. Implementation violations — the code contradicts accepted documents

Materially fewer, and one is severe.

**`SKILL.md` strict-key rejection** contradicts three accepted framework
documents at once:

- `framework/primitives/skills.md:18` — _"The selected `SKILL.md` is
  **authoritative** for the Skill's metadata, applicability, instructions,
  resource organization, and internal loading behavior."_
- `framework/markdown/syntax.md:110` — _"Standard files such as `SKILL.md` keep
  the metadata **required by their active runtime**."_
- `extensions/architecture.md:85` — _"Keep native formats such as `SKILL.md`
  with their own metadata and resources. Routing a native package does **not**
  require wrapping or rewriting it as another Framework category."_

[`SourceAuthoredMetadataParser.cs:120`](../../../../../src/cli/core/OpenForge.Cli.Core/Framework/Sources/Metadata/SourceAuthoredMetadataParser.cs#L120)
rejects any key but `name` and `description`, so `license` and `allowed-tools` —
metadata required by the active runtime — hard-block the workspace. This is a
direct violation, and it is the interoperability ship-blocker.

Also in this category: the `repair` `ArgumentException` crash, `references`
reporting zero links while asserting `coverage complete`, `repair` reporting
`complete` and `verified` after doing nothing, and the derived-projection
comparison that blocks `extension install` after any route change. These are
bugs against intent, not compliance with a bad spec.

### 3. Overreach beyond the stated boundary

The architecture defines its own limit, `architecture.md:60-63`:

> _"The CLI manages user-owned Markdown and supporting files in a local
> development workspace. **It is not a security boundary, database, or
> mission-critical transaction processor.**"_

Against that stated boundary:

| Concern                     | Lines      | Files |
| --------------------------- | ---------- | ----- |
| Recovery, Mutation, Locking | **18,309** | 151   |
| Interaction                 | **355**    | 6     |

A 52:1 ratio, in a tool the architecture explicitly says is not
mission-critical, managing Markdown files that live in a git repository.

And `architecture.md:35` states the audience: _"one predictable native
executable for **agents** and occasional human use."_ Agents are named first.
`doctor` on this repository emits **8.8 MB / 165,919 lines** — roughly 2.2
million tokens. The single most-stated goal is the single most-violated one.

The `Library` subsystem is 180 files and 10,876 lines for a feature that cannot
be used outside a TTY without hand-writing an undocumented JSON schema recovered
from source.

## Where the divergence came from

A causal chain, each link evidenced.

**1. The contracts specify output, exhaustively.** Interface contracts run
700–1,741 lines each and prescribe rendered vocabulary field by field —
`not-applicable` versus `unavailable`, exact paths, exact schema versions.
Meanwhile **most commands have no Technical Design at all**: the contract set
lists _"None; no Technical Design exists"_ for `status`, `references`, `doctor`,
`repair`, `install`, `update`, `cleanup`, and every `extension` operation.

The documentation is heavy on _what the output must contain_ and light on _how
the system is built_. That asymmetry produced exactly what you would predict.

**2. Faithful renderers followed.** 324 rendering files, 25,477 lines, 65
distinct `*Human*` types. Each renders its contracted fields correctly. **No
layer was ever specified to decide what to omit** — because omission was never
in a contract. The missing View-selection stage is not an oversight in the code;
it is absent from the specification.

**3. Task decomposition optimised for boundability, not comprehension.** 1,866
core files, **median 72 lines**, mean 105. 775 files were touched exactly once
and never revisited. `Route` alone is 493 files and 55,088 lines — 28% of the
core for one command family. This is what _"let bounded implementers execute
closed Tasks without inventing architecture"_ (`architecture.md:47`) produces
when applied without a countervailing force toward consolidation.

**4. Bookkeeping outweighed building.** Of 191 commits since the reset:

- **52** begin with _Recorded / Reconciled / Aligned / Prioritized / Restored_
- **28** begin with _Implemented / Added / Established / Completed / Fixed_

Nearly 2:1 in favour of recording state over changing the system, and 107,673
lines of `.agents` churn alongside it.

## Structural failure versus agent failure

**Structural — the model working as designed, producing the wrong thing:**

- Exhaustive interface contracts with no editorial constraint. Nothing in any
  contract says "output must be small", "a healthy result is one line", or
  "informational findings are suppressed by default". The word _token_ appears
  in contracts only as a lexer term. _Progressive disclosure_ appears **nowhere**
  in the CLI contract set, in a framework whose entire premise it is.
- Review verified conformance. Preflight verified safety. Neither could ask
  _"should this exist?"_, because that question is out of scope for both.
- Per-command contracts with no cross-command editor produced 20 commands that
  each look reasonable in isolation and are inconsistent together — `--view`
  meaning format in `find` and density elsewhere; `./` accepted by
  `route inspect` and rejected by `library attach`; `update` and
  `extension install` returning different verdicts for the same change.
- The safety boundary was stated once, in prose, and never converted into a
  budget. So 18,309 lines went into durability that nothing capped.

**Agent — genuine implementation error:**

- The `SKILL.md` strict-key rule, contradicting three accepted documents.
- The `repair` crash surfacing a bare `ArgumentException`.
- `references` ignoring generated Entries while claiming complete coverage.
- `repair` reporting `verified` for a run that selected zero findings.
- The derived-projection baseline comparison.
- `CRLF` blocking despite `NormalizeLineEndings` existing but not being applied
  on the gating path.

The agent failures are real but **contained and cheap to fix** — a handful of
files. The structural failures account for the bulk of the audit and cannot be
fixed by better implementers.

**The honest summary:** dumb implementers were not the problem. Smart reviewers
were not the solution. The specification was the bottleneck, and the process had
no stage whose job was to say _"this contract is too detailed and specifies the
wrong thing."_

## Conceptual quality of the current code

**Sound, and worth keeping:**

- **The typed result model.** Every command produces a typed record that both
  the human renderer and the JSON projector consume. This is the correct spine
  and it is exactly what a View-selection layer plugs into. It already exists.
- **Markdig-based document parsing** with a source map, frontmatter, sections and
  inline facts. Correct choice, correctly used — and already capable of the
  heading-based region detection that would remove the HTML markers.
- **The source identity model.** `SourceIdentity` giving `SKILL.md` its folder's
  ID is genuinely elegant; the interop bug is in the metadata parser above it,
  not the identity model.
- **The semantic fingerprint _intent_.** `RemoveInterior` + line-ending
  normalization is the right idea, undone by the second baseline.
- **The seven-value semantic exit-code scheme.** Good design, applied
  inconsistently.
- **The test harness** — base64 write-freedom snapshots and closed-vocabulary
  enum tests. Better than most projects have.

**Unsound, and worth deleting rather than repairing:**

- **The lifecycle baseline machinery.** 85% of the record, two ship-blockers, and
  git answers the question better.
- **The permission subsystem** as a separate model with its own file and no grant
  path. It is an allow-list; it should be four lines of config.
- **Most of Recovery/Mutation/Locking** relative to the stated non-critical
  boundary. Not all of it — atomic write and a workspace lock are worth keeping —
  but 18,309 lines is not proportionate to "Markdown files in a git repo".
- **The rendering layer as currently structured.** Not because rendering is
  wrong, but because 324 files with no selection stage cannot be incrementally
  steered toward brevity. The renderers should shrink dramatically once selection
  exists.

**Worth rebuilding rather than repairing:** the `Route` command family. 493 files
and 55,088 lines — 28% of the core — for seven operations, carrying the identity
collisions, the `route init` baseline poisoning, the `./` path inconsistency, and
the escaped-payload rendering. It is the largest, most churned, and most
defect-dense area, and its contracts are among the most prescriptive.

## Should it be scrapped?

**No — and the reasoning matters more than the answer.**

A rewrite would discard 133,541 lines of tests encoding real edge-case
knowledge, a correct parsing layer, a correct identity model, and a correct
result-model spine — to fix problems that are **specification problems**. The
same overseer model, pointed at the same contracts, would rebuild the same
system. The failure mode was never "the code came out wrong"; it was "the code
came out exactly as written."

Rewriting also assumes the contracts would be better the second time. Nothing in
the process changed that would make them so. Fix the specification discipline
first and the code follows; rewrite first and you get a second faithful
rendering of the same 40,540 lines.

### What to do instead

1. **Retire the interface contracts as rendering specifications.** Keep them as
   _data-model_ specifications — what facts exist, what states they can hold.
   Delete every prescription of rendered vocabulary and layout. That single act
   removes the source of most of this audit.
2. **Write one presentation contract for all 20 commands**, not 20 contracts.
   It owns the three detail levels, the selection rules, severity ordering, and
   a per-command size budget. It should be short — a few hundred lines against
   the current several thousand.
3. **Delete the three unsound subsystems** (G1 and G4 in the backlog). This is
   subtraction, and it is where the largest quality gain per unit of effort sits.
4. **Rebuild `Route` behind its existing tests**, once the presentation contract
   exists.
5. **Add the one missing process stage**: before a contract is accepted, someone
   asks _"what does a user see, and is it worth what it costs?"_ That question
   has no owner today, and it is the whole gap.

### The decision rule, if you want one

Rebuild a subsystem when its **contract** was wrong and its **tests** encode the
wrong behaviour — because then nothing is preserved by keeping it. That is true
of `Route` and arguably of `Library`.

Repair a subsystem when its contract was wrong but its **model** is right —
because the model is the expensive part. That is true of everything else,
including all of presentation.

By that rule: roughly 65,000 of 196,370 core lines are rebuild candidates,
~25,000 are deletion candidates, and the remaining ~106,000 are worth keeping
and steering. That is a large, well-scoped refactor with a clear order — not a
restart.
