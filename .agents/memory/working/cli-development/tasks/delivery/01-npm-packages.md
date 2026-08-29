---
open-forge:
  description: Create and prove the thin launcher and linux-x64 platform package from accepted native artifacts
  tags: [Memory, Working, CLI, Task, Distribution, Npm, Package, Contextual]
---

# Build The Thin Npm Package Graph

## Task State

- State: Planned after every command is accepted.
- Parent: [CLI Delivery](_delivery.md).

## Expected Outcome

`@thelithiumforge/open-forge` selects and invokes the exact current `linux-x64`
platform package. Both packages contain accepted native artifacts and metadata
only. They do not download, compile, implement behavior, or fall back to another
runtime.

## Package Model

- One launcher package with the public `open-forge` bin entry.
- One optional `linux-x64` platform dependency with exact Linux/x64 metadata and
  a synchronized version.
- The platform package contains one canonical executable, license, package
  metadata, and required notices only.
- Launcher selection admits only supported Node Linux/x64 facts and emits one
  bounded unsupported-platform error otherwise.
- Package source stays below the accepted `src/cli/root/` distribution boundary;
  packed output stays under root `/artifacts/`.

Additional platform packages require a later explicit maintainer decision. They
are not current D1 scope and are not implied by the launcher shape.

## Evidence

Pack both packages locally, inspect exact file inventories and modes, install the
launcher with matching tarballs in isolated npm projects, invoke help/version and
representative commands, prove stdout/stderr/exit parity with the direct binary,
and test unsupported and missing-platform cases. Hash the packaged binary against
the canonical `linux-x64` artifact.

## Stop Conditions

Stop before adding postinstall scripts, downloads, compilation, telemetry,
behavioral wrappers, legacy fallback, unaccepted platform packages, or publishing
any package.
