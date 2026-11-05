# Source audit notes

This page records what the Neon Pairs source-inspection tests check, how they discover files and where their evidence stops.

The audits run as Vitest tests under `scripts`. They inspect authored text and configuration rather than executing the application. Their value is fast, concrete feedback about a deliberately small codebase. Their regular expressions are visible and limited; a passing audit is evidence about those patterns, not a complete semantic proof of the architecture.

## File discovery

`scripts/source-files.ts` exports `sourceFiles(folder)`. It recursively calls `readdir` with directory entries, descends into directories and reads ordinary files as UTF-8. A link or special entry causes `Source contains a link or special file.` rather than being followed or ignored.

The helper returns path/text pairs. It does not apply `.gitignore`, infer TypeScript projects or skip directories based on a general exclusion list. Each test chooses a starting folder and filters the returned paths. This makes the scan boundary part of the test source.

The broad authored-TypeScript checks scan `src`, `scripts`, `tests` and `art`, then retain paths matching `.ts` or `.tsx`. The shipped-source check scans `src` and omits files whose names contain `.test.` or `.fixture.`. CSS and HTML can participate in that latter source-text scan even though the TypeScript-only checks exclude them.

## Packages and compiler projects

The first test in `source-audit.test.ts` parses `package.json` as unknown data and reads `scripts/package.fixture.json` as the reviewed expected versions. It narrows object shapes before using their fields. It asserts that the runtime dependency names are exactly React and React DOM, then compares expected package versions against the combined manifest maps.

It reads `package-lock.json`, finds the root package entry and compares that entry's dependency and development-dependency maps against the manifest. This catches disagreement at the root. It does not independently audit every transitive package or establish what code an installed package executes.

The same test checks `type: module`, the five explicitly enumerated strictness settings in `tsconfig.base.json`, the existence of the app/Node/end-to-end config files, the exact typecheck script and the `vitest run` unit command. It does not compare every compiler option against a fixture. [TypeScript settings](typescript-settings.md) records the full current configuration.

## File length and type escapes

The authored-TypeScript test removes trailing whitespace at the end of the text, splits on line endings and checks for at most 200 lines. The technology specification's 100-line aim is not the threshold in this assertion. A file at 180 lines can satisfy the test while still deserving a behavior-based split during review.

The test rejects diagnostic suppression markers for ignore, nocheck and expected-error forms. A second expression rejects the standalone token `any`, selected `as` assertions, dynamic import and require calls. These are text patterns, so comments and strings can trigger them too.

The assertion pattern catches an `as` followed by an uppercase name, an opening brace or selected primitive/unknown forms. It does not reject every possible TypeScript assertion syntax. In particular, the source uses `as const` for named value objects and literal arrays. Calling this a complete cast prohibition would misdescribe the implemented check.

The static-import patterns search for `import` or `require` followed by an opening parenthesis, allowing whitespace. The compiler remains responsible for actual syntax and symbol resolution. A text scan is useful for the repository convention precisely because its behavior is simple to inspect, but it is not a parser.

## Core-effect scan

The pure-core filename set is `deck.ts`, `game.ts`, `snapshot.ts`, `snapshot-selection.ts` and `replay.ts`. For non-test, non-fixture source with one of those basenames, the audit rejects listed browser/global markers and a Math random call marker.

The matching is by basename extracted from either path separator, not by a fully resolved module identity. A new file with one of those names elsewhere under `src` would also be selected. Conversely, a new differently named core helper is not automatically added to this set just because a pure module imports it.

This is an important coverage limit. The check does not traverse imports and propagate a purity classification. It also does not cover every current data-only helper, such as `saved-format.ts`, through that set. Review still considers the operations and dependencies of changed modules. [Purity guide](purity-guide.md) explains that architectural reasoning.

## Shipped-source scan

For all selected non-test, non-fixture files under `src`, the audit rejects known audio, canvas and network constructs and a pattern for a deck diagnostic property on window. It also rejects the text markers `fixture`, `face-sheet` and `test-results` in shipped source.

The exact constructs include AudioContext, `new Audio`, audio/canvas markup, a fetch call marker and XMLHttpRequest. This is narrower than proving that every conceivable request mechanism or diagnostic exposure is absent. It does not inspect minified output produced by third-party tools.

The generated-output complement is `tests/build-output.e2e.test.ts`, which reads the built HTML and checks the unwanted fetch and preload markers. The browser complement opens the artifact. These checks have separate inputs and failure meanings; [Offline delivery](offline-delivery.md) connects them.

## Export and ownership conventions

`source-conventions.test.ts` builds its own TypeScript file list over the same four authored folders. Its export regex recognizes line-start declarations of exported constants, functions, interfaces, type aliases and classes, with an optional async marker for functions. It does not attempt to enumerate all legal ESM export syntax.

Its named-import regex collects names inside braces, strips an inline type marker and removes local aliases. The test creates a set of imported names, then asserts that every recognized export name is present in that set. For recognized exported constants it also records a single owner by name.

This is a repository-wide name check. It does not prove that a specific import resolves to the specific export being checked. A same-named import can satisfy the set membership, and re-export syntax or a different declaration form can escape recognition. Those limitations explain why direct imports remain useful to a human reader.

The practical intention is legibility: avoid publishing helpers that no actual behavior uses and avoid ambiguous ownership of exported values. The implementation enforces a recognizable subset of that intention, rather than a full reference graph.

## Predicate names and test names

Another expression recognizes exported function signatures with an explicit boolean or type-predicate return. It checks that the name begins with `is` or `has` followed by an uppercase letter. `isValidSize`, `isWon`, `isJsonObject` and `hasExactKeys` fit this style.

Because the pattern is textual, inferred return types and more elaborate signatures can fall outside it. This is a naming check for recognized yes-or-no exports, not a general attempt to infer every function's meaning.

Test names are inspected in files ending `.test.ts`. The pattern extracts recognized `test` titles and rejects requirement-ID forms such as an uppercase letter followed by two digits. Assertions may still carry requirement references; the check is about human-readable titles. Template-literal titles are included in the recognized forms.

## Environment reads

The environment-read test collects `process.env` access using a simple dotted or bracketed identifier pattern. It compares normalized path/name pairs against a fixed list. The expected pairs are:

| File | Name recorded by the expression |
| --- | --- |
| `scripts/replay-cli.ts` | `INIT_CWD` |
| `scripts/replay.test.ts` | `npm_execpath` |
| `tests/browser-test.ts` | `EXECUTABLE_ENV` |
| `tests/browser-test.ts` | `NAMED_COMBINATION_ENV` |
| `tests/browser-test.ts` | `NPM_AGENT_ENV` |
| `tests/browser-test.ts` | `TEMP` and `TMP` |

For bracketed reads it records the variable identifier, not the string value held by that variable. The browser helper's constants correspond to executable selection, named-combination enforcement and npm's user-agent value. The test normalizes Windows separators before comparing the sorted unique set.

The expression does not model destructuring, aliases, computed string expressions or every possible environment access. It is a narrow check of the current authored forms. The fixed comparison makes accidental environment-dependent game behavior conspicuous within those forms.

## Interpreting a failure

A file-length failure names the file. An export failure names the unconsumed symbol or duplicate owner. A predicate failure names the function. The environment test gives a changed set of path/name pairs. These failures are most useful when read alongside the small regex that produced them.

A false positive in a comment is still a real failure of the textual test, but it does not demonstrate that the comment executes an effect. A false negative is also possible when syntax falls outside a pattern. Neither case justifies describing the audit as more powerful than it is.

Compiler checks, focused unit tests and browser journeys remain separate evidence. The source audits can establish that the manifest and root lock maps agree while a browser journey still fails, or that game transitions are correct while an authored file violates a convention. [Code review guide](code-review-guide.md) treats those as distinct findings rather than flattening them into one pass/fail label.
