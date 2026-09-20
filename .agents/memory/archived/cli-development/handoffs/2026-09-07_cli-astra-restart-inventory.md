---
open-forge:
  description: Exact dirty path and content identities captured for the CLI Astra restart
  tags: [Memory, Archived, Contextual, Historical, Handoff, CLI, Evidence]
---

# CLI Astra Restart Inventory

Companion to the [sealed handoff](2026-09-07_cli-astra-restart.md).
Captured after writers stopped and before this handover's own documentation
changes. This inventory records existing work; it is not evidence of acceptance.
Paths are relative to each named worktree, whose locator is relative to main.

All indexes were empty. Manifest SHA-256 covers each row in Git porcelain
order as `status<TAB>path<TAB>permission-mode<TAB>sha256-or-DELETED<LF>`.
The two-character status preserves its leading space. File hashes cover bytes;
a symlink would hash its link text. Deleted files have no current mode/hash.
The manifest does not include subsequently added transfer files or the
handover's own ledger/checkpoint/navigation changes.

## Main Develop

- Worktree: `.`
- Branch: `develop`
- HEAD: `2c62f59aff8d0992b81621c298c4aabc9ab72c9a`
- HEAD tree: `fa607717e461f4e6092d38b69e566ff82a6b963a`
- Dirty paths: 2
- Manifest SHA-256: `50e3e38b9276481e6a78178682b25150954f14d5e3b255ca067bfd6c11077435`

```text
 M	.agents/memory/emerging/ideas/extensions-overhaul.md	664	e58fd700780b634d75baa4220ff969343fc5d29e9b039098fbf5491db76a1bbe
 M	.agents/memory/working/checkpoints/cli-development.md	664	03af0d64bd52bc2e8a14ebaf8128cf32207a4951a9abb29e0f952fe58d0b6d2c
```

## repair-implementation

- Worktree: `../open-forge-worktree/repair-implementation`
- Branch: `codex/repair-implementation`
- HEAD: `eae8eb366e8ad50d8c18cca6a4e08e9d3b6d22bb`
- HEAD tree: `4b2523e8816bffcbb24e28ae6ebd230fcfa73aca`
- Dirty paths: 28
- Manifest SHA-256: `556c919e52611d85ed88bf36c7cd39dd5cde690fdd23cf2e0518545f005fa54b`

```text
 M	src/cli/core/OpenForge.Cli.Core/Commands/Doctor/DoctorOperation.cs	664	9d9a789fa3c80bf9b18fd1e382b420dbca58eff7b4687495597deae018d6c2f5
 M	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Models/Planning/RepairPlanModels.cs	664	989c591f3d946df71b6fe587f81a98d5d89ff1cfa5c945dd5bd77c140853bf10
 M	src/cli/core/OpenForge.Cli.Core/Commands/Repair/RepairOperation.cs	664	c2fe82a68ad8c565e5bc386c83ea90f3e8351f6ccb6c1ccf61fb379cf37b4e67
 M	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Planning/RepairPlanner.cs	664	32f76a43545a5500fbd073a905680a9e9dc53ee89f92a82d88c15bdeabe7ce35
??	src/cli/core/OpenForge.Cli.Core/Commands/Doctor/DoctorDiagnosisReader.cs	664	c8e73d168c77adb0b93eaea5c35cc3aa3c43ec2b3de782532c3bb65ec1695116
??	src/cli/core/OpenForge.Cli.Core/Commands/Doctor/Models/Observation/DoctorDiagnosisRead.cs	664	c87e4b6d08afa03c47286301d169741c7a9622a8842a9fa2216ecd99a6c1601b
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/RepairOperationComponents.cs	664	97df37e1485b05599e94e97bd8fcf7a7ad892e755b27001ddc9d026bf22b3ba2
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/RepairOperationFactory.cs	664	1ad788f4133c69c34bae66a813c61ca0d7943dd4fd9fc302a695cc5ea4f9b103
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/RepairOperationalContributorFactory.cs	664	f822e786005127ab4828186ac40e4c57dcd5fd511d7c6cb1f2ef4939039779fa
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Application/RepairApplicationOperation.cs	664	1bb21e8f56a9a0613333b9b6b255f77ebf12d6d98b5e714d6d7d9a1be97abbb9
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Application/RepairApplicationOutcome.cs	664	8877a294ea870b5b51cb8ae75c7940b14403775066de5fd2a8cbfbf0f95c9895
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Application/RepairApplicationOutcomeFactory.cs	664	8a07e4002fc6551a50ddc027c49787da3d0a553d1f6d3906b2541a59992e3b4f
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Application/RepairPostVerifier.cs	664	e024295d86a79bb2aeb3947de367556d2aab92dd578b222e8beba82d9b90cdd0
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Application/RepairRecoveryLifecycle.cs	664	65f642348180562e71551e86619750ebb434f65b57207e7769255cea93e3969e
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Diagnosis/RepairCandidateMapper.cs	664	2dfba3e6b2a66abccce9fd54df6e28eecc8c1439a12be1b2ff4b41ca08b63354
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Diagnosis/RepairCatalogueRead.cs	664	46c005408a22135847dbe8e2f26df0e2b7fc136080e2dbc01c7a52ea21070b09
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Diagnosis/RepairCatalogueReader.cs	664	2b10637704e6b6cefe7c86a17c35d5fc7405f8ed71809b97a0973a4f539ff318
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Diagnosis/RepairCoverageMapper.cs	664	eb062d9282fbf542c0723c428d52c2c686ff8a91149d2e86e615344198b7b73b
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Planning/ReferenceIdentityComparer.cs	664	69a99876e1dc3db0701a24955819835ed46bee3b0bd9ccd71d0ac704149a6c76
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Planning/RepairDestinationFormatter.cs	664	284b4fe25b7538554021330255fda2a0c0079829c8cc1174b7dffd927786a16d
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Planning/RepairEffectPlanner.cs	664	231db443e332949c8d74c36d4fd0183553f98b13c6fde0e599630fb706feec0e
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Planning/RepairEffectProjection.cs	664	8bce93557c7beb6022f0f639d4c893cf2aed460b904ee68bcd4bd49facafbe05
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Planning/RepairPlanningAction.cs	664	867d871e572b567534efdd6ac516041479c3ac0d47399025d3bd63296eebecbb
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Planning/RepairPlanningSelection.cs	664	f4f78d4e2a0cb254cef2e8c96ae9a3d382fbb095be93f4557e21970e3049f0cb
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Planning/RepairProposalInput.cs	664	6445922894db32d063221f468d4874950de3cd556bf68f66b7e0764ea50dc61d
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Planning/RepairSelectionPlanner.cs	664	a4bbbf78394e23085e30bb107da8dd8099f3bedb52a277a75ad80336ac9b856f
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Result/RepairResultBuilder.cs	664	b63e6e79fff7c983a347e332651e92721d05c76595541a12deee855810843c49
??	src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Result/RepairResultInput.cs	664	62cd6ffbcb8172b66220a3be756fea1419929cc910d9419a76cfc77704d7de4a
```

## cleanup-implementation

- Worktree: `../open-forge-worktree/cleanup-implementation`
- Branch: `codex/cleanup-implementation`
- HEAD: `96ed0aa35b7e092aa78e5bb7249ce71c5598b2c9`
- HEAD tree: `be13c85b4cb26271c0770b880a094f125c0a7f7c`
- Dirty paths: 0
- Manifest SHA-256: `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855`

Clean at capture.

## workspace-libraries-contracts

- Worktree: `../open-forge-worktree/workspace-libraries-contracts`
- Branch: `codex/workspace-libraries-contracts`
- HEAD: `c3f01acb76c572ee486fdc24c6a2379b27459391`
- HEAD tree: `5ccffb7156f73aac3c15a49588b7eafcfa661a0a`
- Dirty paths: 102
- Manifest SHA-256: `a08b3ab894e4e90e0b1500a0bdb73a0d5621a969da29849cead77cb97dc117f7`

```text
 M	src/cli/core/OpenForge.Cli.Core/Commands/Extension/Install/ExtensionInstallOperationFactory.cs	664	b261057193545d87eccd7fe229fe51f9907a1cafb80041249dd562e4d7efa605
 M	src/cli/core/OpenForge.Cli.Core/Commands/Extension/Install/Shared/Application/ExtensionInstallApplicationOperation.cs	664	8eb782cadb70f6e3544271931df7a0b53e7a13f10cfa0ed58270fab093c81a70
 M	src/cli/core/OpenForge.Cli.Core/Commands/Extension/Install/Shared/Application/ExtensionInstallRecoveryOperation.cs	664	92c91a5941cd19e90402df59175542b00bb00751e7a8a1cc5e5df7c85adc7abc
 M	src/cli/core/OpenForge.Cli.Core/Commands/Extension/Install/Shared/Planning/ExtensionInstallFoundationReader.cs	664	32ca58dec19761b2ae9c856ed4e3798fbfaabc534be18208f369ab368f170547
 M	src/cli/core/OpenForge.Cli.Core/Commands/Extension/Install/Shared/Planning/ExtensionInstallPlanner.cs	664	70a83de58f568468f9dbbb24e8e657fcc426e235279340c0ce9007006baa0f86
 M	src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/ExtensionUpdateOperationFactory.cs	664	8956a0a54877eb12efbbd09e5c07f9bc25a25cc766e34234ad65a0fe18fc95fc
 M	src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/Shared/Application/ExtensionUpdateApplicationOperation.cs	664	fb40e3c1b7bc41ebd1e3b6da888809172117afcec9ac8c849193f1e62f9e871f
 M	src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/Shared/Application/ExtensionUpdateRecoveryApplication.cs	664	0f7767c787764c6231cf8ed7633e61c91ffc50887bbb97c7adbd28e1eb42b7b2
 M	src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/Shared/Planning/ExtensionUpdatePlanner.cs	664	5a350a6c245357042fef8e5c19dda6d3e63100fa6ed0fcef4d5122b410708bdc
 M	src/cli/core/OpenForge.Cli.Core/Commands/Index/IndexOperationFactory.cs	664	78550150df48bcc21c7a6a82fed26c4953bf7c842c5d602012d184c4c70cb682
 M	src/cli/core/OpenForge.Cli.Core/Commands/Index/Shared/Operation/IndexApplicationOperation.cs	664	58def604448873bc9a43c1a4cc4c2e5fe362b20c66b1bee22ce75843f06a99e2
 M	src/cli/core/OpenForge.Cli.Core/Commands/Index/Shared/Operation/IndexRecoveryLifecycle.cs	664	5564fb8c76c5d205b0b01b9f226c56710ec84e1736604b8212e0df38f25c240f
 M	src/cli/core/OpenForge.Cli.Core/Commands/Install/InstallOperationFactory.cs	664	9b2b7716784a2a0b338d6a614985f0afa926d797d045438ff08e3ddcc194e192
 M	src/cli/core/OpenForge.Cli.Core/Commands/Install/Shared/Operation/InstallApplicationOperation.cs	664	7baa62aa1d0bec947cc7f52c753c2eb278d1976e00681d0c7506f98bb4b81f02
 M	src/cli/core/OpenForge.Cli.Core/Commands/Install/Shared/Operation/InstallApplicationOperationFactory.cs	664	a5e18a41c709b99a78c97fcbcf5537abac1284ad3e63bf81d338490bb80f4883
 M	src/cli/core/OpenForge.Cli.Core/Commands/Install/Shared/Operation/InstallApplicationPreconditionValidator.cs	664	6b1ae1021c23aa25b316a3f002cdbf46aa7507d8fc0a7ffc2f568fc7265ad7ff
 M	src/cli/core/OpenForge.Cli.Core/Commands/Install/Shared/Operation/InstallRecoveryOperation.cs	664	062dc5eb61a94f19261d8e7a7f3498eb7a9b3d9d7e975546d05ba9b969b8f5d4
 M	src/cli/core/OpenForge.Cli.Core/Commands/Install/Shared/Planning/InstallPlanBuilder.cs	664	f622528456a0a2203411da06fd4330c60288a365c5c5215a1f1fb1bac5dcdef2
 M	src/cli/core/OpenForge.Cli.Core/Commands/Install/Shared/Planning/InstallPlanningInspector.cs	664	6f57844eb296a0b62dcad98d85133d9c4823dafdb52bf283a6841d0b138be40e
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Create/Shared/Application/RouteCreateApplicationOperation.cs	664	ade4d69d6accb97a82b9fb742c710dc3c96e86a65f3971209a38d35f43be17ad
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Create/Shared/Application/RouteCreateRecoveryLifecycle.cs	664	d3886d10ca41c07961af7fc7f1744a63922542e4340fdbb31ae5d3074849e6a8
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Init/Shared/Application/RouteInitApplicationOperation.cs	664	6983219625afe54e93615761f4b93c38d1b662d635a12d39f86687676450dc14
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Init/Shared/Application/RouteInitRecoveryLifecycle.cs	664	573cc61b30f80e35519f80ac3ff261b8fc4929604973f1095c7ec21bd2c7e082
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Init/Shared/Planning/RouteInitPlanBuilder.cs	664	6cb571aa47053f5ec510397e35c2f2afb9af3530ff56f39488c1e653548a244b
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Init/Shared/Planning/RouteInitPlanFinalizer.cs	664	ef7eb5142ff80596a4d140f4c849c174377cdaee11ead7139bb7734d20bd56ed
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Move/RouteMoveOperationFactory.cs	664	e9cbb49d26c1a324b22208e2362a2b2d1c76978c141243ba06c899059ddfae30
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Move/Shared/Application/RouteMoveApplicationCompletion.cs	664	f0f61637ab906e75eaaecf4ae787e95f727b0dcefcaf8166a0b1f250c82d92b8
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Move/Shared/Application/RouteMoveApplicationOperation.cs	664	48d6f13380c8ee322374de967590cca8457ae4771a8b925214d37ef1a0933ca1
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Move/Shared/Application/RouteMoveRecoveryLifecycle.Deletion.cs	664	5e038abbc37d8900420a7b45824a0f5dd95e8aed815f7ffadd932567076d1c38
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Move/Shared/Application/RouteMoveRecoveryLifecycle.Preparation.cs	664	06ff5fc4fc6c4e69f35377aa2ef5ae805b900ecbcf157e6436a417c5204dd8f5
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Move/Shared/Application/RouteMoveRecoveryLifecycle.cs	664	7217e76437130dd998be045aa89e8c7cd6570806536ea99cbdd5b21c58aee1ce
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Remove/RouteRemoveOperationFactory.cs	664	71f07f8866fb70611e5d6ccc90cb388a30397e325f1ce9ddb2fccdfb60391c77
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Remove/Shared/Application/RouteRemoveApplicationCompletion.cs	664	f442a3794f8219787a95114afab8b20c840fa9017c5ae43eb21399344ec35db0
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Remove/Shared/Application/RouteRemoveApplicationOperation.cs	664	5c6e0f2ed0224d4227090f683ddca2934480096ec86483fffea760a00fa2e076
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Remove/Shared/Application/RouteRemoveRecoveryLifecycle.Deletion.cs	664	f344cd7e9ab2dc4c6378fd874603a4e8396c33af21fe72eae49a02200e53330a
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Remove/Shared/Application/RouteRemoveRecoveryLifecycle.Preparation.cs	664	b6fd567910ddbbc4a25e30ae552c9e3513995e85318dd42d4928432a1b49f1a9
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Remove/Shared/Application/RouteRemoveRecoveryLifecycle.cs	664	00f7ea8cd9de9fc5272437b5ceec5869e3b0f5bb4184c8763299b3df98adf5f1
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Update/RouteUpdateOperationFactory.cs	664	0e5040ecfd019126f6d24aa5db60a22635b51e042b2ab0c7e61caba93d11aa10
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Update/Shared/Application/RouteUpdateApplicationPipeline.cs	664	65e78b02d7d177eb282e48a6d8de42c928abaf664e90c7515cb3d63f1dd4f5c2
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Update/Shared/Application/RouteUpdateApplicationPreparer.cs	664	2dd0f42cce9a7f717a2c8411ae2f9731841f3ececdeef6a72b1ad156fc198b80
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Update/Shared/Application/RouteUpdateRecoveryCompleter.cs	664	e6c97135b76f7065f71333f4d6886275fb6c3d5fce9c54d0345b3c881e6f15b7
 M	src/cli/core/OpenForge.Cli.Core/Commands/Route/Update/Shared/Application/RouteUpdateRecoveryPreparer.cs	664	20a7205c3bba6192e4716d5445e1dbbbe0786b7d3d7ff1d800ce04f6e2eb99d5
 M	src/cli/core/OpenForge.Cli.Core/Commands/Update/Shared/Application/UpdateApplicationOperation.cs	664	5c0e4aa939ed53724557e2bcc4264862340d80d7a5626832fd55d30071b7953a
 M	src/cli/core/OpenForge.Cli.Core/Commands/Update/Shared/Recovery/UpdateRecoveryOperation.cs	664	6773bae1f0fa4a2f9eec4b63272210441330d2731143781e022e4de471139739
 M	src/cli/core/OpenForge.Cli.Core/Commands/Update/UpdateOperationFactory.cs	664	db09b314c99db85b185ce47329512afe4f7d02821855550b3786e2553e483cd8
 M	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/Models/RecoveryBundleAttribution.cs	664	1f98560bc4968f9a5e23b8d43e5518f7966d4fa25550f95cfc18ddcabaf6954b
 D	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/Models/RecoveryBundleEntry.cs	-	DELETED
 M	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/Models/RecoveryBundleInput.cs	664	bfe4b91c4f7a06f82968d7b950b0d488229297fabe16ba0ffb1a6846d1f821ef
 M	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/Models/RecoveryBundlePreparation.cs	664	c5298aaec70fcb6bb3fb96cf41154b41f8bda77eb65e435c9cb7dd435253b807
 M	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/Models/RecoveryBundleResults.cs	664	1e016597666da17036af30ea284f3d6054c8d15a3a72a04dc94d51b3ffd26fd3
 M	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/Models/RecoveryBundleTarget.cs	664	7aff3d0b1f1378e92ae2d80b58688f4deb4a9436315ef74e7d952b9e96e4bf2e
 M	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/Models/RecoveryBundleTargetComparison.cs	664	53f7da86f097087918869648338c0f83510a92001528b173a6380ccc35af8cf3
 M	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/Operational/RecoveryResidualOperationalContributor.cs	664	b79916cbad8d8c1ab7b8184166cdc7cac75a2781d2c7c6f8b6e3dae79d82e7e0
 M	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/RecoveryBundleCatalogue.cs	664	eafb55dc2063caa45c1ee42e8623e7efb425b644d9583663a35d97475136e857
 M	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/RecoveryBundleDeletionGuard.cs	664	2dfa10d33c9edd502ac337ba6b71625e30325bcb38619eb277c33aec6a89df64
 M	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/RecoveryBundleReader.cs	664	8cd23feac5009dfeedd7a60ad7d8be73974f8b9a8f1aefe568f38e60efd44477
 M	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/RecoveryBundleStore.cs	664	282e32633c29647c761055ad027a77b650829b10aa004f7c117bcf5325a6092b
 M	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/RecoveryBundleTargetStateReader.cs	664	07106a4c5c496a8dd1bff76cc58dfa44a21c66a31f06eed1f786385955d5fba2
 M	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/Serialization/RecoveryBundleAttributionCodec.cs	664	a2957d20eeca935b84f10afc00ce60bb474a886cbd589d6c6e941a95d9d2752d
 M	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/Serialization/RecoveryBundleJsonModels.cs	664	c8d3055f9b9f4ac37263f6530495592fb6e01936cbebdc1d2a235908a4d10311
 M	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/Serialization/RecoveryBundleManifestCodec.cs	664	d37d34a362b0266f223ce72923292634fcda7ae63fea6f072a3e9093d52e8b45
 M	src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs	664	739546dd5f292cb01c8ecf5a31d43eb0d60096b991217f7f77a67482c80ac430
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Extension/Install/ExtensionInstallMutationIntegrationTests.cs	664	a97649d3400ad91802f17845771633830c484feccf656b3b4774f07cb51469a2
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Index/IndexOperationWorkspace.cs	664	482452ec0ddf61708f61eb4439327383f5acd68765cff57954457a9aee3d2d1d
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Install/InstallOperationWorkspace.cs	664	92e0869a61c53c6676c499b9ccdb39913b7fc86ea6bea7b62002ac835de7dc87
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Install/InstallPreservationIntegrationTests.cs	664	958304597611b65c86378b46e4dbaebb4a52497a318d965328c0b0eb107186c4
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Install/InstallRevalidationIntegrationTests.cs	664	b7d1c753aa7f09675c2465acddf112b1ea00ec7ecf5aa8daf6617a4a2d0cb9fe
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Create/RouteCreateApplicationIntegrationTests.cs	664	263b769ddbb8f68b070a174038bec6629aab46de8a441c4f8cc26f64a7e9dbde
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Create/RouteCreateIntegrationWorkspace.cs	664	31278ec7a3e71f32816bb27f4c7b27fd3809a0c3aa1b2663580653b7a696d167
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Init/Framework/RouteInitFrameworkIntegrationWorkspace.cs	664	13ab60cbf2f2a09de04a0263f8803e197524d94c938411d0971d6f373d9237aa
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Init/Generic/GenericRouteInitIntegrationWorkspace.cs	664	97013f99be883df1ef7ffa9962f047f188a188f5c08c685f5f70a86d529d8ff6
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Move/RouteMoveApplicationIntegrationTests.cs	664	3bf4dfeaac2ca4b8959bffb7347467670e20560ff43550045b08610652fa659a
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Move/RouteMoveIntegrationWorkspace.cs	664	440109527fbca8a30d5e390853341deb27a9873393116277f8d3fc09066e93d7
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Remove/RouteRemoveApplicationIntegrationTests.cs	664	38ca1f19bc634ce0a323bb7b50e3c2a538c64acbb4198c19bb41664767f953db
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Remove/RouteRemoveStatusDoctorIntegrationTests.cs	664	504cf93635b34365c88243f94f3b0d6c82d6e6ca61f07a00e180e50d9e206d7e
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Update/RouteUpdateApplicationIntegrationTests.cs	664	6ecd9269ea81d7d4501fa23ed323e8faa4b7cd80160d3403cf9c29b4e538dfec
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Update/RouteUpdateApplicationIntegrityIntegrationTests.cs	664	805fe4c098ac73640531befffe8e468c261c9deb5ea3538f49a551f7845e71c5
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Update/RouteUpdateIntegrationWorkspace.cs	664	0f50ca73cfb5c64ec648f193b8c7b157d5443ade5ce4df938692145e775b6262
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Update/RouteUpdateRecoveryIntegrationTests.cs	664	dcd2bb5be7514b9ce415aa7e2fa6d9ccaacd789a0af41bec29b0e97d4c85573f
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Mutation/Application/FileChangeApplierIntegrationTests.cs	664	cea3d00e94c15f2ca7c9e0376d2e499269e434cd7e89540667326c360fdddf7e
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Recovery/RecoveryBundleApplicationIntegrationTests.cs	664	9c5a03bebd185c7fb9d8fa003364ce64640133a20c5335def76693e43d339fa3
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Recovery/RecoveryBundleCatalogueIntegrationTests.cs	664	af662ce153b0562ee09d037e588833acec99864990f4a3ec230a6e52edbab5c9
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Recovery/RecoveryBundleDeletionAttributionIntegrationTests.cs	664	f73a4774004a6ba05129b156190c5e81420c904bf6228f532f7bcf1a25928312
 M	src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Recovery/RecoveryBundleStoreIntegrationTests.cs	664	a42670a49151faa5072912a3573e8f70f855ac1426beb38573ee3959406ab807
 M	src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Remove/RouteRemoveDefinitionsAndBindingContractTests.cs	664	b5c88be1979a02d7c58e3e15a5b70d9daa57e700b81ae5b48337a1f39389e1ca
 M	src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Recovery/Models/RecoveryBundleAttributionContractTests.cs	664	3d406c4fa7b9881256ce36b9880a01bded7d03227d081905f9ccb3cd30e70ba2
 M	src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Recovery/Models/RecoveryBundleContractTests.cs	664	847c122b79ca2641bfa685ba8f947059348ba78660adb2f1e99bade05aee92bc
??	src/cli/core/OpenForge.Cli.Core/Framework/Filesystem/LogicalPaths/CanonicalRelativePath.cs	664	6da29cd129a2b4353ba1a559a6f80b0f4e11b52d04bf7dc3b9292728c30a9306
??	src/cli/core/OpenForge.Cli.Core/Framework/Filesystem/PhysicalPaths/NoFollowLeafObservation.cs	664	75296660af5557fbdfac9f7f531e4baf1320179fff29c1d15167b97f8dd54b10
??	src/cli/core/OpenForge.Cli.Core/Framework/Filesystem/PhysicalPaths/NoFollowLeafObserver.cs	664	0bc8112d03e3e2a1be9d37fd17845ef1cb5dbdd7fa134f4c097b2c00c6c696c4
??	src/cli/core/OpenForge.Cli.Core/Framework/Libraries/LibraryPathIdentity.cs	664	1253096e9fd304ddd00b16fa71025861f1c259d70e1fc41792f7b961438f5a60
??	src/cli/core/OpenForge.Cli.Core/Framework/Libraries/Models/LibraryMappingModels.cs	664	27186ca1cb99027f6058c7a9e5f183ef1288c885fe7ccd76fa707abb522ce9c5
??	src/cli/core/OpenForge.Cli.Core/Framework/Libraries/Models/LibraryPathModels.cs	664	e24ef64e00d835ba83bafb229c5a29e6145e55c98089f0a915983fbb5242a554
??	src/cli/core/OpenForge.Cli.Core/Framework/Mutation/Application/RelativeFileLinkApplier.cs	664	76c280d77acca0a605226bc7f6b733841bf8817e216d78529a7e605e9b5d7152
??	src/cli/core/OpenForge.Cli.Core/Framework/Mutation/Models/Filesystem/RelativeFileLinkEffect.cs	664	12230f809ca62e6e07cc2f1266dd0b0b34c35a351d674ba357a29df683d65211
??	src/cli/core/OpenForge.Cli.Core/Framework/Mutation/Models/Filesystem/RelativeFileLinkReceipt.cs	664	746de496a8a3e4babbba5980154a0821c0fb873a7b6e3bc4049d8ed741c1cdc6
??	src/cli/core/OpenForge.Cli.Core/Framework/Mutation/Validation/Models/RelativeFileLinkValidationResult.cs	664	f693c2444cc14f3af5b0646e50eb6e262ae6ad505ddee6cd547b7b26b618efe6
??	src/cli/core/OpenForge.Cli.Core/Framework/Mutation/Validation/RelativeFileLinkRevalidator.cs	664	2d365e492ee6bc79beed0304954ffec8d10fe12dccba45c64c0deadfdd78ed1e
??	src/cli/core/OpenForge.Cli.Core/Framework/Mutation/Validation/RelativeFileLinkValidator.cs	664	cfc056063819085c05a3411991c689ced2de51447fc277ff1602c1c0a6e3cb20
??	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/Application/RelativeFileLinkRecoveryApplier.cs	664	85eab0860a4683d59da501e4553ad5b8e851b46aa802eb17c12682e54affc2b9
??	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/Models/RecoveryEntry.cs	664	da76ad73cf022a2e9a5f2cc2560baec0465423196cddd77ae0d0c964f2541258
??	src/cli/core/OpenForge.Cli.Core/Framework/Recovery/Models/RelativeFileLinkRecoveryResult.cs	664	e11c6a18afc2c3f1b4836c44c3149bcfda0615fed52a2a2822041a6d06c60aa0
```

## extension-queue-realignment

- Worktree: `../open-forge-worktree/extension-queue-realignment`
- Branch: `codex/extension-queue-realignment`
- HEAD: `2c62f59aff8d0992b81621c298c4aabc9ab72c9a`
- HEAD tree: `fa607717e461f4e6092d38b69e566ff82a6b963a`
- Dirty paths: 11
- Manifest SHA-256: `57e1b65d138a7b77122ad3be82a407f655ca466ce299b14f5dbd254f1b8d38c7`

```text
 M	.agents/memory/emerging/ideas/extensions-overhaul.md	664	f769093e5962a99fcb1c431ea1fdde9d4aa4ea7fd19ad43b9fac175d54f25de4
 M	.agents/memory/working/checkpoints/cli-development.md	664	ddb564df238767d6f1899ab217658de978acefd0f5f433f5bea6ab30895453b4
 M	.agents/memory/working/cli-development/overseer-memory.md	664	1d31d31d9067e6b7ef9f785e5ce0a07f81c516c95dae6a57bc8060072f46659f
 M	.agents/memory/working/cli-development/plan.md	664	0ade4acee8d384faa74e244ad44373c30dc08e6a55c2a887e9b2febfb13f5ff5
 M	.agents/memory/working/cli-development/project-control.md	664	c67615a918a08c41b4369f9ab591faaa6fe8ec70350915af3f46e2c05f5d0989
 M	.agents/memory/working/cli-development/tasks/00-cli-development.md	664	8b525ce6ae37c64b07eaf5fa1bb9f064debc1a84bf69b0341e582ea9bfec9a56
 M	.agents/memory/working/cli-development/tasks/_tasks.md	664	6ab9204f846b50d6d37a4b077d1fb9db3b1430e34b3abeea58956543cf26a9c8
 M	.agents/memory/working/cli-development/tasks/extensions-evolution.md	664	2fa047da961b454cc946b22c23f145423d695997cb8ad672e55ddbe8159d1fd8
 M	.agents/memory/working/cli-development/tasks/workspace-libraries.md	664	7cc8b8b6c84ad257ac8d097f60001e6fe68957924e52d9366770225670270da5
??	.agents/memory/working/cli-development/tasks/extension-internal-consolidation.md	664	4b7a71abde1233638176bf7a1152d72a907fb026815118bb9b2769f60c976727
??	.agents/memory/working/cli-development/tasks/workspace-library-destination-projections.md	664	30e2899c940899e641fe961d461af8769efc2fe928555c171413ee35ee01aefe
```

## extensions-evolution-preparation

- Worktree: `../open-forge-worktree/extensions-evolution-preparation`
- Branch: `codex/extensions-evolution-preparation`
- HEAD: `8a153f23dabb05019eae2c15fae51331b9e88335`
- HEAD tree: `bafd3d1e3173ad9342bd25a6086330dcfbe5ee16`
- Dirty paths: 0
- Manifest SHA-256: `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855`

Clean at capture.

The deleted Library `RecoveryBundleEntry.cs` preimage SHA-256 is
`a28d62b1ed420877c0f20c580fc8acef27f615a315cc3f9b3a932d186448a3aa`.
The new canonical `RecoveryEntry.cs` is included explicitly above.
