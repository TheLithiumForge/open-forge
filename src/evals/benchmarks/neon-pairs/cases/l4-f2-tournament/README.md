# Tournament mode
F2 is a large feature case that tests whether documentation helps at a size no agent can read in one go.

The project's code stays as it is. The case brings its own document pack of about 300,000 tokens, with 30 project rules planted one per document. Most of them never mention tournaments, so searching for the feature finds the competition pages and misses the rest.

Status: part of [Evals at Documentation Scale](../../../../../../.agents/memory/emerging/analysis/eval-docs-at-scale.md), Stage 1.

## Case

The request gives every scenario the same labels, so Code only can build the feature. `seed.patch` removes what distracted agents in the first full run: the README's browser download, formatting failures in untouched files and a screenshot test that timed out under load.

## Documents

`docs-pack/` holds the documents, `docs-pack.metadata.json` what each is about, and `docs-scopes.json` which folder each belongs in. `rules.json` lists each planted rule and superseded decision. To change a document, rewrite the manifest afterwards:

```powershell
node src/evals/materialize/document-pack.ts <case>/docs-pack <case>/docs-scopes.json neon-pairs
```

## Checks

There are 42 checks in two groups, reported apart. Core checks (`core-01` to `core-12`) need only the request. Rule checks, named by their rule's ID, need the docs, and their coverage is the test's main measure.

The driver in `checks/tournament.ts` plays through only the labels the request names, so one rule's check doesn't hang on another rule. Run `node checks/worker.ts INPUT_JSON` through the [worker contract](../../../../grading/brownfield/README.md). The K checks read source as data.

## Validation

`case-contract.test.ts` holds the pack to its design: size, one home for each rule's values, no competition words outside the competition pages and no rule value in the starting code. Each solution must also pass `npm run evals -- validate-case --case l4-f2-tournament`.
