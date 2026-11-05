# ADR 040: Keep the delivered game compact

This record explains the compact-artifact aim and its architectural tradeoffs.

## Context

The one-file distribution should be easy to retain and pass along. Generated artwork, a system font and a narrow runtime help keep that artifact manageable. The team wanted compact delivery, with no maximum HTML byte count specified.

The size rules that are explicit concern authored TypeScript files: an aim of 100 lines and a ceiling of 200 lines per file. Those limits are about reviewable behavior modules, not compressed distribution size. Conflating the two would make a smaller source file look like proof of a smaller bundle when it might simply add another import.

## What actually affects the artifact

React and React DOM are bundled runtime dependencies. Vite and its plugins transform the source during the build. The face renderer describes shapes with a small set of SVG primitives rather than embedding 128 raster images. The system font avoids a downloaded or inlined font asset. There is no audio collection to carry with the file.

A single inline file also has no chunk-preload purpose. The main configuration disables the helper and the output test checks its absence. That removes irrelevant runtime machinery, though the reason is the offline contract rather than a claim about a particular number of saved bytes. [ADR 003](adr-003-single-file.md) explains delivery.

## Source structure is a separate optimization

Small modules make changes easier to review and focused tests easier to locate. They do not require collapsing every abstraction to reduce import statements. The build combines source modules into the final artifact, so a clear behavior boundary can remain in source without becoming another file the player must copy.

The current source audit measures authored TypeScript line counts and checks several source conventions. It does not report a universal bundle-size threshold. A developer reading this record can distinguish an enforced source constraint from a qualitative artifact aim. [Source audit notes](source-audit-notes.md) and [ADR 057](adr-057-small-components.md) explain that evidence.

## Alternatives

Removing React could reduce runtime code, but would exchange framework bytes for custom rendering and lifecycle work. Replacing generated SVG with raster sprites might compress differently, but would create asset maintenance and minimum-size review costs. Splitting the output into cached files could benefit a repeatedly visited hosted site while weakening the copy-one-file distribution path.

The project accepted the runtime framework in exchange for clear state-driven rendering. It rejected unnecessary remote assets and diagnostic code in the shipped entry. These are explicit architectural trades, not an assertion that the current bundle is the smallest possible implementation of a memory game.

## Measurement and interpretation

A meaningful artifact comparison uses the same build tools and compares the produced `dist/index.html`. Source character count, compressed transfer size and disk size answer different questions.

Diagnostic face-sheet and session outputs are separate builds. Their size should not be confused with the game artifact, just as test-result screenshots should not be counted as shipped resources. [ADR 051](adr-051-test-artifacts.md) explains that separation.

The practical aim is to keep additions proportionate to a bounded offline game while preserving readable code and correct input. [Dependency policy](dependency-policy.md), [Build pipeline](build-pipeline.md) and [ADR 029](adr-029-pinned-versions.md) provide the neighboring context. The existing requirements and tests define what must pass today.

## The authored-source size check

The implemented size check counts lines in authored TypeScript. It should not be reported as a measurement of the final HTML bundle.

```ts
test('authored TypeScript stays small, uses static ESM and rejects unsafe type escapes', async () => {
  const files = (await Promise.all(['src', 'scripts', 'tests', 'art'].map(sourceFiles))).flat();
  for (const file of files.filter((file) => /\.tsx?$/.test(file.path))) {
    assert.ok(file.text.trimEnd().split(/\r?\n/).length <= 200, file.path);
    assert.doesNotMatch(file.text, /@ts-(?:ignore|nocheck|expect-error)/);
    assert.doesNotMatch(
      file.text,
      /\bany\b|\bas\s+(?:[A-Z{]|unknown\b|number\b|string\b)|\bimport\s*\(|\brequire\s*\(/,
    );
  }
});
```
