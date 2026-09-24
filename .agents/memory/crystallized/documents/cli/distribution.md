---
open-forge:
  description: Accepted public package graph, x64 and ARM64 platform horizon, staging, packing, checksum, proof, and publication boundary for the replacement CLI
  responsibility: Define durable replacement CLI distribution meaning without presenting incomplete platform work or release evidence as complete
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, Distribution, Npm, NativeAOT, Release]
---

# CLI Distribution

## Status And Authority

This document defines the accepted public package graph, platform horizon,
package identities, staging and packing boundary, checksum ownership, proof
boundary, and complete-publication rule for the CLI. Release qualification and
publication are recorded separately from package-graph design below.

[Task 7](../../../archived/cli-development/tasks/delivery/01-npm-packages.md)
defines package-graph realization, npm release, and explicit local-linking work.
[Task 13](../../../archived/cli-development/tasks/delivery/02-native-ci.md)
defines native CI and artifact preparation for all six accepted targets. Their records and Git retain
implementation state and evidence receipts.

## Accepted Package Graph

The default public graph contains exactly one main package and six
platform packages:

| Package                                    | Native payload                 | npm platform metadata                        |
| ------------------------------------------ | ------------------------------ | -------------------------------------------- |
| `@thelithiumforge/open-forge`              | Thin launcher only             | Platform-independent main package            |
| `@thelithiumforge/open-forge-linux-x64`    | `linux-x64` glibc executable   | `os: [linux]`, `cpu: [x64]`, `libc: glibc`   |
| `@thelithiumforge/open-forge-darwin-x64`   | `osx-x64` executable           | `os: [darwin]`, `cpu: [x64]`                 |
| `@thelithiumforge/open-forge-win-x64`      | `win-x64` executable           | `os: [win32]`, `cpu: [x64]`                  |
| `@thelithiumforge/open-forge-linux-arm64`  | `linux-arm64` glibc executable | `os: [linux]`, `cpu: [arm64]`, `libc: glibc` |
| `@thelithiumforge/open-forge-darwin-arm64` | `osx-arm64` executable         | `os: [darwin]`, `cpu: [arm64]`               |
| `@thelithiumforge/open-forge-win-arm64`    | `win-arm64` executable         | `os: [win32]`, `cpu: [arm64]`                |

The main package uses exact synchronized optional dependencies on the platforms
selected for that version, with all six as the default. They are not peer dependencies. Every staged public manifest
uses the same release or development version. The main package alone owns the
`open-forge` executable mapping. Each platform package contains only its
manifest, license, and native executable.

The maintainer accepted ARM64 alongside x64 on Linux, macOS, and Windows on
2026-09-09. Qualification of that graph is recorded below. Additional operating
systems, architectures, RIDs, libc variants,
channels, or support-floor claims require a new maintainer decision.

## Current Implementation And Delivery State

The maintainer authorized hosted pipeline fixes and public beta publication on
2026-09-24. Version `0.9.0-beta.1` uses source
`89438d39015aee71f21e6d2d5a67427f7ab7e520`.
[Build 35942428820](https://github.com/TheLithiumForge/open-forge/actions/runs/35942428820)
passed shared checks, managed and Native AOT suites, and installed npm package
journeys on Linux, macOS and Windows, each on x64 and ARM64. The earlier
Linux-only qualification boundary is superseded by these matching-host results.

The [GitHub prerelease](https://github.com/TheLithiumForge/open-forge/releases/tag/v0.9.0-beta.1)
contains all six portable archives and `SHA256SUMS`. Published archive digests
match the checksum manifest. Task 7 and Task 13 retain the earlier implementation
history and its more limited local evidence.

[npm publication](https://github.com/TheLithiumForge/open-forge/actions/runs/35945963855)
published all seven packages at `0.9.0-beta.1`. Their public exact-version records
and `beta` tags agree, and the main package lists all six native packages at that
exact version. Use `npm install -g @thelithiumforge/open-forge@beta` to select
the beta channel.

Fresh public npm installs by exact version and by `@beta` passed Framework and
Core Templates installation, `status`, and `doctor` on Windows x64. The native
executable matches the qualified build and GitHub download byte for byte.
The [beta release record](../../../archived/cli-development/tasks/task51-beta-release.md)
retains the fixes, publication runs and verification details.

## Main Launcher Boundary

The launcher is thin. It recognizes only the accepted host/platform pairs,
resolves the matching optional package, and directly hands process arguments,
streams, environment, and process state to the native executable. It contains no
CLI domain behavior, download, postinstall compilation, fallback runtime,
telemetry, or hidden installation path.

Repository-local `open-forge-dev` publication and local development linking are
private developer tooling. They are not public package identities, release
channels, or fallback behavior.

## Versions And Local Artifacts

The root package.json owns the product version. Native npm version handling
calculates the next version or accepts an explicit version without creating a
commit or tag. A small npm lifecycle hook projects that exact value to
one .NET version property for direct .NET/IDE builds. Informational version derives
from that property. The seven npm manifests and exact optional dependencies
are generated during staging from the selected version and platform table;
there are no tracked npm manifest copies to synchronize. The version:bump
script delegates directly to npm version --no-git-tag-version. Future shims
consume the same selected version; they do not bump independently.

Local build, test and pack commands are the same root package scripts used by
CI. Native build defaults to the matching host, and optional SHA qualification
produces a development prerelease without changing tracked versions. The
artifact manifest records source commit, effective version, RID, binary identity
and qualification. Dirty local builds remain distinguishable from release
candidates. Tests and packaging reject missing or mismatched artifacts and
preserve the native payload bytes.

Portable archives contain the native executable and license for one supported
OS/architecture. They need no npm installation. npm tarballs contain the thin
launcher and appropriate native package. All local outputs remain under ignored
artifacts; explicit native and wrapper publication commands consume validated
tarballs independently, while the pipeline coordinates complete releases.

## Staging, Packing, And Checksums

Package-owned tooling stages the main package plus the selected host packages
from exact native artifacts into caller-owned ignored artifact roots. Staging
compiles the launcher closure, copies the supplied native artifact and license,
emits the exact synchronized manifests, and does not publish or contact a
registry.

Release proof packs the staged main and applicable platform packages, installs
their tarballs together in an isolated prefix, resolves the installed main
launcher, and invokes the exact native payload on a matching native host. Each
journey asserts Open Forge package placement, launcher reachability, argument and
process handoff, and the candidate version. It does not test npm, Node, the
operating system, or unrelated CLI command behavior.

SHA-256 manifests bind the exact accepted native executable and packed packages
to the candidate source commit. GitHub provides source archives for the release
tag; the pipeline does not create another source archive. A checksum pass proves
traceable identity and integrity. Byte-for-byte reproducibility requires a
separate repeated-build comparison and is not implied by one manifest.

Task 7 owns the synchronized six-target package graph, staging and packing,
and package-owned journeys. Task 13 owns native build and smoke, packed
installation and invocation, checksums, and bounded artifact collection on a
matching native host for each accepted target. It separately consumes the
Architecture's managed and Native AOT test evidence. Emulation or cross-build
success cannot substitute for native execution. Release qualification must
distinguish matching-host runtime evidence from static inspection.

Local packaging can explicitly bypass tests with pack --skip-tests, or build
and package through dist --skip-tests. Source and artifact identity checks stay
mandatory. Skipped outputs record tested: false, do not claim installation proof,
and cannot enter native publication or complete release collection. Normal pack
still requires successful .NET qualification and tests the installed npm package.

## Publication Boundary

The maintainer accepted independent local publication commands as a worktree
trial on 2026-09-12. `dist` qualifies the current host; `dist:wrapper` separately
compiles and packs the main wrapper without native files or .NET. The native
and wrapper publishers each select one validated existing tarball, require an
explicit tag, and require committed matching source for an actual upload.
Their dry runs print an offline publication plan without registry contact.
Wrapper manifests default to all six exact-version optional dependencies.
The maintainer also accepts an explicit target selection for one published
version. `dist:wrapper`, `pack` and `dist` accept `--targets` and generate exactly
that dependency graph. Collection requires the same selection from every
selected host and records it in release.json. Publication consumes that graph;
omitted platforms are not required or uploaded. A changed selection requires
repacking and, after publication, a new version. Successful
publication of one package is not a complete release, and an unavailable native
package leaves its platform unsupported at that version. The following
complete-release rules apply to the release coordinator.

The release coordinator supports manual source-ref/commit selection and
automatic version-tag invocation. Its destination selects GitHub, npm or all
implemented destinations. The default Actions release prepares and tests all six targets before any
publication begins. A supplied build run must be successful, from the expected
repository/workflow, and match the selected source commit and version; otherwise
the reusable build workflow produces the candidate once.

The Actions release uses only build.yml and release.yml. Build's one
six-host matrix calls setup and the explicit build:native, test:built and pack
stages declared by the local dist pipeline;
verify runs once in a shared-check job. Each host builds, tests and packages
before uploading its finished package set. Release collection consumes those
packages directly, and npm publication shares archive validation and upload
code with the individual local publishers. The release publisher validates every
native package and wrapper in the recorded selection before uploading any and places the wrapper last. Its input
is collected packages, independent of the publication host and native build
directories. Intermediate-only historical builds do not supply the new inputs.

GitHub receives one release with the complete portable target set and checksums.
npm publishes every platform package at the synchronized version before the
main package. No command subset, platform subset presented as complete,
partial graph presented as a successful release, or warning-bearing artifact is
accepted. Publication across packages and services is not a transaction: a
failure can leave earlier uploads present. Before any npm upload, the shared
publisher queries every selected exact version. An existing version emits a
warning and is skipped without retagging when its identity matches. Existing
wrappers must have exactly the selected dependencies; a mismatch rejects before
any upload and requires a new version. Only npm E404 means missing. Other
lookup failures stop publication before uploads. Rerunning the same candidate
therefore fills missing packages in native-first order. Availability checks do
not prove remote byte equality or make uploads atomic. Dry runs stay offline.
A skip warning is a publication status, not a warning-bearing build artifact.
Current CI authentication remains NPM_TOKEN.
The replacement is shipping only after all selected destination results agree.
Prerelease versions use a prerelease channel; stable versions use latest.

Credentials, registry contact, remote publication, signatures, SBOM, provenance,
OIDC attestation, and support-floor matrices remain outside local preparation
unless their exact release Task authorizes them. The package graph, checksums,
documentation, native proof, and publication result must agree before the
maintainer accepts the replacement as shipping.

## Related Current Sources

- [Replacement CLI Architecture](architecture.md)
- [Task 7: npm Package Manager Release and Local Linking](../../../archived/cli-development/tasks/delivery/01-npm-packages.md)
- [Task 13: Native CI and Reproducible Artifacts](../../../archived/cli-development/tasks/delivery/02-native-ci.md)
- [CLI Delivery](../../../archived/cli-development/tasks/delivery/_delivery.md)
- [Repository-Root CLI Tooling Decision](../../decisions/repository-root-cli-tooling.md)
