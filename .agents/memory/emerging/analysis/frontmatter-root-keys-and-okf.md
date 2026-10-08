---
open-forge:
  description: Research on moving routed metadata from the open-forge scope to root frontmatter keys, whether to align with Google's Open Knowledge Format, and other improvements found before 1.0
  responsibility: Preserve the evidence, recommendation, costs, and open decisions from the October 2026 frontmatter and OKF research without accepting a format change
  tags: [Memory, Analysis, Contextual, Candidate, Framework, Frontmatter, Metadata, Interoperability, Release]
---

# Root Frontmatter Keys And The Open Knowledge Format

Status: unaccepted research from 2026-10-08. Eleven independent read-only investigations examined `develop` at `70c5b68dd`, each through a different lens: ecosystem interoperability, the OKF specification, OKF fit, CLI impact, content and migration impact, an argument for each side, decision history, the metadata schema, startup cost, Framework improvements, and CLI release readiness. The overseer verified the decisive repository claims before recording them. Nothing here changes a contract.

## Questions

1. Is it viable, and sensible, to drop the `open-forge:` frontmatter scope and put `description`, `responsibility`, `tags`, and `applyTo` at the root?
2. Is the [Open Knowledge Format](https://github.com/GoogleCloudPlatform/open-knowledge-format) (OKF) something Open Forge should align with, bridge to, or ignore?
3. What other improvements matter before 1.0?

## Current Conclusion

Root keys are viable. After the research, the maintainer proposed letting each workspace choose its form at first install instead of switching every workspace to one canonical form. That proposal is the current recommendation, as described in [Choose The Form Per Workspace](#choose-the-form-per-workspace). Either way, every workspace should read both forms. Decide on authoring reliability and coexistence, not on tokens or OKF. Treat OKF as a promising draft to watch, not a standard to adopt. Items I1 to I5 under [Other Improvements](#other-improvements) were fixed on 2026-10-08, independently of the format decision.

## Choose The Form Per Workspace

The maintainer proposed this on 2026-10-08. The first install asks whether the workspace uses root keys or keys scoped under `open-forge:`, and shows an example of each. It is not accepted yet.

One premise needs correcting first. Today the CLI reads only `applyTo` at the root. It ignores root `description`, `tags`, and `responsibility`. Reading both forms is therefore the first piece of work, whichever option is chosen.

The setting should choose output only:

- **Reading.** Every workspace reads both forms. An `open-forge:` block, when present, is the complete Open Forge metadata set, and root keys beside it belong to another tool. This also removes today's silent failure in scoped workspaces.
- **Output.** The setting decides what Install and Update write for Framework and Extension files, what route creation, route initialization, and adoption write for new metadata, and how installed Templates look. Copies then follow the workspace's form.
- **Changing it later.** `install --configure` changes the setting. Update then rewrites owned files through its normal lifecycle. User-authored files stay as they are and remain readable.
- **Storage.** A key such as `"frontmatter": "root" | "scoped"` in the workspace settings file, with `--frontmatter` for unattended installs. A missing key means scoped, so existing workspaces, including this repository, change nothing.

Assumptions that would change:

| Today | With the setting |
|---|---|
| A missing `open-forge:` block means missing metadata | Root keys are read when no block exists |
| Shipped payload bytes are the intended bytes | Intended bytes are the payload written in the workspace's form. Install already projects generated `Entries`, and fingerprints already ignore their interior, so this extends an existing step. The lock records no payload hash |
| Canonical output is scoped | Canonical output follows the setting |
| Route Update edits exactly one scoped mapping | Route Update edits metadata where it was authored |
| The loader and CLI Skill describe only the scoped form | They describe both forms and tell agents to follow the workspace's form |
| Status, Doctor, and Update compare managed files with raw payload | They compare with the payload written in the workspace's form |

Compared with one canonical root form, this avoids converting this repository's 510 non-archived files and every beta workspace. It also keeps the coexistence choice explicit for repositories whose Markdown already serves Docusaurus or Obsidian. The extra cost is a second output form for every writer and for payload rendering, about twice the canonical-output snapshots, and documentation that shows both forms.

Do not label the root option "Open Knowledge Format frontmatter". A file without `type` is not an OKF document. Describe it as root keys that use the same layout as Copilot, Cursor, Docusaurus, and OKF.

## Root Keys

### What The Evidence Shows

Seven of the eleven investigations leaned toward root canonical metadata with scoped input retained. One recommended keeping the namespace for now. Three were neutral and said to decide on the merits.

- **Today's failure is silent.** Root `description` and `tags` are ignored. Index falls back to the automatic source label, `find --tag` misses the file, and a root `LoadNow` loads nothing. Agents and people write root keys by habit, because Copilot, Cursor, Claude Code, Kiro, APM, Agent Skills, Docusaurus, and OKF all use root keys.
- **Metadata location already follows three rules.** Ordinary metadata is scoped, `applyTo` may sit at either location, and native `SKILL.md` metadata stays at the root. The [compatibility contract](../../crystallized/documents/framework/markdown/compatibility.md) says the root exception "applies only to `applyTo`".
- **The namespace was never a technical necessity.** The initial design in May 2026 accepted root keys and offered the scope as an alternative. June made the scope preferred, then mandatory, for consistency: one authored form lowers agent decision cost and parser surface. A single root form satisfies that rationale equally. Task 62 later accepted only root `applyTo` and recorded no reason for excluding the other fields.
- **The namespace's real value is coexistence.** It lets one file carry Open Forge metadata beside a different tool's `description` or `tags`. Its reach is narrow. Install adoption reads only the standard routes under `.agents`, so a Docusaurus `docs/` page or a `.github/instructions` file is never reinterpreted.
- **Host tools rarely activate `.agents` files on their own.** Copilot, Claude Code, Cursor, Kiro, Cline, and Continue discover their own folders. Matching field names do not give matching semantics: Copilot attaches by `applyTo`, while Open Forge says `applyTo` "never loads a file by itself."
- **Token savings are negligible.** Removing the wrapper saves about 88 estimated tokens at dogfood startup and about 36 in a fresh Full Core install, roughly 0.4%. Generated `Entries` lines do not change.
- **OKF does not decide it.** OKF permits arbitrary extra keys, so either shape is acceptable to it. No Open Forge file is OKF-conformant anyway, because OKF requires `type`.

### Proposed Shape

```yaml
---
description: C# design rules for source and tests
responsibility: Define construction, nullability, and callable boundaries
tags: [CSharp, Design]
applyTo: ["**/*.cs", "**/*.csproj"]
---
```

Rules for reading both shapes:

- When an `open-forge:` block exists, it is the complete Open Forge metadata set. Root `description` and `tags` then belong to another tool, and the reader does not fill gaps from them.
- When no block exists, root keys are Open Forge metadata only for sources already admitted as routes. Root keys alone never make a file an Open Forge source.
- `applyTo` keeps its current rule: equivalent declarations in both locations are one condition, and different declarations are invalid.
- Native `SKILL.md` keeps its own contract and parser. The Agent Skills reference validator rejects unexpected root fields, so Open Forge fields must never be added there.
- No per-file version marker is needed, because the two shapes are distinguishable.

### Migration And Cost

1. Amend the syntax and compatibility contracts first, then about twenty command-contract passages that state the scoped rule.
2. Implement dual reading at the shared metadata owner. Switch canonical output to the root. Route Update edits metadata in the location where it was authored and never flattens a file implicitly. The CLI estimate is about 12 to 16 production files, 20 to 30 test files, and 53 canonical-output snapshots. Scoped-input fixtures stay as compatibility coverage.
3. Convert the 70 distributed headers in `src/open-forge` and `src/extensions`, all Templates, the loader text, the CLI Skill reference, and five public documentation pages together. Update replaces owned payloads through its normal lifecycle.
4. Convert the 510 non-archived dogfood files mechanically. The 529 Archived files can stay scoped because they remain readable.
5. Leave user-authored files as they are. A later, previewable conversion command can flatten them on request.

The hardest part is Route Update's layout-preserving editor, together with the rule for a block beside foreign root keys. Older CLI versions cannot read root-only files, so the release notes need a minimum CLI version.

### Related Schema Question

One investigation proposed replacing the `LoadNow` and `KeepInMind` tags with a `load: now | refresh` field during the same migration. Its strongest evidence is that the [loading contract](../../crystallized/documents/framework/routing/loading.md) carries both tags as topic labels. Under the loader's rule that defined tags keep their meaning wherever they appear, selecting that scope loads and refreshes the file. The overseer recommends against the larger change for 1.0. The tags are salient in `Entries` and already reserved, so retagging that one file fixes the observed defect. This remains an open question.

## Open Knowledge Format

OKF is Google Cloud's format for organizational knowledge, such as metrics, tables, datasets, APIs, and runbooks. A bundle is a folder of Markdown files with root YAML frontmatter. Only `type` is required. `title`, `description`, `resource`, and `tags` are recommended, and v0.2 adds `sources`, `generated`, `verified`, `status`, and `stale_after`. Reserved `index.md` files, optionally generated and without frontmatter, support navigation one level at a time.

The evidence supports watching OKF, not adopting it:

- **Governance is Google-led.** The specification lives in a Google repository. Changes require a Google CLA and maintainer review. There is no independent steering group or stability promise.
- **It is young and still moving.** At research time it had 6 commits, no releases, and 17 open issues. v0.2 changed accepted timestamp values without changing its version number, so `okf_version: "0.2"` alone does not identify one contract.
- **Conformance is thin.** The floor is parseable frontmatter and a nonempty `type`. There is no JSON Schema yet. The reference tooling generates knowledge from BigQuery and is not a general loader. The Google connector carries only seven frontmatter keys and drops the rest.
- **The purpose differs.** Open Forge's Directives, loading tags, scopes, and acceptance have no OKF equivalent. OKF's `status: stable` is not Open Forge acceptance.
- **`index.md` matches only by filename.** Open Forge accepts `index.md` as a compatibility `entrypoint` name, but an Open Forge entrypoint needs metadata and generated `Entries`, while an OKF index has neither.

Recommended stance:

1. Do not add `type`, do not make the `.agents` tree a bundle, and do not let OKF affect 1.0.
2. Root keys, if accepted, align `description` and `tags` with OKF as a side benefit.
3. A Map can already point to an unchanged OKF bundle.
4. Build an optional bridge Extension only when a named consumer needs it. It would export selected Crystallized documents with an explicit `type`, rebased links, and a loss report. It would import bundles as referenced knowledge that never becomes accepted automatically. Pin it to a specification commit.

## Other Improvements

The investigations found these candidates. Each was checked against the repository unless marked as inferred.

| ID | Finding | Size | 1.0 |
|---|---|---|---|
| I1 | The [greenfield and brownfield guide](../../../../src/docusaurus/docs/getting-started/greenfield-and-brownfield.md) asks the agent to "record it as a Decision and keep it proposed until I accept it," while the Planning Decisions route says "An unaccepted proposal isn't a Decision". Two Vision prompts had the same problem | Small | Fixed 2026-10-08 |
| I2 | Stale contract statements: the [global flags](../../crystallized/documents/cli/contracts/shared/global-flags/_global-flags.md), `references`, and source-filter contracts said they "do not ship yet," Route Init said "there is no `install --route` spelling" although Install now has `--route`, and the syntax contract still called Task 70 unqualified | Small | Fixed 2026-10-08 |
| I3 | The [loading contract](../../crystallized/documents/framework/routing/loading.md) was tagged `LoadNow` and `KeepInMind` as topics | Small | Fixed 2026-10-08 |
| I4 | The Working backlog still says to preserve Task 70's release hold after the release finished | Small | Fixed 2026-10-08 |
| I5 | Task 53 and the development guide use stale figures. The development guide says about 9.6k startup tokens and 3.5k for the CLI Skill, while the same method now gives about 7.3k and 0.9k. Task 53 counted 58 `LoadNow` entries where 27 remain after I3 | Small | Fixed 2026-10-08 |
| I6 | Dogfood startup is about 23k estimated tokens. The CLI Development chain loads through `KeepInMind` for every task (about 4.2k tokens, plus about 5k for each refresh), the orchestration Directive costs about 3.2k, and the Project Control table's alignment padding costs about 750 | Medium | Dogfood only |
| I7 | Task 63 policy: Update replaces edited managed files, `removedFiles` keeps them through an undocumented side effect, and Update's "Previous content: git diff" hint can point away from the recovery bundle that holds the exact previous bytes | Small decision | Decide |
| I8 | Task 48 Extension route scoping is still unfinished and is already selected as a 1.0 requirement | Medium | Blocker |
| I9 | Task 71 records a release whose cancellation did not stop npm and GitHub publication. Test suites have no deadline or live stall evidence, and releases can rebuild instead of reusing exact-commit artifacts | Medium | Recommended blocker |
| I10 | Private Working records are omitted from shared `Entries`, so an agent following only navigation can miss resumption state (inferred) | Small | Recommended |
| I11 | Bind one release receipt to the exact 1.0 commit: six-host qualification, package identity, beta-to-stable upgrade, public install checks, and matching deployed docs | Small plus a run | Gate |
| I12 | No dogfood file uses `applyTo` in real frontmatter, so the feature is untested in daily use | Small | Later |
| I13 | Polish: hide the no-op `--force` from Update help, present one removal decision table, add an entrypoint starter Template, trim the ten-minute guide, and rebase the bodies of Tasks 36, 37, and 40 onto current outcomes | Small each | Later |
| I14 | Managed and Native AOT qualification reports are validated independently, so a partial test discovery could pass (inferred) | Small to medium | Later |

## Assumptions And Limits

- No investigation built, tested, or ran the CLI. Code-change sizes are estimates from reading the source.
- Token figures divide bytes by four. They are not tokenizer measurements.
- External tool behavior comes from current primary documentation. Behavior with unknown keys is often undocumented.
- OKF activity counts are a snapshot and will age quickly.
- Collision scenarios for root keys are constructed examples, not observed user incidents.

## Open Decisions

- **Choose the form per workspace instead of one canonical form?** Recommended: yes, as an output-only setting. Every workspace reads both forms.
- **What does a fresh unattended install use?** Recommended: root keys. Interactive installs ask, with root preselected. A missing setting in an existing workspace means scoped.
- **When a block exists beside root keys, which wins?** Recommended: the block is the complete Open Forge set, with no field-by-field merging. `applyTo` keeps its equivalence rule.
- **Should Update rewrite user-authored files when the setting changes?** Recommended: no. Rewrite owned files only, and offer explicit conversion later.
- **Replace loading tags with a `load` field?** Recommended: not for 1.0. Retag the loading contract instead.
- **Invest in OKF now?** Recommended: no. Revisit when a consumer exists or the specification publishes versioned releases.
