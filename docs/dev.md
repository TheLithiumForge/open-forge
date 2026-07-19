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
The framework is designed to be a bit more work from the start, because it does not ship complete defaults. It wants maximum flexibility and organic growth. It is meant to be used for one project, many projects together, a monorepo, a private vault, or whatever shape the user's work has. It acknowledges that one person's use cases can differ wildly from another's, so it does not optimize for a middle ground that quietly harms everyone. It optimizes for customizability, providing only a handful of rules and a scalable architecture that the user can artisanally craft to their own needs and wishes.
```

## Layout

```text
src/open-forge/   # installable template payload
src/extensions/   # optional first-party extension packages
src/cli/cli.ts    # CLI source
benchmarks/harness/runner.ts # developer-only benchmark evidence runner
build.ts          # one build script
dist/             # generated release output
```

The shipped CLI runs on Node.js. Bun is the repository-development runtime for builds, tests, source indexing, and the developer benchmark harness; it is not required by users of the distributed CLI.

Framework governance lives in `docs/framework/`: `concepts/` owns cross-cutting behavior (routing, formatting, layers, primitives, extensions, overwrites, payload boundary) and `payload/` mirrors the installable files with one descriptor per installed file. When a descriptor changes required behavior, the matching installed file changes in the same work.

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

## CLI Test Pattern

Command-contract tests use a fresh OS temporary directory per case and invoke the public CLI as a real child process with argument arrays. Use real Git repositories for checkpoint behavior. Assert exit code, stdout, stderr, filesystem bytes, generated routes, and Git status as applicable; every rejected mutation must also prove that no partial output appeared. Clean temporary roots in `afterEach` or `finally`.

Keep direct helper tests for pure algorithms and state transitions, but do not use them as substitutes when parsing, executable lookup, packaging, process output, filesystem effects, Git scope, or rollback is part of the contract. Packaged-layout smoke tests must execute the built Node CLI from both supported distribution layouts. The installable version of this convention lives in the optional `cli-testing-patterns` extension.

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
