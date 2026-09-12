---
open-forge:
  description: Record the bounded C# beta changes, exact qualification, deferred findings and local integration
  tags: [Memory, Working, Contextual, CLI, CSharp, Refactoring, Beta, Closeout]
---

# C# Beta Closeout

## Current State

Task 27's bounded beta batch is complete at phase 4/4, milestone 8/8.
Implementation, evidence accounting, independent reviews, local qualification
and the authorized local develop squash are complete.
The accepted executable/CI candidate is `31f9340f6af51effddb8b48f59454e21f28db364`;
its C# runtime was built from `7f0a9112beff7b66b7df8e2eaba79fd47ca609bc`.
Both descend from local develop `2b54fdc598a48a12e44772a5cb323cf45e9e5a73`.
Later changes are limited to a CI test fixture and documentation; no C# or build
input changed after qualification.

The user deferred wider refactoring to a later release. This record accounts for
all 153 assessed findings and the additional final-review observations. It does
not claim the code is completely duplicate-free or that every possible input
has been tested. [Task 27](csharp-structural-streamlining.md) owns execution and
[the comprehensive preflight](csharp-comprehensive-preflight.md) preserves the
original analysis and accepted boundaries. Task 28's independent source updates
and notes remain protected.

## Local Integration

Accepted feature `1b101bf224f6c7c64b6dac3101e758fce9ce1c8b` was squash-integrated into local develop as
`73ef066a8a7f2b4944af49e299608abf1dc541e0` — “Streamlined the CLI and corrected beta behavior”.
The integration parent is `2b54fdc598a48a12e44772a5cb323cf45e9e5a73`. Both feature and integration have
exact tree `43407622824112eb305f0e9903bff0d87dabd7c3`; all 3,335 tracked paths match.
The integration receipt is `artifacts/task27-beta-final/integration-receipt.json`.
A separate documentation-only completion commit records this identity and final
Task state. The feature history, earlier failed evidence, ignored drafts and
other worktrees remain preserved. Task 28's independent source work was not
integrated. No remote operation or publication occurred.

## Completed Changes

- Root Update now uses correct UTF-8 coordinates when splicing non-ASCII hosts
  and preserves content outside the installed root/provider managed regions.
- Recovery cleanup checks the original attribution of prepared bundles before
  deleting them. Repair maps atomic failures to the actual affected plan steps.
- References can form its filtered cancellation result without violating its
  own selector invariant. Library workspace-failure output uses parsed subjects.
- Remove and Update plans snapshot accepted mutable input collections and bytes.
  Their unchanged 54-case evidence contains the 27 formerly failing ownership
  cases and 27 ordinary controls. This is an internal ownership guarantee, not
  a claim that a public exploit was reproduced.
- Framework source, document, mutation and package work shares proven identical
  mechanisms, reuses immutable derived facts and removes discarded allocations.
  Models and support capabilities have moved to their actual local owners.
- Shell and command bindings use clearer direct ownership. Root command output,
  help and escaping share only identical behavior. Context uses its existing
  ordered accumulator; Route mutations retain work within its original stage.
- Test support shares matching real workspace/process capabilities. Substantive
  evidence changes have separate Purple commits; actual fixes have separate Red
  evidence and correction commits. Public commands retain three journeys each.
- CI counts now match 3,206 Unit selections, 3,228 Unit executions, 1,718
  Integration cases and 111 public cases. The nine deferred theory mappings are
  unchanged; two source hashes changed only for accepted namespace imports.
- The existing Proportionate Development directive now explicitly grounds code,
  testing and review in ordinary product use and realistic consequences.

## Qualification

All required local gates passed. The qualified Release build has zero warnings
and errors. Exact commands, toolchain, source hashes, recursive runtime snapshots,
raw results and review report hashes are sealed in
`artifacts/task27-beta-final/canonical-qualification.json`.
Its SHA-256 is
`9fb3819caa730a1ff7b8c573d6b366d0262357c72139e2a302771cc13b396292`.

| Execution boundary | Passing executions |
| --- | ---: |
| Managed Unit | 3,228 |
| Managed Integration | 1,718 |
| Managed public E2E | 111 |
| Native Integration | 1,718 |
| Native public E2E | 111 |
| Managed public E2E targeting native CLI | 111 |
| **C# total** | **6,997** |

These are repeated runtime executions, not 6,997 different tests. Discovery
selects 3,206 Unit tests; nine existing deferred theories expand that to 3,228
executions. All cases match independently frozen identities and multiplicities;
there are no failures or skips. The 111 public cases include exactly three
journeys for each of 28 commands, plus 27 process, shell and payload checks.
All 411 recursive runtime snapshots are retained. The qualified native version
is `0.0.0-dev.sha-7f0a9112beff7b66b7df8e2eaba79fd47ca609bc`.

The Linux x64 Native AOT installed-package journey also passed; installed and
input native executable hashes are equal. CI type checks, lint and formatting
passed, with 21 CI Unit, 29 CI Integration, seven package-layout and one preserved
package-fixture checks passing. The stale CI producer fixture was corrected in
`18ed11be`: its seven-case run changed from five pass/two fail to seven pass
by updating three literal count rows, with every assertion preserved.

Required whitespace and warning-level formatting passed. Supplementary
informational formatting returned 619 suggestions, all explicitly deferred in
`artifacts/task27-beta-final/style-deferred-inventory.json`. S04 remains partial;
this is not a style-clean claim. Namespace/folder, protected-path, callable-shape,
prohibited-pattern, managed/public and line-length checks are accounted for.
Of 117 C# lines above the 200-character guideline, 116 retain pre-existing text;
one coherent Install output interpolation remains accepted readability debt.
Three pre-existing production null suppressions remain in deferred Doctor/Route
work. No new production suppression or exceptional dispatch machinery was added.

Toolchain: .NET 10.0.111, Node 24.19.0, npm 11.17.0 and clang 18.1.3 on Linux
x64. Prepared Bun is 1.3.0; the hosted pin 1.3.14 was not installed or executed.
These local checks do not qualify hosted dependency setup.

The beta targets supported x64 platforms. Linux x64 receives actual managed,
Native AOT, public-process and installed-package evidence. Windows/macOS x64
receive static review of the matching configuration and commands; foreign-host
execution is not claimed. The six-RID inventory remains intact. No remote Git,
GitHub workflow, publication or global installation update is authorized.

## Deferred And Retained Boundaries

- Broader Library/Repair/Cleanup, Route and discovery-command simplification is
  deferred, together with other unstarted findings in the inventory below.
- C01 and C09 contain only their completed Context portions. R06 moved eight
  leaf models; two shared document-edit facts remain with deferred R10. S04 has
  completed individual improvements, with remaining suggestions accounted for
  separately rather than a blanket formatting rewrite.
- F13 YAML simplification is deferred. The user rejected expanding the beta for
  unusual YAML spellings. No product question remains pending. A later change
  should consume ordinary parsed library values for the fields the CLI needs.
- U07's shared-reader draft stays paused under its existing preflight restriction.
  It is excluded from the beta and has not been retried.
- Library L03 has an unexecuted 28-path draft in its preserved lane. Route P04's
  unexecuted comparison draft records that edits are flattened across documents
  and must not silently become per-document equality. Discovery's future mapping
  drafts and Extension's remaining inventory are also preserved, not accepted
  implementations. The feature branches remain available for those drafts.
- Generic test cleanup hooks remain an idea, as requested. No Python helper is
  tracked. Historical ignored evidence and unrelated worktrees are preserved.

## Final Review Observations

The extension-method pass found three methods and no beta blocker. Two optional
follow-ups are deferred: remove the unused References result-builder receiver
and make the one-caller Update observation lookup a private recovery method.
Neither needs promotion to a global utility owner.

The requested quick Astra/high workaround scan found three deferred candidates:

| ID | Finding | Later action |
| --- | --- | --- |
| T27-PROP-E1-F1 | Library invalid-result handlers retain raw-operand heuristics in a branch unreachable through normal parser failure handling | Remove the unreachable branch and its artificial direct-call test; do not improve the heuristic |
| T27-PROP-E1-F2 | Some Integration tests construct standalone parser scenarios and freeze upstream conversion/aggregation behavior | Retain compact owned-adapter and command-result coverage; reduce redundant upstream-only matrices |
| T27-PROP-E1-F3 | Some internal fixtures combine several invalid values and freeze which guard throws first | Remove arbitrary validation-precedence expectations; preserve useful state, span and ownership invariants |

These observations establish maintenance opportunities, not beta-blocking public
failures. Keep accepted containment, recovery, immutable snapshots and realistic
malformed-input evidence.

The quick scan inspected `b29c7621`; these candidate bodies remain unchanged.
F1 starts at `Commands/Library/Attach/Shared/Binding/LibraryAttachRequestBinder.cs:80`
and the corresponding Inspect binder at line 67, under `src/cli/core/OpenForge.Cli.Core/`.
The actual parser-failure exit is `Shell/Composition/CliCoreApplication.cs:46`.
F2 starts at `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Parsing/SystemCommandLineBehaviorTests.cs:211`,
with related scenarios at lines 239 and 271. F3 starts at
`Framework/Lifecycle/LifecycleSchemaContractTests.cs:503` and
`Framework/Documents/Markdown/MarkdownDocumentFactsOwnershipTests.cs:140`, under
`src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/`.

The Astra/xhigh contract/test review mapped all 56 command Interface/Behavior
documents and all Integration/public test titles. It read every public test body;
Integration body inspection was a declared sample. It found three stale Extension
Install passages describing targets beneath .agents despite the existing accepted
workspace permissions. Commit `31f9340f` corrects those three passages in two
contract files, preserving exact grants and the Framework anchor prerequisite
(`T27-TEST-D01`, fixed). No implementation changed.

`T27-TEST-T01` remains deferred: several Cleanup test names imply revalidation
of a held plan, but their second command invocation builds a new plan. The
assertions remain useful; actual deletion-session race evidence exists separately.
Rename or classify those cases during later test cleanup.

The Astra/xhigh holistic review passed its declared consequential-change sample
and independently checked 59 lane-to-root patch identities. The grouped
`T27-FULL-C1` recheck passed the complete four-path fixture/Task/prose delta.
No material beta finding remains within those reviewed boundaries. Review reports
are retained under `artifacts/task27-complete-preflight/root/`; the canonical
qualification binds their hashes. These passes do not claim a second exhaustive
read of every production body or Integration test.

The quick workaround scan and reviews supplied useful bounded evidence. No
controlled model-cost or quality comparison was performed, so they do not establish
that one model is generally superior.

## Complete Finding Inventory

The inventory contains 78 completed findings, four retained decisions, four
partially completed findings, 66 deferred findings and one paused finding: 153
in total. The CI fixture and final documentation correction are additional
closeout changes, outside that original inventory.

Completed means an accepted, individually qualified change is present on the
feature branch. Retained means an explicit no-change decision. Partial rows
state the completed portion above; all remaining portions are deferred. Exact
per-slice paths, source hashes, raw failures and runtime identities stay in the
existing Task record and ignored evidence, not duplicated in this table.

### Root Install and Update

| ID | Finding | State | Accepted commits |
| --- | --- | --- | --- |
| U01 | UTF16 managed-block offsets slice UTF8 bytes | Completed | `b42f6f08`, `4eb8b65c` |
| U02 | Normal installed root hosts are treated as whole files by Update | Completed | `b42f6f08`, `4eb8b65c` |
| U03 | Install plan predicate/count reads repeatedly materialize effect arrays | Completed | `7e40388d` |
| U04 | Two composition layers add no behavior or boundary | Completed | `9e365492` |
| U05 | Repeated source projection and repeated Markdown parsing | Completed | `20b58087`, `39fa77dc` |
| U06 | Update repeats canonical-coordinate and lowercase SHA256 predicates | Completed | `2a8703b2`, `3748663d` |
| U07 | Two ordinary-target readers repeat a real physical-read mechanism | Paused; excluded from beta | — |
| U08 | Data-only stage/outcome types are mixed into behavior locations | Completed | `672f8120` |
| U09 | Residual nested conditional and null-forgiving control flow | Completed | `01189c63` |
| U10 | Existing formations are unpacked into long forwarding calls | Completed | `deb9dc8a`, `77d2fa2d` |
| U11 | Output construction has remaining coherent blocks and shared standard text | Completed | `cd53d35c`, `adb991fb`, `ebadd8ef`, `d9553e52` |

### Framework sources and documents

| ID | Finding | State | Accepted commits |
| --- | --- | --- | --- |
| F01 | Reuse the successful Markdown parse when normalization is identity | Completed | `8f2a860c` |
| F02 | Materialize Markdown collections once at an immutable ownership boundary | Completed | `9be2d2c3`, `91069756` |
| F03 | Reuse already retained operational derivations | Completed | `2793e70c` |
| F04 | Give adjacent-overwrite identity and generated empty syntax one owner | Completed | `44ff07b6` |
| F05 | Share only the identical strict escaped-destination decoding mechanism | Completed | `2a337a0b`, `ed14a045` |
| F06 | Make Source link lexical resolution one local stage | Completed | `24147ddd`, `423424bf`, `0bb82faf` |
| F07 | Share operational generated-navigation input formation and classification | Completed | `da09ccd2`, `53562e03` |
| F08 | Remove provably dead processing and unused input | Completed | `657d8580` |
| F09 | Flatten nested conditionals and make coherent string construction direct | Completed | `18728aea` |
| F10 | Relocate the remaining source-local models and regroup crowded model topics | Completed | `db2a5d0e` |
| F11 | Keep genuine data contracts but name ambiguous constructor arguments | Completed | `18728aea` |
| F12 | Explicitly close enum-to-value mappings without changing named behavior | Completed | `836c4058`, `37d4227d`, `dd2544e3`, `19854437` |
| F13 | Preserve native YAML facts instead of rescanning YAML source syntax | Deferred | — |
| F14 | Tell the truth about the established source-candidate parent | Completed | `657d8580` |
| F15 | Loader all-whitespace decoded destination escapes result construction | Completed | `b17ad765`, `fbc2b0d7` |

### Framework mutation and recovery

| ID | Finding | State | Accepted commits |
| --- | --- | --- | --- |
| M01 | share the exact relative-link identity predicate | Completed | `c2c6d3ed` |
| M02 | one lifecycle envelope replacement projection | Completed | `377e61d2` |
| M03 | remove two full-size transient byte arrays | Completed | `01a5ec45`, `21a316df` |
| M04 | collapse long lifecycle validation-field forwarding without weakening construction | Completed | `377e61d2`, `a0f0d17b`, `7e4d0a43` |
| M05 | one lifecycle snapshot path invariant | Completed | `1755de51` |
| M06 | validate UTF-8 without constructing a discarded UTF-16 string | Completed | `4b4cee42`, `876e7b83` |
| M07 | share the file/directory-create receipt state values | Completed | `0eb0bc08` |
| M08 | centralize only the shared exception-to-filesystem-kind mapping | Completed | `2df446b7`, `9cbcd79b` |
| M09 | eliminate the nested recovery comparison conditional | Completed | `91abaf3d`, `ba3a5d69` |
| M10 | reject unknown recovery classification enums explicitly | Completed | `2cc70ab4`, `92753288` |
| M11 | cache immutable lease identity work, never lease liveness | Completed | `0e838c83` |
| M12 | define recovery entry wire mapping once | Completed | `d7faff64`, `4eaf2893` |
| M13 | recovery identity equality belongs to Framework | Completed | `7d88c773`, `76e76196`, `799bb942`, `dc6154b5` |
| M14 | complete the model and supporting-capability placement | Completed | `0ac2b77e`, `76e76196`, `21a316df`, `c2c6d3ed`, `0eb0bc08`, `9528081f` |
| M15 | reuse established source-availability facts in each lifecycle view | Completed | `4eccf835` |

### Framework packages and permissions

| ID | Finding | State | Accepted commits |
| --- | --- | --- | --- |
| P01 | Share permission-file proposal mechanics without sharing approval policy | Completed | `e5dfa1e2`, `2f2c6f09`, `3248b3f1`, `0495a611`, `ba666840` |
| P02 | Express link-target precedence directly | Completed | `4accc8eb` |
| P03 | Finish the concrete model-placement inventory | Completed | `5c5ac7a4`, `5c3caa9b`, `5b519daf` |
| P04 | Pass the bridge candidate and comparison as cohesive construction facts | Completed | `993b2b1e`, `1bd29c01` |
| P05 | Avoid allocating decoded manifest text that is immediately discarded | Completed | `d61569d0`, `5c3caa9b`, `92a8c385` |
| P06 | Reuse admitted Library identities during duplicate-ownership classification | Completed | `ea0ba699`, `c9ad3a7c` |
| P07 | Retain Library mappings during one residual-attribution pass | Completed | `90827feb`, `8d44293b` |
| P08 | Reuse the generated-region body already sliced by Plan | Completed | `f5110f51`, `463461a4` |
| P09 | Give Framework lifecycle presence one producer-owned mapping | Completed | `5b519daf`, `cc3af2fc` |
| P10 | Return the original validated portable path instead of rejoining its segments | Completed | `83fd252a`, `bbcdc53f`, `57e16e45`, `d707407b` |

### Extension commands

| ID | Finding | State | Accepted commits |
| --- | --- | --- | --- |
| E01 | Keep command-tree construction in binding owners | Completed | `6c00e153` |
| E02 | Retain immutable Install topology instead of copying it twice at each handoff | Completed | `6f641b9b`, `87281d91`, `2d9ca4be` |
| E03 | Parse each retained topology document once per build | Deferred | — |
| E04 | Use one Inspect installed dependency reachability implementation | Deferred | — |
| E05 | Remove unused Update operation-model scaffolding and the shadow cleanup type | Completed | `05c3c2a7` |
| E06 | Remove four discarded Update next-action parameters | Deferred | — |
| E07 | Give prepared recovery identity matching one neutral owner | Completed | `76e76196`, `799bb942`, `dc6154b5` |
| E08 | Move concrete support facts out of behavior scopes and group their model topics | Completed | `6c00e153`, `05c3c2a7` |
| E09 | Pass existing Remove result facts through the result boundary | Deferred | — |
| E10 | Collapse the forwarding Inspect JSON projection type | Deferred | — |
| E11 | Share the identical complete status/stream help section | Completed | `ebadd8ef` |
| E12 | One Update generated-effect projection implementation can serve both passes | Deferred | — |
| E13 | Remove an outcome-only branch and repeated stable set construction in Update | Deferred | — |
| E14 | Remove chained conditional expressions without changing decision priority | Deferred | — |
| E15 | Use coherent output templates and provider-aware interpolation | Deferred | — |
| E16 | Use the existing lifecycle path identity and one Inspect generated-ownership constant | Deferred | — |
| E17 | Share the exact canonical effect-path predicate without broadening its grammar | Deferred | — |
| E18 | Replace the twelve-scalar Update comparison constructor with named cohesive input | Deferred | — |
| E19 | Remove the pure Update effect forwarding facade | Deferred | — |
| E20 | Close mutable Remove/Update topology at the accepted plan boundary | Completed | `88d4d771`, `19524fc3`, `9a27596d` |
| E21 | Move lifecycle snapshot behavior out of the Install planning model file | Completed | `3ea130bd` |
| E22 | Append ordinary escaped characters without allocating one string per character | Deferred | — |
| E23 | Make finite state-to-finding fallback mappings explicit | Deferred | — |
| E24 | Avoid rebuilding the same result-status precedence policy per result | Deferred | — |

### Library, Repair and Cleanup

| ID | Finding | State | Accepted commits |
| --- | --- | --- | --- |
| L01 | Cleanup repeatedly reconstructs and rescans the full effect graph | Deferred | — |
| L02 | Library application computes and wraps a summary that production discards | Completed | `d20ddee1`, `087e9166` |
| L03 | Existing cohesive Library effects are decomposed, forwarded, and recreated | Deferred | — |
| L04 | Library ancestor-path semantics have two owners | Deferred | — |
| L05 | Preflight creates an explicitly unobserved object solely to replace it immediately | Deferred | — |
| L06 | Repeated keyed/identity lookups turn bounded loops into quadratic scans | Deferred | — |
| L07 | Repair byte projection copies the complete file once per edit and is implemented twice | Deferred | — |
| L08 | Repair selected-reference comparison is copied three times | Deferred | — |
| L09 | Repair final formation duplicates reference facts and receipt semantics | Deferred | — |
| L10 | Each Repair target resolution re-enumerates the source catalogue | Retained | — |
| L11 | Renderers duplicate owned schema/projection mechanics and violate current string style | Deferred | — |
| L12 | A bounded set of pure models and behavioral factories remain misplaced | Deferred | — |
| L13 | Remaining nested conditional and finite-mapping cases | Deferred | — |
| L14 | Workspace-invalid mutation results recover subjects from full argv incorrectly | Completed | `2d7d5a9d`, `94c6fb25` |
| L15 | Small existing concepts have avoidable duplicate computation/representation | Deferred | — |
| L16 | Atomic Repair failure ordinals and final plan step ordinals are different coordinates | Completed | `39426ecd`, `ffd1995f`, `d6d87b8c` |

### Route mutation commands

| ID | Finding | State | Accepted commits |
| --- | --- | --- | --- |
| R01 | Promote the identical category filesystem observation mechanism | Completed | `e315d3fd`, `1fb94b99` |
| R02 | Give generated-navigation exposure observation one Route owner | Completed | `c8aae6f9`, `1fb94b99` |
| R03 | Retain immutable derived work once within its existing stage | Completed | `ec5f95bc`, `fbd251bd`, `c552db4b` |
| R04 | Replace Move hexadecimal equality mirrors with direct typed comparison | Deferred | — |
| R05 | Remove transparent request wrappers and preserve existing held contexts | Completed | `b03b2c2d` |
| R06 | Move state-only local helper models to their existing leaf Models topic | Partial; remainder deferred | `c6acef20` |
| R07 | Delete confirmed unused Remove code and dead local projections | Completed | `c6acef20`, `2e56c71a`, `01636875` |
| R08 | Consolidate Remove composition and separate already distinct absence ownership | Completed | `61ca0a03` |
| R09 | Consolidate fixed output blocks and duplicate workspace naming | Deferred | — |
| R10 | One bounded document edit compositor can serve Move and Remove | Deferred | — |

### Route discovery and Init

| ID | Finding | State | Accepted commits |
| --- | --- | --- | --- |
| D01 | Truthful TryDequeue nullability and ordered catalogue coverage | Deferred | — |
| D02 | Reuse immutable Init result formation when changing application phase | Deferred | — |
| D03 | One Init residual classification for both receipt kinds | Deferred | — |
| D04 | Use the physical-path dictionary already built by Init | Deferred | — |
| D05 | Compute aligned chain paths once instead of constructing every prefix repeatedly | Deferred | — |
| D06 | One Init source snapshot reader with the existing failure policy | Deferred | — |
| D07 | Parse each prospective Init document once within a build | Deferred | — |
| D08 | Represent invalid Framework scope labels as typed alignment facts | Deferred | — |
| D09 | Place actual data models beneath their nearest owning Models scope | Deferred | — |
| D10 | Reuse existing symbolic classification owners | Deferred | — |
| D11 | Apply coherent output construction and direct conditional flow | Deferred | — |
| D12 | Retained: finalizer effect recomposition has a meaningful boundary | Retained | — |
| D13 | Retained: large validated data constructors and List coverage forwarding | Retained | — |
| D14 | One List rendering vocabulary for byte-identical mappings | Deferred | — |
| D15 | Index canonical projections and entrypoint-directory multiplicity at the family fact owner | Deferred | — |
| D16 | One Inspect profile route-chain reader for three actual consumers | Deferred | — |
| D17 | Avoid repeated body measurement within one Inspect profile build | Deferred | — |
| D18 | Retained: command-specific invariant and interpretation policies | Retained | — |
| D19 | Remove the redundant diagnostic clamp after bounded escaping | Deferred | — |
| D20 | Share the exact unpaired-overwrite read projection leaf | Deferred | — |

### Context, Find, References, Index, Status and Doctor

| ID | Finding | State | Accepted commits |
| --- | --- | --- | --- |
| C01 | Finish model/support placement at the nearest owner | Partial; remainder deferred | `6087279d` |
| C02 | Context duplicates its ordered selection accumulator | Completed | `8fa29de8`, `6087279d` |
| C03 | Stop rebuilding UTF-16 validation state for every location | Deferred | — |
| C04 | Narrow Context projection input instead of reconstructing an entire closure | Completed | `8fa29de8`, `6087279d` |
| C05 | Parse Find query/content once per binding outcome | Deferred | — |
| C06 | Centralize exact Find finite predicate/rank mechanisms | Deferred | — |
| C07 | Share identical local rendering vocabulary | Deferred | — |
| C08 | References repeats complete source projection | Deferred | — |
| C09 | Remove unused authority/plumbing and dead compatibility surfaces | Partial; remainder deferred | `6087279d` |
| C10 | Avoid per-occurrence References layer-array allocation | Deferred | — |
| C11 | References finding output is byte-identical in two human renderers | Deferred | — |
| C12 | Reuse Status's existing precedence and value projection owners | Deferred | — |
| C13 | Use the existing Library record-path constant | Deferred | — |
| C14 | Hoist repeated Library mapping formation within diagnosis | Deferred | — |
| C15 | Replace boxed Doctor subject dispatch with typed formation | Deferred | — |
| C16 | Nested conditional expressions remain in bounded owners | Deferred | — |
| C17 | Emit coherent rows/blocks with interpolated templates | Deferred | — |
| C18 | Remove redundant Doctor outer JSON projections without deleting their real consumers | Deferred | — |
| C19 | Remove aliases caused by avoidable behavioral/DTO name collisions | Deferred | — |
| C20 | References filtered cancellation fallback violates its own model invariant | Completed | `f9b9b088`, `1a52d95a` |

### Shell and final style accounting

| ID | Finding | State | Accepted commits |
| --- | --- | --- | --- |
| S01 | Place Shell state contracts at their nearest model owner | Completed | `ab062c77`, `08c91891` |
| S02 | Remove the parser forwarding layer with explicit test adaptation | Completed | `39c0791d`, `75bd2757` |
| S03 | Share exact output mechanisms without merging command policy | Completed | `602c0071`, `d9553e52`, `ebadd8ef`, `c8f1e31d`, `6b6a555b` |
| S04 | Account for style and improve only matching test-support mechanics | Partial; remainder deferred | — |

### Test support

| ID | Finding | State | Accepted commits |
| --- | --- | --- | --- |
| T01 | Consolidate the identical rich tree snapshot at the E2E owner | Completed | `75313baf` |
| T02 | Let the existing external-store owner provide recovery path facts | Completed | `9aa93352` |
| T03 | Give the Move/Remove seed its actual shared owner | Completed | `6c976ceb` |
| T04 | Use the owned replacement operation for Context's tracked file | Completed | `37191c4e`, `39a6939f` |
| T05 | Place state-only shapes with their nearest Models owner | Deferred | — |
| T06 | Reuse existing valid frontmatter seeds in Route fixture builders | Deferred | — |
| T07 | Remove the unconsumed Route Remove write facade | Completed | `d09eca90` |
| T08 | Share the same published-process invocation policy for relocated layouts | Completed | `d75b5a50` |
