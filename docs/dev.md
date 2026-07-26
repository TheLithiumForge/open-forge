# Open Forge Development

This file is for maintainers. The user-facing README should stay about what Open Forge is and how to use it.

## Documentation Voice

The README should sound like Open Forge itself:

- direct and engineering-minded
- honest about tradeoffs
- allergic to fake complete defaults
- warm enough to feel human, but not trying too hard
- lightly funny only when it clarifies the point

The core tone to preserve:

```text
Define Adaptive Context Engineering, then state the accepted Open Forge vision without replacing it with a weaker simplified tagline. Explain the real context and continuity problem, provide the fastest safe useful start, and reveal advanced routing, Memory, customization, Extensions, and tooling progressively. Open Forge starts with a small useful foundation and sensible removable defaults, then grows through real decisions and recurring needs. Keep the exact structural scaling claim near any playful language about infinite scaling or growing your own framework. Present Markdown as the complete semantic contract and the CLI as a deterministic reasoning accelerator. State current maturity and usage honestly.
```

## Layout

```text
src/open-forge/   # installable template payload
src/extensions/   # optional first-party extension packages
src/cli/cli.ts    # CLI source
benchmarks/harness/runner.ts # developer-only benchmark composition and run capture
build.ts          # one build script
dist/             # generated release output
```

The shipped CLI runs on Node.js. Bun is the repository-development runtime for builds, tests, source indexing, and the developer benchmark helper; it is not required by users of the distributed CLI.

Current maintainer contracts for reviewed source live in `.agents/memory/crystallized/documents/maintenance/` and link to their source, relationships, and verification. During the file-by-file migration, remaining `docs/framework/` descriptors continue to govern only the source files they describe. Contract changes update the source, dogfood counterpart, maintenance document, and behavior tests together when they share that contract.

## Commands

Build all release output:

```sh
bun run build
```

Regenerate framework indexes:

```sh
bun run index
```

After build, run the CLI with Node:

```sh
node ./dist/cli.mjs install .
node ./dist/cli.mjs extend --list
node ./dist/cli.mjs index .
```

Bun can run the TypeScript source during local development:

```sh
bun run src/cli/cli.ts install .
bun run src/cli/cli.ts index .
```

These mutating examples expect a clean Git checkpoint. During CLI development, add `--pro` only when intentionally exercising the expert bypass; lifecycle tests should use fresh real repositories. That is local convenience only. The distributed CLI is Node.

## Tests

The default development suite is fast and storage-light:

```sh
bun run test
bun run test:fast
```

Files named `*.unit.test.ts` exercise pure algorithms and state transitions without OS temporary directories, Git repositories, builds, or child CLI processes. Run them freely while developing.

Files named `*.closure.test.ts` cross a real process, filesystem, Git, packaging, or benchmark-run boundary. They are intentionally explicit because they are slower and write substantially more temporary data:

```sh
bun run test:closure
bun run test:ci
```

CI runs both tiers. Run closure tests locally before closing work that changes the command boundary, installation effects, rollback, containment, Git behavior, packaged layouts, or benchmark preparation and capture. Both tiers remain normal Bun test files, so an IDE test extension can run one file or case directly.

Use the package scripts for tier selection. A raw `bun test` intentionally follows Bun's normal discovery and runs both `*.unit.test.ts` and `*.closure.test.ts`; it is therefore a full run, not the routine fast command. In an IDE, select unit files for fast feedback and closure files deliberately.

`tests/run-tests.ts` resolves the repository from its own location and passes absolute test paths, so the test tier does not depend on the caller's current directory. All suites use `tests/support/index.ts` for temporary sandboxes, subprocesses, Git fixtures, path checks, tree snapshots, and repository paths. Prefer one suite-scoped OS temporary root with isolated case directories and one cleanup at suite closeout.

Closure tests invoke the public CLI as a real child process with argument arrays and explicit cwd. Use real Git repositories when checkpoint behavior is the contract. Assert behavior, bytes, ownership, generated-route validity, Git state, and failure atomicity as applicable. A successful file-existence assertion alone is not useful coverage; prove that the file has the intended semantics or that a rejected mutation left no partial output. Packaged-layout smoke tests execute the built Node CLI from both supported distribution layouts. The installable version of this convention lives in the optional `cli-testing-patterns` extension.

## Release Output

`bun run build` produces:

```text
dist/cli.mjs
dist/open-forge-src/
dist/extensions/
dist/open-forge-src.tar.gz
dist/open-forge-src.tar.gz.sha256
dist/open-forge-src.manifest.json
```

## npm Package

The npm package contains:

```text
dist/cli.mjs
docs/cli.md
docs/dev.md
docs/extensions.md
src/open-forge/
src/extensions/
README.md
LICENSE
package.json
```

There are no postinstall scripts. The published package contains the already-built Node CLI.

Dry run:

```sh
npm pack --dry-run
```

Publish:

```sh
bun run build
npm publish
```

## GitHub Releases

Attach these files from `dist/`:

```text
open-forge-src.tar.gz
open-forge-src.tar.gz.sha256
open-forge-src.manifest.json
```

Do not attach `cli.mjs` by itself. The executable resolves install payloads from the npm package layout or from adjacent `open-forge-src/` and `extensions/` directories in the complete `dist/` layout; the lone file is not a standalone distribution.

Open Forge should be consumed from released artifacts, not from a moving branch.
