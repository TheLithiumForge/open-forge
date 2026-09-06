---
open-forge:
  description: Implement Extension update with source review, ownership, and recovery integrity
  tags: [Memory, Working, CLI, Task, Extension, Update, Lifecycle, Contextual]
---

# Task 17: Extension Update

## Task State

- State: Complete. The accepted candidate is commit
  `3bcb602569e7e2243a0780e1ff9cbd8f424b6457`, tree
  `63f5b22c9c73dd9aaf2044c88401e5b91638e143`; it is integrated into local
  `develop` as commit `ae055a73597c4d2310b217dc67d053aa200282db`, the same
  tree, from parent `a9987d5c208272370fc0fc1f647b7f253d12056c`. The accepted
  Red snapshot is commit `4c6d68de09bc45070e42cb184963ba9ebd3c398a`, tree
  `50a77b0d7b49313c2edd6266dc1bcd87f38a090b`, from Red parent
  `1b3f90abee85103018ac9239342e994a297665bc`. Its activation base remains
  local `develop` commit
  `a9987d5c208272370fc0fc1f647b7f253d12056c`, tree
  `e48823870cfb44914ffaaf5044e9e1125c8fd8d7`. Task 6 “Root Update” and
  Task 14 “Extension Install” are accepted prerequisites. Coherent Green is
  accepted at commit `e25a721f0099de7d7ecd160e0e1563edad430194`, tree
  `ddf020d66f2457dbb055b95eb26ca2cb8eae7639`.
- The immutable final gate basis is commit
  `d3c29a2b20e77c18484b5ab58056063e019d469e`, tree
  `18f76ed403fbe2e9047ad68f289b7c89100421b4`. Review `T17-R1` and grouped
  correction `T17-C1` are consumed.
- Permanent mapping: Task 17 “Extension Update” in the
  [project control ledger](../../project-control.md).
- Current progress: phase 5 of 5, milestone 8 of 8. The five streamlined
  phases are Preflight, explicit Gray and Red, one coherent implementation and
  focused-verification pass, one fresh whole-task review with at most one
  grouped correction, and acceptance. Preflight, Gray, Red, and coherent Green
  are accepted under Curie III and Sagan IV. Fresh full managed/public and
  packed supported `linux-x64` Native AOT evidence, grouped correction, and
  final acceptance are accepted. The immutable
  review target is commit `10c2963f8e07109e5c6fb4afb8e566ba067d22b4`, tree
  `570959dc93e3db7989c2ea662ebdaf0659c2b0ee`; the separately accepted
  milestone-5 record is commit `38cc702801def02e2d8c59eb2d39b21aa91479fa`,
  tree `511293c084dc5a4fda175b48e3918cc661438ea2`.
- Status/Doctor obligation: coherent Green extends the explicit typed
  contributor inventory, and the affected Status/Doctor and full gate evidence
  is accepted. Fresh whole-task review `T17-R1` is consumed with final
  `CHANGES_REQUIRED` for exactly one accepted High finding, `T17-R1-F2`.
  Grouped correction `T17-C1` is consumed under Curie III; the review and
  correction budgets are consumed, and Task acceptance is complete.
- Parent: [Lifecycle Commands](_lifecycle.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/extension/update/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/extension/update/behavior.md).

## Outcome And Profile

Task 17 implements `extension update` as one trusted managed reconciliation of
selected installed package identities against one exact reviewed source. It
preserves user content, package isolation, Framework meaning, and complete
recovery coverage for every existing-target effect.

The selected profile is streamlined assured. Sagan IV is the Task Mastermind.
Separate Gray and Red owners freeze the callable and evidence boundaries. One
Brilliant Implementer then owns coherent Green through focused verification and
any one grouped correction. The Task Mastermind performs one fresh whole-task
review across behavior, production structure, and test evidence before
acceptance.

## Activation And Preflight Freeze

- Baseline: the isolated branch is clean at commit
  `a9987d5c208272370fc0fc1f647b7f253d12056c`, tree
  `e48823870cfb44914ffaaf5044e9e1125c8fd8d7`. The queued Task-record content
  SHA-256 was
  `8290f13cc081bf3194d3a0229c27c1475ef738ac1a0be0000aff0182a15bcf76`.
- Accepted authority: the Extension Update Interface and Behavior SHA-256
  values are
  `68eaf38a5db6b84c27a6d9b6b8c8abee43f9ef851c27dc2cf8464ba4b791c341`
  and
  `cf5cccb18ef6c5c5b62c7cec4a6cd099f4f596e5b022aa85e74ce21de8d5512c`.
  The C# root, design, and style Directive SHA-256 values are
  `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53`,
  `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9`,
  and
  `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb`.
- Architecture: reuse the neutral Extension source, identity, manifest,
  dependency, lifecycle, ownership, generated-navigation, workspace-lock,
  mutation, and recovery capabilities. Keep selection, prior/current/intended
  comparison, normal/force/prune policy, complete planning, result,
  presentation, and orchestration local to Extension Update. Do not import
  another command's private `Shared/**` implementation or add a generic
  lifecycle engine.
- Contributor boundary: keep the six-member
  `OperationalContributorCatalogue`. Extend the existing Extension lifecycle
  producer and its narrow Status and Doctor views with the accepted set-valued
  bridge-registration observations. Doctor consumes that typed view and
  removes only the matching structural observation limitation. Static
  composition remains wiring and manufactures no fact.
- Provisional audit: obsolete commit
  `b9118828bf518efed6f3dfa490567c1f9f24cbb7`, tree
  `429dccde219c935540500b44ba59c07463ef9f82`, has parent
  `56fe9b2653b6c5f76d035d6a18f3113cd99f4d56` and diverges from the current
  base at `0d269b7a326377edc173036bc56f36e199eb6e5c`. Its branch removes later
  accepted Root Update and Route Remove work and its source-required grammar
  conflicts with the accepted embedded-source behavior. It is evidence only;
  no commit or file is transplantable without reauthoring and fresh proof.
- Experimental flow: bounded read-only Preflight lanes may overlap. Gray and
  Red preparation may overlap only where their files and meaning are genuinely
  independent. Shared facts, public representation, root composition,
  Status/Doctor integration, serialization, and semantic Green remain
  serialized. Record handoff stalls, correction cycles, gate failures, review
  findings and dispositions, and integration conflicts. Make no unsupported
  elapsed-time or numeric speed claim.
- Accepted carrier decision: `extension.bridge-registration` is a set-valued
  producer horizon with one typed observation per exact lifecycle-owned routed
  Extension payload target whose exact reviewed source facts form exactly one
  ordinary generated-navigation parent `Entries` registration. Lifecycle
  supplies target identity and owners. Exact reviewed source bytes and metadata
  establish the routed role. Neutral generated-navigation formation and
  projection supply the exact parent host and expected entry. Existing
  generated-entry comparison supplies the current, missing, unreadable, or
  inconsistent observation. Singular target wording applies per observation.
- Carrier safety: content is inspected only when readable. Source-unavailable
  coverage remains `incomplete`; ambiguous mapping is `blocked`; neither case
  infers or reconstructs a role. The five-field Extension manifest and current
  lifecycle schema remain unchanged. No compatibility path, provider bridge,
  symbolic link, registry, dependency injection, fuzzy path or content
  inference, or Task 18 manifest scan is added.
- Accepted Doctor Interface and Behavior SHA-256 values after the carrier freeze
  are `84469ddf793c2365bd2dd68e6b23ed0759cfdb5a01573bfbf85e4935d4c32cd3`
  and `888df23e36c84c250b9c22a6854a81abb0efd0bf5207a62379925380d8b45439`.
- Evidence preparation: retain exactly three public Extension Update journeys:
  help without workspace inspection or writes; reviewed update followed by an
  identical no-op; and unsafe or drifted update blocked without target,
  lifecycle, lock, or recovery effects. Retain the existing exactly three
  Doctor journeys. Exact selection counts remain forecasts until Red exists.

## Execution Capsule

- Milestones: 1 Preflight and activation; 2 Gray; 3 Red; 4 coherent production;
  5 focused, public, packed-package, full managed, and supported `linux-x64`
  Native AOT verification; 6 fresh whole-task review; 7 grouped correction or
  documented no-op; 8 acceptance.
- Applicability: the command can replace or delete user-visible managed files
  and publish Extension lifecycle meaning. Git can recover committed work, but
  the verified external bundle is the only accepted recovery evidence for each
  existing-target effect. Ordinary defects, malformed input, unavailable
  sources, interruption, stale plans, safe alias detection, and cooperating
  Open Forge processes are in scope. A malicious same-user actor and transient
  namespace swap are outside the accepted threat model.
- Standard and exceptional machinery: use the pinned .NET runtime, ordinary BCL
  filesystem and hashing APIs, existing source-generated serializers, and
  accepted Framework capabilities. No dependency change, workaround,
  compatibility shim, native bridge, reflection path, unsafe code, fake
  filesystem, JavaScript/MJS/CJS implementation, or other exceptional machinery
  is accepted.
- Behavior matrix: exact workspace and source selection; exact IDs or `--all`;
  dependency-first closure; trusted lifecycle and Framework-anchor gates;
  baseline/current/intended comparison; normal, force, prune, automatic, and
  dry-run policy; shared ownership and retired-path safety; generated
  navigation; one complete plan; lock and expected-state revalidation; recovery
  preparation, application, verification, lifecycle publication, bundle
  disposition; deterministic no-op repetition; complete, attention,
  incomplete, invalid, blocked, failed, and interrupted results; human and JSON
  presentation from one result; and no source mutation.
- Expected production paths:
  `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/**`; the exact
  Extension composition model, composer, root registration, and help paths; and
  narrowly required producer-owned Extension operational models/readers for the
  accepted bridge-registration facts.
- Expected test paths: mirrored Extension Update Unit and Integration paths,
  exactly three simple published Extension Update journeys, affected
  Status/Doctor producer and projection regressions, and the existing exactly
  three Doctor public journeys without expansion.
- Direct integration neighborhood: Extension source and lifecycle readers and
  writers, ownership, generated-navigation formation, physical paths, lock,
  mutation validation and application, recovery preparation/deletion,
  source-generated JSON, the Extension group, root composition, and the
  existing Extension lifecycle Status/Doctor contributor.
- Protected paths and meaning: Task 18 and later command paths; other commands'
  private `Shared/**`; the six-member contributor catalogue; Framework
  lifecycle meaning; public shared result coordinates; dependency, platform,
  project, build, package, release, and generated-source authority; legacy CLI,
  npm/TypeScript/package-manager paths; and all user work outside planned
  effects. A neighboring path changes only when accepted Task 17 meaning
  directly requires it and the Task Mastermind records the expansion.
- Evidence ladder: warning-free focused Unit evidence for grammar, immutable
  contracts, comparison, policy, result, findings, and presentation;
  Integration evidence for real source/workspace/lifecycle, ownership,
  generated navigation, lock/race/revalidation, recovery, failure,
  interruption, idempotence, and producer observations; exactly three public
  Extension Update journeys; one supported packed-source journey; affected
  Status/Doctor regressions; then formatting, static, protected-path,
  callable-shape, prohibited-pattern, line-length, durable host-path, full
  managed, managed-on-native, and supported `linux-x64` Native AOT gates.
- Evidence exclusions: test only Open Forge-owned request, classification,
  planning, effects, state, safety, presentation, and public reachability. Do
  not freeze System.CommandLine, STJ, YamlDotNet, Markdig, ZIP, BCL, operating
  system, xUnit, or test-platform internals. Zero-test, stale `--no-build`,
  skipped, warning-bearing, partially loaded, or wrong-scope negative receipts
  prove no gate.
- Budgets: council `0`; one fresh whole-task review `T17-R1`; one grouped
  correction `T17-C1`. Gray and Red are separate freeze boundaries, not review
  units. Read-only Preflight consumes no review unit.
- Current implementation owner: Curie III is the Brilliant Implementer and is
  paused after the accepted `T17-C1` correction. Sagan IV remains the Task
  Mastermind and retains task-local architecture, transitions, review, evidence,
  and acceptance.
- Stop and escalation: stop before changing public or cross-task meaning,
  broadening the accepted bridge-registration carrier, adding a manifest or
  lifecycle schema field, changing lifecycle/source/ownership/recovery identity,
  weakening safety or result strength, crossing protected paths, adding
  exceptional machinery, or causing any remote, destructive, publishing,
  release, or global-state effect.

## Accepted Gray Freeze

- Identity: Gray commit `fce4d7f2ab054709b6b6b5ed2dd902842cfcbafb`, tree
  `6bcf2f3bc1b7c242364afe4dd6bf084c3e6b08a3`, parent
  `0c024e80548b1c239a022ac2e2cbe38726a307c1`. Its narrow divergence correction
  is immutable commit `1b3f90abee85103018ac9239342e994a297665bc`, tree
  `b326b22b04a6a605b7e4de444862e4098fc9fe20`, with Gray as parent.
- Surface: exactly 23 pure Gray paths freeze the Extension Update callable and
  public representation plus the producer-owned Extension bridge-registration
  reader and models. Root composition, Status/Doctor wiring, serialization, and
  semantic Green remain outside this freeze.
- Evidence: targeted whitespace verification, the Core Release build, changed
  line-length, host-path, prohibited-pattern, protected/unexpected-path, and
  diff checks passed. The seven named `NotSupportedException` seams remain the
  planner, application, operation, human renderer, diagnostic renderer, JSON
  renderer, and bridge observation reader.

## Accepted Red Freeze

- Identity: Red commit `4c6d68de09bc45070e42cb184963ba9ebd3c398a`, tree
  `50a77b0d7b49313c2edd6266dc1bcd87f38a090b`, parent
  `1b3f90abee85103018ac9239342e994a297665bc`.
- Surface: exactly 15 Red test and support paths are frozen. The newline-sorted
  path manifest SHA-256 is
  `0c7fd4513f39b10337f3e5ff3399d41daf5148ab3c7571e1da938de700f94032`; the
  content manifest SHA-256 is
  `d3365b5cb4f2aeebe7c5a94e20c50cf6ee52d0ec14b1325811c2b73c7e453d4b`.
- Evidence: the full Release rebuild passed with zero warnings and errors.
  Focused Red Unit executed 22 cases with 21 passes and one intentional JSON
  seam; Integration executed 8 cases with all 8 intentionally failing at the
  unavailable root route; and public EndToEnd executed exactly 3 cases with
  all 3 intentionally failing at the unavailable root route. Skips and
  warnings were zero. Formatting, static, protected-path, callable-shape,
  prohibited-pattern, host-path, changed-line, and fact-count checks passed;
  exactly three Update and three Doctor public journeys remain. One pre-existing
  unchanged 201-character line remained in the expanded Install helper path;
  no changed addition exceeded 200 characters.
- Experimental observation: after one serialized full rebuild, parallel exact
  mechanical Unit, Integration, and EndToEnd receipts were useful. The E2E
  lane exposed a missing Framework seed in shared fixture setup, and one bounded
  warm Red correction fixed it before freeze. Cold read-only audits stalled, so
  first-file checkpoints and central consolidation remain warranted. No numeric
  speedup is claimed.

## Coherent Green Receipt

The immutable coherent Green snapshot is commit
`e25a721f0099de7d7ecd160e0e1563edad430194`, tree
`ddf020d66f2457dbb055b95eb26ca2cb8eae7639`.

The full solution Release build covered six projects, exited `0`, and reported
`0` warnings and `0` errors. Focused receipts passed Update Unit `22/22`,
Update Integration `8/8`, lifecycle observation `2/2`, Status Integration
`15/15`, and Doctor Integration `3/3`. The public Update EndToEnd suite
selected and passed exactly `3/3`; the public Doctor EndToEnd suite separately
selected and passed exactly `3/3`. All failures, skips, and warnings were `0`.

Formatting, diff, protected-path, durable host-path, prohibited-pattern,
changed-line, and callable-shape checks are clean.

Exactly 22 relative C# paths changed. Sixteen tracked paths and six formerly
untracked paths are accounted for below.

- Tracked paths (16):
  - `src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Domains/ExtensionLifecycleDoctorInspector.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Domains/ExtensionObservationHorizonDoctorInspector.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/ExtensionUpdateOperation.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/ExtensionUpdateOperationFactory.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/Models/Operation/ExtensionUpdateOperationModels.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/Models/Planning/ExtensionUpdatePlan.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/Shared/Application/ExtensionUpdateApplicationOperation.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/Shared/Planning/ExtensionUpdatePlanner.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/Shared/Rendering/ExtensionUpdateJsonProjection.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/Shared/Rendering/ExtensionUpdatePresentation.cs`
  - `src/cli/core/OpenForge.Cli.Core/Framework/Extensions/Operational/ExtensionBridgeRegistrationObservationReader.cs`
  - `src/cli/core/OpenForge.Cli.Core/Framework/Extensions/Operational/ExtensionLifecycleDoctorReader.cs`
  - `src/cli/core/OpenForge.Cli.Core/Framework/Extensions/Operational/ExtensionLifecycleOperationalContributor.cs`
  - `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs`
  - `src/cli/root/OpenForge.Cli/Composition/CliExtensionComposer.cs`
  - `src/cli/root/OpenForge.Cli/Composition/Models/CliExtensionComposition.cs`
- Formerly untracked paths (6):
  - `src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Shared/Domains/ExtensionBridgeRegistrationDoctorInspector.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/Shared/Application/ExtensionUpdateEffectApplier.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/Shared/Application/ExtensionUpdateRecoveryApplication.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/Shared/Planning/ExtensionUpdateReconciler.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/Shared/Planning/ExtensionUpdateResultFormationFactory.cs`
  - `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/Shared/Planning/ExtensionUpdateTopologyBuilder.cs`

The newline-sorted relative path manifest SHA-256 is
`2a11af62c794a6918ccc4e620942bf8ed70cf13cccbd4bb7b54305e3bdd373aa`.
The content manifest SHA-256 is
`f27b55be74212b46520384e2ce082920ac040d3d10edd99214232c8c410b4bd4`.

## Simplified Flow Trial Observation

One serialized fresh build was followed by parallel exact no-build focused and
public lanes, which gave fast independent receipts without overlapping semantic
writes. Command-private lanes were effective after the shared Green/build
freeze; shared semantics remained serialized. Callable tightening needed a
later serialized pass, showing the core-first dependency. No elapsed-time or
numeric speedup claim is made.

## Milestone 5 Gate Receipt

The accepted immutable gate basis is commit
`10c2963f8e07109e5c6fb4afb8e566ba067d22b4`, tree
`570959dc93e3db7989c2ea662ebdaf0659c2b0ee`.

Managed restore and the full solution Release build covered six projects, exited
`0`, and reported `0` warnings and `0` errors. Full managed Unit passed
`1854/1854`, Integration passed `996/996`, and EndToEnd passed `184/184`.
The canonical default-version supported `linux-x64` root artifact has SHA-256
`beeb545a3b968681d79f231b089ecffbe7d6c55276508bae60815e3ee8c662a7`.
Native Integration passed `996/996` with executable SHA-256
`2f7b8051a4b3360cf7c4f62d5459e1cca102b95cf5072e2677fb3ce3b0daa9bc`.
Native EndToEnd passed `184/184` with executable SHA-256
`29e2c0adcc871aa5022bd12a607ca4a07a7a4432a9d8efa7d8b96e65f41795da`.
Managed EndToEnd against the native root passed `184/184`. Public Update
EndToEnd passed exactly `3/3`; public Doctor EndToEnd separately passed
exactly `3/3`. All failures, skips, and warnings were `0`.

The accepted packed journey used package version
`0.0.0-dev.sha-10c2963f8e07109e5c6fb4afb8e566ba067d22b4`. The main tarball
SHA-256 is
`e07a02f2969b855195cbc8d3639c2ca5bd80a2c29ce3176a07721ad288a63805`; the
supported `linux-x64` tarball SHA-256 is
`3c37e2ecbe2863abab6199964a5cb7dbb2d51f8d26e4e1362d72c5da78fab209`.
The native root, staged native payload, and installed native payload all match
root SHA-256
`beeb545a3b968681d79f231b089ecffbe7d6c55276508bae60815e3ee8c662a7`.
Framework Install completed with 44 effects, and Extension Install completed
with 28 effects. The embedded-source Update completed as a no-op; the changed
external-source Update completed with one effect and source-target SHA-256
`9c99cf2c3d165cfa06078a009d10f6120f356c1644f6f366da37102e8a413c00`; and the
repeat Update completed as a no-op. Findings, residuals, and warnings were
absent.

Rejected evidence included SHA-qualified native build configuration, main-repo
`NODE_PATH`, npm offline materializations, and pre-closure stage attempts. Only
rejected SHA-qualified native outputs and partial failed-stage output were
preserved. Failed npm/`NODE_PATH` attempts were rejected and recorded, not
claimed as preserved artifacts. Accepted evidence used only worktree-local
ignored exact Bun-lock materializations (`TypeScript 6.0.2`, `@types/node
26.1.2`, and `undici-types 8.3.0`) and same-worktree source/artifacts.

## Milestone 6 Review Freeze

The immutable review target is commit
`10c2963f8e07109e5c6fb4afb8e566ba067d22b4`, tree
`570959dc93e3db7989c2ea662ebdaf0659c2b0ee`. The separately accepted
milestone-5 record is commit `38cc702801def02e2d8c59eb2d39b21aa91479fa`,
tree `511293c084dc5a4fda175b48e3918cc661438ea2`.

Fresh whole-task review `T17-R1` is consumed with final
`CHANGES_REQUIRED`. It accepted exactly one High finding, `T17-R1-F2`:
the planner collapses lifecycle `Invalid` and `Blocked` into
`LifecycleUnavailable` or `Incomplete`. The accepted repair maps `Invalid` and
`Blocked` to `LifecycleBlocked`; `DocumentMissing`, `SectionMissing`, and
`Unavailable` to `LifecycleUnavailable`; and `Cancelled` to `Interrupted`,
with focused lifecycle-gate evidence.

`T17-R1-F1` is withdrawn as a false positive because Extension Update next is
only at-most-one and blocked null was frozen. Recovery and cancellation have no
finding. The absent JSON `frameworkLifecycle` field is contract-correct. The
review found no architecture, C#, evidence, Native AOT, package, or public
journey finding. Exactly three Update and three Doctor EndToEnd journeys remain
preserved.

The review budget is consumed. Grouped correction `T17-C1` was active under
Curie III; milestone 7/8 correction acceptance is recorded below, and the
correction budget is now consumed.

## Milestone 7 Correction Acceptance

Grouped correction `T17-C1` is accepted at commit
`8a3a754a4d20a8247a248b59e56abd1881530dd6`, tree
`f56ff6677974a949a336c51ab382b83229bf60b5`, with parent commit
`38cc702801def02e2d8c59eb2d39b21aa91479fa`, tree
`511293c084dc5a4fda175b48e3918cc661438ea2`.

The correction fixes `T17-R1-F2`: the planner maps `Invalid` and `Blocked` to
`LifecycleBlocked`; `DocumentMissing`, `SectionMissing`, and `Unavailable` to
`LifecycleUnavailable`; `Cancelled` to `Interrupted`; and `Available` to the
normal outcome. One three-row existing Integration theory covers malformed,
non-ordinary-path, and missing-lifecycle inputs. Each row proves no effects and
leaves the workspace and source unchanged.

The exact six-path newline-sorted relative path manifest SHA-256 is
`8c62f4aa43c05ae5b9923e140b29eb6963ae44bf57d16e99e86280a691c695b7`.
The ordered content manifest SHA-256 is
`48fcd926c79924d8a4afa3d4a4937442d23aab6d278327cdf0f4334fece3367c`.
The six paths are:

- `.agents/memory/working/checkpoints/cli-development.md`
- `.agents/memory/working/cli-development/plan.md`
- `.agents/memory/working/cli-development/project-control.md`
- `.agents/memory/working/cli-development/tasks/lifecycle/extension-update.md`
- `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/Shared/Planning/ExtensionUpdatePlanner.cs`
- `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Update/ExtensionUpdateSafetyIntegrationTests.cs`

The focused Integration build and full solution Release build reported `0`
warnings and `0` errors. Update Integration passed `11/11`, with zero failures
and skips. Formatting, diff, protected-path, callable-shape, prohibited-pattern,
C# line, durable host-path, and exact public-inventory checks are clean, with
exactly three Update and three Doctor EndToEnd journeys preserved. Unstaged and
untracked counts are zero.

## Milestone 8 Final Acceptance

The immutable final gate basis is commit
`d3c29a2b20e77c18484b5ab58056063e019d469e`, tree
`18f76ed403fbe2e9047ad68f289b7c89100421b4`.

Fresh locked restore and the full solution Release build covered six projects,
reported `0` warnings and `0` errors, and passed managed Unit `1854/1854`,
Integration `999/999`, and EndToEnd `184/184`. Public Update passed exactly
`3/3`, and public Doctor passed exactly `3/3`. The default-version supported
`linux-x64` native root SHA-256 is
`67d561a5d877fd4516fa4e35a8a6e3accc67bb68f4f08266b229044c5fbbc154`.
Native Integration passed `999/999` with SHA-256
`00ea4e7854371562ae9b28b7336484e5344d2e5ab3f197cd35ad3af0fdf25cfa`, and
Native EndToEnd passed `184/184` with SHA-256
`6abdf5551b94148942fd09f3434e943a4844afd96771d5a2879a473fcd10e04d`.
Managed EndToEnd against the native root passed `184/184`; native Update and
native Doctor each passed exactly `3/3`. All failures, skips, and warnings were
`0`. Prior native outputs remain archived intact.

The final packed same-worktree package stage, pack, and install exited `0` with
SHA-versioned package manifests. Main and supported `linux-x64` tarball SHA-256
values are `d3c9b2eb24623729c5a225ca0112760bb27c77bbdd627344c4418e59aa81fc93`
and `118ba7b036b92caef55f856b152415c93f83bdefacbb09100a2be610e30ff594`.
Native root, staged native payload, and installed native payload all match the
root hash above. Framework Install completed 44 verified effects, and Extension
Install completed 28 verified effects. Embedded Update was a no-op; edited
external Update completed one verified effect; and the exact repeat was a
no-op. The source/target SHA-256 is
`9c99cf2c3d165cfa06078a009d10f6120f356c1644f6f366da37102e8a413c00`.
Findings, residuals, and warnings were absent. Prior package outputs remain
archived intact.

Task 17 “Extension Update” is Complete at phase 5/5, milestone 8/8. Curie III
is paused. Its completion grace is consumed and Task 17 is dequeued. Task 18
“Extension Remove” is the active successor at phase 2/5, milestone 1/8 with
Gray callable/public-shape review active.

## Accepted Integration

Task 17 “Extension Update” remains Complete at phase 5/5, milestone 8/8 and is
dequeued after its project-control completion grace was consumed. The
accepted candidate commit `3bcb602569e7e2243a0780e1ff9cbd8f424b6457`, tree
`63f5b22c9c73dd9aaf2044c88401e5b91638e143`, is integrated into local
`develop` as commit `ae055a73597c4d2310b217dc67d053aa200282db`, tree
`63f5b22c9c73dd9aaf2044c88401e5b91638e143`, from parent commit
`a9987d5c208272370fc0fc1f647b7f253d12056c`. The integration tree equals the
accepted candidate tree.

The declared integration delta covers 60 paths: 40 added, 20 modified, and
0 deleted. Its sorted-path SHA-256 is
`56d0dab3ba1f9463c864d3570c4c4875ef5975e2c935a39c2c56ed46d2b73911`. The
final full-gate basis is commit `d3c29a2b20e77c18484b5ab58056063e019d469e`,
tree `18f76ed403fbe2e9047ad68f289b7c89100421b4`.

Task 18 “Extension Remove” is active at phase 2/5, milestone 1/8 with Gray
callable/public-shape review active under its accepted clean activation base.
Task 19 “Repair” and Task 20 “Cleanup” remain queued behind it.

## Next Action

Task 17 acceptance and develop integration are complete, and its completion
grace is consumed. Task 18 “Extension Remove” is the active successor at phase
2/5, milestone 1/8 with Gray callable/public-shape review active; Task 19
“Repair” and Task 20 “Cleanup” follow it.

## Expected Outcome

`extension update` reconciles selected installed packages from recorded lifecycle
identity to reviewed source packages while preserving user content, package
isolation, Framework state, and one verified external recovery bundle covering
every existing target it replaces or deletes.

## Architecture

- Share package identity, manifest, payload, lifecycle, ownership, and source
  review facts with Install.
- Keep update selection, current-versus-recorded classification, conflict policy,
  multi-package order, command-local plan, findings, and result local.
- Revalidate both source package identity and workspace expected state under lock.

## Doctor Observation Ownership

Task 17 coherent Green owns the set-valued producer facts for bridge
registration. Each typed observation joins one exact
lifecycle-owned routed payload target and its owners with the same exact
reviewed source facts, one neutral generated-navigation parent host and expected
`Entries` registration, and the generated-entry comparison state. The state may
be current, missing, unreadable, or inconsistent, and content is inspected only
when readable. Source-unavailable and ambiguous mappings stay incomplete or
blocked without inference. The accepted typed contributor views and Doctor now
include these observations; Task 16's prior bounded observation limitation is
closed in this horizon. Task 18 has no Doctor producer obligation and does not
add an installed-manifest scan.

This boundary does not use legacy `open-forge.extensions.json`, package-source
manifests, broad `.agents` recursion, payload/path/byte resemblance, or
Framework bridges. Static CLI composition remains wiring only and cannot
manufacture producer facts; dependency injection and a runtime registry are not
substitutes. It adds no manifest or lifecycle field, schema change,
compatibility path, provider bridge, or installed-manifest scan.

## Evidence

Cover no-op, one/many updates, dependency order, missing/changed source, installed
drift, user-owned text, package conflicts, lifecycle missing/malformed/unknown,
dry run, lock/source race, bundle preparation and retention after partial
failure/cancellation, generated navigation, second run, process, packed source,
and AOT.

## Stop Conditions

Stop before updating unrecorded packages, overwriting unrecognized drift,
resolving ambiguous package identity, or mixing Framework and Extension lifecycle
sections.
