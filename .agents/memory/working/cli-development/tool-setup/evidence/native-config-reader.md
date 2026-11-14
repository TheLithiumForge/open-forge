---
open-forge:
  description: Plan the private OpenClaw config reader from CBM v0.12.1 after checking ordinary JSON5 packages
  tags: [Memory, Working, CLI, Tool, Contextual, Evidence, Design]
---

# Native config reader

The native config reader is a proposed private C# port that validates CBM's JSON-like format and reads the configured OpenClaw workspace.

For example, `{agents:{defaults:{workspace:'~/repo'}},unused:1e9999}` gives the configured path without converting the unrelated number.
F then expands the path against the native home and resolves it through the existing path checks.

Status: Plan accepted for Root's dated contract choice.

## Choice

The accepted plan dated 2026-11-14 ports only CBM v0.12.1's read-only document validation, selected key comparison and selected string lookup into Root ToolHomes.
The user delegates the smallest reversible unresolved choice to Root.
The coordinator reports that Root has authored the dated contract choice.
Root keeps that choice and the maintainer question below in its own increment before F implements the reader.
This report is design evidence for that increment.

This exception covers F-H1 and F-H2 for one native configuration file.
It creates no general public parser, package, interpreter, native bridge, writer or JSON normalizer.
The reader consumes one caller-provided snapshot and returns configured text or missing.
Invalid input uses the existing unavailable result through `IOException`.
It changes no account files or native discovery.

The ordinary-reader investigation checked the current central dependencies first.
They contain no JSON5 reader.
The two published alternatives fail these native requirements:

| Alternative        | Checked API                      | Required behavior it loses                                                                                                                                                                                                                                                                                                                                        |
| ------------------ | -------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Json5` 1.0.4      | `Json5.Json5.ParseDocument(...)` | The [writer entry](https://github.com/devlooped/json5/blob/v1.0.4/src/Json5/Json5Writer.cs#L10-L19) omits trailing-content validation. The [tokenizer](https://github.com/devlooped/json5/blob/v1.0.4/src/Json5/Json5Tokenizer.cs#L51-L174) accepts missing separators. Numeric conversion loses raw values. Its other `Parse` path rejects unrelated duplicates. |
| `Json5Core` 1.0.12 | `Json5Core.Json5.Parse(string)`  | The [object reader](https://github.com/lofcz/Json5Core/blob/v1.0.12/Json5Core/JsonParser.cs#L210-L287) overwrites duplicate keys before a selected-path check can detect them.                                                                                                                                                                                    |

The port follows CBM's tagged format, including its narrower whitespace rules and byte-based identifier rules.
It doesn't promise all JSON5 syntax.
Reconsider this exception when an ordinary maintained reader can keep these outcomes through a supported data API.

## Reader contract

The only proposed caller-facing API stays internal to the Root assembly.
Its namespace is `OpenForge.Cli.Composition.Shared.ToolHomes.Shared.OpenClaw`.

Proposed:

```csharp
internal static class ToolOpenClawConfigurationReader
{
    internal static string? ReadWorkspace(ReadOnlySpan<byte> source);
}
```

The API takes UTF-8 bytes from the existing bounded configuration read.
The existing read first checks physical paths and rejects invalid UTF-8.
F can encode that unchanged text back to UTF-8 at the call without changing shared file-reading behavior.
The pure byte reader also rejects malformed UTF-8 when called directly.
It performs no file or environment access.

The result follows this ordered flow:

1. For zero bytes, return `null`. The existing file reader handles a missing file before this call. Both cases permit native fallback and close F-H2.
2. For nonempty input, validate UTF-8 and the entire root object before reading any selected value. Require complete consumption after permitted whitespace and comments. Whitespace-only and comment-only inputs are invalid.
3. Look up `agents`, then `defaults`, then `workspace`. Compare keys with native `jl_key_equals` behavior. Each selected name must occur at most once in its containing object. A present `agents` or `defaults` value must be an object. A missing selected name ends this lookup as missing. A duplicated selected name or wrong shape throws `IOException` and prevents fallback.
4. If `workspace` exists, require a single-quoted or double-quoted string and decode only that value with native `jl_decode_string_value` behavior. Return its decoded text, including an empty string. F's existing path expansion rejects an empty or unsupported path without trying fallback. A failed string decode throws `IOException`.
5. Only when the selected path is missing, check the root `$include` key. Any occurrence prevents fallback, including an empty string, non-string value or duplicate. Return `null` only when that key is absent. Nested `$include` keys have no effect. Open no include file.
6. When the reader returns `null`, keep F's existing precedence: nonempty `OPENCLAW_WORKSPACE_DIR`, then the native home and profile default. When it returns a string, use that value before those fallbacks.

A configured workspace takes precedence over a well-formed root `$include`, even when that include is duplicated or has a non-string value.
Malformed syntax anywhere still fails step 2.
Unrelated duplicate names stay valid.
Don't put members into a dictionary or decode every key through a different JSON5 implementation.
Native key comparison and selected-value decoding have different escape branches and need separate ports.
Keep the native selected decoder's byte output before creating the managed string.
If those decoded bytes aren't valid UTF-8, use the existing unavailable result through `IOException`.
This is a managed path-string limit. Don't repair the bytes or turn a non-ASCII identity escape into different native bytes.

Keep the input snapshot unchanged.
Private source offsets locate values and count matches.
Numbers and unrelated values are validated and skipped, without conversion, rounding or string substitution.
The grammar keeps CBM's 64-container depth limit with the root at depth zero.
The existing Root file-read bound stays eight MiB.
CBM's native file I/O permits sixteen MiB, which is outside this syntax-only change.
`JL_MAX_KEY_BYTES` limits native caller arguments, not every key written in the document.
Don't impose that limit on unrelated document keys.

## Source map

The cached sources were checked on 2026-11-14 against the v0.12.1 Git tree `a7158ac2f48ca646b4527341b76f930f2aeff87e`.
Each cached file's Git blob matches the tagged tree.
The essential functions are:

| Native source                                                                                                                                                                                                                         | Necessary C# responsibility                                                                                                                                          |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [`config_json_like.c:138-250`](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/config_json_like.c#L138-L250), `jl_line_terminator_width`, `jl_whitespace_width`, hex and identifier predicates, `jl_skip_trivia` | Keep the exact whitespace set, comment termination and byte predicates. Drop the unused strict-JSON mode.                                                            |
| [`config_json_like.c:253-488`](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/config_json_like.c#L253-L488), `jl_parse_string`, `jl_parse_identifier`, `jl_parse_key`, `jl_has_prefix`, `jl_parse_number`       | Validate lexical forms and keep token offsets. Numbers only advance the cursor.                                                                                      |
| [`config_json_like.c:493-631`](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/config_json_like.c#L493-L631), `jl_parse_object`, `jl_parse_array`, `jl_parse_value`, `jl_validate_document`                      | Validate nesting, separators, trailing commas, root kind and full input. Reuse the same value parser when skipping members during lookup.                            |
| [`config_json_like.c:659-867`](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/config_json_like.c#L659-L867), `jl_encode_utf8`, `jl_decode_hex4`, `jl_match_output`, `jl_key_equals`, `jl_scan_object`           | Match the four fixed names without allocating a DOM. Keep match counts and the first value span. Omit indentation, comma-edit and insertion fields.                  |
| [`config_json_like.c:1793-1850`](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/config_json_like.c#L1793-L1850), `jl_decode_utf8`, `jl_validate_utf8`                                                           | Reject invalid UTF-8, overlong sequences, surrogate encodings and values above U+10FFFF. Use the BCL's strict UTF-8 decoder for this validation.                     |
| [`config_json_like.c:2724-2842`](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/config_json_like.c#L2724-L2842), `jl_append_decoded_codepoint`, `jl_decode_string_value`                                        | Decode only the selected string. Keep native NUL and surrogate rejection and line-continuation behavior. Use managed storage in place of the native growable buffer. |
| [`config_json_like.c:3055-3118`](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/config_json_like.c#L3055-L3118), `cbm_json_like_get_string_at_path`                                                             | Keep missing, invalid and unique-string outcomes. Hard-code the selected path within the private reader. Existing Root code handles file access.                     |
| [`cli.c:2893-2950`](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/src/cli/cli.c#L2893-L2950), `cbm_openclaw_workspace_path`                                                                                            | Keep configured-workspace precedence and the root include guard. Leave environment, home and profile expansion in `ToolOpenClawRoots`.                               |

The checked source hashes are:

| File                         | SHA-256                                                            |
| ---------------------------- | ------------------------------------------------------------------ |
| `src/cli/config_json_like.c` | `12f2f8c7e80a28a15b6ad3d5e62a8273fcd042904596dabe72748618ad3d2903` |
| `src/cli/config_json_like.h` | `556b5e494b31f2fca01436da163c0e1f9cdb343d01369a9845290858fbe06e65` |
| `src/cli/cli.c`              | `4d5cad983f2af5d9b9fd23c1ff8f5827cef830b84e1a0119a11cbf25a80c53aa` |
| `LICENSE`                    | `1f58f9911dc5e3bcb96de28bb28e7b6bb7eb323952d29569c5d7214a152146bb` |

The port omits CBM's file snapshots, native file handles, edits, writes, schema matching, array mutation and test hooks.
Those functions aren't needed to answer this one read-only question.

## Syntax

The supported lexical families come from the functions above:

| Family            | Native behavior to keep                                                                                                                                                                                                                                                                                           |
| ----------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Whitespace        | Space, tab, CR, LF, form feed, vertical tab, U+00A0, U+2028, U+2029 and U+FEFF. Other Unicode space characters don't become whitespace.                                                                                                                                                                           |
| Comments          | `//` ends at CR, LF, U+2028, U+2029 or EOF. `/* ... */` must close. Comments can occur wherever trivia is allowed.                                                                                                                                                                                                |
| Keys              | Single-quoted strings, double-quoted strings and native unquoted identifiers. ASCII starts are letters, `_` and `$`. Digits can continue a name. Validated UTF-8 bytes at least `0x80` use the native byte predicate. Valid `\uXXXX` identifier escapes follow the native first-character and continuation rules. |
| Strings           | Matching quotes, JSON escapes, `\v`, `\0`, `\xHH`, `\uXXXX`, identity escapes and backslash line continuations. Reject raw controls, raw U+2028 or U+2029, escaped digits `1` through `9` and `\0` followed by a digit.                                                                                           |
| Numbers           | Decimal integers, fractions, exponents, leading or trailing decimal points, optional signs, hexadecimal values, `Infinity` and `NaN` with native optional signs. Validate syntax only. Reject missing digits and invalid adjacent tokens.                                                                         |
| Values            | Objects, arrays, strings, numbers, `true`, `false` and `null`. Object and array separators are required. A trailing comma is allowed. The root must be an object.                                                                                                                                                 |
| Selected decoding | Convert native escapes only for the selected workspace. Combine a valid surrogate pair. Reject decoded NUL and lone surrogates. Lexical validation alone doesn't decode unrelated string escapes.                                                                                                                 |

Keep byte-oriented native key comparison separate from selected string decoding.
For example, the native value decoder removes an escaped Unicode line separator, while `jl_key_equals` has a different branch for that key spelling.
Don't broaden that comparison with .NET identifier categories, string normalization or a shared all-purpose escape decoder.
Use fixed byte predicates, spans and managed buffers without unsafe code or reflection.

## Fixtures

F adds small authored fixtures tied to the source map.
These cases check native compatibility, including the two rejected package behaviors.
They aren't an exhaustive JSON5 conformance suite.
Expected outcomes are configured text, missing (`null`) or invalid (`IOException`).

| Fixture family       | Cases and expected result                                                                                                                                                                                                                                                                                                          |
| -------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Configured path      | Quoted and unquoted selected names with single-quoted and double-quoted workspace strings give the same decoded path.                                                                                                                                                                                                              |
| Missing input        | Missing file and zero-byte file permit the environment and default paths. Whitespace-only and comment-only files are invalid.                                                                                                                                                                                                      |
| Whole document       | Valid workspace followed by garbage, a second root or an unterminated trailing comment is invalid. Trailing permitted trivia is valid. Array, scalar and null roots are invalid.                                                                                                                                                   |
| Separators           | Missing, leading and repeated commas, missing colons and array elisions are invalid. One trailing comma in an object or array is valid.                                                                                                                                                                                            |
| Trivia               | Each native whitespace family and both comment forms are valid at permitted positions. A non-native Unicode space between a colon and value is invalid.                                                                                                                                                                            |
| Key spellings        | Case-sensitive names, escaped ASCII names and quoted identity escapes use native matching. A differently cased key is missing. Add the escaped Unicode line-separator key case to keep native key comparison distinct from value decoding.                                                                                         |
| Selected duplicates  | Duplicate `agents`, `defaults` or `workspace` is invalid, including two spellings that native key comparison treats as equal.                                                                                                                                                                                                      |
| Unrelated duplicates | Duplicate names outside the selected path stay valid. A later malformed unrelated value still makes the whole document invalid.                                                                                                                                                                                                    |
| Selected shapes      | Non-object `agents` or `defaults`, and each non-string workspace kind, are invalid. Missing selected names permit the root include check.                                                                                                                                                                                          |
| Include guard        | Without a workspace, each present root include kind and duplicate include prevents fallback. A nested include doesn't. A valid workspace wins over well-formed root include values and duplicates.                                                                                                                                 |
| String decoding      | Hex and Unicode escapes, surrogate pairs, identity escapes, `\v` and LF, CRLF and Unicode line continuations produce native decoded text. Keep the byte behavior of a non-ASCII identity escape. If its decoded output isn't valid UTF-8, the managed path-string limit makes it unavailable. The caller still applies path rules. |
| Decode rejection     | Selected NUL escapes and lone surrogates are invalid. The same lexically valid escapes in unrelated strings are skipped. Invalid hex escapes, raw controls and raw line separators are invalid anywhere.                                                                                                                           |
| Numeric skip         | Unrelated `1e9999`, long decimal and hex tokens, signed special numbers and leading or trailing decimal points stay valid without conversion. Incomplete exponents, empty hex tokens, leading-zero adjacent digits and bare signs are invalid.                                                                                     |
| Nesting              | A container at native depth 63 is valid. A container at depth 64 is invalid. A scalar at depth 64 is valid. The root is depth zero.                                                                                                                                                                                                |
| UTF-8                | Valid non-ASCII text stays valid. Bad continuation bytes, truncated sequences, overlong encodings and encoded surrogates are invalid, including in comments and unrelated values.                                                                                                                                                  |
| Caller effects       | Configured path wins over the environment. Invalid or empty configured path blocks fallback. Empty config uses the environment, then the profile default. The include file is never opened. Input bytes stay unchanged.                                                                                                            |

Pure cases exercise the private reader through the existing Integration project's Root reference.
Owned temporary files exercise the caller and the current eight MiB read bound.
Use production constants for semantic keys and limits, and one source-mapped external-format fixture set for native spellings.
Assert states, paths and unchanged bytes.
Use Imprint only if rendered output changes.
F reports actual selected case counts after implementation.
Root's later managed and NativeAOT integration gates cover the new parser exception.
This report supplies no executable or native journey evidence.

## Footprint

F's proposed source changes stay below `src/cli/root/OpenForge.Cli/Composition/Shared/ToolHomes/`.
The private support cluster and tests use matching namespaces and paths.
The exact planned files are:

| Responsible slice | Path                                                                                                                                            | Change                                                                                                                                                                                                |
| ----------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| F                 | `src/cli/root/OpenForge.Cli/Composition/Shared/ToolHomes/Shared/OpenClaw/ToolOpenClawConfigurationReader.cs`                                    | New internal entry, fixed selected path, missing and include flow, match counts and spans                                                                                                             |
| F                 | `src/cli/root/OpenForge.Cli/Composition/Shared/ToolHomes/Shared/OpenClaw/ToolOpenClawJsonSyntax.cs`                                             | New private cursor and native lexical and recursive validation, with numeric skip                                                                                                                     |
| F                 | `src/cli/root/OpenForge.Cli/Composition/Shared/ToolHomes/Shared/OpenClaw/ToolOpenClawJsonStrings.cs`                                            | New native key matching and selected string decoding                                                                                                                                                  |
| F                 | `src/cli/root/OpenForge.Cli/Composition/Shared/ToolHomes/ToolOpenClawRoots.cs`                                                                  | Replace the BCL JSON parse with the reader call. Remove the replaced `Member` helper. Move its four JSON key constants to the reader instead of duplicating them. Keep home and environment behavior. |
| F                 | `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Composition/Shared/ToolHomes/Shared/OpenClaw/ToolOpenClawConfigurationReaderTests.cs` | New focused data cases from the matrix                                                                                                                                                                |
| F                 | `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Composition/Shared/ToolHomes/ToolClientConfigurationTests.cs`                         | Add owned-file F-H1 and F-H2 regression cases and precedence checks                                                                                                                                   |
| Root              | `LICENSE`                                                                                                                                       | Append the native notice described below while keeping the current project licence text                                                                                                               |

This is three new production files, one changed caller, one new test class, one changed test class and one Root notice change.
No project reference, package version, shared model or shared callable ABI changes.
Normal SDK source globs include the new files.
Root records the architecture exception and transfers this closed slice to F through its own increment.
E edits none of those files.

The native [MIT licence](https://github.com/DeusData/codebase-memory-mcp/blob/v0.12.1/LICENSE) says `Copyright (c) 2025 DeusData`.
Keep that copyright, the complete permission and condition text and the complete warranty and liability disclaimer.
Each derived C# source header identifies CBM v0.12.1, its original source path and the retained MIT notice in the root licence.
Append the complete upstream licence verbatim under a Codebase Memory attribution in the existing root `LICENSE`.
Keep TheLithiumForge's existing notice separately.
The cached licence's Git blob is `0b95352746a97a8cf2d1f3ba568bc7bb5e514d1f`.

Existing [portable packaging](../../../../../../scripts/delivery/pack.ts) and [npm staging](../../../../../../scripts/delivery/npm/stage.ts) already copy the root `LICENSE`.
This keeps the notice in shipped binary distributions without another runtime resource or packaging mechanism.
Root checks that existing delivery behavior when the port is integrated.
The port uses none of the rejected libraries' code.

## Checks

This research pass starts at E commit `240c78f3449147fb9993f3fecc8c45a879583241`.
Only this new report and E's packet Progress and Evidence change.
Prettier passes for both changed Markdown files.
Production References reports six complete local links across the two files and twelve external links as unchecked.
LF, final newlines, trailing whitespace, `git diff --check` and the exact two-file edit limit pass.
The packet comparison against HEAD confirms changes stay within Progress and Evidence.
The first research return ran no build, .NET checks, tests, native execution, account action or commit.
The coordinator then accepted the plan and authorized a separate local two-document design evidence commit.
Its continuation keeps the exact earlier managed build receipt because product source and dependencies are unchanged.
Both required .NET checks pass with exit zero before the authorized local commit.
The kept build receipt is `artifacts/task90-lane-e-review-docs/final-receipt.json`.
It records zero warnings and errors in `00:04:39.43`.
Fresh check artifacts are under `artifacts/task90-lane-e-native-reader-design/`.
No tests or native tools run in the commit continuation.
The coordinator's later Index pass registers this new report.

## Not decided yet

- Maintainer question for Root's dated record: keep this CBM v0.12.1 reader exception until an ordinary package can keep the required native outcomes? Proposed reversible choice: yes, limited to this private read-only OpenClaw path.

The [lane E packet](../packets/lane-e.md) records the current return and the later catalogue, documentation and Windows journey work.
