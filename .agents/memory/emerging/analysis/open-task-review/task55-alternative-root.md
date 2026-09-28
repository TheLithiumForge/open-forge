---
open-forge:
  description: Review of Task 55 Alternative root such as .apm, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation
  tags: [Memory, Analysis, TaskReview, Contextual, Candidate]
---

# Task 55 Alternative Root Review

## Question

Is Task 55 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 55](../../../archived/cli-development/tasks/task55-alternative-root.md)

## Current Conclusion

**Recommendation:** Fold into Task 62.

**Size:** Small, because the evidence already points one way and what remains is a decision record plus one coexistence check.

Keep `.agents/` as the only root, record that decision before 1.0, and move the APM interoperability questions into [Task 62](../../../working/cli-development/tasks/task62-glob-scoped-loading.md). APM is a packaging and deployment tool for a few primitive types. It isn't a place to host routed context. Its compile step inlines instructions into always-loaded files, which works against Context By Relevance. APM also deploys Skills into `.agents/skills/` for most harnesses, so `.agents/` is already the folder APM treats as shared.

## What It Implies

A configurable root would touch every layer. Workspace paths, the source ID grammar, the lock and settings paths, the Extension package layout, installed `AGENTS.md` and `CLAUDE.md`, and the documentation would all change. Existing workspaces and Extension packages would need a migration.

Folding the Task changes nothing for users now. APM interoperability then comes through Task 62: accepting root-level `description` and `applyTo` as input, and a later export that writes small files APM or a harness can read.

## State Today

The record is accurate: nothing has started. Verified in the repository:

- The CLI hard-codes the root in at least three constants: `WorkspaceOwnershipDefinitions.DirectoryName`, `WorkspaceSettingsDefinitions.DirectoryName`, and `SourceLogicalPath.AgentsRoot`. Literal `.agents/` prefixes also appear in `SourceIdentity`, `SourceReferenceParser`, `SourceLoaderDestinationParser`, and `EmbeddedFrameworkSourceProjector`.
- `.agents` appears in 16 Framework, 61 Operations, 40 OutputText, and 17 Rendering files. It also appears in 2,663 test files, mostly snapshots and fixtures, and in 103 site pages.
- Every Extension package ships its files under `content/.agents/`.
- This repository's own APM use is small. `apm.yml` targets `opencode` and `codex`, and `.apm/` holds only agent definitions.

APM facts, read from its official documentation on 2026-09-28:

- A package is `apm.yml` plus `.apm/` with `skills`, `prompts`, `instructions`, `agents`, and `hooks` folders.
- Instructions require `description` and `applyTo`. Nothing documents other folders being deployed.
- `apm install` writes per-file rules for Copilot, Claude, Cursor, Windsurf, and Kiro. `apm compile` reads only instructions and inlines their bodies into `AGENTS.md`, `CLAUDE.md`, or `GEMINI.md`. The docs mention no on-demand loading.
- Skills deploy to `.agents/skills/<name>/SKILL.md` for Copilot, Cursor, Codex, OpenCode, Gemini, and Windsurf. Antigravity instructions deploy to `.agents/rules/`.

The category mapping is short:

| Open Forge                                       | APM counterpart                                                     |
| ------------------------------------------------ | ------------------------------------------------------------------- |
| Skills and workflow recipes                      | `skills`, directly                                                  |
| Directives                                       | `instructions`, but each needs `applyTo`, and compile flattens them |
| Templates                                        | Skill `assets`, partly                                              |
| Guidance, Patterns, Maps, Memory states, routing | None                                                                |

## Dependencies

- **Blocked by:** nothing.
- **Blocks:** the 1.0 layout freeze. The task index asks for this decision before 1.0.
- **Overlaps with:** Task 62, which already covers root-level keys, `applyTo`, and pointer files. Task 46 reads native `SKILL.md` frontmatter. Tasks 53 and 54 edit the same frontmatter.

## Remaining Work

1. The maintainer decides to keep `.agents/` as the only root, recorded as a Framework Decision.
2. Move two questions into Task 62: root-level `description` and `applyTo` as accepted input, which Task 62 already has, and an optional export of `.apm/instructions/*.instructions.md` pointer files for glob-scoped routes.
3. In a scratch workspace, install one APM Skill package into an Open Forge workspace. Confirm `open-forge index` lists the deployed Skill, and that `doctor` treats APM's files under `.agents/` sensibly. This is the only prototype still worth running.
4. Close Task 55 with a link to the decision.

## Pros And Cons

| Pros                                                               | Cons                                                                |
| ------------------------------------------------------------------ | ------------------------------------------------------------------- |
| Avoids a breaking root change before 1.0                           | Users who want `.apm/` as their single source of truth don't get it |
| Keeps on-demand loading intact                                     | APM features such as its marketplace don't apply to routed Memory   |
| Reuses the `.agents/skills/` convention APM already targets        | Interoperability depends on Task 62 being accepted                  |
| Removes a duplicate investigation of the same frontmatter question |                                                                     |

## Risks And Open Questions

- Two managers could write under `.agents/`. APM writes `.agents/skills/` and `.agents/rules/`, and Open Forge's lock tracks its own files there. Name collisions and `doctor` findings are unverified.
- APM moved to `github.com/microsoft/apm` and changes quickly. A later release could add on-demand loading or arbitrary folders.
- Open question: should Open Forge publish an APM package, such as the `open-forge-cli` Skill? That adds a distribution channel without a root change.

## Next Check

**Action:** ask the maintainer to accept "keep `.agents/`, fold APM interoperability into Task 62".

**Would change the conclusion:** APM documenting on-demand loading or deployment of arbitrary routed folders, or repeated user requests to author Open Forge content inside `.apm/`.

**Acceptance needed:** the maintainer, through a recorded Framework Decision.
