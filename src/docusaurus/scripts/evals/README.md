# Evals build
The Evals plugin is a Docusaurus plugin that validates published records and gives the main site's Evals pages their initial data.

For example, an empty results submodule builds pages with an empty state. A fixture build shows setup comparisons, judge evidence and CLI and reference history without publishing records.

## Requirements

Run the commands below from `src/docusaurus/`. Install dependencies with `npm ci --cache .npm-cache`.

The data store is the `src/evals/results/` submodule. With no submodule, the site uses its checked-in record code and schemas, shows an empty state and reports an unknown data commit. Fetch latest results still validates records with that copy.

## Build

Run `npm run typecheck`, `npm test` and `npm run build`. Then run `npm run serve -- --port 4319` to preview the built site. Open `/open-forge/docs/evals/results` on the preview server.

Run `npm run evals:prepare` to refresh the checked-in `src/components/evals/records/` copy when the submodule is present. Edit the shared source in the data store, refresh the copy, then include both changes in review. Site tests fail if a present data store differs from the copy. That check skips when the data store is absent. Normal start, build, typecheck and test commands use the checked-in files.

The plugin reads and validates the catalogue and every record at build time. It records the data store's Git commit and marks unpublished changes. The browser's Fetch latest results uses the same validator before replacing the initial copy.

## Fixture preview

Run `npm run evals:fixture` to write four authored batches into the ignored `.evals-fixture/` folder. They cover two implementers, two judges, four setups, two CLI versions and the reference trend. Some token counts are missing. Judge evidence includes HTML-looking text to check that it displays as text.

From the repository root, run `npm run evals -- site --repository src/docusaurus/.evals-fixture`. It starts the docs site and opens the Results page reading the fixture through its local data server. The schemas and validation code come from the checked-in record copy. Keep `.evals-fixture/` out of published records. Stop the site with Ctrl+C when you finish.

To see local runs, including batches still running, run `npm run evals -- site` instead.
