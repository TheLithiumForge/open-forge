# ADR 052: Use release tags as historical checkpoints

This record explains the release-tag rationale and separates product, package and data-format versions.

## Context of the decision

A game can have several meaningful versions at once. The specification identifies itself as version 2.2, the package manifest currently says 1.0.0, and board generation, snapshot and replay retain their first format versions. Treating any one of those numbers as a substitute for the others would make a report ambiguous.

The release discussion favored tags as named source checkpoints: a way to associate a delivered game and its evidence with the source that produced them.

## What a checkpoint answers

A release checkpoint helps answer “which implementation was built?” It does not answer “which board did the player receive?” A board code supplies the latter. A snapshot adds progress on that deal, and a replay adds actions. These identifiers work together because they refer to different layers.

A source tag also differs from a package version. The package is private and its scripts build the game locally. Nothing in the current manifest establishes a public registry publishing process. Describing a tag as a checkpoint avoids implying that changing the package number is itself a deployment or that every game copy is fetched from a registry.

## Why format versions remain separate

Generation version `np1` fixes the deck algorithm. Snapshot and replay objects carry `version: 1`. A release that corrects focus scrolling or copy can preserve those data meanings. Incrementing a data format simply to match a release label would make old reproducible examples harder to use without any changed representation to justify it.

Conversely, a changed generation algorithm cannot hide behind a release tag while continuing to give the same board-code prefix a different meaning. The format contract is what lets independent tools reconstruct a deal. [Format versioning guide](format-versioning-guide.md) and [ADR 009](adr-009-board-codes.md) explain that stability.

## Alternatives considered

Relying only on the latest branch would make older bug reports hard to associate with the source used at the time. Naming a downloaded HTML file manually could help a person but would not identify a repository checkpoint. Embedding a release service in the browser would be a much larger feature and would conflict with the independent offline artifact.

The tag rationale was therefore modest: make history pointable without turning version checking into runtime behavior. There is no update polling or in-game release selector in the current implementation. A local copy remains a local copy until someone replaces it.

## Evidence at the checkpoint

The existing commands provide locked installation, type checking, focused tests, a production build and browser journeys. The browser wrapper records actual runtime versions. These are the relevant materials for assessing a built revision; a tag by itself cannot prove that any of them ran.

The README records successful checks on the named combination. [Release checklist](release-checklist.md), [ADR 029](adr-029-pinned-versions.md) and [ADR 055](adr-055-review-before-merge.md) explain the neighboring review concerns.

A useful release history preserves which source was intended, which behavior was specified and which evidence actually exists. Tags help with the first of those. Keeping that role narrow makes them useful even when the game is delivered as a file rather than through a continuously deployed service.

## The package identity is a separate datum

The private package version identifies the source package declaration. It is not the board-generation prefix or a claim about repository tags.

```json
{
  "name": "neon-pairs-reference",
  "version": "1.0.0",
  "private": true,
  "type": "module",
  "engines": {
    "node": ">=24.19.0"
  }
}
```
