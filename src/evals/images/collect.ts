import { cp } from "node:fs/promises";
import { basename } from "node:path";

const excluded = new Set(["node_modules", ".git", ".codex", ".claude", ".auth", ".npmrc", ".netrc", ".git-credentials"]);

await cp("/work", "/output", { recursive: true, filter: (source) => !excluded.has(basename(source)) && !basename(source).startsWith(".env") });
