---
open-forge:
  description: Candidate follow-up for intermittent Windows integration failures where parallel Install fixtures fail together with DirectoryNotFoundException
  tags: [Memory, Working, CLI, Task, Potential, Contextual, Testing, Windows, Pipeline]
---

# Windows Install Fixture Race

## Question

Do parallel integration tests share machine state that one test removes while another Install writes into it? If so, is the race in the tests only, or can two real Open Forge commands on one machine meet it too?

## Evidence

- A local `test:built` on 2026-10-12 (win-x64, commit `8498d1af5`) failed six native-integration tests within about two seconds of each other. Five ended within 100 ms. Two re-runs of the same executable passed. Managed integration passed in the same run.
- Each failure came from an Install that did not complete. One reported `WriteFailed: DirectoryNotFoundException (0x80070003)` while a Framework fixture established a trusted Install. The failed tests: `ExtensionUpdatePlanningIntegrationTests.InstalledGeneratedCatalogueDoesNotClaimAnUpdate`, `StatusBeforeOutputSnapshotTests.HealthyWithExtension`, `RouteInitBeforeOutputSnapshotTests.FrameworkScaffold`, `InstallRouteSharingIntegrationTests.PrivateMetadataDiagnosisRemainsLocal`, `ExtensionListBeforeOutputSnapshotTests.OwnershipSelection` and `InstallConfigurationFrontmatterIntegrationTests.UnavailableExtensionSourceIsKept`.
- Hosted Builds failed only in Windows test steps four times in ten runs (win-arm64 three times, win-x64 once), with no public detail. [Task 71](../../../../archived/cli-development/tasks/task71-streamline-build-release-pipeline.md#timing-analysis) lists the runs. They may be the same race.
- Integration tests do not set `OPENFORGE_DATA_HOME`, so parallel tests share the signed-in user's data home, which holds workspace locks and recovery bundles ([CurrentUserDataHome.cs](../../../../../../src/cli/framework/OpenForge.Cli.Framework/Framework/Filesystem/PhysicalPaths/CurrentUserDataHome.cs)).

## Next steps

1. Read the failed tests' names in the next failing hosted Build. Task 71 now publishes them as annotations, and `npx forge ci:watch` prints them.
2. Find which test or store operation removes a shared directory while another writes. The CTRF `start` and `stop` times in `results.json` give the overlap.
3. Decide whether the fix belongs in the tests (an isolated data home per run) or in the store (tolerate a concurrently removed empty directory).
