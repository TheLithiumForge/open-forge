---
open-forge:
  description: Record the bounded Root home and native client-path implementation and focused evidence
  tags: [Memory, Working, CLI, Tool, Contextual, Evidence]
---

# Native home roots

Native home roots is a Root provider that resolves Open Forge home, profile-native home and reviewed client path bindings from injected inputs.
For example, CBM can use HOME while CodeGraph on Windows uses USERPROFILE, without changing either child environment.

Status: Accepted reader correction on base `f2200252bfff42c2b260df7375b92a468cf63263`. F-H1 and F-H2 are closed. The provider API stays stable for C. Whole-feature adherence review follows complete integration and journeys.

## Contents

- [Applicability](#applicability)
- [Connections](#connections)
- [Native choices](#native-choices)
- [Binding coverage](#binding-coverage)
- [Sources and limits](#sources-and-limits)
- [Original checks](#original-checks)
- [ABI checks](#abi-checks)
- [Reader correction](#reader-correction)
- [Not decided yet](#not-decided-yet)

## Applicability

This slice reads local developer configuration and returns paths. It writes no product files and runs no tool, process or network request.
Git keeps the implementation and owned temporary fixtures provide recovery during focused tests.
Malformed configuration, missing paths, links and unavailable reads return errors or unavailable binding facts before later effects.
The existing physical-path resolver, YAML parser, immutable collections and path APIs cover home and Rovo resolution.
The dated I1 choice accepts a private port of CBM v0.12.1's read-only JSON-like grammar for OpenClaw after ordinary readers failed native compatibility.
Strict UTF-8 validation and codepoint encoding use BCL support. No package, writer, interpreter, native bridge or public parser API is added.
Invalid native syntax stays unavailable instead of being guessed.
Root Integration tests are the lowest allowed tier because they call the Root assembly and cross real owned filesystem paths.
The full managed and native gates stay in CI under the assigned verification limits.

## Connections

The existing constructor and call are:

```csharp
ToolHomeProvider(IReadOnlyDictionary<string, string> environment,
    Func<Environment.SpecialFolder, string> knownFolder,
    ToolOperatingSystem operatingSystem, CliToolHostInputs? inputs = null)
string HomePath { get; }
ToolHomeFacts Resolve(ToolVersionProfile profile)
```

Create one provider per selected operation.
Construction snapshots the injected environment and client map and creates deferred readers.
It calls no known folder, filesystem, process, download or PATH host.
`HomePath` resolves the injected Open Forge home or the OS-user home plus `.open-forge` once.
The OS-user and Windows runtime-parent known folders are each resolved through the given capability once.
An injected `CliToolHostInputs` takes precedence over the base environment and paths.
Its process, download and PATH members aren't used by this provider.

`ToolHomeFacts` has these required immutable values:

| Field               | Value                                                                                               |
| ------------------- | --------------------------------------------------------------------------------------------------- |
| `HomePath`          | Absolute resolved Open Forge home                                                                   |
| `BinPath`           | That home's bin path                                                                                |
| `NativeHomePath`    | Absolute resolved home for the chosen profile                                                       |
| `Clients`           | `ToolClientRoots` with immutable `Directories`, `Files`, `Unavailable` and combined `Bindings` maps |
| `NativeEnvironment` | Only the declared raw variables needed by the native-setting reader                                 |

The `ToolClientRoots` constructor takes `ImmutableDictionary<string, string> directories`, `files` and `unavailable`.
It creates `Bindings` once from the first two maps.
Its properties are get-only.
A file-valued config binding is the exact selected file, not its containing directory.
A missing conditional binding stays in `Unavailable` instead of getting an invented default.

Later C integration uses these returned values in its existing `ToolRecipeContext` delegate.
Keep C's current workspace and main root, and use A's verified native install directory.
Later D integration uses the Open Forge home and each selected profile's native home.
It uses CBM's combined bindings for native setup planning.
Native discovery still decides which clients exist.
B still checks the native inventory against independently resolved reviewed destinations.
An unavailable required binding blocks that planning step.
The provider doesn't accept returned inventory paths as its own bindings.

Proposed later C connection:

```csharp
var homes = provider.Resolve(profile);
return new ToolRecipeContext
{
    WorkspacePath = request.Locations.WorkspacePath,
    HomePath = homes.HomePath,
    BinPath = homes.BinPath,
    NativeHomePath = homes.NativeHomePath,
    NativeInstallDirectory = nativeInstallDirectory,
    MainRoot = request.MainRoot,
    ClientRoots = homes.Clients.Bindings,
    NativeEnvironment = homes.NativeEnvironment,
};
```

`nativeInstallDirectory` comes from A's verified invocation plan.
The snippet doesn't add another package-manager path resolver.
Full graph creation, command registration, Status and Doctor wiring stay for the later F slice.

## Native choices

The profile's `Node` policy uses Windows `USERPROFILE` or POSIX `HOME`.
An absent key uses the given OS-user capability.
A present empty value fails absolute-path resolution, matching the checked Node behavior.
CBM's `HomeThenUserProfile` policy uses nonempty `HOME`, then `USERPROFILE`, and blocks when both are unavailable.
An injected `NativeHomePath` overrides either policy.
The caller keeps the child environment consistent with that override for owned journeys.
The provider changes no child or process environment.

Windows environment keys compare without case. Other platform keys compare ordinally.
For a CodeGraph telemetry change, `NativeEnvironment` includes only present `DO_NOT_TRACK` and `CODEGRAPH_TELEMETRY` values.
It keeps empty values and raw text without deciding on/off precedence.
B produces normalized effective facts.
CBM's current profile gets an empty native-setting environment map.
Raw values aren't serialized, logged or persisted here.

The possible paths use E's committed catalogue and tagged CBM path functions.
Simple native overrides, tilde-aware overrides and registry overrides keep their different rules.
Registry paths that require an existing config stay unavailable until that file exists.
OpenCode prefers an existing `.jsonc` file and otherwise uses `.json`.
Rovo, Amazon Q and CodeBuddy keep their source-defined filename order.
Windows Hermes keeps its legacy directory only while the native directory is absent.
OMP keeps profile precedence over its agent-directory override and the native default after an invalid profile.

Rovo reads the documented YAML scalar through the existing Framework YAML parser. The shared contained-path resolver rejects the first escape from its client root, including an outside link that later re-enters. Native choices keep their lexical home input while returned bindings and the setting identity use physical paths.
OpenClaw validates its complete native document, then checks its saved workspace before environment and profile defaults.
It doesn't follow `$include` and doesn't guess a workspace through an unavailable config.
Missing and zero-byte files use the environment and profile fallback.
Both config reads are bounded to eight MiB and use strict UTF-8.
Directory and file paths resolve through the existing physical-path mechanism and existing ancestors.
Missing directories stay missing.

## Binding coverage

`ToolClientRootTests.CatalogueCoverage` derives required keys from every committed install and uninstall possible-output declaration.
It compares that set to the complete union of resolved and unavailable bindings on Windows, macOS and Linux policies.
An independent shared external-layout oracle checks the default selected files.
The complete-injection test gives every declared key and checks that none needs a known folder, native process or privileged host.
The keys form these groups:

| Group                 | Exact keys                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| --------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Built-in config files | `claude-code-config`, `codex-config`, `gemini-config`, `zed-config`, `opencode-config`, `antigravity-config`, `kilocode-config`, `vscode-config`, `cursor-config`, `windsurf-config`, `augment-auggie-config`, `openclaw-config`, `kiro-config`, `junie-config`, `hermes-config`, `openhands-config`, `cline-config`, `qwen-config`, `copilot-cli-config`, `factory-droid-config`, `crush-config`, `goose-config`, `mistral-vibe-config`, `grok-config` |
| Registry config files | `qoder-config`, `kimi-config`, `gitlab-duo-config`, `rovo-dev-config`, `amp-config`, `devin-config`, `tabnine-config`, `continue-config`, `visual-studio-config`, `trae-config`, `roo-code-config`, `amazon-q-config`, `codebuddy-config`, `ibm-bob-ide-config`, `ibm-bob-shell-config`, `pochi-config`, `sourcegraph-cody-config`, `omp-config`                                                                                                        |
| Other exact file      | `gitlab-duo-hooks`                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| Client directories    | `claude-code`, `claude-user`, `codex`, `kimi`, `kiro`, `hermes`, `qwen`, `copilot-cli`, `mistral-vibe`, `grok`, `devin`, `omp`, `vscode`, `shared-skills`, `cline-data`, `openclaw-workspace`                                                                                                                                                                                                                                                           |
| Report-only roots     | `codebase-memory-cache`, `codebase-memory-runtime`                                                                                                                                                                                                                                                                                                                                                                                                      |

This is 61 keys, with 43 file bindings and 18 directory bindings.
Pi's direct home-relative declarations need no additional client key.
The native detected-client set is neither reproduced nor narrowed by this table.

## Sources and limits

The [committed E report](upstream-recipes.md#client-bindings) and the [Tool design](../../../../crystallized/documents/cli/technical-designs/tool-setup.md#root-hosts) define this connection.
The native choice evidence is:

- CBM [CLI path helpers](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L2667-L3021), registry environment setup at lines 9111-9202 and native install file choices at lines 9680-10540.
- CBM [registry paths](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/agent_clients.c#L235-L607), including Rovo containment and config precedence.
- CBM [platform paths](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/foundation/platform.c#L499-L650) for home, app configuration and cache.
- CBM [runtime override](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/daemon/bootstrap.c#L227-L301) and [Windows runtime parent](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/daemon/ipc.c#L5537-L5615).
- Node's [Windows home rule](https://github.com/nodejs/node/blob/v24.16.0/deps/uv/src/win/util.c) and [POSIX home rule](https://github.com/nodejs/node/blob/v24.16.0/deps/uv/src/unix/core.c).

Research downloaded tagged C source to ignored `artifacts/tool-home-roots/upstream/` after the public web reader returned cache misses.
No downloaded source is used at product or test runtime.

The current limits are:

- The private reader follows the tagged CBM grammar, which includes JSON5 forms and its byte-based key rules. It doesn't promise every JSON5 spelling. Full document validation precedes fixed key lookup. Unrelated numeric tokens are skipped without conversion.
- Selected string decoding keeps the native byte result before BCL string conversion. Invalid UTF-8 output stays unavailable as the accepted managed path-string limit. Unrelated strings aren't decoded.
- The report-only CBM runtime root is its resolved rendezvous parent, from `CBM_RUNTIME_DIR`, the given Windows LocalApplicationData capability or the native POSIX parent. CBM chooses the identity-named child. Root doesn't derive a UID or SID, read daemon contents or claim an exact child path. Later reports must keep this scope visible.
- The normal raw-path overrides must resolve as absolute paths. Tilde expansion is applied only by the reviewed native rules that support it.

No shared contract or protected model changes are requested by this slice.

## Original checks

The original 49-case check uses base `b22e1ec393280935dea93a312f2a1aa337834ad5`.
Its final managed Windows x64 build uses SDK `10.0.101` and exits zero with zero warnings and errors.
Its artifact log is `artifacts/tool-home-roots/build-accepted.txt`.
The exact command is:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
```

The NuGet audit override stays on that local invocation.
`npm ci` passed before implementation checks without changing tracked dependency files.
Only the three new Integration classes ran.
Each final command exits zero, selects and executes its stated count, and reports zero failures, warnings and skips:

```powershell
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes.ToolHomeProviderTests --minimum-expected-tests 11 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes.ToolClientRootTests --minimum-expected-tests 23 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes.ToolClientConfigurationTests --minimum-expected-tests 15 --fail-warns on --fail-skips on --no-ansi --progress off
```

These are 49 passing cases.
The corresponding logs are `ToolHomeProviderTests-final.txt`, `ToolClientRootTests-final.txt` and `ToolClientConfigurationTests-final.txt` under `artifacts/tool-home-roots/`.
The platform-policy cases use owned host paths. They don't claim native execution on Linux or macOS.
Full suites and Native AOT stay for CI.

Both mandatory commands exit zero on the final source:

```powershell
npm run check:dotnet:whitespace
npm run check:dotnet:style
```

Their logs are `whitespace-final.txt` and `style-final.txt` in the same artifact folder.
Owned-file formatting ran only against the two assigned ToolHomes directories.
Git whitespace, LF, local links, report formatting and file limits pass.
Tracked source outside the new assigned paths is unchanged.
The earlier proposal files and records-review.md stay byte-identical.
The same artifact folder keeps `changed-files.txt`, `source-sha256.txt`, `test-artifact-sha256.txt` and `research-sha256.txt`.
The manifests cover the 18 slice paths, 15 source files, two executable artifacts and five downloaded tagged C sources.
The Root assembly copied beside the test assembly matches the built Root artifact byte for byte.

The original executable artifacts are:

| Artifact                                                                                  | SHA-256                                                            |
| ----------------------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| `artifacts/bin/OpenForge.Cli/release/OpenForge.Cli.dll`                                   | `22987864801100935707d8e740e4a4f8ae37ba84cb86d3ed30fbe9ae8163efb8` |
| `artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll` | `a528e5750ecd256228a660667358b42b14c0eb038537c47eb6d982ec1c3725ae` |

The first solution build found five test declaration errors, corrected before execution.
The first focused selections passed 48 of 49 cases.
The Rovo case exposed a quoted-scalar implicit-flag check, corrected before the final build.
The final source also uses static path tables and the shared contained-path resolver.
Affected checks were repeated after those changes.
No flake or snapshot update is involved.

The candidate adds ten production files below Root `Composition/Shared/ToolHomes/` and five test files below the matching Integration path.
Only this new receipt, root-integration.md and the packet change outside those directories.
The new routed file is this receipt. The coordinator's later Index pass handles it.
Central composition, A's Tools folder, protected models and contracts, other leaves and packets and the parent Task stay unchanged.
No provider or test runs a native process, network request, PATH or registry write, shell profile edit, installation or Tool state publication.
No commit, push or merge ran.

## ABI checks

The coordinator fast-forwarded this worktree to `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9` and kept the slice.
The committed tree is `20c25b59229c7907411ba616e1d696e88e2638e5`.
All 15 source hashes matched the original checked candidate before adaptation.
The current ABI affects these connections:

| Shared input                                                                        | Required adaptation                                                                              |
| ----------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| `IToolDownloadHost.ReadNpmMetadataAsync(ToolNpmMetadataRequest, CancellationToken)` | Add the same throwing declaration to `ToolHomeFixture.UnusedHosts`                               |
| `ToolPathSetupResult.PendingBackups` and `ToolPathRemoveResult.PendingBackups`      | None. The unused-host fake throws and constructs no result                                       |
| `ToolNpmChannel.EntryRelativePath`                                                  | None. The committed catalogue constructs the channel. Home resolution doesn't select npm entries |
| `IToolRecordService.AddAllowInstallPathsAsync`                                      | None. This provider and its owned tests don't implement the record service                       |
| `CliToolHostInputs`                                                                 | Unchanged. Existing environment, homes, roots and privileged-host injection still fit            |

The sole source delta is that new throwing metadata method.
Removing that one line restores the fake's original hash.
All ten production files and the four other owned test files are byte-identical.
The fake is used by `ToolHomeProviderTests` and `ToolClientConfigurationTests`, so those two classes are the affected executable selection.
`ToolClientRootTests` keeps its earlier 23-case result. Its source, CBM catalogue inputs and production behavior are unchanged.
No provider constructor, immutable field or later integration call changes.

The current managed Windows x64 build uses SDK `10.0.101` and exits zero with zero warnings and errors.
It takes 7 minutes 31.15 seconds.
The log is `artifacts/tool-home-roots/abi/build.txt`.
The exact build and affected test commands are:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes.ToolHomeProviderTests --minimum-expected-tests 11 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes.ToolClientConfigurationTests --minimum-expected-tests 15 --fail-warns on --fail-skips on --no-ansi --progress off
```

The two test commands exit zero and execute 11 and 15 cases respectively.
Their logs are `ToolHomeProviderTests.txt` and `ToolClientConfigurationTests.txt` in `artifacts/tool-home-roots/abi/`.
All 26 cases pass with zero failures, warnings and skips.
The provider class takes 9.700 seconds and the configuration class takes 7.871 seconds.
Only those two affected classes run in this continuation.
The 23 original binding cases aren't presented as a fresh execution on this base.
The fresh Root copy beside the test assembly matches the built Root assembly byte for byte.

The current executable artifacts are:

| Artifact                                                                                  | SHA-256                                                            |
| ----------------------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| `artifacts/bin/OpenForge.Cli/release/OpenForge.Cli.dll`                                   | `6af37fb1fa2dce32323e8188b6bd081ea8119196931a38f0f0b4a1333e532402` |
| `artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll` | `7a9ab3f3c0c60298bdd423c746c22876f88af473396ecc38eebf3a3cf16277ed` |

`npm run check:dotnet:whitespace` and `npm run check:dotnet:style` exit zero on the final source.
Their logs are `whitespace.txt` and `style.txt` in the same ABI artifact folder.
Build, package and formatting configuration is unchanged from the original `npm ci` pass, so dependency installation isn't repeated.
Report formatting, local links and anchors, LF, trailing whitespace, exact file limits and unchanged tracked source pass.
The packet's supplied build indentation stays its sole formatter difference, outside Progress and Evidence.
The proposal report, proposal patch and records review stay byte-identical.

The ABI artifact folder keeps these exact manifests:

| Manifest                                                                                                    | Contents                                                                     |
| ----------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------- |
| `changed-files.txt`                                                                                         | 18 paths in the complete home-provider slice                                 |
| `source-sha256.txt`                                                                                         | All 15 current source hashes                                                 |
| `adapted-files.txt`                                                                                         | The one fake changed for this continuation                                   |
| `shared-inputs-sha256.txt`                                                                                  | Eight current shared ABI and catalogue inputs                                |
| `configuration-sha256.txt`                                                                                  | Seven unchanged build, package and formatting inputs                         |
| `test-artifact-sha256.txt`                                                                                  | The two fresh executable artifacts                                           |
| `report-sha256.txt`                                                                                         | The three final assigned Working files                                       |
| `untracked-files.txt`                                                                                       | Exactly 21 untracked files, including the three unchanged historical reports |
| `predecessor-source-sha256.txt`, `predecessor-test-artifact-sha256.txt` and `predecessor-changed-files.txt` | The original 49-case candidate manifests                                     |

This continuation changes one fake and the three assigned Working files.
It changes no provider behavior, client root, parser, dependency, protected model, A host, central composition or peer file.
Full Root graph and registration still wait for checked core integration.
The coordinator accepted these ABI checks and authorized an intermediate local commit with the existing files and reports.
This increment records F-H1 and F-H2 without claiming full native syntax support.
It reuses the just-passed build and mandatory checks. Only the three assigned Working statuses change before the commit.
At that intermediate checkpoint, the coordinator chose a bounded read-only C# port of the tagged CBM grammar and path lookup after the checked published libraries failed native validation or duplicated behavior.
Parser work waited for the dated contract increment and E's exact functions, licence and fixture plan in `r_0a42fc165fa0`.
That increment added no parser, package, native runtime or writer.

## Reader correction

The coordinator integrated the provider base and the [native reader contract](../../../../crystallized/documents/cli/technical-designs/tool-setup.md#native-config-reader) with dated I1 at `f2200252bfff42c2b260df7375b92a468cf63263`.
F implements only the six source and test files in E's closed `native-config-reader.md` plan from the read-only peer tree.
C handles central Root composition on the existing provider API.
No provider constructor, fact field or context delegate changes.

The three new private files are below `ToolHomes/Shared/OpenClaw/`:

- `ToolOpenClawConfigurationReader.cs` - Validate the document, count selected byte-key matches and read `agents.defaults.workspace`, with the root include guard on missing paths.
- `ToolOpenClawJsonSyntax.cs` - Keep native trivia, strings, identifiers, recursive shapes, separators and numeric token skipping.
- `ToolOpenClawJsonStrings.cs` - Keep key matching and selected string decoding as separate native paths. Use strict BCL UTF-8 validation and `Rune` encoding.

`ReadWorkspace(ReadOnlySpan<byte>)` is internal to Root.
It returns configured text, including an empty string, or `null` for a missing selected path.
Invalid syntax, ambiguous selected names, wrong shapes and a missing path with a root include throw `IOException` into the existing unavailable result.
Unrelated duplicate members stay valid.
A configured workspace wins over well-formed root include values and duplicates.
No include is opened.
Zero bytes return missing. Whitespace-only and comment-only documents are invalid.

`ToolOpenClawRoots` calls the reader after the existing bounded file snapshot.
Its four JSON key constants move to the private reader and the replaced `Member` helper is removed.
Home, environment, profile and physical-path choices stay with their current code.
The root container is depth zero. Containers at depth 63 and scalar values at depth 64 are allowed. Containers at depth 64 are rejected.
The caller's eight MiB read bound stays unchanged. Native CBM's sixteen MiB file bound is outside this syntax correction.
Document keys aren't limited by the native lookup-argument limit.

The checked primary sources are:

| Tagged file                              | SHA-256                                                            |
| ---------------------------------------- | ------------------------------------------------------------------ |
| CBM v0.12.1 `src/cli/config_json_like.c` | `12f2f8c7e80a28a15b6ad3d5e62a8273fcd042904596dabe72748618ad3d2903` |
| CBM v0.12.1 `src/cli/config_json_like.h` | `556b5e494b31f2fca01436da163c0e1f9cdb343d01369a9845290858fbe06e65` |

The public web reader returned a cache miss. Authorized public-source retrieval saved those exact bytes under `artifacts/tool-home-roots/native-reader/`.
The port includes no native file snapshots, writer, editor, schema matcher or test hook.
Each derived production source identifies the tagged source and DeusData's MIT notice.
Root has retained the complete upstream notice in its pending `LICENSE` addition, with words unchanged and line endings and trailing blank spaces normalized. That addition stays with Root. F doesn't edit the file.

One source-mapped external-format oracle lives with `ToolClientConfigurationTests` at the nearest shared test scope.
The new private-reader test class and changed owned-file class use it.
Semantic names and depth/read limits use their production definitions.
Cases check configured and missing values, complete validation, ambiguity, separators, native trivia, escape branches, numeric skip, UTF-8, depth, read bounds, precedence and unchanged bytes.
No rendered text assertion or payload snapshot changes.

The final managed Windows x64 build uses SDK `10.0.101` and exits zero with zero warnings and errors in `00:02:02.02`.
Its log is `artifacts/tool-home-roots/native-reader/build-corrected.txt`.
The exact build and focused execution commands are:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes.Shared.OpenClaw.ToolOpenClawConfigurationReaderTests --minimum-expected-tests 151 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-build --no-restore -- --filter-class OpenForge.Cli.IntegrationTests.Composition.Shared.ToolHomes.ToolClientConfigurationTests --minimum-expected-tests 25 --fail-warns on --fail-skips on --no-ansi --progress off
```

Filtered `--list-tests --no-ansi --progress off` discovery first finds 151 and 25 cases.
The corresponding discovery logs are `reader-discovery.txt` and `configuration-discovery.txt` in the same artifact folder.
Both execution commands exit zero and execute those same counts.
The new reader class passes 151 cases in 1.719 seconds.
The changed owned-file class passes 25 cases in 2.328 seconds.
All 176 cases pass with zero failures, warnings and skips.
Their logs are `ToolOpenClawConfigurationReaderTests.txt` and `ToolClientConfigurationTests.txt`.
No other test class runs.
The obsolete unavailable expectation for an unquoted missing path is removed and the native missing outcome is covered by the new reader cases.

Both mandatory commands exit zero on the final source:

```powershell
npm run check:dotnet:whitespace
npm run check:dotnet:style
```

Their logs are `whitespace.txt` and `style.txt` in the same folder.
The source and configuration hashes are checked again after execution.
The seven build, package and formatting inputs still match the earlier `npm ci` and restore inputs.
That dependency setup isn't repeated.
The first build reports five fixture annotation errors after a reflection-based string serializer call triggered formatter fixes.
The fixture now uses `JsonEncodedText.Encode`. The generated attributes and reflection-dependent serializer calls are removed.
The corrected build and both focused classes pass. No test flake occurs.

The final executable artifacts are:

| Artifact                                                                                  | SHA-256                                                            |
| ----------------------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| `artifacts/bin/OpenForge.Cli/release/OpenForge.Cli.dll`                                   | `8f38b24c22c696923d4cde8d9cf3a4e943a2db71ec589a23fd99fbce3beb1871` |
| `artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll` | `2610bbb73a637d3e2faa72d36909746634d9a798a78bbf383bf047b126b7918b` |

The Root copy beside the test assembly matches byte for byte.
The artifact folder keeps `changed-files.txt` for exactly nine candidate paths, `source-sha256.txt` for all six assigned source files, `configuration-sha256.txt` for seven unchanged inputs, `test-artifact-sha256.txt` for both executables, `upstream-sha256.txt` for both tagged reader sources and `report-sha256.txt` for the final three Working files.
Prettier, local links and anchors, LF, whitespace, packet section limits and exact allowed-path checks pass before return.
The packet keeps its original supplied build indentation as its sole formatter difference outside Progress and Evidence.
No new routed Markdown file is added.

F-H1 and F-H2 are fixed in this candidate. Include precedence stays correct.
The provider API and privileged A hosts, central Root, protected models and contracts, LICENSE, peer files, other packets and the parent Task are unchanged.
No native tool, real store, process host, registry or persistent PATH write, profile edit, writer, package, payload fixture update or snapshot regeneration runs in the provider or tests.
No full suite, Native AOT, Doctor, commit, push, merge or integration runs.
Root integrates its pending complete MIT notice before shipping.

### Commit continuation

The coordinator accepts this correction and authorizes the conventional local nine-file commit.
E's focused source/native comparison in `r_73e82470b77f` reports no material conformance gap and no NR1 finding in its `evidence/native-reader-review.md`.
The six frozen source hashes match before and after that review.
The accepted content checkpoint is `c72327e8f458cb28cc1608090da61fc1a41f8ae724d16294f220215aaf3528c3`.
The source manifest SHA-256 is `eab5fcc34c86b3b3b30cb74ec1c986276012a48d8304ed73e537721f8ed51376` and the nine-path manifest SHA-256 is `75e8e353210ea8ccee62007ac46388e1284132c13b0e0763dcb3248d5f5d7ab0`.
The accepted 176 cases, clean managed build and both mandatory .NET checks stand. No executable check is repeated for this status update.
Only the three assigned Working statuses change before the commit. The six source files stay frozen.
Final Markdown, LF, whitespace, hashes and exact staging checks cover all nine assigned files.
No LICENSE, central Root, A host, protected type, peer file, push, merge or payload capture is part of the commit.
This closes F-H1 and F-H2. E's focused check doesn't replace the later whole-feature adherence review.

## Not decided yet

- The first shipping Open Forge version. This slice changes no version.
