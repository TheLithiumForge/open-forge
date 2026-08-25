---
open-forge:
  description: Create and prove the thin launcher package and six platform packages from accepted native artifacts
  tags: [Memory, Working, CLI, Task, Distribution, Npm, Package, Contextual]
---

# Build The Thin Npm Package Graph

## Task State

- State: Planned after every command is accepted.
- Parent: [CLI Delivery](_delivery.md).

## Expected Outcome

`@thelithiumforge/open-forge` selects and invokes one of six exact platform
packages. Packages contain accepted native artifacts and metadata only. They do
not download, compile, implement behavior, or fall back to another runtime.

## Package Model

- One launcher package with the public `open-forge` bin entry.
- Six optional platform dependencies with exact OS/CPU metadata and synchronized
  versions: Windows x64/arm64, Linux x64/arm64, macOS x64/arm64.
- Each platform package contains one canonical executable, license, package
  metadata, and required notices only.
- Launcher selection is deterministic from supported Node platform/architecture
  facts and emits one bounded unsupported-platform error.
- Package source stays below the accepted `src/cli/root/` distribution boundary;
  packed output stays under root `/artifacts/`.

## Evidence

Pack every package locally, inspect exact file inventories and modes, install the
launcher with matching tarballs in isolated npm projects, invoke help/version and
representative commands, prove stdout/stderr/exit parity with direct binaries, and
test unsupported and missing-platform cases. Hash packaged binaries against the
canonical native artifacts.

## Stop Conditions

Stop before adding postinstall scripts, downloads, compilation, telemetry,
behavioral wrappers, legacy fallback, or publishing any package.
