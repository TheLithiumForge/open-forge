---
open-forge:
  description: Executable top-down work graph for completing the greenfield replacement CLI
  tags: [Memory, Working, CLI, Plan, Architecture, Development, Contextual, Active]
---

# Replacement CLI Development Plan

## Task And Planning Boundary

- Task: [Complete The Replacement CLI](tasks/00-cli-development.md).
- Plan state: Active.
- Planning authority: The maintainer accepts consequential decisions. The
  Overseer owns architecture, sequencing, Task decomposition, integration, and
  Plan maintenance within that direction.
- Last updated: 2026-09-06.
- Current active horizon: Task 18 “Extension Remove” → Task 19 “Repair” →
  Task 20 “Cleanup”. The queued last-stage implementation order after Task 20
  is Task 23 “Workspace Libraries” → Task 24 “Extensions Evolution”. Tasks 10,
  21, 13, and 22 remain explicitly postponed. Tasks 23 and 24 have no phase or
  milestone horizons until their separate contract freezes.
- I1 proportional prerequisite: committed at exact `b25d76e`. I1 is Complete in
  exact feature candidate `4e89d945b38a2d1e24600dd22789b55e4395a534`.
  Operation orchestration is committed at `125b6a2a`; public composition and
  presentation are committed at `4e89d945`. Final closeout tip
  `2b353c48978ee88e53345be8037776181612222c` is locally squash-integrated at
  `09aa03eddb97831ff544afe1eac54ad9af501f5c`; both commits have exact tree
  `2dcfca18020980a9cafbc429a72930af3368df5f`.
- Task 4 “Route Move” is Complete at phase 6/6, milestone 12/12. Accepted
  closeout `631983ea1ec7d7ad5f5fc3999f938ba5f445ed81`, tree
  `d6f6fdf7d1caf62a9ac68582609c7282921f557c`, is squash-integrated at
  `d3d2dc1362ec1ba03844927f44fefdffb2fd466d`, tree
  `6959b51e148af44d59512d8bdd801350d88fc651`, from local `develop` parent
  `83902c88`, tree `4b8324b2`.
- Task 12 “CLI Architecture Authority Remediation” is Complete at phase 5/5,
  milestone 6/6. Corrected source
  `a0e6bc8dc25ae9395beae98289f594e1f9f56af2`, exact tree
  `9c4a33b1c16617cf79beefd9f16a6e1d2d551382`, is squash-integrated at
  `495a7ed6b55bca2a879ece83818f89e530c33af2`, the same exact tree, from
  `develop` parent `d3d2dc1362ec1ba03844927f44fefdffb2fd466d`, tree
  `6959b51e148af44d59512d8bdd801350d88fc651`. Task 14 is Complete at phase
  5/5, milestone 8/8. Accepted lane `a6b44f07`, tree `cd4c074d`, is
  squash-integrated at `20807781`, tree `4592a139`, with warning-free managed,
  Native AOT, managed-on-native, and offline package evidence. Task 15 Status is
  complete and dequeued at phase 5/5, milestone 8/8. Its historical phase-3 lane
  records Gray `dac2aece`/`874bdcae`, bounded callable correction
  `df46fb9c`/`eda74e79`, and accepted Red `f633fe1f`/`5dbfe9d7`. A Sol/xhigh
  Brilliant Implementer was activated at `de27fcd6`/`ebdcb4a6`; its early architecture checkpoint found
  that the two lifecycle Status signatures cannot guarantee one common
  lifecycle-document observation without prohibited mutable state. Production
  was held while corrected raw-snapshot Gray `b859d0aa`/`8f417c58` made one
  stateless invocation-local observation explicit and passed a warning-free
  Core Release build. Corrected Red `d768ca52`/`7a9b785b` is accepted with 14
  Unit, 10 Integration, and 3 unchanged EndToEnd tests. Resume checkpoint
  `d120d49e`/`28f23430` reapplied the interrupted Implementer's 94-line partial
  `StatusDefinitions` work losslessly; recovery stash `9b51b3f8` remains
  retained while the same Sol/xhigh Brilliant Implementer completed coherent
  production and focused verification. First coherent production seam
  `44104f03`/`71a19f85` passed a warning-free Core Release build, one direct
  Status operation Integration case, and 62 selected lifecycle-store and
  Extension compatibility tests. The other nine selected Status Integration
  cases were expected Green work at command composition or rendering.
  Immutable seam review found `T15-S1`, a real unresolved-identity
  compatibility change missed by that selection. Corrected Gray
  `e6a89c24`/`9f98c0e4` preserves three raw mechanical failure stages;
  corrected Red `c5726e5c`/`f719db23` fails only the two intended Extension
  mapping rows in its isolated 2/2 Unit selection. The same Brilliant
  Implementer completed exact stage-sensitive Green. The accepted Status lane,
  correction, and Green identities are recorded below, with integration parent
  `e90b22f9a6d4fdd2043516e718fc782490396cb2`, tree
  `74713c32473603e7f9100378fafd314c50f813e3`; the integration identity is the
  commit containing this record. Final gates had Release `0` warnings and `0`
  errors; managed Unit `1739/1739`, Integration `933/933`, EndToEnd `172/172`;
  managed EndToEnd against the native root `172/172`; native Integration
  `933/933`; native EndToEnd `172/172`; all with `0` failures, `0` skips, and
  `0` warnings. Every later producer extends the explicit contributor inventory
  and affected Status evidence before acceptance. Task 16 Doctor is Complete at
  phase 5/5, milestone 8/8. Accepted lane `a48a16cd`, exact tree `90e66b05`, is
  squash-integrated at `59276c3b` with the same tree. Fresh managed, public,
  supported `linux-x64` Native AOT, managed-on-native, focused, structural, and
  post-integration gates passed. Task 16's completion grace is consumed. Task 5
  Route Remove is complete at phase 5/5, milestone 8/8. Accepted lane
  `ab8da620`, exact tree `bb41e1c9`, is squash-integrated at `5a2e650a` with
  exact tree equality; its completion grace is consumed and it is dequeued.
  Task 6 Root Update is Complete at phase 5/5, milestone 8/8. Its final
  candidate is
  `ac96f4cc57550a83ef8651b40869ae4ff35da34e`, tree
  `195f15388d6251f69244946183209d3dfe86b24a`; T6-R1 is consumed with PASS and
  one logical T6-C1 is consumed. It is squash-integrated at
  `c6eec9d26ad6b798d418d260027241795fb4aefc`, exact tree
  `f31cacb0c54bda8ecf8a91d3516c6ffa48f48753`, from `develop` parent
  `d0d475f3b8106dfa7c8552cabab4c197bad53a71`, tree
  `c3d621989a4b6698d5a9a8de1505da16a750dbcb`. Task 7's historical platform-expansion horizon
  remains Complete at phase 4/4, milestone 7/7. Accepted lane `a2942781`, tree
  `fe36fc3f`, is squash-integrated at `e19d429e` with the same tree. Its
  disjoint provisional local-use horizon is complete at phase 3/3, milestone
  5/5. Candidate `092ead98`, tree `1c4ab3ae`, is integrated into current
  `develop`; locked restore, the exact Linux x64 local link, version/help,
  Loader, isolated JSON smoke, clean-state, and no-publication receipts passed.
  Ordinary projects use machine-global `open-forge`, but development, review,
  and acceptance worktrees use only same-worktree artifacts as evidence. ARM and
  publication remain outside scope.
- Task 6 integration adoption covered 65 paths. Its sorted-path SHA-256 is
  `88cbcb641e9a164fa61de9e357a26cf7f007a265b2b7686460d3e8f324ec5dcd`; the
  expected and actual pre-commit tree is
  `f31cacb0c54bda8ecf8a91d3516c6ffa48f48753`; the develop-relative binary
  SHA-256 is `b6975342bb29cb99fec7eef30683b4438ba537083eb7c377205581c7d433bc18`;
  candidate mismatches and eight develop-only path mismatches are zero. The
  fresh post-integration Release build completed with zero warnings and errors,
  and its managed public Update selected, discovered, executed, and passed
  exactly `3/3`, with zero failures and skips. The same-workspace executable is
  `artifacts/publish/open-forge-dev/Release/open-forge-dev` at SHA-256
  `4450c4552ac44a4e463db6c9adad89a39d803da47801801019ee08c02045bd61`; its
  `artifacts/publish/open-forge-dev/Release/open-forge-dev.version` marker has SHA-256
  `fe4d33c8c2c76a67725ea7d54dafb79c485bd2819a317d7235a6910157ef76f4` and
  reports `0.0.0-dev`. The global PATH CLI was not used as acceptance evidence.
- Task 6's accepted immutable lineage is activation
  `0bc82357a4fc7bd54ecbca1d58501d721c04baa6` → Preflight
  `48ac549c241e769246cfecb773124ccbc5076dfa` → Gray
  `ef584350a48d08b2d6307eea70f87f2f3c46283a` → bounded recovery-deletion
  addendum `7a9ded305e9ee185c02aaf636b4ff78f301b1fb1` → Red
  `13fe18a9d1cc9f829cc0cf778c44c96f97abddcb` → Green
  `a59d4df80a1f5d23cd7848140d7462495ca0a77b`, tree
  `d4a530e785dadd5e899fc73a410aec228b536201` → logical `T6-C1` physical
  follow-ups `c03c057cbc5fe204ce115ef4c4001968b27df04c`, tree
  `01769edbff77400bb54507a1d694d68015c29210`, `a252890756d166169c3d5b9bbd8a7adaa62cb896`,
  tree `b55f15814e5f5153f0cbc37ce812b7bd56fa8a68`, and
  `2e6b669d9ec2f8ce6fda7e176c1ba13d913cd9ba`, tree
  `893160afdc52ac5d7fac966cef1e1f308da4817b` → final candidate
  `ac96f4cc57550a83ef8651b40869ae4ff35da34e`, tree
  `195f15388d6251f69244946183209d3dfe86b24a`. T6-R1 is consumed with final
  focused rechecks passing. One logical T6-C1 correction is consumed; its three
  immutable physical follow-ups remain part of the lineage.
- Focused evidence contains exactly 35 Update Unit cases, 22 Update Integration
  cases, and three Update EndToEnd cases, all passing. The exactly three Doctor
  EndToEnd cases remain unchanged and pass. The contributor catalogue remains
  six members with the existing Framework and recovery attribution.
- Final Task 6 evidence is local-only: locked restore covered all six projects,
  the Release solution build had zero warnings and errors, managed Unit
  `1832/1832`, Integration `988/988`, and EndToEnd `181/181` passed, and
  managed-on-native EndToEnd `181/181`, Native Integration `988/988`, and
  Native EndToEnd `181/181` passed. Native root, Integration, and EndToEnd
  publishes each had literal exit `0` with no warning or error lines. Evidence
  uses only worktree-built artifacts; the public executable reports `0.0.0-dev`,
  and the global PATH CLI was not invoked.
- Task 6 remains Complete at phase 5/5, milestone 8/8. Its accepted squash
  integration and post-integration public Update evidence are recorded above.
- Task 6's two subsequent progress-update grace is consumed and it is dequeued.
  Task 17 “Extension Update” is Complete at phase 5/5, milestone 8/8. Its
  accepted candidate `3bcb602569e7e2243a0780e1ff9cbd8f424b6457`, tree
  `63f5b22c9c73dd9aaf2044c88401e5b91638e143`, is integrated into local
  `develop` as `ae055a73597c4d2310b217dc67d053aa200282db`, the same tree, from
  parent `a9987d5c208272370fc0fc1f647b7f253d12056c`. Its streamlined-assured
  Preflight, Gray, and Red plus the set-valued bridge-registration carrier are
  accepted; coherent Green, the full managed/native/packed gates, grouped
  correction, and final acceptance are accepted under Curie III and Sagan IV at commit
  `e25a721f0099de7d7ecd160e0e1563edad430194`, tree
  `ddf020d66f2457dbb055b95eb26ca2cb8eae7639`. The
  carrier joins exact lifecycle target ownership, exact reviewed routed-source
  facts, neutral generated-navigation parent and expected-entry facts, and
  generated-entry comparison per observation without a schema change. Review
  `T17-R1` and correction `T17-C1` are consumed. Task 17 is complete and
  dequeued after its completion grace was consumed. Task 18 “Extension Remove”
  is active at phase 2/5, milestone 1/8 with Gray callable/public-shape review
  active; Task 19 and Task 20 follow it.

Task 17's focused Green receipts are complete: the six-project full solution
Release build exited `0` with `0` warnings and `0` errors; Update Unit passed
`22/22`; Update Integration passed `8/8`; lifecycle observation passed `2/2`;
Status Integration passed `15/15`; Doctor Integration passed `3/3`; public
Update EndToEnd passed exactly `3/3`; and public Doctor EndToEnd separately
passed exactly `3/3`. Failures, skips, and warnings were all `0`. Formatting,
diff, protected-path, durable host-path, prohibited-pattern, changed-line, and
callable-shape checks are clean. Exactly 22 relative C# paths are accounted for
(16 tracked and 6 formerly untracked); the path and content manifest SHA-256
values are `2a11af62c794a6918ccc4e620942bf8ed70cf13cccbd4bb7b54305e3bdd373aa`
and `f27b55be74212b46520384e2ce082920ac040d3d10edd99214232c8c410b4bd4`.
The complete path list remains in the [Task 17 record](tasks/lifecycle/extension-update.md).

The Task 17 flow observation is one serialized fresh build followed by parallel
exact no-build focused and public lanes that gave fast independent receipts
without overlapping semantic writes. Command-private lanes were effective after
the shared Green/build freeze; shared semantics remained serialized. Callable
tightening needed a later serialized pass, showing the core-first dependency.
No elapsed-time or numeric speedup claim is made. Milestone 5's fresh full
managed/public and packed supported `linux-x64` Native AOT gates are accepted
at immutable basis
`10c2963f8e07109e5c6fb4afb8e566ba067d22b4`, tree
`570959dc93e3db7989c2ea662ebdaf0659c2b0ee`; the separately accepted
milestone-5 record is commit `38cc702801def02e2d8c59eb2d39b21aa91479fa`,
tree `511293c084dc5a4fda175b48e3918cc661438ea2`. Fresh whole-task review
`T17-R1` is consumed with final `CHANGES_REQUIRED` for exactly one accepted
High finding, `T17-R1-F2`: the planner collapses lifecycle `Invalid` and
`Blocked` into `LifecycleUnavailable` or `Incomplete`. The accepted repair
maps `Invalid` and `Blocked` to `LifecycleBlocked`; `DocumentMissing`,
`SectionMissing`, and `Unavailable` to `LifecycleUnavailable`; and
`Cancelled` to `Interrupted`, with focused lifecycle-gate evidence.

`T17-R1-F1` is withdrawn as a false positive because Extension Update next is
only at-most-one and blocked null was frozen. Recovery and cancellation have no
finding. The absent JSON `frameworkLifecycle` field is contract-correct. No
architecture, C#, evidence, Native AOT, package, or public journey finding was
accepted. Exactly three Update and three Doctor EndToEnd journeys remain
preserved. Review budget is consumed; grouped correction `T17-C1` was active
under Curie III and is accepted below, so the correction budget is consumed.

Task 17's milestone 7/8 correction acceptance is commit
`8a3a754a4d20a8247a248b59e56abd1881530dd6`, tree
`f56ff6677974a949a336c51ab382b83229bf60b5`, with parent commit
`38cc702801def02e2d8c59eb2d39b21aa91479fa`, tree
`511293c084dc5a4fda175b48e3918cc661438ea2`. It fixes `T17-R1-F2` by mapping
`Invalid` and `Blocked` to `LifecycleBlocked`; `DocumentMissing`,
`SectionMissing`, and `Unavailable` to `LifecycleUnavailable`; `Cancelled` to
`Interrupted`; and `Available` to the normal outcome. One three-row existing
Integration theory covers malformed, non-ordinary-path, and missing-lifecycle
inputs, proving no effects and unchanged workspace and source.

The exact six-path newline-sorted relative path manifest SHA-256 is
`8c62f4aa43c05ae5b9923e140b29eb6963ae44bf57d16e99e86280a691c695b7`; the
ordered content manifest SHA-256 is
`48fcd926c79924d8a4afa3d4a4937442d23aab6d278327cdf0f4334fece3367c`.
The focused Integration build and full solution Release build reported `0`
warnings and `0` errors; Update Integration passed `11/11`, with zero failures
and skips. Formatting, diff, protected-path, callable-shape, prohibited-pattern,
C# line, durable host-path, and exact public-inventory checks are clean, with
exactly three Update and three Doctor EndToEnd journeys preserved. Unstaged and
untracked counts are zero. Curie III is paused. The next boundary is milestone
8/8 fresh final managed/public, supported `linux-x64` Native AOT, and packed
same-worktree acceptance.

Task 17's milestone 8/8 final acceptance is at immutable gate basis commit
`d3c29a2b20e77c18484b5ab58056063e019d469e`, tree
`18f76ed403fbe2e9047ad68f289b7c89100421b4`. Fresh locked restore and the full
solution Release build covered six projects with `0` warnings and `0` errors;
managed Unit, Integration, and EndToEnd passed `1854/1854`, `999/999`, and
`184/184`; public Update and public Doctor each passed exactly `3/3`; native
Update and native Doctor each passed exactly `3/3`; and managed-on-native
EndToEnd passed `184/184`. The default-version supported `linux-x64` native
root SHA-256 is
`67d561a5d877fd4516fa4e35a8a6e3accc67bb68f4f08266b229044c5fbbc154`; native
Integration passed `999/999` at SHA-256
`00ea4e7854371562ae9b28b7336484e5344d2e5ab3f197cd35ad3af0fdf25cfa`; native
EndToEnd passed `184/184` at SHA-256
`6abdf5551b94148942fd09f3434e943a4844afd96771d5a2879a473fcd10e04d`.
Failures, skips, and warnings were all `0`; prior native outputs remain archived
intact.

The final packed same-worktree package stage, pack, and install exited `0` with
SHA-versioned package manifests. Main and supported `linux-x64` tarball
SHA-256 values are
`d3c9b2eb24623729c5a225ca0112760bb27c77bbdd627344c4418e59aa81fc93` and
`118ba7b036b92caef55f856b152415c93f83bdefacbb09100a2be610e30ff594`.
Native root, staged native payload, and installed native payload match the root
hash. Framework Install completed 44 verified effects, Extension Install 28
verified effects, embedded Update was a no-op, edited external Update completed
one verified effect, and the exact repeat was a no-op. The source/target SHA-256
is `9c99cf2c3d165cfa06078a009d10f6120f356c1644f6f366da37102e8a413c00`.
Findings, residuals, and warnings were absent. Prior package outputs remain
  archived intact. Task 17 is Complete and dequeued after its completion grace
  was consumed. Task 18 “Extension Remove” is active at phase 2/5, milestone
  1/8 with Gray callable/public-shape review active; Task 19 and Task 20 follow
  it.

Task 17's accepted candidate commit `3bcb602569e7e2243a0780e1ff9cbd8f424b6457`,
tree `63f5b22c9c73dd9aaf2044c88401e5b91638e143`, is integrated into local
`develop` as commit `ae055a73597c4d2310b217dc67d053aa200282db`, tree
`63f5b22c9c73dd9aaf2044c88401e5b91638e143`, from parent commit
`a9987d5c208272370fc0fc1f647b7f253d12056c`. The integration tree equals the
accepted candidate tree. The declared integration delta covers 60 paths: 40
added, 20 modified, and 0 deleted; its sorted-path SHA-256 is
`56d0dab3ba1f9463c864d3570c4c4875ef5975e2c935a39c2c56ed46d2b73911`. The
final full-gate basis is commit `d3c29a2b20e77c18484b5ab58056063e019d469e`,
tree `18f76ed403fbe2e9047ad68f289b7c89100421b4`. Task 17 is complete and
dequeued after its completion grace was consumed. Task 18 “Extension Remove”
is active at phase 2/5, milestone 1/8 with Gray callable/public-shape review
active; Task 19 and Task 20 follow it.

Task 17's accepted gate receipt is the six-project managed restore/Release build
with exit `0`, `0` warnings, and `0` errors; full managed Unit/Integration/
EndToEnd `1854/1854`, `996/996`, and `184/184`; canonical default-version
supported `linux-x64` root SHA-256
`beeb545a3b968681d79f231b089ecffbe7d6c55276508bae60815e3ee8c662a7`; native
Integration `996/996` at SHA-256
`2f7b8051a4b3360cf7c4f62d5459e1cca102b95cf5072e2677fb3ce3b0daa9bc`; native
EndToEnd `184/184` at SHA-256
`29e2c0adcc871aa5022bd12a607ca4a07a7a4432a9d8efa7d8b96e65f41795da`; and
managed-on-native EndToEnd `184/184`. Public Update and Doctor EndToEnd
separately passed exactly `3/3` each. Failures, skips, and warnings were `0`.

The accepted packed journey used version
`0.0.0-dev.sha-10c2963f8e07109e5c6fb4afb8e566ba067d22b4`; main and supported
`linux-x64` tarball SHA-256 values are
`e07a02f2969b855195cbc8d3639c2ca5bd80a2c29ce3176a07721ad288a63805` and
`3c37e2ecbe2863abab6199964a5cb7dbb2d51f8d26e4e1362d72c5da78fab209`.
Native root, staged native payload, and installed native payload match the root
SHA-256 above. Framework Install completed 44 effects, Extension Install 28
effects, embedded Update was a no-op, changed external Update completed one
effect with source-target SHA-256
`9c99cf2c3d165cfa06078a009d10f6120f356c1644f6f366da37102e8a413c00`, and the
repeat Update was a no-op. No findings, residuals, or warnings were reported.

Rejected evidence included SHA-qualified native build configuration, main-repo
`NODE_PATH`, npm offline materializations, and pre-closure stage attempts. Only
rejected SHA-qualified native outputs and partial failed-stage output were
preserved. Failed npm/`NODE_PATH` attempts were rejected and recorded, not
claimed as preserved artifacts. Accepted evidence used only worktree-local
ignored exact Bun-lock materializations (`TypeScript 6.0.2`, `@types/node
26.1.2`, and `undici-types 8.3.0`) and same-worktree source/artifacts.

- After `T15-S1`, the immutable pre-acceptance Status continuation remains
  historical:
  `cf00fb86`/`bf69e7c2` → `48309613`/`b8e0ad0f` → `e79a9767`/`17068f0b` →
  `2521574d`/`11eb89ee` → `b8ac2dd7`/`453eb1b8`, covering rendering and
  compatibility Green, direct JSON context, plural-source Gray, direct producer
  construction authority, and focused plural-source Red. Red selected,
  discovered, and executed `1/1/1`; only the expected three-observation versus
  one-scaffold oracle failed, while lifecycle completeness/trust and five
  package/source facts passed. Independent Route selected-view work was also
  present. Task 15 is now accepted at phase 5/5, milestone 8/8. Its accepted
  lane is `0c19b7053ef2b8c48898cfebadedff0c5702bf34`, tree
  `8ff2ac4ac0f5985863a9aaf85911c2deb6152980`; correction
  `f2dcdcae8b7c2cc9522b5ce7df250c73a55347db`, tree
  `b6f83beb50cefe56a0aae7d9a4019dab08c19aea`; and Green
  `7dd8de6c4f9eae3355f5ac4ee32d533e6ffb2564`, tree
  `f6c37550b78a01e7611178bd563e274f94bb2bb1`. Final gates had Release `0`
  warnings and `0` errors; managed Unit `1739/1739`, Integration `933/933`,
  EndToEnd `172/172`; managed EndToEnd against native root `172/172`; native
  Integration `933/933`; native EndToEnd `172/172`; all with `0` failures,
  `0` skips, and `0` warnings. Every later producer extends the explicit
  contributor inventory and affected Status evidence before acceptance.
- Task 16 consumes the complete plural Extension view and six explicit
  producer-owned Doctor views without fallback, substitution, reconstruction,
  reread, command-output parsing, dependency injection, a service locator,
  reflection, a runtime registry, or a generic operational engine. The accepted
  108-kind first-release horizon supplies producer-backed emissions for all 19
  Extension kinds. Task 17 closed the bridge-registration observation; Task 18
  has no Doctor producer obligation.
- Accepted Route Update integration input baseline: local `develop` commit
  `996c2e17d1142ffb30dc7a2d17df657419566f97`, exact tree
  `877314d48db49013edc1c4dad1abb545b37caefc`. It integrates CLI
  Quality Remediation and closeout at `862cbf2a` and `5053bf0c`, bounded review
  orchestration at `5aad04ac`, permanent task identity and dynamic progress at
  `3356eba1`, repository-local npm linking for the managed development CLI at
  `128b70b3`, and the streamlined Task 4–6 trial. The Route Create command
  integration is `19412d2`. Route Init
  is squash-integrated at `cc5085ce` from
  reviewed closeout `c5801494ac6426add2c64e32cafbba6f0162561a` with exact tree
  equality. Its executable evidence candidate remains
  `cb62b19f73afcace163371af9093d877821fa800`, exact tree
  `be93900d0dc102fcf2d5a351651c0b0134de39a0`.
  The integration baseline includes the accepted
  intended-source formation integration after public Index, integrated D0/SF1-SF4
  foundations, M2 preparation, Route Inspect interaction correction, and the
  protected Extension Create and root Install public integrations. Task 3
  “Route Update” is Complete at phase 7/7, milestone 12/12, from accepted task
  base `5aad04a`, tree `0f56b2c`, through closeout `27df8325`, tree `b68d4349`.
  The commit containing this record squash-integrates that accepted delta after
  reconciling its five continuity overlaps. The baseline also
  includes canonical lifecycle creation at `1d404c5` and the neutral Markdown
  link-label projection at `89a35a7`. The preceding C2 baseline was
  `c60fcb98a57e9ec80769b9cb1d399ce13a227863`, exact tree
  `464a4a6b6ef6447209edffbf53df7348c70691ed`. The preceding C1 baseline was
  `4c85d1d62004e8bdc885c51873ff6d9cdb6e6db5`, exact tree
  `fa29bd9572df39b2d5457c35bb8a0bd6ba5a9945`. The preceding C3 baseline was
  `fa3db1ee1dbfb687715b5b90b35f6104cbc45c6c`, exact tree
  `605620d990622e093b12e85e50c6e40083896a18`. The shared-foundation baseline was
  `33913dfe7f8f80598ca4765c516d308ed179c3ab`,
  exact tree `a56f3c201013b5999841414e1df469713f08cdfe`. The pre-foundation baseline was
  `18f2acff31cfd6600a16430ac5d689d05482e297`, exact tree
  `39f0a8c4e6d3695cfbe7407dfd6043dc5ec9680a`. Its Release
  build passes with 0 warnings and 0 errors. Managed Unit `1227/1227`,
  Integration `481/481`, and EndToEnd `125/125` pass. Portable `linux-x64`
  Native AOT Integration `481/481` and EndToEnd `125/125` pass. Every accepted
  final test run has zero failures and zero skips. Real-workspace projection
  proves zero writes, and independent Sol/xhigh review is `ROBUST` with 99%
  confidence. Context and
  Extension List are Complete and squash-integrated at `ca097a2` and `db0d39a`.
  Extension Inspect's exact public contract is
  squash-integrated at `92313a0`; accepted rebased feature `2b1e63d` is
  squash-integrated at `73b01be`. Routed Authored Metadata is Complete and squash-integrated at
  `5924698`. Generated Navigation is Complete and squash-integrated at `21e5200`;
  its full managed Unit `1030/1030`, Integration `357/357`, and local `linux-x64`
  Native AOT Integration `357/357` gates pass. The last complete public baseline
  passes managed Unit `978/978`, Integration `354/354`, EndToEnd `111/111`,
  Native AOT Integration `354/354`, and Native AOT EndToEnd `111/111`, all with
  zero skips. The earlier combined read-only baseline passed managed Unit `1054/1054`,
  Integration `378/378`, EndToEnd `116/116`, and `linux-x64` Native AOT
  Integration `378/378` and EndToEnd `116/116`.
  The routed-metadata acceptance gate separately passes full managed Unit
  `1012/1012`, Integration `354/354`, and Native AOT Integration `354/354`.
  [Improve The
  Repository-Root CLI Developer Workflow](tasks/repository-root-developer-workflow.md)
  is Complete and its accepted changes are included in that baseline. Read-Only
  CLI Dogfooding Corrections are squash-integrated at `bba84b6`, with managed
  Unit `1031/1031`, Integration `411/411`, and EndToEnd `120/120`, plus supported
  `linux-x64` Native AOT Integration `411/411` and EndToEnd `120/120`.
- [Correct Proportional CLI Findings](tasks/proportional-cli-corrections.md) is
  Complete. Proportionate guidance is integrated at `5f9f59e`, Route List
  corrections at `2cd525d`, and authoritative Markdown generated-region
  corrections at `0d88606`. The integrated Release build, managed
  `1053/411/120`, and portable `linux-x64` Native AOT `411/120` gates pass with
  zero skips; final Sol/xhigh review is `ROBUST`.
- Current result: the accepted Generated Navigation formation expansion adds
  `Build(SourceCatalogue observedCatalogue, IReadOnlyList<SourceLogicalSource> intendedSources)`
  while preserving exact current-state `Build(SourceCatalogue)` behavior. It
  separates observed catalogue evidence from intended membership and derived
  topology facts without creating a prospective catalogue/source framework,
  virtual filesystem, temporary checkout, or hidden Index.
  [Mutation Foundation](tasks/mutation-foundation/_mutation-foundation.md)
  is Complete at exact production candidate `e7d937f` under authority
  `01dd552`. Its final managed, portable `linux-x64` Native AOT, static-absence,
  format, diff, and independent-review gates pass. Public
  [Index](tasks/read-only/index-command.md) is Complete in exact feature
  candidate `4e89d945b38a2d1e24600dd22789b55e4395a534`. Its warning-free Release
  build, managed Unit `1206/1206`, Integration `480/480`, EndToEnd `125/125`,
  portable `linux-x64` Native AOT Integration `480/480`, EndToEnd `125/125`,
  and root publish/version smoke pass with zero failures or skips. Final closeout
  tip `2b353c48978ee88e53345be8037776181612222c` is locally integrated at
  `09aa03eddb97831ff544afe1eac54ad9af501f5c` with exact tree equality.
  [Test Architecture And
  Constants](tasks/test-architecture-and-constants.md) is Complete and its exact
  accepted tree is squash-integrated at `b6ce31f`.
  [Extension Inspect](tasks/read-only/extension-inspect.md) is
  Complete and integrated at `73b01be`. The [pure Generated Navigation
  foundation](tasks/read-only/index-generated-navigation-foundation.md) is
  Complete and integrated at `21e5200`.
  [Routed Authored Metadata
  foundation](tasks/read-only/routed-authored-metadata-foundation.md) is Complete
  and integrated at `5924698`.
- Current state: D0 and the four next-wave shared foundations are integrated and
  accepted. D0 is integrated at `38e1498`; SF1 native interaction at `e782090`,
  SF2 embedded Framework distribution at `680915a`, SF3 lifecycle source-asset
  provenance at `0989356`, and SF4 lease-bound directory creation at `33913df`.
  The combined reviewed baseline has Release `0` warnings and `0` errors,
  managed Unit `1284/1284`, Integration `500/500`, EndToEnd `125/125`, Native
  AOT Integration `500/500`, Native AOT EndToEnd `125/125`, and zero skips.
  C1 Extension Create is Complete: command-local squash
  `3ef81227ba50fba869f0129b958eabc6d0c29fbc` and protected public integration
  squash `4c85d1d62004e8bdc885c51873ff6d9cdb6e6db5` close exact final candidate
  `789cc917f2d0cb38c5229cc2dc7fee013218d341` at tree
  `fa29bd9572df39b2d5457c35bb8a0bd6ba5a9945`. Its final managed Unit
  `1356/1356`, Integration `575/575`, EndToEnd `132/132`, and `linux-x64`
  Native AOT root version/ELF, Integration `575/575`, and EndToEnd `132/132`
  evidence pass with zero failures and zero skips. Native dogfood makes no
  writes; final independent Sol/xhigh review is `PASS — ROBUST` at `0.98`
  confidence, with direct PTY process proof deferred. C2 root Install is Complete:
  final candidate `11994e4d21ddc807b7480afc39ae3612e5a69a56` is squash-integrated
  at `c60fcb98a57e9ec80769b9cb1d399ce13a227863`, with exact tree
  `464a4a6b6ef6447209edffbf53df7348c70691ed`. Its final Release build has `0`
  warnings and `0` errors; managed Unit `1390/1390`, Integration `598/598`, and
  EndToEnd `136/136` pass; `linux-x64` Native AOT Integration `598/598` and
  EndToEnd `136/136` pass; and two independent Sol/xhigh rechecks pass. Native
  dry-run dogfood safely blocks on the repository's existing generated-region
  state without effects, workspace changes, lifecycle publication, or a new
  external lock. C3 Route Inspect interaction is
  squash-integrated at `fa3db1ee` with exact tree equality to final reviewed
  candidate `37c9360`. C3's Root-owned terminal composition and process seams
  are closed; C2's root composition, serialization, help, process, and Native AOT
  seams are also closed. Its exact fully present ordered public Install JSON
  result is accepted and frozen, including typed residual values `none`,
  `retained`, and `unknown`. Generic and Framework-aware Route Init is Complete
  and squash-integrated at `cc5085ce`. Route Create is Complete and
  squash-integrated at `19412d2`, exact tree `2bbba7e`; it supplies the
  `RouteCreateJsonContext` and Route-help predecessor slices. The separate
  [CLI Quality Remediation](tasks/cli-quality-remediation.md)
  Task is Complete and squash-integrated at `862cbf2a`, exact tree `571f104f`,
  from accepted implementation candidate `a4ccf19a`, tree `97254e65`. Task 3
  “Route Update” is Complete at accepted closeout `27df8325`, tree `b68d4349`.
  Route Move is Complete at accepted closeout `631983ea`, tree `d6f6fdf7`.
  Task 12 is Complete and squash-integrated at `495a7ed6`, exact source tree
  `9c4a33b1`. The adoption slice is complete and Route Remove is accepted and
  integrated at `5a2e650a`, exact tree `bb41e1c9`. The complete Route Mutation
  M2 lane now unblocks root Update M3.
  New lifecycle documents now emit all five ordered root keys. Framework-created
  documents use complete empty Extensions, and existing incomplete files remain
  untrusted. The integrated Markdown parser now exposes AST-only `Supported` or
  `Unsupported` link labels;
  its Release `0/0`, parser `31/31`, Unit `1410/1410`, Integration `614/614`, and
  independent `ROBUST PASS` review gates pass. Task 15 Status is
  complete and dequeued at phase 5/5, milestone 8/8; its
  accepted lane, correction, Green, integration parent, and final gates are
  recorded above. Task 16 Doctor is complete at phase 5/5, milestone 8/8;
  accepted lane `a48a16cd`, exact tree `90e66b05`, is squash-integrated at
  `59276c3b` with exact tree equality. Task 5 Route Remove is complete at phase
  5/5, milestone 8/8; accepted lane `ab8da620`, exact tree `bb41e1c9`, is
  squash-integrated at `5a2e650a` with exact tree equality from parent
  `0d269b7a`, tree `8863d176`. Task 6 Root Update is Complete at phase 5/5,
  milestone 8/8 and is squash-integrated at `c6eec9d2`, exact tree `f31cacb0`.
  Its final candidate is `ac96f4cc57550a83ef8651b40869ae4ff35da34e`, tree
  `195f15388d6251f69244946183209d3dfe86b24a`; T6-R1 is consumed with PASS,
  one logical T6-C1 is consumed, and the accepted lineage is recorded above.
  Its provisional Gray/Red lane remains rejected transplant input preserved for
  history, and one Brilliant Implementer completed coherent Green and focused
  verification.
  Independent scope/contract/ownership discovery, callable-surface analysis,
  Gray/Red readiness, and worktree preparation may overlap earlier work;
  dependent command behavior may not. The localized C1/C2/C3 interaction,
  defaults, catalogue, and directory-effect decisions are frozen below;
  Extension Create's and root Install's exact command-local JSON results are
  frozen. Implementation may not silently broaden either boundary.
  Continue operations, delivery, and final acceptance in the persisted order
  after their accepted predecessors.
  The snapshot slice remains a no-op, with possible LithSnap-backed
  presentation candidates recorded only as a deferred Idea.
  The maintainer accepted that dependency correction; public Index must consume
  the shared accepted mechanics and must not create a local substitute.
  Standard SDK publication restored the exact portable
  `Microsoft.NETCore.App.Runtime.NativeAOT.linux-x64` `10.0.11` pack without a
  project workaround. The explicit `linux-x64` build passed with zero warnings
  and errors; build-selected References EndToEnd passed `12/12`; Native AOT
  Integration passed `308/308`; and Native AOT EndToEnd passed `82/82`, all with
  zero skips. Root tooling, automatic managed `open-forge-dev` publication,
  build-selected native EndToEnd discovery, temporary compatibility-name routing,
  WSL portability, and obsolete preserved-test removal remain implemented.

### Next-Wave Foundation Integration

D0's accepted contract freeze is integrated at `38e1498`. The reviewed shared
foundations are integrated at `e782090` (SF1 native interaction), `680915a`
(SF2 embedded Framework payload), `0989356` (SF3 lifecycle source-asset
provenance), and `33913df` (SF4 directory mutation). The combined reviewed
baseline has a Release build with `0` warnings and `0` errors; managed Unit
`1284/1284`, Integration `500/500`, and EndToEnd `125/125`; Native AOT
Integration `500/500` and EndToEnd `125/125`; and zero skips in every stated
run.

The accepted lock-location correction replaces the workspace-contained lock and
bootstrap result with one persistent external zero-byte ordinary file under
`LocalApplicationData/OpenForge/locks/v1`. Its filename combines a bounded
display-only workspace name with the authoritative full SHA-256 key of the
normalized physical workspace path. Missing `.agents` is an ordinary
lease-bound directory-create effect.

C1 Extension Create is Complete at protected public integration
`4c85d1d62004e8bdc885c51873ff6d9cdb6e6db5`, with exact final candidate
`789cc917f2d0cb38c5229cc2dc7fee013218d341` and tree
`fa29bd9572df39b2d5457c35bb8a0bd6ba5a9945`. Its final managed Unit
`1356/1356`, Integration `575/575`, EndToEnd `132/132`, and `linux-x64`
Native AOT root version/ELF, Integration `575/575`, and EndToEnd `132/132`
evidence pass with zero failures and zero skips. Native dogfood makes no writes;
the final independent Sol/xhigh review is `PASS — ROBUST` at `0.98` confidence,
with direct PTY process proof deferred. C2 root Install is Complete at local
integration `c60fcb98a57e9ec80769b9cb1d399ce13a227863`, exact tree
`464a4a6b6ef6447209edffbf53df7348c70691ed`, from final reviewed candidate
`11994e4d21ddc807b7480afc39ae3612e5a69a56`. Its final managed
`1390/598/136`, portable `linux-x64` Native AOT `598/136`, focused post-rebase,
review, and safe-blocking dry-run dogfood evidence pass within their stated
boundaries. C3 Route Inspect interaction is squash-integrated at `fa3db1ee` with
exact tree equality to final reviewed candidate `37c9360`. C2's exact public
Install JSON result schema is accepted and frozen, including typed residual
values `none`, `retained`, and `unknown`. Route Init is Complete at `cc5085ce`;
Route Create is Complete at `19412d2`, exact tree `2bbba7e`, and supplies the
`RouteCreateJsonContext` and Route-help predecessor slices. The separate CLI
Quality Remediation Task is Complete and squash-integrated at `862cbf2a`, exact
tree `571f104f`, from accepted implementation candidate `a4ccf19a`, tree
`97254e65`. Task 3 “Route Update” is Complete at accepted closeout `27df8325`,
tree `b68d4349`.
M3 root Update remains after the complete M2 Route Mutation lane.

- Previous accepted Find history: Find Child 2 original Preflight through Blue history is accepted
  through exact commit `685e2dd`. During original Purple, top-down review found
  generic YAML event parsing inside Find and duplicate Markdown-frontmatter
  extraction in Route. The Mastermind established the shared
  `Framework/Documents/{Markdown,Yaml}` foundation through a bounded
  Gray → Red → Green → Blue correction. Gray is accepted at exact `2cae7a4`, Red
  at exact `a673ebe`, and Green at exact `d4701ad`. Correction Blue is accepted at
  exact `0006915`, and corrected Purple at exact `2d10474`. Final review found one
  original Green authored-tag mismatch; supplemental Red is accepted at exact
  `c72dd5e`, its inherited Unit expectation correction at exact `d494adb`, and its
  Integration expectation correction at exact `23fe39a`. Corrected Green is
  accepted at exact `2337d62`; Child 2 final evidence and acceptance are recorded
  at exact `ff7ce3f` (`Accept Find query operation`). Child 3 focused Preflight is
  accepted at exact `28d316a` (`Freeze Find presentation preflight`), and Gray is
  accepted at exact `a76a217` (`Establish Find presentation contracts`). The
  complete original Red packet is accepted at exact `22d3bff`. Its historical
  Unit evidence was `202` total with `92` pass and `110` intentional failures;
  Integration was `43` total with `23` pass and `20` intentional failures; and
  published EndToEnd was `13` total with `13` intentional failures, all with
  zero skips. The Integration metadata correction is accepted at exact `6a9a0de`;
  the mirrored EndToEnd metadata correction is accepted at exact `eea3d59`
  (`Complete Find presentation metadata evidence`). Its post-commit Red
  reproduction succeeded as intentional Red: managed non-AOT `win-x64` publish
  passed, and published Find EndToEnd was `13` total with `13` intentional
  failures and zero skips, all terminating at absent Green root registration.
  Historical pre-correction review found the independent human `\t` expectation
  contrary to the frozen lowercase `\uXXXX` control contract and found bounded
  diagnostics slicing escaped code units, which allowed partial `\\`, `\"`, or
  `\uXXXX` tokens. That review returned the work narrowly to Red without changing
  contracts, Child 2, Integration, EndToEnd, production, package/project,
  generated routing, or Route behavior. Supplemental escaping Red correction is
  accepted at exact `a865fd1` (`Correct Find escaping evidence`); it changes only
  `FindHumanRenderingRedTests.cs` and `FindDiagnosticsAndHelpRedTests.cs`, sets
  TAB to `\u0009`, and adds four Windows renderer-level boundary cases for
  complete escape-token grammar, bounds, one-line output, and no payload leak.
  Its clean detached-worktree evidence includes locked restore, warning-free
  Release build, format verification, full focused Unit `206` total with `92`
  pass and `114` intentional Gray-boundary failures, and narrow selected evidence
  `6` total with `6` intentional failures, all with zero skips. Fresh test-only
  correctness review at that Red boundary is `PASS`; no material optional
  improvement remains. The pre-correction narrow Green selection for TAB plus
  four token-boundary cases was `6` total with `1` pass and `5` intended
  defect-exposing failures, zero skips.

  Child 3 Green is accepted at exact commit `cb7874c` (`Implement Find
presentation`) over corrected Red `a865fd1`. It is the production/root-
  composition-only Green; no test, support, contract, project, package,
  configuration, generated-routing, or Route behavior change enters Green. The
  exact Green scope is 15 production paths: Find binding, request, result builder,
  and validation; compact, expanded, JSON, diagnostic, help, and shared text
  escaping; Shell direct-root command tree and root factory; and root composition.
  It keeps one symbol graph and operation/result flow, binding-owned help,
  source-generated concrete `FindJsonDocument`, explicit malformed-content request
  presence, deterministic bounded human/JSON/diagnostic behavior, and exact one
  root Find registration. It adds no reflection, second parser/operation/renderer
  catalogue, workspace writes, or Native AOT work. The correction encodes TAB as
  `\u0009` and truncates only at complete escaped-token boundaries.

  Fresh Green evidence passes locked restore, a warning-free Release solution
  build, format verification, and `git diff --check`; focused Find+direct-root
  Unit `206/206`; focused Find+generated-serialization Integration `43/43`;
  affected Shell+Route Unit `347/347`; affected Shell+Route Integration `160/160`;
  managed non-AOT `win-x64` publish; and published Find EndToEnd `13/13`, all with
  zero skips. Fresh final bounded Green correctness review is `PASS` with no
  material findings. It verifies corrected escaping, Find binding, explicit
  malformed-content state, root leaf registration, renderer dispatch, JSON
  projection/source generation, diagnostics, help, direct Shell integration, one
  operation/result flow, and no protected-surface drift. Native AOT is intentionally
  not claimed.

  Earlier local-improvement review found one material bounded Blue candidate only:
  in `FindJsonProjection`, replace duplicate finite `Status` and `FindingCode`
  switches with canonical `CliStatusDefinitions.Read(...).MachineName` and
  `FindDefinitions.ReadFindingCode(...)`. Its separate escaping correctness
  finding was resolved in Green; it is not a Green defect. Blue applied that
  candidate and is accepted at exact `3f81e76` (`Simplify Find JSON projection`). It changed
  production structure only in
  `src/cli/core/OpenForge.Cli.Core/Commands/Find/Shared/Rendering/FindJsonProjection.cs`:
  it replaced the duplicate local switches with the canonical readers, then
  removed the two duplicate private mapping methods. No behavior, public
  output/order/schema, test/support, contract, package/project/configuration,
  generated routing, Shell/root, Route, workspace-write, or Native AOT change
  occurred. Blue evidence is a warning-free Release solution build, format
  verification, `git diff --check`, focused Find+direct-root Unit `206/206`, and
  focused Find+generated-serialization Integration `43/43`, all with zero skips.
  No managed republish or Native AOT claim is needed for this one-file
  behavior-preserving Blue. Fresh bounded Blue correctness review is `PASS`: all
  7 status and 17 finding-code mappings and undefined-value exception behavior are
  exact; JSON model/property order/context is unchanged; static canonical readers
  remain source-generation/AOT-safe. Fresh local improvement review is
  `APPROVED — NO_MATERIAL_IMPROVEMENTS`; the one-file simplification is complete,
  removing duplicate mapping ownership without adding indirection.

  Fresh Purple assessment ran read-only from exact clean Blue `3f81e76` against the
  exact ten Find Child 3 test/support surfaces. The no-op Purple acceptance is
  recorded at exact `426d4f5` with verdict `NO_MATERIAL_IMPROVEMENTS`. No
  test/support, production, contract, project, package, configuration, generated,
  Route, Shell, or root file changed, and no Purple code/test commit was
  manufactured. Its focused Unit `206/206`, focused Integration/serialization
  `43/43`, and published Find EndToEnd `13/13` evidence passed with zero skips;
  source diff/check against `3f81e76` was clean/empty. No Native AOT claim was made,
  and the record-only commit was not a test change.

  Find Child 3 and the Find parent completed the recorded acceptance boundary.
  Their accepted feature tree was later squash-integrated into local `develop` at
  `1f03d16`; that completed continuation no longer controls current work.

- Generic predecessor: Generic CLI Improvements are Complete and squash-integrated into `develop` at `063c59d`, with exact tree equality to accepted feature tip `a107afe`. Their final gate passes managed Unit `580/580`, Integration `213/213`, and EndToEnd `57/57`; local `win-x64` Native AOT root with managed EndToEnd `57/57`; Native AOT Integration `213/213`; Native AOT EndToEnd `57/57`; package/artifact/public audits; and integrated review.
- Route Inspect/family modernization: Complete and accepted at exact commit
  `62a1dd9` (`Modernize Route Inspect nullable flow`). It changed exactly 32
  production C# files in
  `Commands/Route/Shared/**` and `Commands/Route/Inspect/**`; exact
  `RouteBinding.cs` and `RouteDefinitions.cs` were in scope and unchanged. No
  tests, projects, packages, dependencies, generated files, or configuration
  changed. Scoped directory-pathspec audits are ordinary postfix null suppressions
  `50 → 0` and `ArgumentNullException.ThrowIfNull` `113 → 83` (30 removed).
  Global authored production counts after Framework, Shell/root, and Route
  Inspect/family are `26` suppressions and `327` `ThrowIfNull` calls. Final managed
  verification and review are recorded in the Modern C# Improvements Task; this
  batch makes no Native AOT claim.
- Route List modernization: Complete and accepted at exact commit `273eb45`
  (`Modernize Route List nullable flow`) from exact predecessor `62a1dd9`
  (`Modernize Route Inspect nullable flow`).
  The exact scope is `Commands/Route/List/**`; exactly 18 production C# files
  changed. No tests, projects, packages, dependencies, generated files,
  configuration, or product contracts changed. No output or test expectation
  changed. Scoped ordinary postfix null suppressions are `26 → 0` and
  `ArgumentNullException.ThrowIfNull` is `136 → 106` (30 trusted internal
  duplicates removed). Global authored production is now `0` ordinary postfix null
  suppressions and `297` `ArgumentNullException.ThrowIfNull` calls after all four
  production batches.
  Route List Unit is `117/117`, Integration is `89/89`, and managed published
  `CliProcessTests` EndToEnd is `26/26`, with zero skips. This batch makes no
  Native AOT claim. The final managed, Native AOT, package, audit, and no-write
  gates follow the accepted tests/support mutation batch.
- Tests/support modernization: Complete and accepted at exact clean source commit
  `6af5fb1` (`Modernize test support nullable flow`) from exact predecessor `273eb45`
  (`Modernize Route List nullable flow`).
  Exactly 40 active test/support C# files changed: Unit 24, Integration 14,
  EndToEnd 1, and TestSupport 1. Production, project, package, dependency,
  generated, configuration, and product-contract surfaces, plus test identities,
  expectations, tiers, order, fixtures, and count, are unchanged. Test postfix
  suppressions are `169 → 8`, test `ArgumentNullException.ThrowIfNull` calls fell
  from `21` to `8`,
  and production remains at `0` suppressions and `297` guards. Full managed Unit
  `617/617`, Integration `241/241`, and EndToEnd `57/57` pass with zero skips.
  This batch makes no Native AOT claim; the final gate is recorded below.
- Route-inspect baseline and sequence: exact `edca509` (`Establish and accept route list`) on `feature/cli-route-inspect`; planning commit `37d2e70`; contracts/evidence (Complete) → resolution/promotion (Complete at `a54f4e0`) → profile (Complete at `c407e24`) → presentation (Complete from production commit `51c0960`) → behavior-neutral locality correction (Complete at `9c690b4`) → integrated acceptance and Route Discovery closeout (Complete).
- Maintainer-authorized continuation, completed through generic integration: Route Inspect and Generic CLI Improvements were squash-integrated into `develop`; parser remediation, reusable test fixtures, named component inputs, retained root/Core ownership, and the final full gate were accepted in order. The prerequisite for beginning the next product Task is met at `063c59d`.
- Completed generic-improvements Task: [Improve Generic CLI Structure](tasks/generic-improvements/_generic-improvements.md), integrated at `063c59d`. The read-only [Find](tasks/read-only/find.md) parent and Child 3 are now Complete and accepted in the commit containing this record update; Child 1 is Complete and accepted at exact commit `96fe413`, and Child 2 final acceptance is recorded at exact `ff7ce3f`. Child 3's phase history remains Preflight `28d316a`, Gray `a76a217`, original Red `22d3bff`, metadata corrections `6a9a0de` and `eea3d59`, supplemental escaping Red `a865fd1`, Green `cb7874c`, Blue `3f81e76`, and no-op Purple `426d4f5`. The final managed/native gate and package, artifact, static, no-write, Route/Shell/root, and generated-routing audits passed. Modern C# Improvements is Complete and accepted at `a1cbf09`.

### Modern C# Final Acceptance

The final gate passed from exact clean source commit `6af5fb1`. The Release build
was warning-free, format and diff checks passed, and informational `CA1062`,
`CA1510`, and `CA2264` diagnostics were zero. Full managed Unit, Integration, and
freshly managed-published EndToEnd passed `617/617`, `241/241`, and `57/57`;
focused Source/Route Unit and Integration passed `562/562` and `183/183`. The
local `win-x64` Native AOT root publication drove managed EndToEnd `57/57`
against the native root, the Native AOT Integration executable passed `241/241`,
and the Native AOT EndToEnd executable passed `57/57`; all runs had zero skips.

The managed and native public no-write fixture passed four invocations. Route List
returned exit `5` and `blocked`, and Route Inspect returned exit `3` and
`incomplete`, for both executables. Stderr was empty, JSON `command` and `status`
were typed, and byte/hash/entry snapshots were unchanged. The package audit listed
all six projects and found no vulnerable transitive package. The exact
root→Core, Unit→Core, Integration→Core+root+TestSupport, and EndToEnd→TestSupport
graph, one `.slnx`, six projects, 343 authored active C# files, reflection-disabled
JSON/source-generation/AOT settings, and artifact routing were unchanged. At that
historical acceptance boundary, no project-local `bin/obj` directories existed
and ignored output remained under `src/cli/artifacts/`. DX1 later moved the
solution controls and ignored output to the repository root.

Static audits report production suppressions `126 → 0`, production
`ThrowIfNull` `380 → 297`, active test suppressions `169 → 8` frozen intentional
injections, and test guards `21 → 8`. `required` is `97`, `init` is `122`,
nullable-analysis attributes are `14`, and active test identities are
`500/500` for `DisplayName`, `Feature`, and `Evidence`, with `103` argument
assertions. No forbidden nullable pragmas or `SuppressMessage` entries exist.
The changed-path audit from accepted Child 1 `96fe413` is exactly 121 authorized
paths: 112 C# plus these nine Working records. No project, configuration,
dependency, or generated path changed. Fresh integrated production correctness and
test/evidence reviews passed, and final improvement review is
`NO_MATERIAL_IMPROVEMENTS/PASS`. Optional shared projection/failure-reader
extraction is deferred because it is not a blocker and would reopen accepted
architecture or batches. The native claim is `win-x64` only; no six-RID parity or
`develop` integration is claimed, and no push occurred. Branch `feature/cli-find`
remains isolated while `develop` remains `e77902a`.

This Plan defines how the accepted replacement CLI reaches complete local and
release acceptance. The CLI Architecture and command contracts define what the
system means. Child Tasks define bounded outcomes and acceptance. This Plan
defines dependencies, order, integration gates, evidence, and resumption.

## Planning Basis

- Outcome: One complete, predictable, Native-AOT replacement executable and thin
  package wrappers implement every retained command without importing or falling
  back to the frozen MVP.
- Acceptance: Every command contract, cross-command invariant, filesystem and
  mutation safety boundary, Task 7's accepted x64 package graph and package
  journey, Task 13's later Linux-only D1 build and smoke, packed install and
  invocation, checksum, and release gate has reproducible evidence and
  maintainer acceptance.
- Find starting point: The exact integrated production/source tree is `063c59d`;
  it contains the accepted workspace, Foundation, Route Discovery, and Generic
  CLI Improvements but no Find production or test code. The parent planning
  boundary is recorded at `b2e3106` on `feature/cli-find`; `develop` remains
  `e77902a`. Later Child 1 execution baselines must remain distinct.
- Integration risk: High. The work crosses a complete command tree, filesystem
  identity, persisted lifecycle state, mutation and recovery, Native AOT, package
  distribution, and public release.
- Non-goals: Legacy compatibility, partial publication, runtime plug-ins, a fake
  filesystem, native interop, a universal domain engine, and implementation before
  its parent architecture and Task are ready.

### References And Authority

| Source                                                                                                       | Question it answers                                                     | Status or authority                                                        | Use in this Plan                            |
| ------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------- | -------------------------------------------------------------------------- | ------------------------------------------- |
| [CLI Architecture](../../crystallized/documents/cli/architecture.md)                                         | How is the replacement structured and integrated?                       | Accepted current architecture                                              | Governs all steps                           |
| [Command Contract Set](../../crystallized/documents/cli/command-contract-set.md)                             | Which command-local sources define behavior?                            | Accepted current contract map                                              | Selects command Tasks                       |
| [Detailed Contracts](../../crystallized/documents/cli/contracts/_contracts.md)                               | What must each command and shared operation do?                         | Accepted current product contracts                                         | Requirements and evidence                   |
| [Shared Operation Contract](../../crystallized/documents/cli/shared-operation-contract.md)                   | Which conventions cross commands?                                       | Accepted current contract                                                  | Foundation and integration                  |
| [Program Architecture Directive](../../../directives/program-architecture.md)                                | Who owns architecture and when is delegation ready?                     | Binding workspace Directive                                                | Task readiness and integration              |
| [CLI Directive](../../../directives/open-forge/cli/_cli.md)                                                  | What rules apply to replacement work?                                   | Binding CLI Directive                                                      | Every CLI step                              |
| [Architectural Perspectives](../../../guidance/architectural-perspectives.md)                                | Which top-down and task-master questions apply?                         | Accepted Guidance for this program                                         | Planning and review horizon                 |
| [Task Template](../../../templates/memory/task.md)                                                           | Which fields make one Task durable?                                     | Experimental local Template                                                | Child Task shape                            |
| [Plan Template](../../../templates/memory/plan.md)                                                           | Which fields make coordinated execution resumable?                      | Experimental local Template                                                | This Plan's shape                           |
| [Implementation Reset](../../archived/cli-release/implementation-reset-2026-08-21.md)                        | What was useful or harmful in the removed implementation?               | Historical evidence                                                        | Avoid rediscovery and sunk-cost restoration |
| Maintainer-supplied review files under `.temp/review-20.08.2026/`                                            | What did the independent architecture review find?                      | Historical external evidence accepted where projected into current sources | Task detail and risk checks                 |
| [Replacement CLI edge-case ledger](edge-cases.md)                                                            | Which deferred edge cases need owner resolution or explicit acceptance? | Active working evidence                                                    | Route discovery and delivery gate tracking  |
| [CLI development flow evaluation](../../emerging/observations/2026-08-21_cli-development-flow-evaluation.md) | Which current-flow lessons should shape later slices?                   | Contextual Emerging observation                                            | Route-inspect planning and review           |

## Approach

Complete the program top down:

1. Freeze the physical workspace, project graph, dependency direction, and shell
   call surfaces.
2. Author the complete Task hierarchy before production source returns.
3. Implement the actual route-free foundation in one integrated architecture
   increment owned by the Mastermind.
4. Close shared safety and Framework fact foundations before commands consume
   them.
5. Implement read-only commands and the pure GN1 projection foundation in
   dependency order. Establish neutral mechanical
   foundations at the nearest shared scope when several accepted program outcomes
   require them, even when implementation order exposes one consumer first;
   promote semantic facts only after consumers prove identical meaning. Find's
   source catalogue is sequential; no shared source mutation runs in parallel.
   References may begin only after the source and document facts it needs are
   accepted.
6. Establish M1 lock, lifecycle, mutation, and external recovery-bundle
   foundations before the first mutating command.
7. Implement public Index, including the I1-owned body-free formation expansion,
   on accepted GN1 projection and M1 mechanics.
8. Implement mutations from narrow route operations to extension and root
   lifecycle operations.
9. Implement aggregate status, diagnosis, repair, and cleanup only after all state
   producers exist. After Task 14 acceptance, the Task 7 package-expansion
   follow-up ran beside Status where the paths did not overlap and is complete at
   phase 4/4, milestone 7/7. Its Linux host journey passed; Darwin and Windows
   have stage-and-pack evidence only.
10. Complete the queued last-stage Task 23 Workspace Libraries and Task 24
    Extensions Evolution after Task 20 in that order, with a distinct maintainer
    review and contract freeze before each implementation. Keep Task 13's later
    D1 boundary to Linux-only build and smoke, packed install and invocation,
    and checksum evidence, then complete final acceptance for the full accepted
    x64 graph without partial publication.

Each step ends in one inspectable commit. Architecture and cross-cutting callable
contracts stay with the Mastermind. A smaller implementer receives one closed
child Task and exact predecessor outputs. A reviewer receives the exact commit or
diff, parent requirements, and claimed evidence.

Operational aggregate domains use one explicit immutable application-scoped
`OperationalContributorCatalogue` built by `CliCompositionRoot`. Producer-owned
typed contributors project narrow Status and Doctor views from fresh invocation
observations. They do not use dependency injection, a service locator,
reflection, a runtime registry, a generic operational engine, or ambient
registration. Task 12 owns the durable architecture rewrite; Task 15 Gray owns
the exact contributor and view signatures. Composition alone does not change
Status or Doctor public contracts or the persisted operational-command order.

## Prerequisites

| ID  | Prerequisite           | Required state and evidence                                                                           | Responsible source or role        | Blocks                        |
| --- | ---------------------- | ----------------------------------------------------------------------------------------------------- | --------------------------------- | ----------------------------- |
| P1  | Product contracts      | Current contract route is complete and conflicts are explicit                                         | Crystallized CLI contracts        | All command Tasks             |
| P2  | Greenfield boundary    | Old production removed and useful evidence preserved                                                  | Commit `40ba03e` and reset record | Foundation                    |
| P3  | Architecture           | Physical, project, dependency, call-surface, evidence, and sequence boundaries accepted               | CLI Architecture                  | Task authoring and foundation |
| P4  | Task governance        | Top-down and task-master perspectives are binding                                                     | Program Architecture Directive    | Delegation                    |
| P5  | Local toolchain        | Stable .NET 10 SDK and native prerequisites are available                                             | Foundation verification           | Foundation acceptance         |
| P6  | Find planning boundary | Current Find contract/Working packet passes targeted reviews and is recorded as accepted at `b2e3106` | Find Task and Mastermind          | Q1 readiness                  |

## Resources

| Resource                           | Purpose                                                | Availability or source | Needed by              | Responsible role |
| ---------------------------------- | ------------------------------------------------------ | ---------------------- | ---------------------- | ---------------- |
| Stable .NET 10 SDK                 | Build, test, format, publish, and AOT                  | Local and CI setup     | F1 onward              | Mastermind       |
| Current `linux-x64` native runner  | Accepted build and invocation smoke evidence           | Local or CI            | D1                     | Release Task     |
| Real OS temporary filesystems      | Identity, containment, mutation, and no-write evidence | TestSupport            | F4 and commands        | Owning Task      |
| Recovery-bundle fixtures           | Mutation and interruption evidence                     | Isolated filesystems   | M1 onward              | Owning Task      |
| Preserved test inventory           | Candidate expectations and fixtures                    | `src/cli/tests/`       | Relevant command Tasks | Task creator     |
| Independent advisors               | Named architecture or safety uncertainty only          | Optional               | Decision points        | Mastermind       |
| Bounded implementers and reviewers | Closed implementation and fresh diff review            | After Task readiness   | Commands               | Mastermind       |

## Work Graph

| ID   | State                                                                                                                                                                                                                                         | Action and observable result                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Depends on                                  | Lane                                                   | Task group                              | Verification                                                                                                                                                                                                                                                                                                                                 |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------- | ------------------------------------------------------ | --------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| S0   | Complete                                                                                                                                                                                                                                      | Preserve WIP, define architecture delegation rules, and reset old production                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | None                                        | Sequential                                             | Historical                              | Commits `4b873de`, `aa7d178`, `40ba03e`                                                                                                                                                                                                                                                                                                      |
| S1   | Complete                                                                                                                                                                                                                                      | Define the complete top-down Architecture and executable Plan                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | S0, P1-P4                                   | Sequential                                             | Program                                 | Current sources and link checks                                                                                                                                                                                                                                                                                                              |
| S2   | Complete                                                                                                                                                                                                                                      | Author the complete hierarchical Task set with closed foundation and command boundaries                                                                                                                                                                                                                                                                                                                                                                                                                                                        | S1                                          | Sequential                                             | `tasks/`                                | Task graph audit and backlinks                                                                                                                                                                                                                                                                                                               |
| F1   | Complete                                                                                                                                                                                                                                      | Create the scoped C# workspace, project graph, dependencies, artifacts, and preserved-test quarantine                                                                                                                                                                                                                                                                                                                                                                                                                                          | S2, P5                                      | Sequential                                             | Foundation                              | Restore/build topology and no root C# files                                                                                                                                                                                                                                                                                                  |
| F2   | Complete                                                                                                                                                                                                                                      | Implement Shell definitions, invocation, composition contracts, parser, pipeline, output, and serialization with no command                                                                                                                                                                                                                                                                                                                                                                                                                    | F1                                          | Foundation                                             | Foundation                              | Unit and integration evidence                                                                                                                                                                                                                                                                                                                |
| F3   | Complete                                                                                                                                                                                                                                      | Implement the thin root host and explicit route-free composition                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | F2                                          | Foundation                                             | Foundation                              | Managed process and terminal evidence                                                                                                                                                                                                                                                                                                        |
| F4   | Complete                                                                                                                                                                                                                                      | Implement workspace, filesystem identity, typed reads, and physical-containment foundations                                                                                                                                                                                                                                                                                                                                                                                                                                                    | F2                                          | Foundation                                             | Safety foundation                       | Real-OS matrix including leave-and-reenter                                                                                                                                                                                                                                                                                                   |
| F5   | Complete                                                                                                                                                                                                                                      | Establish active Unit, Integration, EndToEnd, TestSupport, Native AOT, and CI foundations                                                                                                                                                                                                                                                                                                                                                                                                                                                      | F1-F4                                       | Sequential                                             | Foundation                              | Managed and published process evidence                                                                                                                                                                                                                                                                                                       |
| G1   | Complete                                                                                                                                                                                                                                      | Accept the actual command-free architectural foundation                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | F1-F5                                       | Sequential                                             | Foundation gate                         | Full diff, dependency audit, AOT execution                                                                                                                                                                                                                                                                                                   |
| R1   | Complete                                                                                                                                                                                                                                      | Accept the complete `route list` slice and keep shared route facts local until route inspect proves identical consumers                                                                                                                                                                                                                                                                                                                                                                                                                        | G1                                          | Sequential                                             | Route discovery                         | Complete contract and public scenario                                                                                                                                                                                                                                                                                                        |
| R2   | Complete                                                                                                                                                                                                                                      | Split and close route-inspect child Tasks before implementation; then implement and accept `route inspect` and promote proved shared route facts                                                                                                                                                                                                                                                                                                                                                                                               | R1                                          | Sequential                                             | Route discovery                         | List and inspect regressions                                                                                                                                                                                                                                                                                                                 |
| GI1  | Complete                                                                                                                                                                                                                                      | Remediate parser behavior, improve active-test architecture, and accept the closed callable/root-host structure before the next product Task                                                                                                                                                                                                                                                                                                                                                                                                   | R2                                          | Sequential                                             | Generic improvement                     | Beginning/final complete suites and focused phase evidence                                                                                                                                                                                                                                                                                   |
| Q1   | Complete                                                                                                                                                                                                                                      | Complete Find's source catalogue, corrected query operation, and presentation children in order. Children 1 and 2 are accepted, and Child 3 plus the Find parent are Complete and accepted in the commit containing this record update. The no-op Purple acceptance is recorded at exact `426d4f5`; the final managed/native gate and package, artifact, static, no-write, Route/Shell/root, and generated-routing audits passed.                                                                                                              | GI1, P6                                     | Sequential                                             | Source queries                          | Contract, CommonMark, process, AOT                                                                                                                                                                                                                                                                                                           |
| GI2  | Complete                                                                                                                                                                                                                                      | Complete the accepted behavior-neutral Modern C# Improvements batches and final managed, Native AOT, package, audit, and public no-write gates.                                                                                                                                                                                                                                                                                                                                                                                                | Q1 Child 1 acceptance                       | Sequential                                             | Generic improvement                     | Final gate and integrated reviews pass                                                                                                                                                                                                                                                                                                       |
| DX1  | Complete                                                                                                                                                                                                                                      | Move .NET workspace controls and artifacts to the repository root, make ordinary EndToEnd builds publish and discover `open-forge-dev`, remove obsolete preserved tests, and repair temporary compatibility-name routing.                                                                                                                                                                                                                                                                                                                      | Q1, GI2                                     | Sequential                                             | Developer workflow                      | Root build/test, environment-free EndToEnd, routing, review                                                                                                                                                                                                                                                                                  |
| Q2   | Complete                                                                                                                                                                                                                                      | Public Red `43b75f3`, Green `23d5e2e`, managed `1270/1270`, public no-write, and supported local `linux-x64` root, Integration, and EndToEnd Native AOT execution pass.                                                                                                                                                                                                                                                                                                                                                                        | GI1, Q1 source/document acceptance, DX1     | Sequential                                             | Source queries                          | Contract, process, no-write, and Native AOT execution pass                                                                                                                                                                                                                                                                                   |
| Q3   | Complete                                                                                                                                                                                                                                      | Implement and accept `context`; accepted tip `303ad7d` is squash-integrated at `ca097a2` and retained in the combined managed/native baseline.                                                                                                                                                                                                                                                                                                                                                                                                 | Q1, Q2                                      | Sequential                                             | Context                                 | Ordered context and exact content evidence passes                                                                                                                                                                                                                                                                                            |
| E1   | Complete                                                                                                                                                                                                                                      | Extension List is Complete at `db0d39a`; Extension Inspect accepted rebased feature `2b1e63d` is squash-integrated at `73b01be`, with combined managed `1054/378/116` and portable `linux-x64` Native AOT `378/116` acceptance.                                                                                                                                                                                                                                                                                                                | G1, GI1                                     | Read-only C                                            | Extension discovery                     | Catalogue, package-source, lifecycle, process, and AOT                                                                                                                                                                                                                                                                                       |
| RM1  | Complete                                                                                                                                                                                                                                      | Promote the neutral routed authored-metadata fact while retaining `SourceDocumentForm` as the sole source classification authority; accepted feature `8a29321` is integrated at `5924698`.                                                                                                                                                                                                                                                                                                                                                     | Q1-Q3, R2                                   | Sequential                                             | Source metadata                         | Route preservation, metadata grammar, and Native AOT pass                                                                                                                                                                                                                                                                                    |
| GN1  | Complete                                                                                                                                                                                                                                      | Pure Generated Navigation projection and bounded-region facts are accepted and squash-integrated at `21e5200`; full managed and local `linux-x64` Native AOT Integration evidence passes.                                                                                                                                                                                                                                                                                                                                                      | Q1-Q3, R2, RM1                              | Read-only D                                            | Generated navigation                    | Determinism, exact bytes, no-write, and affected regressions                                                                                                                                                                                                                                                                                 |
| DGC1 | Complete                                                                                                                                                                                                                                      | Correct form-aware Context Skill metadata, source-specific compact Context/Find findings, and stale Route Inspect Context help without changing structured command meaning; accepted feature candidate `deb3f14` is squash-integrated at `bba84b6`.                                                                                                                                                                                                                                                                                            | Q1-Q3, R2, RM1, GN1                         | Sequential                                             | Dogfooding correction                   | Managed `1031/411/120` and Native AOT `411/120` pass                                                                                                                                                                                                                                                                                         |
| PC1  | Complete                                                                                                                                                                                                                                      | Proportionate guidance `5f9f59e`, Route List correction `2cd525d`, and Markdown correction `0d88606` are squash-integrated; PCF-001/002/003/007 are closed and all remaining dispositions are preserved.                                                                                                                                                                                                                                                                                                                                       | DGC1                                        | Parallel correction                                    | Proportional correctness                | Release 0/0; managed `1053/411/120`; portable `linux-x64` Native AOT `411/120`; dogfood and Sol/xhigh review pass                                                                                                                                                                                                                            |
| M1   | Complete                                                                                                                                                                                                                                      | Shared lock, lifecycle, revalidation, atomic application, receipt, neutral recovery catalogue, lease-gated deletion guard, and external recovery-bundle foundations are accepted at exact production candidate `e7d937f` under authority `01dd552`. All-five-root-key creation, Framework-created complete empty Extensions, and fail-closed existing incomplete-state planning are integrated at `1d404c5`; focused/full managed, Native AOT dogfood, diff, and review gates pass.                                                            | GN1, E1, DGC1, PC1                          | Sequential                                             | Mutation foundation                     | Direct failure, interruption, residual, lifecycle-envelope, and AOT evidence pass                                                                                                                                                                                                                                                            |
| I1   | Complete                                                                                                                                                                                                                                      | Public `index`, including the I1-owned body-free formation expansion and orchestration over accepted GN1 projection and M1 mechanics, is accepted at exact feature candidate `4e89d945b38a2d1e24600dd22789b55e4395a534`. The warning-free Release, complete managed, portable `linux-x64` Native AOT, and controlled public-process gates pass with zero failures or skips. Final closeout tip `2b353c48978ee88e53345be8037776181612222c` is locally squash-integrated at `09aa03eddb97831ff544afe1eac54ad9af501f5c` with exact tree equality. | GN1, M1                                     | Sequential                                             | Generated navigation                    | Exact contract, idempotence, unchanged-authority, recovery, managed `1206/480/125`, and portable `linux-x64` AOT `480/125` pass                                                                                                                                                                                                              |
| D0   | Complete at `38e1498`                                                                                                                                                                                                                         | Freeze the accepted next-wave Architecture, command contracts, shared-foundation Tasks, command sequencing, checkpoint, and public docs before production mutation.                                                                                                                                                                                                                                                                                                                                                                            | I1, M1, maintainer decisions                | Sequential                                             | Program contract                        | Link/frontmatter/stale-claim checks, dogfood load/doctor, independent Sol/xhigh review                                                                                                                                                                                                                                                       |
| SF1  | Complete at `e782090`                                                                                                                                                                                                                         | Add one BCL-only Shell interaction transport. Protected command integration later injects it only into prompt-capable operation factories; requests retain only command-local interaction-policy Booleans.                                                                                                                                                                                                                                                                                                                                     | D0, F2-F3                                   | Parallel foundation                                    | Shell interaction                       | Focused transport Unit; injected host and redirected-process proof with prompt-capable command integration                                                                                                                                                                                                                                   |
| SF2  | Complete at `680915a`                                                                                                                                                                                                                         | Embed the canonical Framework payload and expose exact ordered asset bytes, paths, hashes, and inventory identity through a neutral BCL reader.                                                                                                                                                                                                                                                                                                                                                                                                | D0, F1                                      | Parallel foundation                                    | Framework distribution                  | Source parity, isolated published binary, Native AOT resource proof                                                                                                                                                                                                                                                                          |
| SF3  | Complete at `0989356`                                                                                                                                                                                                                         | Add required nullable per-target `sourceAssetPath` provenance to Framework lifecycle schema v1 without adding instance collections or migration machinery.                                                                                                                                                                                                                                                                                                                                                                                     | D0, M1                                      | Parallel foundation                                    | Framework lifecycle                     | Source-generated JSON, structural validation, lifecycle and Native AOT regressions                                                                                                                                                                                                                                                           |
| SF4  | Complete at `33913df`; lock-location correction integrated with C2 at `c60fcb98`                                                                                                                                                              | Add one lease-bound ordinary-BCL directory effect for Install and Route Init; the later shared correction makes missing `.agents` an ordinary first effect after external lease acquisition.                                                                                                                                                                                                                                                                                                                                                   | D0, M1                                      | Parallel foundation                                    | Mutation directory effect               | Real filesystem races, residuals, file/recovery non-regression, Native AOT proof                                                                                                                                                                                                                                                             |
| C1   | Complete at protected public integration `4c85d1d62004e8bdc885c51873ff6d9cdb6e6db5`; exact candidate `789cc917f2d0cb38c5229cc2dc7fee013218d341`, tree `fa29bd9572df39b2d5457c35bb8a0bd6ba5a9945`                                              | Implement Extension Create with accepted manifest defaults/options, catalogue boundary, ordered JSON result, and a command-local wizard for missing required facts.                                                                                                                                                                                                                                                                                                                                                                            | SF1, M1, E1                                 | Parallel command                                       | Lifecycle mutation                      | Managed `1356/575/132`; `linux-x64` Native AOT root/version/ELF and `575/132`; dogfood and Sol/xhigh review pass                                                                                                                                                                                                                             |
| C2   | Complete at local integration `c60fcb98a57e9ec80769b9cb1d399ce13a227863`; final candidate `11994e4d21ddc807b7480afc39ae3612e5a69a56`, exact tree `464a4a6b6ef6447209edffbf53df7348c70691ed`                                                   | Implement root Install over the closed embedded base Framework subset while preserving trusted dynamically added scoped lifecycle targets and forming the accepted exact public JSON result, including typed residual values `none`, `retained`, and `unknown`.                                                                                                                                                                                                                                                                                | SF1-SF4, M1, I1                             | Parallel command                                       | Framework lifecycle                     | Release `0/0`; managed `1390/598/136`; `linux-x64` Native AOT `598/136`; focused post-rebase, dogfood, and two Sol/xhigh rechecks                                                                                                                                                                                                            |
| C3   | Complete and squash-integrated at `fa3db1ee` with exact tree equality to final reviewed candidate `37c9360`                                                                                                                                   | Correct Route Inspect's accepted one-answer interactive collision selection without changing its non-interactive contract or result model.                                                                                                                                                                                                                                                                                                                                                                                                     | SF1, R2                                     | Parallel command plus sequential protected integration | Route discovery correction              | Focused `131/8/82/32`, full managed `1284/511/125`, Native AOT `511/125`, no-write dogfood, and review pass                                                                                                                                                                                                                                  |
| M2   | Complete: Task 5 “Route Remove” phase 5/5, milestone 8/8; accepted lane `ab8da620`, tree `bb41e1c9`; squash integration `5a2e650a`, same tree                                                                                                 | Deliver positive-unmanaged Route Remove with exact dependency, reference, navigation, recovery, and fresh postcondition integrity while leaving managed release meaning deferred.                                                                                                                                                                                                                                                                                                                                                              | C2, M1, R2, I1, QR1, O1A                    | Sequential route mutation                              | Route mutation                          | Release `0/0`; managed `1797/966/178`; managed-on-native `178`; native `966/178`; exact public Route Remove `3/3`; review and structural gates pass                                                                                                                                                                                          |
| QR1  | Complete and squash-integrated at `862cbf2a847b5adac77c6923e51f2c28c315415f`, exact tree `571f104f507c3f72b7404cc0dacd86c818ec0a2e`, from accepted implementation candidate `a4ccf19a`, exact tree `97254e655f51ab421dacc8eff7a8c93f726f2625` | Closed all thirteen accepted findings/candidates, including final `QR-R1-001` evidence-tier correction and same-reviewer narrow revalidation.                                                                                                                                                                                                                                                                                                                                                                                                  | M2 Route Create integration, C2, M1, R2, I1 | Sequential quality remediation                         | Architecture/design/evidence correction | Final managed `1522/736/146`; supported Native AOT Integration `736/736`; focused, format, diff, and Sol/xhigh acceptance pass                                                                                                                                                                                                               |
| M3   | Complete: Task 6 “Root Update” phase 5/5, milestone 8/8; final candidate `ac96f4cc57550a83ef8651b40869ae4ff35da34e`, tree `195f15388d6251f69244946183209d3dfe86b24a`; squash integration `c6eec9d2`, exact tree `f31cacb0`                    | Complete the accepted root Update boundary without transplanting historical provisional Gray `ed614488` or Red `69a1bd83`; T6-R1 and one logical T6-C1 are consumed. Post-integration Release and public Update `3/3` evidence passed.                                                                                                                                                                                                                                                                                                         | M2, C2, M1, I1                              | Sequential lifecycle mutation                          | Lifecycle mutation                      | Accepted activation, Preflight, Gray, addendum, Red, Green, T6-C1 lineage, integration adoption, and post-integration managed/public evidence pass                                                                                                                                                                                           |
| M4A  | Complete at phase 5/5, milestone 8/8; lane `a6b44f07`, tree `cd4c074d`, squash-integrated at `20807781`, tree `4592a139`                                                                                                                      | Implement Task 14 Extension Install as the first adoption command. One Brilliant Implementer owned coherent production and focused verification under its Task Mastermind. Unfinished Route Remove and root Update were not prerequisites.                                                                                                                                                                                                                                                                                                     | Task 12, C2, M1, I1                         | Prioritized lifecycle mutation                         | Extension mutation                      | Final managed `1711/917/169`; Native `917/169`; managed-on-native `169`; offline package journey                                                                                                                                                                                                                                             |
| T7   | Complete at phase 4/4, milestone 7/7; accepted lane `a2942781`, tree `fe36fc3f`; squash-integrated at `e19d429e`, the same tree                                                                                                               | Completed the accepted x64 package graph. The Linux host `PackageEndToEnd` journey passed; Darwin and Windows have stage-and-pack evidence only. ARM, publication, and live link or unlink remain unproven and unauthorized.                                                                                                                                                                                                                                                                                                                   | M4A                                         | Parallel adoption slice with O1A where safe            | Distribution                            | Main/optional graph, host staging/packing, launcher reachability/forwarding/completion; no live global link/unlink                                                                                                                                                                                                                           |
| O1A  | Complete: Task 15 and Task 16 are both phase 5/5, milestone 8/8                                                                                                                                                                               | Preserve the explicit producer-owned contributor inventory. Task 16's accepted 108-kind horizon supplies producer-backed emissions for all 19 Extension kinds; Task 17 closed the bridge-registration observation and no Extension observation horizon remains.                                                                                                                                                                                                                                                                                           | M4A                                         | Sequential adoption slice                              | Operations                              | Doctor managed `1768/937/175`; native `937/175`; managed-on-native `175`; exact public Doctor `3/3`; every gate had zero failures/skips and the Release build had zero warnings/errors                                                                                                                                                       |
| M4B  | Task 17 “Extension Update” complete and dequeued; Task 18 “Extension Remove” active at phase 2/5, milestone 1/8 with Gray active                                                                                                                | Complete Task 17, then execute active Task 18 Extension Remove. Task 17's accepted bridge-registration observation is already in the contributor inventory; Task 18 has no installed-manifest scan or Doctor producer obligation.                                                                                                                                                                                                                                                                                               | M3, M4A, O1A                                | Sequential lifecycle mutation                          | Extension mutation                      | Accepted Task 17 candidate `3bcb6025`, tree `63f5b22c`; integrated at `ae055a73`, same tree, from parent `a9987d5c`; final gate basis `d3c29a2b`, tree `18f76ed4`; Task 18 Preflight base `5cabb10d`, tree `fb16fed3` |
| O2   | Pending behind Task 18                                                                                                                                                                                                                         | After active Task 18, implement relevant-domain Task 19 Repair and Task 20 Cleanup; Task 23 follows as the first queued last-stage improvement.                                                                                                                                                                                                                                                                                                                                                                                                        | M2-M4B, O1A                                 | Sequential                                             | Operations                              | Complete inventory, plan/apply/recovery, cleanup, and idempotence evidence                                                                                                                                                                                                                                                                   |
| T23  | Queued after Task 20                                                                                                                                                                                                                             | Prepare and implement Workspace Libraries after a distinct maintainer review and contract freeze for the final drafted contracts and CLI package. Bounded read-only Preflight and draft preparation may run now; shared semantic implementation and integration remain sequential.                                                                                                                                                                                                                                                                    | O2, accepted Library design                     | Sequential last-stage improvement                      | Workspace Libraries                     | Link-aware Route Update/Index/Move/Remove guard, real-filesystem regression, library contract, lifecycle, recovery, and exact three-journey evidence                                                                                                                                                                                          |
| T24  | Queued after Task 23                                                                                                                                                                                                                             | Prepare and implement Extensions Evolution after a distinct maintainer review and contract freeze for the final drafted contracts and CLI package. Bounded read-only Preflight and draft preparation may run now; shared semantic implementation and integration remain sequential.                                                                                                                                                                                                                                                                    | T23, accepted Extensions idea                    | Sequential last-stage improvement                      | Extensions Evolution                   | Accepted current lifecycle boundary, representative limitation, smallest new contract, adversarial evidence, and exact command-owned tests                                                                                                                                                                                                    |
| D1   | Preparation complete; implementation explicitly postponed                                                                                                                                                                                     | Keep Task 13's CI and artifact work to the later Linux-only D1 native build and smoke, checksums, and bounded artifact collection after the retained command sequence and any explicitly reopened Task 10/21 work. Task 7 owns the accepted x64 package graph and journeys; Task 22 owns complete-graph acceptance and release.                                                                                                                                                                                       | O2, T7, Task 10 and accepted remediation    | Later sequential delivery                              | Distribution                            | Linux D1 native artifacts, checksums, bounded collection; no macOS, Windows, or ARM expansion                                                                                                                                                                                                                                                |
| A1   | Pending                                                                                                                                                                                                                                       | Run final whole-program acceptance and local integration for the complete accepted x64 graph                                                                                                                                                                                                                                                                                                                                                                                                                                                   | D1, T7                                      | Sequential                                             | Acceptance                              | Maintainer acceptance; no partial release                                                                                                                                                                                                                                                                                                    |

### Post-M1 Durable Queue

Public Index is Complete in exact feature candidate
`4e89d945b38a2d1e24600dd22789b55e4395a534` after its authority boundary was
explicitly selected following proportional prerequisite `b25d76e`. Its final
closeout is locally integrated with exact tree equality. Preserve these accepted
program items through the later boundaries that own them:

- [x] Author and accept the binding CLI proportionality and evidence Directive,
      and add its per-Task applicability check below before I1 Phase 0 activation;
      satisfied at `b25d76e`.
- [x] Keep generic Route Init as exact-chain initialization and add the accepted
      Framework-aware concrete-route mode after root Install. The accepted mode
      uniquely aligns canonical Framework segments, deterministically slugs only
      ID-form inserted scopes, creates one sparse chain, and keeps scope entrypoints
      user-owned.
- [x] Accept and integrate the existing Generated Navigation formation expansion
      at feature `f82c2b168657baf2fac50c76e2ff2cc0ed3776d7`, squash commit
      `cc35c853fd7b55d31e3fb2c9a454d9ab61c1884e`, with closeout integrated at
      `18f2acff31cfd6600a16430ac5d689d05482e297`, exact tree
      `39f0a8c4e6d3695cfbe7407dfd6043dc5ec9680a`. Route
      Init/Create/Update/Move/Remove and root Install/Update consume its accepted
      generated-navigation formation or projection capabilities. Extension Create
      remains independent of it.
- [x] Route Move completed its separate command-local reference and lifecycle
      proportionality gate and is integrated.
- [x] Route Remove completed its separate command-local reference and lifecycle
      proportionality gate and is integrated with completion grace consumed.
- [x] Accept Extension Create's manifest options and command-local missing-fact
      wizard. Invalid input may be corrected locally without an attempt limit; EOF
      is invalid and cancellation interrupted, both without writes. C1 was sequenced
      after SF1 and does not gain workspace or dependency-resolution behavior.
- [x] Freeze Extension Create's exact defaults, catalogue-parent and sibling
      eligibility, and command-local JSON result shape/order.
- [x] Freeze C3's one-answer prompt grammar, Install's one-confirmation
      interaction, and the shared lease-bound ordinary-BCL directory-create effect.
- [x] Supersede the earlier bootstrap boundary: acquire the persistent external
      zero-byte lock under `LocalApplicationData/OpenForge/locks/v1`, then apply
      missing `.agents` as the first ordinary visible planned/reported lease-bound
      directory effect. Lock and recovery use separate versioned subtrees.
- [x] Preserve Architecture order: complete Route Mutation M2 before root Update
      M3. Extension Install is independently prioritized after the integrated
      Task 12 boundary, followed by incremental Status and Doctor over the exact
      frozen producer inventory. Parallelize only independent preparation until
      each mutation dependency is satisfied.
- [x] Keep the delivery ownership split at the accepted thin tier: Task 7 owns
      the one-main-plus-three-optional-x64 package graph and applicable-host
      package journeys after Task 14; Task 13 owns the later Linux-only D1
      build, smoke, checksums, and bounded artifacts; and Task 22 owns final
      acceptance and separately authorized atomic publication of the complete
      x64 graph. ARM remains undecided, and signing, SBOM, OIDC, provenance,
      attestation, and support-floor matrices remain outside current evidence.
- [ ] Keep the active command order as Task 18 Extension Remove, then Task 19
      Repair, then Task 20 Cleanup. Queue Task 23 Workspace Libraries and Task
      24 Extensions Evolution after Task 20 in that order. Bounded read-only
      Preflight and draft preparation may run now; shared semantic
      implementation and integration remain sequential.
- [x] Keep the simplified O1 Status/Doctor and O2 Repair/Cleanup split reflected
      in the work graph. Task 17 closed the last accepted Extension Doctor
      observation; Task 18 has no Doctor producer obligation.
- [ ] Keep Tasks 10, 21, 13, and 22 explicitly postponed outside the active
      command sequence. Keep queued Tasks 23 Workspace Libraries and 24
      Extensions Evolution after Task 20 without phase or milestone horizons;
      require a distinct maintainer review and contract freeze for each final
      drafted contract and CLI package before implementation.

This queue does not reopen M1. D0 and SF1-SF4 are complete at the integrated
baseline recorded above. C1 is Complete at protected public integration
`4c85d1d62004e8bdc885c51873ff6d9cdb6e6db5`, with exact candidate
`789cc917f2d0cb38c5229cc2dc7fee013218d341` and tree
`fa29bd9572df39b2d5457c35bb8a0bd6ba5a9945`. C2 is Complete at local
integration `c60fcb98a57e9ec80769b9cb1d399ce13a227863`, exact tree
`464a4a6b6ef6447209edffbf53df7348c70691ed`. Its exact fully present ordered
public Install JSON result is accepted and frozen, including typed residual
values `none`, `retained`, and `unknown`. Route Init is Complete at `cc5085ce`,
and Route Create is Complete at `19412d2`, exact tree `2bbba7e`. Route Create
supplies the `RouteCreateJsonContext` and Route-help predecessor slices. The
separate CLI Quality Remediation Task is Complete and squash-integrated at
`862cbf2a`, exact tree `571f104f`, from accepted implementation candidate
`a4ccf19a`, tree `97254e65`. Task 3 “Route Update” is Complete at accepted
closeout `27df8325`, tree `b68d4349`. The
M1 lifecycle correction is integrated at `1d404c5`, and the
neutral Markdown link-label prerequisite is integrated at `89a35a7`. Later
commands still wait for their listed predecessors and any named decision
frontier.

### Per-Task Proportionality And Evidence Check

Every CLI implementation Task records this check in its Execution Capsule before
mutation. It selects evidence after the CLI scope is routed and does not create
a second Directive or route-loading applicability gate.

| Check                                           | Required record                                                                                                                                                                                                                                                |
| ----------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Consequence, reversibility, and threat boundary | Affected users, data, and systems; practical recovery; ordinary failure modes; and the accepted cooperating-process boundary, including what a malicious same-user actor can defeat.                                                                           |
| Standard capability and platform sufficiency    | The pinned runtime/BCL/platform path that proves the requirement, or the exact unmet accepted guarantee that must return to Architecture.                                                                                                                      |
| Shared-foundation reuse                         | Accepted neutral capabilities and facts reused, local semantic policy, and any additional accepted consumer that justifies promotion.                                                                                                                          |
| Exceptional machinery                           | `none` by default; otherwise the accepted requirement, bounded exception, maintainer decision, evidence, documentation impact, and removal or re-evaluation condition.                                                                                         |
| Cheapest decisive evidence                      | The lowest Unit, Integration, EndToEnd, or PackageEndToEnd tier that proves each behavior, plus directly affected regressions and explicit contract/Architecture evidence.                                                                                     |
| Complete managed/AOT gate trigger               | First golden slice for an archetype, integration wave/shared promotion, or material public, composition, shared-capability, safety, serializer, dependency/runtime/toolchain, project/build/package, or release change. Record `none` when no trigger applies. |

Focused leaf evidence is the default. Run one complete managed suite and
supported Native AOT gate at the recorded golden-slice, integration-wave, or
material trigger, not for every leaf by default. An explicit Architecture,
contract, or Task requirement remains binding. An unchanged exact predecessor may
supply the beginning baseline when its projects, executable, environment, counts,
and result are recorded. Reassess the check if evidence changes consequence,
reversibility, trust boundary, compatibility, or likely harm.

### Parallel Lanes

Parallelism begins only after the shared predecessor is committed and each lane
has non-overlapping production and test ownership.

| Lane             | Steps                                                  | May start when                                                                                                                                                                                                                                                                                                                         | Owned surfaces                                                                                                                                             | Shared dependency                                                                     | Integration point                                                                                                           |
| ---------------- | ------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| Foundation       | F2, F4 preparation                                     | F1 complete; callable contracts frozen by Mastermind                                                                                                                                                                                                                                                                                   | Separate Shell and Framework capability paths                                                                                                              | Core models and project graph                                                         | F5                                                                                                                          |
| Find             | Q1                                                     | GI1 and P6 complete; Child 1 and Modern C# are accepted; Child 2 records its focused Preflight before mutation                                                                                                                                                                                                                         | Find source catalogue, query, and presentation paths                                                                                                       | Neutral source/document facts owned by Mastermind                                     | Q1 acceptance                                                                                                               |
| References       | Q2                                                     | GI1 complete and the Q1 source/document facts it needs are accepted                                                                                                                                                                                                                                                                    | References command paths; no shared-source mutation                                                                                                        | Accepted neutral source/document facts                                                | Q2 acceptance                                                                                                               |
| Read-only C      | E1                                                     | G1 and GI1 complete; extension source contract frozen                                                                                                                                                                                                                                                                                  | Extension List/Inspect roots                                                                                                                               | Shell and filesystem foundation                                                       | M1                                                                                                                          |
| Read-only D      | GN1                                                    | Q1-Q3 and R2 complete; pure-effect boundary frozen                                                                                                                                                                                                                                                                                     | New `Framework/GeneratedNavigation/**` and mirrors                                                                                                         | Accepted source/route/document facts                                                  | GN1 acceptance                                                                                                              |
| Next foundation  | SF1, SF2, SF3, SF4                                     | Complete at integrated D0/SF1-SF4 baseline; combined managed and Native AOT evidence recorded above                                                                                                                                                                                                                                    | Separate Shell interaction, Framework distribution, lifecycle-provenance, and directory-effect paths                                                       | M1 and accepted root project graph                                                    | Complete; protected surfaces remain integration-owned                                                                       |
| Next commands    | C1, C2, C3, C4, M4A, M2, M3                            | C1 complete at `4c85d1d6`; C3 at `fa3db1ee`; C2 at `c60fcb98`; C4 Route Init at `cc5085ce`; Route Create at `19412d2`; QR1 at `862cbf2a`; Route Update at `27df8325`; Route Move at `631983ea`; Task 12 at `495a7ed6`; Task 14 at `20807781`; Route Remove at `5a2e650a`; Root Update is complete at `c6eec9d2`, exact tree `f31cacb0` | Separate Extension Create/Install, Route Inspect/Init/Create/Update/Move/Remove, and root Update paths; QR1 consumed Route Create's two predecessor slices | Root composition, serialization, help, process, recovery, lifecycle, and AOT evidence | Task 7, Status, Doctor, Route Remove, and Root Update complete; Task 18, Task 19, Task 20, Task 23, and Task 24 remain in the ordered queue before delivery. |
| Task 7 expansion | Complete                                               | Complete phase 4/4, milestone 7/7; Linux journey passed; Darwin/Windows stage+pack only.                                                                                                                                                                                                                                               | npm x64 package graph, staging, and one host-selected `PackageEndToEnd` journey                                                                            | Task 13 Linux D1 preparation/evidence as consumed input                               | D1/A1                                                                                                                       |
| Delivery         | Task 13 Linux-only D1 smoke, packed journey, checksums | O2 behavior, Task 7 expansion, Task 10, and accepted Task 21 remediation complete; release contracts frozen                                                                                                                                                                                                                            | Separate workflow and artifact paths                                                                                                                       | Accepted command binaries and Task 7 package graph                                    | D1 acceptance                                                                                                               |

No parallel implementation may change the same shared capability. Promotion or
cross-lane contract changes return to a sequential Mastermind integration step.
After Find Child 1 acceptance, the solution-wide [Modern C# Improvements](tasks/modern-csharp-improvements.md)
Preflight was accepted at exact `55eb82e`. All five ordered modernization batches
and the final gate are Complete and accepted at exact `a1cbf09`:
Framework `a90af59`, Shell/root `fe10525`, Route Inspect/family `62a1dd9`, Route
List `273eb45`, and Tests/support `6af5fb1`. Find Child 2's original Preflight
through Blue history is accepted through exact `685e2dd`. Its shared
Markdown/YAML correction Gray is accepted at exact `2cae7a4`, Red at exact
`a673ebe`, and Green at exact `d4701ad`. Correction Blue is accepted at
exact `0006915`, and corrected Purple at exact `2d10474`. Final-review supplemental
Red is accepted at exact `c72dd5e`, and its evidence corrections through exact
`23fe39a`, with the Unit portion at exact `d494adb`. Corrected Green is accepted at
exact `2337d62`; final evidence and Child 2 acceptance are recorded at exact
`ff7ce3f`; Child 3 focused Preflight is accepted at exact `28d316a`, Gray at exact
`a76a217`, original Red at exact `22d3bff`, metadata corrections at exact
`6a9a0de` and `eea3d59`, and supplemental escaping Red at exact `a865fd1`.
Child 3 Green is accepted at exact `cb7874c` (`Implement Find presentation`) over
corrected Red `a865fd1`. Fresh Green evidence passes the locked restore,
warning-free Release build, format and diff checks, focused Find/direct-root and
generated-serialization tests, affected Shell/Route tests, managed non-AOT
publish, and published Find EndToEnd `13/13`, all with zero skips. Fresh final
bounded correctness review is `PASS` with no material findings; Native AOT was
intentionally not claimed for Green. Blue is accepted at exact `3f81e76`
(`Simplify Find JSON projection`) after changing production structure only in
`FindJsonProjection.cs` to use the canonical status and finding-code readers and
remove the two duplicate private mapping methods. Blue's warning-free build,
format/diff checks, focused Unit `206/206`, and focused Integration `43/43` pass
with zero skips; the bounded correctness review is `PASS`, and the local
improvement review is `APPROVED — NO_MATERIAL_IMPROVEMENTS`. The no-op Purple
acceptance is recorded at exact `426d4f5` with verdict
`NO_MATERIAL_IMPROVEMENTS`; focused Unit `206/206`, focused
Integration/serialization `43/43`, and published Find EndToEnd `13/13` passed with
zero skips, and source diff/check against `3f81e76` was clean/empty. No
test/support, production, contract, project, package, configuration, generated,
Route, Shell, or root file changed, and no Purple code/test commit was
manufactured. Find Child 3 and its parent are now Complete and accepted in the
commit containing this record update. The final artifact and public no-write audits
passed. `CLI-EDGE-001` remains non-product only.

## Step Rules

### S2: Task Authoring

- Instantiate one parent program Task, one Task-group entrypoint per phase, and one
  Task per coherent independently accepted result.
- Give foundation Tasks accepted class maps, project paths, dependencies, and
  exact evidence.
- Give command Tasks contract matrices, predecessor facts, local models, promoted
  capability rules, test disposition, and public scenarios.
- Split a Task into child or subchild files when separate ownership, state,
  evidence, or integration justifies it. Keep checklists inside a Task when another
  file would add only ceremony.
- Do not add production source in S2.

### F1-F5: Actual Foundation

- The Mastermind authors the first foundation directly from the Architecture and
  foundation Tasks.
- The result is an actual retained host and Core, not a probe or spike.
- No command symbol, route behavior, fake operation, or Foundation-named domain
  model is introduced merely to prove plumbing.
- Tests prove shell stages, terminal modes, process boundaries, serialization,
  filesystem safety, and AOT without inventing a retained command.

### Command Steps

- Begin with a contract-to-evidence matrix and preserved-test disposition.
- Freeze command-local definitions, request, result, binding, and shared-fact
  dependencies before behavior delegation.
- Implement one complete command. Do not create placeholders for later commands
  beyond symbol/help entries explicitly required by current product help.
- Promote a semantic unit only at the integration point where a second real
  consumer proves identical meaning.
- End with focused managed, process, unchanged-state, diff, and architecture
  evidence, plus directly affected regressions. Run the complete managed/AOT gate
  at the golden slice, integration wave, or material trigger recorded by the
  per-Task check, and whenever an accepted Architecture, contract, or Task
  requirement calls for it.

### Mutation And Delivery Steps

- Mutation Tasks separate planning from application and prove revalidation after
  lock acquisition.
- Recovery and lifecycle schemas are frozen by the Mastermind before command
  implementation.
- Delivery Tasks consume accepted binaries and do not reproduce behavior. Task 7
  owns the accepted x64 package graph and one host-selected package journey.
  Task 13 remains Linux-only D1 for the later native build and smoke, packed
  install and invocation, checksums, and bounded artifacts. Task 22 performs
  final acceptance and separately authorized atomic publication of the complete
  x64 graph.

## Decision Points

| ID  | Decision                                                                                    | Current direction                                                                                                                                                                                                                            | Decision-maker                       | Needed before                | Result if reopened                                                                 |
| --- | ------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------ | ---------------------------- | ---------------------------------------------------------------------------------- |
| D1  | Can the managed BCL prove required component-wise physical identity on every target?        | Prove with real OS and AOT evidence; no interop                                                                                                                                                                                              | Maintainer after Mastermind evidence | F4 acceptance                | Narrow Architecture return                                                         |
| D2  | Does `route init` gain a Framework-shape mode?                                              | Accepted: concrete uniquely aligned sparse scoped chain after root Install                                                                                                                                                                   | Maintainer                           | D0 contract freeze           | Reopen contracts, lifecycle ownership, sequencing, and evidence                    |
| D3  | Does a dependency version need replacement?                                                 | Keep accepted exact versions until evidence requires change                                                                                                                                                                                  | Maintainer                           | Owning foundation/command    | Focused dependency decision and full AOT proof                                     |
| D4  | Do perspectives, Tasks, or Plans become Framework primitives?                               | No; continue local trial                                                                                                                                                                                                                     | Maintainer                           | After several complete Tasks | Separate Framework proposal, not CLI scope                                         |
| D5  | Where does native interaction cross generic Shell boundaries?                               | Root composition injects only prompt-capable factories; requests carry only local policy Booleans                                                                                                                                            | Maintainer                           | SF1                          | Reopen Shell/root callable architecture and affected tests                         |
| D6  | What exact prompt grammar and capability rule closes Route Inspect's collision gap?         | Accepted: terminal-capable stdin/stderr, one answer by one-based number or exact path; invalid/EOF blocked, cancellation interrupted                                                                                                         | Maintainer                           | C3                           | Reopen command contract and focused evidence                                       |
| D7  | What minimal BCL directory-create effect is shared by Install and Route Init?               | Accepted: lease, immediate missing/physical-parent revalidation, BCL create, post-verification, retained residual                                                                                                                            | Maintainer                           | SF4                          | Reopen mutation contract, reuse, and failure evidence                              |
| D8  | What exact name and description defaults does Extension Create write?                       | Accepted: hyphen-split first-ASCII-uppercase name and `Open Forge Extension package <stable-id>.`                                                                                                                                            | Maintainer                           | C1                           | Reopen manifest contract and exact evidence                                        |
| D9  | Which catalogue-parent states are eligible and how do sibling packages affect Create?       | Accepted: any existing safely resolved directory; no marker; empty/siblings allowed; inspect only exact ID destination                                                                                                                       | Maintainer                           | C1                           | Reopen destination classification and collision evidence                           |
| D10 | What exact command-local JSON result shape and property order does Extension Create expose? | Accepted ordered catalogue, destination, ID, manifest, mode, effects, verification, and unchanged-workspace fact                                                                                                                             | Maintainer                           | C1                           | Reopen source-generated schema and renderer evidence                               |
| D11 | How does Install confirm, refuse, handle EOF/cancellation, and avoid prompting?             | Accepted: prompt once only for prompt-capable human writes after preflight; refusal/EOF/cancel interrupted; other modes never prompt                                                                                                         | Maintainer                           | C2                           | Reopen command-local interaction contract and evidence                             |
| D12 | How does Extension Create gather and correct missing required human input?                  | Accepted: ask only missing stable-ID/catalogue facts; local correction without attempt limit; EOF invalid, cancellation interrupted; no generic retry framework                                                                              | Maintainer                           | C1                           | Reopen wizard contract and focused evidence                                        |
| D13 | How does a missing `.agents` container compose with the workspace lock and SF4?             | Superseded by accepted external-lock correction: acquire the persistent external lock first, then apply `.agents` as the first ordinary planned/reported lease-bound directory effect; no bootstrap result remains                           | Maintainer                           | SF4, C2, M2                  | Reopen lock location, result contract, mutation sequencing, and residual evidence  |
| D14 | May root Update M3 behavior begin alongside Route Mutation M2 after Install?                | Accepted: no; full M2 behavior precedes M3, while independent preparation may run earlier                                                                                                                                                    | Maintainer                           | M2, M3                       | Reopen program sequencing and lane ownership                                       |
| D15 | What exact public Root Install JSON result shape and residual vocabulary closes C2?         | Accepted: one fully present ordered result with mode, force, automatic, atomic nullable source/classification/footprint, exact effects, lifecycle, recovery, verification, findings, and typed residual values `none`, `retained`, `unknown` | Maintainer                           | C2                           | Reopen result model, serialization, help, process evidence, and command acceptance |

## Risks, Recovery, And Stop Conditions

| Risk or trigger                                                                            | Affected steps | Safeguard                                                        | Recovery or stop response                                             |
| ------------------------------------------------------------------------------------------ | -------------- | ---------------------------------------------------------------- | --------------------------------------------------------------------- |
| Local command design bypasses the system architecture                                      | All commands   | Parent links, frozen foundation, Mastermind integration          | Reject local implementation and return to parent Task                 |
| A Task still contains an architecture choice                                               | S2 onward      | Task readiness audit                                             | Keep Task blocked; resolve in Architecture first                      |
| Preserved tests anchor obsolete structure                                                  | Command Tasks  | Map behavior to current contracts before porting                 | Rewrite fixture or test; never restore structure for test convenience |
| Shared semantic capability is promoted without a second consumer proving identical meaning | R2 onward      | Promotion evidence in integrating Task                           | Move it back to narrow scope or split semantics                       |
| Filesystem safety cannot be proved portably                                                | F4, mutations  | BCL-first real-OS and AOT matrix                                 | Stop and return to D1; do not weaken or add native code               |
| Native AOT differs from managed behavior                                                   | F5 onward      | Publish and execute affected boundaries every increment          | Reject managed-only pass; correct or reopen dependency                |
| Mutation leaves unverified partial state                                                   | M1 onward      | Plan, lock, revalidate, apply, verify, recovery evidence         | Block command acceptance and preserve owned fixture evidence          |
| Parallel lanes modify shared contracts                                                     | Parallel work  | Non-overlapping paths and sequential integration                 | Stop lanes and integrate one accepted contract first                  |
| Task records become stale bureaucracy                                                      | S2 onward      | Update only at state/evidence boundaries; prune completed detail | Consolidate outcomes and archive or prune temporary records           |

Task 17 “Extension Update” remains Complete at phase 5/5, milestone 8/8, and
is dequeued after its completion grace was consumed; its accepted integration is
recorded above. Task 18 “Extension Remove” is active at phase 2/5, milestone
1/8 with Gray callable/public-shape review active, followed by Task 19 “Repair”
and Task 20 “Cleanup”. Tasks 23 “Workspace Libraries” and 24 “Extensions
Evolution” are queued after Task 20 in that order as last-stage improvements
without phase or milestone horizons. Tasks 10, 21, 13, and 22 remain explicitly
postponed.

## Verification And Integration

| Gate           | Inputs                                          | Verification                                                                                                                                                                                            | Pass condition                                                        | Resulting update         |
| -------------- | ----------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------- | ------------------------ |
| VG0 Planning   | Architecture, Plan, Task hierarchy              | Link, hierarchy, dependency, scope, and stop-condition audit                                                                                                                                            | Every implementation Task is closed or explicitly blocked             | Foundation authorized    |
| VG1 Foundation | F1-F5                                           | Restore, format, build, unit, integration, end-to-end host, dependency audit, local AOT publish and execution                                                                                           | Clean route-free architecture with no probe or root C# clutter        | G1 accepted              |
| VG2 Command    | One command and affected shared facts           | Focused managed tests, directly affected regressions, public process scenario, and unchanged-state check; complete managed/AOT gate at the recorded golden-slice, integration-wave, or material trigger | Contract complete with no architectural debt deferred to next command | Next command authorized  |
| VG3 Mutation   | M1 and one mutation command                     | Focused failure matrix, lock/revalidation, planned effects, bundle retention/cleanup, idempotence, and process evidence; complete managed/AOT gate at the recorded trigger                              | No unverified partial state or hidden lifecycle behavior              | Next mutation authorized |
| VG4 Delivery   | Complete commands, Task 7 graph, and Task 13 D1 | Task 13's Linux-only build and smoke, bounded artifacts and checksums, plus Task 7's complete x64 package graph and host-selected package journey                                                       | Complete non-shipping candidate for the accepted x64 graph            | Final acceptance         |
| VG5 Release    | VG4 and maintainer review                       | Separately authorized atomic main-only release procedure for the complete x64 graph and public smoke tests                                                                                              | Maintainer explicitly accepts shipping release                        | Release and closeout     |

## Coordination And Continuity

- Agents may surface architectural alternatives and supporting evidence, but may
  not accept a product feature addition or removal. Only explicit maintainer
  direction changes accepted product scope.
- Child Tasks: The `tasks/` hierarchy created in S2.
- Checkpoint: [CLI Development Checkpoint](../checkpoints/cli-development.md).
- Handoffs: [CLI Find Accepted Handoff](../handoffs/2026-08-25_cli-find-accepted.md)
  is sealed for the post-acceptance resumption boundary. Do not edit it. The
  generated Handoff `Entries` remain unchanged under `CLI-EDGE-001`.
- Related plans: The removed release Plan is historical at
  `../../archived/cli-release/release-plan-2026-08-21.md`.
- Update points: After S2, every foundation gate, every accepted command, each
  shared promotion, mutation foundation acceptance, delivery acceptance, and any
  Architecture return.
- Resumption path: Read the CLI Architecture, this Plan, the Checkpoint, the
  selected parent Task, and the active leaf Task. Then execute the leaf Task's
  stated next action.

## Completion

This Plan completes only when every child Task is accepted or deliberately
cancelled, all retained commands and release surfaces are integrated, complete
managed and native evidence passes, the accepted x64 graph is verified without a
platform subset presented as complete, current Architecture and contracts match
the implementation, temporary records are consolidated, and the maintainer
accepts the release. Until then the replacement remains non-shipping.
