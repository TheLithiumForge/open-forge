---
open-forge:
  description: Recorded category wording proposals and the inspected source changes for Task 28
  tags: [Memory, Working, Contextual, Framework, Review]
---

# Category Wording Review

This is an earlier review snapshot retained for the active Task 28 Git review. Its status and paths describe the recorded stage. Use [Task 28](../../cli-development/tasks/source-framework-review.md) for current decisions, completion, and remaining work.

Baseline: `28cac0fc47e43671aa6458d318293594542d9bbf` on `codex/task28-framework-review`. All 21 public category openings were read: 19 Framework categories and two packaged Template categories. Their full file hashes still match the previous source-pack review. The loader is not a category.

The questions describe what each category helps answer. The opening should explain that role directly; it must not turn the category entrypoint into the source of every answer beneath it. Nine wording refinements are proposed below. Eleven other openings already align. Memory has a separate positive growth explanation.

## Proposed Opening Refinements

### src/open-forge/.agents/directives/_directives.md

Current question at line 9: What behavior is required in this scope?

Current opening at line 11:

> Directives contain required instructions.

Proposed:

```md
## What behavior is required in this scope?

Directives state the instructions that must be followed within their scope.
```

Make required behavior and its scope explicit.

### src/open-forge/.agents/guidance/_guidance.md

Current question at line 9: What approach is recommended, and when does it fit?

Current opening at line 11:

> Guidance is advice that can be adapted to the current situation.

Proposed:

```md
## What approach is recommended, and when does it fit?

Guidance recommends approaches for recurring situations and explains when they fit. Its advice can be adapted to the current situation.
```

The answer now covers both the recommended approach and when it fits; adaptability remains explicit.

### src/open-forge/.agents/maps/_maps.md

Current question at line 9: Where is a useful local or external source, and when should it be used?

Current opening at line 11:

> Routes under Maps point to important local and external sources. The destination still defines its own detail.

Proposed:

```md
## Where is a useful local or external source, and when should it be used?

Routes under Maps point to important local and external sources and explain when they matter. Each destination still defines its own detail.
```

Match the question's selection purpose while preserving destination authority.

### src/open-forge/.agents/memory/crystallized/decisions/_decisions.md

Current question at line 9: What was chosen, why, and what follows from the choice?

Current opening at line 11:

> Decisions record important accepted choices and why they were made.

Proposed:

```md
## What was chosen, why, and what follows from the choice?

Decisions record important accepted choices and why they were made. They may also preserve consequences and other context when these help future work.
```

Consequences remain conditional, as required by the existing Axioms; the question does not mandate them in every Decision.

### src/open-forge/.agents/memory/emerging/analysis/_analysis.md

Current question at line 9: What does the available evidence support, and what remains uncertain?

Current opening at line 11:

> Analysis keeps structured reasoning, investigation, or comparison that is useful but not accepted yet.

Proposed:

```md
## What does the available evidence support, and what remains uncertain?

Analysis keeps useful structured reasoning, investigation, or comparison that is not accepted yet. It makes the evidence, assumptions, limits, and current conclusion clear.
```

Expose the evidence/conclusion relationship without confusing validity, certainty, and acceptance.

### src/open-forge/.agents/patterns/_patterns.md

Current question at line 9: What reusable shape makes related work easy to create and inspect?

Current opening at line 11:

> Patterns define reusable default shapes that keep related work consistent and easy to review.

Proposed:

```md
## What reusable shape makes related work easy to create and inspect?

Patterns define reusable default shapes that make related work consistent, easy to create, and easy to inspect.
```

Use the question's creation and inspection terms; keep default shape and consistency.

### src/open-forge/.agents/memory/emerging/_emerging.md

Current question at line 9: What useful material remains unsettled?

Current opening at line 11:

> Emerging Memory keeps useful material that is not accepted yet.

Proposed:

```md
## What is useful but still unsettled?

Emerging Memory keeps useful findings, possibilities, and reasoning that are not accepted yet.
```

Replace the generic noun in the question and show representative kinds of unsettled content without introducing a required pipeline or changing routes.

### src/extensions/development-toolkit/content/.agents/templates/documents/_documents.md

Current question at line 9: What starting structure fits this current document?

Current opening at line 11:

> Document Templates help create clear current documents without imposing a fixed document set or schema.

Proposed:

```md
## What starting structure fits this document?

Document Templates provide starting structures for clear current documents. They do not impose a fixed document set or schema.
```

Explain starting structure directly; retain current-document scope and the absence of a fixed set or schema.

### src/extensions/development-toolkit/content/.agents/templates/memory/_memory.md

Current question at line 9: What starting structure fits this record's question and current role?

Current opening at line 11:

> Memory Templates help create records in the route that matches their current role.

Proposed:

```md
## What starting structure fits this record's question and current role?

Memory Templates provide starting structures suited to a record's question and current role. They help create the record in the route that matches that role.
```

Answer structure selection before explaining placement; preserve destination role.

## Memory Growth

The defined mechanism is coherent: people and agents preserve useful records, grow routed scopes, and keep unselected branches outside active context. The broad sentence “This growth is deliberate and does not happen automatically” obscures that mechanism and can sound as though useful Memory requires a separate prompt or approval.

The primary has now authorized using “evolves with the work” while retaining `self-growing` in a useful positive explanation. This preserves the earlier accepted descriptor and avoids a terminology rename, new tag, changed capture threshold, or automatic-ingestion promise.

Proposed opening for `src/open-forge/.agents/memory/_memory.md:9` and its local counterpart:

```md
## What is worth remembering for current or future work?

Memory evolves with the work. It holds Markdown records for active work, coordination, accepted knowledge, candidates, and history.

Its self-growing structure lets people and agents deliberately add useful records and routed scopes without a fixed structural limit. Unrelated branches stay outside the active context.
```

The existing capture and integration rules remain decisive. Every conversation is not saved, evidence does not establish acceptance by itself, and accepted records do not acquire another category's behavioral role. No fixed structural cap is introduced.

The defining explanations to keep aligned are:

- `.agents/memory/crystallized/documents/framework/memory/model.md:12`
- `.agents/memory/crystallized/documents/framework/memory/model.md:91`
- `.agents/memory/crystallized/documents/maintenance/payload/agents/memory/_memory.md:19`
- `.agents/memory/crystallized/documents/maintenance/helpers/dictionary.md:92`

The Writing Standard at `maintenance/writing.md:159` and the Writing Decision at `decisions/framework/user-facing-writing.md:19` already require retaining `self-growing`; the proposed opening satisfies them. Loader tag meanings, tags, generated labels, and the four state names need no change for this wording correction.

## Openings Retained After Review

| Source | Why it already fits |
| --- | --- |
| `src/open-forge/.agents/memory/archived/_archived.md` | Useful history and the absence of current governing authority are explicit. |
| `src/open-forge/.agents/memory/crystallized/_crystallized.md` | Acceptance, current state, and scope are explicit. |
| `src/open-forge/.agents/memory/crystallized/documents/_documents.md` | The answer describes one coherent explanation of accepted current knowledge. |
| `src/open-forge/.agents/memory/emerging/ideas/_ideas.md` | Possibilities, experiments, questions, and later exploration match the question. |
| `src/open-forge/.agents/memory/emerging/observations/_observations.md` | Concrete observations and their possible later value match the question. |
| `src/open-forge/.agents/memory/working/_working.md` | Temporary continuity and resumption match the question. |
| `src/open-forge/.agents/memory/working/checkpoints/_checkpoints.md` | Current state and next steps match the question; update and authority limits remain explicit. |
| `src/open-forge/.agents/memory/working/handoffs/_handoffs.md` | The sealed snapshot preserves the named transfer or resumption boundary. |
| `src/open-forge/.agents/skills/_skills.md` | The answer names specialized capability and the native SKILL.md boundary. |
| `src/open-forge/.agents/templates/_templates.md` | The answer covers copying, adaptation, and independent maintenance. |
| `src/open-forge/.agents/workflows/_workflows.md` | An optional repeatable recipe answers the method/goal question. |

## Wording About People And Readability

The shipped `src/open-forge` and `src/extensions` prose contains no case-insensitive `human` occurrence. The defining Maintenance sources contain three:

| Source | Current wording | Proposed wording |
| --- | --- | --- |
| `.agents/memory/crystallized/documents/maintenance/helpers/dictionary.md:47` | `Human accountability for work or a decision.` | `A person's accountability for work or a decision.` |
| `.agents/memory/crystallized/documents/maintenance/payload/_payload.md:18` | `final human-readable and agent-facing product` | `final readable and agent-facing product` |
| `.agents/memory/crystallized/documents/maintenance/payload/agents/templates.md:26` | `Templates remain human-readable and usable through ordinary file operations.` | `Templates remain readable and usable through ordinary file operations.` |

The Payload passage is inside an Axiom, so it is identified separately from the opening edits. The original person-accountability distinction in the Dictionary must remain; simply deleting `Human` would broaden that meaning.

The inspected Principles also contain `human-readable files` at `.agents/memory/crystallized/documents/principles.md:19`. `Readable files that users own` preserves the intended property and ownership. README and docs remain with the primary; this is not a claim that the whole repository has no further occurrences.

## More Specific Alternatives To Material

Choose the noun from what the sentence refers to. `Records` means saved items; `information` fits facts or statements; `content` includes files and extracted portions; `findings`, `possibilities`, or `reasoning` can make a particular result concrete. Use `important` or `significant` when `material` is an adjective about consequence. Do not turn this into one global replacement: `knowledge` could imply acceptance, and `records` could exclude useful information that has not been saved yet.

| Existing location | Current phrase | Candidate wording |
| --- | --- | --- |
| `src/open-forge/.agents/memory/_memory.md:34` | `whether material was stated, observed, or inferred` | `whether information was stated, observed, or inferred` |
| `src/open-forge/.agents/memory/_memory.md:46` | `Move material when its state, scope, or intended use changes.` | `Move content when its state, scope, or intended use changes.` |
| `src/open-forge/.agents/memory/working/_working.md:18` | `Saving material here does not make it accepted.` | `Saving information here does not make it accepted.` |
| `src/open-forge/.agents/memory/archived/_archived.md:17` | `the material's origin` | `the archived content's origin` |
| `src/open-forge/.agents/loader.md:28` | `Selected non-binding material` / `broader material of the same kind` | `Selected non-binding content` / `broader content of the same kind` |
| `src/open-forge/.agents/loader.md:78` | `Material that must stay aligned with accepted current state.` | `Content that must stay aligned with accepted current state.` |
| `src/extensions/development-toolkit/content/.agents/workflows/development.md:15` | `a material decision` | `an important decision` |
| `src/extensions/development-toolkit/content/.agents/workflows/review.md:20` | `each material finding` | `each significant finding` |

These are concrete discussion candidates, not authorization to rewrite unrelated Axioms or loader terms. The Emerging question and opening above resolve the most visible generic use without changing its tags or category set.

## Application Boundary

This report was prepared before source application. The primary subsequently authorized the opening improvements, the positive Memory explanation, corresponding dogfood prose, and current Extension README wording. Separately named loader/inheritance corrections LI-001–LI-004 have their own bounded authorization. Local Workflow specialization under LI-005 and the mechanical execution escape hatch remain pending; no broad parity or tag change follows from this wording review.

## Authorized Application

The review above is a historical snapshot taken before application against `28cac0fc47e43671aa6458d318293594542d9bbf`. Its statement that all 21 opening files matched the earlier reviewed bytes remains true for that review start. The primary subsequently authorized the changes below, including LI-005 after the initial application boundary was written.

The full-context Astra/max source author applied this 35-path pack in `codex/task28-framework-review`. Verification finished at 2026-09-12 00:28:59 UTC. The primary owns task bookkeeping, the remaining README/docs work, review, and Git operations.

### Applied Meaning

- Applied the nine proposed Q/A refinements and the positive Memory definition, with the matching local category prose. Memory now “evolves with the work”; its self-growing structure explicitly supports deliberate useful records and routed scopes without a structural ceiling. The recorded-state threshold remains in force, and the defining model still excludes automatic ingestion of raw activity. No automatic-growth promise was added.
- LI-001 and LI-002 remove only Memory's repetitions of universal authority and reserved classification-tag meanings. Its evidence, acceptance, provenance, state-transition, and outcome rules remain intact.
- LI-003 removes only the generic Directive child-route selection bullet. The unique duty to report conflicts or instructions that cannot be followed remains intact.
- LI-004 replaces the loader's reference to “defined thresholds” with self-contained eligibility criteria for #LoadNow and #KeepInMind. Both retain “only when”; all defined-tag meanings, refresh times, ancestor/scope boundaries, and the rest of the loader are unchanged.
- Replaced the three scoped uses of “human” in the Dictionary and Maintenance prose while preserving personal accountability and readability. The Writing Standard now asks the opening to answer its question and names people, readers, plain text, and readable text when those relationships or properties are intended.
- Refreshed the three Extension READMEs with current package facts, distinct routed versus native/support-file interpretation, current installation/authoring guide links, manual completeness, explicit source/destination examples, ownership boundaries, and qualified evidence claims. Removed historical executable/snapshot status and local-candidate narratives. The exact manifest representation is delegated to the package-format guide.
- Generic “material” alternatives elsewhere remain discussion candidates. No blanket terminology substitution or tag edit was applied.

The later request for a distinct introductory voice for READMEs is being discussed by the primary. These factual README corrections are stable; they are not approval of a new promotional-writing convention.

### LI-005: Accepted Local Workflow Alignment

The user selected “Align the shared text; preserve the local rule in an overwrite.” The local `.agents/workflows/_workflows.md` now has the exact authored prefix of the unchanged public Workflow category, followed by its unchanged local generated Entries.

The adjacent `.agents/workflows/_workflows.overwrite.md` preserves only the accepted condition:

> Change a Workflow only when evidence shows that its risk profile no longer fits.

The overwrite has a visible local heading and the inherited Axioms/Selection And Use context. It has no frontmatter, tags, generated region, or independent entry. Workflow Maintenance identifies the shared authored contract and this local companion.

Alignment also restores the source's candidate-selection criteria and post-load Goal confirmation, permits recipe-specific headings, uses a significant change to the interaction as the announcement trigger, makes internal route detail optional, and restores the “deliberate user method” / “normal agent behavior” creation wording. These differences were identified before applying the explicitly accepted shared-text alignment; they were not silently treated as byte-only drift. No separate Workflow design or additional local condition was added.

### Verification And Limits

- All 35 final files match the intended bytes. The 34 existing files' before bytes match the declared Git baseline; the overwrite was absent at that baseline.
- All 51 public Framework/Extension files were checked against the baseline: 14 changed and 37 remain exact matches. All 21 category openings were reviewed; 10 changed and 11 remain exact matches. All public category Axioms are unchanged except the exact LI-001, LI-002, and LI-003 deletions.
- Every existing owned file preserves its complete frontmatter and generated Entries. Non-README fenced examples and heading levels/counts are unchanged. README heading changes are confined to removal of obsolete candidate/status framing and the current evidence section.
- Twelve source/local authored prefixes match exactly: the ten updated categories, loader, and Workflow category. Local generated Entries remain unchanged; unrelated local Checkpoint/Handoff content and specialized Workflow recipes were not replaced.
- Both loaders contain 79 non-empty authored lines and 87 non-empty total lines. Only the scoped-entrypoint threshold line changed.
- All six Extension README command examples name `--source /path/to/open-forge/src/extensions --workspace /path/to/project`; those example source and target paths are separate. The READMEs make no embedded-catalogue synchronization claim.
- The source tree and the inspected defining Maintenance/Memory sources contain no case-insensitive “human” occurrence.
- Checked 192 relative link destinations across the owned files; none is missing. All 9 added links resolve, including the new overwrite and the installation, manual-installation, authoring, and package-format fragments. Existing CLI destinations were checked for existence only; their bodies and fragments were not inspected. Existing unchanged fragments were not broadly re-audited.
- `git diff --check` passed. Verification used static text, hash, path, and heading checks. No CLI command, build, test, runtime, or agent-outcome verification was performed, and no such proof is claimed.
- Stored Working, Emerging, and Archived Memory records were not scrubbed. The changed Emerging entrypoints define the current category and remain within the accepted source boundary. Root-owned edits outside this manifest are excluded from these application claims.
- No commits, merges, dispatch, external actions, APM/runtime-agent changes, package schemas, or machine-field changes were made.

### Applied Path Identities

A dash marks the newly added overwrite; all other before hashes are from the declared baseline.

| Path | Before SHA-256 | Applied SHA-256 |
| --- | --- | --- |
| `.agents/directives/_directives.md` | `abac9fc8ffeb1ba198d28e9fabdce0e45e673c271222d6ef0903703422485d15` | `a07224435658e46f1044edb8107ec07445c7dbc5d99c24e3681c15302ea6b93a` |
| `.agents/guidance/_guidance.md` | `871915ae8f365544c36e664ea4836f1ffae5adc0b437661e786972416038eb8d` | `b18d72dfcd387a24ceb86e22ac865ec7df54a1262f09eaed2583ce19adaf2295` |
| `.agents/loader.md` | `e0c139e864b0553ff6ccea1d1ca66da2e66def5446f560065c439341c25b97ac` | `03f4492b30c8d77130290e314654cde70e39b6bae5fd682514486d544bdd53e5` |
| `.agents/maps/_maps.md` | `c95533a03143c7df1f7cf8b777200d704cf2011063e5def72a3834b3ee35164e` | `1f802db017fba895410ac0156615ab02ffe44ca6c3c57653e0106c4212ff49b6` |
| `.agents/memory/_memory.md` | `7896957b9bd1a582f26eac3d44b1ff599437d69f5569359b6dda5a195e4f63f7` | `6948ed9e427e7762c666df8c7270be0c20ad483dab095a844d6f89dc1fcd3a61` |
| `.agents/memory/crystallized/decisions/_decisions.md` | `f5a60767e14ed86e7a1ed91617e5b55a707f5b0db213520b98f502db9d7ab2b7` | `225013d43dda3914848997c71cd6e012ce284283a28fa0d7d695f5666b3ac00f` |
| `.agents/memory/crystallized/documents/framework/memory/model.md` | `b485d55bdcba34ac4c600742fb976d19ca443405788ba7a6403957f0d8d6ccd0` | `d17686f4fc2e5fd417624b38de8d2fe01726bb7ada92dd68a0b9fcfef5cacfb4` |
| `.agents/memory/crystallized/documents/maintenance/helpers/dictionary.md` | `7ddbf750825553ada22e3702933329874b954d5a5258e9f033f7327cdac1e015` | `d2c8f398331f1acaed78a08cd7d71bc9ea2b7ae124c899814d9d6c62a3290048` |
| `.agents/memory/crystallized/documents/maintenance/payload/_payload.md` | `d7f88bf1d72047d75b88c5ce4235ea574e1529f5c68ebe24decb73f2b1e613bf` | `fb8d37825d5638b9179e9e35c3485b00a1eadf4fa7c32f5cbe078e0561f6545a` |
| `.agents/memory/crystallized/documents/maintenance/payload/agents/loader.md` | `d6320c2fee7db27677bfdbef03d940a8570b9987dc095fa6af8314078f6090f7` | `fc5f9f3a7ead556a3372feb25a12b80370bda14f5774b1f4b9061ce3ebf04433` |
| `.agents/memory/crystallized/documents/maintenance/payload/agents/memory/_memory.md` | `bc21e50bfa9d1ce8f2e2e6f7e76b723ac9786de6fb78c62e576de111affc609c` | `cfe9fca53c3e89d13545b8bbdeb4298d8449b8d12763a84f40db8ecef005c4f7` |
| `.agents/memory/crystallized/documents/maintenance/payload/agents/templates.md` | `75760270ef4f911b930df5ca5de9e4d1f499f71ef004195482d6298a0cc6d650` | `cb2a1294a613d4b4df3dfa20286ae32ce852b8068aada7bf64ae9c984667035c` |
| `.agents/memory/crystallized/documents/maintenance/payload/agents/workflows.md` | `b722df97c5b5f124a73c560b2309607e389fe5f9540b998e7022fdc558d99ff0` | `9e927efab26c38961934f365effd9df22da003fb3c6bcb3339ee33520a160194` |
| `.agents/memory/crystallized/documents/maintenance/writing.md` | `523cbdb296a9b4d1c4685656c1c87100b5211b2cc5205c8748319b32c646228d` | `6e5fdb4753551584a4d3b4f6bf77c028d0051ca2f20c8ad26e06f8ff05b32fcf` |
| `.agents/memory/emerging/_emerging.md` | `d68419efbe6b16d3c9c3c6658062fb60db7c7fb5ae0244ec2d555cb46e58c44f` | `d9baf7ce528beeb30031391a05ca593f670b55d4cd19db02a1ebccbaf0ef5d8d` |
| `.agents/memory/emerging/analysis/_analysis.md` | `fb15ccd0987c4bfd1ae9aa66d317fa6a02d85f3d81722836ccaa1a385289eeca` | `8d67951dfa11c1ba465bef54903021a42c19b62fe69efcf000441d6cb3c020f1` |
| `.agents/patterns/_patterns.md` | `5aa680790088670d5e962034dae110a4aba2cc017ec5e46989678dcfa8677d0a` | `3ef91cf2365b99e315a966c2af545fb154a0647d42f9e083bc34da1b955cb146` |
| `.agents/templates/documents/_documents.md` | `61238c203aaddc7e7b8997cdea899755f7733c66ea323dea058bd12ab9137c1a` | `f357abbd3b1994c76acfc2660d83f225ebb7bc048b5980c8fb24ea6532cf6544` |
| `.agents/templates/memory/_memory.md` | `3413a92aa4cdde34c610184a71ef2e2c8c6afb2136e171bd0101b8da544dd06f` | `eb3c6741c412ad9c5bc4c95057c49b78bde1d360f3f48d6d42eba891cd9e2aa6` |
| `.agents/workflows/_workflows.md` | `7cc5675c50395a167964edc9a7318220a74c850b1be1183adc6dfd86a7da04c8` | `596c3b5d874dfa3683239db331f43c6f4f10f310cf5e24794099954e2635ee1c` |
| `.agents/workflows/_workflows.overwrite.md` | — | `6f90f7a5012d34437dd32458ee690e2930311793f6a5f4273ff56bf539b390b8` |
| `src/extensions/development-toolkit/content/.agents/templates/documents/_documents.md` | `18cbac26d62781f192ca15e15b0641b4874755ff624cb810d05ac48ee22c705b` | `4be77baca59a97abd75a2363d24f9c29b476ad4ffde1cb66304c4fd5f67a2299` |
| `src/extensions/development-toolkit/content/.agents/templates/memory/_memory.md` | `fdff3c4d60b807b4a0b52712d914afd02585bd577ea049edd57659d1ee0c549b` | `152d43c5cdc9a371f321f0b5f383af121b2738ab372f0b2cddb9f24488a7324c` |
| `src/extensions/development-toolkit/README.md` | `36ca11023dd392f03aa20166e2391cfdadc512194c0068669ced038558a7777e` | `ad012d3d2724f62b5d25948532bfbb254dbbaf8a6fde16a9b3c4a80d2867deb3` |
| `src/extensions/orchestration/README.md` | `798963d46b89b31dc7d0de547a708ac964fd55bf94c04bccb46f2ceb36ab5b47` | `1d4a7a64913fe3a02e5e64a2fd92508e0b94ed2e457dfa511124d691889ee1bd` |
| `src/extensions/README.md` | `ccd6587b61bd19f508e6749d2e287e4a42cd0ebf5151a3cb35cf125c62563fe7` | `706a068eb3abc6282d35ea795e0524e792c53718cb161abf17dc98fc9151a7ba` |
| `src/open-forge/.agents/directives/_directives.md` | `cc088b1d369d4f003e5ba5bfaee1d19e525121de0e64ed31762bd13d234e5175` | `8e039f8f76c745d7328f8fa38d3b03b9e19c40895e112ee0c7c845d6db9d71bf` |
| `src/open-forge/.agents/guidance/_guidance.md` | `1dc5388c1919eedeb5f29969fc022b0be0e00a442452f66ad9c63c9933c9f332` | `a23c7a09fcbcd9b1758ec03542b89bd5eee82c6e9c89e4d2e40a859230b019e2` |
| `src/open-forge/.agents/loader.md` | `15b2fa7474d89bca16500135be8c5ae72d4d8f3f4076cadfb8cc7ccbea4eeabb` | `860cef29219f09acd2412d7b9e90af7e89a44607826392df06d41b9957af077a` |
| `src/open-forge/.agents/maps/_maps.md` | `33a6a01c6cb1e16b5dd871345bbc3fa77ffd2d1a0bbae5445c7921a6614fb921` | `b87c74644e85ae5b7f28289ff2976cd22b4cd9030ccb26d4ee26bfc8da11c627` |
| `src/open-forge/.agents/memory/_memory.md` | `d14d95e2c66bc30ef5bf7e902dbedf8f7d5527d40bf959d3947419bddafeb80a` | `6caed505ebdd02eabe648a8477dbe247f4db19c97c39620d5bca48fd52cf4619` |
| `src/open-forge/.agents/memory/crystallized/decisions/_decisions.md` | `fa00eb675d492baec84150356fea4784e047abd6822eaec32d5c9825bbcf0156` | `360f361a8819181572a3aff5fc3fa28eaa83368addde0de11609dbd65165ac7d` |
| `src/open-forge/.agents/memory/emerging/_emerging.md` | `6c56db884f4cb1b6c3a713f177945aa65ad7569bbf5c92ae4b5f5f078ef64861` | `a193fd2f3b476fadd0a92c1b87e9ecf3824c89dbf4381b2fba2270cb8ceaf447` |
| `src/open-forge/.agents/memory/emerging/analysis/_analysis.md` | `79d79a64a7533ba72eb37f44d2727a79e3db430d9e82e72ec82e36a3dfba7f6f` | `00c82389f662a4850ecf4b6999c8bed3b3b8f30b6b1236a2069f4b0026cee886` |
| `src/open-forge/.agents/patterns/_patterns.md` | `1de75723e1b41b756f5b64921ec762ce49e795e33eab2f437ccba68050bc8f61` | `3bb0c238db3b194950989df5bef6664853c267b819c2e3de31ab99ed564d67d7` |

### Source And Local Correspondence

| Public source | Local counterpart | Result |
| --- | --- | --- |
| `src/open-forge/.agents/directives/_directives.md` | `.agents/directives/_directives.md` | Exact authored-prefix parity; local Entries preserved |
| `src/open-forge/.agents/guidance/_guidance.md` | `.agents/guidance/_guidance.md` | Exact authored-prefix parity; local Entries preserved |
| `src/open-forge/.agents/maps/_maps.md` | `.agents/maps/_maps.md` | Exact authored-prefix parity; local Entries preserved |
| `src/open-forge/.agents/memory/crystallized/decisions/_decisions.md` | `.agents/memory/crystallized/decisions/_decisions.md` | Exact authored-prefix parity; local Entries preserved |
| `src/open-forge/.agents/memory/emerging/analysis/_analysis.md` | `.agents/memory/emerging/analysis/_analysis.md` | Exact authored-prefix parity; local Entries preserved |
| `src/open-forge/.agents/patterns/_patterns.md` | `.agents/patterns/_patterns.md` | Exact authored-prefix parity; local Entries preserved |
| `src/open-forge/.agents/memory/emerging/_emerging.md` | `.agents/memory/emerging/_emerging.md` | Exact authored-prefix parity; local Entries preserved |
| `src/extensions/development-toolkit/content/.agents/templates/documents/_documents.md` | `.agents/templates/documents/_documents.md` | Exact authored-prefix parity; local Entries preserved |
| `src/extensions/development-toolkit/content/.agents/templates/memory/_memory.md` | `.agents/templates/memory/_memory.md` | Exact authored-prefix parity; local Entries preserved |
| `src/open-forge/.agents/memory/_memory.md` | `.agents/memory/_memory.md` | Exact authored-prefix parity; local Entries preserved |
| `src/open-forge/.agents/loader.md` | `.agents/loader.md` | Exact authored-prefix parity; local Entries preserved |
| `src/open-forge/.agents/workflows/_workflows.md` | `.agents/workflows/_workflows.md` | Exact authored-prefix parity; local Entries preserved |

The public source files and these local counterparts are frozen at the identities above for the primary's focused review. Any later accepted edit needs a fresh identity for its affected path.
