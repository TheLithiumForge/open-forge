# Loader Additions

A bulk-reading CLI tool is available for this workspace's routing tree: run `node tools/forge-dump.mjs` from the workspace root (use `bun tools/forge-dump.mjs` if `node` is unavailable). It can list all declared entries, list all tags in use, fetch every file carrying a given tag in one call, and dump a file plus everything its `Entries` route to. Run it with `--help` for the commands. Prefer it over many individual file reads when a task needs a whole cluster of related routes at once.
