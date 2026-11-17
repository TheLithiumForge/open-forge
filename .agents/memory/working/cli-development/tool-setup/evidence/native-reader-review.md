---
open-forge:
  description: Check F's frozen OpenClaw reader against the tagged CBM validation and lookup functions
  tags: [Memory, Working, CLI, Tool, Contextual, Evidence, Review]
---

# Native reader review

Native reader review is a source check that compares F's private OpenClaw reader with CBM v0.12.1 and the accepted port plan.

For example, a configured workspace must win over a well-formed root include while malformed unrelated data still blocks the document.

Status: Review accepted. No material conformance gap found. No NR1 finding is raised.

## Scope

F's frozen candidate uses base `f2200252bfff42c2b260df7375b92a468cf63263`.
Only the three new reader files, changed `ToolOpenClawRoots.cs` and two assigned test files were reviewed.
The [accepted plan](native-config-reader.md) defines the source map and the two declared host limits.
Other provider behavior and the other lanes weren't reviewed.
This isn't the fresh final whole-feature adherence review.

## Conformance

Production locations below are relative to `src/cli/root/OpenForge.Cli/Composition/Shared/ToolHomes/`.
The checked source traces are:

| F location                                                 | Native comparison and result                                                                                                                                                                                                                                                                                                  |
| ---------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Shared/OpenClaw/ToolOpenClawJsonSyntax.cs:13-225`         | Matches native [validation and lexical functions](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/config_json_like.c#L138-L631): full root object, complete consumption, exact trivia, byte identifiers, string rules, required separators, trailing commas and numeric validation without conversion.   |
| `Shared/OpenClaw/ToolOpenClawJsonSyntax.cs:55-100`         | Keeps native depth: root zero, containers through depth 63, scalars at 64 and rejection of containers at 64.                                                                                                                                                                                                                  |
| `Shared/OpenClaw/ToolOpenClawJsonStrings.cs:18-100`        | Matches [native key equality](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/config_json_like.c#L700-L867) for the four fixed ASCII targets. LF and CR continuations stay distinct from the Unicode continuation branch used for values.                                                                |
| `Shared/OpenClaw/ToolOpenClawJsonStrings.cs:42-133`        | Matches [selected decoding](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/config_json_like.c#L2724-L2842): native byte output, escapes, surrogate pairs and NUL rejection. Unrelated strings aren't decoded. Invalid decoded UTF-8 becomes unavailable under the plan's declared managed-string limit. |
| `Shared/OpenClaw/ToolOpenClawJsonStrings.cs:11-15`         | Strict BCL UTF-8 validation matches the rejection classes in [native UTF-8 validation](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/config_json_like.c#L1793-L1850), including unrelated values and comments.                                                                                         |
| `Shared/OpenClaw/ToolOpenClawConfigurationReader.cs:17-61` | Matches [native lookup](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/config_json_like.c#L3055-L3118): zero bytes return missing, selected duplicates and wrong shapes block, unrelated duplicates stay valid and only the selected string is decoded.                                                 |
| `ToolOpenClawRoots.cs:23-31` and reader `:33-36`           | Match [native precedence](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L2893-L2930). Root include presence blocks only after a missing workspace. Includes aren't opened. Empty or invalid configured paths don't fall back.                                                                    |

The existing eight MiB bound and decoded-string UTF-8 limit are kept as declared.
They aren't stronger native format promises.

## Source checkpoint

All six files match `artifacts/tool-home-roots/native-reader/source-sha256.txt` before and after review.
That manifest file's SHA-256 is `eab5fcc34c86b3b3b30cb74ec1c986276012a48d8304ed73e537721f8ed51376`.
The nine-path `changed-files.txt` SHA-256 is `75e8e353210ea8ccee62007ac46388e1284132c13b0e0763dcb3248d5f5d7ab0`.

The six-file content checkpoint is `c72327e8f458cb28cc1608090da61fc1a41f8ae724d16294f220215aaf3528c3`.
Sort repository-relative paths ordinally and hash UTF-8 rows of path, NUL, lowercase raw-file SHA-256 and LF.
The native C source matches the tagged hash recorded in the plan.

## Evidence

The two reviewed test files cover native lexical forms, complete validation, selected ambiguity, include precedence, empty files, depth, UTF-8, numeric skip and unchanged bytes.
The shared external-format oracle keeps native key and value escape expectations separate.

F's `evidence/native-home-roots.md:323-421` records 151 reader cases and 25 owned-file class cases passing with zero failures, warnings and skips.
It records a clean managed build in `00:02:02.02` and exit zero for both .NET checks.
Those 176 cases and checks were consumed without rerunning them.

Only this report and E's packet Progress and Evidence change.
The source review ran no product edits, build, tests, package or native execution, real stores, PATH, registry, commit or GitHub action.
The coordinator then authorized a separate local evidence commit of these two documents.
Its checks keep the exact prior E build receipt because product source and build configuration are unchanged.
Both required .NET checks pass with exit zero before the authorized commit.
Current Markdown formatting, six local links, LF, whitespace and the two-file limit pass.
The kept build receipt is `artifacts/task90-lane-e-review-docs/final-receipt.json`.
Fresh check artifacts are under `artifacts/task90-lane-e-native-reader-review/`.
The [lane E packet](../packets/lane-e.md) records the return.
