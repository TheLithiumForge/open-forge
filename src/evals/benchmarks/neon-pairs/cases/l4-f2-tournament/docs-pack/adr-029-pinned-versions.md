# ADR 029: Pin the reviewed tool combination

This record explains exact dependency versions, the lockfile and the difference between supported and recorded runtimes.

## Decision

The manifest uses exact package versions and retains `package-lock.json`. This makes the reviewed tool set identifiable and allows `npm ci` to install the locked dependency tree. A successful run can then refer to a specific combination rather than an open-ended interpretation of a version range.

The current runtime dependencies are React and React DOM 19.3.1. Development tools include TypeScript 7.0.2, Vite 8.3.5, its React plugin 6.1.2, the single-file plugin 2.3.4, Vitest 5.0.5 and Playwright 1.64.1. Prettier and image-test tooling are also pinned in the manifest. The technology specification records the reviewed stack; this decision explains the reason for exactness.

## Why exact versions help this project

A bundler change can introduce runtime preload code. A browser update can change focus scrolling. A type checker can change which narrowing is accepted. These are not hypothetical categories unrelated to the game: the suite inspects final HTML, visible focus bounds and strict compiler projects. When one of those observations changes, knowing the exact tool set makes investigation narrower.

The lockfile covers transitive resolution as well as direct names. Pinning only the manifest would still leave a fresh installation open to changes beneath those direct packages. `npm ci` is the locked-install path, while the source audit compares the root lockfile package declarations with the manifest.

## Runtime records

The supported Node engine is `>=24.19.0`. The named tested combination is more specific: Node 26.12.1, npm 11.20.0, Playwright 1.64.1 and Chromium 156.0.8078.4. The README records successful verification on that named set. That is evidence for the named set, not a statement that every supported Node version has already been exercised.

`browser-test.ts` writes `test-results/browser-runtime.json` with the actual versions used. Setting `NEON_PAIRS_NAMED_COMBINATION` to `1` makes the wrapper compare them against the named values. Other supported runtimes can run without that assertion and retain their own record. [Browser support](browser-support.md) explains the resulting evidence boundary.

## Alternatives and tradeoffs

Version ranges make routine updates easy to obtain, but they can change a fresh environment without a source change. Floating browser downloads would have a similar effect on layout and accessibility evidence. The project preferred explicit updates that can be assessed with the affected checks.

Pins do not make dependencies timeless or prove their correctness. They identify what was reviewed. An update still needs current evidence, and a lockfile can faithfully reproduce a flawed combination. The benefit is traceability and controlled comparison rather than a guarantee of quality from the version string alone.

## Scope of this record

Package and runtime checks cover the manifest, lockfile, engine range and named compatibility combination. Hosting, automated dependency updates and release pipelines are separate from those checks.

[ADR 005](adr-005-vite.md), [ADR 018](adr-018-playwright.md), [ADR 020](adr-020-vitest.md) and [Build pipeline](build-pipeline.md) explain how the tools are used. The exact versions make those accounts reproducible enough to investigate, while behavioral tests establish what the resulting game does.

## The declared package set

The manifest expresses concrete reviewed versions. The lockfile then records their resolved dependency tree.

```json
{
  "dependencies": {
    "react": "19.3.1",
    "react-dom": "19.3.1"
  },
  "devDependencies": {
    "@playwright/test": "1.64.1",
    "@types/node": "24.19.2",
    "@types/react": "19.3.0",
    "@types/react-dom": "19.3.0",
    "@vitejs/plugin-react": "6.1.2",
    "typescript": "7.0.2",
    "vite": "8.3.5",
    "vite-plugin-singlefile": "2.3.4",
    "vitest": "5.0.5",
    "prettier": "3.9.6",
    "pngjs": "7.0.0",
    "@types/pngjs": "6.0.5"
  }
}
```
