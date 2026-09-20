# forge

`forge` gives local development and CI the same build, test and delivery
commands. It coordinates the repository's .NET and npm tools. Run it from the
repository root; no global install or separate build of this tool is needed.

```sh
npx forge --help
npx forge pack --help
```

Arguments go directly after the command. Both `--help` and `-h` work. Root npm
aliases remain available, such as `npm run build`; aliases need the usual
separator for flags: `npm run pack -- --skip-tests`.

## Start from a fresh checkout

Install Node/npm as specified in [package.json](../../package.json), the SDK in
[global.json](../../global.json), and your platform's native toolchain if you
want Native AOT output. The [development guide](../../docs/development.md#build-and-test)
has platform prerequisites. Node runs this TypeScript tool directly.

```sh
npx forge setup
npx forge test --no-restore
npx forge dist --no-restore --plan
npx forge dist --no-restore
```

`setup` runs `npm ci` and .NET restore. It replaces `node_modules`; it does not
install Node, the SDK or a native toolchain. `--offline` uses cached dependencies
and fails if required packages are unavailable. Builds restore by default;
`--no-restore` reuses an earlier restore. Choose one of these flags, not both.

## Choose a stage

| Command        | What it does                                                               |
| -------------- | -------------------------------------------------------------------------- |
| `setup`        | Install npm dependencies and restore .NET.                                 |
| `restore`      | Restore .NET only.                                                         |
| `build`        | Build the managed solution and development CLI in Release mode.            |
| `test`         | Build and run managed unit, integration and public-command tests.          |
| `build:native` | Build the current host's native CLI and test executables.                  |
| `test:built`   | Test existing native-build artifacts in six execution modes.               |
| `pack`         | Package existing artifacts and test their npm installation.                |
| `dist`         | Run `build:native`, `test:built`, then `pack`.                             |
| `dist:wrapper` | Build and pack only the npm wrapper, without .NET or native files.         |
| `version`      | Bump the root version and synchronize .NET, without committing or tagging. |
| `clean`        | Remove owned .NET and delivery outputs.                                    |

`dist --plan` shows stage names and effective arguments without executing them.
Each stage reports start, success or failure. Execution stops at the failed
stage and identifies the command or log. GitHub Actions uses separate
build/test/pack steps with those same commands.

The six `test:built` modes run on **one host**: unit tests once, integration
tests twice, and public-command tests three times. They exercise managed and
native combinations. Linux does not execute macOS or Windows tests.

Native commands detect the current host. `--rid` makes that choice explicit
and must match it. Supported RIDs are `linux-x64`, `linux-arm64`, `osx-x64`,
`osx-arm64`, `win-x64` and `win-arm64`.

## Pack without tests

From existing `build:native` artifacts:

```sh
npx forge pack --skip-tests
```

Or build and pack in one command:

```sh
npx forge dist --skip-tests --no-restore
```

The build still compiles test executables. Neither command executes tests.
Source and artifact integrity checks remain active. These archives are marked
untested and cannot enter native publication or release collection. To qualify
the same build later, run `test:built`, then ordinary `pack`.

Normal `pack` does not run .NET tests or rebuild: it requires passing
`test:built` reports and runs the npm installation check itself.

## Publish a wrapper for selected targets

The wrapper can be prepared without native artifacts on your machine:

```sh
npm ci --ignore-scripts
npx forge dist:wrapper --targets linux-x64,osx-x64,win-x64
npx forge publish:wrapper --tag preview --dry-run
```

That version lists exactly the three selected x64 packages as optional
dependencies. Omitting `--targets` selects all six. This option changes the
wrapper's dependency list; it does not cross-compile native executables.

On each selected host, build/test/pack its native package, then preview its
independent upload with `npx forge publish:native --tag preview --dry-run`.
Publish all listed native packages at the same version before the wrapper.
The wrapper provides the `open-forge` npm command; native packages contain the
executable payload. Both include the license.

`--dry-run` validates local artifacts and prints a plan without registry
contact. **Removing it uploads to the configured npm registry.** Actual upload
requires committed matching source, current packages, credentials and an
explicit tag. Prereleases cannot use `latest`. Existing versions warn and skip
without retagging; an existing wrapper with different dependencies rejects the
run. Changing a published wrapper's targets requires a new version.

## Collect a release from several hosts

Pass the same `--targets` to `pack` or `dist` on each selected host. Copy each
host's package output into `artifacts/release-input/package-<RID>/`, then run:

```sh
npx forge release:collect artifacts/release-input artifacts/release --targets linux-x64,osx-x64,win-x64
npx forge publish:release --from artifacts/release --tag preview --dry-run
```

Collection requires tested packages with matching source, version and target
selection. It replaces the output directory and records the selection in
`release.json`. Omitted target artifacts are not required. Publication consumes
that exact selection, checks it before uploads, and publishes the wrapper last.
Uploads are not atomic; rerun the same publication to fill missing versions.
The default Actions workflows still build and release all six targets.

`release:select` is a GitHub Actions helper, not a local release command.
Its help describes the workflow environment it reads.

## Version and clean up

```sh
npx forge version patch
npx forge version prerelease --preid beta
npx forge version 0.1.0-beta.1
```

These are alternatives, not a sequence. npm updates `package.json` and
`package-lock.json`; the lifecycle hook updates `Directory.Build.props`.
Review and commit those files, then rebuild and repack. `version:sync` is that
hook and normally needs no manual invocation. `--sha` on `build:native`, `dist`
or `dist:wrapper` stamps an artifact with the current commit without bumping the
root version.

| Output                                           | Location                                    |
| ------------------------------------------------ | ------------------------------------------- |
| Managed development CLI                          | `artifacts/publish/open-forge-dev/Release/` |
| Native executables                               | `artifacts/publish/<RID>/`                  |
| Host reports and delivery metadata               | `artifacts/delivery/<RID>/`                 |
| Portable archive, two npm tarballs and checksums | `artifacts/delivery/<RID>/packages/`        |
| Standalone wrapper                               | `artifacts/delivery/wrapper/packages/`      |
| Collected release                                | The output passed to `release:collect`.     |

Run `npx forge clean` to remove `artifacts/bin`, `artifacts/obj`,
`artifacts/publish` and owned host/wrapper delivery outputs and managed reports. Dependencies,
offline caches, logs, collected releases and npm links remain. Restore again
before using `--no-restore`. Run commands sequentially within one worktree;
builds and packs replace their own output directories.

## Download development binaries

The GitHub Actions **Build** workflow produces a `binary-<RID>` artifact for each
platform whose native build succeeds. Open the workflow run, choose the artifact
for your platform, then extract its portable archive. It contains `open-forge`
(`open-forge.exe` on Windows), the license and a development-build notice.
The download also includes a SHA-256 checksum. Artifacts remain available for
14 days; creating them does not publish a release or an npm package.

Binaries are uploaded before the test gate. Check the run's test results before
using them: an available binary does not mean its tests passed. The separate
`package-<RID>` artifacts require successful tests and package-install checks.

Dependency setup restores each native executable for its host runtime and
self-contained configuration before `--no-restore` builds. Integration reports
allow only explicit exclusions for another operating system. Those exclusions
remain counted separately from passes; unexpected skips and capability failures
still fail qualification.

## Maintain the scripts

[commands.ts](commands.ts) defines commands, options, usage, guidance and examples.
[cli.ts](cli.ts) dispatches them; [options.ts](options.ts) parses shared options
and renders help. [dist-plan.ts](dist-plan.ts) declares the pipeline.
Focused modules implement each stage. [npm](npm/) contains package preparation
and the launcher; [release](release/) contains collection and release helpers.

Keep the entry point dependency-free so help and setup work before npm install.
To check delivery code and its owned behavior, run `npm run check:delivery` and
`npm run test:delivery`. Package layout and installed-command checks are also
available as `npm run test:package-layout` and `npm run test:package-manager`.
