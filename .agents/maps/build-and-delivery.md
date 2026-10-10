---
open-forge:
  description: Where the build, test, package and release system is documented and implemented, for work on delivery scripts, workflows or npm scripts
  tags: [LoadNow, Map, Build, Delivery, Pipeline, Release, CurrentTruth, Evergreen]
  applyTo: ["scripts/delivery/**", ".github/workflows/**", "package.json"]
---

# Build And Delivery

Route map for changing or running the delivery pipeline. Each destination keeps its own detail and authority.

- [current structure and rules of the delivery system: what runs locally and in GitHub, and how to qualify a release. Read it first.](../memory/crystallized/documents/cli/delivery-system.md) - #CurrentTruth #Evergreen #Build #Delivery #Pipeline #Document
- [every `forge` command with options and examples, and which local command each workflow step uses.](../../scripts/delivery/README.md) - #Evergreen #Build #Delivery #Tooling
- [package graph, staging, checksums and the complete-release publication rules.](../memory/crystallized/documents/cli/distribution.md) - #CurrentTruth #Evergreen #Distribution #Release #Document
- [contributor workflow for building, testing, packaging and releasing, including release triggers and npm credentials.](../../docs/development.md#build-and-test) - #Evergreen #Documentation #Development
- [declared platform skip reasons. Add a reason here before a test skips on a platform.](../../scripts/delivery/platform-skips.ts) - #Build #Testing #Platform
- [the Build, Release and Documentation workflows.](../../.github/workflows/) - #Build #Release #Pipeline
- [open pipeline work and its evidence.](../memory/working/cli-development/tasks/task71-streamline-build-release-pipeline.md) - #Contextual #Task #Pipeline
