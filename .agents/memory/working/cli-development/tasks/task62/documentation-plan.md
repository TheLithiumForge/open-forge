---
open-forge:
  description: Exact Task 62 documentation ownership and verification for Loader, Framework, commands, guides, and diagrams
  tags: [Memory, Working, Task, Plan, Documentation, CLI, Contextual]
---

# Task 62 documentation plan

## Goal

Make installed instructions, current contracts, CLI help, and public examples
describe the same accepted file conditions before dependent work is accepted.

## Authority and working boundary

[Execution](execution.md#frozen-behavioral-decisions) defines the accepted meaning.
The [accepted analysis](../../../../emerging/analysis/glob-scoped-loading.md)
supplies rationale and examples. Do not rewrite that historical input to match
implementation progress. [Task 62](../task62-glob-scoped-loading.md) retains the
outcome. Root alone changes Execution and task/ledger state.

W1–W5 are disjoint writing packets. Each writer independently reads the Loader,
selected ancestor chains, Writing Standard, Dictionary, relevant maintenance
contracts and current nearby prose. W5 also reads Project Voice for introductions.
Writers are not alone in the repository and must preserve other workers' edits.
No writer commits, merges, fetches dependencies or publishes the site.

All paths below are repository-relative and select the whole file at line 1,
unless a more precise line is supplied. The prefixes expand literally:

| Prefix | Exact path |
| --- | --- |
| D | `.agents/memory/crystallized/documents` |
| Q | `.agents/memory/crystallized/decisions/framework` |
| K | `.agents/memory/crystallized/documents/cli/contracts` |
| S | `src/docusaurus` |

## Preconditions and common proof

- [ ] Root records each writer's isolated branch/worktree and exact base.
- [ ] `git status --short` shows only understood state in that worktree.
- [ ] Frozen decisions and the writer's exact ownership are read before edits.
- [ ] Current text is inspected before replacement, preserving unrelated rules.

For each changed prose file, check the diff and exact links. Use
`git diff --check` after each coherent step. Keep examples runnable and point
to the narrow source for detailed grammar instead of copying it everywhere.
Contract prose describes the accepted target while executable verification is
recorded as pending in this working packet until command evidence exists.
Do not claim the published beta already contains the new options.

Generated Entries are a serial root-owned pass after prose integration. Writers
return required entrypoint refresh paths and do not compete over parent indexes.
Root uses the task worktree's built CLI, never an older installed executable.

## W1: Installed and local runtime instructions

Depends on: accepted decisions. Blocks: dependent loading acceptance.

Own exactly:

- `.agents/loader.md:1`
- `src/open-forge/.agents/loader.md:1`
- `.agents/directives/_directives.md:1`
- `src/open-forge/.agents/directives/_directives.md:1`
- `D/maintenance/payload/agents/loader.md:1`
- `D/maintenance/payload/agents/directives.md:1`

1. Add the compact pre-load rule to both Loaders. Conditions are optional, use
   root or scoped `applyTo`, narrow applicability, and require a matching working
   file across the entire selected chain. Explain unknown paths, visible match
   activation, and hidden ancestor limits. Keep the Loader usable without the
   CLI. Verify the shared authored rule text agrees in local and shipped copies.
2. Explain that necessary related edits add their own paths to the task and load
   their own context. Merely reading a source or following a link does not add
   that source's Markdown path as a working file. A condition is not edit
   permission. Preserve overwrite adjacency and existing KeepInMind refresh
   timing while matching paths remain in scope. Check the analysis scenarios
   against the rule text without introducing a global route scan.
3. Update both Directives entrypoints so every direct Directive still requires
   `LoadNow`. `applyTo` is its file condition before loading, not permission to
   omit the loading tag or ignore an already applicable binding instruction.
   Replace any unconditional/no-second-gate wording that would contradict this.
4. Update the two maintenance contracts with the new source/runtime obligations
   and proof. Inspect all six diffs, run `git diff --check`, and return parity
   evidence plus root-owned index/Doctor targets for both trees.

Acceptance: shipped instructions contain the complete usable rule without
depending on repository-only analysis or maintenance records. Local-only routes
and generated regions are preserved.

## W2: Routing and primitive contracts

Depends on: accepted decisions and W1 wording for shared terms. Blocks: loading acceptance.

Own exactly:

- `D/framework/routing/loading.md:32`
- `D/framework/routing/model.md:1`
- `D/framework/routing/scope.md:1`
- `D/framework/routing/overwrites.md:1`
- `D/framework/routing/paths.md:1`
- `D/framework/primitives/model.md:1`
- `D/framework/primitives/directives.md:1`
- `D/framework/primitives/guidance.md:1`
- `D/framework/primitives/patterns.md:1`
- `D/framework/primitives/skills.md:1`

1. Put the full loading behavior in `routing/loading.md`. Distinguish unknown,
   matched, unmatched and invalid conditions. Define visible selection,
   explicit/reference inspection without automatic child activation, first
   LoadNow read, and KeepInMind refresh. Show the two-file counterexample where
   `src/readme.md` and `tests/A.cs` do not satisfy `src/**` plus `**/*.cs`.
2. Reconcile routing model, scope and path documents. Conditions inherit along
   the selected chain, OR within one declaration and AND on the same working
   file across ancestors. An absent local declaration inherits ancestor
   conditions. Patterns and CLI working paths are workspace-relative after
   normalization. Preserve existing Markdown link resolution semantics.
3. Update overwrite prose to inherit the base applicability and load immediately
   after an eligible base. Do not create separate overwrite conditions or index
   entries. Reconcile primitive loading summaries where they otherwise imply
   every exposed source is unconditional. Keep roles and authority unchanged.
4. Check each changed paragraph against W1 and the frozen examples. Run
   `git diff --check`. Return exact headings altered and any unchanged file from
   this ownership list whose current text needed no correction.

Acceptance: no primitive gains another authority level, every current “always”
loading statement has its real scope, and the detailed model remains in routing.

## W3: Syntax, compatibility, summaries, and rationale

Depends on: accepted decisions. Blocks: metadata and Entries acceptance.

Own exactly:

- `D/framework/markdown/syntax.md:70`
- `D/framework/markdown/routes.md:1`
- `D/framework/markdown/compatibility.md:1`
- `D/framework/architecture.md:1`
- `D/architecture.md:1`
- `Q/loading-reliability.md:1`
- `Q/canonical-markdown.md:1`
- `Q/routing-model.md:1`
- `Q/tags.md:1`

1. Define both field locations with equal meaning in syntax. Preserve scoped
   requirements for description/tags/responsibility and native Skill metadata.
   Specify quoted scalar or quoted-string list, absent versus invalid empty
   values, normalized equivalent dual sets, conflict handling, and scoped-list
   output for new fields. State no comma splitting and authored-location
   preservation. Cross-check F2's actual facts before finalizing examples.
2. Define the bounded case-sensitive grammar and the exact Entries suffix in
   Markdown routes. Show tags before ` - applies to` and comma-separated code
   spans. Entries display local declarations only. Keep unconditioned rows
   unchanged and retain existing sentinel/region/entrypoint rules. Compatibility
   must not advertise the rejected alternative suffix forms as accepted input.
3. Update architecture summaries with a concise file-condition explanation and
   links to the narrow contracts. Reconcile the four existing rationale records
   only where the accepted change supersedes their loading or syntax rationale.
   Preserve useful earlier reasoning and explicitly distinguish the new accepted
   choice from historical facts. Do not create a competing full specification.
4. Check examples against F1/F2/F3 test evidence and run `git diff --check`.
   Return any unsupported syntax example as a blocker rather than expanding
   grammar to make the prose work.

Acceptance: one canonical suffix, one grammar, one authoritative detailed rule
per concern, and no accidental support for other root-level metadata fields.

## W4: Command contracts

Depends on: accepted decisions. Blocks: corresponding command acceptance.

Own these exact Interface/Behavior pairs:

- `K/context/interface.md:1` and `K/context/behavior.md:1`
- `K/find/interface.md:1` and `K/find/behavior.md:1`
- `K/route/inspect/interface.md:1` and `K/route/inspect/behavior.md:1`
- `K/route/create/interface.md:1` and `K/route/create/behavior.md:1`
- `K/route/init/interface.md:1` and `K/route/init/behavior.md:1`
- `K/route/update/interface.md:1` and `K/route/update/behavior.md:1`
- `K/doctor/interface.md:1` and `K/doctor/behavior.md:1`
- `K/index-candidate/interface.md:1` and `K/index-candidate/behavior.md:1`

Also own affected realization detail in `K/context/technical-design.md:1`,
`K/find/technical-design.md:1`, `K/route/update/technical-design.md:1`, and
`K/index-candidate/technical-design.md:1`. Do not change shared envelopes or
Status contracts merely because its startup implementation is shared.

1. Add exact repeated option contracts, invalid-value behavior and path base.
   Authoring uses `--apply-to`; Update adds exclusive `--clear-apply-to`.
   Preserve Init metadata restrictions for framework/existing targets and Find's
   existing tag/heading requirements for `--require` and `--within`.
2. Document Context's filtered startup and explicit selection, same-file
   intersection, hidden ancestor limit, permitted nonmatch inspection and
   automatic-child boundary. Define additions-only against the same complete
   normalized path set, still requiring explicit source operands.
3. Record the optional command-owned applicability shape from
   [the command plan](command-plan.md#shared-observable-contract). Define detail
   inclusion and legacy omission. Context pending means incomplete/exit 3, a
   visible limitation, and `pendingConditions` at every detail level. Do not
   silently generalize Context's pending completion policy to Find or Inspect.
4. Specify Find compatibility as an extra filter over its existing universe.
   Ancestor fact reads never widen returned candidates. Specify Inspect's
   declared/inherited explanation and explicit nonmatch behavior. Index projects
   every declared condition without a working-set filter. Doctor uses existing
   malformed-frontmatter reporting throughout its normal scope, including Skills.
5. Describe Update's exact-span preservation and dual update/clear behavior in
   technical design, retaining preservation-unsafe failure. Link shared Framework
   matching/facts, avoiding duplicated command-local parsing instructions.
6. Compare each pair with the matching slice's help, managed process receipts,
   text and JSON. Use `git diff --check` and return a compact option/output/exit
   consistency table. Mark executable checks pending until the code is available.

Acceptance: every changed caller-visible behavior appears in its contract pair,
and no contract contradicts absent-field legacy output or existing mutation safety.

## W5: Public guides and diagrams

Depends on: accepted decisions for drafting, C1–C3/A1–A3 for executable examples.
Blocks: documentation milestone.

Own exactly:

- `README.md:1`
- `docs/cli.md:1`
- `S/docs/concepts/loading-and-tags.md:1`
- `S/docs/concepts/scopes.md:1`
- `S/docs/concepts/routing.md:1`
- `S/docs/cli/index.md:1`
- `S/docs/cli/flows.md:1`
- `S/src/components/framework-map/framework-map-data.ts:1`
- `S/static/img/framework-map-light.svg:1`
- `S/static/img/framework-map-dark.svg:1`

The SVGs are generated output. Edit their data source, then regenerate with the
existing script. Do not hand-edit SVG text or alter diagram layout/components
unless root assigns a concrete resulting layout defect. Keep other diagram
tasks' accepted wording and labels intact.

1. Add one approachable `applyTo` example in the loading guide, including the
   need to select a hidden ancestor explicitly. Explain related C#/TypeScript
   edits as adding both working paths. Show unknown paths as pending and a
   planned nonexistent file as supported. Avoid implying a permission boundary.
2. Update the CLI guide with repeated `--for`, repeated `--apply-to`, explicit
   clearing, additions-only with source operands, and Inspect explanation.
   Align command reference tables and the site CLI entry/flow pages. Use the
   built worktree artifact's help to verify commands before calling them current.
3. Adjust README, routing/scope summaries, and diagram data only where their
   current loading claims become inaccurate. Keep compact introductions compact.
   Link detailed semantics instead of adding the full grammar to every surface.
4. Run the following offline/local commands after dependencies are prepared:

   ```powershell
   npm --prefix src/docusaurus run diagram
   npm --prefix src/docusaurus run typecheck
   npm --prefix src/docusaurus run build
   git diff --check
   ```

   Record exit codes and inspect both generated light/dark diagrams for clipped
   or overlapping changed text. If the site build needs a missing package,
   report the missing prerequisite without a network install.
5. Verify examples using `artifacts/publish/open-forge-dev/Release/open-forge-dev.exe`
   in a disposable test workspace from command fixtures. Execute each new
   authoring example only in that fixture. Compare the produced frontmatter,
   Entries and Context/Find/Inspect output with the prose. Report expected exits
   and actual results to root, keeping private machine paths out of public docs.

Acceptance: public examples run, site links/build succeed, both diagrams remain
legible, and the text does not promise hidden-rule or dependency discovery.

## Root reconciliation and completion

Root integrates W1–W5 after inspecting their actual prose. It checks references
and runs generated navigation and Doctor separately in the repository and shipped
tree as required by the maintenance contract. Use the built artifact and inspect
its `index --help`/`doctor --help` before forming commands for each explicit tree.
Record existing reference-alias findings separately from new Task 62 defects.
Do not bulk-fix unrelated Doctor output or overwrite whole generated trees.

After command integration, root compares Loader rules, Framework contracts,
command contracts, public help, guide examples and diagrams against the same
behavior matrix. Remove stale “proposal” or “not implemented” statements only
from affected current sources when their implementation is demonstrated. Keep
the accepted analysis and archived evidence intact.

- [ ] W1 runtime text and maintenance parity verified.
- [ ] W2/W3 current Framework meaning and syntax reconciled.
- [ ] W4 command option, result, detail and exit contracts verified.
- [ ] W5 public examples, links, site build and diagrams verified.
- [ ] Root-owned Entries and Doctor checks recorded for both trees.
- [ ] One coherent final prose inspection completed within the existing review budget.

## Divergences observed

None recorded. Writers return deviations to root for this shared packet.

## Rollback

Preserve each owned diff. Root removes only that writer's exact integrated
changes when abandoned and regenerates affected SVGs from the retained data.
Do not restore directories or undo another writer's source or generated changes.
